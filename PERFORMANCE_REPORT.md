# パフォーマンス不具合調査レポート

> 調査日: 2026-05-08  
> 担当: Claude Code + Codex CLI  
> 対象: バトル中（ゲームプレイ中）の処理負荷スパイク

---

## 総括

バトル中の重さは「Debug.Log の毎フレーム出力」「FindGameObjectsWithTag の毎フレーム呼び出し」「イベントハンドラの二重登録」の3点が主因。  
加えて、シーン全体を3秒ごとに再ロードする `Loop.cs` がバトルシーンに残っている場合は、それ単体で致命的な負荷になる。

---

## 重大度別一覧

| # | 重大度 | ファイル | 問題 |
|---|--------|----------|------|
| 1 | 🔴 緊急 | `Loop.cs` | 3秒ごとにシーン丸ごと再ロード |
| 2 | 🔴 緊急 | `PlayerStateManager.cs` | `Debug.Log` を毎フレーム・全プレイヤー分実行 |
| 3 | 🟠 深刻 | `BOTController.cs` | `FindGameObjectsWithTag` を毎フレーム実行 |
| 4 | 🟠 深刻 | `BotPlayerController1.cs` | `FindGameObjectsWithTag` を0.5秒ごと + OnTriggerStayでGetComponent+Raycast |
| 5 | 🟠 深刻 | `EfectController.cs` | イベントハンドラを OnEnable と Start の両方で二重登録 |
| 6 | 🟡 中程度 | `PlayerController1.cs` | `OnTriggerStay` 内で Raycast + GetComponent 毎物理ステップ |
| 7 | 🟡 中程度 | `UICameraFollower.cs` | `Camera.main` を毎フレーム2回参照 |
| 8 | 🟡 中程度 | `SurvivalMode.cs` | タイマー文字列を毎フレーム生成 → GC圧迫 |
| 9 | 🟢 軽微 | `BotController.cs` | `Physics.SphereCast` を毎フレーム実行 |
| 10 | 🟢 軽微 | `BoundBall.cs` | 衝突ごとに `Debug.Log` + 文字列補間 |
| 11 | 🟢 軽微 | `ObjectSpawner.cs` | 消去判定の基準点がワールド原点固定 |

---

## 詳細

---

### 🔴 問題1: Loop.cs — 3秒ごとにシーン全体を再ロード

**ファイル:** `Assets/Scripts/Loop.cs:11`

```csharp
void Start()
{
    InvokeRepeating(nameof(LoopScene), loopInterval, loopInterval); // 3秒間隔
}

public void LoopScene()
{
    SceneManager.LoadScene(SceneManager.GetActiveScene().name); // シーン丸ごと破棄・再生成
}
```

`SceneManager.LoadScene` はシーン内のオブジェクトを全て破棄して再構築する。バトル中にこの GameObject が有効になっていれば、3秒ごとに強制的に全リソースが解放・再確保され、確実に「ところどころ超重くなる」を引き起こす。

**対処:** バトルシーンのヒエラルキーから `Loop` コンポーネントが付いたオブジェクトを確認し、不要なら削除またはコンポーネントを Disabled にする。デバッグ用途なら `#if UNITY_EDITOR` で囲む。

---

### 🔴 問題2: PlayerStateManager.cs — Debug.Log を毎フレーム出力

**ファイル:** `Assets/kento/Script/PlayerScript/PlayerStateManager.cs:74-77`

```csharp
private void Update()
{
    Debug.Log($"MoveState: {MoveState}, ActionState: {ActionState}");
}
```

`Debug.Log` は 1 回でも重い処理だが、これが `Update()` 内にある。プレイヤー4人いれば **60fps × 4人 = 毎秒240回** の `Debug.Log` が走る。さらに文字列補間（`$"..."`）は毎回ヒープアロケーションを起こすため、GC（ガベージコレクション）が頻発してフレームレートをスパイク的に落とす。「ところどころ」重くなる典型的なパターン。

**対処:** この `Update()` メソッドを丸ごと削除する。ステート確認が必要なら Unity Profiler または Event ベースのデバッグ表示に切り替える。

---

### 🟠 問題3: BOTController.cs — FindGameObjectsWithTag を毎フレーム実行

**ファイル:** `Assets/kento/Script/PlayerScript/BOT/BOTController.cs:207-226`

```csharp
void Serch()  // Update() から毎フレーム呼ばれる
{
    near = null;
    GameObject[] players = GameObject.FindGameObjectsWithTag("Player"); // ← 毎フレーム全シーン検索
    // ...
}
```

`FindGameObjectsWithTag` はシーン内の全 GameObject を走査し、さらに戻り値として `GameObject[]` を毎回ヒープに確保する。BOT が複数いれば BOT の数だけ重複して実行される。

**対処:** `GameManager_M.GetActivePlayers()` のリストを使うか、ゲーム開始時に一度だけキャッシュする。

---

### 🟠 問題4: BotPlayerController1.cs — 複数のパフォーマンス問題

**ファイル:** `Assets/kento/Scripts/Bot/BotPlayerController1.cs`

**(4-a) FindGameObjectsWithTag を0.5秒ごとに実行（357-375行付近）**

```csharp
// searchInterval = 0.5f ごとに CollectPlayers() を呼び出し
GameObject.FindGameObjectsWithTag("Player"); // 戻り配列を毎回確保
```

BOTController.cs と同様。こちらは `searchInterval` でインターバルを設けているが、BOT が複数いれば並列で走る。

**(4-b) OnTriggerStay 内で Raycast + GetComponent（301-327行付近）**

```csharp
private void OnTriggerStay(Collider other)
{
    // isTackling チェックより前に角度計算・距離計算・Raycast・GetComponent が走る
    Physics.Raycast(...);
    other.gameObject.GetComponent<Reception1>();
}
```

`OnTriggerStay` は接触中の Collider ごとに **毎物理ステップ（FixedUpdate 相当）** 呼ばれる。複数プレイヤーが密集すると急激に重くなる。

**(4-c) Vector3.Distance を sqrMagnitude で代替すべき（384-396行付近）**

```csharp
float dist = Vector3.Distance(a, b); // 内部で平方根計算が走る
```

距離の大小比較だけが目的なら `(a - b).sqrMagnitude < range * range` の方が高速。

---

### 🟠 問題5: EfectController.cs — イベントハンドラを二重登録

**ファイル:** `Assets/kento/Script/PlayerScript/EfectController.cs:13-38`

```csharp
private void OnEnable()
{
    stateManager = GetComponent<PlayerStateManager>();
    stateManager.OnMoveStateChanged += MoveEffect;   // ← ここで登録
    stateManager.OnActionStateChanged += ChargeEffect;
    stateManager.OnAttackPowerChanged += AttackEffect;
    // ...
}

void Start()
{
    stateManager = GetComponent<PlayerStateManager>();
    stateManager.OnMoveStateChanged += MoveEffect;   // ← さらにここでも登録（二重）
    stateManager.OnActionStateChanged += ChargeEffect;
    stateManager.OnAttackPowerChanged += AttackEffect;
    // ...
}
```

MonoBehaviour が有効化されると `OnEnable` → `Start` の順で両方実行される。スコアモードでリスポーンのたびに `OnEnable` が再度呼ばれるため、ラウンドが進むにつれてハンドラが累積登録される。

**結果:** イベント発火のたびに同じ処理が2回（またはそれ以上）走り、パーティクルが二重再生されるなどの副作用も生じる。

**対処:** `OnEnable` / `OnDisable` をペアで使い、`Start` 内の登録を削除する。

```csharp
private void OnEnable()
{
    stateManager = GetComponent<PlayerStateManager>();
    stateManager.OnMoveStateChanged += MoveEffect;
    stateManager.OnActionStateChanged += ChargeEffect;
    stateManager.OnAttackPowerChanged += AttackEffect;
}

private void OnDisable()
{
    if (stateManager == null) return;
    stateManager.OnMoveStateChanged -= MoveEffect;
    stateManager.OnActionStateChanged -= ChargeEffect;
    stateManager.OnAttackPowerChanged -= AttackEffect;
}

// Start() は削除
```

---

### 🟡 問題6: PlayerController1.cs — OnTriggerStay 内で Raycast + GetComponent

**ファイル:** `Assets/kento/Scripts/Player/brink/PlayerController1.cs:286-312`

プレイヤー側でも `OnTriggerStay` 内で `Physics.Raycast` と `GetComponent<Reception1>()` を毎物理ステップ実行している。密集時の負荷スパイクの原因になる。

**対処:** `isTackling` フラグで早期 return、`Reception1` の参照は `Start()` でキャッシュ。

---

### 🟡 問題7: UICameraFollower.cs — Camera.main を毎フレーム2回参照

**ファイル:** `Assets/kento/Scripts/Player/UICameraFollower.cs:10-18`

```csharp
void LateUpdate()
{
    transform.position = Camera.main.transform.position; // ← タグ検索コスト
    transform.rotation = Camera.main.transform.rotation; // ← もう1回
}
```

`Camera.main` は `FindGameObjectsWithTag("MainCamera")` 相当のコストがかかる。プレイヤーUIの数だけ毎フレーム走る。

**対処:** `Start()` で `private Camera _cam; _cam = Camera.main;` としてキャッシュ。

---

### 🟡 問題8: SurvivalMode.cs — タイマー文字列を毎フレーム生成

**ファイル:** `Assets/Miwa/scripts/SurvivalMode.cs:22-33`

```csharp
public void OnUpdate()  // GameManager の Update() から毎フレーム呼ばれる
{
    currentTime -= Time.deltaTime;
    timerText.text = Mathf.Max(0, currentTime).ToString("F1"); // 毎フレーム文字列生成
}
```

`ToString("F1")` は毎フレーム新しい string オブジェクトをヒープに確保する。タイマーが 0.1 秒変化したときだけ更新するだけで GC 圧迫を大幅に軽減できる。

---

### 🟢 問題9: BotController.cs — Physics.SphereCast を毎フレーム実行

**ファイル:** `Assets/Scripts/BotController.cs:41-68`

`Update()` 内で `Physics.SphereCast` を実行。BOT が複数いると並列に走る。単体の影響は小さいが、他の問題と重なると効く。インターバルを挟むか、`OverlapSphere` に切り替えて結果をキャッシュすることを検討。

---

### 🟢 問題10: BoundBall.cs — 衝突ごとに Debug.Log

**ファイル:** `Assets/Scripts/BoundBall.cs:50`

衝突が多発するシーンで文字列補間付き `Debug.Log` が走る。Development Build / Editor 環境では顕著。削除または `#if UNITY_EDITOR` で囲む。

---

### 🟢 問題11: ObjectSpawner.cs — 消去判定の基準点がワールド原点固定

**ファイル:** `Assets/Scripts/ObjectSpawner.cs:97-99`

```csharp
if (Vector3.Distance(Vector3.zero, transform.position) > 50f)
{
    Destroy(gameObject); // 原点から50m以上離れたら削除
}
```

ステージが原点（0,0,0）から離れている場合、この条件が永遠に成立せず生成オブジェクトが蓄積し続ける。基準点をスポーン地点または `Camera.main.transform.position` にすべき。

---

## 修正優先度

```
[即日対応]
  1. Loop.cs がバトルシーンで有効になっているか確認・無効化
  2. PlayerStateManager.Update() の Debug.Log を削除

[今週中]
  3. EfectController.cs のイベントハンドラ二重登録を修正（OnDisable で解除）
  4. BOTController / BotPlayerController1 の FindGameObjectsWithTag をキャッシュに変更

[余裕があれば]
  5. OnTriggerStay 内の GetComponent をキャッシュ化
  6. Camera.main キャッシュ
  7. タイマー文字列の差分更新
```

---

## 修正見積もり（難易度）

| 問題 | 修正難易度 | 効果 |
|------|-----------|------|
| Loop.cs 無効化 | ★☆☆ 低 | 絶大（該当時） |
| Debug.Log 削除 | ★☆☆ 低 | 大 |
| EfectController 修正 | ★★☆ 中 | 中（累積するほど大きくなる） |
| FindGameObjectsWithTag キャッシュ | ★★☆ 中 | 中〜大（BOT数次第） |
| OnTriggerStay 最適化 | ★★☆ 中 | 中（密集時） |
| Camera.main キャッシュ | ★☆☆ 低 | 小〜中 |
| タイマー文字列最適化 | ★☆☆ 低 | 小 |

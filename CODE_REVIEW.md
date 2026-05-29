# コードレビュー報告書

> **対象**: mainブランチ全体（C# 79ファイル）  
> **実施**: マルチエージェント並列レビュー（5エージェント同時）  
> **更新日**: 2026-04-24

---

## 総合スコア

| カテゴリ | スコア | 評価 |
|---------|--------|------|
| プレイヤー・入力・移動 | **45 / 100** | ⚠️ 要改善 |
| ゲーム管理・モード | **48 / 100** | ⚠️ 要改善 |
| ステージ・ギミック | **52 / 100** | ⚠️ 要改善 |
| ボット・AI | **38 / 100** | 🔴 緊急改善 |
| アイテム | **42 / 100** | ⚠️ 要改善 |
| **プロジェクト全体** | **45 / 100** | ⚠️ 要改善 |

### スコア基準
| 範囲 | 評価 |
|------|------|
| 90〜100 | 非常に良い（軽微な改善のみ） |
| 70〜89 | 良い（いくつか改善点あり） |
| 50〜69 | 普通（複数の問題あり） |
| 30〜49 | 要改善（重大な問題が複数） |
| 0〜29 | 緊急改善（クリティカルバグ・構造的欠陥） |

---

## 優先度凡例

| マーク | 意味 | 対応目安 |
|--------|------|---------|
| 🔴 P0 | 即修正（クラッシュ・重大バグ） | 今すぐ |
| 🟠 P1 | 高優先（ゲームへの悪影響） | 次のコミットまでに |
| 🟡 P2 | 中優先（パフォーマンス・設計） | 次のスプリントまでに |
| 🟢 P3 | 低優先（コード品質・可読性） | リファクタリング時に |

---

---

# 1. プレイヤー・入力・移動　45 / 100

対象: `MoveController.cs` / `PlayerStateManager.cs` / `PlayerInputController.cs` / `AtackController.cs` / `ChargeSpike.cs` / `AnimatorController.cs` / `EfectController.cs` / `Reception.cs` / `PlayerController1.cs` / `PowerMeter1.cs` / `Reception1.cs` / `CanvScript.cs` / `UICameraFollower.cs` / `PlayerColorChan.cs` / `RenderColor.cs` / `DecalColor.cs` / `PlayerCursor.cs` / `CursorController.cs` / `knockback.cs` / `move.cs`

---

### 🔴 P0 — PlayerColorChan.cs:17 — Material の取得方法が根本的に誤り

**問題**  
`GetComponent<Material>()` は Unity では常に null を返す。`Material` はコンポーネントではないため、色の変更機能が完全に動作していない。

```csharp
// 現在（バグ）
material = GetComponent<Material>();
```

**修正案**
```csharp
Renderer renderer = GetComponent<Renderer>();
if (renderer != null)
    material = renderer.material;
```

---

### 🔴 P0 — CursorController.cs:25-27 — NullReferenceException が確実に発生

**問題**  
`GameObject.Find("Canvas")` が null を返した場合、次行でクラッシュする。Canvas が存在しないシーンでは必ず落ちる。

```csharp
// 現在（危険）
obj = GameObject.Find("Canvas");
raycaster = obj.GetComponent<GraphicRaycaster>(); // obj が null ならクラッシュ
```

**修正案**
```csharp
obj = GameObject.Find("Canvas");
if (obj == null) { Debug.LogError("Canvas not found"); return; }
raycaster = obj.GetComponent<GraphicRaycaster>();
```

または `[SerializeField] private Canvas canvas;` でインスペクタから直接設定するとより安全。

---

### 🔴 P0 — Reception.cs:90 — 複数回被弾でコルーチンが重複実行

**問題**  
`KnockBack()` が連続で呼ばれると `StartCoroutine(Hit())` が重複し、ノックバック処理が正しく動かない。

```csharp
// 現在（バグ）
StartCoroutine(Hit());
```

**修正案**
```csharp
if (hitCoroutine != null) StopCoroutine(hitCoroutine);
hitCoroutine = StartCoroutine(Hit());
```

---

### 🔴 P0 — Reception1.cs:59-73 — Rigidbody の null チェックなし

**問題**  
`rb` が null の場合 `rb.linearVelocity` で即クラッシュ。

**修正案**
```csharp
public void KnockBack(Vector3 pos, float force)
{
    if (rb == null) return;
    rb.linearVelocity = Vector3.zero;
    // 以降の処理
}
```

---

### 🟠 P1 — PlayerStateManager.cs:74-77 — Update() で毎フレーム Debug.Log を実行

**問題**  
開発中のデバッグログが残ったまま。毎フレーム文字列生成＋ログ出力でパフォーマンスに影響。

```csharp
// 現在（問題あり）
private void Update()
{
    Debug.Log($"MoveState: {MoveState}, ActionState: {ActionState}");
}
```

**修正案**: 削除するか以下でガード。

```csharp
#if UNITY_EDITOR && DEBUG_PLAYER_STATE
    Debug.Log($"MoveState: {MoveState}, ActionState: {ActionState}");
#endif
```

---

### 🟠 P1 — AtackController.cs — エディタ専用 using の混入

**問題**  
ランタイムのスクリプトにエディタ専用の using が含まれている。ビルドエラーの原因になる。

```csharp
using UnityEditor;
using UnityEditor.ShaderGraph.Internal;
using UnityEditorInternal;
```

**修正案**: これら3行を削除する。

---

### 🟠 P1 — Reception.cs と Reception1.cs — ほぼ同一クラスが2つ存在

**問題**  
機能的にほぼ同じクラスが2つあり、どちらが本番稼働中か不明。保守コストが2倍になる。

**修正案**: `Reception.cs` を正として整理し、`Reception1.cs` は削除するか明確に役割を分けて命名し直す。

---

### 🟠 P1 — AtackController.cs:137 / PlayerController1.cs:259 — Invoke() の乱用

**問題**  
文字列ベースの `Invoke()` / `CancelInvoke()` はリファクタリング時にバグを埋め込みやすく型安全性がない。メソッド名を変えてもエラーが出ない。

```csharp
// 現在（問題あり）
Invoke("EndAttack", duration);
CancelInvoke("EndAttack");
```

**修正案**
```csharp
private Coroutine attackCoroutine;

void StartAttack()
{
    if (attackCoroutine != null) StopCoroutine(attackCoroutine);
    attackCoroutine = StartCoroutine(AttackDurationRoutine(duration));
}

IEnumerator AttackDurationRoutine(float duration)
{
    yield return new WaitForSeconds(duration);
    EndAttack();
}
```

---

### 🟡 P2 — UICameraFollower.cs:12-20 — Camera.main を毎フレーム呼び出し

**問題**  
`Camera.main` は内部で `FindGameObjectWithTag("MainCamera")` を実行するため、毎フレーム呼ぶのは非効率。

**修正案**
```csharp
private Camera mainCamera;
private void Start() { mainCamera = Camera.main; }
// Update では mainCamera を使用
```

---

### 🟡 P2 — CursorController.cs:45-56 — Update() で毎フレーム PointerEventData を生成

**問題**  
毎フレーム `new PointerEventData(...)` と `new List<RaycastResult>()` でヒープアロケーションが発生し、GC 負荷が高い。

**修正案**
```csharp
private PointerEventData eventData;
private List<RaycastResult> results = new List<RaycastResult>();

void Awake() { eventData = new PointerEventData(EventSystem.current); }
void Update() { results.Clear(); /* 以降は既存処理 */ }
```

---

### 🟢 P3 — スペルミス一覧

IDE のリファクタリング機能で一括変換できる。Unity ではクラス名変更後に Prefab の参照外れを確認すること。

| ファイル | 現在 | 正しい名前 |
|---------|------|-----------|
| MoveController.cs | `curentSpeed` | `currentSpeed` |
| MoveController.cs | `inputVer` | `inputVector` |
| AtackController.cs（クラス名） | `AtackController` | `AttackController` |
| AtackController.cs | `curentknockbackForce` | `currentKnockbackForce` |
| EfectController.cs（クラス名） | `EfectController` | `EffectController` |
| PlayerColorChan.cs（クラス名） | `PlayerColorChan` | `PlayerColorChange` |
| Reception.cs | `isKonckback` | `isKnockback` |
| DecalColor.cs | `bule`, `yello` | `blue`, `yellow` |
| PlayerInputController.cs | `OnAtatck`, `atack` | `OnAttack`, `attack` |
| PlayerStateManager.cs | `inputVere` | `inputVector` |

---

### 🟢 P3 — 空メソッド・コメントアウトコードの大量残存

**問題のあるファイル**: `knockback.cs` / `PlayerColorChan.cs` / `RenderColor.cs` / `ChargeSpike.cs` / `AtackController.cs` / `Reception.cs` / `PlayerController1.cs`

**修正案**: 空の `Start()` / `Update()` は削除する。コメントアウトコードは git 履歴で追えるため削除してよい。

---

---

# 2. ゲーム管理・モード　48 / 100

対象: `GameManager_M.cs` / `IGameMode.cs` / `SurvivalMode.cs` / `SuddenDeathMode.cs` / `GameOverMode.cs` / `ScoreMode.cs` / `ScoreManager.cs` / `ScoreItem.cs` / `PlayerScoreHandler.cs` / `PauseManager.cs` / `PanelFocusController.cs` / `SceneChanger.cs` / `SoundManager.cs` / `PlayerHealth.cs` / `PlayerStatusUI.cs` / `PlayerUIManager.cs` / `ModeSetting.cs` / `StageSetting.cs` / `MainGameManger.cs` / `PlayerDataHolder.cs` / `PlayerJoinedManager.cs`

---

### 🔴 P0 — PlayerStatusUI.cs:56 — null チェックが逆で NullReferenceException 確定

**問題**  
`stars != null` のときに return しているため、`stars == null` の状態でループに入りクラッシュする。

```csharp
// 現在（バグ：逆の判定）
if (stars != null) return;
for (int i = 0; i < stars.Length; i++) // ← stars が null でクラッシュ
```

**修正案**
```csharp
if (stars == null) return; // != を == に変更するだけ
```

---

### 🔴 P0 — PlayerJoinedManager.cs:98 — OnLeave() で全プレイヤーが削除される

**問題**  
`context.control.device != device` は同じ変数を比較しているため常に false。処理がスキップされず `joinDevices.Clear()` で全員が削除される。

```csharp
// 現在（バグ）
void OnLeave(InputAction.CallbackContext context)
{
    var device = context.control.device;
    if (context.control.device != device) return; // 常に false
    joinDevices.Clear(); // 全員削除！
}
```

**修正案**
```csharp
void OnLeave(InputAction.CallbackContext context)
{
    var device = context.control.device;
    if (joinDevices.Remove(device))
        UpdateDeviceTexts();
}
```

---

### 🔴 P0 — GameManager_M.cs:388-390 — 同一条件のチェックが二重ネスト

**問題**  
全く同じ条件 `if (player.transform.position.y < deathYCoordinate || ...)` が二重にネストされている（コピペミス）。外側のブロックが意味をなさない。

**修正案**: 外側の if 文を削除し、内側の処理のみ残す。

---

### 🔴 P0 — ModeSetting.cs:19 — PreviousMode() が NextMode() と同じ処理

**問題**  
「前のモード」に戻すはずが「次のモード」と同じ `+1` 処理になっている（コピペミス）。

```csharp
// 現在（バグ）
public void PreviousMode()
{
    currentMode = (GameMode)(((int)currentMode + 1) % 2); // NextMode と同じ
}
```

**修正案**
```csharp
public void PreviousMode()
{
    int count = System.Enum.GetValues(typeof(GameMode)).Length;
    currentMode = (GameMode)(((int)currentMode - 1 + count) % count);
    UpdateModeText();
}
```

---

### 🟠 P1 — SuddenDeathMode.cs:38-52 — Reflection の乱用

**問題**  
`typeof(PlayerController1).GetField("StrongKnockbackForce", flags)` でリフレクションを使っている。フィールド名を変えてもエラーが出ず、突然死モードが無音で無効になるバグが発生しうる。パフォーマンスも悪い。

**修正案**  
`PlayerController1` に public メソッドを追加してリフレクション不要にする。

```csharp
// PlayerController1 に追加
public void ApplyPowerUp(float multiplier)
{
    StrongKnockbackForce *= multiplier;
    WeakKnockbackForce *= multiplier;
}

// SuddenDeathMode では
playerCon.ApplyPowerUp(powerUpMultiplier);
```

---

### 🟠 P1 — ScoreManager.cs:4 — Singleton がシーン遷移で重複インスタンスを生成

**問題**  
`DontDestroyOnLoad` がないため、シーン遷移のたびに ScoreManager が増殖する。

```csharp
// 現在（不完全）
private void Awake()
{
    if (Instance == null) Instance = this;
}
```

**修正案**
```csharp
private void Awake()
{
    if (Instance == null)
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    else
    {
        Destroy(gameObject);
    }
}
```

---

### 🟠 P1 — GameManager_M.cs — 778行の巨大クラス（単一責任原則違反）

**問題**  
1つのクラスがゲーム状態管理・UI管理・タイマー・プレイヤー生死・スコア・シーン遷移・音声をすべて担当しており、どこかを変更すると別の機能が壊れるリスクが高い。

**修正案**（段階的に分離）
- `GameStateManager` — 開始・終了・ポーズ
- `RoundManager` — ラウンド進行・勝敗判定
- `PlayerLifeManager` — 落下・復活管理
- `ScoreManager` — スコア管理（一部既存）

---

### 🟠 P1 — GameManager_M.cs:759 — GetActivePlayers() で毎回 null 除去

**問題**  
呼び出しのたびに `RemoveAll(p => p == null)` を実行するのは非効率。

**修正案**: プレイヤー破棄時に `OnDestroy` から `activePlayers.Remove(gameObject)` を呼ぶ設計にする。

---

### 🟡 P2 — SoundManager.cs:84-93 — Dictionary を 2 回引いている

**問題**  
`ContainsKey()` した後に `[clip]` で再アクセスしており、同じキーを2回検索している。

**修正案**
```csharp
if (lastPlayTimes.TryGetValue(clip, out float lastTime) &&
    Time.time - lastTime < defaultSeInterval) return;
seSource.PlayOneShot(clip, seVolume);
lastPlayTimes[clip] = Time.time;
```

---

### 🟡 P2 — IGameMode インターフェース — ラウンド終了判定がない

**問題**  
現在の `IGameMode` にはモード完了を通知する仕組みがなく、`GameManager_M` が毎フレーム状態を監視する必要がある。

**修正案**
```csharp
public interface IGameMode
{
    void OnEnter();
    void OnUpdate();
    void OnExit();
    bool IsComplete { get; }
    event System.Action OnModeComplete;
}
```

---

### 🟢 P3 — コメントアウトコード・空メソッド

`PlayerJoinedManager.cs`（30〜96行周辺）、`MainGameManger.cs`（空の Update）など大量のコメントアウトコードを削除する。

---

---

# 3. ステージ・ギミック　52 / 100

対象: `Stage.cs` / `KillZone.cs` / `Landmine.cs` / `BouncyWall.cs` / `BoundBall.cs` / `conveyor.cs` / `ConveyorCartManager.cs` / `DollyCartFollowPath.cs` / `SplineCartRotationControl.cs` / `Splinecarthitter.cs` / `Cart_Player.cs` / `Meteor.cs` / `Meteor_ver2.cs` / `MeteorSound.cs` / `SpwanAndMoveDown.cs` / `ObjectSpawner.cs` / `ExplosionEffect.cs` / `Ruin.cs` / `Durability.cs` / `Loop.cs` / `TimedDestroy.cs` / `Delay.cs` / `DecalFitToParent.cs` / `おふざけ.cs` / `PlayerControllerForGimmickTest.cs` / `command.cs`

---

### 🔴 P0 — BouncyWall.cs:54 / Meteor.cs:30 — contacts[0] の存在確認なし

**問題**  
`collision.contacts[0]` は接触点が 0 件の場合に配列外アクセスでクラッシュする。

```csharp
// 現在（危険）
Vector3 normal = collision.contacts[0].normal;
```

**修正案**
```csharp
if (collision.contacts.Length == 0) return;
Vector3 normal = collision.contacts[0].normal;
```

---

### 🔴 P0 — BouncyWall.cs:23 — FixedUpdate() で毎フレーム FindGameObjectsWithTag を実行

**問題**  
`FixedUpdate()` 内でシーン全体を毎フレーム検索するため、フレームレートに直接影響する重大なパフォーマンス問題。ボールが増えるほど悪化する。

**修正案**: ボールが生成・削除されるタイミングで登録/削除する静的リストを使う。

```csharp
// ボール側に追加
public class BallRegistry : MonoBehaviour
{
    public static List<Rigidbody> AllBalls = new();
    void OnEnable()  { AllBalls.Add(GetComponent<Rigidbody>()); }
    void OnDisable() { AllBalls.Remove(GetComponent<Rigidbody>()); }
}

// BouncyWall では Find を使わず
private void FixedUpdate()
{
    foreach (var rb in BallRegistry.AllBalls)
        ballVelocities[rb] = rb.linearVelocity;
}
```

---

### 🔴 P0 — Loop.cs:22 — 3秒ごとにシーン全体をリロード

**問題**  
`SceneManager.LoadScene()` を `InvokeRepeating` で繰り返し呼んでいる。全オブジェクトが破棄・再生成されプレイヤーの進捗が失われる。停止手段もない。

**修正案**: デバッグ用であれば削除する。実際にリセットが必要な場合は、対象オブジェクトのみ初期化するメソッドを用意する。

---

### 🟠 P1 — Meteor.cs と Meteor_ver2.cs — 重複コードが 2 ファイルに存在

**問題**  
`ShrinkAndDestroy()` コルーチンなどほぼ同一の処理が2ファイルに分散。どちらが本番用か不明。

**修正案**
```csharp
public abstract class MeteorBase : MonoBehaviour
{
    protected IEnumerator ShrinkAndDestroy(float waitTime, float shrinkDuration)
    {
        // 共通処理
    }
}

public class Meteor : MeteorBase { }
public class Meteor_ver2 : MeteorBase { }
```

---

### 🟠 P1 — Ruin.cs と Durability.cs — 同一ロジックの重複実装

**問題**  
耐久値が 0 になったら `Destroy` する処理が2クラスに重複している。

```csharp
// Ruin.cs
if (durability == con) { Destroy(gameObject); }

// Durability.cs
if ((durability - count) == 0) { Destroy(this.gameObject); }
```

**修正案**: `Durability.cs` に統一し、`Ruin.cs` は削除する。

---

### 🟠 P1 — ObjectSpawner.cs — 無限 Instantiate によるメモリリーク

**問題**  
コルーチン内の `while(true)` で Instantiate し続けるが、生成数の上限や古いオブジェクトの削除がない。長時間プレイでメモリが枯渇する。

**修正案**: 最大生成数 `maxObjects` を設定し、上限に達したら古いものを削除してから生成する。または `ObjectPool` を使って再利用する。

---

### 🟠 P1 — BoundBall.cs:20-24 — Rigidbody を無視して transform を直接操作

**問題**  
物理オブジェクトに対して `transform.position` を直接操作すると物理演算との整合性が崩れる。

```csharp
// 現在（問題あり）
transform.position += velocity * Time.deltaTime;
```

**修正案**
```csharp
void FixedUpdate()
{
    rb.linearVelocity = velocity;
}
```

---

### 🟡 P2 — SpwanAndMoveDown.cs:37 — 実行時 AddComponent

**問題**  
`spawnedObject.AddComponent<MoveDown>()` は GC アロケーションを発生させる。Prefab に最初から含めるべき。

**修正案**: Prefab の GameObject に `MoveDown` コンポーネントをあらかじめアタッチし、`AddComponent` を削除する。

---

### 🟡 P2 — ConveyorCartManager.cs:54 — while(true) コルーチンを OnDestroy で止めていない

**修正案**
```csharp
private Coroutine cycleRoutine;

void Start()    { cycleRoutine = StartCoroutine(CycleRoutine()); }
void OnDestroy() { if (cycleRoutine != null) StopCoroutine(cycleRoutine); }
```

---

### 🟢 P3 — 日本語ファイル名・不明確な変数名

| 問題 | 修正案 |
|------|--------|
| `おふざけ.cs` | `EasterEgg.cs` |
| `Ruin.cs` の `con` | `damageCount` |
| `command.cs` の `kcnt` | `commandSuccessCount` |
| `BoundBall.cs` のクラス名 `AcceleratingBall` | ファイル名を `AcceleratingBall.cs` に変更 |

---

### 🟢 P3 — MeteorSound.cs / KillZone.cs — デッドコード

`MeteorSound.cs` は `Start` と `Update` が空なので削除。`KillZone.cs` の空の `Start()` / `Update()` も削除する。

---

---

# 4. ボット・AI　38 / 100

対象: `BOTController.cs` / `Seencer.cs` / `BotPlayerController1.cs` / `BotPowerMeter.cs` / `BotController.cs`

---

### 🔴 P0 — 3 つの BOT コントローラが並存し、どれが本番か不明

**問題**  
以下3ファイルが存在し機能が重複・矛盾している。コードの把握・修正が著しく困難。

- `/Assets/kento/Script/PlayerScript/BOT/BOTController.cs`
- `/Assets/kento/Scripts/Bot/BotPlayerController1.cs`
- `/Assets/Scripts/BotController.cs`

**修正案**  
実際にシーンで使われているものを1つ特定し、残りを削除する。統合後の推奨構造：

```
Assets/Scripts/Bot/
├── BotController.cs         ← メインコントローラ
├── BotSensor.cs             ← ステージ・敵検知
├── BotAttackBehavior.cs     ← 攻撃ロジック
└── BotMovementBehavior.cs   ← 移動ロジック
```

---

### 🔴 P0 — BotPlayerController1.cs:111-112 — GameObject.Find() の結果を null チェックせず使用

**問題**  
`ob = GameObject.Find("Timebox")` が null の場合、次行で NullReferenceException が発生。

**修正案**
```csharp
ob = GameObject.Find("Timebox");
if (ob == null) { Debug.LogError("Timebox not found"); return; }
gm = ob.GetComponent<GameManager_M>();
if (gm == null) { Debug.LogError("GameManager_M not found"); return; }
```

---

### 🔴 P0 — BOTController.cs:47 — Seencer が null でもエラーハンドリングなし

**問題**  
`GetComponentInChildren<Seencer>()` が null を返しても継続し、`sencer.CheckLayer()` でクラッシュ。

**修正案**
```csharp
sencer = GetComponentInChildren<Seencer>();
if (sencer == null)
{
    Debug.LogError($"{gameObject.name}: Seencer not found in children!");
    enabled = false;
    return;
}
```

---

### 🔴 P0 — BOTController.cs:76-79 — null チェック後の参照に return がない

**問題**  
`pointTarget == null` で `CreatePoint()` を呼んだ直後、同フレーム内で `pointTarget` を参照するとクラッシュする可能性がある。

**修正案**
```csharp
if (pointTarget == null)
{
    CreatePoint();
    return; // 次フレームまで処理をスキップ
}
```

---

### 🟠 P1 — BotPlayerController1.cs / BOTController.cs — FindGameObjectsWithTag を毎フレーム実行

**問題**  
`GameObject.FindGameObjectsWithTag("Player")` を `Update()` 内で毎フレーム実行。シーン全体をスキャンするため CPU 負荷が高い。

**修正案**
```csharp
private static List<GameObject> playerCache = new();
private float cacheTimer = 0f;

void Update()
{
    cacheTimer -= Time.deltaTime;
    if (cacheTimer <= 0f)
    {
        playerCache = new List<GameObject>(GameObject.FindGameObjectsWithTag("Player"));
        cacheTimer = 0.5f; // 0.5 秒ごとに更新
    }
    // 以降は playerCache を使用
}
```

---

### 🟠 P1 — BotPlayerController1.cs:301 — OnTriggerStay で毎フレーム Raycast

**問題**  
`OnTriggerStay()` は毎フレーム呼ばれる。その中で `Physics.Raycast()` を実行すると CPU 負荷が累積する。

**修正案**
```csharp
private float collisionCheckTimer = 0f;
private const float CHECK_INTERVAL = 0.1f;

void OnTriggerStay(Collider other)
{
    collisionCheckTimer += Time.deltaTime;
    if (collisionCheckTimer < CHECK_INTERVAL) return;
    collisionCheckTimer = 0f;
    // Raycast 処理
}
```

---

### 🟠 P1 — BotPlayerController1.cs:193 — アニメーションパラメータのタイポ

**問題**  
`animator.SetBool("IsChage", isStrt)` → "Charge" のタイポ。Animator 側のパラメータ名と不一致の場合、アニメーションが動かない。

**修正案**: `"IsCharging"` に統一（Animator 側も合わせて変更）。

---

### 🟠 P1 — ステージ外判定が 2 種類存在（Seencer vs IsOutOfStage）

**問題**  
- `BOTController` は `Seencer.CheckLayer()`（Trigger ベース）
- `BotPlayerController1` は `IsOutOfStage()`（距離計算ベース）

同じ目的の判定が2通り存在し、どちらが正しいか不明でバグの温床になる。

**修正案**: `StageManager` クラスに統一された判定メソッドを作る。

```csharp
public static class StageManager
{
    public static bool IsInStage(Vector3 position) { /* 統一判定 */ }
}
```

---

### 🟡 P2 — BOTController.cs:175 vs BotPlayerController1.cs:215 — 物理操作方式の混在

**問題**  
`BOTController.cs` は `transform.forward` で直接回転、`BotPlayerController1.cs` は `rb.MovePosition()` で移動。同一オブジェクトで混在させると物理演算が不安定化する。

**修正案**: Rigidbody がある場合はすべて `rb.MovePosition()` / `rb.MoveRotation()` に統一する。

---

### 🟢 P3 — 変数名が不明確

| 現在 | 推奨 |
|------|------|
| `t` | `currentChargeAmount` |
| `r` | `attackRange` |
| `a`（Attack の引数） | `shouldCharge` |
| `isPrese` | `isCharging` |
| `isfinish` | `isInRecovery` |
| `Seencer` | `StageSensor` |

---

### 🟢 P3 — BotPowerMeter.cs:未使用変数

```csharp
private Coroutine meter; // 定義されているが一度も使われていない
```

削除する。

---

---

# 5. アイテム　42 / 100

対象: `Item.cs` / `ItemController.cs` / `ItemSpawn.cs` / `PlayerItemEffect.cs` / `CloroChange.cs`

---

### 🔴 P0 — ItemController.cs:34 — 論理エラー（アイテムが永遠に取得されない）

**問題**  
`if (player != null && !player)` は `player != null` かつ `player == null` という矛盾条件で常に false になる。アイテムが絶対に取得されない。

```csharp
// 現在（バグ）
if (player != null && !player) { player.ApplyItem(item); Destroy(gameObject); }
```

**修正案**
```csharp
if (player != null)
{
    player.ApplyItem(item);
    Destroy(gameObject);
}
```

---

### 🔴 P0 — PlayerItemEffect.cs:65-81 — Random.Range(0, 2) で PaintEffect が絶対に実行されない

**問題**  
`Random.Range(0, 2)` は 0 または 1 しか返さない（上限は排他的）。`r == 2` の分岐（PaintEffect）は永遠に実行されない。

```csharp
// 現在（バグ）
int r = Random.Range(0, 2); // 0 か 1 しか返らない
// ...
else { StartCoroutine(PaintEfect(item)); } // ← 永遠に実行されない
```

**修正案**
```csharp
int r = Random.Range(0, 3); // 0, 1, 2 を返す
```

---

### 🟠 P1 — PlayerItemEffect.cs — SmallBox が Item.Type に定義されているが未実装

**問題**  
`switch` 文に `SmallBox` ケースがなく `default` もないため、使用されてもサイレントに無視される。デバッグが困難。

**修正案**
```csharp
default:
    Debug.LogWarning($"未実装のアイテムタイプ: {item.type}");
    break;
```

---

### 🟠 P1 — ItemSpawn.cs:42-52 — prefabs が 1 件以下のとき無限ループの可能性

**問題**  
`Length <= 1` のとき do-while が同じ index を選び続けて無限ループになる。

**修正案**
```csharp
if (itemPrefabs.Length == 0) { Debug.LogError("No item prefabs!"); return; }

int i;
if (itemPrefabs.Length == 1)
{
    i = 0;
}
else
{
    do { i = Random.Range(0, itemPrefabs.Length); } while (i == lastIndex);
}
```

---

### 🟠 P1 — PlayerItemEffect.cs:113 — FindObjectsByType() の毎回実行

**問題**  
ReverseEffect のたびに全 `PlayerItemEffect` を検索。非効率。

**修正案**
```csharp
private static List<PlayerItemEffect> allPlayers = new();
void OnEnable()  { allPlayers.Add(this); }
void OnDisable() { allPlayers.Remove(this); }

// ReverseEffect 内では
foreach (var player in allPlayers) { /* ... */ }
```

---

### 🟠 P1 — ItemController.cs:17-26 — Update() で毎フレーム位置を固定

**問題**  
毎フレーム `transform.position` を設定して Y 軸を固定するのは非効率で物理演算と干渉する。

**修正案**
```csharp
void Start()
{
    var rb = GetComponent<Rigidbody>();
    if (rb != null) rb.constraints = RigidbodyConstraints.FreezePositionY;
}
```

---

### 🟡 P2 — アイテムシステムの拡張性が低い（Switch 文で全管理）

**問題**  
新しいアイテムを追加するたびに `PlayerItemEffect.cs` の `switch` 文を編集する必要があり、オープン・クローズド原則に違反している。

**修正案（Strategy パターン）**
```csharp
public interface IItemEffect
{
    IEnumerator Apply(PlayerItemEffect player, Item item);
}

public class BigItemEffect   : IItemEffect { /* ... */ }
public class ReverseItemEffect : IItemEffect { /* ... */ }

// PlayerItemEffect では
private Dictionary<Item.ItemType, IItemEffect> effectMap = new()
{
    { Item.ItemType.BigBox, new BigItemEffect() },
    { Item.ItemType.ReverseControl, new ReverseItemEffect() },
};
```

---

### 🟢 P3 — スペルミス・命名問題

| ファイル | 現在 | 推奨 |
|---------|------|------|
| ItemSpawn.cs | `itemPrefabes` | `itemPrefabs` |
| ItemSpawn.cs | `onj` | `spawnedItem` |
| PlayerItemEffect.cs | `defaltCircleSize` | `defaultCircleSize` |
| CloroChange.cs（クラス名） | `CloroChange` | `ColorChange` |

---

### 🟢 P3 — CloroChange.cs — 冗長な if 文

**修正案**
```csharp
Material[] materials = { red, blue, green, yellow };
if (input.playerIndex >= 0 && input.playerIndex < materials.Length)
    render.material = materials[input.playerIndex];
```

---

---

# 全体設計の問題

## Singleton の乱用

現在のプロジェクトには 5 つ以上の Singleton が存在する（`GameManager_M` / `SoundManager` / `ScoreManager` / `PlayerUIManager` / `PlayerDataHolder`）。テストが困難で依存関係が不透明になっている。

**推奨**: 依存関係をコンストラクタ / インスペクタ経由で明示的に渡す設計に段階的に移行する。

## Update / FixedUpdate の混在

移動処理が `Update()` で、物理演算が `FixedUpdate()` で行われている箇所が混在している。Rigidbody を使う処理は必ず `FixedUpdate()` に統一する。

## コメントアウトコードの大量残存

ほぼすべてのファイルにコメントアウトコードが残っており、実装の意図が読み取りにくい。git の履歴で追えるため、すべて削除してよい。

## スペルミスの統一

プロジェクト全体で "Atack" / "Efect" / "Seencer" などのスペルミスが蔓延している。複数人開発ではコミュニケーションコストが高くなるため、一括リファクタリングで修正する。

---

# 修正優先度サマリー

## 🔴 今すぐ修正（P0）

| # | ファイル | 問題 |
|---|---------|------|
| 1 | `PlayerStatusUI.cs:56` | null チェックが逆でクラッシュ確定 |
| 2 | `PlayerJoinedManager.cs:98` | OnLeave が全プレイヤーを削除 |
| 3 | `ItemController.cs:34` | 論理エラーでアイテムが取得不能 |
| 4 | `PlayerItemEffect.cs:67` | Random.Range バグで PaintEffect が実行不能 |
| 5 | `PlayerColorChan.cs:17` | GetComponent\<Material\>() で常に null |
| 6 | `ModeSetting.cs:19` | PreviousMode が NextMode と同一 |
| 7 | `GameManager_M.cs:388` | 同一条件の二重チェック（コピペミス） |
| 8 | `Reception.cs:90` | ノックバックコルーチンの重複実行 |
| 9 | `BouncyWall.cs:54` / `Meteor.cs:30` | contacts[0] の存在確認なし |
| 10 | `BotPlayerController1.cs:111` | GameObject.Find() の null チェックなし |

## 🟠 高優先度（P1）

| # | 問題 |
|---|------|
| 1 | 3 つの BOTController を 1 つに統合 |
| 2 | `BouncyWall.cs:23` — FixedUpdate 内の FindGameObjectsWithTag |
| 3 | `SuddenDeathMode.cs:38` — Reflection 乱用 → public メソッド化 |
| 4 | `ScoreManager.cs:4` — DontDestroyOnLoad の追加 |
| 5 | `Loop.cs:22` — 全シーンリロードの意図確認と修正 |
| 6 | `GameManager_M.cs` — 責任分離（SRP 改善） |
| 7 | `Reception.cs` vs `Reception1.cs` — どちらか一方に統一 |
| 8 | Invoke() 全箇所 → Coroutine に変換 |
| 9 | `Meteor.cs` と `Meteor_ver2.cs` — 共通基底クラスで統合 |
| 10 | `ObjectSpawner.cs` — 無限 Instantiate の上限設定 |

## 🟡 中優先度（P2）

| # | 問題 |
|---|------|
| 1 | Camera.main のキャッシュ化 |
| 2 | FindGameObjectsWithTag 全箇所のキャッシュ化 |
| 3 | アイテムシステムへの Strategy パターン導入 |
| 4 | IGameMode に IsComplete プロパティ追加 |
| 5 | Update 内での毎フレーム重い処理の削減 |

## 🟢 低優先度（P3）

| # | 問題 |
|---|------|
| 1 | スペルミスの一括修正 |
| 2 | 空メソッド・コメントアウトコードの削除 |
| 3 | 日本語ファイル名の英語化（`おふざけ.cs`） |
| 4 | public フィールドを `[SerializeField] private` に変更 |
| 5 | Magic Number を定数化 |

---

*このレポートはマルチエージェント並列レビュー（5エージェント同時）によって生成されました。*

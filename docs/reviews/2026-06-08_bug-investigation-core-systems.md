# バグ調査・コードヘルスレビュー：コアシステム全体

- **レビュー日**: 2026-06-08
- **スコープ**: プレイヤー・BOT・GameManager・アイテム・ギミック各システム
- **モード**: バグ調査 + コードヘルス複合レビュー

## レビュー概要

コアループ（参加→対戦→リザルト）は形になっているが、アイテムシステムの初期化バグ・死亡検知の2系統化・削除予定クラスへの依存が混在しており、このまま本番統合するとリザルトやモード切替が意図通りに動かないリスクが高い。

---

## 致命的な挙動リスク

### [致命的] PlayerItemEffect.OnEnable の null チェック順序が逆で初期化が常にスキップされる

- **場所**: `Assets/kento/Script/ItemScript/PlayerItemEffect.cs:34–36`
- **問題**:
  ```csharp
  GameObject obj = GameObject.Find("PaintImage");
  if(paint == null) { return; }   // ← paint はまだ null のまま
  paint = obj.GetComponent<Image>(); // ← ここに到達しない
  ```
  `paint` が代入される前に null チェックで `return` するため、`paint`・`text` は永遠に null のまま。`OnEnable` 以降の `PaintEfect`・`ReverseEfect` の UI 表示ロジックが全滅する。アイテム取得した瞬間に NullReferenceException が出るか、UI が一切表示されない。
- **なぜ重要か**: PaintBox・ReverseBox 系のアイテム効果は UI フィードバックが前提のゲームプレイ要素。動作が完全に死んでいるので見えない不具合として残る。
- **改善案**:
  ```csharp
  private void OnEnable()
  {
      GameObject obj = GameObject.Find("PaintImage");
      if (obj == null) { return; }
      paint = obj.GetComponent<Image>();

      GameObject a = GameObject.Find("ItemTxt");
      if (a == null) { return; }
      text = a.GetComponent<Text>();

      // 以降の処理...
  }
  ```
  加えて、毎回 `Find` するのは `OnEnable` の呼び出し頻度次第では重い。`Start` で一度だけキャッシュする設計が望ましい。

---

### [致命的] PlayerStateModule が削除予定の PlayerDataHolder を参照している

- **場所**: `Assets/Miwa/scripts/New Folder/PlayerStateManager_M.cs:21`
- **問題**:
  ```csharp
  foreach (var player in PlayerDataHolder.Instance.players)
  ```
  `PlayerDataHolder` は `Assets/kento/Script/Delete/PlayerDataHolder.cs` にあり、フォルダ名から明示的に廃止予定。現行の `MainGameManger.cs` は `JoinData` を使っており `PlayerDataHolder` をシーンに配置していないと思われる。`SetAllPlayersControl(false/true)` 呼び出し時（カウントダウン終了・ゲームオーバー時）に `NullReferenceException` が発生し、プレイヤーの操作制御が機能しなくなる。
- **なぜ重要か**: カウントダウン中に操作できてしまう / ゲームオーバー後も動き続けるという致命的な挙動になる。
- **改善案**: `PlayerStateModule.SetAllPlayersControl` 内のリストを `PlayerDataHolder` ではなく `activePlayers`（同クラス内のフィールド）に切り替える。
  ```csharp
  public void SetAllPlayersControl(bool enabled)
  {
      foreach (var player in activePlayers) // ← PlayerDataHolder → activePlayers
      {
          if (player == null) continue;
          // ...
      }
  }
  ```

---

## アーキテクチャと状態管理

### [高] 死亡検知が KillZone と GameManager の2系統で矛盾している

- **場所**: `Assets/Scripts/KillZone.cs:21` / `Assets/Miwa/scripts/New Folder/PlayerStateManager_M.cs:CheckPlayersFalling()`
- **問題**:
  - `KillZone` → `Destroy(other.gameObject)` で直接消す。GameManager への通知なし。
  - `GameManager_M.CheckPlayersFalling()` → 座標判定で `SetActive(false)` → `OnPlayerEliminated()` 呼び出し。

  `KillZone` 経由で Destroy されると `activePlayers` リストに `null` が残り続け、`GetActivePlayers()` の `RemoveAll(p => p == null)` で取り除かれるが、その前にラウンド判定が走ると生存者数が誤カウントされる。`Gimmick_Landmine.unity` には KillZone が存在するが `MainScene.unity` には存在しないことを確認済みで、シーンごとに挙動が変わる。
- **なぜ重要か**: ラウンド終了・サドンデス突入・最終リザルトの判定がシーンによって壊れる。
- **改善案**: `KillZone` に GameManager 通知を追加して一本化する。
  ```csharp
  void OnCollisionEnter(Collision other)
  {
      if (!other.gameObject.CompareTag("Player")) return;
      var health = other.gameObject.GetComponent<PlayerHealth>();
      if (health != null) health.OnFallOut();
      else Destroy(other.gameObject);
  }
  ```
  または `KillZone` を廃止して `CheckPlayersFalling()` の座標判定に統一する。

### [高] useDebugMode フラグが Start() で一切参照されていない

- **場所**: `Assets/Miwa/scripts/New Folder/GameManager_M.cs:25–26, 104`
- **問題**:
  ```csharp
  public bool useDebugMode = true;       // Inspector で true
  public Mode debugGameMode = Mode.ScoreMode;
  // ...
  CurrentModeState = selectedGameMode;   // debugGameMode は無視
  ```
  `useDebugMode = true` にしていても `debugGameMode` は一切使われず、常に静的変数 `selectedGameMode`（デフォルト `Mode.Survival`）が使われる。直接シーン再生でモードを切り替えようとしても反映されない。
- **なぜ重要か**: チームメンバーが「ScoreModeをテストしたい」と `useDebugMode = true` / `debugGameMode = ScoreMode` に設定しても効かない。開発効率に直接影響する。
- **改善案**: Start() に以下を追加する。
  ```csharp
  if (useDebugMode)
      selectedGameMode = debugGameMode;
  CurrentModeState = selectedGameMode;
  ```

---

## パフォーマンスとフレーム安定性

### [中] BOTController が毎フレーム FindGameObjectsWithTag を呼ぶ可能性がある

- **場所**: `Assets/kento/Script/PlayerScript/BOT/BOTController.cs:78, 216–264`
- **問題**: `Update()` → `Serch()` が毎フレーム呼ばれる。`targetHoldTimer > 0f && near != null` の間は早期 return するが、タイマーが切れたフレームは `FindGameObjectsWithTag("Player")` を実行し、全候補のループと重み計算も走る。4体BOT環境で同フレームに複数BOTのタイマーが切れると集中してシーン検索が走る。
- **なぜ重要か**: `FindGameObjectsWithTag` はフレームごとに呼んでいい処理ではなく、規模が増えるとスパイクの原因になる。
- **改善案**: プレイヤーのリストをゲーム開始時にキャッシュし、脱落時だけ更新する。
  ```csharp
  private List<GameObject> cachedPlayers;

  void Start()
  {
      // ...
      RefreshPlayerCache();
  }

  void RefreshPlayerCache()
  {
      cachedPlayers = new List<GameObject>(
          GameObject.FindGameObjectsWithTag("Player")
      );
  }
  // Serch() 内では cachedPlayers を使う
  ```

---

## 保守性とチーム開発速度

### [中] ItemSpawn のスポーン範囲がコードハードコードで Inspector から変更不可

- **場所**: `Assets/kento/Script/ItemScript/ItemSpawn.cs:7–9`
- **問題**:
  ```csharp
  private Vector2 posX = new Vector2(-7f, 7f);
  private Vector2 posZ = new Vector2(-7f, 7f);
  private float posY = 10f;
  ```
  `private` のため Inspector から変更できない。ステージが変わるたびにコードを編集する必要がある。
- **改善案**: `[SerializeField]` に変更するだけでよい。
  ```csharp
  [SerializeField] private Vector2 posX = new Vector2(-7f, 7f);
  [SerializeField] private Vector2 posZ = new Vector2(-7f, 7f);
  [SerializeField] private float posY = 10f;
  ```

### [中] Landmine のノックバックが上方向のみで爆発感がない

- **場所**: `Assets/Scripts/Landmine.cs:40`
- **問題**:
  ```csharp
  rb.AddForce(Vector3.up * explosionForce, ForceMode.VelocityChange);
  ```
  爆心から外向きのベクトル計算がなく、全員が真上に飛ぶだけ。隣のプレイヤーも中央のプレイヤーも同じ方向に飛ぶ。
- **改善案**:
  ```csharp
  Vector3 dir = (hit.transform.position - transform.position).normalized;
  dir.y = Mathf.Max(dir.y, 0.4f); // 最低限の上方向を確保
  rb.AddForce(dir * explosionForce, ForceMode.VelocityChange);
  ```

### [低] ObjectSpawner(MoveObject) の削除判定が原点基準

- **場所**: `Assets/Scripts/ObjectSpawner.cs:97`
- **問題**:
  ```csharp
  if (Vector3.Distance(Vector3.zero, transform.position) > 50f)
  ```
  ワールド原点を基準に距離判定している。スポーナーが原点から離れた位置に置かれると、生成直後のオブジェクトが即座に消滅する可能性がある。
- **改善案**: スポーナー自身の位置を基準にするか、生成後の移動量で判定する。

---

## 推奨アクション

1. **`PlayerItemEffect.OnEnable` の null チェック順序を修正する** — 5分で直る・アイテムシステム全体が壊れているため最優先
2. **`PlayerStateModule.SetAllPlayersControl` の `PlayerDataHolder` 参照を `activePlayers` に切り替える** — カウントダウン・ゲームオーバーの操作制御が機能しない
3. **`KillZone` に `PlayerHealth.OnFallOut()` 呼び出しを追加するか廃止して一本化する** — ラウンド判定の信頼性に直結
4. **`useDebugMode` フラグを Start() に適用する** — チーム全員がモード切替テストで詰まらないために早めに対処
5. **`ItemSpawn` のスポーン範囲を `SerializeField` 化する** — ステージごとの調整コスト削減

---

## 未確認事項

- `MainScene` に `MainGameManger` と `GameManager_M` のどちらが（または両方が）配置されているか未確認。両方あれば Singleton 衝突でどちらかが `Destroy` される。
- `PlayerDataHolder` がシーンに配置されているかどうか（配置されていれば問題②は今は起きていない可能性があるが、廃止フォルダにある以上いつ消えてもおかしくない）。
- `BOTController` の `Seencer` コンポーネントの `CheckLayer()` が何をしているか未読。センサー判定が正しく機能しているかはプレイテストで確認が必要。
- `Delete/` フォルダ内のスクリプトがコンパイル対象になっているかどうか（Unity はデフォルトで `Delete` という名前でもコンパイルする）。

# バグレポート：カウントダウン中にBOTが動いて攻撃してしまう

> **対象モード**: サバイバルモード・スコアモード（両方）
> **調査方法**: マルチエージェント並列コードレビュー（3エージェント同時）
> **作成日**: 2026-04-27

---

## どんなバグか

ゲーム開始前のカウントダウン（3・2・1・Fight!!）が表示されている最中に、BOTが勝手に動いたり攻撃したりしてしまう。

---

## 原因

### 一言でいうと

**古いBOTコントローラー（`BOTController.cs`）にゲーム開始前は動かないという処理が書かれていない。**

### もう少し詳しく

プロジェクトにはBOTを制御するスクリプトが3種類存在しています。

| ファイル | 場所 | カウントダウン中に止まるか |
|---|---|---|
| `BotController.cs` | Assets/Scripts/ | ✅ 止まる（正常） |
| `BotPlayerController1.cs` | Assets/kento/Scripts/Bot/ | ✅ 止まる（正常） |
| **`BOTController.cs`** | **Assets/kento/Script/PlayerScript/BOT/** | **❌ 止まらない（バグ）** |

正常な2つには「ゲームが始まっていなければ何もしない」という1行のチェックが入っています。

```csharp
// BotController.cs 43行目・BotPlayerController1.cs 121行目（どちらも同じ書き方）
if (GameManager_M.Instance != null && !GameManager_M.Instance.IsGameStartedProperty) return;
```

しかし `BOTController.cs` にはこのチェックが**まったく存在しない**ため、毎フレーム以下の処理が走り続けます。

```
Serch()       → 一番近くの敵を探して追いかける
Attack()      → 攻撃を準備して実際に撃つ（Shot()呼び出し）
MoveToPoint() → 移動入力を送信する
```

### なぜ「止める処理を追加しても効かないのか」

`GameManager_M.cs` には `SetAllPlayersControl(false)` というカウントダウン中に全員を止める処理があります（151行目）。しかしこのメソッドは以下しか止めません。

- PlayerInput（コントローラー入力）の無効化
- Rigidbodyのモード変更（Kinematicにして物理で動かなくする）
- MoveControllerスクリプトの無効化

BOTはコントローラー入力を使わず、自分のスクリプト（Update()）で直接動くため、**このメソッドをいくら呼んでもBOTには効きません。**

---

## スコアモード固有の追加問題

サバイバルモードとスコアモードでカウントダウンの扱いが違います。

| モード | カウントダウン | ゲーム開始のタイミング |
|---|---|---|
| サバイバルモード | 3→2→1→Fight!! あり | カウントダウン完了後 |
| **スコアモード** | **なし** | **モード切り替えと同時に即開始** |

`ScoreMode.cs` の `OnEnter()` メソッド（23行目）でゲーム開始フラグが即座に立つため、スコアモードでは理論上カウントダウン中という状態が存在しません。スコアモードでBOT問題が起きるなら、カウントダウン機能の実装自体が必要です。

---

## 対処法

### 【必須】最小限の修正（1行追加するだけ）

**`Assets/kento/Script/PlayerScript/BOT/BOTController.cs`** を開いて、`Update()` の一番最初に以下を追加してください。

```csharp
void Update()
{
    // ↓ この1行を追加する
    if (GameManager_M.Instance != null && !GameManager_M.Instance.IsGameStartedProperty) return;

    if(stateManager.State == State.Knockback) { return; }
    // 以下は既存コードのまま...
```

これだけでサバイバルモードのBOT問題は解消されます。

---

### 【推奨】スコアモードにカウントダウンを追加する

**`Assets/Miwa/scripts/ScoreMOdeScripts/ScoreMode.cs`** の `OnEnter()` を修正します。

現在の実装（問題あり）：
```csharp
public void OnEnter()
{
    isTimerActive = true; // ← 入った瞬間タイマー開始
    // ...
}
```

修正案：タイマー開始を `GameManager_M` のカウントダウン完了後に任せる。  
`GameManager_M.cs` の `StartCountdown()` コルーチン末尾（177〜179行目付近）で、スコアモードのタイマーも開始するよう修正するのが最もシンプルです。

---

### 【任意】念のため他のスクリプトにも追加する

現状 `MoveController.cs`（52行目付近）と `AtackController.cs`（96行目付近）にもゲーム開始前の動き止め処理がありません。プレイヤー自身がカウント中に動けてしまう場合に対応できます。

---

## 修正チェックリスト

- [ ] `BOTController.cs` の `Update()` 冒頭にフラグチェックを1行追加
- [ ] Unity Editorで実際にBOTに `BOTController.cs` が付いているか Inspectorで確認
- [ ] サバイバルモードでカウント中にBOTが止まっているか確認
- [ ] スコアモードにもカウントダウンを実装するか検討
- [ ] `MoveController` / `AtackController` への追加チェック（任意）

---

## 関係するファイル一覧

| ファイル | 役割 | 修正要否 |
|---|---|---|
| `Assets/kento/Script/PlayerScript/BOT/BOTController.cs` | 問題のBOTコントローラー | **必須** |
| `Assets/Miwa/scripts/ScoreMOdeScripts/ScoreMode.cs` | スコアモードのロジック | **推奨** |
| `Assets/Miwa/scripts/GameManager_M.cs` | カウントダウン・ゲーム管理 | 参照のみ |
| `Assets/kento/Scripts/Bot/BotPlayerController1.cs` | 正常なBOTコントローラー | 変更不要 |
| `Assets/Scripts/BotController.cs` | 正常なBOTコントローラー | 変更不要 |
| `Assets/kento/Script/PlayerScript/MoveController.cs` | プレイヤー移動 | 任意 |
| `Assets/kento/Script/PlayerScript/AtackController.cs` | プレイヤー攻撃 | 任意 |

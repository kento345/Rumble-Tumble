# バグレポート: 1P/3P コントローラー入れ替わりバグ

**報告日**: 2026-05-08  
**対象ブランチ**: main  
**ステータス**: 調査完了・未修正（要修正）

---

## 症状

- 1Pに登録したコントローラーが3Pキャラクターを操作する
- 逆に3Pに登録したコントローラーが1Pキャラクターを操作する
- 特定のプレイヤー数・ロビー操作条件で再現する

---

## 確認バグ

### バグ①（最有力原因）: `PlayerJoinedManager.OnLeave()` の自己比較バグ

**ファイル**: `Assets/kento/Scripts/Joint/PlayerJoinedManager.cs:88-101`

```csharp
void OnLeave(InputAction.CallbackContext context)
{
    var device = context.control.device;          // ← device に代入

    if (context.control.device != device) return; // ← 自分自身と比較 → 常に false → return は絶対実行されない

    joinDevices.Clear();                          // ← 常に全デバイスを消す（バグ）
    //joinDevices.Remove(device);                 // ← 正しい処理がコメントアウトされている
    UpdateDeviceTexts();
}
```

**問題点**: `context.control.device != device` は `device = context.control.device` と代入した直後に同一インスタンスと比較しているため、常に `false` になる。そのため `return` は絶対に実行されず、ロビー画面で誰か一人でも離脱ボタンを押すと `joinDevices` が全削除される。

**修正すべき正しい処理**:
```csharp
joinDevices.Remove(device);  // Clear() ではなく Remove(device)
```

---

### バグ②（副次的原因）: `PlayerHealth.Start()` の playerIndex 読み取り元

**ファイル**: `Assets/Miwa/scripts/PlayerHealth.cs:12`

```csharp
var input = GetComponent<PlayerInput>();
if (input != null) playerIndex = input.playerIndex;  // ← Unity が自動付番した値を読む
```

**問題点**: `MainGameManger.Awake()` は `Instantiate()` を使っており、`PlayerInput.Instantiate(playerIndex: i)` を使っていない。そのため `input.playerIndex` は Unity Input System が InputUser 生成順に自動付番した値（0, 1, 2, 3...）になる。デバイス配列の順序と生成順序が一致していれば問題ないが、バグ①で配列順序が乱れた後は、GameManager/UI/スコア側が間違ったプレイヤー番号で管理することになる。

---

## 1P↔3P 入れ替わりの最有力シナリオ

| ステップ | 内容 |
|---|---|
| 1 | ロビーで 1P, 2P, 3P が参加 → `joinDevices = [1P, 2P, 3P]` |
| 2 | 誰かが Leave を押す → **バグ① が発動し全デバイス消去** |
| 3 | 再参加で順序が変わる → `joinDevices = [3P, 2P, 1P]` |
| 4 | `SetDevices()` でこの順序がバトルシーンへ引き継がれる |
| 5 | `MainGameManger` が i=0 で `devices[0]`（3Pのデバイス）をスポーン位置1に配置 |
| 6 | `PlayerHealth.Start()` が `input.playerIndex = 0` と読み、GameManager に「これは0番（1P）」として登録 |
| 7 | **結果**: 3Pのコントローラーが1Pキャラを動かし、UIも1Pとして表示 |

---

## 確信度

**バグ①（OnLeave 全消去）**: **確定**（コードが証明している）  
**バグ②（playerIndex 読み取り元）**: **確定**（コードが証明している）  
**①が 1P↔3P 入れ替わりの直接トリガー**: **8割** — ロビーで Leave が押された場合に限定されるが、コードを見る限りこれ以外に配列順序を変える処理は見当たらない

---

## 推奨アクション

### 優先度: 高（バグ①）

`PlayerJoinedManager.cs:98` の `joinDevices.Clear()` を `joinDevices.Remove(device)` に変える。

```csharp
void OnLeave(InputAction.CallbackContext context)
{
    var device = context.control.device;
    joinDevices.Remove(device);  // Clear() → Remove(device) に変更
    UpdateDeviceTexts();
}
```

### 優先度: 中（バグ②）

`MainGameManger.cs` を `PlayerInput.Instantiate(playerIndex: i, pairWithDevice: devices[i])` に戻す。これにより `input.playerIndex` が意図したスロット番号と一致するようになる。

---

## 調査対象ファイル

- `Assets/kento/Scripts/Joint/PlayerJoinedManager.cs`
- `Assets/kento/Scripts/Joint/PlayerDataHolder.cs`
- `Assets/kento/Scripts/Joint/MainGameManger.cs`
- `Assets/Miwa/scripts/PlayerHealth.cs`
- `Assets/Prot/prot.unity`

# バグ調査レポート：1P/2P コントローラー割り当てズレ

**調査日**: 2026-05-15  
**対象ブランチ**: kento2  
**報告者**: Claude Code（調査のみ・修正未実施）

---

## 症状

接続したコントローラーに期待と異なる playerIndex が割り当てられ、本来 1P 想定のコントローラーが 2P 扱いになる。

---

## 原因

### 誤解されやすい点

「コントローラーの deviceId が干渉している」という仮説が出ていたが、コード上に `deviceId` の直接利用箇所は存在しない。

**本質的な問題は以下の2点。**

---

### ① MainGameManger.cs で `playerIndex` を未指定（主因）

**`Assets/kento/Script/JoinScript/MainGameManger.cs` line 42–44:**

```csharp
// 現状：playerIndex を指定していない
var obj = PlayerInput.Instantiate(
    prefab: playerPrefab,
    pairWithDevice: device   // playerIndex の指定なし
);
```

`PlayerInput.Instantiate` に `playerIndex` を渡さない場合、Unity は **シーン内の既存 `PlayerInput.all` の数** をもとに自動採番する。

Start シーン（`Assets/Prot/Start.unity`）に UI 用の `PlayerInput` コンポーネントが存在しており、シーン遷移時にそれが残存していると 1P 目が `playerIndex = 1` として採番される。結果として `JoinData` のリスト順（参加順）と Unity の `playerIndex` がズレる。

**旧コード（`Assets/kento/Script/Delete/MainManager.cs` line 90–93）では正しく指定していた:**

```csharp
var obj = PlayerInput.Instantiate(
    prefab: playerPrefab,
    playerIndex: i,          // 明示的に指定していた
    pairWithDevice: devices[i]
);
```

新しい `MainGameManger` への移行時にこの `playerIndex: i` が抜け落ちたのが根本原因。

---

### ② PlayerController1.cs が PlayerDataHolder を参照している（副次懸念）

**`Assets/kento/Scripts/Player/brink/PlayerController1.cs` line 87, 108:**

```csharp
if (!PlayerDataHolder.Instance.devices.Contains(context.control.device))
```

`PlayerDataHolder` は現在シーンに存在せず `Instance` が null のため、入力が通らない / クラッシュする可能性がある。  
現在の参加フローは `JoinData` を使っており、`PlayerDataHolder` とは別系統になっている。

---

## 関係ファイル

| ファイル | 問題箇所 |
|----------|----------|
| `Assets/kento/Script/JoinScript/MainGameManger.cs` | line 42：`playerIndex` 未指定 |
| `Assets/Prot/Start.unity` | UI 用 PlayerInput の残存可能性 |
| `Assets/kento/Scripts/Player/brink/PlayerController1.cs` | line 87, 108：PlayerDataHolder 参照（廃止済み） |

---

## 修正候補

**① MainGameManger.cs（優先度：高）**

```csharp
var obj = PlayerInput.Instantiate(
    prefab: playerPrefab,
    playerIndex: i,        // ← 追加
    pairWithDevice: device
);
```

**② PlayerController1.cs（プレハブで使用中であれば対応）**

`PlayerDataHolder.Instance.devices` → `JoinData.Instance.GetDevices()` に切り替える。

---

## 確認推奨事項

- `MainGameManger` の生成直後に `i`, `device.displayName`, `obj.playerIndex` をログ出力して採番状況を確認する
- `prot` シーン遷移直後に `PlayerInput.all` の中身をログ出力し、ゲーム用以外の `PlayerInput` が残っていないか確認する

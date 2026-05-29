# バグレポート: プレイヤーキャラクター増殖バグ

**報告日**: 2026-05-08  
**対象ブランチ**: main  
**ステータス**: 調査完了・未修正（要検証）

---

## 症状

- バトル開始時、プレイヤーキャラクターの真上に分身キャラクターが重なって表示される
- 2体が同じ座標に存在し、操作も共有される（同じデバイス入力に両方が反応）

---

## 調査結果

### 否定された仮説（Codex初期分析）

**「バトルシーンにPlayerオブジェクトが事前配置されている」→ 原因ではない**

`暴走トロッコ.unity` や `Meteor_ver2.unity` に Playerタグ付きオブジェクトが残っていることは確認したが、これらのシーンには `MainGameManger` が存在しない。実際のバトルシーンは `Assets/Prot/prot.unity` であり、そこにPlayer事前配置はゼロ。

### 確認事項

| 確認項目 | 結果 |
|---|---|
| `prot.unity` 内のPlayer事前配置 | **なし** |
| `prot.unity` 内の `PlayerInputManager` | **なし** |
| `prot.unity` 内の `MainGameManger` の数 | **1つのみ** |
| `PlayerDataHolder` の DontDestroyOnLoad | **あり**（JoinedManager GameObjectごと保持） |
| `PlayerJoinedManager` の直接スポーン処理 | **なし** |
| `GameManager_M` の DontDestroyOnLoad | **なし** |

### 最有力原因

**コミット `e2e186c`（Player修正 / 2026-05-08 kento）** における `MainGameManger.cs` の変更。

#### 変更前（正常動作時）
```csharp
var obj = PlayerInput.Instantiate(
    prefab: playerPrefab,
    playerIndex: i,
    pairWithDevice: devices[i]
);
obj.transform.position = pos[i].position;
obj.transform.rotation = pos[i].rotation;
```

#### 変更後（バグ発生時）
```csharp
var obj = Instantiate(playerPrefab, pos[i].position, pos[i].rotation);
PlayerInput input = obj.GetComponent<PlayerInput>();
input.user.UnpairDevices();
InputUser.PerformPairingWithDevice(devices[i], input.user);
```

`PlayerInput.Instantiate()` は Unity Input System が提供する正規のプレイヤー生成API。  
`playerIndex` の付与・InputUser の正規初期化・デバイスペアリングを一括で行う。  
通常の `Instantiate()` に切り替えたことで、`PlayerInput.OnEnable()` が想定外のデバイス自動ペアリングを行い、同一座標での二重生成・入力共有が起きている可能性が高い。

### 副次的な問題（直接原因ではないが要注意）

- `PlayerDataHolder` が `DontDestroyOnLoad(gameObject)` を呼んでいるため、同じ GameObject 上にある `PlayerJoinedManager` もシーン遷移後のバトルシーンに残存する。現状は直接的なバグに繋がっていないが、バトル中にジョイン入力が受け付けられる状態になっているため、潜在的リスクあり。

---

## 確信度

**7割** — `e2e186c` が最有力だが、正確なメカニズム（なぜ同一座標に2体出現するか）は Unity Editor 上でデバッグしないと完全には特定できない。

---

## 推奨アクション

1. **検証**: `e2e186c` を revert して `PlayerInput.Instantiate()` の元のコードに戻し、バグが解消するか確認する。
2. **修正**: 元のコードに戻すか、`Instantiate()` を使う場合は `playerIndex` の明示的な設定など `PlayerInput.Instantiate()` と同等の初期化処理を追加する。
3. **副次対応（任意）**: `PlayerJoinedManager` がバトルシーンに残存しないよう、`PlayerDataHolder` と `PlayerJoinedManager` を別 GameObject に分けることを検討する。

---

## 調査対象ファイル

- `Assets/kento/Scripts/Joint/MainGameManger.cs`
- `Assets/kento/Scripts/Joint/PlayerDataHolder.cs`
- `Assets/kento/Scripts/Joint/PlayerJoinedManager.cs`
- `Assets/Prot/prot.unity`
- `Assets/Prot/Start.unity`
- `Assets/Prot/Playerprefab/ProtPlayer/Player.prefab`
- `Assets/Miwa/scripts/GameManager_M.cs`

# フォーカスレビュー：サドンデス移行処理

- **レビュー日**: 2026-06-01
- **モード**: フォーカスレビュー
- **対象**:
  - `Assets/Miwa/scripts/GameManager_M.cs`（NextRound, TriggerSuddenDeath, CheckPlayersFalling, RegisterPlayer）
  - `Assets/kento/Script/JoinScript/MainGameManger.cs`
  - `Assets/Miwa/scripts/SuddenDeathMode.cs`
  - `Assets/Miwa/scripts/PlayerHealth.cs`
  - `Assets/Scripts/KillZone.cs`

---

## 指摘事項

### [致命的] KillZone が GameManager に死亡を通知しないため activePlayers のカウントがズレる

- 場所: `Assets/Scripts/KillZone.cs:19`
- 問題: `KillZone.OnCollisionEnter` はプレイヤーを `Destroy` するだけで `OnPlayerEliminated` を呼ばない。一方 `CheckPlayersFalling` は `position.y < deathYCoordinate` の判定で同じプレイヤーを `OnPlayerEliminated` + `Destroy` する。KillZone が先に `Destroy` した場合、次フレームで `CheckPlayersFalling` は null 扱いでスキップするため `activePlayers` からの除去が行われない。結果として `survivorCount` が実態より多く計算され、`NextRound` の `survivorCount == 1` 勝利判定に到達せず試合が終わらない。
- なぜ重要か: サドンデス移行の直接トリガーになる。`activePlayers` のカウント誤差が1でも残ると「1人になったら終わり」の判定が機能しなくなり、ラウンドが永遠に終わらなくなる。
- 改善案: `KillZone` から `Destroy` を直接呼ぶのをやめ、`PlayerHealth.OnFallOut()` 経由で通知する。

```csharp
// KillZone.cs
public void OnCollisionEnter(Collision other)
{
    if (other.gameObject.CompareTag("Player"))
    {
        var health = other.gameObject.GetComponent<PlayerHealth>();
        if (health != null) health.OnFallOut(); // OnFallOut 内で Destroy される
        else Destroy(other.gameObject);
    }
}
```

---

### [高] NextRound の survivors 収集で `GetComponent<PlayerHealth>()` の null チェックが欠落

- 場所: `Assets/Miwa/scripts/GameManager_M.cs:580`
- 問題: `p != null` はチェックしているが `p.GetComponent<PlayerHealth>()` の結果が null の場合に `.playerIndex` で `NullReferenceException` が発生する。Bot プレハブに `PlayerHealth` がなく、他の経路で `activePlayers` に紛れ込んだ場合にクラッシュする。
- なぜ重要か: `NextRound` のクラッシュは `TriggerSuddenDeath` が呼ばれず `_qualifiedIndices` が未設定のままシーンリロードに進む。その結果サドンデスで全プレイヤーが `Destroy` される。
- 改善案:

```csharp
foreach (var p in activePlayers)
{
    if (p == null) continue;
    var health = p.GetComponent<PlayerHealth>();
    if (health != null) survivors.Add(health.playerIndex);
}
```

---

### [高] `_lastActiveIndices` が空のまま `TriggerSuddenDeath` に渡されると全プレイヤーが失格になる

- 場所: `Assets/Miwa/scripts/GameManager_M.cs:582`
- 問題: `survivors` が空のとき `_lastActiveIndices` にフォールバックするが、`_lastActiveIndices` はゲーム開始直後など `CheckPlayersFalling` が一度も `currentLiving.Count > 0` で走っていない状態では空のまま。空リストが `TriggerSuddenDeath([])` に渡ると `_qualifiedIndices = []` となり、新シーンで `RegisterPlayer` が全プレイヤーを `Destroy` し誰もいないサドンデスが始まる。
- なぜ重要か: 発生確率は低いが、発生すると試合が完全に詰む。
- 改善案:

```csharp
if (survivors.Count == 0) survivors = _lastActiveIndices;

// 両方空なら全員をサバイバーとして扱う
if (survivors.Count == 0)
{
    for (int idx = 0; idx < playerWins.Length; idx++)
        survivors.Add(idx);
}

TriggerSuddenDeath(survivors);
```

---

### [中] `CheckPlayersFalling` が毎フレーム `GetComponent` を呼んでいる

- 場所: `Assets/Miwa/scripts/GameManager_M.cs:368`
- 問題: `Update` → `CheckPlayersFalling` ループで毎フレーム `player.GetComponent<PlayerHealth>()` を呼んでいる。プレイヤー数が最大4の現状では許容範囲だが、キャッシュなしの `GetComponent` は検索コストが発生する。
- なぜ重要か: 同パターンが `SetAllPlayersControl` など他のホットパスにも広がっており、将来の拡張時にスパイクの原因になりうる。
- 改善案: `PlayerHealth` への参照を `RegisterPlayer` 時に `Dictionary<GameObject, PlayerHealth>` でキャッシュしておく。

---

## 改善アイデア

- **`OnPlayerEliminated` の二重除去防御**: `CheckPlayersFalling` が `playersToEliminate` 収集後に `foreach` で `OnPlayerEliminated` を呼ぶ流れは、同一フレームの外部呼び出しと重なると `activePlayers` が二重操作される恐れがある。`OnPlayerEliminated` に `activePlayers.Contains(p)` チェックを追加することで防御できる。

- **サドンデス移行時のコントローラーズレ（既知バグ）**: `MainGameManger.Awake()` で死亡済みプレイヤーを `PlayerInput.Instantiate` → 即 `Destroy` する流れが Input System のデバイス再割り当てを引き起こす。対応方針は `BUGREPORT_CONTROLLER_SHIFT.txt` に記載済み。

---

## 未確認事項

- `pos.Length`（スポーン位置の数）がシーンによって4未満に設定されているケースがあるか。4人参加時に `pos.Length < 4` だと `IndexOutOfRangeException` が発生する可能性がある。
- サドンデスに使われるシーンが `prot.unity` と他のステージシーンで `MainGameManger` の設定が共通になっているかどうか。

---

## 推奨アクション

1. **KillZone を `PlayerHealth.OnFallOut()` 経由に変更する**（致命的・1ファイルの修正）
2. **`GetComponent<PlayerHealth>()` の null チェックを `GameManager_M.cs:580` に追加する**（高・1行の修正）
3. **`_lastActiveIndices` 空時のフォールバックを `GameManager_M.cs:582` に追加する**（高・数行の修正）
4. **サドンデス移行時のコントローラーズレを `BUGREPORT_CONTROLLER_SHIFT.txt` の方針で修正する**

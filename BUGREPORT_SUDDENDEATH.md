# バグ調査レポート：サドンデスモードの死亡・復活不具合

**調査日**: 2026-05-15  
**報告者**: Claude Code (調査のみ・修正未実施)  
**プログラマーの仮説**: スコアモードの設定がサバイバルモード（サドンデス）に混入している

---

## 症状

1. サドンデスモード中に死んだプレイヤーが復活する
2. サドンデスモード中に落下しても死なない（試合が進まない）

---

## 原因

### ① 落下しても死なない

**`Assets/Scripts/KillZone.cs` line 17–21**

`KillZone` は落下したプレイヤーを `Destroy()` するだけで、GameManager に死亡を通知していない。

```csharp
// 現状：Destroyだけ
public void OnCollisionEnter(Collision other)
{
    if (other.gameObject.CompareTag("Player"))
        Destroy(other.gameObject); // ← 通知なし
}
```

**正しい経路**:  
`PlayerHealth.OnFallOut()` → `GameManager_M.OnPlayerEliminated()` → ラウンド進行

`KillZone` はこの経路を完全にスキップしているため、`activePlayers.Count <= 1` 判定と `NextRound()` が走らず試合が進まない。

参考: `Assets/Miwa/scripts/PlayerHealth.cs` line 20–27 に正しい脱落処理 `OnFallOut()` が用意されている。

---

### ② 復活してしまう

**`Assets/Miwa/scripts/GameManager_M.cs` line 428–430**

落下判定の時点で `CurrentModeState` が `ScoreMode` になっていると `RespawnPlayer()` が呼ばれる分岐に入る。

```csharp
// ScoreModeのときだけ復活処理が走る想定だが…
if (CurrentModeState == Mode.ScoreMode)
    RespawnPlayer(); // ← サドンデス中にここに入っている可能性
```

サドンデスへのモード切り替え（`GameManager_M.cs` line 130 の `ChangeMode(new SuddenDeathMode())`）が落下判定のタイミングまでに反映されていない可能性がある。

---

### ③ スコアモード処理の混入（プログラマー仮説と合致）

**`Assets/Miwa/scripts/GameManager_M.cs` line 413**

`PlayerScoreHandler.HandleDeath()` がモード判定なしで全モードから無条件に呼ばれている。  
`PlayerScoreHandler` はスコアモード専用の処理であり、サバイバル・サドンデスの死亡フローから呼ばれるのは責務の混在。

---

## 確認・修正の候補

| 優先 | 対象 | 内容 |
|------|------|------|
| 高 | `KillZone.cs` | `Destroy()` の前に `PlayerHealth.OnFallOut()` を呼ぶよう修正 |
| 高 | `GameManager_M.cs` line 428 | 落下判定時に `CurrentModeState` をログ出力して `ScoreMode` になっていないか確認 |
| 中 | `GameManager_M.cs` line 413 | `PlayerScoreHandler.HandleDeath()` の呼び出しにモード判定を追加 |
| 中 | サドンデス用Prefab | `PlayerScoreHandler` コンポーネントがアタッチされていないか確認 |

---

## 関係ファイル

- `Assets/Scripts/KillZone.cs`
- `Assets/Miwa/scripts/GameManager_M.cs`
- `Assets/Miwa/scripts/SuddenDeathMode.cs`
- `Assets/Miwa/scripts/PlayerHealth.cs`
- `Assets/Miwa/scripts/ScoreMOdeScripts/PlayerScoreHandler.cs`

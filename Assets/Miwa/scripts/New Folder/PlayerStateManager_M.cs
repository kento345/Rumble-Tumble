using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateModule
{
    private GameManager_M gm;
    // ★ リストからは絶対に要素を削除せず、全員をここに保持し続けます
    private List<GameObject> activePlayers = new List<GameObject>();
    private bool[] isRespawning = new bool[4];
    public List<int> LastActiveIndices { get; private set; } = new List<int>();

    public PlayerStateModule(GameManager_M manager) => gm = manager;

    public void Awake()
    {
        // 蘇生中フラグを完全にクリアして、全員がリスポーンできる状態にする
        System.Array.Clear(isRespawning, 0, isRespawning.Length);
        LastActiveIndices.Clear();
    }

    public void SetAllPlayersControl(bool enabled)
    {
        foreach (var player in GetActivePlayers())
        {
            if (player == null) continue;
            PlayerInputController controller = player.GetComponent<PlayerInputController>();
            if (controller != null) controller.OnMoveStop(enabled);
            BOTController botCon = player.GetComponent<BOTController>();
            if (botCon != null) botCon.enabled = enabled;
        }
    }

    public void RegisterPlayer(GameObject p, int index)
    {
        // フラグの初期化
        isRespawning[index] = false;

        Rigidbody rb = p.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // 新しいシーンの初期スポーン位置へ配置
        if (gm.SpawnPoint != null && index < gm.SpawnPoint.Length && gm.SpawnPoint[index] != null)
        {
            p.transform.position = gm.SpawnPoint[index].position;
        }

        // アニメーションやプレイヤー独自のステートを完全に初期化する
        var pController = p.GetComponent<PlayerController1>();
        if (pController != null) pController.ResetPlayerState();

        // サドンデスモードの時の参加・不参加の切り分け
        bool isQualified = true;
        if (gm.CurrentModeState == GameManager_M.Mode.SuddenDeath)
        {
            if (!GameManager_M._qualifiedIndices.Contains(index))
            {
                isQualified = false;
            }
        }

        if (isQualified)
        {
            p.SetActive(true);
            if (rb != null) rb.isKinematic = false;

            if (!activePlayers.Contains(p)) activePlayers.Add(p);

            // ★【ここを追加！】
            // 参加プレイヤーを目覚めさせた「その瞬間」に、即座に操作・AIをロックする！
            // これにより、GameManagerのカウントダウンが始まる前のわずかな隙間のフライングを完全に防ぎます。
            PlayerInputController controller = p.GetComponent<PlayerInputController>();
            if (controller != null) controller.OnMoveStop(false); // 操作禁止
            BOTController botCon = p.GetComponent<BOTController>();
            if (botCon != null) botCon.enabled = false; // BOTの思考停止

            // サドンデス専用のパワーアップ処理
            if (gm.CurrentModeState == GameManager_M.Mode.SuddenDeath && gm._currentMode is SuddenDeathMode suddenMode)
            {
                suddenMode.PowerUpSinglePlayer(p);
            }
        }
        else
        {
            p.SetActive(false);
            if (activePlayers.Contains(p)) activePlayers.Remove(p);
        }

        // オブジェクトの状態（参加・不参加）が確定した「後」にUIを初期化する
        if (PlayerUIManager.Instance != null)
        {
            PlayerUIManager.Instance.InitializePlayerUI(GameManager_M.playerWins.Length, gm.CurrentModeState == GameManager_M.Mode.ScoreMode);

            if (!isQualified)
            {
                PlayerUIManager.Instance.SetPlayerDead(index);
            }
            else
            {
                PlayerUIManager.Instance.ResetPlayerStatus(index);
            }
        }
    }


    public void CheckPlayersFalling()
    {
        if (gm.CurrentModeState == GameManager_M.Mode.GameOver || gm.isRoundEnding) return;

        // 現在生きているプレイヤーのIDを記録
        List<int> currentLiving = new List<int>();
        foreach (var p in activePlayers)
        {
            if (p != null && p.activeInHierarchy)
            {
                var h = p.GetComponent<PlayerHealth>();
                // もしPlayerHealthがなくても、ループのインデックスや別の方法でIDを補う
                int id = (h != null) ? h.playerIndex : activePlayers.IndexOf(p);
                currentLiving.Add(id);
            }
        }
        if (currentLiving.Count > 0) LastActiveIndices = new List<int>(currentLiving);

        List<GameObject> playersToEliminate = new List<GameObject>();

        for (int i = activePlayers.Count - 1; i >= 0; i--)
        {
            GameObject player = activePlayers[i];
            if (player == null || !player.activeInHierarchy) continue;

            var health = player.GetComponent<PlayerHealth>();
            int pIndex = (health != null) ? health.playerIndex : i;

            if (pIndex >= 0 && pIndex < isRespawning.Length && isRespawning[pIndex]) continue;

            // 死亡高度またはステージ外の判定
            bool isFallen = player.transform.position.y < gm.deathYCoordinate ||
                             player.transform.position.y > gm.upperDeathYCoordinate ||
                             Mathf.Abs(player.transform.position.x) > gm.deathXLimit ||
                             Mathf.Abs(player.transform.position.z) > gm.deathZLimit;

            if (isFallen)
            {
                if (pIndex >= 0 && pIndex < isRespawning.Length) isRespawning[pIndex] = true;

                var scoreHandler = player.GetComponent<PlayerScoreHandler>();
                if (scoreHandler != null) scoreHandler.HandleDeath();

                if (SoundManager.Instance != null) SoundManager.Instance.PlaySE(SoundManager.Instance.groundBreakSE);
                if (PlayerUIManager.Instance != null) PlayerUIManager.Instance.SetPlayerDead(pIndex);

                if (gm.CurrentModeState == GameManager_M.Mode.ScoreMode)
                {
                    gm.StartCoroutine(RespawnPlayer(player, pIndex));
                }
                else
                {
                    playersToEliminate.Add(player);
                }
            }
        }

        // 脱落したプレイヤー（BOT含む）の処理
        foreach (var p in playersToEliminate)
        {
            // コルーチンを安全に止める
            MonoBehaviour[] components = p.GetComponents<MonoBehaviour>();
            foreach (var comp in components)
            {
                if (comp != null) comp.StopAllCoroutines();
            }

            // ★【確実化】何があろうとオブジェクトを非アクティブにして画面から消す
            p.SetActive(false);

            // GameManagerに通知
            gm.OnPlayerEliminated(p);
        }
    }

    private IEnumerator RespawnPlayer(GameObject player, int playerIndex)
    {
        player.SetActive(false);
        Rigidbody rb = player.GetComponent<Rigidbody>();

        yield return new WaitForSeconds(gm.Spawntime);

        Vector3 spawnPosition = Vector3.zero;
        if (gm.SpawnPoint != null && gm.SpawnPoint.Length > 0)
        {
            int targetIndex = (playerIndex < gm.SpawnPoint.Length) ? playerIndex : 0;
            spawnPosition = gm.SpawnPoint[targetIndex].position;
        }

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        player.transform.position = spawnPosition;
        player.SetActive(true);

        gm.Score.OnPlayerRespawn(playerIndex, spawnPosition);

        var pController = player.GetComponent<PlayerController1>();
        if (pController != null) pController.ResetPlayerState();

        if (PlayerUIManager.Instance != null) PlayerUIManager.Instance.ResetPlayerStatus(playerIndex);

        yield return new WaitForFixedUpdate();

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        isRespawning[playerIndex] = false;
    }

    // ★【修正】現在「アクティブ（電球ON）」なプレイヤーだけを抽出して返す
    public List<GameObject> GetActivePlayers()
    {
        List<GameObject> living = new List<GameObject>();
        foreach (var p in activePlayers)
        {
            if (p != null && p.activeInHierarchy)
            {
                living.Add(p);
            }
        }
        return living;
    }

    public int GetActivePlayersCount()
    {
        int count = 0;
        foreach (var p in activePlayers)
        {
            if (p != null && p.activeInHierarchy) count++;
        }
        return count;
    }

    // ★【新設】生存・死亡問わず、このシーンにいる全員のIDを返す（サドンデス用）
    public List<int> GetAllPlayerIndices()
    {
        List<int> indices = new List<int>();
        foreach (var p in activePlayers)
        {
            if (p != null)
            {
                var h = p.GetComponent<PlayerHealth>();
                if (h != null) indices.Add(h.playerIndex);
            }
        }
        return indices;
    }
}
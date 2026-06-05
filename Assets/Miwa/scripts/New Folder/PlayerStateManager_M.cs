using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class PlayerStateModule
{
    private GameManager_M gm;
    private List<GameObject> activePlayers = new List<GameObject>();
    private bool[] isRespawning = new bool[4];
    public List<int> LastActiveIndices { get; private set; } = new List<int>();

    public PlayerStateModule(GameManager_M manager) => gm = manager;

    public void Awake()
    {
        System.Array.Clear(isRespawning, 0, isRespawning.Length);
    }

    public void SetAllPlayersControl(bool enabled)
    {
        foreach (var player in PlayerDataHolder.Instance.players)
        {
            if (player == null) continue;
            PlayerInputController input = player.GetComponent<PlayerInputController>();
            if (input != null) input.OnMoveStop(enabled);

            var rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                if (!enabled)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    rb.isKinematic = true;
                }
                else rb.isKinematic = false;
            }

           /* var moveScript = player.GetComponent<MoveController>();
            if (moveScript != null) moveScript.enabled = enabled;*/
        }
        /* foreach (var player in GetActivePlayers())
         {
             if (player == null) continue;
             var input = player.GetComponent<UnityEngine.InputSystem.PlayerInput>();
             if (input != null) input.enabled = enabled;

             var rb = player.GetComponent<Rigidbody>();
             if (rb != null)
             {
                 if (!enabled)
                 {
                     rb.linearVelocity = Vector3.zero;
                     rb.angularVelocity = Vector3.zero;
                     rb.isKinematic = true;
                 }
                 else rb.isKinematic = false;
             }

             var moveScript = player.GetComponent<MoveController>();
             if (moveScript != null) moveScript.enabled = enabled;
         }*/
    }

    public void RegisterPlayer(GameObject p, int index)
    {
        // ★ サドンデス時、対象外のプレイヤーをゲームから除外する元のロジックを完全復元
        if (gm.CurrentModeState == GameManager_M.Mode.SuddenDeath && !GameManager_M._qualifiedIndices.Contains(index))
        {
            activePlayers.Remove(p);
            Object.Destroy(p);
            return;
        }

        if (!activePlayers.Contains(p)) activePlayers.Add(p);

        // ★ サドンデスモードが有効な場合、プレイヤーを強化するロジックを完全復元
        if (gm.CurrentModeState == GameManager_M.Mode.SuddenDeath && gm._currentMode is SuddenDeathMode suddenMode)
        {
            suddenMode.PowerUpSinglePlayer(p);
        }

        if (PlayerUIManager.Instance != null)
        {
            PlayerUIManager.Instance.InitializePlayerUI(GameManager_M.playerWins.Length, gm.CurrentModeState == GameManager_M.Mode.ScoreMode);
        }
    }

    public void CheckPlayersFalling()
    {
        if (gm.CurrentModeState == GameManager_M.Mode.GameOver || gm.isRoundEnding) return;

        List<int> currentLiving = new List<int>();
        foreach (var p in activePlayers)
        {
            if (p != null)
            {
                var h = p.GetComponent<PlayerHealth>();
                if (h != null) currentLiving.Add(h.playerIndex);
            }
        }
        if (currentLiving.Count > 0) LastActiveIndices = new List<int>(currentLiving);

        List<GameObject> playersToEliminate = new List<GameObject>();

        for (int i = activePlayers.Count - 1; i >= 0; i--)
        {
            GameObject player = activePlayers[i];
            if (player == null) continue;

            var health = player.GetComponent<PlayerHealth>();
            if (health == null) continue;
            int pIndex = health.playerIndex;

            if (isRespawning[pIndex]) continue;

            Vector3 pos = player.transform.position;
            bool isOutY = pos.y < gm.deathYCoordinate || pos.y > gm.upperDeathYCoordinate;
            bool isOutX = Mathf.Abs(pos.x) > gm.deathXLimit;
            bool isOutZ = Mathf.Abs(pos.z) > gm.deathZLimit;

            if (isOutY || isOutX || isOutZ)
            {
                isRespawning[pIndex] = true;

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

        foreach (var p in playersToEliminate)
        {
            activePlayers.Remove(p);
            gm.OnPlayerEliminated(p); // GameManager側の集計ロジックを走らせる
            Object.Destroy(p);
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

    public List<GameObject> GetActivePlayers() { activePlayers.RemoveAll(p => p == null); return activePlayers; }
    public int GetActivePlayersCount()
    {
        int count = 0;
        foreach (var p in activePlayers) if (p != null) count++;
        return count;
    }
}
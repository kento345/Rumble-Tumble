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
        foreach (var player in GetActivePlayers())
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
        }
    }

    public void RegisterPlayer(GameObject p, int index)
    {
        // ★【修正ポイント】サドンデス時、対象外のプレイヤーをデストロイせず、非アクティブにして除外する
        if (gm.CurrentModeState == GameManager_M.Mode.SuddenDeath && !GameManager_M._qualifiedIndices.Contains(index))
        {
            activePlayers.Remove(p);
            p.SetActive(false); // Destroyから変更
            return;
        }

        if (!activePlayers.Contains(p)) activePlayers.Add(p);

        // ★ サドンデスモードが有効な場合、プレイヤーを強化するロジック
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
            if (p != null && p.activeInHierarchy) // 生きてる（アクティブな）プレイヤーのみ
            {
                var h = p.GetComponent<PlayerHealth>();
                if (h != null) currentLiving.Add(h.playerIndex);
            }
        }
        if (currentLiving.Count > 0) LastActiveIndices = new List<int>(currentLiving);

        List<GameObject> playersToEliminate = new List<GameObject>();

        Camera mainCam = Camera.main;
        if(mainCam ==null) return; // カメラがない場合は落下判定をスキップ

        for (int i = activePlayers.Count - 1; i >= 0; i--)
        {
            GameObject player = activePlayers[i];
            if (player == null) continue;

            var health = player.GetComponent<PlayerHealth>();
            if (health == null) continue;
            int pIndex = health.playerIndex;

            if (isRespawning[pIndex]) continue;

            Vector3 viewportPos = mainCam.WorldToViewportPoint(player.transform.position);
            bool isOutX = viewportPos.x < 0f || viewportPos.x > 1f;
            bool isOutY = viewportPos.y < 0f || viewportPos.y > 1f;
            bool isOutZ = viewportPos.z < 0f;

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

        //落下して脱落したプレイヤーもデストロイせず非アクティブ化
        foreach (var p in playersToEliminate)
        {
            activePlayers.Remove(p);
            gm.OnPlayerEliminated(p);
            p.SetActive(false);
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

    //非アクティブになったプレイヤーも除外して返すように変更
    public List<GameObject> GetActivePlayers()
    {
        activePlayers.RemoveAll(p => p == null || !p.activeInHierarchy);
        return activePlayers;
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
}
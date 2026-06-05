using UnityEngine;
using System.Collections;

public class ScoreModule
{
    private GameManager_M gm;
    private int[] pendingDropCounts = new int[4];

    public ScoreModule(GameManager_M manager) => gm = manager;

    public void AddScore(int playerIndex, int amount)
    {
        if (playerIndex < 0 || playerIndex >= GameManager_M.currentScores.Length) return;

        if (gm.CurrentModeState == GameManager_M.Mode.ScoreMode)
        {
            if (amount < 0) pendingDropCounts[playerIndex] += Mathf.Abs(amount);

            GameManager_M.currentScores[playerIndex] = Mathf.Max(0, GameManager_M.currentScores[playerIndex] + amount);

            if (PlayerUIManager.Instance != null)
            {
                PlayerUIManager.Instance.UpdatePlayerScore(playerIndex, GameManager_M.currentScores[playerIndex]);
            }
        }
    }

    public void OnPlayerRespawn(int playerIndex, Vector3 spawnPosition)
    {
        int countToDrop = pendingDropCounts[playerIndex];
        if (countToDrop > 0)
        {
            SpawnScoreItems(spawnPosition + Vector3.up * 1.5f, countToDrop);
            pendingDropCounts[playerIndex] = 0;
        }
    }

    public void DropScore(Vector3 deathPosition)
    {
        for (int i = 0; i < gm.dropAmountPerDeath; i++)
        {
            if (gm.scoreItemPrefab == null) break;

            GameObject item = Object.Instantiate(gm.scoreItemPrefab, deathPosition + Vector3.up, Quaternion.identity);
            ScoreItem script = item.GetComponent<ScoreItem>();

            if (script != null)
            {
                Vector3 randomDir = new Vector3(Random.Range(-1f, 1f), 1.5f, Random.Range(-1f, 1f)).normalized;
                script.Launch(randomDir, Random.Range(3f, 7f));
            }
        }
    }

    public void SpawnScoreItems(Vector3 position, int count)
    {
        gm.StartCoroutine(SpawnItemsRoutine(position, count));
    }

    private IEnumerator SpawnItemsRoutine(Vector3 pos, int count)
    {
        Vector3 spawnBasePos = pos;
        spawnBasePos.y = 1.0f;

        for (int i = 0; i < count; i++)
        {
            if (gm.scoreItemPrefab == null) break;

            GameObject item = Object.Instantiate(gm.scoreItemPrefab, spawnBasePos + Random.insideUnitSphere * 0.5f, Quaternion.identity);
            ScoreItem script = item.GetComponent<ScoreItem>();
            if (script != null)
            {
                Vector3 dir = new Vector3(Random.Range(-1f, 1f), 2f, Random.Range(-1f, 1f)).normalized;
                script.Launch(dir, 5f);
            }
            yield return new WaitForSeconds(0.05f);
        }
    }

    public void CalculateScoreWinner()
    {
        int maxScore = -1;
        int winnerIndex = -1;
        bool isDraw = false;

        for (int i = 0; i < GameManager_M.currentScores.Length; i++)
        {
            if (GameManager_M.currentScores[i] > maxScore) { maxScore = GameManager_M.currentScores[i]; winnerIndex = i; isDraw = false; }
            else if (GameManager_M.currentScores[i] == maxScore && maxScore != -1) { isDraw = true; }
        }

        if (!isDraw && winnerIndex != -1) GameManager_M.playerWins[winnerIndex]++;
    }

    public void ResetScores()
    {
        System.Array.Clear(GameManager_M.currentScores, 0, GameManager_M.currentScores.Length);
        System.Array.Clear(pendingDropCounts, 0, pendingDropCounts.Length);

        if (PlayerUIManager.Instance != null)
        {
            for (int i = 0; i < GameManager_M.currentScores.Length; i++) PlayerUIManager.Instance.UpdatePlayerScore(i, 0);
        }
    }
}
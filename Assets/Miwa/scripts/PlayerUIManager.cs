using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class PlayerUIManager : MonoBehaviour
{
    public static PlayerUIManager Instance { get; private set; }

    [Header("UI設定")]
    public Transform uiContainer;
    public GameObject statusUIPrefab;

    [Header("【各プレイヤー用】生存・死亡画像設定")]
    public Sprite[] aliveSprites;
    public Sprite[] deadSprites; 

    private List<PlayerStatusUI> spawnedUIs = new List<PlayerStatusUI>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void InitializePlayerUI(int playerCount, bool isScoreMode)
    {
        foreach (var ui in spawnedUIs) if (ui != null) Destroy(ui.gameObject);
        spawnedUIs.Clear();

        if (statusUIPrefab == null || uiContainer == null) return;

        for (int i = 0; i < playerCount; i++)
        {
            GameObject uiObj = Instantiate(statusUIPrefab, uiContainer);
            PlayerStatusUI statusUI = uiObj.GetComponent<PlayerStatusUI>();

            if (statusUI != null)
            {
                Sprite myAlive = (i < aliveSprites.Length) ? aliveSprites[i] : null;
                Sprite myDead = (i < deadSprites.Length) ? deadSprites[i] : null;
                
                int intialValue =isScoreMode ? GameManager_M.currentScores[i]:GameManager_M.playerWins[i];
                statusUI.SetupUI(intialValue, myAlive, myDead, isScoreMode);

                spawnedUIs.Add(statusUI);
            }
        }
    }

    public void ResetPlayerStatus(int index)
    {
        if (index >= 0 && index < spawnedUIs.Count)
        {
            spawnedUIs[index].SetEliminated(false); // 生存スプライトに戻す
        }
    }

    public void UpdatePlayerUI(int playerIndex, bool isScoreMode)
    {
        // エラー修正1: 'playerStatusUIs' を 'spawnedUIs' に変更
        if (spawnedUIs == null || playerIndex >= spawnedUIs.Count)return;

        var ui = spawnedUIs[playerIndex];
        if (ui != null)
        {
            // エラー修正2: GameManager_M.Instance.playerWins ではなく GameManager_M.playerWins (static参照)
            int value = isScoreMode ?
                GameManager_M.currentScores[playerIndex] :
                GameManager_M.playerWins[playerIndex];

            // UIの見た目と値を更新
            ui.SetupUI(value, null, null, isScoreMode);
        }
    }


    public void SetPlayerDead(int index) => spawnedUIs[index].SetEliminated(true);

    public void UpdatePlayerScore(int index, int score) => spawnedUIs[index].UpdateScore(score);

    public void UpdatePlayerStars(int index, int stars) => spawnedUIs[index].UpdateStars(stars);

    public void ResetAllUIState()
    {
        foreach (var ui in spawnedUIs)
        {
            ui.SetEliminated(false);
        }
    }
}
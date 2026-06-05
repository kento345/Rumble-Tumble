using UnityEngine;
using static GameMode;

public class GameOverMode : IGameMode
{
    private string _winnerName;

    public GameOverMode(string winnerName = "")
    {
        _winnerName = winnerName;
    }

    public void OnEnter()
    {
        Time.timeScale = 1.0f;

        string winnerMessage = string.IsNullOrEmpty(_winnerName) ? "DRAW GAME" : _winnerName + " WIN!";
        if (GameManager_M.Instance != null)
        {
            GameManager_M.Instance.ShowResultUI(winnerMessage);
            GameManager_M.Instance.SetAllPlayersControl(false); // プレイヤーの操作を止める
        }
    }

    public void OnUpdate() { }

    public void OnExit()
    {
        if (GameManager_M.Instance != null && GameManager_M.Instance.roundTextUI != null)
            GameManager_M.Instance.roundTextUI.gameObject.SetActive(false);
    }
}
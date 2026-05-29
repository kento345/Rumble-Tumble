using UnityEngine;
using UnityEngine.UI;
using static GameMode;

public class GameOverMode : IGameMode
{
    private string _winnerName;

    // コンストラクタで勝者の名前を受け取るであります！
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
            GameManager_M.Instance.SetAllPlayersControl(false);
        }
    }

    public void OnUpdate() { }

    public void OnExit()
    {
        if (GameManager_M.Instance.roundTextUI != null)
            GameManager_M.Instance.roundTextUI.gameObject.SetActive(false);
    }
}
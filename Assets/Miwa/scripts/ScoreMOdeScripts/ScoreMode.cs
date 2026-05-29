using UnityEngine;
using UnityEngine.UI;
using static GameMode;

public class ScoreMode : IGameMode
{
    private Text _timerText;
    private float _timeLimit;

    // ここで変数を定義（これが「存在しません」というエラーの解決策）
    private float remainingTime;
    private bool isTimerActive;

    public ScoreMode(Text timerText, float timeLimit)
    {
        _timerText = timerText;
        _timeLimit = timeLimit;
    }

    public void OnEnter()
    {
        remainingTime = _timeLimit;
        isTimerActive = true;
        Debug.Log("Score Mode Started!");
    }

    public void OnUpdate()
    {
        if (!isTimerActive) return;

        // タイマーを減らす
        remainingTime -= Time.deltaTime;

        // UIの更新（分:秒 の形式など）
        if (_timerText != null)
        {
            int seconds = Mathf.CeilToInt(remainingTime);
            _timerText.text = seconds.ToString();
        }

        // 0秒になった時の判定
        if (remainingTime <= 0)
        {
            remainingTime = 0;
            isTimerActive = false;

            // GameManagerにタイムアップを知らせる
            GameManager_M.Instance.NextRound(true);
        }
    }

    public void OnExit()
    {
        isTimerActive = false;
    }
}
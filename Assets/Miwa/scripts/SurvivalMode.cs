using UnityEngine;
using UnityEngine.UI;
using static GameMode;

public class SurvivalMode : IGameMode
{
    private float currentTime;
    private Text timerText;
    public bool isTimerActive = false;

    public SurvivalMode(Text uiText, float timeLimit)
    {
        timerText = uiText;
        currentTime = timeLimit;
    }

    public void OnEnter()
    {
        isTimerActive = true;
    }

    public void OnUpdate()
    {
        if (!isTimerActive) return;

        // 1. 時間の計算（GameManagerのUpdateから呼ばれる）
        currentTime -= Time.deltaTime;

        // 2. 秒数を画面に出す
        if (timerText != null)
        {
            timerText.text = Mathf.Max(0, currentTime).ToString("F1");
        }

        // 3. 時間切れ判定
        if (currentTime <= 0)
        {
            isTimerActive = false;
            // GameManagerに「時間切れだよ」と伝える
            GameManager_M.Instance.TimeExpiredForSurvival();
        }
    }

    public void OnExit()
    {
        isTimerActive = false;
    }
}
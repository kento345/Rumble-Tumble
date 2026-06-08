using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class UIModule
{
    private GameManager_M gm;

    public UIModule(GameManager_M manager) => gm = manager;

    public void UpdateRoundDisplay()
    {
        if (gm.roundTextUI != null)
        {
            if (GameManager_M.selectedGameMode == GameManager_M.Mode.ScoreMode)
            {
                gm.roundTextUI.gameObject.SetActive(false);
                return;
            }
            gm.roundTextUI.text = (gm.CurrentModeState == GameManager_M.Mode.SuddenDeath) ? "Round" + GameManager_M.CurrentRound : "ラウンド" + GameManager_M.CurrentRound;
            gm.roundTextUI.gameObject.SetActive(true);
        }
    }

    public IEnumerator InitializeUIWithDelay()
    {
        yield return null;
        if (PlayerUIManager.Instance != null)
        {
            bool isScoreMode = (gm.CurrentModeState == GameManager_M.Mode.ScoreMode);
            PlayerUIManager.Instance.InitializePlayerUI(GameManager_M.playerWins.Length, isScoreMode);

            if (gm.CurrentModeState == GameManager_M.Mode.SuddenDeath)
            {
                for (int i = 0; i < GameManager_M.playerWins.Length; i++)
                {
                    if (!GameManager_M._qualifiedIndices.Contains(i)) PlayerUIManager.Instance.SetPlayerDead(i);
                }
            }
        }
        for (int i = 0; i < GameManager_M.playerWins.Length; i++)
        {
            if (gm.CurrentModeState == GameManager_M.Mode.ScoreMode)
                PlayerUIManager.Instance.UpdatePlayerScore(i, GameManager_M.currentScores[i]);
            else
                PlayerUIManager.Instance.UpdatePlayerStars(i, GameManager_M.playerWins[i]);
        }
    }

    public IEnumerator StartCountdown()
    {
        yield return null;
        gm.isGameStarted = false;

        // ★【追加】カウントダウン開始前に全員の操作・BOTの思考を止める
        gm.PlayerState.SetAllPlayersControl(false);

        // ★【追加】サドンデス突入時などに、前の慣性で滑っていかないよう物理速度を完全にゼロにする
        foreach (var player in gm.GetActivePlayers())
        {
            if (player == null) continue;
            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        // ここで初めてサドンデスフラグをリセット
        GameManager_M._isSuddenDeathNext = false;

        int count = 3;
        while (count > 0)
        {
            if (gm.CountdownUI != null)
            {
                gm.CountdownUI.text = count.ToString();
                gm.CountdownUI.color = (count <= 1) ? Color.red : Color.white;
                gm.StartCoroutine(GhostTrailEffect(gm.CountdownUI));

                gm.PlayerState.SetAllPlayersControl(false);

                yield return new WaitForSeconds(1.0f);
                count--;
            }
        }

        if (gm.CountdownUI != null)
        {
            gm.CountdownUI.text = "Fight!!";
            gm.CountdownUI.color = Color.yellow;
            gm.StartCoroutine(GhostTrailEffect(gm.CountdownUI));

            if (SoundManager.Instance != null) SoundManager.Instance.PlaySE(SoundManager.Instance.gameStartGongSE);
            gm.isGameStarted = true;

            gm.PlayerState.SetAllPlayersControl(true);

            yield return new WaitForSeconds(1.0f);
            gm.CountdownUI.text = "";
        }
    }

    private IEnumerator GhostTrailEffect(Text uiText)
    {
        Text ghost = Object.Instantiate(uiText, uiText.transform.parent);
        ghost.transform.localPosition = uiText.transform.localPosition;

        float duration = 0.6f;
        float elapsed = 0f;
        Vector3 startScale = Vector3.one;
        Vector3 endScale = new Vector3(3.0f, 3.0f, 1f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            ghost.transform.localScale = Vector3.Lerp(startScale, endScale, t);
            ghost.color = new Color(uiText.color.r, uiText.color.g, uiText.color.b, 1f - t);
            yield return null;
        }
        Object.Destroy(ghost.gameObject);
    }

    public IEnumerator WaitAndShowResult()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.StopBGM();
            SoundManager.Instance.PlayBGM(SoundManager.Instance.resultBGM);
        }

        if (PlayerUIManager.Instance != null)
        {
            for (int i = 0; i < GameManager_M.playerWins.Length; i++)
            {
                PlayerUIManager.Instance.UpdatePlayerScore(i, GameManager_M.playerWins[i]);
            }
        }
        yield return new WaitForSeconds(1.0f);

        string finalwinner = (gm.CurrentModeState == GameManager_M.Mode.ScoreMode) ? GetScoreWinnerName() : GetWinnerName();
        gm.ChangeMode(new GameOverMode(finalwinner));
    }

    public void ShowResultUI(string resultText)
    {
        if (gm.resultCanvas != null) gm.resultCanvas.SetActive(true);

        var pm = Object.FindFirstObjectByType<PauseManager>();
        if (pm != null && pm.pausePanel != null) pm.pausePanel.SetActive(false);

        if (gm.resultBlurVolume != null) gm.resultBlurVolume.SetActive(true);

        if (gm.winnerNameTextUI != null)
        {
            gm.winnerNameTextUI.text = resultText;
            gm.winnerNameTextUI.gameObject.SetActive(true);
        }

        if (gm.resultTextUI != null)
        {
            gm.resultTextUI.text = "Result";
            gm.resultTextUI.gameObject.SetActive(true);
        }

        if (gm.resultRibbon != null)
        {
            gm.resultRibbon.gameObject.SetActive(true);
            gm.StartCoroutine(AnimateButtonsSwipe());
        }

        Button firstButton = gm.resultCanvas.GetComponentInChildren<Button>();
        if (firstButton != null) firstButton.Select();
    }

    private IEnumerator AnimateButtonsSwipe()
    {
        if (gm.resultRibbon == null) yield break;

        Vector2 endPos = Vector2.zero;
        Vector2 startPos = new Vector2(2000, 0);
        gm.resultRibbon.anchoredPosition = startPos;

        float duration = 0.3f;
        float elapsed = 0f;
        Time.timeScale = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            t = 1f - Mathf.Pow(1f - t, 5f);
            gm.resultRibbon.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }
        gm.resultRibbon.anchoredPosition = endPos;
    }

    public string GetWinnerName()
    {
        List<string> winners = new List<string>();
        for (int i = 0; i < GameManager_M.playerWins.Length; i++)
        {
            if (GameManager_M.playerWins[i] >= 3) winners.Add("Player " + (i + 1));
        }

        if (winners.Count == 0)
        {
            int maxWins = 0;
            for (int i = 0; i < GameManager_M.playerWins.Length; i++)
                if (GameManager_M.playerWins[i] > maxWins) maxWins = GameManager_M.playerWins[i];

            for (int i = 0; i < GameManager_M.playerWins.Length; i++)
                if (GameManager_M.playerWins[i] == maxWins) winners.Add("Player " + (i + 1));
        }
        return string.Join(" & ", winners);
    }

    public string GetScoreWinnerName()
    {
        int maxScore = -1;
        List<string> winners = new List<string>();

        for (int i = 0; i < GameManager_M.currentScores.Length; i++)
        {
            if (GameManager_M.currentScores[i] > maxScore) maxScore = GameManager_M.currentScores[i];
        }

        for (int i = 0; i < GameManager_M.currentScores.Length; i++)
        {
            if (GameManager_M.currentScores[i] == maxScore && maxScore != -1) winners.Add("Player " + (i + 1));
        }

        if (winners.Count == 0) return "No Winner";
        return string.Join(" & ", winners);
    }
}

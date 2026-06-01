using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using static GameMode;
using System.Collections;

public class GameManager_M : MonoBehaviour
{
    public static GameManager_M Instance { get; private set; }

    private int[] pendingDropCounts = new int[4];

    // --- 静的変数 ---
    private static bool _isSuddenDeathNext = false;
    private static List<int> _qualifiedIndices = new List<int>();
    public static int CurrentRound = 1;
    public static int[] playerWins = new int[4];
    public static Mode selectedGameMode = Mode.Survival;
    
    private bool[] isRespawning = new bool[4];

    private bool isRoundEnding = false;

    [Header("【デバッグ用】直接シーン再生時のモード指定")]
    public bool useDebugMode = true; 
    public Mode debugGameMode = Mode.ScoreMode;

    [Header("タイマー設定")]
    public float scoreModeTimeLimit = 40f; 

    [Header("UI設定")]
    public Text timerTextUI;
    public GameObject resultCanvas;
    public Text CountdownUI;

    [Header("ラウンド表示用")]
    public Text roundTextUI;

    [Header("リザルト表示用")]
    public Text resultTextUI;
    public Text winnerNameTextUI;

    [Header("サドンデス")]
    public GameObject suddenDeathUI;

    [Header("ゲーム設定")]
    public float survivalTimeLimit = 20.0f;
    public float deathYCoordinate = -10.0f;
    public float upperDeathYCoordinate = 20.0f;

    [Header("リザルト演出用")]
    public GameObject ResultCanvas;      
    public RectTransform resultRibbon;    
    public GameObject resultBlurVolume;  

    [Header("スコア")]
    public Transform[] SpawnPoint;
    public float Spawntime = 3.0f;
    public static int[] currentScores = new int[4];

    [Header("スコアばらまき設定")]
    public GameObject scoreItemPrefab; 
    public int dropAmountPerDeath = 1;  

    public enum Mode { Survival, SuddenDeath, ScoreMode, GameOver }
    public Mode CurrentModeState;

    private IGameMode _currentMode;
    [SerializeField] private List<GameObject> activePlayers = new List<GameObject>();

    public float suddenDeathKnockbackMultiplier = 2.0f;
    public float currentKnockbackMultiplier = 1.0f;
    public float suddenDeathSpeedMultiplier = 10.0f;

    private List<int> _lastActiveIndices = new List<int>();

    private GameObject join;
    private bool isGameStarted = false;
    public bool IsGameStartedProperty => isGameStarted;

    void Awake()
    {
        if (CurrentRound == 1 && !_isSuddenDeathNext)
        {
            for (int i = 0; i < playerWins.Length; i++)
            {
                playerWins[i] = 0;
            }
        }

        for (int i = 0; i < pendingDropCounts.Length; i++)
        {
            pendingDropCounts[i] = 0;
        }

        AudioListener.pause = false;
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        Time.timeScale = 1.0f;
        isRoundEnding = false;

        if (resultCanvas != null) resultCanvas.SetActive(false);
        if (timerTextUI != null) timerTextUI.gameObject.SetActive(true);

        UpdateRoundDisplay();
        CurrentModeState = selectedGameMode;

        if (SoundManager.Instance != null)
        {
            if (_isSuddenDeathNext)
                SoundManager.Instance.PlayBGM(SoundManager.Instance.suddenDeathBGM);
            else
                SoundManager.Instance.PlayBGM(SoundManager.Instance.normalBattleBGM);
        }

        if (_isSuddenDeathNext)
        {
            ChangeMode(new SuddenDeathMode());
            _isSuddenDeathNext = false;
        }
        else
        {
            _qualifiedIndices.Clear();
            if (CurrentModeState == Mode.ScoreMode)
            {
                ChangeMode(new ScoreMode(timerTextUI, scoreModeTimeLimit));
            }
            else
            {
                ChangeMode(new SurvivalMode(timerTextUI, survivalTimeLimit));
            }
        }

        join = GameObject.Find("JoinedManager");
        StartCoroutine(InitializeUIWithDelay());
        SetupUIForMode();

        StartCoroutine(StartCountdown());
    }

    private IEnumerator StartCountdown()
    {
        yield return null;

        isGameStarted = false;
        SetAllPlayersControl(false);
        if (_currentMode is SurvivalMode survival) survival.isTimerActive = false;

        int count = 3;
        while (count > 0)
        {
            if (CountdownUI != null)
            {
                CountdownUI.text = count.ToString();
                CountdownUI.color = (count <= 1) ? Color.red : Color.white;
                StartCoroutine(GhostTrailEffect(CountdownUI));
                yield return new WaitForSeconds(1.0f);
                count--;
            }
        }

        if (CountdownUI != null)
        {
            CountdownUI.text = "Fight!!";
            CountdownUI.color = Color.yellow;
            StartCoroutine(GhostTrailEffect(CountdownUI));

            SoundManager.Instance.PlaySE(SoundManager.Instance.gameStartGongSE);
            isGameStarted = true;
            SetAllPlayersControl(true);
            if (_currentMode is SurvivalMode survivalstart) survivalstart.isTimerActive = true;

            yield return new WaitForSeconds(1.0f);
            CountdownUI.text = "";
        }
    }

    private IEnumerator GhostTrailEffect(Text uiText)
    {
        Text ghost = Instantiate(uiText, uiText.transform.parent);
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
        Destroy(ghost.gameObject);
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
                else
                {
                    rb.isKinematic = false;
                }
            }

            var moveScrit = player.GetComponent<MoveController>();
            if (moveScrit != null) moveScrit.enabled = enabled;
        }
    }

    private IEnumerator InitializeUIWithDelay()
    {
        yield return null;

        if (PlayerUIManager.Instance != null)
        {
            bool isScoreMode = (CurrentModeState == Mode.ScoreMode);
            PlayerUIManager.Instance.InitializePlayerUI(playerWins.Length, CurrentModeState == Mode.ScoreMode);

            if (CurrentModeState == Mode.SuddenDeath)
            {
                for (int i = 0; i < playerWins.Length; i++)
                {
                    if (!_qualifiedIndices.Contains(i))
                    {
                        PlayerUIManager.Instance.SetPlayerDead(i);
                    }
                }
            }
        }
        for (int i = 0; i < playerWins.Length; i++)
        {
            if (CurrentModeState == Mode.ScoreMode)
                PlayerUIManager.Instance.UpdatePlayerScore(i, currentScores[i]);
            else
                PlayerUIManager.Instance.UpdatePlayerStars(i, playerWins[i]);
        }
    }

    public void AddScore(int playerIndex, int amount)
    {
        if (playerIndex < 0 || playerIndex >= currentScores.Length) return;

        if (CurrentModeState == Mode.ScoreMode)
        {
            if (amount < 0)
            {
                pendingDropCounts[playerIndex] += Mathf.Abs(amount);
            }

            currentScores[playerIndex] = Mathf.Max(0, currentScores[playerIndex] + amount);

            if (PlayerUIManager.Instance != null)
            {
                PlayerUIManager.Instance.UpdatePlayerScore(playerIndex, currentScores[playerIndex]);
            }
        }
    }

    void Update()
    {
        if (!isGameStarted || CurrentModeState == Mode.GameOver) return;

        if (_currentMode != null) _currentMode.OnUpdate();
        CheckPlayersFalling();
    }

    public void UpdateRoundDisplay()
    {
        if (roundTextUI != null)
        {
            if (selectedGameMode == Mode.ScoreMode)
            {
                roundTextUI.gameObject.SetActive(false);
                return;
            }

            if (CurrentModeState == Mode.SuddenDeath)
            {
                roundTextUI.text="Round"+CurrentRound;
            }
            else
            {
                roundTextUI.text ="ラウンド"+CurrentRound;
            }
            roundTextUI.gameObject.SetActive(true);
        }
    }

    public void RegisterPlayer(GameObject p, int index)
    {
        if (CurrentModeState == Mode.SuddenDeath && !_qualifiedIndices.Contains(index))
        {
            activePlayers.Remove(p);
            Destroy(p);
            return;
        }
        if (!activePlayers.Contains(p))
        {
            activePlayers.Add(p);
        }

        if (CurrentModeState == Mode.SuddenDeath && _currentMode is SuddenDeathMode suddenMode){ suddenMode.PowerUpSinglePlayer(p);}
            if (PlayerUIManager.Instance != null)
            {
                PlayerUIManager.Instance.InitializePlayerUI(playerWins.Length, CurrentModeState == Mode.ScoreMode);
            }
    }

    public void OnPlayerEliminated(GameObject eliminatedPlayer)
    {
        if (isRoundEnding) return;

        if (activePlayers.Contains(eliminatedPlayer)) activePlayers.Remove(eliminatedPlayer);
        
        activePlayers.RemoveAll(p => p == null);

        if (activePlayers.Count <= 1)
        {
            NextRound();
        }
    }

    private void CheckPlayersFalling()
    {
        if (CurrentModeState == Mode.GameOver || isRoundEnding) return;

        List<int> currentLiving = new List<int>();
        foreach (var p in activePlayers)
        {
            if (p != null)
            {
                var h = p.GetComponent<PlayerHealth>();
                if (h != null) currentLiving.Add(h.playerIndex);
            }
        }
        if (currentLiving.Count > 0) _lastActiveIndices = new List<int>(currentLiving);

        List<GameObject> playersToEliminate = new List<GameObject>();

        for (int i = activePlayers.Count - 1; i >= 0; i--)
        {
            GameObject player = activePlayers[i];
            if (player == null) { continue; } 

            var health = player.GetComponent<PlayerHealth>();
            if (health == null) continue;
            int pIndex = health.playerIndex;

            if (isRespawning[pIndex]) continue;

            if (player.transform.position.y < deathYCoordinate || player.transform.position.y > upperDeathYCoordinate)
            {
                isRespawning[pIndex] = true;

                var scoreHandler = player.GetComponent<PlayerScoreHandler>();
                if (scoreHandler != null)
                {
                    scoreHandler.HandleDeath();
                }

                if (SoundManager.Instance != null)
                    SoundManager.Instance.PlaySE(SoundManager.Instance.groundBreakSE);

                if (PlayerUIManager.Instance != null)
                    PlayerUIManager.Instance.SetPlayerDead(pIndex);

                if (CurrentModeState == Mode.ScoreMode)
                {
                    StartCoroutine(RespawnPlayer(player, pIndex));
                }
                else
                {
                    playersToEliminate.Add(player);
                }
            }
        }

        foreach (var p in playersToEliminate)
        {
            OnPlayerEliminated(p);
            Destroy(p);
        }

        activePlayers.RemoveAll(p => p == null);
    }

    private IEnumerator RespawnPlayer(GameObject player, int playerIndex)
    {
        player.SetActive(false);
        Rigidbody rb = player.GetComponent<Rigidbody>();

        yield return new WaitForSeconds(Spawntime);

        Vector3 spawnPosition = Vector3.zero;
        if (SpawnPoint != null && SpawnPoint.Length > 0)
        {
            int targetIndex = (playerIndex < SpawnPoint.Length) ? playerIndex : 0;
            spawnPosition = SpawnPoint[targetIndex].position;
        }

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        player.transform.position = spawnPosition;
        player.SetActive(true);

        int countToDrop = pendingDropCounts[playerIndex];

        if (countToDrop > 0)
        {
            SpawnScoreItems(spawnPosition + Vector3.up * 1.5f, countToDrop);
            pendingDropCounts[playerIndex] = 0;
        }

        var pController = player.GetComponent<PlayerController1>();
        if (pController != null) pController.ResetPlayerState();

        if (PlayerUIManager.Instance != null)
        {
            PlayerUIManager.Instance.ResetPlayerStatus(playerIndex);
        }

        yield return new WaitForFixedUpdate();

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        isRespawning[playerIndex] = false;
    }

    public void DropScore(Vector3 deathPosition)
    {
        for (int i = 0; i < dropAmountPerDeath; i++)
        {
            if (scoreItemPrefab == null) break;

            GameObject item = Instantiate(scoreItemPrefab, deathPosition + Vector3.up, Quaternion.identity);
            ScoreItem script = item.GetComponent<ScoreItem>();

            if (script != null)
            {
                Vector3 randomDir = new Vector3(
                    Random.Range(-1f, 1f),
                    1.5f, 
                    Random.Range(-1f, 1f)
                ).normalized;

                script.Launch(randomDir, Random.Range(3f, 7f));
            }
        }
    }

    public void SpawnScoreItems(Vector3 position, int count)
    {
        StartCoroutine(SpawnItemsRoutine(position, count));
    }

    private IEnumerator SpawnItemsRoutine(Vector3 pos, int count)
    {
        Vector3 spawnBasePos = pos;
        spawnBasePos.y = 1.0f; 

        for (int i = 0; i < count; i++)
        {
            if (scoreItemPrefab == null) break;

            GameObject item = Instantiate(scoreItemPrefab, spawnBasePos + Random.insideUnitSphere * 0.5f, Quaternion.identity);
            ScoreItem script = item.GetComponent<ScoreItem>();
            if (script != null)
            {
                Vector3 dir = new Vector3(Random.Range(-1f, 1f), 2f, Random.Range(-1f, 1f)).normalized;
                script.Launch(dir, 5f);
            }
            yield return new WaitForSeconds(0.05f); 
        }
    }

    public void ResetScores()
    {
        for (int i = 0; i < currentScores.Length; i++)
        {
            currentScores[i] = 0;
        }

        for (int i = 0; i < pendingDropCounts.Length; i++)
        {
            pendingDropCounts[i] = 0;
        }

        if (PlayerUIManager.Instance != null)
        {
            for (int i = 0; i < currentScores.Length; i++)
            {
                PlayerUIManager.Instance.UpdatePlayerScore(i, 0);
            }
        }
    }

    public void TimeExpiredForSurvival()
    {
        NextRound(true);
    }

    public void NextRound(bool isTimeUp = false)
    {
        if (isRoundEnding) return;
        isRoundEnding = true;

        if (CurrentModeState == Mode.ScoreMode)
        {
            if (isTimeUp)
            {
                int maxScore = -1;
                int winnerIndex = -1;
                bool isDraw = false;

                for (int i = 0; i < currentScores.Length; i++)
                {
                    if (currentScores[i] > maxScore) { maxScore = currentScores[i]; winnerIndex = i; isDraw = false; }
                    else if (currentScores[i] == maxScore && maxScore != -1) { isDraw = true; }
                }

                if (!isDraw && winnerIndex != -1) playerWins[winnerIndex]++;
            }
            isGameStarted = false;
            StartCoroutine(WaitAndShowResult());
            return;
        }
        else
        {
            activePlayers.RemoveAll(p => p == null);
            int survivorCount = activePlayers.Count;

            if (survivorCount == 1)
            {
                GameObject winner = activePlayers[0];
                var health = winner.GetComponent<PlayerHealth>();
                if (health != null)
                {
                    playerWins[health.playerIndex]++; 
                }
            }
            else if (isTimeUp || survivorCount == 0)
            {
                List<int> survivors = new List<int>();
                foreach (var p in activePlayers)
                {
                    if (p != null) survivors.Add(p.GetComponent<PlayerHealth>().playerIndex);
                }
                if (survivors.Count == 0) survivors = _lastActiveIndices;

                TriggerSuddenDeath(survivors);
                return;
            }
        }

        CheckForGameWinner();
    }

    private void CheckForGameWinner()
    {
        bool someoneReachedThreeWins = false;
        for (int i = 0; i < playerWins.Length; i++)
        {
            if (playerWins[i] >= 3) { someoneReachedThreeWins = true; break; }
        }

        if (someoneReachedThreeWins)
        {
            StartCoroutine(WaitAndShowResult());
        }
        else
        {
            CurrentRound++;
            RestartGame();
        }
    }

    private IEnumerator WaitAndShowResult()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.StopBGM();
            SoundManager.Instance.PlayBGM(SoundManager.Instance.resultBGM);
        }

        if (PlayerUIManager.Instance != null)
        {
            for (int i = 0; i < playerWins.Length; i++)
            {
                PlayerUIManager.Instance.UpdatePlayerScore(i, playerWins[i]);
            }
        }
        yield return new WaitForSeconds(1.0f);

        string finalwinner = GetWinnerName();

        if (CurrentModeState == Mode.ScoreMode)
        {
            finalwinner = GetScoreWinnerName(); 
        }
        else
        {
            finalwinner = GetWinnerName(); 
        }

        ChangeMode(new GameOverMode(finalwinner));
    }

    private void TriggerSuddenDeath(List<int> qualifiers)
    {
        _isSuddenDeathNext = true;
        _qualifiedIndices = new List<int>(qualifiers);
        RestartGame();
    }

    public void ChangeMode(IGameMode newMode)
    {
        if (_currentMode != null) _currentMode.OnExit();
        _currentMode = newMode;
        if (_currentMode != null) _currentMode.OnEnter();

        if (newMode is SurvivalMode) CurrentModeState = Mode.Survival;
        else if (newMode is SuddenDeathMode) CurrentModeState = Mode.SuddenDeath;
        else if (newMode is GameOverMode) CurrentModeState = Mode.GameOver;
        else if (newMode is ScoreMode) CurrentModeState = Mode.ScoreMode;
    }

    public void ShowResultUI(string resultText)
    {
        if (resultCanvas != null)
        {
            resultCanvas.SetActive(true);
        }

        var pm = Object.FindFirstObjectByType<PauseManager>();
        if (pm != null && pm.pausePanel != null)
        {
            pm.pausePanel.SetActive(false);
        }

        if (resultBlurVolume != null)
        {
            resultBlurVolume.SetActive(true);
        }

        if (winnerNameTextUI != null)
        {
            winnerNameTextUI.text = resultText;
            winnerNameTextUI.gameObject.SetActive(true);
        }

        if (resultTextUI != null)
        {
            resultTextUI.text = "Result";
            resultTextUI.gameObject.SetActive(true);
        }

        if (resultRibbon != null)
        {
            resultRibbon.gameObject.SetActive(true);
            StartCoroutine(AnimateButtonsSwipe());
        }

        Button firstButton = resultCanvas.GetComponentInChildren<Button>();
        if (firstButton != null)
        {
            firstButton.Select();
        }
    }

    private IEnumerator AnimateButtonsSwipe()
    {
        if (resultRibbon == null) yield break;

        Vector2 endPos = Vector2.zero;
        Vector2 startPos = new Vector2(2000, 0);

        resultRibbon.anchoredPosition = startPos;

        float duration = 0.3f;
        float elapsed = 0f;

        Time.timeScale = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            t = 1f - Mathf.Pow(1f - t, 5f);
            resultRibbon.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }

        resultRibbon.anchoredPosition = endPos;
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void BackToJoinScene(string s)
    {
        CurrentRound = 1;
        _isSuddenDeathNext = false;
        _qualifiedIndices.Clear();

        for (int i = 0; i < playerWins.Length; i++) 
        {
            playerWins[i] = 0;
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.StopBGM();
        }

        Destroy(join);
        SceneManager.LoadScene(s);
    }

    public void HideUI(float delay) { }
    private IEnumerator HideUIRoutine(float delay) { yield break; }

    public List<GameObject> GetActivePlayers() { activePlayers.RemoveAll(p => p == null); return activePlayers; }

    public int GetActivePlayersCount()
    {
        int count = 0;
        foreach (var p in activePlayers) if (p != null) count++;
        return count;
    }

    public string GetWinnerName()
    {
        List<string> winners = new List<string>();

        for (int i = 0; i < playerWins.Length; i++)
        {
            if (playerWins[i] >= 3)
            {
                winners.Add("Player " + (i + 1));
            }
        }

        if (winners.Count == 0)
        {
            int maxWins = 0;
            for (int i = 0; i < playerWins.Length; i++)
                if (playerWins[i] > maxWins) maxWins = playerWins[i];

            for (int i = 0; i < playerWins.Length; i++)
                if (playerWins[i] == maxWins) winners.Add("Player " + (i + 1));
        }

        return string.Join(" & ", winners);
    }

    public string GetScoreWinnerName()
    {
        int maxScore = -1;
        List<string> winners = new List<string>();

        for (int i = 0; i < currentScores.Length; i++)
        {
            if (currentScores[i] > maxScore)
            {
                maxScore = currentScores[i];
            }
        }

        for (int i = 0; i < currentScores.Length; i++)
        {
            if (currentScores[i] == maxScore && maxScore != -1)
            {
                winners.Add("Player " + (i + 1));
            }
        }

        if (winners.Count == 0) return "No Winner";
        return string.Join(" & ", winners);
    }

    private void SetupUIForMode()
    {
        UpdateRoundDisplay();

        bool isScore = (CurrentModeState == Mode.ScoreMode);

        if (PlayerUIManager.Instance != null)
        {
            for (int i = 0; i < playerWins.Length; i++)
            {
                PlayerUIManager.Instance.UpdatePlayerUI(i, isScore);
            }
        }
    }
}
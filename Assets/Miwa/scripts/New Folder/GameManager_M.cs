using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using static GameMode;
using System.Collections;

public class GameManager_M : MonoBehaviour
{
    public static GameManager_M Instance { get; private set; }

    // --- 各モジュールの参照 ---
    public PlayerStateModule PlayerState { get; private set; }
    public ScoreModule Score { get; private set; }
    public UIModule UI { get; private set; }

    // --- 静的変数 ---
    public static bool _isSuddenDeathNext = false;
    public static List<int> _qualifiedIndices = new List<int>();
    public static int CurrentRound = 1;
    public static int[] playerWins = new int[4];
    public static Mode selectedGameMode = Mode.Survival;

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
    public float deathXLimit = 25.0f;
    public float deathZLimit = 25.0f;

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

    // ★外部のSuddenDeathModeからアクセスできるようにpublic（またはプロパティ）にします
    public IGameMode _currentMode;

    public float suddenDeathKnockbackMultiplier = 2.0f;
    public float currentKnockbackMultiplier = 1.0f;
    public float suddenDeathSpeedMultiplier = 10.0f;

    [HideInInspector] public GameObject join;
    [HideInInspector] public bool isGameStarted = false;
    public bool IsGameStartedProperty => isGameStarted;
    [HideInInspector] public bool isRoundEnding = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        PlayerState = new PlayerStateModule(this);
        Score = new ScoreModule(this);
        UI = new UIModule(this);

        PlayerState.Awake();
        AudioListener.pause = false;
    }

    void Start()
    {
        Time.timeScale = 1.0f;
        isRoundEnding = false;

        if (resultCanvas != null) resultCanvas.SetActive(false);
        if (timerTextUI != null) timerTextUI.gameObject.SetActive(true);

        UI.UpdateRoundDisplay();
        CurrentModeState = selectedGameMode;

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayBGM(_isSuddenDeathNext ? SoundManager.Instance.suddenDeathBGM : SoundManager.Instance.normalBattleBGM);
        }

        if (_isSuddenDeathNext)
        {
            ChangeMode(new SuddenDeathMode());
            // ★StartCountdown側でフラグがリセットされる前に判定できるよう、ここではリセットを保留するか、UIModuleに伝えます
        }
        else
        {
            _qualifiedIndices.Clear();
            if (CurrentModeState == Mode.ScoreMode) ChangeMode(new ScoreMode(timerTextUI, scoreModeTimeLimit));
            else ChangeMode(new SurvivalMode(timerTextUI, survivalTimeLimit));
        }

        join = GameObject.Find("JoinedManager");
        StartCoroutine(UI.InitializeUIWithDelay());
        SetupUIForMode();

        StartCoroutine(UI.StartCountdown());
    }

    void Update()
    {
        if (!isGameStarted || CurrentModeState == Mode.GameOver) return;

        if (_currentMode != null) _currentMode.OnUpdate();
        PlayerState.CheckPlayersFalling();
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

    public void TimeExpiredForSurvival() => NextRound(true);

    public void OnPlayerEliminated(GameObject eliminatedPlayer)
    {
        if (isRoundEnding || CurrentModeState == Mode.GameOver) return;

        // ★【確実な修正】引数やタイミングに依存せず、今この瞬間に「アクティブなプレイヤー」を直接数える
        int realSurvivorCount = 0;
        var allPlayers = PlayerState.GetActivePlayers(); // これで生存リストが取れる

        foreach (var p in allPlayers)
        {
            // 落ちた本人はこれから非アクティブになる、または既に非アクティブなので除外
            if (p != null && p != eliminatedPlayer && p.activeInHierarchy)
            {
                realSurvivorCount++;
            }
        }

        // 生き残りが1人以下（あるいは全員相打ちで0人）になったら、確実に次のラウンドへ
        if (realSurvivorCount <= 1)
        {
            NextRound();
        }
    }

    public void NextRound(bool isTimeUp = false)
    {
        if (isRoundEnding || CurrentModeState == Mode.GameOver) return;
        isRoundEnding = true;

        if (CurrentModeState == Mode.ScoreMode)
        {
            if (isTimeUp) Score.CalculateScoreWinner();
            isGameStarted = false;
            StartCoroutine(UI.WaitAndShowResult());
            return;
        }

        // 今この瞬間に生き残っている（アクティブな）プレイヤーを正確に取得
        List<GameObject> survivors = new List<GameObject>();
        foreach (var p in PlayerState.GetActivePlayers())
        {
            if (p != null && p.activeInHierarchy)
            {
                survivors.Add(p);
            }
        }

        int survivorCount = survivors.Count;
        Debug.Log($"[ラウンド終了判定] リアル生存数: {survivorCount}, 時間切れフラグ: {isTimeUp}");

        // --- パターンA: 誰も生き残っていない場合（全員相打ちで落ちた） ---
        if (survivorCount == 0)
        {
            Debug.Log("全員死亡のため、直前まで生存していたプレイヤー達でサドンデスを開始します。");
            TriggerSuddenDeath(PlayerState.LastActiveIndices);
            return;
        }

        // --- パターンB: 時間切れで2人以上生き残っている場合（引き分け） ---
        if (isTimeUp && survivorCount > 1)
        {
            List<int> survivorIndices = new List<int>();
            foreach (var p in survivors)
            {
                var health = p.GetComponent<PlayerHealth>();
                if (health != null) survivorIndices.Add(health.playerIndex);
            }

            Debug.Log($"時間切れ引き分けのため、生き残っている {survivorIndices.Count} 人のみでサドンデスを開始します。");
            TriggerSuddenDeath(survivorIndices);
            return;
        }

        // --- パターンC: 綺麗に1人だけが生き残って決着がついた場合 ---
        if (survivorCount == 1)
        {
            GameObject winner = survivors[0];
            var health = winner.GetComponent<PlayerHealth>();
            if (health != null)
            {
                playerWins[health.playerIndex]++;
                Debug.Log($"プレイヤー {health.playerIndex + 1} がラウンド勝利！");
            }
            CheckForGameWinner();
        }
        else
        {
            // それ以外の想定外のケース（時間切れではないが複数人残っているなど）は次のラウンドへ
            CheckForGameWinner();
        }
    }
    private void CheckForGameWinner()
    {
        bool someoneReachedThreeWins = false;
        foreach (int wins in playerWins) if (wins >= 3) { someoneReachedThreeWins = true; break; }

        if (someoneReachedThreeWins) StartCoroutine(UI.WaitAndShowResult());
        else { CurrentRound++; RestartGame(); }
    }

    private void TriggerSuddenDeath(List<int> qualifiers)
    {
        _isSuddenDeathNext = true;
        _qualifiedIndices = new List<int>(qualifiers);
        RestartGame();
    }

    public void RestartGame() => SceneManager.LoadScene(SceneManager.GetActiveScene().name);

    public void BackToJoinScene(string s)
    {
        CurrentRound = 1;
        _isSuddenDeathNext = false;
        _qualifiedIndices.Clear();
        System.Array.Clear(playerWins, 0, playerWins.Length);

        if (SoundManager.Instance != null) SoundManager.Instance.StopBGM();
        Destroy(join);
        SceneManager.LoadScene(s);
    }

    private void SetupUIForMode()
    {
        UI.UpdateRoundDisplay();
        bool isScore = (CurrentModeState == Mode.ScoreMode);
        if (PlayerUIManager.Instance != null)
        {
            for (int i = 0; i < playerWins.Length; i++) PlayerUIManager.Instance.UpdatePlayerUI(i, isScore);
        }
    }

    // --- 外部・互換性維持のためのラップメソッド ---
    public void RegisterPlayer(GameObject p, int index) => PlayerState.RegisterPlayer(p, index);
    public void AddScore(int playerIndex, int amount) => Score.AddScore(playerIndex, amount);
    public void DropScore(Vector3 deathPosition) => Score.DropScore(deathPosition);
    public void SpawnScoreItems(Vector3 position, int count) => Score.SpawnScoreItems(position, count);
    public void ShowResultUI(string resultText) => UI.ShowResultUI(resultText);
    public List<GameObject> GetActivePlayers() => PlayerState.GetActivePlayers();
    public int GetActivePlayersCount() => PlayerState.GetActivePlayersCount();
    public string GetWinnerName() => UI.GetWinnerName();
    public string GetScoreWinnerName() => UI.GetScoreWinnerName();
    public void SetAllPlayersControl(bool enabled) => PlayerState.SetAllPlayersControl(enabled);
    public void ResetScores() => Score.ResetScores();

    
}
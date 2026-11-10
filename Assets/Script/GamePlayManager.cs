using System;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    Ready,
    Playing,
    Finished,
    Paused
}

public class GamePlayManager : MonoBehaviour
{
    public static GamePlayManager instance;

    public event Action<GameState> StateChanged;
    public event Action<int> GameOverTriggered;

    private GameState state = GameState.Ready;
    private bool gameOverHandled;
    private int[] patternMappingSuccessCounts;
    private int[] patternMappingFailureCounts;
    private int[] atomicSpawnSuccessCounts;
    private int[] atomicSpawnFailureCounts;
    private int[] successfulPatternSpawnCounts;
    private int[] failedPatternSpawnCounts;
    private int patternSelectionAttempts;
    private int currentActiveSafeCount;
    private int currentActiveCatCount;
    private int maximumActiveTargetCount;
    private int sessionStartHp;
    private int hpLostToMandatorySafeExpiry;
    private int healingReceived;
    private int currentScore;
    private int highestLevelReached;
    private float sessionDuration;
    private int gameOverLevel;
    private int gameOverFinalScore;
    private readonly System.Collections.Generic.List<PatternContextRecord> successfulPatternContexts = new System.Collections.Generic.List<PatternContextRecord>();

    private struct PatternContextRecord
    {
        public SpawnPatternType Pattern;
        public UnlockPhase Phase;
        public int Level;
        public int Hp;
        public int ActiveSafeCount;
        public int ActiveCatCount;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void Start()
    {
        InitializeSession();
    }

    private void Update()
    {
        if (state == GameState.Playing)
        {
            sessionDuration += Time.deltaTime;
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }
    }

    public void InitializeSession()
    {
        ResetSessionMetrics();
        gameOverHandled = false;
        state = GameState.Ready;
        ScoreManager.Instance?.InitializeSession();
        HealthManager.Instance?.InitializeSession();
        DifficultyManager.Instance?.InitializeSession();
        TargetSpawner.Instance?.InitializeSession();
        sessionStartHp = HealthManager.Instance != null ? HealthManager.Instance.CurrentHp : 0;
        highestLevelReached = DifficultyManager.Instance != null ? DifficultyManager.Instance.GetCurrentLevel() : 0;
        RecordActiveTargetCounts(0, 0);
        NotifyStateChanged();
    }

    public void OnGameStart()
    {
        if (state == GameState.Playing)
        {
            return;
        }

        if (state == GameState.Paused)
        {
            return;
        }

        if (state == GameState.Finished)
        {
            InitializeSession();
        }

        state = GameState.Playing;

        if (SoundManager.GetInstance() != null)
        {
            SoundManager.GetInstance().Play("GamePlay");
        }

        NotifyStateChanged();
    }

    public GameState GetState()
    {
        return state;
    }

    public void OnPause()
    {
        if (state == GameState.Playing)
        {
            state = GameState.Paused;
            SoundManager.GetInstance()?.OnGamePaused(true);
            NotifyStateChanged();
        }
    }

    public void OnResume()
    {
        if (state == GameState.Paused)
        {
            state = GameState.Playing;
            SoundManager.GetInstance()?.OnGamePaused(false);
            NotifyStateChanged();
        }
    }

    public void TriggerGameOver(string reason = "")
    {
        if (state == GameState.Finished || gameOverHandled)
        {
            return;
        }

        state = GameState.Finished;
        gameOverHandled = true;

        int finalScore = ScoreManager.Instance != null ? ScoreManager.Instance.GetCurrentScore() : 0;
        gameOverLevel = DifficultyManager.Instance != null ? DifficultyManager.Instance.GetCurrentLevel() : 0;
        gameOverFinalScore = finalScore;
        currentScore = finalScore;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SaveFinalScore(finalScore);
        }

        if (SoundManager.GetInstance() != null)
        {
            SoundManager.GetInstance().Stop("GamePlay");
            SoundManager.GetInstance().Play("GameOver");
        }

        NotifyStateChanged();
        GameOverTriggered?.Invoke(finalScore);

        if (!string.IsNullOrEmpty(reason))
        {
            Debug.LogWarning("Game Over: " + reason);
        }

        Debug.Log(GetSessionSummary());
    }

    public void RecordPatternSelectionAttempt()
    {
        patternSelectionAttempts++;
    }

    public void RecordPatternMappingResult(SpawnPatternType pattern, bool success)
    {
        int index = (int)pattern;
        if (success)
        {
            patternMappingSuccessCounts[index]++;
        }
        else
        {
            patternMappingFailureCounts[index]++;
        }
    }

    public void RecordAtomicSpawnResult(SpawnPatternType pattern, bool success)
    {
        int index = (int)pattern;
        if (success)
        {
            atomicSpawnSuccessCounts[index]++;
        }
        else
        {
            atomicSpawnFailureCounts[index]++;
        }
    }

    public void RecordPatternOutcome(SpawnPatternType pattern, bool success)
    {
        int index = (int)pattern;
        if (success)
        {
            successfulPatternSpawnCounts[index]++;
        }
        else
        {
            failedPatternSpawnCounts[index]++;
        }
    }

    public void RecordPatternContext(SpawnPatternType pattern, UnlockPhase phase, int level, int hp, int activeSafeCount, int activeCatCount)
    {
        successfulPatternContexts.Add(new PatternContextRecord
        {
            Pattern = pattern,
            Phase = phase,
            Level = level,
            Hp = hp,
            ActiveSafeCount = activeSafeCount,
            ActiveCatCount = activeCatCount
        });
    }

    public void RecordActiveTargetCounts(int activeSafeCount, int activeCatCount)
    {
        currentActiveSafeCount = activeSafeCount;
        currentActiveCatCount = activeCatCount;
        maximumActiveTargetCount = Mathf.Max(maximumActiveTargetCount, activeSafeCount + activeCatCount);
    }

    public void RecordMandatorySafeExpiryHpLoss(int amount)
    {
        hpLostToMandatorySafeExpiry += Mathf.Max(0, amount);
    }

    public void RecordHealingReceived(int amount)
    {
        healingReceived += Mathf.Max(0, amount);
    }

    public void RecordCurrentScore(int score)
    {
        currentScore = Mathf.Max(0, score);
    }

    public void RecordHighestLevel(int level)
    {
        highestLevelReached = Mathf.Max(highestLevelReached, level);
    }

    public string GetSessionSummary()
    {
        StringBuilder summary = new StringBuilder();
        summary.AppendLine("Gameplay Session Summary");
        summary.AppendLine("Pattern selection attempts: " + patternSelectionAttempts);

        for (int i = 0; i < patternMappingSuccessCounts.Length; i++)
        {
            SpawnPatternType pattern = (SpawnPatternType)i;
            summary.AppendLine(pattern + ": mapping " + patternMappingSuccessCounts[i] + " success / " + patternMappingFailureCounts[i] + " failure, atomic " + atomicSpawnSuccessCounts[i] + " success / " + atomicSpawnFailureCounts[i] + " failure, spawn " + successfulPatternSpawnCounts[i] + " success / " + failedPatternSpawnCounts[i] + " failure");
        }

        summary.AppendLine("Active Safe: " + currentActiveSafeCount);
        summary.AppendLine("Active Cat: " + currentActiveCatCount);
        summary.AppendLine("Maximum active targets: " + maximumActiveTargetCount);
        summary.AppendLine("HP start: " + sessionStartHp);
        summary.AppendLine("HP lost to mandatory Safe expiry: " + hpLostToMandatorySafeExpiry);
        summary.AppendLine("Healing received: " + healingReceived);
        summary.AppendLine("Current score: " + currentScore);
        summary.AppendLine("Highest level: " + highestLevelReached);
        summary.AppendLine("Session duration: " + sessionDuration.ToString("F2") + " seconds");
        summary.AppendLine("Game Over level: " + gameOverLevel);
        summary.AppendLine("Game Over final score: " + gameOverFinalScore);
        summary.AppendLine("Successful pattern contexts: " + successfulPatternContexts.Count);

        for (int i = 0; i < successfulPatternContexts.Count; i++)
        {
            PatternContextRecord context = successfulPatternContexts[i];
            summary.AppendLine("Context " + (i + 1) + ": " + context.Pattern + ", phase " + context.Phase + ", level " + context.Level + ", HP " + context.Hp + ", Safe " + context.ActiveSafeCount + ", Cat " + context.ActiveCatCount);
        }

        return summary.ToString();
    }

    public void RetryGame()
    {
        InitializeSession();
        SoundManager.GetInstance()?.Stop("GameOver");

        //if (ScreenTransition.GetInstance() != null)
        //{
        //    ScreenTransition.GetInstance().TransitionToScene("Game");
        //}
    }

    public void ReturnToMainmenu()
    {
        SoundManager.GetInstance()?.Stop("GameOver");

        if (ScreenTransition.GetInstance() != null)
        {
            ScreenTransition.GetInstance().TransitionToScene("MainMenu");
        }
        else
        {
            SceneManager.LoadScene("MainMenu");
        }
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Game")
        {
            InitializeSession();
        }
    }

    private void NotifyStateChanged()
    {
        StateChanged?.Invoke(state);
    }

    private void ResetSessionMetrics()
    {
        int patternCount = Enum.GetValues(typeof(SpawnPatternType)).Length;
        patternMappingSuccessCounts = new int[patternCount];
        patternMappingFailureCounts = new int[patternCount];
        atomicSpawnSuccessCounts = new int[patternCount];
        atomicSpawnFailureCounts = new int[patternCount];
        successfulPatternSpawnCounts = new int[patternCount];
        failedPatternSpawnCounts = new int[patternCount];
        patternSelectionAttempts = 0;
        currentActiveSafeCount = 0;
        currentActiveCatCount = 0;
        maximumActiveTargetCount = 0;
        sessionStartHp = 0;
        hpLostToMandatorySafeExpiry = 0;
        healingReceived = 0;
        currentScore = 0;
        highestLevelReached = 0;
        sessionDuration = 0f;
        gameOverLevel = 0;
        gameOverFinalScore = 0;
        successfulPatternContexts.Clear();
    }
}

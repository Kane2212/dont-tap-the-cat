using System;
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

    private void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }
    }

    public void InitializeSession()
    {
        gameOverHandled = false;
        state = GameState.Ready;
        ScoreManager.Instance?.InitializeSession();
        HealthManager.Instance?.InitializeSession();
        DifficultyManager.Instance?.InitializeSession();
        TargetSpawner.Instance?.InitializeSession();
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
}

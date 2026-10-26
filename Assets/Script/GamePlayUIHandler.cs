using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GamePlayUIHandler : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject readyPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;

    [Header("Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Score")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private TextMeshProUGUI bestScoreText;

    [Header("Health")]
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private List<Image> heartImages = new List<Image>();

    [Header("Difficulty")]
    [SerializeField] private TextMeshProUGUI difficultyText;

    private bool isSubscribed;

    private void Awake()
    {
        BindButtonEvents();
    }

    private void OnEnable()
    {
        SubscribeToManagers();
        RefreshAllUI();
    }

    private void OnDisable()
    {
        UnsubscribeFromManagers();
    }

    private void Start()
    {
        RefreshAllUI();
    }

    private void BindButtonEvents()
    {
        if (startButton != null)
        {
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(OnStartButtonClicked);
        }

        if (pauseButton != null)
        {
            pauseButton.onClick.RemoveAllListeners();
            pauseButton.onClick.AddListener(OnPauseButtonClicked);
        }

        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveAllListeners();
            resumeButton.onClick.AddListener(OnResumeButtonClicked);
        }

        if (retryButton != null)
        {
            retryButton.onClick.RemoveAllListeners();
            retryButton.onClick.AddListener(OnRetryButtonClicked);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);
        }
    }

    private void SubscribeToManagers()
    {
        if (isSubscribed)
        {
            return;
        }

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ScoreChanged += HandleScoreChanged;
        }

        if (HealthManager.Instance != null)
        {
            HealthManager.Instance.HealthChanged += HandleHealthChanged;
        }

        if (DifficultyManager.Instance != null)
        {
            DifficultyManager.Instance.DifficultyChanged += HandleDifficultyChanged;
        }

        if (GamePlayManager.instance != null)
        {
            GamePlayManager.instance.StateChanged += HandleGameStateChanged;
            GamePlayManager.instance.GameOverTriggered += HandleGameOver;
        }

        isSubscribed = true;
    }

    private void UnsubscribeFromManagers()
    {
        if (!isSubscribed)
        {
            return;
        }

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ScoreChanged -= HandleScoreChanged;
        }

        if (HealthManager.Instance != null)
        {
            HealthManager.Instance.HealthChanged -= HandleHealthChanged;
        }

        if (DifficultyManager.Instance != null)
        {
            DifficultyManager.Instance.DifficultyChanged -= HandleDifficultyChanged;
        }

        if (GamePlayManager.instance != null)
        {
            GamePlayManager.instance.StateChanged -= HandleGameStateChanged;
            GamePlayManager.instance.GameOverTriggered -= HandleGameOver;
        }

        isSubscribed = false;
    }

    private void RefreshAllUI()
    {
        RefreshScore();
        RefreshHealth();
        RefreshDifficulty();
        RefreshGameStateUI();
    }

    private void RefreshScore()
    {
        if (scoreText != null)
        {
            scoreText.text = (ScoreManager.Instance != null ? ScoreManager.Instance.GetCurrentScore() : 0).ToString();
        }

        if (finalScoreText != null)
        {
            finalScoreText.text = "Final Score: " + (GameManager.Instance != null ? GameManager.Instance.GetFinalScore() : 0).ToString();
        }

        if (bestScoreText != null)
        {
            bestScoreText.text = "Best Score: " + (GameManager.Instance != null ? GameManager.Instance.GetHighScore() : 0).ToString();
        }
    }

    private void RefreshHealth()
    {
        int currentHp = HealthManager.Instance != null ? HealthManager.Instance.CurrentHp : 0;
        int maxHp = HealthManager.Instance != null ? HealthManager.Instance.MaxHp : 3;

        if (healthText != null)
        {
            healthText.text = currentHp + " / " + maxHp;
        }

        if (heartImages != null && heartImages.Count > 0)
        {
            int targetFilled = Mathf.Clamp(currentHp, 0, heartImages.Count);
            for (int i = 0; i < heartImages.Count; i++)
            {
                if (heartImages[i] == null)
                {
                    continue;
                }

                bool isFilled = i < targetFilled;
                heartImages[i].enabled = isFilled;
                var color = heartImages[i].color;
                color.a = isFilled ? 1f : 0.25f;
                heartImages[i].color = color;
            }
        }
    }

    private void RefreshDifficulty()
    {
        if (difficultyText != null)
        {
            difficultyText.text = "Lv. " + (DifficultyManager.Instance != null ? DifficultyManager.Instance.GetCurrentLevel() : 1).ToString();
        }
    }

    private void RefreshGameStateUI()
    {
        GameState state = GamePlayManager.instance != null ? GamePlayManager.instance.GetState() : GameState.Ready;

        bool readyActive = state == GameState.Ready;
        bool pauseActive = state == GameState.Paused;
        bool gameOverActive = state == GameState.Finished;

        if (readyPanel != null)
        {
            readyPanel.SetActive(readyActive);
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(pauseActive);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(gameOverActive);
        }

        if (pauseButton != null)
        {
            pauseButton.gameObject.SetActive(state == GameState.Playing);
        }
    }

    public void OnStartButtonClicked()
    {
        if (GamePlayManager.instance != null)
        {
            GamePlayManager.instance.OnGameStart();
        }

        if (readyPanel != null)
        {
            readyPanel.SetActive(false);
        }

        if (SoundManager.GetInstance() != null)
        {
            SoundManager.GetInstance().Play("ButtonClick");
        }
    }

    public void OnPauseButtonClicked()
    {
        if (GamePlayManager.instance != null)
        {
            GamePlayManager.instance.OnPause();
        }

        if (SoundManager.GetInstance() != null)
        {
            SoundManager.GetInstance().Play("Pause");
        }
    }

    public void OnResumeButtonClicked()
    {
        if (GamePlayManager.instance != null)
        {
            GamePlayManager.instance.OnResume();
        }

        if (SoundManager.GetInstance() != null)
        {
            SoundManager.GetInstance().Play("ButtonClick");
        }
    }

    public void OnRetryButtonClicked()
    {
        if (GamePlayManager.instance != null)
        {
            GamePlayManager.instance.RetryGame();
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        if (SoundManager.GetInstance() != null)
        {
            SoundManager.GetInstance().Play("ButtonClick");
        }
    }

    public void OnMainMenuButtonClicked()
    {
        if (SoundManager.GetInstance() != null)
        {
            SoundManager.GetInstance().Play("ButtonClick");
        }

        SceneManager.LoadScene("MainMenu");
    }

    private void HandleScoreChanged(int score)
    {
        RefreshScore();
    }

    private void HandleHealthChanged(int health)
    {
        RefreshHealth();
    }

    private void HandleDifficultyChanged(int level)
    {
        RefreshDifficulty();
    }

    private void HandleGameStateChanged(GameState state)
    {
        RefreshGameStateUI();
    }

    private void HandleGameOver(int finalScore)
    {
        RefreshScore();
        RefreshGameStateUI();

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (finalScoreText != null)
        {
            finalScoreText.text = "Final Score: " + finalScore;
        }

        if (bestScoreText != null && GameManager.Instance != null)
        {
            bestScoreText.text = "Best Score: " + GameManager.Instance.GetHighScore();
        }
    }

    public void Gamestart()
    {
        OnStartButtonClicked();
    }

    public void Pause_button()
    {
        OnPauseButtonClicked();
    }

    public void Resume_button()
    {
        OnResumeButtonClicked();
    }
}

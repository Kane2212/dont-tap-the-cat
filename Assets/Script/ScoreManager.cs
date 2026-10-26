using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public event Action<int> ScoreChanged;

    [SerializeField] private int currentScore;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void InitializeSession()
    {
        currentScore = 0;
        ScoreChanged?.Invoke(currentScore);
    }

    public void AddScore(int amount)
    {
        if (GamePlayManager.instance == null || GamePlayManager.instance.GetState() != GameState.Playing)
        {
            return;
        }

        if (amount <= 0)
        {
            return;
        }

        currentScore += amount;
        ScoreChanged?.Invoke(currentScore);
        DifficultyManager.Instance?.UpdateForScore(currentScore);
    }

    public int GetCurrentScore()
    {
        return currentScore;
    }

    public void SetScore(int score)
    {
        if (GamePlayManager.instance == null || GamePlayManager.instance.GetState() != GameState.Playing)
        {
            return;
        }

        currentScore = Mathf.Max(0, score);
        ScoreChanged?.Invoke(currentScore);
    }
}

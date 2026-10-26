using System;
using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance { get; private set; }

    public event Action<int> DifficultyChanged;

    [SerializeField] private int currentLevel = 1;

    public int CurrentLevel => currentLevel;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        currentLevel = 1;
    }

    public void InitializeSession()
    {
        currentLevel = 1;
        DifficultyChanged?.Invoke(currentLevel);
    }

    public void UpdateForScore(int score)
    {
        int calculatedLevel = Mathf.FloorToInt(score / 15f) + 1;
        currentLevel = Mathf.Max(1, calculatedLevel);
        DifficultyChanged?.Invoke(currentLevel);
    }

    public int GetCurrentLevel()
    {
        return currentLevel;
    }

    public int GetMaxActiveCats()
    {
        return Mathf.Clamp(currentLevel, 1, 9);
    }

    public float GetSafeSpawnInterval()
    {
        if (currentLevel <= 3)
        {
            return UnityEngine.Random.Range(2f, 3f);
        }

        if (currentLevel <= 6)
        {
            return UnityEngine.Random.Range(1.5f, 2.5f);
        }

        return UnityEngine.Random.Range(1f, 2f);
    }

    public float GetCatSpawnInterval()
    {
        if (currentLevel <= 3)
        {
            return UnityEngine.Random.Range(3f, 5f);
        }

        if (currentLevel <= 6)
        {
            return UnityEngine.Random.Range(2.5f, 4f);
        }

        return UnityEngine.Random.Range(2f, 3.5f);
    }

    public int GetMaxSafeTargetsPerWave()
    {
        if (currentLevel <= 3)
        {
            return 1;
        }

        if (currentLevel <= 6)
        {
            return 2;
        }

        return 3;
    }

    public int GetMaxActiveSafeTargets()
    {
        if (currentLevel <= 3)
        {
            return 2;
        }

        if (currentLevel <= 6)
        {
            return 4;
        }

        return 6;
    }

    public float GetWeightForPhase(UnlockPhase phase)
    {
        return phase == UnlockPhase.Early ? 1f : phase == UnlockPhase.Middle ? 1.5f : 2f;
    }
}

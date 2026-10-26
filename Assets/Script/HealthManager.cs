using System;
using UnityEngine;

public class HealthManager : MonoBehaviour
{
    public static HealthManager Instance { get; private set; }

    public event Action<int> HealthChanged;

    [SerializeField] private int maxHp = 3;
    [SerializeField] private int currentHp = 3;

    public int MaxHp => maxHp;
    public int CurrentHp => currentHp;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        currentHp = maxHp;
    }

    public void InitializeSession()
    {
        currentHp = maxHp;
        HealthChanged?.Invoke(currentHp);
    }

    public void TakeDamage(int amount)
    {
        if (GamePlayManager.instance == null || GamePlayManager.instance.GetState() != GameState.Playing)
        {
            return;
        }

        if (amount <= 0)
        {
            return;
        }

        currentHp = Mathf.Clamp(currentHp - amount, 0, maxHp);
        HealthChanged?.Invoke(currentHp);

        if (currentHp <= 0)
        {
            GamePlayManager.instance.TriggerGameOver("HP reached zero");
        }
    }

    public void Heal(int amount)
    {
        if (GamePlayManager.instance == null || GamePlayManager.instance.GetState() != GameState.Playing)
        {
            return;
        }

        if (amount <= 0)
        {
            return;
        }

        currentHp = Mathf.Clamp(currentHp + amount, 0, maxHp);
        HealthChanged?.Invoke(currentHp);
    }

    public void SetMaxHp(int value)
    {
        maxHp = Mathf.Max(1, value);
        currentHp = Mathf.Clamp(currentHp, 0, maxHp);
        HealthChanged?.Invoke(currentHp);
    }
}

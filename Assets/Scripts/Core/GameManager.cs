using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Settings")]
    public int StartingHealth = 20;
    public int StartingGold = 100;

    [Header("Current State")]
    public int CurrentHealth;
    public int CurrentGold;
    public bool IsGameOver = false;

    // Events for UI and FX
    public event Action<int> OnHealthChanged;
    public event Action<int> OnGoldChanged;
    public event Action OnGameLost;
    public event Action OnGameWon;
    public event Action OnDamageTaken; // For Camera Shake

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        CurrentHealth = StartingHealth;
        CurrentGold = StartingGold;
    }

    private void Start()
    {
        // Initialize UI
        OnHealthChanged?.Invoke(CurrentHealth);
        OnGoldChanged?.Invoke(CurrentGold);
    }

    public void ReduceHealth(int amount)
    {
        if (IsGameOver) return;

        CurrentHealth -= amount;
        OnHealthChanged?.Invoke(CurrentHealth);
        OnDamageTaken?.Invoke();

        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            GameOver();
        }
    }

    public void AddGold(int amount)
    {
        CurrentGold += amount;
        OnGoldChanged?.Invoke(CurrentGold);
    }

    public bool SpendGold(int amount)
    {
        if (CurrentGold >= amount)
        {
            CurrentGold -= amount;
            OnGoldChanged?.Invoke(CurrentGold);
            return true;
        }
        return false;
    }

    private void GameOver()
    {
        IsGameOver = true;
        OnGameLost?.Invoke();
        Debug.Log("Game Over!");
    }

    public void Victory()
    {
        IsGameOver = true;
        OnGameWon?.Invoke();
        Debug.Log("Victory!");
    }
}

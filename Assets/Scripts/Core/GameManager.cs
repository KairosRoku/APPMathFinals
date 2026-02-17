using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int StartingHealth = 20;
    public int StartingGold = 100;

    public int CurrentHealth;
    public int CurrentGold;
    public bool IsGameOver = false;
    
    public AudioManager AudioManager => AudioManager.Instance;

    public event Action<int> OnHealthChanged;
    public event Action<int> OnGoldChanged;
    public event Action OnGameLost;
    public event Action OnGameWon;
    public event Action OnDamageTaken;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Instance.ResetGameState();
            Destroy(this);
            return;
        }

        Instance = this;
        ResetGameState();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void ResetGameState()
    {
        CurrentHealth = StartingHealth;
        CurrentGold = StartingGold;
        IsGameOver = false;
        
        OnHealthChanged?.Invoke(CurrentHealth);
        OnGoldChanged?.Invoke(CurrentGold);
    }

    private void Start()
    {
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
        if (IsGameOver) return;
        IsGameOver = true;
        OnGameWon?.Invoke();
        Debug.Log("Victory!");
    }
}

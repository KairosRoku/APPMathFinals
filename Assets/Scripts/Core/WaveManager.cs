using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Wave
{
    public int count;
    public float rate;
    public GameObject enemyPrefab; // Could be array for mixed waves
}

public class WaveManager : MonoBehaviour
{
    public Wave[] Waves; // We will populate this programmatically or in Inspector
    public Transform SpawnPoint;

    private int waveIndex = 0;
    private bool _waveInProgress = false;
    private float _nextWaveTimer = 0f;
    private float _autoStartThreshold = 30f;

    private void Start()
    {
        _nextWaveTimer = _autoStartThreshold;
    }

    private void Update()
    {
        if (_waveInProgress || waveIndex >= Waves.Length) 
        {
            if (GameUI.Instance != null) GameUI.Instance.UpdateTimer(0);
            return;
        }

        _nextWaveTimer -= Time.deltaTime;
        
        if (GameUI.Instance != null) GameUI.Instance.UpdateTimer(_nextWaveTimer);

        if (_nextWaveTimer <= 0)
        {
            StartNextWave();
        }
    }

    public void StartNextWave()
    {
        if (_waveInProgress || waveIndex >= Waves.Length) return;
        StartCoroutine(SpawnWave());
    }

    IEnumerator SpawnWave()
    {
        _waveInProgress = true;
        Wave wave = Waves[waveIndex];
        
        // Update UI
        if (GameUI.Instance != null) GameUI.Instance.UpdateWave(waveIndex + 1);
        
        int totalEnemies = GetEnemyCountForWave(waveIndex + 1);
        int spawnedCount = 0;

        // Group size - enemies of the same type spawn together
        int groupSize = 5;

        while (spawnedCount < totalEnemies)
        {
            // Pick a random type for this group
            EnemyType groupType = (EnemyType)Random.Range(0, System.Enum.GetValues(typeof(EnemyType)).Length);
            
            int currentGroupCount = Mathf.Min(groupSize, totalEnemies - spawnedCount);
            
            for (int i = 0; i < currentGroupCount; i++)
            {
                SpawnEnemy(wave.enemyPrefab, groupType);
                spawnedCount++;
                yield return new WaitForSeconds(1f / wave.rate);
            }
        }

        waveIndex++;
        _waveInProgress = false;
        _nextWaveTimer = _autoStartThreshold;
        
        // Trigger win check if this was the last wave
        GameManager.Instance.CheckLevelCompletion(waveIndex, Waves.Length, _waveInProgress);
    }

    void SpawnEnemy(GameObject prefab, EnemyType type)
    {
        GameObject enemyGO = Instantiate(prefab, SpawnPoint.position, Quaternion.identity);
        EnemyBase enemy = enemyGO.GetComponent<EnemyBase>();
        if (enemy != null)
        {
            enemy.SetType(type);
        }
    }
    
    // Hardcoded curve for now based on GDD
    int GetEnemyCountForWave(int waveNum)
    {
        // 5, 8, 12, 16, 22, 28, 35, 43, 52, 65
        int[] counts = { 5, 8, 12, 16, 22, 28, 35, 43, 52, 65 };
        if(waveNum <= counts.Length) return counts[waveNum-1];
        return counts[counts.Length-1] + (waveNum - counts.Length) * 10;
    }
}

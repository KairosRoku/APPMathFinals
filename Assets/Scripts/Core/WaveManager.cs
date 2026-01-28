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

    // Remove Auto Update
    private void Update()
    {
       // Check for enemies alive to determine if wave ended?
       // For now, simple manual start.
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
        
        // Dynamic Scaling based on GDD
        int enemyCount = GetEnemyCountForWave(waveIndex + 1); 
        
        for (int i = 0; i < enemyCount; i++)
        {
            SpawnEnemy(wave.enemyPrefab);
            yield return new WaitForSeconds(1f / wave.rate);
        }

        waveIndex++;
        _waveInProgress = false; // Allow next wave whenever user clicks
    }

    void SpawnEnemy(GameObject prefab)
    {
        Instantiate(prefab, SpawnPoint.position, Quaternion.identity);
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

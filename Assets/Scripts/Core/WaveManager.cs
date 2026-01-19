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

    public float timeBetweenWaves = 5f;
    private float countdown = 2f;
    private int waveIndex = 0;

    private void Update()
    {
        if (waveIndex >= Waves.Length)
        {
            // Level Won logic if all enemies dead
            return;
        }

        if (countdown <= 0f)
        {
            StartCoroutine(SpawnWave());
            countdown = timeBetweenWaves;
        }

        countdown -= Time.deltaTime;
        // Update Timer UI
    }

    IEnumerator SpawnWave()
    {
        Wave wave = Waves[waveIndex];
        
        // Dynamic Scaling based on GDD
        // Wave 1-10: 5, 8, 12, 16... 65
        int enemyCount = GetEnemyCountForWave(waveIndex + 1); 
        
        for (int i = 0; i < enemyCount; i++)
        {
            SpawnEnemy(wave.enemyPrefab);
            yield return new WaitForSeconds(1f / wave.rate);
        }

        waveIndex++;
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

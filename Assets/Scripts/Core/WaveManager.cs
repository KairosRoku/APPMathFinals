using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class EnemyBatch
{
    public EnemyType type;
    public int count;
    public bool resistanceEnabled = true;

    public EnemyBatch() { }
    public EnemyBatch(EnemyType t, int c, bool r = true)
    {
        type = t;
        count = c;
        resistanceEnabled = r;
    }
}

[System.Serializable]
public class Wave
{
    public List<EnemyBatch> batches = new List<EnemyBatch>();
    public float spawnRate = 2.0f;
    public float densityMultiplier = 1.0f;
}

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance;

    [Header("Enemy Prefabs")]
    public GameObject GruntPrefab;
    public GameObject RunnerPrefab;
    public GameObject TankPrefab;
    public GameObject BossPrefab;

    [Header("Wave Data")]
    public Wave[] Waves;
    public Transform SpawnPoint;

    [Header("Settings")]
    public float AutoStartThreshold = 30f;
    
    [Header("Status")]
    [SerializeField] private int waveIndex = 0;
    private bool _waveInProgress = false;
    private float _nextWaveTimer = 0f;
    public int ActiveEnemyCount { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // Transfer scene-specific data to the persistent instance
            Instance.Waves = this.Waves;
            Instance.SpawnPoint = this.SpawnPoint;
            
            // Prefabs might be updated/changed per level
            Instance.GruntPrefab = this.GruntPrefab;
            Instance.RunnerPrefab = this.RunnerPrefab;
            Instance.TankPrefab = this.TankPrefab;
            Instance.BossPrefab = this.BossPrefab;

            Instance.ResetWaveManager();
            Destroy(this);
            return;
        }

        Instance = this;
        ResetWaveManager();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void ResetWaveManager()
    {
        StopAllCoroutines();
        waveIndex = 0;
        _waveInProgress = false;
        _nextWaveTimer = 5f;
        ActiveEnemyCount = 0;

        // Repair references if they are lost (important if manager is persistent)
        if (SpawnPoint == null)
        {
            GameObject sp = GameObject.Find("SpawnPoint");
            if (sp != null) SpawnPoint = sp.transform;
        }
    }

    private void Start()
    {
        ResetWaveManager();
    }

    private void Update()
    {
        if (_waveInProgress || Waves == null || waveIndex >= Waves.Length) 
        {
            return;
        }

        _nextWaveTimer -= Time.deltaTime;
        if (GameUI.Instance != null) 
        {
            GameUI.Instance.UpdateCountdown(_nextWaveTimer);
        }

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
        Wave currentWave = Waves[waveIndex];
        
        if (GameUI.Instance != null) 
        {
            GameUI.Instance.UpdateWave(waveIndex + 1);
            GameUI.Instance.ShowGo();
        }

        foreach (var batch in currentWave.batches)
        {
            GameObject prefabToSpawn = GetPrefabForType(batch.type);
            
            if (prefabToSpawn == null)
            {
                Debug.LogError($"No prefab assigned for enemy type {batch.type} in wave {waveIndex + 1}");
                continue;
            }

            for (int i = 0; i < batch.count; i++)
            {
                SpawnEnemy(prefabToSpawn, batch.type, batch.resistanceEnabled);
                float delay = 1f / (currentWave.spawnRate * currentWave.densityMultiplier);
                yield return new WaitForSeconds(delay);
            }
        }

        waveIndex++;
        _waveInProgress = false;
        _nextWaveTimer = AutoStartThreshold;
        
        CheckForVictory();
    }

    void SpawnEnemy(GameObject prefab, EnemyType type, bool resistance)
    {
        if (SpawnPoint == null) return;
        GameObject enemyGO = Instantiate(prefab, SpawnPoint.position, Quaternion.identity);
        ActiveEnemyCount++;

        EnemyBase enemy = enemyGO.GetComponent<EnemyBase>();
        if (enemy != null)
        {
            enemy.ConfigureStats(type);
            enemy.ResistanceEnabled = resistance;
        }
    }

    public void NotifyEnemyDestroyed()
    {
        ActiveEnemyCount--;
        CheckForVictory();
    }

    private void CheckForVictory()
    {
        // Only win if all waves spawned AND no enemies left progress
        if (!_waveInProgress && waveIndex >= Waves.Length && ActiveEnemyCount <= 0)
        {
            GameManager.Instance.Victory();
        }
    }

    private GameObject GetPrefabForType(EnemyType type)
    {
        switch (type)
        {
            case EnemyType.Grunt: return GruntPrefab;
            case EnemyType.Runner: return RunnerPrefab;
            case EnemyType.Tank: return TankPrefab;
            case EnemyType.Boss: return BossPrefab;
            default: return null;
        }
    }

    [ContextMenu("Generate 10 Wave Plan")]
    public void GenerateWavePlan()
    {
        Waves = new Wave[10];
        
        // Wave 1: 5 Grunts
        Waves[0] = CreateWave(2f, 1f, new EnemyBatch(EnemyType.Grunt, 5));
        
        // Wave 2: 4 Grunts, 4 Runners (No Resistance)
        Waves[1] = CreateWave(2f, 1f, 
            new EnemyBatch(EnemyType.Grunt, 4), 
            new EnemyBatch(EnemyType.Runner, 4, false));
            
        // Wave 3: 6 Grunts, 6 Tanks
        Waves[2] = CreateWave(2f, 1f, 
            new EnemyBatch(EnemyType.Grunt, 6), 
            new EnemyBatch(EnemyType.Tank, 6));
            
        // Wave 4: 8 Runners, 8 Tanks
        Waves[3] = CreateWave(2f, 1f, 
            new EnemyBatch(EnemyType.Runner, 8), 
            new EnemyBatch(EnemyType.Tank, 8));
            
        // Wave 5: Mixed Balance (7 Grunts, 7 Runners, 8 Tanks = 22)
        Waves[4] = CreateWave(2f, 1.2f, 
            new EnemyBatch(EnemyType.Grunt, 7), 
            new EnemyBatch(EnemyType.Runner, 7),
            new EnemyBatch(EnemyType.Tank, 8));
            
        // Wave 6: High Density Runners (28)
        Waves[5] = CreateWave(2f, 2.0f, new EnemyBatch(EnemyType.Runner, 28));
        
        // Wave 7: High Density Tanks (35)
        Waves[6] = CreateWave(2f, 2.0f, new EnemyBatch(EnemyType.Tank, 35));
        
        // Wave 8: Speed Variation (23 Runners, 20 Tanks = 43)
        Waves[7] = CreateWave(2f, 1.5f, 
            new EnemyBatch(EnemyType.Runner, 23), 
            new EnemyBatch(EnemyType.Tank, 20));
            
        // Wave 9: High Health (52 Tanks)
        Waves[8] = CreateWave(2f, 1.5f, new EnemyBatch(EnemyType.Tank, 52));
        
        // Wave 10: Boss + All Types (21 Grunts, 21 Runners, 22 Tanks, 1 Boss = 65)
        Waves[9] = CreateWave(2f, 3.0f, 
            new EnemyBatch(EnemyType.Grunt, 21), 
            new EnemyBatch(EnemyType.Runner, 21),
            new EnemyBatch(EnemyType.Tank, 22),
            new EnemyBatch(EnemyType.Boss, 1));

        Debug.Log("Generated 10-wave plan. Please ensure all 4 Enemy Prefabs are assigned in the WaveManager inspector.");
    }

    private Wave CreateWave(float rate, float density, params EnemyBatch[] batches)
    {
        Wave w = new Wave();
        w.spawnRate = rate;
        w.densityMultiplier = density;
        w.batches.AddRange(batches);
        return w;
    }
}

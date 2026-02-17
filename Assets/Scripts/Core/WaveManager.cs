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

    public GameObject GruntPrefab;
    public GameObject RunnerPrefab;
    public GameObject TankPrefab;
    public GameObject BossPrefab;

    public Wave[] Waves;
    public Transform SpawnPoint;

    public float AutoStartThreshold = 30f;
    
    [SerializeField] private int waveIndex = 0;
    private bool _waveInProgress = false;
    private float _nextWaveTimer = 0f;
    public int ActiveEnemyCount { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Instance.Waves = this.Waves;
            Instance.SpawnPoint = this.SpawnPoint;
            
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
        
        Waves[0] = CreateWave(2f, 1f, new EnemyBatch(EnemyType.Grunt, 5));
        
        Waves[1] = CreateWave(2f, 1f, 
            new EnemyBatch(EnemyType.Grunt, 4), 
            new EnemyBatch(EnemyType.Runner, 4, false));
            
        Waves[2] = CreateWave(2f, 1f, 
            new EnemyBatch(EnemyType.Grunt, 6), 
            new EnemyBatch(EnemyType.Tank, 6));
            
        Waves[3] = CreateWave(2f, 1f, 
            new EnemyBatch(EnemyType.Runner, 8), 
            new EnemyBatch(EnemyType.Tank, 8));
            
        Waves[4] = CreateWave(2f, 1.2f, 
            new EnemyBatch(EnemyType.Grunt, 7), 
            new EnemyBatch(EnemyType.Runner, 7),
            new EnemyBatch(EnemyType.Tank, 8));
            
        Waves[5] = CreateWave(2f, 2.0f, new EnemyBatch(EnemyType.Runner, 28));
        
        Waves[6] = CreateWave(2f, 2.0f, new EnemyBatch(EnemyType.Tank, 35));
        
        Waves[7] = CreateWave(2f, 1.5f, 
            new EnemyBatch(EnemyType.Runner, 23), 
            new EnemyBatch(EnemyType.Tank, 20));
            
        Waves[8] = CreateWave(2f, 1.5f, new EnemyBatch(EnemyType.Tank, 52));
        
        Waves[9] = CreateWave(2f, 3.0f, 
            new EnemyBatch(EnemyType.Grunt, 21), 
            new EnemyBatch(EnemyType.Runner, 21),
            new EnemyBatch(EnemyType.Tank, 22),
            new EnemyBatch(EnemyType.Boss, 1));
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WaveSpawner : MonoBehaviour
{
    [System.Serializable]
    public class WaveEntry
    {
        public EnemyData enemyData;
        public GameObject prefab;
        public int count = 5;
    }

    [System.Serializable]
    public class Wave
    {
        public string waveName = "Wave";
        public List<WaveEntry> entries = new List<WaveEntry>();
        public float spawnDelay = 0.3f;
    }

    [Header("Waves")]
    [SerializeField] List<Wave> waves = new List<Wave>();
    [SerializeField] float timeBetweenWaves = 5f;
    [SerializeField] bool autoStart = true;
    [SerializeField] bool loopWaves;

    [Header("Scaling (for looped waves)")]
    [SerializeField] int extraEnemiesPerLoop = 2;
    [SerializeField] float healthMultiplierPerLoop = 1.2f;

    [Header("Spawn Points")]
    [SerializeField] SpawnPoint[] spawnPoints;

    [Header("Events")]
    public UnityEvent<int, int> OnWaveStarted;
    public UnityEvent<int> OnWaveCompleted;
    public UnityEvent OnAllWavesCompleted;
    public UnityEvent<int> OnEnemyCountChanged;

    int currentWaveIndex;
    int enemiesAlive;
    int loopCount;
    bool isSpawning;
    bool isActive;

    public int CurrentWave => currentWaveIndex + 1;
    public int TotalWaves => waves.Count;
    public int EnemiesAlive => enemiesAlive;
    public bool IsActive => isActive;

    void Start()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
            spawnPoints = FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);

        if (autoStart)
            StartSpawning();
    }

    public void StartSpawning()
    {
        isActive = true;
        currentWaveIndex = 0;
        loopCount = 0;
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (currentWaveIndex < waves.Count)
        {
            yield return new WaitForSeconds(timeBetweenWaves);
            yield return StartCoroutine(SpawnWave(waves[currentWaveIndex]));

            yield return new WaitUntil(() => enemiesAlive <= 0);

            OnWaveCompleted?.Invoke(currentWaveIndex);
            currentWaveIndex++;

            if (currentWaveIndex >= waves.Count && loopWaves)
            {
                currentWaveIndex = 0;
                loopCount++;
            }
        }

        isActive = false;
        OnAllWavesCompleted?.Invoke();
    }

    IEnumerator SpawnWave(Wave wave)
    {
        OnWaveStarted?.Invoke(currentWaveIndex + 1, waves.Count);

        foreach (WaveEntry entry in wave.entries)
        {
            int count = entry.count + (loopCount * extraEnemiesPerLoop);

            for (int i = 0; i < count; i++)
            {
                SpawnEnemy(entry);
                yield return new WaitForSeconds(wave.spawnDelay);
            }
        }
    }

    void SpawnEnemy(WaveEntry entry)
    {
        if (spawnPoints.Length == 0) return;

        SpawnPoint sp = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Vector3 offset = Random.insideUnitSphere * 2f;
        offset.y = 0f;
        Vector3 spawnPos = sp.transform.position + offset;

        GameObject enemyObj = Instantiate(entry.prefab, spawnPos, sp.transform.rotation);

        EnemyAI ai = enemyObj.GetComponent<EnemyAI>();
        if (ai != null)
        {
            Transform player = GameObject.FindGameObjectWithTag("Player")?.transform;
            ai.Initialize(entry.enemyData, player);
        }

        enemiesAlive++;
        OnEnemyCountChanged?.Invoke(enemiesAlive);
    }

    public void OnEnemyDied()
    {
        enemiesAlive = Mathf.Max(0, enemiesAlive - 1);
        OnEnemyCountChanged?.Invoke(enemiesAlive);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager S;   // singleton

    [System.Serializable]
    public class LevelConfig
    {
        public int killTarget;          // 10, 20, 50
        public GameObject[] covers;     // drag cover objects for this level
    }

    [Header("Scene Refs")]
    public Transform platform; 

    [Header("Levels")]
    public LevelConfig[] levels;        // size = 3 in Inspector
    public int currentLevelIndex = 0;

    [Header("Spawning")]
    public GameObject enemyPrefab;      // assign _Enemy prefab here
    public Transform[] spawnPoints;     // empty objects on edges of platform
    public float spawnInterval = 4f;
    public int maxAlive = 8;

    private int enemiesKilled;
    private int enemiesSpawned;
    private int fibIndex;
    private int alive;

    private readonly int[] fib = { 1, 2, 3, 5, 8 };

    void Awake()
    {
        if (S == null) S = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        SetupLevel(currentLevelIndex);
        StartCoroutine(SpawnLoop());
    }

    void SetupLevel(int index)
    {
        enemiesKilled  = 0;
        enemiesSpawned = 0;
        fibIndex       = 0;
        alive          = 0;

        // 1) Turn ALL covers off, across ALL levels
        foreach (LevelConfig lc in levels)
        {
            if (lc.covers == null) continue;

            foreach (GameObject c in lc.covers)
            {
                if (c != null) c.SetActive(false);
            }
        }

        // 2) Turn ON only the covers for the current level
        LevelConfig current = levels[index];
        if (current.covers != null)
        {
            foreach (GameObject c in current.covers)
            {
                if (c != null) c.SetActive(true);
            }
        }

        Debug.Log($"Starting level {index + 1}, kill target = {levels[index].killTarget}");
    }


    IEnumerator SpawnLoop()
    {
        while (currentLevelIndex < levels.Length)
        {
            yield return new WaitForSeconds(spawnInterval);
            SpawnWave();
        }
    }

    void SpawnWave()
    {
        if (enemyPrefab == null || spawnPoints.Length == 0) return;
        if (alive >= maxAlive) return;

        LevelConfig lc = levels[currentLevelIndex];

        int toSpawn = fib[fibIndex];
        if (fibIndex < fib.Length - 1) fibIndex++;

        int remainingNeeded = lc.killTarget - enemiesSpawned;
        if (remainingNeeded <= 0) return;

        toSpawn = Mathf.Min(toSpawn, remainingNeeded);
        toSpawn = Mathf.Min(toSpawn, maxAlive - alive);

        for (int i = 0; i < toSpawn; i++)
        {
            Transform sp = spawnPoints[Random.Range(0, spawnPoints.Length)];

            // 🔽 instantiate enemy
            GameObject enemyGO = Instantiate(enemyPrefab, sp.position, Quaternion.identity);
            alive++;

            // 🔽 assign the scene Platform to this instance's BoundsCheck
            BoundsCheck bc = enemyGO.GetComponent<BoundsCheck>();
            if (bc != null)
            {
                bc.platform = platform;
            }
        }

        enemiesSpawned += toSpawn;
    }


    public void OnEnemyKilled(Enemy e)
    {
        alive--;
        enemiesKilled++;

        Debug.Log($"Enemy killed. {enemiesKilled}/{levels[currentLevelIndex].killTarget} this level.");

        if (enemiesKilled >= levels[currentLevelIndex].killTarget)
        {
            AdvanceLevel();
        }
    }

    void AdvanceLevel()
    {
        currentLevelIndex++;

        if (currentLevelIndex >= levels.Length)
        {
            Debug.Log("All levels complete! YOU WIN!");
            // TODO: show victory UI / stop game, etc.
            return;
        }

        SetupLevel(currentLevelIndex);
    }
}

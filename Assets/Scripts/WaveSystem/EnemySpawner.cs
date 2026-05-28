using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Abdulrahman.EnemySystem;

// I put everything in its own namespace to keep it separate from the enemy/player stuff
namespace Abdulrahman.WaveSystem
{
    // This just holds the settings for a single wave.
    // I made it serializable so Unity shows it nicely in the Inspector
    // instead of me having to hardcode everything.
    [System.Serializable]
    public class WaveConfig
    {
        // just a label so we know which wave is which in the Inspector
        public string waveName = "Wave";

        // how many enemies total get spawned this wave
        public int totalEnemies = 20;

        // we don't dump all of them at once this controls how many come per batch
        public int batchSize = 5;

        // seconds between each batch. It's 5 minutes
        public float timeBetweenBatches = 300f;

        // the max HP enemies spawn with overrides the prefab default at runtime
        public float enemyHealth = 100f;

        // multiplied onto the zombie's base walk/run speed
        [Range(0.5f, 3f)]
        public float speedMultiplier = 1f;

        // how much harder they hit compared to wave 1
        [Range(0.5f, 5f)]
        public float damageMultiplier = 1f;
    }

    // The main spawner. Stick this on an empty GameObject in the scene.
    // It handles all 5 waves automatically you just set it up once and hit Play.
    public class EnemySpawner : MonoBehaviour
    {
        [Header("PREFAB")]
        // AbdulRahman drag the Ghoul prefab here from Assets/Imports/Enemies/Ghoul_Zombie/
        public GameObject enemyPrefab;

        [Header("SPAWN POINTS")]
        // these are the positions where enemies pop in.
        public Transform[] spawnPoints;

        [Header("WAVE SETTINGS")]
        public WaveConfig[] waves = new WaveConfig[]
        {
            // wave 1 
            new WaveConfig
            {
                waveName           = "Wave 1 – The First Sign",
                totalEnemies       = 20,
                batchSize          = 5,
                timeBetweenBatches = 300f,
                enemyHealth        = 100f,
                speedMultiplier    = 1f,
                damageMultiplier   = 1f
            },

            // wave 2
            new WaveConfig
            {
                waveName           = "Wave 2 – Growing Darkness",
                totalEnemies       = 25,
                batchSize          = 5,
                timeBetweenBatches = 240f,
                enemyHealth        = 150f,
                speedMultiplier    = 1.15f,
                damageMultiplier   = 1.25f
            },

            // wave 3
            new WaveConfig
            {
                waveName           = "Wave 3 – The Swarm",
                totalEnemies       = 30,
                batchSize          = 6,
                timeBetweenBatches = 180f,
                enemyHealth        = 200f,
                speedMultiplier    = 1.3f,
                damageMultiplier   = 1.5f
            },

            // wave 4
            new WaveConfig
            {
                waveName           = "Wave 4 – Relentless Hunt",
                totalEnemies       = 40,
                batchSize          = 8,
                timeBetweenBatches = 150f,
                enemyHealth        = 275f,
                speedMultiplier    = 1.5f,
                damageMultiplier   = 1.75f
            },

            // wave 5
            new WaveConfig
            {
                waveName           = "Wave 5 – FINAL ONSLAUGHT",
                totalEnemies       = 50,
                batchSize          = 10,
                timeBetweenBatches = 120f,
                enemyHealth        = 400f,
                speedMultiplier    = 1.75f,
                damageMultiplier   = 2.5f
            }
        };

        [Header("TIMING")]
        // gives the player a few seconds at the start
        public float initialDelay = 5f;


        public float delayBetweenWaves = 10f;

        // these are read by WaveUIManager to show info on screen
        public int       CurrentWaveIndex    { get; private set; } = -1;
        public int       EnemiesAlive        { get; private set; } = 0;
        public int       EnemiesKilledInWave { get; private set; } = 0;
        public bool      AllWavesComplete    { get; private set; } = false;
        public bool      WaveInProgress      { get; private set; } = false;
        public WaveConfig CurrentWave        => (CurrentWaveIndex >= 0 && CurrentWaveIndex < waves.Length)
                                                ? waves[CurrentWaveIndex] : null;

        public event System.Action<int, WaveConfig> OnWaveStarted;
        public event System.Action<int>             OnWaveCompleted;
        public event System.Action                  OnAllWavesCompleted;

        // keeps track of all living enemies so we know when a wave is cleared
        private List<GameObject> _activeEnemies = new List<GameObject>();

        private void Start()
        {

            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                spawnPoints = new Transform[transform.childCount];
                for (int i = 0; i < transform.childCount; i++)
                    spawnPoints[i] = transform.GetChild(i);
            }

            if (spawnPoints.Length == 0)
            {
                Debug.LogError("[EnemySpawner] No spawn points set up. Add child GameObjects or assign them manually.");
                return;
            }

            if (enemyPrefab == null)
            {
                Debug.LogError("[EnemySpawner] Forgot to assign the enemy prefab!");
                return;
            }

            StartCoroutine(RunWaveCycle());
        }

        private IEnumerator RunWaveCycle()
        {
            yield return new WaitForSeconds(initialDelay);

            for (int i = 0; i < waves.Length; i++)
            {
                CurrentWaveIndex    = i;
                EnemiesKilledInWave = 0;
                WaveInProgress      = true;

                WaveConfig cfg = waves[i];
                Debug.Log($"[EnemySpawner] {cfg.waveName} starting — {cfg.totalEnemies} enemies, {cfg.enemyHealth} HP each");

                OnWaveStarted?.Invoke(i, cfg);

                // release enemies in batches until all is sent
                int spawned = 0;
                while (spawned < cfg.totalEnemies)
                {
                    int thisBatch = Mathf.Min(cfg.batchSize, cfg.totalEnemies - spawned);
                    SpawnBatch(thisBatch, cfg);
                    spawned += thisBatch;

                    if (spawned < cfg.totalEnemies)
                        yield return new WaitForSeconds(cfg.timeBetweenBatches);
                }

                // sit here and do nothing until every enemy from this wave is dead
                yield return new WaitUntil(() => EnemiesAlive == 0);

                WaveInProgress = false;
                OnWaveCompleted?.Invoke(i);
                Debug.Log($"[EnemySpawner] {cfg.waveName} cleared!");

                // short break before the next wave kicks off
                if (i < waves.Length - 1)
                    yield return new WaitForSeconds(delayBetweenWaves);
            }

            AllWavesComplete = true;
            OnAllWavesCompleted?.Invoke();
            Debug.Log("[EnemySpawner] All 5 waves done. Player survived.");
        }

        // spawns a group of enemies at random positions from our spawn point list
        private void SpawnBatch(int count, WaveConfig cfg)
        {
            for (int i = 0; i < count; i++)
            {
                Transform sp = spawnPoints[Random.Range(0, spawnPoints.Length)];

                Vector3 pos = sp.position + Vector3.up * 0.1f;
                GameObject enemy = Instantiate(enemyPrefab, pos, sp.rotation);

                ZombieHealth health = enemy.GetComponent<ZombieHealth>();
                if (health != null)
                    health.maxHealth = cfg.enemyHealth;

                // make them faster each wave by scaling their existing speeds
                ZombieAI ai = enemy.GetComponent<ZombieAI>();
                if (ai != null)
                {
                    ai.walkSpeed *= cfg.speedMultiplier;
                    ai.runSpeed  *= cfg.speedMultiplier;
                }

                EnemiesAlive++;
                _activeEnemies.Add(enemy);

                // death watcher so we get notified when this enemy dies
                EnemyDeathNotifier notifier = enemy.AddComponent<EnemyDeathNotifier>();
                notifier.Initialize(this, cfg.damageMultiplier);
            }
        }

        // called by EnemyDeathNotifier when one of our tracked enemies bites the dust
        public void OnEnemyDied(GameObject enemy)
        {
            if (_activeEnemies.Remove(enemy))
            {
                EnemiesAlive        = Mathf.Max(0, EnemiesAlive - 1);
                EnemiesKilledInWave++;
            }
        }

        private void OnDrawGizmos()
        {
            if (spawnPoints == null) return;
            Gizmos.color = Color.red;
            foreach (var sp in spawnPoints)
            {
                if (sp == null) continue;
                Gizmos.DrawWireSphere(sp.position, 0.5f);
                Gizmos.DrawLine(sp.position, sp.position + sp.forward * 1.5f);
            }
        }
    }

    
    [DisallowMultipleComponent]
    public class EnemyDeathNotifier : MonoBehaviour
    {
        private EnemySpawner _spawner;
        private ZombieHealth _health;
        private float        _damageMultiplier;

        public void Initialize(EnemySpawner spawner, float damageMultiplier)
        {
            _spawner          = spawner;
            _damageMultiplier = damageMultiplier;
        }

        private void Start()
        {
            _health = GetComponent<ZombieHealth>();
        }

        private void Update()
        {
            if (_health == null) return;

            // ZombieAI.Die() sets the "IsDead" animator bool,
            Animator anim = GetComponent<Animator>();
            if (anim != null && anim.GetBool("IsDead"))
            {
                _spawner.OnEnemyDied(gameObject);

                // wait 5 seconds so the death animation has time to play, then remove the whole object from the scene
                Destroy(gameObject, 5f);
                Destroy(this);
            }
        }
    }
}

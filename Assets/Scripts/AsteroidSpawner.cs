using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class AsteroidSpawner : MonoBehaviour
{
    public static AsteroidSpawner Instance;
    
    [Header("Configuration")]
    [SerializeField] private Asteroid[] asteroids;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] public Transform player;

    [Header("Spawn Options")]
    [SerializeField] private float frequency;
    [SerializeField] private int density;
    [SerializeField] private int densityRiseRate;
    [SerializeField] private float aggression;
    [SerializeField] private float aggressionRate;
    [SerializeField] private float spreadRange;

    [Header("Difficulty Caps")]
    [SerializeField] private int densityCap;
    [SerializeField] private float aggressionCap;

    [Header("Public Variables")]
    public int asteroidsDestroyed;
    public int asteroidsInWave;
    public int waves;
    
    private void Awake()
    {
        // Debug Safety Check
        if (asteroids.Length <= 0)
        {
            Debug.LogError("No Asteroid Prefabs Loaded");
            Debug.Break();
        }
        if (spawnPoints.Length <= 0)
        {
            Debug.LogError("No Spawn Points Loaded");
            Debug.Break();
        }

        asteroidsDestroyed = 0;
        asteroidsInWave = 0;
        waves = 0;

        Instance = this;
    }

    private void Start()
    {
        StartCoroutine(NextWave());
    }

    public void SpawnWave()
    {
        for (int i = 0; i < density; ++i)
        {
            // Get Indices
            int spawnIndex = Random.Range(0, spawnPoints.Length);
            int asteroidIndex = Random.Range(0, asteroids.Length);
            
            // Calculate Spread
            float spread = Random.Range(-spreadRange, spreadRange);
            // Get SpawnPoint from available points
            Transform spawnPoint = spawnPoints[spawnIndex];
            // Prefetch Spawn Position
            Vector3 spawnPosition = spawnPoint.position;
            // Calculate Direction To Player the Asteroid Takes with Random Spread
            Vector3 dirToPlayer = Quaternion.Euler(0,0,spread) * (player.position - spawnPosition);
            
            var asteroid = Instantiate(asteroids[asteroidIndex], spawnPosition, Quaternion.identity);

            // Random Aggression (velocity buff) to make Asteroids non-uniform;
            float aggressionRange = Random.Range(0f, 5f);
            asteroid.rigidbody.AddForce(dirToPlayer * (aggression + aggressionRange));
        }

        asteroidsInWave += density;
    }

    public void StartNextWave()
    { StartCoroutine(NextWave()); }

    private IEnumerator NextWave()
    {
        yield return new WaitForSeconds(frequency);
        SpawnWave();
        
        // Change Difficulty Per Wave
        density += densityRiseRate;
        density = Mathf.Clamp(density, 0, densityCap);

        // Change Aggression Per Wave
        aggression += aggressionRate;
        aggression = Mathf.Clamp(aggression, 0, aggressionCap);

        ++waves;
    }
}

using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Asteroid : MonoBehaviour
{
    private AsteroidSpawner spawner;
    [HideInInspector] public Rigidbody rigidbody;

    private void Awake()
    { rigidbody = GetComponent<Rigidbody>(); }
    
    private void Start()
    { spawner = AsteroidSpawner.Instance; }

    private void FixedUpdate()
    {
        Vector3 pos = transform.position;
        bool xBoundary = pos.x >= 11 || pos.x <= -11;
        bool yBoundary = pos.y >= 7 || pos.y <= -7;
        if (xBoundary || yBoundary)
            AsteroidDestroy();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Projectile"))
        {
            ++spawner.asteroidsDestroyed;
            AsteroidDestroy();
        }
    }

    private void AsteroidDestroy()
    {
        --spawner.asteroidsInWave;

        if (spawner.asteroidsInWave <= 0)
        { spawner.StartNextWave(); }
        
        // TODO Spawn Particle Effects
        Destroy(gameObject);
    }
}

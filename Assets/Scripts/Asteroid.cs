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
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Projectile"))
        {
            ++spawner.asteroidsDestroyed;
            --spawner.asteroidsInWave;

            if (spawner.asteroidsInWave <= 0)
            { spawner.StartNextWave(); }
            
            // TODO Spawn Particle Effects
            Destroy(gameObject);
        }
    }
}

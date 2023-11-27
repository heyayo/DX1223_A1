using System;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private int pierce = 0;

    private void FixedUpdate()
    {
        Vector3 pos = transform.position;
        bool xBoundary = pos.x >= 11 || pos.x <= -11;
        bool yBoundary = pos.y >= 7 || pos.y <= -7;
        if (xBoundary || yBoundary)
            Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Asteroid"))
        {
            --pierce;
            if (pierce <= 0)
                Destroy(gameObject);
        }
    }
}

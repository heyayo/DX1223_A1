using System;
using UnityEngine;

public class GameScript : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private PlayerData data;
    [SerializeField] private Transform spawnPoint;

    private void Start()
    {
        int shipIndex = Convert.ToInt32(data.selectedShipIndex);
        // Spawn Ship Selected
        var ship = Instantiate(
            data.shipsOwned[shipIndex],
            spawnPoint.position, spawnPoint.rotation);
        Transform shipt = ship.transform;
        shipt.rotation = Quaternion.Euler(-90,0,0);

        AsteroidSpawner.Instance.player = ship.transform;
    }
}

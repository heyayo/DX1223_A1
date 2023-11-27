using System;
using System.Collections;
using UnityEngine;
using TMPro;

public class GameScript : MonoBehaviour
{
    public static GameScript Instance;
    
    [Header("Configuration")]
    [SerializeField] private PlayerData data;
    [SerializeField] private Transform spawnPoint;

    [Header("Game UI")]
    [SerializeField] private TMP_Text statusText;

    private void Awake()
    { Instance = this; }
    
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

        StartCoroutine(TextCountdown());
    }

    public void EndGame()
    {
        var spawner = AsteroidSpawner.Instance;
        // Give AFK Achievement for dying without destroying anything
        if (spawner.asteroidsDestroyed <= 0)
            data.achievements.achievement_afk = true;
        spawner.asteroidsInWave += 999999; // Bump this Number so Asteroids don't start the next wave
        statusText.text = "DEATH";
    }

    private IEnumerator TextCountdown()
    {
        var spawner = AsteroidSpawner.Instance;
        for (int i = (int)spawner.frequency; i > spawner.frequency; ++i)
        {
            statusText.text = "Waves Spawning in: " + i.ToString();
            yield return new WaitForSeconds(1);
        }
    }
}

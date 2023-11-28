using System;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameScript : MonoBehaviour
{
    public static GameScript Instance;
    
    [Header("Configuration")]
    [SerializeField] private PlayerData data;
    [SerializeField] private Transform spawnPoint;

    [Header("Game UI")]
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Button restartText;
    [SerializeField] private Button goBackText;

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

        SetEndScreen(false);
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
        SetEndScreen(true);
        data.GiveCurrency(spawner.asteroidsDestroyed*10);
    }

    public void ReplayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene("GameMenus");
    }

    private void SetEndScreen(bool visibility)
    {
        restartText.gameObject.SetActive(visibility);
        goBackText.gameObject.SetActive(visibility);
        statusText.gameObject.SetActive(visibility);
    }

    private IEnumerator TextCountdown()
    {
        var spawner = AsteroidSpawner.Instance;
        statusText.gameObject.SetActive(true);
        for (int i = (int)spawner.grace; i > spawner.frequency; --i)
        {
            statusText.text = "Waves Spawning in: " + i.ToString();
            yield return new WaitForSeconds(1);
        }
        statusText.gameObject.SetActive(false);
    }
}

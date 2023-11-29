using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;

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

    private DateTime startTime;

    private void Awake()
    { Instance = this; }
    
    private void Start()
    {
        // Spawn Ship Selected
        var ship = Instantiate(
            data.selectedShip,
            spawnPoint.position, spawnPoint.rotation);
        Transform shipt = ship.transform;
        shipt.rotation = Quaternion.Euler(-90,0,0);

        AsteroidSpawner.Instance.player = ship.transform;

        SetEndScreen(false);
        StartCoroutine(TextCountdown());

        ++data.playStatistics.stat_matchesPlayed;
        startTime = DateTime.Now;
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
        data.playStatistics.stat_xp += (spawner.waves * 50) + (spawner.asteroidsDestroyed * 10);
        data.playStatistics.stat_level += data.playStatistics.stat_xp / 1000;
        data.playStatistics.stat_xp %= 1000;
        var delta = DateTime.Now - startTime;
        data.playStatistics.stat_minutesPlayed += delta.TotalMinutes;
        
        if (!data.isGuest)
            LeaderboardSubmitEvent(spawner.waves);

        if (data.playStatistics.stat_level >= 50)
            data.achievements.achievement_maxLevel = true;
        data.UploadStats();
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
    
    public void LeaderboardSubmitEvent(int score)
    {
        var req = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate
                {
                    StatisticName = "Scores",
                    Value = score
                }
            }
        };
        PlayFabClientAPI.UpdatePlayerStatistics(req,
            success =>
            {
                Debug.Log("Updated Scores Leaderboard");
            },
            failure =>
            {
                Debug.Log("Failed To Update Player Statistics | " + failure.ErrorMessage);
            }
            );
    }
}

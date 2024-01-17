using System.Collections.Generic;
using UnityEngine;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.Events;
using UnityEngine.UI;

public class LeaderboardControl : MonoBehaviour
{
    [Header("Leaderboard Panel Elements")]
    [SerializeField] private TMP_Text names;
    [SerializeField] private TMP_Text scores;
    [SerializeField] private TMP_Text positions;
    [SerializeField] private int rowCount;
    [SerializeField] private Button leaderboardButton;
    [SerializeField] private TMP_Text leaderboardButtonText;

    private delegate void APICall();
    private APICall _currentEvent;

    private void Start()
    {
        _currentEvent = LeaderboardEvent;
        gameObject.SetActive(false);
    }
    
    public void LeaderboardEvent()
    {
        var req = new GetLeaderboardRequest
            {
                StatisticName = "Scores",
                StartPosition = 0,
                MaxResultsCount = rowCount
            };
        PlayFabClientAPI.GetLeaderboard(req,
            success =>
            {
                Debug.Log("Successfully Fetched Leaderboard Data");
                PopulateLeaderboard(success.Leaderboard);
            },
            failure =>
            {
                Debug.Log("Failed To Fetch Leaderboard Data | " + failure.ErrorMessage);
            }
        );
    }

    public void NearbyLeaderboardEvent()
    {
        var req = new GetLeaderboardAroundPlayerRequest
        {
            StatisticName = "Scores",
            MaxResultsCount = rowCount
        };
        PlayFabClientAPI.GetLeaderboardAroundPlayer(req,
            success =>
            {
                Debug.Log("Successfully Fetched Nearby Leaderboard Data");
                PopulateLeaderboard(success.Leaderboard);
            },
            failure =>
            { Debug.LogError("Failed To Fetch Leaderboard Data \n" + failure.GenerateErrorReport()); }
            );
    }

    public void LeaderToggle()
    {
        if (_currentEvent == LeaderboardEvent)
        {
            _currentEvent = NearbyLeaderboardEvent;
            leaderboardButton.onClick.RemoveAllListeners();
            leaderboardButton.onClick.AddListener(NearbyLeaderboardEvent);
            leaderboardButtonText.text = "Nearby";
        }
        else
        {
            _currentEvent = LeaderboardEvent;
            leaderboardButton.onClick.RemoveAllListeners();
            leaderboardButton.onClick.AddListener(LeaderboardEvent);
            leaderboardButtonText.text = "Top";
        }
    }

    public void LeaderEvent()
    {
        _currentEvent();
    }

    private void PopulateLeaderboard(List<PlayerLeaderboardEntry> leaderboard)
    {
        names.text = "";
        scores.text = "";
        positions.text = "";
        for (int i = 0; i < leaderboard.Count; ++i)
        {
            names.text += leaderboard[i].DisplayName + '\n';
            scores.text += leaderboard[i].StatValue.ToString() + '\n';
            positions.text += (leaderboard[i].Position + 1).ToString() + '\n';
        }
    }
}

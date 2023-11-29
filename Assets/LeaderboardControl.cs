using UnityEngine;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;

public class LeaderboardControl : MonoBehaviour
{
    [Header("Leaderboard Panel Elements")]
    [SerializeField] private TMP_Text names;
    [SerializeField] private TMP_Text scores;
    [SerializeField] private TMP_Text positions;
    [SerializeField] private int rowCount;

    private void Start()
    { gameObject.SetActive(false); }
    
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
                Debug.Log("Succesfully Fetched Leaderboard Data");
                names.text = "";
                scores.text = "";
                positions.text = "";
                for (int i = 0; i < success.Leaderboard.Count; ++i)
                {
                    names.text += success.Leaderboard[i].DisplayName + '\n';
                    scores.text += success.Leaderboard[i].StatValue.ToString() + '\n';
                    positions.text += (success.Leaderboard[i].Position + 1).ToString() + '\n';
                    Debug.Log("Leaderboard Entry | " +
                              success.Leaderboard[i].DisplayName + " | " +
                              success.Leaderboard[i].StatValue + " | " +
                              success.Leaderboard[i].Position);
                }
            },
            failure =>
            {
                Debug.Log("Failed To Fetch Leaderboard Data | " + failure.ErrorMessage);
            }
        );
    }
}

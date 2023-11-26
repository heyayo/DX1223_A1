using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.UI;

public class AccountAPIManager : MonoBehaviour
{
    [SerializeField] private PlayerData playerData;
    [SerializeField] private MenuShifter menuShifter;
    
    [Header("Login Panel Elements")]
    [SerializeField] private TMP_InputField loginEmailField;
    [SerializeField] private TMP_InputField loginPasswordField;
    [SerializeField] private TMP_Text loginOutput;

    [Header("Register Panel Eleements")]
    [SerializeField] private TMP_InputField registerUsernameField;
    [SerializeField] private TMP_InputField registerPasswordField;
    [SerializeField] private TMP_InputField registerEmailField;
    [SerializeField] private TMP_Text registerOutput;

    [Header("Leaderboard Panel Elements")]
    [SerializeField] private TMP_Text names;
    [SerializeField] private TMP_Text scores;
    [SerializeField] private TMP_Text positions;
    [SerializeField] private TMP_Text leaderboardOutput;

    [Header("Leaderboard Configuration")]
    [SerializeField] private int startingPosition = 0;
    [SerializeField] private int rowCount = 10;
    
    [Header("Password Visibility Assets")]
    [SerializeField] private Sprite closedEye;
    [SerializeField] private Sprite openEye;

    [Header("DEBUG ELEMENTS")]
    [SerializeField] private TMP_InputField cheatScore;
    
    public void LoginEvent()
    {
        var req = new LoginWithEmailAddressRequest
        {
            Email = loginEmailField.text,
            Password = loginPasswordField.text,
        };
        PlayFabClientAPI.LoginWithEmailAddress(req,
            success =>
            {
                Debug.Log("Login Success | " + success.PlayFabId);
                menuShifter.GoGame();
                loginEmailField.text = "";
                loginPasswordField.text = "";
                loginOutput.text = "";
            },
            failure =>
            {
                Debug.Log("Failed To Login | " + failure.ErrorMessage);
                loginOutput.text = "Invalid Credentials";
            }
        );
    }

    public void LoginUsernameEvent()
    { Login(loginEmailField.text,loginPasswordField.text,
        () =>
        {
            loginEmailField.text = "";
            loginPasswordField.text = "";
            loginOutput.text = "";
        },
        () => { loginOutput.text = "Invalid Credentials"; });
    }

    private void Login(string username, string password, Action successAction = null, Action failAction = null)
    {
        var req = new LoginWithPlayFabRequest
        {
            Username = username,
            Password = password,
        };
        PlayFabClientAPI.LoginWithPlayFab(req,
            success =>
            {
                Debug.Log("Login Success | " + success.PlayFabId);
                if (successAction != null) successAction();
                menuShifter.GoGame();
            },
            failure =>
            {
                Debug.Log("Failed To Login | " + failure.ErrorMessage);
                if (failAction != null) failAction();
            }
            );
    }

    public void RegisterEvent()
    {
        var req = new RegisterPlayFabUserRequest
        {
            Email = registerEmailField.text,
            Password = registerPasswordField.text,
            Username = registerUsernameField.text,
            DisplayName = registerUsernameField.text
        };
        PlayFabClientAPI.RegisterPlayFabUser(req,
            success =>
            {
                Debug.Log("Register Success ID | " + success.PlayFabId);
                registerOutput.text = "Successfully Registered";

                playerData.playStatistics.stat_minutesPlayed = 0;
                playerData.playStatistics.stat_matchesPlayed = 0;
                Login(registerUsernameField.text,registerPasswordField.text,
                    () => { playerData.UploadStats(); });
            },
            failure =>
            {
                Debug.Log("Failed To Register | " + failure.ErrorMessage + " | " + failure.Error.ToString());
                registerOutput.text = "Failed To Register";
                switch (failure.Error)
                {
                    case PlayFabErrorCode.UsernameNotAvailable:
                        registerOutput.text += " | Invalid Username";
                        break;
                    case PlayFabErrorCode.NameNotAvailable:
                        registerOutput.text += " | Username is taken";
                        break;
                    case PlayFabErrorCode.EmailAddressNotAvailable:
                        registerOutput.text += " | Email is in use";
                        break;
                    case PlayFabErrorCode.InvalidEmailAddress:
                        registerOutput.text += " | Invalid Email Address";
                        break;
                    default:
                        registerOutput.text += " | " + failure.ErrorMessage;
                        break;
                }
            });
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

                leaderboardOutput.text = "Leaderboard Data Fetched";
            },
            failure =>
            {
                Debug.Log("Failed To Fetch Leaderboard Data | " + failure.ErrorMessage);
                leaderboardOutput.text = "Unable To Fetch Leaderboards | " + failure.ErrorMessage;
            }
        );
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

    public void LogoutEvent()
    {
        PlayFabClientAPI.ForgetAllCredentials();
    }

    public void PasswordFieldVisibilityToggle(TMP_InputField inputField)
    {
        switch (inputField.contentType)
        {
            case TMP_InputField.ContentType.Standard:
                inputField.contentType = TMP_InputField.ContentType.Password;
                break;
            case TMP_InputField.ContentType.Password:
                inputField.contentType = TMP_InputField.ContentType.Standard;
                break;
            default:
                inputField.contentType = TMP_InputField.ContentType.Password;
                break;
        }
        inputField.ForceLabelUpdate();
    }

    public void PasswordIconToggle(Image image)
    {
        if (image.sprite == closedEye)
            image.sprite = openEye;
        else
            image.sprite = closedEye;
    }

    public void DEBUG_AUTOLOGIN()
    {
        var req = new LoginWithEmailAddressRequest()
        {
            Email = "loser@mail.com",
            Password = "supperman"
        };
        PlayFabClientAPI.LoginWithEmailAddress(req,
            success =>
            {
                Debug.Log("DEBUG SIGNED IN | " + success.PlayFabId);
                menuShifter.GoGame();
            },
            failure => { Debug.Log("DEBUG SIGN IN FAILED | " + failure.ErrorMessage);}
                );
    }

    public void DEBUG_CHEATSCORE()
    {
        LeaderboardSubmitEvent(Convert.ToInt32(cheatScore.text));
    }

    public void DEBUG_SETUPPLAYERSTATS()
    {
        var req = new LoginWithEmailAddressRequest()
        {
            Email = "loser@mail.com",
            Password = "supperman"
        };
        PlayFabClientAPI.LoginWithEmailAddress(req,
            success =>
            {
                Debug.Log("DEBUG SIGNED IN | " + success.PlayFabId);
                playerData.playStatistics.stat_minutesPlayed = 2;
                playerData.playStatistics.stat_matchesPlayed = 5;
                playerData.UploadStats();
            },
            failure => { Debug.Log("DEBUG SIGN IN FAILED | " + failure.ErrorMessage);}
            );
    }

    public void DEBUG_LOADPLAYERSTATS()
    {
        var req = new LoginWithEmailAddressRequest()
        {
            Email = "loser@mail.com",
            Password = "supperman"
        };
        PlayFabClientAPI.LoginWithEmailAddress(req,
            success =>
            {
                Debug.Log("DEBUG SIGNED IN | " + success.PlayFabId);
                playerData.FetchStats();
            },
            failure => { Debug.Log("DEBUG SIGN IN FAILED | " + failure.ErrorMessage);}
            );
    }
}

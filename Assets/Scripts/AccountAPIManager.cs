using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.UI;

public class AccountAPIManager : MonoBehaviour
{
    [SerializeField] private PlayFabSharedSettings settings;
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

    [Header("Reset Password Elements")]
    [SerializeField] private TMP_InputField resetEmailField;
    [SerializeField] private TMP_Text resetStatusText;
    [SerializeField] private TMP_Text resetButtonText;
    [SerializeField] private Button resetButton;
    
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

    private static bool loggingIn = false;
    private void Login(string username, string password, Action successAction = null, Action failAction = null)
    {
        if (loggingIn) return;
        loggingIn = true;
        var req = new LoginWithPlayFabRequest
        {
            Username = username,
            Password = password,
        };
        PlayFabClientAPI.LoginWithPlayFab(req,
            success =>
            {
                loggingIn = false;
                Debug.Log("Login Success | " + success.PlayFabId);
                if (successAction != null) successAction();
                menuShifter.GoGame();
                playerData.displayName = username;
                playerData.isGuest = false;
            },
            failure =>
            {
                loggingIn = false;
                Debug.Log("Failed To Login | " + failure.ErrorMessage);
                if (failAction != null) failAction();
            }
            );
    }

    private static bool anonymous = false;
    public void AnonymousLoginEvent()
    {
        if (anonymous) return;
        anonymous = true;
        
        PlayFabClientAPI.LoginWithCustomID(new LoginWithCustomIDRequest
        {
            CreateAccount = true,
            CustomId = "guest"
        },
        success =>
        {
            Debug.Log("Anonymously Logged In | " + success.PlayFabId);
            menuShifter.GoGame();
            anonymous = false;
            playerData.displayName = "guest";
            playerData.isGuest = true;
        },
        failure =>
        {
            Debug.LogError(failure.GenerateErrorReport());
            anonymous = false;
        }
        );
    }

    private static bool registering = false;
    public void RegisterEvent()
    {
        if (registering) return;
        registering = true;
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
                registering = false;
                Debug.Log("Register Success ID | " + success.PlayFabId);
                registerOutput.text = "Successfully Registered";

                playerData.playStatistics.stat_minutesPlayed = 0;
                playerData.playStatistics.stat_matchesPlayed = 0;
                Login(registerUsernameField.text,registerPasswordField.text);
            },
            failure =>
            {
                registering = false;
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

    public void LogoutEvent()
    {
        PlayFabClientAPI.ForgetAllCredentials();
    }

    public void ResetPasswordEvent()
    {
        PlayFabClientAPI.SendAccountRecoveryEmail(new SendAccountRecoveryEmailRequest
        {
            Email = resetEmailField.text,
            TitleId = settings.TitleId
        },
        success =>
        {
            Debug.Log("Email Sent to " + resetEmailField.text);
            resetStatusText.text = "Password Reset Sent";
        },
        failure =>
        {
            Debug.LogError(failure.GenerateErrorReport());
            Debug.LogError(failure.Error.ToString());
            StartCoroutine(DenyReset());
            resetStatusText.text = "Invalid Email";
        }
        );
    }

    private IEnumerator DenyReset()
    {
        resetButton.enabled = false;
        resetButtonText.text = "DENIED";
        yield return new WaitForSeconds(1);
        resetButton.enabled = true;
        resetButtonText.text = "Reset";
    }

    public void ResetText(TMP_Text text)
    { text.text = ""; }

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

    private static bool autologgingin = false;
    public void DEBUG_AUTOLOGIN()
    {
        if (autologgingin) return;
        autologgingin = true;
        var req = new LoginWithEmailAddressRequest()
        {
            Email = "loser@mail.com",
            Password = "supperman"
        };
        PlayFabClientAPI.LoginWithEmailAddress(req,
            success =>
            {
                autologgingin = false;
                Debug.Log("DEBUG SIGNED IN | " + success.PlayFabId);
                menuShifter.GoGame();
            },
            failure =>
            {
                autologgingin = false;
                Debug.Log("DEBUG SIGN IN FAILED | " + failure.ErrorMessage);
            }
                );
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
                playerData.FetchStats(()=>{});
            },
            failure => { Debug.Log("DEBUG SIGN IN FAILED | " + failure.ErrorMessage);}
            );
    }
}

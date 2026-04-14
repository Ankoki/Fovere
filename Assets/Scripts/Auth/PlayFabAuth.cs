using System;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using LoginResult = PlayFab.ClientModels.LoginResult;
using RegisterResult = PlayFab.ClientModels.RegisterPlayFabUserResult;

public enum AuthTypes
{
    None,
    EmailAndPassword,
    RegisterPlayFabAccount
}

public class PlayFabAuth
{
    
    public delegate void DisplayAuthEvent();
    public static event DisplayAuthEvent OnDisplayAuth;
    
    public delegate void LoginSuccessEvent(LoginResult result);
    public static event LoginSuccessEvent OnLoginSuccess;
    
    public delegate void RegisterSuccessEvent(RegisterResult result);
    public static event RegisterSuccessEvent OnRegisterSuccess;
    
    public delegate void AuthErrorEvent(PlayFabError error);
    public static event AuthErrorEvent OnAuthError;

    public string email;
    public string username;
    public string password;
    public string authTicket;
    public GetPlayerCombinedInfoRequestParams requestParams;
    
    private static string PlayFabId { get; set; }

    private static string SessionTicket { get; set; }

    private const string LoginRememberKey = "PlayFabLoginRemember";
    private const string PlayFabRememberMeIdKey = "PlayFabLoginRememberMeId";
    private const string PlayFabAuthTypeKey = "PlayFabAuthType";

    public static PlayFabAuth Instance
    {
        get
        {
            return _instance ??= new PlayFabAuth();
        }
    }
    private static PlayFabAuth _instance;

    private PlayFabAuth()
    {
        _instance = this;
    }

    /// <summary>
    /// Remember the user for the next time they log in.
    /// This is used for auto-login.
    /// </summary>
    public bool RememberMe
    {
        get => PlayerPrefs.GetInt(LoginRememberKey, 0) != 0;
        set => PlayerPrefs.SetInt(LoginRememberKey, value ? 1 : 0);
    }

    /// <summary>
    /// Remember the type of authentication for the user.
    /// </summary>
    public AuthTypes AuthType
    {
        get => (AuthTypes) PlayerPrefs.GetInt(PlayFabAuthTypeKey, 0);
        set => PlayerPrefs.SetInt(PlayFabAuthTypeKey, (int) value);
    }

    /// <summary>
    /// Generated Remember Me ID.
    /// Pass null for an auto-generated ID.
    /// </summary>
    private string RememberMeId
    {
        get => PlayerPrefs.GetString(PlayFabRememberMeIdKey, string.Empty);
        set
        {
            var guid = value ?? Guid.NewGuid().ToString();
            PlayerPrefs.SetString(PlayFabRememberMeIdKey, guid);
        }
    }

    /// <summary>
    /// Clears all data linked to auto-logins.
    /// </summary>
    public void ClearRememberMe()
    {
        PlayerPrefs.DeleteKey(LoginRememberKey);
        PlayerPrefs.DeleteKey(PlayFabRememberMeIdKey);
        PlayerPrefs.DeleteKey(PlayFabAuthTypeKey);
    }

    /// <summary>
    /// Authenticates the player with the given authentication type.
    /// </summary>
    /// <param name="authType">The type of authentication to use.</param>
    public void Authenticate(AuthTypes authType)
    {
        AuthType = authType;
        Authenticate();
    }

    public void Authenticate()
    {
        switch (AuthType)
        {
            case AuthTypes.None:
                    if (OnDisplayAuth != null)
                        OnDisplayAuth.Invoke();
                    break;
            case AuthTypes.EmailAndPassword:
                AuthenticateEmailPassword();
                break;
            case AuthTypes.RegisterPlayFabAccount:
                AddAccountAndPassword();
                break;
        }
    }

    private void AuthenticateEmailPassword()
    {
        if (RememberMe && !string.IsNullOrEmpty(RememberMeId))
        {
            PlayFabClientAPI.LoginWithCustomID(
                new LoginWithCustomIDRequest
                {
                    TitleId = PlayFabSettings.TitleId,
                    CustomId = RememberMeId,
                    CreateAccount = true,
                    InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
                    {
                        GetUserAccountInfo = true
                    }
                },

                result =>
                {
                    PlayFabId = result.PlayFabId;
                    SessionTicket = result.SessionTicket;
                    OnLoginSuccess?.Invoke(result);
                },

                error => OnAuthError?.Invoke(error)
            );
            return;
        }

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            OnDisplayAuth?.Invoke();
            return;
        }

        PlayFabClientAPI.LoginWithEmailAddress(
            new LoginWithEmailAddressRequest
            {
                TitleId = PlayFabSettings.TitleId,
                Email = email,
                Password = password,
                InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
                {
                    GetUserAccountInfo = true
                }
            },

            result =>
            {
                PlayFabId = result.PlayFabId;
                SessionTicket = result.SessionTicket;

                if (RememberMe)
                {
                    RememberMeId = Guid.NewGuid().ToString();
                    AuthType = AuthTypes.EmailAndPassword;
                    PlayFabClientAPI.LinkCustomID(
                        new LinkCustomIDRequest
                        {
                            CustomId = RememberMeId,
                            ForceLink = false
                        },
                        null,
                        null
                    );
                }

                OnLoginSuccess?.Invoke(result);
            },
            error => OnAuthError?.Invoke(error));
    }

    /// <summary>
    /// Register a user with an e-mail and password.
    /// </summary>
    private void AddAccountAndPassword()
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            OnDisplayAuth?.Invoke();
            return;
        }
        
        PlayFabClientAPI.RegisterPlayFabUser(new RegisterPlayFabUserRequest
            {
                TitleId =  PlayFabSettings.TitleId,
                DisplayName = username,
                Username = username,
                Email = email,
                Password = password,
                InfoRequestParameters = requestParams
            },

            result =>
            {
                PlayFabId = result.PlayFabId;
                SessionTicket = result.SessionTicket;
                OnRegisterSuccess?.Invoke(result);
            },
            
            error => OnAuthError?.Invoke(error));
    }
    
}
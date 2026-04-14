using Misc.Settings;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using RegisterResult = PlayFab.ClientModels.RegisterPlayFabUserResult;

public class LoginView : MonoBehaviour
{

    public bool clearPlayerPrefs;
    public Toggle rememberMe;
    
    public Button cancelRegisterButton;
    public Button clearSignInButton;

    [Header("Root Auth")]
    public GameObject authPanel;
    public Button directLoginButton;
    public Button directRegisterButton;
    
    [Header("Register Panel")]
    public GameObject registerPanel;
    public TMP_InputField registerEmail;
    public TMP_InputField registerUsername;
    public TMP_InputField registerPassword;
    public TMP_InputField registerPasswordConfirm;
    public Button registerButton;
    public TMP_Text registerStatusText;
    
    [Header("Login Panel")]
    public GameObject loginPanel;
    public TMP_InputField loginEmail;
    public TMP_InputField loginPassword;
    public Button loginButton;
    public TMP_Text loginStatusText;

    public GetPlayerCombinedInfoRequestParams requestParams;
    
    private readonly PlayFabAuth _auth = PlayFabAuth.Instance;

    public void Awake()
    {
        if (clearPlayerPrefs) // To 'reset' during testing.
        {
            _auth.ClearRememberMe();
            _auth.AuthType = AuthTypes.None;
        }
        rememberMe.isOn = _auth.RememberMe;
        rememberMe.onValueChanged.AddListener(toggle => _auth.RememberMe = toggle);
    }

    private void Start()
    {
        loginPanel.SetActive(false);
        registerPanel.SetActive(false);
        authPanel.SetActive(true);

        PlayFabAuth.OnDisplayAuth += DisplayAuth;
        PlayFabAuth.OnRegisterSuccess += OnRegisterSuccess;
        PlayFabAuth.OnLoginSuccess += OnLoginSuccess;
        PlayFabAuth.OnAuthError += OnAuthError;
        
        directLoginButton.onClick.AddListener(DisplayLogin);
        directRegisterButton.onClick.AddListener(DisplayRegister);
        
        loginButton.onClick.AddListener(OnLoginClicked);
        registerButton.onClick.AddListener(OnRegisterClicked);
        cancelRegisterButton.onClick.AddListener(OnCancelRegisterClicked);
        clearSignInButton.onClick.AddListener(OnClearSignInClicked);
        
        _auth.requestParams = requestParams;
    }

    public void DisplayAuth()
    {
        registerPanel.SetActive(false);
        loginPanel.SetActive(false);
        authPanel.SetActive(true);
    }

    public void DisplayLogin()
    {
        authPanel.SetActive(false);
        registerPanel.SetActive(false);
        loginPanel.SetActive(true);
        loginStatusText.text = string.Empty;
        Debug.Log("Login view displaying.");
    }

    public void DisplayRegister()
    {
        authPanel.SetActive(false);
        loginPanel.SetActive(false);
        registerPanel.SetActive(true);
        registerStatusText.text = string.Empty;
        Debug.Log("Register view displaying.");
    }

    private void OnRegisterSuccess(RegisterResult result)
    {
        registerStatusText.text = "You have successfully registered! Now go and sign in.";
    }

    private void OnLoginSuccess(LoginResult result)
    {
        loginStatusText.text = string.Empty;
        loginPanel.SetActive(false);
        // PlayerData loading logic.
        var entityId = result.EntityToken.Entity.Id;
        var username = result.InfoResultPayload.AccountInfo.Username ?? result.PlayFabId;
        if (FovereSettings.DebugMode)
            Debug.Log("Successfully Logged In as: " + username);
        // TODO show loading screen.
        var playerData = new PlayerData(entityId); // Load player data before the scene, world generation will be effected.
        SceneManager.LoadScene("Scenes/GameScene", LoadSceneMode.Single); // Might be additive in the future to further simplify layers.
        
        
    }

    private void OnAuthError(PlayFabError error)
    {
        switch (error.Error)
        {
            case PlayFabErrorCode.InvalidEmailAddress:
            case PlayFabErrorCode.InvalidPassword:
            case PlayFabErrorCode.InvalidEmailOrPassword:
                loginStatusText.text = "Invalid E-mail or Password.";
                break;
            case PlayFabErrorCode.AccountNotFound:
                loginStatusText.text = "Account not found. Try registering.";
                break;
            default:
                loginStatusText.text = error.GenerateErrorReport();
                break;
        }

        if (FovereSettings.DebugMode)
        {
            Debug.Log(error.Error);
            Debug.LogError(error.GenerateErrorReport());
        }
    }

    private void OnLoginClicked()
    {
        loginStatusText.text = "Logging in as " + loginEmail.text + "...";
        _auth.email = loginEmail.text;
        _auth.password = loginPassword.text;
        _auth.Authenticate(AuthTypes.EmailAndPassword);
    }

    private void OnRegisterClicked()
    {
        if (registerPassword.text != registerPasswordConfirm.text)
        {
            registerStatusText.text = "The given passwords don't match!";
            return;
        }

        loginStatusText.text = "Registering user " + registerUsername.text + "...";
        _auth.email = registerEmail.text;
        _auth.username = registerUsername.text;
        _auth.password = registerPassword.text;
        _auth.Authenticate(AuthTypes.RegisterPlayFabAccount);
    }

    private void OnCancelRegisterClicked()
    {
        registerEmail.text = string.Empty;
        registerUsername.text = string.Empty;
        registerPassword.text = string.Empty;
        registerPasswordConfirm.text = string.Empty;
        
        registerPanel.SetActive(false);
        authPanel.SetActive(true);
    }

    private void OnClearSignInClicked()
    {
        _auth.ClearRememberMe();
        loginStatusText.text = "Sign-in information cleared.";
    }
    
}
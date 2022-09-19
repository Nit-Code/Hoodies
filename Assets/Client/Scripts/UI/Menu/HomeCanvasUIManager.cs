using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SharedScripts;
using SharedScripts.DataId;

public class HomeCanvasUIManager : MonoBehaviour
{
    [SerializeField] private GameObject myHomePanel;
    [SerializeField] private GameObject myMainMenuArea;
    [SerializeField] private GameObject myPrivateMatchArea;
    [SerializeField] private GameObject myOptionsArea;

    [SerializeField] private GameObject myFindingMatchPopup;
    [SerializeField] private GameObject myLobbyClosedPopup;
    [SerializeField] private GameObject myCreatingLobbyPopup;
    [SerializeField] private GameObject mySomethingWentWrongPopup;

    // input
    [SerializeField] private Slider myMasterSlider;
    [SerializeField] private Slider myMusicSlider;
    [SerializeField] private Slider myAmbientSlider;
    [SerializeField] private Slider mySoundSlider;
    [SerializeField] private TMP_InputField myShortPrivateLobbyIdInputField;

    // text
    [SerializeField] private TextMeshProUGUI myTitleText;

    // reference
    [SerializeField] private MenuSceneUIManager myMenuSceneUIManagerReference;
    private AudioManager myAudioManagerReference;
    private AuthenticationManager myAuthenticationManagerReference;

    private void Awake()
    {
        myFindingMatchPopup.SetActive(false);
    }

    private void Start()
    {
        myAuthenticationManagerReference = FindObjectOfType<AuthenticationManager>();
        myAudioManagerReference = FindObjectOfType<AudioManager>();
        //TODO: add error logs if references arent found

        ShowMainMenuArea();
    }

    public void PlaySound(AudioId anAudioId)
    {
        if (myAudioManagerReference == null || anAudioId == AudioId.INVALID)
        {
            Debug.LogError("[HOOD][CLIENT][SCENE] - PlayClickSound()");
        }
        else
        {
            myAudioManagerReference.PlaySound(anAudioId);
        }
    }

    // Main Menu
    public void ShowMainMenuArea()
    {
        if (myMainMenuArea != null)
        {
            HidePrivateMatchArea();
            HideOptionsArea();

            myHomePanel.SetActive(true);
            myMainMenuArea.SetActive(true);
            myTitleText.SetText("HOODIES");
            myTitleText.fontSize = 180;
        }
    }

    private void UnloadMainMenuAreaUI()
    {
        if (myMainMenuArea != null)
        {
            myMainMenuArea.SetActive(false);
        }
    }

    public void OnPrivateMatchButton()
    {
        PlaySound(AudioId.SOUND_MENU_CLICK);

        if (myPrivateMatchArea != null)
        {
            UnloadMainMenuAreaUI();

            myPrivateMatchArea.SetActive(true);
            myTitleText.SetText("PRIVATE MATCH");
            myTitleText.fontSize = 120;
        }
    }

    private void HidePrivateMatchArea()
    {
        if (myPrivateMatchArea != null)
        {
            myPrivateMatchArea.SetActive(false);
        }
    }

    public void LoadPublicLobbyCanvas()
    {
        if (myMenuSceneUIManagerReference != null)
        {
            myMenuSceneUIManagerReference.LoadPublicLobbyCanvas();
        }
    }

    private void HideOptionsArea()
    {
        if (myOptionsArea != null)
        {
            myOptionsArea.SetActive(false);
        }
    }

    #region OnInput
    public void OnMyDecksButton()
    {
        PlaySound(AudioId.SOUND_MENU_CLICK);
        Debug.LogWarning("[HOOD][UI] - Merge in progress, doing nothing.");
        // TODO MERGE
        /*
        if (myMenuSceneUIManagerReference != null)
        {
            myMenuSceneUIManagerReference.LoadMyDecksCanvas();
        }
        */
    }

    public void OnHostMatchButton()
    {
        Debug.Log("[HOOD][CLIENT][PRIVATE_LOBBY][HOST] - Step 1/9 - User input detected.");
        if (myMenuSceneUIManagerReference == null)
        {
            PlaySound(AudioId.SOUND_ERROR);
            Debug.LogError("[HOOD][CLIENT][PRIVATE_LOBBY][HOST] - Step 1/9 - Missing reference");
            return;
        }

        PlaySound(AudioId.SOUND_MENU_CLICK);
        myMenuSceneUIManagerReference.LoadPrivateLobbyCanvas(PlayerType.HOST, "");
        ShowCreatingLobbyPopup();
    }

    public void OnJoinMatchButton()
    {
        Debug.Log("[HOOD][CLIENT][PRIVATE_LOBBY][GUEST] - Step 1/9 - User input detected.");
        string matchId = myShortPrivateLobbyIdInputField.text;
        if (string.IsNullOrEmpty(matchId) && !CLU.GetIsConnectLocalEnabled())
        {
            PlaySound(AudioId.SOUND_ERROR);
            Debug.LogError("[HOOD][CLIENT][PRIVATE_LOBBY][GUEST] - Step 1/9 - Missing match id.");
            return;
        }

        myMenuSceneUIManagerReference.LoadPrivateLobbyCanvas(PlayerType.GUEST, myShortPrivateLobbyIdInputField.text);
        ShowFindingMatchPopup();
    }

    public void OnOptionsButton()
    {
        PlaySound(AudioId.SOUND_MENU_CLICK);

        Debug.LogWarning("[HOOD][UI] - Merge in progress, doing nothing.");
        // TODO MERGE
        /*
        if (myOptionsArea != null)
        {
            UnloadMainMenuAreaUI();

            myOptionsArea.SetActive(true);
            myTitleText.SetText("OPTIONS");
            myTitleText.fontSize = 180;
        }
        */
    }

    public void OnLogOutButton()
    {
        PlaySound(AudioId.SOUND_MENU_CLICK);

        Debug.Log("[HOOD][UI] - HomeCanvasUIManager/LogOut");
        HideOptionsArea();
        HidePrivateMatchArea();
        UnloadMainMenuAreaUI();

        myAuthenticationManagerReference.SignOut();
        myMenuSceneUIManagerReference.NavigateToLoginScene();
    }

    public void OnQuitGameButton()
    {
        PlaySound(AudioId.SOUND_MENU_CLICK);
        myMenuSceneUIManagerReference.Quit();
    }

    public void OnFindMatchButton()
    {
        PlaySound(AudioId.SOUND_MENU_CLICK);
        //TODO MERGE
        Debug.LogWarning("[HOOD][UI] - Merge in progress, doing nothing.");
        //myMenuSceneUIManagerReference.LoadPublicLobbyCanvas();
    }

    public void OnPrivateMatchAreaBackButton()
    {
        PlaySound(AudioId.SOUND_MENU_CLICK);
        ShowMainMenuArea();
    }

    public void OnOptionsAreaBackButton()
    {
        PlaySound(AudioId.SOUND_MENU_CLICK);
        ShowMainMenuArea();
    }

    public void OnCreatingLobbyPopupDismissButton()
    {
        Debug.Log("[HOOD][CLIENT][SCENE] - OnCreatingLobbyPopupDismissButton() not implemented");
    }

    public void OnFindingMatchPopupDismissButton()
    {
        Debug.Log("[HOOD][CLIENT][SCENE] - OnFindingMatchPopupDismissButton() not implemented");
    }

    public void OnLobbyClosedPopupDismissButton()
    {
        HideLobbyClosedPopup();
    }

    public void OnSomethingWentWrongDismissButton()
    {
        HideSomethingWentWrongPopup();
    }

    // OPTIONS
    public void HandleMasterVolumeSlider()
    {
        myAudioManagerReference.ChangeMasterVolume(myMasterSlider.value);
    }

    public void HandleMusicVolumeSlider()
    {
        myAudioManagerReference.ChangeMusicMasterVolume(myMusicSlider.value);
    }

    public void HandleAmbientVolumeSlider()
    {
        myAudioManagerReference.ChangeAmbientMasterVolume(myAmbientSlider.value);
    }

    public void HandleSoundVolumeSlider()
    {
        myAudioManagerReference.ChangeSoundVolume(mySoundSlider.value);
    }
    #endregion

    #region Popups
    // Popups
    public void ShowLobbyClosedPopup()
    {
        myLobbyClosedPopup.SetActive(true);
    }

    public void HideLobbyClosedPopup()
    {
        myLobbyClosedPopup.SetActive(false);
    }

    public void ShowFindingMatchPopup()
    {
        myFindingMatchPopup.SetActive(true);
    }

    public void HideFindingMatchPopup()
    {
        myFindingMatchPopup.SetActive(false);
    }

    public void ShowCreatingLobbyPopup()
    {
        myCreatingLobbyPopup.SetActive(true);
    }

    public void HideCreatingLobbyPopup()
    {
        myCreatingLobbyPopup.SetActive(false);
    }

    public void ShowSomethingWentWrongPopup()
    {
        mySomethingWentWrongPopup.SetActive(true);
    }

    public void HideSomethingWentWrongPopup()
    {
        mySomethingWentWrongPopup.SetActive(false);
    }
    #endregion

  

}
using SharedScripts;
using SharedScripts.DataId;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyCanvasUIManager : MonoBehaviour
{
    private bool myPlayerInputLocked = false;
    private bool myLobbyCountdownIsRunning = false;
    private Coroutine myCountdownCoroutine;

    // sub menu

    // panel
    [SerializeField] GameObject myPlayerPanel;
    [SerializeField] GameObject myOpponentPanel;

    // ???
    [SerializeField] private GameObject myPrivateMatchInfo; // TODO: Info?
    [SerializeField] private Image myCountdownBorder;

    // canvas
    [SerializeField] GameObject myOpponentContainerCanvas;

    // input
    [SerializeField] private Button myReadyButton;
    [SerializeField] private Button myLeaveLobbyButton;
    [SerializeField] private Toggle myPlayerReadyToggle;
    [SerializeField] private Toggle myOpponentReadyToggle;
    [SerializeField] private TMP_Dropdown myDecksDropdown;

    // text
    [SerializeField] private TextMeshProUGUI myShortLobbyIdText;
    [SerializeField] private TextMeshProUGUI myLocalPlayerNameText;
    [SerializeField] private TextMeshProUGUI myOpponentPlayerNameText;
    [SerializeField] private TextMeshProUGUI myOpponentReadyText;
    [SerializeField] private TextMeshProUGUI myCountdownTimerText;
    [SerializeField] private TextMeshProUGUI myLockedInText;

    // reference
    [SerializeField] MenuSceneUIManager myMenuSceneUIManager;
    private AudioManager myAudioManagerReference;

    public void Start()
    {
        Debug.Log("[HOOD][CLIENT][SCENE] - LobbyCanvasUIManager Init()");

        myAudioManagerReference = FindObjectOfType<AudioManager>();
        if (myAudioManagerReference == null)
        {
            Debug.LogError("[HOOD][CLIENT][SCENE] - myAudioManagerReference not found");
        }
    }

    private void OnEnable()
    {
        ResetLobbyUI();
    }

    public void OnLeaveButton()
    {
        if (myPlayerInputLocked)
        {
            return;
        }

        PlayClickSound();
        myMenuSceneUIManager.DisconnectFromLobby();
    }

    public void OnReadyButton()
    {
        if (myPlayerInputLocked)
        {
            return;
        }

        PlayClickSound();
        myPlayerReadyToggle.isOn = !myPlayerReadyToggle.isOn;
        myMenuSceneUIManager.GetNetworkClient().SendReadyStatus(myPlayerReadyToggle.isOn, 1); // deck hardcoded for testing
    }

    public void SetupLobbyUI(MatchType aMatchType, string aLocalPlayerName)
    {
        ResetLobbyUI();

        switch (aMatchType)
        {
            case MatchType.INVALID:
                break;
            case MatchType.PUBLIC:
                break;
            case MatchType.PRIVATE:
                myPrivateMatchInfo.SetActive(true);
                break;
            default:
                break;
        }

        SetShortLobbyWaitingFeedback();
        myLocalPlayerNameText.text = aLocalPlayerName;
    }

    public void SetShortLobbyId(string aShortLobbyId) 
    {
        if (string.IsNullOrEmpty(aShortLobbyId)) 
        {
            Shared.LogError("[HOOD][CLIENT][SCENE] - SetShortLobbyId()");
            myShortLobbyIdText.text = "Error.";
        }
        myShortLobbyIdText.text = aShortLobbyId;
    }

    public void SetShortLobbyWaitingFeedback()
    {
        //TODO: add some sort of animation while the short lobby id hasn't arrived.
        myShortLobbyIdText.text = "...";
    }

    public void ResetLobbyUI()
    {
        myOpponentContainerCanvas.SetActive(false);
        myReadyButton.enabled = false;
        myPlayerReadyToggle.isOn = false;
        myOpponentReadyText.enabled = false;
        myOpponentReadyToggle.isOn = false;
        myCountdownTimerText.enabled = false;
        myLockedInText.enabled = false;
        myCountdownBorder.color = new Color32(170, 232, 255, 255);
        myCountdownTimerText.color = new Color32(170, 232, 255, 255);
        myPlayerInputLocked = false;
        myDecksDropdown.ClearOptions();
    }

    public void PlayClickSound()
    {
        if (myAudioManagerReference != null)
        {
            myAudioManagerReference.PlaySound(AudioId.SOUND_MENU_CLICK);
        }
        else
        {
            Debug.LogError("[HOOD][CLIENT][SCENE] - PlayClickSound()");
        }
    }

    public void ChangeOpponentPlayerReadyStatus(bool aIsReady)
    {
        if (myPlayerInputLocked)
        {
            return;
        }

        myOpponentReadyToggle.isOn = aIsReady;
        myOpponentReadyText.enabled = aIsReady;
    }

    public void LoadOpponentPanel(string aPlayerSessionId)
    {
        myReadyButton.enabled = true;

        myOpponentPlayerNameText.text = aPlayerSessionId;
        myOpponentPanel.SetActive(true);
        myOpponentContainerCanvas.SetActive(true);
    }

    public void StartTimer(int aTimerStartingNumber)
    {
        myCountdownTimerText.enabled = true;
        myCountdownTimerText.text = aTimerStartingNumber.ToString();
        myCountdownCoroutine = StartCoroutine(CountdownTimer(aTimerStartingNumber));
    }

    public void ResetTimer()
    {
        if(myLobbyCountdownIsRunning)
        {
            StopCoroutine(myCountdownCoroutine);
            myLockedInText.enabled = false;
        }
    }

    public void ChangeToLockedIn()
    {
        LockInput();
        myCountdownBorder.color = Color.red;
        myCountdownTimerText.color = Color.red;
    }

    private void LockInput()
    {
        myPlayerInputLocked = true;
        myLeaveLobbyButton.enabled = false;
        myReadyButton.enabled = false;
    }

    private IEnumerator CountdownTimer(int aTimerStartingNumber)
    {
        myLobbyCountdownIsRunning = true;
        int currentNumber = aTimerStartingNumber;

        while (currentNumber > 0)
        {
            yield return new WaitForSeconds(1f);
            currentNumber--;

            string counterText = currentNumber.ToString();

            if (currentNumber <= 9)
            {
                counterText = "0" + counterText;
            }

            myCountdownTimerText.text = counterText;
        }
        yield return null;
    }
}
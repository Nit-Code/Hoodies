using Assets.Shared.Scripts.Messages.Client;
using SharedScripts;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MatchSceneUIManager : MonoBehaviour
{
    private bool myPlayerInputLocked = false;
    private bool myLobbyCountdownIsRunning = false;
    private Coroutine myCountdownCoroutine;

    
    [SerializeField] GameObject myUIBottomContainer;
    [SerializeField] GameObject myUITopContainer;
    [SerializeField] ActionPromptUI myActionPrompt;
    [SerializeField] AbilityPromptUI myAbilityPrompt;
    [SerializeField] Canvas myCanvas;
    [SerializeField] GameObject myLoadingScreen;
    [SerializeField] GameObject myCardGrid;
    [SerializeField] GameObject myPlayerTurnPanel;
    [SerializeField] Image myPlayerTurnPanelBorder;
    [SerializeField] TextMeshProUGUI myPlayerTurnText;
    [SerializeField] TextMeshProUGUI myEnergyNumberText;
    [SerializeField] TextMeshProUGUI myTimerNumberText;
    [SerializeField] Button myEndTurnButton;

    
    [SerializeField] GameObject myMatchEndContainer;
    [SerializeField] Image myMatchEndPanelBorder;
    [SerializeField] Image myMatchEndClickBlocker;
    [SerializeField] TextMeshProUGUI myMatchEndWinnerText;

    //references
    [SerializeField] private ClientGameManager myGameManagerReference;
    private NetworkClient myNetworkClientReference;
    private AudioManager myAudioManagerReference;
    
    private void Start()
    {
        myAudioManagerReference = FindObjectOfType<AudioManager>();
        myNetworkClientReference = FindObjectOfType<NetworkClient>();

        ShowMatchUI();
        HideMatchEndPanel();
        HideActionPrompt();
        HideAbilityButton();
    }

    private void OnEnable()
    {
        EventHandler.OurAfterMatchSceneLoadEvent += ShowLoadingScreen;
    }

    private void OnDisable()
    {
        EventHandler.OurAfterMatchSceneLoadEvent -= ShowLoadingScreen;
    }


    public void ShowMatchUI()
    {
        ChangeMatchUIEnabledStatus(true);
    }

    public void HideMatchUI()
    {
        ChangeMatchUIEnabledStatus(false);
    }

    public void ChangeMatchUIEnabledStatus(bool aStatus)
    {
        myUIBottomContainer.SetActive(aStatus);
        myUITopContainer.SetActive(aStatus);
        myActionPrompt.enabled = aStatus;
        myAbilityPrompt.enabled = aStatus;   
    }

    public void ShowMatchEndPanel(MatchStateMessageId anEndState, string aWinnerName, Color32 aBorderColor)
    {
        myPlayerInputLocked = true;

        if(anEndState == MatchStateMessageId.END_DRAW)
        {
            myMatchEndWinnerText.text = "DRAW";
        }
        else
        {
            myMatchEndWinnerText.text = aWinnerName + " WON!";            
        }

        myMatchEndPanelBorder.color = aBorderColor;
        // if we add any extra menus, deactivate them
        myMatchEndContainer.SetActive(true);
    }

    public void HideMatchEndPanel()
    {
        myMatchEndContainer.SetActive(false);
    }

    public void EnableMatchEndClickBlocker()
    {
        myMatchEndClickBlocker.enabled = true;
    }

    public void DisableMatchEndClickBlocker()
    {
        myMatchEndClickBlocker.enabled = false;
    }

    public GameObject GetCardGrid()
    {
        return myCardGrid;
    }

    private void ShowLoadingScreen()
    {
        myLoadingScreen.SetActive(true);
    }

    public void HideLoadingScreen()
    {
        myLoadingScreen.SetActive(false);
    }

    public void UpdateEnergyNumber(int aNumber)
    {
        if(aNumber <= 9)
        {
            myEnergyNumberText.text = "0" + aNumber.ToString();
        }
        else
        {
            myEnergyNumberText.text = aNumber.ToString();
        }
    }

    public void OnEndTurnButton()
    {
        if(!myPlayerInputLocked)
        {
            myGameManagerReference.EndTurn();
        } 
    }

    public void OnBackToMainMenuButton()
    {
        myNetworkClientReference.LeaveEndedMatch();
    }

    public IEnumerator ShowPlayerTurnMessage(TurnType aTurnType)
    {
        if (aTurnType == TurnType.PLAYER)
        {
            myPlayerTurnPanelBorder.color = new Color32(170, 232, 255, 255);
            myPlayerTurnText.color = new Color32(170, 232, 255, 255);
            myPlayerTurnText.text = "Player Turn";
        }
        else if (aTurnType == TurnType.ENEMY)
        {
            myPlayerTurnPanelBorder.color = Color.red;
            myPlayerTurnText.color = Color.red;
            myPlayerTurnText.text = "Enemy Turn";
        }

        myPlayerTurnPanel.SetActive(true);

        int secondsLeft = 3;

        while (secondsLeft > 0)
        {
            yield return new WaitForSeconds(1f);
            secondsLeft--;
        }

        myPlayerTurnPanel.SetActive(false);
        yield return null;
    }

    public void StartTimer(int aTimerStartingNumber)
    {
        myTimerNumberText.color = new Color32(170, 232, 255, 255);
        myTimerNumberText.text = aTimerStartingNumber.ToString();
        myTimerNumberText.enabled = true;
        myCountdownCoroutine = StartCoroutine(CountDownTimer(aTimerStartingNumber));
    }

    public void StopTimer()
    {
        if (myLobbyCountdownIsRunning)
        {
            StopCoroutine(myCountdownCoroutine);
        }
    }

    private IEnumerator CountDownTimer(int aTimerStartingNumber)
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
                myTimerNumberText.color = Color.red;
                counterText = "0" + counterText;
            }

            myTimerNumberText.text = counterText;
        }
        yield return null;
    }

    public void LockInput()
    {
        myPlayerInputLocked = true;
    }

    public void UnlockInput()
    {
        myPlayerInputLocked = false;
    }

    public bool IsPlayerInputLocked()
    {
        return myPlayerInputLocked;
    }

    public void EnableEndTurnButton()
    {
        myEndTurnButton.enabled = true;
    }

    public void DisableEndTurnButton()
    {
        myEndTurnButton.enabled = false;
    }

    // Action Prompt

    public void HideActionPrompt()
    {
        myActionPrompt.MakeInvisible();
    }

    public void ShowAttackActionPrompt(SharedTile aTile, SharedUnit aUnit)
    {
        myActionPrompt.ActivatePrompt(ActionPromptUI.PromptOption.ATTACK, aTile);
    }

    public void ShowMoveActionPrompt(SharedTile aTile)
    {
        myActionPrompt.ActivatePrompt(ActionPromptUI.PromptOption.MOVE, aTile);
    }

    public void ShowNoneActionPrompt(SharedTile aTile)
    {
        myActionPrompt.ActivatePrompt(ActionPromptUI.PromptOption.NONE, aTile);
    }

    public bool IsMouseOverActionPrompt(PointerEventData anEventData)
    {
        List<RaycastResult> rayCastResultList = new();
        EventSystem.current.RaycastAll(anEventData, rayCastResultList);

        foreach (RaycastResult raycastResult in rayCastResultList)
        {
            if (raycastResult.gameObject.CompareTag("ActionPromptButton"))
            {
                return true;
            }
        }
        return false;
    }

    // Ability Button

    public void HideAbilityButton()
    {
        myAbilityPrompt.MakeInvisible();
    }

    public void UpdateAbilityButtonStatus(SharedUnit aUnit)
    {
        if (aUnit != null && aUnit.CanCastAbility())
        {
            myAbilityPrompt.MakeVisible();
            myAbilityPrompt.RefreshAbilityButton(aUnit);
        }
        else
        {
            myAbilityPrompt.MakeInvisible();
        }
    }

    
    public void PlayClickSound()
    {
        // TODO: implement
        // myAudioManagerReference.PlaySound(SoundName.MenuClick);
    }
}
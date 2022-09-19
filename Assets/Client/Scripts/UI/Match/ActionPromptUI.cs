using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ActionPromptUI : MonoBehaviour
{
    private Button myButton;
    private TextMeshProUGUI myButtonText;
    private RectTransform myRectTransform;
    private Canvas myPromptCanvas;
    private Color myOriginalTextColor;
    private PromptOption myCurrentOption;
    

    //reference
    private ClientGameManager myGameManagerReference;
    private MatchSceneUIManager myMatchSceneUIManager;
    private SharedTile myClickedTileReference;

    public enum PromptOption
    {
        ATTACK,
        MOVE,
        NONE
    }

    private void Awake()
    {
        myButton = GetComponentInChildren<Button>();
        myButtonText = GetComponentInChildren<TextMeshProUGUI>();
        myRectTransform = GetComponent<RectTransform>();
        myPromptCanvas = GetComponent<Canvas>();
        myOriginalTextColor = myButtonText.color;
    }

    private void Start()
    {
        myGameManagerReference = FindObjectOfType<ClientGameManager>();
        myMatchSceneUIManager = FindObjectOfType<MatchSceneUIManager>();
    }

    public void ActivatePrompt(PromptOption aPromptOption, SharedTile aTile)
    {
        if (myMatchSceneUIManager.IsPlayerInputLocked())
        {
            return;
        }

        myRectTransform.anchoredPosition = Input.mousePosition + new Vector3(70,15,0);

        myClickedTileReference = aTile;
        
        // TODO: Hard coded user facing text?
        switch (aPromptOption)
        {
            case PromptOption.ATTACK:
                myButtonText.SetText("ATTACK");
                myButtonText.color = myOriginalTextColor;
                myButton.enabled = true;
                myCurrentOption = PromptOption.ATTACK;
                MakeVisible();
                break;
            case PromptOption.MOVE:
                myButtonText.SetText("MOVE");
                myButtonText.color = myOriginalTextColor;
                myButton.enabled = true;
                myCurrentOption = PromptOption.MOVE;
                MakeVisible();
                break;
            case PromptOption.NONE:
                myButtonText.SetText("NO ACTIONS");
                myButtonText.color = Color.gray;
                myButton.enabled = false;
                MakeVisible();
                break;
        }
    }

    private void MakeVisible()
    {
        myPromptCanvas.enabled = true;
    }

    public void MakeInvisible()
    {
        myCurrentOption = PromptOption.NONE; // We put this here, so it's easy to reset its aPromptOption when a turn ends. (One of the things we do when ending turns is making the prompt invisible)
        myPromptCanvas.enabled = false;
    }

    public void RequestAction()
    {
        if (myMatchSceneUIManager.IsPlayerInputLocked())
        {
            return;
        }

        if (myCurrentOption == PromptOption.ATTACK)
        {
            myGameManagerReference.TryRequestAttackUnit(myClickedTileReference);
        }
        else if(myCurrentOption == PromptOption.MOVE)
        {
            myGameManagerReference.TryRequestMoveUnit(myClickedTileReference);
        }
        MakeInvisible();
    }

    /*
    public void RequestAction()
    {
        if(myCurrentOption == PromptOption.ATTACK)
        {
            myGameManagerReference.RequestAttackUnit(mySelectedUnit, myClickedTile);
        }
        else if(myCurrentOption == PromptOption.MOVE)
        {
            myGameManagerReference.RequestMoveUnit(mySelectedUnit, myClickedTile);
        }
        MakeInvisible();
    }
    */
}

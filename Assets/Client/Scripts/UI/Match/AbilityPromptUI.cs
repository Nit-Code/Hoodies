using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class AbilityPromptUI : MonoBehaviour
{
    private Button myButton;
    [SerializeField] private TextMeshProUGUI myButtonText;
    [SerializeField] private TextMeshProUGUI myCostText;
    private Canvas myPromptCanvas;  
    private Color myOriginalTextColor;

    private ClientGameManager myGameManagerReference;
    private MatchSceneUIManager myMatchSceneUIManager;
    private SharedBoard myBoardReference;

    private void Awake()
    {
        myPromptCanvas = GetComponent<Canvas>();
        myButton = GetComponentInChildren<Button>();
        myOriginalTextColor = myButtonText.color;
        MakeInvisible();
    }

    private void Start()
    {
        myGameManagerReference = FindObjectOfType<ClientGameManager>();
        myBoardReference = FindObjectOfType<SharedBoard>();
        myMatchSceneUIManager = FindObjectOfType<MatchSceneUIManager>();
    }

    public void RefreshAbilityButton(SharedUnit aUnit)
    {
        if (aUnit != null && aUnit.CanCastAbility())
        {
            myButton.enabled = true;            
            myButtonText.color = myOriginalTextColor;
            myCostText.text = aUnit.GetAbility().GetCost().ToString();

            // TODO: Hard coded user facing text?
            if (myGameManagerReference.GetIsCastingUnitAbility())
            {
                myButtonText.text = "Casting...";
            }
            else
            {
                myButtonText.text = "Activate Ability";
            }
        }
        else
        {
            myButton.enabled = false;
            // TODO: Hard coded user facing text?
            myButtonText.text = "Ability Unavailable";
            myButtonText.color = Color.white;
        }
    }

    public void OnButtonClicked()
    {
        if (myMatchSceneUIManager.IsPlayerInputLocked())
        {
            return;
        }

        SharedUnit castingUnit = myGameManagerReference.GetSelectedUnit();

        if(castingUnit == null)
        {
            return;
        }    

        myGameManagerReference.ChangeCastingUnitAbilityStatus();
        myBoardReference.UndoBoardSelectionColors();
        myBoardReference.ColorAndSetPossibleCastingTiles();
    }

    public void MakeVisible()
    {
        myPromptCanvas.enabled = true;
    }

    public void MakeInvisible()
    {
        myPromptCanvas.enabled = false;
    }
}

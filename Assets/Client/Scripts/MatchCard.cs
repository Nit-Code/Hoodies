using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using SharedScripts.DataId;

public class MatchCard : SharedCard, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{ 
    // presentation
    private GameObject myCardPanel;
    [SerializeField] private Image myBorder;
    private RectTransform myRectTransform;
    private CanvasGroup myCanvasGroup;

    // state
    private Vector3 myOriginalPosition;

    // reference
    private SharedBoard myBoardReference;
    private ClientGameManager myClientGameManagerReference;
    private MatchSceneUIManager myMatchSceneUIManager;
    private AbilityPromptUI myAbilityPromptUI;
    private SharedGameObjectFactory myGameObjectFactoryReference;

    private void Awake()
    {
        //TODO: add error messages if components aren't found
        myRectTransform = GetComponent<RectTransform>();
        myCanvasGroup = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        myBoardReference = FindObjectOfType<SharedBoard>();
        myClientGameManagerReference = FindObjectOfType<ClientGameManager>();
        myMatchSceneUIManager = FindObjectOfType<MatchSceneUIManager>();
        myAbilityPromptUI = FindObjectOfType<AbilityPromptUI>();
    }

    public override void Init(UnitCardData aCardData, UnitData aUnitData, AbilityData anAbilityData)
    {
        base.Init(aCardData, aUnitData, anAbilityData);

        myBoardReference = FindObjectOfType<SharedBoard>();
        myClientGameManagerReference = FindObjectOfType<ClientGameManager>();
        myMatchSceneUIManager = FindObjectOfType<MatchSceneUIManager>();
        //TODO use UnitData
    }

    public override void Init(AbilityCardData aCardData, AbilityData anAbilityData)
    {
        base.Init(aCardData, anAbilityData);

        myGameObjectFactoryReference = FindObjectOfType<SharedGameObjectFactory>();
        myBoardReference = FindObjectOfType<SharedBoard>();
        myClientGameManagerReference = FindObjectOfType<ClientGameManager>();

        if (anAbilityData.myId != AbilityId.INVALID && myGameObjectFactoryReference != null)
        {
            myAbility = myGameObjectFactoryReference.AddAbilityComponent(this.gameObject, anAbilityData.myId);
        }
    }

    public UnitId GetUnitId()
    {
        if (myCardType == CardType.UNIT)
        {
            return myUnitId;
        }
        else
        {
            Debug.LogError("[HOOD][CARD] - GetUnitId()");
            return UnitId.INVALID;
        }
    }

    public AbilityId GetAbilityId()
    {
        if (myCardType == CardType.TECHNOLOGY)
        {
            return myAbilityId;
        }
        else
        {
            Debug.LogError("[HOOD][CARD] - GetAbilityId()");
            return AbilityId.INVALID;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        myOriginalPosition = myRectTransform.position;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if(myMatchSceneUIManager.IsPlayerInputLocked())
        {
            return;
        }

        MakeTransparent();
        myBorder.color = Color.yellow;
        myCanvasGroup.blocksRaycasts = false;

        myClientGameManagerReference.ResetPlayerSelections();
        myAbilityPromptUI.RefreshAbilityButton(null);

        if (myCardType == CardType.UNIT)
        {
            myBoardReference.ColorValidSpawnTiles();
        }
        else // if it's a technology card
        {
            myClientGameManagerReference.SetIsCastingTechnologyStatus(true);
            myBoardReference.UndoBoardSelectionColors();
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (myMatchSceneUIManager.IsPlayerInputLocked())
        {
            return;
        }

        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(myRectTransform, eventData.position, eventData.pressEventCamera, out var globalMousePosition))
        {
            myRectTransform.position = globalMousePosition;
        }
    }

    //TODO: Check this one
    public void OnEndDrag(PointerEventData eventData) 
    {
        if (myMatchSceneUIManager.IsPlayerInputLocked())
        {
            return;
        }

        myClientGameManagerReference.SetIsCastingTechnologyStatus(false);
        ResetCard();
    }

    public void MakeInvisible()
    {
        myCanvasGroup.alpha = 0f;
    }

    public void MakeTransparent()
    {
        myCanvasGroup.alpha = 0.5f;
    }

    public void ResetCard()
    {
        // TODO: hard coded data????
        Color32 originalColor = new Color32(170, 232, 255, 255);

        myCanvasGroup.blocksRaycasts = true;
        myCanvasGroup.alpha = 1f;
        myBorder.color = originalColor;
        transform.position = myOriginalPosition;
    }
}
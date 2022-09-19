using SharedScripts;
using SharedScripts.DataId;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;
using System.Collections;

public class SharedTile : MonoBehaviour, IDropHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    // presentation
    [SerializeField] private Material myTileDefaultMaterial;
    [SerializeField] private Material myTileGlowBlue;
    [SerializeField] private Material myTileGlowRed;
    [SerializeField] private Material myTileGlowYellow;
    [SerializeField] private Material myTileGlowGreen;

    // state
    protected Vector2Int myCoordinate;
    protected int myTravelCost;
    protected bool myIsBlocked;
    public bool GetIsBlocked() { return myIsBlocked; }
    protected TileType myType;
    public TileType GetTileType() { return myType; }
    protected bool myCanSpawn;
    public bool GetCanSpawn() { return myCanSpawn; }

    // reference
    protected SharedUnit myUnitReference;
    public SharedUnit GetUnit() { return myUnitReference; }

    protected SharedBoard myBoardReference;
    protected ClientGameManager myClientGameManagerReference;
    protected MatchSceneUIManager myMatchSceneUIManager;

    // component
    private MeshRenderer myMeshRenderer;

    private TileColor myBaseColor = TileColor.WHITE; // This is the color that should be displayed on the tile if there are no selections present
    private TileColor myFallbackColor = TileColor.WHITE; // This is the color this tile will "fall back" to OnPointerExit

    // EXAMPLE:
    // Need simple accessor to copies of several private/protected data fields?
    // Use structs!

    public struct PathingInfo
    {
        public int cost;
        public Vector2Int pos;
        public bool isBlocked;
        public bool canSpawn;

        public PathingInfo(int aCost, Vector2Int aPos, bool anIsBlocked, bool aCanSpawn)
        {
            cost = aCost;
            pos = aPos;
            isBlocked = anIsBlocked;
            canSpawn = aCanSpawn;
        }
    }

    public SharedTile()
    {
        myFallbackColor = TileColor.WHITE;
    }

    public Vector2Int GetCoordinate()
    {
        return myCoordinate;
    }

    private void Awake()
    {
        myMeshRenderer = GetComponent<MeshRenderer>();
    }

    protected void Start()
    {
        myClientGameManagerReference = FindObjectOfType<ClientGameManager>();
        myBoardReference = GetComponentInParent<SharedBoard>();
        myMatchSceneUIManager = FindObjectOfType<MatchSceneUIManager>();
    }

    public void Init(SharedBoard aBoard, Vector2Int aCoordinate, TileData aTileData)
    {
        myBoardReference = aBoard;
        myCoordinate = aCoordinate;
        myTravelCost = aTileData.myTravelCost;
        myIsBlocked = aTileData.myIsBlocked;
        myType = aTileData.myType;
        myCanSpawn = aTileData.myCanSpawn;
    }

    public void SetUnit(SharedUnit aUnit)
    {
        myUnitReference = aUnit;
        aUnit.SetPosition(this.myCoordinate);
        aUnit.transform.SetParent(this.transform);
        myIsBlocked = true;
    }

    public void RemoveUnit()
    {
        myUnitReference = null;
        myIsBlocked = false;
    }

    public void SetCoordinate(Vector2Int aCoordinate)
    {
        myCoordinate = aCoordinate;
    }

    public PathingInfo GetPathingInfo()
    {
        return new PathingInfo(myTravelCost, myCoordinate, myIsBlocked, myCanSpawn);
    }

    public void SetColor(TileColor aColor)
    {
        myBaseColor = aColor;
    }

    public void SetFallbackColor(TileColor aColor)
    {
        myFallbackColor = aColor;
    }

    public void GoToBaseColor(bool anAffectFallback)
    {
        ColorTileTemporary(myBaseColor, anAffectFallback);
    }

    public void ChangeTileColor(TileColor aColor)
    {
        switch (aColor)
        {
            case TileColor.WHITE:
                myMeshRenderer.material = myTileDefaultMaterial;
                break;
            case TileColor.BLUE:
                myMeshRenderer.material = myTileGlowBlue;
                break;
            case TileColor.RED:
                myMeshRenderer.material = myTileGlowRed;
                break;
            case TileColor.YELLOW:
                myMeshRenderer.material = myTileGlowYellow;
                break;
            default:
                Shared.LogError("[HOOD][TILE] - Unhandled TileColor ChangeTileColor()");
                break;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        GameObject draggedObject = eventData.pointerDrag;

        if (draggedObject != null)
        {
            if (draggedObject.CompareTag("MatchCard"))
            {
                MatchCard card = draggedObject.GetComponent<MatchCard>();
                HandleDraggedCard(card);
            }
        }
        else // It's just the mouse pointer
        {
            if (myClientGameManagerReference.GetIsCastingUnitAbility()) // If casting an ability
            {
                HandleCastingAbility();
            }
            else // Just the mouse and not casting an ability
            {
                if (!myMatchSceneUIManager.IsMouseOverActionPrompt(eventData))
                {
                    ChangeTileMaterial(TileColor.BLUE);
                }
            }
        }
    }

    private void HandleDraggedCard(MatchCard aDraggedCard)
    {
        if (aDraggedCard.GetCardType() == CardType.TECHNOLOGY)
        {
            SharedAbility ability = aDraggedCard.gameObject.GetComponent<SharedAbility>();
            aDraggedCard.MakeInvisible();

            if (ability == null)
            {
                return;
            }

            myBoardReference.UndoBoardSelectionColors();

            if (ability.GetShape() != AreaShape.LINE) // If it's not a line, meaning it doesn't need a second tile to work
            {
                myBoardReference.ColorAndSetAbilityAreaTiles(ability, this, true);
            }
        }
    }

    private void HandleCastingAbility()
    {
        if (myClientGameManagerReference.GetSelectedUnit() == null)
        {
            return;
        }

        SharedAbility ability = myClientGameManagerReference.GetSelectedUnitAbility();

        myBoardReference.UndoBoardSelectionColors();
        myBoardReference.ColorAndSetPossibleCastingTiles();

        if (ability.GetShape() != AreaShape.LINE) // If it's not a line, meaning it doesn't need a second tile to work
        {
            if (myClientGameManagerReference.ValidCastingTilesContains(this))
            {
                myBoardReference.ColorAndSetAbilityAreaTiles(ability, this, true);
            }
            else
            {
                myBoardReference.ColorAndSetAbilityAreaTiles(ability, this, false);
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        GameObject draggedObject = eventData.pointerDrag;

        if (draggedObject != null)
        {
            if (draggedObject.CompareTag("MatchCard"))
            {
                MatchCard card = draggedObject.GetComponent<MatchCard>();

                if (card.GetCardType() == CardType.TECHNOLOGY)
                {
                    card.MakeTransparent();
                }
            }
        }

        ChangeTileMaterial(myFallbackColor);
    }

    public virtual void OnDrop(PointerEventData eventData)
    {
        GameObject droppedObject = eventData.pointerDrag;

        if (droppedObject != null)
        {
            myBoardReference.UndoBoardSelectionColors();

            if (droppedObject.CompareTag("MatchCard"))
            {
                MatchCard droppedCard = droppedObject.GetComponent<MatchCard>();

                HandleDroppedCard(droppedCard);
            }
        }
    }

    private void HandleDroppedCard(MatchCard aDroppedCard)
    {
        if (aDroppedCard.GetCardType() == CardType.UNIT) // Check if it is a unit card
        {
            if (!myClientGameManagerReference.TryRequestSpawnUnitFromCard(aDroppedCard, this))
            {
                return;
            }
            else
            {
                Destroy(aDroppedCard.transform.gameObject);
            }
        }
        else if (aDroppedCard.GetCardType() == CardType.TECHNOLOGY)
        {
            if (!myClientGameManagerReference.TryRequestUseTechnologyCard(aDroppedCard, this))
            {
                return;
            }
            else
            {
                Destroy(aDroppedCard.transform.gameObject);
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (!myClientGameManagerReference.GetIsCastingUnitAbility())
            {
                myClientGameManagerReference.ResetPlayerSelections();

                if (myUnitReference != null && myUnitReference.GetIsEnabled())
                {
                    HandleLeftClickOnUnit();
                }
                else
                {
                    StartCoroutine(ChangeColorOnClick()); //Changes the tile color temporarily to give the user some feedback
                }
            }
            else // If casting ability
            {
                myClientGameManagerReference.TryRequestCastUnitAbility(this);
            }
        }

        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (!myClientGameManagerReference.GetIsCastingUnitAbility()) // If a player is casting an ability, it must be canceled before doing anything else
            {
                HandleRightClick();
            }
        }
    }

    private void HandleLeftClickOnUnit()
    {
        myClientGameManagerReference.SetSelectedUnit(myUnitReference);

        if (!myUnitReference.GetHasMoved())
        {
            myBoardReference.ColorPossibleMovementTiles(myUnitReference); // This also registers them in the ClientGameManager if it's my unit
        }
        else
        {
            this.ColorTileTemporary(TileColor.YELLOW, true);
        }

        myBoardReference.ColorAttackRange(myUnitReference); // This also registers them in the ClientGameManager if it's my unit
        SharedPlayer localPlayer = myClientGameManagerReference.GetLocalPlayer();

        if (myUnitReference.IsOwnedByPlayer(localPlayer))
        {
            myMatchSceneUIManager.UpdateAbilityButtonStatus(myUnitReference);
            ColorTileTemporary(TileColor.BLUE, true);
        }
        else
        {
            ColorTileTemporary(TileColor.RED, true);
        }
    }

    private void HandleRightClick()
    {
        if (!myClientGameManagerReference.IsLocalPlayerTurn())
        {
            myMatchSceneUIManager.ShowNoneActionPrompt(this);
            return;
        }

        // TODO: could these senarios not both happen at the same time?
        if (myClientGameManagerReference.SelectedUnitCanAttackOnTile(this))
        {
            myMatchSceneUIManager.ShowAttackActionPrompt(this, myUnitReference);
        }
        else if (myClientGameManagerReference.SelectedUnitCanMoveToTile(this))
        {
            myMatchSceneUIManager.ShowMoveActionPrompt(this);
        }
        else
        {
            myMatchSceneUIManager.ShowNoneActionPrompt(this);
        }
    }

    public void ChangeTileMaterial(TileColor color)
    {
        switch (color)
        {
            case TileColor.WHITE:
                myMeshRenderer.material = myTileDefaultMaterial;
                break;
            case TileColor.BLUE:
                myMeshRenderer.material = myTileGlowBlue;
                break;
            case TileColor.RED:
                myMeshRenderer.material = myTileGlowRed;
                break;
            case TileColor.YELLOW:
                myMeshRenderer.material = myTileGlowYellow;
                break;
            case TileColor.GREEN:
                myMeshRenderer.material = myTileGlowGreen;
                break;
        }
    }

    public void ColorTileTemporary(TileColor color, bool changeFallbackColor) // Use this one when you want board refreshes to change the tile's color back to white
    {
        ChangeTileMaterial(color);

        if (changeFallbackColor)
        {
            myFallbackColor = color;
        }
    }

    public void ColorTile(TileColor color) // Use this one when you don't want board refreshes to change the tile's color back to white
    {
        ChangeTileMaterial(color);
        myBaseColor = color;
        myFallbackColor = color;
    }

    public void ChangeBaseColor(TileColor color)
    {
        myBaseColor = color;
    }

    private IEnumerator ChangeColorOnClick()
    {
        ColorTileTemporary(TileColor.WHITE, false);
        yield return new WaitForSeconds(0.1f);

        ColorTileTemporary(TileColor.BLUE, false);

        yield return null;
    }
}



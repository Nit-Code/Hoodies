using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SharedScripts;
using SharedScripts.DataId;
using UnityEngine;


public abstract class SharedAbility : MonoBehaviour
{
    protected int myInGameId;

    protected SharedUnit myOwnerUnitReference;
    protected AbilityId myAbilityDataId;
    protected string myName;
    protected string myDescription;
    protected int myCost;
    public int GetCost() { return myCost; }
    protected AreaShape myAreaShape;
    public AreaShape GetShape() { return myAreaShape; }
    protected int myAreaShapeSize;
    public int GetShapeSize() { return myAreaShapeSize; }
    protected bool myIncludeCenter;
    public bool GetIncludesCenter() { return myIncludeCenter; }
    protected TileColor myTileColor; // This is used when abilities leave permanent tile coloring when active.
    public TileColor GetTileColor() { return myTileColor; }
    protected int myCastingRange;
    public int GetCastingRange() { return myCastingRange; }
    protected int myCooldown; // How many turns a unit needs to wait until the ability can be cast again
    protected int myCooldownTimer;
    protected int myDuration; // How long an ability lasts on the board
    protected int myDurationTimer;
    protected List<SharedTile> myCastTiles;
    public List<SharedTile> GetCastTiles() { return myCastTiles; }

    protected bool myIsCooldownInProgress;
    public bool IsCooldownInProgress() { return myIsCooldownInProgress; }

    private SpriteRenderer mySpriteRenderer;
    private Sprite mySprite;

    private Animator myAnimator;
    private AnimatorOverrideController myAnimatorOverride;

    protected SharedBoard myBoardReference;

    private void Awake()
    {
        if (TryGetComponent<Animator>(out myAnimator))
            myAnimator.runtimeAnimatorController = myAnimatorOverride;
        else
            Shared.LogError("[HOOD][SHARED][ABILITY] - Animator component not found on Awake()");

        if (TryGetComponent<SpriteRenderer>(out mySpriteRenderer))
            mySpriteRenderer.sprite = mySprite;
        else
            Shared.LogError("[HOOD][SHARED][ABILITY] - SpriteRenderer component not found on Awake()");
    }

    protected void Start()
    {
        myBoardReference = FindObjectOfType<SharedBoard>();
    }

    public void Init(GameObject aGameObject, AbilityData aData)
    {
        myAbilityDataId = aData.myId;
        myName = aData.myName;
        myDescription = aData.myDescription;
        myAreaShape = aData.myAreaShape;
        myAreaShapeSize = aData.myAreaShapeSize;
        myIncludeCenter = aData.myIncludeCenter;
        myTileColor = aData.myTileColor;
        myDuration = aData.myDuration;
        myDurationTimer = 0;
        myCastTiles = new();

        if (aGameObject.TryGetComponent<SharedUnit>(out SharedUnit unit))
        {
            myCost = aData.myCost;
            myCastingRange = aData.myCastingRange;
            myCooldown = aData.myCooldown;
            myCooldownTimer = 0;
            myOwnerUnitReference = unit;
        }
        else if (aGameObject.TryGetComponent<MatchCard>(out MatchCard card))
        {
            myCost = card.GetCost();
        }
    }

    public abstract void ApplyAbilityEffect();
    protected abstract void ApplyVisualEffects();
    protected abstract void RemoveVisualEffects();

    public virtual void UpdateAbilityStatus()
    {
        if (!IsAbilityActive())
        {
            return;
        }
    }

    public void CastAbilityFromUnit(List<SharedTile> castTiles)
    {
        if (myOwnerUnitReference.GetIsEnabled() && !myIsCooldownInProgress)
        {
            myIsCooldownInProgress = true;
            myCastTiles = castTiles;
            myCooldownTimer = myCooldown;

            ApplyAbilityEffect();
        }
    }

    public void TryApplyEffectOnUnit(SharedUnit unit)
    {
        if (!IsAbilityActive())
        {
            return;
        }

        SharedTile unitTile = myBoardReference.GetTile(unit.GetPosition());

        if (myCastTiles.Contains(unitTile))
        {
            ApplyAbilityEffect();
        }
    }

    public bool WasUsed()
    {
        return myIsCooldownInProgress;
    }

    public void UpdateTimers()
    {
        UpdateCooldownTimer();
        UpdateDurationTimer();
    }

    private void UpdateCooldownTimer()
    {
        if (myIsCooldownInProgress)
        {
            myCooldownTimer -= 1;

            if (myCooldownTimer == 0)
            {
                ResetCooldownProgress();
            }
        }
    }

    private void UpdateDurationTimer()
    {
        if (myDurationTimer == myDuration)
        {
            StopCastingAbility();
        }

        if (IsAbilityActive())
        {
            myDurationTimer += 1;
        }
    }

    public bool IsAbilityActive()
    {
        return myCastTiles.Count() > 0;
    }

    private void StopCastingAbility()
    {
        RemoveVisualEffects();
        myCastTiles.Clear();
        myDurationTimer = 0;
    }

    private void ResetCooldownProgress()
    {
        myIsCooldownInProgress = false;
        myCooldownTimer = myCooldown;
    }
}



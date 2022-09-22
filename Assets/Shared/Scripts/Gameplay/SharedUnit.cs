using SharedScripts.DataId;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class SharedUnit : MonoBehaviour
{
    private int myMatchId;
    public int GetMatchId() { return myMatchId; }
    private UnitId myUnitDataId;
    private CardId myCardId;
    public CardId GetCardId() { return myCardId; }

    // presentation
    [SerializeField] private TextMeshProUGUI myHPText;
    [SerializeField] private TextMeshProUGUI myATKText;
    [SerializeField] private TextMeshProUGUI myKindText;
    private Color myOriginalColor;

    // state
    protected Vector2Int myBoardPosition;
    public Vector2Int GetPosition() { return myBoardPosition; }
    public void SetPosition(Vector2Int position) { myBoardPosition = position; }    
    protected bool myIsMothership;
    public void SetIsMothership() { myIsMothership = true;}
    public bool IsMothership() { return myIsMothership; }
    protected int myShield;
    protected int myAttack;
    public int GetAttack() { return myAttack; }
    protected int myAttackRange;
    public int GetAttackRange() { return myAttackRange; }
    protected int myMovementRange;
    protected List<SharedStatusEffect> myStatusEffects;
    protected bool myIsEnabled;
    public bool GetIsEnabled() { return myIsEnabled; }
    private bool myHasMoved;
    public bool GetHasMoved() { return myHasMoved; }
    public void SetHasMoved() { myHasMoved = true; }

    private bool myIsSpawner;
    public bool GetIsSpawner() { return myIsSpawner; }
    public void EnableKindText() { myKindText.enabled = true; }

    // reference
    protected SharedPlayer myOwnerPlayerReference;
    public SharedPlayer GetPlayer() { return myOwnerPlayerReference; }
    private SharedGameObjectFactory myGameObjectFactoryReference;

    // component
    private Animator myAnimator;
    private SpriteRenderer mySpriteRenderer;
    private Sprite mySprite;
    private CanvasGroup myCanvasGroup;
    protected SharedAbility myAbility;
    public SharedAbility GetAbility() { return myAbility; }

    //animation hashes
    public static int AttackAnimation;
    public static int DeathAnimation;
    public static int HurtAnimation;
    public static int IdleAnimation;
    public static int SummonAnimation;

    private void Awake()
    {
        myAnimator = GetComponent<Animator>();
        mySpriteRenderer = GetComponent<SpriteRenderer>();
        myCanvasGroup = GetComponent<CanvasGroup>();  
    }

    public struct MovementInfo
    {
        public int range;
        public Vector2Int pos;

        public MovementInfo(int aRange, Vector2Int aPos)
        {
            range = aRange;
            pos = aPos;
        }
    }

    public SharedUnit()
    {
        myOwnerPlayerReference = null;
        myIsEnabled = false;
    }

    public void Init(SharedPlayer aOwnerPlayer, bool aIsCapitain, Vector2Int aBoardPosition, UnitData aUnitData, int aMatchId, CardId aCardId)
    {
        myGameObjectFactoryReference = FindObjectOfType<SharedGameObjectFactory>();
        myCardId = aCardId;

        if (aUnitData.myAbilityId != AbilityId.INVALID && myGameObjectFactoryReference != null)
        {
            myAbility = myGameObjectFactoryReference.AddAbilityComponent(this.gameObject, aUnitData.myAbilityId);
        }

        myOwnerPlayerReference = aOwnerPlayer;

        if(myOwnerPlayerReference != null)
        {
            aOwnerPlayer.AddPlacedUnit(this);
        }
        
        myIsMothership = aIsCapitain;
        myBoardPosition = aBoardPosition;

        myShield = aUnitData.myShields;
        myAttack = aUnitData.myAttack;
        myAttackRange = aUnitData.myAttackRange;
        myMovementRange = aUnitData.myMovementRange;
        myIsSpawner = aUnitData.canSpawnOtherUnits;
        myStatusEffects = new();

        myUnitDataId = aUnitData.myId;
        myMatchId = aMatchId;
        OverrideAnimatorHashes();

        mySprite = aUnitData.mySprite;
        myOriginalColor = mySpriteRenderer.color;
        myAnimator.runtimeAnimatorController = aUnitData.myOverrideAnimatorController;

        myATKText.text = myAttack.ToString();
        myHPText.text = myShield.ToString();
    }

    public bool IsOwnedByPlayer(SharedPlayer aPlayer)
    {
        return myOwnerPlayerReference.Equals(aPlayer);
    }

    public int GetAbilityCastingRange()
    {
        return myAbility.GetCastingRange();
    }

    public void ModifyShield(int anAmount)
    {
        myShield += anAmount;
        myHPText.text = myShield.ToString();
    }

    public void ModifyAttack(int anAmmount)
    {
        myAttack += anAmmount;
        myATKText.text = myAttack.ToString();
    }

    public void ModifyMovementRange(int anAmmount)
    {
        myMovementRange += anAmmount;
    }

    public void ModifyAttackRange(int anAmmount)
    {
        myAttackRange += anAmmount;
    }

    #region Ability
    public bool HasAbility()
    {
        return myAbility != null;
    }

    public bool IsCastingAbility()
    {
        if(HasAbility())
        {
            return myAbility.IsAbilityActive();
        }

        return false;
    }

    public bool CanCastAbility()
    {
        if (HasAbility() && CanAffordAbility() && !myAbility.IsCooldownInProgress())
        {
            return true;
        }
        return false;
    }

    public bool CanAffordAbility()
    { 
        if (HasAbility() && myAbility.GetCost() <= myOwnerPlayerReference.GetEnergy())
        {
            return true;
        }

        return false;
    }

    public void UseAbility(List<SharedTile> castTiles) // Cast ability for the first time + cost
    {
        if (HasAbility() && myOwnerPlayerReference.TrySubstractEnergyCost(myAbility.GetCost()))
        {
            myAbility.CastAbilityFromUnit(castTiles);
        }
    }

    public void CastAbilityEffect() // Just activate effect. Use at the start of turns and after moving
    {
        if (HasAbility() && myAbility.IsAbilityActive())
        {
            myAbility.UpdateAbilityStatus();
            myAbility.ApplyAbilityEffect();
        }
    }
    #endregion

    #region StatusEffect
    public void TryAddStatusEffect(StatusEffectId anId)
    {
        if (IsAlreadyAppliedStatusEffect(anId))
        {
            return;
        }
        else
        {
            myStatusEffects.Add(myGameObjectFactoryReference.AddStatusEffectComponent(this, anId));
        }
    }

    private bool IsAlreadyAppliedStatusEffect(StatusEffectId anId)
    {
        foreach (SharedStatusEffect effect in myStatusEffects)
        {
            if (effect.GetId() == anId)
            {
                return true;
            }
        }
        return false;
    }

    public void RemoveStatusEffect(SharedStatusEffect aStatusEffect)
    {
        aStatusEffect.RemoveEffect();
        myStatusEffects.Remove(aStatusEffect);
        aStatusEffect.RemoveUnitReference();
    }

    public void CheckStatusEffectsStatus() // Do after move, THEN activate ability of other units
    {
        for(int i = 0; i < myStatusEffects.Count; i++)
        {
            myStatusEffects[i].CheckStatus();
        }
    }

    public void UpdateStatusEffectsTimers()
    {
        for (int i = 0; i < myStatusEffects.Count; i++)
        {
            myStatusEffects[i].UpdateTimerStatus();
        }
    }

    public void UpdateAbilityTimers()
    {
        if(HasAbility())
        {
            myAbility.UpdateTimers();
        }
    }
    #endregion


    //TODO: hard coded strings ????
    private void OverrideAnimatorHashes()
    {
        AttackAnimation = Animator.StringToHash("Attack");
        DeathAnimation = Animator.StringToHash("Death");
        HurtAnimation = Animator.StringToHash("Hurt");
        IdleAnimation = Animator.StringToHash("Idle");
        SummonAnimation = Animator.StringToHash("Summon");
    }

    public float GetAnimationLength(int anAnimationHash)
    {
        if(myAnimator != null)
        {
            foreach (AnimationClip clip in myAnimator.runtimeAnimatorController.animationClips)
            {
                if (Animator.StringToHash(clip.name) == anAnimationHash)
                    return clip.length;
            }
            return -1;
        }
        return -1;
    }

    public async Task PerformAnimation(int anAnimation)
    {
#if !UNITY_SERVER
        float duration = GetAnimationLength(anAnimation);
        if (duration != -1)
        {
            myAnimator.CrossFade(anAnimation, 0);
            float end = Time.time + duration;

            while (Time.time < end)
            {
                await Task.Yield();
            }
            myAnimator.CrossFade(SharedUnit.IdleAnimation, 0);
        }
#endif
    }

    public bool IsAlive()
    {
        return myShield > 0;
    }

    public void KillUnit() // Force kills unit. Visually removes the unit from the game. We don't destroy it so it stays in the unit dictionary, in case we need to do something with it later. Like revive it, or display Match stats at the end.
    {
        if (myShield > 0)
        {
            myShield = -1;
        }

        Destroy(myAnimator);
        MakeInvisible();
        myBoardPosition = new Vector2Int(-99, -99);
        transform.SetParent(null);
        transform.position += new Vector3(-1000, -1000, -1000);
        myIsEnabled = false;
    }

    public void MakeInvisible()
    {
        myCanvasGroup.alpha = 0f;
    }

    public MovementInfo GetMovementInfo()
    {
        return new MovementInfo(myMovementRange, myBoardPosition);
    }

    public void RefreshUnit(bool isAuxUnit)  //TODO
    {
        if(!isAuxUnit)
        {
            ResetUnitSprite();
            myHasMoved = false;
            UpdateStatusEffectsTimers(); // We call this first in case status effects need to be re-applied
        }
        UpdateAbilityTimers();// We call this second in case status effects need to be re-applied   
    }

    public void EnableUnit()
    {
        myIsEnabled = true;
    }
    public void DisableUnit()
    {
        myIsEnabled = false;
    }
    public void ColorGray()
    {
        mySpriteRenderer.color = Color.gray;
    }

    public void ResetUnitSprite()
    {
        mySpriteRenderer.color = myOriginalColor;
        myCanvasGroup.alpha = 1f;
    }

    public void FlipSprite()
    {
        mySpriteRenderer.flipX = true;
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SharedScripts;
using SharedScripts.DataId;
using System;

public class SharedCard : MonoBehaviour
{
    //Card Data
    protected CardId myCardId;
    public CardId GetId() { return myCardId; }
    protected CardType myCardType;
    public CardType GetCardType() { return myCardType; }
    protected string myCardName;
    protected string myCardDescription;
    protected int myCardCost;
    public int GetCost() { return myCardCost; }
    protected Sprite myCardSprite;

    //Ability Data
    protected AbilityId myAbilityId;
    protected SharedAbility myAbility;

    //Unit Data 
    protected UnitId myUnitId;

    [SerializeField] protected Image myCardImage;
    [SerializeField] protected GameObject myHPDisplay;
    [SerializeField] protected GameObject myATKDisplay;
    [SerializeField] protected GameObject myExtraStatsPanel;
    [SerializeField] protected GameObject myAbilityDescriptionPanel;
    [SerializeField] protected GameObject mySummonerTag;

    [SerializeField] protected TextMeshProUGUI myTitleText;
    [SerializeField] protected TextMeshProUGUI myCostText;
    [SerializeField] protected TextMeshProUGUI myHitPointsText;
    [SerializeField] protected TextMeshProUGUI myAttackDamageText;
    [SerializeField] protected TextMeshProUGUI myTypeText;
    //[SerializeField] protected TextMeshProUGUI myDescriptionText;
    [SerializeField] protected TextMeshProUGUI myAbilityNameText;
    [SerializeField] protected TextMeshProUGUI myAbilityDescriptionText;
    [SerializeField] protected TextMeshProUGUI myAbilityDurationText;
    [SerializeField] protected TextMeshProUGUI myAbilityCooldownText;
    [SerializeField] protected TextMeshProUGUI myMovementRangeText;
    [SerializeField] protected TextMeshProUGUI myAttackRangeText;

    private void Awake()
    {
        myUnitId = UnitId.INVALID;
        myAbilityId = AbilityId.INVALID;
    }

    public virtual void Init(UnitCardData aCardData, UnitData aUnitData, AbilityData anAbilityData) 
    {
        if (aCardData == null)
        {
            Debug.LogError("[HOOD][CLIENT][CARD] - No CardData provided on MyDecksCard/Init()");
            return;
        }

        // Data
        myCardId = aCardData.myId;
        myCardType = aCardData.myCardType;
        myCardName = aCardData.myName;
        myCardDescription = aCardData.myDescription;
        myCardCost = aCardData.myCost;
        myUnitId = aCardData.myUnitId;
        myAbilityId = aUnitData.myAbilityId;

        // UI
        myCardImage.sprite = aCardData.mySprite;

        myTitleText.text = aCardData.myName;
        myCostText.text = aCardData.myCost.ToString();
        myHitPointsText.text = aUnitData.myShields.ToString();
        myAttackDamageText.text = aUnitData.myAttack.ToString();
        myMovementRangeText.text = SetMovementRangeText(aUnitData.myMovementRange);
        myAttackRangeText.text = SetAttackRangeText(aUnitData.myAttackRange);
        myTypeText.text = "UNIT";
        //myTypeText.text = aCardData.myCardType.ToString(); //mda TODO: re-create a ToString implementation.

        if(myAbilityId != AbilityId.INVALID || anAbilityData != null)
        {
            myAbilityNameText.text = myAbilityId.ToString(); // TODO: re - create a ToString implementation.
            myAbilityDescriptionText.text = SetAbilityDescriptionText(anAbilityData.myDescription);
            myAbilityDurationText.text = SetAbilityDurationText(anAbilityData.myDuration);
            myAbilityCooldownText.text = SetAbilityCooldownText(anAbilityData.myCooldown);

            myAbilityDescriptionPanel.SetActive(true);
        }
        else
        {
            myAbilityDescriptionPanel.SetActive(false);
        }

        if(aUnitData.canSpawnOtherUnits)
        {
            mySummonerTag.SetActive(true);
        }
        else
        {
            mySummonerTag.SetActive(false);
        }
    }

    public virtual void Init(AbilityCardData aCardData, AbilityData anAbilityData)
    {
        if (aCardData == null)
        {
            Debug.LogError("[HOOD][CLIENT][CARD] - No CardData provided on MyDecksCard/Init()");
            return;
        }

        // Data
        myCardId = aCardData.myId;
        myCardType = aCardData.myCardType;
        myCardName = aCardData.myName;
        myCardDescription = aCardData.myDescription;
        myCardCost = aCardData.myCost;
        myAbilityId = aCardData.myAbilityId;

        // UI
        myCardImage.sprite = aCardData.mySprite;

        myTitleText.text = aCardData.myName;
        myCostText.text = aCardData.myCost.ToString();
        myTypeText.text = "TECH";

        myExtraStatsPanel.SetActive(false);
        myHPDisplay.SetActive(false);
        myATKDisplay.SetActive(false);

        //myTypeText.text = aCardData.myCardType.ToString(); //mda TODO: re-create a ToString implementation.

        if (myAbilityId != AbilityId.INVALID)
        {
            myAbilityNameText.text = myAbilityId.ToString(); // TODO: re - create a ToString implementation.
            myAbilityDescriptionText.text = SetAbilityDescriptionText(anAbilityData.myDescription);
            myAbilityDurationText.text = SetAbilityDurationText(anAbilityData.myDuration);
        }
    }

    public void SetDataFromUnit(UnitCardData aCardData, SharedUnit aUnit)
    {
        myCardType = aCardData.myCardType;
        myCardName = aCardData.myName;
        myCardDescription = aCardData.myDescription;
        myCardCost = aCardData.myCost;

        myCardImage.sprite = aCardData.mySprite;

        // UI
        myCardImage.sprite = aCardData.mySprite;

        myTitleText.text = aCardData.myName;
        myCostText.text = aCardData.myCost.ToString();
        myHitPointsText.text = aUnit.GetShield().ToString();
        myAttackDamageText.text = aUnit.GetAttack().ToString();
        myMovementRangeText.text = SetMovementRangeText(aUnit.GetMovementRange());
        myAttackRangeText.text = SetAttackRangeText(aUnit.GetAttackRange());
        myTypeText.text = "UNIT";
        //myTypeText.text = aCardData.myCardType.ToString(); //mda TODO: re-create a ToString implementation.

        if (aUnit.GetAbility() != null)
        {
            myAbilityNameText.text = aUnit.GetAbility().GetAbilityId().ToString();// TODO: re - create a ToString implementation.
            myAbilityDescriptionText.text = SetAbilityDescriptionText(aUnit.GetAbility().GetDescription());
            myAbilityDurationText.text = SetAbilityDurationText(aUnit.GetAbility().GetDurationTimer());
            myAbilityCooldownText.text = SetAbilityCooldownText(aUnit.GetAbility().GetCooldownTimer());

            myAbilityDescriptionPanel.SetActive(true);
        }
        else
        {
            myAbilityDescriptionPanel.SetActive(false);
        }

        if (aUnit.GetIsSpawner())
        {
            mySummonerTag.SetActive(true);
        }
        else
        {
            mySummonerTag.SetActive(false);
        }
    }

    private string SetMovementRangeText(int aRange)
    {
        return "MOV Range- " + aRange;
    }

    private string SetAttackRangeText(int aRange)
    {
        return "ATK Range- " + aRange;
    }

    private string SetAbilityDescriptionText(string aDescription)
    {
        return "- " + aDescription;
    }

    private string SetAbilityDurationText(int aDuration)
    {
        if(aDuration <= 0)
        {
            return "- Duration: Only this turn";
        }
        
        if(aDuration == 1)
        {
            return "- Duration: " + aDuration + " turn";
        }

        if (aDuration > 1)
        {
            return "- Duration: " + aDuration + " turns";
        }

        return "";
    }

    private string SetAbilityCooldownText(int aCooldown)
    {
        return "- Cooldown:" + aCooldown + " turns"; ;
    }
}

using SharedScripts.DataId;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HandleCardTooltipUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private SharedCard myCard;
    private SharedTile myTile;
    private bool myIsHovered;

    private SharedDataLoader myDataLoaderReference;
    private CardTooltipScreenSpaceUI myTooltipReference;

    private Coroutine myCoroutine;

    private void Start()
    {
#if !UNITY_SERVER
        myTile = this.GetComponent<SharedTile>();
        myCard = GameObject.FindWithTag("TooltipCard").GetComponent<SharedCard>();
        myDataLoaderReference = FindObjectOfType<SharedDataLoader>();
        myTooltipReference = FindObjectOfType<CardTooltipScreenSpaceUI>();
#endif
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        myIsHovered = true;
        SharedUnit unit = myTile.GetUnit();

        if(unit != null)
        {
           myCoroutine = StartCoroutine(HoverTimer(unit));
        } 
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        myIsHovered = false;
        myTooltipReference.MakeInvisible();

        if(myCoroutine != null)
        {
            StopCoroutine(myCoroutine);
        }  
    }

    private IEnumerator HoverTimer(SharedUnit aUnit)
    {
        yield return new WaitForSeconds(1.5f);
        if (myIsHovered)
        {
            SetCardData(aUnit);
            myTooltipReference.MakeVisible();
        }
        yield return null;
    }

    private void SetCardData(SharedUnit aUnit)
    {
        CardId cardId = aUnit.GetCardId();

        CardData cardData = myDataLoaderReference.GetCardData(cardId);
        if (cardData != null)
        {
            UnitCardData unitCardData = cardData as UnitCardData;
            UnitData unitData = myDataLoaderReference.GetUnitData(unitCardData.myUnitId);
            if (unitData != null)
            {
                AbilityData abilityData = null;
                if (unitData.myAbilityId != AbilityId.INVALID)
                {
                    abilityData = myDataLoaderReference.GetAbilityData(unitData.myAbilityId);
                }

                myCard.Init(unitCardData, unitData, abilityData);
            }
        }
    }
}

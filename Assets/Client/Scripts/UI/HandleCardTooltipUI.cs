using SharedScripts.DataId;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HandleCardTooltipUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private SharedCard myCard;
    private SharedUnit myHoveredUnit;
    private MatchCard myHoveredCard;
    private bool myIsHovered;

    private SharedDataLoader myDataLoaderReference;
    private CardTooltipScreenSpaceUI myTooltipReference;

    private Coroutine myCoroutine;

    private void Start()
    {
#if !UNITY_SERVER
        myCard = GameObject.FindWithTag("TooltipCard").GetComponent<SharedCard>();
        myDataLoaderReference = FindObjectOfType<SharedDataLoader>();
        myTooltipReference = FindObjectOfType<CardTooltipScreenSpaceUI>();
#endif
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        myIsHovered = true;
        myHoveredCard = null;
        myHoveredUnit = null;

        if (eventData.pointerEnter.TryGetComponent<SharedTile>(out SharedTile tile)) // Tooltip from unit on board
        {
            SharedUnit unit = tile.GetUnit();

            if (unit != null)
            {
                myHoveredUnit = unit;
                myCoroutine = StartCoroutine(HoverTimer());
            }
        }
        else
        {
            MatchCard card = eventData.pointerEnter.GetComponentInParent<MatchCard>();

            if(card != null)
            {
                myHoveredCard = card;
                myCoroutine = StartCoroutine(HoverTimer());
            }   
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        MatchCard card = eventData.pointerEnter.GetComponentInParent<MatchCard>();
        myTooltipReference.MakeInvisible();

        if (card != null) // We need to do this because card contains many elements inside it, and each time we hover over one of them it triggers this method. We need to get out of this method, because we are still inside the card.
        {
            return;
        }

        myTooltipReference.MakeInvisible();
        myIsHovered = false;

        if (myCoroutine != null)
        {
            StopCoroutine(myCoroutine);
        }
    }

    private IEnumerator HoverTimer()
    {
        yield return new WaitForSeconds(1.5f);
        if (myIsHovered)
        {
            SetCardData();
            myTooltipReference.MakeVisible();
        }
        yield return null;
    }

    private void SetCardData()
    {
        CardId cardId = CardId.INVALID;

        if (myHoveredUnit != null)
        {
            cardId = myHoveredUnit.GetCardId();
        }
        else if (myHoveredCard != null)
        {
            cardId = myHoveredCard.GetId();
        }

        CardData cardData = myDataLoaderReference.GetCardData(cardId);
        if (cardData == null)
        {
            return;
        }

        UnitCardData unitCardData = cardData as UnitCardData;

        if (myHoveredUnit != null)
        {
            myCard.SetDataFromUnit(unitCardData, myHoveredUnit);
        }
        else if (myHoveredCard != null)
        {
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

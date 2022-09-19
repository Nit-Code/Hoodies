using SharedScripts.DataId;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SharedHand
{
    private List<CardId> myCards = new();
    private int myMaxCards;

    public SharedHand(int myMaxCards)
    {
        this.myCards = new List<CardId>();
        this.myMaxCards = myMaxCards;
    }

    public List<CardId> GetCards() 
    {
        return myCards;
    }

    public bool HasFreeSlot()
    {
        return myCards.Count < myMaxCards;
    }

    public void SetMaxCards(int aNumber)
    {
        myMaxCards = aNumber;
    }

    public void RemoveCard(CardId anId)
    {
        for(int i = 0; i < myCards.Count; i++)
        {
            if (myCards[i] == anId)
            {
                myCards.RemoveAt(i);
                break;
            }
        }
    }

    public void AddCard(CardId anId)
    {
        if(myCards.Count < myMaxCards)
            myCards.Add(anId);
    }
}

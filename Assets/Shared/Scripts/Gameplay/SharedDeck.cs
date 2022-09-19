using SharedScripts.DataId;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SharedDeck
{ 
    private int myId;
    public int GetId() { return myId; }

    private string myName;
    public string GetName() { return myName; }
    private List<CardId> myCards; // We leave this as a list and not a stack in case we need to draw a specific card at some point
    public List<CardId> GetCards() { return myCards; }

    private CardId myMothership;

    public CardId GetMothership() { return myMothership; }
    public void SetMothership(CardId aCaptain) { myMothership = aCaptain; }

    public SharedDeck(string aName, List<CardId> cards)
    {
        myName = aName;
        myCards = cards;
    }
    
    public bool IsEmpty()
    {
        return myCards.Count == 0;
    }

    public CardId DrawCard()
    {
        if (!IsEmpty())
        {
            CardId cardToDraw = myCards[0];
            myCards.RemoveAt(0);
            return cardToDraw;
        }
        return CardId.INVALID;
    }
}

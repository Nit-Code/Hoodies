using SharedScripts.DataId;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SharedPlayer : IEquatable<SharedPlayer>
{
    private List<SharedUnit> myPlacedUnits;
    private SharedDeck myDeckReference;
    public SharedDeck GetDeck() { return myDeckReference; }
    public void SetDeck(SharedDeck aDeck) { myDeckReference = aDeck; }
    private SharedHand myHand;
    private SharedUnit myMothership;
    public SharedUnit GetMotherShip() { return myMothership; }
    public void SetMotherShip(SharedUnit aMothership) { myMothership = aMothership; }

    private string myPlayerSessionId;
    private string myUsername;
    public string GetUsername() { return myUsername; }
    public void SetUsername(string aUsername) { myUsername = aUsername; }
    private int myCurrentMaximumEnergy;
    private int myTopEnergy;
    public int GetCurrentMaximumEnergy() { return myCurrentMaximumEnergy; }
    public void SetCurrentMaximumEnergy(int aMaxEnergy) { myCurrentMaximumEnergy = aMaxEnergy; } // TO DO: RULE CLASS
    // falta top energy
    private int myEnergy;
    public int GetEnergy() { return myEnergy; }
    private BoardSide myBoardSide;
    public void SetBoardSide(BoardSide aBoardSide) { myBoardSide = aBoardSide; }
    public BoardSide GetBoardSide() { return myBoardSide; }
    public enum BoardSide
    {
        RIGHT,
        LEFT
    }

    public SharedPlayer(string aPlayerSessionId, SharedDeck aDeck)
    {
        myCurrentMaximumEnergy = 2; // TODO: defaulting maximum energy here until its being properly set using SetCurrentMaximumEnergy()
        myTopEnergy = 10;
        myPlayerSessionId = aPlayerSessionId;
        myDeckReference = aDeck;
        myHand = new SharedHand(5); // TODO: Hardcoded max hand size
        myPlacedUnits = new();
    }

    public string GetSessionId()
    {
        return myPlayerSessionId;
    }

    public bool Equals(SharedPlayer otherPlayer)
    {
        return myPlayerSessionId == otherPlayer.myPlayerSessionId;
    }

    public void AddPlacedUnit(SharedUnit aUnit)
    {
        myPlacedUnits.Add(aUnit);
    }

    public void RemovePlacedUnit(SharedUnit aUnit)
    {
        myPlacedUnits.Remove(aUnit);
    }

    public bool IsPlayersCapitainAlive()
    {
        return myMothership.IsAlive();
    }

    public CardId GetMothershipCard()
    {
        if (myDeckReference != null)
        {
            return myDeckReference.GetMothership();
        }
        else 
        {
            return CardId.INVALID;
        }
    }

    public void SetCaptain(SharedUnit aUnit)
    {
        myMothership = aUnit;
    }

    public Vector2Int GetCaptainsPosition()
    {
        if (myMothership != null) 
        {
            return myMothership.GetPosition();
        }
        else
        {
            Shared.LogError("[HOOD][PLAYER] - GetCaptainsPosition");
            return new Vector2Int(-1,-1);
        }
    }

    public bool IsCardInHand(CardId aCard)
    {
        return myHand.GetCards().Contains(aCard);
    }

    public bool CanSubstractEnergyCost(int anActionCost) // We use this to check before requests
    {
        return anActionCost <= myEnergy;
    }

    public bool TrySubstractEnergyCost(int anActionCost)
    {
        if (anActionCost <= myEnergy)
        {
            SubstractEnergy(anActionCost);
            return true;
        }

        //TODO: Implement me
        return false;
    }

    //TODO: to ensure the correct operation is performed, consider changing input parameter to uint
    public void AddEnergy(int anAmmount)
    {
        myEnergy += anAmmount;

        if (myEnergy > myCurrentMaximumEnergy)
        {
            myEnergy = myCurrentMaximumEnergy;
        }
    }

    public void FillEnergy()
    {
        myEnergy = myCurrentMaximumEnergy;
    }

    public void IncreaseCurrentMaximumEnergy()
    {
        if(myCurrentMaximumEnergy < myTopEnergy)
        {
            myCurrentMaximumEnergy += 1;
        }     
    }

    //TODO: to ensure the correct operation is performed, consider changing input parameter to uint
    private void SubstractEnergy(int anAmmount)
    {
        myEnergy -= anAmmount;
    }

    public CardId TryDrawCard()
    {
        if (CanDrawCard())
        {
            CardId drawedCard = myDeckReference.DrawCard();
            AddCardToHand(drawedCard);
            return drawedCard;
        }
        return CardId.INVALID;
    }

    public bool CanDrawCard()
    {
        if (myHand.HasFreeSlot() && !myDeckReference.IsEmpty())
        {
            return true;
        }
        return false;
    }

    public void AddCardToHand(CardId card)
    {
        myHand.AddCard(card);
    }

    public void RemoveCardFromHand(CardId card)
    {
        if(myHand == null)
        {
            return;
        }

        List<CardId> cards = myHand.GetCards();

        for (int i = 0; i < cards.Count; i++)
        {
            if(cards[i] == card)
            {
                cards.RemoveAt(i);
                return;
            }
        }
    }
}

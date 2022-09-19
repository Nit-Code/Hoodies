using System.Collections.Generic;
using UnityEngine;
using SharedScripts;
using SharedScripts.DataId;

// EXAMPLE:
// Use inheritance in data classes when the child classes have need of mutually exclusive fields
// Add a type enum field in the base class so that child class type can be safely deduced and casted

[System.Serializable]
public class UnitCardData : CardData
{
    public UnitId myUnitId;
}

[System.Serializable]
public class AbilityCardData : CardData
{
    public AbilityId myAbilityId;
}

[System.Serializable]
public abstract class CardData
{
    public string myName;
    public CardId myId;
    public CardType myCardType;

    [TextArea] public string myDescription;   
    public int myCost;
    public Sprite mySprite; // TODO: used on MyDecks consider creating a Sprites_Def scriptable object if we want the same sprite on the Unit and on the MyDecksCard classes
}

[CreateAssetMenu(fileName = "Cards_Inst", menuName = "DataListsInstances/Cards_Inst")]
public class Cards_Def : ScriptableObject
{
    [SerializeField] public List<UnitCardData> myUnitCards;
    [SerializeField] public List<AbilityCardData> myAbilityCards;
}
using SharedScripts.DataId;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SharedSlot
{
    MatchCard myCard;
    public MatchCard GetCard() { return myCard; }
    public void SetCard(MatchCard card) { myCard = card; }
    public void RemoveCard() { myCard = null; }

    public bool IsFree() { return myCard != null; }
}

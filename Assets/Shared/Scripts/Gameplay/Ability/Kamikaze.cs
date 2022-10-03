using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kamikaze : SharedAbility
{
    protected new void Start()
    {
        base.Start();
    }
    public override void ApplyAbilityEffect()
    {
        foreach (SharedTile tile in myCastTiles)
        {
            SharedUnit affectedUnit = tile.GetUnit();

            if (affectedUnit != null)
            {
                affectedUnit.ModifyShield(-4);
            }
        }
        myOwnerUnitReference.ModifyShield(-999); 
    }

    public override void UpdateAbilityStatus()
    {
    }

    protected override void ApplyVisualEffects()
    {
    }
    protected override void RemoveVisualEffects()
    {
    }
}

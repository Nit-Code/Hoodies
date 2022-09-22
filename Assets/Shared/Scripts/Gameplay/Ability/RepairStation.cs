using SharedScripts;
using SharedScripts.DataId;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepairStation : SharedAbility
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

            if (affectedUnit == null)
            {
                continue;
            }

            if (myOwnerUnitReference.IsOwnedByPlayer(affectedUnit.GetPlayer()))
            {
                affectedUnit.ModifyShield(1);
            }
        }
        ApplyVisualEffects();
    }

    public override void UpdateAbilityStatus()
    {
        base.UpdateAbilityStatus();
    }

    protected override void ApplyVisualEffects()
    {
        foreach (SharedTile tile in this.myCastTiles)
        {
            tile.ColorTile(this.myTileColor);
        }
    }
    protected override void RemoveVisualEffects()
    {
        foreach (SharedTile tile in this.myCastTiles)
        {
            tile.ChangeBaseColor(TileColor.WHITE);
            tile.GoToBaseColor(true);
        }
    }
}

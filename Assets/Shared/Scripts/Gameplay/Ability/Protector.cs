using SharedScripts.DataId;
using SharedScripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Protector : SharedAbility
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
                tile.GetUnit().TryAddStatusEffect(StatusEffectId.PROTECTOR_AURA);
            }
        }
        ApplyVisualEffects();
    }

    public override void UpdateAbilityStatus()
    {
        base.UpdateAbilityStatus();

        RemoveVisualEffects();

        myCastTiles = myBoardReference.GetShapeFromCenterTileCoord(this.myIncludeCenter, this.GetShape(), this.myAreaShapeSize, this.myOwnerUnitReference.GetPosition());

        for (int i = 0; i < myCastTiles.Count; i++)
        {
            if (myCastTiles[i].GetTileType() == TileType.BLACKHOLE)
            {
                myCastTiles.RemoveAt(i);
            }
            else if (myCastTiles[i].GetUnit() != null && !myCastTiles[i].GetUnit().IsOwnedByPlayer(myOwnerUnitReference.GetPlayer()))
            {
                myCastTiles.RemoveAt(i);
            }
        }
        ApplyAbilityEffect();
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

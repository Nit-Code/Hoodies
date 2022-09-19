using SharedScripts.DataId;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EngineOverdrive : SharedAbility
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
                tile.GetUnit().TryAddStatusEffect(StatusEffectId.IMPROVED_ENGINES);
            }
        }
    }

    public override void UpdateAbilityStatus()
    {
        base.UpdateAbilityStatus();
    }

    protected override void ApplyVisualEffects()
    {

    }

    protected override void RemoveVisualEffects()
    {

    }
}



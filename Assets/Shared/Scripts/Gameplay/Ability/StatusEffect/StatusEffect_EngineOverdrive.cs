using SharedScripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: why is this named like class_specialization: class ?
public class StatusEffect_ImprovedEngines : SharedStatusEffect
{
    private SharedBoard myBoardReference;

    protected new void Start()
    {
        myBoardReference = FindObjectOfType<SharedBoard>();
        base.Start();
    }

    public override void ApplyEffect()
    {
        myOwnerUnitReference.ModifyMovementRange(2);
        ApplyVisualEffects();
    }

    public override void RemoveEffect()
    {
        myOwnerUnitReference.ModifyMovementRange(-2);
        RemoveVisualEffects();
    }

    public override void ApplyVisualEffects()
    {
        SharedTile unitTile = myBoardReference.GetTile(myOwnerUnitReference.GetPosition().x, myOwnerUnitReference.GetPosition().y);

        // Color unit?
    }

    protected override void RemoveVisualEffects()
    {
        SharedTile unitTile = myBoardReference.GetTile(myOwnerUnitReference.GetPosition().x, myOwnerUnitReference.GetPosition().y);

        unitTile.ColorTile(TileColor.WHITE);
    }
}


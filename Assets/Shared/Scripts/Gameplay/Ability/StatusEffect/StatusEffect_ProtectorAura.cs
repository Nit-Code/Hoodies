using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: why is this named like class_specialization: class ?
public class StatusEffect_ProtectorAura : SharedStatusEffect
{
    protected new void Start()
    {
        base.Start();
    }

    public override void ApplyEffect()
    {
        myOwnerUnitReference.ModifyShield(2);
    }

    public override void RemoveEffect()
    {
        //if(myOwnerUnit.GetShield() > 2)
        myOwnerUnitReference.ModifyShield(-2);
        //else
        //myOwnerUnit.SetShield(1)
    }
    public override void ApplyVisualEffects()
    {

    }

    protected override void RemoveVisualEffects()
    {

    }
}



using SharedScripts.DataId;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SharedStatusEffect : MonoBehaviour
{
    protected StatusEffectId myId;
    public StatusEffectId GetId() { return myId; }
    protected int myDuration; // How long the effect lasts on a unit once it has been applied. Duration of 0 means that the effect only lasts for the turn it was applied in
    protected int myDurationTimer;

    protected SharedUnit myOwnerUnitReference;
    public void RemoveUnitReference()
    {
        this.myOwnerUnitReference = null;
    }


    public abstract void ApplyEffect();
    public virtual void RemoveEffect()
    {
        RemoveVisualEffects();
    }
    public abstract void ApplyVisualEffects();
    protected abstract void RemoveVisualEffects();

    public void Init(SharedUnit aUnit, StatusEffectData aData)
    {
        myId = aData.myId;
        myOwnerUnitReference = aUnit;
        myDuration = aData.myDuration;
        myDurationTimer = 0;
    }

    protected void Start()
    {
        ApplyEffect();
    }

    public void UpdateTimerStatus()
    {
        myDurationTimer += 1;
        CheckStatus();
    }

    public void CheckStatus()
    {
        if (myDuration == 0 || myDurationTimer > myDuration)
        {
            myOwnerUnitReference.RemoveStatusEffect(this);
        }
        else
        {
            ApplyVisualEffects();
        }  
    }
}

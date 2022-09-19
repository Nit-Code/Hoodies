using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using SharedScripts;
using SharedScripts.DataId;

[System.Serializable]
public class StatusEffectData
{
    public string myName;
    public StatusEffectId myId;

    public string myDescription;
    public int myDuration;
    public Sprite mySprite;
    public AnimatorOverrideController myAnimationController;
    public Color myUnitColor;
}

[CreateAssetMenu(fileName = "StatusEffects_Inst", menuName = "DataListsInstances/StatusEffects_Inst")]
public class StatusEffects_Def : ScriptableObject
{
    [SerializeField] public List<StatusEffectData> myStatusEffects;
}


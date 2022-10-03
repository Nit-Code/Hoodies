using System.Collections.Generic;
using UnityEngine;
using SharedScripts;
using SharedScripts.DataId;

[System.Serializable]
public class UnitData
{
    public string myName;
    public UnitId myId;

    public int myAttack;
    public int myAttackRange;
    public int myShields;
    public int myMovementRange;
    public AbilityId myAbilityId;

    public bool canSpawnOtherUnits;

    public AnimatorOverrideController myDefaultOverrideAnimatorController;
    public AnimatorOverrideController myBlueOverrideAnimatorController;
    public AnimatorOverrideController myRedOverrideAnimatorController;
    public Sprite mySprite;
}

[CreateAssetMenu(fileName = "Units_Inst", menuName = "DataListsInstances/Units_Inst")]
public class Units_Def : ScriptableObject
{
    [SerializeField] public List<UnitData> myUnits;
}

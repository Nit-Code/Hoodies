using System.Collections.Generic;
using UnityEngine;
using SharedScripts;
using SharedScripts.DataId;

[System.Serializable]
public class AbilityData
{
    public string myName;
    public AbilityId myId;
    public string myDescription;

    public AreaShape myAreaShape;
    public int myAreaShapeSize;
    public bool myIncludeCenter;
    public TileColor myTileColor;
    public int myCost;
    public int myCastingRange; // Only applies if it's being cast by a unit
    public int myCooldown; // Only applies if it's being cast by a unit
    public int myDuration; // How long the ability lasts on the board after being cast

    public StatusEffectId myStatusEffect;

    public AnimatorOverrideController myOverrideAnimatorController;
    public Sprite mySprite;
}

[CreateAssetMenu(fileName = "Abilities_Inst", menuName = "DataListsInstances/Abilities_Inst")]
public class Abilities_Def : ScriptableObject
{
    [SerializeField] public List<AbilityData> myAbilities;
}
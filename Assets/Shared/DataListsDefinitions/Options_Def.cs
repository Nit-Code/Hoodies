using System.Collections.Generic;
using UnityEngine;
using SharedScripts.DataId;


[System.Serializable]
public class FloatRangeOptionData: OptionData
{
    public FloatRangeOptionId myId;
    public float myMinValue = 0.0f;
    public float myDefaultValue = 0.5f;
    public float myMaxValue = 1.0f;
}

[System.Serializable]
public class BooleanOptionData : OptionData
{
    public BooleanOptionId myId;
    public bool myDefaultValue = false;
}

[System.Serializable]
public abstract class OptionData
{
    public string myName;
    public string myDescription;
}

[CreateAssetMenu(fileName = "Options_Inst", menuName = "DataListsInstances/Options_Inst")]
public class Options_Def : ScriptableObject
{
    public List<FloatRangeOptionData> myFloatOptions;
    public List<BooleanOptionData> myBooleanOptions;
}

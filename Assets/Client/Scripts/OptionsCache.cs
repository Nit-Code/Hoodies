using System.Collections.Generic;
using UnityEngine;
using SharedScripts.DataId;

[System.Serializable]
public class OptionsCache : ISaveable
{
    // Dictionaries can't be serialized so we construct a pair of matching lists and hope for the best
    public List<FloatRangeOptionId> myFloatKeyList;
    public List<float> myFloatValueList;

    public List<BooleanOptionId> myBoolKeyList;
    public List<bool> myBoolValueList;

    public string myUsername;
    public string myPartialFilename;
    private const string MY_EXTENSION = "_options.dat";

    public OptionsCache()
    {

    }

    public OptionsCache(Dictionary<FloatRangeOptionId, float> aFloatOptionsMap, Dictionary<BooleanOptionId, bool> aBooleanOptionsMap, string anUsername)
    {
        myUsername = anUsername;
        myPartialFilename = "";
        myFloatValueList = new List<float>();
        myFloatKeyList = new List<FloatRangeOptionId>();
        foreach (KeyValuePair<FloatRangeOptionId, float> floatOptionValue in aFloatOptionsMap)
        {
            myFloatKeyList.Add(floatOptionValue.Key);
            myFloatValueList.Add(floatOptionValue.Value);
        }

        myBoolValueList = new List<bool>();
        myBoolKeyList = new List<BooleanOptionId>();
        foreach (KeyValuePair<BooleanOptionId, bool> boolOptionValue in aBooleanOptionsMap)
        {
            myBoolKeyList.Add(boolOptionValue.Key);
            myBoolValueList.Add(boolOptionValue.Value);
        }
    }

    public Dictionary<FloatRangeOptionId, float> GetFloatRangeOptionValues() 
    {
        if ((myFloatKeyList == null || myFloatValueList == null) ||
            (myFloatKeyList.Count != myFloatValueList.Count))
        {
            Shared.Log("[HOOD][CLIENT][OPTIONS] - GetFloatRangeOptionValues() Invalid lists");
            return null;
        }

        if (myFloatKeyList.Count == 0)
        {
            Shared.Log("[HOOD][CLIENT][OPTIONS] - GetFloatRangeOptionValues() No options of this type");
        }

        Dictionary<FloatRangeOptionId, float> map = new Dictionary<FloatRangeOptionId, float>();
        for (int i = 0; i < myFloatKeyList.Count; i++) 
        {
            map.Add(myFloatKeyList[i], myFloatValueList[i]);
        }

        return map;
    }

    public Dictionary<BooleanOptionId, bool> GetBoolOptionValues()
    {
        if ((myBoolKeyList == null || myBoolValueList == null) ||
            (myBoolKeyList.Count != myBoolValueList.Count))
        {
            Shared.LogError("[HOOD][CLIENT][OPTIONS] - GetBoolOptionValues() Invalid lists");
            return null;
        }

        if (myBoolKeyList.Count == 0) 
        {
            Shared.Log("[HOOD][CLIENT][OPTIONS] - GetBoolOptionValues() No options of this type");
        }

        Dictionary<BooleanOptionId, bool> map = new Dictionary<BooleanOptionId, bool>();
        for (int i = 0; i < myBoolKeyList.Count; i++)
        {
            map.Add(myBoolKeyList[i], myBoolValueList[i]);
        }

        return map;
    }

    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }

    public void LoadFromJson(string jsonToLoadFrom)
    {
        JsonUtility.FromJsonOverwrite(jsonToLoadFrom, this);
    }

    public string GenerateFullFilename(string aPartialFilename)
    {
        myPartialFilename = aPartialFilename;
        return aPartialFilename + MY_EXTENSION;
    }

    public string GetPartialFilename()
    {
        return myPartialFilename;
    }
}
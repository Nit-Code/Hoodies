public interface ISaveable
{
    string ToJson();
    void LoadFromJson(string aJson);
    string GenerateFullFilename(string aName);
}

// Used to save scripts to json that implement ISaveable
// based on https://github.com/UnityTechnologies/UniteNow20-Persistent-Data
public static class SaveDataManager
{
    public static bool LoadJsonData(ISaveable aSaveable, string aName)
    {
        if (FileReadWrite.LoadFromFile(aSaveable.GenerateFullFilename(aName), out var json))
        {
            aSaveable.LoadFromJson(json);
            return true;
            //Debug.LogError("[HOOD][GUEST][SAVE] - StartLoadFromPersistance complete");
        }
        return false;
    }

    public static bool SaveJsonData(ISaveable aSaveable, string aName)
    {
        if (FileReadWrite.WriteToFile(aSaveable.GenerateFullFilename(aName), aSaveable.ToJson()))
        {
            return true;
            //Debug.LogError("[HOOD][GUEST][SAVE] - Save successful");
        }
        return false;
    }
}
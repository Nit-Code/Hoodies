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
    public static void LoadJsonData(ISaveable aSaveable, string aName)
    {
        if (FileReadWrite.LoadFromFile(aSaveable.GenerateFullFilename(aName), out var json))
        {
            aSaveable.LoadFromJson(json);
            //Debug.LogError("[HOOD][GUEST][SAVE] - Load complete");
        }
    }

    public static void SaveJsonData(ISaveable aSaveable, string aName)
    {
        if (FileReadWrite.WriteToFile(aSaveable.GenerateFullFilename(aName), aSaveable.ToJson()))
        {
            //Debug.LogError("[HOOD][GUEST][SAVE] - Save successful");
        }
    }
}
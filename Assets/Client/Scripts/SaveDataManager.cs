public interface ISaveable
{
    string ToJson();
    void LoadFromJson(string a_Json);
    string FileNameToUseForData();
}

// Used to save scripts to json that implement ISaveable
// based on https://github.com/UnityTechnologies/UniteNow20-Persistent-Data
public static class SaveDataManager
{
    public static void SaveJsonData(ISaveable aSaveable)
    {
        if (FileReadWrite.WriteToFile(aSaveable.FileNameToUseForData(), aSaveable.ToJson()))
        {
            //Debug.LogError("[HOOD][GUEST][SAVE] - Save successful");
        }
    }

    public static void LoadJsonData(ISaveable aSaveable)
    {
        if (FileReadWrite.LoadFromFile(aSaveable.FileNameToUseForData(), out var json))
        {
            aSaveable.LoadFromJson(json);
            //Debug.LogError("[HOOD][GUEST][SAVE] - Load complete");
        }
    }
}
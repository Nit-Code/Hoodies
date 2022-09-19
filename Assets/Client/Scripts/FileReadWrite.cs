using System;
using System.IO;
using UnityEngine;

// Saves a file to the operating system's persistent data path
// based on https://github.com/UnityTechnologies/UniteNow20-Persistent-Data
public static class FileReadWrite
{
    public static string ourCachePath = Application.persistentDataPath;

    public static bool WriteToFile(string aFileName, string aFileContents)
    {
        var fullPath = Path.Combine(ourCachePath, aFileName);
        Debug.Log("[HOOD][CLIENT][FILE] - Writing to path: " + fullPath.ToString());
        try
        {
            File.WriteAllText(fullPath, aFileContents);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("[HOOD][CLIENT][FILE] - Failed writing to path: " + fullPath.ToString() + " with exeption: " + e.Message);
            return false;
        }
    }

    public static bool LoadFromFile(string aFileName, out string anOutResult)
    {
        var fullPath = Path.Combine(ourCachePath, aFileName);
        Debug.Log("[HOOD][CLIENT][FILE] - Reading from path: " + fullPath.ToString());
        try
        {
            anOutResult = File.ReadAllText(fullPath);
            return true;
        }
        catch (Exception e)
        {
            Debug.Log("[HOOD][CLIENT][FILE] - Failed reading from path: " + fullPath.ToString() + ", file should be created after a successful login, can probably ignore. Exeption: " + e.Message);
            anOutResult = "";
            return false;
        }
    }
}
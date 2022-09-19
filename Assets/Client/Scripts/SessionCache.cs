using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SessionCache : ISaveable
{
    public string myIdToken;
    public string myAccessToken;
    public string myRefreshToken;
    public string myUserId;
    public string myUsername;

    public SessionCache()
    {
    }

    public SessionCache(string anIdToken, string anAcessToken, string aRefreshToken, string anUserId, string anUsername)
    {
        myIdToken = anIdToken;
        myAccessToken = anAcessToken;
        myRefreshToken = aRefreshToken;
        myUserId = anUserId;
        myUsername = anUsername;
    }

    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }

    public void LoadFromJson(string jsonToLoadFrom)
    {
        JsonUtility.FromJsonOverwrite(jsonToLoadFrom, this);
    }

    public string FileNameToUseForData()
    {
        // stored at : C:\Users\[USER_NAME]\AppData\LocalLow\hoodies\all
        string filename = CLU.GetSessionCacheFilename();
        if (string.IsNullOrEmpty(filename)) 
        {
            filename = "dev";
        }
        return filename + ".dat";
    }
}

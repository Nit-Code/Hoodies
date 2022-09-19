using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LobbyCache : ISaveable
{
    public string myDevLobbyId;

    public LobbyCache()
    { 

    }

    public LobbyCache(string aDevLobbyId)
    {
        myDevLobbyId = aDevLobbyId;
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
        return "devLobbyId.dat";
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*The Following scenarios are supported*/
/*
SCENARIO 1
if -SESSION_CACHE_LOAD_ENABLED True and -SESSION_CACHE_FILENAME "someName"

on boot: the game will try to load a session cache named someName.dat
on update: the game will write to session cache named someName.dat

SCENARIO 2
if -SESSION_CACHE_LOAD_ENABLED True and -SESSION_CACHE_FILENAME ""

on boot: the game will try to login
on update: the game will write to session cache named cognitoUsername.dat

SCENARIO 3
if -SESSION_CACHE_LOAD_ENABLED False 

on boot: the game will try to login 
on update: the game will write to session cache named cognitoUsername.dat 
*/



[System.Serializable]
public class SessionCache : ISaveable
{
    public string myIdToken;
    public string myAccessToken;
    public string myRefreshToken;
    public string myUserId;
    public string myUsername;
    public string myPartialFilename;
    private const string MY_EXTENSION = ".dat";

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
        myPartialFilename = "";
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

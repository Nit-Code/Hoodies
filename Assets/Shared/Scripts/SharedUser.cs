using Amazon.CognitoIdentity;
using System.Collections.Generic;
using UnityEngine;

public class SharedUser : MonoBehaviour
{
    private SessionCache myUserSessionCache;
    public SessionCache GetUserSessionCache() { return myUserSessionCache; }
    
    private CognitoAWSCredentials myCognitoCredentials;
    public void SetCognitoCredentials(CognitoAWSCredentials aCognito) { myCognitoCredentials = aCognito; }
    public CognitoAWSCredentials GetCognitoCredentials() { return myCognitoCredentials; }

    private bool myIsUserSessionCacheSet;

    private List<SharedDeck> myDecks;
    public List<SharedDeck> GetDecks() { return myDecks; }

    public void AddDeck(SharedDeck aDeck)
    {
        if(!myDecks.Contains(aDeck))
            myDecks.Add(aDeck);
    }
    
    private void Awake()
    {
        myCognitoCredentials = null;
        myIsUserSessionCacheSet = false;
    }

    public void SetSessionCache(SessionCache aUserSessionCache)
    {
        myUserSessionCache = aUserSessionCache;
        SaveDataManager.SaveJsonData(myUserSessionCache);
        myIsUserSessionCacheSet = true;
    }

    public void CleanSessionCache()
    {
        SessionCache cleanCache = new SessionCache("", "", "", "", myUserSessionCache.myUsername);
        myIsUserSessionCacheSet = false;
        SetSessionCache(cleanCache);
    }

    public string GetUserId() 
    {
        if (!myIsUserSessionCacheSet)
        {
            Shared.LogError("[HOOD][CLIENT][CACHE] - GetUserId");
            return null;
        }

        return myUserSessionCache.myUserId;
    }


    public string GetUsername()
    {
        if (!myIsUserSessionCacheSet)
        {
            Shared.LogError("[HOOD][CLIENT][CACHE] - GetUsername");
            return null;
        }

        return myUserSessionCache.myUsername;
    }
}

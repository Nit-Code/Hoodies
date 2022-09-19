using System;
using System.Threading.Tasks;
using UnityEngine;
using Amazon;
using Amazon.GameLift;
using Amazon.GameLift.Model;
using Amazon.CognitoIdentity;

/*
Class goal:
    - Establishes connection with server so we can send custom non gamelift api messages to NetworkServer
    - Interact with gamelift api 
*/

public class GameLiftClient : MonoBehaviour
{
    private AmazonGameLiftClient myAmazonGameLiftClient;
    //public static bool ourIsProd; //TODO: should this be in Shared with the rest of the global data?

    // reference
    private NetworkClient myNetworkClientReference;
    private SharedUser mySharedUserReference;
    public string myCachedUserId;
    private GameSession myGameSession;
    private AuthenticationManager myAuthenticationManagerReference;

    private string myLocalGameSessionId;
    private string myLocalPlayerSessionId;

    private int myToLoad;
    private int myLoaded;



    private void Awake()
    {
        myToLoad = 3;
        myLoaded = 0;

        if (TryGetComponent<NetworkClient>(out myNetworkClientReference))
            myLoaded++;

        if (TryGetComponent<SharedUser>(out mySharedUserReference))
            myLoaded++;

        if (TryGetComponent<AuthenticationManager>(out myAuthenticationManagerReference))
            myLoaded++;

        if (!IsReadyToStart()) 
            Debug.LogError("[HOOD][CLIENT][GAMELIFT] - Not Ready.");

        Reset();
    }

    private bool IsReadyToStart()
    {
        return myToLoad == myLoaded;
    }

    public void CreateOrJoinMatch(string aMatchType, string aGameSessionId = "")
    {
        myCachedUserId = mySharedUserReference.GetUserId(); // TODO: why is this doubly cached?

        switch (aMatchType)
        {
            case "PUBLIC":
                SearchMatch();
                break;
            case "PRIVATE_HOST":
                if (!CLU.GetIsConnectLocalEnabled())
                    SetupRemotePrivateMatch();
                else
                    SetupLocalPrivateMatch();
                break;
            case "PRIVATE_GUEST":
                if (!CLU.GetIsConnectLocalEnabled())
                    ConnectToRemoteMatch(aGameSessionId);
                else
                    ConnectToLocalMatch();
                break;
            default:
                Debug.LogError("[HOOD][CLIENT][GAMELIFT] - Unhandled state at CreateMatch()");
                break;
        }
    }

    public bool GetIsGameSessionActive()
    {
        return myGameSession != null;
    }

    public string GetGameSessionId()
    {
        if (!CLU.GetIsConnectLocalEnabled())
        {
            if (myGameSession != null)
            {
                return myGameSession.GameSessionId;
            }
            else
            {
                Debug.LogError("[HOOD][CLIENT][GAMELIFT] - no active game session");
                return "";
            }
        }
        else
        {
            return myLocalGameSessionId;
        }
    }

    private void Reset()
    {
        myLocalGameSessionId = "";
        myLocalPlayerSessionId = "";
    }

    async private void CreatePlayerSession(GameSession gameSession, string aDebugFlow = "NONE")
    {
        int maxRetryAttempts = 10;
        if (aDebugFlow == "PRIVATE_HOST")
        {
            Debug.Log("[HOOD][CLIENT][PRIVATE_LOBBY][HOST] - Step 5/9 - GameLiftClient creating PlayerSession on GameSession with " + maxRetryAttempts + " attempts.");
        }
        else if (aDebugFlow == "PRIVATE_GUEST")
        {
            Debug.Log("[HOOD][CLIENT][PRIVATE_LOBBY][GUEST] - Step 5/9 - GameLiftClient creating PlayerSession on GameSession with " + maxRetryAttempts + " attempts.");
        }

        PlayerSession playerSession = null;

        await RetryHelper.RetryOnExceptionAsync<Exception>
        (maxRetryAttempts, async () =>
        {
            playerSession = await CreatePlayerSessionAsync(gameSession);
        });

        if (playerSession != null)
        {
            Debug.Log("[HOOD][CLIENT][GAMELIFT] - Player Session Created.");
            if (aDebugFlow == "PRIVATE_HOST")
            {
                Debug.Log("[HOOD][CLIENT][PRIVATE_LOBBY][HOST] - Step 6/9 - PlayerSession created successfully. Ip: " + playerSession.IpAddress + " Port: " + playerSession.Port + "Id:" + playerSession.PlayerSessionId);
            }
            else if (aDebugFlow == "PRIVATE_GUEST")
            {
                Debug.Log("[HOOD][CLIENT][PRIVATE_LOBBY][HOST] - Step 6/9 - PlayerSession created successfully. Ip: " + playerSession.IpAddress + " Port: " + playerSession.Port + "Id:" + playerSession.PlayerSessionId);
            }

            // establish connection with server
            myNetworkClientReference.ConnectToServer(playerSession.IpAddress, playerSession.Port, playerSession.PlayerSessionId, aDebugFlow);
        }
        else
        {
            if (aDebugFlow == "PRIVATE_HOST")
            {
                Debug.LogError("[HOOD][CLIENT][PRIVATE_LOBBY][HOST] - Step 6/9 - PlayerSession failed to create.");
            }
            else if (aDebugFlow == "PRIVATE_GUEST")
            {
                Debug.LogError("[HOOD][CLIENT][PRIVATE_LOBBY][GUEST] - Step 6/9 - PlayerSession failed to create.");
            }

            myNetworkClientReference.PlayerSessionCreationFailed();
        }
    }

    private void CreateLocalPlayerSession(string aPLayerSessionId)
    {
        myLocalPlayerSessionId = aPLayerSessionId;
        if (string.IsNullOrEmpty(myLocalPlayerSessionId) || !CLU.GetIsLocalServerURLSet()) 
        {
            Debug.LogError("[HOOD][CLIENT][PRIVATE_LOBBY][LOCAL]. - PlayerSession failed to create.");
            myNetworkClientReference.PlayerSessionCreationFailed();
            return;
        }
        
        // establish connection with server
        myNetworkClientReference.ConnectToServer(CLU.GetConnectLocalServerIP(), CLU.GetConnectLocalServerPort(), myLocalPlayerSessionId);
    }

    async private Task<PlayerSession> CreatePlayerSessionAsync(GameSession aGameSession)
    {
        var createPlayerSessionRequest = new CreatePlayerSessionRequest();
        createPlayerSessionRequest.GameSessionId = aGameSession.GameSessionId;
        createPlayerSessionRequest.PlayerId = myCachedUserId;

        Task<CreatePlayerSessionResponse> createPlayerSessionResponseTask = myAmazonGameLiftClient.CreatePlayerSessionAsync(createPlayerSessionRequest);
        CreatePlayerSessionResponse createPlayerSessionResponse = await createPlayerSessionResponseTask;

        string playerSessionId = createPlayerSessionResponse.PlayerSession != null ? createPlayerSessionResponse.PlayerSession.PlayerSessionId : "N/A";
        Debug.Log((int)createPlayerSessionResponse.HttpStatusCode + " PLAYER SESSION CREATED: " + playerSessionId);
        return createPlayerSessionResponse.PlayerSession;
    }

    async private Task<GameSession> CreateGameSessionAsync()
    {
        Debug.Log("CreateGameSessionAsync");
        var createGameSessionRequest = new Amazon.GameLift.Model.CreateGameSessionRequest();
        createGameSessionRequest.FleetId = Client.GetFleetId(); // can also use AliasId
        createGameSessionRequest.CreatorId = myCachedUserId;
        createGameSessionRequest.MaximumPlayerSessionCount = 2; // search for two player game
        Task<CreateGameSessionResponse> createGameSessionRequestTask = myAmazonGameLiftClient.CreateGameSessionAsync(createGameSessionRequest);
        Debug.Log("after task createGameSessionRequestTask");
        CreateGameSessionResponse createGameSessionResponse = await createGameSessionRequestTask;
        Debug.Log("after createGameSessionRequestTask");

        string gameSessionId = createGameSessionResponse.GameSession != null ? createGameSessionResponse.GameSession.GameSessionId : "N/A";
        Debug.Log((int)createGameSessionResponse.HttpStatusCode + " GAME SESSION CREATED: " + gameSessionId);

        return createGameSessionResponse.GameSession;
    }
    async private Task<GameSession> CreatePrivateGameSessionAsync()
    {
        Debug.Log("CreateGameSessionAsync");
        var createGameSessionRequest = new Amazon.GameLift.Model.CreateGameSessionRequest();
        createGameSessionRequest.FleetId = Client.GetFleetId(); // can also use AliasId
        createGameSessionRequest.CreatorId = myCachedUserId;
        createGameSessionRequest.MaximumPlayerSessionCount = 2; // search for two player game
        GameProperty gp = new GameProperty()
        {
            Key = "type",
            Value = "private"
        };
        createGameSessionRequest.GameProperties.Add(gp);
        Task<CreateGameSessionResponse> createGameSessionRequestTask = myAmazonGameLiftClient.CreateGameSessionAsync(createGameSessionRequest);
        Debug.Log("Created GameSessionRequestTask");
        CreateGameSessionResponse createGameSessionResponse = await createGameSessionRequestTask;
        Debug.Log("Created GameSessionResponse");
        string gameSessionId = createGameSessionResponse.GameSession != null ? createGameSessionResponse.GameSession.GameSessionId : "N/A";
        Debug.Log((int)createGameSessionResponse.HttpStatusCode + " GAME SESSION CREATED: " + gameSessionId);
        return createGameSessionResponse.GameSession;
    }

    async private Task<GameSession> CreatePublicGameSessionAsync()
    {
        Debug.Log("CreateGameSessionAsync");
        var createGameSessionRequest = new Amazon.GameLift.Model.CreateGameSessionRequest();
        createGameSessionRequest.FleetId = Client.GetFleetId(); // can also use AliasId
        createGameSessionRequest.CreatorId = myCachedUserId;
        createGameSessionRequest.MaximumPlayerSessionCount = 2; // search for two player game
        GameProperty gp = new GameProperty()
        {
            Key = "type",
            Value = "public"
        };
        createGameSessionRequest.GameProperties.Add(gp);
        Task<CreateGameSessionResponse> createGameSessionRequestTask = myAmazonGameLiftClient.CreateGameSessionAsync(createGameSessionRequest);
        Debug.Log("Created GameSessionRequestTask");
        CreateGameSessionResponse createGameSessionResponse = await createGameSessionRequestTask;
        Debug.Log("Created GameSessionResponse");
        string gameSessionId = createGameSessionResponse.GameSession != null ? createGameSessionResponse.GameSession.GameSessionId : "N/A";
        Debug.Log((int)createGameSessionResponse.HttpStatusCode + " GAME SESSION CREATED: " + gameSessionId);
        return createGameSessionResponse.GameSession;
    }

    async private Task<GameSession> SearchGameSessionAsync()
    {
        Debug.Log("SearchGameSession");
        var searchGameSessionsRequest = new SearchGameSessionsRequest();
        searchGameSessionsRequest.FleetId = Client.GetFleetId(); // can also use AliasId
        searchGameSessionsRequest.FilterExpression = "gameSessionProperties.type='public' AND hasAvailablePlayerSessions=true";
        searchGameSessionsRequest.SortExpression = "creationTimeMillis ASC"; // return oldest first
        searchGameSessionsRequest.Limit = 1; // only one session even if there are other valid ones

        Task<SearchGameSessionsResponse> SearchGameSessionsResponseTask = myAmazonGameLiftClient.SearchGameSessionsAsync(searchGameSessionsRequest);
        SearchGameSessionsResponse searchGameSessionsResponse = await SearchGameSessionsResponseTask;

        int gameSessionCount = searchGameSessionsResponse.GameSessions.Count;
        Debug.Log($"GameSessionCount:  {gameSessionCount}");

        if (gameSessionCount > 0)
        {
            Debug.Log("We have game sessions!");
            Debug.Log(searchGameSessionsResponse.GameSessions[0].GameSessionId);
            return searchGameSessionsResponse.GameSessions[0];
        }
        return null;
    }

    async private Task<GameSession> SearchGameSessionsAsync()
    {
        Debug.Log("SearchGameSessions");
        var searchGameSessionsRequest = new SearchGameSessionsRequest();
        searchGameSessionsRequest.FleetId = Client.GetFleetId(); // can also use AliasId
        searchGameSessionsRequest.FilterExpression = "hasAvailablePlayerSessions=true"; // only ones we can join
        searchGameSessionsRequest.SortExpression = "creationTimeMillis ASC"; // return oldest first
        searchGameSessionsRequest.Limit = 1; // only one session even if there are other valid ones

        Task<SearchGameSessionsResponse> SearchGameSessionsResponseTask = myAmazonGameLiftClient.SearchGameSessionsAsync(searchGameSessionsRequest);
        SearchGameSessionsResponse searchGameSessionsResponse = await SearchGameSessionsResponseTask;

        int gameSessionCount = searchGameSessionsResponse.GameSessions.Count;
        Debug.Log($"GameSessionCount:  {gameSessionCount}");

        if (gameSessionCount > 0)
        {
            Debug.Log("We have game sessions!");
            Debug.Log(searchGameSessionsResponse.GameSessions[0].GameSessionId);
            return searchGameSessionsResponse.GameSessions[0];
        }
        return null;
    }

    private void CreateGameLiftClient()
    {
        Debug.Log("[HOOD][CLIENT][GAMELIFT] - CreateGameLiftClient()");
        CognitoAWSCredentials credentials = GetCognitoCredentialsFromPlayer();
        if (!CLU.GetIsConnectLocalEnabled())
        {
            myAmazonGameLiftClient = new AmazonGameLiftClient(credentials, Shared.OurRegionId());
        }
        else
        {
            AmazonGameLiftConfig amazonGameLiftConfig = new AmazonGameLiftConfig()
            {
                ServiceURL = "http://localhost:9080"
            };
            myAmazonGameLiftClient = new AmazonGameLiftClient(credentials, amazonGameLiftConfig);
        }
    }

    async private void SetupPublicMatch()
    {
        Debug.Log("Setup public match");
        CreateGameLiftClient();
        var maxRetryAttempts = 10;
        await RetryHelper.RetryOnExceptionAsync<Exception>
        (maxRetryAttempts, async () =>
        {
            myGameSession = await CreatePublicGameSessionAsync();
        });

        if (myGameSession != null)
        {
            Debug.Log("Game session created.");
            CreatePlayerSession(myGameSession);
            //ClientStartup.ourGameStatus = "LOBBY";//TODO: delete up if not needed
            //ClientStartup.ourLobbyStatus = "CREATED PUBLIC MATCH";//TODO: delete up if not needed
        }
        else
        {
            Debug.LogWarning("FAILED to create public match.");
        }
    }

    async private void SetupRemotePrivateMatch()
    {
        int maxRetryAttempts = 10;
        Debug.Log("[HOOD][CLIENT][PRIVATE_LOBBY][HOST] - Step 3/9 - GameLiftClient creating GameSession on Fleet with " + maxRetryAttempts + " attempts.");

        CreateGameLiftClient();
        await RetryHelper.RetryOnExceptionAsync<Exception>
        (maxRetryAttempts, async () =>
        {
            myGameSession = await CreatePrivateGameSessionAsync();
        });

        if (myGameSession != null)
        {
            Debug.Log("[HOOD][CLIENT][PRIVATE_LOBBY][HOST] - Step 4/9 - GameSession created successfully.");
            CreatePlayerSession(myGameSession, "PRIVATE_HOST");
            if (CLU.GetIsLobbyCacheEnabled())
            {
                LobbyCache lobbyCache = new LobbyCache(myGameSession.GameSessionId);
                SaveDataManager.SaveJsonData(lobbyCache);
            }
        }
        else
        {
            Debug.LogError("[HOOD][CLIENT][PRIVATE_LOBBY][HOST] - Step 4/9 - GameSession failed to create.");
            myNetworkClientReference.GameSessionCreationFailed();
        }
    }

    private void SetupLocalPrivateMatch()
    {
        Debug.Log("[HOOD][CLIENT][PRIVATE_LOBBY][HOST][LOCAL] - Step 3/9 - GameLiftClient creating local GameSession");
        CreateLocalPlayerSession("a");
    }

    private async void ConnectToRemoteMatch(string aGameSessionId)
    {
        int maxRetryAttempts = 10;
        Debug.Log("[HOOD][CLIENT][PRIVATE_LOBBY][GUEST] - Step 3/9 - GameLiftClient searching for  existing GameSession");

        CreateGameLiftClient();
        await RetryHelper.RetryOnExceptionAsync<Exception>
        (maxRetryAttempts, async () =>
        {
            myGameSession = await SearchPrivateGameSessionAsync(aGameSessionId);
        });

        if (myGameSession != null)
        {
            Debug.Log("[HOOD][CLIENT][PRIVATE_LOBBY][GUEST] - Step 4/9 - GameSession found!");
            CreatePlayerSession(myGameSession, "PRIVATE_GUEST");
        }
        else
        {
            Debug.LogError("[HOOD][CLIENT][PRIVATE_LOBBY][GUEST] - Step 4/9 - Failed to find GameSession.");
            myNetworkClientReference.GameSessionCreationFailed();
        }
    }

    private void ConnectToLocalMatch()
    {
        Debug.Log("[HOOD][CLIENT][PRIVATE_LOBBY][GUEST][LOCAL] - Step 3/9 - ???");
        CreateLocalPlayerSession("b");
    }

    private async void SearchMatch()
    {
        Debug.Log("Searching for match");
        CreateGameLiftClient();
        var maxRetryAttempts = 10;
        await RetryHelper.RetryOnExceptionAsync<Exception>
        (maxRetryAttempts, async () =>
        {
            myGameSession = await SearchGameSessionAsync();
        });
        if (myGameSession != null)
        {
            CreatePlayerSession(myGameSession);
            //ClientStartup.ourGameStatus = "LOBBY";//TODO: delete up if not needed
            //ClientStartup.ourLobbyStatus = "FOUND PUBLIC MATCH";//TODO: delete up if not needed
        }
        else
            SetupPublicMatch();
    }

    async private Task<GameSession> SearchPrivateGameSessionAsync(string aGameSessionId)
    {
        Debug.Log("Searching for private match with id:" + aGameSessionId);
        var searchGameSessionsRequest = new SearchGameSessionsRequest();
        searchGameSessionsRequest.FleetId = Client.GetFleetId();
        searchGameSessionsRequest.FilterExpression = "gameSessionProperties.type='private' AND gameSessionId='" + aGameSessionId + "'";
        Debug.Log(searchGameSessionsRequest.FilterExpression.ToString());
        Task<SearchGameSessionsResponse> SearchGameSessionsResponseTask = myAmazonGameLiftClient.SearchGameSessionsAsync(searchGameSessionsRequest);
        SearchGameSessionsResponse searchGameSessionsResponse = await SearchGameSessionsResponseTask;

        int gameSessionCount = searchGameSessionsResponse.GameSessions.Count;
        Debug.Log($"GameSessionCount:  {gameSessionCount}");

        if (gameSessionCount > 0)
        {
            Debug.Log("Game found!");
            Debug.Log(searchGameSessionsResponse.GameSessions[0].GameSessionId);
            return searchGameSessionsResponse.GameSessions[0];
        }
        return null;
    }

    // TODO: Why is it necesary to get the cognito credentials cached on SharedUser and not the ones cached on this class?
    private CognitoAWSCredentials GetCognitoCredentialsFromPlayer()
    {
        return mySharedUserReference.GetCognitoCredentials();
    }

    public void DisconnectMeFromLobby(bool aIsLobbyOwner)
    {
        myNetworkClientReference.DisconnectMeFromLobby(aIsLobbyOwner);
    }
}
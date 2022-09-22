using System;
using UnityEngine;
using System.Text;
using Newtonsoft.Json;
using SharedScripts;
using Assets.Shared.Scripts.Messages.Server;
using Assets.Shared.Scripts.Messages.Client;
using SharedScripts.DataId;

/*
Class goal:
    - Send to the server through TelepathyClient
    - Receive from the server and relay to "ClientCode" 
*/
public class NetworkClient : MonoBehaviour
{
    private Telepathy.Client myTelepathyClient;
    private string myPlayerSession;

    private MenuSceneUIManager myMenuSceneReference;
    private bool myIsMenuSceneReferenceSet;

    private ClientGameManager myClientGameManagerReference;

    private SceneController mySceneControllerReference;
    private MatchSceneUIManager myMatchSceneUIManagerReference;

    public void SetMenuSceneReference(MenuSceneUIManager aMenuScene) 
    {
        myMenuSceneReference = aMenuScene;
        myIsMenuSceneReferenceSet = true;
    }

    public void Start()
    {
        mySceneControllerReference = FindObjectOfType<SceneController>();   
    }

    private void Awake()
    {
        Shared.Log("[HOOD][CLIENT][NETWORK] - NetworkClient Awake");
        myTelepathyClient = new Telepathy.Client(Shared.ourMaxMessageSize);
        myIsMenuSceneReferenceSet = false;
        Application.runInBackground = true;
        myTelepathyClient.OnConnected = OnConnected;
        myTelepathyClient.OnData = OnDataReceived;
        myTelepathyClient.OnDisconnected = OnDisconnected;
    }

    private void Update()
    {
        // tick to process messages, (even if not connected so we still process disconnect messages)
        myTelepathyClient.Tick(100);
    }

    private void OnEnable()
    {
        EventHandler.OurAfterMatchSceneLoadEvent += SendMatchSceneLoadedMessage;
    }

    private void OnDisable()
    {
        EventHandler.OurAfterMatchSceneLoadEvent -= SendMatchSceneLoadedMessage;
    }


    private void OnApplicationQuit()
    {
        if(myMenuSceneReference != null)
            DisconnectMeFromLobby(myMenuSceneReference.GetIsLobbyOwner());
        // the client/server threads won't receive the OnQuit info if we are
        // running them in the Editor. they would only quit when we press Play
        // again later. this is fine, but let's shut them down here for consistency
        //myTelepathyClient.Disconnect();
    }

    private void SendMatchSceneLoadedMessage()
    {
        Shared.Log("[HOOD][CLIENT][NETWORK] - SendMatchSceneLoadedMessage()");
        myClientGameManagerReference = FindObjectOfType<ClientGameManager>();
        myMatchSceneUIManagerReference = FindObjectOfType<MatchSceneUIManager>();

        if (myClientGameManagerReference == null)
        {
            Shared.LogError("[HOOD][CLIENT][NETWORK] - ClientGameManager is null");
            return;
        }

        myClientGameManagerReference.AddLocalPlayer(myPlayerSession);
        ClientPlayerReadyStatusMessage sharedClientMessage = new ClientPlayerReadyStatusMessage(ReadyStatusMessageId.MATCH_SCENE_LOADED, myPlayerSession, true);
        SendMessageToServer(sharedClientMessage);
    }

    private void OnDataReceived(ArraySegment<byte> aMessage)
    {
        Shared.Log("[HOOD][CLIENT][NETWORK] - OnDataReceived");
        string convertedMessage = Encoding.UTF8.GetString(aMessage.Array, 0, aMessage.Count);
        Shared.Log("[HOOD][CLIENT][NETWORK] - Converted message: " + convertedMessage);
        SharedServerMessage SharedServerMessage = JsonConvert.DeserializeObject<SharedServerMessage>(convertedMessage);
        ProcessServerMessage(SharedServerMessage);
    }

    private void ProcessServerMessage(SharedServerMessage aSharedServerMessage)
    {
        if (!myIsMenuSceneReferenceSet)
        {
            Shared.LogError("[HOOD][CLIENT][NETWORK] - ProcessServerMessage()");
            return;
        }

        switch (aSharedServerMessage.myType)
        {
            case nameof(ServerMatchSetupMessage):
                ServerMatchSetupMessage msm = aSharedServerMessage as ServerMatchSetupMessage;                
                ProcessMatchSetupMessage(msm);
                break;
            case nameof(ServerPlayerGameplayMessage):
                ServerPlayerGameplayMessage pgm = aSharedServerMessage as ServerPlayerGameplayMessage;
                ProcessPlayerGameplayMessage(pgm);
                break;
            case nameof(ServerStatusGameplayMessage):
                ServerStatusGameplayMessage sgm = aSharedServerMessage as ServerStatusGameplayMessage;
                ProcessStatusGameplayMessage(sgm);
                break;
            case nameof(ServerGameplayActionMessage):
                ServerGameplayActionMessage ugm = aSharedServerMessage as ServerGameplayActionMessage;
                ProcessActionGameplayMessage(ugm);
                break;
            case nameof(ServerLobbyMessage):
                ServerLobbyMessage lm = aSharedServerMessage as ServerLobbyMessage;
                ProcessLobbyMessage(lm);
                break;
            case nameof(ServerReadyStatusMessage):
                ServerReadyStatusMessage srsm = aSharedServerMessage as ServerReadyStatusMessage;
                ProcessReadyStatusMessage(srsm);
                break;
            case nameof(ServerStartProcessMessage):
                ServerStartProcessMessage sspm = aSharedServerMessage as ServerStartProcessMessage;
                ProcessStartProcessMessage(sspm);
                break;
            case nameof(ServerMatchStateMessage):
                ServerMatchStateMessage smsm = aSharedServerMessage as ServerMatchStateMessage;
                ProcessMatchStateMessage(smsm);
                break;
            case nameof(ServerInformationMessage):
                ServerInformationMessage sem = aSharedServerMessage as ServerInformationMessage;
                ProcessErrorMessage(sem);
                break;
            case nameof(ServerDatabaseMessage):
                ServerDatabaseMessage slim = aSharedServerMessage as ServerDatabaseMessage;
                ProcessServerDatabaseMessage(slim);
                break;
            default:
                Shared.LogError("[HOOD][CLIENT][NETWORK] - Unrecognized message type.");
                break;
        }
    }

    private void ProcessLobbyMessage(ServerLobbyMessage aMessage)
    {
        switch (aMessage.myMessageId)
        {
            case LobbyMessageId.CONNECTED:
                Shared.Log("[HOOD][CLIENT][NETWORK] - Connection to server confirmed.");
                HandleConnectedToLobby(aMessage.myPlayerInfo.myName);
                break;
            case LobbyMessageId.PRIVATE_LOBBY_CREATED:
                Shared.Log("[HOOD][CLIENT][NETWORK] - Private match created with code: " + aMessage.myLobbyId);
                myMenuSceneReference.Lobby_PRIVATE_LOBBY_CREATED(aMessage.myLobbyId);
                break;
            case LobbyMessageId.PLAYER_LEFT:
                Shared.Log("[HOOD][CLIENT][NETWORK] - Player: " + aMessage.myPlayerInfo.myName + " left the lobby");
                bool isLocalPlayer = (aMessage.myPlayerInfo.myPlayerSessionId == myPlayerSession);
                myMenuSceneReference.Lobby_DISCONNECTED_FROM_LOBBY(isLocalPlayer);
                break;
            case LobbyMessageId.LOBBY_FULL:
                Shared.Log("[HOOD][CLIENT][NETWORK] - Lobby is full");
                break;
            case LobbyMessageId.HOST_DISCONNECTED:
                Shared.Log("[HOOD][CLIENT][NETWORK] - Host disconnected");
                myMenuSceneReference.Lobby_HOST_DISCONNECTED();
                break;
            case LobbyMessageId.GUEST_DISCONNECTED:
                Shared.Log("[HOOD][CLIENT][NETWORK] - Guest disconnected");
                myMenuSceneReference.Lobby_GUEST_DISCONNECTED();
                break;
            default:
                Shared.LogError("[HOOD][CLIENT][NETWORK] - Unknown LobbyMessageId received.");
                break;
        }
    }

    private void ProcessMatchSetupMessage(ServerMatchSetupMessage aMessage)
    {
        switch (aMessage.myMessageId)
        {
            case MatchSetupMessageId.BOARD_AND_DECK_CONFIG:
                break;
            case MatchSetupMessageId.PLAYER_CONFIG:
                break;
            default:
                Shared.LogError("[HOOD][CLIENT][NETWORK] - Unknown MatchSetupMessageId received.");
                break;
        }
    }

    private void ProcessPlayerGameplayMessage(ServerPlayerGameplayMessage aMessage)
    {
        switch (aMessage.myMessageId)
        {
            case PlayerGameplayMessageId.DRAW_CARD:
                break;
            case PlayerGameplayMessageId.UPDATE_ENERGY:
                break;
            default:
                Shared.LogError("[HOOD][CLIENT][NETWORK] - Unknown PlayerGameplayMessageId received.");
                break;
        }
    }

    private void ProcessStatusGameplayMessage(ServerStatusGameplayMessage aMessage)
    {
        switch (aMessage.myMessageId)
        {
            case StatusGameplayMessageId.START_GAME:
                break;
            case StatusGameplayMessageId.NEW_TURN:
                break;
            case StatusGameplayMessageId.END_GAME:
                break;
            default:
                Shared.LogError("[HOOD][CLIENT][NETWORK] - Unknown StatusGameplayMessageId received.");
                break;
        }
    }

    private async void ProcessActionGameplayMessage(ServerGameplayActionMessage aMessage)
    {
        bool success = aMessage.mySuccess;
        Vector2Int coord = new Vector2Int(aMessage.myTargetPositionX, aMessage.myTargetPositionY);

        switch (aMessage.myMessageId)
        {
            case GameplayMessageIdServer.SPAWN_UNIT:
                if (success)
                {
                    myClientGameManagerReference.DoSpawnUnitFromCard((CardId)aMessage.myGameplayObjectId, coord);
                }
                else
                {
                    myClientGameManagerReference.OnServerFailedToSpawnCard((CardId)aMessage.myGameplayObjectId);
                    myMatchSceneUIManagerReference.PlayErrorSound();
                }
                break;
            case GameplayMessageIdServer.ATTACK_UNIT:
                if (success)
                {
                    await myClientGameManagerReference.DoAttackUnit(aMessage.myGameplayObjectId, coord);
                }
                else
                {
                    myMatchSceneUIManagerReference.PlayErrorSound();
                }
                break;
            case GameplayMessageIdServer.MOVE_UNIT:
                if (success)
                {
                    myClientGameManagerReference.DoMoveUnit(aMessage.myGameplayObjectId, coord);
                }
                else
                {
                    myMatchSceneUIManagerReference.PlayErrorSound();
                }
                break;
            case GameplayMessageIdServer.USE_ABILITY:
                if (success)
                {
                    await myClientGameManagerReference.DoCastUnitAbility(aMessage.myGameplayObjectId, coord);
                }
                else
                {
                    myMatchSceneUIManagerReference.PlayErrorSound();
                }
                break;
            case GameplayMessageIdServer.USE_TECHNOLOGY:
                if (success)
                {
                 //   myClientGameManagerReference.DoUseTechnologyCard((CardId)aMessage.myGameplayObjectId, coord);
                }
                else
                {
                    //  myClientGameManagerReference.OnServerFailedToSpawnCard((CardId)aMessage.myGameplayObjectId);
                    //myMatchSceneUIManagerReference.PlayErrorSound();
                }
                break;
            default:
                Shared.LogError("[HOOD][CLIENT][NETWORK] - Unknown ActionGameplayMessageId received.");
                break;
        }
    }

    private void ProcessReadyStatusMessage(ServerReadyStatusMessage aMessage)
    {
        switch (aMessage.myMessageId)
        {
            case ReadyStatusMessageId.PLAYER_READY:
                Debug.Log("[HOOD][CLIENT][NETWORK] - Player " + aMessage.myPlayerId + " | Ready = " + aMessage.myIsReady.ToString());
                myMenuSceneReference.GetLobbyCanvasReference().ChangeOpponentPlayerReadyStatus(aMessage.myIsReady);
                break;
            default:
                Shared.LogError("[HOOD][CLIENT][NETWORK] - Unknown ServerReadyStatusMessage received.");
                break;
        }
    }

    private void ProcessStartProcessMessage(ServerStartProcessMessage aMessage)
    {
        switch (aMessage.myMessageId)
        {
            case StartProcessMessageId.START_COUNTDOWN:
                myMenuSceneReference.StartLobbyTimer(aMessage.myCountdownStartingNumber);
                break;
            case StartProcessMessageId.START_LOCK_IN:
                myMenuSceneReference.StartTimerLockedIn();
                break;
            case StartProcessMessageId.STOP_COUNTDOWN:
                myMenuSceneReference.ResetLobbyTimer();
                break;
            case StartProcessMessageId.COUNTDOWN_OVER:
                mySceneControllerReference.LoadScene(SceneId.MATCH);
                break;
            case StartProcessMessageId.START_MATCH:
                break;
            default:
                Shared.LogError("[HOOD][CLIENT][NETWORK] - Unknown ServerStartProcessMessage received.");
                break;
        }
    }

    private void ProcessMatchStateMessage(ServerMatchStateMessage aMessage)
    {
        switch (aMessage.myMessageId)
        {
            case MatchStateMessageId.SETUP:
                myClientGameManagerReference.InitializeNewState(MatchState.SETUP,"", aMessage.myBoardInfo, aMessage.myPlayerInfo);
                break;
            case MatchStateMessageId.PLAYER_TURN:
                myClientGameManagerReference.InitializeNewState(MatchState.PLAYER_TURN, aMessage.myPlayerId);
                break;
            case MatchStateMessageId.END_WINNER:
                myClientGameManagerReference.InitializeNewState(MatchState.END, aMessage.myPlayerId);
                break;
            case MatchStateMessageId.END_DRAW:
                myClientGameManagerReference.InitializeNewState(MatchState.END); // In case we change the enum name at some point
                break;
            default:
                Shared.LogError("[HOOD][CLIENT][NETWORK] - Unknown ProcessMatchStateMessage received.");
                break;
        }
    }

    // These are messages sent by the server on response to database interactions in the clients behest
    private void ProcessServerDatabaseMessage(ServerDatabaseMessage aMessage)
    {
        switch (aMessage.myMessageId)
        {
            case DatabaseMessageId.SEND_HOST_SHORT_LOBBY_ID:
                HandleShortPrivateLobbyIdReceived(aMessage.myHoodId == 1, aMessage.myValue);
                break;
            default:
                Shared.LogError("[HOOD][CLIENT][NETWORK] - Unknown ProcessServerDatabaseMessage received.");
                break;
        }
    }

    private void ProcessErrorMessage(ServerInformationMessage aMessage)
    {
        switch (aMessage.myMessageId)
        {
            case InformationMessageId.ERROR:
                Shared.Log("[HOOD][CLIENT][NETWORK] Server error - " + aMessage.myMessage);
                break;
            case InformationMessageId.INFO:
                Shared.Log("[HOOD][CLIENT][NETWORK] Server info - " + aMessage.myMessage);
                break;
            case InformationMessageId.SERVER_CLOSED:
                Shared.Log("[HOOD][CLIENT][NETWORK] Server info - server closed");
                mySceneControllerReference.LoadScene(SceneId.MENU);
                break;
            default:
                Shared.LogError("[HOOD][CLIENT][NETWORK] - Unknown ServerErrorMessage received.");
                break;
        }
    }

    // hoodies's method
    private void HandleConnectedToLobby(string aUsername) 
    {
        if (!myMenuSceneReference.GetIsLobbyOwner()) //if im guest
        {
            myMenuSceneReference.Lobby_CONNECTED();
            myMenuSceneReference.GetLobbyCanvasReference().LoadOpponentPanel(aUsername); // TODO: network client should't tell a scene manager what ui to display            
        }
        else if (myMenuSceneReference.GetLobbyStatus() != LobbyStatus.WAITING_FOR_OPPONENT) //if im host and creating
        {
            myMenuSceneReference.Lobby_CONNECTED();
        }
        else //if im host and waiting
        {
            myMenuSceneReference.GetLobbyCanvasReference().LoadOpponentPanel(aUsername); // TODO: network client should't tell a scene manager what ui to display            
        }
    }

    // telepathy's method
    private void OnConnected()
    {
        SharedUser user = GetComponent<SharedUser>();  
        Shared.Log("[HOOD][CLIENT][NETWORK] - OnConnected");
        SharedClientMessage sharedClientMessage = new ClientLobbyMessage(LobbyMessageIdClient.CONNECT, myPlayerSession, user.GetUsername(), myMenuSceneReference.GetIsLobbyOwner());
        SendMessageToServer(sharedClientMessage);
        Shared.Log("[HOOD][CLIENT][NETWORK] - after send message to server");
    }

    // telepathy's method
    private void OnDisconnected()
    {
        Shared.Log("[HOOD][CLIENT][NETWORK] - Client Disconnected");
    }

    public void SendMessageToServer(SharedClientMessage aMessage)
    {
        Shared.Log("[HOOD][CLIENT][NETWORK] - SendMessageToServer");
        var data = JsonConvert.SerializeObject(aMessage);
        var encoded = Encoding.UTF8.GetBytes(data);
        var buffer = new ArraySegment<Byte>(encoded, 0, encoded.Length);
        myTelepathyClient.Send(buffer);
    }

    private void HandleShortPrivateLobbyIdReceived(bool aSuccess, string aShortLobbyId)
    {
        Shared.Log("[HOOD][CLIENT][NETWORK] HandleShortPrivateLobbyIdReceived " + aSuccess + aShortLobbyId);
        myMenuSceneReference.SetShortLobbyId(aShortLobbyId);
    }

    public void DisconnectMeFromLobby(bool aIsLobbyOwner)
    {
        SharedUser user = GetComponent<SharedUser>();
        Shared.Log("[HOOD][CLIENT][NETWORK] - Disconnecting me from lobby.");
        SharedClientMessage sharedClientMessage = new ClientLobbyMessage(LobbyMessageIdClient.DISCONNECT_ME, myPlayerSession, user.GetUsername(), aIsLobbyOwner);
        SendMessageToServer(sharedClientMessage);
        Shared.Log("[HOOD][CLIENT][NETWORK] - After send disconnect from lobby message.");
    }

    public void SendReadyStatus(bool aIsReady, int aDeckId)
    {
        SharedUser user = GetComponent<SharedUser>();
        string userName = user.GetUsername();
        ClientPlayerReadyStatusMessage sharedClientMessage = new ClientPlayerReadyStatusMessage(ReadyStatusMessageId.PLAYER_READY, myPlayerSession, aIsReady, userName, aDeckId);
        SendMessageToServer(sharedClientMessage);
    }

    public void GameSessionCreationFailed() 
    {
        myMenuSceneReference.OnGameSessionCreationFailed();
    }

    public void PlayerSessionCreationFailed() 
    {
        myMenuSceneReference.OnPlayerSessionCreationFailed();
        //TODO: Should we do something to terminate de still active GameSession?
    }

    //TODO: Delete?
    private void GameEnded(string aGameOverMessage)
    {
        //ClientStartup.ourGameStatus = aGameOverMessage; //TODO: delete up if not needed
    }

    public void ConnectToServer(string anIp, int aPort, string aPlayerSessionId, string aDebugFlow = "NONE")
    {
        Shared.Log("[HOOD][CLIENT][NETWORK] - ConnectToServer IP: " + anIp + " Port: " + aPort + " PlayerSessionId: " + aPlayerSessionId);
        if (aDebugFlow == "PRIVATE_HOST") 
        {
            Shared.Log("[HOOD][CLIENT][PRIVATE_LOBBY][HOST] - Step 7/9 - Connecting to server through Telepathy.");
        }
        else if (aDebugFlow == "PRIVATE_GUEST")
        {
            Shared.Log("[HOOD][CLIENT][PRIVATE_LOBBY][GUEST] - Step 7/9 - Connecting to server through Telepathy.");
        }

        myPlayerSession = aPlayerSessionId;

        // had to set these to 0 or else the TCP connection would timeout after the default 5 seconds.
        myTelepathyClient.SendTimeout = 0;
        myTelepathyClient.ReceiveTimeout = 0;

        myTelepathyClient.Connect(anIp, aPort);
    }

    public string GetPlayerSession()
    {
        return myPlayerSession;
    }

    private bool IsLocalPlayer(string aPlayerSessionId)
    {
        return aPlayerSessionId == myPlayerSession;
    }

    public void LeaveEndedMatch()
    {
        mySceneControllerReference.LoadScene(SceneId.MENU);
    }
}

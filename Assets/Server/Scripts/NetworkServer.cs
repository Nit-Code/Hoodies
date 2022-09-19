using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.Text;
using SharedScripts;
using Assets.Shared.Scripts.Messages.Client;
using Assets.Shared.Scripts.Messages.Server;
using SharedScripts.DataId;

/*
Class goal:
    - Send to the client(s) through TelepathyClient
    - Receive from the client(s) and relay to "ServerCode"
*/
public class NetworkServer : MonoBehaviour
{
    private Telepathy.Server myServer;
    private Dictionary<int, string> myPlayerSessionsMap;
    private Dictionary<string, int> myConnectionIdMap;
    private GameLiftServer myGameLiftServerReference;
    private GameState myGameState;
    public void SetGameState(GameState aGameState) { myGameState = aGameState; }
    public GameState GetGameState() { return myGameState; }
    private ServerGameManager myGameManagerReference;
    private ServerLambda myServerLambdaReference;
    public int myHostConnectionId;
    private int myToLoad;
    private int myLoaded;
    private bool myIsAConnectionAccomplished;

    private bool IsReadyToStart()
    {
        return myToLoad == myLoaded;
    }

    private void Awake()
    {
        myToLoad = 3;

        if (TryGetComponent<ServerGameManager>(out myGameManagerReference))
            myLoaded++;
        else
            Shared.LogError("[HOOD][SERVER][NETWORK] - ServerGameManager Component not found");

        if (TryGetComponent<GameLiftServer>(out myGameLiftServerReference))
            myLoaded++;
        else
            Shared.LogError("[HOOD][SERVER][NETWORK] - GameLiftServer Component not found");

        if (TryGetComponent<ServerLambda>(out myServerLambdaReference))
            myLoaded++;
        else
            Shared.LogError("[HOOD][SERVER][NETWORK] - ServerLambda Component not found");
    }

    private void Start()
    {
        if (!IsReadyToStart())
        {
            Shared.LogError("[HOOD][SERVER][NETWORK] - Not starting");
            return;
        }

        OnStart();
    }

    private void OnStart()
    {
        Shared.Log("[HOOD][SERVER][NETWORK] - OnStart");

        myIsAConnectionAccomplished = false;
        myServer = new Telepathy.Server(Shared.ourMaxMessageSize);
        myGameState = GameState.IN_MAIN_MENU;
        myPlayerSessionsMap = new Dictionary<int, string>();
        myConnectionIdMap = new Dictionary<string, int>();
        Application.runInBackground = true;
        myServer.OnConnected = OnConnected;
        myServer.OnData = OnDataReceived;
        myServer.OnDisconnected = OnDisconnected;
    }

    private void Update()
    {
        if (IsReadyToStart())
        {
            // tick to process messages, (even if not active so we still process disconnect messages)
            myServer.Tick(100);
        }
    }

    void OnApplicationQuit()
    {
        Shared.Log("[HOOD][SERVER][NETWORK] - OnApplicationQuit");
        myServer.Stop();
    }

    private void OnDataReceived(int aConnectionId, ArraySegment<byte> aMessage)
    {
        try
        {
            Shared.Log("Data received from connectionId: " + aConnectionId);
            string convertedMessage = Encoding.UTF8.GetString(aMessage.Array, 0, aMessage.Count);
            Shared.Log("Converted message: " + convertedMessage);
            SharedClientMessage networkMessage = JsonConvert.DeserializeObject<SharedClientMessage>(convertedMessage);
            ProcessClientMessage(aConnectionId, networkMessage);
        }
        catch (Exception ex)
        {
            SharedServerMessage sem = new ServerInformationMessage(InformationMessageId.ERROR, ex.Message);
            SendMessageToPlayer(aConnectionId, sem);
            ShutDownGameSession();
        }
    }

    private void ProcessClientMessage(int aConnectionId, SharedClientMessage aSharedClientMessage)
    {
        try
        {
            switch (aSharedClientMessage.myType)
            {
                case nameof(ClientPlayerGameplayMessage):
                    ClientPlayerGameplayMessage pgm = aSharedClientMessage as ClientPlayerGameplayMessage;
                    ProcessPlayerGameplayMessage(aConnectionId, pgm);
                    break;
                case nameof(ClientGameplayMessage):
                    ClientGameplayMessage ugm = aSharedClientMessage as ClientGameplayMessage;
                    ProcessGameplayMessage(aConnectionId, ugm);
                    break;
                case nameof(ClientLobbyMessage):
                    ClientLobbyMessage lm = aSharedClientMessage as ClientLobbyMessage;
                    ProcessLobbyMessage(aConnectionId, lm);
                    break;
                case nameof(ClientMatchConnectionMessage):
                    ClientMatchConnectionMessage mcm = aSharedClientMessage as ClientMatchConnectionMessage;
                    ProcessMatchConnectionMessage(aConnectionId, mcm);
                    break;
                case nameof(ClientPlayerReadyStatusMessage):
                    ClientPlayerReadyStatusMessage cprsm = aSharedClientMessage as ClientPlayerReadyStatusMessage;
                    ProcessPlayerReadyStatusMessage(aConnectionId, cprsm);
                    break;
                case nameof(ClientTestFeaturesMessage):
                    ClientTestFeaturesMessage ctfm = aSharedClientMessage as ClientTestFeaturesMessage;
                    ProcessClientTestFeaturesMessage(aConnectionId, ctfm);
                    break;
                default:
                    Shared.LogError("[HOOD][SERVER][NETWORK] - Unrecognized message type.");
                    break;
            }
        }
        catch (Exception ex)
        {
            SharedServerMessage sem = new ServerInformationMessage(InformationMessageId.ERROR, ex.Message);
            Shared.LogError("[HOOD][SERVER][NETWORK] - ProcessClientMessage exeption. Message:" + ex.Message);
            SendMessageToPlayer(aConnectionId, sem);
            ShutDownGameSession();
        }
    }

    private void ProcessPlayerGameplayMessage(int aConnectionId, ClientPlayerGameplayMessage aMessage)
    {
        if (aMessage == null)
        {
            Shared.LogError("[HOOD][SERVER][NETWORK] - ProcessPlayerGameplayMessage, message is null");
            SharedServerMessage sem = new ServerInformationMessage(InformationMessageId.ERROR, "Message is null");
            SendMessageToPlayer(aConnectionId, sem);
            return;
        }

        switch (aMessage.myMessageId)
        {
            case PlayerGameplayMessageIdClient.END_MY_TURN:
                myGameManagerReference.EndTurn();
                break;
            case PlayerGameplayMessageIdClient.SURRENDER:
                break;
            default:
                Shared.LogError("[HOOD][SERVER][NETWORK] - Unknown PlayerGameplayMessage received.");
                break;
        }
    }

    private void ProcessGameplayMessage(int aConnectionId, ClientGameplayMessage aMessage)
    {
        if (aMessage == null)
        {
            Shared.LogError("[HOOD][SERVER][NETWORK] - ProcessGameplayMessage, message is null");
            SharedServerMessage sem = new ServerInformationMessage(InformationMessageId.ERROR, "Message is null");
            SendMessageToPlayer(aConnectionId, sem);
            return;
        }

        SharedServerMessage responseMessage = null;
        string requestingPlayerConnectionId = myPlayerSessionsMap[aConnectionId];
        bool successfulAction = false;
        Vector2Int coord = new Vector2Int(aMessage.myTargetPositionX, aMessage.myTargetPositionY);

        switch (aMessage.myMessageId)
        {
            case GameplayMessageIdClient.REQUEST_SPAWN_UNIT:
                successfulAction = myGameManagerReference.TrySpawnUnitFromCard(aMessage.myPlayerId, (CardId)aMessage.myGameplayObjectId, coord);
                responseMessage = new ServerGameplayActionMessage(GameplayMessageIdServer.SPAWN_UNIT, requestingPlayerConnectionId, aMessage.myGameplayObjectId, aMessage.myTargetPositionX , aMessage.myTargetPositionY, successfulAction);
                break;
            case GameplayMessageIdClient.REQUEST_MOVE_UNIT:
                successfulAction =myGameManagerReference.TryMoveUnit(aMessage.myPlayerId, aMessage.myGameplayObjectId, coord);
                responseMessage = new ServerGameplayActionMessage(GameplayMessageIdServer.MOVE_UNIT, requestingPlayerConnectionId, aMessage.myGameplayObjectId, aMessage.myTargetPositionX, aMessage.myTargetPositionY, successfulAction);
                break;
            case GameplayMessageIdClient.REQUEST_ATTACK_UNIT:
                successfulAction = myGameManagerReference.TryAttackUnit(aMessage.myPlayerId, aMessage.myGameplayObjectId, coord);
                responseMessage = new ServerGameplayActionMessage(GameplayMessageIdServer.ATTACK_UNIT, requestingPlayerConnectionId, aMessage.myGameplayObjectId, aMessage.myTargetPositionX, aMessage.myTargetPositionY, successfulAction);
                break;
            case GameplayMessageIdClient.REQUEST_USE_ABILITY:
                successfulAction = myGameManagerReference.TryCastUnitAbility(aMessage.myPlayerId, aMessage.myGameplayObjectId, coord);
                responseMessage = new ServerGameplayActionMessage(GameplayMessageIdServer.USE_ABILITY, requestingPlayerConnectionId, aMessage.myGameplayObjectId, aMessage.myTargetPositionX, aMessage.myTargetPositionY, successfulAction);
                break;
            case GameplayMessageIdClient.REQUEST_USE_TECHNOLOGY:
                successfulAction =myGameManagerReference.TryUseTechnologyCard(aMessage.myPlayerId, (CardId)aMessage.myGameplayObjectId, coord);
                responseMessage = new ServerGameplayActionMessage(GameplayMessageIdServer.USE_TECHNOLOGY, requestingPlayerConnectionId, aMessage.myGameplayObjectId, aMessage.myTargetPositionX, aMessage.myTargetPositionY, successfulAction);
                break;
            default:
                Shared.LogError("[HOOD][SERVER][NETWORK] - Unknown ClientGameplayMessage received.");
                break;
        }

        if(responseMessage != null)
        {
            if (successfulAction)
            {
                SendMessageToAllPlayers(responseMessage);
            }
            else
            {
                SendMessageToPlayer(aConnectionId, responseMessage);
            }
        }
    }

    private void ProcessLobbyMessage(int aConnectionId, ClientLobbyMessage aMessage)
    {
        if (aMessage == null)
        {
            Shared.LogError("[HOOD][SERVER][NETWORK] - ProcessLobbyMessage, message is null");
            SharedServerMessage sem = new ServerInformationMessage(InformationMessageId.ERROR, "Message is null");
            SendMessageToPlayer(aConnectionId, sem);
            return;
        }

        switch (aMessage.myMessageId)
        {
            case LobbyMessageIdClient.CONNECT:
                HandleConnect(aConnectionId, aMessage.myPlayerSessionId, aMessage.myLobbyOwner);
                break;
            case LobbyMessageIdClient.READY:
                break;
            case LobbyMessageIdClient.DISCONNECT:
                HandleDisconnect(aMessage.myPlayerSessionId, aMessage.myLobbyOwner);
                break;
            default:
                Shared.LogError("[HOOD][SERVER][NETWORK] - Unknown LobbyMessage received.");
                break;
        }
    }

    private void ProcessMatchConnectionMessage(int aConnectionId, ClientMatchConnectionMessage aMessage) //TODO
    {
        if (aMessage == null)
        {
            Shared.LogError("[HOOD][SERVER][NETWORK] - ProcessLobbyMessage, message is null");
            SharedServerMessage sem = new ServerInformationMessage(InformationMessageId.ERROR, "Message is null");
            SendMessageToPlayer(aConnectionId, sem);
            return;
        }

        SharedServerMessage responseMessage = null;
        switch (aMessage.myMessageId)
        {
            case MatchMessageIdClient.LEAVE_STARTING_MATCH:
                HandleDisconnect(aMessage.myPlayerSessionId, false);
                break;
            case MatchMessageIdClient.LEAVE_ONGOING_MATCH:
                HandleDisconnect(aMessage.myPlayerSessionId, false);
                break;
            case MatchMessageIdClient.LEAVE_ENDED_MATCH:
                HandleDisconnect(aMessage.myPlayerSessionId, false);
                break;
        }
    }

    private void ProcessPlayerReadyStatusMessage(int aConnectionId, ClientPlayerReadyStatusMessage aMessage)
    {
        SharedServerMessage responseMessage = null;
        if (aMessage == null)
        {
            responseMessage = new ServerInformationMessage(InformationMessageId.ERROR, "Message is null");
            SendMessageToPlayer(aConnectionId, responseMessage);
            return;
        }

        switch (aMessage.myMessageId)
        {
            case ReadyStatusMessageId.PLAYER_READY:
                if (aMessage.myIsReady)
                {
                    myGameManagerReference.AddReadyPlayer(aMessage.myPlayerId, aMessage.myDeckId);
                }
                else
                {
                    myGameManagerReference.RemoveReadyPlayer(aMessage.myPlayerId);
                }
                responseMessage = new ServerReadyStatusMessage(ReadyStatusMessageId.PLAYER_READY, aMessage.myPlayerId, aMessage.myIsReady, aMessage.myDeckId);
                SendMessageToOtherPlayer(aConnectionId, responseMessage);
                break;
            case ReadyStatusMessageId.MATCH_SCENE_LOADED:
                myGameManagerReference.ChangeState(MatchState.SETUP, aMessage.myPlayerId);
                myGameState = GameState.IN_MATCH;
                break;
            case ReadyStatusMessageId.PLAYER_READY_AND_LOADED:
                myGameManagerReference.AddReadyAndLoadedPlayer(aMessage.myPlayerId);
                break;
            default:
                Shared.LogError("[HOOD][SERVER][NETWORK] - Unknown ClientPlayerReadyStatusMessage received.");
                break;
        }
    }

    private void ProcessClientTestFeaturesMessage(int aConnectionId, ClientTestFeaturesMessage aMessage)
    {
        switch (aMessage.myMessageId)
        {
            case TestFeaturesMessageId.KILL_MY_MOTHERSHIP:
                myGameManagerReference.TestKillMyMothership();
                break;
            default:
                Shared.LogError("[HOOD][SERVER][NETWORK] - Unknown ClientTestFeaturesMessage received.");
                break;
        }
    }

    public void SendMessageToPlayerBySession(string aPlayerSessionId, SharedServerMessage aMessage)
    {
        if (myConnectionIdMap.TryGetValue(aPlayerSessionId, out int connectionId))
        {
            SendMessageToPlayer(connectionId, aMessage);
        }
        else
        {
            Shared.LogError("[HOOD][SERVER][NETWORK] - PlayerSessionId not found in SendMessageToPlayerBySession");
        }
    }

    public void SendMessageToOtherPlayerBySession(string aPlayerSessionId, SharedServerMessage aMessage)
    {
        if (myConnectionIdMap.TryGetValue(aPlayerSessionId, out int connectionId))
        {
            SendMessageToOtherPlayer(connectionId, aMessage);
        }
        else
        {
            Shared.LogError("[HOOD][SERVER][NETWORK] - PlayerSessionId not found in ConnectionIdMap. (SendMessageToOtherPlayerBySession)");
        }
    }

    public void SendMessageToPlayer(int aConnectionId, SharedServerMessage aMessage)
    {
        try
        {
            var data = JsonConvert.SerializeObject(aMessage);
            var encoded = Encoding.UTF8.GetBytes(data);
            var asWriteBuffer = new ArraySegment<Byte>(encoded, 0, encoded.Length);
            myServer.Send(aConnectionId, asWriteBuffer);
        }
        catch (Exception ex)
        {
            SharedServerMessage sem = new ServerInformationMessage(InformationMessageId.ERROR, ex.Message);
            SendMessageToPlayer(aConnectionId, sem);
            ShutDownGameSession();
        }
    }

    public void SendMessageToAllPlayers(SharedServerMessage aMessage)
    {
        foreach (KeyValuePair<int, string> playerSession in myPlayerSessionsMap)
        {
            SendMessageToPlayer(playerSession.Key, aMessage);
        }
    }

    private void SendMessageToOtherPlayer(int aConnectionIdToIgnore, SharedServerMessage aMessage)
    {
        foreach (KeyValuePair<int, string> playerSession in myPlayerSessionsMap)
        {
            if (playerSession.Key != aConnectionIdToIgnore)
                SendMessageToPlayer(playerSession.Key, aMessage);
        }
    }

    private void CheckAndSendGameReadyToStartMsg(int aConnectionId)
    {
        if (myPlayerSessionsMap.Count == Shared.ourMaxPlayersPerSession)
        {
            Shared.Log("Lobby is full and is ready to start.");

            // tell all players the game is ready to start
            foreach (KeyValuePair<int, string> playerSession in myPlayerSessionsMap)
            {
                LobbyPlayer playerInfo = new LobbyPlayer();
                playerInfo.myPlayerSessionId = playerSession.Value;
                SharedServerMessage responseMessage = new ServerLobbyMessage(LobbyMessageId.LOBBY_FULL, playerInfo, "");
                SendMessageToPlayer(playerSession.Key, responseMessage);
            }
        }
    }

    public void HandleConnect(int aConnectionId, string aPlayerSessionId, bool aIsOwner)
    {
        Shared.Log("[HOOD][SERVER][NETWORK] - HandleConnect");
        bool result = false;

        if (!CLU.GetIsConnectLocalEnabled())
        {
            var outcome = myGameLiftServerReference.IncomingPlayerSession(aPlayerSessionId);
            result = outcome.Success;
        }
        else
        {
            if (!myPlayerSessionsMap.ContainsKey(aConnectionId) && !myConnectionIdMap.ContainsKey(aPlayerSessionId)) 
            {
                result = true;
            }
        }

        if (!result)
        {
            Shared.LogError("[HOOD][SERVER][NETWORK] - HandleConnect, player session rejected.");
            ShutDownGameSession();
            return;
        }


        bool isFirstPlayerToJoin = myPlayerSessionsMap.Count == 0;

        if (aIsOwner != isFirstPlayerToJoin) 
        {
            Shared.LogError("[HOOD][SERVER][NETWORK] - HandleConnect, contradicting information.");
            ShutDownGameSession();
            return;
        }

        // Track our player sessions
        myPlayerSessionsMap.Add(aConnectionId, aPlayerSessionId);
        myConnectionIdMap.Add(aPlayerSessionId, aConnectionId);
            
        SharedServerMessage responseMessage = null;
        string hostPlayerSessionId = "";
        if (isFirstPlayerToJoin)
        {
            // Iniciate short private lobby id creation
            string gameSessionId = myGameLiftServerReference.GetGameSessionId();
            // TODO: incoming connections should have the hoodId value, hardcoding for testing 
            // NOTE: We dont need exactly player identifying info, the database index could just mean
            // an empty row in the database, the lambda function could take care of finding out the first
            // non used row in the database or create a new entry on it.
            int hoodId = 1; 

            myServerLambdaReference.InvokeCreateShortLobbyId(hoodId, gameSessionId, aConnectionId);

            LobbyPlayer playerInfo = new LobbyPlayer();
            playerInfo.myPlayerSessionId = aPlayerSessionId;
            responseMessage = new ServerLobbyMessage(LobbyMessageId.CONNECTED, playerInfo, "");
            SendMessageToPlayer(aConnectionId, responseMessage);
            myHostConnectionId = aConnectionId;
        }
        else
        {
            //Send message to my client with host info (player already in match)
            hostPlayerSessionId = myPlayerSessionsMap[myHostConnectionId];
            LobbyPlayer opponentInfo = new LobbyPlayer();
            opponentInfo.myPlayerSessionId = hostPlayerSessionId; //playerInfo.myName = obtener desde AWS
            responseMessage = new ServerLobbyMessage(LobbyMessageId.CONNECTED, opponentInfo, "");
            SendMessageToPlayer(aConnectionId, responseMessage);

            //Send message to player already in match
            LobbyPlayer playerInfo = new LobbyPlayer();
            playerInfo.myPlayerSessionId = aPlayerSessionId; //playerInfo.myName = obtener desde AWS
            responseMessage = new ServerLobbyMessage(LobbyMessageId.CONNECTED, playerInfo, "");
            SendMessageToOtherPlayer(aConnectionId, responseMessage);
        }
    }

    private void EndMatch()
    {
        Shared.Log("Closing match");
        foreach (KeyValuePair<int, string> playerSession in myPlayerSessionsMap)
        {
            myServer.Disconnect(playerSession.Key);
            myPlayerSessionsMap.Remove(playerSession.Key);
            myConnectionIdMap.Remove(playerSession.Value);
            if (!CLU.GetIsConnectLocalEnabled())
                myGameLiftServerReference.RemovePlayerSession(playerSession.Value);
        }
        myGameLiftServerReference.HandleGameEnd();                
    }


    // For the sake of simplicity for this demo, if any player disconnects, just end the game. 
    // That means if only one player joins, then disconnects, the game session ends.
    // Your game may remain open to receiving new players, without ending the game session, up to you.
    private void EndGameAfterDisconnect(int disconnectingId)
    {
        Shared.Log("CheckForGameEnd");

        // TODO: also probably check state of game here or something?

        if (myGameState != GameState.IN_POST_MATCH)
        {
            myGameState = GameState.IN_POST_MATCH;

            // For this demo game, just disconnect everyone else in the session when one player disconnects. 
            // An all or nothing type of game. And at this point, since the game session will be ending, we don't 
            // need to worry about removing playerSessions from the _playerSessions Dictonary.
            foreach (KeyValuePair<int, string> playerSession in myPlayerSessionsMap)
            {
                // disconnect all other clients
                if (playerSession.Key != disconnectingId)
                {
                    myServer.Disconnect(playerSession.Key);
                }

                if (!CLU.GetIsConnectLocalEnabled())
                    myGameLiftServerReference.RemovePlayerSession(playerSession.Value); // player session id
            }

            Shared.Log("Ending game, player disconnected.");
            myGameLiftServerReference.HandleGameEnd();
        }
        else
        {
            Shared.Log("EndGameAfterDisconnect: Disconnecting game over is already being processed.");
        }
    }

    private void OnDisconnected(int connectionId)
    {
        Shared.Log("[HOOD][SERVER][NETWORK] - OnDisonnected connectionId: " + connectionId);
        if (!myIsAConnectionAccomplished)
            Shared.Log("[HOOD][SERVER][NETWORK] - a connection was never acomplished, check if your port is available.");

        if (myGameState == GameState.IN_MATCH)
        {
            if (myPlayerSessionsMap[connectionId] == myGameManagerReference.GetPlayer1().GetSessionId())
                myGameManagerReference.SetMatchWinner(MatchWinner.PLAYER_2);
            else if (myPlayerSessionsMap[connectionId] == myGameManagerReference.GetPlayer2().GetSessionId())
                myGameManagerReference.SetMatchWinner(MatchWinner.PLAYER_1);
            myGameManagerReference.ChangeState(MatchState.END);
            EndMatch();
        }
        //EndGameAfterDisconnect(connectionId);        
    }

    private void OnConnected(int connectionId)
    {
        myIsAConnectionAccomplished = true;
        Shared.Log("Connection ID: " + connectionId + " Connected");
        myGameState = GameState.IN_LOBBY;
    }

    private void HandleDisconnect(string aPlayerSessionId, bool aIsOwner)
    {
        Shared.Log("[HOOD][SERVER][NETWORK] - HandleDisconnect");

        LobbyPlayer disconnectedPlayerInfo = new LobbyPlayer();
        disconnectedPlayerInfo.myPlayerSessionId = aPlayerSessionId;
        if (myGameState == GameState.IN_LOBBY)
        {
            SharedServerMessage responseMessage = new ServerLobbyMessage(LobbyMessageId.PLAYER_LEFT, disconnectedPlayerInfo, "");
            SendMessageToAllPlayers(responseMessage);
        }
        else if (myGameState == GameState.IN_MATCH)
        {
            if (aPlayerSessionId == myGameManagerReference.GetPlayer1().GetSessionId())
                myGameManagerReference.SetMatchWinner(MatchWinner.PLAYER_2);
            else if (aPlayerSessionId == myGameManagerReference.GetPlayer2().GetSessionId())
                myGameManagerReference.SetMatchWinner(MatchWinner.PLAYER_1);
            myGameManagerReference.ChangeState(MatchState.END);
        }
        if (!CLU.GetIsConnectLocalEnabled()) 
        {
            foreach (KeyValuePair<int, string> playerSession in myPlayerSessionsMap)
            {
                if (aIsOwner || playerSession.Value == aPlayerSessionId)
                    myGameLiftServerReference.RemovePlayerSession(playerSession.Value); // player session id
            }
        }

        if (aIsOwner)
            myGameLiftServerReference.HandleGameEnd();
    }

    public void ShutDownGameSession()
    {
        if (!CLU.GetIsConnectLocalEnabled())
        {
            foreach (KeyValuePair<int, string> playerSession in myPlayerSessionsMap)
            {
                myGameLiftServerReference.RemovePlayerSession(playerSession.Value);
            }
        }
        myGameLiftServerReference.HandleGameEnd();
    }

    public void StartTCPServer(int port)
    {
        // had to set these to 0 or else the TCP connection would timeout after the default 5 seconds.  Investivate further.
        myServer.SendTimeout = 0;
        myServer.ReceiveTimeout = 0;

        myServer.Start(port);
    }
}

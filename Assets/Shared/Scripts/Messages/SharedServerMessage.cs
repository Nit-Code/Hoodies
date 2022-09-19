using JsonSubTypes;
using Newtonsoft.Json;
using SharedScripts;
using System.Collections.Generic;

namespace Assets.Shared.Scripts.Messages.Server
{
    [System.Serializable]
    [JsonConverter(typeof(JsonSubtypes), "myType")]
    [JsonSubtypes.KnownSubType(typeof(ServerMatchSetupMessage), nameof(ServerMatchSetupMessage))]
    [JsonSubtypes.KnownSubType(typeof(ServerPlayerGameplayMessage), nameof(ServerPlayerGameplayMessage))]
    [JsonSubtypes.KnownSubType(typeof(ServerStatusGameplayMessage), nameof(ServerStatusGameplayMessage))]
    [JsonSubtypes.KnownSubType(typeof(ServerGameplayActionMessage), nameof(ServerGameplayActionMessage))]
    [JsonSubtypes.KnownSubType(typeof(ServerLobbyMessage), nameof(ServerLobbyMessage))]
    [JsonSubtypes.KnownSubType(typeof(ServerStartProcessMessage), nameof(ServerStartProcessMessage))]
    [JsonSubtypes.KnownSubType(typeof(ServerMatchStateMessage), nameof(ServerMatchStateMessage))]    
    //[JsonSubtypes.KnownSubType(typeof(ServerBoardSetupMessage), nameof(ServerBoardSetupMessage))]
    [JsonSubtypes.KnownSubType(typeof(ServerReadyStatusMessage), nameof(ServerReadyStatusMessage))]
    [JsonSubtypes.KnownSubType(typeof(ServerInformationMessage), nameof(ServerInformationMessage))]
    [JsonSubtypes.KnownSubType(typeof(ServerDatabaseMessage), nameof(ServerDatabaseMessage))]
    public abstract class SharedServerMessage
    {
        public abstract string myType { get; }

        public SharedServerMessage()
        {
        }
    }

    /* PlayerJoined -> myPlayerInfo = enemy player information
     * PlayerReady -> myPlayerInfo = enemy ready player
     * PlayerLeft -> myPlayerInfo = enemy player left
     */
    public class ServerLobbyMessage : SharedServerMessage
    {
        public LobbyMessageId myMessageId;
        public LobbyPlayer myPlayerInfo;
        public string myLobbyId; //NOTE: this will probably not be needed once private lobby id flow is created

        public ServerLobbyMessage(LobbyMessageId aMessageId, LobbyPlayer aPlayerInfo, string aLobbyId) : base()
        {
            myMessageId = aMessageId;
            myPlayerInfo = aPlayerInfo;
            myLobbyId = aLobbyId;
        }

        public override string myType { get; } = nameof(ServerLobbyMessage);
    }

    /* BoardConfig
     * DeckConfig
     * PlayerConfig? */
    public class ServerMatchSetupMessage : SharedServerMessage
    {
        public MatchSetupMessageId myMessageId;
        public int myBoardSize;
        public int myDeckId;

        public ServerMatchSetupMessage(MatchSetupMessageId aMessageId, int aBoardSize, int aDeckId) : base()
        {
            myMessageId = aMessageId;
            myBoardSize = aBoardSize;
            myDeckId = aDeckId;
        }

        public override string myType { get; } = nameof(ServerMatchSetupMessage);
    }

    /* DrawCard
     * UpdateEnergy */
    public class ServerPlayerGameplayMessage : SharedServerMessage
    {
        public PlayerGameplayMessageId myMessageId;
        public string myPlayerId;
        public int myAmount;

        public ServerPlayerGameplayMessage(PlayerGameplayMessageId aMessageId, string aPlayerId, int anAmount) : base()
        {
            myMessageId = aMessageId;
            myPlayerId = aPlayerId;
            myAmount = anAmount;
        }

        public override string myType { get; } = nameof(ServerPlayerGameplayMessage);
    }

    /* StartGame -> myPlayerId = starting player
     * EndGame -> myPlayerId = winner
     * NewTurn -> myPlayerId = new player turn */
    public class ServerStatusGameplayMessage : SharedServerMessage
    {
        public StatusGameplayMessageId myMessageId;
        public string myPlayerId;

        public ServerStatusGameplayMessage(StatusGameplayMessageId aMessageId, string aPlayerId) : base()
        {
            myMessageId = aMessageId;
            myPlayerId = aPlayerId;
        }

        public override string myType { get; } = nameof(ServerStatusGameplayMessage);
    }

    /* Spawn
       Attack 
       Move
       Ability
     */
    public class ServerGameplayActionMessage : SharedServerMessage
    {
        public GameplayMessageIdServer myMessageId;
        public string myRequestingPlayerSessionId;
        public int myGameplayObjectId;
        public int myTargetPositionX;
        public int myTargetPositionY;
        public bool mySuccess;

        public ServerGameplayActionMessage(GameplayMessageIdServer aMessageId, string aRequestingPlayerSessionId, int aGameplayObjectId, int aTargetPositionX, int aTargetPositionY, bool aSuccess) : base()
        {
            myMessageId = aMessageId;
            myRequestingPlayerSessionId = aRequestingPlayerSessionId;
            myGameplayObjectId = aGameplayObjectId;
            myTargetPositionX = aTargetPositionX;
            myTargetPositionY = aTargetPositionY;
            mySuccess = aSuccess;
        }

        public override string myType { get; } = nameof(ServerGameplayActionMessage);
    }

    public class ServerStartProcessMessage : SharedServerMessage
    {
        public StartProcessMessageId myMessageId;
        public int myCountdownStartingNumber;

        public ServerStartProcessMessage(StartProcessMessageId aMessageId, int aCountdownStartingNumber = -1) : base()
        {
            myMessageId = aMessageId;
            myCountdownStartingNumber = aCountdownStartingNumber;
        }

        public override string myType { get; } = nameof(ServerStartProcessMessage);
    }

    public class ServerMatchStateMessage : SharedServerMessage
    {
        public MatchStateMessageId myMessageId;
        public string myPlayerId;
        public SharedBoard.GenerationInfo myBoardInfo;
        public List<ServerGameManager.ReadyPlayerInfo> myPlayerInfo;

        public ServerMatchStateMessage(MatchStateMessageId aMessageId, string aPlayerId = "", SharedBoard.GenerationInfo aBoardInfo = new(), List<ServerGameManager.ReadyPlayerInfo> aPlayerInfo = null) : base()
        {
            myMessageId = aMessageId;
            myPlayerId = aPlayerId;
            myBoardInfo = aBoardInfo;
            myPlayerInfo = aPlayerInfo;

        }

        public override string myType { get; } = nameof(ServerMatchStateMessage);
    }

    //public class ServerBoardSetupMessage : SharedServerMessage
    //{
    //    public DeckSetupMessageId myMessageId;
    //    public int myBoardSeed;
    //    public int myNebulaQty;
    //    public int myBlackHoleQty;

    //    public ServerBoardSetupMessage(DeckSetupMessageId aMessageId, int aBoardSeed, int aNebulaQty, int aBlackHoleQty) : base()
    //    {
    //        myMessageId = aMessageId;
    //        myBoardSeed = aBoardSeed;
    //        myNebulaQty = aNebulaQty;
    //        myBlackHoleQty = aBlackHoleQty;
    //    }

    //    public override string myType { get; } = nameof(ServerBoardSetupMessage);
    //}

    public class ServerReadyStatusMessage : SharedServerMessage
    {
        public ReadyStatusMessageId myMessageId;
        public string myPlayerId;
        public bool myIsReady;
        public int myDeckId;

        public ServerReadyStatusMessage(ReadyStatusMessageId aMessageId, string aPlayerId, bool aIsReady, int aDeckId) : base()
        {
            myMessageId = aMessageId;
            myPlayerId = aPlayerId;
            myIsReady = aIsReady;
            myDeckId = aDeckId;
        }

        public override string myType { get; } = nameof(ServerReadyStatusMessage);
    }

    public class ServerInformationMessage : SharedServerMessage
    {
        public InformationMessageId myMessageId;
        public string myMessage;        

        public ServerInformationMessage(InformationMessageId aMessageId, string aError) : base()
        {
            myMessageId = aMessageId;
            myMessage = aError;
        }

        public override string myType { get; } = nameof(ServerInformationMessage);
    }

    /* 
    * SEND_HOST_SHORT_LOBBY_ID (success) aHoodId = 1 and aValue carries the short lobby id to the host player, (faliure) aHoodId = - 1
    */
    public class ServerDatabaseMessage : SharedServerMessage
    {
        public DatabaseMessageId myMessageId;
        public int myHoodId;
        public string myValue;

        public ServerDatabaseMessage(DatabaseMessageId aMessageId, int aHoodId, string aValue) : base()
        {
            myMessageId = aMessageId;
            myHoodId = aHoodId;
            myValue = aValue;
        }

        public override string myType { get; } = nameof(ServerDatabaseMessage);
    }
}
using JsonSubTypes;
using Newtonsoft.Json;
using SharedScripts;

namespace Assets.Shared.Scripts.Messages.Client
{
    [System.Serializable]
    [JsonConverter(typeof(JsonSubtypes), "myType")]
    [JsonSubtypes.KnownSubType(typeof(ClientGameplayMessage), nameof(ClientGameplayMessage))]
    [JsonSubtypes.KnownSubType(typeof(ClientPlayerGameplayMessage), nameof(ClientPlayerGameplayMessage))]
    [JsonSubtypes.KnownSubType(typeof(ClientLobbyMessage), nameof(ClientLobbyMessage))]
    [JsonSubtypes.KnownSubType(typeof(ClientMatchConnectionMessage), nameof(ClientMatchConnectionMessage))]
    [JsonSubtypes.KnownSubType(typeof(ClientPlayerReadyStatusMessage), nameof(ClientPlayerReadyStatusMessage))]
    [JsonSubtypes.KnownSubType(typeof(ClientTestFeaturesMessage), nameof(ClientTestFeaturesMessage))]
    public abstract class SharedClientMessage
    {
        public abstract string myType { get; }

        public SharedClientMessage()
        {
        }
    }

    /* Connect
     * Ready
     * Leave/Disconnect */
    public class ClientLobbyMessage : SharedClientMessage
    {
        public LobbyMessageIdClient myMessageId;
        public string myPlayerSessionId;
        public string myUsername;
        public bool myLobbyOwner;

        public ClientLobbyMessage(LobbyMessageIdClient aMessageId, string aPlayerSessionId, string aUsername, bool aLobbyOwner) : base()
        {
            myMessageId = aMessageId;
            myPlayerSessionId = aPlayerSessionId;
            myUsername = aUsername;
            myLobbyOwner = aLobbyOwner;
        }

        public override string myType { get; } = nameof(ClientLobbyMessage);
    }

    /* Leave/Disconnect */
    public class ClientMatchConnectionMessage : SharedClientMessage
    {
        public MatchMessageIdClient myMessageId;
        public string myPlayerSessionId;

        public ClientMatchConnectionMessage(MatchMessageIdClient aMessageId, string aPlayerSessionId) : base()
        {
            myMessageId = aMessageId;
            myPlayerSessionId = aPlayerSessionId;
        }

        public override string myType { get; } = nameof(ClientLobbyMessage);
    }

    /* EndMyTurn
     * Surrender */
    public class ClientPlayerGameplayMessage : SharedClientMessage
    {
        public PlayerGameplayMessageIdClient myMessageId;
        public string myPlayerId;

        public ClientPlayerGameplayMessage(PlayerGameplayMessageIdClient aMessageId, string aPlayerId) : base()
        {
            myMessageId = aMessageId;
            myPlayerId = aPlayerId;
        }

        public override string myType { get; } = nameof(ClientPlayerGameplayMessage);
    }

    /* SpawnUnit
     * MoveUnit
     * AttackUnit
     * UseAbility */
    public class ClientGameplayMessage : SharedClientMessage
    {
        public GameplayMessageIdClient myMessageId;
        public string myPlayerId;
        public int myGameplayObjectId;
        public int myTargetPositionX;
        public int myTargetPositionY;

        public ClientGameplayMessage(GameplayMessageIdClient aMessageId, string aPlayerId, int anId, int aTargetPositionX, int aTargetPositionY) : base()
        {
            myMessageId = aMessageId;
            myPlayerId = aPlayerId;
            myGameplayObjectId = anId;
            myTargetPositionX = aTargetPositionX;
            myTargetPositionY = aTargetPositionY;
        }

        public override string myType { get; } = nameof(ClientGameplayMessage);
    }

    public class ClientPlayerReadyStatusMessage : SharedClientMessage
    {
        public ReadyStatusMessageId myMessageId;
        public string myPlayerId;
        public bool myIsReady;
        public string myUsername;
        public int myDeckId;

        public ClientPlayerReadyStatusMessage(ReadyStatusMessageId aMessageId, string aPlayerId, bool aIsReady, string aUsername = "", int aDeckId = -1) : base()
        {
            myMessageId = aMessageId;
            myPlayerId = aPlayerId;
            myIsReady = aIsReady;
            myUsername = aUsername;
            myDeckId = aDeckId;
        }

        public override string myType { get; } = nameof(ClientPlayerReadyStatusMessage);
    }
    
    public class ClientTestFeaturesMessage : SharedClientMessage
    {
        public TestFeaturesMessageId myMessageId;
        public string myPlayerId;
        public ClientTestFeaturesMessage(TestFeaturesMessageId aMessageId, string aPlayerId) : base()
        {
            myMessageId = aMessageId;
            myPlayerId = aPlayerId;
        }

        public override string myType { get; } = nameof(ClientTestFeaturesMessage);
    }
}

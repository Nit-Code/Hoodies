
namespace SharedScripts
{
    public enum GameState
    {
        IN_MAIN_MENU,
        IN_LOBBY,
        IN_MATCH,
        IN_POST_MATCH
    }
    public enum MatchStateMessageId
    {
        INVALID,
        SETUP,
        PLAYER_TURN,
        END_WINNER,
        END_DRAW
    }

    public enum StartProcessMessageId 
    {
        INVALID,
        START_COUNTDOWN,
        START_LOCK_IN,
        STOP_COUNTDOWN,
        COUNTDOWN_OVER,
        START_MATCH
    }

    public enum LobbyMessageIdClient
    { 
        CONNECT,
        DISCONNECT_ME,
        READY
    }

    public enum MatchMessageIdClient
    {
        LEAVE_STARTING_MATCH,
        LEAVE_ONGOING_MATCH,
        LEAVE_ENDED_MATCH
    }

    public enum PlayerGameplayMessageIdClient
    {
        INVALID,
        END_MY_TURN,
        SURRENDER,
    }

    public enum GameplayMessageIdClient
    {
        INVALID,
        REQUEST_SPAWN_UNIT,
        REQUEST_MOVE_UNIT,
        REQUEST_ATTACK_UNIT,
        REQUEST_USE_ABILITY,
        REQUEST_USE_TECHNOLOGY
    }

    public enum ReadyStatusMessageId
    {
        INVALID,
        PLAYER_READY,
        MATCH_SCENE_LOADED,
        PLAYER_READY_AND_LOADED
    }

    public enum TestFeaturesMessageId
    {
        KILL_MY_MOTHERSHIP
    }

    public enum InformationMessageId
    {
        ERROR,
        INFO,
        SERVER_CLOSED
    }

    public enum LobbyMessageId
    {
        CONNECTED,
        PRIVATE_LOBBY_CREATED,
        LOBBY_FULL,     
        PLAYER_LEFT,
        HOST_DISCONNECTED,
        GUEST_DISCONNECTED
    }

    public enum MatchSetupMessageId
    {
        BOARD_AND_DECK_CONFIG,
        PLAYER_CONFIG
    }

    public enum PlayerGameplayMessageId
    {
        DRAW_CARD,
        UPDATE_ENERGY
    }

    public enum StatusGameplayMessageId
    {
        START_GAME,
        NEW_TURN,
        END_GAME        
    }

    public enum UnitGameplayMessageId
    {
        UPDATE_UNITS
    }

    public enum GameplayMessageIdServer
    {
        SPAWN_UNIT,
        ATTACK_UNIT,
        MOVE_UNIT,
        //DRAW_CARD,
        USE_ABILITY,
        USE_TECHNOLOGY,
        //UPDATE_UNITS
    }

    public enum DatabaseMessageId
    {
        SEND_HOST_SHORT_LOBBY_ID
    }

    public enum LobbyStatus
    {
        UNDEFINED,
        CREATING_MATCH,
        MY_PLAYER_CONNECTED,
        LOADING_UI,
        WAITING_FOR_OPPONENT,
        COUNTDOWN,
        LOCKED_IN,
        READY
    }

    public enum GameStatus
    {
        STARTED,
        ENDED
    }

    public enum MatchType 
    {
        INVALID,
        PUBLIC,
        PRIVATE
    }

    public enum MatchState
    {
        INVALID,
        SETUP,
        PLAYER_TURN,
        END
    }

    public enum TurnType
    {
        INVALID,
        PLAYER,
        ENEMY
    }

    public enum MatchWinner
    {
        NO_WINNER,
        PLAYER_1,
        PLAYER_2,
        DRAW
    }

    public enum PlayerType  
    {
        INVALID,
        HOST,
        GUEST
    }

    public enum PlayerColor
    {
        BLUE,
        RED
    }

    // TODO mda: does this need to be here or only on ClientEnums
    public enum SceneName
    {
        LOGIN_SCENE,
        MENU_SCENE,
        MATCH_SCENE,
        PERSISTENT_SCENE,
        UNDEFINED
    }

    public enum AreaShape // This determines the base shape of a cast ability: [ O => ability cast tile | X => Affected tiles | - => Non-affected tiles ] 
    {
        // A size '0' of any shape would only affect the ability cast tile

        SQUARE,
        // XXXXX
        // XXXXX
        // XXOXX    Square of size 2
        // XXXXX
        // XXXXX

        LINE,
        // -------
        // ------- 
        // ---0XXX     Line of size 3
        // -------
        // -------

        CROSS,
        // ---X---
        // ---X---
        // ---X---
        // XXX0XXX      Cross of size 3      
        // ---X---  
        // ---X---
        // ---X---
    }

    public enum TileColor
    {
        WHITE,
        BLUE,
        RED,
        YELLOW,
        GREEN
    }

    namespace DataId
    {
        // Mantain alphabetic order
        // Each base data type at Assets/Shared/DataListsDefinitions should have an id field populated with a custom enum from this namespace,
        // id's can have descriptive names, these are just placeholders to ensure uniqueness
        // TODO: invesigate custom Enumeration clases, could be useful to ensure all our data classes implement their id, but not the same id

        public enum AbilityId
        {
            INVALID,          
            ENGINE_OVERDRIVE,
            PROTECTOR,
            KAMIKAZE,
            REPAIR_STATION
        }

        public enum AudioId
        {
            INVALID,
            NO_AUDIO,
            AMBIENT_RAIN,
            MUSIC_LOGIN_SCENE,
            MUSIC_MENU_SCENE,
            MUSIC_MATCH_SCENE,
            SOUND_MENU_CLICK,
            SOUND_ERROR,
            SOUND_LASER,
            SOUND_EXPLOSION,
            SOUND_MOVE,
            SOUND_SPAWN
        }

        public enum CardId
        {
            INVALID,
            CARD_MOTHERSHIP_TEST,
            CARD_BASIC_UNIT_TEST,
            CARD_3,
            CARD_HUNTER,
            CARD_ROCKET,
            CARD_TANK,
            CARD_FENIX,
            CARD_TRAVELER,
            CARD_SUICIDE_DRONE,
            CARD_REPAIR_STATION
        }

        public enum CardType
        {
            INVALID,
            UNIT,
            TECHNOLOGY,
            MOTHERSHIP
        }

        public enum SceneId
        {
            INVALID,
            LOGIN,
            MENU,
            MATCH,
            PERSISTENT,
            SERVER_BASE
        }

        public enum StatusEffectId
        {
            INVALID,
            IMPROVED_ENGINES,
            PROTECTOR_AURA
        }

        public enum TileType
        {
            INVALID,
            EMPTY,
            NEBULA,
            BLACKHOLE
        }

        public enum UnitId
        {
            INVALID,
            UNIT_MOTHERSHIP_TEST,
            UNIT_BASIC_UNIT_TEST,
            UNIT_3,
            UNIT_HUNTER,
            UNIT_ROCKET,
            UNIT_TANK,
            UNIT_FENIX,
            UNIT_TRAVELER,
            UNIT_SUICIDE_DRONE,
        }

        public enum FloatRangeOptionId
        {
            INVALID,
            VOLUME_MASTER,
            VOLUME_MUSIC,
            VOLUME_SOUND,
            VOLUME_AMBIENT
        }

        public enum BooleanOptionId
        {
            INVALID,
            BOOLEAN_OPTION_1
        }
    }
}

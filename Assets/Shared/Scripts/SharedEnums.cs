
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
        DISCONNECT,
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
        INFO
    }

    public enum LobbyMessageId
    {
        CONNECTED,
        PRIVATE_LOBBY_CREATED,
        LOBBY_FULL,     
        PLAYER_LEFT
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
            ABILITY_3
        }

        public enum AudioId
        {
            INVALID,
            NO_AUDIO,
            AMBIENT_RAIN,
            MUSIC_LOGIN_SCENE,
            MUSIC_MENU_SCENE,
            SOUND_MENU_CLICK,
            SOUND_ERROR
        }

        public enum CardId
        {
            INVALID,
            CARD_MOTHERSHIP_TEST,
            CARD_BASIC_UNIT_TEST,
            CARD_3,
            CARD_HUNTER_SPAWNER,
            CARD_ROCKET_SPAWNER,
            CARD_TANK_SPAWNER,
            CARD_FENIX_SPAWNER,
            CARD_TRAVELER_SPAWNER
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

        //public enum TileId
        //{
        //    INVALID,
        //    EMPTY,
        //    NEBULA,
        //    BLACK_HOLE
        //}

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
            UNIT_TRAVELER
        }
    }
}

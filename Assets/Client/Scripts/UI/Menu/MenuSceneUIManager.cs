using UnityEngine;
using SharedScripts;
using SharedScripts.DataId;
using System.Collections.Generic;

public class MenuSceneUIManager : MonoBehaviour
{
    private enum MenuCanvasId
    {
        INVALID,
        HOME,
        LOBBY,
        MY_DECKS,
        NONE
    }

    [SerializeField] private MenuCanvasId myBaseCanvas;
    private MenuCanvasId myCurrentCanvas;
    private Dictionary<MenuCanvasId, GameObject> myCanvasMap;

    // canvas
    [SerializeField] private HomeCanvasUIManager myHomeCanvasReference;
    [SerializeField] private LobbyCanvasUIManager myLobbyCanvasReference;
    [SerializeField] private MyDecksCanvasUIManager myMyDecksReference;

    // reference
    private GameLiftClient myGameLiftClientReference;
    private SceneController mySceneControllerReference;
    private NetworkClient myNetworkClientReference;
    private SharedUser mySharedUserReference;
    private ClientLambda myLambdaReference;
    public NetworkClient GetNetworkClient() { return myNetworkClientReference; }

    //state lobby
    private LobbyStatus myLobbyStatus;
    public LobbyStatus GetLobbyStatus() { return myLobbyStatus; }
    private MatchType myMatchType;
    private PlayerType myPlayerType;
    private bool myIsLobbyOwner;
    public bool GetIsLobbyOwner() { return myIsLobbyOwner; }
    private string myMatchId;

    private void Awake()
    {
        LoadCanvasMap();
        DeactivateAllCanvas();
        myCurrentCanvas = MenuCanvasId.NONE;
        SwitchToBaseCanvas();
    }

    private void Start()
    {
        mySceneControllerReference = FindObjectOfType<SceneController>();
        //TODO: add error logs if references arent found

        myNetworkClientReference = FindObjectOfType<NetworkClient>();
        if (myNetworkClientReference != null)
        {
            myNetworkClientReference.SetMenuSceneReference(this);
        }

        myGameLiftClientReference = FindObjectOfType<GameLiftClient>();
        if (myGameLiftClientReference == null)
        {
            Debug.LogError("[HOOD][CLIENT][SCENE] - myGameLiftClientReference not found");
        }

        mySharedUserReference = FindObjectOfType<SharedUser>();
        if (mySharedUserReference == null)
        {
            Debug.LogError("[HOOD][CLIENT][SCENE] - mySharedUserReference not found");
        }

        myLambdaReference = FindObjectOfType<ClientLambda>();
        if (myLambdaReference == null)
        {
            Debug.LogError("[HOOD][CLIENT][SCENE] - myLambdaReference not found");
        }
    }

    public LobbyCanvasUIManager GetLobbyCanvasReference()
    {
        return myLobbyCanvasReference;
    }

    private void SetupLobby()
    {
        if (myPlayerType == PlayerType.HOST)
        {
            Debug.Log("[HOOD][CLIENT][PRIVATE_LOBBY][HOST] - Step 9/9 - Setting up lobby UI.");
            myHomeCanvasReference.HideCreatingLobbyPopup();
        }
        else if (myPlayerType == PlayerType.GUEST)
        {
            Debug.Log("[HOOD][CLIENT][PRIVATE_LOBBY][GUEST] - Step 9/9 - Setting up lobby UI.");
            myHomeCanvasReference.HideFindingMatchPopup();
        }

        string gameSessionId = myGameLiftClientReference.GetGameSessionId();
        string playerName = mySharedUserReference.GetUsername();
        myLobbyStatus = LobbyStatus.LOADING_UI;
        SwitchCanvas(MenuCanvasId.LOBBY);
        myLobbyCanvasReference.SetupLobbyUI(myMatchType, playerName);
        myLobbyStatus = LobbyStatus.WAITING_FOR_OPPONENT;
    }

    public void Quit()
    {
        Debug.Log("[HOOD][CLIENT][SCENE] - Quit");
        //TODO: Should we do anything before closing the game?

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    #region ManageCanvas

    private void LoadCanvasMap()
    {
        myCanvasMap = new Dictionary<MenuCanvasId, GameObject>();
        myCanvasMap.Add(MenuCanvasId.HOME, myHomeCanvasReference.gameObject);
        myCanvasMap.Add(MenuCanvasId.LOBBY, myLobbyCanvasReference.gameObject);
        myCanvasMap.Add(MenuCanvasId.MY_DECKS, myMyDecksReference.gameObject);
    }

    private void SwitchCanvas(MenuCanvasId aTargetCanvas)
    {
        if (aTargetCanvas == myCurrentCanvas || aTargetCanvas == MenuCanvasId.INVALID)
        {
            Debug.LogError("[HOOD][CLIENT][SCENE] - SwitchCanvas()");
            return;
        }

        // Deactivate current canvas if any
        if (myCurrentCanvas == MenuCanvasId.NONE || SetCanvasActiveValue(myCurrentCanvas, false))
        {
            // Activate target canvas if any
            if (SetCanvasActiveValue(aTargetCanvas, true))
            {
                myCurrentCanvas = aTargetCanvas;
            }
        }
    }

    private bool SetCanvasActiveValue(MenuCanvasId anId, bool aValue)
    {
        if (anId == MenuCanvasId.INVALID)
        {
            Debug.LogError("[HOOD][CLIENT][SCENE] - SetCanvasActiveValue()");
            return false;
        }

        if (myCanvasMap.TryGetValue(anId, out GameObject canvasGameObject))
        {
            if (canvasGameObject != null)
            {
                canvasGameObject.SetActive(aValue);
                return true;
            }
        }

        Debug.LogError("[HOOD][CLIENT][SCENE] - SetCanvasActiveValue()");
        return false;
    }

    private void DeactivateAllCanvas()
    {
        SetAllCanvasActiveValue(false);
        myCurrentCanvas = MenuCanvasId.NONE;
    }

    private void SetAllCanvasActiveValue(bool aValue)
    {
        foreach (KeyValuePair<MenuCanvasId, GameObject> canvas in myCanvasMap)
        {
            if (canvas.Key == MenuCanvasId.INVALID)
            {
                Debug.LogError("[HOOD][CLIENT][SCENE] - SetAllCanvasActiveValue()");
                continue;
            }

            SetCanvasActiveValue(canvas.Key, aValue);
        }
    }

    public void SwitchToBaseCanvas()
    {
        if (myBaseCanvas == MenuCanvasId.INVALID)
        {
            Debug.LogError("[HOOD][CLIENT][SCENE] - BackToBaseCanvas()");
            return;
        }

        SwitchCanvas(myBaseCanvas);
    }

    public void LoadMyDecksCanvas()
    {
        SwitchCanvas(MenuCanvasId.MY_DECKS);
    }

    public void LoadPrivateLobbyCanvas(PlayerType aPlayerType, string aShortLobbyId)
    {
        if (aPlayerType == PlayerType.HOST)
        {
            Debug.Log("[HOOD][CLIENT][PRIVATE_LOBBY][HOST] - Step 2/9 - Initializing lobby vars.");
            myIsLobbyOwner = true;
        }
        else if (aPlayerType == PlayerType.GUEST)
        {
            Debug.Log("[HOOD][CLIENT][PRIVATE_LOBBY][GUEST] - Step 2/9 - Initializing lobby vars.");
        }

        myPlayerType = aPlayerType;
        myMatchType = MatchType.PRIVATE;
        CreateOrJoinMatch(aShortLobbyId);
    }

    public void LoadPublicLobbyCanvas()
    {
        myMatchType = MatchType.PUBLIC;
        CreateOrJoinMatch("");
        SwitchCanvas(MenuCanvasId.LOBBY);
    }
    #endregion

    #region SceneNavigation



    // TODO: temporary <- why is this temporary?
    public void NavigateToMatchScene()
    {
        DeactivateAllCanvas();
        mySceneControllerReference.LoadScene(SceneId.MATCH);
    }

    public void NavigateToLoginScene()
    {
        DeactivateAllCanvas();
        mySceneControllerReference.LoadScene(SceneId.LOGIN);
    }

    #endregion

    #region LobbyToNetwork
    public void CreateOrJoinMatch(string aPrivateLobbyId)
    {
        myLobbyStatus = LobbyStatus.CREATING_MATCH;
        if (myMatchType == MatchType.PRIVATE && myPlayerType == PlayerType.HOST)
        {
            //HOST PRIVATE MATCH
            myGameLiftClientReference.CreateOrJoinMatch("PRIVATE_HOST");
        }
        else if (myMatchType == MatchType.PRIVATE && myPlayerType == PlayerType.GUEST)
        {
            if (CLU.GetIsConnectLocalEnabled())
            {
                myGameLiftClientReference.CreateOrJoinMatch("PRIVATE_GUEST");
            }
            else
            {
                //JOIN PRIVATE MATCH
                myLambdaReference.InvokeGetGameSessionId(aPrivateLobbyId, myGameLiftClientReference, this);
            }
        }
        else if (myMatchType == MatchType.PUBLIC)
        {
            //FIND PUBLIC MATCH
            myGameLiftClientReference.CreateOrJoinMatch("PUBLIC");
        }
        else
        {
            Debug.LogError("[HOOD][CLIENT][LOBBY] - Unhandled state at CreateMatch()");
        }
    }

    public void DisconnectFromLobby()
    {
        myNetworkClientReference.DisconnectMeFromLobby(myIsLobbyOwner);
        ResetLobbyVars();
        SwitchToBaseCanvas();
    }
    #endregion

    #region NetworkToLobby
    public void Lobby_CONNECTED()
    {
        if (myPlayerType == PlayerType.HOST)
        {
            Debug.Log("[HOOD][CLIENT][PRIVATE_LOBBY][HOST] - Step 8/9 - Telepathy connection successful.");
        }
        else if (myPlayerType == PlayerType.GUEST)
        {
            Debug.Log("[HOOD][CLIENT][PRIVATE_LOBBY][GUEST] - Step 8/9 - Telepathy connection successful.");
        }

        // At this point, connection with gamelift server has been established and verified
        myLobbyStatus = LobbyStatus.MY_PLAYER_CONNECTED;
        SetupLobby();
    }

    public void Lobby_PRIVATE_LOBBY_CREATED(string aMatchId)
    {
        myMatchId = aMatchId;
    }

    public void Lobby_LOBBY_FULL()
    {
        //TODO: FIGURE OUT HOW TO WAIT FOR GUEST TO BECOME CONECTED BEFORE SWITCHING TO THIS STATE
        //myLobbyStatus = LobbyStatus.WAITING_PLAYERS_TO_LOCK_IN;
    }

    public void Lobby_HOST_DISCONNECTED()
    {
        //We are guest
        myHomeCanvasReference.ShowLobbyClosedPopup();
        ResetLobbyVars();
        SwitchToBaseCanvas();
    }

    public void Lobby_GUEST_DISCONNECTED()
    {
        //We are host
        myLobbyCanvasReference.ResetLobbyUI();
        myLobbyStatus = LobbyStatus.WAITING_FOR_OPPONENT;
    }

    public void Lobby_DISCONNECTED_FROM_LOBBY(bool aIsLocalPlayerDisconnect)
    {
        if (myPlayerType == PlayerType.HOST && !aIsLocalPlayerDisconnect) // I'm the host and the guest disconnected
        {
            myLobbyCanvasReference.ResetLobbyUI();
            return;
        }

        if(myPlayerType == PlayerType.GUEST && !aIsLocalPlayerDisconnect)
        {
            ResetLobbyVars();
            myHomeCanvasReference.ShowLobbyClosedPopup();
        }
        ResetLobbyVars();
        SwitchToBaseCanvas();
    }

    public void OnGameSessionCreationFailed()
    {
        if (myMatchType == MatchType.PRIVATE)
        {
            if (myPlayerType == PlayerType.HOST)
            {
                Debug.Log("[HOOD][CLIENT][PRIVATE_LOBBY][HOST] - Step 5/9 - Game session creation failed, reset vars and show message.");
                myHomeCanvasReference.HideCreatingLobbyPopup();
            }
            else if (myPlayerType == PlayerType.GUEST)
            {
                Debug.Log("[HOOD][CLIENT][PRIVATE_LOBBY][GUEST] - Step 5/9 - Game session find failed, reset vars and show message.");
                myHomeCanvasReference.HideFindingMatchPopup();
            }
        }

        ResetLobbyVars();
        myHomeCanvasReference.ShowSomethingWentWrongPopup();
    }

    public void OnPlayerSessionCreationFailed()
    {
        if (myMatchType == MatchType.PRIVATE)
        {
            if (myPlayerType == PlayerType.HOST)
            {
                Debug.Log("[HOOD][CLIENT][PRIVATE_LOBBY][HOST] - Step 7/9 - Player session creation failed, reset vars and show message.");
                myHomeCanvasReference.HideCreatingLobbyPopup();
            }
            else if (myPlayerType == PlayerType.GUEST)
            {
                Debug.Log("[HOOD][CLIENT][PRIVATE_LOBBY][GUEST] - Step 7/9 - Player session creation failed, reset vars and show message.");
                myHomeCanvasReference.HideFindingMatchPopup();
            }
        }

        ResetLobbyVars();
        myHomeCanvasReference.ShowSomethingWentWrongPopup();
    }

    public void StartLobbyTimer(int aStartingNumer)
    {
        myLobbyCanvasReference.StartTimer(aStartingNumer);
    }

    public void ResetLobbyTimer()
    {
        myLobbyCanvasReference.ResetTimer();
    }

    public void StartTimerLockedIn()
    {
        myLobbyCanvasReference.ChangeToLockedIn();
    }

    public void SetShortLobbyId(string aShortLobbyId) 
    {
        if (!string.IsNullOrEmpty(aShortLobbyId)) 
        {
            myLobbyCanvasReference.SetShortLobbyId(aShortLobbyId);
        }
    }

    private void ResetLobbyVars()
    {
        myLobbyStatus = LobbyStatus.UNDEFINED;
        myIsLobbyOwner = false;
        myMatchType = MatchType.INVALID;
        myPlayerType = PlayerType.INVALID;
        myMatchId = "";
        myLobbyCanvasReference.ResetLobbyUI();
    }
    #endregion
}


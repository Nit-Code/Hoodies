using System;
using System.Collections.Generic;
using UnityEngine;
using Aws.GameLift.Server;
using Aws.GameLift.Server.Model;
using Aws.GameLift;

// Based on https://docs.aws.amazon.com/gamelift/latest/developerguide/integration-engines-unity-using.html

/*
Class goal:
    - Interact with gamelift api 
*/
public class GameLiftServer : MonoBehaviour
{
    // Server used to communicate with client
    private NetworkServer myNetworkServerReference;

    // Identify port number the game server is listening on for player connections 
    private static int myTCPServerPort;

    private const string myLocalLogStore = "~/Library/Logs/Unity/server.log";
    private const string myRemoteLogStore = "/local/game/logs/server.log";  // yes remote has local in its name, no that is not a mistake
    private bool myIsReadyToStart;
    private string myGameSessionId;
    public string GetGameSessionId() { return myGameSessionId; }

    private void Awake()
    {
        if (!TryGetComponent<NetworkServer>(out myNetworkServerReference))
        {
            Shared.LogError("[HOOD][SERVER][GAMELIFT] - Component not found");
            myIsReadyToStart = false;
        }
        myIsReadyToStart = true;
    }

    private void Start()
    {
        if (!myIsReadyToStart)
        {
            Shared.LogError("[HOOD][SERVER][GAMELIFT] - Not starting");
            return;
        }

        if (CLU.GetIsConnectLocalEnabled())
        {
            OnStartLocal();
        }
        else 
        {
            OnStartRemote();
        }
    }

    private void OnStartRemote()
    {
        //InitSDK will establish a local connection with GameLift's agent to enable further communication.
        GenericOutcome initSDKOutcome = GameLiftServerAPI.InitSDK();
        if (!initSDKOutcome.Success)
        {
            Shared.Log("[HOOD][SERVER][GAMELIFT] - InitSDK failure : " + initSDKOutcome.Error.ToString());
            return;
        }

        SetPort();
        LogParameters lp = GetLogParameters();
        ProcessParameters processParameters = new ProcessParameters(OnGameSession, OnGameSessionUpdate, OnProcessTerminate, OnHealthCheck, myTCPServerPort, lp);

        GenericOutcome processReadyOutcome = GameLiftServerAPI.ProcessReady(processParameters);
        if (!processReadyOutcome.Success)
        {
            Shared.Log("[HOOD][SERVER][GAMELIFT] - ProcessReady failure : " + processReadyOutcome.Error.ToString());
            return;
        }

        Shared.Log("[HOOD][SERVER][GAMELIFT] - ProcessReady success, server is ready to receive incoming game sessions.");
        myNetworkServerReference.StartTCPServer(myTCPServerPort);
    }

    private void OnStartLocal()
    {
        SetPort();
        myNetworkServerReference.StartTCPServer(myTCPServerPort);
        myGameSessionId = "local";
    }

    private void SetPort()
    {
        if (CLU.GetIsConnectLocalEnabled())
            myTCPServerPort = CLU.GetConnectLocalServerPort();
        else
            myTCPServerPort = UnityEngine.Random.Range(Shared.ourTCPServerPortRangeMin, Shared.ourTCPServerPortRangeMax);

        Shared.Log("[HOOD][SERVER][GAMELIFT] - TCP Port: " + myTCPServerPort);
    }

    private LogParameters GetLogParameters()
    {
        List<String> logList = new List<String>();
        if (CLU.GetIsConnectLocalEnabled())
            logList.Add(myLocalLogStore);
        else
            logList.Add(myRemoteLogStore);

        return new LogParameters(logList);
    }

    public GenericOutcome IncomingPlayerSession(string aPlayerSessionId)
    {
        Shared.Log("[HOOD][SERVER][GAMELIFT] - IncomingPlayerSession for player session id: " + aPlayerSessionId);
        GenericOutcome outcome = GameLiftServerAPI.AcceptPlayerSession(aPlayerSessionId);
        if (outcome.Success)
            Shared.Log("[HOOD][SERVER][GAMELIFT] - Player Session accepted.");
        else
            Shared.Log("[HOOD][SERVER][GAMELIFT] - Player Session rejected " + outcome.Error.ToString());

        return outcome;
    }

    public void RemovePlayerSession(string aPlayerSessionId)
    {
        Shared.Log("[HOOD][SERVER][GAMELIFT] - RemovePlayerSession for player session id: " + aPlayerSessionId);
        try
        {
            // Remove players from the game session that disconnected
            var outcome = GameLiftServerAPI.RemovePlayerSession(aPlayerSessionId);

            if (outcome.Success)
                Shared.Log("[HOOD][SERVER][GAMELIFT] - PLAYER SESSION REMOVED");
            else
                Shared.Log("[HOOD][SERVER][GAMELIFT] - PLAYER SESSION REMOVE FAILED. RemovePlayerSession() returned " + outcome.Error.ToString());
        }
        catch (Exception e)
        {
            Shared.Log("[HOOD][SERVER][GAMELIFT] - PLAYER SESSION REMOVE FAILED. RemovePlayerSession() exception " + Environment.NewLine + e.Message);
            throw;
        }
    }

    public void HandleGameEnd()
    {
        Shared.Log("[HOOD][SERVER][GAMELIFT] - HandleGameEnd");
        if (!CLU.GetIsConnectLocalEnabled())
            FinalizeServerProcessShutdown();
        else
            FinalizeLocalServerProcessShutdown();
    }

    private void OnGameSession(GameSession aGameSession)
    {
        // When a game session is created, GameLift sends an activation request to the game server and passes along 
        // the game session object containing game properties and other settings. Here is where a game server should 
        // take action based on the game session object. Once the game server is ready to receive incoming player 
        // connections, it should invoke GameLiftServerAPI.ActivateGameSession()
        Shared.Log("[HOOD][SERVER][GAMELIFT] - OnGameSession");
        GameLiftServerAPI.ActivateGameSession();
        myGameSessionId = aGameSession.GameSessionId;
    }

    private void OnProcessTerminate()
    {
        // OnProcessTerminate callback. GameLift will invoke this callback before shutting down an instance hosting this game server.
        // It gives this game server a chance to save its state, communicate with services, etc., before being shut down.

        // From the Docs: https://docs.aws.amazon.com/gamelift/latest/developerguide/integration-server-sdk-csharp-ref-actions.html#integration-server-sdk-csharp-ref-getterm
        // GameLift may call onProcessTerminate() for the following reasons: (1) for poor health (the server process has 
        // reported port health or has not responded to GameLift, (2) when terminating the instance during a scale-down event, 
        // or (3) when an instance is being terminated due to a spot-instance interruption.
        Shared.Log("[HOOD][SERVER][GAMELIFT] - OnProcessTerminate");

        FinalizeServerProcessShutdown();
    }

    private void OnGameSessionUpdate(UpdateGameSession anUpdateGameSession)
    {
        // When a game session is updated (e.g. by FlexMatch backfill), GameLiftsends a request to the game
        // server containing the updated game session object.  The game server can then examine the provided
        // matchmakerData and handle new incoming players appropriately.
        // updateReason is the reason this update is being supplied.
        Shared.Log("[HOOD][SERVER][GAMELIFT] - OnGameSessionUpdate");
    }

    private bool OnHealthCheck()
    {
        // This is the HealthCheck callback. GameLift will invoke this callback every 60 seconds or so.
        // Here, a game server might want to check the health of dependencies and such. Simply return true if 
        // healthy, false otherwise. The game server has 60 seconds to respond with its health status. GameLift 
        // will default to 'false' if the game server doesn't respond in time. In this case, we're always healthy!
        return true;
    }

    // A Unity callback when the program is quitting
    private void OnApplicationQuit()
    {
        Shared.Log("[HOOD][SERVER][GAMELIFT] - GameLiftServer.OnApplicationQuit");

        FinalizeServerProcessShutdown();
    }

    private void FinalizeServerProcessShutdown()
    {
        Shared.Log("[HOOD][SERVER][GAMELIFT] - GameLiftServer.FinalizeServerProcessShutdown");

        // All game session clean up should be performed before this, as it should be the last thing that
        // is called when terminating a game session. After a successful outcome from ProcessEnding, make 
        // sure to call Application.Quit(), otherwise the application does not shutdown properly. see:
        // https://forums.awsgametech.com/t/server-process-exited-without-calling-processending/5762/17

        GenericOutcome outcome = GameLiftServerAPI.ProcessEnding();
        if (outcome.Success)
            Shared.Log("[HOOD][SERVER][GAMELIFT] - ProcessEnding success!");
        else
            Shared.Log("[HOOD][SERVER][GAMELIFT] - ProcessEnding failed " + outcome.Error.ToString());

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void FinalizeLocalServerProcessShutdown()
    {
        Shared.Log("[HOOD][SERVER][GAMELIFT] - FinalizeLocalServerProcessShutdown");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

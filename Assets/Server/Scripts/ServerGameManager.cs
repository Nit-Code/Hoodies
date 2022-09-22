using Assets.Shared.Scripts.Messages.Server;
using SharedScripts;
using SharedScripts.DataId;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ServerGameManager : SharedGameManager
{
    public class ReadyPlayerInfo
    {
        public int myDeckId;
        public int myDeckSeed;
        public string myPlayerSessionId;
        public CardId myMothershipId;
        public string myUsername;
        public Vector2Int myMothershipCoord;
        public SharedPlayer.BoardSide myBoardSide;

        public ReadyPlayerInfo(int aDeckId, int aDeckSeed, string aPlayerSessionId, CardId aMothershipId, string aUsername)
        {
            myDeckId = aDeckId;
            myDeckSeed = aDeckSeed;
            myPlayerSessionId = aPlayerSessionId;
            myMothershipId = aMothershipId;
            myUsername = aUsername;
        }
    }

    //public struct GameRules
    //{
    //    maxCards
    //        maxEnergy
    //        startingEnergy
    //        turnMaxTime
    //}

    [SerializeField] private int myLobbyCountdownTime = 15;
    [SerializeField] private int myLockInTime = 5;
    [SerializeField] private int myTurnTimer = 90;
    private bool myLobbyCountdownIsRunning = false;

    private Coroutine myLobbyCountdownCoroutine;
    private Coroutine myTurnCountdownCoroutine;

    private List<string> myReadyAndLoadedPlayers;
    private Dictionary<string, ReadyPlayerInfo> myReadyPlayersInfo;

    private NetworkServer myNetworkServerReference;

    private new void Awake()
    {
        base.Awake();
        myReadyAndLoadedPlayers = new List<string>();
        myReadyPlayersInfo = new Dictionary<string, ReadyPlayerInfo>();
        myLobbyCountdownTime = 15;
        myLockInTime = 5;
        myTurnTimer = 60;
    }

    private new void Start()
    {
        base.Start();
        myNetworkServerReference = FindObjectOfType<NetworkServer>();
    }

    public void ChangeState(MatchState aNewState, string aPlayerSessionId = null)
    {
        this.myMatchState = aNewState;

        ServerMatchStateMessage newStateMessage = null;

        switch (aNewState)
        {
            case MatchState.SETUP:
                newStateMessage = new ServerMatchStateMessage(MatchStateMessageId.SETUP, aPlayerSessionId, myBoardReference.GetGenerationInfo(), myReadyPlayersInfo.Values.ToList());
                myNetworkServerReference.SendMessageToPlayerBySession(aPlayerSessionId, newStateMessage);
                break;
            case MatchState.PLAYER_TURN:
                if (myTurnCountdownCoroutine != null)
                {
                    StopCoroutine(myTurnCountdownCoroutine);
                }
                InitializeNewState(aNewState, aPlayerSessionId);
                newStateMessage = new ServerMatchStateMessage(MatchStateMessageId.PLAYER_TURN, myCurrentPlayerReference.GetSessionId());
                myNetworkServerReference.SendMessageToAllPlayers(newStateMessage);
                myTurnCountdownCoroutine = StartCoroutine(TurnCountdownTimer()); //Commented for testing
                break;
            case MatchState.END:
                if (myMatchWinner == MatchWinner.NO_WINNER)
                    return;
                InitializeNewState(aNewState);
                myNetworkServerReference.SetGameState(GameState.IN_POST_MATCH);
                if (myMatchWinner == MatchWinner.PLAYER_1)
                {
                    newStateMessage = new ServerMatchStateMessage(MatchStateMessageId.END_WINNER, GetPlayer1().GetSessionId());
                }
                else if (myMatchWinner == MatchWinner.PLAYER_2)
                {
                    newStateMessage = new ServerMatchStateMessage(MatchStateMessageId.END_WINNER, GetPlayer2().GetSessionId());
                }
                else if (myMatchWinner == MatchWinner.DRAW)
                {
                    newStateMessage = new ServerMatchStateMessage(MatchStateMessageId.END_DRAW);
                }
                myNetworkServerReference.SendMessageToAllPlayers(newStateMessage);
                ResetMatch();
                myNetworkServerReference.EndMatch();
                break;
        }
    }

    private void ResetMatch()
    {
        myReadyAndLoadedPlayers.Clear();
        myReadyPlayersInfo.Clear();
        myPlayersReferenceList.Clear();
        mySpawnedUnitDictionary.Clear();
    }

    public bool AddReadyPlayer(string aPlayerSessionId, int aPlayerDeckId, string aUsername)
    {
        int shuffleDeckSeed = new System.Random().Next(1, 50000);
        SharedDeck playerDeck = GetShuffledDeck(aPlayerDeckId, shuffleDeckSeed);

        if (playerDeck == null)
        {
            SharedServerMessage errorMessage = new ServerInformationMessage(InformationMessageId.ERROR, "Couldn't find player deck (AddReadyPlayer)");
            myNetworkServerReference.SendMessageToAllPlayers(errorMessage);

            return false;
        }

        SharedPlayer readyPlayer = new SharedPlayer(aPlayerSessionId, playerDeck);
        myPlayersReferenceList.Add(readyPlayer);

        ReadyPlayerInfo playerInfo = new ReadyPlayerInfo(aPlayerDeckId, shuffleDeckSeed, aPlayerSessionId, readyPlayer.GetMothershipCard(), aUsername);
        myReadyPlayersInfo.Add(aPlayerSessionId, playerInfo);

        if (myPlayersReferenceList.Count == 2)
        {
            ServerStartProcessMessage startProcessMessage = new ServerStartProcessMessage(StartProcessMessageId.START_COUNTDOWN, myLobbyCountdownTime);
            myNetworkServerReference.SendMessageToAllPlayers(startProcessMessage);
            myLobbyCountdownCoroutine = StartCoroutine(LobbyCountdownTimer());
        }
        return true;
    }

    public void AddReadyAndLoadedPlayer(string aPlayerSessionId)
    {
        myReadyAndLoadedPlayers.Add(aPlayerSessionId);

        if (myReadyAndLoadedPlayers.Count == 2)
        {
            int startingPlayerIndex = new System.Random().Next(0, 1);
            myCurrentPlayerReference = myPlayersReferenceList[startingPlayerIndex];
            ChangeState(MatchState.PLAYER_TURN, myCurrentPlayerReference.GetSessionId());
        }
    }

    public void RemoveReadyPlayer(string aPlayerSessionId)
    {
        if (!myTimerLockedIn)
        {
            for (int i = 0; i < myPlayersReferenceList.Count; i++)
            {
                if (myPlayersReferenceList[i].GetSessionId() == aPlayerSessionId)
                {
                    myPlayersReferenceList.RemoveAt(i);
                }
            }

            if (myReadyPlayersInfo.ContainsKey(aPlayerSessionId))
            {
                myReadyPlayersInfo.Remove(aPlayerSessionId);
            }

            if (myLobbyCountdownIsRunning)
            {
                StopCoroutine(myLobbyCountdownCoroutine);
                ServerStartProcessMessage startProcessMessage = new ServerStartProcessMessage(StartProcessMessageId.STOP_COUNTDOWN);
                myNetworkServerReference.SendMessageToAllPlayers(startProcessMessage);
            }
        }
    }

    public IEnumerator LobbyCountdownTimer()
    {
        myLobbyCountdownIsRunning = true;
        int timeLeft = myLobbyCountdownTime;

        while (timeLeft > 0)
        {
            yield return new WaitForSeconds(1f);
            timeLeft--;

            if (timeLeft == myLockInTime)
            {
                myTimerLockedIn = true;
                ServerStartProcessMessage startLockInMessage = new ServerStartProcessMessage(StartProcessMessageId.START_LOCK_IN);
                myNetworkServerReference.SendMessageToAllPlayers(startLockInMessage);

                SetBoardStartupData();
                SpawnMotherships();
                DrawStartingHands();
            }
            else
            {
                ServerInformationMessage message = new ServerInformationMessage(InformationMessageId.INFO, "Lobby countdown timer: " + timeLeft);
                myNetworkServerReference.SendMessageToAllPlayers(message);
            }
        }
        ServerStartProcessMessage startCountdownOverMessage = new ServerStartProcessMessage(StartProcessMessageId.COUNTDOWN_OVER);
        myNetworkServerReference.SendMessageToAllPlayers(startCountdownOverMessage);
        yield return null;
    }

    public IEnumerator TurnCountdownTimer()
    {
        yield return new WaitForSeconds(3f); // This wait coincides with how long the "Player Turn" popup that players see lasts. 

        int timeLeft = myTurnTimer;

        while (timeLeft > 0)
        {
            yield return new WaitForSeconds(1f);
            timeLeft--;
        }
        EndTurn();
        yield return null;
    }


    public void SetBoardStartupData()
    {
        ServerInformationMessage StartMessage = new ServerInformationMessage(InformationMessageId.INFO, "SetBoardStartupData() started");
        myNetworkServerReference.SendMessageToAllPlayers(StartMessage);

        //Variable map size?
        int height = 7;
        int width = 13;
        int mapTileQty = height * width;
        int nebulaQty = new System.Random().Next(mapTileQty / 10, mapTileQty / 4); //At least 1/10 of the map should have nebulas. 1/4 at most
        int blackHoleQty = new System.Random().Next(mapTileQty / 20, mapTileQty / 8); //At least 1/20 of the map should have black holes. 1/8 at most

        int mapSeed = new System.Random().Next(1, 50000);

        myBoardReference.Init(mapSeed, nebulaQty, blackHoleQty, width, height);

        while (!myBoardReference.IsMapFullyAccessible())
        {
            myBoardReference.DestroyAllTiles();
            mapSeed++;
            myBoardReference.Init(mapSeed, nebulaQty, blackHoleQty, width, height);
        }

        mySpawnedUnitDictionary.Clear(); // IsMapFullyAccessible() spawns an auxiliary unit
    }

    private void SpawnMotherships()
    {
        SharedPlayer player1 = GetPlayer1();
        SharedPlayer player2 = GetPlayer2();

        if (player1 == null || player2 == null)
        {
            Shared.LogError("[HOOD][SERVER][NETWORK] - null players at SetBoardStartupData");
            return;
        }

        CardId player1Mothership = player1.GetMothershipCard();
        CardId player2Mothership = player2.GetMothershipCard();

        if (player1Mothership == CardId.INVALID || player2Mothership == CardId.INVALID)
        {
            Shared.LogError("[HOOD][SERVER][NETWORK] - INVALID motherships at SetBoardStartupData");
            return;
        }

        SharedTile motherShip1Tile = myBoardReference.GetMotherShip1SpawnTile();
        SharedTile motherShip2Tile = myBoardReference.GetMotherShip2SpawnTile();
        DoSpawnMothership(player1Mothership, motherShip1Tile, player1);
        DoSpawnMothership(player2Mothership, motherShip2Tile, player2);

        ReadyPlayerInfo player1Info = myReadyPlayersInfo[player1.GetSessionId()];
        ReadyPlayerInfo player2Info = myReadyPlayersInfo[player2.GetSessionId()];

        player1Info.myMothershipCoord = motherShip1Tile.GetCoordinate();
        player1Info.myBoardSide = SharedPlayer.BoardSide.LEFT;
        player2Info.myMothershipCoord = motherShip2Tile.GetCoordinate();
        player2Info.myBoardSide = SharedPlayer.BoardSide.RIGHT;
    }
   
    private void DrawStartingHands()
    {
        foreach (SharedPlayer player in myPlayersReferenceList)
        {
            bool fullHand = false;
            while (!fullHand)
            {
                CardId drawedCard = player.TryDrawCard(); // Do draw card returns INVALID if your hand is full or if your deck is empty.
                if (drawedCard == CardId.INVALID)
                {
                    fullHand = true;
                }
            }
        }
    }

    public bool TrySpawnUnitFromCard(string requestingPlayerId, CardId aUnitCardId, Vector2Int aCoord)
    {
        // Request arrived from client

        if (myMatchState != MatchState.PLAYER_TURN || myCurrentPlayerReference == null || requestingPlayerId != myCurrentPlayerReference.GetSessionId())
        {
            return false;
        }

        int cardCost = GetCardCost(aUnitCardId);

        if (cardCost == -1)
        {
            return false;
        }

        if (!myCurrentPlayerReference.IsCardInHand(aUnitCardId))
        {
            return false;
        }

        SharedTile tile = myBoardReference.GetTile(aCoord);
        List<SharedTile> validSpawnTiles = GetValidSpawnTiles();

        if (tile == null || !validSpawnTiles.Contains(tile))
        {
            return false;
        }

        SharedUnit spawnedUnit = DoSpawnUnitFromCard(aUnitCardId, aCoord);

        if (spawnedUnit == null)
        {
            Shared.LogError("[HOOD][SERVER][NETWORK] - spawnedUnit is null at TrySpawnUnitFromCard");
            return false;
        }

        return true;
        // The message is sent from NetworkServer
    }

    // NOTE: There are two approaches, either the unit moves itself, or an outside logic moves the unit, either can be fine, this is the second one
    // if the administration of the unit before/during/after the movement becomes too complex consider changing this to the "the unit moves itself" paradigm
    public bool TryMoveUnit(string requestingPlayerId, int aUnitId, Vector2Int aDestinationTileCoord)
    {
        if (myMatchState != MatchState.PLAYER_TURN || myCurrentPlayerReference == null || requestingPlayerId != myCurrentPlayerReference.GetSessionId())
        {
            return false;
        }

        SharedUnit unit = GetUnit(aUnitId);
        SharedTile tile = myBoardReference.GetTile(aDestinationTileCoord);

        if (unit == null || tile == null || !unit.GetIsEnabled() || unit.GetHasMoved())
        {
            return false;
        }

        List<SharedTile> validMovementTiles = GetValidMovementRanges(unit);

        if (!validMovementTiles.Contains(tile))
        {
            return false;
        }

        SharedUnit movedUnit = DoMoveUnit(aUnitId, aDestinationTileCoord);

        if (movedUnit == null)
        {
            return false;
        }

        return true;
    }

    public bool TryAttackUnit(string requestingPlayerId, int aUnitId, Vector2Int anAttackedTileCoord)
    {
        if (myMatchState != MatchState.PLAYER_TURN || myCurrentPlayerReference == null || requestingPlayerId != myCurrentPlayerReference.GetSessionId())
        {
            return false;
        }

        SharedUnit unit = GetUnit(aUnitId);
        SharedTile tile = myBoardReference.GetTile(anAttackedTileCoord);

        if (unit == null || !unit.GetIsEnabled() || tile == null || tile.GetUnit() == null)
        {
            return false;
        }

        List<SharedTile> validAttackTiles = GetValidAttackTiles(unit);

        if (!validAttackTiles.Contains(tile))
        {
            return false;
        }

        List<SharedUnit> affectedUnits = DoAttackUnit(aUnitId, anAttackedTileCoord).Result;

        if (affectedUnits == null)
        {
            return false;
        }

        foreach (SharedUnit aUnit in affectedUnits)
        {
            if (!aUnit.IsAlive())
            {
                KillUnit(aUnit);
            }
        }

        if (affectedUnits[0].IsMothership() || affectedUnits[1].IsMothership())
        {
            EvaluateWinCondition();

            if (myMatchWinner != MatchWinner.NO_WINNER)
            {
                ChangeState(MatchState.END);
            }
        }

        return true;
    }

    private void EvaluateWinCondition() // En server?. After Attack send evaluate message to server, server decides
    {
        myMatchWinner = MatchWinner.NO_WINNER;

        if (GetPlayer1().IsPlayersCapitainAlive() && !GetPlayer2().IsPlayersCapitainAlive())
        {
            myMatchWinner = MatchWinner.PLAYER_1;
        }
        else if (!GetPlayer1().IsPlayersCapitainAlive() && GetPlayer2().IsPlayersCapitainAlive())
        {
            myMatchWinner = MatchWinner.PLAYER_2;
        }
        else if (!GetPlayer1().IsPlayersCapitainAlive() && !GetPlayer2().IsPlayersCapitainAlive())
        {
            myMatchWinner = MatchWinner.DRAW;
        }
    }

    public bool TryCastUnitAbility(string requestingPlayerId, int aUnitId, Vector2Int aCastTileTargetCoord)
    {
        if (myMatchState != MatchState.PLAYER_TURN || myCurrentPlayerReference == null || requestingPlayerId != myCurrentPlayerReference.GetSessionId())
        {
            return false;
        }

        SharedUnit unit = GetUnit(aUnitId);
        SharedTile tile = myBoardReference.GetTile(aCastTileTargetCoord);

        if (unit == null || !unit.GetIsEnabled() || !unit.CanCastAbility() || tile == null)
        {
            return false;
        }

        List<SharedTile> validCastingTiles = GetValidUnitCastingTiles(unit);

        if (!validCastingTiles.Contains(tile))
        {
            return false;
        }

        List<SharedTile> castTiles = DoCastUnitAbility(aUnitId, aCastTileTargetCoord).Result;

        if (castTiles == null)
        {
            return false;
        }

        foreach (SharedTile castTile in castTiles)
        {
            SharedUnit affectedUnit = castTile.GetUnit();

            if (affectedUnit != null)
            {
                if (!affectedUnit.IsAlive())
                {
                    KillUnit(affectedUnit);
                }
            }
        }

        return true;
    }

    //public bool TryUseTechnologyCard(string requestingPlayerId, CardId aTechCardId, Vector2Int aTileCoord)
    //{
    //    // Request arrived from client

    //    if (myMatchState != MatchState.PLAYER_TURN || myCurrentPlayerReference == null || requestingPlayerId != myCurrentPlayerReference.GetSessionId())
    //    {
    //        return false;
    //    }

    //    if (!myCurrentPlayerReference.IsCardInHand(aTechCardId))
    //    {
    //        return false;
    //    }

    //    int cardCost = GetCardCost(aTechCardId);

    //    if (cardCost == -1)
    //    {
    //        return false;
    //    }

    //    SharedTile tile = myBoardReference.GetTile(aTileCoord);

    //    if (!myBoardReference.GetAllTiles().Contains(tile))
    //    {
    //        return false;
    //    }

    //    List<SharedTile> tiles = DoUseTechnologyCard(aTechCardId, aTileCoord).Result;

    //    if (tiles == null)
    //        return false;

    //    EvaluateWinCondition();

    //    if (myMatchWinner != MatchWinner.NO_WINNER)
    //    {
    //        ChangeState(MatchState.END);
    //    }

    //    return true;
    //    // The message is sent from NetworkServer
    //}

    public void EndTurn()
    {
        foreach (SharedPlayer player in myPlayersReferenceList)
        {
            if (!player.Equals(myCurrentPlayerReference))
            {
                ChangeState(MatchState.PLAYER_TURN, player.GetSessionId());
                return;
            }
        }
    }

    public void Surrender()
    {
        myCurrentPlayerReference.GetMotherShip().KillUnit();
        EvaluateWinCondition();
        if (myMatchWinner != MatchWinner.NO_WINNER)
            ChangeState(MatchState.END);
    }

    public void TestKillMyMothership()
    {
        myCurrentPlayerReference.GetMotherShip().KillUnit();
        EvaluateWinCondition();
        if (myMatchWinner != MatchWinner.NO_WINNER)
            ChangeState(MatchState.END);
    }
}

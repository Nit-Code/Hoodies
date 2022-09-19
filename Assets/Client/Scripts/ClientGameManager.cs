using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharedScripts;
using SharedScripts.DataId;
using System.Threading.Tasks;
using Assets.Shared.Scripts.Messages.Client;
using System.Linq;

public class ClientGameManager : SharedGameManager
{
    private SharedPlayer myLocalPlayer;
    public SharedPlayer GetLocalPlayer() { return myLocalPlayer; }

    private SharedUnit myCurrentlySelectedUnit;
    public SharedUnit GetSelectedUnit() { return myCurrentlySelectedUnit; }

    private List<SharedTile> myCurrentValidSpawnTiles;
    public List<SharedTile> GetCurrentValidSpawnTiles() { return myCurrentValidSpawnTiles; }
    private List<SharedTile> myCurrentValidMovementTiles;
    public List<SharedTile> GetCurrentValidMovementTiles() { return myCurrentValidMovementTiles; }
    private List<SharedTile> myCurrentValidAttackTiles;
    public List<SharedTile> GetCurrentValidAttackTiles() { return myCurrentValidAttackTiles; }
    private List<SharedTile> myCurrentValidCastingTiles;
    public List<SharedTile> GetPossibleCastingTiles() { return myCurrentValidCastingTiles; }
    private List<SharedTile> myCurrentCastingArea;
    public List<SharedTile> GetCurrentCastingArea() { return myCurrentCastingArea; }

    private bool myIsCastingUnitAbility;
    public bool GetIsCastingUnitAbility() { return myIsCastingUnitAbility; }

    // reference
    private NetworkClient myNetworkClientReference;
    private MatchSceneUIManager myMatchSceneUIManagerReference;

    public ClientGameManager(string aPlayer1Id)
    {
        // These assignations only ocurr when you drag the script to the scene game object, they only make sense for instanciated objects
        mySpawnedUnitDictionary = new Dictionary<int, SharedUnit>();
        myIsCastingUnitAbility = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
            TestEndGame();
    }

    private void TestEndGame()
    {
        ClientTestFeaturesMessage ctfm = new ClientTestFeaturesMessage(TestFeaturesMessageId.KILL_MY_MOTHERSHIP, myCurrentPlayerReference.GetSessionId());
        myNetworkClientReference.SendMessageToServer(ctfm);        
    }

    private new void Awake()
    {
        base.Awake();
        myCurrentValidSpawnTiles = new();
        myCurrentValidMovementTiles = new();
        myCurrentValidAttackTiles = new();
        myCurrentValidCastingTiles = new();
        myCurrentCastingArea = new();
        myIsCastingUnitAbility = false;
    }

    private new void Start()
    {
        base.Start();
        myMatchSceneUIManagerReference = FindObjectOfType<MatchSceneUIManager>();
        myNetworkClientReference = FindObjectOfType<NetworkClient>();
    }

    public bool IsLocalPlayerTurn()
    {
        return myLocalPlayer.Equals(myCurrentPlayerReference);
    }

    public void AddLocalPlayer(string aSessionId)
    {
        SharedPlayer localPlayer = new SharedPlayer(aSessionId, null);
        myLocalPlayer = localPlayer;
        myPlayersReferenceList.Add(localPlayer);
    }

    public void SetValidSpawnTiles(List<SharedTile> validTiles)
    {
        myCurrentValidSpawnTiles = validTiles;
    }

    public void SetValidMovementTiles(List<SharedTile> validTiles)
    {
        myCurrentValidMovementTiles = validTiles;
    }

    public void SetValidAttackTiles(List<SharedTile> validTiles)
    {
        myCurrentValidAttackTiles = validTiles;
    }

    public void SetValidCastingTiles(List<SharedTile> validTiles)
    {
        myCurrentValidCastingTiles = validTiles;
    }

    public void SetCurrentCastingArea(List<SharedTile> aCastingArea)
    {
        myCurrentCastingArea = aCastingArea;
    }

    public bool ValidCastingTilesContains(SharedTile aTile)
    {
        return myCurrentValidCastingTiles.Contains(aTile);
    }

    public bool SelectedUnitCanMoveToTile(SharedTile aTile)
    {
        if (myCurrentlySelectedUnit == null)
        {
            return false;
        }

        return myCurrentlySelectedUnit.GetIsEnabled() && myCurrentValidMovementTiles.Contains(aTile);
    }

    public bool SelectedUnitCanAttackOnTile(SharedTile aTile)
    {
        if(myCurrentlySelectedUnit == null)
        {
            return false;
        }

        return myCurrentlySelectedUnit.GetIsEnabled() && myCurrentValidAttackTiles.Contains(aTile);
    }

    public SharedAbility GetSelectedUnitAbility()
    {
        return myCurrentlySelectedUnit.GetAbility();
    }

    public void SetSelectedUnit(SharedUnit aUnit)
    {
        myCurrentlySelectedUnit = aUnit;
    }

    protected override void MatchStartup(SharedBoard.GenerationInfo aBoardInfo, List<ServerGameManager.ReadyPlayerInfo> playersInfo) // Map size if it's set on server
    {
        base.MatchStartup(aBoardInfo, playersInfo); // Generates board

        foreach (ServerGameManager.ReadyPlayerInfo playerInfo in playersInfo)
        {
            if (playerInfo.myPlayerSessionId != myLocalPlayer.GetSessionId()) // EnemyPlayer
            {
                SharedPlayer opponentPlayer = new SharedPlayer(playerInfo.myPlayerSessionId, null);
                myPlayersReferenceList.Add(opponentPlayer);
                DoSpawnMothership(playerInfo.myMothershipId, myBoardReference.GetTile(playerInfo.myMothershipCoord), opponentPlayer);
            }
            else
            {
                SharedDeck deck = GetShuffledDeck(playerInfo.myDeckId, playerInfo.myDeckSeed); // LocalPlayer
                myLocalPlayer.SetDeck(deck);
                DrawStartingHand();
                DoSpawnMothership(playerInfo.myMothershipId, myBoardReference.GetTile(playerInfo.myMothershipCoord), myLocalPlayer);
                myLocalPlayer.FillEnergy();
            }
        }

        ClientPlayerReadyStatusMessage readyAndLoadedMessage = new ClientPlayerReadyStatusMessage(ReadyStatusMessageId.PLAYER_READY_AND_LOADED, myLocalPlayer.GetSessionId(), true);
        myNetworkClientReference.SendMessageToServer(readyAndLoadedMessage);
    }

    private void DrawStartingHand()
    {
        bool fullHand = false;
        while (!fullHand)
        {
            CardId drawedCard = myLocalPlayer.TryDrawCard(); // Do draw card returns INVALID if your hand is full or if your deck is empty.
            if (drawedCard == CardId.INVALID)
            {
                fullHand = true;
            }
            else
            {
                myGameFactoryReference.CreateMatchCard(drawedCard, myMatchSceneUIManagerReference.GetCardGrid().transform);
            }
        }
        Shared.Log("Drawed starting hand");
    }

    protected override void NewPlayerTurn(string aPlayerSessionId)
    {
        base.NewPlayerTurn(aPlayerSessionId);

        myMatchSceneUIManagerReference.StopTimer();
        myMatchSceneUIManagerReference.HideLoadingScreen(); // We put this here to avoid creating a "hide loading screen" message
        myMatchSceneUIManagerReference.StartTimer(90); //Hardcoded for now, could send this from server message later
        myMatchSceneUIManagerReference.UpdateEnergyNumber(myLocalPlayer.GetEnergy());

        if (!IsLocalPlayerTurn())
        {
            StartCoroutine(myMatchSceneUIManagerReference.ShowPlayerTurnMessage(TurnType.ENEMY));
            myMatchSceneUIManagerReference.DisableEndTurnButton();
            myMatchSceneUIManagerReference.LockInput();
        }
        else
        {
            StartCoroutine(myMatchSceneUIManagerReference.ShowPlayerTurnMessage(TurnType.PLAYER));
            myMatchSceneUIManagerReference.EnableEndTurnButton();
            myMatchSceneUIManagerReference.UnlockInput();
        }

        ResetPlayerSelections();
    }

    protected override void MatchEnd(string aWinner)
    {
        if (myMatchSceneUIManagerReference == null)
        {
            return;
        }

        StartCoroutine(TransitionToMatchEndUI(aWinner));
    }

    private IEnumerator TransitionToMatchEndUI(string aWinner)
    {
        myMatchSceneUIManagerReference.HideMatchUI();
        ResetPlayerSelections();
        myMatchSceneUIManagerReference.EnableMatchEndClickBlocker();

        yield return new WaitForSeconds(2f);

        SharedPlayer player = GetPlayerFromPlayerSessionId(aWinner);

        if(player != null) // Somebody won
        {
            if (aWinner == myLocalPlayer.GetSessionId()) // If I won
            {
                myMatchSceneUIManagerReference.ShowMatchEndPanel(MatchStateMessageId.END_WINNER, aWinner, new Color32(170, 232, 255, 255));
            }
            else // If I lost
            {
                myMatchSceneUIManagerReference.ShowMatchEndPanel(MatchStateMessageId.END_WINNER, aWinner, Color.red);
            }
            yield return null;
        }
        else
        {
            myMatchSceneUIManagerReference.ShowMatchEndPanel(MatchStateMessageId.END_DRAW, "", Color.yellow);
        }
        yield return null;
    }

    public bool TryRequestSpawnUnitFromCard(MatchCard aCard, SharedTile aTile)
    {
        if (myMatchState != MatchState.PLAYER_TURN)
        {
            return false;
        }

        if (!IsLocalPlayerTurn() || !myLocalPlayer.IsCardInHand(aCard.GetId()) || !myCurrentValidSpawnTiles.Contains(aTile))
        {
            //Error sound
            return false;
        }

        if (!myLocalPlayer.CanSubstractEnergyCost(aCard.GetCost()))
        {
            CantAffordCard("Not enough energy.");
            //Error sound
            return false;
        }

        int cardIdAsInt = (int)aCard.GetId();

        ClientGameplayMessage sharedGameplayClientMessage = new ClientGameplayMessage(GameplayMessageIdClient.REQUEST_SPAWN_UNIT, myNetworkClientReference.GetPlayerSession(), cardIdAsInt, aTile.GetCoordinate().x, aTile.GetCoordinate().y);

        myNetworkClientReference.SendMessageToServer(sharedGameplayClientMessage);
        ResetPlayerSelections();
        return true;
    }

    public void CantAffordCard(string aReason)
    {
        // Notify player
    }

    public void OnServerFailedToSpawnCard(CardId aUnitCardId) // We do this because client destroys the MatchCard GameObject when TryRequest succeeds.
    {
        myLocalPlayer.AddCardToHand(aUnitCardId);
        myGameFactoryReference.CreateMatchCard(aUnitCardId, myMatchSceneUIManagerReference.GetCardGrid().transform);
    }


    //TODO: call this on server successful response
    public override SharedUnit DoSpawnUnitFromCard(CardId aUnitCardId, Vector2Int aCoord)
    {
        SharedUnit unit = base.DoSpawnUnitFromCard(aUnitCardId, aCoord);

        if (IsLocalPlayerTurn()) //Players don't know each others cards
        {
            myLocalPlayer.RemoveCardFromHand(aUnitCardId);
            myMatchSceneUIManagerReference.UpdateEnergyNumber(myLocalPlayer.GetEnergy());
        }
        else
        {
            unit.SetKindText("Enemy");
        }

        if (unit != null)
        {
            StartCoroutine(ColorGrayAfterSpawn(unit));
        }

        ResetPlayerSelections();
        return unit;
    }

    public bool TryRequestUseTechnologyCard(MatchCard aCard, SharedTile aTile)
    {
        if (myMatchState != MatchState.PLAYER_TURN)
        {
            return false;
        }

        if (!IsLocalPlayerTurn())
        {
            //Error sound
            return false;
        }

        if (!myLocalPlayer.CanSubstractEnergyCost(aCard.GetCost()))
        {
            CantAffordCard("Not enough energy.");
            //Error sound
            return false;
        }

        int cardIdAsInt = (int)aCard.GetId();

        ClientGameplayMessage sharedGameplayClientMessage = new ClientGameplayMessage(GameplayMessageIdClient.REQUEST_USE_TECHNOLOGY, myNetworkClientReference.GetPlayerSession(), cardIdAsInt, aTile.GetCoordinate().x, aTile.GetCoordinate().y);

        myNetworkClientReference.SendMessageToServer(sharedGameplayClientMessage);
        ResetPlayerSelections();
        return true;
    }

    public override List<SharedTile> DoUseTechnologyCard(CardId aTechnologyCardId, Vector2Int aCoord)
    {
        myMatchSceneUIManagerReference.UpdateEnergyNumber(myLocalPlayer.GetEnergy());
        return new List<SharedTile>();
    }

    public bool TryRequestMoveUnit(SharedTile aTile)
    {
        if (myMatchState != MatchState.PLAYER_TURN)
        {
            return false;
        }

        SharedUnit selectedUnit = myCurrentlySelectedUnit;

        if (selectedUnit == null || !selectedUnit.IsOwnedByPlayer(myLocalPlayer))
        {
            return false;
        }

        if (!IsLocalPlayerTurn() || !selectedUnit.GetIsEnabled() || selectedUnit.GetHasMoved() || !myCurrentValidMovementTiles.Contains(aTile)) // You shouldn't be able to select a unit if it's not enabled, or click the button to move. But just in case.
        {
            //Error sound
            return false;
        }

        ClientGameplayMessage sharedGameplayClientMessage = new ClientGameplayMessage(GameplayMessageIdClient.REQUEST_MOVE_UNIT, myNetworkClientReference.GetPlayerSession(), selectedUnit.GetMatchId(), aTile.GetCoordinate().x, aTile.GetCoordinate().y);

        myNetworkClientReference.SendMessageToServer(sharedGameplayClientMessage);
        ResetPlayerSelections();
        return true;
    }

    public override SharedUnit DoMoveUnit(int aUnitInGameId, Vector2Int aCoord)
    {
        SharedUnit unit = base.DoMoveUnit(aUnitInGameId, aCoord);
        SharedTile tile = myBoardReference.GetTile(aCoord);

        Vector3 newUnitPosition = tile.transform.position;
        newUnitPosition.y += 20;
        unit.transform.position = newUnitPosition;

        ResetPlayerSelections();

        if (unit.GetIsEnabled())
        {
            myBoardReference.ColorAttackRange(unit);
        }
        else
        {
            unit.ColorGray();
        }

        return unit;
    }

    public bool TryRequestAttackUnit(SharedTile anAttackedTile)
    {
        if (myMatchState != MatchState.PLAYER_TURN)
        {
            return false;
        }

        SharedUnit attackingUnit = myCurrentlySelectedUnit;

        if (attackingUnit == null || !attackingUnit.IsOwnedByPlayer(myLocalPlayer))
        {
            return false;
        }

        if (!IsLocalPlayerTurn() || !attackingUnit.GetIsEnabled() || !myCurrentValidAttackTiles.Contains(anAttackedTile)) // You shouldn't be able to select a unit if it's not enabled, or click the button to attack. But just in case.
        {
            //Error sound
            return false;
        }

        ClientGameplayMessage sharedGameplayClientMessage = new ClientGameplayMessage(GameplayMessageIdClient.REQUEST_ATTACK_UNIT, myNetworkClientReference.GetPlayerSession(), attackingUnit.GetMatchId(), anAttackedTile.GetCoordinate().x, anAttackedTile.GetCoordinate().y);

        myNetworkClientReference.SendMessageToServer(sharedGameplayClientMessage);
        ResetPlayerSelections();
        return true;
    }

    public override async Task<List<SharedUnit>> DoAttackUnit(int aUnitId, Vector2Int aCoord)
    {
        List<SharedUnit> affectedUnits = await base.DoAttackUnit(aUnitId, aCoord);

        await affectedUnits[0].PerformAnimation(SharedUnit.AttackAnimation);
        //await Task.Delay(500).ConfigureAwait(false);
        await affectedUnits[1].PerformAnimation(SharedUnit.HurtAnimation);

        //await Task.WhenAll(affectedUnits[0].PerformAnimation(SharedUnit.AttackAnimation), affectedUnits[1].PerformAnimation(SharedUnit.HurtAnimation));

        foreach (SharedUnit aUnit in affectedUnits)
        {
            if (!aUnit.IsAlive())
            {
                await KillUnit(aUnit);
            }
        }

        if (!affectedUnits[0].GetIsEnabled())
        {
            affectedUnits[0].ColorGray();
        }

        if (affectedUnits[0].IsMothership() || affectedUnits[1].IsMothership())
        {
            // update UI
        }

        ResetPlayerSelections();
        return affectedUnits;
    }

    public bool TryRequestCastUnitAbility(SharedTile aCastTileTarget)
    {
        if (myMatchState != MatchState.PLAYER_TURN)
        {
            return false;
        }

        SharedUnit castingUnit = myCurrentlySelectedUnit;

        if (castingUnit == null || !castingUnit.IsOwnedByPlayer(myLocalPlayer))
        {
            return false;
        }

        if (!IsLocalPlayerTurn() || !castingUnit.GetIsEnabled() || !castingUnit.HasAbility() || !myCurrentValidCastingTiles.Contains(aCastTileTarget))
        {
            //Error sound
            return false;
        }

        if (!castingUnit.CanAffordAbility())
        {
            //Error message
            return false;
        }

        ClientGameplayMessage sharedGameplayClientMessage = new ClientGameplayMessage(GameplayMessageIdClient.REQUEST_USE_ABILITY, myNetworkClientReference.GetPlayerSession(), castingUnit.GetMatchId(), aCastTileTarget.GetCoordinate().x, aCastTileTarget.GetCoordinate().y);

        myNetworkClientReference.SendMessageToServer(sharedGameplayClientMessage);
        ResetPlayerSelections();
        return true;
    }

    public override async Task<List<SharedTile>> DoCastUnitAbility(int aUnitId, Vector2Int aCoord)
    {
        List<SharedTile> castTiles = await base.DoCastUnitAbility(aUnitId, aCoord);

        if (castTiles == null)
        {
            return null;
        }

        myMatchSceneUIManagerReference.UpdateEnergyNumber(myLocalPlayer.GetEnergy());

        foreach (SharedTile tile in castTiles)
        {
            SharedUnit affectedUnit = tile.GetUnit();

            if (affectedUnit != null)
            {
                if (!affectedUnit.IsAlive())
                {
                    await KillUnit(affectedUnit);
                }
            }
        }

        return castTiles;
    }

    public void EndTurn()
    {
        if (!IsLocalPlayerTurn())
        {
            return;
        }
        ClientPlayerGameplayMessage endTurnMessage = new ClientPlayerGameplayMessage(PlayerGameplayMessageIdClient.END_MY_TURN, myLocalPlayer.GetSessionId());
        myNetworkClientReference.SendMessageToServer(endTurnMessage);
    }

    public override CardId DoDrawCard()
    {
        if (!IsLocalPlayerTurn()) //Players don't know eachother's cards
        {
            return CardId.INVALID;
        }

        CardId cardId = base.DoDrawCard();

        if (cardId == CardId.INVALID)
        {
            return CardId.INVALID;
        }

        myGameFactoryReference.CreateMatchCard(cardId, myMatchSceneUIManagerReference.GetCardGrid().transform);
        return cardId;
    }

    public override async Task KillUnit(SharedUnit aUnit)
    {
        SharedTile tile = myBoardReference.GetTile(aUnit.GetPosition().x, aUnit.GetPosition().y);
        tile.RemoveUnit();

        await aUnit.PerformAnimation(SharedUnit.DeathAnimation);

        aUnit.KillUnit();

        float end = Time.time + 1.5f; // Waits for a bit in case two or more units die from the same action
        while (Time.time < end)
        {
            await Task.Yield();
        }
    }

    public void UpdateUnitStatusEffects() // Call at the begining of each round (same player turn)
    {
        //TODO: implement me
    }

    public void ChangeCastingUnitAbilityStatus()
    {
        if (myMatchState != MatchState.PLAYER_TURN)
        {
            return;
        }

        if (!IsLocalPlayerTurn() || !myCurrentlySelectedUnit.GetIsEnabled() || !myCurrentlySelectedUnit.HasAbility())
        {
            return;
        }

        myIsCastingUnitAbility = !myIsCastingUnitAbility;
        myMatchSceneUIManagerReference.UpdateAbilityButtonStatus(myCurrentlySelectedUnit);
    }

    public void SetIsCastingTechnologyStatus(bool aStatus)
    {
        if (myMatchState != MatchState.PLAYER_TURN)
        {
            return;
        }

        if (!IsLocalPlayerTurn())
        {
            return;
        }

        if(aStatus)
        {
            myIsCastingUnitAbility = false;
        }
    }

    private IEnumerator ColorGrayAfterSpawn(SharedUnit aUnit)
    {
        yield return new WaitForSeconds(aUnit.GetAnimationLength(SharedUnit.SummonAnimation));
        aUnit.ColorGray();

        yield return null;
    }

    public override List<SharedTile> GetValidSpawnTiles()
    {
        if (IsLocalPlayerTurn())
        {
            myCurrentValidSpawnTiles = base.GetValidSpawnTiles();
        }
        return myCurrentValidSpawnTiles;
    }

    public override List<SharedTile> GetValidAttackTiles(SharedUnit anAttackingUnit) // This one returns just the tiles where an attack is possible
    {
        if (anAttackingUnit.IsOwnedByPlayer(myLocalPlayer))
        {
            myCurrentValidAttackTiles = base.GetValidAttackTiles(anAttackingUnit);
        }
        return myCurrentValidAttackTiles;
    }

    public override List<SharedTile> GetValidMovementRanges(SharedUnit unit)
    {
        if (unit.IsOwnedByPlayer(myLocalPlayer))
        {
            myCurrentValidMovementTiles = base.GetValidMovementRanges(unit);
        }
        return myCurrentValidMovementTiles;
    }

    public override List<SharedTile> GetValidUnitCastingTiles(SharedUnit unit)
    {
        if (unit.IsOwnedByPlayer(myLocalPlayer))
        {
            myCurrentValidCastingTiles = base.GetValidUnitCastingTiles(unit);
        }
        return myCurrentValidCastingTiles;
    }

    public void ResetPlayerSelections()
    {
        myBoardReference.UndoBoardSelectionColors();
        myCurrentValidSpawnTiles.Clear();
        myCurrentValidAttackTiles.Clear();
        myCurrentValidMovementTiles.Clear();
        myCurrentValidCastingTiles.Clear();
        myCurrentCastingArea.Clear();
        myIsCastingUnitAbility = false;
        myCurrentlySelectedUnit = null;
        myMatchSceneUIManagerReference.UpdateAbilityButtonStatus(null);
        myMatchSceneUIManagerReference.HideActionPrompt();
    }

    public void LeaveOngoingMatch()
    {
        //TODO
        //myMatchWinner = 
        // InitializeNewState(MatchState.END);
    }
}

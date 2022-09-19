using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharedScripts;
using SharedScripts.DataId;
using System.Linq;
using System;
using System.Threading.Tasks;

/*
    This script should exist on the same game object as SharedGameObjectFactory
*/

public class SharedGameManager : MonoBehaviour
{
    // gameplay data, solo id's, arrays, nada de prefabs 
    // game state data, game phase, turn, timers
    protected bool myTimerLockedIn = false;

    private int myPlayersMaximumEnergy = 10; //Should go on rules class, probably. For now, we put it here.
    private int myPlayersMaximumSlots = 5; //Should go on rules class, probably. For now, we put it here

    protected MatchState myMatchState;
    protected MatchWinner myMatchWinner;
    public void SetMatchWinner(MatchWinner aMatchWinner) { myMatchWinner = aMatchWinner; }
    public MatchState GetMatchState() { return myMatchState; }

    protected List<SharedPlayer> myPlayersReferenceList; //TODO: is this repeated on ClientGameManager?
    public SharedPlayer GetPlayer1() { return myPlayersReferenceList[0]; }
    public SharedPlayer GetPlayer2() { return myPlayersReferenceList[1]; }
    protected SharedPlayer myCurrentPlayerReference; //TODO: Could just be an index on myPlayersReferenceList?

    protected SharedBoard myBoardReference;
    protected SharedGameObjectFactory myGameFactoryReference;
    protected SharedDataLoader myDataLoaderReference;

    protected int myUnitIndex;
    protected Dictionary<int, SharedUnit> mySpawnedUnitDictionary;

    private int myToLoad;
    private int myLoaded;

    protected void Awake()
    {
        myToLoad = 3;
        myLoaded = 0;

        myUnitIndex = 0;
        myMatchWinner = MatchWinner.NO_WINNER;
        myPlayersReferenceList = new List<SharedPlayer>();
        mySpawnedUnitDictionary = new Dictionary<int, SharedUnit>();
    }

    private bool MyHasLoaded()
    {
        return myToLoad == myLoaded;
    }

    protected void Start()
    {
        //TODO: add safety checks for this lookup
        myBoardReference = FindObjectOfType<SharedBoard>();
        if (myBoardReference != null)
        {
            myLoaded++;
        }

        myGameFactoryReference = FindObjectOfType<SharedGameObjectFactory>();
        if (myGameFactoryReference != null)
        {
            myLoaded++;
        }

        myDataLoaderReference = FindObjectOfType<SharedDataLoader>();
        if (myDataLoaderReference != null)
        {
            myLoaded++;
        }
    }

    public void InitializeNewState(MatchState aNewState, string aPlayerSessionId = null, SharedBoard.GenerationInfo aBoardInfo = new(), List<ServerGameManager.ReadyPlayerInfo> playersInfo = null)
    {
        myMatchState = aNewState;

        switch (aNewState)
        {
            case MatchState.SETUP:
                MatchStartup(aBoardInfo, playersInfo);
                break;

            case MatchState.PLAYER_TURN:
                NewPlayerTurn(aPlayerSessionId);
                break;

            case MatchState.END:
                MatchEnd(aPlayerSessionId); // 
                break;
        }
    }

    protected SharedPlayer GetPlayerFromPlayerSessionId(string aPlayerSessionId)
    {
        foreach (SharedPlayer sharedPlayer in myPlayersReferenceList)
        {
            if (sharedPlayer.GetSessionId() == aPlayerSessionId)
            {
                return sharedPlayer;
            }
        }

        Shared.LogError("[HOOD][GAMEMANAGER] - GetPlayerFromPlayerSessionId()");
        return null;
    }

    protected virtual void MatchStartup(SharedBoard.GenerationInfo aBoardInfo, List<ServerGameManager.ReadyPlayerInfo> playersInfo)
    {
        if (playersInfo == null)
        {
            Shared.LogError("[HOOD][GAMEMANAGER] - MatchStartup()");
            return;
        }

        myBoardReference.Init(aBoardInfo.seed, aBoardInfo.nebulaQty, aBoardInfo.blackHoleQty, aBoardInfo.width, aBoardInfo.height);

        // TODO: Override in server
    }

    protected SharedDeck GetShuffledDeck(int aDeckId, int aSeed)
    {
        SharedDeck deck = GetDeckFromId(aDeckId);

        if (deck == null)
        {
            return null;
        }

        List<CardId> cards = deck.GetCards();
        cards = ShuffleItemsWithSeed(cards.ToArray(), aSeed).ToList();
        //set deck? or is this enough?
        return deck;
    }

    private SharedDeck GetDeckFromId(int aDeckId) // TODO
    {
        List<CardId> cards = new() { 
            CardId.CARD_BASIC_UNIT_TEST, 
            CardId.CARD_FENIX_SPAWNER, 
            CardId.CARD_TANK_SPAWNER, 
            CardId.CARD_HUNTER_SPAWNER, 
            CardId.CARD_TRAVELER_SPAWNER, 
            CardId.CARD_ROCKET_SPAWNER };

        SharedDeck deck = new SharedDeck("testDeck", cards);
        deck.SetMothership(CardId.CARD_MOTHERSHIP_TEST);
        return deck;
    }

    protected virtual void NewPlayerTurn(string aPlayerSessionId)
    {
        SharedPlayer player = GetPlayerFromPlayerSessionId(aPlayerSessionId);

        if(player == null)
        {
            return;
        }

        myCurrentPlayerReference = player;

        foreach (KeyValuePair<int, SharedUnit> entry in mySpawnedUnitDictionary)
        {
            SharedUnit unit = entry.Value;

            if (!unit.IsAlive())
            {
                continue;
            }

            if (unit.GetPlayer().Equals(myCurrentPlayerReference))
            {
                unit.EnableUnit();
            }
            else
            {
                unit.DisableUnit();
            }
            unit.RefreshUnit(); // This refreshes ability/status effect timers and resets sprites
        }

        DoDrawCard(); // Only applies to current player
        myCurrentPlayerReference.IncreaseCurrentMaximumEnergy();
        myCurrentPlayerReference.FillEnergy();
        UseAllAliveUnitsAbilities(); //TODO: disabled until this feature is fully implemented
    }

    protected virtual void MatchEnd(string aWinner)
    {

    }

    protected SharedUnit GetUnit(int matchId)
    {
        if (mySpawnedUnitDictionary.TryGetValue(matchId, out SharedUnit unit))
        {
            return unit;
        }
        return null;
    }

    protected bool CanAffordCard(CardId anId, int anEnergyAmmount)
    {
        return GetCardCost(anId) <= anEnergyAmmount;
    }

    protected int GetCardCost(CardId aCardId)
    {
        if (!MyHasLoaded() || aCardId == CardId.INVALID)
        {
            Shared.LogError("[HOOD][GAMEMANAGER] - GetCardCost()");
            return -1;
        }

        CardData cardData = myDataLoaderReference.GetCardData(aCardId);
        return cardData.myCost;
    }

    public virtual SharedUnit DoSpawnUnitFromCard(CardId aUnitCardId, Vector2Int aCoord)
    {
        if (!MyHasLoaded() || myCurrentPlayerReference == null || myBoardReference == null)
        {
            Shared.LogError("[HOOD][GAMEMANAGER][ERROR] - DoSpawnUnitFromCard()");
            return null;
        }

        if (myMatchState != MatchState.PLAYER_TURN)
        {
            return null;
        }

        CardData cardData = myDataLoaderReference.GetCardData(aUnitCardId);
        if (cardData.myCardType != CardType.UNIT)
        {
            Shared.LogError("[HOOD][GAMEMANAGER][ERROR] - DoSpawnUnitFromCard()");
            return null;
        }

        SharedTile tile = myBoardReference.GetTile(aCoord);

        if(tile == null)
        {
            return null;
        }

        UnitCardData unitCardData = cardData as UnitCardData;

        Vector3 unitPosition = tile.transform.position;
        unitPosition.y += 20;

        SharedUnit spawnedUnit = myGameFactoryReference.CreateUnit(tile.transform, unitPosition, myCurrentPlayerReference, false, tile.GetCoordinate(), unitCardData.myUnitId, myUnitIndex);
        bool canAffordCard = myCurrentPlayerReference.TrySubstractEnergyCost(cardData.myCost);

        if (spawnedUnit == null || !canAffordCard)
        {
            Shared.LogError("[HOOD][GAMEMANAGER][ERROR] - DoSpawnUnitFromCard()");
            return null;
        }

        tile.SetUnit(spawnedUnit);
        spawnedUnit.DisableUnit();
        TryUseAbilityOnUnit(spawnedUnit);

        mySpawnedUnitDictionary.Add(spawnedUnit.GetMatchId(), spawnedUnit);
        myUnitIndex++;

        return spawnedUnit;
    }

    public virtual SharedUnit DoSpawnMothership(CardId aUnitCardId, SharedTile aTile, SharedPlayer anOwnerPlayer)
    {
        if (!MyHasLoaded() || aUnitCardId == CardId.INVALID || aTile == null || anOwnerPlayer == null)
        {
            Shared.LogError("[HOOD][GAMEMANAGER][ERROR] - DoSpawnMothership()");
            return null;
        }

        CardData cardData = myDataLoaderReference.GetCardData(aUnitCardId);
        if (cardData.myCardType != CardType.MOTHERSHIP)
        {
            Shared.LogError("[HOOD][GAMEMANAGER][ERROR] - DoSpawnMothership()");
            return null;
        }

        UnitCardData unitCardData = cardData as UnitCardData;

        Vector3 unitPosition = aTile.transform.position;
        unitPosition.y += 20;

        SharedUnit spawnedUnit = myGameFactoryReference.CreateUnit(aTile.transform, unitPosition, anOwnerPlayer, false, aTile.GetCoordinate(), unitCardData.myUnitId, myUnitIndex);

        if (spawnedUnit == null)
        {
            Shared.LogError("[HOOD][GAMEMANAGER][ERROR] - DoSpawnMothership()");
            return null;
        }

        aTile.SetUnit(spawnedUnit);

        spawnedUnit.SetIsMothership();
        anOwnerPlayer.SetMotherShip(spawnedUnit);
        mySpawnedUnitDictionary.Add(spawnedUnit.GetMatchId(), spawnedUnit);
        myUnitIndex++;

        return spawnedUnit;
    }

    public virtual SharedUnit ForceSpawnAuxiliaryUnit(SharedTile aTile)
    {
        if (!MyHasLoaded())
        {
            Shared.LogError("[HOOD][GAMEMANAGER][ERROR] - ForceSpawnAuxiliaryUnit()");
            return null;
        }

        CardData cardData = myDataLoaderReference.GetCardData(CardId.CARD_MOTHERSHIP_TEST);

        if (cardData.myCardType == CardType.INVALID)
        {
            Shared.LogError("[HOOD][GAMEMANAGER][ERROR] - ForceSpawnAuxiliaryUnit()");
            return null;
        }

        UnitCardData unitCardData = cardData as UnitCardData;

        Vector3 unitPosition = aTile.transform.position;
        unitPosition.y += 20;

        SharedUnit spawnedUnit = myGameFactoryReference.CreateUnit(aTile.transform, unitPosition, myCurrentPlayerReference, false, aTile.GetCoordinate(), unitCardData.myUnitId, myUnitIndex);

        if (spawnedUnit == null)
        {
            Shared.LogError("[HOOD][GAMEMANAGER][ERROR] - ForceSpawnAuxiliaryUnit()");
            return null;
        }

        aTile.SetUnit(spawnedUnit);
        return spawnedUnit;
    }

    public virtual List<SharedTile> DoUseTechnologyCard(CardId aTechnologyCardId, Vector2Int aCoord)
    {
        if (!MyHasLoaded() || myBoardReference == null)
        {
            Shared.LogError("[HOOD][GAMEMANAGER][ERROR] - DoUseTechnologyCard()");
            return null;
        }

        if (myMatchState != MatchState.PLAYER_TURN)
        {
            return null;
        }

        SharedTile tile = myBoardReference.GetTile(aCoord);
        CardData cardData = myDataLoaderReference.GetCardData(aTechnologyCardId);
        if (cardData.myCardType != CardType.TECHNOLOGY || tile == null)
        {
            Shared.LogError("[HOOD][GAMEMANAGER][ERROR] - DoUseTechnologyCard()");
            return null;
        }

        AbilityCardData abilityCardData = cardData as AbilityCardData;

        // Create technology prefab that is assigned an ability script. Fill that script with its data and use the abilityEffect


        //TODO
        return null;
    }

    public virtual SharedUnit DoMoveUnit(int aUnitId, Vector2Int aCoord)
    {
        if (!MyHasLoaded() || myBoardReference == null)
        {
            Shared.LogError("[HOOD][GAMEMANAGER][ERROR] - DoMoveUnit()");
            return null;
        }
        
        SharedTile tile = myBoardReference.GetTile(aCoord);
        SharedUnit unit = GetUnit(aUnitId);

        if (unit == null || tile == null)
        {
            Shared.LogError("[HOOD][GAMEMANAGER][ERROR] - DoMoveUnit()");
            return null;
        }

        SharedTile originTile = myBoardReference.GetTile(unit.GetPosition());
        originTile.RemoveUnit();
        tile.SetUnit(unit);
        unit.SetHasMoved();

        UpdateAllUnitsStatusEffectStatus();
        unit.CastAbilityEffect();
        TryUseAbilityOnUnit(unit);

        if (GetValidAttackTiles(unit).Count == 0 && !unit.CanCastAbility()) // I moved and I can't do anything else
        {
            unit.DisableUnit();
        }
        return unit;
    }

    public virtual Task<List<SharedUnit>> DoAttackUnit(int aUnitId, Vector2Int aCoord)
    {
        if (!MyHasLoaded() || myBoardReference == null)
        {
            Shared.LogError("[HOOD][GAMEMANAGER][ERROR] - DoAttackUnit()");
            return null;
        }

        if (myMatchState == MatchState.INVALID)
        {
            return null;
        }

        SharedTile tile = myBoardReference.GetTile(aCoord);

        if (tile == null)
        {
            Shared.LogError("[HOOD][GAMEMANAGER][ERROR] - DoAttackUnit()");
            return null;
        }

        SharedUnit attackingUnit = GetUnit(aUnitId);
        SharedUnit defendingUnit = tile.GetUnit();

        if (attackingUnit == null || defendingUnit == null)
        {
            Shared.LogError("[HOOD][GAMEMANAGER][ERROR] - DoAttackUnit()");
            return null;
        }

        List<SharedUnit> affectedUnits = new()
        {
            attackingUnit,
            defendingUnit
        };

        defendingUnit.ModifyShield(-attackingUnit.GetAttack());
        attackingUnit.ModifyShield(-defendingUnit.GetAttack());

        attackingUnit.DisableUnit();

        return Task.FromResult(affectedUnits);
    }

    public virtual Task<List<SharedTile>> DoCastUnitAbility(int aUnitId, Vector2Int aCoord)
    {
        if (!MyHasLoaded() || myBoardReference == null)
        {
            Shared.LogError("[HOOD][GAMEMANAGER][ERROR] - DoCastUnitAbility()");
            return null;
        }

        if (myMatchState != MatchState.PLAYER_TURN)
        {
            return null;
        }

        SharedTile tile = myBoardReference.GetTile(aCoord);
        SharedUnit castingUnit = GetUnit(aUnitId);

        if (castingUnit == null || tile == null || !castingUnit.CanCastAbility())
        {
            Shared.LogError("[HOOD][GAMEMANAGER][ERROR] - DoCastUnitAbility()");
            return null;
        }

        SharedAbility ability = castingUnit.GetAbility();
        List<SharedTile> castTiles = myBoardReference.GetShapeFromCenterTileCoord(ability.GetIncludesCenter(), ability.GetShape(), ability.GetShapeSize(), tile.GetCoordinate());

        //TODO check captains

        castingUnit.UseAbility(castTiles);
        return Task.FromResult(castTiles);
    }

    public virtual void DoUseTechnologyCard()
    {
        // TODO
    }

    public virtual CardId DoDrawCard()
    {
        if (myMatchState == MatchState.END)
        {
            return CardId.INVALID;
        }

        CardId cardId = myCurrentPlayerReference.TryDrawCard();

        if (cardId == CardId.INVALID)
        {
            return CardId.INVALID;
        }

        return cardId;
    }

    protected virtual Task UseAllAliveUnitsAbilities() // Async in clientGameManager if we end up adding animations
    {
        foreach (KeyValuePair<int, SharedUnit> entry in mySpawnedUnitDictionary)
        {
            SharedUnit unit = entry.Value;

            if (unit.IsAlive() && unit.IsCastingAbility())
            {
                unit.CastAbilityEffect();
            }
        }
        return Task.CompletedTask;
    }

    protected virtual Task TryUseAbilityOnUnit(SharedUnit spawnedUnit) // Async in clientGameManager if we end up adding animations
    {
        foreach (KeyValuePair<int, SharedUnit> entry in mySpawnedUnitDictionary)
        {
            SharedUnit otherUnit = entry.Value;

            if (otherUnit.HasAbility())
            {
                otherUnit.GetAbility().TryApplyEffectOnUnit(spawnedUnit);
            }
        }
        return Task.CompletedTask;
    }

    private void UpdateAllUnitsStatusEffectStatus()
    {
        foreach (KeyValuePair<int, SharedUnit> entry in mySpawnedUnitDictionary)
        {
            entry.Value.CheckStatusEffectsStatus();
        }
    }

    public virtual Task KillUnit(SharedUnit aUnit)
    {
        SharedTile tile = myBoardReference.GetTile(aUnit.GetPosition().x, aUnit.GetPosition().y);
        tile.RemoveUnit();
        aUnit.KillUnit();
        return Task.CompletedTask;
    }

    public void RemoveUnitFromDictionary(int aMatchId)
    {
        if (mySpawnedUnitDictionary.Remove(aMatchId))
        {
            myUnitIndex--;
        }
    }

    #region GetTileMethods

    public virtual List<SharedTile> GetValidSpawnTiles()
    {
        if (!MyHasLoaded())
        {
            Shared.LogError("[HOOD][GAMEMANAGER] - GetValidSpawnTiles()");
            return null;
        }

        List<SharedTile> spawnTiles = myBoardReference.GetShapeFromCenterTileCoord(false, AreaShape.SQUARE, 1, myCurrentPlayerReference.GetCaptainsPosition());

        for (int i = 0; i < spawnTiles.Count; i++)
        {
            SharedTile tile = spawnTiles[i];

            if (tile.GetUnit() != null || !tile.GetCanSpawn())
            {
                spawnTiles.RemoveAt(i);
            }
        }
        return spawnTiles;
    }

    public virtual List<SharedTile> GetValidAttackTiles(SharedUnit anAttackingUnit) // This one returns just the tiles where an attack is possible
    {
        if (!MyHasLoaded())
        {
            Shared.LogError("[HOOD][GAMEMANAGER] - GetPossibleAttackTiles()");
            return null;
        }

        int attackingUnitRange = anAttackingUnit.GetAttackRange();
        List<SharedTile> possibleAttackTiles = new();

        List<List<SharedTile>> attackingTilesListsByDirection = new();
        Vector2Int[] directionHelpers = { new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1) };

        for (int i = 0; i < 4; i++)
        {
            List<SharedTile> rawRangeTiles = myBoardReference.GetShapeFromCenterTileCoord(false, AreaShape.LINE, attackingUnitRange, anAttackingUnit.GetPosition(), anAttackingUnit.GetPosition() - directionHelpers[i]);
            attackingTilesListsByDirection.Add(rawRangeTiles);
        }

        foreach (List<SharedTile> tileList in attackingTilesListsByDirection)
        {
            int tilesToSearchCount = tileList.Count;

            for (int i = 0; i < tilesToSearchCount; i++)
            {
                if (tileList[i].GetUnit() != null && !tileList[i].GetUnit().IsOwnedByPlayer(myCurrentPlayerReference)) // Is this an enemy unit?
                {
                    possibleAttackTiles.Add(tileList[i]);
                    break;
                }

                if (tileList[i].GetIsBlocked())
                {
                    break; //You can't attack behind a blocking entity, so we break out and continue with the next direction.
                }

                if (tileList[i].GetTileType() == TileType.NEBULA)
                {
                    tilesToSearchCount--; // Range gets shortened by one aTile
                }
            }

        }

        return possibleAttackTiles;
    }

    public virtual List<SharedTile> GetValidMovementRanges(SharedUnit unit)
    {
        SharedUnit.MovementInfo movement = unit.GetMovementInfo();
        HashSet<SharedTile> possibleMovementTiles = new();
        Dictionary<Vector2Int, int> exploredAdjacentsWithCostDictionary = new Dictionary<Vector2Int, int>(); // Vector2Int => SharedTile | int => remaining moves when last explored
        int movementRange = movement.range;
        SharedTile unitTile = myBoardReference.GetTile(movement.pos);
        return GetValidMovementRangesRecursive(unitTile, unitTile, movementRange, possibleMovementTiles, exploredAdjacentsWithCostDictionary).ToList();
    }

    private HashSet<SharedTile> GetValidMovementRangesRecursive(SharedTile currentTile, SharedTile previousTile, int remainingMoves, HashSet<SharedTile> possibleMovementTiles, Dictionary<Vector2Int, int> exploredAdjacentsWithCostDictionary)
    {
        SharedTile.PathingInfo currentTileInfo = currentTile.GetPathingInfo();
        List<SharedTile> adjacentTiles = myBoardReference.GetAdjacentTiles(currentTileInfo.pos);
        adjacentTiles.Remove(previousTile);

        foreach (SharedTile adjacentTile in adjacentTiles)
        {
            SharedTile.PathingInfo adjacentTileInfo = adjacentTile.GetPathingInfo();

            if (!adjacentTileInfo.isBlocked)
            {
                int updatedRemainingMoves = remainingMoves - adjacentTileInfo.cost;

                if (updatedRemainingMoves > 0) // If I still have available moves
                {
                    if (exploredAdjacentsWithCostDictionary.TryGetValue(adjacentTileInfo.pos, out int previousRemainingMoves))
                    {
                        if (previousRemainingMoves < updatedRemainingMoves)
                        {
                            exploredAdjacentsWithCostDictionary[adjacentTileInfo.pos] = updatedRemainingMoves; //Update the dictionary value                  
                        }
                        else // If there is a value in the dictionary, but it has more remaining moves than this tile
                        {
                            continue;
                        }
                    }
                    possibleMovementTiles.Add(adjacentTile); // Hashsets don't add duplicates, so no need to check here.
                    exploredAdjacentsWithCostDictionary.TryAdd(adjacentTileInfo.pos, updatedRemainingMoves);

                    GetValidMovementRangesRecursive(adjacentTile, currentTile, updatedRemainingMoves, possibleMovementTiles, exploredAdjacentsWithCostDictionary);
                }
                else if (updatedRemainingMoves == 0)
                {
                    possibleMovementTiles.Add(adjacentTile);
                }
            }
        }
        return possibleMovementTiles;
    }

    public virtual List<SharedTile> GetValidUnitCastingTiles(SharedUnit unit)
    {
        List<SharedTile> possibleCastingTiles = myBoardReference.GetShapeFromCenterTileCoord(true, AreaShape.CROSS, unit.GetAbilityCastingRange(), unit.GetPosition());
        possibleCastingTiles.RemoveAll(tile => tile.GetTileType() == TileType.BLACKHOLE);

        return possibleCastingTiles;
    }

    #endregion

    public T[] ShuffleItemsWithSeed<T>(T[] anArray, int aSeed) // Hacer Utility script?
    {
        System.Random randomNumber = new System.Random(aSeed);

        for (int i = 0; i < anArray.Length - 1; i++)
        {
            int randomIndex = randomNumber.Next(i, anArray.Count());
            T tempItem = anArray[randomIndex];

            anArray[randomIndex] = anArray[i];
            anArray[i] = tempItem;
        }
        return anArray;
    }
}

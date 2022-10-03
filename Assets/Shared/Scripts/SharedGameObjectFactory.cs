using UnityEngine;
using SharedScripts;
using SharedScripts.DataId;
using System.Collections.Generic;

/*
    This script should probably not be attached to a game object on the persistent scene, as instanciating an object there to be used on 
    match/menu scene seems like a bad idea, or maybe not even possible.

    This script should exist on the same game object as SharedGameManager
*/

public class SharedGameObjectFactory : MonoBehaviour
{
    private SharedDataLoader myDataLoaderReference;
    private bool myHasLoader;

    private SharedBoard myBoardReference;
    private bool myHasBoard;

    private SharedGameManager myGameManagerReference;

    [SerializeField] private GameObject myEmptyTilePrefab;
    [SerializeField] private GameObject myBlackHoleTilePrefab;
    [SerializeField] private GameObject myNebulaTilePrefab;

    [SerializeField] private SharedCard myMyDecksCardPrefab;
    [SerializeField] private MatchCard myMatchCardPrefab;
    [SerializeField] private SharedUnit myUnitPrefab;

    private int myDataLoaded;
    private int myDataToLoad;

    private bool IsDataLoaded()
    {
        return myDataLoaded == myDataToLoad;
    }

    private void Awake()
    {
        myDataLoaded = 0;
        myDataToLoad = 7;

        if (myEmptyTilePrefab != null)
            myDataLoaded++;
        if (myBlackHoleTilePrefab != null)
            myDataLoaded++;
        if (myNebulaTilePrefab != null)
            myDataLoaded++;
        if (myMyDecksCardPrefab != null)
            myDataLoaded++;
        if (myMatchCardPrefab != null)
            myDataLoaded++;
        if (myUnitPrefab != null)
            myDataLoaded++;
    }

    private void Start()
    {
        myDataLoaderReference = FindObjectOfType<SharedDataLoader>();

        if (myDataLoaderReference != null)
        {
            myDataLoaded++;
            myHasLoader = true;
        }
        
#if UNITY_SERVER
        myGameManagerReference = FindObjectOfType<ServerGameManager>();
        if(myGameManagerReference != null)
        {
           // myDataLoaded++;
        }

        myBoardReference = FindObjectOfType<SharedBoard>();
        {
            //myDataLoaded++;
            myHasBoard = true;
        }
#endif
    }

    private void OnEnable()
    {
        EventHandler.OurAfterMatchSceneLoadEvent += LoadMatchSceneReferences;
    }

    private void OnDisable()
    {
        EventHandler.OurAfterMatchSceneLoadEvent -= LoadMatchSceneReferences;
    }

    private void LoadMatchSceneReferences()
    {
        if (myGameManagerReference == null)
        {
            myGameManagerReference = FindObjectOfType<SharedGameManager>(); //CHANGE TO CLIENTGAMEMANAGER

            /*if (myGameManagerReference != null)
            {
                myDataLoaded++;
            }*/
        }

        if (myBoardReference == null)
        {
            myBoardReference = FindObjectOfType<SharedBoard>();

            if (myBoardReference != null)
            {
                //myDataLoaded++;
                myHasBoard = true;
            }
        }
    }

    public SharedTile CreateTile(Transform aParent, Vector2Int aCoordinate, Vector3 aPosition, TileType aType) //UNUSED?
    {
        if (!IsDataLoaded() || aType == TileType.INVALID || !myHasBoard || !myHasLoader)
        {
            Shared.LogError("[HOOD][FACTORY] - CreateTile");
            return null;
        }

        //SharedTile tile = Instantiate(myTilePrefab, aPosition, Quaternion.identity, aParent);
        TileData data = myDataLoaderReference.GetTileData(aType);
        if (data != null)
        {
            //tile.Init(myBoardReference, myGameManagerReference, aCoordinate, data);
            // return tile;
            return null;
        }
        else
        {
            Shared.LogError("[HOOD][FACTORY] - CreateTile, no data from DataLoader");
            return null;
        }
    }

    public SharedTile CreateTileByType(Transform aParent, Vector2Int aCoordinate, Vector3 aPosition, TileType aType, int anIndex = 0)
    {
        if (!IsDataLoaded() || aType == TileType.INVALID || !myHasLoader)
        {
            Shared.LogError("[HOOD][FACTORY] - CreateTileByType");
            return null;
        }
        SharedTile tile = null;

        switch (aType)
        {
            case TileType.INVALID:
                break;
            case TileType.EMPTY:                 
                tile = Instantiate(myEmptyTilePrefab.GetComponent<SharedTile>(), aPosition, Quaternion.identity);
                break;
            case TileType.NEBULA:
                tile = Instantiate(myNebulaTilePrefab.GetComponent<SharedTile>(), aPosition, Quaternion.identity);
                break;
            case TileType.BLACKHOLE:
                tile = Instantiate(myBlackHoleTilePrefab.GetComponent<SharedTile>(), aPosition, Quaternion.identity);
                break;
        }

        if(aType != TileType.INVALID)
        {
            tile.transform.SetParent(aParent);
        }        

        TileData data = myDataLoaderReference.GetTileData(aType);
        if (data != null)
        {
            tile.Init(myBoardReference, aCoordinate, data);
            return tile;
        }

        Shared.LogError("[HOOD][FACTORY] - CreateTileByType, no data from DataLoader");
        return null;
    }

    public SharedUnit CreateUnit(Transform aParent, Vector3 aPosition, SharedPlayer anOwnerPlayer, bool aIsCapitain, Vector2Int aBoardPosition, UnitId aUnitId, CardId aCardId, int aMatchId)
    {
        if (!IsDataLoaded() || aUnitId == UnitId.INVALID || !myHasLoader)
        {
            Shared.LogError("[HOOD][FACTORY] - CreateUnit");
            return null;
        }

        SharedUnit unit = Instantiate(myUnitPrefab, aPosition, Quaternion.identity);
        unit.transform.SetParent(aParent);

        UnitData data = myDataLoaderReference.GetUnitData(aUnitId);
        if (data != null)
        {
            unit.Init
                (anOwnerPlayer, aIsCapitain, aBoardPosition, data, aMatchId, aCardId);
            return unit;
        }

        Shared.LogError("[HOOD][FACTORY] - CreateUnit, no data from DataLoader");
        return null;
    }

    //commented for future implementation
    //public SharedUnit CreateTechnologyAuxiliaryUnit(Transform aParent, Vector3 aPosition, SharedPlayer anOwnerPlayer, bool aIsCapitain, Vector2Int aBoardPosition, AbilityId anAbilityId)
    //{
    //    if (!IsDataLoaded() || !myHasLoader)
    //    {
    //        Shared.LogError("[HOOD][FACTORY] - CreateUnit");
    //        return null;
    //    }

    //    SharedUnit unit = Instantiate(myUnitPrefab, aPosition, Quaternion.identity);
    //    unit.transform.SetParent(aParent);

    //    UnitData unitData = myDataLoaderReference.GetUnitData(UnitId.UNIT_BASIC_UNIT_TEST);
    //    AbilityData abilityData = myDataLoaderReference.GetAbilityData(anAbilityId);
        
    //    if (unitData != null && abilityData != null)
    //    {
    //        unitData.myOverrideAnimatorController = abilityData.myOverrideAnimatorController;

    //        unit.Init
    //            (anOwnerPlayer, aIsCapitain, aBoardPosition, unitData, -1);

    //        return unit;
    //    }

    //    Shared.LogError("[HOOD][FACTORY] - CreateUnit, no data from DataLoader");
    //    return null;
    //}

    public SharedAbility AddAbilityComponent(GameObject aGameObject, AbilityId anId)
    {
        if (!IsDataLoaded() || !myHasLoader || aGameObject == null)
        {
            Shared.LogError("[HOOD][FACTORY] - AddAbilityComponent");
            return null;
        }

        AbilityData data = myDataLoaderReference.GetAbilityData(anId);

        if (data != null)
        {
            SharedAbility ability = null;

            switch (anId)
            {
                case AbilityId.PROTECTOR:
                    ability = aGameObject.AddComponent<Protector>();
                    ability.Init(aGameObject, data);
                    break;
                case AbilityId.ENGINE_OVERDRIVE:
                    ability = aGameObject.AddComponent<EngineOverdrive>();
                    ability.Init(aGameObject, data); 
                    break;
                case AbilityId.REPAIR_STATION:
                    ability = aGameObject.AddComponent<RepairStation>();
                    ability.Init(aGameObject, data); // We do this every time in case that some abilities need extra parameters
                    break;
                case AbilityId.KAMIKAZE:
                    ability = aGameObject.AddComponent<Kamikaze>();
                    ability.Init(aGameObject, data);
                    break;
                case AbilityId.INVALID:
                    break;
                default:
                    return null;
            }
            return ability;
        }

        Shared.LogError("[HOOD][FACTORY] - AddAbilityComponent, no data from DataLoader");
        return null;
    }

    public SharedStatusEffect AddStatusEffectComponent(SharedUnit aUnit, StatusEffectId anId)
    {
        if (!IsDataLoaded() || !myHasLoader)
        {
            Shared.LogError("[HOOD][FACTORY] - AddAbilityComponent");
            return null;
        }

        StatusEffectData data = myDataLoaderReference.GetStatusEffectData(anId);

        if (data != null)
        {
            SharedStatusEffect statusEffect;

            switch (anId)
            {
                case StatusEffectId.PROTECTOR_AURA:
                    statusEffect = aUnit.gameObject.AddComponent<StatusEffect_ProtectorAura>();
                    statusEffect.Init(aUnit, data);
                    break;
                case StatusEffectId.IMPROVED_ENGINES:
                    statusEffect = aUnit.gameObject.AddComponent<StatusEffect_ImprovedEngines>();
                    statusEffect.Init(aUnit, data); // We do this every time in case that some abilities need extra parameters
                    break;
                default:
                    return null;
            }
            return statusEffect;
        }

        Shared.LogError("[HOOD][FACTORY] - AddAbilityComponent, no data from DataLoader");
        return null;
    }

    //TODO: does this need a transform parent?
    public SharedCard CreateCard(CardId anId)
    {
        if (!IsDataLoaded() || anId == CardId.INVALID || !myHasLoader)
        {
            Shared.LogError("[HOOD][FACTORY] - CreateMyDecksCard");
            return null;
        }

        SharedCard card = Instantiate(myMyDecksCardPrefab);
        CardData cardData = myDataLoaderReference.GetCardData(anId);
        if (cardData != null)
        {
            if (cardData.myCardType == CardType.UNIT)
            {
                UnitCardData unitCardData = cardData as UnitCardData;
                UnitData unitData = myDataLoaderReference.GetUnitData(unitCardData.myUnitId);
                if (unitData != null)
                {
                    AbilityData abilityData = myDataLoaderReference.GetAbilityData(unitData.myAbilityId);

                    card.Init(unitCardData, unitData, abilityData);
                    return card;
                }
                else
                {
                    Shared.LogError("[HOOD][FACTORY] - CreateMyDecksCard, no unitData from DataLoader");
                    return null;
                }
            }
            else if (cardData.myCardType == CardType.TECHNOLOGY)
            {
                AbilityCardData abilityCardData = cardData as AbilityCardData;
                AbilityData abilityData = myDataLoaderReference.GetAbilityData(abilityCardData.myAbilityId);
                if (abilityData != null)
                {
                    card.Init(abilityCardData, abilityData);
                    return card;
                }
                else
                {
                    Shared.LogError("[HOOD][FACTORY] - CreateMyDecksCard, no abilityData from DataLoader");
                    return null;
                }
            }
        }
        Shared.LogError("[HOOD][FACTORY] - CreateMyDecksCard, no cardData from DataLoader");
        return null;
    }

    //TODO: does this need a transform parent?
    public MatchCard CreateMatchCard(CardId anId, Transform aParent)
    {
        if (!IsDataLoaded() || anId == CardId.INVALID || !myHasBoard || !myHasLoader)
        {
            Shared.LogError("[HOOD][FACTORY] - CreateMatchCard");
            return null;
        }

        MatchCard card = Instantiate(myMatchCardPrefab, aParent.transform);

        CardData cardData = myDataLoaderReference.GetCardData(anId);
        if (cardData != null)
        {
            if (cardData.myCardType == CardType.UNIT)
            {
                UnitCardData unitCardData = cardData as UnitCardData;
                UnitData unitData = myDataLoaderReference.GetUnitData(unitCardData.myUnitId);
                if (unitData != null)
                {
                    AbilityData abilityData = null;
                    if (unitData.myAbilityId != AbilityId.INVALID)
                    {
                        abilityData = myDataLoaderReference.GetAbilityData(unitData.myAbilityId);
                    }
                    
                    card.Init(unitCardData, unitData, abilityData);
                    return card;
                }
                else
                {
                    Shared.LogError("[HOOD][FACTORY] - CreateMatchCard, no unitData from DataLoader");
                    return null;
                }
            }
            else if (cardData.myCardType == CardType.TECHNOLOGY)
            {
                AbilityCardData abilityCardData = cardData as AbilityCardData;
                AbilityData abilityData = myDataLoaderReference.GetAbilityData(abilityCardData.myAbilityId);
                if (abilityData != null)
                {
                    card.Init(abilityCardData, abilityData);
                    return card;
                }
                else
                {
                    Shared.LogError("[HOOD][FACTORY] - CreateMatchCard, no abilityData from DataLoader");
                    return null;
                }
            }
        }
        Shared.LogError("[HOOD][FACTORY] - CreateMatchCard, no cardData from DataLoader");
        return null;
    }


}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharedScripts;
using SharedScripts.DataId;
using System;

public class SharedDataLoader : MonoBehaviour
{
    // EXAMPLE:
    // Load data in order from least dependant to most dependant
    // Cards depend on data from units and abilities, so load units and abilities before attempting to load cards

    // NOTE:
    // If the myIsRequired[DATACLASS] field is false in the inspector, the asociated data will not be loaded without breaking the game
    // use this to remove data requirements for in-development features

    // Scene & Audio

    [SerializeField] private Audios_Def myAudiosData;
    [SerializeField] private bool myIsRequiredAudiosData; 
    private Dictionary<AudioId, AudioData> myAudios;

    [Space(10)]
    [SerializeField] private Scenes_Def myScenesData;
    [SerializeField] private bool myIsRequiredScenesData;
    private Dictionary<SceneId, SceneData> myScenes;

    // Gameplay
    [Space(10)]
    [SerializeField] private StatusEffects_Def myStatusEffectsData;
    [SerializeField] private bool myIsRequiredStatusEffectsData;
    private Dictionary<StatusEffectId, StatusEffectData> myStatusEffects;
    
    [Space(10)]
    [SerializeField] private Abilities_Def myAbilitiesData;
    [SerializeField] private bool myIsRequiredAbilitiesData;
    private Dictionary<AbilityId, AbilityData> myAbilities;
    
    [Space(10)]
    [SerializeField] private Units_Def myUnitsData;
    [SerializeField] private bool myIsRequiredUnitsData;
    private Dictionary<UnitId, UnitData> myUnits;

    [Space(10)]
    [SerializeField] private Cards_Def myCardsData;
    [SerializeField] private bool myIsRequiredCardsData;
    private Dictionary<CardId, CardData> myCards;

    [Space(10)]
    [SerializeField] private Tiles_Def myTilesData;
    [SerializeField] private bool myIsRequiredTilesData;
    private Dictionary<TileType, TileData> myTiles;

    [Space(10)]
    [SerializeField] private Options_Def myOptionsData;
    [SerializeField] private bool myIsRequiredFloatRangeOptionData;
    private Dictionary<FloatRangeOptionId, FloatRangeOptionData> myFloatRangeOptions;

    [SerializeField] private bool myIsRequiredBooleanOptionData;
    private Dictionary<BooleanOptionId, BooleanOptionData> myBooleanOptions;

    private Dictionary<string, bool> myRequiredDatasMap;
    private Dictionary<string, bool> myLoadedDatasMap;
    private int myDataLoaded;
    private int myDataToLoad;

    private bool IsAllTargetDataLoaded()
    {
        return myDataLoaded == myDataToLoad;
    }

    private bool IsSpecificDataLoaded(string aDataClassName)
    {
        if (myLoadedDatasMap.TryGetValue(aDataClassName, out bool isLoaded))
        {
            return isLoaded;
        }

        return false;
    }

    private void GetDataTargetInfo() 
    {
        myDataLoaded = 0;
        myRequiredDatasMap = new Dictionary<string, bool>();
        myRequiredDatasMap.Add(nameof(AudioData),               myIsRequiredAudiosData);
        myRequiredDatasMap.Add(nameof(SceneData),               myIsRequiredScenesData);
        myRequiredDatasMap.Add(nameof(StatusEffectData),        myIsRequiredStatusEffectsData);
        myRequiredDatasMap.Add(nameof(AbilityData),             myIsRequiredAbilitiesData);
        myRequiredDatasMap.Add(nameof(UnitData),                myIsRequiredUnitsData);
        myRequiredDatasMap.Add(nameof(CardData),                myIsRequiredCardsData);
        myRequiredDatasMap.Add(nameof(TileData),                myIsRequiredTilesData);
        myRequiredDatasMap.Add(nameof(FloatRangeOptionData),    myIsRequiredFloatRangeOptionData);
        myRequiredDatasMap.Add(nameof(BooleanOptionData),       myIsRequiredBooleanOptionData);

        myDataToLoad = 0;
        foreach (KeyValuePair<string, bool> toLoadData in myRequiredDatasMap)
        {
            if (toLoadData.Value == true)
            {
                myDataToLoad++;
            }
        }
    }

    private void LoadTargetData() 
    {
        myDataLoaded = 0;
        myLoadedDatasMap = new Dictionary<string, bool>();

        foreach (KeyValuePair<string, bool> toLoadData in myRequiredDatasMap)
        {
            if (toLoadData.Value == true)
            {
                switch (toLoadData.Key)
                {
                    case nameof(AudioData):
                        myLoadedDatasMap.Add(nameof(AudioData), LoadAudios());
                        break;
                    case nameof(SceneData):
                        myLoadedDatasMap.Add(nameof(SceneData), LoadScenes());
                        break;
                    case nameof(StatusEffectData):
                        myLoadedDatasMap.Add(nameof(StatusEffectData), LoadStatusEffects());
                        break;
                    case nameof(AbilityData):
                        myLoadedDatasMap.Add(nameof(AbilityData), LoadAbilities());
                        break;
                    case nameof(UnitData):
                        myLoadedDatasMap.Add(nameof(UnitData), LoadUnits());
                        break;
                    case nameof(CardData):
                        myLoadedDatasMap.Add(nameof(CardData), LoadCards());
                        break;
                    case nameof(TileData):
                        myLoadedDatasMap.Add(nameof(TileData), LoadTiles());
                        break;
                    case nameof(FloatRangeOptionData):
                        myLoadedDatasMap.Add(nameof(FloatRangeOptionData), LoadFloatRangeOptions());
                        break;
                    case nameof(BooleanOptionData):
                        myLoadedDatasMap.Add(nameof(BooleanOptionData), LoadBooleanOptions());
                        break;
                    default:
                        break;
                }
            }
        }

        foreach (KeyValuePair<string, bool> loadedData in myLoadedDatasMap)
        {
            if (loadedData.Value == true)
            {
                myDataLoaded++;
            }
        }
    }

    private void Awake()
    {
        GetDataTargetInfo();
        LoadTargetData();

        if (!IsAllTargetDataLoaded())
        {
            Shared.LogError("[HOOD][LOAD][DATA] - Data load error, loaded: " + myDataLoaded + " out of: " + myDataToLoad);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        else 
        {
            EventHandler.CallAfterDataLoadedEvent();
        }
    }

    private bool LoadAudios() 
    {
        bool success = false;

        if (myAudiosData != null)
        {
            myAudios = new Dictionary<AudioId, AudioData>();
            int validCount = 0;

            foreach (AudioData audio in myAudiosData.myAudios)
            {
                if (audio.myId != AudioId.INVALID)
                {
                    myAudios.Add(audio.myId, audio);
                    validCount++;
                }
            }

            // EXAMPLE:
            // Simple data validation implementation, if some data has not had its id set, its probably not filled in correctly 
            success = validCount == myAudiosData.myAudios.Count;
        }

        return success;
    }
    
    public AudioData GetAudioData(AudioId anId) 
    {
        if (!IsSpecificDataLoaded(nameof(AudioData))) 
        {
            Shared.LogError("[HOOD][AUDIO][DATA] - GetAudioData attempt before load completed.");
            return null;
        }

        if (myAudios.TryGetValue(anId, out AudioData audioData))
        {
            return audioData;
        }
        else 
        {
            Shared.LogError("[HOOD][AUDIO][DATA] - Invalid AudioId at GetAudioData.");
            return null;
        }
    }

    public Dictionary<AudioId, AudioData> GetAllAudiosData()
    {
        if (!IsSpecificDataLoaded(nameof(AudioData)))
        {
            Shared.LogError("[HOOD][SCENE][AUDIO] - GetAllAudiosData attempt before load completed.");
            return null;
        }

        return myAudios;
    }

    private bool LoadScenes()
    {
        bool success = false;

        if (myScenesData != null)
        {
            myScenes = new Dictionary<SceneId, SceneData>();
            int validCount = 0;

            foreach (SceneData scene in myScenesData.myScenes)
            {
                if (scene.myId != SceneId.INVALID)
                {
                    myScenes.Add(scene.myId, scene);
                    validCount++;
                }
            }

            success = validCount == myScenesData.myScenes.Count;
        }

        return success;
    }

    public SceneData GetSceneData(SceneId anId)
    {
        if (!IsSpecificDataLoaded(nameof(SceneData)))
        {
            Shared.LogError("[HOOD][SCENE][DATA] - GetSceneData attempt before load completed.");
            return null;
        }

        if (myScenes.TryGetValue(anId, out SceneData sceneData))
        {
            return sceneData;
        }
        else
        {
            Shared.LogError("[HOOD][SCENE][DATA] - Invalid SceneId at GetSceneData.");
            return null;
        }
    }

    public Dictionary<SceneId, SceneData> GetAllScenesData()
    {
        if (!IsSpecificDataLoaded(nameof(SceneData)))
        {
            Shared.LogError("[HOOD][SCENE][DATA] - GetAllScenesData attempt before load completed.");
            return null;
        }

        return myScenes;
    }

    private bool LoadStatusEffects()
    {
        bool success = false;

        if (myStatusEffectsData != null)
        {
            myStatusEffects = new Dictionary<StatusEffectId, StatusEffectData>();
            int validCount = 0;

            foreach (StatusEffectData statusEffect in myStatusEffectsData.myStatusEffects)
            {
                if (statusEffect.myId != StatusEffectId.INVALID)
                {
                    myStatusEffects.Add(statusEffect.myId, statusEffect);
                    validCount++;
                }
            }

            success = validCount == myStatusEffectsData.myStatusEffects.Count;
        }

        return success;
    }

    public StatusEffectData GetStatusEffectData(StatusEffectId anId)
    {
        if (!IsSpecificDataLoaded(nameof(StatusEffectData)))
        {
            Shared.LogError("[HOOD][UNIT][DATA] - GetStatusEffectData attempt before load completed.");
            return null;
        }

        if (myStatusEffects.TryGetValue(anId, out StatusEffectData statusEffectsData))
        {
            return statusEffectsData;
        }
        else
        {
            Shared.LogError("[HOOD][UNIT][DATA] - Invalid StatusEffectId at GetStatusEffecData.");
            return null;
        }
    }

    private bool LoadUnits()
    {
        bool success = false;

        if (myUnitsData != null)
        {
            myUnits = new Dictionary<UnitId, UnitData>();
            int validCount = 0;

            foreach (UnitData unit in myUnitsData.myUnits)
            {
                if (unit.myId != UnitId.INVALID) 
                {
                    myUnits.Add(unit.myId, unit);
                    validCount++; 
                }
            }

            success = validCount == myUnitsData.myUnits.Count;
        }

        return success;
    }

    public UnitData GetUnitData(UnitId anId) 
    {
        if (!IsSpecificDataLoaded(nameof(UnitData)))
        {
            Shared.LogError("[HOOD][UNIT][DATA] - GetUnitData attempt before load completed.");
            return null;
        }

        if (myUnits.TryGetValue(anId, out UnitData unitData))
        {
            return unitData;
        }
        else
        {
            Shared.LogError("[HOOD][UNIT][DATA] - Invalid UnitId at GetUnitData.");
            return null;
        }
    } 

    private bool LoadAbilities()
    {
        bool success = false;

        if (myAbilitiesData != null)
        {
            myAbilities = new Dictionary<AbilityId, AbilityData>();
            int validCount = 0;

            foreach (AbilityData ability in myAbilitiesData.myAbilities)
            {
                if (ability.myId != AbilityId.INVALID)
                {
                    myAbilities.Add(ability.myId, ability);
                    validCount++;
                }
            }

            success = validCount == myAbilitiesData.myAbilities.Count;
        }

        return success;
    }

    public AbilityData GetAbilityData(AbilityId anId)
    {
        if (!IsSpecificDataLoaded(nameof(AbilityData)))
        {
            Shared.LogError("[HOOD][ABILITY][DATA] - GetAbilityData attempt before load completed.");
            return null;
        }

        if (myAbilities.TryGetValue(anId, out AbilityData abilityData))
        {
            return abilityData;
        }
        else
        {
            Shared.LogError("[HOOD][ABILITY][DATA] - Invalid AbilityId at GetAbilityData.");
            return null;
        }
    }

    private bool LoadFloatRangeOptions()
    {
        bool success = false;

        if (myOptionsData != null)
        {
            myFloatRangeOptions = new Dictionary<FloatRangeOptionId, FloatRangeOptionData>();
            int validCount = 0;

            foreach (FloatRangeOptionData option in myOptionsData.myFloatOptions)
            {
                if (option.myId != FloatRangeOptionId.INVALID)
                {
                    myFloatRangeOptions.Add(option.myId, option);
                    validCount++;
                }
            }

            success = validCount == myOptionsData.myFloatOptions.Count;
        }

        return success;
    }

    public FloatRangeOptionData GetFloatRangeOptionData(FloatRangeOptionId anId)
    {
        if (!IsSpecificDataLoaded(nameof(FloatRangeOptionData)))
        {
            Shared.LogError("[HOOD][OPTION][DATA] - GetFloatRangeOptionData attempt before load completed.");
            return null;
        }

        if (myFloatRangeOptions.TryGetValue(anId, out FloatRangeOptionData optionData))
        {
            return optionData;
        }
        else
        {
            Shared.LogError("[HOOD][OPTION][DATA] - Invalid FloatRangeOptionId at GetFloatRangeOptionData.");
            return null;
        }
    }

    public Dictionary<FloatRangeOptionId, FloatRangeOptionData> GetAllFloatRangeOptionData()
    {
        if (!IsSpecificDataLoaded(nameof(FloatRangeOptionData)))
        {
            Shared.LogError("[HOOD][OPTION][DATA] - GetAllFloatRangeOptionData attempt before load completed.");
            return null;
        }

        return myFloatRangeOptions;
    }

    private bool LoadBooleanOptions()
    {
        bool success = false;

        if (myOptionsData != null)
        {
            myBooleanOptions = new Dictionary<BooleanOptionId, BooleanOptionData>();
            int validCount = 0;

            foreach (BooleanOptionData option in myOptionsData.myBooleanOptions)
            {
                if (option.myId != BooleanOptionId.INVALID)
                {
                    myBooleanOptions.Add(option.myId, option);
                    validCount++;
                }
            }

            success = validCount == myOptionsData.myBooleanOptions.Count;
        }

        return success;
    }

    public BooleanOptionData GetBooleanOptionData(BooleanOptionId anId)
    {
        if (!IsSpecificDataLoaded(nameof(BooleanOptionData)))
        {
            Shared.LogError("[HOOD][OPTION][DATA] - GetBooleanOptionData attempt before load completed.");
            return null;
        }

        if (myBooleanOptions.TryGetValue(anId, out BooleanOptionData optionData))
        {
            return optionData;
        }
        else
        {
            Shared.LogError("[HOOD][OPTION][DATA] - Invalid BooleanOptionId at GetBooleanOptionData.");
            return null;
        }
    }

    public Dictionary<BooleanOptionId, BooleanOptionData> GetAllBooleanOptionData()
    {
        if (!IsSpecificDataLoaded(nameof(BooleanOptionData)))
        {
            Shared.LogError("[HOOD][OPTION][DATA] - GetAllBooleanOptionData attempt before load completed.");
            return null;
        }

        return myBooleanOptions;
    }

    private bool LoadCards()
    {
        bool success = false;

        if (myCardsData != null)
        {
            myCards = new Dictionary<CardId, CardData>();
            int validCount = 0;

            foreach (AbilityCardData abilityCard in myCardsData.myAbilityCards)
            {
                if (abilityCard.myId != CardId.INVALID && abilityCard.myCardType == CardType.TECHNOLOGY)
                {
                    myCards.Add(abilityCard.myId, abilityCard);
                    validCount++;
                }
            }

            foreach (UnitCardData unitCard in myCardsData.myUnitCards)
            {
                if (unitCard.myId != CardId.INVALID && (unitCard.myCardType == CardType.UNIT || unitCard.myCardType == CardType.MOTHERSHIP))
                {
                    myCards.Add(unitCard.myId, unitCard);
                    validCount++;
                }
            }

            success = validCount == myCardsData.myAbilityCards.Count + myCardsData.myUnitCards.Count;
        }

        return success;
    }

    public CardData GetCardData(CardId anId)
    {
        if (!IsSpecificDataLoaded(nameof(CardData)))
        {
            Shared.LogError("[HOOD][CARD][DATA] - GetCardData attempt before load completed.");
            return null;
        }

        if (myCards.TryGetValue(anId, out CardData cardData))
        {
            return cardData;
        }
        else
        {
            Shared.LogError("[HOOD][CARD][DATA] - Invalid CardId at GetCardData.");
            return null;
        }
    }

    private bool LoadTiles()
    {
        bool success = false;

        if (myTilesData != null)
        {
            myTiles = new Dictionary<TileType, TileData>();
            int validCount = 0;

            foreach (TileData tile in myTilesData.myTiles)
            {
                if (tile.myType != TileType.INVALID && tile.myType != TileType.INVALID)
                {
                    myTiles.Add(tile.myType, tile);
                    validCount++;
                }
            }

            success = validCount == myTilesData.myTiles.Count;
        }

        return success;
    }

    public TileData GetTileData(TileType aType)
    {
        if (!IsSpecificDataLoaded(nameof(TileData)))
        {
            Shared.LogError("[HOOD][TILE][DATA] - GetCardData attempt before load completed.");
            return null;
        }

        if (myTiles.TryGetValue(aType, out TileData tileData))
        {
            return tileData;
        }
        else
        {
            Shared.LogError("[HOOD][TILE][DATA] - Invalid TileType at GetTileData.");
            return null;
        }
    }

    public Dictionary<TileType, TileData> GetAllTilesData()
    {
        if (!IsSpecificDataLoaded(nameof(TileData)))
        {
            Shared.LogError("[HOOD][SCENE][DATA] - GetAllTilesData attempt before load completed.");
            return null;
        }

        return myTiles;
    }
}

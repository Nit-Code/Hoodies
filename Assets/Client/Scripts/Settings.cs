using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharedScripts.DataId;
using static Options;

public class Settings : MonoBehaviour
{
    private SharedDataLoader mySharedDataLoaderReference;
    private Dictionary<FloatRangeOptionId, FloatRangeOptionData> myFloatRangeOptionsDataMap;
    private Dictionary<FloatRangeOptionId, FloatRangeOption> myFloatRangeOptionsMap;

    private Dictionary<BooleanOptionId, BooleanOptionData> myBooleanOptionsDataMap;
    private Dictionary<BooleanOptionId, BooleanOption> myBooleanOptionsMap;

    private AudioManager myAudioManagerReference;
    public AudioManager GetAudioManagerReference() { return myAudioManagerReference; }

    private SharedUser mySharedUserReference;

    private bool myIsLoadPending;
    private bool myIsInitPending;
    private bool myIsSavePending;
    private bool myIsLoadInProgress;

    // 1 - We get the constant data for options
    private void Awake()
    {
        myIsInitPending = true;
        myIsLoadPending = false;
        myIsSavePending = false;
        myIsLoadInProgress = false;

        if (TryGetComponent<SharedDataLoader>(out mySharedDataLoaderReference))
        {
            myFloatRangeOptionsDataMap = mySharedDataLoaderReference.GetAllFloatRangeOptionData();
            myBooleanOptionsDataMap = mySharedDataLoaderReference.GetAllBooleanOptionData();
        }

        EventHandler.OurAfterLoggedInEvent += OnUserLoggedIn;
    }

    // 2 - We get some extra references which might not be loaded at Awake(), attempt to Init here.
    private void Start()
    {
        myAudioManagerReference = FindObjectOfType<AudioManager>();

        if (myFloatRangeOptionsDataMap != null && myBooleanOptionsDataMap != null && myAudioManagerReference != null)
            Init();
        else
            Shared.LogError("[HOOD][CLIENT][OPTIONS] - Unable to Init()");
    }

    // 3 - Init all the options with the constant data values.
    private void Init()
    {
        myFloatRangeOptionsMap = new Dictionary<FloatRangeOptionId, FloatRangeOption>();
        foreach (KeyValuePair<FloatRangeOptionId, FloatRangeOptionData> floatRangeOptionData in myFloatRangeOptionsDataMap)
        {
            switch (floatRangeOptionData.Key)
            {
                case FloatRangeOptionId.VOLUME_MASTER:
                    {
                        VolumeMaster option = new VolumeMaster(floatRangeOptionData.Value, this);
                        myFloatRangeOptionsMap.Add(floatRangeOptionData.Key, option);
                        break;
                    }
                case FloatRangeOptionId.VOLUME_MUSIC:
                    {
                        VolumeMusic option = new VolumeMusic(floatRangeOptionData.Value, this);
                        myFloatRangeOptionsMap.Add(floatRangeOptionData.Key, option);
                        break;
                    }
                case FloatRangeOptionId.VOLUME_SOUND:
                    {
                        VolumeSound option = new VolumeSound(floatRangeOptionData.Value, this);
                        myFloatRangeOptionsMap.Add(floatRangeOptionData.Key, option);
                        break;
                    }
                case FloatRangeOptionId.VOLUME_AMBIENT:
                    {
                        VolumeAmbient option = new VolumeAmbient(floatRangeOptionData.Value, this);
                        myFloatRangeOptionsMap.Add(floatRangeOptionData.Key, option);
                        break;
                    }
                case FloatRangeOptionId.INVALID:
                default:
                    Shared.LogError("[HOOD][CLIENT][OPTIONS] - Invalid or missing FloatRangeOptionId at Init()");
                    break;
            }
        }

        myBooleanOptionsMap = new Dictionary<BooleanOptionId, BooleanOption>();
        foreach (KeyValuePair<BooleanOptionId, BooleanOptionData> booleanOptionData in myBooleanOptionsDataMap)
        {
            switch (booleanOptionData.Key)
            {
                case BooleanOptionId.BOOLEAN_OPTION_1:
                    Shared.Log("[HOOD][CLIENT][OPTIONS] - not implemented.");
                    // SomeBooleanOption option = new SomeBooleanOption(booleanOptionData.Value, this);
                    // myBooleanOptionsMap.Add(booleanOptionData.Key, option);
                    break;
                case BooleanOptionId.INVALID:
                default:
                    Shared.LogError("[HOOD][CLIENT][OPTIONS] - Invalid or missing BooleanOptionId at Init()");
                    break;
            }
        }

        myIsInitPending = false;
        SetAllToDefault();
    }

    // 4 - Once our user has logged in, we have data to find their settings persistant file, raise a flag to let the 
    // Update know to search for the newly logged in reference
    private void OnUserLoggedIn()
    {
        EventHandler.OurAfterLoggedInEvent -= OnUserLoggedIn;
        EventHandler.OurAfterLoggedOutEvent += OnUserLoggedOut;

        // Since OurAfterLoggedInEvent is started from a thread which does not allow to perform some actions,
        // raise a flag and catch it in the Update function of this class to unlink it from the callstack and procede.
        if (!myIsInitPending)
        {
            myIsLoadPending = true;
        }
        else
        {
            Shared.LogError("[HOOD][CLIENT][OPTIONS] - Not loading persistent settings since we never the base constant data for options.");
        }
    }

    private void Update()
    {
        if (myIsLoadPending)
        {
            StartLoadFromPersistance();
        }

        if (myIsSavePending && !myIsLoadInProgress) 
        {
            SaveAllToPersistance();
        }
    }

    // 5 - We likely have the user at this point, try to load the persistent settings file
    private void StartLoadFromPersistance() 
    {
        if (!TryGetComponent<SharedUser>(out mySharedUserReference))
        {
            Shared.LogError("[HOOD][CLIENT][OPTIONS] - mySharedUserReference not found");
            return;
        }

        SetAllFromPersistance();
        myIsLoadPending = false;
    }

    private void SetAllToDefault()
    {
        foreach (KeyValuePair<FloatRangeOptionId, FloatRangeOption> floatRangeOption in myFloatRangeOptionsMap)
        {
            floatRangeOption.Value.ResetValue();
        }

        foreach (KeyValuePair<BooleanOptionId, BooleanOption> booleanOption in myBooleanOptionsMap)
        {
            booleanOption.Value.ResetValue();
        }
    }

    private void SetAllFromPersistance()
    {
        string username = mySharedUserReference.GetUsername();
        if (string.IsNullOrEmpty(username))
        {
            Shared.LogError("[HOOD][CLIENT][OPTIONS] - No username found to load settings.");
            return;
        }

        myIsLoadInProgress = true;

        OptionsCache optionsCache = new OptionsCache();
        if (SaveDataManager.LoadJsonData(optionsCache, username))
        {
            Dictionary<BooleanOptionId, bool> persistedBooleanValues = optionsCache.GetBoolOptionValues();
            if (persistedBooleanValues != null) 
            {
                foreach (KeyValuePair<BooleanOptionId, bool> persistedValue in persistedBooleanValues)
                {
                    SetBooleanOptionValue(persistedValue.Key, persistedValue.Value);
                }
            }

            Dictionary<FloatRangeOptionId, float> persistedFloatRangeValues = optionsCache.GetFloatRangeOptionValues();
            if (persistedFloatRangeValues != null) 
            {
                foreach (KeyValuePair<FloatRangeOptionId, float> persistedValue in persistedFloatRangeValues)
                {
                    SetFloatRangeOptionValue(persistedValue.Key, persistedValue.Value, false);
                }
            }
        }

        myIsLoadInProgress = false;
    }
   
    private void OnUserLoggedOut()
    {
        mySharedUserReference = null;
        EventHandler.OurAfterLoggedInEvent += OnUserLoggedIn;
        EventHandler.OurAfterLoggedOutEvent -= OnUserLoggedOut;
    }

    public void SetBooleanOptionValue(BooleanOptionId anId, bool aNewValue)
    {
        if (myBooleanOptionsMap.ContainsKey(anId)) 
        {
            myBooleanOptionsMap[anId].SetValue(aNewValue);
            myIsSavePending = true;
        }
        else
            Shared.LogError("[HOOD][CLIENT][OPTIONS] - SetBooleanOptionValue, BooleanOptionId not found: " + anId);
    }

    public void SetFloatRangeOptionValue(FloatRangeOptionId anId, float aNewPercentualValue, bool aIsSaveRequired)
    {
        if (myFloatRangeOptionsMap.ContainsKey(anId)) 
        {
            myFloatRangeOptionsMap[anId].SetValue(aNewPercentualValue);
            myIsSavePending = aIsSaveRequired;
        }
        else
            Shared.LogError("[HOOD][CLIENT][OPTIONS] - SetFloatRangeOptionValue, FloatRangeOptionId not found: " + anId);
    }

    public bool GetBooleanOptionValue(BooleanOptionId anId)
    {
        if (myBooleanOptionsMap.ContainsKey(anId))
            return myBooleanOptionsMap[anId].GetValue();

        Shared.LogError("[HOOD][CLIENT][OPTIONS] - GetBooleanOptionValue, BooleanOptionId not found: " + anId);
        return false;
    }

    public float GetFloatRangeOptionValue(FloatRangeOptionId anId, bool aIsPercentualValue)
    {
        if (myFloatRangeOptionsMap.ContainsKey(anId))
        {
            if (aIsPercentualValue)
                return myFloatRangeOptionsMap[anId].GetPercentualValue();
            else
                return myFloatRangeOptionsMap[anId].GetContextualValue();
        }

        Shared.LogError("[HOOD][CLIENT][OPTIONS] - SetFloatRangeOptionValue, FloatRangeOptionId not found: " + anId);
        return float.MinValue;
    }

    private void SaveAllToPersistance()
    {
        myIsSavePending = false;
        string username = mySharedUserReference.GetUsername();
        if (string.IsNullOrEmpty(username))
        {
            Shared.LogError("[HOOD][CLIENT][OPTIONS] - No username found to persist settings.");
            return;
        }

        Dictionary<FloatRangeOptionId, float> myFloatRangePercentualValuesMap = new Dictionary<FloatRangeOptionId, float>();
        foreach (KeyValuePair<FloatRangeOptionId, FloatRangeOption> floatRangeOption in myFloatRangeOptionsMap)
        {
            myFloatRangePercentualValuesMap.Add(floatRangeOption.Key, floatRangeOption.Value.GetPercentualValue());
        }

        Dictionary<BooleanOptionId, bool> myBoleanValuesMap = new Dictionary<BooleanOptionId, bool>();
        foreach (KeyValuePair<BooleanOptionId, BooleanOption> booleanOption in myBooleanOptionsMap)
        {
            myBoleanValuesMap.Add(booleanOption.Key, booleanOption.Value.GetValue());
        }

        OptionsCache optionsToSave = new OptionsCache(myFloatRangePercentualValuesMap, myBoleanValuesMap, username);
        SaveDataManager.SaveJsonData(optionsToSave, username);
    }
}


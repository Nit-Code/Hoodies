using SharedScripts;
using SharedScripts.DataId;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource myAmbientAudioSource;
    [SerializeField] private AudioSource myMusicAudioSource;
    [SerializeField] private GameObject mySoundAudioSourcePrefab;

    [Header("Other")]
    [SerializeField] private float myMusicTransitionSeconds = 3f;
    [SerializeField] private AudioMixer myAudioMixer = null;
    
    private Dictionary<AudioId, AudioData> myAudiosDataReference;
    private Dictionary<SceneId, AudioId> mySceneAudiosMap;
    private Dictionary<SceneId, AudioId> mySceneAmbientsMap;

    private SharedDataLoader myDataLoaderReference;
    private GameObjectPool myPoolReference;
    private SceneController mySceneControllerReference;

    private Coroutine myPlaySceneSoundsCoroutine;

    private const string MASTER_VOL_NAME = "MasterVolume";
    private const string MUSIC_MASTER_VOL_NAME = "MusicMasterVolume";
    private const string AMBIENT_MASTER_VOL_NAME = "AmbientMasterVolume";
    private const string SOUND_MASTER_VOL_NAME = "SoundMasterVolume";

    private const string MUSIC_VOL_NAME = "MusicVolume";
    private const string AMBIENT_VOL_NAME = "AmbientVolume";

    protected const float MY_DECIBELS_RANGE_MIN = -80.0f;
    protected const float MY_DECIBELS_RANGE_MAX = 0.0f;
    private Vector2 myDecibelsRange;

    private int myDataToLoad;
    private int myDataLoaded;

    private bool IsDataLoaded() 
    {
        return myDataLoaded == myDataToLoad;
    }

    private void Start()
    {
        myDataLoaded = 0; 
        myDataToLoad = 6;
        myDecibelsRange = new Vector2(MY_DECIBELS_RANGE_MIN, MY_DECIBELS_RANGE_MAX);

        myPoolReference = FindObjectOfType<GameObjectPool>();
        if (myPoolReference != null)
        {
            myDataLoaded++;
        }

        mySceneControllerReference = FindObjectOfType<SceneController>();
        if (mySceneControllerReference != null)
        {
            myDataLoaded++;
        }

        myDataLoaderReference = FindObjectOfType<SharedDataLoader>();
        if (myDataLoaderReference != null)
        {
            myDataLoaded++;

            Dictionary<SceneId, SceneData> scenesDataReference = myDataLoaderReference.GetAllScenesData();
            if (scenesDataReference != null)
            {
                mySceneAudiosMap = new Dictionary<SceneId, AudioId>();
                mySceneAmbientsMap = new Dictionary<SceneId, AudioId>();
                foreach (KeyValuePair<SceneId, SceneData> sceneData in scenesDataReference)
                {
                    mySceneAudiosMap.Add(sceneData.Key, sceneData.Value.myAudioId);
                    mySceneAmbientsMap.Add(sceneData.Key, sceneData.Value.myAmbientAudioId);
                }
                myDataLoaded++;
                myDataLoaded++;
            }

            myAudiosDataReference = myDataLoaderReference.GetAllAudiosData();
            if (myAudiosDataReference != null)
            {
                myDataLoaded++;
            }
        }
    }

    private void OnEnable()
    {
        EventHandler.OurAfterSceneLoadEvent += PlaySceneSounds;
    }

    private void OnDisable()
    {
        EventHandler.OurAfterSceneLoadEvent -= PlaySceneSounds;
    }

    private float RemapFromPercentageToDecibels(float aPercentage) 
    {
        Vector2 percentageRange = new Vector2(0.0f, 1.0f);

        if (aPercentage >= percentageRange.x && aPercentage <= percentageRange.y)
        {
            return RemapClamped(aPercentage, percentageRange, myDecibelsRange);
        }
        else 
        {
            Shared.LogError("[HOOD][CLIENT][AUDIO] - aPercentage out of bounds.");
            return 1.0f;
        }
    }

    public void SetMasterVolume(float aPercentage)
    {
        float volume = RemapFromPercentageToDecibels(aPercentage);
        myAudioMixer.SetFloat(MASTER_VOL_NAME, volume);
    }

    public void SetAmbientMasterVolume(float aPercentage)
    {
        float volume = RemapFromPercentageToDecibels(aPercentage);
        myAudioMixer.SetFloat(AMBIENT_MASTER_VOL_NAME, volume);
    }

    public void SetMusicMasterVolume(float aPercentage)
    {
        float volume = RemapFromPercentageToDecibels(aPercentage);
        myAudioMixer.SetFloat(MUSIC_MASTER_VOL_NAME, volume);
    }

    public void SetSoundMasterVolume(float aPercentage)
    {
        float volume = RemapFromPercentageToDecibels(aPercentage);
        myAudioMixer.SetFloat(SOUND_MASTER_VOL_NAME, volume);
    }

    public void PlaySound(AudioId anId)
    {
        AudioData audio;
        if (!myAudiosDataReference.TryGetValue(anId, out audio) || mySoundAudioSourcePrefab == null)
        {
            Debug.LogError("[HOOD][CLIENT][AUDIO] - Some value not found at PlaySound.");
        }

        GameObject soundGameObject = myPoolReference.ReuseObject(mySoundAudioSourcePrefab, Vector3.zero, Quaternion.identity); // We retrieve the generic 'Sound' gameobject from the pool
        Sound sound = soundGameObject.GetComponent<Sound>();
        sound.SetAudio(audio, audio.myAudioVolume);
        soundGameObject.SetActive(true); // It automatically plays the sound because it plays on Awake
        StartCoroutine(DisableSound(soundGameObject, audio.myAudioClip.length)); // Disable the sound after it's done playing
    }

    private AudioData GetSceneAudioClip(SceneId anId) 
    {
        AudioData audio = null;

        if (IsDataLoaded())
        {
            AudioId audioId;
            if (mySceneAudiosMap.TryGetValue(anId, out audioId))
            {
                audio = GetAudioClip(audioId);
            }
        }

        return audio;
    }

    private AudioData GetSceneAmbientAudioClip(SceneId anId)
    {
        AudioData audio = null;

        if (IsDataLoaded())
        {
            AudioId ambientAudioId;
            if (mySceneAmbientsMap.TryGetValue(anId, out ambientAudioId))
            {
                audio = GetAudioClip(ambientAudioId);
            }
        }

        return audio;
    }

    private AudioData GetAudioClip(AudioId anId) 
    {
        AudioData audio = null;

        if (IsDataLoaded()) 
        {
            AudioData outAudio;
            if (myAudiosDataReference.TryGetValue(anId, out outAudio))
            {

                audio = outAudio;
            }
        }

        return audio;
    }

    private void PlaySceneSounds()
    {
        if (!IsDataLoaded()) 
        {
            Debug.LogError("[HOOD][CLIENT][AUDIO] - Attempted to PlaySceneSounds before AudioManager was ready.");
            return;
        }
        
        SceneId currentSceneId = mySceneControllerReference.GetCurrentScene();
        if (currentSceneId == SceneId.INVALID) 
        {
            Debug.LogError("[HOOD][CLIENT][AUDIO] - Attempted to PlaySceneSounds when currentSceneId == SceneId.INVALID.");
            return;
        }

        bool isPlayingSameMusic = false;
        bool isPlayingSameAmbientSound = false;

        AudioData sceneAudio = GetSceneAudioClip(currentSceneId);
        AudioData sceneAmbienAudio = GetSceneAmbientAudioClip(currentSceneId);

        // Stop already playing sounds
        if (myPlaySceneSoundsCoroutine != null)
        {
            StopCoroutine(myPlaySceneSoundsCoroutine);

            if (sceneAudio != null)
            {
                if (myMusicAudioSource.clip != sceneAudio.myAudioClip)
                {
                    myMusicAudioSource.Stop();
                }
                else
                {
                    Debug.Log("[HOOD][CLIENT][AUDIO] - Attempted to play same music, doing nothing.");
                    isPlayingSameMusic = true;
                }
            }
            if (sceneAmbienAudio != null)
            {
                if (myAmbientAudioSource.clip != sceneAmbienAudio.myAudioClip)
                {
                    myAmbientAudioSource.Stop();
                }
                else
                {
                    Debug.Log("[HOOD][CLIENT][AUDIO] - Attempted to play same ambient, doing nothing.");
                    isPlayingSameAmbientSound = true;
                }
            }
        }

        // Play sounds
        myPlaySceneSoundsCoroutine = StartCoroutine(PlaySceneSoundsCoroutine(sceneAudio, sceneAmbienAudio, isPlayingSameMusic, isPlayingSameAmbientSound));
    }

    private IEnumerator PlaySceneSoundsCoroutine(AudioData aMusicAudioItem, AudioData anAmbientAudioItem, bool anIsPlayingSameMusic, bool anIsPlayingSameAmbientSound)
    {
        if (!anIsPlayingSameAmbientSound && anAmbientAudioItem != null)
        {
            PlayAmbientSoundClip(anAmbientAudioItem);
        }

        if (!anIsPlayingSameMusic && aMusicAudioItem != null)
        {
            // Wait before playing music
            yield return new WaitForSeconds(myMusicTransitionSeconds);
            PlayMusicSoundClip(aMusicAudioItem);
        }
    }

    private void PlayAmbientSoundClip(AudioData anAmbientAudioItem)
    {
        // Set Volume
        myAudioMixer.SetFloat(AMBIENT_VOL_NAME, anAmbientAudioItem.myAudioVolume);

        // Set clip & play
        myAmbientAudioSource.clip = anAmbientAudioItem.myAudioClip;
        myAmbientAudioSource.Play();
    }

    private void PlayMusicSoundClip(AudioData aMusicAudioItem)
    {
        // Set Volume
        myAudioMixer.SetFloat(MUSIC_VOL_NAME, aMusicAudioItem.myAudioVolume);

        // Set clip & play
        myMusicAudioSource.clip = aMusicAudioItem.myAudioClip;
        myMusicAudioSource.Play();
    }

    private float RemapClamped(float aValue, Vector2 aRangeIn, Vector2 aRangeOut)
    {
        float t = (aValue - aRangeIn.x) / (aRangeIn.y - aRangeIn.x);
        if (t > 1f)
            return aRangeOut.x;
        if (t < 0f)
            return aRangeOut.y;
        return aRangeOut.x + (aRangeOut.y - aRangeOut.x) * t;
    }

    // ACTION SOUNDS //
    
    private float ConvertSoundVolumeDecimalFractionToDecibels(float aVolumeDecimalFracion)
    {
        return (aVolumeDecimalFracion * 100f - 80f);
    }
    

    private IEnumerator DisableSound(GameObject aSoundGameObject, float aSoundDuration)
    {
        yield return new WaitForSeconds(aSoundDuration);
        aSoundGameObject.SetActive(false);
    }
}

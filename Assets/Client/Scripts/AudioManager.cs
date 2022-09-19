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
    private float mySoundVolume = 1f;

    private const string MASTER_VOL_NAME = "Master";
    private const string MUSIC_MASTER_VOL_NAME = "MusicMaster";
    private const string AMBIENT_MASTER_VOL_NAME = "AmbientMaster";

    private const string MUSIC_VOL_NAME = "Music";
    private const string AMBIENT_VOL_NAME = "Ambient";

    private const float MAX_DECIBELS = 20;
    private const float MIN_DECIBELS = -80;
    private Vector2 myDecibelsRange;

    private int myDataToLoad;

    //TODO: these values should be stored on the ui class that handles the volume sliders
    private const float MAX_SLIDER = 20;
    private const float MIN_SLIDER = -80;
    private int myDataLoaded;
    private Vector2 myUISliderRange;

    private void Awake()
    {
        myDecibelsRange = new Vector2(MIN_DECIBELS, MAX_DECIBELS);
        myUISliderRange = new Vector2(MIN_SLIDER, MAX_SLIDER);
    }

    private bool IsDataLoaded() 
    {
        return myDataLoaded == myDataToLoad;
    }

    private void Start()
    {
        myDataLoaded = 0; 
        myDataToLoad = 6;
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

    public void ChangeMasterVolume(float aVolume)
    {
        aVolume = Mathf.Clamp(aVolume, 0, 1);
        myAudioMixer.SetFloat(MASTER_VOL_NAME, RemapClamped(aVolume, myUISliderRange, myDecibelsRange));
    }

    public void ChangeMusicMasterVolume(float aVolume)
    {
        aVolume = Mathf.Clamp(aVolume, 0, 1);
        myAudioMixer.SetFloat(MUSIC_MASTER_VOL_NAME, RemapClamped(aVolume, myUISliderRange, myDecibelsRange));
    }

    public void ChangeAmbientMasterVolume(float aVolume)
    {
        aVolume = Mathf.Clamp(aVolume, 0, 1);
        myAudioMixer.SetFloat(AMBIENT_MASTER_VOL_NAME, RemapClamped(aVolume, myUISliderRange, myDecibelsRange));
    }

    // TODO mda: should this ve handled in the same way ambient and music volumes are handled?
    public void ChangeSoundVolume(float aVolume)
    {
        mySoundVolume = aVolume;
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
        sound.SetAudio(audio, mySoundVolume);
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
        float volume = Mathf.Clamp(anAmbientAudioItem.myAudioVolume, myUISliderRange.x, myUISliderRange.y);
        myAudioMixer.SetFloat(AMBIENT_VOL_NAME, RemapClamped(volume, myUISliderRange, myDecibelsRange));

        // Set clip & play
        myAmbientAudioSource.clip = anAmbientAudioItem.myAudioClip;
        myAmbientAudioSource.Play();
    }

    private void PlayMusicSoundClip(AudioData aMusicAudioItem)
    {
        // Set Volume
        float volume = Mathf.Clamp(aMusicAudioItem.myAudioVolume, myUISliderRange.x, myUISliderRange.y);
        myAudioMixer.SetFloat(MUSIC_VOL_NAME, RemapClamped(volume, myUISliderRange, myDecibelsRange));

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
    /*
    private float ConvertSoundVolumeDecimalFractionToDecibels(float aVolumeDecimalFracion)
    {
        return (aVolumeDecimalFracion * 100f - 80f);
    }
    */

    private IEnumerator DisableSound(GameObject aSoundGameObject, float aSoundDuration)
    {
        yield return new WaitForSeconds(aSoundDuration);
        aSoundGameObject.SetActive(false);
    }
}

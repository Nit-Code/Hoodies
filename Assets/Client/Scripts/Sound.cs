using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Sound : MonoBehaviour
{
    private AudioSource myAudioSource;

    public Sound() 
    {
        myAudioSource = null;
    }

    private void Awake()
    {
        myAudioSource = GetComponent<AudioSource>();
    }

    // Optional aVolume, default value = 1
    // TODO: change this to Init() to comply with coding standard
    public void SetAudio(AudioData anAudioItem, float aVolume = 1) 
    {
        if (myAudioSource == null) 
        {
            Debug.LogError("[HOOD][CLIENT][AUDIO] - AudioSource not found at SetAudio.");
            return;
        }

        myAudioSource.volume = anAudioItem.myAudioVolume * aVolume;
        myAudioSource.clip = anAudioItem.myAudioClip;
    }

    private void OnEnable()
    {
        if (myAudioSource.clip != null)
        {
            myAudioSource.Play();
        }
    }

    private void OnDisable()
    {
        myAudioSource.Stop();
    }
}

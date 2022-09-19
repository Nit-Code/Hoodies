using System.Collections.Generic;
using UnityEngine;
using SharedScripts.DataId;

[System.Serializable]
public class AudioData
{
    public string myName;
    public AudioId myId;

    public AudioClip myAudioClip;
    public string myAudioDescription;
    [Range(-80.0f, 20.0f)]
    public float myAudioVolume = 0.0f;
}

[CreateAssetMenu(fileName = "Audios_Inst", menuName = "DataListsInstances/Audios_Inst")]
public class Audios_Def : ScriptableObject
{
    public List<AudioData> myAudios;
}
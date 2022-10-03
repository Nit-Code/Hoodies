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

    // FIXME: This is hella ugly find a way to limit the contribution of this volume to under the master's current level.
    [Header("Hover for tooltip")]
    [Tooltip("Try to not exceede master's volume, if for example this is set to +20 and the master is set to -23, this will be heard loud and distorted.")]
    [Range(-80.0f, 20.0f)]
    public float myAudioVolume = 0.0f;
}

[CreateAssetMenu(fileName = "Audios_Inst", menuName = "DataListsInstances/Audios_Inst")]
public class Audios_Def : ScriptableObject
{
    public List<AudioData> myAudios;
}
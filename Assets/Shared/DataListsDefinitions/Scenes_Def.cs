using System.Collections.Generic;
using UnityEngine;
using SharedScripts;
using SharedScripts.DataId;

[System.Serializable]
public class SceneData
{
    public string myName;
    public SceneId myId;

    public string myScenePath;
    public AudioId myAudioId;
    public AudioId myAmbientAudioId;
}

[CreateAssetMenu(fileName = "Scenes_Inst", menuName = "DataListsInstances/Scenes_Inst")]
public class Scenes_Def : ScriptableObject
{
    public List<SceneData> myScenes;
}


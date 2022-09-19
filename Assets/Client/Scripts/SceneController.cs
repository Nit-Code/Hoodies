using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using SharedScripts;
using SharedScripts.DataId;

public class SceneController : MonoBehaviour
{
    [SerializeField] private Camera myCamera;
    private Dictionary<SceneId, SceneData> myScenesDataMap;

    [SerializeField] private SceneId myStartingScene; //TODO: candidate for GameConfigs_Def
    private SceneId myCurrentScene;
    private SharedDataLoader myDataLoaderReference;
    private bool myIsDataLoaded;

    private void Awake()
    {
        myCurrentScene = SceneId.INVALID;
        myDataLoaderReference = null;

        if (TryGetComponent<SharedDataLoader>(out myDataLoaderReference))
        {
            myScenesDataMap = myDataLoaderReference.GetAllScenesData();
            if (myScenesDataMap != null) 
            {
                myIsDataLoaded = true;
            }
        }
        else 
        {
            myIsDataLoaded = false;
            Debug.LogError("[HOOD][CLIENT][SCENE] - Data not found");
        }
    }

    private IEnumerator Start()
    {
        if (!myIsDataLoaded)
        {
            Debug.LogError("[HOOD][CLIENT][SCENE] - myStartingScene load requested before scenes data finished loading.");
            yield return null;
        }
        else 
        {
            yield return StartCoroutine(LoadSceneAndSetActive(myStartingScene));
            EventHandler.CallAfterSceneLoadEvent();
        }
    }

    public void LoadScene(SceneId anId)
    {
        if (!myIsDataLoaded) 
        {
            Debug.LogError("[CLIENT][HOOD][SCENE] - Load scene request before scenes data finished loading.");
            return;
        }

        Debug.Log("[HOOD][CLIENT][SCENE] - Load scene request: " + anId.ToString());
        StartCoroutine(SwitchScenes(anId));
    }

    public SceneId GetCurrentScene() 
    {
        return myCurrentScene;
    }

    private string GetScenePath(SceneId anId) 
    {
        string scenePath = null;
        if (myIsDataLoaded)
        {
            SceneData sceneData;
            if (myScenesDataMap.TryGetValue(anId, out sceneData))
            {
                scenePath = sceneData.myScenePath;
            }
        }
        return scenePath;
    }

    private IEnumerator SwitchScenes(SceneId anId)
    {
        EventHandler.CallBeforeSceneUnloadEvent();

        // Activate persistent scene camera so we always have a camera running
        myCamera.gameObject.SetActive(true);

        // Unload current active scene
        yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().buildIndex);

        yield return StartCoroutine(LoadSceneAndSetActive(anId));

        EventHandler.CallAfterSceneLoadEvent();

        if(anId == SceneId.MATCH)
        {
            EventHandler.CallAfterMatchSceneLoadEvent();
        }
    }

    private IEnumerator LoadSceneAndSetActive(SceneId anId)
    {
        string scenePath = GetScenePath(anId);
        if (scenePath != null)
        {
            // Additive adds the scene our persistent scene, or whichever scene is active and loaded.
            yield return SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Additive);

            // SceneManager.sceneCount - 1 is the most recently loaded scene
            Scene newlyLoadedScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);

            SceneManager.SetActiveScene(newlyLoadedScene);

            myCurrentScene = anId;

            // Deactivate persistent scene camera so the newly loaded scene's camera can take over
            myCamera.gameObject.SetActive(false);
        }
        else 
        {
            Debug.LogError("[HOOD][CLIENT][SCENE] - Scene not found");
        }
    }
}

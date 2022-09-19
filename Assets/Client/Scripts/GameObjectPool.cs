using System.Collections.Generic;
using UnityEngine;

/*
 * Pool managers create a pool of empty objects of a certain size which are populated when needed. 
 * When the maximus size is reached, the pool manager starts "recycling" objects. 
 * This way objects are not constantly created/destroyed.
 */ 

public class GameObjectPool : MonoBehaviour 
{
    [System.Serializable]
    public struct Pool
    {
        public int myPoolSize;
        public GameObject myPrefab;
    }

    private Dictionary<int, Queue<GameObject>> myInstanceIdToObjectMap;
    [SerializeField] private Pool[] myPoolsArray;
    [SerializeField] private Transform myPoolsRootTransform;

    private void Awake()
    {
        myInstanceIdToObjectMap = new Dictionary<int, Queue<GameObject>>();

        for (int i = 0; i < myPoolsArray.Length; i++)
        {
            CreatePool(myPoolsArray[i].myPrefab);
        }
    }

    private void CreatePool(GameObject aPrefab)
    {
        int poolKey = aPrefab.GetInstanceID();
        string prefabName = aPrefab.name;

        GameObject parentGameObject = CreateParentGameObject(prefabName);
        CreateGameObject(poolKey, aPrefab, parentGameObject);
    }

    private GameObject CreateParentGameObject(string aPrefabName)
    {
        GameObject parentGameObject = new GameObject(aPrefabName + "Anchor"); 

        parentGameObject.transform.SetParent(myPoolsRootTransform);

        return parentGameObject;
    }

    private void CreateGameObject(int aPoolKey, GameObject aPrefab, GameObject aParentGameObject)
    {
        if (!myInstanceIdToObjectMap.ContainsKey(aPoolKey)) 
        {
            myInstanceIdToObjectMap.Add(aPoolKey, new Queue<GameObject>());

            for (int i = 0; i < myPoolsArray.Length; i++)
            {
                GameObject newObject = Instantiate(aPrefab, aParentGameObject.transform) as GameObject;
                newObject.SetActive(false);

                myInstanceIdToObjectMap[aPoolKey].Enqueue(newObject);
            }
        }
    }

    public GameObject ReuseObject(GameObject aPrefab, Vector3 aPosition, Quaternion aRotation)
    {
        int poolKey = aPrefab.GetInstanceID();

        if (myInstanceIdToObjectMap.ContainsKey(poolKey))
        {
            GameObject objectToReuse = GetObjectFromPool(poolKey);
            ResetObject(aPosition, aRotation, objectToReuse, aPrefab);

            return objectToReuse;
        }
        else
        {
            Debug.LogError("[HOOD][CLIENT][POOL] - No object pool for: " + aPrefab);
            return null;
        }
    }

    private GameObject GetObjectFromPool(int aPoolKey)
    {
        GameObject objectToReuse = myInstanceIdToObjectMap[aPoolKey].Dequeue();
        myInstanceIdToObjectMap[aPoolKey].Enqueue(objectToReuse);

        if (objectToReuse.activeSelf == true)
        {
            objectToReuse.SetActive(false);
        }

        return objectToReuse;
    }

    private static void ResetObject(Vector3 aPosition, Quaternion aRotation, GameObject anObjectToReuse, GameObject aPrefab)
    {
        anObjectToReuse.transform.position = aPosition;
        anObjectToReuse.transform.rotation = aRotation;

        // Returns scale to normal, in case it was resized.
        anObjectToReuse.transform.localScale = aPrefab.transform.localScale; 
    }
}

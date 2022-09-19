using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharedScripts;
using SharedScripts.DataId;

// EXAMPLE:
// Use dont use inheritance in data classes when the base class can define all fields for all possible variations
// Add a Type enum if runtime classes that use this data have inheritance or certain behaviour that needs to be
// different for each variation while using the same data fields

[System.Serializable]
public class TileData
{
    public string myName;
    //public TileId myId;

    public TileType myType;
    public int myTravelCost;
    public bool myIsBlocked;
    public bool myCanSpawn;
}

[CreateAssetMenu(fileName = "Tiles_Inst", menuName = "DataListsInstances/Tiles_Inst")]
public class Tiles_Def : ScriptableObject
{
    [SerializeField] public List<TileData> myTiles;
}
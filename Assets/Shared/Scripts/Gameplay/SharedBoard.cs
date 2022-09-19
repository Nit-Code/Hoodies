using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SharedScripts;
using SharedScripts.DataId;

public class SharedBoard : MonoBehaviour
{
    private int mySeed;
    private int myHeight;
    private int myWidth;
    private int myNebulaQty;
    private int myBlackHoleQty;

    public int GetTileQuantity() { return myHeight * myWidth; }

    [SerializeField] private float myMargin;
    [SerializeField] private float myTileSize;
    [SerializeField] private Transform myCamera;

    private Dictionary<Vector2Int, SharedTile> myTileDictionary;
    private SharedGameManager myGameManagerReference;
    private SharedGameObjectFactory myFactoryReference;

    private int myDataToLoad;
    private int myDataLoaded;

    public struct GenerationInfo
    {
        public int seed;
        public int nebulaQty;
        public int blackHoleQty;
        public int width;
        public int height;

        public GenerationInfo(int aSeed, int aNebulaQty, int aBlackHoleQty, int aWidth, int aHeight)
        {
            seed = aSeed;
            nebulaQty = aNebulaQty;
            blackHoleQty = aBlackHoleQty;
            width = aWidth;
            height = aHeight;
        }
    }

    private void Start()
    {
        myDataToLoad = 2;
        myDataLoaded = 0;

        myGameManagerReference = FindObjectOfType<SharedGameManager>();
        if (myGameManagerReference != null)
        {
            myDataLoaded++;
        }

        myFactoryReference = FindObjectOfType<SharedGameObjectFactory>();
        if (myFactoryReference != null)
        {
            myDataLoaded++;
        }
    }

    private bool IsDataLoaded()
    {
        return myDataToLoad == myDataLoaded;
    }

    public void Init(int aSeed, int aNebulaQty, int aBlackHoleQty, int aWidth, int aHeight)
    {
        mySeed = aSeed;
        myHeight = aHeight;
        myWidth = aWidth;
        myNebulaQty = aNebulaQty;
        myBlackHoleQty = aBlackHoleQty;

        GridSetup();
    }

    public GenerationInfo GetGenerationInfo()
    {
        return new GenerationInfo(mySeed, myNebulaQty, myBlackHoleQty, myWidth, myHeight);
    }

    public void UndoBoardSelectionColors()
    {
        foreach (SharedTile tile in GetAllTiles())
        {
            tile.GoToBaseColor(true);
        }
    }

    public void ColorTileListTemporary(List<SharedTile> tilesToColor, TileColor color, bool changeFallbackColor)
    {
        foreach (SharedTile tile in tilesToColor)
        {
            tile.ColorTileTemporary(color, changeFallbackColor);
        }
    }

    public void ColorTileList(List<SharedTile> tilesToColor, TileColor color)
    {
        foreach (SharedTile tile in tilesToColor)
        {
            tile.ColorTile(color);
        }
    }

    public void ColorValidSpawnTiles()
    {
        List<SharedTile> spawnTiles = myGameManagerReference.GetValidSpawnTiles();
        ColorTileListTemporary(spawnTiles, TileColor.BLUE, true);
    }

    public void ColorAttackRange(SharedUnit anAttackingUnit)
    {
        List<SharedTile> attackingTiles = myGameManagerReference.GetValidAttackTiles(anAttackingUnit);
        ColorTileListTemporary(attackingTiles, TileColor.RED, true);
    }

    public void ColorPossibleMovementTiles(SharedUnit aMovingUnit)
    {
        List<SharedTile> possibleMovementTiles = myGameManagerReference.GetValidMovementRanges(aMovingUnit);
        ColorTileListTemporary(possibleMovementTiles, TileColor.YELLOW, true);
    }

    public void ColorAndSetPossibleCastingTiles()
    {
        ClientGameManager gameManager = myGameManagerReference as ClientGameManager;
        SharedUnit castingUnit = gameManager.GetSelectedUnit();

        myGameManagerReference.GetValidUnitCastingTiles(castingUnit);
        ColorTileListTemporary(gameManager.GetPossibleCastingTiles(), TileColor.BLUE, true);
    }

    public void ColorAndSetAbilityAreaTiles(SharedAbility anAbility, SharedTile aCenterTile, bool anIsValidCastingSpot, SharedTile aDirectionTile = null)
    {
        List<SharedTile> abilityAreaTiles = GetShapeFromCenterTileCoord(anAbility.GetIncludesCenter(), anAbility.GetShape(), anAbility.GetShapeSize(), aCenterTile.GetCoordinate());
        ClientGameManager gameManager = myGameManagerReference as ClientGameManager;

        if (anIsValidCastingSpot)
        {
            ColorTileListTemporary(abilityAreaTiles, anAbility.GetTileColor(), false);
            gameManager.SetCurrentCastingArea(abilityAreaTiles);
        }
        else
        {
            ColorTileListTemporary(abilityAreaTiles, TileColor.RED, false);
        }
    }

    private void GridSetup()
    {
        myTileDictionary = new Dictionary<Vector2Int, SharedTile>();
        GenerateGrid();

#if !UNITY_SERVER
        ChangeTileSizes();
        CenterCameraOnGrid();
#endif
    }

    private void ChangeTileSizes()
    {
        Vector3 sizeChange = new Vector3(myTileSize, 3, myTileSize);

        foreach(KeyValuePair<Vector2Int, SharedTile> entry in myTileDictionary)
        {
            entry.Value.transform.localScale = sizeChange;
        }
    }

    //private void GenerateGrid()
    //{
    //    if (!IsDataLoaded())
    //    {
    //        Shared.LogError("[HOOD][BOARD][ERROR] - GenerateGrid()");
    //        return;
    //    }

    //    int xMarginMultiplier = 0;

    //    for (int x = 0; x < myWidth; x++)
    //    {
    //        int yMarginMultiplier = 0;

    //        for (int y = 0; y < myHeight; y++)
    //        {
    //            TileType tileType = GetRandomTileType();

    //            float tileX = x * myTileSize + (myMargin * xMarginMultiplier);
    //            float tileY = 0;
    //            float tileZ = y * myTileSize + (myMargin * yMarginMultiplier);
    //            Vector3 tilePosition = new Vector3(tileX, tileY, tileZ);
    //            Vector2Int tileCoord = new Vector2Int(x + 1, y + 1); //We add 1 to make it more intuitive
    //            string tileName = $"SharedTile {tileCoord}";

    //            SharedTile spawnedTile = myFactoryReference.CreateTileByType(transform, tileCoord, tilePosition, tileType, 0);

    //            //TODO: add this to SharedTile/Init()
    //            spawnedTile.name = tileName;

    //            myTileDictionary[tileCoord] = spawnedTile;
    //            yMarginMultiplier++;
    //        }

    //        xMarginMultiplier++;
    //    }
    //}

    private void GenerateGrid()
    {
        if (!IsDataLoaded() || myBlackHoleQty < 0 || myBlackHoleQty < 0)
        {
            Shared.LogError("[HOOD][BOARD][ERROR] - GenerateGrid()");
            return;
        }

        int nebulaToPlace = myNebulaQty;
        int blackHoleToPlace = myBlackHoleQty;
        List<TileType> tileTypeList = new();

        for (int i = 0; i < myWidth * myHeight; i++)
        {
            TileType tileType = SelectTile(nebulaToPlace, blackHoleToPlace);

            if (tileType == TileType.NEBULA)
                nebulaToPlace--;
            if (tileType == TileType.BLACKHOLE)
                blackHoleToPlace--;

            tileTypeList.Add(tileType);
        }

        tileTypeList = myGameManagerReference.ShuffleItemsWithSeed(tileTypeList.ToArray(), mySeed).ToList();
        InstantiateBoardWithCoords(ConvertToListList(tileTypeList));
    }

    private TileType SelectTile(int nebulasToPlace, int blackHolesToPlace)
    {
        if (nebulasToPlace > 0)
        {
            return TileType.NEBULA;
        }

        if (blackHolesToPlace > 0)
        {
            return TileType.BLACKHOLE;
        }

        return TileType.EMPTY;
    }

    private List<List<TileType>> ConvertToListList(List<TileType> aListToConvert)
    {
        int dividerIndex = 0;
        List<TileType> listToAdd = new();
        List<List<TileType>> returnList = new();

        foreach (TileType tile in aListToConvert)
        {
            dividerIndex++;
            listToAdd.Add(tile);

            if (dividerIndex == myHeight)
            {
                dividerIndex = 0;
                returnList.Add(new List<TileType>(listToAdd));
                listToAdd.Clear();
                continue;
            }
        }
        return returnList;
    }

    private void InstantiateBoardWithCoords(List<List<TileType>> aTileTypeListList)
    {
        int x = 0;

        foreach (List<TileType> tileTypeList in aTileTypeListList)
        {
            int y = 0;

            foreach (TileType tileType in tileTypeList)
            {
                float tileX = x * myTileSize + (myMargin * x);
                float tileY = 0;
                float tileZ = y * myTileSize + (myMargin * y);
                Vector3 tilePosition = new Vector3(tileX, tileY, tileZ);
                Vector2Int tileCoord = new Vector2Int(x + 1, y + 1); //We add 1 to make it more intuitive
                string tileName = $"SharedTile {tileCoord}";
                SharedTile spawnedTile;

                if ((x == 0 && y == myHeight / 2) || (x == myWidth - 1 && y == myHeight / 2))
                {
                    spawnedTile = myFactoryReference.CreateTileByType(transform, tileCoord, tilePosition, TileType.EMPTY, 0); //Mothership starting places should always be empty
                }
                else
                {
                    spawnedTile = myFactoryReference.CreateTileByType(transform, tileCoord, tilePosition, tileType, 0);
                }

                spawnedTile.name = tileName;
                myTileDictionary.Add(tileCoord, spawnedTile);

                y++;
            }
            x++;
        }
    }


    //private TileType GetRandomTileType()
    //{
    //    int randomNum = new System.Random(mySeed).Next(1, 100);

    //    switch (randomNum)
    //    {
    //        case <= 10:
    //            return TileType.BLACKHOLE; // 10 % chance of BH
    //        case <= 20:
    //            return TileType.NEBULA; // 10 % chance of NB
    //        default:
    //            return TileType.EMPTY; // 80 % chance of empty
    //    }
    //}

    private void CenterCameraOnGrid()
    {
        Vector3 boardXCenter;

        if (myWidth % 2 != 0)
        {
            int centralTileX = myWidth / 2 + 1; // casting always rounds DOWN

            boardXCenter = new Vector3(GetTile(centralTileX, 1).transform.position.x, 0, 0);
        }
        else
        {
            int centralTile1X = ((int)myWidth / 2);
            int centralTile2X = (int)myWidth / 2 + 1;
            float centralX = (GetTile(centralTile1X, 1).transform.position.x + GetTile(centralTile2X, 1).transform.position.x) / 2;

            boardXCenter = new Vector3(centralX, 0, 0);
        }

        myCamera.transform.position += boardXCenter;
    }

    public SharedTile GetTile(Vector2Int aPos)
    {
        if (myTileDictionary.TryGetValue(aPos, out SharedTile tile))
        {
            return tile;
        }

        return null;
    }

    public SharedTile GetTile(int x, int y)
    {
        Vector2Int pos = new Vector2Int(x, y);

        if (myTileDictionary.TryGetValue(pos, out SharedTile tile))
        {
            return tile;
        }

        return null;
    }

    public List<SharedTile> GetAllTiles()
    {
        return myTileDictionary.Values.ToList(); ;
    }

    public SharedTile GetMotherShip1SpawnTile()
    {
        return GetTile(1, (myHeight / 2) + 1);

    }

    public SharedTile GetMotherShip2SpawnTile()
    {
        return GetTile(myWidth, (myHeight / 2) + 1);
    }

    public void DestroyAllTiles()
    {
        foreach (KeyValuePair<Vector2Int, SharedTile> entry in myTileDictionary)
        {
            Destroy(entry.Value.gameObject);
        }
        myTileDictionary.Clear();
    }

    public List<SharedTile> GetShapeFromCenterTileCoord(bool includeCenter, AreaShape areaShape, int shapeSize, Vector2Int centerTileCoord, Vector2Int? directionTileCoord = default) // Making vector2Int nullable always assigns null for some reason
    {
        List<SharedTile> returnList = new List<SharedTile>();

        switch (areaShape)
        {
            case AreaShape.SQUARE:
                returnList = GetSquareFromCentralCoord(shapeSize, centerTileCoord, includeCenter);
                break;
            case AreaShape.CROSS:
                returnList = GetCrossFromCentralCoord(shapeSize, centerTileCoord, includeCenter);
                break;
            case AreaShape.LINE:
                returnList = GetLineFromCentralCoord(shapeSize, centerTileCoord, directionTileCoord.Value, includeCenter);
                break;
            default:
                returnList.Add(GetTile(centerTileCoord.x, centerTileCoord.y));
                break;
        }
        return returnList;
    }

    private List<SharedTile> GetSquareFromCentralCoord(int shapeSize, Vector2Int centerTileCoord, bool includeCenter)
    {
        List<SharedTile> tiles = new List<SharedTile>();
        for (int x = centerTileCoord.x - shapeSize; x <= centerTileCoord.x + shapeSize; x++)
        {
            for (int y = centerTileCoord.y - shapeSize; y <= centerTileCoord.y + shapeSize; y++)
            {
                if (x != centerTileCoord.x || y != centerTileCoord.y)
                {
                    SharedTile tileToAdd = GetTile(x, y);
                    if (tileToAdd != null)
                    {
                        tiles.Add(tileToAdd);
                    }
                }
            }
        }
        if (includeCenter)
        {
            tiles.Add(GetTile(centerTileCoord.x, centerTileCoord.y));
        }

        return tiles;
    }
    private List<SharedTile> GetCrossFromCentralCoord(int shapeSize, Vector2Int centerTileCoord, bool includeCenter)
    {
        List<SharedTile> tiles = new List<SharedTile>();
        for (int x = centerTileCoord.x - shapeSize; x <= centerTileCoord.x + shapeSize; x++)
        {
            for (int y = centerTileCoord.y - shapeSize; y <= centerTileCoord.y + shapeSize; y++)
            {
                if (x == centerTileCoord.x || y == centerTileCoord.y)
                {
                    SharedTile tileToAdd = GetTile(x, y);
                    if (tileToAdd != null)
                    {
                        tiles.Add(tileToAdd);
                    }

                }

            }
        }
        if (!includeCenter)
        {
            tiles.Remove(GetTile(centerTileCoord.x, centerTileCoord.y));
        }

        return tiles;
    }

    private List<SharedTile> GetLineFromCentralCoord(int shapeSize, Vector2Int centerTileCoord, Vector2Int directionTileCoord, bool includeCenter)
    {
        List<SharedTile> tiles = new List<SharedTile>();

        if (directionTileCoord != null)
        {
            Vector2Int netDirectionVector = (Vector2Int)(centerTileCoord - directionTileCoord);

            if (netDirectionVector.x < 0) // Rightwards direction
            {
                for (int x = centerTileCoord.x; x <= centerTileCoord.x + shapeSize; x++)
                {
                    SharedTile tileToAdd = GetTile(x, centerTileCoord.y);
                    if (tileToAdd != null)
                    {
                        tiles.Add(tileToAdd);
                    }
                }
            }
            else if (netDirectionVector.x > 0) // Leftwards direction
            {
                for (int x = centerTileCoord.x; x >= centerTileCoord.x - shapeSize; x--)
                {
                    SharedTile tileToAdd = GetTile(x, centerTileCoord.y);
                    if (tileToAdd != null)
                    {
                        tiles.Add(tileToAdd);
                    }
                }
            }
            else if (netDirectionVector.y < 0) // Upwards direction
            {
                for (int y = centerTileCoord.y; y <= centerTileCoord.y + shapeSize; y++)
                {
                    SharedTile tileToAdd = GetTile(centerTileCoord.x, y);
                    if (tileToAdd != null)
                    {
                        tiles.Add(tileToAdd);
                    }
                }
            }
            else if (netDirectionVector.y > 0) // Downwards direction
            {
                for (int y = centerTileCoord.y; y >= centerTileCoord.y - shapeSize; y--)
                {
                    SharedTile tileToAdd = GetTile(centerTileCoord.x, y);
                    if (tileToAdd != null)
                    {
                        tiles.Add(tileToAdd);
                    }
                }
            }
        }
        if (!includeCenter)
        {
            tiles.Remove(GetTile(centerTileCoord.x, centerTileCoord.y));
        }

        return tiles;
    }

    public List<SharedTile> GetAdjacentTiles(Vector2Int centerTileCoord)
    {
        return GetCrossFromCentralCoord(1, centerTileCoord, false);
    }

    public bool IsMapFullyAccessible()
    {
        SharedTile spawnedTile = GetMotherShip1SpawnTile();

        if (GetMotherShip1SpawnTile().GetIsBlocked())
        {
            return false;
        }

        SharedUnit testingUnit = myGameManagerReference.ForceSpawnAuxiliaryUnit(spawnedTile); // Create special method to force spawn
        myGameManagerReference.RemoveUnitFromDictionary(testingUnit.GetMatchId());

        testingUnit.ModifyMovementRange(1000); // Hard coded for now

        List<SharedTile> tileMap = new List<SharedTile>();
        List<SharedTile> validMovementTiles = myGameManagerReference.GetValidMovementRanges(testingUnit);

        foreach (KeyValuePair<Vector2Int, SharedTile> entry in myTileDictionary)
        {
            SharedTile tile = entry.Value;

            if (tile.GetTileType() != TileType.BLACKHOLE && tile.GetCoordinate() != testingUnit.GetPosition())
            {
                tileMap.Add(tile);
            }
        }

        myGameManagerReference.KillUnit(testingUnit);
        Destroy(testingUnit.gameObject);

        return tileMap.All(x => validMovementTiles.Any(y => x == y)); // If all tiles in my board that aren't blackHoles are accessible
    }
}

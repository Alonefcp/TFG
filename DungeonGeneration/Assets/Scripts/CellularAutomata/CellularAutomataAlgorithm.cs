using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct TileCoord
{
    public TileCoord(int x, int y)
    {
        posX = x;
        posY = y;
    }

    public int posX;
    public int posY;
}

public class CellularAutomataAlgorithm : DungeonGenerator
{
    enum Neighborhood {Moore, VonNeummann}
    enum TileType {Wall = 1, Floor = 0}

    [Range(30,200)]
    [SerializeField] private int mapWidth = 80, mapHeight = 60;
    [Range(1, 20)]
    [SerializeField] private int iterations = 5;
    [Range(0.0f, 1.0f)]
    [SerializeField] private float fillPercent = 0.45f;
    [SerializeField] private Neighborhood neighborhood = Neighborhood.Moore;
    [Range(0, 100)]
    [SerializeField] private int wallThresholdSize = 50;
    [Range(0, 100)]
    [SerializeField] private int floorThresholdSize = 50;
    private TileType[,] map;

    //void Start()
    //{
    //    GenerateDungeon();
    //}

    public override void GenerateDungeon()
    {
        tilemapVisualizer.ClearTilemap();

        map = GenerateNoise();    
        CellularAutomata();
        EraseRegions(TileType.Wall, wallThresholdSize);
        EraseRegions(TileType.Floor, floorThresholdSize);
    }

    private TileType[,] GenerateNoise()
    {
        TileType[,] map = new TileType[mapWidth, mapHeight]; // 1: wall , 0: floor

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                if (x == 0 || x == mapWidth - 1 || y == 0 || y == mapHeight - 1)
                {
                    tilemapVisualizer.PaintSingleWallTile(new Vector2Int(x, y));

                    map[x, y] = TileType.Wall;
                }
                else
                {
                    map[x, y] = (Random.Range(0.0f, 1.0f) < fillPercent) ? TileType.Wall : TileType.Floor;

                    if (map[x, y] == TileType.Wall) tilemapVisualizer.PaintSingleWallTile(new Vector2Int(x, y));
                    else tilemapVisualizer.PaintSingleFloorTile(new Vector2Int(x, y));
                }
            }
        }

        return map;
    }

    private void CellularAutomata()
    {
        for (int i = 0; i < iterations; i++)
        {
            TileType[,] mapClone = (TileType[,])map.Clone();
            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    int neighbourWallTiles = GetWallNeighbours(x, y);

                    if (neighborhood == Neighborhood.Moore) MooreAutomata(mapClone, x, y, neighbourWallTiles);
                    else VonNeummannAutomata(mapClone, x, y, neighbourWallTiles);

                }
            }
            map = (TileType[,])mapClone.Clone();
        }    
    }

    private void MooreAutomata(TileType[,] map, int x, int y,int neighbourWallTiles)
    {
        if (neighbourWallTiles > 4) //>=5 , > 4
        {
            map[x, y] = TileType.Wall;
            tilemapVisualizer.PaintSingleWallTile(new Vector2Int(x, y));
        }
        else if (neighbourWallTiles < 4)
        {
            map[x, y] = TileType.Floor;
            tilemapVisualizer.PaintSingleFloorTile(new Vector2Int(x, y));
        }
    }

    private void VonNeummannAutomata(TileType[,] map, int x, int y, int neighbourWallTiles)
    {
        if (neighbourWallTiles > 2)
        {
            map[x, y] = TileType.Wall;
            tilemapVisualizer.PaintSingleWallTile(new Vector2Int(x, y));
        }
        else if (neighbourWallTiles < 2)
        {
            map[x, y] = TileType.Floor;
            tilemapVisualizer.PaintSingleFloorTile(new Vector2Int(x, y));
        }
    }

    private int GetWallNeighbours(int X, int Y)
    {
        Vector2Int[] directions = (neighborhood == Neighborhood.Moore) ? GetEightDiretionsArray() : GetFourDirectionsArray();
        int nWalls = 0;
        foreach (Vector2Int dir in directions)
        {
            int neighbourX = X + dir.x;
            int neighbourY = Y + dir.y;

            if (neighbourX >= 0 && neighbourX < mapWidth && neighbourY >= 0 && neighbourY < mapHeight)
            {
                nWalls += (int)map[neighbourX, neighbourY];
            }
            else
            {
                nWalls++;
            }
        }

        return nWalls;
    }

    private void EraseRegions(TileType regionsToErase, int regionThresholdSize)
    {
        List<List<TileCoord>> wallRegions = GetRegions(regionsToErase);
        
        foreach (List<TileCoord> wallRegion in wallRegions)
        {
            if (wallRegion.Count <= regionThresholdSize)
            {
                foreach (TileCoord tile in wallRegion)
                {
                    map[tile.posX, tile.posY] = (regionsToErase==TileType.Wall)? TileType.Floor: TileType.Wall;
                    if (regionsToErase == TileType.Wall) tilemapVisualizer.PaintSingleFloorTile(new Vector2Int(tile.posX, tile.posY));
                    else tilemapVisualizer.PaintSingleWallTile(new Vector2Int(tile.posX, tile.posY));
                }
            }
        }       
    }

    private List<List<TileCoord>> GetRegions(TileType tileType)
    {
        List<List<TileCoord>> regions = new List<List<TileCoord>>();
        bool[,] visitedTiles = new bool[mapWidth, mapHeight]; // true: visited , false: not visited

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                if (!visitedTiles[x, y] && map[x, y] == tileType)
                {
                    List<TileCoord> newRegion = GetRegionTiles(x, y);
                    regions.Add(newRegion);

                    foreach (TileCoord tile in newRegion)
                    {
                        visitedTiles[tile.posX, tile.posY] = true;
                    }
                }
            }
        }

        return regions;
    }

    private List<TileCoord> GetRegionTiles(int startX, int startY)
    {
        List<TileCoord> tiles = new List<TileCoord>();
        bool[,] visitedTiles = new bool[mapWidth, mapHeight]; // true: visited , false: not visited
        TileType tileType = map[startX, startY];

        Queue<TileCoord> queue = new Queue<TileCoord>();
        queue.Enqueue(new TileCoord(startX, startY));
        visitedTiles[startX, startY] = true;

        while (queue.Count > 0)
        {
            TileCoord tile = queue.Dequeue();
            tiles.Add(tile);

            Vector2Int[] fourDirectionsArray = GetFourDirectionsArray();
            foreach (Vector2Int dir in fourDirectionsArray)
            {
                int neighbourX = tile.posX + dir.x;
                int neighbourY = tile.posY + dir.y;

                if (neighbourX >= 0 && neighbourX < mapWidth && neighbourY >= 0 && neighbourY < mapHeight)
                {
                    if (!visitedTiles[neighbourX, neighbourY] && map[neighbourX, neighbourY] == tileType)
                    {
                        visitedTiles[neighbourX, neighbourY] = true;
                        queue.Enqueue(new TileCoord(neighbourX, neighbourY));
                    }
                }
            }
        }

        return tiles;
    }

    private Vector2Int[] GetEightDiretionsArray()
    {
        Vector2Int[] directions = { new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1),
            new Vector2Int(1, 1), new Vector2Int(-1, 1), new Vector2Int(1, -1), new Vector2Int(-1, -1)};

        return directions;
    }

    private Vector2Int[] GetFourDirectionsArray()
    {
        Vector2Int[] directions = { new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1)};
            
        return directions;
    }
}

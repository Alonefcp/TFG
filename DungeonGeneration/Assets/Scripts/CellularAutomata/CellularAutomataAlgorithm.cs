using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellularAutomataAlgorithm : DungeonGenerator
{
    enum Neighborhood { Moore, VonNeummann}

    [Range(30,200)]
    [SerializeField] private int mapWidth = 80, mapHeight = 60;

    [SerializeField] private int iterations = 5;
    [Range(0.0f, 1.0f)]
    [SerializeField] private float fillPercent = 0.45f;
    [SerializeField] private Neighborhood neighborhood = Neighborhood.Moore;
    [SerializeField] private bool eliminateSingleWalls = false;
    [SerializeField] private bool eliminateSingleFloors = false;

    //void Start()
    //{
    //    GenerateDungeon();
    //}

    public override void GenerateDungeon()
    {
        tilemapVisualizer.ClearTilemap();

        int[,] map = GenerateNoise();    
        CellularAutomata(map);

        if (eliminateSingleWalls) tilemapVisualizer.EliminateSingleWalls();
        if (eliminateSingleFloors) tilemapVisualizer.EliminateSingleFloors();
    }


    private int[,] GenerateNoise()
    {
        int[,] map = new int[mapWidth, mapHeight]; // 1: wall , 0: floor

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                if (x == 0 || x == mapWidth - 1 || y == 0 || y == mapHeight - 1)
                {
                    tilemapVisualizer.PaintSingleWallTile(new Vector2Int(x, y));
                    map[x, y] = 1;
                }
                else
                {
                    map[x, y] = (Random.Range(0.0f, 1.0f) < fillPercent) ? 1 : 0;

                    if (map[x, y] == 1) tilemapVisualizer.PaintSingleWallTile(new Vector2Int(x, y));
                    else tilemapVisualizer.PaintSingleFloorTile(new Vector2Int(x, y));
                }
            }
        }

        return map;
    }

    private void CellularAutomata(int[,] map)
    {
        for (int i = 0; i < iterations; i++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    int neighbourWallTiles = GetWallNeighbourCount(map, x, y);

                    if (neighborhood == Neighborhood.Moore) MooreAutomata(map, x, y, neighbourWallTiles);
                    else VonNeummannAutomata(map, x, y, neighbourWallTiles);

                }
            }
        }    
    }

    private void MooreAutomata(int[,] map, int x, int y,int neighbourWallTiles)
    {
        if (neighbourWallTiles > 4) //>=5
        {
            map[x, y] = 1;
            tilemapVisualizer.PaintSingleWallTile(new Vector2Int(x, y));
        }
        else if (neighbourWallTiles < 4)
        {
            map[x, y] = 0;
            tilemapVisualizer.PaintSingleFloorTile(new Vector2Int(x, y));
        }
    }

    private void VonNeummannAutomata(int[,] map, int x, int y, int neighbourWallTiles)
    {
        if (neighbourWallTiles > 2)
        {
            map[x, y] = 1;
            tilemapVisualizer.PaintSingleWallTile(new Vector2Int(x, y));
        }
        else if (neighbourWallTiles < 2)
        {
            map[x, y] = 0;
            tilemapVisualizer.PaintSingleFloorTile(new Vector2Int(x, y));
        }
    }

    private int GetWallNeighbourCount(int[,] map, int X, int Y)
    {
        Vector2Int[] directions = (neighborhood == Neighborhood.Moore) ? GetEightDiretionsArray() : GetFourDirectionsArray();
        int nWalls = 0;
        foreach (Vector2Int dir in directions)
        {
            int neighbourX = X + dir.x;
            int neighbourY = Y + dir.y;

            if (neighbourX >= 0 && neighbourX < mapWidth && neighbourY >= 0 && neighbourY < mapHeight)
            {

                nWalls += map[neighbourX, neighbourY];

            }
            else
            {
                nWalls++;
            }
        }

        return nWalls;
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

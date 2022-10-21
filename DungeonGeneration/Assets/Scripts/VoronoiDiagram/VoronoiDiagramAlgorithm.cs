using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Random = UnityEngine.Random;

public struct Cell
{
    public Vector2Int cellPos;
    public Vector2Int belongSeed; //semilla a la que esta mas cerca
    public Cell(Vector2Int _cellPos)
    {
        cellPos = _cellPos;
        
        belongSeed = new Vector2Int();
    }

    public void SetBelongSeed(Vector2Int seed) 
    {
        belongSeed = seed;
    }
    public void SetCellPos(Vector2Int pos)
    {
        cellPos = pos;
    }
}

public class VoronoiDiagramAlgorithm : DungeonGenerator
{
    public enum DistanceAlgorithm {Euclidean, Manhattan}

    [SerializeField] private int mapWidth = 40, mapHeight = 40;
    [SerializeField] private int numberOfSeeds = 64;
    [SerializeField] private DistanceAlgorithm distanceAlgorithm;
    [Range(0.0f,1.0f)]
    [SerializeField] private float chance = 0.5f;
    [SerializeField] private bool randomShape = false;

    void Start()
    {
        GenerateDungeon();
    }

    public override void GenerateDungeon()
    {
        HashSet<Vector2Int> seeds = GenerateSeeds();
        List<Cell> mapInfo = VoronoiDiagram(seeds);
        tilemapVisualizer.ClearTilemap();
        CreateWalls(mapInfo);

        //HashSet<Vector2Int> seeds = GenerateSeeds1(out HashSet<Vector2Int> borderSeeds);
        //List<Cell> mapInfo = VoronoiDiagram(seeds);
        //tilemapVisualizer.ClearTilemap();




        //foreach (Vector2Int seed in seeds)
        //{
        //    tilemapVisualizer.PaintSingleFloorTileWithColor(seed, Color.red);
        //}

        //CreateWalls(mapInfo);
        //Dictionary<Vector2Int, HashSet<Vector2Int>> sets = CreateSets(borderSeeds, mapInfo);

        //for (int i = 0; i < sets.Count; i++)
        //{
        //    tilemapVisualizer.EraseTiles(sets[borderSeeds.ElementAt(i)]);

        //    foreach (Vector2Int seed in sets[borderSeeds.ElementAt(i)])
        //    {
        //        tilemapVisualizer.PaintSingleFloorTileWithColor(seed, Color.green);
        //    }
        //}
    }

    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawWireCube(new Vector3(mapWidth / 2, mapHeight / 2), new Vector3(mapWidth, mapHeight));
    //}

    private HashSet<Vector2Int> GenerateSeeds()
    {
        HashSet<Vector2Int> seeds = new HashSet<Vector2Int>();

        while(seeds.Count<numberOfSeeds)
        {
            int xPos = Random.Range(1, mapWidth-1);
            int yPos = Random.Range(1, mapHeight-1);

            seeds.Add(new Vector2Int(xPos, yPos));
        }

        return seeds;
    }

    private HashSet<Vector2Int> GenerateSeeds1(out HashSet<Vector2Int> borderSeeds)
    {
        HashSet<Vector2Int> seeds = new HashSet<Vector2Int>();

        borderSeeds = new HashSet<Vector2Int>();

        int offsetX = mapWidth / 12;
        int offsetY = mapHeight / 12;

        for (int i = 0; i < mapHeight - offsetY; i += offsetY)
        {
            borderSeeds.Add(new Vector2Int(offsetX, offsetY + i));
        }

        for (int i = 0; i < mapHeight - offsetY; i += offsetY)
        {           
            borderSeeds.Add(new Vector2Int(mapWidth - offsetX, offsetY + i));
        }

        for (int i = 0; i < mapWidth - offsetX; i += offsetX)
        {
            borderSeeds.Add(new Vector2Int(offsetX + i, offsetY));
        }

        for (int i = 0; i < mapWidth - offsetX; i += offsetX)
        {
            borderSeeds.Add(new Vector2Int(offsetX + i, mapHeight - offsetY));
            
        }

        while (seeds.Count < numberOfSeeds)
        {
            int xPos = Random.Range(offsetX + 1, mapWidth - offsetX);
            int yPos = Random.Range(offsetY + 1, mapHeight - offsetY);

            seeds.Add(new Vector2Int(xPos, yPos));
        }

        seeds.UnionWith(borderSeeds);
        return seeds;
    }

    private List<Cell> VoronoiDiagram(HashSet<Vector2Int> seeds)
    {
        List<Cell> mapInfo = new List<Cell>(mapWidth * mapHeight);
        for (int i = 0; i < mapWidth * mapHeight; i++) mapInfo.Add(new Cell());


        for (int i = 0; i < mapInfo.Count; i++)
        {
            int x = i % mapWidth;
            int y = i / mapWidth;

            float mindistance = float.MaxValue;

            foreach (Vector2Int seed in seeds)
            {
                float distance = (distanceAlgorithm == DistanceAlgorithm.Euclidean) ? EuclideanDistance(new Vector2Int(x, y), seed) : ManhattanDistance(new Vector2Int(x, y), seed); ;

                if (distance < mindistance)
                {
                    mindistance = distance;
                    Vector2Int nearestSeed = seed;
                    Cell cell = new Cell();
                    cell.SetCellPos(new Vector2Int(x, y));
                    cell.SetBelongSeed(nearestSeed);
                    mapInfo[i] = cell;
                }

            }

        }

        return mapInfo;
    }

    private void CreateWalls(List<Cell> mapInfo)
    {
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                Cell mySeed = mapInfo[MapXYtoIndex(x, y)];

                if (x == 0 || y == 0 || x == mapWidth - 1 || y == mapHeight - 1) //borders
                {
                    tilemapVisualizer.PaintSingleCorridorTile(new Vector2Int(x, y));
                    continue;
                }

                if (mapInfo[MapXYtoIndex(x - 1, y)].belongSeed != mySeed.belongSeed ||
                mapInfo[MapXYtoIndex(x + 1, y)].belongSeed != mySeed.belongSeed ||
                mapInfo[MapXYtoIndex(x, y - 1)].belongSeed != mySeed.belongSeed ||
                mapInfo[MapXYtoIndex(x, y + 1)].belongSeed != mySeed.belongSeed ||

                mapInfo[MapXYtoIndex(x - 1, y - 1)].belongSeed != mySeed.belongSeed ||
                mapInfo[MapXYtoIndex(x + 1, y + 1)].belongSeed != mySeed.belongSeed ||
                mapInfo[MapXYtoIndex(x - 1, y - 1)].belongSeed != mySeed.belongSeed ||
                mapInfo[MapXYtoIndex(x + 1, y + 1)].belongSeed != mySeed.belongSeed)
                {
                    if (Random.value > chance) tilemapVisualizer.PaintSingleCorridorTile(new Vector2Int(x, y));
                    else tilemapVisualizer.PaintSingleFloorTile(new Vector2Int(x, y));
                }
                else
                {
                    tilemapVisualizer.PaintSingleFloorTile(new Vector2Int(x, y));
                }

            }
        }
    }

    private Dictionary<Vector2Int, HashSet<Vector2Int>> CreateSets(HashSet<Vector2Int> borderSeeds, List<Cell> mapInfo)
    {
        Dictionary<Vector2Int, HashSet<Vector2Int>> sets = new Dictionary<Vector2Int, HashSet<Vector2Int>>();

        foreach (Vector2Int seed in borderSeeds)
        {
            /*if (!sets.ContainsKey(seed)) */
            sets.Add(seed, new HashSet<Vector2Int>());
        }

        foreach (Cell cell in mapInfo)
        {
            foreach (Vector2Int seed in borderSeeds)
            {
                if (cell.belongSeed == seed)
                {
                    sets[seed].Add(cell.cellPos);
                }
            }
        }

        return sets;
    }

    private int MapXYtoIndex(int x, int y)
    {
        return x + (y * mapWidth);
    }

    private float ManhattanDistance(Vector2Int from, Vector2Int to)
    {
        return Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y);
    }

    private float EuclideanDistance(Vector2Int from, Vector2Int to) 
    {
        return Math.Abs((from - to).magnitude);
	}
}
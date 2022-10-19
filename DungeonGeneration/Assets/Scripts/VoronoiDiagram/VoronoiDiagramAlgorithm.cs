using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Random = UnityEngine.Random;

public struct Cell: IComparable<Cell>
{
    public Vector2Int seed;
    public float distance;

    public Cell(Vector2Int _seed, float _distance)
    {
        seed = _seed;
        distance = _distance;
    }

    public int CompareTo(Cell other)
    {
        if(distance > other.distance)
        {
            return 1;
        }
        else if(distance < other.distance)
        {
            return -1;
        }
        else
        {
            return 0;
        }
    }
}

public class VoronoiDiagramAlgorithm : DungeonGenerator
{
    public enum DistanceAlgorithm {Euclidean, Manhattan}

    [SerializeField] private int mapWidth = 40, mapHeight = 40;
    [SerializeField] private int numberOfSeeds = 64;
    [SerializeField] private DistanceAlgorithm distanceAlgorithm;

    void Start()
    {
        GenerateDungeon();
    }

    public override void GenerateDungeon()
    {
        HashSet<Vector2Int> seeds = GenerateSeeds();

        List<Cell> distances = new List<Cell>();
        List<Cell> mapInfo = new List<Cell>(mapWidth * mapHeight);
        for (int i = 0; i < mapWidth*mapHeight; i++) mapInfo.Add(new Cell());


        for (int i = 0; i < mapInfo.Count; i++)
        {
            int x = i % mapWidth;
            int y = i / mapWidth;

            foreach (Vector2Int seed in seeds)
            {
                float distance = 0;
                if(distanceAlgorithm == DistanceAlgorithm.Euclidean)
                {
                    distance = EuclideanDistance(seed, new Vector2Int(x, y));
                }
                else if(distanceAlgorithm == DistanceAlgorithm.Manhattan)
                {
                    distance = ManhattanDistance(seed, new Vector2Int(x, y));
                }

                distances.Add(new Cell(seed, distance));
            }

            distances.Sort();
            mapInfo[i] = distances[0];
            distances.Clear();
        }

        tilemapVisualizer.ClearTilemap();

        for (int y = 1; y < mapHeight - 1; y++)
        {
            for (int x = 1; x < mapWidth - 1; x++)
            {
                int neighbors = 0;

                int tileIndex = MapXYtoIndex(x, y);
                Vector2Int mySeed = mapInfo[tileIndex].seed;

                if (mapInfo[MapXYtoIndex(x - 1, y)].seed != mySeed) { neighbors += 1; }
                if (mapInfo[MapXYtoIndex(x + 1, y)].seed != mySeed) { neighbors += 1; }
                if (mapInfo[MapXYtoIndex(x, y - 1)].seed != mySeed) { neighbors += 1; }
                if (mapInfo[MapXYtoIndex(x, y + 1)].seed != mySeed) { neighbors += 1; }

                if (neighbors < 2)
                {
                    tilemapVisualizer.PaintSingleFloorTile(new Vector2Int(x, y));
                }
                else
                {
                    tilemapVisualizer.PaintSingleCorridorTile(new Vector2Int(x, y));
                }
            }
        }

        //tilemapVisualizer.PaintCorridorTiles(seeds);
    }
    

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

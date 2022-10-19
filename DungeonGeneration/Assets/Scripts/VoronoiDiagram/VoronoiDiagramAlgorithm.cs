using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Random = UnityEngine.Random;

public struct Tile: IComparable<Tile>
{
    public Vector2Int seed;
    public float distance;

    public Tile(Vector2Int _seed, float _distance)
    {
        seed = _seed;
        distance = _distance;
    }

    public int CompareTo(Tile other)
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
    [SerializeField] private int mapWidth = 40, mapHeight = 40;
    [SerializeField] private int numberOfSeeds = 64;

    void Start()
    {
        GenerateDungeon();
    }

    public override void GenerateDungeon()
    {
        HashSet<Vector2Int> seeds = GenerateSeeds();
        // Dictionary<Vector2Int, float> voronoi_distance = new Dictionary<Vector2Int, float>(numberOfSeeds);
        List<Tile> voronoi_distance = new List<Tile>();
        List<Tile> voronoi_membership = new List<Tile>(mapWidth * mapHeight);
        for (int i = 0; i < mapWidth*mapHeight; i++)
        {
            voronoi_membership.Add(new Tile());
        }


        for (int i = 0; i < voronoi_membership.Count; i++)
        {
            int x = i % mapWidth;
            int y = i / mapWidth;

            foreach (Vector2Int seed in seeds)
            {
                float distance = Vector2Int.Distance(seed, new Vector2Int(x, y));

                //voronoi_distance[seed] = distance;
                voronoi_distance.Add(new Tile(seed, distance));
            }


            voronoi_distance.Sort();
            voronoi_membership[i] = voronoi_distance[0];
            voronoi_distance.Clear();
        }

        tilemapVisualizer.ClearTilemap();

        for (int y = 1; y < mapHeight - 1; y++)
        {
            for (int x = 1; x < mapWidth - 1; x++)
            {
                int neighbors = 0;

                int tileIndex = MapXYtoIndex(x, y);
                Vector2Int mySeed = voronoi_membership[tileIndex].seed;

                if (voronoi_membership[MapXYtoIndex(x-1, y)].seed != mySeed) { neighbors += 1; }
                if (voronoi_membership[MapXYtoIndex(x + 1, y)].seed != mySeed) { neighbors += 1; }
                if (voronoi_membership[MapXYtoIndex(x, y - 1)].seed != mySeed) { neighbors += 1; }
                if (voronoi_membership[MapXYtoIndex(x, y + 1)].seed != mySeed) { neighbors += 1; }

                if (neighbors < 2)
                {
                    tilemapVisualizer.PaintSingleFloorTile(new Vector2Int(x, y));
                }
            }
        }

        //tilemapVisualizer.PaintCorridorTiles(seeds);
    }
    
    private int MapXYtoIndex(int x, int y)
    {
        return y + (x * mapHeight);
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
}

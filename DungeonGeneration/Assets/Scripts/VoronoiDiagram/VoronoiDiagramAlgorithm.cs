using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
        Dictionary<Vector2Int, float> voronoi_distance = new Dictionary<Vector2Int, float>(numberOfSeeds);
        List<bool> voronoi_membership = new List<bool>(mapWidth * mapHeight);

        for (int i = 0; i < voronoi_membership.Count; i++)
        {
            int x = i % mapWidth;
            int y = i / mapWidth;

            foreach (Vector2Int seed in seeds)
            {
                float distance = Vector2Int.Distance(seed, new Vector2Int(x, y));

                voronoi_distance[seed] = distance;
            }
        }

        tilemapVisualizer.ClearTilemap();
        tilemapVisualizer.PaintFloorTiles(seeds);
    }
    
    private HashSet<Vector2Int> GenerateSeeds()
    {
        HashSet<Vector2Int> seeds = new HashSet<Vector2Int>();

        while(seeds.Count<numberOfSeeds)
        {
            int xPos = Random.Range(1, mapWidth);
            int yPos = Random.Range(1, mapHeight);

            seeds.Add(new Vector2Int(xPos, yPos));
        }

        return seeds;
    }
}

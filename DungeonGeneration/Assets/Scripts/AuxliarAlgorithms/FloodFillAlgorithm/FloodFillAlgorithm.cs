using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FloodFillAlgorithm
{
    //Flood fil algorithm
    private static List<Vector2Int> GetRegionTiles<T>(int startX, int startY, T[,] map, int mapWidth, int mapHeight)
    {
        List<Vector2Int> tiles = new List<Vector2Int>();
        bool[,] visitedTiles = new bool[mapWidth, mapHeight]; // true: visited , false: not visited
        T tileType = map[startX, startY];

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(new Vector2Int(startX, startY));
        visitedTiles[startX, startY] = true;

        while (queue.Count > 0)
        {
            Vector2Int tile = queue.Dequeue();
            tiles.Add(tile);

            Vector2Int[] fourDirectionsArray = Directions.GetFourDirectionsArray();
            foreach (Vector2Int dir in fourDirectionsArray)
            {
                int neighbourX = tile.x + dir.x;
                int neighbourY = tile.y + dir.y;

                if (neighbourX >= 0 && neighbourX < mapWidth && neighbourY >= 0 && neighbourY < mapHeight)
                {
                    if (!visitedTiles[neighbourX, neighbourY] && Equals(map[neighbourX, neighbourY], tileType)/*map[neighbourX, neighbourY] == tileType*/)
                    {
                        visitedTiles[neighbourX, neighbourY] = true;
                        queue.Enqueue(new Vector2Int(neighbourX, neighbourY));
                    }
                }
            }
        }

        return tiles;
    }

    

    public static List<List<Vector2Int>> GetRegionsOfType<T>(T[,] map, T tileType, int mapWidth, int mapHeight)
    {
        List<List<Vector2Int>> regions = new List<List<Vector2Int>>();
        bool[,] visitedTiles = new bool[mapWidth, mapHeight]; // true: visited , false: not visited

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                if (!visitedTiles[x, y] && Equals(map[x, y],tileType)/*map[x, y] == tileType*/)
                {
                    List<Vector2Int> newRegion = GetRegionTiles(x, y, map, mapWidth, mapHeight);
                    regions.Add(newRegion);

                    foreach (Vector2Int tile in newRegion)
                    {
                        visitedTiles[tile.x, tile.y] = true;
                    }
                }
            }
        }

        return regions;
    }
}

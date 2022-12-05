using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FloodFillAlgorithm
{
    /// <summary>
    /// Performs the flood fill algorithm
    /// </summary>
    /// <typeparam name="T">Map type</typeparam>
    /// <param name="startX">X start position</param>
    /// <param name="startY">Y start position</param>
    /// <param name="map">Map</param>
    /// <param name="mapWidth">Map width</param>
    /// <param name="mapHeight">Map height</param>
    /// <returns>A list with all map cell positions which have the same type</returns>
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

    /// <summary>
    /// Returns all map regions which are the given type (tileType parameter)
    /// </summary>
    /// <typeparam name="T">Map type</typeparam>
    /// <param name="map">Map</param>
    /// <param name="tileType">Cell type</param>
    /// <param name="mapWidth">Map width</param>
    /// <param name="mapHeight">Map height</param>
    /// <returns>A list of lists with all map cell positions which have given type</returns>
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

    /// <summary>
    /// Performs the flood fill algorithm
    /// </summary>
    /// <typeparam name="T">Map type</typeparam>
    /// <param name="startX">X start position</param>
    /// <param name="startY">Y start position</param>
    /// <param name="map">Map</param>
    /// <param name="mapWidth">Map width</param>
    /// <param name="mapHeight">Map height</param>
    /// <returns>A list with all map cell positions which have the same type</returns>
    private static List<Vector2Int> GetRegionTiles<T>(int startX, int startY, List<T> map, int mapWidth, int mapHeight)
    {
        List<Vector2Int> tiles = new List<Vector2Int>();
        List<bool> visitedTiles = new List<bool>(); // true: visited , false: not visited
        for (int i = 0; i < mapWidth * mapHeight; i++)
        {
            visitedTiles.Add(false);
        } 
        T tileType = map[MapXYtoIndex(startX,startY,mapWidth)];

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(new Vector2Int(startX, startY));
        visitedTiles[MapXYtoIndex(startX, startY, mapWidth)] = true;

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
                    int index = MapXYtoIndex(neighbourX, neighbourY, mapWidth);
                    if (!visitedTiles[index] && Equals(map[index], tileType)/*map[neighbourX, neighbourY] == tileType*/)
                    {
                        visitedTiles[index] = true;
                        queue.Enqueue(new Vector2Int(neighbourX, neighbourY));
                    }
                }
            }
        }

        return tiles;
    }

    /// <summary>
    /// Returns all map regions which are the given type (tileType parameter)
    /// </summary>
    /// <typeparam name="T">Map type</typeparam>
    /// <param name="map">Map</param>
    /// <param name="tileType">Cell type</param>
    /// <param name="mapWidth">Map width</param>
    /// <param name="mapHeight">Map height</param>
    /// <returns>A list of lists with all map cell positions which have given type</returns>
    public static List<List<Vector2Int>> GetRegionsOfType<T>(List<T> map, T tileType, int mapWidth, int mapHeight)
    {
        List<List<Vector2Int>> regions = new List<List<Vector2Int>>();
        List<bool> visitedTiles = new List<bool>(); // true: visited , false: not visited
        for (int i = 0; i < mapWidth*mapHeight; i++)
        {
            visitedTiles.Add(false);
        }

        for (int i = 0; i < visitedTiles.Count; i++)
        {
            int x = i % mapWidth;
            int y = i / mapWidth;

            if (!visitedTiles[MapXYtoIndex(x, y, mapWidth)] && Equals(map[MapXYtoIndex(x, y, mapWidth)], tileType)/*map[x, y] == tileType*/)
            {
                List<Vector2Int> newRegion = GetRegionTiles(x, y, map, mapWidth, mapHeight);
                regions.Add(newRegion);

                foreach (Vector2Int tile in newRegion)
                {
                    visitedTiles[MapXYtoIndex(tile.x, tile.y, mapWidth)] = true;
                }
            }
            
        }

        return regions;
    }

    /// <summary>
    /// Converts a map position to an index
    /// </summary>
    /// <param name="x">X map position</param>
    /// <param name="y">Y map position</param>
    /// <param name="mapWidth">Map width</param>
    /// <returns>A integer which represents a map position</returns>
    private static int MapXYtoIndex(int x, int y, int mapWidth)
    {
        return x + (y * mapWidth);
    }

}

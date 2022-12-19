using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Class for representing a region for the flood fill algorithm
public class Region : IComparable<Region>
{
    public List<Vector2Int> tiles;
    public List<Vector2Int> borderTiles;
    public List<Region> connectedRegions;
    private int regionSize;
    private bool isAccessibleFromMainRegion;
    private bool isMainRegion;

    public bool IsAccessibleFromMainRegion { get => isAccessibleFromMainRegion; set => isAccessibleFromMainRegion = value; }
    public bool IsMainRoom { get => isMainRegion; set => isMainRegion = value; }

    public Region()
    {
    }

    //We create the region
    public Region(List<Vector2Int> tiles, int[,] map, int mapWidth, int mapHeight)
    {
        this.tiles = tiles;
        regionSize = tiles.Count;
        connectedRegions = new List<Region>();
        borderTiles = new List<Vector2Int>();

        foreach (Vector2Int tile in tiles)
        {
            Vector2Int[] directions = Directions.GetFourDirectionsArray();          
            foreach (Vector2Int dir in directions)
            {
                int neighbourX = tile.x + dir.x;
                int neighbourY = tile.y + dir.y;
                
                if (neighbourX>=0 && neighbourX<mapWidth && neighbourY>=0 && neighbourY<mapHeight && map[neighbourX, neighbourY] == 1) // 1-> wall
                {
                    borderTiles.Add(tile);
                }
            }           
        }
    }

    /// <summary>
    /// Connects two rooms
    /// </summary>
    /// <param name="region1">First region</param>
    /// <param name="region2">Second region</param>
    public static void ConnectRegions(Region region1, Region region2)
    {
        if(region1.isAccessibleFromMainRegion)
        {
            region2.SetAccessFromMainRegion();
        }
        else if(region2.isAccessibleFromMainRegion)
        {
            region1.SetAccessFromMainRegion();
        }

        region1.connectedRegions.Add(region2);
        region2.connectedRegions.Add(region1);
    }

    /// <summary>
    /// Creates an acces from the main region
    /// </summary>
    public void SetAccessFromMainRegion()
    {
        if(!isAccessibleFromMainRegion)
        {
            isAccessibleFromMainRegion = true;
            foreach (Region room in connectedRegions)
            {
                room.SetAccessFromMainRegion();
            }
        }
    }

    /// <summary>
    /// Compares the region size to other
    /// </summary>
    /// <param name="other">The other region</param>
    /// <returns></returns>
    public int CompareTo(Region other)
    {
        return other.regionSize.CompareTo(regionSize);
    }

    /// <summary>
    /// Returns if one region is connected to other region
    /// </summary>
    /// <param name="other">Other rgion</param>
    /// <returns>True if the region is connected to the other region</returns>
    public bool IsConnected(Region other)
    {
        return connectedRegions.Contains(other);
    }
}

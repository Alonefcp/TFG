using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Class for 
public class Map
{
    private int mapWidht, mapHeight;
    private bool[,] map; //false -> hasn't floor , true -> has floor

    public Map(int w, int h)
    {
        mapWidht = w;
        mapHeight = h;

        map = new bool[mapHeight, mapWidht];
    }

    /// <summary>
    /// Returns true if the given position is a floor otherwise it returns false
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public bool IsFloorPosition(Vector2Int pos)
    {
        return map[pos.y, pos.x];
    }

    /// <summary>
    /// Returns true if the floor position is inside the map boundaries otherwise it returns false
    /// </summary>
    /// <param name="pos">Floor position</param>
    /// <returns></returns>
    public bool IsInsideTheMap(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < mapWidht && pos.y >= 0 && pos.y < mapHeight;
    }

    /// <summary>
    /// Sets 
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="hasWall"></param>
    public void SetHasFloor(Vector2Int pos, bool hasWall)
    { 
        map[pos.y, pos.x] = hasWall; 
    }

    /// <summary>
    /// Returns the map widht
    /// </summary>
    /// <returns></returns>
    public int GetMapWidth() { return mapWidht; }

    /// <summary>
    /// Returns the map height
    /// </summary>
    /// <returns></returns>
    public int GetMapHeight() { return mapHeight; }
}

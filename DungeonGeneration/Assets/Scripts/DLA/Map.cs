using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map
{
    private int mapWidth, mapHeight;
    private bool[,] map; //false -> hasn't floor , true -> has floor

    public Map(int w, int h)
    {
        mapWidth = w;
        mapHeight = h;

        map = new bool[mapHeight, mapWidth];
    }

    public bool IsFloorPosition(Vector2Int pos)
    {
        return map[pos.y, pos.x];
    }

    public void SetHasFloor(Vector2Int pos, bool hasWall)
    { 
        map[pos.y, pos.x] = hasWall; 
    }

    public int GetMapWidth() { return mapWidth; }
    public int GetMapHeight() { return mapHeight; }
}

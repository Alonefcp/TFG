using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Region : IComparable<Region>
{
    public List<Vector2Int> tiles;
    public List<Vector2Int> borderTiles;
    public List<Region> connectedRooms;
    private int roomSize;
    private bool isAccessibleFromMainRoom;
    private bool isMainRoom;

    public bool IsAccessibleFromMainRoom { get => isAccessibleFromMainRoom; set => isAccessibleFromMainRoom = value; }
    public bool IsMainRoom { get => isMainRoom; set => isMainRoom = value; }

    public Region()
    {
    }

    public Region(List<Vector2Int> tiles, int[,] map, int mapWidth, int mapHeight)
    {
        this.tiles = tiles;
        roomSize = tiles.Count;
        connectedRooms = new List<Region>();
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

    public Region(List<Vector2Int> tiles, float[,] map, int mapWidth, int mapHeight)
    {
        this.tiles = tiles;
        roomSize = tiles.Count;
        connectedRooms = new List<Region>();
        borderTiles = new List<Vector2Int>();

        foreach (Vector2Int tile in tiles)
        {
            Vector2Int[] directions = Directions.GetFourDirectionsArray();
            foreach (Vector2Int dir in directions)
            {
                int neighbourX = tile.x + dir.x;
                int neighbourY = tile.y + dir.y;

                if (neighbourX >= 0 && neighbourX < mapWidth && neighbourY >= 0 && neighbourY < mapHeight && map[neighbourX, neighbourY] == 1)// 1-> wall
                {
                    borderTiles.Add(tile);
                }
            }
        }
    }

    public static void ConnectRooms(Region room1, Region room2)
    {
        if(room1.isAccessibleFromMainRoom)
        {
            room2.SetAccessFromMainRoom();
        }
        else if(room2.isAccessibleFromMainRoom)
        {
            room1.SetAccessFromMainRoom();
        }

        room1.connectedRooms.Add(room2);
        room2.connectedRooms.Add(room1);
    }

    public void SetAccessFromMainRoom()
    {
        if(!isAccessibleFromMainRoom)
        {
            isAccessibleFromMainRoom = true;
            foreach (Region room in connectedRooms)
            {
                room.SetAccessFromMainRoom();
            }
        }
    }

    public int CompareTo(Region other)
    {
        return other.roomSize.CompareTo(roomSize);
    }

    public bool IsConnected(Region other)
    {
        return connectedRooms.Contains(other);
    }
}

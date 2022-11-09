using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : IComparable<Room>
{
    public List<Vector2Int> tiles;
    public List<Vector2Int> borderTiles;
    public List<Room> connectedRooms;
    public int roomSize;
    public bool isAccessibleFromMainRoom;
    public bool isMainRoom;

    public Room()
    {
    }

    public Room(List<Vector2Int> _tiles, CellularAutomataAlgorithm.TileType[,] map, int mapWidth, int mapHeight)
    {
        tiles = _tiles;
        roomSize = tiles.Count;
        connectedRooms = new List<Room>();
        borderTiles = new List<Vector2Int>();

        foreach (Vector2Int tile in tiles)
        {
            Vector2Int[] directions = Directions.GetFourDirectionsArray();          
            foreach (Vector2Int dir in directions)
            {
                int neighbourX = tile.x + dir.x;
                int neighbourY = tile.y + dir.y;
                
                if (neighbourX>=0 && neighbourX<mapWidth && neighbourY>=0 && neighbourY<mapHeight && map[neighbourX, neighbourY] == CellularAutomataAlgorithm.TileType.Wall)
                {
                    borderTiles.Add(tile);
                }
            }           
        }
    }

    public Room(List<Vector2Int> _tiles, float[,] map, int mapWidth, int mapHeight)
    {
        tiles = _tiles;
        roomSize = tiles.Count;
        connectedRooms = new List<Room>();
        borderTiles = new List<Vector2Int>();

        foreach (Vector2Int tile in tiles)
        {
            Vector2Int[] directions = Directions.GetFourDirectionsArray();
            foreach (Vector2Int dir in directions)
            {
                int neighbourX = tile.x + dir.x;
                int neighbourY = tile.y + dir.y;

                if (neighbourX >= 0 && neighbourX < mapWidth && neighbourY >= 0 && neighbourY < mapHeight && map[neighbourX, neighbourY] == 1)
                {
                    borderTiles.Add(tile);
                }
            }
        }
    }

    public static void ConnectRooms(Room room1, Room room2)
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
            foreach (Room room in connectedRooms)
            {
                room.SetAccessFromMainRoom();
            }
        }
    }

    public int CompareTo(Room other)
    {
        return other.roomSize.CompareTo(roomSize);
    }

    public bool IsConnected(Room other)
    {
        return connectedRooms.Contains(other);
    }
}

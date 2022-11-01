using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public struct TileCoord
{
    public TileCoord(int x, int y)
    {
        posX = x;
        posY = y;
    }

    public int posX;
    public int posY;
}

public class CellularAutomataAlgorithm : DungeonGenerator
{
    enum Neighborhood {Moore, VonNeummann}
    public enum TileType {Wall = 1, Floor = 0}

    [Range(30,200)]
    [SerializeField] private int mapWidth = 80, mapHeight = 60;
    [Range(1, 20)]
    [SerializeField] private int iterations = 5;
    [Range(0.0f, 1.0f)]
    [SerializeField] private float fillPercent = 0.45f;
    [SerializeField] private Neighborhood neighborhood = Neighborhood.Moore;
    [Range(1,3)]
    [SerializeField] private int connectionSize = 1;
    [Range(0, 100)]
    [SerializeField] private int wallThresholdSize = 50;
    [Range(0, 100)]
    [SerializeField] private int floorThresholdSize = 50;

    private TileType[,] map;

    //void Start()
    //{
    //    GenerateDungeon();
    //}

    public override void GenerateDungeon()
    {
        tilemapVisualizer.ClearTilemap();

        map = GenerateNoise();    
        RunCellularAutomata();
        EraseRegions();         
    }

    private TileType[,] GenerateNoise()
    {
        TileType[,] map = new TileType[mapWidth, mapHeight];

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                if (x == 0 || x == mapWidth - 1 || y == 0 || y == mapHeight - 1)
                {
                    tilemapVisualizer.PaintSingleWallTile(new Vector2Int(x, y));

                    map[x, y] = TileType.Wall;
                }
                else
                {
                    map[x, y] = (Random.Range(0.0f, 1.0f) < fillPercent) ? TileType.Wall : TileType.Floor;

                    if (map[x, y] == TileType.Wall) tilemapVisualizer.PaintSingleWallTile(new Vector2Int(x, y));
                    else tilemapVisualizer.PaintSingleFloorTile(new Vector2Int(x, y));
                }
            }
        }

        return map;
    }

    private void RunCellularAutomata()
    {
        for (int i = 0; i < iterations; i++)
        {
            TileType[,] mapClone = (TileType[,])map.Clone();
            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    int neighbourWallTiles = GetWallNeighbours(x, y);

                    if (neighborhood == Neighborhood.Moore) MooreAutomata(mapClone, x, y, neighbourWallTiles);
                    else VonNeummannAutomata(mapClone, x, y, neighbourWallTiles);

                }
            }
            map = (TileType[,])mapClone.Clone();
        }    
    }

    private void MooreAutomata(TileType[,] map, int x, int y,int neighbourWallTiles)
    {
        if (neighbourWallTiles > 4) //>=5 , > 4
        {
            map[x, y] = TileType.Wall;
            tilemapVisualizer.PaintSingleWallTile(new Vector2Int(x, y));
        }
        else if (neighbourWallTiles < 4)
        {
            map[x, y] = TileType.Floor;
            tilemapVisualizer.PaintSingleFloorTile(new Vector2Int(x, y));
        }
    }

    private void VonNeummannAutomata(TileType[,] map, int x, int y, int neighbourWallTiles)
    {
        if (neighbourWallTiles > 2)
        {
            map[x, y] = TileType.Wall;
            tilemapVisualizer.PaintSingleWallTile(new Vector2Int(x, y));
        }
        else if (neighbourWallTiles < 2)
        {
            map[x, y] = TileType.Floor;
            tilemapVisualizer.PaintSingleFloorTile(new Vector2Int(x, y));
        }
    }

    private int GetWallNeighbours(int x, int y)
    {
        Vector2Int[] directions = (neighborhood == Neighborhood.Moore) ? Directions.GetEightDiretionsArray() : Directions.GetFourDirectionsArray();
        int nWalls = 0;
        foreach (Vector2Int dir in directions)
        {
            int neighbourX = x + dir.x;
            int neighbourY = y + dir.y;

            if (neighbourX >= 0 && neighbourX < mapWidth && neighbourY >= 0 && neighbourY < mapHeight)
            {
                nWalls += (int)map[neighbourX, neighbourY];
            }
            else
            {
                nWalls++;
            }
        }

        return nWalls;
    }

    private void EraseRegions()
    {
        //Erase wall regions
        List<List<TileCoord>> wallRegions = GetRegionsOfType(TileType.Wall);

        foreach (List<TileCoord> wallRegion in wallRegions)
        {
            if (wallRegion.Count <= wallThresholdSize)
            {
                foreach (TileCoord tile in wallRegion)
                {
                    map[tile.posX, tile.posY] = TileType.Floor;                   
                    tilemapVisualizer.PaintSingleFloorTile(new Vector2Int(tile.posX, tile.posY));
                }
            }       
        }

        //Erase floor regions
        List<Room> leftFloorRegions = new List<Room>();
        List<List<TileCoord>> floorRegions = GetRegionsOfType(TileType.Floor);

        foreach (List<TileCoord> floorRegion in floorRegions)
        {
            if (floorRegion.Count <= floorThresholdSize)
            {
                foreach (TileCoord tile in floorRegion)
                {
                    map[tile.posX, tile.posY] = TileType.Wall;
                    tilemapVisualizer.PaintSingleWallTile(new Vector2Int(tile.posX, tile.posY));
                }
            }
            else //we store floor regions which are bigger than the floorThresholdSize, to connect them
            {
                leftFloorRegions.Add(new Room(floorRegion, map));
            }
        }

        if(leftFloorRegions.Count>0)
        {
            leftFloorRegions.Sort();
            leftFloorRegions[0].isMainRoom = true;
            leftFloorRegions[0].isAccessibleFromMainRoom = true;
            ConnectClosestRooms(leftFloorRegions);
        }
    }

    private void ConnectClosestRooms(List<Room> survivingRooms, bool forceAccessibilityFromMainRoom=false)
    {
        List<Room> roomList1 = new List<Room>();
        List<Room> roomList2 = new List<Room>();

        if(forceAccessibilityFromMainRoom)
        {
            foreach (Room room in survivingRooms)
            {
                if(room.isAccessibleFromMainRoom)
                {
                    roomList2.Add(room);
                }
                else
                {
                    roomList1.Add(room);
                }
            }
        }
        else
        {
            roomList1 = survivingRooms;
            roomList2 = survivingRooms;
        }

        int bestDistance = 0;
        TileCoord bestTile1 = new TileCoord(); ;
        TileCoord bestTile2 = new TileCoord(); ;
        Room bestRoom1 = new Room();
        Room bestRoom2 = new Room();
        bool possibleConnectionFound = false;

        foreach (Room room1 in roomList1)
        {
            if(!forceAccessibilityFromMainRoom)
            {
                possibleConnectionFound = false;
                if(room1.connectedRooms.Count > 0) continue;
            }

            foreach (Room room2 in roomList2)
            {
                if (room1 == room2 || room1.IsConnected(room2)) continue;

                for (int tileIndex1 = 0; tileIndex1 < room1.borderTiles.Count; tileIndex1++)
                {
                    for (int tileIndex2 = 0; tileIndex2 < room2.borderTiles.Count; tileIndex2++)
                    {
                        TileCoord tile1 = room1.borderTiles[tileIndex1];
                        TileCoord tile2 = room2.borderTiles[tileIndex2];

                        int distanceBetweenRooms = (int)Vector2.Distance(new Vector2(tile1.posX,tile1.posY), new Vector2(tile2.posX, tile2.posY));/*(int)(Mathf.Pow((tile1.posX - tile2.posX), 2) + Mathf.Pow((tile1.posY - tile2.posY), 2));*/
                        if(distanceBetweenRooms < bestDistance || !possibleConnectionFound)
                        {
                            possibleConnectionFound = true;
                            bestDistance = distanceBetweenRooms;
                            bestTile1 = tile1;
                            bestTile2 = tile2;
                            bestRoom1 = room1;
                            bestRoom2 = room2;
                        }
                    }
                }

            }

            if(possibleConnectionFound && !forceAccessibilityFromMainRoom)
            {
                CreateConnection(bestRoom1, bestRoom2, bestTile1, bestTile2);
            }
        }

        if (possibleConnectionFound && forceAccessibilityFromMainRoom)
        {
            CreateConnection(bestRoom1, bestRoom2, bestTile1, bestTile2);
            ConnectClosestRooms(survivingRooms, true);
        }

        if (!forceAccessibilityFromMainRoom)
        {
            ConnectClosestRooms(survivingRooms, true);
        }
    }

    private void CreateConnection(Room room1, Room room2, TileCoord tile1, TileCoord tile2) 
    {
        Room.ConnectRooms(room1, room2);     
        List<Vector2Int> line = BresenhamsLineAlgorithm.GetLinePointsList(tile1.posX, tile1.posY, tile2.posX, tile2.posY);
        foreach (Vector2Int coord in line)
        {
            DrawBiggerTile(coord, connectionSize);          
        }      
    }

    private void DrawBiggerTile(Vector2Int coord, int radius)
    {
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                if (x * x + y * y <= radius * radius)
                {
                    int drawX = coord.x+x;
                    int drawY = coord.y+y;
                    if (drawX >= 0 && drawX < mapWidth && drawY >= 0 && drawY < mapHeight)
                    {
                        map[drawX, drawY] = TileType.Floor;
                        tilemapVisualizer.PaintSingleFloorTile(new Vector2Int(drawX, drawY));
                    }
                }
            }
        }
    }

    private List<List<TileCoord>> GetRegionsOfType(TileType tileType)
    {
        List<List<TileCoord>> regions = new List<List<TileCoord>>();
        bool[,] visitedTiles = new bool[mapWidth, mapHeight]; // true: visited , false: not visited

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                if (!visitedTiles[x, y] && map[x, y] == tileType)
                {
                    List<TileCoord> newRegion = GetRegionTiles(x, y);
                    regions.Add(newRegion);

                    foreach (TileCoord tile in newRegion)
                    {
                        visitedTiles[tile.posX, tile.posY] = true;
                    }
                }
            }
        }

        return regions;
    }

    //Flood fil algorithm
    private List<TileCoord> GetRegionTiles(int startX, int startY)
    {
        List<TileCoord> tiles = new List<TileCoord>();
        bool[,] visitedTiles = new bool[mapWidth, mapHeight]; // true: visited , false: not visited
        TileType tileType = map[startX, startY];

        Queue<TileCoord> queue = new Queue<TileCoord>();
        queue.Enqueue(new TileCoord(startX, startY));
        visitedTiles[startX, startY] = true;

        while (queue.Count > 0)
        {
            TileCoord tile = queue.Dequeue();
            tiles.Add(tile);

            Vector2Int[] fourDirectionsArray = Directions.GetFourDirectionsArray();
            foreach (Vector2Int dir in fourDirectionsArray)
            {
                int neighbourX = tile.posX + dir.x;
                int neighbourY = tile.posY + dir.y;

                if (neighbourX >= 0 && neighbourX < mapWidth && neighbourY >= 0 && neighbourY < mapHeight)
                {
                    if (!visitedTiles[neighbourX, neighbourY] && map[neighbourX, neighbourY] == tileType)
                    {
                        visitedTiles[neighbourX, neighbourY] = true;
                        queue.Enqueue(new TileCoord(neighbourX, neighbourY));
                    }
                }
            }
        }

        return tiles;
    }
}

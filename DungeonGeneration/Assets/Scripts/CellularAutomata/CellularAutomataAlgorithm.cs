using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static FloodFillAlgorithm;
using Random = UnityEngine.Random;

public class CellularAutomataAlgorithm : DungeonGenerator
{
    enum Neighborhood {Moore, VonNeummann}
    enum MooreRule {Rule3=3, Rule4=4, Rule5=5}
    enum VonNeummannRule {Rule1=1, Rule2=2, Rule3=3}
    

    [Range(30,200)]
    [SerializeField] private int mapWidth = 80, mapHeight = 60;
    [Range(1, 20)]
    [SerializeField] private int iterations = 5;
    [Range(0.0f, 1.0f)]
    [SerializeField] private float fillPercent = 0.45f;
    [SerializeField] private Neighborhood neighborhood = Neighborhood.Moore;
    [SerializeField] private MooreRule mooreRule = MooreRule.Rule4;
    [SerializeField] private VonNeummannRule vonNeummannRule = VonNeummannRule.Rule2;
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

    private void SetFillPercentBasedOnRule()
    {
        if(neighborhood==Neighborhood.Moore)
        {
            switch (mooreRule)
            {
                case MooreRule.Rule3:
                    fillPercent = 0.28f;
                    break;
                case MooreRule.Rule4:
                    fillPercent = 0.46f;
                    break;
                case MooreRule.Rule5:
                    fillPercent = 0.66f;
                    break;
                default:
                    fillPercent = 0.46f;
                    break;
            }
        }
        else
        {
            switch (vonNeummannRule)
            {
                case VonNeummannRule.Rule1:
                    fillPercent = 0.13f;
                    break;
                case VonNeummannRule.Rule2:
                    fillPercent = 0.46f;
                    break;
                case VonNeummannRule.Rule3:
                    fillPercent = 0.84f;
                    break;
                default:
                    fillPercent = 0.46f;
                    break;
            }
        }       
    }

    private TileType[,] GenerateNoise()
    {
        SetFillPercentBasedOnRule();

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
        int ruleNumber = (int)mooreRule;
        
        if (neighbourWallTiles > ruleNumber) 
        {
            map[x, y] = TileType.Wall;
            tilemapVisualizer.PaintSingleWallTile(new Vector2Int(x, y));
        }
        else if (neighbourWallTiles < ruleNumber)
        {
            map[x, y] = TileType.Floor;
            tilemapVisualizer.PaintSingleFloorTile(new Vector2Int(x, y));
        }
    }

    private void VonNeummannAutomata(TileType[,] map, int x, int y, int neighbourWallTiles)
    {
        int ruleNumber = (int)vonNeummannRule;
        if (neighbourWallTiles > ruleNumber)
        {
            map[x, y] = TileType.Wall;
            tilemapVisualizer.PaintSingleWallTile(new Vector2Int(x, y));
        }
        else if (neighbourWallTiles < ruleNumber)
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
        List<List<Vector2Int>> wallRegions = GetRegionsOfType(map,TileType.Wall,mapWidth,mapHeight);

        foreach (List<Vector2Int> wallRegion in wallRegions)
        {
            if (wallRegion.Count <= wallThresholdSize)
            {
                foreach (Vector2Int tile in wallRegion)
                {
                    map[tile.x, tile.y] = TileType.Floor;                   
                    tilemapVisualizer.PaintSingleFloorTile(new Vector2Int(tile.x, tile.y));
                }
            }       
        }

        //Erase floor regions
        List<Room> leftFloorRegions = new List<Room>();
        List<List<Vector2Int>> floorRegions = GetRegionsOfType(map,TileType.Floor, mapWidth, mapHeight);

        foreach (List<Vector2Int> floorRegion in floorRegions)
        {
            if (floorRegion.Count <= floorThresholdSize)
            {
                foreach (Vector2Int tile in floorRegion)
                {
                    map[tile.x, tile.y] = TileType.Wall;
                    tilemapVisualizer.PaintSingleWallTile(new Vector2Int(tile.x, tile.y));
                }
            }
            else //we store floor regions which are bigger than the floorThresholdSize, to connect them
            {
                leftFloorRegions.Add(new Room(floorRegion, map,mapWidth,mapHeight));
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
        Vector2Int bestTile1 = new Vector2Int();
        Vector2Int bestTile2 = new Vector2Int(); 
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
                        Vector2Int tile1 = room1.borderTiles[tileIndex1];
                        Vector2Int tile2 = room2.borderTiles[tileIndex2];

                        int distanceBetweenRooms = (int)Vector2.Distance(new Vector2(tile1.x,tile1.y), new Vector2(tile2.x, tile2.y));/*(int)(Mathf.Pow((tile1.posX - tile2.posX), 2) + Mathf.Pow((tile1.posY - tile2.posY), 2));*/
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

    private void CreateConnection(Room room1, Room room2, Vector2Int tile1, Vector2Int tile2) 
    {
        Room.ConnectRooms(room1, room2);     
        List<Vector2Int> line = BresenhamsLineAlgorithm.GetLinePointsList(tile1.x, tile1.y, tile2.x, tile2.y);
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

    
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static FloodFillAlgorithm;
using Random = UnityEngine.Random;

public class CellularAutomataAlgorithm : DungeonGeneration
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
    [SerializeField] private bool connectRegions = true;
    [Range(1,3)]
    [SerializeField] private int connectionSize = 1;
    [Range(0, 100)]
    [SerializeField] private int wallThresholdSize = 50;
    [Range(0, 100)]
    [SerializeField] private int floorThresholdSize = 50;

    [SerializeField] bool useHilbertCurve = false;
    [Range(1, 3)]
    [SerializeField] private int order = 1;
    [Range(0, 70)]
    [SerializeField] private int minOffsetX = 0, maxOffsetX = 30;
    [Range(0, 70)]
    [SerializeField] private int minOffsetY = 0, maxOffsetY=30;

    private int[,] map; //1 -> wall , 0 -> floor
    private HilbertCurve hilbertCurve;

    //void Start()
    //{
    //    GenerateDungeon();
    //}

    public override void GenerateDungeon()
    {
        base.GenerateDungeon();

        if (useHilbertCurve)
        {
            hilbertCurve = new HilbertCurve(order);
            hilbertCurve.CalculateHilbertCurve(mapWidth, mapHeight, minOffsetX, maxOffsetX, minOffsetY, maxOffsetY);
            int initialCount = hilbertCurve.HilbertCurvePoints.Count;
            for (int i = 0; i < initialCount; i++)
            {
                //tilemapVisualizer.PaintSingleFloorTile(point);
                DrawBiggerTile(hilbertCurve.HilbertCurvePoints.ElementAt(i), 2, hilbertCurve.HilbertCurvePoints, 1);
            }
        }

        map = GenerateNoise();
        RunCellularAutomata();
        EraseRegions();

        //Draw the map
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                if (map[x, y] == 1) tilemapVisualizer.PaintSingleWallTile(new Vector2Int(x, y));
                else tilemapVisualizer.PaintSingleFloorTile(new Vector2Int(x, y));
            }
        }

        //Sets player position
        Vector2Int playerPosition = new Vector2Int(Random.Range(1,mapWidth-1), Random.Range(1, mapHeight-1));

        while (map[playerPosition.x,playerPosition.y] == 1)
        {
            playerPosition = new Vector2Int(Random.Range(1, mapWidth - 1), Random.Range(1, mapHeight - 1));
        }
        playerController.SetPlayer(playerPosition, new Vector3(0.3f, 0.3f, 0.3f));
    }

    /// <summary>
    /// Sets the fillPercent based on the automata's rule 
    /// </summary>
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

    /// <summary>
    /// Place random walls
    /// </summary>
    /// <returns></returns>
    private int[,] GenerateNoise()
    {
        SetFillPercentBasedOnRule();

        int[,] map = new int[mapWidth, mapHeight];

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                if (x == 0 || x == mapWidth - 1 || y == 0 || y == mapHeight - 1)
                {
                    map[x, y] = 1;
                }
                else if (useHilbertCurve && hilbertCurve.HilbertCurvePoints.Contains(new Vector2Int(x, y)))
                {
                    hilbertCurve.HilbertCurvePointsInsideTheMap.Add(new Vector2Int(x, y));
                    map[x, y] = 1;                   
                }
                else
                {
                    map[x, y] = (Random.Range(0.0f, 1.0f) < fillPercent) ? 1 : 0;
                }
            }
        }

        return map;
    }

    /// <summary>
    /// Runs the cellular automata by changing the map state each iteration
    /// </summary>
    private void RunCellularAutomata()
    {
        for (int i = 0; i < iterations; i++)
        {
            int[,] mapClone = (int[,])map.Clone();
            for (int x = 0; x < mapWidth; x++)
            {             
                for (int y = 0; y < mapHeight; y++)
                {
                    if (useHilbertCurve && hilbertCurve.HilbertCurvePointsInsideTheMap.Contains(new Vector2Int(x, y)))
                    {
                        continue;
                    }

                    int neighbourWallTiles = GetWallNeighbours(x, y);

                    ApplyAutomata(mapClone, x, y, neighbourWallTiles);
                }
            }
            map = (int[,])mapClone.Clone();
        }    
    }

    /// <summary>
    /// Applies Moore or VonNeummann automata in a given position
    /// </summary>
    /// <param name="map">The map</param>
    /// <param name="x">Given X position</param>
    /// <param name="y">Given Y position</param>
    /// <param name="neighbourWallTiles">Number of walls neighbours</param>
    private void ApplyAutomata(int[,] map, int x, int y,int neighbourWallTiles)
    {
        int ruleNumber = (neighborhood == Neighborhood.Moore)? (int)mooreRule: (int)vonNeummannRule;
        
        if (neighbourWallTiles > ruleNumber) 
        {
            map[x, y] = 1;
        }
        else if (neighbourWallTiles < ruleNumber)
        {
            map[x, y] = 0;
        }
    }

    /// <summary>
    /// Returns the number of wall neighbours of a given position. It checks four or eight neighbours
    /// </summary>
    /// <param name="x">Given X position</param>
    /// <param name="y">Given Y position</param>
    /// <returns></returns>
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

    /// <summary>
    /// Erases floor and wall regions which are bigger than a given threshold. It also connects the floor regions
    /// that are bigger than the threshold
    /// </summary>
    private void EraseRegions()
    {      
        //Erase floor regions
        List<Region> leftFloorRegions = new List<Region>();
        List<List<Vector2Int>> floorRegions = GetRegionsOfType(map,0, mapWidth, mapHeight);

        foreach (List<Vector2Int> floorRegion in floorRegions)
        {
            if (floorRegion.Count <= floorThresholdSize)
            {
                foreach (Vector2Int tile in floorRegion)
                {
                    map[tile.x, tile.y] = 1;
                }
            }
            else //we store floor regions which are bigger than the floorThresholdSize, to connect them
            {
                leftFloorRegions.Add(new Region(floorRegion, map,mapWidth,mapHeight));
            }
        }

        if(connectRegions && leftFloorRegions.Count>0)
        {
            leftFloorRegions.Sort();
            leftFloorRegions[0].IsMainRoom = true;
            leftFloorRegions[0].IsAccessibleFromMainRoom = true;
            ConnectClosestRegions(leftFloorRegions);
        }

        //Erase wall regions
        List<List<Vector2Int>> wallRegions = GetRegionsOfType(map, 1, mapWidth, mapHeight);

        foreach (List<Vector2Int> wallRegion in wallRegions)
        {
            if (wallRegion.Count <= wallThresholdSize)
            {
                foreach (Vector2Int tile in wallRegion)
                {
                    map[tile.x, tile.y] = 0;
                }
            }
        }
    }

    /// <summary>
    /// Connect every map region
    /// </summary>
    /// <param name="survivingRegions">Regions we are going to connect</param>
    /// <param name="forceAccessibilityFromMainRegion">If we want to create a connection from the main region</param>
    private void ConnectClosestRegions(List<Region> survivingRegions, bool forceAccessibilityFromMainRoom=false)
    {
        List<Region> roomList1 = new List<Region>();
        List<Region> roomList2 = new List<Region>();

        if(forceAccessibilityFromMainRoom)
        {
            foreach (Region room in survivingRegions)
            {
                if(room.IsAccessibleFromMainRoom)
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
            roomList1 = survivingRegions;
            roomList2 = survivingRegions;
        }

        int bestDistance = 0;
        Vector2Int bestTile1 = new Vector2Int();
        Vector2Int bestTile2 = new Vector2Int(); 
        Region bestRoom1 = new Region();
        Region bestRoom2 = new Region();
        bool possibleConnectionFound = false;

        foreach (Region room1 in roomList1)
        {
            if(!forceAccessibilityFromMainRoom)
            {
                possibleConnectionFound = false;
                if(room1.connectedRooms.Count > 0) continue;
            }

            foreach (Region room2 in roomList2)
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
            ConnectClosestRegions(survivingRegions, true);
        }

        if (!forceAccessibilityFromMainRoom)
        {
            ConnectClosestRegions(survivingRegions, true);
        }
    }

    /// <summary>
    /// Creates a connection between two regions
    /// </summary>
    /// <param name="region1">First region</param>
    /// <param name="region2">Second region</param>
    /// <param name="tile1">First region tile which is the closest one to the second region</param>
    /// <param name="tile2">Second region tile which is the closest one to the first region</param>
    private void CreateConnection(Region region1, Region region2, Vector2Int tile1, Vector2Int tile2) 
    {
        Region.ConnectRooms(region1, region2);     
        List<Vector2Int> line = BresenhamsLineAlgorithm.GetLinePointsList(tile1.x, tile1.y, tile2.x, tile2.y);
        foreach (Vector2Int coord in line)
        {
            DrawBiggerTile(coord, connectionSize);          
        }      
    }

    /// <summary>
    /// Draws a bigger tile
    /// </summary>
    /// <param name="position">Tile position</param>
    /// <param name="radius">How much we wat to increase the tile size</param>
    private void DrawBiggerTile(Vector2Int position, int radius, HashSet<Vector2Int> addedPoints=null,int cellType = 0)
    {
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                if (x * x + y * y <= radius * radius)
                {
                    int drawX = position.x+x;
                    int drawY = position.y+y;
                    if (drawX >= 0 && drawX < mapWidth && drawY >= 0 && drawY < mapHeight)
                    {
                        if(map!=null) map[drawX, drawY] = cellType;
                        if(addedPoints!=null)
                        {
                            addedPoints.Add(new Vector2Int(drawX, drawY));
                        }                        
                    }
                }
            }
        }
    }   
}

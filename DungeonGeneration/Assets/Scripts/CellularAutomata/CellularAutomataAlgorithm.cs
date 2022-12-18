using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static FloodFillAlgorithm;
using Random = UnityEngine.Random;

public class CellularAutomataAlgorithm : DungeonGeneration
{
    public enum Neighborhood {Moore, VonNeummann}
    enum MooreRule {Rule3=3, Rule4=4, Rule5=5}
    enum VonNeummannRule {Rule1=1, Rule2=2, Rule3=3}
    
    [Range(50,200)]
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
    [Range(3, 4)]
    [SerializeField] private int order = 1;
    [Range(0, 200)]
    [SerializeField] private int minOffsetX = 0, maxOffsetX = 30;
    [Range(0, 200)]
    [SerializeField] private int minOffsetY = 0, maxOffsetY=30;

    private int[,] map = null; //1 -> wall , 0 -> floor
    private HilbertCurve hilbertCurve = null;
    private int mapOffsetX=0, mapOffsetY=0;

    int hilbertMapWidth = 400;
    int hilbertMapHeight = 400;

    public Neighborhood NeighborhoodType { get => neighborhood;}
    public bool ConnectRegions { get => connectRegions;}
    public bool UseHilbertCurve { get => useHilbertCurve;}

    //void Start()
    //{
    //    GenerateDungeon();
    //}

    public override void GenerateDungeon()
    {
        base.GenerateDungeon();

        if (UseHilbertCurve)
        {
            mapOffsetX = Random.Range(minOffsetX, maxOffsetX);
            mapOffsetY = Random.Range(minOffsetY, maxOffsetY);
            hilbertMapWidth = 400;
            hilbertMapHeight = 400;
            map = null;
            DrawHilbertCurve();
        }
        else
        {
            mapOffsetX = 0;
            mapOffsetY = 0;
        }

        map = GenerateNoise();
        RunCellularAutomata();
        EraseRegions();

        //Draw the map
        DrawMap();

        //Sets player position
        Vector2Int playerPosition = (useHilbertCurve)? new Vector2Int(Random.Range(mapOffsetX, mapWidth + mapOffsetX - 1), Random.Range(mapOffsetY, mapHeight + mapOffsetY - 1)) : new Vector2Int(Random.Range(1, mapWidth - 1), Random.Range(1, mapHeight - 1));

        while (map[playerPosition.x, playerPosition.y] == 1)
        {
            playerPosition = (useHilbertCurve) ? new Vector2Int(Random.Range(mapOffsetX, mapWidth + mapOffsetX - 1), Random.Range(mapOffsetY, mapHeight + mapOffsetY - 1)) : new Vector2Int(Random.Range(1, mapWidth - 1), Random.Range(1, mapHeight - 1));
        }
        playerController.SetPlayer(playerPosition, new Vector3(1.0f, 1.0f, 1.0f));
    }

    /// <summary>
    /// Draws the map
    /// </summary>
    private void DrawMap()
    {
        HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();
        
        for (int x = mapOffsetX; x < mapWidth + mapOffsetX; x++)
        {
            for (int y = mapOffsetY; y < mapHeight + mapOffsetY; y++)
            {
                if (map[x, y] == 1) tilemapVisualizer.PaintSingleInnerWallTile(new Vector2Int(x, y));
                else
                {
                    floorPositions.Add(new Vector2Int(x, y));
                    tilemapVisualizer.PaintSingleFloorTile(new Vector2Int(x, y));
                }
            }
        }

        WallGenerator.CreateWalls(floorPositions, tilemapVisualizer);
    }

    /// <summary>
    /// Creates and draws a Hilbert curve
    /// </summary>
    private void DrawHilbertCurve()
    {
        hilbertCurve = new HilbertCurve(order);
        hilbertCurve.CalculateHilbertCurve(hilbertMapWidth, hilbertMapHeight);
        int initialCount = hilbertCurve.HilbertCurvePoints.Count;
        for (int i = 0; i < initialCount; i++)
        {
            //tilemapVisualizer.PaintSingleFloorTile(hilbertCurve.HilbertCurvePoints.ElementAt(i));
            DrawBiggerTile(hilbertCurve.HilbertCurvePoints.ElementAt(i), 2, hilbertCurve.HilbertCurvePoints, 1);
        }

        //HashSet<Vector2Int> hilbertPointsInsdeMap = new HashSet<Vector2Int>();
        //for (int i = 0; i < initialCount; i++)
        //{
        //    Vector2Int hilberCurvePoint = hilbertCurve.HilbertCurvePoints.ElementAt(i);

        //    if(hilberCurvePoint.x>=minOffsetX && hilberCurvePoint.y>=minOffsetY && hilberCurvePoint.x<mapWidth+maxOffsetX && hilberCurvePoint.y<mapHeight+maxOffsetY)
        //    {
        //        //tilemapVisualizer.PaintSinglePathTile(hilberCurvePoint);
        //        hilbertPointsInsdeMap.Add(hilberCurvePoint);
        //        if(i-1>0)
        //        {
        //            Vector2Int hilberCurvePoint2 = hilbertCurve.HilbertCurvePoints.ElementAt(i - 1);
        //            if (hilberCurvePoint2.x < minOffsetX || hilberCurvePoint2.y < minOffsetY || hilberCurvePoint2.x >= mapWidth + maxOffsetX || hilberCurvePoint2.y >= mapHeight + maxOffsetY)
        //            {
        //                tilemapVisualizer.PaintSinglePathTile(hilberCurvePoint);
        //            }
        //        }
               
        //        Vector2Int hilberCurvePoint1 = hilbertCurve.HilbertCurvePoints.ElementAt(i+1);
        //        if (hilberCurvePoint1.x < minOffsetX || hilberCurvePoint1.y < minOffsetY || hilberCurvePoint1.x >= mapWidth + maxOffsetX || hilberCurvePoint1.y >= mapHeight + maxOffsetY)
        //        {
        //            tilemapVisualizer.PaintSinglePathTile(hilberCurvePoint);
        //        }
        //    }
        //}


        //foreach (Vector2Int pos in hilbertPointsInsdeMap)
        //{
        //    if(pos.x < minOffsetX && pos.y < minOffsetY && pos.x >= mapWidth + maxOffsetX && pos.y >= mapHeight + maxOffsetY)
        //    {
        //        tilemapVisualizer.PaintSinglePathTile(pos);
        //    }
        //}

        //tilemapVisualizer.PaintPathTiles(hilbertCurve.HilbertCurvePoints);
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
        //SetFillPercentBasedOnRule();

        int[,] map = (useHilbertCurve)? new int[hilbertMapWidth, hilbertMapHeight]: new int[mapWidth, mapHeight];

        for (int x = mapOffsetX; x < mapWidth+ mapOffsetX; x++)
        {
            for (int y = mapOffsetY; y < mapHeight+ mapOffsetY; y++)
            {
                if (x == mapOffsetX || x == mapWidth+ mapOffsetX - 1 || y == mapOffsetY || y == mapHeight + mapOffsetY - 1)
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
            for (int x = mapOffsetX; x < mapWidth + mapOffsetX; x++)
            {             
                for (int y = mapOffsetY; y < mapHeight + mapOffsetY; y++)
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

            if (neighbourX >= mapOffsetX && neighbourX < mapWidth + mapOffsetX && neighbourY >= mapOffsetY && neighbourY < mapHeight + mapOffsetY)
            {
                nWalls += map[neighbourX, neighbourY];
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
        List<List<Vector2Int>> floorRegions = (useHilbertCurve)? GetRegionsOfType(map, 0, hilbertMapWidth, hilbertMapHeight, mapOffsetX, mapWidth + mapOffsetX, mapOffsetY, mapHeight + mapOffsetY):
            GetRegionsOfType(map, 0, mapWidth, mapHeight);

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
                if(useHilbertCurve) leftFloorRegions.Add(new Region(floorRegion, map, hilbertMapWidth, hilbertMapHeight));
                else leftFloorRegions.Add(new Region(floorRegion, map, mapWidth, mapHeight));
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
        List<List<Vector2Int>> wallRegions = (useHilbertCurve) ? GetRegionsOfType(map, 1, hilbertMapWidth, hilbertMapHeight, mapOffsetX, mapWidth + mapOffsetX, mapOffsetY, mapHeight + mapOffsetY) :
           GetRegionsOfType(map, 1, mapWidth, mapHeight);

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
                    if (drawX >= mapOffsetX + 1 && drawX < mapWidth + mapOffsetX - 1 && drawY >= mapOffsetY + 1 && drawY < mapHeight + mapOffsetY - 1)
                    {
                        if (map != null) map[drawX, drawY] = cellType;
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Random = UnityEngine.Random;

//Struct for storing data for each map cell
public struct Cell
{
    private Vector2Int cellPos;
    private Vector2Int closestSeed;
    private int cellType; //1 -> wall , 0 -> floor, 2 -> erased cell

    public Vector2Int CellPos { get => cellPos; set => cellPos = value; }
    public Vector2Int ClosestSeed { get => closestSeed; set => closestSeed = value; }
    public int CellType { get => cellType; set => cellType = value; }

    public Cell(Vector2Int cellPos)
    {
        this.cellPos = cellPos;

        closestSeed = new Vector2Int();
        cellType = 0;
    }
}

public class VoronoiDiagramAlgorithm : DungeonGenerator
{
    [Range(50,200)]
    [SerializeField] private int mapWidth = 40, mapHeight = 40;
    [Range(2,100)]
    [SerializeField] private int numberOfSeeds = 64;
    [SerializeField] private Distance.DistanceAlgorithm distanceAlgorithm;
    [Range(0.0f,1.0f)]
    [SerializeField] private float wallErosion = 0.5f;
    [SerializeField] private bool randomShape = false;
    [SerializeField] private bool addExtraPaths = false;
    [SerializeField] private bool makeWiderPaths = false;
    [SerializeField] private bool connectRegions = true;
    [Range(0, 15)]
    [SerializeField] private int wallThresholdSize = 5;
    [Range(0, 15)]
    [SerializeField] private int floorThresholdSize = 5;
    [SerializeField] private bool useRandomSeed = true;
    [SerializeField] private string seed;

    private Grid2D grid;
    private System.Random rng = null;

    //void Start()
    //{
    //    GenerateDungeon();
    //}

    public override void GenerateDungeon()
    {
        if (useRandomSeed) seed = Time.time.ToString();
        rng = new System.Random(seed.GetHashCode());

        tilemapVisualizer.ClearTilemap();
        grid = new Grid2D(mapWidth, mapHeight, tilemapVisualizer.GetCellRadius());

        if(randomShape)
        {
            HashSet<Vector2Int> seeds = GenerateSeeds(out HashSet<Vector2Int> borderSeeds);
            List<Cell> mapInfo = RunVoronoiDiagram(seeds);

            CreateWalls(mapInfo);

            Dictionary<Vector2Int, HashSet<Vector2Int>> borderSets = CreateSets(borderSeeds, mapInfo);
            EraseBorderSets(borderSets, borderSeeds,mapInfo);

            HashSet<Vector2Int> randomSeeds = seeds.Except(borderSeeds).ToHashSet();
            if (connectRegions) Connectivity.GenerateConnectivity(randomSeeds,mapInfo,mapWidth,mapHeight,grid,addExtraPaths,makeWiderPaths,tilemapVisualizer);

            EraseRegions(mapInfo);

            tilemapVisualizer.PaintPathTiles(randomSeeds);
            tilemapVisualizer.AddBorderWalls();
        }
        else
        {
            HashSet<Vector2Int> seeds = GenerateSeeds();
            List<Cell> mapInfo = RunVoronoiDiagram(seeds);
            CreateWalls(mapInfo);

            if(connectRegions) Connectivity.GenerateConnectivity(seeds, mapInfo, mapWidth, mapHeight, grid, addExtraPaths, makeWiderPaths, tilemapVisualizer);

            EraseRegions(mapInfo);

            //tilemapVisualizer.PaintPathTiles(seeds);     
        }
    }

    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawWireCube(new Vector3(mapWidth / 2, mapHeight / 2), new Vector3(mapWidth, mapHeight));
    //    if (primEdges != null)
    //    {
    //        foreach (Edge edge in primEdges)
    //        {
    //            Gizmos.DrawLine(new Vector3(edge.U.position.x, edge.U.position.y), new Vector3(edge.V.position.x, edge.V.position.y));
    //        }
    //    }
    //}

    /// <summary>
    /// Creates n "seeds"(positions) with a random position inside the map's boundaries
    /// </summary>
    /// <returns>Returns a hashset with all seeds positions</returns>
    private HashSet<Vector2Int> GenerateSeeds()
    {
        HashSet<Vector2Int> seeds = new HashSet<Vector2Int>();

        while(seeds.Count<numberOfSeeds)
        {
            int xPos = rng.Next(1, mapWidth-1);
            int yPos = rng.Next(1, mapHeight-1);

            seeds.Add(new Vector2Int(xPos, yPos));
        }

        return seeds;
    }

    /// <summary>
    /// Creates n "seeds"(positions) with a random position inside the map's boundaries with an offset. Also it generates
    /// seeds which are next to the map's boundaries 
    /// </summary>
    /// <param name="borderSeeds">Hashset with seeds which are nexto the map's boundaries</param>
    /// <returns>Returns a hashset with all seeds positions</returns>
    private HashSet<Vector2Int> GenerateSeeds(out HashSet<Vector2Int> borderSeeds)
    {
        HashSet<Vector2Int> seeds = new HashSet<Vector2Int>();

        borderSeeds = new HashSet<Vector2Int>();

        int offsetX = mapWidth / 16;
        int offsetY = mapHeight / 16;

        //Left
        for (int i = 0; i < mapHeight - offsetY; i += offsetY)
        {
            if (offsetY + i > mapHeight - offsetY)
            {
                borderSeeds.Add(new Vector2Int(mapWidth - offsetX, mapHeight - offsetY));
                continue;
            }
            borderSeeds.Add(new Vector2Int(offsetX, offsetY + i));
        }

        //Right
        for (int i = 0; i < mapHeight - offsetY; i += offsetY)
        {
            if (offsetY + i > mapHeight - offsetY) 
            {
                borderSeeds.Add(new Vector2Int(mapWidth - offsetX, mapHeight-offsetY));
                continue; 
            }

            borderSeeds.Add(new Vector2Int(mapWidth - offsetX, offsetY + i));
        }

        //Bottom
        for (int i = 0; i < mapWidth - offsetX; i += offsetX)
        {
            if (offsetX + i > mapWidth - offsetX)
            {
                borderSeeds.Add(new Vector2Int(mapWidth - offsetX, mapHeight - offsetY));
                continue;
            }
            borderSeeds.Add(new Vector2Int(offsetX + i, offsetY));
        }

        //Up
        for (int i = 0; i < mapWidth - offsetX; i += offsetX)
        {
            if (offsetX + i > mapWidth - offsetX)
            {
                borderSeeds.Add(new Vector2Int(mapWidth - offsetX, mapHeight - offsetY));
                continue;
            }
            borderSeeds.Add(new Vector2Int(offsetX + i, mapHeight - offsetY));
        }

        int padding = 8;
        while (seeds.Count < numberOfSeeds)
        {
            int xPos = rng.Next(offsetX + padding, mapWidth - offsetX - padding);
            int yPos = rng.Next(offsetY + padding, mapHeight - offsetY - padding);

            seeds.Add(new Vector2Int(xPos, yPos));
        }

        seeds.UnionWith(borderSeeds);
        return seeds;
    }

    /// <summary>
    /// Performs the Voronoi diagram algorithm using brute force
    /// </summary>
    /// <param name="seeds">HashSet with all seed positions</param>
    /// <returns></returns>
    private List<Cell> RunVoronoiDiagram(HashSet<Vector2Int> seeds)
    {
        List<Cell> mapInfo = new List<Cell>();
        for (int i = 0; i < mapWidth * mapHeight; i++) mapInfo.Add(new Cell());

        for (int i = 0; i < mapInfo.Count; i++)
        {
            int x = i % mapWidth;
            int y = i / mapWidth;

            //We get the closest seed to the current map position
            float minDistance = float.MaxValue;

            foreach (Vector2Int seed in seeds)
            {
                float distance = Distance.CalculateDistance(new Vector2Int(x, y), seed, distanceAlgorithm);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    Vector2Int nearestSeed = seed;
                    Cell cell = new Cell(new Vector2Int(x, y));                   
                    cell.ClosestSeed = nearestSeed;
                    mapInfo[i] = cell;
                }
            }
        }

        return mapInfo;
    }

    /// <summary>
    /// Creates the walls between all the sets
    /// </summary>
    /// <param name="mapInfo">List with info about every map seed</param>
    private void CreateWalls(List<Cell> mapInfo)
    {
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                Cell mySeed = mapInfo[MapXYtoIndex(x, y)];

                if (x == 0 || y == 0 || x == mapWidth - 1 || y == mapHeight - 1) //Map boundaries
                {
                    tilemapVisualizer.PaintSingleWallTile(new Vector2Int(x, y));
                    mySeed.CellType = 1; //Wall
                    mapInfo[MapXYtoIndex(x, y)] = mySeed;
                    grid.NodeFromWorldPoint(new Vector2Int(x, y)).SetIsWalkable(false);
                    continue;
                }

                //int neig = 0;
                //if (mapInfo[MapXYtoIndex(x - 1, y)].ClosestSeed != mySeed.ClosestSeed) neig++;
                //if (mapInfo[MapXYtoIndex(x + 1, y)].ClosestSeed != mySeed.ClosestSeed) neig++;
                //if (mapInfo[MapXYtoIndex(x, y - 1)].ClosestSeed != mySeed.ClosestSeed) neig++;
                //if (mapInfo[MapXYtoIndex(x, y + 1)].ClosestSeed != mySeed.ClosestSeed) neig++;
                //if (neig<2)
                //{
                //    tilemapVisualizer.PaintSingleFloorTile(new Vector2Int(x, y));
                //}
                //else
                //{
                //    tilemapVisualizer.PaintSingleWallTile(new Vector2Int(x, y));
                //}
            
                if (mapInfo[MapXYtoIndex(x - 1, y)].ClosestSeed != mySeed.ClosestSeed ||
                mapInfo[MapXYtoIndex(x + 1, y)].ClosestSeed != mySeed.ClosestSeed ||
                mapInfo[MapXYtoIndex(x, y - 1)].ClosestSeed != mySeed.ClosestSeed ||
                mapInfo[MapXYtoIndex(x, y + 1)].ClosestSeed != mySeed.ClosestSeed ||

                mapInfo[MapXYtoIndex(x - 1, y - 1)].ClosestSeed != mySeed.ClosestSeed ||
                mapInfo[MapXYtoIndex(x + 1, y + 1)].ClosestSeed != mySeed.ClosestSeed ||
                mapInfo[MapXYtoIndex(x - 1, y - 1)].ClosestSeed != mySeed.ClosestSeed ||
                mapInfo[MapXYtoIndex(x + 1, y + 1)].ClosestSeed != mySeed.ClosestSeed)
                {
                    if (rng.NextDouble() > wallErosion)
                    {
                        tilemapVisualizer.PaintSingleWallTile(new Vector2Int(x, y));
                        mySeed.CellType = 1; //Wall
                        mapInfo[MapXYtoIndex(x, y)] = mySeed;
                        //grid.NodeFromWorldPoint(new Vector2Int(x, y)).SetIsWalkable(false);
                    }
                    else tilemapVisualizer.PaintSingleFloorTile(new Vector2Int(x, y));
                }
                else
                {
                    tilemapVisualizer.PaintSingleFloorTile(new Vector2Int(x, y));
                }
            }
        }
    }

    /// <summary>
    /// Returns a dictionary which contains the set which belongs to each seed
    /// </summary>
    /// <param name="seeds">HasSet with all seed positions</param>
    /// <param name="mapInfo">List with info about every map seed</param>
    /// <returns>A dictionary with the closest map cells to each seed</returns>
    private Dictionary<Vector2Int, HashSet<Vector2Int>> CreateSets(HashSet<Vector2Int> seeds, List<Cell> mapInfo)
    {
        Dictionary<Vector2Int, HashSet<Vector2Int>> sets = new Dictionary<Vector2Int, HashSet<Vector2Int>>();

        foreach (Vector2Int seed in seeds)
        {
            /*if (!sets.ContainsKey(seed)) */
            sets.Add(seed, new HashSet<Vector2Int>());
        }

        foreach (Cell cell in mapInfo)
        {
            foreach (Vector2Int seed in seeds)
            {
                if (cell.ClosestSeed == seed)
                {
                    sets[seed].Add(cell.CellPos);
                }
            }
        }

        return sets;
    }

    /// <summary>
    /// Erases all the sets that belong to the border seeds
    /// </summary>
    /// <param name="borderSets">Dictionary which store all seeds sets</param>
    /// <param name="borderSeeds">HasSet with all border seeds positions</param>
    /// <param name="mapInfo">List with info about every map seed</param>
    private void EraseBorderSets(Dictionary<Vector2Int, HashSet<Vector2Int>> borderSets, HashSet<Vector2Int> borderSeeds, List<Cell> mapInfo)
    {
        for (int i = 0; i < borderSets.Count; i++)
        {     
            foreach (Vector2Int seed in borderSets[borderSeeds.ElementAt(i)])
            {
                tilemapVisualizer.EraseSingleTile(seed);
                grid.NodeFromWorldPoint(seed).SetIsWalkable(false);
                Cell cell = mapInfo[MapXYtoIndex(seed.x, seed.y)];
                cell.CellType = 2; //Erased cell
                mapInfo[MapXYtoIndex(seed.x, seed.y)] = cell;
            }
        }
    }

    /// <summary>
    /// Erases floor and wall regions which are bigger than a given trheshold
    /// </summary>
    /// <param name="mapInfo">List with info about every map seed</param>
    private void EraseRegions(List<Cell> mapInfo)
    {
        //We get a list with all cells types
        List<int> mapInfoCellType = new List<int>();
        for (int i = 0; i < mapWidth * mapHeight; i++) mapInfoCellType.Add(0);

        for (int i = 0; i < mapWidth * mapHeight; i++)
        {
            int x = i % mapWidth;
            int y = i / mapWidth;

            int type = mapInfo[MapXYtoIndex(x, y)].CellType;

            mapInfoCellType[MapXYtoIndex(x, y)] = type;
        }

        //Erase wall regions
        List<List<Vector2Int>> wallRegions = FloodFillAlgorithm.GetRegionsOfType(mapInfoCellType, 1, mapWidth, mapHeight);

        foreach (List<Vector2Int> region in wallRegions)
        {
            if (region.Count <= wallThresholdSize)
            {
                foreach (Vector2Int pos in region)
                {
                    Cell cell = mapInfo[MapXYtoIndex(pos.x, pos.y)];
                    cell.CellType = 0;
                    mapInfo[MapXYtoIndex(pos.x, pos.y)] = cell;
                    tilemapVisualizer.PaintSingleFloorTile(pos);
                }               
            }
        }

        //Erase floor regions
        List<List<Vector2Int>> floorRegions = FloodFillAlgorithm.GetRegionsOfType(mapInfoCellType, 0, mapWidth, mapHeight);
        
        foreach (List<Vector2Int> region in floorRegions)
        {
            if (region.Count <= floorThresholdSize)
            {
                foreach (Vector2Int pos in region)
                {
                    Cell cell = mapInfo[MapXYtoIndex(pos.x, pos.y)];
                    cell.CellType = 1;
                    mapInfo[MapXYtoIndex(pos.x, pos.y)] = cell;
                    tilemapVisualizer.PaintSingleWallTile(pos);
                }
            }
        }
    }

    /// <summary>
    /// Converts a map position to an index
    /// </summary>
    /// <param name="x">X map position</param>
    /// <param name="y">Y map position</param>
    /// <returns>A integer which represents a map position</returns>
    private int MapXYtoIndex(int x, int y)
    {
        return x + (y * mapWidth);
    }
}
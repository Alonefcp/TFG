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
    private int cellType; //1 -> wall , 0 -> floor, 2 -> erased border cell

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

public class VoronoiDiagramAlgorithm : DungeonGeneration
{
    [Range(50,200)]
    [SerializeField] private int mapWidth = 40, mapHeight = 40;
    [Range(2,100)]
    [SerializeField] private int numberOfSeeds = 64;
    [SerializeField] private Distance.DistanceFormula distanceFormula;
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
 
    [SerializeField] private bool randomSeeds = true;
    [Range(5, 20)]
    [SerializeField] private float seedMinDistance=10;
    [Range(21, 70)]
    [SerializeField] private float seedMaxDistance=50;

    private Grid2D grid;

    public bool RandomSeeds { get => randomSeeds;}

    //void Start()
    //{
    //    GenerateDungeon();
    //}

    public override void GenerateDungeon()
    {
        base.GenerateDungeon();

        grid = new Grid2D(mapWidth, mapHeight, tilemapVisualizer.GetCellRadius());

        if(randomShape)
        {
            HashSet<Vector2Int> seeds = GenerateSeeds(out HashSet<Vector2Int> borderSeeds);
            List<Cell> mapInfo = RunVoronoiDiagram(seeds);

            CreateInnerWalls(mapInfo);

            Dictionary<Vector2Int, HashSet<Vector2Int>> borderSets = CreateSets(borderSeeds, mapInfo);
            EraseBorderSets(borderSets, borderSeeds,mapInfo);

            HashSet<Vector2Int> randomSeeds = seeds.Except(borderSeeds).ToHashSet();
            if (connectRegions) Connectivity.GenerateConnectivity(randomSeeds,mapInfo,mapWidth,mapHeight,grid,addExtraPaths,makeWiderPaths,tilemapVisualizer);

            EraseRegions(mapInfo);

            //tilemapVisualizer.PaintPathTiles(randomSeeds);

            DrawMap(mapInfo);
           
            playerController.SetPlayer(seeds.ElementAt(Random.Range(0, randomSeeds.Count)), new Vector3(0.3f, 0.3f, 0.3f));
        }
        else
        {
            HashSet<Vector2Int> seeds = RandomSeeds? GenerateSeeds(1,1,mapWidth-1,mapHeight-1): GenerateSeedBasedOnDistance(1,1,mapWidth-1,mapHeight-1);
            List<Cell> mapInfo = RunVoronoiDiagram(seeds);
            CreateInnerWalls(mapInfo);

            if(connectRegions) Connectivity.GenerateConnectivity(seeds, mapInfo, mapWidth, mapHeight, grid, addExtraPaths, makeWiderPaths, tilemapVisualizer);

            EraseRegions(mapInfo);

            DrawMap(mapInfo);

            playerController.SetPlayer(seeds.ElementAt(Random.Range(0, seeds.Count)), new Vector3(0.3f, 0.3f, 0.3f));
           
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
    /// Draws the map
    /// </summary>
    /// <param name="mapInfo">Map information</param>
    private void DrawMap(List<Cell> mapInfo)
    {
        for (int i = 0; i < mapInfo.Count; i++)
        {
            if (mapInfo[i].CellType == 0) //Floor
            {
                tilemapVisualizer.PaintSingleFloorTile(mapInfo[i].CellPos);
            }
            else if (mapInfo[i].CellType == 1) //Wall
            {
                tilemapVisualizer.PaintSingleInnerWallTile(mapInfo[i].CellPos);
            }
        }

        HashSet<Cell> cellFloors = mapInfo.Where(cell => cell.CellType == 0).ToHashSet();
        HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();
        foreach (Cell cell in cellFloors)
        {
            floorPositions.Add(cell.CellPos);
        }
        WallGenerator.CreateWalls(floorPositions, tilemapVisualizer);
    }

    /// <summary>
    /// Creates n "seeds"(positions) with a random position inside the map's boundaries
    /// </summary>
    /// <returns>Returns a hashset with all seeds positions</returns>
    private HashSet<Vector2Int> GenerateSeeds(int minWidth, int minHeight, int maxWidth, int maxHeight)
    {
        HashSet<Vector2Int> seeds = new HashSet<Vector2Int>();

        while(seeds.Count<numberOfSeeds)
        {
            int xPos = Random.Range(minWidth, maxWidth);
            int yPos = Random.Range(minHeight, maxHeight);

            seeds.Add(new Vector2Int(xPos, yPos));
        }

        return seeds;
    }

    /// <summary>
    /// Creates n "seeds"(positions) with a random position inside the map's boundaries. The seeds positions
    /// are based on the distance of the previous seed
    /// </summary>
    /// <returns>Returns a hashset with all seeds positions</returns>
    private HashSet<Vector2Int> GenerateSeedBasedOnDistance(int minWidth, int minHeight,int maxWidth, int maxHeight)
    {
        HashSet<Vector2Int> seeds = new HashSet<Vector2Int>();

        Vector2Int prevSeed = new Vector2Int(Random.Range(minWidth, maxWidth), Random.Range(minHeight, maxHeight));
        seeds.Add(prevSeed);

        while (seeds.Count < numberOfSeeds)
        {
            int xPos = Random.Range(minWidth, maxWidth);
            int yPos = Random.Range(minHeight, maxHeight);

            Vector2Int newSeed = new Vector2Int(xPos, yPos);
            if (Vector2Int.Distance(newSeed, prevSeed) <= seedMaxDistance && Vector2Int.Distance(newSeed, prevSeed) >= seedMinDistance)
            {
                seeds.Add(newSeed);
                prevSeed = newSeed;
            }
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

        int padding = 7;
        HashSet<Vector2Int> seeds = RandomSeeds ? GenerateSeeds(offsetX + padding, offsetY + padding, mapWidth - offsetX - padding, mapHeight - offsetY - padding) : GenerateSeedBasedOnDistance(offsetX + padding, offsetY + padding, mapWidth - offsetX - padding, mapHeight - offsetY - padding);
      
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
                float distance = Distance.CalculateDistance(new Vector2Int(x, y), seed, distanceFormula);

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
    private void CreateInnerWalls(List<Cell> mapInfo)
    {
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                Cell mySeed = mapInfo[MapXYtoIndex(x, y)];

                if (x == 0 || y == 0 || x == mapWidth - 1 || y == mapHeight - 1) //Map boundaries
                {
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
                    if (Random.value > wallErosion)
                    {
                        mySeed.CellType = 1; //Wall
                        mapInfo[MapXYtoIndex(x, y)] = mySeed;
                        //grid.NodeFromWorldPoint(new Vector2Int(x, y)).SetIsWalkable(false);
                    }
                    else 
                    { 
                        mySeed.CellType = 0;
                    }
                }
                else
                {
                    mySeed.CellType = 0;
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
                grid.NodeFromWorldPoint(seed).SetIsWalkable(false);
                Cell cell = mapInfo[MapXYtoIndex(seed.x, seed.y)];
                cell.CellType = 2; //Erased border cell
                mapInfo[MapXYtoIndex(seed.x, seed.y)] = cell;
            }
        }
    }

    /// <summary>
    /// Erases floor and wall regions which are bigger than a given threshold
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
                }
            }
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
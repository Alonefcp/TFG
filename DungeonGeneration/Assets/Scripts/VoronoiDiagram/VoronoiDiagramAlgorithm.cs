using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Random = UnityEngine.Random;

//Struct for storing data for each map cell
public struct Cell
{
    public Vector2Int cellPos;
    public Vector2Int belongSeed; //closestSeed
    public Cell(Vector2Int _cellPos)
    {
        cellPos = _cellPos;
        
        belongSeed = new Vector2Int();
    }

    public void SetBelongSeed(Vector2Int seed) 
    {
        belongSeed = seed;
    }
    public void SetCellPos(Vector2Int pos)
    {
        cellPos = pos;
    }
}

public class VoronoiDiagramAlgorithm : DungeonGenerator
{
    public enum DistanceAlgorithm {Euclidean, Manhattan, Chebyshev}

    [Range(20,150)]
    [SerializeField] private int mapWidth = 40, mapHeight = 40;
    [Range(2,100)]
    [SerializeField] private int numberOfSeeds = 64;
    [SerializeField] private DistanceAlgorithm distanceAlgorithm;
    [Range(0.0f,1.0f)]
    [SerializeField] private float wallErosion = 0.5f;
    [SerializeField] private bool randomShape = false;
    [SerializeField] private bool addExtraPaths = false;
    [SerializeField] private bool eliminateSingleWalls = false;
    [SerializeField] private bool eliminateSingleFloors = false;

    private Grid2D grid;
    private List<Edge> edges;
    private HashSet<Edge> primEdges;

    //void Start()
    //{
    //    GenerateDungeon();
    //}

    public override void GenerateDungeon()
    {       
        grid = new Grid2D(mapWidth, mapHeight, tilemapVisualizer.GetCellRadius());

        if(randomShape)
        {
            HashSet<Vector2Int> seeds = GenerateSeeds(out HashSet<Vector2Int> borderSeeds);
            List<Cell> mapInfo = VoronoiDiagram(seeds);

            tilemapVisualizer.ClearTilemap();
            CreateWalls(mapInfo);

            Dictionary<Vector2Int, HashSet<Vector2Int>> borderSets = CreateSets(borderSeeds, mapInfo);
            EraseBorderSets(borderSets, borderSeeds);

            HashSet<Vector2Int> randomSeeds = seeds.Except(borderSeeds).ToHashSet();   
            GenerateConnectivity(randomSeeds);

            //tilemapVisualizer.PaintPathTiles(randomSeeds);
            tilemapVisualizer.AddBorderWalls();
        }
        else
        {
            HashSet<Vector2Int> seeds = GenerateSeeds();
            List<Cell> mapInfo = VoronoiDiagram(seeds);

            tilemapVisualizer.ClearTilemap();
            CreateWalls(mapInfo);

            GenerateConnectivity(seeds);
            //tilemapVisualizer.PaintPathTiles(seeds);     
        }

        if(eliminateSingleWalls) tilemapVisualizer.EliminateSingleWalls();
        if (eliminateSingleFloors) tilemapVisualizer.EliminateSingleFloors(); 
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
    /// Creates n "seeds"(positions) with a random position inside the map's boundaries.
    /// </summary>
    /// <returns></returns>
    private HashSet<Vector2Int> GenerateSeeds()
    {
        HashSet<Vector2Int> seeds = new HashSet<Vector2Int>();

        while(seeds.Count<numberOfSeeds)
        {
            int xPos = Random.Range(1, mapWidth-1);
            int yPos = Random.Range(1, mapHeight-1);

            seeds.Add(new Vector2Int(xPos, yPos));
        }

        return seeds;
    }

    /// <summary>
    /// Creates n "seeds"(positions) with a random position inside the map's boundaries. Also it generates
    /// seeds which are near to the map's boundaries.
    /// </summary>
    /// <returns></returns>
    private HashSet<Vector2Int> GenerateSeeds(out HashSet<Vector2Int> borderSeeds)
    {
        HashSet<Vector2Int> seeds = new HashSet<Vector2Int>();

        borderSeeds = new HashSet<Vector2Int>();

        int offsetX = mapWidth / 16;
        int offsetY = mapHeight / 16;

        for (int i = 0; i < mapHeight - offsetY; i += offsetY)
        {
            borderSeeds.Add(new Vector2Int(offsetX, offsetY + i));
        }

        for (int i = 0; i < mapHeight - offsetY; i += offsetY)
        {           
            borderSeeds.Add(new Vector2Int(mapWidth - offsetX, offsetY + i));
        }

        for (int i = 0; i < mapWidth - offsetX; i += offsetX)
        {
            borderSeeds.Add(new Vector2Int(offsetX + i, offsetY));
        }

        for (int i = 0; i < mapWidth - offsetX; i += offsetX)
        {
            borderSeeds.Add(new Vector2Int(offsetX + i, mapHeight - offsetY));
            
        }

        while (seeds.Count < numberOfSeeds)
        {
            int xPos = Random.Range(offsetX + 1, mapWidth - offsetX);
            int yPos = Random.Range(offsetY + 1, mapHeight - offsetY);

            seeds.Add(new Vector2Int(xPos, yPos));
        }

        seeds.UnionWith(borderSeeds);
        return seeds;
    }

    /// <summary>
    /// Performs the Voronoi diagram algorithm using brute force.
    /// </summary>
    /// <param name="seeds">HashSet with all seed positions</param>
    /// <returns></returns>
    private List<Cell> VoronoiDiagram(HashSet<Vector2Int> seeds)
    {
        List<Cell> mapInfo = new List<Cell>(mapWidth * mapHeight);
        for (int i = 0; i < mapWidth * mapHeight; i++) mapInfo.Add(new Cell());

        for (int i = 0; i < mapInfo.Count; i++)
        {
            int x = i % mapWidth;
            int y = i / mapWidth;

            float mindistance = float.MaxValue;

            foreach (Vector2Int seed in seeds)
            {
                float distance = CalculateDistance(new Vector2Int(x, y), seed);

                if (distance < mindistance)
                {
                    mindistance = distance;
                    Vector2Int nearestSeed = seed;
                    Cell cell = new Cell();
                    cell.SetCellPos(new Vector2Int(x, y));
                    cell.SetBelongSeed(nearestSeed);
                    mapInfo[i] = cell;
                }
            }
        }

        return mapInfo;
    }

    /// <summary>
    /// Calculates the distance between two points with the distance algorithm chosen by the user.
    /// </summary>
    /// <param name="from">Start point</param>
    /// <param name="to">End point</param>
    /// <returns></returns>
    private float CalculateDistance(Vector2Int from, Vector2Int to)
    {
        float distance = 0;

        switch (distanceAlgorithm)
        {
            case DistanceAlgorithm.Euclidean:
                distance = EuclideanDistance(from, to);
                break;
            case DistanceAlgorithm.Manhattan:
                distance = ManhattanDistance(from, to);
                break;
            case DistanceAlgorithm.Chebyshev:
               distance = ChebyshevDistance(from, to);
                break;
            default:
                distance = EuclideanDistance(from, to);
                break;
        }

        return distance;
    }

    /// <summary>
    /// Creates the walls between all the sets.
    /// </summary>
    /// <param name="mapInfo">List with info about every map seed</param>
    private void CreateWalls(List<Cell> mapInfo)
    {
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                Cell mySeed = mapInfo[MapXYtoIndex(x, y)];

                if (x == 0 || y == 0 || x == mapWidth - 1 || y == mapHeight - 1) //map boundaries
                {
                    tilemapVisualizer.PaintSingleWallTile(new Vector2Int(x, y));
                    continue;
                }

                if (mapInfo[MapXYtoIndex(x - 1, y)].belongSeed != mySeed.belongSeed ||
                mapInfo[MapXYtoIndex(x + 1, y)].belongSeed != mySeed.belongSeed ||
                mapInfo[MapXYtoIndex(x, y - 1)].belongSeed != mySeed.belongSeed ||
                mapInfo[MapXYtoIndex(x, y + 1)].belongSeed != mySeed.belongSeed ||

                mapInfo[MapXYtoIndex(x - 1, y - 1)].belongSeed != mySeed.belongSeed ||
                mapInfo[MapXYtoIndex(x + 1, y + 1)].belongSeed != mySeed.belongSeed ||
                mapInfo[MapXYtoIndex(x - 1, y - 1)].belongSeed != mySeed.belongSeed ||
                mapInfo[MapXYtoIndex(x + 1, y + 1)].belongSeed != mySeed.belongSeed)
                {
                    if (Random.value > wallErosion)
                    {
                        tilemapVisualizer.PaintSingleWallTile(new Vector2Int(x, y));
                        //grid.NodeFromWorldPoint(new Vector3(x, y, 0)).SetIsWalkable(false);
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
    /// Makes a path between all the seeds.
    /// </summary>
    /// <param name="seeds">HashSet with all seed positions</param>
    private void GenerateConnectivity(HashSet<Vector2Int> seeds)
    {
        ////Delaunay triangulation 
        List<Vertex> vertex = new List<Vertex>();
        foreach (Vector2Int seed in seeds)
        {
            vertex.Add(new Vertex(seed));
        }

        edges = DelaunayTriangulation.Triangulate(vertex);

        if(edges.Count<=0) //if we can't make a graph with Delaunay
        {
            Vertex prev = vertex[0];
            for (int i = 1; i < vertex.Count; i++)
            {
                edges.Add(new Edge(prev, vertex[i]));
                prev = vertex[i];
            }
        }

        ConnectDisjointedSeeds(vertex, edges, seeds); //if we can make a graph with Delaunay but a seed is disjointed

        //Prim algorithm
        primEdges = PrimAlgorithm.RunMinimumSpanningTree(edges, addExtraPaths);

        //A* algorithm
        foreach (Edge edge in primEdges)
        {
            HashSet<Vector2Int> path = AstarPathfinding.FindPath(grid, edge.U.position, edge.V.position);
            if (path != null) tilemapVisualizer.PaintFloorTiles(path);
            else 
            {               
                Grid2D extraGrid = new Grid2D(mapWidth, mapHeight, tilemapVisualizer.GetCellRadius());            
                HashSet<Vector2Int> extraPath = AstarPathfinding.FindPath(extraGrid, edge.U.position, edge.V.position);
                MakeWiderPath(extraPath);
                tilemapVisualizer.PaintFloorTiles(extraPath);
            }
        }    
    }

    /// <summary>
    /// Make wider the path given by the user.
    /// </summary>
    /// <param name="path">The given path which wil become wider</param>
    private static void MakeWiderPath(HashSet<Vector2Int> path)
    {
        HashSet<Vector2Int> pathSides = new HashSet<Vector2Int>();

        //widdening path
        foreach (Vector2Int pos in path)
        {
            foreach (Vector2Int dir in GetDirectionsArray())
            {
                if (!path.Contains(pos + dir))
                {
                    pathSides.Add(pos + dir);
                }
            }
        }

        path.UnionWith(pathSides);
    }

    /// <summary>
    /// Connects the seeds that could't connect with the rest after the Delaunay triangulation.
    /// </summary>
    /// <param name="vertex">List with all Delaunay vertex</param>
    /// <param name="edges">List with all Delaunay edges</param>
    /// <param name="seeds">HashSet with all seed positions</param>
    private void ConnectDisjointedSeeds(List<Vertex> vertex, List<Edge> edges, HashSet<Vector2Int> seeds)
    {
        HashSet<Vector2Int> disjointedSeed = new HashSet<Vector2Int>();
        foreach (Vertex v in vertex)
        {
            foreach (Edge e in edges)
            {
                if (!(e.U.position == v.position && e.V.position == v.position))
                {
                    disjointedSeed.Add(new Vector2Int(v.position.x, v.position.y));
                }
            }
        }
        foreach (Vector2Int seed in disjointedSeed)
        {
            Vector2Int closestSeed = GetClosestSeed(seeds, seed);
            Vertex a = new Vertex(seed);
            Vertex b = new Vertex(closestSeed);
            edges.Add(new Edge(a, b));
        }
    }

    /// <summary>
    /// Returns a dictionary which contains the set which belong to each seed.
    /// </summary>
    /// <param name="seeds">HasSet with all seed positions</param>
    /// <param name="mapInfo">List with info about every map seed</param>
    /// <returns></returns>
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
                if (cell.belongSeed == seed)
                {
                    sets[seed].Add(cell.cellPos);
                }
            }
        }

        return sets;
    }

    /// <summary>
    /// Returns the closest seed to another one given by the user (destination parameter).
    /// </summary>
    /// <param name="seeds">HasSet with all seed positions</param>
    /// <param name="destination">We will find th closest seed to this one</param>
    /// <returns></returns>
    private Vector2Int GetClosestSeed(HashSet<Vector2Int> seeds, Vector2Int destination)
    {       
        float minDistance = float.MaxValue;
        Vector2Int closestSeed = new Vector2Int(0, 0);
        foreach (Vector2Int seed in seeds)
        {
            if (seed == destination) continue;

            float distance = Vector2Int.Distance(seed, destination);

            if (distance < minDistance)
            {
                minDistance = distance;
                closestSeed = seed;
            }
        }

        return closestSeed;
    }

    /// <summary>
    /// Erases all the sets that belong to the border seeds.
    /// </summary>
    /// <param name="borderSets">Dictionary which store all seeds sets</param>
    /// <param name="borderSeeds">HasSet with all border seeds positions</param>
    private void EraseBorderSets(Dictionary<Vector2Int, HashSet<Vector2Int>> borderSets, HashSet<Vector2Int> borderSeeds)
    {
        for (int i = 0; i < borderSets.Count; i++)
        {
            tilemapVisualizer.EraseTiles(borderSets[borderSeeds.ElementAt(i)]);

            foreach (Vector2Int seed in borderSets[borderSeeds.ElementAt(i)])
            {
                grid.NodeFromWorldPoint(seed).SetIsWalkable(false);
            }
        }
    }

    /// <summary>
    /// Conerts a map position to an index.
    /// </summary>
    /// <param name="x">X map position</param>
    /// <param name="y">Y map position</param>
    /// <returns></returns>
    private int MapXYtoIndex(int x, int y)
    {
        return x + (y * mapWidth);
    }

    /// <summary>
    /// Calculates the Manhattan distance: Abs(x1 - x2) + Abs(y1 - y2);
    /// </summary>
    /// <param name="from">Start point</param>
    /// <param name="to">End point</param>
    /// <returns></returns>
    private float ManhattanDistance(Vector2Int from, Vector2Int to)
    {
        return Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y);
    }

    /// <summary>
    /// Calculates the Manhattan distance: Max(Abs(x1 - x2), Abs(y1 - y2));
    /// </summary>
    /// <param name="from">Start point</param>
    /// <param name="to">End point</param>
    /// <returns></returns>
    private float ChebyshevDistance(Vector2Int from, Vector2Int to)
    {
        return Mathf.Max(Mathf.Abs(from.x - to.x), Mathf.Abs(from.y - to.y));
    }

    /// <summary>
    /// Calculates the Euclidean distance: Sqrt((x1 - x2)^2 + (y1 - y2)^2);
    /// </summary>
    /// <param name="from">Start point</param>
    /// <param name="to">End point</param>
    /// <returns></returns>
    private float EuclideanDistance(Vector2Int from, Vector2Int to) 
    {
        return /*Math.Abs(*/(from - to).magnitude/*)*/;
	}

    /// <summary>
    /// Returns an array with this directions: (1,0), (-1,0), (0,1), (0,-1),
    /// (1,1),(-1,1),(1,-1),(-1,-1).
    /// </summary>
    /// <returns></returns>
    private static Vector2Int[] GetDirectionsArray()
    {
        Vector2Int[] directions = { new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1),
            new Vector2Int(1, 1), new Vector2Int(-1, 1), new Vector2Int(1, -1), new Vector2Int(-1, -1)};

        return directions;
    }
}
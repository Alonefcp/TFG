using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Random = UnityEngine.Random;

public struct Cell
{
    public Vector2Int cellPos;
    public Vector2Int belongSeed; //semilla a la que esta mas cerca
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
    public enum DistanceAlgorithm {Euclidean, Manhattan}

    [SerializeField] private int mapWidth = 40, mapHeight = 40;
    [Range(2,100)]
    [SerializeField] private int numberOfSeeds = 64;
    [SerializeField] private DistanceAlgorithm distanceAlgorithm;
    [Range(0.0f,1.0f)]
    [SerializeField] private float wallErosion = 0.5f;
    [SerializeField] private bool randomShape = false;
    [SerializeField] private bool eliminateSingleWalls = false;
    [SerializeField] private bool eliminateSingleFloors = false;

    private Grid2D grid;
    private List<Edge> edges;

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

            tilemapVisualizer.AddBorderWalls();
            //tilemapVisualizer.PaintPathTiles(randomSeeds);
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
    //    if (edges != null)
    //    {
    //        foreach (Edge edge in edges)
    //        {
    //            Gizmos.DrawLine(edge.U.position, edge.V.position);
    //        }
    //    }
    //}

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

    private HashSet<Vector2Int> GenerateSeeds(out HashSet<Vector2Int> borderSeeds)
    {
        HashSet<Vector2Int> seeds = new HashSet<Vector2Int>();

        borderSeeds = new HashSet<Vector2Int>();

        int offsetX = mapWidth / 10;
        int offsetY = mapHeight / 10;

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
                float distance = (distanceAlgorithm == DistanceAlgorithm.Euclidean) ? EuclideanDistance(new Vector2Int(x, y), seed) : ManhattanDistance(new Vector2Int(x, y), seed); ;

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

    private void CreateWalls(List<Cell> mapInfo)
    {
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                Cell mySeed = mapInfo[MapXYtoIndex(x, y)];

                if (x == 0 || y == 0 || x == mapWidth - 1 || y == mapHeight - 1) //borders
                {
                    tilemapVisualizer.PaintSingleCorridorTile(new Vector2Int(x, y));
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
                        tilemapVisualizer.PaintSingleCorridorTile(new Vector2Int(x, y));
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

    private void GenerateConnectivity(HashSet<Vector2Int> seeds)
    {
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

        HashSet<Edge> primEdges = PrimAlgorithm.RunMinimumSpanningTree(edges, false);

        foreach (Edge edge in primEdges)
        {
            HashSet<Vector2Int> path = AstarPathfinding.FindPath(grid, edge.U.position, edge.V.position);
            if (path != null) tilemapVisualizer.PaintFloorTiles(path);
            else 
            {               
                Grid2D g = new Grid2D(mapWidth, mapHeight, tilemapVisualizer.GetCellRadius());            
                HashSet<Vector2Int> path1 = AstarPathfinding.FindPath(g, edge.U.position, edge.V.position);
                tilemapVisualizer.PaintFloorTiles(path1);
            }
        }    
    }

    private void ConnectDisjointedSeeds(List<Vertex> vertex, List<Edge> edges, HashSet<Vector2Int> seeds)
    {
        HashSet<Vector2Int> disjointedSeed = new HashSet<Vector2Int>();
        foreach (Vertex v in vertex)
        {
            foreach (Edge e in edges)
            {
                if (!(e.U.position == v.position && e.V.position == v.position))
                {
                    disjointedSeed.Add(new Vector2Int((int)v.position.x, (int)v.position.y));
                }
            }
        }
        foreach (Vector2Int seed in disjointedSeed)
        {
            Vector2Int closestSeed = GetClosestSeedTo(seeds, seed);
            Vertex a = new Vertex(seed);
            Vertex b = new Vertex(closestSeed);
            edges.Add(new Edge(a, b));
        }
    }

    private Dictionary<Vector2Int, HashSet<Vector2Int>> CreateSets(HashSet<Vector2Int> borderSeeds, List<Cell> mapInfo)
    {
        Dictionary<Vector2Int, HashSet<Vector2Int>> sets = new Dictionary<Vector2Int, HashSet<Vector2Int>>();

        foreach (Vector2Int seed in borderSeeds)
        {
            /*if (!sets.ContainsKey(seed)) */
            sets.Add(seed, new HashSet<Vector2Int>());
        }

        foreach (Cell cell in mapInfo)
        {
            foreach (Vector2Int seed in borderSeeds)
            {
                if (cell.belongSeed == seed)
                {
                    sets[seed].Add(cell.cellPos);
                }
            }
        }

        return sets;
    }

    private Vector2Int GetClosestSeedTo(HashSet<Vector2Int> seeds, Vector2Int destination)
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

    private int MapXYtoIndex(int x, int y)
    {
        return x + (y * mapWidth);
    }

    private float ManhattanDistance(Vector2Int from, Vector2Int to)
    {
        return Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y);
    }

    private float EuclideanDistance(Vector2Int from, Vector2Int to) 
    {
        return Math.Abs((from - to).magnitude);
	}
}
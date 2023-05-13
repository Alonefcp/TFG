using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Class used for connecting regions in voronoi maps
public static class Connectivity 
{
    /// <summary>
    /// Makes a path between all the seeds
    /// </summary>
    /// <param name="seeds">HashSet with all seed positions</param>
    /// <param name="mapInfo">List with info about every map cell</param>
    /// <param name="mapWidth">Map width</param>
    /// <param name="mapHeight">Map height</param>
    /// <param name="grid">Map grid for A*</param>
    /// <param name="addExtraPaths">If we want to add extra paths between seeds</param>
    /// <param name="makeWiderPaths">If we want to make wider paths</param>
    /// <param name="tilemapVisualizer">Tilemap visualizer for painting</param>
    public static void GenerateConnectivity(HashSet<Vector2Int> seeds, List<Cell> mapInfo, int mapWidth, int mapHeight,Grid2D grid,bool addExtraPaths, bool makeWiderPaths, TilemapVisualizer tilemapVisualizer)
    {
        //Delaunay triangulation 
        List<Vertex> vertex = new List<Vertex>();
        foreach (Vector2Int seed in seeds)
        {
            vertex.Add(new Vertex(seed));
        }

        List<Edge> edges = DelaunayTriangulation.Triangulate(vertex);

        //If we can't make a graph with Delaunay
        if (edges.Count <= 0)
        {
            Vertex prev = vertex[0];
            for (int i = 1; i < vertex.Count; i++)
            {
                edges.Add(new Edge(prev, vertex[i]));
                prev = vertex[i];
            }
        }

        //If we can make a graph with Delaunay but a seed is disjointed
        //ConnectDisjointedSeeds(vertex, edges, seeds);

        //Prim algorithm
        HashSet<Edge> primEdges = PrimAlgorithm.RunMinimumSpanningTree(edges, addExtraPaths);

        //A* algorithm
        foreach (Edge edge in primEdges)
        {
            //We connect all seeds
            HashSet<Vector2Int> path = AstarPathfinding.FindPath(grid, edge.U.position, edge.V.position);
            if (path != null)
            {
                if (makeWiderPaths) MakeWiderPath(path,mapWidth,mapHeight);

                foreach (Vector2Int pos in path)
                {
                    //tilemapVisualizer.PaintSingleFloorTile(pos);
                    Cell cell = mapInfo[MapXYtoIndex(pos.x, pos.y, mapWidth)];
                    cell.CellType = 0; //Floor
                    mapInfo[MapXYtoIndex(pos.x, pos.y, mapWidth)] = cell;
                }
            }
            else
            {
                //Extra path for connectiong the disjointed seed
                Grid2D extraGrid = new Grid2D(mapWidth, mapHeight, tilemapVisualizer.GetCellRadius());
                HashSet<Vector2Int> extraPath = AstarPathfinding.FindPath(extraGrid, edge.U.position, edge.V.position);
                MakeWiderPath(extraPath,mapWidth,mapHeight);
                foreach (Vector2Int pos in extraPath)
                {
                    //tilemapVisualizer.PaintSingleFloorTile(pos);
                    Cell cell = mapInfo[MapXYtoIndex(pos.x, pos.y, mapWidth)];
                    cell.CellType = 0; //Floor
                    mapInfo[MapXYtoIndex(pos.x, pos.y, mapWidth)] = cell;
                }
            }
        }
    }

    /// <summary>
    /// Make wider the path given by the user
    /// </summary>
    /// <param name="path">The given path which wil become wider</param>
    /// <param name="mapWidth">Map width</param>
    /// <param name="mapHeight">Map height</param>
    private static void MakeWiderPath(HashSet<Vector2Int> path, int mapWidth, int mapHeight)
    {
        HashSet<Vector2Int> pathSides = new HashSet<Vector2Int>();

        //Widdening path
        foreach (Vector2Int pos in path)
        {
            foreach (Vector2Int dir in Directions.GetEightDiretionsArray())
            {
                Vector2Int newPos = pos + dir;
                if(newPos.x>=1 && newPos.y>=1 && newPos.x < mapWidth-1 && newPos.y < mapHeight-1)
                {
                    if (!path.Contains(pos + dir))
                    {
                        pathSides.Add(pos + dir);
                    }
                }                
            }
        }

        path.UnionWith(pathSides);
    }

    /// <summary>
    /// Connects the seeds that could't connect with the rest after the Delaunay triangulation
    /// </summary>
    /// <param name="vertex">List with all Delaunay vertex</param>
    /// <param name="edges">List with all Delaunay edges</param>
    /// <param name="seeds">HashSet with all seed positions</param>
    //private static void ConnectDisjointedSeeds(List<Vertex> vertex, List<Edge> edges, HashSet<Vector2Int> seeds)
    //{
    //    HashSet<Vector2Int> disjointedSeed = new HashSet<Vector2Int>();
    //    foreach (Vertex v in vertex)
    //    {
    //        foreach (Edge e in edges)
    //        {
    //            if (!(e.U.position == v.position && e.V.position == v.position))
    //            {
    //                disjointedSeed.Add(new Vector2Int(v.position.x, v.position.y));
    //            }
    //        }
    //    }
    //    foreach (Vector2Int seed in disjointedSeed)
    //    {
    //        Vector2Int closestSeed = GetClosestSeed(seeds, seed);
    //        Vertex a = new Vertex(seed);
    //        Vertex b = new Vertex(closestSeed);
    //        edges.Add(new Edge(a, b));
    //    }
    //}

    /// <summary>
    /// Returns the closest seed to another one given by the user (destination parameter)
    /// </summary>
    /// <param name="seeds">HasSet with all seed positions</param>
    /// <param name="destination">We will find th closest seed to this one</param>
    /// <returns>The position of the closest seed</returns>
    //private static Vector2Int GetClosestSeed(HashSet<Vector2Int> seeds, Vector2Int destination)
    //{
    //    float minDistance = float.MaxValue;
    //    Vector2Int closestSeed = new Vector2Int(0, 0);
    //    foreach (Vector2Int seed in seeds)
    //    {
    //        if (seed == destination) continue;

    //        float distance = Vector2Int.Distance(seed, destination);

    //        if (distance < minDistance)
    //        {
    //            minDistance = distance;
    //            closestSeed = seed;
    //        }
    //    }

    //    return closestSeed;
    //}

    /// <summary>
    /// Converts a map position to an index
    /// </summary>
    /// <param name="x">X map position</param>
    /// <param name="y">Y map position</param>
    /// <param name="mapWidth">Map width</param>
    /// <returns>A integer which represents a map position</returns>
    private static int MapXYtoIndex(int x, int y, int mapWidth)
    {
        return x + (y * mapWidth);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CorridorsAlgorithms
{
    //================================================== Delaunay, Prim and A* ====================================================================

    /// <summary>
    /// Connects two rooms by using the Delaunay Triangulation, Prim algorithm and A*. It makes corridors between two rooms
    /// </summary>
    /// <param name="roomCenters">Rooms positions which are the rooms centers</param>
    /// <param name="grid">Map grid for A*</param>
    /// <param name="widerCorridors">If we want wider corridors</param>
    /// <param name="corridorSize">Corridor size</param>
    /// <param name="mapWidth">MapWidth</param>
    /// <param name="mapHeight">MapHeight</param>
    /// <param name="addSomeRemainingEdges">If we want to add the remaining edges of Prim algortihm</param>
    /// <returns></returns>
    public static List<HashSet<Vector2Int>> ConnectRooms(List<Vertex> roomCenters, Grid2D grid, bool widerCorridors, int corridorSize, int mapWidth, int mapHeight,bool addSomeRemainingEdges=false)
    {
        //Delaunay triangulation
        List<Edge> delaunayEdges = DelaunayTriangulation.Triangulate(roomCenters);

        //If we can't make a graph with Delaunay
        if (delaunayEdges.Count <= 0)
        {
            //We have more than 1 vertex
            if (roomCenters.Count > 1)
            {
                Vertex prev = roomCenters[0];
                for (int i = 1; i < roomCenters.Count; i++)
                {
                    delaunayEdges.Add(new Edge(prev, roomCenters[i]));
                    prev = roomCenters[i];
                }
            }
            else return new List<HashSet<Vector2Int>>(); //We have only 1 vertex   
        }

        //Prim algorithm
        HashSet<Edge> edges = PrimAlgorithm.RunMinimumSpanningTree(delaunayEdges, addSomeRemainingEdges);

        List<HashSet<Vector2Int>> paths = new List<HashSet<Vector2Int>>();
       
        foreach (Edge edge in edges)
        {
            //A* algorithm
            HashSet<Vector2Int> path = AstarPathfinding.FindPath(grid, edge.U.position, edge.V.position);

            if(widerCorridors)
            {
                MakeWiderCorridors(path,corridorSize, mapWidth, mapHeight);
            }

            paths.Add(path);
        }

        return paths;
    }


    //================================================== Tunneling Algorithm ======================================================================

    /// <summary>
    /// Connects two rooms by using a tunneling algortihm. It makes corridors between two rooms
    /// </summary>
    /// <param name="roomCenters">Rooms positions which are the rooms centers</param>
    /// <param name="widerCorridors">If we want wider corridors</param>
    /// <param name="corridorSize">Corridor size</param>
    /// <param name="mapWidth">MapWidth</param>
    /// <param name="mapHeight">MapHeight</param>
    /// <returns>Returns a hashset with all corridors positions</returns>
    public static HashSet<Vector2Int> ConnectRooms(List<Vertex> roomCenters, bool widerCorridors, int corridorSize, int mapWidth, int mapHeight)
    {
        List<Vertex> roomVertex = new List<Vertex>(roomCenters);
        HashSet<Vector2Int> corridors = new HashSet<Vector2Int>();

        Vertex currentVertex = roomVertex[Random.Range(0, roomVertex.Count)];
        Vector2Int currentRoomCenter = currentVertex.position;
        roomVertex.Remove(currentVertex);

        while (roomVertex.Count > 0)
        {
            Vertex closestVertex = FindClosestVertexTo(currentRoomCenter, roomVertex);
            Vector2Int closest = closestVertex.position;
            roomVertex.Remove(closestVertex);          

            HashSet<Vector2Int> newCorridor = CreateCorridor(currentRoomCenter, closest, widerCorridors,corridorSize, mapWidth, mapHeight);

            currentVertex = closestVertex;
            currentRoomCenter = closest;
            
            corridors.UnionWith(newCorridor);
        }

        return corridors;
    }

    /// <summary>
    /// Creates a corridor between two rooms
    /// </summary>
    /// <param name="currentRoomCenter">Current room position</param>
    /// <param name="destination">Destination room position</param>
    /// <param name="widerCorridors">If we want wider corridors</param>
    /// <param name="corridorSize">Corridor size</param>
    /// <param name="mapWidth">MapWidth</param>
    /// <param name="mapHeight">MapHeight</param>
    /// <returns>Returns a hashset with all corridor positions</returns>
    private static HashSet<Vector2Int> CreateCorridor(Vector2Int currentRoomCenter, Vector2Int destination, bool widerCorridors,int corridorSize, int mapWidth, int mapHeight)
    {
        HashSet<Vector2Int> corridor = new HashSet<Vector2Int>();

        Vector2Int position = currentRoomCenter;
        corridor.Add(position);

        while (position.y != destination.y)
        {
            if (destination.y > position.y)
            {
                position += new Vector2Int(0, 1);
            }
            else if (destination.y < position.y)
            {
                position += new Vector2Int(0, -1);
            }

            corridor.Add(position);
        }

        while (position.x != destination.x)
        {
            if (destination.x > position.x)
            {
                position += new Vector2Int(1, 0);
            }
            else if (destination.x < position.x)
            {
                position += new Vector2Int(-1, 0);
            }

            corridor.Add(position);
        }


        if (widerCorridors)
        {
            MakeWiderCorridors(corridor,corridorSize, mapWidth, mapHeight);
        }

        return corridor;
    }


    /// <summary>
    /// Finds the closest room to the current room position
    /// </summary>
    /// <param name="currentRoomCenter">Current room position</param>
    /// <param name="roomCenters">Rooms positions which are the rooms centers</param>
    /// <returns></returns>
    private static Vertex FindClosestVertexTo(Vector2Int currentRoomCenter, List<Vertex> roomCenters)
    {
        Vertex closest = null;
        float distance = float.MaxValue;

        foreach (Vertex vertex in roomCenters)
        {
            float currentDistance = Vector2.Distance(vertex.position, currentRoomCenter);

            if (currentDistance < distance)
            {
                distance = currentDistance;
                closest = vertex;
            }
        }

        return closest;
    }


    //===================================================== Auxiliar methods =================================================
    /// <summary>
    /// Makes wider corridors
    /// </summary>
    /// <param name="corridor">Corridor positions</param>
    /// <param name="corridorSize">Corridor size</param>
    /// <param name="mapWidth">MapWidth</param>
    /// <param name="mapHeight">MapHeight</param>
    private static void MakeWiderCorridors(HashSet<Vector2Int> corridor,int corridorSize,int mapWidth, int mapHeight)
    {
        HashSet<Vector2Int> corridorSides = new HashSet<Vector2Int>();

        //widdening corridor
        foreach (Vector2Int pos in corridor)
        {         
            DrawBiggerTile(pos, corridorSize,corridorSides, mapWidth, mapHeight);
        }

        corridor.UnionWith(corridorSides);
    }

    /// <summary>
    /// Adds extra positions around a given position
    /// </summary>
    /// <param name="pos">Given position</param>
    /// <param name="size">Corridor size</param>
    /// <param name="corridorSides">Corridor extra positions</param>
    /// <param name="mapWidth">MapWidth</param>
    /// <param name="mapHeight">MapHeight</param>
    private static void DrawBiggerTile(Vector2Int pos, int size, HashSet<Vector2Int> corridorSides,int mapWidth,int mapHeight)
    {
        for (int x = -size; x <= size; x++)
        {
            for (int y = -size; y <= size; y++)
            {
                if (x * x + y * y <= size * size)
                {
                    int drawX = pos.x + x;
                    int drawY = pos.y + y;
                    if (drawX >= 0 && drawX < mapWidth && drawY >= 0 && drawY < mapHeight)
                    {
                        corridorSides.Add(new Vector2Int(drawX, drawY));
                    }
                }
            }
        }
    }
}

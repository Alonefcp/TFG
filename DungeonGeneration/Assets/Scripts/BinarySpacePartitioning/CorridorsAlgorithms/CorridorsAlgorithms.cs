using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CorridorsAlgorithms
{
    //================================================== Delaunay, Prim and A* ====================================================================

    public static List<HashSet<Vector2Int>> ConnectRooms(List<Vertex> roomCentersForDelaunay, Grid2D grid, bool widerCorridors, bool addSomeRemainingEdges=false)
    {
        //Delaunay triangulation
        List<Edge> delaunayEdges = DelaunayTriangulation.Triangulate(roomCentersForDelaunay);

        //Prim algorithm
        HashSet<Edge> edges = PrimAlgorithm.RunMinimumSpanningTree(delaunayEdges, addSomeRemainingEdges);

        List<HashSet<Vector2Int>> paths = new List<HashSet<Vector2Int>>();
       
        foreach (var edge in edges)
        {
            //A* algorithm
            HashSet<Vector2Int> path = AstarPathfinding.FindPath(grid, edge.U.position, edge.V.position);

            if(widerCorridors)
            {
                MakeWiderCorridors(path);
            }

            paths.Add(path);
        }

        return paths;
    }

    
    //================================================== Tunneling Algorithm ======================================================================
    public static HashSet<Vector2Int> ConnectRooms(List<Vertex> roomCenters, bool widerCorridors)
    {
        List<Vertex> roomVertex = new List<Vertex>(roomCenters);
        HashSet<Vector2Int> corridors = new HashSet<Vector2Int>();

        Vertex currentVertex = roomVertex[Random.Range(0, roomVertex.Count)];
        Vector2Int currentRoomCenter = (Vector2Int)Vector3Int.RoundToInt(currentVertex.position);
        roomVertex.Remove(currentVertex);

        while (roomVertex.Count > 0)
        {
            Vertex closestVertex = FindClosestVertexTo(currentRoomCenter, roomVertex);
            Vector2Int closest = (Vector2Int)Vector3Int.RoundToInt(closestVertex.position);
            roomVertex.Remove(closestVertex);          

            HashSet<Vector2Int> newCorridor = CreateCorridor(currentRoomCenter, closest, widerCorridors);

            currentVertex = closestVertex;
            currentRoomCenter = closest;
            
            corridors.UnionWith(newCorridor);
        }

        return corridors;
    }

    private static HashSet<Vector2Int> CreateCorridor(Vector2Int currentRoomCenter, Vector2Int destination, bool widerCorridors)
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
            MakeWiderCorridors(corridor);
        }

        return corridor;
    }



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

    private static void MakeWiderCorridors(HashSet<Vector2Int> corridor)
    {
        HashSet<Vector2Int> corridorSides = new HashSet<Vector2Int>();

        //widdening corridor
        foreach (Vector2Int pos in corridor)
        {
            foreach (Vector2Int dir in GetDirectionsArray())
            {
                if (!corridor.Contains(pos + dir))
                {
                    corridorSides.Add(pos + dir);
                }
            }
        }

        corridor.UnionWith(corridorSides);
    }

    private static Vector2Int[] GetDirectionsArray()
    {
        Vector2Int[] directions = { new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1),
            new Vector2Int(1, 1), new Vector2Int(-1, 1), new Vector2Int(1, -1), new Vector2Int(-1, -1)};

        return directions;
    }
}
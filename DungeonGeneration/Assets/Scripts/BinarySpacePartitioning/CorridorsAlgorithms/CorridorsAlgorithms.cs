using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphDataStructure;

public static class CorridorsAlgorithms
{
    //================================================== Delaunay, Prim and A* ====================================================================


    public static List<List<Vector2Int>> ConnectRooms(List<Vertex> roomCentersForDelaunay, Grid grid, bool addSomeRemainingEdges=false)
    {
        //Delaunay triangulation
        DelaunayTriangulation delaunayTriangulation = DelaunayTriangulation.Triangulate(roomCentersForDelaunay);

        //Prim algorithm
        HashSet<Edge> edges = PrimAlgorithm.RunMinimumSpanningTree(delaunayTriangulation.Edges, addSomeRemainingEdges);

        List<List<Vector2Int>> paths = new List<List<Vector2Int>>();

        
        foreach (var edge in edges)
        {
            //A* algorithm
            List<Vector2Int> path = AstarPathfinding.FindPath(grid, edge.U.Position, edge.V.Position);
            paths.Add(path);

            //foreach (var node in path)
            //{
            //    tilemapVisualizer.PaintSingleTile(new Vector2Int((int)node.worldPosition.x, (int)node.worldPosition.y));
            //}
        }

        return paths;
    }

    
    //================================================== Tunneling Algorithm ======================================================================
    public static HashSet<Vector2Int> ConnectRooms(List<Vector2Int> roomCenters, WeightedGraph<Vector2Int> graph, bool widerCorridors)
    {
        HashSet<Vector2Int> corridors = new HashSet<Vector2Int>();

        Vector2Int currentRoomCenter = roomCenters[Random.Range(0, roomCenters.Count)];
        roomCenters.Remove(currentRoomCenter);
        WeightedGraphNode<Vector2Int> currentRoomCenterNode = graph.AddNode(currentRoomCenter);

        while (roomCenters.Count > 0)
        {
            Vector2Int closest = FindClosestPointTo(currentRoomCenter, roomCenters);
            roomCenters.Remove(closest);
            WeightedGraphNode<Vector2Int> closestNode = graph.AddNode(closest);

            HashSet<Vector2Int> newCorridor = CreateCorridor(currentRoomCenter, closest, widerCorridors);

            graph.AddEdge(currentRoomCenterNode, closestNode, 0);

            currentRoomCenter = closestNode.Value;
            currentRoomCenterNode = closestNode;

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
            HashSet<Vector2Int> corridorSides = new HashSet<Vector2Int>();

            //widdening corridor
            foreach (Vector2Int Corridor in corridor)
            {
                foreach (Vector2Int dir in GetDirectionsArray())
                {
                    if (!corridor.Contains(Corridor + dir))
                    {
                        corridorSides.Add(Corridor + dir);
                    }
                }
            }

            corridor.UnionWith(corridorSides);
        }

        return corridor;
    }


    private static Vector2Int[] GetDirectionsArray()
    {
        Vector2Int[] directions = { new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1),
            new Vector2Int(1, 1), new Vector2Int(-1, 1), new Vector2Int(1, -1), new Vector2Int(-1, -1)};

        return directions;
    }

    private static Vector2Int FindClosestPointTo(Vector2Int currentRoomCenter, List<Vector2Int> roomCenters)
    {
        Vector2Int closest = Vector2Int.zero;
        float distance = float.MaxValue;

        foreach (Vector2Int position in roomCenters)
        {
            float currentDistance = Vector2.Distance(position, currentRoomCenter);

            if (currentDistance < distance)
            {
                distance = currentDistance;
                closest = position;
            }
        }

        return closest;
    }
}

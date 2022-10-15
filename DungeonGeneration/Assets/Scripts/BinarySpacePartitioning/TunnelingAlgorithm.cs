using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphDataStructure;

public class TunnelingAlgorithm : MonoBehaviour
{
    public HashSet<Vector2Int> ConnectRooms(List<Vector2Int> roomCenters, WeightedGraph<Vector2Int> graph, bool widerCorridors)
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

    public HashSet<Vector2Int> CreateCorridor(Vector2Int currentRoomCenter, Vector2Int destination, bool widerCorridors)
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


    private Vector2Int[] GetDirectionsArray()
    {
        Vector2Int[] directions = { new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1),
            new Vector2Int(1, 1), new Vector2Int(-1, 1), new Vector2Int(1, -1), new Vector2Int(-1, -1)};

        return directions;
    }

    private Vector2Int FindClosestPointTo(Vector2Int currentRoomCenter, List<Vector2Int> roomCenters)
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SpecialRooms
{
    /// <summary>
    /// Sets the start and end room. The end room is the furthest from the start room
    /// </summary>
    /// <param name="tilemapVisualizer">For marking the start and end room</param>
    /// <param name="roomCenters">Rooms positions which are the rooms centers</param>
    public static void SetStartAndEndRoom(TilemapVisualizer tilemapVisualizer, List<Vertex> roomCenters, out Vector2Int roomStartPosition, out Vector2Int roomEndPosition)
    {
        List<Vertex> rooms = new List<Vertex>(roomCenters);

        Vertex start = rooms[Random.Range(0, rooms.Count)];
        rooms.Remove(start);

        roomStartPosition = start.position;

        float maxDistance = -1;

        Vertex furthest = null;

        foreach (Vertex vertex in rooms)
        {
            float dist = Vector2Int.Distance(start.position, vertex.position);
            if (dist > maxDistance)
            {
                maxDistance = dist;
                furthest = vertex;
            }
        }

        roomEndPosition = furthest.position;       
    }
}

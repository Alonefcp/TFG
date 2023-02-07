using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SpecialRooms
{
    /// <summary>
    /// Sets the start room
    /// </summary>
    /// <param name="roomCenters">Rooms positions which are the rooms centers</param>
    /// <returns>A vector which is the start room position</returns>
    public static Vector2Int SetStartRoom(List<Vertex> roomCenters)
    {
        Vertex start = roomCenters[Random.Range(0, roomCenters.Count)];
        roomCenters.Remove(start);

        return start.position;
    }

    /// <summary>
    /// Sets the end room. The end room is the furthest from the start room
    /// </summary>
    /// <param name="roomCenters">Rooms positions which are the rooms centers</param>
    /// <param name="startPosition">Start room position</param>
    /// <returns>A vector which is the end room position</returns>
    public static Vector2Int SetEndRoom(List<Vertex> roomCenters, Vector2Int startPosition)
    {
        float maxDistance = -1;

        Vertex furthest = null;

        foreach (Vertex vertex in roomCenters)
        {
            float dist = Vector2Int.Distance(startPosition, vertex.position);
            if (dist > maxDistance)
            {
                maxDistance = dist;
                furthest = vertex;
            }
        }

        return furthest.position;       
    }
}

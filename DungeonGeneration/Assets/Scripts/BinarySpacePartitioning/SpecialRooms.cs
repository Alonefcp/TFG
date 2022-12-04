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
    public static void SetStartAndEndRoom(TilemapVisualizer tilemapVisualizer, List<Vertex> roomCenters)
    {
        Vertex start = roomCenters[Random.Range(0, roomCenters.Count)];
        roomCenters.Remove(start);

        tilemapVisualizer.PaintSingleWallTile(start.position);

        float maxDistance = -1;

        Vertex furthest = null;

        foreach (Vertex vertex in roomCenters)
        {
            float dist = Vector2Int.Distance(start.position, vertex.position);
            if (dist > maxDistance)
            {
                maxDistance = dist;
                furthest = vertex;
            }
        }

        tilemapVisualizer.PaintSingleWallTile(furthest.position);
    }
}

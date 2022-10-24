using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SpecialRooms
{
    public static void SetStartAndEndRoom(TilemapVisualizer tilemapVisualizer, List<Vertex> roomCentersForDelaunay)
    {
        Vertex start = roomCentersForDelaunay[Random.Range(0, roomCentersForDelaunay.Count)];
        roomCentersForDelaunay.Remove(start);

        tilemapVisualizer.PaintSingleCorridorTile(start.position);

        float maxDistance = -1;

        Vertex furthest = null;

        foreach (Vertex vertex in roomCentersForDelaunay)
        {
            float dist = Vector2Int.Distance(start.position, vertex.position);
            if (dist > maxDistance)
            {
                maxDistance = dist;
                furthest = vertex;
            }
        }

        tilemapVisualizer.PaintSingleCorridorTile(furthest.position);
    }
}

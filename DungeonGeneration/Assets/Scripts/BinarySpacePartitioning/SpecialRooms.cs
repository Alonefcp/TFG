using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SpecialRooms
{
    public static void SetStartAndEndRoom(TilemapVisualizer tilemapVisualizer, List<Vertex> roomCentersForDelaunay)
    {
        Vertex start = roomCentersForDelaunay[Random.Range(0, roomCentersForDelaunay.Count)];
        roomCentersForDelaunay.Remove(start);

        tilemapVisualizer.PaintSingleCorridorTile((Vector2Int)Vector3Int.RoundToInt(start.position));

        float maxDistance = -1;

        Vertex furthest = null;

        foreach (Vertex vertex in roomCentersForDelaunay)
        {
            float dist = Vector3.Distance(start.position, vertex.position);
            if (dist > maxDistance)
            {
                maxDistance = dist;
                furthest = vertex;
            }
        }

        tilemapVisualizer.PaintSingleCorridorTile((Vector2Int)Vector3Int.RoundToInt(furthest.position));
    }
}

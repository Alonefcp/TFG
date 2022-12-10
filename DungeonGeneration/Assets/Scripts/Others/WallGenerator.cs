using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class WallGenerator
{ 
    public static void CreateWalls(HashSet<Vector2Int> floorPositions, TilemapVisualizer tilemapVisualizer)
    {
        Vector2Int[] eightDirectionsArray = Directions.GetEightDiretionsArray();
        HashSet<Vector2Int> wallPositions = FindWallsInDirections(floorPositions, eightDirectionsArray);
        tilemapVisualizer.PaintWallTiles(wallPositions);

    }

    private static HashSet<Vector2Int> FindWallsInDirections(HashSet<Vector2Int> floorPositions, Vector2Int[] fourDirectionsArray)
    {
        HashSet<Vector2Int> wallPositions = new HashSet<Vector2Int>();

        foreach (Vector2Int position in floorPositions)
        {
            foreach (Vector2Int direction in fourDirectionsArray)
            {
                Vector2Int neighbourPosition = position + direction;
                if(!floorPositions.Contains(neighbourPosition))
                {
                    wallPositions.Add(neighbourPosition);
                }
            }
        }

        return wallPositions;
    }


    
}

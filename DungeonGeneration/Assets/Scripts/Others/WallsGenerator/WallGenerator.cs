using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class WallGenerator
{
    /// <summary>
    /// Creates the dungeon walls
    /// </summary>
    /// <param name="floorPositions">All floor positions</param>
    /// <param name="tilemapVisualizer">Tilemap visualizer for painting</param>
    public static void CreateWalls(HashSet<Vector2Int> floorPositions, TilemapVisualizer tilemapVisualizer)
    {
        HashSet<Vector2Int> basicWallPositions = FindWallsInDirections(floorPositions, Directions.GetFourDirectionsArray());
        HashSet<Vector2Int> cornerWallPositions = FindWallsInDirections(floorPositions, Directions.GetDiagonalsDirectionsArray());
        CreateBasicWall(tilemapVisualizer, basicWallPositions, floorPositions);
        CreateCornerWalls(tilemapVisualizer, cornerWallPositions, floorPositions);
    }

    /// <summary>
    ///  Creates corner walls positions, that is diagonal walls positions
    /// </summary>
    /// <param name="tilemapVisualizer">Tilemap visualizer for painting</param>
    /// <param name="cornerWallPositions">All corner walls positions </param>
    /// <param name="floorPositions">All floor positions</param>
    private static void CreateCornerWalls(TilemapVisualizer tilemapVisualizer, HashSet<Vector2Int> cornerWallPositions, HashSet<Vector2Int> floorPositions)
    {
        foreach (Vector2Int position in cornerWallPositions)
        {
            string neighboursBinaryType = "";
            foreach (Vector2Int direction in Directions.GetEightDiretionsArray())
            {
                Vector2Int neighbourPosition = position + direction;
                if (floorPositions.Contains(neighbourPosition))
                {
                    neighboursBinaryType += "1";
                }
                else
                {
                    neighboursBinaryType += "0";
                }
            }
            tilemapVisualizer.PaintSingleCornerWallTile(position, neighboursBinaryType);
        }
    }

    /// <summary>
    /// Creates basic walls positions, that is cardinal walls positions
    /// </summary>
    /// <param name="tilemapVisualizer">Tilemap visualizer for painting</param>
    /// <param name="basicWallPositions">All basic walls positions</param>
    /// <param name="floorPositions">All floor positions</param>
    private static void CreateBasicWall(TilemapVisualizer tilemapVisualizer, HashSet<Vector2Int> basicWallPositions, HashSet<Vector2Int> floorPositions)
    {
        foreach (Vector2Int position in basicWallPositions)
        {
            string neighboursBinaryType = "";
            foreach (Vector2Int direction in Directions.GetFourDirectionsArray())
            {
                Vector2Int neighbourPosition = position + direction;
                if (floorPositions.Contains(neighbourPosition))
                {
                    neighboursBinaryType += "1";
                }
                else
                {
                    neighboursBinaryType += "0";
                }
            }
            tilemapVisualizer.PaintSingleWallTile(position, neighboursBinaryType);
        }
    }

    /// <summary>
    /// Finds walls positions in a given directions
    /// </summary>
    /// <param name="floorPositions">All floor positions</param>
    /// <param name="directions">Directions in which we are going to see</param>
    /// <returns>A hashset with all wall positions</returns>
    private static HashSet<Vector2Int> FindWallsInDirections(HashSet<Vector2Int> floorPositions, Vector2Int[] directions)
    {
        HashSet<Vector2Int> wallPositions = new HashSet<Vector2Int>();
        foreach (Vector2Int position in floorPositions)
        {
            foreach (Vector2Int direction in directions)
            {
                Vector2Int neighbourPosition = position + direction;
                if (floorPositions.Contains(neighbourPosition) == false)
                    wallPositions.Add(neighbourPosition);
            }
        }
        return wallPositions;
    }
}

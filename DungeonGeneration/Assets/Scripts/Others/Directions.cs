using System.Collections.Generic;
using UnityEngine;

public static class Directions 
{
    /// <summary>
    /// Returns an array with this directions: (1,0), (-1,0), (0,1), (0,-1).
    /// </summary>
    /// <returns></returns>
    public static Vector2Int[] GetFourDirectionsArray()
    {
        Vector2Int[] directions = { new Vector2Int(0, 1), new Vector2Int(1, 0), new Vector2Int(0, -1), new Vector2Int(-1, 0) };

        return directions;
    }

    public static Vector2Int[] GetDiagonalsDirectionsArray()
    {
        Vector2Int[] directions = { new Vector2Int(1, 1), new Vector2Int(1, -1), new Vector2Int(-1, -1), new Vector2Int(-1, 1) };

        return directions;
    }

    /// <summary>
    /// Returns an array with this directions: (1,0), (-1,0), (0,1), (0,-1),
    /// (1,1),(-1,1),(1,-1),(-1,-1).
    /// </summary>
    /// <returns></returns>
    public static Vector2Int[] GetEightDiretionsArray()
    {
        Vector2Int[] directions = { 
            new Vector2Int(0, 1), 
            new Vector2Int(1, 1), 
            new Vector2Int(1, 0), 
            new Vector2Int(1, -1),
            new Vector2Int(0, -1), 
            new Vector2Int(-1, -1), 
            new Vector2Int(-1, 0), 
            new Vector2Int(-1, 1)};

       

        return directions;
    }

    /// <summary>
    /// Returns a random cardinal direction (up, down, right , left)
    /// </summary>
    /// <returns></returns>
    public static Vector2Int GetRandomFourDirection()
    {
        Vector2Int[] directions = { new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1) };

        return directions[Random.Range(0, directions.Length)];
    }

    /// <summary>
    /// Returns a random eight-way direction
    /// </summary>
    /// <returns></returns>
    public static Vector2Int GetRandomEightDirection()
    {
        Vector2Int[] directions = { new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1),
            new Vector2Int(1, 1), new Vector2Int(-1, 1), new Vector2Int(1, -1), new Vector2Int(-1, -1)};

        return directions[Random.Range(0, directions.Length)];
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Class for calculating the differents distances
public static class Distance
{
    public enum DistanceFormula { Euclidean, Manhattan, Chebyshev }

    /// <summary>
    /// Calculates the distance between two points with the distance algorithm chosen by the user
    /// </summary>
    /// <param name="from">Start point</param>
    /// <param name="to">End point</param>
    /// <param name="distanceAlgorithm">Distance algorithm</param>
    /// <returns>A float which is the distance between two points</returns>
    public static float CalculateDistance(Vector2Int from, Vector2Int to, DistanceFormula distanceAlgorithm)
    {
        float distance;
        switch (distanceAlgorithm)
        {
            case DistanceFormula.Euclidean:
                distance = EuclideanDistance(from, to);
                break;
            case DistanceFormula.Manhattan:
                distance = ManhattanDistance(from, to);
                break;
            case DistanceFormula.Chebyshev:
                distance = ChebyshevDistance(from, to);
                break;
            default:
                distance = EuclideanDistance(from, to);
                break;
        }

        return distance;
    }

    /// <summary>
    /// Calculates the Manhattan distance: Abs(x1 - x2) + Abs(y1 - y2)
    /// </summary>
    /// <param name="from">Start point</param>
    /// <param name="to">End point</param>
    /// <returns>A float which is the distance between two points</returns>
    private static float ManhattanDistance(Vector2Int from, Vector2Int to)
    {
        return Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y);
    }

    /// <summary>
    /// Calculates the Chebyshev distance: Max(Abs(x1 - x2), Abs(y1 - y2))
    /// </summary>
    /// <param name="from">Start point</param>
    /// <param name="to">End point</param>
    /// <returns>A float which is the distance between two points</returns>
    private static float ChebyshevDistance(Vector2Int from, Vector2Int to)
    {
        return Mathf.Max(Mathf.Abs(from.x - to.x), Mathf.Abs(from.y - to.y));
    }

    /// <summary>
    /// Calculates the Euclidean distance: Sqrt((x1 - x2)^2 + (y1 - y2)^2)
    /// </summary>
    /// <param name="from">Start point</param>
    /// <param name="to">End point</param>
    /// <returns>A float which is the distance between two points</returns>
    private static float EuclideanDistance(Vector2Int from, Vector2Int to)
    {
        return /*Math.Abs(*/(from - to).magnitude/*)*/;
    }
}

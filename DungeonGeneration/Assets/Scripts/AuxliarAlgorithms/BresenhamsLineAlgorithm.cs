using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Bresenhams algorithm for getting the line points
public static class BresenhamsLineAlgorithm
{
    /// <summary>
    /// Returns the points from a line which goes from (x0,y0) to (x1,y1). It uses Bresenhams line algorithm
    /// </summary>
    /// <param name="x0">X start position</param>
    /// <param name="y0">Y start position</param>
    /// <param name="x1">X end position</param>
    /// <param name="y1">Y end position</param>
    /// <returns>A list with all line points</returns>
    public static List<Vector2Int> GetLinePointsList(int x0, int y0, int x1, int y1)
    {
        List<Vector2Int> linePoints = new List<Vector2Int>();
        bool steep = Mathf.Abs(y1 - y0) > Mathf.Abs(x1 - x0);

        if (steep)
        {
            Swap<int>(ref x0, ref y0);
            Swap<int>(ref x1, ref y1);
        }

        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int error = (dx / 2);
        int ystep = (y0 < y1 ? 1 : -1);
        int xstep = (x0 < x1 ? 1 : -1);
        int y = y0;

        for (int x = x0; x != (x1 + xstep); x += xstep)
        {
            Vector2Int point = new Vector2Int((steep ? y : x), (steep ? x : y));
            linePoints.Add(point);

            error = error - dy;
            if (error < 0)
            {
                y += ystep;
                error += dx;
            }
        }

        return linePoints;
    }

    /// <summary>
    /// Swaps between two numbers
    /// </summary>
    /// <typeparam name="T">number type</typeparam>
    /// <param name="lhs">First number</param>
    /// <param name="rhs">Second number</param>
    private static void Swap<T>(ref T lhs, ref T rhs)
    {
        T temp;
        temp = lhs;
        lhs = rhs;
        rhs = temp;
    }
}

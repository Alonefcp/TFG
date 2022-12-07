using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HilbertCurve
{
    private int N;
    private int totalPoints;
    private int order;
    private HashSet<Vector2Int> hilbertCurvePoints;
    private HashSet<Vector2Int> hilbertCurvePointsInsideTheMap;

    public HilbertCurve(int order)
    {
        this.order = order;
        N = (int)Mathf.Pow(2, this.order);
        totalPoints = N * N;
        hilbertCurvePoints = new HashSet<Vector2Int>();
        hilbertCurvePointsInsideTheMap = new HashSet<Vector2Int>();
    }

    /// <summary>
    /// Getter and setter for getting all Hilbert curve points
    /// </summary>
    public HashSet<Vector2Int> HilbertCurvePoints { get => hilbertCurvePoints; set => hilbertCurvePoints = value; }

    /// <summary>
    /// Getter and setter for getting all Hilbert curve points which are inside the map
    /// </summary>
    public HashSet<Vector2Int> HilbertCurvePointsInsideTheMap { get => hilbertCurvePointsInsideTheMap; set => hilbertCurvePointsInsideTheMap = value; }

    /// <summary>
    /// Calculates Hilbert curve
    /// </summary>
    /// <param name="mapWidth">Map width</param>
    /// <param name="mapHeight">Map height</param>
    /// <param name="minOffsetX">Min X offset for the curve</param>
    /// <param name="maxOffsetX">Max X offset for the curve</param>
    /// <param name="minOffsetY">Min Y offset for the curve</param>
    /// <param name="maxOffsetY">Max Y offset for the curve</param>
    public void CalculateHilbertCurve(int mapWidth, int mapHeight, int minOffsetX, int maxOffsetX, int minOffsetY, int maxOffsetY)
    {
        List<Vector2Int> initialPoints = new List<Vector2Int>();
        hilbertCurvePoints = new HashSet<Vector2Int>();

        int posX = (mapWidth / N) + Random.Range(minOffsetX, maxOffsetX);
        int posY = (mapHeight / N) + Random.Range(minOffsetY, maxOffsetY); ;
        initialPoints.Add(HilbertPoint(0));
        initialPoints[0] = initialPoints[0] * new Vector2Int(posX, posY) + new Vector2Int(posX / 2, posY / 2);

        for (int i = 1; i < totalPoints; i++)
        {
            initialPoints.Add(HilbertPoint(i));
            initialPoints[i] = initialPoints[i] * new Vector2Int(posX, posY) + new Vector2Int(posX / 2, posY / 2);

            List<Vector2Int> extra = BresenhamsLineAlgorithm.GetLinePointsList(initialPoints[i - 1].x, initialPoints[i - 1].y, initialPoints[i].x, initialPoints[i].y);
            for (int j = 0; j < extra.Count; j++)
            {
                hilbertCurvePoints.Add(extra[j]);
            }
        }
    }

    /// <summary>
    /// Gets and calculate a Hilbert point for the curve
    /// </summary>
    /// <param name="point">Which point we want</param>
    /// <returns></returns>
    private Vector2Int HilbertPoint(int point)
    {
        Vector2Int[] points = {
            new Vector2Int(0, 0),
            new Vector2Int(0, 1),
            new Vector2Int(1, 1),
            new Vector2Int(1, 0)
        };


        int index = point & 3;
        Vector2Int v = points[index];

        for (int j = 1; j < order; j++)
        {
            point = point >> 2;
            index = point & 3;
            int len = (int)Mathf.Pow(2, j);
            if (index == 0)
            {
                int temp = v.x;
                v.x = v.y;
                v.y = temp;
            }
            else if (index == 1)
            {
                v.y += len;
            }
            else if (index == 2)
            {
                v.x += len;
                v.y += len;
            }
            else if (index == 3)
            {
                int temp = len - 1 - v.x;
                v.x = len - 1 - v.y;
                v.y = temp;
                v.x += len;
            }
        }
        return v;
    }
}

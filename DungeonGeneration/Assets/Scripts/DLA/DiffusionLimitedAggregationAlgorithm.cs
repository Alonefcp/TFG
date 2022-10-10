using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//Diffusion limited aggregation algorithm
public class DiffusionLimitedAggregationAlgorithm : MonoBehaviour
{
    enum Symmetry {Horizontal, Vertical, Both, None }

    [SerializeField] private TilemapVisualizer tilemapVisualizer;

    [SerializeField] private int mapWidht = 80, mapHeight = 40;
    [Range(0.0f,1.0f)]
    [SerializeField] private float fillPercentage = 0.5f;
    [SerializeField] Symmetry symmetryType = Symmetry.None;
    [SerializeField] private bool useCentralAttractor = false;
    [SerializeField] bool makeWiderDiagonals = false;
    [SerializeField] bool eliminateSingleWalls = false;

    private Vector2Int startPosition;
    private int totalTiles;
    private int maxFloorPositions;

    void Start()
    {
        totalTiles = mapWidht * mapHeight;
        maxFloorPositions = (int)(fillPercentage * totalTiles);

        HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();

        if (useCentralAttractor)
        {
            floorPositions = DiffusionLimitedAggregation_CentralAttractor();       
        }
        else
        {
            floorPositions = DiffusionLimitedAggregation();
        }

        tilemapVisualizer.ClearTilemap();

        if (symmetryType == Symmetry.Horizontal)
        {
            ApplyHorizontalSymmetry(floorPositions, mapWidht);
        }
        else if (symmetryType == Symmetry.Vertical)
        {
            ApplyVerticalSymmetry(floorPositions, mapHeight);
        }
        else if (symmetryType == Symmetry.Both)
        {
            ApplyHorizontalAndVerticalSymmetry(floorPositions, mapWidht, mapHeight);
        }
        else if (symmetryType == Symmetry.None)
        {
            tilemapVisualizer.PaintFloorTiles(floorPositions);
        }

        if (eliminateSingleWalls)
        {
            tilemapVisualizer.EliminateSingleWalls();
        }
    }

    private HashSet<Vector2Int> DiffusionLimitedAggregation()
    {
        Map map = new Map(mapWidht, mapHeight); //false -> wall , true -> floor
        HashSet<Vector2Int> positions = new HashSet<Vector2Int>();

        startPosition = new Vector2Int(mapWidht / 2, mapHeight / 2);

        //Dig a "seed" area around your central starting point
        CreateSeed(map, positions);

        //While the number of floor tiles is less than your desired total
        while (positions.Count < maxFloorPositions)
        {
            //Select a starting point at random for your digger
            int xPos = Random.Range(0, mapWidht);
            int yPos = Random.Range(0, mapHeight);
            Vector2Int diggerPos = new Vector2Int(xPos, yPos);
            Vector2Int diggerPrevPos = diggerPos;

            //Use the "drunkard's walk" algorithm to move randomly
            //If the digger hit a floor tile, then the previous tile they were in also becomes a floor and the digger stops
            while (!map.IsFloorPosition(diggerPos))
            {
                diggerPrevPos = diggerPos;

                int num = Random.Range(1, 5);

                if (num == 1)
                {
                    if (diggerPos.x > 2) diggerPos.x -= 1;
                }
                else if (num == 2)
                {
                    if (diggerPos.x < mapWidht - 2) { diggerPos.x += 1; }
                }
                else if (num == 3)
                {
                    if (diggerPos.y > 2) { diggerPos.y -= 1; }
                }
                else if (num == 4)
                {
                    if (diggerPos.y < mapHeight - 2) { diggerPos.y += 1; }
                }
            }

            map.SetHasFloor(diggerPrevPos, true);
            positions.Add(diggerPrevPos);
        }

        return positions;
    }

    private HashSet<Vector2Int> DiffusionLimitedAggregation_CentralAttractor()
    {
        Map map = new Map(mapWidht, mapHeight);
        HashSet<Vector2Int> positions = new HashSet<Vector2Int>();

        startPosition = new Vector2Int(mapWidht / 2, mapHeight / 2);

        //Dig a "seed" area around your central starting point
        CreateSeed(map, positions);

        //While the number of floor tiles is less than your desired total
        while (positions.Count < maxFloorPositions)
        {
            //Select a starting point at random for your digger
            int xPos = Random.Range(0, mapWidht);
            int yPos = Random.Range(0, mapHeight);
            Vector2Int diggerPos = new Vector2Int(xPos, yPos);
            Vector2Int diggerPrevPos = diggerPos;

            //line path
            List<Vector2Int> path = GetLinePointsList(diggerPos.x, diggerPos.y, startPosition.x, startPosition.y,out int xStep,out int yStep).ToList();

            while (!map.IsFloorPosition(diggerPos) && path.Count > 0)
            {
                diggerPrevPos = diggerPos;
                diggerPos.x = path[0].x;
                diggerPos.y = path[0].y;
                path.RemoveAt(0);
            }

            if(makeWiderDiagonals) MakeWiderDiagonals(diggerPrevPos, xStep, yStep, map, positions);

            map.SetHasFloor(diggerPrevPos, true);
            positions.Add(diggerPrevPos);
        }

        return positions;
    }

    private void CreateSeed(Map map, HashSet<Vector2Int> positions)
    {
        map.SetHasFloor(startPosition, true);
        map.SetHasFloor(startPosition + new Vector2Int(1, 0), true);
        map.SetHasFloor(startPosition + new Vector2Int(-1, 0), true);
        map.SetHasFloor(startPosition + new Vector2Int(0, 1), true);
        map.SetHasFloor(startPosition + new Vector2Int(0, -1), true);

        positions.Add(startPosition);
        positions.Add(startPosition + new Vector2Int(1, 0));
        positions.Add(startPosition + new Vector2Int(-1, 0));
        positions.Add(startPosition + new Vector2Int(0, 1));
        positions.Add(startPosition + new Vector2Int(0, -1));
    }

    private void Swap<T>(ref T lhs, ref T rhs)
    {
        T temp;
        temp = lhs;
        lhs = rhs;
        rhs = temp;
    }

    //Bresenhams line algorithm
    //private IEnumerable<Point> GetLinePoints(int x0, int y0, int x1, int y1)
    //{
    //    bool steep = Mathf.Abs(y1 - y0) > Mathf.Abs(x1 - x0);

    //    if (steep)
    //    {
    //        Swap<int>(ref x0, ref y0);
    //        Swap<int>(ref x1, ref y1);
    //    }

    //    int dx = Mathf.Abs(x1 - x0);
    //    int dy = Mathf.Abs(y1 - y0);
    //    int error = (dx / 2);
    //    int ystep = (y0 < y1 ? 1 : -1);
    //    int xstep = (x0 < x1 ? 1 : -1);
    //    int y = y0;

    //    for (int x = x0; x != (x1 + xstep); x += xstep)
    //    {
    //        yield return new Point((steep ? y : x), (steep ? x : y));
    //        error = error - dy;
    //        if (error < 0)
    //        {
    //            y += ystep;
    //            error += dx;
    //        }
    //    }
    //    yield break;
    //}

    //Bresenhams line algorithm
    private List<Vector2Int> GetLinePointsList(int x0, int y0, int x1, int y1, out int xstep, out int ystep)
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
        ystep = (y0 < y1 ? 1 : -1);
        xstep = (x0 < x1 ? 1 : -1);
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

    private void MakeWiderDiagonals(Vector2Int diggerPrevPos, int xStep, int yStep, Map map, HashSet<Vector2Int> positions)
    {
        if (xStep < 0 && yStep < 0)
        {
            Vector2Int pos1 = diggerPrevPos + new Vector2Int(-1, 0);
            Vector2Int pos2 = diggerPrevPos + new Vector2Int(0, -1);

            if (map.IsInsideTheMap(pos1))
            {
                map.SetHasFloor(pos1, true);
                positions.Add(pos1);
            }
            else if (map.IsInsideTheMap(pos2))
            {
                map.SetHasFloor(pos2, true);
                positions.Add(pos2);
            }

        }
        else if (xStep > 0 && yStep < 0)
        {
            Vector2Int pos1 = diggerPrevPos + new Vector2Int(1, 0);
            Vector2Int pos2 = diggerPrevPos + new Vector2Int(0, -1);

            if (map.IsInsideTheMap(pos1))
            {
                map.SetHasFloor(pos1, true);
                positions.Add(pos1);
            }
            else if (map.IsInsideTheMap(pos2))
            {
                map.SetHasFloor(pos2, true);
                positions.Add(pos2);
            }
        }
        else if (xStep < 0 && yStep > 0)
        {
            Vector2Int pos1 = diggerPrevPos + new Vector2Int(-1, 0);
            Vector2Int pos2 = diggerPrevPos + new Vector2Int(0, 1);

            if (map.IsInsideTheMap(pos1))
            {
                map.SetHasFloor(pos1, true);
                positions.Add(pos1);
            }
            else if (map.IsInsideTheMap(pos2))
            {
                map.SetHasFloor(pos2, true);
                positions.Add(pos2);
            }
        }
        else if (xStep > 0 && yStep > 0)
        {
            Vector2Int pos1 = diggerPrevPos + new Vector2Int(1, 0);
            Vector2Int pos2 = diggerPrevPos + new Vector2Int(0, 1);

            if (map.IsInsideTheMap(pos1))
            {
                map.SetHasFloor(pos1, true);
                positions.Add(pos1);
            }
            else if (map.IsInsideTheMap(pos2))
            {
                map.SetHasFloor(pos2, true);
                positions.Add(pos2);
            }
        }
    }

    private void ApplyHorizontalSymmetry(HashSet<Vector2Int> positions, int mapWidht)
    {
        int centerX = mapWidht / 2;

        foreach (Vector2Int pos in positions)
        {
            if (pos.x == centerX)
            {
                tilemapVisualizer.PaintSingleTile(pos);
            }
            else
            {
                int distX = Mathf.Abs(centerX - pos.x);
                tilemapVisualizer.PaintSingleTile(new Vector2Int(centerX + distX, pos.y));
                tilemapVisualizer.PaintSingleTile(new Vector2Int(centerX - distX, pos.y));
            }
        }    
    }

    private void ApplyVerticalSymmetry(HashSet<Vector2Int> positions, int mapHeight)
    {
        int centerY = mapHeight / 2;

        foreach (Vector2Int pos in positions)
        {
            if (pos.y == centerY)
            {
                tilemapVisualizer.PaintSingleTile(pos);
            }
            else
            {
                int distY = Mathf.Abs(centerY - pos.y);
                tilemapVisualizer.PaintSingleTile(new Vector2Int(pos.x, centerY + distY));
                tilemapVisualizer.PaintSingleTile(new Vector2Int(pos.x, centerY - distY));
            }
        }  
    }

    private void ApplyHorizontalAndVerticalSymmetry(HashSet<Vector2Int> positions, int mapWidht, int mapHeight)
    {
        int centerX = mapWidht / 2;
        int centerY = mapHeight / 2;

        foreach (Vector2Int pos in positions)
        {
            if (pos.x == centerX && pos.y == centerY)
            {
                tilemapVisualizer.PaintSingleTile(pos);
            }
            else
            {
                int distX = Mathf.Abs(centerX - pos.x);
                tilemapVisualizer.PaintSingleTile(new Vector2Int(centerX + distX, pos.y));
                tilemapVisualizer.PaintSingleTile(new Vector2Int(centerX - distX, pos.y));

                int distY = Mathf.Abs(centerY - pos.y);
                tilemapVisualizer.PaintSingleTile(new Vector2Int(pos.x, centerY + distY));
                tilemapVisualizer.PaintSingleTile(new Vector2Int(pos.x, centerY - distY));
            }
        }
    }
}

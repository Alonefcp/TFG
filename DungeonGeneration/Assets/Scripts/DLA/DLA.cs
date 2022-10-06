using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;

//Diffusion limited aggregation algorithm
public class DLA : MonoBehaviour
{

    [SerializeField] private TilemapVisualizer tilemapVisualizer;

    [SerializeField] private int maxFloorPositions = 300;
    [SerializeField] private int mapWidht = 80, mapHeight = 40;
    [SerializeField] private bool useCentralAttractor = false;
    private Vector2Int startPosition;

    void Start()
    {
        if(useCentralAttractor)
        {
            bool[,] map = DiffusionLimitedAggregation_CentralAttractor();
            tilemapVisualizer.ClearTilemap();
            tilemapVisualizer.PaintFloorTiles(map, mapWidht, mapHeight);
        }
        else
        {
            bool[,] map = DiffusionLimitedAggregation();
            tilemapVisualizer.ClearTilemap();
            tilemapVisualizer.PaintFloorTiles(map, mapWidht, mapHeight);
        }
    }

    private bool[,] DiffusionLimitedAggregation()
    {
        bool[,] map = new bool[mapHeight, mapWidht]; //false -> wall , true -> floor

        //Dig a "seed" area around your central starting point.
        startPosition = new Vector2Int(mapWidht / 2, mapHeight / 2);
        map[startPosition.y, startPosition.x] = true;
        map[startPosition.y, startPosition.x + 1] = true;
        map[startPosition.y, startPosition.x - 1] = true;
        map[startPosition.y + 1, startPosition.x] = true;
        map[startPosition.y - 1, startPosition.x] = true;


        //While the number of floor tiles is less than your desired total
        while (CountNumberFloors(map) < maxFloorPositions)
        {
            //Select a starting point at random for your digger
            int xPos = Random.Range(0, mapWidht);
            int yPos = Random.Range(0, mapHeight);
            Vector2Int diggerPos = new Vector2Int(xPos, yPos);
            Vector2Int diggerPrevPos = diggerPos;

            //Use the "drunkard's walk" algorithm to move randomly
            //If the digger hit a floor tile, then the previous tile they were in also becomes a floor and the digger stops
            while (!map[diggerPos.y, diggerPos.x])
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

            map[diggerPrevPos.y, diggerPrevPos.x] = true;
        }

        return map;
    }

    private bool[,] DiffusionLimitedAggregation_CentralAttractor()
    {
        bool[,] map = new bool[mapHeight, mapWidht]; //false -> wall , true -> floor

        //Dig a "seed" area around your central starting point.
        startPosition = new Vector2Int(mapWidht / 2, mapHeight / 2);
        map[startPosition.y, startPosition.x] = true;
        map[startPosition.y, startPosition.x + 1] = true;
        map[startPosition.y, startPosition.x - 1] = true;
        map[startPosition.y + 1, startPosition.x] = true;
        map[startPosition.y - 1, startPosition.x] = true;

        //While the number of floor tiles is less than your desired total
        while (CountNumberFloors(map) < maxFloorPositions)
        {
            //Select a starting point at random for your digger
            int xPos = Random.Range(0, mapWidht);
            int yPos = Random.Range(0, mapHeight);
            Vector2Int diggerPos = new Vector2Int(xPos, yPos);
            Vector2Int diggerPrevPos = diggerPos;

            //line path
            List<Point> path = GetLinePoints(diggerPos.x, diggerPos.y, startPosition.x, startPosition.y).ToList();

            while (!map[diggerPos.y, diggerPos.x] && path.Count > 0)
            {
                diggerPrevPos = diggerPos;
                diggerPos.x = path[0].X;
                diggerPos.y = path[0].Y;
                path.RemoveAt(0);
            }

            map[diggerPrevPos.y, diggerPrevPos.x] = true;
        }

        return map;
    }

    private void Swap<T>(ref T lhs, ref T rhs)
    {
        T temp;
        temp = lhs;
        lhs = rhs;
        rhs = temp;
    }

    //Bresenhams line algorithm
    private IEnumerable<Point> GetLinePoints(int x0, int y0, int x1, int y1)
    {
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
            yield return new Point((steep ? y : x), (steep ? x : y));
            error = error - dy;
            if (error < 0)
            {
                y += ystep;
                error += dx;
            }
        }
        yield break;
    }

    private int CountNumberFloors(bool[,] map)
    {
        int nFloors = 0;

        for (int i = 0; i < map.GetLength(0); i++)
        {
            for (int j = 0; j < map.GetLength(1); j++)
            {
                if (map[i, j]) nFloors++;
            }
        }

        return nFloors;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Diffusion limited aggregation algorithm
public class DLA : MonoBehaviour
{

    [SerializeField] private TilemapVisualizer tilemapVisualizer;

    [SerializeField] private int maxFloorPositions = 300;
    [SerializeField] private int mapWidht = 80, mapHeight = 40;
    private Vector2Int startPosition;

    void Start()
    {
        bool[,] map = DiffusionLimitedAggregation();
        tilemapVisualizer.ClearTilemap();
        tilemapVisualizer.PaintFloorTiles(map, mapWidht, mapHeight);
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

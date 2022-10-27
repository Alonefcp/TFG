using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellularAutomataAlgorithm : DungeonGenerator
{
    [SerializeField]
    private int mapWidth = 50, mapHeight = 50;

    [Range(0.0f,1.0f)]
    [SerializeField]
    private float noisePercent = 0.55f;
    [SerializeField]
    private int iterations = 1;

    //void Start()
    //{
    //    GenerateDungeon();
    //}

    public override void GenerateDungeon()
    {
        tilemapVisualizer.ClearTilemap();
        List<bool> mapInfo = GenerateNoise();
        CellularAutomata(mapInfo);
    }
 
    private List<bool> GenerateNoise()
    {
        List<bool> noise = new List<bool>(); // true: floor , false: wall
        for (int i = 0; i < mapWidth * mapHeight; i++) 
        { 
            noise.Add(false); 
        }

        for (int i = 0; i < noise.Count; i++)
        {
            int x = i % mapWidth;
            int y = i / mapWidth;

            float rnd = Random.Range(0.0f, 1.0f);           
           
            if(rnd > noisePercent)
            {
                noise[i] = true;
                tilemapVisualizer.PaintSingleFloorTile(new Vector2Int(x, y));
            }
            else
            {
                tilemapVisualizer.PaintSingleCorridorTile(new Vector2Int(x, y));
            }
            
        }

        return noise;
    }

    private void CellularAutomata(List<bool> noise)
    {
        for (int i = 0; i < iterations; i++)
        {
            List<bool> map = new List<bool>(noise);

            for (int x = 1; x < mapWidth-1; x++)
            {
                for (int y = 1; y < mapHeight-1; y++)
                {
                    int index = MapXYtoIndex(x, y);

                    int neighbors = 0;

                    if (!map[index - 1]) { neighbors += 1; }
                    if (!map[index + 1]) { neighbors += 1; }
                    if (!map[index - mapWidth]) { neighbors += 1; }
                    if (!map[index + mapWidth]) { neighbors += 1; }
                    if (!map[index - (mapWidth - 1)]) { neighbors += 1; }
                    if (!map[index - (mapWidth + 1)]) { neighbors += 1; }
                    if (!map[index + (mapWidth - 1)]) { neighbors += 1; }
                    if (!map[index + (mapWidth + 1)]) { neighbors += 1; }

                    if (neighbors > 4 || neighbors==0)
                    {
                        tilemapVisualizer.PaintSingleCorridorTile(new Vector2Int(x, y));
                    }
                    else
                    {
                        tilemapVisualizer.PaintSingleFloorTile(new Vector2Int(x, y));
                    }
                }
            }
        }
    }

    /// <summary>
    /// Conerts a map position to an index.
    /// </summary>
    /// <param name="x">X map position</param>
    /// <param name="y">Y map position</param>
    /// <returns></returns>
    private int MapXYtoIndex(int x, int y)
    {
        return x + (y * mapWidth);
    }

}

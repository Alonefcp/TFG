using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinNoiseAlgorithm : DungeonGenerator
{
    [Range(10, 200)]
    [SerializeField] private int mapWidth = 10, mapHeight = 10;
    [SerializeField] private float noiseScale = 0.3f;
    [Range(0,8)]
    [SerializeField] private int octaves = 4;
    [Range(0.0f,1.0f)]
    [SerializeField] private float persistance = 0.5f;
    [Range(1,10)]
    [SerializeField] private float lacunarity = 2.0f;
    [Range(0.0f, 1.0f)]
    [SerializeField] private float threshold = 0.5f;
    [SerializeField] private int seed;
    [SerializeField] private Vector2 offset;
    [SerializeField] private Renderer textureRenderer;


    //void Start()
    //{
    //    GenerateDungeon();
    //}


    public override void GenerateDungeon()
    {
        tilemapVisualizer.ClearTilemap();

        float[,] noiseTexture = GenerateTextureWithPerlinNoise(mapWidth, mapHeight, seed, noiseScale, octaves, persistance, lacunarity, this.offset);

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                if(x==0 || y==0 || x == mapWidth-1 || y== mapHeight-1)
                {
                    tilemapVisualizer.PaintSingleWallTile(new Vector2Int(x, y));
                    continue;
                }

                if (noiseTexture[x, y] < threshold)
                {
                    tilemapVisualizer.PaintSingleFloorTile(new Vector2Int(x, y));
                }
                else
                {
                    tilemapVisualizer.PaintSingleWallTile(new Vector2Int(x, y));
                }
                //tilemapVisualizer.PaintSingleFloorTileWithColor(new Vector2Int(x, y), Color.Lerp(Color.black, Color.white, noiseTexture[x, y]));
            }
        }

        int interval = 2;
        float offset = 7.0f;
        AddBottomBorders(noiseTexture,offset,interval);
        AddUpBorders(noiseTexture, offset, interval);
        AddLeftBorders(noiseTexture, offset, interval);
        AddRightBorders(noiseTexture, offset, interval);

        //AddBorders(noiseTexture);       
    }

    private void AddUpBorders(float[,] noiseTexture, float offset, int interval)
    {
        //Smooth the noise and store it in the int array
        if (interval > 1)
        {
            int newPoint, points;
            //Used to reduced the position of the Perlin point
            float reduction = 0.5f;

            //Used in the smoothing process
            Vector2Int currentPos, lastPos;
            //The corresponding points of the smoothing. One list for x and one for y
            List<int> noiseX = new List<int>();
            List<int> noiseY = new List<int>();

            //Generate the noise
            for (int x = 0; x <= mapWidth; x += interval)
            {
                newPoint = Mathf.FloorToInt((Mathf.PerlinNoise(x, Random.Range(0.0f, 100000.0f) * reduction)) * offset);
                noiseY.Add(newPoint);
                noiseX.Add(x);
            }

            points = noiseY.Count;
            //Start at 1 so we have a previous position already
            for (int i = 1; i < points; i++)
            {
                //Get the current position
                currentPos = new Vector2Int(noiseX[i], noiseY[i]);
                //Also get the last position
                lastPos = new Vector2Int(noiseX[i - 1], noiseY[i - 1]);

                //Find the difference between the two
                Vector2 diff = currentPos - lastPos;

                //Set up what the height change value will be
                float heightChange = diff.y / interval;
                //Determine the current height
                float currHeight = lastPos.y;

                //Work our way through from the last x to the current x
                for (int x = lastPos.x; x < currentPos.x; x++)
                {
                    for (int y =mapHeight - Mathf.FloorToInt(currHeight); y < mapHeight; y++)
                    {
                        noiseTexture[x, y] = 1;
                        tilemapVisualizer.PaintSingleWallTile(new Vector2Int(x, y));
                    }
                    currHeight += heightChange;
                }
            }
        }
    }
    private void AddBottomBorders(float[,] noiseTexture, float offset, int interval)
    {
        //Smooth the noise and store it in the int array
        if (interval > 1)
        {
            int newPoint, points;
            //Used to reduced the position of the Perlin point
            float reduction = 0.5f;

            //Used in the smoothing process
            Vector2Int currentPos, lastPos;
            //The corresponding points of the smoothing. One list for x and one for y
            List<int> noiseX = new List<int>();
            List<int> noiseY = new List<int>();

            //Generate the noise
            for (int x = 0; x <= mapWidth; x += interval)
            {
                newPoint = Mathf.FloorToInt((Mathf.PerlinNoise(x, Random.Range(0.0f, 100000.0f) * reduction)) * offset);
                noiseY.Add(newPoint);
                noiseX.Add(x);
            }

            points = noiseY.Count;
            //Start at 1 so we have a previous position already
            for (int i = 1; i < points; i++)
            {
                //Get the current position
                currentPos = new Vector2Int(noiseX[i], noiseY[i]);
                //Also get the last position
                lastPos = new Vector2Int(noiseX[i - 1], noiseY[i - 1]);

                //Find the difference between the two
                Vector2 diff = currentPos - lastPos;

                //Set up what the height change value will be
                float heightChange = diff.y / interval;
                //Determine the current height
                float currHeight = lastPos.y;

                //Work our way through from the last x to the current x
                for (int x = lastPos.x; x < currentPos.x; x++)
                {
                    for (int y = Mathf.FloorToInt(currHeight); y > 0; y--)
                    {
                        noiseTexture[x, y] = 1;
                        tilemapVisualizer.PaintSingleWallTile(new Vector2Int(x, y));
                    }
                    currHeight += heightChange;
                }
            }
        }
    }
    private void AddLeftBorders(float[,] noiseTexture, float offset, int interval)
    {
        //Smooth the noise and store it in the int array
        if (interval > 1)
        {
            int newPoint, points;
            //Used to reduced the position of the Perlin point
            float reduction = 0.5f;

            //Used in the smoothing process
            Vector2Int currentPos, lastPos;
            //The corresponding points of the smoothing. One list for x and one for y
            List<int> noiseX = new List<int>();
            List<int> noiseY = new List<int>();

            //Generate the noise
            for (int y = 0; y <= mapHeight; y += interval)
            {
                newPoint = Mathf.FloorToInt((Mathf.PerlinNoise(Random.Range(0.0f, 100000.0f) * reduction, y)) * offset);
                noiseY.Add(y);
                noiseX.Add(newPoint);
            }

            points = noiseY.Count;
            //Start at 1 so we have a previous position already
            for (int i = 1; i < points; i++)
            {
                //Get the current position
                currentPos = new Vector2Int(noiseX[i], noiseY[i]);
                //Also get the last position
                lastPos = new Vector2Int(noiseX[i - 1], noiseY[i - 1]);

                //Find the difference between the two
                Vector2 diff = currentPos - lastPos;

                //Set up what the height change value will be
                float heightChange = diff.x / interval;
                //Determine the current height
                float currHeight = lastPos.x;

                //Work our way through from the last x to the current x
                for (int y = lastPos.y; y < currentPos.y; y++)
                {
                    for (int x = Mathf.FloorToInt(currHeight); x > 0; x--)
                    {
                        noiseTexture[x, y] = 1;
                        tilemapVisualizer.PaintSingleWallTile(new Vector2Int(x, y));
                    }
                    currHeight += heightChange;
                }
            }
        }
    }
    private void AddRightBorders(float[,] noiseTexture, float offset, int interval)
    {
        //Smooth the noise and store it in the int array
        if (interval > 1)
        {
            int newPoint, points;
            //Used to reduced the position of the Perlin point
            float reduction = 0.5f;

            //Used in the smoothing process
            Vector2Int currentPos, lastPos;
            //The corresponding points of the smoothing. One list for x and one for y
            List<int> noiseX = new List<int>();
            List<int> noiseY = new List<int>();

            //Generate the noise
            for (int y = 0; y <= mapHeight; y += interval)
            {
                newPoint = Mathf.FloorToInt((Mathf.PerlinNoise(Random.Range(0.0f, 100000.0f) * reduction, y)) * offset);
                noiseY.Add(y);
                noiseX.Add(newPoint);
            }

            points = noiseY.Count;
            //Start at 1 so we have a previous position already
            for (int i = 1; i < points; i++)
            {
                //Get the current position
                currentPos = new Vector2Int(noiseX[i], noiseY[i]);
                //Also get the last position
                lastPos = new Vector2Int(noiseX[i - 1], noiseY[i - 1]);

                //Find the difference between the two
                Vector2 diff = currentPos - lastPos;

                //Set up what the height change value will be
                float heightChange = diff.x / interval;
                //Determine the current height
                float currHeight = lastPos.x;

                //Work our way through from the last x to the current x
                for (int y = lastPos.y; y < currentPos.y; y++)
                {
                    for (int x = mapWidth - Mathf.FloorToInt(currHeight); x < mapWidth; x++)
                    {
                        noiseTexture[x, y] = 1;
                        tilemapVisualizer.PaintSingleWallTile(new Vector2Int(x, y));
                    }
                    currHeight += heightChange;
                }
            }
        }
    }

    private void AddBorders(float[,] noiseTexture)
    {
        int newPoint;
        float reduction = 0.5f;
        int offset = 7;

        //bottom
        for (int x = 0; x < mapWidth; x++)
        {
            newPoint = Mathf.FloorToInt((Mathf.PerlinNoise(x, Random.Range(0.0f, 100000.0f)) - reduction) * offset);

            newPoint += (offset / 2);
            for (int y = newPoint; y >= 0; y--)
            {
                tilemapVisualizer.PaintSingleWallTile(new Vector2Int(x, y));
                noiseTexture[x, y] = 1;
            }
        }

        //up
        for (int x = 0; x < mapWidth; x++)
        {
            newPoint = Mathf.FloorToInt((Mathf.PerlinNoise(x, Random.Range(0.0f, 100000.0f)) - reduction) * offset);
            newPoint += (offset / 2);

            if (newPoint == 0)
            {
                tilemapVisualizer.PaintSingleWallTile(new Vector2Int(x, mapHeight - 1));
                noiseTexture[x, mapHeight - 1] = 1;
                continue;
            }

            for (int y = mapHeight - newPoint; y < mapHeight; y++)
            {
                tilemapVisualizer.PaintSingleWallTile(new Vector2Int(x, y));
                noiseTexture[x, y] = 1;
            }
        }

        //left
        for (int y = 0; y < mapHeight; y++)
        {
            newPoint = Mathf.FloorToInt((Mathf.PerlinNoise(Random.Range(0.0f, 100000.0f), y) - reduction) * offset);

            newPoint += (offset / 2);
            for (int x = newPoint; x >= 0; x--)
            {
                tilemapVisualizer.PaintSingleWallTile(new Vector2Int(x, y));
                noiseTexture[x, y] = 1;
            }
        }

        //right
        for (int y = 0; y < mapHeight; y++)
        {
            newPoint = Mathf.FloorToInt((Mathf.PerlinNoise(Random.Range(0.0f, 100000.0f), y) - reduction) * offset);
            newPoint += (offset / 2);

            if (newPoint == 0)
            {
                tilemapVisualizer.PaintSingleWallTile(new Vector2Int(mapWidth-1,y));
                noiseTexture[mapWidth-1, y] = 1;
                continue;
            }

            for (int x = mapWidth - newPoint; x < mapWidth; x++)
            {
                tilemapVisualizer.PaintSingleWallTile(new Vector2Int(x, y));
                noiseTexture[x, y] = 1;
            }
        }
    }

    private float[,] GenerateTextureWithPerlinNoise(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset)
    {
        float[,] noiseTextue = new float[mapWidth, mapHeight];

        System.Random rng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++)
        {
            float offsetX = rng.Next(-100000, 100000)+offset.x;
            float offsetY = rng.Next(-100000, 100000)+offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        if(scale<=0)scale = 0.0001f;

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        float halfWidth = mapWidth / 2;
        float halfHeight = mapHeight / 2;

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {

                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x-halfWidth) / scale*frequency + octaveOffsets[i].x;
                    float sampleY = (y-halfHeight) / scale*frequency + octaveOffsets[i].y;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY)*2-1;
                    noiseHeight += perlinValue * amplitude;
                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                if(noiseHeight>maxNoiseHeight)
                {
                    maxNoiseHeight = noiseHeight;
                }
                else if(noiseHeight<minNoiseHeight)
                {
                    minNoiseHeight = noiseHeight;
                }

                noiseTextue[x, y] = noiseHeight;
            }
        }

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                noiseTextue[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseTextue[x, y]);
            }
        }

        return noiseTextue;
    }

    private void DrawPerlinNoiseTexture(float[,] noiseTexture)
    {
        Texture2D texture2D = new Texture2D(mapWidth, mapHeight);
        Color32[] colourMap = new Color32[mapWidth * mapHeight];

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                colourMap[y * mapWidth + x] = Color32.Lerp(Color.black, Color.white, noiseTexture[x, y]);
            }
        }

        texture2D.SetPixels32(colourMap,0);
        texture2D.Apply();
        textureRenderer.sharedMaterial.mainTexture = texture2D;
        textureRenderer.transform.localScale = new Vector3(mapWidth, 1.0f, mapHeight);
    }
}

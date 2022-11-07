using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinNoiseAlgorithm : DungeonGenerator
{
    [Range(10, 200)]
    [SerializeField] private int mapWidth = 10, mapHeight = 10;
    [SerializeField] private float noiseScale = 0.3f;
    [Range(0,10)]
    [SerializeField] private int octaves = 4;
    [Range(0.0f,1.0f)]
    [SerializeField] private float persistance = 0.5f;
    [Range(1,20)]
    [SerializeField] private float lacunarity = 2.0f;
    [SerializeField] private int seed;
    [SerializeField] private Vector2 offset;
    [SerializeField] private Renderer textureRenderer;


    //void Start()
    //{
    //    GenerateDungeon();
    //}


    public override void GenerateDungeon()
    {
        float[,] noiseTexture = GenerateTextureWithPerlinNoise(mapWidth, mapHeight, seed, noiseScale, octaves, persistance, lacunarity, offset);
        DrawPerlinNoiseTexture(noiseTexture);
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

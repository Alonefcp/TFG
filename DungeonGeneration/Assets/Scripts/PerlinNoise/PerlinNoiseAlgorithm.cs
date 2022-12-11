using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinNoiseAlgorithm : DungeonGeneration
{
    [Range(10, 200)]
    [SerializeField] private int mapWidth = 10, mapHeight = 10;
    [SerializeField] private float noiseScale = 0.3f;
    [Range(1,8)]
    [SerializeField] private int octaves = 4;
    [Range(0.0f,1.0f)]
    [SerializeField] private float persistance = 0.5f;
    [Range(1,10)]
    [SerializeField] private float lacunarity = 2.0f;
    [Range(0.0f, 1.0f)]
    [SerializeField] private float fillPercent = 0.5f;
    [SerializeField] private Vector2 offset;
    [Range(0, 50)]
    [SerializeField] private int wallThresholdSize=10;
    [Range(0, 50)]
    [SerializeField] private int floorThresholdSize=10;
    [SerializeField] private bool connectRegions = true;
    [Range(1, 3)]
    [SerializeField] private int connectionSize = 1;
    [Range(2, 8)]
    [SerializeField] int borderInterval = 2;
    [Range(6, 10)]
    [SerializeField] int borderOffset = 7;
    [SerializeField] private bool addBiggerBorders = true;
    [SerializeField] private bool showPerlinNoiseTexture = false;

    int[,] map;
    //void Start()
    //{
    //    GenerateDungeon();
    //}

    public override void GenerateDungeon()
    {
        base.GenerateDungeon();

        float[,] perlinNoise = GenerateMatrixWithPerlinNoise(mapWidth, mapHeight, noiseScale, octaves, persistance, lacunarity, offset);
        CreateMap(perlinNoise);

        if (addBiggerBorders && !showPerlinNoiseTexture)
        {
            AddBottomUpSmoothBorders(borderOffset, borderInterval);
            AddLeftRightSmoothBorders(borderOffset, borderInterval);
        }

        if (!showPerlinNoiseTexture)
        {
            EraseRegions();
            AddBorders();
        }

        //Draw map
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                if (map[x, y]==0) //cell becomes a floor
                {
                    tilemapVisualizer.PaintSingleFloorTile(new Vector2Int(x, y));
                }
                else //cell becomes a wall
                {
                    tilemapVisualizer.PaintSingleWallTile(new Vector2Int(x, y));
                }
            }
        }

        //Sets player position
        Vector2Int playerPosition = new Vector2Int(Random.Range(1, mapWidth - 1), Random.Range(1, mapHeight - 1));

        while (map[playerPosition.x, playerPosition.y] == 1)
        {
            playerPosition = new Vector2Int(Random.Range(1, mapWidth - 1), Random.Range(1, mapHeight - 1));
        }
        playerController.SetPlayer(playerPosition, new Vector3(0.3f, 0.3f, 0.3f));
    }

    /// <summary>
    /// Creates a matrix using perlin noise
    /// </summary>
    /// <param name="mapWidth">Map width</param>
    /// <param name="mapHeight">Map height</param>
    /// <param name="scale">Map scale</param>
    /// <param name="octaves">Number of layers of detail</param>
    /// <param name="persistance">Determines how quickly the amplitudes diminish for each successive octave</param>
    /// <param name="lacunarity">Change in frequency between octaves</param>
    /// <param name="offset">Map displacement offset</param>
    /// <returns>A float matrix with values between 0 an 1</returns>
    private float[,] GenerateMatrixWithPerlinNoise(int mapWidth, int mapHeight, float scale, int octaves, float persistance, float lacunarity, Vector2 offset)
    {
        float[,] noise = new float[mapWidth, mapHeight];
       
        Vector2[] octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++)
        {
            float offsetX = Random.Range(-100000, 100001)+offset.x;
            float offsetY = Random.Range(-100000, 100001)+offset.y;
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

                noise[x, y] = noiseHeight;               
            }
        }

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                noise[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noise[x, y]);
            }
        }

        return noise;
    }

    /// <summary>
    /// Creates the map by adding wall tiles or floor tiles
    /// </summary>   
    private void CreateMap(float[,] perlinNoise)
    {
        map = new int[mapWidth, mapHeight];

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                if(showPerlinNoiseTexture)
                {
                    tilemapVisualizer.PaintSingleFloorTileWithColor(new Vector2Int(x, y), Color.Lerp(Color.black, Color.white, perlinNoise[x, y]));
                    continue;
                }

                if (x == 0 || y == 0 || x == mapWidth - 1 || y == mapHeight - 1)
                {                    
                    map[x, y] = 1;
                    continue;
                }

                if (perlinNoise[x, y] <= fillPercent) //cell becomes a floor
                {
                    map[x, y] = 0;
                }
                else //cell becomes a wall
                {
                    map[x, y] = 1;
                }               
            }
        }
    }

    /// <summary>
    /// Adds walls to the map limits
    /// </summary>
    private void AddBorders()
    {
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                if (x == 0 || y == 0 || x == mapWidth - 1 || y == mapHeight - 1)
                {
                    map[x, y] = 1;
                }
            }
        }
    }

    /// <summary>
    /// Adds the bottom and up borders to the map using perlin noise
    /// </summary>
    /// <param name="offset">Borders max height</param>
    /// <param name="interval">Borders length</param>
    private void AddBottomUpSmoothBorders(float offset, int interval)
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
                newPoint = Mathf.FloorToInt(Mathf.PerlinNoise(x, Random.Range(0, 100001) * reduction) * offset);
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
                for (int x = lastPos.x; x < currentPos.x; x++) //Bottom
                {
                    for (int y = Mathf.FloorToInt(currHeight); y > 0; y--)
                    {
                        map[x, y] = 1;
                    }
                    currHeight += heightChange;
                }

                for (int x = lastPos.x; x < currentPos.x; x++) //up
                {
                    for (int y = mapHeight - Mathf.FloorToInt(currHeight); y < mapHeight; y++)
                    {
                        map[x, y] = 1;
                    }
                    currHeight += heightChange;
                }
            }
        }
    }

    /// <summary>
    /// Adds the left and right borders to the map using perlin noise
    /// </summary>
    /// <param name="offset">Borders max height</param>
    /// <param name="interval">Borders length</param>
    private void AddLeftRightSmoothBorders(float offset, int interval)
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
                newPoint = Mathf.FloorToInt(Mathf.PerlinNoise(Random.Range(0, 100001) * reduction, y) * offset);
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
                for (int y = lastPos.y; y < currentPos.y; y++) //left
                {
                    for (int x = Mathf.FloorToInt(currHeight); x > 0; x--)
                    {
                        map[x, y] = 1;
                    }
                    currHeight += heightChange;
                }

                for (int y = lastPos.y; y < currentPos.y; y++) //right
                {
                    for (int x = mapWidth - Mathf.FloorToInt(currHeight); x < mapWidth; x++)
                    {
                        map[x, y] = 1;
                    }
                    currHeight += heightChange;
                }
            }
        }
    }

    /// <summary>
    /// Erases floor and wall regions which are bigger than a given threshold. It also connects the floor regions
    /// that are bigger than the threshold
    /// </summary>
    private void EraseRegions()
    {    
        //Erase floor regions
        List<List<Vector2Int>> floorRegions = FloodFillAlgorithm.GetRegionsOfType(map, 0,mapWidth,mapHeight);
        List<Region> leftFloorRegions = new List<Region>();

        foreach (List<Vector2Int> floorRegion in floorRegions)
        {
            if (floorRegion.Count <= floorThresholdSize)
            {
                foreach (Vector2Int tile in floorRegion)
                {
                    map[tile.x, tile.y] = 1;
                }
            }
            else //we store floor regions which are bigger than the floorThresholdSize, to connect them
            {
                leftFloorRegions.Add(new Region(floorRegion, map, mapWidth, mapHeight));
            }
        }

        if (connectRegions && leftFloorRegions.Count > 0)
        {
            leftFloorRegions.Sort();
            leftFloorRegions[0].IsMainRoom = true;
            leftFloorRegions[0].IsAccessibleFromMainRoom = true;
            ConnectClosestRegions(leftFloorRegions);
        }

        //Erase wall regions
        List<List<Vector2Int>> wallRegions = FloodFillAlgorithm.GetRegionsOfType(map, 1, mapWidth, mapHeight);

        foreach (List<Vector2Int> wallRegion in wallRegions)
        {
            if (wallRegion.Count <= wallThresholdSize)
            {
                foreach (Vector2Int tile in wallRegion)
                {
                    map[tile.x, tile.y] = 0;
                }
            }
        }
    }
    
    /// <summary>
    /// Connect every map region
    /// </summary>
    /// <param name="survivingRegions">Regions we are going to connect</param>
    /// <param name="forceAccessibilityFromMainRegion">If we want to create a connection from the main region</param>
    private void ConnectClosestRegions(List<Region> survivingRegions, bool forceAccessibilityFromMainRegion = false)
    {
        List<Region> roomList1 = new List<Region>();
        List<Region> roomList2 = new List<Region>();

        if (forceAccessibilityFromMainRegion)
        {
            foreach (Region room in survivingRegions)
            {
                if (room.IsAccessibleFromMainRoom)
                {
                    roomList2.Add(room);
                }
                else
                {
                    roomList1.Add(room);
                }
            }
        }
        else
        {
            roomList1 = survivingRegions;
            roomList2 = survivingRegions;
        }

        int bestDistance = 0;
        Vector2Int bestTile1 = new Vector2Int();
        Vector2Int bestTile2 = new Vector2Int();
        Region bestRoom1 = new Region();
        Region bestRoom2 = new Region();
        bool possibleConnectionFound = false;

        foreach (Region room1 in roomList1)
        {
            if (!forceAccessibilityFromMainRegion)
            {
                possibleConnectionFound = false;
                if (room1.connectedRooms.Count > 0) continue;
            }

            foreach (Region room2 in roomList2)
            {
                if (room1 == room2 || room1.IsConnected(room2)) continue;

                for (int tileIndex1 = 0; tileIndex1 < room1.borderTiles.Count; tileIndex1++)
                {
                    for (int tileIndex2 = 0; tileIndex2 < room2.borderTiles.Count; tileIndex2++)
                    {
                        Vector2Int tile1 = room1.borderTiles[tileIndex1];
                        Vector2Int tile2 = room2.borderTiles[tileIndex2];

                        int distanceBetweenRooms = (int)Vector2.Distance(new Vector2(tile1.x, tile1.y), new Vector2(tile2.x, tile2.y));/*(int)(Mathf.Pow((tile1.posX - tile2.posX), 2) + Mathf.Pow((tile1.posY - tile2.posY), 2));*/
                        if (distanceBetweenRooms < bestDistance || !possibleConnectionFound)
                        {
                            possibleConnectionFound = true;
                            bestDistance = distanceBetweenRooms;
                            bestTile1 = tile1;
                            bestTile2 = tile2;
                            bestRoom1 = room1;
                            bestRoom2 = room2;
                        }
                    }
                }
            }

            if (possibleConnectionFound && !forceAccessibilityFromMainRegion)
            {
                CreateConnection(bestRoom1, bestRoom2, bestTile1, bestTile2);
            }
        }

        if (possibleConnectionFound && forceAccessibilityFromMainRegion)
        {
            CreateConnection(bestRoom1, bestRoom2, bestTile1, bestTile2);
            ConnectClosestRegions(survivingRegions, true);
        }

        if (!forceAccessibilityFromMainRegion)
        {
            ConnectClosestRegions(survivingRegions, true);
        }
    }

    /// <summary>
    /// Creates a connection between two regions
    /// </summary>
    /// <param name="region1">First region</param>
    /// <param name="region2">Second region</param>
    /// <param name="tile1">First region tile which is the closest one to the second region</param>
    /// <param name="tile2">Second region tile which is the closest one to the first region</param>
    private void CreateConnection(Region region1, Region region2, Vector2Int tile1, Vector2Int tile2)
    {
        Region.ConnectRooms(region1, region2);
        List<Vector2Int> path = BresenhamsLineAlgorithm.GetLinePointsList(tile1.x, tile1.y, tile2.x, tile2.y);
        foreach (Vector2Int coord in path)
        {
            DrawBiggerTile(coord, connectionSize);
        }
    }

    /// <summary>
    /// Draws a bigger tile
    /// </summary>
    /// <param name="position">Tile position</param>
    /// <param name="radius">How much we wat to increase the tile size</param>
    private void DrawBiggerTile(Vector2Int position, int radius)
    {
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                if (x * x + y * y <= radius * radius)
                {
                    int drawX = position.x + x;
                    int drawY = position.y + y;
                    if (drawX >= 0 && drawX < mapWidth && drawY >= 0 && drawY < mapHeight)
                    {
                        map[drawX, drawY] = 0;
                    }
                }
            }
        }
    }
}

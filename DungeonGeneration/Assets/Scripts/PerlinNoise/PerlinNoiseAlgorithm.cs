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
    [SerializeField] private float fillPercent = 0.5f;
    [SerializeField] private Vector2 offset;
    [SerializeField] private bool useRandomSeed = true;
    [SerializeField] private string seed;
    [SerializeField] private int wallThresholdSize=10;
    [SerializeField] private int floorThresholdSize=10;
    [SerializeField] private bool connectRegions = true;
    [Range(1, 3)]
    [SerializeField] private int connectionSize = 1;
    [Range(2, 8)]
    [SerializeField] int borderInterval = 2;
    [Range(5.0f, 10.0f)]
    [SerializeField] float borderOffset = 7.0f;
    [SerializeField] private bool addBiggerBorders = true;

    private System.Random rng = null;

    //void Start()
    //{
    //    GenerateDungeon();
    //}

    public override void GenerateDungeon()
    {
        if (useRandomSeed) seed = Time.time.ToString();
        rng = new System.Random(seed.GetHashCode());

        tilemapVisualizer.ClearTilemap();

        float[,] noiseTexture = GenerateTextureWithPerlinNoise(mapWidth, mapHeight, noiseScale, octaves, persistance, lacunarity, offset);
        CreateMap(noiseTexture);   

        if(addBiggerBorders)
        {
            AddBottomUpSmoothBorders(noiseTexture, borderOffset, borderInterval);
            AddLeftRightSmoothBorders(noiseTexture, borderOffset, borderInterval);           
        }

        EraseRegions(noiseTexture);

        AddBorders(noiseTexture);
    }

    private float[,] GenerateTextureWithPerlinNoise(int mapWidth, int mapHeight, float scale, int octaves, float persistance, float lacunarity, Vector2 offset)
    {
        float[,] noiseTextue = new float[mapWidth, mapHeight];
       
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
    private void CreateMap(float[,] map)
    {
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                if (x == 0 || y == 0 || x == mapWidth - 1 || y == mapHeight - 1)
                {
                    tilemapVisualizer.PaintSingleWallTile(new Vector2Int(x, y));
                    map[x, y] = 1;
                    continue;
                }

                if (map[x, y] < fillPercent)
                {
                    tilemapVisualizer.PaintSingleFloorTile(new Vector2Int(x, y));
                    map[x, y] = 0;
                }
                else
                {
                    tilemapVisualizer.PaintSingleWallTile(new Vector2Int(x, y));
                    map[x, y] = 1;
                }
                //tilemapVisualizer.PaintSingleFloorTileWithColor(new Vector2Int(x, y), Color.Lerp(Color.black, Color.white, map[x, y]));
            }
        }
    }
    private void AddBorders(float[,] map)
    {
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                if (x == 0 || y == 0 || x == mapWidth - 1 || y == mapHeight - 1)
                {
                    tilemapVisualizer.PaintSingleWallTile(new Vector2Int(x, y));
                    map[x, y] = 1;
                }
            }
        }
    }

    private void AddBottomUpSmoothBorders(float[,] noiseTexture, float offset, int interval)
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
                newPoint = Mathf.FloorToInt(Mathf.PerlinNoise(x, rng.Next(0, 100000) * reduction) * offset);
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
                        noiseTexture[x, y] = 1;
                        tilemapVisualizer.PaintSingleWallTile(new Vector2Int(x, y));
                    }
                    currHeight += heightChange;
                }

                for (int x = lastPos.x; x < currentPos.x; x++) //up
                {
                    for (int y = mapHeight - Mathf.FloorToInt(currHeight); y < mapHeight; y++)
                    {
                        noiseTexture[x, y] = 1;
                        tilemapVisualizer.PaintSingleWallTile(new Vector2Int(x, y));
                    }
                    currHeight += heightChange;
                }
            }
        }
    } 
    private void AddLeftRightSmoothBorders(float[,] noiseTexture, float offset, int interval)
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
                newPoint = Mathf.FloorToInt(Mathf.PerlinNoise(rng.Next(0, 100000) * reduction, y) * offset);
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
                        noiseTexture[x, y] = 1;
                        tilemapVisualizer.PaintSingleWallTile(new Vector2Int(x, y));
                    }
                    currHeight += heightChange;
                }

                for (int y = lastPos.y; y < currentPos.y; y++) //right
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

    private void EraseRegions(float[,] map)
    {
        //Erase wall regions
        List<List<Vector2Int>> wallRegions = FloodFillAlgorithm.GetRegionsOfType(map, FloodFillAlgorithm.TileType.Wall, mapWidth,mapHeight);

        foreach (List<Vector2Int> wallRegion in wallRegions)
        {
            if (wallRegion.Count <= wallThresholdSize)
            {
                foreach (Vector2Int tile in wallRegion)
                {
                    map[tile.x, tile.y] = 0;
                    tilemapVisualizer.PaintSingleFloorTile(new Vector2Int(tile.x, tile.y));
                }
            }
        }

        //Erase floor regions
        List<List<Vector2Int>> floorRegions = FloodFillAlgorithm.GetRegionsOfType(map, FloodFillAlgorithm.TileType.Floor,mapWidth,mapHeight);
        List<Room> leftFloorRegions = new List<Room>();

        foreach (List<Vector2Int> floorRegion in floorRegions)
        {
            if (floorRegion.Count <= floorThresholdSize)
            {
                foreach (Vector2Int tile in floorRegion)
                {
                    map[tile.x, tile.y] = 1;
                    tilemapVisualizer.PaintSingleWallTile(new Vector2Int(tile.x, tile.y));
                }
            }
            else //we store floor regions which are bigger than the floorThresholdSize, to connect them
            {
                leftFloorRegions.Add(new Room(floorRegion, map, mapWidth, mapHeight));
            }
        }

        if (connectRegions && leftFloorRegions.Count > 0)
        {
            leftFloorRegions.Sort();
            leftFloorRegions[0].isMainRoom = true;
            leftFloorRegions[0].isAccessibleFromMainRoom = true;
            ConnectClosestRooms(map,leftFloorRegions);
        }
    }
    

    private void ConnectClosestRooms(float[,] map,List<Room> survivingRooms, bool forceAccessibilityFromMainRoom = false)
    {
        List<Room> roomList1 = new List<Room>();
        List<Room> roomList2 = new List<Room>();

        if (forceAccessibilityFromMainRoom)
        {
            foreach (Room room in survivingRooms)
            {
                if (room.isAccessibleFromMainRoom)
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
            roomList1 = survivingRooms;
            roomList2 = survivingRooms;
        }

        int bestDistance = 0;
        Vector2Int bestTile1 = new Vector2Int();
        Vector2Int bestTile2 = new Vector2Int();
        Room bestRoom1 = new Room();
        Room bestRoom2 = new Room();
        bool possibleConnectionFound = false;

        foreach (Room room1 in roomList1)
        {
            if (!forceAccessibilityFromMainRoom)
            {
                possibleConnectionFound = false;
                if (room1.connectedRooms.Count > 0) continue;
            }

            foreach (Room room2 in roomList2)
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

            if (possibleConnectionFound && !forceAccessibilityFromMainRoom)
            {
                CreateConnection(map,bestRoom1, bestRoom2, bestTile1, bestTile2);
            }
        }

        if (possibleConnectionFound && forceAccessibilityFromMainRoom)
        {
            CreateConnection(map,bestRoom1, bestRoom2, bestTile1, bestTile2);
            ConnectClosestRooms(map,survivingRooms, true);
        }

        if (!forceAccessibilityFromMainRoom)
        {
            ConnectClosestRooms(map,survivingRooms, true);
        }
    }

    private void CreateConnection(float[,] map,Room room1, Room room2, Vector2Int tile1, Vector2Int tile2)
    {
        Room.ConnectRooms(room1, room2);
        List<Vector2Int> line = BresenhamsLineAlgorithm.GetLinePointsList(tile1.x, tile1.y, tile2.x, tile2.y);
        foreach (Vector2Int coord in line)
        {
            DrawBiggerTile(map, coord, connectionSize);
        }
    }

    private void DrawBiggerTile(float[,] map, Vector2Int coord, int radius)
    {
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                if (x * x + y * y <= radius * radius)
                {
                    int drawX = coord.x + x;
                    int drawY = coord.y + y;
                    if (drawX >= 0 && drawX < mapWidth && drawY >= 0 && drawY < mapHeight)
                    {
                        map[drawX, drawY] = 0;
                        tilemapVisualizer.PaintSingleFloorTile(new Vector2Int(drawX, drawY));
                    }
                }
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Random = UnityEngine.Random;

public struct Cell
{
    public Vector2Int cellPos;
    public float distance;
    public Vector2Int belongSeed; //semilla a la que esta mas cerca
    public Cell(Vector2Int _cellPos, float _distance)
    {
        cellPos = _cellPos;
        distance = _distance;
        belongSeed = new Vector2Int();
    }


    public void SetBelongSeed(Vector2Int seed) 
    {
        belongSeed = seed;
    }
    public void SetCellPos(Vector2Int pos)
    {
        cellPos = pos;
    }
}

public class VoronoiDiagramAlgorithm : DungeonGenerator
{
    public enum DistanceAlgorithm {Euclidean, Manhattan}

    [SerializeField] private int mapWidth = 40, mapHeight = 40;
    [SerializeField] private int numberOfSeeds = 64;
    [SerializeField] private DistanceAlgorithm distanceAlgorithm;
    [Range(0.0f,1.0f)]
    [SerializeField] private float chance = 0.5f;
    [SerializeField] private bool showColours = false;

    void Start()
    {
        GenerateDungeon();
    }

    public override void GenerateDungeon()
    {
        //HashSet<Tuple<Vector2Int, Color>> seeds = GenerateSeeds();
        //List<Cell> mapInfo = new List<Cell>(mapWidth * mapHeight);
        //for (int i = 0; i < mapWidth * mapHeight; i++) mapInfo.Add(new Cell());

        //for (int i = 0; i < mapInfo.Count; i++)
        //{
        //    int x = i % mapWidth;
        //    int y = i / mapWidth;

        //    float mindistance = float.MaxValue;

        //    foreach (Tuple<Vector2Int, Color> seed in seeds)
        //    {
        //        float distance = (distanceAlgorithm == DistanceAlgorithm.Euclidean) ? EuclideanDistance(new Vector2Int(x, y), seed.Item1) : ManhattanDistance(new Vector2Int(x, y), seed.Item1); ;

        //        if (distance < mindistance)
        //        {
        //            mindistance = distance;
        //            Vector2Int nearestSeed = seed.Item1;
        //            Cell cell = new Cell();
        //            cell.SetCellPos(new Vector2Int(x, y));
        //            cell.SetBelongSeed(nearestSeed);
        //            mapInfo[i] = cell;
        //        }

        //    }

        //}

        //tilemapVisualizer.ClearTilemap();

        //for (int x = 0; x < mapWidth; x++)
        //{
        //    for (int y = 0; y < mapHeight; y++)
        //    {
        //        Cell mySeed = mapInfo[MapXYtoIndex(x, y)];

        //        if (x == 0 || y == 0 || x == mapWidth - 1 || y == mapHeight - 1) //borders
        //        {
        //            tilemapVisualizer.PaintSingleCorridorTile(new Vector2Int(x, y));
        //            continue;
        //        }

        //        if (mapInfo[MapXYtoIndex(x - 1, y)].belongSeed != mySeed.belongSeed ||
        //        mapInfo[MapXYtoIndex(x + 1, y)].belongSeed != mySeed.belongSeed ||
        //        mapInfo[MapXYtoIndex(x, y - 1)].belongSeed != mySeed.belongSeed ||
        //        mapInfo[MapXYtoIndex(x, y + 1)].belongSeed != mySeed.belongSeed ||

        //        mapInfo[MapXYtoIndex(x - 1, y - 1)].belongSeed != mySeed.belongSeed ||
        //        mapInfo[MapXYtoIndex(x + 1, y + 1)].belongSeed != mySeed.belongSeed ||
        //        mapInfo[MapXYtoIndex(x - 1, y - 1)].belongSeed != mySeed.belongSeed ||
        //        mapInfo[MapXYtoIndex(x + 1, y + 1)].belongSeed != mySeed.belongSeed)
        //        {
        //            if (Random.value > chance) tilemapVisualizer.PaintSingleCorridorTile(new Vector2Int(x, y));
        //            else tilemapVisualizer.PaintSingleFloorTile(new Vector2Int(x, y));
        //        }
        //        else
        //        {
        //            tilemapVisualizer.PaintSingleFloorTile(new Vector2Int(x, y));
        //        }

        //    }
        //}



        HashSet<Vector2Int> seeds = GenerateSeeds1(out HashSet<Vector2Int> borderSeeds);

        List<Cell> mapInfo = new List<Cell>(mapWidth * mapHeight);
        for (int i = 0; i < mapWidth * mapHeight; i++) mapInfo.Add(new Cell());

        for (int i = 0; i < mapInfo.Count; i++)
        {
            int x = i % mapWidth;
            int y = i / mapWidth;

            float mindistance = float.MaxValue;

            foreach (Vector2Int seed in seeds)
            {
                float distance = (distanceAlgorithm == DistanceAlgorithm.Euclidean) ? EuclideanDistance(new Vector2Int(x, y), seed) : ManhattanDistance(new Vector2Int(x, y), seed); ;

                if (distance < mindistance)
                {
                    mindistance = distance;
                    Vector2Int nearestSeed = seed;
                    Cell cell = new Cell();
                    cell.SetCellPos(new Vector2Int(x, y));
                    cell.SetBelongSeed(nearestSeed);
                    mapInfo[i] = cell;
                }

            }

        }

        tilemapVisualizer.ClearTilemap();
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                Cell mySeed = mapInfo[MapXYtoIndex(x, y)];

                if (x == 0 || y == 0 || x == mapWidth - 1 || y == mapHeight - 1) //borders
                {
                    tilemapVisualizer.PaintSingleCorridorTile(new Vector2Int(x, y));
                    continue;
                }

                if (mapInfo[MapXYtoIndex(x - 1, y)].belongSeed != mySeed.belongSeed ||
                mapInfo[MapXYtoIndex(x + 1, y)].belongSeed != mySeed.belongSeed ||
                mapInfo[MapXYtoIndex(x, y - 1)].belongSeed != mySeed.belongSeed ||
                mapInfo[MapXYtoIndex(x, y + 1)].belongSeed != mySeed.belongSeed ||

                mapInfo[MapXYtoIndex(x - 1, y - 1)].belongSeed != mySeed.belongSeed ||
                mapInfo[MapXYtoIndex(x + 1, y + 1)].belongSeed != mySeed.belongSeed ||
                mapInfo[MapXYtoIndex(x - 1, y - 1)].belongSeed != mySeed.belongSeed ||
                mapInfo[MapXYtoIndex(x + 1, y + 1)].belongSeed != mySeed.belongSeed)
                {
                    if (Random.value > chance) tilemapVisualizer.PaintSingleCorridorTile(new Vector2Int(x, y));
                    else tilemapVisualizer.PaintSingleFloorTile(new Vector2Int(x, y));
                }
                else
                {
                    tilemapVisualizer.PaintSingleFloorTile(new Vector2Int(x, y));
                }

            }
        }


        Dictionary<Vector2Int, HashSet<Vector2Int>> sets = new Dictionary<Vector2Int, HashSet<Vector2Int>>();
        foreach (Vector2Int seed in borderSeeds)
        {
            /*if (!sets.ContainsKey(seed)) */sets.Add(seed, new HashSet<Vector2Int>());
        }



        foreach (Cell cell in mapInfo)
        {
            foreach (Vector2Int seed in borderSeeds)
            {
                if (cell.belongSeed == seed)
                {
                    //if (showColours) tilemapVisualizer.PaintSingleFloorTileWithColor(cell.cellPos, seed.Item2);
                    //else tilemapVisualizer.PaintSingleFloorTile(cell.cellPos);

                    sets[seed].Add(cell.cellPos);
                }
            }
        }

        for (int i = 0; i < sets.Count; i++)
        {
            tilemapVisualizer.EraseTiles(sets[borderSeeds.ElementAt(i)]);
        }      
    }

    private HashSet<Tuple<Vector2Int, Color>> GenerateSeeds()
    {
        HashSet<Tuple<Vector2Int, Color>> seeds = new HashSet<Tuple<Vector2Int, Color>>();

        while(seeds.Count<numberOfSeeds)
        {
            int xPos = Random.Range(1, mapWidth-1);
            int yPos = Random.Range(1, mapHeight-1);

            seeds.Add(new Tuple<Vector2Int, Color>(new Vector2Int(xPos, yPos),Random.ColorHSV()));
        }

        return seeds;
    }

    private HashSet<Vector2Int> GenerateSeeds1(out HashSet<Vector2Int> borderSeeds)
    {
        HashSet<Vector2Int> seeds = new HashSet<Vector2Int>();

        borderSeeds = new HashSet<Vector2Int>();

        for (int i=0;i < mapHeight; i+=mapHeight/8)
        {
            //int xPos = Random.Range(1, mapWidth / 8);

            borderSeeds.Add(new Vector2Int(mapWidth / 8, mapHeight / 8 + i));
            seeds.Add(new Vector2Int(mapWidth / 8, mapHeight/8+i));
        }

        for (int i = 0; i < mapHeight; i += mapHeight / 8)
        {
            //int xPos = Random.Range(1, mapWidth / 8);

            borderSeeds.Add(new Vector2Int(mapWidth - mapWidth / 8, mapHeight / 8 + i));
            seeds.Add(new Vector2Int(mapWidth - mapWidth / 8, mapHeight / 8 + i));
        }

        for (int i = 0; i < mapWidth; i += mapWidth / 8)
        {
            //int xPos = Random.Range(1, mapWidth / 8);

            borderSeeds.Add(new Vector2Int(mapWidth / 8 + i, mapHeight / 8));
            seeds.Add(new Vector2Int(mapWidth / 8 + i, mapHeight / 8));
        }

        for (int i = 0; i < mapWidth; i += mapWidth / 8)
        {
            //int xPos = Random.Range(1, mapWidth / 8);

            borderSeeds.Add(new Vector2Int(mapWidth / 8 + i, mapHeight - mapHeight / 8));
            seeds.Add(new Vector2Int(mapWidth / 8 + i, mapHeight - mapHeight / 8));
        }

        while (seeds.Count < numberOfSeeds)
        {
            int xPos = Random.Range(mapWidth/8+1, mapWidth - mapWidth/8);
            int yPos = Random.Range(mapHeight/8+1, mapHeight - mapHeight/8);

            seeds.Add(new Vector2Int(xPos, yPos));
        }

        return seeds;
    }

    private int MapXYtoIndex(int x, int y)
    {
        return x + (y * mapWidth);
    }

    private float ManhattanDistance(Vector2Int from, Vector2Int to)
    {
        return Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y);
    }

    private float EuclideanDistance(Vector2Int from, Vector2Int to) 
    {
        return Math.Abs((from - to).magnitude);
	}
}


//Dictionary<Vector2Int, HashSet<Vector2Int>> sets = new Dictionary<Vector2Int, HashSet<Vector2Int>>();
//foreach (Tuple<Vector2Int,Color> seed in seeds)
//{
//    if (!sets.ContainsKey(seed.Item1))  sets.Add(seed.Item1, new HashSet<Vector2Int>());
//}



//foreach (Cell cell in mapInfo)
//{
//    foreach (Tuple<Vector2Int, Color> seed in seeds)
//    {
//        if (cell.belongSeed == seed.Item1)
//        {
//            //if (showColours) tilemapVisualizer.PaintSingleFloorTileWithColor(cell.cellPos, seed.Item2);
//            //else tilemapVisualizer.PaintSingleFloorTile(cell.cellPos);

//            sets[seed.Item1].Add(cell.cellPos);
//        }
//    }
//}
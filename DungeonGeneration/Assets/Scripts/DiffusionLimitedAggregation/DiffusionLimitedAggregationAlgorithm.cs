using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//Diffusion limited aggregation algorithm
public class DiffusionLimitedAggregationAlgorithm : DungeonGenerator
{
    enum Symmetry { None, Horizontal, Vertical, Both }

    [Range(20,150)]
    [SerializeField] private int mapWidth = 80, mapHeight = 40;
    [Range(0.0f,1.0f)]
    [SerializeField] private float fillPercentage = 0.5f;
    [SerializeField] Symmetry symmetryType = Symmetry.None;
    [SerializeField] private bool useCentralAttractor = false;
    [SerializeField] bool makeWiderDiagonals = false;
    [SerializeField] bool eliminateSingleWalls = false;

    private Vector2Int startPosition;
    private int totalTiles;
    private int maxFloorPositions;

    //void Start()
    //{
    //    GenerateDungeon();
    //}

    /// <summary>
    /// Creates a dungeon with the Diffusion Limited Aggregation algorithm
    /// </summary>
    public override void GenerateDungeon()
    {
        totalTiles = mapWidth * mapHeight;
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
            ApplyHorizontalSymmetry(floorPositions, mapWidth);
        }
        else if (symmetryType == Symmetry.Vertical)
        {
            ApplyVerticalSymmetry(floorPositions, mapHeight);
        }
        else if (symmetryType == Symmetry.Both)
        {
            ApplyHorizontalAndVerticalSymmetry(floorPositions, mapWidth, mapHeight);
        }
        else if (symmetryType == Symmetry.None)
        {
            tilemapVisualizer.PaintFloorTiles(floorPositions);
        }

        if (eliminateSingleWalls)
        {
            tilemapVisualizer.EliminateSingleSpaces();
        }
    }

    /// <summary>
    /// Performs the Diffusion Limited Aggregation algorithm
    /// </summary>
    /// <returns>Returns a HashSet with all floor positions</returns>
    private HashSet<Vector2Int> DiffusionLimitedAggregation()
    {
        Map map = new Map(mapWidth, mapHeight);
        HashSet<Vector2Int> positions = new HashSet<Vector2Int>();

        startPosition = new Vector2Int(mapWidth / 2, mapHeight / 2);

        //Dig a "seed" area around your central starting point
        CreateSeed(map, positions);

        //While the number of floor tiles is less than your desired total
        while (positions.Count < maxFloorPositions)
        {
            //Select a starting point at random for your digger
            int xPos = Random.Range(0, mapWidth);
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
                    if (diggerPos.x < mapWidth - 2) { diggerPos.x += 1; }
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

    /// <summary>
    /// Performs the Diffusion Limited Aggregation algorithm with a central attractor
    /// </summary>
    /// <returns>Returns a HashSet with all floor positions</returns>
    private HashSet<Vector2Int> DiffusionLimitedAggregation_CentralAttractor()
    {
        Map map = new Map(mapWidth, mapHeight);
        HashSet<Vector2Int> positions = new HashSet<Vector2Int>();

        startPosition = new Vector2Int(mapWidth / 2, mapHeight / 2);

        //Dig a "seed" area around your central starting point
        CreateSeed(map, positions);

        //While the number of floor tiles is less than your desired total
        while (positions.Count < maxFloorPositions)
        {
            //Select a starting point at random for your digger
            int xPos = Random.Range(0, mapWidth);
            int yPos = Random.Range(0, mapHeight);
            Vector2Int diggerPos = new Vector2Int(xPos, yPos);
            Vector2Int diggerPrevPos = diggerPos;

            //line path
            List<Vector2Int> path = BresenhamsLineAlgorithm.GetLinePointsList(diggerPos.x, diggerPos.y, startPosition.x, startPosition.y,out int xStep,out int yStep).ToList();

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

    /// <summary>
    /// Creates a seed around the starting position
    /// </summary>
    /// <param name="map">Map info</param>
    /// <param name="positions">Floor positions</param>
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

    /// <summary>
    /// Make wider diagonal lines for the Centra Attractor variation
    /// </summary>
    /// <param name="diggerPos">Digger position</param>
    /// <param name="xStep">X line direction</param>
    /// <param name="yStep">Y line direction</param>
    /// <param name="map">Map info</param>
    /// <param name="positions">Florr positions</param>
    private void MakeWiderDiagonals(Vector2Int diggerPos, int xStep, int yStep, Map map, HashSet<Vector2Int> positions)
    {
        if (xStep < 0 && yStep < 0)
        {
            Vector2Int pos1 = diggerPos + new Vector2Int(-1, 0);
            Vector2Int pos2 = diggerPos + new Vector2Int(0, -1);

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
            Vector2Int pos1 = diggerPos + new Vector2Int(1, 0);
            Vector2Int pos2 = diggerPos + new Vector2Int(0, -1);

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
            Vector2Int pos1 = diggerPos + new Vector2Int(-1, 0);
            Vector2Int pos2 = diggerPos + new Vector2Int(0, 1);

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
            Vector2Int pos1 = diggerPos + new Vector2Int(1, 0);
            Vector2Int pos2 = diggerPos + new Vector2Int(0, 1);

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

    /// <summary>
    /// Paints all floor positions with a horizontal symmetry
    /// </summary>
    /// <param name="positions">Floor positions</param>
    /// <param name="mapWidth">Map width</param>
    private void ApplyHorizontalSymmetry(HashSet<Vector2Int> positions, int mapWidth)
    {
        int centerX = mapWidth / 2;

        foreach (Vector2Int pos in positions)
        {
            if (pos.x == centerX)
            {
                tilemapVisualizer.PaintSingleFloorTile(pos);
            }
            else
            {
                int distX = Mathf.Abs(centerX - pos.x);
                tilemapVisualizer.PaintSingleFloorTile(new Vector2Int(centerX + distX, pos.y));
                tilemapVisualizer.PaintSingleFloorTile(new Vector2Int(centerX - distX, pos.y));
            }
        }    
    }

    /// <summary>
    /// Paints all floor positions with a vertical symmetry
    /// </summary>
    /// <param name="positions">Floor positions</param>
    /// <param name="mapHeight">Map height</param>
    private void ApplyVerticalSymmetry(HashSet<Vector2Int> positions, int mapHeight)
    {
        int centerY = mapHeight / 2;

        foreach (Vector2Int pos in positions)
        {
            if (pos.y == centerY)
            {
                tilemapVisualizer.PaintSingleFloorTile(pos);
            }
            else
            {
                int distY = Mathf.Abs(centerY - pos.y);
                tilemapVisualizer.PaintSingleFloorTile(new Vector2Int(pos.x, centerY + distY));
                tilemapVisualizer.PaintSingleFloorTile(new Vector2Int(pos.x, centerY - distY));
            }
        }  
    }

    /// <summary>
    /// Paints all floor positions with a horizontal and vertical symmetry
    /// </summary>
    /// <param name="positions">Floor positions</param>
    /// <param name="mapWidth">Map width</param>
    /// <param name="mapHeight">Map height</param>
    private void ApplyHorizontalAndVerticalSymmetry(HashSet<Vector2Int> positions, int mapWidth, int mapHeight)
    {
        int centerX = mapWidth / 2;
        int centerY = mapHeight / 2;

        foreach (Vector2Int pos in positions)
        {
            if (pos.x == centerX && pos.y == centerY)
            {
                tilemapVisualizer.PaintSingleFloorTile(pos);
            }
            else
            {
                int distX = Mathf.Abs(centerX - pos.x);
                tilemapVisualizer.PaintSingleFloorTile(new Vector2Int(centerX + distX, pos.y));
                tilemapVisualizer.PaintSingleFloorTile(new Vector2Int(centerX - distX, pos.y));

                int distY = Mathf.Abs(centerY - pos.y);
                tilemapVisualizer.PaintSingleFloorTile(new Vector2Int(pos.x, centerY + distY));
                tilemapVisualizer.PaintSingleFloorTile(new Vector2Int(pos.x, centerY - distY));
            }
        }
    }
}
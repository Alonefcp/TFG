using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//Diffusion limited aggregation algorithm (DLA)
public class DiffusionLimitedAggregationAlgorithm : DungeonGeneration
{
    enum Symmetry { None, Horizontal, Vertical, Both }

    [Range(20,150)]
    [SerializeField] private int mapWidth = 80, mapHeight = 40;
    [Range(0.0f,1.0f)]
    [SerializeField] private float fillPercentage = 0.5f;
    [SerializeField] Symmetry symmetryType = Symmetry.None;
    [Range(1,2)]
    [SerializeField] private int brushSize = 1;
    [Range(1, 10)]
    [SerializeField] private int seedSize = 1;
    [SerializeField] private bool useCentralAttractor = false;
    [SerializeField] bool eliminateSingleWallsCells = false;

    private Vector2Int startPosition;
    private int maxFloorPositions;
    private List<bool> map; //true -> floor , false -> wall
    private HashSet<Vector2Int> floorPositions;

    //void Start()
    //{
    //    GenerateDungeon();
    //}

    /// <summary>
    /// Creates a dungeon with the Diffusion Limited Aggregation algorithm
    /// </summary>
    public override void GenerateDungeon()
    {
        base.GenerateDungeon();

        int totalTiles = mapWidth * mapHeight;
        maxFloorPositions = (int)(fillPercentage * totalTiles);

        //We get all floor positions
        if (useCentralAttractor)
        {
            floorPositions = RunDiffusionLimitedAggregationWithCentralAttractor();
        }
        else
        {
            floorPositions = RunDiffusionLimitedAggregation();
        }

        //We paint the map
        if (symmetryType == Symmetry.Horizontal)
        {
            ApplyHorizontalSymmetry();
        }
        else if (symmetryType == Symmetry.Vertical)
        {
            ApplyVerticalSymmetry();
        }
        else if (symmetryType == Symmetry.Both)
        {
            ApplyHorizontalAndVerticalSymmetry();
        }
        else if (symmetryType == Symmetry.None)
        {
            int initialSize = floorPositions.Count;
            for (int i = 0; i < initialSize; i++)
            {
                DrawWithBrush(floorPositions.ElementAt(i));
            }
        }
       
        if (eliminateSingleWallsCells)
        {
            tilemapVisualizer.EliminateSingleSpaces(out HashSet<Vector2Int> extraPositions);

            //we add the positions which have become walkables
            foreach (Vector2Int pos in extraPositions)
            {
                map[MapXYtoIndex(pos.x, pos.y)] = true;
            }
            floorPositions.UnionWith(extraPositions);
        }

        WallGenerator.CreateWalls(floorPositions, tilemapVisualizer);
        playerController.SetPlayer(startPosition, new Vector3(0.3f, 0.3f, 0.3f));
    }

    /// <summary>
    /// Draws bigger floor positions and the extra positions are stored 
    /// the floor positions hashset
    /// </summary>
    /// <param name="floorPosition">Floor position</param>
    private void DrawWithBrush(Vector2Int floorPosition)
    {
        if(brushSize==1)
        {
            tilemapVisualizer.PaintSingleFloorTile(floorPosition);
            floorPositions.Add(floorPosition);
        }
        else
        {          
            int halfBrushSize = brushSize / 2;

            for (int y = floorPosition.y - halfBrushSize; y < floorPosition.y + halfBrushSize; y++)
            {
                for (int x = floorPosition.x - halfBrushSize; x < floorPosition.x + halfBrushSize; x++)
                {
                    if (x >= 0 && x < mapWidth && y >= 0 && y < mapHeight)
                    {
                        map[MapXYtoIndex(x, y)] = true;
                        floorPositions.Add(new Vector2Int(x, y));
                        tilemapVisualizer.PaintSingleFloorTile(new Vector2Int(x,y));
                    }
                }
            }          
        }
    }

    /// <summary>
    /// Performs the Diffusion Limited Aggregation algorithm
    /// </summary>
    /// <returns>Returns a HashSet with all floor positions</returns>
    private HashSet<Vector2Int> RunDiffusionLimitedAggregation()
    {
        map = CreateMap(); //false -> hasn't floor , true -> has floor
       
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
            while (!map[MapXYtoIndex(diggerPos.x,diggerPos.y)])
            {
                //We store the digger previous position
                diggerPrevPos = diggerPos;

                int number = Random.Range(1, 5);
                //We move the digger
                if (number == 1)
                {
                    if (diggerPos.x > 1) diggerPos.x -= 1;
                }
                else if (number == 2)
                {
                    if (diggerPos.x < mapWidth - 2) { diggerPos.x += 1; }
                }
                else if (number == 3)
                {
                    if (diggerPos.y > 1) { diggerPos.y -= 1; }
                }
                else if (number == 4)
                {
                    if (diggerPos.y < mapHeight - 1) { diggerPos.y += 1; }
                }
            }

            //The digger previous position becomes a floor
            map[MapXYtoIndex(diggerPrevPos.x, diggerPrevPos.y)] = true;
            positions.Add(diggerPrevPos);
        }

        return positions;
    }

    /// <summary>
    /// Performs the Diffusion Limited Aggregation algorithm with a central attractor
    /// </summary>
    /// <returns>Returns a HashSet with all floor positions</returns>
    private HashSet<Vector2Int> RunDiffusionLimitedAggregationWithCentralAttractor()
    {
        map = CreateMap(); //false -> hasn't floor , true -> has floor
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

            //Line path
            List<Vector2Int> path = BresenhamsLineAlgorithm.GetLinePointsList(diggerPos.x, diggerPos.y, startPosition.x, startPosition.y).ToList();

            //We move the digger through the line path
            while (!map[MapXYtoIndex(diggerPos.x,diggerPos.y)] && path.Count > 0)
            {
                //We store the digger previous position
                diggerPrevPos = diggerPos;

                //We change the digger position to the first line point
                diggerPos.x = path[0].x;
                diggerPos.y = path[0].y;
                path.RemoveAt(0);
            }

            //The digger previous position becomes a floor
            map[MapXYtoIndex(diggerPrevPos.x, diggerPrevPos.y)] = true;
            positions.Add(diggerPrevPos);
        }

        return positions;
    }

    /// <summary>
    /// Creates a seed around the starting position
    /// </summary>
    /// <param name="map">Map info</param>
    /// <param name="positions">Floor positions</param>
    private void CreateSeed(List<bool> map, HashSet<Vector2Int> positions)
    {
        //map[MapXYtoIndex(startPosition.x, startPosition.y)] = true;
        //map[MapXYtoIndex(startPosition.x + 1, startPosition.y)] = true;
        //map[MapXYtoIndex(startPosition.x - 1, startPosition.y)] = true;
        //map[MapXYtoIndex(startPosition.x, startPosition.y + 1)] = true;
        //map[MapXYtoIndex(startPosition.x, startPosition.y - 1)] = true;
        //positions.Add(startPosition);
        //positions.Add(startPosition + new Vector2Int(1, 0));
        //positions.Add(startPosition + new Vector2Int(-1, 0));
        //positions.Add(startPosition + new Vector2Int(0, 1));
        //positions.Add(startPosition + new Vector2Int(0, -1));
        
        int halfSize = seedSize / 2;

        for (int x = startPosition.x-halfSize; x <= startPosition.x+halfSize; x++)
        {
            for (int y = startPosition.y - halfSize; y <= startPosition.y + halfSize; y++)
            {
                map[MapXYtoIndex(x,y)] = true;
                positions.Add(new Vector2Int(x,y));
            }
        }
    }

 
    /// <summary>
    /// Paints all floor positions with an horizontal symmetry
    /// </summary>
    private void ApplyHorizontalSymmetry()
    {
        int centerX = mapWidth / 2;
        int initialSize = floorPositions.Count;
        for (int i = 0; i < initialSize; i++)
        {
            if (floorPositions.ElementAt(i).x == centerX)
            {
                DrawWithBrush(floorPositions.ElementAt(i));
            }
            else
            {
                int distX = Mathf.Abs(centerX - floorPositions.ElementAt(i).x);
                DrawWithBrush(new Vector2Int(centerX + distX, floorPositions.ElementAt(i).y));
                DrawWithBrush(new Vector2Int(centerX - distX, floorPositions.ElementAt(i).y));
            }
        }    
    }

    /// <summary>
    /// Paints all floor positions with a vertical symmetry
    /// </summary>
    private void ApplyVerticalSymmetry()
    {
        int centerY = mapHeight / 2;

        int initialSize = floorPositions.Count;
        for (int i = 0; i < initialSize; i++)
        {
            if (floorPositions.ElementAt(i).y == centerY)
            {
                DrawWithBrush(floorPositions.ElementAt(i));
            }
            else
            {
                int distY = Mathf.Abs(centerY - floorPositions.ElementAt(i).y);
                DrawWithBrush(new Vector2Int(floorPositions.ElementAt(i).x, centerY + distY));
                DrawWithBrush(new Vector2Int(floorPositions.ElementAt(i).x, centerY - distY));
            }
        }  
    }

    /// <summary>
    /// Paints all floor positions with an horizontal and vertical symmetry
    /// </summary>
    private void ApplyHorizontalAndVerticalSymmetry()
    {
        int centerX = mapWidth / 2;
        int centerY = mapHeight / 2;

        int initialSize = floorPositions.Count;
        for (int i = 0; i < initialSize; i++)
        {
            if (floorPositions.ElementAt(i).x == centerX && floorPositions.ElementAt(i).y == centerY)
            {
                DrawWithBrush(floorPositions.ElementAt(i));
            }
            else
            {
                int distX = Mathf.Abs(centerX - floorPositions.ElementAt(i).x);
                DrawWithBrush(new Vector2Int(centerX + distX, floorPositions.ElementAt(i).y));
                DrawWithBrush(new Vector2Int(centerX - distX, floorPositions.ElementAt(i).y));

                int distY = Mathf.Abs(centerY - floorPositions.ElementAt(i).y);
                DrawWithBrush(new Vector2Int(floorPositions.ElementAt(i).x, centerY + distY));
                DrawWithBrush(new Vector2Int(floorPositions.ElementAt(i).x, centerY - distY));
            }
        }
    }

    /// <summary>
    /// Creates a map of booleans where false represents that there isn`t a floor and 
    /// true represents that there is a floor
    /// </summary>
    /// <returns>A list of booleans where all elements are false</returns>
    private List<bool> CreateMap()
    {
        List<bool> map = new List<bool>(); //false -> hasn't floor , true -> has floor
        for (int i = 0; i < mapWidth * mapHeight; i++) map.Add(false);
        return map;
    }

    /// <summary>
    /// Converts a map position to an index.
    /// </summary>
    /// <param name="x">X map position</param>
    /// <param name="y">Y map position</param>
    /// <returns>A integer which represents a map position</returns>
    private int MapXYtoIndex(int x, int y)
    {
        return x + (y * mapWidth);
    }
}
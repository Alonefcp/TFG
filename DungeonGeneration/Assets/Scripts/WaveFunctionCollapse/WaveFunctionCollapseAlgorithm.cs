using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WaveFunctionCollapseAlgorithm : DungeonGenerator 
{
    [Serializable]
    private struct TileInfo
    {
        public Sprite sprite;
        public int nRotations;
        public List<string> edges; 
    }
    [Range(5,50)]
    [SerializeField] private int mapWidth = 5, mapHeight = 5;
    [SerializeField] private bool useRandomSeed = true;
    [SerializeField] private string seed;
    [SerializeField] private bool addBorder=false;
    [SerializeField] private bool forceMoreWalkableZones=false;
    [Range(1, 15)]
    [SerializeField] private int nWalkableZones = 3;
    [Range(1, 15)]
    [SerializeField] private int maxWalkableZoneSize = 3;
    [SerializeField] private TileInfo[] tilesInfo;
    private List<WFCCell> grid;
    private List<WFCTile> tiles;
    private List<int> options;
    private TileEdges tileEdges;
    private System.Random rng = null;
    //private bool firstTime = true;
    private HashSet<Vector2Int> walkablePositions;

    //private void Start()
    //{
    //    if (useRandomSeed) seed = Time.time.ToString();
    //    rng = new System.Random(seed.GetHashCode());
    //    tilemapVisualizer.ClearTilemap();
    //    SetUp();

    //    InvokeRepeating("Run", 0.5f, 0.03f);
    //    if (addBorder) DrawBorders();
    //}
    //private void Run()
    //{
    //    RunWaveFunctionCollapsed();
    //}

    public override void GenerateDungeon()
    {
        if (useRandomSeed) seed = Time.time.ToString();
        rng = new System.Random(seed.GetHashCode());

        tilemapVisualizer.ClearTilemaps();

        SetUp();

        bool finishsed = false;
        while (!finishsed)
        {
            finishsed = RunWaveFunctionCollapsed();
        }

        DrawMap();
        if (addBorder) DrawBorders();

        //foreach (Vector2Int pos in walkablePositions) //DEBUG:For seeing all forced walkable poisitions
        //{
        //    tilemapVisualizer.PaintSingleWallTile(new Vector2Int(pos.x, pos.y));
        //}
    }
    
    /// <summary>
    /// Draws the map's borders by using the tilemap visualizer.
    /// </summary>
    private void DrawBorders()
    {        
        for (int row = -1; row < mapHeight + 1; row++)
        {
            tilemapVisualizer.PaintSingleFloorTile(tiles[0].Tile, new Vector2Int(-1, row));
        }
        for (int row = 0; row < mapHeight + 1; row++)
        {
            tilemapVisualizer.PaintSingleFloorTile(tiles[0].Tile, new Vector2Int(mapWidth, row));
        }

        for (int col = -1; col < mapWidth + 1; col++)
        {
            tilemapVisualizer.PaintSingleFloorTile(tiles[0].Tile, new Vector2Int(col, -1));
        }

        for (int col = 0; col < mapWidth + 1; col++)
        {
            tilemapVisualizer.PaintSingleFloorTile(tiles[0].Tile, new Vector2Int(col, mapHeight));
        }
    }

    /// <summary>
    /// Draws the map by using the tilemap visualizer.
    /// </summary>
    private void DrawMap()
    {
        for (int row = 0; row < mapHeight; row++)
        {
            for (int col = 0; col < mapWidth; col++)
            {                
                WFCCell cell = grid[col + row * mapWidth];
                if (cell.Collapsed)
                {
                    int index = cell.Options[0];
                    tilemapVisualizer.PaintSingleFloorTile(tiles[index].Tile, new Vector2Int(col, row));
                }
            }
        }
    }

    /// <summary>
    /// Creates all images and the map grid.
    /// </summary>
    private void SetUp()
    {
        tiles = new List<WFCTile>();
        tileEdges = new TileEdges();

        //Create tiles
        for (int i = 0; i < tilesInfo.Length; i++)
        {           
            Tile tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = tilesInfo[i].sprite;

            List<string> edges = tileEdges.CreateTileEdges(tile.sprite.texture);
            tiles.Add(new WFCTile(tile, edges));

            //tiles.Add(new WFCTile(tile, tilesInfo[i].edges));
        }

        //Create rotations
        for (int j = 0; j < tilesInfo.Length; j++)
        {
            if (tilesInfo[j].nRotations <= 0) continue;

            Texture2D baseTexture = tiles[j].Tile.sprite.texture;
            for (int i = 1; i <= tilesInfo[j].nRotations; i++)
            {
                WFCTile wFCTile = tiles[j].RotateTile(baseTexture, i);
                tiles.Add(wFCTile);

                baseTexture = wFCTile.Tile.sprite.texture;
            }
        }

        //int pos = 0;
        //foreach (WFCTile tile in tiles) //DEBUG: For seeing all the tiles
        //{
        //    tilemapVisualizer.PaintSingleTile(tile.tile, new Vector2Int(pos, 0));

        //    foreach (string edge in tile.edges)
        //    {
        //        Debug.Log(edge);
        //    }

        //    Debug.Log("\n");

        //    pos++;
        //}

        options = new List<int>();
        for (int i = 0; i < tiles.Count; i++)
        { 
            options.Add(i);
            tiles[i].SetNeighbours(tiles);
        }

        grid = CreateGrid();
        if (forceMoreWalkableZones) ForceMoreWalkableZones();
    }

    /// <summary>
    /// Creates walkables zones on the map.
    /// </summary>
    /// <returns></returns>
    private void ForceMoreWalkableZones()
    {
        walkablePositions = new HashSet<Vector2Int>();
        for (int i = 0; i < nWalkableZones; i++)
        {
            int x = rng.Next(0, mapWidth);
            int y = rng.Next(0, mapHeight);

            HashSet<Vector2Int> auxPos = new HashSet<Vector2Int>();
            while (auxPos.Count < maxWalkableZoneSize)
            {
                if(!walkablePositions.Contains(new Vector2Int(x, y)))auxPos.Add(new Vector2Int(x, y));
                Vector2Int randomDirection = Directions.GetRandomFourDirection(rng);
                x += randomDirection.x;
                y += randomDirection.y;
                x = Mathf.Clamp(x, 0, mapWidth - 1);
                y = Mathf.Clamp(y, 0, mapHeight - 1);
            }

            walkablePositions.UnionWith(auxPos);
        }

        foreach (Vector2Int pos in walkablePositions)
        {
            //WFCCell cell = new WFCCell(true, grid[MapXYtoIndex(pos.x, pos.y)].gridIndex, tiles.Count);
            //cell.SetOptions(new List<int> { 1 });

            //grid[MapXYtoIndex(pos.x, pos.y)] = cell;

            grid[MapXYtoIndex(pos.x, pos.y)].Collapsed = true;
            grid[MapXYtoIndex(pos.x, pos.y)].Options = new List<int> { 1 };
        }

        //We change the entropy of the adjacent cells to the collapsed cells
        List<WFCCell> adjacentCellsToTheCollapsedCells = GetAdjacentCellsToCollapsedCells(grid);

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                int index = MapXYtoIndex(x, y);

                if (!grid[index].Collapsed && adjacentCellsToTheCollapsedCells.Contains(grid[index]))
                {
                    List<int> availableOptions = new List<int>(options);

                    if (y + 1 < mapHeight) //Look up
                    {
                        CheckNeighbourd(x, y + 1, availableOptions, 2, grid);
                    }

                    if (x + 1 < mapWidth) //Look right
                    {
                        CheckNeighbourd(x + 1, y, availableOptions, 3, grid);
                    }

                    if (y - 1 >= 0) //Look down
                    {
                        CheckNeighbourd(x, y - 1, availableOptions, 0, grid);
                    }

                    if (x - 1 >= 0) //Look left
                    {
                        CheckNeighbourd(x - 1, y, availableOptions, 1, grid);
                    }

                    WFCCell nextCell = new WFCCell(false, index, tiles.Count);
                    nextCell.Options = availableOptions;
                    grid[index] = nextCell;
                }

            }
        }
    }

    /// <summary>
    /// Creates a grid with all cells uncollapsed with all available options.
    /// </summary>
    /// <returns></returns>
    private List<WFCCell> CreateGrid()
    {
        List<WFCCell> grid = new List<WFCCell>();
        for (int i = 0; i < mapWidth * mapHeight; i++)
        {
            grid.Add(new WFCCell(false, i, tiles.Count));
        }

        return grid;
    }

    /// <summary>
    /// Returns all the adjacent cells to the collapsed cells.
    /// </summary>
    /// <returns></returns>
    private List<WFCCell> GetAdjacentCellsToCollapsedCells(List<WFCCell> grid)
    {
        List<WFCCell> collapsedCells = grid.Where(cell => cell.Collapsed).ToList();
        List<WFCCell> adjacentCellsToCollapsedCells = new List<WFCCell>();

        for (int i = 0; i < collapsedCells.Count; i++)
        {
            //Collapsed cell position
            int x = collapsedCells[i].GridIndex % mapWidth;
            int y = collapsedCells[i].GridIndex / mapWidth;

            //Up
            if (y + 1 < mapHeight)
            {
                int index = MapXYtoIndex(x, y + 1);
                if (!grid[index].Collapsed && !adjacentCellsToCollapsedCells.Contains(grid[index]))
                {
                    adjacentCellsToCollapsedCells.Add(grid[index]);
                }
            }

            //Right
            if (x + 1 < mapWidth)
            {
                int index = MapXYtoIndex(x + 1, y);
                if (!grid[index].Collapsed && !adjacentCellsToCollapsedCells.Contains(grid[index]))
                {
                    adjacentCellsToCollapsedCells.Add(grid[index]);
                }
            }

            //Down
            if (y - 1 >= 0)
            {
                int index = MapXYtoIndex(x, y - 1);
                if (!grid[index].Collapsed && !adjacentCellsToCollapsedCells.Contains(grid[index]))
                {
                    adjacentCellsToCollapsedCells.Add(grid[index]);
                }
            }

            //Left
            if (i - 1 >= 0)
            {
                int index = MapXYtoIndex(x - 1, y);
                if (!grid[index].Collapsed && !adjacentCellsToCollapsedCells.Contains(grid[index]))
                {
                    adjacentCellsToCollapsedCells.Add(grid[index]);
                }
            }

        }

        return adjacentCellsToCollapsedCells;
    }

    /// <summary>
    /// Sets all valid options if the current cell by checking its neighbours (up,right,down,left)
    /// </summary>
    /// <param name="x">Neighbour cell x position</param>
    /// <param name="y">Neighbour cell y position</param>
    /// <param name="availableOptions">All available options of the cell</param>
    /// <param name="direction">0:up options, 1:right options, 2:down options, 3:left options</param>
    private void CheckNeighbourd(int x, int y, List<int> availableOptions, int direction, List<WFCCell> grid)
    {
        WFCCell cell = grid[MapXYtoIndex(x, y)];
        HashSet<int> validOptions = new HashSet<int>();
        foreach (int option in cell.Options)
        {
            if(direction==0)
            {
                List<int> valid = tiles[option].Up;
                validOptions = validOptions.Concat(valid).ToHashSet();
            }
            else if(direction==1)
            {
                List<int> valid = tiles[option].Right;
                validOptions = validOptions.Concat(valid).ToHashSet();
            }
            else if(direction==2)
            {
                List<int> valid = tiles[option].Down;
                validOptions = validOptions.Concat(valid).ToHashSet();
            }
            else if(direction==3)
            {
                List<int> valid = tiles[option].Left;
                validOptions = validOptions.Concat(valid).ToHashSet();
            }          
        }
        CheckValidOptions(availableOptions, validOptions);
    }

    /// <summary>
    /// Sets and returns a collapsed cell. If the the collapsed cell doesn't have
    /// any options it returns null.
    /// </summary>
    /// <param name="gridCopy">Grid copy map</param>
    /// <returns></returns>
    private WFCCell GetCollapsedCell(List<WFCCell> gridCopy)
    {
        WFCCell collapsedCell = gridCopy[0];
        //if (firstTime)
        //{
        //   randomCell = gridCopy[0];
        //   firstTime = false;
        //}
        //else
        //{
        //    randomCell = gridCopy[rng.Next(0, gridCopy.Count)];
        //}

        if (collapsedCell.Options.Count <= 0)
        {           
            return null;
        }

        collapsedCell.Collapsed = true;
        int randomCellOp = collapsedCell.Options[rng.Next(0, collapsedCell.Options.Count)];
        collapsedCell.Options = new List<int>() { randomCellOp };
        return collapsedCell;
    }

    /// <summary>
    /// Performs the wave function collapse algorithm. It return true if a map is generated
    /// otherwise it returns false.
    /// </summary>
    /// <returns></returns>
    private bool RunWaveFunctionCollapsed()
    {
        //We sort the uncollapsed cells by its entropy
        List<WFCCell> gridCopy = new List<WFCCell>(grid);
        gridCopy.RemoveAll(cell => cell.Collapsed);

        if (gridCopy.Count == 0) //All cells are collapse, we finish
        {
            return true;
        }
        gridCopy.Sort((s1, s2) => s1.Options.Count.CompareTo(s2.Options.Count));

        //int minimunEntropy = gridCopy[0].options.Count;
        //gridCopy.RemoveAll(cell => cell.options.Count > minimunEntropy);

        //We get a collapsed cell with the least entropy
        WFCCell collapsedCell = GetCollapsedCell(gridCopy);
        if(collapsedCell==null) //It the collapsed cell is null, we start over
        {
            Debug.Log("Restart");
            tilemapVisualizer.ClearTilemaps();
            grid = CreateGrid();
            if (forceMoreWalkableZones) ForceMoreWalkableZones();
            return false;
        }

        //We assign the collapsed cell to the grid map
        grid[collapsedCell.GridIndex] = collapsedCell;

        //We create a grid map for storing the next iteration
        List<WFCCell> nextGrid = CreateGrid();

        //We get all the adjacent cells to the collapsed cells
        List<WFCCell> adjacentCellsToTheCollapsedCells = GetAdjacentCellsToCollapsedCells(grid);

        //We iterate through the grid map
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                int index = MapXYtoIndex(x, y);

                //if (grid[index].Collapsed) //DEBUG: For seeing the algorithm step by step
                //{
                //    int img = grid[index].Options[0];
                //    tilemapVisualizer.PaintSingleFloorTile(tiles[img].Tile, new Vector2Int(x, y));
                //}

                //If the cell is not a neighbour of a collapsed cell or it is collapsed
                if (grid[index].Collapsed || !adjacentCellsToTheCollapsedCells.Contains(grid[index]))
                {
                    nextGrid[index] = grid[index];
                }
                else //If the cell is a neighbour of a collapsed cell
                {
                    List<int> availableOptions = new List<int>(options);
                    
                    if (y + 1 < mapHeight) //Look up
                    {
                        CheckNeighbourd(x, y + 1, availableOptions, 2, grid);
                    }
                  
                    if (x + 1 < mapWidth) //Look right
                    {
                        CheckNeighbourd(x+1, y, availableOptions, 3, grid);
                    }
                    
                    if (y - 1 >= 0) //Look down
                    {
                        CheckNeighbourd(x, y - 1, availableOptions, 0, grid);
                    }
                   
                    if (x - 1 >= 0) //Look left
                    {
                        CheckNeighbourd(x-1, y, availableOptions, 1, grid);
                    }

                    WFCCell nextCell = new WFCCell(false, index, tiles.Count);
                    nextCell.Options = availableOptions;
                    nextGrid[index] = nextCell;
                }
            }
        }

        grid = new List<WFCCell>(nextGrid);

        return false;
    }

    /// <summary>
    /// Converts a map position to an index.
    /// </summary>
    /// <param name="x">X map position</param>
    /// <param name="y">Y map position</param>
    /// <returns></returns>
    private int MapXYtoIndex(int x, int y)
    {
        return x + (y * mapWidth);
    }

    /// <summary>
    /// Checks which available options are valid.
    /// </summary>
    /// <param name="availableOptions">All available options of the cell</param>
    /// <param name="validOptions">All valid options of the cell</param>
    private void CheckValidOptions(List<int> availableOptions, HashSet<int> validOptions)
    {
        for (int i = availableOptions.Count-1; i >= 0; i--)
        {
            if(!validOptions.Contains(availableOptions[i]))
            {
                availableOptions.RemoveAt(i);
            }
        }
    }
}

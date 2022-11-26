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
    [SerializeField] private int width = 5, height = 5;
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
    private System.Random rng = null;
    //private bool firstTime = true;
    private HashSet<Vector2Int> positions;

    //private void Start()
    //{
    //    if (useRandomSeed) seed = Time.time.ToString();
    //    rng = new System.Random(seed.GetHashCode());
    //    tilemapVisualizer.ClearTilemap();
    //    SetUp();

    //    InvokeRepeating("Run", 0.5f, 0.05f);
    //    //for (int i = 0; i < width * height; i++)
    //    //{
    //    //    RunWFC(out bool restart);
    //    //}

    //}
    //private void Run()
    //{
    //    RunWFC(out bool restart);
    //    if (restart) { tilemapVisualizer.ClearTilemap(); }
    //}


    public override void GenerateDungeon()
    {
        if (useRandomSeed) seed = Time.time.ToString();
        rng = new System.Random(seed.GetHashCode());

        tilemapVisualizer.ClearTilemap();

        SetUp();

        for (int i = 0; i < width * height; i++)
        {
            RunWFC(out bool restart);
            if (restart)
            {
                tilemapVisualizer.ClearTilemap();
                i = -1;
            }
        }

        DrawMap();
        if (addBorder) DrawBorders();
        //foreach (Vector2Int pos in positions)
        //{
        //    tilemapVisualizer.PaintSingleWallTile(new Vector2Int(pos.x, pos.y));
        //}
    }
    
    private void DrawBorders()
    {        
        for (int row = -1; row < height + 1; row++)
        {
            tilemapVisualizer.PaintSingleTile(tiles[0].tile, new Vector2Int(-1, row));
        }
        for (int row = 0; row < height + 1; row++)
        {
            tilemapVisualizer.PaintSingleTile(tiles[0].tile, new Vector2Int(width, row));
        }

        for (int col = -1; col < width + 1; col++)
        {
            tilemapVisualizer.PaintSingleTile(tiles[0].tile, new Vector2Int(col, -1));
        }

        for (int col = 0; col < width + 1; col++)
        {
            tilemapVisualizer.PaintSingleTile(tiles[0].tile, new Vector2Int(col, height));
        }
    }

    private void DrawMap()
    {
        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {                
                WFCCell cell = grid[col + row * width];
                if (cell.collapsed)
                {
                    int index = cell.options[0];
                    tilemapVisualizer.PaintSingleTile(tiles[index].tile, new Vector2Int(col, row));
                }
            }
        }
    }

    private void SetUp()
    {          
        tiles = new List<WFCTile>();

        //Create tiles
        for (int i = 0; i < tilesInfo.Length; i++)
        {           
            Tile tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = tilesInfo[i].sprite;
            tiles.Add(new WFCTile(tile, tilesInfo[i].edges));
        }

        //Create rotations
        for (int j = 0; j < tilesInfo.Length; j++)
        {
            if (tilesInfo[j].nRotations <= 0) continue;

            Texture2D baseTexture = tiles[j].tile.sprite.texture;
            for (int i = 1; i <= tilesInfo[j].nRotations; i++)
            {
                WFCTile wFCTile = tiles[j].RotateTile(baseTexture, i);
                tiles.Add(wFCTile);

                baseTexture = wFCTile.tile.sprite.texture;
            }
        }

        //int pos = 0;
        //foreach (WFCTile tile in tiles)
        //{
        //    tilemapVisualizer.PaintSingleTile(tile.image, new Vector2Int(pos, 0));
        //    pos++;
        //}
 
        options = new List<int>();
        for (int i = 0; i < tiles.Count; i++)
        { 
            options.Add(i);
            tiles[i].SetNeighbours(tiles);
        }

        if (forceMoreWalkableZones) grid = CreateGridWithMoreWalkableZones();
        else grid = CreateGrid();                
    }

    private void ForceMoreWalkableZones(List<WFCCell> grid)
    {
        positions = new HashSet<Vector2Int>();
        for (int i = 0; i < nWalkableZones; i++)
        {
            int x = rng.Next(0, width);
            int y = rng.Next(0, height);

            HashSet<Vector2Int> auxPos = new HashSet<Vector2Int>();
            while (auxPos.Count < maxWalkableZoneSize)
            {
                if(!positions.Contains(new Vector2Int(x, y)))auxPos.Add(new Vector2Int(x, y));
                Vector2Int randomDirection = Directions.GetRandomFourDirection(rng);
                x += randomDirection.x;
                y += randomDirection.y;
                x = Mathf.Clamp(x, 0, width - 1);
                y = Mathf.Clamp(y, 0, height - 1);
            }

            positions.UnionWith(auxPos);
        }

        foreach (Vector2Int pos in positions)
        {
            WFCCell cell = new WFCCell(true, grid[MapXYtoIndex(pos.x, pos.y)].gridIndex, tiles.Count);
            cell.SetOptions(new List<int> { 1 });

            int index = MapXYtoIndex(pos.x, pos.y);
            grid[MapXYtoIndex(pos.x, pos.y)] = cell;
        }
    }

    private List<WFCCell> CreateGrid()
    {
        List<WFCCell> grid = new List<WFCCell>();
        for (int i = 0; i < width * height; i++)
        {
            grid.Add(new WFCCell(false, i, tiles.Count));
        }

        return grid;
    }

    private List<WFCCell> CreateGridWithMoreWalkableZones()
    {
        List<WFCCell> grid = CreateGrid();
        ForceMoreWalkableZones(grid);
        return grid;
    }

    private List<WFCCell> GetAdjacentCellsToCollapsedCells()
    {
        List<WFCCell> collapsedCells = grid.Where(cell => cell.collapsed).ToList();
        List<WFCCell> collapsedCellsNeighbours = new List<WFCCell>();

        for (int i = 0; i < collapsedCells.Count; i++)
        {
            int x = i % width;
            int y = i / width;

            //Up
            if (y + 1 < height)
            {
                int index = MapXYtoIndex(x, y + 1);
                if (!grid[index].collapsed /*&& !collapsedCells.Contains(grid[index])*/)
                {
                    collapsedCellsNeighbours.Add(grid[index]);
                }
            }

            //Right
            if (x + 1 < width)
            {
                int index = MapXYtoIndex(x + 1, y);
                if (!grid[index].collapsed /*&& !collapsedCells.Contains(grid[index])*/)
                {
                    collapsedCellsNeighbours.Add(grid[index]);
                }
            }

            //Down
            if (y - 1 >= 0)
            {
                int index = MapXYtoIndex(x, y - 1);
                if (!grid[index].collapsed /*&& !collapsedCells.Contains(grid[index])*/)
                {
                    collapsedCellsNeighbours.Add(grid[index]);
                }
            }

            //Left
            if (i - 1 >= 0)
            {
                int index = MapXYtoIndex(x - 1, y);
                if (!grid[index].collapsed /*&& !collapsedCells.Contains(grid[index])*/)
                {
                    collapsedCellsNeighbours.Add(grid[index]);
                }
            }

        }

        return collapsedCellsNeighbours;
    }

    private void CheckNeighbourd(int x, int y, List<int> availableOptions, int direction)
    {
        WFCCell cell = grid[MapXYtoIndex(x, y)];
        HashSet<int> validOptions = new HashSet<int>();
        foreach (int option in cell.options)
        {
            if(direction==0)
            {
                List<int> valid = tiles[option].up;
                validOptions = validOptions.Concat(valid).ToHashSet();
            }
            else if(direction==1)
            {
                List<int> valid = tiles[option].right;
                validOptions = validOptions.Concat(valid).ToHashSet();
            }
            else if(direction==2)
            {
                List<int> valid = tiles[option].down;
                validOptions = validOptions.Concat(valid).ToHashSet();
            }
            else if(direction==3)
            {
                List<int> valid = tiles[option].left;
                validOptions = validOptions.Concat(valid).ToHashSet();
            }          
        }
        CheckValidOptions(availableOptions, validOptions);
    }

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

        if (collapsedCell.options.Count <= 0)
        {           
            return null;
        }

        collapsedCell.collapsed = true;
        int randomCellOp = collapsedCell.options[rng.Next(0, collapsedCell.options.Count)];
        collapsedCell.SetOptions(new List<int>() { randomCellOp });
        return collapsedCell;
    }

    private void RunWFC(out bool restart)
    {
        restart = false;
        List<WFCCell> gridCopy = new List<WFCCell>(grid);

        gridCopy.RemoveAll(cell => cell.collapsed);

        if (gridCopy.Count == 0)
        {
            restart = false; return;
        }

        gridCopy.Sort((s1, s2) => s1.options.Count.CompareTo(s2.options.Count));

        //int minimunEntropy = gridCopy[0].options.Count;
        //gridCopy.RemoveAll(cell => cell.options.Count > minimunEntropy);

        WFCCell collapsedCell = GetCollapsedCell(gridCopy);
        if(collapsedCell==null)
        {
            Debug.Log("Restart");
            //grid = CreateGrid();
            if (forceMoreWalkableZones) grid = CreateGridWithMoreWalkableZones();
            else grid = CreateGrid();
            restart = true;
            return;
        }
        grid[collapsedCell.gridIndex] = collapsedCell;

        List<WFCCell> nextGrid = CreateGrid();

        List<WFCCell> adjacentCellsToTheCollapsedCells = GetAdjacentCellsToCollapsedCells();

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                int index = MapXYtoIndex(j, i);

                //if (grid[index].collapsed)
                //{
                //    int img = grid[index].options[0];
                //    tilemapVisualizer.PaintSingleTile(tiles[img].tile, new Vector2Int(j, i));
                //}

                if (grid[index].collapsed || !adjacentCellsToTheCollapsedCells.Contains(grid[index]))
                {
                    nextGrid[index] = grid[index];
                }
                else
                {
                    List<int> availableOptions = new List<int>(options);
                    
                    if (i + 1 < height) //Look up
                    {
                        CheckNeighbourd(j, i + 1, availableOptions, 2);
                    }
                  
                    if (j + 1 < width) //Look right
                    {
                        CheckNeighbourd(j+1, i, availableOptions, 3);
                    }
                    
                    if (i - 1 >= 0) //Look down
                    {
                        CheckNeighbourd(j, i - 1, availableOptions, 0);
                    }
                   
                    if (j - 1 >= 0) //Look left
                    {
                        CheckNeighbourd(j-1, i, availableOptions, 1);
                    }

                    WFCCell nextCell = new WFCCell(false, index, tiles.Count);
                    nextCell.SetOptions(availableOptions);
                    nextGrid[index] = nextCell;
                }
            }
        }

        grid = nextGrid;
    }

    private int MapXYtoIndex(int x, int y)
    {
        return x + (y * width);
    }

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

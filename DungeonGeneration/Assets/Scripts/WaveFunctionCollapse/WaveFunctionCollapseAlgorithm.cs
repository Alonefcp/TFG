using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WaveFunctionCollapseAlgorithm : DungeonGenerator 
{
    struct Cell 
    {
        public bool collapsed;
        public int[] options;
        public int index;

        public Cell(bool _collapsed, int _index,int num)
        {
            collapsed = _collapsed;
            index = _index;
            options = new int[num];
            for (int i = 0; i < num; i++)
            {
                options[i] = i;
            }
        }

        public void SetOptions(int[] op)
        {
            options = op;
        }

        public void SetCollapsed(bool b)
        {
            collapsed = b;
        }
    }

    [SerializeField] private Sprite[] sprites;
    [SerializeField] private int width = 5, height = 5;
    [SerializeField] private bool useRandomSeed = true;
    [SerializeField] private string seed;
    private List<Cell> grid;
    private Tile[] tileImages;
    private System.Random rng = null;
    private List<WFCTile> tiles;

    //private void Start()
    //{
    //    if (useRandomSeed) seed = Time.time.ToString();
    //    rng = new System.Random(seed.GetHashCode());
    //    tilemapVisualizer.ClearTilemap();
    //    SetUp();
    //    for (int i = 0; i < width * height; i++)
    //    {
    //        RunWFC(out bool restart);
    //    }

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
            if (restart) { tilemapVisualizer.ClearTilemap(); i = 0; }
        }

        foreach (Cell cell in grid.Where(c => c.collapsed == false).ToArray())
        {
            Cell newCell = new Cell(true,cell.index,tiles.Count);
            newCell.SetOptions(cell.options);
            grid[newCell.index] = newCell;
        } 
   
        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                Cell cell = grid[col + row * width];
                if (cell.collapsed)
                {
                    int index = (int)cell.options[0];
                    tilemapVisualizer.PaintSingleTile(tiles[index].image, new Vector2Int(col, row));
                }
            }
        }
    }

    //private void Update()
    //{
    //    if (Input.GetMouseButtonDown(0))
    //    {
    //        RunWFC(out bool restart);
    //    }
    //}

    private void SetUp()
    {
        tileImages = new Tile[sprites.Length];
        for (int i = 0; i < sprites.Length; i++)
        {
            tileImages[i] = ScriptableObject.CreateInstance<Tile>();
            tileImages[i].sprite = sprites[i];
        }

        tiles = new List<WFCTile>();
        tiles.Add(new WFCTile(tileImages[0], new List<int> { 0, 0, 0, 0 }));
        tiles.Add(new WFCTile(tileImages[1], new List<int> { 1, 1, 0, 1 }));
        tiles.Add(new WFCTile(tileImages[2], new List<int> { 1, 1, 1, 0 }));
        tiles.Add(new WFCTile(tileImages[3], new List<int> { 0, 1, 1, 1 }));
        tiles.Add(new WFCTile(tileImages[4], new List<int> { 1, 0, 1, 1 }));
    
        for (int i = 0; i < tiles.Count; i++)
        {
            tiles[i].SetNeighbours(tiles);
        }

        grid = new List<Cell>();
        for (int i = 0; i < width*height; i++)
        {
            grid.Add(new Cell(false,i,tiles.Count));
        }

        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {              
                tilemapVisualizer.PaintSingleFloorTile(new Vector2Int(col, row));               
            }
        }
    }

    private void RunWFC(out bool restart)
    {
        restart = false;
        List<Cell> gridCopy = new List<Cell>(grid);

        gridCopy.RemoveAll(c => c.collapsed);

        if (gridCopy.Count == 0) { restart = false; return; }

        gridCopy.Sort((s1, s2) => s1.options.Length.CompareTo(s2.options.Length));

        int len = gridCopy[0].options.Length;
        int stopIndex = 0;
        for (int i = 1; i < gridCopy.Count; i++)
        {
            if (gridCopy[i].options.Length > len)
            {
                stopIndex = i;
                break;
            }
        }
        if (stopIndex > 0)
        {
            for (int i = stopIndex; i < gridCopy.Count; i++)
            {
                gridCopy.Remove(gridCopy[i]);
            }
        }

        Cell randomCell = gridCopy[0];
        if(randomCell.options.Length<=0)
        {
            Debug.Log("Restart");
            grid = new List<Cell>();

            for (int i = 0; i < width * height; i++)
            {
                grid.Add(new Cell(false,i,tiles.Count));
            }
            restart = true;
            return;
        }

        randomCell.collapsed = true;
        int randomCellOp = randomCell.options[rng.Next(0,randomCell.options.Length)];
        randomCell.SetOptions(new int[] { randomCellOp });
        grid[randomCell.index] = randomCell;

        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                Cell cell = grid[col + row * width];
                if (cell.collapsed)
                {
                    int index = (int)cell.options[0];
                    tilemapVisualizer.PaintSingleTile(tiles[index].image, new Vector2Int(col, row));
                }
            }
        }

        List<Cell> nextGrid = new List<Cell>();
        for (int i = 0; i < width * height; i++)
        {
            nextGrid.Add(new Cell(false,i,tiles.Count));
        }

        for (int i = 0; i < height; i++) 
        {
            for (int j = 0; j < width; j++) 
            {                   
                int index = MapXYtoIndex(j, i);

                if (grid[index].collapsed)
                {
                    nextGrid[index] = grid[index];
                }
                else
                {
                    List<int> options = new List<int> { 0, 1, 2, 3, 4};

                    //Look up
                    if (i + 1 < height)
                    {
                        Cell up = grid[MapXYtoIndex(j, i + 1)];
                        HashSet<int> validOptions = new HashSet<int>();
                        foreach (var option in up.options)
                        {
                            List<int> valid = tiles[option].down;
                            validOptions = validOptions.Concat(valid).ToHashSet();
                        }

                        CheckValid(options, validOptions);
                    }

                    //Look right
                    if (j + 1 < width)
                    {   
                        Cell right = grid[MapXYtoIndex(j + 1, i)];
                        HashSet<int> validOptions = new HashSet<int>();
                        foreach (var option in right.options)
                        {
                            List<int> valid = tiles[option].left;
                            validOptions = validOptions.Concat(valid).ToHashSet();
                        }
                        CheckValid(options, validOptions);
                    }

                    //Look down
                    if (i - 1 >= 0)
                    {
                        Cell down = grid[MapXYtoIndex(j, i - 1)];
                        HashSet<int> validOptions = new HashSet<int>();
                        foreach (var option in down.options)
                        {
                            List<int> valid = tiles[option].up;
                            validOptions = validOptions.Concat(valid).ToHashSet();
                        }
                        CheckValid(options, validOptions);
                    }

                    //Look left
                    if (j - 1 >= 0)
                    {
                        Cell left = grid[MapXYtoIndex(j - 1, i)];
                        HashSet<int> validOptions = new HashSet<int>();
                        foreach (var option in left.options)
                        {
                            List<int> valid = tiles[option].right;
                            validOptions = validOptions.Concat(valid).ToHashSet();
                        }
                        CheckValid(options, validOptions);
                    }

                    Cell nextCell = new Cell(false, index,tiles.Count);
                    nextCell.SetOptions(options.ToArray());
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

    private void CheckValid(List<int> options, HashSet<int> valid)
    {
        for (int i = options.Count-1; i >= 0; i--)
        {
            if(!valid.Contains(options[i]))
            {
                options.RemoveAt(i);
            }
        }
    }
}

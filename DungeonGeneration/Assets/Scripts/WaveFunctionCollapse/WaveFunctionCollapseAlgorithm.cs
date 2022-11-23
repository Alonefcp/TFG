using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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

    [SerializeField] private string path;
    [SerializeField] private int width = 5, height = 5;
    [SerializeField] private bool useRandomSeed = true;
    [SerializeField] private string seed;
    private List<Cell> grid;
    private Texture2D[] images;
    private System.Random rng = null;
    private List<WFCTile> tiles;

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
    //private void Update()
    //{
    //    if (Input.GetMouseButtonDown(0))
    //    {
    //        RunWFC(out bool restart);
    //        if (restart) tilemapVisualizer.ClearTilemap();
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
            Cell newCell = new Cell(true, cell.index, tiles.Count);
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
                    int index = cell.options[0];
                    tilemapVisualizer.PaintSingleTile(tiles[index].image, new Vector2Int(col, row));
                }
            }
        }
    }
    

    private Texture2D LoadImage(string path)
    {
        byte[] bytes = File.ReadAllBytes(path);
        Texture2D texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.LoadImage(bytes);
        
        return texture;
    }

    private void CreateTilesFromPath()
    {
        string[] imagePaths = Directory.GetFiles(Application.dataPath + path, "*.png");
        images = new Texture2D[imagePaths.Length];
        for (int i = 0; i < imagePaths.Length; i++)
        {
            Texture2D texture = LoadImage(imagePaths[i]);           
            images[i] = texture;
        }       
    }

    private void SetUp()
    {       
        CreateTilesFromPath();

        tiles = new List<WFCTile>();
        #region Room tiles
        //Room tiles
        //tiles.Add(new WFCTile(images[0], new List<int> { 0, 0, 0, 0 }));
        //tiles.Add(new WFCTile(images[1], new List<int> { 1, 1, 1, 1 }));
        ////tiles.Add(new WFCTile(images[2], new List<int> { 2, 2, 0, 0 }));
        //tiles.Add(new WFCTile(images[3], new List<int> { 0, 1, 0, 1 }));
        ////tiles.Add(new WFCTile(images[4], new List<int> { 0, 2, 2, 2 }));
        ////tiles.Add(new WFCTile(images[5], new List<int> { 1, 0, 0, 0 }));
        //tiles.Add(new WFCTile(images[6], new List<int> { 1, 0, 0, 0 }));
        ////tiles.Add(new WFCTile(images[7], new List<int> { 2, 2, 1, 1 }));

        //for (int j = 2; j < 11; j++)
        //{
        //    Texture2D baseTexture = tiles[j].image.sprite.texture;
        //    for (int i = 1; i < 4; i++)
        //    {
        //        WFCTile wFCTile = tiles[j].RotateTile(baseTexture, i);
        //        tiles.Add(wFCTile);

        //        baseTexture = wFCTile.image.sprite.texture;
        //    }
        //}
        #endregion
        //circuit tiles
        tiles.Add(new WFCTile(images[0], new List<int> { 0, 0, 0, 0 }));
        tiles.Add(new WFCTile(images[1], new List<int> { 1, 1, 1, 1 }));
        tiles.Add(new WFCTile(images[2], new List<int> { 1, 2, 1, 1 }));
        tiles.Add(new WFCTile(images[3], new List<int> { 1, 3, 1, 3 }));
        tiles.Add(new WFCTile(images[4], new List<int> { 1, 2, 1, 2 }));
        tiles.Add(new WFCTile(images[5], new List<int> { 3, 2, 3, 2 }));
        tiles.Add(new WFCTile(images[6], new List<int> { 3, 1, 2, 1 }));
        tiles.Add(new WFCTile(images[7], new List<int> { 2, 2, 1, 2 }));
        tiles.Add(new WFCTile(images[8], new List<int> { 2, 2, 2, 2 }));
        tiles.Add(new WFCTile(images[9], new List<int> { 2, 2, 1, 1 }));
        tiles.Add(new WFCTile(images[10], new List<int> { 1, 2, 1, 2 }));

        for (int j = 2; j < 11; j++)
        {
            Texture2D baseTexture = tiles[j].image.sprite.texture;
            for (int i = 1; i < 4; i++)
            {
                WFCTile wFCTile = tiles[j].RotateTile(baseTexture, i);
                tiles.Add(wFCTile);

                baseTexture = wFCTile.image.sprite.texture;
            }
        }

        //int pos = 0;
        //foreach (WFCTile tile in tiles)
        //{
        //    tilemapVisualizer.PaintSingleTile(tile.image, new Vector2Int(pos, 0));
        //    pos++;
        //}
        #region Example tiles
        //example tiles
        //tiles.Add(new WFCTile(images[0], new List<int> { 0, 0, 0, 0 }));
        //tiles.Add(new WFCTile(images[1], new List<int> { 1, 1, 0, 1 }));
        //Texture2D baseTexture = tiles[1].image.sprite.texture;
        //for (int i = 1; i < 4; i++)
        //{
        //    WFCTile wFCTile = tiles[1].RotateTile(baseTexture, i);
        //    tiles.Add(wFCTile);

        //    baseTexture = wFCTile.image.sprite.texture;
        //}
        #endregion

        for (int i = 0; i < tiles.Count; i++)
        {
            tiles[i].SetNeighbours(tiles);
        }

        grid = new List<Cell>();
        for (int i = 0; i < width*height; i++)
        {
            grid.Add(new Cell(false,i,tiles.Count));
        }

        //for (int row = 0; row < height; row++)
        //{
        //    for (int col = 0; col < width; col++)
        //    {              
        //        tilemapVisualizer.PaintSingleFloorTile(new Vector2Int(col, row));               
        //    }
        //}
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

        Cell randomCell = gridCopy[0]/*gridCopy[rng.Next(0, gridCopy.Count)]*/;
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
                    int index = cell.options[0];
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
                    List<int> options = new List<int>();
                    for (int k = 0; k < tiles.Count; k++)options.Add(k);


                    //Look up
                    if (i + 1 < height)
                    {
                        Cell up = grid[MapXYtoIndex(j, i + 1)];
                        HashSet<int> validOptions = new HashSet<int>();
                        foreach (int option in up.options)
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
                        foreach (int option in right.options)
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
                        foreach (int option in down.options)
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
                        foreach (int option in left.options)
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

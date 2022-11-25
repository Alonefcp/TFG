using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class WaveFunctionCollapseAlgorithm : DungeonGenerator 
{
    [Serializable]
    struct TileInfo
    {
        public int nRotations;
        public List<string> edges;
    }

    [SerializeField] private string path;
    [SerializeField] private int width = 5, height = 5;
    [SerializeField] private bool useRandomSeed = true;
    [SerializeField] private string seed;
    [SerializeField] private TileInfo[] tileInfos;
    private List<WFCCell> grid;
    private Texture2D[] images;
    private System.Random rng = null;
    private List<WFCTile> tiles;
    private List<int> options;
    private bool firstTime = true;
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
            if (restart) 
            { 
                tilemapVisualizer.ClearTilemap(); 
                i = -1; 
            }
        }

        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                WFCCell cell = grid[col + row * width];
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

        //Create tiles
        for (int i = 0; i < tileInfos.Length; i++)
        {
            tiles.Add(new WFCTile(images[i], tileInfos[i].edges));
        }

        //Create rotations
        for (int j = 0; j < tileInfos.Length; j++)
        {
            if (tileInfos[j].nRotations <= 0) continue;

            Texture2D baseTexture = tiles[j].image.sprite.texture;
            for (int i = 1; i <= tileInfos[j].nRotations; i++)
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
 
        options = new List<int>();
        for (int i = 0; i < tiles.Count; i++)
        { 
            options.Add(i);
            tiles[i].SetNeighbours(tiles);
        }

        grid = new List<WFCCell>();
        for (int i = 0; i < width*height; i++)
        {
            grid.Add(new WFCCell(false,i,tiles.Count));
        }
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

        WFCCell randomCell = gridCopy[0];
        //if (firstTime)
        //{
        //   randomCell = gridCopy[0];
        //   firstTime = false;
        //}
        //else
        //{
        //    randomCell = gridCopy[rng.Next(0, gridCopy.Count)];
        //}
      
        if (randomCell.options.Count<=0)
        {
            Debug.Log("Restart");
            grid = new List<WFCCell>();

            for (int i = 0; i < width * height; i++)
            {
                grid.Add(new WFCCell(false,i,tiles.Count));
            }
            restart = true;
            return;
        }

        randomCell.collapsed = true;
        int randomCellOp = randomCell.options[rng.Next(0,randomCell.options.Count)];
        randomCell.SetOptions(new List<int>() { randomCellOp });
        grid[randomCell.index] = randomCell;


        List<WFCCell> nextGrid = new List<WFCCell>();
        for (int i = 0; i < width * height; i++)
        {
            nextGrid.Add(new WFCCell(false,i,tiles.Count));
        }

        List<WFCCell> collapsedCells = grid.Where(cell => cell.collapsed).ToList();
        List<WFCCell> collapsedCellsNeighbours = new List<WFCCell>();

        for (int i = 0; i < collapsedCells.Count; i++)
        {
            int x = i % width;
            int y = i / width;
                
            //Up
            if(y + 1 < height)
            {
                int index = MapXYtoIndex(x, y+1);
                if (!grid[index].collapsed /*&& !collapsedCells.Contains(grid[index])*/)
                {
                    collapsedCellsNeighbours.Add(grid[index]);
                }              
            }

            //Right
            if(x+1<width)
            {
                int index = MapXYtoIndex(x+1, y);
                if (!grid[index].collapsed /*&& !collapsedCells.Contains(grid[index])*/)
                {
                    collapsedCellsNeighbours.Add(grid[index]);
                }
            }

            //Down
            if(y-1>=0)
            {
                int index = MapXYtoIndex(x, y-1);
                if (!grid[index].collapsed /*&& !collapsedCells.Contains(grid[index])*/)
                {
                    collapsedCellsNeighbours.Add(grid[index]);
                }
            }

            //Left
            if(i-1>=0)
            {
                int index = MapXYtoIndex(x-1, y);
                if (!grid[index].collapsed /*&& !collapsedCells.Contains(grid[index])*/)
                {
                    collapsedCellsNeighbours.Add(grid[index]);
                }
            }

        }

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                int index = MapXYtoIndex(j, i);

                if(grid[index].collapsed || !collapsedCellsNeighbours.Contains(grid[index]))
                {
                    nextGrid[index] = grid[index];
                }
                else
                {
                    List<int> availableOptions = new List<int>(options);

                    //Look up
                    if (i + 1 < height)
                    {
                        WFCCell up = grid[MapXYtoIndex(j, i + 1)];
                        HashSet<int> validOptions = new HashSet<int>();
                        foreach (int option in up.options)
                        {
                            List<int> valid = tiles[option].down;
                            validOptions = validOptions.Concat(valid).ToHashSet();
                        }

                        CheckValid(availableOptions, validOptions);
                    }

                    //Look right
                    if (j + 1 < width)
                    {
                        WFCCell right = grid[MapXYtoIndex(j + 1, i)];
                        HashSet<int> validOptions = new HashSet<int>();
                        foreach (int option in right.options)
                        {
                            List<int> valid = tiles[option].left;
                            validOptions = validOptions.Concat(valid).ToHashSet();
                        }
                        CheckValid(availableOptions, validOptions);
                    }

                    //Look down
                    if (i - 1 >= 0)
                    {
                        WFCCell down = grid[MapXYtoIndex(j, i - 1)];
                        HashSet<int> validOptions = new HashSet<int>();
                        foreach (int option in down.options)
                        {
                            List<int> valid = tiles[option].up;
                            validOptions = validOptions.Concat(valid).ToHashSet();
                        }
                        CheckValid(availableOptions, validOptions);
                    }

                    //Look left
                    if (j - 1 >= 0)
                    {
                        WFCCell left = grid[MapXYtoIndex(j - 1, i)];
                        HashSet<int> validOptions = new HashSet<int>();
                        foreach (int option in left.options)
                        {
                            List<int> valid = tiles[option].right;
                            validOptions = validOptions.Concat(valid).ToHashSet();
                        }
                        CheckValid(availableOptions, validOptions);
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

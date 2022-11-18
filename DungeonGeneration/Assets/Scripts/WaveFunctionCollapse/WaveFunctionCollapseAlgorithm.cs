using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WaveFunctionCollapseAlgorithm : DungeonGenerator 
{
    enum Options { BLANK=0,UP=1,RIGHT=2, DOWN=3, LEFT=4}
    struct Cell 
    {
        public bool collapsed;
        public Options[] options;
        public int index;

        public Cell(bool _collapsed, Options[] op, int _index)
        {
            collapsed = _collapsed;
            options = op;
            index = _index;
        }

        public void SetOptions(Options[] op)
        {
            options = op;
        }

        public void SetCollapsed(bool b)
        {
            collapsed = b;
        }
    }

    [SerializeField] private Tile[] tiles;
    [SerializeField] private int width = 5, height = 5;
    private List<Cell> grid;
    private Dictionary<Options, List<List<Options>>> rules;

    private void Start()
    {
        tilemapVisualizer.ClearTilemap();

        SetUp();
    }

    public override void GenerateDungeon()
    {
        //tilemapVisualizer.ClearTilemap();

        //SetUp();
        //RunWFC();
    }

    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            RunWFC();
        }
    }

    private void SetUp()
    {
        grid = new List<Cell>();

        for (int i = 0; i < width*height; i++)
        {
            grid.Add(new Cell(false, new Options[] { Options.BLANK, Options.UP, Options.RIGHT, Options.DOWN, Options.LEFT },i));
        }

        rules = new Dictionary<Options, List<List<Options>>>();

        List<List<Options>> blankOptions = new List<List<Options>>();
        blankOptions.Add(new List<Options>() { Options.BLANK, Options.UP });        //Up  0
        blankOptions.Add(new List<Options>() { Options.BLANK, Options.RIGHT });     //Right  1
        blankOptions.Add(new List<Options>() { Options.BLANK, Options.DOWN});       //Down  2
        blankOptions.Add(new List<Options>() { Options.BLANK, Options.LEFT });      //Left  3
        rules.Add(Options.BLANK, blankOptions);//BLANK

        List<List<Options>> upOptions = new List<List<Options>>();
        upOptions.Add(new List<Options>() { Options.RIGHT, Options.LEFT,Options.DOWN });
        upOptions.Add(new List<Options>() { Options.LEFT, Options.UP,Options.DOWN });
        upOptions.Add(new List<Options>() { Options.BLANK, Options.DOWN});
        upOptions.Add(new List<Options>() { Options.RIGHT, Options.UP,Options.DOWN });
        rules.Add(Options.UP,upOptions); //UP

        List<List<Options>> rightOptions = new List<List<Options>>();
        rightOptions.Add(new List<Options>() { Options.RIGHT, Options.LEFT, Options.DOWN });
        rightOptions.Add(new List<Options>() { Options.LEFT, Options.UP, Options.DOWN });
        rightOptions.Add(new List<Options>() { Options.RIGHT, Options.LEFT, Options.UP });
        rightOptions.Add(new List<Options>() { Options.BLANK, Options.LEFT });
        rules.Add(Options.RIGHT, rightOptions); //RIGHT

        List<List<Options>> downOptions = new List<List<Options>>();
        downOptions.Add(new List<Options>() { Options.BLANK, Options.UP });
        downOptions.Add(new List<Options>() { Options.LEFT, Options.UP, Options.DOWN });
        downOptions.Add(new List<Options>() { Options.RIGHT, Options.LEFT, Options.UP });
        downOptions.Add(new List<Options>() { Options.RIGHT, Options.UP, Options.DOWN });
        rules.Add(Options.DOWN, downOptions); //DOWN

        List<List<Options>> leftOptions = new List<List<Options>>();
        leftOptions.Add(new List<Options>() { Options.RIGHT, Options.LEFT,Options.DOWN });
        leftOptions.Add(new List<Options>() { Options.BLANK, Options.RIGHT});
        leftOptions.Add(new List<Options>() { Options.RIGHT, Options.LEFT, Options.UP });
        leftOptions.Add(new List<Options>() { Options.UP, Options.DOWN, Options.LEFT });
        rules.Add(Options.LEFT, leftOptions); //LEFT

        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
               
                tilemapVisualizer.PaintSingleFloorTile(new Vector2Int(col, row));
                
            }
        }
    }

    private int MapXYtoIndex(int x, int y)
    {
        return x + (y * width);
    }

    private void RunWFC()
    {
        List<Cell> gridCopy = new List<Cell>(grid);

        gridCopy.RemoveAll(c => c.collapsed);
        if (gridCopy.Count == 0) return;

        gridCopy.Sort((s1, s2) => s1.options.Length.CompareTo(s2.options.Length));

        int len = gridCopy[0].options.Length;
        int stopIndex = 0;
        for (int i = 1; i < gridCopy.Count; i++)
        {
            if(gridCopy[i].options.Length >len)
            {
                stopIndex = i;
                break;
            }
        }
        if(stopIndex>0)
        {
            for (int i = stopIndex; i < gridCopy.Count; i++)
            {
                
                gridCopy.Remove(gridCopy[i]);
                
            }
        }

        Cell randomCell = gridCopy[0];
        randomCell.collapsed = true;
        Options randomCellOp = randomCell.options[UnityEngine.Random.Range(0, randomCell.options.Length)];
        randomCell.SetOptions(new Options[] { randomCellOp });

        grid[randomCell.index] = randomCell;

        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                Cell cell = grid[col + row * width];
                if(cell.collapsed)
                {
                    int index = (int)cell.options[0];
                    tilemapVisualizer.PaintSingleTile(tiles[index], new Vector2Int(col, row));
                }
                //else
                //{
                //    tilemapVisualizer.PaintSingleFloorTile(new Vector2Int(col, row));
                //}
            }
        }

        List<Cell> nextGrid = new List<Cell>();
        for (int i = 0; i < width * height; i++)
        {
            nextGrid.Add(new Cell(false, new Options[] { Options.BLANK, Options.UP, Options.RIGHT, Options.DOWN, Options.LEFT },i));
        }


        for (int i = 0; i < height; i++) //i
        {
            for (int j = 0; j < width; j++) //j
            {
                int index = MapXYtoIndex(j, i);//j + i * width;

                if (grid[index].collapsed)
                {
                    nextGrid[index] = grid[index];
                }
                else
                {
                    List<Options> o = new List<Options> { Options.BLANK, Options.UP, Options.RIGHT, Options.DOWN, Options.LEFT };

                    //Look up
                    if (i+1 < height)
                    {
                        int a = MapXYtoIndex(j, i + 1);
                        Cell up = grid[a];
                        HashSet<Options> validOptions = new HashSet<Options>();
                        foreach (var option in up.options)
                        {
                            List<Options> valid = rules[option][2];
                            validOptions = validOptions.Concat(valid).ToHashSet();
                        }

                        checkValid(o, validOptions);
                    }

                    //Look right
                    if (j+1 < width)
                    {
                        int a = MapXYtoIndex(j+1, i);
                        Cell right = grid[a];
                        HashSet<Options> validOptions = new HashSet<Options>();
                        foreach (var option in right.options)
                        {
                            List<Options> valid = rules[option][3];
                            validOptions = validOptions.Concat(valid).ToHashSet();
                        }
                        checkValid(o, validOptions);
                    }

                    //Look down
                    if (i -1>=0)
                    {
                        int a = MapXYtoIndex(j, i-1);
                        Cell down = grid[a];
                        HashSet<Options> validOptions = new HashSet<Options>();
                        foreach (var option in down.options)
                        {
                            List<Options> valid = rules[option][0];
                            validOptions = validOptions.Concat(valid).ToHashSet();
                        }
                        checkValid(o, validOptions);
                    }

                    //Look left
                    if (j - 1>= 0)
                    {
                        int a = MapXYtoIndex(j - 1, i);
                        Cell left = grid[a];
                        HashSet<Options> validOptions = new HashSet<Options>();
                        foreach (var option in left.options)
                        {
                            List<Options> valid = rules[option][1];
                            validOptions = validOptions.Concat(valid).ToHashSet();
                        }
                        checkValid(o, validOptions);
                    }

                    if(o.Count==0)
                    {
                        Debug.Log("cac");
                    }

                    Cell nextCell = new Cell(false,o.ToArray(),index);
                    nextGrid[index] = nextCell;
                }
            }
        }

        grid = nextGrid;

    }

    private void checkValid(List<Options> o, HashSet<Options> valid)
    {
        for (int i = o.Count-1; i >= 0; i--)
        {
            if(!valid.Contains(o[i]))
            {
                o.RemoveAt(i);
            }
        }
    }

    private List<Cell> Splice(List<Cell> source, int start, int size)
    {
        var items = source.Skip(start).Take(size).ToList<Cell>();
        if (source.Count >= size)
            source.RemoveRange(start, size);
        else
            source.Clear();
        return items;
    }
}

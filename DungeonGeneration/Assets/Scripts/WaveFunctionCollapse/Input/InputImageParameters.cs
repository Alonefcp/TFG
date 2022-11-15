using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class InputImageParameters
{
    private Vector2Int? bottomRightTileCoords = null;
    private Vector2Int? topLeftTileCoords = null;

    private BoundsInt inputTileMapBounds;

    private TileBase[] inputTilemapTilesArray;
    private Queue<TileContainer> stackOfTiles = new Queue<TileContainer>();
    private int width = 0, height = 0;
    private Tilemap input;

    public InputImageParameters(Tilemap input)
    {
        this.input = input;
        this.inputTileMapBounds = this.input.cellBounds;
        this.inputTilemapTilesArray = this.input.GetTilesBlock(this.inputTileMapBounds);
        ExtractNonEmptyTiles();
        VeryfyInputTiles();
    }

    private void VeryfyInputTiles()
    {
        throw new NotImplementedException();
    }

    private void ExtractNonEmptyTiles()
    {
        for (int row = 0; row < input.size.y; row++)
        {
            for (int col = 0; col < input.size.x; col++)
            {
                int index = col + (row * inputTileMapBounds.size.x);
                TileBase tile = inputTilemapTilesArray[index];

                if(bottomRightTileCoords == null && tile!=null)
                {
                    bottomRightTileCoords = new Vector2Int(col, row);
                }

                if(tile!=null)
                {
                    stackOfTiles.Enqueue(new TileContainer(tile, col, row));
                    topLeftTileCoords = new Vector2Int(col, row);
                }
            }
        }
    }

    public Queue<TileContainer> StackOfTiles { get => stackOfTiles; set => stackOfTiles = value; }

    public int Width { get => width;}
    public int Height { get => height;}

}
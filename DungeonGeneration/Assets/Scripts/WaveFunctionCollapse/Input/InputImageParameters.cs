using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class InputImageParameters
{
    private Vector2Int? bottomRightTileCoords = null;
    private Vector2Int? topLeftTileCoords = null;
    private BoundsInt inputTileMapBounds;
    private TileBase[] inputTilemapTilesArray;
    private Queue<TileContainer> stackOftiles = new Queue<TileContainer>();
    private int width = 0, height = 0;
    private Tilemap inputTilemap;

    public Queue<TileContainer> StackOfTiles { get => stackOftiles; set => stackOftiles = value; }
    public int Height { get => height; }
    public int Width { get => width; }


    public InputImageParameters(Tilemap inputTilemap)
    {
        this.inputTilemap = inputTilemap;
        inputTileMapBounds = this.inputTilemap.cellBounds;
        inputTilemapTilesArray = this.inputTilemap.GetTilesBlock(inputTileMapBounds);
        ExtractNonEmptyTiles();
        VeryfyInputTiles();
    }

    private void VeryfyInputTiles()
    {
        if (topLeftTileCoords == null || bottomRightTileCoords == null)
        {
            Debug.Log("Input tilemap is empty");
        }
        int minX = bottomRightTileCoords.Value.x;
        int maxX = topLeftTileCoords.Value.x;
        int minY = bottomRightTileCoords.Value.y;
        int maxY = topLeftTileCoords.Value.y;

        width = Math.Abs(maxX - minX) + 1;
        height = Math.Abs(maxY - minY) + 1;

        int tileCount = width * height;
        if (stackOftiles.Count != tileCount)
        {
            Debug.Log("Tilemap has empty fields");
        }
        if (stackOftiles.Any(tile => tile.X > maxX || tile.X < minX || tile.Y > maxY || tile.Y < minY))
        {
            Debug.Log("Tilemap image should be a filled rectangle");
        }
    }

    private void ExtractNonEmptyTiles()
    {
        for (int row = 0; row < inputTileMapBounds.size.y; row++)
        {
            for (int col = 0; col < inputTileMapBounds.size.x; col++)
            {
                int index = col + (row * inputTileMapBounds.size.x);

                TileBase tile = inputTilemapTilesArray[index];
                if (bottomRightTileCoords == null && tile != null)
                {
                    bottomRightTileCoords = new Vector2Int(col, row);
                }
                if (tile != null)
                {
                    stackOftiles.Enqueue(new TileContainer(tile, col, row));
                    topLeftTileCoords = new Vector2Int(col, row);
                }
            }
        }
    }
}
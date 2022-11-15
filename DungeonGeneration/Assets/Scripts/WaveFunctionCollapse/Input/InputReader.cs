using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class InputReader : IInputReader<TileBase>
{
    private Tilemap input;

    public InputReader(Tilemap _input)
    {
        input = _input;
    }


    public IValue<TileBase>[][] ReadInputToGrid()
    {
        var grid = ReadInputTilemap();
        TileBaseValue[][] gridOfValues = null;
        if(grid != null)
        {
            gridOfValues = MyCollectionExtension.CreateJaggedArray<TileBaseValue[][]>(grid.Length, grid[0].Length);
            for (int row = 0; row < grid.Length; row++)
            {
                for (int col = 0; col < grid[0].Length; col++)
                {
                    gridOfValues[row][col] = new TileBaseValue(grid[row][col]);
                }
            }
        }

        return gridOfValues;
    }

    private TileBase[][] ReadInputTilemap()
    {
        InputImageParameters imageParameters = new InputImageParameters(input);
        return CreateTileBasedGrid(imageParameters);
    }

    private TileBase[][] CreateTileBasedGrid(InputImageParameters imageParameters)
    {
        throw new NotImplementedException();
    }
}

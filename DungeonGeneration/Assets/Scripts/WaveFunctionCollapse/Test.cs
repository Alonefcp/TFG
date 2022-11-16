using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Test : MonoBehaviour
{
    [SerializeField] private Tilemap inputTilemap;


    private void Start()
    {
        InputReader inputReader = new InputReader(inputTilemap);
        var grid = inputReader.ReadInputToGrid();

        //for (int row = 0; row < grid.Length; row++)
        //{
        //    for (int col = 0; col < grid[0].Length; col++)
        //    {
        //        Debug.Log("Row: " + row + " Col: " + col + " Tile name: " + grid[row][col].value.name);
        //    }
        //}

        ValuesManager<TileBase> valuesManager = new ValuesManager<TileBase>(grid);
        List<string> list = new List<string>();
        for (int row = -1; row <= grid.Length; row++)
        {
            StringBuilder builder = new StringBuilder();
            for (int col = -1; col <= grid[0].Length; col++)
            {
                builder.Append(valuesManager.GetGridValuesIncludingOffset(col, row) + " ");
            }

            list.Add(builder.ToString());
        }
        list.Reverse();

        foreach (var item in list)
        {
            Debug.Log(item);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Test : MonoBehaviour
{
    [SerializeField] private Tilemap inputTilemap;


    private void Start()
    {
        InputReader inputReader = new InputReader(inputTilemap);
        var grid = inputReader.ReadInputToGrid();

        for (int row = 0; row < grid.Length; row++)
        {
            for (int col = 0; col < grid[0].Length; col++)
            {
                Debug.Log("Row: " + row + " Col: " + col + " Tile name: " + grid[row][col].value.name);
            }
        }
    }
}

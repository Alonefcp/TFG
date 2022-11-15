using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileContainer 
{
    public TileBase Tile { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public TileContainer(TileBase tile, int x, int y)
    {
        Tile = tile;
        X = x;
        Y = y;
    }
}

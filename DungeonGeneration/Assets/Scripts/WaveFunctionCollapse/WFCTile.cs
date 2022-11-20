using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WFCTile
{
    public Tile image;      // 0    1     2     3
    public List<int> edges; //{up, right, down, left}
    public List<int> up; 
    public List<int> down; 
    public List<int> left; 
    public List<int> right; 

    public WFCTile(Tile img, List<int> edges)
    {
        image = img;
        this.edges = edges;
        up = new List<int>();
        down = new List<int>();
        left = new List<int>();
        right = new List<int>();
    }

    public void Analyze(List<WFCTile> tiles)
    {
        for (int i = 0; i < tiles.Count; i++)
        {
            WFCTile tile = tiles[i];

            if(tile.edges[2] == edges[0])
            {
                up.Add(i);
            }

            if (tile.edges[3] == edges[1])
            {
                right.Add(i);
            }

            if (tile.edges[0] == edges[2])
            {
                down.Add(i);
            }

            if (tile.edges[1] == edges[3])
            {
                left.Add(i);
            }
        }
    }
}

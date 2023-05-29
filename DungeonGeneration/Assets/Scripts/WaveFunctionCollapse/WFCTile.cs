using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WFCTile
{
    public Tile Tile { get; }
    public List<int> Up { get; }    //Which tiles can place in the upper edge
    public List<int> Down { get; }  //Which tiles can place in the bottom edge
    public List<int> Left { get; }  //Which tiles can place in the left edge
    public List<int> Right { get; } //Which tiles can place in the right edge

    //This indicates which tile's edge can match with other tile's edge
    private List<string> edges; //{up(0), right(1), down(2), left(3)}

    public WFCTile(Tile tile, List<string> edges)
    {
        Tile = tile;    
        this.edges = edges;
        Up = new List<int>();
        Down = new List<int>();
        Left = new List<int>();
        Right = new List<int>();
    }

    /// <summary>
    /// Sets the neighbours is each tile's edge (up,right,down,left).
    /// </summary>
    /// <param name="tiles">All tiles</param>
    public void SetNeighbours(List<WFCTile> tiles)
    {
        for (int i = 0; i < tiles.Count; i++)
        {
            WFCTile tile = tiles[i];

            //UP
            if(CompareEdges(tile.edges[2],edges[0]))
            {
                Up.Add(i);
            }

            //RIGHT
            if (CompareEdges(tile.edges[3],edges[1]))
            {
                Right.Add(i);
            }

            //DOWN
            if (CompareEdges(tile.edges[0], edges[2]))
            {
                Down.Add(i);
            }

            //LEFT
            if (CompareEdges(tile.edges[1], edges[3]))
            {
                Left.Add(i);
            }
        }
    }

    /// <summary>
    /// Rotates the tile (texture and edges).
    /// </summary>
    /// <param name="nRotations">Number of rotations</param>
    /// <returns></returns>
    public WFCTile RotateTile(int nRotations)
    {
        if (nRotations == 0) return this; //We return the same tile, no need for rotating it
        
        //We rotate the edges
        List<string> newEdges = new List<string>();

        int len = edges.Count;
        for (int i = 0; i < len; i++)
        {
            newEdges.Add(edges[(i - nRotations + len) % len]);
        }

        //We return the new tile
        Tile tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = Tile.sprite;
        tile.colliderType = Tile.colliderType;
        Matrix4x4 newTrasform = Tile.transform;
        newTrasform.SetTRS(Vector3.zero, GetRotation(nRotations), Vector3.one);
        tile.transform = newTrasform;

        return new WFCTile(tile, newEdges);
    }

    private Quaternion GetRotation(int mask)
    {
        switch (mask)
        {            
            case 1:
                return Quaternion.Euler(0f, 0f, -90f);
            case 2:
                return Quaternion.Euler(0f, 0f, -180f);
            case 3:
                return Quaternion.Euler(0f, 0f, -270f);
            default:
                return Quaternion.Euler(0f, 0f, 0f);
        }
        
    }

    /// <summary>
    /// Compare two edges. It returns true if the edges are equal otherwise it return false.
    /// </summary>
    /// <param name="a">First edge to compare</param>
    /// <param name="b">Second edge to compare</param>
    /// <returns></returns>
    private bool CompareEdges(string a, string b)
    {
        return a == Reverse(b);
    }

    /// <summary>
    /// Reverses a string.
    /// </summary>
    /// <param name="s">String we are going to reverse</param>
    /// <returns></returns>
    private string Reverse(string s)
    {
        char[] charArray = s.ToCharArray();
        Array.Reverse(charArray);
        return new string(charArray);
    }
}

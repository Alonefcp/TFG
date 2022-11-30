using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WFCTile
{
    public Tile Tile { get; }
    public List<int> Up { get; }    //Which tiles can hace int the upper edge
    public List<int> Down { get; }  //Which tiles can hace int the bottom edge
    public List<int> Left { get; }  //Which tiles can hace int the left edge
    public List<int> Right { get; } //Which tiles can hace int the right edge

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
    /// <param name="texture">Tile's texture we are going to rotate</param>
    /// <param name="nRotations">Number of rotations</param>
    /// <returns></returns>
    public WFCTile RotateTile(Texture2D texture, int nRotations)
    {
        if (nRotations == 0) return this;

        //We rotate te texture
        Texture2D newTexture = RotateTexture(texture);
        
        //We rotate the edges
        List<string> newEdges = new List<string>();

        int len = edges.Count;
        for (int i = 0; i < len; i++)
        {
            newEdges.Add(edges[(i - nRotations + len) % len]);
        }

        //We return the new tile
        Sprite sprite = Sprite.Create(newTexture, new Rect(0, 0, newTexture.width, newTexture.height), new Vector2(0.5f, 0.5f), this.Tile.sprite.pixelsPerUnit);
        Tile tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = sprite;

        return new WFCTile(tile, newEdges);
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

    /// <summary>
    /// Rotates the tile's texture.
    /// </summary>
    /// <param name="originalTexture">Texture we are going to rotate</param>
    /// <param name="clockwise">If we rotate the texture clockwise or not</param>
    /// <returns></returns>
    private Texture2D RotateTexture(Texture2D originalTexture, bool clockwise = true)
    {
        Color32[] original = originalTexture.GetPixels32();
        Color32[] rotated = new Color32[original.Length];
        int w = originalTexture.width;
        int h = originalTexture.height;

        int iRotated, iOriginal;

        for (int j = 0; j < h; ++j)
        {
            for (int i = 0; i < w; ++i)
            {
                iRotated = (i + 1) * h - j - 1;
                iOriginal = clockwise ? original.Length - 1 - (j * w + i) : j * w + i;
                rotated[iRotated] = original[iOriginal];
            }
        }

        Texture2D rotatedTexture = new Texture2D(h, w);
        rotatedTexture.SetPixels32(rotated);
        rotatedTexture.Apply();
        rotatedTexture.filterMode = FilterMode.Point;
        rotatedTexture.wrapMode = TextureWrapMode.Clamp;
        return rotatedTexture;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WFCTile
{
    public Tile tile;          
    public List<string> edges; //{up(0), right(1), down(2), left(3)}
    public List<int> up; 
    public List<int> down; 
    public List<int> left; 
    public List<int> right; 

    public WFCTile(Tile tile, List<string> edges)
    {
        this.tile = tile;    
        this.edges = edges;
        up = new List<int>();
        down = new List<int>();
        left = new List<int>();
        right = new List<int>();
    }

    public void SetNeighbours(List<WFCTile> tiles)
    {
        for (int i = 0; i < tiles.Count; i++)
        {
            WFCTile tile = tiles[i];

            //UP
            if(CompareEdges(tile.edges[2],edges[0]))
            {
                up.Add(i);
            }

            //RIGHT
            if (CompareEdges(tile.edges[3],edges[1]))
            {
                right.Add(i);
            }

            //DOWN
            if (CompareEdges(tile.edges[0], edges[2]))
            {
                down.Add(i);
            }

            //LEFT
            if (CompareEdges(tile.edges[1], edges[3]))
            {
                left.Add(i);
            }
        }
    }

    public WFCTile RotateTile(Texture2D texture, int nRotations)
    {
        if (nRotations == 0) return this;

        Texture2D newTexture = RotateTexture(texture);
        
        List<string> newEdges = new List<string>();

        int len = edges.Count;

        for (int i = 0; i < len; i++)
        {
            newEdges.Add(edges[(i - nRotations + len) % len]);
        }

        Sprite sprite = Sprite.Create(newTexture, new Rect(0, 0, newTexture.width, newTexture.height), new Vector2(0.5f, 0.5f), this.tile.sprite.pixelsPerUnit);
        Tile tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = sprite;

        return new WFCTile(tile, newEdges);
    }

    private bool CompareEdges(string a, string b)
    {
        return a == Reverse(b);
    }

    private string Reverse(string s)
    {
        char[] charArray = s.ToCharArray();
        Array.Reverse(charArray);
        return new string(charArray);
    }

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

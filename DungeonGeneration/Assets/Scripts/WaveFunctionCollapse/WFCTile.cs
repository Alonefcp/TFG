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

    public WFCTile(Texture2D img, List<int> edges)
    {
        Sprite sprite = Sprite.Create(img, new Rect(0, 0, img.width, img.height), new Vector2(0.5f, 0.5f), img.width);
        Tile tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = sprite;
        image = tile;

       
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

    public WFCTile RotateTile(Texture2D texture, int num)
    {
        Texture2D newTexture = RotateTexture(texture);
        
        List<int> newEdges = new List<int>();

        int len = edges.Count;

        for (int i = 0; i < len; i++)
        {
            newEdges.Add(edges[(i - num + len) % len]);
        }

        return new WFCTile(newTexture, newEdges);
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileEdges
{
    private Dictionary<Color, int> uniqueColors;
    private int colorCount;

    public TileEdges()
    {
        colorCount = -1;
        uniqueColors = new Dictionary<Color, int>();
    }

    /// <summary>
    /// Once we have looped through the texture's edge, we assign the edge of the last part of the texture's edge.
    /// </summary>
    /// <param name="initialColor">Initial pixel color</param>
    /// <param name="currentColor">Current pixel color</param>
    /// <param name="edge">Current tiles's edge</param>
    private void CheckLastEdgePart(Color initialColor, Color currentColor, ref string edge)
    {
        if (initialColor == currentColor)
        {
            if (!uniqueColors.ContainsKey(initialColor))
            {
                colorCount++;
                uniqueColors.Add(initialColor, colorCount);
                edge += uniqueColors[initialColor].ToString();
            }
            else
            {
                edge += uniqueColors[initialColor].ToString();
            }
        }
    }

    /// <summary>
    /// Creates the tiles' edge by checking all the colours of the tile's texture.
    /// </summary>
    /// <param name="texture">Tiles's texture</param>
    /// <param name="initialColor">Initial pixel color</param>
    /// <param name="currentColor">Current pixel color</param>
    /// <param name="edge">Current tiles's edge</param>
    /// <param name="pixelPosX">X pixel postion of the texture</param>
    /// <param name="pixelYpos">Y pixel postion of the texture</param>
    private void CheckPixelColorEdge(Texture2D texture, ref Color initialColor, ref Color currentColor, ref string edge, int pixelPosX, int pixelYpos)
    {
        currentColor = texture.GetPixel(pixelPosX, pixelYpos);
        if (initialColor != currentColor)
        {
            if (!uniqueColors.ContainsKey(initialColor))
            {
                colorCount++;
                uniqueColors.Add(initialColor, colorCount);
                edge += uniqueColors[initialColor].ToString();
            }
            else
            {
                edge += uniqueColors[initialColor].ToString();
            }
            initialColor = currentColor;
        }
    }

    /// <summary>
    /// Creates the tiles's edges which establishes with which tiles can match 
    /// </summary>
    /// <param name="texture">Tiles's texture</param>
    /// <returns></returns>
    public List<string> CreateTileEdges(Texture2D texture)
    {
        List<string> edges = new List<string>();

        string edge = "";
        //Up
        Color initialColor = texture.GetPixel(0, texture.height - 1);
        Color currentColor = new Color();
        int x = 0;
        while (x < texture.width)
        {
            CheckPixelColorEdge(texture, ref initialColor, ref currentColor, ref edge, x, texture.height - 1);
            x++;
        }

        CheckLastEdgePart(initialColor, currentColor, ref edge);

        edges.Add(edge);
        edge = "";

        //Right
        int y = texture.height - 1;
        initialColor = texture.GetPixel(texture.width - 1, texture.height - 1);
        currentColor = new Color();

        while (y >= 0)
        {
            CheckPixelColorEdge(texture, ref initialColor, ref currentColor, ref edge, texture.width - 1, y);
            y--;
        }

        CheckLastEdgePart(initialColor, currentColor, ref edge);

        edges.Add(edge);
        edge = "";

        //Down
        initialColor = texture.GetPixel(texture.width - 1, 0);
        currentColor = new Color();
        x = texture.width - 1;
        while (x >= 0)
        {
            CheckPixelColorEdge(texture, ref initialColor, ref currentColor, ref edge, x, 0);
            x--;
        }

        CheckLastEdgePart(initialColor, currentColor, ref edge);

        edges.Add(edge);
        edge = "";

        //Left
        y = 0;
        initialColor = texture.GetPixel(0, 0);
        currentColor = new Color();

        while (y < texture.height)
        {
            CheckPixelColorEdge(texture, ref initialColor, ref currentColor, ref edge, 0, y);
            y++;
        }

        CheckLastEdgePart(initialColor, currentColor, ref edge);

        edges.Add(edge);
        edge = "";

        return edges;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WFCCell
{
    public int GridIndex { get; }
    private bool collapsed; // If te cell is collapse it has chosen a tile
    private List<int> options; //Possible tiles this cell can have

    //Getter and setter of the collapse attribute
    public bool Collapsed { get => collapsed; set => collapsed = value; }

    //Getter and setter of the options attribute
    public List<int> Options { get => options; set => options = value; }

    public WFCCell(bool collapsed, int index, int numberOfTiles)
    {
        this.collapsed = collapsed;
        GridIndex = index;
        options = new List<int>();
        for (int i = 0; i < numberOfTiles; i++)
        {
            options.Add(i); //Each number represents a tile
        }
    }

    public WFCCell(bool collapsed, int index)
    {
        this.collapsed = collapsed;
        GridIndex = index;
        options = new List<int>();
    }  
}

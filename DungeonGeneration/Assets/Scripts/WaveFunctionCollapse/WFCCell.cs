using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WFCCell
{
    public bool collapsed;
    public List<int> options;
    public int gridIndex;

    public WFCCell(bool collapsed, int index, int num)
    {
        this.collapsed = collapsed;
        gridIndex = index;
        options = new List<int>();
        for (int i = 0; i < num; i++)
        {
            options.Add(i);
        }
    }

    public void SetOptions(List<int> newOptions)
    {
        options = newOptions;
    }

    public void SetCollapsed(bool isCollapsed)
    {
        collapsed = isCollapsed;
    }
    
}

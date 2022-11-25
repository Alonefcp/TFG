using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WFCCell
{
    public bool collapsed;
    public List<int> options;
    public int index;

    public WFCCell(bool _collapsed, int _index, int num)
    {
        collapsed = _collapsed;
        index = _index;
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

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFindNeighbourStrategy
{
    Dictionary<int, PatternNeighbours> FindNeighbours(PatternDataResults patternFinderResult);
}

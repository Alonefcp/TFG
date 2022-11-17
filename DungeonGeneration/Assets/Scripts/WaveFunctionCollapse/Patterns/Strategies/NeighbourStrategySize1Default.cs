using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeighbourStrategySize1Default : IFindNeighbourStrategy
{
    public Dictionary<int, PatternNeighbours> FindNeighbours(PatternDataResults patternFinderResult)
    {
        Dictionary<int, PatternNeighbours> result = new Dictionary<int, PatternNeighbours>();
        FindNeighboursForEachPattern(patternFinderResult,result);

        return result;
    }

    private void FindNeighboursForEachPattern(PatternDataResults patternFinderResult, Dictionary<int, PatternNeighbours> result)
    {
        for (int row = 0; row < patternFinderResult.GetGridLenghtY(); row++)
        {
            for (int col = 0; col < patternFinderResult.GetGridLenghtX(); col++)
            {
                PatternNeighbours neighbours = PatternFinder.CheckNeighboursInEachDirection(col,row,patternFinderResult);
                PatternFinder.AddNeighboursToDictionary(result, patternFinderResult.GetIndexAt(col, row),neighbours);
            }
        }
    }
}

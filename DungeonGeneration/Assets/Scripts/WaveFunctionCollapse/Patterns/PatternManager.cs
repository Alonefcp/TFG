using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class PatternManager
{
    private Dictionary<int, PatternData> patternDataIndexDictionary;
    private Dictionary<int, PatternNeighbours> patternPossibleNeighboursDictionary;
    private int patternSize = -1;
    private IFindNeighbourStrategy strategy;

    public PatternManager(int _patternSize)
    {
        patternSize = _patternSize;
    }

    public void ProcessGrid<T>(ValuesManager<T> valuesManager, bool equalWeights, string strategyName =null)
    {
        NeighbourStrategyFactory strategyFactory = new NeighbourStrategyFactory();
        strategy = strategyFactory.CreateInstance(strategyName == null ? patternSize + "" : strategyName);
        CreatePatterns(valuesManager, strategy, equalWeights);
    }

    private void CreatePatterns<T>(ValuesManager<T> valuesManager, IFindNeighbourStrategy strategy, bool equalWeights)
    {
        var patternFinderResult = PatternFinder.GetPatternDataFromGrid(valuesManager, patternSize, equalWeights);

        //====================================================== For testing ===================================================================================
        //List<string> list = new List<string>();
        //for (int row = 0; row < patternFinderResult.GetGridLenghtY(); row++)
        //{
        //    StringBuilder builder = new StringBuilder();
        //    for (int col = 0; col < patternFinderResult.GetGridLenghtX(); col++)
        //    {
        //        builder.Append(patternFinderResult.GetIndexAt(col,row) + " ");
        //    }

        //    list.Add(builder.ToString());
        //}
        //list.Reverse();

        //foreach (var item in list)
        //{
        //    Debug.Log(item);
        //}
        //=========================================================================================================================================

        patternDataIndexDictionary = patternFinderResult.PatternIndexDictionary;
        GetPatternNeighbours(patternFinderResult, strategy);
    }

    private void GetPatternNeighbours(PatternDataResults patternFinderResult, IFindNeighbourStrategy strategy)
    {
        patternPossibleNeighboursDictionary = PatternFinder.FindPossibleNeighboursForAllPatterns(strategy, patternFinderResult);
    }

    public PatternData GetPatternDataFromIndex(int index)
    {
        return patternDataIndexDictionary[index];
    }

    public HashSet<int> GetPossibleNeighboursForPatternInDirection(int patternIndex, Direction dir)
    {
        return patternPossibleNeighboursDictionary[patternIndex].GetNeighboursDirection(dir);
    }

    public float GetPatternFrequency(int index)
    {
        return GetPatternDataFromIndex(index).FrequencyRelative;
    }

    public float GetPatternFrequencyLog2(int index)
    {
        return GetPatternDataFromIndex(index).FrequencyRelativeLog2;
    }

    public int GetNumberOfPatterns()
    {
        return patternDataIndexDictionary.Count;
    }
}

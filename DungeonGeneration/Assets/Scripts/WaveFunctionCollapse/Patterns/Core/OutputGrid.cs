using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class OutputGrid
{
    private Dictionary<int, HashSet<int>> indexPossiblePatternDictionary = new Dictionary<int, HashSet<int>>();
    public int width { get; }
    public int height { get; }
    private int maxNumberOfPatterns = 0;

    public OutputGrid(int _width, int _height, int _maxNumberOfPatterns)
    {
        width = _width;
        height = _height;
        maxNumberOfPatterns = _maxNumberOfPatterns;
        ResetAllPossibilities();
    }

    private void ResetAllPossibilities()
    {
        HashSet<int> allPossiblePatternsList = new HashSet<int>();
        allPossiblePatternsList.UnionWith(Enumerable.Range(0, maxNumberOfPatterns).ToList());
        
        indexPossiblePatternDictionary.Clear();
        for (int i = 0; i < width*height; i++)
        {
            indexPossiblePatternDictionary.Add(i, new HashSet<int>(allPossiblePatternsList));

        }
    }

    public bool CheckCellExists(Vector2Int position)
    {
        int index = GetIndexFromCoordinate(position);
        return indexPossiblePatternDictionary.ContainsKey(index);
    }

    private int GetIndexFromCoordinate(Vector2Int position)
    {
        return position.x + width * position.y;
    }

    public bool CheckIfCellIsCollapsed(Vector2Int position)
    {
        return GetPossibleValueForPositions(position).Count <= 1;
    }

    public HashSet<int> GetPossibleValueForPositions(Vector2Int position)
    {
        int index = GetIndexFromCoordinate(position);
        if (indexPossiblePatternDictionary.ContainsKey(index))
        {
            return indexPossiblePatternDictionary[index];
        }
        else return new HashSet<int>();
    }

    public bool CheckIhGridIsSolved()
    {
        return !indexPossiblePatternDictionary.Any(x => x.Value.Count > 1);
    }

    public bool CheckIfValidPosition(Vector2Int position)
    {
        return MyCollectionExtension.ValidateCoordinates(position.x, position.y, width, height);
    }

    public Vector2Int GetRandomCell()
    {
        int randomIndex = UnityEngine.Random.Range(0, indexPossiblePatternDictionary.Count);
        return GetCoordsFromIndex(randomIndex);
    }

    private Vector2Int GetCoordsFromIndex(int randomIndex)
    {
        Vector2Int coord = Vector2Int.zero;
        coord.x = randomIndex / width;
        coord.y = randomIndex % height;
        return coord;
    }

    public void SetPatternOnPosition(int x, int y, int patternIndex)
    {
        int index = GetIndexFromCoordinate(new Vector2Int(x, y));
        indexPossiblePatternDictionary[index] = new HashSet<int>() { patternIndex };
    }

    public int[][] GetSolvedOutputGrid()
    {
        int[][] grid = MyCollectionExtension.CreateJaggedArray<int[][]>(height, width);
        if(CheckIhGridIsSolved()==false)
        {
            return MyCollectionExtension.CreateJaggedArray<int[][]>(0, 0);
        }
        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                int index = GetIndexFromCoordinate(new Vector2Int(col, row));
                grid[row][col] = indexPossiblePatternDictionary[index].First();
            }
        }

        return grid;
    }
}

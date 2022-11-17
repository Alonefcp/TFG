using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PropagationHelper 
{
    private OutputGrid outputgrid;
    private CoreHelper coreHelper;
    bool cellWithNoSolutionPresent;
    private SortedSet<LowEntropyCell> lowEntropySet = new SortedSet<LowEntropyCell>();
    private Queue<VectorPair> pairsToPropagate = new Queue<VectorPair>();

    public SortedSet<LowEntropyCell> LowEntropySet { get => lowEntropySet; }
    public Queue<VectorPair> PairsToPropagate { get => pairsToPropagate; }

    public PropagationHelper(OutputGrid _outputgrid, CoreHelper _coreHelper)
    {
        outputgrid = _outputgrid;
        coreHelper = _coreHelper;
    }

    public bool CheckIfPairShouldBeProcessed(VectorPair propagatePair) 
    {
        return outputgrid.CheckIfValidPosition(propagatePair.CellToPropagatePosition) && propagatePair.AreWeCheckingPreviousCellAgain()==false;
    }

    public void AnalyzePropagationResults(VectorPair propagatePair, int startCount, int newPossiblePatternCount)
    {
        if(newPossiblePatternCount>1 && startCount>newPossiblePatternCount)
        {
            AddNewPairsToPropagateQueue(propagatePair.CellToPropagatePosition, propagatePair.BaseCellPosition);
            AddToLowEntropySet(propagatePair.CellToPropagatePosition);

            if (newPossiblePatternCount == 0) cellWithNoSolutionPresent = true;
            if (newPossiblePatternCount == 1) cellWithNoSolutionPresent = coreHelper.CheckCellSolutionForCollision(propagatePair.CellToPropagatePosition, outputgrid);
        }
    }

    private void AddToLowEntropySet(Vector2Int cellToPropagatePosition)
    {
        var elementOfLowEntropySet = lowEntropySet.Where(x => x.Position == cellToPropagatePosition).FirstOrDefault();
        if (elementOfLowEntropySet == null && outputgrid.CheckIfCellIsCollapsed(cellToPropagatePosition) == false)
        {
            float entropy = coreHelper.CalculateEntropy(cellToPropagatePosition, outputgrid);
            lowEntropySet.Add(new LowEntropyCell(cellToPropagatePosition, entropy));
        }
        else
        {
            lowEntropySet.Remove(elementOfLowEntropySet);
            elementOfLowEntropySet.Entropy = coreHelper.CalculateEntropy(cellToPropagatePosition, outputgrid);
            lowEntropySet.Add(elementOfLowEntropySet);
        }
    }

    private void AddNewPairsToPropagateQueue(Vector2Int cellToPropagatePosition, Vector2Int baseCellPosition)
    {
        var list = coreHelper.Create4DirectionNeighbours(cellToPropagatePosition, baseCellPosition);
        foreach (var item in list)
        {
            pairsToPropagate.Enqueue(item);
        }
    }

    public bool CheckForConflicts()
    {
        return cellWithNoSolutionPresent;
    }

    public void SetConflictFlag()
    {
        cellWithNoSolutionPresent = true;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveFunctionCollapseAlgorithm //core solver of the algortihm
{
    private PatternManager patternManager;
    private OutputGrid outputgrid;
    private CoreHelper coreHelper;
    private PropagationHelper propagationHelper;

    public WaveFunctionCollapseAlgorithm(OutputGrid _outputGrid, PatternManager _patternManager)
    {
        outputgrid = _outputGrid;
        patternManager = _patternManager;

        coreHelper = new CoreHelper(patternManager);
        propagationHelper = new PropagationHelper(outputgrid, coreHelper);
    }

    public void Propagate()
    {
        while(propagationHelper.PairsToPropagate.Count>0)
        {
            var propagatePair = propagationHelper.PairsToPropagate.Dequeue();
            if (propagationHelper.CheckIfPairShouldBeProcessed(propagatePair))
            {
                ProcessCell(propagatePair);
            }
            if(propagationHelper.CheckForConflicts() || outputgrid.CheckIhGridIsSolved())
            {
                return;
            }
            if(propagationHelper.CheckForConflicts()&& propagationHelper.PairsToPropagate.Count==0 && propagationHelper.LowEntropySet.Count ==0)
            {
                propagationHelper.SetConflictFlag();
            }
        }
    }

    private void ProcessCell(VectorPair propagatePair)
    {
        throw new NotImplementedException();
    }
}

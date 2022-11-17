using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VectorPair 
{
    public Vector2Int BaseCellPosition { get; set; }
    public Vector2Int CellToPropagatePosition { get; set; }
    public Vector2Int PreviousCellPosition { get; set; }
    public Direction DirectionFromBase { get; set; }

    public VectorPair(Vector2Int baseCellPosition, Vector2Int cellToPropagatePosition, Vector2Int previousCellPosition, Direction directionFromBase)
    {
        BaseCellPosition = baseCellPosition;
        CellToPropagatePosition = cellToPropagatePosition;
        PreviousCellPosition = previousCellPosition;
        DirectionFromBase = directionFromBase;
    }

    public bool AreWeCheckingPreviousCellAgain()
    {
        return PreviousCellPosition == CellToPropagatePosition;
    }
}

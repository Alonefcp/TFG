using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DungeonGenerator : MonoBehaviour
{
    [SerializeField] protected TilemapVisualizer tilemapVisualizer;

    /// <summary>
    /// Creates a dungeon with an algorithm
    /// </summary>
    public abstract void GenerateDungeon();
}

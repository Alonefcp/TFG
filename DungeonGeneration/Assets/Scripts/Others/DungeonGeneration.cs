using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class DungeonGeneration : MonoBehaviour
{
    [SerializeField] protected TilemapVisualizer tilemapVisualizer = null;
    [SerializeField] protected PlayerController playerController = null;
    [SerializeField] protected bool useRandomSeed = true;
    [SerializeField] protected int seed;

    /// <summary>
    /// Creates a dungeon with an algorithm
    /// </summary>
    public virtual void GenerateDungeon()
    {
        if (useRandomSeed) seed = (int)DateTime.Now.Ticks;
        Random.InitState(seed);

        tilemapVisualizer.ClearTilemaps();
    }
}

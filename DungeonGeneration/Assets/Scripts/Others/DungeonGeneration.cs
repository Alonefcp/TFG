using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class DungeonGeneration : MonoBehaviour
{
    [SerializeField] protected TilemapVisualizer tilemapVisualizer = null;
    [SerializeField] protected PlayerController playerController = null;
    [SerializeField] private bool useRandomSeed = true;
    [SerializeField] private int seed;

    /// <summary>
    /// Creates a dungeon with an algorithm
    /// </summary>
    public virtual void GenerateDungeon()
    {
        if (useRandomSeed) seed = (int)DateTime.Now.Ticks/*Time.time*/;
        Random.InitState(seed);

        tilemapVisualizer.ClearTilemaps();
    }
}

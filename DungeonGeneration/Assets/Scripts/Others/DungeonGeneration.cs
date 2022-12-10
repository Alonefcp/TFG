using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGeneration : MonoBehaviour
{
    [SerializeField] protected TilemapVisualizer tilemapVisualizer = null;
    [SerializeField] protected PlayerController playerController = null;
    [SerializeField] private bool useRandomSeed = true;
    [SerializeField] private string seed;
    protected System.Random rng = null;

    /// <summary>
    /// Creates a dungeon with an algorithm
    /// </summary>
    public virtual void GenerateDungeon()
    {
        if (useRandomSeed) seed = Time.time.ToString();
        rng = new System.Random(seed.GetHashCode());

        tilemapVisualizer.ClearTilemaps();
    }
}

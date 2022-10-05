using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct Walker
{
    public Walker(Vector2Int _pos, Vector2Int _dir)
    {
        pos = _pos;
        dir = _dir;
    }

    public Vector2Int pos;
    public Vector2Int dir;

}

//Drunkard’s walk or Random Walk
public class RandomWalkAlgorithm : MonoBehaviour
{
    [SerializeField] private TilemapVisualizer tilemapVisualizer;

    [SerializeField] private bool startRandomlyEachIteration = true;
    [SerializeField] private int maxFloorPositions = 50;
    [SerializeField] private int minSteps = 10, maxSteps = 20;
    [SerializeField] private Vector2Int startPosition = new Vector2Int(0, 0);

    private HashSet<Vector2Int> positions;

    

    void Start()
    {
        HashSet<Vector2Int> floorPositions = RandomWalk();

        tilemapVisualizer.PaintFloorTiles(positions);

        //foreach (Vector2Int pos in floorPositions)
        //{
        //    Debug.Log(pos);
        //}
    }


    private HashSet<Vector2Int> RandomWalk()
    {
        positions = new HashSet<Vector2Int>();

        //pick a map cell as the starting point
        Walker walker = new Walker(startPosition, GetRandomDirection());

        //turn the selected map cell into floor
        positions.Add(startPosition);

        //while insufficient cells have been turned into floor
        while (positions.Count < maxFloorPositions)
        {
            //take X steps 
            int nSteps = Random.Range(minSteps, maxSteps);
            HashSet<Vector2Int> path = SimpleRandomWalk(walker, nSteps);
            positions.UnionWith(path);

            if (startRandomlyEachIteration)
            {
                Vector2Int randomPos = positions.ElementAt(Random.Range(0, positions.Count));
                Walker wal = new Walker(randomPos, walker.dir);
                walker = wal;
            }
        }

        return positions;
    }

    private HashSet<Vector2Int> SimpleRandomWalk(Walker walker, int nSteps)
    {
        float chanceToChangeDir = 1.0f;
        //bool levyFlight = false;
        //float levyFlightChance = 0.02f;

        HashSet<Vector2Int> positions = new HashSet<Vector2Int>();
        positions.Add(walker.pos);

        for (int j = 0; j < nSteps; j++)
        {
            //take one step in a random direction
            Vector2Int newPos = walker.pos + walker.dir;

            //if (walker.dir.Magnitude() > 1)
            //{
            //    CalculatePositions(walker, newPos, positions);
            //}
            //else
            //{
                /*if (newPos.x > -1 && newPos.y > -1)*/ positions.Add(newPos);
            //}
            /*if (newPos.x > -1 && newPos.y > -1)*/ walker.pos = newPos;


            if (Random.Range(0.0f,1.0f) <= chanceToChangeDir)
            {
                //if (levyFlight && (float)rnd.NextDouble() <= levyFlightChance)
                //{
                //    walker.dir = GetRandomEightDirection() * rnd.Next(3, 7);
                //}
                //else
                //{
                    walker.dir = GetRandomDirection();
                //}
            }
        }

        return positions;
    }

    private Vector2Int GetRandomDirection()
    {
        Vector2Int[] directions = { new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1) };

        return directions[Random.Range(0, directions.Length)];
    }
}

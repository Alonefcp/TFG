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

//Drunkard’s walk or Random Walk algorithm
public class RandomWalkAlgorithm : MonoBehaviour
{
    [SerializeField] private TilemapVisualizer tilemapVisualizer;

    [SerializeField] private Vector2Int startPosition = new Vector2Int(0, 0);
    [SerializeField] private bool startRandomlyEachIteration = true;
    [SerializeField] private int maxFloorPositions = 50;
    [SerializeField] private int minSteps = 10, maxSteps = 20;
    [Range(0.0f, 1.0f)]
    [SerializeField] float chanceToChangeDir = 1.0f;
    [SerializeField] bool levyFlight = false;
    [SerializeField] float levyFlightChance = 0.02f;
    [SerializeField] int minStepLength = 3;
    [SerializeField] int maxStepLength = 7;


    void Start()
    {
        HashSet<Vector2Int> floorPositions = RandomWalk();

        tilemapVisualizer.ClearTilemap();
        tilemapVisualizer.PaintFloorTiles(floorPositions);

        //foreach (Vector2Int pos in floorPositions)
        //{
        //    Debug.Log(pos);
        //}
    }


    private HashSet<Vector2Int> RandomWalk()
    {
        HashSet<Vector2Int> positions = new HashSet<Vector2Int>();

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
        HashSet<Vector2Int> positions = new HashSet<Vector2Int>();
        positions.Add(walker.pos);

        for (int j = 0; j < nSteps; j++)
        {
            //take one step in a random direction
            Vector2Int newPos = walker.pos + walker.dir;

            if (walker.dir.magnitude > 1)
            {
                CalculatePositions(walker, newPos, positions);
            }
            else
            {
                positions.Add(newPos);
            }

            walker.pos = newPos;

            if (Random.Range(0.0f,1.0f) <= chanceToChangeDir)
            {
                if (levyFlight && Random.Range(0.0f, 1.0f) <= levyFlightChance)
                {
                    walker.dir = GetRandomDirection() * Random.Range(minStepLength, maxStepLength);
                }
                else
                {
                    walker.dir = GetRandomDirection();
                }
            }
        }

        return positions;
    }

    private void CalculatePositions(Walker walker, Vector2Int newPos, HashSet<Vector2Int> positions)
    {
        while (walker.pos.x != newPos.x || walker.pos.y != newPos.y)
        {
            if (walker.dir.x > 0)
            {
                walker.pos.x++;
            }
            else if (walker.dir.x < 0)
            {
                walker.pos.x--;
            }

            if (walker.dir.y > 0)
            {
                walker.pos.y++;
            }
            else if (walker.dir.y < 0)
            {
                walker.pos.y--;
            }

            positions.Add(walker.pos);
        }
    }

    private Vector2Int GetRandomDirection()
    {
        Vector2Int[] directions = { new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1) };

        return directions[Random.Range(0, directions.Length)];
    }

    private Vector2Int GetRandomEightDirection()
    {
        Vector2Int[] directions = { new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1),
            new Vector2Int(1, 1), new Vector2Int(-1, 1), new Vector2Int(1, -1), new Vector2Int(-1, -1)};

        return directions[Random.Range(0, directions.Length)];
    }
}

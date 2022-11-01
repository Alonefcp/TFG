using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//Walker representation
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
public class RandomWalkAlgorithm : DungeonGenerator
{
    [SerializeField] private Vector2Int startPosition = new Vector2Int(0, 0);
    [SerializeField] private bool startRandomlyEachIteration = true;
    [SerializeField] private int maxFloorPositions = 50;
    [SerializeField] private int minSteps = 10, maxSteps = 20;
    [Range(0.0f, 1.0f)]
    [SerializeField] float chanceToChangeDirection = 1.0f;
    [SerializeField] bool eliminateSingleWalls = false;
    [SerializeField] bool useEightDirections = false;
    [SerializeField] bool levyFlight = false;
    [SerializeField] float levyFlightChance = 0.02f;
    [SerializeField] int minStepLength = 3;
    [SerializeField] int maxStepLength = 7;


    //void Start()
    //{
    //    GenerateDungeon();
    //}

    /// <summary>
    /// Creates a dungeon with the Random Walk or Drunkard's Walk algorithm
    /// </summary>
    public override void GenerateDungeon()
    {
        HashSet<Vector2Int> floorPositions = RandomWalk();

        tilemapVisualizer.ClearTilemap();
        tilemapVisualizer.PaintFloorTiles(floorPositions);
        if (eliminateSingleWalls)
        {
            tilemapVisualizer.EliminateSingleSpaces();
        }
    }

    /// <summary>
    /// Performs the Random Walk or Drunkard's Walk algorithm
    /// </summary>
    /// <returns>Returns a HashSet with all floor positions</returns>
    private HashSet<Vector2Int> RandomWalk()
    {
        HashSet<Vector2Int> positions = new HashSet<Vector2Int>();

        //pick a map cell as the starting point
        Walker walker = new Walker(startPosition, Directions.GetRandomFourDirection());

        //turn the selected map cell into floor
        positions.Add(startPosition);

        //while insufficient cells have been turned into floor
        while (positions.Count < maxFloorPositions)
        {
            //take X steps 
            int nSteps = Random.Range(minSteps, maxSteps);
            HashSet<Vector2Int> path = SimpleRandomWalk(walker, nSteps);
            positions.UnionWith(path);

            walker.dir = Directions.GetRandomFourDirection();

            if (startRandomlyEachIteration)
            {
                Vector2Int randomPos = positions.ElementAt(Random.Range(0, positions.Count));
                Walker wal = new Walker(randomPos, walker.dir);
                walker = wal;
            }
        }

        return positions;
    }

    /// <summary>
    /// Makes a path of nSteps
    /// </summary>
    /// <param name="walker">The walker who makes the path</param>
    /// <param name="nSteps">Path lenght</param>
    /// <returns>Returns a HashSet with all path positions</returns>
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

            if (Random.value <= chanceToChangeDirection)
            {
                if (levyFlight && Random.value <= levyFlightChance)
                {
                    walker.dir = Directions.GetRandomEightDirection() * Random.Range(minStepLength, maxStepLength);
                }
                else
                {
                    walker.dir = useEightDirections ? Directions.GetRandomEightDirection() : Directions.GetRandomFourDirection();
                }
            }
        }

        return positions;
    }

    

    /// <summary>
    /// Calculate the positions between the walker positions and his new position
    /// </summary>
    /// <param name="walker">Walker struct which contains his position and direction</param>
    /// <param name="newPos">New walker positions</param>
    /// <param name="positions">Structure to hold the positions</param>
    private void CalculatePositions(Walker walker, Vector2Int newPos, HashSet<Vector2Int> positions)
    {
        Vector2Int initialWalkerPos = walker.pos;

        while (walker.pos.x != newPos.x || walker.pos.y != newPos.y)
        {
            bool moveDiagonally = true;

            bool positveX = walker.dir.x > 0;
            bool negativeX = walker.dir.x < 0;
            bool positiveY = walker.dir.y > 0;
            bool negativeY = walker.dir.y < 0;

            if (positveX)
            {
                walker.pos.x++;
            }
            else if (negativeX)
            {
                walker.pos.x--;
            }
            else
            {
                moveDiagonally = false;
            }

            if (positiveY)
            {
                walker.pos.y++;
            }
            else if (negativeY)
            {
                walker.pos.y--;
            }
            else
            {
                moveDiagonally = false;
            }

            positions.Add(walker.pos);

            if (moveDiagonally)
            {
                AddDiagonalPositions(initialWalkerPos, walker.pos, positions, positveX, negativeX, positiveY, negativeY);
            }
        }
    }

    /// <summary>
    /// Adds extra positions if the walker moves diagonally
    /// </summary>
    /// <param name="initialWalkerPos">Initial walker position</param>
    /// <param name="currentWalkerPosition"></param>
    /// <param name="positions">Structure to hold the positions</param>
    /// <param name="positveX">If the walker goes in the positive X axis</param>
    /// <param name="negativeX">If the walker goes in the negative X axis</param>
    /// <param name="positiveY">If the walker goes in the positve Y axis</param>
    /// <param name="negativeY">If the walker goes in the negative Y axis</param>
    private void AddDiagonalPositions(Vector2Int initialWalkerPos, Vector2Int currentWalkerPosition, HashSet<Vector2Int> positions, bool positveX,  bool negativeX, bool positiveY, bool negativeY)
    {
        if (positveX && positiveY)
        {
            positions.Add(currentWalkerPosition + new Vector2Int(1, 0));
            positions.Add(currentWalkerPosition + new Vector2Int(0, 1));

            positions.Add(initialWalkerPos + new Vector2Int(1, 0));
            positions.Add(initialWalkerPos + new Vector2Int(0, 1));
        }
        else if (negativeX && negativeY)
        {
            positions.Add(currentWalkerPosition + new Vector2Int(-1, 0));
            positions.Add(currentWalkerPosition + new Vector2Int(0, -1));

            positions.Add(initialWalkerPos + new Vector2Int(-1, 0));
            positions.Add(initialWalkerPos + new Vector2Int(0, -1));
        }
        else if (positveX && negativeY)
        {
            positions.Add(currentWalkerPosition + new Vector2Int(1, 0));
            positions.Add(currentWalkerPosition + new Vector2Int(0, -1));

            positions.Add(initialWalkerPos + new Vector2Int(1, 0));
            positions.Add(initialWalkerPos + new Vector2Int(0, -1));
        }
        else if (negativeX && positiveY)
        {
            positions.Add(currentWalkerPosition + new Vector2Int(-1, 0));
            positions.Add(currentWalkerPosition + new Vector2Int(0, 1));

            positions.Add(initialWalkerPos + new Vector2Int(-1, 0));
            positions.Add(initialWalkerPos + new Vector2Int(0, 1));
        }
    }

    
}

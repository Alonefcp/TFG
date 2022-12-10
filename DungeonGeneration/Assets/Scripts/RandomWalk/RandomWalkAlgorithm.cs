using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//Drunkard’s walk or Random Walk algorithm
public class RandomWalkAlgorithm : DungeonGeneration
{
    //Walker representation
    struct Walker
    {
        public Walker(Vector2Int pos, Vector2Int dir)
        {
            this.pos = pos;
            this.dir = dir;
        }

        public Vector2Int pos;
        public Vector2Int dir;
    }

    [SerializeField] private Vector2Int startPosition = new Vector2Int(0, 0);
    [SerializeField] private bool startRandomlyEachIteration = true;
    [SerializeField] private int numberOfFloorPositions = 50;
    [SerializeField] private int minSteps = 10, maxSteps = 20;
    [Range(0.0f, 1.0f)]
    [SerializeField] float chanceToChangeDirection = 1.0f;
    [SerializeField] bool eliminateSingleWallsCells = false;
    [SerializeField] bool useEightDirections = false;
    [SerializeField] bool levyFlight = false;
    [Range(0.0f, 1.0f)]
    [SerializeField] float levyFlightChance = 0.02f;
    [SerializeField] int minStepLength = 3;
    [SerializeField] int maxStepLength = 7;
    [SerializeField] bool showGizmos = false;

    //void Start()
    //{
    //    GenerateDungeon();
    //}

    /// <summary>
    /// Creates a dungeon with the Random Walk or Drunkard's Walk algorithm
    /// </summary>
    public override void GenerateDungeon()
    {
        base.GenerateDungeon();

        HashSet<Vector2Int> floorPositions = RunRandomWalk();
       /* Debug.Log(floorPositions.Count);*///QUITAR!!
       
        tilemapVisualizer.PaintFloorTiles(floorPositions);
        if (eliminateSingleWallsCells)
        {
            tilemapVisualizer.EliminateSingleSpaces(out HashSet<Vector2Int> positions);
            floorPositions.UnionWith(positions); //we add the positions which have become walkables
        }

        WallGenerator.CreateWalls(floorPositions, tilemapVisualizer);
        playerController.SetPlayer(startPosition,new Vector3(0.3f,0.3f,0.3f));
    }

    //For debugging
    private void OnDrawGizmos()
    {
        if (showGizmos)
        {
            Gizmos.color = Color.red;

            //We show the start position
            Gizmos.DrawWireSphere(new Vector3(startPosition.x+0.5f, startPosition.y+0.5f, 0.0f), 0.5f);
        }
    }

    /// <summary>
    /// Performs the Random Walk or Drunkard's Walk algorithm
    /// </summary>
    /// <returns>Returns a HashSet with all floor positions</returns>
    private HashSet<Vector2Int> RunRandomWalk()
    {
        HashSet<Vector2Int> positions = new HashSet<Vector2Int>();

        //Pick a map cell as the starting point
        Walker walker = new Walker(startPosition, useEightDirections ? Directions.GetRandomEightDirection() : Directions.GetRandomFourDirection());

        //Turn the selected map cell into floor
        positions.Add(startPosition);

        //While insufficient cells have been turned into floor
        while (positions.Count < numberOfFloorPositions)
        {
            //Take X steps (path length) 
            int numberOfSteps = Random.Range(minSteps, maxSteps);

            //Path positions
            HashSet<Vector2Int> path = SimpleRandomWalk(walker, numberOfSteps);
            positions.UnionWith(path);

            //We set a new walker`s direction
            walker.dir = useEightDirections ? Directions.GetRandomEightDirection() : Directions.GetRandomFourDirection();

            //We change walker's position
            if (startRandomlyEachIteration)
            {
                Vector2Int randomPosition = positions.ElementAt(Random.Range(0, positions.Count));
                Walker newWalker = new Walker(randomPosition, walker.dir);
                walker = newWalker;
            }
        }

        return positions;
    }



    /// <summary>
    /// Makes a path of nSteps
    /// </summary>
    /// <param name="walker">The walker who makes the path</param>
    /// <param name="numberOfSteps">Path lenght</param>
    /// <returns>Returns a HashSet with all path positions</returns>
    private HashSet<Vector2Int> SimpleRandomWalk(Walker walker, int numberOfSteps)
    {              
        HashSet<Vector2Int> pathPositions = new HashSet<Vector2Int>();
        pathPositions.Add(walker.pos);

        for (int j = 0; j < numberOfSteps; j++)
        {
            //Take one step in a random direction
            Vector2Int newPos = walker.pos + walker.dir;

            //1.-For the levy fligh we calculate the positions between the walker's position and the new position,
            //and we add them to the path positions
            //2.- If the walker moves diagonally
            if (walker.dir.magnitude > 1)
            {
                CalculatePositions(walker, newPos, pathPositions);
            }
            else //Otherwise we simply add the position to the path positions
            {
                pathPositions.Add(newPos);
            }

            //We set a new walker`s position
            walker.pos = newPos;

            if (Random.value <= chanceToChangeDirection) //There is a chance to change or not the walker's direction
            {
                if (levyFlight && Random.value <= levyFlightChance) //There is a chance to apply or not the levy flight
                {
                    int stepLength = Random.Range(minStepLength, maxStepLength);
                    walker.dir = (useEightDirections ? Directions.GetRandomEightDirection() : Directions.GetRandomFourDirection()) * stepLength;
                }
                else //Otherwise we set a new walker`s direction
                {
                    walker.dir = useEightDirections ? Directions.GetRandomEightDirection() : Directions.GetRandomFourDirection();
                }
            }
        }

        return pathPositions;
    }  

    /// <summary>
    /// Calculate the positions between the walker's position and his new position. This method is used 
    /// for diagonal positions.
    /// </summary>
    /// <param name="walker">Walker struct which contains his position and direction</param>
    /// <param name="newPosition">New walker position</param>
    /// <param name="pathPositions">Structure to hold the path positions</param>
    private void CalculatePositions(Walker walker, Vector2Int newPosition, HashSet<Vector2Int> pathPositions)
    {
        //We add to the path the positions between the walker's position his new position
        while (walker.pos.x != newPosition.x || walker.pos.y != newPosition.y)
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

            pathPositions.Add(walker.pos);

            //If we are moving diagonally, we add more positions around the walker's position,
            //so this positions are walkable. If we don`t do this diagonal positions are not walkable.
            if (moveDiagonally)
            {
                AddMorePositions(walker.pos, 1, pathPositions);             
            }
        }
    }

    /// <summary>
    /// Adds more positions around the walker's position
    /// </summary>
    /// <param name="pos">Walker's position</param>
    /// <param name="radius">Indicates how many positions we want to add around</param>
    /// <param name="pathPositions">Structure to hold the path positions</param>
    private void AddMorePositions(Vector2Int pos, int radius, HashSet<Vector2Int> pathPositions)
    {
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                if (x * x + y * y <= radius * radius)
                {
                    int drawX = pos.x + x;
                    int drawY = pos.y + y;

                    pathPositions.Add(new Vector2Int(drawX, drawY));                                      
                }
            }
        }
    }
}

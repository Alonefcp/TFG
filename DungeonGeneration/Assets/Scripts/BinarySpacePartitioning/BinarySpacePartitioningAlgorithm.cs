using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BinarySpacePartitioningAlgorithm : DungeonGeneration
{
    public enum CorridorsAlgorithm {TunnelingAlgorithm, Delaunay_Prim_Astar}

    [Range(40,150)]
    [SerializeField] private int spaceWidth = 20,  spaceHeight = 20;
    [Range(8, 20)]
    [SerializeField] private int minRoomWidth = 4, /*maxRoomWidth = 4,*/  minRoomHeight = 4; /*maxRoomHeight = 4*/

    [Range(1,2)]
    [SerializeField] private int roomOffset = 1;
    [Range(0.1f, 0.5f)]
    [SerializeField] private float minSplitPercent = 0.1f;
    [Range(0.5f, 1.0f)]
    [SerializeField] private float maxSplitPercent = 0.9f;

    [SerializeField] private CorridorsAlgorithm corridorsAlgorithm = CorridorsAlgorithm.TunnelingAlgorithm;
    [SerializeField] private bool addSomeRemainingEdges = true;
    [SerializeField] private bool widerCorridors = false;
    [Range(1, 4)]
    [SerializeField] private int corridorSize = 1;

    [SerializeField] private bool setSpecialRooms = false;

    private Grid2D grid;
    private List<BoundsInt> roomList;

    public bool WiderCorridors { get => widerCorridors;}
    public CorridorsAlgorithm CorridorsAlgorithmType { get => corridorsAlgorithm;}


    //void Start()
    //{
    //    GenerateDungeon();
    //}

    public override void GenerateDungeon()
    {
        base.GenerateDungeon();

        //Create and draw rooms
        Vector2Int startPosition = new Vector2Int(0, 0);
        BoundsInt totalSpace = new BoundsInt((Vector3Int)startPosition, new Vector3Int(spaceWidth, spaceHeight, 0));
        roomList = BinarySpacePartitioning(totalSpace, minRoomWidth, minRoomHeight);

        grid = new Grid2D(spaceWidth, spaceHeight, tilemapVisualizer.GetCellRadius());
        HashSet<Vector2Int> floorPositions = CreateRooms(roomList, grid);

        tilemapVisualizer.PaintFloorTiles(floorPositions);
        //Create connections between rooms
        List<Vertex> roomCenters = new List<Vertex>();

        foreach (BoundsInt room in roomList)
        {
            Vector2Int center = (Vector2Int)Vector3Int.RoundToInt(room.center);
            roomCenters.Add(new Vertex(center));
        }

        //Set special rooms
        Vector2Int roomStartPosition = new Vector2Int();
        if (setSpecialRooms) SpecialRooms.SetStartAndEndRoom(tilemapVisualizer, roomCenters, out roomStartPosition);

        //Set player position
        playerController.SetPlayer(new Vector2(roomStartPosition.x, roomStartPosition.y), new Vector3(0.3f, 0.3f, 0.3f));

        //Connect rooms
        if (corridorsAlgorithm == CorridorsAlgorithm.TunnelingAlgorithm)
        {
            HashSet<Vector2Int> corridors = CorridorsAlgorithms.ConnectRooms(roomCenters, WiderCorridors,corridorSize,spaceWidth,spaceHeight);
            tilemapVisualizer.PaintFloorTiles(corridors);
            floorPositions.UnionWith(corridors);
        }
        else
        {
            List<HashSet<Vector2Int>> paths = CorridorsAlgorithms.ConnectRooms(roomCenters, grid, WiderCorridors, corridorSize,spaceWidth, spaceHeight, addSomeRemainingEdges);
            foreach (HashSet<Vector2Int> path in paths)
            {
                tilemapVisualizer.PaintFloorTiles(path);
                floorPositions.UnionWith(path);
            }
        }
       

        //Creates outter walls
        WallGenerator.CreateWalls(floorPositions,tilemapVisualizer);      
    }

    /// <summary>
    /// Returns all the positions that are walkable and are part of the rooms generated by BSP
    /// </summary>
    /// <param name="roomsList">All the rooms generated by the BSP</param>
    /// <param name="grid">Map representation</param>
    /// <returns></returns>
    private HashSet<Vector2Int> CreateRooms(List<BoundsInt> roomsList, Grid2D grid)
    {
        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();
        foreach (BoundsInt room in roomsList)
        {
            for (int col = roomOffset; col < room.size.x - roomOffset; col++)
            {
                for (int row = roomOffset; row < room.size.y - roomOffset; row++)
                {
                    Vector2Int position = (Vector2Int)room.min + new Vector2Int(col, row);
                    grid.NodeFromWorldPoint(position).SetType(Node.NodeType.Floor);                    
                    floor.Add(position);
                }
            }
        }
        return floor;
    }

    /// <summary>
    /// Performs the binary space partitioning by splitting a given space into smaller rooms 
    /// </summary>
    /// <param name="spaceToSplit">Space which is going to be splitted</param>
    /// <param name="minWidth">Minimun rooms widht</param>
    /// <param name="maxWidth">Maximun rooms widht</param>
    /// <param name="minHeight">Minimun rooms height</param>
    /// <param name="maxHeight">Maxximun rooms height</param>
    /// <returns></returns>
    private List<BoundsInt> BinarySpacePartitioning(BoundsInt spaceToSplit, int minWidth, int minHeight)
    {
        Queue<BoundsInt> roomsQueue = new Queue<BoundsInt>();
        List<BoundsInt> roomsList = new List<BoundsInt>();
        roomsQueue.Enqueue(spaceToSplit);

        while (roomsQueue.Count > 0)
        {
            BoundsInt room = roomsQueue.Dequeue();

            if (room.size.y >= minHeight && room.size.x >= minWidth)
            {
                float roomMultiplier = Random.Range(1.25f, 3.0f);
                if (Random.value < 0.5f)
                {
                    if (room.size.y >= minHeight * roomMultiplier)
                    {
                        HorizontalSplit(roomsQueue, room);
                    }
                    else if (room.size.x >= minWidth * roomMultiplier)
                    {
                        VerticalSplit(roomsQueue, room);
                    }
                    else 
                    {
                        roomsList.Add(room);
                    }
                }
                else
                {
                    if (room.size.x >= minWidth * roomMultiplier)
                    {
                        VerticalSplit(roomsQueue, room);
                    }
                    else if (room.size.y >= minHeight * roomMultiplier)
                    {
                        HorizontalSplit(roomsQueue, room);
                    }
                    else 
                    {
                        roomsList.Add(room);
                    }
                }
            }

        }

        return roomsList;
    }

    /// <summary>
    /// Splits a given space vertically
    /// </summary>
    /// <param name="roomsQueue">Rooms which are going to be splitted again, if it is possible</param>
    /// <param name="room">Space which is going to be splitted vertically</param>
    private void VerticalSplit(Queue<BoundsInt> roomsQueue, BoundsInt room)
    {
        int xSplit = (int)Random.Range(minSplitPercent*room.size.x, maxSplitPercent*room.size.x);
       
        BoundsInt firstRoom = new BoundsInt(room.min, new Vector3Int(xSplit, room.size.y, room.size.z));
        BoundsInt secondRoom = new BoundsInt(new Vector3Int(room.min.x + xSplit,room.min.y, room.min.z), new Vector3Int(room.size.x - xSplit, room.size.y, room.size.z));

        roomsQueue.Enqueue(firstRoom);
        roomsQueue.Enqueue(secondRoom);
    }

    /// <summary>
    /// Splits a given space horizontally
    /// </summary>
    /// <param name="roomsQueue">Rooms which are going to be splitted again, if it is possible</param>
    /// <param name="room">Space which is going to be splitted horizontally</param>
    private void HorizontalSplit(Queue<BoundsInt> roomsQueue, BoundsInt room)
    {
        int ySplit = (int)Random.Range(minSplitPercent * room.size.y, maxSplitPercent*room.size.y);

        BoundsInt firstRoom = new BoundsInt(room.min, new Vector3Int(room.size.x, ySplit, room.size.z));
        BoundsInt secondRoom = new BoundsInt(new Vector3Int(room.min.x, room.min.y + ySplit, room.min.z), new Vector3Int(room.size.x, room.size.y-ySplit, room.size.z));

        roomsQueue.Enqueue(firstRoom);
        roomsQueue.Enqueue(secondRoom);
    }  
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BinarySpacePartitioningAlgorithm : DungeonGenerator
{
    enum CorridorsAlgorithm {TunnelingAlgorithm, Delaunay_Prim_Astar}

    [SerializeField] private Grid2D grid;
    [SerializeField] private int spaceWidth = 20,  spaceHeight = 20;
    [SerializeField] private int minRoomWidth = 4, maxRoomWidth = 4,  minRoomHeight = 4, maxRoomHeight = 4;
    [SerializeField] private Vector2Int startPosition = new Vector2Int(0, 0);

    [Range(1,10)]
    [SerializeField] private int roomOffset = 1;

    [SerializeField] private CorridorsAlgorithm corridorsAlgorithm = CorridorsAlgorithm.TunnelingAlgorithm;
    [SerializeField] private bool addSomeRemainingEdges = true;
    [SerializeField] private bool widerCorridors = false;

    [SerializeField] private bool verticalGridStructure = false;
    [SerializeField] private bool horizontalGridStructure = false;

    [SerializeField] private bool setSpecialRooms = false;

    //[SerializeField] private bool showGizmos = false;

    private List<BoundsInt> roomList;


    //void Start()
    //{
    //    GenerateDungeon();
    //}

    public override void GenerateDungeon()
    {
        //Create and paint rooms
        BoundsInt totalSpace = new BoundsInt((Vector3Int)startPosition, new Vector3Int(spaceWidth, spaceHeight, 0));
        roomList = BinarySpacePartitioning(totalSpace, minRoomWidth, maxRoomWidth, minRoomHeight, maxRoomHeight);

        HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();
        grid.CreateGrid(spaceWidth, spaceHeight, tilemapVisualizer.GetCellRadius());

        floorPositions = CreateRooms(roomList, grid);

        List<Vertex> roomCenters = new List<Vertex>();

        foreach (BoundsInt room in roomList)
        {
            roomCenters.Add(new Vertex(room.center));
        }

        tilemapVisualizer.ClearTilemap();
        tilemapVisualizer.PaintFloorTiles(floorPositions);

        //Create and paint corridors
        if(corridorsAlgorithm == CorridorsAlgorithm.TunnelingAlgorithm)
        {
            HashSet<Vector2Int> corridors = CorridorsAlgorithms.ConnectRooms(roomCenters, widerCorridors);
            tilemapVisualizer.PaintFloorTiles(corridors);
        }
        else
        {
            List<HashSet<Vector2Int>> paths = CorridorsAlgorithms.ConnectRooms(roomCenters, grid, widerCorridors, addSomeRemainingEdges);
            foreach (var path in paths)
            {
                tilemapVisualizer.PaintFloorTiles(path);
            }
        }

       if(setSpecialRooms) SpecialRooms.SetStartAndEndRoom(tilemapVisualizer, roomCenters);
    }

    //private void OnDrawGizmos()
    //{
    //    if(showGizmos)
    //    {
    //        Gizmos.color = Color.red;
    //        Gizmos.DrawWireCube(new Vector3(spaceWidth / 2, spaceHeight / 2, 0), new Vector3Int(spaceWidth, spaceHeight, 0));

    //        //Gizmos.color = Color.green;
    //        //if (delaunayTriangulation != null)
    //        //{
    //        //    foreach (var edge in edges)
    //        //    {
    //        //        Gizmos.DrawLine(edge.U.Position, edge.V.Position);
    //        //    }
    //        //}

    //        //if(paths!=null)
    //        //{
    //        //    foreach (var path in paths)
    //        //    {
    //        //        foreach (var node in path)
    //        //        {
    //        //            Gizmos.DrawWireSphere(node.worldPosition, 1.0f);
    //        //        }
    //        //    }
    //        //}

    //    }      
    //}

    private HashSet<Vector2Int> CreateRooms(List<BoundsInt> roomsList, Grid2D grid)
    {
        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();
        foreach (var room in roomsList)
        {
            for (int col = roomOffset; col < room.size.x - roomOffset; col++)
            {
                for (int row = roomOffset; row < room.size.y - roomOffset; row++)
                {
                    Vector2Int position = (Vector2Int)room.min + new Vector2Int(col, row);
                    grid.NodeFromWorldPoint(new Vector3(position.x, position.y, 0.0f)).SetType(Node.NodeType.Floor);                    
                    floor.Add(position);
                }
            }
        }
        return floor;
    }

    private List<BoundsInt> BinarySpacePartitioning(BoundsInt spaceToSplit, int minWidth, int maxWidth, int minHeight, int maxHeight)
    {
        Queue<BoundsInt> roomsQueue = new Queue<BoundsInt>();
        List<BoundsInt> roomsList = new List<BoundsInt>();
        roomsQueue.Enqueue(spaceToSplit);

        while (roomsQueue.Count > 0)
        {
            BoundsInt room = roomsQueue.Dequeue();

            if (room.size.y >= minHeight && room.size.x >= minWidth)
            {
                if (Random.value < 0.5f)
                {
                    if (room.size.y >= Random.Range(minHeight, maxHeight+1) * Random.Range(1.25f, 3.0f))
                    {
                        HorizontalSplit(minHeight, roomsQueue, room);
                    }
                    else if (room.size.x >= Random.Range(minWidth, maxWidth+1) * Random.Range(1.25f, 3.0f))
                    {
                        VerticalSplit(minWidth, roomsQueue, room);
                    }
                    else /*if (room.size.x >= minWidth && room.size.y >= minHeight)*/
                    {
                        roomsList.Add(room);
                    }
                }
                else
                {
                    if (room.size.x >= Random.Range(minWidth, minWidth+1) * Random.Range(1.25f, 3.0f))
                    {
                        VerticalSplit(minWidth, roomsQueue, room);
                    }
                    else if (room.size.y >= Random.Range(minHeight, maxHeight+1) * Random.Range(1.25f, 3.0f))
                    {
                        HorizontalSplit(minHeight, roomsQueue, room);
                    }
                    else /*if (room.size.x >= minWidth && room.size.y >= minHeight)*/
                    {
                        roomsList.Add(room);
                    }
                }
            }
        }

        return roomsList;
    }

    private void VerticalSplit(int minWidth, Queue<BoundsInt> roomsQueue, BoundsInt room)
    {
        int xSplit = verticalGridStructure ? Random.Range(minWidth, room.size.x - minWidth) : Random.Range(1, room.size.x);
       
        BoundsInt firstRoom = new BoundsInt(room.min, new Vector3Int(xSplit, room.size.y, room.size.z));
        BoundsInt secondRoom = new BoundsInt(new Vector3Int(room.min.x + xSplit,room.min.y, room.min.z), new Vector3Int(room.size.x - xSplit, room.size.y, room.size.z));

        roomsQueue.Enqueue(firstRoom);
        roomsQueue.Enqueue(secondRoom);
    }

    private void HorizontalSplit(int minHeight, Queue<BoundsInt> roomsQueue, BoundsInt room)
    {
        int ySplit = horizontalGridStructure ? Random.Range(minHeight, room.size.y - minHeight) : Random.Range(1, room.size.y);

        BoundsInt firstRoom = new BoundsInt(room.min, new Vector3Int(room.size.x, ySplit, room.size.z));
        BoundsInt secondRoom = new BoundsInt(new Vector3Int(room.min.x, room.min.y + ySplit, room.min.z), new Vector3Int(room.size.x, room.size.y-ySplit, room.size.z));

        roomsQueue.Enqueue(firstRoom);
        roomsQueue.Enqueue(secondRoom);
    }  
}

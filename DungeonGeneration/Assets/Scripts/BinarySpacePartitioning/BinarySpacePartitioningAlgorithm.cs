using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphDataStructure;

public class BinarySpacePartitioningAlgorithm : DungeonGenerator
{
    [SerializeField] private TunnelingAlgorithm tunnelingAlgorithm; // creating corridors
    [SerializeField] private int spaceWidth = 20,  spaceHeight = 20;
    [SerializeField] private int minRoomWidth = 4,  minRoomHeight = 4;
    [SerializeField] private Vector2Int startPosition = new Vector2Int(0, 0);

    [Range(1,10)]
    [SerializeField] private int roomOffset = 1;

    [SerializeField] private bool widerCorridors = false;

    [SerializeField] private bool verticalGridStructure = false;
    [SerializeField] private bool horizontalGridStructure = false;

    [SerializeField] private bool showGizmos = false;

    private List<BoundsInt> roomList;
    private WeightedGraph<Vector2Int> graphRooms;

    private DelaunayTriangulation delaunayTriangulation;

    //void Start()
    //{
    //    List<BoundsInt> roomList = BinarySpacePartitioning(new BoundsInt((Vector3Int)startPosition, new Vector3Int(spaceWidth, spaceHeight, 0)), minRoomWidth, minRoomHeight);

    //    HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();

    //    floorPositions = CreateRooms(roomList);

    //    tilemapVisualizer.ClearTilemap();
    //    tilemapVisualizer.PaintFloorTiles(floorPositions);
    //}

    public override void GenerateDungeon()
    {
        BoundsInt totalSpace = new BoundsInt((Vector3Int)startPosition, new Vector3Int(spaceWidth, spaceHeight, 0));
        roomList = BinarySpacePartitioning(totalSpace, minRoomWidth, minRoomHeight);

        HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();

        floorPositions = CreateRooms(roomList);

        List<Vector2Int> roomCenters = new List<Vector2Int>();
        List<Vertex> roomCentersForDelaunay = new List<Vertex>();

        foreach (BoundsInt room in roomList)
        {
            roomCenters.Add((Vector2Int)Vector3Int.RoundToInt(room.center));
            roomCentersForDelaunay.Add(new Vertex(room.center));
        }

        graphRooms = new WeightedGraph<Vector2Int>(false, false);

        delaunayTriangulation = DelaunayTriangulation.Triangulate(roomCentersForDelaunay);

        HashSet<Vector2Int> corridors = tunnelingAlgorithm.ConnectRooms(roomCenters, graphRooms, widerCorridors);

        //floorPositions.UnionWith(corridors);
        tilemapVisualizer.ClearTilemap();
        tilemapVisualizer.PaintFloorTiles(floorPositions);
        tilemapVisualizer.PaintCorridorTiles(corridors);
    }

    private void OnDrawGizmos()
    {
        if(showGizmos)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(new Vector3(spaceWidth / 2, spaceHeight / 2, 0), new Vector3Int(spaceWidth, spaceHeight, 0));

            Gizmos.color = Color.green;
            if (delaunayTriangulation != null)
            {
                foreach (var edge in delaunayTriangulation.Edges)
                {
                    Gizmos.DrawLine(edge.U.Position, edge.V.Position);
                }
            }



            //if (graphRooms != null)
            //{
            //    Gizmos.color = Color.green;
            //    foreach (WeightedGraphNode<Vector2Int> node in graphRooms.Nodes)
            //    {
            //        Gizmos.DrawWireSphere(new Vector3(node.Value.x, node.Value.y, 0.0f), 1.5f);
            //    }
            //    Gizmos.color = Color.yellow;
            //    foreach (WeightedEdge<Vector2Int> edge in graphRooms.GetEdges())
            //    {
            //        Gizmos.DrawLine(new Vector3(edge.From.Value.x, edge.From.Value.y, 0.0f), new Vector3(edge.To.Value.x, edge.To.Value.y, 0.0f));
            //    }
            //}

            //foreach (BoundsInt space in roomList)
            //{
            //    Gizmos.DrawWireCube(space.center, space.size);
            //}
        }      
    }

    //private HashSet<Vector2Int> ConnectRooms(List<Vector2Int> roomCenters, WeightedGraph<Vector2Int> graph)
    //{
    //    HashSet<Vector2Int> corridors = new HashSet<Vector2Int>();

    //    Vector2Int currentRoomCenter = roomCenters[Random.Range(0, roomCenters.Count)];
    //    roomCenters.Remove(currentRoomCenter);
    //    WeightedGraphNode<Vector2Int> currentRoomCenterNode = graph.AddNode(currentRoomCenter);

    //    while(roomCenters.Count > 0)
    //    {
    //        Vector2Int closest = FindClosestPointTo(currentRoomCenter, roomCenters);
    //        roomCenters.Remove(closest);
    //        WeightedGraphNode<Vector2Int> closestNode = graph.AddNode(closest);

    //        HashSet<Vector2Int> newCorridor = CreateCorridor(currentRoomCenter, closest);

    //        graph.AddEdge(currentRoomCenterNode, closestNode,0);

    //        currentRoomCenter = closestNode.Value;
    //        currentRoomCenterNode = closestNode;

    //        corridors.UnionWith(newCorridor);
    //    }

    //    return corridors;
    //}

    //private HashSet<Vector2Int> CreateCorridor(Vector2Int currentRoomCenter, Vector2Int destination)
    //{
    //    HashSet<Vector2Int> corridor = new HashSet<Vector2Int>();

    //    Vector2Int position = currentRoomCenter;
    //    corridor.Add(position);

    //    while(position.y != destination.y)
    //    {
    //        if(destination.y > position.y)
    //        {
    //            position += new Vector2Int(0,1);
    //        }
    //        else if(destination.y < position.y)
    //        {
    //            position += new Vector2Int(0,-1);
    //        }

    //        corridor.Add(position);
    //    }

    //    while (position.x != destination.x)
    //    {
    //        if (destination.x > position.x)
    //        {
    //            position += new Vector2Int(1,0);
    //        }
    //        else if (destination.x < position.x)
    //        {
    //            position += new Vector2Int(-1,0);
    //        }

    //        corridor.Add(position);
    //    }


    //    if (widerCorridors)
    //    {
    //        HashSet<Vector2Int> corridorSides = new HashSet<Vector2Int>();

    //        //widdening corridor
    //        foreach (Vector2Int Corridor in corridor)
    //        {
    //            foreach (Vector2Int dir in GetDirectionsArray())
    //            {
    //                if (!corridor.Contains(Corridor + dir))
    //                {
    //                    corridorSides.Add(Corridor + dir);
    //                }
    //            }
    //        }

    //        corridor.UnionWith(corridorSides);
    //    }

    //    return corridor;
    //}


    //private Vector2Int[] GetDirectionsArray()
    //{
    //    Vector2Int[] directions = { new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1),
    //        new Vector2Int(1, 1), new Vector2Int(-1, 1), new Vector2Int(1, -1), new Vector2Int(-1, -1)};

    //    return directions;
    //}

    //private Vector2Int FindClosestPointTo(Vector2Int currentRoomCenter, List<Vector2Int> roomCenters)
    //{
    //    Vector2Int closest = Vector2Int.zero;
    //    float distance = float.MaxValue;

    //    foreach (Vector2Int position in roomCenters)
    //    {
    //        float currentDistance = Vector2.Distance(position, currentRoomCenter);

    //        if(currentDistance < distance)
    //        {
    //            distance = currentDistance;
    //            closest = position;
    //        }
    //    }

    //    return closest;
    //}

    private HashSet<Vector2Int> CreateRooms(List<BoundsInt> roomsList)
    {
        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();
        foreach (var room in roomsList)
        {
            for (int col = roomOffset; col < room.size.x - roomOffset; col++)
            {
                for (int row = roomOffset; row < room.size.y - roomOffset; row++)
                {
                    Vector2Int position = (Vector2Int)room.min + new Vector2Int(col, row);
                    floor.Add(position);
                }
            }
        }
        return floor;
    }

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
                if (Random.value < 0.5f)
                {
                    if (room.size.y >= minHeight * 2)
                    {
                        HorizontalSplit(minHeight, roomsQueue, room);
                    }
                    else if (room.size.x >= minWidth * 2)
                    {
                        VerticalSplit(minWidth, roomsQueue, room);
                    }
                    else if (room.size.x >= minWidth && room.size.y >= minHeight)
                    {
                        roomsList.Add(room);
                    }
                }
                else
                {
                    if (room.size.x >= minWidth * 2)
                    {
                        VerticalSplit(minWidth, roomsQueue, room);
                    }
                    else if (room.size.y >= minHeight * 2)
                    {
                        HorizontalSplit(minHeight, roomsQueue, room);
                    }
                    else if (room.size.x >= minWidth && room.size.y >= minHeight)
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

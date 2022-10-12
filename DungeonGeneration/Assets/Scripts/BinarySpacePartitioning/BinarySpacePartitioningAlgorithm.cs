using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BinarySpacePartitioningAlgorithm : MonoBehaviour
{
    [SerializeField] protected TilemapVisualizer tilemapVisualizer;

    [SerializeField] private int spaceWidth = 20,  spaceHeight = 20;
    [SerializeField] private int minRoomWidth = 4,  minRoomHeight = 4;
    [SerializeField] private Vector2Int startPosition = new Vector2Int(0, 0);

    [SerializeField] private int offset = 1;

    void Start()
    {
        List<BoundsInt> roomList = BinarySpacePartitioning(new BoundsInt((Vector3Int)startPosition, new Vector3Int(spaceWidth, spaceHeight, 0)), minRoomWidth, minRoomHeight);

        HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();

        floorPositions = CreateRooms(roomList);
    }

    private HashSet<Vector2Int> CreateRooms(List<BoundsInt> roomList)
    {
        HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();



        return floorPositions;
    }

    private List<BoundsInt> BinarySpacePartitioning(BoundsInt spaceToSplit, int minWidth, int minHeight)
    {
        Queue<BoundsInt> roomsQueue = new Queue<BoundsInt>(); //rooms that can be splitted
        List<BoundsInt> roomsList = new List<BoundsInt>();

        roomsQueue.Enqueue(spaceToSplit);

        while(roomsQueue.Count > 0)
        {
            BoundsInt room = roomsQueue.Dequeue();

            if(room.size.x >= minWidth && room.size.y >= minHeight)
            {
                if(Random.Range(0.0f,1.0f) < 0.5f)
                {
                    if(room.size.y >= minHeight*2) //Horizontal
                    {
                        HorizontalSplit(minHeight, roomsQueue, room); 
                    }
                    else if(room.size.x >= minWidth * 2)
                    {
                        VerticalSplit(minWidth, roomsQueue, room); //Vertical
                    }
                    else
                    {
                        roomsList.Add(room);
                    }
                }
                else
                {
                    if (room.size.x >= minWidth * 2)
                    {
                        VerticalSplit(minWidth, roomsQueue, room); //Vertical
                    }
                    if (room.size.y >= minHeight * 2) //Horizontal
                    {
                        HorizontalSplit(minHeight, roomsQueue, room); 
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

    private void VerticalSplit(int minWidth, Queue<BoundsInt> roomsQueue, BoundsInt room)
    {
        int xSplit = Random.Range(1, room.size.x);
        BoundsInt firstRoom = new BoundsInt(room.min, new Vector3Int(xSplit, room.size.y, room.size.z));
        BoundsInt secondRoom = new BoundsInt(new Vector3Int(room.min.x + xSplit,room.min.y, room.min.z), new Vector3Int(room.size.x - xSplit, room.size.y, room.size.z));

        roomsQueue.Enqueue(firstRoom);
        roomsQueue.Enqueue(secondRoom);
    }

    private void HorizontalSplit(int minHeight, Queue<BoundsInt> roomsQueue, BoundsInt room)
    {
        int ySplit = Random.Range(1, room.size.y);

        BoundsInt firstRoom = new BoundsInt(room.min, new Vector3Int(room.size.x, ySplit, room.size.z));
        BoundsInt secondRoom = new BoundsInt(new Vector3Int(room.min.x, room.min.y + ySplit, room.min.z), new Vector3Int(room.size.x, room.size.y-ySplit, room.size.z));

        roomsQueue.Enqueue(firstRoom);
        roomsQueue.Enqueue(secondRoom);
    }
}

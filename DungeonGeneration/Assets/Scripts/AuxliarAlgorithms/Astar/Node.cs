using UnityEngine;
using System.Collections;

//Class for representing each position of the Grid2D class
public class Node : IHeapItem<Node>
{
	public enum NodeType { None, Floor, Hallway }

	private bool walkable;
	private Vector2Int worldPosition;
	private int gridX;
	private int gridY;

	private int gCost;
	private int hCost;
	private Node parent;
	private int heapIndex;
	private NodeType nodeType;

	public Node(Vector2Int worldPos, int gridX, int gridY, bool walkable=true)
	{
		Walkable = walkable;
		WorldPosition = worldPos;
		GridX = gridX;
		GridY = gridY;
		AccessNodeType = NodeType.None;
	}

	public int FCost
	{
		get
		{
			return GCost + HCost;
		}
	}

	public int HeapIndex
	{
		get { return heapIndex; }
		set { heapIndex = value; }
	}

    public int GridX { get => gridX; set => gridX = value; }
    public int GridY { get => gridY; set => gridY = value; }
    public int GCost { get => gCost; set => gCost = value; }
    public int HCost { get => hCost; set => hCost = value; }
    public Node Parent { get => parent; set => parent = value; }
    public Vector2Int WorldPosition { get => worldPosition; set => worldPosition = value; }
    public bool Walkable { get => walkable; set => walkable = value; }
    public NodeType AccessNodeType { get => nodeType; set => nodeType = value; }

    public int CompareTo(Node nodeToCompare)
	{
		int compare = FCost.CompareTo(nodeToCompare.FCost);
		if (compare == 0)
		{
			compare = HCost.CompareTo(nodeToCompare.HCost);
		}
		return -compare;
	}
}

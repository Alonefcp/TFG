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
		GridNodeType = NodeType.None;
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

	/// <summary>
	/// Node grid X position
	/// </summary>
    public int GridX { get => gridX; set => gridX = value; }

	/// <summary>
	/// Node grid X position
	/// </summary>
	public int GridY { get => gridY; set => gridY = value; }

	/// <summary>
	/// Node g cost
	/// </summary>
	public int GCost { get => gCost; set => gCost = value; }

	/// <summary>
	/// Node h cost
	/// </summary>
	public int HCost { get => hCost; set => hCost = value; }

	/// <summary>
	/// Node's parent
	/// </summary>
	public Node Parent { get => parent; set => parent = value; }

	/// <summary>
	/// Node world position
	/// </summary>
    public Vector2Int WorldPosition { get => worldPosition; set => worldPosition = value; }

	/// <summary>
	/// If the node is walkable
	/// </summary>
    public bool Walkable { get => walkable; set => walkable = value; }

	/// <summary>
	/// Node type
	/// </summary>
    public NodeType GridNodeType { get => nodeType; set => nodeType = value; }

	/// <summary>
	/// Compares the f cost 
	/// </summary>
	/// <param name="nodeToCompare">Node to compare</param>
	/// <returns></returns>
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

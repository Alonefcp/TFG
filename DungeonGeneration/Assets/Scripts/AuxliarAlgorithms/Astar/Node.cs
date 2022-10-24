using UnityEngine;
using System.Collections;

public class Node : IHeapItem<Node>
{
	public enum NodeType { None, Floor, Hallway }

	public bool walkable;
	public Vector2Int worldPosition;
	public int gridX;
	public int gridY;

	public int gCost;
	public int hCost;
	public Node parent;
	private int heapIndex;
	private NodeType nodeType;

	public Node(Vector2Int _worldPos, int _gridX, int _gridY, bool _walkable=true)
	{
		walkable = _walkable;
		worldPosition = _worldPos;
		gridX = _gridX;
		gridY = _gridY;
		nodeType = NodeType.None;
	}


	public NodeType GetNodeType()
	{
		return nodeType;
	}

	public void SetType(NodeType _nodeType)
	{
		nodeType = _nodeType;
	}

	public void SetIsWalkable(bool b)
    {
		walkable = b;
    }

	public int fCost
	{
		get
		{
			return gCost + hCost;
		}
	}

	public int HeapIndex
	{
		get
		{
			return heapIndex;
		}
		set
		{
			heapIndex = value;
		}
	}

	public int CompareTo(Node nodeToCompare)
	{
		int compare = fCost.CompareTo(nodeToCompare.fCost);
		if (compare == 0)
		{
			compare = hCost.CompareTo(nodeToCompare.hCost);
		}
		return -compare;
	}
}

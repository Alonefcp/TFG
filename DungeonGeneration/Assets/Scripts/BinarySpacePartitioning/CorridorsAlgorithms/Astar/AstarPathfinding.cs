using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class AstarPathfinding 
{
	public static HashSet<Vector2Int> FindPath(Grid grid, Vector3 startPos, Vector3 targetPos)
	{
		Node startNode = grid.NodeFromWorldPoint(startPos);
		Node targetNode = grid.NodeFromWorldPoint(targetPos);

		Heap<Node> openSet = new Heap<Node>(grid.MaxSize);
		HashSet<Node> closedSet = new HashSet<Node>();
		openSet.Add(startNode);

		while (openSet.Count > 0)
		{
			Node currentNode = openSet.RemoveFirst();
			closedSet.Add(currentNode);

			if (currentNode == targetNode)
			{
				return RetracePath(startNode, targetNode, grid);
			}

			foreach (Node neighbour in grid.GetNeighbours(currentNode))
			{
				if (!neighbour.walkable || closedSet.Contains(neighbour))
				{
					continue;
				}

				int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
				int extra = 0;
                if (grid.NodeFromWorldPoint(neighbour.worldPosition).GetNodeType() == Node.NodeType.Floor)
                {
                    extra += 10;
                }
                else if (grid.NodeFromWorldPoint(neighbour.worldPosition).GetNodeType() == Node.NodeType.Hallway)
                {
                    extra += 5;
                }
                else
                {
                    extra += 1;
                }

                if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
				{
					neighbour.gCost = newMovementCostToNeighbour;
					neighbour.hCost = GetDistance(neighbour, targetNode)+extra;
					neighbour.parent = currentNode;

					if (!openSet.Contains(neighbour))
						openSet.Add(neighbour);
					else
					{
						openSet.UpdateItem(neighbour);
					}
				}
			}
		}

		return null;
	}

	private static HashSet<Vector2Int> RetracePath(Node startNode, Node endNode, Grid grid)
	{
		HashSet<Vector2Int> path = new HashSet<Vector2Int>();
		Node currentNode = endNode;

		while (currentNode != startNode)
		{
			path.Add((Vector2Int)Vector3Int.RoundToInt(currentNode.worldPosition));
			currentNode = currentNode.parent;
			if(currentNode.GetNodeType()!=Node.NodeType.Floor)grid.NodeFromWorldPoint(currentNode.worldPosition).SetType(Node.NodeType.Hallway);
		}
		//path.Reverse();

		return path;
	}

	private static int GetDistance(Node nodeA, Node nodeB)
	{
		int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
		int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

		if (dstX > dstY)
			return 14 * dstY + 10 * (dstX - dstY);
		return 14 * dstX + 10 * (dstY - dstX);
	}
}
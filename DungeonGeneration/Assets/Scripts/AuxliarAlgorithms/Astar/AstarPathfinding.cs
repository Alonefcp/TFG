using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//A* algorithm for finding a path
public static class AstarPathfinding 
{
	/// <summary>
	/// Returns the shortest path between two positions
	/// </summary>
	/// <param name="grid">Map grid for A*</param>
	/// <param name="startPos">Start position of teh path</param>
	/// <param name="targetPos">End position of the path</param>
	/// <returns>A hashset with all path positions</returns>
	public static HashSet<Vector2Int> FindPath(Grid2D grid, Vector2Int startPos, Vector2Int targetPos)
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
				if (!neighbour.Walkable || closedSet.Contains(neighbour))
				{
					continue;
				}

				int newMovementCostToNeighbour = currentNode.GCost + GetDistance(currentNode, neighbour);
				int extra = 0;
                if (grid.NodeFromWorldPoint(neighbour.WorldPosition).AccessNodeType == Node.NodeType.Floor)
                {
                    extra += 10;
                }
                else if (grid.NodeFromWorldPoint(neighbour.WorldPosition).AccessNodeType == Node.NodeType.Hallway)
                {
                    extra += 5;
                }
                else
                {
                    extra += 1;
                }

                if (newMovementCostToNeighbour < neighbour.GCost || !openSet.Contains(neighbour))
				{
					neighbour.GCost = newMovementCostToNeighbour;
					neighbour.HCost = GetDistance(neighbour, targetNode)+extra;
					neighbour.Parent = currentNode;

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

	/// <summary>
	/// Creates the shortest path between two positions
	/// </summary>
	/// <param name="startNode">Start node position</param>
	/// <param name="endNode">End node position</param>
	/// <param name="grid">Map grid for A*</param>
	/// <returns>A hashset with all path positions</returns>
	private static HashSet<Vector2Int> RetracePath(Node startNode, Node endNode, Grid2D grid)
	{
		HashSet<Vector2Int> path = new HashSet<Vector2Int>();
		Node currentNode = endNode;

		while (currentNode != startNode)
		{
			path.Add(currentNode.WorldPosition);
			currentNode = currentNode.Parent;
			if(currentNode.AccessNodeType != Node.NodeType.Floor)grid.NodeFromWorldPoint(currentNode.WorldPosition).AccessNodeType = Node.NodeType.Hallway;
		}

		path.Add(startNode.WorldPosition);
		return path;
	}

	/// <summary>
	/// Calculate the distance between two nodes
	/// </summary>
	/// <param name="firstNode">First node</param>
	/// <param name="secondNode">Second node</param>
	/// <returns>The distance between two nodes</returns>
	private static int GetDistance(Node firstNode, Node secondNode)
	{
		int moveDiagonalCost = 14;
		int moveHorizontalAndVerticalCost = 10;

		int dstX = Mathf.Abs(firstNode.GridX - secondNode.GridX);
		int dstY = Mathf.Abs(firstNode.GridY - secondNode.GridY);

		if (dstX > dstY)
			return moveDiagonalCost * dstY + moveHorizontalAndVerticalCost * (dstX - dstY);
		return moveDiagonalCost * dstX + moveHorizontalAndVerticalCost * (dstY - dstX);
	}
}
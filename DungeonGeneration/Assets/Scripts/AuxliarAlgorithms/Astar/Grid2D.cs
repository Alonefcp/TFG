using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Grid2D
{
	private Node[,] grid;
	private float nodeDiameter;

	private int gridSizeX, gridSizeY;

	public Grid2D(int gridWorldSizeX, int gridWorldSizeY, float nodeRadius)
    {
		nodeDiameter = nodeRadius * 2;
		gridSizeX = Mathf.RoundToInt(gridWorldSizeX / nodeDiameter);
		gridSizeY = Mathf.RoundToInt(gridWorldSizeY / nodeDiameter);

		grid = new Node[gridSizeX, gridSizeY];

		for (int x = 0; x < gridSizeX; x++)
		{
			for (int y = 0; y < gridSizeY; y++)
			{
				Vector2Int worldPoint = new Vector2Int(x, y);
				grid[x, y] = new Node(worldPoint, x, y);
			}
		}
	}

	public int MaxSize
	{
		get{ return gridSizeX * gridSizeY; }
					
	}

	//public void CreateGrid(int gridWorldSizeX, int gridWorldSizeY, float nodeRadius)
	//{
	//	nodeDiameter = nodeRadius * 2;
	//	gridSizeX = Mathf.RoundToInt(gridWorldSizeX / nodeDiameter);
	//	gridSizeY = Mathf.RoundToInt(gridWorldSizeY / nodeDiameter);

	//	grid = new Node[gridSizeX, gridSizeY];
		
	//	for (int x = 0; x < gridSizeX; x++)
	//	{
	//		for (int y = 0; y < gridSizeY; y++)
	//		{
	//			Vector3 worldPoint = new Vector3(x, y, 0);
	//			grid[x, y] = new Node(worldPoint, x, y);
	//		}
	//	}
	//}
	

	public List<Node> GetNeighbours(Node node)
	{
		List<Node> neighbours = new List<Node>();

        foreach (var pos in GetDirectionsArray())
        {
			if (pos.x == 0 && pos.y == 0)
                continue;

            int checkX = node.gridX + pos.x;
            int checkY = node.gridY + pos.y;

            if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
            {
                neighbours.Add(grid[checkX, checkY]);
            }
        }
		return neighbours;
	}


	public Node NodeFromWorldPoint(Vector2Int worldPosition)
	{
		return grid[worldPosition.x, worldPosition.y];
	}

	private Vector2Int[] GetDirectionsArray()
	{
		Vector2Int[] directions = { new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1)};

		return directions;
	}
}
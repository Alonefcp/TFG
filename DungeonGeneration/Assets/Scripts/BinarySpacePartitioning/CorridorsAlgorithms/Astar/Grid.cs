using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Grid : MonoBehaviour
{
	//[SerializeField] public bool onlyDisplayPathGizmos;
	private Node[,] grid;
	private float nodeDiameter;

	private int gridSizeX, gridSizeY;


	public int MaxSize
	{
		get
		{
			return gridSizeX * gridSizeY;
		}
	}

	public void CreateGrid(int gridWorldSizeX, int gridWorldSizeY, float nodeRadius)
	{
		nodeDiameter = nodeRadius * 2;
		gridSizeX = Mathf.RoundToInt(gridWorldSizeX / nodeDiameter);
		gridSizeY = Mathf.RoundToInt(gridWorldSizeY / nodeDiameter);

		grid = new Node[gridSizeX, gridSizeY];
		
		for (int x = 0; x < gridSizeX; x++)
		{
			for (int y = 0; y < gridSizeY; y++)
			{
				Vector3 worldPoint = new Vector3(x, y, 0);
				grid[x, y] = new Node(true, worldPoint, x, y);
			}
		}
	}

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


		//for (int x = -1; x <= 1; x++)
		//{
		//	for (int y = -1; y <= 1; y++)
		//	{
		//		if (x == 0 && y == 0)
		//			continue;

		//		int checkX = node.gridX + x;
		//		int checkY = node.gridY + y;

		//		if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
		//		{
		//			neighbours.Add(grid[checkX, checkY]);
		//		}
		//	}
		//}

		return neighbours;
	}


	public Node NodeFromWorldPoint(Vector3 worldPosition)
	{
		return grid[(int)worldPosition.x, (int)worldPosition.y];
	}

	private Vector2Int[] GetDirectionsArray()
	{
		Vector2Int[] directions = { new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1)};

		return directions;
	}
    //void OnDrawGizmos()
    //{
    //    Gizmos.DrawWireCube(new Vector3(gridWorldSize.x - gridWorldSize.x / 2, gridWorldSize.y - gridWorldSize.y / 2, 0), new Vector3(gridWorldSize.x, gridWorldSize.y, 0));
    //}
}
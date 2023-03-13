using System.Collections.Generic;
using UnityEngine;

public class Pathfinding
{
#region Fields
	private const int DIAGONAL_WEIGHT = 14;
	private const int STRAIGHT_LINES_WEIGHT = 10;
	private readonly List<MyTileData> grid;
	private List<MyTileData> openList;
	private List<MyTileData> closedList;
	private bool _isFlyingTowards;
#endregion

#region Methods
	public Pathfinding()
	{
		grid = GridManager.Shared.GetGrid();
	}

	public List<MyTileData> FindPath(int startX, int startY, int endX, int endY, bool isFlying)
	{
		_isFlyingTowards = isFlying;
		if (startY >= GridManager.Shared.GetYTileAmount() || startX >= GridManager.Shared.GetXTileAmount())
		{
			return null;
		}
		MyTileData startNode = grid[GridManager.Shared.GetGridIndexFromPoint(startX, startY)];
		MyTileData endNode = grid[GridManager.Shared.GetGridIndexFromPoint(endX, endY)];
		openList = new List<MyTileData> { startNode };
		closedList = new List<MyTileData>();
		for (int x = 0; x < GridManager.Shared.GetXTileAmount(); x++)
		{
			for (int y = 0; y < GridManager.Shared.GetYTileAmount(); y++)
			{
				MyTileData pathNode = grid[GridManager.Shared.GetGridIndexFromPoint(x, y)];
				pathNode.SetGCost(int.MaxValue);
				pathNode.CalculateFCost();
				pathNode.SetCameFrom(null);
			}
		}
		startNode.SetGCost(0);
		startNode.SetHCost(CalculateDistance(startNode, endNode));
		startNode.CalculateFCost();
		while (openList.Count > 0)
		{
			MyTileData currentNode = GetLowestFCostNode(openList);
			if (currentNode == endNode)
			{
				return CalculatePath(endNode);
			}
			openList.Remove(currentNode);
			closedList.Add(currentNode);
			foreach (MyTileData neighbour in GetNeighbourList(currentNode))
			{
				if (closedList.Contains(neighbour)) continue;
				int tentativeGCost = currentNode.GetGCost() + CalculateDistance(currentNode, neighbour);
				if (tentativeGCost < neighbour.GetGCost())
				{
					neighbour.SetCameFrom(currentNode);
					neighbour.SetGCost(tentativeGCost);
					neighbour.SetHCost(CalculateDistance(neighbour, endNode));
					neighbour.CalculateFCost();
					if (!openList.Contains(neighbour))
					{
						openList.Add(neighbour);
					}
				}
			}
		}
		return null;
	}

	private List<MyTileData> GetNeighbourList(MyTileData currentNode)
	{
		List<MyTileData> neighbourList = new List<MyTileData>();
		if ((currentNode.GetX() - 1 >= 0 && currentNode.IsDugWest()) ||
		    (currentNode.GetX() - 1 >= 0 && _isFlyingTowards)) // left
		{
			neighbourList.Add(
				grid[GridManager.Shared.GetGridIndexFromPoint(currentNode.GetX() - 1, currentNode.GetY())]);
		}
		if (currentNode.GetX() - 1 >= 0 && _isFlyingTowards)
		{
			if (currentNode.GetY() + 1 < GridManager.Shared.GetYTileAmount()) // Left up direction
			{
				neighbourList.Add(grid[
					GridManager.Shared.GetGridIndexFromPoint(currentNode.GetX() - 1, currentNode.GetY() + 1)]);
			}
			if (currentNode.GetY() - 1 >= 0) // Left down direction
			{
				neighbourList.Add(grid[
					GridManager.Shared.GetGridIndexFromPoint(currentNode.GetX() - 1, currentNode.GetY() - 1)]);
			}
		}
		if ((currentNode.GetX() + 1 < GridManager.Shared.GetXTileAmount() && currentNode.IsDugEast()) ||
		    (currentNode.GetX() + 1 < GridManager.Shared.GetXTileAmount() && _isFlyingTowards)) // right
		{
			neighbourList.Add(
				grid[GridManager.Shared.GetGridIndexFromPoint(currentNode.GetX() + 1, currentNode.GetY())]);
		}
		if (currentNode.GetX() + 1 < GridManager.Shared.GetXTileAmount() && _isFlyingTowards)
		{
			if (currentNode.GetY() + 1 < GridManager.Shared.GetYTileAmount()) // Right up direction
			{
				neighbourList.Add(grid[
					GridManager.Shared.GetGridIndexFromPoint(currentNode.GetX() + 1, currentNode.GetY() + 1)]);
			}
			if (currentNode.GetY() - 1 >= 0) // Right down direction
			{
				neighbourList.Add(grid[
					GridManager.Shared.GetGridIndexFromPoint(currentNode.GetX() + 1, currentNode.GetY() - 1)]);
			}
		}
		if ((currentNode.GetY() - 1 >= 0 && currentNode.IsDugSouth()) ||
		    (currentNode.GetY() - 1 >= 0 && _isFlyingTowards)) // down
		{
			neighbourList.Add(
				grid[GridManager.Shared.GetGridIndexFromPoint(currentNode.GetX(), currentNode.GetY() - 1)]);
		}
		if (currentNode.GetY() - 1 >= 0 && _isFlyingTowards)
		{
			if (currentNode.GetX() + 1 < GridManager.Shared.GetYTileAmount()) // Down right direction
			{
				neighbourList.Add(grid[
					GridManager.Shared.GetGridIndexFromPoint(currentNode.GetX() + 1, currentNode.GetY() - 1)]);
			}
			if (currentNode.GetX() - 1 >= 0) // Down left direction
			{
				neighbourList.Add(grid[
					GridManager.Shared.GetGridIndexFromPoint(currentNode.GetX() - 1, currentNode.GetY() - 1)]);
			}
		}
		if ((currentNode.GetY() + 1 < GridManager.Shared.GetYTileAmount() && currentNode.IsDugNorth()) ||
		    (currentNode.GetY() + 1 < GridManager.Shared.GetYTileAmount() && _isFlyingTowards)) // up
		{
			neighbourList.Add(
				grid[GridManager.Shared.GetGridIndexFromPoint(currentNode.GetX(), currentNode.GetY() + 1)]);
		}
		if (currentNode.GetY() + 1 < GridManager.Shared.GetYTileAmount() && _isFlyingTowards)
		{
			if (currentNode.GetX() + 1 < GridManager.Shared.GetYTileAmount()) // Up right direction
			{
				neighbourList.Add(grid[
					GridManager.Shared.GetGridIndexFromPoint(currentNode.GetX() + 1, currentNode.GetY() + 1)]);
			}
			if (currentNode.GetX() - 1 >= 0) // Up left direction
			{
				neighbourList.Add(grid[
					GridManager.Shared.GetGridIndexFromPoint(currentNode.GetX() - 1, currentNode.GetY() + 1)]);
			}
		}
		return neighbourList;
	}

	private List<MyTileData> CalculatePath(MyTileData endNode)
	{
		List<MyTileData> path = new List<MyTileData> { endNode };
		MyTileData currentNode = endNode;
		while (currentNode.GetCameFrom() != null)
		{
			path.Add(currentNode.GetCameFrom());
			currentNode = currentNode.GetCameFrom();
		}
		path.Reverse();
		return path;
	}

	private MyTileData GetLowestFCostNode(List<MyTileData> pathNodeList)
	{
		MyTileData lowestFCostNode = pathNodeList[0];
		for (int i = 1; i < pathNodeList.Count; i++)
		{
			if (pathNodeList[i].GetFCost() < lowestFCostNode.GetFCost())
			{
				lowestFCostNode = pathNodeList[i];
			}
		}
		return lowestFCostNode;
	}

	private int CalculateDistance(MyTileData a, MyTileData b)
	{
		int xDistance = Mathf.Abs(a.GetX() - b.GetX());
		int yDistance = Mathf.Abs(a.GetY() - b.GetY());
		int remaining = Mathf.Abs(xDistance - yDistance);
		if (_isFlyingTowards)
		{
			return DIAGONAL_WEIGHT * Mathf.Min(xDistance, yDistance) + STRAIGHT_LINES_WEIGHT * remaining;
		}
		else
		{
			return STRAIGHT_LINES_WEIGHT * remaining;
		}
	}
#endregion
}
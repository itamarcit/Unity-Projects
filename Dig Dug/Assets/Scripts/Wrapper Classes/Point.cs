using System.Collections.Generic;

public struct Point
{
	public readonly int X;
	public readonly int Y;
	public readonly bool IsWestCorner;
	public readonly bool IsEastCorner;
	public readonly bool IsNorthCorner;
	public readonly bool IsSouthCorner;

	public Point(int x, int y, bool isNorthCorner = false, bool isSouthCorner = false, bool isWestCorner = false,
				 bool isEastCorner = false)

	{
		this.X = x;
		this.Y = y;
		this.IsNorthCorner = isNorthCorner;
		this.IsSouthCorner = isSouthCorner;
		this.IsWestCorner = isWestCorner;
		this.IsEastCorner = isEastCorner;
	}

	// Returns the first point in the given list that matches given x, y. Null if not found
	public static Point? FindPoint(List<Point> listPoints, int x, int y)
	{
		foreach (Point point in listPoints)
		{
			if (point.X == x && point.Y == y)
			{
				return point;
			}
		}
		return null;
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class GridManager : MonoBehaviour
{
#region Fields
	[SerializeField] private float gridSpacingX;
	[SerializeField] private float gridSpacingY;
	[SerializeField] private Vector3 bottomLeftCorner;
	[SerializeField] private int xSquares;
	[SerializeField] private int ySquares;
	private const int MAX_FIND_ITERATIONS = 1000;
	private const float GIZMO_DURATION_SECONDS = 5f;
	private const float SQUARE_LENGTH = 0.5f;
	private const int MAX_DISTANCE_FROM_PLAYER = 2;
	private const int POWER_OF_TWO = 2;
	private const int GRID_DELTA = 2;
	private List<MyTileData> tileData;
	private readonly List<MyTileData> dugTileData = new();
	private readonly List<Point> _dugOnStartLeft = new()
	{
		// TOP ROW
		new Point(0, 13),
		new Point(1, 13),
		new Point(2, 13),
		new Point(3, 13),
		new Point(4, 13),
		new Point(5, 13),
		new Point(6, 13),
		new Point(7, 13),
		new Point(8, 13),
		new Point(9, 13),
		new Point(10, 13),
		new Point(11, 13),
		new Point(12, 13),
		new Point(13, 13),
		new Point(14, 13),
		new Point(15, 13),
		new Point(16, 13),
		new Point(17, 13),
		new Point(18, 13),
		new Point(19, 13),
		new Point(20, 13),
		new Point(21, 13),
		new Point(22, 13),
		new Point(23, 13),
		new Point(24, 13),
		new Point(25, 13),
		new Point(26, 13),
		new Point(27, 13),
		// START PLAYER TUNNEL
		new Point(13, 7, isWestCorner: true),
		new Point(14, 7),
		new Point(15, 7, isEastCorner: true),
		// TOP RIGHT TUNNEL
		new Point(20, 11, isWestCorner: true),
		new Point(21, 11),
		new Point(22, 11),
		new Point(23, 11, isEastCorner: true),
		// BOTTOM LEFT TUNNEL
		new Point(9, 4, isWestCorner: true),
		new Point(10, 4),
		new Point(11, 4),
		new Point(12, 4),
		new Point(13, 4, isEastCorner: true),
	};
	private readonly List<Point> _dugOnStartUp = new()
	{
		// TOP ROW
		new Point(14, 13),
		// START PLAYER TUNNEL
		new Point(14, 7, isSouthCorner: true),
		new Point(14, 8),
		new Point(14, 9),
		new Point(14, 10),
		new Point(14, 11),
		new Point(14, 12),
		new Point(14, 13),
		// TOP LEFT TUNNEL
		new Point(4, 8, isSouthCorner: true),
		new Point(4, 9),
		new Point(4, 10),
		new Point(4, 11, isNorthCorner: true),
		// BOTTOM RIGHT TUNNEL
		new Point(20, 5, isNorthCorner: true),
		new Point(20, 4),
		new Point(20, 3),
		new Point(20, 2),
		new Point(20, 1, isSouthCorner: true),
	};
	private readonly List<Point> _rocksPositions = new();
	private Pathfinding _pathfinding;
	public static GridManager Shared { get; private set; }
#endregion

#region Enums
	public enum Direction
	{
		North,
		South,
		West,
		East
	}
#endregion

#region Events
	private void Awake()
	{
		if (Shared == null)
		{
			Shared = this;
		}
		else
		{
			Destroy(gameObject);
		}
		tileData = new List<MyTileData>();
		for (int i = 0; i < ySquares; i++)
		{
			for (int j = 0; j < xSquares; j++)
			{
				MyTileData tileToBeAdded = CreateTileData(j, i);
				if (tileToBeAdded.IsDug())
				{
					dugTileData.Add(tileToBeAdded);
				}
				tileData.Add(tileToBeAdded);
			}
		}
	}
#endregion

#region Methods
	private MyTileData CreateTileData(int x, int y)
	{
		bool dugNorth = false, dugSouth = false, dugWest = false, dugEast = false;
		Point? isDugUp = Point.FindPoint(_dugOnStartUp, x, y);
		Point? isDugLeft = Point.FindPoint(_dugOnStartLeft, x, y);
		if (isDugUp != null)
		{
			if (!isDugUp.Value.IsNorthCorner)
			{
				dugNorth = true;
			}
			if (!isDugUp.Value.IsSouthCorner)
			{
				dugSouth = true;
			}
		}
		if (isDugLeft != null)
		{
			if (!isDugLeft.Value.IsWestCorner)
			{
				dugWest = true;
			}
			if (!isDugLeft.Value.IsEastCorner)
			{
				dugEast = true;
			}
		}
		return new MyTileData(x, y, dugNorth, dugSouth, dugWest, dugEast);
	}

	public float GetGridSpacingX()
	{
		return gridSpacingX;
	}

	public float GetGridSpacingY()
	{
		return gridSpacingY;
	}

	public bool IsInRangeAfterMovement(Vector2Int currentPosition, Vector2Int direction)
	{
		Vector2 newPosition = currentPosition + direction;
		if (_rocksPositions.Contains(new Point((int)newPosition.x, (int)newPosition.y)))
		{
			return false;
		}
		return !(newPosition.x < 0) && !(newPosition.x > xSquares - 1) && !(newPosition.y < 0) &&
			   !(newPosition.y > ySquares - 1);
	}

	public bool IsGridSquareDug(int x, int y, Direction fromDirection)
	{
		if (y < 0 || x < 0 || x >= xSquares || y >= ySquares) return false;
		switch (fromDirection)
		{
			case Direction.North:
				return tileData[GetGridIndexFromPoint(x, y)].IsDugNorth();
			case Direction.South:
				return tileData[GetGridIndexFromPoint(x, y)].IsDugSouth();
			case Direction.West:
				return tileData[GetGridIndexFromPoint(x, y)].IsDugWest();
			default: // Direction.East
				return tileData[GetGridIndexFromPoint(x, y)].IsDugEast();
		}
	}

	public bool IsGridSquareDug(int x, int y, Vector2Int fromDirection)
	{
		if (y < 0 || x < 0 || x >= xSquares || y >= ySquares) return false;
		if (fromDirection == Vector2Int.up) return tileData[GetGridIndexFromPoint(x, y)].IsDugNorth();
		if (fromDirection == Vector2Int.down) return tileData[GetGridIndexFromPoint(x, y)].IsDugSouth();
		if (fromDirection == Vector2Int.left) return tileData[GetGridIndexFromPoint(x, y)].IsDugWest();
		return tileData[GetGridIndexFromPoint(x, y)].IsDugEast();
	}

	// Will return true if any of the directions in fromDirection is dug.
	public bool IsGridSquareDug(int x, int y, Direction[] fromDirection)
	{
		if (y < 0) return false;
		MyTileData tile = MyTileData.FindTileData(tileData, x, y);
		if (fromDirection.Contains(Direction.South) && tile.IsDugSouth()) return true;
		if (fromDirection.Contains(Direction.West) && tile.IsDugWest()) return true;
		if (fromDirection.Contains(Direction.East) && tile.IsDugEast()) return true;
		if (fromDirection.Contains(Direction.North) && tile.IsDugNorth()) return true;
		return false;
	}

	public bool IsGridSquareDugNorth(int x, int y)
	{
		if (y < 0) return false;
		return tileData[GetGridIndexFromPoint(x, y)].IsDugNorth();
	}

	public void DigSquare(int oldX, int oldY, int newX, int newY, Direction direction)
	{
		MyTileData movedToTile;
		Action digNewTile;
		switch (direction)
		{
			case Direction.North:
				tileData[GetGridIndexFromPoint(oldX, oldY)].DigTileNorth();
				movedToTile = tileData[GetGridIndexFromPoint(newX, newY)];
				digNewTile = () => movedToTile.DigTileSouth();
				break;
			case Direction.South:
				tileData[GetGridIndexFromPoint(oldX, oldY)].DigTileSouth();
				movedToTile = tileData[GetGridIndexFromPoint(newX, newY)];
				digNewTile = () => movedToTile.DigTileNorth();
				break;
			case Direction.West:
				tileData[GetGridIndexFromPoint(oldX, oldY)].DigTileWest();
				movedToTile = tileData[GetGridIndexFromPoint(newX, newY)];
				digNewTile = () => movedToTile.DigTileEast();
				break;
			default: // Direction.East
				tileData[GetGridIndexFromPoint(oldX, oldY)].DigTileEast();
				movedToTile = tileData[GetGridIndexFromPoint(newX, newY)];
				digNewTile = () => movedToTile.DigTileWest();
				break;
		}
		if (!movedToTile.IsDug())
		{
			dugTileData.Add(movedToTile);
		}
		digNewTile.Invoke();
	}

	public void RegisterRock(int x, int y)
	{
		_rocksPositions.Add(new Point(x, y));
	}

	public void DeregisterRock(int x, int y)
	{
		_rocksPositions.RemoveAll(point => point.X == x && point.Y == y);
	}

	public Vector2Int GetStartLocationGrid()
	{
		return new Vector2Int(xSquares / GRID_DELTA, ySquares / GRID_DELTA);
	}

	public int GetXTileAmount()
	{
		return xSquares;
	}

	public int GetYTileAmount()
	{
		return ySquares;
	}

	public int GetGridIndexFromPoint(int x, int y)
	{
		return x + xSquares * y;
	}

	// Gets the direction the player came from if he is heading in the direction of vec.
	// Vec should have only one of x,y as 1 and the rest as 0.
	public static Direction GetDirectionFromVector(Vector3Int vec)
	{
		if (vec.x == 0)
		{
			if (vec.y > 0)
			{
				return Direction.North;
			}
			return Direction.South;
		}
		if (vec.x > 0)
		{
			return Direction.East;
		}
		return Direction.West;
	}

	public Vector3 GetWorldPosition(int x, int y)
	{
		return bottomLeftCorner + Vector3.up * (y * gridSpacingY) + Vector3.right * (x * gridSpacingX);
	}

	public List<MyTileData> GetGrid()
	{
		return tileData;
	}

	// coordinates are in grid format
	public List<MyTileData> FindPath(int yourX, int yourY, int targetX, int targetY, bool isFlying)
	{
		_pathfinding = new Pathfinding();
		List<MyTileData> path = _pathfinding.FindPath(yourX, yourY, targetX, targetY, isFlying);
		return path;
	}

	public Vector2Int GetOppositeDirection(Vector2Int direction)
	{
		if (direction == Vector2Int.up) return Vector2Int.down;
		if (direction == Vector2Int.down) return Vector2Int.up;
		if (direction == Vector2Int.left) return Vector2Int.right;
		if (direction == Vector2Int.right) return Vector2Int.left;
		return Vector2Int.zero;
	}

	public Direction GetOppositeDirection(Direction direction)
	{
		switch (direction)
		{
			case Direction.North:
				return Direction.South;
			case Direction.South:
				return Direction.North;
			case Direction.East:
				return Direction.West;
			default:
				return Direction.East;
		}
	}

	public Vector2Int GetRandomEmptyGridSquare()
	{
		Vector2Int result = new Vector2Int();
		MyTileData randomDugTile = null;
		for (int i = 0; i < MAX_FIND_ITERATIONS; i++)
		{
			int randomDugTileIndex = Random.Range(0, dugTileData.Count);
			randomDugTile = dugTileData[randomDugTileIndex];
			result.x = randomDugTile.GetX();
			result.y = randomDugTile.GetY();
			try
			{
				// check if there's a rock at location, if no .First throws exception
				_rocksPositions.First(point => point.X == result.x && point.Y == result.y);
			}
			catch(Exception)
			{
				break;
			}
			randomDugTile = null;
		}
		if (randomDugTile == null)
		{
			return new Vector2Int(-1, -1);
		}
		return result;
	}

	public Vector2Int GetRandomRockLocation()
	{
		int index = Random.Range(0, tileData.Count);
		int x = tileData[index].GetX();
		int y = tileData[index].GetY();
		for (int i = 0; i < MAX_FIND_ITERATIONS; i++)
		{
			if (CanSpawnRockInPoint(x, y - 1))
			{
				return new Vector2Int(x, y);
			}
			index = Random.Range(0, tileData.Count);
			x = tileData[index].GetX();
			y = tileData[index].GetY();
		}
		return new Vector2Int(-1, -1);
	}

	private bool CanSpawnRockInPoint(int x, int y)
	{
		if (y < 0) return false;
		List<Vector2Int> playerPos = GameManager.Shared.GetPlayerGridPosition();
		if (tileData[GetGridIndexFromPoint(x, y)].IsDugAnywhere() ||
		    tileData[GetGridIndexFromPoint(x, y + 1)].IsRock() ||
		    Mathf.Sqrt(Mathf.Pow(x - playerPos[0].x, POWER_OF_TWO) + Mathf.Pow(y - playerPos[0].y, POWER_OF_TWO)) <
		    MAX_DISTANCE_FROM_PLAYER || // Checks the player's origin location
		    Mathf.Sqrt(Mathf.Pow(x - playerPos[1].x, POWER_OF_TWO) + Mathf.Pow(y - playerPos[1].y, POWER_OF_TWO)) <
		    MAX_DISTANCE_FROM_PLAYER) // Checks the player's target location
		{
			return false;
		}
		return true;
	}

	public bool IsSquareWithRock(int x, int y)
	{
		return _rocksPositions.Contains(new Point(x, y));
	}
#endregion
}
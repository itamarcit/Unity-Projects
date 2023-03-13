using System.Collections.Generic;

public class MyTileData
{
	private readonly int x;
	private readonly int y;
	private bool dugNorth;
	private bool dugSouth;
	private bool dugWest;
	private bool dugEast;
	private int _gCost;
	private int _hCost;
	private int _fCost;
	private MyTileData _cameFrom;

	public MyTileData(int x, int y, bool dugNorth, bool dugSouth, bool dugWest, bool dugEast)
	{
		this.x = x;
		this.y = y;
		this.dugNorth = dugNorth;
		this.dugSouth = dugSouth;
		this.dugWest = dugWest;
		this.dugEast = dugEast;
	}

	public int GetGCost()
	{
		return _gCost;
	}
	
	public void SetGCost(int gCost)
	{
		_gCost = gCost;
	}
	
	public int GetHCost()
	{
		return _hCost;
	}
	
	public void SetHCost(int hCost)
	{
		_hCost = hCost;
	}
	
	public int GetFCost()
	{
		return _fCost;
	}
	
	public void SetFCost(int fCost)
	{
		_fCost = fCost;
	}
	
	public MyTileData GetCameFrom()
	{
		return _cameFrom;
	}

	public void SetCameFrom(MyTileData data)
	{
		_cameFrom = data;
	}

	public static MyTileData FindTileData(List<MyTileData> tileData, int x, int y)
	{
		foreach (MyTileData data in tileData)
		{
			if (data.x == x && data.y == y) return data;
		}
		return null;
	}

	public bool IsDugNorth()
	{
		return dugNorth && !IsRock();
	}

	public bool IsDugSouth()
	{
		return dugSouth && !IsRock();
	}

	public bool IsDugWest()
	{
		return dugWest && !IsRock();
	}

	public bool IsDugEast()
	{
		return dugEast && !IsRock();
	}

	public bool IsDug()
	{
		return dugEast || dugNorth || dugSouth || dugWest;
	}

	public void DigTileNorth()
	{
		dugNorth = true;
	}

	public void DigTileSouth()
	{
		dugSouth = true;
	}

	public void DigTileWest()
	{
		dugWest = true;
	}

	public void DigTileEast()
	{
		dugEast = true;
	}

	public int GetX()
	{
		return x;
	}
	public int GetY()
	{
		return y;
	}
	public override string ToString()
	{
		return x + "," + y;
	}
	public void CalculateFCost()
	{
		_fCost = _gCost + _hCost;
	}

	public bool IsRock()
	{
		return GridManager.Shared.IsSquareWithRock(x, y);
	}

	public bool IsDugAnywhere()
	{
		return dugNorth || dugSouth || dugWest || dugEast;
	}
}
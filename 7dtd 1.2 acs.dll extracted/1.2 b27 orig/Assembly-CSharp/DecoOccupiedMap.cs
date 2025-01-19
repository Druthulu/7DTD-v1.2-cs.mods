using System;
using UnityEngine;

public class DecoOccupiedMap
{
	public DecoOccupiedMap(int _worldWidth, int _worldHeight)
	{
		this.width = _worldWidth;
		this.height = _worldHeight;
		this.widthHalf = this.width / 2;
		this.heightHalf = this.height / 2;
		this.occupiedMap = new EnumDecoOccupied[this.width * this.height];
	}

	public EnumDecoOccupied Get(int _offs)
	{
		return this.occupiedMap[_offs];
	}

	public EnumDecoOccupied Get(int _x, int _z)
	{
		int num;
		if ((num = DecoManager.CheckPosition(this.width, this.height, _x, _z)) < 0)
		{
			return EnumDecoOccupied.NoneAllowed;
		}
		return this.occupiedMap[num];
	}

	public void Set(int _offs, EnumDecoOccupied _v)
	{
		this.occupiedMap[_offs] = _v;
	}

	public void Set(int _x, int _z, EnumDecoOccupied _v)
	{
		int num;
		if ((num = DecoManager.CheckPosition(this.width, this.height, _x, _z)) < 0)
		{
			return;
		}
		this.occupiedMap[num] = _v;
	}

	public bool CheckArea(int _x, int _z, EnumDecoOccupied _v, int _rectSizeX, int _rectSizeZ)
	{
		int num = DecoManager.CheckPosition(this.width, this.height, _x, _z);
		if (num < 0)
		{
			return true;
		}
		for (int i = 0; i < _rectSizeZ; i++)
		{
			for (int j = 0; j < _rectSizeX; j++)
			{
				if (num >= this.occupiedMap.Length)
				{
					return true;
				}
				if (this.occupiedMap[num] >= _v)
				{
					return true;
				}
				num++;
			}
			num += this.width - _rectSizeX;
		}
		return false;
	}

	public void SetArea(int _x, int _z, EnumDecoOccupied _v, int _rectSizeX, int _rectSizeZ)
	{
		int num = _x + this.widthHalf + (_z + this.heightHalf) * this.width;
		for (int i = 0; i < _rectSizeZ; i++)
		{
			for (int j = 0; j < _rectSizeX; j++)
			{
				if (num < 0 || num >= this.occupiedMap.Length)
				{
					num++;
				}
				else
				{
					if (this.occupiedMap[num] < _v)
					{
						this.occupiedMap[num] = _v;
					}
					num++;
				}
			}
			num += this.width - _rectSizeX;
		}
	}

	public EnumDecoOccupied[] GetData()
	{
		return this.occupiedMap;
	}

	public void SaveAsTexture(string _filename)
	{
		Color32[] array = new Color32[this.occupiedMap.Length];
		for (int i = 0; i < this.occupiedMap.Length; i++)
		{
			Color c = Color.black;
			switch (this.occupiedMap[i])
			{
			case EnumDecoOccupied.SmallSlope:
				c = Color.blue;
				break;
			case EnumDecoOccupied.Stop_BigDeco:
				c = Color.gray;
				break;
			case EnumDecoOccupied.Perimeter:
				c = Color.red;
				break;
			case EnumDecoOccupied.Stop_AnyDeco:
				c = Color.cyan;
				break;
			case EnumDecoOccupied.Deco:
				c = Color.green;
				break;
			case EnumDecoOccupied.POI:
				c = Color.magenta;
				break;
			case EnumDecoOccupied.BigSlope:
				c = Color.yellow;
				break;
			case EnumDecoOccupied.NoneAllowed:
				c = Color.white;
				break;
			}
			array[i] = c;
		}
		Texture2D texture2D = new Texture2D(this.width, this.height);
		texture2D.SetPixels32(array);
		texture2D.Apply();
		TextureUtils.SaveTexture(texture2D, _filename);
		UnityEngine.Object.Destroy(texture2D);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public EnumDecoOccupied[] occupiedMap;

	[PublicizedFrom(EAccessModifier.Private)]
	public int width;

	[PublicizedFrom(EAccessModifier.Private)]
	public int height;

	[PublicizedFrom(EAccessModifier.Private)]
	public int widthHalf;

	[PublicizedFrom(EAccessModifier.Private)]
	public int heightHalf;
}

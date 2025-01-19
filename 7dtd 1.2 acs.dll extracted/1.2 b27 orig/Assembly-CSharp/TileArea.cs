using System;
using System.Collections.Generic;

public class TileArea<T> : ITileArea<T> where T : class
{
	public TileArea(TileAreaConfig _config, T[,] _data = null)
	{
		this.config = _config;
		if (_data != null)
		{
			for (int i = 0; i < _data.GetLength(0); i++)
			{
				for (int j = 0; j < _data.GetLength(1); j++)
				{
					int tileX = i + this.config.tileStart.x;
					int tileZ = j + this.config.tileStart.y;
					uint key = TileAreaUtils.MakeKey(tileX, tileZ);
					this.Data[key] = _data[i, j];
				}
			}
		}
	}

	public TileAreaConfig Config
	{
		get
		{
			return this.config;
		}
	}

	public void Remove(uint _key)
	{
		this.Data.Remove(_key);
	}

	public T this[int _tileX, int _tileZ]
	{
		get
		{
			this.config.checkCoordinates(ref _tileX, ref _tileZ);
			uint key = TileAreaUtils.MakeKey(_tileX, _tileZ);
			T result;
			if (!this.Data.TryGetValue(key, out result))
			{
				return default(T);
			}
			return result;
		}
		set
		{
			this.config.checkCoordinates(ref _tileX, ref _tileZ);
			uint key = TileAreaUtils.MakeKey(_tileX, _tileZ);
			this.Data[key] = value;
		}
	}

	public T this[uint _key]
	{
		get
		{
			T result;
			if (!this.Data.TryGetValue(_key, out result))
			{
				return default(T);
			}
			return result;
		}
		set
		{
			this.Data[_key] = value;
		}
	}

	public TileAreaConfig config;

	public Dictionary<uint, T> Data = new Dictionary<uint, T>();
}

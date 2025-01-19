﻿using System;
using System.Runtime.CompilerServices;

public class TileFile<[IsUnmanaged] T> : IDisposable where T : struct, ValueType
{
	public TileFile(FileBackedArray<T> _fileBackedArray, int _tileWidth, int _tileCountWidth, int _tileCountHeight)
	{
		this.fba = _fileBackedArray;
		this.tileWidth = _tileWidth;
		this.tileCountWidth = _tileCountWidth;
		this.tileCountHeight = _tileCountHeight;
		this.dataLength = this.tileWidth * this.tileWidth;
	}

	public bool IsInDatabase(int _tileX, int _tileZ)
	{
		return _tileX >= 0 && _tileX < this.tileCountWidth && _tileZ >= 0 && _tileZ < this.tileCountHeight;
	}

	public unsafe void LoadTile(int _tileX, int _tileZ, ref T[,] _tile)
	{
		if (_tile == null)
		{
			_tile = new T[this.tileWidth, this.tileWidth];
		}
		int start = _tileZ * this.tileCountWidth * this.dataLength + _tileX * this.dataLength;
		ReadOnlySpan<T> readOnlySpan2;
		using (this.fba.GetReadOnlySpan(start, this.dataLength, out readOnlySpan2))
		{
			T[,] array;
			T* pointer;
			if ((array = _tile) == null || array.Length == 0)
			{
				pointer = null;
			}
			else
			{
				pointer = &array[0, 0];
			}
			Span<T> destination = new Span<T>((void*)pointer, this.dataLength);
			readOnlySpan2.CopyTo(destination);
			array = null;
		}
	}

	public void Dispose()
	{
		this.fba.Dispose();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public FileBackedArray<T> fba;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int tileWidth;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int tileCountWidth;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int tileCountHeight;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int dataLength;
}

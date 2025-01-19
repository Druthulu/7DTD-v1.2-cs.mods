using System;

public class FileBackedDecoOccupiedMap : IDisposable
{
	public FileBackedDecoOccupiedMap(int _worldWidth, int _worldHeight)
	{
		this.width = _worldWidth;
		this.height = _worldHeight;
		this.heightHalf = this.height / 2;
		this.occupiedMap = new FileBackedArray<EnumDecoOccupied>(this.width * this.height);
		this.cacheLength = this.width * 128;
		this.cacheEnd = this.cacheLength;
		this.cacheHandle = this.occupiedMap.GetMemory(0, this.cacheLength, out this.cache);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int GetDecoChunkRowCacheStart(int offset)
	{
		return offset / this.cacheLength * this.cacheLength;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Cache(int offset)
	{
		if (offset >= this.cacheEnd || offset < this.cacheStart)
		{
			this.cacheStart = this.GetDecoChunkRowCacheStart(offset);
			this.cacheEnd = this.cacheStart + this.cacheLength;
			this.cacheHandle.Dispose();
			this.cacheHandle = this.occupiedMap.GetMemory(this.cacheStart, this.cacheLength, out this.cache);
		}
	}

	public unsafe EnumDecoOccupied Get(int _offs)
	{
		this.Cache(_offs);
		return *this.cache.Span[_offs - this.cacheStart];
	}

	public void CopyDecoChunkRow(int row, EnumDecoOccupied[] from)
	{
		int num = this.heightHalf / 128;
		int start = (row + num) * 128 * this.width;
		Span<EnumDecoOccupied> destination;
		using (this.occupiedMap.GetSpan(start, this.cacheLength, out destination))
		{
			from.AsSpan(start, this.cacheLength).CopyTo(destination);
		}
	}

	public void Dispose()
	{
		this.cacheHandle.Dispose();
		this.cacheHandle = null;
		this.occupiedMap.Dispose();
		this.occupiedMap = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public FileBackedArray<EnumDecoOccupied> occupiedMap;

	[PublicizedFrom(EAccessModifier.Private)]
	public int width;

	[PublicizedFrom(EAccessModifier.Private)]
	public int height;

	[PublicizedFrom(EAccessModifier.Private)]
	public int heightHalf;

	[PublicizedFrom(EAccessModifier.Private)]
	public int cacheLength;

	[PublicizedFrom(EAccessModifier.Private)]
	public IBackedArrayHandle cacheHandle;

	[PublicizedFrom(EAccessModifier.Private)]
	public Memory<EnumDecoOccupied> cache;

	[PublicizedFrom(EAccessModifier.Private)]
	public int cacheStart;

	[PublicizedFrom(EAccessModifier.Private)]
	public int cacheEnd;
}

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

public class WorldChunkCache
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static long MakeChunkKey(int x, int y)
	{
		return ((long)y & 16777215L) << 24 | ((long)x & 16777215L);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static long MakeChunkKey(int x, int y, int clrIdx)
	{
		return ((long)clrIdx & 255L) << 56 | ((long)y & 16777215L) << 24 | ((long)x & 16777215L);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static long MakeChunkKey(long key, int clrIdx)
	{
		return ((long)clrIdx & 255L) << 56 | (key & 72057594037927935L);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2i ChunkPositionFromKey(long key)
	{
		int x = (int)(key & 16777215L);
		int y = (int)(key >> 24 & 16777215L);
		return new Vector2i(x, y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int extractX(long key)
	{
		return (int)((key & 16777215L) | (long)(((key & 8388608L) != 0L) ? 18446744073692774400UL : 0UL));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int extractZ(long key)
	{
		key = (key >> 24 & 16777215L);
		return (int)(key | (long)(((key & 8388608L) != 0L) ? 18446744073692774400UL : 0UL));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2i extractXZ(long key)
	{
		Vector2i result;
		result.x = WorldChunkCache.extractX(key);
		result.y = WorldChunkCache.extractZ(key);
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int extractClrIdx(long key)
	{
		return (int)(key >> 56 & 255L);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Chunk GetChunkSync(int _x, int _y)
	{
		return this.GetChunkSync(WorldChunkCache.MakeChunkKey(_x, _y));
	}

	public virtual Chunk GetChunkSync(long _key)
	{
		this.sync.EnterReadLock();
		Chunk result;
		this.chunks.dict.TryGetValue(_key & 72057594037927935L, out result);
		this.sync.ExitReadLock();
		return result;
	}

	public virtual void RemoveChunkSync(long _key)
	{
		this.sync.EnterWriteLock();
		Chunk chunk;
		if (this.chunks.dict.TryGetValue(_key & 72057594037927935L, out chunk))
		{
			this.chunks.Remove(_key & 72057594037927935L);
			this.isChunkArrayDirty = true;
		}
		this.chunkKeys.Remove(_key);
		this.isChunkKeysDirty = true;
		this.sync.ExitWriteLock();
		if (chunk != null)
		{
			for (int i = 0; i < this.chunkCallbacks.Count; i++)
			{
				this.chunkCallbacks[i].OnChunkBeforeRemove(chunk);
			}
		}
	}

	public void NotifyOnChunkBeforeSave(Chunk _c)
	{
		for (int i = 0; i < this.chunkCallbacks.Count; i++)
		{
			this.chunkCallbacks[i].OnChunkBeforeSave(_c);
		}
	}

	public virtual bool AddChunkSync(Chunk _chunk, bool _bOmitCallbacks = false)
	{
		this.ChunkMinPos.x = Utils.FastMin(this.ChunkMinPos.x, _chunk.X);
		this.ChunkMinPos.y = Utils.FastMin(this.ChunkMinPos.y, _chunk.Z);
		this.ChunkMaxPos.x = Utils.FastMax(this.ChunkMaxPos.x, _chunk.X);
		this.ChunkMaxPos.y = Utils.FastMax(this.ChunkMaxPos.y, _chunk.Z);
		this.sync.EnterWriteLock();
		long key = _chunk.Key;
		if (this.chunkKeys.Contains(key))
		{
			this.sync.ExitWriteLock();
			return false;
		}
		this.chunks.Add(key & 72057594037927935L, _chunk);
		this.chunkKeys.Add(key);
		this.isChunkArrayDirty = true;
		this.isChunkKeysDirty = true;
		this.sync.ExitWriteLock();
		int num = 0;
		while (!_bOmitCallbacks && num < this.chunkCallbacks.Count)
		{
			this.chunkCallbacks[num].OnChunkAdded(_chunk);
			num++;
		}
		return true;
	}

	public List<Chunk> GetChunkArrayCopySync()
	{
		this.sync.EnterReadLock();
		if (this.isChunkArrayDirty)
		{
			this.chunkArrayCopy = new List<Chunk>(this.chunks.list.Count);
			this.chunkArrayCopy.AddRange(this.chunks.list);
			this.isChunkArrayDirty = false;
		}
		this.sync.ExitReadLock();
		return this.chunkArrayCopy;
	}

	public LinkedList<Chunk> GetChunkArray()
	{
		return this.chunks.list;
	}

	public HashSetLong GetChunkKeysCopySync()
	{
		if (this.isChunkKeysDirty)
		{
			this.sync.EnterReadLock();
			this.chunkKeysCopy.Clear();
			foreach (long item in this.chunkKeys)
			{
				this.chunkKeysCopy.Add(item);
			}
			this.sync.ExitReadLock();
			this.isChunkKeysDirty = false;
		}
		return this.chunkKeysCopy;
	}

	public int Count()
	{
		return this.chunks.list.Count;
	}

	public ReaderWriterLockSlim GetSyncRoot()
	{
		return this.sync;
	}

	public virtual bool ContainsChunkSync(long key)
	{
		this.sync.EnterReadLock();
		bool result = this.chunks.dict.ContainsKey(key);
		this.sync.ExitReadLock();
		return result;
	}

	public void AddChunkCallback(IChunkCallback _callback)
	{
		this.chunkCallbacks.Add(_callback);
	}

	public void RemoveChunkCallback(IChunkCallback _callback)
	{
		this.chunkCallbacks.Remove(_callback);
	}

	public virtual void Update()
	{
	}

	public virtual void Clear()
	{
		this.sync.EnterWriteLock();
		List<Chunk> list = new List<Chunk>();
		list.AddRange(this.GetChunkArray());
		this.chunks.Clear();
		this.chunkKeys.Clear();
		this.isChunkArrayDirty = true;
		this.isChunkKeysDirty = true;
		this.sync.ExitWriteLock();
		MemoryPools.PoolChunks.FreeSync(list);
	}

	public bool GetNeighborChunks(Chunk _chunk, Chunk[] neighbours)
	{
		this.sync.EnterReadLock();
		int x = _chunk.X;
		int z = _chunk.Z;
		Chunk chunk = this.GetChunk(x + 1, z);
		if (chunk != null)
		{
			neighbours[0] = chunk;
			chunk = this.GetChunk(x - 1, z);
			if (chunk != null)
			{
				neighbours[1] = chunk;
				chunk = this.GetChunk(x, z + 1);
				if (chunk != null)
				{
					neighbours[2] = chunk;
					chunk = this.GetChunk(x, z - 1);
					if (chunk != null)
					{
						neighbours[3] = chunk;
						chunk = this.GetChunk(x + 1, z + 1);
						if (chunk != null)
						{
							neighbours[4] = chunk;
							chunk = this.GetChunk(x - 1, z - 1);
							if (chunk != null)
							{
								neighbours[5] = chunk;
								chunk = this.GetChunk(x - 1, z + 1);
								if (chunk != null)
								{
									neighbours[6] = chunk;
									chunk = this.GetChunk(x + 1, z - 1);
									if (chunk != null)
									{
										neighbours[7] = chunk;
										this.sync.ExitReadLock();
										return true;
									}
								}
							}
						}
					}
				}
			}
		}
		this.sync.ExitReadLock();
		return false;
	}

	public bool HasNeighborChunks(Chunk _chunk)
	{
		this.sync.EnterReadLock();
		int x = _chunk.X;
		int z = _chunk.Z;
		if (this.GetChunk(x + 1, z) != null && this.GetChunk(x - 1, z) != null && this.GetChunk(x, z + 1) != null && this.GetChunk(x, z - 1) != null && this.GetChunk(x + 1, z + 1) != null && this.GetChunk(x - 1, z - 1) != null && this.GetChunk(x - 1, z + 1) != null && this.GetChunk(x + 1, z - 1) != null)
		{
			this.sync.ExitReadLock();
			return true;
		}
		this.sync.ExitReadLock();
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Chunk GetChunk(int _x, int _y)
	{
		long key = WorldChunkCache.MakeChunkKey(_x, _y);
		Chunk result;
		this.chunks.dict.TryGetValue(key, out result);
		return result;
	}

	public const long InvalidChunk = 9223372036854775807L;

	public Vector2i ChunkMinPos = Vector2i.zero;

	public Vector2i ChunkMaxPos = Vector2i.zero;

	public DictionaryLinkedList<long, Chunk> chunks = new DictionaryLinkedList<long, Chunk>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<IChunkCallback> chunkCallbacks = new List<IChunkCallback>();

	public HashSetLong chunkKeys = new HashSetLong();

	[PublicizedFrom(EAccessModifier.Private)]
	public volatile bool isChunkKeysDirty = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSetLong chunkKeysCopy = new HashSetLong();

	[PublicizedFrom(EAccessModifier.Private)]
	public volatile bool isChunkArrayDirty = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Chunk> chunkArrayCopy = new List<Chunk>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ReaderWriterLockSlim sync = new ReaderWriterLockSlim();
}

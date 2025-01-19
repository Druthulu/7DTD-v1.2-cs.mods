using System;
using System.Collections.Generic;

public class DChunkSquareMeshPool
{
	public int Count
	{
		get
		{
			object poolLock = this._poolLock;
			int result;
			lock (poolLock)
			{
				int num = 0;
				for (int i = 0; i < this.pool.Length; i++)
				{
					num += this.pool[i].Count;
				}
				result = num;
			}
			return result;
		}
	}

	public DChunkSquareMeshPool(int initialCapacity, int NbLODLevel)
	{
		this._poolLock = new object();
		this.pool = new List<DChunkSquareMesh>[NbLODLevel];
		for (int i = 0; i < NbLODLevel; i++)
		{
			this.pool[i] = new List<DChunkSquareMesh>(initialCapacity);
		}
	}

	public DChunkSquareMesh GetObject(DistantChunkMap DCMap, int LODLevel)
	{
		object poolLock = this._poolLock;
		DChunkSquareMesh result;
		lock (poolLock)
		{
			List<DChunkSquareMesh> list = this.pool[LODLevel];
			if (list.Count == 0)
			{
				result = new DChunkSquareMesh(DCMap, LODLevel);
			}
			else
			{
				DChunkSquareMesh dchunkSquareMesh = list[list.Count - 1];
				list.RemoveAt(list.Count - 1);
				result = dchunkSquareMesh;
			}
		}
		return result;
	}

	public void ReturnObject(DChunkSquareMesh item, int LODLevel)
	{
		if (item == null)
		{
			throw new ArgumentNullException("DChunkSquareMesh is null");
		}
		object poolLock = this._poolLock;
		lock (poolLock)
		{
			if (this.pool[LODLevel].Contains(item))
			{
				throw new InvalidOperationException("ThreadProcessing already in pool");
			}
			this.pool[LODLevel].Add(item);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public List<DChunkSquareMesh>[] pool;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly object _poolLock;
}

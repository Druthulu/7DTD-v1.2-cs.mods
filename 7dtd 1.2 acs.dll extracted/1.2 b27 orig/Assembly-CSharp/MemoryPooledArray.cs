using System;
using System.Collections.Generic;

public class MemoryPooledArray<T> where T : new()
{
	public MemoryPooledArray()
	{
		this.pools = new List<T[]>[MemoryPooledArraySizes.poolElements.Length];
		for (int i = 0; i < this.pools.Length; i++)
		{
			this.pools[i] = new List<T[]>();
		}
		this.poolSize = new int[MemoryPooledArraySizes.poolElements.Length];
	}

	public T[] Alloc(int _minSize = 0)
	{
		List<T[]>[] obj = this.pools;
		T[] result;
		lock (obj)
		{
			int num = this.sizeToIdx(_minSize);
			T[] array;
			if (this.poolSize[num] == 0)
			{
				array = new T[MemoryPooledArraySizes.poolElements[num]];
			}
			else
			{
				int[] array2 = this.poolSize;
				int num2 = num;
				int num3 = array2[num2] - 1;
				array2[num2] = num3;
				int index = num3;
				array = this.pools[num][index];
				this.pools[num][index] = null;
			}
			result = array;
		}
		return result;
	}

	public T[] Grow(T[] _array)
	{
		return this.Grow(_array, _array.Length + 1);
	}

	public T[] Grow(T[] _array, int _minSize)
	{
		T[] array = this.Alloc(_minSize);
		Array.Copy(_array, array, _array.Length);
		this.Free(_array);
		return array;
	}

	public void Free(T[] _array)
	{
		if (_array == null)
		{
			return;
		}
		List<T[]>[] obj = this.pools;
		lock (obj)
		{
			int num = this.sizeToIdx(_array.Length);
			if (this.poolSize[num] >= this.pools[num].Count)
			{
				this.pools[num].Add(_array);
				this.poolSize[num]++;
			}
			else
			{
				List<T[]> list = this.pools[num];
				int[] array = this.poolSize;
				int num2 = num;
				int num3 = array[num2];
				array[num2] = num3 + 1;
				list[num3] = _array;
			}
		}
	}

	public void FreeAll()
	{
		List<T[]>[] obj = this.pools;
		lock (obj)
		{
			for (int i = 0; i < this.pools.Length; i++)
			{
				this.pools[i].Clear();
				this.poolSize[i] = 0;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int sizeToIdx(int _size)
	{
		int num = -1;
		for (int i = 0; i < MemoryPooledArraySizes.poolElements.Length; i++)
		{
			if (MemoryPooledArraySizes.poolElements[i] >= _size)
			{
				num = i;
				break;
			}
		}
		if (num == -1)
		{
			throw new Exception("Array length in pool not supported " + _size.ToString());
		}
		return num;
	}

	public int GetCount()
	{
		int num = 0;
		for (int i = 0; i < this.pools.Length; i++)
		{
			if (this.pools[i] != null)
			{
				num += this.pools[i].Count;
			}
		}
		return num;
	}

	public int GetCount(int _poolIndex)
	{
		if (_poolIndex < 0 || _poolIndex >= this.pools.Length)
		{
			return 0;
		}
		List<T[]> list = this.pools[_poolIndex];
		if (list == null)
		{
			return 0;
		}
		return list.Count;
	}

	public long GetElementsCount()
	{
		int num = 0;
		for (int i = 0; i < this.pools.Length; i++)
		{
			if (this.pools[i] != null)
			{
				num += this.pools[i].Count * MemoryPooledArraySizes.poolElements[i];
			}
		}
		return (long)num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public List<T[]>[] pools;

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] poolSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public int maxCapacity;
}

using System;
using System.Collections.Generic;

public class LoosePool<T> where T : new()
{
	public LoosePool()
	{
		this.pools = new List<T[]>[this.poolElements.Length];
		for (int i = 0; i < this.pools.Length; i++)
		{
			this.pools[i] = new List<T[]>();
		}
		this.poolSize = new int[this.poolElements.Length];
	}

	public T[] Alloc(int _minSize = 0)
	{
		List<T[]>[] obj = this.pools;
		T[] result;
		lock (obj)
		{
			int num = this.sizeToIdx(_minSize, this.AllowHigherPool);
			T[] array;
			if (this.poolSize[num] == 0)
			{
				array = new T[this.poolElements[num]];
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

	public void Free(T[] _array)
	{
		if (_array == null)
		{
			return;
		}
		List<T[]>[] obj = this.pools;
		lock (obj)
		{
			int num = this.sizeToIdx(_array.Length, false);
			int num2 = this.maxSize[num];
			if (_array.Length != this.poolElements[num])
			{
				Log.Out("removing item as it does not match the cache size");
			}
			else if (!this.EnforceMaxSize || num2 <= 0 || this.poolSize[num] < num2)
			{
				if (this.poolSize[num] >= this.pools[num].Count)
				{
					this.pools[num].Add(_array);
					this.poolSize[num]++;
				}
				else
				{
					List<T[]> list = this.pools[num];
					int[] array = this.poolSize;
					int num3 = num;
					int num4 = array[num3];
					array[num3] = num4 + 1;
					list[num4] = _array;
				}
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
	public int sizeToIdx(int _size, bool allowHigher)
	{
		int num = -1;
		for (int i = 0; i < this.poolElements.Length; i++)
		{
			if (this.poolElements[i] >= _size)
			{
				if (num == -1)
				{
					num = i;
				}
				if (!allowHigher || this.poolSize[i] > 0)
				{
					return i;
				}
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

	public int GetTotalItems()
	{
		int num = 0;
		foreach (int num2 in this.poolSize)
		{
			num += num2;
		}
		return num;
	}

	public string GetSize(out int totalBytes)
	{
		string str = "";
		totalBytes = 0;
		for (int i = 0; i < this.pools.Length; i++)
		{
			int num = this.poolSize[i];
			int num2 = this.poolElements[i];
			int num3 = 4 * num2 * num;
			totalBytes += num3;
			str += string.Format("{0} x {1} = {2}\n", num2, num, num3);
		}
		str = str + "Bytes: " + totalBytes.ToString() + "\n";
		str = str + "KBytes: " + (totalBytes / 1024).ToString() + "\n";
		return str + "MBytes: " + (totalBytes / 1024 / 1024).ToString();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] poolElements = new int[]
	{
		64,
		128,
		256,
		512,
		1024,
		2048,
		4096,
		8192,
		16384,
		32768,
		65536,
		131072,
		262144,
		524288,
		1048576,
		2097152,
		4194304,
		8388608,
		16777216,
		33554432,
		67108864,
		134217728
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] maxSize = new int[]
	{
		0,
		0,
		0,
		0,
		0,
		0,
		0,
		0,
		0,
		15,
		20,
		20,
		10,
		10,
		10,
		10,
		5,
		5,
		2,
		2,
		2,
		2
	};

	public bool EnforceMaxSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<T[]>[] pools;

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] poolSize;

	public bool AllowHigherPool = true;
}

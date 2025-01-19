﻿using System;
using System.Collections.Generic;

public class MemoryPooledObject<T> where T : IMemoryPoolableObject, new()
{
	public MemoryPooledObject(int _maxCapacity)
	{
		this.pool = new List<T>(_maxCapacity);
		this.maxCapacity = _maxCapacity;
	}

	public void SetCapacity(int _maxCapacity)
	{
		this.pool.Capacity = _maxCapacity;
		this.maxCapacity = _maxCapacity;
	}

	public T AllocSync(bool _bReset)
	{
		List<T> obj = this.pool;
		T result;
		lock (obj)
		{
			result = this.Alloc(_bReset);
		}
		return result;
	}

	public T Alloc(bool _bReset)
	{
		T result;
		if (this.poolSize == 0)
		{
			result = Activator.CreateInstance<T>();
		}
		else
		{
			this.poolSize--;
			result = this.pool[this.poolSize];
			this.pool[this.poolSize] = default(T);
		}
		if (_bReset)
		{
			result.Reset();
		}
		return result;
	}

	public void FreeSync(IList<T> _array)
	{
		List<T> obj = this.pool;
		lock (obj)
		{
			for (int i = 0; i < _array.Count; i++)
			{
				T t = _array[i];
				if (t != null)
				{
					this.Free(t);
					_array[i] = default(T);
				}
			}
		}
	}

	public void FreeSync(Queue<T> _queue)
	{
		List<T> obj = this.pool;
		lock (obj)
		{
			while (_queue.Count > 0)
			{
				this.Free(_queue.Dequeue());
			}
		}
	}

	public void FreeSync(T _t)
	{
		List<T> obj = this.pool;
		lock (obj)
		{
			this.Free(_t);
		}
	}

	public void Free(T[] _array)
	{
		for (int i = 0; i < _array.Length; i++)
		{
			T t = _array[i];
			if (t != null)
			{
				this.Free(t);
				_array[i] = default(T);
			}
		}
	}

	public void Free(List<T> _list)
	{
		for (int i = 0; i < _list.Count; i++)
		{
			T t = _list[i];
			if (t != null)
			{
				this.Free(t);
			}
		}
		_list.Clear();
	}

	public void Cleanup()
	{
		List<T> obj = this.pool;
		lock (obj)
		{
			for (int i = 0; i < this.poolSize; i++)
			{
				T t = this.pool[i];
				if (t != null)
				{
					t.Cleanup();
				}
			}
			this.pool.Clear();
			this.poolSize = 0;
		}
	}

	public void Free(T _t)
	{
		if (this.poolSize >= this.pool.Count && this.poolSize < this.maxCapacity)
		{
			_t.Reset();
			this.pool.Add(_t);
			this.poolSize++;
			return;
		}
		if (this.poolSize < this.maxCapacity)
		{
			_t.Reset();
			List<T> list = this.pool;
			int num = this.poolSize;
			this.poolSize = num + 1;
			list[num] = _t;
			return;
		}
		_t.Cleanup();
	}

	public int GetPoolSize()
	{
		return this.poolSize;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public List<T> pool;

	[PublicizedFrom(EAccessModifier.Private)]
	public int poolSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public int maxCapacity;
}

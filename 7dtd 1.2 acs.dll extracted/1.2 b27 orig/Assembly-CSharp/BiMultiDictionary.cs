using System;
using System.Collections.Generic;

public sealed class BiMultiDictionary<TKey, TValue>
{
	public BiMultiDictionary()
	{
		this.m_keyToValues = new Dictionary<TKey, HashSet<TValue>>();
		this.m_valueToKey = new Dictionary<TValue, TKey>();
	}

	public void Add(TKey key, TValue value)
	{
		if (this.m_valueToKey.ContainsKey(value))
		{
			throw new ArgumentException("Value already in dictionary.", "value");
		}
		HashSet<TValue> hashSet;
		if (!this.m_keyToValues.TryGetValue(key, out hashSet))
		{
			this.m_keyToValues.Add(key, hashSet = new HashSet<TValue>());
		}
		hashSet.Add(value);
		this.m_valueToKey.Add(value, key);
	}

	public bool ContainsKey(TKey key)
	{
		return this.m_keyToValues.ContainsKey(key);
	}

	public bool ContainsValue(TValue value)
	{
		return this.m_valueToKey.ContainsKey(value);
	}

	public bool TryGetByKey(TKey key, out IReadOnlyCollection<TValue> values)
	{
		HashSet<TValue> hashSet;
		if (!this.m_keyToValues.TryGetValue(key, out hashSet))
		{
			values = null;
			return false;
		}
		values = hashSet;
		return true;
	}

	public int TryGetByKey(TKey key, ICollection<TValue> valuesOut)
	{
		HashSet<TValue> hashSet;
		if (!this.m_keyToValues.TryGetValue(key, out hashSet))
		{
			return 0;
		}
		int num = 0;
		foreach (TValue item in hashSet)
		{
			valuesOut.Add(item);
			num++;
		}
		return num;
	}

	public unsafe int TryGetByKey(TKey key, Span<TValue> valuesOut)
	{
		HashSet<TValue> hashSet;
		if (!this.m_keyToValues.TryGetValue(key, out hashSet))
		{
			return 0;
		}
		int num = 0;
		foreach (TValue tvalue in hashSet)
		{
			if (num >= valuesOut.Length)
			{
				break;
			}
			*valuesOut[num] = tvalue;
			num++;
		}
		return num;
	}

	public bool TryGetByValue(TValue value, out TKey key)
	{
		return this.m_valueToKey.TryGetValue(value, out key);
	}

	public bool RemoveByValue(TValue value)
	{
		TKey key;
		if (!this.m_valueToKey.TryGetValue(value, out key))
		{
			return false;
		}
		HashSet<TValue> hashSet;
		if (!this.m_keyToValues.TryGetValue(key, out hashSet))
		{
			return false;
		}
		this.m_valueToKey.Remove(value);
		hashSet.Remove(value);
		if (hashSet.Count == 0)
		{
			this.m_keyToValues.Remove(key);
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<TKey, HashSet<TValue>> m_keyToValues;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<TValue, TKey> m_valueToKey;
}

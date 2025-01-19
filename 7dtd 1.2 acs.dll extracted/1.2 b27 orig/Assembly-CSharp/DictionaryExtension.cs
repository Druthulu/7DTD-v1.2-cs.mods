using System;
using System.Collections.Generic;
using System.Linq;

public static class DictionaryExtension
{
	public static void RemoveAll<TKey, TValue>(this IDictionary<TKey, TValue> dic, Func<TValue, bool> predicate)
	{
		foreach (TKey key in (from k in dic.Keys
		where predicate(dic[k])
		select k).ToList<TKey>())
		{
			dic.Remove(key);
		}
	}

	public static void RemoveAll<TKey, TValue>(this IDictionary<TKey, TValue> dic, Func<TKey, bool> predicate)
	{
		foreach (TKey key in (from k in dic.Keys
		where predicate(k)
		select k).ToList<TKey>())
		{
			dic.Remove(key);
		}
	}

	public static void CopyTo<TKey, TValue>(this IDictionary<TKey, TValue> _src, IDictionary<TKey, TValue> _dest)
	{
		foreach (KeyValuePair<TKey, TValue> keyValuePair in _src)
		{
			_dest.Add(keyValuePair.Key, keyValuePair.Value);
		}
	}

	public static void CopyKeysTo<TKey, TValue>(this IDictionary<TKey, TValue> _src, ICollection<TKey> _dest)
	{
		foreach (KeyValuePair<TKey, TValue> keyValuePair in _src)
		{
			_dest.Add(keyValuePair.Key);
		}
	}

	public static void CopyKeysTo<TKey, TValue>(this IDictionary<TKey, TValue> _src, TKey[] _dest)
	{
		if (_dest.Length != _src.Count)
		{
			throw new ArgumentOutOfRangeException("_dest", "Target array does not have the same size as the dictionary");
		}
		int num = 0;
		foreach (KeyValuePair<TKey, TValue> keyValuePair in _src)
		{
			_dest[num++] = keyValuePair.Key;
		}
	}

	public static void CopyValuesTo<TKey, TValue>(this IDictionary<TKey, TValue> _src, IList<TValue> _dest)
	{
		foreach (KeyValuePair<TKey, TValue> keyValuePair in _src)
		{
			_dest.Add(keyValuePair.Value);
		}
	}

	public static void CopyValuesTo<TKey, TValue>(this IDictionary<TKey, TValue> _src, TValue[] _dest)
	{
		if (_dest.Length != _src.Count)
		{
			throw new ArgumentOutOfRangeException("_dest", "Target array does not have the same size as the dictionary");
		}
		int num = 0;
		foreach (KeyValuePair<TKey, TValue> keyValuePair in _src)
		{
			_dest[num++] = keyValuePair.Value;
		}
	}
}

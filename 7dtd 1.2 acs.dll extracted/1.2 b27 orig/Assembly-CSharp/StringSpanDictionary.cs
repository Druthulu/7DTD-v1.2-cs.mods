using System;
using System.Collections;
using System.Collections.Generic;

public class StringSpanDictionary<T> : IDictionary<string, T>, ICollection<KeyValuePair<string, T>>, IEnumerable<KeyValuePair<string, T>>, IEnumerable, IReadOnlyDictionary<string, T>, IReadOnlyCollection<KeyValuePair<string, T>>
{
	public StringSpanDictionary() : this(new Dictionary<string, T>())
	{
	}

	public StringSpanDictionary(IDictionary<string, T> dict)
	{
		this.m_dict = dict;
		this.m_hashToKeys = new Dictionary<int, List<string>>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int GenerateHash(StringSpan key)
	{
		return key.GetHashCode();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AddHash(string key)
	{
		int key2 = StringSpanDictionary<T>.GenerateHash(key);
		List<string> list;
		if (!this.m_hashToKeys.TryGetValue(key2, out list))
		{
			list = new List<string>();
			this.m_hashToKeys.Add(key2, list);
		}
		using (List<string>.Enumerator enumerator = list.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current == key)
				{
					return;
				}
			}
		}
		list.Add(key);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RemoveHash(string key)
	{
		int key2 = StringSpanDictionary<T>.GenerateHash(key);
		List<string> list;
		if (!this.m_hashToKeys.TryGetValue(key2, out list))
		{
			return;
		}
		list.Remove(key);
		if (list.Count > 0)
		{
			return;
		}
		this.m_hashToKeys.Remove(key2);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool TryGetStringFromHashedKeys(StringSpan spanKey, out string stringKey)
	{
		int key = StringSpanDictionary<T>.GenerateHash(spanKey);
		List<string> list;
		if (!this.m_hashToKeys.TryGetValue(key, out list))
		{
			stringKey = null;
			return false;
		}
		foreach (string text in list)
		{
			if (!(text != spanKey))
			{
				stringKey = text;
				return true;
			}
		}
		stringKey = null;
		return false;
	}

	public void Add(StringSpan key, T value)
	{
		string key2;
		if (!this.TryGetStringFromHashedKeys(key, out key2))
		{
			this.Add(key.ToString(), value);
			return;
		}
		this.Add(key2, value);
	}

	public bool ContainsKey(StringSpan key)
	{
		string key2;
		return this.TryGetStringFromHashedKeys(key, out key2) && this.ContainsKey(key2);
	}

	public bool Remove(StringSpan key)
	{
		string key2;
		return this.TryGetStringFromHashedKeys(key, out key2) && this.Remove(key2);
	}

	public bool TryGetValue(StringSpan key, out T value)
	{
		string key2;
		if (!this.TryGetStringFromHashedKeys(key, out key2))
		{
			value = default(T);
			return false;
		}
		return this.TryGetValue(key2, out value);
	}

	public T this[StringSpan key]
	{
		get
		{
			string key2;
			if (!this.TryGetStringFromHashedKeys(key, out key2))
			{
				throw new KeyNotFoundException();
			}
			return this[key2];
		}
		set
		{
			string key2;
			if (!this.TryGetStringFromHashedKeys(key, out key2))
			{
				this[key.ToString()] = value;
				return;
			}
			this[key2] = value;
		}
	}

	public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
	{
		return this.m_dict.GetEnumerator();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator GetEnumerator()
	{
		return this.m_dict.GetEnumerator();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Add(KeyValuePair<string, T> item)
	{
		this.Add(item.Key, item.Value);
	}

	public void Clear()
	{
		this.m_dict.Clear();
		this.m_hashToKeys.Clear();
	}

	public bool Contains(KeyValuePair<string, T> item)
	{
		return this.m_dict.Contains(item);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CopyTo(KeyValuePair<string, T>[] array, int arrayIndex)
	{
		this.m_dict.CopyTo(array, arrayIndex);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool Remove(KeyValuePair<string, T> item)
	{
		bool result = this.m_dict.Remove(item);
		if (!this.m_dict.ContainsKey(item.Key))
		{
			this.RemoveHash(item.Key);
		}
		return result;
	}

	public int Count
	{
		get
		{
			return this.m_dict.Count;
		}
	}

	public bool IsReadOnly
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.m_dict.IsReadOnly;
		}
	}

	public void Add(string key, T value)
	{
		this.m_dict.Add(key, value);
		this.AddHash(key);
	}

	public bool ContainsKey(string key)
	{
		return this.m_dict.ContainsKey(key);
	}

	public bool Remove(string key)
	{
		bool result = this.m_dict.Remove(key);
		if (!this.m_dict.ContainsKey(key))
		{
			this.RemoveHash(key);
		}
		return result;
	}

	public bool TryGetValue(string key, out T value)
	{
		return this.m_dict.TryGetValue(key, out value);
	}

	public T this[string key]
	{
		get
		{
			return this.m_dict[key];
		}
		set
		{
			this.m_dict[key] = value;
			this.AddHash(key);
		}
	}

	public ICollection<string> Keys
	{
		get
		{
			return this.m_dict.Keys;
		}
	}

	public ICollection<T> Values
	{
		get
		{
			return this.m_dict.Values;
		}
	}

	public IEnumerable<string> Keys
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.m_dict.Keys;
		}
	}

	public IEnumerable<T> Values
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.m_dict.Values;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly IDictionary<string, T> m_dict;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<int, List<string>> m_hashToKeys;
}

using System;
using System.Collections.Generic;

public class DictionaryNameId<T>
{
	public DictionaryNameId(DictionaryNameIdMapping _mapping)
	{
		this.mapping = _mapping;
	}

	public void Add(string _name, T _value)
	{
		int key = this.mapping.Add(_name);
		this.idsToValues[key] = _value;
	}

	public int Count
	{
		get
		{
			return this.idsToValues.Count;
		}
	}

	public Dictionary<int, T> Dict
	{
		get
		{
			return this.idsToValues;
		}
	}

	public bool Contains(string _name)
	{
		int num = this.mapping.FindId(_name);
		return num != 0 && this.idsToValues.ContainsKey(num);
	}

	public T Get(int _id)
	{
		T result;
		this.idsToValues.TryGetValue(_id, out result);
		return result;
	}

	public T Get(string _name)
	{
		int num = this.mapping.FindId(_name);
		if (num == 0)
		{
			return default(T);
		}
		T result;
		this.idsToValues.TryGetValue(num, out result);
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly DictionaryNameIdMapping mapping;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<int, T> idsToValues = new Dictionary<int, T>();
}

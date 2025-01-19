using System;
using System.Collections;
using System.Collections.Generic;

public class ObservableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable
{
	public event DictionaryAddEventHandler<TKey, TValue> EntryAdded;

	public event DictionaryRemoveEventHandler<TKey, TValue> EntryRemoved;

	public event DictionaryUpdatedValueEventHandler<TKey, TValue> EntryUpdatedValue;

	public event DictionaryEntryModifiedEventHandler<TKey, TValue> EntryModified;

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnEntryModified(TKey key, TValue value, string action)
	{
		DictionaryEntryModifiedEventHandler<TKey, TValue> entryModified = this.EntryModified;
		if (entryModified == null)
		{
			return;
		}
		entryModified(this, new DictionaryChangedEventArgs<TKey, TValue>(key, value, action));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnEntryAdded(TKey key, TValue value)
	{
		DictionaryAddEventHandler<TKey, TValue> entryAdded = this.EntryAdded;
		if (entryAdded != null)
		{
			entryAdded(this, new DictionaryChangedEventArgs<TKey, TValue>(key, value, "Added"));
		}
		this.OnEntryModified(key, value, "Added");
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnEntryRemoved(TKey key, TValue value)
	{
		DictionaryRemoveEventHandler<TKey, TValue> entryRemoved = this.EntryRemoved;
		if (entryRemoved != null)
		{
			entryRemoved(this, new DictionaryChangedEventArgs<TKey, TValue>(key, value, "Removed"));
		}
		this.OnEntryModified(key, value, "Removed");
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnEntryUpdated(TKey key, TValue value)
	{
		DictionaryUpdatedValueEventHandler<TKey, TValue> entryUpdatedValue = this.EntryUpdatedValue;
		if (entryUpdatedValue != null)
		{
			entryUpdatedValue(this, new DictionaryChangedEventArgs<TKey, TValue>(key, value, "Updated"));
		}
		this.OnEntryModified(key, value, "Updated");
	}

	public void Add(TKey key, TValue value)
	{
		this._dictionary.Add(key, value);
		this.OnEntryAdded(key, value);
	}

	public bool Remove(TKey key)
	{
		TValue value;
		if (this._dictionary.TryGetValue(key, out value) && this._dictionary.Remove(key))
		{
			this.OnEntryRemoved(key, value);
			return true;
		}
		return false;
	}

	public TValue this[TKey key]
	{
		get
		{
			return this._dictionary[key];
		}
		set
		{
			if (this._dictionary.ContainsKey(key))
			{
				this._dictionary[key] = value;
				this.OnEntryUpdated(key, value);
				return;
			}
			this._dictionary[key] = value;
			this.OnEntryAdded(key, value);
		}
	}

	public ICollection<TKey> Keys
	{
		get
		{
			return this._dictionary.Keys;
		}
	}

	public ICollection<TValue> Values
	{
		get
		{
			return this._dictionary.Values;
		}
	}

	public bool ContainsKey(TKey key)
	{
		return this._dictionary.ContainsKey(key);
	}

	public bool TryGetValue(TKey key, out TValue value)
	{
		return this._dictionary.TryGetValue(key, out value);
	}

	public void Add(KeyValuePair<TKey, TValue> item)
	{
		this.Add(item.Key, item.Value);
	}

	public bool Remove(KeyValuePair<TKey, TValue> item)
	{
		return this.Remove(item.Key);
	}

	public bool Contains(KeyValuePair<TKey, TValue> item)
	{
		return this._dictionary.ContainsKey(item.Key) && EqualityComparer<TValue>.Default.Equals(this._dictionary[item.Key], item.Value);
	}

	public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
	{
		((ICollection<KeyValuePair<TKey, TValue>>)this._dictionary).CopyTo(array, arrayIndex);
	}

	public int Count
	{
		get
		{
			return this._dictionary.Count;
		}
	}

	public bool IsReadOnly
	{
		get
		{
			return false;
		}
	}

	public void Clear()
	{
		foreach (TKey key in new List<TKey>(this._dictionary.Keys))
		{
			this.Remove(key);
		}
	}

	public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
	{
		return this._dictionary.GetEnumerator();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator GetEnumerator()
	{
		return this._dictionary.GetEnumerator();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<TKey, TValue> _dictionary = new Dictionary<TKey, TValue>();
}

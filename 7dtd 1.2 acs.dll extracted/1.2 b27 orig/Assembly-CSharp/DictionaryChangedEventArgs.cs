using System;

public class DictionaryChangedEventArgs<TKey, TValue> : EventArgs
{
	public TKey Key { get; }

	public TValue Value { get; }

	public string Action { get; }

	public DictionaryChangedEventArgs(TKey key, TValue value, string action)
	{
		this.Key = key;
		this.Value = value;
		this.Action = action;
	}
}

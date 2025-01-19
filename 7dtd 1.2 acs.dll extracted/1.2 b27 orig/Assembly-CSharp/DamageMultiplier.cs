using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

public class DamageMultiplier
{
	public DamageMultiplier()
	{
	}

	public DamageMultiplier(DynamicProperties _properties, string _prefix)
	{
		if (_prefix == null)
		{
			_prefix = "";
		}
		if (_prefix.Length > 0 && !_prefix.EndsWith("."))
		{
			_prefix += ".";
		}
		_prefix += "DamageBonus.";
		foreach (KeyValuePair<string, string> keyValuePair in _properties.Values.Dict)
		{
			if (keyValuePair.Key.StartsWith(_prefix))
			{
				string name = keyValuePair.Key.Substring(_prefix.Length);
				float value = StringParsers.ParseFloat(_properties.Values[keyValuePair.Key], 0, -1, NumberStyles.Any);
				this.addMultiplier(name, value);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void addMultiplier(string _name, float _value)
	{
		this.damageMultiplier[_name] = _value;
	}

	public float Get(string _group)
	{
		if (_group == null || this.damageMultiplier == null || !this.damageMultiplier.ContainsKey(_group))
		{
			return 1f;
		}
		return this.damageMultiplier[_group];
	}

	public void Read(BinaryReader _br)
	{
		this.damageMultiplier.Clear();
		int num = (int)_br.ReadInt16();
		for (int i = 0; i < num; i++)
		{
			string key = _br.ReadString();
			float value = _br.ReadSingle();
			this.damageMultiplier.Add(key, value);
		}
	}

	public void Write(BinaryWriter _bw)
	{
		_bw.Write((short)this.damageMultiplier.Count);
		foreach (KeyValuePair<string, float> keyValuePair in this.damageMultiplier)
		{
			_bw.Write(keyValuePair.Key);
			_bw.Write(keyValuePair.Value);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const string PREFIX = "DamageBonus.";

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, float> damageMultiplier = new Dictionary<string, float>();
}

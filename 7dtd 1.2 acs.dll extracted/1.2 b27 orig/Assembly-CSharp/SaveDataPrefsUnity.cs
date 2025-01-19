﻿using System;
using UnityEngine;

public sealed class SaveDataPrefsUnity : ISaveDataPrefs
{
	public static SaveDataPrefsUnity INSTANCE
	{
		get
		{
			SaveDataPrefsUnity result;
			if ((result = SaveDataPrefsUnity.s_instance) == null)
			{
				result = (SaveDataPrefsUnity.s_instance = new SaveDataPrefsUnity());
			}
			return result;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public SaveDataPrefsUnity()
	{
	}

	public float GetFloat(string key, float defaultValue)
	{
		return PlayerPrefs.GetFloat(key, defaultValue);
	}

	public void SetFloat(string key, float value)
	{
		PlayerPrefs.SetFloat(key, value);
	}

	public int GetInt(string key, int defaultValue)
	{
		return PlayerPrefs.GetInt(key, defaultValue);
	}

	public void SetInt(string key, int value)
	{
		PlayerPrefs.SetInt(key, value);
	}

	public string GetString(string key, string defaultValue)
	{
		return PlayerPrefs.GetString(key, defaultValue);
	}

	public void SetString(string key, string value)
	{
		PlayerPrefs.SetString(key, value);
	}

	public bool HasKey(string key)
	{
		return PlayerPrefs.HasKey(key);
	}

	public void DeleteKey(string key)
	{
		PlayerPrefs.DeleteKey(key);
	}

	public void DeleteAll()
	{
		PlayerPrefs.DeleteAll();
	}

	public void Save()
	{
		PlayerPrefs.Save();
	}

	public void Load()
	{
		throw new NotSupportedException("Unity PlayerPrefs does not support explicit loading.");
	}

	public bool CanLoad
	{
		get
		{
			return false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static SaveDataPrefsUnity s_instance;
}

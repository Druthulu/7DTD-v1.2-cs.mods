using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using Noemax.GZip;
using Platform;
using UnityEngine;

public static class Localization
{
	public static bool LocalizationChecks
	{
		get
		{
			return Localization.localizationChecks;
		}
	}

	public static event Action<string> LanguageSelected;

	public static Dictionary<string, string[]> dictionary
	{
		get
		{
			Localization.CheckLoaded(false);
			return Localization.mDictionary;
		}
	}

	public static string[] knownLanguages
	{
		get
		{
			Localization.CheckLoaded(true);
			return Localization.AllLanguages.ToArray();
		}
	}

	public static string language
	{
		get
		{
			Localization.CheckLoaded(true);
			if (Localization.currentLanguageIndex >= 0)
			{
				return Localization.AllLanguages[Localization.currentLanguageIndex];
			}
			if (Localization.AllLanguages.ContainsCaseInsensitive(Localization.DefaultLanguage))
			{
				return Localization.DefaultLanguage;
			}
			return "";
		}
		set
		{
			Debug.Log("Language selected: " + value);
		}
	}

	public static string RequestedLanguage
	{
		get
		{
			string launchArgument = GameUtils.GetLaunchArgument("language");
			if (!string.IsNullOrEmpty(launchArgument))
			{
				return launchArgument;
			}
			if (!string.IsNullOrEmpty(Localization.platformLanguage))
			{
				return Localization.platformLanguage;
			}
			return Localization.DefaultLanguage;
		}
	}

	public static int TotalKeys
	{
		get
		{
			Localization.CheckLoaded(false);
			return Localization.mDictionary.Count;
		}
	}

	public static byte[] PatchedData
	{
		get
		{
			return Localization.patchedData;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void CheckLoaded(bool _throwExc = false)
	{
		if (Localization.mDictionary == null)
		{
			Localization.LoadAndSelectLanguage(Localization.DefaultLanguage, false);
		}
		if (Localization.mDictionary == null && _throwExc)
		{
			throw new Exception("Localization could not be loaded");
		}
	}

	public static void Init()
	{
		if (!Localization.initialized)
		{
			Localization.initialized = true;
			string launchArgument = GameUtils.GetLaunchArgument("language");
			if (!string.IsNullOrEmpty(launchArgument))
			{
				Localization.LoadAndSelectLanguage(launchArgument, false);
				Log.Out("Localization language from command line: " + Localization.language);
			}
			else
			{
				string @string = GamePrefs.GetString(EnumGamePrefs.Language);
				if (!string.IsNullOrEmpty(@string))
				{
					Localization.platformLanguage = @string;
					Localization.LoadAndSelectLanguage(@string, false);
					Log.Out("Localization language from prefs: " + Localization.language);
				}
				else
				{
					Localization.LoadAndSelectLanguage(Localization.DefaultLanguage, false);
					PlatformManager.NativePlatform.Api.ClientApiInitialized += delegate()
					{
						IUtils utils = PlatformManager.NativePlatform.Utils;
						Localization.platformLanguage = (((utils != null) ? utils.GetAppLanguage() : null) ?? Localization.DefaultLanguage);
						Localization.LoadAndSelectLanguage(Localization.platformLanguage, false);
						Log.Out("Localization language from platform: " + Localization.language);
					};
				}
			}
			Localization.localizationChecks = (GameUtils.GetLaunchArgument("localizationchecks") != null);
		}
	}

	public static bool ReloadBaseLocalization()
	{
		return Localization.LoadAndSelectLanguage(Localization.language, true);
	}

	public static bool LoadAndSelectLanguage(string _language, bool _forceReload = false)
	{
		if (_forceReload)
		{
			Localization.mDictionary = null;
			Localization.mDictionaryCaseInsensitive = null;
		}
		return (Localization.mDictionary != null || Localization.LoadBaseDictionaries()) && Localization.SelectLanguage(_language);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool LoadBaseDictionaries()
	{
		Localization.AllLanguages.Clear();
		Localization.patchedCells.Clear();
		Localization.mDictionary = new Dictionary<string, string[]>();
		Localization.mDictionaryCaseInsensitive = new CaseInsensitiveStringDictionary<string[]>();
		if (!Localization.LoadCsv(GameIO.GetGameDir("Data/Config") + "/Localization.txt", false))
		{
			Localization.mDictionary = null;
			Localization.mDictionaryCaseInsensitive = null;
			return false;
		}
		Localization.UpdateLanguages();
		Localization.WriteCsv();
		return true;
	}

	public static bool LoadPatchDictionaries(string _modName, string _folder, bool _loadingInGame)
	{
		Localization.CheckLoaded(true);
		string text = _folder + "/Localization.txt";
		if (SdFile.Exists(text))
		{
			Log.Out("[MODS] Loading localization from mod: " + _modName);
			if (!Localization.LoadCsv(text, true))
			{
				Log.Error("[MODS] Could not load localization from " + text);
			}
		}
		Localization.UpdateLanguages();
		Localization.SelectLanguage(Localization.RequestedLanguage);
		return true;
	}

	public static bool LoadServerPatchDictionary(byte[] _data)
	{
		Localization.CheckLoaded(true);
		if (!Localization.LoadCsv(_data, true, true))
		{
			Log.Error("Could not load localization from server!");
		}
		Localization.UpdateLanguages();
		Localization.SelectLanguage(Localization.RequestedLanguage);
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool SelectLanguage(string _language)
	{
		if (Localization.mDictionary == null)
		{
			return false;
		}
		Localization.currentLanguageIndex = -1;
		Localization.defaultLanguageIndex = -1;
		string[] array;
		if (Localization.mDictionary.Count > 0 && Localization.mDictionary.TryGetValue(Localization.HeaderKey, out array))
		{
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].EqualsCaseInsensitive(Localization.DefaultLanguage))
				{
					Localization.defaultLanguageIndex = i;
				}
				if (array[i].EqualsCaseInsensitive(_language))
				{
					Localization.currentLanguageIndex = i;
				}
			}
		}
		UIRoot.Broadcast("OnLocalize");
		Action<string> languageSelected = Localization.LanguageSelected;
		if (languageSelected != null)
		{
			languageSelected(_language);
		}
		return Localization.currentLanguageIndex >= 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void UpdateLanguages()
	{
		Localization.AllLanguages.Clear();
		Localization.languageToColumnIndex.Clear();
		if (Localization.mDictionary == null)
		{
			return;
		}
		for (int i = 0; i < Localization.mDictionary[Localization.HeaderKey].Length; i++)
		{
			string text = Localization.mDictionary[Localization.HeaderKey][i];
			Localization.AllLanguages.Add(text);
			Localization.languageToColumnIndex[text] = i;
		}
	}

	public static void WriteCsv()
	{
		new MicroStopwatch(true);
		using (PooledMemoryStream pooledMemoryStream = MemoryPools.poolMS.AllocSync(true))
		{
			using (TextWriter textWriter = new StreamWriter(pooledMemoryStream, Encoding.UTF8, 1024, true))
			{
				textWriter.Write(Localization.HeaderKey);
				string[] array = Localization.mDictionary[Localization.HeaderKey];
				for (int i = 0; i < array.Length; i++)
				{
					textWriter.Write(",");
					textWriter.Write(array[i]);
				}
				textWriter.WriteLine();
				int num = array.Length;
				foreach (KeyValuePair<string, bool[]> keyValuePair in Localization.patchedCells)
				{
					if (Localization.mDictionaryCaseInsensitive.TryGetValue(keyValuePair.Key, out array))
					{
						textWriter.Write(keyValuePair.Key);
						int j;
						for (j = 0; j < array.Length; j++)
						{
							textWriter.Write(',');
							if (keyValuePair.Value[j])
							{
								string text = array[j] ?? "";
								text = text.Replace("\n", "\\n");
								bool flag = text.IndexOf('"') >= 0 || text.IndexOf(',') >= 0;
								if (flag)
								{
									textWriter.Write('"');
									text = text.Replace("\"", "\"\"");
								}
								textWriter.Write(text);
								if (flag)
								{
									textWriter.Write('"');
								}
							}
						}
						while (j < num)
						{
							textWriter.Write(',');
							j++;
						}
						textWriter.WriteLine();
					}
				}
			}
			pooledMemoryStream.Position = 0L;
			using (PooledMemoryStream pooledMemoryStream2 = MemoryPools.poolMS.AllocSync(true))
			{
				using (DeflateOutputStream deflateOutputStream = new DeflateOutputStream(pooledMemoryStream2, 3))
				{
					StreamUtils.StreamCopy(pooledMemoryStream, deflateOutputStream, null, true);
					Localization.patchedData = pooledMemoryStream2.ToArray();
				}
			}
		}
	}

	public static string Get(string _key, bool _caseInsensitive = false)
	{
		Localization.CheckLoaded(false);
		if (string.IsNullOrEmpty(_key))
		{
			return "";
		}
		string[] array;
		if ((_caseInsensitive ? Localization.mDictionaryCaseInsensitive : Localization.mDictionary).TryGetValue(_key, out array))
		{
			if (Localization.currentLanguageIndex >= 0 && Localization.currentLanguageIndex < array.Length && !string.IsNullOrEmpty(array[Localization.currentLanguageIndex]))
			{
				if (!Localization.localizationChecks)
				{
					return array[Localization.currentLanguageIndex];
				}
				return "L_" + array[Localization.currentLanguageIndex];
			}
			else if (Localization.defaultLanguageIndex >= 0 && Localization.defaultLanguageIndex < array.Length && !string.IsNullOrEmpty(array[Localization.defaultLanguageIndex]))
			{
				if (!Localization.localizationChecks)
				{
					return array[Localization.defaultLanguageIndex];
				}
				return "LE_" + array[Localization.defaultLanguageIndex];
			}
		}
		if (Localization.localizationChecks)
		{
			return "UL_" + _key;
		}
		return _key;
	}

	public static string Get(string _key, string _languageName, bool _caseInsensitive = false)
	{
		Localization.CheckLoaded(false);
		if (string.IsNullOrEmpty(_key))
		{
			return "";
		}
		if (string.IsNullOrEmpty(_languageName))
		{
			return Localization.Get(_key, false);
		}
		int num;
		if (!Localization.languageToColumnIndex.TryGetValue(_languageName, out num))
		{
			Log.Warning(string.Concat(new string[]
			{
				"[Localization] Requested '",
				_key,
				"' for non-existing language '",
				_languageName,
				"'"
			}));
			return _key;
		}
		string[] array;
		if ((_caseInsensitive ? Localization.mDictionaryCaseInsensitive : Localization.mDictionary).TryGetValue(_key, out array) && num < array.Length && !string.IsNullOrEmpty(array[num]))
		{
			if (!Localization.localizationChecks)
			{
				return array[num];
			}
			return "L_" + array[num];
		}
		else
		{
			if (Localization.localizationChecks)
			{
				return "UL_" + _key;
			}
			return _key;
		}
	}

	public static bool Exists(string _key, bool _caseInsensitive = false)
	{
		Localization.CheckLoaded(false);
		if (string.IsNullOrEmpty(_key))
		{
			return false;
		}
		if (!_caseInsensitive)
		{
			return Localization.mDictionary.ContainsKey(_key);
		}
		return Localization.mDictionaryCaseInsensitive.ContainsKey(_key);
	}

	public static bool TryGet(string _key, out string _localizedString)
	{
		if (!Localization.Exists(_key, false))
		{
			_localizedString = _key;
			return false;
		}
		_localizedString = Localization.Get(_key, false);
		return true;
	}

	public static string FormatListAnd(params object[] items)
	{
		return Localization.FormatListX("listAndTwo", "listAndStart", "listAndMiddle", "listAndEnd", items);
	}

	public static string FormatListOr(params object[] items)
	{
		return Localization.FormatListX("listOrTwo", "listOrStart", "listOrMiddle", "listOrEnd", items);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string FormatListX(string listXTwo, string listXStart, string listXMiddle, string listXEnd, object[] items)
	{
		Localization.<>c__DisplayClass49_0 CS$<>8__locals1;
		CS$<>8__locals1.listXStart = listXStart;
		CS$<>8__locals1.items = items;
		CS$<>8__locals1.listXMiddle = listXMiddle;
		CS$<>8__locals1.listXEnd = listXEnd;
		int num = CS$<>8__locals1.items.Length;
		string result;
		if (num > 0)
		{
			if (num != 1)
			{
				if (num != 2)
				{
					result = Localization.<FormatListX>g__FormatThreeOrMore|49_0(ref CS$<>8__locals1);
				}
				else
				{
					result = string.Format(Localization.Get(listXTwo, false), CS$<>8__locals1.items);
				}
			}
			else
			{
				result = CS$<>8__locals1.items[0].ToString();
			}
		}
		else
		{
			result = string.Empty;
		}
		return result;
	}

	public static IEnumerable<KeyValuePair<string, string[]>> EnumerateAll()
	{
		foreach (KeyValuePair<string, string[]> keyValuePair in Localization.mDictionary)
		{
			yield return keyValuePair;
		}
		Dictionary<string, string[]>.Enumerator enumerator = default(Dictionary<string, string[]>.Enumerator);
		yield break;
		yield break;
	}

	public static string GetCurrentLocale()
	{
		string language = Localization.language;
		uint num = <PrivateImplementationDetails>.ComputeStringHash(language);
		if (num <= 2499415067U)
		{
			if (num <= 599131013U)
			{
				if (num != 380651494U)
				{
					if (num != 505713757U)
					{
						if (num == 599131013U)
						{
							if (language == "french")
							{
								return "fr-FR";
							}
						}
					}
					else if (language == "brazilian")
					{
						return "pt-BR";
					}
				}
				else if (language == "russian")
				{
					return "ru-RU";
				}
			}
			else if (num != 1901528810U)
			{
				if (num != 2471602315U)
				{
					if (num == 2499415067U)
					{
						if (language == "english")
						{
							return "en-US";
						}
					}
				}
				else if (language == "italian")
				{
					return "it-IT";
				}
			}
			else if (language == "japanese")
			{
				return "ja-JP";
			}
		}
		else if (num <= 3210859552U)
		{
			if (num != 2805355685U)
			{
				if (num != 3180870988U)
				{
					if (num == 3210859552U)
					{
						if (language == "koreana")
						{
							return "ko-KR";
						}
					}
				}
				else if (language == "polish")
				{
					return "pl-PL";
				}
			}
			else if (language == "schinese")
			{
				return "zh-Hans";
			}
		}
		else if (num <= 3405445907U)
		{
			if (num != 3264533134U)
			{
				if (num == 3405445907U)
				{
					if (language == "german")
					{
						return "de-DE";
					}
				}
			}
			else if (language == "tchinese")
			{
				return "zh-Hant";
			}
		}
		else if (num != 3719199419U)
		{
			if (num == 3739448251U)
			{
				if (language == "turkish")
				{
					return "tr-TR";
				}
			}
		}
		else if (language == "spanish")
		{
			return "es-ES";
		}
		throw new Exception("No language code mapping found for Language \"" + Localization.language + "\".");
	}

	public static bool LoadCsv(string _filename, bool _patch = false)
	{
		return Localization.LoadCsv(SdFile.ReadAllBytes(_filename), _patch, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool LoadCsv(byte[] _asset, bool _patch = false, bool _serverData = false)
	{
		ByteReader byteReader = new ByteReader(_asset);
		BetterList<string> betterList = byteReader.ReadCSV();
		if (betterList.size < 2)
		{
			return false;
		}
		betterList.buffer[0] = Localization.HeaderKey;
		if (!string.Equals(betterList.buffer[0], Localization.HeaderKey, StringComparison.OrdinalIgnoreCase))
		{
			Log.Error(string.Concat(new string[]
			{
				"Invalid localization CSV file. The first value is expected to be '",
				Localization.HeaderKey,
				"', followed by language columns.\nInstead found '",
				betterList.buffer[0],
				"'"
			}));
			return false;
		}
		int[] array = null;
		int newLength = 0;
		int origColIndexUsedInMainMenu = -1;
		if (!_patch)
		{
			Localization.mDictionary.Clear();
			Localization.mDictionaryCaseInsensitive.Clear();
		}
		else
		{
			string[] array2 = Localization.mDictionary[Localization.HeaderKey];
			newLength = array2.Length;
			array = new int[betterList.size];
			for (int i = 1; i < betterList.size; i++)
			{
				string text = betterList.buffer[i];
				if (!Localization.languageHeaderMatcher.IsMatch(text) && !text.StartsWith("context", StringComparison.OrdinalIgnoreCase))
				{
					Log.Error(string.Format("Invalid localization CSV file. The first row has to contain the column definition header. Column {0} is not a valid column definition (must be a non-empty string consisting of latin characters only): '{1}'", i + 1, text));
					return false;
				}
				int j;
				for (j = 0; j < array2.Length; j++)
				{
					if (text.EqualsCaseInsensitive(array2[j]))
					{
						array[i] = j;
						break;
					}
				}
				if (j >= array2.Length)
				{
					array[i] = newLength++;
				}
			}
			for (int k = 0; k < array2.Length; k++)
			{
				if (array2[k].EqualsCaseInsensitive(Localization.UsedInMainMenuKey))
				{
					origColIndexUsedInMainMenu = k;
					break;
				}
			}
		}
		while (betterList != null)
		{
			Localization.AddCsv(betterList, array, newLength, origColIndexUsedInMainMenu, _serverData);
			betterList = byteReader.ReadCSV();
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void AddCsv(BetterList<string> _temp, int[] _colTranslationTable, int _newLength, int _origColIndexUsedInMainMenu, bool _serverData = false)
	{
		if (_temp.size < 2)
		{
			return;
		}
		string text = _temp.buffer[0];
		if (string.IsNullOrEmpty(text))
		{
			Log.Warning("Localization: Entry missing a key! Please check Localization file. Skipping entry...");
			return;
		}
		if (_colTranslationTable != null)
		{
			string[] array;
			if (Localization.mDictionaryCaseInsensitive.TryGetValue(text, out array))
			{
				if (_serverData && _origColIndexUsedInMainMenu >= 0 && !string.IsNullOrEmpty(array[_origColIndexUsedInMainMenu]))
				{
					return;
				}
				if (array.Length < _newLength)
				{
					Array.Resize<string>(ref array, _newLength);
				}
				bool[] array2;
				if (Localization.patchedCells.TryGetValue(text, out array2))
				{
					if (array2.Length < _newLength)
					{
						Array.Resize<bool>(ref array2, _newLength);
					}
				}
				else
				{
					array2 = new bool[_newLength];
				}
				int num = 1;
				while (num < _temp.size && num < _colTranslationTable.Length)
				{
					if (!string.IsNullOrEmpty(_temp.buffer[num]))
					{
						int num2 = _colTranslationTable[num];
						array[num2] = _temp.buffer[num];
						array2[num2] = true;
					}
					num++;
				}
				if (_origColIndexUsedInMainMenu < 0 || string.IsNullOrEmpty(array[_origColIndexUsedInMainMenu]))
				{
					Localization.patchedCells[text] = array2;
				}
			}
			else
			{
				array = new string[_newLength];
				bool[] array3 = new bool[_newLength];
				int num3 = 1;
				while (num3 < _temp.size && num3 < _colTranslationTable.Length)
				{
					int num4 = _colTranslationTable[num3];
					array[num4] = _temp.buffer[num3];
					array3[num4] = true;
					num3++;
				}
				Localization.patchedCells.Add(text, array3);
			}
			Localization.mDictionary[text] = array;
			Localization.mDictionaryCaseInsensitive[text] = array;
			return;
		}
		string[] array4 = new string[_temp.size - 1];
		for (int i = 1; i < _temp.size; i++)
		{
			array4[i - 1] = _temp.buffer[i];
		}
		if (!Localization.mDictionary.ContainsKey(text))
		{
			Localization.mDictionary.Add(text, array4);
			Localization.mDictionaryCaseInsensitive.Add(text, array4);
			return;
		}
		Log.Warning("Localization: Duplicate key \"" + text + "\" found! Please check Localization file. Skipping entry...");
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Internal)]
	public static string <FormatListX>g__FormatThreeOrMore|49_0(ref Localization.<>c__DisplayClass49_0 A_0)
	{
		StringBuilder stringBuilder = new StringBuilder();
		StringBuilder stringBuilder2 = new StringBuilder();
		stringBuilder.AppendFormat(Localization.Get(A_0.listXStart, false), A_0.items[0], A_0.items[1]);
		for (int i = 2; i < A_0.items.Length - 1; i++)
		{
			stringBuilder2.AppendFormat(Localization.Get(A_0.listXMiddle, false), stringBuilder, A_0.items[i]);
			StringBuilder stringBuilder3 = stringBuilder2;
			StringBuilder stringBuilder4 = stringBuilder;
			stringBuilder = stringBuilder3;
			stringBuilder2 = stringBuilder4;
			stringBuilder2.Clear();
		}
		StringBuilder stringBuilder5 = stringBuilder2;
		string format = Localization.Get(A_0.listXEnd, false);
		object arg = stringBuilder;
		object[] items = A_0.items;
		stringBuilder5.AppendFormat(format, arg, items[items.Length - 1]);
		return stringBuilder2.ToString();
	}

	public static readonly string DefaultLanguage = "english";

	public static readonly string HeaderKey = "KEY";

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string UsedInMainMenuKey = "UsedInMainMenu";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string MainLocalizationFilename = "Localization.txt";

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool localizationChecks;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<string> AllLanguages = new List<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<string, int> languageToColumnIndex = new CaseInsensitiveStringDictionary<int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, string[]> mDictionary;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, string[]> mDictionaryCaseInsensitive;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<string, bool[]> patchedCells = new Dictionary<string, bool[]>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static int defaultLanguageIndex = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int currentLanguageIndex = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public static byte[] patchedData;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string platformLanguage;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool initialized;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Regex languageHeaderMatcher = new Regex("^[A-Za-z]+$");
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using UnityEngine;

public static class SDCSDataUtils
{
	public static string baseHairColorLoc
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return "Entities/Player/HairColorSwatches";
		}
	}

	public static void SetupData()
	{
		SDCSDataUtils.Load();
	}

	public static List<string> GetRaceList(bool isMale)
	{
		List<string> list = new List<string>();
		foreach (SDCSDataUtils.GenderKey genderKey in SDCSDataUtils.VariantData.Keys)
		{
			if (genderKey.IsMale == isMale)
			{
				list.Add(genderKey.Name.ToLower());
			}
		}
		list.Sort((string a, string b) => b.CompareTo(a));
		return list;
	}

	public static List<string> GetVariantList(bool isMale, string raceName)
	{
		List<string> list = new List<string>();
		foreach (SDCSDataUtils.GenderKey genderKey in SDCSDataUtils.VariantData.Keys)
		{
			if (genderKey.IsMale == isMale && genderKey.Name.EqualsCaseInsensitive(raceName))
			{
				for (int i = 0; i < SDCSDataUtils.VariantData[genderKey].Count; i++)
				{
					list.Add(SDCSDataUtils.VariantData[genderKey][i].ToString());
				}
			}
		}
		return list;
	}

	public static List<string> GetHairNames(bool isMale, SDCSDataUtils.HairTypes hairType)
	{
		List<string> list = new List<string>();
		Dictionary<SDCSDataUtils.GenderKey, SDCSDataUtils.HairData> dictionary = null;
		switch (hairType)
		{
		case SDCSDataUtils.HairTypes.Hair:
			dictionary = SDCSDataUtils.HairDictionary;
			break;
		case SDCSDataUtils.HairTypes.Mustache:
			dictionary = SDCSDataUtils.MustacheDictionary;
			break;
		case SDCSDataUtils.HairTypes.Chops:
			dictionary = SDCSDataUtils.ChopsDictionary;
			break;
		case SDCSDataUtils.HairTypes.Beard:
			dictionary = SDCSDataUtils.BeardDictionary;
			break;
		}
		foreach (SDCSDataUtils.GenderKey genderKey in dictionary.Keys)
		{
			SDCSDataUtils.HairData hairData = dictionary[genderKey];
			if (genderKey.IsMale == isMale)
			{
				list.Add(genderKey.Name);
			}
		}
		return list;
	}

	public static List<string> GetEyeColorNames()
	{
		return SDCSDataUtils.EyeColorList;
	}

	public static List<SDCSDataUtils.HairColorData> GetHairColorNames()
	{
		List<SDCSDataUtils.HairColorData> list = new List<SDCSDataUtils.HairColorData>();
		foreach (SDCSDataUtils.HairColorData item in SDCSDataUtils.HairColorDictionary.Values)
		{
			list.Add(item);
		}
		return list;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void SetupDataFromResources()
	{
		SDCSDataUtils.LoadRaceDataFromResources();
		SDCSDataUtils.EyeColorList = SDCSDataUtils.GetEyeColorNamesFromResources();
		SDCSDataUtils.LoadHairTypeFromResources(SDCSDataUtils.HairDictionary, SDCSDataUtils.HairTypes.Hair);
		SDCSDataUtils.LoadHairTypeFromResources(SDCSDataUtils.MustacheDictionary, SDCSDataUtils.HairTypes.Mustache);
		SDCSDataUtils.LoadHairTypeFromResources(SDCSDataUtils.ChopsDictionary, SDCSDataUtils.HairTypes.Chops);
		SDCSDataUtils.LoadHairTypeFromResources(SDCSDataUtils.BeardDictionary, SDCSDataUtils.HairTypes.Beard);
		SDCSDataUtils.LoadHairColorFromResources(SDCSDataUtils.HairColorDictionary);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void LoadRaceDataFromResources()
	{
		SDCSDataUtils.VariantData.Clear();
		SDCSDataUtils.ParseRaceVariantFromResources(true);
		SDCSDataUtils.ParseRaceVariantFromResources(false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseRaceVariantFromResources(bool isMale)
	{
		string text = isMale ? "Male" : "Female";
		DirectoryInfo directoryInfo = new DirectoryInfo(Application.dataPath + "/Resources/Entities/Player/" + text + "/Heads/Meshes/");
		string text2 = "player" + text;
		foreach (FileInfo fileInfo in directoryInfo.GetFiles())
		{
			if (!(fileInfo.Extension == ".meta") && fileInfo.Name.StartsWith(text2))
			{
				string text3 = fileInfo.Name.Substring(text2.Length);
				text3 = text3.Remove(text3.Length - fileInfo.Extension.Length);
				string value = new Regex("[0-9][0-9]*").Match(text3).Value;
				if (!(value == ""))
				{
					text3 = text3.Remove(text3.Length - value.Length).ToLower();
					SDCSDataUtils.GenderKey key = new SDCSDataUtils.GenderKey(text3, isMale);
					if (!SDCSDataUtils.VariantData.ContainsKey(key))
					{
						SDCSDataUtils.VariantData.Add(key, new List<int>());
					}
					SDCSDataUtils.VariantData[key].Add(StringParsers.ParseSInt32(value, 0, -1, NumberStyles.Integer));
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void LoadHairTypeFromResources(Dictionary<SDCSDataUtils.GenderKey, SDCSDataUtils.HairData> dict, SDCSDataUtils.HairTypes hairType)
	{
		dict.Clear();
		List<string> hairNamesFromResources = SDCSDataUtils.GetHairNamesFromResources(true, hairType);
		for (int i = 0; i < hairNamesFromResources.Count; i++)
		{
			dict.Add(new SDCSDataUtils.GenderKey(hairNamesFromResources[i], true), new SDCSDataUtils.HairData
			{
				Name = hairNamesFromResources[i],
				IsMale = true
			});
		}
		hairNamesFromResources = SDCSDataUtils.GetHairNamesFromResources(false, hairType);
		for (int j = 0; j < hairNamesFromResources.Count; j++)
		{
			dict.Add(new SDCSDataUtils.GenderKey(hairNamesFromResources[j], false), new SDCSDataUtils.HairData
			{
				Name = hairNamesFromResources[j],
				IsMale = false
			});
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void LoadHairColorFromResources(Dictionary<string, SDCSDataUtils.HairColorData> dict)
	{
		dict.Clear();
		List<string> hairColorNamesFromResources = SDCSDataUtils.GetHairColorNamesFromResources();
		for (int i = 0; i < hairColorNamesFromResources.Count; i++)
		{
			string text = hairColorNamesFromResources[i];
			int index = StringParsers.ParseSInt32(text.Substring(0, 2), 0, -1, NumberStyles.Integer);
			string name = text.Substring(3);
			dict.Add(hairColorNamesFromResources[i], new SDCSDataUtils.HairColorData
			{
				Index = index,
				Name = name,
				PrefabName = text
			});
		}
	}

	public static List<string> GetHairNamesFromResources(bool isMale, SDCSDataUtils.HairTypes hairType)
	{
		List<string> list = new List<string>();
		string text = isMale ? "Male" : "Female";
		foreach (DirectoryInfo directoryInfo in new DirectoryInfo(string.Concat(new string[]
		{
			Application.dataPath,
			"/Resources/Entities/Player/",
			text,
			"/HairMorphMatrix/",
			hairType.ToString(),
			"/"
		})).GetDirectories())
		{
			list.Add(directoryInfo.Name);
		}
		return list;
	}

	public static List<string> GetEyeColorNamesFromResources()
	{
		List<string> list = new List<string>();
		foreach (FileInfo fileInfo in new DirectoryInfo(Application.dataPath + "/Resources/Entities/Player/Eyes/").GetFiles())
		{
			if (!fileInfo.Name.EndsWith(".meta"))
			{
				list.Add(fileInfo.Name.Replace(".mat", ""));
			}
		}
		return list;
	}

	public static List<string> GetHairColorNamesFromResources()
	{
		List<string> list = new List<string>();
		foreach (FileInfo fileInfo in new DirectoryInfo(Application.dataPath + "/Resources/" + SDCSDataUtils.baseHairColorLoc + "/").GetFiles())
		{
			if (!fileInfo.Name.EndsWith(".meta"))
			{
				list.Add(fileInfo.Name.Replace(".asset", ""));
			}
		}
		return list;
	}

	public static void Save()
	{
		SDCSDataUtils.SetupDataFromResources();
		StreamWriter streamWriter = new StreamWriter(Application.dataPath + "/Resources/Entities/Player/sdcs.xml");
		string text = "\t";
		streamWriter.WriteLine("<sdcs>");
		foreach (SDCSDataUtils.GenderKey genderKey in SDCSDataUtils.VariantData.Keys)
		{
			if (SDCSDataUtils.VariantData[genderKey].Count > 0)
			{
				for (int i = 0; i < SDCSDataUtils.VariantData[genderKey].Count; i++)
				{
					streamWriter.WriteLine(string.Format("{0}<variant race=\"{1}\" index=\"{2}\" is_male=\"{3}\" />", new object[]
					{
						text,
						genderKey.Name,
						SDCSDataUtils.VariantData[genderKey][i],
						genderKey.IsMale
					}));
				}
			}
		}
		for (int j = 0; j < SDCSDataUtils.EyeColorList.Count; j++)
		{
			streamWriter.WriteLine(string.Format("{0}<eye_color name=\"{1}\" />", text, SDCSDataUtils.EyeColorList[j]));
		}
		foreach (SDCSDataUtils.HairColorData hairColorData in SDCSDataUtils.HairColorDictionary.Values)
		{
			streamWriter.WriteLine(string.Format("{0}<hair_color index=\"{1}\" name=\"{2}\" prefab_name=\"{3}\" />", new object[]
			{
				text,
				hairColorData.Index,
				hairColorData.Name,
				hairColorData.PrefabName
			}));
		}
		foreach (SDCSDataUtils.HairData hairData in SDCSDataUtils.HairDictionary.Values)
		{
			streamWriter.WriteLine(string.Format("{0}<hair name=\"{1}\" is_male=\"{2}\" />", text, hairData.Name, hairData.IsMale));
		}
		foreach (SDCSDataUtils.HairData hairData2 in SDCSDataUtils.MustacheDictionary.Values)
		{
			streamWriter.WriteLine(string.Format("{0}<mustache name=\"{1}\" is_male=\"{2}\" />", text, hairData2.Name, hairData2.IsMale));
		}
		foreach (SDCSDataUtils.HairData hairData3 in SDCSDataUtils.ChopsDictionary.Values)
		{
			streamWriter.WriteLine(string.Format("{0}<chops name=\"{1}\" is_male=\"{2}\" />", text, hairData3.Name, hairData3.IsMale));
		}
		foreach (SDCSDataUtils.HairData hairData4 in SDCSDataUtils.BeardDictionary.Values)
		{
			streamWriter.WriteLine(string.Format("{0}<beard name=\"{1}\" is_male=\"{2}\" />", text, hairData4.Name, hairData4.IsMale));
		}
		streamWriter.WriteLine("</sdcs>");
		streamWriter.Flush();
		streamWriter.Close();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void Load()
	{
		XElement root = XDocument.Parse(((TextAsset)Resources.Load("Entities/Player/sdcs")).text, LoadOptions.SetLineInfo).Root;
		if (root == null || !root.HasElements)
		{
			return;
		}
		SDCSDataUtils.VariantData.Clear();
		SDCSDataUtils.EyeColorList.Clear();
		SDCSDataUtils.HairDictionary.Clear();
		SDCSDataUtils.HairColorDictionary.Clear();
		SDCSDataUtils.MustacheDictionary.Clear();
		SDCSDataUtils.ChopsDictionary.Clear();
		SDCSDataUtils.BeardDictionary.Clear();
		foreach (XElement xelement in root.Elements())
		{
			if (xelement.Name == "variant")
			{
				int item = -1;
				string name = "";
				bool isMale = false;
				if (xelement.HasAttribute("index"))
				{
					item = StringParsers.ParseSInt32(xelement.GetAttribute("index"), 0, -1, NumberStyles.Integer);
				}
				if (xelement.HasAttribute("race"))
				{
					name = xelement.GetAttribute("race");
				}
				if (xelement.HasAttribute("is_male"))
				{
					isMale = StringParsers.ParseBool(xelement.GetAttribute("is_male"), 0, -1, true);
				}
				SDCSDataUtils.GenderKey key = new SDCSDataUtils.GenderKey(name, isMale);
				if (!SDCSDataUtils.VariantData.ContainsKey(key))
				{
					SDCSDataUtils.VariantData.Add(key, new List<int>());
				}
				SDCSDataUtils.VariantData[key].Add(item);
			}
			else if (xelement.Name == "eye_color")
			{
				SDCSDataUtils.EyeColorList.Add(xelement.GetAttribute("name"));
			}
			else if (xelement.Name == "hair_color")
			{
				int index = -1;
				string text = "";
				string prefabName = "";
				if (xelement.HasAttribute("index"))
				{
					index = StringParsers.ParseSInt32(xelement.GetAttribute("index"), 0, -1, NumberStyles.Integer);
				}
				if (xelement.HasAttribute("name"))
				{
					text = xelement.GetAttribute("name");
				}
				if (xelement.HasAttribute("prefab_name"))
				{
					prefabName = xelement.GetAttribute("prefab_name");
				}
				SDCSDataUtils.HairColorDictionary.Add(text, new SDCSDataUtils.HairColorData
				{
					Index = index,
					Name = text,
					PrefabName = prefabName
				});
			}
			else if (xelement.Name == "hair")
			{
				SDCSDataUtils.HairData hairData = SDCSDataUtils.ParseHair(xelement);
				SDCSDataUtils.HairDictionary.Add(new SDCSDataUtils.GenderKey(hairData.Name, hairData.IsMale), hairData);
			}
			else if (xelement.Name == "mustache")
			{
				SDCSDataUtils.HairData hairData2 = SDCSDataUtils.ParseHair(xelement);
				SDCSDataUtils.MustacheDictionary.Add(new SDCSDataUtils.GenderKey(hairData2.Name, hairData2.IsMale), hairData2);
			}
			else if (xelement.Name == "chops")
			{
				SDCSDataUtils.HairData hairData3 = SDCSDataUtils.ParseHair(xelement);
				SDCSDataUtils.ChopsDictionary.Add(new SDCSDataUtils.GenderKey(hairData3.Name, hairData3.IsMale), hairData3);
			}
			else if (xelement.Name == "beard")
			{
				SDCSDataUtils.HairData hairData4 = SDCSDataUtils.ParseHair(xelement);
				SDCSDataUtils.BeardDictionary.Add(new SDCSDataUtils.GenderKey(hairData4.Name, hairData4.IsMale), hairData4);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static SDCSDataUtils.HairData ParseHair(XElement element)
	{
		string name = "";
		bool isMale = false;
		if (element.HasAttribute("name"))
		{
			name = element.GetAttribute("name");
		}
		if (element.HasAttribute("is_male"))
		{
			isMale = StringParsers.ParseBool(element.GetAttribute("is_male"), 0, -1, true);
		}
		return new SDCSDataUtils.HairData
		{
			Name = name,
			IsMale = isMale
		};
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<SDCSDataUtils.GenderKey, List<int>> VariantData = new Dictionary<SDCSDataUtils.GenderKey, List<int>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<SDCSDataUtils.GenderKey, SDCSDataUtils.HairData> HairDictionary = new Dictionary<SDCSDataUtils.GenderKey, SDCSDataUtils.HairData>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<SDCSDataUtils.GenderKey, SDCSDataUtils.HairData> MustacheDictionary = new Dictionary<SDCSDataUtils.GenderKey, SDCSDataUtils.HairData>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<SDCSDataUtils.GenderKey, SDCSDataUtils.HairData> ChopsDictionary = new Dictionary<SDCSDataUtils.GenderKey, SDCSDataUtils.HairData>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<SDCSDataUtils.GenderKey, SDCSDataUtils.HairData> BeardDictionary = new Dictionary<SDCSDataUtils.GenderKey, SDCSDataUtils.HairData>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<string> EyeColorList = new List<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, SDCSDataUtils.HairColorData> HairColorDictionary = new Dictionary<string, SDCSDataUtils.HairColorData>();

	public struct HairColorData
	{
		public int Index;

		public string Name;

		public string PrefabName;
	}

	public struct HairData
	{
		public string Name;

		public bool IsMale;
	}

	public struct GenderKey
	{
		public GenderKey(string name, bool isMale)
		{
			this.Name = name;
			this.IsMale = isMale;
		}

		public string Name;

		public bool IsMale;
	}

	public enum HairTypes
	{
		Hair,
		Mustache,
		Chops,
		Beard
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using System.Xml.Linq;

public class LootFromXml
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseItemList(string _containerId, IEnumerable<XElement> _childNodes, List<LootContainer.LootEntry> _itemList, int _minQualityBase, int _maxQualityBase)
	{
		foreach (XElement element in _childNodes)
		{
			LootContainer.LootEntry lootEntry = new LootContainer.LootEntry();
			lootEntry.prob = 1f;
			if (element.HasAttribute("prob") && !StringParsers.TryParseFloat(element.GetAttribute("prob"), out lootEntry.prob, 0, -1, NumberStyles.Any))
			{
				throw new Exception("Parsing error prob '" + element.GetAttribute("prob") + "'");
			}
			if (element.HasAttribute("force_prob"))
			{
				StringParsers.TryParseBool(element.GetAttribute("force_prob"), out lootEntry.forceProb, 0, -1, true);
			}
			if (element.HasAttribute("group"))
			{
				string attribute = element.GetAttribute("group");
				if (!LootContainer.lootGroups.TryGetValue(attribute, out lootEntry.group))
				{
					throw new Exception(string.Concat(new string[]
					{
						"lootgroup '",
						attribute,
						"' does not exist or has not been defined before being reference by lootcontainer/lootgroup name='",
						_containerId,
						"'"
					}));
				}
			}
			else
			{
				if (!element.HasAttribute("name"))
				{
					throw new Exception("Attribute 'name' or 'group' missing on item in lootcontainer/lootgroup name='" + _containerId + "'");
				}
				lootEntry.item = new LootContainer.LootItem();
				string attribute2 = element.GetAttribute("name");
				lootEntry.item.itemValue = ItemClass.GetItem(attribute2, false);
				if (lootEntry.item.itemValue.IsEmpty())
				{
					throw new Exception("Item with name '" + attribute2 + "' not found!");
				}
			}
			string attribute3 = element.GetAttribute("tags");
			if (attribute3.Length > 0)
			{
				lootEntry.tags = FastTags<TagGroup.Global>.Parse(attribute3);
			}
			lootEntry.minCount = 1;
			lootEntry.maxCount = 1;
			if ((lootEntry.item == null || ItemClass.GetForId(lootEntry.item.itemValue.type).CanStack()) && element.HasAttribute("count"))
			{
				StringParsers.ParseMinMaxCount(element.GetAttribute("count"), out lootEntry.minCount, out lootEntry.maxCount);
			}
			lootEntry.minQuality = _minQualityBase;
			lootEntry.maxQuality = _maxQualityBase;
			if (element.HasAttribute("quality"))
			{
				StringParsers.ParseMinMaxCount(element.GetAttribute("quality"), out lootEntry.minQuality, out lootEntry.maxQuality);
			}
			if (element.HasAttribute("loot_prob_template"))
			{
				lootEntry.lootProbTemplate = element.GetAttribute("loot_prob_template");
			}
			else
			{
				lootEntry.lootProbTemplate = string.Empty;
			}
			if (element.HasAttribute("mods"))
			{
				lootEntry.modsToInstall = element.GetAttribute("mods").Split(',', StringSplitOptions.None);
			}
			else
			{
				lootEntry.modsToInstall = new string[0];
			}
			if (element.HasAttribute("mod_chance"))
			{
				lootEntry.modChance = StringParsers.ParseFloat(element.GetAttribute("mod_chance"), 0, -1, NumberStyles.Any);
			}
			if (element.HasAttribute("loot_stage_count_mod"))
			{
				lootEntry.lootstageCountMod = StringParsers.ParseFloat(element.GetAttribute("loot_stage_count_mod"), 0, -1, NumberStyles.Any);
			}
			_itemList.Add(lootEntry);
		}
	}

	public static IEnumerator LoadLootContainers(XmlFile _xmlFile)
	{
		XElement root = _xmlFile.XmlDoc.Root;
		if (!root.HasElements)
		{
			throw new Exception("No document root or no children found!");
		}
		using (IEnumerator<XElement> enumerator = root.Elements().GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				XElement xelement = enumerator.Current;
				if (xelement.Name == "lootcontainer")
				{
					LootFromXml.LoadLootContainer(xelement);
				}
				if (xelement.Name == "lootgroup")
				{
					LootFromXml.LoadLootGroup(xelement);
				}
				if (xelement.Name == "lootprobtemplates")
				{
					LootFromXml.LoadLootProbabilityTemplate(xelement);
				}
				if (xelement.Name == "lootqualitytemplates")
				{
					LootFromXml.LoadLootQualityTemplate(xelement);
				}
				if (xelement.Name == "loot_settings")
				{
					LootFromXml.LoadLootSetting(xelement);
				}
			}
			yield break;
		}
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void LoadLootSetting(XElement _rootNode)
	{
		if (_rootNode.HasAttribute("poi_tier_count"))
		{
			int num = StringParsers.ParseSInt32(_rootNode.GetAttribute("poi_tier_count"), 0, -1, NumberStyles.Integer);
			LootManager.POITierMod = new float[num];
			LootManager.POITierBonus = new float[num];
		}
		else
		{
			LootManager.POITierMod = new float[5];
			LootManager.POITierBonus = new float[5];
		}
		if (_rootNode.HasAttribute("poi_tier_mod"))
		{
			string attribute = _rootNode.GetAttribute("poi_tier_mod");
			if (attribute.Contains(","))
			{
				string[] array = attribute.Split(',', StringSplitOptions.None);
				LootManager.POITierMod = new float[array.Length];
				for (int i = 0; i < array.Length; i++)
				{
					LootManager.POITierMod[i] = StringParsers.ParseFloat(array[i], 0, -1, NumberStyles.Any);
				}
			}
			else
			{
				LootManager.POITierMod = new float[]
				{
					StringParsers.ParseFloat(attribute, 0, -1, NumberStyles.Any)
				};
			}
		}
		if (_rootNode.HasAttribute("poi_tier_bonus"))
		{
			string attribute2 = _rootNode.GetAttribute("poi_tier_bonus");
			if (attribute2.Contains(","))
			{
				string[] array2 = attribute2.Split(',', StringSplitOptions.None);
				LootManager.POITierBonus = new float[array2.Length];
				for (int j = 0; j < array2.Length; j++)
				{
					LootManager.POITierBonus[j] = StringParsers.ParseFloat(array2[j], 0, -1, NumberStyles.Any);
				}
				return;
			}
			LootManager.POITierBonus = new float[]
			{
				StringParsers.ParseFloat(attribute2, 0, -1, NumberStyles.Any)
			};
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void LoadLootQualityTemplate(XElement _rootNode)
	{
		foreach (XElement xelement in _rootNode.Elements("lootqualitytemplate"))
		{
			LootContainer.LootQualityTemplate lootQualityTemplate = new LootContainer.LootQualityTemplate();
			if (!xelement.HasAttribute("name"))
			{
				throw new Exception("Attribute 'name' required on lootqualitytemplate");
			}
			lootQualityTemplate.name = xelement.GetAttribute("name");
			if (LootContainer.lootGroups.ContainsKey(lootQualityTemplate.name))
			{
				throw new Exception("lootqualitytemplate '" + lootQualityTemplate.name + "' is defined multiple times");
			}
			foreach (XElement xelement2 in xelement.Elements("qualitytemplate"))
			{
				LootContainer.LootGroup lootGroup = new LootContainer.LootGroup();
				lootGroup.minLevel = 0f;
				lootGroup.maxLevel = 1f;
				if (xelement2.HasAttribute("level"))
				{
					StringParsers.ParseMinMaxCount(xelement2.GetAttribute("level"), out lootGroup.minLevel, out lootGroup.maxLevel);
				}
				lootGroup.minQuality = -1;
				lootGroup.maxQuality = -1;
				if (xelement2.HasAttribute("default_quality"))
				{
					StringParsers.ParseMinMaxCount(xelement2.GetAttribute("default_quality"), out lootGroup.minQuality, out lootGroup.maxQuality);
				}
				foreach (XElement element in xelement2.Elements("loot"))
				{
					LootContainer.LootEntry lootEntry = new LootContainer.LootEntry();
					lootEntry.minQuality = lootGroup.minQuality;
					lootEntry.maxQuality = lootGroup.maxQuality;
					if (element.HasAttribute("quality"))
					{
						StringParsers.ParseMinMaxCount(element.GetAttribute("quality"), out lootEntry.minQuality, out lootEntry.maxQuality);
					}
					lootEntry.prob = 1f;
					if (element.HasAttribute("prob") && !StringParsers.TryParseFloat(element.GetAttribute("prob"), out lootEntry.prob, 0, -1, NumberStyles.Any))
					{
						throw new Exception(string.Concat(new string[]
						{
							"Parsing error prob '",
							element.GetAttribute("prob"),
							"' in '",
							lootQualityTemplate.name,
							"' level '",
							lootGroup.minLevel.ToCultureInvariantString(),
							",",
							lootGroup.maxLevel.ToCultureInvariantString(),
							"'"
						}));
					}
					lootGroup.items.Add(lootEntry);
				}
				lootQualityTemplate.templates.Add(lootGroup);
			}
			LootContainer.lootQualityTemplates[lootQualityTemplate.name] = lootQualityTemplate;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void LoadLootProbabilityTemplate(XElement _rootNode)
	{
		foreach (XElement xelement in _rootNode.Elements("lootprobtemplate"))
		{
			LootContainer.LootProbabilityTemplate lootProbabilityTemplate = new LootContainer.LootProbabilityTemplate();
			if (!xelement.HasAttribute("name"))
			{
				throw new Exception("Attribute 'name' required on lootprobtemplate");
			}
			lootProbabilityTemplate.name = xelement.GetAttribute("name");
			if (LootContainer.lootGroups.ContainsKey(lootProbabilityTemplate.name))
			{
				throw new Exception("lootprobtemplate '" + lootProbabilityTemplate.name + "' is defined multiple times");
			}
			foreach (XElement element in xelement.Elements("loot"))
			{
				LootContainer.LootEntry lootEntry = new LootContainer.LootEntry();
				lootEntry.minLevel = -1f;
				lootEntry.maxLevel = -1f;
				if (element.HasAttribute("level"))
				{
					StringParsers.ParseMinMaxCount(element.GetAttribute("level"), out lootEntry.minLevel, out lootEntry.maxLevel);
				}
				lootEntry.prob = 1f;
				if (element.HasAttribute("prob") && !StringParsers.TryParseFloat(element.GetAttribute("prob"), out lootEntry.prob, 0, -1, NumberStyles.Any))
				{
					throw new Exception(string.Concat(new string[]
					{
						"Parsing error prob '",
						element.GetAttribute("prob"),
						"' in '",
						lootProbabilityTemplate.name,
						"' level '",
						lootEntry.minLevel.ToCultureInvariantString(),
						",",
						lootEntry.maxLevel.ToCultureInvariantString(),
						"'"
					}));
				}
				lootProbabilityTemplate.templates.Add(lootEntry);
			}
			LootContainer.lootProbTemplates[lootProbabilityTemplate.name] = lootProbabilityTemplate;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void LoadLootGroup(XElement _elementGroup)
	{
		LootContainer.LootGroup lootGroup = new LootContainer.LootGroup();
		if (!_elementGroup.HasAttribute("name"))
		{
			throw new Exception("Attribute 'name' required on lootgroup");
		}
		lootGroup.name = _elementGroup.GetAttribute("name");
		if (LootContainer.lootGroups.ContainsKey(lootGroup.name))
		{
			throw new Exception("lootgroup '" + lootGroup.name + "' is defined multiple times");
		}
		if (_elementGroup.HasAttribute("loot_quality_template"))
		{
			lootGroup.lootQualityTemplate = _elementGroup.GetAttribute("loot_quality_template");
		}
		lootGroup.minCount = 1;
		lootGroup.maxCount = 1;
		if (_elementGroup.HasAttribute("count"))
		{
			if (_elementGroup.GetAttribute("count") == "all")
			{
				lootGroup.minCount = -1;
				lootGroup.maxCount = -1;
			}
			else
			{
				StringParsers.ParseMinMaxCount(_elementGroup.GetAttribute("count"), out lootGroup.minCount, out lootGroup.maxCount);
			}
		}
		lootGroup.minLevel = 0f;
		lootGroup.maxLevel = 10000f;
		if (_elementGroup.HasAttribute("level"))
		{
			StringParsers.ParseMinMaxCount(_elementGroup.GetAttribute("level"), out lootGroup.minLevel, out lootGroup.maxLevel);
		}
		lootGroup.minQuality = -1;
		lootGroup.maxQuality = -1;
		if (_elementGroup.HasAttribute("quality"))
		{
			StringParsers.ParseMinMaxCount(_elementGroup.GetAttribute("quality"), out lootGroup.minQuality, out lootGroup.maxQuality);
		}
		if (_elementGroup.HasAttribute("mods"))
		{
			lootGroup.modsToInstall = _elementGroup.GetAttribute("mods").Split(',', StringSplitOptions.None);
		}
		else
		{
			lootGroup.modsToInstall = new string[0];
		}
		if (_elementGroup.HasAttribute("mod_chance"))
		{
			lootGroup.modChance = StringParsers.ParseFloat(_elementGroup.GetAttribute("mod_chance"), 0, -1, NumberStyles.Any);
		}
		LootFromXml.ParseItemList(lootGroup.name, _elementGroup.Elements("item"), lootGroup.items, lootGroup.minQuality, lootGroup.maxQuality);
		for (int i = 0; i < lootGroup.items.Count; i++)
		{
			lootGroup.items[i].parentGroup = lootGroup;
		}
		LootContainer.lootGroups[lootGroup.name] = lootGroup;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void LoadLootContainer(XElement _elementContainer)
	{
		LootContainer lootContainer = new LootContainer();
		if (!_elementContainer.HasAttribute("name"))
		{
			throw new XmlException("Attribute 'name' missing on container");
		}
		string attribute = _elementContainer.GetAttribute("name");
		if (LootContainer.GetLootContainer(attribute, false) != null)
		{
			throw new Exception("Duplicate lootlist entry with name " + attribute);
		}
		lootContainer.Name = attribute;
		if (_elementContainer.HasAttribute("count"))
		{
			StringParsers.ParseMinMaxCount(_elementContainer.GetAttribute("count"), out lootContainer.minCount, out lootContainer.maxCount);
		}
		else
		{
			lootContainer.minCount = (lootContainer.maxCount = 1);
		}
		if (_elementContainer.HasAttribute("size"))
		{
			lootContainer.size = StringParsers.ParseVector2i(_elementContainer.GetAttribute("size"), ',');
			if (lootContainer.size == Vector2i.zero)
			{
				lootContainer.size = new Vector2i(3, 3);
			}
		}
		lootContainer.BuffActions = new List<string>();
		if (_elementContainer.HasAttribute("buff"))
		{
			lootContainer.BuffActions.AddRange(_elementContainer.GetAttribute("buff").Replace(" ", "").Split(',', StringSplitOptions.None));
		}
		if (_elementContainer.HasAttribute("sound_open"))
		{
			lootContainer.soundOpen = _elementContainer.GetAttribute("sound_open");
		}
		if (_elementContainer.HasAttribute("sound_close"))
		{
			lootContainer.soundClose = _elementContainer.GetAttribute("sound_close");
		}
		if (_elementContainer.HasAttribute("ignore_loot_abundance"))
		{
			lootContainer.ignoreLootAbundance = StringParsers.ParseBool(_elementContainer.GetAttribute("ignore_loot_abundance"), 0, -1, true);
		}
		if (_elementContainer.HasAttribute("unique_items"))
		{
			lootContainer.UniqueItems = StringParsers.ParseBool(_elementContainer.GetAttribute("unique_items"), 0, -1, true);
		}
		if (_elementContainer.HasAttribute("ignore_loot_prob"))
		{
			lootContainer.IgnoreLootProb = StringParsers.ParseBool(_elementContainer.GetAttribute("ignore_loot_prob"), 0, -1, true);
		}
		if (_elementContainer.HasAttribute("unmodified_lootstage"))
		{
			lootContainer.useUnmodifiedLootstage = StringParsers.ParseBool(_elementContainer.GetAttribute("unmodified_lootstage"), 0, -1, true);
		}
		string attribute2 = _elementContainer.GetAttribute("destroy_on_close");
		if (attribute2.Length > 0)
		{
			lootContainer.destroyOnClose = EnumUtils.Parse<LootContainer.DestroyOnClose>(attribute2, true);
		}
		if (_elementContainer.HasAttribute("open_time"))
		{
			float openTime;
			if (StringParsers.TryParseFloat(_elementContainer.GetAttribute("open_time"), out openTime, 0, -1, NumberStyles.Any))
			{
				lootContainer.openTime = openTime;
			}
			else
			{
				lootContainer.openTime = 1f;
			}
		}
		else
		{
			lootContainer.openTime = 1f;
		}
		if (_elementContainer.HasAttribute("loot_quality_template"))
		{
			string attribute3 = _elementContainer.GetAttribute("loot_quality_template");
			if (LootContainer.lootQualityTemplates.ContainsKey(attribute3))
			{
				lootContainer.lootQualityTemplate = attribute3;
			}
			else
			{
				Log.Error("LootContainer {0} uses an unknown loot_quality_template \"{1}\"", new object[]
				{
					attribute,
					attribute3
				});
			}
		}
		else
		{
			lootContainer.lootQualityTemplate = string.Empty;
		}
		LootFromXml.ParseItemList(attribute, _elementContainer.Elements("item"), lootContainer.itemsToSpawn, -1, -1);
		int count = lootContainer.itemsToSpawn.Count;
		lootContainer.Init();
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;

public class TradersFromXml
{
	public static IEnumerator LoadTraderInfo(XmlFile xmlFile)
	{
		XElement root = xmlFile.XmlDoc.Root;
		if (!root.HasElements)
		{
			throw new Exception("No element <traders> found!");
		}
		TradersFromXml.ParseNode(root);
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseNode(XElement e)
	{
		if (e.HasAttribute("buy_markup"))
		{
			TraderInfo.BuyMarkup = StringParsers.ParseFloat(e.GetAttribute("buy_markup"), 0, -1, NumberStyles.Any);
		}
		if (e.HasAttribute("sell_markdown"))
		{
			TraderInfo.SellMarkdown = StringParsers.ParseFloat(e.GetAttribute("sell_markdown"), 0, -1, NumberStyles.Any);
		}
		TraderManager.QuestTierMod = new float[5];
		TraderManager.TraderStageTemplates.Clear();
		if (e.HasAttribute("quest_tier_mod"))
		{
			string attribute = e.GetAttribute("quest_tier_mod");
			if (attribute.Contains(","))
			{
				string[] array = attribute.Split(',', StringSplitOptions.None);
				TraderManager.QuestTierMod = new float[array.Length];
				for (int i = 0; i < array.Length; i++)
				{
					TraderManager.QuestTierMod[i] = StringParsers.ParseFloat(array[i], 0, -1, NumberStyles.Any);
				}
			}
			else
			{
				TraderManager.QuestTierMod = new float[]
				{
					StringParsers.ParseFloat(attribute, 0, -1, NumberStyles.Any)
				};
			}
		}
		TraderInfo.QualityMaxMod = 1f;
		TraderInfo.QualityMinMod = 1f;
		if (e.HasAttribute("quality_mod"))
		{
			float qualityMinMod = 1f;
			float qualityMaxMod = 1f;
			StringParsers.ParseMinMaxCount(e.GetAttribute("quality_mod"), out qualityMinMod, out qualityMaxMod);
			TraderInfo.QualityMinMod = qualityMinMod;
			TraderInfo.QualityMaxMod = qualityMaxMod;
		}
		if (e.HasAttribute("currency_item"))
		{
			TraderInfo.CurrencyItem = e.GetAttribute("currency_item");
		}
		foreach (XElement xelement in e.Elements())
		{
			if (xelement.Name == "trader_info")
			{
				TradersFromXml.ParseTraderInfo(xelement);
			}
			else if (xelement.Name == "trader_item_groups")
			{
				TradersFromXml.ParseTraderItemGroups(xelement);
			}
			else
			{
				if (!(xelement.Name == "traderstage_templates"))
				{
					string str = "Unrecognized xml element ";
					XName name = xelement.Name;
					throw new Exception(str + ((name != null) ? name.ToString() : null));
				}
				TradersFromXml.ParseTraderStageTemplates(xelement);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseTraderInfo(XElement e)
	{
		if (!e.HasAttribute("id"))
		{
			throw new Exception("trader must have an id attribute");
		}
		int num = 0;
		if (!int.TryParse(e.GetAttribute("id"), out num))
		{
			throw new Exception("Parsing error id '" + e.GetAttribute("id") + "'");
		}
		if (TraderInfo.traderInfoList[num] != null)
		{
			throw new Exception("Duplicate lootlist entry with id " + num.ToString());
		}
		TraderInfo traderInfo = new TraderInfo();
		traderInfo.Id = num;
		TraderInfo.traderInfoList[num] = traderInfo;
		if (e.HasAttribute("reset_interval"))
		{
			traderInfo.ResetInterval = int.Parse(e.GetAttribute("reset_interval"));
			traderInfo.ResetIntervalInTicks = traderInfo.ResetInterval * 24000;
		}
		if (e.HasAttribute("allow_buy"))
		{
			traderInfo.AllowSell = StringParsers.ParseBool(e.GetAttribute("allow_buy"), 0, -1, true);
		}
		if (e.HasAttribute("allow_sell"))
		{
			traderInfo.AllowSell = StringParsers.ParseBool(e.GetAttribute("allow_sell"), 0, -1, true);
		}
		if (e.HasAttribute("override_buy_markup"))
		{
			traderInfo.OverrideBuyMarkup = StringParsers.ParseFloat(e.GetAttribute("override_buy_markup"), 0, -1, NumberStyles.Any);
		}
		if (e.HasAttribute("override_sell_markup"))
		{
			traderInfo.OverrideSellMarkdown = StringParsers.ParseFloat(e.GetAttribute("override_sell_markup"), 0, -1, NumberStyles.Any);
		}
		if (e.HasAttribute("player_owned"))
		{
			traderInfo.PlayerOwned = StringParsers.ParseBool(e.GetAttribute("player_owned"), 0, -1, true);
		}
		if (e.HasAttribute("rentable"))
		{
			traderInfo.Rentable = StringParsers.ParseBool(e.GetAttribute("rentable"), 0, -1, true);
		}
		if (e.HasAttribute("rent_cost"))
		{
			traderInfo.RentCost = int.Parse(e.GetAttribute("rent_cost"));
		}
		if (e.HasAttribute("rent_time"))
		{
			traderInfo.RentTimeInDays = int.Parse(e.GetAttribute("rent_time"));
		}
		if (e.HasAttribute("open_time"))
		{
			string[] array = e.GetAttribute("open_time").Split(':', StringSplitOptions.None);
			traderInfo.OpenTime = GameUtils.DayTimeToWorldTime(1, Convert.ToInt32(array[0]), (array.Length > 1) ? Convert.ToInt32(array[1]) : 0);
			traderInfo.UseOpenHours = true;
		}
		if (e.HasAttribute("close_time"))
		{
			string[] array2 = e.GetAttribute("close_time").Split(':', StringSplitOptions.None);
			traderInfo.CloseTime = GameUtils.DayTimeToWorldTime(1, Convert.ToInt32(array2[0]), (array2.Length > 1) ? Convert.ToInt32(array2[1]) : 0);
			traderInfo.WarningTime = traderInfo.CloseTime - 300UL;
			traderInfo.UseOpenHours = true;
		}
		foreach (XElement xelement in e.Elements())
		{
			if (xelement.Name == "trader_items")
			{
				TradersFromXml.ParseTraderItems(traderInfo, xelement);
			}
			else
			{
				if (!(xelement.Name == "tier_items"))
				{
					string str = "Unrecognized xml element ";
					XName name = xelement.Name;
					throw new Exception(str + ((name != null) ? name.ToString() : null));
				}
				TradersFromXml.ParseTierItems(traderInfo, xelement);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseTraderItems(TraderInfo info, XElement e)
	{
		info.minCount = 1;
		info.maxCount = 1;
		if (e.HasAttribute("count"))
		{
			if (e.GetAttribute("count") == "all")
			{
				info.minCount = -1;
				info.maxCount = -1;
			}
			else
			{
				StringParsers.ParseMinMaxCount(e.GetAttribute("count"), out info.minCount, out info.maxCount);
			}
		}
		TradersFromXml.parseItemList(info.Id.ToString(), e.Elements("item"), info.traderItems, -1, -1);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseTierItems(TraderInfo info, XElement e)
	{
		TraderInfo.TierItemGroup tierItemGroup = new TraderInfo.TierItemGroup();
		tierItemGroup.minCount = 1;
		tierItemGroup.maxCount = 1;
		if (e.HasAttribute("count"))
		{
			if (e.GetAttribute("count") == "all")
			{
				tierItemGroup.minCount = -1;
				tierItemGroup.maxCount = -1;
			}
			else
			{
				StringParsers.ParseMinMaxCount(e.GetAttribute("count"), out tierItemGroup.minCount, out tierItemGroup.maxCount);
			}
		}
		if (e.HasAttribute("level"))
		{
			StringParsers.ParseMinMaxCount(e.GetAttribute("level"), out tierItemGroup.minLevel, out tierItemGroup.maxLevel);
			TradersFromXml.parseItemList(info.Id.ToString(), e.Elements("item"), tierItemGroup.traderItems, -1, -1);
			info.TierItemGroups.Add(tierItemGroup);
			return;
		}
		throw new Exception("level range missing on tier items group.");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseTraderItemGroups(XElement e)
	{
		foreach (XElement e2 in e.Elements("trader_item_group"))
		{
			TradersFromXml.ParseTraderItemGroup(e2);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseTraderItemGroup(XElement e)
	{
		if (!e.HasAttribute("name"))
		{
			throw new Exception("trader item group must have a name attribute");
		}
		string attribute = e.GetAttribute("name");
		if (TraderInfo.traderItemGroups.ContainsKey(attribute))
		{
			throw new Exception("Duplicate trader_item_group entry with name " + attribute);
		}
		TraderInfo.TraderItemGroup traderItemGroup = new TraderInfo.TraderItemGroup();
		traderItemGroup.name = attribute;
		TraderInfo.traderItemGroups[attribute] = traderItemGroup;
		traderItemGroup.minCount = 1;
		traderItemGroup.maxCount = 1;
		if (e.HasAttribute("count"))
		{
			if (e.GetAttribute("count") == "all")
			{
				traderItemGroup.minCount = -1;
				traderItemGroup.maxCount = -1;
			}
			else
			{
				StringParsers.ParseMinMaxCount(e.GetAttribute("count"), out traderItemGroup.minCount, out traderItemGroup.maxCount);
			}
		}
		if (e.HasAttribute("mods"))
		{
			traderItemGroup.modsToInstall = e.GetAttribute("mods").Split(',', StringSplitOptions.None);
		}
		else
		{
			traderItemGroup.modsToInstall = new string[0];
		}
		if (e.HasAttribute("mod_chance"))
		{
			traderItemGroup.modChance = StringParsers.ParseFloat(e.GetAttribute("mod_chance"), 0, -1, NumberStyles.Any);
		}
		if (e.HasAttribute("unique_only"))
		{
			traderItemGroup.uniqueOnly = StringParsers.ParseBool(e.GetAttribute("unique_only"), 0, -1, true);
		}
		TradersFromXml.parseItemList(traderItemGroup.name, e.Elements("item"), traderItemGroup.items, -1, -1);
		for (int i = 0; i < traderItemGroup.items.Count; i++)
		{
			traderItemGroup.items[i].parentGroup = traderItemGroup;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseTraderStageTemplates(XElement e)
	{
		foreach (XElement e2 in e.Elements("traderstage_template"))
		{
			TradersFromXml.ParseTraderStageTemplate(e2);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseTraderStageTemplate(XElement e)
	{
		if (!e.HasAttribute("name"))
		{
			throw new Exception("traderstage template must have a name attribute");
		}
		string attribute = e.GetAttribute("name");
		if (TraderManager.TraderStageTemplates.ContainsKey(attribute))
		{
			throw new Exception("Duplicate traderstage_template entry with name " + attribute);
		}
		TraderStageTemplateGroup traderStageTemplateGroup = new TraderStageTemplateGroup();
		traderStageTemplateGroup.Name = attribute;
		TraderManager.TraderStageTemplates.Add(attribute, traderStageTemplateGroup);
		foreach (XElement element in e.Elements("entry"))
		{
			TraderStageTemplate traderStageTemplate = new TraderStageTemplate();
			if (element.HasAttribute("min"))
			{
				traderStageTemplate.Min = StringParsers.ParseSInt32(element.GetAttribute("min"), 0, -1, NumberStyles.Integer);
			}
			if (element.HasAttribute("max"))
			{
				traderStageTemplate.Max = StringParsers.ParseSInt32(element.GetAttribute("max"), 0, -1, NumberStyles.Integer);
			}
			if (element.HasAttribute("quality"))
			{
				traderStageTemplate.Quality = StringParsers.ParseSInt32(element.GetAttribute("quality"), 0, -1, NumberStyles.Integer);
			}
			traderStageTemplateGroup.Templates.Add(traderStageTemplate);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void parseItemList(string _containerId, IEnumerable<XElement> _childNodes, List<TraderInfo.TraderItemEntry> _itemList, int minQualityBase, int maxQualityBase)
	{
		foreach (XElement element in _childNodes)
		{
			TraderInfo.TraderItemEntry traderItemEntry = new TraderInfo.TraderItemEntry();
			traderItemEntry.prob = 1f;
			if (element.HasAttribute("prob") && !StringParsers.TryParseFloat(element.GetAttribute("prob"), out traderItemEntry.prob, 0, -1, NumberStyles.Any))
			{
				throw new Exception("Parsing error prob '" + element.GetAttribute("prob") + "'");
			}
			if (element.HasAttribute("group"))
			{
				string attribute = element.GetAttribute("group");
				if (!TraderInfo.traderItemGroups.TryGetValue(attribute, out traderItemEntry.group))
				{
					throw new Exception(string.Concat(new string[]
					{
						"traderItemGroup '",
						attribute,
						"' does not exist or has not been defined before being reference by trader_items/trader_item_group id='",
						_containerId,
						"'"
					}));
				}
			}
			else
			{
				if (!element.HasAttribute("name"))
				{
					throw new Exception("Attribute 'name' or 'group' missing on item in lootcontainer/lootgroup id='" + _containerId + "'");
				}
				traderItemEntry.item = new TraderInfo.TraderItem();
				string attribute2 = element.GetAttribute("name");
				traderItemEntry.item.itemValue = ItemClass.GetItem(attribute2, false);
				if (traderItemEntry.item.itemValue.IsEmpty())
				{
					throw new Exception("Item with name '" + attribute2 + "' not found!");
				}
			}
			traderItemEntry.minCount = 1;
			traderItemEntry.maxCount = 1;
			if ((traderItemEntry.item == null || ItemClass.GetForId(traderItemEntry.item.itemValue.type).CanStack()) && element.HasAttribute("count"))
			{
				StringParsers.ParseMinMaxCount(element.GetAttribute("count"), out traderItemEntry.minCount, out traderItemEntry.maxCount);
			}
			traderItemEntry.minQuality = minQualityBase;
			traderItemEntry.maxQuality = maxQualityBase;
			if (element.HasAttribute("quality"))
			{
				StringParsers.ParseMinMaxCount(element.GetAttribute("quality"), out traderItemEntry.minQuality, out traderItemEntry.maxQuality);
			}
			if (element.HasAttribute("unique_only"))
			{
				traderItemEntry.uniqueOnly = StringParsers.ParseBool(element.GetAttribute("unique_only"), 0, -1, true);
			}
			if (element.HasAttribute("mods"))
			{
				traderItemEntry.modsToInstall = element.GetAttribute("mods").Split(',', StringSplitOptions.None);
			}
			else
			{
				traderItemEntry.modsToInstall = new string[0];
			}
			if (element.HasAttribute("mod_chance"))
			{
				traderItemEntry.modChance = StringParsers.ParseFloat(element.GetAttribute("mod_chance"), 0, -1, NumberStyles.Any);
			}
			_itemList.Add(traderItemEntry);
		}
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using UnityEngine;
using XMLData.Item;

public class ItemModificationsFromXml
{
	public static IEnumerator Load(XmlFile xmlFile)
	{
		XElement root = xmlFile.XmlDoc.Root;
		if (!root.HasElements)
		{
			throw new Exception("No element <item_modifiers> found!");
		}
		ItemModificationsFromXml.ParseNode(root);
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseNode(XElement root)
	{
		foreach (XElement element in root.Elements("item_modifier"))
		{
			ItemModificationsFromXml.ParseModifier(element);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseModifier(XElement _element)
	{
		ItemClassModifier itemClassModifier = new ItemClassModifier();
		itemClassModifier.Groups = new string[]
		{
			"Mods"
		};
		if (_element.HasAttribute("installable_tags"))
		{
			itemClassModifier.InstallableTags = FastTags<TagGroup.Global>.Parse(_element.GetAttribute("installable_tags"));
		}
		if (_element.HasAttribute("blocked_tags"))
		{
			itemClassModifier.DisallowedTags = FastTags<TagGroup.Global>.Parse(_element.GetAttribute("blocked_tags"));
		}
		if (_element.HasAttribute("modifier_tags"))
		{
			itemClassModifier.ItemTags = FastTags<TagGroup.Global>.Parse(_element.GetAttribute("modifier_tags"));
		}
		if (_element.HasAttribute("type"))
		{
			itemClassModifier.Type = EnumUtils.Parse<ItemClassModifier.ModifierTypes>(_element.GetAttribute("type"), true);
		}
		ItemModificationsFromXml.parseItem(_element, itemClassModifier);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void parseItem(XElement elementItem, ItemClassModifier item)
	{
		string text = string.Empty;
		DynamicProperties dynamicProperties = new DynamicProperties();
		if (!elementItem.HasAttribute("name"))
		{
			throw new Exception("Attribute 'name' missing on item");
		}
		text = elementItem.GetAttribute("name");
		item.CosmeticInstallChance = 1f;
		if (elementItem.HasAttribute("cosmetic_install_chance"))
		{
			item.CosmeticInstallChance = StringParsers.ParseFloat(elementItem.GetAttribute("cosmetic_install_chance"), 0, -1, NumberStyles.Any);
		}
		List<IRequirement>[] array = new List<IRequirement>[3];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = new List<IRequirement>();
		}
		item.PropertyOverrides = new Dictionary<string, DynamicProperties>();
		foreach (XElement xelement in elementItem.Elements())
		{
			if (xelement.Name == XNames.property)
			{
				dynamicProperties.Add(xelement, true);
				string attribute = xelement.GetAttribute(XNames.class_);
				if (attribute.StartsWith("Action"))
				{
					int num = (int)(attribute[attribute.Length - 1] - '0');
					array[num].AddRange(RequirementBase.ParseRequirements(xelement));
				}
			}
			else if (xelement.Name == "item_property_overrides" && xelement.HasAttribute(XNames.name))
			{
				string attribute2 = xelement.GetAttribute(XNames.name);
				DynamicProperties dynamicProperties2 = new DynamicProperties();
				foreach (XElement propertyNode in xelement.Elements(XNames.property))
				{
					dynamicProperties2.Add(propertyNode, true);
				}
				if (dynamicProperties2.Values.Dict.Count > 0)
				{
					item.PropertyOverrides[attribute2] = dynamicProperties2;
				}
			}
		}
		if (dynamicProperties.Values.ContainsKey("Extends"))
		{
			string text2 = dynamicProperties.Values["Extends"];
			ItemClass itemClass = ItemClass.GetItemClass(text2, false);
			if (itemClass == null)
			{
				throw new Exception(string.Format("Extends item {0} is not specified for item {1}'", text2, text));
			}
			HashSet<string> hashSet = new HashSet<string>
			{
				Block.PropCreativeMode
			};
			if (dynamicProperties.Params1.ContainsKey("Extends"))
			{
				foreach (string text3 in dynamicProperties.Params1["Extends"].Split(',', StringSplitOptions.None))
				{
					hashSet.Add(text3.Trim());
				}
			}
			DynamicProperties dynamicProperties3 = new DynamicProperties();
			dynamicProperties3.CopyFrom(itemClass.Properties, hashSet);
			dynamicProperties3.CopyFrom(dynamicProperties, null);
			dynamicProperties = dynamicProperties3;
		}
		item.Properties = dynamicProperties;
		item.Effects = MinEffectController.ParseXml(elementItem, null, MinEffectController.SourceParentType.ItemModifierClass, item.Id);
		item.SetName(text);
		item.setLocalizedItemName(Localization.Get(text, false));
		if (dynamicProperties.Values.ContainsKey("Stacknumber"))
		{
			item.Stacknumber = new DataItem<int>(int.Parse(dynamicProperties.Values["Stacknumber"]));
		}
		else
		{
			item.Stacknumber = new DataItem<int>(500);
		}
		if (dynamicProperties.Values.ContainsKey("Canhold"))
		{
			item.SetCanHold(StringParsers.ParseBool(dynamicProperties.Values["Canhold"], 0, -1, true));
		}
		if (dynamicProperties.Values.ContainsKey("Candrop"))
		{
			item.SetCanDrop(StringParsers.ParseBool(dynamicProperties.Values["Candrop"], 0, -1, true));
		}
		if (dynamicProperties.Values.ContainsKey("Material"))
		{
			item.MadeOfMaterial = MaterialBlock.fromString(dynamicProperties.Values["Material"]);
		}
		else
		{
			item.MadeOfMaterial = MaterialBlock.fromString("Miron");
		}
		if (dynamicProperties.Values.ContainsKey("Meshfile") && item.CanHold())
		{
			item.MeshFile = dynamicProperties.Values["Meshfile"];
			DataLoader.PreloadBundle(item.MeshFile);
		}
		if (dynamicProperties.Values.ContainsKey("StickyOffset"))
		{
			StringParsers.TryParseFloat(dynamicProperties.Values["StickyOffset"], out item.StickyOffset, 0, -1, NumberStyles.Any);
		}
		if (dynamicProperties.Values.ContainsKey("ImageEffectOnActive"))
		{
			item.ImageEffectOnActive = new DataItem<string>(dynamicProperties.Values["ImageEffectOnActive"]);
		}
		if (dynamicProperties.Values.ContainsKey("Active"))
		{
			item.Active = new DataItem<bool>(false);
		}
		if (dynamicProperties.Values.ContainsKey("DropMeshfile") && item.CanHold())
		{
			item.DropMeshFile = dynamicProperties.Values["DropMeshfile"];
			DataLoader.PreloadBundle(item.DropMeshFile);
		}
		if (dynamicProperties.Values.ContainsKey("HandMeshfile") && item.CanHold())
		{
			item.HandMeshFile = dynamicProperties.Values["HandMeshfile"];
			DataLoader.PreloadBundle(item.HandMeshFile);
		}
		if (dynamicProperties.Values.ContainsKey("HoldType"))
		{
			string s = dynamicProperties.Values["HoldType"];
			int startValue = 0;
			if (!int.TryParse(s, out startValue))
			{
				throw new Exception("Cannot parse attribute hold_type for item '" + text + "'");
			}
			item.HoldType = new DataItem<int>(startValue);
		}
		if (dynamicProperties.Values.ContainsKey("RepairTools"))
		{
			string[] array3 = dynamicProperties.Values["RepairTools"].Replace(" ", "").Split(',', StringSplitOptions.None);
			DataItem<string>[] array4 = new DataItem<string>[array3.Length];
			for (int k = 0; k < array3.Length; k++)
			{
				array4[k] = new DataItem<string>(array3[k]);
			}
			item.RepairTools = new ItemData.DataItemArrayRepairTools(array4);
		}
		if (dynamicProperties.Values.ContainsKey("RepairAmount"))
		{
			int startValue2 = 0;
			int.TryParse(dynamicProperties.Values["RepairAmount"], out startValue2);
			item.RepairAmount = new DataItem<int>(startValue2);
		}
		if (dynamicProperties.Values.ContainsKey("RepairTime"))
		{
			float startValue3 = 0f;
			StringParsers.TryParseFloat(dynamicProperties.Values["RepairTime"], out startValue3, 0, -1, NumberStyles.Any);
			item.RepairTime = new DataItem<float>(startValue3);
		}
		else if (item.RepairAmount != null)
		{
			item.RepairTime = new DataItem<float>(1f);
		}
		if (dynamicProperties.Values.ContainsKey("Degradation"))
		{
			item.MaxUseTimes = new DataItem<int>(int.Parse(dynamicProperties.Values["Degradation"]));
		}
		else
		{
			item.MaxUseTimes = new DataItem<int>(0);
			item.MaxUseTimesBreaksAfter = new DataItem<bool>(false);
		}
		if (dynamicProperties.Values.ContainsKey("DegradationBreaksAfter"))
		{
			item.MaxUseTimesBreaksAfter = new DataItem<bool>(StringParsers.ParseBool(dynamicProperties.Values["DegradationBreaksAfter"], 0, -1, true));
		}
		else if (dynamicProperties.Values.ContainsKey("Degradation"))
		{
			item.MaxUseTimesBreaksAfter = new DataItem<bool>(true);
		}
		if (dynamicProperties.Values.ContainsKey("EconomicValue"))
		{
			item.EconomicValue = StringParsers.ParseFloat(dynamicProperties.Values["EconomicValue"], 0, -1, NumberStyles.Any);
		}
		if (dynamicProperties.Classes.ContainsKey("Preview"))
		{
			DynamicProperties dynamicProperties4 = dynamicProperties.Classes["Preview"];
			item.Preview = new PreviewData();
			if (dynamicProperties4.Values.ContainsKey("Zoom"))
			{
				item.Preview.Zoom = new DataItem<int>(int.Parse(dynamicProperties4.Values["Zoom"]));
			}
			if (dynamicProperties4.Values.ContainsKey("Pos"))
			{
				item.Preview.Pos = new DataItem<Vector2>(StringParsers.ParseVector2(dynamicProperties4.Values["Pos"]));
			}
			else
			{
				item.Preview.Pos = new DataItem<Vector2>(Vector2.zero);
			}
			if (dynamicProperties4.Values.ContainsKey("Rot"))
			{
				item.Preview.Rot = new DataItem<Vector3>(StringParsers.ParseVector3(dynamicProperties4.Values["Rot"], 0, -1));
			}
			else
			{
				item.Preview.Rot = new DataItem<Vector3>(Vector3.zero);
			}
		}
		for (int l = 0; l < item.Actions.Length; l++)
		{
			string text4 = ItemClass.itemActionNames[l];
			if (dynamicProperties.Classes.ContainsKey(text4))
			{
				ItemAction itemAction = null;
				if (dynamicProperties.Values.ContainsKey(text4 + ".Class"))
				{
					string text5 = dynamicProperties.Values[text4 + ".Class"];
					try
					{
						itemAction = (ItemAction)Activator.CreateInstance(ReflectionHelpers.GetTypeWithPrefix("ItemAction", text5));
						goto IL_8EE;
					}
					catch (Exception)
					{
						throw new Exception("ItemAction class '" + text5 + " could not be instantiated");
					}
					goto IL_8BC;
					IL_8EE:
					itemAction.item = item;
					itemAction.ReadFrom(dynamicProperties.Classes[text4]);
					if (array[l].Count > 0)
					{
						itemAction.ExecutionRequirements = array[l];
					}
					item.Actions[l] = itemAction;
					goto IL_92C;
				}
				IL_8BC:
				throw new Exception(string.Concat(new string[]
				{
					"No class attribute found on ",
					text4,
					" in item with '",
					text,
					"'"
				}));
			}
			IL_92C:;
		}
		item.Init();
	}
}

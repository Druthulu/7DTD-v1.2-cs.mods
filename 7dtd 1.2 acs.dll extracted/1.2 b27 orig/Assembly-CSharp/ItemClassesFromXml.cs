using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;
using XMLData.Item;

public class ItemClassesFromXml
{
	public static void CreateItemsFromBlocks()
	{
		for (int i = 1; i < Block.ItemsStartHere; i++)
		{
			if (Block.list[i] != null)
			{
				ItemClassBlock itemClassBlock = new ItemClassBlock();
				itemClassBlock.SetId(Block.list[i].blockID);
				itemClassBlock.SetName(Block.list[i].GetBlockName());
				itemClassBlock.Stacknumber = new DataItem<int>(Block.list[i].Stacknumber);
				ItemClass.list[itemClassBlock.Id] = itemClassBlock;
				itemClassBlock.Init();
			}
		}
	}

	public static IEnumerator CreateItems(XmlFile _xmlFile)
	{
		XElement root = _xmlFile.XmlDoc.Root;
		if (!root.HasElements)
		{
			throw new Exception("No element <items> found!");
		}
		Dictionary<string, bool> dictionary = new Dictionary<string, bool>();
		foreach (XElement element in from i in root.Elements("items")
		from a in i.Elements("animation")
		from h in a.Elements("hold_type")
		select h)
		{
			if (!element.HasAttribute("id"))
			{
				throw new Exception("hold_type with missing name or id!");
			}
			dictionary[element.GetAttribute("id")] = StringParsers.ParseBool(element.GetAttribute("newmodel"), 0, -1, true);
		}
		MicroStopwatch msw = new MicroStopwatch(true);
		foreach (XElement xelement in root.Elements())
		{
			if (xelement.Name == "item")
			{
				ItemClassesFromXml.parseItem(xelement);
			}
			else if (xelement.Name == "animation")
			{
				ItemClassesFromXml.parseAnimation(xelement);
			}
			else if (xelement.Name == "noise")
			{
				ItemClassesFromXml.parseNoise(xelement);
			}
			else if (xelement.Name == "smell")
			{
				ItemClassesFromXml.parseSmell(xelement);
			}
			if (msw.ElapsedMilliseconds > (long)Constants.cMaxLoadTimePerFrameMillis)
			{
				yield return null;
				msw.ResetAndRestart();
			}
		}
		IEnumerator<XElement> enumerator2 = null;
		yield break;
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void parseItem(XElement _node)
	{
		DynamicProperties dynamicProperties = new DynamicProperties();
		string attribute = _node.GetAttribute("name");
		if (attribute.Length == 0)
		{
			throw new Exception("Attribute 'name' missing on item");
		}
		List<IRequirement>[] array = new List<IRequirement>[3];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = new List<IRequirement>();
		}
		foreach (XElement xelement in _node.Elements("property"))
		{
			dynamicProperties.Add(xelement, true);
			string attribute2 = xelement.GetAttribute("class");
			if (attribute2.StartsWith("Action"))
			{
				int num = (int)(attribute2[attribute2.Length - 1] - '0');
				array[num].AddRange(RequirementBase.ParseRequirements(xelement));
			}
		}
		if (dynamicProperties.Values.ContainsKey("Extends"))
		{
			string text = dynamicProperties.Values["Extends"];
			ItemClass itemClass = ItemClass.GetItemClass(text, false);
			if (itemClass == null)
			{
				throw new Exception(string.Format("Extends item {0} is not specified for item {1}'", text, attribute));
			}
			HashSet<string> hashSet = new HashSet<string>
			{
				Block.PropCreativeMode
			};
			if (dynamicProperties.Params1.ContainsKey("Extends"))
			{
				foreach (string text2 in dynamicProperties.Params1["Extends"].Split(',', StringSplitOptions.None))
				{
					hashSet.Add(text2.Trim());
				}
			}
			DynamicProperties dynamicProperties2 = new DynamicProperties();
			dynamicProperties2.CopyFrom(itemClass.Properties, hashSet);
			dynamicProperties2.CopyFrom(dynamicProperties, null);
			dynamicProperties = dynamicProperties2;
		}
		ItemClass itemClass2;
		if (dynamicProperties.Values.ContainsKey("Class"))
		{
			string text3 = dynamicProperties.Values["Class"];
			try
			{
				itemClass2 = (ItemClass)Activator.CreateInstance(Type.GetType(text3));
				goto IL_1FB;
			}
			catch (Exception)
			{
				throw new Exception("No item class '" + text3 + " found!");
			}
		}
		itemClass2 = new ItemClass();
		IL_1FB:
		itemClass2.Properties = dynamicProperties;
		if (dynamicProperties.Params1.ContainsKey("Extends"))
		{
			string text4 = dynamicProperties.Values["Extends"];
			if (ItemClass.GetItemClass(text4, false) == null)
			{
				throw new Exception(string.Format("Extends item {0} is not specified for item {1}'", text4, attribute));
			}
		}
		itemClass2.Effects = MinEffectController.ParseXml(_node, null, MinEffectController.SourceParentType.ItemClass, itemClass2.Id);
		itemClass2.SetName(attribute);
		itemClass2.setLocalizedItemName(Localization.Get(attribute, false));
		if (dynamicProperties.Values.ContainsKey("Stacknumber"))
		{
			itemClass2.Stacknumber = new DataItem<int>(int.Parse(dynamicProperties.Values["Stacknumber"]));
		}
		else
		{
			itemClass2.Stacknumber = new DataItem<int>(500);
		}
		if (dynamicProperties.Values.ContainsKey("Canhold"))
		{
			itemClass2.SetCanHold(StringParsers.ParseBool(dynamicProperties.Values["Canhold"], 0, -1, true));
		}
		if (dynamicProperties.Values.ContainsKey("Candrop"))
		{
			itemClass2.SetCanDrop(StringParsers.ParseBool(dynamicProperties.Values["Candrop"], 0, -1, true));
		}
		if (!dynamicProperties.Values.ContainsKey("Material"))
		{
			throw new Exception("Attribute 'material' missing on item '" + attribute + "'");
		}
		itemClass2.MadeOfMaterial = MaterialBlock.fromString(dynamicProperties.Values["Material"]);
		if (itemClass2.MadeOfMaterial == null)
		{
			throw new Exception(string.Concat(new string[]
			{
				"Attribute 'material' '",
				dynamicProperties.Values["Material"],
				"' refers to not existing material in item '",
				attribute,
				"'"
			}));
		}
		if (!dynamicProperties.Values.ContainsKey("Meshfile") && itemClass2.CanHold())
		{
			throw new Exception("Attribute 'Meshfile' missing on item '" + attribute + "'");
		}
		itemClass2.MeshFile = dynamicProperties.Values["Meshfile"];
		DataLoader.PreloadBundle(itemClass2.MeshFile);
		StringParsers.TryParseFloat(dynamicProperties.Values["StickyOffset"], out itemClass2.StickyOffset, 0, -1, NumberStyles.Any);
		StringParsers.TryParseFloat(dynamicProperties.Values["StickyColliderRadius"], out itemClass2.StickyColliderRadius, 0, -1, NumberStyles.Any);
		StringParsers.TryParseSInt32(dynamicProperties.Values["StickyColliderUp"], out itemClass2.StickyColliderUp, 0, -1, NumberStyles.Integer);
		StringParsers.TryParseFloat(dynamicProperties.Values["StickyColliderLength"], out itemClass2.StickyColliderLength, 0, -1, NumberStyles.Any);
		itemClass2.StickyMaterial = dynamicProperties.Values["StickyMaterial"];
		if (dynamicProperties.Values.ContainsKey("ImageEffectOnActive"))
		{
			itemClass2.ImageEffectOnActive = new DataItem<string>(dynamicProperties.Values["ImageEffectOnActive"]);
		}
		if (dynamicProperties.Values.ContainsKey("Active"))
		{
			itemClass2.Active = new DataItem<bool>(false);
		}
		if (dynamicProperties.Values.ContainsKey(ItemClass.PropIsSticky))
		{
			itemClass2.IsSticky = StringParsers.ParseBool(dynamicProperties.Values[ItemClass.PropIsSticky], 0, -1, true);
		}
		if (dynamicProperties.Values.ContainsKey("DropMeshfile") && itemClass2.CanHold())
		{
			itemClass2.DropMeshFile = dynamicProperties.Values["DropMeshfile"];
			DataLoader.PreloadBundle(itemClass2.DropMeshFile);
		}
		if (dynamicProperties.Values.ContainsKey("HandMeshfile") && itemClass2.CanHold())
		{
			itemClass2.HandMeshFile = dynamicProperties.Values["HandMeshfile"];
			DataLoader.PreloadBundle(itemClass2.HandMeshFile);
		}
		if (dynamicProperties.Values.ContainsKey("HoldType"))
		{
			string s = dynamicProperties.Values["HoldType"];
			int startValue = 0;
			if (!int.TryParse(s, out startValue))
			{
				throw new Exception("Cannot parse attribute hold_type for item '" + attribute + "'");
			}
			itemClass2.HoldType = new DataItem<int>(startValue);
		}
		if (dynamicProperties.Values.ContainsKey("RepairTools"))
		{
			string[] array3 = dynamicProperties.Values["RepairTools"].Replace(" ", "").Split(',', StringSplitOptions.None);
			DataItem<string>[] array4 = new DataItem<string>[array3.Length];
			for (int k = 0; k < array3.Length; k++)
			{
				array4[k] = new DataItem<string>(array3[k]);
			}
			itemClass2.RepairTools = new ItemData.DataItemArrayRepairTools(array4);
		}
		if (dynamicProperties.Values.ContainsKey("RepairAmount"))
		{
			int startValue2 = 0;
			int.TryParse(dynamicProperties.Values["RepairAmount"], out startValue2);
			itemClass2.RepairAmount = new DataItem<int>(startValue2);
		}
		if (dynamicProperties.Values.ContainsKey("RepairTime"))
		{
			float startValue3 = 0f;
			StringParsers.TryParseFloat(dynamicProperties.Values["RepairTime"], out startValue3, 0, -1, NumberStyles.Any);
			itemClass2.RepairTime = new DataItem<float>(startValue3);
		}
		else if (itemClass2.RepairAmount != null)
		{
			itemClass2.RepairTime = new DataItem<float>(1f);
		}
		if (dynamicProperties.Values.ContainsKey("Degradation"))
		{
			itemClass2.MaxUseTimes = new DataItem<int>(int.Parse(dynamicProperties.Values["Degradation"]));
		}
		else
		{
			itemClass2.MaxUseTimes = new DataItem<int>(0);
			itemClass2.MaxUseTimesBreaksAfter = new DataItem<bool>(false);
		}
		if (dynamicProperties.Values.ContainsKey("DegradationBreaksAfter"))
		{
			itemClass2.MaxUseTimesBreaksAfter = new DataItem<bool>(StringParsers.ParseBool(dynamicProperties.Values["DegradationBreaksAfter"], 0, -1, true));
		}
		else if (dynamicProperties.Values.ContainsKey("Degradation"))
		{
			itemClass2.MaxUseTimesBreaksAfter = new DataItem<bool>(true);
		}
		if (dynamicProperties.Values.ContainsKey("EconomicValue"))
		{
			itemClass2.EconomicValue = StringParsers.ParseFloat(dynamicProperties.Values["EconomicValue"], 0, -1, NumberStyles.Any);
		}
		if (dynamicProperties.Classes.ContainsKey("Preview"))
		{
			DynamicProperties dynamicProperties3 = dynamicProperties.Classes["Preview"];
			itemClass2.Preview = new PreviewData();
			if (dynamicProperties3.Values.ContainsKey("Zoom"))
			{
				itemClass2.Preview.Zoom = new DataItem<int>(int.Parse(dynamicProperties3.Values["Zoom"]));
			}
			if (dynamicProperties3.Values.ContainsKey("Pos"))
			{
				itemClass2.Preview.Pos = new DataItem<Vector2>(StringParsers.ParseVector2(dynamicProperties3.Values["Pos"]));
			}
			else
			{
				itemClass2.Preview.Pos = new DataItem<Vector2>(Vector2.zero);
			}
			if (dynamicProperties3.Values.ContainsKey("Rot"))
			{
				itemClass2.Preview.Rot = new DataItem<Vector3>(StringParsers.ParseVector3(dynamicProperties3.Values["Rot"], 0, -1));
			}
			else
			{
				itemClass2.Preview.Rot = new DataItem<Vector3>(Vector3.zero);
			}
		}
		for (int l = 0; l < itemClass2.Actions.Length; l++)
		{
			string text5 = ItemClass.itemActionNames[l];
			if (dynamicProperties.Classes.ContainsKey(text5))
			{
				if (dynamicProperties.Values.ContainsKey(text5 + ".Class"))
				{
					string text6 = dynamicProperties.Values[text5 + ".Class"];
					ItemAction itemAction;
					try
					{
						itemAction = (ItemAction)Activator.CreateInstance(ReflectionHelpers.GetTypeWithPrefix("ItemAction", text6));
						goto IL_9A8;
					}
					catch (Exception)
					{
						throw new Exception("ItemAction class '" + text6 + " could not be instantiated");
					}
					goto IL_976;
					IL_9A8:
					itemAction.item = itemClass2;
					itemAction.ActionIndex = l;
					itemAction.ReadFrom(dynamicProperties.Classes[text5]);
					if (array[l].Count > 0)
					{
						itemAction.ExecutionRequirements = array[l];
					}
					itemClass2.Actions[l] = itemAction;
					goto IL_9F1;
				}
				IL_976:
				throw new Exception(string.Concat(new string[]
				{
					"No class attribute found on ",
					text5,
					" in item with '",
					attribute,
					"'"
				}));
			}
			IL_9F1:;
		}
		itemClass2.Init();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void parseAnimation(XElement _node)
	{
		foreach (XElement element in _node.Elements("hold_type"))
		{
			if (!element.HasAttribute("id"))
			{
				throw new Exception("Attribute 'id' missing in hold_type");
			}
			int num = 0;
			if (!int.TryParse(element.GetAttribute("id"), out num))
			{
				throw new Exception("Unknown hold_type id for animation");
			}
			float num2 = 0f;
			if (element.HasAttribute("ray_cast"))
			{
				num2 = StringParsers.ParseFloat(element.GetAttribute("ray_cast"), 0, -1, NumberStyles.Any);
			}
			float rayCastMoving = num2;
			if (element.HasAttribute("ray_cast_moving"))
			{
				num2 = StringParsers.ParseFloat(element.GetAttribute("ray_cast_moving"), 0, -1, NumberStyles.Any);
			}
			float num3 = Constants.cMinHolsterTime;
			if (element.HasAttribute("holster"))
			{
				num3 = Utils.FastMax(StringParsers.ParseFloat(element.GetAttribute("holster"), 0, -1, NumberStyles.Any), num3);
			}
			float num4 = Constants.cMinUnHolsterTime;
			if (element.HasAttribute("unholster"))
			{
				num4 = Utils.FastMax(StringParsers.ParseFloat(element.GetAttribute("unholster"), 0, -1, NumberStyles.Any), num4);
			}
			bool twoHanded = false;
			if (element.HasAttribute("two_handed"))
			{
				twoHanded = StringParsers.ParseBool(element.GetAttribute("two_handed"), 0, -1, true);
			}
			AnimationDelayData.AnimationDelay[num] = new AnimationDelayData.AnimationDelays(num2, rayCastMoving, num3, num4, twoHanded);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void parseNoise(XElement _node)
	{
		foreach (XElement element in _node.Elements("sound"))
		{
			if (!element.HasAttribute("name"))
			{
				throw new Exception("Attribute 'name' missing in noise/sound");
			}
			string attribute = element.GetAttribute("name");
			float volume = 0f;
			if (element.HasAttribute("volume"))
			{
				volume = StringParsers.ParseFloat(element.GetAttribute("volume"), 0, -1, NumberStyles.Any);
			}
			if (!element.HasAttribute("time"))
			{
				throw new Exception("Attribute 'time' missing in noise/sound name='" + attribute + "'");
			}
			float duration = StringParsers.ParseFloat(element.GetAttribute("time"), 0, -1, NumberStyles.Any);
			float muffledWhenCrouched = 1f;
			if (element.HasAttribute("muffled_when_crouched"))
			{
				muffledWhenCrouched = StringParsers.ParseFloat(element.GetAttribute("muffled_when_crouched"), 0, -1, NumberStyles.Any);
			}
			float heatMapStrength = 0f;
			if (element.HasAttribute("heat_map_strength"))
			{
				heatMapStrength = StringParsers.ParseFloat(element.GetAttribute("heat_map_strength"), 0, -1, NumberStyles.Any);
			}
			float num = 100f;
			if (element.HasAttribute("heat_map_time"))
			{
				num = StringParsers.ParseFloat(element.GetAttribute("heat_map_time"), 0, -1, NumberStyles.Any);
			}
			num *= 10f;
			AIDirectorData.AddNoisySound(attribute, new AIDirectorData.Noise(attribute, volume, duration, muffledWhenCrouched, heatMapStrength, (ulong)num));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void parseSmell(XElement _node)
	{
		foreach (XElement element in _node.Elements("smell"))
		{
			if (!element.HasAttribute("name"))
			{
				throw new Exception("Attribute 'name' missing in smell");
			}
			string attribute = element.GetAttribute("name");
			if (!element.HasAttribute("range"))
			{
				throw new Exception("Attribute 'range' missing in smell name='" + attribute + "'");
			}
			float range = StringParsers.ParseFloat(element.GetAttribute("range"), 0, -1, NumberStyles.Any);
			if (!element.HasAttribute("belt_range"))
			{
				throw new Exception("Attribute 'belt_range' missing in smell name='" + attribute + "'");
			}
			float beltRange = StringParsers.ParseFloat(element.GetAttribute("belt_range"), 0, -1, NumberStyles.Any);
			float heatMapStrength = 0f;
			if (element.HasAttribute("heat_map_strength"))
			{
				heatMapStrength = StringParsers.ParseFloat(element.GetAttribute("heat_map_strength"), 0, -1, NumberStyles.Any);
			}
			float num = 100f;
			if (element.HasAttribute("heat_map_time"))
			{
				num = StringParsers.ParseFloat(element.GetAttribute("heat_map_time"), 0, -1, NumberStyles.Any);
			}
			num *= 10f;
			AIDirectorData.AddSmell(attribute, new AIDirectorData.Smell(attribute, range, beltRange, heatMapStrength, (ulong)num));
		}
	}
}

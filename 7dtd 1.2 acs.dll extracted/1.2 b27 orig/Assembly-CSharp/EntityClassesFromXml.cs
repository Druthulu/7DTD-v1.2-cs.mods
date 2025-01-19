using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;

public class EntityClassesFromXml
{
	public static IEnumerator LoadEntityClasses(XmlFile _xmlFile)
	{
		MicroStopwatch msw = new MicroStopwatch(true);
		EntityClass.list.Clear();
		EntityClassesFromXml.sEntityClassElements = new Dictionary<int, XElement>();
		XElement root = _xmlFile.XmlDoc.Root;
		if (!root.HasElements)
		{
			throw new Exception("No element <entity_classes> found!");
		}
		foreach (XElement xelement in root.Elements())
		{
			xelement.Name == "LevelingTable";
			if (xelement.Name == "entity_class")
			{
				XElement xelement2 = xelement;
				EntityClass entityClass = new EntityClass();
				string attribute = xelement2.GetAttribute("name");
				if (attribute.Length == 0)
				{
					throw new Exception("Attribute 'name' missing on property in entity_class");
				}
				entityClass.entityClassName = attribute;
				int num = EntityClass.FromString(entityClass.entityClassName);
				EntityClassesFromXml.sEntityClassElements.Add(num, xelement2);
				string attribute2 = xelement2.GetAttribute("extends");
				if (attribute2.Length > 0)
				{
					int num2 = EntityClass.FromString(attribute2);
					if (!EntityClass.list.ContainsKey(num2))
					{
						throw new Exception("Did not find 'extends' entity '" + attribute2 + "'");
					}
					HashSet<string> hashSet = new HashSet<string>();
					if (xelement2.HasAttribute("ignore"))
					{
						foreach (string text in xelement2.GetAttribute("ignore").Split(',', StringSplitOptions.None))
						{
							hashSet.Add(text.Trim());
						}
					}
					hashSet.Add("HideInSpawnMenu");
					entityClass.CopyFrom(EntityClass.list[num2], hashSet);
					entityClass.Effects = MinEffectController.ParseXml(xelement2, EntityClassesFromXml.sEntityClassElements[num2], MinEffectController.SourceParentType.EntityClass, num);
				}
				else
				{
					entityClass.Effects = MinEffectController.ParseXml(xelement2, null, MinEffectController.SourceParentType.EntityClass, num);
				}
				foreach (XElement xelement3 in xelement2.Elements())
				{
					if (xelement3.Name == "property")
					{
						entityClass.Properties.Add(xelement3, true);
					}
					if (xelement3.Name == "drop")
					{
						XElement element = xelement3;
						int minCount = 1;
						int maxCount = 1;
						if (element.HasAttribute("count"))
						{
							StringParsers.ParseMinMaxCount(element.GetAttribute("count"), out minCount, out maxCount);
						}
						float prob = 1f;
						if (element.HasAttribute("prob"))
						{
							prob = StringParsers.ParseFloat(element.GetAttribute("prob"), 0, -1, NumberStyles.Any);
						}
						string attribute3 = element.GetAttribute("name");
						EnumDropEvent eEvent = EnumDropEvent.Destroy;
						if (element.HasAttribute("event"))
						{
							eEvent = EnumUtils.Parse<EnumDropEvent>(element.GetAttribute("event"), false);
						}
						float stickChance = 0f;
						if (element.HasAttribute("stick_chance"))
						{
							stickChance = StringParsers.ParseFloat(element.GetAttribute("stick_chance"), 0, -1, NumberStyles.Any);
						}
						string toolCategory = null;
						if (element.HasAttribute("tool_category"))
						{
							toolCategory = element.GetAttribute("tool_category");
						}
						string tag = "";
						if (element.HasAttribute("tag"))
						{
							tag = element.GetAttribute("tag");
						}
						entityClass.AddDroppedId(eEvent, attribute3, minCount, maxCount, prob, stickChance, toolCategory, tag);
					}
				}
				EntityClass.list[num] = entityClass;
				entityClass.Init();
			}
			if (msw.ElapsedMilliseconds > (long)Constants.cMaxLoadTimePerFrameMillis)
			{
				yield return null;
				msw.ResetAndRestart();
			}
		}
		IEnumerator<XElement> enumerator = null;
		EntityClassesFromXml.sEntityClassElements.Clear();
		EntityClassesFromXml.sEntityClassElements = null;
		yield break;
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<int, XElement> sEntityClassElements;
}

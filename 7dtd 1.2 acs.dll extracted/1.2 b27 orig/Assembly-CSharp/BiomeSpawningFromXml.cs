using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;

public class BiomeSpawningFromXml
{
	public static IEnumerator Load(XmlFile _xmlFile)
	{
		XElement root = _xmlFile.XmlDoc.Root;
		if (!root.HasElements)
		{
			throw new Exception("No element <spawning> found!");
		}
		using (IEnumerator<XElement> enumerator = root.Elements("biome").GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				XElement xelement = enumerator.Current;
				string attribute = xelement.GetAttribute("name");
				if (attribute.Length == 0)
				{
					throw new Exception("Attribute 'name' missing on biome tag");
				}
				BiomeSpawnEntityGroupList biomeSpawnEntityGroupList = new BiomeSpawnEntityGroupList();
				BiomeSpawningClass.list[attribute] = biomeSpawnEntityGroupList;
				foreach (XElement element in xelement.Elements("spawn"))
				{
					string attribute2 = element.GetAttribute("id");
					int hashCode = attribute2.GetHashCode();
					if (biomeSpawnEntityGroupList.Find(hashCode) != null)
					{
						throw new Exception(string.Concat(new string[]
						{
							"Duplicate id hash '",
							attribute2,
							"' in biome '",
							attribute,
							"'"
						}));
					}
					int maxCount = 1;
					if (element.HasAttribute("maxcount"))
					{
						maxCount = int.Parse(element.GetAttribute("maxcount"));
					}
					int respawndelay = 0;
					if (element.HasAttribute("respawndelay"))
					{
						respawndelay = (int)(StringParsers.ParseFloat(element.GetAttribute("respawndelay"), 0, -1, NumberStyles.Any) * 24000f);
					}
					EDaytime daytime = EDaytime.Any;
					if (element.HasAttribute("time"))
					{
						daytime = EnumUtils.Parse<EDaytime>(element.GetAttribute("time"), false);
					}
					BiomeSpawnEntityGroupData biomeSpawnEntityGroupData = new BiomeSpawnEntityGroupData(hashCode, maxCount, respawndelay, daytime);
					string attribute3 = element.GetAttribute("tags");
					if (attribute3.Length > 0)
					{
						biomeSpawnEntityGroupData.POITags = FastTags<TagGroup.Poi>.Parse(attribute3);
					}
					attribute3 = element.GetAttribute("notags");
					if (attribute3.Length > 0)
					{
						biomeSpawnEntityGroupData.noPOITags = FastTags<TagGroup.Poi>.Parse(attribute3);
					}
					string attribute4 = element.GetAttribute("entitygroup");
					if (attribute4.Length == 0)
					{
						throw new Exception("Missing attribute 'entitygroup' in entitygroup of biome '" + attribute + "'");
					}
					if (!EntityGroups.list.ContainsKey(attribute4))
					{
						throw new Exception("Entity group '" + attribute4 + "' not existing!");
					}
					biomeSpawnEntityGroupData.entityGroupName = attribute4;
					biomeSpawnEntityGroupList.list.Add(biomeSpawnEntityGroupData);
				}
			}
			yield break;
		}
		yield break;
	}
}

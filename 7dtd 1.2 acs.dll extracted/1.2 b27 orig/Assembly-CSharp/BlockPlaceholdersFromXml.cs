using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;

public class BlockPlaceholdersFromXml
{
	public static IEnumerator Load(XmlFile _xmlFile)
	{
		BlockPlaceholderMap.InitStatic();
		XElement root = _xmlFile.XmlDoc.Root;
		if (!root.HasElements)
		{
			throw new Exception("No element <placeholder> found!");
		}
		using (IEnumerator<XElement> enumerator = root.Elements().GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				XElement xelement = enumerator.Current;
				if (!xelement.HasAttribute("name"))
				{
					throw new Exception("Attribute 'name' missing on placeholder");
				}
				string attribute = xelement.GetAttribute("name");
				BlockValue placeholderBlockValue = ItemClass.GetItem(attribute, false).ToBlockValue();
				foreach (XElement element in xelement.Elements("block"))
				{
					if (!element.HasAttribute("name"))
					{
						throw new Exception("Attribute 'name' missing on placeholder block of '" + attribute + "'");
					}
					string attribute2 = element.GetAttribute("name");
					BlockValue targetValue = ItemClass.GetItem(attribute2, false).ToBlockValue();
					float num = 1f;
					if (element.HasAttribute("prob") && !StringParsers.TryParseFloat(element.GetAttribute("prob"), out num, 0, -1, NumberStyles.Any))
					{
						throw new Exception(string.Concat(new string[]
						{
							"Parsing error prob '",
							element.GetAttribute("prob"),
							"' in '",
							attribute2,
							"'"
						}));
					}
					if (num <= 0f)
					{
						throw new Exception(string.Concat(new string[]
						{
							"Parsing error prob '",
							element.GetAttribute("prob"),
							"' in '",
							attribute2,
							"': can not be negative!"
						}));
					}
					string biome = null;
					if (element.HasAttribute("biome"))
					{
						biome = element.GetAttribute("biome");
					}
					FastTags<TagGroup.Global> questTags = FastTags<TagGroup.Global>.none;
					if (element.HasAttribute("questtag"))
					{
						questTags = FastTags<TagGroup.Global>.Parse(element.GetAttribute("questtag"));
					}
					bool randomRotation = false;
					if (element.HasAttribute("randomrotation"))
					{
						randomRotation = StringParsers.ParseBool(element.GetAttribute("randomrotation"), 0, -1, true);
					}
					if (!questTags.IsEmpty)
					{
						BlockPlaceholderMap.Instance.AddQuestResetPlaceholder(placeholderBlockValue, targetValue, num, biome, randomRotation, questTags);
					}
					else
					{
						BlockPlaceholderMap.Instance.AddPlaceholder(placeholderBlockValue, targetValue, num, biome, randomRotation);
					}
				}
			}
			yield break;
		}
		yield break;
	}
}

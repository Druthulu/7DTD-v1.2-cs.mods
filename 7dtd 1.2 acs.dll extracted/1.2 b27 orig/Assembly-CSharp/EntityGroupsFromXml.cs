using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using System.Xml.Linq;

public class EntityGroupsFromXml
{
	public static IEnumerator LoadEntityGroups(XmlFile _xmlFile)
	{
		XElement root = _xmlFile.XmlDoc.Root;
		if (!root.HasElements)
		{
			throw new Exception("No element <entitygroups> found!");
		}
		int num = 0;
		using (IEnumerator<XElement> enumerator = root.Elements("entitygroup").GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				XElement xelement = enumerator.Current;
				string attribute = xelement.GetAttribute("name");
				if (attribute.Length == 0)
				{
					throw new Exception("Attribute 'name' missing on entitygroup tag");
				}
				List<SEntityClassAndProb> list = new List<SEntityClassAndProb>();
				EntityGroups.list[attribute] = list;
				if (EntityGroups.DefaultGroupName == null)
				{
					EntityGroups.DefaultGroupName = attribute;
				}
				float num2 = 0f;
				foreach (XNode xnode in xelement.Nodes())
				{
					if (xnode.NodeType == XmlNodeType.Text)
					{
						string value = ((XText)xnode).Value;
						int num3 = 0;
						int i = value.IndexOf('\n', num3);
						if (i < 0)
						{
							i = value.Length;
						}
						while (i >= 0)
						{
							string text = value.Substring(num3, i - num3);
							num3 = i + 1;
							if (num3 >= value.Length)
							{
								i = -1;
							}
							else
							{
								i = value.IndexOf('\n', num3);
								if (i < 0)
								{
									i = value.Length;
								}
							}
							string text2 = text;
							float num4 = 1f;
							int num5 = text.IndexOf(',');
							if (num5 >= 0)
							{
								text2 = text.Substring(0, num5);
								num4 = StringParsers.ParseFloat(text, num5 + 1, -1, NumberStyles.Any);
							}
							text2 = text2.Trim();
							if (text2.Length > 0)
							{
								int num6 = 0;
								if (text2 != "none")
								{
									num6 = EntityClass.FromString(text2);
									if (!EntityClass.list.ContainsKey(num6))
									{
										throw new Exception("Entity with name '" + text2 + "' not found");
									}
								}
								list.Add(new SEntityClassAndProb
								{
									entityClassId = num6,
									prob = num4
								});
								num++;
								num2 += num4;
							}
						}
					}
					if (xnode.NodeType == XmlNodeType.Element && ((XElement)xnode).Name == "entity")
					{
						XElement element = (XElement)xnode;
						SEntityClassAndProb item = default(SEntityClassAndProb);
						string attribute2 = element.GetAttribute("name");
						if (attribute2.Length == 0)
						{
							throw new Exception("Attribute 'name' missing on entity in group '" + attribute + "'");
						}
						int num7 = 0;
						if (attribute2 != "none")
						{
							num7 = EntityClass.FromString(attribute2);
							if (!EntityClass.list.ContainsKey(num7))
							{
								throw new Exception("Entity with name '" + attribute2 + "' not found");
							}
						}
						item.entityClassId = num7;
						float num8 = 1f;
						string attribute3 = element.GetAttribute("prob");
						if (attribute3.Length > 0)
						{
							num8 = StringParsers.ParseFloat(attribute3, 0, -1, NumberStyles.Any);
						}
						item.prob = num8;
						list.Add(item);
						num++;
						num2 += num8;
					}
				}
				if (num2 > 0f)
				{
					EntityGroups.Normalize(attribute, num2);
				}
				if (list.Count == 0)
				{
					throw new Exception("Empty entity groups not allowed! Group name: " + attribute);
				}
			}
			yield break;
		}
		yield break;
	}
}

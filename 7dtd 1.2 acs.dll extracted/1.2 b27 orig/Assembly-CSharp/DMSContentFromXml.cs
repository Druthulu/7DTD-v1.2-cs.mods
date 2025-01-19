using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using DynamicMusic;
using MusicUtils.Enums;

public class DMSContentFromXml
{
	public static IEnumerator Load(XmlFile _xmlFile)
	{
		XElement root = _xmlFile.XmlDoc.Root;
		if (!root.HasElements)
		{
			throw new Exception("No element <root> found!");
		}
		Content.SamplesFor.Clear();
		Content.SourcePathFor.Clear();
		using (IEnumerator<XElement> enumerator = root.Elements().GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				XElement xelement = enumerator.Current;
				if (xelement.Name == "contents")
				{
					foreach (XElement xelement2 in xelement.Elements())
					{
						if (xelement2.Name == "section" || xelement2.Name == "content")
						{
							SectionType sectionType = EnumUtils.Parse<SectionType>(xelement2.GetAttribute("name"), false);
							if (xelement2.HasAttribute("source"))
							{
								Content.SourcePathFor.Add(sectionType, xelement2.GetAttribute("source"));
							}
							if (xelement2.Name == "section")
							{
								if (xelement2.HasAttribute("samples"))
								{
									Content.SamplesFor.Add(sectionType, int.Parse(xelement2.GetAttribute("samples")));
								}
								foreach (XElement element in xelement2.Elements("layer"))
								{
									int num = int.Parse(element.GetAttribute("num"));
									string attribute = element.GetAttribute("contentType");
									string attribute2 = element.GetAttribute("clipAdapterType");
									LayerType layer = EnumUtils.Parse<LayerType>(element.GetAttribute("name"), false);
									for (int i = 0; i < num; i++)
									{
										LayeredContent layeredContent = Content.CreateWrapper(attribute) as LayeredContent;
										if (layeredContent != null)
										{
											layeredContent.SetData(attribute2, i, sectionType, layer, element.HasAttribute("loopOnly"));
										}
									}
								}
							}
							if (xelement2.Name == "content")
							{
								Content.CreateWrapper(xelement2.GetAttribute("type")).ParseFromXml(xelement2);
							}
						}
					}
				}
				if (xelement.Name == "configurations")
				{
					foreach (XElement xelement3 in xelement.Elements("configuration"))
					{
						if (xelement3.HasAttribute("type"))
						{
							AbstractConfiguration.CreateWrapper(xelement3.GetAttribute("type")).ParseFromXml(xelement3);
						}
					}
				}
			}
			yield break;
		}
		yield break;
	}
}

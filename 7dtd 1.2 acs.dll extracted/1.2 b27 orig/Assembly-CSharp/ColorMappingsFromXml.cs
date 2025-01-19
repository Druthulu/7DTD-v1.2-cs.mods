using System;
using System.Collections;
using System.Xml.Linq;
using UnityEngine;
using XMLData;

public class ColorMappingsFromXml : MonoBehaviour
{
	public static IEnumerator Load(XmlFile _xmlFile)
	{
		XElement root = _xmlFile.XmlDoc.Root;
		if (!root.HasElements)
		{
			throw new Exception("No element <root> found!");
		}
		ColorMappingData.Instance.ColorFromID.Clear();
		ColorMappingData.Instance.IDFromName.Clear();
		ColorMappingData.Instance.NameFromID.Clear();
		foreach (XElement element in root.Elements("color"))
		{
			int num = int.Parse(element.GetAttribute("id"));
			string attribute = element.GetAttribute("name");
			Color value;
			if (ColorUtility.TryParseHtmlString(element.GetAttribute("value"), out value))
			{
				ColorMappingData.Instance.ColorFromID.Add(num, value);
				ColorMappingData.Instance.IDFromName.Add(attribute, num);
				ColorMappingData.Instance.NameFromID.Add(num, attribute);
			}
			else
			{
				Log.Warning(string.Format("No color value for {0} and {1}", num, attribute));
			}
		}
		yield return null;
		yield break;
	}
}

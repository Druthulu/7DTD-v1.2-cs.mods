using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

public class QualityInfoFromXml : MonoBehaviour
{
	public static IEnumerator CreateQualityInfo(XmlFile _xmlFile)
	{
		XElement root = _xmlFile.XmlDoc.Root;
		if (!root.HasElements)
		{
			throw new Exception("No element <quality> found!");
		}
		using (IEnumerator<XElement> enumerator = root.Elements("quality").GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				XElement element = enumerator.Current;
				int num = -1;
				if (element.HasAttribute("key"))
				{
					num = int.Parse(element.GetAttribute("key"));
				}
				string hexColor = "#FFFFFF";
				if (element.HasAttribute("color"))
				{
					hexColor = element.GetAttribute("color");
				}
				if (num > -1)
				{
					QualityInfo.Add(num, hexColor);
				}
			}
			yield break;
		}
		yield break;
	}
}

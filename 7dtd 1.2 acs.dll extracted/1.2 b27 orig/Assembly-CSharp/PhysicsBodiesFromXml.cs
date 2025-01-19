﻿using System;
using System.Collections;
using System.Xml;
using System.Xml.Linq;

public class PhysicsBodiesFromXml
{
	public static IEnumerator Load(XmlFile xmlFile)
	{
		try
		{
			XElement root = xmlFile.XmlDoc.Root;
			if (!root.HasElements)
			{
				yield break;
			}
			foreach (XElement e in root.Elements("body"))
			{
				PhysicsBodyLayout.Read(e);
			}
			yield break;
		}
		catch (Exception)
		{
			PhysicsBodyLayout.Reset();
			throw;
		}
		yield break;
	}

	public static void Save(string path)
	{
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.CreateXmlDeclaration();
		XmlElement elem = xmlDocument.AddXmlElement("bodies");
		PhysicsBodyLayout[] bodyLayouts = PhysicsBodyLayout.BodyLayouts;
		for (int i = 0; i < bodyLayouts.Length; i++)
		{
			bodyLayouts[i].Write(elem);
		}
		xmlDocument.SdSave(path);
	}
}

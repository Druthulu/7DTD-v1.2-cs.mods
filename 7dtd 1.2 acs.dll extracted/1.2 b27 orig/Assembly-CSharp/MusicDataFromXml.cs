using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using DynamicMusic.Legacy.ObjectModel;
using MusicUtils.Enums;

public class MusicDataFromXml
{
	public static IEnumerator Load(XmlFile _xmlFile)
	{
		XElement root = _xmlFile.XmlDoc.Root;
		if (!root.HasElements)
		{
			throw new Exception("No element <root> found!");
		}
		string str = "@:Sounds/Music/";
		using (IEnumerator<XElement> enumerator = root.Elements().GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				XElement xelement = enumerator.Current;
				if (xelement.Name == "property")
				{
					str = xelement.GetAttribute("value");
				}
				if (xelement.Name == "music_groups")
				{
					foreach (XElement xelement2 in xelement.Elements("group"))
					{
						int.Parse(xelement2.GetAttribute("id"));
						int sampleRate = int.Parse(xelement2.GetAttribute("sample-rate"));
						byte hbLength = byte.Parse(xelement2.GetAttribute("hyperbar-length"));
						MusicGroup musicGroup = new MusicGroup(sampleRate, hbLength);
						foreach (XElement xelement3 in xelement2.Elements("configurations"))
						{
							if (xelement3.Name == "configurations")
							{
								foreach (XElement xelement4 in xelement3.Elements("configuration"))
								{
									musicGroup.ConfigIDs.Add(int.Parse(xelement4.Attribute("id").Value));
								}
							}
							if (xelement3.Name == "threat_level")
							{
								ThreatLevelLegacyType key = EnumUtils.Parse<ThreatLevelLegacyType>(xelement3.GetAttribute("name"), false);
								double tempo = StringParsers.ParseDouble(xelement3.GetAttribute("tempo"), 0, -1, NumberStyles.Any);
								double sigHi = StringParsers.ParseDouble(xelement3.GetAttribute("sig-hi"), 0, -1, NumberStyles.Any);
								double sigLo = StringParsers.ParseDouble(xelement3.GetAttribute("sig-lo"), 0, -1, NumberStyles.Any);
								ThreatLevel threatLevel;
								musicGroup.Add(key, threatLevel = new ThreatLevel(tempo, sigHi, sigLo));
								foreach (XElement xelement5 in xelement3.Elements("layer"))
								{
									LayerType key2 = EnumUtils.Parse<LayerType>(xelement5.GetAttribute("name"), false);
									Layer layer;
									threatLevel.Add(key2, layer = new Layer());
									foreach (XElement xelement6 in xelement5.Elements("instrument_ID"))
									{
										int key3 = int.Parse(xelement6.GetAttribute("id"));
										InstrumentID instrumentID;
										layer.Add(key3, instrumentID = new InstrumentID());
										instrumentID.Name = xelement6.GetAttribute("name");
										instrumentID.SourceName = xelement6.GetAttribute("AudioSource");
										if (!StringParsers.TryParseFloat(xelement6.GetAttribute("Volume"), out instrumentID.Volume, 0, -1, NumberStyles.Any))
										{
											instrumentID.Volume = 1f;
										}
										foreach (XElement element in xelement6.Elements("placement"))
										{
											PlacementType key4 = EnumUtils.Parse<PlacementType>(element.GetAttribute("value"), false);
											string attribute = element.GetAttribute("location");
											instrumentID.Add(key4, str + attribute);
										}
									}
									layer.PopulateQueue();
								}
							}
						}
						MusicGroup.AllGroups.Add(musicGroup);
					}
				}
				if (xelement.Name == "configurations")
				{
					foreach (XElement xelement7 in xelement.Elements("configuration"))
					{
						int key5 = int.Parse(xelement7.GetAttribute("id"));
						ConfigSet configSet = new ConfigSet();
						foreach (XElement xelement8 in xelement7.Elements("threat_level"))
						{
							ThreatLevelLegacyType key6 = EnumUtils.Parse<ThreatLevelLegacyType>(xelement8.GetAttribute("name"), false);
							ThreatLevelConfig threatLevelConfig;
							configSet.Add(key6, threatLevelConfig = new ThreatLevelConfig());
							foreach (XElement element2 in xelement8.Elements("layer"))
							{
								LayerType key7 = EnumUtils.Parse<LayerType>(element2.GetAttribute("name"), false);
								LayerConfig layerConfig;
								threatLevelConfig.Add(key7, layerConfig = new LayerConfig());
								string[] array = element2.GetAttribute("values").Split(',', StringSplitOptions.None);
								for (int i = 0; i < array.Length; i++)
								{
									layerConfig.Add(byte.Parse(array[i]), (i != 0 && i != array.Length - 1) ? PlacementType.Loop : ((i == 0) ? PlacementType.Begin : PlacementType.End));
								}
							}
						}
						ConfigSet.AllConfigSets.Add(key5, configSet);
					}
				}
			}
			yield break;
		}
		yield break;
	}
}

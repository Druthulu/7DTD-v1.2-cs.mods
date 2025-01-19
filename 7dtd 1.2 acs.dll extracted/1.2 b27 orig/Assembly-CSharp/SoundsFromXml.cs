using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using Audio;
using UniLinq;

public class SoundsFromXml
{
	public static IEnumerator CreateSounds(XmlFile xmlFile)
	{
		XElement root = xmlFile.XmlDoc.Root;
		if (!root.HasElements)
		{
			throw new Exception("No element <mix> found!");
		}
		Manager.Reset();
		SoundsFromXml.ParseNode(null, root);
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseNode(string master, XElement root)
	{
		foreach (XElement node in root.Elements("SoundDataNode"))
		{
			SoundsFromXml.Parse(node);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void Parse(XElement node)
	{
		string value = node.Attributes().First<XAttribute>().Value;
		XmlData xmlData = new XmlData();
		xmlData.soundGroupName = value;
		xmlData.playImmediate = false;
		string text = null;
		foreach (XElement xelement in node.Elements())
		{
			string localName = xelement.Name.LocalName;
			if (localName.EqualsCaseInsensitive("audiosource"))
			{
				text = xelement.Attributes().First<XAttribute>().Value;
			}
			else if (localName.EqualsCaseInsensitive("noise"))
			{
				XElement element = xelement;
				string attribute = element.GetAttribute("noise");
				xmlData.noiseData.volume = (string.IsNullOrEmpty(attribute) ? 0f : StringParsers.ParseFloat(attribute, 0, -1, NumberStyles.Any));
				attribute = element.GetAttribute("time");
				xmlData.noiseData.time = (string.IsNullOrEmpty(attribute) ? 0f : StringParsers.ParseFloat(attribute, 0, -1, NumberStyles.Any));
				attribute = element.GetAttribute("heat_map_strength");
				xmlData.noiseData.heatMapStrength = (string.IsNullOrEmpty(attribute) ? 0f : StringParsers.ParseFloat(attribute, 0, -1, NumberStyles.Any));
				attribute = element.GetAttribute("heat_map_time");
				xmlData.noiseData.heatMapTime = (string.IsNullOrEmpty(attribute) ? 100UL : ulong.Parse(attribute));
				attribute = element.GetAttribute("muffled_when_crouched");
				xmlData.noiseData.crouchMuffle = (string.IsNullOrEmpty(attribute) ? 1f : StringParsers.ParseFloat(attribute, 0, -1, NumberStyles.Any));
			}
			else if (localName.EqualsCaseInsensitive("audioclip"))
			{
				ClipSourceMap clipSourceMap = new ClipSourceMap();
				XElement element2 = xelement;
				clipSourceMap.clipName = element2.GetAttribute("ClipName");
				clipSourceMap.audioSourceName = element2.GetAttribute("AudioSourceName");
				if (element2.HasAttribute("Loop"))
				{
					clipSourceMap.forceLoop = StringParsers.ParseBool(element2.GetAttribute("Loop"), 0, -1, true);
				}
				clipSourceMap.clipName_distant = element2.GetAttribute("DistantClip");
				clipSourceMap.audioSourceName_distant = element2.GetAttribute("DistantSource");
				if (string.IsNullOrEmpty(clipSourceMap.audioSourceName))
				{
					clipSourceMap.audioSourceName = text;
				}
				bool flag;
				StringParsers.TryParseBool(element2.GetAttribute("AltSound"), out flag, 0, -1, true);
				if (flag)
				{
					xmlData.AddAltClipSourceMap(clipSourceMap);
				}
				else
				{
					xmlData.audioClipMap.Add(clipSourceMap);
				}
				if (text == null)
				{
					text = clipSourceMap.audioSourceName;
				}
				if (string.IsNullOrEmpty(clipSourceMap.audioSourceName))
				{
					Log.Error("ParseSoundDataNode() - missing audio source for " + clipSourceMap.clipName + ".");
				}
				clipSourceMap.subtitleID = element2.GetAttribute("Subtitle");
				clipSourceMap.hasSubtitle = !string.IsNullOrEmpty(clipSourceMap.subtitleID);
				if (element2.HasAttribute("profanity"))
				{
					clipSourceMap.profanity = StringParsers.ParseBool(element2.GetAttribute("profanity"), 0, -1, true);
				}
				if (clipSourceMap.profanity)
				{
					xmlData.hasProfanity = true;
				}
				DataLoader.PreloadBundle(clipSourceMap.audioSourceName);
				DataLoader.PreloadBundle(clipSourceMap.clipName);
				DataLoader.PreloadBundle(clipSourceMap.audioSourceName_distant);
				DataLoader.PreloadBundle(clipSourceMap.clipName_distant);
			}
			else if (localName.EqualsCaseInsensitive("localcrouchvolumescale"))
			{
				xmlData.localCrouchVolumeScale = StringParsers.ParseFloat(xelement.Attributes().First<XAttribute>().Value, 0, -1, NumberStyles.Any);
			}
			else if (localName.EqualsCaseInsensitive("crouchnoisescale"))
			{
				xmlData.crouchNoiseScale = StringParsers.ParseFloat(xelement.Attributes().First<XAttribute>().Value, 0, -1, NumberStyles.Any);
			}
			else if (localName.EqualsCaseInsensitive("noisescale"))
			{
				xmlData.noiseScale = StringParsers.ParseFloat(xelement.Attributes().First<XAttribute>().Value, 0, -1, NumberStyles.Any);
			}
			else if (localName.EqualsCaseInsensitive("maxvoices"))
			{
				xmlData.maxVoices = int.Parse(xelement.Attributes().First<XAttribute>().Value);
			}
			else if (localName.EqualsCaseInsensitive("maxVoicesPerEntity"))
			{
				xmlData.maxVoicesPerEntity = int.Parse(xelement.Attributes().First<XAttribute>().Value);
			}
			else if (localName.EqualsCaseInsensitive("maxrepeatrate"))
			{
				xmlData.maxRepeatRate = StringParsers.ParseFloat(xelement.Attributes().First<XAttribute>().Value, 0, -1, NumberStyles.Any);
			}
			else if (localName.EqualsCaseInsensitive("ignoredistancecheck"))
			{
				Manager.AddSoundToIgnoreDistanceCheckList(value);
			}
			else if (localName.EqualsCaseInsensitive("immediate"))
			{
				xmlData.playImmediate = true;
			}
			else if (localName.EqualsCaseInsensitive("sequence"))
			{
				xmlData.sequence = true;
			}
			else if (localName.EqualsCaseInsensitive("runningvolumescale"))
			{
				xmlData.runningVolumeScale = StringParsers.ParseFloat(xelement.Attributes().First<XAttribute>().Value, 0, -1, NumberStyles.Any);
			}
			else if (localName.EqualsCaseInsensitive("lowestpitch"))
			{
				xmlData.lowestPitch = StringParsers.ParseFloat(xelement.Attributes().First<XAttribute>().Value, 0, -1, NumberStyles.Any);
			}
			else if (localName.EqualsCaseInsensitive("highestpitch"))
			{
				xmlData.highestPitch = StringParsers.ParseFloat(xelement.Attributes().First<XAttribute>().Value, 0, -1, NumberStyles.Any);
			}
			else if (localName.EqualsCaseInsensitive("distantfadestart"))
			{
				xmlData.distantFadeStart = StringParsers.ParseFloat(xelement.Attributes().First<XAttribute>().Value, 0, -1, NumberStyles.Any);
			}
			else if (localName.EqualsCaseInsensitive("distantfadeend"))
			{
				xmlData.distantFadeEnd = StringParsers.ParseFloat(xelement.Attributes().First<XAttribute>().Value, 0, -1, NumberStyles.Any);
			}
			else if (localName.EqualsCaseInsensitive("channel"))
			{
				if (xelement.Attributes().First<XAttribute>().Value.EqualsCaseInsensitive("mouth"))
				{
					xmlData.channel = XmlData.Channel.Mouth;
				}
				else
				{
					xmlData.channel = XmlData.Channel.Environment;
				}
			}
			else if (localName.EqualsCaseInsensitive("priority"))
			{
				xmlData.priority = int.Parse(xelement.Attributes().First<XAttribute>().Value);
			}
			else if (localName.EqualsCaseInsensitive("vibratecontroller"))
			{
				xmlData.vibratesController = StringParsers.ParseBool(xelement.Attributes().First<XAttribute>().Value, 0, -1, true);
			}
			else if (localName.EqualsCaseInsensitive("vibrationstrengthmultiply"))
			{
				xmlData.vibrationStrengthMultiplier = StringParsers.ParseFloat(xelement.Attributes().First<XAttribute>().Value, 0, -1, NumberStyles.Any);
			}
		}
		Manager.AddAudioData(xmlData);
	}

	public static IEnumerator LoadSubtitleXML(XmlFile xmlFile)
	{
		XElement root = xmlFile.XmlDoc.Root;
		if (!root.HasElements)
		{
			throw new Exception("No element <mix> found!");
		}
		SoundsFromXml.ParseSubtitleNode(null, root);
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseSubtitleNode(string master, XElement root)
	{
		List<SubtitleData> list = new List<SubtitleData>();
		List<SubtitleSpeakerColor> list2 = new List<SubtitleSpeakerColor>();
		foreach (XElement xelement in root.Elements())
		{
			if (xelement.Name == "Subtitle")
			{
				SubtitleData subtitleData = new SubtitleData();
				xelement.Elements();
				subtitleData.name = xelement.Attributes().First<XAttribute>().Value;
				subtitleData.contentLocId = xelement.GetAttribute("contentLocId");
				subtitleData.speakerColorId = xelement.GetAttribute("speakerColor");
				subtitleData.speakerLocId = xelement.GetAttribute("speakerLocId");
				list.Add(subtitleData);
			}
			else if (xelement.Name == "SpeakerColors")
			{
				foreach (XElement xelement2 in xelement.Elements())
				{
					list2.Add(new SubtitleSpeakerColor
					{
						name = xelement2.Attributes().First<XAttribute>().Value,
						color = xelement2.Attributes().Last<XAttribute>().Value
					});
				}
			}
		}
		Manager.AddSubtitleData(list, list2);
	}
}

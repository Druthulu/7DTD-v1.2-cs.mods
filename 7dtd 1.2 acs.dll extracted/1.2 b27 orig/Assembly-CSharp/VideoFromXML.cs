using System;
using System.Collections;
using System.Xml.Linq;

public class VideoFromXML
{
	public static IEnumerator CreateVideos(XmlFile xmlFile)
	{
		XElement root = xmlFile.XmlDoc.Root;
		bool hasElements = root.HasElements;
		VideoManager.Init();
		VideoFromXML.Parse(root);
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void Parse(XElement root)
	{
		foreach (XElement xelement in root.Elements("Video"))
		{
			VideoData videoData = new VideoData();
			videoData.name = xelement.Attribute("name").Value;
			videoData.url = xelement.Attribute("path").Value;
			videoData.defaultSubtitleDuration = float.Parse(xelement.Attribute("defaultSubtitleDuration").Value);
			foreach (XElement element in xelement.Elements("Subtitle"))
			{
				VideoSubtitle item = default(VideoSubtitle);
				item.timestamp = double.Parse(element.GetAttribute("timestamp"));
				item.subtitleId = element.GetAttribute("id");
				if (element.HasAttribute("duration"))
				{
					item.duration = float.Parse(element.GetAttribute("duration"));
				}
				else
				{
					item.duration = videoData.defaultSubtitleDuration;
				}
				videoData.subtitles.Add(item);
			}
			VideoManager.AddVideo(videoData);
		}
	}
}

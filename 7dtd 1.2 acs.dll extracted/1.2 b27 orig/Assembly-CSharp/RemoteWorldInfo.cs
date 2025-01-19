using System;
using System.Xml;
using System.Xml.Linq;
using UnityEngine;

public struct RemoteWorldInfo
{
	public RemoteWorldInfo(string gameName, string worldName, VersionInformation gameVersion, long saveSize)
	{
		this.gameName = gameName;
		this.worldName = worldName;
		this.gameVersion = gameVersion;
		this.saveSize = saveSize;
	}

	public static bool TryRead(string filePath, out RemoteWorldInfo remoteWorldInfo)
	{
		if (!SdFile.Exists(filePath))
		{
			remoteWorldInfo = default(RemoteWorldInfo);
			return false;
		}
		bool result;
		try
		{
			XElement root = SdXDocument.Load(filePath).Root;
			string text;
			string text2;
			string serializedVersionInformation;
			VersionInformation versionInformation;
			string s;
			long num;
			if (root == null)
			{
				Debug.LogError("Failed to read RemoteWorldInfo at path \"" + filePath + "\". Could not find root node.");
				remoteWorldInfo = default(RemoteWorldInfo);
				result = false;
			}
			else if (!root.TryGetAttribute("gameName", out text))
			{
				Debug.LogError("Failed to read RemoteWorldInfo at path \"" + filePath + "\". Could not find gameName attribute.");
				remoteWorldInfo = default(RemoteWorldInfo);
				result = false;
			}
			else if (!root.TryGetAttribute("worldName", out text2))
			{
				Debug.LogError("Failed to read RemoteWorldInfo at path \"" + filePath + "\". Could not find worldName attribute.");
				remoteWorldInfo = default(RemoteWorldInfo);
				result = false;
			}
			else if (!root.TryGetAttribute("gameVersion", out serializedVersionInformation))
			{
				Debug.LogError("Failed to read RemoteWorldInfo at path \"" + filePath + "\". Could not find gameVersion attribute.");
				remoteWorldInfo = default(RemoteWorldInfo);
				result = false;
			}
			else if (!VersionInformation.TryParseSerializedString(serializedVersionInformation, out versionInformation))
			{
				Debug.LogError("Failed to read RemoteWorldInfo at path \"" + filePath + "\". Failed to parse gameVersion value.");
				remoteWorldInfo = default(RemoteWorldInfo);
				result = false;
			}
			else if (!root.TryGetAttribute("saveSize", out s))
			{
				Debug.LogError("Failed to read RemoteWorldInfo at path \"" + filePath + "\". Could not find saveSize attribute.");
				remoteWorldInfo = default(RemoteWorldInfo);
				result = false;
			}
			else if (!long.TryParse(s, out num))
			{
				Debug.LogError("Failed to read RemoteWorldInfo at path \"" + filePath + "\". Failed to parse saveSize value.");
				remoteWorldInfo = default(RemoteWorldInfo);
				result = false;
			}
			else
			{
				remoteWorldInfo = new RemoteWorldInfo(text, text2, versionInformation, num);
				result = true;
			}
		}
		catch (Exception arg)
		{
			Debug.LogError(string.Format("Failed to read RemoteWorldInfo at path \"{0}\". Failed with exception: \n\n{1}", filePath, arg));
			remoteWorldInfo = default(RemoteWorldInfo);
			result = false;
		}
		return result;
	}

	public void Write(string filePath)
	{
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.CreateXmlDeclaration();
		XmlElement element = xmlDocument.AddXmlElement("RemoteWorldInfo");
		element.SetAttrib("gameName", this.gameName);
		element.SetAttrib("worldName", this.worldName);
		element.SetAttrib("gameVersion", this.gameVersion.SerializableString);
		element.SetAttrib("saveSize", this.saveSize.ToString());
		xmlDocument.SdSave(filePath);
	}

	public const string FileNameString = "RemoteWorldInfo.xml";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string GameNameString = "gameName";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string WorldNameString = "worldName";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string GameVersionString = "gameVersion";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string SaveSizeString = "saveSize";

	public readonly string gameName;

	public readonly string worldName;

	public readonly VersionInformation gameVersion;

	public readonly long saveSize;
}

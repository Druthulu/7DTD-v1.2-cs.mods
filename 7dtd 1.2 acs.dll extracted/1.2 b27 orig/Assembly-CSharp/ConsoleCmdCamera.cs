using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdCamera : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[]
		{
			"camera",
			"cam"
		};
	}

	public override bool IsExecuteOnClient
	{
		get
		{
			return true;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Lock/unlock camera movement or load/save a specific camera position";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Usage:\n   1. cam save <name> [comment]\n   2. cam load <name>\n   3. cam list\n   4. cam lock\n   5. cam unlock\n1. Save the current player's position and camera view or the camera position\nand view if in detached mode under the given name. Optionally a more descriptive\ncomment can be supplied.\n2. Load the position and direction with the given name. If in detached camera\nmode the camera itself will be adjusted, otherwise the player will be teleported.\n3. List the saved camera positions.\n4/5. Lock/unlock the camera rotation. Can also be achieved with the \"Lock Camera\" key.";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count < 1)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("No sub command given.");
			return;
		}
		if (!_senderInfo.IsLocalGame)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Command can only be used on clients");
			return;
		}
		EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		if (_params[0].EqualsCaseInsensitive("lock"))
		{
			this.ExecuteLock(_params, primaryPlayer);
			return;
		}
		if (_params[0].EqualsCaseInsensitive("unlock"))
		{
			this.ExecuteUnlock(_params, primaryPlayer);
			return;
		}
		if (_params[0].EqualsCaseInsensitive("save"))
		{
			this.ExecuteSave(_params, primaryPlayer);
			return;
		}
		if (_params[0].EqualsCaseInsensitive("load"))
		{
			this.ExecuteLoad(_params, primaryPlayer);
			return;
		}
		if (_params[0].EqualsCaseInsensitive("list"))
		{
			this.ExecuteList(_params);
			return;
		}
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Invalid sub command \"" + _params[0] + "\".");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ExecuteLock(List<string> _params, EntityPlayerLocal _epl)
	{
		_epl.movementInput.bCameraPositionLocked = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ExecuteUnlock(List<string> _params, EntityPlayerLocal _epl)
	{
		_epl.movementInput.bCameraPositionLocked = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ExecuteSave(List<string> _params, EntityPlayerLocal _epl)
	{
		if (_params.Count < 2)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Command requires a name for the position.");
			return;
		}
		string text = _params[1];
		string comment = (_params.Count > 2) ? _params[2] : null;
		Vector3 position;
		Vector3 vector;
		if (_epl.movementInput.bDetachedCameraMove)
		{
			position = _epl.cameraTransform.position - Constants.cDefaultCameraPlayerOffset;
			vector = _epl.cameraTransform.localEulerAngles;
			vector.x = -vector.x;
		}
		else
		{
			position = _epl.GetPosition();
			vector = _epl.rotation;
		}
		IDictionary<string, ConsoleCmdCamera.CameraPosition> dictionary = this.Load();
		dictionary[text] = new ConsoleCmdCamera.CameraPosition(position, vector, comment);
		this.Save(dictionary);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Position saved with name \"" + text + "\"");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ExecuteLoad(List<string> _params, EntityPlayerLocal _epl)
	{
		if (_params.Count < 2)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("No position name given.");
			return;
		}
		ConsoleCmdCamera.CameraPosition cameraPosition;
		if (!this.Load().TryGetValue(_params[1], out cameraPosition))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Position name not found.");
			return;
		}
		if (_epl.movementInput.bDetachedCameraMove)
		{
			_epl.cameraTransform.position = cameraPosition.Position + Constants.cDefaultCameraPlayerOffset;
			Vector3 direction = cameraPosition.Direction;
			direction.x = -direction.x;
			_epl.cameraTransform.localEulerAngles = direction;
			return;
		}
		_epl.TeleportToPosition(cameraPosition.Position, false, new Vector3?(cameraPosition.Direction));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ExecuteList(List<string> _params)
	{
		IEnumerable<KeyValuePair<string, ConsoleCmdCamera.CameraPosition>> enumerable = this.Load();
		string text = (_params.Count > 1) ? _params[1] : null;
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Saved camera positions:");
		foreach (KeyValuePair<string, ConsoleCmdCamera.CameraPosition> keyValuePair in enumerable)
		{
			if (text == null || keyValuePair.Key.ContainsCaseInsensitive(text) || keyValuePair.Value.Comment.ContainsCaseInsensitive(text))
			{
				string str = string.IsNullOrEmpty(keyValuePair.Value.Comment) ? "" : (" (" + keyValuePair.Value.Comment + ")");
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("  " + keyValuePair.Key + str);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string GetFullFilePath()
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			return GameIO.GetSaveGameDir() + "/camerapositions.xml";
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
		{
			return GameIO.GetSaveGameLocalDir() + "/camerapositions.xml";
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IDictionary<string, ConsoleCmdCamera.CameraPosition> Load()
	{
		SortedDictionary<string, ConsoleCmdCamera.CameraPosition> sortedDictionary = new SortedDictionary<string, ConsoleCmdCamera.CameraPosition>(StringComparer.OrdinalIgnoreCase);
		string fullFilePath = this.GetFullFilePath();
		if (!SdFile.Exists(fullFilePath))
		{
			return sortedDictionary;
		}
		XmlDocument xmlDocument = new XmlDocument();
		try
		{
			xmlDocument.SdLoad(fullFilePath);
		}
		catch (XmlException ex)
		{
			Log.Error("Failed loading camera file: " + ex.Message);
			return sortedDictionary;
		}
		if (xmlDocument.DocumentElement == null)
		{
			Log.Warning("Camera file has no root XML element.");
			return sortedDictionary;
		}
		foreach (object obj in xmlDocument.DocumentElement.ChildNodes)
		{
			XmlNode xmlNode = (XmlNode)obj;
			if (xmlNode.NodeType == XmlNodeType.Element && xmlNode.Name == "position")
			{
				XmlElement xmlElement = (XmlElement)xmlNode;
				if (!xmlElement.HasAttribute("name"))
				{
					Log.Warning("Ignoring camera-entry because of missing 'name' attribute: " + xmlElement.OuterXml);
				}
				else if (!xmlElement.HasAttribute("position"))
				{
					Log.Warning("Ignoring camera-entry because of missing 'position' attribute: " + xmlElement.OuterXml);
				}
				else if (!xmlElement.HasAttribute("direction"))
				{
					Log.Warning("Ignoring camera-entry because of missing 'direction' attribute: " + xmlElement.OuterXml);
				}
				else
				{
					string attribute = xmlElement.GetAttribute("name");
					Vector3 position = StringParsers.ParseVector3(xmlElement.GetAttribute("position"), 0, -1);
					Vector3 direction = StringParsers.ParseVector3(xmlElement.GetAttribute("direction"), 0, -1);
					string comment = null;
					if (xmlElement.HasAttribute("comment"))
					{
						comment = xmlElement.GetAttribute("comment");
					}
					sortedDictionary.Add(attribute, new ConsoleCmdCamera.CameraPosition(position, direction, comment));
				}
			}
		}
		return sortedDictionary;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Save(IDictionary<string, ConsoleCmdCamera.CameraPosition> _positions)
	{
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.CreateXmlDeclaration();
		XmlElement node = xmlDocument.AddXmlElement("camerapositions");
		foreach (KeyValuePair<string, ConsoleCmdCamera.CameraPosition> keyValuePair in _positions)
		{
			XmlElement element = node.AddXmlElement("position").SetAttrib("name", keyValuePair.Key).SetAttrib("position", keyValuePair.Value.Position.ToString()).SetAttrib("direction", keyValuePair.Value.Direction.ToString());
			if (!string.IsNullOrEmpty(keyValuePair.Value.Comment))
			{
				element.SetAttrib("comment", keyValuePair.Value.Comment);
			}
		}
		xmlDocument.SdSave(this.GetFullFilePath());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class CameraPosition : IEquatable<ConsoleCmdCamera.CameraPosition>
	{
		public CameraPosition(Vector3 _position, Vector3 _direction, string _comment)
		{
			this.Position = _position;
			this.Direction = _direction;
			this.Comment = _comment;
		}

		public bool Equals(ConsoleCmdCamera.CameraPosition _other)
		{
			return _other != null && (this == _other || (this.Position.Equals(_other.Position) && this.Direction.Equals(_other.Direction)));
		}

		public override bool Equals(object _obj)
		{
			return _obj != null && (this == _obj || (!(_obj.GetType() != base.GetType()) && this.Equals((ConsoleCmdCamera.CameraPosition)_obj)));
		}

		public override int GetHashCode()
		{
			return this.Position.GetHashCode() * 397 ^ this.Direction.GetHashCode();
		}

		public readonly Vector3 Position;

		public readonly Vector3 Direction;

		public readonly string Comment;
	}
}

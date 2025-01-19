using System;
using System.Collections.Generic;
using System.Xml;

public class AdminCommands : AdminSectionAbs
{
	public AdminCommands(AdminTools _parent) : base(_parent, "commands")
	{
	}

	public override void Clear()
	{
		this.commands.Clear();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void ParseElement(XmlElement _childElement)
	{
		AdminCommands.CommandPermission commandPermission;
		if (AdminCommands.CommandPermission.TryParse(_childElement, out commandPermission))
		{
			this.commands[commandPermission.Command] = commandPermission;
		}
	}

	public override void Save(XmlElement _root)
	{
		XmlElement xmlElement = _root.AddXmlElement(this.SectionTypeName);
		xmlElement.AddXmlComment(" <permission cmd=\"dm\" permission_level=\"0\" /> ");
		xmlElement.AddXmlComment(" <permission cmd=\"kick\" permission_level=\"1\" /> ");
		xmlElement.AddXmlComment(" <permission cmd=\"say\" permission_level=\"1000\" /> ");
		foreach (KeyValuePair<string, AdminCommands.CommandPermission> keyValuePair in this.commands)
		{
			keyValuePair.Value.ToXml(xmlElement);
		}
	}

	public void AddCommand(string _cmd, int _permissionLevel, bool _save = true)
	{
		AdminTools parent = this.Parent;
		lock (parent)
		{
			AdminCommands.CommandPermission value = new AdminCommands.CommandPermission(_cmd, _permissionLevel);
			this.commands[_cmd] = value;
			if (_save)
			{
				this.Parent.Save();
			}
		}
	}

	public bool RemoveCommand(string[] _cmds)
	{
		AdminTools parent = this.Parent;
		bool result;
		lock (parent)
		{
			bool flag2 = false;
			foreach (string key in _cmds)
			{
				flag2 |= this.commands.Remove(key);
			}
			if (flag2)
			{
				this.Parent.Save();
			}
			result = flag2;
		}
		return result;
	}

	public bool IsPermissionDefined(string[] _cmds)
	{
		AdminTools parent = this.Parent;
		bool result;
		lock (parent)
		{
			foreach (string key in _cmds)
			{
				if (this.commands.ContainsKey(key))
				{
					return true;
				}
			}
			result = false;
		}
		return result;
	}

	public Dictionary<string, AdminCommands.CommandPermission> GetCommands()
	{
		AdminTools parent = this.Parent;
		Dictionary<string, AdminCommands.CommandPermission> result;
		lock (parent)
		{
			result = this.commands;
		}
		return result;
	}

	public int GetCommandPermissionLevel(string[] _cmdNames)
	{
		AdminTools parent = this.Parent;
		int permissionLevel;
		lock (parent)
		{
			permissionLevel = this.GetAdminToolsCommandPermission(_cmdNames).PermissionLevel;
		}
		return permissionLevel;
	}

	public AdminCommands.CommandPermission GetAdminToolsCommandPermission(string[] _cmdNames)
	{
		AdminTools parent = this.Parent;
		AdminCommands.CommandPermission result;
		lock (parent)
		{
			foreach (string text in _cmdNames)
			{
				if (!string.IsNullOrEmpty(text) && this.commands.ContainsKey(text))
				{
					return this.commands[text];
				}
			}
			result = this.defaultCommandPermission;
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<string, AdminCommands.CommandPermission> commands = new Dictionary<string, AdminCommands.CommandPermission>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly AdminCommands.CommandPermission defaultCommandPermission = new AdminCommands.CommandPermission("", 0);

	public readonly struct CommandPermission
	{
		public CommandPermission(string _cmd, int _permissionLevel)
		{
			this.Command = _cmd;
			this.PermissionLevel = _permissionLevel;
		}

		public void ToXml(XmlElement _parent)
		{
			_parent.AddXmlElement("permission").SetAttrib("cmd", this.Command).SetAttrib("permission_level", this.PermissionLevel.ToString());
		}

		public static bool TryParse(XmlElement _element, out AdminCommands.CommandPermission _result)
		{
			_result = default(AdminCommands.CommandPermission);
			string attribute = _element.GetAttribute("cmd");
			if (string.IsNullOrEmpty(attribute))
			{
				Log.Warning("Ignoring permission-entry because of missing or empty 'cmd' attribute: " + _element.OuterXml);
				return false;
			}
			if (!_element.HasAttribute("permission_level"))
			{
				Log.Warning("Ignoring permission-entry because of missing 'permission_level' attribute: " + _element.OuterXml);
				return false;
			}
			int permissionLevel;
			if (!int.TryParse(_element.GetAttribute("permission_level"), out permissionLevel))
			{
				Log.Warning("Ignoring permission-entry because of invalid (non-numeric) value for 'permission_level' attribute: " + _element.OuterXml);
				return false;
			}
			_result = new AdminCommands.CommandPermission(attribute, permissionLevel);
			return true;
		}

		public readonly string Command;

		public readonly int PermissionLevel;
	}
}

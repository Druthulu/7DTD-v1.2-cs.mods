using System;
using System.Collections.Generic;
using Platform;

public abstract class ConsoleCmdAbstract : IConsoleCommand
{
	public virtual bool IsExecuteOnClient
	{
		get
		{
			return false;
		}
	}

	public virtual int DefaultPermissionLevel
	{
		get
		{
			return 0;
		}
	}

	public virtual bool AllowedInMainMenu
	{
		get
		{
			return false;
		}
	}

	public virtual DeviceFlag AllowedDeviceTypes
	{
		get
		{
			return DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX;
		}
	}

	public virtual DeviceFlag AllowedDeviceTypesClient
	{
		get
		{
			return DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX;
		}
	}

	public ConsoleCmdAbstract()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract string[] getCommands();

	public virtual string[] GetCommands()
	{
		string[] result;
		if ((result = this.commandNamesCache) == null)
		{
			result = (this.commandNamesCache = this.getCommands());
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract string getDescription();

	public virtual string GetDescription()
	{
		string result;
		if ((result = this.commandDescriptionCache) == null)
		{
			result = (this.commandDescriptionCache = this.getDescription());
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual string getHelp()
	{
		return null;
	}

	public virtual string GetHelp()
	{
		string result;
		if ((result = this.commandHelpCache) == null)
		{
			result = (this.commandHelpCache = this.getHelp());
		}
		return result;
	}

	public abstract void Execute(List<string> _params, CommandSenderInfo _senderInfo);

	[PublicizedFrom(EAccessModifier.Private)]
	public string[] commandNamesCache;

	[PublicizedFrom(EAccessModifier.Private)]
	public string commandDescriptionCache;

	[PublicizedFrom(EAccessModifier.Private)]
	public string commandHelpCache;
}

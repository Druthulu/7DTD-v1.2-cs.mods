using System;
using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdKick : ConsoleCmdAbstract
{
	public override DeviceFlag AllowedDeviceTypes
	{
		get
		{
			return DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[]
		{
			"kick"
		};
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Kicks user with optional reason. \"kick playername reason\"";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count < 1)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Wrong number of arguments, expected at least 1, found " + _params.Count.ToString() + ".");
			return;
		}
		PlatformUserIdentifierAbs platformUserIdentifierAbs;
		ClientInfo clientInfo;
		if (ConsoleHelper.ParseParamPartialNameOrId(_params[0], out platformUserIdentifierAbs, out clientInfo, true) != 1 || clientInfo == null)
		{
			return;
		}
		string text = string.Empty;
		if (_params.Count > 1)
		{
			text = _params[1];
		}
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Kicking Player " + clientInfo.playerName + ": " + text);
		ClientInfo cInfo = clientInfo;
		GameUtils.EKickReason kickReason = GameUtils.EKickReason.ManualKick;
		int apiResponseEnum = 0;
		string customReason = text;
		GameUtils.KickPlayerForClientInfo(cInfo, new GameUtils.KickPlayerData(kickReason, apiResponseEnum, default(DateTime), customReason));
	}
}

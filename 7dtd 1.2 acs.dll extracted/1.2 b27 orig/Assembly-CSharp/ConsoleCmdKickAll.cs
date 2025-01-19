﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdKickAll : ConsoleCmdAbstract
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
			"kickall"
		};
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Kicks all users with optional reason. \"kickall reason\"";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		string text = string.Empty;
		if (_params.Count > 0)
		{
			text = _params[0];
		}
		ReadOnlyCollection<ClientInfo> list = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.List;
		for (int i = 0; i < list.Count; i++)
		{
			ClientInfo clientInfo = list[i];
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Kicking Player {0}: {1}", clientInfo.playerName, text));
			ClientInfo cInfo = clientInfo;
			GameUtils.EKickReason kickReason = GameUtils.EKickReason.ManualKick;
			int apiResponseEnum = 0;
			string customReason = text;
			GameUtils.KickPlayerForClientInfo(cInfo, new GameUtils.KickPlayerData(kickReason, apiResponseEnum, default(DateTime), customReason));
		}
	}
}

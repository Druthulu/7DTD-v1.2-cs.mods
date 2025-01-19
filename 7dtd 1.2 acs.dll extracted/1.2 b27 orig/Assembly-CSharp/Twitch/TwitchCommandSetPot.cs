﻿using System;
using System.Collections.Generic;

namespace Twitch
{
	public class TwitchCommandSetPot : BaseTwitchCommand
	{
		public override BaseTwitchCommand.PermissionLevels RequiredPermission
		{
			get
			{
				return BaseTwitchCommand.PermissionLevels.Mod;
			}
		}

		public override string[] CommandText
		{
			get
			{
				return new string[]
				{
					"#setpot"
				};
			}
		}

		public override string[] LocalizedCommandNames
		{
			get
			{
				return new string[]
				{
					Localization.Get("TwitchCommand_SetPot", false)
				};
			}
		}

		public override void Execute(ViewerEntry entry, TwitchIRCClient.TwitchChatMessage message)
		{
			string[] array = message.Message.Split(' ', StringSplitOptions.None);
			if (array.Length == 2)
			{
				int pot = 0;
				if (int.TryParse(array[1], out pot))
				{
					TwitchManager.Current.SetPot(pot);
				}
			}
		}

		public override void ExecuteConsole(List<string> arguments)
		{
			if (arguments.Count == 2)
			{
				int pot = 0;
				if (int.TryParse(arguments[1], out pot))
				{
					TwitchManager.Current.SetPot(pot);
				}
			}
		}
	}
}

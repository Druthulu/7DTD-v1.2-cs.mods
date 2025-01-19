﻿using System;
using System.Collections.Generic;

namespace Twitch
{
	public class TwitchCommandAddBitCredit : BaseTwitchCommand
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
					"#addcredit"
				};
			}
		}

		public override string[] LocalizedCommandNames
		{
			get
			{
				return new string[]
				{
					Localization.Get("TwitchCommand_AddBitCredit", false)
				};
			}
		}

		public override void Execute(ViewerEntry entry, TwitchIRCClient.TwitchChatMessage message)
		{
			string[] array = message.Message.Split(' ', StringSplitOptions.None);
			if (array.Length == 3)
			{
				int credit = 0;
				if (int.TryParse(array[2], out credit))
				{
					TwitchManager.Current.ViewerData.AddCredit(array[1], credit, true);
				}
			}
		}

		public override void ExecuteConsole(List<string> arguments)
		{
			if (arguments.Count == 3)
			{
				int credit = 0;
				if (int.TryParse(arguments[2], out credit))
				{
					TwitchManager.Current.ViewerData.AddCredit(arguments[1], credit, true);
				}
			}
		}
	}
}

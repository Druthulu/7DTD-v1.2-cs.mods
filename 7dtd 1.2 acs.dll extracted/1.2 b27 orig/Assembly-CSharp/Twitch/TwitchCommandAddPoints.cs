﻿using System;
using System.Collections.Generic;

namespace Twitch
{
	public class TwitchCommandAddPoints : BaseTwitchCommand
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
					"#addpp"
				};
			}
		}

		public override string[] LocalizedCommandNames
		{
			get
			{
				return new string[]
				{
					Localization.Get("TwitchCommand_AddPoints", false)
				};
			}
		}

		public override void Execute(ViewerEntry entry, TwitchIRCClient.TwitchChatMessage message)
		{
			string[] array = message.Message.Split(' ', StringSplitOptions.None);
			if (array.Length == 3)
			{
				int points = 0;
				if (int.TryParse(array[2], out points))
				{
					if (array[1].EqualsCaseInsensitive(BaseTwitchCommand.allText))
					{
						TwitchManager.Current.ViewerData.AddPoints("", points, false, true);
						return;
					}
					TwitchManager.Current.ViewerData.AddPoints(array[1], points, false, true);
				}
			}
		}

		public override void ExecuteConsole(List<string> arguments)
		{
			if (arguments.Count == 3)
			{
				int points = 0;
				if (int.TryParse(arguments[2], out points))
				{
					if (arguments[1].EqualsCaseInsensitive(BaseTwitchCommand.allText))
					{
						TwitchManager.Current.ViewerData.AddPoints("", points, false, true);
						return;
					}
					TwitchManager.Current.ViewerData.AddPoints(arguments[1], points, false, true);
				}
			}
		}
	}
}

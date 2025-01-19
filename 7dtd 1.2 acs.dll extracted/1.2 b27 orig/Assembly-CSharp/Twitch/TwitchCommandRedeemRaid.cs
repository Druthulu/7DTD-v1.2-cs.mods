using System;
using System.Collections.Generic;
using System.Globalization;

namespace Twitch
{
	public class TwitchCommandRedeemRaid : BaseTwitchCommand
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
					"#redeem_raid"
				};
			}
		}

		public override string[] LocalizedCommandNames
		{
			get
			{
				return new string[]
				{
					Localization.Get("TwitchCommand_RedeemRaid", false)
				};
			}
		}

		public override void Execute(ViewerEntry entry, TwitchIRCClient.TwitchChatMessage message)
		{
			string[] array = message.Message.Split(' ', StringSplitOptions.None);
			if (array.Length == 3)
			{
				string text = array[1];
				if (text.StartsWith("@"))
				{
					text = text.Substring(1).ToLower();
				}
				else
				{
					text = text.ToLower();
				}
				int viewerAmount = 0;
				if (StringParsers.TryParseSInt32(array[2], out viewerAmount, 0, -1, NumberStyles.Integer))
				{
					TwitchManager.Current.HandleRaidRedeem(text, viewerAmount, null);
				}
			}
		}

		public override void ExecuteConsole(List<string> arguments)
		{
			if (arguments.Count == 3)
			{
				string text = arguments[1];
				if (text.StartsWith("@"))
				{
					text = text.Substring(1).ToLower();
				}
				else
				{
					text = text.ToLower();
				}
				int viewerAmount = 0;
				if (StringParsers.TryParseSInt32(arguments[2], out viewerAmount, 0, -1, NumberStyles.Integer))
				{
					TwitchManager.Current.HandleRaidRedeem(text, viewerAmount, null);
				}
			}
		}
	}
}

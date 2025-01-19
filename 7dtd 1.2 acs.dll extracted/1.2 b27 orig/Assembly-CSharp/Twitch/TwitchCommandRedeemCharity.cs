using System;
using System.Collections.Generic;
using System.Globalization;

namespace Twitch
{
	public class TwitchCommandRedeemCharity : BaseTwitchCommand
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
					"#redeem_charity"
				};
			}
		}

		public override string[] LocalizedCommandNames
		{
			get
			{
				return new string[]
				{
					Localization.Get("TwitchCommand_RedeemCharity", false)
				};
			}
		}

		public override void Execute(ViewerEntry entry, TwitchIRCClient.TwitchChatMessage message)
		{
			string[] array = message.Message.Split(' ', StringSplitOptions.None);
			if (array.Length == 2)
			{
				int charityAmount = 0;
				if (StringParsers.TryParseSInt32(array[1], out charityAmount, 0, -1, NumberStyles.Integer))
				{
					TwitchManager.Current.HandleCharityRedeem(message.UserName, charityAmount, null);
					return;
				}
			}
			else if (array.Length == 3)
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
				int charityAmount2 = 0;
				if (StringParsers.TryParseSInt32(array[2], out charityAmount2, 0, -1, NumberStyles.Integer))
				{
					TwitchManager.Current.HandleCharityRedeem(text, charityAmount2, null);
				}
			}
		}

		public override void ExecuteConsole(List<string> arguments)
		{
			if (arguments.Count == 2)
			{
				int charityAmount = 0;
				if (StringParsers.TryParseSInt32(arguments[1], out charityAmount, 0, -1, NumberStyles.Integer))
				{
					TwitchManager.Current.HandleCharityRedeem(TwitchManager.Current.Authentication.userName, charityAmount, null);
					return;
				}
			}
			else if (arguments.Count == 3)
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
				int charityAmount2 = 0;
				if (StringParsers.TryParseSInt32(arguments[2], out charityAmount2, 0, -1, NumberStyles.Integer))
				{
					TwitchManager.Current.HandleCharityRedeem(text, charityAmount2, null);
				}
			}
		}
	}
}

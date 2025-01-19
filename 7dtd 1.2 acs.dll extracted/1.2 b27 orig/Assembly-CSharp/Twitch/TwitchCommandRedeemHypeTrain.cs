using System;
using System.Collections.Generic;
using System.Globalization;

namespace Twitch
{
	public class TwitchCommandRedeemHypeTrain : BaseTwitchCommand
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
					"#redeem_hypetrain"
				};
			}
		}

		public override string[] LocalizedCommandNames
		{
			get
			{
				return new string[]
				{
					Localization.Get("TwitchCommand_RedeemHypeTrain", false)
				};
			}
		}

		public override void Execute(ViewerEntry entry, TwitchIRCClient.TwitchChatMessage message)
		{
			string[] array = message.Message.Split(' ', StringSplitOptions.None);
			if (array.Length == 2)
			{
				int hypeTrainLevel = 0;
				if (StringParsers.TryParseSInt32(array[1], out hypeTrainLevel, 0, -1, NumberStyles.Integer))
				{
					TwitchManager.Current.HandleHypeTrainRedeem(hypeTrainLevel);
				}
			}
		}

		public override void ExecuteConsole(List<string> arguments)
		{
			if (arguments.Count == 2)
			{
				int hypeTrainLevel = 0;
				if (StringParsers.TryParseSInt32(arguments[1], out hypeTrainLevel, 0, -1, NumberStyles.Integer))
				{
					TwitchManager.Current.HandleHypeTrainRedeem(hypeTrainLevel);
				}
			}
		}
	}
}

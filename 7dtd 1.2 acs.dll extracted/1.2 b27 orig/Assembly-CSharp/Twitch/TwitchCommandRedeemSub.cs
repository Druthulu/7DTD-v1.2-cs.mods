﻿using System;
using System.Collections.Generic;
using System.Globalization;

namespace Twitch
{
	public class TwitchCommandRedeemSub : BaseTwitchCommand
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
					"#redeem_sub"
				};
			}
		}

		public override string[] LocalizedCommandNames
		{
			get
			{
				return new string[]
				{
					Localization.Get("TwitchCommand_RedeemSub", false)
				};
			}
		}

		public override void Execute(ViewerEntry entry, TwitchIRCClient.TwitchChatMessage message)
		{
			string[] array = message.Message.Split(' ', StringSplitOptions.None);
			if (array.Length == 2)
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
				TwitchManager.Current.HandleSubEvent(text, 1, TwitchSubEventEntry.SubTierTypes.Tier1);
				return;
			}
			if (array.Length == 3)
			{
				string text2 = array[1];
				if (text2.StartsWith("@"))
				{
					text2 = text2.Substring(1).ToLower();
				}
				else
				{
					text2 = text2.ToLower();
				}
				int months = 0;
				if (StringParsers.TryParseSInt32(array[2], out months, 0, -1, NumberStyles.Integer))
				{
					TwitchManager.Current.HandleSubEvent(text2, months, TwitchSubEventEntry.SubTierTypes.Tier1);
					return;
				}
			}
			else if (array.Length == 4)
			{
				string text3 = array[1];
				if (text3.StartsWith("@"))
				{
					text3 = text3.Substring(1).ToLower();
				}
				else
				{
					text3 = text3.ToLower();
				}
				int months2 = 0;
				int num = 0;
				TwitchSubEventEntry.SubTierTypes tier = TwitchSubEventEntry.SubTierTypes.Tier1;
				StringParsers.TryParseSInt32(array[2], out months2, 0, -1, NumberStyles.Integer);
				if (array[3].Trim().ToLower() == "prime")
				{
					tier = TwitchSubEventEntry.SubTierTypes.Prime;
				}
				else
				{
					StringParsers.TryParseSInt32(array[3], out num, 0, -1, NumberStyles.Integer);
					if (num != 2)
					{
						if (num == 3)
						{
							tier = TwitchSubEventEntry.SubTierTypes.Tier3;
						}
					}
					else
					{
						tier = TwitchSubEventEntry.SubTierTypes.Tier2;
					}
				}
				TwitchManager.Current.HandleSubEvent(text3, months2, tier);
			}
		}

		public override void ExecuteConsole(List<string> arguments)
		{
			if (arguments.Count == 2)
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
				TwitchManager.Current.HandleSubEvent(text, 1, TwitchSubEventEntry.SubTierTypes.Tier1);
				return;
			}
			if (arguments.Count == 3)
			{
				string text2 = arguments[1];
				if (text2.StartsWith("@"))
				{
					text2 = text2.Substring(1).ToLower();
				}
				else
				{
					text2 = text2.ToLower();
				}
				int months = 0;
				if (StringParsers.TryParseSInt32(arguments[2], out months, 0, -1, NumberStyles.Integer))
				{
					TwitchManager.Current.HandleSubEvent(text2, months, TwitchSubEventEntry.SubTierTypes.Tier1);
					return;
				}
			}
			else if (arguments.Count == 4)
			{
				string text3 = arguments[1];
				if (text3.StartsWith("@"))
				{
					text3 = text3.Substring(1).ToLower();
				}
				else
				{
					text3 = text3.ToLower();
				}
				int months2 = 0;
				int num = 0;
				TwitchSubEventEntry.SubTierTypes tier = TwitchSubEventEntry.SubTierTypes.Tier1;
				StringParsers.TryParseSInt32(arguments[2], out months2, 0, -1, NumberStyles.Integer);
				if (arguments[3].Trim().ToLower() == "prime")
				{
					tier = TwitchSubEventEntry.SubTierTypes.Prime;
				}
				else
				{
					StringParsers.TryParseSInt32(arguments[3], out num, 0, -1, NumberStyles.Integer);
					if (num != 2)
					{
						if (num == 3)
						{
							tier = TwitchSubEventEntry.SubTierTypes.Tier3;
						}
					}
					else
					{
						tier = TwitchSubEventEntry.SubTierTypes.Tier2;
					}
				}
				TwitchManager.Current.HandleSubEvent(text3, months2, tier);
			}
		}
	}
}

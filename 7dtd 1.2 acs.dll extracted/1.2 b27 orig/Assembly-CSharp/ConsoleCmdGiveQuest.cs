using System;
using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdGiveQuest : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[]
		{
			"givequest"
		};
	}

	public override bool IsExecuteOnClient
	{
		get
		{
			return true;
		}
	}

	public override DeviceFlag AllowedDeviceTypesClient
	{
		get
		{
			return DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5;
		}
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (GameManager.IsDedicatedServer)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Cannot execute givequest on dedicated server, please execute as a client");
		}
		if (_params.Count < 1)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("givequest requires a quest id. Available quests:");
			foreach (KeyValuePair<string, QuestClass> keyValuePair in QuestClass.s_Quests)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("   " + keyValuePair.Key);
			}
			return;
		}
		if (!QuestClass.s_Quests.ContainsKey(_params[0]))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Quest '{0}' does not exist!", _params[0]));
			return;
		}
		string text = _params[0];
		foreach (KeyValuePair<string, QuestClass> keyValuePair2 in QuestClass.s_Quests)
		{
			if (keyValuePair2.Key.EqualsCaseInsensitive(text))
			{
				text = keyValuePair2.Key;
				break;
			}
		}
		Quest quest = QuestClass.CreateQuest(text);
		if (quest == null)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Quest '{0}' does not exist!", _params[0]));
			return;
		}
		XUiM_Player.GetPlayer().QuestJournal.AddQuest(quest, true);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "usage: givequest questname";
	}
}

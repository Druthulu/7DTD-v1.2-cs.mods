using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdGiveXp : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[]
		{
			"givexp"
		};
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Give XP to a player";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Give a player experience points\nUsage:\n   givexp <entity id / player name / user id> <number>";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count < 2)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("givexp requires a target entity id and xp amount");
			return;
		}
		EntityPlayer entityPlayer = null;
		ClientInfo forNameOrId = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.GetForNameOrId(_params[0], true, false);
		int key;
		if (forNameOrId != null)
		{
			entityPlayer = GameManager.Instance.World.Players.dict[forNameOrId.entityId];
		}
		else if (int.TryParse(_params[0], out key))
		{
			GameManager.Instance.World.Players.dict.TryGetValue(key, out entityPlayer);
		}
		if (entityPlayer == null)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Playername or entity id not found.");
			return;
		}
		int num;
		if (!int.TryParse(_params[1], out num))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("xp amount must be a number.");
			return;
		}
		if (num < 0)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("xp amount must be positive.");
			return;
		}
		num = Mathf.Clamp(num, 0, 1073741823);
		if (entityPlayer.isEntityRemote)
		{
			NetPackageEntityAddExpClient package = NetPackageManager.GetPackage<NetPackageEntityAddExpClient>().Setup(entityPlayer.entityId, num, Progression.XPTypes.Debug);
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(package, false, entityPlayer.entityId, -1, -1, null, 192);
			return;
		}
		entityPlayer.Progression.AddLevelExp(num, "_xpOther", Progression.XPTypes.Debug, true, true);
	}
}

using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdServerJunkDrone : ConsoleCmdAbstract
{
	public override bool IsExecuteOnClient
	{
		get
		{
			return false;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[]
		{
			"jds"
		};
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Server drone commands";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return base.getHelp();
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count == 0)
		{
			Log.Out("JSD");
			return;
		}
		if (_params[0].ContainsCaseInsensitive("assign"))
		{
			string playerName = _params[1];
			int entityId = -1;
			if (int.TryParse(_params[2], out entityId))
			{
				EntityPlayer entityPlayer = GameManager.Instance.World.Players.list.Find((EntityPlayer p) => p.EntityName.ContainsCaseInsensitive(playerName));
				EntityDrone entityDrone = GameManager.Instance.World.GetEntity(entityId) as EntityDrone;
				if (entityPlayer && entityDrone)
				{
					entityDrone.position = entityPlayer.getChestPosition() - entityPlayer.GetForwardVector() * 2f;
					entityPlayer.AddOwnedEntity(entityDrone);
					entityDrone.DebugUnstuck();
					Log.Out("Drone {0} assigned to {1}", new object[]
					{
						entityDrone.entityId,
						entityPlayer.EntityName
					});
				}
			}
		}
		if (_params[0].ContainsCaseInsensitive("mas"))
		{
			string playerName = _params[1];
			int num = -1;
			if (int.TryParse(_params[2], out num))
			{
				GameManager.Instance.World.Players.list.Find((EntityPlayer p) => p.EntityName.ContainsCaseInsensitive(playerName));
			}
		}
	}
}

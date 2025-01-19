using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdCover : ConsoleCmdAbstract
{
	public override bool IsExecuteOnClient
	{
		get
		{
			return true;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[]
		{
			"tcs",
			"testCoverSystem"
		};
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "CoverSystem queries.";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		GameManager.Instance.World.GetPrimaryPlayer();
		if (_params.Count == 0)
		{
			EntityCoverManager.DebugModeEnabled = !EntityCoverManager.DebugModeEnabled;
			Log.Warning("coverSystem" + string.Format(" - enabled:{0}", EntityCoverManager.DebugModeEnabled));
			return;
		}
		if (_params[0].ContainsCaseInsensitive("help"))
		{
			Log.Out("coverSystem help:" + Environment.NewLine);
			return;
		}
	}
}

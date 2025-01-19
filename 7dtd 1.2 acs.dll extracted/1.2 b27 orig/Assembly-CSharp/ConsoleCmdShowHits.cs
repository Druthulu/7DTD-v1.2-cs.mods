using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdShowHits : ConsoleCmdAbstract
{
	public override bool IsExecuteOnClient
	{
		get
		{
			return true;
		}
	}

	public override bool AllowedInMainMenu
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
			"showhits"
		};
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		EntityAlive.ShowDebugDisplayHit = !EntityAlive.ShowDebugDisplayHit;
		if (EntityAlive.ShowDebugDisplayHit)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Show Hits on");
		}
		else
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Show Hits off");
		}
		if (_params.Count > 0)
		{
			EntityAlive.DebugDisplayHitTime = StringParsers.ParseFloat(_params[0], 0, -1, NumberStyles.Any);
		}
		if (_params.Count > 1)
		{
			EntityAlive.DebugDisplayHitSize = StringParsers.ParseFloat(_params[1], 0, -1, NumberStyles.Any);
		}
		ItemAction.ShowDebugDisplayHit = EntityAlive.ShowDebugDisplayHit;
		ItemAction.DebugDisplayHitTime = EntityAlive.DebugDisplayHitTime;
		ItemAction.DebugDisplayHitSize = EntityAlive.DebugDisplayHitSize;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Show hit entity locations";
	}
}

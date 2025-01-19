using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdDecoMgr : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[]
		{
			"decomgr"
		};
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "\"decomgr\": Saves a debug texture visualising the DecoOccupiedMap.\n\"decomgr state\": Saves a debug texture visualising the location/state of all of the DecoObjects saved in decorations.7dtd.";
	}

	public override bool IsExecuteOnClient
	{
		get
		{
			return true;
		}
	}

	public override int DefaultPermissionLevel
	{
		get
		{
			return 1000;
		}
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count > 0 && _params[0] == "state")
		{
			DecoManager.Instance.SaveStateDebugTexture("decostate.png");
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Saved decostate.png to application directory.");
			return;
		}
		DecoManager.Instance.SaveDebugTexture("deco.png");
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Saved deco.png to application directory.");
	}
}

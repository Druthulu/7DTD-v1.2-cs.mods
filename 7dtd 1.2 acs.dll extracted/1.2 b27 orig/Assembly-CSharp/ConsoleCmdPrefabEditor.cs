﻿using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdPrefabEditor : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[]
		{
			"prefabeditor",
			"prefabedit",
			"predit"
		};
	}

	public override bool AllowedInMainMenu
	{
		get
		{
			return true;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Open the Prefab Editor";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "prefabeditor";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (PrefabEditModeManager.Instance.IsActive())
		{
			Log.Out("You are already in the prefab editor.");
			return;
		}
		if (!GameManager.Instance.IsSafeToConnect())
		{
			Log.Warning("Please return to the main menu before using this command.");
			return;
		}
		XUiC_EditingTools.OpenPrefabEditor(null);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_connectingToPrefabEditor;
}

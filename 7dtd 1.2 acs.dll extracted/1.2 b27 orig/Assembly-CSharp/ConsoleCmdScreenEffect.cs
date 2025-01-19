using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdScreenEffect : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[]
		{
			"ScreenEffect"
		};
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Sets a screen effect";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "ScreenEffect [name] [intensity] [fade time]\nScreenEffect clear\nScreenEffect reload";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count == 0)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(this.GetHelp());
			return;
		}
		List<EntityPlayerLocal> localPlayers = GameManager.Instance.World.GetLocalPlayers();
		if (_params[0] == "clear")
		{
			for (int i = 0; i < localPlayers.Count; i++)
			{
				localPlayers[i].ScreenEffectManager.DisableScreenEffects();
			}
			return;
		}
		if (_params[0] == "reload")
		{
			for (int j = 0; j < localPlayers.Count; j++)
			{
				localPlayers[j].ScreenEffectManager.ResetEffects();
			}
			return;
		}
		float intensity = 0f;
		float fadeTime = 4f;
		if (_params.Count >= 2)
		{
			StringParsers.TryParseFloat(_params[1], out intensity, 0, -1, NumberStyles.Any);
		}
		if (_params.Count >= 3)
		{
			StringParsers.TryParseFloat(_params[2], out fadeTime, 0, -1, NumberStyles.Any);
		}
		for (int k = 0; k < localPlayers.Count; k++)
		{
			localPlayers[k].ScreenEffectManager.SetScreenEffect(_params[0], intensity, fadeTime);
		}
	}
}

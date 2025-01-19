using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdGraph : ConsoleCmdAbstract
{
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

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[]
		{
			"graph"
		};
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Draws graphs on screen";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Graph commands:\n# - 0 removes all graphs, 1+ sets graphs height\ncvar <name> <count> <max> - show graph of a cvar, line count (0 hides), max graph value (default is 1)\ndr <count> - show dynamic res graph, line count (0 hides)\nfps <count> <fps max> - show fps graph, line count (0 hides), max graph value\npe <name> <count> <max> - show graph of a passive effect (healthmax..), line count (0 hides), max graph value (default is 1)\nspf <count> <spf max> - show seconds per frame graph, line count (0 hides), max graph value\nstat <name> <count> <max> - show graph of a stat (health, stamina..) with line count (0 hides), max graph value (default is 1)\ntex <name> <count> - show texture graph (mem or stream), line count (0 hides)";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count == 0)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(this.GetHelp());
			return;
		}
		if (GameManager.Instance.World == null)
		{
			return;
		}
		GameRenderManager renderManager = GameManager.Instance.World.GetPrimaryPlayer().renderManager;
		GameGraphManager graphManager = renderManager.graphManager;
		string text = _params[0].ToLower();
		if (char.IsDigit(text[0]))
		{
			int num = int.Parse(text);
			if (num == 0)
			{
				graphManager.RemoveAll();
				return;
			}
			graphManager.SetHeight(num);
			return;
		}
		else
		{
			if (text == "dr")
			{
				int sampleCount = 0;
				if (_params.Count >= 2)
				{
					int.TryParse(_params[1], out sampleCount);
				}
				graphManager.Add("Dynamic Res", new GameGraphManager.Graph.Callback(renderManager.DynamicResolutionUpdateGraph), sampleCount, 1f, 0.5f);
				return;
			}
			if (text == "fps")
			{
				int sampleCount2 = 0;
				if (_params.Count >= 2)
				{
					int.TryParse(_params[1], out sampleCount2);
				}
				int num2 = 100;
				if (_params.Count >= 3)
				{
					int.TryParse(_params[2], out num2);
				}
				graphManager.Add("FPS", new GameGraphManager.Graph.Callback(renderManager.FPSUpdateGraph), sampleCount2, (float)num2, 60f);
				return;
			}
			if (text == "spf")
			{
				int sampleCount3 = 0;
				if (_params.Count >= 2)
				{
					int.TryParse(_params[1], out sampleCount3);
				}
				int num3 = 100;
				if (_params.Count >= 3)
				{
					int.TryParse(_params[2], out num3);
				}
				int num4 = 17;
				if (_params.Count >= 4)
				{
					int.TryParse(_params[3], out num4);
				}
				graphManager.Add("SPF", new GameGraphManager.Graph.Callback(renderManager.SPFUpdateGraph), sampleCount3, (float)num3, (float)num4);
				return;
			}
			if (text == "tex" && _params.Count >= 2)
			{
				string a = _params[1].ToLower();
				int sampleCount4 = 0;
				if (_params.Count >= 3)
				{
					int.TryParse(_params[2], out sampleCount4);
				}
				GameGraphManager.Graph.Callback callback;
				if (a == "mem")
				{
					callback = delegate(ref float value)
					{
						value = Texture.currentTextureMemory / 1048576UL;
						return true;
					};
					graphManager.Add("Tex Current", callback, sampleCount4, 12288f, 6144f);
					callback = delegate(ref float value)
					{
						value = Texture.desiredTextureMemory / 1048576UL;
						return true;
					};
					graphManager.Add("Tex Desired", callback, sampleCount4, 12288f, 6144f);
					callback = delegate(ref float value)
					{
						value = Texture.totalTextureMemory / 1048576UL;
						return true;
					};
					graphManager.Add("Tex Total", callback, sampleCount4, 12288f, 6144f);
					return;
				}
				if (!(a == "stream"))
				{
					return;
				}
				callback = delegate(ref float value)
				{
					value = Texture.streamingTextureCount;
					return true;
				};
				graphManager.Add("TStream Textures", callback, sampleCount4, 3000f, 0f);
				callback = delegate(ref float value)
				{
					value = Texture.streamingTextureLoadingCount;
					return true;
				};
				graphManager.Add("TStream Loading", callback, sampleCount4, 100f, 0f);
				callback = delegate(ref float value)
				{
					value = Texture.streamingRendererCount;
					return true;
				};
				graphManager.Add("TStream Renderers", callback, sampleCount4, 25000f, 0f);
				return;
			}
			else if (text == "cvar")
			{
				if (_params.Count < 2)
				{
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("No cvar name");
					return;
				}
				string text2 = _params[1];
				int count = 0;
				if (_params.Count >= 3)
				{
					int.TryParse(_params[2], out count);
				}
				int num5 = 1;
				if (_params.Count >= 4)
				{
					int.TryParse(_params[3], out num5);
				}
				graphManager.AddCVar("CVar " + text2, count, text2, (float)num5, 0f);
				return;
			}
			else if (text == "pe")
			{
				if (_params.Count < 2)
				{
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("No pe name");
					return;
				}
				PassiveEffects passiveEffects;
				Enum.TryParse<PassiveEffects>(_params[1], true, out passiveEffects);
				int count2 = 0;
				if (_params.Count >= 3)
				{
					int.TryParse(_params[2], out count2);
				}
				int num6 = 1;
				if (_params.Count >= 4)
				{
					int.TryParse(_params[3], out num6);
				}
				graphManager.AddPassiveEffect("PE " + passiveEffects.ToStringCached<PassiveEffects>(), count2, passiveEffects, (float)num6, 0f);
				return;
			}
			else
			{
				if (!(text == "stat"))
				{
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Unknown command " + _params[0]);
					return;
				}
				if (_params.Count < 2)
				{
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("No stat name");
					return;
				}
				string text3 = _params[1];
				int count3 = 0;
				if (_params.Count >= 3)
				{
					int.TryParse(_params[2], out count3);
				}
				int num7 = 1;
				if (_params.Count >= 4)
				{
					int.TryParse(_params[3], out num7);
				}
				graphManager.AddStat("Stat " + text3, count3, text3, (float)num7, 0f);
				return;
			}
		}
	}
}

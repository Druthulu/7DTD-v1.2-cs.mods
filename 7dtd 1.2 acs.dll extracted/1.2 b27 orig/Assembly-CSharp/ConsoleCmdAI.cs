using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdAI : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[]
		{
			"ai"
		};
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "AI commands";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "AI commands:\nactivityclear - remove all activity areas (heat)\nlatency - toggles drawing\npathlines - toggles drawing editor path lines\npathgrid - force grid update\nragdoll <force> <time>\nrage <speed> <time> - make all zombies rage (0 - 2, 0 stops) (seconds)\nsendnames - toggles admin clients receiving debug name info";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count == 0)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(this.GetHelp());
			return;
		}
		World world = GameManager.Instance.World;
		string text = _params[0].ToLower();
		uint num = <PrivateImplementationDetails>.ComputeStringHash(text);
		if (num <= 2125639436U)
		{
			if (num <= 1098165981U)
			{
				if (num != 511673672U)
				{
					if (num != 1098165981U)
					{
						goto IL_372;
					}
					if (!(text == "pathlines"))
					{
						goto IL_372;
					}
					GameManager.Instance.DebugAILines = !GameManager.Instance.DebugAILines;
					return;
				}
				else
				{
					if (!(text == "ragdoll"))
					{
						goto IL_372;
					}
					float d = 1f;
					float stunTime = 1f;
					if (_params.Count >= 2)
					{
						float.TryParse(_params[1], out d);
					}
					if (_params.Count >= 3)
					{
						float.TryParse(_params[2], out stunTime);
					}
					using (List<Entity>.Enumerator enumerator = world.Entities.list.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							Entity entity = enumerator.Current;
							EntityAlive entityAlive = entity as EntityAlive;
							if (entityAlive && !(entityAlive is EntityPlayer) && !(entityAlive is EntityTrader))
							{
								entityAlive.emodel.DoRagdoll(stunTime, EnumBodyPartHit.None, -entityAlive.GetForwardVector() * d, Vector3.zero, false);
							}
						}
						return;
					}
				}
			}
			else if (num != 1311049565U)
			{
				if (num != 2125639436U)
				{
					goto IL_372;
				}
				if (!(text == "rage"))
				{
					goto IL_372;
				}
			}
			else
			{
				if (!(text == "ac"))
				{
					goto IL_372;
				}
				goto IL_185;
			}
			float num2 = 1f;
			float time = 5f;
			if (_params.Count >= 2)
			{
				float.TryParse(_params[1], out num2);
			}
			if (_params.Count >= 3)
			{
				float.TryParse(_params[2], out time);
			}
			using (List<Entity>.Enumerator enumerator = world.Entities.list.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Entity entity2 = enumerator.Current;
					EntityHuman entityHuman = entity2 as EntityHuman;
					if (entityHuman)
					{
						if (num2 <= 0f)
						{
							entityHuman.StopRage();
						}
						else
						{
							entityHuman.StartRage(num2, time);
						}
					}
				}
				return;
			}
			goto IL_359;
		}
		if (num <= 2689840001U)
		{
			if (num != 2670967086U)
			{
				if (num != 2689840001U)
				{
					goto IL_372;
				}
				if (!(text == "activityclear"))
				{
					goto IL_372;
				}
			}
			else
			{
				if (!(text == "pathgrid"))
				{
					goto IL_372;
				}
				if (AstarManager.Instance)
				{
					AstarManager.Instance.OriginChanged();
					return;
				}
				return;
			}
		}
		else if (num != 2708253965U)
		{
			if (num != 3909890315U)
			{
				if (num != 4076569433U)
				{
					goto IL_372;
				}
				if (!(text == "latency"))
				{
					goto IL_372;
				}
			}
			else if (!(text == "l"))
			{
				goto IL_372;
			}
			int num3 = world.GetPrimaryPlayerId();
			if (_senderInfo.RemoteClientInfo != null)
			{
				num3 = _senderInfo.RemoteClientInfo.entityId;
			}
			if (num3 != -1)
			{
				AIDirector.DebugToggleSendLatency(num3);
				return;
			}
			return;
		}
		else
		{
			if (!(text == "sendnames"))
			{
				goto IL_372;
			}
			goto IL_359;
		}
		IL_185:
		GameManager.Instance.World.aiDirector.GetComponent<AIDirectorChunkEventComponent>().Clear();
		return;
		IL_359:
		if (_senderInfo.RemoteClientInfo != null)
		{
			AIDirector.DebugToggleSendNameInfo(_senderInfo.RemoteClientInfo.entityId);
			return;
		}
		return;
		IL_372:
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Unknown command " + text + ".");
	}
}

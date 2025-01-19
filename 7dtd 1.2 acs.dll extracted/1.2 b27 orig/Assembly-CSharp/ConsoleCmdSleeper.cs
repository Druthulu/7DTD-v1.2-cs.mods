using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdSleeper : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[]
		{
			"sleeper"
		};
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Drawn or list sleeper info";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "draw - toggle drawing for current player prefab\nlist - list for current player prefab\nlistall - list all\nr - reset all";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count == 0)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(this.GetHelp());
			return;
		}
		string a = _params[0].ToLower();
		if (!(a == "draw"))
		{
			if (a == "listall")
			{
				this.LogInfo(false);
				return;
			}
			if (a == "list")
			{
				this.LogInfo(true);
				return;
			}
			if (!(a == "r"))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Command not recognized. <end/>");
				return;
			}
			this.Reset();
			return;
		}
		else
		{
			if (this.drawVolumesCo != null)
			{
				GameManager.Instance.StopCoroutine(this.drawVolumesCo);
				this.drawVolumesCo = null;
				return;
			}
			this.drawVolumesCo = GameManager.Instance.StartCoroutine(this.DrawVolumes());
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LogInfo(bool onlyPlayer)
	{
		World world = GameManager.Instance.World;
		if (world == null)
		{
			return;
		}
		EntityPlayerLocal entityPlayerLocal = onlyPlayer ? world.GetPrimaryPlayer() : null;
		int sleeperVolumeCount = world.GetSleeperVolumeCount();
		int num = 0;
		int i = 0;
		while (i < sleeperVolumeCount)
		{
			SleeperVolume sleeperVolume = world.GetSleeperVolume(i);
			if (!entityPlayerLocal)
			{
				goto IL_57;
			}
			if (sleeperVolume.PrefabInstance == entityPlayerLocal.prefab)
			{
				sleeperVolume.Draw(3f);
				goto IL_57;
			}
			IL_80:
			i++;
			continue;
			IL_57:
			num++;
			this.Print("#{0} {1}", new object[]
			{
				i,
				sleeperVolume.GetDescription()
			});
			goto IL_80;
		}
		this.Print("Sleeper volumes {0} of {1}", new object[]
		{
			num,
			sleeperVolumeCount
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator DrawVolumes()
	{
		int num;
		for (int i = 0; i < 99999; i = num)
		{
			World world = GameManager.Instance.World;
			if (world == null)
			{
				break;
			}
			EntityPlayerLocal primaryPlayer = world.GetPrimaryPlayer();
			if (!primaryPlayer)
			{
				break;
			}
			int sleeperVolumeCount = world.GetSleeperVolumeCount();
			for (int j = 0; j < sleeperVolumeCount; j++)
			{
				SleeperVolume sleeperVolume = world.GetSleeperVolume(j);
				if (sleeperVolume.PrefabInstance == primaryPlayer.prefab)
				{
					sleeperVolume.DrawDebugLines(1f);
				}
			}
			int triggerVolumeCount = world.GetTriggerVolumeCount();
			for (int k = 0; k < triggerVolumeCount; k++)
			{
				TriggerVolume triggerVolume = world.GetTriggerVolume(k);
				if (triggerVolume.PrefabInstance == primaryPlayer.prefab)
				{
					triggerVolume.DrawDebugLines(1f);
				}
			}
			yield return new WaitForSeconds(0.5f);
			num = i + 1;
		}
		this.drawVolumesCo = null;
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Reset()
	{
		World world = GameManager.Instance.World;
		if (world == null)
		{
			return;
		}
		int sleeperVolumeCount = world.GetSleeperVolumeCount();
		for (int i = 0; i < sleeperVolumeCount; i++)
		{
			SleeperVolume sleeperVolume = world.GetSleeperVolume(i);
			if (sleeperVolume != null)
			{
				sleeperVolume.DespawnAndReset(world);
			}
		}
		this.Print("Reset {0}", new object[]
		{
			sleeperVolumeCount
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Print(string _s, params object[] _values)
	{
		string line = string.Format(_s, _values);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output(line);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Coroutine drawVolumesCo;
}

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdBents : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[]
		{
			"bents"
		};
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Switches block entities on/off";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Use on or off or only the command to toggle";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		GameObject gameObject = GameObject.Find("/Chunks");
		if (gameObject == null)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Parent not found!");
			return;
		}
		if (_params.Count == 0)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Specify on or off");
			return;
		}
		if (_params[0] == "on")
		{
			this.setAll(gameObject.transform, true);
			return;
		}
		if (_params[0] == "off")
		{
			this.setAll(gameObject.transform, false);
			return;
		}
		if (_params[0] == "info")
		{
			this.totalBents = 0;
			this.bentsPerName.Clear();
			this.countAll(gameObject.transform);
			int num = 1;
			foreach (KeyValuePair<string, int> keyValuePair in this.bentsPerName)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Concat(new string[]
				{
					num.ToString(),
					". ",
					keyValuePair.Key,
					" = ",
					keyValuePair.Value.ToString()
				}));
				num++;
			}
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Total: " + this.totalBents.ToString());
			return;
		}
		if (_params[0] == "cullon")
		{
			int num2 = 0;
			MicroStopwatch microStopwatch = new MicroStopwatch();
			foreach (Chunk chunk in GameManager.Instance.World.ChunkCache.GetChunkArray())
			{
				num2 += chunk.EnableInsideBlockEntities(true);
			}
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Setting " + num2.ToString() + " to ON took " + microStopwatch.ElapsedMilliseconds.ToString());
			return;
		}
		if (_params[0] == "culloff")
		{
			int num3 = 0;
			MicroStopwatch microStopwatch2 = new MicroStopwatch();
			foreach (Chunk chunk2 in GameManager.Instance.World.ChunkCache.GetChunkArray())
			{
				num3 += chunk2.EnableInsideBlockEntities(false);
			}
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Setting " + num3.ToString() + " to OFF took " + microStopwatch2.ElapsedMilliseconds.ToString());
			return;
		}
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Unknown parameter");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setAll(Transform _t, bool _bOn)
	{
		for (int i = 0; i < _t.childCount; i++)
		{
			Transform child = _t.GetChild(i);
			if (child.name == "_BlockEntities")
			{
				child.gameObject.SetActive(_bOn);
			}
			else
			{
				this.setAll(child, _bOn);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void countAll(Transform _t)
	{
		for (int i = 0; i < _t.childCount; i++)
		{
			Transform child = _t.GetChild(i);
			if (child.name == "_BlockEntities")
			{
				this.totalBents += child.childCount;
				for (int j = 0; j < child.childCount; j++)
				{
					this.bentsPerName[child.GetChild(j).name] = (this.bentsPerName.ContainsKey(child.GetChild(j).name) ? (this.bentsPerName[child.GetChild(j).name] + 1) : 1);
				}
			}
			else
			{
				this.countAll(child);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int totalBents;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, int> bentsPerName = new Dictionary<string, int>();
}

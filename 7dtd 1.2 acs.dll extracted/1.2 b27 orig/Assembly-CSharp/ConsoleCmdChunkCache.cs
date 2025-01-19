using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdChunkCache : ConsoleCmdAbstract
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
			"chunkcache",
			"cc"
		};
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
		int num = 1;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		ReaderWriterLockSlim syncRoot = GameManager.Instance.World.ChunkClusters[0].GetSyncRoot();
		lock (syncRoot)
		{
			foreach (Chunk chunk in GameManager.Instance.World.ChunkClusters[0].GetChunkArray())
			{
				int usedMem = chunk.GetUsedMem();
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Concat(new string[]
				{
					num++.ToString(),
					". ",
					chunk.X.ToString(),
					", ",
					chunk.Z.ToString(),
					"  M=",
					(usedMem / 1024).ToString(),
					"k",
					chunk.IsDisplayed ? "D" : ""
				}));
				num2 += (chunk.IsDisplayed ? 1 : 0);
				num3 += usedMem;
				num4 += chunk.MeshLayerCount;
			}
		}
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Chunks: " + GameManager.Instance.World.ChunkClusters[0].Count().ToString());
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Chunk Memory: " + (num3 / 1048576).ToString() + "MB");
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Displayed: " + num2.ToString());
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("VML: " + num4.ToString());
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "shows all loaded chunks in cache";
	}
}

using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdRegionReset : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[]
		{
			"regionreset",
			"rr"
		};
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Resets chunks within a target region, or for the entire map.";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Usage: \n'rr' Shorthand for 'rr 0'; reset all unprotected chunks in all regions.\n'rr [mode]' Process all regions, resetting chunks based on the specified mode.\n'rr [x] [z]' Shorthand for 'rr [x] [z] 0'; reset all unprotected chunks in the specified region. E.g. 'rr 1 -2' will only affect region (1,-2).\n'rr [x] [z] [mode]' Process the specified region, resetting chunks based on the specified mode.\n\nModes: \n'0' - Default: All protection statuses are respected, including the dynamic protection of synced chunks around active player position(s).\n'1' - EXPERIMENTAL: Most protection statuses are respected, excepting the dynamic protection of synced chunks around active player position(s). Chunks whose *only* protection status is \"CurrentlySynced\" will be treated as unprotected and are subject to being reset.\n'2' - EXPERIMENTAL: All protection statuses are ignored. Every chunk in the target area will be reset whether protected or not.\n'3' - EXPERIMENTAL: Most protection statuses are ignored, excepting the dynamic protection of synced chunks around active player position(s). Chunks whose protection status includes \"CurrentlySynced\" will be treated as protected; all other chunks are subject to being reset.\n\nNotes: \n - Use with caution! This operation permanently deletes all saved data for affected chunks.\n - The experimental modes are provided for debug purposes only. They bypass various protections in order to force chunks to be reset. This can cause a significant hitch whilst any synced chunks are regenerated, and may cause other side effects such as failing to clean up nav markers for land claims, etc.";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count > 3)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Region reset failed: too many parameters or incorrect parameter format. See 'help rr' for examples.");
		}
		World world = GameManager.Instance.World;
		ChunkCluster chunkCache = world.ChunkCache;
		ChunkProviderGenerateWorld chunkProviderGenerateWorld = chunkCache.ChunkProvider as ChunkProviderGenerateWorld;
		HashSetLong hashSetLong = new HashSetLong();
		HashSetLong hashSetLong2 = new HashSetLong();
		if (chunkProviderGenerateWorld == null)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Region reset failed: ChunkProviderGenerateWorld could not be found for current world instance.");
			return;
		}
		ChunkProtectionLevel chunkProtectionLevel = ChunkProtectionLevel.All;
		if (_params.Count == 1 || _params.Count == 3)
		{
			string text = _params[_params.Count - 1];
			int num;
			if (!int.TryParse(text, out num))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Region reset failed: unexpected mode input '" + text + "'.");
				return;
			}
			switch (num)
			{
			case 0:
				chunkProtectionLevel = ChunkProtectionLevel.All;
				break;
			case 1:
				chunkProtectionLevel = ~ChunkProtectionLevel.CurrentlySynced;
				break;
			case 2:
				chunkProtectionLevel = ChunkProtectionLevel.None;
				break;
			case 3:
				chunkProtectionLevel = ChunkProtectionLevel.CurrentlySynced;
				break;
			default:
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Region reset failed: '{0}' is not a supported mode value.", num));
				return;
			}
		}
		HashSetLong hashSetLong3;
		if (_params.Count < 2)
		{
			hashSetLong3 = chunkProviderGenerateWorld.ResetAllChunks(chunkProtectionLevel);
		}
		else
		{
			int regionX;
			int regionZ;
			if (!int.TryParse(_params[0], out regionX) || !int.TryParse(_params[1], out regionZ))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Region reset failed: invalid region coordinates.");
				return;
			}
			hashSetLong3 = chunkProviderGenerateWorld.ResetRegion(regionX, regionZ, chunkProtectionLevel);
		}
		if ((chunkProtectionLevel & ChunkProtectionLevel.CurrentlySynced) == ChunkProtectionLevel.None)
		{
			foreach (long num2 in hashSetLong3)
			{
				if (chunkCache.ContainsChunkSync(num2))
				{
					hashSetLong.Add(num2);
				}
			}
			if (hashSetLong.Count > 0)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Regenerating {0} synced chunks.", hashSetLong.Count));
				foreach (long num3 in hashSetLong)
				{
					if (!chunkProviderGenerateWorld.GenerateSingleChunk(chunkCache, num3, true))
					{
						SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Region reset failed regenerating chunk at world XZ position: {0}, {1}", WorldChunkCache.extractX(num3) << 4, WorldChunkCache.extractZ(num3) << 4));
					}
					else
					{
						hashSetLong2.Add(num3);
					}
				}
				world.m_ChunkManager.ResendChunksToClients(hashSetLong2);
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Regeneration complete.");
			}
		}
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Region reset complete. Reset {0} chunks.", hashSetLong3.Count));
	}
}

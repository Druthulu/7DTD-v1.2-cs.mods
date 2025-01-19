using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdChunkReset : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[]
		{
			"chunkreset",
			"cr"
		};
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "resets the specified chunks";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Usage:\n  1. chunkreset <x1> <z1> <x2> <z2>\n  2. chunkreset [f]\n1. Rebuilds the chunks that contain the given coordinate range.\n2. Can only be executed by a player in the ingame console! Behaviour depends on whether the\n   player is currently within the bounds of a POI:\n   Within a POI: The POI is reset.\n   Not within a POI: The chunk the player is in and the eight chunks around that one are\n     rebuilt. Not deco! Does not reload POI data!\n   d - regen deco\n   f - fully regenerates chunks (may cause double entities!)\n   u, utimed, ue - Unload chunks or entities\n   nq - Enqueue a 3x3 group of chunks centred around the player to be reset when unsynced, unless they are otherwise protected from the chunk reset system.";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		GameManager.Instance.StartCoroutine(this.execute(_params, _senderInfo));
	}

	public IEnumerator execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		World world = GameManager.Instance.World;
		if (_params.Count >= 2)
		{
			int x;
			if (!int.TryParse(_params[0], out x))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("x1 is not a valid integer");
				yield break;
			}
			int z;
			if (!int.TryParse(_params[1], out z))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("z1 is not a valid integer");
				yield break;
			}
			int x2 = x;
			int z2 = z;
			if (_params.Count >= 3 && !int.TryParse(_params[2], out x2))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("x2 is not a valid integer");
				yield break;
			}
			if (_params.Count >= 4 && !int.TryParse(_params[3], out z2))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("z2 is not a valid integer");
				yield break;
			}
			Vector2i chunkMin = new Vector2i((x <= x2) ? x : x2, (z <= z2) ? z : z2);
			Vector2i chunkMax = new Vector2i((x <= x2) ? x2 : x, (z <= z2) ? z2 : z);
			if (chunkMax.x - chunkMin.x > 16384 || chunkMax.y - chunkMin.y > 16384)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("area too big");
				yield break;
			}
			chunkMin = World.toChunkXZ(chunkMin);
			chunkMax = World.toChunkXZ(chunkMax);
			HashSetLong chunks = new HashSetLong();
			for (int i = chunkMin.x; i <= chunkMax.x; i++)
			{
				for (int j = chunkMin.y; j <= chunkMax.y; j++)
				{
					chunks.Add(WorldChunkCache.MakeChunkKey(i, j));
				}
			}
			ChunkCluster cc = world.ChunkCache;
			ChunkProviderGenerateWorld chunkProvider = world.ChunkCache.ChunkProvider as ChunkProviderGenerateWorld;
			if (chunkProvider != null)
			{
				yield return GameManager.Instance.ResetWindowsAndLocks(chunks);
				chunkProvider.RemoveChunks(chunks);
				foreach (long key in chunks)
				{
					if (!chunkProvider.GenerateSingleChunk(cc, key, true))
					{
						SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Failed regenerating chunk at position {0}/{1}", WorldChunkCache.extractX(key) << 4, WorldChunkCache.extractZ(key) << 4));
					}
				}
				GameManager.Instance.World.m_ChunkManager.ResendChunksToClients(chunks);
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Reset chunks covering area {0}/{1} to {2}/{3} (chunk coordinates {4} to {5}).", new object[]
				{
					x,
					z,
					x2,
					z2,
					chunkMin,
					chunkMax
				}));
				if (!(DynamicMeshManager.Instance != null))
				{
					goto IL_44F;
				}
				using (HashSetLong.Enumerator enumerator = chunks.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						long key2 = enumerator.Current;
						DynamicMeshManager.Instance.AddChunk(key2, true, true, null);
					}
					goto IL_44F;
				}
			}
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Can not reset chunks on this game");
			IL_44F:
			chunks = null;
			cc = null;
			chunkProvider = null;
		}
		else
		{
			if (_params.Count > 1 || (_senderInfo.RemoteClientInfo == null && (!_senderInfo.IsLocalGame || GameManager.IsDedicatedServer)))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Invalid arguments, please see command help.");
				yield break;
			}
			Vector3 position;
			if (_senderInfo.RemoteClientInfo != null)
			{
				position = world.Players.dict[_senderInfo.RemoteClientInfo.entityId].position;
			}
			else
			{
				position = world.GetLocalPlayers()[0].position;
			}
			int x = World.toChunkXZ((int)position.x) - 1;
			int z = World.toChunkXZ((int)position.z) - 1;
			int x2 = x + 2;
			int z2 = z + 2;
			HashSetLong chunks = new HashSetLong();
			for (int k = x; k <= x2; k++)
			{
				for (int l = z; l <= z2; l++)
				{
					chunks.Add(WorldChunkCache.MakeChunkKey(k, l));
				}
			}
			if (_params.Count == 1)
			{
				ChunkCluster cc = world.ChunkCache;
				ChunkProviderGenerateWorld chunkProvider = cc.ChunkProvider as ChunkProviderGenerateWorld;
				if (chunkProvider == null)
				{
					yield break;
				}
				if (_params[0] == "nq")
				{
					foreach (long chunkKey in chunks)
					{
						chunkProvider.RequestChunkReset(chunkKey);
					}
					yield break;
				}
				if (_params[0] == "d")
				{
					yield return GameManager.Instance.ResetWindowsAndLocks(chunks);
					foreach (long key3 in chunks)
					{
						Chunk chunkSync = cc.GetChunkSync(key3);
						if (chunkSync != null)
						{
							chunkSync.NeedsLightDecoration = true;
							chunkSync.NeedsLightCalculation = true;
						}
					}
					GameManager.Instance.World.m_ChunkManager.ResendChunksToClients(chunks);
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Generate deco around player");
				}
				else if (_params[0] == "f")
				{
					yield return GameManager.Instance.ResetWindowsAndLocks(chunks);
					foreach (long key4 in chunks)
					{
						if (!chunkProvider.GenerateSingleChunk(cc, key4, true))
						{
							SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Failed regenerating chunk at position {0}/{1}", WorldChunkCache.extractX(key4) << 4, WorldChunkCache.extractZ(key4) << 4));
						}
					}
					GameManager.Instance.World.m_ChunkManager.ResendChunksToClients(chunks);
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Generate chunks around player");
				}
				else
				{
					if (_params[0] == "r")
					{
						using (HashSetLong.Enumerator enumerator = chunks.GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
								long key5 = enumerator.Current;
								Chunk chunkSync2 = cc.GetChunkSync(key5);
								if (chunkSync2 != null)
								{
									chunkSync2.NeedsRegeneration = true;
								}
							}
							goto IL_A20;
						}
					}
					if (_params[0] == "u")
					{
						foreach (long chunkKey2 in chunks)
						{
							world.m_ChunkManager.RemoveChunk(chunkKey2);
						}
						SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Unload around player");
					}
					else if (_params[0] == "utimed")
					{
						if (this.unloadTimed != null)
						{
							GameManager.Instance.StopCoroutine(this.unloadTimed);
							this.unloadTimed = null;
						}
						else
						{
							this.unloadTimed = GameManager.Instance.StartCoroutine(this.UnloadTimed());
						}
						SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Unload timed at player {0}", new object[]
						{
							this.unloadTimed != null
						});
					}
					else if (_params[0] == "ue")
					{
						foreach (long key6 in chunks)
						{
							Chunk chunkSync3 = cc.GetChunkSync(key6);
							if (chunkSync3 != null)
							{
								for (int m = 0; m < chunkSync3.entityLists.Length; m++)
								{
									List<Entity> list = chunkSync3.entityLists[m];
									for (int n = list.Count - 1; n >= 0; n--)
									{
										Entity entity = list[n];
										if (!entity.bWillRespawn && (!(entity.AttachedMainEntity != null) || !entity.AttachedMainEntity.bWillRespawn))
										{
											world.unloadEntity(entity, EnumRemoveEntityReason.Unloaded);
											list.RemoveAt(n);
										}
									}
								}
							}
						}
						SingletonMonoBehaviour<SdtdConsole>.Instance.Output("UnloadEntities around player");
					}
				}
				IL_A20:
				cc = null;
				chunkProvider = null;
			}
			else
			{
				DynamicPrefabDecorator dynamicPrefabDecorator = GameManager.Instance.GetDynamicPrefabDecorator();
				List<PrefabInstance> prefabsFromWorldPosInside;
				if (dynamicPrefabDecorator != null && (prefabsFromWorldPosInside = dynamicPrefabDecorator.GetPrefabsFromWorldPosInside(position, FastTags<TagGroup.Global>.none)) != null)
				{
					yield return world.ResetPOIS(prefabsFromWorldPosInside, QuestEventManager.manualResetTag, -1, null, null);
				}
				else
				{
					yield return GameManager.Instance.ResetWindowsAndLocks(chunks);
					world.RebuildTerrain(chunks, Vector3i.zero, Vector3i.zero, false, true, true, false);
					GameManager.Instance.World.m_ChunkManager.ResendChunksToClients(chunks);
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Reset chunks around player");
				}
			}
			if (DynamicMeshManager.Instance != null)
			{
				foreach (long key7 in chunks)
				{
					DynamicMeshManager.Instance.AddChunk(key7, true, true, null);
				}
			}
			chunks = null;
		}
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator UnloadTimed()
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
			Vector3i blockPosition = primaryPlayer.GetBlockPosition();
			for (int j = -16; j <= 16; j += 16)
			{
				for (int k = -16; k <= 16; k += 16)
				{
					int x = World.toChunkXZ(blockPosition.x + k);
					int y = World.toChunkXZ(blockPosition.z + j);
					long chunkKey = WorldChunkCache.MakeChunkKey(x, y);
					world.m_ChunkManager.RemoveChunk(chunkKey);
				}
			}
			yield return new WaitForSeconds(1.5f);
			num = i + 1;
		}
		this.unloadTimed = null;
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Coroutine unloadTimed;
}

﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ChunkProviderGenerateFlat : ChunkProviderAbstract
{
	public ChunkProviderGenerateFlat(ChunkCluster _cc, string _worldName)
	{
		this.cc = _cc;
		this.worldName = _worldName;
	}

	public override IEnumerator Init(World _world)
	{
		this.world = _world;
		GamePrefs.OnGamePrefChanged += this.GamePrefChanged;
		bool flag = false;
		bool createGroundTerrain = false;
		bool createQuestContainerAndTrader = false;
		Prefab prefabToPlace = null;
		Vector3i playerPos = Vector3i.zero;
		if (GameUtils.IsPlaytesting())
		{
			flag = true;
			createGroundTerrain = true;
			createQuestContainerAndTrader = true;
			prefabToPlace = new Prefab();
			prefabToPlace.Load(GamePrefs.GetString(EnumGamePrefs.GameName), true, true, false, false);
			playerPos = new Vector3i(1f, WorldConstants.WaterLevel, (float)(-(float)prefabToPlace.size.z / 2 - 4 - 10));
		}
		MultiBlockManager.Instance.Initialize(null);
		this.prefabDecorator = new DynamicPrefabDecorator();
		if (flag)
		{
			if (SdDirectory.Exists(GameIO.GetSaveGameRegionDir()))
			{
				SdDirectory.Delete(GameIO.GetSaveGameRegionDir(), true);
			}
			if (SdFile.Exists(GameIO.GetSaveGameDir() + "/decoration.7dt"))
			{
				SdFile.Delete(GameIO.GetSaveGameDir() + "/decoration.7dt");
			}
		}
		bool flag2 = _world.IsEditor() || !SdDirectory.Exists(GameIO.GetSaveGameRegionDir());
		this.cc.IsFixedSize = true;
		PathAbstractions.AbstractedLocation worldLocation = PathAbstractions.WorldsSearchPaths.GetLocation(this.worldName, null, null);
		if (flag2)
		{
			yield return this.prefabDecorator.Load(worldLocation.FullPath);
			this.prefabDecorator.CopyAllPrefabsIntoWorld(this.world, false);
		}
		this.spawnPointManager = new SpawnPointManager(true);
		this.spawnPointManager.Load(worldLocation.FullPath);
		BoundsInt boundsInt;
		if (prefabToPlace != null)
		{
			int num = -1 * prefabToPlace.size.x / 2;
			int num2 = -1 * prefabToPlace.size.z / 2;
			int num3 = num + prefabToPlace.size.x;
			int num4 = num2 + prefabToPlace.size.z;
			Vector3Int vector3Int = new Vector3Int((num - 1) / 16 - 1, 0, (num2 - 1) / 16 - 1);
			vector3Int -= new Vector3Int(ChunkProviderGenerateFlat.prefabChunkBorderSize, 0, ChunkProviderGenerateFlat.prefabChunkBorderSize + 3);
			Vector3Int a = new Vector3Int((num3 + 1) / 16 + 1, 0, (num4 + 1) / 16 + 1);
			a += new Vector3Int(ChunkProviderGenerateFlat.prefabChunkBorderSize, 0, ChunkProviderGenerateFlat.prefabChunkBorderSize);
			boundsInt = new BoundsInt(vector3Int, a - vector3Int);
		}
		else
		{
			boundsInt = new BoundsInt(new Vector3Int(-ChunkProviderGenerateFlat.defaultWorldAreaChunkDim / 2, 0, -ChunkProviderGenerateFlat.defaultWorldAreaChunkDim / 2), new Vector3Int(ChunkProviderGenerateFlat.defaultWorldAreaChunkDim, 0, ChunkProviderGenerateFlat.defaultWorldAreaChunkDim));
		}
		this.world.m_ChunkManager.RemoveAllChunks();
		BiomeDefinition.BiomeType biomeType = (BiomeDefinition.BiomeType)GamePrefs.GetInt(EnumGamePrefs.PlaytestBiome);
		if (biomeType <= BiomeDefinition.BiomeType.Any || biomeType > BiomeDefinition.BiomeType.burnt_forest)
		{
			biomeType = BiomeDefinition.BiomeType.PineForest;
		}
		byte biomeId = (byte)biomeType;
		for (int i = boundsInt.xMin; i <= boundsInt.xMax; i++)
		{
			for (int j = boundsInt.zMin; j <= boundsInt.zMax; j++)
			{
				Chunk chunk = MemoryPools.PoolChunks.AllocSync(true);
				chunk.X = i;
				chunk.Z = j;
				chunk.FillBiomeId(biomeId);
				if (createGroundTerrain)
				{
					chunk.FillBlockRaw((int)WorldConstants.WaterLevel - 1, Block.GetBlockValue("terrainFiller", false));
				}
				chunk.SetFullSunlight();
				chunk.NeedsLightCalculation = false;
				chunk.NeedsDecoration = false;
				chunk.NeedsRegeneration = false;
				this.cc.AddChunkSync(chunk, true);
			}
		}
		List<Chunk> chunkArrayCopySync = this.world.ChunkClusters[0].GetChunkArrayCopySync();
		if (prefabToPlace != null)
		{
			Vector3i position = new Vector3i((float)(-(float)prefabToPlace.size.x / 2), WorldConstants.WaterLevel + (float)prefabToPlace.yOffset, (float)(-(float)prefabToPlace.size.z / 2));
			PrefabInstance pi = new PrefabInstance(1, prefabToPlace.location, position, 0, prefabToPlace, 0);
			this.prefabDecorator.AddPrefab(pi, true);
			this.prefabDecorator.CopyAllPrefabsIntoWorld(this.world, false);
			foreach (Chunk c in chunkArrayCopySync)
			{
				WaterSimulationNative.Instance.InitializeChunk(c);
			}
		}
		if (playerPos == Vector3i.zero)
		{
			playerPos = new Vector3i(0f, createGroundTerrain ? (WorldConstants.WaterLevel + 1f) : 16f, -10f);
		}
		if (createQuestContainerAndTrader)
		{
			this.world.SetBlock(0, playerPos + new Vector3i(1, 0, 3), Block.GetBlockValue("cntQuestTestLoot", false), false, false);
			Vector3i vector3i = playerPos + new Vector3i(-3, 0, 3);
			if (this.world.GetChunkFromWorldPos(vector3i) != null)
			{
				_world.SpawnEntityInWorld(EntityFactory.CreateEntity(new EntityCreationData
				{
					pos = vector3i,
					rot = new Vector3(0f, 180f, 0f),
					entityClass = EntityClass.FromString("npcTraderTest")
				}));
			}
		}
		foreach (Chunk chunk2 in chunkArrayCopySync)
		{
			chunk2.ResetStability();
			chunk2.ResetLights(0);
			chunk2.RefreshSunlight();
			chunk2.NeedsLightCalculation = true;
		}
		this.spawnPointManager.spawnPointList = new SpawnPointList
		{
			new SpawnPoint(playerPos)
		};
		yield return null;
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GamePrefChanged(EnumGamePrefs _pref)
	{
		if (_pref == EnumGamePrefs.PlaytestBiome)
		{
			BiomeDefinition.BiomeType biomeType = (BiomeDefinition.BiomeType)GamePrefs.GetInt(EnumGamePrefs.PlaytestBiome);
			if (biomeType <= BiomeDefinition.BiomeType.Any || biomeType > BiomeDefinition.BiomeType.Wasteland)
			{
				biomeType = BiomeDefinition.BiomeType.PineForest;
			}
			byte biomeId = (byte)biomeType;
			foreach (Chunk chunk in this.world.ChunkClusters[0].GetChunkArrayCopySync())
			{
				chunk.FillBiomeId(biomeId);
				chunk.ResetStability();
				chunk.ResetLights(0);
				chunk.RefreshSunlight();
				chunk.NeedsLightCalculation = true;
			}
		}
	}

	public override DynamicPrefabDecorator GetDynamicPrefabDecorator()
	{
		return this.prefabDecorator;
	}

	public override SpawnPointList GetSpawnPointList()
	{
		return this.spawnPointManager.spawnPointList;
	}

	public override void SetSpawnPointList(SpawnPointList _spawnPointList)
	{
		this.spawnPointManager.spawnPointList = _spawnPointList;
	}

	public override bool GetOverviewMap(Vector2i _startPos, Vector2i _size, Color[] _mapColors)
	{
		return false;
	}

	public override EnumChunkProviderId GetProviderId()
	{
		return EnumChunkProviderId.FlatWorld;
	}

	public override void Cleanup()
	{
		GamePrefs.OnGamePrefChanged -= this.GamePrefChanged;
		Thread.Sleep(200);
		MultiBlockManager.Instance.Cleanup();
	}

	public override bool GetWorldExtent(out Vector3i _minSize, out Vector3i _maxSize)
	{
		_minSize = new Vector3i(this.world.ChunkCache.ChunkMinPos.x * 16, 0, this.world.ChunkCache.ChunkMinPos.y * 16);
		_maxSize = new Vector3i(this.world.ChunkCache.ChunkMaxPos.x * 16, 255, this.world.ChunkCache.ChunkMaxPos.y * 16);
		return true;
	}

	public override void RebuildTerrain(HashSetLong _chunks, Vector3i _areaStart, Vector3i _areaSize, bool _bStopStabilityUpdate, bool _bRegenerateChunk, bool _bFillEmptyBlocks, bool _isReset)
	{
		if (_bStopStabilityUpdate)
		{
			foreach (long key in _chunks)
			{
				Chunk chunkSync = this.cc.GetChunkSync(key);
				if (chunkSync != null)
				{
					chunkSync.StopStabilityCalculation = true;
				}
			}
		}
		PathAbstractions.AbstractedLocation location = PathAbstractions.WorldsSearchPaths.GetLocation(this.worldName, null, null);
		ThreadManager.RunCoroutineSync(this.prefabDecorator.Load(location.FullPath));
		this.prefabDecorator.CopyAllPrefabsIntoWorld(this.world, true);
		Chunk[] neighbours = new Chunk[8];
		foreach (Chunk chunk in this.world.ChunkClusters[0].GetChunkArrayCopySync())
		{
			chunk.ResetStability();
			chunk.ResetLights(0);
			chunk.RefreshSunlight();
			if (this.cc.GetNeighborChunks(chunk, neighbours))
			{
				this.cc.LightChunk(chunk, neighbours);
			}
			List<TileEntity> list = chunk.GetTileEntities().list;
			for (int i = 0; i < list.Count; i++)
			{
				list[i].Reset(FastTags<TagGroup.Global>.none);
			}
		}
		List<EntityPlayerLocal> localPlayers = this.world.GetLocalPlayers();
		foreach (long num in _chunks)
		{
			Chunk chunkSync2 = this.cc.GetChunkSync(num);
			if (chunkSync2 != null)
			{
				for (int j = 0; j < this.world.m_ChunkManager.m_ObservedEntities.Count; j++)
				{
					if (!this.world.m_ChunkManager.m_ObservedEntities[j].bBuildVisualMeshAround)
					{
						bool flag = false;
						int num2 = 0;
						while (!flag && num2 < localPlayers.Count)
						{
							if (this.world.m_ChunkManager.m_ObservedEntities[j].entityIdToSendChunksTo == localPlayers[num2].entityId)
							{
								flag = true;
							}
							num2++;
						}
						if (!flag && this.world.m_ChunkManager.m_ObservedEntities[j].chunksLoaded.Contains(num))
						{
							SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageChunk>().Setup(chunkSync2, true), false, this.world.m_ChunkManager.m_ObservedEntities[j].entityIdToSendChunksTo, -1, -1, null, 192);
						}
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly int defaultWorldAreaChunkDim = 32;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly int prefabChunkBorderSize = 3;

	[PublicizedFrom(EAccessModifier.Private)]
	public World world;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly string worldName;

	[PublicizedFrom(EAccessModifier.Private)]
	public DynamicPrefabDecorator prefabDecorator;

	[PublicizedFrom(EAccessModifier.Private)]
	public IChunkProviderIndicator chunkManager;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ChunkCluster cc;

	[PublicizedFrom(EAccessModifier.Private)]
	public SpawnPointManager spawnPointManager;
}

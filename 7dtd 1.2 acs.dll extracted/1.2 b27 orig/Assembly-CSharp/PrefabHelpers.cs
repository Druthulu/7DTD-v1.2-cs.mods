﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class PrefabHelpers
{
	public static Coroutine IteratePrefabs(bool _ignoreExcludeImposterCheck = false, Action<Prefab> _onPrefabLoaded = null, Action<PathAbstractions.AbstractedLocation, Prefab> _onChunksBuilt = null, Func<PathAbstractions.AbstractedLocation, bool> _prefabPathFilter = null, Func<Prefab, bool> _prefabContentFilter = null, Action _cleanupAction = null)
	{
		return ThreadManager.StartCoroutine(PrefabHelpers.runBulk(_ignoreExcludeImposterCheck, _onPrefabLoaded, _onChunksBuilt, _prefabPathFilter, _prefabContentFilter, _cleanupAction));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator runBulk(bool _ignoreExcludeImposterCheck, Action<Prefab> _onPrefabLoaded, Action<PathAbstractions.AbstractedLocation, Prefab> _onChunksBuilt, Func<PathAbstractions.AbstractedLocation, bool> _prefabPathFilter, Func<Prefab, bool> _prefabContentFilter, Action _cleanupAction)
	{
		foreach (PathAbstractions.AbstractedLocation abstractedLocation in PathAbstractions.PrefabsSearchPaths.GetAvailablePathsList(null, null, null, false))
		{
			if ((_prefabPathFilter == null || _prefabPathFilter(abstractedLocation)) && GameManager.Instance.prefabEditModeManager.LoadVoxelPrefab(abstractedLocation, true, _ignoreExcludeImposterCheck) && (_prefabContentFilter == null || _prefabContentFilter(PrefabEditModeManager.Instance.VoxelPrefab)))
			{
				yield return PrefabHelpers.processPrefab(_onPrefabLoaded, _onChunksBuilt, PrefabEditModeManager.Instance.VoxelPrefab, abstractedLocation);
			}
		}
		List<PathAbstractions.AbstractedLocation>.Enumerator enumerator = default(List<PathAbstractions.AbstractedLocation>.Enumerator);
		if (_cleanupAction != null)
		{
			_cleanupAction();
		}
		Log.Out("Processing prefabs done");
		yield break;
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator processPrefab(Action<Prefab> _onPrefabLoaded, Action<PathAbstractions.AbstractedLocation, Prefab> _onChunksBuilt, Prefab _prefab, PathAbstractions.AbstractedLocation _path)
	{
		Log.Out("Processing " + _path.Name);
		if (_onPrefabLoaded != null)
		{
			_onPrefabLoaded(_prefab);
		}
		ChunkCluster cc = GameManager.Instance.World.ChunkCache;
		List<Chunk> chunkArrayCopySync = cc.GetChunkArrayCopySync();
		foreach (Chunk c in chunkArrayCopySync)
		{
			if (!cc.IsOnBorder(c))
			{
				if (!c.IsEmpty())
				{
					while (c.NeedsRegeneration || c.NeedsCopying)
					{
						yield return null;
					}
					c = null;
				}
			}
		}
		List<Chunk>.Enumerator enumerator = default(List<Chunk>.Enumerator);
		if (_onChunksBuilt != null)
		{
			_onChunksBuilt(_path, _prefab);
		}
		yield break;
		yield break;
	}

	public static void Cleanup()
	{
		PrefabHelpers.cInnerBlockBVReplace = BlockValue.Air;
		BlockShapeNew.bImposterGenerationActive = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void Init()
	{
		PrefabHelpers.cInnerBlockBVReplace = Block.GetBlockValue(PrefabHelpers.cInnerBlockReplace, false);
	}

	public static void convert(Action _callbackOnDone = null)
	{
		PrefabHelpers.SimplifyPrefab(false);
		ThreadManager.StartCoroutine(PrefabHelpers.convertWaitForAllChunksBuilt(_callbackOnDone));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator convertWaitForAllChunksBuilt(Action _callbackOnDone)
	{
		Prefab prefab = PrefabEditModeManager.Instance.VoxelPrefab;
		if (prefab != null)
		{
			ChunkCluster cc = GameManager.Instance.World.ChunkCache;
			List<Chunk> chunkArrayCopySync = cc.GetChunkArrayCopySync();
			foreach (Chunk c in chunkArrayCopySync)
			{
				if (!cc.IsOnBorder(c))
				{
					if (!c.IsEmpty())
					{
						while (c.NeedsRegeneration || c.NeedsCopying)
						{
							yield return new WaitForEndOfFrame();
						}
						c = null;
					}
				}
			}
			List<Chunk>.Enumerator enumerator = default(List<Chunk>.Enumerator);
			yield return new WaitForEndOfFrame();
			if (PrefabHelpers.combine(true))
			{
				PrefabHelpers.export();
				UnityEngine.Object.Destroy(PrefabEditModeManager.Instance.ImposterPrefab);
				bool bTextureArray = MeshDescription.meshes[0].bTextureArray;
				PrefabEditModeManager.Instance.ImposterPrefab = SimpleMeshFile.ReadGameObject(prefab.location.FullPathNoExtension + ".mesh", 0f, null, bTextureArray, false, null, null);
				PrefabEditModeManager.Instance.ImposterPrefab.transform.name = prefab.PrefabName;
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Done " + prefab.location.ToString());
			}
			else
			{
				Log.Out("Skipping " + prefab.location.ToString());
			}
			cc = null;
		}
		if (_callbackOnDone != null)
		{
			_callbackOnDone();
		}
		yield break;
		yield break;
	}

	public static void convertInsideOutside(Action _callbackOnDone = null)
	{
		ThreadManager.StartCoroutine(PrefabHelpers.convertInsideOutsideWaitForAllChunksBuilt(_callbackOnDone));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator convertInsideOutsideWaitForAllChunksBuilt(Action _callbackOnDone)
	{
		ChunkCluster cc = GameManager.Instance.World.ChunkCache;
		List<Chunk> chunkArrayCopySync = cc.GetChunkArrayCopySync();
		foreach (Chunk c in chunkArrayCopySync)
		{
			if (!cc.IsOnBorder(c))
			{
				if (!c.IsEmpty())
				{
					while (c.NeedsRegeneration || c.NeedsCopying)
					{
						yield return new WaitForSeconds(1f);
					}
					c = null;
				}
			}
		}
		List<Chunk>.Enumerator enumerator = default(List<Chunk>.Enumerator);
		GameManager.Instance.prefabEditModeManager.SaveVoxelPrefab();
		GameManager.Instance.prefabEditModeManager.ClearVoxelPrefab();
		if (_callbackOnDone != null)
		{
			_callbackOnDone();
		}
		yield break;
		yield break;
	}

	public static void SimplifyPrefab(bool _bOnlySimplify1 = false)
	{
		Prefab voxelPrefab = PrefabEditModeManager.Instance.VoxelPrefab;
		if (voxelPrefab == null)
		{
			return;
		}
		PrefabHelpers.Init();
		new MicroStopwatch();
		MicroStopwatch microStopwatch = new MicroStopwatch();
		World world = GameManager.Instance.World;
		ChunkCluster chunkCache = world.ChunkCache;
		List<Chunk> chunkArrayCopySync = chunkCache.GetChunkArrayCopySync();
		PrefabHelpers.dim.x = (chunkCache.ChunkMaxPos.x - chunkCache.ChunkMinPos.x + 1) * 16;
		PrefabHelpers.dim.z = (chunkCache.ChunkMaxPos.y - chunkCache.ChunkMinPos.y + 1) * 16;
		PrefabHelpers.dim.y = 0;
		foreach (Chunk chunk in chunkArrayCopySync)
		{
			PrefabHelpers.dim.y = Utils.FastMax(PrefabHelpers.dim.y, (int)chunk.GetMaxHeight());
		}
		PrefabHelpers.dim.y = PrefabHelpers.dim.y + 1;
		int num = PrefabHelpers.dim.x * PrefabHelpers.dim.y * PrefabHelpers.dim.z;
		PrefabHelpers.eInsideOutside = new EnumInsideOutside[num];
		PrefabHelpers.bTouched = new bool[num];
		PrefabHelpers.blockIds = new BlockValue[num];
		PrefabHelpers.textureIdxOverride = new byte[num];
		int num2 = -chunkCache.ChunkMinPos.x * 16;
		int num3 = -chunkCache.ChunkMinPos.y * 16;
		foreach (Chunk chunk2 in chunkArrayCopySync)
		{
			PrefabHelpers.<>c__DisplayClass16_0 CS$<>8__locals1 = new PrefabHelpers.<>c__DisplayClass16_0();
			CS$<>8__locals1.pos = chunk2.ToWorldPos();
			PrefabHelpers.<>c__DisplayClass16_0 CS$<>8__locals2 = CS$<>8__locals1;
			CS$<>8__locals2.pos.x = CS$<>8__locals2.pos.x + num2;
			PrefabHelpers.<>c__DisplayClass16_0 CS$<>8__locals3 = CS$<>8__locals1;
			CS$<>8__locals3.pos.z = CS$<>8__locals3.pos.z + num3;
			chunk2.LoopOverAllBlocks(delegate(int x, int y, int z, BlockValue bv)
			{
				if (y < PrefabHelpers.dim.y)
				{
					PrefabHelpers.blockIds[x + CS$<>8__locals1.pos.x + y * PrefabHelpers.dim.x + (z + CS$<>8__locals1.pos.z) * PrefabHelpers.dim.x * PrefabHelpers.dim.y] = bv;
				}
			}, true, false);
			chunk2.ClearWater();
		}
		PrefabHelpers.simplifyPrefab1();
		if (!voxelPrefab.bExcludePOICulling)
		{
			PrefabHelpers.insideOutsidePrefab();
		}
		else
		{
			for (int i = 0; i < PrefabHelpers.eInsideOutside.Length; i++)
			{
				PrefabHelpers.eInsideOutside[i] = EnumInsideOutside.Outside;
			}
		}
		if (!_bOnlySimplify1)
		{
			PrefabHelpers.simplifyPrefab2();
		}
		foreach (Chunk chunk3 in chunkArrayCopySync)
		{
			Vector3i vector3i = chunk3.ToWorldPos();
			for (int j = PrefabHelpers.dim.y - 1; j >= 0; j--)
			{
				for (int k = 0; k < 16; k++)
				{
					int num4 = (vector3i.y + j) * PrefabHelpers.dim.x + (vector3i.z + k + num3) * PrefabHelpers.dim.x * PrefabHelpers.dim.y;
					for (int l = 0; l < 16; l++)
					{
						int num5 = vector3i.x + l + num2 + num4;
						if (PrefabHelpers.eInsideOutside[num5] == EnumInsideOutside.Inside)
						{
							chunk3.SetBlock(world, l, j, k, PrefabHelpers.cInnerBlockBVReplace, true, true, false, false, -1);
							chunk3.SetDensity(l, j, k, MarchingCubes.DensityAir);
						}
						else
						{
							chunk3.SetDensity(l, j, k, MarchingCubes.DensityAir);
							if (PrefabHelpers.bTouched[num5])
							{
								chunk3.SetBlock(world, l, j, k, PrefabHelpers.blockIds[num5], true, true, false, false, -1);
								long num6 = (long)((ulong)PrefabHelpers.textureIdxOverride[num5]);
								if (num6 != 0L && chunk3.GetTextureFull(l, j, k) == 0L)
								{
									long texturefull = num6 | num6 << 8 | num6 << 16 | num6 << 24 | num6 << 32 | num6 << 40;
									chunk3.SetTextureFull(l, j, k, texturefull);
								}
							}
						}
					}
				}
			}
		}
		Log.Out("SimplifyPrefab {0}, time {1}", new object[]
		{
			PrefabHelpers.dim,
			microStopwatch.ElapsedMilliseconds
		});
		PrefabHelpers.rebuildMesh();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void simplifyPrefab1()
	{
		bool flag = PrefabEditModeManager.Instance.VoxelPrefab.PrefabName.Contains("rwg_tile");
		int num = -(PrefabEditModeManager.Instance.VoxelPrefab.yOffset - 1);
		for (int i = 0; i < PrefabHelpers.blockIds.Length; i++)
		{
			BlockValue blockValue = PrefabHelpers.blockIds[i];
			if (!blockValue.isair)
			{
				Block block = blockValue.Block;
				if ((i / PrefabHelpers.dim.x % PrefabHelpers.dim.y < num && !flag) || block.bImposterExclude || block.shape is BlockShapeDistantDecoTree || (block.IsTerrainDecoration && block.ImposterExchange == 0))
				{
					PrefabHelpers.blockIds[i] = BlockValue.Air;
					PrefabHelpers.bTouched[i] = true;
				}
				else
				{
					if (block.ImposterExchange != 0)
					{
						byte rotation = blockValue.rotation;
						if (blockValue.ischild)
						{
							int num2 = blockValue.parentx + blockValue.parenty * PrefabHelpers.dim.x + blockValue.parentz * PrefabHelpers.dim.x * PrefabHelpers.dim.y;
							rotation = PrefabHelpers.blockIds[i + num2].rotation;
						}
						PrefabHelpers.blockIds[i] = new BlockValue((uint)block.ImposterExchange);
						PrefabHelpers.blockIds[i].rotation = rotation;
						PrefabHelpers.bTouched[i] = true;
						PrefabHelpers.textureIdxOverride[i] = block.ImposterExchangeTexIdx;
					}
					if (PrefabHelpers.blockIds[i].Block.MeshIndex == 5)
					{
						PrefabHelpers.blockIds[i] = BlockValue.Air;
						PrefabHelpers.bTouched[i] = true;
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void simplifyPrefab2()
	{
		for (int i = 0; i < PrefabHelpers.blockIds.Length; i++)
		{
			BlockValue blockValue = PrefabHelpers.blockIds[i];
			if (!blockValue.isair)
			{
				Block block = blockValue.Block;
				if (block.shape is BlockShapeModelEntity || block.shape is BlockShapeExt3dModel || block.MeshIndex != 0 || block.bImposterExcludeAndStop || block.blockMaterial.IsLiquid)
				{
					PrefabHelpers.blockIds[i] = BlockValue.Air;
					PrefabHelpers.bTouched[i] = true;
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void insideOutsidePrefab()
	{
		int num = PrefabHelpers.dim.x * PrefabHelpers.dim.y;
		for (int i = 0; i < PrefabHelpers.dim.z; i++)
		{
			for (int j = 0; j < PrefabHelpers.dim.x; j++)
			{
				int num2 = j + i * num;
				for (int k = PrefabHelpers.dim.y - 1; k >= 0; k--)
				{
					int num3 = num2 + k * PrefabHelpers.dim.x;
					PrefabHelpers.eInsideOutside[num3] = EnumInsideOutside.Outside;
					if (!PrefabHelpers.blockIds[num3].Block.bImposterDontBlock)
					{
						PrefabHelpers.eInsideOutside[num3] = EnumInsideOutside.Border;
						break;
					}
				}
			}
		}
		for (int l = 0; l < PrefabHelpers.dim.z; l++)
		{
			for (int m = 0; m < PrefabHelpers.dim.y; m++)
			{
				int num4 = m * PrefabHelpers.dim.x + l * num;
				for (int n = 0; n < PrefabHelpers.dim.x; n++)
				{
					int num5 = n + num4;
					PrefabHelpers.eInsideOutside[num5] = EnumInsideOutside.Outside;
					if (!PrefabHelpers.blockIds[num5].isair && !PrefabHelpers.blockIds[num5].Block.bImposterDontBlock)
					{
						PrefabHelpers.eInsideOutside[num5] = EnumInsideOutside.Border;
						break;
					}
				}
				for (int num6 = PrefabHelpers.dim.x - 1; num6 >= 0; num6--)
				{
					int num7 = num6 + num4;
					PrefabHelpers.eInsideOutside[num7] = EnumInsideOutside.Outside;
					if (!PrefabHelpers.blockIds[num7].isair && !PrefabHelpers.blockIds[num7].Block.bImposterDontBlock)
					{
						PrefabHelpers.eInsideOutside[num7] = EnumInsideOutside.Border;
						break;
					}
				}
			}
		}
		for (int num8 = 0; num8 < PrefabHelpers.dim.y; num8++)
		{
			for (int num9 = 0; num9 < PrefabHelpers.dim.x; num9++)
			{
				int num10 = num9 + num8 * PrefabHelpers.dim.x;
				for (int num11 = 0; num11 < PrefabHelpers.dim.z; num11++)
				{
					int num12 = num10 + num11 * num;
					PrefabHelpers.eInsideOutside[num12] = EnumInsideOutside.Outside;
					if (!PrefabHelpers.blockIds[num12].isair && !PrefabHelpers.blockIds[num12].Block.bImposterDontBlock)
					{
						PrefabHelpers.eInsideOutside[num12] = EnumInsideOutside.Border;
						break;
					}
				}
				for (int num13 = PrefabHelpers.dim.z - 1; num13 >= 0; num13--)
				{
					int num14 = num10 + num13 * num;
					PrefabHelpers.eInsideOutside[num14] = EnumInsideOutside.Outside;
					if (!PrefabHelpers.blockIds[num14].isair && !PrefabHelpers.blockIds[num14].Block.bImposterDontBlock)
					{
						PrefabHelpers.eInsideOutside[num14] = EnumInsideOutside.Border;
						break;
					}
				}
			}
		}
	}

	public static void mergePrefab(bool _bRebuildMesh = true)
	{
		ChunkCluster chunkCache = GameManager.Instance.World.ChunkCache;
		PrefabHelpers.dim = new Vector3i((chunkCache.ChunkMaxPos.x - chunkCache.ChunkMinPos.x + 1) * 16, 256, (chunkCache.ChunkMaxPos.y - chunkCache.ChunkMinPos.y + 1) * 16);
		PrefabHelpers.eInsideOutside = new EnumInsideOutside[PrefabHelpers.dim.x * PrefabHelpers.dim.y * PrefabHelpers.dim.z];
		PrefabHelpers.bTouched = new bool[PrefabHelpers.dim.x * PrefabHelpers.dim.y * PrefabHelpers.dim.z];
		PrefabHelpers.blockIds = new BlockValue[PrefabHelpers.dim.x * PrefabHelpers.dim.y * PrefabHelpers.dim.z];
		int[] oldBlockIds = new int[PrefabHelpers.dim.x * PrefabHelpers.dim.y * PrefabHelpers.dim.z];
		int num = -chunkCache.ChunkMinPos.x * 16;
		int num2 = -chunkCache.ChunkMinPos.y * 16;
		List<Chunk> chunkArrayCopySync = chunkCache.GetChunkArrayCopySync();
		foreach (Chunk chunk in chunkArrayCopySync)
		{
			Vector3i wp = chunk.ToWorldPos(new Vector3i(num, 0, num2));
			chunk.LoopOverAllBlocks(delegate(int x, int y, int z, BlockValue bv)
			{
				PrefabHelpers.blockIds[x + wp.x + y * PrefabHelpers.dim.x + (z + wp.z) * PrefabHelpers.dim.x * PrefabHelpers.dim.y] = bv;
				oldBlockIds[x + wp.x + y * PrefabHelpers.dim.x + (z + wp.z) * PrefabHelpers.dim.x * PrefabHelpers.dim.y] = bv.type;
			}, true, false);
		}
		for (int i = 0; i < PrefabHelpers.blockIds.Length; i++)
		{
			BlockValue blockValue = PrefabHelpers.blockIds[i];
			if (blockValue.Block.MergeIntoId != 0)
			{
				PrefabHelpers.blockIds[i].type = blockValue.Block.MergeIntoId;
				PrefabHelpers.bTouched[i] = true;
			}
		}
		foreach (Chunk chunk2 in chunkArrayCopySync)
		{
			for (int j = 0; j < 16; j++)
			{
				for (int k = 0; k < 16; k++)
				{
					for (int l = 252; l > 0; l--)
					{
						Vector3i vector3i = chunk2.ToWorldPos(new Vector3i(j + num, l, k + num2));
						int num3 = vector3i.x + l * PrefabHelpers.dim.x + vector3i.z * PrefabHelpers.dim.x * PrefabHelpers.dim.y;
						if (PrefabHelpers.bTouched[num3])
						{
							chunk2.SetBlock(GameManager.Instance.World, j, l, k, PrefabHelpers.blockIds[num3], true, true, false, false, -1);
							if (Block.list[oldBlockIds[num3]].MergeIntoTexIds != null)
							{
								long num4 = 0L;
								long textureFull = chunk2.GetTextureFull(j, l, k);
								int[] mergeIntoTexIds = Block.list[oldBlockIds[num3]].MergeIntoTexIds;
								for (int m = 0; m < 6; m++)
								{
									if ((textureFull & 255L << m * 8) == 0L)
									{
										num4 |= (long)mergeIntoTexIds[m] << m * 8;
									}
									else
									{
										num4 |= (textureFull & 255L << m * 8);
									}
								}
								chunk2.SetTextureFull(j, l, k, num4);
							}
						}
					}
				}
			}
		}
		if (_bRebuildMesh)
		{
			PrefabHelpers.rebuildMesh();
		}
	}

	public static void cull()
	{
		PrefabHelpers.Init();
		MicroStopwatch microStopwatch = new MicroStopwatch();
		new MicroStopwatch();
		WorldChunkCache chunkCache = GameManager.Instance.World.ChunkCache;
		int num = 0;
		int num2 = 0;
		List<Chunk> chunkArrayCopySync = chunkCache.GetChunkArrayCopySync();
		Log.Out("copy from took " + microStopwatch.ElapsedMilliseconds.ToString());
		microStopwatch.ResetAndRestart();
		PrefabEditModeManager.Instance.UpdateMinMax();
		PrefabHelpers.eInsideOutside = PrefabEditModeManager.Instance.VoxelPrefab.UpdateInsideOutside(PrefabEditModeManager.Instance.minPos, PrefabEditModeManager.Instance.maxPos);
		Log.Out("insideOutsidePrefab took " + microStopwatch.ElapsedMilliseconds.ToString());
		microStopwatch.ResetAndRestart();
		Vector3i size = PrefabEditModeManager.Instance.VoxelPrefab.size;
		Vector3i minPos = PrefabEditModeManager.Instance.minPos;
		Vector3i maxPos = PrefabEditModeManager.Instance.maxPos;
		int num3 = 0;
		foreach (Chunk chunk in chunkArrayCopySync)
		{
			for (int i = 0; i < 16; i++)
			{
				for (int j = 0; j < 16; j++)
				{
					for (int k = 252; k > 0; k--)
					{
						Vector3i vector3i = chunk.ToWorldPos(new Vector3i(i + num, k, j + num2));
						if (vector3i.x >= minPos.x && vector3i.y >= minPos.y && vector3i.z >= minPos.z && vector3i.x <= maxPos.x && vector3i.y <= maxPos.y && vector3i.z <= maxPos.z)
						{
							int num4 = vector3i.x - minPos.x + (k - minPos.y) * size.x + (vector3i.z - minPos.z) * size.x * size.y;
							if (PrefabHelpers.eInsideOutside[num4] == EnumInsideOutside.Inside)
							{
								num3++;
								chunk.SetBlock(GameManager.Instance.World, i, k, j, PrefabHelpers.cInnerBlockBVReplace, true, true, false, false, -1);
								chunk.SetDensity(i, k, j, MarchingCubes.DensityAir);
							}
						}
					}
				}
			}
		}
		Debug.LogError("COUNT: " + num3.ToString());
		foreach (Chunk chunk2 in chunkArrayCopySync)
		{
			chunk2.NeedsRegeneration = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void rebuildMesh()
	{
		BlockShapeNew.bImposterGenerationActive = true;
		foreach (Chunk chunk in GameManager.Instance.World.ChunkCache.GetChunkArrayCopySync())
		{
			chunk.NeedsRegeneration = true;
		}
	}

	public static void export()
	{
		using (Stream stream = SdFile.Create(PrefabEditModeManager.Instance.VoxelPrefab.location.FullPathNoExtension + ".mesh"))
		{
			using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(false))
			{
				pooledBinaryWriter.SetBaseStream(stream);
				SimpleMeshFile.WriteGameObject(pooledBinaryWriter, PrefabEditModeManager.Instance.ImposterPrefab);
			}
		}
	}

	public static bool combine(bool _bCombineSliced)
	{
		if (PrefabEditModeManager.Instance.VoxelPrefab == null)
		{
			return false;
		}
		Dictionary<string, List<CombineInstance>> dictionary = null;
		if (_bCombineSliced)
		{
			dictionary = new Dictionary<string, List<CombineInstance>>();
		}
		List<MeshFilter> list = new List<MeshFilter>();
		List<ChunkGameObject> usedChunkGameObjects = GameManager.Instance.World.m_ChunkManager.GetUsedChunkGameObjects();
		for (int i = 0; i < usedChunkGameObjects.Count; i++)
		{
			MeshFilter[] componentsInChildren = usedChunkGameObjects[i].GetComponentsInChildren<MeshFilter>();
			list.AddRange(componentsInChildren);
		}
		UnityEngine.Object.Destroy(PrefabEditModeManager.Instance.ImposterPrefab);
		PrefabEditModeManager.Instance.ImposterPrefab = new GameObject(PrefabEditModeManager.Instance.VoxelPrefab.PrefabName);
		List<CombineInstance> list2 = new List<CombineInstance>();
		int num = 0;
		int j = 0;
		while (j < list.Count)
		{
			Mesh sharedMesh = list[j].sharedMesh;
			if (sharedMesh == null || sharedMesh.vertexCount == 0)
			{
				j++;
			}
			else
			{
				if (!_bCombineSliced)
				{
					if (num + sharedMesh.vertexCount > 65000)
					{
						Mesh mesh = new Mesh();
						mesh.CombineMeshes(list2.ToArray());
						PrefabHelpers.combine_createSubGameObject(null, mesh, Vector3.zero);
						list2.Clear();
						num = 0;
					}
					num += sharedMesh.vertexCount;
					list2.Add(new CombineInstance
					{
						mesh = sharedMesh,
						transform = list[j].transform.localToWorldMatrix
					});
				}
				else
				{
					string text = list[j].transform.parent.parent.name;
					if (!text.StartsWith("Chunk_"))
					{
						j++;
						continue;
					}
					text = text.Substring("Chunk_".Length);
					string[] array = text.Split(',', StringSplitOptions.None);
					int num2 = int.Parse(array[0]);
					int num3 = int.Parse(array[1]);
					text = string.Empty + Utils.Fastfloor((float)num2 / 2f).ToString() + "," + Utils.Fastfloor((float)num3 / 2f).ToString();
					List<CombineInstance> list3;
					if (!dictionary.TryGetValue(text, out list3))
					{
						list3 = new List<CombineInstance>();
						dictionary[text] = list3;
					}
					int num4 = 0;
					for (int k = 0; k < list3.Count; k++)
					{
						num4 += list3[k].mesh.vertexCount;
					}
					if (num4 + sharedMesh.vertexCount > 65000)
					{
						Mesh mesh2 = new Mesh();
						mesh2.CombineMeshes(list3.ToArray());
						PrefabHelpers.combine_createSubGameObject(text, mesh2, Vector3.zero);
						list3.Clear();
					}
					CombineInstance item = new CombineInstance
					{
						mesh = sharedMesh,
						transform = list[j].transform.localToWorldMatrix
					};
					list3.Add(item);
				}
				list[j].gameObject.SetActive(false);
				j++;
			}
		}
		if (!_bCombineSliced)
		{
			if (num > 0)
			{
				Mesh mesh3 = new Mesh();
				mesh3.CombineMeshes(list2.ToArray());
				PrefabHelpers.combine_createSubGameObject(null, mesh3, Vector3.zero);
			}
		}
		else
		{
			foreach (KeyValuePair<string, List<CombineInstance>> keyValuePair in dictionary)
			{
				Mesh mesh4 = new Mesh();
				mesh4.CombineMeshes(keyValuePair.Value.ToArray());
				PrefabHelpers.combine_createSubGameObject(keyValuePair.Key, mesh4, Vector3.zero);
			}
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static GameObject combine_createSubGameObject(string _goName, Mesh _mesh, Vector3 _goPosition)
	{
		GameObject gameObject = new GameObject();
		gameObject.transform.parent = PrefabEditModeManager.Instance.ImposterPrefab.transform;
		gameObject.transform.localPosition = _goPosition;
		gameObject.AddComponent<MeshRenderer>().sharedMaterial = MeshDescription.GetOpaqueMaterial();
		gameObject.AddComponent<MeshFilter>().mesh = _mesh;
		_mesh.RecalculateNormals();
		Vector3[] normals = _mesh.normals;
		Vector3[] vertices = _mesh.vertices;
		bool[] array = new bool[vertices.Length];
		bool flag = false;
		float num = (float)(-(float)PrefabEditModeManager.Instance.VoxelPrefab.yOffset + 4) + 0.25f;
		for (int i = 0; i < normals.Length; i++)
		{
			if (normals[i].y < -0.9f && vertices[i].y <= num)
			{
				array[i] = true;
				flag = true;
			}
		}
		Vector3 b = new Vector3(0f, 3f, 0f);
		if (flag)
		{
			Vector2[] uv = _mesh.uv;
			Color[] colors = _mesh.colors;
			Vector2[] uv2 = _mesh.uv2;
			int[] array2 = new int[vertices.Length];
			List<Vector3> list = new List<Vector3>();
			List<Vector2> list2 = new List<Vector2>();
			List<Vector2> list3 = new List<Vector2>();
			int num2 = 0;
			for (int j = 0; j < array.Length; j++)
			{
				if (!array[j])
				{
					list.Add(vertices[j] + b);
					list2.Add(uv[j]);
					list3.Add(uv2[j]);
				}
				else
				{
					num2++;
				}
				array2[j] = num2;
			}
			int[] indices = _mesh.GetIndices(0);
			List<int> list4 = new List<int>();
			for (int k = 0; k < indices.Length; k += 3)
			{
				if (!array[indices[k]] || !array[indices[k + 1]] || !array[indices[k + 2]])
				{
					list4.Add(indices[k] - array2[indices[k]]);
					list4.Add(indices[k + 1] - array2[indices[k + 1]]);
					list4.Add(indices[k + 2] - array2[indices[k + 2]]);
				}
			}
			_mesh.Clear();
			_mesh.SetVertices(list);
			_mesh.SetTriangles(list4, 0);
			_mesh.SetUVs(0, list2);
			_mesh.SetUVs(1, list3);
		}
		else
		{
			for (int l = 0; l < vertices.Length; l++)
			{
				vertices[l] += b;
			}
			_mesh.vertices = vertices;
		}
		gameObject.transform.name = (_goName ?? ("mesh_" + _mesh.vertexCount.ToString()));
		return gameObject;
	}

	public static void DensityChange(int _densityMatch, int _densitySet)
	{
		MicroStopwatch microStopwatch = new MicroStopwatch();
		List<Chunk> chunkArrayCopySync = GameManager.Instance.World.ChunkCache.GetChunkArrayCopySync();
		PrefabEditModeManager.Instance.UpdateMinMax();
		Vector3i minPos = PrefabEditModeManager.Instance.minPos;
		Vector3i maxPos = PrefabEditModeManager.Instance.maxPos;
		int num = 0;
		foreach (Chunk chunk in chunkArrayCopySync)
		{
			for (int i = 0; i < 16; i++)
			{
				Vector3i pos;
				pos.z = i;
				for (int j = 0; j < 16; j++)
				{
					pos.x = j;
					for (int k = 255; k >= 0; k--)
					{
						pos.y = k;
						Vector3i vector3i = chunk.ToWorldPos(pos);
						if (vector3i.x >= minPos.x && vector3i.y >= minPos.y && vector3i.z >= minPos.z && vector3i.x <= maxPos.x && vector3i.y <= maxPos.y && vector3i.z <= maxPos.z && chunk.GetBlockId(j, k, i) != 0 && (int)chunk.GetDensity(j, k, i) == _densityMatch)
						{
							num++;
							chunk.SetDensity(j, k, i, (sbyte)_densitySet);
						}
					}
				}
			}
		}
		Log.Out("DensityChange {0} chunks, {1} blocks, in {2}ms", new object[]
		{
			chunkArrayCopySync.Count,
			num,
			(float)microStopwatch.ElapsedMicroseconds * 0.001f
		});
		foreach (Chunk chunk2 in chunkArrayCopySync)
		{
			chunk2.NeedsRegeneration = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string cInnerBlockReplace = "imposterBlock";

	[PublicizedFrom(EAccessModifier.Private)]
	public static BlockValue cInnerBlockBVReplace;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector3i dim;

	[PublicizedFrom(EAccessModifier.Private)]
	public static EnumInsideOutside[] eInsideOutside;

	[PublicizedFrom(EAccessModifier.Private)]
	public static BlockValue[] blockIds;

	[PublicizedFrom(EAccessModifier.Private)]
	public static byte[] textureIdxOverride;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool[] bTouched;
}

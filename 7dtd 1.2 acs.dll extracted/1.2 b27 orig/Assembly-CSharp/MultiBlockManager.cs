using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using Unity.Profiling;
using UnityEngine;

public class MultiBlockManager
{
	public static MultiBlockManager Instance
	{
		get
		{
			if (MultiBlockManager.m_Instance == null)
			{
				MultiBlockManager.m_Instance = new MultiBlockManager();
			}
			return MultiBlockManager.m_Instance;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public MultiBlockManager()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool CheckFeatures(MultiBlockManager.FeatureFlags targetFeatures, MultiBlockManager.FeatureRequirement requirement = MultiBlockManager.FeatureRequirement.AllEnabled)
	{
		this.DoCurrentModeSanityChecks();
		switch (requirement)
		{
		case MultiBlockManager.FeatureRequirement.OneOrMoreEnabled:
			return (this.enabledFeatures & targetFeatures) > MultiBlockManager.FeatureFlags.None;
		case MultiBlockManager.FeatureRequirement.AllDisabled:
			return (this.enabledFeatures & targetFeatures) == MultiBlockManager.FeatureFlags.None;
		}
		return (this.enabledFeatures & targetFeatures) == targetFeatures;
	}

	public bool NoFeaturesEnabled
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.CheckFeatures(MultiBlockManager.FeatureFlags.All, MultiBlockManager.FeatureRequirement.AllDisabled);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public MultiBlockManager.FeatureFlags GetFeaturesForMode(MultiBlockManager.Mode mode)
	{
		MultiBlockManager.FeatureFlags result;
		switch (mode)
		{
		case MultiBlockManager.Mode.Normal:
			result = MultiBlockManager.FeatureFlags.All;
			break;
		case MultiBlockManager.Mode.WorldEditor:
			result = (MultiBlockManager.FeatureFlags.POIMBTracking | MultiBlockManager.FeatureFlags.CrossChunkMBTracking | MultiBlockManager.FeatureFlags.OversizedStability | MultiBlockManager.FeatureFlags.TerrainAlignment);
			break;
		case MultiBlockManager.Mode.PrefabPlaytest:
			result = (MultiBlockManager.FeatureFlags.POIMBTracking | MultiBlockManager.FeatureFlags.OversizedStability | MultiBlockManager.FeatureFlags.TerrainAlignment);
			break;
		case MultiBlockManager.Mode.PrefabEditor:
			result = (MultiBlockManager.FeatureFlags.POIMBTracking | MultiBlockManager.FeatureFlags.TerrainAlignment);
			break;
		case MultiBlockManager.Mode.Client:
			result = MultiBlockManager.FeatureFlags.TerrainAlignment;
			break;
		default:
			result = MultiBlockManager.FeatureFlags.None;
			break;
		}
		return result;
	}

	public bool POIMBTrackingEnabled
	{
		get
		{
			return this.CheckFeatures(MultiBlockManager.FeatureFlags.POIMBTracking, MultiBlockManager.FeatureRequirement.AllEnabled);
		}
	}

	public void Initialize(RegionFileManager regionFileManager)
	{
		object obj = this.lockObj;
		lock (obj)
		{
			this.regionFileManager = regionFileManager;
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				if (GameManager.Instance.IsEditMode())
				{
					this.currentMode = ((regionFileManager == null) ? MultiBlockManager.Mode.PrefabEditor : MultiBlockManager.Mode.WorldEditor);
				}
				else
				{
					this.currentMode = ((regionFileManager == null) ? MultiBlockManager.Mode.PrefabPlaytest : MultiBlockManager.Mode.Normal);
				}
			}
			else
			{
				this.currentMode = MultiBlockManager.Mode.Client;
			}
			this.enabledFeatures = this.GetFeaturesForMode(this.currentMode);
			this.world = GameManager.Instance.World;
			this.cc = this.world.ChunkCache;
			this.chunkManager = this.world.m_ChunkManager;
			if (this.CheckFeatures(MultiBlockManager.FeatureFlags.OversizedStability | MultiBlockManager.FeatureFlags.TerrainAlignment, MultiBlockManager.FeatureRequirement.OneOrMoreEnabled))
			{
				ChunkManager chunkManager = this.chunkManager;
				chunkManager.OnChunkInitialized = (Action<Chunk>)Delegate.Combine(chunkManager.OnChunkInitialized, new Action<Chunk>(this.OnChunkInitialized));
			}
			ChunkManager chunkManager2 = this.chunkManager;
			chunkManager2.OnChunkRegenerated = (Action<Chunk>)Delegate.Combine(chunkManager2.OnChunkRegenerated, new Action<Chunk>(this.OnChunkRegeneratedOrDisplayed));
			ChunkManager chunkManager3 = this.chunkManager;
			chunkManager3.OnChunkCopiedToUnity = (Action<Chunk>)Delegate.Combine(chunkManager3.OnChunkCopiedToUnity, new Action<Chunk>(this.OnChunkRegeneratedOrDisplayed));
			this.filePath = Path.Combine(GameIO.GetSaveGameDir(), "multiblocks.7dt");
			this.<Initialize>g__TryLoad|56_0();
			this.UpdateProfilerCounters();
			this.isDirty = false;
		}
	}

	public void Cleanup()
	{
		if (this.NoFeaturesEnabled)
		{
			return;
		}
		object obj = this.lockObj;
		lock (obj)
		{
			ChunkManager chunkManager = this.chunkManager;
			chunkManager.OnChunkInitialized = (Action<Chunk>)Delegate.Remove(chunkManager.OnChunkInitialized, new Action<Chunk>(this.OnChunkInitialized));
			ChunkManager chunkManager2 = this.chunkManager;
			chunkManager2.OnChunkRegenerated = (Action<Chunk>)Delegate.Remove(chunkManager2.OnChunkRegenerated, new Action<Chunk>(this.OnChunkRegeneratedOrDisplayed));
			ChunkManager chunkManager3 = this.chunkManager;
			chunkManager3.OnChunkCopiedToUnity = (Action<Chunk>)Delegate.Remove(chunkManager3.OnChunkCopiedToUnity, new Action<Chunk>(this.OnChunkRegeneratedOrDisplayed));
			this.SaveIfDirty();
			this.trackedDataMap.Clear();
			this.oversizedBlocksWithDirtyStability.Clear();
			this.blocksWithDirtyAlignment.Clear();
			this.deregisteredMultiBlockCount = 0;
			this.UpdateProfilerCounters();
			this.filePath = null;
			this.regionFileManager = null;
			this.cc = null;
			this.currentMode = MultiBlockManager.Mode.Disabled;
			this.enabledFeatures = MultiBlockManager.FeatureFlags.None;
		}
	}

	public void SaveIfDirty()
	{
		if (!this.CheckFeatures(MultiBlockManager.FeatureFlags.Serialization, MultiBlockManager.FeatureRequirement.AllEnabled))
		{
			return;
		}
		ProfilerCounterValue<int> profilerCounterValue = MultiBlockManager.s_MultiBlockManagerSaveCallCount;
		int value = profilerCounterValue.Value;
		profilerCounterValue.Value = value + 1;
		using (MultiBlockManager.s_MultiBlockManagerSaveAll.Auto())
		{
			object obj = this.lockObj;
			lock (obj)
			{
				if (!this.isDirty)
				{
					return;
				}
				if (string.IsNullOrEmpty(this.filePath))
				{
					UnityEngine.Debug.LogError("[MultiBlockManager] Failed to save MultiBlock data; MultiBlockManager has not been initialized with a valid filepath.");
					return;
				}
				this.CullCompletePOIPlacements();
				using (Stream stream = SdFile.OpenWrite(this.filePath))
				{
					using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(false))
					{
						pooledBinaryWriter.SetBaseStream(stream);
						pooledBinaryWriter.Write(6);
						pooledBinaryWriter.Write(this.trackedDataMap.TrackedDataByPosition.Count);
						foreach (KeyValuePair<Vector3i, MultiBlockManager.TrackedBlockData> keyValuePair in this.trackedDataMap.TrackedDataByPosition)
						{
							StreamUtils.Write(pooledBinaryWriter, keyValuePair.Key);
							MultiBlockManager.TrackedBlockData value2 = keyValuePair.Value;
							pooledBinaryWriter.Write(value2.rawData);
							pooledBinaryWriter.Write((byte)value2.trackingTypeFlags);
						}
						stream.SetLength(stream.Position);
						pooledBinaryWriter.Flush();
					}
				}
				this.isDirty = false;
			}
		}
		profilerCounterValue = MultiBlockManager.s_MultiBlockManagerSaveProcessedCount;
		value = profilerCounterValue.Value;
		profilerCounterValue.Value = value + 1;
	}

	public void CullChunklessData()
	{
		if (!this.CheckFeatures(MultiBlockManager.FeatureFlags.CrossChunkMBTracking, MultiBlockManager.FeatureRequirement.AllEnabled))
		{
			return;
		}
		using (MultiBlockManager.s_MultiBlockManagerCullChunkless.Auto())
		{
			object obj = this.lockObj;
			lock (obj)
			{
				foreach (KeyValuePair<Vector3i, MultiBlockManager.TrackedBlockData> keyValuePair in this.trackedDataMap.TrackedDataByPosition)
				{
					Vector3i key = keyValuePair.Key;
					RectInt flatChunkBounds = keyValuePair.Value.flatChunkBounds;
					if (!this.<CullChunklessData>g__AnyOverlappedChunkIsSyncedOrInSaveDir|59_0(flatChunkBounds))
					{
						this.keysToRemove.Enqueue(key);
					}
				}
				Vector3i worldPos;
				while (this.keysToRemove.TryDequeue(out worldPos))
				{
					this.DeregisterTrackedBlockDataInternal(worldPos);
				}
				this.ProcessDeregistrationCleanup();
				this.UpdateProfilerCounters();
			}
		}
	}

	public void UpdateTrackedBlockData(Vector3i worldPos, BlockValue blockValue, bool poiOwned)
	{
		if (this.NoFeaturesEnabled)
		{
			return;
		}
		using (MultiBlockManager.s_MultiBlockManagerUpdateTrackedBlockData.Auto())
		{
			object obj = this.lockObj;
			lock (obj)
			{
				MultiBlockManager.TrackingTypeFlags trackingTypeFlags = MultiBlockManager.TrackingTypeFlags.None;
				MultiBlockManager.TrackedBlockData trackedBlockData;
				if (this.trackedDataMap.TryGetValue(worldPos, out trackedBlockData))
				{
					if (trackedBlockData.rawData != blockValue.rawData)
					{
						this.DeregisterTrackedBlockDataInternal(worldPos);
						this.ProcessDeregistrationCleanup();
					}
					else
					{
						trackingTypeFlags = trackedBlockData.trackingTypeFlags;
						if (poiOwned)
						{
							if ((trackedBlockData.trackingTypeFlags & MultiBlockManager.TrackingTypeFlags.CrossChunkMultiBlock) != MultiBlockManager.TrackingTypeFlags.None)
							{
								this.trackedDataMap.RemoveTrackedData(worldPos, MultiBlockManager.TrackingTypeFlags.CrossChunkMultiBlock);
								this.deregisteredMultiBlockCount++;
								this.ProcessDeregistrationCleanup();
								trackingTypeFlags &= ~MultiBlockManager.TrackingTypeFlags.CrossChunkMultiBlock;
							}
						}
						else if ((trackedBlockData.trackingTypeFlags & MultiBlockManager.TrackingTypeFlags.PoiMultiBlock) != MultiBlockManager.TrackingTypeFlags.None)
						{
							this.trackedDataMap.RemoveTrackedData(worldPos, MultiBlockManager.TrackingTypeFlags.PoiMultiBlock);
							trackingTypeFlags &= ~MultiBlockManager.TrackingTypeFlags.PoiMultiBlock;
						}
					}
				}
				if (blockValue.Block.isMultiBlock)
				{
					if (poiOwned)
					{
						if ((trackingTypeFlags & MultiBlockManager.TrackingTypeFlags.PoiMultiBlock) == MultiBlockManager.TrackingTypeFlags.None)
						{
						}
					}
					else if ((trackingTypeFlags & MultiBlockManager.TrackingTypeFlags.CrossChunkMultiBlock) == MultiBlockManager.TrackingTypeFlags.None)
					{
						this.TryRegisterCrossChunkMultiBlock(worldPos, blockValue);
					}
				}
				if (blockValue.Block.isOversized && (trackingTypeFlags & MultiBlockManager.TrackingTypeFlags.OversizedBlock) == MultiBlockManager.TrackingTypeFlags.None)
				{
					this.TryRegisterOversizedBlock(worldPos, blockValue);
				}
				if (blockValue.Block.terrainAlignmentMode != TerrainAlignmentMode.None && (trackingTypeFlags & MultiBlockManager.TrackingTypeFlags.TerrainAlignedBlock) == MultiBlockManager.TrackingTypeFlags.None)
				{
					this.TryRegisterTerrainAlignedBlockInternal(worldPos, blockValue);
				}
				this.UpdateProfilerCounters();
			}
		}
	}

	public void DeregisterTrackedBlockData(Vector3i worldPos)
	{
		if (this.NoFeaturesEnabled)
		{
			return;
		}
		object obj = this.lockObj;
		lock (obj)
		{
			this.DeregisterTrackedBlockDataInternal(worldPos);
			this.ProcessDeregistrationCleanup();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DeregisterTrackedBlockDataInternal(Vector3i worldPos)
	{
		MultiBlockManager.TrackedBlockData trackedBlockData;
		if (this.trackedDataMap.TryGetValue(worldPos, out trackedBlockData))
		{
			if ((trackedBlockData.trackingTypeFlags & MultiBlockManager.TrackingTypeFlags.CrossChunkMultiBlock) != MultiBlockManager.TrackingTypeFlags.None)
			{
				this.deregisteredMultiBlockCount++;
			}
			this.trackedDataMap.RemoveTrackedData(worldPos, MultiBlockManager.TrackingTypeFlags.All);
			this.blocksWithDirtyAlignment.Remove(worldPos);
			this.oversizedBlocksWithDirtyStability.Remove(worldPos);
			this.isDirty = true;
			this.UpdateProfilerCounters();
		}
	}

	public void DeregisterTrackedBlockDatas(Bounds bounds)
	{
		if (this.NoFeaturesEnabled)
		{
			return;
		}
		object obj = this.lockObj;
		lock (obj)
		{
			List<Vector3i> list = null;
			foreach (Vector3i vector3i in this.trackedDataMap.TrackedDataByPosition.Keys)
			{
				if (bounds.Contains(vector3i))
				{
					if (list == null)
					{
						list = new List<Vector3i>();
					}
					list.Add(vector3i);
				}
			}
			if (list != null)
			{
				foreach (Vector3i worldPos in list)
				{
					this.DeregisterTrackedBlockDataInternal(worldPos);
				}
				this.ProcessDeregistrationCleanup();
			}
		}
	}

	public void MainThreadUpdate()
	{
		this.UpdateAlignment();
		this.UpdateOversizedStability();
	}

	public bool TryRegisterTerrainAlignedBlock(Vector3i worldPos, BlockValue blockValue)
	{
		bool result = this.TryRegisterTerrainAlignedBlockInternal(worldPos, blockValue);
		this.UpdateProfilerCounters();
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool TryRegisterTerrainAlignedBlockInternal(Vector3i worldPos, BlockValue blockValue)
	{
		if (!this.CheckFeatures(MultiBlockManager.FeatureFlags.TerrainAlignment, MultiBlockManager.FeatureRequirement.AllEnabled))
		{
			return false;
		}
		bool result;
		using (MultiBlockManager.s_MultiBlockManagerTryRegisterTerrainAligned.Auto())
		{
			if (blockValue.Block.terrainAlignmentMode == TerrainAlignmentMode.None)
			{
				UnityEngine.Debug.LogError(string.Format("[MultiBlockManager] TryRegisterTerrainAlignedBlock failed: target block of type {0} at {1} is not a terrain-aligned block.", blockValue.Block.GetBlockName(), worldPos));
				result = false;
			}
			else if (blockValue.ischild)
			{
				UnityEngine.Debug.LogError(string.Format("[MultiBlockManager] TryRegisterTerrainAlignedBlock failed: target block is not a parent at position {0}.", worldPos));
				result = false;
			}
			else
			{
				object obj = this.lockObj;
				lock (obj)
				{
					MultiBlockManager.TrackedBlockData trackedBlockData;
					if (this.trackedDataMap.TryGetValue(worldPos, out trackedBlockData))
					{
						if (blockValue.rawData != trackedBlockData.rawData)
						{
							UnityEngine.Debug.LogError(string.Format("Unexpected condition in TryRegisterTerrainAlignedBlock: encountered raw data mismatch at position {0}.", worldPos));
							result = false;
						}
						else if ((trackedBlockData.trackingTypeFlags & MultiBlockManager.TrackingTypeFlags.TerrainAlignedBlock) != MultiBlockManager.TrackingTypeFlags.None)
						{
							result = true;
						}
						else
						{
							this.<TryRegisterTerrainAlignedBlockInternal>g__RegisterTerrainAlignedBlock|66_0(worldPos, blockValue, trackedBlockData.flatChunkBounds);
							result = true;
						}
					}
					else
					{
						bool flag2 = false;
						if (blockValue.Block.isMultiBlock)
						{
							Vector3i v;
							Vector3i v2;
							MultiBlockManager.GetMinMaxWorldPositions(worldPos, blockValue, out v, out v2);
							Vector2i vector2i = World.toChunkXZ(v);
							Vector2i one = World.toChunkXZ(v2);
							RectInt flatChunkBounds = new RectInt(vector2i, one - vector2i);
							this.<TryRegisterTerrainAlignedBlockInternal>g__RegisterTerrainAlignedBlock|66_0(worldPos, blockValue, flatChunkBounds);
							flag2 = true;
						}
						if (blockValue.Block.isOversized)
						{
							Vector3i v3;
							Vector3i v4;
							OversizedBlockUtils.GetWorldAlignedBoundsExtents(worldPos, blockValue.Block.shape.GetRotation(blockValue), blockValue.Block.oversizedBounds, out v3, out v4);
							Vector2i vector2i2 = World.toChunkXZ(v3);
							Vector2i one2 = World.toChunkXZ(v4);
							RectInt flatChunkBounds2 = new RectInt(vector2i2, one2 - vector2i2);
							this.<TryRegisterTerrainAlignedBlockInternal>g__RegisterTerrainAlignedBlock|66_0(worldPos, blockValue, flatChunkBounds2);
							flag2 = true;
						}
						result = flag2;
					}
				}
			}
		}
		return result;
	}

	public void SetTerrainAlignmentDirty(Vector3i worldPos)
	{
		if (!this.CheckFeatures(MultiBlockManager.FeatureFlags.TerrainAlignment, MultiBlockManager.FeatureRequirement.AllEnabled))
		{
			return;
		}
		using (MultiBlockManager.s_MultiBlockManagerSetAlignmentDirty.Auto())
		{
			object obj = this.lockObj;
			lock (obj)
			{
				if (!this.blocksWithDirtyAlignment.Contains(worldPos))
				{
					if (!this.trackedDataMap.TerrainAlignedBlocks.ContainsKey(worldPos))
					{
						UnityEngine.Debug.LogWarning(string.Format("[MultiBlockManager][Alignment] SetTerrainAlignmentDirty failed; no terrain-aligned block has been registered at the specified world position: {0}", worldPos));
					}
					else
					{
						this.blocksWithDirtyAlignment.Add(worldPos);
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateAlignment()
	{
		if (!this.CheckFeatures(MultiBlockManager.FeatureFlags.TerrainAlignment, MultiBlockManager.FeatureRequirement.AllEnabled))
		{
			return;
		}
		using (MultiBlockManager.s_MultiBlockManagerUpdateAlignment.Auto())
		{
			object obj = this.lockObj;
			lock (obj)
			{
				ProfilerCounterValue<int> profilerCounterValue;
				MultiBlockManager.s_AlignmentChecksCount.Value = profilerCounterValue.Value + this.blocksWithDirtyAlignment.Count;
				foreach (Vector3i vector3i in this.blocksWithDirtyAlignment)
				{
					MultiBlockManager.TrackedBlockData blockData;
					if (!this.trackedDataMap.TerrainAlignedBlocks.TryGetValue(vector3i, out blockData))
					{
						UnityEngine.Debug.LogWarning(string.Format("[MultiBlockManager][Alignment] Failed to retrieve registered terrain-aligned block at expected location: {0}", vector3i));
					}
					else
					{
						this.<UpdateAlignment>g__TryAlignBlock|68_1(vector3i, blockData);
					}
				}
				this.blocksWithDirtyAlignment.Clear();
			}
		}
	}

	public void CullChunklessDataOnClient(List<long> removedChunks)
	{
		MultiBlockManager.<>c__DisplayClass69_0 CS$<>8__locals1;
		CS$<>8__locals1.removedChunks = removedChunks;
		CS$<>8__locals1.<>4__this = this;
		if (this.currentMode != MultiBlockManager.Mode.Client)
		{
			return;
		}
		using (MultiBlockManager.s_MultiBlockManagerCullChunkless.Auto())
		{
			object obj = this.lockObj;
			lock (obj)
			{
				foreach (KeyValuePair<Vector3i, MultiBlockManager.TrackedBlockData> keyValuePair in this.trackedDataMap.TrackedDataByPosition)
				{
					Vector3i key = keyValuePair.Key;
					RectInt flatChunkBounds = keyValuePair.Value.flatChunkBounds;
					if (!this.<CullChunklessDataOnClient>g__AnyOverlappedChunkIsSynced|69_0(flatChunkBounds, ref CS$<>8__locals1))
					{
						this.keysToRemove.Enqueue(key);
					}
				}
				Vector3i worldPos;
				while (this.keysToRemove.TryDequeue(out worldPos))
				{
					this.DeregisterTrackedBlockDataInternal(worldPos);
				}
				this.UpdateProfilerCounters();
			}
		}
	}

	public bool TryRegisterPOIMultiBlock(Vector3i parentWorldPos, BlockValue blockValue)
	{
		if (!this.CheckFeatures(MultiBlockManager.FeatureFlags.POIMBTracking, MultiBlockManager.FeatureRequirement.AllEnabled))
		{
			return false;
		}
		bool result;
		using (MultiBlockManager.s_MultiBlockManagerTryAddPOIMultiBlock.Auto())
		{
			object obj = this.lockObj;
			lock (obj)
			{
				MultiBlockManager.TrackedBlockData trackedBlockData;
				MultiBlockManager.TrackedBlockData trackedBlockData2;
				if (this.trackedDataMap.CrossChunkMultiBlocks.TryGetValue(parentWorldPos, out trackedBlockData))
				{
					UnityEngine.Debug.LogError(string.Format("[MultiBlockManager] Failed to register POI multiblock at {0} due to previously registered CrossChunk data.", parentWorldPos) + string.Format("\nOld value: {0} ", trackedBlockData.rawData) + string.Format("\nNew value: {0}", blockValue.rawData));
					result = false;
				}
				else if (this.trackedDataMap.PoiMultiBlocks.TryGetValue(parentWorldPos, out trackedBlockData2))
				{
					UnityEngine.Debug.LogError(string.Format("[MultiBlockManager] Duplicate multiblock placement at {0}. New value will not be applied.", parentWorldPos) + string.Format("\nOld value: {0} ", trackedBlockData2.rawData) + string.Format("\nNew value: {0}", blockValue.rawData));
					result = false;
				}
				else if (blockValue.ischild)
				{
					UnityEngine.Debug.LogError("[MultiBlockManager] TryAddPOIMultiBlock failed: target block is not a parent.");
					result = false;
				}
				else
				{
					RectInt flatChunkBounds;
					if (!blockValue.Block.isMultiBlock)
					{
						Vector2i v2i = World.toChunkXZ(parentWorldPos);
						flatChunkBounds = new RectInt(v2i, Vector2Int.zero);
					}
					else
					{
						Vector3i v;
						Vector3i v2;
						MultiBlockManager.GetMinMaxWorldPositions(parentWorldPos, blockValue, out v, out v2);
						Vector2i vector2i = World.toChunkXZ(v);
						Vector2i one = World.toChunkXZ(v2);
						flatChunkBounds = new RectInt(vector2i, one - vector2i);
					}
					this.trackedDataMap.AddOrMergeTrackedData(parentWorldPos, blockValue.rawData, flatChunkBounds, MultiBlockManager.TrackingTypeFlags.PoiMultiBlock);
					this.isDirty = true;
					this.UpdateProfilerCounters();
					result = true;
				}
			}
		}
		return result;
	}

	public static void GetMinMaxWorldPositions(Vector3i parentWorldPos, BlockValue blockValue, out Vector3i minPos, out Vector3i maxPos)
	{
		minPos = parentWorldPos;
		maxPos = parentWorldPos;
		for (int i = 0; i < blockValue.Block.multiBlockPos.Length; i++)
		{
			Vector3i v = parentWorldPos + blockValue.Block.multiBlockPos.Get(i, blockValue.type, (int)blockValue.rotation);
			minPos = Vector3i.Min(minPos, v);
			maxPos = Vector3i.Max(maxPos, v);
		}
	}

	public bool TryGetPOIMultiBlock(Vector3i parentWorldPos, out MultiBlockManager.TrackedBlockData poiMultiBlock)
	{
		if (!this.CheckFeatures(MultiBlockManager.FeatureFlags.POIMBTracking, MultiBlockManager.FeatureRequirement.AllEnabled))
		{
			poiMultiBlock = default(MultiBlockManager.TrackedBlockData);
			return false;
		}
		object obj = this.lockObj;
		bool result;
		lock (obj)
		{
			result = this.trackedDataMap.PoiMultiBlocks.TryGetValue(parentWorldPos, out poiMultiBlock);
		}
		return result;
	}

	public void CullCompletePOIPlacements()
	{
		if (!this.CheckFeatures(MultiBlockManager.FeatureFlags.POIMBTracking | MultiBlockManager.FeatureFlags.Serialization, MultiBlockManager.FeatureRequirement.AllEnabled))
		{
			return;
		}
		using (MultiBlockManager.s_MultiBlockManagerCullCompletePOIPlacements.Auto())
		{
			object obj = this.lockObj;
			lock (obj)
			{
				foreach (KeyValuePair<Vector3i, MultiBlockManager.TrackedBlockData> keyValuePair in this.trackedDataMap.PoiMultiBlocks)
				{
					Vector3i key = keyValuePair.Key;
					RectInt flatChunkBounds = keyValuePair.Value.flatChunkBounds;
					if (this.<CullCompletePOIPlacements>g__AllOverlappedChunksAreSavedAndDormant|73_0(flatChunkBounds))
					{
						this.keysToRemove.Enqueue(key);
					}
				}
				Vector3i worldPos;
				while (this.keysToRemove.TryDequeue(out worldPos))
				{
					this.trackedDataMap.RemoveTrackedData(worldPos, MultiBlockManager.TrackingTypeFlags.PoiMultiBlock);
					this.isDirty = true;
				}
				this.UpdateProfilerCounters();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool TryRegisterCrossChunkMultiBlock(Vector3i parentWorldPos, BlockValue parentBlockValue)
	{
		if (!this.CheckFeatures(MultiBlockManager.FeatureFlags.CrossChunkMBTracking, MultiBlockManager.FeatureRequirement.AllEnabled))
		{
			return false;
		}
		bool result;
		using (MultiBlockManager.s_MultiBlockManagerTryRegisterWorld.Auto())
		{
			object obj = this.lockObj;
			lock (obj)
			{
				if (this.trackedDataMap.PoiMultiBlocks.ContainsKey(parentWorldPos))
				{
					result = false;
				}
				else if (!parentBlockValue.Block.isMultiBlock)
				{
					UnityEngine.Debug.LogError(string.Format("[MultiBlockManager] TryRegisterCrossChunkMultiBlock failed: target block of type {0} at {1} is not a MultiBlock.", parentBlockValue.Block.GetBlockName(), parentWorldPos));
					result = false;
				}
				else if (parentBlockValue.ischild)
				{
					UnityEngine.Debug.LogError("[MultiBlockManager] TryRegisterCrossChunkMultiBlock failed: target block is not a parent.");
					result = false;
				}
				else
				{
					Vector3 vector = parentBlockValue.Block.shape.GetRotation(parentBlockValue) * parentBlockValue.Block.multiBlockPos.dim;
					if (Mathf.Approximately(Mathf.Abs(vector.x), 1f) && Mathf.Approximately(Mathf.Abs(vector.z), 1f))
					{
						result = false;
					}
					else
					{
						Vector3i vector3i;
						Vector3i vector3i2;
						MultiBlockManager.GetMinMaxWorldPositions(parentWorldPos, parentBlockValue, out vector3i, out vector3i2);
						if (vector3i.x == vector3i2.x && vector3i.z == vector3i2.z)
						{
							result = false;
						}
						else
						{
							Vector2i vector2i = World.toChunkXZ(vector3i);
							Vector2i vector2i2 = World.toChunkXZ(vector3i2);
							if (vector2i == vector2i2)
							{
								result = false;
							}
							else
							{
								RectInt flatChunkBounds = new RectInt(vector2i, vector2i2 - vector2i);
								this.trackedDataMap.AddOrMergeTrackedData(parentWorldPos, parentBlockValue.rawData, flatChunkBounds, MultiBlockManager.TrackingTypeFlags.CrossChunkMultiBlock);
								this.isDirty = true;
								this.tempChunksToGroup.Clear();
								for (int i = flatChunkBounds.yMin; i <= flatChunkBounds.yMax; i++)
								{
									for (int j = flatChunkBounds.xMin; j <= flatChunkBounds.xMax; j++)
									{
										long item = WorldChunkCache.MakeChunkKey(j, i);
										this.tempChunksToGroup.Add(item);
									}
								}
								this.regionFileManager.AddGroupedChunks(this.tempChunksToGroup);
								this.tempChunksToGroup.Clear();
								result = true;
							}
						}
					}
				}
			}
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ProcessDeregistrationCleanup()
	{
		if (!this.CheckFeatures(MultiBlockManager.FeatureFlags.CrossChunkMBTracking, MultiBlockManager.FeatureRequirement.AllEnabled))
		{
			return;
		}
		if (this.deregisteredMultiBlockCount <= 20)
		{
			return;
		}
		using (MultiBlockManager.s_MultiBlockManagerDeregistrationCleanup.Auto())
		{
			this.regionFileManager.RebuildChunkGroupsFromPOIs();
			this.tempChunksToGroup.Clear();
			foreach (KeyValuePair<Vector3i, MultiBlockManager.TrackedBlockData> keyValuePair in this.trackedDataMap.CrossChunkMultiBlocks)
			{
				RectInt flatChunkBounds = keyValuePair.Value.flatChunkBounds;
				for (int i = flatChunkBounds.yMin; i <= flatChunkBounds.yMax; i++)
				{
					for (int j = flatChunkBounds.xMin; j <= flatChunkBounds.xMax; j++)
					{
						long item = WorldChunkCache.MakeChunkKey(j, i);
						this.tempChunksToGroup.Add(item);
					}
				}
				this.regionFileManager.AddGroupedChunks(this.tempChunksToGroup);
				this.tempChunksToGroup.Clear();
			}
			this.deregisteredMultiBlockCount = 0;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool TryRegisterOversizedBlock(Vector3i worldPos, BlockValue blockValue)
	{
		if (!this.CheckFeatures(MultiBlockManager.FeatureFlags.OversizedStability, MultiBlockManager.FeatureRequirement.AllEnabled))
		{
			return false;
		}
		bool result;
		using (MultiBlockManager.s_MultiBlockManagerTryRegisterOversized.Auto())
		{
			object obj = this.lockObj;
			lock (obj)
			{
				if (!blockValue.Block.isOversized)
				{
					UnityEngine.Debug.LogError(string.Format("[MultiBlockManager] TryRegisterOversizedBlock failed: target block of type {0} at {1} is not an Oversized Block.", blockValue.Block.GetBlockName(), worldPos));
					return false;
				}
				Vector3i v;
				Vector3i v2;
				OversizedBlockUtils.GetWorldAlignedBoundsExtents(worldPos, blockValue.Block.shape.GetRotation(blockValue), blockValue.Block.oversizedBounds, out v, out v2);
				Vector2i vector2i = World.toChunkXZ(v);
				Vector2i one = World.toChunkXZ(v2);
				RectInt flatChunkBounds = new RectInt(vector2i, one - vector2i);
				this.trackedDataMap.AddOrMergeTrackedData(worldPos, blockValue.rawData, flatChunkBounds, MultiBlockManager.TrackingTypeFlags.OversizedBlock);
				this.oversizedBlocksWithDirtyStability.Add(worldPos);
				this.isDirty = true;
			}
			result = true;
		}
		return result;
	}

	public void SetOversizedStabilityDirty(Vector3i worldPos)
	{
		if (!this.CheckFeatures(MultiBlockManager.FeatureFlags.OversizedStability, MultiBlockManager.FeatureRequirement.AllEnabled))
		{
			return;
		}
		using (MultiBlockManager.s_MultiBlockManagerSetStabilityDirty.Auto())
		{
			object obj = this.lockObj;
			lock (obj)
			{
				Vector2i v2i = World.toChunkXZ(worldPos);
				foreach (KeyValuePair<Vector3i, MultiBlockManager.TrackedBlockData> keyValuePair in this.trackedDataMap.OversizedBlocks)
				{
					Vector3i key = keyValuePair.Key;
					if (!this.oversizedBlocksWithDirtyStability.Contains(key))
					{
						MultiBlockManager.TrackedBlockData value = keyValuePair.Value;
						RectInt flatChunkBounds = value.flatChunkBounds;
						flatChunkBounds.max += Vector2Int.one;
						if (flatChunkBounds.Contains(v2i))
						{
							BlockValue blockValue = new BlockValue(value.rawData);
							Quaternion rotation = blockValue.Block.shape.GetRotation(blockValue);
							Bounds localStabilityBounds = OversizedBlockUtils.GetLocalStabilityBounds(blockValue.Block.oversizedBounds, rotation);
							localStabilityBounds.extents += Vector3Int.one;
							Matrix4x4 blockWorldToLocalMatrix = OversizedBlockUtils.GetBlockWorldToLocalMatrix(key, rotation);
							if (OversizedBlockUtils.IsBlockCenterWithinBounds(worldPos, localStabilityBounds, blockWorldToLocalMatrix))
							{
								this.oversizedBlocksWithDirtyStability.Add(key);
							}
						}
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnChunkRegeneratedOrDisplayed(Chunk chunk)
	{
		if (!this.CheckFeatures(MultiBlockManager.FeatureFlags.TerrainAlignment, MultiBlockManager.FeatureRequirement.AllEnabled))
		{
			return;
		}
		Vector2i chunkPos = new Vector2i(chunk.X, chunk.Z);
		object obj = this.lockObj;
		lock (obj)
		{
			using (MultiBlockManager.s_MultiBlockManagerSetAlignmentDirty.Auto())
			{
				MultiBlockManager.AddChunkOverlappingBlocksToSet(chunkPos, this.trackedDataMap.TerrainAlignedBlocks, this.blocksWithDirtyAlignment);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnChunkInitialized(Chunk chunk)
	{
		if (!this.CheckFeatures(MultiBlockManager.FeatureFlags.OversizedStability | MultiBlockManager.FeatureFlags.TerrainAlignment, MultiBlockManager.FeatureRequirement.OneOrMoreEnabled))
		{
			return;
		}
		Vector2i chunkPos = new Vector2i(chunk.X, chunk.Z);
		object obj = this.lockObj;
		lock (obj)
		{
			if (this.CheckFeatures(MultiBlockManager.FeatureFlags.OversizedStability, MultiBlockManager.FeatureRequirement.AllEnabled))
			{
				using (MultiBlockManager.s_MultiBlockManagerSetStabilityDirty.Auto())
				{
					MultiBlockManager.AddChunkOverlappingBlocksToSet(chunkPos, this.trackedDataMap.OversizedBlocks, this.oversizedBlocksWithDirtyStability);
				}
			}
			if (this.CheckFeatures(MultiBlockManager.FeatureFlags.TerrainAlignment, MultiBlockManager.FeatureRequirement.AllEnabled))
			{
				using (MultiBlockManager.s_MultiBlockManagerSetAlignmentDirty.Auto())
				{
					MultiBlockManager.AddChunkOverlappingBlocksToSet(chunkPos, this.trackedDataMap.TerrainAlignedBlocks, this.blocksWithDirtyAlignment);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void AddChunkOverlappingBlocksToSet(Vector2i chunkPos, MultiBlockManager.TrackedDataMap.SubsetAccessor blocksMap, HashSet<Vector3i> targetSet)
	{
		foreach (KeyValuePair<Vector3i, MultiBlockManager.TrackedBlockData> keyValuePair in blocksMap)
		{
			Vector3i key = keyValuePair.Key;
			if (!targetSet.Contains(key))
			{
				RectInt flatChunkBounds = keyValuePair.Value.flatChunkBounds;
				flatChunkBounds.max += Vector2Int.one;
				if (flatChunkBounds.Contains(chunkPos))
				{
					targetSet.Add(key);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateOversizedStability()
	{
		if (!this.CheckFeatures(MultiBlockManager.FeatureFlags.OversizedStability, MultiBlockManager.FeatureRequirement.AllEnabled))
		{
			return;
		}
		using (MultiBlockManager.s_MultiBlockManagerUpdateStability.Auto())
		{
			object obj = this.lockObj;
			lock (obj)
			{
				ProfilerCounterValue<int> profilerCounterValue;
				MultiBlockManager.s_OversizedStabilityChecksCount.Value = profilerCounterValue.Value + this.oversizedBlocksWithDirtyStability.Count;
				foreach (Vector3i vector3i in this.oversizedBlocksWithDirtyStability)
				{
					MultiBlockManager.TrackedBlockData blockData;
					if (!this.trackedDataMap.OversizedBlocks.TryGetValue(vector3i, out blockData))
					{
						UnityEngine.Debug.LogWarning(string.Format("[MultiBlockManager][Stability] Failed to retrieve registered Oversized Block at expected location: {0}", vector3i));
					}
					else if (!this.<UpdateOversizedStability>g__IsOversizedBlockStable|81_1(vector3i, blockData))
					{
						GameManager.Instance.World.AddFallingBlock(vector3i, true);
					}
				}
				this.oversizedBlocksWithDirtyStability.Clear();
			}
		}
	}

	[Conditional("MBM_ENABLE_GENERAL_LOG")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void DebugLogGeneral(string message)
	{
		UnityEngine.Debug.Log(message);
	}

	[Conditional("MBM_ENABLE_PLACEMENT_LOG")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void DebugLogPlacement(string message)
	{
		UnityEngine.Debug.Log(message);
	}

	[Conditional("MBM_ENABLE_REGISTRATION_LOG")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void DebugLogRegistration(string message)
	{
		UnityEngine.Debug.Log(message);
	}

	[Conditional("MBM_ENABLE_STABILITY_LOG")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void DebugLogStability(string message)
	{
		UnityEngine.Debug.Log(message);
	}

	[Conditional("MBM_ENABLE_ALIGNMENT_LOG")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void DebugLogAlignment(string message)
	{
		UnityEngine.Debug.Log(message);
	}

	[Conditional("MBM_ENABLE_PROFILER_MARKERS")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateProfilerCounters()
	{
		MultiBlockManager.s_POIMultiBlockCount.Value = this.trackedDataMap.PoiMultiBlocks.Count;
		MultiBlockManager.s_CrossChunkMultiBlockCount.Value = this.trackedDataMap.CrossChunkMultiBlocks.Count;
		MultiBlockManager.s_OversizedBlockCount.Value = this.trackedDataMap.OversizedBlocks.Count;
		MultiBlockManager.s_TerrainAlignedBlockCount.Value = this.trackedDataMap.TerrainAlignedBlocks.Count;
		MultiBlockManager.s_DeregisteredMultiBlockCount.Value = this.deregisteredMultiBlockCount;
		MultiBlockManager.s_TrackedDataCount.Value = this.trackedDataMap.Count;
	}

	[Conditional("MBM_ENABLED_SANITY_CHECKS")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void DoCurrentModeSanityChecks()
	{
		MultiBlockManager.Mode mode = MultiBlockManager.Mode.Client;
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			if (GameManager.Instance.IsEditMode())
			{
				mode = ((this.regionFileManager == null) ? MultiBlockManager.Mode.PrefabEditor : MultiBlockManager.Mode.WorldEditor);
			}
			else
			{
				mode = ((this.regionFileManager == null) ? MultiBlockManager.Mode.PrefabPlaytest : MultiBlockManager.Mode.Normal);
			}
		}
		if (this.currentMode != mode)
		{
			UnityEngine.Debug.LogError("[MultiBlockManager] Unexpected mode state. \n" + string.Format("Current mode: {0}. Expected mode: {1}. \n", this.currentMode, mode) + string.Format("GameManager.Instance.IsEditMode(): {0}, ", GameManager.Instance.IsEditMode()) + string.Format("ConnectionManager.Instance.IsServer: {0}, ", SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer));
		}
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool <Initialize>g__TryLoad|56_0()
	{
		this.trackedDataMap.Clear();
		this.deregisteredMultiBlockCount = 0;
		if (!this.CheckFeatures(MultiBlockManager.FeatureFlags.Serialization, MultiBlockManager.FeatureRequirement.AllEnabled))
		{
			return false;
		}
		if (!SdFile.Exists(this.filePath))
		{
			return false;
		}
		bool result;
		using (Stream stream = SdFile.OpenRead(this.filePath))
		{
			using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(false))
			{
				pooledBinaryReader.SetBaseStream(stream);
				byte b = pooledBinaryReader.ReadByte();
				if (b != 6)
				{
					Log.Error(string.Format("[MultiBlockManager] Saved MultiBlock data is out of date. Saved version is ({0}). Current version is ({1}). ", b, 6) + "This data is no longer compatible and will be ignored. MultiBlock-related bugs are likely to occur if you continue with this save. Please start a fresh game to avoid these issues.");
					result = false;
				}
				else
				{
					int num = pooledBinaryReader.ReadInt32();
					for (int i = 0; i < num; i++)
					{
						Vector3i vector3i = StreamUtils.ReadVector3i(pooledBinaryReader);
						uint rawData = pooledBinaryReader.ReadUInt32();
						BlockValue blockValue = new BlockValue(rawData);
						byte b2 = pooledBinaryReader.ReadByte();
						if ((b2 & 1) != 0)
						{
							this.TryRegisterPOIMultiBlock(vector3i, blockValue);
						}
						if ((b2 & 2) != 0)
						{
							this.TryRegisterCrossChunkMultiBlock(vector3i, blockValue);
						}
						if ((b2 & 4) != 0)
						{
							this.TryRegisterOversizedBlock(vector3i, blockValue);
						}
						if ((b2 & 8) != 0)
						{
							this.TryRegisterTerrainAlignedBlockInternal(vector3i, blockValue);
						}
					}
					result = true;
				}
			}
		}
		return result;
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool <CullChunklessData>g__AnyOverlappedChunkIsSyncedOrInSaveDir|59_0(RectInt flatChunkBounds)
	{
		for (int i = flatChunkBounds.yMin; i <= flatChunkBounds.yMax; i++)
		{
			for (int j = flatChunkBounds.xMin; j <= flatChunkBounds.xMax; j++)
			{
				long key = WorldChunkCache.MakeChunkKey(j, i);
				if (this.regionFileManager.ContainsChunkSync(key) || this.cc.ContainsChunkSync(key))
				{
					return true;
				}
			}
		}
		return false;
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public void <TryRegisterTerrainAlignedBlockInternal>g__RegisterTerrainAlignedBlock|66_0(Vector3i worldPos, BlockValue blockValue, RectInt flatChunkBounds)
	{
		this.trackedDataMap.AddOrMergeTrackedData(worldPos, blockValue.rawData, flatChunkBounds, MultiBlockManager.TrackingTypeFlags.TerrainAlignedBlock);
		this.blocksWithDirtyAlignment.Add(worldPos);
		this.isDirty = true;
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool <UpdateAlignment>g__AllOverlappedChunksAreReady|68_0(RectInt flatChunkBounds)
	{
		for (int i = flatChunkBounds.yMin; i <= flatChunkBounds.yMax; i++)
		{
			for (int j = flatChunkBounds.xMin; j <= flatChunkBounds.xMax; j++)
			{
				long key = WorldChunkCache.MakeChunkKey(j, i);
				Chunk chunkSync = this.cc.GetChunkSync(key);
				if (chunkSync == null || !chunkSync.IsInitialized || chunkSync.NeedsRegeneration || chunkSync.InProgressRegeneration || chunkSync.NeedsCopying || chunkSync.InProgressCopying)
				{
					return false;
				}
			}
		}
		return true;
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool <UpdateAlignment>g__TryAlignBlock|68_1(Vector3i worldPos, MultiBlockManager.TrackedBlockData blockData)
	{
		RectInt flatChunkBounds = blockData.flatChunkBounds;
		if (!this.<UpdateAlignment>g__AllOverlappedChunksAreReady|68_0(flatChunkBounds))
		{
			return false;
		}
		BlockEntityData blockEntity = ((Chunk)this.world.GetChunkFromWorldPos(worldPos)).GetBlockEntity(worldPos);
		BlockValue blockValue = new BlockValue(blockData.rawData);
		Block block = blockValue.Block;
		TerrainAlignmentMode terrainAlignmentMode = block.terrainAlignmentMode;
		if (terrainAlignmentMode != TerrainAlignmentMode.None && terrainAlignmentMode - TerrainAlignmentMode.Vehicle <= 1)
		{
			return TerrainAlignmentUtils.AlignToTerrain(block, worldPos, blockValue, blockEntity, block.terrainAlignmentMode);
		}
		UnityEngine.Debug.LogError(string.Format("[MultiBlockManager][Alignment] TryAlignBlock cannot align block with TerrainAlignmentMode \"{0}\" of type {1} at {2}", block.terrainAlignmentMode, block.GetBlockName(), worldPos));
		return false;
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool <CullChunklessDataOnClient>g__AnyOverlappedChunkIsSynced|69_0(RectInt flatChunkBounds, ref MultiBlockManager.<>c__DisplayClass69_0 A_2)
	{
		for (int i = flatChunkBounds.yMin; i <= flatChunkBounds.yMax; i++)
		{
			for (int j = flatChunkBounds.xMin; j <= flatChunkBounds.xMax; j++)
			{
				long num = WorldChunkCache.MakeChunkKey(j, i);
				if (!A_2.removedChunks.Contains(num) && this.cc.ContainsChunkSync(num))
				{
					return true;
				}
			}
		}
		return false;
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool <CullCompletePOIPlacements>g__AllOverlappedChunksAreSavedAndDormant|73_0(RectInt flatChunkBounds)
	{
		for (int i = flatChunkBounds.yMin; i <= flatChunkBounds.yMax; i++)
		{
			for (int j = flatChunkBounds.xMin; j <= flatChunkBounds.xMax; j++)
			{
				long key = WorldChunkCache.MakeChunkKey(j, i);
				if (this.cc.ContainsChunkSync(key) || !this.regionFileManager.IsChunkSavedAndDormant(key))
				{
					return false;
				}
			}
		}
		return true;
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool <UpdateOversizedStability>g__AllOverlappedChunksAreSyncedAndInitialized|81_0(RectInt flatChunkBounds)
	{
		for (int i = flatChunkBounds.yMin; i <= flatChunkBounds.yMax; i++)
		{
			for (int j = flatChunkBounds.xMin; j <= flatChunkBounds.xMax; j++)
			{
				long key = WorldChunkCache.MakeChunkKey(j, i);
				Chunk chunkSync = this.cc.GetChunkSync(key);
				if (chunkSync == null || !chunkSync.IsInitialized)
				{
					return false;
				}
			}
		}
		return true;
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool <UpdateOversizedStability>g__IsOversizedBlockStable|81_1(Vector3i worldPos, MultiBlockManager.TrackedBlockData blockData)
	{
		RectInt flatChunkBounds = blockData.flatChunkBounds;
		if (!this.<UpdateOversizedStability>g__AllOverlappedChunksAreSyncedAndInitialized|81_0(flatChunkBounds))
		{
			return true;
		}
		BlockValue blockValue = new BlockValue(blockData.rawData);
		Quaternion rotation = blockValue.Block.shape.GetRotation(blockValue);
		Bounds localStabilityBounds = OversizedBlockUtils.GetLocalStabilityBounds(blockValue.Block.oversizedBounds, rotation);
		Vector3i vector3i;
		Vector3i vector3i2;
		OversizedBlockUtils.GetWorldAlignedBoundsExtents(worldPos, rotation, localStabilityBounds, out vector3i, out vector3i2);
		Matrix4x4 blockWorldToLocalMatrix = OversizedBlockUtils.GetBlockWorldToLocalMatrix(worldPos, rotation);
		World world = GameManager.Instance.World;
		for (int i = vector3i.x; i <= vector3i2.x; i++)
		{
			for (int j = vector3i.y; j <= vector3i2.y; j++)
			{
				for (int k = vector3i.z; k <= vector3i2.z; k++)
				{
					Vector3i vector3i3 = new Vector3i(i, j, k);
					if (OversizedBlockUtils.IsBlockCenterWithinBounds(vector3i3, localStabilityBounds, blockWorldToLocalMatrix) && world.GetStability(vector3i3) > 1)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const byte FILEVERSION = 6;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int c_deregisteredMultiBlockLimit = 20;

	[PublicizedFrom(EAccessModifier.Private)]
	public object lockObj = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_MultiBlockManagerSaveAll = new ProfilerMarker("MultiBlockManager.SaveAll");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_MultiBlockManagerUpdateTrackedBlockData = new ProfilerMarker("MultiBlockManager.UpdateTrackedBlockData");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_MultiBlockManagerTryAddPOIMultiBlock = new ProfilerMarker("MultiBlockManager.TryAddPOIMultiBlock");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_MultiBlockManagerCullChunkless = new ProfilerMarker("MultiBlockManager.CullChunkless");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_MultiBlockManagerCullCompletePOIPlacements = new ProfilerMarker("MultiBlockManager.CullCompletePOIPlacements");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_MultiBlockManagerTryRegisterWorld = new ProfilerMarker("MultiBlockManager.TryRegisterWorld");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_MultiBlockManagerTryRegisterOversized = new ProfilerMarker("MultiBlockManager.TryRegisterOversized");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_MultiBlockManagerTryRegisterTerrainAligned = new ProfilerMarker("MultiBlockManager.TryRegisterTerrainAligned");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_MultiBlockManagerDeregistrationCleanup = new ProfilerMarker("MultiBlockManager.DeregistrationCleanup");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_MultiBlockManagerSetStabilityDirty = new ProfilerMarker("MultiBlockManager.SetStabilityDirty");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_MultiBlockManagerUpdateStability = new ProfilerMarker("MultiBlockManager.UpdateStability");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_MultiBlockManagerSetAlignmentDirty = new ProfilerMarker("MultiBlockManager.SetAlignmentDirty");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_MultiBlockManagerUpdateAlignment = new ProfilerMarker("MultiBlockManager.UpdateAlignment");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerCounterValue<int> s_POIMultiBlockCount = new ProfilerCounterValue<int>(ProfilerCategory.Scripts, "POI MultiBlocks", ProfilerMarkerDataUnit.Count, ProfilerCounterOptions.FlushOnEndOfFrame);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerCounterValue<int> s_CrossChunkMultiBlockCount = new ProfilerCounterValue<int>(ProfilerCategory.Scripts, "Cross-Chunk MultiBlocks", ProfilerMarkerDataUnit.Count, ProfilerCounterOptions.FlushOnEndOfFrame);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerCounterValue<int> s_OversizedBlockCount = new ProfilerCounterValue<int>(ProfilerCategory.Scripts, "Oversized Blocks", ProfilerMarkerDataUnit.Count, ProfilerCounterOptions.FlushOnEndOfFrame);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerCounterValue<int> s_TerrainAlignedBlockCount = new ProfilerCounterValue<int>(ProfilerCategory.Scripts, "Terrain-Aligned Blocks", ProfilerMarkerDataUnit.Count, ProfilerCounterOptions.FlushOnEndOfFrame);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerCounterValue<int> s_TrackedDataCount = new ProfilerCounterValue<int>(ProfilerCategory.Scripts, "Tracked Data Count", ProfilerMarkerDataUnit.Count, ProfilerCounterOptions.FlushOnEndOfFrame);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerCounterValue<int> s_OversizedStabilityChecksCount = new ProfilerCounterValue<int>(ProfilerCategory.Scripts, "Oversized Stability Checks", ProfilerMarkerDataUnit.Count, ProfilerCounterOptions.FlushOnEndOfFrame | ProfilerCounterOptions.ResetToZeroOnFlush);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerCounterValue<int> s_AlignmentChecksCount = new ProfilerCounterValue<int>(ProfilerCategory.Scripts, "Alignment Checks", ProfilerMarkerDataUnit.Count, ProfilerCounterOptions.FlushOnEndOfFrame | ProfilerCounterOptions.ResetToZeroOnFlush);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerCounterValue<int> s_DeregisteredMultiBlockCount = new ProfilerCounterValue<int>(ProfilerCategory.Scripts, "Deregistered MultiBlocks", ProfilerMarkerDataUnit.Count, ProfilerCounterOptions.FlushOnEndOfFrame);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerCounterValue<int> s_MultiBlockManagerSaveCallCount = new ProfilerCounterValue<int>(ProfilerCategory.Scripts, "MultiBlock Save Called", ProfilerMarkerDataUnit.Count, ProfilerCounterOptions.FlushOnEndOfFrame | ProfilerCounterOptions.ResetToZeroOnFlush);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerCounterValue<int> s_MultiBlockManagerSaveProcessedCount = new ProfilerCounterValue<int>(ProfilerCategory.Scripts, "MultiBlock Save Processed", ProfilerMarkerDataUnit.Count, ProfilerCounterOptions.FlushOnEndOfFrame | ProfilerCounterOptions.ResetToZeroOnFlush);

	[PublicizedFrom(EAccessModifier.Private)]
	public static MultiBlockManager m_Instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public RegionFileManager regionFileManager;

	[PublicizedFrom(EAccessModifier.Private)]
	public MultiBlockManager.TrackedDataMap trackedDataMap = new MultiBlockManager.TrackedDataMap();

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<Vector3i> oversizedBlocksWithDirtyStability = new HashSet<Vector3i>();

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<Vector3i> blocksWithDirtyAlignment = new HashSet<Vector3i>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Queue<Vector3i> keysToRemove = new Queue<Vector3i>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<long> tempChunksToGroup = new List<long>();

	[PublicizedFrom(EAccessModifier.Private)]
	public World world;

	[PublicizedFrom(EAccessModifier.Private)]
	public ChunkCluster cc;

	[PublicizedFrom(EAccessModifier.Private)]
	public ChunkManager chunkManager;

	[PublicizedFrom(EAccessModifier.Private)]
	public string filePath;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDirty;

	[PublicizedFrom(EAccessModifier.Private)]
	public int deregisteredMultiBlockCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public MultiBlockManager.Mode currentMode;

	[PublicizedFrom(EAccessModifier.Private)]
	public MultiBlockManager.FeatureFlags enabledFeatures;

	[Flags]
	public enum TrackingTypeFlags : byte
	{
		None = 0,
		PoiMultiBlock = 1,
		CrossChunkMultiBlock = 2,
		OversizedBlock = 4,
		TerrainAlignedBlock = 8,
		All = 15
	}

	public struct TrackedBlockData
	{
		public TrackedBlockData(uint rawData, RectInt flatChunkBounds, MultiBlockManager.TrackingTypeFlags trackingTypeFlags)
		{
			this.rawData = rawData;
			this.flatChunkBounds = flatChunkBounds;
			this.trackingTypeFlags = trackingTypeFlags;
		}

		public readonly uint rawData;

		public readonly RectInt flatChunkBounds;

		public readonly MultiBlockManager.TrackingTypeFlags trackingTypeFlags;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class TrackedDataMap
	{
		public TrackedDataMap()
		{
			this.TrackedDataByPosition = new ReadOnlyDictionary<Vector3i, MultiBlockManager.TrackedBlockData>(this.trackedDataByPosition);
		}

		public bool ContainsKey(Vector3i key)
		{
			return this.trackedDataByPosition.ContainsKey(key);
		}

		public int Count
		{
			get
			{
				return this.trackedDataByPosition.Count;
			}
		}

		public bool TryGetValue(Vector3i key, out MultiBlockManager.TrackedBlockData value)
		{
			return this.trackedDataByPosition.TryGetValue(key, out value);
		}

		public void Clear()
		{
			this.trackedDataByPosition.Clear();
			this.poiMultiBlocks.Clear();
			this.crossChunkMultiBlocks.Clear();
			this.oversizedBlocks.Clear();
			this.terrainAlignedBlocks.Clear();
		}

		public void AddOrMergeTrackedData(Vector3i worldPos, uint rawData, RectInt flatChunkBounds, MultiBlockManager.TrackingTypeFlags trackingTypeFlags)
		{
			if (trackingTypeFlags == MultiBlockManager.TrackingTypeFlags.None)
			{
				UnityEngine.Debug.LogError(string.Format("AddOrMergeTrackedData failed: Cannot add or merge tracked data with no tracking flags set. At position {0}.", worldPos));
				return;
			}
			MultiBlockManager.TrackedBlockData trackedBlockData;
			MultiBlockManager.TrackingTypeFlags trackingTypeFlags2;
			if (this.trackedDataByPosition.TryGetValue(worldPos, out trackedBlockData))
			{
				if (rawData != trackedBlockData.rawData)
				{
					UnityEngine.Debug.LogWarning(string.Format("Unexpected condition in AddOrMergeTrackedData: encountered raw data mismatch at position {0}.", worldPos));
				}
				RectInt rectInt = flatChunkBounds;
				if (!rectInt.Equals(trackedBlockData.flatChunkBounds))
				{
					rectInt.SetMinMax(Vector2Int.Min(rectInt.min, trackedBlockData.flatChunkBounds.min), Vector2Int.Max(rectInt.max, trackedBlockData.flatChunkBounds.max));
					UnityEngine.Debug.Log(string.Format("Expanding FlatChunkBounds at position {0}. Old bounds: {1}. New bounds: {2}. Merged bounds: {3}.", new object[]
					{
						worldPos,
						trackedBlockData.flatChunkBounds,
						flatChunkBounds,
						rectInt
					}));
				}
				trackingTypeFlags2 = (trackingTypeFlags & ~trackedBlockData.trackingTypeFlags);
				if (trackingTypeFlags2 != trackingTypeFlags)
				{
					UnityEngine.Debug.LogWarning(string.Format("Unexpected condition in AddOrMergeTrackedData: tracked data already has one or more target flag(s) set at position {0}.", worldPos));
				}
				MultiBlockManager.TrackingTypeFlags trackingTypeFlags3 = trackingTypeFlags | trackedBlockData.trackingTypeFlags;
				this.trackedDataByPosition[worldPos] = new MultiBlockManager.TrackedBlockData(rawData, rectInt, trackingTypeFlags3);
			}
			else
			{
				trackingTypeFlags2 = trackingTypeFlags;
				this.trackedDataByPosition[worldPos] = new MultiBlockManager.TrackedBlockData(rawData, flatChunkBounds, trackingTypeFlags);
			}
			if (trackingTypeFlags2 == MultiBlockManager.TrackingTypeFlags.None)
			{
				return;
			}
			if ((trackingTypeFlags2 & MultiBlockManager.TrackingTypeFlags.PoiMultiBlock) != MultiBlockManager.TrackingTypeFlags.None)
			{
				this.poiMultiBlocks.Add(worldPos);
			}
			if ((trackingTypeFlags2 & MultiBlockManager.TrackingTypeFlags.CrossChunkMultiBlock) != MultiBlockManager.TrackingTypeFlags.None)
			{
				this.crossChunkMultiBlocks.Add(worldPos);
			}
			if ((trackingTypeFlags2 & MultiBlockManager.TrackingTypeFlags.OversizedBlock) != MultiBlockManager.TrackingTypeFlags.None)
			{
				this.oversizedBlocks.Add(worldPos);
			}
			if ((trackingTypeFlags2 & MultiBlockManager.TrackingTypeFlags.TerrainAlignedBlock) != MultiBlockManager.TrackingTypeFlags.None)
			{
				this.terrainAlignedBlocks.Add(worldPos);
			}
		}

		public void RemoveTrackedData(Vector3i worldPos, MultiBlockManager.TrackingTypeFlags flagsToRemove)
		{
			MultiBlockManager.TrackedBlockData trackedBlockData;
			if (!this.trackedDataByPosition.TryGetValue(worldPos, out trackedBlockData))
			{
				UnityEngine.Debug.LogError(string.Format("RemoveTrackedData failed; no tracked data at position {0}.", worldPos));
				return;
			}
			MultiBlockManager.TrackingTypeFlags trackingTypeFlags = flagsToRemove & trackedBlockData.trackingTypeFlags;
			if (trackingTypeFlags == MultiBlockManager.TrackingTypeFlags.None)
			{
				UnityEngine.Debug.LogError(string.Format("RemoveTrackedData failed; tracked data at position {0} does not have the target flag(s).", worldPos));
				return;
			}
			if ((trackingTypeFlags & MultiBlockManager.TrackingTypeFlags.PoiMultiBlock) != MultiBlockManager.TrackingTypeFlags.None)
			{
				this.poiMultiBlocks.Remove(worldPos);
			}
			if ((trackingTypeFlags & MultiBlockManager.TrackingTypeFlags.CrossChunkMultiBlock) != MultiBlockManager.TrackingTypeFlags.None)
			{
				this.crossChunkMultiBlocks.Remove(worldPos);
			}
			if ((trackingTypeFlags & MultiBlockManager.TrackingTypeFlags.OversizedBlock) != MultiBlockManager.TrackingTypeFlags.None)
			{
				this.oversizedBlocks.Remove(worldPos);
			}
			if ((trackingTypeFlags & MultiBlockManager.TrackingTypeFlags.TerrainAlignedBlock) != MultiBlockManager.TrackingTypeFlags.None)
			{
				this.terrainAlignedBlocks.Remove(worldPos);
			}
			MultiBlockManager.TrackingTypeFlags trackingTypeFlags2 = trackedBlockData.trackingTypeFlags & ~flagsToRemove;
			if (trackingTypeFlags2 == MultiBlockManager.TrackingTypeFlags.None)
			{
				this.trackedDataByPosition.Remove(worldPos);
				return;
			}
			this.trackedDataByPosition[worldPos] = new MultiBlockManager.TrackedBlockData(trackedBlockData.rawData, trackedBlockData.flatChunkBounds, trackingTypeFlags2);
		}

		public MultiBlockManager.TrackedDataMap.SubsetAccessor PoiMultiBlocks
		{
			get
			{
				return new MultiBlockManager.TrackedDataMap.SubsetAccessor(this.trackedDataByPosition, this.poiMultiBlocks);
			}
		}

		public MultiBlockManager.TrackedDataMap.SubsetAccessor CrossChunkMultiBlocks
		{
			get
			{
				return new MultiBlockManager.TrackedDataMap.SubsetAccessor(this.trackedDataByPosition, this.crossChunkMultiBlocks);
			}
		}

		public MultiBlockManager.TrackedDataMap.SubsetAccessor OversizedBlocks
		{
			get
			{
				return new MultiBlockManager.TrackedDataMap.SubsetAccessor(this.trackedDataByPosition, this.oversizedBlocks);
			}
		}

		public MultiBlockManager.TrackedDataMap.SubsetAccessor TerrainAlignedBlocks
		{
			get
			{
				return new MultiBlockManager.TrackedDataMap.SubsetAccessor(this.trackedDataByPosition, this.terrainAlignedBlocks);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Dictionary<Vector3i, MultiBlockManager.TrackedBlockData> trackedDataByPosition = new Dictionary<Vector3i, MultiBlockManager.TrackedBlockData>();

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly HashSet<Vector3i> poiMultiBlocks = new HashSet<Vector3i>();

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly HashSet<Vector3i> crossChunkMultiBlocks = new HashSet<Vector3i>();

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly HashSet<Vector3i> oversizedBlocks = new HashSet<Vector3i>();

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly HashSet<Vector3i> terrainAlignedBlocks = new HashSet<Vector3i>();

		public readonly ReadOnlyDictionary<Vector3i, MultiBlockManager.TrackedBlockData> TrackedDataByPosition;

		public struct SubsetAccessor : IEnumerator<KeyValuePair<Vector3i, MultiBlockManager.TrackedBlockData>>, IEnumerator, IDisposable
		{
			public SubsetAccessor(Dictionary<Vector3i, MultiBlockManager.TrackedBlockData> trackedData, HashSet<Vector3i> subset)
			{
				this._trackedData = trackedData;
				this._subset = subset;
				this._subsetEnumerator = subset.GetEnumerator();
			}

			public KeyValuePair<Vector3i, MultiBlockManager.TrackedBlockData> Current
			{
				get
				{
					return new KeyValuePair<Vector3i, MultiBlockManager.TrackedBlockData>(this._subsetEnumerator.Current, this._trackedData[this._subsetEnumerator.Current]);
				}
			}

			public bool MoveNext()
			{
				return this._subsetEnumerator.MoveNext();
			}

			public void Reset()
			{
				throw new NotSupportedException();
			}

			public void Dispose()
			{
				this._subsetEnumerator.Dispose();
			}

			public bool ContainsKey(Vector3i key)
			{
				return this._subset.Contains(key);
			}

			public int Count
			{
				get
				{
					return this._subset.Count;
				}
			}

			public bool TryGetValue(Vector3i key, out MultiBlockManager.TrackedBlockData value)
			{
				if (this._subset.Contains(key) && this._trackedData.TryGetValue(key, out value))
				{
					return true;
				}
				value = default(MultiBlockManager.TrackedBlockData);
				return false;
			}

			public MultiBlockManager.TrackedDataMap.SubsetAccessor GetEnumerator()
			{
				return this;
			}

			public object Current
			{
				[PublicizedFrom(EAccessModifier.Private)]
				get
				{
					return this.Current;
				}
			}

			public MultiBlockManager.TrackedBlockData this[Vector3i key]
			{
				get
				{
					if (!this._subset.Contains(key))
					{
						throw new KeyNotFoundException(string.Format("The key \"{0}\" was not found in the subset.", key));
					}
					return this._trackedData[key];
				}
			}

			[PublicizedFrom(EAccessModifier.Private)]
			public readonly Dictionary<Vector3i, MultiBlockManager.TrackedBlockData> _trackedData;

			[PublicizedFrom(EAccessModifier.Private)]
			public readonly HashSet<Vector3i> _subset;

			[PublicizedFrom(EAccessModifier.Private)]
			public HashSet<Vector3i>.Enumerator _subsetEnumerator;
		}
	}

	public enum Mode
	{
		Disabled,
		Normal,
		WorldEditor,
		PrefabPlaytest,
		PrefabEditor,
		Client
	}

	[Flags]
	public enum FeatureFlags
	{
		None = 0,
		POIMBTracking = 1,
		Serialization = 2,
		CrossChunkMBTracking = 4,
		OversizedStability = 8,
		TerrainAlignment = 16,
		All = 31
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public enum FeatureRequirement
	{
		AllEnabled,
		OneOrMoreEnabled,
		AllDisabled
	}
}

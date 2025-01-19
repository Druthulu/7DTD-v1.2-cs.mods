using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using Unity.Profiling;
using UnityEngine;

public class RegionFileManager : WorldChunkCache
{
	public long MaxBytes
	{
		get
		{
			return this.maxBytes;
		}
	}

	public SaveDataSlot SaveDataSlot { get; }

	public ReadOnlyCollection<HashSetLong> ChunkGroups { get; [PublicizedFrom(EAccessModifier.Private)] set; }

	public RegionFileManager(string _loadDirectory, string _saveDirectory, int _maxChunksInCache, bool _bSaveOnChunkDrop)
	{
		RegionFileManager.platformFactory = RegionFilePlatform.CreateFactory();
		SaveDataUtils.SaveDataManager.RegisterRegionFileManager(this);
		Vector2i worldSize = GameManager.Instance.World.ChunkCache.ChunkProvider.GetWorldSize();
		int capacity = worldSize.x / 16 * (worldSize.y / 16);
		this.chunksInSaveDir = new Dictionary<long, uint>(capacity);
		this.bSaveOnChunkDrop = _bSaveOnChunkDrop;
		this.groupTimestamps.Clear();
		this.RebuildChunkGroupsFromPOIs();
		this.ChunkGroups = new ReadOnlyCollection<HashSetLong>(this.chunkGroups);
		this.loadDirectory = ((_loadDirectory != null) ? Path.GetFullPath(_loadDirectory) : null);
		this.saveDirectory = ((_saveDirectory != null) ? Path.GetFullPath(_saveDirectory) : null);
		SaveDataManagedPath saveDataManagedPath;
		if (SaveDataUtils.TryGetManagedPath(this.saveDirectory, out saveDataManagedPath))
		{
			this.SaveDataSlot = saveDataManagedPath.Slot;
			Debug.Log(string.Format("[RegionFileManager] SaveDataSlot set to: {0}", saveDataManagedPath.Slot));
		}
		this.regionFileAccess = RegionFileManager.platformFactory.CreateRegionFileAccess();
		this.regionFileAccess.ReadDirectory(this.loadDirectory, delegate(long chunkKey, string ext, uint timeStamp)
		{
			this.chunksInLoadDir.Add(chunkKey);
		});
		this.regionFileAccess.ReadDirectory(this.saveDirectory, delegate(long chunkKey, string ext, uint timeStamp)
		{
			this.SetChunkTimestamp(chunkKey, timeStamp);
		});
		this.maxChunksInCache = _maxChunksInCache;
		this.snapshotUtil = RegionFileManager.platformFactory.CreateSnapshotUtil(this.regionFileAccess);
		this.OnGamePrefChanged(EnumGamePrefs.MaxChunkAge);
		this.OnGamePrefChanged(EnumGamePrefs.SaveDataLimit);
		GamePrefs.OnGamePrefChanged += this.OnGamePrefChanged;
		this.LoadResetRequests();
		string str;
		if (!string.IsNullOrEmpty(this.saveDirectory))
		{
			str = this.saveDirectory;
		}
		else
		{
			str = this.loadDirectory;
		}
		this.taskInfoThreadSaveChunks = ThreadManager.StartThread("SaveChunks " + str, null, new ThreadManager.ThreadFunctionLoopDelegate(this.thread_SaveChunks), null, System.Threading.ThreadPriority.Normal, null, null, true, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LoadResetRequests()
	{
		if (string.IsNullOrEmpty(this.saveDirectory))
		{
			return;
		}
		this.resetRequestedChunks.Clear();
		string path = Path.Combine(this.saveDirectory, "PendingResets.7pr");
		if (SdFile.Exists(path))
		{
			using (Stream stream = SdFile.OpenRead(path))
			{
				int num = StreamUtils.ReadInt32(stream);
				for (int i = 0; i < num; i++)
				{
					this.resetRequestedChunks.Add(StreamUtils.ReadInt64(stream));
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SaveResetRequests()
	{
		if (string.IsNullOrEmpty(this.saveDirectory))
		{
			return;
		}
		string path = Path.Combine(this.saveDirectory, "PendingResets.7pr");
		if (this.resetRequestedChunks.Count == 0)
		{
			if (SdFile.Exists(path))
			{
				SdFile.Delete(path);
			}
			return;
		}
		using (Stream stream = SdFile.OpenWrite(path))
		{
			StreamUtils.Write(stream, this.resetRequestedChunks.Count);
			foreach (long v in this.resetRequestedChunks)
			{
				StreamUtils.Write(stream, v);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnGamePrefChanged(EnumGamePrefs pref)
	{
		Dictionary<long, uint> obj = this.chunksInSaveDir;
		lock (obj)
		{
			if (pref == EnumGamePrefs.MaxChunkAge)
			{
				int @int = GamePrefs.GetInt(EnumGamePrefs.MaxChunkAge);
				this.maxChunkAge = (long)(@int * 24 * 60);
				if (this.maxChunkAge >= 0L)
				{
					ValueTuple<int, int, int> valueTuple = GameUtils.WorldTimeToElements(GameUtils.TotalMinutesToWorldTime((uint)this.maxChunkAge));
					int num = valueTuple.Item1;
					int item = valueTuple.Item2;
					int item2 = valueTuple.Item3;
					num--;
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Chunk Reset Time (MaxChunkAge) has been set to {0} days, {1} hours, and {2} minutes.", num, item, item2));
				}
				else
				{
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Chunk Reset Time (MaxChunkAge) has been disabled.");
				}
			}
			else if (pref == EnumGamePrefs.SaveDataLimit)
			{
				long limitFromPref = SaveDataLimit.GetLimitFromPref();
				if (limitFromPref >= 0L)
				{
					this.maxBytes = limitFromPref;
					if (this.maxBytes < 20971520L)
					{
						Log.Warning(string.Format("Cannot set RegionFileManager storage limit to {0} bytes as it is below the minimum value of {1} bytes. The miniumum value will be used instead.", this.maxBytes, 20971520));
						this.maxBytes = 20971520L;
					}
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Save data limit has been set to {0} bytes.", this.maxBytes));
				}
				else
				{
					this.maxBytes = -1L;
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Save data limit has been disabled.");
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetChunkTimestamp(long chunkKey, uint timeStamp)
	{
		this.chunksInSaveDir[chunkKey] = timeStamp;
		HashSetLong key;
		uint num;
		if (this.groupsByChunkKey.TryGetValue(chunkKey, out key) && (!this.groupTimestamps.TryGetValue(key, out num) || num < timeStamp))
		{
			this.groupTimestamps[key] = timeStamp;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public uint GetChunkTimestamp(long chunkKey)
	{
		HashSetLong key;
		uint result;
		if (this.groupsByChunkKey.TryGetValue(chunkKey, out key) && this.groupTimestamps.TryGetValue(key, out result))
		{
			return result;
		}
		uint result2;
		if (this.chunksInSaveDir.TryGetValue(chunkKey, out result2))
		{
			return result2;
		}
		Log.Error(string.Format("No timestamp available for chunk with key: {0}", chunkKey));
		return 0U;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ChunkProtectionLevel GetChunkProtectionLevelWithGrouping(long chunkKey)
	{
		HashSetLong key;
		ChunkProtectionLevel result;
		if (this.groupsByChunkKey.TryGetValue(chunkKey, out key) && this.groupProtectionLevels.TryGetValue(key, out result))
		{
			return result;
		}
		ChunkProtectionLevel result2;
		if (this.chunkProtectionLevels.TryGetValue(chunkKey, out result2))
		{
			return result2;
		}
		return ChunkProtectionLevel.None;
	}

	public void RebuildChunkGroupsFromPOIs()
	{
		using (RegionFileManager.s_ComputeChunkGroups.Auto())
		{
			Dictionary<long, uint> obj = this.chunksInSaveDir;
			lock (obj)
			{
				List<PrefabInstance> dynamicPrefabs = GameManager.Instance.World.ChunkCache.ChunkProvider.GetDynamicPrefabDecorator().GetDynamicPrefabs();
				this.chunkGroups.Clear();
				this.groupsByChunkKey.Clear();
				this.chunksByTraderID.Clear();
				HashSetLong hashSetLong = null;
				foreach (PrefabInstance prefabInstance in dynamicPrefabs)
				{
					if (!prefabInstance.name.Contains("rwg_tile"))
					{
						HashSetLong occupiedChunks = prefabInstance.GetOccupiedChunks();
						if (occupiedChunks.Count > 1)
						{
							if (prefabInstance.prefab.bTraderArea)
							{
								this.chunksByTraderID[prefabInstance.id] = new HashSetLong(occupiedChunks);
							}
							HashSetLong hashSetLong2 = this.MergeOrCreateChunkGroup(occupiedChunks);
							if (hashSetLong == null || hashSetLong2.Count > hashSetLong.Count)
							{
								hashSetLong = hashSetLong2;
							}
						}
					}
				}
				Log.Out(string.Format("Computed {0} chunk groups containing a total of {1} chunks. Largest group contains {2} chunks.", this.chunkGroups.Count, this.groupsByChunkKey.Count, (hashSetLong != null) ? new int?(hashSetLong.Count) : null));
			}
		}
	}

	public void AddGroupedChunks(ICollection<long> chunksToGroup)
	{
		this.MergeOrCreateChunkGroup(chunksToGroup);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSetLong MergeOrCreateChunkGroup(ICollection<long> chunksToGroup)
	{
		Dictionary<long, uint> obj = this.chunksInSaveDir;
		HashSetLong result;
		lock (obj)
		{
			HashSetLong hashSetLong = null;
			foreach (long key in chunksToGroup)
			{
				HashSetLong hashSetLong2;
				if (this.groupsByChunkKey.TryGetValue(key, out hashSetLong2))
				{
					if (hashSetLong == null)
					{
						hashSetLong = hashSetLong2;
					}
					else if (hashSetLong != hashSetLong2)
					{
						hashSetLong.UnionWith(hashSetLong2);
						this.chunkGroups.Remove(hashSetLong2);
						foreach (long key2 in hashSetLong2)
						{
							this.groupsByChunkKey[key2] = hashSetLong;
						}
					}
				}
			}
			if (hashSetLong == null)
			{
				hashSetLong = new HashSetLong(chunksToGroup);
				this.chunkGroups.Add(hashSetLong);
			}
			else
			{
				hashSetLong.UnionWith(chunksToGroup);
			}
			foreach (long key3 in chunksToGroup)
			{
				this.groupsByChunkKey[key3] = hashSetLong;
			}
			result = hashSetLong;
		}
		return result;
	}

	public override void Update()
	{
		base.Update();
	}

	public void Cleanup()
	{
		this.startSavingTask();
		this.bSaveRunning = false;
		if (this.taskInfoThreadSaveChunks != null)
		{
			this.taskInfoThreadSaveChunks.WaitForEnd();
		}
		this.ProcessChunkDeletionPackages();
		this.SaveResetRequests();
		this.regionFileAccess.Close();
		SaveDataUtils.SaveDataManager.DeregisterRegionFileManager(this);
		GamePrefs.OnGamePrefChanged -= this.OnGamePrefChanged;
		this.expiredChunks.Clear();
		this.resetRequestedChunks.Clear();
		this.chunkProtectionLevels.Clear();
		this.sortedCullingCandidates.Clear();
		this.chunksByTraderID.Clear();
		this.chunkGroups.Clear();
		this.groupsByChunkKey.Clear();
		this.groupProtectionLevels.Clear();
		this.groupTimestamps.Clear();
		this.processedChunkGroups.Clear();
		this.pendingChunkDeletionPackages.Clear();
		RegionFileManager.ProtectedPositionCache obj = this.ppdPositionCache;
		lock (obj)
		{
			this.ppdPositionCache.ClearAll();
		}
		this.snapshotUtil.Cleanup();
	}

	public void SetCacheSize(int _cacheSize)
	{
		this.maxChunksInCache = _cacheSize;
	}

	public void RequestChunkReset(long _chunkKey)
	{
		Dictionary<long, uint> obj = this.chunksInSaveDir;
		lock (obj)
		{
			HashSetLong hashSetLong;
			if (this.groupsByChunkKey.TryGetValue(_chunkKey, out hashSetLong))
			{
				using (HashSetLong.Enumerator enumerator = hashSetLong.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						long item = enumerator.Current;
						if (!this.resetRequestedChunks.Contains(item))
						{
							this.resetRequestedChunks.Add(item);
						}
					}
					return;
				}
			}
			if (!this.resetRequestedChunks.Contains(_chunkKey))
			{
				this.resetRequestedChunks.Add(_chunkKey);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CullExpiredChunks()
	{
		if (this.maxChunkAge < 0L && this.resetRequestedChunks.Count == 0)
		{
			return;
		}
		using (RegionFileManager.s_CullExpiredChunks.Auto())
		{
			object obj = this.saveLock;
			lock (obj)
			{
				Dictionary<long, uint> obj2 = this.chunksInSaveDir;
				lock (obj2)
				{
					if (this.protectionLevelsDirty)
					{
						this.UpdateChunkProtectionLevels();
					}
					if (this.maxChunkAge >= 0L)
					{
						uint num = GameUtils.WorldTimeToTotalMinutes(GameManager.Instance.World.worldTime);
						using (Dictionary<long, uint>.KeyCollection.Enumerator enumerator = this.chunksInSaveDir.Keys.GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
								long num2 = enumerator.Current;
								if (!this.resetRequestedChunks.Contains(num2))
								{
									uint chunkTimestamp = this.GetChunkTimestamp(num2);
									if ((ulong)(num - chunkTimestamp) <= (ulong)this.maxChunkAge)
									{
										continue;
									}
								}
								ChunkProtectionLevel protectionLevel;
								if (this.chunkProtectionLevels.TryGetValue(num2, out protectionLevel))
								{
									this.<CullExpiredChunks>g__UpdateResetRequest|76_0(num2, protectionLevel);
								}
								else
								{
									this.expiredChunks.Add(num2);
									this.resetRequestedChunks.Remove(num2);
									if (this.expiredChunks.Count >= 10000)
									{
										break;
									}
								}
							}
							goto IL_1A2;
						}
					}
					for (int i = this.resetRequestedChunks.Count - 1; i >= 0; i--)
					{
						long num3 = this.resetRequestedChunks[i];
						ChunkProtectionLevel protectionLevel2;
						if (this.chunkProtectionLevels.TryGetValue(num3, out protectionLevel2))
						{
							this.<CullExpiredChunks>g__UpdateResetRequest|76_0(num3, protectionLevel2);
						}
						else
						{
							this.expiredChunks.Add(num3);
							this.resetRequestedChunks.RemoveAt(i);
							if (this.expiredChunks.Count >= 10000)
							{
								break;
							}
						}
					}
					IL_1A2:
					this.RemoveChunks(this.expiredChunks, true);
					this.expiredChunks.Clear();
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void MakeRoomForChunk(long chunkSizeInBytes, long chunkKey)
	{
		object obj = this.saveLock;
		lock (obj)
		{
			Dictionary<long, uint> obj2 = this.chunksInSaveDir;
			lock (obj2)
			{
				if (this.maxBytes > 0L)
				{
					long num = (long)this.GetChunkByteCount(chunkKey);
					long num2 = chunkSizeInBytes - num;
					if (num2 > 0L)
					{
						long totalByteCount = this.regionFileAccess.GetTotalByteCount(this.saveDirectory);
						long num3 = totalByteCount + num2 - this.maxBytes;
						if (num3 > 0L)
						{
							if (chunkSizeInBytes > this.maxBytes)
							{
								throw new Exception(string.Format("Requested space ({0} bytes) exceeds maximum available space ({1} bytes).", chunkSizeInBytes, this.maxBytes));
							}
							HashSetLong hashSetLong = null;
							this.groupsByChunkKey.TryGetValue(chunkKey, out hashSetLong);
							if (this.protectionLevelsDirty)
							{
								this.UpdateChunkProtectionLevels();
							}
							using (RegionFileManager.s_SortCullingCandidates.Auto())
							{
								this.sortedCullingCandidates.Clear();
								foreach (long num4 in this.chunksInSaveDir.Keys)
								{
									if (num4 != chunkKey && (hashSetLong == null || !hashSetLong.Contains(chunkKey)))
									{
										ulong num5 = (ulong)this.GetChunkTimestamp(num4);
										ChunkProtectionLevel chunkProtectionLevel;
										if (this.chunkProtectionLevels.TryGetValue(num4, out chunkProtectionLevel))
										{
											num5 += (ulong)((ulong)((long)chunkProtectionLevel) << 32);
										}
										this.sortedCullingCandidates.Add(new KeyValuePair<long, ulong>(num4, num5));
									}
								}
								this.sortedCullingCandidates.Sort((KeyValuePair<long, ulong> a, KeyValuePair<long, ulong> b) => a.Value.CompareTo(b.Value));
							}
							long num6 = 0L;
							using (RegionFileManager.s_CollectBestCandidates.Auto())
							{
								long num7 = num3 + 5242880L;
								this.expiredChunks.Clear();
								this.processedChunkGroups.Clear();
								foreach (KeyValuePair<long, ulong> keyValuePair in this.sortedCullingCandidates)
								{
									if (num6 >= num7)
									{
										break;
									}
									if (num6 >= num3 && keyValuePair.Value >= 549755813888UL)
									{
										break;
									}
									HashSetLong hashSetLong2;
									if (this.groupsByChunkKey.TryGetValue(keyValuePair.Key, out hashSetLong2))
									{
										if (!this.processedChunkGroups.Contains(hashSetLong2))
										{
											foreach (long num8 in hashSetLong2)
											{
												if (this.chunksInSaveDir.ContainsKey(num8))
												{
													this.expiredChunks.Add(num8);
													int chunkByteCount = this.GetChunkByteCount(num8);
													num6 += (long)chunkByteCount;
												}
											}
											this.processedChunkGroups.Add(hashSetLong2);
										}
									}
									else
									{
										this.expiredChunks.Add(keyValuePair.Key);
										int chunkByteCount2 = this.GetChunkByteCount(keyValuePair.Key);
										num6 += (long)chunkByteCount2;
									}
								}
							}
							using (RegionFileManager.s_ForceCullChunks.Auto())
							{
								this.RemoveChunks(this.expiredChunks, true);
								this.expiredChunks.Clear();
							}
							if (totalByteCount - this.regionFileAccess.GetTotalByteCount(this.saveDirectory) < num3)
							{
								throw new Exception("Failed to clear as much space as requested.");
							}
						}
					}
				}
			}
		}
	}

	public bool MakeRoom(long requiredBytesToClear)
	{
		if (requiredBytesToClear <= 0L)
		{
			return true;
		}
		object obj = this.saveLock;
		lock (obj)
		{
			Dictionary<long, uint> obj2 = this.chunksInSaveDir;
			lock (obj2)
			{
				if (this.protectionLevelsDirty)
				{
					this.UpdateChunkProtectionLevels();
				}
				long totalByteCount = this.regionFileAccess.GetTotalByteCount(this.saveDirectory);
				if (requiredBytesToClear > totalByteCount)
				{
					Debug.LogError("RegionFileManager has been requested to clear more space than is currently occupied by region data. This may imply the save data limit has been set too low, or another system has exceeded its expected save data budget. Region data will be cleared, but the target will not be met.");
				}
				using (RegionFileManager.s_SortCullingCandidates.Auto())
				{
					this.sortedCullingCandidates.Clear();
					foreach (long num in this.chunksInSaveDir.Keys)
					{
						ulong num2 = (ulong)this.GetChunkTimestamp(num);
						ChunkProtectionLevel chunkProtectionLevel;
						if (this.chunkProtectionLevels.TryGetValue(num, out chunkProtectionLevel))
						{
							num2 += (ulong)((ulong)((long)chunkProtectionLevel) << 32);
						}
						this.sortedCullingCandidates.Add(new KeyValuePair<long, ulong>(num, num2));
					}
					this.sortedCullingCandidates.Sort((KeyValuePair<long, ulong> a, KeyValuePair<long, ulong> b) => a.Value.CompareTo(b.Value));
				}
				long num3 = 0L;
				using (RegionFileManager.s_CollectBestCandidates.Auto())
				{
					long num4 = requiredBytesToClear + 5242880L;
					this.expiredChunks.Clear();
					this.processedChunkGroups.Clear();
					foreach (KeyValuePair<long, ulong> keyValuePair in this.sortedCullingCandidates)
					{
						if (num3 >= num4)
						{
							break;
						}
						if (num3 >= requiredBytesToClear && keyValuePair.Value >= 549755813888UL)
						{
							break;
						}
						HashSetLong hashSetLong;
						if (this.groupsByChunkKey.TryGetValue(keyValuePair.Key, out hashSetLong))
						{
							if (!this.processedChunkGroups.Contains(hashSetLong))
							{
								foreach (long num5 in hashSetLong)
								{
									if (this.chunksInSaveDir.ContainsKey(num5))
									{
										this.expiredChunks.Add(num5);
										int chunkByteCount = this.GetChunkByteCount(num5);
										num3 += (long)chunkByteCount;
									}
								}
								this.processedChunkGroups.Add(hashSetLong);
							}
						}
						else
						{
							this.expiredChunks.Add(keyValuePair.Key);
							int chunkByteCount2 = this.GetChunkByteCount(keyValuePair.Key);
							num3 += (long)chunkByteCount2;
						}
					}
				}
				using (RegionFileManager.s_ForceCullChunks.Auto())
				{
					this.RemoveChunks(this.expiredChunks, true);
					this.expiredChunks.Clear();
				}
				long num6 = totalByteCount - this.regionFileAccess.GetTotalByteCount(this.saveDirectory);
				if (num6 < requiredBytesToClear)
				{
					Debug.LogError("Failed to clear as much space as requested." + string.Format("\nCleared: {0} ({1:0.00} MB).", num6, (float)num6 / 1048576f) + string.Format("\nRequested: {0} ({1:0.00} MB).", requiredBytesToClear, (float)requiredBytesToClear / 1048576f));
					return false;
				}
			}
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int GetChunkByteCount(long cullingCandidate)
	{
		int chunkX = WorldChunkCache.extractX(cullingCandidate);
		int chunkZ = WorldChunkCache.extractZ(cullingCandidate);
		return this.GetChunkByteCount(chunkX, chunkZ);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int GetChunkByteCount(int chunkX, int chunkZ)
	{
		return this.regionFileAccess.GetChunkByteCount(this.saveDirectory, chunkX, chunkZ);
	}

	public void MainThreadCacheProtectedPositions()
	{
		if (!ThreadManager.IsMainThread())
		{
			Debug.LogError("RegionFileManager.MainThreadCacheProtectedPositions called from a secondary thread.");
			return;
		}
		using (RegionFileManager.s_UpdateProtectionLevels.Auto())
		{
			RegionFileManager.ProtectedPositionCache obj = this.ppdPositionCache;
			lock (obj)
			{
				this.ppdPositionCache.ClearAll();
				double num = (double)GameStats.GetInt(EnumGameStats.LandClaimExpiryTime) * 24.0;
				double num2 = (double)GameStats.GetInt(EnumGameStats.BedrollExpiryTime) * 24.0;
				foreach (PersistentPlayerData persistentPlayerData in ((IDictionary<PlatformUserIdentifierAbs, PersistentPlayerData>)GameManager.Instance.GetPersistentPlayerList().Players).Values)
				{
					if (persistentPlayerData.HasBedrollPos && persistentPlayerData.OfflineHours < num2)
					{
						this.ppdPositionCache.bedrolls.Add(persistentPlayerData.BedrollPos);
					}
					if (persistentPlayerData.OfflineHours < num)
					{
						foreach (Vector3i item in persistentPlayerData.GetLandProtectionBlocks())
						{
							this.ppdPositionCache.lpBlocks.Add(item);
						}
					}
					if (persistentPlayerData.EntityId == -1)
					{
						this.ppdPositionCache.offlinePlayers.Add(persistentPlayerData.Position);
					}
					if (persistentPlayerData.OfflineHours < num)
					{
						persistentPlayerData.ProcessBackpacks(delegate(PersistentPlayerData.ProtectedBackpack backpack)
						{
							this.ppdPositionCache.backpacks.Add(backpack.Position);
						});
					}
					foreach (QuestPositionData questPositionData in persistentPlayerData.QuestPositions)
					{
						this.ppdPositionCache.quests.Add(questPositionData.blockPosition);
					}
					foreach (Vector3i item2 in persistentPlayerData.OwnedVendingMachinePositions)
					{
						this.ppdPositionCache.vendingMachines.Add(item2);
					}
				}
				foreach (EntityCreationData entityCreationData in VehicleManager.Instance.GetVehicleList())
				{
					this.ppdPositionCache.vendingMachines.Add(World.worldToBlockPos(entityCreationData.pos));
				}
				foreach (AIDirectorAirDropComponent.SupplyCrateCache supplyCrateCache in GameManager.Instance.World.aiDirector.GetComponent<AIDirectorAirDropComponent>().supplyCrates)
				{
					this.ppdPositionCache.supplyCrates.Add(supplyCrateCache.blockPos);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateChunkProtectionLevels()
	{
		using (RegionFileManager.s_UpdateProtectionLevels.Auto())
		{
			this.chunkProtectionLevels.Clear();
			this.groupProtectionLevels.Clear();
			this.<UpdateChunkProtectionLevels>g__EvaluateSingleChunkProtectionLevels|84_0();
			foreach (KeyValuePair<HashSetLong, ChunkProtectionLevel> keyValuePair in this.groupProtectionLevels)
			{
				foreach (long num in keyValuePair.Key)
				{
					ChunkProtectionLevel chunkProtectionLevel;
					if (this.chunkProtectionLevels.TryGetValue(num, out chunkProtectionLevel) && chunkProtectionLevel != (chunkProtectionLevel & keyValuePair.Value))
					{
						Log.Error(string.Format("Error in chunk group protection: member has protections not accounted for by the group. chunkKey: {0}, protectionLevel: {1}, groupProtectionLevel: {2})", num, chunkProtectionLevel, keyValuePair.Value));
					}
					this.chunkProtectionLevels[num] = keyValuePair.Value;
				}
			}
			this.protectionLevelsDirty = false;
			RegionFileManager.s_ProtectedChunkCount.Value = this.chunkProtectionLevels.Count;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool DoSaveChunks()
	{
		IRegionFileChunkSnapshot regionFileChunkSnapshot = null;
		try
		{
			List<Chunk> obj = this.chunksToUnloadLater;
			lock (obj)
			{
				for (int i = this.chunksToUnloadLater.Count - 1; i >= 0; i--)
				{
					if (!this.chunksToUnloadLater[i].IsLockedExceptUnloading)
					{
						MemoryPools.PoolChunks.FreeSync(this.chunksToUnloadLater[i]);
						this.chunksToUnloadLater.RemoveAt(i);
					}
				}
			}
			this.protectionLevelsDirty = true;
			this.CullExpiredChunks();
			if (this.chunkMemoryStreamsToSave.Count == 0 && this.chunksToSave.Count == 0)
			{
				return false;
			}
			long num = 0L;
			Dictionary<long, IRegionFileChunkSnapshot> obj2 = this.chunkMemoryStreamsToSave;
			lock (obj2)
			{
				if (this.chunkMemoryStreamsToSave.Count > 0)
				{
					using (Dictionary<long, IRegionFileChunkSnapshot>.Enumerator enumerator = this.chunkMemoryStreamsToSave.GetEnumerator())
					{
						if (enumerator.MoveNext())
						{
							KeyValuePair<long, IRegionFileChunkSnapshot> keyValuePair = enumerator.Current;
							num = keyValuePair.Key;
							regionFileChunkSnapshot = this.chunkMemoryStreamsToSave[num];
							this.chunkMemoryStreamsToSave.Remove(num);
						}
					}
				}
			}
			if (regionFileChunkSnapshot == null)
			{
				Chunk chunk = null;
				Dictionary<long, Chunk> obj3 = this.chunksToSave;
				lock (obj3)
				{
					using (Dictionary<long, Chunk>.Enumerator enumerator2 = this.chunksToSave.GetEnumerator())
					{
						if (enumerator2.MoveNext())
						{
							KeyValuePair<long, Chunk> keyValuePair2 = enumerator2.Current;
							chunk = this.chunksToSave[keyValuePair2.Key];
							this.chunksToSave.Remove(keyValuePair2.Key);
							this.chunkKeyCurrentlySaved = keyValuePair2.Key;
						}
					}
				}
				if (chunk != null)
				{
					num = chunk.Key;
					regionFileChunkSnapshot = this.snapshotUtil.TakeSnapshot(chunk, false);
					chunk.InProgressSaving = false;
					if (chunk.IsLockedExceptUnloading)
					{
						obj = this.chunksToUnloadLater;
						lock (obj)
						{
							this.chunksToUnloadLater.Add(chunk);
							goto IL_221;
						}
					}
					MemoryPools.PoolChunks.FreeSync(chunk);
				}
			}
			IL_221:
			if (regionFileChunkSnapshot != null)
			{
				int chunkX = WorldChunkCache.extractX(num);
				int chunkZ = WorldChunkCache.extractZ(num);
				this.MakeRoomForChunk(regionFileChunkSnapshot.Size, num);
				this.snapshotUtil.WriteSnapshot(regionFileChunkSnapshot, this.saveDirectory, chunkX, chunkZ);
				Dictionary<long, uint> obj4 = this.chunksInSaveDir;
				lock (obj4)
				{
					uint timeStamp = GameUtils.WorldTimeToTotalMinutes(GameManager.Instance.World.worldTime);
					this.SetChunkTimestamp(num, timeStamp);
					this.chunkKeyCurrentlySaved = long.MaxValue;
				}
				RegionFileManager.s_RegionDataSize.Value = this.regionFileAccess.GetTotalByteCount(this.saveDirectory);
			}
			MultiBlockManager.Instance.SaveIfDirty();
		}
		catch (Exception ex)
		{
			string str = "ERROR: ";
			Exception ex2 = ex;
			Log.Error(str + ((ex2 != null) ? ex2.ToString() : null));
			return false;
		}
		finally
		{
			if (regionFileChunkSnapshot != null)
			{
				this.snapshotUtil.Free(regionFileChunkSnapshot);
			}
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int thread_SaveChunks(ThreadManager.ThreadInfo _taskInfo)
	{
		if (this.bSaveRunning)
		{
			if (this.thread_SaveChunks_SleepCount >= 40 || this.saveThreadWaitHandle.WaitOne(0))
			{
				for (;;)
				{
					object obj = this.saveLock;
					lock (obj)
					{
						if (this.DoSaveChunks())
						{
							continue;
						}
					}
					break;
				}
				this.thread_SaveChunks_SleepCount = 0;
			}
			return 5;
		}
		return -1;
	}

	public override Chunk GetChunkSync(long _key)
	{
		Chunk chunk = null;
		bool flag = false;
		if (base.ContainsChunkSync(_key))
		{
			chunk = base.GetChunkSync(_key);
			flag = true;
		}
		IRegionFileChunkSnapshot snapshot = null;
		if (chunk == null)
		{
			Dictionary<long, IRegionFileChunkSnapshot> obj = this.chunkMemoryStreamsToSave;
			lock (obj)
			{
				if (this.chunkMemoryStreamsToSave.ContainsKey(_key))
				{
					snapshot = this.chunkMemoryStreamsToSave[_key];
					this.chunkMemoryStreamsToSave.Remove(_key);
				}
			}
			this.snapshotUtil.Free(snapshot);
		}
		if (chunk == null)
		{
			Dictionary<long, Chunk> obj2 = this.chunksToSave;
			lock (obj2)
			{
				if (this.chunksToSave.ContainsKey(_key))
				{
					chunk = this.chunksToSave[_key];
					this.chunksToSave.Remove(_key);
					chunk.OnLoadedFromCache();
				}
			}
		}
		if (chunk == null)
		{
			bool flag3 = false;
			do
			{
				Dictionary<long, Chunk> obj2 = this.chunksToSave;
				lock (obj2)
				{
					flag3 = (_key == this.chunkKeyCurrentlySaved);
				}
				if (flag3)
				{
					Thread.Sleep(5);
				}
			}
			while (flag3);
		}
		if (chunk == null)
		{
			if (this.isChunkInSaveDir(_key))
			{
				chunk = this.snapshotUtil.LoadChunk(this.saveDirectory, _key);
				if (chunk != null)
				{
					goto IL_180;
				}
				Dictionary<long, uint> obj3 = this.chunksInSaveDir;
				lock (obj3)
				{
					this.chunksInSaveDir.Remove(_key);
					goto IL_180;
				}
			}
			if (this.isChunkInLoadDir(_key))
			{
				chunk = this.snapshotUtil.LoadChunk(this.loadDirectory, _key);
				if (chunk == null)
				{
					this.chunksInLoadDir.Remove(_key);
				}
			}
		}
		IL_180:
		if (chunk != null && !flag && this.maxChunksInCache > 0)
		{
			this.cacheChunk(chunk);
		}
		return chunk;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void cacheChunk(Chunk _chunk)
	{
		if (this.maxChunksInCache > 0)
		{
			base.AddChunkSync(_chunk, false);
			List<long> obj = this.chunksInLocalCache;
			long key;
			lock (obj)
			{
				this.chunksInLocalCache.Add(_chunk.Key);
				if (this.chunksInLocalCache.Count <= this.maxChunksInCache)
				{
					return;
				}
				key = this.chunksInLocalCache[0];
			}
			_chunk = base.GetChunkSync(key);
			this.RemoveChunkSync(key);
		}
		if (this.bSaveOnChunkDrop && _chunk.NeedsSaving && !_chunk.InProgressSaving)
		{
			_chunk.InProgressSaving = true;
			Dictionary<long, Chunk> obj2 = this.chunksToSave;
			lock (obj2)
			{
				if (!this.chunksToSave.ContainsKey(_chunk.Key))
				{
					this.chunksToSave.Add(_chunk.Key, _chunk);
					this.startSavingTask();
				}
				return;
			}
		}
		if (_chunk.IsLockedExceptUnloading)
		{
			List<Chunk> obj3 = this.chunksToUnloadLater;
			lock (obj3)
			{
				this.chunksToUnloadLater.Add(_chunk);
				return;
			}
		}
		MemoryPools.PoolChunks.FreeSync(_chunk);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void startSavingTask()
	{
		this.saveThreadWaitHandle.Set();
	}

	public override bool AddChunkSync(Chunk _chunk, bool _bOmitCallbacks = false)
	{
		if (!GameManager.bSavingActive || _chunk.NeedsDecoration)
		{
			MemoryPools.PoolChunks.FreeSync(_chunk);
			return false;
		}
		if (!base.ContainsChunkSync(_chunk.Key))
		{
			this.cacheChunk(_chunk);
		}
		return true;
	}

	public override bool ContainsChunkSync(long key)
	{
		if (base.ContainsChunkSync(key))
		{
			return true;
		}
		Dictionary<long, Chunk> obj = this.chunksToSave;
		lock (obj)
		{
			if (this.chunksToSave.ContainsKey(key))
			{
				return true;
			}
			if (this.chunkKeyCurrentlySaved == key)
			{
				return true;
			}
		}
		return this.isChunkInSaveDir(key) || this.isChunkInLoadDir(key);
	}

	public bool IsChunkSavedAndDormant(long key)
	{
		Dictionary<long, uint> obj = this.chunksInSaveDir;
		bool result;
		lock (obj)
		{
			if (!this.chunksInSaveDir.ContainsKey(key))
			{
				result = false;
			}
			else if (base.ContainsChunkSync(key))
			{
				result = false;
			}
			else
			{
				Dictionary<long, Chunk> obj2 = this.chunksToSave;
				lock (obj2)
				{
					if (this.chunksToSave.ContainsKey(key))
					{
						return false;
					}
					if (this.chunkKeyCurrentlySaved == key)
					{
						return false;
					}
				}
				result = true;
			}
		}
		return result;
	}

	public override void RemoveChunkSync(long key)
	{
		if (base.ContainsChunkSync(key))
		{
			base.RemoveChunkSync(key);
		}
		List<long> obj = this.chunksInLocalCache;
		lock (obj)
		{
			this.chunksInLocalCache.Remove(key);
		}
	}

	public int MakePersistent(ChunkCluster _mainCache, bool _bSaveEvenIfUnchanged)
	{
		List<Chunk> list = new List<Chunk>();
		List<Chunk> chunkArrayCopySync = base.GetChunkArrayCopySync();
		for (int i = 0; i < chunkArrayCopySync.Count; i++)
		{
			Chunk chunk = chunkArrayCopySync[i];
			if (!chunk.NeedsDecoration && (chunk.NeedsSaving || _bSaveEvenIfUnchanged))
			{
				Dictionary<long, Chunk> obj = this.chunksToSave;
				lock (obj)
				{
					if (this.chunksToSave.ContainsKey(chunk.Key))
					{
						goto IL_75;
					}
				}
				chunk.InProgressSaving = true;
				list.Add(chunk);
			}
			IL_75:;
		}
		if (_mainCache != null)
		{
			List<Chunk> chunkArrayCopySync2 = _mainCache.GetChunkArrayCopySync();
			int j = 0;
			while (j < chunkArrayCopySync2.Count)
			{
				Chunk chunk2 = chunkArrayCopySync2[j];
				Chunk obj2 = chunk2;
				lock (obj2)
				{
					if (chunk2.InProgressUnloading)
					{
						goto IL_107;
					}
					chunk2.InProgressSaving = true;
				}
				goto IL_D5;
				IL_107:
				j++;
				continue;
				IL_D5:
				_mainCache.NotifyOnChunkBeforeSave(chunk2);
				if (!chunk2.NeedsDecoration && (chunk2.NeedsSaving || _bSaveEvenIfUnchanged))
				{
					list.Add(chunk2);
					goto IL_107;
				}
				chunk2.InProgressSaving = false;
				goto IL_107;
			}
		}
		for (int k = 0; k < list.Count; k++)
		{
			this.SaveChunkSnapshot(list[k], _bSaveEvenIfUnchanged);
			list[k].InProgressSaving = false;
		}
		this.startSavingTask();
		return list.Count;
	}

	public void WaitSaveDone()
	{
		MicroStopwatch microStopwatch = new MicroStopwatch();
		int count = this.chunkMemoryStreamsToSave.Count;
		int num = 0;
		do
		{
			Dictionary<long, IRegionFileChunkSnapshot> obj = this.chunkMemoryStreamsToSave;
			lock (obj)
			{
				num = this.chunkMemoryStreamsToSave.Count;
			}
			Dictionary<long, Chunk> obj2 = this.chunksToSave;
			lock (obj2)
			{
				num += this.chunksToSave.Count;
			}
			Thread.Sleep(20);
		}
		while (num > 0);
		Log.Out(string.Concat(new string[]
		{
			"Saving ",
			count.ToString(),
			" of chunks took ",
			microStopwatch.ElapsedMilliseconds.ToString(),
			"ms"
		}));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isChunkInLoadDir(long _key)
	{
		return this.chunksInLoadDir.Contains(_key);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isChunkInSaveDir(long _key)
	{
		Dictionary<long, uint> obj = this.chunksInSaveDir;
		bool result;
		lock (obj)
		{
			result = this.chunksInSaveDir.ContainsKey(_key);
		}
		return result;
	}

	public virtual void SaveChunkSnapshot(Chunk _chunk, bool _saveIfUnchanged)
	{
		if (this.saveDirectory == null)
		{
			return;
		}
		Dictionary<long, IRegionFileChunkSnapshot> obj = this.chunkMemoryStreamsToSave;
		lock (obj)
		{
			IRegionFileChunkSnapshot regionFileChunkSnapshot;
			if (this.chunkMemoryStreamsToSave.TryGetValue(_chunk.Key, out regionFileChunkSnapshot))
			{
				regionFileChunkSnapshot.Update(_chunk, _saveIfUnchanged);
			}
			else
			{
				this.chunkMemoryStreamsToSave.Add(_chunk.Key, this.snapshotUtil.TakeSnapshot(_chunk, _saveIfUnchanged));
			}
		}
		this.startSavingTask();
	}

	public long[] GetAllChunkKeys()
	{
		HashSetLong hashSetLong = new HashSetLong();
		foreach (long item in this.chunksInLoadDir)
		{
			hashSetLong.Add(item);
		}
		Dictionary<long, uint> obj = this.chunksInSaveDir;
		lock (obj)
		{
			foreach (long item2 in this.chunksInSaveDir.Keys)
			{
				hashSetLong.Add(item2);
			}
		}
		long[] array = new long[hashSetLong.Count];
		hashSetLong.CopyTo(array);
		return array;
	}

	public HashSetLong GetUniqueChunkKeys(ChunkProtectionLevel excludedProtectionLevels)
	{
		HashSetLong hashSetLong = new HashSetLong();
		foreach (long num in this.chunksInLoadDir)
		{
			ChunkProtectionLevel chunkProtectionLevel;
			if (!this.chunkProtectionLevels.TryGetValue(num, out chunkProtectionLevel) || (chunkProtectionLevel & excludedProtectionLevels) == ChunkProtectionLevel.None)
			{
				hashSetLong.Add(num);
			}
		}
		Dictionary<long, uint> obj = this.chunksInSaveDir;
		lock (obj)
		{
			foreach (long num2 in this.chunksInSaveDir.Keys)
			{
				ChunkProtectionLevel chunkProtectionLevel2;
				if (!this.chunkProtectionLevels.TryGetValue(num2, out chunkProtectionLevel2) || (chunkProtectionLevel2 & excludedProtectionLevels) == ChunkProtectionLevel.None)
				{
					hashSetLong.Add(num2);
				}
			}
		}
		return hashSetLong;
	}

	public override void Clear()
	{
		base.Clear();
	}

	public void ClearCaches()
	{
		this.regionFileAccess.ClearCache();
	}

	public void RemoveChunks(ICollection<long> _chunks, bool _resetDecos = true)
	{
		if (_chunks.Count == 0)
		{
			return;
		}
		if (Monitor.IsEntered(this.chunksInSaveDir) && !Monitor.IsEntered(this.saveLock))
		{
			Debug.LogError("RemoveChunks failed. Thread safety violation: the lock on \"saveLock\" must be taken before the lock on \"chunksInSaveDir\" to avoid a possible deadlock.");
			return;
		}
		object obj = this.saveLock;
		lock (obj)
		{
			Dictionary<long, uint> obj2 = this.chunksInSaveDir;
			lock (obj2)
			{
				RegionFileManager.RemovePersistentDataForChunks(_chunks);
				RegionFileManager.resetVolumeDataForChunks(_chunks);
				foreach (long key in _chunks)
				{
					this.<RemoveChunks>g__RemoveChunk|105_0(key, _resetDecos);
				}
				MultiBlockManager.Instance.CullChunklessData();
				List<NetPackageDeleteChunkData> obj3 = this.pendingChunkDeletionPackages;
				lock (obj3)
				{
					NetPackageDeleteChunkData item = NetPackageManager.GetPackage<NetPackageDeleteChunkData>().Setup(_chunks);
					this.pendingChunkDeletionPackages.Add(item);
				}
				ThreadManager.AddSingleTaskMainThread("RemoveChunks.ProcessChunkDeletionPackages", delegate
				{
					this.ProcessChunkDeletionPackages();
				}, null);
				this.regionFileAccess.OptimizeLayouts();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ProcessChunkDeletionPackages()
	{
		List<NetPackageDeleteChunkData> obj = this.pendingChunkDeletionPackages;
		lock (obj)
		{
			foreach (NetPackageDeleteChunkData netPackageDeleteChunkData in this.pendingChunkDeletionPackages)
			{
				DynamicMeshUnity.DeleteDynamicMeshData(netPackageDeleteChunkData.chunkKeys);
				WaterSimulationNative.Instance.changeApplier.DiscardChangesForChunks(netPackageDeleteChunkData.chunkKeys);
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(netPackageDeleteChunkData, false, -1, -1, -1, null, 192);
			}
			this.pendingChunkDeletionPackages.Clear();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void RemovePersistentDataForChunks(ICollection<long> _chunks)
	{
		if (_chunks.Count == 0)
		{
			return;
		}
		foreach (PersistentPlayerData persistentPlayerData in GameManager.Instance.persistentPlayers.Players.Values)
		{
			bool backpacksChanged = false;
			persistentPlayerData.RemoveBackpacks(delegate(PersistentPlayerData.ProtectedBackpack backpack)
			{
				long num3 = WorldChunkCache.MakeChunkKey(World.toChunkXZ(backpack.Position.x), World.toChunkXZ(backpack.Position.z));
				using (IEnumerator<long> enumerator6 = _chunks.GetEnumerator())
				{
					while (enumerator6.MoveNext())
					{
						if (enumerator6.Current == num3)
						{
							backpacksChanged = true;
							return true;
						}
					}
				}
				return false;
			});
			if (backpacksChanged && persistentPlayerData.EntityId != -1)
			{
				EntityPlayer entityPlayer = GameManager.Instance.World.GetEntity(persistentPlayerData.EntityId) as EntityPlayer;
				if (entityPlayer != null)
				{
					if (!entityPlayer.isEntityRemote)
					{
						entityPlayer.SetDroppedBackpackPositions(persistentPlayerData.GetDroppedBackpackPositions());
					}
					else
					{
						SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackagePlayerSetBackpackPosition>().Setup(persistentPlayerData.EntityId, persistentPlayerData.GetDroppedBackpackPositions()), false, persistentPlayerData.EntityId, -1, -1, null, 192);
					}
				}
			}
		}
		foreach (EntityCreationData entityCreationData in VehicleManager.Instance.GetVehicleList())
		{
			Vector3i vector3i = World.worldToBlockPos(entityCreationData.pos);
			long num = WorldChunkCache.MakeChunkKey(World.toChunkXZ(vector3i.x), World.toChunkXZ(vector3i.z));
			using (IEnumerator<long> enumerator3 = _chunks.GetEnumerator())
			{
				while (enumerator3.MoveNext())
				{
					if (enumerator3.Current == num)
					{
						VehicleManager.Instance.RemoveUnloadedVehicle(entityCreationData.id);
					}
				}
			}
		}
		AIDirectorAirDropComponent component = GameManager.Instance.World.aiDirector.GetComponent<AIDirectorAirDropComponent>();
		List<int> list = new List<int>();
		foreach (AIDirectorAirDropComponent.SupplyCrateCache supplyCrateCache in component.supplyCrates)
		{
			long num2 = WorldChunkCache.MakeChunkKey(World.toChunkXZ(supplyCrateCache.blockPos.x), World.toChunkXZ(supplyCrateCache.blockPos.z));
			using (IEnumerator<long> enumerator3 = _chunks.GetEnumerator())
			{
				while (enumerator3.MoveNext())
				{
					if (enumerator3.Current == num2)
					{
						list.Add(supplyCrateCache.entityId);
					}
				}
			}
		}
		foreach (int entityId in list)
		{
			component.RemoveSupplyCrate(entityId);
		}
		PowerManager.Instance.RemoveUnloadedPowerNodes(_chunks);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void resetVolumeDataForChunks(ICollection<long> _chunks)
	{
		World world = GameManager.Instance.World;
		foreach (long chunkKey in _chunks)
		{
			world.ResetTriggerVolumes(chunkKey);
			world.ResetSleeperVolumes(chunkKey);
		}
	}

	public HashSetLong ResetAllChunks(ChunkProtectionLevel excludedProtectionLevels)
	{
		object obj = this.saveLock;
		HashSetLong result;
		lock (obj)
		{
			Dictionary<long, uint> obj2 = this.chunksInSaveDir;
			lock (obj2)
			{
				this.UpdateChunkProtectionLevels();
				HashSetLong uniqueChunkKeys = this.GetUniqueChunkKeys(excludedProtectionLevels);
				this.RemoveChunks(uniqueChunkKeys, true);
				result = uniqueChunkKeys;
			}
		}
		return result;
	}

	public HashSetLong ResetRegion(int _regionX, int _regionZ, ChunkProtectionLevel excludedProtectionLevels)
	{
		object obj = this.saveLock;
		HashSetLong result;
		lock (obj)
		{
			Dictionary<long, uint> obj2 = this.chunksInSaveDir;
			lock (obj2)
			{
				this.UpdateChunkProtectionLevels();
				int chunksPerRegionPerDimension = this.regionFileAccess.ChunksPerRegionPerDimension;
				Vector2i vector2i = new Vector2i(_regionX * chunksPerRegionPerDimension, _regionZ * chunksPerRegionPerDimension);
				Vector2i vector2i2 = vector2i + new Vector2i(chunksPerRegionPerDimension - 1, chunksPerRegionPerDimension - 1);
				HashSetLong hashSetLong = new HashSetLong();
				for (int i = vector2i.x; i <= vector2i2.x; i++)
				{
					for (int j = vector2i.y; j <= vector2i2.y; j++)
					{
						long num = WorldChunkCache.MakeChunkKey(i, j);
						ChunkProtectionLevel chunkProtectionLevel;
						if (this.ContainsChunkSync(num) && (!this.chunkProtectionLevels.TryGetValue(num, out chunkProtectionLevel) || (chunkProtectionLevel & excludedProtectionLevels) == ChunkProtectionLevel.None))
						{
							hashSetLong.Add(num);
						}
					}
				}
				this.RemoveChunks(hashSetLong, true);
				result = hashSetLong;
			}
		}
		return result;
	}

	public void IterateChunkExpiryTimes(Action<long, ulong> action)
	{
		if (this.maxChunkAge < 0L)
		{
			return;
		}
		Dictionary<long, uint> obj = this.chunksInSaveDir;
		lock (obj)
		{
			this.UpdateChunkProtectionLevels();
			foreach (KeyValuePair<long, uint> keyValuePair in this.chunksInSaveDir)
			{
				ChunkProtectionLevel chunkProtectionLevel;
				if (!this.chunkProtectionLevels.TryGetValue(keyValuePair.Key, out chunkProtectionLevel))
				{
					action(keyValuePair.Key, GameUtils.TotalMinutesToWorldTime(keyValuePair.Value + (uint)this.maxChunkAge));
				}
			}
		}
	}

	public void SaveChunkAgeDebugTexture(float rangeInDays)
	{
		float num = rangeInDays * 24f * 60f;
		Vector2i worldSize = GameManager.Instance.World.ChunkCache.ChunkProvider.GetWorldSize();
		Vector2i vector2i = new Vector2i(worldSize.x / 16, worldSize.y / 16);
		int num2 = vector2i.x / 2;
		int num3 = vector2i.y / 2;
		uint num4 = GameUtils.WorldTimeToTotalMinutes(GameManager.Instance.World.worldTime);
		Color32[] array = new Color32[vector2i.x * vector2i.y];
		Dictionary<long, uint> obj = this.chunksInSaveDir;
		lock (obj)
		{
			this.UpdateChunkProtectionLevels();
			float num5 = 1f / (float)RegionFileManager.<SaveChunkAgeDebugTexture>g__OrdinalProtectionLevel|112_0(ChunkProtectionLevel.CurrentlySynced);
			for (int i = 0; i < array.Length; i++)
			{
				int x = i % vector2i.x - num2;
				int y = i / vector2i.x - num3;
				long key = WorldChunkCache.MakeChunkKey(x, y);
				ValueTuple<float, float, float, float> valueTuple = new ValueTuple<float, float, float, float>(0f, 0f, 0f, 0f);
				uint num6;
				if (this.chunksInSaveDir.TryGetValue(key, out num6))
				{
					valueTuple.Item4 = 1f;
					long num7 = (long)((ulong)(num4 - num6));
					valueTuple.Item2 = (float)num7 / num;
				}
				HashSetLong key2;
				uint num8;
				if (this.groupsByChunkKey.TryGetValue(key, out key2) && this.groupTimestamps.TryGetValue(key2, out num8))
				{
					long num9 = (long)((ulong)(num4 - num8));
					valueTuple.Item1 = (float)num9 / num;
				}
				else
				{
					valueTuple.Item1 = valueTuple.Item2;
				}
				ChunkProtectionLevel level;
				if (this.chunkProtectionLevels.TryGetValue(key, out level))
				{
					valueTuple.Item3 = (float)RegionFileManager.<SaveChunkAgeDebugTexture>g__OrdinalProtectionLevel|112_0(level) * num5;
				}
				array[i] = new Color(valueTuple.Item1, valueTuple.Item2, valueTuple.Item3, valueTuple.Item4);
			}
		}
		Texture2D texture2D = new Texture2D(vector2i.x, vector2i.y);
		texture2D.SetPixels32(array);
		texture2D.Apply();
		byte[] bytes = texture2D.EncodeToTGA();
		SdFile.WriteAllBytes("ChunkAgeMap.tga", bytes);
		UnityEngine.Object.Destroy(texture2D);
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public void <CullExpiredChunks>g__UpdateResetRequest|76_0(long chunkKey, ChunkProtectionLevel protectionLevel)
	{
		if (!protectionLevel.HasFlag(ChunkProtectionLevel.CurrentlySynced) && !protectionLevel.HasFlag(ChunkProtectionLevel.OfflinePlayer) && !protectionLevel.HasFlag(ChunkProtectionLevel.NearOfflinePlayer) && !protectionLevel.HasFlag(ChunkProtectionLevel.QuestObjective) && !protectionLevel.HasFlag(ChunkProtectionLevel.NearQuestObjective))
		{
			this.resetRequestedChunks.Remove(chunkKey);
		}
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public void <UpdateChunkProtectionLevels>g__EvaluateSingleChunkProtectionLevels|84_0()
	{
		int halfSize = GamePrefs.GetInt(EnumGamePrefs.LandClaimSize) / 2;
		int @int = GamePrefs.GetInt(EnumGamePrefs.BedrollDeadZoneSize);
		ChunkCluster chunkCache = GameManager.Instance.World.ChunkCache;
		if (chunkCache != null)
		{
			chunkCache.GetSyncRoot().EnterReadLock();
			foreach (long chunkKey in chunkCache.chunkKeys)
			{
				this.<UpdateChunkProtectionLevels>g__AddProtectionLevel|84_3(chunkKey, ChunkProtectionLevel.CurrentlySynced);
			}
			chunkCache.GetSyncRoot().ExitReadLock();
		}
		GameManager.Instance.World.m_ChunkManager.ProcessChunksPendingUnload(delegate(Chunk chunk)
		{
			this.<UpdateChunkProtectionLevels>g__AddProtectionLevel|84_3(chunk.Key, ChunkProtectionLevel.CurrentlySynced);
		});
		Dictionary<long, Chunk> obj = this.chunksToSave;
		lock (obj)
		{
			foreach (long chunkKey2 in this.chunksToSave.Keys)
			{
				this.<UpdateChunkProtectionLevels>g__AddProtectionLevel|84_3(chunkKey2, ChunkProtectionLevel.CurrentlySynced);
			}
		}
		Dictionary<long, IRegionFileChunkSnapshot> obj2 = this.chunkMemoryStreamsToSave;
		lock (obj2)
		{
			foreach (long chunkKey3 in this.chunkMemoryStreamsToSave.Keys)
			{
				this.<UpdateChunkProtectionLevels>g__AddProtectionLevel|84_3(chunkKey3, ChunkProtectionLevel.CurrentlySynced);
			}
		}
		foreach (HashSetLong hashSetLong in this.chunksByTraderID.Values)
		{
			using (HashSetLong.Enumerator enumerator = hashSetLong.GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					long chunkKey4 = enumerator.Current;
					this.<UpdateChunkProtectionLevels>g__AddProtectionLevel|84_3(chunkKey4, ChunkProtectionLevel.Trader);
				}
			}
		}
		RegionFileManager.ProtectedPositionCache obj3 = this.ppdPositionCache;
		lock (obj3)
		{
			this.<UpdateChunkProtectionLevels>g__ProcessPotectedPositionList|84_1(this.ppdPositionCache.bedrolls, @int, ChunkProtectionLevel.Bedroll, 1, ChunkProtectionLevel.NearBedroll);
			this.<UpdateChunkProtectionLevels>g__ProcessPotectedPositionList|84_1(this.ppdPositionCache.lpBlocks, halfSize, ChunkProtectionLevel.LandClaim, 1, ChunkProtectionLevel.NearLandClaim);
			this.<UpdateChunkProtectionLevels>g__ProcessPotectedPositionList|84_1(this.ppdPositionCache.offlinePlayers, 0, ChunkProtectionLevel.OfflinePlayer, 1, ChunkProtectionLevel.NearOfflinePlayer);
			this.<UpdateChunkProtectionLevels>g__ProcessPotectedPositionList|84_1(this.ppdPositionCache.backpacks, 0, ChunkProtectionLevel.DroppedBackpack, 1, ChunkProtectionLevel.NearDroppedBackpack);
			this.<UpdateChunkProtectionLevels>g__ProcessPotectedPositionList|84_1(this.ppdPositionCache.quests, halfSize, ChunkProtectionLevel.QuestObjective, 1, ChunkProtectionLevel.NearQuestObjective);
			this.<UpdateChunkProtectionLevels>g__ProcessPotectedPositionList|84_1(this.ppdPositionCache.vendingMachines, halfSize, ChunkProtectionLevel.Trader, 1, ChunkProtectionLevel.Trader);
			this.<UpdateChunkProtectionLevels>g__ProcessPotectedPositionList|84_1(this.ppdPositionCache.vehicles, halfSize, ChunkProtectionLevel.Vehicle, 1, ChunkProtectionLevel.NearVehicle);
			this.<UpdateChunkProtectionLevels>g__ProcessPotectedPositionList|84_1(this.ppdPositionCache.supplyCrates, halfSize, ChunkProtectionLevel.SupplyCrate, 1, ChunkProtectionLevel.NearSupplyCrate);
		}
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public void <UpdateChunkProtectionLevels>g__ProcessPotectedPositionList|84_1(List<Vector3i> centerPositions, int halfSize, ChunkProtectionLevel innerProtectionLevel, int margin, ChunkProtectionLevel marginProtectionLevel)
	{
		foreach (Vector3i centerPos in centerPositions)
		{
			this.<UpdateChunkProtectionLevels>g__AddChunksInWorldArea|84_2(centerPos, halfSize, innerProtectionLevel, margin, marginProtectionLevel);
		}
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public void <UpdateChunkProtectionLevels>g__AddChunksInWorldArea|84_2(Vector3i centerPos, int halfSize, ChunkProtectionLevel innerProtectionLevel, int margin, ChunkProtectionLevel marginProtectionLevel)
	{
		int num = World.toChunkXZ(centerPos.x - halfSize);
		int num2 = World.toChunkXZ(centerPos.x + halfSize);
		int num3 = World.toChunkXZ(centerPos.z - halfSize);
		int num4 = World.toChunkXZ(centerPos.z + halfSize);
		for (int i = num - margin; i <= num2 + margin; i++)
		{
			for (int j = num3 - margin; j <= num4 + margin; j++)
			{
				ChunkProtectionLevel protectionLevel = (i < num || i > num2 || j < num3 || j > num4) ? marginProtectionLevel : innerProtectionLevel;
				long chunkKey = WorldChunkCache.MakeChunkKey(i, j);
				this.<UpdateChunkProtectionLevels>g__AddProtectionLevel|84_3(chunkKey, protectionLevel);
			}
		}
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public void <UpdateChunkProtectionLevels>g__AddProtectionLevel|84_3(long chunkKey, ChunkProtectionLevel protectionLevel)
	{
		ChunkProtectionLevel chunkProtectionLevel;
		if (this.chunkProtectionLevels.TryGetValue(chunkKey, out chunkProtectionLevel))
		{
			if (protectionLevel == (protectionLevel & chunkProtectionLevel))
			{
				return;
			}
			this.chunkProtectionLevels[chunkKey] = (chunkProtectionLevel | protectionLevel);
		}
		else
		{
			this.chunkProtectionLevels[chunkKey] = protectionLevel;
		}
		HashSetLong key;
		if (this.groupsByChunkKey.TryGetValue(chunkKey, out key))
		{
			ChunkProtectionLevel chunkProtectionLevel2;
			if (this.groupProtectionLevels.TryGetValue(key, out chunkProtectionLevel2))
			{
				this.groupProtectionLevels[key] = (chunkProtectionLevel2 | protectionLevel);
				return;
			}
			this.groupProtectionLevels[key] = protectionLevel;
		}
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public void <RemoveChunks>g__RemoveChunk|105_0(long _key, bool _resetDecos)
	{
		using (RegionFileManager.s_RemoveChunk.Auto())
		{
			Dictionary<long, IRegionFileChunkSnapshot> obj = this.chunkMemoryStreamsToSave;
			lock (obj)
			{
				this.chunkMemoryStreamsToSave.Remove(_key);
			}
			Dictionary<long, Chunk> obj2 = this.chunksToSave;
			lock (obj2)
			{
				this.chunksToSave.Remove(_key);
			}
			int chunkX = WorldChunkCache.extractX(_key);
			int chunkZ = WorldChunkCache.extractZ(_key);
			this.regionFileAccess.Remove(this.saveDirectory, chunkX, chunkZ);
			this.RemoveChunkSync(_key);
			this.chunksInLoadDir.Remove(_key);
			this.chunksInSaveDir.Remove(_key);
			if (_resetDecos)
			{
				DecoManager.Instance.ResetDecosForWorldChunk(_key);
			}
		}
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Internal)]
	public static int <SaveChunkAgeDebugTexture>g__OrdinalProtectionLevel|112_0(ChunkProtectionLevel level)
	{
		if (level != ChunkProtectionLevel.None)
		{
			return 1 + (int)Math.Log((double)Convert.ToInt32(level), 2.0);
		}
		return 0;
	}

	public const int cProtectedLandClaimChunkMargin = 1;

	public const int cProtectedBedrollChunkMargin = 1;

	public const int cProtectedOfflinePlayerChunkMargin = 1;

	public const int cProtectedDroppedBackpackChunkMargin = 1;

	public const int cProtectedVehicleChunkMargin = 1;

	public const int cProtectedQuestObjectiveMargin = 1;

	public const int cProtectedSupplyCrateMargin = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_CullExpiredChunks = new ProfilerMarker("RegionFileManager.CullExpiredChunks");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_RemoveChunk = new ProfilerMarker("RegionFileManager.RemoveChunk");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_UpdateProtectionLevels = new ProfilerMarker("RegionFileManager.UpdateProtectionLevels");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_SortCullingCandidates = new ProfilerMarker("RegionFileManager.SortCullingCandidates");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_CollectBestCandidates = new ProfilerMarker("RegionFileManager.CollectBestCandidates");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_ForceCullChunks = new ProfilerMarker("RegionFileManager.ForceCullChunks");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_ComputeChunkGroups = new ProfilerMarker("RegionFileManager.ComputeChunkGroups");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerCounterValue<long> s_RegionDataSize = new ProfilerCounterValue<long>(ProfilerCategory.Scripts, "Saved Region Data Size", ProfilerMarkerDataUnit.Bytes, ProfilerCounterOptions.FlushOnEndOfFrame);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerCounterValue<int> s_ProtectedChunkCount = new ProfilerCounterValue<int>(ProfilerCategory.Scripts, "Protected Chunks", ProfilerMarkerDataUnit.Count, ProfilerCounterOptions.FlushOnEndOfFrame);

	public const string pendingResetsFileName = "PendingResets.7pr";

	public const string cChunkFileExt = ".ttc";

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cMaxChunksToCull = 10000;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cMinimumByteAllowance = 20971520;

	[PublicizedFrom(EAccessModifier.Private)]
	public const long cHeadroomBytes = 5242880L;

	[PublicizedFrom(EAccessModifier.Private)]
	public static IRegionFilePlatformFactory platformFactory = RegionFilePlatform.CreateFactory();

	public static readonly IRegionFileDebugUtil DebugUtil = RegionFileManager.platformFactory.CreateDebugUtil();

	[PublicizedFrom(EAccessModifier.Private)]
	public RegionFileAccessAbstract regionFileAccess;

	[PublicizedFrom(EAccessModifier.Private)]
	public string saveDirectory;

	[PublicizedFrom(EAccessModifier.Private)]
	public string loadDirectory;

	[PublicizedFrom(EAccessModifier.Private)]
	public int maxChunksInCache;

	[PublicizedFrom(EAccessModifier.Private)]
	public long maxChunkAge = -1L;

	[PublicizedFrom(EAccessModifier.Private)]
	public long maxBytes = -1L;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<long> chunksInLocalCache = new List<long>();

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSetLong chunksInLoadDir = new HashSetLong();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<long, uint> chunksInSaveDir;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<long> expiredChunks = new List<long>(10000);

	[PublicizedFrom(EAccessModifier.Private)]
	public List<long> resetRequestedChunks = new List<long>();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool protectionLevelsDirty = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<long, ChunkProtectionLevel> chunkProtectionLevels = new Dictionary<long, ChunkProtectionLevel>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<KeyValuePair<long, ulong>> sortedCullingCandidates = new List<KeyValuePair<long, ulong>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<int, HashSetLong> chunksByTraderID = new Dictionary<int, HashSetLong>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<HashSetLong> chunkGroups = new List<HashSetLong>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<long, HashSetLong> groupsByChunkKey = new Dictionary<long, HashSetLong>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<HashSetLong, ChunkProtectionLevel> groupProtectionLevels = new Dictionary<HashSetLong, ChunkProtectionLevel>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<HashSetLong, uint> groupTimestamps = new Dictionary<HashSetLong, uint>();

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<HashSetLong> processedChunkGroups = new HashSet<HashSetLong>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<NetPackageDeleteChunkData> pendingChunkDeletionPackages = new List<NetPackageDeleteChunkData>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<long, Chunk> chunksToSave = new Dictionary<long, Chunk>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<long, IRegionFileChunkSnapshot> chunkMemoryStreamsToSave = new Dictionary<long, IRegionFileChunkSnapshot>();

	[PublicizedFrom(EAccessModifier.Private)]
	public IRegionFileChunkSnapshotUtil snapshotUtil;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Chunk> chunksToUnloadLater = new List<Chunk>();

	[PublicizedFrom(EAccessModifier.Private)]
	public AutoResetEvent saveThreadWaitHandle = new AutoResetEvent(false);

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bSaveOnChunkDrop;

	[PublicizedFrom(EAccessModifier.Private)]
	public long chunkKeyCurrentlySaved = long.MaxValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public ThreadManager.ThreadInfo taskInfoThreadSaveChunks;

	[PublicizedFrom(EAccessModifier.Private)]
	public volatile bool bSaveRunning = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly RegionFileManager.ProtectedPositionCache ppdPositionCache = new RegionFileManager.ProtectedPositionCache();

	[PublicizedFrom(EAccessModifier.Private)]
	public int thread_SaveChunks_SleepCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly object saveLock = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public class ProtectedPositionCache
	{
		public void ClearAll()
		{
			this.bedrolls.Clear();
			this.lpBlocks.Clear();
			this.offlinePlayers.Clear();
			this.backpacks.Clear();
			this.quests.Clear();
			this.vendingMachines.Clear();
			this.vehicles.Clear();
			this.supplyCrates.Clear();
		}

		public readonly List<Vector3i> bedrolls = new List<Vector3i>();

		public readonly List<Vector3i> lpBlocks = new List<Vector3i>();

		public readonly List<Vector3i> offlinePlayers = new List<Vector3i>();

		public readonly List<Vector3i> backpacks = new List<Vector3i>();

		public readonly List<Vector3i> quests = new List<Vector3i>();

		public readonly List<Vector3i> vendingMachines = new List<Vector3i>();

		public readonly List<Vector3i> vehicles = new List<Vector3i>();

		public readonly List<Vector3i> supplyCrates = new List<Vector3i>();
	}
}

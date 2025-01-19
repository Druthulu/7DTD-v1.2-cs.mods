using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEngine;

public class ChunkManager : IChunkProviderIndicator
{
	public void Init(World _world)
	{
		this.m_World = _world;
		this.rectanglesAroundPlayers = new List<Vector2i>[15];
		for (int i = 0; i < 15; i++)
		{
			this.rectanglesAroundPlayers[i] = new List<Vector2i>();
			for (int j = -i; j <= i; j++)
			{
				for (int k = -i; k <= i; k++)
				{
					if (j == -i || j == i || k == -i || k == i)
					{
						this.rectanglesAroundPlayers[i].Add(new Vector2i(j, k));
					}
				}
			}
		}
		GamePrefs.OnGamePrefChanged += this.OnGamePrefChanged;
		ChunkManager.MaxQueuedMeshLayers = GamePrefs.GetInt(EnumGamePrefs.MaxQueuedMeshLayers);
		this.threadInfoRegenerating = ThreadManager.StartThread("ChunkRegeneration", new ThreadManager.ThreadFunctionDelegate(this.thread_RegeneratingInit), new ThreadManager.ThreadFunctionLoopDelegate(this.thread_Regenerating), null, System.Threading.ThreadPriority.Normal, null, null, true, false);
		this.threadInfoCalc = ThreadManager.StartThread("ChunkCalc", null, new ThreadManager.ThreadFunctionLoopDelegate(this.thread_Calc), null, System.Threading.ThreadPriority.Normal, null, null, true, false);
		this.BakeInit();
	}

	public void Cleanup()
	{
		this.threadInfoRegenerating.WaitForEnd();
		this.threadInfoRegenerating = null;
		this.calcThreadWaitHandle.Set();
		this.threadInfoCalc.WaitForEnd();
		this.threadInfoCalc = null;
		this.groundAlignBlocks.Clear();
		this.BakeCleanup();
		for (int i = this.m_ObservedEntities.Count - 1; i >= 0; i--)
		{
			this.RemoveChunkObserver(this.m_ObservedEntities[i]);
		}
		this.FreePools();
		this.m_UsedChunkGameObjects.Clear();
	}

	public void FreePools()
	{
		for (int i = 0; i < this.m_FreeChunkGameObjects.Count; i++)
		{
			ChunkGameObject chunkGameObject = this.m_FreeChunkGameObjects[i];
			chunkGameObject.Cleanup();
			UnityEngine.Object.Destroy(chunkGameObject.gameObject);
		}
		this.m_FreeChunkGameObjects.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnGamePrefChanged(EnumGamePrefs _pref)
	{
		if (_pref == EnumGamePrefs.MaxQueuedMeshLayers)
		{
			ChunkManager.MaxQueuedMeshLayers = GamePrefs.GetInt(_pref);
		}
	}

	public void OriginChanged(Vector3 _offset)
	{
		for (int i = 0; i < this.m_UsedChunkGameObjects.Count; i++)
		{
			GameObject gameObject = this.m_UsedChunkGameObjects[i].gameObject;
			if (gameObject)
			{
				gameObject.transform.position += _offset;
			}
		}
	}

	public ChunkManager.ChunkObserver AddChunkObserver(Vector3 _initialPosition, bool _bBuildVisualMeshAround, int _viewDim, int _entityIdToSendChunksTo)
	{
		ChunkManager.ChunkObserver chunkObserver = new ChunkManager.ChunkObserver(_initialPosition, _bBuildVisualMeshAround, _viewDim, _entityIdToSendChunksTo);
		this.m_ObservedEntities.Add(chunkObserver);
		this.isInternalForceUpdate = true;
		return chunkObserver;
	}

	public void RemoveChunkObserver(ChunkManager.ChunkObserver _chunkObserver)
	{
		for (int i = 0; i < this.m_ObservedEntities.Count; i++)
		{
			if (this.m_ObservedEntities[i].id == _chunkObserver.id)
			{
				this.m_ObservedEntities.RemoveAt(i);
				this.isInternalForceUpdate = true;
				return;
			}
		}
	}

	public void SendChunksToClients()
	{
		ChunkCluster chunkCache = this.m_World.ChunkCache;
		this.sendToClientPackages.Clear();
		for (int i = 0; i < this.m_ObservedEntities.Count; i++)
		{
			ChunkManager.ChunkObserver chunkObserver = this.m_ObservedEntities[i];
			if (chunkObserver.entityIdToSendChunksTo != -1)
			{
				foreach (long num in chunkObserver.chunksToRemove)
				{
					this.sendToClientPackages.Add(NetPackageManager.GetPackage<NetPackageChunkRemove>().Setup(num));
					chunkObserver.chunksLoaded.Remove(num);
					chunkObserver.chunksToReload.Remove(num);
				}
				chunkObserver.chunksToRemove.Clear();
				if (this.sendToClientPackages.Count > 0)
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(this.sendToClientPackages, false, chunkObserver.entityIdToSendChunksTo, -1, -1, null, 192);
					this.sendToClientPackages.Clear();
				}
				int j = 0;
				while (j < chunkObserver.chunksToLoad.list.Count)
				{
					long num2 = chunkObserver.chunksToLoad.list[j];
					Chunk chunkSync;
					if (chunkCache != null && (chunkSync = chunkCache.GetChunkSync(num2)) != null && !chunkSync.NeedsLightCalculation)
					{
						this.sendToClientPackages.Add(NetPackageManager.GetPackage<NetPackageChunk>().Setup(chunkSync, false));
						chunkObserver.chunksLoaded.Add(num2);
						chunkObserver.chunksToLoad.Remove(num2);
					}
					else
					{
						j++;
					}
					if (this.sendToClientPackages.Count >= 3)
					{
						break;
					}
				}
				for (int k = chunkObserver.chunksToReload.Count - 1; k >= 0; k--)
				{
					long key = chunkObserver.chunksToReload[k];
					if (chunkCache != null)
					{
						Chunk chunkSync2 = chunkCache.GetChunkSync(key);
						if (chunkSync2 != null && !chunkSync2.NeedsLightCalculation)
						{
							this.sendToClientPackages.Add(NetPackageManager.GetPackage<NetPackageChunk>().Setup(chunkSync2, true));
							chunkObserver.chunksToReload.RemoveAt(k);
						}
					}
				}
				if (chunkObserver.mapDatabase != null)
				{
					NetPackage mapChunkPackagesToSend = chunkObserver.mapDatabase.GetMapChunkPackagesToSend();
					if (mapChunkPackagesToSend != null)
					{
						this.sendToClientPackages.Add(mapChunkPackagesToSend);
					}
				}
				if (this.sendToClientPackages.Count > 0)
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(this.sendToClientPackages, false, chunkObserver.entityIdToSendChunksTo, -1, -1, null, 192);
					this.sendToClientPackages.Clear();
				}
			}
		}
	}

	public void ResendChunksToClients(HashSetLong _chunks)
	{
		List<EntityPlayerLocal> localPlayers = this.m_World.GetLocalPlayers();
		for (int i = 0; i < this.m_ObservedEntities.Count; i++)
		{
			ChunkManager.ChunkObserver chunkObserver = this.m_ObservedEntities[i];
			if (!chunkObserver.bBuildVisualMeshAround)
			{
				bool flag = false;
				int num = 0;
				while (!flag && num < localPlayers.Count)
				{
					if (chunkObserver.entityIdToSendChunksTo == localPlayers[num].entityId)
					{
						flag = true;
					}
					num++;
				}
				if (!flag)
				{
					chunkObserver.chunksToReload.AddRange(_chunks);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Chunk FindNextChunkToCopy()
	{
		if (this.ChunksToCopyInOneFrame.Count > 0)
		{
			if (this.chunksToCopyInOneFramePass == 0)
			{
				this.chunksToCopyInOneFramePass = 1;
				for (int i = 0; i < this.ChunksToCopyInOneFrame.Count; i++)
				{
					if (!this.ChunksToCopyInOneFrame[i].NeedsCopying)
					{
						this.chunksToCopyInOneFramePass = 0;
						break;
					}
				}
			}
			if (this.chunksToCopyInOneFramePass > 0)
			{
				if (this.chunksToCopyInOneFrameIndex < this.ChunksToCopyInOneFrame.Count)
				{
					List<Chunk> chunksToCopyInOneFrame = this.ChunksToCopyInOneFrame;
					int num = this.chunksToCopyInOneFrameIndex;
					this.chunksToCopyInOneFrameIndex = num + 1;
					Chunk chunk = chunksToCopyInOneFrame[num];
					if (chunk != null)
					{
						this.currentClusterIndex = chunk.ClrIdx;
						chunk.InProgressCopying = true;
					}
					return chunk;
				}
				this.chunksToCopyInOneFramePass = 0;
				this.ChunksToCopyInOneFrame.Clear();
			}
		}
		bool flag = false;
		long num2;
		do
		{
			long[] obj = this.chunksToCopyArr;
			lock (obj)
			{
				if (this.chunksToCopyIdx >= this.m_ChunksToCopy.Count)
				{
					return null;
				}
				long[] array = this.m_ChunksToCopy.Array;
				int num = this.chunksToCopyIdx;
				this.chunksToCopyIdx = num + 1;
				num2 = array[num];
			}
			for (int j = 0; j < this.ChunksToCopyInOneFrame.Count; j++)
			{
				if (num2 == this.ChunksToCopyInOneFrame[j].Key)
				{
					flag = true;
					break;
				}
			}
		}
		while (flag);
		this.currentClusterIndex = WorldChunkCache.extractClrIdx(num2);
		ChunkCluster chunkCluster = this.m_World.ChunkClusters[this.currentClusterIndex];
		if (chunkCluster == null)
		{
			return null;
		}
		Chunk chunkSync = chunkCluster.GetChunkSync(num2);
		if (chunkSync == null)
		{
			return null;
		}
		chunkSync.EnterWriteLock();
		if (chunkSync.IsLocked)
		{
			chunkSync.ExitWriteLock();
			return null;
		}
		chunkSync.InProgressCopying = true;
		chunkSync.ExitWriteLock();
		return chunkSync;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void freeChunkGameObjects()
	{
		long[] obj = this.chunksToFreeArr;
		lock (obj)
		{
			for (int i = 0; i < this.m_ChunksToFree.Count; i++)
			{
				long key = this.m_ChunksToFree.Array[i];
				ChunkCluster chunkCluster = this.m_World.ChunkClusters[WorldChunkCache.extractClrIdx(key)];
				if (chunkCluster == null)
				{
					Log.Warning("freeChunkGameObjects: cluster not found " + WorldChunkCache.extractClrIdx(key).ToString());
				}
				else
				{
					Chunk chunkSync = chunkCluster.GetChunkSync(key);
					if (chunkSync != null && !chunkSync.hasEntities && chunkSync.IsDisplayed)
					{
						this.FreeChunkGameObject(chunkCluster, chunkSync);
						chunkSync.IsCollisionMeshGenerated = false;
						chunkSync.NeedsRegeneration = true;
					}
				}
			}
			this.m_ChunksToFree = default(ArraySegment<long>);
		}
	}

	public void ResetChunksToCopyInOneFrame()
	{
		this.ChunksToCopyInOneFrame.Clear();
		this.chunksToCopyInOneFrameIndex = 0;
		this.chunksToCopyInOneFramePass = 0;
	}

	public bool CopyChunksToUnity()
	{
		bool result;
		do
		{
			result = this.doCopyChunksToUnity();
		}
		while (this.ChunksToCopyInOneFrame.Count > 0 && this.currentCopiedChunk != null);
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool doCopyChunksToUnity()
	{
		if (this.m_ChunksToFree.Count > 0)
		{
			this.freeChunkGameObjects();
		}
		if (!this.isContinueCopying && this.currentCopiedChunk != null)
		{
			this.currentCopiedChunkGameObject.EndCopyMeshLayer();
			if (!this.currentCopiedChunk.NeedsCopying)
			{
				if (this.currentCopiedChunk.displayState == Chunk.DisplayState.Start)
				{
					this.currentCopiedChunk.InProgressCopying = false;
					this.currentCopiedChunk.IsCollisionMeshGenerated = true;
					ChunkCluster chunkCache = this.m_World.ChunkCache;
					if (chunkCache != null)
					{
						chunkCache.OnChunkDisplayed(this.currentCopiedChunk.Key, true);
						this.currentCopiedChunk.OnDisplay(this.m_World, this.currentCopiedChunkGameObject.blockEntitiesParentT, chunkCache);
					}
				}
				if (this.currentCopiedChunk.displayState == Chunk.DisplayState.BlockEntities)
				{
					ChunkCluster chunkCache2 = this.m_World.ChunkCache;
					if (chunkCache2 != null)
					{
						this.currentCopiedChunk.OnDisplayBlockEntities(this.m_World, this.currentCopiedChunkGameObject.blockEntitiesParentT, chunkCache2);
					}
				}
				if (this.currentCopiedChunk.displayState != Chunk.DisplayState.Done)
				{
					return true;
				}
				Action<Chunk> onChunkCopiedToUnity = this.OnChunkCopiedToUnity;
				if (onChunkCopiedToUnity != null)
				{
					onChunkCopiedToUnity(this.currentCopiedChunk);
				}
				this.currentCopiedChunk = null;
				if (this.ChunksToCopyInOneFrame.Count == 0)
				{
					return true;
				}
			}
		}
		if (this.currentCopiedChunk == null)
		{
			this.isContinueCopying = false;
			this.currentCopiedChunk = this.FindNextChunkToCopy();
			if (this.currentCopiedChunk == null)
			{
				return this.chunksToCopyIdx < this.m_ChunksToCopy.Count - 1;
			}
			ChunkCluster chunkCluster = this.m_World.ChunkClusters[this.currentClusterIndex];
			if (chunkCluster == null)
			{
				return false;
			}
			this.currentCopiedChunk.displayState = Chunk.DisplayState.Start;
			long key = this.currentCopiedChunk.Key;
			if (!chunkCluster.DisplayedChunkGameObjects.TryGetValue(key, out this.currentCopiedChunkGameObject))
			{
				this.currentCopiedChunkGameObject = this.GetNextFreeChunkGameObject();
				this.currentCopiedChunkGameObject.SetChunk(this.currentCopiedChunk, chunkCluster);
				this.currentCopiedChunkGameObject.gameObject.SetActive(true);
				chunkCluster.SetDisplayedChunkGameObject(key, this.currentCopiedChunkGameObject);
			}
			else if (this.currentCopiedChunkGameObject.chunk != this.currentCopiedChunk)
			{
				Log.Warning("currentCopiedChunk {0} wrong chunk on obj!", new object[]
				{
					this.currentCopiedChunk
				});
				this.currentCopiedChunkGameObject.SetChunk(this.currentCopiedChunk, chunkCluster);
			}
		}
		if (!this.isContinueCopying)
		{
			this.currentCopiedChunkLayer = this.currentCopiedChunkGameObject.StartCopyMeshLayer();
			if (this.currentCopiedChunkLayer < 0)
			{
				return true;
			}
		}
		if (this.chunksToCopyInOneFramePass > 0)
		{
			int num;
			int num2;
			this.currentCopiedChunkGameObject.CreateMeshAll(out num, out num2);
			this.isContinueCopying = false;
		}
		else
		{
			int num;
			int num2;
			int num3;
			int num4;
			this.isContinueCopying = this.currentCopiedChunkGameObject.CreateFromChunkNext(out num3, out num4, out num, out num2);
		}
		return this.isContinueCopying;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void task_Lighting(ThreadManager.TaskInfo _taskInfo)
	{
		Chunk chunk = null;
		int i = 0;
		while (i < this.m_World.Players.list.Count * 2)
		{
			this.chunksToLightIdx = 0;
			ChunkCluster chunkCluster;
			for (;;)
			{
				long[] obj = this.chunksToLightArr;
				long num2;
				lock (obj)
				{
					if (this.chunksToLightIdx >= this.m_ChunksToLight.Count)
					{
						chunk = null;
						break;
					}
					long[] array = this.m_ChunksToLight.Array;
					int num = this.chunksToLightIdx;
					this.chunksToLightIdx = num + 1;
					num2 = array[num];
					if (num2 == 9223372036854775807L)
					{
						continue;
					}
					this.chunksToLightArr[this.chunksToLightIdx - 1] = long.MaxValue;
				}
				int idx = WorldChunkCache.extractClrIdx(num2);
				chunkCluster = this.m_World.ChunkClusters[idx];
				if (chunkCluster != null)
				{
					int x = WorldChunkCache.extractX(num2);
					int y = WorldChunkCache.extractZ(num2);
					chunk = chunkCluster.GetChunkSync(x, y);
					if (chunk != null && !chunk.IsLocked)
					{
						if (chunk.NeedsLightDecoration && !chunk.IsLocked)
						{
							chunk.EnterWriteLock();
							if (chunk.IsLocked)
							{
								chunk.ExitWriteLock();
								continue;
							}
							chunk.InProgressDecorating = true;
							chunk.ExitWriteLock();
							ChunkProviderGenerateWorld chunkProviderGenerateWorld = chunkCluster.ChunkProvider as ChunkProviderGenerateWorld;
							if (chunkProviderGenerateWorld != null)
							{
								chunkProviderGenerateWorld.UpdateDecorations(chunk);
								if (chunk.InProgressDecorating)
								{
									chunk.InProgressDecorating = false;
									continue;
								}
							}
							chunk.NeedsLightDecoration = false;
							chunk.NeedsDecoration = false;
							chunk.InProgressDecorating = false;
						}
						if (chunk.NeedsLightCalculation && !chunk.NeedsDecoration && !chunk.IsLocked)
						{
							chunk.EnterWriteLock();
							if (chunk.IsLocked)
							{
								chunk.ExitWriteLock();
							}
							else
							{
								chunk.InProgressLighting = true;
								chunk.ExitWriteLock();
								if (!chunkCluster.GetNeighborChunks(chunk, this.neighborsLightingThread))
								{
									chunk.InProgressLighting = false;
								}
								else if (!Chunk.IsNeighbourChunksDecorated(this.neighborsLightingThread))
								{
									chunk.InProgressLighting = false;
								}
								else
								{
									if (this.lockLightingInProgress(this.neighborsLightingThread))
									{
										goto IL_20A;
									}
									chunk.InProgressLighting = false;
								}
							}
						}
					}
				}
			}
			IL_26B:
			if (chunk != null)
			{
				i++;
				continue;
			}
			break;
			IL_20A:
			chunkCluster.LightChunk(chunk, this.neighborsLightingThread);
			chunk.InProgressLighting = false;
			chunk.NeedsLightCalculation = false;
			chunk.NeedsRegeneration = true;
			for (int j = 0; j < this.neighborsLightingThread.Length; j++)
			{
				this.neighborsLightingThread[j].InProgressLighting = false;
			}
			Action<Chunk> onChunkInitialized = this.OnChunkInitialized;
			if (onChunkInitialized == null)
			{
				goto IL_26B;
			}
			onChunkInitialized(chunk);
			goto IL_26B;
		}
		Array.Clear(this.neighborsLightingThread, 0, this.neighborsLightingThread.Length);
		this.bLightingDone = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool lockLightingInProgress(Chunk[] chunks)
	{
		for (int i = 0; i < chunks.Length; i++)
		{
			Chunk chunk = chunks[i];
			chunk.EnterWriteLock();
			if (chunk.IsLocked)
			{
				for (int j = 0; j < i; j++)
				{
					chunks[j].InProgressLighting = false;
				}
				chunk.ExitWriteLock();
				return false;
			}
			chunk.InProgressLighting = true;
			chunk.ExitWriteLock();
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int thread_Calc(ThreadManager.ThreadInfo _threadInfo)
	{
		if (!_threadInfo.TerminationRequested())
		{
			this.calcThreadWaitHandle.WaitOne();
			this.task_CalcChunkPositions(_threadInfo);
			this.task_Lighting(null);
			return 5;
		}
		return -1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void task_CalcChunkPositions(ThreadManager.ThreadInfo _threadInfo)
	{
		if (this.isViewingOrCollisionPositionsChanged_threadCalc)
		{
			this.viewingChunkPositionsCopy.Clear();
			this.collisionChunkPositionsCopy.Clear();
			this.lightingChunkPositionsCopy.Clear();
			object obj = this.lockObject;
			lock (obj)
			{
				this.viewingChunkPositionsCopy.AddRange(this.m_ViewingChunkPositions.list);
				this.collisionChunkPositionsCopy.AddRange(this.m_CollisionChunkPositions.list);
				this.lightingChunkPositionsCopy.AddRange(this.m_AllChunkPositions.list);
			}
			this.isViewingOrCollisionPositionsChanged_threadCalc = false;
		}
		this.chunksToCopyTemp.Clear();
		this.chunksToFreeTemp.Clear();
		this.chunksToLightTemp.Clear();
		int num = Utils.FastMax(Utils.FastMax(this.viewingChunkPositionsCopy.Count, this.collisionChunkPositionsCopy.Count), this.lightingChunkPositionsCopy.Count);
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		for (int i = 0; i < num; i++)
		{
			if (this.chunksToCopyTemp.Count < 100 && num2 < this.viewingChunkPositionsCopy.Count)
			{
				long num6 = this.viewingChunkPositionsCopy[num2];
				if (this.checkChunkNeedsCopying(num6) != null)
				{
					this.chunksToCopyTemp.Add(num6);
				}
				num2++;
			}
			if (this.chunksToCopyTemp.Count < 100 && num3 < this.collisionChunkPositionsCopy.Count)
			{
				long num7 = this.collisionChunkPositionsCopy[num3];
				if (this.checkChunkNeedsCopying(num7) != null)
				{
					this.chunksToCopyTemp.Add(num7);
				}
				num3++;
			}
			if (this.chunksToLightTemp.Count < 100 && num5 < this.lightingChunkPositionsCopy.Count)
			{
				long num8 = this.lightingChunkPositionsCopy[num5];
				int idx = WorldChunkCache.extractClrIdx(num8);
				ChunkCluster chunkCluster = this.m_World.ChunkClusters[idx];
				if (chunkCluster != null)
				{
					Chunk chunkSync = chunkCluster.GetChunkSync(num8);
					if (chunkSync != null && chunkSync.NeedsLightCalculation && (!chunkSync.NeedsDecoration || chunkSync.NeedsLightDecoration) && !chunkSync.IsLocked && !chunkCluster.IsOnBorder(chunkSync))
					{
						this.chunksToLightTemp.Add(num8);
					}
				}
				num5++;
			}
			if (ChunkManager.GenerateCollidersOnlyAroundEntites && num4 < this.collisionChunkPositionsCopy.Count)
			{
				ChunkCluster chunkCluster2 = this.m_World.ChunkClusters[0];
				if (chunkCluster2 != null)
				{
					long key = chunkCluster2.ToLocalKey(this.collisionChunkPositionsCopy[num4]);
					Chunk chunkSync2 = chunkCluster2.GetChunkSync(key);
					if (chunkSync2 != null && chunkSync2.IsCollisionMeshGenerated && chunkSync2.NeedsOnlyCollisionMesh)
					{
						if (!chunkSync2.hasEntities && chunkCluster2.GetNeighborChunks(chunkSync2, this.neighborsGenerationThread2) && !this.hasAnyChunkEntities(this.neighborsGenerationThread2))
						{
							this.chunksToFreeTemp.Add(chunkSync2.Key);
						}
						Array.Clear(this.neighborsGenerationThread2, 0, this.neighborsGenerationThread2.Length);
					}
				}
				num4++;
			}
		}
		long[] obj2 = this.chunksToCopyArr;
		lock (obj2)
		{
			this.chunksToCopyIdx = 0;
			this.chunksToCopyTemp.CopyTo(this.chunksToCopyArr);
			this.m_ChunksToCopy = new ArraySegment<long>(this.chunksToCopyArr, 0, this.chunksToCopyTemp.Count);
		}
		obj2 = this.chunksToFreeArr;
		lock (obj2)
		{
			this.chunksToFreeTemp.CopyTo(this.chunksToFreeArr);
			this.m_ChunksToFree = new ArraySegment<long>(this.chunksToFreeArr, 0, this.chunksToFreeTemp.Count);
		}
		obj2 = this.chunksToLightArr;
		lock (obj2)
		{
			this.chunksToLightIdx = 0;
			this.chunksToLightTemp.CopyTo(this.chunksToLightArr);
			this.m_ChunksToLight = new ArraySegment<long>(this.chunksToLightArr, 0, this.chunksToLightTemp.Count);
		}
		this.bCalcPositionsDone = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void thread_RegeneratingInit(ThreadManager.ThreadInfo _threadInfo)
	{
		_threadInfo.threadData = new ChunkManager.ThreadRegeneratingData();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public int thread_Regenerating(ThreadManager.ThreadInfo _threadInfo)
	{
		if (_threadInfo.TerminationRequested())
		{
			return -1;
		}
		if (VoxelMeshLayer.InstanceCount - MemoryPools.poolVML.GetPoolSize() > ChunkManager.MaxQueuedMeshLayers)
		{
			return 20;
		}
		ChunkManager.ThreadRegeneratingData threadRegeneratingData = _threadInfo.threadData as ChunkManager.ThreadRegeneratingData;
		if (this.isViewingOrCollisionPositionsChanged_threadReg)
		{
			this.isViewingOrCollisionPositionsChanged_threadReg = false;
			threadRegeneratingData.viewingChunkPositionsCopy.Clear();
			threadRegeneratingData.collisionChunkPositionsCopy.Clear();
			object obj = this.lockObject;
			lock (obj)
			{
				threadRegeneratingData.viewingChunkPositionsCopy.AddRange(this.m_ViewingChunkPositions.list);
				threadRegeneratingData.collisionChunkPositionsCopy.AddRange(this.m_CollisionChunkPositions.list);
			}
		}
		this.RegenerateNextChunk(threadRegeneratingData.viewingChunkPositionsCopy, false);
		this.RegenerateNextChunk(threadRegeneratingData.collisionChunkPositionsCopy, true);
		return 5;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Chunk RegenerateNextChunk(List<long> _chunkPositions, bool _isOnlyColliders)
	{
		for (int i = 0; i < _chunkPositions.Count; i++)
		{
			long key = _chunkPositions[i];
			int idx = WorldChunkCache.extractClrIdx(key);
			ChunkCluster chunkCluster = this.m_World.ChunkClusters[idx];
			if (chunkCluster != null)
			{
				Chunk chunkSync = chunkCluster.GetChunkSync(key);
				if (chunkSync != null && !chunkSync.IsLocked && (chunkSync.NeedsRegeneration || (chunkSync.NeedsOnlyCollisionMesh && !_isOnlyColliders)) && !chunkSync.NeedsLightCalculation && !chunkSync.NeedsDecoration && !chunkSync.IsLocked)
				{
					chunkSync.EnterWriteLock();
					if (chunkSync.IsLocked)
					{
						chunkSync.ExitWriteLock();
					}
					else
					{
						chunkSync.InProgressRegeneration = true;
						chunkSync.ExitWriteLock();
						if (!chunkCluster.GetNeighborChunks(chunkSync, this.regenerateNextChunkNeighbors))
						{
							chunkSync.InProgressRegeneration = false;
						}
						else if (!Chunk.IsNeighbourChunksLit(this.regenerateNextChunkNeighbors))
						{
							chunkSync.InProgressRegeneration = false;
						}
						else if (ChunkManager.GenerateCollidersOnlyAroundEntites && _isOnlyColliders && !chunkSync.hasEntities && !this.hasAnyChunkEntities(this.regenerateNextChunkNeighbors))
						{
							chunkSync.InProgressRegeneration = false;
						}
						else
						{
							if (this.lockGenerateInProgress(this.regenerateNextChunkNeighbors))
							{
								if (chunkSync.NeedsOnlyCollisionMesh && !_isOnlyColliders)
								{
									chunkSync.NeedsRegeneration = true;
								}
								chunkCluster.RegenerateChunk(chunkSync, this.regenerateNextChunkNeighbors);
								chunkSync.InProgressRegeneration = false;
								chunkSync.NeedsOnlyCollisionMesh = _isOnlyColliders;
								for (int j = 0; j < this.regenerateNextChunkNeighbors.Length; j++)
								{
									this.regenerateNextChunkNeighbors[j].InProgressRegeneration = false;
								}
								Action<Chunk> onChunkRegenerated = this.OnChunkRegenerated;
								if (onChunkRegenerated != null)
								{
									onChunkRegenerated(chunkSync);
								}
								for (int k = 0; k < this.regenerateNextChunkNeighbors.Length; k++)
								{
									Chunk chunk = this.regenerateNextChunkNeighbors[k];
									if (!chunk.NeedsRegeneration)
									{
										Action<Chunk> onChunkRegenerated2 = this.OnChunkRegenerated;
										if (onChunkRegenerated2 != null)
										{
											onChunkRegenerated2(chunk);
										}
									}
								}
								Array.Clear(this.regenerateNextChunkNeighbors, 0, this.regenerateNextChunkNeighbors.Length);
								return chunkSync;
							}
							chunkSync.InProgressRegeneration = false;
						}
					}
				}
			}
		}
		Array.Clear(this.regenerateNextChunkNeighbors, 0, this.regenerateNextChunkNeighbors.Length);
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasAnyChunkEntities(Chunk[] chunks)
	{
		return chunks[0].hasEntities || chunks[1].hasEntities || chunks[2].hasEntities || chunks[3].hasEntities || chunks[4].hasEntities || chunks[5].hasEntities || chunks[6].hasEntities || chunks[7].hasEntities;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool lockGenerateInProgress(Chunk[] chunks)
	{
		for (int i = 0; i < chunks.Length; i++)
		{
			Chunk chunk = chunks[i];
			chunk.EnterWriteLock();
			if (chunk.IsLocked)
			{
				for (int j = 0; j < i; j++)
				{
					chunks[j].InProgressRegeneration = false;
				}
				chunk.ExitWriteLock();
				return false;
			}
			chunk.InProgressRegeneration = true;
			chunk.ExitWriteLock();
		}
		return true;
	}

	public void ReloadAllChunks()
	{
		this.m_ViewingChunkPositions.Clear();
		this.m_AllChunkPositions.Clear();
		this.m_CollisionChunkPositions.Clear();
		this.recalcFreeChunkGameObjects(int.MaxValue, true);
		ChunkCluster chunkCluster = this.m_World.ChunkClusters[0];
		if (chunkCluster != null)
		{
			chunkCluster.Clear();
		}
		for (int i = 0; i < this.m_ObservedEntities.Count; i++)
		{
			this.m_ObservedEntities[i].curChunkPos.y = -1;
		}
	}

	public ArraySegment<long> GetActiveChunkSet()
	{
		return new ArraySegment<long>(this.activeChunkSetArr, 0, this.m_AllChunkPositions.list.Count);
	}

	public bool IsForceUpdate()
	{
		return this.isInternalForceUpdate || this.isChunkClusterChanged;
	}

	public void DetermineChunksToLoad()
	{
		int num = this.removeChunksToUnload(8);
		bool flag = this.isInternalForceUpdate || this.isChunkClusterChanged;
		for (int i = 0; i < this.m_ObservedEntities.Count; i++)
		{
			ChunkManager.ChunkObserver chunkObserver = this.m_ObservedEntities[i];
			Vector3i vector3i = new Vector3i(World.toChunkXZ(Utils.Fastfloor(chunkObserver.position.x)), 0, World.toChunkXZ(Utils.Fastfloor(chunkObserver.position.z)));
			bool flag2 = vector3i != chunkObserver.curChunkPos;
			chunkObserver.curChunkPos = vector3i;
			if (this.isChunkClusterChanged || flag2)
			{
				flag = true;
				int x = vector3i.x;
				int z = vector3i.z;
				chunkObserver.chunksAround.Clear();
				for (int j = 0; j < chunkObserver.viewDim + 2; j++)
				{
					List<Vector2i> list = this.rectanglesAroundPlayers[j];
					for (int k = 0; k < list.Count; k++)
					{
						chunkObserver.chunksAround.Add(j, WorldChunkCache.MakeChunkKey(list[k].x + x, list[k].y + z));
					}
				}
				int count = this.m_World.ChunkClusters.Count;
				for (int l = 1; l < count; l++)
				{
					ChunkCluster chunkCluster = this.m_World.ChunkClusters[l];
					if (chunkCluster != null)
					{
						chunkObserver.chunksAround.Add(2, chunkCluster.GetChunkKeysCopySync());
					}
				}
				chunkObserver.chunksAround.RecalcHashSetList();
				if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
				{
					chunkObserver.chunksToLoad.Clear();
					for (int m = 0; m < chunkObserver.chunksToLoad.buckets.Count; m++)
					{
						chunkObserver.chunksToLoad.Add(m, chunkObserver.chunksAround.buckets.array[m]);
						chunkObserver.chunksToLoad.buckets.array[m].ExceptWithHashSetLong(chunkObserver.chunksLoaded);
					}
					chunkObserver.chunksToLoad.RecalcHashSetList();
					chunkObserver.chunksToRemove.Clear();
					chunkObserver.chunksToRemove.UnionWithHashSetLong(chunkObserver.chunksLoaded);
					chunkObserver.chunksAround.ExceptTarget(chunkObserver.chunksToRemove);
				}
			}
		}
		this.isInternalForceUpdate = false;
		this.isChunkClusterChanged = false;
		if (flag)
		{
			object obj = this.lockObject;
			lock (obj)
			{
				this.isViewingOrCollisionPositionsChanged_threadCalc = true;
				this.isViewingOrCollisionPositionsChanged_threadReg = true;
				this.m_ViewingChunkPositions.Clear();
				this.m_AllChunkPositions.Clear();
				for (int n = 0; n <= this.m_AllChunkPositions.buckets.Count; n++)
				{
					for (int num2 = 0; num2 < this.m_ObservedEntities.Count; num2++)
					{
						ChunkManager.ChunkObserver chunkObserver2 = this.m_ObservedEntities[num2];
						if (n < chunkObserver2.viewDim + 2)
						{
							this.m_AllChunkPositions.Add(n, chunkObserver2.chunksAround.buckets.array[n]);
							if (chunkObserver2.bBuildVisualMeshAround)
							{
								this.m_ViewingChunkPositions.buckets.array[n].UnionWithHashSetLong(chunkObserver2.chunksAround.buckets.array[n]);
							}
						}
					}
				}
				this.m_ViewingChunkPositions.RecalcHashSetList();
				this.m_AllChunkPositions.RecalcHashSetList();
				if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
				{
					this.m_AllChunkPositions.list.CopyTo(this.activeChunkSetArr);
					this.m_CollisionChunkPositions.Clear();
					for (int num3 = 0; num3 < this.m_CollisionChunkPositions.buckets.Count; num3++)
					{
						this.m_CollisionChunkPositions.buckets.array[num3].UnionWithHashSetLong(this.m_AllChunkPositions.buckets.array[num3]);
						this.m_CollisionChunkPositions.buckets.array[num3].ExceptWithHashSetLong(this.m_ViewingChunkPositions.buckets.array[num3]);
					}
					this.m_CollisionChunkPositions.RecalcHashSetList();
					ChunkCluster chunkCache = this.m_World.ChunkCache;
					if (chunkCache != null && !chunkCache.IsFixedSize)
					{
						HashSetList<Chunk> obj2 = this.chunksToUnload;
						lock (obj2)
						{
							foreach (long num4 in chunkCache.GetChunkKeysCopySync())
							{
								if (!this.m_AllChunkPositions.Contains(num4) && !DynamicMeshThread.ChunksToProcess.Contains(num4) && !DynamicMeshThread.ChunksToLoad.Contains(num4))
								{
									Chunk chunkSync = chunkCache.GetChunkSync(num4);
									chunkSync.InProgressUnloading = true;
									this.chunksToUnload.Add(chunkSync);
								}
							}
							for (int num5 = 0; num5 < this.chunksToUnload.list.Count; num5++)
							{
								chunkCache.RemoveChunk(this.chunksToUnload.list[num5]);
							}
						}
					}
				}
			}
			if (8 - num > 0)
			{
				this.recalcFreeChunkGameObjects(8 - num, false);
			}
		}
		this.calcThreadWaitHandle.Set();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int removeChunksToUnload(int _maxCGOsToUnload = 2147483647)
	{
		HashSetList<Chunk> obj = this.chunksToUnload;
		int result;
		lock (obj)
		{
			int num = 0;
			for (int i = this.chunksToUnload.list.Count - 1; i >= 0; i--)
			{
				Chunk chunk = this.chunksToUnload.list[i];
				ChunkCluster chunkCluster = this.m_World.ChunkClusters[WorldChunkCache.extractClrIdx(chunk.Key)];
				if (chunkCluster != null)
				{
					bool flag2 = false;
					chunk.EnterWriteLock();
					chunk.InProgressUnloading = true;
					if (!chunk.IsLockedExceptUnloading)
					{
						flag2 = true;
					}
					chunk.ExitWriteLock();
					if (flag2)
					{
						this.chunksToUnload.Remove(chunk);
						if (chunk.IsDisplayed)
						{
							this.FreeChunkGameObject(chunkCluster, chunk);
							num++;
						}
						chunkCluster.UnloadChunk(chunk);
						if (num >= _maxCGOsToUnload)
						{
							break;
						}
					}
				}
			}
			result = num;
		}
		return result;
	}

	public void ProcessChunksPendingUnload(Action<Chunk> action)
	{
		HashSetList<Chunk> obj = this.chunksToUnload;
		lock (obj)
		{
			foreach (Chunk obj2 in this.chunksToUnload.list)
			{
				action(obj2);
			}
		}
	}

	public long GetNextChunkToProvide()
	{
		ChunkCluster chunkCache = this.m_World.ChunkCache;
		if (chunkCache != null)
		{
			int num = 0;
			object obj = this.lockObject;
			lock (obj)
			{
				this.m_AllChunkPositions.list.CopyTo(this.allChunkPositionsCopy);
				num = this.m_AllChunkPositions.list.Count;
			}
			for (int i = 0; i < num; i++)
			{
				long num2 = this.allChunkPositionsCopy[i];
				if (WorldChunkCache.extractClrIdx(num2) == 0 && !chunkCache.ContainsChunkSync(num2))
				{
					return num2;
				}
			}
			IChunkProvider chunkProvider = chunkCache.ChunkProvider;
			if (chunkProvider != null)
			{
				HashSetList<long> requestedChunks = chunkProvider.GetRequestedChunks();
				if (requestedChunks != null)
				{
					List<long> list = requestedChunks.list;
					lock (list)
					{
						int count = requestedChunks.list.Count;
						if (count > 0)
						{
							long num3 = requestedChunks.list[count - 1];
							requestedChunks.Remove(num3);
							return num3;
						}
					}
				}
			}
		}
		return long.MaxValue;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Chunk checkChunkNeedsCopying(long key)
	{
		int num = WorldChunkCache.extractClrIdx(key);
		ChunkCluster chunkCluster = this.m_World.ChunkClusters[num];
		if (chunkCluster == null)
		{
			return null;
		}
		Chunk chunkSync = chunkCluster.GetChunkSync(key);
		if (chunkSync == null || chunkSync.IsLocked)
		{
			return null;
		}
		bool needsCopying = chunkSync.NeedsCopying;
		if (num != 0 && !needsCopying)
		{
			return null;
		}
		DictionarySave<long, ChunkGameObject> displayedChunkGameObjects = chunkCluster.DisplayedChunkGameObjects;
		bool flag2;
		lock (displayedChunkGameObjects)
		{
			flag2 = chunkCluster.DisplayedChunkGameObjects.ContainsKey(chunkSync.Key);
		}
		bool flag3 = !chunkSync.NeedsDecoration && !chunkSync.NeedsLightCalculation && !chunkSync.NeedsRegeneration;
		chunkSync.EnterWriteLock();
		if ((flag2 && !needsCopying) || (!flag2 && !flag3) || chunkSync.IsLocked)
		{
			chunkSync.ExitWriteLock();
			return null;
		}
		if (chunkSync.HasMeshLayer() || chunkSync.IsEmpty())
		{
			chunkSync.ExitWriteLock();
			return chunkSync;
		}
		chunkSync.NeedsRegeneration = true;
		chunkSync.ExitWriteLock();
		return null;
	}

	public ICollection GetCurrDisplayedChunkGameObjects()
	{
		return this.tempDisplayedCGOs;
	}

	public IList<ChunkGameObject> GetDisplayedChunkGameObjects()
	{
		this.tempDisplayedCGOs.Clear();
		for (int i = 0; i < this.m_World.ChunkClusters.Count; i++)
		{
			if (this.m_World.ChunkClusters[i] != null)
			{
				this.tempDisplayedCGOs.AddRange(this.m_World.ChunkClusters[i].DisplayedChunkGameObjects.Dict.Values);
			}
		}
		return this.tempDisplayedCGOs;
	}

	public int GetDisplayedChunkGameObjectsCount()
	{
		int num = 0;
		for (int i = 0; i < this.m_World.ChunkClusters.Count; i++)
		{
			if (this.m_World.ChunkClusters[i] != null)
			{
				num += this.m_World.ChunkClusters[i].DisplayedChunkGameObjects.Count;
			}
		}
		return num;
	}

	public List<ChunkGameObject> GetFreeChunkGameObjects()
	{
		return this.m_FreeChunkGameObjects;
	}

	public List<ChunkGameObject> GetUsedChunkGameObjects()
	{
		return this.m_UsedChunkGameObjects;
	}

	public void RemoveChunk(long _chunkKey)
	{
		int idx = WorldChunkCache.extractClrIdx(_chunkKey);
		ChunkCluster chunkCluster = this.m_World.ChunkClusters[idx];
		if (chunkCluster == null)
		{
			Log.Warning("RemoveChunk: cluster not found " + idx.ToString());
			return;
		}
		Chunk chunkSync = chunkCluster.GetChunkSync(_chunkKey);
		if (chunkSync == null)
		{
			Log.Warning("RemoveChunk: chunk not found " + WorldChunkCache.extractX(_chunkKey).ToString() + "/" + WorldChunkCache.extractZ(_chunkKey).ToString());
			return;
		}
		chunkSync.InProgressUnloading = true;
		chunkCluster.RemoveChunk(chunkSync);
		HashSetList<Chunk> obj = this.chunksToUnload;
		lock (obj)
		{
			this.chunksToUnload.Add(chunkSync);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void recalcFreeChunkGameObjects(int _maxToUnload = 2147483647, bool _bIgnoreFixedSizeFlag = false)
	{
		ChunkCluster chunkCache = this.m_World.ChunkCache;
		if (chunkCache == null || (chunkCache.IsFixedSize && !_bIgnoreFixedSizeFlag))
		{
			return;
		}
		this.cgoToRemove.Clear();
		foreach (KeyValuePair<long, ChunkGameObject> keyValuePair in chunkCache.DisplayedChunkGameObjects.Dict)
		{
			if (!this.m_ViewingChunkPositions.Contains(keyValuePair.Key) && !this.m_CollisionChunkPositions.Contains(keyValuePair.Key))
			{
				this.cgoToRemove.Add(keyValuePair.Key);
			}
		}
		for (int i = 0; i < this.cgoToRemove.Count; i++)
		{
			this.FreeChunkGameObject(chunkCache, this.cgoToRemove[i]);
			if (_maxToUnload-- <= 0)
			{
				break;
			}
		}
	}

	public void FreeChunkGameObject(ChunkCluster _cc, Chunk _chunk)
	{
		long key = _chunk.Key;
		ChunkGameObject chunkGameObject = _cc.DisplayedChunkGameObjects[key];
		if (chunkGameObject == null)
		{
			return;
		}
		if (chunkGameObject.GetChunk() != _chunk)
		{
			return;
		}
		this.FreeChunkGameObject(_cc, key);
	}

	public void FreeChunkGameObject(ChunkCluster _cc, long _key)
	{
		DynamicMeshThread.RemoveChunkGameObject(_key);
		ChunkGameObject chunkGameObject = _cc.RemoveDisplayedChunkGameObject(_key);
		chunkGameObject.gameObject.SetActive(false);
		_cc.OnChunkDisplayed(_key, false);
		if (this.currentCopiedChunkGameObject == chunkGameObject && this.currentCopiedChunk != null)
		{
			this.currentCopiedChunk.InProgressCopying = false;
			this.currentCopiedChunk = null;
		}
		chunkGameObject.SetChunk(null, null);
		this.m_UsedChunkGameObjects.Remove(chunkGameObject);
		this.m_FreeChunkGameObjects.Add(chunkGameObject);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ChunkGameObject GetNextFreeChunkGameObject()
	{
		ChunkGameObject chunkGameObject;
		if (this.m_FreeChunkGameObjects.Count == 0)
		{
			chunkGameObject = new GameObject("Chunk (new)").AddComponent<ChunkGameObject>();
		}
		else
		{
			chunkGameObject = this.m_FreeChunkGameObjects[this.m_FreeChunkGameObjects.Count - 1];
			this.m_FreeChunkGameObjects.RemoveAt(this.m_FreeChunkGameObjects.Count - 1);
		}
		this.m_UsedChunkGameObjects.Add(chunkGameObject);
		return chunkGameObject;
	}

	public void ClearChunksForAllObservers(ChunkCluster _cc)
	{
		for (int i = 0; i < this.m_ObservedEntities.Count; i++)
		{
			ChunkManager.ChunkObserver chunkObserver = this.m_ObservedEntities[i];
			this.ClearChunksForObserver(chunkObserver, _cc);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isKeyLocalCC(long _key)
	{
		return WorldChunkCache.extractClrIdx(_key) == 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ClearChunksForObserver(ChunkManager.ChunkObserver _chunkObserver, ChunkCluster cc)
	{
		for (int i = 0; i < _chunkObserver.chunksAround.buckets.Count; i++)
		{
			_chunkObserver.chunksAround.buckets.array[i].RemoveWhere(new Predicate<long>(this.isKeyLocalCC));
			_chunkObserver.chunksToLoad.buckets.array[i].RemoveWhere(new Predicate<long>(this.isKeyLocalCC));
		}
		_chunkObserver.chunksAround.RecalcHashSetList();
		_chunkObserver.chunksToLoad.RecalcHashSetList();
		_chunkObserver.chunksToReload.Clear();
		HashSetLong chunkKeysCopySync = cc.GetChunkKeysCopySync();
		_chunkObserver.chunksLoaded.ExceptWithHashSetLong(chunkKeysCopySync);
		_chunkObserver.chunksToRemove.ExceptWithHashSetLong(chunkKeysCopySync);
	}

	public void DebugOnGUI(float middleX, float middleY, int size)
	{
		for (int i = 0; i < this.m_ObservedEntities.Count; i++)
		{
			Color col = Color.white;
			if (!this.m_ObservedEntities[i].bBuildVisualMeshAround && this.m_ObservedEntities[i].entityIdToSendChunksTo != -1)
			{
				col = Color.cyan;
			}
			Vector3i vector3i = World.worldToBlockPos(this.m_ObservedEntities[i].position);
			GUIUtils.DrawRect(new Rect(middleX + (float)(World.toChunkXZ(vector3i.x) * size) - (float)(size / 2), middleY - (float)(World.toChunkXZ(vector3i.z) * size) - (float)(size / 2), (float)size, (float)size), col);
		}
	}

	public void RemoveAllChunksOnAllClients()
	{
		HashSetLong chunkKeysCopySync = this.m_World.ChunkCache.GetChunkKeysCopySync();
		List<EntityPlayerLocal> localPlayers = this.m_World.GetLocalPlayers();
		for (int i = 0; i < this.m_ObservedEntities.Count; i++)
		{
			if (!this.m_ObservedEntities[i].bBuildVisualMeshAround)
			{
				bool flag = false;
				int num = 0;
				while (!flag && num < localPlayers.Count)
				{
					if (this.m_ObservedEntities[i].entityIdToSendChunksTo == localPlayers[num].entityId)
					{
						flag = true;
					}
					num++;
				}
				if (!flag)
				{
					foreach (long item in chunkKeysCopySync)
					{
						this.m_ObservedEntities[i].chunksLoaded.Remove(item);
					}
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageChunkRemoveAll>(), false, this.m_ObservedEntities[i].entityIdToSendChunksTo, -1, -1, null, 192);
				}
			}
		}
		this.isChunkClusterChanged = true;
	}

	public void RemoveAllChunks()
	{
		ChunkCluster chunkCache = this.m_World.ChunkCache;
		foreach (long key in chunkCache.GetChunkKeysCopySync())
		{
			Chunk chunkSync = chunkCache.GetChunkSync(key);
			if (chunkSync != null)
			{
				this.RemoveChunk(chunkSync.Key);
			}
		}
		this.removeChunksToUnload(int.MaxValue);
		chunkCache.Clear();
	}

	public void ForceUpdate()
	{
		this.isInternalForceUpdate = true;
	}

	public void AddGroundAlignBlock(BlockEntityData _data)
	{
		this.groundAlignBlocks.Add(_data);
	}

	public void GroundAlignFrameUpdate()
	{
		int count = this.groundAlignBlocks.Count;
		for (int i = 0; i < count; i++)
		{
			BlockEntityData blockEntityData = this.groundAlignBlocks[i];
			blockEntityData.blockValue.Block.GroundAlign(blockEntityData);
		}
		this.groundAlignBlocks.Clear();
	}

	public void BakeInit()
	{
		this.bakeThreadInfo = ThreadManager.StartThread("ChunkMeshBake", null, new ThreadManager.ThreadFunctionLoopDelegate(this.BakeThread), null, System.Threading.ThreadPriority.Normal, null, null, true, false);
		this.bakeCoroutine = this.BakeEndOfFrame();
		GameManager.Instance.StartCoroutine(this.bakeCoroutine);
	}

	public void BakeCleanup()
	{
		this.bakeThreadInfo.RequestTermination();
		this.bakeEvent.Set();
		this.bakeThreadInfo.WaitForEnd();
		this.bakeThreadInfo = null;
		this.bakes.Clear();
		GameManager.Instance.StopCoroutine(this.bakeCoroutine);
	}

	public void BakeDestroyCancel(MeshCollider _meshCollider)
	{
		List<ChunkManager.BakeCollider> obj = this.bakes;
		lock (obj)
		{
			Mesh sharedMesh = _meshCollider.sharedMesh;
			if (sharedMesh)
			{
				_meshCollider.sharedMesh = null;
				UnityEngine.Object.Destroy(sharedMesh);
			}
			for (int i = 0; i < this.bakes.Count; i++)
			{
				ChunkManager.BakeCollider bakeCollider = this.bakes[i];
				if (bakeCollider.meshCollider == _meshCollider)
				{
					if (bakeCollider.mesh)
					{
						UnityEngine.Object.Destroy(bakeCollider.mesh);
						bakeCollider.mesh = null;
					}
					bakeCollider.isBaked = true;
				}
			}
		}
	}

	public Mesh BakeCancelAndGetMesh(MeshCollider _meshCollider)
	{
		List<ChunkManager.BakeCollider> obj = this.bakes;
		Mesh result;
		lock (obj)
		{
			Mesh mesh = _meshCollider.sharedMesh;
			ChunkManager.BakeCollider bakeCollider = null;
			for (int i = this.bakes.Count - 1; i >= 0; i--)
			{
				ChunkManager.BakeCollider bakeCollider2 = this.bakes[i];
				if (bakeCollider2.meshCollider == _meshCollider && bakeCollider2.mesh)
				{
					mesh = bakeCollider2.mesh;
					bakeCollider = bakeCollider2;
					break;
				}
			}
			ChunkManager.BakeCollider bakeCollider3 = this.bakeCurrent;
			if (bakeCollider3 != null && bakeCollider3.mesh && bakeCollider3.mesh == mesh)
			{
				bakeCollider3.isCancelledDestroy = true;
				result = null;
			}
			else
			{
				if (bakeCollider != null)
				{
					bakeCollider.mesh = null;
					bakeCollider.isBaked = true;
				}
				result = mesh;
			}
		}
		return result;
	}

	public void BakeAdd(Mesh _mesh, MeshCollider _meshCollider)
	{
		ChunkManager.BakeCollider bakeCollider = new ChunkManager.BakeCollider();
		bakeCollider.mesh = _mesh;
		bakeCollider.id = _mesh.GetInstanceID();
		bakeCollider.meshCollider = _meshCollider;
		List<ChunkManager.BakeCollider> obj = this.bakes;
		lock (obj)
		{
			this.bakes.Add(bakeCollider);
		}
		this.bakeEvent.Set();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int BakeThread(ThreadManager.ThreadInfo _threadInfo)
	{
		this.bakeEvent.WaitOne();
		if (_threadInfo.TerminationRequested())
		{
			return -1;
		}
		for (;;)
		{
			List<ChunkManager.BakeCollider> obj = this.bakes;
			ChunkManager.BakeCollider bakeCollider;
			lock (obj)
			{
				if (this.bakeIndex >= this.bakes.Count)
				{
					break;
				}
				List<ChunkManager.BakeCollider> list = this.bakes;
				int num = this.bakeIndex;
				this.bakeIndex = num + 1;
				bakeCollider = list[num];
				if (bakeCollider.isBaked)
				{
					continue;
				}
				this.bakeCurrent = bakeCollider;
			}
			Physics.BakeMesh(bakeCollider.id, false);
			bakeCollider.isBaked = true;
			this.bakeCurrent = null;
		}
		return 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator BakeEndOfFrame()
	{
		WaitForEndOfFrame wait = new WaitForEndOfFrame();
		for (;;)
		{
			yield return wait;
			this.BakeMeshAssign();
		}
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BakeMeshAssign()
	{
		List<ChunkManager.BakeCollider> obj = this.bakes;
		lock (obj)
		{
			this.bakeIndex = 0;
			for (int i = 0; i < this.bakes.Count; i++)
			{
				ChunkManager.BakeCollider bakeCollider = this.bakes[i];
				if (bakeCollider.mesh)
				{
					if (bakeCollider.isCancelledDestroy)
					{
						UnityEngine.Object.Destroy(bakeCollider.mesh);
					}
					else
					{
						Mesh sharedMesh = bakeCollider.meshCollider.sharedMesh;
						bakeCollider.meshCollider.sharedMesh = bakeCollider.mesh;
						if (sharedMesh && sharedMesh != bakeCollider.mesh)
						{
							UnityEngine.Object.Destroy(sharedMesh);
						}
					}
				}
			}
			this.bakes.Clear();
		}
	}

	public int SetBlockEntitiesVisible(bool _on, string _name)
	{
		ChunkCluster chunkCache = this.m_World.ChunkCache;
		if (chunkCache == null)
		{
			return 0;
		}
		int num = 0;
		for (int i = 0; i < this.m_AllChunkPositions.list.Count; i++)
		{
			long key = this.m_AllChunkPositions.list[i];
			Chunk chunkSync = chunkCache.GetChunkSync(key);
			if (chunkSync != null)
			{
				num += chunkSync.EnableEntityBlocks(_on, _name);
			}
		}
		return num;
	}

	[Conditional("DEBUG_CHUNK")]
	public static void TimerStart(string _name)
	{
		Dictionary<string, MicroStopwatch> obj = ChunkManager.debugTimers;
		MicroStopwatch microStopwatch;
		lock (obj)
		{
			if (!ChunkManager.debugTimers.TryGetValue(_name, out microStopwatch))
			{
				microStopwatch = new MicroStopwatch(true);
				ChunkManager.debugTimers.Add(_name, microStopwatch);
			}
		}
		microStopwatch.Restart();
	}

	[Conditional("DEBUG_CHUNK")]
	public static void TimerLog(string _name, string _format = "", params object[] _args)
	{
		Dictionary<string, MicroStopwatch> obj = ChunkManager.debugTimers;
		MicroStopwatch microStopwatch;
		lock (obj)
		{
			ChunkManager.debugTimers.TryGetValue(_name, out microStopwatch);
		}
		if (microStopwatch != null)
		{
			float num = (float)microStopwatch.ElapsedMicroseconds * 0.001f;
			int frameCount = GameManager.frameCount;
			if (ThreadManager.IsMainThread())
			{
				frameCount = Time.frameCount;
			}
			_format = string.Format("{0} {1}ms {2} {3}", new object[]
			{
				frameCount,
				num,
				_name,
				_format
			});
			Log.Warning(_format, _args);
			microStopwatch.Restart();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cReloadPosY = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cMaxChunksSupported = 100000;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cMaxChunksAroundPlayers = 15;

	public static bool GenerateCollidersOnlyAroundEntites = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public World m_World;

	[PublicizedFrom(EAccessModifier.Private)]
	public object lockObject = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public int chunksToCopyIdx;

	[PublicizedFrom(EAccessModifier.Private)]
	public ArraySegment<long> m_ChunksToCopy;

	[PublicizedFrom(EAccessModifier.Private)]
	public long[] chunksToCopyArr = new long[100000];

	[PublicizedFrom(EAccessModifier.Private)]
	public ArraySegment<long> m_ChunksToFree;

	[PublicizedFrom(EAccessModifier.Private)]
	public long[] chunksToFreeArr = new long[100000];

	[PublicizedFrom(EAccessModifier.Private)]
	public int chunksToLightIdx;

	[PublicizedFrom(EAccessModifier.Private)]
	public ArraySegment<long> m_ChunksToLight;

	[PublicizedFrom(EAccessModifier.Private)]
	public long[] chunksToLightArr = new long[100000];

	[PublicizedFrom(EAccessModifier.Private)]
	public long[] allChunkPositionsCopy = new long[100000];

	[PublicizedFrom(EAccessModifier.Private)]
	public BucketHashSetList m_ViewingChunkPositions = new BucketHashSetList(15);

	[PublicizedFrom(EAccessModifier.Private)]
	public BucketHashSetList m_AllChunkPositions = new BucketHashSetList(15);

	[PublicizedFrom(EAccessModifier.Private)]
	public BucketHashSetList m_CollisionChunkPositions = new BucketHashSetList(15);

	[PublicizedFrom(EAccessModifier.Private)]
	public long[] activeChunkSetArr = new long[100000];

	[PublicizedFrom(EAccessModifier.Private)]
	public List<ChunkGameObject> m_FreeChunkGameObjects = new List<ChunkGameObject>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<ChunkGameObject> m_UsedChunkGameObjects = new List<ChunkGameObject>();

	public List<ChunkManager.ChunkObserver> m_ObservedEntities = new List<ChunkManager.ChunkObserver>();

	[PublicizedFrom(EAccessModifier.Private)]
	public ThreadManager.ThreadInfo threadInfoRegenerating;

	[PublicizedFrom(EAccessModifier.Private)]
	public ThreadManager.ThreadInfo threadInfoCalc;

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSetList<Chunk> chunksToUnload = new HashSetList<Chunk>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Vector2i>[] rectanglesAroundPlayers;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<NetPackage> sendToClientPackages = new List<NetPackage>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Chunk currentCopiedChunk;

	[PublicizedFrom(EAccessModifier.Private)]
	public int currentClusterIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public ChunkGameObject currentCopiedChunkGameObject;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isContinueCopying;

	[PublicizedFrom(EAccessModifier.Private)]
	public int currentCopiedChunkLayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isInternalForceUpdate;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isChunkClusterChanged;

	[PublicizedFrom(EAccessModifier.Private)]
	public AutoResetEvent calcThreadWaitHandle = new AutoResetEvent(false);

	public static int MaxQueuedMeshLayers = 1000;

	public Action<Chunk> OnChunkInitialized;

	public Action<Chunk> OnChunkRegenerated;

	public Action<Chunk> OnChunkCopiedToUnity;

	public List<Chunk> ChunksToCopyInOneFrame = new List<Chunk>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int chunksToCopyInOneFrameIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public int chunksToCopyInOneFramePass;

	[PublicizedFrom(EAccessModifier.Private)]
	public Chunk[] neighborsLightingThread = new Chunk[8];

	[PublicizedFrom(EAccessModifier.Private)]
	public volatile bool bLightingDone = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public volatile bool bCalcPositionsDone = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public volatile bool isViewingOrCollisionPositionsChanged_threadCalc;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<long> viewingChunkPositionsCopy = new List<long>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<long> collisionChunkPositionsCopy = new List<long>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<long> lightingChunkPositionsCopy = new List<long>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<long> chunksToCopyTemp = new List<long>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<long> chunksToFreeTemp = new List<long>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<long> chunksToLightTemp = new List<long>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Chunk[] neighborsGenerationThread2 = new Chunk[8];

	[PublicizedFrom(EAccessModifier.Private)]
	public volatile bool isViewingOrCollisionPositionsChanged_threadReg = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public Chunk[] regenerateNextChunkNeighbors = new Chunk[8];

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cMaxCGOsToUnloadPerFrame = 8;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<ChunkGameObject> tempDisplayedCGOs = new List<ChunkGameObject>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<long> cgoToRemove = new List<long>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<BlockEntityData> groundAlignBlocks = new List<BlockEntityData>();

	[PublicizedFrom(EAccessModifier.Private)]
	public ThreadManager.ThreadInfo bakeThreadInfo;

	[PublicizedFrom(EAccessModifier.Private)]
	public AutoResetEvent bakeEvent = new AutoResetEvent(false);

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator bakeCoroutine;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<ChunkManager.BakeCollider> bakes = new List<ChunkManager.BakeCollider>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int bakeIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public volatile ChunkManager.BakeCollider bakeCurrent;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, MicroStopwatch> debugTimers = new Dictionary<string, MicroStopwatch>();

	public class ChunkObserver
	{
		public ChunkObserver(Vector3 _initialPosition, bool _bBuildVisualMeshAround, int _viewDim, int _entityIdToSendChunksTo)
		{
			this.id = ++ChunkManager.ChunkObserver.idCnt;
			this.entityIdToSendChunksTo = _entityIdToSendChunksTo;
			this.position = _initialPosition;
			this.curChunkPos = new Vector3i(int.MaxValue, 0, int.MaxValue);
			this.bBuildVisualMeshAround = _bBuildVisualMeshAround;
			this.viewDim = _viewDim;
			this.chunksLoaded = new HashSetLong();
			this.chunksToLoad = new BucketHashSetList(_viewDim + 2);
			this.chunksToReload = new List<long>();
			this.chunksToRemove = new HashSetLong();
			this.chunksAround = new BucketHashSetList(_viewDim + 2);
		}

		public void SetPosition(Vector3 _position)
		{
			this.position = _position;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static int idCnt;

		public int id;

		public Vector3 position;

		public Vector3i curChunkPos;

		public bool bBuildVisualMeshAround;

		public int viewDim;

		public HashSetLong chunksLoaded;

		public BucketHashSetList chunksToLoad;

		public List<long> chunksToReload;

		public HashSetLong chunksToRemove;

		public BucketHashSetList chunksAround;

		public int entityIdToSendChunksTo;

		public IMapChunkDatabase mapDatabase;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class ThreadRegeneratingData
	{
		public List<long> viewingChunkPositionsCopy = new List<long>();

		public List<long> collisionChunkPositionsCopy = new List<long>();
	}

	public class BakeCollider
	{
		public Mesh mesh;

		public int id;

		public MeshCollider meshCollider;

		public bool isBaked;

		public bool isCancelledDestroy;
	}
}

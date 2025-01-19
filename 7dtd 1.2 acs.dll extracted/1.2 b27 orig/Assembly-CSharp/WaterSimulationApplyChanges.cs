using System;
using System.Collections.Generic;
using System.Threading;
using Unity.Mathematics;

public class WaterSimulationApplyChanges
{
	public WaterSimulationApplyChanges(ChunkCluster _cc)
	{
		this.chunks = _cc;
		this.isServer = SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer;
		this.applyThread = ThreadManager.StartThread("WaterSimulationApplyChanges", null, new ThreadManager.ThreadFunctionLoopDelegate(this.ThreadLoop), null, ThreadPriority.Normal, null, null, true, false);
	}

	public WaterSimulationApplyChanges.ChangesForChunk.Writer GetChangeWriter(long _chunkKey)
	{
		Dictionary<long, WaterSimulationApplyChanges.ChangesForChunk> obj = this.changeCache;
		WaterSimulationApplyChanges.ChangesForChunk.Writer result;
		lock (obj)
		{
			WaterSimulationApplyChanges.ChangesForChunk changesForChunk;
			if (!this.changeCache.TryGetValue(_chunkKey, out changesForChunk))
			{
				changesForChunk = this.changesPool.AllocSync(true);
				this.changeCache.Add(_chunkKey, changesForChunk);
				this.changedChunkList.AddLast(_chunkKey);
			}
			result = new WaterSimulationApplyChanges.ChangesForChunk.Writer(changesForChunk);
		}
		return result;
	}

	public void DiscardChangesForChunks(List<long> _chunkKeys)
	{
		Dictionary<long, WaterSimulationApplyChanges.ChangesForChunk> obj = this.changeCache;
		lock (obj)
		{
			foreach (long num in _chunkKeys)
			{
				WaterSimulationApplyChanges.ChangesForChunk t;
				if (this.changeCache.TryGetValue(num, out t))
				{
					this.changesPool.FreeSync(t);
					this.changeCache.Remove(num);
					this.changedChunkList.Remove(num);
					Log.Out(string.Format("[DiscardChangesForChunks] Discarding pending water changes for chunk: {0}", num));
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int ThreadLoop(ThreadManager.ThreadInfo _threadInfo)
	{
		if (_threadInfo.TerminationRequested())
		{
			return -1;
		}
		if (this.isServer && this.networkMaxBytesPerSecond > 0L)
		{
			NetPackageMeasure obj = this.networkMeasure;
			lock (obj)
			{
				this.networkMeasure.RecalculateTotals();
			}
		}
		Chunk chunk;
		WaterSimulationApplyChanges.ChangesForChunk changesForChunk;
		if (!this.TryFindChangeToApply(out chunk, out changesForChunk))
		{
			return 15;
		}
		this.ApplyChanges(chunk, changesForChunk.changedVoxels);
		chunk.EnterWriteLock();
		chunk.InProgressWaterSim = false;
		chunk.ExitWriteLock();
		this.changesPool.FreeSync(changesForChunk);
		return 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool TryFindChangeToApply(out Chunk _chunk, out WaterSimulationApplyChanges.ChangesForChunk _changes)
	{
		Dictionary<long, WaterSimulationApplyChanges.ChangesForChunk> obj = this.changeCache;
		bool result;
		lock (obj)
		{
			if (this.changedChunkList.Count == 0)
			{
				_chunk = null;
				_changes = null;
				result = false;
			}
			else
			{
				LinkedListNode<long> linkedListNode = this.changedChunkList.First;
				while (linkedListNode != null)
				{
					long value = linkedListNode.Value;
					if (!this.changeCache.TryGetValue(value, out _changes))
					{
						LinkedListNode<long> node = linkedListNode;
						linkedListNode = linkedListNode.Next;
						this.changedChunkList.Remove(node);
					}
					else if (_changes.IsRecordingChanges)
					{
						linkedListNode = linkedListNode.Next;
					}
					else
					{
						if (WaterUtils.TryOpenChunkForUpdate(this.chunks, value, out _chunk))
						{
							LinkedListNode<long> node2 = linkedListNode;
							linkedListNode = linkedListNode.Next;
							this.changedChunkList.Remove(node2);
							this.changeCache.Remove(value);
							return true;
						}
						linkedListNode = linkedListNode.Next;
					}
				}
				_chunk = null;
				_changes = null;
				result = false;
			}
		}
		return result;
	}

	public void ApplyChanges(Chunk _chunk, Dictionary<int, WaterValue> changedVoxels)
	{
		NetPackageWaterSimChunkUpdate netPackageWaterSimChunkUpdate = null;
		if (this.isServer && SingletonMonoBehaviour<ConnectionManager>.Instance.ClientCount() > 0)
		{
			netPackageWaterSimChunkUpdate = this.SetupForSend(_chunk);
		}
		int num = 0;
		int num2 = 0;
		foreach (KeyValuePair<int, WaterValue> keyValuePair in changedVoxels)
		{
			int key = keyValuePair.Key;
			int3 voxelCoords = WaterDataHandle.GetVoxelCoords(key);
			WaterValue value = keyValuePair.Value;
			WaterValue waterValue;
			_chunk.SetWaterSimUpdate(voxelCoords.x, voxelCoords.y, voxelCoords.z, value, out waterValue);
			if (value.GetMass() != waterValue.GetMass())
			{
				if (WaterUtils.GetWaterLevel(waterValue) != WaterUtils.GetWaterLevel(value))
				{
					num2 |= 1 << voxelCoords.y / 16;
				}
				if (netPackageWaterSimChunkUpdate != null)
				{
					netPackageWaterSimChunkUpdate.AddChange((ushort)key, value);
				}
			}
		}
		if (netPackageWaterSimChunkUpdate != null)
		{
			netPackageWaterSimChunkUpdate.FinalizeSend();
			num += this.SendUpdateToClients(netPackageWaterSimChunkUpdate);
		}
		if (num2 != 0)
		{
			lock (_chunk)
			{
				int needsRegenerationAt = _chunk.NeedsRegenerationAt;
				_chunk.SetNeedsRegenerationRaw(needsRegenerationAt | num2);
			}
		}
		if (num > 0)
		{
			if (this.networkMaxBytesPerSecond > 0L)
			{
				NetPackageMeasure obj = this.networkMeasure;
				lock (obj)
				{
					this.networkMeasure.AddSample((long)num);
				}
			}
			SingletonMonoBehaviour<ConnectionManager>.Instance.FlushClientSendQueues();
		}
	}

	public bool HasNetWorkLimitBeenReached()
	{
		if (this.isServer && this.networkMaxBytesPerSecond > 0L && SingletonMonoBehaviour<ConnectionManager>.Instance.ClientCount() > 0)
		{
			NetPackageMeasure obj = this.networkMeasure;
			lock (obj)
			{
				return this.networkMeasure.totalSent > this.networkMaxBytesPerSecond;
			}
			return false;
		}
		return false;
	}

	public NetPackageWaterSimChunkUpdate SetupForSend(Chunk _chunk)
	{
		this.clientsNearChunkBuffer.Clear();
		long key = _chunk.Key;
		foreach (ClientInfo clientInfo in SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.List)
		{
			EntityPlayer entityPlayer;
			if (GameManager.Instance.World.Players.dict.TryGetValue(clientInfo.entityId, out entityPlayer) && entityPlayer.ChunkObserver.chunksAround.Contains(key))
			{
				this.clientsNearChunkBuffer.Add(clientInfo);
			}
		}
		NetPackageWaterSimChunkUpdate package = NetPackageManager.GetPackage<NetPackageWaterSimChunkUpdate>();
		package.SetupForSend(_chunk);
		return package;
	}

	public int SendUpdateToClients(NetPackageWaterSimChunkUpdate _package)
	{
		int num = 0;
		_package.RegisterSendQueue();
		foreach (ClientInfo clientInfo in this.clientsNearChunkBuffer)
		{
			clientInfo.SendPackage(_package);
			num += _package.GetLength();
		}
		_package.SendQueueHandled();
		return num;
	}

	public void Cleanup()
	{
		this.applyThread.WaitForEnd();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public MemoryPooledObject<WaterSimulationApplyChanges.ChangesForChunk> changesPool = new MemoryPooledObject<WaterSimulationApplyChanges.ChangesForChunk>(300);

	[PublicizedFrom(EAccessModifier.Private)]
	public ChunkCluster chunks;

	[PublicizedFrom(EAccessModifier.Private)]
	public ThreadManager.ThreadInfo applyThread;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int noWorkPauseDurationMs = 15;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<long, WaterSimulationApplyChanges.ChangesForChunk> changeCache = new Dictionary<long, WaterSimulationApplyChanges.ChangesForChunk>();

	[PublicizedFrom(EAccessModifier.Private)]
	public LinkedList<long> changedChunkList = new LinkedList<long>();

	[PublicizedFrom(EAccessModifier.Private)]
	public NetPackageMeasure networkMeasure = new NetPackageMeasure(1.0);

	public long networkMaxBytesPerSecond = 524288L;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<ClientInfo> clientsNearChunkBuffer = new List<ClientInfo>();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isServer;

	public class ChangesForChunk : IMemoryPoolableObject
	{
		public bool IsRecordingChanges
		{
			get
			{
				bool result;
				lock (this)
				{
					result = this.isRecordingChanges;
				}
				return result;
			}
		}

		public void Cleanup()
		{
			this.Reset();
		}

		public void Reset()
		{
			this.isRecordingChanges = false;
			this.changedVoxels.Clear();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool isRecordingChanges;

		public Dictionary<int, WaterValue> changedVoxels = new Dictionary<int, WaterValue>();

		public struct Writer : IDisposable
		{
			public Writer(WaterSimulationApplyChanges.ChangesForChunk _changes)
			{
				lock (_changes)
				{
					_changes.isRecordingChanges = true;
				}
				this.changes = _changes;
			}

			public void RecordChange(int _voxelIndex, WaterValue _waterValue)
			{
				this.changes.changedVoxels[_voxelIndex] = _waterValue;
			}

			public void Dispose()
			{
				WaterSimulationApplyChanges.ChangesForChunk obj = this.changes;
				lock (obj)
				{
					this.changes.isRecordingChanges = false;
				}
			}

			[PublicizedFrom(EAccessModifier.Private)]
			public WaterSimulationApplyChanges.ChangesForChunk changes;
		}
	}
}

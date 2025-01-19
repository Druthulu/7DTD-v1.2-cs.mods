using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile(CompileSynchronously = true)]
public struct WaterSimulationPreProcess : IJob
{
	public void Execute()
	{
		this.neighborCache = WaterNeighborCacheNative.InitializeCache(this.waterDataHandles);
		foreach (ChunkKey chunkKey in this.modifiedChunks)
		{
			WaterDataHandle waterDataHandle;
			if (this.waterDataHandles.TryGetValue(chunkKey, out waterDataHandle))
			{
				this.neighborCache.SetChunk(chunkKey);
				UnsafeParallelMultiHashMap<ChunkKey, int3>.Enumerator valuesForKey = this.voxelsToWakeup.GetValuesForKey(chunkKey);
				while (valuesForKey.MoveNext())
				{
					int3 @int = valuesForKey.Current;
					waterDataHandle.SetVoxelActive(@int.x, @int.y, @int.z);
					this.neighborCache.SetVoxel(@int.x, @int.y, @int.z);
					this.WakeNeighbor(WaterNeighborCacheNative.X_NEG);
					this.WakeNeighbor(WaterNeighborCacheNative.X_POS);
					this.WakeNeighbor(1);
					this.WakeNeighbor(-1);
					this.WakeNeighbor(WaterNeighborCacheNative.Z_NEG);
					this.WakeNeighbor(WaterNeighborCacheNative.Z_POS);
				}
				this.activeChunks.Add(chunkKey);
			}
		}
		this.modifiedChunks.Clear();
		this.voxelsToWakeup.Clear();
		NativeArray<ChunkKey> nativeArray = this.activeChunks.ToNativeArray(Allocator.Temp);
		for (int i = 0; i < nativeArray.Length; i++)
		{
			ChunkKey chunkKey2 = nativeArray[i];
			this.TryTrackChunk(chunkKey2.x + 1, chunkKey2.z);
			this.TryTrackChunk(chunkKey2.x - 1, chunkKey2.z);
			this.TryTrackChunk(chunkKey2.x, chunkKey2.z + 1);
			this.TryTrackChunk(chunkKey2.x, chunkKey2.z - 1);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WakeNeighbor(int _yOffset)
	{
		int num = this.neighborCache.voxelY + _yOffset;
		if (num < 0 || num > 255)
		{
			return;
		}
		this.neighborCache.center.SetVoxelActive(this.neighborCache.voxelX, num, this.neighborCache.voxelZ);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WakeNeighbor(int2 _xzOffset)
	{
		ChunkKey chunkKey;
		WaterDataHandle waterDataHandle;
		int x;
		int y;
		int z;
		if (this.neighborCache.TryGetNeighbor(_xzOffset, out chunkKey, out waterDataHandle, out x, out y, out z))
		{
			waterDataHandle.SetVoxelActive(x, y, z);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TryTrackChunk(int _chunkX, int _chunkZ)
	{
		ChunkKey chunkKey = new ChunkKey(_chunkX, _chunkZ);
		if (this.waterDataHandles.ContainsKey(chunkKey))
		{
			this.activeChunks.Add(chunkKey);
		}
	}

	public UnsafeParallelHashSet<ChunkKey> activeChunks;

	public UnsafeParallelHashMap<ChunkKey, WaterDataHandle> waterDataHandles;

	public UnsafeParallelHashSet<ChunkKey> modifiedChunks;

	public UnsafeParallelMultiHashMap<ChunkKey, int3> voxelsToWakeup;

	[PublicizedFrom(EAccessModifier.Private)]
	public WaterNeighborCacheNative neighborCache;
}

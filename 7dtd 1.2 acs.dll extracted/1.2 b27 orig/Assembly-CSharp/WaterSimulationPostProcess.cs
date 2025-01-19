using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

[BurstCompile(CompileSynchronously = true)]
public struct WaterSimulationPostProcess : IJob
{
	public void Execute()
	{
		for (int i = 0; i < this.nonFlowingChunks.Length; i++)
		{
			ChunkKey key = this.nonFlowingChunks[i];
			WaterDataHandle waterDataHandle;
			if (this.waterDataHandles.TryGetValue(key, out waterDataHandle))
			{
				this.activeChunks.Remove(this.nonFlowingChunks[i]);
			}
		}
		for (int j = 0; j < this.processingChunks.Length; j++)
		{
			ChunkKey chunkKey = this.processingChunks[j];
			WaterDataHandle waterDataHandle2;
			if (this.waterDataHandles.TryGetValue(chunkKey, out waterDataHandle2) && waterDataHandle2.activationsFromOtherChunks.Count > 0)
			{
				waterDataHandle2.ApplyEnqueuedActivations();
				this.activeChunks.Add(chunkKey);
			}
		}
	}

	public NativeArray<ChunkKey> processingChunks;

	public NativeList<ChunkKey> nonFlowingChunks;

	public UnsafeParallelHashSet<ChunkKey> activeChunks;

	public UnsafeParallelHashMap<ChunkKey, WaterDataHandle> waterDataHandles;
}

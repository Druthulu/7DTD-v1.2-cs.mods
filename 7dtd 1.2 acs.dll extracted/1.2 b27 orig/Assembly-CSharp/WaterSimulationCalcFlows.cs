using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile(CompileSynchronously = true)]
public struct WaterSimulationCalcFlows : IJobParallelFor
{
	public void Execute(int chunkIndex)
	{
		ChunkKey chunkKey = this.processingChunks[chunkIndex];
		this.stats = this.waterStats[chunkIndex];
		this.neighborCache = WaterNeighborCacheNative.InitializeCache(this.waterDataHandles);
		this.ProcessFlows(chunkKey);
		this.waterStats[chunkIndex] = this.stats;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ProcessFlows(ChunkKey chunkKey)
	{
		WaterDataHandle chunkData;
		if (this.waterDataHandles.TryGetValue(chunkKey, out chunkData))
		{
			this.stats.NumChunksProcessed = this.stats.NumChunksProcessed + 1;
			if (!chunkData.HasActiveWater)
			{
				return;
			}
			this.stats.NumChunksActive = this.stats.NumChunksActive + 1;
			this.neighborCache.SetChunk(chunkKey);
			UnsafeBitArraySetIndicesEnumerator activeVoxelIndices = chunkData.ActiveVoxelIndices;
			while (activeVoxelIndices.MoveNext())
			{
				this.stats.NumVoxelsProcessed = this.stats.NumVoxelsProcessed + 1;
				int num = activeVoxelIndices.Current;
				int3 voxelCoords = WaterDataHandle.GetVoxelCoords(num);
				int num2 = chunkData.voxelData.Get(num);
				this.neighborCache.SetVoxel(voxelCoords.x, voxelCoords.y, voxelCoords.z);
				if (chunkData.IsInGroundWater(voxelCoords.x, voxelCoords.y, voxelCoords.z))
				{
					WaterVoxelState fromVoxelState = chunkData.voxelState.Get(voxelCoords.x, voxelCoords.y, voxelCoords.z);
					if (fromVoxelState.IsSolid())
					{
						chunkData.SetVoxelInactive(num);
						this.stats.NumVoxelsPutToSleep = this.stats.NumVoxelsPutToSleep + 1;
					}
					else if (num2 != 19500)
					{
						chunkData.ApplyFlow(num, 19500);
					}
					else if (num2 > 195 && this.ProcessGroundWaterFlowSide(chunkKey, fromVoxelState, num2, WaterNeighborCacheNative.X_NEG) + this.ProcessGroundWaterFlowSide(chunkKey, fromVoxelState, num2, WaterNeighborCacheNative.X_POS) + this.ProcessGroundWaterFlowSide(chunkKey, fromVoxelState, num2, WaterNeighborCacheNative.Z_NEG) + this.ProcessGroundWaterFlowSide(chunkKey, fromVoxelState, num2, WaterNeighborCacheNative.Z_POS) < 195)
					{
						chunkData.SetVoxelInactive(num);
						this.stats.NumVoxelsPutToSleep = this.stats.NumVoxelsPutToSleep + 1;
					}
				}
				else
				{
					int num3 = num2;
					if (num3 < 195)
					{
						chunkData.SetVoxelInactive(num);
						this.stats.NumVoxelsPutToSleep = this.stats.NumVoxelsPutToSleep + 1;
					}
					else
					{
						int num4 = num3;
						num3 -= this.ProcessFlowBelow(chunkData, num, voxelCoords.x, voxelCoords.y, voxelCoords.z, num3);
						if (num3 > 0)
						{
							num3 -= this.ProcessOverfull(chunkData, num, voxelCoords.x, voxelCoords.y, voxelCoords.z, num3);
							int num5 = this.ProcessFlowSide(chunkKey, chunkData, num, num3, WaterNeighborCacheNative.X_NEG);
							num5 += this.ProcessFlowSide(chunkKey, chunkData, num, num3, WaterNeighborCacheNative.X_POS);
							num5 += this.ProcessFlowSide(chunkKey, chunkData, num, num3, WaterNeighborCacheNative.Z_NEG);
							num5 += this.ProcessFlowSide(chunkKey, chunkData, num, num3, WaterNeighborCacheNative.Z_POS);
							num3 -= num5;
							if (num3 > 0 && num4 - num3 < 195)
							{
								chunkData.SetVoxelInactive(num);
								this.stats.NumVoxelsPutToSleep = this.stats.NumVoxelsPutToSleep + 1;
							}
						}
					}
				}
			}
			activeVoxelIndices.Dispose();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int ProcessFlowBelow(WaterDataHandle _chunkData, int _voxelIndex, int _x, int _y, int _z, int _mass)
	{
		int num = _y - 1;
		if (num < 0)
		{
			return 0;
		}
		if (_chunkData.voxelState.Get(_x, _y, _z).IsSolidYNeg() || _chunkData.voxelState.Get(_x, num, _z).IsSolidYPos())
		{
			return 0;
		}
		if (_chunkData.IsInGroundWater(_x, num, _z))
		{
			_chunkData.ApplyFlow(_voxelIndex, -_mass);
			this.stats.NumFlowEvents = this.stats.NumFlowEvents + 1;
			return _mass;
		}
		int num2;
		if (!this.TryGetMass(_chunkData, _x, num, _z, out num2))
		{
			return 0;
		}
		int num3 = WaterConstants.GetStableMassBelow(_mass, num2) - num2;
		if (num3 > 0)
		{
			num3 = (int)((float)num3 * 0.5f);
			num3 = math.clamp(num3, 1, _mass);
			_chunkData.ApplyFlow(_voxelIndex, -num3);
			_chunkData.ApplyFlow(_x, num, _z, num3);
			this.stats.NumFlowEvents = this.stats.NumFlowEvents + 1;
			return num3;
		}
		return 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int ProcessOverfull(WaterDataHandle _chunkData, int _voxelIndex, int _x, int _y, int _z, int _mass)
	{
		if (_mass < 19500)
		{
			return 0;
		}
		int num = _y + 1;
		if (num > 255)
		{
			return 0;
		}
		if (_chunkData.voxelState.Get(_x, _y, _z).IsSolidYPos() || _chunkData.voxelState.Get(_x, num, _z).IsSolidYNeg())
		{
			return 0;
		}
		int num2;
		if (!this.TryGetMass(_chunkData, _x, num, _z, out num2))
		{
			return 0;
		}
		int num3 = math.min(_mass - 19500, 58500 - num2);
		if (num3 > 195)
		{
			num3 = math.clamp(num3, 1, _mass);
			_chunkData.ApplyFlow(_voxelIndex, -num3);
			_chunkData.ApplyFlow(_x, num, _z, num3);
			this.stats.NumFlowEvents = this.stats.NumFlowEvents + 1;
			return num3;
		}
		return 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int ProcessFlowSide(ChunkKey _chunkKey, WaterDataHandle _chunkData, int _voxelIndex, int _mass, int2 _xzOffset)
	{
		WaterVoxelState waterVoxelState = _chunkData.voxelState.Get(_voxelIndex);
		if (waterVoxelState.IsSolidXZ(_xzOffset))
		{
			return 0;
		}
		ChunkKey other;
		WaterDataHandle chunkData;
		int x;
		int num;
		int z;
		if (this.neighborCache.TryGetNeighbor(_xzOffset, out other, out chunkData, out x, out num, out z))
		{
			WaterVoxelState waterVoxelState2 = chunkData.voxelState.Get(x, num, z);
			if (waterVoxelState2.IsSolidXZ(-_xzOffset))
			{
				return 0;
			}
			int num2;
			if (!this.TryGetMass(chunkData, x, num, z, out num2))
			{
				return 0;
			}
			int num3 = num - 1;
			bool flag = true;
			bool flag2 = true;
			if (num3 >= 0)
			{
				flag = (_chunkData.voxelState.Get(this.neighborCache.voxelX, num3, this.neighborCache.voxelZ).IsSolidYPos() || waterVoxelState.IsSolidYNeg());
				flag2 = (chunkData.voxelState.Get(x, num3, z).IsSolidYPos() || waterVoxelState2.IsSolidYNeg());
			}
			int num4 = 195;
			if (flag == flag2)
			{
				if (_mass <= 4875)
				{
					return 0;
				}
			}
			else
			{
				num4 = 0;
			}
			int num5 = (int)((float)(_mass - num2) * 0.5f);
			num5 = math.clamp(num5, 0, (int)((float)_mass * 0.25f));
			if (num5 > num4)
			{
				num5 = (int)((float)num5 * 0.5f);
				num5 = math.clamp(num5, 1, _mass);
				_chunkData.ApplyFlow(_voxelIndex, -num5);
				if (_chunkKey.Equals(other))
				{
					chunkData.ApplyFlow(x, num, z, num5);
				}
				else
				{
					chunkData.EnqueueFlow(WaterDataHandle.GetVoxelIndex(x, num, z), num5);
				}
				this.stats.NumFlowEvents = this.stats.NumFlowEvents + 1;
				return num5;
			}
		}
		return 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int ProcessGroundWaterFlowSide(ChunkKey _chunkKey, WaterVoxelState _fromVoxelState, int _mass, int2 _xzOffset)
	{
		if (_fromVoxelState.IsSolidXZ(_xzOffset))
		{
			return 0;
		}
		ChunkKey other;
		WaterDataHandle chunkData;
		int x;
		int y;
		int z;
		if (this.neighborCache.TryGetNeighbor(_xzOffset, out other, out chunkData, out x, out y, out z))
		{
			if (chunkData.voxelState.Get(x, y, z).IsSolidXZ(-_xzOffset))
			{
				return 0;
			}
			int num;
			if (!this.TryGetMass(chunkData, x, y, z, out num))
			{
				return 0;
			}
			int num2 = math.max(19500 - num, 0);
			int num3 = math.min(_mass, (int)((float)num2 * 0.25f));
			if (num3 > 195)
			{
				num3 = (int)((float)num3 * 0.5f);
				num3 = math.clamp(num3, 1, _mass);
				if (_chunkKey.Equals(other))
				{
					chunkData.ApplyFlow(x, y, z, num3);
				}
				else
				{
					chunkData.EnqueueFlow(WaterDataHandle.GetVoxelIndex(x, y, z), num3);
				}
				this.stats.NumFlowEvents = this.stats.NumFlowEvents + 1;
				return num3;
			}
		}
		return 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool TryGetMass(WaterDataHandle _chunkData, int _x, int _y, int _z, out int _mass)
	{
		if (_chunkData.IsInGroundWater(_x, _y, _z))
		{
			_mass = 19500;
			return false;
		}
		int num = _chunkData.voxelData.Get(_x, _y, _z);
		if (num > 195)
		{
			_mass = num;
			return true;
		}
		_mass = 0;
		return !_chunkData.voxelState.Get(_x, _y, _z).IsSolid();
	}

	public NativeArray<ChunkKey> processingChunks;

	public NativeArray<WaterStats> waterStats;

	public UnsafeParallelHashMap<ChunkKey, WaterDataHandle> waterDataHandles;

	[PublicizedFrom(EAccessModifier.Private)]
	public WaterStats stats;

	[PublicizedFrom(EAccessModifier.Private)]
	public WaterNeighborCacheNative neighborCache;
}

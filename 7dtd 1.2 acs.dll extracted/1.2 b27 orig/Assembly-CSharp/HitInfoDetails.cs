using System;
using UnityEngine;

public struct HitInfoDetails
{
	public BlockValue blockValue
	{
		get
		{
			return this.voxelData.BlockValue;
		}
	}

	public WaterValue waterValue
	{
		get
		{
			return this.voxelData.WaterValue;
		}
	}

	public void Clear()
	{
		this.pos = Vector3.zero;
		this.blockPos = Vector3i.zero;
		this.blockFace = BlockFace.Top;
		this.voxelData.Clear();
		this.clrIdx = 0;
		this.distanceSq = 0f;
	}

	public void CopyFrom(HitInfoDetails _other)
	{
		this.pos = _other.pos;
		this.blockPos = _other.blockPos;
		this.blockFace = _other.blockFace;
		this.voxelData = _other.voxelData;
		this.clrIdx = _other.clrIdx;
		this.distanceSq = _other.distanceSq;
	}

	public HitInfoDetails Clone()
	{
		return new HitInfoDetails
		{
			pos = this.pos,
			blockPos = this.blockPos,
			blockFace = this.blockFace,
			voxelData = this.voxelData,
			clrIdx = this.clrIdx,
			distanceSq = this.distanceSq
		};
	}

	public Vector3 pos;

	public Vector3i blockPos;

	public HitInfoDetails.VoxelData voxelData;

	public BlockFace blockFace;

	public float distanceSq;

	public int clrIdx;

	public struct VoxelData : IEquatable<HitInfoDetails.VoxelData>
	{
		public void Set(BlockValue _bv, WaterValue _wv)
		{
			this.BlockValue = _bv;
			this.WaterValue = _wv;
		}

		public static HitInfoDetails.VoxelData GetFrom(ChunkCluster _cc, Vector3i _blockPos)
		{
			return new HitInfoDetails.VoxelData
			{
				BlockValue = _cc.GetBlock(_blockPos),
				WaterValue = _cc.GetWater(_blockPos)
			};
		}

		public static HitInfoDetails.VoxelData GetFrom(World _world, Vector3i _blockPos)
		{
			return new HitInfoDetails.VoxelData
			{
				BlockValue = _world.GetBlock(_blockPos),
				WaterValue = _world.GetWater(_blockPos)
			};
		}

		public static HitInfoDetails.VoxelData GetFrom(IChunk _chunk, int _x, int _y, int _z)
		{
			return new HitInfoDetails.VoxelData
			{
				BlockValue = _chunk.GetBlock(_x, _y, _z),
				WaterValue = _chunk.GetWater(_x, _y, _z)
			};
		}

		public bool IsOnlyAir()
		{
			return this.BlockValue.isair && !this.WaterValue.HasMass();
		}

		public bool IsOnlyWater()
		{
			return this.BlockValue.isair && this.WaterValue.HasMass();
		}

		public bool Equals(HitInfoDetails.VoxelData _other)
		{
			return this.BlockValue.Equals(_other.BlockValue) && this.WaterValue.HasMass() == _other.WaterValue.HasMass();
		}

		public void Clear()
		{
			this.BlockValue = BlockValue.Air;
			this.WaterValue = WaterValue.Empty;
		}

		public BlockValue BlockValue;

		public WaterValue WaterValue;
	}
}

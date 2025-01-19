using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageWaterSimChunkUpdate : NetPackage, IMemoryPoolableObject
{
	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToClient;
		}
	}

	public static int GetPoolSize()
	{
		return 200;
	}

	public void SetupForSend(Chunk chunk)
	{
		this.ms = MemoryPools.poolMemoryStream.AllocSync(true);
		this.sendWriter = MemoryPools.poolBinaryWriter.AllocSync(true);
		this.ms.Position = 0L;
		this.sendWriter.SetBaseStream(this.ms);
		this.sendWriter.Write(chunk.X);
		this.sendWriter.Write(chunk.Z);
		this.lengthStreamPos = this.ms.Position;
		this.sendWriter.Write(0);
	}

	public void AddChange(ushort _voxelIndex, WaterValue _newValue)
	{
		this.sendWriter.Write(_voxelIndex);
		_newValue.Write(this.sendWriter);
		this.numVoxelUpdates++;
	}

	public void FinalizeSend()
	{
		this.ms.Position = this.lengthStreamPos;
		this.sendWriter.Write(this.numVoxelUpdates);
		this.sendLength = (int)this.ms.Length;
		this.sendBytes = MemoryPools.poolByte.Alloc(this.sendLength);
		this.ms.Position = 0L;
		this.ms.Read(this.sendBytes, 0, this.sendLength);
		MemoryPools.poolBinaryWriter.FreeSync(this.sendWriter);
		this.sendWriter = null;
		MemoryPools.poolMemoryStream.FreeSync(this.ms);
		this.ms = null;
	}

	public override void read(PooledBinaryReader _br)
	{
		this.ms = MemoryPools.poolMemoryStream.AllocSync(true);
		this.ms.Position = 0L;
		int length = _br.ReadInt32();
		StreamUtils.StreamCopy(_br.BaseStream, this.ms, length, null, true);
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(this.sendLength);
		_bw.Write(this.sendBytes, 0, this.sendLength);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(true))
		{
			this.ms.Position = 0L;
			pooledBinaryReader.SetBaseStream(this.ms);
			int x = pooledBinaryReader.ReadInt32();
			int y = pooledBinaryReader.ReadInt32();
			long chunkKey = WorldChunkCache.MakeChunkKey(x, y);
			WaterSimulationNative.Instance.changeApplier.GetChangeWriter(chunkKey);
			using (WaterSimulationApplyChanges.ChangesForChunk.Writer changeWriter = WaterSimulationNative.Instance.changeApplier.GetChangeWriter(chunkKey))
			{
				int num = pooledBinaryReader.ReadInt32();
				for (int i = 0; i < num; i++)
				{
					int voxelIndex = (int)pooledBinaryReader.ReadUInt16();
					WaterValue waterValue = WaterValue.FromStream(pooledBinaryReader);
					changeWriter.RecordChange(voxelIndex, waterValue);
				}
			}
		}
	}

	public override int GetLength()
	{
		return this.sendLength + 4;
	}

	public void Reset()
	{
		if (this.sendWriter != null)
		{
			MemoryPools.poolBinaryWriter.FreeSync(this.sendWriter);
			this.sendWriter = null;
		}
		if (this.ms != null)
		{
			MemoryPools.poolMemoryStream.FreeSync(this.ms);
			this.ms = null;
		}
		if (this.sendBytes != null)
		{
			MemoryPools.poolByte.Free(this.sendBytes);
			this.sendBytes = null;
		}
		this.lengthStreamPos = 0L;
		this.numVoxelUpdates = 0;
		this.sendLength = 0;
	}

	public void Cleanup()
	{
		this.Reset();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public PooledExpandableMemoryStream ms;

	[PublicizedFrom(EAccessModifier.Private)]
	public PooledBinaryWriter sendWriter;

	[PublicizedFrom(EAccessModifier.Private)]
	public long lengthStreamPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public int numVoxelUpdates;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] sendBytes;

	[PublicizedFrom(EAccessModifier.Private)]
	public int sendLength;
}

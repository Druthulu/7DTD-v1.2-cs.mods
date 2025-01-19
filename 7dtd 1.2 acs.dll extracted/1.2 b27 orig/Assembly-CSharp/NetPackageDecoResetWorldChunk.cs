using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageDecoResetWorldChunk : NetPackage
{
	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToClient;
		}
	}

	public NetPackageDecoResetWorldChunk Setup(long _chunkKey)
	{
		using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(false))
		{
			pooledBinaryWriter.SetBaseStream(this.ms);
			pooledBinaryWriter.Write(_chunkKey);
		}
		return this;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public ~NetPackageDecoResetWorldChunk()
	{
		MemoryPools.poolMemoryStream.FreeSync(this.ms);
	}

	public override void read(PooledBinaryReader _br)
	{
		int length = _br.ReadInt32();
		StreamUtils.StreamCopy(_br.BaseStream, this.ms, length, null, true);
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write((int)this.ms.Length);
		this.ms.WriteTo(_bw.BaseStream);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(false))
		{
			PooledExpandableMemoryStream obj = this.ms;
			lock (obj)
			{
				pooledBinaryReader.SetBaseStream(this.ms);
				this.ms.Position = 0L;
				long worldChunkKey = pooledBinaryReader.ReadInt64();
				DecoManager.Instance.ResetDecosForWorldChunk(worldChunkKey);
			}
		}
	}

	public override int GetLength()
	{
		return (int)this.ms.Length;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public PooledExpandableMemoryStream ms = MemoryPools.poolMemoryStream.AllocSync(true);
}

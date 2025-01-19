using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageDecoResetWorldRect : NetPackage
{
	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToClient;
		}
	}

	public NetPackageDecoResetWorldRect Setup(Rect _worldRect)
	{
		using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(false))
		{
			pooledBinaryWriter.SetBaseStream(this.ms);
			pooledBinaryWriter.Write((int)_worldRect.x);
			pooledBinaryWriter.Write((int)_worldRect.y);
			pooledBinaryWriter.Write((int)_worldRect.width);
			pooledBinaryWriter.Write((int)_worldRect.height);
		}
		return this;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public ~NetPackageDecoResetWorldRect()
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
				int num = pooledBinaryReader.ReadInt32();
				int num2 = pooledBinaryReader.ReadInt32();
				int num3 = pooledBinaryReader.ReadInt32();
				int num4 = pooledBinaryReader.ReadInt32();
				Rect worldRect = new Rect((float)num, (float)num2, (float)num3, (float)num4);
				DecoManager.Instance.ResetDecosInWorldRect(worldRect);
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

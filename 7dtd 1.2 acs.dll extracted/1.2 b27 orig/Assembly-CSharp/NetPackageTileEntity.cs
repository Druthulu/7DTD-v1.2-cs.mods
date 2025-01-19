using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageTileEntity : NetPackage
{
	public NetPackageTileEntity Setup(TileEntity _te, TileEntity.StreamModeWrite _eStreamMode)
	{
		return this.Setup(_te, _eStreamMode, byte.MaxValue);
	}

	public NetPackageTileEntity Setup(TileEntity _te, TileEntity.StreamModeWrite _eStreamMode, byte _handle)
	{
		this.handle = _handle;
		this.teEntityId = _te.entityId;
		this.teWorldPos = _te.ToWorldPos();
		this.bValidEntityId = (this.teEntityId != -1);
		this.clrIdx = _te.GetClrIdx();
		using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(false))
		{
			pooledBinaryWriter.SetBaseStream(this.ms);
			_te.write(pooledBinaryWriter, _eStreamMode);
		}
		return this;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public ~NetPackageTileEntity()
	{
		MemoryPools.poolMemoryStream.FreeSync(this.ms);
	}

	public override void read(PooledBinaryReader _br)
	{
		this.handle = _br.ReadByte();
		this.bValidEntityId = _br.ReadBoolean();
		if (this.bValidEntityId)
		{
			this.teEntityId = _br.ReadInt32();
		}
		else
		{
			this.clrIdx = (int)_br.ReadUInt16();
			this.teWorldPos = StreamUtils.ReadVector3i(_br);
			this.teEntityId = -1;
		}
		int length = (int)_br.ReadUInt16();
		StreamUtils.StreamCopy(_br.BaseStream, this.ms, length, null, true);
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(this.handle);
		_bw.Write(this.bValidEntityId);
		if (this.bValidEntityId)
		{
			_bw.Write(this.teEntityId);
		}
		else
		{
			_bw.Write((ushort)this.clrIdx);
			StreamUtils.Write(_bw, this.teWorldPos);
		}
		_bw.Write((ushort)this.ms.Length);
		this.ms.WriteTo(_bw.BaseStream);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		TileEntity tileEntity = this.bValidEntityId ? _world.GetTileEntity(this.teEntityId) : _world.GetTileEntity(this.clrIdx, this.teWorldPos);
		if (tileEntity == null)
		{
			return;
		}
		tileEntity.SetHandle(this.handle);
		using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(false))
		{
			PooledExpandableMemoryStream obj = this.ms;
			lock (obj)
			{
				pooledBinaryReader.SetBaseStream(this.ms);
				this.ms.Position = 0L;
				tileEntity.read(pooledBinaryReader, _world.IsRemote() ? TileEntity.StreamModeRead.FromServer : TileEntity.StreamModeRead.FromClient);
			}
		}
		tileEntity.NotifyListeners();
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			tileEntity.SetChunkModified();
			Vector3? entitiesInRangeOfWorldPos = new Vector3?(tileEntity.ToWorldCenterPos());
			if (entitiesInRangeOfWorldPos.Value == Vector3.zero)
			{
				entitiesInRangeOfWorldPos = null;
			}
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageTileEntity>().Setup(tileEntity, TileEntity.StreamModeWrite.ToClient, this.handle), true, -1, -1, -1, entitiesInRangeOfWorldPos, 192);
		}
	}

	public override int GetLength()
	{
		return (int)(22L + this.ms.Length);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public byte handle;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bValidEntityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public int teEntityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public int clrIdx;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i teWorldPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public PooledExpandableMemoryStream ms = MemoryPools.poolMemoryStream.AllocSync(true);
}

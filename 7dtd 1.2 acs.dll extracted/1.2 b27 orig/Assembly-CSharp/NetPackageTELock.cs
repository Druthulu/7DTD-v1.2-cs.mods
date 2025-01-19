using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageTELock : NetPackage
{
	public NetPackageTELock Setup(NetPackageTELock.TELockType _type, int _clrIdx, Vector3i _pos, int _lootEntityId, int _entityIdThatOpenedIt, string _customUi = null, bool _allowEmptyDestroy = true)
	{
		this.type = _type;
		this.clrIdx = _clrIdx;
		this.posX = _pos.x;
		this.posY = _pos.y;
		this.posZ = _pos.z;
		this.lootEntityId = _lootEntityId;
		this.entityIdThatOpenedIt = _entityIdThatOpenedIt;
		this.customUi = (_customUi ?? "");
		this.allowEmptyDestroy = _allowEmptyDestroy;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		this.type = (NetPackageTELock.TELockType)_br.ReadByte();
		this.clrIdx = (int)_br.ReadInt16();
		this.posX = _br.ReadInt32();
		this.posY = _br.ReadInt32();
		this.posZ = _br.ReadInt32();
		this.lootEntityId = _br.ReadInt32();
		this.entityIdThatOpenedIt = _br.ReadInt32();
		this.customUi = _br.ReadString();
		this.allowEmptyDestroy = _br.ReadBoolean();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write((byte)this.type);
		_bw.Write((short)this.clrIdx);
		_bw.Write(this.posX);
		_bw.Write(this.posY);
		_bw.Write(this.posZ);
		_bw.Write(this.lootEntityId);
		_bw.Write(this.entityIdThatOpenedIt);
		_bw.Write(this.customUi);
		_bw.Write(this.allowEmptyDestroy);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (this.type != NetPackageTELock.TELockType.UnlockServer && !base.ValidEntityIdForSender(this.entityIdThatOpenedIt, false))
		{
			return;
		}
		switch (this.type)
		{
		case NetPackageTELock.TELockType.LockServer:
			_world.GetGameManager().TELockServer(this.clrIdx, new Vector3i(this.posX, this.posY, this.posZ), this.lootEntityId, this.entityIdThatOpenedIt, this.customUi);
			return;
		case NetPackageTELock.TELockType.UnlockServer:
			_world.GetGameManager().TEUnlockServer(this.clrIdx, new Vector3i(this.posX, this.posY, this.posZ), this.lootEntityId, this.allowEmptyDestroy);
			return;
		case NetPackageTELock.TELockType.AccessClient:
			_world.GetGameManager().TEAccessClient(this.clrIdx, new Vector3i(this.posX, this.posY, this.posZ), this.lootEntityId, this.entityIdThatOpenedIt, this.customUi);
			return;
		case NetPackageTELock.TELockType.DeniedAccess:
			_world.GetGameManager().TEDeniedAccessClient(this.clrIdx, new Vector3i(this.posX, this.posY, this.posZ), this.lootEntityId, this.entityIdThatOpenedIt);
			return;
		default:
			return;
		}
	}

	public override int GetLength()
	{
		return 27 + this.customUi.Length * 2 + 1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public NetPackageTELock.TELockType type;

	[PublicizedFrom(EAccessModifier.Private)]
	public int clrIdx;

	[PublicizedFrom(EAccessModifier.Private)]
	public int posX;

	[PublicizedFrom(EAccessModifier.Private)]
	public int posY;

	[PublicizedFrom(EAccessModifier.Private)]
	public int posZ;

	[PublicizedFrom(EAccessModifier.Private)]
	public int lootEntityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public int entityIdThatOpenedIt;

	[PublicizedFrom(EAccessModifier.Private)]
	public string customUi;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool allowEmptyDestroy;

	public enum TELockType : byte
	{
		LockServer,
		UnlockServer,
		AccessClient,
		DeniedAccess
	}
}

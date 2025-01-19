using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageEntityCollect : NetPackage
{
	public NetPackageEntityCollect Setup(int _entityId, int _playerId)
	{
		this.entityId = _entityId;
		this.playerId = _playerId;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		this.entityId = _br.ReadInt32();
		this.playerId = _br.ReadInt32();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(this.entityId);
		_bw.Write(this.playerId);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		if (!base.ValidEntityIdForSender(this.playerId, false))
		{
			return;
		}
		if (!_world.IsRemote())
		{
			_world.GetGameManager().CollectEntityServer(this.entityId, this.playerId);
			return;
		}
		_world.GetGameManager().CollectEntityClient(this.entityId, this.playerId);
	}

	public override int GetLength()
	{
		return 8;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int entityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public int playerId;
}

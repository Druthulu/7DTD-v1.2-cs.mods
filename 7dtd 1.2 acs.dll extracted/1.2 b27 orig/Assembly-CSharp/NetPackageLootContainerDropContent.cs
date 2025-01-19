using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageLootContainerDropContent : NetPackage
{
	public NetPackageLootContainerDropContent Setup(Vector3i _worldPos, int _lootEntityId)
	{
		this.worldPos = _worldPos;
		this.lootEntityId = _lootEntityId;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		this.worldPos = StreamUtils.ReadVector3i(_br);
		this.lootEntityId = _br.ReadInt32();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		StreamUtils.Write(_bw, this.worldPos);
		_bw.Write(this.lootEntityId);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		_world.GetGameManager().DropContentOfLootContainerServer(_world.GetBlock(this.worldPos), this.worldPos, this.lootEntityId, null);
	}

	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToServer;
		}
	}

	public override int GetLength()
	{
		return 16;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i worldPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public int lootEntityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack[] items;

	[PublicizedFrom(EAccessModifier.Private)]
	public int oldBlockType = -1;
}

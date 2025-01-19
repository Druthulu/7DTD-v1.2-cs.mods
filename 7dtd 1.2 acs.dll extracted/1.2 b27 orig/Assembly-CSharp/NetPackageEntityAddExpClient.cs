using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageEntityAddExpClient : NetPackage
{
	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToClient;
		}
	}

	public NetPackageEntityAddExpClient Setup(int _entityId, int _xp, Progression.XPTypes _xpType)
	{
		this.entityId = _entityId;
		this.xp = _xp;
		this.xpType = (int)_xpType;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		this.entityId = _reader.ReadInt32();
		this.xp = _reader.ReadInt32();
		this.xpType = (int)_reader.ReadInt16();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(this.entityId);
		_writer.Write(this.xp);
		_writer.Write((short)this.xpType);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		EntityAlive entityAlive = (EntityAlive)_world.GetEntity(this.entityId);
		if (entityAlive != null)
		{
			string cvarXPName = "_xpOther";
			if (this.xpType == 0)
			{
				cvarXPName = "_xpFromKill";
			}
			entityAlive.Progression.AddLevelExp(this.xp, cvarXPName, (Progression.XPTypes)this.xpType, true, true);
		}
	}

	public override int GetLength()
	{
		return 8;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public int entityId;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int xp;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int xpType;
}

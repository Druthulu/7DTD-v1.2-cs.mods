using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageModifyCVar : NetPackage
{
	public NetPackageModifyCVar Setup(EntityAlive entity, string _cvarName, float _value)
	{
		this.m_entityId = entity.entityId;
		this.cvarName = _cvarName;
		this.value = _value;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		this.m_entityId = _reader.ReadInt32();
		this.cvarName = _reader.ReadString();
		this.value = _reader.ReadSingle();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(this.m_entityId);
		_writer.Write(this.cvarName);
		_writer.Write(this.value);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		EntityAlive entityAlive = _world.GetEntity(this.m_entityId) as EntityAlive;
		if (entityAlive != null)
		{
			entityAlive.Buffs.SetCustomVar(this.cvarName, this.value, SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer);
		}
	}

	public override int GetLength()
	{
		return 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_entityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public string cvarName;

	[PublicizedFrom(EAccessModifier.Private)]
	public float value;
}

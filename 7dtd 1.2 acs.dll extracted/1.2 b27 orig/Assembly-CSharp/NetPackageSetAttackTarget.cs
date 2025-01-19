using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageSetAttackTarget : NetPackage
{
	public NetPackageSetAttackTarget Setup(int entityId, int targetId)
	{
		this.m_entityId = entityId;
		this.m_targetId = targetId;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		this.m_entityId = _reader.ReadInt32();
		this.m_targetId = _reader.ReadInt32();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(this.m_entityId);
		_writer.Write(this.m_targetId);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		EntityAlive entityAlive = _world.GetEntity(this.m_entityId) as EntityAlive;
		if (entityAlive == null)
		{
			return;
		}
		EntityAlive attackTargetClient = _world.GetEntity(this.m_targetId) as EntityAlive;
		entityAlive.SetAttackTargetClient(attackTargetClient);
	}

	public override int GetLength()
	{
		return 8;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_targetId;

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_entityId;
}

using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageEntityRagdoll : NetPackage
{
	public NetPackageEntityRagdoll Setup(Entity _entity, sbyte _state)
	{
		this.entityId = _entity.entityId;
		this.state = _state;
		return this;
	}

	public NetPackageEntityRagdoll Setup(Entity _entity, float _duration, EnumBodyPartHit _bodyPart, Vector3 _forceVec, Vector3 _forceWorldPos)
	{
		this.entityId = _entity.entityId;
		this.state = -1;
		this.duration = _duration;
		this.bodyPart = _bodyPart;
		this.forceVec = _forceVec;
		this.forceWorldPos = _forceWorldPos;
		this.hipPos = _entity.emodel.GetHipPosition();
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		this.entityId = _br.ReadInt32();
		this.state = _br.ReadSByte();
		if (this.state < 0)
		{
			this.duration = _br.ReadSingle();
			this.bodyPart = (EnumBodyPartHit)_br.ReadInt16();
			this.forceVec = StreamUtils.ReadVector3(_br);
			this.forceWorldPos = StreamUtils.ReadVector3(_br);
			this.hipPos = StreamUtils.ReadVector3(_br);
		}
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(this.entityId);
		_bw.Write(this.state);
		if (this.state < 0)
		{
			_bw.Write(this.duration);
			_bw.Write((short)this.bodyPart);
			StreamUtils.Write(_bw, this.forceVec);
			StreamUtils.Write(_bw, this.forceWorldPos);
			StreamUtils.Write(_bw, this.hipPos);
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		EntityAlive entityAlive = _world.GetEntity(this.entityId) as EntityAlive;
		if (entityAlive == null)
		{
			Log.Out("Discarding " + base.GetType().Name + " for entity Id=" + this.entityId.ToString());
			return;
		}
		if (this.state < 0)
		{
			entityAlive.emodel.DoRagdoll(this.duration, this.bodyPart, this.forceVec, this.forceWorldPos, true);
			return;
		}
		entityAlive.emodel.SetRagdollState((int)this.state);
	}

	public override int GetLength()
	{
		return 48;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int entityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public sbyte state;

	[PublicizedFrom(EAccessModifier.Private)]
	public float duration;

	[PublicizedFrom(EAccessModifier.Private)]
	public EnumBodyPartHit bodyPart;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 forceVec;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 forceWorldPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 hipPos;
}

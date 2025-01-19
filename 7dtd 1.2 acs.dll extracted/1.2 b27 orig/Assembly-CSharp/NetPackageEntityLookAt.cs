﻿using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageEntityLookAt : NetPackage
{
	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToClient;
		}
	}

	public NetPackageEntityLookAt Setup(int _entityId, Vector3 _lookAtPosition)
	{
		this.entityId = _entityId;
		this.lookAtPosition = _lookAtPosition;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		this.entityId = _reader.ReadInt32();
		this.lookAtPosition = new Vector3((float)_reader.ReadInt32(), (float)_reader.ReadInt32(), (float)_reader.ReadInt32());
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(this.entityId);
		_writer.Write((int)this.lookAtPosition.x);
		_writer.Write((int)this.lookAtPosition.y);
		_writer.Write((int)this.lookAtPosition.z);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		EntityAlive entityAlive = (EntityAlive)_world.GetEntity(this.entityId);
		if (entityAlive != null && entityAlive.emodel != null && entityAlive.emodel.avatarController != null)
		{
			entityAlive.emodel.avatarController.SetLookPosition(this.lookAtPosition);
		}
	}

	public override int GetLength()
	{
		return 8;
	}

	public int entityId;

	public Vector3 lookAtPosition;
}

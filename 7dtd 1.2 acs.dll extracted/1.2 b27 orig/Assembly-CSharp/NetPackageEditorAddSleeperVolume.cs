using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageEditorAddSleeperVolume : NetPackage
{
	public NetPackageEditorAddSleeperVolume Setup(Vector3i _hitPointBlockPos)
	{
		this.hitPointBlockPos = _hitPointBlockPos;
		return this;
	}

	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToServer;
		}
	}

	public override void read(PooledBinaryReader _br)
	{
		this.hitPointBlockPos = StreamUtils.ReadVector3i(_br);
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		StreamUtils.Write(_bw, this.hitPointBlockPos);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		if (!_world.IsRemote())
		{
			PrefabSleeperVolumeManager.Instance.AddSleeperVolumeServer(this.hitPointBlockPos);
		}
	}

	public override int GetLength()
	{
		return 16;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3i hitPointBlockPos;
}

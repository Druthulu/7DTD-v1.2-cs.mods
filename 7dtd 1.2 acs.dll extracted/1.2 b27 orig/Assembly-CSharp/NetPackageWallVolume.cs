using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageWallVolume : NetPackage
{
	public NetPackageWallVolume Setup(WallVolume _wallVolume)
	{
		this.wallVolume = _wallVolume;
		return this;
	}

	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToClient;
		}
	}

	public override void read(PooledBinaryReader _reader)
	{
		this.wallVolume = WallVolume.Read(_reader);
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		this.wallVolume.Write(_writer);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
		{
			_world.AddWallVolume(this.wallVolume);
		}
	}

	public override int GetLength()
	{
		return 29;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public WallVolume wallVolume;
}

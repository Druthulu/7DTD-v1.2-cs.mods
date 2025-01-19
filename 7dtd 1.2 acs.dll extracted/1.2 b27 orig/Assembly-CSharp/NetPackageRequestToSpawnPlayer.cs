using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageRequestToSpawnPlayer : NetPackage
{
	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToServer;
		}
	}

	public NetPackageRequestToSpawnPlayer Setup(int _chunkViewDim, PlayerProfile _playerProfile)
	{
		this.chunkViewDim = _chunkViewDim;
		this.playerProfile = _playerProfile;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		this.chunkViewDim = (int)_reader.ReadInt16();
		this.playerProfile = PlayerProfile.Read(_reader);
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write((short)this.chunkViewDim);
		this.playerProfile.Write(_writer);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		_callbacks.RequestToSpawnPlayer(base.Sender, this.chunkViewDim, this.playerProfile);
	}

	public override int GetLength()
	{
		return 50;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int chunkViewDim;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlayerProfile playerProfile;
}

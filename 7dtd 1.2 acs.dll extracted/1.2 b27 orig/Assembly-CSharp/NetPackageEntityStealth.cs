using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageEntityStealth : NetPackage
{
	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.Both;
		}
	}

	public NetPackageEntityStealth Setup(EntityPlayer player, bool _isCrouching)
	{
		this.id = player.entityId;
		this.data = (_isCrouching ? 1 : 0);
		return this;
	}

	public NetPackageEntityStealth Setup(EntityPlayer player, int _lightLevel, int _noiseVolume, bool _isAlert)
	{
		this.id = player.entityId;
		this.data = (ushort)((int)((byte)_lightLevel) | (_noiseVolume & 127) << 8);
		if (_isAlert)
		{
			this.data |= 32768;
		}
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		this.id = _br.ReadInt32();
		this.data = _br.ReadUInt16();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(this.id);
		_bw.Write(this.data);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		if (!base.ValidEntityIdForSender(this.id, false))
		{
			return;
		}
		EntityPlayer entityPlayer = _world.GetEntity(this.id) as EntityPlayer;
		if (entityPlayer == null)
		{
			Log.Out("Discarding " + base.GetType().Name);
			return;
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			entityPlayer.Crouching = ((this.data & 1) > 0);
			return;
		}
		float lightLevel = (float)((byte)this.data);
		float noiseVolume = (float)(this.data >> 8 & 127);
		entityPlayer.Stealth.SetClientLevels(lightLevel, noiseVolume, (this.data & 32768) > 0);
	}

	public override int GetLength()
	{
		return 20;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int id;

	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cFIsCrouching = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cFIsAlert = 32768;

	[PublicizedFrom(EAccessModifier.Private)]
	public ushort data;
}

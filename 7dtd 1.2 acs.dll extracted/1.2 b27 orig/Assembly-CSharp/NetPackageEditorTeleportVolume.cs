using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageEditorTeleportVolume : NetPackage
{
	public NetPackageEditorTeleportVolume Setup(NetPackageEditorSleeperVolume.EChangeType _changeType, int _prefabInstanceId, int _volumeId, Prefab.PrefabTeleportVolume _volume)
	{
		this.changeType = _changeType;
		this.prefabInstanceId = _prefabInstanceId;
		this.volumeId = _volumeId;
		this.startPos = _volume.startPos;
		this.size = _volume.size;
		return this;
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		Prefab.PrefabTeleportVolume volumeSettings = new Prefab.PrefabTeleportVolume
		{
			startPos = this.startPos,
			size = this.size
		};
		if (!_world.IsRemote())
		{
			switch (this.changeType)
			{
			case NetPackageEditorSleeperVolume.EChangeType.Changed:
				PrefabVolumeManager.Instance.UpdateTeleportPropertiesServer(this.prefabInstanceId, this.volumeId, volumeSettings, false);
				return;
			case NetPackageEditorSleeperVolume.EChangeType.Removed:
				PrefabVolumeManager.Instance.UpdateTeleportPropertiesServer(this.prefabInstanceId, this.volumeId, volumeSettings, true);
				return;
			}
			throw new ArgumentOutOfRangeException();
		}
		NetPackageEditorSleeperVolume.EChangeType echangeType = this.changeType;
		if (echangeType <= NetPackageEditorSleeperVolume.EChangeType.Changed)
		{
			PrefabVolumeManager.Instance.AddUpdateTeleportPropertiesClient(this.prefabInstanceId, this.volumeId, volumeSettings, false);
			return;
		}
		if (echangeType != NetPackageEditorSleeperVolume.EChangeType.Removed)
		{
			throw new ArgumentOutOfRangeException();
		}
		PrefabVolumeManager.Instance.AddUpdateTeleportPropertiesClient(this.prefabInstanceId, this.volumeId, volumeSettings, true);
	}

	public override void read(PooledBinaryReader _br)
	{
		this.changeType = (NetPackageEditorSleeperVolume.EChangeType)_br.ReadByte();
		this.prefabInstanceId = _br.ReadInt32();
		this.volumeId = _br.ReadInt32();
		this.startPos = StreamUtils.ReadVector3i(_br);
		this.size = StreamUtils.ReadVector3i(_br);
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write((byte)this.changeType);
		_bw.Write(this.prefabInstanceId);
		_bw.Write(this.volumeId);
		StreamUtils.Write(_bw, this.startPos);
		StreamUtils.Write(_bw, this.size);
	}

	public override int GetLength()
	{
		return 37;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public NetPackageEditorSleeperVolume.EChangeType changeType;

	[PublicizedFrom(EAccessModifier.Private)]
	public int prefabInstanceId;

	[PublicizedFrom(EAccessModifier.Private)]
	public int volumeId;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i startPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i size;
}

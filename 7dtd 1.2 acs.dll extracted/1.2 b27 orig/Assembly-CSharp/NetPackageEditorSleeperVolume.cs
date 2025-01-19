using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageEditorSleeperVolume : NetPackage
{
	public NetPackageEditorSleeperVolume Setup(NetPackageEditorSleeperVolume.EChangeType _changeType, int _prefabInstanceId, int _volumeId, Prefab.PrefabSleeperVolume _volume)
	{
		this.changeType = _changeType;
		this.prefabInstanceId = _prefabInstanceId;
		this.volumeId = _volumeId;
		this.used = _volume.used;
		this.startPos = _volume.startPos;
		this.size = _volume.size;
		this.groupName = _volume.groupName;
		this.isPriority = _volume.isPriority;
		this.isQuestExclude = _volume.isQuestExclude;
		this.spawnCountMin = _volume.spawnCountMin;
		this.spawnCountMax = _volume.spawnCountMax;
		this.groupId = _volume.groupId;
		this.flags = _volume.flags;
		this.minScript = _volume.minScript;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		this.changeType = (NetPackageEditorSleeperVolume.EChangeType)_br.ReadByte();
		this.prefabInstanceId = _br.ReadInt32();
		this.volumeId = _br.ReadInt32();
		this.used = _br.ReadBoolean();
		this.startPos = StreamUtils.ReadVector3i(_br);
		this.size = StreamUtils.ReadVector3i(_br);
		this.groupName = _br.ReadString();
		this.isPriority = _br.ReadBoolean();
		this.isQuestExclude = _br.ReadBoolean();
		this.spawnCountMin = _br.ReadInt16();
		this.spawnCountMax = _br.ReadInt16();
		this.groupId = _br.ReadInt16();
		this.flags = _br.ReadInt32();
		this.minScript = _br.ReadString();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write((byte)this.changeType);
		_bw.Write(this.prefabInstanceId);
		_bw.Write(this.volumeId);
		_bw.Write(this.used);
		StreamUtils.Write(_bw, this.startPos);
		StreamUtils.Write(_bw, this.size);
		_bw.Write(this.groupName);
		_bw.Write(this.isPriority);
		_bw.Write(this.isQuestExclude);
		_bw.Write(this.spawnCountMin);
		_bw.Write(this.spawnCountMax);
		_bw.Write(this.groupId);
		_bw.Write(this.flags);
		_bw.Write(this.minScript ?? "");
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		Prefab.PrefabSleeperVolume volumeSettings = new Prefab.PrefabSleeperVolume
		{
			used = this.used,
			startPos = this.startPos,
			size = this.size,
			groupName = this.groupName,
			isPriority = this.isPriority,
			isQuestExclude = this.isQuestExclude,
			spawnCountMin = this.spawnCountMin,
			spawnCountMax = this.spawnCountMax,
			groupId = this.groupId,
			flags = this.flags,
			minScript = this.minScript
		};
		if (!_world.IsRemote())
		{
			NetPackageEditorSleeperVolume.EChangeType echangeType = this.changeType;
			if (echangeType != NetPackageEditorSleeperVolume.EChangeType.Added && echangeType - NetPackageEditorSleeperVolume.EChangeType.Changed <= 1)
			{
				PrefabSleeperVolumeManager.Instance.UpdateSleeperPropertiesServer(this.prefabInstanceId, this.volumeId, volumeSettings);
				return;
			}
			throw new ArgumentOutOfRangeException();
		}
		else
		{
			NetPackageEditorSleeperVolume.EChangeType echangeType = this.changeType;
			if (echangeType <= NetPackageEditorSleeperVolume.EChangeType.Removed)
			{
				PrefabSleeperVolumeManager.Instance.AddUpdateSleeperPropertiesClient(this.prefabInstanceId, this.volumeId, volumeSettings);
				return;
			}
			throw new ArgumentOutOfRangeException();
		}
	}

	public override int GetLength()
	{
		return 38 + this.groupName.Length + 1 + 1 + 2 + 2;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public NetPackageEditorSleeperVolume.EChangeType changeType;

	[PublicizedFrom(EAccessModifier.Private)]
	public int prefabInstanceId;

	[PublicizedFrom(EAccessModifier.Private)]
	public int volumeId;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool used;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i startPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i size;

	[PublicizedFrom(EAccessModifier.Private)]
	public string groupName;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isPriority;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isQuestExclude;

	[PublicizedFrom(EAccessModifier.Private)]
	public short spawnCountMin;

	[PublicizedFrom(EAccessModifier.Private)]
	public short spawnCountMax;

	[PublicizedFrom(EAccessModifier.Private)]
	public short groupId;

	[PublicizedFrom(EAccessModifier.Private)]
	public int flags;

	[PublicizedFrom(EAccessModifier.Private)]
	public string minScript;

	public enum EChangeType
	{
		Added,
		Changed,
		Removed
	}
}

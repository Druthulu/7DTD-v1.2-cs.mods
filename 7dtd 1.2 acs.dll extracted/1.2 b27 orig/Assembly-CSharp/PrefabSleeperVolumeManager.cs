using System;
using System.Collections.Generic;

public class PrefabSleeperVolumeManager
{
	public static PrefabSleeperVolumeManager Instance
	{
		get
		{
			if (PrefabSleeperVolumeManager.instance == null)
			{
				PrefabSleeperVolumeManager.instance = new PrefabSleeperVolumeManager();
			}
			return PrefabSleeperVolumeManager.instance;
		}
	}

	public void Cleanup()
	{
		this.clientPrefabs.Clear();
		DynamicPrefabDecorator dynamicPrefabDecorator = GameManager.Instance.GetDynamicPrefabDecorator();
		if (dynamicPrefabDecorator != null)
		{
			dynamicPrefabDecorator.OnPrefabLoaded -= this.PrefabLoadedServer;
			dynamicPrefabDecorator.OnPrefabChanged -= this.PrefabChangedServer;
			dynamicPrefabDecorator.OnPrefabRemoved -= this.PrefabRemovedServer;
		}
		PrefabEditModeManager.Instance.OnPrefabChanged -= this.PrefabChangedServer;
	}

	public void StartAsServer()
	{
		DynamicPrefabDecorator dynamicPrefabDecorator = GameManager.Instance.GetDynamicPrefabDecorator();
		dynamicPrefabDecorator.OnPrefabLoaded += this.PrefabLoadedServer;
		dynamicPrefabDecorator.OnPrefabChanged += this.PrefabChangedServer;
		dynamicPrefabDecorator.OnPrefabRemoved += this.PrefabRemovedServer;
		PrefabEditModeManager.Instance.OnPrefabChanged += this.PrefabChangedServer;
		GameManager.Instance.OnClientSpawned += this.SendAllPrefabs;
	}

	public void StartAsClient()
	{
		this.clientPrefabs.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SendAllPrefabs(ClientInfo _toClient)
	{
		if (_toClient != null)
		{
			foreach (PrefabInstance prefabInstance in GameManager.Instance.GetDynamicPrefabDecorator().GetDynamicPrefabs())
			{
				_toClient.SendPackage(NetPackageManager.GetPackage<NetPackageEditorPrefabInstance>().Setup(NetPackageEditorPrefabInstance.EChangeType.Added, prefabInstance));
				for (int i = 0; i < prefabInstance.prefab.SleeperVolumes.Count; i++)
				{
					_toClient.SendPackage(NetPackageManager.GetPackage<NetPackageEditorSleeperVolume>().Setup(NetPackageEditorSleeperVolume.EChangeType.Added, prefabInstance.id, i, prefabInstance.prefab.SleeperVolumes[i]));
				}
				for (int j = 0; j < prefabInstance.prefab.TeleportVolumes.Count; j++)
				{
					_toClient.SendPackage(NetPackageManager.GetPackage<NetPackageEditorTeleportVolume>().Setup(NetPackageEditorSleeperVolume.EChangeType.Added, prefabInstance.id, j, prefabInstance.prefab.TeleportVolumes[j]));
				}
				for (int k = 0; k < prefabInstance.prefab.InfoVolumes.Count; k++)
				{
					_toClient.SendPackage(NetPackageManager.GetPackage<NetPackageEditorInfoVolume>().Setup(NetPackageEditorSleeperVolume.EChangeType.Added, prefabInstance.id, k, prefabInstance.prefab.InfoVolumes[k]));
				}
				for (int l = 0; l < prefabInstance.prefab.WallVolumes.Count; l++)
				{
					_toClient.SendPackage(NetPackageManager.GetPackage<NetPackageEditorWallVolume>().Setup(NetPackageEditorSleeperVolume.EChangeType.Added, prefabInstance.id, l, prefabInstance.prefab.WallVolumes[l]));
				}
				for (int m = 0; m < prefabInstance.prefab.TriggerVolumes.Count; m++)
				{
					_toClient.SendPackage(NetPackageManager.GetPackage<NetPackageEditorTriggerVolume>().Setup(NetPackageEditorSleeperVolume.EChangeType.Added, prefabInstance.id, m, prefabInstance.prefab.TriggerVolumes[m]));
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PrefabLoadedServer(PrefabInstance _prefabInstance)
	{
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEditorPrefabInstance>().Setup(NetPackageEditorPrefabInstance.EChangeType.Added, _prefabInstance), false, -1, -1, -1, null, 192);
	}

	public void PrefabLoadedClient(int _prefabInstanceId, Vector3i _boundingBoxPosition, Vector3i _boundingBoxSize, string _prefabInstanceName, Vector3i _prefabSize, string _prefabFilename, int _prefabLocalRotation, int _yOffset)
	{
		PathAbstractions.AbstractedLocation location = PathAbstractions.PrefabsSearchPaths.GetLocation(_prefabFilename, null, null);
		PrefabInstance prefabInstance = new PrefabInstance(_prefabInstanceId, location, _boundingBoxPosition, 0, null, 0)
		{
			boundingBoxSize = _boundingBoxSize,
			name = _prefabInstanceName,
			prefab = new Prefab
			{
				size = _prefabSize,
				location = location,
				yOffset = _yOffset
			}
		};
		prefabInstance.prefab.SetLocalRotation(_prefabLocalRotation);
		prefabInstance.CreateBoundingBox(false);
		this.clientPrefabs.Add(prefabInstance);
		if (this.clientPrefabs.Count == 1)
		{
			PrefabEditModeManager.Instance.SetGroundLevel(_yOffset);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PrefabChangedServer(PrefabInstance _prefabInstance)
	{
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEditorPrefabInstance>().Setup(NetPackageEditorPrefabInstance.EChangeType.Changed, _prefabInstance), false, -1, -1, -1, null, 192);
	}

	public void PrefabChangedClient(int _prefabInstanceId, Vector3i _boundingBoxPosition, Vector3i _boundingBoxSize, string _prefabInstanceName, Vector3i _prefabSize, string _prefabFilename, int _prefabLocalRotation, int _yOffset)
	{
		PrefabInstance prefabInstance = this.GetPrefabInstance(_prefabInstanceId);
		if (prefabInstance == null)
		{
			Log.Error("Prefab not found: " + _prefabInstanceId.ToString());
			return;
		}
		PathAbstractions.AbstractedLocation location = PathAbstractions.PrefabsSearchPaths.GetLocation(_prefabFilename, null, null);
		prefabInstance.boundingBoxPosition = _boundingBoxPosition;
		prefabInstance.boundingBoxSize = _boundingBoxSize;
		prefabInstance.name = _prefabInstanceName;
		prefabInstance.prefab.size = _prefabSize;
		prefabInstance.prefab.location = location;
		prefabInstance.prefab.SetLocalRotation(_prefabLocalRotation);
		prefabInstance.prefab.yOffset = _yOffset;
		prefabInstance.CreateBoundingBox(false);
		if (this.clientPrefabs.IndexOf(prefabInstance) == 0)
		{
			PrefabEditModeManager.Instance.SetGroundLevel(_yOffset);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PrefabRemovedServer(PrefabInstance _prefabInstance)
	{
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEditorPrefabInstance>().Setup(NetPackageEditorPrefabInstance.EChangeType.Removed, _prefabInstance), false, -1, -1, -1, null, 192);
	}

	public void PrefabRemovedClient(int _prefabInstanceId)
	{
		for (int i = 0; i < this.clientPrefabs.Count; i++)
		{
			PrefabInstance prefabInstance = this.clientPrefabs[i];
			if (prefabInstance.id == _prefabInstanceId)
			{
				this.clientPrefabs.RemoveAt(i);
				for (int j = 0; j < prefabInstance.prefab.SleeperVolumes.Count; j++)
				{
					Prefab.PrefabSleeperVolume prefabSleeperVolume = prefabInstance.prefab.SleeperVolumes[j];
					prefabSleeperVolume.used = false;
					prefabInstance.prefab.SetSleeperVolume(prefabInstance.name, prefabInstance.boundingBoxPosition, j, prefabSleeperVolume);
				}
				for (int k = 0; k < prefabInstance.prefab.TeleportVolumes.Count; k++)
				{
					Prefab.PrefabTeleportVolume volumeSettings = prefabInstance.prefab.TeleportVolumes[k];
					prefabInstance.prefab.SetTeleportVolume(prefabInstance.name, prefabInstance.boundingBoxPosition, k, volumeSettings, false);
				}
				for (int l = 0; l < prefabInstance.prefab.InfoVolumes.Count; l++)
				{
					Prefab.PrefabInfoVolume volumeSettings2 = prefabInstance.prefab.InfoVolumes[l];
					prefabInstance.prefab.SetInfoVolume(prefabInstance.name, prefabInstance.boundingBoxPosition, l, volumeSettings2, false);
				}
				for (int m = 0; m < prefabInstance.prefab.WallVolumes.Count; m++)
				{
					Prefab.PrefabWallVolume volumeSettings3 = prefabInstance.prefab.WallVolumes[m];
					prefabInstance.prefab.SetWallVolume(prefabInstance.name, prefabInstance.boundingBoxPosition, m, volumeSettings3, false);
				}
				for (int n = 0; n < prefabInstance.prefab.TriggerVolumes.Count; n++)
				{
					Prefab.PrefabTriggerVolume volumeSettings4 = prefabInstance.prefab.TriggerVolumes[n];
					prefabInstance.prefab.SetTriggerVolume(prefabInstance.name, prefabInstance.boundingBoxPosition, n, volumeSettings4, false);
				}
				return;
			}
		}
	}

	public void AddSleeperVolumeServer(Vector3i _hitPointBlockPos)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			DynamicPrefabDecorator dynamicPrefabDecorator = GameManager.Instance.World.ChunkClusters[0].ChunkProvider.GetDynamicPrefabDecorator();
			if (dynamicPrefabDecorator == null)
			{
				return;
			}
			PrefabInstance prefabInstance = GameUtils.FindPrefabForBlockPos(dynamicPrefabDecorator.GetDynamicPrefabs(), _hitPointBlockPos);
			if (prefabInstance != null)
			{
				Vector3i vector3i = new Vector3i(15, 3, 15);
				int num = prefabInstance.prefab.AddSleeperVolume(prefabInstance.name, prefabInstance.boundingBoxPosition, _hitPointBlockPos - prefabInstance.boundingBoxPosition - new Vector3i(vector3i.x / 2, 0, vector3i.z / 2), vector3i, 0, "GroupGenericZombie", 5, 6);
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEditorSleeperVolume>().Setup(NetPackageEditorSleeperVolume.EChangeType.Added, prefabInstance.id, num, prefabInstance.prefab.SleeperVolumes[num]), false, -1, -1, -1, null, 192);
				return;
			}
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEditorAddSleeperVolume>().Setup(_hitPointBlockPos), false);
		}
	}

	public void UpdateSleeperPropertiesServer(int _prefabInstanceId, int _volumeId, Prefab.PrefabSleeperVolume _volumeSettings)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			this.AddUpdateSleeperPropertiesClient(_prefabInstanceId, _volumeId, _volumeSettings);
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEditorSleeperVolume>().Setup(NetPackageEditorSleeperVolume.EChangeType.Changed, _prefabInstanceId, _volumeId, _volumeSettings), false, -1, -1, -1, null, 192);
			return;
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEditorSleeperVolume>().Setup(NetPackageEditorSleeperVolume.EChangeType.Changed, _prefabInstanceId, _volumeId, _volumeSettings), false);
	}

	public void AddUpdateSleeperPropertiesClient(int _prefabInstanceId, int _volumeId, Prefab.PrefabSleeperVolume _volumeSettings)
	{
		PrefabInstance prefabInstance = this.GetPrefabInstance(_prefabInstanceId);
		if (prefabInstance == null)
		{
			Log.Error("Prefab not found: " + _prefabInstanceId.ToString());
			return;
		}
		prefabInstance.prefab.SetSleeperVolume(prefabInstance.name, prefabInstance.boundingBoxPosition, _volumeId, _volumeSettings);
		XUiC_WoPropsSleeperVolume.SleeperVolumeChanged(_prefabInstanceId, _volumeId);
	}

	public PrefabInstance GetPrefabInstance(int _prefabId)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			foreach (PrefabInstance prefabInstance in this.clientPrefabs)
			{
				if (prefabInstance.id == _prefabId)
				{
					return prefabInstance;
				}
			}
			return null;
		}
		if (GameManager.Instance.GetDynamicPrefabDecorator() == null)
		{
			return null;
		}
		return GameManager.Instance.GetDynamicPrefabDecorator().GetPrefab(_prefabId);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static PrefabSleeperVolumeManager instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<PrefabInstance> clientPrefabs = new List<PrefabInstance>();
}

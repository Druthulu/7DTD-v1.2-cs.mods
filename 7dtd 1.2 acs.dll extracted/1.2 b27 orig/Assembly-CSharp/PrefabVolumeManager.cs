using System;

public class PrefabVolumeManager
{
	public static PrefabVolumeManager Instance
	{
		get
		{
			if (PrefabVolumeManager.instance == null)
			{
				PrefabVolumeManager.instance = new PrefabVolumeManager();
			}
			return PrefabVolumeManager.instance;
		}
	}

	public void Cleanup()
	{
		GUIWindowDynamicPrefabMenu.Cleanup();
	}

	public void AddTeleportVolumeServer(Vector3i _hitPointBlockPos)
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
				if (!prefabInstance.prefab.bTraderArea)
				{
					(((XUiWindowGroup)LocalPlayerUI.primaryUI.windowManager.GetWindow(XUiC_MessageBoxWindowGroup.ID)).Controller as XUiC_MessageBoxWindowGroup).ShowMessage(Localization.Get("failed", false), Localization.Get("xuiPrefabEditorTraderTeleportError", false), XUiC_MessageBoxWindowGroup.MessageBoxTypes.Ok, null, null, false, true, true);
					return;
				}
				Vector3i vector3i = new Vector3i(15, 3, 15);
				int num = prefabInstance.prefab.AddTeleportVolume(prefabInstance.name, prefabInstance.boundingBoxPosition, _hitPointBlockPos - prefabInstance.boundingBoxPosition - new Vector3i(vector3i.x / 2, 0, vector3i.z / 2), vector3i);
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEditorTeleportVolume>().Setup(NetPackageEditorSleeperVolume.EChangeType.Added, prefabInstance.id, num, prefabInstance.prefab.TeleportVolumes[num]), false, -1, -1, -1, null, 192);
				return;
			}
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEditorAddTeleportVolume>().Setup(_hitPointBlockPos), false);
		}
	}

	public void UpdateTeleportPropertiesServer(int _prefabInstanceId, int _volumeId, Prefab.PrefabTeleportVolume _volumeSettings, bool remove = false)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			this.AddUpdateTeleportPropertiesClient(_prefabInstanceId, _volumeId, _volumeSettings, remove);
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEditorTeleportVolume>().Setup(remove ? NetPackageEditorSleeperVolume.EChangeType.Removed : NetPackageEditorSleeperVolume.EChangeType.Changed, _prefabInstanceId, _volumeId, _volumeSettings), false, -1, -1, -1, null, 192);
			return;
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEditorTeleportVolume>().Setup(remove ? NetPackageEditorSleeperVolume.EChangeType.Removed : NetPackageEditorSleeperVolume.EChangeType.Changed, _prefabInstanceId, _volumeId, _volumeSettings), false);
	}

	public void AddUpdateTeleportPropertiesClient(int _prefabInstanceId, int _volumeId, Prefab.PrefabTeleportVolume _volumeSettings, bool remove = false)
	{
		PrefabInstance prefabInstance = PrefabSleeperVolumeManager.Instance.GetPrefabInstance(_prefabInstanceId);
		if (prefabInstance == null)
		{
			Log.Error("Prefab not found: " + _prefabInstanceId.ToString());
			return;
		}
		prefabInstance.prefab.SetTeleportVolume(prefabInstance.name, prefabInstance.boundingBoxPosition, _volumeId, _volumeSettings, remove);
	}

	public void AddInfoVolumeServer(Vector3i _hitPointBlockPos)
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
				int num = prefabInstance.prefab.AddInfoVolume(prefabInstance.name, prefabInstance.boundingBoxPosition, _hitPointBlockPos - prefabInstance.boundingBoxPosition - new Vector3i(vector3i.x / 2, 0, vector3i.z / 2), vector3i);
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEditorInfoVolume>().Setup(NetPackageEditorSleeperVolume.EChangeType.Added, prefabInstance.id, num, prefabInstance.prefab.InfoVolumes[num]), false, -1, -1, -1, null, 192);
				return;
			}
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEditorAddInfoVolume>().Setup(_hitPointBlockPos), false);
		}
	}

	public void UpdateInfoPropertiesServer(int _prefabInstanceId, int _volumeId, Prefab.PrefabInfoVolume _volumeSettings, bool remove = false)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			this.AddUpdateInfoPropertiesClient(_prefabInstanceId, _volumeId, _volumeSettings, remove);
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEditorInfoVolume>().Setup(remove ? NetPackageEditorSleeperVolume.EChangeType.Removed : NetPackageEditorSleeperVolume.EChangeType.Changed, _prefabInstanceId, _volumeId, _volumeSettings), false, -1, -1, -1, null, 192);
			return;
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEditorInfoVolume>().Setup(remove ? NetPackageEditorSleeperVolume.EChangeType.Removed : NetPackageEditorSleeperVolume.EChangeType.Changed, _prefabInstanceId, _volumeId, _volumeSettings), false);
	}

	public void AddUpdateInfoPropertiesClient(int _prefabInstanceId, int _volumeId, Prefab.PrefabInfoVolume _volumeSettings, bool remove = false)
	{
		PrefabInstance prefabInstance = PrefabSleeperVolumeManager.Instance.GetPrefabInstance(_prefabInstanceId);
		if (prefabInstance == null)
		{
			Log.Error("Prefab not found: " + _prefabInstanceId.ToString());
			return;
		}
		prefabInstance.prefab.SetInfoVolume(prefabInstance.name, prefabInstance.boundingBoxPosition, _volumeId, _volumeSettings, remove);
	}

	public void AddWallVolumeServer(Vector3i _hitPointBlockPos)
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
				int num = prefabInstance.prefab.AddWallVolume(prefabInstance.name, prefabInstance.boundingBoxPosition, _hitPointBlockPos - prefabInstance.boundingBoxPosition - new Vector3i(vector3i.x / 2, 0, vector3i.z / 2), vector3i);
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEditorWallVolume>().Setup(NetPackageEditorSleeperVolume.EChangeType.Added, prefabInstance.id, num, prefabInstance.prefab.WallVolumes[num]), false, -1, -1, -1, null, 192);
				return;
			}
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEditorAddWallVolume>().Setup(_hitPointBlockPos), false);
		}
	}

	public void UpdateWallPropertiesServer(int _prefabInstanceId, int _volumeId, Prefab.PrefabWallVolume _volumeSettings, bool remove = false)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			this.AddUpdateWallPropertiesClient(_prefabInstanceId, _volumeId, _volumeSettings, remove);
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEditorWallVolume>().Setup(remove ? NetPackageEditorSleeperVolume.EChangeType.Removed : NetPackageEditorSleeperVolume.EChangeType.Changed, _prefabInstanceId, _volumeId, _volumeSettings), false, -1, -1, -1, null, 192);
			return;
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEditorWallVolume>().Setup(remove ? NetPackageEditorSleeperVolume.EChangeType.Removed : NetPackageEditorSleeperVolume.EChangeType.Changed, _prefabInstanceId, _volumeId, _volumeSettings), false);
	}

	public void AddUpdateWallPropertiesClient(int _prefabInstanceId, int _volumeId, Prefab.PrefabWallVolume _volumeSettings, bool remove = false)
	{
		PrefabInstance prefabInstance = PrefabSleeperVolumeManager.Instance.GetPrefabInstance(_prefabInstanceId);
		if (prefabInstance == null)
		{
			Log.Error("Prefab not found: " + _prefabInstanceId.ToString());
			return;
		}
		prefabInstance.prefab.SetWallVolume(prefabInstance.name, prefabInstance.boundingBoxPosition, _volumeId, _volumeSettings, remove);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static PrefabVolumeManager instance;
}

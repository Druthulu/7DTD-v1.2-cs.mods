using System;

public class PrefabTriggerVolumeManager
{
	public static PrefabTriggerVolumeManager Instance
	{
		get
		{
			if (PrefabTriggerVolumeManager.instance == null)
			{
				PrefabTriggerVolumeManager.instance = new PrefabTriggerVolumeManager();
			}
			return PrefabTriggerVolumeManager.instance;
		}
	}

	public void Cleanup()
	{
		GUIWindowDynamicPrefabMenu.Cleanup();
	}

	public void AddTriggerVolumeServer(Vector3i _hitPointBlockPos)
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
				int num = prefabInstance.prefab.AddTriggerVolume(prefabInstance.name, prefabInstance.boundingBoxPosition, _hitPointBlockPos - prefabInstance.boundingBoxPosition - new Vector3i(vector3i.x / 2, 0, vector3i.z / 2), vector3i);
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEditorTriggerVolume>().Setup(NetPackageEditorSleeperVolume.EChangeType.Added, prefabInstance.id, num, prefabInstance.prefab.TriggerVolumes[num]), false, -1, -1, -1, null, 192);
				return;
			}
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEditorAddTriggerVolume>().Setup(_hitPointBlockPos), false);
		}
	}

	public void UpdateTriggerPropertiesServer(int _prefabInstanceId, int _volumeId, Prefab.PrefabTriggerVolume _volumeSettings, bool remove = false)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			this.AddUpdateTriggerPropertiesClient(_prefabInstanceId, _volumeId, _volumeSettings, remove);
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEditorTriggerVolume>().Setup(remove ? NetPackageEditorSleeperVolume.EChangeType.Removed : NetPackageEditorSleeperVolume.EChangeType.Changed, _prefabInstanceId, _volumeId, _volumeSettings), false, -1, -1, -1, null, 192);
			return;
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEditorTriggerVolume>().Setup(remove ? NetPackageEditorSleeperVolume.EChangeType.Removed : NetPackageEditorSleeperVolume.EChangeType.Changed, _prefabInstanceId, _volumeId, _volumeSettings), false);
	}

	public void AddUpdateTriggerPropertiesClient(int _prefabInstanceId, int _volumeId, Prefab.PrefabTriggerVolume _volumeSettings, bool remove = false)
	{
		PrefabInstance prefabInstance = PrefabSleeperVolumeManager.Instance.GetPrefabInstance(_prefabInstanceId);
		if (prefabInstance == null)
		{
			Log.Error("Prefab not found: " + _prefabInstanceId.ToString());
			return;
		}
		prefabInstance.prefab.SetTriggerVolume(prefabInstance.name, prefabInstance.boundingBoxPosition, _volumeId, _volumeSettings, remove);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static PrefabTriggerVolumeManager instance;
}

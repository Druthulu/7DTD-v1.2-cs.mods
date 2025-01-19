using System;
using System.Collections.Generic;
using UnityEngine;

public class GUIWindowDynamicPrefabMenu : ISelectionBoxCallback
{
	public GUIWindowDynamicPrefabMenu(GameManager _gm)
	{
		this.gameManager = _gm;
		SelectionBoxManager.Instance.GetCategory("TraderTeleport").SetCallback(this);
		SelectionBoxManager.Instance.GetCategory("InfoVolume").SetCallback(this);
		SelectionBoxManager.Instance.GetCategory("WallVolume").SetCallback(this);
		GUIWindowDynamicPrefabMenu.instance = this;
	}

	public static void Cleanup()
	{
		if (SelectionBoxManager.Instance != null && GUIWindowDynamicPrefabMenu.instance != null)
		{
			SelectionBoxManager.Instance.GetCategory("TraderTeleport").SetCallback(null);
		}
		if (SelectionBoxManager.Instance != null && GUIWindowDynamicPrefabMenu.instance != null)
		{
			SelectionBoxManager.Instance.GetCategory("InfoVolume").SetCallback(null);
		}
		if (SelectionBoxManager.Instance != null && GUIWindowDynamicPrefabMenu.instance != null)
		{
			SelectionBoxManager.Instance.GetCategory("WallVolume").SetCallback(null);
		}
		GUIWindowDynamicPrefabMenu.instance = null;
	}

	public bool OnSelectionBoxActivated(string _category, string _name, bool _bActivated)
	{
		if (_bActivated)
		{
			int num;
			int num2;
			if (this.getPrefabIdAndVolumeId(_name, out num, out num2))
			{
				this.selIdx = num2;
			}
		}
		else
		{
			this.selectedPrefab = null;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool getPrefabIdAndVolumeId(string _name, out int _prefabInstanceId, out int _volumeId)
	{
		_prefabInstanceId = (_volumeId = 0);
		string[] array = _name.Split('.', StringSplitOptions.None);
		if (array.Length > 1)
		{
			string[] array2 = array[1].Split('_', StringSplitOptions.None);
			if (array2.Length > 1 && int.TryParse(array2[1], out _volumeId) && int.TryParse(array2[0], out _prefabInstanceId))
			{
				this.selectedPrefab = PrefabSleeperVolumeManager.Instance.GetPrefabInstance(_prefabInstanceId);
				return true;
			}
		}
		return false;
	}

	public void OnSelectionBoxMoved(string _category, string _name, Vector3 _moveVector)
	{
		if (this.selectedPrefab == null)
		{
			return;
		}
		if (_category == "TraderTeleport")
		{
			Prefab.PrefabTeleportVolume prefabTeleportVolume = new Prefab.PrefabTeleportVolume(this.selectedPrefab.prefab.TeleportVolumes[this.selIdx]);
			prefabTeleportVolume.startPos += new Vector3i(_moveVector);
			PrefabVolumeManager.Instance.UpdateTeleportPropertiesServer(this.selectedPrefab.id, this.selIdx, prefabTeleportVolume, false);
			return;
		}
		if (_category == "InfoVolume")
		{
			Prefab.PrefabInfoVolume prefabInfoVolume = new Prefab.PrefabInfoVolume(this.selectedPrefab.prefab.InfoVolumes[this.selIdx]);
			prefabInfoVolume.startPos += new Vector3i(_moveVector);
			PrefabVolumeManager.Instance.UpdateInfoPropertiesServer(this.selectedPrefab.id, this.selIdx, prefabInfoVolume, false);
			return;
		}
		if (_category == "WallVolume")
		{
			Prefab.PrefabWallVolume prefabWallVolume = new Prefab.PrefabWallVolume(this.selectedPrefab.prefab.WallVolumes[this.selIdx]);
			prefabWallVolume.startPos += new Vector3i(_moveVector);
			PrefabVolumeManager.Instance.UpdateWallPropertiesServer(this.selectedPrefab.id, this.selIdx, prefabWallVolume, false);
		}
	}

	public void OnSelectionBoxSized(string _category, string _name, int _dTop, int _dBottom, int _dNorth, int _dSouth, int _dEast, int _dWest)
	{
		if (this.selectedPrefab == null)
		{
			return;
		}
		if (_category == "TraderTeleport")
		{
			Prefab.PrefabTeleportVolume prefabTeleportVolume = new Prefab.PrefabTeleportVolume(this.selectedPrefab.prefab.TeleportVolumes[this.selIdx]);
			prefabTeleportVolume.size += new Vector3i(_dEast + _dWest, _dTop + _dBottom, _dNorth + _dSouth);
			prefabTeleportVolume.startPos += new Vector3i(-_dWest, -_dBottom, -_dSouth);
			Vector3i size = prefabTeleportVolume.size;
			if (size.x < 2)
			{
				size = new Vector3i(1, size.y, size.z);
			}
			if (size.y < 2)
			{
				size = new Vector3i(size.x, 1, size.z);
			}
			if (size.z < 2)
			{
				size = new Vector3i(size.x, size.y, 1);
			}
			prefabTeleportVolume.size = size;
			PrefabVolumeManager.Instance.UpdateTeleportPropertiesServer(this.selectedPrefab.id, this.selIdx, prefabTeleportVolume, false);
			return;
		}
		if (_category == "InfoVolume")
		{
			Prefab.PrefabInfoVolume prefabInfoVolume = new Prefab.PrefabInfoVolume(this.selectedPrefab.prefab.InfoVolumes[this.selIdx]);
			prefabInfoVolume.size += new Vector3i(_dEast + _dWest, _dTop + _dBottom, _dNorth + _dSouth);
			prefabInfoVolume.startPos += new Vector3i(-_dWest, -_dBottom, -_dSouth);
			Vector3i size2 = prefabInfoVolume.size;
			if (size2.x < 2)
			{
				size2 = new Vector3i(1, size2.y, size2.z);
			}
			if (size2.y < 2)
			{
				size2 = new Vector3i(size2.x, 1, size2.z);
			}
			if (size2.z < 2)
			{
				size2 = new Vector3i(size2.x, size2.y, 1);
			}
			prefabInfoVolume.size = size2;
			PrefabVolumeManager.Instance.UpdateInfoPropertiesServer(this.selectedPrefab.id, this.selIdx, prefabInfoVolume, false);
			return;
		}
		if (_category == "WallVolume")
		{
			Prefab.PrefabWallVolume prefabWallVolume = new Prefab.PrefabWallVolume(this.selectedPrefab.prefab.WallVolumes[this.selIdx]);
			prefabWallVolume.size += new Vector3i(_dEast + _dWest, _dTop + _dBottom, _dNorth + _dSouth);
			prefabWallVolume.startPos += new Vector3i(-_dWest, -_dBottom, -_dSouth);
			Vector3i size3 = prefabWallVolume.size;
			if (size3.x < 2)
			{
				size3 = new Vector3i(1, size3.y, size3.z);
			}
			if (size3.y < 2)
			{
				size3 = new Vector3i(size3.x, 1, size3.z);
			}
			if (size3.z < 2)
			{
				size3 = new Vector3i(size3.x, size3.y, 1);
			}
			prefabWallVolume.size = size3;
			PrefabVolumeManager.Instance.UpdateWallPropertiesServer(this.selectedPrefab.id, this.selIdx, prefabWallVolume, false);
		}
	}

	public void OnSelectionBoxMirrored(Vector3i _axis)
	{
	}

	public bool OnSelectionBoxDelete(string _category, string _name)
	{
		using (IEnumerator<LocalPlayerUI> enumerator = LocalPlayerUI.PlayerUIs.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.windowManager.IsModalWindowOpen())
				{
					SelectionBoxManager.Instance.SetActive(_category, _name, true);
					return false;
				}
			}
		}
		int num;
		int num2;
		if (this.getPrefabIdAndVolumeId(_name, out num, out num2))
		{
			if (_category == "TraderTeleport")
			{
				Prefab.PrefabTeleportVolume volumeSettings = new Prefab.PrefabTeleportVolume(this.selectedPrefab.prefab.TeleportVolumes[num2]);
				PrefabVolumeManager.Instance.UpdateTeleportPropertiesServer(this.selectedPrefab.id, num2, volumeSettings, true);
			}
			else if (_category == "InfoVolume")
			{
				Prefab.PrefabInfoVolume volumeSettings2 = new Prefab.PrefabInfoVolume(this.selectedPrefab.prefab.InfoVolumes[num2]);
				PrefabVolumeManager.Instance.UpdateInfoPropertiesServer(this.selectedPrefab.id, num2, volumeSettings2, true);
			}
			else if (_category == "WallVolume")
			{
				Prefab.PrefabWallVolume volumeSettings3 = new Prefab.PrefabWallVolume(this.selectedPrefab.prefab.WallVolumes[num2]);
				PrefabVolumeManager.Instance.UpdateWallPropertiesServer(this.selectedPrefab.id, num2, volumeSettings3, true);
			}
			return true;
		}
		return false;
	}

	public bool OnSelectionBoxIsAvailable(string _category, EnumSelectionBoxAvailabilities _criteria)
	{
		return _criteria == EnumSelectionBoxAvailabilities.CanResize;
	}

	public void OnSelectionBoxShowProperties(bool _bVisible, GUIWindowManager _windowManager)
	{
	}

	public void OnSelectionBoxRotated(string _category, string _name)
	{
	}

	public static PrefabInstance selectedPrefabInstance
	{
		get
		{
			if (GUIWindowDynamicPrefabMenu.instance != null)
			{
				return GUIWindowDynamicPrefabMenu.instance.selectedPrefab;
			}
			return null;
		}
	}

	public static int selectedVolumeIndex
	{
		get
		{
			if (GUIWindowDynamicPrefabMenu.instance != null && GUIWindowDynamicPrefabMenu.instance.selectedPrefab != null)
			{
				return GUIWindowDynamicPrefabMenu.instance.selIdx;
			}
			return -1;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly GameManager gameManager;

	[PublicizedFrom(EAccessModifier.Private)]
	public static GUIWindowDynamicPrefabMenu instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public int selIdx;

	[PublicizedFrom(EAccessModifier.Private)]
	public PrefabInstance selectedPrefab;
}

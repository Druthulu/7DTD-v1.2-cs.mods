using System;
using System.Collections.Generic;
using UnityEngine;

public class SleeperVolumeToolManager
{
	public static void RegisterSleeperBlock(BlockValue _bv, Transform prefabTrans, Vector3i position)
	{
		BlockSleeper blockSleeper = _bv.Block as BlockSleeper;
		if (blockSleeper == null)
		{
			Log.Warning("SleeperVolumeToolManager RegisterSleeperBlock not sleeper {0}", new object[]
			{
				_bv
			});
			return;
		}
		if (SleeperVolumeToolManager.sleepers == null)
		{
			SleeperVolumeToolManager.sleepers = new Dictionary<Vector3i, List<SleeperVolumeToolManager.BlockData>>();
			Shader shader = Shader.Find("Game/UI/Sleeper");
			for (int i = 0; i < SleeperVolumeToolManager.typeColors.Length; i++)
			{
				Material material = new Material(shader);
				material.renderQueue = 4001;
				material.color = SleeperVolumeToolManager.typeColors[i];
				SleeperVolumeToolManager.typeMats.Add(material);
			}
		}
		Vector3i worldPos = GameManager.Instance.World.ChunkClusters[0].GetChunkFromWorldPos(position).GetWorldPos();
		SleeperVolumeToolManager.BlockData blockData = new SleeperVolumeToolManager.BlockData();
		blockData.block = blockSleeper;
		blockData.prefabT = prefabTrans;
		blockData.position = position;
		prefabTrans.position = prefabTrans.position + position.ToVector3() + Vector3.one * 0.5f + Vector3.up * 0.01f;
		if (SleeperVolumeToolManager.GroupGameObject == null)
		{
			SleeperVolumeToolManager.GroupGameObject = new GameObject();
			SleeperVolumeToolManager.GroupGameObject.name = "SleeperVolumeToolManagerPrefabs";
		}
		prefabTrans.parent = SleeperVolumeToolManager.GroupGameObject.transform;
		List<SleeperVolumeToolManager.BlockData> list;
		if (!SleeperVolumeToolManager.sleepers.TryGetValue(worldPos, out list))
		{
			list = new List<SleeperVolumeToolManager.BlockData>();
			SleeperVolumeToolManager.sleepers.Add(worldPos, list);
		}
		list.Add(blockData);
		SleeperVolumeToolManager.UpdateSleeperVisuals(blockData);
	}

	public static void UnRegisterSleeperBlock(Vector3i position)
	{
		if (SleeperVolumeToolManager.sleepers == null)
		{
			return;
		}
		Vector3i worldPos = GameManager.Instance.World.ChunkClusters[0].GetChunkFromWorldPos(position).GetWorldPos();
		List<SleeperVolumeToolManager.BlockData> list;
		if (SleeperVolumeToolManager.sleepers.TryGetValue(worldPos, out list))
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].position == position)
				{
					UnityEngine.Object.Destroy(list[i].prefabT.gameObject);
					list.RemoveAt(i);
					return;
				}
			}
		}
	}

	public static void CleanUp()
	{
		if (SleeperVolumeToolManager.sleepers != null)
		{
			foreach (KeyValuePair<Vector3i, List<SleeperVolumeToolManager.BlockData>> keyValuePair in SleeperVolumeToolManager.sleepers)
			{
				for (int i = 0; i < keyValuePair.Value.Count; i++)
				{
					Transform prefabT = keyValuePair.Value[i].prefabT;
					if (prefabT)
					{
						UnityEngine.Object.Destroy(prefabT.gameObject);
					}
				}
			}
			SleeperVolumeToolManager.sleepers.Clear();
		}
		SleeperVolumeToolManager.ClearSleeperVolumes();
	}

	public static void RegisterSleeperVolume(SelectionBox _selBox)
	{
		if (SleeperVolumeToolManager.registeredSleeperVolumes.Contains(_selBox))
		{
			return;
		}
		SleeperVolumeToolManager.registeredSleeperVolumes.Add(_selBox);
	}

	public static void UnRegisterSleeperVolume(SelectionBox _selBox)
	{
		if (!SleeperVolumeToolManager.registeredSleeperVolumes.Contains(_selBox))
		{
			return;
		}
		SleeperVolumeToolManager.registeredSleeperVolumes.Remove(_selBox);
	}

	public static void ClearSleeperVolumes()
	{
		SleeperVolumeToolManager.registeredSleeperVolumes.Clear();
	}

	public static void CheckKeys()
	{
		if (Input.GetKeyDown(KeyCode.RightBracket) && SleeperVolumeToolManager.currentSelectionBox)
		{
			Prefab.PrefabSleeperVolume prefabSleeperVolume = (Prefab.PrefabSleeperVolume)SleeperVolumeToolManager.currentSelectionBox.UserData;
			if (prefabSleeperVolume != null)
			{
				short num = -1;
				if (InputUtils.ShiftKeyPressed)
				{
					num = 0;
				}
				else if (SleeperVolumeToolManager.previousSelectionBox)
				{
					Prefab.PrefabSleeperVolume prefabSleeperVolume2 = (Prefab.PrefabSleeperVolume)SleeperVolumeToolManager.previousSelectionBox.UserData;
					if (prefabSleeperVolume2 != null)
					{
						num = prefabSleeperVolume2.groupId;
						if (num == 0)
						{
							PrefabInstance selectedPrefabInstance = XUiC_WoPropsSleeperVolume.selectedPrefabInstance;
							if (selectedPrefabInstance != null)
							{
								num = selectedPrefabInstance.prefab.FindSleeperVolumeFreeGroupId();
								prefabSleeperVolume2.groupId = num;
								Log.Out("Set sleeper volume {0} to new group ID {1}", new object[]
								{
									prefabSleeperVolume2.startPos,
									num
								});
							}
						}
					}
				}
				if (num >= 0)
				{
					prefabSleeperVolume.groupId = num;
					SleeperVolumeToolManager.SelectionChanged(SleeperVolumeToolManager.currentSelectionBox);
					Log.Out("Set sleeper volume {0} to group ID {1}", new object[]
					{
						prefabSleeperVolume.startPos,
						num
					});
				}
			}
		}
	}

	public static void SetVisible(bool _visible)
	{
		if (_visible)
		{
			SleeperVolumeToolManager.SelectionChanged(null);
			return;
		}
		SleeperVolumeToolManager.ShowSleepers(false);
	}

	public static void SelectionChanged(SelectionBox selBox)
	{
		if (selBox && selBox != SleeperVolumeToolManager.currentSelectionBox)
		{
			SleeperVolumeToolManager.previousSelectionBox = SleeperVolumeToolManager.currentSelectionBox;
		}
		SleeperVolumeToolManager.currentSelectionBox = selBox;
		SleeperVolumeToolManager.UpdateSleeperVisuals();
		SleeperVolumeToolManager.UpdateVolumeColors();
	}

	public static void UpdateVolumeColors()
	{
		int num = 0;
		if (SleeperVolumeToolManager.currentSelectionBox)
		{
			num = (int)((Prefab.PrefabSleeperVolume)SleeperVolumeToolManager.currentSelectionBox.UserData).groupId;
		}
		for (int i = 0; i < SleeperVolumeToolManager.registeredSleeperVolumes.Count; i++)
		{
			SelectionBox selectionBox = SleeperVolumeToolManager.registeredSleeperVolumes[i];
			Prefab.PrefabSleeperVolume prefabSleeperVolume = (Prefab.PrefabSleeperVolume)selectionBox.UserData;
			if (prefabSleeperVolume.groupId != 0)
			{
				if ((int)prefabSleeperVolume.groupId == num)
				{
					selectionBox.SetAllFacesColor(SleeperVolumeToolManager.groupSelectedColor, true);
				}
				else
				{
					selectionBox.SetAllFacesColor(SleeperVolumeToolManager.groupColors[(int)prefabSleeperVolume.groupId % SleeperVolumeToolManager.groupColors.Length], true);
				}
			}
			else
			{
				selectionBox.SetAllFacesColor(SelectionBoxManager.ColSleeperVolumeInactive, true);
			}
		}
		if (SleeperVolumeToolManager.currentSelectionBox)
		{
			SleeperVolumeToolManager.currentSelectionBox.SetAllFacesColor(SelectionBoxManager.ColSleeperVolume, true);
		}
	}

	public static void ShowSleepers(bool bShow = true)
	{
		if (SleeperVolumeToolManager.sleepers == null)
		{
			return;
		}
		foreach (KeyValuePair<Vector3i, List<SleeperVolumeToolManager.BlockData>> keyValuePair in SleeperVolumeToolManager.sleepers)
		{
			for (int i = 0; i < keyValuePair.Value.Count; i++)
			{
				Transform prefabT = keyValuePair.Value[i].prefabT;
				if (prefabT)
				{
					prefabT.gameObject.SetActive(bShow);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void UpdateSleeperVisuals()
	{
		if (SleeperVolumeToolManager.sleepers == null)
		{
			return;
		}
		foreach (KeyValuePair<Vector3i, List<SleeperVolumeToolManager.BlockData>> keyValuePair in SleeperVolumeToolManager.sleepers)
		{
			List<SleeperVolumeToolManager.BlockData> value = keyValuePair.Value;
			for (int i = 0; i < value.Count; i++)
			{
				SleeperVolumeToolManager.UpdateSleeperVisuals(value[i]);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void UpdateSleeperVisuals(SleeperVolumeToolManager.BlockData data)
	{
		Transform prefabT = data.prefabT;
		if (!SelectionBoxManager.Instance.GetCategory("SleeperVolume").IsVisible() || (SleeperVolumeToolManager.currentSelectionBox == null && !SleeperVolumeToolManager.xRayOn))
		{
			prefabT.gameObject.SetActive(false);
			return;
		}
		Vector3i vector3i = Vector3i.min;
		Vector3i vector3i2 = Vector3i.min;
		SelectionBox selectionBox = SleeperVolumeToolManager.currentSelectionBox;
		if (selectionBox != null)
		{
			vector3i = Vector3i.FromVector3Rounded(selectionBox.bounds.min);
			vector3i2 = Vector3i.FromVector3Rounded(selectionBox.bounds.max);
		}
		Vector3i position = data.position;
		if (position.x >= vector3i.x && position.x < vector3i2.x && position.y >= vector3i.y && position.y < vector3i2.y && position.z >= vector3i.z && position.z < vector3i2.z)
		{
			int index = 0;
			PrefabInstance selectedPrefabInstance = XUiC_WoPropsSleeperVolume.selectedPrefabInstance;
			Vector3i pos = position - selectedPrefabInstance.boundingBoxPosition;
			Prefab.PrefabSleeperVolume prefabSleeperVolume = selectedPrefabInstance.prefab.FindSleeperVolume(pos);
			if (prefabSleeperVolume != null && prefabSleeperVolume.isPriority)
			{
				index = 1;
			}
			if (data.block.spawnMode == BlockSleeper.eMode.Bandit)
			{
				index = 4;
			}
			if (data.block.spawnMode == BlockSleeper.eMode.Infested)
			{
				index = 5;
			}
			prefabT.gameObject.SetActive(true);
			SleeperVolumeToolManager.SetMats(prefabT, SleeperVolumeToolManager.typeMats[index]);
			return;
		}
		if (!SleeperVolumeToolManager.InAnyVolume(position))
		{
			prefabT.gameObject.SetActive(true);
			SleeperVolumeToolManager.SetMats(prefabT, SleeperVolumeToolManager.typeMats[2]);
			return;
		}
		if (SleeperVolumeToolManager.xRayOn && SleeperVolumeToolManager.currentSelectionBox == null)
		{
			prefabT.gameObject.SetActive(true);
			SleeperVolumeToolManager.SetMats(prefabT, SleeperVolumeToolManager.typeMats[3]);
			return;
		}
		prefabT.gameObject.SetActive(false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool InAnyVolume(Vector3i pos)
	{
		Vector3i zero = Vector3i.zero;
		Vector3i zero2 = Vector3i.zero;
		for (int i = 0; i < SleeperVolumeToolManager.registeredSleeperVolumes.Count; i++)
		{
			SelectionBox selectionBox = SleeperVolumeToolManager.registeredSleeperVolumes[i];
			zero.RoundToInt(selectionBox.bounds.min);
			zero2.RoundToInt(selectionBox.bounds.max);
			if (pos.x >= zero.x && pos.x < zero2.x && pos.y >= zero.y && pos.y < zero2.y && pos.z >= zero.z && pos.z < zero2.z)
			{
				return true;
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void SetMats(Transform t, Material _mat)
	{
		int value = SleeperVolumeToolManager.xRayOn ? -200000000 : -200000;
		_mat.SetInt("_Offset", value);
		MeshRenderer[] componentsInChildren = t.GetComponentsInChildren<MeshRenderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].sharedMaterial = _mat;
		}
	}

	public static bool GetXRay()
	{
		return SleeperVolumeToolManager.xRayOn;
	}

	public static void SetXRay(bool _on)
	{
		if (SleeperVolumeToolManager.xRayOn != _on)
		{
			SleeperVolumeToolManager.xRayOn = _on;
			int value = SleeperVolumeToolManager.xRayOn ? -200000000 : -200000;
			for (int i = 0; i < SleeperVolumeToolManager.typeMats.Count; i++)
			{
				SleeperVolumeToolManager.typeMats[i].SetInt("_Offset", value);
			}
			SleeperVolumeToolManager.UpdateSleeperVisuals();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static GameObject GroupGameObject = null;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<Vector3i, List<SleeperVolumeToolManager.BlockData>> sleepers;

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<SelectionBox> registeredSleeperVolumes = new List<SelectionBox>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool xRayOn = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cActiveIndex = 0;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cPriorityIndex = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cNoVolumeIndex = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cDarkIndex = 3;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cBanditIndex = 4;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cInfestedIndex = 5;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Color[] typeColors = new Color[]
	{
		new Color(1f, 0.6f, 0.1f),
		new Color(0.7f, 0.7f, 0.7f),
		new Color(1f, 0.1f, 1f),
		new Color(0.02f, 0.02f, 0.02f),
		new Color(0.1f, 1f, 0.1f),
		new Color(1f, 0.1f, 0.1f)
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<Material> typeMats = new List<Material>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static Color groupSelectedColor = new Color(0.9f, 0.9f, 1f, 0.4f);

	[PublicizedFrom(EAccessModifier.Private)]
	public static Color[] groupColors = new Color[]
	{
		new Color(1f, 0.2f, 0.2f, 0.4f),
		new Color(1f, 0.6f, 0.2f, 0.4f),
		new Color(1f, 1f, 0.2f, 0.4f),
		new Color(0.6f, 1f, 0.2f, 0.4f),
		new Color(0.2f, 1f, 0.2f, 0.4f),
		new Color(0.2f, 1f, 0.6f, 0.4f),
		new Color(0.2f, 1f, 1f, 0.4f),
		new Color(0.2f, 0.6f, 1f, 0.4f)
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static SelectionBox currentSelectionBox;

	[PublicizedFrom(EAccessModifier.Private)]
	public static SelectionBox previousSelectionBox;

	[PublicizedFrom(EAccessModifier.Private)]
	public class BlockData
	{
		public BlockSleeper block;

		public Transform prefabT;

		public Vector3i position;
	}
}

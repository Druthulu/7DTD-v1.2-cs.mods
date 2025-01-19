using System;
using System.Collections.Generic;
using UnityEngine;
using WorldGenerationEngineFinal;

public class POIMarkerToolManager
{
	public static PrefabManagerData prefabManagerData
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			PrefabManagerData result;
			if ((result = POIMarkerToolManager.m_prefabManagerData) == null)
			{
				result = (POIMarkerToolManager.m_prefabManagerData = new PrefabManagerData());
			}
			return result;
		}
	}

	public static void CleanUp()
	{
		PrefabManagerData prefabManagerData = POIMarkerToolManager.m_prefabManagerData;
		if (prefabManagerData != null)
		{
			prefabManagerData.Cleanup();
		}
		POIMarkerToolManager.m_prefabManagerData = null;
		if (POIMarkerToolManager.POIMarkers != null)
		{
			foreach (KeyValuePair<Vector3i, List<POIMarkerToolManager.PrefabAndPos>> keyValuePair in POIMarkerToolManager.POIMarkers)
			{
				for (int i = 0; i < keyValuePair.Value.Count; i++)
				{
					Transform prefabTrans = keyValuePair.Value[i].prefabTrans;
					if (prefabTrans && prefabTrans.gameObject != null)
					{
						UnityEngine.Object.Destroy(prefabTrans.gameObject);
					}
				}
			}
			POIMarkerToolManager.POIMarkers.Clear();
		}
		POIMarkerToolManager.ClearPOIMarkers();
	}

	public static void RegisterPOIMarker(SelectionBox _selBox)
	{
		if (POIMarkerToolManager.registeredPOIMarkers.Contains(_selBox))
		{
			return;
		}
		POIMarkerToolManager.registeredPOIMarkers.Add(_selBox);
		if (_selBox)
		{
			Prefab.Marker marker = _selBox.UserData as Prefab.Marker;
			if (marker != null && marker.MarkerType == Prefab.Marker.MarkerTypes.PartSpawn && marker.PartToSpawn != null && marker.PartToSpawn.Length > 0 && marker.PartDirty)
			{
				POIMarkerToolManager.spawnPrefabViz(marker, _selBox);
			}
		}
	}

	public static void DisplayPrefabPreviewForMarker(SelectionBox _selBox)
	{
		if (POIMarkerToolManager.prefabManagerData.AllPrefabDatas.Count == 0)
		{
			ThreadManager.RunCoroutineSync(POIMarkerToolManager.prefabManagerData.LoadPrefabs());
			POIMarkerToolManager.prefabManagerData.ShufflePrefabData(GameRandomManager.Instance.BaseSeed);
		}
		Prefab.Marker marker = _selBox.UserData as Prefab.Marker;
		if (marker == null)
		{
			return;
		}
		string text = "ghosttown,countrytown";
		if (PrefabEditModeManager.Instance.VoxelPrefab != null)
		{
			text = PrefabEditModeManager.Instance.VoxelPrefab.PrefabName;
			text = text.Replace("rwg_tile_", "");
			text = text.Split('_', StringSplitOptions.None)[0];
		}
		bool useAnySizeSmaller = !Prefab.Marker.MarkerSizes.Contains(new Vector3i(marker.Size.x, 0, marker.Size.z));
		Prefab prefab = POIMarkerToolManager.prefabManagerData.GetPreviewPrefabWithAnyTags(FastTags<TagGroup.Poi>.Parse(text), -1, new Vector2i(marker.Size.x, marker.Size.z), useAnySizeSmaller);
		if (prefab == null)
		{
			return;
		}
		prefab = prefab.Clone(false);
		int x = prefab.size.x;
		int z = prefab.size.z;
		prefab.RotateY(true, (prefab.rotationToFaceNorth + (int)marker.Rotations) % 4);
		Transform transform = _selBox.transform.Find("PrefabPreview");
		if (transform != null)
		{
			UnityEngine.Object.Destroy(transform.gameObject);
		}
		GameManager.Instance.StartCoroutine(prefab.ToTransform(true, true, true, false, _selBox.transform, "PrefabPreview", new Vector3(-((float)x / 2f), (float)prefab.yOffset + 0.15f, -((float)z / 2f)), 0));
	}

	public static void UnRegisterPOIMarker(SelectionBox _selBox)
	{
		if (!POIMarkerToolManager.registeredPOIMarkers.Contains(_selBox))
		{
			return;
		}
		POIMarkerToolManager.registeredPOIMarkers.Remove(_selBox);
	}

	public static void ClearPOIMarkers()
	{
		POIMarkerToolManager.registeredPOIMarkers.Clear();
	}

	public static void SelectionChanged(SelectionBox selBox)
	{
		if (selBox && selBox != POIMarkerToolManager.currentSelectionBox)
		{
			POIMarkerToolManager.previousSelectionBox = POIMarkerToolManager.currentSelectionBox;
			POIMarkerToolManager.currentSelectionBox = selBox;
		}
		POIMarkerToolManager.currentSelectionBox = selBox;
		if (XUiC_WoPropsPOIMarker.Instance != null && POIMarkerToolManager.currentSelectionBox != null && POIMarkerToolManager.currentSelectionBox.UserData is Prefab.Marker)
		{
			XUiC_WoPropsPOIMarker.Instance.CurrentMarker = (POIMarkerToolManager.currentSelectionBox.UserData as Prefab.Marker);
		}
		POIMarkerToolManager.UpdateAllColors();
	}

	public static void spawnPrefabViz(Prefab.Marker _currentMarker, SelectionBox selBox)
	{
		Transform transform = selBox.transform.Find("PrefabPreview");
		if (transform != null)
		{
			UnityEngine.Object.Destroy(transform.gameObject);
		}
		Prefab prefab = new Prefab();
		prefab.Load(_currentMarker.PartToSpawn, true, false, true, false);
		prefab.Init(0, 0);
		prefab.RotateY(false, (prefab.rotationToFaceNorth + (int)_currentMarker.Rotations) % 4);
		ValueTuple<SelectionCategory, SelectionBox>? valueTuple;
		GameManager.Instance.StartCoroutine(prefab.ToTransform(true, true, true, false, (SelectionBoxManager.Instance.Selection != null) ? valueTuple.GetValueOrDefault().Item2.transform : null, "PrefabPreview", new Vector3(-((float)prefab.size.x / 2f), 0.1f, -((float)prefab.size.z / 2f)), 0));
	}

	public static void UpdateAllColors()
	{
		int num = 0;
		if (POIMarkerToolManager.currentSelectionBox)
		{
			Prefab.Marker marker = POIMarkerToolManager.currentSelectionBox.UserData as Prefab.Marker;
			if (marker != null)
			{
				num = marker.GroupId;
			}
		}
		for (int i = 0; i < POIMarkerToolManager.registeredPOIMarkers.Count; i++)
		{
			SelectionBox selectionBox = POIMarkerToolManager.registeredPOIMarkers[i];
			Prefab.Marker marker = selectionBox.UserData as Prefab.Marker;
			if (marker.GroupId == num)
			{
				selectionBox.SetAllFacesColor(marker.GroupColor + new Color(0.2f, 0.2f, 0.2f, 0f), true);
			}
			else
			{
				selectionBox.SetAllFacesColor(marker.GroupColor, true);
			}
		}
		if (POIMarkerToolManager.currentSelectionBox)
		{
			Prefab.Marker marker = POIMarkerToolManager.currentSelectionBox.UserData as Prefab.Marker;
			if (marker != null)
			{
				if (marker.MarkerType == Prefab.Marker.MarkerTypes.PartSpawn)
				{
					POIMarkerToolManager.currentSelectionBox.SetAllFacesColor(new Color(0f, 0f, 0f, 0f), true);
					return;
				}
				POIMarkerToolManager.currentSelectionBox.SetAllFacesColor(marker.GroupColor + new Color(0.5f, 0.5f, 0.5f, 0f), true);
			}
		}
	}

	public static void ShowPOIMarkers(bool bShow = true)
	{
		if (POIMarkerToolManager.POIMarkers == null)
		{
			return;
		}
		foreach (KeyValuePair<Vector3i, List<POIMarkerToolManager.PrefabAndPos>> keyValuePair in POIMarkerToolManager.POIMarkers)
		{
			for (int i = 0; i < keyValuePair.Value.Count; i++)
			{
				Transform prefabTrans = keyValuePair.Value[i].prefabTrans;
				if (prefabTrans && prefabTrans.gameObject != null)
				{
					prefabTrans.gameObject.SetActive(bShow);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static PrefabManagerData m_prefabManagerData;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<Vector3i, List<POIMarkerToolManager.PrefabAndPos>> POIMarkers = null;

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<SelectionBox> registeredPOIMarkers = new List<SelectionBox>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static Material markerMat;

	public static SelectionBox currentSelectionBox;

	[PublicizedFrom(EAccessModifier.Private)]
	public static SelectionBox previousSelectionBox;

	[PublicizedFrom(EAccessModifier.Private)]
	public struct PrefabAndPos
	{
		public Transform prefabTrans;

		public Vector3i position;
	}
}

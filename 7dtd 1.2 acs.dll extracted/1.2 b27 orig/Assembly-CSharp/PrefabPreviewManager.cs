using System;
using System.Collections.Generic;
using Platform;
using UnityEngine;
using WorldGenerationEngineFinal;

public class PrefabPreviewManager
{
	public static WorldBuilder worldBuilder
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			XUiC_WorldGenerationWindowGroup instance = XUiC_WorldGenerationWindowGroup.Instance;
			if (instance == null)
			{
				return null;
			}
			return instance.worldBuilder;
		}
	}

	public void InitPrefabs()
	{
		if (this.initialized)
		{
			return;
		}
		this.prefabsAround.Clear();
		PrefabPreviewManager.worldBuilder.PrefabManager.GetPrefabsAround(XUiC_WorldGenerationWindowGroup.Instance.PreviewWindow.GetCameraPosition(), 100000f, this.prefabsAround);
		this.UpdatePrefabsAround(this.prefabsAround);
		this.initialized = true;
	}

	public void RemovePrefabs()
	{
		this.ClearDisplayedPrefabs();
		this.displayedPrefabs.Clear();
		this.prefabsAround.Clear();
		this.initialized = false;
	}

	public void Update()
	{
		if (Time.time - this.lastDisplayUpdate < 2f)
		{
			return;
		}
		this.lastDisplayUpdate = Time.time;
		this.ForceUpdate();
	}

	public void ForceUpdate()
	{
		this.UpdateDisplay();
		if (Time.time - this.lastTime < 2f)
		{
			return;
		}
		this.lastTime = Time.time;
		if (PrefabPreviewManager.worldBuilder == null)
		{
			return;
		}
		if (XUiC_WorldGenerationWindowGroup.Instance == null)
		{
			return;
		}
		if (XUiC_WorldGenerationWindowGroup.Instance.PreviewWindow == null)
		{
			return;
		}
		if (PrefabPreviewManager.worldBuilder.PrefabManager.UsedPrefabsWorld == null)
		{
			return;
		}
		this.InitPrefabs();
	}

	public void ClearDisplayedPrefabs()
	{
		if (this.displayedPrefabs == null || this.displayedPrefabs.Count == 0)
		{
			return;
		}
		List<int> list = new List<int>();
		foreach (KeyValuePair<int, PrefabPreviewManager.PrefabGameObject> keyValuePair in this.displayedPrefabs)
		{
			list.Add(keyValuePair.Key);
		}
		foreach (int key in list)
		{
			if (!(this.displayedPrefabs[key].go == null))
			{
				MeshFilter[] componentsInChildren = this.displayedPrefabs[key].go.GetComponentsInChildren<MeshFilter>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					UnityEngine.Object.Destroy(componentsInChildren[i].mesh);
				}
				Renderer[] componentsInChildren2 = this.displayedPrefabs[key].go.GetComponentsInChildren<Renderer>();
				for (int j = 0; j < componentsInChildren2.Length; j++)
				{
					UnityEngine.Object.Destroy(componentsInChildren2[j].material);
				}
				UnityEngine.Object.Destroy(this.displayedPrefabs[key].go);
				this.displayedPrefabs.Remove(key);
			}
		}
	}

	public void UpdatePrefabsAround(Dictionary<int, PrefabDataInstance> _prefabsAround)
	{
		foreach (KeyValuePair<int, PrefabDataInstance> keyValuePair in _prefabsAround)
		{
			PrefabDataInstance value = keyValuePair.Value;
			if (!this.displayedPrefabs.ContainsKey(value.id))
			{
				string name = value.location.Name;
				if (PathAbstractions.PrefabImpostersSearchPaths.GetLocation(name, null, null).Type != PathAbstractions.EAbstractedLocationType.None)
				{
					PrefabPreviewManager.PrefabGameObject prefabGameObject = new PrefabPreviewManager.PrefabGameObject();
					prefabGameObject.prefabInstance = value;
					this.displayedPrefabs.Add(value.id, prefabGameObject);
				}
			}
		}
		List<int> list = new List<int>();
		foreach (KeyValuePair<int, PrefabPreviewManager.PrefabGameObject> keyValuePair2 in this.displayedPrefabs)
		{
			if (!_prefabsAround.ContainsKey(keyValuePair2.Key))
			{
				list.Add(keyValuePair2.Key);
			}
		}
		foreach (int key in list)
		{
			if (!(this.displayedPrefabs[key].go == null))
			{
				MeshFilter[] componentsInChildren = this.displayedPrefabs[key].go.GetComponentsInChildren<MeshFilter>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					UnityEngine.Object.Destroy(componentsInChildren[i].mesh);
				}
				Renderer[] componentsInChildren2 = this.displayedPrefabs[key].go.GetComponentsInChildren<Renderer>();
				for (int j = 0; j < componentsInChildren2.Length; j++)
				{
					UnityEngine.Object.Destroy(componentsInChildren2[j].material);
				}
				UnityEngine.Object.Destroy(this.displayedPrefabs[key].go);
				this.displayedPrefabs.Remove(key);
			}
		}
	}

	public void UpdateDisplay()
	{
		if (XUiC_WorldGenerationWindowGroup.Instance.PreviewQualityLevel == XUiC_WorldGenerationWindowGroup.PreviewQuality.NoPreview)
		{
			return;
		}
		MicroStopwatch microStopwatch = new MicroStopwatch();
		if (this.parentTransform == null)
		{
			this.parentTransform = new GameObject("PrefabsLOD").transform;
			this.parentTransform.gameObject.layer = 11;
		}
		foreach (KeyValuePair<int, PrefabPreviewManager.PrefabGameObject> keyValuePair in this.displayedPrefabs)
		{
			PrefabPreviewManager.PrefabGameObject value = keyValuePair.Value;
			PrefabDataInstance prefabInstance = value.prefabInstance;
			Vector3 vector = prefabInstance.boundingBoxPosition.ToVector3();
			Vector3 a = prefabInstance.boundingBoxPosition.ToVector3();
			Vector3 vector2 = prefabInstance.boundingBoxSize.ToVector3();
			if (prefabInstance.rotation % 2 == 0)
			{
				vector += new Vector3((float)prefabInstance.boundingBoxSize.x * 0.5f, 0f, (float)prefabInstance.boundingBoxSize.z * 0.5f);
				a += new Vector3((float)prefabInstance.boundingBoxSize.x * 0.5f, 0f, (float)prefabInstance.boundingBoxSize.z * 0.5f);
			}
			else
			{
				vector += new Vector3((float)prefabInstance.boundingBoxSize.z * 0.5f, 0f, (float)prefabInstance.boundingBoxSize.x * 0.5f);
				a += new Vector3((float)prefabInstance.boundingBoxSize.z * 0.5f, 0f, (float)prefabInstance.boundingBoxSize.x * 0.5f);
				vector2 = new Vector3(vector2.z, vector2.y, vector2.x);
			}
			Vector3 zero = Vector3.zero;
			Quaternion rotation = Quaternion.identity;
			switch (prefabInstance.rotation)
			{
			case 0:
				zero = new Vector3(-0.5f, 0f, -0.5f);
				break;
			case 1:
				zero = new Vector3(0.5f, 0f, -0.5f);
				rotation = Quaternion.Euler(0f, 270f, 0f);
				break;
			case 2:
				zero = new Vector3(0.5f, 0f, 0.5f);
				rotation = Quaternion.Euler(0f, 180f, 0f);
				break;
			case 3:
				zero = new Vector3(-0.5f, 0f, 0.5f);
				rotation = Quaternion.Euler(0f, 90f, 0f);
				break;
			}
			if (Utils.FastAbs(vector.x - (float)((int)vector.x)) > 0.001f)
			{
				vector.x += zero.x;
			}
			if (Utils.FastAbs(vector.z - (float)((int)vector.z)) > 0.001f)
			{
				vector.z += zero.z;
			}
			float num = 0f;
			Utils.DrawBounds(new Bounds(a + new Vector3(0f, (float)prefabInstance.boundingBoxSize.y * 0.5f + 0.1f + num, 0f) - Origin.position, vector2), Color.green, 2f);
			if (!value.go)
			{
				XUiC_WorldGenerationWindowGroup.PreviewQuality previewQuality = XUiC_WorldGenerationWindowGroup.Instance.PreviewQualityLevel;
				if (((DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent() || SystemInfo.systemMemorySize < 1300) && previewQuality > XUiC_WorldGenerationWindowGroup.PreviewQuality.Default)
				{
					previewQuality = XUiC_WorldGenerationWindowGroup.PreviewQuality.Default;
				}
				GameObject gameObject;
				if (previewQuality == XUiC_WorldGenerationWindowGroup.PreviewQuality.Highest)
				{
					string name = prefabInstance.location.Name;
					gameObject = SimpleMeshFile.ReadGameObject(PathAbstractions.PrefabImpostersSearchPaths.GetLocation(name, null, null), 0f, null, true, false, null, null);
					if (gameObject == null)
					{
						continue;
					}
					Renderer[] componentsInChildren = gameObject.GetComponentsInChildren<Renderer>();
					for (int i = 0; i < componentsInChildren.Length; i++)
					{
						componentsInChildren[i].gameObject.layer = 11;
					}
				}
				else if (previewQuality >= XUiC_WorldGenerationWindowGroup.PreviewQuality.Low && previewQuality <= XUiC_WorldGenerationWindowGroup.PreviewQuality.High)
				{
					gameObject = new GameObject();
					if (!prefabInstance.prefab.Name.Contains("rwg_tile") && !prefabInstance.prefab.Name.Contains("part_driveway"))
					{
						Transform transform = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
						transform.SetParent(gameObject.transform);
						transform.localPosition = new Vector3(0f, (float)(prefabInstance.boundingBoxSize.y / 2), 0f);
						transform.localScale = prefabInstance.boundingBoxSize.ToVector3();
						if ((int)(prefabInstance.previewColor.r + prefabInstance.previewColor.g + prefabInstance.previewColor.b) != 765)
						{
							transform.GetComponent<Renderer>().material.color = prefabInstance.previewColor;
						}
						Renderer[] componentsInChildren = gameObject.GetComponentsInChildren<Renderer>();
						for (int i = 0; i < componentsInChildren.Length; i++)
						{
							componentsInChildren[i].gameObject.layer = 11;
						}
					}
				}
				else
				{
					gameObject = new GameObject();
				}
				value.go = gameObject;
				gameObject.layer = 11;
				Transform transform2 = gameObject.transform;
				transform2.name = prefabInstance.location.Name;
				transform2.SetParent(this.parentTransform, false);
				Vector3 position = vector + new Vector3(0f, 0.1f + num - 5.1f, 0f) - Origin.position;
				transform2.SetPositionAndRotation(position, rotation);
				GameObject gameObject2 = new GameObject(prefabInstance.prefab.Name);
				Transform transform3 = gameObject2.transform;
				transform3.SetParent(transform2);
				transform3.rotation = Quaternion.Euler(90f, gameObject.transform.rotation.eulerAngles.y, 0f);
				transform3.localPosition = new Vector3(0f, (float)(prefabInstance.boundingBoxSize.y + prefabInstance.prefab.yOffset) + 0.25f, 0f);
				gameObject2.layer = 11;
				Vector2i vector2i = new Vector2i(((int)vector.x + PrefabPreviewManager.worldBuilder.WorldSize / 2) / 150, ((int)vector.z + PrefabPreviewManager.worldBuilder.WorldSize / 2) / 150);
				TextMesh textMesh = gameObject2.AddMissingComponent<TextMesh>();
				textMesh.alignment = TextAlignment.Center;
				textMesh.anchor = TextAnchor.MiddleCenter;
				textMesh.fontSize = (prefabInstance.prefab.Name.Contains("trader") ? 100 : 20);
				textMesh.color = (prefabInstance.prefab.Name.Contains("trader") ? Color.red : Color.green);
				textMesh.text = string.Concat(new string[]
				{
					prefabInstance.prefab.Name,
					Environment.NewLine,
					string.Format("pos {0}{1}", prefabInstance.boundingBoxPosition, Environment.NewLine),
					string.Format("yoffset {0}{1}", prefabInstance.prefab.yOffset, Environment.NewLine),
					string.Format("rots to north {0}, total left {1}{2}", prefabInstance.prefab.RotationsToNorth, prefabInstance.rotation, Environment.NewLine),
					string.Format("tile pos {0}{1}", vector2i, Environment.NewLine),
					string.Format("score {0}", prefabInstance.prefab.DensityScore)
				});
				if (microStopwatch.ElapsedMilliseconds > 50L)
				{
					this.lastDisplayUpdate = 0f;
					return;
				}
			}
		}
		foreach (KeyValuePair<int, PrefabPreviewManager.PrefabGameObject> keyValuePair2 in this.displayedPrefabs)
		{
			if (!(keyValuePair2.Value.go == null))
			{
				Transform transform4 = keyValuePair2.Value.go.transform;
				for (int j = 0; j < transform4.childCount; j++)
				{
					transform4.GetChild(j).gameObject.SetActive(true);
				}
			}
		}
	}

	public void ClearOldPreview()
	{
		this.RemovePrefabs();
	}

	public void Cleanup()
	{
		this.RemovePrefabs();
		if (this.parentTransform)
		{
			UnityEngine.Object.Destroy(this.parentTransform.gameObject);
			this.parentTransform = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cPrefabYPosition = 5.1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cDisplayUpdateDelay = 2f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cPrefabListUpdateDelay = 2f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cLodPoiDistance = 100000;

	public static bool ReadyToDisplay;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<int, PrefabDataInstance> prefabsAround = new Dictionary<int, PrefabDataInstance>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<int, PrefabPreviewManager.PrefabGameObject> displayedPrefabs = new Dictionary<int, PrefabPreviewManager.PrefabGameObject>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform parentTransform;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastDisplayUpdate;

	public bool initialized;

	[PublicizedFrom(EAccessModifier.Private)]
	public class PrefabGameObject
	{
		public PrefabDataInstance prefabInstance;

		public GameObject go;
	}
}

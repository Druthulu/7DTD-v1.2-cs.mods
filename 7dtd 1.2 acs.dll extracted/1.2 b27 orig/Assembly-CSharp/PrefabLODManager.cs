using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

public class PrefabLODManager
{
	public void TriggerUpdate()
	{
		this.lastTime = 0f;
		this.lastDisplayUpdate = 0f;
	}

	public void FrameUpdate()
	{
		try
		{
			if (!GameManager.IsDedicatedServer)
			{
				float time = Time.time;
				if (time - this.lastDisplayUpdate >= 1f)
				{
					this.lastDisplayUpdate = time;
					World world = GameManager.Instance.World;
					if (!world.ChunkClusters[0].IsFixedSize)
					{
						List<EntityPlayerLocal> localPlayers = world.GetLocalPlayers();
						if (localPlayers.Count != 0)
						{
							EntityPlayerLocal entityPlayerLocal = localPlayers[0];
							if (!(entityPlayerLocal == null))
							{
								this.UpdateDisplay(entityPlayerLocal);
								if (time - this.lastTime >= 1f)
								{
									this.lastTime = time;
									DynamicPrefabDecorator dynamicPrefabDecorator = world.ChunkCache.ChunkProvider.GetDynamicPrefabDecorator();
									if (dynamicPrefabDecorator != null)
									{
										for (int i = 0; i < world.Players.list.Count; i++)
										{
											this.prefabsAroundFar.Clear();
											this.prefabsAroundNear.Clear();
											EntityPlayer entityPlayer = world.Players.list[i];
											if (!(entityPlayer == null))
											{
												Vector3 position = entityPlayer.position;
												int num = (entityPlayer.ChunkObserver != null) ? entityPlayer.ChunkObserver.viewDim : GamePrefs.GetInt(EnumGamePrefs.OptionsGfxViewDistance);
												num = (num - 1) * 16;
												if (!entityPlayer.isEntityRemote)
												{
													dynamicPrefabDecorator.GetPrefabsAround(position, (float)num, (float)PrefabLODManager.lodPoiDistance, this.prefabsAroundFar, this.prefabsAroundNear);
													this.UpdatePrefabsAround(this.prefabsAroundFar, this.prefabsAroundNear);
												}
												else if ((entityPlayer.position - entityPlayerLocal.position).sqrMagnitude <= (float)(2 * num * (2 * num)))
												{
													dynamicPrefabDecorator.GetPrefabsAround(position, (float)num, (float)PrefabLODManager.lodPoiDistance, this.prefabsAroundFar, this.prefabsAroundNear);
													entityPlayer.SetPrefabsAroundNear(this.prefabsAroundNear);
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}
		finally
		{
		}
	}

	public PrefabLODManager.PrefabGameObject GetInstance(int id)
	{
		PrefabLODManager.PrefabGameObject result;
		this.displayedPrefabs.TryGetValue(id, out result);
		return result;
	}

	public void UpdatePrefabsAround(Dictionary<int, PrefabInstance> _prefabsAroundFar, Dictionary<int, PrefabInstance> _prefabsAroundNear)
	{
		try
		{
			if (!GameManager.IsDedicatedServer)
			{
				List<EntityPlayerLocal> localPlayers = GameManager.Instance.World.GetLocalPlayers();
				if (localPlayers.Count != 0)
				{
					localPlayers[0].SetPrefabsAroundNear(_prefabsAroundNear);
					foreach (KeyValuePair<int, PrefabInstance> keyValuePair in _prefabsAroundFar)
					{
						PrefabInstance value = keyValuePair.Value;
						if (!this.displayedPrefabs.ContainsKey(value.id))
						{
							PathAbstractions.AbstractedLocation imposterLocation = value.GetImposterLocation();
							if (imposterLocation.Type != PathAbstractions.EAbstractedLocationType.None)
							{
								string fullPath = imposterLocation.FullPath;
								PrefabLODManager.PrefabGameObject prefabGameObject = new PrefabLODManager.PrefabGameObject();
								prefabGameObject.prefabInstance = value;
								prefabGameObject.meshPath = imposterLocation.FullPath;
								this.displayedPrefabs.Add(value.id, prefabGameObject);
								if (!this.meshPathToData.ContainsKey(fullPath))
								{
									this.meshPathToData.Add(fullPath, new PrefabLODManager.MeshPrefabSet());
								}
								this.meshPathToData[fullPath].prefabIDs.Add(value.id);
							}
						}
					}
					List<int> list = new List<int>();
					foreach (KeyValuePair<int, PrefabLODManager.PrefabGameObject> keyValuePair2 in this.displayedPrefabs)
					{
						int key = keyValuePair2.Key;
						if (!_prefabsAroundFar.ContainsKey(key))
						{
							list.Add(key);
						}
					}
					if (list.Count > 0)
					{
						this.removePrefabs(list);
					}
				}
			}
		}
		finally
		{
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void removePrefabs(List<int> toRemove)
	{
		try
		{
			foreach (int num in toRemove)
			{
				string meshPath = this.displayedPrefabs[num].meshPath;
				GameObject go = this.displayedPrefabs[num].go;
				if (go)
				{
					PrefabLODManager.MeshPrefabSet meshPrefabSet = this.meshPathToData[meshPath];
					meshPrefabSet.prefabIDs.Remove(num);
					if (meshPrefabSet.prefabIDs.Count == 0)
					{
						Mesh[] meshes = meshPrefabSet.meshInfo.meshes;
						for (int i = 0; i < meshes.Length; i++)
						{
							UnityEngine.Object.Destroy(meshes[i]);
						}
						this.meshPathToData.Remove(meshPath);
					}
					this.displayedPrefabs.Remove(num);
					UnityEngine.Object.Destroy(go);
				}
			}
		}
		finally
		{
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BuildGameObjectFromMeshInfo(SimpleMeshInfo _meshInfo, PrefabLODManager.PrefabGameObject _pgo)
	{
		try
		{
			_pgo.go = SimpleMeshFile.CreateUnityObjects(_meshInfo);
			GameObject go = _pgo.go;
			PrefabInstance prefabInstance = _pgo.prefabInstance;
			if (!go)
			{
				Log.Error("Loading LOD mesh for Prefab " + _pgo.prefabInstance.name + " failed.");
			}
			else
			{
				go.name = prefabInstance.location.Name;
				Transform transform = go.transform;
				transform.SetParent(this.parentTransform, false);
				Vector3 vector = prefabInstance.boundingBoxPosition.ToVector3();
				vector += new Vector3((float)prefabInstance.boundingBoxSize.x * 0.5f, -4f, (float)prefabInstance.boundingBoxSize.z * 0.5f);
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
				float num;
				if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
				{
					num = prefabInstance.prefab.distantPOIYOffset;
				}
				else
				{
					num = prefabInstance.yOffsetOfPrefab;
				}
				vector.y += num;
				transform.SetPositionAndRotation(vector - Origin.position, rotation);
				for (int i = 0; i < transform.childCount; i++)
				{
					transform.GetChild(i).gameObject.SetActive(false);
				}
				if (OcclusionManager.Instance.cullPrefabs)
				{
					Occludee.Add(go);
				}
			}
		}
		finally
		{
		}
	}

	public void meshLoadedCallback(SimpleMeshInfo meshInfo, object userCallbackData)
	{
		try
		{
			string text = (string)userCallbackData;
			PrefabLODManager.MeshPrefabSet meshPrefabSet = this.meshPathToData[text];
			if (meshPrefabSet.meshInfo != null)
			{
				Log.Error("Meshes have already been provided for path " + text);
			}
			else
			{
				meshPrefabSet.meshInfo = meshInfo;
				meshPrefabSet.isLoading = false;
				for (int i = 0; i < meshPrefabSet.prefabIDs.Count; i++)
				{
					PrefabLODManager.PrefabGameObject pgo = this.displayedPrefabs[meshPrefabSet.prefabIDs[i]];
					this.BuildGameObjectFromMeshInfo(meshInfo, pgo);
				}
			}
		}
		finally
		{
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateDisplay(EntityPlayerLocal _localPlayer)
	{
		try
		{
			this.stopWatch.ResetAndRestart();
			if (!this.parentTransform)
			{
				this.parentTransform = new GameObject("PrefabsLOD").transform;
				Origin.Add(this.parentTransform, 0);
				this.sharedMat = MeshDescription.GetOpaqueMaterial();
			}
			bool bTextureArray = MeshDescription.meshes[0].bTextureArray;
			foreach (KeyValuePair<string, PrefabLODManager.MeshPrefabSet> keyValuePair in this.meshPathToData)
			{
				string key = keyValuePair.Key;
				PrefabLODManager.MeshPrefabSet value = keyValuePair.Value;
				if (value.meshInfo != null)
				{
					using (List<int>.Enumerator enumerator2 = value.prefabIDs.GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							int key2 = enumerator2.Current;
							PrefabLODManager.PrefabGameObject prefabGameObject = this.displayedPrefabs[key2];
							if (prefabGameObject.go == null)
							{
								this.BuildGameObjectFromMeshInfo(value.meshInfo, prefabGameObject);
							}
						}
						continue;
					}
				}
				if (!value.isLoading)
				{
					value.isLoading = true;
					string filename = key;
					float offsetY = 0f;
					Material mat = this.sharedMat;
					bool bTextureArray2 = bTextureArray;
					bool markMeshesNoLongerReadable = true;
					SimpleMeshFile.GameObjectMeshesReadCallback asyncCallback = new SimpleMeshFile.GameObjectMeshesReadCallback(this.meshLoadedCallback);
					SimpleMeshFile.ReadMesh(filename, offsetY, mat, bTextureArray2, markMeshesNoLongerReadable, key, asyncCallback);
					if (this.stopWatch.ElapsedMicroseconds > 900L)
					{
						this.lastDisplayUpdate = 0f;
						return;
					}
				}
			}
			Vector3 position = _localPlayer.position;
			int num = (int)position.x;
			int num2 = num;
			int num3 = (int)position.z;
			int num4 = num3;
			int num5 = 0;
			int num6 = 0;
			int num7 = 0;
			int num8 = 0;
			List<Chunk> chunkArrayCopySync = GameManager.Instance.World.ChunkCache.GetChunkArrayCopySync();
			for (int i = 0; i < chunkArrayCopySync.Count; i++)
			{
				Chunk chunk = chunkArrayCopySync[i];
				if (chunk.IsDisplayed && chunk.IsCollisionMeshGenerated && (chunk.GetAABB().center - position).sqrMagnitude <= 36864f)
				{
					if (chunk.worldPosIMin.x < num)
					{
						num = chunk.worldPosIMin.x;
						num5 = 1;
					}
					else if (chunk.worldPosIMin.x == num)
					{
						num5++;
					}
					if (chunk.worldPosIMax.x > num2)
					{
						num2 = chunk.worldPosIMax.x;
						num6 = 1;
					}
					else if (chunk.worldPosIMax.x == num2)
					{
						num6++;
					}
					if (chunk.worldPosIMin.z < num3)
					{
						num3 = chunk.worldPosIMin.z;
						num7 = 1;
					}
					else if (chunk.worldPosIMin.z == num3)
					{
						num7++;
					}
					if (chunk.worldPosIMax.z > num4)
					{
						num4 = chunk.worldPosIMax.z;
						num8 = 1;
					}
					else if (chunk.worldPosIMax.z == num4)
					{
						num8++;
					}
				}
			}
			int num9 = num2 - num + 1;
			int num10 = num4 - num3 + 1;
			if (num5 * 16 != num10)
			{
				num += 16;
			}
			if (num6 * 16 != num10)
			{
				num2 -= 16;
			}
			if (num7 * 16 != num9)
			{
				num3 += 16;
			}
			if (num8 * 16 != num9)
			{
				num4 -= 16;
			}
			Vector3 b = new Vector3((float)num, 0f, (float)num3);
			Vector3 a = new Vector3((float)(num2 + 1), 256f, (float)(num4 + 1));
			Bounds bounds = new Bounds((a + b) * 0.5f, a - b);
			foreach (KeyValuePair<int, PrefabLODManager.PrefabGameObject> keyValuePair2 in this.displayedPrefabs)
			{
				PrefabLODManager.PrefabGameObject value2 = keyValuePair2.Value;
				if (value2.go)
				{
					PrefabInstance prefabInstance = value2.prefabInstance;
					Vector3 vector = prefabInstance.boundingBoxPosition;
					Vector3 vector2 = prefabInstance.boundingBoxPosition + prefabInstance.boundingBoxSize;
					if (!(bounds.Contains(vector) + bounds.Contains(vector2) + bounds.Contains(new Vector3(vector2.x, vector.y, vector.z)) + bounds.Contains(new Vector3(vector.x, vector.y, vector2.z))))
					{
						if (!value2.isAllShown)
						{
							value2.isAllShown = true;
							Transform transform = value2.go.transform;
							int childCount = transform.childCount;
							for (int j = 0; j < childCount; j++)
							{
								transform.GetChild(j).gameObject.SetActive(true);
							}
						}
					}
					else
					{
						value2.isAllShown = false;
						Transform transform2 = value2.go.transform;
						int childCount2 = transform2.childCount;
						for (int k = 0; k < childCount2; k++)
						{
							Transform child = transform2.GetChild(k);
							string name = child.name;
							StringParsers.SeparatorPositions separatorPositions = StringParsers.GetSeparatorPositions(name, ',', 1, 0, -1);
							if (separatorPositions.TotalFound != 1)
							{
								break;
							}
							int num11 = StringParsers.ParseSInt32(name, 0, separatorPositions.Sep1 - 1, NumberStyles.Integer);
							int num12 = StringParsers.ParseSInt32(name, separatorPositions.Sep1 + 1, -1, NumberStyles.Integer);
							float num13 = 1f;
							float num14 = 1f;
							switch (prefabInstance.rotation)
							{
							case 1:
							{
								int num15 = num11;
								num11 = -num12;
								num12 = num15;
								num13 = -1f;
								break;
							}
							case 2:
								num11 = -num11;
								num12 = -num12;
								num13 = -1f;
								num14 = -1f;
								break;
							case 3:
							{
								int num16 = num11;
								num11 = num12;
								num12 = -num16;
								num14 = -1f;
								break;
							}
							}
							num11 *= 32;
							num12 *= 32;
							num11 += prefabInstance.boundingBoxPosition.x;
							num12 += prefabInstance.boundingBoxPosition.z;
							num11 += prefabInstance.boundingBoxSize.x / 2;
							num12 += prefabInstance.boundingBoxSize.z / 2;
							Vector3 point = new Vector3((float)num11, (float)prefabInstance.boundingBoxPosition.y, (float)num12);
							Vector3 point2 = new Vector3((float)num11 + num13 * 32f, (float)prefabInstance.boundingBoxPosition.y, (float)num12 + num14 * 32f);
							if (bounds.Contains(point) && bounds.Contains(point2))
							{
								if (child.gameObject.activeSelf)
								{
									child.gameObject.SetActive(false);
								}
							}
							else if (!child.gameObject.activeSelf)
							{
								child.gameObject.SetActive(true);
							}
						}
					}
				}
			}
		}
		finally
		{
		}
	}

	public void Cleanup()
	{
		if (this.parentTransform != null)
		{
			UnityEngine.Object.Destroy(this.parentTransform.gameObject);
			this.parentTransform = null;
		}
		this.removePrefabs(this.displayedPrefabs.Keys.ToList<int>());
	}

	public void SetPOIDistance(int _distance)
	{
		PrefabLODManager.lodPoiDistance = _distance;
		this.lastDisplayUpdate = 0f;
		this.lastTime = 0f;
	}

	public void UpdateMaterials()
	{
		try
		{
			UnityEngine.Object.Destroy(this.sharedMat);
			this.sharedMat = MeshDescription.GetOpaqueMaterial();
			foreach (KeyValuePair<int, PrefabLODManager.PrefabGameObject> keyValuePair in this.displayedPrefabs)
			{
				PrefabLODManager.PrefabGameObject value = keyValuePair.Value;
				if (value.go)
				{
					MeshDescription.GetOpaqueMaterial();
					Renderer[] componentsInChildren = value.go.GetComponentsInChildren<Renderer>(true);
					for (int i = 0; i < componentsInChildren.Length; i++)
					{
						componentsInChildren[i].material = this.sharedMat;
					}
				}
			}
		}
		finally
		{
		}
	}

	public const int cPrefabYPosition = 4;

	public const int cLodPoiDistance = 1000;

	public static int lodPoiDistance = 1000;

	[PublicizedFrom(EAccessModifier.Private)]
	public Material sharedMat;

	public Dictionary<int, PrefabInstance> prefabsAroundFar = new Dictionary<int, PrefabInstance>();

	public Dictionary<int, PrefabInstance> prefabsAroundNear = new Dictionary<int, PrefabInstance>();

	public Dictionary<int, PrefabLODManager.PrefabGameObject> displayedPrefabs = new Dictionary<int, PrefabLODManager.PrefabGameObject>();

	public Dictionary<string, PrefabLODManager.MeshPrefabSet> meshPathToData = new Dictionary<string, PrefabLODManager.MeshPrefabSet>();

	public Transform parentTransform;

	public float lastTime;

	public float lastDisplayUpdate;

	public MicroStopwatch stopWatch = new MicroStopwatch();

	public class PrefabGameObject
	{
		public string meshPath;

		public PrefabInstance prefabInstance;

		public GameObject go;

		public bool isAllShown;
	}

	public class MeshPrefabSet
	{
		public MeshPrefabSet()
		{
			this.prefabIDs = new List<int>();
		}

		public bool isLoading;

		public SimpleMeshInfo meshInfo;

		public List<int> prefabIDs;
	}
}

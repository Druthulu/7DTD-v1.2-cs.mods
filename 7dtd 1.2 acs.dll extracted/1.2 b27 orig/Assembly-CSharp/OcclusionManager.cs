using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class OcclusionManager : MonoBehaviour
{
	public static void Load()
	{
		UnityEngine.Object.Instantiate<GameObject>(Resources.Load<GameObject>("Occlusion/Occlusion"));
	}

	public void EnableCulling(bool isCulling)
	{
		this.gpuCullingEnabled = isCulling;
	}

	public void SetMultipleCameras(bool isMultiple)
	{
		if (!this.isEnabled)
		{
			return;
		}
		this.gpuCullingEnabled = !isMultiple;
		if (isMultiple)
		{
			this.SetRenderersEnabled(true);
		}
	}

	public void WorldChanging(bool isEditWorld)
	{
		this.isEnabled = GamePrefs.GetBool(EnumGamePrefs.OptionsGfxOcclusion);
		if (isEditWorld)
		{
			this.isEnabled = false;
		}
		if (GameManager.IsDedicatedServer)
		{
			this.isEnabled = false;
		}
		if (this.isEnabled)
		{
			if (!SystemInfo.supportsAsyncGPUReadback)
			{
				Log.Warning("Occlusion: !supportsAsyncGPUReadback");
				this.isEnabled = false;
			}
			if (!SystemInfo.supportsComputeShaders)
			{
				Log.Warning("Occlusion: !supportsComputeShaders");
				this.isEnabled = false;
			}
		}
		if (this.isEnabled && !this.depthTestMat)
		{
			this.depthTestMat = Resources.Load<Material>("Occlusion/OcclusionDepthTest");
			if (this.depthTestMat == null)
			{
				Log.Error("Occlusion: Missing OcclusionDepthTest mat");
				this.isEnabled = false;
			}
		}
		if (!this.isEnabled)
		{
			base.gameObject.SetActive(false);
			Log.Out("Occlusion: Disabled");
		}
		else
		{
			base.gameObject.SetActive(true);
			Log.Out("Occlusion: Enabled");
		}
		this.gpuCullingEnabled = this.isEnabled;
		if (!this.isEnabled)
		{
			this.ClearCullingTypes();
			return;
		}
		this.cullChunkEntities = true;
		this.cullDecorations = true;
		if (this.cullEverything)
		{
			this.cullChunkEntities = true;
			this.cullEntities = true;
			this.cullDistantChunks = true;
			this.cullDistantTerrain = true;
			this.cullDecorations = true;
			this.cullPrefabs = true;
		}
		this.hugeErrorCount = 0;
	}

	public void AddChunkTransforms(Chunk chunk, List<Transform> transforms)
	{
		OcclusionManager.OccludeeZone occludeeZone = chunk.occludeeZone;
		if (occludeeZone == null)
		{
			occludeeZone = new OcclusionManager.OccludeeZone();
			occludeeZone.extentsTotalMax = 24f;
			chunk.occludeeZone = occludeeZone;
		}
		for (int i = transforms.Count - 1; i >= 0; i--)
		{
			Transform transform = transforms[i];
			if (transform)
			{
				occludeeZone.AddTransform(transform);
			}
		}
		OcclusionManager.tempRenderers.Clear();
		this.UpdateZoneRegistration(occludeeZone);
	}

	public void RemoveChunkTransforms(Chunk chunk, List<Transform> transforms)
	{
		OcclusionManager.OccludeeZone occludeeZone = chunk.occludeeZone;
		if (occludeeZone == null)
		{
			return;
		}
		for (int i = transforms.Count - 1; i >= 0; i--)
		{
			Transform t = transforms[i];
			occludeeZone.RemoveTransform(t);
		}
		this.UpdateZoneRegistration(occludeeZone);
	}

	public void RemoveChunk(Chunk chunk)
	{
		OcclusionManager.OccludeeZone occludeeZone = chunk.occludeeZone;
		if (occludeeZone == null)
		{
			return;
		}
		this.RemoveFullZone(occludeeZone);
		chunk.occludeeZone = null;
	}

	public void RemoveFullZone(OcclusionManager.OccludeeZone zone)
	{
		for (int i = zone.layers.Length - 1; i >= 0; i--)
		{
			OcclusionManager.OccludeeLayer occludeeLayer = zone.layers[i];
			if (occludeeLayer != null && occludeeLayer.node != null)
			{
				this.UnregisterOccludee(occludeeLayer.node);
				occludeeLayer.node = null;
			}
		}
	}

	public void AddDeco(DecoChunk chunk, List<Transform> addTs)
	{
		OcclusionManager.OccludeeZone occludeeZone = chunk.occludeeZone;
		if (occludeeZone == null)
		{
			occludeeZone = new OcclusionManager.OccludeeZone();
			occludeeZone.extentsTotalMax = 189.44f;
			chunk.occludeeZone = occludeeZone;
		}
		if (!this.pendingZones.Contains(occludeeZone))
		{
			this.pendingZones.Add(occludeeZone);
		}
		occludeeZone.addTs.AddRange(addTs);
	}

	public void RemoveDeco(DecoChunk chunk, Transform removeT)
	{
		OcclusionManager.OccludeeZone occludeeZone = chunk.occludeeZone;
		if (occludeeZone == null)
		{
			Log.Error("RemoveDeco !zone");
			return;
		}
		if (!this.pendingZones.Contains(occludeeZone))
		{
			this.pendingZones.Add(occludeeZone);
		}
		occludeeZone.addTs.Remove(removeT);
		occludeeZone.removeTs.Add(removeT);
	}

	public void RemoveDecoChunk(DecoChunk chunk)
	{
		OcclusionManager.OccludeeZone occludeeZone = chunk.occludeeZone;
		if (occludeeZone == null)
		{
			return;
		}
		this.RemoveFullZone(occludeeZone);
		chunk.occludeeZone = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateZones()
	{
		if (this.pendingZones.Count > 0)
		{
			for (int i = this.pendingZones.Count - 1; i >= 0; i--)
			{
				OcclusionManager.OccludeeZone zone = this.pendingZones[i];
				this.UpdateZonePending(zone);
			}
			this.pendingZones.Clear();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateZonePending(OcclusionManager.OccludeeZone zone)
	{
		for (int i = zone.removeTs.Count - 1; i >= 0; i--)
		{
			Transform t = zone.removeTs[i];
			zone.RemoveTransform(t);
		}
		zone.removeTs.Clear();
		for (int j = zone.addTs.Count - 1; j >= 0; j--)
		{
			Transform transform = zone.addTs[j];
			if (transform)
			{
				zone.AddTransform(transform);
			}
		}
		zone.addTs.Clear();
		this.UpdateZoneRegistration(zone);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateZoneRegistration(OcclusionManager.OccludeeZone zone)
	{
		for (int i = zone.layers.Length - 1; i >= 0; i--)
		{
			OcclusionManager.OccludeeLayer occludeeLayer = zone.layers[i];
			if (occludeeLayer != null && occludeeLayer.isOld)
			{
				occludeeLayer.isOld = false;
				if (occludeeLayer.node != null)
				{
					this.UnregisterOccludee(occludeeLayer.node);
					occludeeLayer.node = null;
				}
				if (occludeeLayer.renderers.Count > 0)
				{
					List<Renderer> list = new List<Renderer>();
					foreach (KeyValuePair<int, OcclusionManager.OccludeeRenderers> keyValuePair in occludeeLayer.renderers)
					{
						list.AddRange(keyValuePair.Value.renderers);
					}
					occludeeLayer.node = this.RegisterOccludee(list.ToArray(), zone.extentsTotalMax);
				}
			}
		}
	}

	public static void AddEntity(EntityAlive _ea, float extentsTotalMax = 32f)
	{
		OcclusionManager instance = OcclusionManager.Instance;
		if (instance != null)
		{
			_ea.GetComponentsInChildren<Renderer>(true, OcclusionManager.tempRenderers);
			for (int i = OcclusionManager.tempRenderers.Count - 1; i >= 0; i--)
			{
				if (OcclusionManager.tempRenderers[i] is ParticleSystemRenderer)
				{
					OcclusionManager.tempRenderers.RemoveAt(i);
				}
				else if (OcclusionManager.tempRenderers[i].gameObject.CompareTag("NoOcclude"))
				{
					OcclusionManager.tempRenderers.RemoveAt(i);
				}
			}
			if (OcclusionManager.tempRenderers.Count > 0)
			{
				OcclusionManager.OccludeeEntity occludeeEntity = new OcclusionManager.OccludeeEntity();
				occludeeEntity.entity = _ea;
				occludeeEntity.pos = _ea.position;
				occludeeEntity.entry = instance.RegisterOccludee(OcclusionManager.tempRenderers.ToArray(), extentsTotalMax);
				instance.entities[_ea.entityId] = occludeeEntity;
				OcclusionManager.tempRenderers.Clear();
			}
		}
	}

	public static void RemoveEntity(EntityAlive _ea)
	{
		OcclusionManager instance = OcclusionManager.Instance;
		if (instance != null)
		{
			OcclusionManager.OccludeeEntity occludeeEntity;
			if (!instance.entities.TryGetValue(_ea.entityId, out occludeeEntity))
			{
				Log.Warning("Occlusion: RemoveEntity {0} missing", new object[]
				{
					_ea
				});
				return;
			}
			instance.entities.Remove(_ea.entityId);
			if (occludeeEntity.entry != null)
			{
				instance.UnregisterOccludee(occludeeEntity.entry);
				occludeeEntity.entry = null;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateEntities()
	{
		foreach (KeyValuePair<int, OcclusionManager.OccludeeEntity> keyValuePair in this.entities)
		{
			OcclusionManager.OccludeeEntity value = keyValuePair.Value;
			if ((value.pos - value.entity.position).sqrMagnitude > 1f)
			{
				value.pos = value.entity.position;
				value.entry.Value.isAreaFound = false;
			}
		}
	}

	public LinkedListNode<OcclusionManager.OcclusionEntry> RegisterOccludee(Renderer[] renderers, float extentsTotalMax = 32f)
	{
		if (!this.isEnabled)
		{
			return null;
		}
		LinkedListNode<OcclusionManager.OcclusionEntry> first = this.freeEntries.First;
		if (first != null)
		{
			this.freeEntries.RemoveFirst();
			this.usedEntries.AddFirst(first);
			OcclusionManager.OcclusionEntry value = first.Value;
			value.allRenderers = renderers;
			float num = extentsTotalMax * 0.55f;
			value.cullStartDistSq = num * num;
			value.extentsTotalMax = extentsTotalMax;
			value.centerPos.y = -9999f;
			value.isAreaFound = false;
			value.isForceOn = true;
			value.isVisible = true;
			this.totalEntryCount++;
			return first;
		}
		if (Time.frameCount != this.errorFrame)
		{
			this.errorFrame = Time.frameCount;
			Log.Warning("Occlusion used all entries");
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CalcArea(OcclusionManager.OcclusionEntry entry)
	{
		if (entry.renderItems == null)
		{
			OcclusionManager.RenderItem[] array = new OcclusionManager.RenderItem[entry.allRenderers.Length];
			entry.renderItems = array;
			int num = 0;
			for (int i = entry.allRenderers.Length - 1; i >= 0; i--)
			{
				Renderer renderer = entry.allRenderers[i];
				if (renderer && !renderer.forceRenderingOff)
				{
					ShadowCastingMode shadowCastingMode = renderer.shadowCastingMode;
					if (shadowCastingMode != ShadowCastingMode.ShadowsOnly)
					{
						Vector3 extents = renderer.bounds.extents;
						if (extents.x <= 29f && extents.y <= 35f && extents.z <= 29f)
						{
							OcclusionManager.RenderItem renderItem;
							renderItem.renderer = renderer;
							renderItem.shadowMode = shadowCastingMode;
							array[num] = renderItem;
							num++;
						}
					}
				}
			}
			entry.renderItemsUsed = num;
		}
		Bounds bounds = default(Bounds);
		bool flag = false;
		for (int j = entry.renderItemsUsed - 1; j >= 0; j--)
		{
			Renderer renderer2 = entry.renderItems[j].renderer;
			if (renderer2)
			{
				Bounds bounds2 = renderer2.bounds;
				if (bounds2.extents.sqrMagnitude > 0.001f)
				{
					if (!flag)
					{
						bounds = bounds2;
						flag = true;
					}
					else
					{
						bounds.Encapsulate(bounds2);
						if (bounds.extents.x > entry.extentsTotalMax || bounds.extents.z > entry.extentsTotalMax)
						{
							this.hugeErrorCount++;
							return;
						}
					}
				}
			}
		}
		entry.centerPos = bounds.center;
		Vector3 extents2 = bounds.extents;
		if (extents2.x < 2f)
		{
			extents2.x = 2f;
		}
		if (extents2.y < 2f)
		{
			extents2.y = 2f;
		}
		if (extents2.z < 2f)
		{
			extents2.z = 2f;
		}
		entry.size = extents2 * 4f;
		this.areaMatrix.m03 = entry.centerPos.x;
		this.areaMatrix.m13 = entry.centerPos.y;
		this.areaMatrix.m23 = entry.centerPos.z;
		this.areaMatrix.m00 = entry.size.x;
		this.areaMatrix.m11 = entry.size.y;
		this.areaMatrix.m22 = entry.size.z;
		this.objectMatrixLists[entry.matrixUnitIndex][entry.matrixSubIndex] = this.areaMatrix;
		entry.isAreaFound = true;
	}

	public void UnregisterOccludee(LinkedListNode<OcclusionManager.OcclusionEntry> node)
	{
		if (node != null)
		{
			OcclusionManager.OcclusionEntry value = node.Value;
			this.usedEntries.Remove(node);
			this.freeEntries.AddFirst(node);
			value.allRenderers = null;
			if (value.renderItems != null)
			{
				for (int i = value.renderItemsUsed - 1; i >= 0; i--)
				{
					OcclusionManager.RenderItem renderItem = value.renderItems[i];
					if (renderItem.renderer)
					{
						renderItem.renderer.forceRenderingOff = false;
						renderItem.renderer.shadowCastingMode = renderItem.shadowMode;
					}
				}
				value.renderItems = null;
			}
			this.objectMatrixLists[value.matrixUnitIndex][value.matrixSubIndex] = this.tinyMatrix;
			this.totalEntryCount--;
		}
	}

	public void OriginChanged(Vector3 offset)
	{
		for (LinkedListNode<OcclusionManager.OcclusionEntry> linkedListNode = this.usedEntries.First; linkedListNode != null; linkedListNode = linkedListNode.Next)
		{
			OcclusionManager.OcclusionEntry value = linkedListNode.Value;
			value.centerPos += offset;
			this.areaMatrix.m03 = value.centerPos.x;
			this.areaMatrix.m13 = value.centerPos.y;
			this.areaMatrix.m23 = value.centerPos.z;
			this.areaMatrix.m00 = value.size.x;
			this.areaMatrix.m11 = value.size.y;
			this.areaMatrix.m22 = value.size.z;
			this.objectMatrixLists[value.matrixUnitIndex][value.matrixSubIndex] = this.areaMatrix;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		OcclusionManager.Instance = this;
		this.onRequestDelegate = new Action<AsyncGPUReadbackRequest>(this.OnRequest);
		this.tinyMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(0.0001f, 0.0001f, 0.0001f));
		this.cubeObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
		this.cubeMesh = this.cubeObj.GetComponent<MeshFilter>().sharedMesh;
		this.cubeObj.SetActive(false);
		this.cubeObj.transform.SetParent(base.gameObject.transform);
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < 4088; i++)
		{
			this.entries.Add(new OcclusionManager.OcclusionEntry());
			OcclusionManager.OcclusionEntry occlusionEntry = this.entries[i];
			if (num2 == 511)
			{
				num++;
				num2 = 0;
			}
			occlusionEntry.index = i;
			occlusionEntry.matrixUnitIndex = num;
			occlusionEntry.matrixSubIndex = num2++;
			this.freeEntries.AddLast(occlusionEntry);
		}
		int num3 = 128;
		this.initialData = new uint[num3];
		this.visibleData = new uint[num3];
		for (int j = 0; j <= num; j++)
		{
			this.objectMatrixLists.Add(new Matrix4x4[511]);
		}
		for (int k = 0; k < this.initialData.Length; k++)
		{
			this.initialData[k] = 0U;
		}
		for (int l = 0; l < this.counterBuffer.Length; l++)
		{
			ComputeBuffer computeBuffer = new ComputeBuffer(num3, 4, ComputeBufferType.Default);
			computeBuffer.SetData(this.initialData);
			this.counterBuffer[l] = computeBuffer;
		}
		this.materialBlocks = new MaterialPropertyBlock[this.objectMatrixLists.Count];
		int num4 = 0;
		for (int m = 0; m < this.objectMatrixLists.Count; m++)
		{
			this.materialBlocks[m] = new MaterialPropertyBlock();
			this.materialBlocks[m].SetInt("_InstanceOffset", num4);
			num4 += this.objectMatrixLists[m].Length;
		}
		this.depthCamera = base.GetComponent<Camera>();
		this.depthCamera.enabled = false;
		GameOptionsManager.ResolutionChanged += this.OnResolutionChanged;
		this.CreateDepthRT();
		Log.Out("Occlusion: Awake");
	}

	public void SetSourceDepthCamera(Camera _camera)
	{
		if (this.depthCopyCmdBuf != null)
		{
			if (this.sourceDepthCamera != null)
			{
				this.sourceDepthCamera.RemoveCommandBuffer(CameraEvent.AfterForwardOpaque, this.depthCopyCmdBuf);
			}
			_camera.AddCommandBuffer(CameraEvent.AfterForwardOpaque, this.depthCopyCmdBuf);
		}
		this.sourceDepthCamera = _camera;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ClearCullingTypes()
	{
		this.cullEverything = false;
		this.cullChunkEntities = false;
		this.cullEntities = false;
		this.cullDistantChunks = false;
		this.cullDistantTerrain = false;
		this.cullDecorations = false;
		this.cullPrefabs = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnResolutionChanged(int _width, int _height)
	{
		this.CreateDepthRT();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CreateDepthRT()
	{
		float num = (float)Screen.width / (float)Screen.height;
		if (this.depthRT)
		{
			this.depthRT.Release();
			this.depthRT = null;
		}
		this.depthRT = new RenderTexture(256, (int)(256f / num), 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
		this.depthRT.name = "Occlusion";
		this.depthRT.Create();
		this.depthCamera.targetTexture = this.depthRT;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateVisibility(uint[] data)
	{
		this.visibleEntryCount = 0;
		this.hasVisibilityData = false;
		for (LinkedListNode<OcclusionManager.OcclusionEntry> linkedListNode = this.usedEntries.First; linkedListNode != null; linkedListNode = linkedListNode.Next)
		{
			OcclusionManager.OcclusionEntry value = linkedListNode.Value;
			uint num = 0U;
			uint num2 = (uint)value.index >> 5;
			int num3 = value.index & 31;
			if (((ulong)data[(int)num2] & (ulong)(1L << (num3 & 31))) != 0UL)
			{
				num = 1U;
			}
			if (num > 0U || value.isForceOn)
			{
				if (!value.isVisible)
				{
					value.isVisible = true;
					for (int i = value.renderItemsUsed - 1; i >= 0; i--)
					{
						OcclusionManager.RenderItem renderItem = value.renderItems[i];
						if (renderItem.renderer)
						{
							if (renderItem.shadowMode != ShadowCastingMode.Off)
							{
								renderItem.renderer.shadowCastingMode = renderItem.shadowMode;
							}
							else
							{
								renderItem.renderer.forceRenderingOff = false;
							}
						}
					}
				}
				this.visibleEntryCount++;
			}
			else if (value.isVisible)
			{
				value.isVisible = false;
				for (int j = value.renderItemsUsed - 1; j >= 0; j--)
				{
					OcclusionManager.RenderItem renderItem2 = value.renderItems[j];
					if (renderItem2.renderer)
					{
						if (renderItem2.shadowMode != ShadowCastingMode.Off)
						{
							renderItem2.renderer.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
						}
						else
						{
							renderItem2.renderer.forceRenderingOff = true;
						}
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		if (this.sourceDepthCamera != null && this.depthCopyCmdBuf != null)
		{
			this.sourceDepthCamera.AddCommandBuffer(CameraEvent.AfterForwardOpaque, this.depthCopyCmdBuf);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDisable()
	{
		if (this.sourceDepthCamera != null && this.depthCopyCmdBuf != null)
		{
			this.sourceDepthCamera.RemoveCommandBuffer(CameraEvent.AfterForwardOpaque, this.depthCopyCmdBuf);
		}
		this.SetRenderersEnabled(true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDestroy()
	{
		this.isEnabled = false;
		this.gpuCullingEnabled = false;
		GameOptionsManager.ResolutionChanged -= this.OnResolutionChanged;
		if (this.depthRT)
		{
			this.depthRT.Release();
			this.depthRT = null;
		}
		if (this.depthCopyRT)
		{
			this.depthCopyRT.Release();
			this.depthCopyRT = null;
		}
		for (int i = 0; i < this.counterBuffer.Length; i++)
		{
			if (this.counterBuffer[i] != null)
			{
				this.counterBuffer[i].Release();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RenderOccludees(Camera renderCamera, int layer)
	{
		Vector3 position = this.sourceDepthCamera.transform.position;
		for (LinkedListNode<OcclusionManager.OcclusionEntry> linkedListNode = this.usedEntries.First; linkedListNode != null; linkedListNode = linkedListNode.Next)
		{
			OcclusionManager.OcclusionEntry value = linkedListNode.Value;
			if ((position - value.centerPos).sqrMagnitude < value.cullStartDistSq)
			{
				value.isForceOn = true;
			}
			else if (!value.isAreaFound)
			{
				this.CalcArea(value);
			}
			else
			{
				value.isForceOn = false;
			}
		}
		Graphics.SetRandomWriteTarget(1, this.counterBuffer[this.counterBufferCurrentIndex]);
		for (int i = 0; i < this.objectMatrixLists.Count; i++)
		{
			Matrix4x4[] array = this.objectMatrixLists[i];
			Graphics.DrawMeshInstanced(this.cubeMesh, 0, this.depthTestMat, array, array.Length, this.materialBlocks[i], ShadowCastingMode.Off, false, layer, renderCamera);
		}
	}

	public void LocalPlayerOnPreCull()
	{
		if (!this.gpuCullingEnabled)
		{
			return;
		}
		if (this.forceAllVisible || this.forceAllHidden)
		{
			this.SetRenderersEnabled(this.forceAllVisible);
			this.visibleEntryCount = this.totalEntryCount;
			return;
		}
		if (this.isCameraChanged)
		{
			return;
		}
		Vector3 forward = Camera.current.transform.forward;
		if (Vector3.Dot(this.camDirVec, forward) < 0.94f)
		{
			this.SetRenderersEnabled(true);
			return;
		}
		if (this.hasVisibilityData)
		{
			this.UpdateVisibility(this.visibleData);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnRenderObject()
	{
		if (this.isOcclusionChecking || !this.gpuCullingEnabled)
		{
			return;
		}
		if (GameManager.Instance.World == null)
		{
			return;
		}
		if (Camera.current == this.sourceDepthCamera)
		{
			this.UpdateEntities();
			this.UpdateZones();
			this.isOcclusionChecking = true;
			this.isCameraChanged = false;
			Transform transform = this.sourceDepthCamera.transform;
			this.camDirVec = transform.forward;
			this.depthCamera.transform.position = transform.position;
			this.depthCamera.transform.rotation = transform.rotation;
			this.depthCamera.fieldOfView = this.sourceDepthCamera.fieldOfView;
			this.depthCamera.nearClipPlane = this.sourceDepthCamera.nearClipPlane;
			this.depthCamera.farClipPlane = this.sourceDepthCamera.farClipPlane;
			if (this.depthCopyCmdBuf != null)
			{
				Graphics.Blit(this.depthCopyRT, this.depthRT, this.depthCopyMat, 1);
			}
			else
			{
				Graphics.Blit(null, this.depthRT, this.depthCopyMat, 2);
			}
			this.depthCamera.Render();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPreCull()
	{
		this.counterBuffer[this.counterBufferCurrentIndex].SetData(this.initialData);
		this.RenderOccludees(this.depthCamera, 11);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPostRender()
	{
		Graphics.ClearRandomWriteTargets();
		AsyncGPUReadback.Request(this.counterBuffer[this.counterBufferCurrentIndex], this.onRequestDelegate);
		this.counterBufferCurrentIndex = (this.counterBufferCurrentIndex + 1) % this.counterBuffer.Length;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnRequest(AsyncGPUReadbackRequest req)
	{
		this.isOcclusionChecking = false;
		if (this.isCameraChanged)
		{
			return;
		}
		req.GetData<uint>(0).CopyTo(this.visibleData);
		this.hasVisibilityData = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetRenderersEnabled(bool isEnabled)
	{
		this.isCameraChanged = true;
		this.hasVisibilityData = false;
		for (LinkedListNode<OcclusionManager.OcclusionEntry> linkedListNode = this.usedEntries.First; linkedListNode != null; linkedListNode = linkedListNode.Next)
		{
			OcclusionManager.OcclusionEntry value = linkedListNode.Value;
			if (value.isVisible != isEnabled && value.renderItems != null)
			{
				value.isVisible = isEnabled;
				for (int i = value.renderItemsUsed - 1; i >= 0; i--)
				{
					OcclusionManager.RenderItem renderItem = value.renderItems[i];
					if (renderItem.renderer)
					{
						if (renderItem.shadowMode != ShadowCastingMode.Off)
						{
							renderItem.renderer.shadowCastingMode = (isEnabled ? renderItem.shadowMode : ShadowCastingMode.ShadowsOnly);
						}
						else
						{
							renderItem.renderer.forceRenderingOff = !isEnabled;
						}
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void LogOcclusion(string format, params object[] args)
	{
		format = string.Format("{0} OC {1}", GameManager.frameCount, format);
		Log.Warning(format, args);
	}

	public void ToggleDebugView()
	{
		this.isDebugView = !this.isDebugView;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnGUI()
	{
		if (this.isDebugView && this.depthRT)
		{
			GUI.DrawTexture(new Rect(0f, 0f, 256f, (float)(256 * this.depthRT.height / this.depthRT.width)), this.depthRT);
			string text = string.Format("{0} of {1}, huge {2}", this.visibleEntryCount, this.usedEntries.Count, this.hugeErrorCount);
			GUI.color = Color.black;
			GUI.Label(new Rect(1f, 1f, 256f, 256f), text);
			GUI.color = Color.white;
			GUI.Label(new Rect(0f, 0f, 256f, 256f), text);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int cDepthRTWidth = 256;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cBoundsScale = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cCameraEnableAllAngleCos = 0.94f;

	public static OcclusionManager Instance;

	public bool isEnabled;

	public Material depthTestMat;

	public int visibleEntryCount;

	public int totalEntryCount;

	public bool forceAllVisible;

	public bool forceAllHidden;

	public bool cullEverything;

	public bool cullChunkEntities = true;

	public bool cullDecorations = true;

	public bool cullEntities;

	public bool cullDistantChunks;

	public bool cullDistantTerrain;

	public bool cullPrefabs;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int cMaxUnits = 511;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int cMaxEntries = 4088;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Matrix4x4 tinyMatrix;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public ComputeBuffer[] counterBuffer = new ComputeBuffer[3];

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int counterBufferCurrentIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public uint[] initialData;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool hasVisibilityData;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public uint[] visibleData;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Camera sourceDepthCamera;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public CommandBuffer depthCopyCmdBuf;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<OcclusionManager.OcclusionEntry> entries = new List<OcclusionManager.OcclusionEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public LinkedList<OcclusionManager.OcclusionEntry> freeEntries = new LinkedList<OcclusionManager.OcclusionEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public LinkedList<OcclusionManager.OcclusionEntry> usedEntries = new LinkedList<OcclusionManager.OcclusionEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<Matrix4x4[]> objectMatrixLists = new List<Matrix4x4[]>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public MaterialPropertyBlock[] materialBlocks;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public GameObject cubeObj;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Mesh cubeMesh;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int errorFrame;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool gpuCullingEnabled;

	public Camera depthCamera;

	public Material depthCopyMat;

	public RenderTexture depthRT;

	public RenderTexture depthCopyRT;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isOcclusionChecking;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isCameraChanged;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Matrix4x4 areaMatrix = Matrix4x4.identity;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int hugeErrorCount;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public readonly List<OcclusionManager.OccludeeZone> pendingZones = new List<OcclusionManager.OccludeeZone>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static readonly List<Renderer> tempRenderers = new List<Renderer>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Dictionary<int, OcclusionManager.OccludeeEntity> entities = new Dictionary<int, OcclusionManager.OccludeeEntity>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 camDirVec;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Action<AsyncGPUReadbackRequest> onRequestDelegate;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isDebugView;

	public struct RenderItem
	{
		public Renderer renderer;

		public ShadowCastingMode shadowMode;
	}

	public class OcclusionEntry
	{
		public Renderer[] allRenderers;

		public OcclusionManager.RenderItem[] renderItems;

		public int renderItemsUsed;

		public float cullStartDistSq;

		public float extentsTotalMax;

		public int index;

		public int matrixUnitIndex;

		public int matrixSubIndex;

		public bool isAreaFound;

		public Vector3 centerPos;

		public Vector3 size;

		public bool isForceOn;

		public bool isVisible;
	}

	public class OccludeeZone
	{
		public int GetIndex(float y)
		{
			int num = (int)y >> 3;
			if (num < 32)
			{
				return num;
			}
			if (num <= 0)
			{
				return 0;
			}
			return 31;
		}

		public void AddTransform(Transform t)
		{
			int index = this.GetIndex(t.position.y + Origin.position.y);
			OcclusionManager.OccludeeLayer occludeeLayer = this.layers[index];
			if (occludeeLayer == null)
			{
				occludeeLayer = new OcclusionManager.OccludeeLayer();
				this.layers[index] = occludeeLayer;
			}
			occludeeLayer.isOld = true;
			OcclusionManager.OccludeeRenderers occludeeRenderers = occludeeLayer.AddTransform(t);
			t.GetComponentsInChildren<Renderer>(true, OcclusionManager.tempRenderers);
			if (OcclusionManager.tempRenderers.Count == 0)
			{
				OcclusionManager.LogOcclusion("AddTransform {0} tempRenderers 0", new object[]
				{
					t.name
				});
			}
			foreach (Renderer renderer in OcclusionManager.tempRenderers)
			{
				if (!(renderer is ParticleSystemRenderer) && renderer.shadowCastingMode != ShadowCastingMode.ShadowsOnly && !renderer.CompareTag("NoOcclude"))
				{
					occludeeRenderers.renderers.Add(renderer);
				}
			}
			OcclusionManager.tempRenderers.Clear();
		}

		public void RemoveTransform(Transform t)
		{
			int hashCode = t.GetHashCode();
			if (t)
			{
				int index = this.GetIndex(t.position.y + Origin.position.y);
				OcclusionManager.OccludeeLayer occludeeLayer = this.layers[index];
				if (occludeeLayer != null && occludeeLayer.renderers.Remove(hashCode))
				{
					occludeeLayer.isOld = true;
					return;
				}
			}
			for (int i = 0; i < this.layers.Length; i++)
			{
				OcclusionManager.OccludeeLayer occludeeLayer2 = this.layers[i];
				if (occludeeLayer2 != null && occludeeLayer2.renderers.Remove(hashCode))
				{
					occludeeLayer2.isOld = true;
					return;
				}
			}
		}

		public float extentsTotalMax;

		public OcclusionManager.OccludeeLayer[] layers = new OcclusionManager.OccludeeLayer[32];

		public List<Transform> addTs = new List<Transform>();

		public List<Transform> removeTs = new List<Transform>();

		[PublicizedFrom(EAccessModifier.Private)]
		public const int cLayerShift = 3;

		[PublicizedFrom(EAccessModifier.Private)]
		public const int cLayerH = 8;

		[PublicizedFrom(EAccessModifier.Private)]
		public const int cLayerCount = 32;
	}

	public class OccludeeLayer
	{
		public OcclusionManager.OccludeeRenderers AddTransform(Transform t)
		{
			int hashCode = t.GetHashCode();
			OcclusionManager.OccludeeRenderers occludeeRenderers;
			if (this.renderers.TryGetValue(hashCode, out occludeeRenderers))
			{
				Log.Warning("OccludeeLayer AddTransform {0} {1} exists", new object[]
				{
					t ? t.name : "",
					hashCode
				});
				return occludeeRenderers;
			}
			occludeeRenderers = new OcclusionManager.OccludeeRenderers();
			this.renderers.Add(hashCode, occludeeRenderers);
			return occludeeRenderers;
		}

		public Dictionary<int, OcclusionManager.OccludeeRenderers> renderers = new Dictionary<int, OcclusionManager.OccludeeRenderers>();

		public LinkedListNode<OcclusionManager.OcclusionEntry> node;

		public bool isOld;
	}

	public class OccludeeRenderers
	{
		public List<Renderer> renderers = new List<Renderer>();
	}

	public class OccludeeEntity
	{
		public Entity entity;

		public Vector3 pos;

		public LinkedListNode<OcclusionManager.OcclusionEntry> entry;
	}
}

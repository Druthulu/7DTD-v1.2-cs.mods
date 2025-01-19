using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PrefabEditModeManager
{
	public event Action<PrefabInstance> OnPrefabChanged;

	public PrefabEditModeManager()
	{
		PrefabEditModeManager.Instance = this;
	}

	public void Init()
	{
		this.ReloadAllXmls();
		this.InitXmlWatcher();
		if (this.IsActive())
		{
			SelectionBoxManager.Instance.GetCategory("DynamicPrefabs").SetVisible(false);
		}
		this.NeedsSaving = false;
		GameManager.Instance.World.ChunkClusters[0].OnBlockChangedDelegates += this.blockChangeDelegate;
	}

	public bool IsActive()
	{
		return GameManager.Instance.IsEditMode() && GamePrefs.GetString(EnumGamePrefs.GameWorld) == "Empty";
	}

	public void LoadRecentlyUsedOrCreateNew()
	{
		if (this.VoxelPrefab != null)
		{
			return;
		}
		string @string = GamePrefs.GetString(EnumGamePrefs.LastLoadedPrefab);
		PathAbstractions.AbstractedLocation abstractedLocation = PathAbstractions.AbstractedLocation.None;
		if (!string.IsNullOrEmpty(@string))
		{
			abstractedLocation = PathAbstractions.PrefabsSearchPaths.GetLocation(@string, null, null);
		}
		if (abstractedLocation.Exists())
		{
			ThreadManager.StartCoroutine(this.loadLastUsedPrefabLater());
			return;
		}
		this.NewVoxelPrefab();
	}

	public void LoadRecentlyUsed()
	{
		string @string = GamePrefs.GetString(EnumGamePrefs.LastLoadedPrefab);
		if (!string.IsNullOrEmpty(@string) && (this.VoxelPrefab == null || @string != this.VoxelPrefab.PrefabName))
		{
			ThreadManager.StartCoroutine(this.loadLastUsedPrefabLater());
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator loadLastUsedPrefabLater()
	{
		yield return new WaitForSeconds(1f);
		ChunkCluster cc = GameManager.Instance.World.ChunkCache;
		List<Chunk> chunkArrayCopySync = cc.GetChunkArrayCopySync();
		foreach (Chunk c in chunkArrayCopySync)
		{
			if (!cc.IsOnBorder(c))
			{
				if (!c.IsEmpty())
				{
					while (c.NeedsRegeneration || c.NeedsCopying)
					{
						yield return new WaitForSeconds(1f);
					}
					c = null;
				}
			}
		}
		List<Chunk>.Enumerator enumerator = default(List<Chunk>.Enumerator);
		PathAbstractions.AbstractedLocation location = PathAbstractions.PrefabsSearchPaths.GetLocation(GamePrefs.GetString(EnumGamePrefs.LastLoadedPrefab), null, null);
		this.LoadVoxelPrefab(location, false, false);
		yield break;
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitXmlWatcher()
	{
		string gameDir = GameIO.GetGameDir("Data/Prefabs");
		Log.Out("Watching prefabs folder for XML changes: " + gameDir);
		this.xmlWatcher = new FileSystemWatcher(gameDir, "*.xml");
		this.xmlWatcher.IncludeSubdirectories = true;
		this.xmlWatcher.Changed += this.OnXmlFileChanged;
		this.xmlWatcher.Created += this.OnXmlFileChanged;
		this.xmlWatcher.Deleted += this.OnXmlFileChanged;
		this.xmlWatcher.EnableRaisingEvents = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnXmlFileChanged(object _sender, FileSystemEventArgs _e)
	{
		Log.Out(string.Format("Prefab XML {0}: {1}", _e.ChangeType, _e.Name));
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(_e.Name);
		PathAbstractions.AbstractedLocation abstractedLocation = new PathAbstractions.AbstractedLocation(PathAbstractions.EAbstractedLocationType.GameData, fileNameWithoutExtension, Path.ChangeExtension(_e.FullPath, ".tts"), null, false, null);
		if (_e.ChangeType == WatcherChangeTypes.Deleted)
		{
			Dictionary<PathAbstractions.AbstractedLocation, Prefab> obj = this.loadedPrefabHeaders;
			lock (obj)
			{
				this.loadedPrefabHeaders.Remove(abstractedLocation);
				return;
			}
		}
		this.LoadXml(abstractedLocation);
		if (this.VoxelPrefab != null && this.VoxelPrefab.location == abstractedLocation)
		{
			Log.Out("Applying XML changes to loaded prefab");
			this.VoxelPrefab.LoadXMLData(this.VoxelPrefab.location);
			return;
		}
		if (this.VoxelPrefab != null)
		{
			Log.Out(string.Format("XML changed not related to loaded prefab. (Loaded: {0}, FP {1}; Changed: {2}, FP {3})", new object[]
			{
				this.VoxelPrefab.location,
				this.VoxelPrefab.location.FullPath,
				abstractedLocation,
				abstractedLocation.FullPath
			}));
		}
	}

	public void ReloadAllXmls()
	{
		Dictionary<PathAbstractions.AbstractedLocation, Prefab> obj = this.loadedPrefabHeaders;
		lock (obj)
		{
			this.loadedPrefabHeaders.Clear();
			foreach (PathAbstractions.AbstractedLocation location in PathAbstractions.PrefabsSearchPaths.GetAvailablePathsList(null, null, null, false))
			{
				this.LoadXml(location);
			}
		}
	}

	public void LoadXml(PathAbstractions.AbstractedLocation _location)
	{
		Prefab prefab = new Prefab();
		prefab.LoadXMLData(_location);
		Dictionary<PathAbstractions.AbstractedLocation, Prefab> obj = this.loadedPrefabHeaders;
		lock (obj)
		{
			this.loadedPrefabHeaders[_location] = prefab;
		}
	}

	public void Cleanup()
	{
		if (this.xmlWatcher != null)
		{
			this.xmlWatcher.EnableRaisingEvents = false;
			this.xmlWatcher = null;
		}
		Dictionary<PathAbstractions.AbstractedLocation, Prefab> obj = this.loadedPrefabHeaders;
		lock (obj)
		{
			this.loadedPrefabHeaders.Clear();
		}
		if (this.groundGrid)
		{
			UnityEngine.Object.Destroy(this.groundGrid);
			this.groundGrid = null;
		}
		if (this.prefabInstanceId != -1)
		{
			DynamicPrefabDecorator dynamicPrefabDecorator = GameManager.Instance.GetDynamicPrefabDecorator();
			dynamicPrefabDecorator.RemovePrefabAndSelection(GameManager.Instance.World, dynamicPrefabDecorator.GetPrefab(this.prefabInstanceId), false);
			this.prefabInstanceId = -1;
		}
		GameManager.Instance.World.ChunkClusters[0].OnBlockChangedDelegates -= this.blockChangeDelegate;
		this.ClearImposterPrefab();
		this.ClearVoxelPrefab();
		this.OnPrefabChanged = null;
		this.HighlightBlocks(null);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void blockChangeDelegate(Vector3i pos, BlockValue bvOld, sbyte oldDens, long oldTex, BlockValue bvNew)
	{
		this.NeedsSaving = true;
	}

	public void FindPrefabs(string _group, List<PathAbstractions.AbstractedLocation> _result)
	{
		Dictionary<PathAbstractions.AbstractedLocation, Prefab> obj = this.loadedPrefabHeaders;
		lock (obj)
		{
			foreach (KeyValuePair<PathAbstractions.AbstractedLocation, Prefab> keyValuePair in this.loadedPrefabHeaders)
			{
				if (_group == null)
				{
					_result.Add(keyValuePair.Key);
				}
				else if (_group.Length == 0)
				{
					if (keyValuePair.Value.editorGroups == null || keyValuePair.Value.editorGroups.Count == 0)
					{
						_result.Add(keyValuePair.Key);
					}
				}
				else if (keyValuePair.Value.editorGroups != null)
				{
					for (int i = 0; i < keyValuePair.Value.editorGroups.Count; i++)
					{
						if (string.Compare(keyValuePair.Value.editorGroups[i], _group, StringComparison.OrdinalIgnoreCase) == 0)
						{
							_result.Add(keyValuePair.Key);
							break;
						}
					}
				}
			}
		}
	}

	public void GetAllTags(List<string> _result, Prefab _considerLoadedPrefab = null)
	{
		Dictionary<PathAbstractions.AbstractedLocation, Prefab> obj = this.loadedPrefabHeaders;
		lock (obj)
		{
			foreach (KeyValuePair<PathAbstractions.AbstractedLocation, Prefab> keyValuePair in this.loadedPrefabHeaders)
			{
				if (!keyValuePair.Value.Tags.IsEmpty)
				{
					foreach (string item in keyValuePair.Value.Tags.GetTagNames())
					{
						if (!_result.ContainsCaseInsensitive(item))
						{
							_result.Add(item);
						}
					}
				}
			}
		}
		if (_considerLoadedPrefab != null && !_considerLoadedPrefab.Tags.IsEmpty)
		{
			foreach (string item2 in _considerLoadedPrefab.Tags.GetTagNames())
			{
				if (!_result.ContainsCaseInsensitive(item2))
				{
					_result.Add(item2);
				}
			}
		}
	}

	public void GetAllThemeTags(List<string> _result, Prefab _considerLoadedPrefab = null)
	{
		Dictionary<PathAbstractions.AbstractedLocation, Prefab> obj = this.loadedPrefabHeaders;
		lock (obj)
		{
			foreach (KeyValuePair<PathAbstractions.AbstractedLocation, Prefab> keyValuePair in this.loadedPrefabHeaders)
			{
				if (!keyValuePair.Value.ThemeTags.IsEmpty)
				{
					foreach (string item in keyValuePair.Value.ThemeTags.GetTagNames())
					{
						if (!_result.ContainsCaseInsensitive(item))
						{
							_result.Add(item);
						}
					}
				}
			}
		}
		if (_considerLoadedPrefab != null && !_considerLoadedPrefab.ThemeTags.IsEmpty)
		{
			foreach (string item2 in _considerLoadedPrefab.ThemeTags.GetTagNames())
			{
				if (!_result.ContainsCaseInsensitive(item2))
				{
					_result.Add(item2);
				}
			}
		}
	}

	public void GetAllGroups(List<string> _result, Prefab _considerLoadedPrefab = null)
	{
		Dictionary<PathAbstractions.AbstractedLocation, Prefab> obj = this.loadedPrefabHeaders;
		lock (obj)
		{
			foreach (KeyValuePair<PathAbstractions.AbstractedLocation, Prefab> keyValuePair in this.loadedPrefabHeaders)
			{
				if (keyValuePair.Value.editorGroups != null)
				{
					foreach (string item in keyValuePair.Value.editorGroups)
					{
						if (!_result.ContainsCaseInsensitive(item))
						{
							_result.Add(item);
						}
					}
				}
			}
		}
		if (((_considerLoadedPrefab != null) ? _considerLoadedPrefab.editorGroups : null) != null)
		{
			foreach (string item2 in _considerLoadedPrefab.editorGroups)
			{
				if (!_result.ContainsCaseInsensitive(item2))
				{
					_result.Add(item2);
				}
			}
		}
	}

	public void GetAllZones(List<string> _result, Prefab _considerLoadedPrefab = null)
	{
		Dictionary<PathAbstractions.AbstractedLocation, Prefab> obj = this.loadedPrefabHeaders;
		lock (obj)
		{
			foreach (KeyValuePair<PathAbstractions.AbstractedLocation, Prefab> keyValuePair in this.loadedPrefabHeaders)
			{
				string[] allowedZones = keyValuePair.Value.GetAllowedZones();
				if (allowedZones != null)
				{
					foreach (string item in allowedZones)
					{
						if (!_result.ContainsCaseInsensitive(item))
						{
							_result.Add(item);
						}
					}
				}
			}
		}
		if (_considerLoadedPrefab != null)
		{
			string[] allowedZones2 = _considerLoadedPrefab.GetAllowedZones();
			if (allowedZones2 != null)
			{
				foreach (string item2 in allowedZones2)
				{
					if (!_result.ContainsCaseInsensitive(item2))
					{
						_result.Add(item2);
					}
				}
			}
		}
	}

	public void GetAllQuestTags(List<string> _result, Prefab _considerLoadedPrefab = null)
	{
		string[] array;
		if (_considerLoadedPrefab != null)
		{
			array = _considerLoadedPrefab.GetQuestTags().ToString().Split(',', StringSplitOptions.None);
			Array.Sort<string>(array);
			for (int i = 0; i < array.Length; i++)
			{
				string item = array[i].Trim();
				if (!_result.ContainsCaseInsensitive(item))
				{
					_result.Add(item);
				}
			}
		}
		array = QuestEventManager.allQuestTags.ToString().Split(',', StringSplitOptions.None);
		Array.Sort<string>(array);
		for (int j = 0; j < array.Length; j++)
		{
			string item2 = array[j].Trim();
			if (!_result.ContainsCaseInsensitive(item2))
			{
				_result.Add(item2);
			}
		}
	}

	public bool HasPrefabImposter(PathAbstractions.AbstractedLocation _location)
	{
		return SdFile.Exists(_location.FullPathNoExtension + ".mesh");
	}

	public void ClearImposterPrefab()
	{
		UnityEngine.Object.Destroy(this.ImposterPrefab);
		this.ImposterPrefab = null;
	}

	public bool LoadImposterPrefab(PathAbstractions.AbstractedLocation _location)
	{
		this.ClearImposterPrefab();
		this.ClearVoxelPrefab();
		if (!SdFile.Exists(_location.FullPathNoExtension + ".mesh"))
		{
			return false;
		}
		this.LoadedPrefab = _location;
		bool bTextureArray = MeshDescription.meshes[0].bTextureArray;
		this.ImposterPrefab = SimpleMeshFile.ReadGameObject(_location.FullPathNoExtension + ".mesh", 0f, null, bTextureArray, false, null, null);
		this.ImposterPrefab.transform.name = _location.Name;
		this.ImposterPrefab.transform.position = new Vector3(0f, -3f, 0f);
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void removeAllChunks()
	{
		GameManager.Instance.World.m_ChunkManager.RemoveAllChunks();
		GameManager.Instance.World.ChunkCache.Clear();
		WaterSimulationNative.Instance.Clear();
	}

	public void ClearVoxelPrefab()
	{
		SelectionBoxManager.Instance.Unselect();
		SelectionBoxManager.Instance.GetCategory("DynamicPrefabs").Clear();
		SelectionBoxManager.Instance.GetCategory("TraderTeleport").Clear();
		SelectionBoxManager.Instance.GetCategory("SleeperVolume").Clear();
		SelectionBoxManager.Instance.GetCategory("InfoVolume").Clear();
		SelectionBoxManager.Instance.GetCategory("WallVolume").Clear();
		SelectionBoxManager.Instance.GetCategory("TriggerVolume").Clear();
		SelectionBoxManager.Instance.GetCategory("POIMarker").Clear();
		SleeperVolumeToolManager.CleanUp();
		this.prefabInstanceId = -1;
		this.LoadedPrefab = PathAbstractions.AbstractedLocation.None;
		this.VoxelPrefab = null;
		this.removeAllChunks();
		DecoManager.Instance.OnWorldUnloaded();
		ThreadManager.RunCoroutineSync(DecoManager.Instance.OnWorldLoaded(1024, 1024, GameManager.Instance.World, null));
		this.TogglePrefabFacing(false);
		this.HighlightQuestLoot = this.HighlightQuestLoot;
		this.HighlightBlockTriggers = this.HighlightBlockTriggers;
	}

	public bool NewVoxelPrefab()
	{
		this.ClearImposterPrefab();
		this.ClearVoxelPrefab();
		this.VoxelPrefab = new Prefab();
		DynamicPrefabDecorator dynamicPrefabDecorator = GameManager.Instance.GetDynamicPrefabDecorator();
		if (this.prefabInstanceId != -1)
		{
			dynamicPrefabDecorator.RemovePrefabAndSelection(GameManager.Instance.World, dynamicPrefabDecorator.GetPrefab(this.prefabInstanceId), false);
		}
		dynamicPrefabDecorator.ClearAllPrefabs();
		this.prefabInstanceId = dynamicPrefabDecorator.CreateNewPrefabAndActivate(Prefab.LocationForNewPrefab("New Prefab", null), Vector3i.zero, this.VoxelPrefab, true).id;
		SelectionBoxManager.Instance.GetCategory("DynamicPrefabs").SetVisible(false);
		this.curGridYPos = this.VoxelPrefab.yOffset;
		if (this.groundGrid)
		{
			this.groundGrid.transform.position = new Vector3(0f, (float)(1 - this.curGridYPos) - 0.01f, 0f);
		}
		GameManager.Instance.prefabEditModeManager.ToggleGroundGrid(true);
		for (int i = -6; i <= 6; i++)
		{
			for (int j = -6; j <= 6; j++)
			{
				Chunk chunk = MemoryPools.PoolChunks.AllocSync(true);
				chunk.X = i;
				chunk.Z = j;
				chunk.ResetBiomeIntensity(BiomeIntensity.Default);
				chunk.NeedsRegeneration = true;
				chunk.NeedsLightCalculation = false;
				chunk.NeedsDecoration = false;
				chunk.ResetLights(byte.MaxValue);
				GameManager.Instance.World.ChunkCache.AddChunkSync(chunk, false);
				WaterSimulationNative.Instance.InitializeChunk(chunk);
			}
		}
		this.NeedsSaving = false;
		GamePrefs.Set(EnumGamePrefs.LastLoadedPrefab, string.Empty);
		GameManager.Instance.World.m_ChunkManager.RemoveAllChunksOnAllClients();
		WaterSimulationNative.Instance.SetPaused(true);
		return true;
	}

	public bool LoadVoxelPrefab(PathAbstractions.AbstractedLocation _location, bool _bBulk = false, bool _bIgnoreExcludeImposterCheck = false)
	{
		this.ClearImposterPrefab();
		this.ClearVoxelPrefab();
		this.highlightBlocks(0);
		if (_location.Type == PathAbstractions.EAbstractedLocationType.None)
		{
			Log.Out("No prefab found to load!");
			return false;
		}
		this.VoxelPrefab = new Prefab();
		if (!this.VoxelPrefab.Load(_location, true, true, true, false))
		{
			Log.Out(string.Format("Error loading prefab {0}", _location));
			this.VoxelPrefab = null;
			return false;
		}
		if (!_bIgnoreExcludeImposterCheck && _bBulk && this.VoxelPrefab.bExcludeDistantPOIMesh)
		{
			this.VoxelPrefab = null;
			return false;
		}
		int num = this.VoxelPrefab.size.x * this.VoxelPrefab.size.y * this.VoxelPrefab.size.z;
		if (!_bIgnoreExcludeImposterCheck && _bBulk && ((this.VoxelPrefab.size.y <= 6 && num < 1500) || (this.VoxelPrefab.size.y > 6 && num < 100)))
		{
			this.VoxelPrefab = null;
			return false;
		}
		this.LoadedPrefab = _location;
		this.curGridYPos = this.VoxelPrefab.yOffset;
		if (this.groundGrid)
		{
			this.groundGrid.transform.position = new Vector3(0f, (float)(1 - this.curGridYPos) - 0.01f, 0f);
		}
		ChunkCluster chunkCache = GameManager.Instance.World.ChunkCache;
		this.removeAllChunks();
		int num2 = -1 * this.VoxelPrefab.size.x / 2;
		int num3 = -1 * this.VoxelPrefab.size.z / 2;
		int num4 = num2 + this.VoxelPrefab.size.x;
		int num5 = num3 + this.VoxelPrefab.size.z;
		chunkCache.ChunkMinPos = new Vector2i((num2 - 1) / 16 - 1, (num3 - 1) / 16 - 1);
		chunkCache.ChunkMinPos -= new Vector2i(2, 2);
		chunkCache.ChunkMaxPos = new Vector2i(num4 / 16 + 1, num5 / 16 + 1);
		chunkCache.ChunkMaxPos += new Vector2i(2, 2);
		List<Chunk> list = new List<Chunk>();
		for (int i = chunkCache.ChunkMinPos.x; i <= chunkCache.ChunkMaxPos.x; i++)
		{
			for (int j = chunkCache.ChunkMinPos.y; j <= chunkCache.ChunkMaxPos.y; j++)
			{
				Chunk chunk = MemoryPools.PoolChunks.AllocSync(true);
				chunk.X = i;
				chunk.Z = j;
				chunk.SetFullSunlight();
				chunk.NeedsLightCalculation = false;
				chunk.NeedsDecoration = false;
				chunk.NeedsRegeneration = false;
				chunkCache.AddChunkSync(chunk, true);
				list.Add(chunk);
			}
		}
		Vector3i vector3i = new Vector3i(num2, 1, num3);
		this.VoxelPrefab.CopyIntoLocal(chunkCache, vector3i, true, false, FastTags<TagGroup.Global>.none);
		for (int k = 0; k < list.Count; k++)
		{
			Chunk chunk2 = list[k];
			chunk2.NeedsLightCalculation = false;
			chunk2.NeedsRegeneration = true;
			WaterSimulationNative.Instance.InitializeChunk(chunk2);
		}
		DynamicPrefabDecorator dynamicPrefabDecorator = GameManager.Instance.GetDynamicPrefabDecorator();
		if (this.prefabInstanceId != -1)
		{
			dynamicPrefabDecorator.RemovePrefabAndSelection(GameManager.Instance.World, dynamicPrefabDecorator.GetPrefab(this.prefabInstanceId), false);
		}
		dynamicPrefabDecorator.ClearAllPrefabs();
		this.prefabInstanceId = dynamicPrefabDecorator.CreateNewPrefabAndActivate(this.VoxelPrefab.location, vector3i, this.VoxelPrefab, false).id;
		this.NeedsSaving = false;
		GamePrefs.Set(EnumGamePrefs.LastLoadedPrefab, _location.Name);
		GameManager.Instance.World.m_ChunkManager.RemoveAllChunksOnAllClients();
		this.HighlightQuestLoot = this.HighlightQuestLoot;
		this.HighlightBlockTriggers = this.HighlightBlockTriggers;
		WaterSimulationNative.Instance.SetPaused(true);
		this.highlightBlocks(this.highlightBlockId);
		return true;
	}

	public bool SaveVoxelPrefab()
	{
		if (this.VoxelPrefab == null)
		{
			return false;
		}
		this.updatePrefabBounds();
		EnumInsideOutside[] eInsideOutside = this.VoxelPrefab.UpdateInsideOutside(this.minPos, this.maxPos);
		this.VoxelPrefab.RecalcInsideDevices(eInsideOutside);
		bool flag = this.VoxelPrefab.Save(this.VoxelPrefab.location, true);
		if (flag)
		{
			this.LoadedPrefab = this.VoxelPrefab.location;
			this.LoadXml(this.VoxelPrefab.location);
			GamePrefs.Set(EnumGamePrefs.LastLoadedPrefab, this.VoxelPrefab.PrefabName);
		}
		this.NeedsSaving = false;
		return flag;
	}

	public void UpdateMinMax()
	{
		if (this.VoxelPrefab == null)
		{
			return;
		}
		Vector3i zero = new Vector3i(int.MaxValue, int.MaxValue, int.MaxValue);
		Vector3i zero2 = new Vector3i(int.MinValue, int.MinValue, int.MinValue);
		foreach (Chunk chunk in GameManager.Instance.World.ChunkCache.GetChunkArrayCopySync())
		{
			for (int i = 0; i < 16; i++)
			{
				for (int j = 0; j < 16; j++)
				{
					for (int k = 0; k < 256; k++)
					{
						int blockId = chunk.GetBlockId(i, k, j);
						WaterValue water = chunk.GetWater(i, k, j);
						if (blockId != 0 || water.HasMass() || chunk.GetDensity(i, k, j) < 0)
						{
							Vector3i vector3i = chunk.ToWorldPos(new Vector3i(i, k, j));
							if (zero.x > vector3i.x)
							{
								zero.x = vector3i.x;
							}
							if (zero.y > vector3i.y)
							{
								zero.y = vector3i.y;
							}
							if (zero.z > vector3i.z)
							{
								zero.z = vector3i.z;
							}
							if (zero2.x < vector3i.x)
							{
								zero2.x = vector3i.x;
							}
							if (zero2.y < vector3i.y)
							{
								zero2.y = vector3i.y;
							}
							if (zero2.z < vector3i.z)
							{
								zero2.z = vector3i.z;
							}
						}
					}
				}
			}
		}
		if (zero.x == 2147483647)
		{
			zero = Vector3i.zero;
			zero2 = Vector3i.zero;
		}
		this.minPos = zero;
		this.maxPos = zero2;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updatePrefabBounds()
	{
		if (this.VoxelPrefab == null)
		{
			return;
		}
		this.VoxelPrefab.yOffset = this.curGridYPos;
		this.UpdateMinMax();
		this.VoxelPrefab.CopyFromWorldWithEntities(GameManager.Instance.World, this.minPos, this.maxPos, new List<int>());
		if (this.prefabInstanceId != -1)
		{
			GameManager.Instance.GetDynamicPrefabDecorator().GetPrefab(this.prefabInstanceId).UpdateBoundingBoxPosAndScale(this.minPos, this.VoxelPrefab.size, true);
		}
	}

	public void SetGroundLevel(int _yOffset)
	{
		this.curGridYPos = _yOffset;
		if (this.groundGrid)
		{
			this.groundGrid.transform.position = new Vector3(0f, (float)(1 - this.curGridYPos) - 0.01f, 0f);
		}
	}

	public void ToggleGroundGrid(bool _bForceOn = false)
	{
		if (this.groundGrid == null)
		{
			this.groundGrid = UnityEngine.Object.Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/GroundGrid/GroundGrid"));
			this.groundGrid.transform.position = new Vector3(0f, (float)(1 - this.curGridYPos) + 0.01f, 0f);
			for (int i = 0; i < this.groundGrid.transform.childCount; i++)
			{
				this.groundGrid.transform.GetChild(i).gameObject.tag = "B_Mesh";
			}
			return;
		}
		this.groundGrid.SetActive(_bForceOn || !this.groundGrid.activeSelf);
	}

	public bool IsGroundGrid()
	{
		return this.groundGrid != null && this.groundGrid.activeSelf;
	}

	public void UpdatePrefabBounds()
	{
		DynamicPrefabDecorator dynamicPrefabDecorator = GameManager.Instance.GetDynamicPrefabDecorator();
		if (this.prefabInstanceId == -1)
		{
			this.VoxelPrefab = new Prefab();
			this.prefabInstanceId = dynamicPrefabDecorator.CreateNewPrefabAndActivate(this.VoxelPrefab.location, Vector3i.zero, this.VoxelPrefab, true).id;
		}
		if (this.prefabInstanceId != -1)
		{
			SelectionBoxManager.Instance.GetCategory("DynamicPrefabs").GetBox(dynamicPrefabDecorator.GetPrefab(this.prefabInstanceId).name).SetVisible(true);
			SelectionBoxManager.Instance.GetCategory("DynamicPrefabs").SetVisible(true);
		}
		this.updatePrefabBounds();
	}

	public bool IsPrefabFacing()
	{
		return this.bShowFacing;
	}

	public void TogglePrefabFacing(bool _bShow)
	{
		if (this.VoxelPrefab == null)
		{
			return;
		}
		if (_bShow && this.boxShowFacing == null)
		{
			this.boxShowFacing = SelectionBoxManager.Instance.GetCategory("PrefabFacing").AddBox("single", Vector3i.zero, Vector3i.one, true, false);
		}
		this.bShowFacing = _bShow;
		this.updateFacing();
		if (this.boxShowFacing != null)
		{
			if (this.bShowFacing)
			{
				this.boxShowFacing.SetPositionAndSize(new Vector3(0f, 2f, (float)(-(float)this.VoxelPrefab.size.z / 2 - 3)), Vector3i.one);
			}
			SelectionBoxManager.Instance.SetActive("PrefabFacing", "single", this.bShowFacing);
			SelectionBoxManager.Instance.GetCategory("PrefabFacing").GetBox("single").SetVisible(this.bShowFacing);
			SelectionBoxManager.Instance.GetCategory("PrefabFacing").SetVisible(this.bShowFacing);
		}
	}

	public void RotatePrefabFacing()
	{
		if (this.VoxelPrefab == null)
		{
			return;
		}
		this.VoxelPrefab.rotationToFaceNorth++;
		this.VoxelPrefab.rotationToFaceNorth &= 3;
		this.updateFacing();
		this.NeedsSaving = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateFacing()
	{
		if (this.VoxelPrefab == null)
		{
			return;
		}
		float facing = 0f;
		switch (this.VoxelPrefab.rotationToFaceNorth)
		{
		case 1:
			facing = 90f;
			break;
		case 2:
			facing = 180f;
			break;
		case 3:
			facing = 270f;
			break;
		}
		SelectionBoxManager.Instance.SetFacingDirection("PrefabFacing", "single", facing);
	}

	public void MoveGroundGridUpOrDown(int _deltaY)
	{
		if (this.groundGrid && this.groundGrid.activeSelf)
		{
			this.curGridYPos = Utils.FastClamp(this.curGridYPos - _deltaY, -200, 0);
			if (this.VoxelPrefab != null)
			{
				this.VoxelPrefab.yOffset = this.curGridYPos;
				if (this.prefabInstanceId != -1 && this.OnPrefabChanged != null)
				{
					PrefabInstance prefab = GameManager.Instance.GetDynamicPrefabDecorator().GetPrefab(this.prefabInstanceId);
					this.OnPrefabChanged(prefab);
				}
			}
			if (this.groundGrid)
			{
				this.groundGrid.transform.position = new Vector3(0f, (float)(1 - this.curGridYPos) - 0.01f, 0f);
			}
		}
	}

	public void MovePrefabUpOrDown(int _deltaY)
	{
		this.updatePrefabBounds();
		Vector3i vector3i = this.minPos + _deltaY * Vector3i.up;
		if (vector3i.y < 1 || this.maxPos.y + _deltaY > 250)
		{
			return;
		}
		ChunkCluster chunkCache = GameManager.Instance.World.ChunkCache;
		List<Chunk> chunkArrayCopySync = chunkCache.GetChunkArrayCopySync();
		foreach (Chunk chunk in chunkArrayCopySync)
		{
			chunk.RemoveAllTileEntities();
			if (!chunk.IsEmpty())
			{
				for (int i = 0; i < 16; i++)
				{
					for (int j = 0; j < 16; j++)
					{
						for (int k = 0; k < 254; k++)
						{
							chunk.SetWater(i, k, j, WaterValue.Empty);
							BlockValue block = chunk.GetBlock(i, k, j);
							if (!block.isair && !block.ischild)
							{
								chunkCache.SetBlock(chunk.ToWorldPos(new Vector3i(i, k, j)), BlockValue.Air, true, false);
								chunk.SetDensity(i, k, j, MarchingCubes.DensityAir);
								chunk.SetTextureFull(i, k, j, 0L);
							}
						}
					}
				}
			}
		}
		this.VoxelPrefab.CopyIntoLocal(chunkCache, vector3i, true, false, FastTags<TagGroup.Global>.none);
		foreach (Chunk chunk2 in chunkArrayCopySync)
		{
			chunk2.NeedsRegeneration = true;
		}
		GameManager.Instance.World.m_ChunkManager.RemoveAllChunksOnAllClients();
		this.UpdateMinMax();
		if (this.prefabInstanceId != -1)
		{
			GameManager.Instance.GetDynamicPrefabDecorator().GetPrefab(this.prefabInstanceId).UpdateBoundingBoxPosAndScale(this.minPos, this.VoxelPrefab.size, false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool checkLayerEmpty(int _y, List<Chunk> chunks)
	{
		bool bAllEmpty = true;
		ChunkBlockLayer.LoopBlocksDelegate <>9__0;
		for (int i = 0; i < chunks.Count; i++)
		{
			Chunk chunk = chunks[i];
			ChunkBlockLayer.LoopBlocksDelegate @delegate;
			if ((@delegate = <>9__0) == null)
			{
				@delegate = (<>9__0 = delegate(int x, int y, int z, BlockValue bv)
				{
					if (y == _y)
					{
						bAllEmpty = false;
					}
				});
			}
			chunk.LoopOverAllBlocks(@delegate, false, false);
		}
		return bAllEmpty;
	}

	public void StripTextures()
	{
		HashSet<Chunk> changedChunks = new HashSet<Chunk>();
		List<Chunk> chunkArrayCopySync = GameManager.Instance.World.ChunkCache.GetChunkArrayCopySync();
		bool bUseSelection = BlockToolSelection.Instance.SelectionActive;
		Vector3i selStart = BlockToolSelection.Instance.SelectionMin;
		Vector3i selEnd = selStart + BlockToolSelection.Instance.SelectionSize - Vector3i.one;
		for (int i = 0; i < chunkArrayCopySync.Count; i++)
		{
			Chunk chunk = chunkArrayCopySync[i];
			chunk.LoopOverAllBlocks(delegate(int x, int y, int z, BlockValue bv)
			{
				if (bUseSelection)
				{
					Vector3i vector3i = chunk.ToWorldPos(new Vector3i(x, y, z));
					if (vector3i.x < selStart.x || vector3i.x > selEnd.x || vector3i.y < selStart.y || vector3i.y > selEnd.y || vector3i.z < selStart.z || vector3i.z > selEnd.z)
					{
						return;
					}
				}
				if (chunk.GetTextureFull(x, y, z) != 0L)
				{
					chunk.SetTextureFull(x, y, z, 0L);
					changedChunks.Add(chunk);
				}
			}, false, false);
		}
		foreach (Chunk chunk2 in changedChunks)
		{
			chunk2.NeedsRegeneration = true;
		}
		if (changedChunks.Count > 0)
		{
			this.NeedsSaving = true;
		}
		GameManager.Instance.World.m_ChunkManager.RemoveAllChunksOnAllClients();
	}

	public void StripInternalTextures()
	{
		HashSet<Chunk> changedChunks = new HashSet<Chunk>();
		World world = GameManager.Instance.World;
		List<Chunk> chunkArrayCopySync = world.ChunkCache.GetChunkArrayCopySync();
		bool bUseSelection = BlockToolSelection.Instance.SelectionActive;
		Vector3i selStart = BlockToolSelection.Instance.SelectionMin;
		Vector3i selEnd = selStart + BlockToolSelection.Instance.SelectionSize - Vector3i.one;
		for (int i = 0; i < chunkArrayCopySync.Count; i++)
		{
			Chunk chunk = chunkArrayCopySync[i];
			chunk.LoopOverAllBlocks(delegate(int x, int y, int z, BlockValue bv)
			{
				if (bUseSelection)
				{
					Vector3i vector3i = chunk.ToWorldPos(new Vector3i(x, y, z));
					if (vector3i.x < selStart.x || vector3i.x > selEnd.x || vector3i.y < selStart.y || vector3i.y > selEnd.y || vector3i.z < selStart.z || vector3i.z > selEnd.z)
					{
						return;
					}
				}
				if (chunk.GetTextureFull(x, y, z) == 0L)
				{
					return;
				}
				BlockShapeNew blockShapeNew = bv.Block.shape as BlockShapeNew;
				if (blockShapeNew == null)
				{
					return;
				}
				for (int j = 0; j < 6; j++)
				{
					BlockFace blockFace = (BlockFace)j;
					Vector3i other = new Vector3i(BlockFaceFlags.OffsetForFace(blockFace));
					Vector3i pos = chunk.ToWorldPos(new Vector3i(x, y, z)) + other;
					BlockValue block = world.GetBlock(pos);
					if (!block.isair)
					{
						BlockShapeNew blockShapeNew2 = block.Block.shape as BlockShapeNew;
						if (blockShapeNew2 != null)
						{
							BlockFace face = BlockFaceFlags.OppositeFace(blockFace);
							BlockShapeNew.EnumFaceOcclusionInfo faceInfo = blockShapeNew.GetFaceInfo(bv, blockFace);
							BlockShapeNew.EnumFaceOcclusionInfo faceInfo2 = blockShapeNew2.GetFaceInfo(block, face);
							if ((faceInfo == BlockShapeNew.EnumFaceOcclusionInfo.Full && faceInfo2 == BlockShapeNew.EnumFaceOcclusionInfo.Full) || (faceInfo == BlockShapeNew.EnumFaceOcclusionInfo.Part && faceInfo2 == BlockShapeNew.EnumFaceOcclusionInfo.Full) || (faceInfo == BlockShapeNew.EnumFaceOcclusionInfo.Part && faceInfo2 == BlockShapeNew.EnumFaceOcclusionInfo.Part && blockShapeNew == blockShapeNew2 && bv.rotation == block.rotation))
							{
								blockFace = blockShapeNew.GetRotatedBlockFace(bv, blockFace);
								world.ChunkCache.SetBlockFaceTexture(chunk.ToWorldPos(new Vector3i(x, y, z)), blockFace, 0);
								changedChunks.Add(chunk);
							}
						}
					}
				}
			}, false, false);
		}
		foreach (Chunk chunk2 in changedChunks)
		{
			chunk2.NeedsRegeneration = true;
		}
		if (changedChunks.Count > 0)
		{
			this.NeedsSaving = true;
		}
		GameManager.Instance.World.m_ChunkManager.RemoveAllChunksOnAllClients();
	}

	public void GetLootAndFetchLootContainerCount(out int _loot, out int _fetchLoot, out int _restorePower)
	{
		int tempLoot = 0;
		int tempFetchLoot = 0;
		int tempRestorePower = 0;
		List<Chunk> chunkArrayCopySync = GameManager.Instance.World.ChunkCache.GetChunkArrayCopySync();
		ChunkBlockLayer.LoopBlocksDelegate <>9__0;
		for (int i = 0; i < chunkArrayCopySync.Count; i++)
		{
			Chunk chunk = chunkArrayCopySync[i];
			ChunkBlockLayer.LoopBlocksDelegate @delegate;
			if ((@delegate = <>9__0) == null)
			{
				@delegate = (<>9__0 = delegate(int _, int _, int _, BlockValue bv)
				{
					Block block = bv.Block;
					if (block == null)
					{
						return;
					}
					bool flag = block.IndexName != null;
					if (flag && block.IndexName == Constants.cQuestLootFetchContainerIndexName)
					{
						int num = tempFetchLoot;
						tempFetchLoot = num + 1;
						return;
					}
					if (flag && block.IndexName == Constants.cQuestRestorePowerIndexName)
					{
						int num = tempRestorePower;
						tempRestorePower = num + 1;
						return;
					}
					if (block is BlockLoot)
					{
						int num = tempLoot;
						tempLoot = num + 1;
						return;
					}
					BlockCompositeTileEntity blockCompositeTileEntity = block as BlockCompositeTileEntity;
					if (blockCompositeTileEntity != null && blockCompositeTileEntity.CompositeData.HasFeature<ITileEntityLootable>())
					{
						int num = tempLoot;
						tempLoot = num + 1;
					}
				});
			}
			chunk.LoopOverAllBlocks(@delegate, false, false);
		}
		_loot = tempLoot;
		_fetchLoot = tempFetchLoot;
		_restorePower = tempRestorePower;
	}

	public void HighlightBlocks(Block _blockClass)
	{
		this.highlightBlockId = ((_blockClass != null) ? _blockClass.blockID : 0);
		this.highlightBlocks(this.highlightBlockId);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void highlightBlocks(int _blockId)
	{
		BlockHighlighter.Cleanup();
		if (_blockId <= 0)
		{
			return;
		}
		List<Chunk> chunkArrayCopySync = GameManager.Instance.World.ChunkCache.GetChunkArrayCopySync();
		for (int i = 0; i < chunkArrayCopySync.Count; i++)
		{
			Chunk chunk = chunkArrayCopySync[i];
			chunk.LoopOverAllBlocks(delegate(int _x, int _y, int _z, BlockValue _bv)
			{
				if (_bv.type == _blockId)
				{
					BlockHighlighter.AddBlock(chunk.worldPosIMin + new Vector3i(_x, _y, _z));
				}
			}, false, false);
		}
	}

	public bool HighlightQuestLoot
	{
		get
		{
			return this.bShowQuestLoot;
		}
		set
		{
			this.bShowQuestLoot = value;
			NavObjectManager.Instance.UnRegisterNavObjectByClass("editor_quest_loot_container");
			if (value)
			{
				foreach (Chunk chunk in GameManager.Instance.World.ChunkCache.GetChunkArrayCopySync())
				{
					List<Vector3i> list;
					if (chunk.IndexedBlocks.TryGetValue(Constants.cQuestLootFetchContainerIndexName, out list))
					{
						Vector3i worldPos = chunk.GetWorldPos();
						foreach (Vector3i other in list)
						{
							NavObjectManager.Instance.RegisterNavObject("editor_quest_loot_container", (worldPos + other).ToVector3Center(), "", false, null);
						}
					}
				}
			}
		}
	}

	public bool HighlightBlockTriggers
	{
		get
		{
			return this.bShowBlockTriggers;
		}
		set
		{
			this.bShowBlockTriggers = value;
			NavObjectManager.Instance.UnRegisterNavObjectByClass("editor_block_trigger");
			if (value)
			{
				foreach (Chunk chunk in GameManager.Instance.World.ChunkCache.GetChunkArrayCopySync())
				{
					List<BlockTrigger> list = chunk.GetBlockTriggers().list;
					Vector3i worldPos = chunk.GetWorldPos();
					for (int i = 0; i < list.Count; i++)
					{
						NavObject navObject = NavObjectManager.Instance.RegisterNavObject("editor_block_trigger", (worldPos + list[i].LocalChunkPos).ToVector3Center(), "", false, null);
						navObject.name = list[i].TriggerDisplay();
						navObject.OverrideColor = ((list[i].TriggeredByIndices.Count > 0) ? Color.blue : Color.red);
					}
				}
			}
		}
	}

	public const int cGroundGridYDefault = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public const string SingleFacingBoxName = "single";

	public static PrefabEditModeManager Instance;

	public PathAbstractions.AbstractedLocation LoadedPrefab;

	public Prefab VoxelPrefab;

	public GameObject ImposterPrefab;

	public bool NeedsSaving;

	public Vector3i minPos;

	public Vector3i maxPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<PathAbstractions.AbstractedLocation, Prefab> loadedPrefabHeaders = new Dictionary<PathAbstractions.AbstractedLocation, Prefab>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int curGridYPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject groundGrid;

	[PublicizedFrom(EAccessModifier.Private)]
	public int prefabInstanceId = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public FileSystemWatcher xmlWatcher;

	[PublicizedFrom(EAccessModifier.Private)]
	public SelectionBox boxShowFacing;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bShowFacing;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bShowQuestLoot;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bShowBlockTriggers;

	public List<byte> TriggerLayers = new List<byte>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int highlightBlockId;
}

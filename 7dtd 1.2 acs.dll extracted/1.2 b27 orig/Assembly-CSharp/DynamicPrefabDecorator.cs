using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using UnityEngine;

public class DynamicPrefabDecorator : IDynamicDecorator, ISelectionBoxCallback
{
	public event Action<PrefabInstance> OnPrefabLoaded;

	public event Action<PrefabInstance> OnPrefabChanged;

	public event Action<PrefabInstance> OnPrefabRemoved;

	public IEnumerator Load(string _path)
	{
		if (!SdFile.Exists(_path + "/prefabs.xml"))
		{
			yield break;
		}
		MicroStopwatch msw = new MicroStopwatch(true);
		XmlFile xmlFile;
		try
		{
			this.id = 0;
			xmlFile = new XmlFile(_path, "prefabs", false, false);
		}
		catch (Exception ex)
		{
			Log.Error("Loading prefabs xml file for level '" + Path.GetFileName(_path) + "': " + ex.Message);
			Log.Exception(ex);
			yield break;
		}
		int i = 0;
		int totalPrefabs = xmlFile.XmlDoc.Root.Elements("decoration").Count<XElement>();
		LocalPlayerUI ui = LocalPlayerUI.primaryUI;
		bool progressWindowOpen = ui && ui.windowManager.IsWindowOpen(XUiC_ProgressWindow.ID);
		foreach (XElement element in xmlFile.XmlDoc.Root.Elements("decoration"))
		{
			try
			{
				int num = i;
				i = num + 1;
				if (element.HasAttribute("name"))
				{
					string attribute = element.GetAttribute("name");
					Vector3i vector3i = Vector3i.Parse(element.GetAttribute("position"));
					bool flag;
					StringParsers.TryParseBool(element.GetAttribute("y_is_groundlevel"), out flag, 0, -1, true);
					byte rotation = 0;
					if (element.HasAttribute("rotation"))
					{
						rotation = byte.Parse(element.GetAttribute("rotation"));
					}
					Prefab prefabRotated = this.GetPrefabRotated(attribute, (int)rotation, true, true, false);
					if (prefabRotated == null)
					{
						Log.Warning("Could not load prefab '" + attribute + "'. Skipping it");
						continue;
					}
					if (flag)
					{
						vector3i.y += prefabRotated.yOffset;
					}
					if (prefabRotated.bTraderArea && SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
					{
						this.AddTrader(new TraderArea(vector3i, prefabRotated.size, prefabRotated.TraderAreaProtect, prefabRotated.TeleportVolumes));
					}
					num = this.id;
					this.id = num + 1;
					PrefabInstance prefabInstance = new PrefabInstance(num, prefabRotated.location, vector3i, rotation, prefabRotated, 0);
					this.AddPrefab(prefabInstance, prefabInstance.prefab.HasQuestTag());
				}
			}
			catch (Exception ex2)
			{
				Log.Error("Loading prefabs xml file for level '" + Path.GetFileName(_path) + "': " + ex2.Message);
				Log.Exception(ex2);
			}
			if (msw.ElapsedMilliseconds > (long)Constants.cMaxLoadTimePerFrameMillis)
			{
				if (progressWindowOpen)
				{
					XUiC_ProgressWindow.SetText(ui, string.Format(Localization.Get("uiLoadCreatingWorldPrefabs", false), Math.Min(100.0, 105.0 * (double)i / (double)totalPrefabs).ToString("0")), true);
				}
				yield return null;
				msw.ResetAndRestart();
			}
		}
		IEnumerator<XElement> enumerator = null;
		if (progressWindowOpen)
		{
			XUiC_ProgressWindow.SetText(ui, string.Format(Localization.Get("uiLoadCreatingWorldPrefabs", false), "100"), true);
			yield return null;
		}
		this.SortPrefabs();
		XUiC_ProgressWindow.SetText(ui, Localization.Get("uiLoadCreatingWorld", false), true);
		yield return null;
		yield break;
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SortPrefabs()
	{
		List<PrefabInstance> obj = this.allPrefabsSorted;
		lock (obj)
		{
			this.allPrefabsSorted.Clear();
			this.allPrefabsSorted.AddRange(this.allPrefabs);
			this.allPrefabsSorted.Sort((PrefabInstance a, PrefabInstance b) => a.boundingBoxPosition.x.CompareTo(b.boundingBoxPosition.x));
			this.isSortNeeded = false;
		}
	}

	public int GetNextId()
	{
		int num = this.id;
		this.id = num + 1;
		return num;
	}

	public bool Save(string _path)
	{
		bool result;
		try
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.CreateXmlDeclaration();
			XmlElement node = xmlDocument.AddXmlElement("prefabs");
			for (int i = 0; i < this.allPrefabs.Count; i++)
			{
				PrefabInstance prefabInstance = this.allPrefabs[i];
				if (prefabInstance != null)
				{
					string value = "";
					Vector3i boundingBoxPosition = prefabInstance.boundingBoxPosition;
					if (prefabInstance.prefab != null && prefabInstance.prefab.location.Type != PathAbstractions.EAbstractedLocationType.None)
					{
						value = prefabInstance.prefab.PrefabName;
						boundingBoxPosition.y -= prefabInstance.prefab.yOffset;
					}
					else if (prefabInstance.location.Type != PathAbstractions.EAbstractedLocationType.None)
					{
						value = prefabInstance.location.Name;
					}
					node.AddXmlElement("decoration").SetAttrib("type", "model").SetAttrib("name", value).SetAttrib("position", boundingBoxPosition.ToStringNoBlanks()).SetAttrib("rotation", prefabInstance.rotation.ToString()).SetAttrib("y_is_groundlevel", "true");
				}
			}
			xmlDocument.SdSave(_path + "/prefabs.xml");
			result = true;
		}
		catch (Exception ex)
		{
			Log.Error(ex.ToString());
			Log.Error(ex.StackTrace);
			result = false;
		}
		return result;
	}

	public void Cleanup()
	{
		this.prefabCache.Clear();
		this.prefabCacheRotations.Clear();
		this.prefabMeshExisting.Clear();
	}

	public List<PrefabInstance> GetDynamicPrefabs()
	{
		return this.allPrefabs;
	}

	public void AddPrefab(PrefabInstance _pi, bool _isPOI = false)
	{
		this.allPrefabs.Add(_pi);
		if (_isPOI)
		{
			this.poiPrefabs.Add(_pi);
		}
		this.isSortNeeded = true;
	}

	public void RemovePrefab(PrefabInstance _pi)
	{
		this.allPrefabs.Remove(_pi);
		this.poiPrefabs.Remove(_pi);
		List<PrefabInstance> obj = this.allPrefabsSorted;
		lock (obj)
		{
			this.allPrefabsSorted.Remove(_pi);
		}
	}

	public List<PrefabInstance> GetPOIPrefabs()
	{
		return this.poiPrefabs;
	}

	public void ClearTraders()
	{
		this.traderAreas.Clear();
	}

	public void AddTrader(TraderArea _ta)
	{
		this.ProtectSizeXMax = Utils.FastMax(this.ProtectSizeXMax, _ta.ProtectSize.x);
		this.traderAreas.Add(_ta);
		this.traderAreas.Sort(this.traderComparer);
	}

	public List<TraderArea> GetTraderAreas()
	{
		return this.traderAreas;
	}

	public bool IsWithinTraderArea(Vector3i _minPos, Vector3i _maxPos)
	{
		for (int i = 0; i < this.traderAreas.Count; i++)
		{
			if (this.traderAreas[i].Overlaps(_minPos, _maxPos))
			{
				return true;
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int TraderBinarySearch(int x)
	{
		int num = x - this.ProtectSizeXMax;
		int i = 0;
		int num2 = this.traderAreas.Count;
		while (i < num2)
		{
			int num3 = (i + num2) / 2;
			if (this.traderAreas[num3].ProtectPosition.x < num)
			{
				i = num3 + 1;
			}
			else
			{
				num2 = num3;
			}
		}
		return i;
	}

	public TraderArea GetTraderAtPosition(Vector3i _pos, int _padding)
	{
		TraderArea result = null;
		int num = -_padding;
		int i = this.TraderBinarySearch(_pos.x - _padding);
		int count = this.traderAreas.Count;
		while (i < count)
		{
			TraderArea traderArea = this.traderAreas[i];
			int num2 = _pos.x - traderArea.ProtectPosition.x;
			if (num2 < num)
			{
				break;
			}
			if (num2 < traderArea.ProtectSize.x + _padding)
			{
				int num3 = _pos.z - traderArea.ProtectPosition.z;
				if (num3 >= num && num3 < traderArea.ProtectSize.z + _padding)
				{
					result = traderArea;
					break;
				}
			}
			i++;
		}
		return result;
	}

	public void CopyAllPrefabsIntoWorld(World _world, bool _bOverwriteExistingBlocks = false)
	{
		for (int i = 0; i < this.allPrefabs.Count; i++)
		{
			if (this.allPrefabs[i].standaloneBlockSize == 0)
			{
				this.allPrefabs[i].CopyIntoWorld(_world, true, _bOverwriteExistingBlocks, FastTags<TagGroup.Global>.none);
			}
			else
			{
				Log.Warning("Prefab with standaloneBlockSize={0} not supported", new object[]
				{
					this.allPrefabs[i].standaloneBlockSize
				});
			}
		}
	}

	public void CleanAllPrefabsFromWorld(World _world)
	{
		for (int i = 0; i < this.allPrefabs.Count; i++)
		{
			this.allPrefabs[i].CleanFromWorld(_world, true);
		}
	}

	public void ClearAllPrefabs()
	{
		foreach (PrefabInstance prefabInstance in this.allPrefabs)
		{
			this.CallPrefabRemovedEvent(prefabInstance);
		}
		this.allPrefabs.Clear();
		this.poiPrefabs.Clear();
		List<PrefabInstance> obj = this.allPrefabsSorted;
		lock (obj)
		{
			this.allPrefabsSorted.Clear();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CallPrefabRemovedEvent(PrefabInstance _prefabInstance)
	{
		if (this.OnPrefabRemoved != null)
		{
			this.OnPrefabRemoved(_prefabInstance);
		}
	}

	public void CallPrefabChangedEvent(PrefabInstance _prefabInstance)
	{
		this.isSortNeeded = true;
		if (this.OnPrefabChanged != null)
		{
			this.OnPrefabChanged(_prefabInstance);
		}
	}

	public Prefab GetPrefab(string _name, bool _applyMapping = true, bool _fixChildblocks = true, bool _allowMissingBlocks = false)
	{
		Dictionary<string, Prefab> obj = this.prefabCache;
		Prefab result;
		lock (obj)
		{
			if (this.prefabCache.ContainsKey(_name))
			{
				result = this.prefabCache[_name];
			}
			else
			{
				Prefab prefab = new Prefab();
				if (prefab.Load(_name, _applyMapping, _fixChildblocks, _allowMissingBlocks, false))
				{
					this.prefabCache[_name] = prefab;
					result = prefab;
				}
				else
				{
					result = null;
				}
			}
		}
		return result;
	}

	public Prefab GetPrefabRotated(string _name, int _rotation, bool _applyMapping = true, bool _fixChildblocks = true, bool _allowMissingBlocks = false)
	{
		_rotation &= 3;
		Dictionary<string, Prefab> obj = this.prefabCache;
		Prefab result;
		lock (obj)
		{
			Prefab[] array;
			if (this.prefabCacheRotations.TryGetValue(_name, out array))
			{
				if (array[_rotation] != null)
				{
					return array[_rotation];
				}
			}
			else
			{
				array = new Prefab[4];
				this.prefabCacheRotations[_name] = array;
			}
			Prefab prefab = this.GetPrefab(_name, _applyMapping, _fixChildblocks && _rotation == 0, _allowMissingBlocks);
			if (prefab == null)
			{
				result = null;
			}
			else
			{
				if (_rotation > 0)
				{
					prefab = prefab.Clone(true);
					prefab.RotateY(true, _rotation);
				}
				array[_rotation] = prefab;
				result = prefab;
			}
		}
		return result;
	}

	public void CreateBoundingBoxes()
	{
		for (int i = 0; i < this.allPrefabs.Count; i++)
		{
			this.allPrefabs[i].CreateBoundingBox(false);
		}
	}

	public PrefabInstance GetPrefab(int _id)
	{
		for (int i = 0; i < this.allPrefabs.Count; i++)
		{
			if (this.allPrefabs[i].id == _id)
			{
				return this.allPrefabs[i];
			}
		}
		return null;
	}

	public bool IsActivePrefab(int _id)
	{
		PrefabInstance prefab = this.GetPrefab(_id);
		return prefab != null && prefab == this.ActivePrefab;
	}

	public PrefabInstance CreateNewPrefabAndActivate(PathAbstractions.AbstractedLocation _location, Vector3i _position, Prefab _bad, bool _bSetActive = true)
	{
		if (_bad == null)
		{
			_bad = new Prefab(new Vector3i(3, 3, 3));
		}
		PrefabInstance prefabInstance = new PrefabInstance(this.GetNextId(), _location, _position, 0, _bad, 0);
		prefabInstance.CreateBoundingBox(true);
		this.AddPrefab(prefabInstance, false);
		if (_bSetActive)
		{
			SelectionBoxManager.Instance.SetActive("DynamicPrefabs", prefabInstance.name, true);
		}
		if (this.OnPrefabLoaded != null)
		{
			this.OnPrefabLoaded(prefabInstance);
		}
		return prefabInstance;
	}

	public PrefabInstance RemoveActivePrefab(World _world)
	{
		if (this.ActivePrefab == null)
		{
			return null;
		}
		PrefabInstance activePrefab = this.ActivePrefab;
		this.RemovePrefabAndSelection(_world, activePrefab, true);
		this.ActivePrefab = null;
		return activePrefab;
	}

	public void RemovePrefabAndSelection(World _world, PrefabInstance _prefab, bool _bCleanFromWorld)
	{
		if (_bCleanFromWorld)
		{
			_prefab.CleanFromWorld(_world, true);
		}
		this.RemovePrefab(_prefab);
		SelectionBoxManager.Instance.GetCategory("DynamicPrefabs").RemoveBox(_prefab.name);
		SelectionBoxManager.Instance.GetCategory("TraderTeleport").RemoveBox(_prefab.name);
		SelectionBoxManager.Instance.GetCategory("InfoVolume").RemoveBox(_prefab.name);
		SelectionBoxManager.Instance.GetCategory("WallVolume").RemoveBox(_prefab.name);
		SelectionBoxManager.Instance.GetCategory("TriggerVolume").RemoveBox(_prefab.name);
		for (int i = 0; i < _prefab.prefab.SleeperVolumes.Count; i++)
		{
			if (_prefab.prefab.SleeperVolumes[i].used)
			{
				SelectionBoxManager.Instance.GetCategory("SleeperVolume").RemoveBox(_prefab.name + "_" + i.ToString());
			}
		}
		SelectionBoxManager.Instance.GetCategory("POIMarker").Clear();
		SelectionBoxManager.Instance.GetCategory("SleeperVolume").RemoveBox(_prefab.name);
		this.CallPrefabRemovedEvent(_prefab);
	}

	public virtual void DecorateChunk(World _world, Chunk _chunk)
	{
		this.DecorateChunk(_world, _chunk, false);
	}

	public void DecorateChunk(World _world, Chunk _chunk, bool _bForceOverwriteBlocks = false)
	{
		int blockWorldPosX = _chunk.GetBlockWorldPosX(0);
		int blockWorldPosZ = _chunk.GetBlockWorldPosZ(0);
		this.GetPrefabsAtXZ(blockWorldPosX, blockWorldPosX + 15, blockWorldPosZ, blockWorldPosZ + 15, this.decorateChunkPIs);
		this.decorateChunkPIs.Sort(new Comparison<PrefabInstance>(this.prefabInstanceSizeComparison));
		for (int i = 0; i < this.decorateChunkPIs.Count; i++)
		{
			PrefabInstance prefabInstance = this.decorateChunkPIs[i];
			if (prefabInstance.Overlaps(_chunk))
			{
				prefabInstance.CopyIntoChunk(_world, _chunk, _bForceOverwriteBlocks);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int prefabInstanceSizeComparison(PrefabInstance _a, PrefabInstance _b)
	{
		int value = _a.boundingBoxSize.x * _a.boundingBoxSize.z;
		return (_b.boundingBoxSize.x * _b.boundingBoxSize.z).CompareTo(value);
	}

	public bool IsEntityInPrefab(int _entityId)
	{
		for (int i = 0; i < this.allPrefabs.Count; i++)
		{
			if (this.allPrefabs[i].Contains(_entityId))
			{
				return true;
			}
		}
		return false;
	}

	public bool OnSelectionBoxActivated(string _category, string _name, bool _bActivated)
	{
		if (_bActivated)
		{
			ValueTuple<SelectionCategory, SelectionBox>? valueTuple;
			SelectionBox selectionBox = (SelectionBoxManager.Instance.Selection != null) ? valueTuple.GetValueOrDefault().Item2 : null;
			if (selectionBox == null)
			{
				Log.Error("Prefab SelectionBox selected but no prefab defined (OSBA)!");
				return true;
			}
			PrefabInstance prefabInstance = selectionBox.UserData as PrefabInstance;
			if (prefabInstance != null)
			{
				this.ActivePrefab = prefabInstance;
				this.ActivePrefab.UpdateImposterView();
			}
			else
			{
				Log.Error("Selected prefab SelectionBox has no PrefabInstance assigned");
				StringParsers.SeparatorPositions separatorPositions = StringParsers.GetSeparatorPositions(_name, '.', 1, 0, -1);
				int num = 0;
				if (separatorPositions.TotalFound >= 1 && StringParsers.TryParseSInt32(_name, out num, separatorPositions.Sep1 + 1, separatorPositions.Sep2 - 1, NumberStyles.Integer))
				{
					this.ActivePrefab = this.GetPrefab(num);
				}
			}
		}
		else
		{
			this.ActivePrefab = null;
		}
		return true;
	}

	public void OnSelectionBoxMoved(string _category, string _name, Vector3 _moveVector)
	{
		if (this.ActivePrefab != null)
		{
			ValueTuple<SelectionCategory, SelectionBox>? valueTuple;
			if (((SelectionBoxManager.Instance.Selection != null) ? valueTuple.GetValueOrDefault().Item2 : null) == null)
			{
				Log.Error("Prefab SelectionBox selected but no prefab defined (OSBM)!");
				return;
			}
			this.ActivePrefab.MoveBoundingBox(new Vector3i(_moveVector));
			this.ActivePrefab.UpdateImposterView();
		}
	}

	public void OnSelectionBoxSized(string _category, string _name, int _dTop, int _dBottom, int _dNorth, int _dSouth, int _dEast, int _dWest)
	{
		if (GameManager.Instance.IsEditMode() && !PrefabEditModeManager.Instance.IsActive())
		{
			return;
		}
		if (this.ActivePrefab != null)
		{
			this.ActivePrefab.ResizeBoundingBox(new Vector3i(_dEast + _dWest, _dTop + _dBottom, _dNorth + _dSouth));
			this.ActivePrefab.MoveBoundingBox(new Vector3i(-_dWest, -_dBottom, -_dSouth));
		}
	}

	public void OnSelectionBoxMirrored(Vector3i _axis)
	{
	}

	public bool OnSelectionBoxDelete(string _category, string _name)
	{
		SelectionCategory category = SelectionBoxManager.Instance.GetCategory(_category);
		SelectionBox selectionBox = (category != null) ? category.GetBox(_name) : null;
		if (selectionBox == null)
		{
			Log.Error("SelectionBox null (OSBD)");
			return false;
		}
		PrefabInstance prefabInstance = selectionBox.UserData as PrefabInstance;
		if (prefabInstance != null)
		{
			prefabInstance.DestroyImposterView();
		}
		return false;
	}

	public bool OnSelectionBoxIsAvailable(string _category, EnumSelectionBoxAvailabilities _criteria)
	{
		if (_criteria == EnumSelectionBoxAvailabilities.CanResize)
		{
			return PrefabEditModeManager.Instance.IsActive();
		}
		return _criteria == EnumSelectionBoxAvailabilities.CanShowProperties;
	}

	public void OnSelectionBoxShowProperties(bool _bVisible, GUIWindowManager _windowManager)
	{
		XUiC_EditorPanelSelector childByType = _windowManager.playerUI.xui.FindWindowGroupByName(XUiC_EditorPanelSelector.ID).GetChildByType<XUiC_EditorPanelSelector>();
		if (childByType == null)
		{
			return;
		}
		childByType.SetSelected("prefabList");
		_windowManager.SwitchVisible(XUiC_InGameMenuWindow.ID, false);
	}

	public void OnSelectionBoxRotated(string _category, string _name)
	{
		ValueTuple<SelectionCategory, SelectionBox>? valueTuple;
		if (((SelectionBoxManager.Instance.Selection != null) ? valueTuple.GetValueOrDefault().Item2 : null) == null)
		{
			Log.Error("Prefab SelectionBox selected but no prefab defined (OSBR)!");
			return;
		}
		this.ActivePrefab.RotateAroundY();
		this.ActivePrefab.UpdateImposterView();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int PrefabBinarySearch(float x)
	{
		if (this.isSortNeeded)
		{
			this.SortPrefabs();
		}
		int num = (int)x - 200;
		int i = 0;
		int num2 = this.allPrefabsSorted.Count;
		while (i < num2)
		{
			int num3 = (i + num2) / 2;
			if (this.allPrefabsSorted[num3].boundingBoxPosition.x < num)
			{
				i = num3 + 1;
			}
			else
			{
				num2 = num3;
			}
		}
		return i;
	}

	public PrefabInstance GetPrefabAtPosition(Vector3 _position, bool _checkTags = true)
	{
		PrefabInstance prefabInstance = null;
		Vector3i vector3i = Vector3i.Floor(_position);
		int i = this.PrefabBinarySearch((float)vector3i.x);
		int count = this.allPrefabsSorted.Count;
		while (i < count)
		{
			PrefabInstance prefabInstance2 = this.allPrefabsSorted[i];
			int num = vector3i.x - prefabInstance2.boundingBoxPosition.x;
			if (num < 0)
			{
				break;
			}
			if (num < prefabInstance2.boundingBoxSize.x)
			{
				int num2 = vector3i.z - prefabInstance2.boundingBoxPosition.z;
				if (num2 >= 0 && num2 < prefabInstance2.boundingBoxSize.z)
				{
					int num3 = vector3i.y - prefabInstance2.boundingBoxPosition.y;
					if (num3 >= 0 && num3 < prefabInstance2.boundingBoxSize.y && (!_checkTags || !prefabInstance2.prefab.Tags.Test_AnySet(this.streetTileTag)))
					{
						prefabInstance = prefabInstance2;
						for (i++; i < count; i++)
						{
							prefabInstance2 = this.allPrefabsSorted[i];
							num = vector3i.x - prefabInstance2.boundingBoxPosition.x;
							if (num < 0)
							{
								break;
							}
							if (num < prefabInstance2.boundingBoxSize.x)
							{
								num2 = vector3i.z - prefabInstance2.boundingBoxPosition.z;
								if (num2 >= 0 && num2 < prefabInstance2.boundingBoxSize.z)
								{
									num3 = vector3i.y - prefabInstance2.boundingBoxPosition.y;
									if (num3 >= 0 && num3 < prefabInstance2.boundingBoxSize.y && (!_checkTags || !prefabInstance2.prefab.Tags.Test_AnySet(this.streetTileTag)))
									{
										if (prefabInstance.boundingBoxPosition.x != prefabInstance2.boundingBoxPosition.x || prefabInstance.boundingBoxSize.x >= prefabInstance2.boundingBoxPosition.x)
										{
											prefabInstance = prefabInstance2;
											break;
										}
										break;
									}
								}
							}
						}
						break;
					}
				}
			}
			i++;
		}
		return prefabInstance;
	}

	public void GetPrefabsAtXZ(int _xMin, int _xMax, int _zMin, int _zMax, List<PrefabInstance> _list)
	{
		List<PrefabInstance> obj = this.allPrefabsSorted;
		lock (obj)
		{
			_list.Clear();
			if (this.isSortNeeded)
			{
				_list.AddRange(this.allPrefabsSorted);
			}
			else
			{
				int count = this.allPrefabsSorted.Count;
				int num = Utils.Fastfloor((float)_xMin) - 200;
				int i = 0;
				int num2 = count;
				while (i < num2)
				{
					int num3 = (i + num2) / 2;
					if (this.allPrefabsSorted[num3].boundingBoxPosition.x < num)
					{
						i = num3 + 1;
					}
					else
					{
						num2 = num3;
					}
				}
				for (int j = i; j < count; j++)
				{
					PrefabInstance prefabInstance = this.allPrefabsSorted[j];
					if (prefabInstance.boundingBoxPosition.x > _xMax)
					{
						break;
					}
					if (prefabInstance.boundingBoxPosition.x + prefabInstance.boundingBoxSize.x > _xMin && prefabInstance.boundingBoxPosition.z <= _zMax && prefabInstance.boundingBoxPosition.z + prefabInstance.boundingBoxSize.z > _zMin)
					{
						_list.Add(prefabInstance);
					}
				}
			}
		}
	}

	public virtual void GetPrefabsAround(Vector3 _position, float _distance, Dictionary<int, PrefabInstance> _prefabs)
	{
		float num = _distance * _distance;
		for (int i = 0; i < this.allPrefabs.Count; i++)
		{
			PrefabInstance prefabInstance = this.allPrefabs[i];
			float num2 = _position.x - ((float)prefabInstance.boundingBoxPosition.x + (float)prefabInstance.boundingBoxSize.x * 0.5f);
			float num3 = _position.z - ((float)prefabInstance.boundingBoxPosition.z + (float)prefabInstance.boundingBoxSize.z * 0.5f);
			if (num2 * num2 + num3 * num3 <= num)
			{
				string text = (prefabInstance.prefab.distantPOIOverride == null) ? prefabInstance.prefab.PrefabName : prefabInstance.prefab.distantPOIOverride;
				bool flag;
				if (!this.prefabMeshExisting.TryGetValue(text, out flag))
				{
					flag = (PathAbstractions.PrefabImpostersSearchPaths.GetLocation(text, null, null).Type != PathAbstractions.EAbstractedLocationType.None);
					this.prefabMeshExisting[text] = flag;
				}
				if (flag)
				{
					_prefabs.Add(prefabInstance.id, prefabInstance);
				}
			}
		}
	}

	public virtual void GetPrefabsAround(Vector3 _position, float _nearDistance, float _farDistance, Dictionary<int, PrefabInstance> _prefabsFar, Dictionary<int, PrefabInstance> _prefabsNear)
	{
		Vector2 vector = new Vector2(_position.x, _position.z);
		float num = _farDistance * _farDistance;
		for (int i = this.PrefabBinarySearch(vector.x - _farDistance); i < this.allPrefabsSorted.Count; i++)
		{
			PrefabInstance prefabInstance = this.allPrefabsSorted[i];
			if (_position.x - (float)prefabInstance.boundingBoxPosition.x < -_farDistance)
			{
				break;
			}
			float num2 = _position.x - ((float)prefabInstance.boundingBoxPosition.x + (float)prefabInstance.boundingBoxSize.x * 0.5f);
			float num3 = _position.z - ((float)prefabInstance.boundingBoxPosition.z + (float)prefabInstance.boundingBoxSize.z * 0.5f);
			if (num2 * num2 + num3 * num3 <= num)
			{
				Vector2 vector2;
				vector2.x = (float)prefabInstance.boundingBoxPosition.x;
				vector2.y = (float)prefabInstance.boundingBoxPosition.z;
				Vector2 vector3;
				vector3.x = vector2.x + (float)prefabInstance.boundingBoxSize.x;
				vector3.y = vector2.y;
				Vector2 vector4;
				vector4.x = vector2.x;
				vector4.y = vector2.y + (float)prefabInstance.boundingBoxSize.z;
				Vector2 a;
				a.x = vector3.x;
				a.y = vector4.y;
				if (!DynamicMeshManager.IsOutsideDistantTerrain(vector2.x, vector3.x, vector2.y, vector4.y))
				{
					Vector2 vector5 = vector2 - vector;
					if (Utils.FastMax(Utils.FastAbs(vector5.x), Utils.FastAbs(vector5.y)) < _nearDistance)
					{
						Vector2 vector6 = vector3 - vector;
						if (Utils.FastMax(Utils.FastAbs(vector6.x), Utils.FastAbs(vector6.y)) < _nearDistance)
						{
							Vector2 vector7 = vector4 - vector;
							if (Utils.FastMax(Utils.FastAbs(vector7.x), Utils.FastAbs(vector7.y)) < _nearDistance)
							{
								Vector2 vector8 = a - vector;
								if (Utils.FastMax(Utils.FastAbs(vector8.x), Utils.FastAbs(vector8.y)) < _nearDistance)
								{
									_prefabsNear.Add(prefabInstance.id, prefabInstance);
									goto IL_2A5;
								}
							}
						}
					}
					string text = (prefabInstance.prefab.distantPOIOverride == null) ? prefabInstance.prefab.PrefabName : prefabInstance.prefab.distantPOIOverride;
					bool flag;
					if (!this.prefabMeshExisting.TryGetValue(text, out flag))
					{
						flag = (PathAbstractions.PrefabImpostersSearchPaths.GetLocation(text, null, null).Type != PathAbstractions.EAbstractedLocationType.None);
						this.prefabMeshExisting[text] = flag;
					}
					if (flag)
					{
						_prefabsFar.Add(prefabInstance.id, prefabInstance);
					}
				}
			}
			IL_2A5:;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool ValidPrefabForQuest(EntityTrader trader, PrefabInstance prefab, FastTags<TagGroup.Global> questTag, List<Vector2> usedPOILocations = null, int entityIDforQuests = -1, BiomeFilterTypes biomeFilterType = BiomeFilterTypes.SameBiome, string biomeFilter = "")
	{
		if (!prefab.prefab.bSleeperVolumes || !prefab.prefab.GetQuestTag(questTag))
		{
			return false;
		}
		Vector2 vector = new Vector2((float)prefab.boundingBoxPosition.x, (float)prefab.boundingBoxPosition.z);
		if (usedPOILocations != null && usedPOILocations.Contains(vector))
		{
			return false;
		}
		ulong num;
		if (QuestEventManager.Current.CheckForPOILockouts(entityIDforQuests, vector, out num) != QuestEventManager.POILockoutReasonTypes.None)
		{
			return false;
		}
		new Vector2((float)prefab.boundingBoxPosition.x + (float)prefab.boundingBoxSize.x / 2f, (float)prefab.boundingBoxPosition.z + (float)prefab.boundingBoxSize.z / 2f);
		if (biomeFilterType != BiomeFilterTypes.AnyBiome)
		{
			string[] array = null;
			BiomeDefinition biomeAt = GameManager.Instance.World.ChunkCache.ChunkProvider.GetBiomeProvider().GetBiomeAt((int)vector.x, (int)vector.y);
			if (biomeFilterType == BiomeFilterTypes.OnlyBiome)
			{
				if (biomeAt.m_sBiomeName != biomeFilter)
				{
					return false;
				}
			}
			else if (biomeFilterType == BiomeFilterTypes.ExcludeBiome)
			{
				if (array == null)
				{
					array = biomeFilter.Split(',', StringSplitOptions.None);
				}
				bool flag = false;
				for (int i = 0; i < array.Length; i++)
				{
					if (biomeAt.m_sBiomeName == array[i])
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					return false;
				}
			}
			else if (biomeFilterType == BiomeFilterTypes.SameBiome && trader != null)
			{
				BiomeDefinition biomeAt2 = GameManager.Instance.World.ChunkCache.ChunkProvider.GetBiomeProvider().GetBiomeAt((int)trader.position.x, (int)trader.position.z);
				if (biomeAt != biomeAt2)
				{
					return false;
				}
			}
		}
		return true;
	}

	public virtual PrefabInstance GetRandomPOINearTrader(EntityTrader trader, FastTags<TagGroup.Global> questTag, byte difficulty, List<Vector2> usedPOILocations = null, int entityIDforQuests = -1, BiomeFilterTypes biomeFilterType = BiomeFilterTypes.SameBiome, string biomeFilter = "")
	{
		World world = GameManager.Instance.World;
		for (int i = 0; i < 3; i++)
		{
			List<PrefabInstance> prefabsForTrader = QuestEventManager.Current.GetPrefabsForTrader(trader.traderArea, (int)difficulty, i, world.GetGameRandom());
			if (prefabsForTrader != null)
			{
				for (int j = 0; j < prefabsForTrader.Count; j++)
				{
					PrefabInstance prefabInstance = prefabsForTrader[j];
					if (this.ValidPrefabForQuest(trader, prefabInstance, questTag, usedPOILocations, entityIDforQuests, biomeFilterType, biomeFilter))
					{
						return prefabInstance;
					}
				}
			}
		}
		return null;
	}

	public virtual PrefabInstance GetRandomPOINearWorldPos(Vector2 worldPos, int minSearchDistance, int maxSearchDistance, FastTags<TagGroup.Global> questTag, byte difficulty, List<Vector2> usedPOILocations = null, int entityIDforQuests = -1, BiomeFilterTypes biomeFilterType = BiomeFilterTypes.SameBiome, string biomeFilter = "")
	{
		List<PrefabInstance> prefabsByDifficultyTier = QuestEventManager.Current.GetPrefabsByDifficultyTier((int)difficulty);
		if (prefabsByDifficultyTier == null)
		{
			return null;
		}
		string[] array = null;
		BiomeDefinition biomeAt = GameManager.Instance.World.ChunkCache.ChunkProvider.GetBiomeProvider().GetBiomeAt((int)worldPos.x, (int)worldPos.y);
		World world = GameManager.Instance.World;
		for (int i = 0; i < 50; i++)
		{
			int index = world.GetGameRandom().RandomRange(prefabsByDifficultyTier.Count);
			PrefabInstance prefabInstance = prefabsByDifficultyTier[index];
			if (prefabInstance.prefab.bSleeperVolumes && prefabInstance.prefab.GetQuestTag(questTag) && prefabInstance.prefab.DifficultyTier == difficulty)
			{
				Vector2 vector = new Vector2((float)prefabInstance.boundingBoxPosition.x, (float)prefabInstance.boundingBoxPosition.z);
				ulong num;
				if ((usedPOILocations == null || !usedPOILocations.Contains(vector)) && QuestEventManager.Current.CheckForPOILockouts(entityIDforQuests, vector, out num) == QuestEventManager.POILockoutReasonTypes.None)
				{
					Vector2 b = new Vector2((float)prefabInstance.boundingBoxPosition.x + (float)prefabInstance.boundingBoxSize.x / 2f, (float)prefabInstance.boundingBoxPosition.z + (float)prefabInstance.boundingBoxSize.z / 2f);
					if (biomeFilterType != BiomeFilterTypes.AnyBiome)
					{
						BiomeDefinition biomeAt2 = GameManager.Instance.World.ChunkCache.ChunkProvider.GetBiomeProvider().GetBiomeAt((int)vector.x, (int)vector.y);
						if (biomeFilterType == BiomeFilterTypes.OnlyBiome)
						{
							if (biomeAt2.m_sBiomeName != biomeFilter)
							{
								goto IL_1F8;
							}
						}
						else if (biomeFilterType == BiomeFilterTypes.ExcludeBiome)
						{
							if (array == null)
							{
								array = biomeFilter.Split(',', StringSplitOptions.None);
							}
							bool flag = false;
							for (int j = 0; j < array.Length; j++)
							{
								if (biomeAt2.m_sBiomeName == array[j])
								{
									flag = true;
									break;
								}
							}
							if (flag)
							{
								goto IL_1F8;
							}
						}
						else if (biomeFilterType == BiomeFilterTypes.SameBiome && biomeAt2 != biomeAt)
						{
							goto IL_1F8;
						}
					}
					float sqrMagnitude = (worldPos - b).sqrMagnitude;
					if (sqrMagnitude < (float)maxSearchDistance && sqrMagnitude > (float)minSearchDistance)
					{
						return prefabInstance;
					}
				}
			}
			IL_1F8:;
		}
		return null;
	}

	public virtual PrefabInstance GetClosestPOIToWorldPos(FastTags<TagGroup.Global> questTag, Vector2 worldPos, List<Vector2> excludeList = null, int maxSearchDist = -1, bool ignoreCurrentPOI = false, BiomeFilterTypes biomeFilterType = BiomeFilterTypes.SameBiome, string biomeFilter = "")
	{
		PrefabInstance result = null;
		string[] array = null;
		Vector3 pos = new Vector3(worldPos.x, 0f, worldPos.y);
		float num = (maxSearchDist < 0) ? float.MaxValue : ((float)maxSearchDist);
		IBiomeProvider biomeProvider = GameManager.Instance.World.ChunkCache.ChunkProvider.GetBiomeProvider();
		BiomeDefinition biomeDefinition = (biomeProvider != null) ? biomeProvider.GetBiomeAt((int)worldPos.x, (int)worldPos.y) : null;
		for (int i = 0; i < this.poiPrefabs.Count; i++)
		{
			PrefabInstance prefabInstance = this.poiPrefabs[i];
			if (!prefabInstance.prefab.PrefabName.Contains("rwg_tile") && (prefabInstance.prefab.GetQuestTag(questTag) || questTag.IsEmpty))
			{
				if (ignoreCurrentPOI)
				{
					pos.y = (float)prefabInstance.boundingBoxPosition.y;
					if (prefabInstance.Overlaps(pos, 0f))
					{
						goto IL_1FE;
					}
				}
				Vector2 vector = new Vector2((float)prefabInstance.boundingBoxPosition.x + (float)prefabInstance.boundingBoxSize.x / 2f, (float)prefabInstance.boundingBoxPosition.z + (float)prefabInstance.boundingBoxSize.z / 2f);
				if (excludeList == null || !excludeList.Contains(new Vector2((float)prefabInstance.boundingBoxPosition.x, (float)prefabInstance.boundingBoxPosition.z)))
				{
					if (biomeFilterType != BiomeFilterTypes.AnyBiome)
					{
						BiomeDefinition biomeDefinition2 = (biomeProvider != null) ? biomeProvider.GetBiomeAt((int)vector.x, (int)vector.y) : null;
						if (biomeFilterType == BiomeFilterTypes.OnlyBiome)
						{
							if (biomeDefinition2.m_sBiomeName != biomeFilter)
							{
								goto IL_1FE;
							}
						}
						else if (biomeFilterType == BiomeFilterTypes.ExcludeBiome)
						{
							if (array == null)
							{
								array = biomeFilter.Split(',', StringSplitOptions.None);
							}
							bool flag = false;
							for (int j = 0; j < array.Length; j++)
							{
								if (biomeDefinition2.m_sBiomeName == array[j])
								{
									flag = true;
									break;
								}
							}
							if (flag)
							{
								goto IL_1FE;
							}
						}
						else if (biomeFilterType == BiomeFilterTypes.SameBiome && biomeDefinition2 != biomeDefinition)
						{
							goto IL_1FE;
						}
					}
					float sqrMagnitude = (worldPos - vector).sqrMagnitude;
					if (sqrMagnitude < num)
					{
						num = sqrMagnitude;
						result = prefabInstance;
					}
				}
			}
			IL_1FE:;
		}
		return result;
	}

	public virtual PrefabInstance GetPrefabFromWorldPos(int x, int z)
	{
		for (int i = 0; i < this.allPrefabs.Count; i++)
		{
			if (this.allPrefabs[i].boundingBoxPosition.x == x && this.allPrefabs[i].boundingBoxPosition.z == z && !this.allPrefabs[i].prefab.PrefabName.Contains("rwg_tile") && !this.allPrefabs[i].prefab.PrefabName.Contains("part_"))
			{
				return this.allPrefabs[i];
			}
		}
		return null;
	}

	public virtual PrefabInstance GetPrefabFromWorldPosInside(int _x, int _z)
	{
		for (int i = 0; i < this.allPrefabs.Count; i++)
		{
			PrefabInstance prefabInstance = this.allPrefabs[i];
			int x = prefabInstance.boundingBoxPosition.x;
			int z = prefabInstance.boundingBoxPosition.z;
			if (x <= _x && z <= _z && x + prefabInstance.boundingBoxSize.x >= _x && z + prefabInstance.boundingBoxSize.z >= _z)
			{
				return this.allPrefabs[i];
			}
		}
		return null;
	}

	public virtual PrefabInstance GetPrefabFromWorldPosInsideWithOffset(int _x, int _z, int _offset)
	{
		for (int i = 0; i < this.allPrefabs.Count; i++)
		{
			PrefabInstance prefabInstance = this.allPrefabs[i];
			int num = prefabInstance.boundingBoxPosition.x - _offset;
			int num2 = prefabInstance.boundingBoxPosition.z - _offset;
			int num3 = prefabInstance.boundingBoxPosition.x + prefabInstance.boundingBoxSize.x + _offset;
			int num4 = prefabInstance.boundingBoxPosition.z + prefabInstance.boundingBoxSize.z + _offset;
			if (num <= _x && num2 <= _z && num3 >= _x && num4 >= _z)
			{
				return this.allPrefabs[i];
			}
		}
		return null;
	}

	public virtual PrefabInstance GetPrefabFromWorldPosInside(int _x, int _y, int _z)
	{
		for (int i = 0; i < this.allPrefabs.Count; i++)
		{
			PrefabInstance prefabInstance = this.allPrefabs[i];
			int x = prefabInstance.boundingBoxPosition.x;
			int y = prefabInstance.boundingBoxPosition.y;
			int z = prefabInstance.boundingBoxPosition.z;
			if (x <= _x && y <= _y && z <= _z && x + prefabInstance.boundingBoxSize.x >= _x && y + prefabInstance.boundingBoxSize.y >= _y && z + prefabInstance.boundingBoxSize.z >= _z)
			{
				return this.allPrefabs[i];
			}
		}
		return null;
	}

	public virtual List<PrefabInstance> GetPrefabsFromWorldPosInside(Vector3 _pos, FastTags<TagGroup.Global> _questTags)
	{
		_pos += this.boundsPad;
		List<PrefabInstance> list = new List<PrefabInstance>();
		Bounds bounds = default(Bounds);
		for (int i = 0; i < this.allPrefabs.Count; i++)
		{
			PrefabInstance prefabInstance = this.allPrefabs[i];
			if (prefabInstance.prefab.GetQuestTag(_questTags))
			{
				bounds.SetMinMax(prefabInstance.boundingBoxPosition, prefabInstance.boundingBoxPosition + prefabInstance.boundingBoxSize - this.boundsPad);
				if (bounds.Contains(_pos))
				{
					list.AddRange(this.GetPrefabsIntersecting(prefabInstance));
				}
			}
		}
		return (from pi in list
		orderby pi.boundingBoxSize.x * pi.boundingBoxSize.z descending
		select pi).ToList<PrefabInstance>();
	}

	public virtual List<PrefabInstance> GetPrefabsIntersecting(PrefabInstance parentPI)
	{
		List<PrefabInstance> list = new List<PrefabInstance>();
		list.Add(parentPI);
		Bounds bounds = default(Bounds);
		bounds.SetMinMax(parentPI.boundingBoxPosition, parentPI.boundingBoxPosition + parentPI.boundingBoxSize - this.boundsPad);
		float num = bounds.size.x * bounds.size.z;
		Bounds bounds2 = default(Bounds);
		for (int i = 0; i < this.allPrefabs.Count; i++)
		{
			PrefabInstance prefabInstance = this.allPrefabs[i];
			if (parentPI != prefabInstance)
			{
				bounds2.SetMinMax(prefabInstance.boundingBoxPosition, prefabInstance.boundingBoxPosition + prefabInstance.boundingBoxSize - this.boundsPad);
				if (bounds.Intersects(bounds2) && bounds2.size.x * bounds2.size.z < num && !list.Contains(prefabInstance))
				{
					list.Add(prefabInstance);
				}
			}
		}
		return (from pi in list
		orderby pi.boundingBoxSize.x * pi.boundingBoxSize.z descending
		select pi).ToList<PrefabInstance>();
	}

	public void CopyPrefabHeightsIntoHeightMap(int _heightMapWidth, int _heightMapHeight, IBackedArray<ushort> _heightData, int _heightMapScale = 1, ushort[] _topTextures = null)
	{
		if (this.blockValueTerrainFiller.isair)
		{
			this.blockValueTerrainFiller = Block.GetBlockValue(Constants.cTerrainFillerBlockName, false);
			this.blockValueTerrainFiller2 = Block.GetBlockValue(Constants.cTerrainFiller2BlockName, false);
		}
		List<PrefabInstance> dynamicPrefabs = this.GetDynamicPrefabs();
		for (int i = 0; i < dynamicPrefabs.Count; i++)
		{
			PrefabInstance prefabInstance = dynamicPrefabs[i];
			if (prefabInstance.prefab != null)
			{
				this.copyPrefabsIntoHeightMap(prefabInstance, _heightMapWidth, _heightMapHeight, _heightData, _heightMapScale, _topTextures);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void copyPrefabsIntoHeightMap(PrefabInstance _pi, int _heightMapWidth, int _heightMapHeight, IBackedArray<ushort> _heightData, int _heightMapScale, ushort[] _topTextures = null)
	{
		using (IBackedArrayView<ushort> backedArrayView = BackedArrays.CreateSingleView<ushort>(_heightData, BackedArrayHandleMode.ReadWrite, 0, 0))
		{
			int rotation = (int)_pi.rotation;
			Prefab prefab = _pi.prefab;
			int yOffset = prefab.yOffset;
			Vector3i size = prefab.size;
			int x = _pi.boundingBoxPosition.x;
			int y = _pi.boundingBoxPosition.y;
			int z = _pi.boundingBoxPosition.z;
			if (_pi.boundingBoxPosition.x < -_heightMapWidth / 2 || _pi.boundingBoxPosition.x + size.x > _heightMapWidth / 2 || _pi.boundingBoxPosition.z < -_heightMapHeight / 2 || _pi.boundingBoxPosition.z + size.z > _heightMapHeight / 2)
			{
				Log.Warning(string.Format("Prefab {0} outside of the world bounds (position {1})", _pi.name, _pi.boundingBoxPosition));
			}
			bool flag = _pi.name.Contains("rwg_tile");
			for (int i = (size.z + _heightMapScale - 1) % _heightMapScale; i < size.z; i += _heightMapScale)
			{
				int num = i + z;
				int num2 = (num / _heightMapScale + _heightMapHeight / 2) * _heightMapWidth;
				int num3 = (num + _heightMapHeight / 2) * _heightMapScale * _heightMapWidth;
				for (int j = (size.x + _heightMapScale - 1) % _heightMapScale; j < size.x; j += _heightMapScale)
				{
					int num4 = j + x;
					int num5 = (num4 / _heightMapScale + _heightMapWidth / 2 + num2) % _heightData.Length;
					int num6 = (num4 + _heightMapWidth / 2) * _heightMapScale + num3;
					int k = 0;
					while (k < size.y)
					{
						BlockValue blockNoDamage = prefab.GetBlockNoDamage(rotation, j, k, i);
						WaterValue water = prefab.GetWater(j, k, i);
						Block block = blockNoDamage.Block;
						if (!blockNoDamage.isair && block != null && block.shape.IsTerrain() && !water.HasMass())
						{
							goto IL_1AE;
						}
						if (k > -yOffset)
						{
							break;
						}
						if (flag && k <= 0)
						{
							goto IL_1AE;
						}
						IL_2A5:
						k++;
						continue;
						IL_1AE:
						sbyte density = prefab.GetDensity(rotation, j, k, i);
						float num7 = (float)(y + k + yOffset);
						num7 += (float)(-(float)density) / 128f - 1f;
						if (flag)
						{
							num7 -= (float)yOffset;
						}
						if (num7 <= 0f)
						{
							goto IL_2A5;
						}
						ushort num8 = (ushort)(num7 / 0.00389105058f);
						if (blockNoDamage.type == this.blockValueTerrainFiller2.type && num8 > backedArrayView[num5])
						{
							goto IL_2A5;
						}
						if (num5 >= 0 && num5 < _heightData.Length && (flag || num8 > backedArrayView[num5]))
						{
							backedArrayView[num5] = num8;
						}
						if (block == null || _topTextures == null || blockNoDamage.isair || blockNoDamage.type == this.blockValueTerrainFiller.type || blockNoDamage.type == this.blockValueTerrainFiller2.type)
						{
							goto IL_2A5;
						}
						int sideTextureId = block.GetSideTextureId(blockNoDamage, BlockFace.Top);
						if (num6 >= 0 && num6 < _topTextures.Length)
						{
							_topTextures[num6] = (ushort)sideTextureId;
							goto IL_2A5;
						}
						goto IL_2A5;
					}
				}
			}
		}
	}

	public void CalculateStats(out int basePrefabCount, out int rotatedPrefabsCount, out int activePrefabCount, out int basePrefabBytes, out int rotatedPrefabBytes, out int activePrefabBytes)
	{
		ChunkCluster chunkCache = GameManager.Instance.World.ChunkCache;
		Dictionary<string, Prefab> obj = this.prefabCache;
		lock (obj)
		{
			basePrefabCount = this.prefabCache.Count;
			basePrefabBytes = 0;
			foreach (Prefab prefab in this.prefabCache.Values)
			{
				basePrefabBytes += prefab.EstimateOwnedBytes();
			}
			rotatedPrefabsCount = 0;
			rotatedPrefabBytes = 0;
			foreach (Prefab[] array in this.prefabCacheRotations.Values)
			{
				for (int i = 1; i < array.Length; i++)
				{
					Prefab prefab2 = array[i];
					if (prefab2 != null)
					{
						rotatedPrefabsCount++;
						rotatedPrefabBytes += prefab2.EstimateOwnedBytes();
					}
				}
			}
		}
		List<EntityPlayer> list = GameManager.Instance.World.Players.list;
		HashSet<Prefab> hashSet = new HashSet<Prefab>();
		List<PrefabInstance> list2 = new List<PrefabInstance>();
		foreach (EntityPlayer entityPlayer in list)
		{
			foreach (long key in entityPlayer.ChunkObserver.chunksAround.list)
			{
				IChunk chunkSync = chunkCache.GetChunkSync(key);
				if (chunkSync != null)
				{
					Vector3i worldPos = chunkSync.GetWorldPos();
					this.GetPrefabsAtXZ(worldPos.x, worldPos.x + 15, worldPos.z, worldPos.z + 15, list2);
					foreach (PrefabInstance prefabInstance in list2)
					{
						hashSet.Add(prefabInstance.prefab);
					}
				}
			}
		}
		activePrefabCount = hashSet.Count;
		activePrefabBytes = 0;
		foreach (Prefab prefab3 in hashSet)
		{
			activePrefabBytes += prefab3.EstimateOwnedBytes();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cPrefabMaxRadius = 200;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, Prefab> prefabCache = new Dictionary<string, Prefab>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, Prefab[]> prefabCacheRotations = new Dictionary<string, Prefab[]>();

	public List<PrefabInstance> allPrefabs = new List<PrefabInstance>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<PrefabInstance> poiPrefabs = new List<PrefabInstance>();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isSortNeeded = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<PrefabInstance> allPrefabsSorted = new List<PrefabInstance>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<TraderArea> traderAreas = new List<TraderArea>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int id;

	public PrefabInstance ActivePrefab;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, bool> prefabMeshExisting = new Dictionary<string, bool>();

	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Poi> streetTileTag = FastTags<TagGroup.Poi>.Parse("streettile");

	public int ProtectSizeXMax;

	[PublicizedFrom(EAccessModifier.Private)]
	public DynamicPrefabDecorator.TraderComparer traderComparer = new DynamicPrefabDecorator.TraderComparer();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<PrefabInstance> decorateChunkPIs = new List<PrefabInstance>();

	public static int PrefabPreviewLimit;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Vector3 boundsPad = new Vector3(0.001f, 0.001f, 0.001f);

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockValue blockValueTerrainFiller;

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockValue blockValueTerrainFiller2;

	public class TraderComparer : IComparer<TraderArea>
	{
		public int Compare(TraderArea _ta1, TraderArea _ta2)
		{
			return _ta1.ProtectPosition.x - _ta2.ProtectPosition.x;
		}
	}
}

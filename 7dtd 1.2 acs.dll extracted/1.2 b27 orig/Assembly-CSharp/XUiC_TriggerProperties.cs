using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_TriggerProperties : XUiController, ISelectionBoxCallback
{
	public Vector3i BlockPos
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.blockPos;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			this.blockPos = value;
			this.triggerVolume = null;
			this.SetupTrigger();
		}
	}

	public Prefab.PrefabTriggerVolume TriggerVolume
	{
		get
		{
			return this.triggerVolume;
		}
		set
		{
			this.triggerVolume = value;
			this.blockTrigger = null;
		}
	}

	public List<byte> TriggersIndices
	{
		get
		{
			if (this.blockTrigger != null)
			{
				return this.blockTrigger.TriggersIndices;
			}
			if (this.triggerVolume != null)
			{
				return this.triggerVolume.TriggersIndices;
			}
			return null;
		}
	}

	public List<byte> TriggeredByIndices
	{
		get
		{
			if (this.blockTrigger != null)
			{
				return this.blockTrigger.TriggeredByIndices;
			}
			return null;
		}
	}

	public Prefab Prefab
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.prefab;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (value != this.prefab)
			{
				this.prefab = value;
				this.triggersList.EditPrefab = value;
				this.triggersList.Owner = this;
				this.triggersList.IsTriggers = true;
				this.triggeredByList.EditPrefab = value;
				this.triggeredByList.Owner = this;
				this.triggeredByList.IsTriggers = false;
				if (this.prefab.TriggerLayers.Count == 0)
				{
					this.prefab.AddInitialTriggerLayers();
				}
			}
		}
	}

	public override void Init()
	{
		base.Init();
		XUiC_TriggerProperties.ID = base.WindowGroup.ID;
		this.triggersList = (base.GetChildById("triggers") as XUiC_PrefabTriggerEditorList);
		if (this.triggersList != null)
		{
			this.triggersList.SelectionChanged += this.TriggersList_SelectionChanged;
		}
		XUiController childById = base.GetChildById("addTriggersButton");
		if (childById != null)
		{
			childById.OnPress += this.HandleAddTriggersEntry;
		}
		this.triggeredByList = (base.GetChildById("triggeredBy") as XUiC_PrefabTriggerEditorList);
		if (this.triggeredByList != null)
		{
			this.triggeredByList.SelectionChanged += this.TriggeredByList_SelectionChanged;
		}
		XUiController childById2 = base.GetChildById("addTriggeredByButton");
		if (childById2 != null)
		{
			childById2.OnPress += this.HandleAddTriggeredByEntry;
		}
		XUiController childById3 = base.GetChildById("exclude");
		if (childById3 != null)
		{
			childById3.OnPress += this.triggerExclude_OnPressed;
		}
		childById3 = base.GetChildById("operation");
		if (childById3 != null)
		{
			childById3.OnPress += this.triggerOperation_OnPressed;
		}
		childById3 = base.GetChildById("unlock");
		if (childById3 != null)
		{
			childById3.OnPress += this.triggerUnlock_OnPressed;
		}
		if (SelectionBoxManager.Instance != null)
		{
			SelectionBoxManager.Instance.GetCategory("TriggerVolume").SetCallback(this);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void triggerExclude_OnPressed(XUiController controller, int button)
	{
		if (this.blockTrigger != null)
		{
			this.blockTrigger.ExcludeIcon = !this.blockTrigger.ExcludeIcon;
			Chunk chunkModified = (Chunk)GameManager.Instance.World.ChunkClusters[this.clrIdx].GetChunkSync(World.toChunkXZ(this.blockPos.x), this.blockPos.y, World.toChunkXZ(this.blockPos.z));
			this.setChunkModified(chunkModified);
			base.RefreshBindings(false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void triggerOperation_OnPressed(XUiController controller, int button)
	{
		if (this.blockTrigger != null)
		{
			this.blockTrigger.UseOrForMultipleTriggers = !this.blockTrigger.UseOrForMultipleTriggers;
			Chunk chunkModified = (Chunk)GameManager.Instance.World.ChunkClusters[this.clrIdx].GetChunkSync(World.toChunkXZ(this.blockPos.x), this.blockPos.y, World.toChunkXZ(this.blockPos.z));
			this.setChunkModified(chunkModified);
			base.RefreshBindings(false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void triggerUnlock_OnPressed(XUiController controller, int button)
	{
		if (this.blockTrigger != null)
		{
			this.blockTrigger.Unlock = !this.blockTrigger.Unlock;
			Chunk chunkModified = (Chunk)GameManager.Instance.World.ChunkClusters[this.clrIdx].GetChunkSync(World.toChunkXZ(this.blockPos.x), this.blockPos.y, World.toChunkXZ(this.blockPos.z));
			this.setChunkModified(chunkModified);
			base.RefreshBindings(false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setChunkModified(Chunk _chunk)
	{
		PrefabEditModeManager.Instance.NeedsSaving = true;
		_chunk.isModified = true;
	}

	public override void Cleanup()
	{
		base.Cleanup();
		if (SelectionBoxManager.Instance != null)
		{
			SelectionBoxManager.Instance.GetCategory("TriggerVolume").SetCallback(null);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleAddTriggersEntry(XUiController _sender, int _mouseButton)
	{
		this.TriggerOnAddTriggersPressed();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleAddTriggeredByEntry(XUiController _sender, int _mouseButton)
	{
		this.TriggerOnAddTriggersPressed();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnOpenInEditor_OnOnPressed(XUiController _sender, int _mouseButton)
	{
		Process.Start(this.prefab.location.FullPathNoExtension + ".xml");
		base.xui.playerUI.windowManager.Close(this.windowGroup.ID);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TriggerOnAddTriggersPressed()
	{
		this.prefab.AddNewTriggerLayer();
		this.triggersList.RebuildList(false);
		this.triggeredByList.RebuildList(false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool validTriggerName(byte val)
	{
		return !this.prefab.TriggerLayers.Contains(val);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TriggersList_SelectionChanged(XUiC_ListEntry<XUiC_PrefabTriggerEditorList.PrefabTriggerEntry> _previousEntry, XUiC_ListEntry<XUiC_PrefabTriggerEditorList.PrefabTriggerEntry> _newEntry)
	{
		if (_newEntry != null)
		{
			byte triggerLayer = 0;
			if (StringParsers.TryParseUInt8(_newEntry.GetEntry().name, out triggerLayer, 0, -1, NumberStyles.Integer))
			{
				if (this.triggerVolume != null)
				{
					this.HandleTriggersSetting(triggerLayer, true, GameManager.Instance.World);
				}
				else
				{
					this.HandleTriggersSetting(triggerLayer, true, GameManager.Instance.World, this.clrIdx, this.blockPos);
				}
			}
			_newEntry.IsDirty = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TriggeredByList_SelectionChanged(XUiC_ListEntry<XUiC_PrefabTriggerEditorList.PrefabTriggerEntry> _previousEntry, XUiC_ListEntry<XUiC_PrefabTriggerEditorList.PrefabTriggerEntry> _newEntry)
	{
		if (_newEntry != null)
		{
			byte triggerLayer = 0;
			if (StringParsers.TryParseUInt8(_newEntry.GetEntry().name, out triggerLayer, 0, -1, NumberStyles.Integer))
			{
				if (this.triggerVolume != null)
				{
					this.HandleTriggersSetting(triggerLayer, false, GameManager.Instance.World);
				}
				else
				{
					this.HandleTriggersSetting(triggerLayer, false, GameManager.Instance.World, this.clrIdx, this.blockPos);
				}
			}
			_newEntry.IsDirty = true;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.IsDirty = true;
	}

	public override void OnClose()
	{
		base.OnClose();
		this.prefab = null;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.IsDirty)
		{
			base.RefreshBindings(false);
			this.IsDirty = false;
		}
	}

	public override bool GetBindingValue(ref string _value, string _bindingName)
	{
		uint num = <PrivateImplementationDetails>.ComputeStringHash(_bindingName);
		if (num <= 1452740656U)
		{
			if (num != 930054621U)
			{
				if (num != 1012116031U)
				{
					if (num == 1452740656U)
					{
						if (_bindingName == "excludeTickmarkSelected")
						{
							_value = ((this.blockTrigger != null) ? this.blockTrigger.ExcludeIcon.ToString() : "false");
							return true;
						}
					}
				}
				else if (_bindingName == "operationTickmarkSelected")
				{
					_value = ((this.blockTrigger != null) ? this.blockTrigger.UseOrForMultipleTriggers.ToString() : "false");
					return true;
				}
			}
			else if (_bindingName == "triggeredby_enabled")
			{
				_value = this.ShowTriggeredBy.ToString();
				return true;
			}
		}
		else if (num <= 2194844717U)
		{
			if (num != 1868194796U)
			{
				if (num == 2194844717U)
				{
					if (_bindingName == "window_height")
					{
						_value = ((this.ShowTriggeredBy && this.ShowTriggers) ? "752" : "396");
						return true;
					}
				}
			}
			else if (_bindingName == "unlockTickmarkSelected")
			{
				_value = ((this.blockTrigger != null) ? this.blockTrigger.Unlock.ToString() : "false");
				return true;
			}
		}
		else if (num != 2556802313U)
		{
			if (num == 3933558790U)
			{
				if (_bindingName == "triggers_enabled")
				{
					_value = this.ShowTriggers.ToString();
					return true;
				}
			}
		}
		else if (_bindingName == "title")
		{
			_value = Localization.Get("xuiPrefabProperties", false) + ": " + ((this.prefab != null) ? this.prefab.PrefabName : "-");
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupTrigger()
	{
		Chunk chunk = (Chunk)GameManager.Instance.World.ChunkClusters[this.clrIdx].GetChunkSync(World.toChunkXZ(this.blockPos.x), this.blockPos.y, World.toChunkXZ(this.blockPos.z));
		this.blockTrigger = chunk.GetBlockTrigger(World.toBlock(this.blockPos));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void HandleTriggersSetting(byte triggerLayer, bool isTriggers, World _world, int _cIdx, Vector3i _blockPos)
	{
		if (_world.IsEditor())
		{
			Chunk chunk = (Chunk)_world.ChunkClusters[_cIdx].GetChunkSync(World.toChunkXZ(_blockPos.x), _blockPos.y, World.toChunkXZ(_blockPos.z));
			this.blockTrigger = chunk.GetBlockTrigger(World.toBlock(_blockPos));
			if (triggerLayer == 0)
			{
				if (this.blockTrigger != null)
				{
					chunk.RemoveBlockTrigger(this.blockTrigger);
					this.blockTrigger = null;
				}
			}
			else
			{
				if (this.blockTrigger == null)
				{
					this.blockTrigger = new BlockTrigger(chunk);
					if (isTriggers)
					{
						if (this.blockTrigger.HasTriggers(triggerLayer))
						{
							this.blockTrigger.RemoveTriggersFlag(triggerLayer);
						}
						else
						{
							this.blockTrigger.SetTriggersFlag(triggerLayer);
						}
					}
					else if (this.blockTrigger.HasTriggeredBy(triggerLayer))
					{
						this.blockTrigger.RemoveTriggeredByFlag(triggerLayer);
					}
					else
					{
						this.blockTrigger.SetTriggeredByFlag(triggerLayer);
					}
					this.blockTrigger.LocalChunkPos = World.toBlock(_blockPos);
					chunk.AddBlockTrigger(this.blockTrigger);
				}
				else if (isTriggers)
				{
					if (this.blockTrigger.HasTriggers(triggerLayer))
					{
						this.blockTrigger.RemoveTriggersFlag(triggerLayer);
					}
					else
					{
						this.blockTrigger.SetTriggersFlag(triggerLayer);
					}
				}
				else if (this.blockTrigger.HasTriggeredBy(triggerLayer))
				{
					this.blockTrigger.RemoveTriggeredByFlag(triggerLayer);
				}
				else
				{
					this.blockTrigger.SetTriggeredByFlag(triggerLayer);
				}
				if (!this.blockTrigger.HasAnyTriggers() && !this.blockTrigger.HasAnyTriggeredBy() && this.blockTrigger != null)
				{
					chunk.RemoveBlockTrigger(this.blockTrigger);
					this.blockTrigger = null;
				}
				this.setChunkModified(chunk);
			}
			if (this.blockTrigger != null)
			{
				this.blockTrigger.TriggerUpdated(null);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void HandleTriggersSetting(byte triggerLayer, bool isTriggers, World _world)
	{
		if (_world.IsEditor())
		{
			if (isTriggers)
			{
				if (this.triggerVolume.HasTriggers(triggerLayer))
				{
					this.triggerVolume.RemoveTriggersFlag(triggerLayer);
				}
				else
				{
					this.triggerVolume.SetTriggersFlag(triggerLayer);
				}
			}
			if (this.blockTrigger != null)
			{
				this.blockTrigger.TriggerUpdated(null);
			}
		}
	}

	public static void Show(XUi _xui, int _clrIdx, Vector3i _blockPos, bool _showTriggers, bool _showTriggeredBy)
	{
		XUiC_TriggerProperties childByType = ((XUiWindowGroup)_xui.playerUI.windowManager.GetWindow(XUiC_TriggerProperties.ID)).Controller.GetChildByType<XUiC_TriggerProperties>();
		childByType.Prefab = PrefabEditModeManager.Instance.VoxelPrefab;
		childByType.clrIdx = _clrIdx;
		childByType.BlockPos = _blockPos;
		childByType.ShowTriggers = _showTriggers;
		childByType.ShowTriggeredBy = _showTriggeredBy;
		childByType.RefreshBindings(false);
		_xui.playerUI.windowManager.Open(XUiC_TriggerProperties.ID, true, false, true);
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
			this.selectedPrefabInstance = null;
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
				this.selectedPrefabInstance = PrefabSleeperVolumeManager.Instance.GetPrefabInstance(_prefabInstanceId);
				this.Prefab = this.selectedPrefabInstance.prefab;
				return true;
			}
		}
		return false;
	}

	public void OnSelectionBoxMoved(string _category, string _name, Vector3 _moveVector)
	{
		if (this.selectedPrefabInstance == null)
		{
			return;
		}
		Prefab.PrefabTriggerVolume prefabTriggerVolume = new Prefab.PrefabTriggerVolume(this.selectedPrefabInstance.prefab.TriggerVolumes[this.selIdx]);
		prefabTriggerVolume.startPos += new Vector3i(_moveVector);
		PrefabTriggerVolumeManager.Instance.UpdateTriggerPropertiesServer(this.selectedPrefabInstance.id, this.selIdx, prefabTriggerVolume, false);
	}

	public void OnSelectionBoxSized(string _category, string _name, int _dTop, int _dBottom, int _dNorth, int _dSouth, int _dEast, int _dWest)
	{
		if (this.selectedPrefabInstance == null)
		{
			return;
		}
		Prefab.PrefabTriggerVolume prefabTriggerVolume = new Prefab.PrefabTriggerVolume(this.selectedPrefabInstance.prefab.TriggerVolumes[this.selIdx]);
		prefabTriggerVolume.size += new Vector3i(_dEast + _dWest, _dTop + _dBottom, _dNorth + _dSouth);
		prefabTriggerVolume.startPos += new Vector3i(-_dWest, -_dBottom, -_dSouth);
		Vector3i size = prefabTriggerVolume.size;
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
		prefabTriggerVolume.size = size;
		PrefabTriggerVolumeManager.Instance.UpdateTriggerPropertiesServer(this.selectedPrefabInstance.id, this.selIdx, prefabTriggerVolume, false);
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
			Prefab.PrefabTriggerVolume volumeSettings = new Prefab.PrefabTriggerVolume(this.selectedPrefabInstance.prefab.TriggerVolumes[num2]);
			PrefabTriggerVolumeManager.Instance.UpdateTriggerPropertiesServer(this.selectedPrefabInstance.id, num2, volumeSettings, true);
			return true;
		}
		return false;
	}

	public bool OnSelectionBoxIsAvailable(string _category, EnumSelectionBoxAvailabilities _criteria)
	{
		return _criteria == EnumSelectionBoxAvailabilities.CanShowProperties || _criteria == EnumSelectionBoxAvailabilities.CanResize;
	}

	public void OnSelectionBoxShowProperties(bool _bVisible, GUIWindowManager _windowManager)
	{
		string text;
		string name;
		int num;
		int index;
		if (SelectionBoxManager.Instance.GetSelected(out text, out name) && text.Equals("TriggerVolume") && this.getPrefabIdAndVolumeId(name, out num, out index))
		{
			Prefab.PrefabTriggerVolume prefabTriggerVolume = this.selectedPrefabInstance.prefab.TriggerVolumes[index];
			this.ShowTriggers = true;
			this.ShowTriggeredBy = false;
			this.TriggerVolume = prefabTriggerVolume;
			_windowManager.SwitchVisible(XUiC_TriggerProperties.ID, false);
		}
	}

	public void OnSelectionBoxRotated(string _category, string _name)
	{
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_PrefabTriggerEditorList triggersList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_PrefabTriggerEditorList triggeredByList;

	[PublicizedFrom(EAccessModifier.Private)]
	public Prefab prefab;

	[PublicizedFrom(EAccessModifier.Private)]
	public int clrIdx;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i blockPos;

	public BlockTrigger blockTrigger;

	[PublicizedFrom(EAccessModifier.Private)]
	public Prefab.PrefabTriggerVolume triggerVolume;

	public bool ShowTriggers = true;

	public bool ShowTriggeredBy = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public int selIdx;

	[PublicizedFrom(EAccessModifier.Private)]
	public PrefabInstance selectedPrefabInstance;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<string> prefabGroupsList = new List<string>();
}

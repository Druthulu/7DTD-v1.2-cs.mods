using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_WoPropsSleeperVolume : XUiController, ISelectionBoxCallback
{
	public static int selectedVolumeIndex
	{
		get
		{
			if (XUiC_WoPropsSleeperVolume.instance != null && XUiC_WoPropsSleeperVolume.instance.m_selectedPrefabInstance != null)
			{
				return XUiC_WoPropsSleeperVolume.instance.selIdx;
			}
			return -1;
		}
	}

	public static PrefabInstance selectedPrefabInstance
	{
		get
		{
			if (XUiC_WoPropsSleeperVolume.instance != null)
			{
				return XUiC_WoPropsSleeperVolume.instance.m_selectedPrefabInstance;
			}
			return null;
		}
	}

	public List<byte> TriggeredByIndices
	{
		get
		{
			if (this.m_selectedPrefabInstance != null)
			{
				return this.m_selectedPrefabInstance.prefab.SleeperVolumes[this.selIdx].triggeredByIndices;
			}
			return null;
		}
	}

	public override void Init()
	{
		base.Init();
		XUiC_WoPropsSleeperVolume.ID = base.WindowGroup.ID;
		XUiC_WoPropsSleeperVolume.instance = this;
		this.labelIndex = (base.GetChildById("labelIndex").ViewComponent as XUiV_Label);
		this.labelPosition = (base.GetChildById("labelPosition").ViewComponent as XUiV_Label);
		this.labelSize = (base.GetChildById("labelSize").ViewComponent as XUiV_Label);
		this.labelSleeperCount = (base.GetChildById("labelSleeperCount").ViewComponent as XUiV_Label);
		this.labelGroup = (base.GetChildById("labelGroup").ViewComponent as XUiV_Label);
		this.txtGroupId = (XUiC_TextInput)base.GetChildById("groupId");
		this.txtGroupId.OnChangeHandler += this.TxtGroupId_OnChangeHandler;
		this.cbxPriority = (XUiC_ComboBoxBool)base.GetChildById("cbxPriority");
		this.cbxPriority.OnValueChanged += this.CbxPriority_OnValueChanged;
		this.cbxQuestExclude = (XUiC_ComboBoxBool)base.GetChildById("cbxQuestExclude");
		this.cbxQuestExclude.OnValueChanged += this.CbxQuestExclude_OnValueChanged;
		this.cbxCountPreset = (XUiC_ComboBoxList<XUiC_WoPropsSleeperVolume.CountPreset>)base.GetChildById("cbxCountPreset");
		this.cbxCountPreset.OnValueChanged += this.CbxCountPreset_OnValueChanged;
		this.cbxCountPreset.Elements.Add(new XUiC_WoPropsSleeperVolume.CountPreset(-1, -1, "Custom"));
		this.cbxCountPreset.Elements.Add(new XUiC_WoPropsSleeperVolume.CountPreset(1, 2, "12"));
		this.cbxCountPreset.Elements.Add(new XUiC_WoPropsSleeperVolume.CountPreset(2, 3, "23"));
		this.cbxCountPreset.Elements.Add(new XUiC_WoPropsSleeperVolume.CountPreset(3, 4, "34"));
		this.cbxCountPreset.Elements.Add(new XUiC_WoPropsSleeperVolume.CountPreset(4, 5, "45"));
		this.cbxCountPreset.Elements.Add(new XUiC_WoPropsSleeperVolume.CountPreset(5, 6, "56"));
		this.cbxCountPreset.Elements.Add(new XUiC_WoPropsSleeperVolume.CountPreset(6, 7, "67"));
		this.cbxCountPreset.Elements.Add(new XUiC_WoPropsSleeperVolume.CountPreset(7, 8, "78"));
		this.cbxCountPreset.Elements.Add(new XUiC_WoPropsSleeperVolume.CountPreset(8, 9, "89"));
		this.cbxCountPreset.Elements.Add(new XUiC_WoPropsSleeperVolume.CountPreset(9, 10, "910"));
		this.cbxCountPreset.MinIndex = 1;
		this.txtSpawnMin = (XUiC_TextInput)base.GetChildById("spawnMin");
		this.txtSpawnMin.OnChangeHandler += this.TxtSpawnMin_OnChangeHandler;
		this.txtSpawnMax = (XUiC_TextInput)base.GetChildById("spawnMax");
		this.txtSpawnMax.OnChangeHandler += this.TxtSpawnMax_OnChangeHandler;
		this.cbxTrigger = (XUiC_ComboBoxEnum<SleeperVolume.ETriggerType>)base.GetChildById("cbxTrigger");
		this.cbxTrigger.OnValueChanged += this.CbxTrigger_OnValueChanged;
		this.txtMinScript = (XUiC_TextInput)base.GetChildById("script");
		this.txtMinScript.OnChangeHandler += this.TxtMinScript_OnChangeHandler;
		this.spawnersList = (XUiC_SpawnersList)base.GetChildById("spawners");
		this.spawnersList.SelectionChanged += this.SpawnersList_SelectionChanged;
		this.spawnersList.SelectableEntries = false;
		this.triggeredByTitle = (base.GetChildById("triggeredByTitle").ViewComponent as XUiV_Label);
		this.triggeredByList = (base.GetChildById("triggeredBy") as XUiC_PrefabTriggerEditorList);
		if (this.triggeredByList != null)
		{
			this.triggeredByList.SelectionChanged += this.TriggeredByList_SelectionChanged;
		}
		XUiController childById = base.GetChildById("addTriggeredByButton");
		if (childById != null)
		{
			childById.OnPress += this.HandleAddTriggeredByEntry;
		}
		if (SelectionBoxManager.Instance != null)
		{
			SelectionBoxManager.Instance.GetCategory("SleeperVolume").SetCallback(this);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleAddTriggeredByEntry(XUiController _sender, int _mouseButton)
	{
		this.TriggerOnAddTriggersPressed();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TxtGroupId_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		if (!_changeFromCode && _text.Length > 0 && this.m_selectedPrefabInstance != null)
		{
			Prefab.PrefabSleeperVolume prefabSleeperVolume = new Prefab.PrefabSleeperVolume(this.m_selectedPrefabInstance.prefab.SleeperVolumes[this.selIdx]);
			short groupId = StringParsers.ParseSInt16(_text, 0, -1, NumberStyles.Integer);
			prefabSleeperVolume.groupId = groupId;
			PrefabSleeperVolumeManager.Instance.UpdateSleeperPropertiesServer(this.m_selectedPrefabInstance.id, this.selIdx, prefabSleeperVolume);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxPriority_OnValueChanged(XUiController _sender, bool _oldValue, bool _newValue)
	{
		if (this.m_selectedPrefabInstance != null)
		{
			Prefab.PrefabSleeperVolume prefabSleeperVolume = new Prefab.PrefabSleeperVolume(this.m_selectedPrefabInstance.prefab.SleeperVolumes[this.selIdx]);
			prefabSleeperVolume.isPriority = _newValue;
			PrefabSleeperVolumeManager.Instance.UpdateSleeperPropertiesServer(this.m_selectedPrefabInstance.id, this.selIdx, prefabSleeperVolume);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxQuestExclude_OnValueChanged(XUiController _sender, bool _oldValue, bool _newValue)
	{
		if (this.m_selectedPrefabInstance != null)
		{
			Prefab.PrefabSleeperVolume prefabSleeperVolume = new Prefab.PrefabSleeperVolume(this.m_selectedPrefabInstance.prefab.SleeperVolumes[this.selIdx]);
			prefabSleeperVolume.isQuestExclude = _newValue;
			PrefabSleeperVolumeManager.Instance.UpdateSleeperPropertiesServer(this.m_selectedPrefabInstance.id, this.selIdx, prefabSleeperVolume);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int FindCountPresetIndex(int _min, int _max)
	{
		for (int i = 0; i < this.cbxCountPreset.Elements.Count; i++)
		{
			if ((int)this.cbxCountPreset.Elements[i].min == _min && (int)this.cbxCountPreset.Elements[i].max == _max)
			{
				return i;
			}
		}
		return -1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateCountPresetLabel()
	{
		if (this.m_selectedPrefabInstance == null)
		{
			return;
		}
		Prefab.PrefabSleeperVolume prefabSleeperVolume = this.m_selectedPrefabInstance.prefab.SleeperVolumes[this.selIdx];
		int num = this.FindCountPresetIndex((int)prefabSleeperVolume.spawnCountMin, (int)prefabSleeperVolume.spawnCountMax);
		if (num < 0)
		{
			this.cbxCountPreset.MinIndex = 0;
			this.cbxCountPreset.SelectedIndex = 0;
			return;
		}
		this.cbxCountPreset.MinIndex = 1;
		this.cbxCountPreset.SelectedIndex = num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxCountPreset_OnValueChanged(XUiController _sender, XUiC_WoPropsSleeperVolume.CountPreset _oldvalue, XUiC_WoPropsSleeperVolume.CountPreset _newvalue)
	{
		this.cbxCountPreset.MinIndex = 1;
		if (this.m_selectedPrefabInstance != null)
		{
			Prefab.PrefabSleeperVolume prefabSleeperVolume = new Prefab.PrefabSleeperVolume(this.m_selectedPrefabInstance.prefab.SleeperVolumes[this.selIdx]);
			prefabSleeperVolume.spawnCountMin = _newvalue.min;
			prefabSleeperVolume.spawnCountMax = _newvalue.max;
			PrefabSleeperVolumeManager.Instance.UpdateSleeperPropertiesServer(this.m_selectedPrefabInstance.id, this.selIdx, prefabSleeperVolume);
		}
		this.UpdateCountPresetLabel();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TxtSpawnMin_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		if (!_changeFromCode && _text.Length > 0)
		{
			short spawnCountMin = StringParsers.ParseSInt16(_text, 0, -1, NumberStyles.Integer);
			if (this.m_selectedPrefabInstance != null)
			{
				Prefab.PrefabSleeperVolume prefabSleeperVolume = new Prefab.PrefabSleeperVolume(this.m_selectedPrefabInstance.prefab.SleeperVolumes[this.selIdx]);
				prefabSleeperVolume.spawnCountMin = spawnCountMin;
				PrefabSleeperVolumeManager.Instance.UpdateSleeperPropertiesServer(this.m_selectedPrefabInstance.id, this.selIdx, prefabSleeperVolume);
				this.UpdateCountPresetLabel();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TxtSpawnMax_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		if (!_changeFromCode && _text.Length > 0)
		{
			short spawnCountMax = StringParsers.ParseSInt16(_text, 0, -1, NumberStyles.Integer);
			if (this.m_selectedPrefabInstance != null)
			{
				Prefab.PrefabSleeperVolume prefabSleeperVolume = new Prefab.PrefabSleeperVolume(this.m_selectedPrefabInstance.prefab.SleeperVolumes[this.selIdx]);
				prefabSleeperVolume.spawnCountMax = spawnCountMax;
				PrefabSleeperVolumeManager.Instance.UpdateSleeperPropertiesServer(this.m_selectedPrefabInstance.id, this.selIdx, prefabSleeperVolume);
				this.UpdateCountPresetLabel();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxTrigger_OnValueChanged(XUiController _sender, SleeperVolume.ETriggerType _oldValue, SleeperVolume.ETriggerType _newValue)
	{
		if (this.m_selectedPrefabInstance != null)
		{
			Prefab.PrefabSleeperVolume prefabSleeperVolume = new Prefab.PrefabSleeperVolume(this.m_selectedPrefabInstance.prefab.SleeperVolumes[this.selIdx]);
			prefabSleeperVolume.SetTrigger(_newValue);
			PrefabSleeperVolumeManager.Instance.UpdateSleeperPropertiesServer(this.m_selectedPrefabInstance.id, this.selIdx, prefabSleeperVolume);
			this.triggeredByTitle.IsVisible = (this.triggeredByList.ViewComponent.IsVisible = true);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TxtMinScript_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		if (!_changeFromCode && this.m_selectedPrefabInstance != null)
		{
			Prefab.PrefabSleeperVolume prefabSleeperVolume = new Prefab.PrefabSleeperVolume(this.m_selectedPrefabInstance.prefab.SleeperVolumes[this.selIdx]);
			prefabSleeperVolume.minScript = MinScript.ConvertFromUIText(_text);
			PrefabSleeperVolumeManager.Instance.UpdateSleeperPropertiesServer(this.m_selectedPrefabInstance.id, this.selIdx, prefabSleeperVolume);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SpawnersList_SelectionChanged(XUiC_ListEntry<XUiC_SpawnersList.SpawnerEntry> _previousEntry, XUiC_ListEntry<XUiC_SpawnersList.SpawnerEntry> _newEntry)
	{
		string groupName = null;
		if (_newEntry != null)
		{
			groupName = _newEntry.GetEntry().name;
		}
		if (this.m_selectedPrefabInstance != null)
		{
			Prefab.PrefabSleeperVolume prefabSleeperVolume = new Prefab.PrefabSleeperVolume(this.m_selectedPrefabInstance.prefab.SleeperVolumes[this.selIdx]);
			prefabSleeperVolume.groupName = groupName;
			PrefabSleeperVolumeManager.Instance.UpdateSleeperPropertiesServer(this.m_selectedPrefabInstance.id, this.selIdx, prefabSleeperVolume);
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
				Prefab.PrefabSleeperVolume prefabSleeperVolume = new Prefab.PrefabSleeperVolume(this.m_selectedPrefabInstance.prefab.SleeperVolumes[this.selIdx]);
				if (prefabSleeperVolume != null)
				{
					this.HandleTriggersSetting(prefabSleeperVolume, triggerLayer, false, GameManager.Instance.World);
				}
				PrefabSleeperVolumeManager.Instance.UpdateSleeperPropertiesServer(this.m_selectedPrefabInstance.id, this.selIdx, prefabSleeperVolume);
			}
			_newEntry.IsDirty = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void HandleTriggersSetting(Prefab.PrefabSleeperVolume psv, byte triggerLayer, bool isTriggers, World _world)
	{
		if (_world.IsEditor() && !isTriggers)
		{
			if (psv.HasTriggeredBy(triggerLayer))
			{
				psv.RemoveTriggeredByFlag(triggerLayer);
				return;
			}
			psv.SetTriggeredByFlag(triggerLayer);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TriggerOnAddTriggersPressed()
	{
		if (this.m_selectedPrefabInstance != null)
		{
			this.m_selectedPrefabInstance.prefab.AddNewTriggerLayer();
			this.triggeredByList.RebuildList(false);
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.UpdateCountPresetLabel();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.m_selectedPrefabInstance != null)
		{
			if (this.bSleeperVolumeChanged)
			{
				this.bSleeperVolumeChanged = false;
				this.m_selectedPrefabInstance.prefab.CountSleeperSpawnsInVolume(GameManager.Instance.World, this.m_selectedPrefabInstance.boundingBoxPosition, this.selIdx);
				this.UpdateCountPresetLabel();
			}
			Prefab.PrefabSleeperVolume prefabSleeperVolume = this.m_selectedPrefabInstance.prefab.SleeperVolumes[this.selIdx];
			this.txtGroupId.Text = prefabSleeperVolume.groupId.ToString();
			this.cbxPriority.Value = prefabSleeperVolume.isPriority;
			this.cbxQuestExclude.Value = prefabSleeperVolume.isQuestExclude;
			this.labelIndex.Text = this.selIdx.ToString();
			this.labelPosition.Text = prefabSleeperVolume.startPos.ToString();
			this.labelSize.Text = prefabSleeperVolume.size.ToString();
			this.labelSleeperCount.Text = this.m_selectedPrefabInstance.prefab.Transient_NumSleeperSpawns.ToString();
			this.labelGroup.Text = GameStageGroup.MakeDisplayName(prefabSleeperVolume.groupName);
			this.txtSpawnMin.Text = prefabSleeperVolume.spawnCountMin.ToString();
			this.txtSpawnMax.Text = prefabSleeperVolume.spawnCountMax.ToString();
			this.cbxTrigger.Value = (SleeperVolume.ETriggerType)(prefabSleeperVolume.flags & 7);
			this.txtMinScript.Text = MinScript.ConvertToUIText(prefabSleeperVolume.minScript);
		}
		else
		{
			this.txtGroupId.Text = string.Empty;
			this.cbxPriority.Value = false;
			this.cbxQuestExclude.Value = false;
			this.labelIndex.Text = string.Empty;
			this.labelPosition.Text = string.Empty;
			this.labelSize.Text = string.Empty;
			this.labelSleeperCount.Text = string.Empty;
			this.labelGroup.Text = string.Empty;
			this.txtSpawnMin.Text = string.Empty;
			this.txtSpawnMax.Text = string.Empty;
			this.cbxTrigger.Value = SleeperVolume.ETriggerType.Active;
			this.txtMinScript.Text = string.Empty;
		}
		this.triggeredByTitle.IsVisible = (this.triggeredByList.ViewComponent.IsVisible = true);
	}

	public override void Cleanup()
	{
		base.Cleanup();
		if (SelectionBoxManager.Instance != null)
		{
			SelectionBoxManager.Instance.GetCategory("SleeperVolume").SetCallback(null);
		}
		XUiC_WoPropsSleeperVolume.instance = null;
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
			this.m_selectedPrefabInstance = null;
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
				this.m_selectedPrefabInstance = PrefabSleeperVolumeManager.Instance.GetPrefabInstance(_prefabInstanceId);
				this.bSleeperVolumeChanged = true;
				Prefab prefab = this.m_selectedPrefabInstance.prefab;
				this.triggeredByList.EditPrefab = prefab;
				this.triggeredByList.SleeperOwner = this;
				this.triggeredByList.IsTriggers = false;
				if (prefab.TriggerLayers.Count == 0)
				{
					prefab.AddInitialTriggerLayers();
				}
				return true;
			}
		}
		return false;
	}

	public static void SleeperVolumeChanged(int _prefabInstanceId, int _volumeId)
	{
		if (XUiC_WoPropsSleeperVolume.selectedPrefabInstance == null)
		{
			return;
		}
		if (XUiC_WoPropsSleeperVolume.selectedPrefabInstance.id != _prefabInstanceId || XUiC_WoPropsSleeperVolume.selectedVolumeIndex != _volumeId)
		{
			return;
		}
		XUiC_WoPropsSleeperVolume.instance.bSleeperVolumeChanged = true;
	}

	public void OnSelectionBoxMoved(string _category, string _name, Vector3 _moveVector)
	{
		if (this.m_selectedPrefabInstance == null)
		{
			return;
		}
		Prefab.PrefabSleeperVolume prefabSleeperVolume = new Prefab.PrefabSleeperVolume(this.m_selectedPrefabInstance.prefab.SleeperVolumes[this.selIdx]);
		prefabSleeperVolume.startPos += new Vector3i(_moveVector);
		PrefabSleeperVolumeManager.Instance.UpdateSleeperPropertiesServer(this.m_selectedPrefabInstance.id, this.selIdx, prefabSleeperVolume);
	}

	public void OnSelectionBoxSized(string _category, string _name, int _dTop, int _dBottom, int _dNorth, int _dSouth, int _dEast, int _dWest)
	{
		if (this.m_selectedPrefabInstance == null)
		{
			return;
		}
		Prefab.PrefabSleeperVolume prefabSleeperVolume = new Prefab.PrefabSleeperVolume(this.m_selectedPrefabInstance.prefab.SleeperVolumes[this.selIdx]);
		prefabSleeperVolume.size += new Vector3i(_dEast + _dWest, _dTop + _dBottom, _dNorth + _dSouth);
		prefabSleeperVolume.startPos += new Vector3i(-_dWest, -_dBottom, -_dSouth);
		Vector3i size = prefabSleeperVolume.size;
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
		prefabSleeperVolume.size = size;
		PrefabSleeperVolumeManager.Instance.UpdateSleeperPropertiesServer(this.m_selectedPrefabInstance.id, this.selIdx, prefabSleeperVolume);
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
			Prefab.PrefabSleeperVolume prefabSleeperVolume = new Prefab.PrefabSleeperVolume(this.m_selectedPrefabInstance.prefab.SleeperVolumes[num2]);
			prefabSleeperVolume.used = false;
			PrefabSleeperVolumeManager.Instance.UpdateSleeperPropertiesServer(this.m_selectedPrefabInstance.id, num2, prefabSleeperVolume);
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
		string text2;
		if (SelectionBoxManager.Instance.GetSelected(out text, out text2) && text.Equals("SleeperVolume"))
		{
			_windowManager.SwitchVisible(XUiC_WoPropsSleeperVolume.ID, false);
		}
	}

	public void OnSelectionBoxRotated(string _category, string _name)
	{
	}

	public static bool GetSelectedVolumeStats(out XUiC_WoPropsSleeperVolume.VolumeStats _stats)
	{
		_stats = default(XUiC_WoPropsSleeperVolume.VolumeStats);
		int selectedVolumeIndex = XUiC_WoPropsSleeperVolume.selectedVolumeIndex;
		if (selectedVolumeIndex >= 0)
		{
			if (XUiC_WoPropsSleeperVolume.instance.bSleeperVolumeChanged)
			{
				XUiC_WoPropsSleeperVolume.instance.bSleeperVolumeChanged = false;
				XUiC_WoPropsSleeperVolume.selectedPrefabInstance.prefab.CountSleeperSpawnsInVolume(GameManager.Instance.World, XUiC_WoPropsSleeperVolume.selectedPrefabInstance.boundingBoxPosition, selectedVolumeIndex);
				XUiC_WoPropsSleeperVolume.instance.UpdateCountPresetLabel();
			}
			Prefab.PrefabSleeperVolume prefabSleeperVolume = XUiC_WoPropsSleeperVolume.selectedPrefabInstance.prefab.SleeperVolumes[selectedVolumeIndex];
			_stats.index = selectedVolumeIndex;
			_stats.pos = XUiC_WoPropsSleeperVolume.selectedPrefabInstance.boundingBoxPosition + prefabSleeperVolume.startPos;
			_stats.size = prefabSleeperVolume.size;
			_stats.groupName = GameStageGroup.MakeDisplayName(prefabSleeperVolume.groupName);
			_stats.isPriority = prefabSleeperVolume.isPriority;
			_stats.isQuestExclude = prefabSleeperVolume.isQuestExclude;
			_stats.sleeperCount = XUiC_WoPropsSleeperVolume.selectedPrefabInstance.prefab.Transient_NumSleeperSpawns;
			_stats.spawnCountMin = (int)prefabSleeperVolume.spawnCountMin;
			_stats.spawnCountMax = (int)prefabSleeperVolume.spawnCountMax;
			return true;
		}
		return false;
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label labelIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label labelPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label labelSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label labelSleeperCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label labelGroup;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label triggeredByTitle;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtGroupId;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool cbxPriority;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool cbxQuestExclude;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<XUiC_WoPropsSleeperVolume.CountPreset> cbxCountPreset;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtSpawnMin;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtSpawnMax;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxEnum<SleeperVolume.ETriggerType> cbxTrigger;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtMinScript;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SpawnersList spawnersList;

	[PublicizedFrom(EAccessModifier.Private)]
	public PrefabInstance m_selectedPrefabInstance;

	[PublicizedFrom(EAccessModifier.Private)]
	public int selIdx;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bSleeperVolumeChanged;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_PrefabTriggerEditorList triggeredByList;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showTriggeredBy;

	[PublicizedFrom(EAccessModifier.Private)]
	public static XUiC_WoPropsSleeperVolume instance;

	public struct VolumeStats
	{
		public int index;

		public Vector3i pos;

		public Vector3i size;

		public string groupName;

		public int sleeperCount;

		public int spawnCountMin;

		public int spawnCountMax;

		public bool isPriority;

		public bool isQuestExclude;
	}

	public struct CountPreset
	{
		public CountPreset(short _min, short _max, string _name)
		{
			this.min = _min;
			this.max = _max;
			this.name = _name;
		}

		public override string ToString()
		{
			return this.name;
		}

		public readonly short min;

		public readonly short max;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string name;
	}
}

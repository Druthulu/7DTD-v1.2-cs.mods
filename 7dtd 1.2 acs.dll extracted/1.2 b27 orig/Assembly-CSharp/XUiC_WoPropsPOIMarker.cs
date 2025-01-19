using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_WoPropsPOIMarker : XUiController, ISelectionBoxCallback
{
	public override void Init()
	{
		base.Init();
		XUiC_WoPropsPOIMarker.ID = base.WindowGroup.ID;
		XUiC_WoPropsPOIMarker.Instance = this;
		this.markerList = (base.GetChildById("markers") as XUiC_PrefabMarkerList);
		if (this.markerList != null)
		{
			this.markerList.SelectionChanged += this.MarkerList_SelectionChanged;
		}
		this.StartX = (base.GetChildById("txtStartX") as XUiC_TextInput);
		if (this.StartX != null)
		{
			this.StartX.OnChangeHandler += this.StartX_OnChangeHandler;
		}
		this.StartY = (base.GetChildById("txtStartY") as XUiC_TextInput);
		if (this.StartY != null)
		{
			this.StartY.OnChangeHandler += this.StartY_OnChangeHandler;
		}
		this.StartZ = (base.GetChildById("txtStartZ") as XUiC_TextInput);
		if (this.StartZ != null)
		{
			this.StartZ.OnChangeHandler += this.StartZ_OnChangeHandler;
		}
		this.SizeX = (base.GetChildById("txtSizeX") as XUiC_TextInput);
		if (this.SizeX != null)
		{
			this.SizeX.OnChangeHandler += this.SizeX_OnChangeHandler;
		}
		this.SizeY = (base.GetChildById("txtSizeY") as XUiC_TextInput);
		if (this.SizeY != null)
		{
			this.SizeY.OnChangeHandler += this.SizeY_OnChangeHandler;
		}
		this.SizeZ = (base.GetChildById("txtSizeZ") as XUiC_TextInput);
		if (this.SizeZ != null)
		{
			this.SizeZ.OnChangeHandler += this.SizeZ_OnChangeHandler;
		}
		this.Rotations = (base.GetChildById("txtMarkerRotations") as XUiC_ComboBoxInt);
		if (this.Rotations != null)
		{
			this.Rotations.OnValueChanged += this.Rotations_OnValueChanged;
		}
		this.PartSpawnChance = (base.GetChildById("cbxPartSpawnChance") as XUiC_ComboBoxFloat);
		if (this.PartSpawnChance != null)
		{
			this.PartSpawnChance.OnValueChanged += this.PartSpawnChance_OnValueChanged;
		}
		this.MarkerSize = (base.GetChildById("cbxPOIMarkerSize") as XUiC_ComboBoxEnum<Prefab.Marker.MarkerSize>);
		if (this.MarkerSize != null)
		{
			this.MarkerSize.OnValueChanged += this.MarkerSize_OnValueChanged;
		}
		this.MarkerType = (base.GetChildById("cbxPOIMarkerType") as XUiC_ComboBoxEnum<Prefab.Marker.MarkerTypes>);
		if (this.MarkerType != null)
		{
			this.MarkerType.OnValueChanged += this.MarkerType_OnValueChanged;
		}
		this.GroupName = (base.GetChildById("txtGroup") as XUiC_TextInput);
		if (this.GroupName != null)
		{
			this.GroupName.OnChangeHandler += this.GroupName_OnChangeHandler;
		}
		this.Tags = (base.GetChildById("txtTags") as XUiC_TextInput);
		if (this.Tags != null)
		{
			this.Tags.OnChangeHandler += this.Tags_OnChangeHandler;
		}
		this.btnPOIMarker = base.GetChildById("btnPOIMarker");
		if (this.btnPOIMarker != null)
		{
			this.btnPOIMarker.GetChildById("clickable").OnPress += this.BtnPOIMarker_Controller_OnPress;
		}
		this.MarkerPartName = (base.GetChildById("cbxPOIMarkerPartName") as XUiC_ComboBoxList<string>);
		if (this.MarkerPartName != null)
		{
			foreach (PathAbstractions.AbstractedLocation abstractedLocation in PathAbstractions.PrefabsSearchPaths.GetAvailablePathsList(null, null, null, true))
			{
				if (abstractedLocation.RelativePath.EqualsCaseInsensitive("parts"))
				{
					this.MarkerPartName.Elements.Add(abstractedLocation.Name);
				}
			}
			this.MarkerPartName.OnValueChanged += this.MarkerPartName_OnValueChanged;
		}
		this.lblPartSpawn = base.GetChildById("lblPartName");
		this.lblCustSize = base.GetChildById("lblCustSize");
		this.grdCustSize = base.GetChildById("grdCustSize");
		this.lblPartRotations = base.GetChildById("lblMarkerRotations");
		this.lblMarkerSize = base.GetChildById("lblMarkerSize");
		this.lblPartSpawnChance = base.GetChildById("lblPartSpawnChance");
		if (SelectionBoxManager.Instance != null)
		{
			SelectionBoxManager.Instance.GetCategory("POIMarker").SetCallback(this);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Tags_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		if (this.CurrentMarker == null)
		{
			return;
		}
		this.CurrentMarker.Tags = FastTags<TagGroup.Poi>.Parse(_text);
		PrefabEditModeManager.Instance.NeedsSaving = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PartSpawnChance_OnValueChanged(XUiController _sender, double _oldValue, double _newValue)
	{
		if (this.CurrentMarker == null)
		{
			return;
		}
		this.CurrentMarker.PartChanceToSpawn = (float)Mathf.RoundToInt((float)_newValue * 100f) / 100f;
		this.updatePrefabDataAndVis();
		PrefabEditModeManager.Instance.NeedsSaving = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Rotations_OnValueChanged(XUiController _sender, long _oldValue, long _newValue)
	{
		if (this.CurrentMarker == null)
		{
			return;
		}
		this.CurrentMarker.Rotations = (byte)_newValue;
		this.updatePrefabDataAndVis();
		PrefabEditModeManager.Instance.NeedsSaving = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void MarkerPartName_OnValueChanged(XUiController _sender, string _oldValue, string _newValue)
	{
		if (this.CurrentMarker == null)
		{
			return;
		}
		this.CurrentMarker.PartToSpawn = _newValue;
		this.updatePrefabDataAndVis();
		PrefabEditModeManager.Instance.NeedsSaving = true;
	}

	public override bool GetBindingValue(ref string value, string bindingName)
	{
		if (bindingName == "iscustomsize")
		{
			value = (this.MarkerSize != null && this.MarkerSize.Value == Prefab.Marker.MarkerSize.Custom).ToString();
			return true;
		}
		return false;
	}

	public override void OnClose()
	{
		this.saveDataToPrefab();
		base.OnClose();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnPOIMarker_Controller_OnPress(XUiController _sender, int _mouseButton)
	{
		Vector3 raycastHitPoint = XUiC_LevelTools3Window.getRaycastHitPoint(1000f, 0f);
		if (raycastHitPoint.Equals(Vector3.zero))
		{
			return;
		}
		Vector3i vector3i = World.worldToBlockPos(raycastHitPoint);
		DynamicPrefabDecorator dynamicPrefabDecorator = GameManager.Instance.World.ChunkClusters[0].ChunkProvider.GetDynamicPrefabDecorator();
		if (dynamicPrefabDecorator == null)
		{
			return;
		}
		PrefabInstance prefabInstance = GameUtils.FindPrefabForBlockPos(dynamicPrefabDecorator.GetDynamicPrefabs(), vector3i);
		if (prefabInstance != null)
		{
			Vector3i vector3i2 = new Vector3i(1, 1, 1);
			prefabInstance.prefab.AddNewPOIMarker(prefabInstance.name, prefabInstance.boundingBoxPosition, vector3i - prefabInstance.boundingBoxPosition - new Vector3i(vector3i2.x / 2, 0, vector3i2.z / 2), vector3i2, "new", FastTags<TagGroup.Poi>.none, Prefab.Marker.MarkerTypes.None, false);
		}
		this.updatePrefabDataAndVis();
		POIMarkerToolManager.UpdateAllColors();
		this.markerList.RebuildList(false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GroupName_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		if (this.CurrentMarker == null)
		{
			return;
		}
		this.CurrentMarker.GroupName = _text;
		this.updatePrefabDataAndVis();
		POIMarkerToolManager.UpdateAllColors();
		if (!_changeFromCode)
		{
			this.markerList.RebuildList(false);
		}
		PrefabEditModeManager.Instance.NeedsSaving = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void MarkerType_OnValueChanged(XUiController _sender, Prefab.Marker.MarkerTypes _oldValue, Prefab.Marker.MarkerTypes _newValue)
	{
		if (this.CurrentMarker == null)
		{
			return;
		}
		this.CurrentMarker.MarkerType = _newValue;
		if (this.CurrentMarker.MarkerType == Prefab.Marker.MarkerTypes.POISpawn)
		{
			this.SizeY.Text = "0";
		}
		this.updatePrefabDataAndVis();
		PrefabEditModeManager.Instance.NeedsSaving = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void MarkerSize_OnValueChanged(XUiController _sender, Prefab.Marker.MarkerSize _oldValue, Prefab.Marker.MarkerSize _newValue)
	{
		if (this.CurrentMarker == null)
		{
			return;
		}
		if (_newValue == Prefab.Marker.MarkerSize.Custom)
		{
			return;
		}
		this.CurrentMarker.Size = Prefab.Marker.MarkerSizes[(int)_newValue];
		this.updatePrefabDataAndVis();
		PrefabEditModeManager.Instance.NeedsSaving = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SizeZ_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		if (_text.Length == 0 || _changeFromCode || this.CurrentMarker == null)
		{
			return;
		}
		int z;
		if (!StringParsers.TryParseSInt32(_text, out z, 0, -1, NumberStyles.Integer))
		{
			return;
		}
		this.CurrentMarker.Size = new Vector3i(this.CurrentMarker.Size.x, this.CurrentMarker.Size.y, z);
		this.updatePrefabDataAndVis();
		PrefabEditModeManager.Instance.NeedsSaving = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SizeY_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		if (_text.Length == 0 || _changeFromCode || this.CurrentMarker == null)
		{
			return;
		}
		int y = 0;
		if (!StringParsers.TryParseSInt32(_text, out y, 0, -1, NumberStyles.Integer))
		{
			return;
		}
		if (this.CurrentMarker.MarkerType == Prefab.Marker.MarkerTypes.POISpawn)
		{
			y = 0;
			this.SizeY.Text = y.ToString();
		}
		this.CurrentMarker.Size = new Vector3i(this.CurrentMarker.Size.x, y, this.CurrentMarker.Size.z);
		this.updatePrefabDataAndVis();
		PrefabEditModeManager.Instance.NeedsSaving = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SizeX_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		if (_text.Length == 0 || _changeFromCode || this.CurrentMarker == null)
		{
			return;
		}
		int x;
		if (!StringParsers.TryParseSInt32(_text, out x, 0, -1, NumberStyles.Integer))
		{
			return;
		}
		this.CurrentMarker.Size = new Vector3i(x, this.CurrentMarker.Size.y, this.CurrentMarker.Size.z);
		this.updatePrefabDataAndVis();
		PrefabEditModeManager.Instance.NeedsSaving = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void StartZ_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		if (_text.Length == 0 || _changeFromCode || this.CurrentMarker == null)
		{
			return;
		}
		int z;
		if (!StringParsers.TryParseSInt32(_text, out z, 0, -1, NumberStyles.Integer))
		{
			return;
		}
		this.CurrentMarker.Start = new Vector3i(this.CurrentMarker.Start.x, this.CurrentMarker.Start.y, z);
		this.updatePrefabDataAndVis();
		PrefabEditModeManager.Instance.NeedsSaving = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void StartY_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		if (_text.Length == 0 || _changeFromCode || this.CurrentMarker == null)
		{
			return;
		}
		int y;
		if (!StringParsers.TryParseSInt32(_text, out y, 0, -1, NumberStyles.Integer))
		{
			return;
		}
		this.CurrentMarker.Start = new Vector3i(this.CurrentMarker.Start.x, y, this.CurrentMarker.Start.z);
		this.updatePrefabDataAndVis();
		PrefabEditModeManager.Instance.NeedsSaving = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void StartX_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		if (_text.Length == 0 || _changeFromCode || this.CurrentMarker == null)
		{
			return;
		}
		int x;
		if (!StringParsers.TryParseSInt32(_text, out x, 0, -1, NumberStyles.Integer))
		{
			return;
		}
		this.CurrentMarker.Start = new Vector3i(x, this.CurrentMarker.Start.y, this.CurrentMarker.Start.z);
		this.updatePrefabDataAndVis();
		PrefabEditModeManager.Instance.NeedsSaving = true;
	}

	public void updatePrefabDataAndVis()
	{
		if (this.CurrentMarker == null)
		{
			return;
		}
		SelectionCategory category = SelectionBoxManager.Instance.GetCategory("POIMarker");
		SelectionBox selectionBox = (category != null) ? category.GetBox(this.CurrentMarker.Name) : null;
		if (selectionBox != null)
		{
			POIMarkerToolManager.UnRegisterPOIMarker(selectionBox);
			category.RemoveBox(this.CurrentMarker.Name);
			if (this.CurrentMarker.MarkerType == Prefab.Marker.MarkerTypes.PartSpawn && this.CurrentMarker.PartToSpawn != null && this.CurrentMarker.PartToSpawn.Length > 0)
			{
				Prefab prefab = new Prefab();
				prefab.Load(this.CurrentMarker.PartToSpawn, false, false, true, false);
				if ((prefab.rotationToFaceNorth + (int)this.CurrentMarker.Rotations) % 2 == 1)
				{
					this.CurrentMarker.Size = new Vector3i(prefab.size.z, prefab.size.y, prefab.size.x);
				}
				else
				{
					this.CurrentMarker.Size = prefab.size;
				}
			}
			selectionBox = category.AddBox(this.CurrentMarker.Name, this.CurrentMarker.Start - XUiC_WoPropsPOIMarker.getBaseVisualOffset(), this.CurrentMarker.Size, false, false);
			selectionBox.UserData = this.CurrentMarker;
			selectionBox.bAlwaysDrawDirection = true;
			selectionBox.bDrawDirection = true;
			float facing = 0f;
			switch (this.CurrentMarker.Rotations)
			{
			case 1:
				facing = (float)((this.CurrentMarker.MarkerType == Prefab.Marker.MarkerTypes.PartSpawn) ? 90 : 270);
				break;
			case 2:
				facing = 180f;
				break;
			case 3:
				facing = (float)((this.CurrentMarker.MarkerType == Prefab.Marker.MarkerTypes.PartSpawn) ? 270 : 90);
				break;
			}
			SelectionBoxManager.Instance.SetFacingDirection("POIMarker", this.CurrentMarker.Name, facing);
			SelectionBoxManager.Instance.SetActive("POIMarker", this.CurrentMarker.Name, true);
			POIMarkerToolManager.RegisterPOIMarker(selectionBox);
		}
		this.saveDataToPrefab();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void saveDataToPrefab()
	{
		if (PrefabEditModeManager.Instance.VoxelPrefab != null && PrefabEditModeManager.Instance.VoxelPrefab.POIMarkers != null)
		{
			for (int i = 0; i < PrefabEditModeManager.Instance.VoxelPrefab.POIMarkers.Count; i++)
			{
				if (PrefabEditModeManager.Instance.VoxelPrefab.POIMarkers[i].Name == this.CurrentMarker.Name)
				{
					PrefabEditModeManager.Instance.VoxelPrefab.POIMarkers[i] = this.CurrentMarker;
					return;
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateUIElements()
	{
		if (this.MarkerType == null)
		{
			return;
		}
		switch (this.MarkerType.Value)
		{
		case Prefab.Marker.MarkerTypes.None:
			this.lblMarkerSize.ViewComponent.IsVisible = (this.MarkerSize.ViewComponent.IsVisible = false);
			this.lblPartSpawn.ViewComponent.IsVisible = (this.MarkerPartName.ViewComponent.IsVisible = false);
			this.lblPartRotations.ViewComponent.IsVisible = (this.Rotations.ViewComponent.IsVisible = false);
			this.lblPartSpawnChance.ViewComponent.IsVisible = (this.PartSpawnChance.ViewComponent.IsVisible = false);
			this.lblCustSize.ViewComponent.IsVisible = (this.grdCustSize.ViewComponent.IsVisible = false);
			return;
		case Prefab.Marker.MarkerTypes.POISpawn:
			this.lblMarkerSize.ViewComponent.IsVisible = (this.MarkerSize.ViewComponent.IsVisible = true);
			this.lblPartSpawn.ViewComponent.IsVisible = (this.MarkerPartName.ViewComponent.IsVisible = false);
			this.lblPartRotations.ViewComponent.IsVisible = (this.Rotations.ViewComponent.IsVisible = true);
			this.lblPartSpawnChance.ViewComponent.IsVisible = (this.PartSpawnChance.ViewComponent.IsVisible = false);
			this.lblCustSize.ViewComponent.IsVisible = (this.grdCustSize.ViewComponent.IsVisible = (this.MarkerSize.Value == Prefab.Marker.MarkerSize.Custom));
			return;
		case Prefab.Marker.MarkerTypes.RoadExit:
			this.lblMarkerSize.ViewComponent.IsVisible = (this.MarkerSize.ViewComponent.IsVisible = true);
			this.lblPartSpawn.ViewComponent.IsVisible = (this.MarkerPartName.ViewComponent.IsVisible = false);
			this.lblPartRotations.ViewComponent.IsVisible = (this.Rotations.ViewComponent.IsVisible = true);
			this.lblPartSpawnChance.ViewComponent.IsVisible = (this.PartSpawnChance.ViewComponent.IsVisible = false);
			this.lblCustSize.ViewComponent.IsVisible = (this.grdCustSize.ViewComponent.IsVisible = (this.MarkerSize.Value == Prefab.Marker.MarkerSize.Custom));
			return;
		case Prefab.Marker.MarkerTypes.PartSpawn:
			this.lblMarkerSize.ViewComponent.IsVisible = (this.MarkerSize.ViewComponent.IsVisible = false);
			this.lblCustSize.ViewComponent.IsVisible = (this.grdCustSize.ViewComponent.IsVisible = false);
			this.lblPartRotations.ViewComponent.IsVisible = (this.Rotations.ViewComponent.IsVisible = true);
			this.lblPartSpawn.ViewComponent.IsVisible = (this.MarkerPartName.ViewComponent.IsVisible = true);
			this.lblPartRotations.ViewComponent.IsVisible = (this.Rotations.ViewComponent.IsVisible = true);
			this.lblPartSpawnChance.ViewComponent.IsVisible = (this.PartSpawnChance.ViewComponent.IsVisible = true);
			return;
		default:
			return;
		}
	}

	public override void Update(float _dt)
	{
		this.updateUIElements();
		base.Update(_dt);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.updateValues();
		this.updateUIElements();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateValues()
	{
		if (this.CurrentMarker != null)
		{
			XUiC_TextInput startX = this.StartX;
			Vector3i vector3i = this.CurrentMarker.Start;
			startX.Text = vector3i.x.ToString();
			XUiC_TextInput startY = this.StartY;
			vector3i = this.CurrentMarker.Start;
			startY.Text = vector3i.y.ToString();
			XUiC_TextInput startZ = this.StartZ;
			vector3i = this.CurrentMarker.Start;
			startZ.Text = vector3i.z.ToString();
			if (this.MarkerSize != null)
			{
				if (Prefab.Marker.MarkerSizes.Contains(this.CurrentMarker.Size))
				{
					this.MarkerSize.Value = (Prefab.Marker.MarkerSize)Prefab.Marker.MarkerSizes.IndexOf(this.CurrentMarker.Size);
				}
				else
				{
					this.MarkerSize.Value = Prefab.Marker.MarkerSize.Custom;
				}
			}
			if (this.SizeX != null)
			{
				XUiC_TextInput sizeX = this.SizeX;
				vector3i = this.CurrentMarker.Size;
				sizeX.Text = vector3i.x.ToString();
			}
			if (this.SizeY != null)
			{
				XUiC_TextInput sizeY = this.SizeY;
				vector3i = this.CurrentMarker.Size;
				sizeY.Text = vector3i.y.ToString();
			}
			if (this.SizeZ != null)
			{
				XUiC_TextInput sizeZ = this.SizeZ;
				vector3i = this.CurrentMarker.Size;
				sizeZ.Text = vector3i.z.ToString();
			}
			if (this.MarkerType != null)
			{
				this.MarkerType.Value = this.CurrentMarker.MarkerType;
			}
			if (this.GroupName != null)
			{
				this.GroupName.Text = this.CurrentMarker.GroupName;
			}
			if (this.Tags != null)
			{
				this.Tags.Text = this.CurrentMarker.Tags.ToString();
			}
			if (this.MarkerPartName != null)
			{
				this.MarkerPartName.Value = this.CurrentMarker.PartToSpawn;
			}
			if (this.Rotations != null)
			{
				this.Rotations.Value = (long)((ulong)this.CurrentMarker.Rotations);
			}
			if (this.PartSpawnChance != null)
			{
				this.PartSpawnChance.Value = (double)((float)Mathf.RoundToInt(this.CurrentMarker.PartChanceToSpawn * 100f) / 100f);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void MarkerList_SelectionChanged(XUiC_ListEntry<XUiC_PrefabMarkerList.PrefabMarkerEntry> _previousEntry, XUiC_ListEntry<XUiC_PrefabMarkerList.PrefabMarkerEntry> _newEntry)
	{
		if (_newEntry == null || _newEntry.GetEntry() == null)
		{
			return;
		}
		this.CurrentMarker = _newEntry.GetEntry().marker;
		SelectionBoxManager.Instance.SetActive("POIMarker", this.CurrentMarker.Name, true);
		_newEntry.Selected = true;
		_newEntry.IsDirty = true;
		this.updateValues();
		this.IsDirty = true;
	}

	public void CheckSpecialKeys(Event ev, PlayerActionsLocal playerActions)
	{
		if ((ev.modifiers & EventModifiers.Control) != EventModifiers.None && (ev.modifiers & EventModifiers.Shift) != EventModifiers.None && ev.keyCode == KeyCode.Return)
		{
			this.SpawnNewMarker();
			if (POIMarkerToolManager.currentSelectionBox != null && POIMarkerToolManager.currentSelectionBox.UserData is Prefab.Marker)
			{
				this.CurrentMarker = (POIMarkerToolManager.currentSelectionBox.UserData as Prefab.Marker);
			}
			this.updatePrefabDataAndVis();
			POIMarkerToolManager.UpdateAllColors();
			this.markerList.RebuildList(false);
			ev.Use();
		}
		if ((ev.modifiers & EventModifiers.Shift) != EventModifiers.None && ev.keyCode == KeyCode.Return)
		{
			if (POIMarkerToolManager.currentSelectionBox != null && POIMarkerToolManager.currentSelectionBox.UserData is Prefab.Marker)
			{
				this.CurrentMarker = (POIMarkerToolManager.currentSelectionBox.UserData as Prefab.Marker);
			}
			base.xui.playerUI.windowManager.Open(XUiC_WoPropsPOIMarker.ID, true, false, true);
			ev.Use();
		}
		if ((ev.modifiers & EventModifiers.Control) != EventModifiers.None && (ev.modifiers & EventModifiers.Shift) != EventModifiers.None && ev.keyCode == KeyCode.Z)
		{
			if (this.CurrentMarker != null)
			{
				this.CurrentMarker.Rotations = (this.CurrentMarker.Rotations + 1) % 4;
				this.updatePrefabDataAndVis();
			}
			ev.Use();
		}
		if ((ev.modifiers & EventModifiers.Control) != EventModifiers.None && (ev.modifiers & EventModifiers.Shift) != EventModifiers.None && ev.keyCode == KeyCode.A)
		{
			ev.Use();
			if (PrefabEditModeManager.Instance.VoxelPrefab != null && PrefabEditModeManager.Instance.VoxelPrefab.POIMarkers != null)
			{
				for (int i = 0; i < PrefabEditModeManager.Instance.VoxelPrefab.POIMarkers.Count; i++)
				{
					if (PrefabEditModeManager.Instance.VoxelPrefab.POIMarkers[i].MarkerType == Prefab.Marker.MarkerTypes.POISpawn)
					{
						SelectionBox selBox = null;
						if (SelectionBoxManager.Instance.TryGetSelectionBox("POIMarker", "POIMarker_" + i.ToString(), out selBox))
						{
							POIMarkerToolManager.DisplayPrefabPreviewForMarker(selBox);
						}
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SpawnNewMarker()
	{
		Vector3 raycastHitPoint = XUiC_LevelTools3Window.getRaycastHitPoint(1000f, 0f);
		if (raycastHitPoint.Equals(Vector3.zero))
		{
			return;
		}
		Vector3i vector3i = World.worldToBlockPos(raycastHitPoint);
		DynamicPrefabDecorator dynamicPrefabDecorator = GameManager.Instance.World.ChunkClusters[0].ChunkProvider.GetDynamicPrefabDecorator();
		if (dynamicPrefabDecorator == null)
		{
			return;
		}
		PrefabInstance prefabInstance = GameUtils.FindPrefabForBlockPos(dynamicPrefabDecorator.GetDynamicPrefabs(), vector3i);
		if (prefabInstance != null)
		{
			Vector3i vector3i2 = new Vector3i(1, 1, 1);
			prefabInstance.prefab.AddNewPOIMarker(prefabInstance.name, prefabInstance.boundingBoxPosition, vector3i - prefabInstance.boundingBoxPosition - new Vector3i(vector3i2.x / 2, 0, vector3i2.z / 2), vector3i2, "new", FastTags<TagGroup.Poi>.none, Prefab.Marker.MarkerTypes.None, true);
		}
	}

	public override void Cleanup()
	{
		base.Cleanup();
		if (SelectionBoxManager.Instance != null)
		{
			SelectionCategory category = SelectionBoxManager.Instance.GetCategory("POIMarker");
			if (category != null)
			{
				category.SetCallback(null);
			}
		}
		POIMarkerToolManager.CleanUp();
		XUiC_WoPropsPOIMarker.Instance = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool tryGetSelectedMarker(out Prefab.Marker _marker)
	{
		if (POIMarkerToolManager.currentSelectionBox == null || POIMarkerToolManager.currentSelectionBox.UserData == null)
		{
			_marker = null;
			return false;
		}
		_marker = (Prefab.Marker)POIMarkerToolManager.currentSelectionBox.UserData;
		return true;
	}

	public bool OnSelectionBoxActivated(string _category, string _name, bool _bActivated)
	{
		return true;
	}

	public bool OnSelectionBoxDelete(string _category, string _name)
	{
		if (PrefabEditModeManager.Instance.VoxelPrefab == null)
		{
			return false;
		}
		if (POIMarkerToolManager.currentSelectionBox)
		{
			POIMarkerToolManager.UnRegisterPOIMarker(POIMarkerToolManager.currentSelectionBox);
		}
		SelectionCategory category = SelectionBoxManager.Instance.GetCategory(_category);
		if (category != null)
		{
			category.RemoveBox(_name);
		}
		return PrefabEditModeManager.Instance.VoxelPrefab.POIMarkers.RemoveAll((Prefab.Marker x) => x.Name == _name) > 0;
	}

	public bool OnSelectionBoxIsAvailable(string _category, EnumSelectionBoxAvailabilities _criteria)
	{
		return _criteria == EnumSelectionBoxAvailabilities.CanShowProperties || _criteria == EnumSelectionBoxAvailabilities.CanResize;
	}

	public void OnSelectionBoxMirrored(Vector3i _axis)
	{
	}

	public void OnSelectionBoxShowProperties(bool _bVisible, GUIWindowManager _windowManager)
	{
		string text;
		string text2;
		if (SelectionBoxManager.Instance.GetSelected(out text, out text2) && text.Equals("POIMarker"))
		{
			_windowManager.SwitchVisible(XUiC_WoPropsPOIMarker.ID, false);
		}
	}

	public void OnSelectionBoxRotated(string _category, string _name)
	{
	}

	public void OnSelectionBoxMoved(string _category, string _name, Vector3 _moveVector)
	{
		Prefab.Marker marker;
		if (this.tryGetSelectedMarker(out marker))
		{
			marker.Start += new Vector3i(_moveVector.x, _moveVector.y, _moveVector.z);
			SelectionCategory category = SelectionBoxManager.Instance.GetCategory("POIMarker");
			if (category == null)
			{
				return;
			}
			SelectionBox box = category.GetBox(_name);
			if (box == null)
			{
				return;
			}
			box.SetPositionAndSize(marker.Start - XUiC_WoPropsPOIMarker.getBaseVisualOffset(), marker.Size);
		}
	}

	public void OnSelectionBoxSized(string _category, string _name, int _dTop, int _dBottom, int _dNorth, int _dSouth, int _dEast, int _dWest)
	{
		Prefab.Marker marker;
		if (!this.tryGetSelectedMarker(out marker))
		{
			marker.Start += new Vector3i(-_dWest, -_dBottom, -_dSouth);
			marker.Size += new Vector3i(_dEast + _dWest, _dTop + _dBottom, _dNorth + _dSouth);
			SelectionCategory category = SelectionBoxManager.Instance.GetCategory("POIMarker");
			if (category == null)
			{
				return;
			}
			SelectionBox box = category.GetBox(_name);
			if (box == null)
			{
				return;
			}
			box.SetPositionAndSize(marker.Start - XUiC_WoPropsPOIMarker.getBaseVisualOffset(), marker.Size);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector3i getBaseVisualOffset()
	{
		Vector3i result = Vector3i.zero;
		if (PrefabEditModeManager.Instance.VoxelPrefab != null)
		{
			result = PrefabEditModeManager.Instance.VoxelPrefab.size * 0.5f;
			result.y = -1;
		}
		return result;
	}

	public static string ID = "";

	public static XUiC_WoPropsPOIMarker Instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput StartX;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput StartY;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput StartZ;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput SizeX;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput SizeY;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput SizeZ;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput GroupName;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput Tags;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController grdCustSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController lblCustSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController lblPartSpawn;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController lblPartRotations;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController btnPOIMarker;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController lblMarkerSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController lblPartSpawnChance;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxEnum<Prefab.Marker.MarkerSize> MarkerSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxEnum<Prefab.Marker.MarkerTypes> MarkerType;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> MarkerPartName;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_PrefabMarkerList markerList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxInt Rotations;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat PartSpawnChance;

	public const float cPrefabYPosition = 4f;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool lastIsCustomSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool lastIsPartSpawner;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool lastShowRotations;

	public Prefab.Marker CurrentMarker;
}

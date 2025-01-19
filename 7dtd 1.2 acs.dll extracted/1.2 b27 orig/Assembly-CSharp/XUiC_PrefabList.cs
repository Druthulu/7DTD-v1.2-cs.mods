using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PrefabList : XUiController
{
	public override void Init()
	{
		base.Init();
		XUiC_PrefabList.ID = base.WindowGroup.ID;
		this.prefabPreview = (XUiV_Texture)base.GetChildById("prefabPreview").ViewComponent;
		this.noPreviewLabel = (XUiV_Label)base.GetChildById("noPreview").ViewComponent;
		this.noPreviewLabel.IsVisible = false;
		this.groupList = (base.GetChildById("groups") as XUiC_PrefabGroupList);
		this.fileList = (base.GetChildById("files") as XUiC_PrefabFileList);
		this.btnLoad = (base.GetChildById("btnLoad") as XUiC_SimpleButton);
		this.btnProperties = (base.GetChildById("btnProperties") as XUiC_SimpleButton);
		this.btnSave = (base.GetChildById("btnSave") as XUiC_SimpleButton);
		this.btnLoadIntoPrefab = (base.GetChildById("btnLoadIntoPrefab") as XUiC_SimpleButton);
		this.btnApplyLoadedPrefab = (base.GetChildById("btnApplyLoadedPrefab") as XUiC_SimpleButton);
		this.groupList.SelectionChanged += this.GroupListSelectionChanged;
		this.fileList.SelectionChanged += this.FileList_SelectionChanged;
		this.fileList.OnEntryDoubleClicked += this.FileList_OnEntryDoubleClicked;
		this.fileList.PageNumberChanged += this.FileListOnPageNumberChanged;
		this.btnLoad.OnPressed += this.BtnLoad_OnPressed;
		this.btnProperties.OnPressed += this.BtnPropertiesOnOnPressed;
		this.btnSave.OnPressed += this.BtnSave_OnPressed;
		this.btnLoadIntoPrefab.OnPressed += this.BtnLoadIntoPrefabOnOnPressed;
		this.btnApplyLoadedPrefab.OnPressed += this.BtnApplyLoadedPrefabOnOnPressed;
		XUiC_SimpleButton xuiC_SimpleButton = base.GetChildById("btnCleanOtherPrefabs") as XUiC_SimpleButton;
		if (xuiC_SimpleButton != null)
		{
			xuiC_SimpleButton.OnPressed += this.BtnCleanOtherPrefabsOnOnPressed;
		}
		XUiC_SimpleButton xuiC_SimpleButton2 = base.GetChildById("btnLoadIntoClipboard") as XUiC_SimpleButton;
		if (xuiC_SimpleButton2 != null)
		{
			xuiC_SimpleButton2.OnPressed += this.BtnLoadIntoClipboardOnOnPressed;
		}
		XUiC_SimpleButton xuiC_SimpleButton3 = base.GetChildById("btnNew") as XUiC_SimpleButton;
		if (xuiC_SimpleButton3 != null)
		{
			xuiC_SimpleButton3.OnPressed += this.BtnNewOnOnPressed;
		}
		this.btnWorldPlacePrefab = (base.GetChildById("btnWorldPlacePrefab") as XUiC_SimpleButton);
		if (this.btnWorldPlacePrefab != null)
		{
			this.btnWorldPlacePrefab.OnPressed += this.BtnWorldPlacePrefabOnPressed;
		}
		this.btnWorldReplacePrefab = (base.GetChildById("btnWorldReplacePrefab") as XUiC_SimpleButton);
		if (this.btnWorldReplacePrefab != null)
		{
			this.btnWorldReplacePrefab.OnPressed += this.BtnWorldReplacePrefabOnPressed;
		}
		this.btnWorldDeletePrefab = (base.GetChildById("btnWorldDeletePrefab") as XUiC_SimpleButton);
		if (this.btnWorldDeletePrefab != null)
		{
			this.btnWorldDeletePrefab.OnPressed += this.BtnWorldDeletePrefabOnPressed;
		}
		this.btnWorldApplyPrefabChanges = (base.GetChildById("btnWorldApplyPrefabChanges") as XUiC_SimpleButton);
		if (this.btnWorldApplyPrefabChanges != null)
		{
			this.btnWorldApplyPrefabChanges.OnPressed += this.BtnWorldApplyPrefabChangesOnPressed;
		}
		this.btnWorldRevertPrefabChanges = (base.GetChildById("btnWorldRevertPrefabChanges") as XUiC_SimpleButton);
		if (this.btnWorldRevertPrefabChanges != null)
		{
			this.btnWorldRevertPrefabChanges.OnPressed += this.BtnWorldRevertPrefabChangesOnPressed;
		}
		this.btnLoad.Enabled = false;
		this.btnProperties.Enabled = false;
		this.groupList.SelectedEntryIndex = 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GroupListSelectionChanged(XUiC_ListEntry<XUiC_PrefabGroupList.PrefabGroupEntry> _previousEntry, XUiC_ListEntry<XUiC_PrefabGroupList.PrefabGroupEntry> _newEntry)
	{
		string groupFilter = null;
		if (_newEntry != null)
		{
			groupFilter = _newEntry.GetEntry().filterString;
		}
		this.fileList.SetGroupFilter(groupFilter);
		if (this.fileList.EntryCount > 0)
		{
			this.fileList.SelectedEntryIndex = 0;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FileListOnPageNumberChanged(int _pageNumber)
	{
		this.fileList.SelectedEntryIndex = this.fileList.Page * this.fileList.PageLength;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FileList_SelectionChanged(XUiC_ListEntry<XUiC_PrefabFileList.PrefabFileEntry> _previousEntry, XUiC_ListEntry<XUiC_PrefabFileList.PrefabFileEntry> _newEntry)
	{
		this.btnLoad.Enabled = (_newEntry != null);
		this.btnProperties.Enabled = (_newEntry != null);
		this.updatePreview(_newEntry);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updatePreview(XUiC_ListEntry<XUiC_PrefabFileList.PrefabFileEntry> _newEntry)
	{
		if (this.prefabPreview.Texture != null)
		{
			UnityEngine.Object obj = (Texture2D)this.prefabPreview.Texture;
			this.prefabPreview.Texture = null;
			UnityEngine.Object.Destroy(obj);
		}
		if (((_newEntry != null) ? _newEntry.GetEntry() : null) != null)
		{
			string path = _newEntry.GetEntry().location.FullPathNoExtension + ".jpg";
			if (SdFile.Exists(path))
			{
				Texture2D texture2D = new Texture2D(1, 1, TextureFormat.RGB24, false);
				byte[] data = SdFile.ReadAllBytes(path);
				texture2D.LoadImage(data);
				this.prefabPreview.Texture = texture2D;
				this.noPreviewLabel.IsVisible = false;
				this.prefabPreview.IsVisible = true;
				return;
			}
		}
		this.noPreviewLabel.IsVisible = true;
		this.prefabPreview.IsVisible = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FileList_OnEntryDoubleClicked(XUiC_PrefabFileList.PrefabFileEntry _entry)
	{
		if (PrefabEditModeManager.Instance.IsActive())
		{
			this.BtnLoad_OnPressed(this, -1);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnLoad_OnPressed(XUiController _sender, int _mouseButton)
	{
		XUiC_ListEntry<XUiC_PrefabFileList.PrefabFileEntry> selectedEntry = this.fileList.SelectedEntry;
		if (((selectedEntry != null) ? selectedEntry.GetEntry() : null) != null)
		{
			XUiC_SaveDirtyPrefab.Show(base.xui, new Action<XUiC_SaveDirtyPrefab.ESelectedAction>(this.loadPrefab), XUiC_SaveDirtyPrefab.EMode.AskSaveIfDirty);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void loadPrefab(XUiC_SaveDirtyPrefab.ESelectedAction _action)
	{
		base.xui.playerUI.windowManager.Open(XUiC_InGameMenuWindow.ID, true, false, true);
		if (_action == XUiC_SaveDirtyPrefab.ESelectedAction.Cancel)
		{
			return;
		}
		PathAbstractions.AbstractedLocation location = this.fileList.SelectedEntry.GetEntry().location;
		bool flag = XUiC_LevelTools2Window.IsShowImposter(base.xui);
		if (flag && PrefabEditModeManager.Instance.HasPrefabImposter(location))
		{
			PrefabEditModeManager.Instance.LoadImposterPrefab(location);
			return;
		}
		if (flag)
		{
			GameManager.ShowTooltip(GameManager.Instance.World.GetLocalPlayers()[0], string.Format(Localization.Get("xuiPrefabsPrefabHasNoImposter", false), location.Name), false);
			XUiC_LevelTools2Window.SetShowImposter(base.xui, false);
		}
		PrefabEditModeManager.Instance.LoadVoxelPrefab(location, false, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnPropertiesOnOnPressed(XUiController _sender, int _mouseButton)
	{
		PathAbstractions.AbstractedLocation location = this.fileList.SelectedEntry.GetEntry().location;
		PathAbstractions.AbstractedLocation loadedPrefab = PrefabEditModeManager.Instance.LoadedPrefab;
		if (location == loadedPrefab)
		{
			XUiC_PrefabPropertiesEditor.Show(base.xui, XUiC_PrefabPropertiesEditor.EPropertiesFrom.LoadedPrefab, PathAbstractions.AbstractedLocation.None);
			return;
		}
		XUiC_PrefabPropertiesEditor.Show(base.xui, XUiC_PrefabPropertiesEditor.EPropertiesFrom.FileBrowserSelection, location);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnSave_OnPressed(XUiController _sender, int _mouseButton)
	{
		XUiC_SaveDirtyPrefab.Show(base.xui, new Action<XUiC_SaveDirtyPrefab.ESelectedAction>(this.savePrefab), XUiC_SaveDirtyPrefab.EMode.ForceSave);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void savePrefab(XUiC_SaveDirtyPrefab.ESelectedAction _action)
	{
		base.xui.playerUI.windowManager.Open(XUiC_InGameMenuWindow.ID, true, false, true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnLoadIntoPrefabOnOnPressed(XUiController _sender, int _mouseButton)
	{
		PathAbstractions.AbstractedLocation location = this.fileList.SelectedEntry.GetEntry().location;
		BlockToolSelection blockToolSelection = GameManager.Instance.GetActiveBlockTool() as BlockToolSelection;
		DynamicPrefabDecorator dynamicPrefabDecorator = GameManager.Instance.GetDynamicPrefabDecorator();
		Prefab prefab = new Prefab();
		prefab.Load(location, true, true, true, false);
		dynamicPrefabDecorator.CreateNewPrefabAndActivate(location, blockToolSelection.SelectionStart, prefab, true);
		base.xui.playerUI.windowManager.Close(XUiC_InGameMenuWindow.ID);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnApplyLoadedPrefabOnOnPressed(XUiController _sender, int _mouseButton)
	{
		DynamicPrefabDecorator dynamicPrefabDecorator = GameManager.Instance.GetDynamicPrefabDecorator();
		PrefabInstance prefabInstance = (((dynamicPrefabDecorator != null) ? dynamicPrefabDecorator.ActivePrefab : null) != null) ? dynamicPrefabDecorator.ActivePrefab : null;
		prefabInstance.CleanFromWorld(GameManager.Instance.World, true);
		prefabInstance.CopyIntoWorld(GameManager.Instance.World, true, false, FastTags<TagGroup.Global>.none);
		GameManager.Instance.World.m_ChunkManager.RemoveAllChunksOnAllClients();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCleanOtherPrefabsOnOnPressed(XUiController _sender, int _mouseButton)
	{
		throw new NotImplementedException();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnLoadIntoClipboardOnOnPressed(XUiController _sender, int _mouseButton)
	{
		PathAbstractions.AbstractedLocation location = this.fileList.SelectedEntry.GetEntry().location;
		BlockToolSelection blockToolSelection = GameManager.Instance.GetActiveBlockTool() as BlockToolSelection;
		Prefab prefab = new Prefab();
		prefab.Load(location, true, true, true, false);
		blockToolSelection.LoadPrefabIntoClipboard(prefab);
		base.xui.playerUI.windowManager.Close(XUiC_InGameMenuWindow.ID);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnNewOnOnPressed(XUiController _sender, int _mouseButton)
	{
		XUiC_SaveDirtyPrefab.Show(base.xui, new Action<XUiC_SaveDirtyPrefab.ESelectedAction>(this.newPrefab), XUiC_SaveDirtyPrefab.EMode.AskSaveIfDirty);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void newPrefab(XUiC_SaveDirtyPrefab.ESelectedAction _action)
	{
		base.xui.playerUI.windowManager.Open(XUiC_InGameMenuWindow.ID, true, false, true);
		if (_action == XUiC_SaveDirtyPrefab.ESelectedAction.Cancel)
		{
			return;
		}
		if (XUiC_LevelTools2Window.IsShowImposter(base.xui))
		{
			XUiC_LevelTools2Window.SetShowImposter(base.xui, false);
		}
		PrefabEditModeManager.Instance.NewVoxelPrefab();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnWorldPlacePrefabOnPressed(XUiController _sender, int _mouseButton)
	{
		GameUtils.DirEightWay closestDirection = GameUtils.GetClosestDirection(base.xui.playerUI.entityPlayer.rotation.y, true);
		BlockToolSelection blockToolSelection = GameManager.Instance.GetActiveBlockTool() as BlockToolSelection;
		if (blockToolSelection == null)
		{
			return;
		}
		if (GameManager.Instance.GetDynamicPrefabDecorator() == null)
		{
			return;
		}
		PathAbstractions.AbstractedLocation location = this.fileList.SelectedEntry.GetEntry().location;
		Prefab prefab = new Prefab();
		prefab.Load(location, true, true, false, false);
		int num;
		switch (closestDirection)
		{
		case GameUtils.DirEightWay.N:
			num = 2;
			goto IL_A8;
		case GameUtils.DirEightWay.E:
			num = 3;
			goto IL_A8;
		case GameUtils.DirEightWay.S:
			num = 0;
			goto IL_A8;
		case GameUtils.DirEightWay.W:
			num = 1;
			goto IL_A8;
		}
		throw new ArgumentOutOfRangeException();
		IL_A8:
		int num2 = MathUtils.Mod(num - prefab.rotationToFaceNorth, 4);
		Vector3i vector3i;
		switch (closestDirection)
		{
		case GameUtils.DirEightWay.N:
			vector3i = new Vector3i(-prefab.size.x / 2, 0, 0);
			goto IL_15F;
		case GameUtils.DirEightWay.E:
			vector3i = new Vector3i(0, 0, -prefab.size.z / 2);
			goto IL_15F;
		case GameUtils.DirEightWay.S:
			vector3i = new Vector3i(-prefab.size.x / 2, 0, 1 - prefab.size.z);
			goto IL_15F;
		case GameUtils.DirEightWay.W:
			vector3i = new Vector3i(1 - prefab.size.x, 0, -prefab.size.z / 2);
			goto IL_15F;
		}
		throw new ArgumentOutOfRangeException();
		IL_15F:
		Vector3i other = vector3i;
		other.y = prefab.yOffset;
		PrefabInstance prefabInstance = GameManager.Instance.GetDynamicPrefabDecorator().CreateNewPrefabAndActivate(prefab.location, blockToolSelection.SelectionStart + other, prefab, true);
		while (num2-- > 0)
		{
			prefabInstance.RotateAroundY();
		}
		prefabInstance.UpdateImposterView();
		base.xui.playerUI.windowManager.Close(XUiC_InGameMenuWindow.ID);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnWorldReplacePrefabOnPressed(XUiController _sender, int _mouseButton)
	{
		if (GameManager.Instance.GetDynamicPrefabDecorator() != null)
		{
			PrefabInstance prefabInstance = GameManager.Instance.GetDynamicPrefabDecorator().RemoveActivePrefab(GameManager.Instance.World);
			PathAbstractions.AbstractedLocation location = this.fileList.SelectedEntry.GetEntry().location;
			Prefab prefab = new Prefab();
			prefab.Load(location, true, true, false, false);
			Vector3i position = prefabInstance.boundingBoxPosition + new Vector3i(0f, (float)prefab.yOffset - prefabInstance.yOffsetOfPrefab, 0f);
			PrefabInstance prefabInstance2 = GameManager.Instance.GetDynamicPrefabDecorator().CreateNewPrefabAndActivate(prefab.location, position, prefab, true);
			for (;;)
			{
				PrefabInstance prefabInstance3 = prefabInstance;
				byte rotation = prefabInstance3.rotation;
				prefabInstance3.rotation = rotation - 1;
				if (rotation <= 0)
				{
					break;
				}
				prefabInstance2.RotateAroundY();
			}
			prefabInstance2.UpdateImposterView();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnWorldDeletePrefabOnPressed(XUiController _sender, int _mouseButton)
	{
		GameManager.Instance.GetDynamicPrefabDecorator().RemoveActivePrefab(GameManager.Instance.World);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnWorldApplyPrefabChangesOnPressed(XUiController _sender, int _mouseButton)
	{
		DynamicPrefabDecorator dynamicPrefabDecorator = GameManager.Instance.GetDynamicPrefabDecorator();
		PrefabInstance prefabInstance = (dynamicPrefabDecorator != null) ? dynamicPrefabDecorator.ActivePrefab : null;
		if (prefabInstance == null)
		{
			return;
		}
		prefabInstance.CleanFromWorld(GameManager.Instance.World, true);
		prefabInstance.CopyIntoWorld(GameManager.Instance.World, true, false, FastTags<TagGroup.Global>.none);
		prefabInstance.UpdateImposterView();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnWorldRevertPrefabChangesOnPressed(XUiController _sender, int _mouseButton)
	{
		DynamicPrefabDecorator dynamicPrefabDecorator = GameManager.Instance.GetDynamicPrefabDecorator();
		PrefabInstance prefabInstance = (dynamicPrefabDecorator != null) ? dynamicPrefabDecorator.ActivePrefab : null;
		if (prefabInstance == null)
		{
			return;
		}
		prefabInstance.UpdateBoundingBoxPosAndScale(GameManager.Instance.GetDynamicPrefabDecorator().ActivePrefab.lastCopiedPrefabPosition, prefabInstance.prefab.size, true);
		prefabInstance.rotation = prefabInstance.lastCopiedRotation;
		prefabInstance.UpdateImposterView();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		PathAbstractions.AbstractedLocation abstractedLocation = PathAbstractions.AbstractedLocation.None;
		if (this.fileList.SelectedEntry != null)
		{
			abstractedLocation = this.fileList.SelectedEntry.GetEntry().location;
		}
		if (this.groupList.SelectedEntry != null)
		{
			string name = this.groupList.SelectedEntry.GetEntry().name;
			this.groupList.RebuildList(false);
			if (!this.groupList.SelectByName(name))
			{
				this.groupList.SelectedEntryIndex = 0;
			}
		}
		this.fileList.RebuildList(false);
		if (abstractedLocation.Type != PathAbstractions.EAbstractedLocationType.None)
		{
			this.fileList.SelectByLocation(abstractedLocation);
		}
		base.RefreshBindings(false);
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		bool enabled = PrefabEditModeManager.Instance.VoxelPrefab != null;
		this.btnSave.Enabled = enabled;
		BlockToolSelection blockToolSelection = GameManager.Instance.GetActiveBlockTool() as BlockToolSelection;
		DynamicPrefabDecorator dynamicPrefabDecorator = GameManager.Instance.GetDynamicPrefabDecorator();
		PrefabInstance prefabInstance = (dynamicPrefabDecorator != null) ? dynamicPrefabDecorator.ActivePrefab : null;
		this.btnLoadIntoPrefab.Enabled = (this.fileList.SelectedEntry != null && blockToolSelection != null && blockToolSelection.SelectionActive && dynamicPrefabDecorator != null);
		this.btnApplyLoadedPrefab.Enabled = (prefabInstance != null && !prefabInstance.IsBBInSyncWithPrefab());
		this.btnWorldPlacePrefab.Enabled = (blockToolSelection != null && blockToolSelection.SelectionActive && this.fileList.SelectedEntry != null);
		this.btnWorldReplacePrefab.Enabled = (prefabInstance != null && this.fileList.SelectedEntry != null);
		this.btnWorldDeletePrefab.Enabled = (prefabInstance != null);
		this.btnWorldApplyPrefabChanges.Enabled = (prefabInstance != null && !prefabInstance.IsBBInSyncWithPrefab());
		this.btnWorldRevertPrefabChanges.Enabled = (prefabInstance != null && !prefabInstance.IsBBInSyncWithPrefab() && prefabInstance.bPrefabCopiedIntoWorld);
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_PrefabGroupList groupList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_PrefabFileList fileList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Texture prefabPreview;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label noPreviewLabel;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnLoad;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnProperties;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnSave;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnLoadIntoPrefab;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnApplyLoadedPrefab;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnWorldPlacePrefab;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnWorldReplacePrefab;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnWorldDeletePrefab;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnWorldApplyPrefabChanges;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnWorldRevertPrefabChanges;
}

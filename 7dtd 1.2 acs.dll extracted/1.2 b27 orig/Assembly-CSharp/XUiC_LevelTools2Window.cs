using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_LevelTools2Window : XUiController
{
	public override void Init()
	{
		base.Init();
		XUiC_LevelTools2Window.ID = base.WindowGroup.ID;
		this.layoutTable = (base.GetChildById("layoutTable").ViewComponent as XUiV_Table);
		XUiController childById = base.GetChildById("toggleGroundGrid");
		this.toggleGroundGrid = ((childById != null) ? childById.GetChildByType<XUiC_ToggleButton>() : null);
		if (this.toggleGroundGrid != null)
		{
			this.toggleGroundGrid.OnValueChanged += this.ToggleGroundGrid_OnValueChanged;
		}
		this.btnMoveGroundGridUp = (base.GetChildById("btnMoveGroundGridUp") as XUiC_SimpleButton);
		if (this.btnMoveGroundGridUp != null)
		{
			this.btnMoveGroundGridUp.OnPressed += this.BtnMoveGroundGridUp_OnPressed;
		}
		this.btnMoveGroundGridDown = (base.GetChildById("btnMoveGroundGridDown") as XUiC_SimpleButton);
		if (this.btnMoveGroundGridDown != null)
		{
			this.btnMoveGroundGridDown.OnPressed += this.BtnMoveGroundGridDown_OnPressed;
		}
		this.btnMovePrefabUp = (base.GetChildById("btnMovePrefabUp") as XUiC_SimpleButton);
		if (this.btnMovePrefabUp != null)
		{
			this.btnMovePrefabUp.OnPressed += this.BtnMovePrefabUp_OnPressed;
		}
		this.btnMovePrefabDown = (base.GetChildById("btnMovePrefabDown") as XUiC_SimpleButton);
		if (this.btnMovePrefabDown != null)
		{
			this.btnMovePrefabDown.OnPressed += this.BtnMovePrefabDown_OnPressed;
		}
		this.btnUpdateBounds = (base.GetChildById("btnUpdateBounds") as XUiC_SimpleButton);
		if (this.btnUpdateBounds != null)
		{
			this.btnUpdateBounds.OnPressed += this.BtnUpdateBoundsOnOnPressed;
		}
		XUiController childById2 = base.GetChildById("toggleShowFacing");
		this.toggleShowFacing = ((childById2 != null) ? childById2.GetChildByType<XUiC_ToggleButton>() : null);
		if (this.toggleShowFacing != null)
		{
			this.toggleShowFacing.OnValueChanged += this.ToggleShowFacing_OnValueChanged;
		}
		this.btnUpdateFacing = (base.GetChildById("btnUpdateFacing") as XUiC_SimpleButton);
		if (this.btnUpdateFacing != null)
		{
			this.btnUpdateFacing.OnPressed += this.BtnUpdateFacingOnOnPressed;
		}
		this.txtOldId = (base.GetChildById("txtOldId") as XUiC_DropDown);
		this.txtNewId = (base.GetChildById("txtNewId") as XUiC_DropDown);
		if (this.txtOldId != null)
		{
			this.txtOldId.OnChangeHandler += this.ReplaceBlockIds_OnChangeHandler;
			this.txtOldId.OnSubmitHandler += this.ReplaceBlockIds_OnSubmitHandler;
			XUiC_TextInput textInput = this.txtOldId.TextInput;
			XUiC_DropDown xuiC_DropDown = this.txtNewId;
			textInput.SelectOnTab = ((xuiC_DropDown != null) ? xuiC_DropDown.TextInput : null);
		}
		if (this.txtNewId != null)
		{
			this.txtNewId.OnChangeHandler += this.ReplaceBlockIds_OnChangeHandler;
			this.txtNewId.OnSubmitHandler += this.ReplaceBlockIds_OnSubmitHandler;
			XUiC_TextInput textInput2 = this.txtNewId.TextInput;
			XUiC_DropDown xuiC_DropDown2 = this.txtOldId;
			textInput2.SelectOnTab = ((xuiC_DropDown2 != null) ? xuiC_DropDown2.TextInput : null);
		}
		this.btnReplaceBlockIds = (base.GetChildById("btnReplaceBlockIds") as XUiC_SimpleButton);
		if (this.btnReplaceBlockIds != null)
		{
			this.btnReplaceBlockIds.OnPressed += this.BtnReplaceBlockIds_OnPressed;
		}
		this.toggleHighlightBlocks = (base.GetChildById("toggleHighlightBlocks") as XUiC_ToggleButton);
		if (this.toggleHighlightBlocks != null)
		{
			this.toggleHighlightBlocks.OnValueChanged += this.ToggleHighlightBlocks_OnValueChanged;
		}
		this.txtHighlightBlockName = (base.GetChildById("txtHighlightBlockName") as XUiC_DropDown);
		if (this.txtHighlightBlockName != null)
		{
			this.txtHighlightBlockName.OnChangeHandler += this.HighlightBlock_OnChangeHandler;
			this.txtHighlightBlockName.OnSubmitHandler += this.HighlightBlock_OnSubmitHandler;
		}
		XUiController childById3 = base.GetChildById("toggleScreenshotBounds");
		XUiC_ToggleButton xuiC_ToggleButton = (childById3 != null) ? childById3.GetChildByType<XUiC_ToggleButton>() : null;
		if (xuiC_ToggleButton != null)
		{
			xuiC_ToggleButton.OnValueChanged += this.ToggleScreenshotBounds_OnValueChanged;
		}
		this.btnTakeScreenshot = (base.GetChildById("btnTakeScreenshot") as XUiC_SimpleButton);
		if (this.btnTakeScreenshot != null)
		{
			this.btnTakeScreenshot.OnPressed += this.BtnTakeScreenshot_OnPressed;
		}
		this.btnUpdateImposter = (base.GetChildById("btnUpdateImposter") as XUiC_SimpleButton);
		if (this.btnUpdateImposter != null)
		{
			this.btnUpdateImposter.OnPressed += this.BtnUpdateImposterOnOnPressed;
		}
		XUiController childById4 = base.GetChildById("toggleShowImposter");
		this.toggleShowImposter = ((childById4 != null) ? childById4.GetChildByType<XUiC_ToggleButton>() : null);
		if (this.toggleShowImposter != null)
		{
			this.toggleShowImposter.OnValueChanged += this.ToggleShowImposterOnOnValueChanged;
		}
		this.btnPrefabProperties = (base.GetChildById("btnPrefabProperties") as XUiC_SimpleButton);
		if (this.btnPrefabProperties != null)
		{
			this.btnPrefabProperties.OnPressed += this.BtnPrefabPropertiesOnOnPressed;
		}
		this.btnStripTextures = (base.GetChildById("btnStripTextures") as XUiC_SimpleButton);
		if (this.btnStripTextures != null)
		{
			this.btnStripTextures.OnPressed += this.BtnStripTexturesOnPressed;
		}
		XUiC_SimpleButton xuiC_SimpleButton = base.GetChildById("btnStripInternalTextures") as XUiC_SimpleButton;
		if (xuiC_SimpleButton != null)
		{
			xuiC_SimpleButton.OnPressed += this.BtnStripInternalTexturesOnPressed;
		}
		this.btnCleanDensity = (base.GetChildById("btnCleanDensity") as XUiC_SimpleButton);
		if (this.btnCleanDensity != null)
		{
			this.btnCleanDensity.OnPressed += this.BtnCleanDensityOnPressed;
		}
		XUiC_SimpleButton xuiC_SimpleButton2 = base.GetChildById("btnCapturePrefabStats") as XUiC_SimpleButton;
		if (xuiC_SimpleButton2 != null)
		{
			xuiC_SimpleButton2.OnPressed += this.BtnCapturePrefabStatsOnPressed;
		}
		XUiC_SimpleButton xuiC_SimpleButton3 = base.GetChildById("btnPOIMarkers") as XUiC_SimpleButton;
		if (xuiC_SimpleButton3 != null)
		{
			xuiC_SimpleButton3.OnPressed += this.BtnPOIMarkers_OnPressed;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnPOIMarkers_OnPressed(XUiController _sender, int _mouseButton)
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ToggleGroundGrid_OnValueChanged(XUiC_ToggleButton _sender, bool _newValue)
	{
		PrefabEditModeManager.Instance.ToggleGroundGrid(false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnMoveGroundGridUp_OnPressed(XUiController _sender, int _mouseButton)
	{
		PrefabEditModeManager.Instance.MoveGroundGridUpOrDown(1);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnMoveGroundGridDown_OnPressed(XUiController _sender, int _mouseButton)
	{
		PrefabEditModeManager.Instance.MoveGroundGridUpOrDown(-1);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnMovePrefabUp_OnPressed(XUiController _sender, int _mouseButton)
	{
		PrefabEditModeManager.Instance.MovePrefabUpOrDown(1);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnMovePrefabDown_OnPressed(XUiController _sender, int _mouseButton)
	{
		PrefabEditModeManager.Instance.MovePrefabUpOrDown(-1);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnUpdateBoundsOnOnPressed(XUiController _sender, int _mouseButton)
	{
		PrefabEditModeManager.Instance.UpdatePrefabBounds();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ToggleShowFacing_OnValueChanged(XUiC_ToggleButton _sender, bool _newvalue)
	{
		PrefabEditModeManager.Instance.TogglePrefabFacing(this.toggleShowFacing.Value);
		this.btnUpdateFacing.Enabled = this.toggleShowFacing.Value;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnUpdateFacingOnOnPressed(XUiController _sender, int _mouseButton)
	{
		PrefabEditModeManager.Instance.RotatePrefabFacing();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ReplaceBlockIds_OnChangeHandler(XUiController _sender, string _text, bool _changefromcode)
	{
		bool flag = Block.GetBlockByName(this.txtOldId.Text, true) != null;
		bool flag2 = Block.GetBlockByName(this.txtNewId.Text, true) != null;
		this.txtOldId.TextInput.ActiveTextColor = (flag ? Color.white : Color.red);
		this.txtNewId.TextInput.ActiveTextColor = (flag2 ? Color.white : Color.red);
		this.btnReplaceBlockIds.Enabled = (flag && flag2);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ReplaceBlockIds_OnSubmitHandler(XUiController _sender, string _text)
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnReplaceBlockIds_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.replaceBlockId();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ToggleHighlightBlocks_OnValueChanged(XUiC_ToggleButton _sender, bool _newValue)
	{
		if (_newValue)
		{
			this.HighlightBlock_OnSubmitHandler(_sender, this.txtHighlightBlockName.Text);
			return;
		}
		PrefabEditModeManager.Instance.HighlightBlocks(null);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HighlightBlock_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		bool flag = Block.GetBlockByName(this.txtHighlightBlockName.Text, true) != null;
		this.txtHighlightBlockName.TextInput.ActiveTextColor = (flag ? Color.white : Color.red);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HighlightBlock_OnSubmitHandler(XUiController _sender, string _text)
	{
		Block blockByName = Block.GetBlockByName(this.txtHighlightBlockName.Text, true);
		if (this.toggleHighlightBlocks != null)
		{
			this.toggleHighlightBlocks.Value = true;
		}
		if (blockByName != null)
		{
			PrefabEditModeManager.Instance.HighlightBlocks(blockByName);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void replaceBlockId()
	{
		if (!this.btnReplaceBlockIds.Enabled)
		{
			return;
		}
		Block srcBlockClass = Block.GetBlockByName(this.txtOldId.Text, true);
		Block dstBlockClass = Block.GetBlockByName(this.txtNewId.Text, true);
		if (srcBlockClass == null)
		{
			return;
		}
		if (dstBlockClass == null)
		{
			return;
		}
		int sourceBlockId = srcBlockClass.blockID;
		int targetBlockId = dstBlockClass.blockID;
		HashSet<Chunk> changedChunks = new HashSet<Chunk>();
		bool bUseSelection = BlockToolSelection.Instance.SelectionActive;
		Vector3i selStart = BlockToolSelection.Instance.SelectionMin;
		Vector3i selEnd = selStart + BlockToolSelection.Instance.SelectionSize - Vector3i.one;
		List<Chunk> chunkArrayCopySync = GameManager.Instance.World.ChunkCache.GetChunkArrayCopySync();
		for (int i = 0; i < chunkArrayCopySync.Count; i++)
		{
			Chunk curChunk = chunkArrayCopySync[i];
			curChunk.LoopOverAllBlocks(delegate(int _x, int _y, int _z, BlockValue _bv)
			{
				if (_bv.type != sourceBlockId)
				{
					return;
				}
				if (bUseSelection)
				{
					Vector3i vector3i = curChunk.ToWorldPos(new Vector3i(_x, _y, _z));
					if (vector3i.x < selStart.x || vector3i.x > selEnd.x || vector3i.y < selStart.y || vector3i.y > selEnd.y || vector3i.z < selStart.z || vector3i.z > selEnd.z)
					{
						return;
					}
				}
				if (srcBlockClass.shape.IsTerrain() != dstBlockClass.shape.IsTerrain())
				{
					sbyte b = curChunk.GetDensity(_x, _y, _z);
					if (dstBlockClass.shape.IsTerrain())
					{
						b = MarchingCubes.DensityTerrain;
					}
					else if (b != 0)
					{
						b = MarchingCubes.DensityAir;
					}
					curChunk.SetDensity(_x, _y, _z, b);
				}
				BlockValue blockValue = new BlockValue((uint)targetBlockId)
				{
					rotation = _bv.rotation,
					meta = _bv.meta
				};
				curChunk.SetBlockRaw(_x, _y, _z, blockValue);
				changedChunks.Add(curChunk);
			}, false, true);
		}
		foreach (Chunk chunk in changedChunks)
		{
			chunk.NeedsRegeneration = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void replacePaint()
	{
		int sourcePaintId;
		if (!int.TryParse(this.txtOldId.Text, out sourcePaintId))
		{
			return;
		}
		int targetPaintId;
		if (!int.TryParse(this.txtNewId.Text, out targetPaintId))
		{
			return;
		}
		HashSet<Chunk> changedChunks = new HashSet<Chunk>();
		bool bUseSelection = BlockToolSelection.Instance.SelectionActive;
		Vector3i selStart = BlockToolSelection.Instance.SelectionMin;
		Vector3i selEnd = selStart + BlockToolSelection.Instance.SelectionSize - Vector3i.one;
		List<Chunk> chunkArrayCopySync = GameManager.Instance.World.ChunkCache.GetChunkArrayCopySync();
		for (int i = 0; i < chunkArrayCopySync.Count; i++)
		{
			Chunk curChunk = chunkArrayCopySync[i];
			curChunk.LoopOverAllBlocks(delegate(int _x, int _y, int _z, BlockValue _bv)
			{
				if (bUseSelection)
				{
					Vector3i vector3i = curChunk.ToWorldPos(new Vector3i(_x, _y, _z));
					if (vector3i.x < selStart.x || vector3i.x > selEnd.x || vector3i.y < selStart.y || vector3i.y > selEnd.y || vector3i.z < selStart.z || vector3i.z > selEnd.z)
					{
						return;
					}
				}
				bool flag = false;
				long num = curChunk.GetTextureFull(_x, _y, _z);
				for (int j = 0; j < 6; j++)
				{
					if ((num >> j * 8 & 255L) == (long)sourcePaintId)
					{
						num &= ~(255L << j * 8);
						num |= (long)targetPaintId << j * 8;
						flag = true;
					}
				}
				if (flag)
				{
					curChunk.SetTextureFull(_x, _y, _z, num);
					changedChunks.Add(curChunk);
				}
			}, false, false);
		}
		foreach (Chunk chunk in changedChunks)
		{
			chunk.NeedsRegeneration = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ToggleScreenshotBounds_OnValueChanged(XUiC_ToggleButton _sender, bool _newvalue)
	{
		this.drawingScreenshotGuide = _newvalue;
		if (_newvalue)
		{
			ThreadManager.StartCoroutine(this.drawScreenshotGuide());
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator drawScreenshotGuide()
	{
		while (this.drawingScreenshotGuide)
		{
			yield return new WaitForEndOfFrame();
			Rect screenshotRect = GameUtils.GetScreenshotRect(0.15f, true);
			screenshotRect = new Rect(screenshotRect.x - 2f, screenshotRect.y - 2f, screenshotRect.width + 4f, screenshotRect.height + 4f);
			GUIUtils.DrawRect(screenshotRect, Color.green);
		}
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnTakeScreenshot_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (PrefabEditModeManager.Instance.VoxelPrefab == null)
		{
			GameManager.ShowTooltip(base.xui.playerUI.entityPlayer, "[FF4444]" + Localization.Get("xuiScreenshotNoPrefabLoaded", false), false);
			return;
		}
		string fullPathNoExtension = PrefabEditModeManager.Instance.LoadedPrefab.FullPathNoExtension;
		ThreadManager.StartCoroutine(this.screenshotCo(fullPathNoExtension));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ToggleShowImposterOnOnValueChanged(XUiC_ToggleButton _sender, bool _newValue)
	{
		if (_newValue)
		{
			XUiC_SaveDirtyPrefab.Show(base.xui, new Action<XUiC_SaveDirtyPrefab.ESelectedAction>(this.showImposter), XUiC_SaveDirtyPrefab.EMode.AskSaveIfDirty);
			return;
		}
		this.showPrefab();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnUpdateImposterOnOnPressed(XUiController _sender, int _mouseButton)
	{
		XUiC_SaveDirtyPrefab.Show(base.xui, new Action<XUiC_SaveDirtyPrefab.ESelectedAction>(this.updateImposter), XUiC_SaveDirtyPrefab.EMode.AskSaveIfDirty);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnPrefabPropertiesOnOnPressed(XUiController _sender, int _mouseButton)
	{
		XUiC_PrefabPropertiesEditor.Show(base.xui, XUiC_PrefabPropertiesEditor.EPropertiesFrom.LoadedPrefab, PathAbstractions.AbstractedLocation.None);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnStripTexturesOnPressed(XUiController _sender, int _mouseButton)
	{
		PrefabEditModeManager.Instance.StripTextures();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnStripInternalTexturesOnPressed(XUiController _sender, int _mouseButton)
	{
		PrefabEditModeManager.Instance.StripInternalTextures();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCleanDensityOnPressed(XUiController _sender, int _mouseButton)
	{
		if (!this.btnCleanDensity.Enabled)
		{
			return;
		}
		HashSet<Chunk> changedChunks = new HashSet<Chunk>();
		bool bUseSelection = BlockToolSelection.Instance.SelectionActive;
		Vector3i selStart = BlockToolSelection.Instance.SelectionMin;
		Vector3i selEnd = selStart + BlockToolSelection.Instance.SelectionSize - Vector3i.one;
		List<Chunk> chunkArrayCopySync = GameManager.Instance.World.ChunkCache.GetChunkArrayCopySync();
		for (int i = 0; i < chunkArrayCopySync.Count; i++)
		{
			Chunk curChunk = chunkArrayCopySync[i];
			curChunk.LoopOverAllBlocks(delegate(int _x, int _y, int _z, BlockValue _bv)
			{
				if (bUseSelection)
				{
					Vector3i vector3i = curChunk.ToWorldPos(new Vector3i(_x, _y, _z));
					if (vector3i.x < selStart.x || vector3i.x > selEnd.x || vector3i.y < selStart.y || vector3i.y > selEnd.y || vector3i.z < selStart.z || vector3i.z > selEnd.z)
					{
						return;
					}
				}
				Block block = _bv.Block;
				sbyte density = curChunk.GetDensity(_x, _y, _z);
				sbyte b = block.shape.IsTerrain() ? MarchingCubes.DensityTerrain : MarchingCubes.DensityAir;
				if (b != density)
				{
					curChunk.SetDensity(_x, _y, _z, b);
					changedChunks.Add(curChunk);
				}
			}, false, true);
		}
		foreach (Chunk chunk in changedChunks)
		{
			chunk.NeedsRegeneration = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCapturePrefabStatsOnPressed(XUiController _sender, int _mouseButton)
	{
		if (PrefabEditModeManager.Instance.VoxelPrefab == null)
		{
			GameManager.ShowTooltip(base.xui.playerUI.entityPlayer, "[FF4444]" + Localization.Get("xuiPrefabStatsNoPrefabLoaded", false), false);
			return;
		}
		XUiC_EditorStat.ManualStats = WorldStats.CaptureWorldStats();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator screenshotCo(string _filename)
	{
		base.xui.playerUI.windowManager.TempHUDDisable();
		EntityPlayerLocal player = GameManager.Instance.World.GetPrimaryPlayer();
		bool isSpectator = player.IsSpectator;
		player.IsSpectator = true;
		SkyManager.SetSkyEnabled(false);
		yield return null;
		try
		{
			GameUtils.TakeScreenShot(GameUtils.EScreenshotMode.File, _filename, 0.15f, true, 280, 210, false);
		}
		catch (Exception e)
		{
			Log.Exception(e);
		}
		yield return null;
		player.IsSpectator = isSpectator;
		SkyManager.SetSkyEnabled(true);
		base.xui.playerUI.windowManager.ReEnableHUD();
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateImposter(XUiC_SaveDirtyPrefab.ESelectedAction _action)
	{
		base.xui.playerUI.windowManager.Open(XUiC_InGameMenuWindow.ID, true, false, true);
		if (_action == XUiC_SaveDirtyPrefab.ESelectedAction.Cancel)
		{
			return;
		}
		base.xui.playerUI.windowManager.TempHUDDisable();
		PrefabHelpers.convert(new Action(this.waitForUpdateImposter));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void waitForUpdateImposter()
	{
		PrefabHelpers.Cleanup();
		if (this.toggleShowImposter.Value)
		{
			PrefabEditModeManager.Instance.LoadImposterPrefab(PrefabEditModeManager.Instance.LoadedPrefab);
		}
		else
		{
			PrefabEditModeManager.Instance.LoadVoxelPrefab(PrefabEditModeManager.Instance.LoadedPrefab, false, false);
		}
		base.xui.playerUI.windowManager.ReEnableHUD();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void showImposter(XUiC_SaveDirtyPrefab.ESelectedAction _action)
	{
		base.xui.playerUI.windowManager.Open(XUiC_InGameMenuWindow.ID, true, false, true);
		if (_action == XUiC_SaveDirtyPrefab.ESelectedAction.Cancel)
		{
			return;
		}
		PathAbstractions.AbstractedLocation loadedPrefab = PrefabEditModeManager.Instance.LoadedPrefab;
		PrefabEditModeManager.Instance.ClearImposterPrefab();
		if (PrefabEditModeManager.Instance.HasPrefabImposter(loadedPrefab))
		{
			PrefabEditModeManager.Instance.LoadImposterPrefab(loadedPrefab);
			return;
		}
		GameManager.ShowTooltip(GameManager.Instance.World.GetLocalPlayers()[0], "Prefab " + loadedPrefab.Name + " has no imposter yet", false);
		this.toggleShowImposter.Value = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void showPrefab()
	{
		if (PrefabEditModeManager.Instance.LoadedPrefab.Type != PathAbstractions.EAbstractedLocationType.None)
		{
			PrefabEditModeManager.Instance.LoadVoxelPrefab(PrefabEditModeManager.Instance.LoadedPrefab, false, false);
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.ReplaceBlockIds_OnChangeHandler(this, null, true);
		this.toggleShowFacing.Value = PrefabEditModeManager.Instance.IsPrefabFacing();
		this.btnUpdateFacing.Enabled = this.toggleShowFacing.Value;
		if (!this.blockListsInitDone)
		{
			List<string> list = new List<string>();
			foreach (Block block in Block.list)
			{
				if (block != null)
				{
					list.Add(block.GetBlockName());
				}
			}
			this.txtOldId.AllEntries.AddRange(list);
			this.txtOldId.UpdateFilteredList();
			this.txtNewId.AllEntries.AddRange(list);
			this.txtNewId.UpdateFilteredList();
			this.txtHighlightBlockName.AllEntries.AddRange(list);
			this.txtHighlightBlockName.UpdateFilteredList();
			this.blockListsInitDone = true;
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		bool flag = PrefabEditModeManager.Instance.VoxelPrefab != null;
		bool isServer = SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer;
		bool flag2 = PrefabEditModeManager.Instance.IsGroundGrid();
		this.btnMoveGroundGridDown.Enabled = (flag2 && isServer);
		this.btnMoveGroundGridUp.Enabled = (flag2 && isServer);
		this.toggleGroundGrid.Value = flag2;
		this.btnMovePrefabDown.Enabled = (flag && isServer);
		this.btnMovePrefabUp.Enabled = (flag && isServer);
		this.toggleShowImposter.Enabled = isServer;
		this.btnUpdateBounds.Enabled = (flag && isServer);
		this.btnTakeScreenshot.Enabled = (flag && isServer);
		this.btnUpdateImposter.Enabled = (PrefabEditModeManager.Instance.LoadedPrefab.Type != PathAbstractions.EAbstractedLocationType.None && isServer);
		this.btnPrefabProperties.Enabled = (isServer && flag);
		this.btnStripTextures.Enabled = (isServer && flag);
		this.btnCleanDensity.Enabled = (isServer && flag);
	}

	public override void Cleanup()
	{
		base.Cleanup();
		this.drawingScreenshotGuide = false;
	}

	public static bool IsShowImposter(XUi _xui)
	{
		return ((XUiWindowGroup)_xui.playerUI.windowManager.GetWindow(XUiC_LevelTools2Window.ID)).Controller.GetChildByType<XUiC_LevelTools2Window>().toggleShowImposter.Value;
	}

	public static void SetShowImposter(XUi _xui, bool _showImposter)
	{
		((XUiWindowGroup)_xui.playerUI.windowManager.GetWindow(XUiC_LevelTools2Window.ID)).Controller.GetChildByType<XUiC_LevelTools2Window>().toggleShowImposter.Value = _showImposter;
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public const float screenshotBorderPercentage = 0.15f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const bool screenshot4To3 = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Table layoutTable;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ToggleButton toggleGroundGrid;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnMoveGroundGridUp;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnMoveGroundGridDown;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnMovePrefabUp;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnMovePrefabDown;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnUpdateBounds;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ToggleButton toggleShowFacing;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnUpdateFacing;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_DropDown txtOldId;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_DropDown txtNewId;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnReplaceBlockIds;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ToggleButton toggleHighlightBlocks;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_DropDown txtHighlightBlockName;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnTakeScreenshot;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnUpdateImposter;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ToggleButton toggleShowImposter;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnPrefabProperties;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnStripTextures;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnCleanDensity;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool blockListsInitDone;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool drawingScreenshotGuide;
}

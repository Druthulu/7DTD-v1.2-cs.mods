using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_CreateWorld : XUiController
{
	public bool IsCustomSize
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.cmbSize != null && this.cmbSize.Value == -1;
		}
	}

	public int NewWorldSize
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (!this.IsCustomSize)
			{
				XUiC_ComboBoxList<int> xuiC_ComboBoxList = this.cmbSize;
				if (xuiC_ComboBoxList == null)
				{
					return -1;
				}
				return xuiC_ComboBoxList.Value;
			}
			else
			{
				int result;
				if (this.txtSize != null && int.TryParse(this.txtSize.Text, out result))
				{
					return result;
				}
				return -1;
			}
		}
	}

	public bool CustomSizeValid
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			int num;
			return this.txtSize != null && int.TryParse(this.txtSize.Text, out num) && num >= 1024 && num % 1024 == 0;
		}
	}

	public bool CustomNameValid
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.txtNameInput != null && this.txtNameInput.Text.Trim().Length > 0 && !SdDirectory.Exists(GameIO.GetGameDir("Data/Worlds") + "/" + this.txtNameInput.Text);
		}
	}

	public override void Init()
	{
		base.Init();
		XUiC_CreateWorld.ID = base.WindowGroup.ID;
		this.toggleSelectWorld = base.GetChildById("toggleSelectWorld").GetChildByType<XUiC_ToggleButton>();
		this.toggleSelectWorld.OnValueChanged += this.ToggleSelectWorld_OnValueChanged;
		this.worldList = (base.GetChildById("worlds") as XUiC_WorldList);
		this.worldList.SelectionChanged += this.WorldList_SelectionChanged;
		this.worldList.OnEntryDoubleClicked += this.WorldList_OnEntryDoubleClicked;
		this.worldList.OnEntryClicked += this.WorldList_OnEntryClicked;
		this.savesList = (base.GetChildById("saves") as XUiC_SavegamesList);
		this.savesList.SelectionChanged += this.SavesList_SelectionChanged;
		XUiController childById = base.GetChildById("toggleNewWorld");
		this.toggleNewWorld = ((childById != null) ? childById.GetChildByType<XUiC_ToggleButton>() : null);
		this.txtNameInput = (base.GetChildById("nameInput") as XUiC_TextInput);
		this.txtSize = (base.GetChildById("txtSize") as XUiC_TextInput);
		this.cmbSize = base.GetChildById("cmbSize").GetChildByType<XUiC_ComboBoxList<int>>();
		this.toggleNewWorld.OnValueChanged += this.ToggleNewWorld_OnValueChanged;
		this.txtNameInput.OnChangeHandler += this.TxtInput_OnChangeHandler;
		this.txtSize.OnChangeHandler += this.TxtInput_OnChangeHandler;
		this.cmbSize.OnValueChanged += this.CmbSize_OnValueChanged;
		this.txtSize.OnScroll += delegate(XUiController _sender, float _args)
		{
			this.cmbSize.ScrollEvent(_sender, _args);
		};
		XUiC_SimpleButton xuiC_SimpleButton = base.GetChildById("btnBack") as XUiC_SimpleButton;
		if (xuiC_SimpleButton != null)
		{
			xuiC_SimpleButton.OnPressed += this.BtnBack_OnPressed;
		}
		XUiC_SimpleButton xuiC_SimpleButton2 = base.GetChildById("btnDeleteWorld") as XUiC_SimpleButton;
		if (xuiC_SimpleButton2 != null)
		{
			xuiC_SimpleButton2.OnPressed += this.BtnDeleteWorld_OnPressed;
		}
		XUiC_SimpleButton xuiC_SimpleButton3 = base.GetChildById("btnDeleteSave") as XUiC_SimpleButton;
		if (xuiC_SimpleButton3 != null)
		{
			xuiC_SimpleButton3.OnPressed += this.BtnDeleteSave_OnPressed;
		}
		XUiC_SimpleButton xuiC_SimpleButton4 = base.GetChildById("btnStart") as XUiC_SimpleButton;
		if (xuiC_SimpleButton4 != null)
		{
			xuiC_SimpleButton4.OnPressed += this.BtnStart_OnPressed;
		}
		this.deleteSavePanel = (XUiV_Panel)base.GetChildById("deleteSavePanel").ViewComponent;
		XUiC_SimpleButton xuiC_SimpleButton5 = (XUiC_SimpleButton)this.deleteSavePanel.Controller.GetChildById("btnCancel");
		XUiC_SimpleButton xuiC_SimpleButton6 = (XUiC_SimpleButton)this.deleteSavePanel.Controller.GetChildById("btnConfirm");
		xuiC_SimpleButton5.OnPressed += this.BtnCancelDelete_OnPressed;
		xuiC_SimpleButton6.OnPressed += this.BtnConfirmDelete_OnPressed;
		this.deleteSaveText = (XUiV_Label)this.deleteSavePanel.Controller.GetChildById("deleteText").ViewComponent;
		this.deleteHeaderText = (XUiV_Label)this.deleteSavePanel.Controller.GetChildById("headerText").ViewComponent;
		XUiV_Panel xuiV_Panel = (XUiV_Panel)base.GetChildById("worldInfo").ViewComponent;
		this.worldLastPlayedLabel = (XUiV_Label)xuiV_Panel.Controller.GetChildById("worldLastPlayedLabel").ViewComponent;
		this.worldVersionLabel = (XUiV_Label)xuiV_Panel.Controller.GetChildById("worldVersionLabel").ViewComponent;
		this.worldMemoryLabel = (XUiV_Label)xuiV_Panel.Controller.GetChildById("worldMemoryLabel").ViewComponent;
		XUiV_Panel xuiV_Panel2 = (XUiV_Panel)base.GetChildById("saveInfo").ViewComponent;
		this.saveLastPlayedLabel = (XUiV_Label)xuiV_Panel2.Controller.GetChildById("saveLastPlayedLabel").ViewComponent;
		this.saveVersionLabel = (XUiV_Label)xuiV_Panel2.Controller.GetChildById("saveVersionLabel").ViewComponent;
		this.saveMemoryLabel = (XUiV_Label)xuiV_Panel2.Controller.GetChildById("saveMemoryLabel").ViewComponent;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.worldDataSizeCache.Clear();
		this.saveSizeCache.Clear();
		this.windowGroup.openWindowOnEsc = XUiC_EditingTools.ID;
		this.worldList.RebuildList(false);
		this.txtNameInput.Text = GamePrefs.GetString(EnumGamePrefs.CreateLevelName);
		int num;
		if (int.TryParse(GamePrefs.GetString(EnumGamePrefs.CreateLevelDim), out num))
		{
			if (this.cmbSize.Elements.Contains(num))
			{
				this.cmbSize.Value = num;
			}
			else
			{
				this.cmbSize.SelectedIndex = 0;
				this.txtSize.Text = num.ToString();
			}
		}
		else
		{
			this.txtSize.Text = "";
		}
		this.deleteSavePanel.IsVisible = false;
		this.toggleSelectWorld.Value = true;
		this.toggleNewWorld.Value = false;
		if (string.IsNullOrEmpty(GamePrefs.GetString(EnumGamePrefs.GameWorld)) || !this.worldList.SelectByName(GamePrefs.GetString(EnumGamePrefs.GameWorld)))
		{
			this.worldList.SelectedEntryIndex = 0;
		}
		this.RefreshButtonStates();
		this.IsDirty = true;
	}

	public override void OnClose()
	{
		base.OnClose();
		this.worldDataSizeCache.Clear();
		this.saveSizeCache.Clear();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (!this.IsDirty)
		{
			return;
		}
		base.RefreshBindings(true);
		this.IsDirty = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ToggleSelectWorld_OnValueChanged(XUiC_ToggleButton _sender, bool _newValue)
	{
		if (_newValue)
		{
			this.toggleNewWorld.Value = false;
		}
		else
		{
			this.toggleSelectWorld.Value = true;
		}
		this.RefreshButtonStates();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WorldList_SelectionChanged(XUiC_ListEntry<XUiC_WorldList.WorldListEntry> _previousEntry, XUiC_ListEntry<XUiC_WorldList.WorldListEntry> _newEntry)
	{
		string worldFilter = "";
		if (_newEntry != null)
		{
			this.toggleSelectWorld.Value = true;
			this.toggleNewWorld.Value = false;
			XUiC_WorldList.WorldListEntry entry = _newEntry.GetEntry();
			worldFilter = ((entry != null) ? entry.Location.Name : null);
		}
		this.savesList.SetWorldFilter(worldFilter);
		this.savesList.RebuildList(false);
		this.RefreshInfoLabels();
		this.RefreshButtonStates();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WorldList_OnEntryClicked(XUiController _sender, int _mouseButton)
	{
		this.savesList.ClearSelection();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SavesList_SelectionChanged(XUiC_ListEntry<XUiC_SavegamesList.ListEntry> _previousEntry, XUiC_ListEntry<XUiC_SavegamesList.ListEntry> _newEntry)
	{
		this.RefreshInfoLabels();
		this.RefreshButtonStates();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WorldList_OnEntryDoubleClicked(XUiController _sender, int _mouseButton)
	{
		this.toggleSelectWorld.Value = true;
		this.toggleNewWorld.Value = false;
		if (this.worldList.SelectedEntryIndex >= 0)
		{
			this.BtnStart_OnPressed(_sender, _mouseButton);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TxtInput_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		this.worldList.ClearSelection();
		this.toggleSelectWorld.Value = false;
		this.toggleNewWorld.Value = true;
		this.RefreshButtonStates();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ToggleNewWorld_OnValueChanged(XUiC_ToggleButton _sender, bool _newValue)
	{
		if (_newValue)
		{
			this.worldList.ClearSelection();
			this.toggleSelectWorld.Value = false;
		}
		else
		{
			this.toggleNewWorld.Value = true;
		}
		this.RefreshButtonStates();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CmbSize_OnValueChanged(XUiController _sender, int _oldvalue, int _newvalue)
	{
		this.RefreshButtonStates();
		if (_newvalue < 0 && PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard)
		{
			this.txtSize.SetSelected(true, true);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnBack_OnPressed(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(this.windowGroup.ID);
		base.xui.playerUI.windowManager.Open(XUiC_EditingTools.ID, true, false, true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDeleteWorld_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.deleteWorldGetConfirmation();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void deleteWorldGetConfirmation()
	{
		this.deleteSavePanel.IsVisible = true;
		this.deleteHeaderText.Text = Localization.Get("xuiWorldDelete", false);
		XUiC_SavegamesList xuiC_SavegamesList = this.savesList;
		XUiC_ListEntry<XUiC_WorldList.WorldListEntry> selectedEntry = this.worldList.SelectedEntry;
		int num = xuiC_SavegamesList.GetSavesInWorld((selectedEntry != null) ? selectedEntry.GetEntry().Location.Name : null).Count<XUiC_SavegamesList.ListEntry>();
		if (num > 0)
		{
			this.deleteSaveText.Text = string.Format(Localization.Get("xuiWorldDeleteConfirmation", false), num);
		}
		else
		{
			this.deleteSaveText.Text = Localization.Get("xuiWorldDeleteConfirmationNoSaves", false);
		}
		base.xui.playerUI.CursorController.SetNavigationLockView(this.deleteSavePanel, this.deleteSavePanel.Controller.GetChildById("btnCancel").ViewComponent);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDeleteSave_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.deleteSaveGetConfirmation();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void deleteSaveGetConfirmation()
	{
		this.deleteSavePanel.IsVisible = true;
		this.deleteHeaderText.Text = Localization.Get("xuiDeleteSaveGame", false);
		this.deleteSaveText.Text = Localization.Get("xuiDeleteSaveGame", false);
		base.xui.playerUI.CursorController.SetNavigationLockView(this.deleteSavePanel, this.deleteSavePanel.Controller.GetChildById("btnCancel").ViewComponent);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCancelDelete_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.deleteSavePanel.IsVisible = false;
		base.xui.playerUI.CursorController.SetNavigationLockView(null, null);
		if (this.savesList.SelectedEntry != null)
		{
			base.GetChildById("btnDeleteSave").SelectCursorElement(false, false);
			return;
		}
		base.GetChildById("btnDeleteWorld").SelectCursorElement(false, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnConfirmDelete_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.deleteSavePanel.IsVisible = false;
		base.xui.playerUI.CursorController.SetNavigationLockView(null, null);
		XUiC_ListEntry<XUiC_SavegamesList.ListEntry> selectedEntry = this.savesList.SelectedEntry;
		XUiC_SavegamesList.ListEntry listEntry = (selectedEntry != null) ? selectedEntry.GetEntry() : null;
		if (listEntry != null)
		{
			string saveGameDir = GameIO.GetSaveGameDir(listEntry.worldName, listEntry.saveName);
			if (SdDirectory.Exists(saveGameDir))
			{
				SdDirectory.Delete(saveGameDir, true);
			}
			this.savesList.RebuildList(false);
			base.GetChildById("btnDeleteSave").SelectCursorElement(false, false);
			return;
		}
		XUiC_WorldList.WorldListEntry entry = this.worldList.SelectedEntry.GetEntry();
		if (!entry.GeneratedWorld)
		{
			Log.Warning("Tried to delete non-generated world");
			return;
		}
		try
		{
			GameUtils.DeleteWorld(entry.Location);
		}
		catch (Exception ex)
		{
			Log.Error("Error occurred while deleting world: \"" + ex.Message + "\"");
		}
		this.worldList.RebuildList(false);
		this.worldList.SelectedEntryIndex = 0;
		base.GetChildById("btnDeleteWorld").SelectCursorElement(false, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnStart_OnPressed(XUiController _sender, int _mouseButton)
	{
		new GameModeEditWorld().ResetGamePrefs();
		if (this.toggleNewWorld.Value)
		{
			this.startWithNewWorld();
			return;
		}
		this.startWithExistingWorld();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void startWithNewWorld()
	{
		string text = this.txtNameInput.Text.Trim();
		int newWorldSize = this.NewWorldSize;
		GamePrefs.Set(EnumGamePrefs.GameWorld, text);
		GamePrefs.Set(EnumGamePrefs.CreateLevelName, text);
		GamePrefs.Set(EnumGamePrefs.CreateLevelDim, newWorldSize.ToString());
		MicroStopwatch microStopwatch = new MicroStopwatch(true);
		MicroStopwatch microStopwatch2 = new MicroStopwatch(true);
		WorldStaticData.Cleanup(null);
		Log.Out(string.Format("WSD.Cleanup took {0} ms", microStopwatch2.ElapsedMilliseconds));
		microStopwatch2.ResetAndRestart();
		WorldStaticData.Reset(null);
		Log.Out(string.Format("WSD.Reset took {0} ms", microStopwatch2.ElapsedMilliseconds));
		microStopwatch2.ResetAndRestart();
		GameUtils.CreateEmptyFlatLevel(text, newWorldSize, 60);
		Log.Out(string.Format("Creating empty world took {0} ms", microStopwatch.ElapsedMilliseconds));
		this.startEditor();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void startWithExistingWorld()
	{
		PathAbstractions.AbstractedLocation selectedWorld = this.worldList.SelectedEntry.GetEntry().Location;
		GamePrefs.Set(EnumGamePrefs.GameWorld, selectedWorld.Name);
		if (XUiC_CreateWorld.CanSaveWorldIn(selectedWorld))
		{
			this.startEditor();
			return;
		}
		string text = selectedWorld.Name + "_UserCopy";
		PathAbstractions.AbstractedLocation newLocation = XUiC_CreateWorld.LocationForNewWorld(text);
		int num = 0;
		while (newLocation.Exists())
		{
			string name = selectedWorld.Name;
			string str = "_UserCopy";
			int num2;
			num = (num2 = num + 1);
			text = name + str + num2.ToString();
			newLocation = XUiC_CreateWorld.LocationForNewWorld(text);
		}
		GamePrefs.Set(EnumGamePrefs.GameWorld, text);
		XUiC_MessageBoxWindowGroup.ShowMessageBox(base.xui, Localization.Get("xuiCreateWorldCanNotEditWorldInGameFolderTitle", false), string.Format(Localization.Get("xuiCreateWorldCanNotEditWorldInGameFolderText", false), GameIO.GetOsStylePath(newLocation.FullPath)), XUiC_MessageBoxWindowGroup.MessageBoxTypes.OkCancel, delegate()
		{
			Log.Out(string.Concat(new string[]
			{
				"Will copy the world from '",
				selectedWorld.FullPath,
				"' to '",
				newLocation.FullPath,
				"' for editing."
			}));
			GameIO.CopyDirectory(selectedWorld.FullPath, newLocation.FullPath);
			this.startEditor();
		}, delegate()
		{
			this.xui.playerUI.windowManager.Open(this.windowGroup.ID, true, false, true);
		}, false, true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void startEditor()
	{
		GamePrefs.Set(EnumGamePrefs.GameMode, GameModeEditWorld.TypeName);
		GamePrefs.Set(EnumGamePrefs.GameName, "WorldEditor");
		base.xui.playerUI.windowManager.Close(this.windowGroup.ID);
		NetworkConnectionError networkConnectionError = SingletonMonoBehaviour<ConnectionManager>.Instance.StartServers(GamePrefs.GetString(EnumGamePrefs.ServerPassword), false);
		if (networkConnectionError != NetworkConnectionError.NoError)
		{
			(((XUiWindowGroup)base.xui.playerUI.windowManager.GetWindow(XUiC_MessageBoxWindowGroup.ID)).Controller as XUiC_MessageBoxWindowGroup).ShowNetworkError(networkConnectionError);
		}
	}

	public static PathAbstractions.AbstractedLocation LocationForNewWorld(string _name)
	{
		return new PathAbstractions.AbstractedLocation(PathAbstractions.EAbstractedLocationType.UserDataPath, _name, Path.Combine(GameIO.GetUserGameDataDir(), "GeneratedWorlds"), null, _name, null, true, null);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool CanSaveWorldIn(PathAbstractions.AbstractedLocation _location)
	{
		return _location.Type != PathAbstractions.EAbstractedLocationType.GameData;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshInfoLabels()
	{
		XUiC_ListEntry<XUiC_WorldList.WorldListEntry> selectedEntry = this.worldList.SelectedEntry;
		XUiC_WorldList.WorldListEntry worldListEntry = (selectedEntry != null) ? selectedEntry.GetEntry() : null;
		XUiC_ListEntry<XUiC_SavegamesList.ListEntry> selectedEntry2 = this.savesList.SelectedEntry;
		XUiC_SavegamesList.ListEntry listEntry = (selectedEntry2 != null) ? selectedEntry2.GetEntry() : null;
		this.worldLastPlayedLabel.Text = "--";
		this.worldVersionLabel.Text = "--";
		this.worldMemoryLabel.Text = "--";
		this.saveLastPlayedLabel.Text = "--";
		this.saveVersionLabel.Text = "--";
		this.saveMemoryLabel.Text = "--";
		this.saveVersionValid = true;
		this.worldVersionValid = true;
		if (worldListEntry == null)
		{
			return;
		}
		if (worldListEntry.GeneratedWorld)
		{
			if (1 != worldListEntry.Version.Major)
			{
				this.worldVersionValid = false;
			}
			this.worldVersionLabel.Text = worldListEntry.Version.LongStringNoBuild;
		}
		else
		{
			this.worldVersionLabel.Text = VersionInformation.EGameReleaseType.V.ToString() + " " + 1.ToString();
		}
		this.worldMemoryLabel.Text = this.FormatMemoryString(this.GetWorldMemory(worldListEntry));
		IEnumerator<XUiC_SavegamesList.ListEntry> enumerator = this.savesList.GetSavesInWorld(worldListEntry.Location.Name).GetEnumerator();
		enumerator.MoveNext();
		if (enumerator.Current != null)
		{
			this.worldLastPlayedLabel.Text = (enumerator.Current.lastSaved.ToString() ?? "");
		}
		if (listEntry == null)
		{
			return;
		}
		this.saveLastPlayedLabel.Text = (listEntry.lastSaved.ToString() ?? "");
		VersionInformation gameVersion = listEntry.worldState.gameVersion;
		this.saveVersionLabel.Text = gameVersion.LongStringNoBuild;
		if (gameVersion.Major != 1)
		{
			this.saveVersionValid = false;
		}
		this.saveMemoryLabel.Text = this.FormatMemoryString(this.saveSizeCache[new XUiC_CreateWorld.WorldSavePair
		{
			worldName = listEntry.worldName,
			saveName = listEntry.saveName
		}]);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string FormatMemoryString(long bytes)
	{
		return (bytes / 1024L / 1024L).ToString() + " MB";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshButtonStates()
	{
		int selectedEntryIndex = this.worldList.SelectedEntryIndex;
		int selectedEntryIndex2 = this.savesList.SelectedEntryIndex;
		if (this.toggleSelectWorld.Value)
		{
			this.startable = (selectedEntryIndex >= 0 && selectedEntryIndex2 < 0);
		}
		else
		{
			this.startable = (this.CustomNameValid && (!this.IsCustomSize || this.CustomSizeValid));
		}
		this.worlddeletable = (selectedEntryIndex >= 0 && this.worldList.SelectedEntry.GetEntry().GeneratedWorld && this.toggleSelectWorld.Value);
		this.savedeletable = (selectedEntryIndex2 >= 0);
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public long GetWorldMemory(XUiC_WorldList.WorldListEntry _worldEntry)
	{
		long num = 0L;
		string name = _worldEntry.Location.Name;
		if (!this.worldDataSizeCache.ContainsKey(name))
		{
			this.worldDataSizeCache.Add(name, GameIO.GetDirectorySize(_worldEntry.Location.FullPath, true));
		}
		num += this.worldDataSizeCache[name];
		foreach (XUiC_SavegamesList.ListEntry listEntry in this.savesList.GetSavesInWorld(name))
		{
			XUiC_CreateWorld.WorldSavePair key = new XUiC_CreateWorld.WorldSavePair
			{
				worldName = name,
				saveName = listEntry.saveName
			};
			if (!this.saveSizeCache.ContainsKey(key))
			{
				this.saveSizeCache.Add(key, GameIO.GetDirectorySize(GameIO.GetSaveGameDir(listEntry.worldName, listEntry.saveName), true));
			}
			num += this.saveSizeCache[key];
		}
		return num;
	}

	public override bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
		if (_name == "default_font_color")
		{
			this.defaultFontColor = _value;
			return true;
		}
		if (!(_name == "invalid_font_color"))
		{
			return base.ParseAttribute(_name, _value, _parent);
		}
		this.invalidFontColor = _value;
		return true;
	}

	public override bool GetBindingValue(ref string _value, string _bindingName)
	{
		uint num = <PrivateImplementationDetails>.ComputeStringHash(_bindingName);
		if (num <= 2266769916U)
		{
			if (num <= 485507968U)
			{
				if (num != 184981848U)
				{
					if (num == 485507968U)
					{
						if (_bindingName == "worldversioncolor")
						{
							_value = (this.worldVersionValid ? this.defaultFontColor : this.invalidFontColor);
							return true;
						}
					}
				}
				else if (_bindingName == "false")
				{
					_value = "false";
					return true;
				}
			}
			else if (num != 594028389U)
			{
				if (num != 905824105U)
				{
					if (num == 2266769916U)
					{
						if (_bindingName == "savedeletable")
						{
							_value = this.savedeletable.ToString();
							return true;
						}
					}
				}
				else if (_bindingName == "customsizewarning")
				{
					_value = ((this.IsCustomSize && !this.CustomSizeValid) ? string.Format(Localization.Get("xuiCreateWorldSizeInvalidTooltip", false), 1024, 1024) : "");
					return true;
				}
			}
			else if (_bindingName == "worlddeletable")
			{
				_value = this.worlddeletable.ToString();
				return true;
			}
		}
		else if (num <= 2897913312U)
		{
			if (num != 2345414078U)
			{
				if (num != 2607538187U)
				{
					if (num == 2897913312U)
					{
						if (_bindingName == "customnamecolor")
						{
							_value = (this.CustomNameValid ? this.defaultFontColor : this.invalidFontColor);
							return true;
						}
					}
				}
				else if (_bindingName == "startable")
				{
					_value = this.startable.ToString();
					return true;
				}
			}
			else if (_bindingName == "customsizecolor")
			{
				_value = (this.CustomSizeValid ? this.defaultFontColor : this.invalidFontColor);
				return true;
			}
		}
		else if (num != 3033975131U)
		{
			if (num != 3649963479U)
			{
				if (num == 3817766367U)
				{
					if (_bindingName == "iscustomsize")
					{
						_value = this.IsCustomSize.ToString();
						return true;
					}
				}
			}
			else if (_bindingName == "customnamewarning")
			{
				_value = ((!this.CustomNameValid) ? Localization.Get("xuiCreateWorldNameInvalidTooltip", false) : "");
				return true;
			}
		}
		else if (_bindingName == "saveversioncolor")
		{
			_value = (this.saveVersionValid ? this.defaultFontColor : this.invalidFontColor);
			return true;
		}
		return base.GetBindingValue(ref _value, _bindingName);
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public const int MinWorldSize = 1024;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ToggleButton toggleSelectWorld;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_WorldList worldList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SavegamesList savesList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ToggleButton toggleNewWorld;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtNameInput;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<int> cmbSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Panel deleteSavePanel;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label deleteSaveText;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label deleteHeaderText;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label worldLastPlayedLabel;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label worldVersionLabel;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label worldMemoryLabel;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label saveLastPlayedLabel;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label saveVersionLabel;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label saveMemoryLabel;

	[PublicizedFrom(EAccessModifier.Private)]
	public string invalidFontColor = "255,0,0";

	[PublicizedFrom(EAccessModifier.Private)]
	public string defaultFontColor = "255,255,255";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool startable;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool worlddeletable;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool savedeletable;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool saveVersionValid;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool worldVersionValid;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, long> worldDataSizeCache = new Dictionary<string, long>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<XUiC_CreateWorld.WorldSavePair, long> saveSizeCache = new Dictionary<XUiC_CreateWorld.WorldSavePair, long>();

	[PublicizedFrom(EAccessModifier.Private)]
	public struct WorldSavePair
	{
		public string worldName;

		public string saveName;
	}
}

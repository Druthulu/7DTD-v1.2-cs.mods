using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_NewContinueGame : XUiController
{
	public static void SetIsContinueGame(XUi _xuiInstance, bool _continueGame)
	{
		_xuiInstance.FindWindowGroupByName(XUiC_NewContinueGame.ID).GetChildByType<XUiC_NewContinueGame>().isContinueGame = _continueGame;
	}

	public override void Init()
	{
		base.Init();
		XUiC_NewContinueGame.ID = base.WindowGroup.ID;
		this.cbxGameMode = (XUiC_ComboBoxList<GameMode>)base.GetChildById("cbxGameMode");
		this.cbxGameMode.OnValueChanged += this.CbxGameMode_OnValueChanged;
		this.cbxWorldName = (XUiC_ComboBoxList<XUiC_NewContinueGame.LevelInfo>)base.GetChildById("cbxWorldName");
		this.cbxWorldName.OnValueChanged += this.CbxWorldName_OnValueChanged;
		this.txtGameName = (XUiC_TextInput)base.GetChildById("txtGameName");
		this.txtGameName.OnChangeHandler += this.TxtGameName_OnChangeHandler;
		this.txtGameName.UIInput.onValidate = new UIInput.OnValidate(GameUtils.ValidateGameNameInput);
		this.txtGameNameView = this.txtGameName.ViewComponent;
		this.cbxSaveDataLimit = (XUiC_ComboBoxEnum<SaveDataLimitType>)base.GetChildById("cbxSaveDataLimit");
		SaveDataLimitUIHelper.AddComboBox(this.cbxSaveDataLimit);
		this.worldGenerationControls = base.GetChildByType<XUiC_WorldGenerationWindowGroup>();
		this.worldGenerationControls.OnCountyNameChanged += this.ValidateStartable;
		this.savesList = (XUiC_SavegamesList)base.GetChildById("saves");
		this.savesList.SelectionChanged += this.SavesList_OnSelectionChanged;
		this.savesList.OnEntryDoubleClicked += this.SavesList_OnEntryDoubleClicked;
		this.btnDeleteSave = (XUiC_SimpleButton)base.GetChildById("btnDeleteSave");
		this.btnDeleteSave.OnPressed += this.BtnDeleteSave_OnPressed;
		this.btnDeleteSave.Button.Controller.OnHover += this.BtnDeleteSave_OnHover;
		this.btnDeleteSave.Enabled = false;
		this.deleteSavePanel = (XUiV_Panel)base.GetChildById("deleteSavePanel").ViewComponent;
		this.deleteSaveText = (XUiV_Label)this.deleteSavePanel.Controller.GetChildById("deleteText").ViewComponent;
		XUiC_SimpleButton xuiC_SimpleButton = (XUiC_SimpleButton)this.deleteSavePanel.Controller.GetChildById("btnCancel");
		XUiC_SimpleButton xuiC_SimpleButton2 = (XUiC_SimpleButton)this.deleteSavePanel.Controller.GetChildById("btnConfirm");
		xuiC_SimpleButton.OnPressed += this.BtnCancelDelete_OnPressed;
		xuiC_SimpleButton2.OnPressed += this.BtnConfirmDelete_OnPressed;
		XUiView viewComponent = xuiC_SimpleButton.GetChildById("clickable").ViewComponent;
		XUiView viewComponent2 = xuiC_SimpleButton2.GetChildById("clickable").ViewComponent;
		((XUiC_SimpleButton)base.GetChildById("btnDataManagement")).OnPressed += this.BtnDataManagement_OnPressed;
		this.windowheader = (XUiV_Label)base.GetChildById("windowheader").ViewComponent;
		((XUiC_SimpleButton)base.GetChildById("btnBack")).OnPressed += this.BtnBack_OnPressed;
		((XUiC_SimpleButton)base.GetChildById("btnDefaults")).OnPressed += this.BtnDefaults_OnPressed;
		this.btnStart = (XUiC_SimpleButton)base.GetChildById("btnStart");
		this.btnStart.OnPressed += this.BtnStart_OnPressed;
		this.btnStart.Enabled = false;
		this.RefreshStartLabel();
		this.tabsSelector = (XUiC_TabSelector)base.GetChildById("tabs");
		((XUiC_SimpleButton)base.GetChildById("btnGenerateWorld")).OnPressed += this.BtnRwgPreviewerOnOnPressed;
		XUiC_GamePrefSelector[] childrenByType = base.GetChildrenByType<XUiC_GamePrefSelector>(null);
		int i = 0;
		while (i < childrenByType.Length)
		{
			XUiC_GamePrefSelector xuiC_GamePrefSelector = childrenByType[i];
			EnumGamePrefs gamePref = xuiC_GamePrefSelector.GamePref;
			if (gamePref <= EnumGamePrefs.ServerEACPeerToPeer)
			{
				if (gamePref == EnumGamePrefs.ServerPort)
				{
					xuiC_GamePrefSelector.ControlText.UIInput.validation = UIInput.Validation.Integer;
					xuiC_GamePrefSelector.ControlText.OnChangeHandler += this.TxtPort_OnChangeHandler;
					goto IL_44B;
				}
				if (gamePref != EnumGamePrefs.ServerEACPeerToPeer)
				{
					goto IL_44B;
				}
				XUiC_GamePrefSelector xuiC_GamePrefSelector2 = xuiC_GamePrefSelector;
				xuiC_GamePrefSelector2.OnValueChanged = (Action<XUiC_GamePrefSelector, EnumGamePrefs>)Delegate.Combine(xuiC_GamePrefSelector2.OnValueChanged, new Action<XUiC_GamePrefSelector, EnumGamePrefs>(delegate(XUiC_GamePrefSelector _, EnumGamePrefs gamePrefs)
				{
					this.RefreshMultiplayerOptions(GamePrefs.GetBool(EnumGamePrefs.ServerEnabled));
				}));
				goto IL_44B;
			}
			else
			{
				if (gamePref == EnumGamePrefs.BloodMoonFrequency)
				{
					XUiC_GamePrefSelector xuiC_GamePrefSelector3 = xuiC_GamePrefSelector;
					xuiC_GamePrefSelector3.OnValueChanged = (Action<XUiC_GamePrefSelector, EnumGamePrefs>)Delegate.Combine(xuiC_GamePrefSelector3.OnValueChanged, new Action<XUiC_GamePrefSelector, EnumGamePrefs>(this.CmbBloodMoonFrequency_OnChangeHandler));
					goto IL_44B;
				}
				if (gamePref == EnumGamePrefs.AirDropFrequency)
				{
					xuiC_GamePrefSelector.ValuePreDisplayModifierFunc = ((int _n) => _n / 24);
					goto IL_44B;
				}
				if (gamePref != EnumGamePrefs.ServerEnabled)
				{
					goto IL_44B;
				}
				this.serverEnabledSelector = xuiC_GamePrefSelector;
			}
			IL_459:
			i++;
			continue;
			IL_44B:
			this.gameOptions.Add(gamePref, xuiC_GamePrefSelector);
			goto IL_459;
		}
		this.dataManagementBar = (base.GetChildById("data_bar_controller") as XUiC_DataManagementBar);
		this.dataManagementBarEnabled = (this.dataManagementBar != null && SaveInfoProvider.DataLimitEnabled);
		base.RegisterForInputStyleChanges();
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshStartLabel()
	{
		InControlExtensions.SetApplyButtonString(this.btnStart, "xuiStart");
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void InputStyleChanged(PlayerInputManager.InputStyle _oldStyle, PlayerInputManager.InputStyle _newStyle)
	{
		base.InputStyleChanged(_oldStyle, _newStyle);
		this.RefreshStartLabel();
	}

	public override void OnOpen()
	{
		this.IsDirty = true;
		if (this.dataManagementBarEnabled)
		{
			SaveInfoProvider.Instance.SetDirty();
		}
		XUiC_GamePrefSelector xuiC_GamePrefSelector = this.serverEnabledSelector;
		xuiC_GamePrefSelector.OnValueChanged = (Action<XUiC_GamePrefSelector, EnumGamePrefs>)Delegate.Remove(xuiC_GamePrefSelector.OnValueChanged, new Action<XUiC_GamePrefSelector, EnumGamePrefs>(this.CmbServerEnabled_OnChangeHandler));
		this.windowGroup.openWindowOnEsc = XUiC_MainMenu.ID;
		this.cbxGameMode.Elements.Clear();
		this.createWorldList();
		foreach (GameMode gameMode in GameMode.AvailGameModes)
		{
			if (this.worldsPerMode.ContainsKey((EnumGameMode)gameMode.GetID()))
			{
				this.cbxGameMode.Elements.Add(gameMode);
			}
		}
		if (!this.isContinueGame)
		{
			string modeName = GamePrefs.GetString(EnumGamePrefs.GameMode);
			int num = this.cbxGameMode.Elements.FindIndex((GameMode _mode) => _mode.GetTypeName().EqualsCaseInsensitive(modeName));
			if (num < 0)
			{
				num = 0;
			}
			this.cbxGameMode.SelectedIndex = num;
			this.GameModeChanged(this.cbxGameMode.Value);
			GamePrefs.Instance.Load(GameIO.GetSaveGameRootDir() + "/newGameOptions.sdf");
			this.updateGameOptionValues();
			if (GamePrefs.GetInt(EnumGamePrefs.MaxChunkAge) == 0)
			{
				GamePrefs.Set(EnumGamePrefs.MaxChunkAge, (int)GamePrefs.GetDefault(EnumGamePrefs.MaxChunkAge));
			}
			if (GamePrefs.GetString(EnumGamePrefs.GameName).Trim().Length < 1)
			{
				GamePrefs.Set(EnumGamePrefs.GameName, "MyGame");
			}
			this.txtGameName.Text = GamePrefs.GetString(EnumGamePrefs.GameName).Trim();
			this.ValidateStartable();
		}
		this.windowheader.Text = (this.isContinueGame ? Localization.Get("xuiContinueGame", false) : Localization.Get("xuiNewGame", false));
		base.OnOpen();
		this.deleteSavePanel.IsVisible = false;
		this.tabsSelector.Enabled = true;
		if (GamePrefs.GetString(EnumGamePrefs.GameMode) == "GameMode" + EnumGameMode.Survival.ToStringCached<EnumGameMode>() && GameModeSurvival.OverrideMaxPlayerCount >= 2)
		{
			List<string> list = new List<string>();
			for (int j = 2; j <= GameModeSurvival.OverrideMaxPlayerCount; j++)
			{
				list.Add(j.ToString());
			}
			this.gameOptions[EnumGamePrefs.ServerMaxPlayerCount].OverrideValues(list);
		}
		if (this.isContinueGame && this.savesList.EntryCount > 0)
		{
			this.savesList.SelectedEntryIndex = 0;
			this.savesList.SelectedEntry.SelectCursorElement(true, false);
		}
		else if (this.isContinueGame)
		{
			this.tabsSelector.Enabled = false;
			string text = GamePrefs.GetString(EnumGamePrefs.GameMode);
			if (string.IsNullOrEmpty(text))
			{
				text = (string)GamePrefs.GetDefault(EnumGamePrefs.GameMode);
			}
			GameMode gameModeForName = GameMode.GetGameModeForName(text);
			if (gameModeForName == null)
			{
				text = (string)GamePrefs.GetDefault(EnumGamePrefs.GameMode);
				gameModeForName = GameMode.GetGameModeForName(text);
			}
			this.GameModeChanged(gameModeForName);
			this.ValidateStartable();
			base.GetChildById("btnBack").SelectCursorElement(true, false);
			this.UpdateBarSelectionValues();
		}
		else if (!this.isContinueGame)
		{
			this.updateGameOptionVisibilityForMode(this.cbxGameMode.Value);
			base.GetChildById("txtGameName").SelectCursorElement(true, false);
		}
		bool flag = GamePrefs.GetBool(EnumGamePrefs.ServerEnabled) && PermissionsManager.IsMultiplayerAllowed() && PermissionsManager.CanHostMultiplayer();
		GamePrefs.Set(EnumGamePrefs.ServerEnabled, flag);
		this.serverEnabledSelector.SetCurrentValue();
		if (PlatformManager.MultiPlatform.User.UserStatus == EUserStatus.OfflineMode)
		{
			this.serverEnabledSelector.Enabled = false;
		}
		else
		{
			XUiC_GamePrefSelector xuiC_GamePrefSelector2 = this.serverEnabledSelector;
			xuiC_GamePrefSelector2.OnValueChanged = (Action<XUiC_GamePrefSelector, EnumGamePrefs>)Delegate.Combine(xuiC_GamePrefSelector2.OnValueChanged, new Action<XUiC_GamePrefSelector, EnumGamePrefs>(this.CmbServerEnabled_OnChangeHandler));
		}
		this.RefreshMultiplayerOptions(flag);
		if ((DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent())
		{
			this.gameOptions[EnumGamePrefs.BuildCreate].Enabled = !this.isContinueGame;
		}
		if (this.dataManagementBarEnabled)
		{
			this.dataManagementBar.SetDisplayMode(this.isContinueGame ? XUiC_DataManagementBar.DisplayMode.Selection : XUiC_DataManagementBar.DisplayMode.Preview);
			this.dataManagementBar.SetSelectionDepth(XUiC_DataManagementBar.SelectionDepth.Primary);
			this.UpdateBarUsageAndAllowanceValues();
			if (!this.isContinueGame)
			{
				this.UpdateBarPendingValue();
			}
			SaveDataLimitUIHelper.OnValueChanged = (Action)Delegate.Combine(SaveDataLimitUIHelper.OnValueChanged, new Action(this.UpdateBarPendingValue));
			this.worldGenerationControls.OnWorldSizeChanged += this.UpdateBarPendingValue;
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		XUiC_MultiplayerPrivilegeNotification.Close();
		SaveDataLimitUIHelper.OnValueChanged = (Action)Delegate.Remove(SaveDataLimitUIHelper.OnValueChanged, new Action(this.UpdateBarPendingValue));
		this.worldGenerationControls.OnWorldSizeChanged -= this.UpdateBarPendingValue;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SelectWorld(string worldName)
	{
		this.cbxWorldName.SelectedIndex = Mathf.Max(this.cbxWorldName.Elements.FindIndex((XUiC_NewContinueGame.LevelInfo _info) => _info.RealName.EqualsCaseInsensitive(worldName)), 0);
		GamePrefs.Set(EnumGamePrefs.GameWorld, this.cbxWorldName.Value.RealName);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnRwgPreviewerOnOnPressed(XUiController _sender, int _mouseButton)
	{
		base.xui.FindWindowGroupByName("rwgeditor").GetChildByType<XUiC_WorldGenerationWindowGroup>().LastWindowID = XUiC_NewContinueGame.ID;
		base.xui.playerUI.windowManager.Open("rwgeditor", true, false, true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxGameMode_OnValueChanged(XUiController _sender, GameMode _oldValue, GameMode _newValue)
	{
		this.GameModeChanged(_newValue);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxWorldName_OnValueChanged(XUiController _sender, XUiC_NewContinueGame.LevelInfo _oldValue, XUiC_NewContinueGame.LevelInfo _newValue)
	{
		this.IsDirty = true;
		GamePrefs.Set(EnumGamePrefs.GameWorld, _newValue.RealName);
		this.UpdateBarPendingValue();
		this.ValidateStartable();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TxtGameName_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		GamePrefs.Set(EnumGamePrefs.GameName, _text);
		this.ValidateStartable();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void createWorldList()
	{
		this.worldsPerMode.Clear();
		foreach (PathAbstractions.AbstractedLocation abstractedLocation in PathAbstractions.WorldsSearchPaths.GetAvailablePathsList(null, null, null, false))
		{
			XUiC_NewContinueGame.LevelInfo levelInfo = new XUiC_NewContinueGame.LevelInfo
			{
				RealName = abstractedLocation.Name
			};
			GameUtils.WorldInfo worldInfo = GameUtils.WorldInfo.LoadWorldInfo(abstractedLocation);
			if (worldInfo == null)
			{
				levelInfo.Description = "No Description Found";
				if (!this.worldsPerMode.ContainsKey(EnumGameMode.Creative))
				{
					this.worldsPerMode.Add(EnumGameMode.Creative, new List<XUiC_NewContinueGame.LevelInfo>());
				}
				this.worldsPerMode[EnumGameMode.Creative].Add(levelInfo);
			}
			else
			{
				levelInfo.CustName = worldInfo.Name;
				levelInfo.Description = worldInfo.Description;
				levelInfo.WorldInfo = worldInfo;
				string[] modes = worldInfo.Modes;
				if (modes != null)
				{
					foreach (string text in modes)
					{
						EnumGameMode key;
						if (EnumUtils.TryParse<EnumGameMode>(text, out key, true))
						{
							if (!this.worldsPerMode.ContainsKey(key))
							{
								this.worldsPerMode.Add(key, new List<XUiC_NewContinueGame.LevelInfo>());
							}
							this.worldsPerMode[key].Add(levelInfo);
						}
						else
						{
							Log.Warning(string.Concat(new string[]
							{
								"World \"",
								levelInfo.RealName,
								"\" has unknown game mode \"",
								text,
								"\"."
							}));
						}
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateWorlds()
	{
		string worldName = GamePrefs.GetString(EnumGamePrefs.GameWorld);
		EnumGameMode id = (EnumGameMode)this.cbxGameMode.Value.GetID();
		this.cbxWorldName.Elements.Clear();
		if (this.worldsPerMode.ContainsKey(id))
		{
			foreach (XUiC_NewContinueGame.LevelInfo levelInfo in this.worldsPerMode[id])
			{
				if (XUiC_NewContinueGame.<updateWorlds>g__CanAddWorld|34_0(levelInfo.WorldInfo) && (levelInfo.WorldInfo == null || !levelInfo.WorldInfo.RandomGeneratedWorld))
				{
					this.cbxWorldName.Elements.Add(levelInfo);
				}
			}
			string text = Localization.Get("lblNewRandomWorld", false);
			if (text == "")
			{
				text = "New Random World";
			}
			this.cbxWorldName.Elements.Add(new XUiC_NewContinueGame.LevelInfo
			{
				RealName = text,
				CustName = text,
				Description = "Generate New Random World",
				IsNewRwg = true
			});
			foreach (XUiC_NewContinueGame.LevelInfo levelInfo2 in this.worldsPerMode[id])
			{
				if (XUiC_NewContinueGame.<updateWorlds>g__CanAddWorld|34_0(levelInfo2.WorldInfo) && levelInfo2.WorldInfo != null && levelInfo2.WorldInfo.RandomGeneratedWorld)
				{
					this.cbxWorldName.Elements.Add(levelInfo2);
				}
			}
		}
		int num = this.cbxWorldName.Elements.FindIndex((XUiC_NewContinueGame.LevelInfo _info) => _info.RealName.EqualsCaseInsensitive(worldName));
		this.cbxWorldName.SelectedIndex = ((num >= 0) ? num : 0);
		this.CbxWorldName_OnValueChanged(this.cbxWorldName, default(XUiC_NewContinueGame.LevelInfo), this.cbxWorldName.Value);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SavesList_OnEntryDoubleClicked(XUiController _sender, int _mouseButton)
	{
		if (PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard && this.savesList.SelectedEntryIndex >= 0)
		{
			this.BtnStart_OnPressed(_sender, _mouseButton);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SavesList_OnSelectionChanged(XUiC_ListEntry<XUiC_SavegamesList.ListEntry> _previousEntry, XUiC_ListEntry<XUiC_SavegamesList.ListEntry> _newEntry)
	{
		bool flag = _newEntry != null;
		this.btnDeleteSave.Enabled = flag;
		this.tabsSelector.Enabled = flag;
		if (flag)
		{
			foreach (XUiC_GamePrefSelector xuiC_GamePrefSelector in this.gameOptions.Values)
			{
				GamePrefs.SetObject(xuiC_GamePrefSelector.GamePref, GamePrefs.GetDefault(xuiC_GamePrefSelector.GamePref));
			}
			XUiC_SavegamesList.ListEntry entry = _newEntry.GetEntry();
			GamePrefs.Instance.Load(GameIO.GetSaveGameDir(entry.worldName, entry.saveName) + "/gameOptions.sdf");
			this.GameModeChanged(entry.gameMode);
			GamePrefs.Set(EnumGamePrefs.GameName, entry.saveName);
			GamePrefs.Set(EnumGamePrefs.GameWorld, entry.worldName);
			this.serverEnabledSelector.SetCurrentValue();
			this.RefreshMultiplayerOptions(GamePrefs.GetBool(EnumGamePrefs.ServerEnabled));
		}
		this.UpdateBarSelectionValues();
		this.ValidateStartable();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SaveGameOptions()
	{
		List<EnumGamePrefs> list = new List<EnumGamePrefs>(this.gameOptions.Count);
		this.gameOptions.CopyKeysTo(list);
		GamePrefs.Instance.Save(GameIO.GetSaveGameDir() + "/gameOptions.sdf", list);
		if (!this.isContinueGame)
		{
			GamePrefs.Instance.Save(GameIO.GetSaveGameRootDir() + "/newGameOptions.sdf", list);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDeleteSave_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.deleteSaveGetConfirmation();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDeleteSave_OnHover(XUiController _sender, bool _isOver)
	{
		if (this.dataManagementBarEnabled)
		{
			this.dataManagementBar.SetDeleteHovered(_isOver);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void deleteSaveGetConfirmation()
	{
		this.deleteSavePanel.IsVisible = true;
		XUiC_SimpleButton xuiC_SimpleButton = (XUiC_SimpleButton)this.deleteSavePanel.Controller.GetChildById("btnConfirm");
		this.deleteSaveText.Text = string.Format(Localization.Get("xuiSavegameDeleteConfirmation", false), this.savesList.SelectedEntry.GetEntry().saveName);
		base.xui.playerUI.CursorController.SetNavigationLockView(this.deleteSavePanel, this.deleteSavePanel.Controller.GetChildById("btnCancel").ViewComponent);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCancelDelete_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.deleteSavePanel.IsVisible = false;
		base.xui.playerUI.CursorController.SetNavigationLockView(null, null);
		this.btnDeleteSave.SelectCursorElement(false, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnConfirmDelete_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.deleteSavePanel.IsVisible = false;
		base.xui.playerUI.CursorController.SetNavigationLockView(null, null);
		this.btnDeleteSave.SelectCursorElement(false, false);
		XUiC_SavegamesList.ListEntry entry = this.savesList.SelectedEntry.GetEntry();
		string saveGameDir = GameIO.GetSaveGameDir(entry.worldName, entry.saveName);
		if (SdDirectory.Exists(saveGameDir))
		{
			SdDirectory.Delete(saveGameDir, true);
			SaveInfoProvider.Instance.SetDirty();
			this.UpdateBarUsageAndAllowanceValues();
		}
		this.savesList.RebuildList(false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateGameOptionVisibilityForMode(GameMode _gameMode)
	{
		foreach (KeyValuePair<EnumGamePrefs, XUiC_GamePrefSelector> keyValuePair in this.gameOptions)
		{
			keyValuePair.Value.SetCurrentGameMode(_gameMode);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateGameOptionValues()
	{
		foreach (KeyValuePair<EnumGamePrefs, XUiC_GamePrefSelector> keyValuePair in this.gameOptions)
		{
			keyValuePair.Value.SetCurrentValue();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CmbBloodMoonFrequency_OnChangeHandler(XUiC_GamePrefSelector _prefSelector, EnumGamePrefs _gamePref)
	{
		this.gameOptions[EnumGamePrefs.BloodMoonRange].Enabled = (GamePrefs.GetInt(_gamePref) != 0);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CmbServerEnabled_OnChangeHandler(XUiC_GamePrefSelector _prefSelector, EnumGamePrefs _gamePref)
	{
		bool @bool = GamePrefs.GetBool(EnumGamePrefs.ServerEnabled);
		this.RefreshMultiplayerOptions(@bool);
		if (@bool && (!PermissionsManager.IsMultiplayerAllowed() || !PermissionsManager.CanHostMultiplayer()))
		{
			if (this.wdwMultiplayerPrivileges == null)
			{
				this.wdwMultiplayerPrivileges = XUiC_MultiplayerPrivilegeNotification.GetWindow();
			}
			XUiC_MultiplayerPrivilegeNotification xuiC_MultiplayerPrivilegeNotification = this.wdwMultiplayerPrivileges;
			if (xuiC_MultiplayerPrivilegeNotification == null)
			{
				return;
			}
			xuiC_MultiplayerPrivilegeNotification.ResolvePrivilegesWithDialog(EUserPerms.HostMultiplayer, delegate(bool result)
			{
				GamePrefs.Set(EnumGamePrefs.ServerEnabled, result);
				this.serverEnabledSelector.SetCurrentValue();
			}, (EUserPerms)0, -1f, false, null);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshMultiplayerOptions(bool enabled)
	{
		IPlatform crossplatformPlatform = PlatformManager.CrossplatformPlatform;
		bool? flag;
		if (crossplatformPlatform == null)
		{
			flag = null;
		}
		else
		{
			IAntiCheatServer antiCheatServer = crossplatformPlatform.AntiCheatServer;
			flag = ((antiCheatServer != null) ? new bool?(antiCheatServer.ServerEacAvailable()) : null);
		}
		bool? flag2 = flag;
		bool valueOrDefault = flag2.GetValueOrDefault();
		IPlatform crossplatformPlatform2 = PlatformManager.CrossplatformPlatform;
		bool? flag3;
		if (crossplatformPlatform2 == null)
		{
			flag3 = null;
		}
		else
		{
			IAntiCheatServer antiCheatServer2 = crossplatformPlatform2.AntiCheatServer;
			flag3 = ((antiCheatServer2 != null) ? new bool?(antiCheatServer2.ServerEacEnabled()) : null);
		}
		flag2 = flag3;
		bool valueOrDefault2 = flag2.GetValueOrDefault();
		bool flag4 = PermissionsManager.IsCrossplayAllowed() && (valueOrDefault2 || !Submission.Enabled) && enabled;
		bool @bool = GamePrefs.GetBool(EnumGamePrefs.ServerEACPeerToPeer);
		bool bool2 = GamePrefs.GetBool(EnumGamePrefs.ServerAllowCrossplay);
		bool flag5 = (DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX).IsCurrent() || !Submission.Enabled;
		GamePrefs.Set(EnumGamePrefs.ServerEACPeerToPeer, !flag5 || valueOrDefault2);
		this.gameOptions[EnumGamePrefs.ServerEACPeerToPeer].ViewComponent.IsVisible = flag5;
		this.gameOptions[EnumGamePrefs.ServerEACPeerToPeer].Enabled = valueOrDefault;
		if (@bool != GamePrefs.GetBool(EnumGamePrefs.ServerEACPeerToPeer))
		{
			this.gameOptions[EnumGamePrefs.ServerEACPeerToPeer].SetCurrentValue();
		}
		this.gameOptions[EnumGamePrefs.ServerEACPeerToPeer].CheckDefaultValue();
		if (enabled)
		{
			GamePrefs.Set(EnumGamePrefs.ServerAllowCrossplay, bool2 && flag4);
		}
		this.gameOptions[EnumGamePrefs.ServerAllowCrossplay].Enabled = flag4;
		this.gameOptions[EnumGamePrefs.ServerAllowCrossplay].SetCurrentValue();
		this.gameOptions[EnumGamePrefs.ServerAllowCrossplay].CheckDefaultValue();
		this.gameOptions[EnumGamePrefs.Region].Enabled = enabled;
		this.gameOptions[EnumGamePrefs.ServerVisibility].Enabled = enabled;
		this.gameOptions[EnumGamePrefs.ServerPassword].Enabled = enabled;
		this.gameOptions[EnumGamePrefs.ServerMaxPlayerCount].Enabled = enabled;
		this.gameOptions[EnumGamePrefs.Region].CheckDefaultValue();
		this.gameOptions[EnumGamePrefs.ServerVisibility].CheckDefaultValue();
		this.gameOptions[EnumGamePrefs.ServerPassword].CheckDefaultValue();
		this.gameOptions[EnumGamePrefs.ServerMaxPlayerCount].CheckDefaultValue();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TxtPort_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		XUiC_TextInput xuiC_TextInput = (XUiC_TextInput)_sender;
		if (_text.Length < 1)
		{
			xuiC_TextInput.Text = "0";
		}
		else if (_text.Length > 1 && _text[0] == '0')
		{
			xuiC_TextInput.Text = _text.Substring(1);
		}
		this.ValidateStartable();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnBack_OnPressed(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(this.windowGroup.ID);
		base.xui.playerUI.windowManager.Open(XUiC_MainMenu.ID, true, false, true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDefaults_OnPressed(XUiController _sender, int _mouseButton)
	{
		GamePrefs.Set(EnumGamePrefs.ServerEnabled, (bool)GamePrefs.GetDefault(EnumGamePrefs.ServerEnabled));
		this.serverEnabledSelector.SetCurrentValue();
		this.CmbServerEnabled_OnChangeHandler(this.serverEnabledSelector, EnumGamePrefs.ServerEnabled);
		this.cbxGameMode.Value.ResetGamePrefs();
		this.updateGameOptionValues();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDataManagement_OnPressed(XUiController _sender, int _mouseButton)
	{
		XUiC_DataManagement.OpenDataManagementWindow(this, new Action(this.OnDataManagementWindowClosed));
	}

	public void OnDataManagementWindowClosed()
	{
		this.UpdateBarUsageAndAllowanceValues();
		if (this.isContinueGame)
		{
			if (this.dataManagementBarEnabled)
			{
				this.dataManagementBar.SetDisplayMode(XUiC_DataManagementBar.DisplayMode.Selection);
				this.dataManagementBar.SetSelectedByteRegion(XUiC_DataManagementBar.BarRegion.None);
			}
			this.savesList.RebuildList(false);
		}
		else
		{
			this.createWorldList();
			this.updateWorlds();
			if (this.dataManagementBarEnabled)
			{
				this.dataManagementBar.SetDisplayMode(XUiC_DataManagementBar.DisplayMode.Preview);
				this.UpdateBarPendingValue();
			}
		}
		this.worldGenerationControls.RefreshCountyName();
		this.ValidateGameName();
		this.worldGenerationControls.RefreshBindings(false);
		base.RefreshBindings(false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateBarUsageAndAllowanceValues()
	{
		if (!this.dataManagementBarEnabled)
		{
			return;
		}
		SaveInfoProvider instance = SaveInfoProvider.Instance;
		this.dataManagementBar.SetUsedBytes(instance.TotalUsedBytes);
		this.dataManagementBar.SetAllowanceBytes(instance.TotalAllowanceBytes);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateBarPendingValue()
	{
		if (!this.dataManagementBarEnabled)
		{
			return;
		}
		this.pendingPreviewSize = this.<UpdateBarPendingValue>g__CalculatePendingSaveSize|54_0();
		this.dataManagementBar.SetPendingBytes(this.pendingPreviewSize);
		this.ValidateStartable();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateBarSelectionValues()
	{
		if (!this.dataManagementBarEnabled)
		{
			return;
		}
		XUiC_ListEntry<XUiC_SavegamesList.ListEntry> selectedEntry = this.savesList.SelectedEntry;
		XUiC_SavegamesList.ListEntry listEntry = (selectedEntry != null) ? selectedEntry.GetEntry() : null;
		SaveInfoProvider.SaveEntryInfo saveEntryInfo;
		if (listEntry != null && SaveInfoProvider.Instance.TryGetLocalSaveEntry(listEntry.worldName, listEntry.saveName, out saveEntryInfo))
		{
			XUiC_DataManagementBar.BarRegion selectedByteRegion = new XUiC_DataManagementBar.BarRegion(saveEntryInfo.BarStartOffset, saveEntryInfo.SizeInfo.ReportedSize);
			this.dataManagementBar.SetSelectedByteRegion(selectedByteRegion);
			this.dataManagementBar.SetSelectionDepth(XUiC_DataManagementBar.SelectionDepth.Primary);
			return;
		}
		this.dataManagementBar.SetSelectedByteRegion(XUiC_DataManagementBar.BarRegion.None);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnStart_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (!this.btnStart.Enabled)
		{
			return;
		}
		GameManager.Instance.showOpenerMovieOnLoad = (GamePrefs.GetBool(EnumGamePrefs.OptionsIntroMovieEnabled) && !this.isContinueGame);
		if (!GamePrefs.GetBool(EnumGamePrefs.ServerEnabled))
		{
			ThreadManager.StartCoroutine(this.startGameCo());
			return;
		}
		if (this.wdwMultiplayerPrivileges == null)
		{
			this.wdwMultiplayerPrivileges = XUiC_MultiplayerPrivilegeNotification.GetWindow();
		}
		EUserPerms perms = EUserPerms.HostMultiplayer;
		if (GamePrefs.GetBool(EnumGamePrefs.ServerAllowCrossplay))
		{
			perms |= EUserPerms.Crossplay;
		}
		XUiC_MultiplayerPrivilegeNotification xuiC_MultiplayerPrivilegeNotification = this.wdwMultiplayerPrivileges;
		if (xuiC_MultiplayerPrivilegeNotification == null)
		{
			return;
		}
		xuiC_MultiplayerPrivilegeNotification.ResolvePrivilegesWithDialog(perms, delegate(bool result)
		{
			bool value = PermissionsManager.CanHostMultiplayer();
			GamePrefs.Set(EnumGamePrefs.ServerEnabled, value);
			this.serverEnabledSelector.SetCurrentValue();
			bool value2 = perms.HasCrossplay() && PermissionsManager.IsCrossplayAllowed();
			GamePrefs.Set(EnumGamePrefs.ServerAllowCrossplay, value2);
			this.gameOptions[EnumGamePrefs.ServerAllowCrossplay].SetCurrentValue();
			if (!result)
			{
				return;
			}
			ThreadManager.StartCoroutine(this.startGameCo());
		}, (EUserPerms)0, -1f, false, null);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator startGameCo()
	{
		base.xui.playerUI.windowManager.Close(this.windowGroup.ID);
		if (!this.isContinueGame)
		{
			XUiC_NewContinueGame.<>c__DisplayClass57_0 CS$<>8__locals1 = new XUiC_NewContinueGame.<>c__DisplayClass57_0();
			XUiC_NewContinueGame.LevelInfo value = this.cbxWorldName.Value;
			Vector2i worldSize;
			if (value.IsNewRwg)
			{
				CS$<>8__locals1.worldName = null;
				yield return this.worldGenerationControls.GenerateCo(false, delegate(string name)
				{
					CS$<>8__locals1.worldName = name;
				}, null);
				if (CS$<>8__locals1.worldName == null)
				{
					base.xui.playerUI.windowManager.Open(this.windowGroup.ID, true, false, true);
					yield break;
				}
				GamePrefs.Set(EnumGamePrefs.GameWorld, CS$<>8__locals1.worldName);
				worldSize.x = this.worldGenerationControls.WorldSize;
				worldSize.y = worldSize.x;
			}
			else
			{
				worldSize = value.WorldInfo.WorldSize;
				CS$<>8__locals1.worldName = value.WorldInfo.Name;
			}
			long saveDataSize = SaveDataLimitUIHelper.CurrentValue.CalculateTotalSize(worldSize);
			XUiC_SaveSpaceNeeded confirmationWindow = XUiC_SaveSpaceNeeded.Open(saveDataSize, new string[]
			{
				GameIO.GetWorldDir(CS$<>8__locals1.worldName),
				GameIO.GetSaveGameDir(CS$<>8__locals1.worldName, GamePrefs.GetString(EnumGamePrefs.GameName))
			}, null, true, true, false, null, "xuiDmCreateSave", null, null, "xuiCreate", null);
			while (confirmationWindow.IsOpen)
			{
				yield return null;
			}
			if (confirmationWindow.Result != XUiC_SaveSpaceNeeded.ConfirmationResult.Confirmed)
			{
				base.xui.playerUI.windowManager.Open(this.windowGroup.ID, true, false, true);
				yield break;
			}
			SaveDataLimit.SetLimitToPref(saveDataSize);
			CS$<>8__locals1 = null;
			confirmationWindow = null;
		}
		GamePrefs.SetPersistent(EnumGamePrefs.GameMode, true);
		this.SaveGameOptions();
		if (PlatformOptimizations.RestartAfterRwg)
		{
			yield return PlatformApplicationManager.CheckRestartCoroutine(true);
		}
		bool offline = !GamePrefs.GetBool(EnumGamePrefs.ServerEnabled);
		NetworkConnectionError networkConnectionError = SingletonMonoBehaviour<ConnectionManager>.Instance.StartServers(GamePrefs.GetString(EnumGamePrefs.ServerPassword), offline);
		if (networkConnectionError != NetworkConnectionError.NoError)
		{
			((XUiC_MessageBoxWindowGroup)((XUiWindowGroup)base.xui.playerUI.windowManager.GetWindow(XUiC_MessageBoxWindowGroup.ID)).Controller).ShowNetworkError(networkConnectionError);
		}
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GameModeChanged(GameMode _newMode)
	{
		GamePrefs.Set(EnumGamePrefs.GameMode, _newMode.GetTypeName());
		this.updateWorlds();
		this.updateGameOptionVisibilityForMode(_newMode);
		this.updateGameOptionValues();
		this.ValidateStartable();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ValidateStartable()
	{
		bool flag;
		if (this.isContinueGame)
		{
			flag = (this.savesList.SelectedEntryIndex >= 0);
			if (flag)
			{
				XUiC_SavegamesList.ListEntry entry = this.savesList.SelectedEntry.GetEntry();
				flag = (entry.versionComparison != VersionInformation.EVersionComparisonResult.DifferentMajor);
				if (flag && entry.AbstractedLocation.Type == PathAbstractions.EAbstractedLocationType.None)
				{
					flag = false;
				}
			}
			this.tabsSelector.Enabled = flag;
		}
		else
		{
			flag = this.ValidateGameName();
			if (!this.cbxWorldName.Value.IsNewRwg && this.cbxWorldName.Value.WorldInfo != null)
			{
				GameUtils.WorldInfo worldInfo = this.cbxWorldName.Value.WorldInfo;
				bool flag2 = worldInfo.GameVersionCreated.IsValid && !worldInfo.GameVersionCreated.EqualsMajor(Constants.cVersionInformation);
				flag &= !flag2;
			}
			if (this.dataManagementBarEnabled)
			{
				flag &= (SaveInfoProvider.Instance.TotalAvailableBytes >= this.pendingPreviewSize);
			}
		}
		flag &= this.ValidatePort();
		this.btnStart.Enabled = flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool ValidateGameName()
	{
		bool isNewRwg = this.cbxWorldName.Value.IsNewRwg;
		string text = this.txtGameName.Text;
		string text2 = isNewRwg ? this.worldGenerationControls.CountyNameLabel.Text : this.cbxWorldName.Value.RealName;
		bool flag = GameUtils.ValidateGameName(text);
		bool flag2 = text.Trim().Length > 0;
		bool flag3 = !text.EqualsCaseInsensitive("Region");
		bool flag4 = !SdFile.Exists(GameIO.GetSaveGameDir(text2, text) + "/main.ttw");
		bool flag5 = PathAbstractions.WorldsSearchPaths.GetLocation(text2, text2, text).Type != PathAbstractions.EAbstractedLocationType.None;
		bool validCountyName = this.worldGenerationControls.ValidCountyName;
		bool flag6 = (!isNewRwg) ? flag5 : validCountyName;
		bool flag7 = flag && flag2 && flag3 && flag4 && flag6;
		this.txtGameName.ActiveTextColor = (flag7 ? Color.white : Color.red);
		if (!flag2)
		{
			this.txtGameNameView.ToolTip = Localization.Get("mmLblNameErrorEmpty", false);
		}
		else if (!flag)
		{
			this.txtGameNameView.ToolTip = Localization.Get("mmLblNameErrorInvalid", false);
		}
		else if (!flag3)
		{
			this.txtGameNameView.ToolTip = Localization.Get("mmLblNameErrorDefault", false);
		}
		else if (!flag4)
		{
			this.txtGameNameView.ToolTip = Localization.Get("mmLblNameErrorExists", false);
		}
		else if (!flag5)
		{
			this.txtGameNameView.ToolTip = Localization.Get("mmLblNameErrorNoWorld", false);
		}
		else
		{
			this.txtGameNameView.ToolTip = "";
		}
		return flag7;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool ValidatePort()
	{
		XUiC_TextInput controlText = this.gameOptions[EnumGamePrefs.ServerPort].ControlText;
		int num;
		bool flag = StringParsers.TryParseSInt32(controlText.Text, out num, 0, -1, NumberStyles.Integer) && num >= 1024 && num < 65533;
		if (!flag)
		{
			controlText.ActiveTextColor = Color.red;
		}
		return flag;
	}

	public override void UpdateInput()
	{
		base.UpdateInput();
		if (!base.xui.playerUI.windowManager.IsKeyShortcutsAllowed())
		{
			return;
		}
		if (this.isContinueGame && !this.deleteSavePanel.IsVisible && Input.GetKeyUp(KeyCode.Delete))
		{
			XUiC_ListEntry<XUiC_SavegamesList.ListEntry> selectedEntry = this.savesList.SelectedEntry;
			if (((selectedEntry != null) ? selectedEntry.GetEntry() : null) != null)
			{
				this.deleteSaveGetConfirmation();
			}
		}
		if (base.xui.playerUI.playerInput.GUIActions.Apply.WasPressed)
		{
			this.BtnStart_OnPressed(null, 0);
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.IsDirty)
		{
			base.RefreshBindings(false);
			this.IsDirty = false;
		}
		this.DoLoadSaveGameAutomation();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DoLoadSaveGameAutomation()
	{
		EPlatformLoadSaveGameState loadSaveGameState = PlatformApplicationManager.GetLoadSaveGameState();
		switch (loadSaveGameState)
		{
		case EPlatformLoadSaveGameState.NewGameSelect:
			if (!this.isContinueGame)
			{
				string @string = GamePrefs.GetString(EnumGamePrefs.GameWorld);
				if (!this.cbxWorldName.Value.RealName.EqualsCaseInsensitive(@string))
				{
					this.SelectWorld(@string);
				}
				if (!this.cbxWorldName.Value.RealName.EqualsCaseInsensitive(@string))
				{
					PlatformApplicationManager.SetFailedLoadSaveGame();
					return;
				}
				PlatformApplicationManager.AdvanceLoadSaveGameStateFrom(loadSaveGameState);
				return;
			}
			break;
		case EPlatformLoadSaveGameState.NewGamePlay:
		case EPlatformLoadSaveGameState.ContinueGamePlay:
			if (!this.btnStart.Enabled)
			{
				PlatformApplicationManager.SetFailedLoadSaveGame();
				return;
			}
			this.BtnStart_OnPressed(this.btnStart, -1);
			PlatformApplicationManager.AdvanceLoadSaveGameStateFrom(loadSaveGameState);
			break;
		case EPlatformLoadSaveGameState.ContinueGameOpen:
			break;
		case EPlatformLoadSaveGameState.ContinueGameSelect:
			if (this.isContinueGame)
			{
				string string2 = GamePrefs.GetString(EnumGamePrefs.GameWorld);
				string string3 = GamePrefs.GetString(EnumGamePrefs.GameName);
				XUiC_ListEntry<XUiC_SavegamesList.ListEntry> selectedEntry = this.savesList.SelectedEntry;
				XUiC_SavegamesList.ListEntry listEntry = (selectedEntry != null) ? selectedEntry.GetEntry() : null;
				if (listEntry == null || !listEntry.worldName.EqualsCaseInsensitive(string2) || !listEntry.saveName.EqualsCaseInsensitive(string3))
				{
					this.savesList.SelectEntry(string2, string3);
				}
				XUiC_ListEntry<XUiC_SavegamesList.ListEntry> selectedEntry2 = this.savesList.SelectedEntry;
				listEntry = ((selectedEntry2 != null) ? selectedEntry2.GetEntry() : null);
				if (listEntry == null || !listEntry.worldName.EqualsCaseInsensitive(string2) || !listEntry.saveName.EqualsCaseInsensitive(string3))
				{
					PlatformApplicationManager.SetFailedLoadSaveGame();
					return;
				}
				PlatformApplicationManager.AdvanceLoadSaveGameStateFrom(loadSaveGameState);
				return;
			}
			break;
		default:
			return;
		}
	}

	public override bool GetBindingValue(ref string _value, string _bindingName)
	{
		uint num = <PrivateImplementationDetails>.ComputeStringHash(_bindingName);
		if (num <= 2544934302U)
		{
			if (num <= 870970795U)
			{
				if (num != 184981848U)
				{
					if (num == 870970795U)
					{
						if (_bindingName == "isnotgenerateworld")
						{
							_value = (!this.isContinueGame && !this.cbxWorldName.Value.IsNewRwg).ToString();
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
			else if (num != 1457541641U)
			{
				if (num != 2138785856U)
				{
					if (num == 2544934302U)
					{
						if (_bindingName == "iscontinuegame")
						{
							_value = this.isContinueGame.ToString();
							return true;
						}
					}
				}
				else if (_bindingName == "mapsize")
				{
					if (!this.isContinueGame && this.cbxWorldName.Value.WorldInfo != null)
					{
						GameUtils.WorldInfo worldInfo = this.cbxWorldName.Value.WorldInfo;
						Vector2i worldSize = worldInfo.WorldSize;
						string str = worldSize.x.ToString();
						string str2 = " x ";
						worldSize = worldInfo.WorldSize;
						_value = str + str2 + worldSize.y.ToString();
					}
					else
					{
						_value = "";
					}
					return true;
				}
			}
			else if (_bindingName == "showbar")
			{
				_value = this.dataManagementBarEnabled.ToString();
				return true;
			}
		}
		else if (num <= 3571689340U)
		{
			if (num != 3328507019U)
			{
				if (num == 3571689340U)
				{
					if (_bindingName == "crossplayTooltip")
					{
						string permissionDenyReason = PermissionsManager.GetPermissionDenyReason(EUserPerms.Crossplay, PermissionsManager.PermissionSources.All);
						if (!string.IsNullOrEmpty(permissionDenyReason))
						{
							_value = permissionDenyReason;
						}
						else if ((DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX).IsCurrent())
						{
							this.gameOptions[EnumGamePrefs.ServerEACPeerToPeer].CheckDefaultValue();
							_value = string.Format(Localization.Get("xuiOptionsGeneralCrossplayTooltipPC", false), 8);
						}
						else
						{
							_value = Localization.Get("xuiOptionsGeneralCrossplayTooltip", false);
						}
						return true;
					}
				}
			}
			else if (_bindingName == "worldgameversion")
			{
				if (!this.isContinueGame && this.cbxWorldName.Value.WorldInfo != null)
				{
					GameUtils.WorldInfo worldInfo2 = this.cbxWorldName.Value.WorldInfo;
					_value = (worldInfo2.GameVersionCreated.IsValid ? (worldInfo2.GameVersionCreated.ReleaseType.ToStringCached<VersionInformation.EGameReleaseType>() + " " + worldInfo2.GameVersionCreated.Major.ToString()) : "");
				}
				else
				{
					_value = "false";
				}
				return true;
			}
		}
		else if (num != 3632962507U)
		{
			if (num != 3892156446U)
			{
				if (num == 4282068674U)
				{
					if (_bindingName == "differentworldversion")
					{
						if (!this.isContinueGame && this.cbxWorldName.Value.WorldInfo != null)
						{
							GameUtils.WorldInfo worldInfo3 = this.cbxWorldName.Value.WorldInfo;
							_value = (worldInfo3.GameVersionCreated.IsValid && !worldInfo3.GameVersionCreated.EqualsMajor(Constants.cVersionInformation)).ToString();
						}
						else
						{
							_value = "false";
						}
						return true;
					}
				}
			}
			else if (_bindingName == "isgenerateworld")
			{
				_value = (!this.isContinueGame && this.cbxWorldName.Value.IsNewRwg).ToString();
				return true;
			}
		}
		else if (_bindingName == "isnewgame")
		{
			_value = (!this.isContinueGame).ToString();
			return true;
		}
		return base.GetBindingValue(ref _value, _bindingName);
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Internal)]
	public static bool <updateWorlds>g__CanAddWorld|34_0(GameUtils.WorldInfo _worldInfo)
	{
		if (_worldInfo != null && PlatformOptimizations.EnforceMaxWorldSizeHost)
		{
			Vector2i worldSize = _worldInfo.WorldSize;
			if (worldSize.x > PlatformOptimizations.MaxWorldSizeHost || worldSize.y > PlatformOptimizations.MaxWorldSizeHost)
			{
				return false;
			}
		}
		return true;
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public long <UpdateBarPendingValue>g__CalculatePendingSaveSize|54_0()
	{
		XUiC_NewContinueGame.LevelInfo value = this.cbxWorldName.Value;
		Vector2i worldSize;
		if (value.IsNewRwg)
		{
			worldSize.x = Math.Max(1, this.worldGenerationControls.WorldSize);
			worldSize.y = worldSize.x;
		}
		else
		{
			worldSize = value.WorldInfo.WorldSize;
		}
		if (SaveDataLimitUIHelper.CurrentValue == SaveDataLimitType.Unlimited)
		{
			return Math.Max(SaveDataLimitType.Short.CalculateTotalSize(worldSize), SaveInfoProvider.Instance.TotalAvailableBytes);
		}
		return SaveDataLimitUIHelper.CurrentValue.CalculateTotalSize(worldSize);
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnStart;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label windowheader;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_MultiplayerPrivilegeNotification wdwMultiplayerPrivileges;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<GameMode> cbxGameMode;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<XUiC_NewContinueGame.LevelInfo> cbxWorldName;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtGameName;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiView txtGameNameView;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<EnumGameMode, List<XUiC_NewContinueGame.LevelInfo>> worldsPerMode = new EnumDictionary<EnumGameMode, List<XUiC_NewContinueGame.LevelInfo>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxEnum<SaveDataLimitType> cbxSaveDataLimit;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_WorldGenerationWindowGroup worldGenerationControls;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SavegamesList savesList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnDeleteSave;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Panel deleteSavePanel;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label deleteSaveText;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<EnumGamePrefs, XUiC_GamePrefSelector> gameOptions = new EnumDictionary<EnumGamePrefs, XUiC_GamePrefSelector>();

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TabSelector tabsSelector;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_GamePrefSelector serverEnabledSelector;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isContinueGame = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_DataManagementBar dataManagementBar;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool dataManagementBarEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public long pendingPreviewSize;

	public struct LevelInfo
	{
		public override string ToString()
		{
			return this.RealName;
		}

		public string RealName;

		public string CustName;

		public string Description;

		public GameUtils.WorldInfo WorldInfo;

		public bool IsNewRwg;
	}
}

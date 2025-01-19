using System;
using GUI_2;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_MainMenu : XUiController
{
	public override void Init()
	{
		base.Init();
		XUiC_MainMenu.ID = base.WindowGroup.ID;
		this.btnNewGame = (base.GetChildById("btnNewGame") as XUiC_SimpleButton);
		this.btnContinueGame = (base.GetChildById("btnContinueGame") as XUiC_SimpleButton);
		this.btnConnectToServer = (base.GetChildById("btnConnectToServer") as XUiC_SimpleButton);
		this.btnOptions = (base.GetChildById("btnOptions") as XUiC_SimpleButton);
		this.btnCredits = (base.GetChildById("btnCredits") as XUiC_SimpleButton);
		this.btnNews = (base.GetChildById("btnNews") as XUiC_SimpleButton);
		this.btnQuit = (base.GetChildById("btnQuit") as XUiC_SimpleButton);
		this.btnNewGame.OnPressed += this.btnNewGame_OnPressed;
		this.btnContinueGame.OnPressed += this.btnContinueGame_OnPressed;
		this.btnConnectToServer.OnPressed += this.btnConnectToServer_OnPressed;
		this.btnOptions.OnPressed += this.btnOptions_OnPressed;
		this.btnCredits.OnPressed += this.btnCredits_OnPressed;
		if (this.btnNews != null)
		{
			this.btnNews.OnPressed += this.btnNews_OnPressed;
		}
		if (this.btnQuit != null)
		{
			this.btnQuit.OnPressed += this.btnQuit_OnPressed;
		}
		this.btnEditingTools = (base.GetChildById("btnEditingTools") as XUiC_SimpleButton);
		if (this.btnEditingTools != null)
		{
			this.btnEditingTools.OnPressed += this.btnEditingTools_OnPressed;
		}
		this.btnRWG = (base.GetChildById("btnRWG") as XUiC_SimpleButton);
		if (this.btnRWG != null)
		{
			this.btnRWG.OnPressed += this.btnRWG_OnPressed;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnNewGame_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (GameManager.HasAcceptedLatestEula())
		{
			XUiC_NewContinueGame.SetIsContinueGame(base.xui, false);
			this.CheckProfile(XUiC_NewContinueGame.ID);
			return;
		}
		XUiC_EulaWindow.Open(base.xui, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnContinueGame_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (GameManager.HasAcceptedLatestEula())
		{
			XUiC_NewContinueGame.SetIsContinueGame(base.xui, true);
			this.CheckProfile(XUiC_NewContinueGame.ID);
			return;
		}
		XUiC_EulaWindow.Open(base.xui, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnConnectToServer_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (!GameManager.HasAcceptedLatestEula())
		{
			XUiC_EulaWindow.Open(base.xui, false);
			return;
		}
		if (this.wdwMultiplayerPrivileges == null)
		{
			this.wdwMultiplayerPrivileges = XUiC_MultiplayerPrivilegeNotification.GetWindow();
		}
		XUiC_MultiplayerPrivilegeNotification xuiC_MultiplayerPrivilegeNotification = this.wdwMultiplayerPrivileges;
		if (xuiC_MultiplayerPrivilegeNotification == null)
		{
			return;
		}
		xuiC_MultiplayerPrivilegeNotification.ResolvePrivilegesWithDialog(EUserPerms.Multiplayer, delegate(bool result)
		{
			if (PermissionsManager.IsMultiplayerAllowed())
			{
				this.CheckProfile(XUiC_ServerBrowser.ID);
			}
		}, EUserPerms.Crossplay, -1f, false, null);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnEditingTools_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (GameManager.HasAcceptedLatestEula())
		{
			base.xui.playerUI.windowManager.Close(this.windowGroup.ID);
			base.xui.playerUI.windowManager.Open(XUiC_EditingTools.ID, true, false, true);
			return;
		}
		XUiC_EulaWindow.Open(base.xui, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnRWG_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (!GameManager.HasAcceptedLatestEula())
		{
			XUiC_EulaWindow.Open(base.xui, false);
			return;
		}
		base.xui.FindWindowGroupByName("rwgeditor").GetChildByType<XUiC_WorldGenerationWindowGroup>().LastWindowID = XUiC_MainMenu.ID;
		base.xui.playerUI.windowManager.Open("rwgeditor", true, false, true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnOptions_OnPressed(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(this.windowGroup.ID);
		base.xui.playerUI.windowManager.Open(XUiC_OptionsMenu.ID, true, false, true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnCredits_OnPressed(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(this.windowGroup.ID);
		base.xui.playerUI.windowManager.Open(XUiC_Credits.ID, true, false, true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnNews_OnPressed(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(this.windowGroup.ID);
		XUiC_NewsScreen.Open(base.xui);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnQuit_OnPressed(XUiController _sender, int _mouseButton)
	{
		Application.Quit();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CheckProfile(string _windowToOpen)
	{
		base.xui.playerUI.windowManager.Close(this.windowGroup.ID);
		if (ProfileSDF.CurrentProfileName().Length == 0)
		{
			XUiC_OptionsProfiles.Open(base.xui, delegate
			{
				this.xui.playerUI.windowManager.Open(_windowToOpen, true, false, true);
			});
			return;
		}
		base.xui.playerUI.windowManager.Open(_windowToOpen, true, false, true);
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		this.windowGroup.isEscClosable = false;
		base.RefreshBindings(false);
		bool flag = true;
		bool flag2 = PlatformManager.MultiPlatform.JoinSessionGameInviteListener != null && PlatformManager.MultiPlatform.JoinSessionGameInviteListener.IsProcessingIntent(out flag);
		if (flag)
		{
			ThreadManager.StartCoroutine(PlatformApplicationManager.CheckRestartCoroutine(false));
		}
		if (flag2)
		{
			string text = Localization.Get("lblReceivedGameInvite", false);
			XUiC_ProgressWindow.Open(LocalPlayerUI.primaryUI, text, delegate
			{
				IJoinSessionGameInviteListener joinSessionGameInviteListener = PlatformManager.MultiPlatform.JoinSessionGameInviteListener;
				if (joinSessionGameInviteListener != null)
				{
					joinSessionGameInviteListener.Cancel();
				}
				LocalPlayerUI.primaryUI.windowManager.Open(XUiC_MainMenu.ID, true, false, true);
			}, true, true, true, false);
		}
		this.DoLoadSaveGameAutomation();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DoLoadSaveGameAutomation()
	{
		EPlatformLoadSaveGameState loadSaveGameState = PlatformApplicationManager.GetLoadSaveGameState();
		if (loadSaveGameState != EPlatformLoadSaveGameState.NewGameOpen)
		{
			if (loadSaveGameState != EPlatformLoadSaveGameState.ContinueGameOpen)
			{
				return;
			}
			if (!this.btnContinueGame.Enabled)
			{
				PlatformApplicationManager.SetFailedLoadSaveGame();
				return;
			}
			this.btnContinueGame_OnPressed(this.btnContinueGame, -1);
			if (!base.xui.playerUI.windowManager.IsWindowOpen(XUiC_NewContinueGame.ID))
			{
				PlatformApplicationManager.SetFailedLoadSaveGame();
				return;
			}
			PlatformApplicationManager.AdvanceLoadSaveGameStateFrom(loadSaveGameState);
			return;
		}
		else
		{
			if (!this.btnNewGame.Enabled)
			{
				PlatformApplicationManager.SetFailedLoadSaveGame();
				return;
			}
			this.btnNewGame_OnPressed(this.btnNewGame, -1);
			if (!base.xui.playerUI.windowManager.IsWindowOpen(XUiC_NewContinueGame.ID))
			{
				PlatformApplicationManager.SetFailedLoadSaveGame();
				return;
			}
			PlatformApplicationManager.AdvanceLoadSaveGameStateFrom(loadSaveGameState);
			return;
		}
	}

	public override bool GetBindingValue(ref string _value, string _bindingName)
	{
		if (_bindingName == "systemtime_hr")
		{
			_value = DateTime.Now.Hour.ToString();
			return true;
		}
		if (_bindingName == "systemtime_min")
		{
			_value = DateTime.Now.Minute.ToString();
			return true;
		}
		if (_bindingName == "systemtime_sec")
		{
			_value = DateTime.Now.Second.ToString();
			return true;
		}
		if (!(_bindingName == "has_saved_game"))
		{
			return base.GetBindingValue(ref _value, _bindingName);
		}
		_value = this.anySaveFilesExist.ToString();
		return true;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		PlatformManager.MultiPlatform.RichPresence.UpdateRichPresence(IRichPresence.PresenceStates.Menu);
		TitleStorageOverridesManager.Instance.FetchFromSource(null);
		XUiC_MainMenu.openedOnce = true;
		SaveDataUtils.SaveDataManager.CommitAsync();
		this.anySaveFilesExist = (GameIO.GetPlayerSaves(null, false) > 0);
		base.xui.playerUI.windowManager.Close("eacWarning");
		base.xui.playerUI.windowManager.Close("crossplayWarning");
		XUiC_MainMenuPlayerName.OpenIfNotOpen(base.xui);
		if (base.xui.playerUI.ActionSetManager != null)
		{
			base.xui.playerUI.ActionSetManager.Reset();
			base.xui.playerUI.ActionSetManager.Push(base.xui.playerUI.playerInput);
			if (base.xui.playerUI.windowManager.IsWindowOpen(GUIWindowConsole.ID))
			{
				base.xui.playerUI.ActionSetManager.Push(base.xui.playerUI.playerInput.GUIActions);
			}
		}
		this.btnConnectToServer.Enabled = (PlatformManager.MultiPlatform.User.UserStatus != EUserStatus.OfflineMode);
		base.xui.playerUI.nguiWindowManager.Show(EnumNGUIWindow.MainMenuBackground, false);
		base.xui.playerUI.windowManager.Open("menuBackground", false, false, true);
		base.xui.playerUI.windowManager.Open("mainMenuLogo", false, false, true);
		GameManager.Instance.SetCursorEnabledOverride(false, false);
		base.xui.playerUI.CursorController.SetCursorHidden(false);
		base.GetChildById("btnNewGame").SelectCursorElement(true, false);
		base.xui.playerUI.windowManager.OpenIfNotOpen("CalloutGroup", false, false, true);
		base.xui.calloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonSouth, "igcoSelect", XUiC_GamepadCalloutWindow.CalloutType.Menu);
		base.xui.calloutWindow.RemoveCallout(UIUtils.ButtonIcon.FaceButtonEast, "igcoBack", XUiC_GamepadCalloutWindow.CalloutType.Menu);
		base.xui.calloutWindow.SetCalloutsEnabled(XUiC_GamepadCalloutWindow.CalloutType.Menu, true);
		TriggerEffectManager.SetMainMenuLightbarColor();
	}

	public override void OnClose()
	{
		base.OnClose();
		base.xui.playerUI.windowManager.Close("mainMenuLogo");
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonEast, "igcoBack", XUiC_GamepadCalloutWindow.CalloutType.Menu);
		XUiC_MainMenuPlayerName.Close(base.xui);
	}

	public static void Open(XUi _xuiInstance)
	{
		if (LaunchPrefs.SkipNewsScreen.Value || PlatformApplicationManager.GetLoadSaveGameState() != EPlatformLoadSaveGameState.Done)
		{
			XUiC_MainMenu.shownNewsScreenOnce = true;
		}
		IJoinSessionGameInviteListener joinSessionGameInviteListener = PlatformManager.NativePlatform.JoinSessionGameInviteListener;
		if (joinSessionGameInviteListener != null && joinSessionGameInviteListener.HasPendingIntent())
		{
			XUiC_MainMenu.shownNewsScreenOnce = true;
		}
		if (!XUiC_MainMenu.shownNewsScreenOnce)
		{
			XUiC_NewsScreen.Open(_xuiInstance);
			XUiC_MainMenu.shownNewsScreenOnce = true;
			return;
		}
		_xuiInstance.playerUI.windowManager.Open(XUiC_MainMenu.ID, true, false, true);
	}

	public static string ID = "";

	public static bool openedOnce;

	public static bool shownNewsScreenOnce;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool allowEditTools;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool anySaveFilesExist;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnNewGame;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnContinueGame;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnConnectToServer;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnEditingTools;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnRWG;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnOptions;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnCredits;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnNews;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnQuit;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_MultiplayerPrivilegeNotification wdwMultiplayerPrivileges;
}

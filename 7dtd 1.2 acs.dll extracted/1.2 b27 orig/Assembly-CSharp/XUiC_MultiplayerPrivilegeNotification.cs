using System;
using System.Collections;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_MultiplayerPrivilegeNotification : XUiController
{
	public static XUiC_MultiplayerPrivilegeNotification GetWindow()
	{
		if (GameManager.Instance.gameStateManager.IsGameStarted())
		{
			return (XUiC_MultiplayerPrivilegeNotification)((XUiWindowGroup)LocalPlayerUI.GetUIForPrimaryPlayer().windowManager.GetWindow(XUiC_MultiplayerPrivilegeNotification.InGameWindowID)).Controller;
		}
		return (XUiC_MultiplayerPrivilegeNotification)((XUiWindowGroup)LocalPlayerUI.primaryUI.windowManager.GetWindow(XUiC_MultiplayerPrivilegeNotification.MenuWindowID)).Controller;
	}

	public static void Close()
	{
		XUiC_MultiplayerPrivilegeNotification window = XUiC_MultiplayerPrivilegeNotification.GetWindow();
		string text = (window != null) ? window.ID : null;
		if (!string.IsNullOrEmpty(text))
		{
			LocalPlayerUI.primaryUI.windowManager.Close(text);
		}
	}

	public override void Init()
	{
		base.Init();
		if (base.WindowGroup.ID.Contains("menu", StringComparison.OrdinalIgnoreCase))
		{
			XUiC_MultiplayerPrivilegeNotification.MenuWindowID = base.WindowGroup.ID;
		}
		else
		{
			if (!base.WindowGroup.ID.Contains("ingame", StringComparison.OrdinalIgnoreCase))
			{
				throw new Exception("Found Window Group for XUiC_MultiplayerPrivilegeNotification, name didn't contain \"menu\" or \"ingame\"");
			}
			XUiC_MultiplayerPrivilegeNotification.InGameWindowID = base.WindowGroup.ID;
		}
		this.ID = base.WindowGroup.ID;
		this.btnCancel = (XUiC_SimpleButton)base.GetChildById("btnCancel");
		this.btnCancel.OnPressed += this.BtnCancel_OnPressed;
		this.btnClose = (XUiC_SimpleButton)base.GetChildById("btnClose");
		this.btnClose.OnPressed += this.BtnClose_OnPressed;
		this.lblResolvingPrivileges = (XUiV_Label)base.GetChildById("lblResolvingPrivileges").ViewComponent;
		this.lblInvalidPrivileges = (XUiV_Label)base.GetChildById("lblInvalidPrivileges").ViewComponent;
		this.header = (XUiV_Panel)base.GetChildById("header").ViewComponent;
		this.content = (XUiV_Panel)base.GetChildById("content").ViewComponent;
		this.buttons = (XUiV_Panel)base.GetChildById("buttons").ViewComponent;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCancel_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (this.cancellationToken != null)
		{
			this.cancellationToken.Cancel();
		}
		else
		{
			this.CloseWindow(false);
		}
		this.btnCancel.Enabled = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnClose_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.CloseWindow(false);
	}

	public override void OnClose()
	{
		if (this.cancellationToken != null)
		{
			this.cancellationToken.Cancel();
		}
		this.lblResolvingPrivileges.IsVisible = false;
		this.lblInvalidPrivileges.IsVisible = false;
		this.btnCancel.IsVisible = false;
		this.btnClose.IsVisible = false;
		base.OnClose();
		if (XUiC_ProgressWindow.IsWindowOpen())
		{
			this.CloseWindow(false);
		}
	}

	public bool ResolvePrivilegesWithDialog(EUserPerms _permissionsWithPrompt, Action<bool> _resolutionComplete, EUserPerms _permissionsSilent = (EUserPerms)0, float _delayDisplay = -1f, bool _usingProgressWindow = false, Action _cancellationCleanupAction = null)
	{
		if (this.resolving)
		{
			return false;
		}
		if (_permissionsWithPrompt == (EUserPerms)0 && _permissionsSilent == (EUserPerms)0)
		{
			Log.Error("No privileges specified.");
			return false;
		}
		this.resolving = true;
		this.cancellationToken = new CoroutineCancellationToken();
		this.cancellationCleanupAction = _cancellationCleanupAction;
		if (_usingProgressWindow)
		{
			string text = Localization.Get("lblResolvingPrivileges", false) + "\n\n[FFFFFF]" + Utils.GetCancellationMessage();
			XUiC_ProgressWindow.Open(LocalPlayerUI.primaryUI, text, delegate
			{
				this.cancellationToken.Cancel();
			}, true, true, true, false);
		}
		else
		{
			base.xui.playerUI.windowManager.Open(this.ID, false, false, true);
			this.SetContentVisibility(false);
			this.btnCancel.Enabled = true;
			this.btnCancel.IsVisible = true;
			this.btnClose.Enabled = false;
			this.btnClose.IsVisible = false;
			this.lblResolvingPrivileges.IsVisible = true;
			this.lblInvalidPrivileges.IsVisible = false;
		}
		if (_delayDisplay < 0f)
		{
			_delayDisplay = this.GetDefaultPlatformDelay();
		}
		ThreadManager.StartCoroutine(this.DelayPanelVisibility(_delayDisplay));
		ThreadManager.StartCoroutine(this.ResolvePrivilegesCoroutine(_permissionsWithPrompt, _permissionsSilent, _resolutionComplete, _usingProgressWindow));
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator ResolvePrivilegesCoroutine(EUserPerms _permissionsWithPrompt, EUserPerms _permissionsSilent, Action<bool> _resolutionComplete, bool _usingProgressWindow)
	{
		EUserPerms allPermissions = _permissionsWithPrompt | _permissionsSilent;
		_permissionsSilent &= ~_permissionsWithPrompt;
		if (_permissionsWithPrompt != (EUserPerms)0)
		{
			yield return PermissionsManager.ResolvePermissions(_permissionsWithPrompt, true, this.cancellationToken);
		}
		if (_permissionsSilent != (EUserPerms)0)
		{
			yield return PermissionsManager.ResolvePermissions(_permissionsSilent, false, this.cancellationToken);
		}
		this.resolving = false;
		CoroutineCancellationToken coroutineCancellationToken = this.cancellationToken;
		if (coroutineCancellationToken != null && coroutineCancellationToken.IsCancelled())
		{
			Action action = this.cancellationCleanupAction;
			if (action != null)
			{
				action();
			}
			this.CloseWindow(false);
			yield break;
		}
		this.eulaAccepted = GameManager.HasAcceptedLatestEula();
		bool flag = this.eulaAccepted && (PermissionsManager.GetPermissions(PermissionsManager.PermissionSources.All) & _permissionsWithPrompt) == _permissionsWithPrompt;
		if (_usingProgressWindow)
		{
			if (flag)
			{
				this.CloseWindow(true);
			}
			else
			{
				string text = (!this.eulaAccepted) ? Localization.Get("uiPermissionsEula", false) : (PermissionsManager.GetPermissionDenyReason(_permissionsWithPrompt, PermissionsManager.PermissionSources.All) ?? Localization.Get("lblInvalidPrivileges", false));
				text = text + "\n\n[FFFFFF]" + Utils.GetCancellationMessage();
				XUiC_ProgressWindow.Open(LocalPlayerUI.primaryUI, text, delegate
				{
					this.CloseWindow(false);
				}, true, true, true, false);
			}
		}
		else if (flag)
		{
			this.CloseWindow(true);
		}
		else
		{
			this.lblResolvingPrivileges.IsVisible = false;
			string text2;
			if (!this.eulaAccepted)
			{
				text2 = Localization.Get("uiPermissionsEula", false);
			}
			else
			{
				text2 = PermissionsManager.GetPermissionDenyReason(_permissionsWithPrompt, PermissionsManager.PermissionSources.All);
			}
			if (!string.IsNullOrEmpty(text2))
			{
				this.btnCancel.Enabled = false;
				this.btnCancel.IsVisible = false;
				this.btnClose.Enabled = true;
				this.btnClose.IsVisible = true;
				this.lblInvalidPrivileges.Text = text2;
				this.lblInvalidPrivileges.IsVisible = true;
				this.SetContentVisibility(true);
			}
			else
			{
				this.CloseWindow(false);
			}
		}
		bool obj = this.eulaAccepted && (PermissionsManager.GetPermissions(PermissionsManager.PermissionSources.All) & allPermissions) == allPermissions;
		if (_resolutionComplete != null)
		{
			_resolutionComplete(obj);
		}
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CloseWindow(bool _success)
	{
		if (!XUiC_ProgressWindow.IsWindowOpen())
		{
			base.xui.playerUI.windowManager.Close(this.ID);
			return;
		}
		XUiC_ProgressWindow.Close(LocalPlayerUI.primaryUI);
		if (_success || GameManager.Instance.gameStateManager.IsGameStarted())
		{
			return;
		}
		LocalPlayerUI.primaryUI.windowManager.Open(XUiC_MainMenu.ID, true, false, true);
		if (!GameManager.HasAcceptedLatestEula())
		{
			XUiC_EulaWindow.Open(base.xui, false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float GetDefaultPlatformDelay()
	{
		if (DeviceFlags.Current == DeviceFlag.PS5)
		{
			return 1f;
		}
		return 0.2f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetContentVisibility(bool _visible)
	{
		this.header.IsVisible = _visible;
		this.content.IsVisible = _visible;
		this.buttons.IsVisible = _visible;
		if (_visible)
		{
			if (this.btnClose.IsVisible)
			{
				base.xui.playerUI.CursorController.SetNavigationLockView(this.buttons, this.btnClose.ViewComponent);
				this.btnClose.SelectCursorElement(true, false);
				return;
			}
			if (this.btnCancel.IsVisible)
			{
				base.xui.playerUI.CursorController.SetNavigationLockView(this.buttons, this.btnCancel.ViewComponent);
				this.btnCancel.SelectCursorElement(true, false);
				return;
			}
		}
		else
		{
			base.xui.playerUI.CursorController.SetNavigationLockView(null, null);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator DelayPanelVisibility(float _delay)
	{
		yield return new WaitForSeconds(_delay);
		if (this.resolving)
		{
			this.SetContentVisibility(true);
		}
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string MenuWindowID;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string InGameWindowID;

	[PublicizedFrom(EAccessModifier.Private)]
	public string ID;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnCancel;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnClose;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblResolvingPrivileges;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblInvalidPrivileges;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Panel header;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Panel content;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Panel buttons;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool resolving;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool eulaAccepted;

	[PublicizedFrom(EAccessModifier.Private)]
	public CoroutineCancellationToken cancellationToken;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action cancellationCleanupAction;
}

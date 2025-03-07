﻿using System;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_LoginPS5 : XUiController
{
	public override void Init()
	{
		base.Init();
		XUiC_LoginPS5.ID = base.WindowGroup.ID;
		this.btnRetry = (XUiC_SimpleButton)base.GetChildById("btnRetry");
		this.btnRetry.OnPressed += this.BtnRetry_OnPressed;
		this.btnOffline = (XUiC_SimpleButton)base.GetChildById("btnOffline");
		this.btnOffline.OnPressed += this.BtnOffline_OnPressed;
	}

	public override bool GetBindingValue(ref string _value, string _bindingName)
	{
		if (_bindingName == "title")
		{
			_value = ((this.offendingPlatform == null) ? "" : string.Format(Localization.Get("xuiPSNLogin", false), this.offendingPlatform.PlatformDisplayName));
			return true;
		}
		if (_bindingName == "caption")
		{
			_value = ((this.offendingPlatform == null) ? "" : string.Format(Localization.Get("xuiSteamLoginFailure", false), this.offendingPlatform.PlatformDisplayName));
			return true;
		}
		if (!(_bindingName == "reason"))
		{
			return base.GetBindingValue(ref _value, _bindingName);
		}
		_value = ((this.offendingPlatform == null) ? "" : string.Format(Localization.Get("xuiSteamLoginReason" + this.statusReason.ToStringCached<EApiStatusReason>(), false), this.offendingPlatform.PlatformDisplayName, this.statusReasonAdditionalText));
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnRetry_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.btnOffline.Enabled = false;
		this.btnRetry.Enabled = false;
		this.offendingPlatform = null;
		this.wantOffline = false;
		base.RefreshBindings(false);
		PlatformManager.MultiPlatform.User.Login(new LoginUserCallback(this.updateState));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnOffline_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.btnOffline.Enabled = false;
		this.btnRetry.Enabled = false;
		this.offendingPlatform = null;
		base.RefreshBindings(false);
		this.wantOffline = true;
		PlatformManager.MultiPlatform.User.PlayOffline(new LoginUserCallback(this.updateState));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateState(IPlatform _platform, EApiStatusReason _statusReason, string _statusReasonAdditionalText)
	{
		if (_platform.User.UserStatus == EUserStatus.LoggedIn || (this.wantOffline && _platform.User.UserStatus == EUserStatus.OfflineMode))
		{
			base.xui.playerUI.windowManager.Close(this.windowGroup.ID);
			Action action = this.onLoginComplete;
			if (action != null)
			{
				action();
			}
			this.onLoginComplete = null;
			return;
		}
		this.btnRetry.Enabled = (_platform.Api.ClientApiStatus != EApiStatus.PermanentError);
		this.btnOffline.Enabled = (_platform.User.UserStatus == EUserStatus.OfflineMode);
		this.offendingPlatform = _platform;
		this.statusReason = _statusReason;
		this.statusReasonAdditionalText = _statusReasonAdditionalText;
		base.RefreshBindings(false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void Open(XUi _xuiInstance, IPlatform _platform, EApiStatusReason _statusReason, string _statusReasonAdditionalText, Action _onLoginComplete)
	{
		XUiC_LoginPS5 childByType = _xuiInstance.FindWindowGroupByName(XUiC_LoginPS5.ID).GetChildByType<XUiC_LoginPS5>();
		_xuiInstance.playerUI.windowManager.Open(XUiC_LoginPS5.ID, true, true, true);
		childByType.onLoginComplete = _onLoginComplete;
		childByType.updateState(_platform, _statusReason, _statusReasonAdditionalText);
	}

	public static void Login(XUi _xuiInstance, Action _onLoginComplete)
	{
		PlatformManager.MultiPlatform.User.Login(delegate(IPlatform _platform, EApiStatusReason _statusReason, string _statusReasonAdditionalText)
		{
			if (_platform.Api.ClientApiStatus != EApiStatus.Ok)
			{
				XUiC_LoginPS5.Open(_xuiInstance, _platform, _statusReason, _statusReasonAdditionalText, _onLoginComplete);
				return;
			}
			if (_platform.User.UserStatus != EUserStatus.LoggedIn || _statusReason != EApiStatusReason.Ok)
			{
				XUiC_LoginPS5.Open(_xuiInstance, _platform, _statusReason, _statusReasonAdditionalText, _onLoginComplete);
				return;
			}
			_onLoginComplete();
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnRetry;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnOffline;

	[PublicizedFrom(EAccessModifier.Private)]
	public IPlatform offendingPlatform;

	[PublicizedFrom(EAccessModifier.Private)]
	public EApiStatusReason statusReason;

	[PublicizedFrom(EAccessModifier.Private)]
	public string statusReasonAdditionalText;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool wantOffline;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action onLoginComplete;
}

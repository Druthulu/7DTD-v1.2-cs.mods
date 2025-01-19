using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ConfirmationPrompt : XUiController
{
	public bool IsVisible
	{
		get
		{
			return this.viewComponent.IsVisible;
		}
	}

	public override void Init()
	{
		base.Init();
		this.viewComponent.IsVisible = false;
		this.btnCancel = (XUiC_SimpleButton)base.GetChildById("btnPromptCancel");
		this.btnConfirm = (XUiC_SimpleButton)base.GetChildById("btnPromptConfirm");
		this.btnCancel.Button.Controller.OnPress += this.BtnCancel_OnPress;
		this.btnConfirm.Button.Controller.OnPress += this.BtnConfirm_OnPress;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.viewComponent.IsVisible = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnConfirm_OnPress(XUiController _sender, int _mouseButton)
	{
		this.Confirm();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCancel_OnPress(XUiController _sender, int _mouseButton)
	{
		this.Cancel();
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

	public void ShowPrompt(string headerText, string bodyText, string cancelText, string confirmText, Action<XUiC_ConfirmationPrompt.Result> callback)
	{
		this.headerText = headerText;
		this.bodyText = bodyText;
		this.cancelText = cancelText;
		this.confirmText = confirmText;
		this.resultHandler = callback;
		this.viewComponent.IsVisible = true;
		base.xui.playerUI.CursorController.SetNavigationLockView(this.viewComponent, this.btnCancel.ViewComponent);
		this.IsDirty = true;
	}

	public override bool GetBindingValue(ref string _value, string _bindingName)
	{
		if (_bindingName == "headertext")
		{
			_value = this.headerText;
			return true;
		}
		if (_bindingName == "bodytext")
		{
			_value = this.bodyText;
			return true;
		}
		if (_bindingName == "canceltext")
		{
			_value = this.cancelText;
			return true;
		}
		if (_bindingName == "confirmtext")
		{
			_value = this.confirmText;
			return true;
		}
		if (!(_bindingName == "confirmvisible"))
		{
			return base.GetBindingValue(ref _value, _bindingName);
		}
		_value = (!string.IsNullOrWhiteSpace(this.confirmText)).ToString();
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ClosePrompt()
	{
		base.xui.playerUI.CursorController.SetNavigationLockView(null, null);
		this.viewComponent.IsVisible = false;
	}

	public void Confirm()
	{
		if (!this.IsVisible)
		{
			return;
		}
		this.ClosePrompt();
		this.resultHandler(XUiC_ConfirmationPrompt.Result.Confirmed);
	}

	public void Cancel()
	{
		if (!this.IsVisible)
		{
			return;
		}
		this.ClosePrompt();
		this.resultHandler(XUiC_ConfirmationPrompt.Result.Cancelled);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Action<XUiC_ConfirmationPrompt.Result> resultHandler;

	[PublicizedFrom(EAccessModifier.Private)]
	public string headerText;

	[PublicizedFrom(EAccessModifier.Private)]
	public string bodyText;

	[PublicizedFrom(EAccessModifier.Private)]
	public string cancelText;

	[PublicizedFrom(EAccessModifier.Private)]
	public string confirmText;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnCancel;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnConfirm;

	public enum Result
	{
		Cancelled,
		Confirmed
	}
}

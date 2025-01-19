using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ServerPasswordWindow : XUiController
{
	public override bool GetBindingValue(ref string _value, string _bindingName)
	{
		return _bindingName == "msgText";
	}

	public override void Init()
	{
		base.Init();
		XUiC_ServerPasswordWindow.ID = base.WindowGroup.ID;
		this.lblPassNormal = (XUiV_Label)base.GetChildById("lblPassNormal").ViewComponent;
		this.lblPassIncorrect = (XUiV_Label)base.GetChildById("lblPassIncorrect").ViewComponent;
		this.txtPassword = (XUiC_TextInput)base.GetChildById("txtPassword");
		this.txtPassword.OnSubmitHandler += this.TxtPassword_OnSubmitHandler;
		((XUiC_SimpleButton)base.GetChildById("btnCancel")).OnPressed += this.BtnCancel_OnPressed;
		((XUiC_SimpleButton)base.GetChildById("btnSubmit")).OnPressed += this.BtnSubmit_OnPressed;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TxtPassword_OnSubmitHandler(XUiController _sender, string _text)
	{
		this.BtnSubmit_OnPressed(_sender, -1);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnSubmit_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.onCancel = null;
		this.onSubmit(this.txtPassword.Text);
		base.xui.playerUI.windowManager.Close(base.WindowGroup.ID);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCancel_OnPressed(XUiController _sender, int _mouseButton)
	{
		Action action = this.onCancel;
		this.onCancel = null;
		base.xui.playerUI.windowManager.Close(base.WindowGroup.ID);
		if (action == null)
		{
			return;
		}
		action();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.IsDirty)
		{
			base.RefreshBindings(true);
			this.IsDirty = false;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		base.xui.playerUI.CursorController.SetNavigationLockView(this.viewComponent, null);
	}

	public override void OnClose()
	{
		base.OnClose();
		if (this.onCancel != null)
		{
			this.BtnCancel_OnPressed(this, -1);
		}
		base.xui.playerUI.CursorController.SetNavigationLockView(null, null);
		this.txtPassword.Text = "";
	}

	public override void UpdateInput()
	{
		base.UpdateInput();
		if (base.xui.playerUI.playerInput != null && base.xui.playerUI.playerInput.PermanentActions.Cancel.WasReleased)
		{
			base.xui.playerUI.windowManager.Close(this.windowGroup.ID);
		}
	}

	public static void OpenPasswordWindow(XUi _xuiInstance, bool _badPassword, string _currentPwd, bool _modal, Action<string> _onSubmitDelegate, Action _onCancelDelegate)
	{
		XUiC_ServerPasswordWindow childByType = _xuiInstance.FindWindowGroupByName(XUiC_ServerPasswordWindow.ID).GetChildByType<XUiC_ServerPasswordWindow>();
		childByType.txtPassword.Text = _currentPwd;
		_xuiInstance.playerUI.windowManager.Open(XUiC_ServerPasswordWindow.ID, _modal, false, true);
		childByType.onSubmit = _onSubmitDelegate;
		childByType.onCancel = _onCancelDelegate;
		if (_badPassword)
		{
			childByType.lblPassNormal.IsVisible = false;
			childByType.lblPassIncorrect.IsVisible = true;
			return;
		}
		childByType.lblPassNormal.IsVisible = true;
		childByType.lblPassIncorrect.IsVisible = false;
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtPassword;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblPassNormal;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblPassIncorrect;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action<string> onSubmit;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action onCancel;
}

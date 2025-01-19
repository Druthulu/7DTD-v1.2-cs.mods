using System;
using Audio;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_KeypadWindow : XUiController
{
	public override void Init()
	{
		XUiC_KeypadWindow.ID = this.windowGroup.ID;
		base.Init();
		this.txtPassword = (XUiC_TextInput)base.GetChildById("txtPassword");
		this.txtPassword.OnSubmitHandler += this.TxtPassword_OnSubmitHandler;
		XUiC_SimpleButton xuiC_SimpleButton = (XUiC_SimpleButton)base.GetChildById("btnCancel");
		xuiC_SimpleButton.OnPressed += this.BtnCancel_OnPressed;
		xuiC_SimpleButton.ViewComponent.NavUpTarget = this.txtPassword.ViewComponent;
		XUiC_SimpleButton xuiC_SimpleButton2 = (XUiC_SimpleButton)base.GetChildById("btnOk");
		xuiC_SimpleButton2.OnPressed += this.BtnOk_OnPressed;
		xuiC_SimpleButton2.ViewComponent.NavUpTarget = this.txtPassword.ViewComponent;
		this.txtPassword.ViewComponent.NavDownTarget = xuiC_SimpleButton.ViewComponent;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TextInput_OnInputAbortedHandler(XUiController _sender)
	{
		base.xui.playerUI.windowManager.Close(base.WindowGroup.ID);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TxtPassword_OnSubmitHandler(XUiController _sender, string _text)
	{
		this.BtnOk_OnPressed(_sender, -1);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnOk_OnPressed(XUiController _sender, int _mouseButton)
	{
		string text = this.txtPassword.Text;
		bool flag;
		if (this.LockedItem.CheckPassword(text, PlatformManager.InternalLocalUserIdentifier, out flag))
		{
			if (this.LockedItem.LocalPlayerIsOwner())
			{
				if (flag)
				{
					if (text.Length == 0)
					{
						GameManager.ShowTooltip(base.xui.playerUI.entityPlayer, "passcodeRemoved", false);
					}
					else
					{
						GameManager.ShowTooltip(base.xui.playerUI.entityPlayer, "passcodeSet", false);
					}
				}
				Manager.PlayInsidePlayerHead("Misc/password_set", -1, 0f, false, false);
			}
			else
			{
				GameManager.ShowTooltip(base.xui.playerUI.entityPlayer, "passcodeAccepted", false);
				Manager.PlayInsidePlayerHead("Misc/password_pass", -1, 0f, false, false);
			}
			base.xui.playerUI.windowManager.Close(base.WindowGroup.ID);
			return;
		}
		Manager.PlayInsidePlayerHead("Misc/password_fail", -1, 0f, false, false);
		GameManager.ShowTooltip(base.xui.playerUI.entityPlayer, "passcodeRejected", false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCancel_OnPressed(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(base.WindowGroup.ID);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		base.xui.playerUI.entityPlayer.PlayOneShot("open_sign", false, false, false);
		base.xui.playerUI.CursorController.SetNavigationLockView(this.viewComponent, this.txtPassword.ViewComponent);
	}

	public override void OnClose()
	{
		base.OnClose();
		base.xui.playerUI.entityPlayer.PlayOneShot("close_sign", false, false, false);
		this.LockedItem = null;
		base.xui.playerUI.CursorController.SetNavigationLockView(null, null);
	}

	public static void Open(LocalPlayerUI _playerUi, ILockable _lockedItem)
	{
		_playerUi.xui.FindWindowGroupByName(XUiC_KeypadWindow.ID).GetChildByType<XUiC_KeypadWindow>().LockedItem = _lockedItem;
		_playerUi.windowManager.Open(XUiC_KeypadWindow.ID, true, false, true);
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtPassword;

	[PublicizedFrom(EAccessModifier.Private)]
	public ILockable LockedItem;
}

using System;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_NewsScreen : XUiController
{
	public override void Init()
	{
		base.Init();
		XUiC_NewsScreen.ID = base.WindowGroup.ID;
		XUiController childById = base.GetChildById("btnContinue");
		if (childById != null && childById.ViewComponent is XUiV_Button)
		{
			childById.OnPress += this.BtnContinue_OnPress;
		}
		this.controllerContinueLabel = (base.GetChildById("continueButtonLabelController").ViewComponent as XUiV_Label);
		if (DeviceFlag.PS5.IsCurrent())
		{
			this.UpdateContinueLabel(PlayerInputManager.InputStyle.PS4);
			return;
		}
		if ((DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX).IsCurrent())
		{
			this.UpdateContinueLabel(PlayerInputManager.InputStyle.XB1);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void InputStyleChanged(PlayerInputManager.InputStyle _oldStyle, PlayerInputManager.InputStyle _newStyle)
	{
		base.InputStyleChanged(_oldStyle, _newStyle);
		base.RefreshBindings(true);
		this.UpdateContinueLabel(_newStyle);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateContinueLabel(PlayerInputManager.InputStyle _style)
	{
		if (_style != PlayerInputManager.InputStyle.Keyboard)
		{
			string arg;
			if (PlatformManager.NativePlatform.Input.CurrentControllerInputStyle == PlayerInputManager.InputStyle.PS4)
			{
				arg = "[sp=PS5_Button_Options]";
			}
			else
			{
				arg = "[sp=XB_Button_Menu]";
			}
			string text = Localization.Get("xuiNewsContinueController", false).ToUpper();
			text = string.Format(text, arg);
			this.controllerContinueLabel.Text = text;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnContinue_OnPress(XUiController _sender, int _mousebutton)
	{
		XUiC_MainMenu.Open(base.xui);
	}

	public override void UpdateInput()
	{
		base.UpdateInput();
		if (base.xui.playerUI.playerInput.GUIActions.Apply.WasReleased && !base.xui.playerUI.windowManager.IsWindowOpen(XUiC_MessageBoxWindowGroup.ID))
		{
			this.BtnContinue_OnPress(this, -1);
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		base.RefreshBindings(true);
		base.WindowGroup.openWindowOnEsc = XUiC_MainMenu.ID;
	}

	public static void Open(XUi _xuiInstance)
	{
		_xuiInstance.playerUI.windowManager.Open(XUiC_NewsScreen.ID, true, false, true);
	}

	public override void OnClose()
	{
		base.OnClose();
		base.xui.playerUI.windowManager.CloseIfOpen(XUiC_MessageBoxWindowGroup.ID);
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label controllerContinueLabel;
}

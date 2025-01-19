using System;
using InControl;
using Platform;
using UnityEngine.Scripting;

[UnityEngine.Scripting.Preserve]
public class XUiC_EmptyInfoWindow : XUiC_InfoWindow
{
	public override void Init()
	{
		base.Init();
		this.descriptionText = (base.GetChildById("descriptionText").ViewComponent as XUiV_Label);
		base.RegisterForInputStyleChanges();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.UpdateDescriptionText();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void InputStyleChanged(PlayerInputManager.InputStyle _oldStyle, PlayerInputManager.InputStyle _newStyle)
	{
		base.InputStyleChanged(_oldStyle, _newStyle);
		this.UpdateDescriptionText();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateDescriptionText()
	{
		if (!(DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent() && PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard)
		{
			this.descriptionText.Text = Localization.Get("xuiEmptyInfoPanelText", false);
			return;
		}
		PlayerInputManager.InputStyleFromSelectedIconStyle();
		string text = string.Format(Localization.Get("xuiEmptyInfoPanelTextController", false), new object[]
		{
			InControlExtensions.GetGamepadSourceString(InputControlType.Action1),
			InControlExtensions.GetGamepadSourceString(InputControlType.Action3),
			InControlExtensions.GetGamepadSourceString(InputControlType.Action3),
			InControlExtensions.GetGamepadSourceString(InputControlType.Action4),
			InControlExtensions.GetGamepadSourceString(InputControlType.Action4),
			InControlExtensions.GetGamepadSourceString(InputControlType.DPadUp),
			InControlExtensions.GetGamepadSourceString(InputControlType.DPadDown),
			InControlExtensions.GetGamepadSourceString(InputControlType.DPadLeft),
			InControlExtensions.GetGamepadSourceString(InputControlType.DPadRight),
			InControlExtensions.GetGamepadSourceString(InputControlType.RightStickButton),
			InControlExtensions.GetGamepadSourceString(InputControlType.LeftStickButton),
			InControlExtensions.GetGamepadSourceString(InputControlType.Back)
		});
		this.descriptionText.Text = text;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label descriptionText;
}

using System;
using GUI_2;
using InControl;
using Platform;

public class XUiV_GamepadIcon : XUiV_Sprite
{
	public XUiV_GamepadIcon(string _id) : base(_id)
	{
	}

	public override void InitView()
	{
		base.InitView();
		base.UIAtlas = UIUtils.IconAtlas.name;
		PlatformManager.NativePlatform.Input.OnLastInputStyleChanged += this.OnLastInputStyleChanged;
		this.curInput = PlatformManager.NativePlatform.Input.CurrentInputStyle;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnLastInputStyleChanged(PlayerInputManager.InputStyle _style)
	{
		this.curInput = _style;
	}

	public override void Cleanup()
	{
		base.Cleanup();
		IPlatform nativePlatform = PlatformManager.NativePlatform;
		if (((nativePlatform != null) ? nativePlatform.Input : null) != null)
		{
			PlatformManager.NativePlatform.Input.OnLastInputStyleChanged -= this.OnLastInputStyleChanged;
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.curInput != this.lastInputStyle)
		{
			this.lastInputStyle = this.curInput;
			base.UIAtlas = UIUtils.IconAtlas.name;
		}
	}

	public override bool ParseAttribute(string _attribute, string _value, XUiController _parent)
	{
		if (_attribute == "action")
		{
			PlayerAction playerActionByName = base.xui.playerUI.playerInput.GUIActions.GetPlayerActionByName(_value);
			base.SpriteName = UIUtils.GetSpriteName(UIUtils.GetButtonIconForAction(playerActionByName));
			return true;
		}
		return base.ParseAttribute(_attribute, _value, _parent);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public PlayerInputManager.InputStyle lastInputStyle = PlayerInputManager.InputStyle.Count;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlayerInputManager.InputStyle curInput;
}

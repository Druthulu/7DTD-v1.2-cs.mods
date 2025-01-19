using System;
using GUI_2;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_GamepadCallout : XUiController
{
	public XUiV_Sprite icon { get; [PublicizedFrom(EAccessModifier.Private)] set; }

	public XUiV_Label action { get; [PublicizedFrom(EAccessModifier.Private)] set; }

	public override void Init()
	{
		base.Init();
		this.icon = (XUiV_Sprite)base.GetChildById("icon").ViewComponent;
		this.action = (XUiV_Label)base.GetChildById("action").ViewComponent;
		this.icon.UIAtlas = UIUtils.IconAtlas.name;
	}

	public void SetupCallout(UIUtils.ButtonIcon _icon, string _action)
	{
		this.icon.SpriteName = UIUtils.GetSpriteName(_icon);
		this.action.Text = Localization.Get(_action, false);
	}
}

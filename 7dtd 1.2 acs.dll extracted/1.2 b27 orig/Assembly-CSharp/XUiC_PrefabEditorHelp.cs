using System;
using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PrefabEditorHelp : XUiController
{
	public override void Init()
	{
		base.Init();
		XUiC_PrefabEditorHelp.ID = base.WindowGroup.ID;
		base.GetChildById("outclick").OnPress += this.Close_OnPress;
		this.findLabels(this);
		base.RegisterForInputStyleChanges();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Close_OnPress(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(XUiC_PrefabEditorHelp.ID);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void InputStyleChanged(PlayerInputManager.InputStyle _oldStyle, PlayerInputManager.InputStyle _newStyle)
	{
		base.InputStyleChanged(_oldStyle, _newStyle);
		foreach (XUiV_Label xuiV_Label in this.labels)
		{
			xuiV_Label.ForceTextUpdate();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void findLabels(XUiController _controller)
	{
		foreach (XUiController xuiController in _controller.Children)
		{
			XUiV_Label xuiV_Label = xuiController.ViewComponent as XUiV_Label;
			if (xuiV_Label != null)
			{
				this.labels.Add(xuiV_Label);
			}
			this.findLabels(xuiController);
		}
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<XUiV_Label> labels = new List<XUiV_Label>();
}

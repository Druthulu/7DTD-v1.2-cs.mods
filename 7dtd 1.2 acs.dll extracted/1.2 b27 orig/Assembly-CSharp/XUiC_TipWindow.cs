using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_TipWindow : XUiController
{
	public string TipText
	{
		get
		{
			return this.tipText;
		}
		set
		{
			this.tipText = value;
			this.IsDirty = true;
		}
	}

	public string TipTitle
	{
		get
		{
			return this.tipTitle;
		}
		set
		{
			this.tipTitle = value;
			this.IsDirty = true;
		}
	}

	public ToolTipEvent CloseEvent { get; set; }

	public override bool GetBindingValue(ref string value, string bindingName)
	{
		if (bindingName == "tiptext")
		{
			value = this.TipText;
			return true;
		}
		if (!(bindingName == "tiptitle"))
		{
			return false;
		}
		value = this.TipTitle;
		return true;
	}

	public override void Init()
	{
		base.Init();
		((XUiV_Button)base.GetChildById("clickable").ViewComponent).Controller.OnPress += this.closeButton_OnPress;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void closeButton_OnPress(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(base.WindowGroup.ID);
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

	public static void ShowTip(string _tip, string _title, EntityPlayerLocal _localPlayer, ToolTipEvent _closeEvent)
	{
		LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(_localPlayer);
		if (uiforPlayer != null && uiforPlayer.xui != null && uiforPlayer.windowManager.IsHUDEnabled())
		{
			XUiC_TipWindow childByType = uiforPlayer.xui.FindWindowGroupByName("tipWindow").GetChildByType<XUiC_TipWindow>();
			childByType.TipText = Localization.Get(_tip, false);
			childByType.TipTitle = Localization.Get(_title, false);
			childByType.CloseEvent = _closeEvent;
			uiforPlayer.windowManager.Open("tipWindow", true, false, true);
			uiforPlayer.windowManager.CloseIfOpen("windowpaging");
			uiforPlayer.windowManager.CloseIfOpen("toolbelt");
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		if (this.CloseEvent != null)
		{
			this.CloseEvent.HandleEvent();
			this.CloseEvent = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string tipText = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string tipTitle = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string nextTip = "";
}

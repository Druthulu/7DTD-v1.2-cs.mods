using System;
using GUI_2;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_OptionsMenu : XUiController
{
	public override void Init()
	{
		base.Init();
		UIOptions.OnOptionsVideoWindowChanged += this.OnVideoOptionsWindowChanged;
		XUiC_OptionsMenu.ID = base.WindowGroup.ID;
		(base.GetChildById("btnGeneral") as XUiC_SimpleButton).OnPressed += this.btnGeneral_OnPressed;
		(base.GetChildById("btnVideo") as XUiC_SimpleButton).OnPressed += this.btnVideo_OnPressed;
		(base.GetChildById("btnAudio") as XUiC_SimpleButton).OnPressed += this.btnAudio_OnPressed;
		(base.GetChildById("btnControls") as XUiC_SimpleButton).OnPressed += this.btnControls_OnPressed;
		(base.GetChildById("btnProfiles") as XUiC_SimpleButton).OnPressed += this.btnProfiles_OnPressed;
		(base.GetChildById("btnBlockList") as XUiC_SimpleButton).OnPressed += this.btnBlockList_OnPressed;
		(base.GetChildById("btnAccount") as XUiC_SimpleButton).OnPressed += this.btnAccount_OnPressed;
		(base.GetChildById("btnTwitch") as XUiC_SimpleButton).OnPressed += this.btnTwitch_OnPressed;
		(base.GetChildById("btnController") as XUiC_SimpleButton).OnPressed += this.btnController_OnPressed;
		XUiController[] childrenById = base.GetChildrenById("btnBack", null);
		for (int i = 0; i < childrenById.Length; i++)
		{
			((XUiC_SimpleButton)childrenById[i]).OnPressed += this.btnBack_OnPressed;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnVideo_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.OpenOptions(this.GetOptionsVideoWindowName(UIOptions.OptionsVideoWindow));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnVideoOptionsWindowChanged(OptionsVideoWindowMode _mode)
	{
		if (XUi.InGameMenuOpen && (base.xui.playerUI.windowManager.IsWindowOpen(XUiC_OptionsVideoSimplified.ID) || base.xui.playerUI.windowManager.IsWindowOpen(XUiC_OptionsVideo.ID)))
		{
			this.OpenOptions(this.GetOptionsVideoWindowName(_mode));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string GetOptionsVideoWindowName(OptionsVideoWindowMode _mode)
	{
		if (_mode == OptionsVideoWindowMode.Simplified)
		{
			return XUiC_OptionsVideoSimplified.ID;
		}
		if (_mode != OptionsVideoWindowMode.Detailed)
		{
			Log.Error(string.Format("Unknown video options menu {0}", _mode));
			return XUiC_OptionsVideo.ID;
		}
		return XUiC_OptionsVideo.ID;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnGeneral_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.OpenOptions(XUiC_OptionsGeneral.ID);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnAudio_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.OpenOptions(XUiC_OptionsAudio.ID);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnControls_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.OpenOptions(XUiC_OptionsControls.ID);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnProfiles_OnPressed(XUiController _sender, int _mouseButton)
	{
		int v = EntityClass.FromString("playerMale");
		EntityClass entityClass = EntityClass.list[v];
		this.OpenOptions(XUiC_OptionsProfiles.ID);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnAccount_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.OpenOptions(XUiC_OptionsUsername.ID);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnTwitch_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.OpenOptions(XUiC_OptionsTwitch.ID);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnController_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.OpenOptions(XUiC_OptionsController.ID);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnBlockList_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.OpenOptions(XUiC_OptionsBlockedPlayersList.ID);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnBack_OnPressed(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(this.windowGroup.ID);
		if (GameStats.GetInt(EnumGameStats.GameState) == 0)
		{
			base.xui.playerUI.windowManager.Open(XUiC_MainMenu.ID, true, false, true);
		}
		XUi.InGameMenuOpen = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OpenOptions(string _optionsWindowName)
	{
		this.continueGamePause = true;
		base.xui.playerUI.windowManager.Close(this.windowGroup.ID);
		base.xui.playerUI.windowManager.Open(_optionsWindowName, true, false, true);
		XUi.InGameMenuOpen = true;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.IsDirty = true;
		this.continueGamePause = false;
		this.windowGroup.openWindowOnEsc = ((GameStats.GetInt(EnumGameStats.GameState) == 0) ? XUiC_MainMenu.ID : null);
		base.RefreshBindings(false);
		XUi.InGameMenuOpen = true;
		base.xui.playerUI.windowManager.OpenIfNotOpen("CalloutGroup", false, false, true);
		base.xui.calloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonSouth, "igcoSelect", XUiC_GamepadCalloutWindow.CalloutType.Menu);
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonEast, "igcoBack", XUiC_GamepadCalloutWindow.CalloutType.Menu);
		base.xui.calloutWindow.SetCalloutsEnabled(XUiC_GamepadCalloutWindow.CalloutType.Menu, true);
	}

	public override void OnClose()
	{
		base.OnClose();
		if (!this.continueGamePause && GameStats.GetInt(EnumGameStats.GameState) == 2)
		{
			GameManager.Instance.Pause(false);
		}
		XUi.InGameMenuOpen = false;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.IsDirty)
		{
			base.RefreshBindings(false);
			this.IsDirty = false;
		}
	}

	public override bool GetBindingValue(ref string _value, string _bindingName)
	{
		if (_bindingName == "ingame")
		{
			_value = (GameStats.GetInt(EnumGameStats.GameState) != 0).ToString();
			return true;
		}
		if (_bindingName == "notingame")
		{
			_value = (GameStats.GetInt(EnumGameStats.GameState) == 0).ToString();
			return true;
		}
		if (_bindingName == "notreleaseingame")
		{
			_value = "false";
			return true;
		}
		if (_bindingName == "ingamenoteditor")
		{
			if (GameStats.GetInt(EnumGameStats.GameState) != 0)
			{
				_value = (!GameManager.Instance.World.IsEditor()).ToString();
			}
			else
			{
				_value = "false";
			}
			return true;
		}
		if (!(_bindingName == "showblocklist"))
		{
			return base.GetBindingValue(ref _value, _bindingName);
		}
		_value = (BlockedPlayerList.Instance != null).ToString();
		return true;
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool continueGamePause;
}

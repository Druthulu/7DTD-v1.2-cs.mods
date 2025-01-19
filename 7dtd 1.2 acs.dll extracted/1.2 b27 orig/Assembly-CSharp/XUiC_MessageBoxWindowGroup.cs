using System;
using System.Runtime.CompilerServices;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_MessageBoxWindowGroup : XUiController
{
	public string Title
	{
		get
		{
			return this.title;
		}
		set
		{
			this.title = value;
			this.IsDirty = true;
		}
	}

	public string Text
	{
		get
		{
			return this.text;
		}
		set
		{
			this.text = value;
			this.IsDirty = true;
		}
	}

	public event Action OnLeftButtonEvent;

	public event Action OnRightButtonEvent;

	public override bool GetBindingValue(ref string _value, string _bindingName)
	{
		if (_bindingName == "msgTitle")
		{
			_value = this.title;
			return true;
		}
		if (_bindingName == "msgText")
		{
			_value = this.text;
			return true;
		}
		if (_bindingName == "showleftbutton")
		{
			_value = (this.MessageBoxType == XUiC_MessageBoxWindowGroup.MessageBoxTypes.OkCancel).ToString();
			return true;
		}
		if (_bindingName == "rightbuttontext")
		{
			_value = ((this.MessageBoxType == XUiC_MessageBoxWindowGroup.MessageBoxTypes.Ok) ? "xuiOk" : "xuiCancel");
			return true;
		}
		if (!(_bindingName == "leftbuttontext"))
		{
			return base.GetBindingValue(ref _value, _bindingName);
		}
		_value = ((this.MessageBoxType == XUiC_MessageBoxWindowGroup.MessageBoxTypes.Ok) ? "" : "xuiOk");
		return true;
	}

	public override void Init()
	{
		base.Init();
		XUiC_MessageBoxWindowGroup.ID = base.WindowGroup.ID;
		this.btnLeft = base.GetChildById("clickable2");
		if (this.btnLeft != null)
		{
			((XUiV_Button)this.btnLeft.ViewComponent).Controller.OnPress += this.leftButton_OnPress;
		}
		this.btnRight = base.GetChildById("clickable");
		if (this.btnRight != null)
		{
			((XUiV_Button)this.btnRight.ViewComponent).Controller.OnPress += this.rightButton_OnPress;
		}
		this.leftButtonPressed = false;
		this.rightButtonPressed = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void leftButton_OnPress(XUiController _sender, int _mouseButton)
	{
		if (this.OnLeftButtonEvent != null)
		{
			this.OnLeftButtonEvent();
		}
		this.leftButtonPressed = true;
		base.xui.playerUI.windowManager.Close(base.WindowGroup.ID);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void rightButton_OnPress(XUiController _sender, int _mouseButton)
	{
		if (this.OnRightButtonEvent != null)
		{
			this.OnRightButtonEvent();
		}
		this.rightButtonPressed = true;
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

	public override void UpdateInput()
	{
		base.UpdateInput();
		if (base.xui.playerUI.playerInput.GUIActions.Cancel.WasReleased)
		{
			this.rightButton_OnPress(this, -1);
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.leftButtonPressed = false;
		this.rightButtonPressed = false;
		this.windowGroup.isEscClosable = false;
	}

	public override void OnClose()
	{
		base.OnClose();
		base.xui.playerUI.CursorController.SetNavigationLockView(null, null);
		if (!this.OpenMainMenuOnClose)
		{
			base.xui.playerUI.CursorController.SetNavigationTargetLater(XUiC_MessageBoxWindowGroup.returnNavigationTarget);
		}
		if (this.MessageBoxType == XUiC_MessageBoxWindowGroup.MessageBoxTypes.OkCancel && !this.rightButtonPressed && !this.leftButtonPressed)
		{
			Action onRightButtonEvent = this.OnRightButtonEvent;
			if (onRightButtonEvent != null)
			{
				onRightButtonEvent();
			}
		}
		if (GameManager.Instance.World == null)
		{
			if (this.OpenMainMenuOnClose)
			{
				base.xui.playerUI.windowManager.Open(XUiC_MainMenu.ID, true, false, true);
			}
			ThreadManager.StartCoroutine(PlatformApplicationManager.CheckRestartCoroutine(false));
		}
	}

	public void ShowMessage(string _title, string _text, XUiC_MessageBoxWindowGroup.MessageBoxTypes _messageBoxType = XUiC_MessageBoxWindowGroup.MessageBoxTypes.Ok, Action _onLeftButton = null, Action _onRightButton = null, bool _openMainMenuOnClose = true, bool _modal = true, bool _bCloseAllOpenWindows = true)
	{
		this.Text = _text;
		this.Title = _title;
		this.MessageBoxType = _messageBoxType;
		this.OnLeftButtonEvent = _onLeftButton;
		this.OnRightButtonEvent = _onRightButton;
		this.OpenMainMenuOnClose = _openMainMenuOnClose;
		base.xui.playerUI.windowManager.Open(base.WindowGroup.ID, _modal, false, _bCloseAllOpenWindows);
	}

	public void ShowNetworkError(NetworkConnectionError _error)
	{
		string arg = _error.ToStringCached<NetworkConnectionError>();
		switch (_error)
		{
		case NetworkConnectionError.InternalDirectConnectFailed:
		case NetworkConnectionError.EmptyConnectTarget:
		case NetworkConnectionError.IncorrectParameters:
		case NetworkConnectionError.AlreadyConnectedToAnotherServer:
			break;
		case NetworkConnectionError.CreateSocketOrThreadFailure:
			arg = string.Format(Localization.Get("mmLblErrorSocketFailure", false), SingletonMonoBehaviour<ConnectionManager>.Instance.GetRequiredPortsString());
			goto IL_CD;
		default:
			switch (_error)
			{
			case NetworkConnectionError.ConnectionFailed:
			case NetworkConnectionError.AlreadyConnectedToServer:
			case (NetworkConnectionError)17:
			case NetworkConnectionError.TooManyConnectedPlayers:
			case (NetworkConnectionError)19:
			case (NetworkConnectionError)20:
			case NetworkConnectionError.RSAPublicKeyMismatch:
			case NetworkConnectionError.ConnectionBanned:
				break;
			case NetworkConnectionError.InvalidPassword:
				arg = Localization.Get("mmLblErrorWrongPassword", false);
				goto IL_CD;
			default:
				switch (_error)
				{
				case NetworkConnectionError.RestartRequired:
					arg = string.Format(Localization.Get("app_restartRequired", false), Array.Empty<object>());
					goto IL_CD;
				}
				break;
			}
			break;
		}
		arg = string.Format(Localization.Get("mmLblErrorUnknown", false), arg);
		IL_CD:
		this.ShowMessage(Localization.Get("mmLblErrorServerInit", false), arg, XUiC_MessageBoxWindowGroup.MessageBoxTypes.Ok, null, null, true, true, true);
	}

	public void ShowUrlConfirmationDialog(string _url, string _displayUrl, bool _modal = false, Func<string, bool> _browserOpenMethod = null, string _title = null, string _text = null)
	{
		if (string.IsNullOrEmpty(_url))
		{
			return;
		}
		if (!Utils.IsValidWebUrl(ref _url))
		{
			return;
		}
		this.urlBoxData.Item1 = base.xui.playerUI.windowManager.GetModalWindow();
		this.urlBoxData.Item2 = _url;
		if (_browserOpenMethod == null)
		{
			_browserOpenMethod = ((PlatformManager.NativePlatform.Utils != null) ? new Func<string, bool>(PlatformManager.NativePlatform.Utils.OpenBrowser) : null);
		}
		this.urlBoxData.Item3 = _browserOpenMethod;
		if (_title == null)
		{
			_title = Localization.Get("xuiOpenUrlConfirmationTitle", false);
		}
		if (_text == null)
		{
			_text = Localization.Get("xuiOpenUrlConfirmationText", false);
		}
		_text = string.Format(_text, _displayUrl);
		this.ShowMessage(_title, _text, XUiC_MessageBoxWindowGroup.MessageBoxTypes.OkCancel, new Action(this.openPage), new Action(this.cancelOpenPage), false, _modal, true);
		base.xui.playerUI.playerInput.PermanentActions.Cancel.Enabled = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void openPage()
	{
		this.urlBoxData.Item1 = null;
		base.xui.playerUI.playerInput.PermanentActions.Cancel.Enabled = true;
		Func<string, bool> item = this.urlBoxData.Item3;
		if (item == null)
		{
			return;
		}
		item(this.urlBoxData.Item2);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void cancelOpenPage()
	{
		this.urlBoxData.Item1 = null;
		base.xui.playerUI.playerInput.PermanentActions.Cancel.Enabled = true;
	}

	public static void ShowMessageBox(XUi _xuiInstance, string _title, string _text, XUiC_MessageBoxWindowGroup.MessageBoxTypes _messageBoxType = XUiC_MessageBoxWindowGroup.MessageBoxTypes.Ok, Action _onOk = null, Action _onCancel = null, bool _openMainMenuOnClose = true, bool _bCloseAllOpenWindows = true)
	{
		XUiC_MessageBoxWindowGroup.returnNavigationTarget = _xuiInstance.playerUI.CursorController.navigationTarget;
		((XUiC_MessageBoxWindowGroup)_xuiInstance.FindWindowGroupByName(XUiC_MessageBoxWindowGroup.ID)).ShowMessage(_title, _text, _messageBoxType, _onOk, _onCancel, _openMainMenuOnClose, true, _bCloseAllOpenWindows);
	}

	public static void ShowMessageBox(XUi _xuiInstance, string _title, string _text, Action _onOk = null, bool _openMainMenuOnClose = true)
	{
		XUiC_MessageBoxWindowGroup.returnNavigationTarget = _xuiInstance.playerUI.CursorController.navigationTarget;
		((XUiC_MessageBoxWindowGroup)_xuiInstance.FindWindowGroupByName(XUiC_MessageBoxWindowGroup.ID)).ShowMessage(_title, _text, XUiC_MessageBoxWindowGroup.MessageBoxTypes.Ok, null, _onOk, _openMainMenuOnClose, true, true);
	}

	public static void ShowUrlConfirmationDialog(XUi _xuiInstance, string _url, bool _modal = false, Func<string, bool> _browserOpenMethod = null, string _title = null, string _text = null, string _displayUrl = null)
	{
		XUiC_MessageBoxWindowGroup.returnNavigationTarget = _xuiInstance.playerUI.CursorController.navigationTarget;
		XUiC_MessageBoxWindowGroup xuiC_MessageBoxWindowGroup = (XUiC_MessageBoxWindowGroup)_xuiInstance.FindWindowGroupByName(XUiC_MessageBoxWindowGroup.ID);
		if (_displayUrl == null)
		{
			_displayUrl = _url;
		}
		xuiC_MessageBoxWindowGroup.ShowUrlConfirmationDialog(_url, _displayUrl, _modal, _browserOpenMethod, _title, _text);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string title = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string text = "";

	public static string ID = "";

	public bool OpenMainMenuOnClose = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_MessageBoxWindowGroup.MessageBoxTypes MessageBoxType;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool leftButtonPressed;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool rightButtonPressed;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController btnLeft;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController btnRight;

	[PublicizedFrom(EAccessModifier.Private)]
	public static XUiView returnNavigationTarget = null;

	[TupleElementNames(new string[]
	{
		"prevModalWindow",
		"url",
		"browserOpenMethod"
	})]
	[PublicizedFrom(EAccessModifier.Private)]
	public ValueTuple<GUIWindow, string, Func<string, bool>> urlBoxData;

	public enum MessageBoxTypes
	{
		Ok,
		OkCancel
	}
}

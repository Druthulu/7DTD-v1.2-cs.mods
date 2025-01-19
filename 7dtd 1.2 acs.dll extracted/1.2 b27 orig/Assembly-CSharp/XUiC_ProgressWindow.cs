using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ProgressWindow : XUiController
{
	public string ProgressText
	{
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			this.lblProgress.Text = value;
		}
	}

	public bool UseShadow
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.useShadow;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (this.useShadow != value)
			{
				this.IsDirty = true;
				this.useShadow = value;
			}
		}
	}

	public override void Init()
	{
		base.Init();
		XUiC_ProgressWindow.ID = base.WindowGroup.ID;
		this.lblProgress = (XUiV_Label)base.GetChildById("lblProgress").ViewComponent;
		this.ellipsisAnimator = new TextEllipsisAnimator(null, this.lblProgress);
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		this.ellipsisAnimator.GetNextAnimatedString(_dt);
		if (this.escapeDelegate != null && base.xui.playerUI.playerInput != null && (base.xui.playerUI.playerInput.Menu.WasPressed || base.xui.playerUI.playerInput.PermanentActions.Cancel.WasPressed))
		{
			this.escapeDelegate();
		}
		if (this.IsDirty)
		{
			base.RefreshBindings(true);
			this.IsDirty = false;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		((XUiV_Window)base.ViewComponent).Panel.alpha = 1f;
		base.xui.playerUI.CursorController.SetNavigationTarget(null);
		base.xui.playerUI.CursorController.Locked = true;
	}

	public override void OnClose()
	{
		base.OnClose();
		this.escapeDelegate = null;
		base.xui.playerUI.CursorController.Locked = false;
	}

	public override bool GetBindingValue(ref string _value, string _bindingName)
	{
		if (_bindingName == "use_shadow")
		{
			_value = this.useShadow.ToString();
			return true;
		}
		return base.GetBindingValue(ref _value, _bindingName);
	}

	public static bool IsWindowOpen()
	{
		XUiC_ProgressWindow childByType = LocalPlayerUI.primaryUI.xui.FindWindowGroupByName(XUiC_ProgressWindow.ID).GetChildByType<XUiC_ProgressWindow>();
		return childByType != null && childByType.IsOpen;
	}

	public static void Open(LocalPlayerUI _playerUi, string _text, Action _escDelegate = null, bool _modal = true, bool _escClosable = true, bool _closeOpenWindows = true, bool _useShadow = false)
	{
		if (_playerUi != null && _playerUi.xui != null)
		{
			XUiC_ProgressWindow childByType = _playerUi.xui.FindWindowGroupByName(XUiC_ProgressWindow.ID).GetChildByType<XUiC_ProgressWindow>();
			childByType.baseText = _text;
			childByType.ellipsisAnimator.SetBaseString(childByType.baseText, TextEllipsisAnimator.AnimationMode.All);
			_playerUi.windowManager.Open(XUiC_ProgressWindow.ID, _modal, _escClosable, _closeOpenWindows);
			childByType.escapeDelegate = _escDelegate;
			childByType.UseShadow = _useShadow;
		}
	}

	public static void Close(LocalPlayerUI _playerUi)
	{
		if (_playerUi != null && _playerUi.xui != null)
		{
			_playerUi.windowManager.CloseIfOpen(XUiC_ProgressWindow.ID);
		}
	}

	public static void SetText(LocalPlayerUI _playerUi, string _text, bool _clearEscDelegate = true)
	{
		if (_playerUi != null && _playerUi.xui != null)
		{
			XUiC_ProgressWindow childByType = _playerUi.xui.FindWindowGroupByName(XUiC_ProgressWindow.ID).GetChildByType<XUiC_ProgressWindow>();
			childByType.baseText = _text;
			childByType.ellipsisAnimator.SetBaseString(childByType.baseText, TextEllipsisAnimator.AnimationMode.All);
			if (_clearEscDelegate)
			{
				childByType.escapeDelegate = null;
			}
		}
	}

	public static string GetText(LocalPlayerUI _playerUi)
	{
		return _playerUi.xui.FindWindowGroupByName(XUiC_ProgressWindow.ID).GetChildByType<XUiC_ProgressWindow>().baseText;
	}

	public static void SetEscDelegate(LocalPlayerUI _playerUi, Action _escapeDelegate)
	{
		_playerUi.xui.FindWindowGroupByName(XUiC_ProgressWindow.ID).GetChildByType<XUiC_ProgressWindow>().escapeDelegate = _escapeDelegate;
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool useShadow;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action escapeDelegate;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblProgress;

	[PublicizedFrom(EAccessModifier.Private)]
	public TextEllipsisAnimator ellipsisAnimator;

	[PublicizedFrom(EAccessModifier.Private)]
	public string baseText;
}

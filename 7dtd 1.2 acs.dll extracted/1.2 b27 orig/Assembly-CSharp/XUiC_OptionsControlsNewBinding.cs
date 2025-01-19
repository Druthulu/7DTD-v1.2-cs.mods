using System;
using System.Collections;
using System.Collections.Generic;
using InControl;
using Platform;
using UnityEngine.Scripting;

[UnityEngine.Scripting.Preserve]
public class XUiC_OptionsControlsNewBinding : XUiController
{
	public override void Init()
	{
		base.Init();
		XUiC_OptionsControlsNewBinding.ID = base.WindowGroup.ID;
		this.lblAction = (base.GetChildById("forAction").ViewComponent as XUiV_Label);
		this.rectInUse = (base.GetChildById("inUse").ViewComponent as XUiV_Rect);
		this.lblInUseBy = (base.GetChildById("inUseBy").ViewComponent as XUiV_Label);
		this.lblAbort = (base.GetChildById("newBindingAbort").ViewComponent as XUiV_Label);
		((XUiC_SimpleButton)base.GetChildById("btnCancel")).OnPressed += this.BtnCancel_OnPressed;
		((XUiC_SimpleButton)base.GetChildById("btnNewBinding")).OnPressed += this.BtnNewBinding_OnPressed;
		((XUiC_SimpleButton)base.GetChildById("btnGrabBinding")).OnPressed += this.BtnGrabBinding_OnPressed;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCancel_OnPressed(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(this.windowGroup.ID);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnNewBinding_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.startGetBinding();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnGrabBinding_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.conflictingAction.ClearInputState();
		this.conflictingAction.RemoveBinding(this.binding);
		this.action.UnbindBindingsOfType(this.forController);
		this.action.AddBinding(this.binding);
		ThreadManager.StartCoroutine(this.closeNextFrame());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void startGetBinding()
	{
		base.xui.playerUI.windowManager.IsInputLocked = true;
		InputUtils.EnableAllPlayerActions(false);
		GameManager.Instance.SetCursorEnabledOverride(true, false);
		this.rectInUse.IsVisible = false;
		this.conflictingAction = null;
		this.binding = null;
		this.action.Owner.ListenOptions.IncludeUnknownControllers = false;
		this.action.Owner.ListenOptions.IncludeMouseButtons = !this.forController;
		this.action.Owner.ListenOptions.IncludeMouseScrollWheel = !this.forController;
		this.action.Owner.ListenOptions.IncludeKeys = true;
		this.action.Owner.ListenOptions.OnBindingFound = new Func<PlayerAction, BindingSource, bool>(this.onBindingReceived);
		this.action.ListenForBinding();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void stoppedGetBinding(bool _alsoStopListening = true)
	{
		if (_alsoStopListening)
		{
			this.action.StopListeningForBinding();
		}
		InputUtils.EnableAllPlayerActions(true);
		base.xui.playerUI.windowManager.IsInputLocked = false;
		GameManager.Instance.SetCursorEnabledOverride(false, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool onBindingReceived(PlayerAction _action, BindingSource _binding)
	{
		if (this.bindingAbortActions.Contains(_binding))
		{
			Log.Out("Abort action pressed, aborting listening for new binding for {0}", new object[]
			{
				_action.Name
			});
			this.stoppedGetBinding(true);
			ThreadManager.StartCoroutine(this.closeNextFrame());
			return false;
		}
		if (this.forController && (_binding is KeyBindingSource || _binding is MouseBindingSource))
		{
			Log.Out("Cannot accept key or mouse for controller binding");
			return false;
		}
		if (!this.forController && _binding is DeviceBindingSource)
		{
			Log.Out("Cannot accept device binding source for keyboard/mouse binding");
			return false;
		}
		foreach (BindingSource b in this.bindingForbidden)
		{
			if (_binding == b)
			{
				Log.Out("Binding {0} not allowed", new object[]
				{
					_binding.Name
				});
				return false;
			}
		}
		if (this.forController != (_binding.BindingSourceType == BindingSourceType.DeviceBindingSource))
		{
			Log.Out("New binding ({0}) doesn't match expected input device type ({1})", new object[]
			{
				_binding.BindingSourceType.ToStringCached<BindingSourceType>(),
				this.forController ? BindingSourceType.DeviceBindingSource.ToStringCached<BindingSourceType>() : BindingSourceType.KeyBindingSource.ToStringCached<BindingSourceType>()
			});
			return false;
		}
		if (_action.HasBinding(_binding))
		{
			Log.Out("Binding {0} already bound to the current action {1}", new object[]
			{
				_binding.Name,
				_action.Name
			});
			this.stoppedGetBinding(true);
			ThreadManager.StartCoroutine(this.closeNextFrame());
			return false;
		}
		if (this.alreadyBound(_binding, _action))
		{
			Log.Out("Binding {0} already bound to the action {1}", new object[]
			{
				_binding.Name,
				this.conflictingAction.Name
			});
			this.stoppedGetBinding(true);
			return false;
		}
		_action.UnbindBindingsOfType(this.forController);
		this.stoppedGetBinding(false);
		ThreadManager.StartCoroutine(this.closeNextFrame());
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool alreadyBound(BindingSource _binding, PlayerAction _selfAction)
	{
		PlayerAction playerAction = _selfAction.Owner.BindingUsed(_binding);
		if (playerAction == null && _selfAction.Owner.UserData != null)
		{
			PlayerActionsBase[] bindingsConflictWithSet = ((PlayerActionData.ActionSetUserData)_selfAction.Owner.UserData).bindingsConflictWithSet;
			for (int i = 0; i < bindingsConflictWithSet.Length; i++)
			{
				playerAction = bindingsConflictWithSet[i].BindingUsed(_binding);
				if (playerAction != null)
				{
					break;
				}
			}
		}
		if (playerAction != null)
		{
			PlayerActionData.ActionUserData actionUserData = _selfAction.UserData as PlayerActionData.ActionUserData;
			PlayerActionData.ActionUserData actionUserData2 = playerAction.UserData as PlayerActionData.ActionUserData;
			if (actionUserData.allowMultipleBindings || actionUserData2.allowMultipleBindings)
			{
				PlayerActionSet playerActionSet = base.xui.playerUI.playerInput;
				if (!base.xui.playerUI.playerInput.Actions.Contains(_selfAction) || !base.xui.playerUI.playerInput.Actions.Contains(playerAction))
				{
					if (base.xui.playerUI.playerInput.GUIActions.Actions.Contains(_selfAction) && base.xui.playerUI.playerInput.GUIActions.Actions.Contains(playerAction))
					{
						playerActionSet = base.xui.playerUI.playerInput.GUIActions;
					}
					else if (base.xui.playerUI.playerInput.VehicleActions.Actions.Contains(_selfAction) && base.xui.playerUI.playerInput.VehicleActions.Actions.Contains(playerAction))
					{
						playerActionSet = base.xui.playerUI.playerInput.VehicleActions;
					}
					else
					{
						if (!base.xui.playerUI.playerInput.PermanentActions.Actions.Contains(_selfAction) || !base.xui.playerUI.playerInput.PermanentActions.Actions.Contains(playerAction))
						{
							return false;
						}
						playerActionSet = base.xui.playerUI.playerInput.PermanentActions;
					}
				}
				bool flag = false;
				foreach (PlayerAction playerAction2 in playerActionSet.Actions)
				{
					if (playerAction2 != playerAction && playerAction2 != _selfAction && playerAction2.Bindings.Contains(_binding))
					{
						flag = true;
						playerAction = playerAction2;
						break;
					}
				}
				if (!flag)
				{
					return false;
				}
			}
			this.rectInUse.IsVisible = true;
			this.binding = playerAction.GetBindingOfType(this.forController);
			this.conflictingAction = playerAction;
			if (this.forController)
			{
				this.lblInUseBy.Text = string.Format(Localization.Get("xuiNewBindingConflictingAction_Controller", false), playerAction.GetBindingString(true, PlatformManager.NativePlatform.Input.CurrentControllerInputStyle, XUiUtils.EmptyBindingStyle.LocalizedNone, XUiUtils.DisplayStyle.Plain, false, null), ((PlayerActionData.ActionUserData)playerAction.UserData).LocalizedName);
			}
			else
			{
				this.lblInUseBy.Text = string.Format(Localization.Get("xuiNewBindingConflictingAction", false), playerAction.GetBindingString(this.forController, PlayerInputManager.InputStyle.Undefined, XUiUtils.EmptyBindingStyle.LocalizedNone, XUiUtils.DisplayStyle.Plain, false, null), ((PlayerActionData.ActionUserData)playerAction.UserData).LocalizedName);
			}
			base.GetChildById("btnNewBinding").SelectCursorElement(true, false);
			this.lblInUseBy.ToolTip = ((PlayerActionData.ActionUserData)playerAction.UserData).LocalizedDescription;
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator closeNextFrame()
	{
		yield return null;
		base.xui.playerUI.windowManager.Close(this.windowGroup.ID);
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator startNextFrame()
	{
		yield return null;
		this.startGetBinding();
		yield break;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (this.forController)
		{
			this.lblAction.Text = string.Format(Localization.Get("xuiNewBindingCurrent_Controller", false), ((PlayerActionData.ActionUserData)this.action.UserData).LocalizedName, this.action.GetBindingString(true, PlatformManager.NativePlatform.Input.CurrentControllerInputStyle, XUiUtils.EmptyBindingStyle.LocalizedNone, XUiUtils.DisplayStyle.Plain, false, null));
		}
		else
		{
			this.lblAction.Text = string.Format(Localization.Get("xuiNewBindingCurrent", false), ((PlayerActionData.ActionUserData)this.action.UserData).LocalizedName, this.action.GetBindingString(this.forController, PlayerInputManager.InputStyle.Undefined, XUiUtils.EmptyBindingStyle.LocalizedNone, XUiUtils.DisplayStyle.Plain, false, null));
		}
		string text;
		if (this.forController)
		{
			string arg;
			if (PlatformManager.NativePlatform.Input.CurrentControllerInputStyle == PlayerInputManager.InputStyle.PS4)
			{
				arg = "[sp=PS5_Button_Options] / ESC";
			}
			else
			{
				arg = "[sp=XB_Button_Back] / ESC";
			}
			text = string.Format(Localization.Get("xuiNewBindingAbort_Controller", false), arg);
		}
		else
		{
			string arg2;
			if (PlatformManager.NativePlatform.Input.CurrentControllerInputStyle == PlayerInputManager.InputStyle.PS4)
			{
				arg2 = "ESC / [sp=PS5_Button_Options]";
			}
			else
			{
				arg2 = "ESC / [sp=XB_Button_Back]";
			}
			text = string.Format(Localization.Get("xuiNewBindingAbort_Controller", false), arg2);
		}
		this.lblAbort.Text = text;
		ThreadManager.StartCoroutine(this.startNextFrame());
	}

	public override void OnClose()
	{
		base.OnClose();
		base.xui.playerUI.windowManager.Open(this.windowToOpen, true, false, true);
	}

	public static void GetNewBinding(XUi _xuiInstance, PlayerAction _action, string _windowToOpen, bool _forController = false)
	{
		XUiC_OptionsControlsNewBinding childByType = _xuiInstance.FindWindowGroupByName(XUiC_OptionsControlsNewBinding.ID).GetChildByType<XUiC_OptionsControlsNewBinding>();
		childByType.action = _action;
		childByType.windowToOpen = _windowToOpen;
		childByType.forController = _forController;
		_xuiInstance.playerUI.windowManager.Open(childByType.WindowGroup.ID, true, false, true);
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblAction;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblAbort;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Rect rectInUse;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblInUseBy;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlayerAction action;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlayerAction conflictingAction;

	[PublicizedFrom(EAccessModifier.Private)]
	public BindingSource binding;

	[PublicizedFrom(EAccessModifier.Private)]
	public string windowToOpen;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool forController;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<BindingSource> bindingAbortActions = new List<BindingSource>
	{
		new DeviceBindingSource(InputControlType.Back),
		new DeviceBindingSource(InputControlType.Options),
		new DeviceBindingSource(InputControlType.View),
		new DeviceBindingSource(InputControlType.Minus),
		new KeyBindingSource(new Key[]
		{
			Key.Escape
		})
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly BindingSource[] bindingForbidden = new BindingSource[]
	{
		new KeyBindingSource(new Key[]
		{
			Key.F1
		}),
		new KeyBindingSource(new Key[]
		{
			Key.F2
		}),
		new KeyBindingSource(new Key[]
		{
			Key.F3
		}),
		new KeyBindingSource(new Key[]
		{
			Key.F4
		}),
		new KeyBindingSource(new Key[]
		{
			Key.F5
		}),
		new KeyBindingSource(new Key[]
		{
			Key.F6
		}),
		new KeyBindingSource(new Key[]
		{
			Key.F7
		}),
		new KeyBindingSource(new Key[]
		{
			Key.F8
		}),
		new KeyBindingSource(new Key[]
		{
			Key.F9
		}),
		new KeyBindingSource(new Key[]
		{
			Key.F10
		}),
		new KeyBindingSource(new Key[]
		{
			Key.F11
		}),
		new KeyBindingSource(new Key[]
		{
			Key.F12
		}),
		new DeviceBindingSource(InputControlType.Start),
		new DeviceBindingSource(InputControlType.Back),
		new DeviceBindingSource(InputControlType.LeftStickUp),
		new DeviceBindingSource(InputControlType.LeftStickDown),
		new DeviceBindingSource(InputControlType.LeftStickLeft),
		new DeviceBindingSource(InputControlType.LeftStickRight),
		new DeviceBindingSource(InputControlType.RightStickUp),
		new DeviceBindingSource(InputControlType.RightStickDown),
		new DeviceBindingSource(InputControlType.RightStickLeft),
		new DeviceBindingSource(InputControlType.RightStickRight),
		new DeviceBindingSource(InputControlType.Share),
		new DeviceBindingSource(InputControlType.Menu),
		new DeviceBindingSource(InputControlType.View),
		new DeviceBindingSource(InputControlType.Options),
		new DeviceBindingSource(InputControlType.Plus),
		new DeviceBindingSource(InputControlType.Minus),
		new DeviceBindingSource(InputControlType.TouchPadButton),
		new DeviceBindingSource(InputControlType.Select),
		new DeviceBindingSource(InputControlType.LeftStickY),
		new DeviceBindingSource(InputControlType.LeftStickX),
		new DeviceBindingSource(InputControlType.RightStickY),
		new DeviceBindingSource(InputControlType.RightStickX),
		new DeviceBindingSource(InputControlType.Create),
		new DeviceBindingSource(InputControlType.Guide),
		new DeviceBindingSource(InputControlType.Home),
		new DeviceBindingSource(InputControlType.Mute),
		new DeviceBindingSource(InputControlType.Capture)
	};
}

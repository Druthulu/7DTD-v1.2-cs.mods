using System;
using System.Collections;
using InControl;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[UnityEngine.Scripting.Preserve]
public class XUiC_TextInput : XUiController
{
	public event XUiEvent_InputOnSubmitEventHandler OnSubmitHandler;

	public event XUiEvent_InputOnChangedEventHandler OnChangeHandler;

	public event XUiEvent_InputOnAbortedEventHandler OnInputAbortedHandler;

	public event XUiEvent_InputOnSelectedEventHandler OnInputSelectedHandler;

	public event XUiEvent_InputOnErrorEventHandler OnInputErrorHandler;

	public event UIInput.OnClipboard OnClipboardHandler;

	public static void SelectCurrentSearchField(LocalPlayerUI _playerUi)
	{
		if (XUiC_TextInput.currentSearchField != null && !_playerUi.windowManager.IsInputActive() && XUiC_TextInput.currentSearchField.viewComponent.UiTransform.gameObject.activeInHierarchy)
		{
			XUiC_TextInput.currentSearchField.SetSelected(true, false);
		}
	}

	public UIInput UIInput
	{
		get
		{
			return this.uiInput;
		}
	}

	public string Text
	{
		get
		{
			return this.uiInput.value;
		}
		set
		{
			this.textChangeFromCode = true;
			this.uiInput.value = value;
			this.textChangeFromCode = false;
			this.uiInput.UpdateLabel();
		}
	}

	public XUiC_TextInput SelectOnTab
	{
		get
		{
			return this.selectOnTab;
		}
		set
		{
			if (this.selectOnTab != value)
			{
				this.selectOnTab = value;
				if (value != null)
				{
					UIKeyNavigation uikeyNavigation = base.ViewComponent.UiTransform.gameObject.AddMissingComponent<UIKeyNavigation>();
					uikeyNavigation.constraint = UIKeyNavigation.Constraint.Explicit;
					uikeyNavigation.onTab = value.uiInput.gameObject;
					return;
				}
				UIKeyNavigation uikeyNavigation2 = base.ViewComponent.UiTransform.gameObject.AddMissingComponent<UIKeyNavigation>();
				if (uikeyNavigation2 != null)
				{
					UnityEngine.Object.Destroy(uikeyNavigation2);
				}
			}
		}
	}

	public Color ActiveTextColor
	{
		get
		{
			return this.activeTextColor;
		}
		set
		{
			if (value != this.activeTextColor)
			{
				this.activeTextColor = value;
				this.IsDirty = true;
			}
		}
	}

	public bool Enabled
	{
		get
		{
			return this.uiInput.enabled;
		}
		set
		{
			this.uiInput.enabled = value;
		}
	}

	public bool SupportBbCode
	{
		get
		{
			return ((XUiV_Label)this.text.ViewComponent).SupportBbCode;
		}
		set
		{
			((XUiV_Label)this.text.ViewComponent).SupportBbCode = value;
		}
	}

	public bool IsSelected
	{
		get
		{
			return this.uiInput.isSelected;
		}
	}

	public override void Init()
	{
		XUiV_Panel xuiV_Panel = this.viewComponent as XUiV_Panel;
		if (xuiV_Panel != null)
		{
			xuiV_Panel.createUiPanel = true;
		}
		base.Init();
		this.text = base.GetChildById("text");
		Transform uiTransform = base.ViewComponent.UiTransform;
		uiTransform.gameObject.GetOrAddComponent<BoxCollider>();
		base.ViewComponent.RefreshBoxCollider();
		this.uiInput = uiTransform.gameObject.AddComponent<UIInput>();
		XUiController childById = base.GetChildById("btnShowPassword");
		if (childById != null)
		{
			childById.OnPress += this.BtnShowPassword_OnPress;
		}
		XUiController childById2 = base.GetChildById("btnClearInput");
		if (childById2 != null)
		{
			childById2.OnPress += this.BtnClearInput_OnPress;
		}
		EventDelegate.Add(this.uiInput.onSubmit, new EventDelegate.Callback(this.OnSubmit));
		EventDelegate.Add(this.uiInput.onChange, new EventDelegate.Callback(this.OnChange));
		this.uiInput.onClipboard += this.OnClipboard;
		this.uiInput.label = ((XUiV_Label)this.text.ViewComponent).Label;
		if (this.value != null)
		{
			this.uiInput.value = this.value;
		}
		else
		{
			this.uiInput.value = "";
		}
		this.uiInput.activeTextColor = this.activeTextColor;
		this.uiInput.caretColor = this.caretColor;
		this.uiInput.selectionColor = this.selectionColor;
		this.uiInput.inputType = this.displayType;
		this.uiInput.validation = this.validation;
		this.uiInput.hideInput = this.hideInput;
		this.uiInput.onReturnKey = this.onReturnKey;
		this.uiInput.characterLimit = this.characterLimit;
		this.uiInput.keyboardType = this.inputType;
		base.OnSelect += this.InputFieldSelected;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnShowPassword_OnPress(XUiController _sender, int _mouseButton)
	{
		this.displayType = ((this.displayType == UIInput.InputType.Password) ? UIInput.InputType.Standard : UIInput.InputType.Password);
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnClearInput_OnPress(XUiController _sender, int _mouseButton)
	{
		this.Text = "";
		this.IsDirty = true;
	}

	public void ShowVirtualKeyboard()
	{
		this.XUiC_TextInput_OnPress(this, -1);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InputFieldSelected(XUiController _sender, bool _selected)
	{
		if (_selected)
		{
			ThreadManager.StartCoroutine(this.removeSelection());
			return;
		}
		if (this.OnInputSelectedHandler != null)
		{
			this.OnInputSelectedHandler(this, false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator removeSelection()
	{
		yield return null;
		if (PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard)
		{
			this.SetSelected(false, false);
			this.XUiC_TextInput_OnPress(this, -1);
		}
		if (this.OnInputSelectedHandler != null)
		{
			this.OnInputSelectedHandler(this, true);
		}
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void XUiC_TextInput_OnPress(XUiController _sender, int _mouseButton)
	{
		if (!this.Enabled)
		{
			return;
		}
		IVirtualKeyboard virtualKeyboard = PlatformManager.NativePlatform.VirtualKeyboard;
		string text;
		if (virtualKeyboard == null)
		{
			text = Localization.Get("ttPlatformHasNoVirtualKeyboard", false);
		}
		else
		{
			text = virtualKeyboard.Open(this.virtKeyboardPrompt, this.uiInput.value, new Action<bool, string>(this.OnTextReceived), this.displayType, this.onReturnKey == UIInput.OnReturnKey.NewLine);
		}
		if (text != null)
		{
			GameManager instance = GameManager.Instance;
			EntityPlayerLocal player;
			if (instance == null)
			{
				player = null;
			}
			else
			{
				World world = instance.World;
				player = ((world != null) ? world.GetPrimaryPlayer() : null);
			}
			GameManager.ShowTooltip(player, "[BB0000]" + text, false);
			if (this.OnInputErrorHandler != null)
			{
				this.OnInputErrorHandler(this, text);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnTextReceived(bool _success, string _text)
	{
		if (this.isOpen)
		{
			this.Text = _text;
			if (_success)
			{
				if (this.OnSubmitHandler != null)
				{
					this.OnSubmitHandler(this, _text);
				}
				else if (this.OnChangeHandler != null)
				{
					this.OnChangeHandler(this, _text, false);
				}
			}
			else if (this.OnInputAbortedHandler != null)
			{
				this.OnInputAbortedHandler(this);
			}
			this.uiInput.RemoveFocus();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnSubmit()
	{
		string input = UIInput.current.value;
		this.uiInput.RemoveFocus();
		if (this.OnSubmitHandler != null)
		{
			ThreadManager.StartCoroutine(this.delaySubmitHandler(input));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator delaySubmitHandler(string _input)
	{
		yield return null;
		XUiEvent_InputOnSubmitEventHandler onSubmitHandler = this.OnSubmitHandler;
		if (onSubmitHandler != null)
		{
			onSubmitHandler(this, _input);
		}
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnChange()
	{
		XUiEvent_InputOnChangedEventHandler onChangeHandler = this.OnChangeHandler;
		if (onChangeHandler == null)
		{
			return;
		}
		onChangeHandler(this, UIInput.current.value, this.textChangeFromCode);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnClipboard(UIInput.ClipboardAction _actiontype, string _oldtext, int _selstart, int _selend, string _actionresulttext)
	{
		UIInput.OnClipboard onClipboardHandler = this.OnClipboardHandler;
		if (onClipboardHandler == null)
		{
			return;
		}
		onClipboardHandler(_actiontype, _oldtext, _selstart, _selend, _actionresulttext);
	}

	public override bool ParseAttribute(string name, string value, XUiController _parent)
	{
		uint num = <PrivateImplementationDetails>.ComputeStringHash(name);
		if (num <= 2346532384U)
		{
			if (num <= 679658536U)
			{
				if (num <= 94319076U)
				{
					if (num != 38871051U)
					{
						if (num == 94319076U)
						{
							if (name == "input_type")
							{
								this.inputType = EnumUtils.Parse<UIInput.KeyboardType>(value, true);
							}
						}
					}
					else if (name == "clear_on_open")
					{
						this.clearOnOpen = StringParsers.ParseBool(value, 0, -1, true);
					}
				}
				else if (num != 226467462U)
				{
					if (num == 679658536U)
					{
						if (name == "caret_color")
						{
							this.caretColor = StringParsers.ParseColor32(value);
						}
					}
				}
				else if (name == "search_field")
				{
					this.isSearchField = StringParsers.ParseBool(value, 0, -1, true);
				}
			}
			else if (num <= 1114941561U)
			{
				if (num != 1113510858U)
				{
					if (num == 1114941561U)
					{
						if (name == "close_group_on_tab")
						{
							this.closeGroupOnTab = StringParsers.ParseBool(value, 0, -1, true);
						}
					}
				}
				else if (name == "value")
				{
					this.value = value;
				}
			}
			else if (num != 1682030439U)
			{
				if (num != 2196932415U)
				{
					if (num == 2346532384U)
					{
						if (name == "open_vk_on_open")
						{
							this.openVKOnOpen = StringParsers.ParseBool(value, 0, -1, true);
						}
					}
				}
				else if (name == "active_text_color")
				{
					this.ActiveTextColor = StringParsers.ParseColor32(value);
				}
			}
			else if (name == "password_field")
			{
				this.isPasswordField = StringParsers.ParseBool(value, 0, -1, true);
			}
		}
		else if (num <= 3198318611U)
		{
			if (num <= 2431563478U)
			{
				if (num != 2408036522U)
				{
					if (num == 2431563478U)
					{
						if (name == "focus_on_open")
						{
							this.focusOnOpen = StringParsers.ParseBool(value, 0, -1, true);
						}
					}
				}
				else if (name == "hide_input")
				{
					this.hideInput = StringParsers.ParseBool(value, 0, -1, true);
				}
			}
			else if (num != 2509030743U)
			{
				if (num == 3198318611U)
				{
					if (name == "virtual_keyboard_prompt")
					{
						this.virtKeyboardPrompt = Localization.Get(value, false);
					}
				}
			}
			else if (name == "clear_button")
			{
				this.hasClearButton = StringParsers.ParseBool(value, 0, -1, true);
			}
		}
		else if (num <= 3827651548U)
		{
			if (num != 3378670817U)
			{
				if (num == 3827651548U)
				{
					if (name == "use_virtual_keyboard")
					{
						this.useVirtualKeyboard = StringParsers.ParseBool(value, 0, -1, true);
					}
				}
			}
			else if (name == "selection_color")
			{
				this.selectionColor = StringParsers.ParseColor32(value);
			}
		}
		else if (num != 4029825831U)
		{
			if (num != 4061606266U)
			{
				if (num == 4181838876U)
				{
					if (name == "character_limit")
					{
						this.characterLimit = int.Parse(value);
					}
				}
			}
			else if (name == "validation")
			{
				this.validation = EnumUtils.Parse<UIInput.Validation>(value, true);
			}
		}
		else if (name == "on_return")
		{
			this.onReturnKey = EnumUtils.Parse<UIInput.OnReturnKey>(value, true);
		}
		return base.ParseAttribute(name, value, _parent);
	}

	public override bool GetBindingValue(ref string _value, string _bindingName)
	{
		if (_bindingName == "clearbutton")
		{
			_value = this.hasClearButton.ToString();
			return true;
		}
		if (_bindingName == "passwordfield")
		{
			_value = this.isPasswordField.ToString();
			return true;
		}
		if (!(_bindingName == "showpassword"))
		{
			return base.GetBindingValue(ref _value, _bindingName);
		}
		_value = (this.displayType == UIInput.InputType.Standard).ToString();
		return true;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		((XUiV_Label)this.text.ViewComponent).Text = this.uiInput.value;
		this.IsDirty = true;
		this.isOpen = true;
		this.openCompleted = false;
		this.removeFocusOnOpen = true;
		if (this.isPasswordField)
		{
			this.displayType = UIInput.InputType.Password;
			this.uiInput.UpdateLabel();
		}
		if (this.isSearchField)
		{
			XUiC_TextInput.currentSearchField = this;
		}
		if (this.clearOnOpen)
		{
			this.Text = "";
		}
		if (this.focusOnOpen)
		{
			this.SetSelected(true, true);
		}
		if (this.openVKOnOpen)
		{
			this.SelectOrVirtualKeyboard(true);
		}
	}

	public override void OnClose()
	{
		this.SetSelected(false, false);
		base.OnClose();
		this.isOpen = false;
		if (XUiC_TextInput.currentSearchField == this)
		{
			XUiC_TextInput.currentSearchField = null;
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (base.xui.playerUI.CursorController.GetMouseButton(UICamera.MouseButton.LeftButton) && this.uiInput.isSelected && UICamera.hoveredObject != base.ViewComponent.UiTransform.gameObject)
		{
			this.uiInput.isSelected = false;
		}
		if (this.closeGroupOnTab && this.uiInput.isSelected && this.selectOnTab == null)
		{
			PlayerAction inventory = base.xui.playerUI.playerInput.PermanentActions.Inventory;
			KeyBindingSource keyBindingSource = inventory.GetBindingOfType(false) as KeyBindingSource;
			if (inventory.WasPressed && keyBindingSource != null && keyBindingSource.Control == XUiC_TextInput.tabCombo)
			{
				ThreadManager.StartCoroutine(this.closeOnTabLater());
			}
		}
		if (this.IsDirty)
		{
			this.uiInput.inputType = this.displayType;
			this.uiInput.activeTextColor = this.activeTextColor;
			this.uiInput.UpdateLabel();
			if (!this.openCompleted && this.removeFocusOnOpen)
			{
				this.uiInput.RemoveFocus();
			}
			this.IsDirty = false;
			base.RefreshBindings(false);
			if (!this.openCompleted)
			{
				this.openCompleted = true;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator closeOnTabLater()
	{
		PlayerAction inventoryAction = base.xui.playerUI.playerInput.PermanentActions.Inventory;
		KeyBindingSource keyBindingSource = inventoryAction.GetBindingOfType(false) as KeyBindingSource;
		if (keyBindingSource == null || keyBindingSource.Control != XUiC_TextInput.tabCombo)
		{
			yield break;
		}
		while (inventoryAction.IsPressed)
		{
			yield return null;
		}
		if (this.closeGroupOnTab && this.uiInput.isSelected && this.selectOnTab == null)
		{
			base.xui.playerUI.windowManager.Close(this.windowGroup, false);
		}
		yield break;
	}

	public void SetSelected(bool _selected = true, bool _delayed = false)
	{
		ThreadManager.StartCoroutine(this.setSelectedDelayed(_selected, _delayed));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator setSelectedDelayed(bool _selected, bool _delayed)
	{
		if (_delayed)
		{
			yield return null;
		}
		if (!_selected || PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard)
		{
			this.uiInput.isSelected = _selected;
			if (!this.openCompleted)
			{
				this.removeFocusOnOpen = false;
			}
		}
		yield break;
	}

	public void SelectOrVirtualKeyboard(bool _delayed = false)
	{
		if (PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard)
		{
			this.SetSelected(true, _delayed);
			return;
		}
		this.ShowVirtualKeyboard();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController text;

	[PublicizedFrom(EAccessModifier.Private)]
	public UIInput uiInput;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput selectOnTab;

	[PublicizedFrom(EAccessModifier.Private)]
	public string value;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color activeTextColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color caretColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color selectionColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public UIInput.InputType displayType;

	[PublicizedFrom(EAccessModifier.Private)]
	public UIInput.Validation validation;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hideInput;

	[PublicizedFrom(EAccessModifier.Private)]
	public UIInput.OnReturnKey onReturnKey = UIInput.OnReturnKey.Submit;

	[PublicizedFrom(EAccessModifier.Private)]
	public int characterLimit;

	[PublicizedFrom(EAccessModifier.Private)]
	public UIInput.KeyboardType inputType;

	public bool useVirtualKeyboard;

	[PublicizedFrom(EAccessModifier.Private)]
	public string virtKeyboardPrompt = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isOpen;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool openCompleted;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool removeFocusOnOpen = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isSearchField;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasClearButton;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isPasswordField;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool focusOnOpen;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool openVKOnOpen;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool clearOnOpen;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly KeyCombo tabCombo = new KeyCombo(new Key[]
	{
		Key.Tab
	});

	[PublicizedFrom(EAccessModifier.Private)]
	public bool closeGroupOnTab;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool textChangeFromCode;

	[PublicizedFrom(EAccessModifier.Private)]
	public static XUiC_TextInput currentSearchField;
}

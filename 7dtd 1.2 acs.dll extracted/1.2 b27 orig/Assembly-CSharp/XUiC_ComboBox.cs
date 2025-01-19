using System;
using System.Collections.Generic;
using System.Globalization;
using InControl;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[UnityEngine.Scripting.Preserve]
public abstract class XUiC_ComboBox<TValue> : XUiController
{
	public event XUiC_ComboBox<TValue>.XUiEvent_ValueChanged OnValueChanged;

	public event XUiC_ComboBox<TValue>.XUiEvent_GenericValueChanged OnValueChangedGeneric;

	public bool Enabled
	{
		get
		{
			return this.enabled;
		}
		set
		{
			if (value != this.enabled)
			{
				this.enabled = value;
				this.IsDirty = true;
			}
		}
	}

	public Color TextColor
	{
		get
		{
			return this.colorEnabled;
		}
		set
		{
			if (this.colorEnabled != value)
			{
				this.colorEnabled = value;
				this.IsDirty = true;
			}
		}
	}

	public string ValueText
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return this.valueText;
		}
		[PublicizedFrom(EAccessModifier.Protected)]
		set
		{
			if (this.valueText != value)
			{
				this.valueText = value;
				this.IsDirty = true;
			}
		}
	}

	public abstract TValue Value { get; set; }

	public abstract int IndexElementCount { [PublicizedFrom(EAccessModifier.Protected)] get; }

	public abstract int IndexMarkerIndex { [PublicizedFrom(EAccessModifier.Protected)] get; }

	public virtual bool UsesIndexMarkers
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return this.indexMarkers && this.sprFill != null && this.IndexElementCount > 0 && this.IndexElementCount < this.IndexMarkerSprites.Count;
		}
	}

	public override void Init()
	{
		base.Init();
		XUiController childById = base.GetChildById("forward");
		if (childById != null)
		{
			childById.OnPress += this.ForwardButton_OnPress;
			this.forwardButton = (childById.ViewComponent as XUiV_Button);
		}
		XUiController childById2 = base.GetChildById("back");
		if (childById2 != null)
		{
			childById2.OnPress += this.BackButton_OnPress;
			this.backButton = (childById2.ViewComponent as XUiV_Button);
		}
		XUiController childById3 = base.GetChildById("fill");
		if (childById3 != null)
		{
			XUiV_Sprite xuiV_Sprite = childById3.ViewComponent as XUiV_Sprite;
			if (xuiV_Sprite != null)
			{
				this.sprFill = xuiV_Sprite;
			}
		}
		XUiController childById4 = base.GetChildById("indexMarkers");
		if (childById4 != null)
		{
			string input;
			if (childById4.CustomAttributes.TryGetValue("active_color", out input))
			{
				this.colorIndexMarkerActive = StringParsers.ParseColor32(input);
			}
			string input2;
			if (childById4.CustomAttributes.TryGetValue("inactive_color", out input2))
			{
				this.colorIndexMarkerInactive = StringParsers.ParseColor32(input2);
			}
			for (int i = 0; i < childById4.Children.Count; i++)
			{
				XUiV_Sprite xuiV_Sprite2 = childById4.Children[i].ViewComponent as XUiV_Sprite;
				if (xuiV_Sprite2 != null)
				{
					this.IndexMarkerSprites.Add(xuiV_Sprite2);
				}
			}
		}
		this.InitSegmentedFillPositions();
		this.clickable = base.GetChildById("directvalue");
		if (this.clickable != null)
		{
			this.clickable.OnPress += this.PressEvent;
			this.clickable.OnScroll += this.ScrollEvent;
			this.clickable.OnDrag += this.DragEvent;
			this.clickable.OnHover += this.HoverEvent;
		}
		if (string.IsNullOrEmpty(this.viewComponent.ToolTip) && base.Parent != null)
		{
			foreach (XUiController xuiController in base.Parent.Children)
			{
				XUiV_Label xuiV_Label = xuiController.ViewComponent as XUiV_Label;
				if (xuiV_Label != null)
				{
					this.viewComponent.ToolTip = xuiV_Label.ToolTip;
					break;
				}
			}
		}
	}

	public override void OnOpen()
	{
		this.IsDirty = true;
		base.OnOpen();
		this.UpdateLabel();
		this.UpdateIndexMarkerPositions();
		this.UpdateIndexMarkerStates();
		base.RefreshBindings(false);
	}

	public override void OnClose()
	{
		base.OnClose();
		if (this.isOver)
		{
			base.xui.calloutWindow.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuComboBox);
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.enabled && this.clickable != null && base.xui.playerUI.CursorController.CurrentTarget == this.clickable.ViewComponent && !base.xui.playerUI.CursorController.Locked)
		{
			XUi.HandlePaging(base.xui, new Action(this.PageUpAction), new Action(this.PageDownAction), false);
		}
		if (this.enabled && PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard)
		{
			this.forwardButton.CurrentColor = (this.isOver ? this.forwardButton.HoverSpriteColor : this.forwardButton.DefaultSpriteColor);
			this.backButton.CurrentColor = (this.isOver ? this.backButton.HoverSpriteColor : this.backButton.DefaultSpriteColor);
		}
		if (this.gamepadDecreaseShortcut != null && this.gamepadDecreaseShortcut.WasPressed)
		{
			this.PageDownAction();
		}
		if (this.gamepadIncreaseShortcut != null && this.gamepadIncreaseShortcut.WasPressed)
		{
			this.PageUpAction();
		}
		if (this.IsDirty)
		{
			this.IsDirty = false;
			base.RefreshBindings(false);
		}
	}

	public override bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
		uint num = <PrivateImplementationDetails>.ComputeStringHash(_name);
		if (num <= 2099093595U)
		{
			if (num <= 1464747222U)
			{
				if (num != 470464957U)
				{
					if (num != 575017818U)
					{
						if (num == 1464747222U)
						{
							if (_name == "enabled_fill_color")
							{
								this.colorFillEnabled = StringParsers.ParseColor32(_value);
								return true;
							}
						}
					}
					else if (_name == "bg_color")
					{
						this.colorBg = StringParsers.ParseColor32(_value);
						return true;
					}
				}
				else if (_name == "gamepad_increase")
				{
					this.gamepadIncreaseShortcut = base.xui.playerUI.playerInput.GUIActions.GetPlayerActionByName(_value);
					return true;
				}
			}
			else if (num != 1490568401U)
			{
				if (num != 1871259129U)
				{
					if (num == 2099093595U)
					{
						if (_name == "disabled_fill_color")
						{
							this.colorFillDisabled = StringParsers.ParseColor32(_value);
							return true;
						}
					}
				}
				else if (_name == "segmented_fill")
				{
					this.UsesSegmentedFill = StringParsers.ParseBool(_value, 0, -1, true);
					return true;
				}
			}
			else if (_name == "gamepad_decrease")
			{
				this.gamepadDecreaseShortcut = base.xui.playerUI.playerInput.GUIActions.GetPlayerActionByName(_value);
				return true;
			}
		}
		else if (num <= 2643151247U)
		{
			if (num != 2185908414U)
			{
				if (num != 2349648791U)
				{
					if (num == 2643151247U)
					{
						if (_name == "index_markers")
						{
							this.indexMarkers = StringParsers.ParseBool(_value, 0, -1, true);
							return true;
						}
					}
				}
				else if (_name == "value_wrap")
				{
					if (!_value.EqualsCaseInsensitive("@def"))
					{
						this.Wrap = StringParsers.ParseBool(_value, 0, -1, true);
					}
					return true;
				}
			}
			else if (_name == "scroll_by_increment")
			{
				this.ScrollByIncrement = StringParsers.ParseBool(_value, 0, -1, true);
				return true;
			}
		}
		else if (num != 3262104480U)
		{
			if (num != 3868148786U)
			{
				if (num == 4076031121U)
				{
					if (_name == "disabled_color")
					{
						this.colorDisabled = StringParsers.ParseColor32(_value);
						return true;
					}
				}
			}
			else if (_name == "enabled_color")
			{
				this.colorEnabled = StringParsers.ParseColor32(_value);
				return true;
			}
		}
		else if (_name == "segment_spacing")
		{
			this.SegmentedFillSpacing = StringParsers.ParseSInt32(_value, 0, -1, NumberStyles.Integer);
			return true;
		}
		return base.ParseAttribute(_name, _value, _parent);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract void UpdateLabel();

	[PublicizedFrom(EAccessModifier.Protected)]
	public void UpdateIndexMarkerStates()
	{
		if (this.UsesIndexMarkers)
		{
			for (int i = 0; i < this.IndexMarkerSprites.Count; i++)
			{
				this.IndexMarkerSprites[i].Color = this.colorIndexMarkerInactive;
			}
			if (this.IndexMarkerIndex >= 0 && this.IndexMarkerIndex < this.IndexMarkerSprites.Count)
			{
				this.IndexMarkerSprites[this.IndexMarkerIndex].Color = this.colorIndexMarkerActive;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void UpdateIndexMarkerPositions()
	{
		if (this.UsesIndexMarkers)
		{
			float num = (float)(this.sprFill.Size.x + 5) / (float)this.IndexElementCount;
			int x = Mathf.RoundToInt(num - 5f);
			for (int i = 0; i < this.IndexMarkerSprites.Count; i++)
			{
				if (i >= this.IndexElementCount)
				{
					this.IndexMarkerSprites[i].IsVisible = false;
				}
				else
				{
					Vector2i position = this.IndexMarkerSprites[i].Position;
					position.x = Mathf.RoundToInt((float)i * num);
					this.IndexMarkerSprites[i].Position = position;
					this.IndexMarkerSprites[i].IsDirty = true;
					Vector2i size = this.IndexMarkerSprites[i].Size;
					size.x = x;
					if (i == this.IndexElementCount - 1)
					{
						size.x = this.sprFill.Size.x - position.x;
					}
					this.IndexMarkerSprites[i].Size = size;
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void InitSegmentedFillPositions()
	{
		if (!this.UsesSegmentedFill || this.sprFill == null)
		{
			return;
		}
		XUiController childById = base.GetChildById("segmentedFill");
		if (childById == null || this.sprFill == null)
		{
			return;
		}
		this.SegmentedFillCount = childById.Children.Count;
		float num = (float)(this.sprFill.Size.x + this.SegmentedFillSpacing) / (float)this.SegmentedFillCount;
		int x = Mathf.RoundToInt(num - (float)this.SegmentedFillSpacing);
		for (int i = 0; i < childById.Children.Count; i++)
		{
			XUiView viewComponent = childById.Children[i].ViewComponent;
			Vector2i position = viewComponent.Position;
			position.x = Mathf.RoundToInt((float)i * num);
			viewComponent.Position = position;
			viewComponent.IsDirty = true;
			foreach (XUiController xuiController in viewComponent.Controller.Children)
			{
				XUiView viewComponent2 = xuiController.ViewComponent;
				Vector2i size = viewComponent2.Size;
				size.x = x;
				if (i == this.SegmentedFillCount - 1)
				{
					size.x = this.sprFill.Size.x - position.x;
				}
				viewComponent2.Size = size;
				viewComponent2.IsDirty = true;
			}
		}
	}

	public void TriggerValueChangedEvent(TValue _oldVal)
	{
		this.UpdateIndexMarkerStates();
		if (this.OnValueChangedGeneric != null)
		{
			this.OnValueChangedGeneric(this);
		}
		if (this.OnValueChanged != null)
		{
			this.OnValueChanged(this, _oldVal, this.currentValue);
		}
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void BackButton_OnPress(XUiController _sender, int _mouseButton)
	{
		if (this.enabled)
		{
			TValue oldVal = this.currentValue;
			this.BackPressed();
			this.UpdateLabel();
			if (this.isDifferentValue(oldVal, this.currentValue))
			{
				this.TriggerValueChangedEvent(oldVal);
			}
			this.IsDirty = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void ForwardButton_OnPress(XUiController _sender, int _mouseButton)
	{
		if (this.enabled)
		{
			TValue oldVal = this.currentValue;
			this.ForwardPressed();
			this.UpdateLabel();
			if (this.isDifferentValue(oldVal, this.currentValue))
			{
				this.TriggerValueChangedEvent(oldVal);
			}
			this.IsDirty = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void PageUpAction()
	{
		this.ForwardButton_OnPress(null, 0);
		if (base.xui.playerUI.CursorController.CursorModeActive)
		{
			base.SelectCursorElement(false, true);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void PageDownAction()
	{
		this.BackButton_OnPress(null, 0);
		if (base.xui.playerUI.CursorController.CursorModeActive)
		{
			base.SelectCursorElement(false, true);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract bool isDifferentValue(TValue _oldVal, TValue _currentValue);

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract void BackPressed();

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract void ForwardPressed();

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract bool isMax();

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract bool isMin();

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract bool isEmpty();

	public override bool GetBindingValue(ref string _value, string _bindingName)
	{
		if (_bindingName.StartsWith("segment_fill_", StringComparison.Ordinal))
		{
			int index;
			return int.TryParse(_bindingName.AsSpan("segment_fill_".Length), out index) && this.handleSegmentedFillValueBinding(ref _value, index);
		}
		uint num = <PrivateImplementationDetails>.ComputeStringHash(_bindingName);
		if (num <= 2803537331U)
		{
			if (num <= 56539099U)
			{
				if (num != 12543033U)
				{
					if (num == 56539099U)
					{
						if (_bindingName == "can_backward")
						{
							_value = (this.enabled && !this.isEmpty() && (this.Wrap || !this.isMin())).ToString();
							return true;
						}
					}
				}
				else if (_bindingName == "fillvalue")
				{
					_value = "0";
					return true;
				}
			}
			else if (num != 122655197U)
			{
				if (num != 1222862367U)
				{
					if (num == 2803537331U)
					{
						if (_bindingName == "can_forward")
						{
							_value = (this.enabled && !this.isEmpty() && (this.Wrap || !this.isMax())).ToString();
							return true;
						}
					}
				}
				else if (_bindingName == "fillcolor")
				{
					_value = (this.enabled ? this.colorFillEnabled : this.colorFillDisabled).ToXuiColorString();
					return true;
				}
			}
			else if (_bindingName == "valuecolor")
			{
				_value = (this.enabled ? this.colorEnabled : this.colorDisabled).ToXuiColorString();
				return true;
			}
		}
		else if (num <= 3154291628U)
		{
			if (num != 2939888673U)
			{
				if (num == 3154291628U)
				{
					if (_bindingName == "usesmarkers")
					{
						_value = this.UsesIndexMarkers.ToString();
						return true;
					}
				}
			}
			else if (_bindingName == "valuetext")
			{
				_value = this.valueText;
				return true;
			}
		}
		else if (num != 3427432841U)
		{
			if (num != 3853892426U)
			{
				if (num == 4283090096U)
				{
					if (_bindingName == "isnumber")
					{
						_value = false.ToString();
						return true;
					}
				}
			}
			else if (_bindingName == "hascontrollershortcuts")
			{
				_value = (this.gamepadIncreaseShortcut != null && this.gamepadDecreaseShortcut != null).ToString();
				return true;
			}
		}
		else if (_bindingName == "bgcolor")
		{
			_value = this.colorBg.ToXuiColorString();
			return true;
		}
		return base.GetBindingValue(ref _value, _bindingName);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool handleSegmentedFillValueBinding(ref string _value, int _index)
	{
		_value = "0";
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PressEvent(XUiController _sender, int _mouseButton)
	{
		if (PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard)
		{
			return;
		}
		if (this.enabled)
		{
			Vector2i mouseXUIPosition = base.xui.GetMouseXUIPosition();
			XUiController parent = this.clickable;
			Vector3 vector = parent.ViewComponent.UiTransform.localPosition;
			while (parent.Parent != null && parent.Parent.ViewComponent != null)
			{
				parent = parent.Parent;
				vector += parent.ViewComponent.UiTransform.localPosition;
			}
			vector += parent.ViewComponent.UiTransform.parent.localPosition;
			XUiV_Window xuiV_Window = parent.ViewComponent as XUiV_Window;
			if (xuiV_Window != null && xuiV_Window.IsInStackpanel)
			{
				Transform parent2 = xuiV_Window.UiTransform.parent.parent;
				vector *= parent2.localScale.x;
				vector += parent2.localPosition;
			}
			Vector2i vector2i = new Vector2i((int)vector.x, (int)vector.y);
			int num = (vector2i + this.clickable.ViewComponent.Size).x - vector2i.x;
			float num2 = (float)(mouseXUIPosition.x - vector2i.x) / (float)num;
			this.setRelativeValue((double)num2);
		}
	}

	public void ScrollEvent(XUiController _sender, float _delta)
	{
		if (this.enabled && !this.isEmpty())
		{
			if (this.ScrollByIncrement)
			{
				if (_delta > 0f)
				{
					this.PageUpAction();
					return;
				}
				this.PageDownAction();
				return;
			}
			else
			{
				this.incrementalChangeValue((double)_delta);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HoverEvent(XUiController _sender, bool _isOver)
	{
		this.isOver = _isOver;
		LocalPlayerUI.IsAnyComboBoxFocused = this.isOver;
		if (this.isOver)
		{
			base.xui.calloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuComboBox, 0f);
			return;
		}
		base.xui.calloutWindow.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuComboBox);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DragEvent(XUiController _sender, EDragType _dragType, Vector2 _mousePositionDelta)
	{
		if (!this.isOver)
		{
			return;
		}
		if (this.enabled && !this.isEmpty())
		{
			this.PressEvent(_sender, -1);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract void setRelativeValue(double _value);

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract void incrementalChangeValue(double _value);

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_ComboBox()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool enabled = true;

	public TValue Min;

	public TValue Max;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool Wrap = true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool ScrollByIncrement;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color colorEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color colorDisabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color colorBg;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color colorFillEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color colorFillDisabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color colorIndexMarkerActive = Color.white;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color colorIndexMarkerInactive = Color.grey;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlayerAction gamepadDecreaseShortcut;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlayerAction gamepadIncreaseShortcut;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool indexMarkers;

	[PublicizedFrom(EAccessModifier.Private)]
	public string valueText = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiV_Sprite sprFill;

	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly IList<XUiV_Sprite> IndexMarkerSprites = new List<XUiV_Sprite>();

	[PublicizedFrom(EAccessModifier.Protected)]
	public TValue currentValue;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool UsesSegmentedFill;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int SegmentedFillSpacing;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int SegmentedFillCount = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController clickable;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isOver;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDragging;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button forwardButton;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button backButton;

	[PublicizedFrom(EAccessModifier.Private)]
	public const string bindingPrefixSegmentFillValue = "segment_fill_";

	public delegate void XUiEvent_ValueChanged(XUiController _sender, TValue _oldValue, TValue _newValue);

	public delegate void XUiEvent_GenericValueChanged(XUiController _sender);
}

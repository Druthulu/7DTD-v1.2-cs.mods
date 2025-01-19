using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SimpleButton : XUiController
{
	public new event XUiEvent_OnPressEventHandler OnPressed;

	public string Text
	{
		get
		{
			if (this.label == null)
			{
				return null;
			}
			return this.label.Text;
		}
		set
		{
			if (this.label != null)
			{
				this.label.Text = value;
			}
		}
	}

	public string Tooltip
	{
		get
		{
			return this.button.ToolTip;
		}
		set
		{
			if (this.button.ToolTip != value)
			{
				this.button.ToolTip = value;
			}
		}
	}

	public string DisabledToolTip
	{
		get
		{
			return this.button.DisabledToolTip;
		}
		set
		{
			if (this.button.DisabledToolTip != value)
			{
				this.button.DisabledToolTip = value;
			}
		}
	}

	public XUiV_Label Label
	{
		get
		{
			return this.label;
		}
	}

	public XUiV_Button Button
	{
		get
		{
			return this.button;
		}
	}

	public bool Enabled
	{
		get
		{
			return this.isEnabled;
		}
		set
		{
			if (value != this.isEnabled || (this.button != null && value != this.button.Enabled))
			{
				this.isEnabled = value;
				if (this.button != null)
				{
					this.button.Enabled = value;
				}
				if (this.label != null)
				{
					this.label.Color = (value ? this.EnabledLabelColor : this.DisabledLabelColor);
					this.updateLabelFontSize();
					this.updateLabelFontColor();
				}
				this.IsDirty = true;
			}
		}
	}

	public bool IsVisible
	{
		get
		{
			return this.button.IsVisible || this.label.IsVisible;
		}
		set
		{
			this.button.IsVisible = value;
			this.label.IsVisible = value;
			if (this.border != null)
			{
				this.border.IsVisible = value;
			}
		}
	}

	public int FontSizeDefault
	{
		get
		{
			if (this.fontSizeDefault != 0)
			{
				return this.fontSizeDefault;
			}
			if (this.label == null)
			{
				return 0;
			}
			return this.label.FontSize;
		}
		set
		{
			if (value != this.fontSizeDefault)
			{
				this.fontSizeDefault = value;
				this.updateLabelFontSize();
			}
		}
	}

	public int FontSizeHover
	{
		get
		{
			if (this.fontSizeHover != 0)
			{
				return this.fontSizeHover;
			}
			return this.FontSizeDefault;
		}
		set
		{
			if (value != this.fontSizeHover)
			{
				this.fontSizeHover = value;
				this.updateLabelFontSize();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateLabelFontSize()
	{
		if (this.label != null)
		{
			if (this.isEnabled)
			{
				this.label.FontSize = (this.isOver ? this.FontSizeHover : this.FontSizeDefault);
				return;
			}
			this.label.FontSize = this.FontSizeDefault;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateLabelFontColor()
	{
		if (this.label != null)
		{
			if (this.isEnabled)
			{
				this.label.Color = ((this.isOver && this.HoveredLabelColor != null) ? this.HoveredLabelColor.Value : this.EnabledLabelColor);
				return;
			}
			this.label.Color = this.DisabledLabelColor;
		}
	}

	public override void Init()
	{
		base.Init();
		this.button = (base.GetChildById("clickable").ViewComponent as XUiV_Button);
		this.button.Controller.OnPress += this.Btn_OnPress;
		this.button.Controller.OnHover += this.Btn_OnHover;
		this.label = (base.GetChildById("btnLabel").ViewComponent as XUiV_Label);
		if (this.label != null)
		{
			this.label.Color = this.EnabledLabelColor;
		}
		XUiController childById = base.GetChildById("border");
		if (childById != null)
		{
			this.border = (childById.ViewComponent as XUiV_Sprite);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Btn_OnHover(XUiController _sender, bool _isOver)
	{
		this.isOver = _isOver;
		this.updateLabelFontSize();
		this.updateLabelFontColor();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Btn_OnPress(XUiController _sender, int _mouseButton)
	{
		if (this.isEnabled && this.OnPressed != null)
		{
			this.OnPressed(this, _mouseButton);
		}
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

	public override bool ParseAttribute(string name, string value, XUiController _parent)
	{
		if (base.ParseAttribute(name, value, _parent))
		{
			return true;
		}
		if (!(name == "enabled_font_color"))
		{
			if (!(name == "hovered_font_color"))
			{
				if (!(name == "disabled_font_color"))
				{
					if (!(name == "font_size_default"))
					{
						if (!(name == "font_size_hover"))
						{
							if (!(name == "button_enabled"))
							{
								return false;
							}
							this.Enabled = StringParsers.ParseBool(value, 0, -1, true);
						}
						else
						{
							this.FontSizeHover = StringParsers.ParseSInt32(value, 0, -1, NumberStyles.Integer);
						}
					}
					else
					{
						this.FontSizeDefault = StringParsers.ParseSInt32(value, 0, -1, NumberStyles.Integer);
					}
				}
				else
				{
					this.DisabledLabelColor = StringParsers.ParseColor32(value);
				}
			}
			else
			{
				this.HoveredLabelColor = new Color?(StringParsers.ParseColor32(value));
			}
		}
		else
		{
			this.EnabledLabelColor = StringParsers.ParseColor32(value);
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label label;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button button;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite border;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isEnabled = true;

	public string Tag;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isOver;

	[PublicizedFrom(EAccessModifier.Private)]
	public int fontSizeDefault;

	[PublicizedFrom(EAccessModifier.Private)]
	public int fontSizeHover;

	public Color EnabledLabelColor;

	public Color? HoveredLabelColor;

	public Color DisabledLabelColor;
}

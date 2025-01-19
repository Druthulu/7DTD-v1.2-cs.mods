using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ToggleButton : XUiController
{
	public event XUiEvent_ToggleButtonValueChanged OnValueChanged;

	public string Label
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
			if (this.button == null)
			{
				return null;
			}
			return this.button.ToolTip;
		}
		set
		{
			if (this.button != null)
			{
				this.button.ToolTip = value;
			}
		}
	}

	public bool Value
	{
		get
		{
			return this.val;
		}
		set
		{
			if (value != this.val)
			{
				this.val = value;
				this.IsDirty = true;
			}
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
				}
				this.IsDirty = true;
			}
		}
	}

	public override void Init()
	{
		base.Init();
		this.button = (base.GetChildById("clickable").ViewComponent as XUiV_Button);
		this.button.Controller.OnPress += this.Btn_OnPress;
		this.label = (base.GetChildById("btnLabel").ViewComponent as XUiV_Label);
		if (this.label != null)
		{
			this.label.Color = this.EnabledLabelColor;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Btn_OnPress(XUiController _sender, int _mouseButton)
	{
		if (!this.isEnabled)
		{
			return;
		}
		this.val = !this.val;
		this.IsDirty = true;
		if (this.OnValueChanged != null)
		{
			this.OnValueChanged(this, this.val);
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

	public override bool GetBindingValue(ref string value, string bindingName)
	{
		if (bindingName == "value")
		{
			value = this.val.ToString();
			return true;
		}
		return false;
	}

	public override bool ParseAttribute(string name, string value, XUiController _parent)
	{
		if (base.ParseAttribute(name, value, _parent))
		{
			return true;
		}
		if (!(name == "enabled_font_color"))
		{
			if (!(name == "disabled_font_color"))
			{
				return false;
			}
			this.DisabledLabelColor = StringParsers.ParseColor32(value);
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

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool val;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isEnabled = true;

	public string Tag;

	public Color EnabledLabelColor;

	public Color DisabledLabelColor;
}

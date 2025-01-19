using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ComboBoxBool : XUiC_ComboBox<bool>
{
	public override bool Value
	{
		get
		{
			return this.currentValue;
		}
		set
		{
			if (this.currentValue != value)
			{
				this.currentValue = value;
				this.IsDirty = true;
				this.UpdateLabel();
			}
		}
	}

	public override int IndexElementCount
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return 2;
		}
	}

	public override int IndexMarkerIndex
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (!this.currentValue)
			{
				return 0;
			}
			return 1;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateLabel()
	{
		base.ValueText = ((!string.IsNullOrEmpty(this.LocalizationPrefix)) ? Localization.Get(this.LocalizationPrefix + (this.currentValue ? "On" : "Off"), false) : (this.currentValue ? "Yes" : "No"));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isDifferentValue(bool _oldVal, bool _currentValue)
	{
		return _oldVal != _currentValue;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void BackPressed()
	{
		this.currentValue = !this.currentValue;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void ForwardPressed()
	{
		this.currentValue = !this.currentValue;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isMax()
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isMin()
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isEmpty()
	{
		return false;
	}

	public override bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
		if (_name == "localization_prefix")
		{
			this.LocalizationPrefix = _value;
			return true;
		}
		return base.ParseAttribute(_name, _value, _parent);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void setRelativeValue(double _value)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void incrementalChangeValue(double _value)
	{
		base.ForwardButton_OnPress(this, -1);
	}

	public string LocalizationPrefix;
}

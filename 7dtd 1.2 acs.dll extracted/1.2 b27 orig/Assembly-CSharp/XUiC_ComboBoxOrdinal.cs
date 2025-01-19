using System;
using UnityEngine.Scripting;

[Preserve]
public abstract class XUiC_ComboBoxOrdinal<TValue> : XUiC_ComboBox<TValue> where TValue : struct, IEquatable<TValue>, IComparable<TValue>, IFormattable, IConvertible
{
	public override TValue Value
	{
		get
		{
			return this.currentValue;
		}
		set
		{
			if (!this.currentValue.Equals(value))
			{
				if (value.CompareTo(this.Max) > 0)
				{
					value = this.Max;
				}
				else if (value.CompareTo(this.Min) < 0)
				{
					value = this.Min;
				}
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
			return -1;
		}
	}

	public override int IndexMarkerIndex
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return -1;
		}
	}

	public override void OnOpen()
	{
		if (this.currentValue.CompareTo(this.Max) > 0)
		{
			this.Value = this.Max;
		}
		else if (this.currentValue.CompareTo(this.Min) < 0)
		{
			this.Value = this.Min;
		}
		base.OnOpen();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateLabel()
	{
		base.ValueText = this.currentValue.ToString(this.FormatString, Utils.StandardCulture);
	}

	public void UpdateLabel(string text)
	{
		base.ValueText = text;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isDifferentValue(TValue _oldVal, TValue _currentValue)
	{
		return !_oldVal.Equals(_currentValue);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void BackPressed()
	{
		if (this.currentValue.CompareTo(this.Min) < 0)
		{
			this.currentValue = (this.Wrap ? this.Max : this.Min);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void ForwardPressed()
	{
		if (this.currentValue.CompareTo(this.Max) > 0)
		{
			this.currentValue = (this.Wrap ? this.Min : this.Max);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isMax()
	{
		return this.currentValue.CompareTo(this.Max) == 0;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isMin()
	{
		return this.currentValue.CompareTo(this.Min) == 0;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isEmpty()
	{
		return false;
	}

	public override bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
		if (_name == "format_string")
		{
			this.FormatString = (string.IsNullOrEmpty(_value) ? null : _value);
			return true;
		}
		return base.ParseAttribute(_name, _value, _parent);
	}

	public override bool GetBindingValue(ref string _value, string _bindingName)
	{
		if (_bindingName == "isnumber")
		{
			_value = true.ToString();
			return true;
		}
		return base.GetBindingValue(ref _value, _bindingName);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_ComboBoxOrdinal()
	{
	}

	public string FormatString;
}

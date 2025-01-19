using System;
using System.Globalization;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ComboBoxFloat : XUiC_ComboBoxOrdinal<double>
{
	public XUiC_ComboBoxFloat()
	{
		this.Max = 1.0;
		this.Min = 0.0;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void BackPressed()
	{
		this.currentValue -= this.IncrementSize;
		base.BackPressed();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void ForwardPressed()
	{
		this.currentValue += this.IncrementSize;
		base.ForwardPressed();
	}

	public override bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
		if (_name == "value_min")
		{
			if (!_value.EqualsCaseInsensitive("@def"))
			{
				this.Min = StringParsers.ParseDouble(_value, 0, -1, NumberStyles.Any);
			}
			return true;
		}
		if (_name == "value_max")
		{
			if (!_value.EqualsCaseInsensitive("@def"))
			{
				this.Max = StringParsers.ParseDouble(_value, 0, -1, NumberStyles.Any);
			}
			return true;
		}
		if (!(_name == "value_increment"))
		{
			return base.ParseAttribute(_name, _value, _parent);
		}
		if (!_value.EqualsCaseInsensitive("@def"))
		{
			this.IncrementSize = StringParsers.ParseDouble(_value, 0, -1, NumberStyles.Any);
		}
		return true;
	}

	public override bool GetBindingValue(ref string _value, string _bindingName)
	{
		if (_bindingName == "fillvalue")
		{
			_value = ((this.currentValue - this.Min) / (this.Max - this.Min)).ToCultureInvariantString();
			return true;
		}
		return base.GetBindingValue(ref _value, _bindingName);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void setRelativeValue(double _value)
	{
		double value = this.Value;
		this.Value = (this.Max - this.Min) * _value + this.Min;
		base.TriggerValueChangedEvent(value);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void incrementalChangeValue(double _value)
	{
		double num = (this.Max - this.Min) * _value * 0.5;
		if (num > this.IncrementSize)
		{
			num = this.IncrementSize;
		}
		else if (num < -this.IncrementSize)
		{
			num = -this.IncrementSize;
		}
		double value = this.Value;
		double num2 = this.Value + num;
		if (_value < 0.0 && num2 < this.Min && this.Wrap)
		{
			num2 = this.Max;
		}
		else if (_value > 0.0 && num2 > this.Max && this.Wrap)
		{
			num2 = this.Min;
		}
		this.Value = num2;
		base.TriggerValueChangedEvent(value);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool handleSegmentedFillValueBinding(ref string _value, int _index)
	{
		double num = (this.Max - this.Min) / (double)this.SegmentedFillCount;
		double num2 = (double)_index * num;
		double num3 = (double)(_index + 1) * num;
		if (this.currentValue <= num2)
		{
			_value = "0";
		}
		else if (this.currentValue >= num3)
		{
			_value = "1";
		}
		else
		{
			_value = ((this.currentValue - num2) / num).ToCultureInvariantString();
		}
		return true;
	}

	public double IncrementSize = 1.0;
}

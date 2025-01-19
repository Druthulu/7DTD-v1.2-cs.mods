using System;
using System.Globalization;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ComboBoxInt : XUiC_ComboBoxOrdinal<long>
{
	public XUiC_ComboBoxInt()
	{
		this.Max = long.MaxValue;
		this.Min = long.MinValue;
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
				this.Min = StringParsers.ParseSInt64(_value, 0, -1, NumberStyles.Integer);
			}
			return true;
		}
		if (_name == "value_max")
		{
			if (!_value.EqualsCaseInsensitive("@def"))
			{
				this.Max = StringParsers.ParseSInt64(_value, 0, -1, NumberStyles.Integer);
			}
			return true;
		}
		if (!(_name == "value_increment"))
		{
			return base.ParseAttribute(_name, _value, _parent);
		}
		if (!_value.EqualsCaseInsensitive("@def"))
		{
			this.IncrementSize = StringParsers.ParseSInt64(_value, 0, -1, NumberStyles.Integer);
		}
		return true;
	}

	public override bool GetBindingValue(ref string _value, string _bindingName)
	{
		if (_bindingName == "fillvalue")
		{
			_value = this.fillvalueFormatter.Format(((float)this.currentValue - (float)this.Min) / (float)(this.Max - this.Min));
			return true;
		}
		return base.GetBindingValue(ref _value, _bindingName);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void setRelativeValue(double _value)
	{
		long value = this.Value;
		this.Value = (long)((double)(this.Max - this.Min) * _value) + this.Min;
		base.TriggerValueChangedEvent(value);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void incrementalChangeValue(double _value)
	{
		long num = (long)((double)(this.Max - this.Min) * _value * 0.5);
		if (_value > 0.0 && num == 0L)
		{
			num = 1L;
		}
		else if (_value < 0.0 && num == 0L)
		{
			num = -1L;
		}
		if (num > this.IncrementSize)
		{
			num = this.IncrementSize;
		}
		else if (num < -this.IncrementSize)
		{
			num = -this.IncrementSize;
		}
		long value = this.Value;
		long num2 = this.Value + num;
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

	public long IncrementSize = 1L;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterFloat fillvalueFormatter = new CachedStringFormatterFloat(null);
}

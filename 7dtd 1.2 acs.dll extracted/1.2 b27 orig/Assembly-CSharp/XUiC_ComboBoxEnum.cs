using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ComboBoxEnum<TEnum> : XUiC_ComboBox<TEnum> where TEnum : struct, IConvertible
{
	public override TEnum Value
	{
		get
		{
			return this.currentValue;
		}
		set
		{
			if (this.currentValue.Ordinal<TEnum>() != value.Ordinal<TEnum>())
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

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateLabel()
	{
		base.ValueText = ((!string.IsNullOrEmpty(this.LocalizationPrefix)) ? Localization.Get(this.LocalizationPrefix + this.currentValue.ToStringCached<TEnum>(), false) : this.currentValue.ToStringCached<TEnum>());
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isDifferentValue(TEnum _oldVal, TEnum _currentValue)
	{
		return _oldVal.Ordinal<TEnum>() != _currentValue.Ordinal<TEnum>();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void BackPressed()
	{
		if (this.MinSet && this.MaxSet)
		{
			this.currentValue = this.currentValue.CycleEnum(this.Min, this.Max, true, this.Wrap);
			return;
		}
		this.currentValue = this.currentValue.CycleEnum(true, this.Wrap);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void ForwardPressed()
	{
		if (this.MinSet && this.MaxSet)
		{
			this.currentValue = this.currentValue.CycleEnum(this.Min, this.Max, false, this.Wrap);
			return;
		}
		this.currentValue = this.currentValue.CycleEnum(false, this.Wrap);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isMax()
	{
		if (!this.MinSet || !this.MaxSet)
		{
			return this.currentValue.Ordinal<TEnum>() == EnumUtils.MaxValue<TEnum>().Ordinal<TEnum>();
		}
		return this.currentValue.Ordinal<TEnum>() == this.Max.Ordinal<TEnum>();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isMin()
	{
		if (!this.MinSet || !this.MaxSet)
		{
			return this.currentValue.Ordinal<TEnum>() == EnumUtils.MinValue<TEnum>().Ordinal<TEnum>();
		}
		return this.currentValue.Ordinal<TEnum>() == this.Min.Ordinal<TEnum>();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isEmpty()
	{
		return false;
	}

	public override bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
		if (_name == "value_min")
		{
			if (!_value.EqualsCaseInsensitive("@def"))
			{
				this.Min = EnumUtils.Parse<TEnum>(_value, false);
				this.MinSet = true;
			}
			return true;
		}
		if (_name == "value_max")
		{
			if (!_value.EqualsCaseInsensitive("@def"))
			{
				this.Max = EnumUtils.Parse<TEnum>(_value, false);
				this.MaxSet = true;
			}
			return true;
		}
		if (!(_name == "localization_prefix"))
		{
			return base.ParseAttribute(_name, _value, _parent);
		}
		this.LocalizationPrefix = _value;
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void setRelativeValue(double _value)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void incrementalChangeValue(double _value)
	{
		if (_value > 0.0)
		{
			base.ForwardButton_OnPress(this, -1);
			return;
		}
		if (_value < 0.0)
		{
			base.BackButton_OnPress(this, -1);
		}
	}

	public void SetMinMax(TEnum _min, TEnum _max)
	{
		this.Min = _min;
		this.Max = _max;
		this.MinSet = true;
		this.MaxSet = true;
	}

	public string LocalizationPrefix;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool MinSet;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool MaxSet;
}

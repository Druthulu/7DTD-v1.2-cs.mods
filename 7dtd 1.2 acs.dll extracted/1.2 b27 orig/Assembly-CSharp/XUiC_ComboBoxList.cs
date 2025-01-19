using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ComboBoxList<TElement> : XUiC_ComboBox<TElement>
{
	public override TElement Value
	{
		get
		{
			return this.currentValue;
		}
		set
		{
			if (this.Elements.Contains(value))
			{
				int selectedIndex = this.Elements.IndexOf(value);
				this.SelectedIndex = selectedIndex;
				this.useCustomValue = false;
				return;
			}
			if (value != null)
			{
				this.CustomValue = value;
				this.useCustomValue = true;
				this.SelectedIndex = -1;
			}
		}
	}

	public override int IndexElementCount
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return this.MaxIndex - this.MinIndex + 1;
		}
	}

	public override int IndexMarkerIndex
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return this.SelectedIndex - this.MinIndex;
		}
	}

	public int SelectedIndex
	{
		get
		{
			return this.currentIndex;
		}
		set
		{
			if (value < 0)
			{
				if (this.useCustomValue)
				{
					this.currentIndex = value;
					this.currentValue = this.CustomValue;
				}
				else
				{
					this.currentIndex = this.minIndex;
					this.currentValue = this.Elements[this.currentIndex];
				}
			}
			else
			{
				if (value >= this.Elements.Count)
				{
					value = this.Elements.Count - 1;
				}
				if (value < this.minIndex)
				{
					value = this.minIndex;
				}
				else if (value > this.maxIndex)
				{
					value = this.maxIndex;
				}
				this.currentIndex = value;
				this.currentValue = this.Elements[this.currentIndex];
			}
			this.IsDirty = true;
			this.UpdateLabel();
		}
	}

	public int MinIndex
	{
		get
		{
			if (this.minIndex >= 0)
			{
				return this.minIndex;
			}
			return 0;
		}
		set
		{
			if (value != this.minIndex)
			{
				this.minIndex = value;
				if (this.currentIndex < this.minIndex)
				{
					this.SelectedIndex = this.minIndex;
				}
				base.UpdateIndexMarkerPositions();
				this.IsDirty = true;
			}
		}
	}

	public int MaxIndex
	{
		get
		{
			if (this.maxIndex < this.Elements.Count)
			{
				return this.maxIndex;
			}
			return this.Elements.Count - 1;
		}
		set
		{
			if (value != this.maxIndex)
			{
				this.maxIndex = value;
				if (this.currentIndex > this.maxIndex)
				{
					this.SelectedIndex = this.maxIndex;
				}
				base.UpdateIndexMarkerPositions();
				this.IsDirty = true;
			}
		}
	}

	public override void OnOpen()
	{
		if (this.Elements.Count > 0 && !this.useCustomValue)
		{
			if (this.currentIndex < 0)
			{
				this.SelectedIndex = 0;
			}
			if (this.currentIndex > this.Elements.Count)
			{
				this.SelectedIndex = this.Elements.Count - 1;
			}
			if (this.currentIndex < this.minIndex)
			{
				this.SelectedIndex = this.minIndex;
			}
			if (this.currentIndex > this.maxIndex)
			{
				this.SelectedIndex = this.maxIndex;
			}
		}
		base.OnOpen();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateLabel()
	{
		if (this.isEmpty())
		{
			base.ValueText = "";
			return;
		}
		base.ValueText = ((!string.IsNullOrEmpty(this.LocalizationPrefix)) ? Localization.Get(this.LocalizationPrefix + this.currentValue.ToString(), this.LocalizationKeyCaseInsensitive) : this.currentValue.ToString());
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isDifferentValue(TElement _oldVal, TElement _currentValue)
	{
		return !_oldVal.Equals(_currentValue);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void BackPressed()
	{
		this.ChangeIndex(this.ReverseList ? 1 : -1);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void ForwardPressed()
	{
		this.ChangeIndex(this.ReverseList ? -1 : 1);
	}

	public void ChangeIndex(int _direction)
	{
		int num = this.currentIndex + _direction;
		if (num < this.minIndex)
		{
			num = (this.Wrap ? Utils.FastMin(this.Elements.Count - 1, this.maxIndex) : this.minIndex);
		}
		if (num > this.maxIndex)
		{
			num = (this.Wrap ? Utils.FastMax(0, this.minIndex) : this.maxIndex);
		}
		if (num < 0)
		{
			num = (this.Wrap ? (this.Elements.Count - 1) : 0);
		}
		if (num >= this.Elements.Count)
		{
			num = (this.Wrap ? 0 : (this.Elements.Count - 1));
		}
		this.SelectedIndex = num;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isMax()
	{
		if (!this.ReverseList)
		{
			return this.isMaxIndex();
		}
		return this.isMinIndex();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isMin()
	{
		if (!this.ReverseList)
		{
			return this.isMinIndex();
		}
		return this.isMaxIndex();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isEmpty()
	{
		return this.Elements.Count == 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isMaxIndex()
	{
		return this.currentIndex == this.maxIndex || this.currentIndex == this.Elements.Count - 1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isMinIndex()
	{
		return this.currentIndex == this.minIndex || this.currentIndex == 0;
	}

	public override bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
		if (_name == "values")
		{
			if (!_value.EqualsCaseInsensitive("@def"))
			{
				string[] array = _value.Split(',', StringSplitOptions.None);
				Type typeFromHandle = typeof(TElement);
				if (typeFromHandle == typeof(string))
				{
					foreach (string text in array)
					{
						this.Elements.Add((TElement)((object)text.Trim()));
					}
				}
				else if (typeof(IConvertible).IsAssignableFrom(typeFromHandle))
				{
					foreach (string text2 in array)
					{
						try
						{
							this.Elements.Add((TElement)((object)Convert.ChangeType(text2, typeFromHandle)));
						}
						catch (Exception e)
						{
							Log.Error(string.Format("[XUi] Value \"{0}\" not supported for the ComboBox type {1}", text2, typeFromHandle));
							Log.Exception(e);
						}
					}
				}
			}
			return true;
		}
		if (_name == "reverse_list")
		{
			if (!_value.EqualsCaseInsensitive("@def"))
			{
				this.ReverseList = StringParsers.ParseBool(_value, 0, -1, true);
			}
			return true;
		}
		if (_name == "localization_prefix")
		{
			this.LocalizationPrefix = _value;
			return true;
		}
		if (!(_name == "localization_key_caseinsensitive"))
		{
			return base.ParseAttribute(_name, _value, _parent);
		}
		this.LocalizationKeyCaseInsensitive = StringParsers.ParseBool(_value, 0, -1, true);
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

	public string LocalizationPrefix;

	public bool LocalizationKeyCaseInsensitive;

	public readonly List<TElement> Elements = new List<TElement>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int currentIndex = int.MinValue;

	public TElement CustomValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool useCustomValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public int minIndex = int.MinValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public int maxIndex = int.MaxValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool ReverseList;
}

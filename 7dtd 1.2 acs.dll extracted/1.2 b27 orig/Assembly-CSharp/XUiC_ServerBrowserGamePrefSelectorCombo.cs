using System;
using System.Globalization;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ServerBrowserGamePrefSelectorCombo : XUiController, IServerBrowserFilterControl
{
	public bool isCustomRange
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.controlCombo != null && this.controlCombo.Value.Type == XUiC_ServerBrowserGamePrefSelectorCombo.EOptionValueType.Custom;
		}
	}

	public GameInfoBool GameInfoBool { get; [PublicizedFrom(EAccessModifier.Private)] set; } = (GameInfoBool)(-1);

	public GameInfoInt GameInfoInt { get; [PublicizedFrom(EAccessModifier.Private)] set; } = (GameInfoInt)(-1);

	public GameInfoString GameInfoString { get; [PublicizedFrom(EAccessModifier.Private)] set; } = (GameInfoString)(-1);

	public int? ValueRangeMin
	{
		set
		{
			int? num = this.valueRangeMin;
			int? num2 = value;
			if (num.GetValueOrDefault() == num2.GetValueOrDefault() & num != null == (num2 != null))
			{
				return;
			}
			this.valueRangeMin = value;
			this.SetupOptions();
		}
	}

	public int? ValueRangeMax
	{
		set
		{
			int? num = this.valueRangeMax;
			int? num2 = value;
			if (num.GetValueOrDefault() == num2.GetValueOrDefault() & num != null == (num2 != null))
			{
				return;
			}
			this.valueRangeMax = value;
			this.SetupOptions();
		}
	}

	public Func<int, int> CustomValuePreFilterModifierFunc
	{
		set
		{
			this.customValuePreFilterModifierFunc = value;
		}
	}

	public Func<int, int> ValuePreDisplayModifierFunc
	{
		set
		{
			this.valuePreDisplayModifierFunc = value;
			if (this.valuePreDisplayModifierFunc != null)
			{
				this.SetupOptions();
			}
		}
	}

	public bool Enabled
	{
		get
		{
			return this.enabled;
		}
		set
		{
			if (this.enabled != value)
			{
				this.enabled = value;
				this.controlCombo.Enabled = value;
				this.forwardButton.Enabled = value;
				this.backButton.Enabled = value;
			}
		}
	}

	public override void Init()
	{
		base.Init();
		GameInfoBool gameInfoBool;
		if (EnumUtils.TryParse<GameInfoBool>(this.viewComponent.ID, out gameInfoBool, false))
		{
			this.GameInfoBool = gameInfoBool;
			this.valueType = GamePrefs.EnumType.Bool;
		}
		GameInfoInt gameInfoInt;
		if (EnumUtils.TryParse<GameInfoInt>(this.viewComponent.ID, out gameInfoInt, false))
		{
			this.GameInfoInt = gameInfoInt;
			this.valueType = GamePrefs.EnumType.Int;
		}
		GameInfoString gameInfoString;
		if (EnumUtils.TryParse<GameInfoString>(this.viewComponent.ID, out gameInfoString, false))
		{
			this.GameInfoString = gameInfoString;
			this.valueType = GamePrefs.EnumType.String;
		}
		this.controlCombo = base.GetChildById("ControlCombo").GetChildByType<XUiC_ComboBoxList<XUiC_ServerBrowserGamePrefSelectorCombo.GameOptionValue>>();
		this.controlCombo.OnValueChanged += this.ControlCombo_OnValueChanged;
		this.forwardButton = (XUiV_Button)this.controlCombo.GetChildById("forward").ViewComponent;
		this.backButton = (XUiV_Button)this.controlCombo.GetChildById("back").ViewComponent;
		this.txtValueMin = base.GetChildById("valuemin").GetChildByType<XUiC_TextInput>();
		this.txtValueMin.OnChangeHandler += this.ControlValue_OnChangeHandler;
		this.txtValueMin.OnScroll += this.controlCombo.ScrollEvent;
		this.txtValueMax = base.GetChildById("valuemax").GetChildByType<XUiC_TextInput>();
		this.txtValueMax.OnChangeHandler += this.ControlValue_OnChangeHandler;
		this.txtValueMax.OnScroll += this.controlCombo.ScrollEvent;
		this.txtValueString = base.GetChildById("valuestring").GetChildByType<XUiC_TextInput>();
		this.txtValueString.OnChangeHandler += this.ControlText_OnChangeHandler;
		this.txtValueString.OnScroll += this.controlCombo.ScrollEvent;
		this.txtValueMin.SelectOnTab = this.txtValueMax;
		this.txtValueMax.SelectOnTab = this.txtValueMin;
		this.SetupOptions();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (this.forceCustom)
		{
			this.forwardButton.IsVisible = false;
			this.backButton.IsVisible = false;
		}
		this.IsDirty = true;
	}

	public override bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
		uint num = <PrivateImplementationDetails>.ComputeStringHash(_name);
		if (num <= 1633745671U)
		{
			if (num <= 877087803U)
			{
				if (num != 345560954U)
				{
					if (num == 877087803U)
					{
						if (_name == "values")
						{
							if (_value.Length > 0)
							{
								this.valuesFromXml = _value.Split(',', StringSplitOptions.None);
								for (int i = 0; i < this.valuesFromXml.Length; i++)
								{
									this.valuesFromXml[i] = this.valuesFromXml[i].Trim();
								}
							}
							return true;
						}
					}
				}
				else if (_name == "force_custom")
				{
					this.forceCustom = StringParsers.ParseBool(_value, 0, -1, true);
					if (this.forceCustom)
					{
						this.allowCustom = true;
					}
					return true;
				}
			}
			else if (num != 1102564575U)
			{
				if (num != 1222086634U)
				{
					if (num == 1633745671U)
					{
						if (_name == "allow_any")
						{
							this.SetAllowAny(StringParsers.ParseBool(_value, 0, -1, true));
							return true;
						}
					}
				}
				else if (_name == "display_names")
				{
					if (_value.Length > 0)
					{
						this.namesFromXml = _value.Split(',', StringSplitOptions.None);
						for (int j = 0; j < this.namesFromXml.Length; j++)
						{
							this.namesFromXml[j] = this.namesFromXml[j].Trim();
						}
					}
					return true;
				}
			}
			else if (_name == "value_localization_prefix")
			{
				if (_value.Length > 0)
				{
					this.valueLocalizationPrefixFromXml = _value.Trim();
				}
				return true;
			}
		}
		else if (num <= 2470140894U)
		{
			if (num != 1704990518U)
			{
				if (num == 2470140894U)
				{
					if (_name == "default")
					{
						this.defaultValue = _value;
						return true;
					}
				}
			}
			else if (_name == "default_string")
			{
				this.defaultString = _value;
				return true;
			}
		}
		else if (num != 3400394597U)
		{
			if (num != 3569450715U)
			{
				if (num == 3838393572U)
				{
					if (_name == "allow_custom")
					{
						this.allowCustom = (this.forceCustom || StringParsers.ParseBool(_value, 0, -1, true));
						return true;
					}
				}
			}
			else if (_name == "default_max")
			{
				this.defaultMax = _value;
				return true;
			}
		}
		else if (_name == "default_min")
		{
			this.defaultMin = _value;
			return true;
		}
		return base.ParseAttribute(_name, _value, _parent);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetAllowAny(bool _value)
	{
		if (_value == this.allowAny)
		{
			return;
		}
		this.allowAny = _value;
		if (this.controlCombo == null || this.controlCombo.Elements.Count == 0)
		{
			return;
		}
		if (_value)
		{
			if (!this.controlCombo.Elements.Contains(new XUiC_ServerBrowserGamePrefSelectorCombo.GameOptionValue(XUiC_ServerBrowserGamePrefSelectorCombo.EOptionValueType.Any, "")))
			{
				this.controlCombo.Elements.Insert(1, new XUiC_ServerBrowserGamePrefSelectorCombo.GameOptionValue(XUiC_ServerBrowserGamePrefSelectorCombo.EOptionValueType.Any, Localization.Get("goAnyValue", false)));
				if (this.controlCombo.SelectedIndex > 0)
				{
					XUiC_ComboBoxList<XUiC_ServerBrowserGamePrefSelectorCombo.GameOptionValue> xuiC_ComboBoxList = this.controlCombo;
					int selectedIndex = xuiC_ComboBoxList.SelectedIndex;
					xuiC_ComboBoxList.SelectedIndex = selectedIndex + 1;
				}
			}
		}
		else if (this.controlCombo.Elements.Remove(new XUiC_ServerBrowserGamePrefSelectorCombo.GameOptionValue(XUiC_ServerBrowserGamePrefSelectorCombo.EOptionValueType.Any, "")) && this.controlCombo.SelectedIndex > 1)
		{
			XUiC_ComboBoxList<XUiC_ServerBrowserGamePrefSelectorCombo.GameOptionValue> xuiC_ComboBoxList2 = this.controlCombo;
			int selectedIndex = xuiC_ComboBoxList2.SelectedIndex;
			xuiC_ComboBoxList2.SelectedIndex = selectedIndex - 1;
		}
		this.controlCombo.SelectedIndex = this.controlCombo.SelectedIndex;
	}

	public override bool GetBindingValue(ref string _value, string _bindingName)
	{
		if (_bindingName == "iscustomrange")
		{
			_value = (this.valueType != GamePrefs.EnumType.String && (this.forceCustom || this.isCustomRange)).ToString();
			return true;
		}
		if (_bindingName == "iscustomstring")
		{
			_value = (this.valueType == GamePrefs.EnumType.String && (this.forceCustom || this.isCustomRange)).ToString();
			return true;
		}
		if (!(_bindingName == "useCombo"))
		{
			return base.GetBindingValue(ref _value, _bindingName);
		}
		_value = (!this.forceCustom).ToString();
		return true;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.IsDirty)
		{
			this.IsDirty = false;
			base.RefreshBindings(false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string GetGameInfoName()
	{
		switch (this.valueType)
		{
		case GamePrefs.EnumType.Int:
			return this.GameInfoInt.ToStringCached<GameInfoInt>();
		case GamePrefs.EnumType.String:
			return this.GameInfoString.ToStringCached<GameInfoString>();
		case GamePrefs.EnumType.Bool:
			return this.GameInfoBool.ToStringCached<GameInfoBool>();
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupOptions()
	{
		this.controlCombo.Elements.Clear();
		this.controlCombo.Elements.Add(new XUiC_ServerBrowserGamePrefSelectorCombo.GameOptionValue(XUiC_ServerBrowserGamePrefSelectorCombo.EOptionValueType.Custom, ""));
		if (this.allowAny)
		{
			this.controlCombo.Elements.Add(new XUiC_ServerBrowserGamePrefSelectorCombo.GameOptionValue(XUiC_ServerBrowserGamePrefSelectorCombo.EOptionValueType.Any, Localization.Get("goAnyValue", false)));
		}
		if (!this.forceCustom)
		{
			int[] array = null;
			string[] array2 = null;
			string[] array3;
			switch (this.valueType)
			{
			case GamePrefs.EnumType.Int:
				if (!string.IsNullOrEmpty(this.defaultValue))
				{
					this.defaultOptionValue = new XUiC_ServerBrowserGamePrefSelectorCombo.GameOptionValue(StringParsers.ParseSInt32(this.defaultValue, 0, -1, NumberStyles.Integer), "");
				}
				if (this.valuesFromXml != null)
				{
					array = new int[this.valuesFromXml.Length];
					for (int i = 0; i < this.valuesFromXml.Length; i++)
					{
						array[i] = StringParsers.ParseSInt32(this.valuesFromXml[i], 0, -1, NumberStyles.Integer);
					}
				}
				if (array == null)
				{
					if (this.namesFromXml == null)
					{
						throw new Exception("Illegal option setup for " + this.GetGameInfoName() + " (no values and no names specified)");
					}
					array = new int[this.namesFromXml.Length];
					for (int j = 0; j < array.Length; j++)
					{
						array[j] = j;
					}
				}
				array3 = new string[array.Length];
				if (this.namesFromXml == null || this.namesFromXml.Length != array.Length)
				{
					for (int k = 0; k < array.Length; k++)
					{
						if (this.namesFromXml != null && k < this.namesFromXml.Length)
						{
							array3[k] = Localization.Get(this.namesFromXml[k], false);
						}
						else
						{
							int num = array[k];
							if (this.valuePreDisplayModifierFunc != null)
							{
								num = this.valuePreDisplayModifierFunc(num);
							}
							if (string.IsNullOrEmpty(this.valueLocalizationPrefixFromXml))
							{
								array3[k] = num.ToString();
							}
							else
							{
								array3[k] = string.Format(Localization.Get(this.valueLocalizationPrefixFromXml + ((num == 1) ? "" : "s"), false), num);
							}
						}
					}
					goto IL_3A1;
				}
				for (int l = 0; l < this.namesFromXml.Length; l++)
				{
					array3[l] = Localization.Get(this.namesFromXml[l], false);
				}
				goto IL_3A1;
			case GamePrefs.EnumType.String:
				if (!string.IsNullOrEmpty(this.defaultValue))
				{
					this.defaultOptionValue = new XUiC_ServerBrowserGamePrefSelectorCombo.GameOptionValue(this.defaultValue, "");
				}
				array2 = this.valuesFromXml;
				array3 = new string[array2.Length];
				if (this.namesFromXml == null || this.namesFromXml.Length != array2.Length)
				{
					for (int m = 0; m < array2.Length; m++)
					{
						if (this.namesFromXml != null && m < this.namesFromXml.Length)
						{
							array3[m] = Localization.Get(this.namesFromXml[m], false);
						}
						else
						{
							string text = array2[m];
							if (string.IsNullOrEmpty(this.valueLocalizationPrefixFromXml))
							{
								array3[m] = text;
							}
							else
							{
								array3[m] = Localization.Get(this.valueLocalizationPrefixFromXml + text, false);
							}
						}
					}
					goto IL_3A1;
				}
				for (int n = 0; n < this.namesFromXml.Length; n++)
				{
					array3[n] = Localization.Get(this.namesFromXml[n], false);
				}
				goto IL_3A1;
			case GamePrefs.EnumType.Bool:
				if (!string.IsNullOrEmpty(this.defaultValue))
				{
					this.defaultOptionValue = new XUiC_ServerBrowserGamePrefSelectorCombo.GameOptionValue(StringParsers.ParseBool(this.defaultValue, 0, -1, true) ? 1 : 0, "");
				}
				array = new int[]
				{
					0,
					1
				};
				array3 = new string[]
				{
					Localization.Get("goOff", false),
					Localization.Get("goOn", false)
				};
				goto IL_3A1;
			}
			throw new NotSupportedException("Not a valid GameInfoX: " + this.viewComponent.ID);
			IL_3A1:
			if (array3 == null)
			{
				throw new Exception("Illegal option setup for " + this.GetGameInfoName() + " (names null)");
			}
			if (this.valueType != GamePrefs.EnumType.String)
			{
				if (array == null)
				{
					throw new Exception("Illegal option setup for " + this.GetGameInfoName() + " (values still null)");
				}
				for (int num2 = 0; num2 < array.Length; num2++)
				{
					if ((this.valueRangeMin == null || array[num2] >= this.valueRangeMin.Value) && (this.valueRangeMax == null || array[num2] <= this.valueRangeMax.Value))
					{
						this.controlCombo.Elements.Add(new XUiC_ServerBrowserGamePrefSelectorCombo.GameOptionValue(array[num2], array3[num2]));
					}
				}
			}
			if (this.valueType == GamePrefs.EnumType.String)
			{
				if (array2 == null)
				{
					throw new Exception("Illegal option setup for " + this.GetGameInfoName() + " (values still null)");
				}
				for (int num3 = 0; num3 < array2.Length; num3++)
				{
					this.controlCombo.Elements.Add(new XUiC_ServerBrowserGamePrefSelectorCombo.GameOptionValue(array2[num3], array3[num3]));
				}
			}
		}
		this.controlCombo.MinIndex = (((this.valueType == GamePrefs.EnumType.Int || this.valueType == GamePrefs.EnumType.String) && this.allowCustom) ? 0 : 1);
		this.setDefaultValues();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ControlCombo_OnValueChanged(XUiController _sender, XUiC_ServerBrowserGamePrefSelectorCombo.GameOptionValue _oldValue, XUiC_ServerBrowserGamePrefSelectorCombo.GameOptionValue _newValue)
	{
		this.IsDirty = true;
		Action<IServerBrowserFilterControl> onValueChanged = this.OnValueChanged;
		if (onValueChanged == null)
		{
			return;
		}
		onValueChanged(this);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public unsafe void ControlValue_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		XUiC_TextInput xuiC_TextInput = _sender as XUiC_TextInput;
		if (xuiC_TextInput != null && !string.IsNullOrEmpty(_text))
		{
			ReadOnlySpan<char> readOnlySpan = _text.AsSpan().Trim();
			while (readOnlySpan.Length > 1 && *readOnlySpan[0] == 48)
			{
				ReadOnlySpan<char> readOnlySpan2 = readOnlySpan;
				readOnlySpan = readOnlySpan2.Slice(1, readOnlySpan2.Length - 1);
			}
			int value;
			int? num = StringParsers.TryParseSInt32(new string(readOnlySpan), out value, 0, -1, NumberStyles.Integer) ? new int?(value) : null;
			if (num != null)
			{
				if (this.valueRangeMin != null && num.Value < this.valueRangeMin.Value)
				{
					num = this.valueRangeMin;
				}
				if (this.valueRangeMax != null && num.Value > this.valueRangeMax.Value)
				{
					num = this.valueRangeMax;
				}
			}
			string text = (num != null) ? num.Value.ToString(CultureInfo.InvariantCulture) : "";
			if (text != _text)
			{
				xuiC_TextInput.Text = text;
			}
		}
		Action<IServerBrowserFilterControl> onValueChanged = this.OnValueChanged;
		if (onValueChanged == null)
		{
			return;
		}
		onValueChanged(this);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ControlText_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		Action<IServerBrowserFilterControl> onValueChanged = this.OnValueChanged;
		if (onValueChanged == null)
		{
			return;
		}
		onValueChanged(this);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setDefaultValues()
	{
		if (!this.forceCustom && this.defaultOptionValue.Type != XUiC_ServerBrowserGamePrefSelectorCombo.EOptionValueType.Null)
		{
			this.controlCombo.SelectedIndex = this.controlCombo.Elements.IndexOf(this.defaultOptionValue);
		}
		else
		{
			this.controlCombo.SelectedIndex = (this.forceCustom ? 0 : 1);
		}
		this.txtValueMin.Text = (this.defaultMin ?? "");
		this.txtValueMax.Text = (this.defaultMax ?? "");
		this.txtValueString.Text = (this.defaultString ?? "");
	}

	public void Reset()
	{
		this.setDefaultValues();
		this.IsDirty = true;
		Action<IServerBrowserFilterControl> onValueChanged = this.OnValueChanged;
		if (onValueChanged == null)
		{
			return;
		}
		onValueChanged(this);
	}

	public void SelectEntry(XUiC_ServerBrowserGamePrefSelectorCombo.GameOptionValue _value)
	{
		int num = this.controlCombo.Elements.IndexOf(_value);
		if (num >= 0)
		{
			this.controlCombo.SelectedIndex = num;
			Action<IServerBrowserFilterControl> onValueChanged = this.OnValueChanged;
			if (onValueChanged == null)
			{
				return;
			}
			onValueChanged(this);
		}
	}

	public XUiC_ServerBrowserGamePrefSelectorCombo.GameOptionValue GetSelection()
	{
		return this.controlCombo.Value;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ServersList.UiServerFilter GetValueRangeFilter(string filterName, int? filterMin, int? filterMax)
	{
		if (this.valueRangeMin != null)
		{
			if (filterMin != null)
			{
				int? num = filterMin;
				int value = this.valueRangeMin.Value;
				if (!(num.GetValueOrDefault() < value & num != null))
				{
					goto IL_74;
				}
			}
			filterMin = new int?(this.valueRangeMin.Value);
		}
		IL_74:
		if (this.valueRangeMax != null)
		{
			if (filterMax != null)
			{
				int? num = filterMax;
				int value = this.valueRangeMax.Value;
				if (!(num.GetValueOrDefault() > value & num != null))
				{
					goto IL_CD;
				}
			}
			filterMax = new int?(this.valueRangeMax.Value);
		}
		IL_CD:
		Func<XUiC_ServersList.ListEntry, bool> func;
		IServerListInterface.ServerFilter.EServerFilterType type;
		if (filterMin != null)
		{
			if (filterMax != null)
			{
				if (filterMin.Value == filterMax.Value)
				{
					func = delegate(XUiC_ServersList.ListEntry _entry)
					{
						int value2 = _entry.gameServerInfo.GetValue(this.GameInfoInt);
						int? filterMin2 = filterMin;
						return value2 == filterMin2.GetValueOrDefault() & filterMin2 != null;
					};
					type = IServerListInterface.ServerFilter.EServerFilterType.IntValue;
				}
				else
				{
					func = delegate(XUiC_ServersList.ListEntry _entry)
					{
						int value2 = _entry.gameServerInfo.GetValue(this.GameInfoInt);
						int num2 = value2;
						int? num3 = filterMin;
						if (num2 >= num3.GetValueOrDefault() & num3 != null)
						{
							int num4 = value2;
							num3 = filterMax;
							return num4 <= num3.GetValueOrDefault() & num3 != null;
						}
						return false;
					};
					type = IServerListInterface.ServerFilter.EServerFilterType.IntRange;
				}
			}
			else
			{
				func = delegate(XUiC_ServersList.ListEntry _entry)
				{
					int value2 = _entry.gameServerInfo.GetValue(this.GameInfoInt);
					int? filterMin2 = filterMin;
					return value2 >= filterMin2.GetValueOrDefault() & filterMin2 != null;
				};
				type = IServerListInterface.ServerFilter.EServerFilterType.IntMin;
			}
		}
		else if (filterMax != null)
		{
			func = delegate(XUiC_ServersList.ListEntry _entry)
			{
				int value2 = _entry.gameServerInfo.GetValue(this.GameInfoInt);
				int? filterMax2 = filterMax;
				return value2 <= filterMax2.GetValueOrDefault() & filterMax2 != null;
			};
			type = IServerListInterface.ServerFilter.EServerFilterType.IntMax;
		}
		else
		{
			func = null;
			type = IServerListInterface.ServerFilter.EServerFilterType.Any;
		}
		return new XUiC_ServersList.UiServerFilter(filterName, XUiC_ServersList.EnumServerLists.Regular, func, type, filterMin.GetValueOrDefault(), filterMax.GetValueOrDefault(), false, null);
	}

	public XUiC_ServersList.UiServerFilter GetFilter()
	{
		string gameInfoName = this.GetGameInfoName();
		if (this.controlCombo.Value.Type == XUiC_ServerBrowserGamePrefSelectorCombo.EOptionValueType.Any)
		{
			if (this.valueType != GamePrefs.EnumType.String)
			{
				return this.GetValueRangeFilter(gameInfoName, null, null);
			}
			return new XUiC_ServersList.UiServerFilter(gameInfoName, XUiC_ServersList.EnumServerLists.Regular, null, IServerListInterface.ServerFilter.EServerFilterType.Any, 0, 0, false, null);
		}
		else
		{
			string sValue;
			if (!this.isCustomRange)
			{
				switch (this.valueType)
				{
				case GamePrefs.EnumType.Int:
				{
					int intValue = this.controlCombo.Value.IntValue;
					return this.GetValueRangeFilter(gameInfoName, new int?(intValue), new int?(intValue));
				}
				case GamePrefs.EnumType.String:
				{
					string sValue = this.controlCombo.Value.StringValue;
					Func<XUiC_ServersList.ListEntry, bool> func = (XUiC_ServersList.ListEntry _entry) => _entry.gameServerInfo.GetValue(this.GameInfoString).EqualsCaseInsensitive(sValue);
					IServerListInterface.ServerFilter.EServerFilterType type = IServerListInterface.ServerFilter.EServerFilterType.StringValue;
					return new XUiC_ServersList.UiServerFilter(gameInfoName, XUiC_ServersList.EnumServerLists.Regular, func, type, 0, 0, false, sValue);
				}
				case GamePrefs.EnumType.Bool:
				{
					bool bValue = this.controlCombo.Value.IntValue == 1;
					Func<XUiC_ServersList.ListEntry, bool> func = (XUiC_ServersList.ListEntry _entry) => _entry.gameServerInfo.GetValue(this.GameInfoBool) == bValue;
					IServerListInterface.ServerFilter.EServerFilterType type = IServerListInterface.ServerFilter.EServerFilterType.BoolValue;
					return new XUiC_ServersList.UiServerFilter(gameInfoName, XUiC_ServersList.EnumServerLists.Regular, func, type, 0, 0, bValue, null);
				}
				}
				throw new ArgumentOutOfRangeException();
			}
			if (this.valueType != GamePrefs.EnumType.String)
			{
				int value;
				int? filterMin = StringParsers.TryParseSInt32(this.txtValueMin.Text, out value, 0, -1, NumberStyles.Integer) ? new int?(value) : null;
				int value2;
				int? filterMax = StringParsers.TryParseSInt32(this.txtValueMax.Text, out value2, 0, -1, NumberStyles.Integer) ? new int?(value2) : null;
				if (filterMin != null && this.customValuePreFilterModifierFunc != null)
				{
					filterMin = new int?(this.customValuePreFilterModifierFunc(filterMin.Value));
				}
				if (filterMax != null && this.customValuePreFilterModifierFunc != null)
				{
					filterMax = new int?(this.customValuePreFilterModifierFunc(filterMax.Value));
				}
				return this.GetValueRangeFilter(gameInfoName, filterMin, filterMax);
			}
			sValue = this.txtValueString.Text.Trim();
			if (sValue.Length == 0)
			{
				return new XUiC_ServersList.UiServerFilter(gameInfoName, XUiC_ServersList.EnumServerLists.Regular, null, IServerListInterface.ServerFilter.EServerFilterType.Any, 0, 0, false, null);
			}
			Func<XUiC_ServersList.ListEntry, bool> func2 = (XUiC_ServersList.ListEntry _entry) => _entry.gameServerInfo.GetValue(this.GameInfoString).ContainsCaseInsensitive(sValue);
			IServerListInterface.ServerFilter.EServerFilterType type2 = IServerListInterface.ServerFilter.EServerFilterType.StringContains;
			return new XUiC_ServersList.UiServerFilter(gameInfoName, XUiC_ServersList.EnumServerLists.Regular, func2, type2, 0, 0, false, this.txtValueString.Text);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<XUiC_ServerBrowserGamePrefSelectorCombo.GameOptionValue> controlCombo;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button forwardButton;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button backButton;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtValueMin;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtValueMax;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtValueString;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool allowAny = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool allowCustom = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool forceCustom;

	[PublicizedFrom(EAccessModifier.Private)]
	public string defaultValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ServerBrowserGamePrefSelectorCombo.GameOptionValue defaultOptionValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public string defaultMin;

	[PublicizedFrom(EAccessModifier.Private)]
	public string defaultMax;

	[PublicizedFrom(EAccessModifier.Private)]
	public string defaultString;

	[PublicizedFrom(EAccessModifier.Private)]
	public string[] valuesFromXml;

	[PublicizedFrom(EAccessModifier.Private)]
	public string[] namesFromXml;

	[PublicizedFrom(EAccessModifier.Private)]
	public string valueLocalizationPrefixFromXml;

	[PublicizedFrom(EAccessModifier.Private)]
	public int? valueRangeMin;

	[PublicizedFrom(EAccessModifier.Private)]
	public int? valueRangeMax;

	[PublicizedFrom(EAccessModifier.Private)]
	public GamePrefs.EnumType valueType;

	[PublicizedFrom(EAccessModifier.Private)]
	public Func<int, int> customValuePreFilterModifierFunc;

	[PublicizedFrom(EAccessModifier.Private)]
	public Func<int, int> valuePreDisplayModifierFunc;

	public Action<IServerBrowserFilterControl> OnValueChanged;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool enabled = true;

	public enum EOptionValueType
	{
		Null,
		Custom,
		Any,
		Int,
		String
	}

	public readonly struct GameOptionValue : IEquatable<XUiC_ServerBrowserGamePrefSelectorCombo.GameOptionValue>
	{
		public GameOptionValue(XUiC_ServerBrowserGamePrefSelectorCombo.EOptionValueType _type, string _displayName)
		{
			this.Type = _type;
			this.IntValue = -1;
			this.StringValue = null;
			this.DisplayName = _displayName;
		}

		public GameOptionValue(int _intValue, string _displayName)
		{
			this.Type = XUiC_ServerBrowserGamePrefSelectorCombo.EOptionValueType.Int;
			this.IntValue = _intValue;
			this.StringValue = null;
			this.DisplayName = _displayName;
		}

		public GameOptionValue(string _stringValue, string _displayName)
		{
			this.Type = XUiC_ServerBrowserGamePrefSelectorCombo.EOptionValueType.String;
			this.IntValue = -1;
			this.StringValue = _stringValue;
			this.DisplayName = _displayName;
		}

		public override string ToString()
		{
			return this.DisplayName;
		}

		public bool Equals(XUiC_ServerBrowserGamePrefSelectorCombo.GameOptionValue _other)
		{
			if (this.Type != _other.Type)
			{
				return false;
			}
			switch (this.Type)
			{
			case XUiC_ServerBrowserGamePrefSelectorCombo.EOptionValueType.Custom:
			case XUiC_ServerBrowserGamePrefSelectorCombo.EOptionValueType.Any:
				return true;
			case XUiC_ServerBrowserGamePrefSelectorCombo.EOptionValueType.Int:
				return this.IntValue == _other.IntValue;
			case XUiC_ServerBrowserGamePrefSelectorCombo.EOptionValueType.String:
				return this.StringValue == _other.StringValue;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}

		public override bool Equals(object _obj)
		{
			if (_obj is XUiC_ServerBrowserGamePrefSelectorCombo.GameOptionValue)
			{
				XUiC_ServerBrowserGamePrefSelectorCombo.GameOptionValue other = (XUiC_ServerBrowserGamePrefSelectorCombo.GameOptionValue)_obj;
				return this.Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (int)(((this.Type * (XUiC_ServerBrowserGamePrefSelectorCombo.EOptionValueType)397 ^ (XUiC_ServerBrowserGamePrefSelectorCombo.EOptionValueType)this.IntValue) * (XUiC_ServerBrowserGamePrefSelectorCombo.EOptionValueType)397 ^ (XUiC_ServerBrowserGamePrefSelectorCombo.EOptionValueType)((this.StringValue != null) ? this.StringValue.GetHashCode() : 0)) * (XUiC_ServerBrowserGamePrefSelectorCombo.EOptionValueType)397 ^ (XUiC_ServerBrowserGamePrefSelectorCombo.EOptionValueType)((this.DisplayName != null) ? this.DisplayName.GetHashCode() : 0));
		}

		public readonly XUiC_ServerBrowserGamePrefSelectorCombo.EOptionValueType Type;

		public readonly int IntValue;

		public readonly string StringValue;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string DisplayName;
	}
}

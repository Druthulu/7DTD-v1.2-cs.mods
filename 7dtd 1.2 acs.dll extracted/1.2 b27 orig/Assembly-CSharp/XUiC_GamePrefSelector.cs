using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_GamePrefSelector : XUiController
{
	public EnumGamePrefs GamePref
	{
		get
		{
			return this.gamePref;
		}
	}

	public XUiC_TextInput ControlText
	{
		get
		{
			return this.controlText;
		}
	}

	public Func<int, int> ValuePreDisplayModifierFunc
	{
		get
		{
			return this.valuePreDisplayModifierFunc;
		}
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
				this.controlText.Enabled = value;
				this.controlText.ActiveTextColor = (value ? this.enabledColor : this.disabledColor);
				this.controlLabel.Color = (value ? this.enabledColor : this.disabledColor);
			}
		}
	}

	public override void Init()
	{
		base.Init();
		this.gamePref = EnumUtils.Parse<EnumGamePrefs>(this.viewComponent.ID, false);
		this.controlLabel = (XUiV_Label)base.GetChildById("ControlLabel").ViewComponent;
		this.enabledColor = this.controlLabel.Color;
		this.controlCombo = base.GetChildById("ControlCombo").GetChildByType<XUiC_ComboBoxList<XUiC_GamePrefSelector.GameOptionValue>>();
		this.controlCombo.OnValueChanged += this.ControlCombo_OnValueChanged;
		this.controlText = base.GetChildById("ControlText").GetChildByType<XUiC_TextInput>();
		this.controlText.OnChangeHandler += this.ControlText_OnChangeHandler;
		if (!this.isTextInput)
		{
			this.SetupOptions();
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.controlCombo.ViewComponent.IsVisible = !this.isTextInput;
		this.controlText.ViewComponent.IsVisible = this.isTextInput;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
	}

	public override bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
		uint num = <PrivateImplementationDetails>.ComputeStringHash(_name);
		if (num <= 1102564575U)
		{
			if (num <= 877087803U)
			{
				if (num != 811104650U)
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
				else if (_name == "values_enforced")
				{
					this.valuesEnforced = StringParsers.ParseBool(_value, 0, -1, true);
					return true;
				}
			}
			else if (num != 996973687U)
			{
				if (num == 1102564575U)
				{
					if (_name == "value_localization_prefix")
					{
						if (_value.Length > 0)
						{
							this.valueLocalizationPrefixFromXml = _value.Trim();
						}
						return true;
					}
				}
			}
			else if (_name == "value_type")
			{
				this.valueType = EnumUtils.Parse<GamePrefs.EnumType>(_value, true);
				return true;
			}
		}
		else if (num <= 2533046024U)
		{
			if (num != 1222086634U)
			{
				if (num == 2533046024U)
				{
					if (_name == "always_show")
					{
						this.alwaysShow = StringParsers.ParseBool(_value, 0, -1, true);
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
		else if (num != 3319127657U)
		{
			if (num != 3589293078U)
			{
				if (num == 3931288483U)
				{
					if (_name == "is_textinput")
					{
						this.isTextInput = StringParsers.ParseBool(_value, 0, -1, true);
						return true;
					}
				}
			}
			else if (_name == "values_from_gameserverconfig")
			{
				this.valuesFromGameServerConfig = StringParsers.ParseBool(_value, 0, -1, true);
				return true;
			}
		}
		else if (_name == "has_default")
		{
			this.hasDefault = StringParsers.ParseBool(_value, 0, -1, true);
			return true;
		}
		return base.ParseAttribute(_name, _value, _parent);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupOptions()
	{
		int[] array = null;
		string[] array2 = null;
		string[] array3;
		switch (this.valueType)
		{
		case GamePrefs.EnumType.Int:
		{
			bool flag = false;
			if (this.valuesFromGameServerConfig && (this.valuesFromXml == null || this.valuesFromXml.Length == 0))
			{
				array = GameServerInfo.GetDefaultIntValues(this.gamePref);
				flag = true;
			}
			if (this.valuesFromXml != null && !flag)
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
					throw new Exception("Illegal option setup for " + this.gamePref.ToStringCached<EnumGamePrefs>() + " (no values and no names specified)");
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
				goto IL_331;
			}
			for (int l = 0; l < this.namesFromXml.Length; l++)
			{
				array3[l] = Localization.Get(this.namesFromXml[l], false);
			}
			goto IL_331;
		}
		case GamePrefs.EnumType.String:
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
				goto IL_331;
			}
			for (int n = 0; n < this.namesFromXml.Length; n++)
			{
				array3[n] = Localization.Get(this.namesFromXml[n], false);
			}
			goto IL_331;
		case GamePrefs.EnumType.Bool:
			array = new int[]
			{
				0,
				1
			};
			array3 = new string[2];
			if (this.namesFromXml != null && this.namesFromXml.Length == 2)
			{
				array3[0] = Localization.Get(this.namesFromXml[0], false);
				array3[1] = Localization.Get(this.namesFromXml[1], false);
				goto IL_331;
			}
			array3[0] = Localization.Get("goOff", false);
			array3[1] = Localization.Get("goOn", false);
			goto IL_331;
		}
		throw new NotSupportedException("Not a valid GamePref: " + this.viewComponent.ID);
		IL_331:
		this.controlCombo.Elements.Clear();
		if (array3 == null)
		{
			throw new Exception("Illegal option setup for " + this.gamePref.ToStringCached<EnumGamePrefs>() + " (names null)");
		}
		if (this.valueType != GamePrefs.EnumType.String)
		{
			if (array == null)
			{
				throw new Exception("Illegal option setup for " + this.gamePref.ToStringCached<EnumGamePrefs>() + " (values still null)");
			}
			for (int num2 = 0; num2 < array.Length; num2++)
			{
				this.controlCombo.Elements.Add(new XUiC_GamePrefSelector.GameOptionValue(array[num2], array3[num2]));
			}
		}
		if (this.valueType == GamePrefs.EnumType.String)
		{
			if (array2 == null)
			{
				throw new Exception("Illegal option setup for " + this.gamePref.ToStringCached<EnumGamePrefs>() + " (values still null)");
			}
			for (int num3 = 0; num3 < array2.Length; num3++)
			{
				this.controlCombo.Elements.Add(new XUiC_GamePrefSelector.GameOptionValue(array2[num3], array3[num3]));
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ControlText_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		GamePrefs.EnumType enumType = this.valueType;
		int value;
		if (enumType != GamePrefs.EnumType.Int)
		{
			if (enumType != GamePrefs.EnumType.String)
			{
				throw new Exception("Illegal option setup for " + this.gamePref.ToStringCached<EnumGamePrefs>());
			}
			GamePrefs.Set(this.gamePref, _text);
		}
		else if (int.TryParse(_text, out value))
		{
			GamePrefs.Set(this.gamePref, value);
		}
		Action<XUiC_GamePrefSelector, EnumGamePrefs> onValueChanged = this.OnValueChanged;
		if (onValueChanged != null)
		{
			onValueChanged(this, this.gamePref);
		}
		this.CheckDefaultValue();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ControlCombo_OnValueChanged(XUiController _sender, XUiC_GamePrefSelector.GameOptionValue _oldValue, XUiC_GamePrefSelector.GameOptionValue _newValue)
	{
		switch (this.valueType)
		{
		case GamePrefs.EnumType.Int:
			GamePrefs.Set(this.gamePref, _newValue.IntValue);
			goto IL_76;
		case GamePrefs.EnumType.String:
			GamePrefs.Set(this.gamePref, _newValue.StringValue);
			goto IL_76;
		case GamePrefs.EnumType.Bool:
			GamePrefs.Set(this.gamePref, _newValue.IntValue == 1);
			goto IL_76;
		}
		throw new Exception("Illegal option setup for " + this.gamePref.ToStringCached<EnumGamePrefs>());
		IL_76:
		Action<XUiC_GamePrefSelector, EnumGamePrefs> onValueChanged = this.OnValueChanged;
		if (onValueChanged != null)
		{
			onValueChanged(this, this.gamePref);
		}
		this.CheckDefaultValue();
	}

	public void SetCurrentValue()
	{
		try
		{
			switch (this.valueType)
			{
			case GamePrefs.EnumType.Int:
			{
				int @int = GamePrefs.GetInt(this.gamePref);
				if (this.isTextInput)
				{
					this.controlText.Text = @int.ToString();
					goto IL_3A2;
				}
				bool flag = false;
				for (int i = 1; i < this.controlCombo.Elements.Count; i++)
				{
					if (this.controlCombo.Elements[i].IntValue == @int)
					{
						this.controlCombo.SelectedIndex = i;
						flag = true;
						break;
					}
				}
				if (this.valuesEnforced && !flag)
				{
					int num = -1;
					int num2 = int.MaxValue;
					for (int j = 0; j < this.controlCombo.Elements.Count; j++)
					{
						int num3 = Math.Abs(this.controlCombo.Elements[j].IntValue - @int);
						if (num < 0 || num3 < num2)
						{
							num = j;
							num2 = num3;
							if (num2 <= 0)
							{
								break;
							}
						}
					}
					if (num >= 0)
					{
						this.controlCombo.SelectedIndex = num;
						GamePrefs.Set(this.gamePref, this.controlCombo.Value.IntValue);
						flag = true;
					}
				}
				if (flag)
				{
					goto IL_3A2;
				}
				if (string.IsNullOrEmpty(this.valueLocalizationPrefixFromXml))
				{
					XUiC_GamePrefSelector.GameOptionValue value = new XUiC_GamePrefSelector.GameOptionValue(@int, string.Format("{0} {1}", @int.ToString(), Localization.Get("goCustomValueSuffix", false)));
					this.controlCombo.Value = value;
					goto IL_3A2;
				}
				XUiC_GamePrefSelector.GameOptionValue value2 = new XUiC_GamePrefSelector.GameOptionValue(@int, string.Format(Localization.Get(this.valueLocalizationPrefixFromXml + ((@int == 1) ? "" : "s"), false), @int.ToString()) + " " + Localization.Get("goCustomValueSuffix", false));
				this.controlCombo.Value = value2;
				goto IL_3A2;
			}
			case GamePrefs.EnumType.String:
			{
				string @string = GamePrefs.GetString(this.gamePref);
				if (this.isTextInput)
				{
					this.controlText.Text = GamePrefs.GetString(this.gamePref);
					goto IL_3A2;
				}
				bool flag2 = false;
				for (int k = 1; k < this.controlCombo.Elements.Count; k++)
				{
					if (this.controlCombo.Elements[k].StringValue == @string)
					{
						this.controlCombo.SelectedIndex = k;
						flag2 = true;
						break;
					}
				}
				if (this.valuesEnforced && !flag2 && this.controlCombo.Elements.Count > 0)
				{
					this.controlCombo.SelectedIndex = 0;
					GamePrefs.Set(this.gamePref, this.controlCombo.Value.StringValue);
					flag2 = true;
				}
				if (flag2)
				{
					goto IL_3A2;
				}
				if (string.IsNullOrEmpty(@string))
				{
					this.controlCombo.SelectedIndex = 0;
					GamePrefs.Set(this.gamePref, this.controlCombo.Value.StringValue);
					goto IL_3A2;
				}
				if (string.IsNullOrEmpty(this.valueLocalizationPrefixFromXml))
				{
					XUiC_GamePrefSelector.GameOptionValue value3 = new XUiC_GamePrefSelector.GameOptionValue(@string, string.Format("{0} {1}", @string, Localization.Get("goCustomValueSuffix", false)));
					this.controlCombo.Value = value3;
					goto IL_3A2;
				}
				XUiC_GamePrefSelector.GameOptionValue value4 = new XUiC_GamePrefSelector.GameOptionValue(@string, string.Format(Localization.Get(this.valueLocalizationPrefixFromXml ?? "", false), @string) + " " + Localization.Get("goCustomValueSuffix", false));
				this.controlCombo.Value = value4;
				goto IL_3A2;
			}
			case GamePrefs.EnumType.Bool:
				this.controlCombo.SelectedIndex = (GamePrefs.GetBool(this.gamePref) ? 1 : 0);
				goto IL_3A2;
			}
			throw new Exception("Illegal option setup for " + this.gamePref.ToStringCached<EnumGamePrefs>());
			IL_3A2:;
		}
		catch (Exception e)
		{
			Log.Exception(e);
		}
		Action<XUiC_GamePrefSelector, EnumGamePrefs> onValueChanged = this.OnValueChanged;
		if (onValueChanged != null)
		{
			onValueChanged(this, this.gamePref);
		}
		this.CheckDefaultValue();
	}

	public void CheckDefaultValue()
	{
		Color color;
		if (this.enabled)
		{
			color = (this.IsDefaultValueForGameMode() ? Color.white : Color.yellow);
		}
		else
		{
			color = this.disabledColor;
		}
		this.controlText.ActiveTextColor = color;
		this.controlCombo.TextColor = color;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsDefaultValueForGameMode()
	{
		if (!this.hasDefault)
		{
			return true;
		}
		if (this.currentGameMode == null)
		{
			return true;
		}
		Dictionary<EnumGamePrefs, GameMode.ModeGamePref> gamePrefs = this.currentGameMode.GetGamePrefs();
		if (!gamePrefs.ContainsKey(this.gamePref))
		{
			return false;
		}
		switch (this.valueType)
		{
		case GamePrefs.EnumType.Int:
		{
			int intValue;
			if (this.isTextInput)
			{
				StringParsers.TryParseSInt32(this.controlText.Text, out intValue, 0, -1, NumberStyles.Integer);
			}
			else
			{
				intValue = this.controlCombo.Value.IntValue;
			}
			return intValue == (int)gamePrefs[this.gamePref].DefaultValue;
		}
		case GamePrefs.EnumType.String:
			if (this.isTextInput)
			{
				return this.controlText.Text == (string)gamePrefs[this.gamePref].DefaultValue;
			}
			return this.controlCombo.Value.StringValue == (string)gamePrefs[this.gamePref].DefaultValue;
		case GamePrefs.EnumType.Bool:
			return this.controlCombo.Value.IntValue == 1 == (bool)gamePrefs[this.gamePref].DefaultValue;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetVisible(bool _visible)
	{
		this.viewComponent.IsVisible = _visible;
	}

	public void SetCurrentGameMode(GameMode _gameMode)
	{
		this.currentGameMode = _gameMode;
		this.SetVisible(this.alwaysShow || (_gameMode != null && _gameMode.GetGamePrefs().ContainsKey(this.gamePref)));
		this.CheckDefaultValue();
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void OverrideValues(List<string> overrideValues)
	{
		this.valuesFromXml = overrideValues.ToArray();
		this.SetupOptions();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label controlLabel;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<XUiC_GamePrefSelector.GameOptionValue> controlCombo;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput controlText;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color enabledColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color disabledColor = new Color(0.625f, 0.625f, 0.625f);

	[PublicizedFrom(EAccessModifier.Private)]
	public bool valuesFromGameServerConfig;

	[PublicizedFrom(EAccessModifier.Private)]
	public string[] valuesFromXml;

	[PublicizedFrom(EAccessModifier.Private)]
	public string[] namesFromXml;

	[PublicizedFrom(EAccessModifier.Private)]
	public string valueLocalizationPrefixFromXml;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool valuesEnforced;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isTextInput;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasDefault = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool alwaysShow;

	[PublicizedFrom(EAccessModifier.Private)]
	public EnumGamePrefs gamePref = EnumGamePrefs.Last;

	[PublicizedFrom(EAccessModifier.Private)]
	public GamePrefs.EnumType valueType;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameMode currentGameMode;

	[PublicizedFrom(EAccessModifier.Private)]
	public Func<int, int> valuePreDisplayModifierFunc;

	public Action<XUiC_GamePrefSelector, EnumGamePrefs> OnValueChanged;

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

	public readonly struct GameOptionValue : IEquatable<XUiC_GamePrefSelector.GameOptionValue>
	{
		public GameOptionValue(XUiC_GamePrefSelector.EOptionValueType _type, string _displayName)
		{
			this.Type = _type;
			this.IntValue = -1;
			this.StringValue = null;
			this.DisplayName = _displayName;
		}

		public GameOptionValue(int _intValue, string _displayName)
		{
			this.Type = XUiC_GamePrefSelector.EOptionValueType.Int;
			this.IntValue = _intValue;
			this.StringValue = null;
			this.DisplayName = _displayName;
		}

		public GameOptionValue(string _stringValue, string _displayName)
		{
			this.Type = XUiC_GamePrefSelector.EOptionValueType.String;
			this.IntValue = -1;
			this.StringValue = _stringValue;
			this.DisplayName = _displayName;
		}

		public override string ToString()
		{
			return this.DisplayName;
		}

		public bool Equals(XUiC_GamePrefSelector.GameOptionValue _other)
		{
			if (this.Type != _other.Type)
			{
				return false;
			}
			switch (this.Type)
			{
			case XUiC_GamePrefSelector.EOptionValueType.Custom:
			case XUiC_GamePrefSelector.EOptionValueType.Any:
				return true;
			case XUiC_GamePrefSelector.EOptionValueType.Int:
				return this.IntValue == _other.IntValue;
			case XUiC_GamePrefSelector.EOptionValueType.String:
				return this.StringValue == _other.StringValue;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}

		public override bool Equals(object _obj)
		{
			if (_obj is XUiC_GamePrefSelector.GameOptionValue)
			{
				XUiC_GamePrefSelector.GameOptionValue other = (XUiC_GamePrefSelector.GameOptionValue)_obj;
				return this.Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (int)(((this.Type * (XUiC_GamePrefSelector.EOptionValueType)397 ^ (XUiC_GamePrefSelector.EOptionValueType)this.IntValue) * (XUiC_GamePrefSelector.EOptionValueType)397 ^ (XUiC_GamePrefSelector.EOptionValueType)((this.StringValue != null) ? this.StringValue.GetHashCode() : 0)) * (XUiC_GamePrefSelector.EOptionValueType)397 ^ (XUiC_GamePrefSelector.EOptionValueType)((this.DisplayName != null) ? this.DisplayName.GetHashCode() : 0));
		}

		public readonly XUiC_GamePrefSelector.EOptionValueType Type;

		public readonly int IntValue;

		public readonly string StringValue;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string DisplayName;
	}
}

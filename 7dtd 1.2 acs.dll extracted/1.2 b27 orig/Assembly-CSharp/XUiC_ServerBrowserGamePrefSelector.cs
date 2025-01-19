using System;
using System.Globalization;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ServerBrowserGamePrefSelector : XUiController
{
	public GamePrefs.EnumType ValueType
	{
		get
		{
			return this.valueType;
		}
	}

	public GameInfoBool GameInfoBool
	{
		get
		{
			return this.gameInfoBool;
		}
	}

	public GameInfoInt GameInfoInt
	{
		get
		{
			return this.gameInfoInt;
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

	public override void Init()
	{
		base.Init();
		if (EnumUtils.TryParse<GameInfoBool>(this.viewComponent.ID, out this.gameInfoBool, false))
		{
			this.valueType = GamePrefs.EnumType.Bool;
		}
		else
		{
			this.gameInfoBool = (GameInfoBool)(-1);
		}
		if (EnumUtils.TryParse<GameInfoInt>(this.viewComponent.ID, out this.gameInfoInt, false))
		{
			this.valueType = GamePrefs.EnumType.Int;
		}
		else
		{
			this.gameInfoInt = (GameInfoInt)(-1);
		}
		this.controlCombo = base.GetChildById("ControlCombo").GetChildByType<XUiC_ComboBoxList<XUiC_ServerBrowserGamePrefSelector.GameOptionValue>>();
		this.controlCombo.OnValueChanged += this.ControlCombo_OnValueChanged;
		this.SetupOptions();
	}

	public override bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
		if (_name == "values")
		{
			if (_value.Length > 0)
			{
				string[] array = _value.Split(',', StringSplitOptions.None);
				this.valuesFromXml = new int[array.Length];
				for (int i = 0; i < array.Length; i++)
				{
					this.valuesFromXml[i] = StringParsers.ParseSInt32(array[i], 0, -1, NumberStyles.Integer);
				}
			}
			return true;
		}
		if (_name == "display_names")
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
		if (!(_name == "value_localization_prefix"))
		{
			return base.ParseAttribute(_name, _value, _parent);
		}
		if (_value.Length > 0)
		{
			this.valueLocalizationPrefixFromXml = _value.Trim();
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string GetGameInfoName()
	{
		GamePrefs.EnumType enumType = this.valueType;
		if (enumType == GamePrefs.EnumType.Int)
		{
			return this.gameInfoInt.ToStringCached<GameInfoInt>();
		}
		if (enumType == GamePrefs.EnumType.Bool)
		{
			return this.gameInfoBool.ToStringCached<GameInfoBool>();
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupOptions()
	{
		string[] array = null;
		if (this.valueType == GamePrefs.EnumType.Int)
		{
			if (this.valuesFromXml == null)
			{
				if (this.namesFromXml == null)
				{
					throw new Exception("Illegal option setup for " + this.GetGameInfoName() + " (no values and no names specified)");
				}
				this.valuesFromXml = new int[this.namesFromXml.Length];
				for (int i = 0; i < this.valuesFromXml.Length; i++)
				{
					this.valuesFromXml[i] = i;
				}
			}
			array = new string[this.valuesFromXml.Length];
			if (this.namesFromXml == null || this.namesFromXml.Length != this.valuesFromXml.Length)
			{
				for (int j = 0; j < this.valuesFromXml.Length; j++)
				{
					if (this.namesFromXml != null && j < this.namesFromXml.Length)
					{
						array[j] = Localization.Get(this.namesFromXml[j], false);
					}
					else
					{
						int num = this.valuesFromXml[j];
						if (this.valuePreDisplayModifierFunc != null)
						{
							num = this.valuePreDisplayModifierFunc(num);
						}
						array[j] = string.Format(Localization.Get(this.valueLocalizationPrefixFromXml + ((num == 1) ? "" : "s"), false), num);
					}
				}
			}
			else
			{
				for (int k = 0; k < this.namesFromXml.Length; k++)
				{
					array[k] = Localization.Get(this.namesFromXml[k], false);
				}
			}
		}
		else if (this.valueType == GamePrefs.EnumType.Bool)
		{
			this.valuesFromXml = new int[]
			{
				0,
				1
			};
			array = new string[]
			{
				Localization.Get("goOff", false),
				Localization.Get("goOn", false)
			};
		}
		this.controlCombo.Elements.Clear();
		XUiC_ServerBrowserGamePrefSelector.GameOptionValue item = new XUiC_ServerBrowserGamePrefSelector.GameOptionValue(int.MinValue, Localization.Get("goAnyValue", false));
		this.controlCombo.Elements.Add(item);
		if (this.valuesFromXml == null)
		{
			throw new Exception("Illegal option setup for " + this.GetGameInfoName() + " (values still null)");
		}
		if (array == null)
		{
			throw new Exception("Illegal option setup for " + this.GetGameInfoName() + " (names null)");
		}
		for (int l = 0; l < this.valuesFromXml.Length; l++)
		{
			item = new XUiC_ServerBrowserGamePrefSelector.GameOptionValue(this.valuesFromXml[l], array[l]);
			this.controlCombo.Elements.Add(item);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ControlCombo_OnValueChanged(XUiController _sender, XUiC_ServerBrowserGamePrefSelector.GameOptionValue _oldValue, XUiC_ServerBrowserGamePrefSelector.GameOptionValue _newValue)
	{
		if (this.OnValueChanged != null)
		{
			this.OnValueChanged(this);
		}
	}

	public void SetCurrentValue(object _value)
	{
		try
		{
			if (this.valueType == GamePrefs.EnumType.Int)
			{
				int num = (int)_value;
				bool flag = false;
				for (int i = 1; i < this.controlCombo.Elements.Count; i++)
				{
					if (this.controlCombo.Elements[i].Value == num)
					{
						this.controlCombo.SelectedIndex = i;
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					this.controlCombo.SelectedIndex = this.controlCombo.MinIndex;
				}
			}
			else if (this.valueType == GamePrefs.EnumType.Bool)
			{
				this.controlCombo.SelectedIndex = (((bool)_value) ? 2 : 1);
			}
		}
		catch (Exception e)
		{
			Log.Exception(e);
		}
	}

	public XUiC_ServersList.UiServerFilter GetFilter()
	{
		int value = this.controlCombo.Value.Value;
		bool flag = value == int.MinValue;
		GamePrefs.EnumType enumType = this.valueType;
		string name;
		if (enumType != GamePrefs.EnumType.Int)
		{
			if (enumType != GamePrefs.EnumType.Bool)
			{
				throw new ArgumentOutOfRangeException();
			}
			name = this.gameInfoBool.ToStringCached<GameInfoBool>();
		}
		else
		{
			name = this.gameInfoInt.ToStringCached<GameInfoInt>();
		}
		if (flag)
		{
			return new XUiC_ServersList.UiServerFilter(name, XUiC_ServersList.EnumServerLists.Regular, null, IServerListInterface.ServerFilter.EServerFilterType.Any, 0, 0, false, null);
		}
		int intMinValue = 0;
		bool boolValue = false;
		enumType = this.valueType;
		Func<XUiC_ServersList.ListEntry, bool> func;
		IServerListInterface.ServerFilter.EServerFilterType type;
		if (enumType != GamePrefs.EnumType.Int)
		{
			if (enumType != GamePrefs.EnumType.Bool)
			{
				throw new ArgumentOutOfRangeException();
			}
			bool bValue = value == 1;
			func = ((XUiC_ServersList.ListEntry _entry) => _entry.gameServerInfo.GetValue(this.gameInfoBool) == bValue);
			type = IServerListInterface.ServerFilter.EServerFilterType.BoolValue;
			boolValue = bValue;
		}
		else
		{
			int iValue = value;
			func = ((XUiC_ServersList.ListEntry _entry) => _entry.gameServerInfo.GetValue(this.gameInfoInt) == iValue);
			type = IServerListInterface.ServerFilter.EServerFilterType.IntValue;
			intMinValue = iValue;
		}
		return new XUiC_ServersList.UiServerFilter(name, XUiC_ServersList.EnumServerLists.Regular, func, type, intMinValue, 0, boolValue, null);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<XUiC_ServerBrowserGamePrefSelector.GameOptionValue> controlCombo;

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] valuesFromXml;

	[PublicizedFrom(EAccessModifier.Private)]
	public string[] namesFromXml;

	[PublicizedFrom(EAccessModifier.Private)]
	public string valueLocalizationPrefixFromXml;

	[PublicizedFrom(EAccessModifier.Private)]
	public GamePrefs.EnumType valueType;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameInfoBool gameInfoBool;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameInfoInt gameInfoInt;

	[PublicizedFrom(EAccessModifier.Private)]
	public Func<int, int> valuePreDisplayModifierFunc;

	public Action<XUiC_ServerBrowserGamePrefSelector> OnValueChanged;

	public struct GameOptionValue
	{
		public GameOptionValue(int _value, string _displayName)
		{
			this.Value = _value;
			this.DisplayName = _displayName;
		}

		public override string ToString()
		{
			return this.DisplayName;
		}

		public readonly int Value;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string DisplayName;
	}
}

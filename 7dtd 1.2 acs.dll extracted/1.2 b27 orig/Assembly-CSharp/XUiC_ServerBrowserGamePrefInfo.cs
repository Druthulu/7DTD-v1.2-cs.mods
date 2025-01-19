using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ServerBrowserGamePrefInfo : XUiController
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

	public GameInfoString GameInfoString
	{
		get
		{
			return this.gameInfoString;
		}
	}

	public Func<int, int> ValuePreDisplayModifierFunc
	{
		set
		{
			if (this.valuePreDisplayModifierFunc != value)
			{
				this.valuePreDisplayModifierFunc = value;
				this.SetupOptions();
			}
		}
	}

	public Func<GameServerInfo, int, string> CustomIntValueFormatter
	{
		set
		{
			if (this.customIntValueFormatter != value)
			{
				this.customIntValueFormatter = value;
				this.IsDirty = true;
			}
		}
	}

	public Func<GameServerInfo, string, string> CustomStringValueFormatter
	{
		set
		{
			if (this.customStringValueFormatter != value)
			{
				this.customStringValueFormatter = value;
				this.IsDirty = true;
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
		if (EnumUtils.TryParse<GameInfoString>(this.viewComponent.ID, out this.gameInfoString, false))
		{
			this.valueType = GamePrefs.EnumType.String;
		}
		else
		{
			this.gameInfoString = (GameInfoString)(-1);
		}
		this.label = (XUiV_Label)base.GetChildById("value").ViewComponent;
		this.SetupOptions();
	}

	public override bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
		if (_name == "display_names")
		{
			if (_value.Length > 0)
			{
				this.namesFromXml = _value.Split(',', StringSplitOptions.None);
				for (int i = 0; i < this.namesFromXml.Length; i++)
				{
					this.namesFromXml[i] = this.namesFromXml[i].Trim();
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
	public void SetupOptions()
	{
		if (this.valueType == GamePrefs.EnumType.Int)
		{
			if (this.namesFromXml != null)
			{
				this.values = new List<XUiC_ServerBrowserGamePrefInfo.GameOptionValue>();
				for (int i = 0; i < this.namesFromXml.Length; i++)
				{
					string text = this.namesFromXml[i];
					int value = i;
					if (text.IndexOf('=') > 0)
					{
						string[] array = text.Split('=', StringSplitOptions.None);
						text = array[1];
						value = StringParsers.ParseSInt32(array[0], 0, -1, NumberStyles.Integer);
					}
					this.values.Add(new XUiC_ServerBrowserGamePrefInfo.GameOptionValue(value, Localization.Get(text, false)));
				}
				return;
			}
		}
		else if (this.valueType == GamePrefs.EnumType.Bool)
		{
			this.values = new List<XUiC_ServerBrowserGamePrefInfo.GameOptionValue>
			{
				new XUiC_ServerBrowserGamePrefInfo.GameOptionValue(0, Localization.Get("goOff", false)),
				new XUiC_ServerBrowserGamePrefInfo.GameOptionValue(1, Localization.Get("goOn", false))
			};
		}
	}

	public void SetCurrentValue(GameServerInfo _gameInfo)
	{
		try
		{
			if (_gameInfo != null)
			{
				if (this.valueType == GamePrefs.EnumType.Int)
				{
					int value = _gameInfo.GetValue(this.gameInfoInt);
					bool flag = false;
					if (this.values != null)
					{
						foreach (XUiC_ServerBrowserGamePrefInfo.GameOptionValue gameOptionValue in this.values)
						{
							if (gameOptionValue.Value == value)
							{
								this.label.Text = gameOptionValue.ToString();
								flag = true;
								break;
							}
						}
					}
					if (!flag)
					{
						if (this.customIntValueFormatter != null)
						{
							this.label.Text = this.customIntValueFormatter(_gameInfo, value);
						}
						else if (this.valueLocalizationPrefixFromXml != null)
						{
							this.label.Text = string.Format(Localization.Get(this.valueLocalizationPrefixFromXml + ((value == 1) ? "" : "s"), false), value);
						}
						else
						{
							this.label.Text = value.ToString();
						}
					}
				}
				else if (this.valueType == GamePrefs.EnumType.Bool)
				{
					bool value2 = _gameInfo.GetValue(this.gameInfoBool);
					this.label.Text = (value2 ? this.values[1].ToString() : this.values[0].ToString());
				}
				else if (this.valueType == GamePrefs.EnumType.String)
				{
					string value3 = _gameInfo.GetValue(this.gameInfoString);
					if (this.customStringValueFormatter != null)
					{
						this.label.Text = this.customStringValueFormatter(_gameInfo, value3);
					}
					else if (this.valueLocalizationPrefixFromXml != null && !string.IsNullOrEmpty(value3))
					{
						this.label.Text = Localization.Get(this.valueLocalizationPrefixFromXml + value3, false);
					}
					else
					{
						this.label.Text = value3;
					}
				}
			}
			else
			{
				this.label.Text = "";
			}
		}
		catch (Exception e)
		{
			Log.Exception(e);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label label;

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
	public GameInfoString gameInfoString;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<XUiC_ServerBrowserGamePrefInfo.GameOptionValue> values;

	[PublicizedFrom(EAccessModifier.Private)]
	public Func<int, int> valuePreDisplayModifierFunc;

	[PublicizedFrom(EAccessModifier.Private)]
	public Func<GameServerInfo, int, string> customIntValueFormatter;

	[PublicizedFrom(EAccessModifier.Private)]
	public Func<GameServerInfo, string, string> customStringValueFormatter;

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

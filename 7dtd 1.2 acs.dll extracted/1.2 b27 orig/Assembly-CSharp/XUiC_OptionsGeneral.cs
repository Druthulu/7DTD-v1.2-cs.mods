using System;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_OptionsGeneral : XUiController
{
	public static event Action OnSettingsChanged;

	public override void Init()
	{
		base.Init();
		XUiC_OptionsGeneral.ID = base.WindowGroup.ID;
		this.comboLanguage = base.GetChildById("Language").GetChildByType<XUiC_ComboBoxList<XUiC_OptionsGeneral.LanguageInfo>>();
		this.comboUseEnglishCompass = base.GetChildById("UseEnglishCompass").GetChildByType<XUiC_ComboBoxBool>();
		this.comboTempCelsius = base.GetChildById("TempCelsius").GetChildByType<XUiC_ComboBoxBool>();
		this.comboDisableXmlEvents = base.GetChildById("DisableXmlEvents").GetChildByType<XUiC_ComboBoxBool>();
		this.comboQuestsAutoShare = base.GetChildById("QuestsAutoShare").GetChildByType<XUiC_ComboBoxBool>();
		this.comboQuestsAutoAccept = base.GetChildById("QuestsAutoAccept").GetChildByType<XUiC_ComboBoxBool>();
		this.comboLnlMtuWorkaround = base.GetChildById("LnlMtuWorkaround").GetChildByType<XUiC_ComboBoxBool>();
		this.comboTxtChat = base.GetChildById("ChatComms").GetChildByType<XUiC_ComboBoxBool>();
		this.comboCrossplay = base.GetChildById("Crossplay").GetChildByType<XUiC_ComboBoxBool>();
		this.comboShowConsoleButton = base.GetChildById("ShowConsoleButton").GetChildByType<XUiC_ComboBoxBool>();
		this.comboLanguage.OnValueChangedGeneric += this.anyOtherValueChanged;
		this.comboUseEnglishCompass.OnValueChangedGeneric += this.anyOtherValueChanged;
		this.comboTempCelsius.OnValueChangedGeneric += this.anyOtherValueChanged;
		this.comboDisableXmlEvents.OnValueChangedGeneric += this.anyOtherValueChanged;
		this.comboQuestsAutoShare.OnValueChangedGeneric += this.anyOtherValueChanged;
		this.comboQuestsAutoAccept.OnValueChangedGeneric += this.anyOtherValueChanged;
		this.comboLnlMtuWorkaround.OnValueChangedGeneric += this.anyOtherValueChanged;
		this.comboTxtChat.OnValueChangedGeneric += this.anyOtherValueChanged;
		this.comboCrossplay.OnValueChangedGeneric += this.anyOtherValueChanged;
		this.comboShowConsoleButton.OnValueChangedGeneric += this.anyOtherValueChanged;
		((XUiC_SimpleButton)base.GetChildById("btnEula")).OnPressed += this.BtnEula_OnPressed;
		((XUiC_SimpleButton)base.GetChildById("btnBugReport")).OnPressed += this.BtnBugReport_OnPressed;
		((XUiC_SimpleButton)base.GetChildById("btnBack")).OnPressed += this.BtnBack_OnPressed;
		((XUiC_SimpleButton)base.GetChildById("btnDefaults")).OnPressed += this.BtnDefaults_OnOnPressed;
		this.btnApply = (XUiC_SimpleButton)base.GetChildById("btnApply");
		this.btnApply.OnPressed += this.BtnApply_OnPressed;
		this.RefreshApplyLabel();
		base.RegisterForInputStyleChanges();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshApplyLabel()
	{
		InControlExtensions.SetApplyButtonString(this.btnApply, "xuiApply");
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void InputStyleChanged(PlayerInputManager.InputStyle _oldStyle, PlayerInputManager.InputStyle _newStyle)
	{
		base.InputStyleChanged(_oldStyle, _newStyle);
		this.RefreshApplyLabel();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void anyOtherValueChanged(XUiController _sender)
	{
		this.btnApply.Enabled = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnApply_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.applyChanges();
		this.btnApply.Enabled = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDefaults_OnOnPressed(XUiController _sender, int _mouseButton)
	{
		this.comboLanguage.SelectedIndex = 0;
		this.comboUseEnglishCompass.Value = (bool)GamePrefs.GetDefault(EnumGamePrefs.OptionsUiCompassUseEnglishCardinalDirections);
		this.comboTempCelsius.Value = (bool)GamePrefs.GetDefault(EnumGamePrefs.OptionsTempCelsius);
		this.comboDisableXmlEvents.Value = (bool)GamePrefs.GetDefault(EnumGamePrefs.OptionsDisableXmlEvents);
		this.comboQuestsAutoShare.Value = (bool)GamePrefs.GetDefault(EnumGamePrefs.OptionsQuestsAutoShare);
		this.comboQuestsAutoAccept.Value = (bool)GamePrefs.GetDefault(EnumGamePrefs.OptionsQuestsAutoAccept);
		this.comboLnlMtuWorkaround.Value = (bool)GamePrefs.GetDefault(EnumGamePrefs.OptionsLiteNetLibMtuOverride);
		this.comboShowConsoleButton.Value = (bool)GamePrefs.GetDefault(EnumGamePrefs.OptionsShowConsoleButton);
		this.comboTxtChat.Value = (this.otherPerms.HasCommunication() && (bool)GamePrefs.GetDefault(EnumGamePrefs.OptionsChatCommunication));
		this.comboCrossplay.Value = (this.otherPerms.HasCrossplay() && (bool)GamePrefs.GetDefault(EnumGamePrefs.OptionsCrossplay));
		this.btnApply.Enabled = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnBack_OnPressed(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(this.windowGroup.ID);
		base.xui.playerUI.windowManager.Open(XUiC_OptionsMenu.ID, true, false, true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnEula_OnPressed(XUiController _sender, int _mouseButton)
	{
		XUiC_EulaWindow.Open(base.xui, GameManager.HasAcceptedLatestEula());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnBugReport_OnPressed(XUiController _sender, int _mouseButton)
	{
		XUiC_BugReportWindow.Open(base.xui, true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateLanguageList()
	{
		this.comboLanguage.Elements.Clear();
		bool flag = false;
		foreach (string text in Localization.knownLanguages)
		{
			if ((flag || (flag = text.EqualsCaseInsensitive(Localization.DefaultLanguage))) && text.IndexOf(' ') < 0)
			{
				this.comboLanguage.Elements.Add(new XUiC_OptionsGeneral.LanguageInfo(text));
			}
		}
		this.comboLanguage.Elements.Sort();
		this.comboLanguage.Elements.Insert(0, new XUiC_OptionsGeneral.LanguageInfo(""));
		string @string = GamePrefs.GetString(EnumGamePrefs.Language);
		for (int j = 0; j < this.comboLanguage.Elements.Count; j++)
		{
			if (this.comboLanguage.Elements[j].LanguageKey.EqualsCaseInsensitive(@string))
			{
				this.comboLanguage.SelectedIndex = j;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void applyChanges()
	{
		bool flag = this.comboLanguage.Value.LanguageKey != GamePrefs.GetString(EnumGamePrefs.Language);
		if (flag)
		{
			Log.Out("Language selection changed: " + this.comboLanguage.Value.LanguageKey);
			GamePrefs.Set(EnumGamePrefs.Language, this.comboLanguage.Value.LanguageKey);
		}
		GamePrefs.Set(EnumGamePrefs.OptionsUiCompassUseEnglishCardinalDirections, this.comboUseEnglishCompass.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsTempCelsius, this.comboTempCelsius.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsDisableXmlEvents, this.comboDisableXmlEvents.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsQuestsAutoShare, this.comboQuestsAutoShare.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsQuestsAutoAccept, this.comboQuestsAutoAccept.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsLiteNetLibMtuOverride, this.comboLnlMtuWorkaround.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsShowConsoleButton, this.comboShowConsoleButton.Value);
		if (this.otherPerms.HasCommunication())
		{
			GamePrefs.Set(EnumGamePrefs.OptionsChatCommunication, this.comboTxtChat.Value);
		}
		if (this.otherPerms.HasCrossplay())
		{
			GamePrefs.Set(EnumGamePrefs.OptionsCrossplay, this.comboCrossplay.Value);
		}
		GamePrefs.Instance.Save();
		Action onSettingsChanged = XUiC_OptionsGeneral.OnSettingsChanged;
		if (onSettingsChanged != null)
		{
			onSettingsChanged();
		}
		if (flag)
		{
			if ((DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX).IsCurrent())
			{
				XUiC_MessageBoxWindowGroup.ShowMessageBox(base.xui, Localization.Get("xuiConfirmRestartLanguageTitle", false), Localization.Get("xuiConfirmRestartLanguageText", false), XUiC_MessageBoxWindowGroup.MessageBoxTypes.OkCancel, delegate()
				{
					Utils.RestartGame(Utils.ERestartAntiCheatMode.KeepAntiCheatMode);
				}, delegate()
				{
					base.xui.playerUI.windowManager.Open(XUiC_OptionsGeneral.ID, true, false, true);
				}, false, true);
				return;
			}
			if ((DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent())
			{
				XUiC_MessageBoxWindowGroup.ShowMessageBox(base.xui, Localization.Get("xuiLanguageChangedTitle", false), Localization.Get("xuiLanguageChangedText", false), XUiC_MessageBoxWindowGroup.MessageBoxTypes.Ok, null, delegate()
				{
					base.xui.playerUI.windowManager.Open(XUiC_OptionsGeneral.ID, true, false, true);
				}, false, true);
			}
		}
	}

	public override void OnOpen()
	{
		this.otherPerms = PermissionsManager.GetPermissions(PermissionsManager.PermissionSources.Platform | PermissionsManager.PermissionSources.LaunchPrefs | PermissionsManager.PermissionSources.DebugMask | PermissionsManager.PermissionSources.TitleStorage);
		base.WindowGroup.openWindowOnEsc = XUiC_OptionsMenu.ID;
		this.updateLanguageList();
		this.comboUseEnglishCompass.Value = GamePrefs.GetBool(EnumGamePrefs.OptionsUiCompassUseEnglishCardinalDirections);
		this.comboTempCelsius.Value = GamePrefs.GetBool(EnumGamePrefs.OptionsTempCelsius);
		this.comboDisableXmlEvents.Value = GamePrefs.GetBool(EnumGamePrefs.OptionsDisableXmlEvents);
		this.comboQuestsAutoShare.Value = GamePrefs.GetBool(EnumGamePrefs.OptionsQuestsAutoShare);
		this.comboQuestsAutoAccept.Value = GamePrefs.GetBool(EnumGamePrefs.OptionsQuestsAutoAccept);
		this.comboLnlMtuWorkaround.Value = GamePrefs.GetBool(EnumGamePrefs.OptionsLiteNetLibMtuOverride);
		this.comboShowConsoleButton.Value = GamePrefs.GetBool(EnumGamePrefs.OptionsShowConsoleButton);
		this.comboTxtChat.Value = (this.otherPerms.HasCommunication() && GamePrefs.GetBool(EnumGamePrefs.OptionsChatCommunication));
		this.comboTxtChat.Enabled = this.otherPerms.HasCommunication();
		this.comboCrossplay.Value = (this.otherPerms.HasCrossplay() && GamePrefs.GetBool(EnumGamePrefs.OptionsCrossplay));
		this.comboCrossplay.Enabled = this.otherPerms.HasCrossplay();
		base.OnOpen();
		this.btnApply.Enabled = false;
		base.RefreshBindings(false);
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.btnApply.Enabled && base.xui.playerUI.playerInput.GUIActions.Apply.WasPressed)
		{
			this.BtnApply_OnPressed(null, 0);
		}
	}

	public override bool GetBindingValue(ref string _value, string _bindingName)
	{
		if (_bindingName == "crossplayTooltip")
		{
			_value = (PermissionsManager.GetPermissionDenyReason(EUserPerms.Crossplay, PermissionsManager.PermissionSources.Platform | PermissionsManager.PermissionSources.LaunchPrefs | PermissionsManager.PermissionSources.DebugMask | PermissionsManager.PermissionSources.TitleStorage) ?? Localization.Get("xuiOptionsGeneralCrossplayTooltip", false));
			return true;
		}
		if (!(_bindingName == "bug_reporting"))
		{
			return base.GetBindingValue(ref _value, _bindingName);
		}
		_value = BacktraceUtils.BugReportFeature.ToString();
		return true;
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<XUiC_OptionsGeneral.LanguageInfo> comboLanguage;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboUseEnglishCompass;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboTempCelsius;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboDisableXmlEvents;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboQuestsAutoShare;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboQuestsAutoAccept;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboLnlMtuWorkaround;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboTxtChat;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboCrossplay;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboShowConsoleButton;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnApply;

	[PublicizedFrom(EAccessModifier.Private)]
	public EUserPerms otherPerms;

	public struct LanguageInfo : IComparable<XUiC_OptionsGeneral.LanguageInfo>, IEquatable<XUiC_OptionsGeneral.LanguageInfo>, IComparable
	{
		public LanguageInfo(string _languageKey)
		{
			this.LanguageKey = _languageKey;
			if (_languageKey == "")
			{
				this.NameEnglish = null;
				this.NameNative = null;
				return;
			}
			this.NameEnglish = Localization.Get("languageNameEnglish", _languageKey, false);
			this.NameNative = Localization.Get("languageNameNative", _languageKey, false);
		}

		public override string ToString()
		{
			if (!(this.LanguageKey == ""))
			{
				return this.NameEnglish + " / " + this.NameNative;
			}
			return "-Auto-";
		}

		public bool Equals(XUiC_OptionsGeneral.LanguageInfo _other)
		{
			return this.LanguageKey == _other.LanguageKey;
		}

		public override bool Equals(object _obj)
		{
			if (_obj is XUiC_OptionsGeneral.LanguageInfo)
			{
				XUiC_OptionsGeneral.LanguageInfo other = (XUiC_OptionsGeneral.LanguageInfo)_obj;
				return this.Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			if (this.LanguageKey == null)
			{
				return 0;
			}
			return this.LanguageKey.GetHashCode();
		}

		public int CompareTo(XUiC_OptionsGeneral.LanguageInfo _other)
		{
			return string.Compare(this.NameEnglish, _other.NameEnglish, StringComparison.OrdinalIgnoreCase);
		}

		public int CompareTo(object _obj)
		{
			if (_obj == null)
			{
				return 1;
			}
			if (_obj is XUiC_OptionsGeneral.LanguageInfo)
			{
				XUiC_OptionsGeneral.LanguageInfo other = (XUiC_OptionsGeneral.LanguageInfo)_obj;
				return this.CompareTo(other);
			}
			throw new ArgumentException("Object must be of type LanguageInfo");
		}

		public readonly string NameEnglish;

		public readonly string NameNative;

		public readonly string LanguageKey;
	}
}

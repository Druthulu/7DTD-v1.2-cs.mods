using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_OptionsAudio : XUiController
{
	public static event Action OnSettingsChanged;

	public bool voiceAvailable
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return PlatformManager.MultiPlatform.PartyVoice != null && PlatformManager.MultiPlatform.PartyVoice.Status == EPartyVoiceStatus.Ok;
		}
	}

	public override void Init()
	{
		base.Init();
		XUiC_OptionsAudio.ID = base.WindowGroup.ID;
		this.comboOverallAudioVolumeLevel = base.GetChildById("OverallAudioVolumeLevel").GetChildByType<XUiC_ComboBoxFloat>();
		this.comboDynamicMusicEnabled = base.GetChildById("DynamicMusicEnabled").GetChildByType<XUiC_ComboBoxBool>();
		this.comboDynamicMusicDailyTimeAllotted = base.GetChildById("DynamicMusicDailyTimeAllotted").GetChildByType<XUiC_ComboBoxFloat>();
		this.comboAmbientVolumeLevel = base.GetChildById("AmbientVolumeLevel").GetChildByType<XUiC_ComboBoxFloat>();
		this.comboInGameMusicVolumeLevel = base.GetChildById("InGameMusicVolumeLevel").GetChildByType<XUiC_ComboBoxFloat>();
		this.comboMenuMusicVolumeLevel = base.GetChildById("MenuMusicVolumeLevel").GetChildByType<XUiC_ComboBoxFloat>();
		this.comboProfanityFilter = base.GetChildById("ProfanityFilter").GetChildByType<XUiC_ComboBoxBool>();
		this.comboOverallAudioVolumeLevel.OnValueChanged += this.ComboOverallAudioVolumeLevelOnOnValueChanged;
		this.comboDynamicMusicEnabled.OnValueChanged += this.ComboDynamicMusicEnabledOnOnValueChanged;
		this.comboDynamicMusicDailyTimeAllotted.OnValueChanged += this.ComboDynamicMusicDailyTimeAllottedOnOnValueChanged;
		this.comboAmbientVolumeLevel.OnValueChanged += this.ComboAmbientVolumeLevelOnOnValueChanged;
		this.comboInGameMusicVolumeLevel.OnValueChanged += this.ComboInGameMusicVolumeLevelOnOnValueChanged;
		this.comboMenuMusicVolumeLevel.OnValueChanged += this.ComboMenuMusicVolumeLevelOnOnValueChanged;
		this.comboProfanityFilter.OnValueChanged += this.ComboSubtitlesEnabledValuesChanged;
		this.comboOverallAudioVolumeLevel.Min = 0.0;
		this.comboOverallAudioVolumeLevel.Max = 1.0;
		this.comboAmbientVolumeLevel.Min = 0.0;
		this.comboAmbientVolumeLevel.Max = 1.0;
		this.comboDynamicMusicDailyTimeAllotted.Min = 0.0;
		this.comboDynamicMusicDailyTimeAllotted.Max = 1.0;
		this.comboInGameMusicVolumeLevel.Min = 0.0;
		this.comboInGameMusicVolumeLevel.Max = 1.0;
		this.comboMenuMusicVolumeLevel.Min = 0.0;
		this.comboMenuMusicVolumeLevel.Max = 1.0;
		if (!(DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent())
		{
			this.comboMumblePositionalAudioSupportEnabled = base.GetChildById("MumblePositionalAudioSupportEnabled").GetChildByType<XUiC_ComboBoxBool>();
			this.comboMumblePositionalAudioSupportEnabled.OnValueChanged += this.ComboMumblePositionalAudioSupportEnabledOnValueChanged;
		}
		if (this.voiceAvailable)
		{
			this.comboVoiceChatEnabled = base.GetChildById("VoiceChatEnabled").GetChildByType<XUiC_ComboBoxBool>();
			XUiController childById = base.GetChildById("VoiceOutputDevice");
			this.comboVoiceOutputDevice = ((childById != null) ? childById.GetChildByType<XUiC_ComboBoxList<IPartyVoice.VoiceAudioDevice>>() : null);
			XUiController childById2 = base.GetChildById("VoiceInputDevice");
			this.comboVoiceInputDevice = ((childById2 != null) ? childById2.GetChildByType<XUiC_ComboBoxList<IPartyVoice.VoiceAudioDevice>>() : null);
			this.comboVoiceVolumeLevel = base.GetChildById("VoiceVolumeLevel").GetChildByType<XUiC_ComboBoxFloat>();
			this.comboVoiceChatEnabled.OnValueChanged += this.ComboVoiceChatEnabledOnOnValueChanged;
			if (this.comboVoiceOutputDevice != null)
			{
				this.comboVoiceOutputDevice.OnValueChanged += this.ComboVoiceDeviceOnValueChanged;
			}
			if (this.comboVoiceInputDevice != null)
			{
				this.comboVoiceInputDevice.OnValueChanged += this.ComboVoiceDeviceOnValueChanged;
			}
			this.comboVoiceVolumeLevel.OnValueChanged += this.ComboVoiceVolumeLevelOnOnValueChanged;
			this.comboVoiceVolumeLevel.Min = 0.0;
			this.comboVoiceVolumeLevel.Max = 2.0;
		}
		((XUiC_SimpleButton)base.GetChildById("btnBack")).OnPressed += this.BtnBack_OnPressed;
		((XUiC_SimpleButton)base.GetChildById("btnDefaults")).OnPressed += this.BtnDefaults_OnOnPressed;
		this.btnApply = (XUiC_SimpleButton)base.GetChildById("btnApply");
		this.btnApply.OnPressed += this.BtnApply_OnPressed;
		base.RegisterForInputStyleChanges();
		this.RefreshApplyLabel();
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

	[PublicizedFrom(EAccessModifier.Private)]
	public void ComboOverallAudioVolumeLevelOnOnValueChanged(XUiController _sender, double _oldValue, double _newValue)
	{
		GamePrefs.Set(EnumGamePrefs.OptionsOverallAudioVolumeLevel, (float)this.comboOverallAudioVolumeLevel.Value);
		AudioListener.volume = GamePrefs.GetFloat(EnumGamePrefs.OptionsOverallAudioVolumeLevel);
		this.btnApply.Enabled = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ComboDynamicMusicEnabledOnOnValueChanged(XUiController _sender, bool _oldValue, bool _newValue)
	{
		GamePrefs.Set(EnumGamePrefs.OptionsDynamicMusicEnabled, this.comboDynamicMusicEnabled.Value);
		this.btnApply.Enabled = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ComboDynamicMusicDailyTimeAllottedOnOnValueChanged(XUiController _sender, double _oldValue, double _newValue)
	{
		GamePrefs.Set(EnumGamePrefs.OptionsDynamicMusicDailyTime, (float)this.comboDynamicMusicDailyTimeAllotted.Value);
		this.btnApply.Enabled = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ComboAmbientVolumeLevelOnOnValueChanged(XUiController _sender, double _oldValue, double _newValue)
	{
		GamePrefs.Set(EnumGamePrefs.OptionsAmbientVolumeLevel, (float)this.comboAmbientVolumeLevel.Value);
		this.btnApply.Enabled = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ComboInGameMusicVolumeLevelOnOnValueChanged(XUiController _sender, double _oldValue, double _newValue)
	{
		GamePrefs.Set(EnumGamePrefs.OptionsMusicVolumeLevel, (float)this.comboInGameMusicVolumeLevel.Value);
		this.btnApply.Enabled = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ComboMenuMusicVolumeLevelOnOnValueChanged(XUiController _sender, double _oldValue, double _newValue)
	{
		GamePrefs.Set(EnumGamePrefs.OptionsMenuMusicVolumeLevel, (float)this.comboMenuMusicVolumeLevel.Value);
		this.btnApply.Enabled = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ComboMumblePositionalAudioSupportEnabledOnValueChanged(XUiController _sender, bool _oldvalue, bool _newvalue)
	{
		this.btnApply.Enabled = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ComboVoiceChatEnabledOnOnValueChanged(XUiController _sender, bool _oldValue, bool _newValue)
	{
		GamePrefs.Set(EnumGamePrefs.OptionsVoiceChatEnabled, this.comboVoiceChatEnabled.Value);
		this.btnApply.Enabled = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ComboVoiceDeviceOnValueChanged(XUiController _sender, IPartyVoice.VoiceAudioDevice _oldvalue, IPartyVoice.VoiceAudioDevice _newvalue)
	{
		this.btnApply.Enabled = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ComboVoiceVolumeLevelOnOnValueChanged(XUiController _sender, double _oldValue, double _newValue)
	{
		GamePrefs.Set(EnumGamePrefs.OptionsVoiceVolumeLevel, (float)this.comboVoiceVolumeLevel.Value);
		this.btnApply.Enabled = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ComboSubtitlesEnabledValuesChanged(XUiController _sender, bool _oldValue, bool _newValue)
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
		GameOptionsManager.ResetGameOptions(GameOptionsManager.ResetType.Audio);
		this.updateOptions();
		this.applyChanges();
		this.btnApply.Enabled = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnBack_OnPressed(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(this.windowGroup.ID);
		base.xui.playerUI.windowManager.Open(XUiC_OptionsMenu.ID, true, false, true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateVoiceDevices()
	{
		if (!this.voiceAvailable)
		{
			return;
		}
		if (!PermissionsManager.IsCommunicationAllowed())
		{
			return;
		}
		ValueTuple<IList<IPartyVoice.VoiceAudioDevice>, IList<IPartyVoice.VoiceAudioDevice>> devicesList = PlatformManager.MultiPlatform.PartyVoice.GetDevicesList();
		IList<IPartyVoice.VoiceAudioDevice> item = devicesList.Item1;
		IList<IPartyVoice.VoiceAudioDevice> item2 = devicesList.Item2;
		XUiC_OptionsAudio.<updateVoiceDevices>g__SelectActiveDevice|50_0(item, this.comboVoiceInputDevice, EnumGamePrefs.OptionsVoiceInputDevice);
		XUiC_OptionsAudio.<updateVoiceDevices>g__SelectActiveDevice|50_0(item2, this.comboVoiceOutputDevice, EnumGamePrefs.OptionsVoiceOutputDevice);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateOptions()
	{
		this.comboOverallAudioVolumeLevel.Value = (double)(this.origOverallAudioVolumeLevel = GamePrefs.GetFloat(EnumGamePrefs.OptionsOverallAudioVolumeLevel));
		this.comboDynamicMusicEnabled.Value = (this.origDynamicMusicEnabled = GamePrefs.GetBool(EnumGamePrefs.OptionsDynamicMusicEnabled));
		this.comboDynamicMusicDailyTimeAllotted.Value = (double)(this.origDynamicMusicDailyTimeAllotted = GamePrefs.GetFloat(EnumGamePrefs.OptionsDynamicMusicDailyTime));
		this.comboAmbientVolumeLevel.Value = (double)(this.origAmbientVolumeLevel = GamePrefs.GetFloat(EnumGamePrefs.OptionsAmbientVolumeLevel));
		this.comboInGameMusicVolumeLevel.Value = (double)(this.origInGameMusicVolumeLevel = GamePrefs.GetFloat(EnumGamePrefs.OptionsMusicVolumeLevel));
		this.comboMenuMusicVolumeLevel.Value = (double)(this.origMenuMusicVolumeLevel = GamePrefs.GetFloat(EnumGamePrefs.OptionsMenuMusicVolumeLevel));
		this.comboProfanityFilter.Value = (this.origProfanityFilter = GamePrefs.GetBool(EnumGamePrefs.OptionsFilterProfanity));
		if (!(DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent())
		{
			this.comboMumblePositionalAudioSupportEnabled.Value = (this.origMumblePositionalAudioSupportEnabled = GamePrefs.GetBool(EnumGamePrefs.OptionsMumblePositionalAudioSupport));
		}
		if (this.voiceAvailable)
		{
			this.comboVoiceChatEnabled.Value = (this.origVoiceChatEnabled = GamePrefs.GetBool(EnumGamePrefs.OptionsVoiceChatEnabled));
			this.comboVoiceVolumeLevel.Value = (double)(this.origVoiceVolumeLevel = GamePrefs.GetFloat(EnumGamePrefs.OptionsVoiceVolumeLevel));
			this.updateVoiceDevices();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void applyChanges()
	{
		AudioListener.volume = GamePrefs.GetFloat(EnumGamePrefs.OptionsOverallAudioVolumeLevel);
		this.origOverallAudioVolumeLevel = (float)this.comboOverallAudioVolumeLevel.Value;
		this.origDynamicMusicEnabled = this.comboDynamicMusicEnabled.Value;
		this.origDynamicMusicDailyTimeAllotted = (float)this.comboDynamicMusicDailyTimeAllotted.Value;
		this.origAmbientVolumeLevel = (float)this.comboAmbientVolumeLevel.Value;
		this.origInGameMusicVolumeLevel = (float)this.comboInGameMusicVolumeLevel.Value;
		this.origMenuMusicVolumeLevel = (float)this.comboMenuMusicVolumeLevel.Value;
		this.origProfanityFilter = this.comboProfanityFilter.Value;
		if (!(DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent())
		{
			this.origMumblePositionalAudioSupportEnabled = this.comboMumblePositionalAudioSupportEnabled.Value;
			GamePrefs.Set(EnumGamePrefs.OptionsMumblePositionalAudioSupport, this.origMumblePositionalAudioSupportEnabled);
		}
		if (this.voiceAvailable)
		{
			this.origVoiceChatEnabled = this.comboVoiceChatEnabled.Value;
			this.origVoiceVolumeLevel = (float)this.comboVoiceVolumeLevel.Value;
			XUiC_ComboBoxList<IPartyVoice.VoiceAudioDevice> xuiC_ComboBoxList = this.comboVoiceInputDevice;
			if (((xuiC_ComboBoxList != null) ? xuiC_ComboBoxList.Value : null) != null)
			{
				GamePrefs.Set(EnumGamePrefs.OptionsVoiceInputDevice, this.comboVoiceInputDevice.Value.Identifier);
			}
			XUiC_ComboBoxList<IPartyVoice.VoiceAudioDevice> xuiC_ComboBoxList2 = this.comboVoiceOutputDevice;
			if (((xuiC_ComboBoxList2 != null) ? xuiC_ComboBoxList2.Value : null) != null)
			{
				GamePrefs.Set(EnumGamePrefs.OptionsVoiceOutputDevice, this.comboVoiceOutputDevice.Value.Identifier);
			}
			this.updateVoiceDevices();
		}
		GamePrefs.Instance.Save();
		if (XUiC_OptionsAudio.OnSettingsChanged != null)
		{
			XUiC_OptionsAudio.OnSettingsChanged();
		}
	}

	public override void OnOpen()
	{
		base.WindowGroup.openWindowOnEsc = XUiC_OptionsMenu.ID;
		this.updateOptions();
		base.OnOpen();
		this.btnApply.Enabled = false;
		base.RefreshBindings(false);
		this.RefreshApplyLabel();
	}

	public override void OnClose()
	{
		base.OnClose();
		GamePrefs.Set(EnumGamePrefs.OptionsOverallAudioVolumeLevel, this.origOverallAudioVolumeLevel);
		AudioListener.volume = this.origOverallAudioVolumeLevel;
		GamePrefs.Set(EnumGamePrefs.OptionsDynamicMusicEnabled, this.origDynamicMusicEnabled);
		GamePrefs.Set(EnumGamePrefs.OptionsDynamicMusicDailyTime, this.origDynamicMusicDailyTimeAllotted);
		GamePrefs.Set(EnumGamePrefs.OptionsAmbientVolumeLevel, this.origAmbientVolumeLevel);
		GamePrefs.Set(EnumGamePrefs.OptionsMusicVolumeLevel, this.origInGameMusicVolumeLevel);
		GamePrefs.Set(EnumGamePrefs.OptionsMenuMusicVolumeLevel, this.origMenuMusicVolumeLevel);
		GamePrefs.Set(EnumGamePrefs.OptionsFilterProfanity, this.origProfanityFilter);
		if (!(DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent())
		{
			GamePrefs.Set(EnumGamePrefs.OptionsMumblePositionalAudioSupport, this.origMumblePositionalAudioSupportEnabled);
		}
		if (this.voiceAvailable)
		{
			GamePrefs.Set(EnumGamePrefs.OptionsVoiceChatEnabled, this.origVoiceChatEnabled);
			GamePrefs.Set(EnumGamePrefs.OptionsVoiceVolumeLevel, this.origVoiceVolumeLevel);
		}
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
		if (_bindingName == "notingame")
		{
			_value = (GameStats.GetInt(EnumGameStats.GameState) == 0).ToString();
			return true;
		}
		if (_bindingName == "notinlinux")
		{
			_value = "true";
			return true;
		}
		if (_bindingName == "commsallowed")
		{
			_value = PermissionsManager.IsCommunicationAllowed().ToString();
			return true;
		}
		if (!(_bindingName == "voiceavailable"))
		{
			return base.GetBindingValue(ref _value, _bindingName);
		}
		_value = this.voiceAvailable.ToString();
		return true;
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Internal)]
	public static void <updateVoiceDevices>g__SelectActiveDevice|50_0(IList<IPartyVoice.VoiceAudioDevice> _devices, XUiC_ComboBoxList<IPartyVoice.VoiceAudioDevice> _combo, EnumGamePrefs _activeDevicePref)
	{
		if (_combo == null)
		{
			return;
		}
		string activeDevice = GamePrefs.GetString(_activeDevicePref);
		_combo.Elements.Clear();
		_combo.Elements.AddRange(_devices);
		if (_combo.Elements.Count == 0)
		{
			_combo.Elements.Add(XUiC_OptionsAudio.noDeviceEntry);
			_combo.SelectedIndex = 0;
			return;
		}
		int selectedIndex;
		if (string.IsNullOrEmpty(activeDevice) || (selectedIndex = _combo.Elements.FindIndex((IPartyVoice.VoiceAudioDevice _device) => _device.Identifier == activeDevice)) < 0)
		{
			int num = _combo.Elements.FindIndex((IPartyVoice.VoiceAudioDevice _device) => _device.IsDefault);
			if (num < 0)
			{
				_combo.Elements.Insert(0, XUiC_OptionsAudio.defaultDeviceEntry);
				num = 0;
			}
			_combo.SelectedIndex = num;
			return;
		}
		_combo.SelectedIndex = selectedIndex;
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat comboOverallAudioVolumeLevel;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboDynamicMusicEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat comboDynamicMusicDailyTimeAllotted;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat comboAmbientVolumeLevel;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat comboInGameMusicVolumeLevel;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat comboMenuMusicVolumeLevel;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboMumblePositionalAudioSupportEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboVoiceChatEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<IPartyVoice.VoiceAudioDevice> comboVoiceOutputDevice;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<IPartyVoice.VoiceAudioDevice> comboVoiceInputDevice;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat comboVoiceVolumeLevel;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboSubtitlesEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboProfanityFilter;

	[PublicizedFrom(EAccessModifier.Private)]
	public float origOverallAudioVolumeLevel;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool origDynamicMusicEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public float origDynamicMusicDailyTimeAllotted;

	[PublicizedFrom(EAccessModifier.Private)]
	public float origAmbientVolumeLevel;

	[PublicizedFrom(EAccessModifier.Private)]
	public float origInGameMusicVolumeLevel;

	[PublicizedFrom(EAccessModifier.Private)]
	public float origMenuMusicVolumeLevel;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool origMumblePositionalAudioSupportEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool origVoiceChatEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public float origVoiceVolumeLevel;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool origSubtitlesEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool origProfanityFilter;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnApply;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly IPartyVoice.VoiceAudioDevice noDeviceEntry = new IPartyVoice.VoiceAudioDeviceNotFound();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly IPartyVoice.VoiceAudioDevice defaultDeviceEntry = new IPartyVoice.VoiceAudioDeviceDefault();
}

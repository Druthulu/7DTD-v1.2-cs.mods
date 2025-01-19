using System;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_OptionsVideoSimplified : XUiController
{
	public static event Action OnSettingsChanged;

	public override void Init()
	{
		base.Init();
		XUiC_OptionsVideoSimplified.ID = base.WindowGroup.ID;
		this.tabs = base.GetChildByType<XUiC_TabSelector>();
		this.tabs.OnTabChanged += this.TabSelector_OnTabChanged;
		this.comboBrightness = base.GetChildById("Brightness").GetChildByType<XUiC_ComboBoxFloat>();
		this.comboBrightness.OnValueChangedGeneric += this.anyOtherValueChanged;
		this.btnDefaultBrightness = base.GetChildById("btnDefaultBrightness").GetChildByType<XUiC_SimpleButton>();
		this.btnDefaultBrightness.OnPressed += this.BtnDefaultBrightness_OnPressed;
		this.comboFieldOfView = base.GetChildById("FieldOfViewSimplified").GetChildByType<XUiC_ComboBoxInt>();
		this.comboFieldOfView.OnValueChangedGeneric += this.anyOtherValueChanged;
		this.comboFieldOfView.Min = (long)Constants.cMinCameraFieldOfView;
		this.comboFieldOfView.Max = (long)Constants.cMaxCameraFieldOfView;
		this.btnDefaultFOV = base.GetChildById("btnDefaultFOV").GetChildByType<XUiC_SimpleButton>();
		this.btnDefaultFOV.OnPressed += this.BtnDefaultFOV_OnPressed;
		this.comboMotionBlur = base.GetChildById("MotionBlurToggle").GetChildByType<XUiC_ComboBoxBool>();
		this.comboMotionBlur.OnValueChangedGeneric += this.AnyPresetValueChanged;
		this.comboGraphicsMode = base.GetChildById("GraphicsMode").GetChildByType<XUiC_ComboBoxList<string>>();
		this.comboGraphicsMode.OnValueChangedGeneric += this.AnyPresetValueChanged;
		this.comboUIBackgroundOpacity = base.GetChildById("UIBackgroundOpacity").GetChildByType<XUiC_ComboBoxFloat>();
		this.comboUIForegroundOpacity = base.GetChildById("UIForegroundOpacity").GetChildByType<XUiC_ComboBoxFloat>();
		this.comboUiSize = base.GetChildById("UiSize").GetChildByType<XUiC_ComboBoxFloat>();
		this.comboScreenBounds = base.GetChildById("ScreenBounds").GetChildByType<XUiC_ComboBoxFloat>();
		this.comboUIBackgroundOpacity.OnValueChangedGeneric += this.anyOtherValueChanged;
		this.comboUIForegroundOpacity.OnValueChangedGeneric += this.anyOtherValueChanged;
		this.comboUiSize.OnValueChangedGeneric += this.anyOtherValueChanged;
		this.comboScreenBounds.OnValueChangedGeneric += this.anyOtherValueChanged;
		this.comboUIBackgroundOpacity.Min = (double)Constants.cMinGlobalBackgroundOpacity;
		this.comboUIBackgroundOpacity.Max = 1.0;
		this.comboUIForegroundOpacity.Min = (double)Constants.cMinGlobalForegroundOpacity;
		this.comboUIForegroundOpacity.Max = 1.0;
		this.comboUiSize.Min = 0.7;
		this.comboUiSize.Max = 1.0;
		this.comboScreenBounds.Min = 0.8;
		this.comboScreenBounds.Max = 1.0;
		this.comboBrightness.Min = 0.0;
		this.comboBrightness.Max = 1.0;
		this.btnBack = (base.GetChildById("btnBack") as XUiC_SimpleButton);
		this.btnDefaults = (base.GetChildById("btnDefaults") as XUiC_SimpleButton);
		this.btnApply = (base.GetChildById("btnApply") as XUiC_SimpleButton);
		this.btnBack.OnPressed += this.BtnBack_OnPressed;
		this.btnDefaults.OnPressed += this.BtnDefaults_OnOnPressed;
		this.btnApply.OnPressed += this.BtnApply_OnPressed;
		this.btnApply.Text = "[action:gui:GUI Apply] " + Localization.Get("xuiApply", false).ToUpper();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void TabSelector_OnTabChanged(int _i, string _s)
	{
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void anyOtherValueChanged(XUiController _sender)
	{
		this.btnApply.Enabled = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDefaultBrightness_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.comboBrightness.Value = (double)((float)GamePrefs.GetDefault(EnumGamePrefs.OptionsGfxBrightness));
		this.btnApply.Enabled = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDefaultFOV_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.comboFieldOfView.Value = (long)((int)GamePrefs.GetDefault(EnumGamePrefs.OptionsGfxFOV));
		this.btnApply.Enabled = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void BtnApply_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.applyChanges();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void BtnDefaults_OnOnPressed(XUiController _sender, int _mouseButton)
	{
		this.comboUIBackgroundOpacity.Value = (double)((float)GamePrefs.GetDefault(EnumGamePrefs.OptionsBackgroundGlobalOpacity));
		this.comboUIForegroundOpacity.Value = (double)((float)GamePrefs.GetDefault(EnumGamePrefs.OptionsForegroundGlobalOpacity));
		this.comboUiSize.Value = (double)((float)GamePrefs.GetDefault(EnumGamePrefs.OptionsHudSize));
		this.comboScreenBounds.Value = (double)((float)GamePrefs.GetDefault(EnumGamePrefs.OptionsScreenBoundsValue));
		this.btnApply.Enabled = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void BtnBack_OnPressed(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(this.windowGroup.ID);
		base.xui.playerUI.windowManager.Open(XUiC_OptionsMenu.ID, true, false, true);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void AnyPresetValueChanged(XUiController _sender)
	{
		this.btnApply.Enabled = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateGraphicOptions()
	{
		this.comboBrightness.Value = (double)GamePrefs.GetFloat(EnumGamePrefs.OptionsGfxBrightness);
		this.comboFieldOfView.Value = (long)GamePrefs.GetInt(EnumGamePrefs.OptionsGfxFOV);
		this.comboMotionBlur.Value = GamePrefs.GetBool(EnumGamePrefs.OptionsGfxMotionBlurEnabled);
		int selectedIndex = (int)XUiC_OptionsVideoSimplified.QualityPresetToGraphicsMode(GamePrefs.GetInt(EnumGamePrefs.OptionsGfxQualityPreset));
		this.UpdateCustomModeVisibility(selectedIndex);
		this.comboGraphicsMode.SelectedIndex = selectedIndex;
		this.comboUIBackgroundOpacity.Value = (double)GamePrefs.GetFloat(EnumGamePrefs.OptionsBackgroundGlobalOpacity);
		this.comboUIForegroundOpacity.Value = (double)GamePrefs.GetFloat(EnumGamePrefs.OptionsForegroundGlobalOpacity);
		this.comboUiSize.Value = (double)GamePrefs.GetFloat(EnumGamePrefs.OptionsHudSize);
		this.comboScreenBounds.Value = (double)GamePrefs.GetFloat(EnumGamePrefs.OptionsScreenBoundsValue);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateCustomModeVisibility(int selectedIndex)
	{
		this.comboGraphicsMode.MaxIndex = Math.Max(selectedIndex, 3);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void applyChanges()
	{
		GamePrefs.Set(EnumGamePrefs.OptionsGfxBrightness, (float)this.comboBrightness.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsGfxFOV, (int)this.comboFieldOfView.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsGfxMotionBlurEnabled, this.comboMotionBlur.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsGfxQualityPreset, XUiC_OptionsVideoSimplified.GraphicsModeToQualityPreset((XUiC_OptionsVideoSimplified.GraphicsMode)this.comboGraphicsMode.SelectedIndex));
		this.UpdateCustomModeVisibility(this.comboGraphicsMode.SelectedIndex);
		GameOptionsManager.SetGraphicsQuality();
		GamePrefs.Set(EnumGamePrefs.OptionsBackgroundGlobalOpacity, (float)this.comboUIBackgroundOpacity.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsForegroundGlobalOpacity, (float)this.comboUIForegroundOpacity.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsHudSize, (float)this.comboUiSize.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsScreenBoundsValue, (float)this.comboScreenBounds.Value);
		GamePrefs.Instance.Save();
		GameOptionsManager.ApplyAllOptions(base.xui.playerUI);
		foreach (XUi xui in UnityEngine.Object.FindObjectsOfType<XUi>())
		{
			xui.BackgroundGlobalOpacity = GamePrefs.GetFloat(EnumGamePrefs.OptionsBackgroundGlobalOpacity);
			xui.ForegroundGlobalOpacity = GamePrefs.GetFloat(EnumGamePrefs.OptionsForegroundGlobalOpacity);
		}
		Action onSettingsChanged = XUiC_OptionsVideoSimplified.OnSettingsChanged;
		if (onSettingsChanged != null)
		{
			onSettingsChanged();
		}
		this.previousSettings = GamePrefs.GetSettingsCopy();
		this.btnApply.Enabled = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static XUiC_OptionsVideoSimplified.GraphicsMode QualityPresetToGraphicsMode(int qualityPreset)
	{
		XUiC_OptionsVideoSimplified.GraphicsMode result;
		switch (qualityPreset)
		{
		case 6:
			result = XUiC_OptionsVideoSimplified.GraphicsMode.ConsolePerformance;
			break;
		case 7:
			result = XUiC_OptionsVideoSimplified.GraphicsMode.ConsolePerformanceFSR;
			break;
		case 8:
			result = XUiC_OptionsVideoSimplified.GraphicsMode.ConsoleQuality;
			break;
		case 9:
			result = XUiC_OptionsVideoSimplified.GraphicsMode.ConsoleQualityFSR;
			break;
		default:
			result = XUiC_OptionsVideoSimplified.GraphicsMode.ConsoleCustom;
			break;
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int GraphicsModeToQualityPreset(XUiC_OptionsVideoSimplified.GraphicsMode graphicsMode)
	{
		int result;
		switch (graphicsMode)
		{
		case XUiC_OptionsVideoSimplified.GraphicsMode.ConsolePerformance:
			result = 6;
			break;
		case XUiC_OptionsVideoSimplified.GraphicsMode.ConsolePerformanceFSR:
			result = 7;
			break;
		case XUiC_OptionsVideoSimplified.GraphicsMode.ConsoleQuality:
			result = 8;
			break;
		case XUiC_OptionsVideoSimplified.GraphicsMode.ConsoleQualityFSR:
			result = 9;
			break;
		default:
			result = 5;
			break;
		}
		return result;
	}

	public override void OnOpen()
	{
		base.WindowGroup.openWindowOnEsc = XUiC_OptionsMenu.ID;
		this.previousSettings = GamePrefs.GetSettingsCopy();
		this.VSyncCountPref = PlatformApplicationManager.Application.VSyncCountPref;
		this.updateGraphicOptions();
		base.OnOpen();
		this.btnApply.Enabled = false;
	}

	public override void OnClose()
	{
		GamePrefs.ApplySettingsCopy(this.previousSettings);
		base.OnClose();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.IsDirty)
		{
			base.RefreshBindings(false);
			this.IsDirty = false;
		}
		if (this.btnApply.Enabled && base.xui.playerUI.playerInput.GUIActions.Apply.WasPressed)
		{
			this.BtnApply_OnPressed(null, 0);
		}
	}

	public override bool GetBindingValue(ref string _value, string _bindingName)
	{
		if (_bindingName == "isTabUi")
		{
			_value = ((this.tabs != null && this.tabs.IsSelected("xuiOptionsVideoUI")) ? "true" : "false");
			return true;
		}
		if (_bindingName == "ui_size_limited")
		{
			float num = (float)GameOptionsManager.GetUiSizeLimit();
			float @float = GamePrefs.GetFloat(EnumGamePrefs.OptionsHudSize);
			_value = (@float > num).ToString();
			return true;
		}
		if (!(_bindingName == "ui_size_limit"))
		{
			return base.GetBindingValue(ref _value, _bindingName);
		}
		_value = GameOptionsManager.GetUiSizeLimit().ToCultureInvariantString();
		return true;
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TabSelector tabs;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat comboBrightness;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnDefaultBrightness;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxInt comboFieldOfView;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnDefaultFOV;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboMotionBlur;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboGraphicsMode;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat comboUIBackgroundOpacity;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat comboUIForegroundOpacity;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat comboUiSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat comboScreenBounds;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnBack;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnDefaults;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnApply;

	[PublicizedFrom(EAccessModifier.Private)]
	public object[] previousSettings;

	[PublicizedFrom(EAccessModifier.Private)]
	public float previousRefreshRate = -1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public EnumGamePrefs VSyncCountPref = EnumGamePrefs.OptionsGfxVsync;

	public enum GraphicsMode
	{
		ConsolePerformance,
		ConsolePerformanceFSR,
		ConsoleQuality,
		ConsoleQualityFSR,
		ConsoleCustom
	}
}

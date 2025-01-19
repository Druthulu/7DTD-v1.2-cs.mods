using System;
using System.Runtime.CompilerServices;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_OptionsVideo : XUiController
{
	public static event Action OnSettingsChanged;

	public override void Init()
	{
		base.Init();
		XUiC_OptionsVideo.ID = base.WindowGroup.ID;
		this.tabs = base.GetChildByType<XUiC_TabSelector>();
		this.tabs.OnTabChanged += this.TabSelector_OnTabChanged;
		this.comboResolution = base.GetChildById("Resolution").GetChildByType<XUiC_ComboBoxList<XUiC_OptionsVideo.ResolutionInfo>>();
		this.comboFullscreen = base.GetChildById("Fullscreen").GetChildByType<XUiC_ComboBoxList<string>>();
		this.comboDynamicMode = base.GetChildById("DyMode").GetChildByType<XUiC_ComboBoxList<string>>();
		this.comboDynamicMinFPS = base.GetChildById("DyMinFPS").GetChildByType<XUiC_ComboBoxInt>();
		this.comboDynamicScale = base.GetChildById("DyScale").GetChildByType<XUiC_ComboBoxFloat>();
		this.comboVSync = base.GetChildById("VSync").GetChildByType<XUiC_ComboBoxList<string>>();
		this.comboBrightness = base.GetChildById("Brightness").GetChildByType<XUiC_ComboBoxFloat>();
		this.btnDefaultBrightness = base.GetChildById("btnDefaultBrightness").GetChildByType<XUiC_SimpleButton>();
		this.comboFieldOfView = base.GetChildById("FieldOfView").GetChildByType<XUiC_ComboBoxInt>();
		this.btnDefaultFOV = base.GetChildById("btnDefaultFOV").GetChildByType<XUiC_SimpleButton>();
		this.comboQualityPreset = base.GetChildById("QualityPreset").GetChildByType<XUiC_ComboBoxList<string>>();
		this.comboAntiAliasing = base.GetChildById("AntiAliasing").GetChildByType<XUiC_ComboBoxList<string>>();
		this.comboAntiAliasingSharp = base.GetChildById("AntiAliasingSharp").GetChildByType<XUiC_ComboBoxFloat>();
		this.comboTextureQuality = base.GetChildById("TextureQuality").GetChildByType<XUiC_ComboBoxList<string>>();
		this.comboTextureFilter = base.GetChildById("TextureFilter").GetChildByType<XUiC_ComboBoxList<string>>();
		this.comboReflectionQuality = base.GetChildById("ReflectionQuality").GetChildByType<XUiC_ComboBoxList<string>>();
		this.comboReflectedShadows = base.GetChildById("ReflectedShadows").GetChildByType<XUiC_ComboBoxBool>();
		this.comboWaterQuality = base.GetChildById("WaterQuality").GetChildByType<XUiC_ComboBoxList<string>>();
		this.comboViewDistance = base.GetChildById("ViewDistance").GetChildByType<XUiC_ComboBoxList<string>>();
		this.comboLODDistance = base.GetChildById("LODDistance").GetChildByType<XUiC_ComboBoxFloat>();
		this.comboShadowsDistance = base.GetChildById("ShadowsDistance").GetChildByType<XUiC_ComboBoxList<string>>();
		this.comboShadowsQuality = base.GetChildById("ShadowsQuality").GetChildByType<XUiC_ComboBoxList<string>>();
		this.comboTerrainQuality = base.GetChildById("TerrainQuality").GetChildByType<XUiC_ComboBoxList<string>>();
		this.comboObjectQuality = base.GetChildById("ObjectQuality").GetChildByType<XUiC_ComboBoxList<string>>();
		this.comboGrassDistance = base.GetChildById("GrassDistance").GetChildByType<XUiC_ComboBoxList<string>>();
		this.comboOcclusion = base.GetChildById("Occlusion").GetChildByType<XUiC_ComboBoxBool>();
		this.comboDymeshEnabled = base.GetChildById("DynamicMeshEnabled").GetChildByType<XUiC_ComboBoxBool>();
		this.comboDymeshDistance = base.GetChildById("DynamicMeshDistance").GetChildByType<XUiC_ComboBoxList<int>>();
		this.comboDymeshHighQualityMesh = base.GetChildById("DynamicMeshHighQualityMesh").GetChildByType<XUiC_ComboBoxBool>();
		this.comboDymeshMaxRegions = base.GetChildById("DynamicMeshMaxRegionLoads").GetChildByType<XUiC_ComboBoxList<int>>();
		this.comboDymeshMaxMesh = base.GetChildById("DynamicMeshMaxMeshCache").GetChildByType<XUiC_ComboBoxList<int>>();
		this.comboDymeshLandClaimOnly = base.GetChildById("DynamicMeshLandClaimOnly").GetChildByType<XUiC_ComboBoxBool>();
		this.comboDymeshLandClaimBuffer = base.GetChildById("DynamicMeshLandClaimBuffer").GetChildByType<XUiC_ComboBoxList<int>>();
		this.comboBloom = base.GetChildById("Bloom").GetChildByType<XUiC_ComboBoxBool>();
		XUiController childById = base.GetChildById("DepthOfField");
		this.comboDepthOfField = ((childById != null) ? childById.GetChildByType<XUiC_ComboBoxBool>() : null);
		this.comboMotionBlur = base.GetChildById("MotionBlur").GetChildByType<XUiC_ComboBoxList<string>>();
		this.comboSSAO = base.GetChildById("SSAO").GetChildByType<XUiC_ComboBoxBool>();
		this.comboSSReflections = base.GetChildById("SSReflections").GetChildByType<XUiC_ComboBoxList<string>>();
		this.comboSunShafts = base.GetChildById("SunShafts").GetChildByType<XUiC_ComboBoxBool>();
		this.comboParticles = base.GetChildById("Particles").GetChildByType<XUiC_ComboBoxFloat>();
		this.comboUIBackgroundOpacity = base.GetChildById("UIBackgroundOpacity").GetChildByType<XUiC_ComboBoxFloat>();
		this.comboUIForegroundOpacity = base.GetChildById("UIForegroundOpacity").GetChildByType<XUiC_ComboBoxFloat>();
		this.comboUiSize = base.GetChildById("UiSize").GetChildByType<XUiC_ComboBoxFloat>();
		this.comboScreenBounds = base.GetChildById("ScreenBounds").GetChildByType<XUiC_ComboBoxFloat>();
		this.comboUiFpsScaling = base.GetChildById("UiFpsScaling").GetChildByType<XUiC_ComboBoxFloat>();
		this.comboQualityPreset.OnValueChanged += this.QualityPresetChanged;
		this.comboAntiAliasing.OnValueChangedGeneric += this.AnyPresetValueChanged;
		this.comboAntiAliasingSharp.OnValueChangedGeneric += this.AnyPresetValueChanged;
		this.comboTextureQuality.OnValueChangedGeneric += this.AnyPresetValueChanged;
		this.comboTextureFilter.OnValueChangedGeneric += this.AnyPresetValueChanged;
		this.comboReflectionQuality.OnValueChangedGeneric += this.AnyPresetValueChanged;
		this.comboReflectedShadows.OnValueChangedGeneric += this.AnyPresetValueChanged;
		this.comboWaterQuality.OnValueChangedGeneric += this.AnyPresetValueChanged;
		this.comboViewDistance.OnValueChangedGeneric += this.AnyPresetValueChanged;
		this.comboLODDistance.OnValueChangedGeneric += this.AnyPresetValueChanged;
		this.comboShadowsDistance.OnValueChangedGeneric += this.AnyPresetValueChanged;
		this.comboShadowsQuality.OnValueChangedGeneric += this.AnyPresetValueChanged;
		this.comboTerrainQuality.OnValueChangedGeneric += this.AnyPresetValueChanged;
		this.comboObjectQuality.OnValueChangedGeneric += this.AnyPresetValueChanged;
		this.comboGrassDistance.OnValueChangedGeneric += this.AnyPresetValueChanged;
		this.comboOcclusion.OnValueChangedGeneric += this.AnyPresetValueChanged;
		this.comboDymeshEnabled.OnValueChangedGeneric += this.AnyPresetValueChanged;
		this.comboDymeshDistance.OnValueChangedGeneric += this.AnyPresetValueChanged;
		this.comboDymeshHighQualityMesh.OnValueChangedGeneric += this.AnyPresetValueChanged;
		this.comboDymeshMaxRegions.OnValueChangedGeneric += this.AnyPresetValueChanged;
		this.comboDymeshMaxMesh.OnValueChangedGeneric += this.AnyPresetValueChanged;
		this.comboDymeshLandClaimOnly.OnValueChangedGeneric += this.AnyPresetValueChanged;
		this.comboDymeshLandClaimBuffer.OnValueChangedGeneric += this.AnyPresetValueChanged;
		this.comboBloom.OnValueChangedGeneric += this.AnyPresetValueChanged;
		if (this.comboDepthOfField != null)
		{
			this.comboDepthOfField.OnValueChangedGeneric += this.AnyPresetValueChanged;
		}
		this.comboMotionBlur.OnValueChangedGeneric += this.AnyPresetValueChanged;
		this.comboSSAO.OnValueChangedGeneric += this.AnyPresetValueChanged;
		this.comboSSReflections.OnValueChangedGeneric += this.AnyPresetValueChanged;
		this.comboSunShafts.OnValueChangedGeneric += this.AnyPresetValueChanged;
		this.comboParticles.OnValueChangedGeneric += this.AnyPresetValueChanged;
		this.comboResolution.OnValueChangedGeneric += this.anyOtherValueChanged;
		this.comboFullscreen.OnValueChangedGeneric += this.anyOtherValueChanged;
		this.comboDynamicMode.OnValueChangedGeneric += this.anyOtherValueChanged;
		this.comboDynamicMinFPS.OnValueChangedGeneric += this.anyOtherValueChanged;
		this.comboDynamicScale.OnValueChangedGeneric += this.anyOtherValueChanged;
		this.comboVSync.OnValueChangedGeneric += this.anyOtherValueChanged;
		this.comboBrightness.OnValueChangedGeneric += this.anyOtherValueChanged;
		this.comboFieldOfView.OnValueChangedGeneric += this.anyOtherValueChanged;
		this.comboUIBackgroundOpacity.OnValueChangedGeneric += this.anyOtherValueChanged;
		this.comboUIForegroundOpacity.OnValueChangedGeneric += this.anyOtherValueChanged;
		this.comboUiSize.OnValueChangedGeneric += this.anyOtherValueChanged;
		this.comboScreenBounds.OnValueChangedGeneric += this.anyOtherValueChanged;
		this.comboUiFpsScaling.OnValueChangedGeneric += this.anyOtherValueChanged;
		this.comboDynamicMinFPS.Min = 10L;
		this.comboDynamicMinFPS.Max = 60L;
		this.comboDynamicScale.Min = 0.20000000298023224;
		this.comboDynamicScale.Max = 1.0;
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
		this.btnDefaultBrightness.OnPressed += this.BtnDefaultBrightness_OnPressed;
		this.comboFieldOfView.Min = (long)Constants.cMinCameraFieldOfView;
		this.comboFieldOfView.Max = (long)Constants.cMaxCameraFieldOfView;
		this.btnDefaultFOV.OnPressed += this.BtnDefaultFOV_OnPressed;
		this.comboAntiAliasingSharp.Min = 0.0;
		this.comboAntiAliasingSharp.Max = 1.0;
		this.comboParticles.Min = 0.0;
		this.comboParticles.Max = 1.0;
		this.comboLODDistance.Min = 0.0;
		this.comboLODDistance.Max = 1.0;
		this.origLength_ReflectionQuality = this.comboReflectionQuality.Elements.Count;
		this.comboReflectionQuality.MaxIndex = this.origLength_ReflectionQuality - 1;
		this.comboReflectionQuality.Elements.Add("Custom");
		this.origLength_ShadowDistance = this.comboShadowsDistance.Elements.Count;
		this.comboShadowsDistance.MaxIndex = this.origLength_ShadowDistance - 1;
		this.comboShadowsDistance.Elements.Add("Custom");
		this.origLength_ShadowQuality = this.comboShadowsQuality.Elements.Count;
		this.comboShadowsQuality.MaxIndex = this.origLength_ShadowQuality - 1;
		this.comboShadowsQuality.Elements.Add("Custom");
		this.origLength_TerrainQuality = this.comboTerrainQuality.Elements.Count;
		this.comboTerrainQuality.MaxIndex = this.origLength_TerrainQuality - 1;
		this.comboTerrainQuality.Elements.Add("Custom");
		this.origLength_ObjectQuality = this.comboObjectQuality.Elements.Count;
		this.comboObjectQuality.MaxIndex = this.origLength_ObjectQuality - 1;
		this.comboObjectQuality.Elements.Add("Custom");
		this.origLength_GrassDistance = this.comboGrassDistance.Elements.Count;
		this.comboGrassDistance.MaxIndex = this.origLength_GrassDistance - 1;
		this.comboGrassDistance.Elements.Add("Custom");
		this.origLength_MotionBlur = this.comboMotionBlur.Elements.Count;
		this.comboMotionBlur.MaxIndex = this.origLength_MotionBlur - 1;
		this.comboMotionBlur.Elements.Add("Custom");
		this.origLength_SSR = this.comboSSReflections.Elements.Count;
		this.comboSSReflections.MaxIndex = this.origLength_SSR - 1;
		this.comboSSReflections.Elements.Add("Custom");
		if ((DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent())
		{
			this.comboQualityPreset.Elements.Add("ConsolePerformance");
			this.comboQualityPreset.Elements.Add("ConsolePerformanceFSR");
			this.comboQualityPreset.Elements.Add("ConsoleQuality");
			this.comboQualityPreset.Elements.Add("ConsoleQualityFSR");
		}
		this.btnBack = (base.GetChildById("btnBack") as XUiC_SimpleButton);
		this.btnDefaults = (base.GetChildById("btnDefaults") as XUiC_SimpleButton);
		this.btnApply = (base.GetChildById("btnApply") as XUiC_SimpleButton);
		this.btnBack.OnPressed += this.BtnBack_OnPressed;
		this.btnDefaults.OnPressed += this.BtnDefaults_OnOnPressed;
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
	public void TabSelector_OnTabChanged(int _i, string _s)
	{
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void anyOtherValueChanged(XUiController _sender)
	{
		this.updateDynamicOptions();
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
		this.comboUiFpsScaling.Value = (double)((float)GamePrefs.GetDefault(EnumGamePrefs.OptionsUiFpsScaling));
		this.btnApply.Enabled = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void BtnBack_OnPressed(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(this.windowGroup.ID);
		base.xui.playerUI.windowManager.Open(XUiC_OptionsMenu.ID, true, false, true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void QualityPresetChanged(XUiController _sender, string _oldValue, string _newValue)
	{
		if (this.comboQualityPreset.SelectedIndex != 5)
		{
			GamePrefs.Set(EnumGamePrefs.OptionsGfxQualityPreset, this.comboQualityPreset.SelectedIndex);
			GameOptionsManager.SetGraphicsQuality();
			this.updateGraphicOptions();
			this.updateDynamicOptions();
		}
		this.btnApply.Enabled = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void AnyPresetValueChanged(XUiController _sender)
	{
		this.comboQualityPreset.SelectedIndex = 5;
		GamePrefs.Set(EnumGamePrefs.OptionsGfxQualityPreset, this.comboQualityPreset.SelectedIndex);
		this.btnApply.Enabled = true;
		this.updateGraphicsAAOptions();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateDynamicOptions()
	{
		this.comboDynamicMinFPS.Enabled = (this.comboDynamicMode.SelectedIndex == 1);
		this.comboDynamicScale.Enabled = (this.comboDynamicMode.SelectedIndex == 2);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateGraphicsAAOptions()
	{
		this.comboAntiAliasingSharp.Enabled = (this.comboAntiAliasing.SelectedIndex >= 4);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateGraphicOptions()
	{
		Resolution[] supportedResolutions = PlatformApplicationManager.Application.SupportedResolutions;
		this.comboResolution.Elements.Clear();
		foreach (Resolution resolution in supportedResolutions)
		{
			XUiC_OptionsVideo.ResolutionInfo item = new XUiC_OptionsVideo.ResolutionInfo(resolution.width, resolution.height);
			if (!this.comboResolution.Elements.Contains(item))
			{
				this.comboResolution.Elements.Add(item);
			}
		}
		this.comboResolution.Elements.Sort();
		ValueTuple<int, int, FullScreenMode> screenOptions = PlatformApplicationManager.Application.ScreenOptions;
		int item2 = screenOptions.Item1;
		int item3 = screenOptions.Item2;
		XUiC_OptionsVideo.ResolutionInfo item4 = new XUiC_OptionsVideo.ResolutionInfo(item2, item3);
		if (!this.comboResolution.Elements.Contains(item4))
		{
			this.comboResolution.Elements.Add(item4);
			this.comboResolution.Elements.Sort();
		}
		this.comboResolution.SelectedIndex = this.comboResolution.Elements.IndexOf(item4);
		FullScreenMode @int = (FullScreenMode)SdPlayerPrefs.GetInt("Screenmanager Fullscreen mode", 3);
		this.comboFullscreen.SelectedIndex = XUiC_OptionsVideo.ConvertFullScreenModeToIndex(@int);
		this.comboDynamicMode.SelectedIndex = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxDynamicMode);
		this.comboDynamicMinFPS.Value = (long)GamePrefs.GetInt(EnumGamePrefs.OptionsGfxDynamicMinFPS);
		this.comboDynamicScale.Value = (double)GamePrefs.GetFloat(EnumGamePrefs.OptionsGfxDynamicScale);
		this.comboVSync.SelectedIndex = GamePrefs.GetInt(this.VSyncCountPref);
		this.comboBrightness.Value = (double)GamePrefs.GetFloat(EnumGamePrefs.OptionsGfxBrightness);
		this.comboFieldOfView.Value = (long)GamePrefs.GetInt(EnumGamePrefs.OptionsGfxFOV);
		this.comboDymeshEnabled.Value = GamePrefs.GetBool(EnumGamePrefs.DynamicMeshEnabled);
		this.comboDymeshDistance.Value = GamePrefs.GetInt(EnumGamePrefs.DynamicMeshDistance);
		this.comboDymeshHighQualityMesh.Value = !GamePrefs.GetBool(EnumGamePrefs.DynamicMeshUseImposters);
		this.comboDymeshMaxRegions.Value = GamePrefs.GetInt(EnumGamePrefs.DynamicMeshMaxRegionCache);
		this.comboDymeshMaxMesh.Value = GamePrefs.GetInt(EnumGamePrefs.DynamicMeshMaxItemCache);
		this.comboDymeshLandClaimOnly.Value = GamePrefs.GetBool(EnumGamePrefs.DynamicMeshLandClaimOnly);
		this.comboDymeshLandClaimBuffer.Value = GamePrefs.GetInt(EnumGamePrefs.DynamicMeshLandClaimBuffer);
		this.comboQualityPreset.SelectedIndex = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxQualityPreset);
		this.comboAntiAliasing.SelectedIndex = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxAA);
		this.comboAntiAliasingSharp.Value = (double)GamePrefs.GetFloat(EnumGamePrefs.OptionsGfxAASharpness);
		this.updateGraphicsAAOptions();
		this.comboTextureQuality.SelectedIndex = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxTexQuality);
		this.comboTextureFilter.SelectedIndex = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxTexFilter);
		int int2 = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxReflectQuality);
		if (int2 < this.origLength_ReflectionQuality)
		{
			this.comboReflectionQuality.SelectedIndex = int2;
		}
		else
		{
			this.comboReflectionQuality.MaxIndex = this.origLength_ReflectionQuality;
			this.comboReflectionQuality.SelectedIndex = this.comboReflectionQuality.Elements.Count - 1;
		}
		this.comboReflectedShadows.Value = GamePrefs.GetBool(EnumGamePrefs.OptionsGfxReflectShadows);
		this.comboWaterQuality.SelectedIndex = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxWaterQuality);
		this.comboViewDistance.SelectedIndex = ((GamePrefs.GetInt(EnumGamePrefs.OptionsGfxViewDistance) == 5) ? 0 : ((GamePrefs.GetInt(EnumGamePrefs.OptionsGfxViewDistance) == 6) ? 1 : 2));
		this.comboLODDistance.Value = (double)GamePrefs.GetFloat(EnumGamePrefs.OptionsGfxLODDistance);
		int num = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxShadowDistance);
		if (num >= this.origLength_ShadowDistance && num < 20)
		{
			num = 20;
			GamePrefs.Set(EnumGamePrefs.OptionsGfxShadowDistance, 20);
		}
		if (num < this.origLength_ShadowDistance)
		{
			this.comboShadowsDistance.SelectedIndex = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxShadowDistance);
		}
		else
		{
			this.comboShadowsDistance.MaxIndex = this.origLength_ShadowDistance;
			this.comboShadowsDistance.SelectedIndex = this.comboShadowsDistance.Elements.Count - 1;
		}
		int int3 = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxShadowQuality);
		if (int3 < this.origLength_ShadowQuality)
		{
			this.comboShadowsQuality.SelectedIndex = int3;
		}
		else
		{
			this.comboShadowsQuality.MaxIndex = this.origLength_ShadowQuality;
			this.comboShadowsQuality.SelectedIndex = this.comboShadowsQuality.Elements.Count - 1;
		}
		int int4 = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxTerrainQuality);
		if (int4 < this.origLength_TerrainQuality)
		{
			this.comboTerrainQuality.SelectedIndex = int4;
		}
		else
		{
			this.comboTerrainQuality.MaxIndex = this.origLength_TerrainQuality;
			this.comboTerrainQuality.SelectedIndex = this.comboTerrainQuality.Elements.Count - 1;
		}
		int int5 = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxObjQuality);
		if (int5 < this.origLength_ObjectQuality)
		{
			this.comboObjectQuality.SelectedIndex = int5;
		}
		else
		{
			this.comboObjectQuality.MaxIndex = this.origLength_ObjectQuality;
			this.comboObjectQuality.SelectedIndex = this.comboObjectQuality.Elements.Count - 1;
		}
		int int6 = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxGrassDistance);
		if (int6 < this.origLength_GrassDistance)
		{
			this.comboGrassDistance.SelectedIndex = int6;
		}
		else
		{
			this.comboGrassDistance.MaxIndex = this.origLength_GrassDistance;
			this.comboGrassDistance.SelectedIndex = this.comboGrassDistance.Elements.Count - 1;
		}
		this.comboOcclusion.Value = GamePrefs.GetBool(EnumGamePrefs.OptionsGfxOcclusion);
		this.comboBloom.Value = GamePrefs.GetBool(EnumGamePrefs.OptionsGfxBloom);
		if (this.comboDepthOfField != null)
		{
			this.comboDepthOfField.Value = GamePrefs.GetBool(EnumGamePrefs.OptionsGfxDOF);
		}
		int int7 = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxMotionBlur);
		if (int7 < this.origLength_MotionBlur)
		{
			this.comboMotionBlur.SelectedIndex = int7;
		}
		else
		{
			this.comboMotionBlur.MaxIndex = this.origLength_MotionBlur;
			this.comboMotionBlur.SelectedIndex = this.comboMotionBlur.Elements.Count - 1;
		}
		this.comboSSAO.Value = GamePrefs.GetBool(EnumGamePrefs.OptionsGfxSSAO);
		int int8 = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxSSReflections);
		if (int8 < this.origLength_SSR)
		{
			this.comboSSReflections.SelectedIndex = int8;
		}
		else
		{
			this.comboSSReflections.MaxIndex = this.origLength_SSR;
			this.comboSSReflections.SelectedIndex = this.comboSSReflections.Elements.Count - 1;
		}
		this.comboSunShafts.Value = GamePrefs.GetBool(EnumGamePrefs.OptionsGfxSunShafts);
		this.comboParticles.Value = (double)GamePrefs.GetFloat(EnumGamePrefs.OptionsGfxWaterPtlLimiter);
		this.comboUIBackgroundOpacity.Value = (double)GamePrefs.GetFloat(EnumGamePrefs.OptionsBackgroundGlobalOpacity);
		this.comboUIForegroundOpacity.Value = (double)GamePrefs.GetFloat(EnumGamePrefs.OptionsForegroundGlobalOpacity);
		this.comboUiSize.Value = (double)GamePrefs.GetFloat(EnumGamePrefs.OptionsHudSize);
		this.comboScreenBounds.Value = (double)GamePrefs.GetFloat(EnumGamePrefs.OptionsScreenBoundsValue);
		this.comboUiFpsScaling.Value = (double)GamePrefs.GetFloat(EnumGamePrefs.OptionsUiFpsScaling);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void applyChanges()
	{
		XUiC_OptionsVideo.ResolutionInfo value = this.comboResolution.Value;
		GameOptionsManager.SetResolution(value.Width, value.Height, XUiC_OptionsVideo.ConvertIndexToFullScreenMode(this.comboFullscreen.SelectedIndex));
		if (this.comboAntiAliasing.SelectedIndex >= 5)
		{
			this.comboDynamicMode.SelectedIndex = 0;
		}
		GamePrefs.Set(EnumGamePrefs.OptionsGfxDynamicMode, this.comboDynamicMode.SelectedIndex);
		GamePrefs.Set(EnumGamePrefs.OptionsGfxDynamicMinFPS, (int)this.comboDynamicMinFPS.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsGfxDynamicScale, (float)this.comboDynamicScale.Value);
		GamePrefs.Set(this.VSyncCountPref, this.comboVSync.SelectedIndex);
		GamePrefs.Set(EnumGamePrefs.OptionsGfxBrightness, (float)this.comboBrightness.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsGfxFOV, (int)this.comboFieldOfView.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsGfxQualityPreset, this.comboQualityPreset.SelectedIndex);
		GamePrefs.Set(EnumGamePrefs.OptionsGfxAA, this.comboAntiAliasing.SelectedIndex);
		GamePrefs.Set(EnumGamePrefs.OptionsGfxAASharpness, (float)this.comboAntiAliasingSharp.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsGfxTexQuality, this.comboTextureQuality.SelectedIndex);
		GamePrefs.Set(EnumGamePrefs.OptionsGfxTexFilter, this.comboTextureFilter.SelectedIndex);
		if (this.comboReflectionQuality.SelectedIndex < this.origLength_ReflectionQuality)
		{
			GamePrefs.Set(EnumGamePrefs.OptionsGfxReflectQuality, this.comboReflectionQuality.SelectedIndex);
		}
		GamePrefs.Set(EnumGamePrefs.OptionsGfxReflectShadows, this.comboReflectedShadows.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsGfxWaterQuality, this.comboWaterQuality.SelectedIndex);
		GamePrefs.Set(EnumGamePrefs.OptionsGfxViewDistance, (this.comboViewDistance.SelectedIndex == 0) ? 5 : ((this.comboViewDistance.SelectedIndex == 1) ? 6 : 7));
		GamePrefs.Set(EnumGamePrefs.OptionsGfxLODDistance, (float)this.comboLODDistance.Value);
		if (this.comboShadowsDistance.SelectedIndex < this.origLength_ShadowDistance)
		{
			GamePrefs.Set(EnumGamePrefs.OptionsGfxShadowDistance, this.comboShadowsDistance.SelectedIndex);
		}
		if (this.comboShadowsQuality.SelectedIndex < this.origLength_ShadowQuality)
		{
			GamePrefs.Set(EnumGamePrefs.OptionsGfxShadowQuality, this.comboShadowsQuality.SelectedIndex);
		}
		if (this.comboTerrainQuality.SelectedIndex < this.origLength_TerrainQuality)
		{
			GamePrefs.Set(EnumGamePrefs.OptionsGfxTerrainQuality, this.comboTerrainQuality.SelectedIndex);
		}
		if (this.comboObjectQuality.SelectedIndex < this.origLength_ObjectQuality)
		{
			GamePrefs.Set(EnumGamePrefs.OptionsGfxObjQuality, this.comboObjectQuality.SelectedIndex);
		}
		if (this.comboGrassDistance.SelectedIndex < this.origLength_GrassDistance)
		{
			GamePrefs.Set(EnumGamePrefs.OptionsGfxGrassDistance, this.comboGrassDistance.SelectedIndex);
		}
		this.origDymeshEnabled = GamePrefs.GetBool(EnumGamePrefs.DynamicMeshEnabled);
		GamePrefs.Set(EnumGamePrefs.DynamicMeshEnabled, this.comboDymeshEnabled.Value);
		GamePrefs.Set(EnumGamePrefs.DynamicMeshDistance, this.comboDymeshDistance.Value);
		DynamicMeshSettings.MaxViewDistance = this.comboDymeshDistance.Value;
		GamePrefs.Set(EnumGamePrefs.DynamicMeshUseImposters, !this.comboDymeshHighQualityMesh.Value);
		GamePrefs.Set(EnumGamePrefs.DynamicMeshMaxRegionCache, this.comboDymeshMaxRegions.Value);
		GamePrefs.Set(EnumGamePrefs.DynamicMeshMaxItemCache, this.comboDymeshMaxMesh.Value);
		GamePrefs.Set(EnumGamePrefs.DynamicMeshLandClaimOnly, this.comboDymeshLandClaimOnly.Value);
		GamePrefs.Set(EnumGamePrefs.DynamicMeshLandClaimBuffer, this.comboDymeshLandClaimBuffer.Value);
		if (this.origDymeshEnabled != this.comboDymeshEnabled.Value)
		{
			DynamicMeshManager.EnabledChanged(this.comboDymeshEnabled.Value);
		}
		GamePrefs.Set(EnumGamePrefs.OptionsGfxOcclusion, this.comboOcclusion.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsGfxBloom, this.comboBloom.Value);
		if (this.comboDepthOfField != null)
		{
			GamePrefs.Set(EnumGamePrefs.OptionsGfxDOF, this.comboDepthOfField.Value);
		}
		if (this.comboMotionBlur.SelectedIndex < this.origLength_MotionBlur)
		{
			GamePrefs.Set(EnumGamePrefs.OptionsGfxMotionBlur, this.comboMotionBlur.SelectedIndex);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxMotionBlurEnabled, this.comboMotionBlur.SelectedIndex > 0);
		}
		GamePrefs.Set(EnumGamePrefs.OptionsGfxSSAO, this.comboSSAO.Value);
		if (this.comboSSReflections.SelectedIndex < this.origLength_SSR)
		{
			GamePrefs.Set(EnumGamePrefs.OptionsGfxSSReflections, this.comboSSReflections.SelectedIndex);
		}
		GamePrefs.Set(EnumGamePrefs.OptionsGfxSunShafts, this.comboSunShafts.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsGfxWaterPtlLimiter, (float)this.comboParticles.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsBackgroundGlobalOpacity, (float)this.comboUIBackgroundOpacity.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsForegroundGlobalOpacity, (float)this.comboUIForegroundOpacity.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsHudSize, (float)this.comboUiSize.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsScreenBoundsValue, (float)this.comboScreenBounds.Value);
		GamePrefs.Set(EnumGamePrefs.OptionsUiFpsScaling, (float)this.comboUiFpsScaling.Value);
		GamePrefs.Instance.Save();
		GameOptionsManager.ApplyAllOptions(base.xui.playerUI);
		QualitySettings.vSyncCount = GamePrefs.GetInt(this.VSyncCountPref);
		ReflectionManager.ApplyOptions(false);
		WaterSplashCubes.particleLimiter = GamePrefs.GetFloat(EnumGamePrefs.OptionsGfxWaterPtlLimiter);
		foreach (XUi xui in UnityEngine.Object.FindObjectsOfType<XUi>())
		{
			xui.BackgroundGlobalOpacity = GamePrefs.GetFloat(EnumGamePrefs.OptionsBackgroundGlobalOpacity);
			xui.ForegroundGlobalOpacity = GamePrefs.GetFloat(EnumGamePrefs.OptionsForegroundGlobalOpacity);
		}
		Action onSettingsChanged = XUiC_OptionsVideo.OnSettingsChanged;
		if (onSettingsChanged != null)
		{
			onSettingsChanged();
		}
		this.previousSettings = GamePrefs.GetSettingsCopy();
		this.btnApply.Enabled = false;
	}

	public static int ConvertFullScreenModeToIndex(FullScreenMode _mode)
	{
		if (_mode == FullScreenMode.ExclusiveFullScreen)
		{
			return 2;
		}
		if (_mode == FullScreenMode.FullScreenWindow)
		{
			return 1;
		}
		return 0;
	}

	public static FullScreenMode ConvertIndexToFullScreenMode(int _index)
	{
		if (_index == 1)
		{
			return FullScreenMode.FullScreenWindow;
		}
		if (_index != 2)
		{
			return FullScreenMode.Windowed;
		}
		return FullScreenMode.ExclusiveFullScreen;
	}

	public override void OnOpen()
	{
		base.WindowGroup.openWindowOnEsc = XUiC_OptionsMenu.ID;
		this.previousSettings = GamePrefs.GetSettingsCopy();
		int minIndex = GameOptionsManager.CalcTextureQualityMin();
		this.comboTextureQuality.MinIndex = minIndex;
		this.VSyncCountPref = PlatformApplicationManager.Application.VSyncCountPref;
		this.updateGraphicOptions();
		this.updateDynamicOptions();
		bool flag = GameManager.Instance.World != null;
		this.comboDymeshLandClaimOnly.Enabled = !flag;
		this.comboViewDistance.Enabled = !flag;
		this.comboOcclusion.Enabled = !flag;
		this.comboDymeshEnabled.Enabled = !flag;
		base.OnOpen();
		this.btnApply.Enabled = false;
		this.RefreshApplyLabel();
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
		if (_bindingName == "ui_size_limit")
		{
			_value = GameOptionsManager.GetUiSizeLimit().ToCultureInvariantString();
			return true;
		}
		if (!(_bindingName == "texture_quality_limited"))
		{
			return base.GetBindingValue(ref _value, _bindingName);
		}
		XUiC_ComboBoxList<string> xuiC_ComboBoxList = this.comboTextureQuality;
		_value = (xuiC_ComboBoxList != null && xuiC_ComboBoxList.MinIndex > 0).ToString();
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public EnumGamePrefs VSyncCountPref = EnumGamePrefs.OptionsGfxVsync;

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TabSelector tabs;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<XUiC_OptionsVideo.ResolutionInfo> comboResolution;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboFullscreen;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboDynamicMode;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxInt comboDynamicMinFPS;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat comboDynamicScale;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboVSync;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat comboBrightness;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnDefaultBrightness;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxInt comboFieldOfView;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnDefaultFOV;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboQualityPreset;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboAntiAliasing;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat comboAntiAliasingSharp;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboTextureQuality;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboTextureFilter;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboReflectionQuality;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboReflectedShadows;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboWaterQuality;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboViewDistance;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat comboLODDistance;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboShadowsDistance;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboShadowsQuality;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboTerrainQuality;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboObjectQuality;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboGrassDistance;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboOcclusion;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboDymeshEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<int> comboDymeshDistance;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboDymeshHighQualityMesh;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<int> comboDymeshMaxRegions;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<int> comboDymeshMaxMesh;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboDymeshLandClaimOnly;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<int> comboDymeshLandClaimBuffer;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboBloom;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboDepthOfField;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboMotionBlur;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboSSAO;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboSSReflections;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboSunShafts;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat comboParticles;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat comboUIBackgroundOpacity;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat comboUIForegroundOpacity;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat comboUiSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat comboScreenBounds;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat comboUiFpsScaling;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnBack;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnDefaults;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnApply;

	[PublicizedFrom(EAccessModifier.Private)]
	public object[] previousSettings;

	[PublicizedFrom(EAccessModifier.Private)]
	public int origLength_ReflectionQuality;

	[PublicizedFrom(EAccessModifier.Private)]
	public int origLength_ShadowDistance;

	[PublicizedFrom(EAccessModifier.Private)]
	public int origLength_ShadowQuality;

	[PublicizedFrom(EAccessModifier.Private)]
	public int origLength_TerrainQuality;

	[PublicizedFrom(EAccessModifier.Private)]
	public int origLength_ObjectQuality;

	[PublicizedFrom(EAccessModifier.Private)]
	public int origLength_GrassDistance;

	[PublicizedFrom(EAccessModifier.Private)]
	public int origLength_MotionBlur;

	[PublicizedFrom(EAccessModifier.Private)]
	public int origLength_SSR;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool origDymeshEnabled;

	public struct ResolutionInfo : IComparable<XUiC_OptionsVideo.ResolutionInfo>, IEquatable<XUiC_OptionsVideo.ResolutionInfo>
	{
		[return: TupleElementNames(new string[]
		{
			"_aspectRatio",
			"_aspectRatioFactor",
			"_aspectRatioString"
		})]
		public static ValueTuple<XUiC_OptionsVideo.ResolutionInfo.EAspectRatio, float, string> DimensionsToAspectRatio(int _width, int _height)
		{
			if (_height == 0)
			{
				return new ValueTuple<XUiC_OptionsVideo.ResolutionInfo.EAspectRatio, float, string>(XUiC_OptionsVideo.ResolutionInfo.EAspectRatio.Unknown, 0f, "n/a");
			}
			int num = 1000 * _width / _height;
			if (num <= 1770)
			{
				if (num <= 1500)
				{
					if (num <= 1250)
					{
						if (num == 1000)
						{
							return new ValueTuple<XUiC_OptionsVideo.ResolutionInfo.EAspectRatio, float, string>(XUiC_OptionsVideo.ResolutionInfo.EAspectRatio.Aspect_1_1, 1f, "1:1");
						}
						if (num != 1250)
						{
							goto IL_203;
						}
						return new ValueTuple<XUiC_OptionsVideo.ResolutionInfo.EAspectRatio, float, string>(XUiC_OptionsVideo.ResolutionInfo.EAspectRatio.Aspect_5_4, 1.25f, "5:4");
					}
					else
					{
						if (num == 1333)
						{
							return new ValueTuple<XUiC_OptionsVideo.ResolutionInfo.EAspectRatio, float, string>(XUiC_OptionsVideo.ResolutionInfo.EAspectRatio.Aspect_4_3, 1.333f, "4:3");
						}
						if (num != 1500)
						{
							goto IL_203;
						}
						return new ValueTuple<XUiC_OptionsVideo.ResolutionInfo.EAspectRatio, float, string>(XUiC_OptionsVideo.ResolutionInfo.EAspectRatio.Aspect_3_2, 1.5f, "3:2");
					}
				}
				else if (num <= 1600)
				{
					if (num == 1562)
					{
						return new ValueTuple<XUiC_OptionsVideo.ResolutionInfo.EAspectRatio, float, string>(XUiC_OptionsVideo.ResolutionInfo.EAspectRatio.Aspect_25_16, 1.562f, "25:16");
					}
					if (num != 1600)
					{
						goto IL_203;
					}
					return new ValueTuple<XUiC_OptionsVideo.ResolutionInfo.EAspectRatio, float, string>(XUiC_OptionsVideo.ResolutionInfo.EAspectRatio.Aspect_16_10, 1.6f, "16:10");
				}
				else
				{
					if (num == 1666)
					{
						return new ValueTuple<XUiC_OptionsVideo.ResolutionInfo.EAspectRatio, float, string>(XUiC_OptionsVideo.ResolutionInfo.EAspectRatio.Aspect_5_3, 1.666f, "5:3");
					}
					if (num != 1770)
					{
						goto IL_203;
					}
				}
			}
			else if (num <= 2370)
			{
				if (num <= 1896)
				{
					if (num - 1777 > 1)
					{
						if (num != 1896)
						{
							goto IL_203;
						}
						return new ValueTuple<XUiC_OptionsVideo.ResolutionInfo.EAspectRatio, float, string>(XUiC_OptionsVideo.ResolutionInfo.EAspectRatio.Aspect_17_9, 2.37f, "17:9");
					}
				}
				else
				{
					if (num != 2333 && num != 2370)
					{
						goto IL_203;
					}
					return new ValueTuple<XUiC_OptionsVideo.ResolutionInfo.EAspectRatio, float, string>(XUiC_OptionsVideo.ResolutionInfo.EAspectRatio.Aspect_21_9, 2.37f, "21:9");
				}
			}
			else if (num <= 3200)
			{
				if (num == 2400)
				{
					return new ValueTuple<XUiC_OptionsVideo.ResolutionInfo.EAspectRatio, float, string>(XUiC_OptionsVideo.ResolutionInfo.EAspectRatio.Aspect_24_10, 2.4f, "24:10");
				}
				if (num != 3200)
				{
					goto IL_203;
				}
				return new ValueTuple<XUiC_OptionsVideo.ResolutionInfo.EAspectRatio, float, string>(XUiC_OptionsVideo.ResolutionInfo.EAspectRatio.Aspect_32_10, 3.2f, "32:10");
			}
			else
			{
				if (num - 3555 <= 1 || num == 3600)
				{
					return new ValueTuple<XUiC_OptionsVideo.ResolutionInfo.EAspectRatio, float, string>(XUiC_OptionsVideo.ResolutionInfo.EAspectRatio.Aspect_32_9, 3.555f, "32:9");
				}
				goto IL_203;
			}
			return new ValueTuple<XUiC_OptionsVideo.ResolutionInfo.EAspectRatio, float, string>(XUiC_OptionsVideo.ResolutionInfo.EAspectRatio.Aspect_16_9, 1.777f, "16:9");
			IL_203:
			float num2 = (float)_width / (float)_height;
			return new ValueTuple<XUiC_OptionsVideo.ResolutionInfo.EAspectRatio, float, string>(XUiC_OptionsVideo.ResolutionInfo.EAspectRatio.Unknown, num2, num2.ToCultureInvariantString("0.##") + ":1");
		}

		public ResolutionInfo(int _width, int _height)
		{
			this.Width = _width;
			if (_height > _width)
			{
				_height = _width;
			}
			this.Height = _height;
			ValueTuple<XUiC_OptionsVideo.ResolutionInfo.EAspectRatio, float, string> valueTuple = XUiC_OptionsVideo.ResolutionInfo.DimensionsToAspectRatio(_width, _height);
			this.AspectRatio = valueTuple.Item1;
			string item = valueTuple.Item3;
			this.Label = string.Concat(new string[]
			{
				_width.ToString(),
				"x",
				_height.ToString(),
				" (",
				item,
				")"
			});
		}

		public int CompareTo(XUiC_OptionsVideo.ResolutionInfo _other)
		{
			int num = this.Width.CompareTo(_other.Width);
			if (num == 0)
			{
				return this.Height.CompareTo(_other.Height);
			}
			return num;
		}

		public bool Equals(XUiC_OptionsVideo.ResolutionInfo _other)
		{
			return this.Width == _other.Width && this.Height == _other.Height;
		}

		public override string ToString()
		{
			return this.Label;
		}

		public readonly int Width;

		public readonly int Height;

		public readonly XUiC_OptionsVideo.ResolutionInfo.EAspectRatio AspectRatio;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string Label;

		public enum EAspectRatio
		{
			Aspect_32_9,
			Aspect_32_10,
			Aspect_24_10,
			Aspect_21_9,
			Aspect_17_9,
			Aspect_16_9,
			Aspect_5_3,
			Aspect_16_10,
			Aspect_25_16,
			Aspect_3_2,
			Aspect_4_3,
			Aspect_5_4,
			Aspect_1_1,
			Unknown
		}
	}
}

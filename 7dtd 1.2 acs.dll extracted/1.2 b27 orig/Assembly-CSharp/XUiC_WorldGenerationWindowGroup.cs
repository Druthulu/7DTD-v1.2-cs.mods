using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Scripting;
using WorldGenerationEngineFinal;

[Preserve]
public class XUiC_WorldGenerationWindowGroup : XUiController
{
	public event Action OnCountyNameChanged;

	public event Action OnWorldSizeChanged;

	public bool HasSufficientSpace
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return !SaveInfoProvider.DataLimitEnabled || this.m_pendingBytes <= this.m_totalAvailableBytes;
		}
	}

	public override bool GetBindingValue(ref string _value, string _bindingName)
	{
		if (_bindingName == "showbar")
		{
			_value = this.dataManagementBarEnabled.ToString();
			return true;
		}
		if (!(_bindingName == "canNewGame"))
		{
			return base.GetBindingValue(ref _value, _bindingName);
		}
		bool flag;
		if (!this.isGenerating)
		{
			WorldBuilder worldBuilder = this.worldBuilder;
			if (worldBuilder != null && worldBuilder.IsFinished && !worldBuilder.IsCanceled && this.worldBuilder.CanSaveData())
			{
				flag = this.HasSufficientSpace;
				goto IL_64;
			}
		}
		flag = false;
		IL_64:
		bool flag2 = flag;
		_value = flag2.ToString();
		return true;
	}

	public override void Init()
	{
		base.Init();
		this.PreviewWindow = base.GetChildByType<XUiC_WorldGenerationPreview>();
		this.SeedInput = base.GetChildByType<XUiC_TextInput>();
		this.GenerateButton = (base.GetChildById("generate") as XUiC_SimpleButton);
		this.TerrainAndBiomeOnly = (base.GetChildById("cbxTerrainAndBiomeOnly") as XUiC_ComboBoxBool);
		this.BackButton = (base.GetChildById("btnBack") as XUiC_SimpleButton);
		this.NewGameButton = (base.GetChildById("btnNewGame") as XUiC_SimpleButton);
		this.TerrainAndBiomeOnly = (base.GetChildById("cbxTerrainAndBiomeOnly") as XUiC_ComboBoxBool);
		if (base.GetChildById("countyName") != null)
		{
			this.CountyNameLabel = (base.GetChildById("countyName").ViewComponent as XUiV_Label);
		}
		this.btnManage = (base.GetChildById("btnDataManagement") as XUiC_SimpleButton);
		this.dataManagementBar = (base.GetChildById("data_bar_controller") as XUiC_DataManagementBar);
		this.dataManagementBarEnabled = (this.dataManagementBar != null && SaveInfoProvider.DataLimitEnabled);
	}

	public static bool IsGenerating()
	{
		XUiC_WorldGenerationWindowGroup instance = XUiC_WorldGenerationWindowGroup.Instance;
		WorldBuilder worldBuilder = (instance != null) ? instance.worldBuilder : null;
		return worldBuilder != null && !worldBuilder.IsFinished;
	}

	public static void CancelGeneration()
	{
		XUiC_WorldGenerationWindowGroup instance = XUiC_WorldGenerationWindowGroup.Instance;
		WorldBuilder worldBuilder = (instance != null) ? instance.worldBuilder : null;
		if (worldBuilder != null)
		{
			worldBuilder.IsCanceled = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void NewGameButton_OnPressed(XUiController _sender, int _mouseButton)
	{
		base.xui.StartCoroutine(this.SaveAndNewGameCo());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator SaveAndNewGameCo()
	{
		if (this.isClosing)
		{
			yield break;
		}
		this.isClosing = true;
		bool shouldClose = false;
		WorldBuilder worldBuilder = this.worldBuilder;
		bool canPrompt = true;
		XUiV_Window parentWindow = base.GetParentWindow();
		yield return worldBuilder.SaveData(canPrompt, ((parentWindow != null) ? parentWindow.Controller : null) ?? this, true, delegate
		{
			shouldClose = false;
		}, null, delegate
		{
			shouldClose = true;
		});
		if (!shouldClose)
		{
			this.isClosing = false;
			yield break;
		}
		XUiC_NewContinueGame.SetIsContinueGame(base.xui, false);
		GamePrefs.Set(EnumGamePrefs.GameWorld, this.CountyName);
		this.CheckProfile(XUiC_NewContinueGame.ID);
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CheckProfile(string _windowToOpen)
	{
		base.xui.playerUI.windowManager.Close(this.windowGroup.ID);
		if (ProfileSDF.CurrentProfileName().Length == 0)
		{
			XUiC_OptionsProfiles.Open(base.xui, delegate
			{
				this.xui.playerUI.windowManager.Open(_windowToOpen, true, false, true);
			});
			return;
		}
		base.xui.playerUI.windowManager.Open(_windowToOpen, true, false, true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnBack_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.StartClose();
	}

	public override void OnOpen()
	{
		XUiC_WorldGenerationWindowGroup.Instance = this;
		base.OnOpen();
		this.isAdvancedUI = (this.windowGroup.ID == "rwgeditor");
		if (this.isAdvancedUI)
		{
			this.windowGroup.isEscClosable = false;
		}
		this.isClosing = false;
		if (!base.xui.playerUI.windowManager.IsWindowOpen(XUiC_NewContinueGame.ID))
		{
			base.xui.calloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.RWGEditor, 0f);
		}
		PathAbstractions.CacheEnabled = true;
		if (this.PreviewWindow != null)
		{
			this.prefabPreviewManager = new PrefabPreviewManager();
		}
		if ((this.WorldSizeComboBox = (base.GetChildById("WorldSize") as XUiC_ComboBoxList<int>)) != null)
		{
			if (PlatformOptimizations.EnforceMaxWorldSizeHost)
			{
				int num = this.WorldSizeComboBox.Elements.FindLastIndex((int element) => element <= PlatformOptimizations.MaxWorldSizeHost);
				if (num >= 0)
				{
					this.WorldSizeComboBox.MinIndex = 0;
					this.WorldSizeComboBox.MaxIndex = num;
					this.WorldSizeComboBox.SelectedIndex = num;
				}
			}
			if (this.WorldSizeComboBox.Elements.Contains(8192))
			{
				this.WorldSizeComboBox.Value = 8192;
			}
		}
		this.SaveDataLimitComboBox = SaveDataLimitUIHelper.AddComboBox(base.GetChildById("SaveDataLimitComboBox") as XUiC_ComboBoxEnum<SaveDataLimitType>);
		if ((this.Rivers = (base.GetChildById("Rivers") as XUiC_ComboBoxEnum<WorldBuilder.GenerationSelections>)) != null)
		{
			this.Rivers.Value = WorldBuilder.GenerationSelections.Default;
		}
		if ((this.Craters = (base.GetChildById("Craters") as XUiC_ComboBoxEnum<WorldBuilder.GenerationSelections>)) != null)
		{
			this.Craters.Value = WorldBuilder.GenerationSelections.Default;
		}
		if ((this.Canyons = (base.GetChildById("Cracks") as XUiC_ComboBoxEnum<WorldBuilder.GenerationSelections>)) != null)
		{
			this.Canyons.Value = WorldBuilder.GenerationSelections.Default;
		}
		if ((this.Lakes = (base.GetChildById("Lakes") as XUiC_ComboBoxEnum<WorldBuilder.GenerationSelections>)) != null)
		{
			this.Lakes.Value = WorldBuilder.GenerationSelections.Default;
		}
		if ((this.Rural = (base.GetChildById("Rural") as XUiC_ComboBoxEnum<WorldBuilder.GenerationSelections>)) != null)
		{
			this.Rural.Value = WorldBuilder.GenerationSelections.Default;
		}
		if ((this.Town = (base.GetChildById("Town") as XUiC_ComboBoxEnum<WorldBuilder.GenerationSelections>)) != null)
		{
			this.Town.Value = WorldBuilder.GenerationSelections.Default;
		}
		if ((this.City = (base.GetChildById("City") as XUiC_ComboBoxEnum<WorldBuilder.GenerationSelections>)) != null)
		{
			this.City.Value = WorldBuilder.GenerationSelections.Default;
		}
		if ((this.Towns = (base.GetChildById("Towns") as XUiC_ComboBoxEnum<WorldBuilder.GenerationSelections>)) != null)
		{
			this.Towns.Value = WorldBuilder.GenerationSelections.Default;
		}
		if ((this.Wilderness = (base.GetChildById("Wilderness") as XUiC_ComboBoxEnum<WorldBuilder.GenerationSelections>)) != null)
		{
			this.Wilderness.Value = WorldBuilder.GenerationSelections.Default;
		}
		if ((this.PlainsWeight = (base.GetChildById("PlainsWeight") as XUiC_ComboBoxInt)) != null)
		{
			this.PlainsWeight.Value = 4L;
			this.PlainsWeight.OnValueChanged += this.PlainsWeight_OnValueChanged;
		}
		if ((this.HillsWeight = (base.GetChildById("HillsWeight") as XUiC_ComboBoxInt)) != null)
		{
			this.HillsWeight.Value = 4L;
			this.HillsWeight.OnValueChanged += this.HillsWeight_OnValueChanged;
		}
		if ((this.MountainsWeight = (base.GetChildById("MountainsWeight") as XUiC_ComboBoxInt)) != null)
		{
			this.MountainsWeight.Value = 2L;
			this.MountainsWeight.OnValueChanged += this.MountainsWeight_OnValueChanged;
		}
		if ((this.BiomeLayoutComboBox = (base.GetChildById("BiomeLayout") as XUiC_ComboBoxEnum<WorldBuilder.BiomeLayout>)) != null)
		{
			this.BiomeLayoutComboBox.Value = WorldBuilder.BiomeLayout.CenterForest;
		}
		XUiController childById = base.GetChildById("biomes");
		if (childById != null)
		{
			for (int i = 0; i < 5; i++)
			{
				XUiController xuiController = childById.Children[i];
				XUiController childById2 = xuiController.GetChildById("label");
				if (childById2 != null)
				{
					((XUiV_Label)childById2.ViewComponent).Text = Localization.Get(this.BiomeToUIName[i], false);
				}
				XUiC_ComboBoxInt childByType = xuiController.GetChildByType<XUiC_ComboBoxInt>();
				this.biomeComboBoxes[i] = childByType;
				if (childByType != null)
				{
					childByType.Value = (long)WorldBuilderConstants.BiomeWeightDefaults[i];
					childByType.OnValueChanged += this.BiomeWeight_OnValueChanged;
				}
				XUiController childById3 = xuiController.GetChildById("color");
				if (childById3 != null)
				{
					XUiV_Sprite xuiV_Sprite = (XUiV_Sprite)childById3.ViewComponent;
					Color color = WorldBuilderConstants.biomeColorList[i] * 0.7f;
					color.a = 1f;
					xuiV_Sprite.Color = color;
				}
			}
		}
		this.updateTerrainPercentages(false);
		this.updateBiomePercentages();
		if (this.BackButton != null)
		{
			this.BackButton.OnPressed += this.BtnBack_OnPressed;
		}
		if (this.GenerateButton != null)
		{
			this.GenerateButton.OnPressed += this.GenerateButton_OnPressed;
		}
		if (this.NewGameButton != null)
		{
			this.NewGameButton.OnPressed += this.NewGameButton_OnPressed;
		}
		if (this.btnManage != null)
		{
			this.btnManage.OnPressed += this.BtnManage_OnPressed;
		}
		if ((this.Quality = (base.GetChildById("PreviewQuality") as XUiC_ComboBoxEnum<XUiC_WorldGenerationWindowGroup.PreviewQuality>)) != null)
		{
			this.Quality.Value = XUiC_WorldGenerationWindowGroup.PreviewQuality.Default;
			this.Quality.OnValueChanged += this.Quality_OnValueChanged;
		}
		if (this.SeedInput != null)
		{
			this.SeedInput.OnChangeHandler += this.SeedInput_OnChangeHandler;
			this.SeedInput_OnChangeHandler(this.SeedInput, this.SeedInput.Text, true);
		}
		if (this.WorldSizeComboBox != null)
		{
			this.WorldSizeComboBox.OnValueChanged += this.WorldSizeComboBox_OnValueChanged;
			this.WorldSizeComboBox_OnValueChanged(this.WorldSizeComboBox, this.WorldSizeComboBox.Value - 1, this.WorldSizeComboBox.Value);
		}
		this.oldFogDensity = RenderSettings.fogDensity;
		RenderSettings.fogDensity = 0f;
		this.UpdateBarValues();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Quality_OnValueChanged(XUiController _sender, XUiC_WorldGenerationWindowGroup.PreviewQuality _oldValue, XUiC_WorldGenerationWindowGroup.PreviewQuality _newValue)
	{
		this.PreviewQualityLevel = _newValue;
		if (XUiC_WorldGenerationPreview.Instance != null)
		{
			XUiC_WorldGenerationPreview.Instance.GeneratePreview();
		}
		if (this.prefabPreviewManager != null && this.prefabPreviewManager.initialized && (_oldValue < XUiC_WorldGenerationWindowGroup.PreviewQuality.Low || _oldValue > XUiC_WorldGenerationWindowGroup.PreviewQuality.High || _newValue < XUiC_WorldGenerationWindowGroup.PreviewQuality.Low || _newValue > XUiC_WorldGenerationWindowGroup.PreviewQuality.High))
		{
			this.prefabPreviewManager.RemovePrefabs();
			this.prefabPreviewManager.InitPrefabs();
			this.prefabPreviewManager.ForceUpdate();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BiomeWeight_OnValueChanged(XUiController _sender, long _oldValue, long _newValue)
	{
		this.updateBiomePercentages();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PlainsWeight_OnValueChanged(XUiController _sender, long _oldValue, long _newValue)
	{
		this.updateTerrainPercentages(false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HillsWeight_OnValueChanged(XUiController _sender, long _oldValue, long _newValue)
	{
		this.updateTerrainPercentages(false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void MountainsWeight_OnValueChanged(XUiController _sender, long _oldValue, long _newValue)
	{
		this.updateTerrainPercentages(true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateTerrainPercentages(bool _isMountainsChanged = false)
	{
		if (this.PlainsWeight == null || this.HillsWeight == null || this.MountainsWeight == null)
		{
			return;
		}
		int num2;
		int num3;
		int num4;
		for (;;)
		{
			float num = (float)(this.PlainsWeight.Value + this.HillsWeight.Value + this.MountainsWeight.Value);
			if (num <= 0f)
			{
				num = 1f;
			}
			num2 = Mathf.RoundToInt((float)this.PlainsWeight.Value / num * 100f);
			num3 = Mathf.RoundToInt((float)this.HillsWeight.Value / num * 100f);
			num4 = Mathf.RoundToInt((float)this.MountainsWeight.Value / num * 100f);
			if (num4 <= 50)
			{
				break;
			}
			if (_isMountainsChanged)
			{
				this.PlainsWeight.Value += 1L;
			}
			else
			{
				this.MountainsWeight.Value -= 1L;
			}
		}
		if (num2 + num3 + num4 == 0)
		{
			num2 = 100;
		}
		this.PlainsWeight.UpdateLabel(string.Format("{0}%", Mathf.Max(0, num2)));
		this.HillsWeight.UpdateLabel(string.Format("{0}%", Mathf.Max(0, num3)));
		this.MountainsWeight.UpdateLabel(string.Format("{0}%", Mathf.Max(0, num4)));
		this.PlainsWeight.IsDirty = true;
		this.HillsWeight.IsDirty = true;
		this.MountainsWeight.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateBiomePercentages()
	{
		float num = 0f;
		float num2 = 0f;
		int num3 = 0;
		for (int i = 0; i < 5; i++)
		{
			XUiC_ComboBoxInt xuiC_ComboBoxInt = this.biomeComboBoxes[i];
			if (xuiC_ComboBoxInt == null)
			{
				return;
			}
			float num4 = (float)xuiC_ComboBoxInt.Value;
			num += num4;
			if (num4 > num2)
			{
				num2 = num4;
				num3 = i;
			}
		}
		int num5 = 0;
		for (int j = 0; j < 5; j++)
		{
			XUiC_ComboBoxInt xuiC_ComboBoxInt2 = this.biomeComboBoxes[j];
			int num6 = Mathf.RoundToInt((float)xuiC_ComboBoxInt2.Value / num * 100f);
			num6 = Utils.FastMax(5, num6);
			num5 += num6;
			if (j == 4)
			{
				num6 += 100 - num5;
				if (num6 < 5)
				{
					XUiC_ComboBoxInt xuiC_ComboBoxInt3 = this.biomeComboBoxes[num3];
					int num7 = Mathf.RoundToInt((float)xuiC_ComboBoxInt3.Value / num * 100f);
					xuiC_ComboBoxInt3.UpdateLabel(string.Format("{0}%", num7 + 5 - num6));
					num6 = 5;
				}
			}
			xuiC_ComboBoxInt2.UpdateLabel(string.Format("{0}%", num6));
			xuiC_ComboBoxInt2.IsDirty = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WorldSizeComboBox_OnValueChanged(XUiController _sender, int _oldValue, int _newValue)
	{
		this.WorldSize = _newValue;
		this.RefreshCountyName();
		Action onWorldSizeChanged = this.OnWorldSizeChanged;
		if (onWorldSizeChanged == null)
		{
			return;
		}
		onWorldSizeChanged();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SeedInput_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		this.RefreshCountyName();
	}

	public void RefreshCountyName()
	{
		this.CountyName = WorldBuilder.GetGeneratedWorldName(this.SeedInput.Text, this.WorldSize);
		this.ValidateNewRwg();
		if (this.CountyNameLabel != null)
		{
			this.CountyNameLabel.Text = this.CountyName;
		}
		this.TriggerCountyNameChangedEvent();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ValidateNewRwg()
	{
		string countyName = this.CountyName;
		bool flag = PathAbstractions.WorldsSearchPaths.GetLocation(countyName, countyName, null).Type != PathAbstractions.EAbstractedLocationType.None;
		this.ValidCountyName = !flag;
		if (this.CountyNameLabel != null)
		{
			this.CountyNameLabel.Color = (this.ValidCountyName ? Color.white : Color.red);
			if (flag)
			{
				this.CountyNameLabel.ToolTip = Localization.Get("mmLblRwgSeedErrorWorldExists", false);
				return;
			}
			this.CountyNameLabel.ToolTip = "";
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TriggerCountyNameChangedEvent()
	{
		if (this.OnCountyNameChanged != null)
		{
			this.OnCountyNameChanged();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnManage_OnPressed(XUiController _sender, int _mouseButton)
	{
		XUiC_DataManagement.OpenDataManagementWindow(this, new Action(this.OnDataManagementWindowClosed));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDataManagementWindowClosed()
	{
		this.UpdateBarValues();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateBarValues()
	{
		if (!this.dataManagementBarEnabled)
		{
			base.RefreshBindings(false);
			return;
		}
		SaveInfoProvider instance = SaveInfoProvider.Instance;
		this.dataManagementBar.SetUsedBytes(instance.TotalUsedBytes);
		this.dataManagementBar.SetAllowanceBytes(instance.TotalAllowanceBytes);
		this.dataManagementBar.SetDisplayMode(XUiC_DataManagementBar.DisplayMode.Preview);
		WorldBuilder worldBuilder = this.worldBuilder;
		this.m_pendingBytes = ((worldBuilder != null) ? worldBuilder.SerializedSize : 0L);
		this.m_totalAvailableBytes = instance.TotalAvailableBytes;
		this.dataManagementBar.SetPendingBytes(this.m_pendingBytes);
		base.RefreshBindings(false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GenerateButton_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (this.PreviewWindow != null)
		{
			this.PreviewWindow.CleanupTerrainMesh();
		}
		if (this.prefabPreviewManager != null)
		{
			this.prefabPreviewManager.ClearOldPreview();
		}
		ThreadManager.StartCoroutine(this.GenerateCo(true, null, null));
	}

	public IEnumerator GenerateCo(bool _usePreviewer = true, Action<string> onSuccess = null, Action onFailure = null)
	{
		this.isGenerating = true;
		this.UpdateBarValues();
		this.DestroyBuilder();
		this.worldBuilder = new WorldBuilder(this.SeedInput.Text, this.WorldSizeComboBox.Value);
		this.worldBuilder.UsePreviewer = _usePreviewer;
		if (this.Towns != null)
		{
			this.worldBuilder.Towns = this.Towns.Value;
		}
		if (this.Wilderness != null)
		{
			this.worldBuilder.Wilderness = this.Wilderness.Value;
		}
		if (this.Rivers != null)
		{
			this.worldBuilder.Rivers = this.Rivers.Value;
		}
		if (this.Craters != null)
		{
			this.worldBuilder.Craters = this.Craters.Value;
		}
		if (this.Canyons != null)
		{
			this.worldBuilder.Canyons = this.Canyons.Value;
		}
		if (this.Lakes != null)
		{
			this.worldBuilder.Lakes = this.Lakes.Value;
		}
		if (this.PlainsWeight != null)
		{
			this.worldBuilder.Plains = (int)this.PlainsWeight.Value;
		}
		if (this.HillsWeight != null)
		{
			this.worldBuilder.Hills = (int)this.HillsWeight.Value;
		}
		if (this.MountainsWeight != null)
		{
			this.worldBuilder.Mountains = (int)this.MountainsWeight.Value;
		}
		if (this.BiomeLayoutComboBox != null)
		{
			this.worldBuilder.biomeLayout = this.BiomeLayoutComboBox.Value;
		}
		for (int i = 0; i < 5; i++)
		{
			XUiC_ComboBoxInt xuiC_ComboBoxInt = this.biomeComboBoxes[i];
			if (xuiC_ComboBoxInt != null)
			{
				this.worldBuilder.SetBiomeWeight((BiomeType)i, (int)xuiC_ComboBoxInt.Value);
			}
		}
		if (this.Quality != null)
		{
			this.PreviewQualityLevel = this.Quality.Value;
		}
		PrefabPreviewManager.ReadyToDisplay = false;
		this.UpdateBarValues();
		yield return GCUtils.WaitForIdle();
		yield return this.worldBuilder.GenerateFromUI();
		if (this.worldBuilder.UsePreviewer)
		{
			yield return this.worldBuilder.FinishForPreview();
			if (XUiC_WorldGenerationPreview.Instance != null)
			{
				XUiC_WorldGenerationPreview.Instance.GeneratePreview();
			}
			if (!this.worldBuilder.IsCanceled)
			{
				yield return new WaitForSeconds(2f);
			}
		}
		else
		{
			XUiC_WorldGenerationWindowGroup.<>c__DisplayClass78_0 CS$<>8__locals1 = new XUiC_WorldGenerationWindowGroup.<>c__DisplayClass78_0();
			XUiC_ProgressWindow.Close(LocalPlayerUI.primaryUI);
			CS$<>8__locals1.success = false;
			WorldBuilder worldBuilder = this.worldBuilder;
			bool canPrompt = true;
			XUiV_Window parentWindow = base.GetParentWindow();
			yield return worldBuilder.SaveData(canPrompt, ((parentWindow != null) ? parentWindow.Controller : null) ?? this, true, null, delegate
			{
				CS$<>8__locals1.success = false;
			}, delegate
			{
				CS$<>8__locals1.success = true;
			});
			if (CS$<>8__locals1.success)
			{
				if (onSuccess != null)
				{
					onSuccess(this.worldBuilder.WorldName);
				}
			}
			else if (onFailure != null)
			{
				onFailure();
			}
			this.DestroyBuilder();
			CS$<>8__locals1 = null;
		}
		XUiC_ProgressWindow.Close(LocalPlayerUI.primaryUI);
		PrefabPreviewManager.ReadyToDisplay = true;
		this.isGenerating = false;
		this.UpdateBarValues();
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DestroyBuilder()
	{
		if (this.worldBuilder != null)
		{
			this.worldBuilder.Cleanup();
			this.worldBuilder = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void StartClose()
	{
		if (this.isGenerating)
		{
			return;
		}
		if (this.worldBuilder == null || !this.worldBuilder.IsFinished || !this.worldBuilder.CanSaveData())
		{
			this.Close();
			return;
		}
		base.xui.StartCoroutine(this.StartCloseCo());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator StartCloseCo()
	{
		if (this.isClosing)
		{
			yield break;
		}
		this.isClosing = true;
		bool shouldClose = false;
		WorldBuilder worldBuilder = this.worldBuilder;
		bool canPrompt = true;
		XUiV_Window parentWindow = base.GetParentWindow();
		yield return worldBuilder.SaveData(canPrompt, ((parentWindow != null) ? parentWindow.Controller : null) ?? this, false, delegate
		{
			shouldClose = false;
		}, delegate
		{
			shouldClose = true;
		}, delegate
		{
			shouldClose = true;
		});
		if (!shouldClose)
		{
			this.UpdateBarValues();
			this.isClosing = false;
			yield break;
		}
		this.Close();
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Close()
	{
		base.xui.playerUI.windowManager.Close(this.windowGroup.ID);
		base.xui.playerUI.windowManager.Open(this.LastWindowID, true, false, true);
	}

	public override void OnClose()
	{
		base.OnClose();
		base.xui.calloutWindow.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.RWGEditor);
		base.xui.calloutWindow.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.RWGCamera);
		base.xui.calloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu, 0f);
		this.Clean();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Clean()
	{
		this.DestroyBuilder();
		PathAbstractions.CacheEnabled = false;
		if (this.BackButton != null)
		{
			this.BackButton.OnPressed -= this.BtnBack_OnPressed;
		}
		if (this.GenerateButton != null)
		{
			this.GenerateButton.OnPressed -= this.GenerateButton_OnPressed;
		}
		if (this.NewGameButton != null)
		{
			this.NewGameButton.OnPressed -= this.NewGameButton_OnPressed;
		}
		if (this.btnManage != null)
		{
			this.btnManage.OnPressed -= this.BtnManage_OnPressed;
		}
		if (this.Quality != null)
		{
			this.Quality.OnValueChanged -= this.Quality_OnValueChanged;
		}
		if (this.SeedInput != null)
		{
			this.SeedInput.OnChangeHandler -= this.SeedInput_OnChangeHandler;
		}
		if (this.WorldSizeComboBox != null)
		{
			this.WorldSizeComboBox.OnValueChanged -= this.WorldSizeComboBox_OnValueChanged;
		}
		if (this.PlainsWeight != null)
		{
			this.PlainsWeight.OnValueChanged -= this.PlainsWeight_OnValueChanged;
		}
		if (this.HillsWeight != null)
		{
			this.HillsWeight.OnValueChanged -= this.HillsWeight_OnValueChanged;
		}
		if (this.MountainsWeight != null)
		{
			this.MountainsWeight.OnValueChanged -= this.MountainsWeight_OnValueChanged;
		}
		if (this.prefabPreviewManager != null)
		{
			this.prefabPreviewManager.Cleanup();
			this.prefabPreviewManager = null;
		}
		RenderSettings.fogDensity = this.oldFogDensity;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (!this.isAdvancedUI)
		{
			return;
		}
		if (base.xui.playerUI.playerInput.PermanentActions.Cancel.WasPressed && !XUiC_DataManagement.IsWindowOpen(base.xui))
		{
			this.StartClose();
		}
		if (PrefabPreviewManager.ReadyToDisplay && this.prefabPreviewManager != null)
		{
			this.prefabPreviewManager.Update();
		}
	}

	public static XUiC_WorldGenerationWindowGroup Instance;

	public string LastWindowID = string.Empty;

	public DynamicPrefabDecorator PrefabDecorator;

	public XUiC_WorldGenerationPreview PreviewWindow;

	public PrefabPreviewManager prefabPreviewManager;

	public XUiC_TextInput SeedInput;

	public XUiC_SimpleButton GenerateButton;

	public XUiC_SimpleButton BackButton;

	public XUiC_SimpleButton NewGameButton;

	public XUiC_ComboBoxList<int> WorldSizeComboBox;

	public XUiC_ComboBoxEnum<SaveDataLimitType> SaveDataLimitComboBox;

	public XUiC_ComboBoxBool TerrainAndBiomeOnly;

	public XUiV_Label CountyNameLabel;

	public XUiC_ComboBoxEnum<WorldBuilder.BiomeLayout> BiomeLayoutComboBox;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxInt[] biomeComboBoxes = new XUiC_ComboBoxInt[5];

	public XUiC_ComboBoxInt PlainsWeight;

	public XUiC_ComboBoxInt HillsWeight;

	public XUiC_ComboBoxInt MountainsWeight;

	public XUiC_ComboBoxEnum<XUiC_WorldGenerationWindowGroup.PreviewQuality> Quality;

	public XUiC_ComboBoxEnum<WorldBuilder.GenerationSelections> Rivers;

	public XUiC_ComboBoxEnum<WorldBuilder.GenerationSelections> Craters;

	public XUiC_ComboBoxEnum<WorldBuilder.GenerationSelections> Canyons;

	public XUiC_ComboBoxEnum<WorldBuilder.GenerationSelections> Lakes;

	public XUiC_ComboBoxEnum<WorldBuilder.GenerationSelections> Rural;

	public XUiC_ComboBoxEnum<WorldBuilder.GenerationSelections> Town;

	public XUiC_ComboBoxEnum<WorldBuilder.GenerationSelections> City;

	public XUiC_ComboBoxEnum<WorldBuilder.GenerationSelections> Towns;

	public XUiC_ComboBoxEnum<WorldBuilder.GenerationSelections> Wilderness;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnManage;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_DataManagementBar dataManagementBar;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool dataManagementBarEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public long m_pendingBytes;

	[PublicizedFrom(EAccessModifier.Private)]
	public long m_totalAvailableBytes;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isAdvancedUI;

	public int WorldSize;

	public bool ValidCountyName;

	[PublicizedFrom(EAccessModifier.Private)]
	public string CountyName;

	[PublicizedFrom(EAccessModifier.Private)]
	public float oldFogDensity;

	public XUiC_WorldGenerationWindowGroup.PreviewQuality PreviewQualityLevel = XUiC_WorldGenerationWindowGroup.PreviewQuality.Default;

	public WorldBuilder worldBuilder;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isGenerating;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isClosing;

	[PublicizedFrom(EAccessModifier.Private)]
	public string[] BiomeToUIName = new string[]
	{
		"xuiPineForest",
		"xuiBurntForest",
		"xuiDesert",
		"xuiSnow",
		"xuiWasteland"
	};

	public struct PrefabData
	{
		public string Name;

		public Vector3i Position;

		public byte Rotation;

		public string DistantPOIOverride;

		public int ID;
	}

	public enum PreviewQuality
	{
		NoPreview,
		Lowest,
		Low,
		Default,
		High,
		Highest
	}
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_InGameDebugMenu : XUiController
{
	public ChunkCluster ChunkCluster0
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (GameManager.Instance == null)
			{
				return null;
			}
			if (GameManager.Instance.World != null)
			{
				return GameManager.Instance.World.ChunkClusters[0];
			}
			return null;
		}
	}

	public bool HasWorld
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return GameManager.Instance.World != null;
		}
	}

	public override bool GetBindingValue(ref string value, string bindingName)
	{
		if (bindingName == "decorationsState")
		{
			if (this.HasWorld)
			{
				value = ((this.ChunkCluster0 != null && this.ChunkCluster0.ChunkProvider.IsDecorationsEnabled()) ? "On" : "Off");
			}
			else
			{
				value = "N/A";
			}
			return true;
		}
		if (bindingName == "gameTicksName")
		{
			value = Localization.Get("xuiDebugTicks", false);
			return true;
		}
		if (!(bindingName == "gameTicks"))
		{
			return base.GetBindingValue(ref value, bindingName);
		}
		value = this.ticksFormatter.Format(GameTimer.Instance.ticks);
		return true;
	}

	public override void Init()
	{
		base.Init();
		XUiC_InGameDebugMenu.ID = base.WindowGroup.ID;
		this.btnSuicide = base.GetChildById("btnSuicide");
		this.btnSuicide.GetChildById("clickable").OnPress += this.BtnSuicide_Controller_OnPress;
		this.teleportX = (base.GetChildById("teleportX") as XUiC_TextInput);
		this.teleportZ = (base.GetChildById("teleportZ") as XUiC_TextInput);
		this.teleportX.OnSubmitHandler += this.Teleport_OnSubmitHandler;
		this.teleportZ.OnSubmitHandler += this.Teleport_OnSubmitHandler;
		this.teleportX.SelectOnTab = this.teleportZ;
		this.teleportZ.SelectOnTab = this.teleportX;
		this.teleportX.Text = XUiC_InGameDebugMenu.LastTeleportX.ToString();
		this.teleportZ.Text = XUiC_InGameDebugMenu.LastTeleportZ.ToString();
		this.btnTeleport = base.GetChildById("btnTeleport");
		this.btnTeleport.GetChildById("clickable").OnPress += this.BtnTeleport_Controller_OnPress;
		((XUiV_Button)base.GetChildById("btnRecalcLight").GetChildById("clickable").ViewComponent).Controller.OnPress += this.BtnRecalcLight_Controller_OnPress;
		((XUiV_Button)base.GetChildById("btnRecalcStability").GetChildById("clickable").ViewComponent).Controller.OnPress += this.BtnRecalcStability_Controller_OnPress;
		this.btnReloadChunks = base.GetChildById("btnReloadChunks");
		this.btnReloadChunks.GetChildById("clickable").OnPress += this.BtnReloadChunks_Controller_OnPress;
		this.sliderDay = base.GetChildById("sliderDayRect").GetChildByType<XUiC_Slider>();
		this.sliderDay.Label = Localization.Get("xuiDebugDay", false);
		this.sliderDay.ValueFormatter = new Func<float, string>(this.SliderDay_ValueFormatter);
		this.sliderDay.OnValueChanged += this.SliderDay_OnValueChanged;
		this.sliderTime = base.GetChildById("sliderTimeRect").GetChildByType<XUiC_Slider>();
		this.sliderTime.Label = Localization.Get("xuiDebugTime", false);
		this.sliderTime.ValueFormatter = new Func<float, string>(this.SliderTime_ValueFormatter);
		this.sliderTime.OnValueChanged += this.SliderTime_OnValueChanged;
		this.sliderSpeed = base.GetChildById("sliderSpeedRect").GetChildByType<XUiC_Slider>();
		this.sliderSpeed.Label = Localization.Get("xuiDebugSpeed", false);
		this.sliderSpeed.ValueFormatter = new Func<float, string>(this.SliderSpeed_ValueFormatter);
		this.sliderSpeed.OnValueChanged += this.SliderSpeed_OnValueChanged;
		this.toggleFlyMode = base.GetChildById("toggleFlyMode").GetChildByType<XUiC_ToggleButton>();
		this.toggleFlyMode.OnValueChanged += this.ToggleFlyMode_OnValueChanged;
		this.toggleGodMode = base.GetChildById("toggleGodMode").GetChildByType<XUiC_ToggleButton>();
		this.toggleGodMode.OnValueChanged += this.ToggleGodMode_OnValueChanged;
		this.toggleNoCollisionMode = base.GetChildById("toggleNoCollisionMode").GetChildByType<XUiC_ToggleButton>();
		this.toggleNoCollisionMode.OnValueChanged += this.ToggleNoCollisionMode_OnValueChanged;
		this.toggleInvisibileMode = base.GetChildById("toggleInvisibileMode").GetChildByType<XUiC_ToggleButton>();
		this.toggleInvisibileMode.OnValueChanged += this.ToggleInvisibileMode_OnValueChanged;
		this.toggleSaving = base.GetChildById("toggleSaving").GetChildByType<XUiC_ToggleButton>();
		this.toggleSaving.OnValueChanged += this.ToggleSaving_OnValueChanged;
		this.togglePhysics = base.GetChildById("togglePhysics").GetChildByType<XUiC_ToggleButton>();
		this.togglePhysics.OnValueChanged += this.TogglePhysics_OnValueChanged;
		this.toggleTicking = base.GetChildById("toggleTicking").GetChildByType<XUiC_ToggleButton>();
		this.toggleTicking.OnValueChanged += this.ToggleTicking_OnValueChanged;
		this.toggleWaterSim = base.GetChildById("toggleWaterSim").GetChildByType<XUiC_ToggleButton>();
		this.toggleWaterSim.OnValueChanged += this.ToggleWaterSim_OnValueChanged;
		this.toggleDebugShaders = base.GetChildById("toggleDebugShaders").GetChildByType<XUiC_ToggleButton>();
		this.toggleDebugShaders.OnValueChanged += this.ToggleDebugShaders_OnValueChanged;
		this.toggleLightPerformance = base.GetChildById("toggleLightPerformance").GetChildByType<XUiC_ToggleButton>();
		this.toggleLightPerformance.OnValueChanged += this.ToggleLightPerformance_OnValueChanged;
		this.toggleStabilityGlue = base.GetChildById("toggleStabilityGlue").GetChildByType<XUiC_ToggleButton>();
		this.toggleStabilityGlue.OnValueChanged += this.ToggleStabilityGlue_OnValueChanged;
		this.btnPlaytest = (base.GetChildById("btnPlaytest") as XUiC_SimpleButton);
		this.btnPlaytest.OnPressed += this.BtnPlaytestOnPressed;
		this.btnBackToEditor = (base.GetChildById("btnBackToEditor") as XUiC_SimpleButton);
		this.btnBackToEditor.OnPressed += this.BtnBackToEditorOnPressed;
		this.cbxPlaytestBiome = (base.GetChildById("cbxPlaytestBiome") as XUiC_ComboBoxList<BiomeDefinition.BiomeType>);
		this.cbxPlaytestBiome.Elements.AddRange(new BiomeDefinition.BiomeType[]
		{
			BiomeDefinition.BiomeType.Snow,
			BiomeDefinition.BiomeType.PineForest,
			BiomeDefinition.BiomeType.Desert,
			BiomeDefinition.BiomeType.Wasteland,
			BiomeDefinition.BiomeType.burnt_forest
		});
		this.cbxPlaytestBiome.Value = (BiomeDefinition.BiomeType)GamePrefs.GetInt(EnumGamePrefs.PlaytestBiome);
		this.cbxPlaytestBiome.OnValueChanged += delegate(XUiController _sender, BiomeDefinition.BiomeType _value, BiomeDefinition.BiomeType _newValue)
		{
			GamePrefs.Set(EnumGamePrefs.PlaytestBiome, (int)_newValue);
		};
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnSuicide_Controller_OnPress(XUiController _sender, int _mouseButton)
	{
		if (base.xui.playerUI.entityPlayer != null)
		{
			base.xui.playerUI.windowManager.Close(base.WindowGroup.ID);
			base.xui.playerUI.entityPlayer.Kill(DamageResponse.New(new DamageSource(EnumDamageSource.Internal, EnumDamageTypes.Suicide), true));
			GameManager.Instance.Pause(false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Teleport_OnSubmitHandler(XUiController _sender, string _text)
	{
		this.BtnTeleport_Controller_OnPress(_sender, -1);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnTeleport_Controller_OnPress(XUiController _sender, int _mouseButton)
	{
		if (base.xui.playerUI.entityPlayer != null)
		{
			int num;
			int num2;
			if (!int.TryParse(this.teleportX.Text, out num) || !int.TryParse(this.teleportZ.Text, out num2))
			{
				return;
			}
			XUiC_InGameDebugMenu.LastTeleportX = num;
			XUiC_InGameDebugMenu.LastTeleportZ = num2;
			base.xui.playerUI.windowManager.Close(base.WindowGroup.ID);
			base.xui.playerUI.entityPlayer.Teleport(new Vector3((float)num, 240f, (float)num2), float.MinValue);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnViewStabilityGlue_Controller_OnPress(XUiController _sender, int _mouseButton)
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnRecalcLight_Controller_OnPress(XUiController _sender, int _mouseButton)
	{
		if (this.ChunkCluster0 != null)
		{
			ReaderWriterLockSlim syncRoot = this.ChunkCluster0.GetSyncRoot();
			lock (syncRoot)
			{
				LightProcessor lightProcessor = new LightProcessor(GameManager.Instance.World);
				Stopwatch stopwatch = new Stopwatch();
				stopwatch.Start();
				List<Chunk> chunkArrayCopySync = this.ChunkCluster0.GetChunkArrayCopySync();
				foreach (Chunk chunk in chunkArrayCopySync)
				{
					chunk.ResetLights(0);
					chunk.RefreshSunlight();
				}
				foreach (Chunk chunk2 in chunkArrayCopySync)
				{
					lightProcessor.GenerateSunlight(chunk2, false);
				}
				foreach (Chunk chunk3 in chunkArrayCopySync)
				{
					lightProcessor.GenerateSunlight(chunk3, true);
				}
				foreach (Chunk c in chunkArrayCopySync)
				{
					lightProcessor.LightChunk(c);
				}
				stopwatch.Stop();
				foreach (Chunk chunk4 in chunkArrayCopySync)
				{
					chunk4.NeedsRegeneration = true;
				}
				Log.Out(string.Concat(new string[]
				{
					"#",
					chunkArrayCopySync.Count.ToString(),
					" chunks needed ",
					stopwatch.ElapsedMilliseconds.ToString(),
					"ms"
				}));
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RecalcStability()
	{
		ChunkCluster chunkCluster = this.ChunkCluster0;
		if (chunkCluster != null)
		{
			ReaderWriterLockSlim syncRoot = chunkCluster.GetSyncRoot();
			lock (syncRoot)
			{
				StabilityInitializer stabilityInitializer = new StabilityInitializer(GameManager.Instance.World);
				MicroStopwatch microStopwatch = new MicroStopwatch();
				foreach (Chunk chunk in chunkCluster.GetChunkArray())
				{
					chunk.ResetStabilityToBottomMost();
				}
				Log.Out("RecalcStability reset in {0}ms", new object[]
				{
					microStopwatch.ElapsedMilliseconds
				});
				foreach (Chunk chunk2 in chunkCluster.GetChunkArray())
				{
					stabilityInitializer.DistributeStability(chunk2);
					chunk2.NeedsRegeneration = true;
				}
				Log.Out("RecalcStability #{0} in {1}ms", new object[]
				{
					chunkCluster.GetChunkArray().Count,
					microStopwatch.ElapsedMilliseconds
				});
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnRecalcStability_Controller_OnPress(XUiController _sender, int _mouseButton)
	{
		this.RecalcStability();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnReloadChunks_Controller_OnPress(XUiController _sender, int _mouseButton)
	{
		if (this.ChunkCluster0 != null)
		{
			GameManager.Instance.World.m_ChunkManager.ReloadAllChunks();
			this.ChunkCluster0.ChunkProvider.ReloadAllChunks();
			GameManager.Instance.World.UnloadEntities(GameManager.Instance.World.Entities.list);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string SliderDay_ValueFormatter(float _value)
	{
		return this.SliderDay_Value().ToString();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SliderDay_OnValueChanged(XUiC_Slider _sender)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && this.HasWorld)
		{
			ulong num = GameManager.Instance.World.worldTime % 24000UL;
			GameManager.Instance.World.SetTimeJump(num + (ulong)((long)(this.SliderDay_Value() - 1) * 24000L), true);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int SliderDay_Value()
	{
		return (int)(Mathf.Clamp(this.sliderDay.Value, 0f, 0.99f) * 16f + 1f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string SliderTime_ValueFormatter(float _value)
	{
		return this.SliderTime_Value().ToCultureInvariantString("0.00");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SliderTime_OnValueChanged(XUiC_Slider _sender)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && this.HasWorld)
		{
			ulong num = GameManager.Instance.World.worldTime / 24000UL;
			ulong num2 = (ulong)(this.SliderTime_Value() * 1000f);
			GameManager.Instance.World.SetTimeJump(num2 + num * 24000UL, true);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float SliderTime_Value()
	{
		return Mathf.Clamp(this.sliderTime.Value, 0f, 0.99f) * 24f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string SliderSpeed_ValueFormatter(float _value)
	{
		return this.SliderSpeed_Value().ToString();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SliderSpeed_OnValueChanged(XUiC_Slider _sender)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			int value = this.SliderSpeed_Value();
			GameStats.Set(EnumGameStats.TimeOfDayIncPerSec, value);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int SliderSpeed_Value()
	{
		return (int)(Mathf.Clamp(this.sliderSpeed.Value, 0f, 0.99f) * 500f + 1f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ToggleFlyMode_OnValueChanged(XUiC_ToggleButton _sender, bool _newValue)
	{
		if (base.xui.playerUI.entityPlayer != null)
		{
			base.xui.playerUI.entityPlayer.IsFlyMode.Value = _newValue;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ToggleGodMode_OnValueChanged(XUiC_ToggleButton _sender, bool _newValue)
	{
		if (base.xui.playerUI.entityPlayer != null)
		{
			base.xui.playerUI.entityPlayer.IsGodMode.Value = _newValue;
			base.xui.playerUI.entityPlayer.IsNoCollisionMode.Value = _newValue;
			base.xui.playerUI.entityPlayer.IsFlyMode.Value = _newValue;
			base.xui.playerUI.entityPlayer.bEntityAliveFlagsChanged = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ToggleNoCollisionMode_OnValueChanged(XUiC_ToggleButton _sender, bool _newValue)
	{
		if (base.xui.playerUI.entityPlayer != null)
		{
			base.xui.playerUI.entityPlayer.IsNoCollisionMode.Value = _newValue;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ToggleInvisibileMode_OnValueChanged(XUiC_ToggleButton _sender, bool _newValue)
	{
		if (base.xui.playerUI.entityPlayer != null)
		{
			base.xui.playerUI.entityPlayer.IsSpectator = _newValue;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ToggleSaving_OnValueChanged(XUiC_ToggleButton _sender, bool _newValue)
	{
		GameManager.bSavingActive = _newValue;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TogglePhysics_OnValueChanged(XUiC_ToggleButton _sender, bool _newValue)
	{
		GameManager.bPhysicsActive = _newValue;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ToggleTicking_OnValueChanged(XUiC_ToggleButton _sender, bool _newValue)
	{
		GameManager.bTickingActive = _newValue;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ToggleWaterSim_OnValueChanged(XUiC_ToggleButton _sender, bool _newValue)
	{
		WaterSimulationNative.Instance.SetPaused(!_newValue);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ToggleDebugShaders_OnValueChanged(XUiC_ToggleButton _sender, bool _newValue)
	{
		MeshDescription.SetDebugStabilityShader(_newValue);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ToggleLightPerformance_OnValueChanged(XUiC_ToggleButton _sender, bool _newValue)
	{
		LightViewer.SetEnabled(_newValue);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ToggleStabilityGlue_OnValueChanged(XUiC_ToggleButton _sender, bool _newValue)
	{
		if (GameManager.Instance.stabilityViewer != null)
		{
			GameManager.Instance.ClearStabilityViewer();
			return;
		}
		GameManager.Instance.CreateStabilityViewer();
		GameManager.Instance.stabilityViewer.StartSearch(100);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (GameManager.IsDedicatedServer)
		{
			this.btnSuicide.ViewComponent.IsVisible = false;
			this.teleportX.ViewComponent.IsVisible = false;
			this.teleportZ.ViewComponent.IsVisible = false;
			this.btnTeleport.ViewComponent.IsVisible = false;
			this.toggleFlyMode.ViewComponent.IsVisible = false;
			this.toggleGodMode.ViewComponent.IsVisible = false;
			this.toggleNoCollisionMode.ViewComponent.IsVisible = false;
			this.toggleInvisibileMode.ViewComponent.IsVisible = false;
		}
		if (GameManager.Instance.IsEditMode())
		{
			this.btnSuicide.ViewComponent.IsVisible = false;
		}
		if (!GameManager.Instance.IsEditMode())
		{
			this.btnReloadChunks.ViewComponent.IsVisible = false;
		}
		bool isServer = SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer;
		bool flag = GamePrefs.GetString(EnumGamePrefs.GameWorld) == "Empty";
		bool flag2 = GameUtils.IsPlaytesting();
		this.btnPlaytest.ViewComponent.IsVisible = ((flag || flag2) && isServer);
		this.btnPlaytest.Text = (flag2 ? Localization.Get("xuiDebugMenuPlaytestReset", false) : Localization.Get("xuiDebugMenuPlaytest", false));
		this.btnBackToEditor.ViewComponent.IsVisible = ((flag || flag2) && isServer);
		this.btnBackToEditor.Enabled = flag2;
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			this.sliderDay.ViewComponent.IsVisible = false;
			this.sliderTime.ViewComponent.IsVisible = false;
			this.sliderSpeed.ViewComponent.IsVisible = false;
			this.toggleSaving.ViewComponent.IsVisible = false;
			this.togglePhysics.ViewComponent.IsVisible = false;
			this.toggleTicking.ViewComponent.IsVisible = false;
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		this.sliderDay.Value = (this.HasWorld ? (GameManager.Instance.World.worldTime / 24000UL / 16f) : 0f);
		this.sliderTime.Value = (this.HasWorld ? (GameManager.Instance.World.worldTime % 24000UL / 24000f) : 0f);
		this.sliderSpeed.Value = (this.HasWorld ? ((float)(GameStats.GetInt(EnumGameStats.TimeOfDayIncPerSec) - 1) / 500f) : 0f);
		this.toggleFlyMode.Value = (base.xui.playerUI.entityPlayer != null && base.xui.playerUI.entityPlayer.IsFlyMode.Value);
		this.toggleGodMode.Value = (base.xui.playerUI.entityPlayer != null && base.xui.playerUI.entityPlayer.IsGodMode.Value);
		this.toggleNoCollisionMode.Value = (base.xui.playerUI.entityPlayer != null && base.xui.playerUI.entityPlayer.IsNoCollisionMode.Value);
		this.toggleInvisibileMode.Value = (base.xui.playerUI.entityPlayer != null && base.xui.playerUI.entityPlayer.IsSpectator);
		this.toggleSaving.Value = GameManager.bSavingActive;
		this.togglePhysics.Value = GameManager.bPhysicsActive;
		this.toggleTicking.Value = GameManager.bTickingActive;
		this.toggleWaterSim.Value = !WaterSimulationNative.Instance.IsPaused;
		this.toggleDebugShaders.Value = MeshDescription.bDebugStability;
		this.toggleLightPerformance.Value = LightViewer.IsEnabled;
		base.RefreshBindings(true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnPlaytestOnPressed(XUiController _sender, int _mouseButton)
	{
		if (PrefabEditModeManager.Instance.IsActive() && SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			XUiC_SaveDirtyPrefab.Show(base.xui, new Action<XUiC_SaveDirtyPrefab.ESelectedAction>(this.startPlaytest), XUiC_SaveDirtyPrefab.EMode.AskSaveIfDirty);
			return;
		}
		this.startPlaytest(XUiC_SaveDirtyPrefab.ESelectedAction.DontSave);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void startPlaytest(XUiC_SaveDirtyPrefab.ESelectedAction _action)
	{
		if (_action == XUiC_SaveDirtyPrefab.ESelectedAction.Cancel)
		{
			return;
		}
		GameUtils.StartPlaytesting();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnBackToEditorOnPressed(XUiController _sender, int _mouseButton)
	{
		GameUtils.StartSinglePrefabEditing();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int LastTeleportX = 0;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int LastTeleportZ = 0;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int SLIDER_MAX_DAYS = 16;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int SLIDER_MAX_SPEED = 500;

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController btnSuicide;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController btnTeleport;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController btnReloadChunks;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput teleportX;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput teleportZ;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_Slider sliderDay;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_Slider sliderTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_Slider sliderSpeed;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ToggleButton toggleSaving;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ToggleButton togglePhysics;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ToggleButton toggleTicking;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ToggleButton toggleWaterSim;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ToggleButton toggleDebugShaders;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ToggleButton toggleLightPerformance;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ToggleButton toggleStabilityGlue;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ToggleButton toggleFlyMode;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ToggleButton toggleGodMode;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ToggleButton toggleNoCollisionMode;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ToggleButton toggleInvisibileMode;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<BiomeDefinition.BiomeType> cbxPlaytestBiome;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnPlaytest;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnBackToEditor;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<ulong> ticksFormatter = new CachedStringFormatter<ulong>((ulong _i) => _i.ToString());
}

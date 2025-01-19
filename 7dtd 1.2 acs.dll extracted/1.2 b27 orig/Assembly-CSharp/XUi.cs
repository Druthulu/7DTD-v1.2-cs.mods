using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Audio;
using Platform;
using UnityEngine;

public class XUi : MonoBehaviour
{
	public AudioClip uiScrollSound { get; [PublicizedFrom(EAccessModifier.Private)] set; }

	public AudioClip uiClickSound { get; [PublicizedFrom(EAccessModifier.Private)] set; }

	public AudioClip uiConfirmSound { get; [PublicizedFrom(EAccessModifier.Private)] set; }

	public AudioClip uiBackSound { get; [PublicizedFrom(EAccessModifier.Private)] set; }

	public AudioClip uiSliderSound { get; [PublicizedFrom(EAccessModifier.Private)] set; }

	public bool isReady
	{
		get
		{
			return this.mIsReady;
		}
		set
		{
			this.mIsReady = value;
			if (this.mIsReady && this.OnBuilt != null)
			{
				this.OnBuilt();
			}
		}
	}

	public bool isMinimal { get; set; }

	public event Action OnShutdown;

	public event Action OnBuilt;

	public bool GlobalOpacityChanged
	{
		get
		{
			return this.oldBackgroundGlobalOpacity != this.BackgroundGlobalOpacity || this.oldForegroundGlobalOpacity != this.ForegroundGlobalOpacity;
		}
	}

	public Transform StackPanelTransform
	{
		get
		{
			return this.stackPanelRoot;
		}
	}

	public static XUi Instantiate(LocalPlayerUI playerUI, GameObject xuiPrefab = null)
	{
		if (GameManager.IsDedicatedServer)
		{
			return null;
		}
		Log.Out("[XUi] Instantiating XUi from {0} prefab.", new object[]
		{
			(xuiPrefab != null) ? xuiPrefab.name : ((XUi.defaultPrefab != null) ? XUi.defaultPrefab.name : "default")
		});
		MicroStopwatch microStopwatch = new MicroStopwatch(true);
		Transform transform = UnityEngine.Object.Instantiate<GameObject>(xuiPrefab ? xuiPrefab : ((XUi.defaultPrefab != null) ? XUi.defaultPrefab : Resources.Load<GameObject>("Prefabs/XUi"))).transform;
		transform.name = transform.name.Replace("(Clone)", "").Replace("_Full", "");
		transform.parent = playerUI.transform.Find("NGUI Camera");
		XUi.UIRoot = UnityEngine.Object.FindObjectOfType<UIRoot>();
		XUi component = transform.GetComponent<XUi>();
		component.SetScale(-1f);
		component.gameObject.SetActive(true);
		component.Init(XUi.ID++);
		Log.Out("[XUi] XUi instantiation completed in {0} ms", new object[]
		{
			microStopwatch.ElapsedMilliseconds
		});
		return component;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnGamePrefChanged(EnumGamePrefs _obj)
	{
		if (_obj == EnumGamePrefs.OptionsScreenBoundsValue)
		{
			this.SetScale(-1f);
			this.UpdateAnchors();
			this.RecenterWindowGroup(null, true);
			return;
		}
		if (_obj == EnumGamePrefs.OptionsHudSize)
		{
			this.SetScale(-1f);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnResolutionChanged(int _arg1, int _arg2)
	{
		ThreadManager.StartCoroutine(this.delayedScaleUpdate());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator delayedScaleUpdate()
	{
		yield return null;
		this.SetScale(-1f);
		yield break;
	}

	public void LateInitialize()
	{
		this.LateInit();
	}

	public void Shutdown(bool _destroyImmediate = false)
	{
		if (this.OnShutdown != null)
		{
			this.OnShutdown();
		}
		this.Cleanup(_destroyImmediate);
	}

	public static void Reload(LocalPlayerUI _playerUI)
	{
		if (_playerUI.xui != null)
		{
			_playerUI.xui.Shutdown(true);
		}
		XUi.SetXmlsForUi(_playerUI);
		XUi xui = XUi.Instantiate(_playerUI, null);
		xui.Load(null, false);
		xui.SetDataConnections();
	}

	public static void ReloadWindow(LocalPlayerUI _playerUI, string _windowGroupName)
	{
		if (_playerUI.xui == null)
		{
			Log.Error("Can not reload single window, XUi not instantiated");
		}
		for (int i = 0; i < _playerUI.xui.WindowGroups.Count; i++)
		{
			XUiWindowGroup xuiWindowGroup = _playerUI.xui.WindowGroups[i];
			if (xuiWindowGroup.ID == _windowGroupName)
			{
				xuiWindowGroup.Controller.Cleanup();
				_playerUI.windowManager.Remove(xuiWindowGroup.ID);
				for (int j = 0; j < xuiWindowGroup.Controller.Children.Count; j++)
				{
					UnityEngine.Object.DestroyImmediate(xuiWindowGroup.Controller.Children[j].ViewComponent.UiTransform.gameObject);
				}
				_playerUI.xui.WindowGroups.RemoveAt(i);
				break;
			}
		}
		XUi.SetXmlsForUi(_playerUI);
		_playerUI.xui.Load(new List<string>
		{
			_windowGroupName
		}, false);
	}

	public static void SetXmlsForUi(LocalPlayerUI _playerUI)
	{
		XUiFromXml.ClearData();
		if (!_playerUI.isPrimaryUI)
		{
			ThreadManager.RunCoroutineSync(XUi.PatchAndLoadXuiXml("XUi_Common/styles"));
			ThreadManager.RunCoroutineSync(XUi.PatchAndLoadXuiXml("XUi_Common/controls"));
			ThreadManager.RunCoroutineSync(XUi.PatchAndLoadXuiXml("XUi/styles"));
			ThreadManager.RunCoroutineSync(XUi.PatchAndLoadXuiXml("XUi/controls"));
			ThreadManager.RunCoroutineSync(XUi.PatchAndLoadXuiXml("XUi/windows"));
			ThreadManager.RunCoroutineSync(XUi.PatchAndLoadXuiXml("XUi/xui"));
			return;
		}
		ThreadManager.RunCoroutineSync(XUi.PatchAndLoadXuiXml("XUi_Common/styles"));
		ThreadManager.RunCoroutineSync(XUi.PatchAndLoadXuiXml("XUi_Common/controls"));
		ThreadManager.RunCoroutineSync(XUi.PatchAndLoadXuiXml("XUi_Menu/styles"));
		ThreadManager.RunCoroutineSync(XUi.PatchAndLoadXuiXml("XUi_Menu/controls"));
		ThreadManager.RunCoroutineSync(XUi.PatchAndLoadXuiXml("XUi_Menu/windows"));
		ThreadManager.RunCoroutineSync(XUi.PatchAndLoadXuiXml("XUi_Menu/xui"));
	}

	public static IEnumerator PatchAndLoadXuiXml(string _relPathXuiFile)
	{
		MicroStopwatch timer = null;
		bool coroutineHadException = false;
		XmlFile xmlFile = null;
		yield return XmlPatcher.LoadAndPatchConfig(_relPathXuiFile, delegate(XmlFile _file)
		{
			xmlFile = _file;
		});
		yield return XmlPatcher.ApplyConditionalXmlBlocks(_relPathXuiFile, xmlFile, timer, XmlPatcher.EEvaluator.Host, delegate
		{
			coroutineHadException = true;
		});
		if (coroutineHadException)
		{
			yield break;
		}
		yield return XmlPatcher.ApplyConditionalXmlBlocks(_relPathXuiFile, xmlFile, timer, XmlPatcher.EEvaluator.Client, delegate
		{
			coroutineHadException = true;
		});
		if (coroutineHadException)
		{
			yield break;
		}
		yield return XUiFromXml.Load(xmlFile);
		yield break;
	}

	public void SetDataConnections()
	{
		if (this.playerUI.entityPlayer != null)
		{
			this.PlayerInventory = new XUiM_PlayerInventory(this, this.playerUI.entityPlayer);
			this.PlayerEquipment = new XUiM_PlayerEquipment(this, this.playerUI.entityPlayer);
		}
	}

	public float GetPixelRatioFactor()
	{
		if (XUi.pixelRatioFactor == 0f || Screen.height != XUi.lastScreenHeight)
		{
			float @float = GamePrefs.GetFloat(EnumGamePrefs.OptionsScreenBoundsValue);
			float activeUiScale = GameOptionsManager.GetActiveUiScale();
			XUi.pixelRatioFactor = XUi.UIRoot.pixelSizeAdjustment / this.xuiGlobalScaling / @float / activeUiScale;
			XUi.lastScreenHeight = Screen.height;
		}
		return XUi.pixelRatioFactor;
	}

	public Vector2i GetXUiScreenSize()
	{
		return new Vector2i(new Vector2((float)this.playerUI.camera.pixelWidth, (float)this.playerUI.camera.pixelHeight) * this.GetPixelRatioFactor());
	}

	public Vector2i GetMouseXUIPosition()
	{
		Vector3 v = this.playerUI.CursorController.GetLocalScreenPosition();
		return this.TranslateScreenVectorToXuiVector(v);
	}

	public Vector2i TranslateScreenVectorToXuiVector(Vector2 _screenSpaceVector)
	{
		_screenSpaceVector.x -= (float)this.playerUI.camera.pixelWidth / 2f;
		_screenSpaceVector.y -= (float)this.playerUI.camera.pixelHeight / 2f;
		return new Vector2i(_screenSpaceVector * this.GetPixelRatioFactor());
	}

	public Vector3 TranslateScreenVectorToXuiVector(Vector3 _screenSpaceVector)
	{
		_screenSpaceVector.x -= (float)this.playerUI.camera.pixelWidth / 2f;
		_screenSpaceVector.y -= (float)this.playerUI.camera.pixelHeight / 2f;
		return _screenSpaceVector * this.GetPixelRatioFactor();
	}

	public static bool IsGameRunning()
	{
		return GameManager.Instance != null && GameManager.Instance.World != null;
	}

	public LocalPlayerUI playerUI
	{
		get
		{
			if (this.mPlayerUI == null)
			{
				this.mPlayerUI = base.GetComponentInParent<LocalPlayerUI>();
			}
			return this.mPlayerUI;
		}
	}

	public XUiC_GamepadCalloutWindow calloutWindow
	{
		get
		{
			XUiController xuiController = this.FindWindowGroupByName("CalloutGroup");
			if (xuiController != null)
			{
				this.mCalloutWindow = xuiController.GetChildByType<XUiC_GamepadCalloutWindow>();
			}
			return this.mCalloutWindow;
		}
	}

	public XUiC_DragAndDropWindow dragAndDrop { get; set; }

	public XUiC_OnScreenIcons onScreenIcons { get; set; }

	public XUiC_ToolTip currentToolTip { get; set; }

	public XUiC_PopupMenu currentPopupMenu { get; set; }

	public XUiC_SaveIndicator saveIndicator { get; set; }

	public XUiC_BasePartStack basePartStack { get; set; }

	public XUiC_EquipmentStack equipmentStack { get; set; }

	public XUiC_ItemStack itemStack { get; set; }

	public XUiC_RecipeEntry recipeEntry { get; set; }

	public ProgressionValue selectedSkill { get; set; }

	public ITileEntityLootable lootContainer { get; set; }

	public string currentWorkstation { get; set; }

	public EntityVehicle vehicle { get; set; }

	public XUiC_WorkstationToolGrid currentWorkstationToolGrid { get; set; }

	public XUiC_WorkstationFuelGrid currentWorkstationFuelGrid { get; set; }

	public XUiC_WorkstationInputGrid currentWorkstationInputGrid { get; set; }

	public XUiC_DewCollectorModGrid currentDewCollectorModGrid { get; set; }

	public XUiC_CombineGrid currentCombineGrid { get; set; }

	public XUiC_PowerSourceSlots powerSourceSlots { get; set; }

	public XUiC_PowerRangedAmmoSlots powerAmmoSlots { get; set; }

	public XUiC_SelectableEntry currentSelectedEntry { get; set; }

	public bool isUsingItemActionEntryUse { get; set; }

	public bool isUsingItemActionEntryPromptComplete { get; set; }

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDestroy()
	{
		GameOptionsManager.ResolutionChanged -= this.OnResolutionChanged;
		GamePrefs.OnGamePrefChanged -= this.OnGamePrefChanged;
		LocalPlayerManager.OnLocalPlayersChanged -= this.HandleLocalPlayersChanged;
		this.Shutdown(false);
	}

	public void SetScale(float scale = -1f)
	{
		if (scale > 0f)
		{
			this.xuiGlobalScaling = scale;
		}
		float @float = GamePrefs.GetFloat(EnumGamePrefs.OptionsScreenBoundsValue);
		float activeUiScale = GameOptionsManager.GetActiveUiScale();
		base.transform.localScale = Vector3.one * this.xuiGlobalScaling * @float * activeUiScale;
		base.transform.localPosition = Vector3.zero;
		XUi.pixelRatioFactor = 0f;
	}

	public float GetScale()
	{
		return this.xuiGlobalScaling;
	}

	public void SetStackPanelScale(float scale)
	{
		this.defaultStackPanelScale = scale;
		this.stackPanelRoot.localScale = Vector3.one * scale;
		this.stackPanelRoot.localPosition = Vector3.zero;
	}

	public void Awake()
	{
		this.WindowGroups = new List<XUiWindowGroup>();
		this.ControllersByType = new Dictionary<Type, List<XUiController>>();
		this.FontsByName = new CaseInsensitiveStringDictionary<NGUIFont>();
		foreach (NGUIFont nguifont in this.NGUIFonts)
		{
			this.FontsByName.Add(nguifont.name, nguifont);
		}
	}

	public void Load(List<string> windowGroupSubset = null, bool async = false)
	{
		if (async)
		{
			this.asyncLoad = true;
			this.loadAsyncCoroutine = ThreadManager.StartCoroutine(this.LoadAsync(windowGroupSubset));
			return;
		}
		this.asyncLoad = false;
		if (!XUiFromXml.HasData())
		{
			Log.Error("Loading XUi synchronously failed: XMLs not set.");
			return;
		}
		ThreadManager.RunCoroutineSync(this.LoadAsync(windowGroupSubset));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator LoadAsync(List<string> windowGroupSubset = null)
	{
		yield return null;
		while (!XUiFromXml.HasData())
		{
			yield return null;
		}
		MicroStopwatch msw = new MicroStopwatch();
		Log.Out("[XUi] Loading XUi " + (this.asyncLoad ? "asynchronously" : "synchronously"));
		List<string> asyncWindowGroupList = (windowGroupSubset != null) ? new List<string>(windowGroupSubset) : new List<string>();
		if (windowGroupSubset == null)
		{
			XUiFromXml.GetWindowGroupNames(out asyncWindowGroupList);
		}
		if (msw.ElapsedMilliseconds > (long)Constants.cMaxLoadTimePerFrameMillis)
		{
			yield return null;
			msw.ResetAndRestart();
		}
		this.accumElapsedMilliseconds = 0L;
		MicroStopwatch ms = new MicroStopwatch(true);
		foreach (string text in asyncWindowGroupList)
		{
			ms.Reset();
			ms.Start();
			XUiFromXml.LoadXui(this, text);
			this.accumElapsedMilliseconds += ms.ElapsedMilliseconds;
			if (XUiFromXml.DebugXuiLoading == XUiFromXml.DebugLevel.Verbose)
			{
				Log.Out("[XUi] Parsing window group, {0}, completed in {1} ms.", new object[]
				{
					text,
					ms.ElapsedMilliseconds
				});
			}
			if (msw.ElapsedMilliseconds > 20L)
			{
				yield return null;
				msw.ResetAndRestart();
			}
		}
		List<string>.Enumerator enumerator = default(List<string>.Enumerator);
		XUiFromXml.LoadDone(windowGroupSubset == null);
		Log.Out("[XUi] Parsing all window groups completed in {0} ms total.", new object[]
		{
			this.accumElapsedMilliseconds
		});
		if (msw.ElapsedMilliseconds > (long)Constants.cMaxLoadTimePerFrameMillis)
		{
			yield return null;
			msw.ResetAndRestart();
		}
		this.accumElapsedMilliseconds = 0L;
		int num;
		for (int i = 0; i < this.WindowGroups.Count; i = num + 1)
		{
			XUiWindowGroup xuiWindowGroup = this.WindowGroups[i];
			ms.Reset();
			ms.Start();
			try
			{
				xuiWindowGroup.Init();
			}
			catch (Exception e)
			{
				Log.Error("[XUi] Failed initializing window group " + xuiWindowGroup.ID);
				Log.Exception(e);
			}
			this.accumElapsedMilliseconds += ms.ElapsedMilliseconds;
			if (XUiFromXml.DebugXuiLoading == XUiFromXml.DebugLevel.Verbose)
			{
				Log.Out("[XUi] Initialize window group, {0}, completed in {1} ms.", new object[]
				{
					xuiWindowGroup.ID,
					ms.ElapsedMilliseconds
				});
			}
			if (msw.ElapsedMilliseconds > 20L)
			{
				yield return null;
				msw.ResetAndRestart();
			}
			num = i;
		}
		Log.Out("[XUi] Initialized all window groups completed in {0} ms total.", new object[]
		{
			this.accumElapsedMilliseconds
		});
		while (this.loadGroup.Pending)
		{
			yield return null;
		}
		this.PostLoadInit();
		this.isReady = (windowGroupSubset == null);
		this.loadAsyncCoroutine = null;
		XUiUpdater.Add(this);
		yield break;
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PostLoadInit()
	{
		this.RadialWindow = (XUiC_Radial)this.FindWindowGroupByName("radial");
		foreach (XUiWindowGroup xuiWindowGroup in this.WindowGroups)
		{
			if (xuiWindowGroup.Initialized)
			{
				this.AddControllerTypeEntry(xuiWindowGroup.Controller);
				foreach (XUiController controller in xuiWindowGroup.Controller.Children)
				{
					this.AddControllerTypeEntry(controller);
				}
			}
		}
		if (WorldStaticData.LoadAllXmlsCoComplete)
		{
			XUiFromXml.ClearLoadingData();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AddControllerTypeEntry(XUiController _controller)
	{
		Type type = _controller.GetType();
		List<XUiController> list;
		if (!this.ControllersByType.TryGetValue(type, out list))
		{
			list = new List<XUiController>();
			this.ControllersByType.Add(type, list);
		}
		list.Add(_controller);
	}

	public int RegisterXUiView(XUiView _view)
	{
		this.xuiViewList.Add(_view);
		return this.xuiViewList.Count - 1;
	}

	public void LoadData<T>(string _path, Action<T> _callback) where T : UnityEngine.Object
	{
		LoadManager.LoadAssetFromResources<T>(_path, _callback, this.loadGroup, false, !this.asyncLoad);
	}

	public void Init(int _id)
	{
		GamePrefs.OnGamePrefChanged += this.OnGamePrefChanged;
		GameOptionsManager.ResolutionChanged += this.OnResolutionChanged;
		this.loadGroup = LoadManager.CreateGroup();
		this.id = _id;
		this.windows = new List<XUiV_Window>();
		this.lastScreenSize = new Vector2((float)this.playerUI.camera.pixelWidth, (float)this.playerUI.camera.pixelHeight);
		base.gameObject.GetOrAddComponent<XUi_FallThrough>().SetXUi(this);
		this.stackPanelRoot = base.transform.Find("StackPanels").transform;
		this.stackPanels.Add("Left", new XUi.StackPanel("Left", base.transform.Find("StackPanels/Left").transform));
		this.stackPanels.Add("Center", new XUi.StackPanel("Center", base.transform.Find("StackPanels/Center").transform));
		this.stackPanels.Add("Right", new XUi.StackPanel("Right", base.transform.Find("StackPanels/Right").transform));
		MultiSourceAtlasManager[] array = Resources.FindObjectsOfTypeAll<MultiSourceAtlasManager>();
		for (int i = 0; i < array.Length; i++)
		{
			this.allMultiSourceAtlases.Add(array[i].name, array[i]);
		}
		if (Application.isPlaying)
		{
			this.LoadData<AudioClip>("Sounds/UI/ui_menu_cycle", delegate(AudioClip o)
			{
				this.uiScrollSound = o;
			});
			this.LoadData<AudioClip>("Sounds/UI/ui_menu_click", delegate(AudioClip o)
			{
				this.uiClickSound = o;
			});
			this.LoadData<AudioClip>("Sounds/UI/ui_menu_start", delegate(AudioClip o)
			{
				this.uiConfirmSound = o;
			});
			this.LoadData<AudioClip>("Sounds/UI/ui_menu_back", delegate(AudioClip o)
			{
				this.uiBackSound = o;
			});
			this.LoadData<AudioClip>("Sounds/UI/ui_hover", delegate(AudioClip o)
			{
				this.uiSliderSound = o;
			});
		}
		this.anchors = base.transform.parent.GetComponentsInChildren<UIAnchor>();
		this.xuiAnchors = new UIAnchor[9];
		foreach (UIAnchor uianchor in this.anchors)
		{
			uianchor.runOnlyOnce = false;
			uianchor.uiCamera = this.playerUI.camera;
			if (uianchor.transform.parent.GetComponent<XUi>() == this)
			{
				this.xuiAnchors[(int)uianchor.side] = uianchor;
			}
		}
		this.UpdateAnchors();
		this.BackgroundGlobalOpacity = GamePrefs.GetFloat(EnumGamePrefs.OptionsBackgroundGlobalOpacity);
		this.ForegroundGlobalOpacity = GamePrefs.GetFloat(EnumGamePrefs.OptionsForegroundGlobalOpacity);
		LocalPlayerManager.OnLocalPlayersChanged += this.HandleLocalPlayersChanged;
		this.Vehicle = new XUiM_Vehicle();
		this.AssembleItem = new XUiM_AssembleItem();
		this.QuestTracker = new XUiM_Quest();
		this.Recipes = new XUiM_Recipes();
		this.Trader = new XUiM_Trader();
		this.Dialog = new XUiM_Dialog();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleLocalPlayersChanged()
	{
		this.lastScreenSize = new Vector2((float)this.playerUI.camera.pixelWidth, (float)this.playerUI.camera.pixelHeight);
		this.UpdateAnchors();
		this.RecenterWindowGroup(null, true);
		for (int i = 0; i < this.windows.Count; i++)
		{
			XUiV_Window xuiV_Window = this.windows[i];
			if (xuiV_Window.IsCursorArea && xuiV_Window.IsOpen)
			{
				this.UpdateWindowSoftCursorBounds(xuiV_Window);
			}
		}
	}

	public void LateInit()
	{
		this.RadialWindow = (XUiC_Radial)this.FindWindowGroupByName("radial");
		XUiM_PlayerBuffs.HasLocalizationBeenCached = false;
		XUiM_Vehicle.HasLocalizationBeenCached = false;
	}

	public void Cleanup(bool _destroyImmediate = false)
	{
		this.CancelLoading();
		for (int i = 0; i < this.WindowGroups.Count; i++)
		{
			XUiWindowGroup xuiWindowGroup = this.WindowGroups[i];
			xuiWindowGroup.Controller.Cleanup();
			this.playerUI.windowManager.Remove(xuiWindowGroup.ID);
		}
		this.WindowGroups.Clear();
		XUiUpdater.Remove(this);
		if (_destroyImmediate)
		{
			UnityEngine.Object.DestroyImmediate(base.gameObject);
		}
	}

	public void CancelLoading()
	{
		if (this.loadAsyncCoroutine != null)
		{
			ThreadManager.StopCoroutine(this.loadAsyncCoroutine);
			this.loadAsyncCoroutine = null;
		}
	}

	public void OnUpdateInput()
	{
		if (this.WindowGroups == null)
		{
			return;
		}
		for (int i = 0; i < this.WindowGroups.Count; i++)
		{
			if (!this.WindowGroups[i].Controller.IsDormant && (this.WindowGroups[i].isShowing || this.WindowGroups[i].Controller.AlwaysUpdate()))
			{
				try
				{
					this.WindowGroups[i].Controller.UpdateInput();
				}
				catch (Exception e)
				{
					Log.Error("[XUi] Error while handling input for window group '" + this.WindowGroups[i].ID + "':");
					Log.Exception(e);
				}
			}
		}
	}

	public void OnUpdateDeltaTime(float updateDeltaTime)
	{
		if (this.playerUI.entityPlayer != null)
		{
			if (this.PlayerInventory == null)
			{
				this.PlayerInventory = new XUiM_PlayerInventory(this, this.playerUI.entityPlayer);
			}
			if (this.PlayerEquipment == null)
			{
				this.PlayerEquipment = new XUiM_PlayerEquipment(this, this.playerUI.entityPlayer);
			}
		}
		if (this.WindowGroups == null)
		{
			return;
		}
		if (this.currentToolTip != null)
		{
			this.playerUI.windowManager.OpenIfNotOpen(this.currentToolTip.ID, false, false, true);
		}
		if (this.saveIndicator != null)
		{
			this.playerUI.windowManager.OpenIfNotOpen(this.saveIndicator.ID, false, false, true);
		}
		if (this.lastScreenSize.x != (float)this.playerUI.camera.pixelWidth || this.lastScreenSize.y != (float)this.playerUI.camera.pixelHeight)
		{
			this.lastScreenSize = new Vector2((float)this.playerUI.camera.pixelWidth, (float)this.playerUI.camera.pixelHeight);
			this.UpdateAnchors();
			this.RecenterWindowGroup(null, true);
		}
		for (int i = 0; i < this.WindowGroups.Count; i++)
		{
			XUiWindowGroup xuiWindowGroup = this.WindowGroups[i];
			if (xuiWindowGroup.Initialized && !xuiWindowGroup.Controller.IsDormant && (xuiWindowGroup.isShowing || xuiWindowGroup.Controller.AlwaysUpdate()))
			{
				try
				{
					xuiWindowGroup.Controller.Update(updateDeltaTime);
				}
				catch (Exception e)
				{
					Log.Error("[XUi] Error while updating window group '" + xuiWindowGroup.ID + "':");
					Log.Exception(e);
				}
			}
		}
		this.oldBackgroundGlobalOpacity = this.BackgroundGlobalOpacity;
		this.oldForegroundGlobalOpacity = this.ForegroundGlobalOpacity;
	}

	public void RecenterWindowGroup(XUiWindowGroup _wg, bool _forceImmediate = false)
	{
		if (!_forceImmediate && GameStats.GetInt(EnumGameStats.GameState) != 2)
		{
			if (base.gameObject.activeInHierarchy)
			{
				base.StartCoroutine(this.recenterLater(_wg));
			}
			return;
		}
		if (_wg == null)
		{
			this.CalculateWindowGroupLayouts();
			return;
		}
		this.CalculateWindowGroupLayout(_wg);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator recenterLater(XUiWindowGroup _wg)
	{
		yield return null;
		if (_wg != null)
		{
			this.CalculateWindowGroupLayout(_wg);
		}
		else
		{
			this.CalculateWindowGroupLayouts();
		}
		yield return null;
		this.playerUI.CursorController.ResetNavigationTarget();
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CalculateWindowGroupLayouts()
	{
		foreach (XUiWindowGroup xuiWindowGroup in this.WindowGroups)
		{
			if (xuiWindowGroup.isShowing)
			{
				this.CalculateWindowGroupLayout(xuiWindowGroup);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CalculateWindowGroupLayout(XUiWindowGroup _wg)
	{
		if (_wg == null || _wg.ID == "backpack")
		{
			return;
		}
		XUiController controller = _wg.Controller;
		if (((controller != null) ? controller.Children : null) == null)
		{
			return;
		}
		if (!_wg.HasStackPanelWindows())
		{
			return;
		}
		bool flag = false;
		int num = 0;
		int num2 = 0;
		foreach (XUi.StackPanel stackPanel in this.stackPanels.list)
		{
			bool flag2 = this.LayoutWindowsInPanel(_wg, stackPanel);
			if (flag2 && num > 0)
			{
				num += _wg.StackPanelPadding;
			}
			stackPanel.Transform.localPosition = new Vector3((float)num, 0f, 0f);
			num += stackPanel.Size.x;
			num2 = Math.Max(num2, stackPanel.Size.y);
			flag = (flag || flag2);
		}
		if (flag)
		{
			this.stackPanelRoot.localPosition = new Vector3(-(this.defaultStackPanelScale * (float)num / 2f), (float)_wg.StackPanelYOffset, 0f);
			this.stackPanelRoot.localScale = Vector3.one * this.defaultStackPanelScale;
		}
		if (flag && (!_wg.LeftPanelVAlignTop || !_wg.RightPanelVAlignTop))
		{
			if (!_wg.LeftPanelVAlignTop)
			{
				this.stackPanels.list[0].Transform.localPosition = new Vector3((float)((int)this.stackPanels.list[0].Transform.position.x), (float)(-(float)(num2 - this.stackPanels.list[0].Size.y)), 0f);
			}
			if (!_wg.RightPanelVAlignTop)
			{
				this.stackPanels.list[2].Transform.localPosition = new Vector3((float)((int)this.stackPanels.list[2].Transform.position.x), (float)(-(float)(num2 - this.stackPanels.list[2].Size.y)), 0f);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool LayoutWindowsInPanel(XUiWindowGroup _wg, XUi.StackPanel _panel)
	{
		XUi.<>c__DisplayClass232_0 CS$<>8__locals1;
		CS$<>8__locals1._panel = _panel;
		CS$<>8__locals1.windowCount = 0;
		CS$<>8__locals1.yPos = 0;
		CS$<>8__locals1.maxWidth = 0;
		XUi.<LayoutWindowsInPanel>g__LayoutWindowGroupWindows|232_0(_wg, ref CS$<>8__locals1);
		if (this.playerUI.windowManager.IsWindowOpen("backpack"))
		{
			XUi.<LayoutWindowsInPanel>g__LayoutWindowGroupWindows|232_0(this.GetWindowGroupById("backpack"), ref CS$<>8__locals1);
		}
		CS$<>8__locals1._panel.Size.x = CS$<>8__locals1.maxWidth;
		CS$<>8__locals1._panel.Size.y = -CS$<>8__locals1.yPos;
		CS$<>8__locals1._panel.WindowCount = CS$<>8__locals1.windowCount;
		return CS$<>8__locals1.windowCount != 0;
	}

	public Bounds GetXUIWindowWorldBounds(Transform _xuiElement, bool _includeInactive = false)
	{
		Bounds result = new Bounds(Vector3.zero, Vector3.zero);
		bool flag = false;
		_xuiElement.GetComponentsInChildren<UIBasicSprite>(_includeInactive, XUi.getXUIWindowWorldBoundsList);
		List<UIBasicSprite> list = XUi.getXUIWindowWorldBoundsList;
		for (int i = 0; i < list.Count; i++)
		{
			UIBasicSprite uibasicSprite = list[i];
			Transform parent = uibasicSprite.transform.parent;
			if (!(parent != null) || !parent.name.Equals("MapSpriteEntity(Clone)"))
			{
				Vector3[] worldCorners = uibasicSprite.worldCorners;
				for (int j = 0; j < worldCorners.Length; j++)
				{
					if (!flag)
					{
						result = new Bounds(worldCorners[j], Vector3.zero);
						flag = true;
					}
					else
					{
						result.Encapsulate(worldCorners[j]);
					}
				}
			}
		}
		XUi.getXUIWindowWorldBoundsList.Clear();
		return result;
	}

	public Bounds GetXUIWindowScreenBounds(Transform _xuiElement, bool _includeInactive = false)
	{
		Bounds xuiwindowWorldBounds = this.GetXUIWindowWorldBounds(_xuiElement, _includeInactive);
		Bounds result = new Bounds(this.playerUI.camera.WorldToScreenPoint(xuiwindowWorldBounds.min), Vector3.zero);
		result.Encapsulate(this.playerUI.camera.WorldToScreenPoint(xuiwindowWorldBounds.max));
		return result;
	}

	public Bounds GetXUIWindowViewportBounds(Transform _xuiElement, bool _includeInactive = false)
	{
		Bounds xuiwindowWorldBounds = this.GetXUIWindowWorldBounds(_xuiElement, _includeInactive);
		Bounds result = new Bounds(this.playerUI.camera.WorldToViewportPoint(xuiwindowWorldBounds.min), Vector3.zero);
		result.Encapsulate(this.playerUI.camera.WorldToViewportPoint(xuiwindowWorldBounds.max));
		return result;
	}

	public Bounds GetXUIWindowPixelBounds(Transform _xuiElement, bool _includeInactive = false)
	{
		Bounds xuiwindowViewportBounds = this.GetXUIWindowViewportBounds(_xuiElement, _includeInactive);
		Vector3 center = Vector3.Scale(xuiwindowViewportBounds.min, new Vector3((float)this.playerUI.camera.pixelWidth, (float)this.playerUI.camera.pixelHeight, 1f));
		Vector3 point = Vector3.Scale(xuiwindowViewportBounds.max, new Vector3((float)this.playerUI.camera.pixelWidth, (float)this.playerUI.camera.pixelHeight, 1f));
		Bounds result = new Bounds(center, Vector3.zero);
		result.Encapsulate(point);
		return result;
	}

	public void RefreshAllWindows(bool _includeViewComponents = false)
	{
		if (this.WindowGroups == null)
		{
			return;
		}
		for (int i = 0; i < this.WindowGroups.Count; i++)
		{
			this.WindowGroups[i].Controller.SetAllChildrenDirty(_includeViewComponents);
		}
	}

	public void PlayMenuSound(XUi.UISoundType _soundType)
	{
		switch (_soundType)
		{
		case XUi.UISoundType.ClickSound:
			this.PlayMenuClickSound();
			return;
		case XUi.UISoundType.ScrollSound:
			this.PlayMenuScrollSound();
			return;
		case XUi.UISoundType.ConfirmSound:
			this.PlayMenuConfirmSound();
			return;
		case XUi.UISoundType.BackSound:
			this.PlayMenuBackSound();
			return;
		case XUi.UISoundType.SliderSound:
			this.PlayMenuSliderSound();
			return;
		case XUi.UISoundType.None:
			return;
		default:
			return;
		}
	}

	public void PlayMenuScrollSound()
	{
		Manager.PlayXUiSound(this.uiScrollSound, this.uiScrollVolume);
	}

	public void PlayMenuClickSound()
	{
		Manager.PlayXUiSound(this.uiClickSound, this.uiClickVolume);
	}

	public void PlayMenuConfirmSound()
	{
		Manager.PlayXUiSound(this.uiConfirmSound, this.uiConfirmVolume);
	}

	public void PlayMenuBackSound()
	{
		Manager.PlayXUiSound(this.uiBackSound, this.uiBackVolume);
	}

	public void PlayMenuSliderSound()
	{
		Manager.PlayXUiSound(this.uiSliderSound, this.uiSliderVolume);
	}

	public UIAtlas GetAtlasByName(string _atlasName, string _spriteName)
	{
		if (string.IsNullOrEmpty(_atlasName))
		{
			return null;
		}
		MultiSourceAtlasManager multiSourceAtlasManager;
		if (!string.IsNullOrEmpty(_spriteName) && this.allMultiSourceAtlases.TryGetValue(_atlasName, out multiSourceAtlasManager))
		{
			return multiSourceAtlasManager.GetAtlasForSprite(_spriteName);
		}
		UIAtlas result;
		if (this.allAtlases.TryGetValue(_atlasName, out result))
		{
			return result;
		}
		return null;
	}

	public NGUIFont GetUIFontByName(string _name, bool _showWarning = true)
	{
		NGUIFont result;
		if (this.FontsByName.TryGetValue(_name, out result))
		{
			return result;
		}
		if (_showWarning)
		{
			Log.Warning("XUi font not found: " + _name + ", from: " + StackTraceUtility.ExtractStackTrace());
		}
		return null;
	}

	public void AddWindow(XUiV_Window _window)
	{
		this.windows.Add(_window);
	}

	public XUiV_Window GetWindow(string _name)
	{
		for (int i = 0; i < this.windows.Count; i++)
		{
			if (this.windows[i].ID == _name)
			{
				return this.windows[i];
			}
		}
		return null;
	}

	public XUiController FindWindowGroupByName(string _name)
	{
		for (int i = 0; i < this.WindowGroups.Count; i++)
		{
			if (this.WindowGroups[i].ID.EqualsCaseInsensitive(_name))
			{
				return this.WindowGroups[i].Controller;
			}
		}
		return null;
	}

	public XUiController GetChildById(string _id)
	{
		XUiController xuiController = null;
		for (int i = 0; i < this.WindowGroups.Count; i++)
		{
			xuiController = this.WindowGroups[i].Controller.GetChildById(_id);
			if (xuiController != null)
			{
				return xuiController;
			}
		}
		return xuiController;
	}

	public List<XUiController> GetChildrenById(string _id)
	{
		List<XUiController> list = new List<XUiController>();
		for (int i = 0; i < this.WindowGroups.Count; i++)
		{
			this.WindowGroups[i].Controller.GetChildrenById(_id, list);
		}
		return list;
	}

	public T GetChildByType<T>() where T : XUiController
	{
		for (int i = 0; i < this.WindowGroups.Count; i++)
		{
			T childByType = this.WindowGroups[i].Controller.GetChildByType<T>();
			if (childByType != null)
			{
				return childByType;
			}
		}
		return default(T);
	}

	public List<T> GetChildrenByType<T>() where T : XUiController
	{
		List<T> list = new List<T>();
		for (int i = 0; i < this.WindowGroups.Count; i++)
		{
			this.WindowGroups[i].Controller.GetChildrenByType<T>(list);
		}
		return list;
	}

	public T GetWindowByType<T>() where T : XUiController
	{
		Type typeFromHandle = typeof(T);
		List<XUiController> list;
		this.ControllersByType.TryGetValue(typeFromHandle, out list);
		if (list == null || list.Count == 0)
		{
			return default(T);
		}
		if (list.Count > 1)
		{
			Log.Warning("Multiple controllers of type " + typeof(T).FullName);
		}
		return (T)((object)list[0]);
	}

	public List<T> GetWindowsByType<T>() where T : XUiController
	{
		Type typeFromHandle = typeof(T);
		List<XUiController> list;
		this.ControllersByType.TryGetValue(typeFromHandle, out list);
		List<T> list2 = new List<T>();
		if (list != null)
		{
			foreach (XUiController xuiController in list)
			{
				list2.Add((T)((object)xuiController));
			}
		}
		return list2;
	}

	public XUiWindowGroup GetWindowGroupById(string _id)
	{
		foreach (XUiWindowGroup xuiWindowGroup in this.WindowGroups)
		{
			if (xuiWindowGroup.ID == _id)
			{
				return xuiWindowGroup;
			}
		}
		return null;
	}

	public static string UppercaseFirst(string s)
	{
		if (string.IsNullOrEmpty(s))
		{
			return string.Empty;
		}
		return char.ToUpper(s[0]).ToString() + s.Substring(1);
	}

	public void CancelAllCrafting()
	{
		XUiC_RecipeStack[] recipesToCraft = this.FindWindowGroupByName("crafting").GetChildByType<XUiC_CraftingQueue>().GetRecipesToCraft();
		for (int i = 0; i < recipesToCraft.Length; i++)
		{
			recipesToCraft[i].ForceCancel();
		}
	}

	public CraftingData GetCraftingData()
	{
		CraftingData craftingData = new CraftingData();
		XUiController xuiController = this.FindWindowGroupByName("crafting");
		if (xuiController == null)
		{
			return craftingData;
		}
		XUiC_CraftingQueue childByType = xuiController.GetChildByType<XUiC_CraftingQueue>();
		if (childByType == null)
		{
			return craftingData;
		}
		XUiC_RecipeStack[] recipesToCraft = childByType.GetRecipesToCraft();
		if (recipesToCraft == null)
		{
			return craftingData;
		}
		new RecipeQueueItem[recipesToCraft.Length];
		craftingData.RecipeQueueItems = new RecipeQueueItem[recipesToCraft.Length];
		for (int i = 0; i < recipesToCraft.Length; i++)
		{
			RecipeQueueItem recipeQueueItem = new RecipeQueueItem();
			recipeQueueItem.Recipe = recipesToCraft[i].GetRecipe();
			recipeQueueItem.Multiplier = (short)recipesToCraft[i].GetRecipeCount();
			recipeQueueItem.CraftingTimeLeft = recipesToCraft[i].GetRecipeCraftingTimeLeft();
			recipeQueueItem.IsCrafting = recipesToCraft[i].IsCrafting;
			recipeQueueItem.Quality = (byte)recipesToCraft[i].OutputQuality;
			recipeQueueItem.StartingEntityId = recipesToCraft[i].StartingEntityId;
			recipeQueueItem.RepairItem = recipesToCraft[i].OriginalItem;
			recipeQueueItem.AmountToRepair = (ushort)recipesToCraft[i].AmountToRepair;
			recipeQueueItem.OneItemCraftTime = recipesToCraft[i].GetOneItemCraftTime();
			craftingData.RecipeQueueItems[i] = recipeQueueItem;
		}
		return craftingData;
	}

	public void SetCraftingData(CraftingData _cd)
	{
		base.StartCoroutine(this.SetCraftingDataAsync(_cd));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator SetCraftingDataAsync(CraftingData _cd)
	{
		while (!this.isReady)
		{
			yield return null;
		}
		XUiC_CraftingQueue childByType = this.GetChildByType<XUiC_CraftingQueue>();
		if (childByType != null)
		{
			childByType.ClearQueue();
			for (int i = 0; i < _cd.RecipeQueueItems.Length; i++)
			{
				RecipeQueueItem recipeQueueItem = _cd.RecipeQueueItems[i];
				if (recipeQueueItem != null)
				{
					if (recipeQueueItem.RepairItem != null && recipeQueueItem.RepairItem.type != 0)
					{
						childByType.AddItemToRepairAtIndex(i, recipeQueueItem.CraftingTimeLeft, recipeQueueItem.RepairItem, (int)recipeQueueItem.AmountToRepair, recipeQueueItem.IsCrafting, recipeQueueItem.StartingEntityId);
					}
					else
					{
						childByType.AddRecipeToCraftAtIndex(i, recipeQueueItem.Recipe, (int)recipeQueueItem.Multiplier, recipeQueueItem.CraftingTimeLeft, recipeQueueItem.IsCrafting, false, (int)recipeQueueItem.Quality, recipeQueueItem.StartingEntityId, recipeQueueItem.OneItemCraftTime);
					}
				}
				else
				{
					childByType.AddRecipeToCraftAtIndex(i, null, 0, -1f, false, false, -1, -1, -1f);
				}
			}
			childByType.IsDirty = true;
		}
		yield break;
	}

	public UIAnchor GetAnchor(UIAnchor.Side _anchorSide)
	{
		return this.xuiAnchors[(int)_anchorSide];
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateAnchors()
	{
		float @float = GamePrefs.GetFloat(EnumGamePrefs.OptionsScreenBoundsValue);
		float num = (1f - @float) / 2f;
		foreach (UIAnchor uianchor in this.anchors)
		{
			if (uianchor != null && !(uianchor.name == "AnchorCenterCenter"))
			{
				if (uianchor.side == UIAnchor.Side.Right || uianchor.side == UIAnchor.Side.TopRight || uianchor.side == UIAnchor.Side.BottomRight)
				{
					uianchor.relativeOffset.x = -num;
				}
				if (uianchor.side == UIAnchor.Side.Left || uianchor.side == UIAnchor.Side.TopLeft || uianchor.side == UIAnchor.Side.BottomLeft)
				{
					uianchor.relativeOffset.x = num;
				}
				if (uianchor.side == UIAnchor.Side.Top || uianchor.side == UIAnchor.Side.TopRight || uianchor.side == UIAnchor.Side.TopLeft)
				{
					uianchor.relativeOffset.y = -num;
				}
				if (uianchor.side == UIAnchor.Side.Bottom || uianchor.side == UIAnchor.Side.BottomRight || uianchor.side == UIAnchor.Side.BottomLeft)
				{
					uianchor.relativeOffset.y = num;
				}
				if (uianchor.side == UIAnchor.Side.Bottom || uianchor.side == UIAnchor.Side.Top || uianchor.side == UIAnchor.Side.Center)
				{
					uianchor.relativeOffset.x = 0f;
				}
			}
		}
	}

	public static void HandlePaging(XUi _xui, Action _onPageUp, Action _onPageDown, bool useVerticalAxis = false)
	{
		if (!(null != _xui.playerUI) || _xui.playerUI.playerInput == null || _xui.playerUI.playerInput.GUIActions == null || !_xui.playerUI.windowManager.IsKeyShortcutsAllowed())
		{
			XUi.previousPagingVector = Vector2.zero;
			return;
		}
		Vector2 vector = _xui.playerUI.playerInput.GUIActions.Camera.Vector;
		if (vector == Vector2.zero)
		{
			XUi.pagingRepeatTimer = 0f;
			XUi.previousPagingVector = Vector2.zero;
			return;
		}
		if (XUi.previousPagingVector != Vector2.zero)
		{
			XUi.pagingRepeatTimer -= Time.deltaTime;
			if (XUi.pagingRepeatTimer > 0f)
			{
				return;
			}
		}
		else
		{
			XUi.initialPagingInput = true;
		}
		XUi.previousPagingVector = vector;
		if (useVerticalAxis)
		{
			if (vector.y > 0f)
			{
				_onPageUp();
			}
			else if (vector.y < 0f)
			{
				_onPageDown();
			}
		}
		else if (vector.x > 0f)
		{
			_onPageUp();
		}
		else if (vector.x < 0f)
		{
			_onPageDown();
		}
		XUi.pagingRepeatTimer = (XUi.initialPagingInput ? 0.35f : 0.1f);
		XUi.initialPagingInput = false;
	}

	public void UpdateWindowSoftCursorBounds(XUiV_Window _window)
	{
		this.playerUI.CursorController != null;
	}

	public void RemoveWindowFromSoftCursorBounds(XUiV_Window _window)
	{
		CursorControllerAbs cursorController = this.playerUI.CursorController;
		if (cursorController != null)
		{
			cursorController.RemoveBounds(_window.ID);
		}
	}

	public void GetOpenWindows(List<XUiV_Window> list)
	{
		list.Clear();
		foreach (XUiV_Window xuiV_Window in this.windows)
		{
			if (xuiV_Window.IsOpen && xuiV_Window.IsVisible)
			{
				list.Add(xuiV_Window);
			}
		}
	}

	public void ForceInputStyleChange()
	{
		List<XUiV_Window> list = new List<XUiV_Window>();
		this.GetOpenWindows(list);
		foreach (XUiV_Window xuiV_Window in list)
		{
			xuiV_Window.Controller.ForceInputStyleChange(PlatformManager.NativePlatform.Input.CurrentInputStyle, PlatformManager.NativePlatform.Input.CurrentInputStyle);
		}
	}

	public static bool IsMatchingPlatform(string platformStr)
	{
		bool result = true;
		string[] array = platformStr.Split(",", StringSplitOptions.None);
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = array[i].Trim().ToUpper();
			if (!array[i].StartsWith("!"))
			{
				result = false;
			}
		}
		for (int j = 0; j < array.Length; j++)
		{
			if (Submission.Enabled)
			{
				if (array[j] == "SUBMISSION")
				{
					return true;
				}
				if (array[j] == "!SUBMISSION")
				{
					return false;
				}
			}
			if (DeviceFlag.StandaloneWindows.IsCurrent())
			{
				if (array[j] == "WINDOWS")
				{
					return true;
				}
				if (array[j] == "!WINDOWS")
				{
					return false;
				}
			}
			if (DeviceFlag.StandaloneLinux.IsCurrent())
			{
				if (array[j] == "LINUX")
				{
					return true;
				}
				if (array[j] == "!LINUX")
				{
					return false;
				}
			}
			if (DeviceFlag.StandaloneOSX.IsCurrent())
			{
				if (array[j] == "OSX")
				{
					return true;
				}
				if (array[j] == "!OSX")
				{
					return false;
				}
			}
			if ((DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX).IsCurrent())
			{
				if (array[j] == "STANDALONE")
				{
					return true;
				}
				if (array[j] == "!STANDALONE")
				{
					return false;
				}
			}
			if (DeviceFlag.PS5.IsCurrent())
			{
				if (array[j] == "PS5")
				{
					return true;
				}
				if (array[j] == "!PS5")
				{
					return false;
				}
			}
			if (DeviceFlag.XBoxSeriesS.IsCurrent())
			{
				if (array[j] == "XBOX_S")
				{
					return true;
				}
				if (array[j] == "!XBOX_S")
				{
					return false;
				}
			}
			if (DeviceFlag.XBoxSeriesX.IsCurrent())
			{
				if (array[j] == "XBOX_X")
				{
					return true;
				}
				if (array[j] == "!XBOX_X")
				{
					return false;
				}
			}
			if ((DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX).IsCurrent())
			{
				if (array[j] == "XBOX")
				{
					return true;
				}
				if (array[j] == "!XBOX")
				{
					return false;
				}
			}
			if ((DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent())
			{
				if (array[j] == "CONSOLE")
				{
					return true;
				}
				if (array[j] == "!CONSOLE")
				{
					return false;
				}
			}
		}
		return result;
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Internal)]
	public static void <LayoutWindowsInPanel>g__LayoutWindowGroupWindows|232_0(XUiWindowGroup _wg, ref XUi.<>c__DisplayClass232_0 A_1)
	{
		foreach (XUiController xuiController in _wg.Controller.Children)
		{
			XUiV_Window xuiV_Window = (XUiV_Window)xuiController.ViewComponent;
			if (xuiV_Window != null && xuiV_Window.UiTransform.gameObject.activeInHierarchy && !(xuiController.ViewComponent.UiTransform.parent.name != A_1._panel.Name) && xuiV_Window.Size.y > 0)
			{
				if (A_1.yPos < 0)
				{
					A_1.yPos -= _wg.StackPanelPadding;
				}
				xuiV_Window.Position = new Vector2i(0, A_1.yPos);
				xuiV_Window.UiTransform.localPosition = new Vector3(0f, (float)A_1.yPos);
				A_1.yPos -= xuiV_Window.Size.y;
				A_1.maxWidth = Math.Max(A_1.maxWidth, xuiV_Window.Size.x);
				int windowCount = A_1.windowCount;
				A_1.windowCount = windowCount + 1;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float xuiGlobalScaling = 1.255f;

	public static string RootNode = "NGUI Camera";

	public static int ID = -1;

	public static string BlankTexture = "menu_empty";

	public static Transform XUiRootTransform;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static int lastScreenHeight;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static float pixelRatioFactor;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static UIRoot UIRoot;

	[NonSerialized]
	public float uiScrollVolume = 0.5f;

	[NonSerialized]
	public float uiClickVolume = 0.5f;

	[NonSerialized]
	public float uiConfirmVolume = 0.5f;

	[NonSerialized]
	public float uiBackVolume = 0.5f;

	[NonSerialized]
	public float uiSliderVolume = 0.25f;

	public int id;

	public NGUIFont[] NGUIFonts;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public CaseInsensitiveStringDictionary<NGUIFont> FontsByName;

	public List<XUiWindowGroup> WindowGroups;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Dictionary<Type, List<XUiController>> ControllersByType;

	public Vector2 lastScreenSize = Vector2.zero;

	public float BackgroundGlobalOpacity = 1f;

	public float ForegroundGlobalOpacity = 1f;

	public string Ruleset = "default";

	public XUiM_PlayerInventory PlayerInventory;

	public XUiM_PlayerEquipment PlayerEquipment;

	public XUiM_Vehicle Vehicle;

	public XUiM_AssembleItem AssembleItem;

	public XUiM_Quest QuestTracker;

	public XUiM_Recipes Recipes;

	public XUiM_Trader Trader;

	public XUiM_Dialog Dialog;

	public XUiC_BuffPopoutList BuffPopoutList;

	public XUiC_CollectedItemList CollectedItemList;

	public XUiC_Radial RadialWindow;

	public bool IgnoreMissingClass;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool mIsReady;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool asyncLoad;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public LoadManager.LoadGroup loadGroup;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<XUiV_Window> windows;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public UIAnchor[] anchors;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public UIAnchor[] xuiAnchors;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float oldBackgroundGlobalOpacity;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float oldForegroundGlobalOpacity;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int repositionFrames;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public XUiWindowGroup currentlyOpeningWindowGroup;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float defaultStackPanelScale = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Transform stackPanelRoot;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public readonly DictionaryList<string, XUi.StackPanel> stackPanels = new DictionaryList<string, XUi.StackPanel>();

	public static MicroStopwatch Stopwatch;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Dictionary<string, UIAtlas> allAtlases = new CaseInsensitiveStringDictionary<UIAtlas>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Dictionary<string, MultiSourceAtlasManager> allMultiSourceAtlases = new CaseInsensitiveStringDictionary<MultiSourceAtlasManager>();

	public static GameObject defaultPrefab = null;

	public static GameObject fullPrefab = null;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public LocalPlayerUI mPlayerUI;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public XUiC_GamepadCalloutWindow mCalloutWindow;

	public static bool InGameMenuOpen = false;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public long accumElapsedMilliseconds;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Coroutine loadAsyncCoroutine;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<XUiView> xuiViewList = new List<XUiView>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static List<UIBasicSprite> getXUIWindowWorldBoundsList = new List<UIBasicSprite>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Vector2 previousPagingVector = Vector2.zero;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static float pagingRepeatTimer;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static bool initialPagingInput = false;

	public enum UISoundType
	{
		ClickSound,
		ScrollSound,
		ConfirmSound,
		BackSound,
		SliderSound,
		None
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class StackPanel
	{
		public StackPanel(string _name, Transform _transform)
		{
			this.Name = _name;
			this.Transform = _transform;
		}

		public readonly string Name;

		public readonly Transform Transform;

		public int WindowCount;

		public Vector2Int Size;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public enum Anchor
	{
		LeftTop,
		LeftCenter,
		LeftBottom,
		CenterTop,
		CenterCenter,
		CenterBottom,
		RightTop,
		RightCenter,
		RightBottom,
		Count
	}

	public enum Alignment
	{
		TopLeft,
		CenterLeft,
		BottomLeft,
		TopCenter,
		CenterCenter,
		BottomCenter,
		TopRight,
		CenterRight,
		BottomRight,
		Count
	}
}

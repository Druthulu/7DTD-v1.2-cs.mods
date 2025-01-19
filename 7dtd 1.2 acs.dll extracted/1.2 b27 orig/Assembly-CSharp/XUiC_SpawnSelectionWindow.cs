using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SpawnSelectionWindow : XUiController
{
	public bool ShowButtons
	{
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (this.showButtons != value)
			{
				this.showButtons = value;
				this.IsDirty = true;
			}
		}
	}

	public string ProgressText
	{
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			this.lblProgress.Text = value;
		}
	}

	public override bool GetBindingValue(ref string _value, string _bindingName)
	{
		if (_bindingName == "buttonsVisible")
		{
			_value = this.showButtons.ToString();
			return true;
		}
		if (!(_bindingName == "progressVisible"))
		{
			return base.GetBindingValue(ref _value, _bindingName);
		}
		_value = (!this.showButtons).ToString();
		return true;
	}

	public override void Init()
	{
		base.Init();
		XUiC_SpawnSelectionWindow.ID = base.WindowGroup.ID;
		this.lblProgress = (XUiV_Label)base.GetChildById("lblProgress").ViewComponent;
		this.ellipsisAnimator = new TextEllipsisAnimator(null, this.lblProgress);
		this.btnOption1 = (XUiC_SimpleButton)base.GetChildById("btnOption1");
		this.btnOption2 = (XUiC_SimpleButton)base.GetChildById("btnOption2");
		this.btnOption3 = (XUiC_SimpleButton)base.GetChildById("btnOption3");
		this.btnOption1.OnPressed += delegate(XUiController _sender, int _args)
		{
			this.SpawnButtonPressed(this.option1Method, this.option1Position);
		};
		this.btnOption2.OnPressed += delegate(XUiController _sender, int _args)
		{
			this.SpawnButtonPressed(this.option2Method, this.option2Position);
		};
		this.btnOption3.OnPressed += delegate(XUiController _sender, int _args)
		{
			this.SpawnButtonPressed(this.option3Method, this.option3Position);
		};
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshButtons()
	{
		if (!this.showButtons && !this.bEnteringGame)
		{
			this.btnOption1.ViewComponent.UiTransform.gameObject.SetActive(false);
			this.btnOption2.ViewComponent.UiTransform.gameObject.SetActive(false);
			this.btnOption3.ViewComponent.UiTransform.gameObject.SetActive(false);
			return;
		}
		if (this.bEnteringGame)
		{
			this.btnOption1.ViewComponent.UiTransform.gameObject.SetActive(true);
			this.btnOption1.Text = Localization.Get("lblSpawn", false);
			this.option1Method = SpawnMethod.Invalid;
			this.btnOption2.ViewComponent.UiTransform.gameObject.SetActive(false);
			this.btnOption3.ViewComponent.UiTransform.gameObject.SetActive(false);
			return;
		}
		EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		if (primaryPlayer == null)
		{
			Log.Warning("Refresh buttons cannot process without an EntityPlayerLocal");
			return;
		}
		SpawnPosition spawnPoint = primaryPlayer.GetSpawnPoint();
		bool flag = !primaryPlayer.GetSpawnPoint().IsUndef();
		Vector3i lastDroppedBackpackPosition = primaryPlayer.GetLastDroppedBackpackPosition();
		bool flag2 = lastDroppedBackpackPosition != Vector3i.zero;
		SpawnPosition spawnPosition = flag2 ? new SpawnPosition(lastDroppedBackpackPosition, 0f) : SpawnPosition.Undef;
		this.option1Position = (this.option2Position = (this.option3Position = SpawnPosition.Undef));
		if (!flag && !flag2)
		{
			this.btnOption1.ViewComponent.UiTransform.gameObject.SetActive(true);
			this.btnOption1.Text = Localization.Get("lblRespawn", false);
			this.option1Method = SpawnMethod.Invalid;
			this.btnOption2.ViewComponent.UiTransform.gameObject.SetActive(false);
			this.btnOption3.ViewComponent.UiTransform.gameObject.SetActive(false);
			return;
		}
		if (flag2 && !flag)
		{
			this.btnOption1.ViewComponent.UiTransform.gameObject.SetActive(true);
			this.btnOption1.Text = Localization.Get("lblSpawnNearBackpack", false);
			this.option1Method = SpawnMethod.NearBackpack;
			this.option1Position = spawnPosition;
			this.btnOption2.ViewComponent.UiTransform.gameObject.SetActive(false);
			this.btnOption3.ViewComponent.UiTransform.gameObject.SetActive(false);
			return;
		}
		if (!flag2 && flag)
		{
			this.btnOption1.ViewComponent.UiTransform.gameObject.SetActive(true);
			this.btnOption1.Text = Localization.Get("lblSpawnOnBedroll", false);
			this.option1Method = SpawnMethod.OnBedRoll;
			this.option1Position = spawnPoint;
			this.btnOption2.ViewComponent.UiTransform.gameObject.SetActive(true);
			this.btnOption2.Text = Localization.Get("lblSpawnNearBedroll", false);
			this.option2Method = SpawnMethod.NearBedroll;
			this.option2Position = spawnPosition;
			this.btnOption3.ViewComponent.UiTransform.gameObject.SetActive(false);
			return;
		}
		this.btnOption1.ViewComponent.UiTransform.gameObject.SetActive(true);
		this.btnOption1.Text = Localization.Get("lblSpawnNearBackpack", false);
		this.option1Method = SpawnMethod.NearBackpack;
		this.option1Position = spawnPosition;
		this.btnOption2.ViewComponent.UiTransform.gameObject.SetActive(true);
		this.btnOption2.Text = Localization.Get("lblSpawnOnBedroll", false);
		this.option2Method = SpawnMethod.OnBedRoll;
		this.option2Position = spawnPoint;
		this.btnOption3.ViewComponent.UiTransform.gameObject.SetActive(true);
		this.btnOption3.Text = Localization.Get("lblSpawnNearBedroll", false);
		this.option3Method = SpawnMethod.NearBedroll;
		this.option3Position = spawnPoint;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SpawnButtonPressed(SpawnMethod _method, SpawnPosition _position)
	{
		base.xui.playerUI.xui.PlayMenuConfirmSound();
		if (!this.bEnteringGame)
		{
			this.spawnMethod = _method;
			this.spawnTarget = _position;
			base.xui.playerUI.windowManager.Close(this.windowGroup.ID);
			return;
		}
		this.ShowButtons = false;
		base.xui.playerUI.CursorController.SetNavigationTarget(null);
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			GameManager.Instance.canSpawnPlayer = true;
			return;
		}
		GameManager.Instance.RequestToSpawn();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		this.ellipsisAnimator.GetNextAnimatedString(_dt);
		if (this.IsDirty)
		{
			base.RefreshBindings(true);
			this.RefreshButtons();
			this.IsDirty = false;
		}
		this.updateLoadState();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		GameManager.Instance.World.GetPrimaryPlayer();
		this.delayCountdownTime = 1f;
		this.spawnMethod = SpawnMethod.Invalid;
		this.ellipsisAnimator.SetBaseString(Localization.Get("msgBuildingEnvironment", false), TextEllipsisAnimator.AnimationMode.All);
		this.showSpawningComponents(false);
		base.RefreshBindings(false);
		((XUiV_Window)base.ViewComponent).Panel.alpha = 1f;
		this.setCursor = false;
		base.xui.playerUI.CursorController.SetCursorHidden(true);
	}

	public override void OnClose()
	{
		base.OnClose();
		this.bChooseSpawnPosition = false;
		base.xui.playerUI.CursorController.SetCursorHidden(false);
		this.setCursor = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateLoadState()
	{
		if (!GameManager.Instance.gameStateManager.IsGameStarted())
		{
			if (this.bEnteringGame)
			{
				this.showSpawningComponents(true);
			}
			return;
		}
		if (GameStats.GetInt(EnumGameStats.GameState) == 2)
		{
			base.xui.playerUI.windowManager.Close(this.windowGroup.ID);
			return;
		}
		if (this.delayCountdownTime > 0f)
		{
			this.delayCountdownTime -= Time.deltaTime;
			return;
		}
		int displayedChunkGameObjectsCount = GameManager.Instance.World.m_ChunkManager.GetDisplayedChunkGameObjectsCount();
		int viewDistance = GameUtils.GetViewDistance();
		int num = (!GameManager.Instance.World.ChunkCache.IsFixedSize) ? (viewDistance * viewDistance - 10) : 0;
		if (displayedChunkGameObjectsCount < num)
		{
			if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				this.ellipsisAnimator.SetBaseString(Localization.Get("msgStartingGame", false), TextEllipsisAnimator.AnimationMode.All);
				GameManager.Instance.World.GetPrimaryPlayer();
				return;
			}
		}
		else if (DistantTerrain.Instance != null && !DistantTerrain.Instance.IsTerrainReady)
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
			{
				this.ellipsisAnimator.SetBaseString(Localization.Get("msgGeneratingDistantTerrain", false), TextEllipsisAnimator.AnimationMode.All);
				return;
			}
		}
		else
		{
			if (!LocalPlayerUI.GetUIForPrimaryPlayer().xui.isReady)
			{
				this.ellipsisAnimator.SetBaseString(Localization.Get("msgLoadingUI", false), TextEllipsisAnimator.AnimationMode.All);
				return;
			}
			if (this.bChooseSpawnPosition && GameManager.Instance.World.GetPrimaryPlayer() != null)
			{
				this.showSpawningComponents(true);
				return;
			}
			base.xui.playerUI.windowManager.Close(this.windowGroup.ID);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void showSpawningComponents(bool _spawning)
	{
		GameManager.Instance.SetCursorEnabledOverride(false, false);
		if (!this.setCursor)
		{
			this.ShowButtons = _spawning;
			base.xui.playerUI.CursorController.SetCursorHidden(false);
			this.btnOption1.SelectCursorElement(true, false);
			this.setCursor = true;
		}
	}

	public static void Open(LocalPlayerUI _playerUi, bool _chooseSpawnPosition, bool _enteringGame)
	{
		XUiC_SpawnSelectionWindow childByType = _playerUi.xui.FindWindowGroupByName(XUiC_SpawnSelectionWindow.ID).GetChildByType<XUiC_SpawnSelectionWindow>();
		childByType.bChooseSpawnPosition = _chooseSpawnPosition;
		childByType.bEnteringGame = _enteringGame;
		_playerUi.windowManager.Open(XUiC_SpawnSelectionWindow.ID, true, true, true);
	}

	public static void Close(LocalPlayerUI _playerUi)
	{
		_playerUi.windowManager.CloseIfOpen(XUiC_SpawnSelectionWindow.ID);
	}

	public static bool IsOpenInUI(LocalPlayerUI _playerUi)
	{
		return _playerUi.windowManager.IsWindowOpen(XUiC_SpawnSelectionWindow.ID);
	}

	public static XUiC_SpawnSelectionWindow GetWindow(LocalPlayerUI _playerUi)
	{
		return _playerUi.xui.FindWindowGroupByName(XUiC_SpawnSelectionWindow.ID).GetChildByType<XUiC_SpawnSelectionWindow>();
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblProgress;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showButtons;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bChooseSpawnPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bEnteringGame;

	[PublicizedFrom(EAccessModifier.Private)]
	public float delayCountdownTime;

	public SpawnMethod spawnMethod;

	public SpawnPosition spawnTarget;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnOption1;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnOption2;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnOption3;

	[PublicizedFrom(EAccessModifier.Private)]
	public SpawnMethod option1Method;

	[PublicizedFrom(EAccessModifier.Private)]
	public SpawnMethod option2Method;

	[PublicizedFrom(EAccessModifier.Private)]
	public SpawnMethod option3Method;

	[PublicizedFrom(EAccessModifier.Private)]
	public SpawnPosition option1Position;

	[PublicizedFrom(EAccessModifier.Private)]
	public SpawnPosition option2Position;

	[PublicizedFrom(EAccessModifier.Private)]
	public SpawnPosition option3Position;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool setCursor;

	[PublicizedFrom(EAccessModifier.Private)]
	public TextEllipsisAnimator ellipsisAnimator;
}

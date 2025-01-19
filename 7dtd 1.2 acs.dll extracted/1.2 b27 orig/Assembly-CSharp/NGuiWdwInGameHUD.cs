using System;
using UnityEngine;

public class NGuiWdwInGameHUD : MonoBehaviour
{
	public GameManager gameManager
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return GameManager.Instance;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Awake()
	{
		this.playerUI = base.GetComponentInParent<LocalPlayerUI>();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Start()
	{
		NGuiAction nguiAction = new NGuiAction("TPV", PlayerActionsGlobal.Instance.SwitchView);
		nguiAction.SetClickActionDelegate(delegate
		{
			if (this.playerEntity)
			{
				this.playerEntity.SwitchFirstPersonView(true);
			}
		});
		nguiAction.SetIsCheckedDelegate(() => this.playerEntity != null && this.playerEntity.bFirstPersonView);
		nguiAction.SetIsEnabledDelegate(() => this.playerEntity != null && this.playerEntity.Spawned && !this.playerEntity.IsDead() && GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled));
		NGuiAction nguiAction2 = new NGuiAction("DebugSpawn", PlayerActionsGlobal.Instance.DebugSpawn);
		nguiAction2.SetClickActionDelegate(delegate
		{
			this.playerUI.windowManager.SwitchVisible(XUiC_SpawnMenu.ID, false);
		});
		nguiAction2.SetIsEnabledDelegate(() => GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled));
		NGuiAction nguiAction3 = new NGuiAction("DebugGameEvent", PlayerActionsGlobal.Instance.DebugGameEvent);
		nguiAction3.SetClickActionDelegate(delegate
		{
			this.playerUI.windowManager.SwitchVisible(XUiC_GameEventMenu.ID, false);
		});
		nguiAction3.SetIsEnabledDelegate(() => GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled));
		NGuiAction nguiAction4 = new NGuiAction("SwitchHUD", PlayerActionsGlobal.Instance.SwitchHUD);
		nguiAction4.SetClickActionDelegate(delegate
		{
			this.playerUI.windowManager.ToggleHUDEnabled();
		});
		this.playerUI.windowManager.AddGlobalAction(nguiAction);
		this.playerUI.windowManager.AddGlobalAction(nguiAction2);
		this.playerUI.windowManager.AddGlobalAction(nguiAction3);
		this.playerUI.windowManager.AddGlobalAction(nguiAction4);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		this.playerEntity = this.playerUI.entityPlayer;
		this.playerUI.OnEntityPlayerLocalAssigned += this.HandleEntityPlayerLocalAssigned;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDisable()
	{
		this.playerUI.OnEntityPlayerLocalAssigned -= this.HandleEntityPlayerLocalAssigned;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleEntityPlayerLocalAssigned(EntityPlayerLocal _entity)
	{
		this.playerEntity = _entity;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnGUI()
	{
		if (!this.gameManager.gameStateManager.IsGameStarted())
		{
			return;
		}
		string @string = GameStats.GetString(EnumGameStats.ShowWindow);
		if (!string.IsNullOrEmpty(@string))
		{
			if (!this.playerUI.windowManager.IsWindowOpen(@string))
			{
				this.playerUI.windowManager.Open(@string, false, false, true);
				this.wdwOpenedOnGameStatsShowWindow = @string;
			}
		}
		else if (this.wdwOpenedOnGameStatsShowWindow != null)
		{
			this.playerUI.windowManager.Close(this.wdwOpenedOnGameStatsShowWindow);
			this.wdwOpenedOnGameStatsShowWindow = null;
		}
		if (this.playerEntity != null)
		{
			this.playerEntity.OnHUD();
		}
		if (!this.playerUI.windowManager.IsHUDEnabled())
		{
			return;
		}
		int @int = GameStats.GetInt(EnumGameStats.GameState);
		if (@int != 1)
		{
		}
	}

	public Texture2D[] overlayDamageTextures = new Texture2D[8];

	public Texture2D[] overlayDamageBloodDrops = new Texture2D[3];

	public Texture2D CrosshairTexture;

	public Texture2D CrosshairDamage;

	public Texture2D CrosshairUpgrade;

	public Texture2D CrosshairRepair;

	public Texture2D CrosshairAiming;

	public Texture2D CrosshairPowerSource;

	public Texture2D CrosshairPowerItem;

	public Texture2D[] StealthIcons = new Texture2D[5];

	public Texture2D[] StealthOverlays = new Texture2D[2];

	public Transform FocusCube;

	public float crosshairAlpha = 1f;

	public bool showCrosshair = true;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string wdwOpenedOnGameStatsShowWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public EntityPlayerLocal playerEntity;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public LocalPlayerUI playerUI;
}

using System;
using System.Collections;
using System.Collections.Generic;
using Audio;
using InControl;
using Platform;
using UnityEngine;

public class PlayerMoveController : MonoBehaviour
{
	public bool RunToggleActive
	{
		get
		{
			return this.runToggleActive;
		}
	}

	public PlayerActionsLocal playerInput
	{
		get
		{
			return PlatformManager.NativePlatform.Input.PrimaryPlayer;
		}
	}

	public void Init()
	{
		PlayerMoveController.Instance = this;
		this.entityPlayerLocal = base.GetComponent<EntityPlayerLocal>();
		this.playerUI = LocalPlayerUI.GetUIForPlayer(this.entityPlayerLocal);
		this.windowManager = this.playerUI.windowManager;
		this.nguiWindowManager = this.playerUI.nguiWindowManager;
		this.gameManager = (GameManager)UnityEngine.Object.FindObjectOfType(typeof(GameManager));
		this.guiInGame = this.nguiWindowManager.InGameHUD;
		this.nguiWindowManager.Show(EnumNGUIWindow.InGameHUD, true);
		PlayerMoveController.UpdateControlsOptions();
		GamePrefs.Set(EnumGamePrefs.DebugMenuShowTasks, false);
		this.focusBoxScript = new RenderDisplacedCube((this.guiInGame.FocusCube != null) ? UnityEngine.Object.Instantiate<Transform>(this.guiInGame.FocusCube) : null);
		this.playerAutoPilotControllor = new PlayerAutoPilotControllor(this.gameManager);
		this.toggleGodMode = delegate()
		{
			this.entityPlayerLocal.bEntityAliveFlagsChanged = true;
			this.entityPlayerLocal.IsGodMode.Value = !this.entityPlayerLocal.IsGodMode.Value;
			this.entityPlayerLocal.IsNoCollisionMode.Value = this.entityPlayerLocal.IsGodMode.Value;
			this.entityPlayerLocal.IsFlyMode.Value = this.entityPlayerLocal.IsGodMode.Value;
			if (this.entityPlayerLocal.IsGodMode.Value)
			{
				this.entityPlayerLocal.Buffs.AddBuff("god", -1, true, false, -1f);
				return;
			}
			if (!GameManager.Instance.World.IsEditor() && !GameModeCreative.TypeName.Equals(GamePrefs.GetString(EnumGamePrefs.GameMode)))
			{
				this.entityPlayerLocal.Buffs.RemoveBuff("god", true);
			}
		};
		this.teleportPlayer = delegate()
		{
			Ray lookRay = this.entityPlayerLocal.GetLookRay();
			if (InputUtils.ControlKeyPressed)
			{
				lookRay.direction *= -1f;
			}
			lookRay.origin -= Origin.position;
			RaycastHit raycastHit;
			Vector3 a;
			if (Physics.SphereCast(lookRay, 0.3f, out raycastHit, 500f, 1342242816))
			{
				a = raycastHit.point - lookRay.direction.normalized * 0.5f;
			}
			else
			{
				a = lookRay.origin + lookRay.direction.normalized * 100f;
			}
			this.entityPlayerLocal.SetPosition(a + Origin.position, true);
			GameEventManager.Current.HandleForceBossDespawn(this.entityPlayerLocal);
		};
		NGuiAction nguiAction = new NGuiAction("SelectionMode", null, null, true, this.playerInput.Drop);
		nguiAction.SetClickActionDelegate(delegate
		{
			if (InputUtils.AltKeyPressed)
			{
				GamePrefs.Set(EnumGamePrefs.SelectionOperationMode, 4);
				return;
			}
			if (InputUtils.ShiftKeyPressed)
			{
				GamePrefs.Set(EnumGamePrefs.SelectionOperationMode, 2);
				GameManager.Instance.SetCursorEnabledOverride(true, true);
				return;
			}
			if (InputUtils.ControlKeyPressed && GameManager.Instance.IsEditMode())
			{
				GamePrefs.Set(EnumGamePrefs.SelectionOperationMode, 3);
				GameManager.Instance.SetCursorEnabledOverride(true, true);
				return;
			}
			GamePrefs.Set(EnumGamePrefs.SelectionOperationMode, 1);
			GameManager.Instance.SetCursorEnabledOverride(true, true);
		});
		nguiAction.SetReleaseActionDelegate(delegate
		{
			GamePrefs.Set(EnumGamePrefs.SelectionOperationMode, 0);
			GameManager.Instance.SetCursorEnabledOverride(false, false);
		});
		nguiAction.SetIsEnabledDelegate(() => (this.gameManager.gameStateManager.IsGameStarted() && GameStats.GetInt(EnumGameStats.GameState) == 1 && GameManager.Instance.World.IsEditor()) || BlockToolSelection.Instance.SelectionActive);
		this.globalActions.Add(nguiAction);
		NGuiAction.IsEnabledDelegate menuIsEnabled = () => !XUiC_SpawnSelectionWindow.IsOpenInUI(LocalPlayerUI.primaryUI) && this.gameManager.gameStateManager.IsGameStarted() && GameStats.GetInt(EnumGameStats.GameState) == 1 && !LocalPlayerUI.primaryUI.windowManager.IsModalWindowOpen() && !this.windowManager.IsFullHUDDisabled();
		NGuiAction.OnClickActionDelegate clickActionDelegate = delegate()
		{
			this.entityPlayerLocal.AimingGun = false;
			if (this.windowManager.IsWindowOpen("windowpaging") || this.windowManager.IsModalWindowOpen())
			{
				this.windowManager.CloseAllOpenWindows(null, false);
				this.windowManager.CloseIfOpen("windowpaging");
				return;
			}
			this.windowManager.CloseAllOpenWindows(null, false);
			this.playerUI.xui.RadialWindow.Open();
			this.playerUI.xui.RadialWindow.SetupMenuData();
		};
		NGuiAction.IsCheckedDelegate isCheckedDelegate = () => this.windowManager.IsWindowOpen("windowpaging");
		NGuiAction.IsEnabledDelegate isEnabledDelegate = () => menuIsEnabled() && !this.windowManager.IsWindowOpen("radial");
		NGuiAction nguiAction2 = new NGuiAction("Inventory", null, null, true, this.playerInput.Inventory);
		nguiAction2.SetClickActionDelegate(clickActionDelegate);
		nguiAction2.SetIsEnabledDelegate(isEnabledDelegate);
		nguiAction2.SetIsCheckedDelegate(isCheckedDelegate);
		this.globalActions.Add(nguiAction2);
		NGuiAction nguiAction3 = new NGuiAction("Inventory", null, null, true, this.playerInput.PermanentActions.Inventory);
		nguiAction3.SetClickActionDelegate(clickActionDelegate);
		nguiAction3.SetIsEnabledDelegate(isEnabledDelegate);
		nguiAction3.SetIsCheckedDelegate(isCheckedDelegate);
		this.globalActions.Add(nguiAction3);
		NGuiAction nguiAction4 = new NGuiAction("Inventory", null, null, true, this.playerInput.VehicleActions.Inventory);
		nguiAction4.SetClickActionDelegate(clickActionDelegate);
		nguiAction4.SetIsEnabledDelegate(isEnabledDelegate);
		nguiAction4.SetIsCheckedDelegate(isCheckedDelegate);
		this.globalActions.Add(nguiAction4);
		NGuiAction nguiAction5 = new NGuiAction("Creative", null, null, true, this.playerInput.PermanentActions.Creative);
		nguiAction5.SetClickActionDelegate(delegate
		{
			this.entityPlayerLocal.AimingGun = false;
			XUiC_WindowSelector.OpenSelectorAndWindow(this.entityPlayerLocal, "creative");
		});
		nguiAction5.SetIsEnabledDelegate(() => menuIsEnabled() && (GameStats.GetBool(EnumGameStats.IsCreativeMenuEnabled) || GamePrefs.GetBool(EnumGamePrefs.CreativeMenuEnabled)));
		nguiAction5.SetIsCheckedDelegate(() => this.windowManager.IsWindowOpen("creative"));
		this.globalActions.Add(nguiAction5);
		NGuiAction nguiAction6 = new NGuiAction("Map", null, null, true, this.playerInput.PermanentActions.Map);
		nguiAction6.SetClickActionDelegate(delegate
		{
			this.entityPlayerLocal.AimingGun = false;
			XUiC_WindowSelector.OpenSelectorAndWindow(this.entityPlayerLocal, "map");
		});
		nguiAction6.SetIsEnabledDelegate(menuIsEnabled);
		nguiAction6.SetIsCheckedDelegate(() => this.windowManager.IsWindowOpen("map"));
		this.globalActions.Add(nguiAction6);
		NGuiAction nguiAction7 = new NGuiAction("Character", null, null, true, this.playerInput.PermanentActions.Character);
		nguiAction7.SetClickActionDelegate(delegate
		{
			this.entityPlayerLocal.AimingGun = false;
			XUiC_WindowSelector.OpenSelectorAndWindow(this.entityPlayerLocal, "character");
		});
		nguiAction7.SetIsEnabledDelegate(menuIsEnabled);
		this.globalActions.Add(nguiAction7);
		NGuiAction nguiAction8 = new NGuiAction("Skills", null, null, true, this.playerInput.PermanentActions.Skills);
		nguiAction8.SetClickActionDelegate(delegate
		{
			this.entityPlayerLocal.AimingGun = false;
			XUiC_WindowSelector.OpenSelectorAndWindow(this.entityPlayerLocal, "skills");
		});
		nguiAction8.SetIsEnabledDelegate(menuIsEnabled);
		this.globalActions.Add(nguiAction8);
		NGuiAction nguiAction9 = new NGuiAction("Quests", null, null, true, this.playerInput.PermanentActions.Quests);
		nguiAction9.SetClickActionDelegate(delegate
		{
			this.entityPlayerLocal.AimingGun = false;
			XUiC_WindowSelector.OpenSelectorAndWindow(this.entityPlayerLocal, "quests");
		});
		nguiAction9.SetIsEnabledDelegate(menuIsEnabled);
		this.globalActions.Add(nguiAction9);
		NGuiAction.OnClickActionDelegate clickActionDelegate2 = delegate()
		{
			this.entityPlayerLocal.AimingGun = false;
			XUiC_WindowSelector.OpenSelectorAndWindow(this.entityPlayerLocal, "players");
		};
		NGuiAction nguiAction10 = new NGuiAction("Players", null, null, true, this.playerInput.Scoreboard);
		nguiAction10.SetClickActionDelegate(clickActionDelegate2);
		nguiAction10.SetIsEnabledDelegate(menuIsEnabled);
		this.globalActions.Add(nguiAction10);
		NGuiAction nguiAction11 = new NGuiAction("Players", null, null, true, this.playerInput.VehicleActions.Scoreboard);
		nguiAction11.SetClickActionDelegate(clickActionDelegate2);
		nguiAction11.SetIsEnabledDelegate(menuIsEnabled);
		this.globalActions.Add(nguiAction11);
		NGuiAction nguiAction12 = new NGuiAction("Players", null, null, true, this.playerInput.PermanentActions.Scoreboard);
		nguiAction12.SetClickActionDelegate(clickActionDelegate2);
		nguiAction12.SetIsEnabledDelegate(menuIsEnabled);
		this.globalActions.Add(nguiAction12);
		NGuiAction nguiAction13 = new NGuiAction("Challenges", null, null, true, this.playerInput.PermanentActions.Challenges);
		nguiAction13.SetClickActionDelegate(delegate
		{
			this.entityPlayerLocal.AimingGun = false;
			XUiC_WindowSelector.OpenSelectorAndWindow(this.entityPlayerLocal, "challenges");
		});
		nguiAction13.SetIsEnabledDelegate(menuIsEnabled);
		this.globalActions.Add(nguiAction13);
		NGuiAction nguiAction14 = new NGuiAction("Chat", null, null, true, this.playerInput.PermanentActions.Chat);
		nguiAction14.SetClickActionDelegate(delegate
		{
			this.entityPlayerLocal.AimingGun = false;
			this.windowManager.Open(XUiC_Chat.ID, true, false, true);
		});
		nguiAction14.SetIsEnabledDelegate(menuIsEnabled);
		nguiAction14.SetIsCheckedDelegate(() => this.windowManager.IsWindowOpen(XUiC_Chat.ID));
		this.globalActions.Add(nguiAction14);
		NGuiAction nguiAction15 = new NGuiAction("Prefab", null, null, true, this.playerInput.Prefab);
		nguiAction15.SetClickActionDelegate(delegate
		{
			this.entityPlayerLocal.AimingGun = false;
			Manager.PlayButtonClick();
			this.windowManager.SwitchVisible(GUIWindowWOChooseCategory.ID, false);
		});
		nguiAction15.SetIsEnabledDelegate(() => menuIsEnabled() && this.gameManager.IsEditMode());
		nguiAction15.SetIsCheckedDelegate(() => this.windowManager.IsWindowOpen(GUIWindowWOChooseCategory.ID));
		this.globalActions.Add(nguiAction15);
		NGuiAction nguiAction16 = new NGuiAction("DetachCamera", null, null, false, this.playerInput.DetachCamera);
		nguiAction16.SetClickActionDelegate(delegate
		{
			Manager.PlayButtonClick();
			this.entityPlayerLocal.SetCameraAttachedToPlayer(!this.entityPlayerLocal.IsCameraAttachedToPlayerOrScope());
		});
		nguiAction16.SetIsEnabledDelegate(() => this.gameManager.gameStateManager.IsGameStarted() && GameStats.GetInt(EnumGameStats.GameState) == 1 && !this.entityPlayerLocal.AimingGun && (this.gameManager.IsEditMode() || GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled)) && !InputUtils.ControlKeyPressed);
		this.globalActions.Add(nguiAction16);
		NGuiAction nguiAction17 = new NGuiAction("ToggleDCMove", null, null, false, this.playerInput.ToggleDCMove);
		nguiAction17.SetClickActionDelegate(delegate
		{
			Manager.PlayButtonClick();
			this.entityPlayerLocal.movementInput.bDetachedCameraMove = (!this.entityPlayerLocal.movementInput.bDetachedCameraMove && !this.entityPlayerLocal.IsCameraAttachedToPlayerOrScope());
		});
		nguiAction17.SetIsEnabledDelegate(() => this.gameManager.gameStateManager.IsGameStarted() && GameStats.GetInt(EnumGameStats.GameState) == 1 && !this.entityPlayerLocal.AimingGun && (this.gameManager.IsEditMode() || GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled)));
		this.globalActions.Add(nguiAction17);
		NGuiAction nguiAction18 = new NGuiAction("LockCamera", null, null, false, this.playerInput.LockFreeCamera);
		nguiAction18.SetClickActionDelegate(delegate
		{
			Manager.PlayButtonClick();
			this.entityPlayerLocal.movementInput.bCameraPositionLocked = !this.entityPlayerLocal.movementInput.bCameraPositionLocked;
		});
		nguiAction18.SetIsEnabledDelegate(() => this.gameManager.gameStateManager.IsGameStarted() && GameStats.GetInt(EnumGameStats.GameState) == 1 && !this.entityPlayerLocal.AimingGun && (this.gameManager.IsEditMode() || GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled)));
		this.globalActions.Add(nguiAction18);
		NGuiAction.OnClickActionDelegate clickActionDelegate3 = delegate()
		{
			if (!XUiC_SpawnSelectionWindow.IsOpenInUI(LocalPlayerUI.primaryUI))
			{
				Manager.PlayButtonClick();
				if (!this.windowManager.CloseAllOpenWindows(null, false))
				{
					this.entityPlayerLocal.PlayerUI.CursorController.HoverTarget = null;
					this.windowManager.SwitchVisible(XUiC_InGameMenuWindow.ID, false);
				}
			}
		};
		NGuiAction nguiAction19 = new NGuiAction("Menu", null, null, false, this.playerInput.Menu);
		nguiAction19.SetClickActionDelegate(clickActionDelegate3);
		nguiAction19.SetIsEnabledDelegate(() => !this.windowManager.IsFullHUDDisabled());
		this.globalActions.Add(nguiAction19);
		NGuiAction nguiAction20 = new NGuiAction("Menu", null, null, false, this.playerInput.VehicleActions.Menu);
		nguiAction20.SetClickActionDelegate(clickActionDelegate3);
		nguiAction20.SetIsEnabledDelegate(() => !this.windowManager.IsFullHUDDisabled());
		this.globalActions.Add(nguiAction20);
		NGuiAction nguiAction21 = new NGuiAction("Fly Mode", null, null, true, this.playerInput.Fly);
		nguiAction21.SetClickActionDelegate(delegate
		{
			Manager.PlayButtonClick();
			this.entityPlayerLocal.IsFlyMode.Value = !this.entityPlayerLocal.IsFlyMode.Value;
		});
		nguiAction21.SetIsCheckedDelegate(() => this.entityPlayerLocal != null && this.entityPlayerLocal.IsFlyMode.Value);
		nguiAction21.SetIsEnabledDelegate(() => GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled) || GamePrefs.GetBool(EnumGamePrefs.CreativeMenuEnabled) || GameStats.GetBool(EnumGameStats.IsFlyingEnabled));
		this.globalActions.Add(nguiAction21);
		NGuiAction nguiAction22 = new NGuiAction("God Mode", null, null, true, this.playerInput.God);
		nguiAction22.SetClickActionDelegate(delegate
		{
			Manager.PlayButtonClick();
			if (InputUtils.ShiftKeyPressed)
			{
				this.teleportPlayer();
				return;
			}
			this.toggleGodMode();
		});
		nguiAction22.SetIsCheckedDelegate(() => this.entityPlayerLocal != null && this.entityPlayerLocal.IsGodMode.Value);
		nguiAction22.SetIsEnabledDelegate(() => GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled) || !GameStats.GetBool(EnumGameStats.IsPlayerDamageEnabled));
		this.globalActions.Add(nguiAction22);
		NGuiAction nguiAction23 = new NGuiAction("No Collision", null, null, true, null);
		nguiAction23.SetClickActionDelegate(delegate
		{
			Manager.PlayButtonClick();
			this.entityPlayerLocal.IsNoCollisionMode.Value = !this.entityPlayerLocal.IsNoCollisionMode.Value;
		});
		nguiAction23.SetIsCheckedDelegate(() => this.entityPlayerLocal != null && this.entityPlayerLocal.IsNoCollisionMode.Value);
		nguiAction23.SetIsEnabledDelegate(() => GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled) || !GameStats.GetBool(EnumGameStats.IsPlayerCollisionEnabled));
		this.globalActions.Add(nguiAction23);
		NGuiAction nguiAction24 = new NGuiAction("Invisible", null, null, true, this.playerInput.Invisible);
		nguiAction24.SetClickActionDelegate(delegate
		{
			Manager.PlayButtonClick();
			this.entityPlayerLocal.IsSpectator = !this.entityPlayerLocal.IsSpectator;
		});
		nguiAction24.SetIsCheckedDelegate(() => this.entityPlayerLocal != null && this.entityPlayerLocal.IsSpectator);
		nguiAction24.SetIsEnabledDelegate(() => GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled) || !GameStats.GetBool(EnumGameStats.IsPlayerDamageEnabled));
		this.globalActions.Add(nguiAction24);
		for (int i = 0; i < this.globalActions.Count; i++)
		{
			this.windowManager.AddGlobalAction(this.globalActions[i]);
		}
		EAIManager.isAnimFreeze = false;
	}

	public static void UpdateControlsOptions()
	{
		if (PlayerMoveController.Instance != null)
		{
			PlayerMoveController.Instance.invertMouse = (GamePrefs.GetBool(EnumGamePrefs.OptionsInvertMouse) ? -1 : 1);
			PlayerMoveController.Instance.invertController = (GamePrefs.GetBool(EnumGamePrefs.OptionsControllerLookInvert) ? -1 : 1);
			PlayerMoveController.Instance.bControllerVibration = GamePrefs.GetBool(EnumGamePrefs.OptionsControllerVibration);
			PlayerMoveController.Instance.UpdateLookSensitivity(GamePrefs.GetFloat(EnumGamePrefs.OptionsLookSensitivity), GamePrefs.GetFloat(EnumGamePrefs.OptionsZoomSensitivity), GamePrefs.GetFloat(EnumGamePrefs.OptionsZoomAccel), GamePrefs.GetFloat(EnumGamePrefs.OptionsVehicleLookSensitivity), new Vector2(GamePrefs.GetFloat(EnumGamePrefs.OptionsControllerSensitivityX), GamePrefs.GetFloat(EnumGamePrefs.OptionsControllerSensitivityY)));
			PlayerMoveController.Instance.lookAccelerationRate = GamePrefs.GetFloat(EnumGamePrefs.OptionsControllerLookAcceleration) * 0.5f;
			PlayerMoveController.Instance.controllerZoomSensitivity = GamePrefs.GetFloat(EnumGamePrefs.OptionsControllerZoomSensitivity);
			PlayerMoveController.Instance.controllerVehicleSensitivity = GamePrefs.GetFloat(EnumGamePrefs.OptionsControllerVehicleSensitivity);
			PlayerMoveController.Instance.controllerAimAssistsEnabled = GamePrefs.GetBool(EnumGamePrefs.OptionsControllerAimAssists);
			PlayerMoveController.Instance.playerInput.SetJoyStickLayout((eControllerJoystickLayout)GamePrefs.GetInt(EnumGamePrefs.OptionsControllerJoystickLayout));
			PlayerMoveController.Instance.sprintLockEnabled = GamePrefs.GetBool(EnumGamePrefs.OptionsControlsSprintLock);
			PlayerMoveController.SetDeadzones();
		}
	}

	public static void SetDeadzones()
	{
		PlayerMoveController.Instance.playerInput.SetDeadzones(GamePrefs.GetFloat(EnumGamePrefs.OptionsControllerLookAxisDeadzone), GamePrefs.GetFloat(EnumGamePrefs.OptionsControllerMoveAxisDeadzone));
	}

	public void UpdateInvertMouse(bool _invertMouse)
	{
		this.invertMouse = (_invertMouse ? -1 : 1);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateLookSensitivity(float _sensitivity, float _zoomSensitivity, float _zoomAccel, float _vehicleSensitivity, Vector2 _controllerLookSensitivity)
	{
		this.mouseLookSensitivity = Vector3.one * _sensitivity * 5f;
		this.mouseZoomSensitivity = _zoomSensitivity;
		this.zoomAccel = _zoomAccel * 0.5f;
		this.vehicleLookSensitivity = _vehicleSensitivity * 5f;
		this.controllerLookSensitivity = _controllerLookSensitivity * 10f;
	}

	public Vector2 GetCameraInputSensitivity()
	{
		if (this.playerInput.LastInputType == BindingSourceType.DeviceBindingSource)
		{
			return this.controllerLookSensitivity;
		}
		return this.mouseLookSensitivity;
	}

	public bool GetControllerVibration()
	{
		return this.bControllerVibration;
	}

	public void UpdateControllerVibration(bool _controllerVibration)
	{
		this.bControllerVibration = _controllerVibration;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnDestroy()
	{
		for (int i = 0; i < this.globalActions.Count; i++)
		{
			this.windowManager.RemoveGlobalAction(this.globalActions[i]);
		}
		this.focusBoxScript.Cleanup();
		PlayerMoveController.Instance = null;
		this.playerUI = null;
		this.windowManager = null;
		this.nguiWindowManager = null;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnGUI()
	{
		if (!this.gameManager.gameStateManager.IsGameStarted() || GameStats.GetInt(EnumGameStats.GameState) != 1)
		{
			return;
		}
		if (this.windowManager.IsFullHUDDisabled())
		{
			return;
		}
		if (this.entityPlayerLocal.inventory != null && this.gameManager.World.worldTime % 2UL == 0UL)
		{
			ItemValue holdingItemItemValue = this.entityPlayerLocal.inventory.holdingItemItemValue;
			ItemClass forId = ItemClass.GetForId(holdingItemItemValue.type);
			int maxUseTimes = holdingItemItemValue.MaxUseTimes;
			if (maxUseTimes > 0 && forId.MaxUseTimesBreaksAfter.Value && holdingItemItemValue.UseTimes >= (float)maxUseTimes)
			{
				this.entityPlayerLocal.inventory.DecHoldingItem(1);
				if (forId.Properties.Values.ContainsKey(ItemClass.PropSoundDestroy))
				{
					Manager.BroadcastPlay(this.entityPlayerLocal, forId.Properties.Values[ItemClass.PropSoundDestroy], false);
				}
			}
			this.entityPlayerLocal.equipment.CheckBreakUseItems();
		}
		if (!this.windowManager.IsInputActive() && !this.windowManager.IsModalWindowOpen() && Event.current.rawType == EventType.KeyDown && this.gameManager.IsEditMode() && this.entityPlayerLocal.inventory != null)
		{
			this.gameManager.GetActiveBlockTool().CheckSpecialKeys(Event.current, this.playerInput);
			if (XUiC_WoPropsPOIMarker.Instance != null)
			{
				XUiC_WoPropsPOIMarker.Instance.CheckSpecialKeys(Event.current, this.playerInput);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Start()
	{
		if (this.entityPlayerLocal.inventory.GetFocusedItemIdx() < 0 || this.entityPlayerLocal.inventory.GetFocusedItemIdx() >= this.entityPlayerLocal.inventory.PUBLIC_SLOTS)
		{
			this.entityPlayerLocal.inventory.SetFocusedItemIdx(0);
			this.entityPlayerLocal.inventory.SetHoldingItemIdx(0);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateRespawn()
	{
		if (this.entityPlayerLocal.Spawned)
		{
			if (this.unstuckCoState == ERoutineState.Running && (this.playerInput.GUIActions.Cancel.WasPressed || this.windowManager.IsWindowOpen(XUiC_InGameMenuWindow.ID)))
			{
				this.unstuckCoState = ERoutineState.Cancelled;
			}
			return;
		}
		if (GameManager.IsVideoPlaying())
		{
			return;
		}
		if (!this.bLastRespawnActive)
		{
			this.spawnWindowOpened = false;
			this.spawnPosition = SpawnPosition.Undef;
			this.entityPlayerLocal.BeforePlayerRespawn(this.respawnReason);
			this.bLastRespawnActive = true;
			this.waitingForSpawnPointSelection = false;
		}
		this.entityPlayerLocal.ResetLastTickPos(this.entityPlayerLocal.GetPosition());
		this.respawnTime -= Time.deltaTime;
		if (this.respawnTime > 0f)
		{
			return;
		}
		this.respawnTime = 0f;
		if (this.spawnWindowOpened && XUiC_SpawnSelectionWindow.IsOpenInUI(LocalPlayerUI.primaryUI))
		{
			if (Mathf.Abs(this.entityPlayerLocal.GetPosition().y - Constants.cStartPositionPlayerInLevel.y) < 0.01f)
			{
				Vector3 position = this.entityPlayerLocal.GetPosition();
				Vector3i blockPosition = this.entityPlayerLocal.GetBlockPosition();
				if (this.gameManager.World.GetChunkFromWorldPos(blockPosition) != null)
				{
					float num = (float)(this.gameManager.World.GetHeight(blockPosition.x, blockPosition.z) + 1);
					if (position.y < 0f || num < position.y || (num > position.y && num - 2.5f < position.y))
					{
						this.entityPlayerLocal.SetPosition(new Vector3(this.entityPlayerLocal.GetPosition().x, num, this.entityPlayerLocal.GetPosition().z), true);
					}
				}
			}
			if (this.playerAutoPilotControllor != null && this.playerAutoPilotControllor.IsEnabled())
			{
				XUiC_SpawnSelectionWindow.Close(LocalPlayerUI.primaryUI);
			}
			return;
		}
		bool flag = this.respawnReason == RespawnType.NewGame || this.respawnReason == RespawnType.EnterMultiplayer || this.respawnReason == RespawnType.JoinMultiplayer || this.respawnReason == RespawnType.LoadedGame;
		Entity entity = this.entityPlayerLocal.AttachedToEntity ? this.entityPlayerLocal.AttachedToEntity : this.entityPlayerLocal;
		switch (this.respawnReason)
		{
		case RespawnType.NewGame:
			if (!this.spawnWindowOpened)
			{
				this.openSpawnWindow(this.respawnReason);
				return;
			}
			this.spawnPosition = new SpawnPosition(this.entityPlayerLocal.GetPosition(), this.entityPlayerLocal.rotation.y);
			goto IL_5D7;
		case RespawnType.LoadedGame:
			if (!this.spawnWindowOpened)
			{
				this.spawnPosition = new SpawnPosition(this.entityPlayerLocal.GetPosition(), this.entityPlayerLocal.rotation.y);
				this.entityPlayerLocal.SetPosition(this.spawnPosition.position, true);
				this.openSpawnWindow(this.respawnReason);
				return;
			}
			this.spawnPosition = new SpawnPosition(this.entityPlayerLocal.GetPosition(), this.entityPlayerLocal.rotation.y);
			goto IL_5D7;
		case RespawnType.Teleport:
			this.spawnPosition = new SpawnPosition(entity.GetPosition(), entity.rotation.y);
			this.spawnPosition.position.y = -1f;
			goto IL_5D7;
		case RespawnType.EnterMultiplayer:
		case RespawnType.JoinMultiplayer:
			if (!this.spawnWindowOpened)
			{
				this.spawnPosition = new SpawnPosition(this.entityPlayerLocal.GetPosition(), this.entityPlayerLocal.rotation.y);
				if ((this.spawnPosition.IsUndef() || this.spawnPosition.position.Equals(Constants.cStartPositionPlayerInLevel)) && !this.entityPlayerLocal.lastSpawnPosition.IsUndef())
				{
					this.spawnPosition = this.entityPlayerLocal.lastSpawnPosition;
				}
				if (this.spawnPosition.IsUndef() || this.spawnPosition.position.Equals(Constants.cStartPositionPlayerInLevel))
				{
					this.spawnPosition = this.gameManager.GetSpawnPointList().GetRandomSpawnPosition(this.entityPlayerLocal.world, null, 0, 0);
				}
				this.entityPlayerLocal.SetPosition(new Vector3(this.spawnPosition.position.x, (this.spawnPosition.position.y == 0f) ? Constants.cStartPositionPlayerInLevel.y : this.spawnPosition.position.y, this.spawnPosition.position.z), true);
				this.openSpawnWindow(this.respawnReason);
				return;
			}
			this.spawnPosition = new SpawnPosition(this.entityPlayerLocal.GetPosition(), this.entityPlayerLocal.rotation.y);
			goto IL_5D7;
		}
		if (!this.gameManager.IsEditMode() && !this.spawnWindowOpened)
		{
			this.openSpawnWindow(this.respawnReason);
			return;
		}
		XUiC_SpawnSelectionWindow window = XUiC_SpawnSelectionWindow.GetWindow(LocalPlayerUI.primaryUI);
		if (!this.waitingForSpawnPointSelection && !this.gameManager.IsEditMode() && this.spawnWindowOpened && window.spawnMethod != SpawnMethod.Invalid)
		{
			base.StartCoroutine(this.FindRespawnSpawnPointRoutine(window.spawnMethod, window.spawnTarget));
			window.spawnMethod = SpawnMethod.Invalid;
			window.spawnTarget = SpawnPosition.Undef;
		}
		if (this.waitingForSpawnPointSelection)
		{
			return;
		}
		if (this.entityPlayerLocal.position != this.spawnPosition.position)
		{
			Vector3 position2 = this.spawnPosition.position;
			if (this.spawnPosition.IsUndef())
			{
				position2 = this.entityPlayerLocal.GetPosition();
			}
			this.spawnPosition = new SpawnPosition(position2 + new Vector3(0f, 5f, 0f), this.entityPlayerLocal.rotation.y);
			this.entityPlayerLocal.SetPosition(this.spawnPosition.position, true);
		}
		IL_5D7:
		if (GameUtils.IsPlaytesting() || (GameManager.Instance.IsEditMode() && GamePrefs.GetString(EnumGamePrefs.GameWorld) == "Empty"))
		{
			SpawnPointList spawnPointList = GameManager.Instance.GetSpawnPointList();
			if (this.respawnReason != RespawnType.Teleport && spawnPointList.Count > 0)
			{
				this.spawnPosition.position = spawnPointList[0].spawnPosition.position;
				this.spawnPosition.heading = spawnPointList[0].spawnPosition.heading;
				this.entityPlayerLocal.SetPosition(this.spawnPosition.position, true);
			}
		}
		if (!this.spawnPosition.IsUndef())
		{
			if (!PrefabEditModeManager.Instance.IsActive() && !this.gameManager.World.IsPositionAvailable(this.spawnPosition.ClrIdx, this.spawnPosition.position))
			{
				this.spawnPosition.position = this.gameManager.World.ClampToValidWorldPos(this.spawnPosition.position);
				if (this.entityPlayerLocal.position != this.spawnPosition.position)
				{
					this.entityPlayerLocal.SetPosition(this.spawnPosition.position, true);
				}
				return;
			}
			if (!this.entityPlayerLocal.CheckSpawnPointStillThere())
			{
				this.entityPlayerLocal.RemoveSpawnPoints(true);
				if (flag)
				{
					this.entityPlayerLocal.QuestJournal.RemoveAllSharedQuests();
					this.entityPlayerLocal.QuestJournal.StartQuests();
				}
			}
			Vector3i vector3i = World.worldToBlockPos(this.spawnPosition.position);
			float num2 = (float)(this.gameManager.World.GetHeight(vector3i.x, vector3i.z) + 1);
			if (this.spawnPosition.position.y < 0f || this.spawnPosition.position.y > num2)
			{
				this.spawnPosition.position.y = num2;
			}
			else if (this.spawnPosition.position.y < num2 && !this.gameManager.World.CanPlayersSpawnAtPos(this.spawnPosition.position, true))
			{
				this.spawnPosition.position.y = this.spawnPosition.position.y + 1f;
				if (!this.gameManager.World.CanPlayersSpawnAtPos(this.spawnPosition.position, true))
				{
					this.spawnPosition.position.y = num2;
				}
			}
		}
		Log.Out("Respawn almost done");
		if (this.spawnPosition.IsUndef())
		{
			this.entityPlayerLocal.Respawn(this.respawnReason);
			return;
		}
		RaycastHit raycastHit;
		float num3;
		if (Physics.Raycast(new Ray(this.spawnPosition.position + Vector3.up - Origin.position, Vector3.down), out raycastHit, 3f, 1342242816))
		{
			num3 = raycastHit.point.y - this.spawnPosition.position.y + Origin.position.y;
		}
		else
		{
			num3 = this.gameManager.World.GetTerrainOffset(0, World.worldToBlockPos(this.spawnPosition.position)) + 0.05f;
		}
		this.gameManager.ClearTooltips(this.nguiWindowManager);
		this.spawnPosition.position.y = this.spawnPosition.position.y + num3;
		this.entityPlayerLocal.onGround = true;
		this.entityPlayerLocal.lastSpawnPosition = this.spawnPosition;
		this.entityPlayerLocal.Spawned = true;
		GameManager.Instance.PlayerSpawnedInWorld(null, this.respawnReason, new Vector3i(this.spawnPosition.position), this.entityPlayerLocal.entityId);
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendToClientsOrServer(NetPackageManager.GetPackage<NetPackagePlayerSpawnedInWorld>().Setup(this.respawnReason, new Vector3i(this.spawnPosition.position), this.entityPlayerLocal.entityId));
		if (this.respawnReason == RespawnType.Died || this.respawnReason == RespawnType.EnterMultiplayer || this.respawnReason == RespawnType.NewGame)
		{
			this.entityPlayerLocal.SetAlive();
		}
		else
		{
			this.entityPlayerLocal.bDead = false;
		}
		if (this.respawnReason == RespawnType.NewGame || this.respawnReason == RespawnType.LoadedGame || this.respawnReason == RespawnType.EnterMultiplayer || this.respawnReason == RespawnType.JoinMultiplayer)
		{
			this.entityPlayerLocal.TryAddRecoveryPosition(Vector3i.FromVector3Rounded(this.spawnPosition.position));
		}
		this.entityPlayerLocal.ResetLastTickPos(this.spawnPosition.position);
		if (!this.entityPlayerLocal.AttachedToEntity)
		{
			this.entityPlayerLocal.transform.position = this.spawnPosition.position - Origin.position;
		}
		else
		{
			this.spawnPosition.position.y = this.spawnPosition.position.y + 2f;
		}
		entity.SetPosition(this.spawnPosition.position, true);
		entity.SetRotation(new Vector3(0f, this.spawnPosition.heading, 0f));
		this.entityPlayerLocal.JetpackWearing = false;
		this.entityPlayerLocal.ParachuteWearing = false;
		this.entityPlayerLocal.AfterPlayerRespawn(this.respawnReason);
		if (flag)
		{
			this.entityPlayerLocal.QuestJournal.RemoveAllSharedQuests();
			this.entityPlayerLocal.QuestJournal.StartQuests();
		}
		if ((this.respawnReason == RespawnType.NewGame || this.respawnReason == RespawnType.EnterMultiplayer) && !GameManager.Instance.World.IsEditor() && !(GameMode.GetGameModeForId(GameStats.GetInt(EnumGameStats.GameModeId)) is GameModeCreative) && !GameUtils.IsPlaytesting() && !GameManager.bRecordNextSession && !GameManager.bPlayRecordedSession)
		{
			GameEventManager.Current.HandleAction("game_first_spawn", this.entityPlayerLocal, this.entityPlayerLocal, false, "", "", false, true, "", null);
		}
		if (this.respawnReason != RespawnType.Died && this.respawnReason != RespawnType.Teleport && GameStats.GetBool(EnumGameStats.AutoParty) && this.entityPlayerLocal.Party == null)
		{
			if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackagePartyActions>().Setup(NetPackagePartyActions.PartyActions.JoinAutoParty, this.entityPlayerLocal.entityId, this.entityPlayerLocal.entityId, null, null), false);
			}
			else
			{
				Party.ServerHandleAutoJoinParty(this.entityPlayerLocal);
			}
		}
		if (this.respawnReason == RespawnType.JoinMultiplayer || this.respawnReason == RespawnType.LoadedGame)
		{
			this.entityPlayerLocal.ReassignEquipmentTransforms();
			GameEventManager.Current.HandleAction("game_on_spawn", this.entityPlayerLocal, this.entityPlayerLocal, false, "", "", false, true, "", null);
		}
		this.entityPlayerLocal.EnableCamera(true);
		GameManager.Instance.World.RefreshEntitiesOnMap();
		LocalPlayerUI.primaryUI.windowManager.Close(XUiC_LoadingScreen.ID);
		LocalPlayerUI.primaryUI.windowManager.Close("eacWarning");
		LocalPlayerUI.primaryUI.windowManager.Close("crossplayWarning");
		if (flag && PlatformManager.NativePlatform.GameplayNotifier != null)
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				bool isOnlineMultiplayer = SingletonMonoBehaviour<ConnectionManager>.Instance.CurrentMode == ProtocolManager.NetworkType.Server;
				bool allowsCrossplay = SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo.AllowsCrossplay;
				PlatformManager.NativePlatform.GameplayNotifier.GameplayStart(isOnlineMultiplayer, allowsCrossplay);
			}
			else if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
			{
				bool allowsCrossplay2 = SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo.AllowsCrossplay;
				PlatformManager.NativePlatform.GameplayNotifier.GameplayStart(true, allowsCrossplay2);
			}
		}
		if (this.respawnReason == RespawnType.Died)
		{
			this.entityPlayerLocal.QuestJournal.FailAllActivatedQuests();
			this.entityPlayerLocal.Progression.OnRespawnFromDeath();
			switch (GameStats.GetInt(EnumGameStats.DeathPenalty))
			{
			case 0:
				GameEventManager.Current.HandleAction("game_on_respawn_none", this.entityPlayerLocal, this.entityPlayerLocal, false, "", "", false, true, "", null);
				break;
			case 1:
				GameEventManager.Current.HandleAction("game_on_respawn_default", this.entityPlayerLocal, this.entityPlayerLocal, false, "", "", false, true, "", null);
				break;
			case 2:
				GameEventManager.Current.HandleAction("game_on_respawn_injured", this.entityPlayerLocal, this.entityPlayerLocal, false, "", "", false, true, "", null);
				break;
			case 3:
				GameEventManager.Current.HandleAction("game_on_respawn_permanent", this.entityPlayerLocal, this.entityPlayerLocal, false, "", "", false, true, "", null);
				break;
			}
		}
		if (!this.gameManager.IsEditMode() && (this.respawnReason == RespawnType.NewGame || this.respawnReason == RespawnType.EnterMultiplayer))
		{
			this.windowManager.TempHUDDisable();
			this.entityPlayerLocal.SetControllable(false);
			this.entityPlayerLocal.bIntroAnimActive = true;
			GameManager.Instance.StartCoroutine(this.showUILater());
			if (!GameUtils.IsPlaytesting())
			{
				GameManager.Instance.StartCoroutine(this.initializeHoldingItemLater(4f));
			}
		}
		else
		{
			this.entityPlayerLocal.SetControllable(true);
			if (!this.gameManager.IsEditMode() && !GameUtils.IsPlaytesting() && (this.respawnReason == RespawnType.LoadedGame || this.respawnReason == RespawnType.JoinMultiplayer))
			{
				GameManager.Instance.StartCoroutine(this.initializeHoldingItemLater(0.1f));
			}
		}
		this.bLastRespawnActive = false;
	}

	public IEnumerator UnstuckPlayerCo()
	{
		if (!this.entityPlayerLocal.Spawned || this.unstuckCoState == ERoutineState.Running)
		{
			yield break;
		}
		this.unstuckCoState = ERoutineState.Running;
		SpawnPosition spawnTarget = new SpawnPosition(this.entityPlayerLocal.GetPosition(), this.entityPlayerLocal.rotation.y);
		GameManager.ShowTooltip(this.entityPlayerLocal, string.Format(Localization.Get("xuiMenuUnstuckTooltip", false), 5), true);
		DateTime currentTime = DateTime.Now;
		yield return this.FindRespawnSpawnPointRoutine(SpawnMethod.Unstuck, spawnTarget);
		if (this.unstuckCoState == ERoutineState.Cancelled)
		{
			GameManager.ShowTooltip(this.entityPlayerLocal, Localization.Get("xuiMenuUnstuckCancelled", false), true);
			yield break;
		}
		double remainingTime = 5.0 - (DateTime.Now - currentTime).TotalSeconds;
		while (remainingTime > 0.0)
		{
			GameManager.ShowTooltip(this.entityPlayerLocal, string.Format(Localization.Get("xuiMenuUnstuckTooltip", false), (int)(remainingTime + 0.5)), true);
			yield return new WaitForSeconds(Math.Min(1f, (float)remainingTime));
			remainingTime -= 1.0;
			if (this.unstuckCoState == ERoutineState.Cancelled)
			{
				GameManager.ShowTooltip(this.entityPlayerLocal, Localization.Get("xuiMenuUnstuckCancelled", false), true);
				yield break;
			}
		}
		if (!this.waitingForSpawnPointSelection && !this.spawnPosition.IsUndef())
		{
			this.entityPlayerLocal.TeleportToPosition(this.spawnPosition.position, false, null);
		}
		this.unstuckCoState = ERoutineState.Succeeded;
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public IEnumerator initializeHoldingItemLater(float _time)
	{
		yield return new WaitForSeconds(_time);
		if (this.entityPlayerLocal != null && this.entityPlayerLocal.inventory != null)
		{
			this.entityPlayerLocal.inventory.ForceHoldingItemUpdate();
		}
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public IEnumerator showUILater()
	{
		yield return new WaitForSeconds(4f);
		if (this.entityPlayerLocal != null && this.entityPlayerLocal.transform != null)
		{
			this.entityPlayerLocal.bIntroAnimActive = false;
			this.entityPlayerLocal.SetControllable(true);
		}
		if (this.windowManager != null)
		{
			this.windowManager.ReEnableHUD();
		}
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void openSpawnWindow(RespawnType _respawnReason)
	{
		Log.Out("OpenSpawnWindow");
		XUiC_ProgressWindow.Close(LocalPlayerUI.primaryUI);
		if (!this.gameManager.IsEditMode())
		{
			LocalPlayerUI.primaryUI.windowManager.Open(XUiC_LoadingScreen.ID, false, true, true);
		}
		XUiC_SpawnSelectionWindow.Open(LocalPlayerUI.primaryUI, _respawnReason != RespawnType.EnterMultiplayer && _respawnReason != RespawnType.JoinMultiplayer && _respawnReason != RespawnType.NewGame && _respawnReason != RespawnType.Teleport && _respawnReason != RespawnType.LoadedGame, false);
		this.spawnWindowOpened = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public SpawnPosition findSpawnPosition(SpawnMethod _spawnMethod, SpawnPosition _spawnTarget)
	{
		SpawnPosition spawnPosition = SpawnPosition.Undef;
		if (_spawnMethod == SpawnMethod.OnBedRoll && spawnPosition.IsUndef())
		{
			spawnPosition = _spawnTarget;
			if (!spawnPosition.IsUndef())
			{
				string str = "1. Spawn pos: sleeping bag ";
				SpawnPosition spawnPosition2 = spawnPosition;
				Log.Out(str + spawnPosition2.ToString());
			}
		}
		Vector3 position;
		if (_spawnMethod == SpawnMethod.NearBedroll && spawnPosition.IsUndef() && !_spawnTarget.IsUndef() && this.gameManager.World.GetRandomSpawnPositionMinMaxToPosition(_spawnTarget.position, 48, 96, 48, false, out position, true, true, 50))
		{
			spawnPosition.position = position;
			string str2 = "2. Spawn pos: random near bedroll ";
			SpawnPosition spawnPosition2 = spawnPosition;
			Log.Out(str2 + spawnPosition2.ToString());
		}
		if (_spawnMethod == SpawnMethod.NearBackpack && spawnPosition.IsUndef() && !_spawnTarget.IsUndef())
		{
			Vector3 position2;
			if (this.gameManager.World.GetRandomSpawnPositionMinMaxToPosition(_spawnTarget.position, 48, 96, 48, false, out position2, true, true, 50))
			{
				spawnPosition.position = position2;
				string str3 = "3. Spawn pos: random near backpack ";
				SpawnPosition spawnPosition2 = spawnPosition;
				Log.Out(str3 + spawnPosition2.ToString());
			}
			if (spawnPosition.IsUndef() && this.entityPlayerLocal.recoveryPositions.Count > 0)
			{
				for (int i = this.entityPlayerLocal.recoveryPositions.Count - 1; i >= 0; i--)
				{
					if (Vector3.Distance(this.entityPlayerLocal.recoveryPositions[i], _spawnTarget.position) > 48f)
					{
						spawnPosition.position = this.entityPlayerLocal.recoveryPositions[i];
						string str4 = "4. Spawn pos: Recovery Point ";
						SpawnPosition spawnPosition2 = spawnPosition;
						Log.Out(str4 + spawnPosition2.ToString());
						break;
					}
				}
			}
		}
		if (_spawnMethod == SpawnMethod.Unstuck && spawnPosition.IsUndef() && !_spawnTarget.IsUndef())
		{
			Vector3 position3;
			if (this.gameManager.World.GetRandomSpawnPositionMinMaxToPosition(_spawnTarget.position, 0, 16, 0, false, out position3, true, true, 100))
			{
				spawnPosition.position = position3;
				string str5 = "5. Spawn pos: try 'unstuck' player ";
				SpawnPosition spawnPosition2 = spawnPosition;
				Log.Out(str5 + spawnPosition2.ToString());
			}
			if (spawnPosition.IsUndef() && this.entityPlayerLocal.recoveryPositions.Count >= 2)
			{
				spawnPosition.position = this.entityPlayerLocal.recoveryPositions[this.entityPlayerLocal.recoveryPositions.Count - 2];
				string str6 = "6. Spawn pos: try 'unstuck' player at Recovery Point ";
				SpawnPosition spawnPosition2 = spawnPosition;
				Log.Out(str6 + spawnPosition2.ToString());
			}
		}
		if (spawnPosition.IsUndef())
		{
			if (!_spawnTarget.IsUndef())
			{
				spawnPosition = this.gameManager.GetSpawnPointList().GetRandomSpawnPosition(this.entityPlayerLocal.world, new Vector3?(_spawnTarget.position), 300, 600);
				string str7 = "7. Spawn pos: start point ";
				SpawnPosition spawnPosition2 = spawnPosition;
				Log.Out(str7 + spawnPosition2.ToString() + " distance to backpack: " + (spawnPosition.position - _spawnTarget.position).magnitude.ToCultureInvariantString());
			}
			else
			{
				spawnPosition = this.gameManager.GetSpawnPointList().GetRandomSpawnPosition(this.entityPlayerLocal.world, new Vector3?(this.entityPlayerLocal.position), 300, 600);
				string str8 = "7. Spawn pos: start point ";
				SpawnPosition spawnPosition2 = spawnPosition;
				Log.Out(str8 + spawnPosition2.ToString());
			}
		}
		if (spawnPosition.IsUndef())
		{
			int x = Utils.Fastfloor(this.entityPlayerLocal.position.x);
			int y = Utils.Fastfloor(this.entityPlayerLocal.position.y);
			int z = Utils.Fastfloor(this.entityPlayerLocal.position.z);
			IChunk chunkFromWorldPos = this.gameManager.World.GetChunkFromWorldPos(x, y, z);
			if (chunkFromWorldPos != null)
			{
				if (this.entityPlayerLocal.position.y == Constants.cStartPositionPlayerInLevel.y)
				{
					this.entityPlayerLocal.position.y = (float)(chunkFromWorldPos.GetHeight(ChunkBlockLayerLegacy.CalcOffset(x, z)) + 1);
				}
				spawnPosition = new SpawnPosition(this.entityPlayerLocal.GetPosition(), this.entityPlayerLocal.rotation.y);
				string str9 = "8. Spawn pos: current player pos ";
				SpawnPosition spawnPosition2 = spawnPosition;
				Log.Out(str9 + spawnPosition2.ToString());
			}
		}
		return spawnPosition;
	}

	public IEnumerator FindRespawnSpawnPointRoutine(SpawnMethod _method, SpawnPosition _spawnTarget)
	{
		this.waitingForSpawnPointSelection = true;
		Vector3 targetPosition = this.entityPlayerLocal.position;
		if (!_spawnTarget.IsUndef())
		{
			targetPosition = _spawnTarget.position;
		}
		this.entityPlayerLocal.SetPosition(targetPosition, true);
		yield return new WaitForSeconds(2f);
		float waitTime = 0f;
		while (!GameManager.Instance.World.IsChunkAreaLoaded(targetPosition) && waitTime < 5f)
		{
			yield return new WaitForSeconds(0.25f);
			waitTime += 0.25f;
		}
		this.spawnPosition = this.findSpawnPosition(_method, _spawnTarget);
		this.waitingForSpawnPointSelection = false;
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void stopMoving()
	{
		this.entityPlayerLocal.movementInput.moveForward = 0f;
		this.entityPlayerLocal.movementInput.moveStrafe = 0f;
		this.entityPlayerLocal.MoveByInput();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateDebugKeys()
	{
		if (this.windowManager.IsModalWindowOpen())
		{
			return;
		}
		if (!GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled))
		{
			return;
		}
		bool flag = this.playerUI.windowManager.IsInputActive();
		bool flag2 = flag || this.wasUIInputActive;
		this.wasUIInputActive = flag;
		if (flag2)
		{
			return;
		}
		if (Input.GetKeyDown(KeyCode.Keypad0))
		{
			Manager.PlayButtonClick();
			if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageDebug>().Setup(NetPackageDebug.Type.AINameInfoServerToggle, -1, null), false);
			}
			else
			{
				bool flag3 = !GamePrefs.GetBool(EnumGamePrefs.DebugMenuShowTasks);
				GamePrefs.Set(EnumGamePrefs.DebugMenuShowTasks, flag3);
				EntityAlive.SetupAllDebugNameHUDs(flag3);
			}
		}
		if (this.gameManager.World.IsRemote())
		{
			return;
		}
		bool shiftKeyPressed = InputUtils.ShiftKeyPressed;
		if (!shiftKeyPressed)
		{
			float num = Time.timeScale;
			if (Input.GetKeyDown(KeyCode.KeypadEnter))
			{
				if (num > 0f)
				{
					num = 0f;
				}
				else
				{
					num = 1f;
				}
			}
			if (Input.GetKeyDown(KeyCode.KeypadMinus))
			{
				num = Mathf.Max(num - 0.05f, 0f);
			}
			if (Input.GetKeyDown(KeyCode.KeypadPlus))
			{
				num = Mathf.Min(num + 0.05f, 2f);
			}
			if (num != Time.timeScale)
			{
				Time.timeScale = num;
				Log.Out("Time scale {0}", new object[]
				{
					num.ToCultureInvariantString()
				});
				Manager.PlayButtonClick();
			}
			if (Input.GetKeyDown(KeyCode.KeypadPeriod))
			{
				if (InputUtils.ControlKeyPressed)
				{
					if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
					{
						SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync("killall all", null);
					}
					else
					{
						SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageConsoleCmdServer>().Setup("killall all"), false);
					}
				}
				else
				{
					float d = (float)(InputUtils.AltKeyPressed ? 100 : 0);
					this.entityPlayerLocal.emodel.DoRagdoll(2f, EnumBodyPartHit.Torso, this.entityPlayerLocal.rand.RandomInsideUnitSphere * d, this.entityPlayerLocal.transform.position + this.entityPlayerLocal.rand.RandomInsideUnitSphere * 0.1f + Origin.position, false);
				}
			}
		}
		else if (Input.GetKeyDown(KeyCode.KeypadPlus))
		{
			this.drawChunkMode = (this.drawChunkMode + 1) % 3;
		}
		if (this.playerInput.AiFreeze.WasPressed && !GameManager.Instance.IsEditMode())
		{
			Manager.PlayButtonClick();
			if (InputUtils.ControlKeyPressed)
			{
				EAIManager.ToggleAnimFreeze();
				if (EAIManager.isAnimFreeze)
				{
					this.entityPlayerLocal.Buffs.AddBuff("buffShowAnimationDisabled", -1, true, false, -1f);
					return;
				}
				this.entityPlayerLocal.Buffs.RemoveBuff("buffShowAnimationDisabled", true);
				return;
			}
			else
			{
				if (shiftKeyPressed)
				{
					this.entityPlayerLocal.SetIgnoredByAI(!this.entityPlayerLocal.IsIgnoredByAI());
					return;
				}
				bool flag4 = !GamePrefs.GetBool(EnumGamePrefs.DebugStopEnemiesMoving);
				GamePrefs.Set(EnumGamePrefs.DebugStopEnemiesMoving, flag4);
				if (flag4)
				{
					this.entityPlayerLocal.Buffs.AddBuff("buffShowAIDisabled", -1, true, false, -1f);
					return;
				}
				this.entityPlayerLocal.Buffs.RemoveBuff("buffShowAIDisabled", true);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Update()
	{
		bool flag = !this.playerUI.windowManager.IsCursorWindowOpen() && !this.playerUI.windowManager.IsModalWindowOpen() && (this.playerInput.Enabled || this.playerInput.VehicleActions.Enabled);
		if (DroneManager.Debug_LocalControl)
		{
			flag = false;
		}
		if (this.playerAutoPilotControllor != null && this.playerAutoPilotControllor.IsEnabled())
		{
			this.playerAutoPilotControllor.Update();
		}
		if ((!this.bCanControlOverride || !flag) && GamePrefs.GetInt(EnumGamePrefs.SelectionOperationMode) == 0)
		{
			XUiC_InteractionPrompt.SetText(this.playerUI, null);
			this.strTextLabelPointingTo = string.Empty;
		}
		if (!this.gameManager.gameStateManager.IsGameStarted() || GameStats.GetInt(EnumGameStats.GameState) != 1)
		{
			this.stopMoving();
			return;
		}
		if (this.entityPlayerLocal.PlayerUI.windowManager.IsModalWindowOpen())
		{
			if (!this.IsGUICancelPressed && this.playerInput.PermanentActions.Cancel.WasPressed)
			{
				this.IsGUICancelPressed = true;
			}
		}
		else if (this.IsGUICancelPressed)
		{
			this.IsGUICancelPressed = this.playerInput.PermanentActions.Cancel.GetBindingOfType(this.playerInput.ActiveDevice.DeviceClass == InputDeviceClass.Controller).GetState(this.playerInput.ActiveDevice);
		}
		this.updateRespawn();
		if (this.unstuckCoState == ERoutineState.Running)
		{
			this.stopMoving();
			return;
		}
		this.updateDebugKeys();
		if (this.drawChunkMode > 0)
		{
			this.DrawChunkBoundary();
			if (this.drawChunkMode == 2)
			{
				this.DrawChunkDensities();
			}
		}
		if (this.entityPlayerLocal.emodel.IsRagdollActive)
		{
			this.stopMoving();
			return;
		}
		if (this.entityPlayerLocal.IsDead())
		{
			XUiC_InteractionPrompt.SetText(this.playerUI, null);
			this.strTextLabelPointingTo = string.Empty;
			return;
		}
		bool flag2 = false;
		float num = this.playerInput.Scroll.Value;
		if (this.playerInput.LastInputType == BindingSourceType.DeviceBindingSource)
		{
			if (!this.entityPlayerLocal.AimingGun)
			{
				num = 0f;
			}
			else
			{
				num *= 0.25f;
			}
		}
		num *= 0.25f;
		if (Mathf.Abs(num) < 0.001f)
		{
			num = 0f;
		}
		this.gameManager.GetActiveBlockTool().CheckKeys(this.entityPlayerLocal.inventory.holdingItemData, this.entityPlayerLocal.HitInfo, this.playerInput);
		if (this.gameManager.IsEditMode() || BlockToolSelection.Instance.SelectionActive)
		{
			SelectionBoxManager.Instance.CheckKeys(this.gameManager, this.playerInput, this.entityPlayerLocal.HitInfo);
			if (!flag2)
			{
				flag2 = SelectionBoxManager.Instance.ConsumeScrollWheel(num, this.playerInput);
			}
			flag2 = this.gameManager.GetActiveBlockTool().ConsumeScrollWheel(this.entityPlayerLocal.inventory.holdingItemData, num, this.playerInput);
		}
		if ((!this.bCanControlOverride || !flag) && GamePrefs.GetInt(EnumGamePrefs.SelectionOperationMode) == 0)
		{
			this.stopMoving();
			return;
		}
		this.entityPlayerLocal.movementInput.lastInputController = (this.playerInput.LastInputType == BindingSourceType.DeviceBindingSource);
		if (!this.IsGUICancelPressed && (!this.gameManager.IsEditMode() || GamePrefs.GetInt(EnumGamePrefs.SelectionOperationMode) == 0))
		{
			bool controlKeyPressed = InputUtils.ControlKeyPressed;
			PlayerAction playerAction = this.playerInput.VehicleActions.Enabled ? this.playerInput.VehicleActions.Turbo : this.playerInput.Run;
			PlayerAction playerAction2 = this.playerInput.VehicleActions.Enabled ? this.playerInput.VehicleActions.MoveForward : this.playerInput.MoveForward;
			if (playerAction.WasPressed)
			{
				this.runInputTime = 0f;
				this.entityPlayerLocal.movementInput.running = true;
				this.entityPlayerLocal.AimingGun = false;
				this.runPressedWhileActive = true;
			}
			else if (playerAction.WasReleased && this.runPressedWhileActive)
			{
				if (this.runInputTime > 0.2f)
				{
					this.entityPlayerLocal.movementInput.running = false;
					this.runToggleActive = false;
				}
				else if (this.runToggleActive)
				{
					this.runToggleActive = false;
					this.entityPlayerLocal.movementInput.running = false;
				}
				else if (playerAction2.IsPressed || this.sprintLockEnabled)
				{
					this.runToggleActive = true;
				}
				else
				{
					this.runToggleActive = false;
					this.entityPlayerLocal.movementInput.running = false;
				}
				this.runPressedWhileActive = false;
			}
			if (playerAction.IsPressed)
			{
				this.runInputTime += Time.deltaTime;
			}
			if (this.runToggleActive)
			{
				if (this.entityPlayerLocal.Stamina <= 0f && !this.sprintLockEnabled)
				{
					this.runToggleActive = false;
					this.runPressedWhileActive = false;
					this.entityPlayerLocal.movementInput.running = false;
				}
				else if (playerAction2.WasReleased && !this.sprintLockEnabled)
				{
					this.entityPlayerLocal.movementInput.running = false;
					this.runToggleActive = false;
					this.runPressedWhileActive = false;
				}
				else
				{
					this.entityPlayerLocal.movementInput.running = true;
				}
			}
			this.entityPlayerLocal.movementInput.down = (this.playerInput.Crouch.IsPressed && (!this.gameManager.IsEditMode() || !controlKeyPressed));
			this.entityPlayerLocal.movementInput.jump = this.playerInput.Jump.IsPressed;
			if (this.entityPlayerLocal.movementInput.running && this.entityPlayerLocal.AimingGun)
			{
				this.entityPlayerLocal.AimingGun = false;
			}
		}
		else
		{
			this.runToggleActive = false;
			this.entityPlayerLocal.movementInput.running = false;
			this.runPressedWhileActive = false;
		}
		this.entityPlayerLocal.movementInput.downToggle = (!this.gameManager.IsEditMode() && !this.entityPlayerLocal.IsFlyMode.Value && this.playerInput.ToggleCrouch.WasPressed);
		if (GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled) && this.playerInput.PermanentActions.DebugControllerLeft.IsPressed && this.playerInput.PermanentActions.DebugControllerRight.IsPressed)
		{
			if (this.playerInput.GodAlternate.WasPressed)
			{
				this.toggleGodMode();
			}
			if (this.playerInput.TeleportAlternate.WasPressed)
			{
				this.teleportPlayer();
			}
		}
		if (this.playerInput.DecSpeed.WasPressed)
		{
			this.entityPlayerLocal.GodModeSpeedModifier = Utils.FastMax(0.1f, this.entityPlayerLocal.GodModeSpeedModifier - 0.1f);
		}
		if (this.playerInput.IncSpeed.WasPressed)
		{
			this.entityPlayerLocal.GodModeSpeedModifier = Utils.FastMin(3f, this.entityPlayerLocal.GodModeSpeedModifier + 0.1f);
		}
		Vector2 vector;
		Vector2 vector2;
		if (this.playerInput.Look.LastInputType != BindingSourceType.MouseBindingSource)
		{
			this.entityPlayerLocal.movementInput.down = (this.entityPlayerLocal.IsFlyMode.Value && this.playerInput.ToggleCrouch.IsPressed);
			float magnitude;
			if (this.playerInput.VehicleActions.Enabled)
			{
				vector.x = this.playerInput.VehicleActions.Look.X;
				vector.y = this.playerInput.VehicleActions.Look.Y * (float)this.invertController;
				magnitude = this.playerInput.VehicleActions.Look.Vector.magnitude;
			}
			else
			{
				vector.x = this.playerInput.Look.X;
				vector.y = this.playerInput.Look.Y * (float)this.invertController;
				magnitude = this.playerInput.Look.Vector.magnitude;
			}
			if (this.lookAccelerationRate <= 0f)
			{
				this.currentLookAcceleration = 1f;
			}
			else if (magnitude > 0f)
			{
				this.currentLookAcceleration = Mathf.Clamp(this.currentLookAcceleration + this.lookAccelerationRate * magnitude * Time.unscaledDeltaTime, 0f, magnitude);
			}
			else
			{
				this.currentLookAcceleration = 0f;
			}
			Vector2 a = this.controllerLookSensitivity;
			if (this.entityPlayerLocal.AimingGun)
			{
				a *= this.controllerZoomSensitivity;
			}
			else if (this.playerInput.VehicleActions.Enabled)
			{
				a *= this.controllerVehicleSensitivity;
			}
			vector2 = a * this.lookAccelerationCurve.Evaluate(this.currentLookAcceleration);
			if (this.entityPlayerLocal.AimingGun)
			{
				float d = Mathf.Lerp(0.2f, 1f, (this.entityPlayerLocal.playerCamera.fieldOfView - 10f) / ((float)Constants.cDefaultCameraFieldOfView - 10f));
				vector2 *= d;
			}
			if (this.entityPlayerLocal.AttachedToEntity != null)
			{
				this.aimAssistSlowAmount = 1f;
			}
			else
			{
				bool flag3 = false;
				WorldRayHitInfo hitInfo = this.entityPlayerLocal.HitInfo;
				if (hitInfo.bHitValid)
				{
					if (hitInfo.transform)
					{
						Transform hitRootTransform = GameUtils.GetHitRootTransform(hitInfo.tag, hitInfo.transform);
						if (hitRootTransform != null)
						{
							EntityAlive entityAlive;
							EntityItem entityItem;
							if (hitRootTransform.TryGetComponent<EntityAlive>(out entityAlive) && entityAlive.IsAlive() && entityAlive.IsValidAimAssistSlowdownTarget && hitInfo.hit.distanceSq <= 50f && (this.entityPlayerLocal.inventory.holdingItem.Actions[0] is ItemActionAttack || this.entityPlayerLocal.inventory.holdingItem.Actions[0] is ItemActionDynamicMelee))
							{
								this.bAimAssistTargetingItem = false;
								flag3 = true;
							}
							else if ((hitInfo.tag.StartsWith("Item", StringComparison.Ordinal) || hitRootTransform.TryGetComponent<EntityItem>(out entityItem)) && hitInfo.hit.distanceSq <= 10f)
							{
								this.bAimAssistTargetingItem = true;
								flag3 = true;
							}
						}
					}
					else if (this.entityPlayerLocal.ThreatLevel.Numeric < 0.75f && GameUtils.IsBlockOrTerrain(hitInfo.tag) && this.entityPlayerLocal.PlayerUI.windowManager.IsWindowOpen("interactionPrompt"))
					{
						BlockValue blockValue = hitInfo.hit.blockValue;
						if (!blockValue.Block.isMultiBlock && !blockValue.Block.isOversized && blockValue.Block.shape is BlockShapeModelEntity)
						{
							this.bAimAssistTargetingItem = true;
							flag3 = true;
						}
					}
				}
				if (flag3)
				{
					this.aimAssistSlowAmount = (this.bAimAssistTargetingItem ? 0.6f : 0.5f);
				}
				else
				{
					this.aimAssistSlowAmount = Mathf.MoveTowards(this.aimAssistSlowAmount, 1f, Time.unscaledDeltaTime * 5f);
				}
				vector2 *= this.aimAssistSlowAmount;
				if (this.controllerAimAssistsEnabled && this.cameraSnapTargetEntity != null && this.cameraSnapTargetEntity.IsAlive() && Time.time - this.cameraSnapTime < 0.3f)
				{
					Vector2 b = Vector2.one * 0.5f;
					Vector2 vector3 = (this.snapTargetingHead ? this.entityPlayerLocal.playerCamera.WorldToViewportPoint(this.cameraSnapTargetEntity.emodel.GetHeadTransform().position) : this.entityPlayerLocal.playerCamera.WorldToViewportPoint(this.cameraSnapTargetEntity.GetChestTransformPosition())) - b;
					float d2 = (this.cameraSnapMode == eCameraSnapMode.MeleeAttack) ? 1.5f : 1f;
					vector += vector3.normalized * d2 * vector3.magnitude / 0.15f;
				}
			}
		}
		else
		{
			vector2 = this.mouseLookSensitivity;
			Vector2 a2 = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y") * (float)this.invertMouse);
			vector = (a2 + this.previousMouseInput) / 2f;
			this.previousMouseInput = a2;
			if (this.playerInput.VehicleActions.Enabled)
			{
				vector2 *= this.vehicleLookSensitivity;
			}
			else
			{
				float magnitude2 = vector.magnitude;
				float num2 = 1f;
				if (this.entityPlayerLocal.AimingGun && magnitude2 > 0f)
				{
					vector2 *= this.mouseZoomSensitivity;
					float num3 = Mathf.Pow(magnitude2 * 0.4f, 2.5f) / magnitude2;
					num2 += num3 * this.zoomAccel;
					num2 *= Mathf.Lerp(0.2f, 1f, (this.entityPlayerLocal.playerCamera.fieldOfView - 10f) / ((float)Constants.cDefaultCameraFieldOfView - 10f));
					vector2 *= num2;
					if (vector2.magnitude > this.mouseLookSensitivity.magnitude)
					{
						vector2 = this.mouseLookSensitivity;
					}
				}
			}
			if (this.skipMouseLookNextFrame > 0 && (vector.x <= -1f || vector.x >= 1f || vector.y <= -1f || vector.y >= 1f))
			{
				this.skipMouseLookNextFrame--;
				vector = Vector2.zero;
			}
		}
		MovementInput movementInput = this.entityPlayerLocal.movementInput;
		if (!movementInput.bDetachedCameraMove)
		{
			PlayerActionsLocal playerInput = this.playerInput;
			if (this.playerAutoPilotControllor != null && this.playerAutoPilotControllor.IsEnabled())
			{
				movementInput.moveForward = this.playerAutoPilotControllor.GetForwardMovement();
			}
			else
			{
				movementInput.moveForward = playerInput.Move.Y;
			}
			movementInput.moveStrafe = playerInput.Move.X;
			if (movementInput.bCameraPositionLocked)
			{
				vector = Vector2.zero;
			}
			if (PlayerMoveController.useScaledMouseLook && !this.entityPlayerLocal.movementInput.lastInputController)
			{
				MovementInput movementInput2 = movementInput;
				movementInput2.rotation.x = movementInput2.rotation.x + vector.y * vector2.y * Time.unscaledDeltaTime * PlayerMoveController.mouseDeltaTimeScale;
				MovementInput movementInput3 = movementInput;
				movementInput3.rotation.y = movementInput3.rotation.y + vector.x * vector2.x * Time.unscaledDeltaTime * PlayerMoveController.mouseDeltaTimeScale;
			}
			else if (this.entityPlayerLocal.movementInput.lastInputController)
			{
				MovementInput movementInput4 = movementInput;
				movementInput4.rotation.x = movementInput4.rotation.x + vector.y * vector2.y * Time.unscaledDeltaTime * PlayerMoveController.lookDeltaTimeScale;
				MovementInput movementInput5 = movementInput;
				movementInput5.rotation.y = movementInput5.rotation.y + vector.x * vector2.x * Time.unscaledDeltaTime * PlayerMoveController.lookDeltaTimeScale;
			}
			else
			{
				MovementInput movementInput6 = movementInput;
				movementInput6.rotation.x = movementInput6.rotation.x + vector.y * vector2.y;
				MovementInput movementInput7 = movementInput;
				movementInput7.rotation.y = movementInput7.rotation.y + vector.x * vector2.x;
			}
			bool value = this.entityPlayerLocal.IsGodMode.Value;
			movementInput.bCameraChange = (playerInput.CameraChange.IsPressed && !value && !playerInput.Primary.IsPressed && !playerInput.Secondary.IsPressed);
			if (movementInput.bCameraChange)
			{
				flag2 = true;
				if (this.entityPlayerLocal.bFirstPersonView)
				{
					if (num < 0f)
					{
						this.entityPlayerLocal.SwitchFirstPersonViewFromInput();
						this.wasCameraChangeUsedWithWheel = true;
					}
				}
				else
				{
					movementInput.cameraDistance = Utils.FastMin(movementInput.cameraDistance - 2f * num, 3f);
					if (movementInput.cameraDistance < -0.2f)
					{
						movementInput.cameraDistance = -0.2f;
						this.entityPlayerLocal.SwitchFirstPersonViewFromInput();
					}
					if (num != 0f)
					{
						this.wasCameraChangeUsedWithWheel = true;
					}
				}
			}
			if (playerInput.CameraChange.WasReleased && !value)
			{
				if (!this.wasCameraChangeUsedWithWheel && !playerInput.Primary.IsPressed && !playerInput.Secondary.IsPressed)
				{
					this.entityPlayerLocal.SwitchFirstPersonViewFromInput();
				}
				this.wasCameraChangeUsedWithWheel = false;
			}
			if ((this.gameManager.IsEditMode() || BlockToolSelection.Instance.SelectionActive) && (Input.GetKey(KeyCode.LeftControl) || GamePrefs.GetInt(EnumGamePrefs.SelectionOperationMode) != 0))
			{
				movementInput.Clear();
			}
			this.entityPlayerLocal.MoveByInput();
		}
		else
		{
			float num4 = 0.15f;
			if (this.entityPlayerLocal.movementInput.running)
			{
				num4 *= 3f;
			}
			else
			{
				num4 *= this.entityPlayerLocal.GodModeSpeedModifier;
			}
			if (this.playerInput.MoveForward.IsPressed)
			{
				this.entityPlayerLocal.cameraTransform.position += this.entityPlayerLocal.cameraTransform.forward * num4;
			}
			if (this.playerInput.MoveBack.IsPressed)
			{
				this.entityPlayerLocal.cameraTransform.position -= this.entityPlayerLocal.cameraTransform.forward * num4;
			}
			if (this.playerInput.MoveLeft.IsPressed)
			{
				this.entityPlayerLocal.cameraTransform.position -= this.entityPlayerLocal.cameraTransform.right * num4;
			}
			if (this.playerInput.MoveRight.IsPressed)
			{
				this.entityPlayerLocal.cameraTransform.position += this.entityPlayerLocal.cameraTransform.right * num4;
			}
			if (this.playerInput.Jump.IsPressed)
			{
				this.entityPlayerLocal.cameraTransform.position += Vector3.up * num4;
			}
			if (this.playerInput.Crouch.IsPressed)
			{
				this.entityPlayerLocal.cameraTransform.position -= Vector3.up * num4;
			}
			if (!movementInput.bCameraPositionLocked)
			{
				Vector3 localEulerAngles = this.entityPlayerLocal.cameraTransform.localEulerAngles;
				this.entityPlayerLocal.cameraTransform.localEulerAngles = new Vector3(localEulerAngles.x - vector.y, localEulerAngles.y + vector.x, localEulerAngles.z);
			}
		}
		bool flag4 = this.gameManager.IsEditMode() && this.playerInput.Run.IsPressed;
		Ray ray = this.entityPlayerLocal.GetLookRay();
		if (this.gameManager.IsEditMode() && GamePrefs.GetInt(EnumGamePrefs.SelectionOperationMode) == 4)
		{
			ray = this.entityPlayerLocal.cameraTransform.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
			ray.origin += Origin.position;
		}
		ray.origin += ray.direction.normalized * 0.1f;
		float num5 = Utils.FastMax(Utils.FastMax(Constants.cDigAndBuildDistance, Constants.cCollectItemDistance), 30f);
		RaycastHit raycastHit;
		bool flag5 = Physics.Raycast(new Ray(ray.origin - Origin.position, ray.direction), out raycastHit, num5, 73728);
		bool flag6 = false;
		if (flag5 && raycastHit.transform.CompareTag("E_BP_Body"))
		{
			flag6 = true;
		}
		if (flag5)
		{
			flag5 &= raycastHit.transform.CompareTag("Item");
		}
		int num6 = 69;
		bool flag7;
		if (!this.gameManager.IsEditMode())
		{
			flag7 = Voxel.Raycast(this.gameManager.World, ray, num5, -555528213, num6, 0f);
			if (flag7)
			{
				Transform hitRootTransform2 = GameUtils.GetHitRootTransform(Voxel.voxelRayHitInfo.tag, Voxel.voxelRayHitInfo.transform);
				Entity entity;
				EntityAlive entityAlive2 = (hitRootTransform2 != null && hitRootTransform2.TryGetComponent<Entity>(out entity)) ? (entity as EntityAlive) : null;
				if (entityAlive2 == null || !entityAlive2.IsDead())
				{
					flag7 = Voxel.Raycast(this.gameManager.World, ray, num5, -555266069, num6, 0f);
				}
			}
		}
		else
		{
			num6 |= 256;
			int num7 = -555266069;
			num7 |= 268435456;
			if (!GameManager.bVolumeBlocksEditing)
			{
				num7 = int.MinValue;
			}
			flag7 = Voxel.RaycastOnVoxels(this.gameManager.World, ray, num5, num7, num6, 0f);
			if (flag7 && !GameManager.bVolumeBlocksEditing)
			{
				Voxel.voxelRayHitInfo.lastBlockPos = Vector3i.zero;
				Voxel.voxelRayHitInfo.hit.voxelData.Clear();
				Voxel.voxelRayHitInfo.hit.blockPos = Vector3i.zero;
			}
		}
		WorldRayHitInfo hitInfo2 = this.entityPlayerLocal.HitInfo;
		Vector3i zero = Vector3i.zero;
		Vector3i vector3i = Vector3i.zero;
		if (flag7)
		{
			hitInfo2.CopyFrom(Voxel.voxelRayHitInfo);
			vector3i = hitInfo2.hit.blockPos;
			Vector3i lastBlockPos = hitInfo2.lastBlockPos;
			hitInfo2.bHitValid = true;
		}
		else
		{
			hitInfo2.bHitValid = false;
		}
		if (!hitInfo2.hit.blockValue.isair)
		{
			Block block = hitInfo2.hit.blockValue.Block;
			if (!block.IsCollideMovement || block.CanBlocksReplace)
			{
				hitInfo2.lastBlockPos = vector3i;
			}
		}
		float num8 = flag5 ? raycastHit.distance : 1000f;
		if (flag7 && GameUtils.IsBlockOrTerrain(hitInfo2.tag))
		{
			num8 -= 1.2f;
			if (num8 < 0f)
			{
				num8 = 0.1f;
			}
		}
		if (flag5 && (!flag7 || (flag7 && num8 * num8 <= hitInfo2.hit.distanceSq)))
		{
			hitInfo2.bHitValid = true;
			hitInfo2.tag = "Item";
			hitInfo2.transform = raycastHit.collider.transform;
			hitInfo2.hit.pos = raycastHit.point;
			hitInfo2.hit.blockPos = World.worldToBlockPos(hitInfo2.hit.pos);
			hitInfo2.hit.distanceSq = raycastHit.distance * raycastHit.distance;
		}
		if (flag6 && raycastHit.distance * raycastHit.distance <= hitInfo2.hit.distanceSq)
		{
			hitInfo2.bHitValid = true;
			hitInfo2.tag = "E_BP_Body";
			hitInfo2.transform = raycastHit.collider.transform;
			hitInfo2.hit.pos = raycastHit.point;
			hitInfo2.hit.blockPos = World.worldToBlockPos(hitInfo2.hit.pos);
			hitInfo2.hit.distanceSq = raycastHit.distance * raycastHit.distance;
		}
		bool flag8 = true;
		EntityCollisionRules entityCollisionRules;
		if (hitInfo2.hitCollider && hitInfo2.hitCollider.TryGetComponent<EntityCollisionRules>(out entityCollisionRules) && !entityCollisionRules.IsInteractable)
		{
			flag8 = false;
		}
		if (this.entityPlayerLocal.inventory != null && this.entityPlayerLocal.inventory.holdingItemData != null)
		{
			this.entityPlayerLocal.inventory.holdingItemData.hitInfo = this.entityPlayerLocal.HitInfo;
		}
		TileEntity tileEntity = null;
		EntityTurret entityTurret = null;
		bool flag9 = true;
		bool flag10 = true;
		bool flag11 = this.playerInput.Primary.IsPressed && this.bAllowPlayerInput && !this.IsGUICancelPressed;
		bool flag12 = this.playerInput.Secondary.IsPressed && this.bAllowPlayerInput && !this.IsGUICancelPressed;
		if (flag11 && GameManager.Instance.World.IsEditor())
		{
			if (this.bIgnoreLeftMouseUntilReleased)
			{
				flag11 = false;
			}
		}
		else
		{
			this.bIgnoreLeftMouseUntilReleased = false;
		}
		bool flag13 = false;
		ITileEntityLootable tileEntityLootable = null;
		EntityItem entityItem2 = null;
		BlockValue blockValue2 = BlockValue.Air;
		ProjectileMoveScript projectileMoveScript = null;
		ThrownWeaponMoveScript thrownWeaponMoveScript = null;
		string text = null;
		bool flag14 = GameManager.Instance.IsEditMode() && this.entityPlayerLocal.HitInfo.transform != null && this.entityPlayerLocal.HitInfo.transform.gameObject.layer == 28;
		Entity entity2 = null;
		if (this.entityPlayerLocal.AttachedToEntity == null && flag8)
		{
			if (hitInfo2.bHitValid && (flag14 |= GameUtils.IsBlockOrTerrain(hitInfo2.tag)))
			{
				int activationDistanceSq = hitInfo2.hit.blockValue.Block.GetActivationDistanceSq();
				if (hitInfo2.hit.distanceSq < (float)activationDistanceSq)
				{
					blockValue2 = hitInfo2.hit.blockValue;
					Block block2 = blockValue2.Block;
					BlockValue blockValue3 = blockValue2;
					Vector3i vector3i2 = vector3i;
					if (blockValue3.ischild && block2 != null && block2.multiBlockPos != null)
					{
						vector3i2 = block2.multiBlockPos.GetParentPos(vector3i2, blockValue3);
						blockValue3 = this.gameManager.World.GetBlock(hitInfo2.hit.clrIdx, vector3i2);
					}
					if (block2.HasBlockActivationCommands(this.gameManager.World, blockValue3, hitInfo2.hit.clrIdx, vector3i2, this.entityPlayerLocal))
					{
						text = block2.GetActivationText(this.gameManager.World, blockValue3, hitInfo2.hit.clrIdx, vector3i2, this.entityPlayerLocal);
						if (text != null)
						{
							string arg = this.playerInput.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null) + this.playerInput.PermanentActions.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null);
							text = string.Format(text, arg);
						}
						tileEntity = this.gameManager.World.GetTileEntity(hitInfo2.hit.clrIdx, vector3i);
					}
					else if (block2.DisplayInfo == Block.EnumDisplayInfo.Name)
					{
						text = block2.GetLocalizedBlockName();
					}
					else if (block2.DisplayInfo == Block.EnumDisplayInfo.Description)
					{
						text = Localization.Get(block2.DescriptionKey, false);
					}
					else if (block2.DisplayInfo == Block.EnumDisplayInfo.Custom)
					{
						text = block2.GetCustomDescription(vector3i2, blockValue2);
					}
					if (flag12 && InputUtils.ShiftKeyPressed && InputUtils.AltKeyPressed && this.gameManager.IsEditMode())
					{
						GUIWindowEditBlockValue guiwindowEditBlockValue = (GUIWindowEditBlockValue)this.windowManager.GetWindow(GUIWindowEditBlockValue.ID);
						if (guiwindowEditBlockValue != null)
						{
							guiwindowEditBlockValue.SetBlock(hitInfo2.hit.blockPos, hitInfo2.hit.blockFace);
							this.windowManager.Open(GUIWindowEditBlockValue.ID, true, false, true);
							flag9 = false;
						}
					}
					if (flag12 && InputUtils.ShiftKeyPressed && InputUtils.AltKeyPressed && this.gameManager.IsEditMode() && blockValue2.Block is BlockSpawnEntity)
					{
						this.windowManager.GetWindow<GUIWindowEditBlockSpawnEntity>(GUIWindowEditBlockSpawnEntity.ID).SetBlockValue(hitInfo2.hit.blockPos, blockValue2);
						this.windowManager.Open(GUIWindowEditBlockSpawnEntity.ID, true, false, true);
						flag9 = false;
					}
				}
			}
			else if (hitInfo2.bHitValid && hitInfo2.tag.Equals("Item") && hitInfo2.hit.distanceSq < Constants.cCollectItemDistance * Constants.cCollectItemDistance)
			{
				entityItem2 = hitInfo2.transform.GetComponent<EntityItem>();
				RootTransformRefEntity component;
				if (entityItem2 == null && (component = hitInfo2.transform.GetComponent<RootTransformRefEntity>()) != null && component.RootTransform != null)
				{
					entityItem2 = component.RootTransform.GetComponent<EntityItem>();
				}
				if (entityItem2 != null)
				{
					if (entityItem2.onGround && entityItem2.CanCollect())
					{
						string localizedItemName = ItemClass.GetForId(entityItem2.itemStack.itemValue.type).GetLocalizedItemName();
						string arg2 = this.playerInput.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null) + this.playerInput.PermanentActions.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null);
						if (entityItem2.itemStack.count > 1)
						{
							text = string.Format(Localization.Get("itemTooltipFocusedSeveral", false), arg2, localizedItemName, entityItem2.itemStack.count);
						}
						else
						{
							text = string.Format(Localization.Get("itemTooltipFocusedOne", false), arg2, localizedItemName);
						}
					}
				}
				else
				{
					projectileMoveScript = hitInfo2.transform.GetComponent<ProjectileMoveScript>();
					if (projectileMoveScript != null)
					{
						string localizedItemName2 = ItemClass.GetForId(projectileMoveScript.itemValueProjectile.type).GetLocalizedItemName();
						string arg3 = this.playerInput.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null) + this.playerInput.PermanentActions.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null);
						text = string.Format(Localization.Get("itemTooltipFocusedOne", false), arg3, localizedItemName2);
					}
					thrownWeaponMoveScript = hitInfo2.transform.GetComponent<ThrownWeaponMoveScript>();
					if (thrownWeaponMoveScript != null)
					{
						string localizedItemName3 = ItemClass.GetForId(thrownWeaponMoveScript.itemValueWeapon.type).GetLocalizedItemName();
						string arg4 = this.playerInput.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null) + this.playerInput.PermanentActions.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null);
						text = string.Format(Localization.Get("itemTooltipFocusedOne", false), arg4, localizedItemName3);
					}
				}
			}
			else if (hitInfo2.bHitValid && hitInfo2.tag.StartsWith("E_") && hitInfo2.hit.distanceSq < Constants.cCollectItemDistance * Constants.cCollectItemDistance)
			{
				Transform hitRootTransform3 = GameUtils.GetHitRootTransform(hitInfo2.tag, hitInfo2.transform);
				if (hitRootTransform3 != null && (entity2 = hitRootTransform3.GetComponent<Entity>()) != null)
				{
					if ((projectileMoveScript = hitRootTransform3.GetComponentInChildren<ProjectileMoveScript>()) != null)
					{
						if (!entity2.IsDead() && entity2 as EntityPlayer != null && (entity2 as EntityPlayer).inventory != null && (entity2 as EntityPlayer).inventory.holdingItem != null && (entity2 as EntityPlayer).inventory.holdingItem.HasAnyTags(PlayerMoveController.BowTag))
						{
							projectileMoveScript = null;
						}
						if (entity2.IsDead())
						{
							string localizedItemName4 = ItemClass.GetForId(projectileMoveScript.itemValueProjectile.type).GetLocalizedItemName();
							string arg5 = this.playerInput.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null) + this.playerInput.PermanentActions.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null);
							text = string.Format(Localization.Get("itemTooltipFocusedOne", false), arg5, localizedItemName4);
						}
					}
					else if ((thrownWeaponMoveScript = hitRootTransform3.GetComponentInChildren<ThrownWeaponMoveScript>()) != null)
					{
						if (!entity2.IsDead() && entity2 as EntityPlayer != null && (entity2 as EntityPlayer).inventory != null && (entity2 as EntityPlayer).inventory.holdingItem != null && (entity2 as EntityPlayer).inventory.holdingItem.HasAnyTags(PlayerMoveController.BowTag))
						{
							thrownWeaponMoveScript = null;
						}
						if (entity2.IsDead())
						{
							string localizedItemName5 = ItemClass.GetForId(thrownWeaponMoveScript.itemValueWeapon.type).GetLocalizedItemName();
							string arg6 = this.playerInput.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null) + this.playerInput.PermanentActions.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null);
							text = string.Format(Localization.Get("itemTooltipFocusedOne", false), arg6, localizedItemName5);
						}
					}
					else if (entity2 is EntityNPC && entity2.IsAlive())
					{
						tileEntity = (this.gameManager.World.GetTileEntity(entity2.entityId) as TileEntityTrader);
						if (tileEntity != null)
						{
							EntityTrader entityTrader = (EntityTrader)entity2;
							string arg7 = this.playerInput.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null) + this.playerInput.PermanentActions.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null);
							string arg8 = Localization.Get(entityTrader.EntityName, false);
							text = string.Format(Localization.Get("npcTooltipTalk", false), arg7, arg8);
							entityTrader.HandleClientQuests(this.entityPlayerLocal);
						}
						else
						{
							tileEntity = this.gameManager.World.GetTileEntity(entity2.entityId);
							if (tileEntity != null)
							{
								string arg9 = this.playerInput.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null) + this.playerInput.PermanentActions.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null);
								string arg10 = Localization.Get(((EntityNPC)entity2).EntityName, false);
								text = string.Format(Localization.Get("npcTooltipTalk", false), arg9, arg10);
								EntityDrone entityDrone = entity2 as EntityDrone;
								if (entityDrone && entityDrone.IsLocked() && !entityDrone.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
								{
									text = Localization.Get("ttLocked", false) + "\n" + text;
								}
							}
						}
					}
					else if (entity2 as EntityTurret != null)
					{
						entityTurret = (entity2 as EntityTurret);
						if (entityTurret.CanInteract(this.entityPlayerLocal.entityId))
						{
							string arg11 = this.playerInput.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null) + this.playerInput.PermanentActions.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null);
							string arg12 = Localization.Get(((EntityTurret)entity2).EntityName, false);
							text = string.Format(Localization.Get("turretPickUp", false), arg11, arg12);
						}
					}
					else if (!string.IsNullOrEmpty(entity2.GetLootList()))
					{
						tileEntityLootable = this.gameManager.World.GetTileEntity(entity2.entityId).GetSelfOrFeature<ITileEntityLootable>();
						if (tileEntityLootable != null)
						{
							string text2 = Localization.Get(EntityClass.list[entity2.entityClass].entityClassName, false);
							string arg13 = this.playerInput.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null) + this.playerInput.PermanentActions.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null);
							string arg14 = text2;
							if (entity2 is EntityNPC && entity2.IsAlive())
							{
								text = string.Format(Localization.Get("npcTooltipTalk", false), arg13, arg14);
								EntityDrone entityDrone2 = entity2 as EntityDrone;
								if (entityDrone2 && entityDrone2.IsLocked() && !entityDrone2.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
								{
									text = Localization.Get("ttLocked", false) + "\n" + text;
								}
							}
							else if (entity2 is EntityDriveable && entity2.IsAlive())
							{
								text = string.Format(Localization.Get("tooltipInteract", false), arg13, arg14);
								if (((EntityDriveable)entity2).IsLockedForLocalPlayer(this.entityPlayerLocal))
								{
									text = Localization.Get("ttLocked", false) + "\n" + text;
								}
							}
							else if (!tileEntityLootable.bTouched)
							{
								text = string.Format(Localization.Get("lootTooltipNew", false), arg13, arg14);
							}
							else if (tileEntityLootable.IsEmpty())
							{
								text = string.Format(Localization.Get("lootTooltipEmpty", false), arg13, arg14);
							}
							else
							{
								text = string.Format(Localization.Get("lootTooltipTouched", false), arg13, arg14);
							}
						}
					}
				}
			}
			if (text == null)
			{
				this.InteractName = null;
				if (this.entityPlayerLocal.IsMoveStateStill() && (!this.entityPlayerLocal.IsSwimming() || this.entityPlayerLocal.cameraTransform.up.y < 0.7f))
				{
					this.InteractName = this.entityPlayerLocal.inventory.CanInteract();
					if (this.InteractName != null && this.InteractWaitTime == 0f)
					{
						this.InteractWaitTime = Time.time + 0.3f;
					}
				}
				if (this.InteractName != null)
				{
					if (Time.time >= this.InteractWaitTime)
					{
						flag13 = true;
						string arg15 = this.playerInput.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null) + this.playerInput.PermanentActions.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null);
						text = string.Format(Localization.Get("ttPressTo", false), arg15, Localization.Get(this.InteractName, false));
					}
				}
				else
				{
					this.InteractWaitTime = 0f;
				}
			}
			else
			{
				this.InteractWaitTime = 0f;
			}
		}
		if (this.entityPlayerLocal.IsAlive())
		{
			if (!string.Equals(text, this.strTextLabelPointingTo) && (Time.time - this.timeActivatePressed > 0.5f || string.IsNullOrEmpty(text)))
			{
				XUiC_InteractionPrompt.SetText(this.playerUI, text);
				this.strTextLabelPointingTo = text;
			}
		}
		else
		{
			text = "";
			this.strTextLabelPointingTo = text;
			XUiC_InteractionPrompt.SetText(this.playerUI, null);
		}
		this.FocusBoxPosition = hitInfo2.lastBlockPos;
		if (flag4 || (this.entityPlayerLocal.inventory != null && this.entityPlayerLocal.inventory.holdingItem.IsFocusBlockInside()))
		{
			this.FocusBoxPosition = vector3i;
		}
		this.focusBoxScript.Update(flag14, this.gameManager.World, hitInfo2, this.FocusBoxPosition, this.entityPlayerLocal, this.gameManager.persistentLocalPlayer, flag4);
		if (!this.windowManager.IsInputActive() && !this.windowManager.IsFullHUDDisabled() && (this.playerInput.Activate.IsPressed || this.playerInput.VehicleActions.Activate.IsPressed || this.playerInput.PermanentActions.Activate.IsPressed))
		{
			if (this.playerInput.Activate.WasPressed || this.playerInput.VehicleActions.Activate.WasPressed || this.playerInput.PermanentActions.Activate.WasPressed)
			{
				this.timeActivatePressed = Time.time;
				if (flag13 && hitInfo2.bHitValid && GameUtils.IsBlockOrTerrain(hitInfo2.tag))
				{
					blockValue2 = BlockValue.Air;
				}
				if (this.entityPlayerLocal.AttachedToEntity != null)
				{
					this.entityPlayerLocal.SendDetach();
				}
				else if (entityTurret != null || projectileMoveScript != null || thrownWeaponMoveScript != null || entityItem2 != null || !blockValue2.isair || tileEntityLootable != null || tileEntity != null)
				{
					BlockValue blockValue4 = blockValue2;
					Vector3i vector3i3 = vector3i;
					if (blockValue4.ischild)
					{
						vector3i3 = blockValue4.Block.multiBlockPos.GetParentPos(vector3i3, blockValue4);
						blockValue4 = this.gameManager.World.GetBlock(hitInfo2.hit.clrIdx, vector3i3);
					}
					if (!blockValue4.Equals(BlockValue.Air) && blockValue4.Block.HasBlockActivationCommands(this.gameManager.World, blockValue4, hitInfo2.hit.clrIdx, vector3i3, this.entityPlayerLocal))
					{
						this.playerUI.xui.RadialWindow.Open();
						this.playerUI.xui.RadialWindow.SetCurrentBlockData(this.gameManager.World, vector3i3, hitInfo2.hit.clrIdx, blockValue4, this.entityPlayerLocal);
						flag9 = true;
					}
					else if (tileEntityLootable != null && entity2.GetActivationCommands(tileEntityLootable.ToWorldPos(), this.entityPlayerLocal).Length != 0)
					{
						this.entityPlayerLocal.AimingGun = false;
						tileEntityLootable.bWasTouched = tileEntityLootable.bTouched;
						this.playerUI.xui.RadialWindow.Open();
						this.playerUI.xui.RadialWindow.SetCurrentEntityData(this.gameManager.World, entity2, tileEntityLootable, this.entityPlayerLocal);
						flag9 = true;
					}
					else if (tileEntity != null && entity2.GetActivationCommands(tileEntity.ToWorldPos(), this.entityPlayerLocal).Length != 0)
					{
						this.entityPlayerLocal.AimingGun = false;
						this.playerUI.xui.RadialWindow.Open();
						this.playerUI.xui.RadialWindow.SetCurrentEntityData(this.gameManager.World, entity2, tileEntity, this.entityPlayerLocal);
						flag9 = true;
					}
					else if (entityTurret != null)
					{
						if (entityTurret.CanInteract(this.entityPlayerLocal.entityId))
						{
							ItemStack itemStack = new ItemStack(entityTurret.OriginalItemValue, 1);
							if (this.entityPlayerLocal.inventory.CanTakeItem(itemStack) || this.entityPlayerLocal.bag.CanTakeItem(itemStack))
							{
								this.gameManager.CollectEntityServer(entityTurret.entityId, this.playerUI.entityPlayer.entityId);
							}
							else
							{
								GameManager.ShowTooltip(this.entityPlayerLocal, Localization.Get("xuiInventoryFullForPickup", false), string.Empty, "ui_denied", null, false);
							}
						}
					}
					else
					{
						this.windowManager.Close("radial");
						if (entityItem2 != null)
						{
							EntityItem entityItem3 = entityItem2;
							if (entityItem3 != null && entityItem3.CanCollect() && entityItem3.onGround)
							{
								if (this.entityPlayerLocal.inventory.CanTakeItem(entityItem3.itemStack) || this.entityPlayerLocal.bag.CanTakeItem(entityItem3.itemStack))
								{
									this.gameManager.CollectEntityServer(entityItem2.entityId, this.entityPlayerLocal.entityId);
								}
								else
								{
									GameManager.ShowTooltip(this.entityPlayerLocal, Localization.Get("xuiInventoryFullForPickup", false), string.Empty, "ui_denied", null, false);
								}
							}
						}
						else if (projectileMoveScript != null)
						{
							if (projectileMoveScript.itemProjectile.IsSticky)
							{
								ItemStack itemStack2 = new ItemStack(projectileMoveScript.itemValueProjectile, 1);
								if (this.entityPlayerLocal.inventory.CanTakeItem(itemStack2) || this.entityPlayerLocal.bag.CanTakeItem(itemStack2))
								{
									this.playerUI.xui.PlayerInventory.AddItem(itemStack2);
									projectileMoveScript.ProjectileID = -1;
									UnityEngine.Object.Destroy(projectileMoveScript.gameObject);
								}
								else
								{
									GameManager.ShowTooltip(this.entityPlayerLocal, Localization.Get("xuiInventoryFullForPickup", false), string.Empty, "ui_denied", null, false);
								}
							}
						}
						else if (thrownWeaponMoveScript != null)
						{
							if (thrownWeaponMoveScript.itemWeapon.IsSticky)
							{
								ItemStack itemStack3 = new ItemStack(thrownWeaponMoveScript.itemValueWeapon, 1);
								if (this.entityPlayerLocal.inventory.CanTakeItem(itemStack3) || this.entityPlayerLocal.bag.CanTakeItem(itemStack3))
								{
									this.playerUI.xui.PlayerInventory.AddItem(itemStack3);
									thrownWeaponMoveScript.ProjectileID = -1;
									UnityEngine.Object.Destroy(thrownWeaponMoveScript.gameObject);
								}
								else
								{
									GameManager.ShowTooltip(this.entityPlayerLocal, Localization.Get("xuiInventoryFullForPickup", false), string.Empty, "ui_denied", null, false);
								}
							}
						}
						else
						{
							this.suckItemsNearby(entityItem2);
						}
					}
				}
				else if (flag13)
				{
					this.entityPlayerLocal.inventory.Interact();
				}
			}
			else
			{
				this.windowManager.Close("radial");
				this.suckItemsNearby(entityItem2);
			}
		}
		if (this.gameManager.IsEditMode() && flag11 && flag10 && !this.playerInput.Drop.IsPressed)
		{
			WorldRayHitInfo other = Voxel.voxelRayHitInfo.Clone();
			num6 = 325;
			int minValue = int.MinValue;
			if (Voxel.RaycastOnVoxels(this.gameManager.World, ray, 250f, minValue, num6, 0f) && SelectionBoxManager.Instance.Select(Voxel.voxelRayHitInfo))
			{
				flag10 = false;
				this.bIgnoreLeftMouseUntilReleased = true;
			}
			Voxel.voxelRayHitInfo.CopyFrom(other);
		}
		if (flag11 && (GameManager.Instance.World.IsEditor() || BlockToolSelection.Instance.SelectionActive))
		{
			flag11 &= !this.playerInput.Drop.IsPressed;
		}
		int num9 = this.playerInput.InventorySlotWasPressed;
		if (num9 >= 0)
		{
			if (this.playerInput.LastInputType == BindingSourceType.DeviceBindingSource)
			{
				if (this.entityPlayerLocal.AimingGun)
				{
					num9 = -1;
				}
			}
			else if (InputUtils.ShiftKeyPressed && this.entityPlayerLocal.inventory.PUBLIC_SLOTS > this.entityPlayerLocal.inventory.SHIFT_KEY_SLOT_OFFSET)
			{
				num9 += this.entityPlayerLocal.inventory.SHIFT_KEY_SLOT_OFFSET;
			}
		}
		if (this.inventoryScrollPressed && this.inventoryScrollIdxToSelect != -1)
		{
			num9 = this.inventoryScrollIdxToSelect;
		}
		if (!flag2)
		{
			flag2 = this.entityPlayerLocal.inventory.holdingItem.ConsumeScrollWheel(this.entityPlayerLocal.inventory.holdingItemData, num, this.playerInput);
		}
		this.entityPlayerLocal.inventory.holdingItem.CheckKeys(this.entityPlayerLocal.inventory.holdingItemData, hitInfo2);
		ItemClass holdingItem = this.entityPlayerLocal.inventory.holdingItem;
		bool flag15 = (holdingItem.Actions[0] != null && holdingItem.Actions[0].AllowConcurrentActions()) || (holdingItem.Actions[1] != null && holdingItem.Actions[1].AllowConcurrentActions());
		bool flag16 = holdingItem.Actions[1] != null && holdingItem.Actions[1].IsActionRunning(this.entityPlayerLocal.inventory.holdingItemData.actionData[1]);
		if (flag10 && flag11 && (flag15 || !flag16))
		{
			if (this.gameManager.IsEditMode())
			{
				flag10 = !this.gameManager.GetActiveBlockTool().ExecuteAttackAction(this.entityPlayerLocal.inventory.holdingItemData, false, this.playerInput);
			}
			if (flag10)
			{
				this.entityPlayerLocal.inventory.Execute(0, false, this.playerInput);
			}
		}
		if (flag10 && this.playerInput.Primary.WasReleased)
		{
			if (this.gameManager.IsEditMode() && !this.entityPlayerLocal.inventory.holdingItem.IsGun())
			{
				flag10 = !this.gameManager.GetActiveBlockTool().ExecuteAttackAction(this.entityPlayerLocal.inventory.holdingItemData, true, this.playerInput);
			}
			if (flag10)
			{
				this.entityPlayerLocal.inventory.Execute(0, true, this.playerInput);
			}
		}
		ItemAction itemAction = this.entityPlayerLocal.inventory.holdingItem.Actions[0];
		bool flag17 = itemAction != null && itemAction.IsActionRunning(this.entityPlayerLocal.inventory.holdingItemData.actionData[0]);
		if (flag9 && flag12 && (flag15 || !flag17))
		{
			if (this.gameManager.IsEditMode())
			{
				flag9 = !this.gameManager.GetActiveBlockTool().ExecuteUseAction(this.entityPlayerLocal.inventory.holdingItemData, false, this.playerInput);
			}
			if (flag9)
			{
				this.entityPlayerLocal.inventory.Execute(1, false, this.playerInput);
			}
		}
		if (flag9 && this.playerInput.Secondary.WasReleased && this.entityPlayerLocal.inventory != null)
		{
			this.entityPlayerLocal.inventory.Execute(1, true, this.playerInput);
			if (this.gameManager.IsEditMode() && !this.entityPlayerLocal.inventory.holdingItem.IsGun())
			{
				this.gameManager.GetActiveBlockTool().ExecuteUseAction(this.entityPlayerLocal.inventory.holdingItemData, true, this.playerInput);
			}
		}
		if (this.playerInput.Drop.WasPressed && !this.gameManager.IsEditMode() && !BlockToolSelection.Instance.SelectionActive && this.entityPlayerLocal.inventory != null && !this.entityPlayerLocal.inventory.IsHoldingItemActionRunning() && !this.entityPlayerLocal.inventory.IsHolsterDelayActive() && !this.entityPlayerLocal.inventory.IsUnholsterDelayActive() && this.entityPlayerLocal.inventory.holdingItemIdx != this.entityPlayerLocal.inventory.DUMMY_SLOT_IDX && !this.entityPlayerLocal.AimingGun && num9 == -1 && !flag2)
		{
			Vector3 dropPosition = this.entityPlayerLocal.GetDropPosition();
			ItemValue holdingItemItemValue = this.entityPlayerLocal.inventory.holdingItemItemValue;
			if (ItemClass.GetForId(holdingItemItemValue.type).CanDrop(holdingItemItemValue) && this.entityPlayerLocal.inventory.holdingCount > 0 && this.entityPlayerLocal.DropTimeDelay <= 0f)
			{
				this.entityPlayerLocal.DropTimeDelay = 0.5f;
				int count = this.entityPlayerLocal.inventory.holdingItemStack.count;
				this.gameManager.ItemDropServer(this.entityPlayerLocal.inventory.holdingItemStack.Clone(), dropPosition, Vector3.zero, this.entityPlayerLocal.entityId, ItemClass.GetForId(holdingItemItemValue.type).GetLifetimeOnDrop(), false);
				this.entityPlayerLocal.AddUIHarvestingItem(new ItemStack(holdingItemItemValue, -count), false);
				Manager.BroadcastPlay(this.entityPlayerLocal, "itemdropped", false);
				this.entityPlayerLocal.inventory.DecHoldingItem(count);
			}
		}
		bool flag18 = this.playerInput.InventorySlotLeft.WasPressed || this.playerInput.InventorySlotRight.WasPressed || this.inventoryScrollPressed;
		this.inventoryScrollPressed = false;
		if (this.entityPlayerLocal.AttachedToEntity == null)
		{
			if (num9 != -1 && num9 != this.entityPlayerLocal.inventory.GetFocusedItemIdx() && num9 < this.entityPlayerLocal.inventory.PUBLIC_SLOTS && this.entityPlayerLocal.inventory != null)
			{
				ItemActionRanged itemActionRanged = this.entityPlayerLocal.inventory.GetHoldingGun() as ItemActionRanged;
				if (itemActionRanged != null)
				{
					itemActionRanged.CancelReload(this.entityPlayerLocal.inventory.holdingItemData.actionData[0]);
				}
				else
				{
					ItemActionActivate itemActionActivate = this.entityPlayerLocal.inventory.holdingItem.Actions[1] as ItemActionActivate;
					if (itemActionActivate != null)
					{
						itemActionActivate.CancelAction(this.entityPlayerLocal.inventory.holdingItemData.actionData[1]);
					}
				}
				this.entityPlayerLocal.AimingGun = false;
				this.inventoryItemToSetAfterTimeout = this.entityPlayerLocal.inventory.SetFocusedItemIdx(num9);
				this.inventoryItemSwitchTimeout = (flag18 ? 0.3f : 0f);
			}
			if (this.inventoryItemSwitchTimeout > 0f)
			{
				this.inventoryItemSwitchTimeout -= Time.deltaTime;
			}
			Inventory inventory = this.entityPlayerLocal.inventory;
			if (this.inventoryItemToSetAfterTimeout != -2147483648 && this.inventoryItemSwitchTimeout <= 0f && inventory != null)
			{
				if (inventory.IsHoldingItemActionRunning())
				{
					ItemActionRanged itemActionRanged2 = inventory.GetHoldingGun() as ItemActionRanged;
					if (itemActionRanged2 != null)
					{
						itemActionRanged2.CancelReload(inventory.holdingItemData.actionData[0]);
					}
				}
				else
				{
					this.entityPlayerLocal.AimingGun = false;
					inventory.SetHoldingItemIdx(this.inventoryItemToSetAfterTimeout);
					this.inventoryItemToSetAfterTimeout = int.MinValue;
				}
			}
			if ((this.playerInput.Reload.WasPressed || this.playerInput.PermanentActions.Reload.WasPressed) && this.entityPlayerLocal.inventory != null)
			{
				bool flag19 = this.entityPlayerLocal.inventory.IsHoldingGun() || this.entityPlayerLocal.inventory.IsHoldingDynamicMelee();
				ItemAction holdingPrimary = this.entityPlayerLocal.inventory.GetHoldingPrimary();
				ItemAction holdingSecondary = this.entityPlayerLocal.inventory.GetHoldingSecondary();
				if (flag19 && holdingPrimary != null)
				{
					if (holdingPrimary.HasRadial())
					{
						this.timeActivatePressed = Time.time;
						this.playerUI.xui.RadialWindow.Open();
						holdingPrimary.SetupRadial(this.playerUI.xui.RadialWindow, this.entityPlayerLocal);
					}
					else
					{
						holdingPrimary.CancelAction(this.entityPlayerLocal.inventory.holdingItemData.actionData[0]);
						if (holdingSecondary != null && !(holdingSecondary is ItemActionSpawnTurret))
						{
							holdingSecondary.CancelAction(this.entityPlayerLocal.inventory.holdingItemData.actionData[1]);
						}
					}
				}
				else if (this.entityPlayerLocal.inventory.GetHoldingBlock() != null)
				{
					this.timeActivatePressed = Time.time;
					this.playerUI.xui.RadialWindow.Open();
					this.playerUI.xui.RadialWindow.SetupBlockShapeData();
				}
			}
			if (!flag2 && (this.playerInput.ToggleFlashlight.WasPressed || this.playerInput.PermanentActions.ToggleFlashlight.WasPressed))
			{
				this.timeActivatePressed = Time.time;
				this.playerUI.xui.RadialWindow.Open();
				this.playerUI.xui.RadialWindow.SetActivatableItemData(this.entityPlayerLocal);
			}
			if (!flag2 && (this.playerInput.Swap.WasPressed || this.playerInput.PermanentActions.Swap.WasPressed))
			{
				this.timeActivatePressed = Time.time;
				this.playerUI.xui.RadialWindow.Open();
				this.playerUI.xui.RadialWindow.SetupToolbeltMenu(0);
			}
			if (!flag2 && this.playerInput.InventorySlotRight.WasPressed)
			{
				this.timeActivatePressed = Time.time;
				this.playerUI.xui.RadialWindow.Open();
				this.playerUI.xui.RadialWindow.SetupToolbeltMenu(1);
			}
			if (!flag2 && this.playerInput.InventorySlotLeft.WasPressed)
			{
				this.timeActivatePressed = Time.time;
				this.playerUI.xui.RadialWindow.Open();
				this.playerUI.xui.RadialWindow.SetupToolbeltMenu(-1);
				return;
			}
		}
		else
		{
			EntityVehicle entityVehicle = this.entityPlayerLocal.AttachedToEntity as EntityVehicle;
			if (entityVehicle != null)
			{
				if (this.playerInput.PermanentActions.ToggleFlashlight.WasPressed || this.playerInput.VehicleActions.ToggleFlashlight.WasPressed)
				{
					if (entityVehicle.HasHeadlight())
					{
						entityVehicle.ToggleHeadlight();
					}
					else
					{
						this.timeActivatePressed = Time.time;
						this.playerUI.xui.RadialWindow.Open();
						this.playerUI.xui.RadialWindow.SetActivatableItemData(this.entityPlayerLocal);
					}
				}
				if (this.playerInput.VehicleActions.HonkHorn.WasPressed)
				{
					entityVehicle.UseHorn();
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void suckItemsNearby(EntityItem focusedItem)
	{
		if (!this.countdownSuckItemsNearby.HasPassed())
		{
			return;
		}
		this.countdownSuckItemsNearby.ResetAndRestart();
		if (!this.entityPlayerLocal.addedToChunk)
		{
			return;
		}
		int num = this.entityPlayerLocal.GetBlockPosition().y >> 4;
		for (int i = this.entityPlayerLocal.chunkPosAddedEntityTo.x - 1; i <= this.entityPlayerLocal.chunkPosAddedEntityTo.x + 1; i++)
		{
			for (int j = this.entityPlayerLocal.chunkPosAddedEntityTo.z - 1; j <= this.entityPlayerLocal.chunkPosAddedEntityTo.z + 1; j++)
			{
				Chunk chunk = (Chunk)this.gameManager.World.GetChunkSync(i, 0, j);
				if (chunk != null)
				{
					int num2 = Utils.FastMax(num - 1, 0);
					int num3 = Utils.FastMin(num + 1, chunk.entityLists.Length - 1);
					for (int k = num2; k <= num3; k++)
					{
						int num4 = 0;
						while (chunk.entityLists[k] != null && num4 < chunk.entityLists[k].Count)
						{
							if (chunk.entityLists[k][num4] is EntityItem)
							{
								EntityItem entityItem = (EntityItem)chunk.entityLists[k][num4];
								if (entityItem.CanCollect())
								{
									Vector3 velToAdd = this.entityPlayerLocal.getHeadPosition() - chunk.entityLists[k][num4].GetPosition();
									if (velToAdd.sqrMagnitude <= 16f && (this.entityPlayerLocal.inventory.CanTakeItem(entityItem.itemStack) || this.entityPlayerLocal.bag.CanTakeItem(entityItem.itemStack)))
									{
										if (velToAdd.sqrMagnitude < 4f)
										{
											if (focusedItem != null && focusedItem.onGround)
											{
												this.gameManager.CollectEntityServer(focusedItem.entityId, this.entityPlayerLocal.entityId);
											}
										}
										else
										{
											velToAdd.Normalize();
											velToAdd.x *= 0.7f;
											velToAdd.y *= 1.5f;
											velToAdd.z *= 0.7f;
											this.gameManager.AddVelocityToEntityServer(chunk.entityLists[k][num4].entityId, velToAdd);
										}
									}
								}
							}
							num4++;
						}
					}
				}
			}
		}
	}

	public void SkipMouseLookNextFrame()
	{
		this.skipMouseLookNextFrame = 3;
	}

	public void SetControllableOverride(bool _b)
	{
		this.bCanControlOverride = _b;
	}

	public void Respawn(RespawnType _type)
	{
		this.gameManager.World.GetPrimaryPlayer().Spawned = false;
		this.respawnReason = _type;
		switch (_type)
		{
		case RespawnType.NewGame:
			this.respawnTime = Constants.cRespawnEnterGameTime;
			return;
		case RespawnType.LoadedGame:
			this.respawnTime = 0f;
			return;
		case RespawnType.Died:
			this.respawnTime = Constants.cRespawnAfterDeathTime;
			return;
		default:
			return;
		}
	}

	public void AllowPlayerInput(bool allow)
	{
		this.bAllowPlayerInput = allow;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DrawChunkBoundary()
	{
		Vector3i vector3i = World.toChunkXYZCube(this.entityPlayerLocal.position);
		for (int i = -1; i <= 1; i++)
		{
			for (int j = -1; j <= 1; j++)
			{
				for (int k = -1; k <= 1; k++)
				{
					string name = string.Format("PlayerChunk{0},{1},{2}", k, i, j);
					Vector3 vector;
					vector.x = (float)((vector3i.x + k) * 16);
					vector.y = (float)((vector3i.y + i) * 16);
					vector.z = (float)((vector3i.z + j) * 16);
					Vector3 cornerPos = vector;
					cornerPos.x += 16f;
					cornerPos.y += 16f;
					cornerPos.z += 16f;
					DebugLines debugLines;
					if (k == 0 && i == 0 && j == 0)
					{
						debugLines = DebugLines.Create(name, this.entityPlayerLocal.RootTransform, new Color(1f, 1f, 1f), new Color(1f, 1f, 1f), 0.1f, 0.1f, 0.1f);
					}
					else
					{
						debugLines = DebugLines.Create(name, this.entityPlayerLocal.RootTransform, new Color(0.3f, 0.3f, 0.3f), new Color(0.3f, 0.3f, 0.3f), 0.033f, 0.033f, 0.1f);
					}
					debugLines.AddCube(vector, cornerPos);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DrawChunkDensities()
	{
		Vector3i vector3i = World.toChunkXYZCube(this.entityPlayerLocal.position);
		vector3i *= 16;
		IChunk chunkFromWorldPos = this.entityPlayerLocal.world.GetChunkFromWorldPos(World.worldToBlockPos(this.entityPlayerLocal.position));
		if (chunkFromWorldPos == null)
		{
			return;
		}
		for (int i = 0; i < 16; i++)
		{
			for (int j = 0; j < 16; j++)
			{
				for (int k = 0; k < 16; k++)
				{
					int num = i + vector3i.y;
					int density = (int)chunkFromWorldPos.GetDensity(k, num, j);
					if (density != (int)MarchingCubes.DensityAir && density != (int)MarchingCubes.DensityTerrain)
					{
						float num2 = 0f;
						if (num > 0)
						{
							sbyte density2 = chunkFromWorldPos.GetDensity(k, num - 1, j);
							num2 = MarchingCubes.GetDecorationOffsetY((sbyte)density, density2);
						}
						string name = string.Format("PlayerDensity{0},{1},{2}", k, i, j);
						Vector3 vector;
						vector.x = (float)(vector3i.x + k) + 0.5f - 0.5f;
						vector.y = (float)num;
						vector.z = (float)(vector3i.z + j) + 0.5f - 0.5f;
						Vector3 cornerPos = vector;
						cornerPos.x += 1f;
						cornerPos.y += 0.5f + num2;
						cornerPos.z += 1f;
						DebugLines debugLines;
						if (density > 0)
						{
							float b = 1f - (float)density / 127f;
							Color color = new Color(0.2f, 0.2f, b);
							debugLines = DebugLines.Create(name, this.entityPlayerLocal.RootTransform, color, color, 0.005f, 0.005f, 0.1f);
						}
						else
						{
							float num3 = (float)(-(float)density) / 128f;
							Color color2 = new Color(num3, num3, 0.2f);
							debugLines = DebugLines.Create(name, this.entityPlayerLocal.RootTransform, color2, color2, 0.01f, 0.01f, 0.1f);
						}
						debugLines.AddCube(vector, cornerPos);
					}
				}
			}
		}
	}

	public void FindCameraSnapTarget(eCameraSnapMode snapMode, float maxDistance)
	{
		PlayerMoveController.cameraSnapTargets.Clear();
		GameManager.Instance.World.GetEntitiesInBounds(typeof(EntityAlive), new Bounds(this.entityPlayerLocal.position, Vector3.one * maxDistance), PlayerMoveController.cameraSnapTargets);
		float num = float.MaxValue;
		EntityAlive target = null;
		if (PlayerMoveController.cameraSnapTargets.Count > 0)
		{
			foreach (Entity entity in PlayerMoveController.cameraSnapTargets)
			{
				EntityAlive entityAlive = (EntityAlive)entity;
				if (!(entityAlive == this.entityPlayerLocal) && entityAlive.IsValidAimAssistSnapTarget && entityAlive.IsAlive() && !(entityAlive.ModelTransform == null))
				{
					Vector3 direction = entityAlive.GetChestTransformPosition() - this.entityPlayerLocal.cameraTransform.position;
					float sqrMagnitude = direction.sqrMagnitude;
					float num2 = Vector3.Angle(this.entityPlayerLocal.cameraTransform.forward, direction.normalized);
					if (snapMode == eCameraSnapMode.Zoom)
					{
						float num3 = 15f * (1f - this.targetSnapFalloffCurve.Evaluate(sqrMagnitude / 50f));
						if (num2 > num3)
						{
							continue;
						}
					}
					else if (num2 > 20f)
					{
						continue;
					}
					if (this.entityPlayerLocal.HitInfo.transform && this.entityPlayerLocal.HitInfo.transform.IsChildOf(entityAlive.ModelTransform) && sqrMagnitude < num)
					{
						target = entityAlive;
						break;
					}
					RaycastHit raycastHit;
					if (sqrMagnitude < num && Physics.Raycast(new Ray(this.entityPlayerLocal.cameraTransform.position, direction), out raycastHit, maxDistance, -538750997) && ((entityAlive.PhysicsTransform != null && raycastHit.collider.transform.IsChildOf(entityAlive.PhysicsTransform)) || raycastHit.collider.transform.IsChildOf(entityAlive.ModelTransform)))
					{
						num = sqrMagnitude;
						target = entityAlive;
					}
				}
			}
			this.SetCameraSnapEntity(target, snapMode);
		}
	}

	public void SetCameraSnapEntity(EntityAlive _target, eCameraSnapMode _snapMode)
	{
		this.cameraSnapTargetEntity = _target;
		this.cameraSnapMode = _snapMode;
		if (this.cameraSnapTargetEntity != null)
		{
			Vector2 b = Vector2.one * 0.5f;
			Vector2 a = this.entityPlayerLocal.playerCamera.WorldToViewportPoint(this.cameraSnapTargetEntity.GetChestTransformPosition());
			Vector2 a2 = this.entityPlayerLocal.playerCamera.WorldToViewportPoint(this.cameraSnapTargetEntity.emodel.GetHeadTransform().position);
			Vector2 vector = a - b;
			this.snapTargetingHead = ((a2 - b).sqrMagnitude < vector.sqrMagnitude);
			this.cameraSnapTime = Time.time;
		}
	}

	public void ForceStopRunning()
	{
		this.entityPlayerLocal.movementInput.running = false;
		this.runToggleActive = false;
	}

	public void SetInventoryIdxFromScroll(int _idx)
	{
		this.inventoryScrollPressed = true;
		this.inventoryScrollIdxToSelect = _idx;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool bAllowPlayerInput = true;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static FastTags<TagGroup.Global> BowTag = FastTags<TagGroup.Global>.Parse("bow");

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static PlayerMoveController Instance = null;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<NGuiAction> globalActions = new List<NGuiAction>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector2 mouseLookSensitivity;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float mouseZoomSensitivity;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float zoomAccel;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float vehicleLookSensitivity;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector2 controllerLookSensitivity;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public GameManager gameManager;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public NGuiWdwInGameHUD guiInGame;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool bCanControlOverride;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public const float UnstuckCountdownTime = 5f;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool bLastRespawnActive;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float respawnTime;

	public RespawnType respawnReason;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool spawnWindowOpened;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public EntityPlayerLocal entityPlayerLocal;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public LocalPlayerUI playerUI;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public GUIWindowManager windowManager;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public NGUIWindowManager nguiWindowManager;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int invertMouse;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int invertController;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool bControllerVibration;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public RenderDisplacedCube focusBoxScript;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string strTextLabelPointingTo;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public CountdownTimer countdownSuckItemsNearby = new CountdownTimer(0.05f, true);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float inventoryItemSwitchTimeout;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int inventoryItemToSetAfterTimeout = int.MinValue;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public PlayerAutoPilotControllor playerAutoPilotControllor;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string InteractName;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float InteractWaitTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float timeActivatePressed;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool bIgnoreLeftMouseUntilReleased;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int skipMouseLookNextFrame;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool wasCameraChangeUsedWithWheel;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int drawChunkMode;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public AnimationCurve lookAccelerationCurve;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float lookAccelerationRate;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float currentLookAcceleration;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float controllerZoomSensitivity;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float controllerVehicleSensitivity;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool controllerAimAssistsEnabled = true;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool sprintLockEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float aimAssistSlowAmount = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool bAimAssistTargetingItem;

	public Vector3i FocusBoxPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public EntityAlive cameraSnapTargetEntity;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool snapTargetingHead;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public eCameraSnapMode cameraSnapMode;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float cameraSnapTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static List<Entity> cameraSnapTargets = new List<Entity>();

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public AnimationCurve targetSnapFalloffCurve;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Action toggleGodMode;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Action teleportPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool IsGUICancelPressed;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool runToggleActive;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float runInputTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool inventoryScrollPressed;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int inventoryScrollIdxToSelect = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public SpawnPosition spawnPosition = SpawnPosition.Undef;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool waitingForSpawnPointSelection;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public ERoutineState unstuckCoState;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool wasUIInputActive;

	public static bool useScaledMouseLook = false;

	public static float lookDeltaTimeScale = 100f;

	public static float mouseDeltaTimeScale = 75f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector2 previousMouseInput = Vector2.zero;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool runPressedWhileActive;
}

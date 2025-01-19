using System;
using System.Collections.Generic;
using InControl;

public class PlayerActionsLocal : PlayerActionsBase
{
	public int InventorySlotWasPressed
	{
		get
		{
			for (int i = 0; i < this.InventoryActions.Count; i++)
			{
				if (this.InventoryActions[i].WasPressed)
				{
					return i;
				}
			}
			return -1;
		}
	}

	public int InventorySlotWasReleased
	{
		get
		{
			for (int i = 0; i < this.InventoryActions.Count; i++)
			{
				if (this.InventoryActions[i].WasReleased)
				{
					return i;
				}
			}
			return -1;
		}
	}

	public int InventorySlotIsPressed
	{
		get
		{
			for (int i = 0; i < this.InventoryActions.Count; i++)
			{
				if (this.InventoryActions[i].IsPressed)
				{
					return i;
				}
			}
			return -1;
		}
	}

	public PlayerActionsGUI GUIActions { get; }

	public PlayerActionsVehicle VehicleActions { get; }

	public PlayerActionsPermanent PermanentActions { get; }

	public PlayerActionsLocal()
	{
		base.Name = "local";
		this.GUIActions = new PlayerActionsGUI
		{
			Enabled = false
		};
		this.PermanentActions = new PlayerActionsPermanent
		{
			Enabled = true
		};
		this.VehicleActions = new PlayerActionsVehicle
		{
			Enabled = false
		};
		base.UserData = new PlayerActionData.ActionSetUserData(new PlayerActionsBase[]
		{
			this.PermanentActions
		});
		this.VehicleActions.UserData = new PlayerActionData.ActionSetUserData(new PlayerActionsBase[]
		{
			this.PermanentActions
		});
		this.GUIActions.UserData = new PlayerActionData.ActionSetUserData(new PlayerActionsBase[]
		{
			this.PermanentActions
		});
		this.PermanentActions.UserData = new PlayerActionData.ActionSetUserData(new PlayerActionsBase[]
		{
			this,
			this.VehicleActions,
			this.GUIActions
		});
		this.InventoryActions.Add(this.InventorySlot1);
		this.InventoryActions.Add(this.InventorySlot2);
		this.InventoryActions.Add(this.InventorySlot3);
		this.InventoryActions.Add(this.InventorySlot4);
		this.InventoryActions.Add(this.InventorySlot5);
		this.InventoryActions.Add(this.InventorySlot6);
		this.InventoryActions.Add(this.InventorySlot7);
		this.InventoryActions.Add(this.InventorySlot8);
		this.InventoryActions.Add(this.InventorySlot9);
		this.InventoryActions.Add(this.InventorySlot10);
		InputManager.OnActiveDeviceChanged += delegate(InputDevice inputDevice)
		{
			this.UpdateDeadzones();
		};
		this.UpdateDeadzones();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CreateActions()
	{
		this.MoveForward = base.CreatePlayerAction("Forward");
		this.MoveForward.UserData = new PlayerActionData.ActionUserData("inpActPlayerMoveForwardName", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.Both, true, false, false, true);
		this.MoveBack = base.CreatePlayerAction("Back");
		this.MoveBack.UserData = new PlayerActionData.ActionUserData("inpActPlayerMoveBackName", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.Both, true, false, false, true);
		this.MoveLeft = base.CreatePlayerAction("Left");
		this.MoveLeft.UserData = new PlayerActionData.ActionUserData("inpActPlayerMoveLeftName", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.Both, true, false, false, true);
		this.MoveRight = base.CreatePlayerAction("Right");
		this.MoveRight.UserData = new PlayerActionData.ActionUserData("inpActPlayerMoveRightName", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.Both, true, false, false, true);
		this.Move = base.CreateTwoAxisPlayerAction(this.MoveLeft, this.MoveRight, this.MoveBack, this.MoveForward);
		this.LookLeft = base.CreatePlayerAction("LookLeft");
		this.LookLeft.UserData = new PlayerActionData.ActionUserData("inpActPlayerLookLeft", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.ControllerOnly, true, false, false, true);
		this.LookRight = base.CreatePlayerAction("LookRight");
		this.LookRight.UserData = new PlayerActionData.ActionUserData("inpActPlayerLookRight", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.ControllerOnly, true, false, false, true);
		this.LookUp = base.CreatePlayerAction("LookUp");
		this.LookUp.UserData = new PlayerActionData.ActionUserData("inpActPlayerLookUp", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.ControllerOnly, true, false, false, true);
		this.LookDown = base.CreatePlayerAction("LookDown");
		this.LookDown.UserData = new PlayerActionData.ActionUserData("inpActPlayerLookDown", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.ControllerOnly, true, false, false, true);
		this.Look = base.CreateTwoAxisPlayerAction(this.LookLeft, this.LookRight, this.LookDown, this.LookUp);
		this.Primary = base.CreatePlayerAction("Primary");
		this.Primary.UserData = new PlayerActionData.ActionUserData("inpActPlayerPrimaryName", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.Both, true, false, false, true);
		this.Primary.StateThreshold = 0.25f;
		this.Secondary = base.CreatePlayerAction("Secondary");
		this.Secondary.UserData = new PlayerActionData.ActionUserData("inpActPlayerSecondaryName", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.Both, true, false, false, true);
		this.Secondary.StateThreshold = 0.25f;
		this.Run = base.CreatePlayerAction("Run");
		this.Run.UserData = new PlayerActionData.ActionUserData("inpActPlayerRunName", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.Both, true, false, false, true);
		this.Jump = base.CreatePlayerAction("Jump");
		this.Jump.UserData = new PlayerActionData.ActionUserData("inpActPlayerJumpName", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.Both, true, false, false, true);
		this.Crouch = base.CreatePlayerAction("Crouch");
		this.Crouch.UserData = new PlayerActionData.ActionUserData("inpActPlayerCrouchName", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, false, false, true);
		this.ToggleCrouch = base.CreatePlayerAction("ToggleCrouch");
		this.ToggleCrouch.UserData = new PlayerActionData.ActionUserData("inpActPlayerToggleCrouchName", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.Both, true, false, false, true);
		this.ScrollUp = base.CreatePlayerAction("ScrollUp");
		this.ScrollUp.UserData = new PlayerActionData.ActionUserData("inpActScopeZoomInName", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.Both, true, true, false, true);
		this.ScrollDown = base.CreatePlayerAction("ScrollDown");
		this.ScrollDown.UserData = new PlayerActionData.ActionUserData("inpActScopeZoomOutName", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.Both, true, true, false, true);
		this.Scroll = base.CreateOneAxisPlayerAction(this.ScrollDown, this.ScrollUp);
		this.Activate = base.CreatePlayerAction("Activate");
		this.Activate.UserData = new PlayerActionData.ActionUserData("inpActActivateName", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.ControllerOnly, true, false, false, true);
		this.Activate.FirstRepeatDelay = 0.3f;
		this.Drop = base.CreatePlayerAction("Drop");
		this.Drop.UserData = new PlayerActionData.ActionUserData("inpActPlayerDropName", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.Both, true, false, false, true);
		this.Swap = base.CreatePlayerAction("Swap");
		this.Swap.UserData = new PlayerActionData.ActionUserData("inpActSwapName", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.ControllerOnly, true, false, false, true);
		this.Reload = base.CreatePlayerAction("Reload");
		this.Reload.UserData = new PlayerActionData.ActionUserData("inpActPlayerReloadName", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.ControllerOnly, true, false, false, true);
		this.Reload.FirstRepeatDelay = 0.3f;
		this.ToggleFlashlight = base.CreatePlayerAction("ToggleFlashlight");
		this.ToggleFlashlight.UserData = new PlayerActionData.ActionUserData("inpActPlayerToggleFlashlightName", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.ControllerOnly, true, false, false, true);
		this.ToggleFlashlight.FirstRepeatDelay = 0.3f;
		this.InventorySlot1 = base.CreatePlayerAction("Inventory1");
		this.InventorySlot1.UserData = new PlayerActionData.ActionUserData("inpActInventorySlot1Name", null, PlayerActionData.GroupToolbelt, PlayerActionData.EAppliesToInputType.Both, true, false, false, true);
		this.InventorySlot2 = base.CreatePlayerAction("Inventory2");
		this.InventorySlot2.UserData = new PlayerActionData.ActionUserData("inpActInventorySlot2Name", null, PlayerActionData.GroupToolbelt, PlayerActionData.EAppliesToInputType.Both, true, false, false, true);
		this.InventorySlot3 = base.CreatePlayerAction("Inventory3");
		this.InventorySlot3.UserData = new PlayerActionData.ActionUserData("inpActInventorySlot3Name", null, PlayerActionData.GroupToolbelt, PlayerActionData.EAppliesToInputType.Both, true, false, false, true);
		this.InventorySlot4 = base.CreatePlayerAction("Inventory4");
		this.InventorySlot4.UserData = new PlayerActionData.ActionUserData("inpActInventorySlot4Name", null, PlayerActionData.GroupToolbelt, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, false, false, true);
		this.InventorySlot5 = base.CreatePlayerAction("Inventory5");
		this.InventorySlot5.UserData = new PlayerActionData.ActionUserData("inpActInventorySlot5Name", null, PlayerActionData.GroupToolbelt, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, false, false, true);
		this.InventorySlot6 = base.CreatePlayerAction("Inventory6");
		this.InventorySlot6.UserData = new PlayerActionData.ActionUserData("inpActInventorySlot6Name", null, PlayerActionData.GroupToolbelt, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, false, false, true);
		this.InventorySlot7 = base.CreatePlayerAction("Inventory7");
		this.InventorySlot7.UserData = new PlayerActionData.ActionUserData("inpActInventorySlot7Name", null, PlayerActionData.GroupToolbelt, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, false, false, true);
		this.InventorySlot8 = base.CreatePlayerAction("Inventory8");
		this.InventorySlot8.UserData = new PlayerActionData.ActionUserData("inpActInventorySlot8Name", null, PlayerActionData.GroupToolbelt, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, false, false, true);
		this.InventorySlot9 = base.CreatePlayerAction("Inventory9");
		this.InventorySlot9.UserData = new PlayerActionData.ActionUserData("inpActInventorySlot9Name", null, PlayerActionData.GroupToolbelt, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, false, false, true);
		this.InventorySlot10 = base.CreatePlayerAction("Inventory10");
		this.InventorySlot10.UserData = new PlayerActionData.ActionUserData("inpActInventorySlot10Name", null, PlayerActionData.GroupToolbelt, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, false, false, true);
		this.InventorySlotLeft = base.CreatePlayerAction("InventorySelectLeft");
		this.InventorySlotLeft.UserData = new PlayerActionData.ActionUserData("inpActInventorySlotLeftName", null, PlayerActionData.GroupToolbelt, PlayerActionData.EAppliesToInputType.Both, true, false, false, true);
		this.InventorySlotRight = base.CreatePlayerAction("InventorySelectRight");
		this.InventorySlotRight.UserData = new PlayerActionData.ActionUserData("inpActInventorySlotRightName", null, PlayerActionData.GroupToolbelt, PlayerActionData.EAppliesToInputType.Both, true, false, false, true);
		this.Menu = base.CreatePlayerAction("Menu");
		this.Menu.UserData = new PlayerActionData.ActionUserData("inpActMenuName", null, PlayerActionData.GroupMenu, PlayerActionData.EAppliesToInputType.Both, true, false, false, true);
		this.God = base.CreatePlayerAction("God");
		this.God.UserData = new PlayerActionData.ActionUserData("inpActGodModeName", null, PlayerActionData.GroupAdmin, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, false, false, true);
		this.Fly = base.CreatePlayerAction("Fly");
		this.Fly.UserData = new PlayerActionData.ActionUserData("inpActFlyModeName", null, PlayerActionData.GroupAdmin, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, false, false, true);
		this.Invisible = base.CreatePlayerAction("Invisible");
		this.Invisible.UserData = new PlayerActionData.ActionUserData("inpActInvisibleModeName", null, PlayerActionData.GroupAdmin, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, false, false, true);
		this.IncSpeed = base.CreatePlayerAction("IncSpeed");
		this.IncSpeed.UserData = new PlayerActionData.ActionUserData("inpActIncGodSpeedName", null, PlayerActionData.GroupAdmin, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, false, false, true);
		this.DecSpeed = base.CreatePlayerAction("DecSpeed");
		this.DecSpeed.UserData = new PlayerActionData.ActionUserData("inpActDecGodSpeedName", null, PlayerActionData.GroupAdmin, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, false, false, true);
		this.GodAlternate = base.CreatePlayerAction("GodAlternate");
		this.GodAlternate.UserData = new PlayerActionData.ActionUserData("inpActGodAlternateModeName", null, PlayerActionData.GroupAdmin, PlayerActionData.EAppliesToInputType.ControllerOnly, false, true, true, true);
		this.TeleportAlternate = base.CreatePlayerAction("TeleportAlternate");
		this.TeleportAlternate.UserData = new PlayerActionData.ActionUserData("inpActTeleportAlternateModeName", null, PlayerActionData.GroupAdmin, PlayerActionData.EAppliesToInputType.ControllerOnly, false, true, true, true);
		this.CameraChange = base.CreatePlayerAction("CameraChange");
		this.CameraChange.UserData = new PlayerActionData.ActionUserData("inpActCameraChangeName", null, PlayerActionData.GroupEditCamera, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, false, false, true);
		this.DetachCamera = base.CreatePlayerAction("DetachCamera");
		this.DetachCamera.UserData = new PlayerActionData.ActionUserData("inpActDetachCameraName", null, PlayerActionData.GroupEditCamera, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, false, false, true);
		this.ToggleDCMove = base.CreatePlayerAction("ToggleDCMove");
		this.ToggleDCMove.UserData = new PlayerActionData.ActionUserData("inpActToggleDCMoveName", null, PlayerActionData.GroupEditCamera, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, false, false, true);
		this.LockFreeCamera = base.CreatePlayerAction("LockFreeCamera");
		this.LockFreeCamera.UserData = new PlayerActionData.ActionUserData("inpActLockFreeCameraName", null, PlayerActionData.GroupEditCamera, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, false, false, true);
		this.SelectionFill = base.CreatePlayerAction("SelectionFill");
		this.SelectionFill.UserData = new PlayerActionData.ActionUserData("inpActSelectionFillName", null, PlayerActionData.GroupEditSelection, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, false, false, true);
		this.SelectionClear = base.CreatePlayerAction("SelectionClear");
		this.SelectionClear.UserData = new PlayerActionData.ActionUserData("inpActSelectionClearName", null, PlayerActionData.GroupEditSelection, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, false, false, true);
		this.SelectionSet = base.CreatePlayerAction("SelectionSet");
		this.SelectionSet.UserData = new PlayerActionData.ActionUserData("inpActSelectionSetName", null, PlayerActionData.GroupEditSelection, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, false, false, true);
		this.SelectionRotate = base.CreatePlayerAction("SelectionRotate");
		this.SelectionRotate.UserData = new PlayerActionData.ActionUserData("inpActSelectionRotateName", null, PlayerActionData.GroupEditSelection, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, false, false, true);
		this.SelectionDelete = base.CreatePlayerAction("SelectionDelete");
		this.SelectionDelete.UserData = new PlayerActionData.ActionUserData("inpActSelectionDeleteName", null, PlayerActionData.GroupEditSelection, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, false, false, true);
		this.SelectionMoveMode = base.CreatePlayerAction("SelectionMoveMode");
		this.SelectionMoveMode.UserData = new PlayerActionData.ActionUserData("inpActSelectionMoveModeName", null, PlayerActionData.GroupEditSelection, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, false, false, true);
		this.FocusCopyBlock = base.CreatePlayerAction("FocusCopyBlock");
		this.FocusCopyBlock.UserData = new PlayerActionData.ActionUserData("inpActFocusCopyBlockName", null, PlayerActionData.GroupEditOther, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, false, false, true);
		this.Prefab = base.CreatePlayerAction("Prefab");
		this.Prefab.UserData = new PlayerActionData.ActionUserData("inpActPrefabName", null, PlayerActionData.GroupEditOther, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, false, false, true);
		this.DensityM1 = base.CreatePlayerAction("DensityM1");
		this.DensityM1.UserData = new PlayerActionData.ActionUserData("inpActDensityM1Name", null, PlayerActionData.GroupEditOther, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, false, false, true);
		this.DensityP1 = base.CreatePlayerAction("DensityP1");
		this.DensityP1.UserData = new PlayerActionData.ActionUserData("inpActDensityP1Name", null, PlayerActionData.GroupEditOther, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, false, false, true);
		this.DensityM10 = base.CreatePlayerAction("DensityM10");
		this.DensityM10.UserData = new PlayerActionData.ActionUserData("inpActDensityM10Name", null, PlayerActionData.GroupEditOther, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, false, false, true);
		this.DensityP10 = base.CreatePlayerAction("DensityP10");
		this.DensityP10.UserData = new PlayerActionData.ActionUserData("inpActDensityP10Name", null, PlayerActionData.GroupEditOther, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, false, false, true);
		this.Inventory = base.CreatePlayerAction("Inventory");
		this.Inventory.UserData = new PlayerActionData.ActionUserData("inpActInventoryName", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.ControllerOnly, true, false, false, true);
		this.Scoreboard = base.CreatePlayerAction("Scoreboard");
		this.Scoreboard.UserData = new PlayerActionData.ActionUserData("inpActScoreboardName", null, PlayerActionData.GroupDialogs, PlayerActionData.EAppliesToInputType.ControllerOnly, true, false, false, true);
		this.AiFreeze = base.CreatePlayerAction("AiFreeze");
		this.AiFreeze.UserData = new PlayerActionData.ActionUserData("inpActAiFreezeName", null, PlayerActionData.GroupDebugFunctions, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, false, false, true);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CreateDefaultJoystickBindings()
	{
		base.ListenOptions.IncludeControllers = true;
		this.ConfigureJoystickLayout();
		this.Run.AddDefaultBinding(InputControlType.LeftStickButton);
		this.Jump.AddDefaultBinding(InputControlType.Action1);
		this.ToggleCrouch.AddDefaultBinding(InputControlType.RightStickButton);
		this.Activate.AddDefaultBinding(InputControlType.Action4);
		this.Reload.AddDefaultBinding(InputControlType.Action3);
		this.Primary.AddDefaultBinding(InputControlType.RightTrigger);
		this.Secondary.AddDefaultBinding(InputControlType.LeftTrigger);
		this.InventorySlotLeft.AddDefaultBinding(InputControlType.LeftBumper);
		this.InventorySlotRight.AddDefaultBinding(InputControlType.RightBumper);
		this.ToggleFlashlight.AddDefaultBinding(InputControlType.DPadUp);
		this.Drop.AddDefaultBinding(InputControlType.DPadDown);
		this.Swap.AddDefaultBinding(InputControlType.DPadLeft);
		this.ScrollUp.AddDefaultBinding(InputControlType.DPadUp);
		this.ScrollDown.AddDefaultBinding(InputControlType.DPadDown);
		this.Menu.AddDefaultBinding(InputControlType.Menu);
		this.Menu.AddDefaultBinding(InputControlType.Options);
		this.Menu.AddDefaultBinding(InputControlType.Start);
		this.Menu.AddDefaultBinding(InputControlType.Plus);
		this.Inventory.AddDefaultBinding(InputControlType.Action2);
		this.Scoreboard.AddDefaultBinding(InputControlType.View);
		this.Scoreboard.AddDefaultBinding(InputControlType.TouchPadButton);
		this.Scoreboard.AddDefaultBinding(InputControlType.Back);
		this.GodAlternate.AddDefaultBinding(InputControlType.Action4);
		this.TeleportAlternate.AddDefaultBinding(InputControlType.Action1);
		this.ControllerRebindableActions.Clear();
		this.ControllerRebindableActions.Add(this.Jump);
		this.ControllerRebindableActions.Add(this.Reload);
		this.ControllerRebindableActions.Add(this.Inventory);
		this.ControllerRebindableActions.Add(this.Activate);
		this.ControllerRebindableActions.Add(this.Swap);
		this.ControllerRebindableActions.Add(this.Primary);
		this.ControllerRebindableActions.Add(this.Secondary);
		this.ControllerRebindableActions.Add(this.Run);
		this.ControllerRebindableActions.Add(this.ToggleCrouch);
		this.ControllerRebindableActions.Add(this.InventorySlotRight);
		this.ControllerRebindableActions.Add(this.InventorySlotLeft);
		this.ControllerRebindableActions.Add(this.ToggleFlashlight);
		this.ControllerRebindableActions.Add(this.ScrollUp);
		this.ControllerRebindableActions.Add(this.ScrollDown);
		this.ControllerRebindableActions.Add(this.Drop);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CreateDefaultKeyboardBindings()
	{
		base.ListenOptions.IncludeKeys = true;
		base.ListenOptions.IncludeMouseButtons = true;
		base.ListenOptions.IncludeMouseScrollWheel = true;
		this.MoveLeft.AddDefaultBinding(new Key[]
		{
			Key.A
		});
		this.MoveRight.AddDefaultBinding(new Key[]
		{
			Key.D
		});
		this.MoveForward.AddDefaultBinding(new Key[]
		{
			Key.W
		});
		this.MoveBack.AddDefaultBinding(new Key[]
		{
			Key.S
		});
		this.LookLeft.AddDefaultBinding(Mouse.NegativeX);
		this.LookRight.AddDefaultBinding(Mouse.PositiveX);
		this.LookUp.AddDefaultBinding(Mouse.PositiveY);
		this.LookDown.AddDefaultBinding(Mouse.NegativeY);
		this.Run.AddDefaultBinding(new Key[]
		{
			Key.LeftShift
		});
		this.Jump.AddDefaultBinding(new Key[]
		{
			Key.Space
		});
		this.Crouch.AddDefaultBinding(new Key[]
		{
			Key.C
		});
		this.ToggleCrouch.AddDefaultBinding(new Key[]
		{
			Key.LeftControl
		});
		this.Drop.AddDefaultBinding(new Key[]
		{
			Key.G
		});
		this.God.AddDefaultBinding(new Key[]
		{
			Key.Q
		});
		this.Fly.AddDefaultBinding(new Key[]
		{
			Key.H
		});
		this.Invisible.AddDefaultBinding(new Key[]
		{
			Key.PadDivide
		});
		this.IncSpeed.AddDefaultBinding(new Key[]
		{
			Key.Shift,
			Key.Equals
		});
		this.DecSpeed.AddDefaultBinding(new Key[]
		{
			Key.Shift,
			Key.Minus
		});
		this.CameraChange.AddDefaultBinding(new Key[]
		{
			Key.LeftAlt
		});
		this.Primary.AddDefaultBinding(Mouse.LeftButton);
		this.Secondary.AddDefaultBinding(Mouse.RightButton);
		this.SelectionFill.AddDefaultBinding(new Key[]
		{
			Key.L
		});
		this.SelectionClear.AddDefaultBinding(new Key[]
		{
			Key.J
		});
		this.SelectionSet.AddDefaultBinding(new Key[]
		{
			Key.Z
		});
		this.SelectionRotate.AddDefaultBinding(new Key[]
		{
			Key.X
		});
		this.SelectionDelete.AddDefaultBinding(new Key[]
		{
			Key.Backspace
		});
		this.SelectionMoveMode.AddDefaultBinding(new Key[]
		{
			Key.Insert
		});
		this.FocusCopyBlock.AddDefaultBinding(Mouse.MiddleButton);
		this.Prefab.AddDefaultBinding(new Key[]
		{
			Key.K
		});
		this.DetachCamera.AddDefaultBinding(new Key[]
		{
			Key.P
		});
		this.ToggleDCMove.AddDefaultBinding(new Key[]
		{
			Key.LeftBracket
		});
		this.LockFreeCamera.AddDefaultBinding(new Key[]
		{
			Key.Pad1
		});
		this.DensityM1.AddDefaultBinding(new Key[]
		{
			Key.RightArrow
		});
		this.DensityP1.AddDefaultBinding(new Key[]
		{
			Key.LeftArrow
		});
		this.DensityM10.AddDefaultBinding(new Key[]
		{
			Key.UpArrow
		});
		this.DensityP10.AddDefaultBinding(new Key[]
		{
			Key.DownArrow
		});
		this.ScrollUp.AddDefaultBinding(Mouse.PositiveScrollWheel);
		this.ScrollDown.AddDefaultBinding(Mouse.NegativeScrollWheel);
		this.Menu.AddDefaultBinding(new Key[]
		{
			Key.Escape
		});
		this.InventorySlot1.AddDefaultBinding(new Key[]
		{
			Key.Key1
		});
		this.InventorySlot2.AddDefaultBinding(new Key[]
		{
			Key.Key2
		});
		this.InventorySlot3.AddDefaultBinding(new Key[]
		{
			Key.Key3
		});
		this.InventorySlot4.AddDefaultBinding(new Key[]
		{
			Key.Key4
		});
		this.InventorySlot5.AddDefaultBinding(new Key[]
		{
			Key.Key5
		});
		this.InventorySlot6.AddDefaultBinding(new Key[]
		{
			Key.Key6
		});
		this.InventorySlot7.AddDefaultBinding(new Key[]
		{
			Key.Key7
		});
		this.InventorySlot8.AddDefaultBinding(new Key[]
		{
			Key.Key8
		});
		this.InventorySlot9.AddDefaultBinding(new Key[]
		{
			Key.Key9
		});
		this.InventorySlot10.AddDefaultBinding(new Key[]
		{
			Key.Key0
		});
		this.InventorySlotRight.AddDefaultBinding(Mouse.NegativeScrollWheel);
		this.InventorySlotLeft.AddDefaultBinding(Mouse.PositiveScrollWheel);
		this.AiFreeze.AddDefaultBinding(new Key[]
		{
			Key.PadMultiply
		});
	}

	public void SetDeadzones(float _left, float _right)
	{
		this.leftStickDeadzone = _left;
		this.rightStickDeadzone = _right;
		this.UpdateDeadzones();
	}

	public void UpdateDeadzones()
	{
		InputManager.ActiveDevice.LeftStick.LowerDeadZone = this.leftStickDeadzone;
		InputManager.ActiveDevice.RightStick.LowerDeadZone = this.rightStickDeadzone;
	}

	public void SetJoyStickLayout(eControllerJoystickLayout layout)
	{
		this.joystickLayout = layout;
		this.ConfigureJoystickLayout();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ConfigureJoystickLayout()
	{
		switch (this.joystickLayout)
		{
		case eControllerJoystickLayout.Standard:
			this.MoveForward.AddDefaultBinding(InputControlType.LeftStickUp);
			this.MoveForward.RemoveBinding(new DeviceBindingSource(InputControlType.RightStickUp));
			this.MoveBack.AddDefaultBinding(InputControlType.LeftStickDown);
			this.MoveBack.RemoveBinding(new DeviceBindingSource(InputControlType.RightStickDown));
			this.MoveLeft.AddDefaultBinding(InputControlType.LeftStickLeft);
			this.MoveLeft.RemoveBinding(new DeviceBindingSource(InputControlType.RightStickLeft));
			this.MoveRight.AddDefaultBinding(InputControlType.LeftStickRight);
			this.MoveRight.RemoveBinding(new DeviceBindingSource(InputControlType.RightStickRight));
			this.LookUp.AddDefaultBinding(InputControlType.RightStickUp);
			this.LookUp.RemoveBinding(new DeviceBindingSource(InputControlType.LeftStickUp));
			this.LookDown.AddDefaultBinding(InputControlType.RightStickDown);
			this.LookDown.RemoveBinding(new DeviceBindingSource(InputControlType.LeftStickDown));
			this.LookLeft.AddDefaultBinding(InputControlType.RightStickLeft);
			this.LookLeft.RemoveBinding(new DeviceBindingSource(InputControlType.LeftStickLeft));
			this.LookRight.AddDefaultBinding(InputControlType.RightStickRight);
			this.LookRight.RemoveBinding(new DeviceBindingSource(InputControlType.LeftStickRight));
			return;
		case eControllerJoystickLayout.Southpaw:
			this.MoveForward.AddDefaultBinding(InputControlType.RightStickUp);
			this.MoveForward.RemoveBinding(new DeviceBindingSource(InputControlType.LeftStickUp));
			this.MoveBack.AddDefaultBinding(InputControlType.RightStickDown);
			this.MoveBack.RemoveBinding(new DeviceBindingSource(InputControlType.LeftStickDown));
			this.MoveLeft.AddDefaultBinding(InputControlType.RightStickLeft);
			this.MoveLeft.RemoveBinding(new DeviceBindingSource(InputControlType.LeftStickLeft));
			this.MoveRight.AddDefaultBinding(InputControlType.RightStickRight);
			this.MoveRight.RemoveBinding(new DeviceBindingSource(InputControlType.LeftStickRight));
			this.LookUp.AddDefaultBinding(InputControlType.LeftStickUp);
			this.LookUp.RemoveBinding(new DeviceBindingSource(InputControlType.RightStickUp));
			this.LookDown.AddDefaultBinding(InputControlType.LeftStickDown);
			this.LookDown.RemoveBinding(new DeviceBindingSource(InputControlType.RightStickDown));
			this.LookLeft.AddDefaultBinding(InputControlType.LeftStickLeft);
			this.LookLeft.RemoveBinding(new DeviceBindingSource(InputControlType.RightStickLeft));
			this.LookRight.AddDefaultBinding(InputControlType.LeftStickRight);
			this.LookRight.RemoveBinding(new DeviceBindingSource(InputControlType.RightStickRight));
			return;
		case eControllerJoystickLayout.Legacy:
			this.MoveForward.AddDefaultBinding(InputControlType.LeftStickUp);
			this.MoveForward.RemoveBinding(new DeviceBindingSource(InputControlType.RightStickUp));
			this.MoveBack.AddDefaultBinding(InputControlType.LeftStickDown);
			this.MoveBack.RemoveBinding(new DeviceBindingSource(InputControlType.RightStickDown));
			this.MoveLeft.AddDefaultBinding(InputControlType.RightStickLeft);
			this.MoveLeft.RemoveBinding(new DeviceBindingSource(InputControlType.LeftStickLeft));
			this.MoveRight.AddDefaultBinding(InputControlType.RightStickRight);
			this.MoveRight.RemoveBinding(new DeviceBindingSource(InputControlType.LeftStickRight));
			this.LookUp.AddDefaultBinding(InputControlType.RightStickUp);
			this.LookUp.RemoveBinding(new DeviceBindingSource(InputControlType.LeftStickUp));
			this.LookDown.AddDefaultBinding(InputControlType.RightStickDown);
			this.LookDown.RemoveBinding(new DeviceBindingSource(InputControlType.LeftStickDown));
			this.LookLeft.AddDefaultBinding(InputControlType.LeftStickLeft);
			this.LookLeft.RemoveBinding(new DeviceBindingSource(InputControlType.RightStickLeft));
			this.LookRight.AddDefaultBinding(InputControlType.LeftStickRight);
			this.LookRight.RemoveBinding(new DeviceBindingSource(InputControlType.RightStickRight));
			return;
		case eControllerJoystickLayout.LegacySouthpaw:
			this.MoveForward.AddDefaultBinding(InputControlType.RightStickUp);
			this.MoveForward.RemoveBinding(new DeviceBindingSource(InputControlType.LeftStickUp));
			this.MoveBack.AddDefaultBinding(InputControlType.RightStickDown);
			this.MoveBack.RemoveBinding(new DeviceBindingSource(InputControlType.LeftStickDown));
			this.MoveLeft.AddDefaultBinding(InputControlType.LeftStickLeft);
			this.MoveLeft.RemoveBinding(new DeviceBindingSource(InputControlType.RightStickLeft));
			this.MoveRight.AddDefaultBinding(InputControlType.LeftStickRight);
			this.MoveRight.RemoveBinding(new DeviceBindingSource(InputControlType.RightStickRight));
			this.LookUp.AddDefaultBinding(InputControlType.LeftStickUp);
			this.LookUp.RemoveBinding(new DeviceBindingSource(InputControlType.RightStickUp));
			this.LookDown.AddDefaultBinding(InputControlType.LeftStickDown);
			this.LookDown.RemoveBinding(new DeviceBindingSource(InputControlType.RightStickDown));
			this.LookLeft.AddDefaultBinding(InputControlType.RightStickLeft);
			this.LookLeft.RemoveBinding(new DeviceBindingSource(InputControlType.LeftStickLeft));
			this.LookRight.AddDefaultBinding(InputControlType.RightStickRight);
			this.LookRight.RemoveBinding(new DeviceBindingSource(InputControlType.LeftStickRight));
			return;
		default:
			return;
		}
	}

	public bool AnyGUIActionPressed()
	{
		using (IEnumerator<PlayerAction> enumerator = this.GUIActions.Actions.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.WasPressed)
				{
					return true;
				}
			}
		}
		return false;
	}

	public PlayerTwoAxisAction Move;

	public PlayerAction MoveLeft;

	public PlayerAction MoveRight;

	public PlayerAction MoveForward;

	public PlayerAction MoveBack;

	public PlayerTwoAxisAction Look;

	public PlayerAction LookLeft;

	public PlayerAction LookRight;

	public PlayerAction LookUp;

	public PlayerAction LookDown;

	public PlayerAction Run;

	public PlayerAction Jump;

	public PlayerAction Crouch;

	public PlayerAction ToggleCrouch;

	public PlayerAction Activate;

	public PlayerAction Drop;

	public PlayerAction Swap;

	public PlayerAction Reload;

	public PlayerAction Primary;

	public PlayerAction Secondary;

	public PlayerAction ToggleFlashlight;

	public PlayerAction God;

	public PlayerAction Fly;

	public PlayerAction Invisible;

	public PlayerAction IncSpeed;

	public PlayerAction DecSpeed;

	public PlayerAction GodAlternate;

	public PlayerAction TeleportAlternate;

	public PlayerAction CameraChange;

	public PlayerAction SelectionFill;

	public PlayerAction SelectionClear;

	public PlayerAction SelectionSet;

	public PlayerAction SelectionRotate;

	public PlayerAction SelectionDelete;

	public PlayerAction SelectionMoveMode;

	public PlayerAction FocusCopyBlock;

	public PlayerAction Prefab;

	public PlayerAction DetachCamera;

	public PlayerAction ToggleDCMove;

	public PlayerAction LockFreeCamera;

	public PlayerAction DensityM1;

	public PlayerAction DensityP1;

	public PlayerAction DensityM10;

	public PlayerAction DensityP10;

	public PlayerAction ScrollUp;

	public PlayerAction ScrollDown;

	public PlayerOneAxisAction Scroll;

	public PlayerAction Menu;

	public PlayerAction Inventory;

	public PlayerAction Scoreboard;

	public PlayerAction InventorySlot1;

	public PlayerAction InventorySlot2;

	public PlayerAction InventorySlot3;

	public PlayerAction InventorySlot4;

	public PlayerAction InventorySlot5;

	public PlayerAction InventorySlot6;

	public PlayerAction InventorySlot7;

	public PlayerAction InventorySlot8;

	public PlayerAction InventorySlot9;

	public PlayerAction InventorySlot10;

	public PlayerAction InventorySlotLeft;

	public PlayerAction InventorySlotRight;

	public PlayerAction AiFreeze;

	public float leftStickDeadzone = 0.1f;

	public float rightStickDeadzone = 0.1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<PlayerAction> InventoryActions = new List<PlayerAction>();

	public eControllerJoystickLayout joystickLayout;
}

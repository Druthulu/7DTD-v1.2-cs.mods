using System;
using InControl;

public class PlayerActionsVehicle : PlayerActionsBase
{
	public PlayerActionsVehicle()
	{
		base.Name = "vehicle";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CreateActions()
	{
		this.MoveForward = base.CreatePlayerAction("Forward");
		this.MoveForward.UserData = new PlayerActionData.ActionUserData("inpActVehicleMoveForwardName", null, PlayerActionData.GroupVehicle, PlayerActionData.EAppliesToInputType.Both, true, false, false, true);
		this.MoveBack = base.CreatePlayerAction("Back");
		this.MoveBack.UserData = new PlayerActionData.ActionUserData("inpActVehicleMoveBackName", null, PlayerActionData.GroupVehicle, PlayerActionData.EAppliesToInputType.Both, true, false, false, true);
		this.MoveLeft = base.CreatePlayerAction("Left");
		this.MoveLeft.UserData = new PlayerActionData.ActionUserData("inpActVehicleMoveLeftName", null, PlayerActionData.GroupVehicle, PlayerActionData.EAppliesToInputType.Both, true, false, false, true);
		this.MoveRight = base.CreatePlayerAction("Right");
		this.MoveRight.UserData = new PlayerActionData.ActionUserData("inpActVehicleMoveRightName", null, PlayerActionData.GroupVehicle, PlayerActionData.EAppliesToInputType.Both, true, false, false, true);
		this.Move = base.CreateTwoAxisPlayerAction(this.MoveLeft, this.MoveRight, this.MoveBack, this.MoveForward);
		this.LookLeft = base.CreatePlayerAction("LookLeft");
		this.LookLeft.UserData = new PlayerActionData.ActionUserData("inpActVehicleLookName", null, PlayerActionData.GroupVehicle, PlayerActionData.EAppliesToInputType.ControllerOnly, true, false, false, true);
		this.LookRight = base.CreatePlayerAction("LookRight");
		this.LookUp = base.CreatePlayerAction("LookUp");
		this.LookDown = base.CreatePlayerAction("LookDown");
		this.Look = base.CreateTwoAxisPlayerAction(this.LookLeft, this.LookRight, this.LookDown, this.LookUp);
		this.Turbo = base.CreatePlayerAction("Run");
		this.Turbo.UserData = new PlayerActionData.ActionUserData("inpActVehicleTurboName", null, PlayerActionData.GroupVehicle, PlayerActionData.EAppliesToInputType.Both, true, false, false, true);
		this.Brake = base.CreatePlayerAction("Jump");
		this.Brake.UserData = new PlayerActionData.ActionUserData("inpActVehicleBrakeName", null, PlayerActionData.GroupVehicle, PlayerActionData.EAppliesToInputType.Both, true, false, false, true);
		this.Hop = base.CreatePlayerAction("Crouch");
		this.Hop.UserData = new PlayerActionData.ActionUserData("inpActVehicleHopName", null, PlayerActionData.GroupVehicle, PlayerActionData.EAppliesToInputType.Both, true, false, false, true);
		this.Activate = base.CreatePlayerAction("Activate");
		this.Activate.UserData = new PlayerActionData.ActionUserData("inpActActivateName", null, PlayerActionData.GroupVehicle, PlayerActionData.EAppliesToInputType.ControllerOnly, true, false, false, true);
		this.Activate.FirstRepeatDelay = 0.3f;
		this.ToggleFlashlight = base.CreatePlayerAction("ToggleFlashlight");
		this.ToggleFlashlight.UserData = new PlayerActionData.ActionUserData("inpActVehicleToggleLightName", null, PlayerActionData.GroupVehicle, PlayerActionData.EAppliesToInputType.ControllerOnly, true, false, false, true);
		this.HonkHorn = base.CreatePlayerAction("HonkHorn");
		this.HonkHorn.UserData = new PlayerActionData.ActionUserData("inpActHonkHornName", null, PlayerActionData.GroupVehicle, PlayerActionData.EAppliesToInputType.Both, true, false, false, true);
		this.Menu = base.CreatePlayerAction("Menu");
		this.Menu.UserData = new PlayerActionData.ActionUserData("inpActMenuName", null, PlayerActionData.GroupVehicle, PlayerActionData.EAppliesToInputType.Both, true, false, false, true);
		this.Inventory = base.CreatePlayerAction("Inventory");
		this.Inventory.UserData = new PlayerActionData.ActionUserData("inpActInventoryName", null, PlayerActionData.GroupVehicle, PlayerActionData.EAppliesToInputType.ControllerOnly, true, false, false, true);
		this.Scoreboard = base.CreatePlayerAction("Scoreboard");
		this.Scoreboard.UserData = new PlayerActionData.ActionUserData("inpActScoreboardName", null, PlayerActionData.GroupVehicle, PlayerActionData.EAppliesToInputType.ControllerOnly, true, false, false, true);
		this.ToggleTurnMode = base.CreatePlayerAction("ToggleTurnMode");
		this.ToggleTurnMode.UserData = new PlayerActionData.ActionUserData("inpActToggleTurnMode", null, PlayerActionData.GroupVehicle, PlayerActionData.EAppliesToInputType.Both, true, false, false, true);
		this.ScrollUp = base.CreatePlayerAction("ScrollUp");
		this.ScrollUp.UserData = new PlayerActionData.ActionUserData("inpActCameraZoomInName", null, PlayerActionData.GroupVehicle, PlayerActionData.EAppliesToInputType.Both, true, false, false, true);
		this.ScrollDown = base.CreatePlayerAction("ScrollDown");
		this.ScrollDown.UserData = new PlayerActionData.ActionUserData("inpActCameraZoomOutName", null, PlayerActionData.GroupVehicle, PlayerActionData.EAppliesToInputType.Both, true, false, false, true);
		this.Scroll = base.CreateOneAxisPlayerAction(this.ScrollDown, this.ScrollUp);
		this.LeftStickLeft = base.CreatePlayerAction("LeftStickLeft");
		this.LeftStickLeft.UserData = new PlayerActionData.ActionUserData("inpActVehicleLeftStickLeft", null, PlayerActionData.GroupVehicle, PlayerActionData.EAppliesToInputType.ControllerOnly, false, false, true, true);
		this.LeftStickRight = base.CreatePlayerAction("LeftStickRight");
		this.LeftStickRight.UserData = new PlayerActionData.ActionUserData("inpActVehicleLeftStickRight", null, PlayerActionData.GroupVehicle, PlayerActionData.EAppliesToInputType.ControllerOnly, false, false, true, true);
		this.LeftStickForward = base.CreatePlayerAction("LeftStickForward");
		this.LeftStickForward.UserData = new PlayerActionData.ActionUserData("inpActVehicleLeftStickForward", null, PlayerActionData.GroupVehicle, PlayerActionData.EAppliesToInputType.ControllerOnly, false, false, true, true);
		this.LeftStickBack = base.CreatePlayerAction("LeftStickBack");
		this.LeftStickBack.UserData = new PlayerActionData.ActionUserData("inpActVehicleLeftStickBack", null, PlayerActionData.GroupVehicle, PlayerActionData.EAppliesToInputType.ControllerOnly, false, false, true, true);
		this.LeftStick = base.CreateTwoAxisPlayerAction(this.LeftStickLeft, this.LeftStickRight, this.LeftStickBack, this.LeftStickForward);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CreateDefaultJoystickBindings()
	{
		base.ListenOptions.IncludeControllers = true;
		this.MoveLeft.AddDefaultBinding(InputControlType.LeftStickLeft);
		this.MoveRight.AddDefaultBinding(InputControlType.LeftStickRight);
		this.MoveForward.AddDefaultBinding(InputControlType.RightTrigger);
		this.MoveBack.AddDefaultBinding(InputControlType.LeftTrigger);
		this.LookLeft.AddDefaultBinding(InputControlType.RightStickLeft);
		this.LookRight.AddDefaultBinding(InputControlType.RightStickRight);
		this.LookUp.AddDefaultBinding(InputControlType.RightStickUp);
		this.LookDown.AddDefaultBinding(InputControlType.RightStickDown);
		this.Turbo.AddDefaultBinding(InputControlType.RightBumper);
		this.Brake.AddDefaultBinding(InputControlType.Action3);
		this.Hop.AddDefaultBinding(InputControlType.Action1);
		this.Activate.AddDefaultBinding(InputControlType.Action4);
		this.ToggleFlashlight.AddDefaultBinding(InputControlType.DPadLeft);
		this.HonkHorn.AddDefaultBinding(InputControlType.LeftBumper);
		this.Menu.AddDefaultBinding(InputControlType.Menu);
		this.Menu.AddDefaultBinding(InputControlType.Options);
		this.Menu.AddDefaultBinding(InputControlType.Start);
		this.Inventory.AddDefaultBinding(InputControlType.Action2);
		this.Scoreboard.AddDefaultBinding(InputControlType.View);
		this.Scoreboard.AddDefaultBinding(InputControlType.TouchPadButton);
		this.Scoreboard.AddDefaultBinding(InputControlType.Back);
		this.ToggleTurnMode.AddDefaultBinding(InputControlType.RightStickButton);
		this.ScrollUp.AddDefaultBinding(InputControlType.DPadUp);
		this.ScrollDown.AddDefaultBinding(InputControlType.DPadDown);
		this.LeftStickLeft.AddDefaultBinding(InputControlType.LeftStickLeft);
		this.LeftStickRight.AddDefaultBinding(InputControlType.LeftStickRight);
		this.LeftStickForward.AddDefaultBinding(InputControlType.LeftStickUp);
		this.LeftStickBack.AddDefaultBinding(InputControlType.LeftStickDown);
		this.ControllerRebindableActions.Clear();
		this.ControllerRebindableActions.Add(this.MoveForward);
		this.ControllerRebindableActions.Add(this.MoveBack);
		this.ControllerRebindableActions.Add(this.Hop);
		this.ControllerRebindableActions.Add(this.Brake);
		this.ControllerRebindableActions.Add(this.Inventory);
		this.ControllerRebindableActions.Add(this.Activate);
		this.ControllerRebindableActions.Add(this.Turbo);
		this.ControllerRebindableActions.Add(this.HonkHorn);
		this.ControllerRebindableActions.Add(this.ToggleFlashlight);
		this.ControllerRebindableActions.Add(this.ToggleTurnMode);
		this.ControllerRebindableActions.Add(this.ScrollUp);
		this.ControllerRebindableActions.Add(this.ScrollDown);
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
		this.Turbo.AddDefaultBinding(new Key[]
		{
			Key.LeftShift
		});
		this.Brake.AddDefaultBinding(new Key[]
		{
			Key.Space
		});
		this.Hop.AddDefaultBinding(new Key[]
		{
			Key.C
		});
		this.HonkHorn.AddDefaultBinding(new Key[]
		{
			Key.X
		});
		this.Menu.AddDefaultBinding(new Key[]
		{
			Key.Escape
		});
		this.ToggleTurnMode.AddDefaultBinding(Mouse.LeftButton);
		this.ScrollUp.AddDefaultBinding(Mouse.PositiveScrollWheel);
		this.ScrollDown.AddDefaultBinding(Mouse.NegativeScrollWheel);
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

	public PlayerAction Turbo;

	public PlayerAction Brake;

	public PlayerAction Hop;

	public PlayerAction Activate;

	public PlayerAction ToggleFlashlight;

	public PlayerAction HonkHorn;

	public PlayerAction Menu;

	public PlayerAction Inventory;

	public PlayerAction Scoreboard;

	public PlayerAction ToggleTurnMode;

	public PlayerAction ScrollUp;

	public PlayerAction ScrollDown;

	public PlayerOneAxisAction Scroll;

	public PlayerTwoAxisAction LeftStick;

	public PlayerAction LeftStickLeft;

	public PlayerAction LeftStickRight;

	public PlayerAction LeftStickForward;

	public PlayerAction LeftStickBack;
}

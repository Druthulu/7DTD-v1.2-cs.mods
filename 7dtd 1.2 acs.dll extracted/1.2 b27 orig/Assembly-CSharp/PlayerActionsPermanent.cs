using System;
using InControl;

public class PlayerActionsPermanent : PlayerActionsBase
{
	public PlayerActionsPermanent()
	{
		base.Name = "permanent";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CreateActions()
	{
		this.Reload = base.CreatePlayerAction("Reload");
		this.Reload.UserData = new PlayerActionData.ActionUserData("inpActReloadTakeAllName", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, false, false, true);
		this.Reload.FirstRepeatDelay = 0.3f;
		this.Activate = base.CreatePlayerAction("Activate");
		this.Activate.UserData = new PlayerActionData.ActionUserData("inpActActivateName", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, false, false, true);
		this.ToggleFlashlight = base.CreatePlayerAction("ToggleFlashlight");
		this.ToggleFlashlight.UserData = new PlayerActionData.ActionUserData("inpActPlayerToggleFlashlightName", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, false, false, true);
		this.ToggleFlashlight.FirstRepeatDelay = 0.3f;
		this.Inventory = base.CreatePlayerAction("Inventory");
		this.Inventory.UserData = new PlayerActionData.ActionUserData("inpActInventoryName", null, PlayerActionData.GroupDialogs, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, false, false, true);
		this.Character = base.CreatePlayerAction("Character");
		this.Character.UserData = new PlayerActionData.ActionUserData("inpActCharacterName", null, PlayerActionData.GroupDialogs, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, false, false, true);
		this.Map = base.CreatePlayerAction("Map");
		this.Map.UserData = new PlayerActionData.ActionUserData("inpActMapName", null, PlayerActionData.GroupDialogs, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, false, false, true);
		this.Skills = base.CreatePlayerAction("Skills");
		this.Skills.UserData = new PlayerActionData.ActionUserData("inpActSkillsName", null, PlayerActionData.GroupDialogs, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, false, false, true);
		this.Quests = base.CreatePlayerAction("Quests");
		this.Quests.UserData = new PlayerActionData.ActionUserData("inpActQuestsName", null, PlayerActionData.GroupDialogs, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, false, false, true);
		this.Challenges = base.CreatePlayerAction("Challenges");
		this.Challenges.UserData = new PlayerActionData.ActionUserData("inpActChallengesName", null, PlayerActionData.GroupDialogs, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, false, false, true);
		this.Scoreboard = base.CreatePlayerAction("Scoreboard");
		this.Scoreboard.UserData = new PlayerActionData.ActionUserData("inpActScoreboardName", null, PlayerActionData.GroupDialogs, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, false, false, true);
		this.Creative = base.CreatePlayerAction("Creative");
		this.Creative.UserData = new PlayerActionData.ActionUserData("inpActCreativeName", null, PlayerActionData.GroupDialogs, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, false, false, true);
		this.DebugControllerLeft = base.CreatePlayerAction("DebugControllerLeft");
		this.DebugControllerLeft.UserData = new PlayerActionData.ActionUserData("inpActDebugControllerLeftName", null, PlayerActionData.GroupAdmin, PlayerActionData.EAppliesToInputType.ControllerOnly, false, true, true, true);
		this.DebugControllerRight = base.CreatePlayerAction("DebugControllerRight");
		this.DebugControllerRight.UserData = new PlayerActionData.ActionUserData("inpActDebugControllerRightName", null, PlayerActionData.GroupAdmin, PlayerActionData.EAppliesToInputType.ControllerOnly, false, true, true, true);
		this.Chat = base.CreatePlayerAction("Chat");
		this.Chat.UserData = new PlayerActionData.ActionUserData("inpActChatName", null, PlayerActionData.GroupMp, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, false, false, true);
		this.PushToTalk = base.CreatePlayerAction("PushToTalk");
		this.PushToTalk.UserData = new PlayerActionData.ActionUserData("inpActPushToTalkName", null, PlayerActionData.GroupMp, PlayerActionData.EAppliesToInputType.Both, true, false, false, false);
		this.Cancel = base.CreatePlayerAction("Cancel");
		this.Cancel.UserData = new PlayerActionData.ActionUserData("inpActCancelName", null, PlayerActionData.GroupAdmin, PlayerActionData.EAppliesToInputType.Both, false, true, true, true);
		this.Swap = base.CreatePlayerAction("Swap");
		this.Swap.UserData = new PlayerActionData.ActionUserData("inpActSwapName", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, false, false, true);
		this.PageTipsForward = base.CreatePlayerAction("PageTipsForward");
		this.PageTipsForward.UserData = new PlayerActionData.ActionUserData("inpActPageTipsForward", null, PlayerActionData.GroupAdmin, PlayerActionData.EAppliesToInputType.Both, false, false, true, true);
		this.PageTipsBack = base.CreatePlayerAction("PageTipsBack");
		this.PageTipsBack.UserData = new PlayerActionData.ActionUserData("inpActPageTipsBack", null, PlayerActionData.GroupAdmin, PlayerActionData.EAppliesToInputType.Both, false, false, true, true);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CreateDefaultJoystickBindings()
	{
		base.ListenOptions.IncludeControllers = true;
		this.DebugControllerLeft.AddDefaultBinding(InputControlType.LeftBumper);
		this.DebugControllerRight.AddDefaultBinding(InputControlType.RightBumper);
		this.Cancel.AddDefaultBinding(InputControlType.Action2);
		this.PageTipsForward.AddDefaultBinding(InputControlType.RightTrigger);
		this.PageTipsBack.AddDefaultBinding(InputControlType.LeftTrigger);
		this.PushToTalk.AddDefaultBinding(InputControlType.DPadRight);
		this.ControllerRebindableActions.Clear();
		this.ControllerRebindableActions.Add(this.PushToTalk);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CreateDefaultKeyboardBindings()
	{
		base.ListenOptions.IncludeKeys = true;
		base.ListenOptions.IncludeMouseButtons = true;
		base.ListenOptions.IncludeMouseScrollWheel = true;
		this.Reload.AddDefaultBinding(new Key[]
		{
			Key.R
		});
		this.Activate.AddDefaultBinding(new Key[]
		{
			Key.E
		});
		this.ToggleFlashlight.AddDefaultBinding(new Key[]
		{
			Key.F
		});
		this.Inventory.AddDefaultBinding(new Key[]
		{
			Key.Tab
		});
		this.Skills.AddDefaultBinding(new Key[]
		{
			Key.N
		});
		this.Quests.AddDefaultBinding(new Key[]
		{
			Key.O
		});
		this.Challenges.AddDefaultBinding(new Key[]
		{
			Key.Y
		});
		this.Character.AddDefaultBinding(new Key[]
		{
			Key.B
		});
		this.Map.AddDefaultBinding(new Key[]
		{
			Key.M
		});
		this.Creative.AddDefaultBinding(new Key[]
		{
			Key.U
		});
		this.Scoreboard.AddDefaultBinding(new Key[]
		{
			Key.I
		});
		this.Chat.AddDefaultBinding(new Key[]
		{
			Key.T
		});
		this.PushToTalk.AddDefaultBinding(new Key[]
		{
			Key.V
		});
		this.Cancel.AddDefaultBinding(new Key[]
		{
			Key.Escape
		});
		this.Swap.AddDefaultBinding(Mouse.Button4);
		this.PageTipsForward.AddDefaultBinding(Mouse.LeftButton);
		this.PageTipsBack.AddDefaultBinding(Mouse.RightButton);
	}

	public PlayerAction Reload;

	public PlayerAction Activate;

	public PlayerAction ToggleFlashlight;

	public PlayerAction Inventory;

	public PlayerAction Skills;

	public PlayerAction Quests;

	public PlayerAction Challenges;

	public PlayerAction Character;

	public PlayerAction Map;

	public PlayerAction Creative;

	public PlayerAction Scoreboard;

	public PlayerAction DebugControllerLeft;

	public PlayerAction DebugControllerRight;

	public PlayerAction Chat;

	public PlayerAction PushToTalk;

	public PlayerAction Cancel;

	public PlayerAction Swap;

	public PlayerAction PageTipsForward;

	public PlayerAction PageTipsBack;
}

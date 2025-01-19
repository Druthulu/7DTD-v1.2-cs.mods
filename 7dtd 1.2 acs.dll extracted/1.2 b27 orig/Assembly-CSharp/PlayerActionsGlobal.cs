using System;
using InControl;

public class PlayerActionsGlobal : PlayerActionsBase
{
	public static PlayerActionsGlobal Instance
	{
		get
		{
			return PlayerActionsGlobal.m_Instance;
		}
	}

	public static void Init()
	{
		if (PlayerActionsGlobal.m_Instance == null)
		{
			PlayerActionsGlobal.m_Instance = new PlayerActionsGlobal();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public PlayerActionsGlobal()
	{
		base.Name = "global";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CreateActions()
	{
		this.Console = base.CreatePlayerAction("Console");
		this.Console.UserData = new PlayerActionData.ActionUserData("inpActConsoleName", null, PlayerActionData.GroupGlobalFunctions, PlayerActionData.EAppliesToInputType.KbdMouseOnly, false, false, false, true);
		this.ShowDebugData = base.CreatePlayerAction("Show Debug Data");
		this.ShowDebugData.UserData = new PlayerActionData.ActionUserData("inpActShowDebugDataName", null, PlayerActionData.GroupGlobalFunctions, PlayerActionData.EAppliesToInputType.KbdMouseOnly, false, false, false, true);
		this.Fullscreen = base.CreatePlayerAction("Fullscreen");
		this.Fullscreen.UserData = new PlayerActionData.ActionUserData("inpActFullscreenName", null, PlayerActionData.GroupGlobalFunctions, PlayerActionData.EAppliesToInputType.KbdMouseOnly, false, false, false, true);
		this.SwitchView = base.CreatePlayerAction("TVP");
		this.SwitchView.UserData = new PlayerActionData.ActionUserData("inpActSwitchViewName", null, PlayerActionData.GroupGlobalFunctions, PlayerActionData.EAppliesToInputType.KbdMouseOnly, false, false, false, true);
		this.DebugSpawn = base.CreatePlayerAction("DebugSpawn");
		this.DebugSpawn.UserData = new PlayerActionData.ActionUserData("inpActDebugSpawnName", null, PlayerActionData.GroupGlobalFunctions, PlayerActionData.EAppliesToInputType.KbdMouseOnly, false, false, false, true);
		this.DebugGameEvent = base.CreatePlayerAction("DebugGameEvent");
		this.DebugGameEvent.UserData = new PlayerActionData.ActionUserData("inpActDebugGameEventName", null, PlayerActionData.GroupGlobalFunctions, PlayerActionData.EAppliesToInputType.KbdMouseOnly, false, false, false, true);
		this.SwitchHUD = base.CreatePlayerAction("SwitchHUD");
		this.SwitchHUD.UserData = new PlayerActionData.ActionUserData("inpActSwitchHUDName", null, PlayerActionData.GroupGlobalFunctions, PlayerActionData.EAppliesToInputType.KbdMouseOnly, false, false, false, true);
		this.ShowFPS = base.CreatePlayerAction("ShowFPS");
		this.ShowFPS.UserData = new PlayerActionData.ActionUserData("inpActShowFPSName", null, PlayerActionData.GroupGlobalFunctions, PlayerActionData.EAppliesToInputType.KbdMouseOnly, false, false, false, true);
		this.Screenshot = base.CreatePlayerAction("Screenshot");
		this.Screenshot.UserData = new PlayerActionData.ActionUserData("inpActScreenshotName", null, PlayerActionData.GroupGlobalFunctions, PlayerActionData.EAppliesToInputType.KbdMouseOnly, false, false, false, true);
		this.DebugScreenshot = base.CreatePlayerAction("DebugScreenshot");
		this.DebugScreenshot.UserData = new PlayerActionData.ActionUserData("inpActDebugScreenshotName", null, PlayerActionData.GroupGlobalFunctions, PlayerActionData.EAppliesToInputType.KbdMouseOnly, false, false, false, true);
		this.BackgroundedScreenshot = base.CreatePlayerAction("BackgroundedScreenshot");
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CreateDefaultKeyboardBindings()
	{
		if (!Submission.Enabled)
		{
			this.Console.AddDefaultBinding(new Key[]
			{
				Key.F1
			});
			this.ShowDebugData.AddDefaultBinding(new Key[]
			{
				Key.F3
			});
			this.Fullscreen.AddDefaultBinding(new Key[]
			{
				Key.F4
			});
			this.SwitchView.AddDefaultBinding(new Key[]
			{
				Key.F5
			});
			this.DebugSpawn.AddDefaultBinding(new Key[]
			{
				Key.F6
			});
			this.DebugGameEvent.AddDefaultBinding(new Key[]
			{
				Key.F6,
				Key.Shift
			});
			this.SwitchHUD.AddDefaultBinding(new Key[]
			{
				Key.F7
			});
			this.ShowFPS.AddDefaultBinding(new Key[]
			{
				Key.F8
			});
			this.Screenshot.AddDefaultBinding(new Key[]
			{
				Key.F9
			});
			this.BackgroundedScreenshot.AddDefaultBinding(new Key[]
			{
				Key.F10
			});
			this.DebugScreenshot.AddDefaultBinding(new Key[]
			{
				Key.F11
			});
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CreateDefaultJoystickBindings()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static PlayerActionsGlobal m_Instance;

	public PlayerAction Console;

	public PlayerAction ShowDebugData;

	public PlayerAction Fullscreen;

	public PlayerAction SwitchView;

	public PlayerAction DebugSpawn;

	public PlayerAction DebugGameEvent;

	public PlayerAction SwitchHUD;

	public PlayerAction ShowFPS;

	public PlayerAction Screenshot;

	public PlayerAction DebugScreenshot;

	public PlayerAction BackgroundedScreenshot;
}

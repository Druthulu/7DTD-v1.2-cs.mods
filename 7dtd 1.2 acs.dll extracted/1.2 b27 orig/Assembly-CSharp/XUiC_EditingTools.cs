using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_EditingTools : XUiController
{
	public override void Init()
	{
		base.Init();
		XUiC_EditingTools.ID = base.WindowGroup.ID;
		((XUiC_SimpleButton)base.GetChildById("btnBack")).OnPressed += this.BtnBack_OnPressed;
		((XUiC_SimpleButton)base.GetChildById("btnRwgPreviewer")).OnPressed += this.BtnRwgPreviewerOnOnPressed;
		XUiC_SimpleButton xuiC_SimpleButton = (XUiC_SimpleButton)base.GetChildById("btnPrefabEditor");
		if (xuiC_SimpleButton != null)
		{
			xuiC_SimpleButton.OnPressed += this.BtnPrefabEditorOnOnPressed;
		}
		((XUiC_SimpleButton)base.GetChildById("btnWorldEditor")).OnPressed += this.BtnLevelEditorOnOnPressed;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.windowGroup.openWindowOnEsc = XUiC_MainMenu.ID;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnRwgPreviewerOnOnPressed(XUiController _sender, int _mouseButton)
	{
		base.xui.FindWindowGroupByName("rwgeditor").GetChildByType<XUiC_WorldGenerationWindowGroup>().LastWindowID = XUiC_EditingTools.ID;
		base.xui.playerUI.windowManager.Open("rwgeditor", true, false, true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnPrefabEditorOnOnPressed(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(this.windowGroup.ID);
		XUiC_EditingTools.OpenPrefabEditor(base.xui);
	}

	public static void OpenPrefabEditor(XUi xui = null)
	{
		if (xui == null)
		{
			xui = LocalPlayerUI.primaryUI.xui;
		}
		new GameModeEditWorld().ResetGamePrefs();
		GamePrefs.Set(EnumGamePrefs.GameWorld, "Empty");
		GamePrefs.Set(EnumGamePrefs.GameMode, GameModeEditWorld.TypeName);
		GamePrefs.Set(EnumGamePrefs.GameName, "PrefabEditor");
		GamePrefs.Set(EnumGamePrefs.ServerPort, 27020);
		NetworkConnectionError networkConnectionError = SingletonMonoBehaviour<ConnectionManager>.Instance.StartServers(GamePrefs.GetString(EnumGamePrefs.ServerPassword), false);
		if (networkConnectionError != NetworkConnectionError.NoError)
		{
			((XUiC_MessageBoxWindowGroup)((XUiWindowGroup)xui.playerUI.windowManager.GetWindow(XUiC_MessageBoxWindowGroup.ID)).Controller).ShowNetworkError(networkConnectionError);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnLevelEditorOnOnPressed(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(this.windowGroup.ID);
		base.xui.playerUI.windowManager.Open(XUiC_CreateWorld.ID, true, false, true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnBack_OnPressed(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(this.windowGroup.ID);
		base.xui.playerUI.windowManager.Open(XUiC_MainMenu.ID, true, false, true);
	}

	public static string ID = "";
}

using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_MainMenuPlayerName : XUiController
{
	public override void Init()
	{
		base.Init();
		XUiC_MainMenuPlayerName.ID = base.WindowGroup.ID;
		this.playerName = (XUiV_Label)base.GetChildById("mainMenuPlayerNameLabel").ViewComponent;
		this.playerName.SupportBbCode = false;
	}

	public void UpdateName()
	{
		string @string = GamePrefs.GetString(EnumGamePrefs.PlayerName);
		if (@string != string.Empty)
		{
			this.playerName.Text = @string;
		}
	}

	public static void OpenIfNotOpen(XUi _xuiInstance)
	{
		XUiC_MainMenuPlayerName childByType = _xuiInstance.FindWindowGroupByName(XUiC_MainMenuPlayerName.ID).GetChildByType<XUiC_MainMenuPlayerName>();
		_xuiInstance.playerUI.windowManager.OpenIfNotOpen(XUiC_MainMenuPlayerName.ID, false, true, true);
		childByType.UpdateName();
	}

	public static void Close(XUi _xuiInstance)
	{
		_xuiInstance.playerUI.windowManager.Close(XUiC_MainMenuPlayerName.ID);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label playerName;
}

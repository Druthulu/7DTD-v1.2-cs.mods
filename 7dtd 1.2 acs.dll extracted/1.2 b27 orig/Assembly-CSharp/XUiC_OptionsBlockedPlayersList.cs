using System;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_OptionsBlockedPlayersList : XUiController
{
	public override void Init()
	{
		base.Init();
		XUiC_OptionsBlockedPlayersList.ID = base.WindowGroup.ID;
		base.WindowGroup.openWindowOnEsc = XUiC_OptionsMenu.ID;
		this.blockedPlayersList = base.GetChildByType<XUiC_BlockedPlayersList>();
		(base.GetChildById("btnBack") as XUiC_SimpleButton).OnPressed += this.BtnBack_OnPressed;
	}

	public override void OnClose()
	{
		base.OnClose();
		BlockedPlayerList instance = BlockedPlayerList.Instance;
		if (instance == null)
		{
			return;
		}
		instance.MarkForWrite();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnBack_OnPressed(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(this.windowGroup.ID);
		base.xui.playerUI.windowManager.Open(XUiC_OptionsMenu.ID, true, false, true);
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_BlockedPlayersList blockedPlayersList;
}

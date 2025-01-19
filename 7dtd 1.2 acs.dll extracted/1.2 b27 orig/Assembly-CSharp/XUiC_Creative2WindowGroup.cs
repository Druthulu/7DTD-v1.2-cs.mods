using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_Creative2WindowGroup : XUiController
{
	public override void OnOpen()
	{
		base.OnOpen();
		base.xui.playerUI.windowManager.OpenIfNotOpen("windowpaging", false, false, true);
		XUiC_WindowSelector childByType = base.xui.FindWindowGroupByName("windowpaging").GetChildByType<XUiC_WindowSelector>();
		if (childByType == null)
		{
			return;
		}
		childByType.SetSelected("creative2");
	}

	public override void OnClose()
	{
		base.OnClose();
		base.xui.playerUI.windowManager.CloseIfOpen("windowpaging");
	}
}

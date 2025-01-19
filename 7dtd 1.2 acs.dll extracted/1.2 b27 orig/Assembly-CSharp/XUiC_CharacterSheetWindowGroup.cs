using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_CharacterSheetWindowGroup : XUiController
{
	public override void Init()
	{
		base.Init();
		this.buffList = base.GetChildByType<XUiC_ActiveBuffList>();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (this.buffList != null)
		{
			this.buffList.setFirstEntry = true;
		}
		base.xui.playerUI.windowManager.OpenIfNotOpen("windowpaging", false, false, true);
		XUiC_WindowSelector childByType = base.xui.FindWindowGroupByName("windowpaging").GetChildByType<XUiC_WindowSelector>();
		if (childByType != null)
		{
			childByType.SetSelected("character");
		}
		if (base.xui.PlayerEquipment != null)
		{
			base.xui.PlayerEquipment.IsOpen = true;
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		base.xui.playerUI.windowManager.CloseIfOpen("windowpaging");
		if (base.xui.PlayerEquipment != null)
		{
			base.xui.PlayerEquipment.IsOpen = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ActiveBuffList buffList;
}

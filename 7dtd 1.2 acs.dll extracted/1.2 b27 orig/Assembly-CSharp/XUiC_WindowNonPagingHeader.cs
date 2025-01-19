using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_WindowNonPagingHeader : XUiController
{
	public override void Init()
	{
		base.Init();
		XUiController childById = base.GetChildById("lblWindowName");
		if (childById != null)
		{
			this.lblWindowName = (XUiV_Label)childById.ViewComponent;
		}
	}

	public void SetHeader(string name)
	{
		if (this.lblWindowName != null)
		{
			this.lblWindowName.Text = name;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		XUiC_FocusedBlockHealth.SetData(base.xui.playerUI, null, 0f);
		base.xui.dragAndDrop.InMenu = true;
	}

	public override void OnClose()
	{
		base.OnClose();
		base.xui.dragAndDrop.InMenu = false;
		if (base.xui.currentSelectedEntry != null)
		{
			base.xui.currentSelectedEntry.Selected = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblWindowName;
}

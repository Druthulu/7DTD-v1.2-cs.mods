using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_DialogWindowGroup : XUiController
{
	public Dialog CurrentDialog { get; [PublicizedFrom(EAccessModifier.Private)] set; }

	public override void Init()
	{
		base.Init();
		this.statementWindow = base.GetChildByType<XUiC_DialogStatementWindow>();
		this.responseWindow = base.GetChildByType<XUiC_DialogResponseList>();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		base.xui.Dialog.DialogWindowGroup = this;
		base.xui.playerUI.entityPlayer.OverrideFOV = 30f;
		base.xui.playerUI.entityPlayer.OverrideLookAt = base.xui.Dialog.Respondent.getHeadPosition();
		base.xui.playerUI.windowManager.CloseIfOpen("windowpaging");
		base.xui.playerUI.windowManager.CloseIfOpen("toolbelt");
		this.CurrentDialog = Dialog.DialogList[base.xui.Dialog.Respondent.NPCInfo.DialogID];
		this.CurrentDialog.CurrentOwner = base.xui.Dialog.Respondent;
		if (base.xui.Dialog.ReturnStatement == "" || this.CurrentDialog.CurrentStatement == null)
		{
			this.CurrentDialog.RestartDialog(base.xui.playerUI.entityPlayer);
		}
		else if (base.xui.Dialog.ReturnStatement != "")
		{
			this.CurrentDialog.CurrentStatement = this.CurrentDialog.GetStatement(base.xui.Dialog.ReturnStatement);
			this.CurrentDialog.ChildDialog = null;
		}
		base.xui.Dialog.ReturnStatement = "";
		this.statementWindow.CurrentDialog = this.CurrentDialog;
		this.responseWindow.CurrentDialog = this.CurrentDialog;
		GameManager.Instance.SetToolTipPause(base.xui.playerUI.nguiWindowManager, true);
	}

	public override void OnClose()
	{
		base.OnClose();
		base.xui.playerUI.windowManager.CloseIfOpen("questOffer");
		base.xui.playerUI.windowManager.OpenIfNotOpen("toolbelt", false, false, true);
		base.xui.Dialog.Respondent = null;
		GameManager.Instance.SetToolTipPause(base.xui.playerUI.nguiWindowManager, false);
		if (!base.xui.Dialog.keepZoomOnClose)
		{
			base.xui.playerUI.entityPlayer.OverrideFOV = -1f;
		}
	}

	public void RefreshDialog()
	{
		this.statementWindow.Refresh();
		if (this.CurrentDialog.CurrentStatement != null)
		{
			this.statementWindow.Refresh();
			this.responseWindow.Refresh();
			return;
		}
		base.xui.playerUI.windowManager.Close("dialog");
	}

	public void ShowResponseWindow(bool isVisible)
	{
		this.responseWindow.Parent.ViewComponent.IsVisible = isVisible;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_DialogStatementWindow statementWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_DialogResponseList responseWindow;
}

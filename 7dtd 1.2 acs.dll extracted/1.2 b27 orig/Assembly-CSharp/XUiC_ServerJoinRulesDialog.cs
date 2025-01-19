using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ServerJoinRulesDialog : XUiController
{
	public override void Init()
	{
		base.Init();
		XUiC_ServerJoinRulesDialog.ID = base.WindowGroup.ID;
		this.labelConfirmationText = (XUiV_Label)base.GetChildById("labelConfirmationText").ViewComponent;
		((XUiC_SimpleButton)base.GetChildById("btnSpawn")).OnPressed += this.BtnSpawn_OnPressed;
		((XUiC_SimpleButton)base.GetChildById("btnLeave")).OnPressed += this.BtnLeave_OnPressed;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		base.xui.playerUI.CursorController.SetCursorHidden(false);
		base.GetChildById("btnLeave").SelectCursorElement(false, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnLeave_OnPressed(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(this.windowGroup.ID);
		SingletonMonoBehaviour<ConnectionManager>.Instance.Disconnect();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnSpawn_OnPressed(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(this.windowGroup.ID);
		GameManager.Instance.RequestToSpawn();
	}

	public static void Show(LocalPlayerUI _playerUi, string _confirmationText)
	{
		_playerUi.xui.FindWindowGroupByName(XUiC_ServerJoinRulesDialog.ID).GetChildByType<XUiC_ServerJoinRulesDialog>().labelConfirmationText.Text = _confirmationText.Replace("\\n", "\n");
		_playerUi.windowManager.Open(XUiC_ServerJoinRulesDialog.ID, true, true, true);
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label labelConfirmationText;
}

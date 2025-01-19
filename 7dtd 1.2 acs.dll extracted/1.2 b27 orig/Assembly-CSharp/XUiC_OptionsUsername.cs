using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_OptionsUsername : XUiController
{
	public override void Init()
	{
		base.Init();
		XUiC_OptionsUsername.ID = base.WindowGroup.ID;
		this.txtUsername = (XUiC_TextInput)base.GetChildById("txtUsername");
		this.txtUsername.OnSubmitHandler += this.TxtUsername_OnSubmitHandler;
		((XUiC_SimpleButton)base.GetChildById("btnCancel")).OnPressed += this.BtnCancel_OnPressed;
		((XUiC_SimpleButton)base.GetChildById("btnOk")).OnPressed += this.BtnOk_OnPressed;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TxtUsername_OnSubmitHandler(XUiController _sender, string _text)
	{
		this.BtnOk_OnPressed(_sender, -1);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnOk_OnPressed(XUiController _sender, int _mouseButton)
	{
		GamePrefs.Set(EnumGamePrefs.PlayerName, this.txtUsername.Text);
		base.xui.playerUI.windowManager.Close(base.WindowGroup.ID);
		base.xui.playerUI.windowManager.Open(XUiC_OptionsMenu.ID, true, false, true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCancel_OnPressed(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(base.WindowGroup.ID);
		base.xui.playerUI.windowManager.Open(XUiC_OptionsMenu.ID, true, false, true);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.txtUsername.Text = GamePrefs.GetString(EnumGamePrefs.PlayerName);
		base.WindowGroup.openWindowOnEsc = XUiC_OptionsMenu.ID;
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtUsername;
}

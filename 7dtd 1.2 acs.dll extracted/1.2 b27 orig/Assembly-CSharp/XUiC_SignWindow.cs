using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SignWindow : XUiController
{
	public override void Init()
	{
		base.Init();
		XUiView xuiView = (XUiV_Button)base.GetChildById("clickable").ViewComponent;
		this.textInput = base.GetChildByType<XUiC_TextInput>();
		this.textInput.OnSubmitHandler += this.TextInput_OnSubmitHandler;
		this.textInput.SupportBbCode = false;
		xuiView.Controller.OnPress += this.closeButton_OnPress;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TextInput_OnSubmitHandler(XUiController _sender, string _text)
	{
		base.xui.playerUI.windowManager.Close(base.WindowGroup.ID);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void closeButton_OnPress(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(base.WindowGroup.ID);
	}

	public void SetTileEntitySign(ITileEntitySignable _te)
	{
		this.SignTileEntity = _te;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.textInput.Text = GeneratedTextManager.GetDisplayTextImmediately(this.SignTileEntity.GetAuthoredText(), true, GeneratedTextManager.TextFilteringMode.Filter, GeneratedTextManager.BbCodeSupportMode.NotSupported);
		base.xui.playerUI.entityPlayer.PlayOneShot("open_sign", false, false, false);
		base.xui.playerUI.CursorController.SetNavigationLockView(base.ViewComponent, null);
	}

	public override void OnClose()
	{
		base.OnClose();
		base.xui.playerUI.entityPlayer.PlayOneShot("close_sign", false, false, false);
		base.xui.playerUI.CursorController.SetNavigationLockView(null, null);
		if (GameManager.Instance.World.GetTileEntity(this.SignTileEntity.GetClrIdx(), this.SignTileEntity.ToWorldPos()).GetSelfOrFeature<ITileEntitySignable>() != this.SignTileEntity)
		{
			this.FinishClosing();
			return;
		}
		if (!this.SignTileEntity.CanRenderString(this.textInput.Text))
		{
			GameManager.ShowTooltip(base.xui.playerUI.entityPlayer, "uiInvalidCharacters", false);
			this.FinishClosing();
			return;
		}
		PersistentPlayerData playerDataFromEntityID = GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(base.xui.playerUI.entityPlayer.entityId);
		this.SignTileEntity.SetText(this.textInput.Text, true, (playerDataFromEntityID != null) ? playerDataFromEntityID.PrimaryId : null);
		GeneratedTextManager.GetDisplayText(this.SignTileEntity.GetAuthoredText(), delegate(string _)
		{
			this.FinishClosing();
		}, true, false, GeneratedTextManager.TextFilteringMode.FilterWithSafeString, GeneratedTextManager.BbCodeSupportMode.NotSupported);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FinishClosing()
	{
		this.SignTileEntity.SetUserAccessing(false);
		GameManager.Instance.TEUnlockServer(this.SignTileEntity.GetClrIdx(), this.SignTileEntity.ToWorldPos(), this.SignTileEntity.EntityId, true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ITileEntitySignable SignTileEntity;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput textInput;
}

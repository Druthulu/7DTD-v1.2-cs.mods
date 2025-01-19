using System;
using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_Chat : XUiController
{
	public override void Init()
	{
		XUiC_Chat.ID = this.windowGroup.ID;
		base.Init();
		this.cbxTarget = base.GetChildByType<XUiC_ComboBoxList<EChatType>>();
		this.txtInput = base.GetChildByType<XUiC_TextInput>();
		this.txtInput.OnSubmitHandler += this.TextInput_OnSubmitHandler;
		this.txtInput.OnInputAbortedHandler += this.TextInput_OnInputAbortedHandler;
		this.txtInput.SupportBbCode = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TextInput_OnInputAbortedHandler(XUiController _sender)
	{
		base.xui.playerUI.windowManager.Close(base.WindowGroup.ID);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TextInput_OnSubmitHandler(XUiController _sender, string _text)
	{
		if (_text.Length > 0 && _text != " ")
		{
			_text = _text.Replace('\n', ' ');
			List<int> recipientEntityIds = null;
			EChatType value = this.cbxTarget.Value;
			switch (value)
			{
			case EChatType.Friends:
				recipientEntityIds = this.entityIdsFriends;
				break;
			case EChatType.Party:
				recipientEntityIds = this.entityIdsParty;
				break;
			case EChatType.Whisper:
				throw new NotImplementedException("Whisper not yet implemented");
			}
			GameManager.Instance.ChatMessageServer(null, value, base.xui.playerUI.entityPlayer.entityId, _text, recipientEntityIds, EMessageSender.SenderIdAsPlayer);
			this.txtInput.Text = "";
		}
		base.xui.playerUI.windowManager.Close(base.WindowGroup.ID);
	}

	public override void OnOpen()
	{
		this.cbxTarget.Enabled = PermissionsManager.IsCommunicationAllowed();
		this.txtInput.Enabled = PermissionsManager.IsCommunicationAllowed();
		this.entityIdsFriends.Clear();
		this.entityIdsFriends.Add(base.xui.playerUI.entityPlayer.entityId);
		foreach (EntityPlayer entityPlayer in GameManager.Instance.World.Players.list)
		{
			if (entityPlayer.IsFriendOfLocalPlayer)
			{
				this.entityIdsFriends.Add(entityPlayer.entityId);
			}
		}
		this.entityIdsParty.Clear();
		this.entityIdsParty.Add(base.xui.playerUI.entityPlayer.entityId);
		if (base.xui.playerUI.entityPlayer.Party != null)
		{
			foreach (EntityPlayer entityPlayer2 in base.xui.playerUI.entityPlayer.Party.MemberList)
			{
				if (entityPlayer2 != base.xui.playerUI.entityPlayer)
				{
					this.entityIdsParty.Add(entityPlayer2.entityId);
				}
			}
		}
		this.cbxTarget.Elements.Clear();
		this.cbxTarget.Elements.Add(EChatType.Global);
		if (this.entityIdsFriends.Count > 1)
		{
			this.cbxTarget.Elements.Add(EChatType.Friends);
		}
		if (this.entityIdsParty.Count > 1)
		{
			this.cbxTarget.Elements.Add(EChatType.Party);
		}
		base.OnOpen();
	}

	public override void UpdateInput()
	{
		base.UpdateInput();
		if (base.xui.playerUI.playerInput != null)
		{
			if (PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard)
			{
				PlayerActionsGUI guiactions = base.xui.playerUI.playerInput.GUIActions;
				if (guiactions.Up.WasPressed)
				{
					this.cbxTarget.ChangeIndex(-1);
				}
				if (guiactions.Down.WasPressed)
				{
					this.cbxTarget.ChangeIndex(1);
				}
			}
			if (base.xui.playerUI.playerInput.PermanentActions.Cancel.WasPressed)
			{
				base.xui.playerUI.windowManager.Close(this.windowGroup.ID);
			}
		}
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<EChatType> cbxTarget;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtInput;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<int> entityIdsFriends = new List<int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<int> entityIdsParty = new List<int>();
}

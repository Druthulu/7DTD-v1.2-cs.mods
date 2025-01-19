using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PartyWindow : XUiController
{
	public override bool GetBindingValue(ref string value, string bindingName)
	{
		if (bindingName == "partyvisible")
		{
			value = ((this.player != null) ? (this.player.Party != null && !this.playerDead).ToString() : "false");
			return true;
		}
		if (bindingName == "isleader")
		{
			value = ((this.player != null) ? (this.player.Party != null && this.player.Party.Leader == this.player).ToString() : "false");
			return true;
		}
		if (bindingName == "voicevisible")
		{
			value = GamePrefs.GetBool(EnumGamePrefs.OptionsVoiceChatEnabled).ToString();
			return true;
		}
		if (!(bindingName == "voiceactive"))
		{
			return false;
		}
		value = this.voiceActive.ToString();
		return true;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.player = base.xui.playerUI.entityPlayer;
		base.RefreshBindings(true);
		this.player.PartyJoined += this.Player_PartyChanged;
		this.player.PartyChanged += this.Player_PartyChanged;
		this.player.PartyLeave += this.Player_PartyChanged;
		this.playerDead = base.xui.playerUI.entityPlayer.IsDead();
	}

	public override void OnClose()
	{
		base.OnClose();
		this.player.PartyJoined -= this.Player_PartyChanged;
		this.player.PartyChanged -= this.Player_PartyChanged;
		this.player.PartyLeave -= this.Player_PartyChanged;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (Time.time > this.updateTime && this.player != null)
		{
			this.updateTime = Time.time + 1f;
			bool flag = this.player.IsDead();
			if (flag != this.playerDead)
			{
				this.playerDead = flag;
				base.RefreshBindings(true);
			}
		}
		this.updateVoiceState();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Player_PartyChanged(Party _affectedParty, EntityPlayer _player)
	{
		base.RefreshBindings(true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateVoiceState()
	{
		if (GameManager.IsDedicatedServer)
		{
			return;
		}
		if (GameStats.GetInt(EnumGameStats.GameState) != 1)
		{
			return;
		}
		if (this.player == null)
		{
			return;
		}
		bool controlKeyPressed = InputUtils.ControlKeyPressed;
		bool flag = this.player.PlayerUI.playerInput.PermanentActions.PushToTalk.IsPressed && (!GameManager.Instance.IsEditMode() || !controlKeyPressed) && GamePrefs.GetBool(EnumGamePrefs.OptionsVoiceChatEnabled) && !this.player.PlayerUI.windowManager.IsInputActive();
		if (flag != this.voiceActive)
		{
			this.voiceActive = flag;
			base.RefreshBindings(false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal player;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool voiceActive;

	[PublicizedFrom(EAccessModifier.Private)]
	public float updateTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool playerDead;
}

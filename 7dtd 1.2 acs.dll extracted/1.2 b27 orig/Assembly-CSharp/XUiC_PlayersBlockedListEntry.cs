using System;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PlayersBlockedListEntry : XUiController
{
	public PlatformUserIdentifierAbs PlayerId { get; [PublicizedFrom(EAccessModifier.Private)] set; }

	public bool IsAlternating
	{
		set
		{
			if (value)
			{
				this.rowBG.Color = this.alternatingColor;
			}
		}
	}

	public override void Init()
	{
		base.Init();
		this.rowBG = (XUiV_Sprite)base.GetChildById("background").ViewComponent;
		this.PlayerName = (XUiC_PlayerName)base.GetChildById("playerName");
		this.btnReportPlayer = (XUiV_Button)base.GetChildById("btnReportPlayer").ViewComponent;
		this.btnReportPlayer.Controller.OnPress += this.ReportPlayerPressed;
		this.btnUnblockPlayer = (XUiV_Button)base.GetChildById("unblockBtn").ViewComponent;
		this.btnUnblockPlayer.Controller.OnPress += this.UnblockPlayerPressed;
	}

	public void UpdateEntry(PlatformUserIdentifierAbs _playerId)
	{
		BlockedPlayerList.ListEntry playerStateInfo = BlockedPlayerList.Instance.GetPlayerStateInfo(_playerId);
		if (playerStateInfo != null && playerStateInfo.Blocked)
		{
			this.PlayerId = _playerId;
			this.PlayerName.UpdatePlayerData(playerStateInfo.PlayerData, true, null);
			this.btnReportPlayer.IsVisible = true;
			this.btnUnblockPlayer.IsVisible = true;
			return;
		}
		this.Clear();
	}

	public void Clear()
	{
		this.PlayerId = null;
		this.PlayerName.ClearPlayerData();
		this.btnReportPlayer.IsVisible = false;
		this.btnUnblockPlayer.IsVisible = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ReportPlayerPressed(XUiController _sender, int _mouseButton)
	{
		if (PlatformManager.MultiPlatform.PlayerReporting != null && this.PlayerId != null)
		{
			BlockedPlayerList.ListEntry playerStateInfo = BlockedPlayerList.Instance.GetPlayerStateInfo(this.PlayerId);
			bool flag = GameStats.GetInt(EnumGameStats.GameState) != 0;
			XUiC_ReportPlayer.Open(playerStateInfo.PlayerData, flag ? "" : XUiC_OptionsBlockedPlayersList.ID);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UnblockPlayerPressed(XUiController _sender, int _mouseButton)
	{
		if (this.PlayerId == null)
		{
			return;
		}
		BlockedPlayerList.ListEntry listEntry = BlockedPlayerList.Instance.GetPlayerStateInfo(this.PlayerId) ?? null;
		if (listEntry != null && listEntry.ResolvedOnce)
		{
			listEntry.SetBlockState(false);
			this.BlockList.IsDirty = true;
		}
	}

	public override bool ParseAttribute(string name, string value, XUiController _parent)
	{
		if (name == "enabled_color")
		{
			this.enabledColor = StringParsers.ParseColor32(value);
			return true;
		}
		if (name == "disabled_color")
		{
			this.disabledColor = StringParsers.ParseColor32(value);
			return true;
		}
		if (!(name == "alternating_color"))
		{
			return base.ParseAttribute(name, value, _parent);
		}
		this.alternatingColor = StringParsers.ParseColor32(value);
		return true;
	}

	public XUiC_BlockedPlayersList BlockList;

	public XUiC_PlayerName PlayerName;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button btnReportPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button btnUnblockPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite rowBG;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color enabledColor = Color.white;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color disabledColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color alternatingColor;
}

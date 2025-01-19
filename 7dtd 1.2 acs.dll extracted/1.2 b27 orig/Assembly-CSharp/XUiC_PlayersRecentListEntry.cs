using System;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PlayersRecentListEntry : XUiController
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
		this.lblLastSeenTime = (XUiV_Label)base.GetChildById("lastSeen").ViewComponent;
		this.btnReportPlayer = (XUiV_Button)base.GetChildById("btnReportPlayer").ViewComponent;
		this.btnReportPlayer.Controller.OnPress += this.ReportPlayerPressed;
		this.btnBlockPlayer = (XUiV_Button)base.GetChildById("blockBtn").ViewComponent;
		this.btnBlockPlayer.Controller.OnPress += this.BlockPlayerPressed;
		this.btnViewProfile = (XUiV_Button)base.GetChildById("btnViewProfile").ViewComponent;
		this.btnViewProfile.Controller.OnPress += this.ViewProfilePressed;
	}

	public void UpdateEntry(PlatformUserIdentifierAbs _playerId)
	{
		BlockedPlayerList.ListEntry playerStateInfo = BlockedPlayerList.Instance.GetPlayerStateInfo(_playerId);
		if (playerStateInfo == null || playerStateInfo.Blocked)
		{
			this.Clear();
			return;
		}
		this.PlayerId = _playerId;
		this.PlayerName.UpdatePlayerData(playerStateInfo.PlayerData, true, null);
		this.lblLastSeenTime.Text = Utils.DescribeTimeSince(DateTime.UtcNow, playerStateInfo.LastSeen);
		this.btnReportPlayer.IsVisible = true;
		if (this.PlayerName.CanShowProfile())
		{
			this.btnBlockPlayer.IsVisible = false;
			this.btnViewProfile.IsVisible = true;
			return;
		}
		this.btnBlockPlayer.IsVisible = true;
		this.btnViewProfile.IsVisible = false;
	}

	public void Clear()
	{
		this.PlayerId = null;
		this.PlayerName.ClearPlayerData();
		this.lblLastSeenTime.Text = "";
		this.btnReportPlayer.IsVisible = false;
		this.btnBlockPlayer.IsVisible = false;
		this.btnViewProfile.IsVisible = false;
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
	public void BlockPlayerPressed(XUiController _sender, int _mouseButton)
	{
		if (this.PlayerId == null || this.PlayerName.CanShowProfile())
		{
			return;
		}
		BlockedPlayerList.ListEntry playerStateInfo = BlockedPlayerList.Instance.GetPlayerStateInfo(this.PlayerId);
		if (playerStateInfo != null && playerStateInfo.ResolvedOnce)
		{
			ValueTuple<bool, string> valueTuple = playerStateInfo.SetBlockState(true);
			bool item = valueTuple.Item1;
			string item2 = valueTuple.Item2;
			if (item)
			{
				this.BlockList.IsDirty = true;
				return;
			}
			if (!string.IsNullOrEmpty(item2))
			{
				this.BlockList.DisplayMessage(Localization.Get("xuiBlockedPlayersCantAddHeader", false), item2);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ViewProfilePressed(XUiController _sender, int _mouseButton)
	{
		if (this.PlayerName.CanShowProfile())
		{
			this.PlayerName.ShowProfile();
			return;
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
	public XUiV_Label lblLastSeenTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button btnReportPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button btnBlockPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button btnViewProfile;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite rowBG;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color enabledColor = Color.white;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color disabledColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color alternatingColor;
}

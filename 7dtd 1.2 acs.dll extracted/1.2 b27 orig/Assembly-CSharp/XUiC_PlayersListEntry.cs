using System;
using System.Collections.Generic;
using Audio;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PlayersListEntry : XUiController
{
	public bool ShowOnMapEnabled
	{
		set
		{
			this.buttonShowOnMap.Enabled = value;
			this.buttonShowOnMap.IsVisible = value;
			this.labelShowOnMap.IsVisible = !value;
		}
	}

	public bool IsOffline
	{
		set
		{
			this.isOffline = value;
			Color color = this.isOffline ? this.disabledColor : this.enabledColor;
			if (this.PlayerName.Color != color)
			{
				this.PlayerName.Color = color;
				this.LevelText.Color = color;
				this.GamestageText.Color = color;
				this.labelPartyIcon.Color = color;
				this.labelAllyIcon.Color = color;
				this.labelShowOnMap.Color = color;
				this.buttonAllyIcon.CurrentColor = color;
				this.DistanceToFriend.Color = color;
				this.ZombieKillsText.Color = color;
				this.PlayerKillsText.Color = color;
				this.DeathsText.Color = color;
				this.PingText.Color = color;
			}
		}
	}

	public XUiC_PlayersListEntry.EnumAllyInviteStatus AllyStatus
	{
		get
		{
			return this.m_allyStatus;
		}
		set
		{
			this.m_allyStatus = value;
			this.updateInviteStatus();
		}
	}

	public XUiC_PlayersListEntry.EnumPartyStatus PartyStatus
	{
		get
		{
			return this.m_partyStatus;
		}
		set
		{
			this.m_partyStatus = value;
			this.updatePartyStatus();
		}
	}

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

	public static string SentKeyword
	{
		get
		{
			if (XUiC_PlayersListEntry.sentKeyword == "")
			{
				XUiC_PlayersListEntry.sentKeyword = Localization.Get("xuiSent", false);
			}
			return XUiC_PlayersListEntry.sentKeyword;
		}
	}

	public static string ReceivedKeyword
	{
		get
		{
			if (XUiC_PlayersListEntry.receivedKeyword == "")
			{
				XUiC_PlayersListEntry.receivedKeyword = Localization.Get("xuiReceived", false);
			}
			return XUiC_PlayersListEntry.receivedKeyword;
		}
	}

	public static string NAKeyword
	{
		get
		{
			if (XUiC_PlayersListEntry.naKeyword == "")
			{
				XUiC_PlayersListEntry.naKeyword = Localization.Get("xuiNA", false);
			}
			return XUiC_PlayersListEntry.naKeyword;
		}
	}

	public override void Init()
	{
		base.Init();
		this.PlayerName = (XUiC_PlayerName)base.GetChildById("playerName");
		this.AdminSprite = (XUiV_Sprite)base.GetChildById("admin").ViewComponent;
		this.TwitchSprite = (XUiV_Sprite)base.GetChildById("twitch").ViewComponent;
		this.TwitchDisabledSprite = (XUiV_Sprite)base.GetChildById("twitchDisabled").ViewComponent;
		this.ZombieKillsText = (XUiV_Label)base.GetChildById("zombieKillsText").ViewComponent;
		this.PlayerKillsText = (XUiV_Label)base.GetChildById("playerKillsText").ViewComponent;
		this.DeathsText = (XUiV_Label)base.GetChildById("deathsText").ViewComponent;
		this.LevelText = (XUiV_Label)base.GetChildById("levelText").ViewComponent;
		this.PingText = (XUiV_Label)base.GetChildById("pingText").ViewComponent;
		this.GamestageText = (XUiV_Label)base.GetChildById("gamestageText").ViewComponent;
		this.Voice = (XUiV_Button)base.GetChildById("iconVoice").ViewComponent;
		this.Voice.Controller.OnPress += this.voiceChatButtonOnPress;
		this.Chat = (XUiV_Button)base.GetChildById("iconChat").ViewComponent;
		this.Chat.Controller.OnPress += this.textChatButtonOnPress;
		base.xui.OnShutdown += this.Shutdown;
		this.buttonShowOnMap = (XUiV_Button)base.GetChildById("iconShowOnMap").ViewComponent;
		this.DistanceToFriend = (XUiV_Label)base.GetChildById("labelDistanceWalked").ViewComponent;
		this.rowBG = (XUiV_Sprite)base.GetChildById("background").ViewComponent;
		this.buttonAllyIcon = (XUiV_Button)base.GetChildById("iconAllyIcon").ViewComponent;
		this.buttonAllyIcon.Controller.OnPress += this.oniconAllyIconPress;
		this.buttonPartyIcon = (XUiV_Button)base.GetChildById("iconPartyIcon").ViewComponent;
		this.buttonPartyIcon.Controller.OnPress += this.oniconPartyIconPress;
		this.buttonReportPlayer = (XUiV_Button)base.GetChildById("btnReportPlayer").ViewComponent;
		this.buttonReportPlayer.Controller.OnPress += this.onReportPlayerPress;
		this.buttonShowOnMap.Controller.OnPress += this.onShowOnMapPress;
		this.enabledColor = this.PingText.Color;
		this.labelPartyIcon = (XUiV_Label)base.GetChildById("labelPartyIcon").ViewComponent;
		this.labelAllyIcon = (XUiV_Label)base.GetChildById("labelAllyIcon").ViewComponent;
		this.labelShowOnMap = (XUiV_Label)base.GetChildById("labelShowOnMap").ViewComponent;
		PlatformUserManager.BlockedStateChanged += this.playerBlockStateChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Shutdown()
	{
		base.xui.OnShutdown -= this.Shutdown;
		PlatformUserManager.BlockedStateChanged -= this.playerBlockStateChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void oniconAllyIconPress(XUiController _sender, int _mouseButton)
	{
		switch (this.m_allyStatus)
		{
		case XUiC_PlayersListEntry.EnumAllyInviteStatus.NA:
		case XUiC_PlayersListEntry.EnumAllyInviteStatus.Received:
			this.PlayersList.AddInvitePress(this.EntityId);
			return;
		case XUiC_PlayersListEntry.EnumAllyInviteStatus.Friends:
		case XUiC_PlayersListEntry.EnumAllyInviteStatus.Sent:
		{
			List<MenuItemEntry> list = new List<MenuItemEntry>();
			MenuItemEntry menuItemEntry = new MenuItemEntry
			{
				IconName = "ui_game_symbol_x",
				Text = Localization.Get("lblRemove", false),
				IsEnabled = true,
				Tag = new object[0]
			};
			menuItemEntry.ItemClicked += this.RemoveAlly_ItemClicked;
			list.Add(menuItemEntry);
			base.xui.currentPopupMenu.SetupItems(list, new Vector2i(0, -26), this.buttonAllyIcon);
			return;
		}
		default:
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RemoveAlly_ItemClicked(MenuItemEntry entry)
	{
		if (this.PlayerData != null)
		{
			this.PlayersList.RemoveInvitePress(this.PlayerData);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void oniconPartyIconPress(XUiController _sender, int _mouseButton)
	{
		if (GameStats.GetBool(EnumGameStats.AutoParty))
		{
			return;
		}
		EntityPlayerLocal entityPlayer = base.xui.playerUI.entityPlayer;
		switch (this.m_partyStatus)
		{
		case XUiC_PlayersListEntry.EnumPartyStatus.LocalPlayer_InParty:
		{
			List<MenuItemEntry> list = new List<MenuItemEntry>();
			MenuItemEntry menuItemEntry = new MenuItemEntry
			{
				IconName = "ui_game_symbol_x",
				Text = Localization.Get("lblLeave", false),
				IsEnabled = true,
				Tag = new object[0]
			};
			menuItemEntry.ItemClicked += this.LeaveParty_ItemClicked;
			list.Add(menuItemEntry);
			base.xui.currentPopupMenu.SetupItems(list, new Vector2i(0, -26), this.buttonPartyIcon);
			return;
		}
		case XUiC_PlayersListEntry.EnumPartyStatus.LocalPlayer_InPartyAsLead:
		{
			List<MenuItemEntry> list2 = new List<MenuItemEntry>();
			MenuItemEntry menuItemEntry2 = new MenuItemEntry
			{
				IconName = "ui_game_symbol_x",
				Text = Localization.Get("lblLeave", false),
				IsEnabled = true,
				Tag = new object[0]
			};
			menuItemEntry2.ItemClicked += this.LeaveParty_ItemClicked;
			list2.Add(menuItemEntry2);
			base.xui.currentPopupMenu.SetupItems(list2, new Vector2i(0, -26), this.buttonPartyIcon);
			return;
		}
		case XUiC_PlayersListEntry.EnumPartyStatus.LocalPlayer_NoParty:
		case XUiC_PlayersListEntry.EnumPartyStatus.OtherPlayer_NoPartyAsLead:
		{
			EntityPlayer entityPlayer2 = GameManager.Instance.World.GetEntity(this.EntityId) as EntityPlayer;
			if (Time.time <= this.lastTime)
			{
				GameManager.ShowTooltip(entityPlayer, string.Format(Localization.Get("ttPartyInviteWait", false), entityPlayer2.PlayerDisplayName), false);
				return;
			}
			this.lastTime = Time.time + 5f;
			if (!entityPlayer2.partyInvites.Contains(entityPlayer))
			{
				entityPlayer2.AddPartyInvite(entityPlayer.entityId);
			}
			GameManager.ShowTooltip(entityPlayer, string.Format(Localization.Get("ttPartyInviteSent", false), entityPlayer2.PlayerDisplayName), false);
			if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackagePartyActions>().Setup(NetPackagePartyActions.PartyActions.SendInvite, entityPlayer.entityId, this.EntityId, null, null), false);
				return;
			}
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackagePartyActions>().Setup(NetPackagePartyActions.PartyActions.SendInvite, entityPlayer.entityId, this.EntityId, null, null), false, -1, -1, -1, null, 192);
			return;
		}
		case XUiC_PlayersListEntry.EnumPartyStatus.LocalPlayer_Received:
		{
			EntityPlayer invitedBy = GameManager.Instance.World.GetEntity(this.EntityId) as EntityPlayer;
			Manager.PlayInsidePlayerHead("party_join", -1, 0f, false, false);
			if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackagePartyActions>().Setup(NetPackagePartyActions.PartyActions.AcceptInvite, this.EntityId, entityPlayer.entityId, null, null), false);
				return;
			}
			if (entityPlayer.Party == null)
			{
				Party.ServerHandleAcceptInvite(invitedBy, entityPlayer);
			}
			break;
		}
		case XUiC_PlayersListEntry.EnumPartyStatus.OtherPlayer_InParty:
		case XUiC_PlayersListEntry.EnumPartyStatus.OtherPlayer_InPartyIsLead:
		case XUiC_PlayersListEntry.EnumPartyStatus.OtherPlayer_NoParty:
			break;
		case XUiC_PlayersListEntry.EnumPartyStatus.OtherPlayer_InPartyAsLead:
		{
			List<MenuItemEntry> list3 = new List<MenuItemEntry>();
			MenuItemEntry menuItemEntry3 = new MenuItemEntry
			{
				IconName = "ui_game_symbol_x",
				Text = Localization.Get("lblKick", false),
				IsEnabled = true,
				Tag = new object[0]
			};
			menuItemEntry3.ItemClicked += this.KickParty_ItemClicked;
			list3.Add(menuItemEntry3);
			menuItemEntry3 = new MenuItemEntry
			{
				IconName = "server_favorite",
				Text = Localization.Get("lblMakeLeader", false),
				IsEnabled = true,
				Tag = new object[0]
			};
			menuItemEntry3.ItemClicked += this.MakeLeader_ItemClicked;
			list3.Add(menuItemEntry3);
			base.xui.currentPopupMenu.SetupItems(list3, new Vector2i(0, -26), this.buttonPartyIcon);
			return;
		}
		default:
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LeaveParty_ItemClicked(MenuItemEntry entry)
	{
		EntityPlayer entityPlayer = base.xui.playerUI.entityPlayer;
		Manager.PlayInsidePlayerHead("party_invite_leave", -1, 0f, false, false);
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackagePartyActions>().Setup(NetPackagePartyActions.PartyActions.LeaveParty, entityPlayer.entityId, this.EntityId, null, null), false);
			return;
		}
		Party.ServerHandleLeaveParty(entityPlayer, this.EntityId);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void MakeLeader_ItemClicked(MenuItemEntry entry)
	{
		EntityPlayer entityPlayer = base.xui.playerUI.entityPlayer;
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackagePartyActions>().Setup(NetPackagePartyActions.PartyActions.ChangeLead, entityPlayer.entityId, this.EntityId, null, null), false);
			return;
		}
		Party.ServerHandleChangeLead(GameManager.Instance.World.GetEntity(this.EntityId) as EntityPlayer);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void KickParty_ItemClicked(MenuItemEntry entry)
	{
		EntityPlayer entityPlayer = base.xui.playerUI.entityPlayer;
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackagePartyActions>().Setup(NetPackagePartyActions.PartyActions.KickFromParty, entityPlayer.entityId, this.EntityId, null, null), false);
			return;
		}
		Party.ServerHandleKickParty(this.EntityId);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onReportPlayerPress(XUiController _sender, int _mouseButton)
	{
		if (PlatformManager.MultiPlatform.PlayerReporting != null && this.PlayerData != null)
		{
			XUiC_ReportPlayer.Open(this.PlayerData.PlayerData, "");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateInviteStatus()
	{
		switch (this.m_allyStatus)
		{
		case XUiC_PlayersListEntry.EnumAllyInviteStatus.LocalPlayer:
			this.labelAllyIcon.IsVisible = true;
			this.buttonAllyIcon.IsVisible = false;
			return;
		case XUiC_PlayersListEntry.EnumAllyInviteStatus.NA:
			this.labelAllyIcon.IsVisible = false;
			this.buttonAllyIcon.IsVisible = true;
			this.buttonAllyIcon.Enabled = true;
			this.buttonAllyIcon.DefaultSpriteName = "ui_game_symbol_add";
			return;
		case XUiC_PlayersListEntry.EnumAllyInviteStatus.Friends:
		{
			this.labelAllyIcon.IsVisible = false;
			this.buttonAllyIcon.IsVisible = true;
			this.buttonAllyIcon.Enabled = true;
			this.buttonAllyIcon.DefaultSpriteName = "ui_game_symbol_allies";
			bool flag = this.isOffline;
			return;
		}
		case XUiC_PlayersListEntry.EnumAllyInviteStatus.Sent:
			this.labelAllyIcon.IsVisible = false;
			this.buttonAllyIcon.IsVisible = true;
			this.buttonAllyIcon.Enabled = false;
			this.buttonAllyIcon.DefaultSpriteName = "ui_game_symbol_invite";
			return;
		case XUiC_PlayersListEntry.EnumAllyInviteStatus.Received:
			this.labelAllyIcon.IsVisible = false;
			this.buttonAllyIcon.IsVisible = true;
			this.buttonAllyIcon.Enabled = true;
			this.buttonAllyIcon.DefaultSpriteName = "ui_game_symbol_invite";
			return;
		case XUiC_PlayersListEntry.EnumAllyInviteStatus.Empty:
		{
			this.buttonAllyIcon.IsVisible = false;
			this.labelAllyIcon.IsVisible = true;
			bool flag2 = this.isOffline;
			return;
		}
		default:
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updatePartyStatus()
	{
		if (GameStats.GetBool(EnumGameStats.AutoParty))
		{
			if (this.m_partyStatus == XUiC_PlayersListEntry.EnumPartyStatus.Offline)
			{
				this.buttonPartyIcon.IsVisible = false;
				this.buttonPartyIcon.DefaultSpriteName = "";
				this.buttonPartyIcon.Enabled = false;
				this.labelPartyIcon.IsVisible = true;
				return;
			}
			this.buttonPartyIcon.IsVisible = true;
			this.buttonPartyIcon.DefaultSpriteName = "ui_game_symbol_players";
			this.buttonPartyIcon.Enabled = false;
			this.labelPartyIcon.IsVisible = false;
			return;
		}
		else
		{
			switch (this.m_partyStatus)
			{
			case XUiC_PlayersListEntry.EnumPartyStatus.LocalPlayer_InParty:
				this.buttonPartyIcon.IsVisible = true;
				this.buttonPartyIcon.DefaultSpriteName = "ui_game_symbol_players";
				this.buttonPartyIcon.Enabled = true;
				this.labelPartyIcon.IsVisible = false;
				return;
			case XUiC_PlayersListEntry.EnumPartyStatus.LocalPlayer_InPartyAsLead:
				this.buttonPartyIcon.IsVisible = true;
				this.buttonPartyIcon.DefaultSpriteName = "server_favorite";
				this.buttonPartyIcon.Enabled = true;
				this.labelPartyIcon.IsVisible = false;
				return;
			case XUiC_PlayersListEntry.EnumPartyStatus.LocalPlayer_NoParty:
			case XUiC_PlayersListEntry.EnumPartyStatus.OtherPlayer_NoParty:
			case XUiC_PlayersListEntry.EnumPartyStatus.OtherPlayer_PartyFullAsLead:
			case XUiC_PlayersListEntry.EnumPartyStatus.Offline:
				this.buttonPartyIcon.IsVisible = false;
				this.buttonPartyIcon.DefaultSpriteName = "";
				this.buttonPartyIcon.Enabled = false;
				this.labelPartyIcon.IsVisible = true;
				return;
			case XUiC_PlayersListEntry.EnumPartyStatus.LocalPlayer_Received:
				this.buttonPartyIcon.IsVisible = true;
				this.buttonPartyIcon.DefaultSpriteName = "ui_game_symbol_invite";
				this.buttonPartyIcon.Enabled = true;
				this.labelPartyIcon.IsVisible = false;
				return;
			case XUiC_PlayersListEntry.EnumPartyStatus.OtherPlayer_InParty:
				this.buttonPartyIcon.IsVisible = true;
				this.buttonPartyIcon.DefaultSpriteName = "ui_game_symbol_players";
				this.buttonPartyIcon.Enabled = true;
				this.labelPartyIcon.IsVisible = false;
				return;
			case XUiC_PlayersListEntry.EnumPartyStatus.OtherPlayer_InPartyIsLead:
				this.buttonPartyIcon.IsVisible = true;
				this.buttonPartyIcon.DefaultSpriteName = "server_favorite";
				this.buttonPartyIcon.Enabled = false;
				this.labelPartyIcon.IsVisible = false;
				return;
			case XUiC_PlayersListEntry.EnumPartyStatus.OtherPlayer_InPartyAsLead:
				this.buttonPartyIcon.IsVisible = true;
				this.buttonPartyIcon.DefaultSpriteName = "ui_game_symbol_players";
				this.buttonPartyIcon.Enabled = true;
				this.labelPartyIcon.IsVisible = false;
				return;
			case XUiC_PlayersListEntry.EnumPartyStatus.OtherPlayer_NoPartyAsLead:
				this.buttonPartyIcon.IsVisible = true;
				this.buttonPartyIcon.DefaultSpriteName = "ui_game_symbol_add";
				this.buttonPartyIcon.Enabled = true;
				this.labelPartyIcon.IsVisible = false;
				return;
			case XUiC_PlayersListEntry.EnumPartyStatus.OtherPlayer_Sent:
				this.buttonPartyIcon.IsVisible = true;
				this.buttonPartyIcon.DefaultSpriteName = "ui_game_symbol_invite";
				this.buttonPartyIcon.Enabled = false;
				this.labelPartyIcon.IsVisible = false;
				return;
			default:
				return;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onShowOnMapPress(XUiController _sender, int _mouseButton)
	{
		this.PlayersList.ShowOnMap(this.EntityId);
	}

	public override bool ParseAttribute(string name, string value, XUiController _parent)
	{
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

	[PublicizedFrom(EAccessModifier.Private)]
	public void textChatButtonOnPress(XUiController _sender, int _mouseButton)
	{
		this.blockButtonPressed(EBlockType.TextChat);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void voiceChatButtonOnPress(XUiController _sender, int _mouseButton)
	{
		this.blockButtonPressed(EBlockType.VoiceChat);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void blockButtonPressed(EBlockType _blockType)
	{
		IPlatformUserBlockedData platformUserBlockedData = this.PlayerData.PlatformData.Blocked[_blockType];
		if (platformUserBlockedData.State == EUserBlockState.ByPlatform)
		{
			return;
		}
		platformUserBlockedData.Locally = !platformUserBlockedData.Locally;
	}

	public void playerBlockStateChanged(IPlatformUserData _pud, EBlockType _blockType, EUserBlockState _blockState)
	{
		if (this.PlayerData == null || !object.Equals(_pud.PrimaryId, this.PlayerData.PrimaryId))
		{
			return;
		}
		switch (_blockType)
		{
		case EBlockType.TextChat:
			this.updateBlockButton(_blockState, this.Chat, "xuiBlockChat");
			return;
		case EBlockType.VoiceChat:
			this.updateBlockButton(_blockState, this.Voice, "xuiBlockVoice");
			return;
		case EBlockType.Play:
			return;
		default:
			throw new ArgumentOutOfRangeException("_blockType", _blockType, string.Format("{0}.{1} missing implementation for {2}.{3}.", new object[]
			{
				"XUiC_PlayersListEntry",
				"playerBlockStateChanged",
				"EBlockType",
				_blockType
			}));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateBlockButton(EUserBlockState _blockState, XUiV_Button _button, string _typeLocalizationKey)
	{
		_button.ManualColors = true;
		Color currentColor;
		switch (_blockState)
		{
		case EUserBlockState.NotBlocked:
			currentColor = Color.white;
			break;
		case EUserBlockState.InGame:
			currentColor = new Color(0.8f, 0.4f, 0f);
			break;
		case EUserBlockState.ByPlatform:
			currentColor = new Color(0.8f, 0f, 0f);
			break;
		default:
			throw new ArgumentOutOfRangeException("_blockState", _blockState, null);
		}
		_button.CurrentColor = currentColor;
		_button.Enabled = (_blockState != EUserBlockState.ByPlatform);
		string format = Localization.Get(_typeLocalizationKey, false);
		string key;
		switch (_blockState)
		{
		case EUserBlockState.NotBlocked:
			key = "xuiChatNotBlocked";
			break;
		case EUserBlockState.InGame:
			key = "xuiChatBlockedInGame";
			break;
		case EUserBlockState.ByPlatform:
			key = "xuiChatBlockedByPlatform";
			break;
		default:
			throw new ArgumentOutOfRangeException("_blockState", _blockState, null);
		}
		string arg = Localization.Get(key, false);
		_button.ToolTip = string.Format(format, arg);
	}

	public XUiC_PlayersList PlayersList;

	public int EntityId;

	public PersistentPlayerData PlayerData;

	public XUiC_PlayerName PlayerName;

	public XUiV_Label ZombieKillsText;

	public XUiV_Label PlayerKillsText;

	public XUiV_Label DeathsText;

	public XUiV_Label LevelText;

	public XUiV_Label PingText;

	public XUiV_Label GamestageText;

	public XUiV_Sprite AdminSprite;

	public XUiV_Sprite TwitchSprite;

	public XUiV_Sprite TwitchDisabledSprite;

	public XUiV_Label labelPartyIcon;

	public XUiV_Label labelAllyIcon;

	public XUiV_Label labelShowOnMap;

	public bool IsFriend;

	public XUiV_Button buttonShowOnMap;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isOffline;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_PlayersListEntry.EnumAllyInviteStatus m_allyStatus;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_PlayersListEntry.EnumPartyStatus m_partyStatus;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite rowBG;

	public XUiV_Button Voice;

	public XUiV_Button Chat;

	public XUiV_Label DistanceToFriend;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button buttonPartyIcon;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button buttonAllyIcon;

	public XUiV_Button buttonReportPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color enabledColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color disabledColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color alternatingColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string sentKeyword = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public static string receivedKeyword = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public static string naKeyword = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastTime;

	public enum EnumAllyInviteStatus
	{
		LocalPlayer,
		NA,
		Friends,
		Sent,
		Received,
		Empty
	}

	public enum EnumTrackStatus
	{
		Hidden,
		NotTracked,
		Tracked
	}

	public enum EnumPartyStatus
	{
		LocalPlayer_InParty,
		LocalPlayer_InPartyAsLead,
		LocalPlayer_NoParty,
		LocalPlayer_Received,
		OtherPlayer_InParty,
		OtherPlayer_InPartyIsLead,
		OtherPlayer_InPartyAsLead,
		OtherPlayer_NoParty,
		OtherPlayer_NoPartyAsLead,
		OtherPlayer_PartyFullAsLead,
		OtherPlayer_Sent,
		Offline
	}
}

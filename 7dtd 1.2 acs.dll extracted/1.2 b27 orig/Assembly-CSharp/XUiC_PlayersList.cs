using System;
using System.Collections.Generic;
using GUI_2;
using InControl;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[UnityEngine.Scripting.Preserve]
public class XUiC_PlayersList : XUiController
{
	public override void Init()
	{
		base.Init();
		this.playersRect = (XUiV_Rect)base.GetChildById("playersRect").ViewComponent;
		this.playerList = (XUiV_Grid)base.GetChildById("playerList").ViewComponent;
		this.playerEntries = base.GetChildrenByType<XUiC_PlayersListEntry>(null);
		this.playerPager = (XUiC_Paging)base.GetChildById("playerPager");
		this.playerPager.OnPageChanged += this.updatePlayersList;
		this.blockedPlayersList = base.GetChildByType<XUiC_BlockedPlayersList>();
		this.btnPlayersListRect = (XUiV_Rect)base.GetChildById("btnPlayersListRect").ViewComponent;
		this.btnPlayersList = (XUiC_SimpleButton)base.GetChildById("btnPlayersList");
		this.btnBlockedPlayersRect = (XUiV_Rect)base.GetChildById("btnBlockedPlayersRect").ViewComponent;
		this.btnBlockedPlayers = (XUiC_SimpleButton)base.GetChildById("btnBlockedPlayers");
		if (BlockedPlayerList.Instance != null)
		{
			this.btnPlayersList.OnPressed += delegate(XUiController _, int _)
			{
				this.SwapLists();
			};
			this.btnBlockedPlayers.OnPressed += delegate(XUiController _, int _)
			{
				this.SwapLists();
			};
		}
		this.numberOfPlayers = (XUiV_Label)base.GetChildById("numberOfPlayers").ViewComponent;
		if (Application.isPlaying)
		{
			GameManager.Instance.OnLocalPlayerChanged += this.onLocalPlayerChanged;
			base.xui.OnShutdown += this.Shutdown;
		}
		for (int i = 0; i < this.playerEntries.Length; i++)
		{
			this.playerEntries[i].PlayersList = this;
			this.playerEntries[i].IsAlternating = (i % 2 == 0);
		}
		if (XUiC_PlayersList.twitchDisabled == "")
		{
			XUiC_PlayersList.twitchDisabled = Localization.Get("xuiTwitchDisabled", false);
			XUiC_PlayersList.twitchSafe = Localization.Get("xuiTwitchSafe", false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SwapLists()
	{
		if (BlockedPlayerList.Instance != null)
		{
			this.ShowHideBlockList(this.playersRect.IsVisible);
			base.xui.playerUI.CursorController.SetNavigationTargetLater(null);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Shutdown()
	{
		base.xui.OnShutdown -= this.Shutdown;
		this.onLocalPlayerChanged(null);
		GameManager.Instance.OnLocalPlayerChanged -= this.onLocalPlayerChanged;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public ~XUiC_PlayersList()
	{
		if (GameManager.Instance != null)
		{
			GameManager.Instance.OnLocalPlayerChanged -= this.onLocalPlayerChanged;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onLocalPlayerChanged(EntityPlayerLocal _localPlayer)
	{
		if (_localPlayer != null)
		{
			return;
		}
		if (this.persistentLocalPlayer != null)
		{
			this.persistentLocalPlayer.RemovePlayerEventHandler(new PersistentPlayerData.PlayerEventHandler(this.OnPlayerEventHandler));
			this.persistentLocalPlayer = null;
		}
		if (this.persistentPlayerList != null)
		{
			this.persistentPlayerList.RemovePlayerEventHandler(new PersistentPlayerData.PlayerEventHandler(this.OnListEventHandler));
			this.persistentPlayerList = null;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.ShowHideBlockList(false);
		if (!this.bOpened)
		{
			if (this.persistentPlayerList == null)
			{
				this.persistentPlayerList = GameManager.Instance.GetPersistentPlayerList();
				this.persistentPlayerList.AddPlayerEventHandler(new PersistentPlayerData.PlayerEventHandler(this.OnListEventHandler));
			}
			if (this.persistentLocalPlayer == null)
			{
				this.persistentLocalPlayer = GameManager.Instance.persistentLocalPlayer;
				this.persistentLocalPlayer.AddPlayerEventHandler(new PersistentPlayerData.PlayerEventHandler(this.OnPlayerEventHandler));
			}
		}
		this.bOpened = true;
		base.xui.playerUI.windowManager.OpenIfNotOpen("windowpaging", false, false, true);
		base.xui.calloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonSouth, "igcoSelect", XUiC_GamepadCalloutWindow.CalloutType.Menu);
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonEast, "igcoExit", XUiC_GamepadCalloutWindow.CalloutType.Menu);
		base.xui.calloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu, 0f);
		this.playerPager.Reset();
		this.updatePlayersList();
		XUiC_WindowSelector childByType = base.xui.FindWindowGroupByName("windowpaging").GetChildByType<XUiC_WindowSelector>();
		if (childByType != null)
		{
			childByType.SetSelected("players");
		}
		this.windowGroup.isEscClosable = false;
	}

	public override void OnClose()
	{
		base.OnClose();
		BlockedPlayerList instance = BlockedPlayerList.Instance;
		if (instance != null)
		{
			instance.MarkForWrite();
		}
		base.xui.playerUI.windowManager.CloseIfOpen("windowpaging");
		base.xui.calloutWindow.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
		this.bOpened = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ShowHideBlockList(bool _show)
	{
		if (BlockedPlayerList.Instance == null)
		{
			this.btnBlockedPlayersRect.IsVisible = false;
			this.btnPlayersListRect.IsVisible = false;
			this.blockedPlayersList.IsVisible = false;
			return;
		}
		if (_show)
		{
			this.playersRect.IsVisible = false;
			this.btnBlockedPlayersRect.IsVisible = false;
			this.btnPlayersListRect.IsVisible = true;
			this.blockedPlayersList.IsVisible = true;
			return;
		}
		this.playersRect.IsVisible = true;
		this.btnBlockedPlayersRect.IsVisible = true;
		this.btnPlayersListRect.IsVisible = false;
		this.blockedPlayersList.IsVisible = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updatePlayersList()
	{
		EntityPlayerLocal entityPlayer = base.xui.playerUI.entityPlayer;
		List<XUiC_PlayersList.SEntityIdRef> list = new List<XUiC_PlayersList.SEntityIdRef>();
		for (int i = 0; i < GameManager.Instance.World.Players.list.Count; i++)
		{
			EntityPlayer entityPlayer2 = GameManager.Instance.World.Players.list[i];
			list.Add(new XUiC_PlayersList.SEntityIdRef(entityPlayer2.entityId, entityPlayer2));
		}
		this.numberOfPlayers.Text = list.Count.ToString();
		this.playerPager.SetLastPageByElementsAndPageLength(list.Count, this.playerList.Rows);
		if (GameManager.Instance.persistentLocalPlayer.ACL != null)
		{
			foreach (PlatformUserIdentifierAbs userIdentifier in GameManager.Instance.persistentLocalPlayer.ACL)
			{
				PersistentPlayerData playerData = GameManager.Instance.persistentPlayers.GetPlayerData(userIdentifier);
				if (playerData != null && !(GameManager.Instance.World.GetEntity(playerData.EntityId) != null))
				{
					list.Add(new XUiC_PlayersList.SEntityIdRef(playerData));
				}
			}
		}
		list.Sort(new XUiC_PlayersList.PlayersSorter(entityPlayer));
		GameServerInfo gameServerInfo = SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer ? SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo : SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo;
		bool flag = gameServerInfo != null && gameServerInfo.AllowsCrossplay;
		if (!flag)
		{
			EPlayGroup eplayGroup = DeviceFlags.Current.ToPlayGroup();
			for (int j = 0; j < list.Count; j++)
			{
				EPlayGroup playGroup = list[j].PlayerData.PlayGroup;
				if (playGroup != EPlayGroup.Unknown && playGroup != eplayGroup)
				{
					flag = true;
					break;
				}
			}
		}
		int k;
		for (k = 0; k < this.playerList.Rows; k++)
		{
			if (k >= list.Count)
			{
				break;
			}
			int num = k + this.playerList.Rows * this.playerPager.GetPage();
			if (num >= list.Count)
			{
				break;
			}
			XUiC_PlayersListEntry xuiC_PlayersListEntry = this.playerEntries[k];
			if (xuiC_PlayersListEntry != null)
			{
				EntityPlayer @ref = list[num].Ref;
				bool flag2 = @ref != null && @ref != entityPlayer && @ref.IsInPartyOfLocalPlayer;
				bool flag3 = @ref == null || (@ref != entityPlayer && @ref.IsFriendOfLocalPlayer);
				if (!(@ref == null))
				{
					int entityId = @ref.entityId;
				}
				PersistentPlayerData persistentPlayerData = (list[num].PlayerId != null) ? GameManager.Instance.persistentPlayers.GetPlayerData(list[num].PlayerId) : GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(@ref.entityId);
				if (persistentPlayerData != null)
				{
					foreach (EBlockType eblockType in EnumUtils.Values<EBlockType>())
					{
						xuiC_PlayersListEntry.playerBlockStateChanged(persistentPlayerData.PlatformData, eblockType, persistentPlayerData.PlatformData.Blocked[eblockType].State);
					}
					if (@ref != null)
					{
						xuiC_PlayersListEntry.IsOffline = false;
						xuiC_PlayersListEntry.EntityId = @ref.entityId;
						xuiC_PlayersListEntry.PlayerData = persistentPlayerData;
						xuiC_PlayersListEntry.ViewComponent.IsVisible = true;
						xuiC_PlayersListEntry.PlayerName.UpdatePlayerData(persistentPlayerData.PlayerData, flag, persistentPlayerData.PlayerName.DisplayName);
						xuiC_PlayersListEntry.AdminSprite.IsVisible = @ref.IsAdmin;
						xuiC_PlayersListEntry.TwitchSprite.IsVisible = (@ref.TwitchEnabled && @ref.TwitchActionsEnabled == EntityPlayer.TwitchActionsStates.Enabled);
						xuiC_PlayersListEntry.TwitchDisabledSprite.IsVisible = (@ref.TwitchActionsEnabled != EntityPlayer.TwitchActionsStates.Enabled || @ref.TwitchSafe);
						xuiC_PlayersListEntry.TwitchDisabledSprite.SpriteName = ((@ref.TwitchActionsEnabled != EntityPlayer.TwitchActionsStates.Enabled) ? "ui_game_symbol_twitch_action_disabled" : "ui_game_symbol_brick");
						xuiC_PlayersListEntry.TwitchDisabledSprite.ToolTip = ((@ref.TwitchActionsEnabled != EntityPlayer.TwitchActionsStates.Enabled) ? XUiC_PlayersList.twitchDisabled : XUiC_PlayersList.twitchSafe);
						xuiC_PlayersListEntry.ZombieKillsText.Text = @ref.KilledZombies.ToString();
						xuiC_PlayersListEntry.PlayerKillsText.Text = @ref.KilledPlayers.ToString();
						xuiC_PlayersListEntry.DeathsText.Text = @ref.Died.ToString();
						xuiC_PlayersListEntry.LevelText.Text = @ref.Progression.GetLevel().ToString();
						xuiC_PlayersListEntry.GamestageText.Text = @ref.gameStage.ToString();
						xuiC_PlayersListEntry.PingText.Text = ((@ref == entityPlayer && SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer) ? "--" : ((@ref.pingToServer < 0) ? "--" : @ref.pingToServer.ToString()));
						xuiC_PlayersListEntry.Voice.IsVisible = (@ref != entityPlayer);
						xuiC_PlayersListEntry.Chat.IsVisible = (@ref != entityPlayer);
						xuiC_PlayersListEntry.IsFriend = flag3;
						xuiC_PlayersListEntry.ShowOnMapEnabled = (flag3 || flag2);
						if (flag3 || flag2)
						{
							float magnitude = (@ref.GetPosition() - entityPlayer.GetPosition()).magnitude;
							xuiC_PlayersListEntry.DistanceToFriend.Text = ValueDisplayFormatters.Distance(magnitude);
						}
						else
						{
							xuiC_PlayersListEntry.DistanceToFriend.Text = "--";
						}
						xuiC_PlayersListEntry.buttonReportPlayer.IsVisible = (PlatformManager.MultiPlatform.PlayerReporting != null && @ref != entityPlayer);
						if (@ref == entityPlayer)
						{
							xuiC_PlayersListEntry.AllyStatus = XUiC_PlayersListEntry.EnumAllyInviteStatus.LocalPlayer;
						}
						else if (flag3)
						{
							xuiC_PlayersListEntry.AllyStatus = XUiC_PlayersListEntry.EnumAllyInviteStatus.Friends;
						}
						else if (this.invitesReceivedList.Contains(persistentPlayerData.PrimaryId))
						{
							xuiC_PlayersListEntry.AllyStatus = XUiC_PlayersListEntry.EnumAllyInviteStatus.Received;
						}
						else if (this.invitesSentList.Contains(persistentPlayerData.PrimaryId))
						{
							xuiC_PlayersListEntry.AllyStatus = XUiC_PlayersListEntry.EnumAllyInviteStatus.Sent;
						}
						else
						{
							xuiC_PlayersListEntry.AllyStatus = XUiC_PlayersListEntry.EnumAllyInviteStatus.NA;
						}
						if (@ref == entityPlayer)
						{
							if (entityPlayer.partyInvites.Contains(@ref))
							{
								xuiC_PlayersListEntry.PartyStatus = XUiC_PlayersListEntry.EnumPartyStatus.LocalPlayer_Received;
							}
							else if (entityPlayer.IsInParty())
							{
								xuiC_PlayersListEntry.PartyStatus = ((entityPlayer.Party.Leader == entityPlayer) ? XUiC_PlayersListEntry.EnumPartyStatus.LocalPlayer_InPartyAsLead : XUiC_PlayersListEntry.EnumPartyStatus.LocalPlayer_InParty);
							}
							else
							{
								xuiC_PlayersListEntry.PartyStatus = XUiC_PlayersListEntry.EnumPartyStatus.LocalPlayer_NoParty;
							}
						}
						else if (entityPlayer.IsInParty())
						{
							bool flag4 = entityPlayer.IsPartyLead();
							if (entityPlayer.Party.MemberList.Contains(@ref))
							{
								if (flag4)
								{
									xuiC_PlayersListEntry.PartyStatus = XUiC_PlayersListEntry.EnumPartyStatus.OtherPlayer_InPartyAsLead;
								}
								else
								{
									xuiC_PlayersListEntry.PartyStatus = (@ref.IsPartyLead() ? XUiC_PlayersListEntry.EnumPartyStatus.OtherPlayer_InPartyIsLead : XUiC_PlayersListEntry.EnumPartyStatus.OtherPlayer_InParty);
								}
							}
							else if (@ref.IsInParty() && @ref.Party.IsFull())
							{
								xuiC_PlayersListEntry.PartyStatus = XUiC_PlayersListEntry.EnumPartyStatus.OtherPlayer_PartyFullAsLead;
							}
							else
							{
								xuiC_PlayersListEntry.PartyStatus = (flag4 ? XUiC_PlayersListEntry.EnumPartyStatus.OtherPlayer_NoPartyAsLead : XUiC_PlayersListEntry.EnumPartyStatus.OtherPlayer_NoParty);
							}
						}
						else if (entityPlayer.partyInvites.Contains(@ref))
						{
							if (@ref.IsInParty() && @ref.Party.IsFull())
							{
								xuiC_PlayersListEntry.PartyStatus = XUiC_PlayersListEntry.EnumPartyStatus.OtherPlayer_NoPartyAsLead;
								entityPlayer.partyInvites.Remove(@ref);
							}
							else
							{
								xuiC_PlayersListEntry.PartyStatus = XUiC_PlayersListEntry.EnumPartyStatus.LocalPlayer_Received;
							}
						}
						else
						{
							@ref.IsInParty();
							xuiC_PlayersListEntry.PartyStatus = XUiC_PlayersListEntry.EnumPartyStatus.OtherPlayer_NoPartyAsLead;
						}
					}
					else
					{
						xuiC_PlayersListEntry.IsOffline = true;
						xuiC_PlayersListEntry.EntityId = -1;
						xuiC_PlayersListEntry.PlayerData = persistentPlayerData;
						xuiC_PlayersListEntry.PlayerName.UpdatePlayerData(persistentPlayerData.PlayerData, flag, persistentPlayerData.PlayerName.DisplayName ?? list[num].PlayerId.CombinedString);
						xuiC_PlayersListEntry.AdminSprite.IsVisible = false;
						xuiC_PlayersListEntry.TwitchSprite.IsVisible = false;
						xuiC_PlayersListEntry.TwitchDisabledSprite.IsVisible = false;
						xuiC_PlayersListEntry.DistanceToFriend.IsVisible = true;
						xuiC_PlayersListEntry.DistanceToFriend.Text = "--";
						xuiC_PlayersListEntry.ZombieKillsText.Text = "--";
						xuiC_PlayersListEntry.PlayerKillsText.Text = "--";
						xuiC_PlayersListEntry.DeathsText.Text = "--";
						xuiC_PlayersListEntry.LevelText.Text = "--";
						xuiC_PlayersListEntry.GamestageText.Text = "--";
						xuiC_PlayersListEntry.PingText.Text = "--";
						xuiC_PlayersListEntry.Voice.IsVisible = false;
						xuiC_PlayersListEntry.Chat.IsVisible = false;
						xuiC_PlayersListEntry.IsOffline = true;
						if (@ref == entityPlayer)
						{
							xuiC_PlayersListEntry.AllyStatus = XUiC_PlayersListEntry.EnumAllyInviteStatus.LocalPlayer;
						}
						else if (flag3)
						{
							xuiC_PlayersListEntry.AllyStatus = XUiC_PlayersListEntry.EnumAllyInviteStatus.Friends;
						}
						else if (this.invitesReceivedList.Contains(persistentPlayerData.PrimaryId))
						{
							xuiC_PlayersListEntry.AllyStatus = XUiC_PlayersListEntry.EnumAllyInviteStatus.Received;
						}
						else if (this.invitesSentList.Contains(persistentPlayerData.PrimaryId))
						{
							xuiC_PlayersListEntry.AllyStatus = XUiC_PlayersListEntry.EnumAllyInviteStatus.Sent;
						}
						else
						{
							xuiC_PlayersListEntry.AllyStatus = XUiC_PlayersListEntry.EnumAllyInviteStatus.NA;
						}
						xuiC_PlayersListEntry.PartyStatus = XUiC_PlayersListEntry.EnumPartyStatus.Offline;
						xuiC_PlayersListEntry.labelPartyIcon.IsVisible = true;
						xuiC_PlayersListEntry.buttonReportPlayer.IsVisible = true;
						xuiC_PlayersListEntry.buttonShowOnMap.IsVisible = false;
						xuiC_PlayersListEntry.labelShowOnMap.IsVisible = true;
					}
					xuiC_PlayersListEntry.RefreshBindings(false);
				}
			}
		}
		while (k < this.playerList.Rows)
		{
			XUiC_PlayersListEntry xuiC_PlayersListEntry2 = this.playerEntries[k];
			if (xuiC_PlayersListEntry2 != null)
			{
				xuiC_PlayersListEntry2.EntityId = -1;
				xuiC_PlayersListEntry2.PlayerData = null;
				xuiC_PlayersListEntry2.PlayerName.ClearPlayerData();
				xuiC_PlayersListEntry2.AdminSprite.IsVisible = false;
				xuiC_PlayersListEntry2.TwitchSprite.IsVisible = false;
				xuiC_PlayersListEntry2.TwitchDisabledSprite.IsVisible = false;
				xuiC_PlayersListEntry2.ZombieKillsText.Text = string.Empty;
				xuiC_PlayersListEntry2.PlayerKillsText.Text = string.Empty;
				xuiC_PlayersListEntry2.DeathsText.Text = string.Empty;
				xuiC_PlayersListEntry2.LevelText.Text = string.Empty;
				xuiC_PlayersListEntry2.GamestageText.Text = string.Empty;
				xuiC_PlayersListEntry2.PingText.Text = string.Empty;
				xuiC_PlayersListEntry2.Voice.IsVisible = false;
				xuiC_PlayersListEntry2.Chat.IsVisible = false;
				xuiC_PlayersListEntry2.ShowOnMapEnabled = false;
				xuiC_PlayersListEntry2.DistanceToFriend.IsVisible = false;
				xuiC_PlayersListEntry2.AllyStatus = XUiC_PlayersListEntry.EnumAllyInviteStatus.Empty;
				xuiC_PlayersListEntry2.PartyStatus = XUiC_PlayersListEntry.EnumPartyStatus.Offline;
				xuiC_PlayersListEntry2.buttonReportPlayer.IsVisible = false;
				xuiC_PlayersListEntry2.labelAllyIcon.IsVisible = false;
				xuiC_PlayersListEntry2.labelPartyIcon.IsVisible = false;
				xuiC_PlayersListEntry2.labelShowOnMap.IsVisible = false;
			}
			k++;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnListEventHandler(PersistentPlayerData ppData, PersistentPlayerData otherPlayer, EnumPersistentPlayerDataReason reason)
	{
		if (otherPlayer != null && reason == EnumPersistentPlayerDataReason.Disconnected)
		{
			this.invitesReceivedList.Remove(otherPlayer.PrimaryId);
			this.invitesSentList.Remove(otherPlayer.PrimaryId);
		}
		this.updatePlayersList();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPlayerEventHandler(PersistentPlayerData ppData, PersistentPlayerData otherPlayer, EnumPersistentPlayerDataReason reason)
	{
		EntityPlayerLocal entityPlayer = base.xui.playerUI.entityPlayer;
		EntityPlayer entityPlayer2 = GameManager.Instance.World.GetEntity(otherPlayer.EntityId) as EntityPlayer;
		switch (reason)
		{
		case EnumPersistentPlayerDataReason.ACL_AcceptedInvite:
			if (this.invitesSentList.Contains(otherPlayer.PrimaryId))
			{
				GameManager.ShowTooltip(entityPlayer, "friendInviteAccepted2", entityPlayer2.PlayerDisplayName, null, null, false);
			}
			this.invitesReceivedList.Remove(otherPlayer.PrimaryId);
			this.invitesSentList.Remove(otherPlayer.PrimaryId);
			if (entityPlayer != null && entityPlayer.trackedFriendEntityIds.Contains(otherPlayer.EntityId))
			{
				entityPlayer.trackedFriendEntityIds.Remove(otherPlayer.EntityId);
			}
			break;
		case EnumPersistentPlayerDataReason.ACL_DeclinedInvite:
			GameManager.ShowTooltip(entityPlayer, "friendInviteDeclined2", entityPlayer2.PlayerDisplayName, null, null, false);
			this.invitesReceivedList.Remove(otherPlayer.PrimaryId);
			this.invitesSentList.Remove(otherPlayer.PrimaryId);
			break;
		case EnumPersistentPlayerDataReason.ACL_Removed:
			if ((entityPlayer2 != null && entityPlayer2.entityId != this.entityIdJustRemoved) || (entityPlayer2 == null && otherPlayer.PrimaryId != this.playerIdJustRemoved))
			{
				GameManager.ShowTooltip(entityPlayer, "friendRemoved2", entityPlayer2.PlayerDisplayName, null, null, false);
			}
			this.entityIdJustRemoved = -1;
			this.invitesReceivedList.Remove(otherPlayer.PrimaryId);
			this.invitesSentList.Remove(otherPlayer.PrimaryId);
			if (entityPlayer != null && entityPlayer.trackedFriendEntityIds.Contains(otherPlayer.EntityId))
			{
				entityPlayer.trackedFriendEntityIds.Remove(otherPlayer.EntityId);
			}
			break;
		}
		this.updatePlayersList();
	}

	public bool AddInvite(PlatformUserIdentifierAbs _playerId)
	{
		if (this.invitesSentList.Contains(_playerId))
		{
			GameManager.Instance.ReplyToPlayerACLInvite(_playerId, true);
			return true;
		}
		if (!this.invitesReceivedList.Contains(_playerId))
		{
			this.invitesReceivedList.Add(_playerId);
			return true;
		}
		return false;
	}

	public void AddInvitePress(int _playerId)
	{
		PersistentPlayerData playerDataFromEntityID = GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(_playerId);
		GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(base.xui.playerUI.entityPlayer.entityId);
		EntityPlayerLocal entityPlayer = base.xui.playerUI.entityPlayer;
		if (this.invitesReceivedList.Contains(playerDataFromEntityID.PrimaryId))
		{
			GameManager.Instance.ReplyToPlayerACLInvite(playerDataFromEntityID.PrimaryId, true);
			this.invitesSentList.Remove(playerDataFromEntityID.PrimaryId);
			this.invitesReceivedList.Remove(playerDataFromEntityID.PrimaryId);
			GameManager.ShowTooltip(entityPlayer, "friendInviteAccepted", ((EntityPlayer)GameManager.Instance.World.GetEntity(_playerId)).PlayerDisplayName, null, null, false);
		}
		else
		{
			GameManager.Instance.SendPlayerACLInvite(playerDataFromEntityID);
			GameManager.ShowTooltip(entityPlayer, "friendSentInvite", ((EntityPlayer)GameManager.Instance.World.GetEntity(_playerId)).PlayerDisplayName, null, null, false);
			this.invitesSentList.Add(playerDataFromEntityID.PrimaryId);
		}
		this.updatePlayersList();
	}

	public void RemoveInvitePress(PersistentPlayerData ppData)
	{
		EntityPlayer entityPlayer = GameManager.Instance.World.GetEntity(ppData.EntityId) as EntityPlayer;
		PersistentPlayerData playerDataFromEntityID = GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(base.xui.playerUI.entityPlayer.entityId);
		if (ppData != null)
		{
			if (entityPlayer != null)
			{
				if (entityPlayer.IsFriendOfLocalPlayer || (playerDataFromEntityID.ACL != null && playerDataFromEntityID.ACL.Contains(ppData.PrimaryId)))
				{
					GameManager.Instance.RemovePlayerFromACL(ppData);
				}
				else
				{
					GameManager.Instance.ReplyToPlayerACLInvite(ppData.PrimaryId, false);
					this.invitesReceivedList.Remove(ppData.PrimaryId);
					this.invitesSentList.Remove(ppData.PrimaryId);
				}
				GameManager.ShowTooltip(base.xui.playerUI.entityPlayer, "friendRemoved", entityPlayer.PlayerDisplayName, null, null, false);
				this.entityIdJustRemoved = entityPlayer.entityId;
			}
			else
			{
				this.playerIdJustRemoved = ppData.PrimaryId;
				GameManager.Instance.RemovePlayerFromACL(ppData);
				GameManager.ShowTooltip(base.xui.playerUI.entityPlayer, "friendRemoved", ppData.PlayerName.DisplayName, null, null, false);
			}
		}
		this.updatePlayersList();
	}

	public void ShowOnMap(int _playerId)
	{
		Entity entity = GameManager.Instance.World.GetEntity(_playerId);
		if (entity == null)
		{
			return;
		}
		XUiC_WindowSelector.OpenSelectorAndWindow(base.xui.playerUI.entityPlayer, "map");
		((XUiC_MapArea)base.xui.GetWindow("mapArea").Controller).PositionMapAt(entity.GetPosition());
	}

	public void TrackPlayer(int _playerId)
	{
		EntityPlayerLocal entityPlayer = base.xui.playerUI.entityPlayer;
		if (entityPlayer == null)
		{
			return;
		}
		if (!entityPlayer.trackedFriendEntityIds.Contains(_playerId))
		{
			entityPlayer.trackedFriendEntityIds.Add(_playerId);
			GameManager.ShowTooltip(entityPlayer, "friendTracked", ((EntityPlayer)GameManager.Instance.World.GetEntity(_playerId)).PlayerDisplayName, null, null, false);
		}
		else
		{
			entityPlayer.trackedFriendEntityIds.Remove(_playerId);
			GameManager.ShowTooltip(entityPlayer, "friendUntracked", ((EntityPlayer)GameManager.Instance.World.GetEntity(_playerId)).PlayerDisplayName, null, null, false);
		}
		this.updatePlayersList();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (!(DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent() && PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard)
		{
			this.btnPlayersList.Label.Text = Localization.Get("xuiBlockedPlayers", false);
			this.btnBlockedPlayers.Label.Text = Localization.Get("xuiPlayerList", false);
		}
		else
		{
			this.btnPlayersList.Label.Text = string.Format(Localization.Get("xuiBlockedPlayersHotkey", false), InControlExtensions.GetGamepadSourceString(InputControlType.Action4));
			this.btnBlockedPlayers.Label.Text = string.Format(Localization.Get("xuiPlayerListHotkey", false), InControlExtensions.GetGamepadSourceString(InputControlType.Action4));
		}
		if (this.playersRect.IsVisible)
		{
			this.updateLimiter -= _dt;
			if (this.updateLimiter < 0f)
			{
				this.updateLimiter = 1f;
				this.updatePlayersList();
			}
		}
	}

	public override void UpdateInput()
	{
		base.UpdateInput();
		if (!base.xui.playerUI.playerInput.GUIActions.Cancel.WasPressed && !base.xui.playerUI.playerInput.PermanentActions.Cancel.WasPressed)
		{
			if (base.xui.playerUI.playerInput.ActiveDevice.Action4.WasPressed && PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard)
			{
				this.SwapLists();
			}
			return;
		}
		if (base.xui.currentPopupMenu.ViewComponent.IsVisible)
		{
			base.xui.currentPopupMenu.ClearItems();
			return;
		}
		base.xui.playerUI.windowManager.CloseAllOpenWindows(null, true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Rect playersRect;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Grid playerList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_PlayersListEntry[] playerEntries;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_Paging playerPager;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_BlockedPlayersList blockedPlayersList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Rect btnPlayersListRect;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnPlayersList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Rect btnBlockedPlayersRect;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnBlockedPlayers;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<PlatformUserIdentifierAbs> invitesReceivedList = new List<PlatformUserIdentifierAbs>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<PlatformUserIdentifierAbs> invitesSentList = new List<PlatformUserIdentifierAbs>();

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label numberOfPlayers;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite reportheaderSprite;

	[PublicizedFrom(EAccessModifier.Private)]
	public PersistentPlayerData persistentLocalPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public PersistentPlayerList persistentPlayerList;

	[PublicizedFrom(EAccessModifier.Private)]
	public float updateLimiter;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string twitchDisabled = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public static string twitchSafe = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bOpened;

	[PublicizedFrom(EAccessModifier.Private)]
	public int entityIdJustRemoved = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlatformUserIdentifierAbs playerIdJustRemoved;

	[PublicizedFrom(EAccessModifier.Private)]
	public class PlayersSorter : IComparer<XUiC_PlayersList.SEntityIdRef>
	{
		public PlayersSorter(EntityPlayerLocal _localPlayer)
		{
			this.localPlayer = _localPlayer;
		}

		public int Compare(XUiC_PlayersList.SEntityIdRef _p1, XUiC_PlayersList.SEntityIdRef _p2)
		{
			if (_p1.Ref == this.localPlayer)
			{
				return -1;
			}
			if (_p2.Ref == this.localPlayer)
			{
				return 1;
			}
			if (_p1.Ref == null)
			{
				return 1;
			}
			if (_p2.Ref == null)
			{
				return -1;
			}
			if (_p1.Ref.IsFriendOfLocalPlayer && _p2.Ref.IsFriendOfLocalPlayer)
			{
				if (this.localPlayer.trackedFriendEntityIds.Contains(_p1.Ref.entityId))
				{
					return -1;
				}
				if (this.localPlayer.trackedFriendEntityIds.Contains(_p2.Ref.entityId))
				{
					return 1;
				}
				if (_p1.Ref.Progression.GetLevel() > _p2.Ref.Progression.GetLevel())
				{
					return -1;
				}
				if (_p1.Ref.Progression.GetLevel() != _p2.Ref.Progression.GetLevel())
				{
					return 1;
				}
				return 0;
			}
			else
			{
				if (_p1.Ref.IsFriendOfLocalPlayer)
				{
					return -1;
				}
				if (_p2.Ref.IsFriendOfLocalPlayer)
				{
					return 1;
				}
				if (_p1.Ref.Progression.GetLevel() > _p2.Ref.Progression.GetLevel())
				{
					return -1;
				}
				if (_p1.Ref.Progression.GetLevel() != _p2.Ref.Progression.GetLevel())
				{
					return 1;
				}
				return 0;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public EntityPlayerLocal localPlayer;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct SEntityIdRef
	{
		public SEntityIdRef(int _EntityId, EntityPlayer _Ref)
		{
			this.EntityId = _EntityId;
			this.Ref = _Ref;
			this.PlayerData = GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(_EntityId);
			PersistentPlayerData playerData = this.PlayerData;
			this.PlayerId = ((playerData != null) ? playerData.PrimaryId : null);
		}

		public SEntityIdRef(PersistentPlayerData _PlayerData)
		{
			this.EntityId = -1;
			this.Ref = null;
			this.PlayerData = _PlayerData;
			PersistentPlayerData playerData = this.PlayerData;
			this.PlayerId = ((playerData != null) ? playerData.PrimaryId : null);
		}

		public PersistentPlayerData PlayerData;

		public PlatformUserIdentifierAbs PlayerId;

		public int EntityId;

		public EntityPlayer Ref;
	}
}

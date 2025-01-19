﻿using System;
using System.Collections.Generic;
using Audio;
using UnityEngine.Scripting;

[Preserve]
public class NetPackagePartyData : NetPackage
{
	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToClient;
		}
	}

	public NetPackagePartyData Setup(Party _party, int _changedEntityID, NetPackagePartyData.PartyActions _partyAction, bool _disbandParty = false)
	{
		this.PartyID = _party.PartyID;
		this.LeaderIndex = _party.LeaderIndex;
		this.VoiceLobbyId = _party.VoiceLobbyId;
		this.partyMembers = _party.GetMemberIdArray();
		this.changedEntityID = _changedEntityID;
		this.partyAction = _partyAction;
		this.disbandParty = _disbandParty;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		this.PartyID = _br.ReadInt32();
		this.LeaderIndex = (int)_br.ReadByte();
		this.VoiceLobbyId = _br.ReadString();
		int num = _br.ReadInt32();
		this.partyMembers = new int[num];
		for (int i = 0; i < num; i++)
		{
			this.partyMembers[i] = _br.ReadInt32();
		}
		this.changedEntityID = _br.ReadInt32();
		this.partyAction = (NetPackagePartyData.PartyActions)_br.ReadByte();
		this.disbandParty = _br.ReadBoolean();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(this.PartyID);
		_bw.Write((byte)this.LeaderIndex);
		_bw.Write(this.VoiceLobbyId ?? "");
		_bw.Write(this.partyMembers.Length);
		for (int i = 0; i < this.partyMembers.Length; i++)
		{
			_bw.Write(this.partyMembers[i]);
		}
		_bw.Write(this.changedEntityID);
		_bw.Write((byte)this.partyAction);
		_bw.Write(this.disbandParty);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			return;
		}
		Party party = PartyManager.Current.GetParty(this.PartyID);
		if (party == null)
		{
			party = PartyManager.Current.CreateClientParty(_world, this.PartyID, this.LeaderIndex, this.partyMembers, this.VoiceLobbyId);
		}
		else
		{
			party.LeaderIndex = this.LeaderIndex;
			party.VoiceLobbyId = this.VoiceLobbyId;
			party.UpdateMemberList(_world, this.partyMembers);
		}
		List<EntityPlayerLocal> localPlayers = GameManager.Instance.World.GetLocalPlayers();
		if (localPlayers != null && localPlayers.Count > 0)
		{
			EntityPlayerLocal entityPlayerLocal = localPlayers[0];
			EntityPlayer entityPlayer = (EntityPlayer)_world.GetEntity(this.changedEntityID);
			if (entityPlayerLocal.Party == party)
			{
				if (this.changedEntityID != -1)
				{
					switch (this.partyAction)
					{
					case NetPackagePartyData.PartyActions.AcceptInvite:
						entityPlayer.RemoveAllPartyInvites();
						if (entityPlayer == entityPlayerLocal)
						{
							GameManager.Instance.RemovePartyInvitesFromAllPlayers(entityPlayer);
							Manager.PlayInsidePlayerHead("party_join", -1, 0f, false, false);
						}
						else
						{
							Manager.PlayInsidePlayerHead("party_member_join", -1, 0f, false, false);
						}
						break;
					case NetPackagePartyData.PartyActions.LeaveParty:
						entityPlayer.Party = null;
						if (entityPlayer != entityPlayerLocal)
						{
							Manager.PlayInsidePlayerHead("party_member_leave", -1, 0f, false, false);
							GameManager.ShowTooltip(entityPlayerLocal, string.Format(Localization.Get("ttPartyOtherLeftParty", false), entityPlayer.PlayerDisplayName), false);
						}
						break;
					case NetPackagePartyData.PartyActions.KickFromParty:
						entityPlayer.Party = null;
						if (entityPlayer != entityPlayerLocal)
						{
							Manager.PlayInsidePlayerHead("party_member_leave", -1, 0f, false, false);
							GameManager.ShowTooltip(entityPlayerLocal, string.Format(Localization.Get("ttPartyOtherKickedFromParty", false), entityPlayer.PlayerDisplayName), false);
							entityPlayerLocal.QuestJournal.RemovePlayerFromSharedWiths(entityPlayer);
						}
						break;
					case NetPackagePartyData.PartyActions.Disconnected:
						if (entityPlayer != entityPlayerLocal)
						{
							Manager.PlayInsidePlayerHead("party_member_leave", -1, 0f, false, false);
							GameManager.ShowTooltip(entityPlayerLocal, string.Format(Localization.Get("ttPartyDisconnectedFromParty", false), entityPlayer.PlayerDisplayName), false);
							entityPlayerLocal.QuestJournal.RemovePlayerFromSharedWiths(entityPlayer);
						}
						break;
					case NetPackagePartyData.PartyActions.SetVoiceLobby:
						for (int i = 0; i < party.MemberList.Count; i++)
						{
							party.MemberList[i].HandleOnPartyChanged();
						}
						break;
					}
				}
			}
			else if (entityPlayerLocal == entityPlayer)
			{
				NetPackagePartyData.PartyActions partyActions = this.partyAction;
				if (partyActions != NetPackagePartyData.PartyActions.LeaveParty)
				{
					if (partyActions == NetPackagePartyData.PartyActions.KickFromParty)
					{
						Manager.PlayInsidePlayerHead("party_leave", -1, 0f, false, false);
						GameManager.ShowTooltip(entityPlayerLocal, Localization.Get("ttPartyKickedFromParty", false), false);
					}
				}
				else
				{
					Manager.PlayInsidePlayerHead("party_leave", -1, 0f, false, false);
				}
			}
		}
		if (this.disbandParty)
		{
			party.Disband();
		}
	}

	public override int GetLength()
	{
		return 9;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int PartyID;

	[PublicizedFrom(EAccessModifier.Private)]
	public int LeaderIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public string VoiceLobbyId;

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] partyMembers;

	[PublicizedFrom(EAccessModifier.Private)]
	public int changedEntityID = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public NetPackagePartyData.PartyActions partyAction;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool disbandParty;

	public enum PartyActions
	{
		SendInvite,
		AcceptInvite,
		ChangeLead,
		LeaveParty,
		KickFromParty,
		Disconnected,
		AutoJoin,
		SetVoiceLobby
	}
}

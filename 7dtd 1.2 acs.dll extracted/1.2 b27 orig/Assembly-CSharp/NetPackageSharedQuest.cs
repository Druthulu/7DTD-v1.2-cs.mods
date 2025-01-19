using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageSharedQuest : NetPackage
{
	public NetPackageSharedQuest Setup(int _questCode, int _sharedByEntityID)
	{
		this.questCode = _questCode;
		this.sharedByEntityID = _sharedByEntityID;
		this.questEvent = NetPackageSharedQuest.SharedQuestEvents.RemoveQuest;
		return this;
	}

	public NetPackageSharedQuest Setup(int _questCode, int _sharedByEntityID, int _sharedWithEntityID, bool adding)
	{
		this.questCode = _questCode;
		this.sharedByEntityID = _sharedByEntityID;
		this.sharedWithEntityID = _sharedWithEntityID;
		this.questEvent = (adding ? NetPackageSharedQuest.SharedQuestEvents.AddSharedMember : NetPackageSharedQuest.SharedQuestEvents.RemoveSharedMember);
		return this;
	}

	public NetPackageSharedQuest Setup(int _questCode, string _questID, string _poiName, Vector3 _position, Vector3 _size, Vector3 _returnPos, int _sharedByEntityID, int _sharedWithEntityID, int _questGiverID)
	{
		this.questCode = _questCode;
		this.questID = _questID;
		this.poiName = _poiName;
		this.position = _position;
		this.size = _size;
		this.returnPos = _returnPos;
		this.sharedByEntityID = _sharedByEntityID;
		this.sharedWithEntityID = _sharedWithEntityID;
		this.questGiverID = _questGiverID;
		this.questEvent = NetPackageSharedQuest.SharedQuestEvents.ShareQuest;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		this.sharedByEntityID = _br.ReadInt32();
		this.questEvent = (NetPackageSharedQuest.SharedQuestEvents)_br.ReadByte();
		if (this.questEvent == NetPackageSharedQuest.SharedQuestEvents.ShareQuest)
		{
			this.questCode = _br.ReadInt32();
			this.questID = _br.ReadString();
			this.poiName = _br.ReadString();
			this.position = StreamUtils.ReadVector3(_br);
			this.size = StreamUtils.ReadVector3(_br);
			this.returnPos = StreamUtils.ReadVector3(_br);
			this.questGiverID = _br.ReadInt32();
			this.sharedWithEntityID = _br.ReadInt32();
			return;
		}
		if (this.questEvent == NetPackageSharedQuest.SharedQuestEvents.RemoveQuest)
		{
			this.questCode = _br.ReadInt32();
			return;
		}
		this.questCode = _br.ReadInt32();
		this.sharedWithEntityID = _br.ReadInt32();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(this.sharedByEntityID);
		_bw.Write((byte)this.questEvent);
		if (this.questEvent == NetPackageSharedQuest.SharedQuestEvents.ShareQuest)
		{
			_bw.Write(this.questCode);
			_bw.Write(this.questID);
			_bw.Write(this.poiName);
			StreamUtils.Write(_bw, this.position);
			StreamUtils.Write(_bw, this.size);
			StreamUtils.Write(_bw, this.returnPos);
			_bw.Write(this.questGiverID);
			_bw.Write(this.sharedWithEntityID);
			return;
		}
		if (this.questEvent == NetPackageSharedQuest.SharedQuestEvents.RemoveQuest)
		{
			_bw.Write(this.questCode);
			return;
		}
		_bw.Write(this.questCode);
		_bw.Write(this.sharedWithEntityID);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		switch (this.questEvent)
		{
		case NetPackageSharedQuest.SharedQuestEvents.ShareQuest:
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				GameManager.Instance.QuestShareServer(this.questCode, this.questID, this.poiName, this.position, this.size, this.returnPos, this.sharedByEntityID, this.sharedWithEntityID, this.questGiverID);
				return;
			}
			GameManager.Instance.QuestShareClient(this.questCode, this.questID, this.poiName, this.position, this.size, this.returnPos, this.sharedByEntityID, this.questGiverID, null);
			return;
		case NetPackageSharedQuest.SharedQuestEvents.RemoveQuest:
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				EntityPlayer entityPlayer = GameManager.Instance.World.GetEntity(this.sharedByEntityID) as EntityPlayer;
				if (entityPlayer != null && entityPlayer.Party != null)
				{
					for (int i = 0; i < entityPlayer.Party.MemberList.Count; i++)
					{
						EntityPlayer entityPlayer2 = entityPlayer.Party.MemberList[i];
						if (entityPlayer2 != entityPlayer)
						{
							if (entityPlayer2 is EntityPlayerLocal)
							{
								entityPlayer2.QuestJournal.RemoveSharedQuestByOwner(this.questCode);
								entityPlayer2.QuestJournal.RemoveSharedQuestEntry(this.questCode);
							}
							else
							{
								SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageSharedQuest>().Setup(this.questCode, this.sharedByEntityID), false, entityPlayer2.entityId, -1, -1, null, 192);
							}
						}
					}
					return;
				}
			}
			else
			{
				List<EntityPlayerLocal> localPlayers = GameManager.Instance.World.GetLocalPlayers();
				if (localPlayers != null && localPlayers.Count > 0)
				{
					EntityPlayerLocal entityPlayerLocal = localPlayers[0];
					entityPlayerLocal.QuestJournal.RemoveSharedQuestByOwner(this.questCode);
					entityPlayerLocal.QuestJournal.RemoveSharedQuestEntry(this.questCode);
					return;
				}
			}
			break;
		case NetPackageSharedQuest.SharedQuestEvents.AddSharedMember:
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				EntityPlayer entityPlayer3 = GameManager.Instance.World.GetEntity(this.sharedByEntityID) as EntityPlayer;
				if (entityPlayer3 != null && entityPlayer3.Party != null)
				{
					EntityPlayerLocal entityPlayerLocal2 = entityPlayer3 as EntityPlayerLocal;
					if (entityPlayerLocal2 == null)
					{
						SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageSharedQuest>().Setup(this.questCode, this.sharedByEntityID, this.sharedWithEntityID, true), false, this.sharedByEntityID, -1, -1, null, 192);
						return;
					}
					EntityPlayer entityPlayer4 = GameManager.Instance.World.GetEntity(this.sharedWithEntityID) as EntityPlayer;
					if (entityPlayer4 != null)
					{
						Quest sharedQuest = entityPlayerLocal2.QuestJournal.GetSharedQuest(this.questCode);
						if (sharedQuest != null)
						{
							sharedQuest.AddSharedWith(entityPlayer4);
							GameManager.ShowTooltip(entityPlayerLocal2, string.Format(Localization.Get("ttQuestSharedAccepted", false), sharedQuest.QuestClass.Name, entityPlayer4.PlayerDisplayName), false);
							return;
						}
					}
				}
			}
			else
			{
				EntityPlayer entityPlayer5 = GameManager.Instance.World.GetEntity(this.sharedByEntityID) as EntityPlayer;
				EntityPlayerLocal entityPlayerLocal3 = entityPlayer5 as EntityPlayerLocal;
				if (entityPlayerLocal3 != null)
				{
					EntityPlayer entityPlayer6 = GameManager.Instance.World.GetEntity(this.sharedWithEntityID) as EntityPlayer;
					if (entityPlayer6 != null)
					{
						Quest sharedQuest2 = entityPlayer5.QuestJournal.GetSharedQuest(this.questCode);
						if (sharedQuest2 != null)
						{
							sharedQuest2.AddSharedWith(entityPlayer6);
							GameManager.ShowTooltip(entityPlayerLocal3, string.Format(Localization.Get("ttQuestSharedAccepted", false), sharedQuest2.QuestClass.Name, entityPlayer6.PlayerDisplayName), false);
							return;
						}
					}
				}
			}
			break;
		case NetPackageSharedQuest.SharedQuestEvents.RemoveSharedMember:
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				EntityPlayer entityPlayer7 = GameManager.Instance.World.GetEntity(this.sharedByEntityID) as EntityPlayer;
				if (entityPlayer7 != null)
				{
					EntityPlayerLocal entityPlayerLocal4 = entityPlayer7 as EntityPlayerLocal;
					if (entityPlayerLocal4 == null)
					{
						SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageSharedQuest>().Setup(this.questCode, this.sharedByEntityID, this.sharedWithEntityID, false), false, this.sharedByEntityID, -1, -1, null, 192);
						return;
					}
					EntityPlayer entityPlayer8 = GameManager.Instance.World.GetEntity(this.sharedWithEntityID) as EntityPlayer;
					if (entityPlayer8 != null)
					{
						Quest sharedQuest3 = entityPlayerLocal4.QuestJournal.GetSharedQuest(this.questCode);
						if (sharedQuest3 != null && sharedQuest3.RemoveSharedWith(entityPlayer8))
						{
							GameManager.ShowTooltip(entityPlayerLocal4, string.Format(Localization.Get("ttQuestSharedRemoved", false), sharedQuest3.QuestClass.Name, entityPlayer8.PlayerDisplayName), false);
							return;
						}
					}
				}
			}
			else
			{
				EntityPlayer entityPlayer9 = GameManager.Instance.World.GetEntity(this.sharedByEntityID) as EntityPlayer;
				EntityPlayerLocal entityPlayerLocal5 = entityPlayer9 as EntityPlayerLocal;
				if (entityPlayerLocal5 != null)
				{
					EntityPlayer entityPlayer10 = GameManager.Instance.World.GetEntity(this.sharedWithEntityID) as EntityPlayer;
					if (entityPlayer10 != null)
					{
						Quest sharedQuest4 = entityPlayer9.QuestJournal.GetSharedQuest(this.questCode);
						if (sharedQuest4 != null && sharedQuest4.RemoveSharedWith(entityPlayer10))
						{
							GameManager.ShowTooltip(entityPlayerLocal5, string.Format(Localization.Get("ttQuestSharedRemoved", false), sharedQuest4.QuestClass.Name, entityPlayer10.PlayerDisplayName), false);
						}
					}
				}
			}
			break;
		default:
			return;
		}
	}

	public override int GetLength()
	{
		return 4;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string questID;

	[PublicizedFrom(EAccessModifier.Private)]
	public string poiName;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 position;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 size;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 returnPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public int sharedByEntityID;

	[PublicizedFrom(EAccessModifier.Private)]
	public int sharedWithEntityID;

	[PublicizedFrom(EAccessModifier.Private)]
	public int questGiverID;

	[PublicizedFrom(EAccessModifier.Private)]
	public int questCode = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public NetPackageSharedQuest.SharedQuestEvents questEvent;

	public enum SharedQuestEvents
	{
		ShareQuest,
		RemoveQuest,
		AddSharedMember,
		RemoveSharedMember
	}
}

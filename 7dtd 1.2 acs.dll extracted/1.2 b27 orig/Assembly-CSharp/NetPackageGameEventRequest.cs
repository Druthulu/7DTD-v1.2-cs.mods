using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageGameEventRequest : NetPackage
{
	public NetPackageGameEventRequest Setup(string _event, int _entityId, bool _isTwitchEvent, Vector3 _targetPos, string _extraData = "", string _tag = "", bool _crateShare = false, bool _allowRefunds = true, string _sequenceLink = "")
	{
		this.eventName = _event;
		this.entityID = _entityId;
		this.extraData = _extraData;
		this.tag = _tag;
		this.isTwitchEvent = _isTwitchEvent;
		this.crateShare = _crateShare;
		this.targetPos = _targetPos;
		this.allowRefunds = _allowRefunds;
		this.sequenceLink = _sequenceLink;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		this.eventName = _br.ReadString();
		this.entityID = _br.ReadInt32();
		this.extraData = _br.ReadString();
		this.tag = _br.ReadString();
		this.isTwitchEvent = _br.ReadBoolean();
		this.crateShare = _br.ReadBoolean();
		this.allowRefunds = _br.ReadBoolean();
		this.sequenceLink = _br.ReadString();
		this.targetPos = StreamUtils.ReadVector3(_br);
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(this.eventName);
		_bw.Write(this.entityID);
		_bw.Write(this.extraData);
		_bw.Write(this.tag);
		_bw.Write(this.isTwitchEvent);
		_bw.Write(this.crateShare);
		_bw.Write(this.allowRefunds);
		_bw.Write(this.sequenceLink);
		StreamUtils.Write(_bw, this.targetPos);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		EntityPlayer entityPlayer = GameManager.Instance.World.GetEntity(base.Sender.entityId) as EntityPlayer;
		Entity entity = GameManager.Instance.World.GetEntity(this.entityID);
		EntityPlayer entityPlayer2 = entity as EntityPlayer;
		if (entityPlayer2 == null || entityPlayer == entityPlayer2 || (entityPlayer.Party != null && entityPlayer.Party.ContainsMember(entityPlayer2)))
		{
			if (GameEventManager.Current.HandleAction(this.eventName, entityPlayer, entity, this.isTwitchEvent, this.targetPos, this.extraData, this.tag, this.crateShare, this.allowRefunds, this.sequenceLink, null))
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageGameEventResponse>().Setup(this.eventName, entity ? entity.entityId : -1, this.extraData, this.tag, NetPackageGameEventResponse.ResponseTypes.Approved, -1, -1, false), false, base.Sender.entityId, -1, -1, null, 192);
				if (this.isTwitchEvent && entityPlayer.Party != null)
				{
					for (int i = 0; i < entityPlayer.Party.MemberList.Count; i++)
					{
						EntityPlayer entityPlayer3 = entityPlayer.Party.MemberList[i];
						if (entityPlayer3 != entityPlayer && entityPlayer3.TwitchEnabled)
						{
							if (entityPlayer3 is EntityPlayerLocal)
							{
								GameEventManager.Current.HandleTwitchPartyGameEventApproved(this.eventName, entity ? entity.entityId : -1, this.extraData, this.tag);
							}
							else
							{
								SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageGameEventResponse>().Setup(this.eventName, entity ? entity.entityId : -1, this.extraData, this.tag, NetPackageGameEventResponse.ResponseTypes.TwitchPartyActionApproved, -1, -1, false), false, entityPlayer3.entityId, -1, -1, null, 192);
							}
						}
					}
					return;
				}
			}
			else
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageGameEventResponse>().Setup(this.eventName, entity ? entity.entityId : -1, this.extraData, this.tag, NetPackageGameEventResponse.ResponseTypes.Denied, -1, -1, false), false, base.Sender.entityId, -1, -1, null, 192);
			}
		}
	}

	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToServer;
		}
	}

	public override int GetLength()
	{
		return 30;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string eventName;

	[PublicizedFrom(EAccessModifier.Private)]
	public string extraData;

	[PublicizedFrom(EAccessModifier.Private)]
	public string tag;

	[PublicizedFrom(EAccessModifier.Private)]
	public int entityID = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isTwitchEvent;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool crateShare;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool allowRefunds = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 targetPos = Vector3.zero;

	[PublicizedFrom(EAccessModifier.Private)]
	public string sequenceLink = "";
}

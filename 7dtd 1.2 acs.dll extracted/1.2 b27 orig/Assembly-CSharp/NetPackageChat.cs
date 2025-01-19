﻿using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageChat : NetPackage
{
	public NetPackageChat Setup(EChatType _chatType, int _senderEntityId, string _msg, List<int> _recipientEntityIds, EMessageSender _msgSender)
	{
		this.chatType = _chatType;
		this.senderEntityId = _senderEntityId;
		this.msg = (string.IsNullOrEmpty(_msg) ? string.Empty : _msg);
		this.msgSender = _msgSender;
		this.recipientEntityIds = _recipientEntityIds;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		this.chatType = (EChatType)_br.ReadInt32();
		this.senderEntityId = _br.ReadInt32();
		this.msg = _br.ReadString();
		this.msgSender = (EMessageSender)_br.ReadInt32();
		int num = _br.ReadInt32();
		if (num > 0)
		{
			this.recipientEntityIds = new List<int>();
			for (int i = 0; i < num; i++)
			{
				this.recipientEntityIds.Add(_br.ReadInt32());
			}
		}
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write((int)this.chatType);
		_bw.Write(this.senderEntityId);
		_bw.Write(this.msg);
		_bw.Write((int)this.msgSender);
		_bw.Write((this.recipientEntityIds != null) ? this.recipientEntityIds.Count : 0);
		if (this.recipientEntityIds != null && this.recipientEntityIds.Count > 0)
		{
			for (int i = 0; i < this.recipientEntityIds.Count; i++)
			{
				_bw.Write(this.recipientEntityIds[i]);
			}
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		if (!_world.IsRemote())
		{
			GameManager.Instance.ChatMessageServer(base.Sender, this.chatType, this.senderEntityId, this.msg, this.recipientEntityIds, this.msgSender);
			return;
		}
		GameManager.Instance.ChatMessageClient(this.chatType, this.senderEntityId, this.msg, null, this.msgSender);
	}

	public override int GetLength()
	{
		int num = (this.recipientEntityIds == null) ? 0 : this.recipientEntityIds.Count;
		return 12 + this.msg.Length + 4 * num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public EChatType chatType;

	[PublicizedFrom(EAccessModifier.Private)]
	public int senderEntityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public string msg;

	[PublicizedFrom(EAccessModifier.Private)]
	public EMessageSender msgSender;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<int> recipientEntityIds;
}

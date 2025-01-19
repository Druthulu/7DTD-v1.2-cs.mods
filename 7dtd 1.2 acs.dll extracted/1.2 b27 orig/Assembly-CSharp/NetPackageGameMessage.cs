using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageGameMessage : NetPackage
{
	public NetPackageGameMessage Setup(EnumGameMessages _type, string _msg, int _mainEntityId, int _secondaryEntityId)
	{
		this.msgType = _type;
		this.msg = (string.IsNullOrEmpty(_msg) ? string.Empty : _msg);
		this.mainEntityId = _mainEntityId;
		this.secondaryEntityId = _secondaryEntityId;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		this.msgType = (EnumGameMessages)_br.ReadByte();
		this.msg = _br.ReadString();
		this.mainEntityId = _br.ReadInt32();
		this.secondaryEntityId = _br.ReadInt32();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write((byte)this.msgType);
		_bw.Write(this.msg);
		_bw.Write(this.mainEntityId);
		_bw.Write(this.secondaryEntityId);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		if (!_world.IsRemote())
		{
			GameManager.Instance.GameMessageServer(base.Sender, this.msgType, this.msg, this.mainEntityId, this.secondaryEntityId);
			return;
		}
		GameManager.Instance.DisplayGameMessage(this.msgType, this.mainEntityId, this.secondaryEntityId, true);
	}

	public override int GetLength()
	{
		return 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public EnumGameMessages msgType;

	[PublicizedFrom(EAccessModifier.Private)]
	public string msg;

	[PublicizedFrom(EAccessModifier.Private)]
	public int mainEntityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public int secondaryEntityId;
}

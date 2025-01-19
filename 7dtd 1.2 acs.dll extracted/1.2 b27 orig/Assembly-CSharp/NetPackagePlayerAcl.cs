using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackagePlayerAcl : NetPackage
{
	public NetPackagePlayerAcl Setup(PlatformUserIdentifierAbs playerId, PlatformUserIdentifierAbs otherPlayerID, EnumPersistentPlayerDataReason reason)
	{
		this.m_reason = reason;
		this.m_playerID = playerId;
		this.m_otherPlayerID = otherPlayerID;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		this.m_reason = (EnumPersistentPlayerDataReason)_reader.ReadByte();
		this.m_playerID = PlatformUserIdentifierAbs.FromStream(_reader, false, false);
		this.m_otherPlayerID = PlatformUserIdentifierAbs.FromStream(_reader, false, false);
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write((byte)this.m_reason);
		this.m_playerID.ToStream(_writer, false);
		this.m_otherPlayerID.ToStream(_writer, false);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (!base.ValidUserIdForSender(this.m_playerID))
		{
			return;
		}
		_callbacks.PersistentPlayerEvent(this.m_playerID, this.m_otherPlayerID, this.m_reason);
	}

	public override int GetLength()
	{
		return 20;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public EnumPersistentPlayerDataReason m_reason;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlatformUserIdentifierAbs m_playerID;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlatformUserIdentifierAbs m_otherPlayerID;
}

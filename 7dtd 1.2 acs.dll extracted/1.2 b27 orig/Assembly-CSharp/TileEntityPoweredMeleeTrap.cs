using System;

public class TileEntityPoweredMeleeTrap : TileEntityPoweredBlock
{
	public TileEntityPoweredMeleeTrap(Chunk _chunk) : base(_chunk)
	{
	}

	public int OwnerEntityID
	{
		get
		{
			if (this.ownerEntityID == -1)
			{
				this.SetOwnerEntityID();
			}
			return this.ownerEntityID;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			this.ownerEntityID = value;
		}
	}

	public override TileEntityType GetTileEntityType()
	{
		return TileEntityType.PowerMeleeTrap;
	}

	public bool IsOwner(PlatformUserIdentifierAbs _userIdentifier)
	{
		return _userIdentifier != null && _userIdentifier.Equals(this.ownerID);
	}

	public PlatformUserIdentifierAbs GetOwner()
	{
		return this.ownerID;
	}

	public void SetOwner(PlatformUserIdentifierAbs _userIdentifier)
	{
		this.ownerID = _userIdentifier;
		this.SetOwnerEntityID();
		this.setModified();
	}

	public override void OnSetLocalChunkPosition()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetOwnerEntityID()
	{
		this.ownerEntityID = -1;
		PersistentPlayerList persistentPlayerList = GameManager.Instance.GetPersistentPlayerList();
		PersistentPlayerData persistentPlayerData = (persistentPlayerList != null) ? persistentPlayerList.GetPlayerData(this.ownerID) : null;
		if (persistentPlayerData != null)
		{
			this.ownerEntityID = persistentPlayerData.EntityId;
		}
	}

	public override void read(PooledBinaryReader _br, TileEntity.StreamModeRead _eStreamMode)
	{
		base.read(_br, _eStreamMode);
		this.ownerID = PlatformUserIdentifierAbs.FromStream(_br, false, false);
		this.SetOwnerEntityID();
	}

	public override void write(PooledBinaryWriter _bw, TileEntity.StreamModeWrite _eStreamMode)
	{
		base.write(_bw, _eStreamMode);
		this.ownerID.ToStream(_bw, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public PlatformUserIdentifierAbs ownerID;

	[PublicizedFrom(EAccessModifier.Private)]
	public int ownerEntityID = -1;
}

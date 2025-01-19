using System;
using System.Collections.Generic;
using Platform;

public class TileEntitySecure : TileEntityLootContainer, ILockable
{
	public TileEntitySecure(Chunk _chunk) : base(_chunk)
	{
		this.allowedUserIds = new List<PlatformUserIdentifierAbs>();
		this.isLocked = false;
		this.ownerID = null;
		this.password = "";
		this.bPlayerPlaced = false;
	}

	public bool IsLocked()
	{
		return this.isLocked;
	}

	public void SetLocked(bool _isLocked)
	{
		this.isLocked = _isLocked;
		this.setModified();
	}

	public void SetOwner(PlatformUserIdentifierAbs _userIdentifier)
	{
		this.ownerID = _userIdentifier;
		this.setModified();
	}

	public PlatformUserIdentifierAbs GetOwner()
	{
		return this.ownerID;
	}

	public bool IsUserAllowed(PlatformUserIdentifierAbs _userIdentifier)
	{
		return (_userIdentifier != null && _userIdentifier.Equals(this.ownerID)) || this.allowedUserIds.Contains(_userIdentifier);
	}

	public List<PlatformUserIdentifierAbs> GetUsers()
	{
		return this.allowedUserIds;
	}

	public bool LocalPlayerIsOwner()
	{
		return this.IsOwner(PlatformManager.InternalLocalUserIdentifier);
	}

	public bool IsOwner(PlatformUserIdentifierAbs _userIdentifier)
	{
		return _userIdentifier != null && _userIdentifier.Equals(this.ownerID);
	}

	public bool HasPassword()
	{
		return !string.IsNullOrEmpty(this.password);
	}

	public string GetPassword()
	{
		return this.password;
	}

	public override void UpgradeDowngradeFrom(TileEntity _other)
	{
		base.UpgradeDowngradeFrom(_other);
		ILockable lockable = _other as ILockable;
		if (lockable != null)
		{
			this.entityId = lockable.EntityId;
			this.isLocked = lockable.IsLocked();
			this.ownerID = lockable.GetOwner();
			this.allowedUserIds = new List<PlatformUserIdentifierAbs>(lockable.GetUsers());
			this.password = lockable.GetPassword();
		}
	}

	public bool CheckPassword(string _password, PlatformUserIdentifierAbs _userIdentifier, out bool changed)
	{
		changed = false;
		if (_userIdentifier != null && _userIdentifier.Equals(this.ownerID))
		{
			if (Utils.HashString(_password) != this.password)
			{
				changed = true;
				this.password = Utils.HashString(_password);
				this.allowedUserIds.Clear();
				this.setModified();
			}
			return true;
		}
		if (Utils.HashString(_password) == this.password)
		{
			this.allowedUserIds.Add(_userIdentifier);
			this.setModified();
			return true;
		}
		return false;
	}

	public override void read(PooledBinaryReader _br, TileEntity.StreamModeRead _eStreamMode)
	{
		base.read(_br, _eStreamMode);
		if (_br.ReadInt32() > 0)
		{
			this.bPlayerPlaced = _br.ReadBoolean();
			this.isLocked = _br.ReadBoolean();
			this.ownerID = PlatformUserIdentifierAbs.FromStream(_br, false, false);
			int num = _br.ReadInt32();
			this.allowedUserIds = new List<PlatformUserIdentifierAbs>();
			for (int i = 0; i < num; i++)
			{
				this.allowedUserIds.Add(PlatformUserIdentifierAbs.FromStream(_br, false, false));
			}
			this.password = _br.ReadString();
		}
	}

	public override void write(PooledBinaryWriter _bw, TileEntity.StreamModeWrite _eStreamMode)
	{
		base.write(_bw, _eStreamMode);
		_bw.Write(1);
		_bw.Write(this.bPlayerPlaced);
		_bw.Write(this.isLocked);
		this.ownerID.ToStream(_bw, false);
		_bw.Write(this.allowedUserIds.Count);
		for (int i = 0; i < this.allowedUserIds.Count; i++)
		{
			this.allowedUserIds[i].ToStream(_bw, false);
		}
		_bw.Write(this.password);
	}

	public new int EntityId
	{
		get
		{
			return this.entityId;
		}
		set
		{
			this.entityId = value;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int ver = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isLocked;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlatformUserIdentifierAbs ownerID;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<PlatformUserIdentifierAbs> allowedUserIds;

	[PublicizedFrom(EAccessModifier.Private)]
	public string password;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bPlayerPlaced;
}

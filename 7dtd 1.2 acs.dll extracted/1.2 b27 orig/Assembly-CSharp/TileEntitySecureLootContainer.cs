using System;
using System.Collections.Generic;
using Platform;

public class TileEntitySecureLootContainer : TileEntityLootContainer, ILockable, ILockPickable
{
	public override float LootStageMod
	{
		get
		{
			return ((BlockSecureLoot)base.blockValue.Block).LootStageMod;
		}
	}

	public override float LootStageBonus
	{
		get
		{
			return ((BlockSecureLoot)base.blockValue.Block).LootStageBonus;
		}
	}

	public TileEntitySecureLootContainer(Chunk _chunk) : base(_chunk)
	{
		this.allowedUserIds = new List<PlatformUserIdentifierAbs>();
		this.isLocked = true;
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

	public bool IsUserAllowed(PlatformUserIdentifierAbs _userIdentifier)
	{
		return (_userIdentifier != null && _userIdentifier.Equals(this.ownerID)) || this.allowedUserIds.Contains(_userIdentifier);
	}

	public bool LocalPlayerIsOwner()
	{
		return this.IsOwner(PlatformManager.InternalLocalUserIdentifier);
	}

	public bool IsOwner(PlatformUserIdentifierAbs _userIdentifier)
	{
		return _userIdentifier != null && _userIdentifier.Equals(this.ownerID);
	}

	public PlatformUserIdentifierAbs GetOwner()
	{
		return this.ownerID;
	}

	public bool HasPassword()
	{
		return !string.IsNullOrEmpty(this.password);
	}

	public string GetPassword()
	{
		return this.password;
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

	public override TileEntityType GetTileEntityType()
	{
		return TileEntityType.SecureLoot;
	}

	public override void read(PooledBinaryReader _br, TileEntity.StreamModeRead _eStreamMode)
	{
		base.read(_br, _eStreamMode);
		_br.ReadInt32();
		this.bPlayerPlaced = _br.ReadBoolean();
		this.isLocked = _br.ReadBoolean();
		this.ownerID = PlatformUserIdentifierAbs.FromStream(_br, false, false);
		this.password = _br.ReadString();
		this.allowedUserIds = new List<PlatformUserIdentifierAbs>();
		int num = _br.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			this.allowedUserIds.Add(PlatformUserIdentifierAbs.FromStream(_br, false, false));
		}
	}

	public override void write(PooledBinaryWriter _bw, TileEntity.StreamModeWrite _eStreamMode)
	{
		base.write(_bw, _eStreamMode);
		_bw.Write(1);
		_bw.Write(this.bPlayerPlaced);
		_bw.Write(this.isLocked);
		this.ownerID.ToStream(_bw, false);
		_bw.Write(this.password);
		_bw.Write(this.allowedUserIds.Count);
		for (int i = 0; i < this.allowedUserIds.Count; i++)
		{
			this.allowedUserIds[i].ToStream(_bw, false);
		}
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

	public override void UpgradeDowngradeFrom(TileEntity _other)
	{
		base.UpgradeDowngradeFrom(_other);
		if (_other is ILockable)
		{
			ILockable lockable = _other as ILockable;
			this.EntityId = lockable.EntityId;
			this.SetLocked(lockable.IsLocked());
			this.SetOwner(lockable.GetOwner());
			this.allowedUserIds = new List<PlatformUserIdentifierAbs>(lockable.GetUsers());
			this.password = lockable.GetPassword();
			this.setModified();
		}
	}

	public List<PlatformUserIdentifierAbs> GetUsers()
	{
		return this.allowedUserIds;
	}

	public void ShowLockpickUi(EntityPlayerLocal _player)
	{
		if (_player != null)
		{
			BlockSecureLoot blockSecureLoot = base.blockValue.Block as BlockSecureLoot;
			if (blockSecureLoot != null)
			{
				blockSecureLoot.ShowLockpickUi(this, _player);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int ver = 1;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool isLocked;

	[PublicizedFrom(EAccessModifier.Protected)]
	public PlatformUserIdentifierAbs ownerID;

	[PublicizedFrom(EAccessModifier.Protected)]
	public List<PlatformUserIdentifierAbs> allowedUserIds;

	public float PickTimeLeft = -1f;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string password;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool bPlayerPlaced;
}

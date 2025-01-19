using System;
using UnityEngine;

public class TileEntityLandClaim : TileEntity
{
	public bool ShowBounds
	{
		get
		{
			return this.showBounds;
		}
		set
		{
			this.showBounds = value;
			base.SetModified();
		}
	}

	public TileEntityLandClaim(Chunk _chunk) : base(_chunk)
	{
		this.ownerID = null;
	}

	public void SetOwner(PlatformUserIdentifierAbs _userIdentifier)
	{
		this.ownerID = _userIdentifier;
		this.setModified();
	}

	public bool IsUserAllowed(PlatformUserIdentifierAbs _userIdentifier)
	{
		return _userIdentifier != null && _userIdentifier.Equals(this.ownerID);
	}

	public bool IsOwner(PlatformUserIdentifierAbs _userIdentifier)
	{
		return _userIdentifier != null && _userIdentifier.Equals(this.ownerID);
	}

	public PlatformUserIdentifierAbs GetOwner()
	{
		return this.ownerID;
	}

	public override void read(PooledBinaryReader _br, TileEntity.StreamModeRead _eStreamMode)
	{
		base.read(_br, _eStreamMode);
		_br.ReadInt32();
		this.ownerID = PlatformUserIdentifierAbs.FromStream(_br, false, false);
		this.showBounds = _br.ReadBoolean();
	}

	public override void write(PooledBinaryWriter _bw, TileEntity.StreamModeWrite _eStreamMode)
	{
		base.write(_bw, _eStreamMode);
		_bw.Write(0);
		this.ownerID.ToStream(_bw, false);
		_bw.Write(this.showBounds);
	}

	public override TileEntity Clone()
	{
		return new TileEntityLandClaim(this.chunk)
		{
			localChunkPos = base.localChunkPos,
			ownerID = this.ownerID,
			showBounds = this.showBounds
		};
	}

	public override void CopyFrom(TileEntity _other)
	{
		TileEntityLandClaim tileEntityLandClaim = (TileEntityLandClaim)_other;
		base.localChunkPos = tileEntityLandClaim.localChunkPos;
		this.ownerID = tileEntityLandClaim.ownerID;
		this.showBounds = tileEntityLandClaim.ShowBounds;
	}

	public int GetEntityID()
	{
		return this.entityId;
	}

	public void SetEntityID(int _entityID)
	{
		this.entityId = _entityID;
	}

	public override void UpdateTick(World world)
	{
		base.UpdateTick(world);
		if (this.BoundsHelper != null)
		{
			this.BoundsHelper.localPosition = base.ToWorldPos().ToVector3() - Origin.position + new Vector3(0.5f, 0.5f, 0.5f);
		}
	}

	public override TileEntityType GetTileEntityType()
	{
		return TileEntityType.LandClaim;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int ver = 0;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlatformUserIdentifierAbs ownerID;

	public Transform BoundsHelper;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showBounds;
}

using System;

public class TileEntityTrader : TileEntity
{
	public TileEntityTrader(Chunk _chunk) : base(_chunk)
	{
		this.TraderData = new TraderData();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public TileEntityTrader(TileEntityTrader _other) : base(null)
	{
		this.bUserAccessing = _other.bUserAccessing;
		this.TraderData = new TraderData(_other.TraderData);
	}

	public override TileEntity Clone()
	{
		return new TileEntityTrader(this);
	}

	public override void read(PooledBinaryReader _br, TileEntity.StreamModeRead _eStreamMode)
	{
		base.read(_br, _eStreamMode);
		_br.ReadInt32();
		if (this.TraderData == null)
		{
			this.TraderData = new TraderData();
		}
		this.TraderData.Read(0, _br);
		this.syncNeeded = false;
	}

	public override void write(PooledBinaryWriter _bw, TileEntity.StreamModeWrite _eStreamMode)
	{
		base.write(_bw, _eStreamMode);
		_bw.Write(1);
		this.TraderData.Write(_bw);
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

	public override TileEntityType GetTileEntityType()
	{
		return TileEntityType.Trader;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int ver = 1;

	public TraderData TraderData;

	public bool syncNeeded = true;
}

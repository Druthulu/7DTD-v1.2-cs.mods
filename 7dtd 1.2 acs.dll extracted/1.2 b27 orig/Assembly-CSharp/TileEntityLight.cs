using System;
using UnityEngine;

public class TileEntityLight : TileEntity
{
	public TileEntityLight(Chunk _chunk) : base(_chunk)
	{
	}

	public override TileEntityType GetTileEntityType()
	{
		return TileEntityType.Light;
	}

	public override TileEntity Clone()
	{
		return new TileEntityLight(this.chunk)
		{
			localChunkPos = base.localChunkPos,
			LightType = this.LightType,
			LightIntensity = this.LightIntensity,
			LightRange = this.LightRange,
			LightColor = this.LightColor,
			LightAngle = this.LightAngle,
			LightShadows = this.LightShadows,
			LightState = this.LightState,
			Rate = this.Rate,
			Delay = this.Delay
		};
	}

	public override void CopyFrom(TileEntity _other)
	{
		TileEntityLight tileEntityLight = (TileEntityLight)_other;
		base.localChunkPos = tileEntityLight.localChunkPos;
		this.LightType = tileEntityLight.LightType;
		this.LightIntensity = tileEntityLight.LightIntensity;
		this.LightRange = tileEntityLight.LightRange;
		this.LightColor = tileEntityLight.LightColor;
		this.LightAngle = tileEntityLight.LightAngle;
		this.LightShadows = tileEntityLight.LightShadows;
		this.LightState = tileEntityLight.LightState;
		this.Rate = tileEntityLight.Rate;
		this.Delay = tileEntityLight.Delay;
	}

	public override void read(PooledBinaryReader _br, TileEntity.StreamModeRead _eStreamMode)
	{
		base.read(_br, _eStreamMode);
		this.LightIntensity = _br.ReadSingle();
		this.LightRange = _br.ReadSingle();
		this.LightColor = StreamUtils.ReadColor32(_br);
		if (_eStreamMode != TileEntity.StreamModeRead.Persistency || this.readVersion > 4)
		{
			this.LightType = (LightType)_br.ReadByte();
			this.LightAngle = _br.ReadSingle();
			this.LightShadows = (LightShadows)_br.ReadByte();
		}
		if (_eStreamMode != TileEntity.StreamModeRead.Persistency || this.readVersion > 5)
		{
			this.LightState = (LightStateType)_br.ReadByte();
		}
		if (_eStreamMode != TileEntity.StreamModeRead.Persistency || this.readVersion > 6)
		{
			this.Rate = _br.ReadSingle();
		}
		if (_eStreamMode != TileEntity.StreamModeRead.Persistency || this.readVersion > 7)
		{
			this.Delay = _br.ReadSingle();
		}
	}

	public override void write(PooledBinaryWriter _bw, TileEntity.StreamModeWrite _eStreamMode)
	{
		base.write(_bw, _eStreamMode);
		_bw.Write(this.LightIntensity);
		_bw.Write(this.LightRange);
		StreamUtils.WriteColor32(_bw, this.LightColor);
		_bw.Write((byte)this.LightType);
		_bw.Write(this.LightAngle);
		_bw.Write((byte)this.LightShadows);
		_bw.Write((byte)this.LightState);
		_bw.Write(this.Rate);
		_bw.Write(this.Delay);
	}

	public LightType LightType = LightType.Point;

	public LightShadows LightShadows;

	public float LightIntensity = 1f;

	public float LightRange = 10f;

	public float LightAngle = 45f;

	public Color LightColor = Color.white;

	public LightStateType LightState;

	public float Rate = 1f;

	public float Delay = 1f;
}

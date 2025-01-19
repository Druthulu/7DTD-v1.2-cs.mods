using System;
using System.IO;

public class BlockChangeInfo
{
	public BlockChangeInfo()
	{
		this.pos = Vector3i.zero;
		this.blockValue = BlockValue.Air;
		this.density = MarchingCubes.DensityAir;
		this.changedByEntityId = -1;
	}

	public BlockChangeInfo(int _clrIdx, Vector3i _pos, BlockValue _blockValue)
	{
		this.pos = _pos;
		this.blockValue = _blockValue;
		this.bChangeBlockValue = true;
		this.bUpdateLight = false;
		this.clrIdx = _clrIdx;
	}

	public BlockChangeInfo(Vector3i _blockPos, BlockValue _blockValue, bool _updateLight, bool _bOnlyDamage = false) : this(0, _blockPos, _blockValue, _updateLight)
	{
		this.bChangeDamage = _bOnlyDamage;
	}

	public BlockChangeInfo(int _clrIdx, Vector3i _pos, BlockValue _blockValue, int _changedEntityId) : this(_clrIdx, _pos, _blockValue)
	{
		this.changedByEntityId = _changedEntityId;
	}

	public BlockChangeInfo(Vector3i _blockPos, BlockValue _blockValue, sbyte _density) : this(0, _blockPos, _blockValue, _density)
	{
	}

	public BlockChangeInfo(int _x, int _y, int _z, BlockValue _blockValue, bool _updateLight) : this(0, new Vector3i(_x, _y, _z), _blockValue, _updateLight)
	{
	}

	public BlockChangeInfo(int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, bool _updateLight) : this(_clrIdx, _blockPos, _blockValue)
	{
		this.bUpdateLight = _updateLight;
	}

	public BlockChangeInfo(int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, bool _updateLight, int _changingEntityId) : this(_clrIdx, _blockPos, _blockValue, _updateLight)
	{
		this.changedByEntityId = _changingEntityId;
	}

	public BlockChangeInfo(int _clrIdx, Vector3i _blockPos, sbyte _density, bool _bForceDensityChange = false)
	{
		this.clrIdx = _clrIdx;
		this.pos = _blockPos;
		this.density = _density;
		this.bChangeDensity = true;
		this.bForceDensityChange = _bForceDensityChange;
		this.changedByEntityId = -1;
	}

	public BlockChangeInfo(int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, sbyte _density)
	{
		this.pos = _blockPos;
		this.blockValue = _blockValue;
		this.bChangeBlockValue = true;
		this.density = _density;
		this.bChangeDensity = true;
		this.bUpdateLight = true;
		this.clrIdx = _clrIdx;
		this.changedByEntityId = -1;
	}

	public BlockChangeInfo(int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, sbyte _density, int _changedByEntityId) : this(_clrIdx, _blockPos, _blockValue, _density)
	{
		this.changedByEntityId = _changedByEntityId;
	}

	public BlockChangeInfo(int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, sbyte _density, long _tex) : this(_clrIdx, _blockPos, _blockValue, _density)
	{
		this.bChangeTexture = true;
		this.textureFull = _tex;
		this.changedByEntityId = -1;
	}

	public override bool Equals(object other)
	{
		BlockChangeInfo blockChangeInfo = other as BlockChangeInfo;
		return blockChangeInfo != null && (this.pos.Equals(blockChangeInfo.pos) && this.blockValue.type == blockChangeInfo.blockValue.type) && this.density == blockChangeInfo.density;
	}

	public void Read(BinaryReader _br)
	{
		this.clrIdx = (int)_br.ReadByte();
		this.pos = StreamUtils.ReadVector3i(_br);
		this.changedByEntityId = _br.ReadInt32();
		int num = (int)_br.ReadByte();
		this.bChangeBlockValue = ((num & 1) != 0);
		this.bChangeDensity = ((num & 2) != 0);
		this.bUpdateLight = ((num & 4) != 0);
		this.bChangeDamage = ((num & 8) != 0);
		this.bChangeTexture = ((num & 16) != 0);
		if (this.bChangeBlockValue)
		{
			this.blockValue.rawData = _br.ReadUInt32();
			this.blockValue.damage = (int)_br.ReadUInt16();
		}
		if (this.bChangeDensity)
		{
			this.density = _br.ReadSByte();
			this.bForceDensityChange = _br.ReadBoolean();
		}
		if (this.bChangeTexture)
		{
			this.textureFull = _br.ReadInt64();
		}
	}

	public void Write(BinaryWriter _bw)
	{
		_bw.Write((byte)this.clrIdx);
		StreamUtils.Write(_bw, this.pos);
		_bw.Write(this.changedByEntityId);
		int num = this.bChangeBlockValue ? 1 : 0;
		num |= (this.bChangeDensity ? 2 : 0);
		num |= (this.bUpdateLight ? 4 : 0);
		num |= (this.bChangeDamage ? 8 : 0);
		num |= (this.bChangeTexture ? 16 : 0);
		_bw.Write((byte)num);
		if (this.bChangeBlockValue)
		{
			_bw.Write(this.blockValue.rawData);
			_bw.Write((ushort)this.blockValue.damage);
		}
		if (this.bChangeDensity)
		{
			_bw.Write(this.density);
			_bw.Write(this.bForceDensityChange);
		}
		if (this.bChangeTexture)
		{
			_bw.Write(this.textureFull);
		}
	}

	public override int GetHashCode()
	{
		return this.pos.GetHashCode();
	}

	public static bool operator ==(BlockChangeInfo point1, BlockChangeInfo point2)
	{
		return (point1 == null && point2 == null) || point1.Equals(point2);
	}

	public static bool operator !=(BlockChangeInfo point1, BlockChangeInfo point2)
	{
		return !(point1 == point2);
	}

	public static BlockChangeInfo Empty = new BlockChangeInfo();

	public int clrIdx;

	public Vector3i pos;

	public bool bChangeBlockValue;

	public bool bChangeDamage;

	public BlockValue blockValue;

	public bool bChangeDensity;

	public bool bForceDensityChange;

	public sbyte density;

	public bool bUpdateLight;

	public bool bChangeTexture;

	public long textureFull;

	public int changedByEntityId = -1;
}

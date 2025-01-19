using System;
using System.IO;

public class WorldBlockTickerEntry
{
	public WorldBlockTickerEntry(int _clrIdx, Vector3i _pos, int _id, ulong _scheduledTime)
	{
		this.clrIdx = _clrIdx;
		long num = WorldBlockTickerEntry.nextTickEntryID;
		WorldBlockTickerEntry.nextTickEntryID = num + 1L;
		this.tickEntryID = num;
		this.worldPos = _pos;
		this.blockID = _id;
		this.scheduledTime = _scheduledTime;
	}

	public static WorldBlockTickerEntry Read(BinaryReader _br, int _chunkX, int _chunkZ, int _version)
	{
		Vector3i pos = new Vector3i((int)_br.ReadByte() + _chunkX * 16, (int)_br.ReadByte(), (int)_br.ReadByte() + _chunkZ * 16);
		int id = (int)_br.ReadUInt16();
		ulong num = _br.ReadUInt64();
		return new WorldBlockTickerEntry((int)_br.ReadUInt16(), pos, id, num);
	}

	public void Write(BinaryWriter _bw)
	{
		_bw.Write((byte)World.toBlockXZ(this.worldPos.x));
		_bw.Write((byte)this.worldPos.y);
		_bw.Write((byte)World.toBlockXZ(this.worldPos.z));
		_bw.Write((ushort)this.blockID);
		_bw.Write(this.scheduledTime);
		_bw.Write((ushort)this.clrIdx);
	}

	public override bool Equals(object _obj)
	{
		WorldBlockTickerEntry worldBlockTickerEntry = _obj as WorldBlockTickerEntry;
		return worldBlockTickerEntry != null && (this.worldPos.Equals(worldBlockTickerEntry.worldPos) && this.blockID == worldBlockTickerEntry.blockID && this.clrIdx == worldBlockTickerEntry.clrIdx) && worldBlockTickerEntry.tickEntryID == this.tickEntryID;
	}

	public override int GetHashCode()
	{
		return WorldBlockTickerEntry.ToHashCode(this.clrIdx, this.worldPos, this.blockID);
	}

	public static int ToHashCode(int _clrIdx, Vector3i _pos, int _blockID)
	{
		return (_pos.GetHashCode() * 397 ^ _blockID) * 397 ^ _clrIdx;
	}

	public long GetChunkKey()
	{
		return WorldChunkCache.MakeChunkKey(World.toChunkXZ(this.worldPos.x), World.toChunkXZ(this.worldPos.z), this.clrIdx);
	}

	public readonly Vector3i worldPos;

	public readonly int blockID;

	public readonly ulong scheduledTime;

	public readonly int clrIdx;

	[PublicizedFrom(EAccessModifier.Private)]
	public static long nextTickEntryID;

	public readonly long tickEntryID;
}

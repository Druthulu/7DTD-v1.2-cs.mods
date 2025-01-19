using System;
using System.Collections.Generic;
using UnityEngine;

public class TraderArea
{
	public TraderArea(Vector3i _pos, Vector3i _size, Vector3i _protectPadding, List<Prefab.PrefabTeleportVolume> _teleportVolumes)
	{
		this.Position = _pos;
		this.PrefabSize = _size;
		this.ProtectSize = _size + _protectPadding;
		this.ProtectSize.x = this.ProtectSize.x + 2;
		this.ProtectSize.z = this.ProtectSize.z + 2;
		this.ProtectPosition = _pos + _size / 2 - this.ProtectSize / 2;
		this.ProtectBounds = new BoundsInt(this.ProtectPosition, this.ProtectSize);
		this.TeleportVolumes = _teleportVolumes;
	}

	public Vector3i GetProtectPadding()
	{
		Vector3i result = this.ProtectSize - this.PrefabSize;
		result.x -= 2;
		result.z -= 2;
		return result;
	}

	public bool IsWithinTeleportArea(Vector3 _pos, out Prefab.PrefabTeleportVolume tpVolume)
	{
		foreach (Prefab.PrefabTeleportVolume prefabTeleportVolume in this.TeleportVolumes)
		{
			Vector3i vector3i = this.Position + prefabTeleportVolume.startPos;
			if ((float)vector3i.x <= _pos.x && _pos.x <= (float)(vector3i.x + prefabTeleportVolume.size.x) && (float)vector3i.y <= _pos.y && _pos.y <= (float)(vector3i.y + prefabTeleportVolume.size.y) && (float)vector3i.z <= _pos.z && _pos.z <= (float)(vector3i.z + prefabTeleportVolume.size.z))
			{
				tpVolume = prefabTeleportVolume;
				return true;
			}
		}
		tpVolume = null;
		return false;
	}

	public bool SetClosed(World _world, bool _bClosed, EntityTrader _trader, bool playSound = false)
	{
		this.IsClosed = _bClosed;
		int num = World.toChunkXZ(this.Position.x - 1);
		int num2 = World.toChunkXZ(this.Position.x + this.PrefabSize.x + 1);
		int num3 = World.toChunkXZ(this.Position.z - 1);
		int num4 = World.toChunkXZ(this.Position.z + this.PrefabSize.z + 1);
		for (int i = num3; i <= num4; i++)
		{
			for (int j = num; j <= num2; j++)
			{
				if (!(_world.GetChunkSync(j, i) is Chunk))
				{
					return false;
				}
			}
		}
		for (int k = num3; k <= num4; k++)
		{
			for (int l = num; l <= num2; l++)
			{
				Chunk chunk = _world.GetChunkSync(l, k) as Chunk;
				List<Vector3i> list = chunk.IndexedBlocks["TraderOnOff"];
				if (list != null)
				{
					for (int m = 0; m < list.Count; m++)
					{
						BlockValue block = chunk.GetBlock(list[m]);
						if (!block.ischild)
						{
							Vector3i vector3i = chunk.ToWorldPos(list[m]);
							if (this.ProtectBounds.Contains(vector3i))
							{
								Block block2 = block.Block;
								if (block2 is BlockDoor)
								{
									if (_bClosed && BlockDoor.IsDoorOpen(block.meta))
									{
										block2.OnBlockActivated(_world, 0, vector3i, block, null);
									}
									BlockDoorSecure blockDoorSecure = block2 as BlockDoorSecure;
									if (blockDoorSecure != null)
									{
										if (_bClosed)
										{
											if (!blockDoorSecure.IsDoorLocked(_world, vector3i))
											{
												block2.OnBlockActivated("lock", _world, 0, vector3i, block, null);
											}
										}
										else if (blockDoorSecure.IsDoorLocked(_world, vector3i))
										{
											block2.OnBlockActivated("unlock", _world, 0, vector3i, block, null);
										}
									}
								}
								else if (block2 is BlockLight)
								{
									block.meta = (byte)((!_bClosed) ? ((int)(block.meta | 2)) : ((int)block.meta & -3));
									_world.SetBlockRPC(vector3i, block);
								}
								else if (block2 is BlockSpeakerTrader && playSound)
								{
									BlockSpeakerTrader blockSpeakerTrader = block2 as BlockSpeakerTrader;
									if (_bClosed)
									{
										blockSpeakerTrader.PlayClose(vector3i, _trader);
									}
									else
									{
										blockSpeakerTrader.PlayOpen(vector3i, _trader);
									}
								}
							}
						}
					}
				}
			}
		}
		return true;
	}

	public void HandleWarning(World _world, EntityTrader _trader)
	{
		int num = World.toChunkXZ(this.Position.x - 1);
		int num2 = World.toChunkXZ(this.Position.x + this.PrefabSize.x + 1);
		int num3 = World.toChunkXZ(this.Position.z - 1);
		int num4 = World.toChunkXZ(this.Position.z + this.PrefabSize.z + 1);
		for (int i = num; i <= num2; i++)
		{
			for (int j = num3; j <= num4; j++)
			{
				Chunk chunk = _world.GetChunkSync(i, j) as Chunk;
				if (chunk != null)
				{
					List<Vector3i> list = chunk.IndexedBlocks["TraderOnOff"];
					if (list != null)
					{
						for (int k = 0; k < list.Count; k++)
						{
							BlockValue block = chunk.GetBlock(list[k]);
							if (!block.ischild)
							{
								BlockSpeakerTrader blockSpeakerTrader = block.Block as BlockSpeakerTrader;
								if (blockSpeakerTrader != null)
								{
									Vector3i blockPos = chunk.ToWorldPos(list[k]);
									blockSpeakerTrader.PlayWarning(blockPos, _trader);
								}
							}
						}
					}
				}
			}
		}
	}

	public bool Overlaps(Vector3i _min, Vector3i _max)
	{
		return _max.x >= this.ProtectPosition.x && _min.x < this.ProtectPosition.x + this.ProtectSize.x && _max.z >= this.ProtectPosition.z && _min.z < this.ProtectPosition.z + this.ProtectSize.z;
	}

	public int GetReadWriteSize()
	{
		return 21 + (1 + this.TeleportVolumes.Count * 6);
	}

	public static TraderArea Read(PooledBinaryReader _reader)
	{
		Vector3i pos;
		pos.x = _reader.ReadInt32();
		pos.y = _reader.ReadInt32();
		pos.z = _reader.ReadInt32();
		Vector3i size;
		size.x = (int)_reader.ReadInt16();
		size.y = (int)_reader.ReadInt16();
		size.z = (int)_reader.ReadInt16();
		Vector3i protectPadding;
		protectPadding.x = (int)_reader.ReadSByte();
		protectPadding.y = (int)_reader.ReadSByte();
		protectPadding.z = (int)_reader.ReadSByte();
		int num = (int)_reader.ReadByte();
		List<Prefab.PrefabTeleportVolume> list = new List<Prefab.PrefabTeleportVolume>();
		for (int i = 0; i < num; i++)
		{
			Prefab.PrefabTeleportVolume prefabTeleportVolume = new Prefab.PrefabTeleportVolume();
			prefabTeleportVolume.startPos.x = (int)_reader.ReadSByte();
			prefabTeleportVolume.startPos.y = (int)_reader.ReadSByte();
			prefabTeleportVolume.startPos.z = (int)_reader.ReadSByte();
			prefabTeleportVolume.size.x = (int)_reader.ReadByte();
			prefabTeleportVolume.size.y = (int)_reader.ReadByte();
			prefabTeleportVolume.size.z = (int)_reader.ReadByte();
			list.Add(prefabTeleportVolume);
		}
		return new TraderArea(pos, size, protectPadding, list);
	}

	public void Write(PooledBinaryWriter _writer)
	{
		_writer.Write(this.Position.x);
		_writer.Write(this.Position.y);
		_writer.Write(this.Position.z);
		_writer.Write((short)this.PrefabSize.x);
		_writer.Write((short)this.PrefabSize.y);
		_writer.Write((short)this.PrefabSize.z);
		Vector3i protectPadding = this.GetProtectPadding();
		_writer.Write((sbyte)protectPadding.x);
		_writer.Write((sbyte)protectPadding.y);
		_writer.Write((sbyte)protectPadding.z);
		_writer.Write((byte)this.TeleportVolumes.Count);
		for (int i = 0; i < this.TeleportVolumes.Count; i++)
		{
			Prefab.PrefabTeleportVolume prefabTeleportVolume = this.TeleportVolumes[i];
			_writer.Write((sbyte)prefabTeleportVolume.startPos.x);
			_writer.Write((sbyte)prefabTeleportVolume.startPos.y);
			_writer.Write((sbyte)prefabTeleportVolume.startPos.z);
			_writer.Write((byte)prefabTeleportVolume.size.x);
			_writer.Write((byte)prefabTeleportVolume.size.y);
			_writer.Write((byte)prefabTeleportVolume.size.z);
		}
	}

	public Vector3i Position;

	public Vector3i PrefabSize;

	public Vector3i ProtectPosition;

	public Vector3i ProtectSize;

	public BoundsInt ProtectBounds;

	public bool IsClosed = true;

	public List<Prefab.PrefabTeleportVolume> TeleportVolumes;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cPadXZ = 2;
}

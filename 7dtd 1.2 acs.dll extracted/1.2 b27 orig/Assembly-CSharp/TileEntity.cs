using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class TileEntity : ITileEntity
{
	public int EntityId
	{
		get
		{
			return this.entityId;
		}
	}

	public Vector3i localChunkPos
	{
		get
		{
			return this.chunkPos;
		}
		set
		{
			this.chunkPos = value;
			this.OnSetLocalChunkPosition();
		}
	}

	public BlockValue blockValue
	{
		get
		{
			return this.chunk.GetBlock(this.localChunkPos);
		}
	}

	public event XUiEvent_TileEntityDestroyed Destroyed;

	public List<ITileEntityChangedListener> listeners
	{
		get
		{
			return this._listeners;
		}
	}

	public bool bWaitingForServerResponse
	{
		get
		{
			return this.lockHandleWaitingFor != byte.MaxValue;
		}
	}

	public TileEntity(Chunk _chunk)
	{
		this.chunk = _chunk;
	}

	public virtual TileEntity Clone()
	{
		throw new NotImplementedException("Clone() not implemented yet");
	}

	public virtual void CopyFrom(TileEntity _other)
	{
		throw new NotImplementedException("CopyFrom() not implemented yet");
	}

	public virtual void UpdateTick(World world)
	{
	}

	public abstract TileEntityType GetTileEntityType();

	public virtual void OnSetLocalChunkPosition()
	{
	}

	public Vector3i ToWorldPos()
	{
		if (this.chunk != null)
		{
			return new Vector3i(this.chunk.X * 16, this.chunk.Y * 256, this.chunk.Z * 16) + this.localChunkPos;
		}
		return Vector3i.zero;
	}

	public Vector3 ToWorldCenterPos()
	{
		if (this.entityId != -1)
		{
			Entity entity = GameManager.Instance.World.GetEntity(this.entityId);
			if (entity)
			{
				return entity.position;
			}
		}
		if (this.chunk != null)
		{
			BlockValue blockNoDamage = this.chunk.GetBlockNoDamage(this.chunkPos.x, this.chunkPos.y, this.chunkPos.z);
			Block block = blockNoDamage.Block;
			Vector3 vector;
			vector.x = (float)(this.chunk.X * 16 + this.chunkPos.x);
			vector.y = (float)(this.chunk.Y * 256 + this.chunkPos.y);
			vector.z = (float)(this.chunk.Z * 16 + this.chunkPos.z);
			if (!block.isMultiBlock)
			{
				vector.x += 0.5f;
				vector.y += 0.5f;
				vector.z += 0.5f;
			}
			else
			{
				BlockShapeModelEntity blockShapeModelEntity = block.shape as BlockShapeModelEntity;
				if (blockShapeModelEntity != null)
				{
					Quaternion rotation = blockShapeModelEntity.GetRotation(blockNoDamage);
					vector += blockShapeModelEntity.GetRotatedOffset(block, rotation);
					vector.x += 0.5f;
					vector.z += 0.5f;
				}
			}
			return vector;
		}
		return Vector3.zero;
	}

	public int GetClrIdx()
	{
		if (this.chunk == null)
		{
			return 0;
		}
		return this.chunk.ClrIdx;
	}

	public Chunk GetChunk()
	{
		return this.chunk;
	}

	public void SetChunk(Chunk _chunk)
	{
		this.chunk = _chunk;
	}

	public static TileEntity Instantiate(TileEntityType type, Chunk _chunk)
	{
		switch (type)
		{
		case TileEntityType.DewCollector:
			return new TileEntityDewCollector(_chunk);
		case TileEntityType.LandClaim:
			return new TileEntityLandClaim(_chunk);
		case TileEntityType.Loot:
			return new TileEntityLootContainer(_chunk);
		case TileEntityType.Trader:
			return new TileEntityTrader(_chunk);
		case TileEntityType.VendingMachine:
			return new TileEntityVendingMachine(_chunk);
		case TileEntityType.Forge:
			return new TileEntityForge(_chunk);
		case TileEntityType.SecureLoot:
			return new TileEntitySecureLootContainer(_chunk);
		case TileEntityType.SecureDoor:
			return new TileEntitySecureDoor(_chunk);
		case TileEntityType.Workstation:
			return new TileEntityWorkstation(_chunk);
		case TileEntityType.Sign:
			return new TileEntitySign(_chunk);
		case TileEntityType.GoreBlock:
			return new TileEntityGoreBlock(_chunk);
		case TileEntityType.Powered:
			return new TileEntityPoweredBlock(_chunk);
		case TileEntityType.PowerSource:
			return new TileEntityPowerSource(_chunk);
		case TileEntityType.PowerRangeTrap:
			return new TileEntityPoweredRangedTrap(_chunk);
		case TileEntityType.Light:
			return new TileEntityLight(_chunk);
		case TileEntityType.Trigger:
			return new TileEntityPoweredTrigger(_chunk);
		case TileEntityType.Sleeper:
			return new TileEntitySleeper(_chunk);
		case TileEntityType.PowerMeleeTrap:
			return new TileEntityPoweredMeleeTrap(_chunk);
		case TileEntityType.SecureLootSigned:
			return new TileEntitySecureLootContainerSigned(_chunk);
		case TileEntityType.Composite:
			return new TileEntityComposite(_chunk);
		}
		Log.Warning("Dropping TE with unknown type: " + type.ToStringCached<TileEntityType>());
		return null;
	}

	public virtual void read(PooledBinaryReader _br, TileEntity.StreamModeRead _eStreamMode)
	{
		if (_eStreamMode == TileEntity.StreamModeRead.Persistency)
		{
			this.readVersion = (int)_br.ReadUInt16();
			this.localChunkPos = StreamUtils.ReadVector3i(_br);
			this.entityId = _br.ReadInt32();
			if (this.readVersion > 1)
			{
				this.heapMapUpdateTime = _br.ReadUInt64();
				this.heapMapLastTime = this.heapMapUpdateTime - AIDirector.GetActivityWorldTimeDelay();
				return;
			}
		}
		else
		{
			this.localChunkPos = StreamUtils.ReadVector3i(_br);
			this.entityId = _br.ReadInt32();
		}
	}

	public virtual void write(PooledBinaryWriter _bw, TileEntity.StreamModeWrite _eStreamMode)
	{
		if (_eStreamMode == TileEntity.StreamModeWrite.Persistency)
		{
			_bw.Write(11);
			StreamUtils.Write(_bw, this.localChunkPos);
			_bw.Write(this.entityId);
			_bw.Write(this.heapMapUpdateTime);
			return;
		}
		StreamUtils.Write(_bw, this.localChunkPos);
		_bw.Write(this.entityId);
	}

	public override string ToString()
	{
		return string.Format(string.Concat(new string[]
		{
			"[TE] ",
			this.GetTileEntityType().ToStringCached<TileEntityType>(),
			"/",
			this.ToWorldPos().ToString(),
			"/",
			this.entityId.ToString()
		}), Array.Empty<object>());
	}

	public virtual void OnRemove(World world)
	{
		this.OnDestroy();
	}

	public virtual void OnUnload(World world)
	{
	}

	public virtual void OnReadComplete()
	{
	}

	public void SetDisableModifiedCheck(bool _b)
	{
		this.bDisableModifiedCheck = _b;
	}

	public void SetModified()
	{
		this.setModified();
	}

	public void SetChunkModified()
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && this.chunk != null)
		{
			this.chunk.isModified = true;
		}
	}

	public virtual bool IsActive(World world)
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool IsByWater(World _world, Vector3i _blockPos)
	{
		return _world.IsWater(_blockPos.x, _blockPos.y + 1, _blockPos.z) | _world.IsWater(_blockPos.x + 1, _blockPos.y, _blockPos.z) | _world.IsWater(_blockPos.x - 1, _blockPos.y, _blockPos.z) | _world.IsWater(_blockPos.x, _blockPos.y, _blockPos.z + 1) | _world.IsWater(_blockPos.x, _blockPos.y, _blockPos.z - 1);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void emitHeatMapEvent(World world, EnumAIDirectorChunkEvent eventType)
	{
		if (world.worldTime < this.heapMapLastTime)
		{
			this.heapMapUpdateTime = 0UL;
		}
		if (world.worldTime >= this.heapMapUpdateTime && world.aiDirector != null)
		{
			Vector3i vector3i = this.ToWorldPos();
			Block block = world.GetBlock(vector3i).Block;
			if (block != null)
			{
				world.aiDirector.NotifyActivity(eventType, vector3i, block.HeatMapStrength, 720f);
				this.heapMapLastTime = world.worldTime;
				this.heapMapUpdateTime = world.worldTime + AIDirector.GetActivityWorldTimeDelay();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void setModified()
	{
		if (this.bDisableModifiedCheck)
		{
			return;
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			this.SetChunkModified();
			Vector3? entitiesInRangeOfWorldPos = new Vector3?(this.ToWorldCenterPos());
			if (entitiesInRangeOfWorldPos.Value == Vector3.zero)
			{
				entitiesInRangeOfWorldPos = null;
			}
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageTileEntity>().Setup(this, TileEntity.StreamModeWrite.ToClient, byte.MaxValue), true, -1, -1, -1, entitiesInRangeOfWorldPos, 192);
		}
		else
		{
			byte b = this.handleCounter + 1;
			this.handleCounter = b;
			if (b == 255)
			{
				this.handleCounter = 0;
			}
			this.lockHandleWaitingFor = this.handleCounter;
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageTileEntity>().Setup(this, TileEntity.StreamModeWrite.ToServer, this.lockHandleWaitingFor), false);
		}
		this.NotifyListeners();
	}

	public override int GetHashCode()
	{
		if (this.entityId != -1)
		{
			return this.entityId | 134217728;
		}
		return this.ToWorldPos().GetHashCode() & int.MaxValue;
	}

	public override bool Equals(object obj)
	{
		return base.Equals(obj) && obj.GetHashCode() == this.GetHashCode();
	}

	public void NotifyListeners()
	{
		for (int i = 0; i < this.listeners.Count; i++)
		{
			this.listeners[i].OnTileEntityChanged(this);
		}
	}

	public virtual void UpgradeDowngradeFrom(TileEntity _other)
	{
		_other.OnDestroy();
	}

	public virtual void ReplacedBy(BlockValue _bvOld, BlockValue _bvNew, TileEntity _teNew)
	{
	}

	public virtual void SetUserAccessing(bool _bUserAccessing)
	{
		this.bUserAccessing = _bUserAccessing;
	}

	public bool IsUserAccessing()
	{
		return this.bUserAccessing;
	}

	public virtual void SetHandle(byte _handle)
	{
		if (this.lockHandleWaitingFor != 255 && this.lockHandleWaitingFor == _handle)
		{
			this.lockHandleWaitingFor = byte.MaxValue;
		}
	}

	public virtual void OnDestroy()
	{
		if (this.Destroyed != null)
		{
			this.Destroyed(this);
		}
	}

	public virtual void Reset(FastTags<TagGroup.Global> questTags)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public const int CurrentSaveVersion = 11;

	public int entityId = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i chunkPos;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int readVersion;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Chunk chunk;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong heapMapLastTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong heapMapUpdateTime;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool bDisableModifiedCheck;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool bUserAccessing;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<ITileEntityChangedListener> _listeners = new List<ITileEntityChangedListener>();

	[PublicizedFrom(EAccessModifier.Private)]
	public byte handleCounter;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte lockHandleWaitingFor = byte.MaxValue;

	public enum StreamModeRead
	{
		Persistency,
		FromServer,
		FromClient
	}

	public enum StreamModeWrite
	{
		Persistency,
		ToServer,
		ToClient
	}
}

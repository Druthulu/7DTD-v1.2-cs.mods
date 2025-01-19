using System;
using System.IO;
using System.Xml;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EntityCreationData
{
	public EntityCreationData()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityCreationData(EntityCreationData _other)
	{
		this.entityClass = _other.entityClass;
		this.pos = _other.pos;
		this.rot = _other.rot;
		this.id = _other.id;
		this.onGround = _other.onGround;
		this.stats = ((_other.stats != null) ? _other.stats.SimpleClone() : null);
		this.deathTime = _other.deathTime;
		this.lifetime = _other.lifetime;
		this.itemStack = _other.itemStack;
		this.belongsPlayerId = _other.belongsPlayerId;
		this.clientEntityId = _other.clientEntityId;
		this.holdingItem = _other.holdingItem;
		this.teamNumber = _other.teamNumber;
		this.entityName = _other.entityName;
		this.skinTexture = _other.skinTexture;
		this.subType = _other.subType;
		this.lootContainer = ((_other.lootContainer != null) ? ((TileEntityLootContainer)_other.lootContainer.Clone()) : null);
		this.traderData = ((_other.traderData != null) ? ((TileEntityTrader)_other.traderData.Clone()) : null);
		this.homePosition = _other.homePosition;
		this.homeRange = _other.homeRange;
		this.entityData = _other.entityData;
		this.readFileVersion = _other.readFileVersion;
		this.playerProfile = _other.playerProfile;
		this.bodyDamage = _other.bodyDamage;
		this.sleeperPose = _other.sleeperPose;
		this.isSleeper = _other.isSleeper;
		this.isSleeperPassive = _other.isSleeperPassive;
		this.spawnByName = _other.spawnByName;
		this.spawnById = _other.spawnById;
		this.spawnByAllowShare = _other.spawnByAllowShare;
		this.headState = _other.headState;
		this.overrideSize = _other.overrideSize;
		this.overrideHeadSize = _other.overrideHeadSize;
		this.isDancing = _other.isDancing;
	}

	public EntityCreationData(XmlElement _entityElement)
	{
		this.readXml(_entityElement);
	}

	public void ApplyToEntity(Entity _e)
	{
		EntityAlive entityAlive = _e as EntityAlive;
		if (entityAlive)
		{
			if (this.stats != null)
			{
				this.stats.CopyBuffChangedDelegates(entityAlive.Stats);
				entityAlive.Stats = this.stats;
				entityAlive.Stats.Entity = entityAlive;
			}
			else
			{
				entityAlive.Stats.InitWithOldFormatData(this.health, this.stamina, this.sickness, this.gassiness);
			}
			if (entityAlive.Health <= 0)
			{
				entityAlive.HasDeathAnim = false;
			}
			entityAlive.SetDeathTime(this.deathTime);
			entityAlive.setHomeArea(this.homePosition, this.homeRange);
			EntityPlayer entityPlayer = _e as EntityPlayer;
			if (entityPlayer)
			{
				entityPlayer.playerProfile = this.playerProfile;
			}
			entityAlive.bodyDamage = this.bodyDamage;
			entityAlive.IsSleeper = this.isSleeper;
			if (entityAlive.IsSleeper)
			{
				entityAlive.IsSleeperPassive = this.isSleeperPassive;
			}
			entityAlive.CurrentHeadState = this.headState;
			entityAlive.IsDancing = this.isDancing;
		}
		_e.lootContainer = this.lootContainer;
		_e.spawnByAllowShare = this.spawnByAllowShare;
		_e.spawnById = this.spawnById;
		_e.spawnByName = this.spawnByName;
		EntityTrader entityTrader = _e as EntityTrader;
		if (entityTrader)
		{
			entityTrader.TileEntityTrader = this.traderData;
		}
		if (this.sleeperPose != 255 && entityAlive)
		{
			entityAlive.TriggerSleeperPose((int)this.sleeperPose, false);
		}
		_e.SetSpawnerSource(this.spawnerSource);
		if (this.entityData.Length > 0L)
		{
			this.entityData.Position = 0L;
			try
			{
				using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(false))
				{
					pooledBinaryReader.SetBaseStream(this.entityData);
					_e.Read(this.readFileVersion, pooledBinaryReader);
				}
			}
			catch (Exception e)
			{
				Log.Exception(e);
				Log.Error("Error loading entity " + ((_e != null) ? _e.ToString() : null));
			}
		}
	}

	public EntityCreationData(Entity _e, bool _bNetworkWrite = true)
	{
		this.entityClass = _e.entityClass;
		this.id = _e.entityId;
		this.pos = _e.position;
		this.rot = _e.rotation;
		this.onGround = _e.onGround;
		this.belongsPlayerId = _e.belongsPlayerId;
		this.clientEntityId = _e.clientEntityId;
		this.lifetime = _e.lifetime;
		this.lootContainer = _e.lootContainer;
		this.spawnerSource = _e.GetSpawnerSource();
		this.spawnById = _e.spawnById;
		this.spawnByAllowShare = _e.spawnByAllowShare;
		this.spawnByName = _e.spawnByName;
		if (_e is EntityAlive)
		{
			EntityAlive entityAlive = _e as EntityAlive;
			if (entityAlive.inventory != null)
			{
				this.holdingItem = entityAlive.inventory.holdingItemItemValue;
			}
			this.stats = entityAlive.Stats;
			this.deathTime = entityAlive.GetDeathTime();
			this.teamNumber = entityAlive.TeamNumber;
			this.entityName = entityAlive.EntityName;
			this.skinTexture = string.Empty;
			this.homePosition = entityAlive.getHomePosition().position;
			this.homeRange = entityAlive.getMaximumHomeDistance();
			this.bodyDamage = entityAlive.bodyDamage;
			this.sleeperPose = (byte)(entityAlive.IsSleeping ? entityAlive.lastSleeperPose : 255);
			this.isSleeper = entityAlive.IsSleeper;
			this.isSleeperPassive = entityAlive.IsSleeperPassive;
			if (_e is EntityPlayer)
			{
				EntityPlayer entityPlayer = _e as EntityPlayer;
				this.playerProfile = entityPlayer.playerProfile;
			}
			else if (_e is EntityTrader)
			{
				this.traderData = ((EntityTrader)_e).TileEntityTrader;
			}
			this.headState = entityAlive.GetHeadState();
			this.overrideSize = entityAlive.OverrideSize;
			this.overrideHeadSize = entityAlive.OverrideHeadSize;
			this.isDancing = entityAlive.IsDancing;
		}
		else if (_e is EntityItem)
		{
			EntityItem entityItem = (EntityItem)_e;
			this.itemStack = entityItem.itemStack;
		}
		else if (_e is EntityFallingBlock)
		{
			EntityFallingBlock entityFallingBlock = _e as EntityFallingBlock;
			this.blockValue = entityFallingBlock.GetBlockValue();
			this.textureFull = entityFallingBlock.GetTextureFull();
		}
		else if (_e is EntityFallingTree)
		{
			EntityFallingTree entityFallingTree = _e as EntityFallingTree;
			this.blockPos = entityFallingTree.GetBlockPos();
			this.fallTreeDir = entityFallingTree.GetFallTreeDir();
		}
		using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(false))
		{
			pooledBinaryWriter.SetBaseStream(this.entityData);
			_e.Write(pooledBinaryWriter, _bNetworkWrite);
		}
		this.readFileVersion = 28;
	}

	public EntityCreationData Clone()
	{
		return new EntityCreationData(this);
	}

	public void read(PooledBinaryReader _br, bool _bNetworkRead)
	{
		this.readFileVersion = _br.ReadByte();
		byte b = this.readFileVersion;
		this.entityClass = _br.ReadInt32();
		this.id = _br.ReadInt32();
		this.lifetime = _br.ReadSingle();
		this.pos.x = _br.ReadSingle();
		this.pos.y = _br.ReadSingle();
		this.pos.z = _br.ReadSingle();
		this.rot.x = _br.ReadSingle();
		this.rot.y = _br.ReadSingle();
		this.rot.z = _br.ReadSingle();
		this.onGround = _br.ReadBoolean();
		this.bodyDamage = BodyDamage.Read(_br, (int)b);
		if (b >= 8)
		{
			if (_br.ReadBoolean())
			{
				this.stats = new EntityStats(null);
				if (b > 8)
				{
					this.stats.Read(_br);
				}
				else
				{
					this.stats.ReadBeforeEmbeddedVersion(_br);
				}
			}
		}
		else
		{
			this.health = (int)_br.ReadInt16();
			this.stamina = (int)_br.ReadInt16();
			if (b >= 7)
			{
				this.sickness = (int)_br.ReadInt16();
				this.gassiness = (int)_br.ReadInt16();
			}
		}
		this.deathTime = (int)_br.ReadInt16();
		if (b >= 2 && _br.ReadBoolean())
		{
			TileEntityType type = (TileEntityType)_br.ReadInt32();
			this.lootContainer = (TileEntityLootContainer)TileEntity.Instantiate(type, null);
			this.lootContainer.read(_br, _bNetworkRead ? TileEntity.StreamModeRead.FromServer : TileEntity.StreamModeRead.Persistency);
		}
		if (b >= 3)
		{
			this.homePosition = new Vector3i(_br.ReadInt32(), _br.ReadInt32(), _br.ReadInt32());
			this.homeRange = (int)_br.ReadInt16();
		}
		if (b >= 5)
		{
			this.spawnerSource = (EnumSpawnerSource)_br.ReadByte();
		}
		if (this.entityClass == EntityClass.itemClass)
		{
			if (b <= 5)
			{
				this.belongsPlayerId = (int)_br.ReadInt16();
			}
			else
			{
				this.belongsPlayerId = _br.ReadInt32();
			}
			if (b >= 27)
			{
				this.clientEntityId = _br.ReadInt32();
			}
			this.itemStack = ItemStack.Empty.Clone();
			if (b < 14)
			{
				this.itemStack.ReadOld(_br);
			}
			else
			{
				this.itemStack.Read(_br);
			}
			if (b >= 3)
			{
				_br.ReadSByte();
			}
		}
		else if (this.entityClass == EntityClass.fallingBlockClass)
		{
			this.blockValue = new BlockValue(_br.ReadUInt32());
			this.textureFull = _br.ReadInt64();
		}
		else if (this.entityClass == EntityClass.fallingTreeClass)
		{
			this.blockPos = StreamUtils.ReadVector3i(_br);
			this.fallTreeDir = StreamUtils.ReadVector3(_br);
		}
		else if (this.entityClass == EntityClass.playerMaleClass || this.entityClass == EntityClass.playerFemaleClass)
		{
			this.holdingItem.Read(_br);
			this.teamNumber = (int)_br.ReadByte();
			this.entityName = _br.ReadString();
			this.skinTexture = _br.ReadString();
			if (b > 12)
			{
				if (_br.ReadBoolean())
				{
					this.playerProfile = PlayerProfile.Read(_br);
				}
				else
				{
					this.playerProfile = null;
				}
			}
		}
		if (b > 9)
		{
			int num = (int)_br.ReadUInt16();
			if (num > 0)
			{
				byte[] buffer = _br.ReadBytes(num);
				this.entityData = new MemoryStream(buffer);
			}
		}
		if (b > 23 && _br.ReadBoolean())
		{
			TileEntityType type2 = (TileEntityType)_br.ReadInt32();
			this.traderData = (TileEntityTrader)TileEntity.Instantiate(type2, null);
			this.traderData.read(_br, _bNetworkRead ? TileEntity.StreamModeRead.FromServer : TileEntity.StreamModeRead.Persistency);
		}
		if (_bNetworkRead)
		{
			this.sleeperPose = _br.ReadByte();
			this.isSleeper = _br.ReadBoolean();
			this.spawnById = _br.ReadInt32();
			this.spawnByName = _br.ReadString();
			this.spawnByAllowShare = _br.ReadBoolean();
			this.headState = (EModelBase.HeadStates)_br.ReadByte();
			this.overrideSize = _br.ReadSingle();
			this.overrideHeadSize = _br.ReadSingle();
			this.isDancing = _br.ReadBoolean();
			if (this.isSleeper)
			{
				this.isSleeperPassive = _br.ReadBoolean();
			}
		}
	}

	public void write(PooledBinaryWriter _bw, bool _bNetworkWrite)
	{
		_bw.Write(28);
		_bw.Write(this.entityClass);
		_bw.Write(this.id);
		_bw.Write(this.lifetime);
		_bw.Write(this.pos.x);
		_bw.Write(this.pos.y);
		_bw.Write(this.pos.z);
		_bw.Write(this.rot.x);
		_bw.Write(this.rot.y);
		_bw.Write(this.rot.z);
		_bw.Write(this.onGround);
		this.bodyDamage.Write(_bw);
		_bw.Write(this.stats != null);
		if (this.stats != null)
		{
			this.stats.Write(_bw);
		}
		_bw.Write((short)this.deathTime);
		_bw.Write(this.lootContainer != null);
		if (this.lootContainer != null)
		{
			_bw.Write((int)this.lootContainer.GetTileEntityType());
			this.lootContainer.write(_bw, _bNetworkWrite ? TileEntity.StreamModeWrite.ToClient : TileEntity.StreamModeWrite.Persistency);
		}
		_bw.Write(this.homePosition.x);
		_bw.Write(this.homePosition.y);
		_bw.Write(this.homePosition.z);
		_bw.Write((short)this.homeRange);
		_bw.Write((byte)this.spawnerSource);
		if (this.entityClass == EntityClass.itemClass)
		{
			_bw.Write(this.belongsPlayerId);
			_bw.Write(this.clientEntityId);
			this.itemStack.Write(_bw);
			_bw.Write(0);
		}
		else if (this.entityClass == EntityClass.fallingBlockClass)
		{
			_bw.Write(this.blockValue.rawData);
			_bw.Write(this.textureFull);
		}
		else if (this.entityClass == EntityClass.fallingTreeClass)
		{
			StreamUtils.Write(_bw, this.blockPos);
			StreamUtils.Write(_bw, this.fallTreeDir);
		}
		else if (this.entityClass == EntityClass.playerMaleClass || this.entityClass == EntityClass.playerFemaleClass)
		{
			ItemValue.Write(this.holdingItem, _bw);
			_bw.Write((byte)this.teamNumber);
			_bw.Write(this.entityName);
			_bw.Write(this.skinTexture);
			_bw.Write(this.playerProfile != null);
			if (this.playerProfile != null)
			{
				this.playerProfile.Write(_bw);
			}
		}
		int num = (int)this.entityData.Length;
		_bw.Write((ushort)num);
		if (num > 0)
		{
			_bw.Write(this.entityData.ToArray());
		}
		_bw.Write(this.traderData != null);
		if (this.traderData != null)
		{
			_bw.Write((int)this.traderData.GetTileEntityType());
			this.traderData.write(_bw, _bNetworkWrite ? TileEntity.StreamModeWrite.ToClient : TileEntity.StreamModeWrite.Persistency);
		}
		if (_bNetworkWrite)
		{
			_bw.Write(this.sleeperPose);
			_bw.Write(this.isSleeper);
			_bw.Write(this.spawnById);
			_bw.Write(this.spawnByName);
			_bw.Write(this.spawnByAllowShare);
			_bw.Write((byte)this.headState);
			_bw.Write(this.overrideSize);
			_bw.Write(this.overrideHeadSize);
			_bw.Write(this.isDancing);
			if (this.isSleeper)
			{
				_bw.Write(this.isSleeperPassive);
			}
		}
	}

	public void readXml(XmlElement _entityElement)
	{
		if (!_entityElement.HasAttribute("type"))
		{
			throw new Exception("No 'type' element found in entity tag!");
		}
		this.entityClass = EntityClass.FromString(_entityElement.GetAttribute("type"));
		if (!_entityElement.HasAttribute("position"))
		{
			throw new Exception("No 'position' element found in entity tag!");
		}
		this.pos = StringParsers.ParseVector3(_entityElement.GetAttribute("position"), 0, -1);
		if (!_entityElement.HasAttribute("rotation"))
		{
			throw new Exception("No 'rotation' element found in entity tag!");
		}
		this.rot = StringParsers.ParseVector3(_entityElement.GetAttribute("rotation"), 0, -1);
		this.id = -1;
	}

	public void writeXml(StreamWriter _sw)
	{
		_sw.WriteLine(string.Concat(new string[]
		{
			"    <entity type=\"",
			EntityClass.list[this.entityClass].entityClassName,
			"\" position=\"",
			this.pos.x.ToCultureInvariantString(),
			",",
			this.pos.y.ToCultureInvariantString(),
			",",
			this.pos.z.ToCultureInvariantString(),
			"\" rotation=\"",
			this.rot.x.ToCultureInvariantString(),
			",",
			this.rot.y.ToCultureInvariantString(),
			",",
			this.rot.z.ToCultureInvariantString(),
			"\" />"
		}));
	}

	public override string ToString()
	{
		return string.Concat(new string[]
		{
			EntityClass.list[this.entityClass].entityClassName,
			" ",
			this.entityName,
			" id=",
			this.id.ToString(),
			" pos=",
			this.pos.ToCultureInvariantString()
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int FileVersion = 28;

	public int entityClass;

	public Vector3 pos;

	public Vector3 rot;

	public int id;

	public bool onGround;

	public int health = int.MinValue;

	public int stamina = int.MinValue;

	public int sickness = int.MinValue;

	public int gassiness = int.MinValue;

	public EntityStats stats;

	public int deathTime;

	public float lifetime = float.MaxValue;

	public int belongsPlayerId = -1;

	public int clientEntityId;

	public ItemValue holdingItem = ItemValue.None.Clone();

	public int teamNumber;

	public string entityName = "";

	public string skinTexture = "";

	public TileEntityLootContainer lootContainer;

	public TileEntityTrader traderData;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i homePosition;

	[PublicizedFrom(EAccessModifier.Private)]
	public int homeRange = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public EnumSpawnerSource spawnerSource;

	public ItemStack itemStack = ItemStack.Empty.Clone();

	public BlockValue blockValue;

	public long textureFull;

	public Vector3i blockPos;

	public Vector3 fallTreeDir;

	public int subType;

	public byte sleeperPose = byte.MaxValue;

	public PlayerProfile playerProfile;

	public BodyDamage bodyDamage;

	public bool isSleeper;

	public bool isSleeperPassive;

	public string spawnByName = "";

	public int spawnById = -1;

	public bool spawnByAllowShare;

	public EModelBase.HeadStates headState;

	public float overrideSize = 1f;

	public float overrideHeadSize = 1f;

	public bool isDancing;

	public byte readFileVersion;

	public MemoryStream entityData = new MemoryStream(0);
}

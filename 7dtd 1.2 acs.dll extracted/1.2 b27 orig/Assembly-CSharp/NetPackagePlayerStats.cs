using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackagePlayerStats : NetPackage
{
	public NetPackagePlayerStats Setup(EntityAlive _entity)
	{
		this.entityId = _entity.entityId;
		this.killed = _entity.Died;
		this.holdingItemStack = _entity.inventory.holdingItemStack;
		this.holdingItemIndex = (byte)_entity.inventory.holdingItemIdx;
		this.deathHealth = _entity.DeathHealth;
		this.teamNumber = _entity.TeamNumber;
		this.equipment = _entity.equipment;
		if (GameManager.Instance.World.GetPrimaryPlayer() == _entity)
		{
			_entity.inventory.TurnOffLightFlares();
		}
		if (_entity.Progression != null && _entity.Progression.bProgressionStatsChanged)
		{
			_entity.Progression.bProgressionStatsChanged = false;
			this.hasProgression = true;
			using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(false))
			{
				pooledBinaryWriter.SetBaseStream(this.progStream);
				_entity.Progression.Write(pooledBinaryWriter, true);
			}
		}
		this.attachedToEntityId = ((_entity.AttachedToEntity != null) ? _entity.AttachedToEntity.entityId : -1);
		this.entityName = _entity.EntityName;
		EntityPlayer entityPlayer = _entity as EntityPlayer;
		if (entityPlayer != null)
		{
			this.isPlayer = true;
			this.killedPlayers = _entity.KilledPlayers;
			this.killedZombies = _entity.KilledZombies;
			this.experience = entityPlayer.Progression.ExpToNextLevel;
			this.level = entityPlayer.Progression.Level;
			this.totalItemsCrafted = entityPlayer.totalItemsCrafted;
			this.distanceWalked = entityPlayer.distanceWalked;
			this.longestLife = entityPlayer.longestLife;
			this.currentLife = entityPlayer.currentLife;
			this.totalTimePlayed = entityPlayer.totalTimePlayed;
			this.vehiclePose = entityPlayer.GetVehicleAnimation();
			this.isSpectator = entityPlayer.IsSpectator;
		}
		else
		{
			this.isPlayer = false;
			this.experience = 0;
			this.level = 1;
			this.distanceWalked = 0f;
			this.totalItemsCrafted = 0U;
			this.longestLife = 0f;
			this.currentLife = 0f;
			this.totalTimePlayed = 0f;
		}
		return this;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public ~NetPackagePlayerStats()
	{
		MemoryPools.poolMemoryStream.FreeSync(this.progStream);
	}

	public override void read(PooledBinaryReader _reader)
	{
		this.entityId = _reader.ReadInt32();
		this.killed = _reader.ReadInt32();
		this.holdingItemStack = new ItemStack();
		this.holdingItemStack.Read(_reader);
		this.holdingItemIndex = _reader.ReadByte();
		this.deathHealth = _reader.ReadInt32();
		this.teamNumber = (int)_reader.ReadByte();
		this.equipment = Equipment.Read(_reader);
		this.attachedToEntityId = _reader.ReadInt32();
		this.entityName = _reader.ReadString();
		this.isPlayer = _reader.ReadBoolean();
		if (this.isPlayer)
		{
			this.killedZombies = _reader.ReadInt32();
			this.killedPlayers = _reader.ReadInt32();
			this.experience = _reader.ReadInt32();
			this.level = _reader.ReadInt32();
			this.totalItemsCrafted = _reader.ReadUInt32();
			this.distanceWalked = _reader.ReadSingle();
			this.longestLife = _reader.ReadSingle();
			this.currentLife = _reader.ReadSingle();
			this.totalTimePlayed = _reader.ReadSingle();
			this.vehiclePose = _reader.ReadInt32();
			this.isSpectator = _reader.ReadBoolean();
		}
		this.hasProgression = _reader.ReadBoolean();
		if (this.hasProgression)
		{
			int length = (int)_reader.ReadInt16();
			StreamUtils.StreamCopy(_reader.BaseStream, this.progStream, length, null, true);
		}
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(this.entityId);
		_writer.Write(this.killed);
		this.holdingItemStack.Write(_writer);
		_writer.Write(this.holdingItemIndex);
		_writer.Write(this.deathHealth);
		_writer.Write((byte)this.teamNumber);
		this.equipment.Write(_writer);
		_writer.Write(this.attachedToEntityId);
		_writer.Write(this.entityName);
		_writer.Write(this.isPlayer);
		if (this.isPlayer)
		{
			_writer.Write(this.killedZombies);
			_writer.Write(this.killedPlayers);
			_writer.Write(this.experience);
			_writer.Write(this.level);
			_writer.Write(this.totalItemsCrafted);
			_writer.Write(this.distanceWalked);
			_writer.Write(this.longestLife);
			_writer.Write(this.currentLife);
			_writer.Write(this.totalTimePlayed);
			_writer.Write(this.vehiclePose);
			_writer.Write(this.isSpectator);
		}
		_writer.Write(this.hasProgression);
		if (this.hasProgression)
		{
			_writer.Write((short)this.progStream.Length);
			this.progStream.WriteTo(_writer.BaseStream);
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		EntityAlive entityAlive = _world.GetEntity(this.entityId) as EntityAlive;
		if (!entityAlive)
		{
			Log.Out("Discarding " + base.GetType().Name + " for entity Id=" + this.entityId.ToString());
			return;
		}
		if (!base.ValidEntityIdForSender(this.entityId, true))
		{
			return;
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && entityAlive is EntityPlayer)
		{
			this.entityName = base.Sender.playerName;
		}
		entityAlive.Died = this.killed;
		entityAlive.DeathHealth = this.deathHealth;
		entityAlive.TeamNumber = this.teamNumber;
		entityAlive.inventory.bResetLightLevelWhenChanged = true;
		if (!entityAlive.inventory.GetItem((int)this.holdingItemIndex).Equals(this.holdingItemStack))
		{
			entityAlive.inventory.SetItem((int)this.holdingItemIndex, this.holdingItemStack);
		}
		if (entityAlive.inventory.holdingItemIdx != (int)this.holdingItemIndex)
		{
			entityAlive.inventory.SetHoldingItemIdxNoHolsterTime((int)this.holdingItemIndex);
		}
		entityAlive.equipment.Apply(this.equipment, false);
		if (this.hasProgression)
		{
			using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(false))
			{
				PooledExpandableMemoryStream obj = this.progStream;
				lock (obj)
				{
					pooledBinaryReader.SetBaseStream(this.progStream);
					this.progStream.Position = 0L;
					entityAlive.Progression = Progression.Read(pooledBinaryReader, entityAlive);
				}
			}
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && entityAlive.Progression != null)
			{
				entityAlive.Progression.bProgressionStatsChanged = true;
			}
		}
		entityAlive.SetEntityName(this.entityName);
		EntityPlayer entityPlayer = entityAlive as EntityPlayer;
		if (entityPlayer != null && this.isPlayer)
		{
			if (entityAlive.NavObject != null)
			{
				entityAlive.NavObject.name = this.entityName;
			}
			entityAlive.KilledZombies = this.killedZombies;
			entityAlive.KilledPlayers = this.killedPlayers;
			entityPlayer.Progression.ExpToNextLevel = this.experience;
			entityPlayer.Progression.Level = this.level;
			entityPlayer.totalItemsCrafted = this.totalItemsCrafted;
			entityPlayer.distanceWalked = this.distanceWalked;
			entityPlayer.longestLife = this.longestLife;
			entityPlayer.currentLife = this.currentLife;
			entityPlayer.totalTimePlayed = this.totalTimePlayed;
			entityPlayer.SetVehiclePoseMode(this.vehiclePose);
			entityPlayer.IsSpectator = this.isSpectator;
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackagePlayerStats>().Setup(entityAlive), false, -1, base.Sender.entityId, -1, null, 192);
		}
	}

	public override int GetLength()
	{
		return 60;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int entityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public int experience;

	[PublicizedFrom(EAccessModifier.Private)]
	public int level;

	[PublicizedFrom(EAccessModifier.Private)]
	public int killed;

	[PublicizedFrom(EAccessModifier.Private)]
	public int killedZombies;

	[PublicizedFrom(EAccessModifier.Private)]
	public int killedPlayers;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack holdingItemStack;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte holdingItemIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public int deathHealth;

	[PublicizedFrom(EAccessModifier.Private)]
	public int teamNumber;

	[PublicizedFrom(EAccessModifier.Private)]
	public Equipment equipment;

	[PublicizedFrom(EAccessModifier.Private)]
	public PooledExpandableMemoryStream progStream = MemoryPools.poolMemoryStream.AllocSync(true);

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasProgression;

	[PublicizedFrom(EAccessModifier.Private)]
	public int attachedToEntityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public string entityName;

	[PublicizedFrom(EAccessModifier.Private)]
	public float distanceWalked;

	[PublicizedFrom(EAccessModifier.Private)]
	public uint totalItemsCrafted;

	[PublicizedFrom(EAccessModifier.Private)]
	public float longestLife;

	[PublicizedFrom(EAccessModifier.Private)]
	public float currentLife;

	[PublicizedFrom(EAccessModifier.Private)]
	public float totalTimePlayed;

	[PublicizedFrom(EAccessModifier.Private)]
	public int vehiclePose;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isSpectator;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isPlayer;
}

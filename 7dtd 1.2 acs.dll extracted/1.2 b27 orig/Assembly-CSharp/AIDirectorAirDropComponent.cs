using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class AIDirectorAirDropComponent : AIDirectorComponent
{
	[PublicizedFrom(EAccessModifier.Private)]
	public ulong calcNextAirdrop(ulong currentTime, ulong dropFrequency)
	{
		ulong num = currentTime / 1000UL;
		ulong result;
		if (dropFrequency % 24UL == 0UL)
		{
			result = (dropFrequency + num) / 24UL * 24000UL + 12000UL;
		}
		else
		{
			result = (dropFrequency + num) * 1000UL;
		}
		return result;
	}

	public override void Connect()
	{
		base.Connect();
	}

	public override void InitNewGame()
	{
		base.InitNewGame();
		this.activeAirDrop = null;
		this.lastAirdropCheckTime = this.Director.World.worldTime;
		this.nextAirDropTime = this.calcNextAirdrop(this.lastAirdropCheckTime, (ulong)((long)GameStats.GetInt(EnumGameStats.AirDropFrequency)));
		this.supplyCrates.Clear();
	}

	public override void Tick(double _dt)
	{
		base.Tick(_dt);
		if (this.activeAirDrop != null)
		{
			if (this.activeAirDrop.Tick((float)_dt))
			{
				this.activeAirDrop = null;
				return;
			}
		}
		else
		{
			ulong num = (ulong)((long)GameStats.GetInt(EnumGameStats.AirDropFrequency));
			ulong worldTime = this.Director.World.worldTime;
			if (!GameUtils.IsPlaytesting() && num > 0UL)
			{
				if (worldTime >= this.nextAirDropTime)
				{
					if (this.SpawnAirDrop())
					{
						this.lastAirdropCheckTime = worldTime;
						this.nextAirDropTime = this.calcNextAirdrop(worldTime, num);
						return;
					}
				}
				else if (num != this.lastFrequency || worldTime < this.lastAirdropCheckTime)
				{
					this.nextAirDropTime = this.calcNextAirdrop(worldTime, num);
					this.lastFrequency = num;
					this.lastAirdropCheckTime = worldTime;
				}
			}
		}
	}

	public override void Read(BinaryReader _stream, int _version)
	{
		base.Read(_stream, _version);
		this.nextAirDropTime = _stream.ReadUInt64();
		if (_version >= 9)
		{
			this.lastFrequency = _stream.ReadUInt64();
		}
		else
		{
			this.lastFrequency = (ulong)((long)GameStats.GetInt(EnumGameStats.AirDropFrequency));
		}
		this.supplyCrates.Clear();
		int num = _stream.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			int num2 = _stream.ReadInt32();
			if (num2 > this.lastID)
			{
				this.lastID = num2;
			}
			Vector3i blockPos = StreamUtils.ReadVector3i(_stream);
			this.supplyCrates.Add(new AIDirectorAirDropComponent.SupplyCrateCache(num2, blockPos));
		}
	}

	public override void Write(BinaryWriter _stream)
	{
		base.Write(_stream);
		_stream.Write(this.nextAirDropTime);
		_stream.Write(this.lastFrequency);
		_stream.Write(this.supplyCrates.Count);
		for (int i = 0; i < this.supplyCrates.Count; i++)
		{
			_stream.Write(this.supplyCrates[i].entityId);
			StreamUtils.Write(_stream, this.supplyCrates[i].blockPos);
		}
	}

	public bool SpawnAirDrop()
	{
		bool result = false;
		if (this.activeAirDrop == null)
		{
			List<EntityPlayer> list = new List<EntityPlayer>();
			DictionaryList<int, AIDirectorPlayerState> trackedPlayers = this.Director.GetComponent<AIDirectorPlayerManagementComponent>().trackedPlayers;
			for (int i = 0; i < trackedPlayers.list.Count; i++)
			{
				AIDirectorPlayerState aidirectorPlayerState = trackedPlayers.list[i];
				if (!aidirectorPlayerState.Player.IsDead())
				{
					list.Add(aidirectorPlayerState.Player);
				}
			}
			if (list.Count > 0)
			{
				this.activeAirDrop = new AIAirDrop(this, this.Director.World, list);
				result = true;
			}
		}
		return result;
	}

	public void RemoveSupplyCrate(int entityId)
	{
		int num = -1;
		for (int i = 0; i < this.supplyCrates.Count; i++)
		{
			if (this.supplyCrates[i].entityId == entityId)
			{
				num = i;
				break;
			}
		}
		if (num > -1)
		{
			this.supplyCrates.RemoveAt(num);
			return;
		}
		Log.Error("AIDirectorAirDropComponent: Attempted to remove supply crate cache with missing entityID {0}", new object[]
		{
			entityId
		});
	}

	public void SetSupplyCratePosition(int entityId, Vector3i blockPos)
	{
		foreach (AIDirectorAirDropComponent.SupplyCrateCache supplyCrateCache in this.supplyCrates)
		{
			if (supplyCrateCache.entityId == entityId)
			{
				supplyCrateCache.blockPos = blockPos;
				break;
			}
		}
	}

	public EntitySupplyCrate SpawnSupplyCrate(Vector3 spawnPos)
	{
		if (this.Director.World == null)
		{
			return null;
		}
		if (this.supplyCrates.Count >= 12)
		{
			Entity entity = this.Director.World.GetEntity(this.supplyCrates[0].entityId);
			if (entity != null)
			{
				entity.MarkToUnload();
			}
			this.supplyCrates.RemoveAt(0);
		}
		Entity entity2 = EntityFactory.CreateEntity(EntityClass.FromString(AIDirectorAirDropComponent.crateTypes[base.Random.RandomRange(0, AIDirectorAirDropComponent.crateTypes.Length)]), spawnPos, new Vector3(base.Random.RandomFloat * 360f, 0f, 0f));
		this.Director.World.SpawnEntityInWorld(entity2);
		this.supplyCrates.Add(new AIDirectorAirDropComponent.SupplyCrateCache(entity2.entityId, World.worldToBlockPos(entity2.position)));
		return entity2 as EntitySupplyCrate;
	}

	public List<AIDirectorAirDropComponent.SupplyCrateCache> supplyCrates = new List<AIDirectorAirDropComponent.SupplyCrateCache>();

	[PublicizedFrom(EAccessModifier.Private)]
	public AIAirDrop activeAirDrop;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong nextAirDropTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong lastAirdropCheckTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong lastFrequency;

	[PublicizedFrom(EAccessModifier.Private)]
	public int lastID = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string[] crateTypes = new string[]
	{
		"sc_General"
	};

	[Preserve]
	public class SupplyCrateCache
	{
		public SupplyCrateCache(int id, Vector3i blockPos)
		{
			this.entityId = id;
			this.blockPos = blockPos;
		}

		public int entityId;

		public Vector3i blockPos;
	}
}

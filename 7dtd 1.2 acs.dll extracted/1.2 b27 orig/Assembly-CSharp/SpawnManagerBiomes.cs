using System;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManagerBiomes : SpawnManagerAbstract
{
	public SpawnManagerBiomes(World _world) : base(_world)
	{
		_world.EntityUnloadedDelegates += this.OnEntityUnloaded;
	}

	public void Cleanup()
	{
		this.world.EntityUnloadedDelegates -= this.OnEntityUnloaded;
		this.world = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEntityUnloaded(Entity entity, EnumRemoveEntityReason _reason)
	{
		if (_reason == EnumRemoveEntityReason.Undef || _reason == EnumRemoveEntityReason.Unloaded)
		{
			return;
		}
		if (entity.GetSpawnerSource() != EnumSpawnerSource.Biome)
		{
			return;
		}
		Chunk chunk = (Chunk)this.world.GetChunkSync(entity.GetSpawnerSourceChunkKey());
		if (chunk == null)
		{
			return;
		}
		ChunkAreaBiomeSpawnData chunkBiomeSpawnData = chunk.GetChunkBiomeSpawnData();
		if (chunkBiomeSpawnData == null)
		{
			return;
		}
		int spawnerSourceBiomeIdHash = entity.GetSpawnerSourceBiomeIdHash();
		if (_reason == EnumRemoveEntityReason.Despawned)
		{
			chunkBiomeSpawnData.DecCount(spawnerSourceBiomeIdHash, false);
			return;
		}
		if (_reason == EnumRemoveEntityReason.Killed)
		{
			EntityHuman entityHuman = entity as EntityHuman;
			if (entityHuman && this.world.worldTime >= entityHuman.timeToDie)
			{
				chunkBiomeSpawnData.DecCount(spawnerSourceBiomeIdHash, false);
				return;
			}
			chunkBiomeSpawnData.DecCount(spawnerSourceBiomeIdHash, true);
		}
	}

	public override void Update(string _spawnerName, bool _bSpawnEnemyEntities, object _userData)
	{
		if (!GameUtils.IsPlaytesting())
		{
			this.SpawnUpdate(_spawnerName, _bSpawnEnemyEntities, _userData as ChunkAreaBiomeSpawnData);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SpawnUpdate(string _spawnerName, bool _isSpawnEnemy, ChunkAreaBiomeSpawnData _spawnData)
	{
		if (_spawnData == null)
		{
			return;
		}
		if (_isSpawnEnemy)
		{
			if (!AIDirector.CanSpawn(1f))
			{
				_isSpawnEnemy = false;
			}
			else if (this.world.aiDirector.BloodMoonComponent.BloodMoonActive)
			{
				_isSpawnEnemy = false;
			}
		}
		if (!_isSpawnEnemy && GameStats.GetInt(EnumGameStats.AnimalCount) >= GamePrefs.GetInt(EnumGamePrefs.MaxSpawnedAnimals))
		{
			return;
		}
		bool flag = false;
		List<EntityPlayer> players = this.world.GetPlayers();
		for (int i = 0; i < players.Count; i++)
		{
			EntityPlayer entityPlayer = players[i];
			if (entityPlayer.Spawned)
			{
				Rect rect = new Rect(entityPlayer.position.x - 40f, entityPlayer.position.z - 40f, 80f, 80f);
				if (rect.Overlaps(_spawnData.area))
				{
					flag = true;
					break;
				}
			}
		}
		if (!flag)
		{
			return;
		}
		int minDistance = _isSpawnEnemy ? 28 : 48;
		int unused_maxDistance = _isSpawnEnemy ? 54 : 70;
		Vector3 vector;
		if (!this.world.GetRandomSpawnPositionInAreaMinMaxToPlayers(_spawnData.area, minDistance, unused_maxDistance, true, out vector))
		{
			return;
		}
		BiomeDefinition biome = this.world.Biomes.GetBiome(_spawnData.biomeId);
		if (biome == null)
		{
			return;
		}
		BiomeSpawnEntityGroupList biomeSpawnEntityGroupList = BiomeSpawningClass.list[biome.m_sBiomeName];
		if (biomeSpawnEntityGroupList == null)
		{
			return;
		}
		EDaytime edaytime = this.world.IsDaytime() ? EDaytime.Day : EDaytime.Night;
		GameRandom gameRandom = this.world.GetGameRandom();
		if (!_spawnData.checkedPOITags)
		{
			_spawnData.checkedPOITags = true;
			FastTags<TagGroup.Poi> fastTags = FastTags<TagGroup.Poi>.none;
			Vector3i worldPos = _spawnData.chunk.GetWorldPos();
			this.world.GetPOIsAtXZ(worldPos.x + 16, worldPos.x + 80 - 16, worldPos.z + 16, worldPos.z + 80 - 16, this.spawnPIs);
			for (int j = 0; j < this.spawnPIs.Count; j++)
			{
				PrefabInstance prefabInstance = this.spawnPIs[j];
				fastTags |= prefabInstance.prefab.Tags;
			}
			_spawnData.poiTags = fastTags;
			bool isEmpty = fastTags.IsEmpty;
			for (int k = 0; k < biomeSpawnEntityGroupList.list.Count; k++)
			{
				BiomeSpawnEntityGroupData biomeSpawnEntityGroupData = biomeSpawnEntityGroupList.list[k];
				if ((biomeSpawnEntityGroupData.POITags.IsEmpty || biomeSpawnEntityGroupData.POITags.Test_AnySet(fastTags)) && (isEmpty || biomeSpawnEntityGroupData.noPOITags.IsEmpty || !biomeSpawnEntityGroupData.noPOITags.Test_AnySet(fastTags)))
				{
					_spawnData.groupsEnabledFlags |= 1 << k;
				}
			}
		}
		int num = 0;
		int num2 = -1;
		int num3 = gameRandom.RandomRange(biomeSpawnEntityGroupList.list.Count);
		int num4 = Utils.FastMin(5, biomeSpawnEntityGroupList.list.Count);
		int l = 0;
		while (l < num4)
		{
			BiomeSpawnEntityGroupData biomeSpawnEntityGroupData2 = biomeSpawnEntityGroupList.list[num3];
			if ((_spawnData.groupsEnabledFlags & 1 << num3) != 0 && (biomeSpawnEntityGroupData2.daytime == EDaytime.Any || biomeSpawnEntityGroupData2.daytime == edaytime))
			{
				bool flag2 = EntityGroups.IsEnemyGroup(biomeSpawnEntityGroupData2.entityGroupName);
				if (!flag2 || _isSpawnEnemy)
				{
					num = biomeSpawnEntityGroupData2.idHash;
					ulong delayWorldTime = _spawnData.GetDelayWorldTime(num);
					if (this.world.worldTime > delayWorldTime)
					{
						int num5 = biomeSpawnEntityGroupData2.maxCount;
						if (flag2)
						{
							num5 = EntitySpawner.ModifySpawnCountByGameDifficulty(num5);
						}
						_spawnData.ResetRespawn(num, this.world, num5);
					}
					if (_spawnData.CanSpawn(num))
					{
						num2 = num3;
						break;
					}
				}
			}
			l++;
			num3 = (num3 + 1) % biomeSpawnEntityGroupList.list.Count;
		}
		if (num2 < 0)
		{
			return;
		}
		Bounds bb = new Bounds(vector, new Vector3(4f, 2.5f, 4f));
		this.world.GetEntitiesInBounds(typeof(Entity), bb, this.spawnNearList);
		int count = this.spawnNearList.Count;
		this.spawnNearList.Clear();
		if (count > 0)
		{
			return;
		}
		int randomFromGroup = EntityGroups.GetRandomFromGroup(biomeSpawnEntityGroupList.list[num2].entityGroupName, ref this.lastClassId, null);
		if (randomFromGroup == 0)
		{
			_spawnData.DecMaxCount(num);
			return;
		}
		_spawnData.IncCount(num);
		Entity entity = EntityFactory.CreateEntity(randomFromGroup, vector);
		entity.SetSpawnerSource(EnumSpawnerSource.Biome, _spawnData.chunk.Key, num);
		this.world.SpawnEntityInWorld(entity);
		this.world.DebugAddSpawnedEntity(entity);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LogSpawn(Entity _entity, string format, params object[] args)
	{
		format = string.Format("{0} SpawnManagerBiomes {1}, {2}", GameManager.frameCount, _entity ? _entity.ToString() : "", format);
		Log.Warning(format, args);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cAnimalMinDistance = 48;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cAnimalMaxDistance = 70;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cEnemyMinDistance = 28;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cEnemyMaxDistance = 54;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Entity> spawnNearList = new List<Entity>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int lastClassId;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<PrefabInstance> spawnPIs = new List<PrefabInstance>();
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;

public class SleeperVolume
{
	public bool IsTrigger
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.TriggeredByIndices.Count > 0;
		}
	}

	public bool IsTriggerAndNoRespawn
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return (this.flags & 7) == 3 && this.respawnMap.Count == 0;
		}
	}

	public static void WorldInit()
	{
		SleeperVolume.sleeperRandom = GameRandomManager.Instance.CreateGameRandom();
	}

	public static SleeperVolume Create(Prefab.PrefabSleeperVolume psv, Vector3i _boxMin, Vector3i _boxMax)
	{
		SleeperVolume sleeperVolume = new SleeperVolume();
		sleeperVolume.SetMinMax(_boxMin, _boxMax);
		sleeperVolume.groupId = psv.groupId;
		sleeperVolume.isQuestExclude = psv.isQuestExclude;
		sleeperVolume.isPriority = psv.isPriority;
		sleeperVolume.spawnCountMin = psv.spawnCountMin;
		sleeperVolume.spawnCountMax = psv.spawnCountMax;
		sleeperVolume.flags = psv.flags;
		sleeperVolume.TriggeredByIndices = new List<byte>(psv.triggeredByIndices);
		sleeperVolume.groupName = GameStageGroup.CleanName(psv.groupName);
		sleeperVolume.SetScript(psv.minScript);
		sleeperVolume.AddToPrefabInstance();
		return sleeperVolume;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetMinMax(Vector3i _boxMin, Vector3i _boxMax)
	{
		this.BoxMin = _boxMin;
		this.BoxMax = _boxMax;
		this.Center = (this.BoxMin + this.BoxMax).ToVector3() * 0.5f;
	}

	public bool Intersects(Bounds bounds)
	{
		return BoundsUtils.Intersects(bounds, this.BoxMin, this.BoxMax);
	}

	public PrefabInstance PrefabInstance
	{
		get
		{
			return this.prefabInstance;
		}
	}

	public void AddToPrefabInstance()
	{
		DynamicPrefabDecorator dynamicPrefabDecorator = GameManager.Instance.World.ChunkCache.ChunkProvider.GetDynamicPrefabDecorator();
		this.prefabInstance = dynamicPrefabDecorator.GetPrefabAtPosition(this.Center, true);
		if (this.prefabInstance != null)
		{
			this.prefabInstance.AddSleeperVolume(this);
		}
	}

	public void AddSpawnPoint(int _x, int _y, int _z, BlockSleeper _block, BlockValue _blockValue)
	{
		if (this.spawnPointList.Count < 255)
		{
			this.spawnPointList.Add(new SleeperVolume.SpawnPoint(new Vector3i(_x, _y, _z), _block.GetSleeperRotation(_blockValue), _blockValue.type));
		}
	}

	public void SetScript(string _script)
	{
		if (string.IsNullOrEmpty(_script))
		{
			this.minScript = null;
			return;
		}
		this.minScript = new MinScript();
		this.minScript.SetText(_script);
	}

	public void Tick(World _world)
	{
		if (this.isSpawning)
		{
			if (this.minScript != null && this.minScript.IsRunning())
			{
				foreach (KeyValuePair<int, SleeperVolume.RespawnData> keyValuePair in this.respawnMap)
				{
					if (!_world.GetEntity(keyValuePair.Key))
					{
						this.respawnMap.Clear();
						this.respawnList = null;
						this.minScript.Restart();
						break;
					}
				}
				this.minScript.Tick(this);
			}
			if (SleeperVolume.TickSpawnCount < 2)
			{
				this.UpdateSpawn(_world);
			}
		}
		if (!this.isSpawning)
		{
			if (this.isSpawned)
			{
				if (this.respawnMap.Count == 0)
				{
					this.isSpawned = false;
				}
				foreach (KeyValuePair<int, SleeperVolume.RespawnData> keyValuePair2 in this.respawnMap)
				{
					if (!_world.GetEntity(keyValuePair2.Key))
					{
						this.isSpawned = false;
						break;
					}
				}
			}
			if (this.playerTouchedToUpdate != null)
			{
				this.UpdatePlayerTouched(_world, this.playerTouchedToUpdate);
				this.playerTouchedToUpdate = null;
				return;
			}
			int num = this.ticksUntilDespawn - 1;
			this.ticksUntilDespawn = num;
			if (num == 0)
			{
				this.Despawn(_world);
			}
		}
	}

	public int GetPlayerTouchedToUpdateId()
	{
		int result = -1;
		if (this.playerTouchedToUpdate != null)
		{
			result = this.playerTouchedToUpdate.entityId;
		}
		return result;
	}

	public int GetPlayerTouchedTriggerId()
	{
		int result = -1;
		if (this.playerTouchedTrigger != null)
		{
			result = this.playerTouchedTrigger.entityId;
		}
		return result;
	}

	public void DespawnAndReset(World _world)
	{
		this.Despawn(_world);
		this.Reset();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Despawn(World _world)
	{
		this.triggerState = SleeperVolume.ETriggerType.Passive;
		this.playerTouchedTrigger = null;
		int num = 0;
		foreach (KeyValuePair<int, SleeperVolume.RespawnData> keyValuePair in this.respawnMap)
		{
			EntityAlive entityAlive = _world.GetEntity(keyValuePair.Key) as EntityAlive;
			if (entityAlive && entityAlive.IsSleeping)
			{
				entityAlive.IsDespawned = true;
				entityAlive.MarkToUnload();
				num++;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Reset()
	{
		this.playerTouchedToUpdate = null;
		this.playerTouchedTrigger = null;
		this.respawnTime = ulong.MaxValue;
		this.isSpawning = false;
		this.isSpawned = false;
		this.wasCleared = false;
		this.numSpawned = 0;
		this.respawnMap.Clear();
		this.respawnList = null;
		if (this.minScript != null)
		{
			this.minScript.Reset();
		}
	}

	public int GetAliveCount()
	{
		int num = 0;
		for (int i = 0; i < this.groupCountList.Count; i++)
		{
			num += this.groupCountList[i].count;
		}
		return num - this.numSpawned + this.respawnMap.Count;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdatePlayerTouched(World _world, EntityPlayer _playerTouched)
	{
		if (this.isSpawned || (_world.worldTime < this.respawnTime && this.wasCleared))
		{
			return;
		}
		if (_world.worldTime >= this.respawnTime)
		{
			this.Reset();
		}
		this.isSpawning = true;
		this.isSpawned = true;
		float num = 1f;
		if (this.prefabInstance != null)
		{
			num = ((this.prefabInstance.LastQuestClass == null) ? 1f : this.prefabInstance.LastQuestClass.SpawnMultiplier);
			byte difficultyTier = this.prefabInstance.prefab.DifficultyTier;
			num *= (((int)difficultyTier < SleeperVolume.difficultyTierScale.Length) ? SleeperVolume.difficultyTierScale[(int)difficultyTier] : SleeperVolume.difficultyTierScale[SleeperVolume.difficultyTierScale.Length - 1]);
			if (this.prefabInstance.LastRefreshType.Test_AnySet(QuestEventManager.banditTag))
			{
				num = 0.2f;
			}
		}
		if (this.spawnPointList.Count > 0)
		{
			int num2 = 0;
			this.gameStage = Mathf.Max(0, this.GetGameStageAround(_playerTouched) + num2);
			if (this.respawnMap.Count > 0)
			{
				this.respawnList = new List<int>(this.respawnMap.Count);
				foreach (KeyValuePair<int, SleeperVolume.RespawnData> keyValuePair in this.respawnMap)
				{
					this.respawnList.Add(keyValuePair.Key);
				}
			}
			this.ResetSpawnsAvailable();
			if (this.groupCountList != null)
			{
				this.groupCountList.Clear();
			}
			if (this.spawnCountMin < 0 || this.spawnCountMax < 0)
			{
				this.spawnCountMin = 5;
				this.spawnCountMax = 6;
			}
			this.AddSpawnCount(this.groupName, (float)this.spawnCountMin * num, (float)this.spawnCountMax * num);
			this.spawnDelay = 0;
		}
		if (this.minScript != null)
		{
			this.minScript.Run(this, _playerTouched, num);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ResetSpawnsAvailable()
	{
		bool flag = false;
		if (this.prefabInstance != null && this.prefabInstance.LastRefreshType.Test_AnySet(QuestEventManager.infestedTag))
		{
			flag = true;
		}
		this.spawnsAvailable = new List<int>(this.spawnPointList.Count);
		for (int i = 0; i < this.spawnPointList.Count; i++)
		{
			if (flag || this.spawnPointList[i].GetBlock().spawnMode != BlockSleeper.eMode.Infested)
			{
				this.spawnsAvailable.Add(i);
			}
		}
	}

	public void AddSpawnCount(string _groupName, float _min, float _max)
	{
		SleeperVolume.GroupCount item;
		item.groupName = _groupName;
		float num = SleeperVolume.sleeperRandom.RandomRange(_min, _max);
		int num2 = (int)num;
		if (SleeperVolume.sleeperRandom.RandomFloat < num - (float)num2)
		{
			num2++;
		}
		if (_min > 0f && num2 == 0)
		{
			num2 = 1;
		}
		item.count = num2;
		if (num2 > 0)
		{
			if (this.groupCountList == null)
			{
				this.groupCountList = new List<SleeperVolume.GroupCount>();
			}
			this.groupCountList.Add(item);
		}
	}

	public void CheckTouching(World _world, EntityPlayer _player)
	{
		if (this.IsTriggerAndNoRespawn || _player.IsSpectator)
		{
			return;
		}
		Vector3 position = _player.position;
		position.y += 0.8f;
		SleeperVolume.ETriggerType etriggerType = (SleeperVolume.ETriggerType)(this.flags & 7);
		if (this.hasPassives)
		{
			if (position.x >= (float)this.BoxMin.x - -0.3f && position.x < (float)this.BoxMax.x + -0.3f && position.y >= (float)this.BoxMin.y && position.y < (float)this.BoxMax.y && position.z >= (float)this.BoxMin.z - -0.3f && position.z < (float)this.BoxMax.z + -0.3f && etriggerType != SleeperVolume.ETriggerType.Passive)
			{
				this.TouchGroup(_world, _player, true);
			}
		}
		else if ((etriggerType == SleeperVolume.ETriggerType.Attack || etriggerType == SleeperVolume.ETriggerType.Trigger) && this.triggerState != etriggerType && position.x >= (float)this.BoxMin.x - -0.1f && position.x < (float)this.BoxMax.x + -0.1f && position.y >= (float)this.BoxMin.y && position.y < (float)this.BoxMax.y && position.z >= (float)this.BoxMin.z - -0.1f && position.z < (float)this.BoxMax.z + -0.1f)
		{
			this.TouchGroup(_world, _player, true);
		}
		if (this.playerTouchedToUpdate == null && this.CheckTrigger(_world, position))
		{
			this.TouchGroup(_world, _player, false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TouchGroup(World _world, EntityPlayer _player, bool setActive)
	{
		SleeperVolume.ETriggerType trigger = (SleeperVolume.ETriggerType)(this.flags & 7);
		if (this.groupId == 0 || this.prefabInstance == null)
		{
			this.Touch(_world, _player, setActive, trigger);
			return;
		}
		List<SleeperVolume> sleeperVolumes = this.prefabInstance.sleeperVolumes;
		for (int i = 0; i < sleeperVolumes.Count; i++)
		{
			SleeperVolume sleeperVolume = sleeperVolumes[i];
			if (sleeperVolume.groupId == this.groupId && !sleeperVolume.IsTriggerAndNoRespawn)
			{
				sleeperVolume.Touch(_world, _player, setActive, trigger);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Touch(World _world, EntityPlayer _player, bool setActive, SleeperVolume.ETriggerType trigger)
	{
		if (setActive)
		{
			bool flag = (trigger == SleeperVolume.ETriggerType.Attack || trigger == SleeperVolume.ETriggerType.Trigger) && _player;
			foreach (KeyValuePair<int, SleeperVolume.RespawnData> keyValuePair in this.respawnMap)
			{
				int key = keyValuePair.Key;
				EntityAlive entityAlive = (EntityAlive)_world.GetEntity(key);
				if (entityAlive)
				{
					if (flag && _player.Stealth.CanSleeperAttackDetect(entityAlive))
					{
						entityAlive.ConditionalTriggerSleeperWakeUp();
						entityAlive.SetAttackTarget(_player, 400);
					}
					else if (trigger == SleeperVolume.ETriggerType.Wander)
					{
						entityAlive.ConditionalTriggerSleeperWakeUp();
					}
					else if (--SleeperVolume.wanderingCountdown <= 0)
					{
						SleeperVolume.wanderingCountdown = 10;
						entityAlive.ConditionalTriggerSleeperWakeUp();
					}
					else
					{
						entityAlive.SetSleeperActive();
					}
				}
			}
			this.hasPassives = false;
			this.triggerState = trigger;
			return;
		}
		this.playerTouchedToUpdate = _player;
		this.ticksUntilDespawn = 900;
		if (this.hasPassives)
		{
			this.ticksUntilDespawn = 200;
		}
		if (this.wasCleared && _world.worldTime < this.respawnTime)
		{
			this.respawnTime = Math.Max(this.respawnTime, _world.worldTime + 1000UL);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool CheckTrigger(World _world, Vector3 playerPos)
	{
		if (this.isSpawned)
		{
			Vector3i vector3i = this.BoxMin - SleeperVolume.unpadding;
			Vector3i vector3i2 = this.BoxMax + SleeperVolume.unpadding;
			return playerPos.x >= (float)vector3i.x && playerPos.x < (float)vector3i2.x && playerPos.y >= (float)vector3i.y && playerPos.y < (float)vector3i2.y && playerPos.z >= (float)vector3i.z && playerPos.z < (float)vector3i2.z;
		}
		Vector3i vector3i3 = this.BoxMin - SleeperVolume.triggerPaddingMin;
		Vector3i vector3i4 = this.BoxMax + SleeperVolume.triggerPaddingMax;
		if (playerPos.x < (float)vector3i3.x || playerPos.x >= (float)vector3i4.x || playerPos.y < (float)vector3i3.y || playerPos.y >= (float)vector3i4.y || playerPos.z < (float)vector3i3.z || playerPos.z >= (float)vector3i4.z)
		{
			return false;
		}
		if (!this.wasCleared)
		{
			if (this.prefabInstance != null)
			{
				_world.UncullPOI(this.prefabInstance);
			}
			return true;
		}
		if (GameUtils.CheckForAnyPlayerHome(GameManager.Instance.World, this.BoxMin, this.BoxMax) != GameUtils.EPlayerHomeType.None)
		{
			this.respawnTime = Math.Max(this.respawnTime, _world.worldTime + 24000UL);
			return false;
		}
		return true;
	}

	public void CheckNoise(World _world, Vector3 pos)
	{
		if (this.hasPassives && pos.x >= (float)this.BoxMin.x - 0.9f && pos.x < (float)this.BoxMax.x + 0.9f && pos.y >= (float)this.BoxMin.y - 0.9f && pos.y < (float)this.BoxMax.y + 0.9f && pos.z >= (float)this.BoxMin.z - 0.9f && pos.z < (float)this.BoxMax.z + 0.9f && (this.flags & 7) != 1)
		{
			this.TouchGroup(_world, null, true);
		}
	}

	public void OnTriggered(EntityPlayer _player, World _world, int _triggerIndex)
	{
		this.triggerState = (SleeperVolume.ETriggerType)(this.flags & 7);
		this.playerTouchedTrigger = _player;
		this.UpdatePlayerTouched(_world, _player);
	}

	public void EntityDied(EntityAlive entity)
	{
		if (!this.respawnMap.Remove(entity.entityId))
		{
			return;
		}
		if (this.respawnList != null)
		{
			this.respawnList.Remove(entity.entityId);
		}
		int num = this.numSpawned;
		int count = this.respawnMap.Count;
		if (!this.isSpawning && !this.wasCleared && this.respawnMap.Count == 0)
		{
			this.SetRespawnTime(entity.world);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetRespawnTime(World _world)
	{
		int num = GamePrefs.GetInt(EnumGamePrefs.LootRespawnDays);
		if (num <= 0)
		{
			num = 30;
		}
		this.respawnTime = _world.worldTime + (ulong)(num * 24000);
		this.wasCleared = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int GetGameStageAround(EntityPlayer player)
	{
		return GameStageDefinition.CalcGameStageAround(player);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool SpawnPointIsHidden(World _world, int _index)
	{
		SleeperVolume.SpawnPoint spawnPoint = this.spawnPointList[_index];
		Vector3 a = spawnPoint.pos.ToVector3();
		a.x += 0.5f;
		a.z += 0.5f;
		int num = 0;
		if (spawnPoint.GetBlock().pose == 5)
		{
			num = 1;
		}
		float[] array = SleeperVolume.isHiddenOffsets[num];
		for (int i = 0; i < _world.Players.list.Count; i++)
		{
			EntityPlayer entityPlayer = _world.Players.list[i];
			Vector3 headPosition = entityPlayer.getHeadPosition();
			int modelLayer = entityPlayer.GetModelLayer();
			entityPlayer.SetModelLayer(2, false, null);
			Ray ray = new Ray(headPosition, Vector3.one);
			Vector3 a2 = Vector3.Cross((a - headPosition).normalized, Vector3.up);
			for (int j = 0; j < array.Length; j += 2)
			{
				Vector3 a3 = a + a2 * array[j];
				a3.y += array[j + 1];
				Vector3 direction = a3 - headPosition;
				ray.direction = direction;
				if (!Voxel.Raycast(_world, ray, direction.magnitude, 71, 0f))
				{
					entityPlayer.SetModelLayer(modelLayer, false, null);
					return false;
				}
			}
			entityPlayer.SetModelLayer(modelLayer, false, null);
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int FindFathestSpawnFromPlayers(World _world)
	{
		int num = -1;
		float num2 = float.MinValue;
		for (int i = 0; i < this.spawnsAvailable.Count; i++)
		{
			int index = this.spawnsAvailable[i];
			Vector3i pos = this.spawnPointList[index].pos;
			if (_world.GetBlock(pos).isair)
			{
				if (this.minScript == null)
				{
					pos.y--;
					if (_world.GetBlock(pos).isair)
					{
						goto IL_10B;
					}
					pos.y++;
				}
				Vector3 a = pos.ToVector3();
				a.x += 0.5f;
				a.z += 0.5f;
				float num3 = float.MaxValue;
				for (int j = 0; j < _world.Players.list.Count; j++)
				{
					Vector3 position = _world.Players.list[j].position;
					float sqrMagnitude = (a - position).sqrMagnitude;
					if (sqrMagnitude < num3)
					{
						num3 = sqrMagnitude;
					}
				}
				if (num3 > num2)
				{
					num2 = num3;
					num = i;
				}
			}
			IL_10B:;
		}
		if (num < 0)
		{
			return -1;
		}
		int result = this.spawnsAvailable[num];
		this.spawnsAvailable.RemoveAt(num);
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RemoveSpawnAvailable(int index)
	{
		for (int i = 0; i < this.spawnsAvailable.Count; i++)
		{
			if (this.spawnsAvailable[i] == index)
			{
				this.spawnsAvailable.RemoveAt(i);
				return;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateSpawn(World _world)
	{
		int num = this.spawnDelay - 1;
		this.spawnDelay = num;
		if (num > 0)
		{
			return;
		}
		this.spawnDelay = 2;
		bool flag = AIDirector.CanSpawn(2.1f);
		int @int = GameStats.GetInt(EnumGameStats.EnemyCount);
		bool flag2 = false;
		if (this.minScript != null && this.minScript.IsRunning())
		{
			flag2 = true;
		}
		if (this.spawnsAvailable != null)
		{
			string text = Time.time.ToCultureInvariantString();
			if (this.respawnList != null && this.respawnList.Count > 0)
			{
				int num2 = this.respawnList[this.respawnList.Count - 1];
				this.respawnList.RemoveAt(this.respawnList.Count - 1);
				Entity entity = _world.GetEntity(num2);
				if (entity)
				{
					this.hasPassives = true;
					flag2 = true;
					Log.Out("{0} SleeperVolume {1}: Still alive '{2}'", new object[]
					{
						text,
						this.BoxMin,
						entity.name
					});
				}
				else
				{
					int num3 = this.respawnMap[num2].spawnPointIndex;
					if (num3 >= 0)
					{
						this.RemoveSpawnAvailable(num3);
					}
					else
					{
						num3 = this.FindSpawnIndex(_world);
					}
					if (num3 >= 0)
					{
						SleeperVolume.SpawnPoint spawnPoint = this.spawnPointList[num3];
						if (!this.CheckSpawnPos(_world, spawnPoint.pos))
						{
							this.respawnList.Add(num2);
							this.spawnsAvailable.Add(num3);
							return;
						}
						string className = this.respawnMap[num2].className;
						Log.Out("{0} SleeperVolume {1}: Restoring {2} ({3}) '{4}', count {5}", new object[]
						{
							text,
							this.BoxMin,
							spawnPoint.pos,
							World.toChunkXZ(spawnPoint.pos),
							className,
							@int
						});
						int entityClass = EntityClass.FromString(className);
						BlockSleeper block = spawnPoint.GetBlock();
						if (this.Spawn(_world, entityClass, num3, block))
						{
							this.respawnMap.Remove(num2);
						}
						flag2 = true;
					}
				}
			}
			else if (flag)
			{
				GameStageDefinition gameStageDefinition = null;
				if (this.groupCountList != null)
				{
					int num4 = 0;
					for (int i = 0; i < this.groupCountList.Count; i++)
					{
						num4 += this.groupCountList[i].count;
						if (num4 > this.numSpawned)
						{
							GameStageGroup gameStageGroup = GameStageGroup.TryGet(this.groupCountList[i].groupName);
							if (gameStageGroup != null)
							{
								gameStageDefinition = gameStageGroup.spawner;
								break;
							}
						}
					}
				}
				if (gameStageDefinition != null)
				{
					GameStageDefinition.Stage stage = gameStageDefinition.GetStage(this.gameStage);
					if (stage != null)
					{
						int num5 = this.FindSpawnIndex(_world);
						if (num5 >= 0)
						{
							SleeperVolume.SpawnPoint spawnPoint2 = this.spawnPointList[num5];
							if (!this.CheckSpawnPos(_world, spawnPoint2.pos))
							{
								this.spawnsAvailable.Add(num5);
								return;
							}
							BlockSleeper block2 = spawnPoint2.GetBlock();
							if (block2 == null)
							{
								Log.Error("{0} BlockSleeper {1} null, type {2}", new object[]
								{
									text,
									spawnPoint2.pos,
									spawnPoint2.blockType
								});
							}
							else
							{
								string spawnGroup = block2.spawnGroup;
								if (string.IsNullOrEmpty(spawnGroup))
								{
									spawnGroup = stage.GetSpawnGroup(0).groupName;
								}
								int randomFromGroup = EntityGroups.GetRandomFromGroup(spawnGroup, ref this.lastClassId, SleeperVolume.sleeperRandom);
								EntityClass entityClass2;
								EntityClass.list.TryGetValue(randomFromGroup, out entityClass2);
								Log.Out("{0} SleeperVolume {1}: Spawning {2} ({3}), group '{4}', class {5}, count {6}", new object[]
								{
									text,
									this.BoxMin,
									spawnPoint2.pos,
									World.toChunkXZ(spawnPoint2.pos),
									spawnGroup,
									(entityClass2 != null) ? entityClass2.entityClassName : "?",
									@int
								});
								if (this.Spawn(_world, randomFromGroup, num5, block2))
								{
									this.numSpawned++;
								}
								flag2 = true;
							}
						}
					}
				}
			}
		}
		if (!flag2)
		{
			this.isSpawning = false;
			this.respawnList = null;
			if (this.numSpawned == 0)
			{
				if (this.respawnMap.Count == 0)
				{
					this.wasCleared = true;
				}
				Log.Out("{0} SleeperVolume {1}: None spawned, canSpawn {2}, respawnCnt {3}", new object[]
				{
					Time.time.ToCultureInvariantString(),
					this.BoxMin,
					flag,
					this.respawnMap.Count
				});
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int FindSpawnIndex(World _world)
	{
		if (this.spawnsAvailable.Count == 0)
		{
			this.ResetSpawnsAvailable();
		}
		int num = SleeperVolume.sleeperRandom.RandomRange(0, this.spawnsAvailable.Count);
		for (int i = this.spawnsAvailable.Count; i > 0; i--)
		{
			int num2 = this.spawnsAvailable[num];
			Vector3i pos = this.spawnPointList[num2].pos;
			if (_world.GetBlock(pos).isair)
			{
				pos.y--;
				if (!_world.GetBlock(pos).isair && this.SpawnPointIsHidden(_world, num2))
				{
					this.spawnsAvailable.RemoveAt(num);
					return num2;
				}
			}
			if (++num >= this.spawnsAvailable.Count)
			{
				num = 0;
			}
		}
		return this.FindFathestSpawnFromPlayers(_world);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool CheckSpawnPos(World _world, Vector3i pos)
	{
		if (GameManager.bRecordNextSession || GameManager.bPlayRecordedSession)
		{
			return true;
		}
		Chunk chunk = (Chunk)_world.GetChunkFromWorldPos(pos);
		return chunk != null && !chunk.IsInternalBlocksCulled && !chunk.NeedsCopying && !chunk.NeedsRegeneration;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityAlive Spawn(World _world, int entityClass, int spawnIndex, BlockSleeper block)
	{
		SleeperVolume.SpawnPoint spawnPoint = this.spawnPointList[spawnIndex];
		Vector3 vector = spawnPoint.pos.ToVector3();
		vector.x += 0.502f;
		vector.z += 0.501f;
		EntityClass entityClass2;
		if (!EntityClass.list.TryGetValue(entityClass, out entityClass2))
		{
			Log.Warning("Spawn class {0} is missing", new object[]
			{
				entityClass
			});
			entityClass = EntityClass.FromString("zombieArlene");
		}
		else if (block != null && block.ExcludesWalkType(EntityAlive.GetSpawnWalkType(entityClass2)))
		{
			Log.Warning("Spawn {0} can't walk on block {1} with walkType {2}", new object[]
			{
				entityClass2.entityClassName,
				block,
				EntityAlive.GetSpawnWalkType(entityClass2)
			});
			return null;
		}
		EntityAlive entityAlive = (EntityAlive)EntityFactory.CreateEntity(entityClass, vector, new Vector3(0f, spawnPoint.rot, 0f));
		if (!entityAlive)
		{
			Log.Error("Spawn class {0} is null", new object[]
			{
				entityClass
			});
			return null;
		}
		SleeperVolume.TickSpawnCount++;
		entityAlive.SetSpawnerSource(EnumSpawnerSource.Dynamic);
		entityAlive.IsSleeperPassive = true;
		entityAlive.SleeperSpawnPosition = vector;
		entityAlive.SleeperSpawnLookDir = block.look;
		TileEntitySleeper tileEntitySleeper = _world.GetTileEntity(0, spawnPoint.pos) as TileEntitySleeper;
		if (tileEntitySleeper != null)
		{
			entityAlive.SetSleeperSight((float)tileEntitySleeper.GetSightAngle(), (float)tileEntitySleeper.GetSightRange());
			entityAlive.SetSleeperHearing(tileEntitySleeper.GetHearingPercent());
		}
		entityAlive.SetSleeper();
		entityAlive.TriggerSleeperPose(block.pose, false);
		_world.SpawnEntityInWorld(entityAlive);
		SleeperVolume.RespawnData value;
		value.className = EntityClass.list[entityClass].entityClassName;
		value.spawnPointIndex = spawnIndex;
		this.respawnMap.Add(entityAlive.entityId, value);
		this.hasPassives = true;
		if (this.playerTouchedTrigger)
		{
			GameManager.Instance.StartCoroutine(this.WakeAttackLater(entityAlive, this.playerTouchedTrigger));
		}
		return entityAlive;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator WakeAttackLater(EntityAlive _ea, EntityPlayer _playerTouched)
	{
		yield return new WaitForSeconds(1f);
		if (_ea && _playerTouched)
		{
			_ea.ConditionalTriggerSleeperWakeUp();
			_ea.SetAttackTarget(_playerTouched, 400);
		}
		yield break;
	}

	public List<SleeperVolume.SpawnPoint> GetSpawnPoints()
	{
		return this.spawnPointList;
	}

	public static SleeperVolume Read(BinaryReader _br)
	{
		SleeperVolume sleeperVolume = new SleeperVolume();
		int num = (int)_br.ReadByte();
		string name = _br.ReadString();
		name = GameStageGroup.CleanName(name);
		if (num >= 13)
		{
			if (num >= 16)
			{
				sleeperVolume.groupId = _br.ReadInt16();
			}
			sleeperVolume.spawnCountMin = _br.ReadInt16();
			sleeperVolume.spawnCountMax = _br.ReadInt16();
		}
		sleeperVolume.groupName = name;
		sleeperVolume.SetMinMax(new Vector3i(_br.ReadInt32(), _br.ReadInt32(), _br.ReadInt32()), new Vector3i(_br.ReadInt32(), _br.ReadInt32(), _br.ReadInt32()));
		sleeperVolume.respawnTime = _br.ReadUInt64();
		if (num <= 13)
		{
			_br.ReadUInt64();
		}
		sleeperVolume.numSpawned = _br.ReadInt32();
		if (num > 7)
		{
			_br.ReadInt32();
		}
		sleeperVolume.gameStage = _br.ReadInt32();
		if (num > 3)
		{
			if (num > 4)
			{
				if (num < 11)
				{
					_br.ReadString();
				}
			}
			else
			{
				_br.ReadInt32();
			}
		}
		if (num >= 10)
		{
			_br.ReadString();
			_br.ReadInt32();
		}
		if (num > 5)
		{
			sleeperVolume.ticksUntilDespawn = _br.ReadInt32();
		}
		if (num >= 14)
		{
			ushort num2 = _br.ReadUInt16();
			sleeperVolume.isQuestExclude = ((num2 & 1) > 0);
			sleeperVolume.isPriority = ((num2 & 2) > 0);
			sleeperVolume.isSpawning = ((num2 & 4) > 0);
			sleeperVolume.wasCleared = ((num2 & 8) > 0);
			if (num >= 18)
			{
				sleeperVolume.flags = _br.ReadInt32();
			}
		}
		else
		{
			sleeperVolume.isSpawning = _br.ReadBoolean();
			sleeperVolume.wasCleared = _br.ReadBoolean();
			if (num >= 12)
			{
				sleeperVolume.isQuestExclude = _br.ReadBoolean();
			}
		}
		int num3 = (int)_br.ReadByte();
		if (num3 > 0)
		{
			for (int i = 0; i < num3; i++)
			{
				sleeperVolume.spawnPointList.Add(SleeperVolume.SpawnPoint.Read(_br, num));
			}
		}
		if (num > 1)
		{
			num3 = (int)_br.ReadByte();
			if (num3 > 0)
			{
				sleeperVolume.spawnsAvailable = new List<int>(num3);
				for (int j = 0; j < num3; j++)
				{
					sleeperVolume.spawnsAvailable.Add((int)_br.ReadByte());
				}
			}
		}
		num3 = (int)_br.ReadByte();
		if (num3 > 0)
		{
			for (int k = 0; k < num3; k++)
			{
				_br.ReadInt32();
			}
			sleeperVolume.hasPassives = true;
		}
		if (num >= 8)
		{
			num3 = (int)_br.ReadByte();
			if (num3 > 0)
			{
				for (int l = 0; l < num3; l++)
				{
					int key = _br.ReadInt32();
					SleeperVolume.RespawnData value;
					value.className = _br.ReadString();
					value.spawnPointIndex = ((num >= 17) ? ((int)_br.ReadByte()) : -1);
					sleeperVolume.respawnMap.Add(key, value);
				}
			}
		}
		num3 = (int)_br.ReadByte();
		if (num3 > 0)
		{
			sleeperVolume.groupCountList = new List<SleeperVolume.GroupCount>(num3);
			for (int m = 0; m < num3; m++)
			{
				SleeperVolume.GroupCount item;
				item.groupName = name;
				if (num >= 21)
				{
					item.groupName = _br.ReadString();
				}
				item.count = _br.ReadInt32();
				sleeperVolume.groupCountList.Add(item);
			}
		}
		if (num >= 19)
		{
			num3 = (int)_br.ReadByte();
			sleeperVolume.TriggeredByIndices.Clear();
			if (num3 > 0)
			{
				for (int n = 0; n < num3; n++)
				{
					sleeperVolume.TriggeredByIndices.Add(_br.ReadByte());
				}
			}
		}
		if ((sleeperVolume.flags & 16) > 0)
		{
			sleeperVolume.minScript = MinScript.Read(_br);
		}
		return sleeperVolume;
	}

	public void Write(BinaryWriter _bw)
	{
		_bw.Write(21);
		_bw.Write(this.groupName ?? string.Empty);
		_bw.Write(this.groupId);
		_bw.Write(this.spawnCountMin);
		_bw.Write(this.spawnCountMax);
		_bw.Write(this.BoxMin.x);
		_bw.Write(this.BoxMin.y);
		_bw.Write(this.BoxMin.z);
		_bw.Write(this.BoxMax.x);
		_bw.Write(this.BoxMax.y);
		_bw.Write(this.BoxMax.z);
		_bw.Write(this.respawnTime);
		_bw.Write(this.numSpawned);
		_bw.Write(0);
		_bw.Write(this.gameStage);
		_bw.Write(string.Empty);
		_bw.Write(0);
		_bw.Write(this.ticksUntilDespawn);
		ushort num = 0;
		if (this.isQuestExclude)
		{
			num |= 1;
		}
		if (this.isPriority)
		{
			num |= 2;
		}
		if (this.isSpawning)
		{
			num |= 4;
		}
		if (this.wasCleared)
		{
			num |= 8;
		}
		_bw.Write(num);
		this.flags &= -17;
		if (this.minScript != null && this.minScript.HasData())
		{
			this.flags |= 16;
		}
		_bw.Write(this.flags);
		int count = this.spawnPointList.Count;
		_bw.Write((byte)count);
		for (int i = 0; i < count; i++)
		{
			this.spawnPointList[i].Write(_bw);
		}
		int num2 = (this.spawnsAvailable != null) ? this.spawnsAvailable.Count : 0;
		_bw.Write((byte)num2);
		for (int j = 0; j < num2; j++)
		{
			_bw.Write((byte)this.spawnsAvailable[j]);
		}
		_bw.Write(0);
		_bw.Write((byte)((this.respawnMap != null) ? this.respawnMap.Count : 0));
		if (this.respawnMap != null)
		{
			foreach (KeyValuePair<int, SleeperVolume.RespawnData> keyValuePair in this.respawnMap)
			{
				_bw.Write(keyValuePair.Key);
				_bw.Write(keyValuePair.Value.className);
				_bw.Write((byte)keyValuePair.Value.spawnPointIndex);
			}
		}
		_bw.Write((byte)((this.groupCountList != null) ? this.groupCountList.Count : 0));
		if (this.groupCountList != null)
		{
			for (int k = 0; k < this.groupCountList.Count; k++)
			{
				_bw.Write(this.groupCountList[k].groupName);
				_bw.Write(this.groupCountList[k].count);
			}
		}
		_bw.Write((byte)this.TriggeredByIndices.Count);
		for (int l = 0; l < this.TriggeredByIndices.Count; l++)
		{
			_bw.Write(this.TriggeredByIndices[l]);
		}
		if ((this.flags & 16) > 0)
		{
			this.minScript.Write(_bw);
		}
	}

	public override string ToString()
	{
		string text = (this.groupCountList != null && this.groupCountList.Count > 0) ? this.groupCountList[0].groupName : "";
		return string.Format("{0} {1} Trig{2} RespawnC{3}", new object[]
		{
			this.BoxMin,
			text,
			this.IsTrigger ? 1 : 0,
			this.respawnMap.Count
		});
	}

	[Conditional("DEBUG_SLEEPERLOG")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void LogSleeper(string format, params object[] args)
	{
		format = string.Format("{0} {1} SleeperVolume {2}", GameManager.frameTime.ToCultureInvariantString(), GameManager.frameCount, format);
		Log.Warning(format, args);
	}

	public void Draw(float _duration)
	{
		Vector3 minPos = this.BoxMin.ToVector3() - Origin.position;
		Vector3 vector = this.BoxMax.ToVector3();
		vector -= Origin.position;
		Color color = this.GetColor();
		Utils.DrawBoxLines(minPos, vector, color, _duration);
	}

	public void DrawDebugLines(float _duration)
	{
		string name = string.Format("SleeperVolume{0},{1}", this.BoxMin, this.BoxMax);
		Color color = this.GetColor();
		Vector3 vector = this.BoxMin.ToVector3();
		Vector3 vector2 = this.BoxMax.ToVector3();
		vector += DebugLines.InsideOffsetV;
		vector2 -= DebugLines.InsideOffsetV;
		DebugLines.Create(name, GameManager.Instance.World.GetPrimaryPlayer().RootTransform, color, color, 0.05f, 0.05f, _duration).AddCube(vector, vector2);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Color GetColor()
	{
		Color result = this.isQuestExclude ? Color.red : Color.green;
		if (this.respawnMap.Count > 0)
		{
			result.b = 1f;
		}
		if (this.IsTrigger)
		{
			result = new Color(0.25f, 0.25f, 0.25f);
		}
		if (this.wasCleared)
		{
			result.r *= 0.4f;
			result.g *= 0.4f;
			result.b *= 0.4f;
			result.a = 0.16f;
		}
		return result;
	}

	public string GetDescription()
	{
		long num = (long)(this.respawnTime - GameManager.Instance.World.worldTime);
		if (num < 0L)
		{
			num = 0L;
		}
		int num2 = -1;
		int num3 = 0;
		if (this.groupCountList != null)
		{
			num2 = this.groupCountList.Count;
			for (int i = 0; i > this.groupCountList.Count; i++)
			{
				num3 += this.groupCountList[i].count;
			}
		}
		return string.Format("{0}, grpId {1}, {2} ({3}), cntList {4}/{5}, respawnCnt {6}, spawned {7}, clear{8}, plHome {9}, respawnIn {10}, {11}", new object[]
		{
			this.BoxMin,
			this.groupId,
			(SleeperVolume.ETriggerType)(this.flags & 7),
			this.triggerState,
			num2,
			num3,
			this.respawnMap.Count,
			this.numSpawned,
			this.wasCleared,
			GameUtils.CheckForAnyPlayerHome(GameManager.Instance.World, this.BoxMin, this.BoxMax),
			this.DurationToString(num),
			(this.prefabInstance != null) ? (this.prefabInstance.name + ", volumes " + this.prefabInstance.sleeperVolumes.Count.ToString()) : "?"
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string DurationToString(long duration)
	{
		string str = "";
		int num = (int)((double)duration / 1000.0 / 24.0);
		if (num > 0)
		{
			str += num.ToString("0:");
		}
		int num2 = (int)((double)duration / 1000.0) % 24;
		if (num > 0 || num2 > 0)
		{
			str += num2.ToString("00:");
		}
		int num3 = (int)((double)duration / 1000.0 * 60.0) % 60;
		if (num > 0 || num2 > 0 || num3 > 0)
		{
			str += num3.ToString("00:");
		}
		return str + ((int)((double)duration / 1000.0 * 60.0 * 60.0) % 60).ToString("00");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const byte cVersion = 21;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cSpawnDelay = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cDespawnDelay = 900;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cDespawnPassiveDelay = 200;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cBedrollClearTime = 24000;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cPlayerInsideDelayTime = 1000;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cPlayerYOffset = 0.8f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cAttackPaddingXZ = -0.1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cPassivePaddingXZ = -0.3f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cPassiveNoisePadding = 0.9f;

	public static Vector3i chunkPadding = new Vector3i(12, 1, 12);

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector3i triggerPaddingMin = new Vector3i(8f, 0.7f, 8f);

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector3i triggerPaddingMax = new Vector3i(8f, 0.7f, 8f);

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector3i unpadding = new Vector3i(14, 16, 14);

	[PublicizedFrom(EAccessModifier.Private)]
	public PrefabInstance prefabInstance;

	[PublicizedFrom(EAccessModifier.Private)]
	public short groupId;

	[PublicizedFrom(EAccessModifier.Private)]
	public short spawnCountMin;

	[PublicizedFrom(EAccessModifier.Private)]
	public short spawnCountMax;

	public const int cTriggerFlagsMask = 7;

	public const int cFlagsHasScript = 16;

	[PublicizedFrom(EAccessModifier.Private)]
	public int flags;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<SleeperVolume.SpawnPoint> spawnPointList = new List<SleeperVolume.SpawnPoint>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<int> spawnsAvailable;

	[PublicizedFrom(EAccessModifier.Private)]
	public string groupName;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<SleeperVolume.GroupCount> groupCountList;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<int, SleeperVolume.RespawnData> respawnMap = new Dictionary<int, SleeperVolume.RespawnData>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<int> respawnList;

	[PublicizedFrom(EAccessModifier.Private)]
	public int gameStage;

	[PublicizedFrom(EAccessModifier.Private)]
	public int lastClassId;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong respawnTime = ulong.MaxValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public int numSpawned;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isSpawned;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isSpawning;

	[PublicizedFrom(EAccessModifier.Private)]
	public int spawnDelay;

	[PublicizedFrom(EAccessModifier.Private)]
	public int ticksUntilDespawn;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayer playerTouchedToUpdate;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayer playerTouchedTrigger;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasPassives;

	[PublicizedFrom(EAccessModifier.Private)]
	public SleeperVolume.ETriggerType triggerState = SleeperVolume.ETriggerType.Passive;

	public bool wasCleared;

	public bool isQuestExclude;

	public bool isPriority;

	public Vector3i BoxMin;

	public Vector3i BoxMax;

	public Vector3 Center;

	public List<byte> TriggeredByIndices = new List<byte>();

	[PublicizedFrom(EAccessModifier.Private)]
	public MinScript minScript;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cWanderingCountdown = 10;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int wanderingCountdown = 5;

	[PublicizedFrom(EAccessModifier.Private)]
	public static GameRandom sleeperRandom;

	public static int TickSpawnCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cSpawnPerTickMax = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	public static float[] difficultyTierScale = new float[]
	{
		1f,
		1f,
		1f,
		0.9f,
		0.9f,
		0.9f,
		0.9f
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static float[][] isHiddenOffsets = new float[][]
	{
		new float[]
		{
			-0.7f,
			0.3f,
			0f,
			0.3f,
			0.7f,
			0.3f,
			-0.7f,
			0.8f,
			0f,
			0.8f,
			0.7f,
			0.8f
		},
		new float[]
		{
			-0.4f,
			0.5f,
			0f,
			0.5f,
			0.4f,
			0.5f,
			-0.4f,
			1.5f,
			0f,
			1.5f,
			0.4f,
			1.5f
		}
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cFlagsQuestExclude = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cFlagsPriority = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cFlagsSpawning = 4;

	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cFlagsCleared = 8;

	public enum ETriggerType
	{
		Active,
		Passive,
		Attack,
		Trigger,
		Wander
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct GroupCount
	{
		public string groupName;

		public int count;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct RespawnData
	{
		public string className;

		public int spawnPointIndex;
	}

	public struct SpawnPoint
	{
		public SpawnPoint(Vector3i _pos, float _rot, int _blockType)
		{
			this.pos = _pos;
			this.rot = _rot;
			this.blockType = _blockType;
		}

		public BlockSleeper GetBlock()
		{
			BlockSleeper blockSleeper = Block.list[this.blockType] as BlockSleeper;
			if (blockSleeper == null)
			{
				blockSleeper = (BlockSleeper)Block.GetBlockByName("sleeperSit", false);
			}
			return blockSleeper;
		}

		public static SleeperVolume.SpawnPoint Read(BinaryReader _br, int _version)
		{
			Vector3i vector3i = new Vector3i(_br.ReadInt32(), _br.ReadInt32(), _br.ReadInt32());
			if (_version >= 7 && _version < 20)
			{
				_br.ReadSingle();
				_br.ReadSingle();
				_br.ReadSingle();
			}
			float num = _br.ReadSingle();
			if (_version < 20)
			{
				_br.ReadByte();
			}
			int num2 = 0;
			if (_version > 14)
			{
				string text = _br.ReadString();
				Block blockByName = Block.GetBlockByName(text, false);
				if (blockByName != null)
				{
					num2 = blockByName.blockID;
				}
				else
				{
					Log.Warning("SpawnPoint Read missing block {0}", new object[]
					{
						text
					});
				}
			}
			else if (_version >= 9)
			{
				num2 = (int)_br.ReadUInt16();
			}
			return new SleeperVolume.SpawnPoint(vector3i, num, num2);
		}

		public void Write(BinaryWriter _bw)
		{
			_bw.Write(this.pos.x);
			_bw.Write(this.pos.y);
			_bw.Write(this.pos.z);
			_bw.Write(this.rot);
			_bw.Write(this.GetBlock().GetBlockName());
		}

		public readonly Vector3i pos;

		public readonly float rot;

		public readonly int blockType;
	}
}

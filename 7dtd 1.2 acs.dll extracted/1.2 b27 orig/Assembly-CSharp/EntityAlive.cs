using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using Audio;
using GamePath;
using Platform;
using UAI;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public abstract class EntityAlive : Entity
{
	public bool IsEquipping
	{
		get
		{
			return this.equippingCount > 0;
		}
		set
		{
			if (value)
			{
				this.equippingCount++;
				return;
			}
			if (this.equippingCount > 0)
			{
				this.equippingCount--;
			}
		}
	}

	public bool IsDancing
	{
		get
		{
			return this.isDancing;
		}
		set
		{
			this.isDancing = value;
			if (value)
			{
				if (this.emodel != null && this.emodel.avatarController != null)
				{
					this.emodel.avatarController.UpdateInt("IsDancing", base.EntityClass.DanceTypeID, true);
					return;
				}
			}
			else if (this.emodel != null && this.emodel.avatarController != null)
			{
				this.emodel.avatarController.UpdateInt("IsDancing", 0, true);
			}
		}
	}

	public void BeginDynamicRagdoll(DynamicRagdollFlags flags, FloatRange stunTime)
	{
		this._dynamicRagdoll = flags;
		this._dynamicRagdollRootMotion = Vector3.zero;
		this._dynamicRagdollStunTime = stunTime.Random(this.rand);
	}

	public void ActivateDynamicRagdoll()
	{
		if (this._dynamicRagdoll.HasFlag(DynamicRagdollFlags.Active))
		{
			DynamicRagdollFlags dynamicRagdoll = this._dynamicRagdoll;
			this._dynamicRagdoll = DynamicRagdollFlags.None;
			Vector3 forceVec = this._dynamicRagdollRootMotion * 20f;
			this.bodyDamage.StunDuration = this._dynamicRagdollStunTime;
			this.emodel.DoRagdoll(this._dynamicRagdollStunTime, EnumBodyPartHit.None, forceVec, Vector3.zero, true);
			if (dynamicRagdoll.HasFlag(DynamicRagdollFlags.UseBoneVelocities) && this._ragdollPositionsPrev.Count == this._ragdollPositionsCur.Count)
			{
				List<Vector3> list = new List<Vector3>();
				for (int i = 0; i < this._ragdollPositionsPrev.Count; i++)
				{
					Vector3 a = this._ragdollPositionsCur[i] - this._ragdollPositionsPrev[i];
					list.Add(a * 20f);
				}
				this.emodel.ApplyRagdollVelocities(list);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		base.Awake();
		this.entityName = base.GetType().Name;
		this.MinEventContext.Self = this;
		this.seeCache = new EntitySeeCache(this);
		this.maximumHomeDistance = -1;
		this.homePosition = new ChunkCoordinates(0, 0, 0);
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && !(this is EntityPlayer))
		{
			this.hasAI = true;
			this.navigator = AstarManager.CreateNavigator(this);
			this.aiManager = new EAIManager(this);
			this.lookHelper = new EntityLookHelper(this);
			this.moveHelper = new EntityMoveHelper(this);
		}
		this.equipment = new Equipment(this);
		this.InitInventory();
		if (this.bag == null)
		{
			this.bag = new Bag(this);
		}
		this.stepHeight = 0.52f;
		this.soundDelayTicks = this.GetSoundRandomTicks() / 3 - 5;
		this.spawnPoints = new EntityBedrollPositionList(this);
		this.CreationTimeSinceLevelLoad = Time.timeSinceLevelLoad;
		this.Buffs = new EntityBuffs(this);
		this.droppedBackpackPositions = new List<Vector3i>();
	}

	public override void Init(int _entityClass)
	{
		base.Init(_entityClass);
		this.constructEntityStats();
		this.switchModelView(EnumEntityModelView.ThirdPerson);
		this.InitPostCommon();
	}

	public override void InitFromPrefab(int _entityClass)
	{
		base.InitFromPrefab(_entityClass);
		this.switchModelView(EnumEntityModelView.ThirdPerson);
		this.InitPostCommon();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitPostCommon()
	{
		if (GameManager.IsDedicatedServer)
		{
			Transform modelTransform = this.emodel.GetModelTransform();
			if (modelTransform)
			{
				ServerHelper.SetupForServer(modelTransform.gameObject);
			}
		}
		this.AddCharacterController();
		this.wasSeenByPlayer = false;
		this.ticksToCheckSeenByPlayer = 20;
		if (EntityClass.list[this.entityClass].UseAIPackages)
		{
			this.hasAI = true;
			this.AIPackages = new List<string>();
			this.AIPackages.AddRange(EntityClass.list[this.entityClass].AIPackages);
			this.utilityAIContext = new Context(this);
		}
		List<string> buffs = EntityClass.list[this.entityClass].Buffs;
		if (buffs != null)
		{
			for (int i = 0; i < buffs.Count; i++)
			{
				string name = buffs[i];
				if (!this.Buffs.HasBuff(name))
				{
					this.Buffs.AddBuff(name, -1, true, false, -1f);
				}
			}
		}
		if ((this.entityFlags & (EntityFlags.Zombie | EntityFlags.Animal | EntityFlags.Bandit)) > EntityFlags.None)
		{
			this.emodel.SetVisible(false, false);
			this.emodel.SetFade(0f);
		}
	}

	public override void PostInit()
	{
		base.PostInit();
		this.ApplySpawnState();
		LODGroup componentInChildren = this.emodel.GetModelTransform().GetComponentInChildren<LODGroup>();
		if (componentInChildren)
		{
			LOD[] lods = componentInChildren.GetLODs();
			lods[lods.Length - 1].screenRelativeTransitionHeight = 0.003f;
			componentInChildren.SetLODs(lods);
		}
		this.disableFallBehaviorUntilOnGround = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ApplySpawnState()
	{
		if (this.Health <= 0 && this.isEntityRemote)
		{
			this.ClientKill(DamageResponse.New(true));
		}
		this.ExecuteDismember(true);
	}

	public virtual void InitInventory()
	{
		if (this.inventory == null)
		{
			this.inventory = new Inventory(GameManager.Instance, this);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void switchModelView(EnumEntityModelView modelView)
	{
		this.emodel.SwitchModelAndView(modelView == EnumEntityModelView.FirstPerson, this.IsMale);
		this.ReassignEquipmentTransforms();
	}

	public virtual void ReassignEquipmentTransforms()
	{
		this.equipment.InitializeEquipmentTransforms();
	}

	public override void CopyPropertiesFromEntityClass()
	{
		base.CopyPropertiesFromEntityClass();
		EntityClass entityClass = EntityClass.list[this.entityClass];
		string @string = entityClass.Properties.GetString(EntityClass.PropHandItem);
		if (@string.Length > 0)
		{
			this.handItem = ItemClass.GetItem(@string, false);
			if (this.handItem.IsEmpty())
			{
				throw new Exception("Item with name '" + @string + "' not found!");
			}
		}
		else
		{
			this.handItem = ItemClass.GetItem("meleeHandPlayer", false).Clone();
		}
		if (this.inventory != null)
		{
			this.inventory.SetBareHandItem(this.handItem);
		}
		this.rightHandTransformName = "Gunjoint";
		if (this.emodel is EModelSDCS)
		{
			this.rightHandTransformName = "RightWeapon";
		}
		entityClass.Properties.ParseString(EntityClass.PropRightHandJointName, ref this.rightHandTransformName);
		if (!(this is EntityPlayer))
		{
			this.factionId = 0;
			this.factionRank = 0;
			if (EntityClass.list[this.entityClass].Properties.Contains("Faction"))
			{
				Faction factionByName = FactionManager.Instance.GetFactionByName(EntityClass.list[this.entityClass].Properties.Values["Faction"]);
				if (factionByName != null)
				{
					this.factionId = factionByName.ID;
					if (EntityClass.list[this.entityClass].Properties.Contains("FactionRank"))
					{
						this.factionRank = StringParsers.ParseUInt8(EntityClass.list[this.entityClass].Properties.Values["FactionRank"], 0, -1, NumberStyles.Integer);
					}
				}
			}
		}
		else if (FactionManager.Instance.GetFaction(this.factionId).ID == 0)
		{
			this.factionId = FactionManager.Instance.CreateFaction(this.entityName, true, "").ID;
			this.factionRank = byte.MaxValue;
		}
		this.maxViewAngle = 180f;
		if (entityClass.Properties.Values.ContainsKey(EntityClass.PropMaxViewAngle))
		{
			this.maxViewAngle = StringParsers.ParseFloat(entityClass.Properties.Values[EntityClass.PropMaxViewAngle], 0, -1, NumberStyles.Any);
		}
		this.sightRangeBase = entityClass.SightRange;
		this.sightLightThreshold = entityClass.sightLightThreshold;
		this.SetSleeperSight(-1f, -1f);
		this.sightWakeThresholdAtRange = new Vector2(this.rand.RandomRange(entityClass.WakeMin.x, entityClass.WakeMin.y), this.rand.RandomRange(entityClass.WakeMax.y, entityClass.WakeMax.y));
		this.sightGroanThresholdAtRange = new Vector2(this.rand.RandomRange(entityClass.GroanMin.x, entityClass.GroanMin.y), this.rand.RandomRange(entityClass.GroanMax.y, entityClass.GroanMax.y));
		this.noiseWake = this.rand.RandomRange(entityClass.NoiseWake.x, entityClass.NoiseWake.y);
		this.noiseGroan = this.rand.RandomRange(entityClass.NoiseGroan.x, entityClass.NoiseGroan.y);
		this.smellWake = this.rand.RandomRange(entityClass.SmellWake.x, entityClass.SmellWake.y);
		this.smellGroan = this.rand.RandomRange(entityClass.SmellGroan.x, entityClass.SmellGroan.y);
		this.groanChance = entityClass.GroanChance;
		if (entityClass.Properties.Values.ContainsKey(EntityClass.PropWeight))
		{
			this.weight = Mathf.Max(StringParsers.ParseFloat(entityClass.Properties.Values[EntityClass.PropWeight], 0, -1, NumberStyles.Any), 0.5f);
		}
		else
		{
			this.weight = 1f;
		}
		if (entityClass.Properties.Values.ContainsKey(EntityClass.PropPushFactor))
		{
			this.pushFactor = StringParsers.ParseFloat(entityClass.Properties.Values[EntityClass.PropPushFactor], 0, -1, NumberStyles.Any);
		}
		else
		{
			this.pushFactor = 1f;
		}
		if (entityClass.Properties.Values.ContainsKey(EntityClass.PropTimeStayAfterDeath))
		{
			this.timeStayAfterDeath = (int)(StringParsers.ParseFloat(entityClass.Properties.Values[EntityClass.PropTimeStayAfterDeath], 0, -1, NumberStyles.Any) * 20f);
		}
		else
		{
			this.timeStayAfterDeath = 100;
		}
		if (entityClass.Properties.Values.ContainsKey(EntityClass.PropIsMale))
		{
			this.IsMale = StringParsers.ParseBool(entityClass.Properties.Values[EntityClass.PropIsMale], 0, -1, true);
		}
		else
		{
			this.IsMale = true;
		}
		if (this.aiManager != null)
		{
			this.aiManager.CopyPropertiesFromEntityClass(entityClass);
		}
		this.IsFeral = entityClass.Tags.Test_Bit(EntityAlive.FeralTagBit);
		this.moveSpeed = 1f;
		entityClass.Properties.ParseFloat(EntityClass.PropMoveSpeed, ref this.moveSpeed);
		this.moveSpeedNight = this.moveSpeed;
		entityClass.Properties.ParseFloat(EntityClass.PropMoveSpeedNight, ref this.moveSpeedNight);
		this.moveSpeedAggro = this.moveSpeed;
		this.moveSpeedAggroMax = this.moveSpeed;
		entityClass.Properties.ParseVec(EntityClass.PropMoveSpeedAggro, ref this.moveSpeedAggro, ref this.moveSpeedAggroMax);
		this.moveSpeedPanic = 1f;
		this.moveSpeedPanicMax = 1f;
		entityClass.Properties.ParseFloat(EntityClass.PropMoveSpeedPanic, ref this.moveSpeedPanic);
		if (this.moveSpeedPanic != 1f)
		{
			this.moveSpeedPanicMax = this.moveSpeedPanic;
		}
		entityClass.Properties.ParseFloat(EntityClass.PropSwimSpeed, ref this.swimSpeed);
		entityClass.Properties.ParseVec(EntityClass.PropSwimStrokeRate, ref this.swimStrokeRate);
		Vector2 negativeInfinity = Vector2.negativeInfinity;
		entityClass.Properties.ParseVec(EntityClass.PropMoveSpeedRand, ref negativeInfinity);
		if (negativeInfinity.x > -1f)
		{
			float num = this.rand.RandomRange(negativeInfinity.x, negativeInfinity.y);
			int @int = GameStats.GetInt(EnumGameStats.GameDifficulty);
			num *= EntityAlive.moveSpeedRandomness[@int];
			if (this.moveSpeedAggro < 1f)
			{
				this.moveSpeedAggro += num;
				if (this.moveSpeedAggro < 0.1f)
				{
					this.moveSpeedAggro = 0.1f;
				}
				if (this.moveSpeedAggro > this.moveSpeedAggroMax)
				{
					this.moveSpeedAggro = this.moveSpeedAggroMax;
				}
			}
		}
		this.walkType = EntityAlive.GetSpawnWalkType(entityClass);
		if (entityClass.Properties.Values.ContainsKey(EntityClass.PropCanClimbVertical))
		{
			this.bCanClimbVertical = StringParsers.ParseBool(entityClass.Properties.Values[EntityClass.PropCanClimbVertical], 0, -1, true);
		}
		if (entityClass.Properties.Values.ContainsKey(EntityClass.PropCanClimbLadders))
		{
			this.bCanClimbLadders = StringParsers.ParseBool(entityClass.Properties.Values[EntityClass.PropCanClimbLadders], 0, -1, true);
		}
		Vector2 vector = new Vector2(1.9f, 2.1f);
		entityClass.Properties.ParseVec(EntityClass.PropJumpMaxDistance, ref vector);
		this.jumpMaxDistance = this.rand.RandomRange(vector.x, vector.y);
		this.jumpDelay = 1f;
		entityClass.Properties.ParseFloat(EntityClass.PropJumpDelay, ref this.jumpDelay);
		this.jumpDelay *= 20f;
		entityClass.Properties.ParseString(EntityClass.PropSoundSpawn, ref this.soundSpawn);
		entityClass.Properties.ParseString(EntityClass.PropSoundSleeperGroan, ref this.soundSleeperGroan);
		entityClass.Properties.ParseString(EntityClass.PropSoundSleeperSnore, ref this.soundSleeperSnore);
		entityClass.Properties.ParseString(EntityClass.PropSoundDeath, ref this.soundDeath);
		entityClass.Properties.ParseString(EntityClass.PropSoundAlert, ref this.soundAlert);
		entityClass.Properties.ParseString(EntityClass.PropSoundAttack, ref this.soundAttack);
		entityClass.Properties.ParseString(EntityClass.PropSoundLiving, ref this.soundLiving);
		entityClass.Properties.ParseString(EntityClass.PropSoundRandom, ref this.soundRandom);
		entityClass.Properties.ParseString(EntityClass.PropSoundSense, ref this.soundSense);
		entityClass.Properties.ParseString(EntityClass.PropSoundGiveUp, ref this.soundGiveUp);
		entityClass.Properties.ParseString(EntityClass.PropSoundFootstepModifier, ref this.soundFootstepModifier);
		entityClass.Properties.ParseString(EntityClass.PropSoundStamina, ref this.soundStamina);
		entityClass.Properties.ParseString(EntityClass.PropSoundJump, ref this.soundJump);
		entityClass.Properties.ParseString(EntityClass.PropSoundLand, ref this.soundLand);
		entityClass.Properties.ParseString(EntityClass.PropSoundHurt, ref this.soundHurt);
		entityClass.Properties.ParseString(EntityClass.PropSoundHurtSmall, ref this.soundHurtSmall);
		entityClass.Properties.ParseString(EntityClass.PropSoundDrownPain, ref this.soundDrownPain);
		entityClass.Properties.ParseString(EntityClass.PropSoundDrownDeath, ref this.soundDrownDeath);
		entityClass.Properties.ParseString(EntityClass.PropSoundWaterSurface, ref this.soundWaterSurface);
		this.soundAlertTicks = 25;
		entityClass.Properties.ParseInt(EntityClass.PropSoundAlertTime, ref this.soundAlertTicks);
		this.soundAlertTicks *= 20;
		this.soundRandomTicks = 25;
		entityClass.Properties.ParseInt(EntityClass.PropSoundRandomTime, ref this.soundRandomTicks);
		this.soundRandomTicks *= 20;
		if (entityClass.Properties.Values.ContainsKey(EntityClass.PropAttackTimeoutDay))
		{
			this.attackTimeoutDay = (int)(StringParsers.ParseFloat(entityClass.Properties.Values[EntityClass.PropAttackTimeoutDay], 0, -1, NumberStyles.Any) * 20f);
		}
		if (entityClass.Properties.Values.ContainsKey(EntityClass.PropAttackTimeoutNight))
		{
			this.attackTimeoutNight = (int)(StringParsers.ParseFloat(entityClass.Properties.Values[EntityClass.PropAttackTimeoutNight], 0, -1, NumberStyles.Any) * 20f);
		}
		if (entityClass.Properties.Values.ContainsKey(EntityClass.PropParticleOnDeath))
		{
			this.particleOnDeath = entityClass.Properties.Values[EntityClass.PropParticleOnDeath];
		}
		if (entityClass.Properties.Values.ContainsKey(EntityClass.PropParticleOnDestroy))
		{
			this.particleOnDestroy = entityClass.Properties.Values[EntityClass.PropParticleOnDestroy];
		}
		if (entityClass.Properties.Values.ContainsKey(EntityClass.PropCorpseBlock))
		{
			this.corpseBlockValue = Block.GetBlockValue(entityClass.Properties.Values[EntityClass.PropCorpseBlock], false);
		}
		this.corpseBlockChance = 1f;
		if (entityClass.Properties.Values.ContainsKey(EntityClass.PropCorpseBlockChance))
		{
			this.corpseBlockChance = StringParsers.ParseFloat(entityClass.Properties.Values[EntityClass.PropCorpseBlockChance], 0, -1, NumberStyles.Any);
		}
		this.ExperienceValue = 20;
		if (entityClass.Properties.Values.ContainsKey(EntityClass.PropExperienceGain))
		{
			this.ExperienceValue = (int)StringParsers.ParseFloat(entityClass.Properties.Values[EntityClass.PropExperienceGain], 0, -1, NumberStyles.Any);
		}
		if (entityClass.Properties.Values.ContainsKey(EntityClass.PropStompsSpikes))
		{
			this.stompsSpikes = StringParsers.ParseBool(entityClass.Properties.Values[EntityClass.PropStompsSpikes], 0, -1, true);
		}
		this.proneRefillRate = this.rand.RandomRange(entityClass.KnockdownProneRefillRate.x, entityClass.KnockdownProneRefillRate.y);
		this.kneelRefillRate = this.rand.RandomRange(entityClass.KnockdownKneelRefillRate.x, entityClass.KnockdownKneelRefillRate.y);
		GameMode gameModeForId = GameMode.GetGameModeForId(GameStats.GetInt(EnumGameStats.GameModeId));
		if (gameModeForId != null)
		{
			string string2 = entityClass.Properties.GetString(EntityClass.PropItemsOnEnterGame + "." + gameModeForId.GetTypeName());
			if (string2.Length > 0)
			{
				foreach (string text in string2.Split(',', StringSplitOptions.None))
				{
					ItemStack itemStack = ItemStack.FromString(text.Trim());
					if (itemStack.itemValue.IsEmpty())
					{
						throw new Exception("Item with name '" + text + "' not found in class " + EntityClass.list[this.entityClass].entityClassName);
					}
					if (itemStack.itemValue.ItemClass.CreativeMode != EnumCreativeMode.Console || (DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent())
					{
						this.itemsOnEnterGame.Add(itemStack);
					}
				}
			}
		}
		this.distractionResistance = EffectManager.GetValue(PassiveEffects.DistractionResistance, null, 0f, this, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
		this.distractionResistanceWithTarget = EffectManager.GetValue(PassiveEffects.DistractionResistance, null, 0f, this, null, EntityAlive.DistractionResistanceWithTargetTags, true, true, true, true, true, 1, true, false);
		DynamicProperties dynamicProperties = entityClass.Properties.Classes[EntityClass.PropFallLandBehavior];
		if (dynamicProperties != null)
		{
			foreach (KeyValuePair<string, string> keyValuePair in dynamicProperties.Data.Dict)
			{
				string key = keyValuePair.Key;
				DictionarySave<string, string> dictionarySave = dynamicProperties.ParseKeyData(key);
				if (dictionarySave != null)
				{
					FloatRange height = default(FloatRange);
					FloatRange ragePer = default(FloatRange);
					FloatRange rageTime = default(FloatRange);
					IntRange difficulty = new IntRange(0, 10);
					string text2;
					EntityAlive.FallBehavior.Op type;
					if (!dictionarySave.TryGetValue("anim", out text2) || !Enum.TryParse<EntityAlive.FallBehavior.Op>(text2, out type))
					{
						Log.Error("Expected 'anim' parameter as float for FallBehavior " + key + ", skipping");
					}
					else
					{
						float num2 = 0f;
						if (!dictionarySave.TryGetValue("weight", out text2) || !StringParsers.TryParseFloat(text2, out num2, 0, -1, NumberStyles.Any))
						{
							Log.Error("Expected 'weight' parameter as float for FallBehavior " + key + ", skipping");
						}
						else if (dictionarySave.TryGetValue("height", out text2))
						{
							FloatRange floatRange;
							if (StringParsers.TryParseRange(text2, out floatRange, new float?(3.40282347E+38f)))
							{
								height = floatRange;
								if (dictionarySave.TryGetValue("ragePer", out text2))
								{
									FloatRange floatRange2;
									if (!StringParsers.TryParseRange(text2, out floatRange2, null))
									{
										Log.Error("Expected 'ragePer' parameter as range(min,min-max) " + key + ", skipping");
										continue;
									}
									ragePer = floatRange2;
								}
								if (dictionarySave.TryGetValue("rageTime", out text2))
								{
									FloatRange floatRange3;
									if (!StringParsers.TryParseRange(text2, out floatRange3, null))
									{
										Log.Error("Expected 'rageTime' parameter as range(min,min-max) " + key + ", skipping");
										continue;
									}
									rageTime = floatRange3;
								}
								if (dictionarySave.TryGetValue("difficulty", out text2))
								{
									IntRange intRange;
									if (!StringParsers.TryParseRange(text2, out intRange, null))
									{
										Log.Error("Expected 'difficulty' parameter as range(min,min-max) " + key + ", skipping");
										continue;
									}
									difficulty = intRange;
								}
								this.fallBehaviors.Add(new EntityAlive.FallBehavior(key, type, height, num2, ragePer, rageTime, difficulty));
							}
							else
							{
								Log.Error("Expected 'height' parameter as range(min,min-max) " + key + ", skipping");
							}
						}
						else
						{
							Log.Error("Expected 'height' parameter for FallBehavior " + key + ", skipping");
						}
					}
				}
			}
		}
		DynamicProperties dynamicProperties2 = entityClass.Properties.Classes[EntityClass.PropDestroyBlockBehavior];
		if (dynamicProperties2 != null)
		{
			EntityAlive.DestroyBlockBehavior.Op[] array2 = Enum.GetValues(typeof(EntityAlive.DestroyBlockBehavior.Op)) as EntityAlive.DestroyBlockBehavior.Op[];
			for (int j = 0; j < array2.Length; j++)
			{
				string text3 = array2[j].ToStringCached<EntityAlive.DestroyBlockBehavior.Op>();
				DictionarySave<string, string> dictionarySave2 = dynamicProperties2.ParseKeyData(array2[j].ToStringCached<EntityAlive.DestroyBlockBehavior.Op>());
				if (dictionarySave2 != null)
				{
					FloatRange ragePer2 = default(FloatRange);
					FloatRange rageTime2 = default(FloatRange);
					IntRange difficulty2 = new IntRange(0, 10);
					string input;
					float num3;
					if (!dictionarySave2.TryGetValue("weight", out input) || !StringParsers.TryParseFloat(input, out num3, 0, -1, NumberStyles.Any))
					{
						Log.Error(string.Format("Expected 'weight' parameter as float for FallBehavior {0}, skipping", array2[j]));
					}
					else
					{
						if (dictionarySave2.TryGetValue("ragePer", out input))
						{
							FloatRange floatRange4;
							if (!StringParsers.TryParseRange(input, out floatRange4, null))
							{
								Log.Error(string.Format("Expected 'ragePer' parameter as range(min,min-max) {0}, skipping", array2[j]));
								goto IL_10DA;
							}
							ragePer2 = floatRange4;
						}
						if (dictionarySave2.TryGetValue("rageTime", out input))
						{
							FloatRange floatRange5;
							if (!StringParsers.TryParseRange(input, out floatRange5, null))
							{
								Log.Error(string.Format("Expected 'rageTime' parameter as range(min,min-max) {0}, skipping", array2[j]));
								goto IL_10DA;
							}
							rageTime2 = floatRange5;
						}
						if (dictionarySave2.TryGetValue("difficulty", out input))
						{
							IntRange intRange2;
							if (!StringParsers.TryParseRange(input, out intRange2, null))
							{
								Log.Error("Expected 'difficulty' parameter as range(min,min-max) " + text3 + ", skipping");
								goto IL_10DA;
							}
							difficulty2 = intRange2;
						}
						this._destroyBlockBehaviors.Add(new EntityAlive.DestroyBlockBehavior(text3, array2[j], num3, ragePer2, rageTime2, difficulty2));
					}
				}
				IL_10DA:;
			}
		}
	}

	public static int GetSpawnWalkType(EntityClass _entityClass)
	{
		int result = 0;
		_entityClass.Properties.ParseInt(EntityClass.PropWalkType, ref result);
		return result;
	}

	public override void VisiblityCheck(float _distanceSqr, bool _isZoom)
	{
		if ((this.entityFlags & (EntityFlags.Zombie | EntityFlags.Animal | EntityFlags.Bandit)) > EntityFlags.None)
		{
			if (GameManager.IsDedicatedServer)
			{
				this.emodel.SetVisible(true, false);
				return;
			}
			if (_distanceSqr < (float)(_isZoom ? 14400 : 8100))
			{
				this.renderFadeTarget = 1f;
				return;
			}
			this.renderFadeTarget = 0f;
		}
	}

	public virtual void SetSleeper()
	{
		this.IsSleeper = true;
		this.aiManager.pathCostScale += 0.2f;
	}

	public void SetSleeperSight(float angle, float range)
	{
		if (angle < 0f)
		{
			angle = this.maxViewAngle;
		}
		this.sleeperViewAngle = angle;
		if (range < 0f)
		{
			range = Mathf.Max(3f, this.sightRangeBase * 0.2f);
		}
		this.sleeperSightRange = range;
	}

	public void SetSleeperHearing(float percent)
	{
		if (percent < 0.001f)
		{
			percent = 0.001f;
		}
		percent = 1f / percent;
		this.noiseGroan *= percent;
		this.noiseWake *= percent;
	}

	public int GetSleeperDisturbedLevel(float dist, float lightLevel)
	{
		float num = dist / this.sightRangeBase;
		if (num <= 1f)
		{
			float num2 = Mathf.Lerp(this.sightWakeThresholdAtRange.x, this.sightWakeThresholdAtRange.y, num);
			if (lightLevel > num2)
			{
				return 2;
			}
			float num3 = Mathf.Lerp(this.sightGroanThresholdAtRange.x, this.sightGroanThresholdAtRange.y, num);
			if (lightLevel > num3)
			{
				return 1;
			}
		}
		return 0;
	}

	public void GetSleeperDebugScale(float dist, out float wake, out float groan)
	{
		float t = dist / this.sightRangeBase;
		wake = Mathf.Lerp(this.sightWakeThresholdAtRange.x, this.sightWakeThresholdAtRange.y, t);
		groan = Mathf.Lerp(this.sightGroanThresholdAtRange.x, this.sightGroanThresholdAtRange.y, t);
	}

	public bool sleepingOrWakingUp
	{
		get
		{
			return this.IsSleeping;
		}
	}

	public void TriggerSleeperPose(int _pose, bool _returningToSleep = false)
	{
		if (this.IsDead())
		{
			return;
		}
		if (this.emodel != null && this.emodel.avatarController != null)
		{
			this.emodel.avatarController.TriggerSleeperPose(_pose, _returningToSleep);
			this.pendingSleepTrigger = -1;
		}
		else
		{
			this.pendingSleepTrigger = _pose;
		}
		this.lastSleeperPose = _pose;
		this.IsSleeping = true;
		this.SleeperSupressLivingSounds = true;
		this.sleeperLookDir = Quaternion.AngleAxis(this.rotation.y, Vector3.up) * this.SleeperSpawnLookDir;
	}

	public void ResumeSleeperPose()
	{
		this.TriggerSleeperPose(this.lastSleeperPose, true);
	}

	public void ConditionalTriggerSleeperWakeUp()
	{
		if (this.IsSleeping && !this.IsDead())
		{
			this.IsSleeping = false;
			this.IsSleeperPassive = false;
			this.emodel.avatarController.TriggerSleeperPose(-1, false);
			if (this.aiManager != null)
			{
				this.aiManager.SleeperWokeUp();
			}
			if (!this.world.IsRemote())
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageSleeperWakeup>().Setup(this.entityId), false, -1, -1, -1, null, 192);
			}
		}
	}

	public void SetSleeperActive()
	{
		if (this.IsSleeperPassive)
		{
			this.IsSleeperPassive = false;
			if (!this.world.IsRemote())
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageSleeperPassiveChange>().Setup(this.entityId), false, -1, -1, -1, null, 192);
			}
		}
	}

	public EntityStats Stats
	{
		get
		{
			return this.entityStats;
		}
		set
		{
			this.entityStats = value;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void constructEntityStats()
	{
		this.entityStats = new EntityStats(this);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual ItemValue GetHandItem()
	{
		return this.handItem;
	}

	public bool IsHoldingLight()
	{
		return this.inventory.IsFlashlightOn;
	}

	public void CycleActivatableItems()
	{
	}

	public List<ItemValue> GetActivatableItemPool()
	{
		List<ItemValue> list = new List<ItemValue>();
		this.CollectActivatableItems(list);
		return list;
	}

	public void CollectActivatableItems(List<ItemValue> _pool)
	{
		if (this.inventory != null)
		{
			EntityAlive.GetActivatableItems(this.inventory.holdingItemItemValue, _pool);
		}
		if (this.equipment != null)
		{
			int slotCount = this.equipment.GetSlotCount();
			for (int i = 0; i < slotCount; i++)
			{
				EntityAlive.GetActivatableItems(this.equipment.GetSlotItemOrNone(i), _pool);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void GetActivatableItems(ItemValue _item, List<ItemValue> _itemPool)
	{
		ItemClass itemClass = _item.ItemClass;
		if (itemClass == null)
		{
			return;
		}
		if (itemClass.HasTrigger(MinEventTypes.onSelfItemActivate))
		{
			_itemPool.Add(_item);
		}
		for (int i = 0; i < _item.Modifications.Length; i++)
		{
			ItemValue itemValue = _item.Modifications[i];
			if (itemValue != null)
			{
				ItemClass itemClass2 = itemValue.ItemClass;
				if (itemClass2 != null && itemClass2.HasTrigger(MinEventTypes.onSelfItemActivate))
				{
					_itemPool.Add(itemValue);
				}
			}
		}
	}

	public override void OnUpdatePosition(float _partialTicks)
	{
		base.OnUpdatePosition(_partialTicks);
		this.prevRotation = this.rotation;
		Vector3 vector = Vector3.zero;
		for (int i = 0; i < this.lastTickPos.Length - 1; i++)
		{
			vector.x += this.lastTickPos[i].x - this.lastTickPos[i + 1].x;
			vector.z += this.lastTickPos[i].z - this.lastTickPos[i + 1].z;
		}
		vector += this.position - this.lastTickPos[0];
		vector /= (float)this.lastTickPos.Length;
		if (this.AttachedToEntity == null)
		{
			this.updateStepSound(vector.x, vector.z);
		}
		if (!this.RootMotion && !this.isEntityRemote)
		{
			this.updateSpeedForwardAndStrafe(vector, _partialTicks);
		}
	}

	public void Snore()
	{
		if (!this.isSnore && this.isGroan && this.snoreGroanCD <= 0)
		{
			this.isSnore = true;
			this.isGroan = false;
			this.snoreGroanCD = this.rand.RandomRange(20, 21);
			if (this.soundSleeperSnore != null && !this.isGroanSilent)
			{
				Manager.BroadcastPlay(this, this.soundSleeperSnore, false);
			}
		}
	}

	public void Groan()
	{
		if (!this.isGroan && this.snoreGroanCD <= 0)
		{
			this.isGroan = true;
			this.isSnore = false;
			this.snoreGroanCD = this.rand.RandomRange(20, 21);
			if (this.groanChance >= 1f || this.rand.RandomFloat <= this.groanChance)
			{
				this.isGroanSilent = false;
				if (this.soundSleeperGroan != null)
				{
					Manager.BroadcastPlay(this, this.soundSleeperGroan, false);
					return;
				}
			}
			else
			{
				this.isGroanSilent = true;
			}
		}
	}

	public override void OnUpdateEntity()
	{
		base.OnUpdateEntity();
		this.Buffs.SetCustomVar("_underwater", this.inWaterPercent, true);
		if (this.Buffs != null)
		{
			this.Buffs.Update(Time.deltaTime);
		}
		this.OnUpdateLive();
		if (!this.IsSleeping)
		{
			this.bag.OnUpdate();
			if (this.inventory != null)
			{
				this.inventory.OnUpdate();
			}
		}
		if (this.Health <= 0 && !this.IsDead() && !this.isEntityRemote && !this.IsGodMode.Value)
		{
			if (this.Buffs.HasBuff("drowning"))
			{
				this.DamageEntity(DamageSource.suffocating, 1, false, 1f);
			}
			else
			{
				this.DamageEntity(DamageSource.disease, 1, false, 1f);
			}
		}
		if (base.IsAlive() && this.bPlayHurtSound)
		{
			string text = this.GetSoundHurt(this.woundedDamageSource, this.woundedStrength);
			if (text != null)
			{
				this.PlayOneShot(text, false, false, false);
			}
		}
		this.bPlayHurtSound = false;
		this.bBeenWounded = false;
		this.woundedStrength = 0;
		this.woundedDamageSource = null;
		if (this.snoreGroanCD > 0)
		{
			this.snoreGroanCD--;
		}
		if (!this.IsDead() && !this.isEntityRemote)
		{
			if (this.isRadiationSensitive() && this.biomeStandingOn != null && this.biomeStandingOn.m_RadiationLevel > 0 && !this.IsGodMode.Value && this.world.worldTime % 20UL == 0UL)
			{
				this.DamageEntity(DamageSource.radiation, this.biomeStandingOn.m_RadiationLevel, false, 1f);
			}
			if (this.hasAI)
			{
				if (this.IsSleeping && this.pendingSleepTrigger > -1)
				{
					this.TriggerSleeperPose(this.pendingSleepTrigger, false);
				}
				this.soundDelayTicks--;
				if (this.attackingTime <= 0 && this.soundDelayTicks <= 0 && this.aiClosestPlayerDistSq <= 400f && this.bodyDamage.CurrentStun == EnumEntityStunType.None && !this.SleeperSupressLivingSounds)
				{
					if (this.targetAlertChanged)
					{
						this.targetAlertChanged = false;
						this.soundDelayTicks = this.GetSoundAlertTicks();
						if (this.GetSoundAlert() != null && !this.IsScoutZombie)
						{
							this.PlayOneShot(this.GetSoundAlert(), false, false, false);
						}
						this.OnEntityTargeted(this.attackTarget);
					}
					else
					{
						this.soundDelayTicks = this.GetSoundRandomTicks();
						this.attackTargetLast = null;
						if (this.GetSoundRandom() != null)
						{
							this.PlayOneShot(this.GetSoundRandom(), false, false, false);
						}
					}
				}
			}
		}
		if (this.hasBeenAttackedTime > 0)
		{
			this.hasBeenAttackedTime--;
		}
		if (this.painResistPercent > 0f)
		{
			this.painResistPercent -= 0.01f;
		}
		if (this.attackingTime > 0)
		{
			this.attackingTime--;
			if (this.attackingTime == 0 && this.attackTarget != null)
			{
				this.LastTargetPos = this.attackTarget.GetPosition();
			}
		}
		if (this.investigatePositionTicks > 0)
		{
			int num = this.investigatePositionTicks - 1;
			this.investigatePositionTicks = num;
			if (num == 0)
			{
				this.ClearInvestigatePosition();
			}
		}
		bool flag = this.IsDead();
		if (this.alertEnabled)
		{
			this.isAlert = this.bReplicatedAlertFlag;
			if (!this.isEntityRemote)
			{
				if (this.alertTicks > 0)
				{
					this.alertTicks--;
				}
				this.isAlert = (!flag && (this.alertTicks > 0 || this.attackTarget || (this.HasInvestigatePosition && this.isInvestigateAlert)));
				if (this.bReplicatedAlertFlag != this.isAlert)
				{
					this.bReplicatedAlertFlag = this.isAlert;
					this.bEntityAliveFlagsChanged = true;
				}
			}
			if (!this.isAlert && !flag)
			{
				this.Buffs.SetCustomVar(EntityAlive.notAlertedId, 1f, true);
				this.notAlertDelayTicks = 4;
			}
			else
			{
				if (this.notAlertDelayTicks > 0)
				{
					this.notAlertDelayTicks--;
				}
				if (this.notAlertDelayTicks == 0)
				{
					this.Buffs.SetCustomVar(EntityAlive.notAlertedId, 0f, true);
				}
			}
		}
		if (flag)
		{
			this.OnDeathUpdate();
		}
		if (this.revengeEntity != null)
		{
			if (!this.revengeEntity.IsAlive())
			{
				this.SetRevengeTarget(null);
				return;
			}
			if (this.revengeTimer > 0)
			{
				this.revengeTimer--;
				return;
			}
			this.SetRevengeTarget(null);
		}
	}

	public override void KillLootContainer()
	{
		if (!this.isEntityRemote && this.IsDead() && !this.corpseBlockValue.isair && this.deathUpdateTime < this.timeStayAfterDeath)
		{
			this.deathUpdateTime = this.timeStayAfterDeath - 1;
		}
		base.KillLootContainer();
	}

	public override void Kill(DamageResponse _dmResponse)
	{
		this.NotifySleeperDeath();
		if (this.AttachedToEntity != null)
		{
			this.Detach();
		}
		if (this.deathUpdateTime == 0)
		{
			string text = this.GetSoundDeath(_dmResponse.Source);
			if (text != null)
			{
				this.PlayOneShot(text, false, false, false);
			}
		}
		if (this.IsDead())
		{
			this.SetDead();
			return;
		}
		this.ClientKill(_dmResponse);
		base.Kill(_dmResponse);
	}

	public override void SetDead()
	{
		base.SetDead();
		this.Stats.Health.Value = 0f;
	}

	public void NotifySleeperDeath()
	{
		if (!this.isEntityRemote && this.IsSleeper)
		{
			this.world.NotifySleeperVolumesEntityDied(this);
		}
	}

	public void ClearEntityThatKilledMe()
	{
		this.entityThatKilledMe = null;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void ClientKill(DamageResponse _dmResponse)
	{
		this.lastHitDirection = Utils.EnumHitDirection.Back;
		if (this.entityThatKilledMe == null && _dmResponse.Source != null)
		{
			Entity entity = (_dmResponse.Source.getEntityId() != -1) ? this.world.GetEntity(_dmResponse.Source.getEntityId()) : null;
			if (this.Spawned && entity is EntityAlive)
			{
				this.entityThatKilledMe = (EntityAlive)entity;
			}
		}
		if (!this.IsDead())
		{
			this.SetDead();
			if (this.Buffs != null)
			{
				this.Buffs.OnDeath(this.entityThatKilledMe, _dmResponse.Source != null && _dmResponse.Source.damageType == EnumDamageTypes.Crushing, (_dmResponse.Source == null) ? FastTags<TagGroup.Global>.Parse("crushing") : _dmResponse.Source.DamageTypeTag);
			}
			if (this.Progression != null)
			{
				this.Progression.OnDeath();
			}
			UnityEngine.Object x = this as EntityPlayer;
			this.AnalyticsSendDeath(_dmResponse);
			if (x == null && this.entityThatKilledMe is EntityPlayer && EffectManager.GetValue(PassiveEffects.CelebrationKill, null, 0f, this.entityThatKilledMe, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false) > 0f)
			{
				this.HandleClientDeath((_dmResponse.Source != null) ? _dmResponse.Source.BlockPosition : base.GetBlockPosition());
				this.OnEntityDeath();
				float lightBrightness = this.world.GetLightBrightness(base.GetBlockPosition());
				this.world.GetGameManager().SpawnParticleEffectServer(new ParticleEffect("confetti", this.position, lightBrightness, Color.white, null, null, false), this.entityId, false, true);
				Manager.BroadcastPlayByLocalPlayer(this.position, "twitch_celebrate");
				GameManager.Instance.World.RemoveEntity(this.entityId, EnumRemoveEntityReason.Killed);
				return;
			}
			this.HandleClientDeath((_dmResponse.Source != null) ? _dmResponse.Source.BlockPosition : base.GetBlockPosition());
			this.OnEntityDeath();
			this.emodel.OnDeath(_dmResponse, this.world.ChunkClusters[0]);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void HandleClientDeath(Vector3i attackPos)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnEntityTargeted(EntityAlive target)
	{
	}

	public virtual void SetHoldingItemTransform(Transform _transform)
	{
		this.emodel.SetInRightHand(_transform);
	}

	public virtual void OnHoldingItemChanged()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void updateCameraPosition(bool _bLerpPosition)
	{
	}

	public float GetWetnessPercentage()
	{
		float num = this.inWaterPercent;
		if (this.Stats.AmountEnclosed < WeatherParams.EnclosureDetectionThreshold)
		{
			num = Mathf.Max(num, WeatherManager.Instance.GetCurrentRainfallValue());
			num = Mathf.Max(num, WeatherManager.Instance.GetCurrentSnowfallValue());
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnHeadUnderwaterStateChanged(bool _bUnderwater)
	{
		base.OnHeadUnderwaterStateChanged(_bUnderwater);
		if (_bUnderwater)
		{
			this.FireEvent(MinEventTypes.onSelfWaterSubmerge, true);
			return;
		}
		this.FireEvent(MinEventTypes.onSelfWaterSurface, true);
	}

	public virtual bool JetpackActive
	{
		get
		{
			return this.bJetpackActive;
		}
		set
		{
			if (value != this.bJetpackActive)
			{
				this.bJetpackActive = value;
				this.bEntityAliveFlagsChanged |= !this.isEntityRemote;
			}
		}
	}

	public virtual bool JetpackWearing
	{
		get
		{
			return this.bJetpackWearing;
		}
		set
		{
			if (value != this.bJetpackWearing)
			{
				this.bJetpackWearing = value;
				this.bEntityAliveFlagsChanged |= !this.isEntityRemote;
			}
		}
	}

	public virtual bool ParachuteWearing
	{
		get
		{
			return this.bParachuteWearing;
		}
		set
		{
			if (value != this.bParachuteWearing)
			{
				this.bParachuteWearing = value;
				this.bEntityAliveFlagsChanged |= !this.isEntityRemote;
			}
		}
	}

	public virtual bool AimingGun
	{
		get
		{
			return this.emodel.avatarController != null && this.emodel.avatarController.TryGetBool(AvatarController.isAimingHash, out this.bAimingGun) && this.bAimingGun;
		}
		set
		{
			bool aimingGun = this.AimingGun;
			if (value != aimingGun)
			{
				if (this.emodel.avatarController != null)
				{
					this.emodel.avatarController.UpdateBool(AvatarController.isAimingHash, value, true);
				}
				this.updateCameraPosition(true);
			}
			if (this is EntityPlayerLocal && this.inventory != null)
			{
				ItemAction itemAction = this.inventory.holdingItem.Actions[1];
				if (itemAction != null)
				{
					itemAction.AimingSet(this.inventory.holdingItemData.actionData[1], value, aimingGun);
				}
			}
		}
	}

	public virtual Vector3 GetChestTransformPosition()
	{
		if (this.IsCrouching || this.bodyDamage.CurrentStun == EnumEntityStunType.Kneel || this.bodyDamage.CurrentStun == EnumEntityStunType.Prone)
		{
			return base.transform.position + new Vector3(0f, this.GetEyeHeight() * 0.25f, 0f);
		}
		return base.transform.position + new Vector3(0f, this.GetEyeHeight() * 0.95f, 0f);
	}

	public virtual bool MovementRunning
	{
		get
		{
			return this.bMovementRunning;
		}
		set
		{
			if (value != this.bMovementRunning)
			{
				this.bMovementRunning = value;
			}
		}
	}

	public virtual bool Crouching
	{
		get
		{
			return this.bCrouching;
		}
		set
		{
			if (value != this.bCrouching)
			{
				this.bCrouching = value;
				if (this.emodel.avatarController != null)
				{
					this.emodel.avatarController.SetCrouching(value);
				}
				this.CurrentStanceTag = (this.bCrouching ? EntityAlive.StanceTagCrouching : EntityAlive.StanceTagStanding);
				this.Buffs.SetCustomVar("_crouching", (float)(this.bCrouching ? 1 : 0), true);
				this.bEntityAliveFlagsChanged |= !this.isEntityRemote;
			}
		}
	}

	public bool IsCrouching
	{
		get
		{
			return this.Crouching || this.CrouchingLocked;
		}
	}

	public virtual bool Jumping
	{
		get
		{
			return this.bJumping && EffectManager.GetValue(PassiveEffects.JumpStrength, null, 1f, this, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false) != 0f;
		}
		set
		{
			if (value != this.bJumping)
			{
				this.bJumping = value;
				if (this.Jumping)
				{
					this.StartJump();
					this.CurrentMovementTag &= EntityAlive.MovementTagIdle;
					this.CurrentMovementTag |= EntityAlive.MovementTagJumping;
				}
				else
				{
					this.EndJump();
					this.CurrentMovementTag &= EntityAlive.MovementTagJumping;
					this.bJumping = false;
				}
				this.bEntityAliveFlagsChanged |= !this.isEntityRemote;
			}
		}
	}

	public bool Climbing
	{
		get
		{
			return this.bClimbing;
		}
		set
		{
			if (value != this.bClimbing)
			{
				this.bClimbing = value;
				this.bPlayerStatsChanged |= !this.isEntityRemote;
				if (this.bClimbing)
				{
					this.CurrentMovementTag &= EntityAlive.MovementTagIdle;
					this.CurrentMovementTag |= EntityAlive.MovementTagClimbing;
					return;
				}
				this.CurrentMovementTag &= EntityAlive.MovementTagClimbing;
			}
		}
	}

	public virtual bool CanNavigatePath()
	{
		return this.onGround || this.isSwimming || this.bInElevator || this.Climbing;
	}

	public virtual bool RightArmAnimationAttack
	{
		get
		{
			return this.emodel.avatarController != null && this.emodel.avatarController.IsAnimationAttackPlaying();
		}
		set
		{
			if (this.emodel.avatarController != null && value && !this.emodel.avatarController.IsAnimationAttackPlaying())
			{
				this.emodel.avatarController.StartAnimationAttack();
			}
		}
	}

	public virtual bool RightArmAnimationUse
	{
		get
		{
			return this.emodel.avatarController != null && this.emodel.avatarController.IsAnimationUsePlaying();
		}
		set
		{
			if (this.emodel.avatarController != null && value != this.emodel.avatarController.IsAnimationUsePlaying())
			{
				this.emodel.avatarController.StartAnimationUse();
			}
		}
	}

	public virtual bool SpecialAttack
	{
		get
		{
			return this.emodel.avatarController != null && this.emodel.avatarController.IsAnimationSpecialAttackPlaying();
		}
		set
		{
			if (this.emodel.avatarController != null && value != this.emodel.avatarController.IsAnimationSpecialAttackPlaying())
			{
				this.bPlayerStatsChanged |= !this.isEntityRemote;
				this.emodel.avatarController.StartAnimationSpecialAttack(value, 0);
			}
		}
	}

	public virtual void StartSpecialAttack(int _animType)
	{
		if (this.emodel.avatarController != null && !this.emodel.avatarController.IsAnimationSpecialAttackPlaying())
		{
			this.bPlayerStatsChanged |= !this.isEntityRemote;
			this.emodel.avatarController.StartAnimationSpecialAttack(true, _animType);
		}
	}

	public virtual bool SpecialAttack2
	{
		get
		{
			return this.emodel.avatarController != null && this.emodel.avatarController.IsAnimationSpecialAttack2Playing();
		}
		set
		{
			if (this.emodel.avatarController != null && value)
			{
				this.bPlayerStatsChanged |= !this.isEntityRemote;
				this.emodel.avatarController.StartAnimationSpecialAttack2();
			}
		}
	}

	public virtual bool Raging
	{
		get
		{
			return this.emodel.avatarController != null && this.emodel.avatarController.IsAnimationRagingPlaying();
		}
		set
		{
			if (this.emodel.avatarController != null && value && !this.emodel.avatarController.IsAnimationRagingPlaying())
			{
				this.emodel.avatarController.StartAnimationRaging();
			}
		}
	}

	public virtual bool Electrocuted
	{
		get
		{
			return this.emodel != null && this.emodel.avatarController != null && this.emodel.avatarController.GetAnimationElectrocuteRemaining() > 0f;
		}
		set
		{
			if (this.emodel != null && this.emodel.avatarController != null && value != this.emodel.avatarController.GetAnimationElectrocuteRemaining() > 0.4f)
			{
				this.bPlayerStatsChanged |= !this.isEntityRemote;
				if (value)
				{
					this.emodel.avatarController.StartAnimationElectrocute(0.6f);
					this.emodel.avatarController.Electrocute(true);
				}
			}
		}
	}

	public virtual bool HarvestingAnimation
	{
		get
		{
			return this.emodel.avatarController != null && this.emodel.avatarController.IsAnimationHarvestingPlaying();
		}
		set
		{
			this.emodel.avatarController.UpdateBool("Harvesting", value, true);
		}
	}

	public virtual void StartHarvestingAnim()
	{
		if (this.emodel != null && this.emodel.avatarController != null)
		{
			this.emodel.avatarController.StartAnimationHarvesting();
		}
	}

	public bool IsEating
	{
		get
		{
			return this.m_isEating;
		}
		set
		{
			if (value != this.m_isEating)
			{
				this.m_isEating = value;
				this.bPlayerStatsChanged |= !this.isEntityRemote;
				if (this.emodel != null && this.emodel.avatarController != null)
				{
					if (this.m_isEating)
					{
						this.emodel.avatarController.StartEating();
						return;
					}
					this.emodel.avatarController.StopEating();
				}
			}
		}
	}

	public virtual void SetVehicleAnimation(int _animHash, int _pose)
	{
		if (this.emodel && this.emodel.avatarController)
		{
			this.emodel.avatarController.SetVehicleAnimation(_animHash, _pose);
			this.bPlayerStatsChanged = !this.isEntityRemote;
		}
	}

	public virtual int GetVehicleAnimation()
	{
		if (this.emodel && this.emodel.avatarController)
		{
			return this.emodel.avatarController.GetVehicleAnimation();
		}
		return -1;
	}

	public virtual int Died
	{
		get
		{
			return this.died;
		}
		set
		{
			if (value != this.died)
			{
				this.died = value;
				this.bPlayerStatsChanged |= !this.isEntityRemote;
			}
		}
	}

	public virtual int Score
	{
		get
		{
			return this.score;
		}
		set
		{
			if (value != this.score)
			{
				this.score = value;
				this.bPlayerStatsChanged |= !this.isEntityRemote;
			}
		}
	}

	public virtual int KilledZombies
	{
		get
		{
			return this.killedZombies;
		}
		set
		{
			if (value != this.killedZombies)
			{
				this.killedZombies = value;
				this.bPlayerStatsChanged |= !this.isEntityRemote;
			}
		}
	}

	public virtual int KilledPlayers
	{
		get
		{
			return this.killedPlayers;
		}
		set
		{
			if (value != this.killedPlayers)
			{
				this.killedPlayers = value;
				this.bPlayerStatsChanged |= !this.isEntityRemote;
			}
		}
	}

	public virtual int TeamNumber
	{
		get
		{
			return this.teamNumber;
		}
		set
		{
			if (value != this.teamNumber)
			{
				this.teamNumber = value;
				this.bPlayerStatsChanged |= !this.isEntityRemote;
				if (!this.isEntityRemote)
				{
					GameManager.Instance.GameMessage(EnumGameMessages.ChangedTeam, this, null);
				}
			}
		}
	}

	public virtual string EntityName
	{
		get
		{
			return this.entityName;
		}
	}

	public override void SetEntityName(string _name)
	{
		if (!_name.Equals(this.entityName))
		{
			this.entityName = _name;
			this.bPlayerStatsChanged |= !this.isEntityRemote;
			this.HandleSetNavName();
		}
	}

	public virtual int DeathHealth
	{
		get
		{
			return this.deathHealth;
		}
		set
		{
			if (value != this.deathHealth)
			{
				this.deathHealth = value;
				this.bPlayerStatsChanged |= !this.isEntityRemote;
			}
		}
	}

	public virtual bool Spawned
	{
		get
		{
			return this.bSpawned;
		}
		set
		{
			if (value != this.bSpawned)
			{
				this.bSpawned = value;
				this.onSpawnStateChanged();
				this.bEntityAliveFlagsChanged |= !this.isEntityRemote;
			}
		}
	}

	public bool IsBreakingBlocks
	{
		get
		{
			return this.m_isBreakingBlocks;
		}
		set
		{
			if (value != this.m_isBreakingBlocks)
			{
				this.m_isBreakingBlocks = value;
				this.bPlayerStatsChanged |= !this.isEntityRemote;
			}
		}
	}

	public override bool IsSpawned()
	{
		return this.bSpawned;
	}

	public virtual EntityBedrollPositionList SpawnPoints
	{
		get
		{
			return this.spawnPoints;
		}
	}

	public virtual void RemoveIKTargets()
	{
		this.emodel.RemoveIKController();
	}

	public virtual void SetIKTargets(List<IKController.Target> targets)
	{
		IKController ikcontroller = this.emodel.AddIKController();
		if (ikcontroller)
		{
			ikcontroller.SetTargets(targets);
		}
	}

	public virtual List<Vector3i> GetDroppedBackpackPositions()
	{
		return this.droppedBackpackPositions;
	}

	public virtual Vector3i GetLastDroppedBackpackPosition()
	{
		if (this.droppedBackpackPositions == null)
		{
			return Vector3i.zero;
		}
		if (this.droppedBackpackPositions.Count == 0)
		{
			return Vector3i.zero;
		}
		List<Vector3i> list = this.droppedBackpackPositions;
		return list[list.Count - 1];
	}

	public virtual bool EqualsDroppedBackpackPositions(Vector3i position)
	{
		if (this.droppedBackpackPositions != null)
		{
			foreach (Vector3i other in this.droppedBackpackPositions)
			{
				if (position.Equals(other))
				{
					return true;
				}
			}
			return false;
		}
		return false;
	}

	public virtual void SetDroppedBackpackPositions(List<Vector3i> positions)
	{
		this.droppedBackpackPositions.Clear();
		if (positions != null)
		{
			this.droppedBackpackPositions.AddRange(positions);
		}
	}

	public virtual void ClearDroppedBackpackPositions()
	{
		this.droppedBackpackPositions.Clear();
	}

	public virtual int Health
	{
		get
		{
			return (int)this.Stats.Health.Value;
		}
		set
		{
			this.Stats.Health.Value = (float)value;
		}
	}

	public virtual float Stamina
	{
		get
		{
			return this.Stats.Stamina.Value;
		}
		set
		{
			this.Stats.Stamina.Value = value;
		}
	}

	public virtual float Water
	{
		get
		{
			return this.Stats.Water.Value;
		}
		set
		{
			this.Stats.Water.Value = value;
		}
	}

	public virtual int GetMaxHealth()
	{
		return (int)this.Stats.Health.Max;
	}

	public virtual int GetMaxStamina()
	{
		return (int)this.Stats.Stamina.Max;
	}

	public virtual int GetMaxWater()
	{
		return (int)this.Stats.Water.Max;
	}

	public virtual bool IsValidAimAssistSlowdownTarget
	{
		get
		{
			return true;
		}
	}

	public virtual bool IsValidAimAssistSnapTarget
	{
		get
		{
			return true;
		}
	}

	public virtual EModelBase.HeadStates CurrentHeadState
	{
		get
		{
			return this.currentHeadState;
		}
		set
		{
			if (value != this.currentHeadState)
			{
				this.currentHeadState = value;
				this.bPlayerStatsChanged |= !this.isEntityRemote;
			}
			this.emodel.ForceHeadState(value);
		}
	}

	public virtual float GetStaminaMultiplier()
	{
		return 1f;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void SetMovementState()
	{
		float num = this.speedStrafe;
		if (num >= 1234f)
		{
			num = 0f;
		}
		float num2 = this.speedForward * this.speedForward + num * num;
		this.MovementState = ((num2 > this.moveSpeedAggro * this.moveSpeedAggro) ? 3 : ((num2 > this.moveSpeed * this.moveSpeed) ? 2 : ((num2 > 0.001f) ? 1 : 0)));
	}

	public virtual void OnUpdateLive()
	{
		this.Stats.Health.RegenerationAmount = 0f;
		this.Stats.Stamina.RegenerationAmount = 0f;
		if (!this.isEntityRemote && !this.IsDead())
		{
			this.Stats.Update(0.05f, this.world.worldTime);
		}
		if (this.jumpTicks > 0)
		{
			this.jumpTicks--;
		}
		if (this.attackTargetTime > 0)
		{
			this.attackTargetTime--;
			if (this.attackTarget != null && this.attackTargetTime == 0)
			{
				this.attackTarget = null;
				if (!this.isEntityRemote)
				{
					this.world.entityDistributer.SendPacketToTrackedPlayersAndTrackedEntity(this.entityId, -1, NetPackageManager.GetPackage<NetPackageSetAttackTarget>().Setup(this.entityId, -1), false);
				}
			}
		}
		this.updateCurrentBlockPosAndValue();
		if (this.AttachedToEntity == null)
		{
			if (this.isEntityRemote)
			{
				if (this.RootMotion)
				{
					this.MoveEntityHeaded(Vector3.zero, false);
				}
			}
			else
			{
				if (this.Health <= 0)
				{
					this.bJumping = false;
					this.bClimbing = false;
					this.moveDirection = Vector3.zero;
				}
				else if (!this.world.IsRemote() && !this.IsDead() && !this.IsClientControlled() && this.hasAI)
				{
					this.updateTasks();
				}
				this.noisePlayer = null;
				this.noisePlayerDistance = 0f;
				this.noisePlayerVolume = 0f;
				if (this.bJumping)
				{
					this.UpdateJump();
				}
				else
				{
					this.jumpTicks = 0;
				}
				float num = this.landMovementFactor;
				this.landMovementFactor *= this.GetSpeedModifier();
				this.MoveEntityHeaded(this.moveDirection, this.isMoveDirAbsolute);
				this.landMovementFactor = num;
			}
			if (this.moveDirection.x > 0f || this.moveDirection.z > 0f)
			{
				if (this.bMovementRunning)
				{
					this.CurrentMovementTag = EntityAlive.MovementTagRunning;
				}
				else
				{
					this.CurrentMovementTag = EntityAlive.MovementTagWalking;
				}
			}
			else
			{
				this.CurrentMovementTag = EntityAlive.MovementTagIdle;
			}
		}
		if (this.bodyDamage.CurrentStun != EnumEntityStunType.None && !this.emodel.IsRagdollActive && !this.IsDead())
		{
			if (this.bodyDamage.CurrentStun == EnumEntityStunType.Getup)
			{
				if (!this.emodel.avatarController || !this.emodel.avatarController.IsAnimationStunRunning())
				{
					this.ClearStun();
				}
			}
			else
			{
				this.bodyDamage.StunDuration = this.bodyDamage.StunDuration - 0.05f;
				if (this.bodyDamage.StunDuration <= 0f)
				{
					this.SetStun(EnumEntityStunType.Getup);
					if (this.emodel.avatarController)
					{
						this.emodel.avatarController.EndStun();
					}
				}
			}
		}
		this.proneRefillCounter += 0.05f * this.proneRefillRate;
		while (this.proneRefillCounter >= 1f)
		{
			this.bodyDamage.StunProne = Mathf.Max(0, this.bodyDamage.StunProne - 1);
			this.proneRefillCounter -= 1f;
		}
		this.kneelRefillCounter += 0.05f * this.kneelRefillRate;
		while (this.kneelRefillCounter >= 1f)
		{
			this.bodyDamage.StunKnee = Mathf.Max(0, this.bodyDamage.StunKnee - 1);
			this.kneelRefillCounter -= 1f;
		}
		EntityPlayer primaryPlayer = this.world.GetPrimaryPlayer();
		if (primaryPlayer != null && primaryPlayer != this)
		{
			int num2 = this.ticksToCheckSeenByPlayer - 1;
			this.ticksToCheckSeenByPlayer = num2;
			if (num2 <= 0)
			{
				this.wasSeenByPlayer = primaryPlayer.CanSee(this);
				if (this.wasSeenByPlayer)
				{
					this.ticksToCheckSeenByPlayer = 200;
				}
				else
				{
					this.ticksToCheckSeenByPlayer = 20;
				}
			}
			else if (this.wasSeenByPlayer)
			{
				primaryPlayer.SetCanSee(this);
			}
		}
		if (this.onGround)
		{
			this.disableFallBehaviorUntilOnGround = false;
		}
		this.UpdateDynamicRagdoll();
		this.checkForTeleportOutOfTraderArea();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void checkForTeleportOutOfTraderArea()
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && !GameManager.Instance.IsEditMode() && !this.IsGodMode.Value && this is EntityPlayer && Time.time - this.lastTimeTraderStationChecked > 2f)
		{
			this.lastTimeTraderStationChecked = Time.time;
			Vector3 position = this.position;
			position.y += 0.5f;
			Vector3i vector3i = World.worldToBlockPos(position);
			TraderArea traderAreaAt = this.world.GetTraderAreaAt(vector3i);
			if (traderAreaAt != null)
			{
				EntityPlayer entityPlayer = this as EntityPlayer;
				bool flag = false;
				Vector3 b = traderAreaAt.ProtectPosition + traderAreaAt.ProtectSize * 0.5f;
				if (entityPlayer && this.world.IsWorldEvent(World.WorldEvent.BloodMoon))
				{
					flag = true;
				}
				Prefab.PrefabTeleportVolume prefabTeleportVolume;
				if ((entityPlayer || this is EntityHuman) && traderAreaAt.IsWithinTeleportArea(position, out prefabTeleportVolume))
				{
					flag = traderAreaAt.IsClosed;
					if (!flag && entityPlayer && EffectManager.GetValue(PassiveEffects.NoTrader, null, 0f, this, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false) == 1f)
					{
						flag = true;
						b = this.world.GetPOIAtPosition(vector3i, true).boundingBoxPosition + prefabTeleportVolume.startPos + prefabTeleportVolume.size * 0.5f;
					}
				}
				if (flag)
				{
					this.traderTeleportStreak++;
					Vector3 normalized = (base.GetPosition() - b).normalized;
					normalized.y = 0f;
					Vector3 vector = base.GetPosition() + normalized * 5f * (float)this.traderTeleportStreak;
					vector = this.GetTeleportPosition(vector, normalized, 5f, 20);
					if (this.isEntityRemote)
					{
						SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.ForEntityId(this.entityId).SendPackage(NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(vector, null, false));
					}
					else if (entityPlayer)
					{
						entityPlayer.Teleport(vector, float.MinValue);
					}
					else if (this.AttachedToEntity != null)
					{
						this.AttachedToEntity.SetPosition(vector, true);
					}
					else
					{
						this.SetPosition(vector, true);
					}
					if (entityPlayer)
					{
						GameEventManager.Current.HandleAction("game_on_trader_teleport", entityPlayer, entityPlayer, false, "", "", false, true, "", null);
						return;
					}
				}
			}
			else
			{
				this.traderTeleportStreak = 1;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 GetTeleportPosition(Vector3 _position, Vector3 _direction, float _directionIncrease = 5f, int _maxAttempts = 20)
	{
		Vector3 result = _position;
		Vector3 result2 = _position;
		bool flag = false;
		int num = 0;
		while (!flag && num < _maxAttempts)
		{
			flag = this.world.GetRandomSpawnPositionMinMaxToPosition(_position, 5, 10, 1, false, out result2, true, true, 20);
			_position += _direction * _directionIncrease;
			num++;
		}
		if (flag)
		{
			return result2;
		}
		Log.Warning("Trader teleport: Could not find a valid teleport position, returning original position");
		return result;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void StartJump()
	{
		this.jumpState = EntityAlive.JumpState.Leap;
		this.jumpStateTicks = 0;
		this.jumpDistance = 1f;
		this.jumpHeightDiff = 0f;
		this.disableFallBehaviorUntilOnGround = true;
		if (this.isSwimming)
		{
			this.jumpState = EntityAlive.JumpState.SwimStart;
			if (this.emodel.avatarController != null)
			{
				this.emodel.avatarController.SetSwim(true);
				return;
			}
		}
		else if (this.emodel.avatarController != null)
		{
			this.emodel.avatarController.StartAnimationJump(AnimJumpMode.Start);
		}
	}

	public virtual void SetJumpDistance(float _distance, float _heightDiff)
	{
		this.jumpDistance = _distance;
		this.jumpHeightDiff = _heightDiff;
	}

	public virtual void SetSwimValues(float _durationTicks, Vector3 _motion)
	{
		this.jumpSwimDurationTicks = Mathf.Clamp(_durationTicks / this.swimSpeed - 6f, 3f, 20f);
		this.jumpSwimMotion = _motion;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateJump()
	{
		if (this.IsFlyMode.Value)
		{
			this.Jumping = false;
			return;
		}
		this.jumpStateTicks++;
		switch (this.jumpState)
		{
		case EntityAlive.JumpState.Leap:
			if (this.accumulatedRootMotion.y > 0.005f || (float)this.jumpStateTicks >= this.jumpDelay)
			{
				this.StartJumpMotion();
				this.jumpTicks = 200;
				this.jumpState = EntityAlive.JumpState.Air;
				this.jumpStateTicks = 0;
				this.jumpIsMoving = true;
				return;
			}
			break;
		case EntityAlive.JumpState.Air:
			if (this.onGround || (this.motionMultiplier < 0.45f && this.jumpStateTicks > 40))
			{
				this.jumpState = EntityAlive.JumpState.Land;
				this.jumpStateTicks = 0;
				this.jumpIsMoving = false;
				return;
			}
			break;
		case EntityAlive.JumpState.Land:
			if (this.jumpStateTicks > 5)
			{
				this.Jumping = false;
				return;
			}
			break;
		case EntityAlive.JumpState.SwimStart:
			if ((float)this.jumpStateTicks > 6f)
			{
				this.jumpTicks = 100;
				this.jumpState = EntityAlive.JumpState.Swim;
				this.jumpStateTicks = 0;
				this.jumpIsMoving = true;
				this.StartJumpSwimMotion();
				return;
			}
			break;
		case EntityAlive.JumpState.Swim:
			if (!this.isSwimming || (float)this.jumpStateTicks >= this.jumpSwimDurationTicks)
			{
				this.Jumping = false;
			}
			break;
		default:
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void StartJumpSwimMotion()
	{
		if (this.inWaterPercent > 0.65f)
		{
			float num = Mathf.Sqrt(this.jumpSwimMotion.x * this.jumpSwimMotion.x + this.jumpSwimMotion.z * this.jumpSwimMotion.z) + 0.001f;
			float min = Mathf.Lerp(-0.6f, -0.05f, num * 0.8f);
			this.jumpSwimMotion.y = Utils.FastClamp(this.jumpSwimMotion.y, min, 1f);
			float num2 = this.jumpSwimDurationTicks;
			float num3 = (num2 - 1f) * this.world.Gravity * 0.025f * 0.4999f;
			num3 /= Mathf.Pow(0.91f, (num2 - 3f) * 0.91f * 0.115f);
			float t = (num2 - 1f) / 15f;
			float num4 = Mathf.LerpUnclamped(0.46f, 0.418600023f, t);
			float num5 = Mathf.Pow(0.91f, (num2 - 1f) * num4);
			float num6 = 1f / num2 / num5;
			num3 += this.jumpSwimMotion.y * num6;
			num6 /= Utils.FastMax(1f, num);
			this.motion.x = this.jumpSwimMotion.x * num6;
			this.motion.z = this.jumpSwimMotion.z * num6;
			this.motion.y = num3;
			return;
		}
		this.motion.y = 0f;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void FaceJumpTo()
	{
		Vector3 vector = this.moveHelper.JumpToPos - this.position;
		float yaw = Mathf.Round(Mathf.Atan2(vector.x, vector.z) * 57.29578f / 90f) * 90f;
		base.SeekYaw(yaw, 0f, 0f);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void StartJumpMotion()
	{
		base.SetAirBorne(true);
		float num = (float)((int)(5f + Mathf.Pow(this.jumpDistance * 8f, 0.5f)));
		this.motion = this.GetForwardVector() * (this.jumpDistance / num);
		float num2 = num * this.world.Gravity * 0.5f;
		this.motion.y = Utils.FastMax(num2 * 0.5f, num2 + this.jumpHeightDiff / num);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void JumpMove()
	{
		this.accumulatedRootMotion = Vector3.zero;
		Vector3 motion = this.motion;
		this.entityCollision(this.motion);
		this.motion.x = motion.x;
		this.motion.z = motion.z;
		if (this.motion.y != 0f)
		{
			this.motion.y = motion.y;
		}
		if (this.jumpState == EntityAlive.JumpState.Air)
		{
			this.motion.y = this.motion.y - this.world.Gravity;
			return;
		}
		this.motion.x = this.motion.x * 0.91f;
		this.motion.z = this.motion.z * 0.91f;
		this.motion.y = this.motion.y - this.world.Gravity * 0.025f;
		this.motion.y = this.motion.y * 0.91f;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void EndJump()
	{
		this.jumpState = EntityAlive.JumpState.Off;
		this.jumpIsMoving = false;
		if (!this.isEntityRemote && this.emodel.avatarController != null)
		{
			this.emodel.avatarController.StartAnimationJump(AnimJumpMode.Land);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool CalcIfSwimming()
	{
		float num = (this.onGround || this.Jumping) ? 0.7f : 0.5f;
		return this.inWaterPercent >= num;
	}

	public override void SwimChanged()
	{
		if (this.emodel.avatarController)
		{
			this.emodel.avatarController.SetSwim(this.isSwimming);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Update()
	{
		base.Update();
		if (!this.isEntityRemote && this.RootMotion && this.lerpForwardSpeed)
		{
			if (this.speedForwardTarget > 0f)
			{
				this.speedForward = Mathf.Lerp(this.speedForward, this.speedForwardTarget, Time.deltaTime * 5f);
			}
			else
			{
				this.speedForward = Mathf.Lerp(this.speedForward, this.speedForwardTarget, Time.deltaTime * 3.8f);
			}
		}
		if (this.isHeadUnderwater != (this.Buffs.GetCustomVar("_underwater", 0f) == 1f))
		{
			this.Buffs.SetCustomVar("_underwater", (float)(this.isHeadUnderwater ? 1 : 0), true);
		}
		this.MinEventContext.Area = this.boundingBox;
		this.MinEventContext.Biome = this.biomeStandingOn;
		this.MinEventContext.ItemValue = this.inventory.holdingItemItemValue;
		this.MinEventContext.BlockValue = this.blockValueStandingOn;
		this.MinEventContext.ItemInventoryData = this.inventory.holdingItemData;
		this.MinEventContext.Position = this.position;
		this.MinEventContext.Seed = this.entityId + Mathf.Abs(GameManager.Instance.World.Seed);
		this.MinEventContext.Transform = base.transform;
		FastTags<TagGroup.Global>.CombineTags(EntityClass.list[this.entityClass].Tags, this.inventory.holdingItem.ItemTags, this.CurrentStanceTag, this.CurrentMovementTag, ref this.MinEventContext.Tags);
		if (this.Progression != null)
		{
			this.Progression.Update();
		}
		if (this.renderFade != this.renderFadeTarget)
		{
			this.renderFade = Mathf.MoveTowards(this.renderFade, this.renderFadeTarget, Time.deltaTime);
			this.emodel.SetFade(this.renderFade);
			bool flag = this.renderFade > 0.01f;
			if (this.emodel.visible != flag)
			{
				this.emodel.SetVisible(flag, false);
			}
		}
	}

	public virtual void OnDeathUpdate()
	{
		if (this.deathUpdateTime < this.timeStayAfterDeath)
		{
			this.deathUpdateTime++;
		}
		int deadBodyHitPoints = EntityClass.list[this.entityClass].DeadBodyHitPoints;
		if (deadBodyHitPoints > 0 && this.DeathHealth <= -deadBodyHitPoints)
		{
			this.deathUpdateTime = this.timeStayAfterDeath;
		}
		if (this.deathUpdateTime < this.timeStayAfterDeath)
		{
			return;
		}
		if (!this.isEntityRemote && !this.markedForUnload)
		{
			this.dropCorpseBlock();
			if (this.particleOnDestroy != null && this.particleOnDestroy.Length > 0)
			{
				float lightBrightness = this.world.GetLightBrightness(base.GetBlockPosition());
				this.world.GetGameManager().SpawnParticleEffectServer(new ParticleEffect(this.particleOnDestroy, this.getHeadPosition(), lightBrightness, Color.white, null, null, false), this.entityId, false, false);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual Vector3i dropCorpseBlock()
	{
		if (this.corpseBlockValue.isair)
		{
			return Vector3i.zero;
		}
		if (this.rand.RandomFloat > this.corpseBlockChance)
		{
			return Vector3i.zero;
		}
		Vector3i vector3i = World.worldToBlockPos(this.position);
		while (vector3i.y < 254 && (float)vector3i.y - this.position.y < 3f && !this.corpseBlockValue.Block.CanPlaceBlockAt(this.world, 0, vector3i, this.corpseBlockValue, false))
		{
			vector3i += Vector3i.up;
		}
		if (vector3i.y >= 254)
		{
			return Vector3i.zero;
		}
		if ((float)vector3i.y - this.position.y >= 2.1f)
		{
			return Vector3i.zero;
		}
		this.world.SetBlockRPC(vector3i, this.corpseBlockValue);
		return vector3i;
	}

	public void NotifyRootMotion(Animator animator)
	{
		this.accumulatedRootMotion += animator.deltaPosition;
	}

	public virtual float MaxVelocity
	{
		get
		{
			return 5f;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void DefaultMoveEntity(Vector3 _direction, bool _isDirAbsolute)
	{
		float num = 0.91f;
		if (this.onGround)
		{
			num = 0.546f;
			if (!this.IsDead() && this is EntityPlayer)
			{
				BlockValue block = this.world.GetBlock(Utils.Fastfloor(this.position.x), Utils.Fastfloor(this.boundingBox.min.y), Utils.Fastfloor(this.position.z));
				if (block.isair || block.Block.blockMaterial.IsGroundCover)
				{
					block = this.world.GetBlock(Utils.Fastfloor(this.position.x), Utils.Fastfloor(this.boundingBox.min.y - 1f), Utils.Fastfloor(this.position.z));
				}
				if (!block.isair)
				{
					num = Mathf.Clamp(1f - block.Block.blockMaterial.Friction, 0.01f, 1f);
				}
			}
		}
		if (!this.RootMotion || (!this.onGround && this.jumpTicks > 0))
		{
			float num2;
			if (this.onGround)
			{
				num2 = this.landMovementFactor;
				float num3 = 0.163f / (num * num * num);
				num2 *= num3;
			}
			else
			{
				num2 = this.jumpMovementFactor;
			}
			this.Move(_direction, _isDirAbsolute, num2, this.MaxVelocity);
		}
		if (this.Climbing)
		{
			this.fallDistance = 0f;
			this.entityCollision(this.motion);
			this.distanceClimbed += this.motion.magnitude;
			if (this.distanceClimbed > 0.5f)
			{
				this.internalPlayStepSound();
				this.distanceClimbed = 0f;
			}
		}
		else
		{
			if (base.IsInElevator())
			{
				if (!this.RootMotion)
				{
					float num4 = 0.15f;
					if (this.motion.x < -num4)
					{
						this.motion.x = -num4;
					}
					if (this.motion.x > num4)
					{
						this.motion.x = num4;
					}
					if (this.motion.z < -num4)
					{
						this.motion.z = -num4;
					}
					if (this.motion.z > num4)
					{
						this.motion.z = num4;
					}
				}
				this.fallDistance = 0f;
			}
			if (this.IsSleeping)
			{
				this.motion.x = 0f;
				this.motion.z = 0f;
			}
			this.entityCollision(this.motion);
		}
		if (this.isSwimming)
		{
			this.motion.x = this.motion.x * 0.91f;
			this.motion.z = this.motion.z * 0.91f;
			this.motion.y = this.motion.y - this.world.Gravity * 0.025f;
			this.motion.y = this.motion.y * 0.91f;
			return;
		}
		this.motion.x = this.motion.x * num;
		this.motion.z = this.motion.z * num;
		if (!this.bInElevator)
		{
			this.motion.y = this.motion.y - this.world.Gravity;
		}
		this.motion.y = this.motion.y * 0.98f;
	}

	public virtual void MoveEntityHeaded(Vector3 _direction, bool _isDirAbsolute)
	{
		if (this.AttachedToEntity != null)
		{
			return;
		}
		if (this.jumpIsMoving)
		{
			this.JumpMove();
			return;
		}
		if (this.RootMotion)
		{
			if (this.isEntityRemote && this.bodyDamage.CurrentStun == EnumEntityStunType.None && !this.IsDead() && (!(this.emodel != null) || !(this.emodel.avatarController != null) || !this.emodel.avatarController.IsAnimationHitRunning()))
			{
				this.accumulatedRootMotion = Vector3.zero;
				return;
			}
			bool flag = this.emodel && this.emodel.IsRagdollActive;
			if (this.isSwimming && !flag)
			{
				this.motion += this.accumulatedRootMotion * 0.001f;
			}
			else if (this.onGround || this.jumpTicks > 0)
			{
				if (flag)
				{
					this.motion.x = 0f;
					this.motion.z = 0f;
				}
				else
				{
					float y = this.motion.y;
					this.motion = this.accumulatedRootMotion;
					this.motion.y = this.motion.y + y;
				}
			}
			this.accumulatedRootMotion = Vector3.zero;
		}
		if (this.IsFlyMode.Value)
		{
			EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
			float num = (primaryPlayer != null) ? primaryPlayer.GodModeSpeedModifier : 1f;
			float num2 = 2f * (this.MovementRunning ? 0.35f : 0.12f) * num;
			if (!this.RootMotion)
			{
				this.Move(_direction, _isDirAbsolute, this.GetPassiveEffectSpeedModifier() * num2, this.GetPassiveEffectSpeedModifier() * num2);
			}
			if (!this.IsNoCollisionMode.Value)
			{
				this.entityCollision(this.motion);
				this.motion *= base.ConditionalScalePhysicsMulConstant(0.546f);
			}
			else
			{
				this.SetPosition(this.position + this.motion, true);
				this.motion = Vector3.zero;
			}
		}
		else
		{
			this.DefaultMoveEntity(_direction, _isDirAbsolute);
		}
		if (!this.isEntityRemote && this.RootMotion)
		{
			float num3 = this.landMovementFactor;
			num3 *= 2.5f;
			if (this.inWaterPercent > 0.3f)
			{
				if (num3 > 0.01f)
				{
					float t = (this.inWaterPercent - 0.3f) * 1.42857146f;
					num3 = Mathf.Lerp(num3, 0.01f + (num3 - 0.01f) * 0.1f, t);
				}
				if (this.isSwimming)
				{
					num3 = this.landMovementFactor * 5f;
				}
			}
			float num4 = _direction.magnitude;
			num4 = Mathf.Max(num4, 1f);
			float num5 = num3 / num4;
			if (this.lerpForwardSpeed)
			{
				this.speedForwardTarget = _direction.z * num5;
			}
			else
			{
				this.speedForward = _direction.z * num5;
			}
			this.speedStrafe = _direction.x * num5;
			this.SetMovementState();
			base.ReplicateSpeeds();
		}
	}

	public float GetPassiveEffectSpeedModifier()
	{
		if (this.IsCrouching)
		{
			if (this.MovementRunning)
			{
				return EffectManager.GetValue(PassiveEffects.WalkSpeed, null, Constants.cPlayerSpeedModifierWalking, this, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
			}
			return EffectManager.GetValue(PassiveEffects.CrouchSpeed, null, Constants.cPlayerSpeedModifierCrouching, this, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
		}
		else
		{
			if (this.MovementRunning)
			{
				return EffectManager.GetValue(PassiveEffects.RunSpeed, null, Constants.cPlayerSpeedModifierRunning, this, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
			}
			return EffectManager.GetValue(PassiveEffects.WalkSpeed, null, Constants.cPlayerSpeedModifierWalking, this, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
		}
	}

	public void SetMoveForward(float _moveForward)
	{
		this.moveDirection.x = 0f;
		this.moveDirection.z = _moveForward;
		this.isMoveDirAbsolute = false;
		this.Climbing = false;
		this.lerpForwardSpeed = true;
		this.motion.x = 0f;
		this.motion.z = 0f;
		this.accumulatedRootMotion.x = 0f;
		this.accumulatedRootMotion.z = 0f;
		if (this.bInElevator)
		{
			this.motion.y = 0f;
		}
	}

	public void SetMoveForwardWithModifiers(float _speedModifier, float _speedScale, bool _climb)
	{
		this.moveDirection.x = 0f;
		this.moveDirection.z = 1f;
		this.isMoveDirAbsolute = false;
		this.Climbing = _climb;
		this.lerpForwardSpeed = true;
		float num = this.speedModifier;
		this.speedModifier = _speedModifier * _speedScale;
		if (num > 0.2f)
		{
			num = this.speedModifier / num;
			this.accumulatedRootMotion.x = this.accumulatedRootMotion.x * num;
			this.accumulatedRootMotion.z = this.accumulatedRootMotion.z * num;
		}
	}

	public void AddMotion(float dir, float speed)
	{
		float f = dir * 0.0174532924f;
		this.accumulatedRootMotion.x = this.accumulatedRootMotion.x + Mathf.Sin(f) * speed;
		this.accumulatedRootMotion.z = this.accumulatedRootMotion.z + Mathf.Cos(f) * speed;
	}

	public void MakeMotionMoveToward(float x, float z, float minMotion, float maxMotion)
	{
		if (this.RootMotion)
		{
			float num = Mathf.Sqrt(x * x + z * z);
			if (num > 0f)
			{
				num = Utils.FastClamp(Mathf.Sqrt(this.accumulatedRootMotion.x * this.accumulatedRootMotion.x + this.accumulatedRootMotion.z * this.accumulatedRootMotion.z), minMotion, maxMotion) / num;
				if (num < 1f)
				{
					x *= num;
					z *= num;
				}
			}
			this.accumulatedRootMotion.x = x;
			this.accumulatedRootMotion.z = z;
			return;
		}
		this.moveDirection.x = x;
		this.moveDirection.z = z;
		this.isMoveDirAbsolute = true;
	}

	public bool IsInFrontOfMe(Vector3 _position)
	{
		Vector3 headPosition = this.getHeadPosition();
		Vector3 dir = _position - headPosition;
		Vector3 forwardVector = this.GetForwardVector();
		float angleBetween = Utils.GetAngleBetween(dir, forwardVector);
		float num = this.GetMaxViewAngle() * 0.5f;
		return angleBetween >= -num && angleBetween <= num;
	}

	public bool IsInViewCone(Vector3 _position)
	{
		Vector3 headPosition = this.getHeadPosition();
		Vector3 dir = _position - headPosition;
		Vector3 lookVector;
		float num;
		if (this.IsSleeping)
		{
			lookVector = this.sleeperLookDir;
			num = this.sleeperViewAngle;
		}
		else
		{
			lookVector = this.GetLookVector();
			num = this.GetMaxViewAngle();
		}
		num *= 0.5f;
		float angleBetween = Utils.GetAngleBetween(dir, lookVector);
		return angleBetween >= -num && angleBetween <= num;
	}

	public void DrawViewCone()
	{
		Vector3 vector;
		float num;
		if (this.IsSleeping)
		{
			vector = this.sleeperLookDir;
			num = this.sleeperViewAngle;
		}
		else
		{
			vector = this.GetLookVector();
			num = this.GetMaxViewAngle();
		}
		vector *= this.GetSeeDistance();
		num *= 0.5f;
		Vector3 start = this.getHeadPosition() - Origin.position;
		Debug.DrawRay(start, vector, new Color(0.9f, 0.9f, 0.5f), 0.1f);
		Vector3 dir = Quaternion.Euler(0f, -num, 0f) * vector;
		Debug.DrawRay(start, dir, new Color(0.6f, 0.6f, 0.3f), 0.1f);
		Vector3 dir2 = Quaternion.Euler(0f, num, 0f) * vector;
		Debug.DrawRay(start, dir2, new Color(0.6f, 0.6f, 0.3f), 0.1f);
	}

	public bool CanSee(Vector3 _pos)
	{
		Vector3 headPosition = this.getHeadPosition();
		Vector3 direction = _pos - headPosition;
		float seeDistance = this.GetSeeDistance();
		if (direction.magnitude > seeDistance)
		{
			return false;
		}
		if (!this.IsInViewCone(_pos))
		{
			return false;
		}
		Ray ray = new Ray(headPosition, direction);
		ray.origin += direction.normalized * 0.2f;
		int modelLayer = this.GetModelLayer();
		this.SetModelLayer(2, false, null);
		bool result = true;
		if (Voxel.Raycast(this.world, ray, seeDistance, false, false))
		{
			result = false;
		}
		this.SetModelLayer(modelLayer, false, null);
		return result;
	}

	public bool CanEntityBeSeen(Entity _other)
	{
		Vector3 headPosition = this.getHeadPosition();
		Vector3 headPosition2 = _other.getHeadPosition();
		Vector3 direction = headPosition2 - headPosition;
		float magnitude = direction.magnitude;
		float num = this.GetSeeDistance();
		EntityPlayer entityPlayer = _other as EntityPlayer;
		if (entityPlayer != null)
		{
			num *= entityPlayer.DetectUsScale(this);
		}
		if (magnitude > num)
		{
			return false;
		}
		if (!this.IsInViewCone(headPosition2))
		{
			return false;
		}
		bool result = false;
		Ray ray = new Ray(headPosition, direction);
		ray.origin += direction.normalized * -0.1f;
		int modelLayer = this.GetModelLayer();
		this.SetModelLayer(2, false, null);
		if (Voxel.Raycast(this.world, ray, num, -1612492821, 64, 0f))
		{
			if (Voxel.voxelRayHitInfo.tag == "E_Vehicle")
			{
				EntityVehicle entityVehicle = EntityVehicle.FindCollisionEntity(Voxel.voxelRayHitInfo.transform);
				if (entityVehicle && entityVehicle.IsAttached(_other))
				{
					result = true;
				}
			}
			else
			{
				if (Voxel.voxelRayHitInfo.tag.StartsWith("E_BP_"))
				{
					Voxel.voxelRayHitInfo.transform = GameUtils.GetHitRootTransform(Voxel.voxelRayHitInfo.tag, Voxel.voxelRayHitInfo.transform);
				}
				if (_other.transform == Voxel.voxelRayHitInfo.transform)
				{
					result = true;
				}
			}
		}
		this.SetModelLayer(modelLayer, false, null);
		return result;
	}

	public virtual float GetSeeDistance()
	{
		this.senseScale = 1f;
		if (this.IsSleeping)
		{
			this.sightRange = this.sleeperSightRange;
			return this.sleeperSightRange;
		}
		this.sightRange = this.sightRangeBase;
		if (this.aiManager != null)
		{
			float num = EAIManager.CalcSenseScale();
			this.senseScale = 1f + num * this.aiManager.feralSense;
			this.sightRange = this.sightRangeBase * this.senseScale;
		}
		return this.sightRange;
	}

	public bool CanSeeStealth(float dist, float lightLevel)
	{
		float t = dist / this.sightRange;
		float num = Utils.FastLerp(this.sightLightThreshold.x, this.sightLightThreshold.y, t);
		return lightLevel > num;
	}

	public float GetSeeStealthDebugScale(float dist)
	{
		float t = dist / this.sightRange;
		return Utils.FastLerp(this.sightLightThreshold.x, this.sightLightThreshold.y, t);
	}

	public override void SetAlive()
	{
		if (this.IsDead())
		{
			this.lastAliveTime = Time.time;
		}
		base.SetAlive();
		if (!this.isEntityRemote)
		{
			this.Stats.ResetStats();
		}
		this.Stats.Health.MaxModifier = 0f;
		this.Health = (int)this.Stats.Health.ModifiedMax;
		this.Stamina = this.Stats.Stamina.ModifiedMax;
		this.deathUpdateTime = 0;
		this.bDead = false;
		this.RecordedDamage.Fatal = false;
		this.emodel.SetAlive();
	}

	public float YawForTarget(Entity _otherEntity)
	{
		return this.YawForTarget(_otherEntity.GetPosition());
	}

	public float YawForTarget(Vector3 target)
	{
		float num = target.x - this.position.x;
		return -(float)(Math.Atan2((double)(target.z - this.position.z), (double)num) * 180.0 / 3.1415926535897931) + 90f;
	}

	public void RotateTo(Entity _otherEntity, float _dYaw, float _dPitch)
	{
		float num = _otherEntity.position.x - this.position.x;
		float num2 = _otherEntity.position.z - this.position.z;
		float num3;
		if (_otherEntity is EntityAlive)
		{
			EntityAlive entityAlive = (EntityAlive)_otherEntity;
			num3 = this.position.y + this.GetEyeHeight() - (entityAlive.position.y + entityAlive.GetEyeHeight());
		}
		else
		{
			num3 = (_otherEntity.boundingBox.min.y + _otherEntity.boundingBox.max.y) / 2f - (this.position.y + this.GetEyeHeight());
		}
		float num4 = Mathf.Sqrt(num * num + num2 * num2);
		float intendedRotation = -(float)(Math.Atan2((double)num2, (double)num) * 180.0 / 3.1415926535897931) + 90f;
		float intendedRotation2 = (float)(-(float)(Math.Atan2((double)num3, (double)num4) * 180.0 / 3.1415926535897931));
		this.rotation.x = EntityAlive.UpdateRotation(this.rotation.x, intendedRotation2, _dPitch);
		this.rotation.y = EntityAlive.UpdateRotation(this.rotation.y, intendedRotation, _dYaw);
	}

	public void RotateTo(float _x, float _y, float _z, float _dYaw, float _dPitch)
	{
		float num = _x - this.position.x;
		float num2 = _z - this.position.z;
		float num3 = Mathf.Sqrt(num * num + num2 * num2);
		float intendedRotation = -(float)(Math.Atan2((double)num2, (double)num) * 180.0 / 3.1415926535897931) + 90f;
		this.rotation.y = EntityAlive.UpdateRotation(this.rotation.y, intendedRotation, _dYaw);
		if (_dPitch > 0f)
		{
			float intendedRotation2 = (float)(-(float)(Math.Atan2((double)(_y - this.position.y), (double)num3) * 180.0 / 3.1415926535897931));
			this.rotation.x = -EntityAlive.UpdateRotation(this.rotation.x, intendedRotation2, _dPitch);
		}
	}

	public static float UpdateRotation(float _curRotation, float _intendedRotation, float _maxIncr)
	{
		float num;
		for (num = _intendedRotation - _curRotation; num < -180f; num += 360f)
		{
		}
		while (num >= 180f)
		{
			num -= 360f;
		}
		if (num > _maxIncr)
		{
			num = _maxIncr;
		}
		if (num < -_maxIncr)
		{
			num = -_maxIncr;
		}
		return _curRotation + num;
	}

	public override float GetEyeHeight()
	{
		if (this.walkType == 21)
		{
			return 0.15f;
		}
		if (this.walkType == 22)
		{
			return 0.6f;
		}
		if (!this.IsCrouching)
		{
			return base.height * 0.8f;
		}
		return base.height * 0.5f;
	}

	public virtual float GetSpeedModifier()
	{
		return this.speedModifier;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void fallHitGround(float _distance, Vector3 _fallMotion)
	{
		base.fallHitGround(_distance, _fallMotion);
		if (_distance > 2f)
		{
			int num = (int)((-_fallMotion.y - 0.85f) * 160f);
			if (num > 0)
			{
				this.DamageEntity(DamageSource.fall, num, false, 1f);
			}
			this.PlayHitGroundSound();
		}
		if (!this.IsDead() && !this.emodel.IsRagdollActive && (this.disableFallBehaviorUntilOnGround || !this.ChooseFallBehavior(_distance, _fallMotion)) && this.emodel && this.emodel.avatarController)
		{
			this.emodel.avatarController.StartAnimationJump(AnimJumpMode.Land);
		}
		if (this.aiManager != null)
		{
			this.aiManager.FallHitGround(_distance);
		}
	}

	public bool NotifyDestroyedBlock(ItemActionAttack.AttackHitInfo attackHitInfo)
	{
		if (attackHitInfo == null || this.moveHelper == null || !this.moveHelper.IsBlocked)
		{
			return false;
		}
		if (this.moveHelper.HitInfo != null && this.moveHelper.HitInfo.hit.blockPos == attackHitInfo.hitPosition)
		{
			this.moveHelper.ClearBlocked();
		}
		if (this._destroyBlockBehaviors.Count == 0)
		{
			return false;
		}
		float num = 0f;
		EntityAlive.weightBehaviorTemp.Clear();
		int @int = GameStats.GetInt(EnumGameStats.GameDifficulty);
		for (int i = 0; i < this._destroyBlockBehaviors.Count; i++)
		{
			EntityAlive.DestroyBlockBehavior destroyBlockBehavior = this._destroyBlockBehaviors[i];
			if (@int >= destroyBlockBehavior.Difficulty.min && @int <= destroyBlockBehavior.Difficulty.max)
			{
				EntityAlive.WeightBehavior item;
				item.weight = destroyBlockBehavior.Weight + num;
				item.index = i;
				EntityAlive.weightBehaviorTemp.Add(item);
				num += destroyBlockBehavior.Weight;
			}
		}
		bool result = false;
		if (num > 0f)
		{
			EntityAlive.DestroyBlockBehavior destroyBlockBehavior2 = null;
			float num2 = this.rand.RandomFloat * num;
			for (int j = 0; j < EntityAlive.weightBehaviorTemp.Count; j++)
			{
				if (num2 <= EntityAlive.weightBehaviorTemp[j].weight)
				{
					destroyBlockBehavior2 = this._destroyBlockBehaviors[EntityAlive.weightBehaviorTemp[j].index];
					break;
				}
			}
			if (destroyBlockBehavior2 != null)
			{
				result = this.ExecuteDestroyBlockBehavior(destroyBlockBehavior2, attackHitInfo);
			}
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool ExecuteDestroyBlockBehavior(EntityAlive.DestroyBlockBehavior behavior, ItemActionAttack.AttackHitInfo attackHitInfo)
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool ChooseFallBehavior(float _distance, Vector3 _fallMotion)
	{
		if (this.fallBehaviors.Count == 0)
		{
			return false;
		}
		float num = 0f;
		EntityAlive.weightBehaviorTemp.Clear();
		int @int = GameStats.GetInt(EnumGameStats.GameDifficulty);
		for (int i = 0; i < this.fallBehaviors.Count; i++)
		{
			EntityAlive.FallBehavior fallBehavior = this.fallBehaviors[i];
			if (_distance >= fallBehavior.Height.min && _distance <= fallBehavior.Height.max && @int >= fallBehavior.Difficulty.min && @int <= fallBehavior.Difficulty.max)
			{
				EntityAlive.WeightBehavior item;
				item.weight = fallBehavior.Weight + num;
				item.index = i;
				EntityAlive.weightBehaviorTemp.Add(item);
				num += fallBehavior.Weight;
			}
		}
		bool result = false;
		if (num > 0f)
		{
			EntityAlive.FallBehavior fallBehavior2 = null;
			float num2 = this.rand.RandomFloat * num;
			for (int j = 0; j < EntityAlive.weightBehaviorTemp.Count; j++)
			{
				if (num2 <= EntityAlive.weightBehaviorTemp[j].weight)
				{
					fallBehavior2 = this.fallBehaviors[EntityAlive.weightBehaviorTemp[j].index];
					break;
				}
			}
			if (fallBehavior2 != null)
			{
				result = this.ExecuteFallBehavior(fallBehavior2, _distance, _fallMotion);
			}
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool ExecuteFallBehavior(EntityAlive.FallBehavior behavior, float _distance, Vector3 _fallMotion)
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PlayHitGroundSound()
	{
		if (this.soundLand == null || this.soundLand.Length == 0)
		{
			this.PlayOneShot("entityhitsground", false, false, false);
			return;
		}
		this.PlayOneShot(this.soundLand, false, false, false);
	}

	public virtual bool FriendlyFireCheck(EntityAlive other)
	{
		return true;
	}

	public virtual bool HasImmunity(BuffClass _buffClass)
	{
		return false;
	}

	public int CalculateBlockDamage(BlockDamage block, int defaultBlockDamage, out bool bypassMaxDamage)
	{
		if (this.stompsSpikes && block.HasTag(BlockTags.Spike))
		{
			bypassMaxDamage = true;
			return 999;
		}
		bypassMaxDamage = false;
		return defaultBlockDamage;
	}

	public override int DamageEntity(DamageSource _damageSource, int _strength, bool _criticalHit, float _impulseScale = 1f)
	{
		EnumDamageSource source = _damageSource.GetSource();
		if (_damageSource.IsIgnoreConsecutiveDamages() && source != EnumDamageSource.Internal)
		{
			if (this.damageSourceTimeouts.ContainsKey(source) && GameTimer.Instance.ticks - this.damageSourceTimeouts[source] < 30UL)
			{
				return -1;
			}
			this.damageSourceTimeouts[source] = GameTimer.Instance.ticks;
		}
		EntityAlive entityAlive = this.world.GetEntity(_damageSource.getEntityId()) as EntityAlive;
		if (!this.FriendlyFireCheck(entityAlive))
		{
			return -1;
		}
		bool flag = _damageSource.GetDamageType() == EnumDamageTypes.Heat;
		if (!flag && entityAlive && (this.entityFlags & entityAlive.entityFlags & EntityFlags.Zombie) > EntityFlags.None)
		{
			return -1;
		}
		if (this.IsGodMode.Value)
		{
			return -1;
		}
		if (!this.IsDead() && entityAlive)
		{
			float value = EffectManager.GetValue(PassiveEffects.DamageBonus, null, 0f, entityAlive, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
			if (value > 0f)
			{
				_damageSource.DamageMultiplier = value;
				_damageSource.BonusDamageType = EnumDamageBonusType.Sneak;
			}
		}
		float value2 = EffectManager.GetValue(PassiveEffects.GeneralDamageResist, null, 0f, this, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
		float num = (float)_strength * value2 + this.accumulatedDamageResisted;
		int num2 = (int)num;
		this.accumulatedDamageResisted = num - (float)num2;
		_strength -= num2;
		DamageResponse damageResponse = this.damageEntityLocal(_damageSource, _strength, _criticalHit, _impulseScale);
		NetPackage package = NetPackageManager.GetPackage<NetPackageDamageEntity>().Setup(this.entityId, damageResponse);
		if (this.world.IsRemote())
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(package, false);
		}
		else
		{
			int excludePlayer = -1;
			if (!flag && _damageSource.CreatorEntityId != -2)
			{
				excludePlayer = _damageSource.getEntityId();
				if (_damageSource.CreatorEntityId != -1)
				{
					Entity entity = this.world.GetEntity(_damageSource.CreatorEntityId);
					if (entity && !entity.isEntityRemote)
					{
						excludePlayer = -1;
					}
				}
			}
			this.world.entityDistributer.SendPacketToTrackedPlayersAndTrackedEntity(this.entityId, excludePlayer, package, false);
		}
		return damageResponse.ModStrength;
	}

	public virtual void SetDamagedTarget(EntityAlive _attackTarget)
	{
		this.damagedTarget = _attackTarget;
	}

	public virtual void ClearDamagedTarget()
	{
		this.damagedTarget = null;
	}

	public EntityAlive GetDamagedTarget()
	{
		return this.damagedTarget;
	}

	public override bool IsDead()
	{
		return base.IsDead() || this.RecordedDamage.Fatal;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual DamageResponse damageEntityLocal(DamageSource _damageSource, int _strength, bool _criticalHit, float impulseScale)
	{
		DamageResponse damageResponse = default(DamageResponse);
		damageResponse.Source = _damageSource;
		damageResponse.Strength = _strength;
		damageResponse.Critical = _criticalHit;
		damageResponse.HitDirection = Utils.EnumHitDirection.None;
		damageResponse.MovementState = this.MovementState;
		damageResponse.Random = this.rand.RandomFloat;
		damageResponse.ImpulseScale = impulseScale;
		damageResponse.HitBodyPart = _damageSource.GetEntityDamageBodyPart(this);
		damageResponse.ArmorSlot = _damageSource.GetEntityDamageEquipmentSlot(this);
		damageResponse.ArmorSlotGroup = _damageSource.GetEntityDamageEquipmentSlotGroup(this);
		if (_strength > 0)
		{
			damageResponse.HitDirection = (_damageSource.Equals(DamageSource.fall) ? Utils.EnumHitDirection.Back : ((Utils.EnumHitDirection)Utils.Get4HitDirectionAsInt(_damageSource.getDirection(), this.GetLookVector())));
		}
		if (!GameManager.IsDedicatedServer && _damageSource.damageSource != EnumDamageSource.Internal && GameManager.Instance != null)
		{
			World world = GameManager.Instance.World;
			if (world != null && _damageSource.getEntityId() == world.GetPrimaryPlayerId())
			{
				Transform hitTransform = this.emodel.GetHitTransform(_damageSource);
				Vector3 position;
				if (hitTransform)
				{
					position = hitTransform.position;
				}
				else
				{
					position = this.emodel.transform.position;
				}
				bool flag = world.GetPrimaryPlayer().inventory.holdingItem.HasAnyTags(FastTags<TagGroup.Global>.Parse("ranged"));
				float magnitude = (world.GetPrimaryPlayer().GetPosition() - position).magnitude;
				if (flag && magnitude > EntityAlive.HitSoundDistance)
				{
					Manager.PlayInsidePlayerHead("HitEntitySound", -1, 0f, false, false);
				}
				if (EntityAlive.ShowDebugDisplayHit)
				{
					Transform transform = hitTransform ? hitTransform : this.emodel.transform;
					Vector3 position2 = Camera.main.transform.position;
					DebugLines.CreateAttached("EntityDamage" + this.entityId.ToString(), transform, position2 + Origin.position, _damageSource.getHitTransformPosition(), new Color(0.3f, 0f, 0.3f), new Color(1f, 0f, 1f), EntityAlive.DebugDisplayHitSize * 2f, EntityAlive.DebugDisplayHitSize, EntityAlive.DebugDisplayHitTime);
					DebugLines.CreateAttached("EntityDamage2" + this.entityId.ToString(), transform, _damageSource.getHitTransformPosition(), transform.position + Origin.position, new Color(0f, 0f, 0.5f), new Color(0.3f, 0.3f, 1f), EntityAlive.DebugDisplayHitSize * 2f, EntityAlive.DebugDisplayHitSize, EntityAlive.DebugDisplayHitTime);
				}
			}
		}
		this.MinEventContext.Other = (this.world.GetEntity(damageResponse.Source.getEntityId()) as EntityAlive);
		if (_damageSource.AffectedByArmor())
		{
			this.equipment.CalcDamage(ref damageResponse.Strength, ref damageResponse.ArmorDamage, damageResponse.Source.DamageTypeTag, this.MinEventContext.Other, damageResponse.Source.AttackingItem);
		}
		float num = this.GetDamageFraction((float)damageResponse.Strength);
		if (damageResponse.Fatal || damageResponse.Strength >= this.Health)
		{
			if ((damageResponse.HitBodyPart & EnumBodyPartHit.Head) > EnumBodyPartHit.None)
			{
				if (num >= 0.2f)
				{
					damageResponse.Source.DismemberChance = Utils.FastMax(damageResponse.Source.DismemberChance * 0.5f, 0.3f);
				}
			}
			else if (num >= 0.12f)
			{
				damageResponse.Source.DismemberChance = Utils.FastMax(damageResponse.Source.DismemberChance * 0.5f, 0.5f);
			}
			num = 1f;
		}
		this.CheckDismember(ref damageResponse, num);
		int num2 = this.bodyDamage.StunKnee;
		int num3 = this.bodyDamage.StunProne;
		if ((damageResponse.HitBodyPart & EnumBodyPartHit.Head) > EnumBodyPartHit.None && damageResponse.Dismember)
		{
			if (this.Health > 0)
			{
				damageResponse.Strength = this.Health;
			}
		}
		else if (_damageSource.CanStun && this.GetWalkType() != 21 && this.bodyDamage.CurrentStun != EnumEntityStunType.Prone)
		{
			if ((damageResponse.HitBodyPart & (EnumBodyPartHit.Torso | EnumBodyPartHit.Head | EnumBodyPartHit.LeftUpperArm | EnumBodyPartHit.RightUpperArm | EnumBodyPartHit.LeftLowerArm | EnumBodyPartHit.RightLowerArm)) > EnumBodyPartHit.None)
			{
				num3 += _strength;
			}
			else if (damageResponse.HitBodyPart.IsLeg())
			{
				num2 += _strength * (_criticalHit ? 2 : 1);
			}
		}
		if ((!damageResponse.HitBodyPart.IsLeg() || !damageResponse.Dismember) && this.GetWalkType() != 21 && !this.sleepingOrWakingUp)
		{
			EntityClass entityClass = EntityClass.list[this.entityClass];
			if (this.GetDamageFraction((float)num3) >= entityClass.KnockdownProneDamageThreshold && entityClass.KnockdownProneDamageThreshold > 0f)
			{
				if (this.bodyDamage.CurrentStun != EnumEntityStunType.Prone)
				{
					damageResponse.Stun = EnumEntityStunType.Prone;
					damageResponse.StunDuration = this.rand.RandomRange(entityClass.KnockdownProneStunDuration.x, entityClass.KnockdownProneStunDuration.y);
				}
			}
			else if (this.GetDamageFraction((float)num2) >= entityClass.KnockdownKneelDamageThreshold && entityClass.KnockdownKneelDamageThreshold > 0f && this.bodyDamage.CurrentStun != EnumEntityStunType.Prone)
			{
				damageResponse.Stun = EnumEntityStunType.Kneel;
				damageResponse.StunDuration = this.rand.RandomRange(entityClass.KnockdownKneelStunDuration.x, entityClass.KnockdownKneelStunDuration.y);
			}
		}
		bool flag2 = false;
		int num4 = damageResponse.Strength + damageResponse.ArmorDamage / 2;
		if (num4 > 0 && !this.IsGodMode.Value && damageResponse.Stun == EnumEntityStunType.None && !this.sleepingOrWakingUp)
		{
			flag2 = (damageResponse.Strength < this.Health);
			if (flag2)
			{
				flag2 = (this.GetWalkType() == 21 || !damageResponse.Dismember || !damageResponse.HitBodyPart.IsLeg());
			}
			if (flag2 && damageResponse.Source.GetDamageType() != EnumDamageTypes.Bashing)
			{
				flag2 = (num4 >= 6);
			}
			if (damageResponse.Source.GetDamageType() == EnumDamageTypes.BarbedWire)
			{
				flag2 = true;
			}
		}
		damageResponse.PainHit = flag2;
		if (damageResponse.Strength >= this.Health)
		{
			damageResponse.Fatal = true;
		}
		if (damageResponse.Fatal)
		{
			damageResponse.Stun = EnumEntityStunType.None;
		}
		if (this.isEntityRemote)
		{
			damageResponse.ModStrength = 0;
		}
		else
		{
			if (this.Health <= damageResponse.Strength)
			{
				_strength -= this.Health;
			}
			damageResponse.ModStrength = _strength;
		}
		if (damageResponse.Dismember)
		{
			EntityAlive entityAlive = this.world.GetEntity(damageResponse.Source.getEntityId()) as EntityAlive;
			if (entityAlive != null)
			{
				entityAlive.FireEvent(MinEventTypes.onDismember, true);
			}
		}
		if (this.MinEventContext.Other != null)
		{
			this.MinEventContext.Other.MinEventContext.DamageResponse = damageResponse;
			float value = EffectManager.GetValue(PassiveEffects.HealthSteal, null, 0f, this.MinEventContext.Other, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
			if (value != 0f)
			{
				int num5 = (int)((float)num4 * value);
				if (num5 + this.MinEventContext.Other.Health <= 0)
				{
					num5 = (this.MinEventContext.Other.Health - 1) * -1;
				}
				this.MinEventContext.Other.AddHealth(num5);
				if (num5 < 0 && this.MinEventContext.Other is EntityPlayerLocal)
				{
					((EntityPlayerLocal)this.MinEventContext.Other).ForceBloodSplatter();
				}
			}
		}
		if (damageResponse.Source.BuffClass == null)
		{
			this.MinEventContext.DamageResponse = damageResponse;
			this.FireEvent(MinEventTypes.onOtherAttackedSelf, true);
		}
		this.ProcessDamageResponseLocal(damageResponse);
		return damageResponse;
	}

	public virtual bool IsImmuneToLegDamage
	{
		get
		{
			EntityClass entityClass = EntityClass.list[this.entityClass];
			return this.GetWalkType() == 21 || !this.bodyDamage.HasLeftLeg || !this.bodyDamage.HasRightLeg || (entityClass.LowerLegDismemberThreshold <= 0f && entityClass.UpperLegDismemberThreshold <= 0f);
		}
	}

	public override void ProcessDamageResponse(DamageResponse _dmResponse)
	{
		if (Time.time - this.lastAliveTime < 1f)
		{
			return;
		}
		base.ProcessDamageResponse(_dmResponse);
		this.ProcessDamageResponseLocal(_dmResponse);
		if (!this.world.IsRemote())
		{
			Entity entity = this.world.GetEntity(_dmResponse.Source.getEntityId());
			if (entity && !entity.isEntityRemote && this.isEntityRemote && this is EntityPlayer)
			{
				this.world.entityDistributer.SendPacketToTrackedPlayers(this.entityId, this.entityId, NetPackageManager.GetPackage<NetPackageDamageEntity>().Setup(this.entityId, _dmResponse), false);
				return;
			}
			if (_dmResponse.Source.BuffClass != null)
			{
				this.world.entityDistributer.SendPacketToTrackedPlayers(this.entityId, this.entityId, NetPackageManager.GetPackage<NetPackageDamageEntity>().Setup(this.entityId, _dmResponse), false);
				return;
			}
			this.world.entityDistributer.SendPacketToTrackedPlayersAndTrackedEntity(this.entityId, _dmResponse.Source.getEntityId(), NetPackageManager.GetPackage<NetPackageDamageEntity>().Setup(this.entityId, _dmResponse), false);
		}
	}

	public virtual void ProcessDamageResponseLocal(DamageResponse _dmResponse)
	{
		if (this.emodel == null)
		{
			return;
		}
		if (_dmResponse.Source.BonusDamageType != EnumDamageBonusType.None)
		{
			EntityPlayerLocal primaryPlayer = this.world.GetPrimaryPlayer();
			if (primaryPlayer && primaryPlayer.entityId == _dmResponse.Source.getEntityId())
			{
				EnumDamageBonusType bonusDamageType = _dmResponse.Source.BonusDamageType;
				if (bonusDamageType != EnumDamageBonusType.Sneak)
				{
					if (bonusDamageType == EnumDamageBonusType.Stun)
					{
						primaryPlayer.NotifyDamageMultiplier(_dmResponse.Source.DamageMultiplier);
					}
				}
				else
				{
					primaryPlayer.NotifySneakDamage(_dmResponse.Source.DamageMultiplier);
				}
			}
		}
		EntityAlive entityAlive = this.world.GetEntity(_dmResponse.Source.getEntityId()) as EntityAlive;
		if (entityAlive != null)
		{
			entityAlive.SetDamagedTarget(this);
		}
		if (this.IsSleeperPassive)
		{
			this.world.CheckSleeperVolumeNoise(this.position);
		}
		this.ConditionalTriggerSleeperWakeUp();
		this.SleeperSupressLivingSounds = false;
		this.bPlayHurtSound = false;
		if (this.AttachedToEntity != null && !_dmResponse.Source.bIsDamageTransfer && _dmResponse.Source.GetSource() != EnumDamageSource.Internal)
		{
			_dmResponse.Source.bIsDamageTransfer = true;
			this.AttachedToEntity.DamageEntity(_dmResponse.Source, _dmResponse.Strength, _dmResponse.Critical, _dmResponse.ImpulseScale);
			return;
		}
		if (this.equipment != null && _dmResponse.ArmorDamage > 0)
		{
			List<ItemValue> armor = this.equipment.GetArmor();
			if (armor.Count > 0)
			{
				float num = (float)_dmResponse.ArmorDamage / (float)armor.Count;
				if (num < 1f && num != 0f)
				{
					num = 1f;
				}
				for (int i = 0; i < armor.Count; i++)
				{
					armor[i].UseTimes += EffectManager.GetValue(PassiveEffects.DegradationPerUse, armor[i], num, this, null, armor[i].ItemClass.ItemTags, true, true, true, true, true, 1, true, false);
				}
			}
		}
		this.ApplyLocalBodyDamage(_dmResponse);
		this.lastHitRanged = false;
		bool flag = EffectManager.GetValue(PassiveEffects.NegateDamageSelf, null, 0f, this, null, FastTags<TagGroup.Global>.Parse(_dmResponse.HitBodyPart.ToString()), true, true, true, true, true, 1, true, false) > 0f || EffectManager.GetValue(PassiveEffects.NegateDamageOther, (entityAlive != null) ? entityAlive.inventory.holdingItemItemValue : null, 0f, entityAlive, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false) > 0f;
		if (_dmResponse.Dismember && !flag)
		{
			this.lastHitImpactDir = _dmResponse.Source.getDirection();
			if (entityAlive != null)
			{
				this.lastHitEntityFwd = entityAlive.GetForwardVector();
			}
			if (_dmResponse.Source.ItemClass != null && _dmResponse.Source.ItemClass.HasAnyTags(DismembermentManager.rangedTags))
			{
				this.lastHitRanged = true;
			}
			if (_dmResponse.Source.ItemClass != null)
			{
				float strength = (float)_dmResponse.ModStrength / (float)this.GetMaxHealth();
				this.lastHitForce = DismembermentManager.GetImpactForce(_dmResponse.Source.ItemClass, strength);
			}
			this.ExecuteDismember(false);
		}
		bool flag2 = _dmResponse.Stun > EnumEntityStunType.None;
		bool flag3 = this.bodyDamage.CurrentStun > EnumEntityStunType.None;
		if (!flag && _dmResponse.Fatal && this.isEntityRemote)
		{
			this.ClientKill(_dmResponse);
		}
		else if (this.emodel.avatarController != null && flag2)
		{
			if (_dmResponse.Stun == EnumEntityStunType.Prone)
			{
				if (this.bodyDamage.CurrentStun == EnumEntityStunType.None)
				{
					if ((_dmResponse.Critical && _dmResponse.Source.damageType == EnumDamageTypes.Bashing) || this.rand.RandomFloat < 0.6f)
					{
						this.DoRagdoll(_dmResponse);
					}
					else
					{
						this.emodel.avatarController.BeginStun(EnumEntityStunType.Prone, _dmResponse.HitBodyPart, _dmResponse.HitDirection, _dmResponse.Critical, _dmResponse.Random);
					}
					this.SetStun(EnumEntityStunType.Prone);
					this.bodyDamage.StunDuration = _dmResponse.StunDuration;
				}
				else if (this.bodyDamage.CurrentStun != EnumEntityStunType.Prone)
				{
					this.DoRagdoll(_dmResponse);
					this.SetStun(EnumEntityStunType.Prone);
					this.bodyDamage.StunDuration = _dmResponse.StunDuration * 0.5f;
				}
			}
			else if (_dmResponse.Stun == EnumEntityStunType.Kneel)
			{
				bool flag4 = false;
				if (this.bodyDamage.CurrentStun == EnumEntityStunType.None)
				{
					if (_dmResponse.Critical || this.rand.RandomFloat < 0.25f)
					{
						flag4 = true;
					}
					else
					{
						this.SetStun(EnumEntityStunType.Kneel);
						this.emodel.avatarController.BeginStun(EnumEntityStunType.Kneel, _dmResponse.HitBodyPart, _dmResponse.HitDirection, _dmResponse.Critical, _dmResponse.Random);
					}
				}
				else if (this.bodyDamage.CurrentStun == EnumEntityStunType.Kneel)
				{
					flag4 = true;
				}
				if (flag4)
				{
					this.DoRagdoll(_dmResponse);
					this.SetStun(EnumEntityStunType.Prone);
				}
				this.bodyDamage.StunDuration = _dmResponse.StunDuration;
			}
		}
		else if (this.emodel.avatarController != null && _dmResponse.PainHit && !flag3)
		{
			float painResistPerHit = EntityClass.list[this.entityClass].PainResistPerHit;
			if (painResistPerHit >= 0f)
			{
				this.painResistPercent = Utils.FastMin(this.painResistPercent + painResistPerHit, 3f);
				float duration = (this.painResistPercent >= 1f) ? Mathf.Lerp(0.6f, 0.15f, (this.painResistPercent - 1f) * 0.5f) : float.MaxValue;
				this.emodel.avatarController.StartAnimationHit(_dmResponse.HitBodyPart, (int)_dmResponse.HitDirection, (int)((float)_dmResponse.Strength * 100f / (float)this.GetMaxHealth()), _dmResponse.Critical, _dmResponse.MovementState, _dmResponse.Random, duration);
			}
		}
		if (this.bodyDamage.CurrentStun == EnumEntityStunType.None)
		{
			if (_dmResponse.Source.CanStun)
			{
				if ((_dmResponse.HitBodyPart & (EnumBodyPartHit.Torso | EnumBodyPartHit.Head | EnumBodyPartHit.LeftUpperArm | EnumBodyPartHit.RightUpperArm | EnumBodyPartHit.LeftLowerArm | EnumBodyPartHit.RightLowerArm)) > EnumBodyPartHit.None)
				{
					this.bodyDamage.StunProne = this.bodyDamage.StunProne + _dmResponse.Strength;
				}
				else if (_dmResponse.HitBodyPart.IsLeg())
				{
					this.bodyDamage.StunKnee = this.bodyDamage.StunKnee + _dmResponse.Strength;
				}
			}
		}
		else
		{
			this.bodyDamage.StunProne = 0;
			this.bodyDamage.StunKnee = 0;
		}
		bool flag5 = this.Health <= 0;
		if (this.Health <= 0 && this.deathUpdateTime > 0)
		{
			this.DeathHealth -= _dmResponse.Strength;
		}
		int num2 = _dmResponse.Strength;
		if (EffectManager.GetValue(PassiveEffects.HeadShotOnly, null, 0f, GameManager.Instance.World.GetEntity(_dmResponse.Source.getEntityId()) as EntityAlive, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false) > 0f && (_dmResponse.HitBodyPart & EnumBodyPartHit.Head) == EnumBodyPartHit.None)
		{
			num2 = 0;
			_dmResponse.Fatal = false;
		}
		if (flag)
		{
			num2 = 0;
			_dmResponse.Fatal = false;
		}
		if (this.isEntityRemote)
		{
			this.Health -= num2;
			this.RecordedDamage = _dmResponse;
		}
		else
		{
			if (!this.IsGodMode.Value)
			{
				this.Health -= num2;
				if (_dmResponse.Fatal && this.Health > 0)
				{
					this.Health = 0;
				}
				this.hasBeenAttackedTime = 0;
				if (_dmResponse.PainHit)
				{
					this.hasBeenAttackedTime = this.GetMaxAttackTime();
				}
			}
			this.bPlayHurtSound = (this.bBeenWounded = (num2 > 0));
			if (this.bBeenWounded)
			{
				base.setBeenAttacked();
				this.MinEventContext.Other = (GameManager.Instance.World.GetEntity(_dmResponse.Source.getEntityId()) as EntityAlive);
				this.FireEvent(MinEventTypes.onOtherDamagedSelf, true);
			}
			if (num2 > this.woundedStrength)
			{
				this.woundedStrength = _dmResponse.Strength;
				this.woundedDamageSource = _dmResponse.Source;
			}
			this.lastHitDirection = _dmResponse.HitDirection;
			if (this.Health <= 0)
			{
				_dmResponse.Source.getDirection();
				_dmResponse.Strength += this.Health;
				Entity entity = (_dmResponse.Source.getEntityId() != -1) ? this.world.GetEntity(_dmResponse.Source.getEntityId()) : null;
				if (this.Spawned && !flag5)
				{
					if (entity is EntityAlive)
					{
						this.entityThatKilledMe = (EntityAlive)entity;
					}
					else
					{
						this.entityThatKilledMe = null;
					}
				}
				this.Kill(_dmResponse);
				if (!_dmResponse.Fatal && this.world.IsRemote())
				{
					this.DamageEntity(DamageSource.disease, 1, false, 1f);
				}
			}
		}
		Entity entity2 = (_dmResponse.Source.getEntityId() != -1) ? this.world.GetEntity(_dmResponse.Source.getEntityId()) : null;
		if (entity2 != null && entity2 != this)
		{
			if (entity2 is EntityAlive && !this.isEntityRemote && !entity2.IsIgnoredByAI())
			{
				this.SetRevengeTarget((EntityAlive)entity2);
				if (this.aiManager != null)
				{
					this.aiManager.DamagedByEntity();
				}
			}
			if (entity2 is EntityPlayer)
			{
				((EntityPlayer)entity2).FireEvent(MinEventTypes.onCombatEntered, true);
			}
			this.FireEvent(MinEventTypes.onCombatEntered, true);
		}
		if (_dmResponse.Strength > 0 && _dmResponse.Source.GetDamageType() == EnumDamageTypes.Electrical)
		{
			this.Electrocuted = true;
		}
		this.RecordedDamage = _dmResponse;
	}

	public EntityAlive GetRevengeTarget()
	{
		return this.revengeEntity;
	}

	public void SetRevengeTarget(EntityAlive _other)
	{
		this.revengeEntity = _other;
		this.revengeTimer = ((this.revengeEntity == null) ? 0 : 500);
	}

	public void SetRevengeTimer(int ticks)
	{
		this.revengeTimer = ticks;
	}

	public override bool CanBePushed()
	{
		return !this.IsDead();
	}

	public override bool CanCollideWith(Entity _other)
	{
		return !this.IsDead() && !(_other is EntityItem) && !(_other is EntitySupplyCrate);
	}

	public override bool CanCollideWithBlocks()
	{
		return !this.IsSleeping;
	}

	public void DoRagdoll(DamageResponse _dmResponse)
	{
		this.emodel.DoRagdoll(_dmResponse, _dmResponse.StunDuration);
	}

	public void AddScore(int _diedMySelfTimes, int _zombieKills, int _playerKills, int _otherTeamnumber, int _conditions)
	{
		this.KilledZombies += _zombieKills;
		this.KilledPlayers += _playerKills;
		this.Died += _diedMySelfTimes;
		this.Score += _zombieKills * GameStats.GetInt(EnumGameStats.ScoreZombieKillMultiplier) + _playerKills * GameStats.GetInt(EnumGameStats.ScorePlayerKillMultiplier) + _diedMySelfTimes * GameStats.GetInt(EnumGameStats.ScoreDiedMultiplier);
		if (this.Score < 0)
		{
			this.Score = 0;
		}
		if (this is EntityPlayerLocal)
		{
			if (_diedMySelfTimes > 0)
			{
				IAchievementManager achievementManager = PlatformManager.NativePlatform.AchievementManager;
				if (achievementManager != null)
				{
					achievementManager.SetAchievementStat(EnumAchievementDataStat.Deaths, _diedMySelfTimes);
				}
			}
			if (_zombieKills > 0)
			{
				IAchievementManager achievementManager2 = PlatformManager.NativePlatform.AchievementManager;
				if (achievementManager2 != null)
				{
					achievementManager2.SetAchievementStat(EnumAchievementDataStat.ZombiesKilled, _zombieKills);
				}
			}
			if (_playerKills > 0)
			{
				IAchievementManager achievementManager3 = PlatformManager.NativePlatform.AchievementManager;
				if (achievementManager3 != null)
				{
					achievementManager3.SetAchievementStat(EnumAchievementDataStat.PlayersKilled, _playerKills);
				}
			}
			if ((_conditions & 2) != 0)
			{
				IAchievementManager achievementManager4 = PlatformManager.NativePlatform.AchievementManager;
				if (achievementManager4 == null)
				{
					return;
				}
				achievementManager4.SetAchievementStat(EnumAchievementDataStat.KilledWith44Magnum, 1);
			}
		}
	}

	public virtual void AwardKill(EntityAlive killer)
	{
		if (killer != null && killer != this)
		{
			int num = 0;
			int num2 = 0;
			int conditions = 0;
			EntityType entityType = this.entityType;
			if (entityType != EntityType.Player)
			{
				if (entityType == EntityType.Zombie)
				{
					num++;
				}
			}
			else
			{
				num2++;
			}
			EntityPlayer entityPlayer = killer as EntityPlayer;
			if (entityPlayer)
			{
				GameManager.Instance.AwardKill(killer, this);
				if (entityPlayer.inventory.IsHoldingGun() && entityPlayer.inventory.holdingItem.Name.Equals("gunHandgunT2Magnum44"))
				{
					conditions = 2;
				}
				GameManager.Instance.AddScoreServer(killer.entityId, num, num2, this.TeamNumber, conditions);
			}
		}
	}

	public virtual void OnEntityDeath()
	{
		if (this.deathUpdateTime != 0)
		{
			return;
		}
		this.AddScore(1, 0, 0, -1, 0);
		if (this.soundLiving != null && this.soundLivingID >= 0)
		{
			Manager.Stop(this.entityId, this.soundLiving);
			this.soundLivingID = -1;
		}
		if (this.AttachedToEntity)
		{
			this.Detach();
		}
		if (this.isEntityRemote)
		{
			return;
		}
		this.AwardKill(this.entityThatKilledMe);
		if (this.particleOnDeath != null && this.particleOnDeath.Length > 0)
		{
			float lightBrightness = this.world.GetLightBrightness(base.GetBlockPosition());
			this.world.GetGameManager().SpawnParticleEffectServer(new ParticleEffect(this.particleOnDeath, this.getHeadPosition(), lightBrightness, Color.white, null, null, false), this.entityId, false, false);
		}
		if (this.isGameMessageOnDeath())
		{
			GameManager.Instance.GameMessage(EnumGameMessages.EntityWasKilled, this, this.entityThatKilledMe);
		}
		if (this.entityThatKilledMe != null)
		{
			Log.Out("Entity {0} {1} killed by {2} {3}", new object[]
			{
				base.GetDebugName(),
				this.entityId,
				this.entityThatKilledMe.GetDebugName(),
				this.entityThatKilledMe.entityId
			});
		}
		else
		{
			Log.Out("Entity {0} {1} killed", new object[]
			{
				base.GetDebugName(),
				this.entityId
			});
		}
		ModEvents.EntityKilled.Invoke(this, this.entityThatKilledMe);
		this.dropItemOnDeath();
		this.entityThatKilledMe = null;
	}

	public virtual void PlayGiveUpSound()
	{
		if (this.soundGiveUp != null)
		{
			this.PlayOneShot(this.soundGiveUp, false, false, false);
		}
	}

	public virtual Vector3 GetCameraLook(float _t)
	{
		return this.GetLookVector();
	}

	public Vector3 GetForwardVector()
	{
		float num = Mathf.Cos(this.rotation.y * 0.0175f - 3.14159274f);
		float num2 = Mathf.Sin(this.rotation.y * 0.0175f - 3.14159274f);
		float num3 = -Mathf.Cos(0f);
		float y = Mathf.Sin(0f);
		return new Vector3(num2 * num3, y, num * num3);
	}

	public Vector2 GetForwardVector2()
	{
		float f = this.rotation.y * 0.0174532924f;
		float y = Mathf.Cos(f);
		return new Vector2(Mathf.Sin(f), y);
	}

	public virtual Vector3 GetLookVector()
	{
		float num = Mathf.Cos(this.rotation.y * 0.0175f - 3.14159274f);
		float num2 = Mathf.Sin(this.rotation.y * 0.0175f - 3.14159274f);
		float num3 = -Mathf.Cos(this.rotation.x * 0.0175f);
		float y = Mathf.Sin(this.rotation.x * 0.0175f);
		return new Vector3(num2 * num3, y, num * num3);
	}

	public virtual Vector3 GetLookVector(Vector3 _altLookVector)
	{
		return this.GetLookVector();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public int GetSoundRandomTicks()
	{
		return this.rand.RandomRange(this.soundRandomTicks / 2, this.soundRandomTicks);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public int GetSoundAlertTicks()
	{
		return this.rand.RandomRange(this.soundAlertTicks / 2, this.soundAlertTicks);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public string GetSoundRandom()
	{
		return this.soundRandom;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual string GetSoundJump()
	{
		return this.soundJump;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual string GetSoundHurt(DamageSource _damageSource, int _damageStrength)
	{
		return this.soundHurt;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public string GetSoundHurtSmall()
	{
		return this.soundHurtSmall;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public string GetSoundHurt()
	{
		return this.soundHurt;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public string GetSoundDrownPain()
	{
		return this.soundDrownPain;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual string GetSoundDeath(DamageSource _damageSource)
	{
		return this.soundDeath;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual string GetSoundAttack()
	{
		return this.soundAttack;
	}

	public virtual string GetSoundAlert()
	{
		return this.soundAlert;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual string GetSoundStamina()
	{
		return this.soundStamina;
	}

	public virtual Ray GetLookRay()
	{
		return new Ray(this.position + new Vector3(0f, this.GetEyeHeight(), 0f), this.GetLookVector());
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void dropItemOnDeath()
	{
		for (int i = 0; i < this.inventory.GetItemCount(); i++)
		{
			ItemStack item = this.inventory.GetItem(i);
			ItemClass forId = ItemClass.GetForId(item.itemValue.type);
			if (forId != null && forId.CanDrop(null))
			{
				this.world.GetGameManager().ItemDropServer(item, this.position, new Vector3(0.5f, 0f, 0.5f), -1, Constants.cItemDroppedOnDeathLifetime, false);
				this.inventory.SetItem(i, ItemValue.None.Clone(), 0, true);
			}
		}
		this.inventory.SetFlashlight(false);
		this.equipment.DropItems();
		if (this.world.IsDark())
		{
			this.lootDropProb *= 1f;
		}
		if (this.entityThatKilledMe)
		{
			this.lootDropProb = EffectManager.GetValue(PassiveEffects.LootDropProb, this.entityThatKilledMe.inventory.holdingItemItemValue, this.lootDropProb, this.entityThatKilledMe, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
		}
		if (this.lootDropProb > this.rand.RandomFloat)
		{
			GameManager.Instance.DropContentOfLootContainerServer(BlockValue.Air, new Vector3i(this.position), this.entityId, null);
		}
	}

	public virtual Vector3 GetDropPosition()
	{
		if (this.ParachuteWearing || this.JetpackWearing)
		{
			return base.transform.position + base.transform.forward - Vector3.up * 0.3f + Origin.position;
		}
		return base.transform.position + base.transform.forward + Vector3.up + Origin.position;
	}

	public virtual void OnFired()
	{
		if (this.emodel.avatarController != null)
		{
			this.emodel.avatarController.StartAnimationFiring();
		}
	}

	public virtual void OnReloadStart()
	{
		if (this.emodel.avatarController != null)
		{
			this.emodel.avatarController.StartAnimationReloading();
		}
	}

	public virtual void OnReloadEnd()
	{
	}

	public virtual bool WillForceToFollow(EntityAlive _other)
	{
		return false;
	}

	public void AddHealth(int _v)
	{
		if (this.Health <= 0)
		{
			return;
		}
		this.Health += _v;
	}

	public void AddStamina(float _v)
	{
		if (this.Health <= 0)
		{
			return;
		}
		this.Stats.Stamina.Value += _v;
	}

	public void AddWater(float _v)
	{
		this.Stats.Water.Value += _v;
	}

	public int GetTicksNoPlayerAdjacent()
	{
		return this.ticksNoPlayerAdjacent;
	}

	public bool CanSee(EntityAlive _other)
	{
		return this.seeCache.CanSee(_other);
	}

	public void SetCanSee(EntityAlive _other)
	{
		this.seeCache.SetCanSee(_other);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void updateTasks()
	{
		if (GamePrefs.GetBool(EnumGamePrefs.DebugStopEnemiesMoving))
		{
			this.SetMoveForwardWithModifiers(0f, 0f, false);
			if (this.aiManager != null)
			{
				this.aiManager.UpdateDebugName();
			}
			return;
		}
		this.CheckDespawn();
		this.seeCache.ClearIfExpired();
		bool useAIPackages = EntityClass.list[this.entityClass].UseAIPackages;
		this.aiActiveDelay -= this.aiActiveScale;
		if (this.aiActiveDelay <= 0f)
		{
			this.aiActiveDelay = 1f;
			if (!useAIPackages)
			{
				this.aiManager.Update();
			}
			else
			{
				UAIBase.Update(this.utilityAIContext);
			}
		}
		PathInfo path = PathFinderThread.Instance.GetPath(this.entityId);
		if (path.path != null)
		{
			bool flag = true;
			if (!useAIPackages)
			{
				flag = this.aiManager.CheckPath(path);
			}
			if (flag)
			{
				this.navigator.SetPath(path, path.speed);
			}
		}
		this.navigator.UpdateNavigation();
		this.moveHelper.UpdateMoveHelper();
		this.lookHelper.onUpdateLook();
		if (this.distraction != null && (this.distraction.IsDead() || this.distraction.IsMarkedForUnload()))
		{
			this.distraction = null;
		}
		if (this.pendingDistraction != null && (this.pendingDistraction.IsDead() || this.pendingDistraction.IsMarkedForUnload()))
		{
			this.pendingDistraction = null;
		}
	}

	public PathNavigate getNavigator()
	{
		return this.navigator;
	}

	public void FindPath(Vector3 targetPos, float moveSpeed, bool canBreak, EAIBase behavior)
	{
		Vector3 vector = targetPos - this.position;
		if (vector.x * vector.x + vector.z * vector.z > 1225f)
		{
			if (vector.y > 45f)
			{
				targetPos.y = this.position.y + 45f;
			}
			else if (vector.y < -45f)
			{
				targetPos.y = this.position.y - 45f;
			}
		}
		PathFinderThread.Instance.FindPath(this, targetPos, moveSpeed, canBreak, behavior);
	}

	public bool isWithinHomeDistanceCurrentPosition()
	{
		return this.isWithinHomeDistance(Utils.Fastfloor(this.position.x), Utils.Fastfloor(this.position.y), Utils.Fastfloor(this.position.z));
	}

	public bool isWithinHomeDistance(int _x, int _y, int _z)
	{
		return this.maximumHomeDistance < 0 || this.homePosition.getDistanceSquared(_x, _y, _z) < (float)(this.maximumHomeDistance * this.maximumHomeDistance);
	}

	public void setHomeArea(Vector3i _pos, int _maxDistance)
	{
		this.homePosition.position = _pos;
		this.maximumHomeDistance = _maxDistance;
	}

	public ChunkCoordinates getHomePosition()
	{
		return this.homePosition;
	}

	public int getMaximumHomeDistance()
	{
		return this.maximumHomeDistance;
	}

	public void detachHome()
	{
		this.maximumHomeDistance = -1;
	}

	public bool hasHome()
	{
		return this.maximumHomeDistance >= 0;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool canDespawn()
	{
		return !this.IsClientControlled() && base.GetSpawnerSource() != EnumSpawnerSource.StaticSpawner && !this.IsSleeping;
	}

	public void ResetDespawnTime()
	{
		this.ticksNoPlayerAdjacent = 0;
		this.seeCache.SetLastTimePlayerSeen();
	}

	public void CheckDespawn()
	{
		if (this.isEntityRemote)
		{
			return;
		}
		if (!this.CanUpdateEntity() && this.bIsChunkObserver && this.world.GetClosestPlayer(this, -1f, false) == null)
		{
			this.MarkToUnload();
			return;
		}
		if (!this.canDespawn())
		{
			return;
		}
		int num = this.despawnDelayCounter + 1;
		this.despawnDelayCounter = num;
		if (num < 20)
		{
			return;
		}
		this.despawnDelayCounter = 0;
		this.ticksNoPlayerAdjacent += 20;
		EnumSpawnerSource spawnerSource = base.GetSpawnerSource();
		EntityPlayer closestPlayer = this.world.GetClosestPlayer(this, -1f, false);
		if (spawnerSource == EnumSpawnerSource.Dynamic)
		{
			if (!closestPlayer)
			{
				if (!this.world.GetClosestPlayer(this, -1f, true))
				{
					this.Despawn();
				}
				return;
			}
		}
		else if (spawnerSource == EnumSpawnerSource.Biome && !this.world.GetClosestPlayer(this, 130f, false))
		{
			if (this.world.GetClosestPlayer(this, 20f, true))
			{
				this.isDespawnWhenPlayerFar = true;
			}
			else if (this.isDespawnWhenPlayerFar)
			{
				this.Despawn();
			}
		}
		if (!closestPlayer)
		{
			return;
		}
		float sqrMagnitude = (closestPlayer.position - this.position).sqrMagnitude;
		if (sqrMagnitude < 6400f)
		{
			this.ticksNoPlayerAdjacent = 0;
		}
		int num2 = int.MaxValue;
		float lastTimePlayerSeen = this.seeCache.GetLastTimePlayerSeen();
		if (lastTimePlayerSeen > 0f)
		{
			num2 = (int)(Time.time - lastTimePlayerSeen);
		}
		switch (spawnerSource)
		{
		case EnumSpawnerSource.Biome:
			if (this.ticksNoPlayerAdjacent > 100 && sqrMagnitude > 16384f)
			{
				this.Despawn();
				return;
			}
			if (this.ticksNoPlayerAdjacent > 1800)
			{
				this.Despawn();
				return;
			}
			break;
		case EnumSpawnerSource.StaticSpawner:
			break;
		case EnumSpawnerSource.Dynamic:
			if (this.attackTarget)
			{
				num2 = 0;
			}
			if (this.IsSleeper && !this.IsSleeping)
			{
				if (sqrMagnitude > 9216f && num2 > 80)
				{
					this.Despawn();
					return;
				}
			}
			else
			{
				if (sqrMagnitude > 2304f && num2 > 60 && !this.HasInvestigatePosition)
				{
					this.Despawn();
					return;
				}
				if (this.ticksNoPlayerAdjacent > 1800)
				{
					this.Despawn();
					return;
				}
			}
			break;
		default:
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Despawn()
	{
		this.IsDespawned = true;
		this.MarkToUnload();
	}

	public void ForceDespawn()
	{
		this.Despawn();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public EntityAlive GetAttackTarget()
	{
		return this.attackTarget;
	}

	public virtual Vector3 GetAttackTargetHitPosition()
	{
		return this.attackTarget.getChestPosition();
	}

	public EntityAlive GetAttackTargetLocal()
	{
		if (this.isEntityRemote)
		{
			return this.attackTargetClient;
		}
		return this.attackTarget;
	}

	public void SetAttackTarget(EntityAlive _attackTarget, int _attackTargetTime)
	{
		if (_attackTarget == this.attackTarget)
		{
			this.attackTargetTime = _attackTargetTime;
			return;
		}
		if (this.attackTarget)
		{
			this.attackTargetLast = this.attackTarget;
		}
		this.targetAlertChanged = false;
		if (_attackTarget)
		{
			if (_attackTarget != this.attackTargetLast)
			{
				this.targetAlertChanged = true;
				this.soundDelayTicks = this.rand.RandomRange(5, 20);
			}
			this.investigatePositionTicks = 0;
		}
		if (!this.isEntityRemote)
		{
			this.world.entityDistributer.SendPacketToTrackedPlayersAndTrackedEntity(this.entityId, -1, NetPackageManager.GetPackage<NetPackageSetAttackTarget>().Setup(this.entityId, _attackTarget ? _attackTarget.entityId : -1), false);
		}
		this.attackTarget = _attackTarget;
		this.attackTargetTime = _attackTargetTime;
	}

	public void SetAttackTargetClient(EntityAlive _attackTarget)
	{
		this.attackTargetClient = _attackTarget;
	}

	public bool HasInvestigatePosition
	{
		get
		{
			return this.investigatePositionTicks > 0;
		}
	}

	public Vector3 InvestigatePosition
	{
		get
		{
			return this.investigatePos;
		}
	}

	public int GetInvestigatePositionTicks()
	{
		return this.investigatePositionTicks;
	}

	public void ClearInvestigatePosition()
	{
		this.investigatePos = Vector3.zero;
		this.investigatePositionTicks = 0;
		this.ResetDespawnTime();
		int num = this.rand.RandomRange(12, 25) * 20;
		if (this.entityType == EntityType.Zombie)
		{
			num /= 2;
		}
		this.SetAlertTicks(num);
	}

	public int CalcInvestigateTicks(int _ticks, EntityAlive _investigateEntity)
	{
		float value = EffectManager.GetValue(PassiveEffects.EnemySearchDuration, null, 1f, _investigateEntity, null, EntityClass.list[this.entityClass].Tags, true, true, true, true, true, 1, true, false);
		return (int)((float)_ticks / value);
	}

	public void SetInvestigatePosition(Vector3 pos, int ticks, bool isAlert = true)
	{
		this.investigatePos = pos;
		this.investigatePositionTicks = ticks;
		this.isInvestigateAlert = isAlert;
	}

	public int GetAlertTicks()
	{
		return this.alertTicks;
	}

	public void SetAlertTicks(int ticks)
	{
		this.alertTicks = ticks;
	}

	public virtual bool IsAlert
	{
		get
		{
			if (this.isEntityRemote)
			{
				return this.bReplicatedAlertFlag;
			}
			return this.isAlert;
		}
	}

	public Vector3 LastTargetPos
	{
		get
		{
			return this.lastTargetPos;
		}
		set
		{
			this.lastTargetPos = value;
		}
	}

	public EntitySeeCache GetEntitySenses()
	{
		return this.seeCache;
	}

	public virtual bool IsRunning
	{
		get
		{
			return this.IsBloodMoon || this.world.IsDark();
		}
	}

	public virtual float GetMoveSpeed()
	{
		if (this.IsBloodMoon || this.world.IsDark())
		{
			return EffectManager.GetValue(PassiveEffects.WalkSpeed, null, this.moveSpeedNight, this, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
		}
		return EffectManager.GetValue(PassiveEffects.CrouchSpeed, null, this.moveSpeed, this, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
	}

	public virtual float GetMoveSpeedAggro()
	{
		if (this.IsBloodMoon || this.world.IsDark())
		{
			return EffectManager.GetValue(PassiveEffects.RunSpeed, null, this.moveSpeedAggroMax, this, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
		}
		return EffectManager.GetValue(PassiveEffects.WalkSpeed, null, this.moveSpeedAggro, this, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
	}

	public float GetMoveSpeedPanic()
	{
		return EffectManager.GetValue(PassiveEffects.RunSpeed, null, this.moveSpeedPanic, this, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
	}

	public override float GetWeight()
	{
		return this.weight;
	}

	public override float GetPushFactor()
	{
		return this.pushFactor;
	}

	public virtual bool CanEntityJump()
	{
		return true;
	}

	public void SetMaxViewAngle(float _angle)
	{
		this.maxViewAngle = _angle;
	}

	public virtual float GetMaxViewAngle()
	{
		return this.maxViewAngle;
	}

	public void SetSightLightThreshold(Vector2 _threshold)
	{
		this.sightLightThreshold = _threshold;
	}

	public int GetModelLayer()
	{
		return this.emodel.GetModelTransform().gameObject.layer;
	}

	public virtual void SetModelLayer(int _layerId, bool force = false, string[] excludeTags = null)
	{
		Utils.SetLayerRecursively(this.emodel.GetModelTransform().gameObject, _layerId, null);
	}

	public virtual void SetColliderLayer(int _layerId, bool _force = false)
	{
		Utils.SetColliderLayerRecursively(this.emodel.GetModelTransform().gameObject, _layerId);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual int GetMaxAttackTime()
	{
		return 10;
	}

	public int GetAttackTimeoutTicks()
	{
		if (!this.world.IsDark())
		{
			return this.attackTimeoutDay;
		}
		return this.attackTimeoutNight;
	}

	public override string GetLootList()
	{
		if (!string.IsNullOrEmpty(this.lootListOnDeath) && this.IsDead())
		{
			return this.lootListOnDeath;
		}
		return base.GetLootList();
	}

	public override void MarkToUnload()
	{
		base.MarkToUnload();
		this.deathUpdateTime = this.timeStayAfterDeath;
	}

	public override bool IsMarkedForUnload()
	{
		return base.IsMarkedForUnload() && this.deathUpdateTime >= this.timeStayAfterDeath;
	}

	public virtual bool IsAttackValid()
	{
		if (!(this is EntityPlayer))
		{
			if (this.Electrocuted)
			{
				return false;
			}
			if (this.bodyDamage.CurrentStun == EnumEntityStunType.Kneel || this.bodyDamage.CurrentStun == EnumEntityStunType.Prone)
			{
				return false;
			}
		}
		return (!(this.emodel != null) || !(this.emodel.avatarController != null) || !this.emodel.avatarController.IsAttackPrevented()) && !this.IsDead() && (this.painResistPercent >= 1f || (this.hasBeenAttackedTime <= 0 && (this.emodel.avatarController == null || !this.emodel.avatarController.IsAnimationHitRunning())));
	}

	public virtual bool IsAttackImpact()
	{
		return this.emodel && this.emodel.avatarController && this.emodel.avatarController.IsAttackImpact();
	}

	public virtual bool Attack(bool _bAttackReleased)
	{
		if (!_bAttackReleased)
		{
			if (this.emodel && this.emodel.avatarController && this.emodel.avatarController.IsAnimationAttackPlaying())
			{
				return false;
			}
			if (!this.IsAttackValid())
			{
				return false;
			}
		}
		if (this.bLastAttackReleased && this.GetSoundAttack() != null)
		{
			this.PlayOneShot(this.GetSoundAttack(), false, false, false);
		}
		this.bLastAttackReleased = _bAttackReleased;
		this.attackingTime = 60;
		ItemAction itemAction = this.inventory.holdingItem.Actions[0];
		if (itemAction != null)
		{
			itemAction.ExecuteAction(this.inventory.holdingItemData.actionData[0], _bAttackReleased);
		}
		return true;
	}

	public bool Use(bool _bUseReleased)
	{
		if (!_bUseReleased && !this.IsAttackValid())
		{
			return false;
		}
		this.attackingTime = 60;
		if (this.inventory.holdingItem.Actions[1] != null)
		{
			this.inventory.holdingItem.Actions[1].ExecuteAction(this.inventory.holdingItemData.actionData[1], _bUseReleased);
		}
		return true;
	}

	public Entity GetTargetIfAttackedNow()
	{
		if (!this.IsAttackValid())
		{
			return null;
		}
		ItemClass holdingItem = this.inventory.holdingItem;
		if (holdingItem != null)
		{
			int holdingItemIdx = this.inventory.holdingItemIdx;
			ItemAction itemAction = holdingItem.Actions[holdingItemIdx];
			if (itemAction != null)
			{
				WorldRayHitInfo executeActionTarget = itemAction.GetExecuteActionTarget(this.inventory.holdingItemData.actionData[holdingItemIdx]);
				if (executeActionTarget != null && executeActionTarget.bHitValid && executeActionTarget.transform)
				{
					float num = itemAction.Range;
					if (num == 0f)
					{
						ItemValue holdingItemItemValue = this.inventory.holdingItemItemValue;
						num = EffectManager.GetItemValue(PassiveEffects.MaxRange, holdingItemItemValue, 0f);
					}
					num += 0.3f;
					if (executeActionTarget.hit.distanceSq <= num * num)
					{
						Transform transform = executeActionTarget.transform;
						if (executeActionTarget.tag.StartsWith("E_BP_"))
						{
							transform = GameUtils.GetHitRootTransform(Voxel.voxelRayHitInfo.tag, executeActionTarget.transform);
						}
						if (transform != null)
						{
							Entity component = transform.GetComponent<Entity>();
							if (component)
							{
								return component;
							}
						}
						if (executeActionTarget.tag == "E_Vehicle")
						{
							return EntityVehicle.FindCollisionEntity(transform);
						}
					}
				}
			}
		}
		return null;
	}

	public virtual float GetBlockDamageScale()
	{
		EnumGamePrefs eProperty = EnumGamePrefs.BlockDamageAI;
		if (this.IsBloodMoon)
		{
			eProperty = EnumGamePrefs.BlockDamageAIBM;
		}
		return (float)GamePrefs.GetInt(eProperty) * 0.01f;
	}

	public virtual void PlayStepSound()
	{
		this.internalPlayStepSound();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void internalPlayStepSound()
	{
		if (this.blockValueStandingOn.isair)
		{
			return;
		}
		if ((!this.onGround && !base.IsInElevator()) || this.isHeadUnderwater)
		{
			if (!(this is EntityPlayerLocal) && (this.isHeadUnderwater || this.world.IsWater(this.blockPosStandingOn)))
			{
				Manager.Play(this, "player_swim", 1f, false);
			}
			return;
		}
		BlockValue blockValue = this.blockValueStandingOn;
		Vector3i vector3i = this.blockPosStandingOn;
		vector3i.y++;
		BlockValue blockValue2 = this.world.GetBlock(vector3i);
		if (blockValue2.Block.blockMaterial.stepSound != null)
		{
			blockValue = blockValue2;
		}
		else
		{
			BlockValue block;
			blockValue2 = (block = this.world.GetBlock(vector3i + Vector3i.right));
			if (!block.isair && blockValue2.Block.blockMaterial.stepSound != null)
			{
				blockValue = blockValue2;
			}
			else
			{
				blockValue2 = (block = this.world.GetBlock(vector3i - Vector3i.right));
				if (!block.isair && blockValue2.Block.blockMaterial.stepSound != null)
				{
					blockValue = blockValue2;
				}
				else
				{
					blockValue2 = (block = this.world.GetBlock(vector3i + Vector3i.forward));
					if (!block.isair && blockValue2.Block.blockMaterial.stepSound != null)
					{
						blockValue = blockValue2;
					}
					else
					{
						blockValue2 = (block = this.world.GetBlock(vector3i - Vector3i.forward));
						if (!block.isair && blockValue2.Block.blockMaterial.stepSound != null)
						{
							blockValue = blockValue2;
						}
					}
				}
			}
		}
		Block block2 = blockValue.Block;
		MaterialBlock materialForSide;
		string str;
		if (!blockValue.isair && (materialForSide = block2.GetMaterialForSide(blockValue, BlockFace.Top)) != null && materialForSide.stepSound != null && (str = materialForSide.stepSound.name).Length > 0)
		{
			if (WeatherManager.GetCurrentSnowValue() > 0.25f && this.GetLightBrightness() > 0.75f)
			{
				str = "snow";
			}
			if (EffectManager.GetValue(PassiveEffects.SilenceBlockSteps, null, 0f, this, null, block2.Tags, true, true, true, true, true, 1, true, false) <= 0f)
			{
				string stepSound = "step" + str;
				this.playStepSound(stepSound);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void updateStepSound(float _distX, float _distZ)
	{
		if (this.blockValueStandingOn.isair)
		{
			return;
		}
		if (!this.onGround || this.isHeadUnderwater)
		{
			this.distanceSwam += Mathf.Sqrt(_distX * _distX + _distZ * _distZ);
			if (this.distanceSwam > this.nextSwimDistance)
			{
				this.nextSwimDistance += 1f;
				if (this.nextSwimDistance < this.distanceSwam || this.nextSwimDistance > this.distanceSwam + 1f)
				{
					this.nextSwimDistance = this.distanceSwam + 1f;
				}
				this.internalPlayStepSound();
			}
			return;
		}
		float num = Mathf.Sqrt(_distX * _distX + _distZ * _distZ);
		this.distanceWalked += num;
		this.stepDistanceToPlaySound -= num;
		if (this.stepDistanceToPlaySound <= 0f)
		{
			this.stepDistanceToPlaySound = this.getNextStepSoundDistance();
			this.internalPlayStepSound();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void updatePlayerLandSound(float _distXZ, float _diffY)
	{
		if (this.blockValueStandingOn.isair)
		{
			return;
		}
		if (_distXZ >= 0.025f || Utils.FastAbs(_diffY) >= 0.015f)
		{
			float num = this.inWaterPercent * 2f;
			float x = num - this.landWaterLevel;
			this.landWaterLevel = num;
			float num2 = Utils.FastAbs(x);
			if (num > 0f)
			{
				num2 = Utils.FastMax(num2, _distXZ);
			}
			if (num2 >= 0.02f)
			{
				float volumeScale = Utils.FastMin(num2 * 2.2f + 0.01f, 1f);
				Manager.Play(this, "player_swim", volumeScale, false);
			}
		}
		if (this.isHeadUnderwater)
		{
			this.wasOnGround = true;
			return;
		}
		this.wasOnGround = this.onGround;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void updateCurrentBlockPosAndValue()
	{
		Vector3i vector3i = base.GetBlockPosition();
		BlockValue block = this.world.GetBlock(vector3i);
		if (block.isair)
		{
			vector3i.y--;
			block = this.world.GetBlock(vector3i);
		}
		if (block.ischild)
		{
			vector3i += block.parent;
			block = this.world.GetBlock(vector3i);
		}
		if (this.blockPosStandingOn != vector3i || !this.blockValueStandingOn.Equals(block) || (this.onGround && !this.wasOnGround))
		{
			this.blockPosStandingOn = vector3i;
			this.blockValueStandingOn = block;
			this.blockStandingOnChanged = !this.world.IsRemote();
			BiomeDefinition biome = this.world.GetBiome(this.blockPosStandingOn.x, this.blockPosStandingOn.z);
			if (this.biomeStandingOn != biome && (this.biomeStandingOn == null || biome == null || this.biomeStandingOn.m_Id != biome.m_Id))
			{
				WeatherManager.Instance.HandleBiomeChanging(this as EntityPlayer, this.biomeStandingOn, biome);
				this.onNewBiomeEntered(biome);
			}
		}
		this.CalcIfInElevator();
		Block block2 = this.blockValueStandingOn.Block;
		if (block2.BuffsWhenWalkedOn != null && block2.UseBuffsWhenWalkedOn(this.world, this.blockPosStandingOn, this.blockValueStandingOn))
		{
			bool flag = true;
			TileEntityWorkstation tileEntityWorkstation = this.world.GetTileEntity(0, this.blockPosStandingOn) as TileEntityWorkstation;
			if (tileEntityWorkstation != null)
			{
				flag = tileEntityWorkstation.IsBurning;
			}
			if (flag)
			{
				for (int i = 0; i < block2.BuffsWhenWalkedOn.Length; i++)
				{
					BuffValue buff = this.Buffs.GetBuff(block2.BuffsWhenWalkedOn[i]);
					if (buff == null || buff.DurationInSeconds >= 1f)
					{
						this.Buffs.AddBuff(block2.BuffsWhenWalkedOn[i], vector3i, -1, true, false, -1f);
					}
				}
			}
		}
		if (this.onGround && !this.IsFlyMode.Value)
		{
			if (block2.MovementFactor != 1f && block2.HasCollidingAABB(this.blockValueStandingOn, this.blockPosStandingOn.x, this.blockPosStandingOn.y, this.blockPosStandingOn.z, 0f, this.boundingBox))
			{
				this.SetMotionMultiplier(EffectManager.GetValue(PassiveEffects.MovementFactorMultiplier, null, block2.MovementFactor, this, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false));
			}
			if (this.blockStandingOnChanged)
			{
				this.blockStandingOnChanged = false;
				if (!this.blockValueStandingOn.isair)
				{
					block2.OnEntityWalking(this.world, this.blockPosStandingOn.x, this.blockPosStandingOn.y, this.blockPosStandingOn.z, this.blockValueStandingOn, this);
					if (GameManager.bPhysicsActive && !this.blockValueStandingOn.ischild && !this.blockValueStandingOn.Block.isOversized && this.world.GetStability(this.blockPosStandingOn) == 0 && Block.CanFallBelow(this.world, this.blockPosStandingOn.x, this.blockPosStandingOn.y, this.blockPosStandingOn.z))
					{
						Log.Warning("EntityAlive {0} AddFallingBlock stab 0 happens?", new object[]
						{
							this.EntityName
						});
						this.world.AddFallingBlock(this.blockPosStandingOn, false);
					}
				}
				BlockValue block3 = this.world.GetBlock(this.blockPosStandingOn + Vector3i.up);
				if (!block3.isair)
				{
					block3.Block.OnEntityWalking(this.world, this.blockPosStandingOn.x, this.blockPosStandingOn.y + 1, this.blockPosStandingOn.z, block3, this);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CalcIfInElevator()
	{
		ChunkCluster chunkCache = this.world.ChunkCache;
		Vector3i pos = new Vector3i(this.blockPosStandingOn.x, Utils.Fastfloor(this.boundingBox.min.y), this.blockPosStandingOn.z);
		BlockValue block = chunkCache.GetBlock(pos);
		Block block2 = block.Block;
		this.bInElevator = block2.IsElevator((int)block.rotation);
		pos.y++;
		block = chunkCache.GetBlock(pos);
		block2 = block.Block;
		this.bInElevator |= block2.IsElevator((int)block.rotation);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual float getNextStepSoundDistance()
	{
		return 1.5f;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void onNewBiomeEntered(BiomeDefinition _biome)
	{
		this.biomeStandingOn = _biome;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void updateSpeedForwardAndStrafe(Vector3 _dist, float _partialTicks)
	{
		if (this.isEntityRemote && _partialTicks > 1f)
		{
			_dist /= _partialTicks;
		}
		this.speedForward *= 0.5f;
		this.speedStrafe *= 0.5f;
		this.speedVertical *= 0.5f;
		if (Mathf.Abs(_dist.x) > 0.001f || Mathf.Abs(_dist.z) > 0.001f)
		{
			float num = Mathf.Sin(-this.rotation.y * 3.14159274f / 180f);
			float num2 = Mathf.Cos(-this.rotation.y * 3.14159274f / 180f);
			this.speedForward += num2 * _dist.z - num * _dist.x;
			this.speedStrafe += num2 * _dist.x + num * _dist.z;
		}
		if (Mathf.Abs(_dist.y) > 0.001f)
		{
			this.speedVertical += _dist.y;
		}
		this.SetMovementState();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void playStepSound(string stepSound)
	{
		if (this is EntityPlayerLocal)
		{
			Manager.BroadcastPlay(this, stepSound, false);
			if (this.soundFootstepModifier != null)
			{
				Manager.BroadcastPlay(this, this.soundFootstepModifier, false);
				return;
			}
		}
		else
		{
			Manager.Play(this, stepSound, 1f, false);
			if (this.soundFootstepModifier != null)
			{
				Manager.Play(this, this.soundFootstepModifier, 1f, false);
			}
		}
	}

	public virtual void SetLookPosition(Vector3 _lookPos)
	{
		if ((this.lookAtPosition - _lookPos).sqrMagnitude < 0.0016f)
		{
			return;
		}
		this.lookAtPosition = _lookPos;
		if (this.world.entityDistributer != null)
		{
			this.world.entityDistributer.SendPacketToTrackedPlayers(this.entityId, (this.world.GetPrimaryPlayer() != null) ? this.world.GetPrimaryPlayer().entityId : -1, NetPackageManager.GetPackage<NetPackageEntityLookAt>().Setup(this.entityId, _lookPos), false);
		}
		if (this.emodel.avatarController != null)
		{
			this.emodel.avatarController.SetLookPosition(_lookPos);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool isRadiationSensitive()
	{
		return true;
	}

	public virtual bool IsAimingGunPossible()
	{
		return true;
	}

	public int GetDeathTime()
	{
		return this.deathUpdateTime;
	}

	public void SetDeathTime(int _deathTime)
	{
		this.deathUpdateTime = _deathTime;
	}

	public int GetTimeStayAfterDeath()
	{
		return this.timeStayAfterDeath;
	}

	public bool IsCorpse()
	{
		return this.emodel && this.emodel.IsRagdollDead && (float)this.deathUpdateTime > 70f;
	}

	public override void OnAddedToWorld()
	{
		if (!(this is EntityPlayerLocal))
		{
			OcclusionManager.AddEntity(this, 7f);
		}
		this.m_addedToWorld = true;
		if (!this.isEntityRemote)
		{
			this.bSpawned = true;
		}
		if (this as EntityPlayer == null)
		{
			this.FireEvent(MinEventTypes.onSelfFirstSpawn, true);
		}
		this.StartStopLivingSound();
	}

	public override void OnEntityUnload()
	{
		if (!(this is EntityPlayerLocal))
		{
			OcclusionManager.RemoveEntity(this);
		}
		if (this.navigator != null)
		{
			this.navigator.SetPath(null, 0f);
			this.navigator = null;
		}
		base.OnEntityUnload();
		this.lookHelper = null;
		this.moveHelper = null;
		this.seeCache = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float GetDamageFraction(float _damage)
	{
		return _damage / (float)this.GetMaxHealth();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float GetDismemberChance(ref DamageResponse _dmResponse, float damagePer)
	{
		EnumBodyPartHit hitBodyPart = _dmResponse.HitBodyPart;
		EntityClass entityClass = EntityClass.list[this.entityClass];
		float num = 0f;
		switch (hitBodyPart.ToPrimary())
		{
		case BodyPrimaryHit.Head:
			num = entityClass.DismemberMultiplierHead;
			break;
		case BodyPrimaryHit.LeftUpperArm:
		case BodyPrimaryHit.RightUpperArm:
		case BodyPrimaryHit.LeftLowerArm:
		case BodyPrimaryHit.RightLowerArm:
			num = entityClass.DismemberMultiplierArms;
			break;
		case BodyPrimaryHit.LeftUpperLeg:
		case BodyPrimaryHit.RightUpperLeg:
		case BodyPrimaryHit.LeftLowerLeg:
		case BodyPrimaryHit.RightLowerLeg:
			num = entityClass.DismemberMultiplierLegs;
			break;
		}
		num = EffectManager.GetValue(PassiveEffects.DismemberSelfChance, null, num, this, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
		float dismemberChance = _dmResponse.Source.DismemberChance;
		float num2 = dismemberChance * damagePer * num;
		EntityPlayerLocal entityPlayerLocal = this.world.GetEntity(_dmResponse.Source.getEntityId()) as EntityPlayerLocal;
		if (entityPlayerLocal && entityPlayerLocal.DebugDismembermentChance)
		{
			num2 = 1f;
		}
		if (DismembermentManager.DebugLogEnabled && num2 > 0f)
		{
			Log.Out("GetDismemberChance {0}, primary {1}, damage {2}, chance {3} * damage% {4} * multiplier {5} = {6}", new object[]
			{
				hitBodyPart,
				hitBodyPart.ToPrimary(),
				_dmResponse.Strength,
				dismemberChance.ToCultureInvariantString(),
				damagePer.ToCultureInvariantString(),
				num.ToCultureInvariantString(),
				num2.ToCultureInvariantString()
			});
		}
		return num2;
	}

	public virtual void CheckDismember(ref DamageResponse _dmResponse, float damagePer)
	{
		bool flag = _dmResponse.HitBodyPart.IsLeg();
		if (flag && base.IsAlive() && (this.bodyDamage.CurrentStun != EnumEntityStunType.None || this.sleepingOrWakingUp))
		{
			return;
		}
		float dismemberChance = this.GetDismemberChance(ref _dmResponse, damagePer);
		if (dismemberChance > 0f && this.rand.RandomFloat <= dismemberChance)
		{
			_dmResponse.Dismember = true;
			if (flag)
			{
				_dmResponse.TurnIntoCrawler = true;
			}
			return;
		}
		if (flag)
		{
			EntityClass entityClass = EntityClass.list[this.entityClass];
			if (entityClass.LegCrawlerThreshold > 0f && this.GetDamageFraction((float)_dmResponse.Strength) >= entityClass.LegCrawlerThreshold)
			{
				_dmResponse.TurnIntoCrawler = true;
			}
			if (!this.bodyDamage.ShouldBeCrawler && !_dmResponse.TurnIntoCrawler && entityClass.LegCrippleScale > 0f)
			{
				float num = this.GetDamageFraction((float)_dmResponse.Strength) * entityClass.LegCrippleScale;
				if (num >= 0.05f)
				{
					if ((this.bodyDamage.Flags & 4096U) == 0U && _dmResponse.HitBodyPart.IsLeftLeg() && this.rand.RandomFloat < num)
					{
						_dmResponse.CrippleLegs = true;
					}
					if ((this.bodyDamage.Flags & 8192U) == 0U && _dmResponse.HitBodyPart.IsRightLeg() && this.rand.RandomFloat < num)
					{
						_dmResponse.CrippleLegs = true;
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ApplyLocalBodyDamage(DamageResponse _dmResponse)
	{
		EnumBodyPartHit enumBodyPartHit = _dmResponse.HitBodyPart;
		this.bodyDamage.bodyPartHit = enumBodyPartHit;
		this.bodyDamage.damageType = _dmResponse.Source.damageType;
		if (_dmResponse.Dismember)
		{
			if (DismembermentManager.DebugBodyPartHit != EnumBodyPartHit.None)
			{
				enumBodyPartHit = DismembermentManager.DebugBodyPartHit;
			}
			DismemberedPartData partData = DismembermentManager.GetPartData(this);
			if (partData != null && partData.isLinked)
			{
				EnumBodyPartHit bodyPartHit = DismembermentManager.GetBodyPartHit(partData.propertyKey);
				this.bodyDamage.bodyPartHit = bodyPartHit;
				if (DismembermentManager.DebugLogEnabled)
				{
					Log.Out("partDataLinked: " + partData.Log());
				}
			}
			if ((enumBodyPartHit & EnumBodyPartHit.Head) > EnumBodyPartHit.None)
			{
				this.bodyDamage.Flags = (this.bodyDamage.Flags | 1U);
			}
			if ((enumBodyPartHit & EnumBodyPartHit.LeftUpperArm) > EnumBodyPartHit.None)
			{
				this.bodyDamage.Flags = (this.bodyDamage.Flags | 2U);
			}
			if ((enumBodyPartHit & EnumBodyPartHit.LeftLowerArm) > EnumBodyPartHit.None)
			{
				this.bodyDamage.Flags = (this.bodyDamage.Flags | 4U);
			}
			if ((enumBodyPartHit & EnumBodyPartHit.RightUpperArm) > EnumBodyPartHit.None)
			{
				this.bodyDamage.Flags = (this.bodyDamage.Flags | 8U);
			}
			if ((enumBodyPartHit & EnumBodyPartHit.RightLowerArm) > EnumBodyPartHit.None)
			{
				this.bodyDamage.Flags = (this.bodyDamage.Flags | 16U);
			}
			if ((enumBodyPartHit & EnumBodyPartHit.LeftUpperLeg) > EnumBodyPartHit.None)
			{
				this.bodyDamage.Flags = (this.bodyDamage.Flags | 32U);
				this.bodyDamage.ShouldBeCrawler = true;
			}
			if ((enumBodyPartHit & EnumBodyPartHit.LeftLowerLeg) > EnumBodyPartHit.None)
			{
				this.bodyDamage.Flags = (this.bodyDamage.Flags | 64U);
				this.bodyDamage.ShouldBeCrawler = true;
			}
			if ((enumBodyPartHit & EnumBodyPartHit.RightUpperLeg) > EnumBodyPartHit.None)
			{
				this.bodyDamage.Flags = (this.bodyDamage.Flags | 128U);
				this.bodyDamage.ShouldBeCrawler = true;
			}
			if ((enumBodyPartHit & EnumBodyPartHit.RightLowerLeg) > EnumBodyPartHit.None)
			{
				this.bodyDamage.Flags = (this.bodyDamage.Flags | 256U);
				this.bodyDamage.ShouldBeCrawler = true;
			}
		}
		if (_dmResponse.TurnIntoCrawler)
		{
			this.bodyDamage.ShouldBeCrawler = true;
		}
		if (_dmResponse.CrippleLegs)
		{
			if (_dmResponse.HitBodyPart.IsLeftLeg())
			{
				this.bodyDamage.Flags = (this.bodyDamage.Flags | 4096U);
			}
			if (_dmResponse.HitBodyPart.IsRightLeg())
			{
				this.bodyDamage.Flags = (this.bodyDamage.Flags | 8192U);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void ExecuteDismember(bool restoreState)
	{
		if (this.emodel == null || this.emodel.avatarController == null)
		{
			return;
		}
		this.emodel.avatarController.DismemberLimb(this.bodyDamage, restoreState);
		if (this.bodyDamage.ShouldBeCrawler)
		{
			this.SetupCrawlerState(restoreState);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupCrawlerState(bool restoreState)
	{
		if (!this.IsDead())
		{
			this.emodel.avatarController.TurnIntoCrawler(restoreState);
			base.SetMaxHeight(0.5f);
			ItemValue itemValue = null;
			if (EntityClass.list[this.entityClass].Properties.Values.ContainsKey(EntityClass.PropHandItemCrawler))
			{
				itemValue = ItemClass.GetItem(EntityClass.list[this.entityClass].Properties.Values[EntityClass.PropHandItemCrawler], false);
				if (itemValue.IsEmpty())
				{
					itemValue = null;
				}
			}
			if (itemValue == null)
			{
				itemValue = ItemClass.GetItem("meleeHandZombie02", false);
			}
			this.inventory.SetBareHandItem(itemValue);
			this.TurnIntoCrawler();
		}
		this.walkType = 21;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void TurnIntoCrawler()
	{
	}

	public void ClearStun()
	{
		this.bodyDamage.CurrentStun = EnumEntityStunType.None;
		this.bodyDamage.StunDuration = 0f;
		this.SetCVar("_stunned", 0f);
	}

	public void SetStun(EnumEntityStunType stun)
	{
		this.bodyDamage.CurrentStun = stun;
		this.SetCVar("_stunned", 1f);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void onSpawnStateChanged()
	{
		if (!this.m_addedToWorld)
		{
			return;
		}
		this.StartStopLivingSound();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void StartStopLivingSound()
	{
		if (this.soundLiving != null)
		{
			if (this.Spawned)
			{
				if (!this.IsDead() && this.Health > 0)
				{
					Manager.Play(this, this.soundLiving, 1f, false);
					this.soundLivingID = 0;
				}
			}
			else if (this.soundLivingID >= 0)
			{
				Manager.Stop(this.entityId, this.soundLiving);
				this.soundLivingID = -1;
			}
		}
		if (this.Spawned && this.soundSpawn != null && !this.SleeperSupressLivingSounds)
		{
			this.PlayOneShot(this.soundSpawn, false, false, false);
		}
	}

	public void HeadHeightFixedUpdate()
	{
		if (this.physicsBaseHeight <= 1.3f)
		{
			return;
		}
		float num = this.physicsBaseHeight;
		if (base.IsInElevator())
		{
			num *= 1.06f;
		}
		if (this.emodel.IsRagdollMovement || this.bodyDamage.CurrentStun == EnumEntityStunType.Prone)
		{
			num = this.physicsBaseHeight * 0.08f;
		}
		float num2 = this.m_characterController.GetRadius() * 0.9f;
		float num3 = num2 + 0.5f;
		float maxDistance = num + 0.22f - num3 - num2;
		Vector3 position = this.PhysicsTransform.position;
		position.y += num3;
		RaycastHit raycastHit;
		bool flag = Physics.SphereCast(position, num2, Vector3.up, out raycastHit, maxDistance, 1083277312);
		if (flag)
		{
			Transform transform = raycastHit.transform;
			if (transform && transform.CompareTag("Physics"))
			{
				Entity component = transform.GetComponent<Entity>();
				if (component != null)
				{
					component.PhysicsPush(transform.forward * 0.1f * Time.fixedDeltaTime, raycastHit.point, true);
				}
				return;
			}
			if (this.world.GetBlock(new Vector3i(raycastHit.point + Origin.position)).Block.Damage <= 0f)
			{
				num = num3 + raycastHit.distance - 0.16f;
			}
		}
		if (num < this.physicsHeight)
		{
			if (base.IsInElevator())
			{
				return;
			}
			num = Mathf.MoveTowards(this.physicsHeight, num, 0.03333333f);
		}
		else
		{
			num = Mathf.MoveTowards(this.physicsHeight, num, 0.0142857144f);
		}
		base.SetHeight(num);
		if (flag && num <= this.physicsBaseHeight * 0.75f)
		{
			int num4 = 22;
			if (this.walkType != num4 && this.walkType != 21)
			{
				this.walkTypeBeforeCrouch = this.walkType;
				this.SetWalkType(num4);
				return;
			}
		}
		else if (this.walkTypeBeforeCrouch != 0 && num >= this.physicsBaseHeight)
		{
			this.SetWalkType(this.walkTypeBeforeCrouch);
			this.walkTypeBeforeCrouch = 0;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetWalkType(int _walkType)
	{
		this.walkType = _walkType;
		this.emodel.avatarController.SetWalkType(_walkType, true);
	}

	public int GetWalkType()
	{
		return this.walkType;
	}

	public bool IsWalkTypeACrawl()
	{
		return this.walkType >= 20;
	}

	public string GetRightHandTransformName()
	{
		return this.rightHandTransformName;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool isGameMessageOnDeath()
	{
		return true;
	}

	public override float GetLightBrightness()
	{
		Vector3i blockPosition = base.GetBlockPosition();
		Vector3i blockPos = blockPosition;
		blockPos.y += Mathf.RoundToInt(base.height + 0.5f);
		return Utils.FastMax(this.world.GetLightBrightness(blockPosition), this.world.GetLightBrightness(blockPos));
	}

	public virtual float GetLightLevel()
	{
		EntityAlive entityAlive = this.AttachedToEntity as EntityAlive;
		if (entityAlive)
		{
			return entityAlive.GetLightLevel();
		}
		return this.inventory.GetLightLevel();
	}

	public override int AttachToEntity(Entity _other, int slot = -1)
	{
		slot = base.AttachToEntity(_other, slot);
		if (slot >= 0)
		{
			this.CurrentMovementTag = EntityAlive.MovementTagIdle;
			this.Crouching = false;
			this.saveInventory = null;
			if (_other is EntityAlive && _other.GetAttachedToInfo(slot).bReplaceLocalInventory)
			{
				this.saveInventory = this.inventory;
				this.saveHoldingItemIdxBeforeAttach = this.inventory.holdingItemIdx;
				this.inventory.SetHoldingItemIdxNoHolsterTime(this.inventory.DUMMY_SLOT_IDX);
				this.inventory = ((EntityAlive)_other).inventory;
			}
			this.bPlayerStatsChanged |= !this.isEntityRemote;
		}
		return slot;
	}

	public override void Detach()
	{
		if (this.saveInventory != null)
		{
			this.inventory = this.saveInventory;
			this.inventory.SetHoldingItemIdxNoHolsterTime(this.saveHoldingItemIdxBeforeAttach);
			this.saveInventory = null;
		}
		base.Detach();
		this.bPlayerStatsChanged |= !this.isEntityRemote;
	}

	public override void Write(BinaryWriter _bw, bool _bNetworkWrite)
	{
		base.Write(_bw, _bNetworkWrite);
		_bw.Write(this.deathHealth);
	}

	public override void Read(byte _version, BinaryReader _br)
	{
		base.Read(_version, _br);
		if (_version > 24)
		{
			this.deathHealth = _br.ReadInt32();
		}
	}

	public override string ToString()
	{
		return string.Format("[type={0}, name={1}, id={2}]", base.GetType().Name, GameUtils.SafeStringFormat(this.EntityName), this.entityId);
	}

	public virtual void FireEvent(MinEventTypes _eventType, bool useInventory = true)
	{
		MinEffectController effects = EntityClass.list[this.entityClass].Effects;
		if (effects != null)
		{
			effects.FireEvent(_eventType, this.MinEventContext);
		}
		if (this.Progression != null)
		{
			this.Progression.FireEvent(_eventType, this.MinEventContext);
		}
		if (this.challengeJournal != null)
		{
			this.challengeJournal.FireEvent(_eventType, this.MinEventContext);
		}
		if (this.inventory != null && useInventory)
		{
			this.inventory.FireEvent(_eventType, this.MinEventContext);
		}
		this.equipment.FireEvent(_eventType, this.MinEventContext);
		this.Buffs.FireEvent(_eventType, this.MinEventContext);
	}

	public float GetCVar(string _varName)
	{
		if (this.Buffs == null)
		{
			return 0f;
		}
		return this.Buffs.GetCustomVar(_varName, 0f);
	}

	public void SetCVar(string _varName, float _value)
	{
		if (this.Buffs == null)
		{
			return;
		}
		this.Buffs.SetCustomVar(_varName, _value, true);
	}

	public virtual void BuffAdded(BuffValue _buff)
	{
	}

	public override void OnCollisionForward(Transform t, Collision collision, bool isStay)
	{
		if (!this.emodel.IsRagdollActive)
		{
			return;
		}
		if (collision.relativeVelocity.sqrMagnitude < 0.0625f)
		{
			return;
		}
		float sqrMagnitude = collision.impulse.sqrMagnitude;
		if (sqrMagnitude < 400f)
		{
			return;
		}
		if (this.IsDead())
		{
			EntityAlive.ImpactData impactData;
			this.impacts.TryGetValue(t, out impactData);
			impactData.count++;
			this.impacts[t] = impactData;
			if (impactData.count >= 10)
			{
				if (impactData.count == 10)
				{
					Rigidbody component = t.GetComponent<Rigidbody>();
					if (component)
					{
						component.velocity = Vector3.zero;
						component.angularVelocity = Vector3.zero;
						component.drag = 0.5f;
						component.angularDrag = 0.5f;
					}
					CharacterJoint component2 = t.GetComponent<CharacterJoint>();
					if (component2)
					{
						component2.enableProjection = false;
					}
				}
				if (impactData.count == 25 && !t.gameObject.CompareTag("E_BP_Body"))
				{
					t.GetComponent<Collider>().enabled = false;
				}
				return;
			}
		}
		if (Time.time - this.impactSoundTime < 0.25f)
		{
			return;
		}
		this.impactSoundTime = Time.time;
		if (t.lossyScale.x == 0f)
		{
			return;
		}
		string soundGroupName = "impactbodylight";
		if (sqrMagnitude >= 3600f)
		{
			soundGroupName = "impactbodyheavy";
		}
		Vector3 a = Vector3.zero;
		int contactCount = collision.contactCount;
		for (int i = 0; i < contactCount; i++)
		{
			a += collision.GetContact(i).point;
		}
		a *= 1f / (float)contactCount;
		Manager.BroadcastPlay(a + Origin.position, soundGroupName, 0f);
	}

	public void AddParticle(string _name, Transform _t)
	{
		if (this.particles.ContainsKey(_name))
		{
			this.particles[_name] = _t;
			return;
		}
		this.particles.Add(_name, _t);
	}

	public bool RemoveParticle(string _name)
	{
		Transform transform;
		if (this.particles.Remove(_name, out transform))
		{
			if (transform)
			{
				UnityEngine.Object.Destroy(transform.gameObject);
			}
			return true;
		}
		return false;
	}

	public bool HasParticle(string _name)
	{
		Transform transform;
		return this.particles.TryGetValue(_name, out transform);
	}

	public void AddPart(string _name, Transform _t)
	{
		if (this.parts.ContainsKey(_name))
		{
			this.parts[_name] = _t;
			return;
		}
		this.parts.Add(_name, _t);
	}

	public void RemovePart(string _name)
	{
		Transform transform;
		if (this.parts.TryGetValue(_name, out transform))
		{
			this.parts.Remove(_name);
			if (transform)
			{
				transform.gameObject.name = ".";
				UnityEngine.Object.Destroy(transform.gameObject);
			}
		}
	}

	public void SetPartActive(string _name, bool isActive)
	{
		Transform transform;
		if (this.parts.TryGetValue(_name, out transform) && transform)
		{
			bool flag = true;
			for (int i = transform.childCount - 1; i >= 0; i--)
			{
				Transform child = transform.GetChild(i);
				if (child.CompareTag("ModOn"))
				{
					child.gameObject.SetActive(isActive);
					flag = false;
				}
				else if (child.CompareTag("ModMesh"))
				{
					if (transform.parent.name == "CameraNode")
					{
						child.gameObject.SetActive(false);
					}
					flag = false;
				}
			}
			if (flag)
			{
				transform.gameObject.SetActive(isActive);
			}
		}
	}

	public void AddOwnedEntity(OwnedEntityData _entityData)
	{
		if (_entityData != null)
		{
			this.ownedEntities.Add(_entityData);
		}
	}

	public void AddOwnedEntity(Entity _entity)
	{
		if (this.ownedEntities.Find((OwnedEntityData e) => e.Id == _entity.entityId) == null)
		{
			this.AddOwnedEntity(new OwnedEntityData(_entity));
		}
	}

	public void RemoveOwnedEntity(OwnedEntityData _entityData)
	{
		if (_entityData != null)
		{
			this.ownedEntities.Remove(_entityData);
		}
	}

	public void RemoveOwnedEntity(int _entityId)
	{
		this.RemoveOwnedEntity(this.ownedEntities.Find((OwnedEntityData e) => e.Id == _entityId));
	}

	public void RemoveOwnedEntity(Entity _entity)
	{
		this.RemoveOwnedEntity(_entity.entityId);
	}

	public OwnedEntityData GetOwnedEntity(int _entityId)
	{
		return this.ownedEntities.Find((OwnedEntityData e) => e.Id == _entityId);
	}

	public OwnedEntityData[] GetOwnedEntityClass(string name)
	{
		List<OwnedEntityData> list = new List<OwnedEntityData>();
		for (int i = 0; i < this.ownedEntities.Count; i++)
		{
			OwnedEntityData ownedEntityData = this.ownedEntities[i];
			if (EntityClass.list[ownedEntityData.ClassId].entityClassName.ContainsCaseInsensitive(name))
			{
				list.Add(ownedEntityData);
			}
		}
		return list.ToArray();
	}

	public bool HasOwnedEntity(int _entityId)
	{
		return this.GetOwnedEntity(_entityId) != null;
	}

	public OwnedEntityData[] GetOwnedEntities()
	{
		return this.ownedEntities.ToArray();
	}

	public int OwnedEntityCount
	{
		get
		{
			return this.ownedEntities.Count;
		}
	}

	public void ClearOwnedEntities()
	{
		this.ownedEntities.Clear();
	}

	public void HandleSetNavName()
	{
		if (this.NavObject != null)
		{
			this.NavObject.name = this.entityName;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateDynamicRagdoll()
	{
		if (this._dynamicRagdoll.HasFlag(DynamicRagdollFlags.Active))
		{
			if (this.accumulatedRootMotion != Vector3.zero)
			{
				this._dynamicRagdollRootMotion = this.accumulatedRootMotion;
			}
			if (this._dynamicRagdoll.HasFlag(DynamicRagdollFlags.UseBoneVelocities))
			{
				this._ragdollPositionsPrev.Clear();
				this._ragdollPositionsCur.CopyTo(this._ragdollPositionsPrev);
				this.emodel.CaptureRagdollPositions(this._ragdollPositionsCur);
			}
			if (this._dynamicRagdoll.HasFlag(DynamicRagdollFlags.RagdollOnFall) && !this.onGround)
			{
				this.ActivateDynamicRagdoll();
				return;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void AnalyticsSendDeath(DamageResponse _dmResponse)
	{
	}

	public virtual string MakeDebugNameInfo()
	{
		return string.Empty;
	}

	public static void SetupAllDebugNameHUDs(bool _isAdd)
	{
		List<Entity> list = GameManager.Instance.World.Entities.list;
		for (int i = 0; i < list.Count; i++)
		{
			EntityAlive entityAlive = list[i] as EntityAlive;
			if (entityAlive)
			{
				entityAlive.SetupDebugNameHUD(_isAdd);
			}
		}
	}

	public void SetupDebugNameHUD(bool _isAdd)
	{
		if (this is EntityPlayer)
		{
			return;
		}
		GUIHUDEntityName component = this.ModelTransform.GetComponent<GUIHUDEntityName>();
		if (_isAdd)
		{
			if (!component)
			{
				this.ModelTransform.gameObject.AddComponent<GUIHUDEntityName>();
				return;
			}
		}
		else if (component)
		{
			UnityEngine.Object.Destroy(component);
		}
	}

	public EModelBase.HeadStates GetHeadState()
	{
		if (base.EntityClass.CanBigHead)
		{
			return this.emodel.HeadState;
		}
		return EModelBase.HeadStates.Standard;
	}

	public void SetBigHead()
	{
		if ((this is EntityAnimal || this is EntityEnemy || this is EntityTrader) && base.EntityClass.CanBigHead && this.emodel.HeadState == EModelBase.HeadStates.Standard)
		{
			this.emodel.HeadState = EModelBase.HeadStates.Growing;
			Manager.BroadcastPlayByLocalPlayer(this.position, "twitch_bighead_inflate");
		}
	}

	public void ResetHead()
	{
		if ((this is EntityAnimal || this is EntityEnemy || this is EntityTrader) && base.EntityClass.CanBigHead && (this.emodel.HeadState == EModelBase.HeadStates.BigHead || this.emodel.HeadState == EModelBase.HeadStates.Growing))
		{
			base.StartCoroutine(this.resetHeadLater(this.emodel));
		}
	}

	public void SetDancing(bool enabled)
	{
		if (base.EntityClass.DanceTypeID != 0)
		{
			this.IsDancing = enabled;
			return;
		}
		this.IsDancing = false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public IEnumerator resetHeadLater(EModelBase model)
	{
		yield return new WaitForSeconds(0.25f);
		if (this.emodel != null && this.emodel.GetHeadTransform() != null && this.emodel.GetHeadTransform().localScale.x > 1f)
		{
			this.emodel.HeadState = EModelBase.HeadStates.Shrinking;
			Manager.BroadcastPlayByLocalPlayer(this.position, "twitch_bighead_deflate");
		}
		yield break;
	}

	public void SetSpawnByData(int newSpawnByID, string newSpawnByName)
	{
		this.spawnById = newSpawnByID;
		this.spawnByName = newSpawnByName;
		this.bPlayerStatsChanged |= !this.isEntityRemote;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void SetHeadSize(float overrideHeadSize)
	{
		this.OverrideHeadSize = overrideHeadSize;
		this.emodel.SetHeadScale(overrideHeadSize);
	}

	public void SetVehiclePoseMode(int _pose)
	{
		this.vehiclePoseMode = _pose;
		if (_pose != this.GetVehicleAnimation())
		{
			this.Crouching = false;
			this.SetVehicleAnimation(AvatarController.vehiclePoseHash, _pose);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public EntityAlive()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cDamageImmunityOnRespawnSeconds = 1f;

	public static readonly FastTags<TagGroup.Global> DistractionResistanceWithTargetTags = FastTags<TagGroup.Global>.GetTag("with_target");

	public static readonly int FeralTagBit = FastTags<TagGroup.Global>.GetBit("feral");

	public static readonly int FallingBuffTagBit = FastTags<TagGroup.Global>.GetBit("buffPlayerFallingDamage");

	public static readonly FastTags<TagGroup.Global> StanceTagCrouching = FastTags<TagGroup.Global>.GetTag("crouching");

	public static readonly FastTags<TagGroup.Global> StanceTagStanding = FastTags<TagGroup.Global>.GetTag("standing");

	public static readonly FastTags<TagGroup.Global> MovementTagIdle = FastTags<TagGroup.Global>.GetTag("idle");

	public static readonly FastTags<TagGroup.Global> MovementTagWalking = FastTags<TagGroup.Global>.GetTag("walking");

	public static readonly FastTags<TagGroup.Global> MovementTagRunning = FastTags<TagGroup.Global>.GetTag("running");

	public static readonly FastTags<TagGroup.Global> MovementTagFloating = FastTags<TagGroup.Global>.GetTag("floating");

	public static readonly FastTags<TagGroup.Global> MovementTagSwimming = FastTags<TagGroup.Global>.GetTag("swimming");

	public static readonly FastTags<TagGroup.Global> MovementTagSwimmingRun = FastTags<TagGroup.Global>.GetTag("swimmingRun");

	public static readonly FastTags<TagGroup.Global> MovementTagJumping = FastTags<TagGroup.Global>.GetTag("jumping");

	public static readonly FastTags<TagGroup.Global> MovementTagFalling = FastTags<TagGroup.Global>.GetTag("falling");

	public static readonly FastTags<TagGroup.Global> MovementTagClimbing = FastTags<TagGroup.Global>.GetTag("climbing");

	public static readonly FastTags<TagGroup.Global> MovementTagDriving = FastTags<TagGroup.Global>.GetTag("driving");

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static readonly float[] moveSpeedRandomness = new float[]
	{
		0.2f,
		1f,
		1.1f,
		1.2f,
		1.35f,
		1.5f
	};

	public const float CLIMB_LADDER_SPEED = 1234f;

	public static ulong HitDelay = 11000UL;

	public static float HitSoundDistance = 10f;

	public MinEventParams MinEventContext = new MinEventParams();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int equippingCount;

	public bool IsSleeper;

	public bool IsSleeping;

	public bool IsSleeperPassive;

	public bool SleeperSupressLivingSounds;

	public Vector3 SleeperSpawnPosition;

	public Vector3 SleeperSpawnLookDir;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float accumulatedDamageResisted;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int pendingSleepTrigger = -1;

	public int lastSleeperPose;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 sleeperLookDir;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float sleeperSightRange;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float sleeperViewAngle;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector2 sightLightThreshold;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector2 sightWakeThresholdAtRange;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector2 sightGroanThresholdAtRange;

	public float noiseGroan;

	public float noiseWake;

	public float smellGroan;

	public float smellWake;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isSnore;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isGroan;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isGroanSilent;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float groanChance;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int snoreGroanCD;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int kSnoreGroanMinCD = 20;

	public float noisePlayerDistance;

	public float noisePlayerVolume;

	public EntityPlayer noisePlayer;

	public EntityItem pendingDistraction;

	public float pendingDistractionDistanceSq;

	public EntityItem distraction;

	public float distractionResistance;

	public float distractionResistanceWithTarget;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cSwimGravityPer = 0.025f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cSwimDragY = 0.91f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cSwimDrag = 0.91f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cSwimAnimDelay = 6f;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public int jumpTicks;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public EntityAlive.JumpState jumpState;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public int jumpStateTicks;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float jumpDistance;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float jumpHeightDiff;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float jumpSwimDurationTicks;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector3 jumpSwimMotion;

	public float jumpDelay;

	public float jumpMaxDistance;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool jumpIsMoving;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int ticksNoPlayerAdjacent;

	public int hasBeenAttackedTime;

	public float painResistPercent;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public int attackingTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public EntityAlive revengeEntity;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int revengeTimer;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool targetAlertChanged;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float lastAliveTime;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool alertEnabled = true;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int alertTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static string notAlertedId = "_notAlerted";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int notAlertDelayTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isAlert;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 investigatePos;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int investigatePositionTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isInvestigateAlert;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool hasAI;

	public EAIManager aiManager;

	public List<string> AIPackages;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Context utilityAIContext;

	public EntityPlayer aiClosestPlayer;

	public float aiClosestPlayerDistSq;

	public float aiActiveScale;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float aiActiveDelay;

	public bool IsBloodMoon;

	public bool IsFeral;

	public bool IsBreakingDoors;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool m_isBreakingBlocks;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool m_isEating;

	public Vector3 ChaseReturnLocation;

	public bool IsScoutZombie;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public EntityLookHelper lookHelper;

	public EntityMoveHelper moveHelper;

	public PathNavigate navigator;

	public bool bCanClimbLadders;

	public bool bCanClimbVertical;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 lastTargetPos;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public EntityAlive damagedTarget;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public EntityAlive attackTarget;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int attackTargetTime;

	public EntityAlive attackTargetClient;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public EntityAlive attackTargetLast;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public EntitySeeCache seeCache;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public ChunkCoordinates homePosition;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int maximumHomeDistance;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float jumpMovementFactor = 0.02f;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float landMovementFactor = 0.1f;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float jumpMotionYValue = 0.419f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float stepDistanceToPlaySound;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float nextSwimDistance;

	public Inventory inventory;

	public Inventory saveInventory;

	public Equipment equipment;

	public Bag bag;

	public ChallengeJournal challengeJournal;

	public int ExperienceValue;

	public int deathUpdateTime;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public EntityAlive entityThatKilledMe;

	public bool bPlayerStatsChanged;

	public bool bEntityAliveFlagsChanged;

	public bool bPlayerTwitchChanged;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Dictionary<EnumDamageSource, ulong> damageSourceTimeouts = new EnumDictionary<EnumDamageSource, ulong>();

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public int traderTeleportStreak = 1;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool bJetpackWearing;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool bJetpackActive;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool bParachuteWearing;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool bAimingGun;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool bMovementRunning;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool bCrouching;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool bJumping;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool bClimbing;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public int died;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public int score;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public int killedZombies;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public int killedPlayers;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public int teamNumber;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public string entityName = string.Empty;

	public string DebugNameInfo = string.Empty;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public int damageLocationBits;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool bSpawned;

	public bool bReplicatedAlertFlag;

	public int vehiclePoseMode = -1;

	public byte factionId;

	public byte factionRank;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int ticksToCheckSeenByPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool wasSeenByPlayer;

	public DamageResponse RecordedDamage;

	public float moveSpeed;

	public float moveSpeedNight;

	public float moveSpeedAggro;

	public float moveSpeedAggroMax;

	public float moveSpeedPanic;

	public float moveSpeedPanicMax;

	public float swimSpeed;

	public Vector2 swimStrokeRate;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public ItemValue handItem;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string soundSpawn;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string soundSleeperGroan;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string soundSleeperSnore;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string soundDeath;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string soundAlert;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string soundAttack;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string soundLiving;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string soundRandom;

	public string soundSense;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string soundGiveUp;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string soundFootstepModifier;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string soundStamina;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string soundJump;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string soundLand;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string soundHurt;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string soundHurtSmall;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string soundDrownPain;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public string soundDrownDeath;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public string soundWaterSurface;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int soundDelayTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int soundLivingID = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cSoundRandomMaxDist = 20f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int soundAlertTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int soundRandomTicks;

	public int classMaxHealth;

	public int classMaxStamina;

	public int classMaxFood;

	public int classMaxWater;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float weight;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float pushFactor;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float maxViewAngle;

	public float sightRangeBase;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float sightRange;

	public float senseScale;

	public int timeStayAfterDeath;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public BlockValue corpseBlockValue;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float corpseBlockChance;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int attackTimeoutDay;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int attackTimeoutNight;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string particleOnDeath;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string particleOnDestroy;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public EntityBedrollPositionList spawnPoints;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public List<Vector3i> droppedBackpackPositions;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float speedModifier = 1f;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector3 accumulatedRootMotion;

	public Vector3 moveDirection;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isMoveDirAbsolute;

	public Vector3 lookAtPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3i blockPosStandingOn;

	public BlockValue blockValueStandingOn;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool blockStandingOnChanged;

	public BiomeDefinition biomeStandingOn;

	public bool IsMale;

	public const int cWalkTypeSwim = -1;

	public const int cWalkTypeFat = 1;

	public const int cWalkTypeCripple = 5;

	public const int cWalkTypeBandit = 15;

	public const int cWalkTypeCrawlFirst = 20;

	public const int cWalkTypeCrawler = 21;

	public const int cWalkTypeSpider = 22;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public int walkType;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public int walkTypeBeforeCrouch;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string rightHandTransformName;

	public int pingToServer;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public List<ItemStack> itemsOnEnterGame = new List<ItemStack>();

	public Utils.EnumHitDirection lastHitDirection = Utils.EnumHitDirection.None;

	public Vector3 lastHitImpactDir = Vector3.zero;

	public Vector3 lastHitEntityFwd = Vector3.zero;

	public bool lastHitRanged;

	public float lastHitForce;

	public float CreationTimeSinceLevelLoad;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public EntityStats entityStats;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float proneRefillRate;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float kneelRefillRate;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float proneRefillCounter;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float kneelRefillCounter;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int deathHealth;

	public BodyDamage bodyDamage;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool stompsSpikes;

	public float OverrideSize = 1f;

	public float OverrideHeadSize = 1f;

	public float OverrideHeadDismemberScaleTime = 1.5f;

	public float OverridePitch;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isDancing;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float lastTimeTraderStationChecked;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool lerpForwardSpeed;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float speedForwardTarget;

	public EntityBuffs Buffs;

	public Progression Progression;

	public FastTags<TagGroup.Global> CurrentStanceTag = EntityAlive.StanceTagStanding;

	public FastTags<TagGroup.Global> CurrentMovementTag = FastTags<TagGroup.Global>.none;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float renderFade;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float renderFadeTarget;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public readonly List<EntityAlive.FallBehavior> fallBehaviors = new List<EntityAlive.FallBehavior>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool disableFallBehaviorUntilOnGround;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public readonly List<EntityAlive.DestroyBlockBehavior> _destroyBlockBehaviors = new List<EntityAlive.DestroyBlockBehavior>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public DynamicRagdollFlags _dynamicRagdoll;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float _dynamicRagdollStunTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 _dynamicRagdollRootMotion;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public readonly List<Vector3> _ragdollPositionsPrev = new List<Vector3>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public readonly List<Vector3> _ragdollPositionsCur = new List<Vector3>();

	public bool CrouchingLocked;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public EModelBase.HeadStates currentHeadState;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static readonly List<EntityAlive.WeightBehavior> weightBehaviorTemp = new List<EntityAlive.WeightBehavior>();

	public static bool ShowDebugDisplayHit = false;

	public static float DebugDisplayHitSize = 0.005f;

	public static float DebugDisplayHitTime = 10f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool bPlayHurtSound;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool bBeenWounded;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public int woundedStrength;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public DamageSource woundedDamageSource;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int despawnDelayCounter;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isDespawnWhenPlayerFar;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool bLastAttackReleased;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool wasOnGround = true;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float landWaterLevel;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool m_addedToWorld;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int saveHoldingItemIdxBeforeAttach;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float impactSoundTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Dictionary<Transform, EntityAlive.ImpactData> impacts = new Dictionary<Transform, EntityAlive.ImpactData>();

	public const string cParticlePrefix = "Ptl_";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Dictionary<string, Transform> particles = new Dictionary<string, Transform>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Dictionary<string, Transform> parts = new Dictionary<string, Transform>();

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<OwnedEntityData> ownedEntities = new List<OwnedEntityData>();

	public enum JumpState
	{
		Off,
		Climb,
		Leap,
		Air,
		Land,
		SwimStart,
		Swim
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public class FallBehavior
	{
		public FallBehavior(string name, EntityAlive.FallBehavior.Op type, FloatRange height, float weight, FloatRange ragePer, FloatRange rageTime, IntRange difficulty)
		{
			this.Name = name;
			this.ResponseOp = type;
			this.Height = height;
			this.Weight = weight;
			this.RagePer = ragePer;
			this.RageTime = rageTime;
			this.Difficulty = difficulty;
		}

		public string Name;

		public readonly EntityAlive.FallBehavior.Op ResponseOp;

		public readonly FloatRange Height;

		public readonly float Weight;

		public readonly FloatRange RagePer;

		public readonly FloatRange RageTime;

		public readonly IntRange Difficulty = new IntRange(int.MinValue, int.MaxValue);

		public enum Op
		{
			None,
			Land,
			LandLow,
			LandHard,
			Stumble,
			Ragdoll
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public class DestroyBlockBehavior
	{
		public DestroyBlockBehavior(string name, EntityAlive.DestroyBlockBehavior.Op type, float weight, FloatRange ragePer, FloatRange rageTime, IntRange difficulty)
		{
			this.Name = name;
			this.ResponseOp = type;
			this.Weight = weight;
			this.RagePer = ragePer;
			this.RageTime = rageTime;
			this.Difficulty = difficulty;
		}

		public string Name;

		public readonly EntityAlive.DestroyBlockBehavior.Op ResponseOp;

		public readonly float Weight;

		public readonly FloatRange RagePer;

		public readonly FloatRange RageTime;

		public readonly IntRange Difficulty = new IntRange(int.MinValue, int.MaxValue);

		public enum Op
		{
			None,
			Ragdoll,
			Stumble
		}
	}

	public enum EnumApproachState
	{
		Ok,
		TooFarAway,
		BlockedByWorldMesh,
		BlockedByEntity,
		Unknown
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct WeightBehavior
	{
		public float weight;

		public int index;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct ImpactData
	{
		public int count;
	}
}

using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EntityHuman : EntityEnemy
{
	public override Entity.EnumPositionUpdateMovementType positionUpdateMovementType
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return Entity.EnumPositionUpdateMovementType.MoveTowards;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		base.Awake();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void InitCommon()
	{
		base.InitCommon();
		if (this.walkType == 21)
		{
			this.TurnIntoCrawler();
		}
	}

	public override void OnAddedToWorld()
	{
		base.OnAddedToWorld();
		if (base.GetSpawnerSource() == EnumSpawnerSource.Biome)
		{
			this.timeToDie = this.world.worldTime + 12000UL + (ulong)(10000f * this.rand.RandomFloat);
			if (this.IsFeral)
			{
				int num = (int)SkyManager.GetDawnTime();
				int num2 = (int)SkyManager.GetDuskTime();
				int num3 = GameUtils.WorldTimeToHours(this.WorldTimeBorn);
				if (num3 < num || num3 >= num2)
				{
					int num4 = GameUtils.WorldTimeToDays(this.world.worldTime);
					if (GameUtils.WorldTimeToHours(this.world.worldTime) >= num2)
					{
						num4++;
					}
					this.timeToDie = GameUtils.DayTimeToWorldTime(num4, num, 0);
					return;
				}
			}
		}
		else
		{
			this.timeToDie = this.world.worldTime + 4000UL + (ulong)(4000f * this.rand.RandomFloat);
		}
	}

	public override void OnUpdateLive()
	{
		base.OnUpdateLive();
		if (this.moveSpeedRagePer > 0f && this.bodyDamage.CurrentStun == EnumEntityStunType.None)
		{
			this.moveSpeedScaleTime -= 0.05f;
			if (this.moveSpeedScaleTime <= 0f)
			{
				this.StopRage();
			}
		}
		if (!this.isEntityRemote && !this.IsDead() && this.world.worldTime >= this.timeToDie && !this.attackTarget)
		{
			this.Kill(DamageResponse.New(true));
		}
		if (this.emodel)
		{
			AvatarController avatarController = this.emodel.avatarController;
			if (avatarController)
			{
				bool flag = this.onGround || this.isSwimming || this.bInElevator;
				if (flag)
				{
					this.fallTime = 0f;
					this.fallThresholdTime = 0f;
					if (this.bInElevator)
					{
						this.fallThresholdTime = 0.6f;
					}
				}
				else
				{
					if (this.fallThresholdTime == 0f)
					{
						this.fallThresholdTime = 0.1f + this.rand.RandomFloat * 0.3f;
					}
					this.fallTime += 0.05f;
				}
				bool canFall = !this.emodel.IsRagdollActive && this.bodyDamage.CurrentStun == EnumEntityStunType.None && !this.isSwimming && !this.bInElevator && this.jumpState == EntityAlive.JumpState.Off && !this.IsDead();
				if (this.fallTime <= this.fallThresholdTime)
				{
					canFall = false;
				}
				avatarController.SetFallAndGround(canFall, flag);
			}
		}
	}

	public override Ray GetLookRay()
	{
		Ray result;
		if (base.IsBreakingBlocks)
		{
			result = new Ray(this.position + new Vector3(0f, this.GetEyeHeight(), 0f), this.GetLookVector());
		}
		else if (base.GetWalkType() == 22)
		{
			result = new Ray(this.getHeadPosition(), this.GetLookVector());
		}
		else
		{
			result = new Ray(this.getHeadPosition(), this.GetLookVector());
		}
		return result;
	}

	public override Vector3 GetLookVector()
	{
		if (this.lookAtPosition.Equals(Vector3.zero))
		{
			return base.GetLookVector();
		}
		return this.lookAtPosition - this.getHeadPosition();
	}

	public override bool IsRunning
	{
		get
		{
			EnumGamePrefs eProperty = EnumGamePrefs.ZombieMove;
			if (this.IsBloodMoon)
			{
				eProperty = EnumGamePrefs.ZombieBMMove;
			}
			else if (this.IsFeral)
			{
				eProperty = EnumGamePrefs.ZombieFeralMove;
			}
			else if (this.world.IsDark())
			{
				eProperty = EnumGamePrefs.ZombieMoveNight;
			}
			return GamePrefs.GetInt(eProperty) >= 2;
		}
	}

	public override float GetMoveSpeedAggro()
	{
		EnumGamePrefs eProperty = EnumGamePrefs.ZombieMove;
		if (this.IsBloodMoon)
		{
			eProperty = EnumGamePrefs.ZombieBMMove;
		}
		else if (this.IsFeral)
		{
			eProperty = EnumGamePrefs.ZombieFeralMove;
		}
		else if (this.world.IsDark())
		{
			eProperty = EnumGamePrefs.ZombieMoveNight;
		}
		int @int = GamePrefs.GetInt(eProperty);
		float num = EntityHuman.moveSpeeds[@int];
		if (this.moveSpeedRagePer > 1f)
		{
			num = EntityHuman.moveSuperRageSpeeds[@int];
		}
		else if (this.moveSpeedRagePer > 0f)
		{
			float num2 = EntityHuman.moveRageSpeeds[@int];
			num = num * (1f - this.moveSpeedRagePer) + num2 * this.moveSpeedRagePer;
		}
		if (num < 1f)
		{
			num = this.moveSpeedAggro * (1f - num) + this.moveSpeedAggroMax * num;
		}
		else
		{
			num = this.moveSpeedAggroMax * num;
		}
		return EffectManager.GetValue(PassiveEffects.RunSpeed, null, num, this, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override float getNextStepSoundDistance()
	{
		if (!this.IsRunning)
		{
			return 0.5f;
		}
		return 1.5f;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void updateStepSound(float _distX, float _distZ)
	{
	}

	public override void MoveEntityHeaded(Vector3 _direction, bool _isDirAbsolute)
	{
		if (this.walkType == 21 && this.Jumping)
		{
			this.motion = this.accumulatedRootMotion;
			this.accumulatedRootMotion = Vector3.zero;
			this.IsRotateToGroundFlat = true;
			if (this.moveHelper != null)
			{
				Vector3 vector = this.moveHelper.JumpToPos - this.position;
				if (Utils.FastAbs(vector.y) < 0.2f)
				{
					this.motion.y = vector.y * 0.2f;
				}
				if (Utils.FastAbs(vector.x) < 0.3f)
				{
					this.motion.x = vector.x * 0.2f;
				}
				if (Utils.FastAbs(vector.z) < 0.3f)
				{
					this.motion.z = vector.z * 0.2f;
				}
				if (vector.sqrMagnitude < 0.0100000007f)
				{
					if (this.emodel && this.emodel.avatarController)
					{
						this.emodel.avatarController.StartAnimationJump(AnimJumpMode.Land);
					}
					this.Jumping = false;
				}
			}
			this.entityCollision(this.motion);
			return;
		}
		base.MoveEntityHeaded(_direction, _isDirAbsolute);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateJump()
	{
		if (this.walkType == 21 && !this.isSwimming)
		{
			base.FaceJumpTo();
			this.jumpState = EntityAlive.JumpState.Climb;
			if (!this.emodel.avatarController || !this.emodel.avatarController.IsAnimationJumpRunning())
			{
				this.Jumping = false;
			}
			if (this.jumpTicks == 0 && this.accumulatedRootMotion.y > 0.005f)
			{
				this.jumpTicks = 30;
			}
			return;
		}
		base.UpdateJump();
		if (this.isSwimming)
		{
			return;
		}
		this.accumulatedRootMotion.y = 0f;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void EndJump()
	{
		base.EndJump();
		this.IsRotateToGroundFlat = false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool ExecuteFallBehavior(EntityAlive.FallBehavior behavior, float _distance, Vector3 _fallMotion)
	{
		if (behavior == null || !this.emodel)
		{
			return false;
		}
		AvatarController avatarController = this.emodel.avatarController;
		if (!avatarController)
		{
			return false;
		}
		avatarController.UpdateInt("RandomSelector", this.rand.RandomRange(0, 64), true);
		switch (behavior.ResponseOp)
		{
		case EntityAlive.FallBehavior.Op.None:
			avatarController.UpdateInt(AvatarController.jumpLandResponseHash, -1, true);
			break;
		case EntityAlive.FallBehavior.Op.Land:
			avatarController.UpdateInt(AvatarController.jumpLandResponseHash, 0, true);
			break;
		case EntityAlive.FallBehavior.Op.LandLow:
			avatarController.UpdateInt(AvatarController.jumpLandResponseHash, 1, true);
			break;
		case EntityAlive.FallBehavior.Op.LandHard:
			avatarController.UpdateInt(AvatarController.jumpLandResponseHash, 2, true);
			break;
		case EntityAlive.FallBehavior.Op.Stumble:
			avatarController.UpdateInt(AvatarController.jumpLandResponseHash, 3, true);
			break;
		case EntityAlive.FallBehavior.Op.Ragdoll:
			this.emodel.DoRagdoll(this.rand.RandomFloat * 2f, EnumBodyPartHit.None, _fallMotion * 20f, Vector3.zero, false);
			break;
		}
		if (this.attackTarget != null && behavior.RagePer.IsSet() && behavior.RageTime.IsSet() && this.StartRage(behavior.RagePer.Random(this.rand), behavior.RageTime.Random(this.rand)))
		{
			avatarController.StartAnimationRaging();
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool ExecuteDestroyBlockBehavior(EntityAlive.DestroyBlockBehavior behavior, ItemActionAttack.AttackHitInfo attackHitInfo)
	{
		if (behavior == null || attackHitInfo == null || this.moveHelper == null || this.emodel == null || this.emodel.avatarController == null)
		{
			return false;
		}
		if (this.walkType == 21)
		{
			return false;
		}
		this.moveHelper.ClearBlocked();
		this.moveHelper.ClearTempMove();
		this.emodel.avatarController.UpdateInt("RandomSelector", this.rand.RandomRange(0, 64), true);
		switch (behavior.ResponseOp)
		{
		case EntityAlive.DestroyBlockBehavior.Op.Ragdoll:
			this.emodel.avatarController.BeginStun(EnumEntityStunType.StumbleBreakThroughRagdoll, EnumBodyPartHit.LeftUpperLeg, Utils.EnumHitDirection.None, false, 1f);
			base.SetStun(EnumEntityStunType.StumbleBreakThroughRagdoll);
			break;
		case EntityAlive.DestroyBlockBehavior.Op.Stumble:
			this.emodel.avatarController.BeginStun(EnumEntityStunType.StumbleBreakThrough, EnumBodyPartHit.LeftUpperLeg, Utils.EnumHitDirection.None, false, 1f);
			base.SetStun(EnumEntityStunType.StumbleBreakThrough);
			this.bodyDamage.StunDuration = 1f;
			break;
		}
		if (this.attackTarget != null && behavior.RagePer.IsSet() && behavior.RageTime.IsSet())
		{
			this.StartRage(behavior.RagePer.Random(this.rand), behavior.RageTime.Random(this.rand));
		}
		return true;
	}

	public override int DamageEntity(DamageSource _damageSource, int _strength, bool _criticalHit, float impulseScale)
	{
		if (_damageSource.GetDamageType() == EnumDamageTypes.Falling)
		{
			_strength = (_strength + 1) / 2;
			int num = (this.GetMaxHealth() + 2) / 3;
			if (_strength > num)
			{
				_strength = num;
			}
		}
		return base.DamageEntity(_damageSource, _strength, _criticalHit, impulseScale);
	}

	public override void ProcessDamageResponseLocal(DamageResponse _dmResponse)
	{
		base.ProcessDamageResponseLocal(_dmResponse);
		if (!this.isEntityRemote)
		{
			int @int = GameStats.GetInt(EnumGameStats.GameDifficulty);
			float num = (float)_dmResponse.Strength / 40f;
			if (num > 1f)
			{
				num = Mathf.Pow(num, 0.29f);
			}
			float num2 = EntityHuman.rageChances[@int] * num;
			if (this.rand.RandomFloat < num2)
			{
				if (this.rand.RandomFloat < EntityHuman.superRageChances[@int])
				{
					this.StartRage(2f, 30f);
					this.PlayOneShot(this.GetSoundAlert(), false, false, false);
					return;
				}
				this.StartRage(0.5f + this.rand.RandomFloat * 0.5f, 4f + this.rand.RandomFloat * 6f);
			}
		}
	}

	public bool StartRage(float speedPercent, float time)
	{
		if (speedPercent >= this.moveSpeedRagePer)
		{
			this.moveSpeedRagePer = speedPercent;
			this.moveSpeedScaleTime = time;
			return true;
		}
		return false;
	}

	public void StopRage()
	{
		this.moveSpeedRagePer = 0f;
		this.moveSpeedScaleTime = 0f;
	}

	public override void OnEntityDeath()
	{
		base.OnEntityDeath();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override Vector3i dropCorpseBlock()
	{
		if (this.lootContainer != null && this.lootContainer.IsUserAccessing())
		{
			return Vector3i.zero;
		}
		Vector3i vector3i = base.dropCorpseBlock();
		if (vector3i == Vector3i.zero)
		{
			return Vector3i.zero;
		}
		TileEntityLootContainer tileEntityLootContainer = this.world.GetTileEntity(0, vector3i) as TileEntityLootContainer;
		if (tileEntityLootContainer == null)
		{
			return Vector3i.zero;
		}
		if (this.lootContainer != null)
		{
			tileEntityLootContainer.CopyLootContainerDataFromOther(this.lootContainer);
		}
		else
		{
			tileEntityLootContainer.lootListName = this.lootListOnDeath;
			tileEntityLootContainer.SetContainerSize(LootContainer.GetLootContainer(this.lootListOnDeath, true).size, true);
		}
		tileEntityLootContainer.SetModified();
		return vector3i;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void AnalyticsSendDeath(DamageResponse _dmResponse)
	{
		DamageSource source = _dmResponse.Source;
		if (source == null)
		{
			return;
		}
		string name;
		if (source.BuffClass != null)
		{
			name = source.BuffClass.Name;
		}
		else
		{
			if (source.ItemClass == null)
			{
				return;
			}
			name = source.ItemClass.Name;
		}
		GameSparksCollector.IncrementCounter(GameSparksCollector.GSDataKey.ZombiesKilledBy, name, 1, true, GameSparksCollector.GSDataCollection.SessionUpdates);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void TurnIntoCrawler()
	{
		BoxCollider component = base.gameObject.GetComponent<BoxCollider>();
		if (component)
		{
			component.center = new Vector3(0f, 0.35f, 0f);
			component.size = new Vector3(0.8f, 0.8f, 0.8f);
		}
		base.SetupBounds();
		this.boundingBox.center = this.boundingBox.center + this.position;
		this.bCanClimbLadders = false;
	}

	public override void BuffAdded(BuffValue _buff)
	{
		if (_buff.BuffClass.DamageType == EnumDamageTypes.Electrical)
		{
			this.Electrocuted = true;
		}
	}

	public ulong timeToDie;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float moveSpeedRagePer;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float moveSpeedScaleTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float fallTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float fallThresholdTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static float[] moveSpeeds = new float[]
	{
		0f,
		0.35f,
		0.7f,
		1f,
		1.35f
	};

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static float[] moveRageSpeeds = new float[]
	{
		0.75f,
		0.8f,
		0.9f,
		1.15f,
		1.7f
	};

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static float[] moveSuperRageSpeeds = new float[]
	{
		0.88f,
		0.92f,
		1f,
		1.2f,
		1.7f
	};

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static float[] rageChances = new float[]
	{
		0f,
		0.15f,
		0.3f,
		0.35f,
		0.4f,
		0.5f
	};

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static float[] superRageChances = new float[]
	{
		0f,
		0.01f,
		0.03f,
		0.05f,
		0.08f,
		0.15f
	};
}

using System;
using System.Collections.Generic;
using Assets.DuckType.Jiggle;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class AvatarZombieController : AvatarController
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		base.Awake();
		this.modelT = EModelBase.FindModel(base.transform);
		this.assignStates();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Start()
	{
		this.hitLayerIndex = 3;
		if (!this.mainZombieMaterial)
		{
			EModelBase emodel = this.entity.emodel;
			if (emodel)
			{
				Transform meshTransform = emodel.meshTransform;
				if (meshTransform)
				{
					Renderer component = meshTransform.GetComponent<Renderer>();
					if (component)
					{
						this.mainZombieMaterial = component.sharedMaterial;
						this.mainZombieMaterial.name = this.mainZombieMaterial.name + "(local)";
						this.logDismemberment("load main zombie mat: " + this.mainZombieMaterial.name);
						bool flag = this.entity.HasAnyTags(DismembermentManager.radiatedTag) && (this.mainZombieMaterial.HasProperty("_IsRadiated") || this.mainZombieMaterial.HasProperty("_Irradiated"));
						DismembermentManager instance = DismembermentManager.Instance;
						this.gibCapMaterial = ((!flag) ? instance.GibCapsMaterial : instance.GibCapsRadMaterial);
						this.logDismemberment("load cap zombie mat: " + this.mainZombieMaterial.name);
					}
				}
			}
		}
	}

	public override void SwitchModelAndView(string _modelName, bool _bFPV, bool _bMale)
	{
		this.bipedT = this.modelT.Find(_modelName);
		this.FindBodyParts();
		base.SetAnimator(this.bipedT);
		this._setBool(AvatarController.isMaleHash, _bMale, true);
		if (this.entity.RootMotion)
		{
			AvatarRootMotion avatarRootMotion = this.bipedT.GetComponent<AvatarRootMotion>();
			if (avatarRootMotion == null)
			{
				avatarRootMotion = this.bipedT.gameObject.AddComponent<AvatarRootMotion>();
			}
			avatarRootMotion.Init(this, this.anim);
		}
		this.SetWalkType(this.entity.GetWalkType(), false);
		this._setBool(AvatarController.isDeadHash, this.entity.IsDead(), true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FindBodyParts()
	{
		this.neck = this.bipedT.FindInChilds("Neck", false);
		this.headAccessoriesT = this.bipedT.Find("HeadAccessories");
		this.rightHandT = this.bipedT.FindInChilds(this.entity.GetRightHandTransformName(), false);
		this.leftUpperLeg = this.bipedT.FindInChilds("LeftUpLeg", false);
		this.leftLowerLeg = this.bipedT.FindInChilds("LeftLeg", false);
		this.rightUpperLeg = this.bipedT.FindInChilds("RightUpLeg", false);
		this.rightLowerLeg = this.bipedT.FindInChilds("RightLeg", false);
		this.leftUpperArm = this.bipedT.FindInChilds("LeftArm", false);
		this.leftLowerArm = this.bipedT.FindInChilds("LeftForeArm", false);
		this.rightUpperArm = this.bipedT.FindInChilds("RightArm", false);
		this.rightLowerArm = this.bipedT.FindInChilds("RightForeArm", false);
		this.neckGore = GameUtils.FindTagInChilds(this.bipedT, "L_HeadGore");
		this.leftUpperArmGore = GameUtils.FindTagInChilds(this.bipedT, "L_LeftUpperArmGore");
		this.leftLowerArmGore = GameUtils.FindTagInChilds(this.bipedT, "L_LeftLowerArmGore");
		this.rightUpperArmGore = GameUtils.FindTagInChilds(this.bipedT, "L_RightUpperArmGore");
		this.rightLowerArmGore = GameUtils.FindTagInChilds(this.bipedT, "L_RightLowerArmGore");
		this.leftUpperLegGore = GameUtils.FindTagInChilds(this.bipedT, "L_LeftUpperLegGore");
		this.leftLowerLegGore = GameUtils.FindTagInChilds(this.bipedT, "L_LeftLowerLegGore");
		this.rightUpperLegGore = GameUtils.FindTagInChilds(this.bipedT, "L_RightUpperLegGore");
		this.rightLowerLegGore = GameUtils.FindTagInChilds(this.bipedT, "L_RightLowerLegGore");
	}

	public override void SetVisible(bool _b)
	{
		if (this.isVisible != _b || !this.isVisibleInit)
		{
			this.isVisible = _b;
			this.isVisibleInit = true;
			Transform transform = this.bipedT;
			if (transform)
			{
				Renderer[] componentsInChildren = transform.GetComponentsInChildren<Renderer>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].enabled = _b;
				}
			}
		}
	}

	public override Transform GetActiveModelRoot()
	{
		return this.modelT;
	}

	public override Transform GetRightHandTransform()
	{
		return this.rightHandT;
	}

	public override void SetInRightHand(Transform _transform)
	{
		this.idleTime = 0f;
		if (_transform)
		{
			Quaternion identity = Quaternion.identity;
			_transform.SetParent(this.GetRightHandTransform(), false);
			if (this.entity.inventory != null && this.entity.inventory.holdingItem != null)
			{
				AnimationGunjointOffsetData.AnimationGunjointOffsets animationGunjointOffsets = AnimationGunjointOffsetData.AnimationGunjointOffset[this.entity.inventory.holdingItem.HoldType.Value];
				_transform.localPosition = animationGunjointOffsets.position;
				_transform.localRotation = Quaternion.Euler(animationGunjointOffsets.rotation);
				return;
			}
			_transform.localPosition = Vector3.zero;
			_transform.localRotation = identity;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Update()
	{
		base.Update();
		if (this.attackPlayingTime > 0f)
		{
			this.attackPlayingTime -= Time.deltaTime;
			if (this.attackPlayingTime <= 0f)
			{
				this.isAttackImpact = true;
			}
		}
		if (this.timeUseAnimationPlaying > 0f)
		{
			this.timeUseAnimationPlaying -= Time.deltaTime;
		}
		if (this.timeSpecialAttack2Playing > 0f)
		{
			this.timeSpecialAttack2Playing -= Time.deltaTime;
		}
		if (!this.isVisible && (!this.entity || !this.entity.RootMotion || this.entity.isEntityRemote))
		{
			return;
		}
		if (!this.bipedT || !this.bipedT.gameObject.activeInHierarchy)
		{
			return;
		}
		if (!this.anim || !this.anim.avatar.isValid || !this.anim.enabled)
		{
			return;
		}
		this.UpdateLayerStateInfo();
		this.SetLayerWeights();
		int value = this.entity.inventory.holdingItem.HoldType.Value;
		this._setInt(AvatarController.weaponHoldTypeHash, value, true);
		float num;
		this.TryGetFloat(AvatarController.forwardHash, out num);
		float num2;
		this.TryGetFloat(AvatarController.strafeHash, out num2);
		this.targetSpeedForward = this.entity.speedForward;
		this.targetSpeedStrafe = this.entity.speedStrafe;
		if (!this.entity.IsFlyMode.Value)
		{
			num = Mathf.Lerp(num, this.targetSpeedForward, Time.deltaTime * this.forwardSpeedLerpMultiplier);
			num2 = Mathf.Lerp(num2, this.targetSpeedStrafe, Time.deltaTime * this.strafeSpeedLerpMultiplier);
		}
		float num3 = num2;
		if (num3 >= 1234f)
		{
			num3 = 0f;
		}
		this._setFloat(AvatarController.forwardHash, num, false);
		this._setFloat(AvatarController.strafeHash, num3, false);
		if (!this.entity.IsDead())
		{
			if (this.movementStateOverride != -1)
			{
				this._setInt(AvatarController.movementStateHash, this.movementStateOverride, true);
				this.movementStateOverride = -1;
			}
			else if (num2 >= 1234f)
			{
				this._setInt(AvatarController.movementStateHash, 4, false);
			}
			else
			{
				float num4 = num * num + num3 * num3;
				this._setInt(AvatarController.movementStateHash, (num4 > this.entity.moveSpeedAggro * this.entity.moveSpeedAggro) ? 3 : ((num4 > this.entity.moveSpeed * this.entity.moveSpeed) ? 2 : ((num4 > 0.001f) ? 1 : 0)), false);
			}
		}
		if (Mathf.Abs(num) > 0.01f || Mathf.Abs(num2) > 0.01f)
		{
			this.idleTime = 0f;
			this._setBool(AvatarController.isMovingHash, true, false);
		}
		else
		{
			this._setBool(AvatarController.isMovingHash, false, false);
		}
		this._setFloat(AvatarController.rotationPitchHash, this.entity.rotation.x, true);
		base.SendAnimParameters(0.05f);
		if (this.electrocuteTime > 0.3f && !this.entity.emodel.IsRagdollActive)
		{
			this._setTrigger(AvatarController.isElectrocutedHash, true);
		}
		if (this.timeSpecialAttackPlaying > 0f)
		{
			this.timeSpecialAttackPlaying -= Time.deltaTime;
		}
		if (this.timeRagePlaying > 0f)
		{
			this.timeRagePlaying -= Time.deltaTime;
		}
		if (!this.bipedT.gameObject.activeInHierarchy)
		{
			return;
		}
		if (this.entity.IsInElevator() || this.entity.Climbing)
		{
			this._setBool(AvatarController.isClimbingHash, true, true);
			this._setFloat(AvatarController.verticalSpeedHash, this.entity.speedVertical, true);
			return;
		}
		this._setBool(AvatarController.isClimbingHash, false, true);
		this._setFloat(AvatarController.verticalSpeedHash, 0f, true);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void LateUpdate()
	{
		if (!this.entity || !this.bipedT || !this.bipedT.gameObject.activeInHierarchy)
		{
			return;
		}
		if (!this.anim || !this.anim.enabled)
		{
			return;
		}
		this.UpdateLayerStateInfo();
		ItemClass holdingItem = this.entity.inventory.holdingItem;
		if (holdingItem.Actions[0] != null)
		{
			holdingItem.Actions[0].UpdateNozzleParticlesPosAndRot(this.entity.inventory.holdingItemData.actionData[0]);
		}
		if (holdingItem.Actions[1] != null)
		{
			holdingItem.Actions[1].UpdateNozzleParticlesPosAndRot(this.entity.inventory.holdingItemData.actionData[1]);
		}
		int fullPathHash = this.baseStateInfo.fullPathHash;
		bool flag = this.anim.IsInTransition(0);
		if (!flag)
		{
			this.isJumpStarted = false;
			if (fullPathHash == this.jumpState)
			{
				this._setBool(AvatarController.jumpHash, false, true);
			}
		}
		if (this.anim.GetBool(AvatarController.itemUseHash))
		{
			int num = this.itemUseTicks - 1;
			this.itemUseTicks = num;
			if (num <= 0)
			{
				this._setBool(AvatarController.itemUseHash, false, true);
			}
		}
		if (this.isInDeathAnim)
		{
			if (this.baseStateInfo.tagHash == AvatarController.deathHash && this.baseStateInfo.normalizedTime >= 1f && !flag)
			{
				this.isInDeathAnim = false;
				if (this.entity.HasDeathAnim)
				{
					this.entity.emodel.DoRagdoll(DamageResponse.New(true), 999999f);
				}
			}
			if (this.entity.HasDeathAnim && this.entity.RootMotion && this.entity.isCollidedHorizontally)
			{
				this.isInDeathAnim = false;
				this.entity.emodel.DoRagdoll(DamageResponse.New(true), 999999f);
			}
		}
		if (this.isCrawler && Time.time - this.crawlerTime > 2f)
		{
			this.isSuppressPain = false;
		}
		if (this.boneTransformOverrides.Count > 0)
		{
			for (int i = 0; i < this.boneTransformOverrides.Count; i++)
			{
				this.boneTransformOverrides[i].localRotation = Quaternion.identity;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateLayerStateInfo()
	{
		this.baseStateInfo = this.anim.GetCurrentAnimatorStateInfo(0);
		this.overrideStateInfo = this.anim.GetCurrentAnimatorStateInfo(1);
		this.fullBodyStateInfo = this.anim.GetCurrentAnimatorStateInfo(2);
		if (this.anim.layerCount > 3)
		{
			this.hitStateInfo = this.anim.GetCurrentAnimatorStateInfo(3);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetLayerWeights()
	{
		this.isSuppressPain = (this.isSuppressPain && (this.anim.IsInTransition(2) || this.fullBodyStateInfo.fullPathHash != 0));
		this.anim.SetLayerWeight(1, 1f);
		this.anim.SetLayerWeight(2, (float)((this.isSuppressPain || this.entity.bodyDamage.CurrentStun != EnumEntityStunType.None) ? 0 : 1));
	}

	public override void ResetAnimations()
	{
		base.ResetAnimations();
		this.anim.Play("None", 1, 0f);
		this.anim.Play("None", 2, 0f);
	}

	public override bool IsAnimationAttackPlaying()
	{
		return this.attackPlayingTime > 0f || this.overrideStateInfo.tagHash == AvatarController.attackHash || this.fullBodyStateInfo.tagHash == AvatarController.attackHash;
	}

	public override void StartAnimationAttack()
	{
		if (!this.bipedT.gameObject.activeInHierarchy)
		{
			return;
		}
		this.idleTime = 0f;
		this.isAttackImpact = false;
		this.attackPlayingTime = 2f;
		float randomFloat = this.entity.rand.RandomFloat;
		int num = -1;
		if (!this.rightArmDismembered)
		{
			num = 0;
			if (!this.leftArmDismembered)
			{
				num = (this.entity.rand.RandomInt & 1);
			}
		}
		else if (!this.leftArmDismembered)
		{
			num = 1;
		}
		int num2 = 8;
		if (num >= 0)
		{
			num2 = num;
		}
		int walkType = this.entity.GetWalkType();
		if (walkType >= 20)
		{
			num2 += walkType * 100;
		}
		if (this.entity.IsBreakingDoors && num >= 0)
		{
			num2 += 10;
		}
		if (num2 <= 1)
		{
			if (walkType == 1)
			{
				num2 += 100;
			}
			else if (this.entity.rand.RandomFloat < 0.25f)
			{
				num2 += 4;
			}
		}
		this._setInt(AvatarController.attackHash, num2, true);
		this._setFloat(AvatarController.attackBlendHash, randomFloat, true);
		this._setTrigger(AvatarController.attackTriggerHash, true);
	}

	public override void SetAttackImpact()
	{
		if (!this.isAttackImpact)
		{
			this.isAttackImpact = true;
			this.attackPlayingTime = 0.1f;
		}
	}

	public override bool IsAttackImpact()
	{
		return this.isAttackImpact;
	}

	public override bool IsAnimationUsePlaying()
	{
		return this.timeUseAnimationPlaying > 0f;
	}

	public override void StartAnimationUse()
	{
		this.idleTime = 0f;
		this.itemUseTicks = 3;
		this.timeUseAnimationPlaying = 0.3f;
		if (this.bipedT == null || !this.bipedT.gameObject.activeInHierarchy)
		{
			return;
		}
		this._setBool(AvatarController.itemUseHash, true, true);
	}

	public override bool IsAnimationHitRunning()
	{
		if (this.hitWeight == 0f)
		{
			return false;
		}
		int tagHash = this.hitStateInfo.tagHash;
		return tagHash == AvatarController.hitStartHash || (tagHash == AvatarController.hitHash && this.hitStateInfo.normalizedTime < 0.55f) || this.anim.IsInTransition(3);
	}

	public override bool IsAnimationSpecialAttackPlaying()
	{
		return this.timeSpecialAttackPlaying > 0f;
	}

	public override void StartAnimationSpecialAttack(bool _b, int _animType)
	{
		if (_b)
		{
			this.idleTime = 0f;
			this._setInt(AvatarController.attackHash, _animType, true);
			this._setTrigger(AvatarController.specialAttackHash, true);
			this.timeSpecialAttackPlaying = 0.3f;
		}
	}

	public override bool IsAnimationSpecialAttack2Playing()
	{
		return this.timeSpecialAttack2Playing > 0f;
	}

	public override void StartAnimationSpecialAttack2()
	{
		this.idleTime = 0f;
		this.timeSpecialAttack2Playing = 0.3f;
		this._setTrigger(AvatarController.specialAttack2Hash, true);
	}

	public override bool IsAnimationRagingPlaying()
	{
		return this.timeRagePlaying > 0f;
	}

	public override void StartAnimationRaging()
	{
		this.idleTime = 0f;
		this._setTrigger(AvatarController.rageHash, true);
		this.timeRagePlaying = 0.3f;
	}

	public override void StartAnimationElectrocute(float _duration)
	{
		base.StartAnimationElectrocute(_duration);
		this.idleTime = 0f;
	}

	public override bool IsAnimationDigRunning()
	{
		return AvatarController.digHash == this.baseStateInfo.tagHash;
	}

	public override void StartAnimationDodge(float _blend)
	{
		this._setFloat(AvatarController.dodgeBlendHash, _blend, true);
		this._setBool(AvatarController.dodgeTriggerHash, true, true);
	}

	public override void StartAnimationJumping()
	{
		this.idleTime = 0f;
		if (this.bipedT == null || !this.bipedT.gameObject.activeInHierarchy)
		{
			return;
		}
		if (this.anim != null)
		{
			this._setBool(AvatarController.jumpHash, true, true);
		}
	}

	public override void StartAnimationJump(AnimJumpMode jumpMode)
	{
		this.idleTime = 0f;
		if (this.bipedT == null || !this.bipedT.gameObject.activeInHierarchy)
		{
			return;
		}
		this.isJumpStarted = true;
		if (this.anim != null)
		{
			if (jumpMode == AnimJumpMode.Start)
			{
				this._setTrigger(AvatarController.jumpStartHash, true);
				return;
			}
			this._setTrigger(AvatarController.jumpLandHash, true);
			this._setInt(AvatarController.jumpLandResponseHash, 0, true);
		}
	}

	public override bool IsAnimationJumpRunning()
	{
		return this.isJumpStarted || AvatarController.jumpHash == this.baseStateInfo.tagHash;
	}

	public override bool IsAnimationWithMotionRunning()
	{
		int tagHash = this.baseStateInfo.tagHash;
		return tagHash == AvatarController.jumpHash || tagHash == AvatarController.moveHash;
	}

	public override void SetSwim(bool _enable)
	{
		int walkType = -1;
		if (!_enable)
		{
			walkType = this.entity.GetWalkType();
		}
		else
		{
			this._setFloat(AvatarController.swimSelectHash, this.entity.rand.RandomFloat, true);
		}
		this.SetWalkType(walkType, true);
	}

	public override void BeginStun(EnumEntityStunType stun, EnumBodyPartHit _bodyPart, Utils.EnumHitDirection _hitDirection, bool _criticalHit, float random)
	{
		this._setInt(AvatarController.stunTypeHash, (int)stun, true);
		this._setInt(AvatarController.stunBodyPartHash, (int)_bodyPart.ToPrimary().LowerToUpperLimb(), true);
		this._setInt(AvatarController.hitDirectionHash, (int)_hitDirection, true);
		this._setBool(AvatarController.isCriticalHash, _criticalHit, true);
		this._setFloat(AvatarController.HitRandomValueHash, random, true);
		this._setTrigger(AvatarController.beginStunTriggerHash, true);
		this._resetTrigger(AvatarController.endStunTriggerHash, true);
	}

	public override void EndStun()
	{
		this._setBool(AvatarController.isCriticalHash, false, true);
		this._setTrigger(AvatarController.endStunTriggerHash, true);
	}

	public override bool IsAnimationStunRunning()
	{
		return this.baseStateInfo.tagHash == AvatarController.stunHash;
	}

	public override void StartDeathAnimation(EnumBodyPartHit _bodyPart, int _movementState, float random)
	{
		this.idleTime = 0f;
		this.isInDeathAnim = true;
		this.didDeathTransition = false;
		if (this.bipedT == null || !this.bipedT.gameObject.activeInHierarchy)
		{
			return;
		}
		if (this.anim != null)
		{
			this.movementStateOverride = _movementState;
			this._setInt(AvatarController.movementStateHash, _movementState, true);
			this._setBool(AvatarController.isAliveHash, false, true);
			this._setInt(AvatarController.hitBodyPartHash, (int)_bodyPart.ToPrimary().LowerToUpperLimb(), true);
			this._setFloat(AvatarController.HitRandomValueHash, random, true);
			this.SetFallAndGround(false, this.entity.onGround);
		}
		if (this.bipedT == null || !this.bipedT.gameObject.activeInHierarchy)
		{
			return;
		}
		if (this.anim != null)
		{
			this._setTrigger(AvatarController.deathTriggerHash, true);
		}
	}

	public override void StartEating()
	{
		if (!this.isEating)
		{
			this._setInt(AvatarController.attackHash, 0, true);
			this._setTrigger(AvatarController.beginCorpseEatHash, true);
			this.isEating = true;
		}
	}

	public override void StopEating()
	{
		if (this.isEating)
		{
			this._setInt(AvatarController.attackHash, 0, true);
			this._setTrigger(AvatarController.endCorpseEatHash, true);
			this.isEating = false;
		}
	}

	public override void StartAnimationHit(EnumBodyPartHit _bodyPart, int _dir, int _hitDamage, bool _criticalHit, int _movementState, float _random, float _duration)
	{
		if (!this.isCrawler || Time.time - this.crawlerTime > 2f)
		{
			this.InternalStartAnimationHit(_bodyPart, _dir, _hitDamage, _criticalHit, _movementState, _random, _duration);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InternalStartAnimationHit(EnumBodyPartHit _bodyPart, int _dir, int _hitDamage, bool _criticalHit, int _movementState, float random, float _duration)
	{
		if (this.bipedT == null || !this.bipedT.gameObject.activeInHierarchy)
		{
			return;
		}
		if (!base.CheckHit(_duration))
		{
			this.SetDataFloat(AvatarController.DataTypes.HitDuration, _duration, true);
			return;
		}
		this.idleTime = 0f;
		if (this.anim)
		{
			this.movementStateOverride = _movementState;
			this._setInt(AvatarController.movementStateHash, _movementState, true);
			this._setBool(AvatarController.isCriticalHash, _criticalHit, true);
			this._setInt(AvatarController.hitDirectionHash, _dir, true);
			this._setInt(AvatarController.hitDamageHash, _hitDamage, true);
			this._setFloat(AvatarController.HitRandomValueHash, random, true);
			this._setInt(AvatarController.hitBodyPartHash, (int)_bodyPart.ToPrimary().LowerToUpperLimb(), true);
			this.SetDataFloat(AvatarController.DataTypes.HitDuration, _duration, true);
			this._setTrigger(AvatarController.hitTriggerHash, true);
		}
	}

	public bool IsCrippled
	{
		get
		{
			return this.isCrippled;
		}
	}

	public override void CrippleLimb(BodyDamage _bodyDamage, bool restoreState)
	{
		if (this.isCrippled)
		{
			return;
		}
		if (_bodyDamage.bodyPartHit.IsLeg())
		{
			int walkType = this.entity.GetWalkType();
			if (walkType != 5 && walkType < 20)
			{
				this.isCrippled = true;
				this.SetWalkType(5, false);
				this._setTrigger(AvatarController.movementTriggerHash, true);
			}
		}
	}

	public bool rightArmDismembered
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.rightUpperArmDismembered || this.rightLowerArmDismembered;
		}
	}

	public bool leftArmDismembered
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.leftUpperArmDismembered || this.leftLowerArmDismembered;
		}
	}

	public override void RemoveLimb(BodyDamage _bodyDamage, bool restoreState)
	{
		EnumBodyPartHit bodyPartHit = _bodyDamage.bodyPartHit;
		EnumDamageTypes damageType = _bodyDamage.damageType;
		if (!this.headDismembered && (bodyPartHit & EnumBodyPartHit.Head) > EnumBodyPartHit.None)
		{
			this.headDismembered = true;
			if (this.entity.OverrideHeadSize != 1f)
			{
				damageType = EnumDamageTypes.Piercing;
			}
			this.MakeDismemberedPart(1U, damageType, this.neck, this.neckGore, restoreState);
			if (this.headAccessoriesT)
			{
				this.headAccessoriesT.gameObject.SetActive(false);
			}
		}
		if (!this.leftUpperLegDismembered && (bodyPartHit & EnumBodyPartHit.LeftUpperLeg) > EnumBodyPartHit.None)
		{
			this.leftUpperLegDismembered = true;
			this.MakeDismemberedPart(32U, damageType, this.leftUpperLeg, this.leftUpperLegGore, restoreState);
		}
		if (!this.leftLowerLegDismembered && !this.leftUpperLegDismembered && (bodyPartHit & EnumBodyPartHit.LeftLowerLeg) > EnumBodyPartHit.None)
		{
			this.leftLowerLegDismembered = true;
			this.MakeDismemberedPart(64U, damageType, this.leftLowerLeg, this.leftLowerLegGore, restoreState);
		}
		if (!this.rightUpperLegDismembered && (bodyPartHit & EnumBodyPartHit.RightUpperLeg) > EnumBodyPartHit.None)
		{
			this.rightUpperLegDismembered = true;
			this.MakeDismemberedPart(128U, damageType, this.rightUpperLeg, this.rightUpperLegGore, restoreState);
		}
		if (!this.rightLowerLegDismembered && !this.rightUpperLegDismembered && (bodyPartHit & EnumBodyPartHit.RightLowerLeg) > EnumBodyPartHit.None)
		{
			this.rightLowerLegDismembered = true;
			this.MakeDismemberedPart(256U, damageType, this.rightLowerLeg, this.rightLowerLegGore, restoreState);
		}
		if (!this.leftUpperArmDismembered && (bodyPartHit & EnumBodyPartHit.LeftUpperArm) > EnumBodyPartHit.None)
		{
			this.leftUpperArmDismembered = true;
			this.MakeDismemberedPart(2U, damageType, this.leftUpperArm, this.leftUpperArmGore, restoreState);
		}
		if (!this.leftLowerArmDismembered && !this.leftUpperArmDismembered && (bodyPartHit & EnumBodyPartHit.LeftLowerArm) > EnumBodyPartHit.None)
		{
			this.leftLowerArmDismembered = true;
			this.MakeDismemberedPart(4U, damageType, this.leftLowerArm, this.leftLowerArmGore, restoreState);
		}
		if (!this.rightUpperArmDismembered && (bodyPartHit & EnumBodyPartHit.RightUpperArm) > EnumBodyPartHit.None)
		{
			this.rightUpperArmDismembered = true;
			this.MakeDismemberedPart(8U, damageType, this.rightUpperArm, this.rightUpperArmGore, restoreState);
		}
		if (!this.rightLowerArmDismembered && !this.rightUpperArmDismembered && (bodyPartHit & EnumBodyPartHit.RightLowerArm) > EnumBodyPartHit.None)
		{
			this.rightLowerArmDismembered = true;
			this.MakeDismemberedPart(16U, damageType, this.rightLowerArm, this.rightLowerArmGore, restoreState);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform SpawnLimbGore(Transform parent, string path, bool restoreState)
	{
		if (!parent || string.IsNullOrEmpty(path))
		{
			return null;
		}
		string assetBundlePath = DismembermentManager.GetAssetBundlePath(path);
		GameObject gameObject = DataLoader.LoadAsset<GameObject>(assetBundlePath);
		if (!gameObject)
		{
			this.logDismemberment(string.Format("{0} SpawnLimbGore prefab not found in asset bundle. path: {1}", this.entity.EntityName, assetBundlePath));
			return null;
		}
		this.logDismemberment(string.Format("{0} SpawnLimbGore loaded prefab in asset bundle. path: {1}", this.entity.EntityName, assetBundlePath));
		GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(gameObject, parent);
		GorePrefab component = gameObject2.GetComponent<GorePrefab>();
		if (component)
		{
			component.restoreState = restoreState;
		}
		return gameObject2.transform;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ProcDismemberedPart(Transform t, Transform partT, DismemberedPartData part, uint bodyDamageFlag)
	{
		Transform transform = partT.FindRecursive(part.targetBone);
		if (transform)
		{
			if (!part.attachToParent)
			{
				Vector3 localScale = t.localScale;
				localScale.x /= Utils.FastMax(0.01f, transform.localScale.x);
				localScale.y /= Utils.FastMax(0.01f, transform.localScale.y);
				localScale.z /= Utils.FastMax(0.01f, transform.localScale.z);
				t.localScale = localScale;
			}
			if (part.alignToBone)
			{
				t.localPosition = transform.localPosition;
			}
			if (!string.IsNullOrEmpty(part.childTargetObj))
			{
				Transform transform2 = new GameObject("scaleTarget").transform;
				transform2.position = transform.position;
				for (int i = 0; i < transform.childCount; i++)
				{
					transform.GetChild(i).SetParent(transform2);
				}
				transform2.SetParent(transform.parent);
				transform.SetParent(transform2);
				transform2.localScale = Vector3.zero;
			}
			if (part.snapToChild)
			{
				Transform child = transform.GetChild(0);
				if (child)
				{
					t.position = child.position;
					t.localEulerAngles += transform.localEulerAngles;
					t.localPosition += new Vector3(0f, transform.transform.localPosition.y, 0f);
				}
			}
		}
		if (part.hasRotOffset)
		{
			t.localEulerAngles = part.rot;
		}
		if (DismembermentManager.DebugShowArmRotations)
		{
			DismembermentManager.AddDebugArmObjects(partT, t);
		}
		if (!part.alignToBone && !part.snapToChild)
		{
			t.localPosition = Vector3.zero;
		}
		if (part.offset != Vector3.zero)
		{
			Transform transform3 = t.FindRecursive("pos");
			if (transform3)
			{
				transform3.localPosition += part.offset;
			}
		}
		if (part.particlePaths != null)
		{
			for (int j = 0; j < part.particlePaths.Length; j++)
			{
				string text = part.particlePaths[j];
				if (!string.IsNullOrEmpty(text))
				{
					DismembermentManager.SpawnParticleEffect(new ParticleEffect(text, t.position + Origin.position, Quaternion.identity, 1f, Color.white), -1);
				}
			}
		}
		Transform transform4 = t.FindRecursive("pos");
		if (transform4)
		{
			Renderer[] componentsInChildren = transform4.GetComponentsInChildren<Renderer>(true);
			Material altMaterial = this.entity.emodel.AltMaterial;
			if (altMaterial)
			{
				this.altMatName = altMaterial.name;
				for (int k = 0; k < this.altMatName.Length; k++)
				{
					char c = this.altMatName[k];
					if (char.IsDigit(c))
					{
						this.altEntityMatId = int.Parse(c.ToString());
						break;
					}
				}
			}
			else
			{
				foreach (char c2 in this.mainZombieMaterial.name)
				{
					if (char.IsDigit(c2))
					{
						this.altEntityMatId = int.Parse(c2.ToString());
						break;
					}
				}
			}
			foreach (Renderer renderer in componentsInChildren)
			{
				if (!renderer.GetComponent<ParticleSystem>())
				{
					Material[] sharedMaterials = renderer.sharedMaterials;
					for (int n = 0; n < sharedMaterials.Length; n++)
					{
						Material material = sharedMaterials[n];
						string name2 = material.name;
						if ((!part.prefabPath.ContainsCaseInsensitive("head") || !name2.ContainsCaseInsensitive("hair")) && (!renderer.name.ContainsCaseInsensitive("eye") || material.HasProperty("_IsRadiated") || material.HasProperty("_Irradiated")))
						{
							bool flag = false;
							int num = 0;
							while (num < DismembermentManager.DefaultBundleGibs.Length)
							{
								flag = name2.ContainsCaseInsensitive(DismembermentManager.DefaultBundleGibs[num]);
								if (flag)
								{
									if (name2.ContainsCaseInsensitive("ZombieGibs_caps"))
									{
										if (!this.gibCapMaterialCopy)
										{
											this.gibCapMaterialCopy = UnityEngine.Object.Instantiate<Material>(this.gibCapMaterial);
											this.gibCapMaterialCopy.name = this.gibCapMaterial.name.Replace("(global)", "(local)");
										}
										sharedMaterials[n] = this.gibCapMaterialCopy;
										break;
									}
									break;
								}
								else
								{
									num++;
								}
							}
							if (!flag && material.name.Contains("HD_"))
							{
								if (!this.mainZombieMaterialCopy)
								{
									this.mainZombieMaterialCopy = UnityEngine.Object.Instantiate<Material>(this.mainZombieMaterial);
								}
								sharedMaterials[n] = this.mainZombieMaterialCopy;
								this.logDismemberment(string.Format("update {0} mat to match entity {1}", name2, this.mainZombieMaterialCopy.name));
							}
						}
					}
					renderer.materials = sharedMaterials;
				}
			}
		}
		if (this.entity.IsFeral && bodyDamageFlag == 1U)
		{
			this.setUpEyeMats(t);
			if (part.isDetachable)
			{
				Transform transform5 = t.FindRecursive("Detachable");
				if (transform5)
				{
					this.setUpEyeMats(transform5);
				}
			}
			Transform transform6 = t.FindRecursive("FeralFlame");
			if (transform6 && !this.entity.HasAnyTags(DismembermentManager.radiatedTag))
			{
				transform6.gameObject.SetActive(true);
				string text2 = "large_flames_LOD (3)";
				Transform transform7 = this.entity.transform.FindRecursive(text2);
				if (transform7)
				{
					transform7.gameObject.SetActive(false);
				}
				else
				{
					Log.Warning("entity {0} no longer has a child named {1}", new object[]
					{
						this.entity.name,
						text2
					});
				}
			}
		}
		if (!this.dismemberMat && !string.IsNullOrEmpty(this.subFolderDismemberEntityName))
		{
			string text3 = this.rootDismmemberDir + string.Format("/gibs_{0}", this.subFolderDismemberEntityName.ToLower());
			Material sharedMaterial = this.skinnedMeshRenderer.sharedMaterial;
			if (this.entity.HasAnyTags(DismembermentManager.radiatedTag) && (sharedMaterial.HasProperty("_IsRadiated") || sharedMaterial.HasProperty("_Irradiated")))
			{
				text3 += "_IsRadiated";
			}
			string str = text3;
			object obj = (this.altEntityMatId != -1) ? this.altEntityMatId : "";
			text3 = str + ((obj != null) ? obj.ToString() : null);
			text3 += ".mat";
			Material material2 = DataLoader.LoadAsset<Material>(text3);
			if (!material2)
			{
				if (part.useMask)
				{
					this.logDismemberment(string.Format(this.entity.EntityName + " dismemberMat not found in asset bundle. path: {0}", text3));
					return;
				}
			}
			else
			{
				this.dismemberMat = UnityEngine.Object.Instantiate<Material>(material2);
				this.skinnedMeshRenderer.material = this.dismemberMat;
				if (this.smrLODOne)
				{
					this.smrLODOne.material = this.dismemberMat;
				}
				if (this.smrLODTwo)
				{
					this.smrLODTwo.material = this.dismemberMat;
				}
				this.logDismemberment(this.entity.EntityName + string.Format(" dismemberMat loaded prefab in asset bundle. path: {0}", text3));
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setUpEyeMats(Transform t)
	{
		Transform transform = t.FindRecursive("NormalEye");
		Transform transform2 = t.FindRecursive("FeralEye");
		if (transform2 && !this.entity.HasAnyTags(DismembermentManager.radiatedTag))
		{
			if (transform)
			{
				transform.gameObject.SetActive(false);
			}
			transform2.gameObject.SetActive(true);
			return;
		}
		if (transform)
		{
			MeshRenderer component = transform.GetComponent<MeshRenderer>();
			if (component)
			{
				Material material = UnityEngine.Object.Instantiate<Material>(component.material);
				if (material.HasProperty("_IsRadiated"))
				{
					material.SetFloat("_IsRadiated", 1f);
				}
				if (material.HasProperty("_Irradiated"))
				{
					material.SetFloat("_Irradiated", 1f);
				}
				component.material = material;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void MakeDismemberedPart(uint bodyDamageFlag, EnumDamageTypes damageType, Transform partT, Transform goreT, bool restoreState)
	{
		DismemberedPartData dismemberedPartData = DismembermentManager.DismemberPart(bodyDamageFlag, damageType, this.entity, true);
		if (dismemberedPartData == null)
		{
			return;
		}
		if (partT)
		{
			if (!string.IsNullOrEmpty(dismemberedPartData.targetBone))
			{
				if (!this.skinnedMeshRenderer)
				{
					this.skinnedMeshRenderer = this.entity.emodel.meshTransform.GetComponent<SkinnedMeshRenderer>();
					Transform parent = this.skinnedMeshRenderer.transform.parent;
					for (int i = 0; i < parent.childCount; i++)
					{
						Transform child = parent.GetChild(i);
						if (child.name.ContainsCaseInsensitive("LOD1"))
						{
							this.smrLODOne = child.GetComponent<SkinnedMeshRenderer>();
						}
						if (child.name.ContainsCaseInsensitive("LOD2"))
						{
							this.smrLODTwo = child.GetComponent<SkinnedMeshRenderer>();
						}
					}
				}
				Transform transform = partT.FindRecursive(dismemberedPartData.targetBone);
				if (!transform)
				{
					transform = partT.FindParent(dismemberedPartData.targetBone);
				}
				Transform transform2 = new GameObject("DynamicGore").transform;
				if (!dismemberedPartData.attachToParent)
				{
					transform2.SetParent(transform);
				}
				else
				{
					transform2.SetParent(transform.parent);
				}
				transform2.localPosition = Vector3.zero;
				transform2.localRotation = Quaternion.identity;
				transform2.localScale = Vector3.one;
				if (dismemberedPartData.snapToChild)
				{
					Vector3 position = transform.localPosition;
					if (transform.childCount > 0)
					{
						position = transform.GetChild(0).position;
					}
					transform2.position = position;
				}
				goreT = transform2;
				if (!dismemberedPartData.useMask)
				{
					transform.localScale = dismemberedPartData.scale;
					this.scaleOutChildBones(transform);
				}
				else
				{
					Collider component = transform.GetComponent<Collider>();
					if (component)
					{
						component.enabled = false;
					}
					this.disableChildColliders(transform);
				}
				if (dismemberedPartData.overrideAnimationState)
				{
					this.boneTransformOverrides.Add(transform);
				}
			}
			else
			{
				partT.localScale = dismemberedPartData.scale;
			}
			if (!string.IsNullOrEmpty(dismemberedPartData.prefabPath))
			{
				if (string.IsNullOrEmpty(this.rootDismmemberDir) && dismemberedPartData.prefabPath.Contains("/"))
				{
					this.subFolderDismemberEntityName = dismemberedPartData.prefabPath.Remove(dismemberedPartData.prefabPath.IndexOf("/"));
					this.rootDismmemberDir = "@:Entities/Zombies/Dismemberment/" + this.subFolderDismemberEntityName;
				}
				Transform transform3 = this.SpawnLimbGore(goreT, dismemberedPartData.prefabPath, restoreState);
				if (transform3 && !string.IsNullOrEmpty(dismemberedPartData.targetBone))
				{
					this.ProcDismemberedPart(transform3, partT, dismemberedPartData, bodyDamageFlag);
					DismembermentPart dismembermentPart = new DismembermentPart(dismemberedPartData, bodyDamageFlag, damageType);
					this.dismemberedParts.Add(dismembermentPart);
					dismembermentPart.SetObj(transform3);
					Transform transform4 = partT.FindRecursive(dismemberedPartData.targetBone);
					if (!transform4)
					{
						transform4 = partT.FindParent(dismemberedPartData.targetBone);
					}
					dismembermentPart.SetTarget(transform4);
					if (dismemberedPartData.useMask)
					{
						if (dismemberedPartData.scaleOutLimb)
						{
							Transform transform5 = partT.FindRecursive(dismemberedPartData.targetBone);
							if (!transform5)
							{
								transform5 = partT.FindParent(dismemberedPartData.targetBone);
							}
							if (!string.IsNullOrEmpty(dismemberedPartData.solTarget))
							{
								transform5 = partT.FindRecursive(dismemberedPartData.solTarget);
								if (!transform5)
								{
									transform5 = partT.FindParent(dismemberedPartData.solTarget);
								}
							}
							this.scaleOutChildBones(transform5);
							if (dismemberedPartData.hasSolScale)
							{
								transform5.localScale = dismemberedPartData.solScale;
							}
						}
						else
						{
							this.scaleOutChildBones(transform4);
						}
						if (!dismemberedPartData.scaleOutLimb || !string.IsNullOrEmpty(dismemberedPartData.solTarget))
						{
							this.setLimbShaderProps(DismembermentManager.GetBodyPartHit(dismembermentPart.bodyDamageFlag), dismembermentPart);
						}
					}
					if (dismemberedPartData.snapToChild)
					{
						transform3.localPosition = Vector3.zero;
					}
					Transform entitiesTransform = GameManager.Instance.World.EntitiesTransform;
					if (!entitiesTransform)
					{
						return;
					}
					Transform transform6 = entitiesTransform.Find("DismemberedLimbs");
					if (!transform6)
					{
						transform6 = new GameObject("DismemberedLimbs").transform;
						transform6.SetParent(entitiesTransform);
						transform6.localPosition = Vector3.zero;
					}
					if (dismemberedPartData.isDetachable)
					{
						this.ActivateDetachableLimbs(bodyDamageFlag, damageType, transform3, transform6, dismembermentPart);
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void logDismemberment(string _log)
	{
		if (DismembermentManager.DebugLogEnabled)
		{
			Type type = base.GetType();
			Log.Out(((type != null) ? type.ToString() : null) + " " + _log);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void scaleOutChildBones(Transform _boneT)
	{
		if (_boneT.childCount > 0)
		{
			for (int i = 0; i < _boneT.childCount; i++)
			{
				Transform child = _boneT.GetChild(i);
				if (child && !child.name.Equals("DynamicGore"))
				{
					child.localScale = Vector3.zero;
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void disableChildColliders(Transform _boneT)
	{
		foreach (Collider collider in _boneT.GetComponentsInChildren<Collider>())
		{
			if (collider)
			{
				collider.enabled = false;
			}
		}
		foreach (CharacterJoint characterJoint in _boneT.GetComponentsInChildren<CharacterJoint>())
		{
			if (characterJoint)
			{
				Rigidbody component = characterJoint.GetComponent<Rigidbody>();
				UnityEngine.Object.Destroy(characterJoint);
				UnityEngine.Object.Destroy(component);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ActivateDetachableLimbs(uint bodyDamageFlag, EnumDamageTypes damageType, Transform partT, Transform parentT, DismembermentPart part)
	{
		Transform transform = partT.FindRecursive("Detachable");
		if (transform)
		{
			DetachedDismembermentPart detachedDismembermentPart = new DetachedDismembermentPart();
			EnumBodyPartHit bodyPartHit = DismembermentManager.GetBodyPartHit(bodyDamageFlag);
			GameObject gameObject = new GameObject(string.Format("{0}_{1}_{2}", this.entity.entityId, this.entity.EntityName, bodyPartHit));
			Transform transform2 = gameObject.transform;
			transform2.SetParent(parentT);
			detachedDismembermentPart.SetDetached(transform2);
			if (this.entity.IsBloodMoon)
			{
				detachedDismembermentPart.lifeTime /= 3f;
			}
			if (this.leftLowerArmDismembered && bodyDamageFlag == 2U)
			{
				DismembermentManager.ActivateDetachable(transform, "HalfArm");
				this.hideDismemberedPart(bodyDamageFlag);
			}
			if (this.leftLowerLegDismembered && bodyDamageFlag == 32U)
			{
				DismembermentManager.ActivateDetachable(transform, "HalfLeg");
				this.hideDismemberedPart(bodyDamageFlag);
			}
			if (this.rightLowerArmDismembered && bodyDamageFlag == 8U)
			{
				DismembermentManager.ActivateDetachable(transform, "HalfArm");
				this.hideDismemberedPart(bodyDamageFlag);
			}
			if (this.rightLowerLegDismembered && bodyDamageFlag == 128U)
			{
				DismembermentManager.ActivateDetachable(transform, "HalfLeg");
				this.hideDismemberedPart(bodyDamageFlag);
			}
			if (!transform.gameObject.activeSelf)
			{
				transform.gameObject.SetActive(true);
			}
			if (this.entity.OverrideHeadSize != 1f && this.headDismembered && bodyDamageFlag == 1U)
			{
				float headBigSize = this.entity.emodel.HeadBigSize;
				detachedDismembermentPart.overrideHeadSize = headBigSize;
				detachedDismembermentPart.overrideHeadDismemberScaleTime = this.entity.OverrideHeadDismemberScaleTime;
				Transform transform3 = transform.Find("Physics");
				Transform transform4 = new GameObject("pivot").transform;
				transform4.SetParent(transform3);
				transform4.localScale = Vector3.one;
				int i = 0;
				while (i < part.targetT.childCount)
				{
					Transform child = part.targetT.GetChild(i);
					if (child.CompareTag("E_BP_Head"))
					{
						Transform transform5 = transform.FindRecursive(bodyPartHit.ToString());
						if (transform5)
						{
							Renderer component = transform5.GetComponent<Renderer>();
							transform4.position = child.position + (component.bounds.center - child.position);
							detachedDismembermentPart.SetPivot(transform4);
							break;
						}
						transform4.position = child.position;
						detachedDismembermentPart.SetPivot(transform4);
						this.logDismemberment(string.Format("{0} is missing a child with the name {1}, unable to center piviot", gameObject.name, bodyPartHit));
						break;
					}
					else
					{
						i++;
					}
				}
				List<Transform> list = new List<Transform>();
				for (int j = 0; j < transform3.childCount; j++)
				{
					Transform child2 = transform3.GetChild(j);
					if (child2 != transform3)
					{
						list.Add(child2);
					}
				}
				for (int k = 0; k < list.Count; k++)
				{
					list[k].SetParent(transform4);
				}
				transform4.localScale = new Vector3(headBigSize, headBigSize, headBigSize);
			}
			transform.SetParent(transform2);
			DismembermentManager instance = DismembermentManager.Instance;
			if (instance != null)
			{
				instance.AddPart(detachedDismembermentPart);
			}
			string text = string.Empty;
			foreach (Renderer renderer in transform.GetComponentsInChildren<Renderer>())
			{
				if (renderer != null)
				{
					Material[] sharedMaterials = renderer.sharedMaterials;
					for (int m = 0; m < sharedMaterials.Length; m++)
					{
						Material material = sharedMaterials[m];
						if (material != null)
						{
							text = material.name;
							if ((!part.prefabPath.ContainsCaseInsensitive("head") || !text.ContainsCaseInsensitive("hair")) && (!renderer.name.ContainsCaseInsensitive("eye") || material.HasProperty("_IsRadiated") || material.HasProperty("_Irradiated")))
							{
								if (text.ContainsCaseInsensitive("ZombieGibs_caps"))
								{
									sharedMaterials[m] = this.gibCapMaterial;
								}
								if (text.Contains("HD_"))
								{
									sharedMaterials[m] = this.mainZombieMaterial;
								}
								sharedMaterials[m].DisableKeyword("_ELECTRIC_SHOCK_ON");
							}
						}
					}
					renderer.sharedMaterials = sharedMaterials;
				}
			}
			Jiggle[] componentsInChildren2 = transform.GetComponentsInChildren<Jiggle>(true);
			for (int n = 0; n < componentsInChildren2.Length; n++)
			{
				componentsInChildren2[n].enabled = true;
			}
			Rigidbody componentInChildren = transform.GetComponentInChildren<Rigidbody>();
			if (componentInChildren)
			{
				Vector3 vector = Vector3.up * this.entity.lastHitForce;
				float num = Vector3.Angle(this.entity.GetForwardVector(), this.entity.lastHitImpactDir);
				componentInChildren.AddTorque(Quaternion.FromToRotation(this.entity.GetForwardVector(), this.entity.lastHitImpactDir).eulerAngles * (1f + num / 90f), ForceMode.Impulse);
				componentInChildren.AddForce((this.entity.lastHitImpactDir + vector) * this.entity.lastHitForce, ForceMode.Impulse);
				string damageTag = DismembermentManager.getDamageTag(damageType, this.entity.lastHitRanged);
				if (damageTag == "blunt")
				{
					if (damageType == EnumDamageTypes.Piercing)
					{
						componentInChildren.AddForce(this.entity.lastHitImpactDir + vector, ForceMode.Impulse);
					}
					else
					{
						componentInChildren.AddForce(this.entity.lastHitImpactDir * this.entity.lastHitForce * 1.5f + vector * 1.25f, ForceMode.Impulse);
					}
				}
				if (damageTag == "blade")
				{
					float num2 = Vector3.Dot(this.entity.GetForwardVector(), this.entity.lastHitImpactDir);
					float num3 = Vector3.Dot(this.entity.GetForwardVector(), this.entity.lastHitEntityFwd);
					componentInChildren.AddForce((num2 < num3) ? (-this.entity.transform.right * this.entity.lastHitForce + vector) : (this.entity.transform.right * this.entity.lastHitForce + vector), ForceMode.Impulse);
					componentInChildren.AddTorque(Quaternion.FromToRotation(this.entity.GetForwardVector(), this.entity.lastHitImpactDir).eulerAngles * (1f + num / 90f) * this.entity.lastHitForce, ForceMode.Impulse);
				}
				if (damageType == EnumDamageTypes.Heat)
				{
					float d = 2.67f;
					componentInChildren.AddForce(this.entity.lastHitImpactDir * d + Vector3.up * d * 0.67f, ForceMode.Impulse);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Material GetMainZombieBodyMaterial()
	{
		EModelBase emodel = this.entity.emodel;
		if (emodel)
		{
			Transform meshTransform = emodel.meshTransform;
			if (meshTransform)
			{
				return meshTransform.GetComponent<Renderer>().sharedMaterial;
			}
		}
		return null;
	}

	public override void Electrocute(bool enabled)
	{
		base.Electrocute(enabled);
		if (enabled)
		{
			Material mainZombieBodyMaterial = this.GetMainZombieBodyMaterial();
			if (mainZombieBodyMaterial)
			{
				mainZombieBodyMaterial.EnableKeyword("_ELECTRIC_SHOCK_ON");
			}
			if (this.dismemberMat)
			{
				this.dismemberMat.EnableKeyword("_ELECTRIC_SHOCK_ON");
			}
			if (this.mainZombieMaterialCopy)
			{
				this.mainZombieMaterialCopy.EnableKeyword("_ELECTRIC_SHOCK_ON");
			}
			if (this.gibCapMaterialCopy)
			{
				this.gibCapMaterialCopy.EnableKeyword("_ELECTRIC_SHOCK_ON");
			}
			this.StartAnimationElectrocute(0.6f);
			return;
		}
		Material mainZombieBodyMaterial2 = this.GetMainZombieBodyMaterial();
		if (mainZombieBodyMaterial2)
		{
			mainZombieBodyMaterial2.DisableKeyword("_ELECTRIC_SHOCK_ON");
		}
		if (this.dismemberMat)
		{
			this.dismemberMat.DisableKeyword("_ELECTRIC_SHOCK_ON");
		}
		if (this.mainZombieMaterialCopy)
		{
			this.mainZombieMaterialCopy.DisableKeyword("_ELECTRIC_SHOCK_ON");
		}
		if (this.gibCapMaterialCopy)
		{
			this.gibCapMaterialCopy.DisableKeyword("_ELECTRIC_SHOCK_ON");
		}
		this.StartAnimationElectrocute(0f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setLimbShaderProps(EnumBodyPartHit partHit, DismembermentPart part)
	{
		if (this.dismemberMat && !part.Data.maskOverride)
		{
			if (this.dismemberMat.HasProperty("_LeftLowerLeg") && (partHit & EnumBodyPartHit.LeftLowerLeg) > EnumBodyPartHit.None)
			{
				this.dismemberMat.SetFloat("_LeftLowerLeg", 1f);
			}
			if (this.dismemberMat.HasProperty("_LeftUpperLeg") && (partHit & EnumBodyPartHit.LeftUpperLeg) > EnumBodyPartHit.None)
			{
				this.dismemberMat.SetFloat("_LeftUpperLeg", 1f);
				if (!part.Data.scaleOutLimb)
				{
					this.dismemberMat.SetFloat("_LeftLowerLeg", 1f);
				}
			}
			if (this.dismemberMat.HasProperty("_RightLowerLeg") && (partHit & EnumBodyPartHit.RightLowerLeg) > EnumBodyPartHit.None)
			{
				this.dismemberMat.SetFloat("_RightLowerLeg", 1f);
			}
			if (this.dismemberMat.HasProperty("_RightUpperLeg") && (partHit & EnumBodyPartHit.RightUpperLeg) > EnumBodyPartHit.None)
			{
				this.dismemberMat.SetFloat("_RightUpperLeg", 1f);
				if (!part.Data.scaleOutLimb)
				{
					this.dismemberMat.SetFloat("_RightLowerLeg", 1f);
				}
			}
			if (this.dismemberMat.HasProperty("_LeftLowerArm") && (partHit & EnumBodyPartHit.LeftLowerArm) > EnumBodyPartHit.None && !part.Data.scaleOutLimb)
			{
				this.dismemberMat.SetFloat("_LeftLowerArm", 1f);
			}
			if (this.dismemberMat.HasProperty("_LeftUpperArm") && (partHit & EnumBodyPartHit.LeftUpperArm) > EnumBodyPartHit.None)
			{
				this.dismemberMat.SetFloat("_LeftUpperArm", 1f);
				if (!part.Data.scaleOutLimb)
				{
					this.dismemberMat.SetFloat("_LeftLowerArm", 1f);
				}
			}
			if (this.dismemberMat.HasProperty("_RightLowerArm") && (partHit & EnumBodyPartHit.RightLowerArm) > EnumBodyPartHit.None && !part.Data.scaleOutLimb)
			{
				this.dismemberMat.SetFloat("_RightLowerArm", 1f);
			}
			if (this.dismemberMat.HasProperty("_RightUpperArm") && (partHit & EnumBodyPartHit.RightUpperArm) > EnumBodyPartHit.None)
			{
				this.dismemberMat.SetFloat("_RightUpperArm", 1f);
				if (!part.Data.scaleOutLimb)
				{
					this.dismemberMat.SetFloat("_RightLowerArm", 1f);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void hideDismemberedPart(uint bodyDamageFlag)
	{
		uint lowerBodyPart = 0U;
		if (bodyDamageFlag == 2U)
		{
			lowerBodyPart = 4U;
		}
		if (bodyDamageFlag == 8U)
		{
			lowerBodyPart = 16U;
		}
		if (bodyDamageFlag == 32U)
		{
			lowerBodyPart = 64U;
		}
		if (bodyDamageFlag == 128U)
		{
			lowerBodyPart = 256U;
		}
		if (lowerBodyPart != 0U)
		{
			DismembermentPart dismembermentPart = this.dismemberedParts.Find((DismembermentPart p) => p.bodyDamageFlag == lowerBodyPart);
			if (dismembermentPart != null)
			{
				dismembermentPart.Hide();
			}
		}
	}

	public bool IsCrawler
	{
		get
		{
			return this.isCrawler;
		}
	}

	public override void TurnIntoCrawler(bool restoreState)
	{
		if (!this.isCrawler && this.entity.GetWalkType() != 21)
		{
			this.isCrawler = true;
			this.crawlerTime = Time.time;
			this.isSuppressPain = true;
			this._setInt(AvatarController.hitBodyPartHash, 0, true);
			this._setBool(AvatarController.isCriticalHash, false, true);
			this.SetWalkType(21, false);
			this._setTrigger(AvatarController.toCrawlerTriggerHash, true);
		}
	}

	public override void TriggerSleeperPose(int pose, bool returningToSleep = false)
	{
		if (returningToSleep)
		{
			base.TriggerSleeperPose(pose, returningToSleep);
			return;
		}
		if (this.anim != null)
		{
			this._setInt(AvatarController.sleeperPoseHash, pose, true);
			switch (pose)
			{
			case 0:
				this.anim.Play(AvatarController.sleeperIdleSitHash);
				return;
			case 1:
				this.anim.Play(AvatarController.sleeperIdleSideRightHash);
				return;
			case 2:
				this.anim.Play(AvatarController.sleeperIdleSideLeftHash);
				return;
			case 3:
				this.anim.Play(AvatarController.sleeperIdleBackHash);
				return;
			case 4:
				this.anim.Play(AvatarController.sleeperIdleStomachHash);
				return;
			case 5:
				this.anim.Play(AvatarController.sleeperIdleStandHash);
				return;
			default:
				this._setTrigger(AvatarController.sleeperTriggerHash, true);
				break;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cAnimSyncWaitTimeMax = 0.05f;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public const int cOverrideLayerIndex = 1;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public const int cFullBodyLayerIndex = 2;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public const int cHitLayerIndex = 3;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform modelT;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform bipedT;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform rightHandT;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform neck;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform leftUpperArm;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform leftLowerArm;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform rightLowerArm;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform rightUpperArm;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform leftUpperLeg;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform leftLowerLeg;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform rightLowerLeg;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform rightUpperLeg;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform neckGore;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform leftUpperArmGore;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform leftLowerArmGore;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform rightUpperArmGore;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform rightLowerArmGore;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform leftUpperLegGore;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform leftLowerLegGore;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform rightLowerLegGore;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform rightUpperLegGore;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform headAccessoriesT;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public AnimatorStateInfo baseStateInfo;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public AnimatorStateInfo overrideStateInfo;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public AnimatorStateInfo fullBodyStateInfo;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public AnimatorStateInfo hitStateInfo;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool isSuppressPain;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool isCrippled;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool isCrawler;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool isVisibleInit;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool isVisible;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float idleTime;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float crawlerTime;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float attackPlayingTime;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool isAttackImpact;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float timeSpecialAttackPlaying;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float timeSpecialAttack2Playing;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float timeUseAnimationPlaying;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public int itemUseTicks;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float timeRagePlaying;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int jumpState;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isJumpStarted;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isEating;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public int movementStateOverride = -1;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool headDismembered;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool leftUpperArmDismembered;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool leftLowerArmDismembered;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool rightUpperArmDismembered;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool rightLowerArmDismembered;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool leftUpperLegDismembered;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool leftLowerLegDismembered;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool rightUpperLegDismembered;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool rightLowerLegDismembered;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool isInDeathAnim;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool didDeathTransition;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Material mainZombieMaterial;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Material mainZombieMaterialCopy;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Material gibCapMaterial;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Material gibCapMaterialCopy;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Material dismemberMat;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public SkinnedMeshRenderer skinnedMeshRenderer;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public SkinnedMeshRenderer smrLODOne;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public SkinnedMeshRenderer smrLODTwo;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string rootDismmemberDir;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string subFolderDismemberEntityName;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int altEntityMatId = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string altMatName;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<Transform> boneTransformOverrides = new List<Transform>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<DismembermentPart> dismemberedParts = new List<DismembermentPart>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const string cElectrocuteKeyword = "_ELECTRIC_SHOCK_ON";
}

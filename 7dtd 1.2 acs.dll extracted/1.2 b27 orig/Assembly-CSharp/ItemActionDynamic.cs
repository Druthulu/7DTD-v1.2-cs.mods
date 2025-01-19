﻿using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionDynamic : ItemAction
{
	public override void ReadFrom(DynamicProperties _props)
	{
		base.ReadFrom(_props);
		foreach (KeyValuePair<string, string> keyValuePair in _props.Values.Dict)
		{
			if (keyValuePair.Key.StartsWith("ToolCategory."))
			{
				this.ToolBonuses[keyValuePair.Key.Substring("ToolCategory.".Length)] = new ItemActionAttack.Bonuses(StringParsers.ParseFloat(_props.Values[keyValuePair.Key], 0, -1, NumberStyles.Any), _props.Params1.ContainsKey(keyValuePair.Key) ? StringParsers.ParseFloat(_props.Params1[keyValuePair.Key], 0, -1, NumberStyles.Any) : 2f);
			}
		}
		if (_props.Values.ContainsKey("Damage_type"))
		{
			this.DamageType = EnumUtils.Parse<EnumDamageTypes>(_props.Values["Damage_type"], false);
		}
		else if (_props.Values.ContainsKey("DamageType"))
		{
			this.DamageType = EnumUtils.Parse<EnumDamageTypes>(_props.Values["DamageType"], false);
		}
		else
		{
			this.DamageType = EnumDamageTypes.Bashing;
		}
		this.RangeDefault = 2f;
		_props.ParseFloat("Range", ref this.RangeDefault);
		this.UsePowerAttackAnimation = (this.ActionIndex == 1);
		this.UsePowerAttackTriggers = (this.ActionIndex == 1);
		if (_props.Values.ContainsKey("UsePowerAttackAnimation"))
		{
			this.UsePowerAttackAnimation = StringParsers.ParseBool(_props.Values["UsePowerAttackAnimation"], 0, -1, true);
		}
		if (_props.Values.ContainsKey("UsePowerAttackTriggers"))
		{
			this.UsePowerAttackTriggers = StringParsers.ParseBool(_props.Values["UsePowerAttackTriggers"], 0, -1, true);
		}
		if (_props.Values.ContainsKey("UseGrazingHits"))
		{
			this.UseGrazingHits = StringParsers.ParseBool(_props.Values["UseGrazingHits"], 0, -1, true);
		}
		else
		{
			this.UseGrazingHits = false;
		}
		if (_props.Values.ContainsKey("GrazeDamagePercentage"))
		{
			this.GrazeDamagePercentage = StringParsers.ParseFloat(_props.Values["GrazeDamagePercentage"], 0, -1, NumberStyles.Any);
		}
		else
		{
			this.GrazeDamagePercentage = 0.1f;
		}
		if (_props.Values.ContainsKey("GrazeStaminaPercentage"))
		{
			this.GrazeStaminaPercentage = StringParsers.ParseFloat(_props.Values["GrazeStaminaPercentage"], 0, -1, NumberStyles.Any);
		}
		else
		{
			this.GrazeStaminaPercentage = 0.01f;
		}
		if (_props.Values.ContainsKey("Sphere"))
		{
			this.SphereRadius = StringParsers.ParseFloat(_props.Values["Sphere"], 0, -1, NumberStyles.Any);
		}
		else
		{
			this.SphereRadius = 0f;
		}
		if (_props.Values.ContainsKey("GrazeStart"))
		{
			this.GrazeStart = StringParsers.ParseFloat(_props.Values["GrazeStart"], 0, -1, NumberStyles.Any);
		}
		else
		{
			this.GrazeStart = -0.15f;
		}
		if (_props.Values.ContainsKey("GrazeEnd"))
		{
			this.GrazeEnd = StringParsers.ParseFloat(_props.Values["GrazeEnd"], 0, -1, NumberStyles.Any);
		}
		else
		{
			this.GrazeEnd = 0.15f;
		}
		if (_props.Values.ContainsKey("IsVerticalSwing"))
		{
			this.IsVerticalSwing = StringParsers.ParseBool(_props.Values["IsVerticalSwing"], 0, -1, true);
		}
		else
		{
			this.IsVerticalSwing = false;
		}
		if (_props.Values.ContainsKey("IsHorizontalSwing"))
		{
			this.IsHorizontalSwing = StringParsers.ParseBool(_props.Values["IsHorizontalSwing"], 0, -1, true);
		}
		else
		{
			this.IsHorizontalSwing = false;
		}
		if (_props.Values.ContainsKey("InvertSwing"))
		{
			this.InvertSwing = StringParsers.ParseBool(_props.Values["InvertSwing"], 0, -1, true);
		}
		else
		{
			this.InvertSwing = false;
		}
		if (_props.Values.ContainsKey("SwingDegrees"))
		{
			this.SwingDegrees = StringParsers.ParseFloat(_props.Values["SwingDegrees"], 0, -1, NumberStyles.Any);
		}
		else
		{
			this.SwingDegrees = 65f;
		}
		if (_props.Values.ContainsKey("SwingAngle"))
		{
			this.SwingAngle = StringParsers.ParseFloat(_props.Values["SwingAngle"], 0, -1, NumberStyles.Any);
		}
		else
		{
			this.SwingAngle = 0f;
		}
		_props.ParseInt("EntityPenetrationCount", ref this.EntityPenetrationCount);
		this.harvestHitEffectOn = true;
		_props.ParseBool("HarvestHitEffectOn", ref this.harvestHitEffectOn);
		if (_props.Classes.ContainsKey("HitSounds"))
		{
			this.HitSoundOverrides = new Dictionary<string, string>();
			for (int i = 0; i < 10; i++)
			{
				string text = "Override" + i.ToString();
				if (_props.Classes["HitSounds"].Values.ContainsKey(text) && _props.Classes["HitSounds"].Params1.ContainsKey(text))
				{
					this.HitSoundOverrides[_props.Classes["HitSounds"].Values[text]] = _props.Classes["HitSounds"].Params1[text];
				}
			}
		}
		if (_props.Classes.ContainsKey("GrazeSounds"))
		{
			this.GrazeSoundOverrides = new Dictionary<string, string>();
			for (int j = 0; j < 10; j++)
			{
				string text2 = "Override" + j.ToString();
				if (_props.Classes["GrazeSounds"].Values.ContainsKey(text2) && _props.Classes["GrazeSounds"].Params1.ContainsKey(text2))
				{
					this.GrazeSoundOverrides[_props.Classes["GrazeSounds"].Values[text2]] = _props.Classes["GrazeSounds"].Params1[text2];
				}
			}
		}
		this.hitmaskOverride = Voxel.ToHitMask(_props.GetString("Hitmask_override"));
	}

	public override void ExecuteAction(ItemActionData _actionData, bool _bReleased)
	{
	}

	public override void OnHoldingUpdate(ItemActionData _actionData)
	{
	}

	public override bool IsActionRunning(ItemActionData _actionData)
	{
		return false;
	}

	public override ItemActionData CreateModifierData(ItemInventoryData _invData, int _indexInEntityOfAction)
	{
		return new ItemActionDynamic.ItemActionDynamicData(_invData, _indexInEntityOfAction);
	}

	public virtual bool GrazeCast(ItemActionDynamic.ItemActionDynamicData _actionData, float normalizedClipTime = -1f)
	{
		return false;
	}

	public virtual bool Raycast(ItemActionDynamic.ItemActionDynamicData _actionData)
	{
		return false;
	}

	public override WorldRayHitInfo GetExecuteActionTarget(ItemActionData _actionData)
	{
		float sphereRadius = this.SphereRadius;
		ItemActionDynamic.ItemActionDynamicData itemActionDynamicData = (ItemActionDynamic.ItemActionDynamicData)_actionData;
		EntityAlive holdingEntity = itemActionDynamicData.invData.holdingEntity;
		if (!itemActionDynamicData.useExistingRay)
		{
			itemActionDynamicData.ray = holdingEntity.GetLookRay();
			if (holdingEntity.IsBreakingBlocks)
			{
				if (itemActionDynamicData.ray.direction.y < 0f)
				{
					itemActionDynamicData.ray.direction = new Vector3(itemActionDynamicData.ray.direction.x, 0f, itemActionDynamicData.ray.direction.z);
					ItemActionDynamic.ItemActionDynamicData itemActionDynamicData2 = itemActionDynamicData;
					itemActionDynamicData2.ray.origin = itemActionDynamicData2.ray.origin + new Vector3(0f, -0.7f, 0f);
				}
			}
			else if (holdingEntity.GetAttackTarget() != null)
			{
				Vector3 direction = holdingEntity.GetAttackTargetHitPosition() - itemActionDynamicData.ray.origin;
				itemActionDynamicData.ray = new Ray(itemActionDynamicData.ray.origin, direction);
			}
			ItemActionDynamic.ItemActionDynamicData itemActionDynamicData3 = itemActionDynamicData;
			itemActionDynamicData3.ray.origin = itemActionDynamicData3.ray.origin - sphereRadius * itemActionDynamicData.ray.direction;
			itemActionDynamicData.rayStartPos = itemActionDynamicData.ray.origin;
		}
		itemActionDynamicData.useExistingRay = false;
		this.lastModelLayer = holdingEntity.GetModelLayer();
		holdingEntity.SetModelLayer(2, false, null);
		ItemValue itemValue = itemActionDynamicData.invData.itemValue;
		ItemClass holdingItem = holdingEntity.inventory.holdingItem;
		FastTags<TagGroup.Global> fastTags = _actionData.ActionTags;
		fastTags |= ((holdingItem == null) ? ItemActionAttack.MeleeTag : holdingItem.ItemTags);
		this.Range = EffectManager.GetValue(PassiveEffects.MaxRange, itemValue, this.RangeDefault, holdingEntity, null, fastTags, true, true, true, true, true, 1, true, false);
		this.BlockRange = EffectManager.GetValue(PassiveEffects.BlockRange, itemValue, this.Range, holdingEntity, null, fastTags, true, true, true, true, true, 1, true, false);
		float num = Utils.FastMax(this.Range, this.BlockRange) + sphereRadius;
		num -= (itemActionDynamicData.ray.origin - itemActionDynamicData.rayStartPos).magnitude;
		if (holdingEntity is EntityEnemy && holdingEntity.IsBreakingBlocks)
		{
			Voxel.Raycast(itemActionDynamicData.invData.world, itemActionDynamicData.ray, num, 1073807360, (this.hitmaskOverride == 0) ? 128 : this.hitmaskOverride, 0.4f);
		}
		else
		{
			EntityAlive x = null;
			int layerMask = -538767381;
			if (Voxel.Raycast(itemActionDynamicData.invData.world, itemActionDynamicData.ray, num, layerMask, (this.hitmaskOverride == 0) ? 128 : this.hitmaskOverride, this.SphereRadius))
			{
				x = (ItemActionAttack.GetEntityFromHit(Voxel.voxelRayHitInfo) as EntityAlive);
			}
			if (x == null)
			{
				Voxel.Raycast(itemActionDynamicData.invData.world, itemActionDynamicData.ray, num, -538488837, (this.hitmaskOverride == 0) ? 128 : this.hitmaskOverride, this.SphereRadius);
			}
		}
		holdingEntity.SetModelLayer(this.lastModelLayer, false, null);
		return _actionData.GetUpdatedHitInfo();
	}

	public WorldRayHitInfo[] GetExecuteActionGrazeTarget(ItemActionData _actionData, float normalizedClipTime = -1f)
	{
		List<WorldRayHitInfo> list = new List<WorldRayHitInfo>();
		normalizedClipTime = Mathf.Clamp((normalizedClipTime - this.GrazeStart) / (this.GrazeEnd - this.GrazeStart), 0f, 1f);
		float num = this.SphereRadius * 1.25f;
		if (num == 0f)
		{
			num = 0.15f;
		}
		float num2 = num;
		float b = -(this.SwingDegrees * 0.5f);
		float a = this.SwingDegrees * 0.5f;
		ItemActionDynamic.ItemActionDynamicData itemActionDynamicData = (ItemActionDynamic.ItemActionDynamicData)_actionData;
		EntityAlive holdingEntity = itemActionDynamicData.invData.holdingEntity;
		Ray lookRay = itemActionDynamicData.invData.holdingEntity.GetLookRay();
		lookRay.direction = Quaternion.AngleAxis(Mathf.Lerp(a, b, normalizedClipTime), (holdingEntity as EntityPlayerLocal).cameraTransform.right) * lookRay.direction;
		if (this.SwingAngle != 0f)
		{
			lookRay.direction = Quaternion.AngleAxis(this.SwingAngle, (holdingEntity as EntityPlayerLocal).cameraTransform.forward) * lookRay.direction;
		}
		float num3 = EffectManager.GetValue(PassiveEffects.MaxRange, itemActionDynamicData.invData.itemValue, 2f, holdingEntity, null, itemActionDynamicData.ActionTags, true, true, true, true, true, 1, true, false) + num2;
		lookRay.origin -= lookRay.direction * num2;
		Vector3 vector = lookRay.origin + lookRay.direction * num3;
		if (itemActionDynamicData.lastWeaponHeadPosition == Vector3.zero)
		{
			itemActionDynamicData.lastWeaponHeadPosition = vector;
			return list.ToArray();
		}
		float num4 = Vector3.Distance(vector, itemActionDynamicData.lastWeaponHeadPosition);
		Vector3 normalized = (vector - itemActionDynamicData.lastWeaponHeadPosition).normalized;
		Vector3 direction = itemActionDynamicData.invData.holdingEntity.GetLookRay().direction;
		this.lastModelLayer = holdingEntity.GetModelLayer();
		holdingEntity.SetModelLayer(2, false, null);
		Entity entity = null;
		Ray lookRay2 = itemActionDynamicData.invData.holdingEntity.GetLookRay();
		lookRay2.origin -= lookRay2.direction * this.SphereRadius;
		if (Voxel.Raycast(itemActionDynamicData.invData.world, lookRay2, num3, -538750981, (this.hitmaskOverride == 0) ? 128 : this.hitmaskOverride, this.SphereRadius))
		{
			WorldRayHitInfo updatedHitInfo = _actionData.GetUpdatedHitInfo();
			if (updatedHitInfo.tag != null && updatedHitInfo.tag.StartsWith("E_"))
			{
				entity = (ItemActionAttack.GetEntityFromHit(Voxel.voxelRayHitInfo) as EntityAlive);
				if (entity != null && !this.shouldIgnoreTarget(entity, _actionData.invData.holdingEntity, true))
				{
					itemActionDynamicData.alreadyHitEnts.Add(entity.entityId);
				}
			}
		}
		Debug.DrawLine(lookRay2.origin, lookRay2.origin + lookRay2.direction * num3, Color.green, Time.deltaTime);
		float num5 = -num;
		while (num5 < num4)
		{
			num5 += num;
			num5 = Mathf.Clamp(num5, 0f, num4);
			lookRay.direction = (itemActionDynamicData.lastWeaponHeadPosition + normalized * num5 - lookRay.origin).normalized;
			lookRay.origin = itemActionDynamicData.invData.holdingEntity.GetLookRay().origin - num2 * lookRay.direction;
			lookRay.origin + lookRay.direction * (num3 + num2);
			EntityAlive entityAlive = null;
			Color color = Color.red;
			if (Voxel.Raycast(itemActionDynamicData.invData.world, lookRay, num3, -538750981, (this.hitmaskOverride == 0) ? 128 : this.hitmaskOverride, num))
			{
				Vector3.Distance(Voxel.voxelRayHitInfo.hit.pos, lookRay.origin);
				entityAlive = (ItemActionAttack.GetEntityFromHit(Voxel.voxelRayHitInfo) as EntityAlive);
				if (entityAlive == null)
				{
					entityAlive = Voxel.voxelRayHitInfo.hitCollider.GetComponentInParent<EntityAlive>();
				}
				if (this.shouldIgnoreTarget(entityAlive, _actionData.invData.holdingEntity, true))
				{
					entityAlive = null;
				}
				if (entityAlive != null && entityAlive.IsAlive() && !itemActionDynamicData.alreadyHitEnts.Contains(entityAlive.entityId))
				{
					color = Color.green;
				}
				else
				{
					_actionData.invData.holdingEntity.FireEvent((_actionData.indexInEntityOfAction == 0) ? MinEventTypes.onSelfPrimaryActionGrazeMiss : MinEventTypes.onSelfSecondaryActionGrazeMiss, true);
				}
				if (entityAlive != null && entityAlive.IsAlive() && !(entityAlive is EntityVehicle))
				{
					EntityPlayerLocal entityPlayerLocal = holdingEntity as EntityPlayerLocal;
					if (entityPlayerLocal != null && (entityPlayerLocal.HitInfo.transform == null || !entityPlayerLocal.HitInfo.transform.IsChildOf(entityAlive.ModelTransform)) && Vector3.Angle(holdingEntity.transform.forward, entityAlive.transform.position - holdingEntity.transform.position) <= 30f)
					{
						entityPlayerLocal.MoveController.SetCameraSnapEntity(entityAlive, eCameraSnapMode.MeleeAttack);
					}
				}
			}
			Debug.DrawLine(lookRay.origin, lookRay.origin + lookRay.direction * num3, color, 2f);
			if (ItemActionDynamic.ShowDebugSwing)
			{
				ItemActionDynamic.DebugDisplayHits.Add(GameObject.CreatePrimitive(PrimitiveType.Sphere));
				ItemActionDynamic.DebugDisplayHits[ItemActionDynamic.DebugDisplayHits.Count - 1].transform.position = lookRay.origin + lookRay.direction * (num2 + 0.2f) + lookRay.direction * ((num3 - (num2 + 0.2f)) * 0.5f);
				ItemActionDynamic.DebugDisplayHits[ItemActionDynamic.DebugDisplayHits.Count - 1].transform.LookAt(lookRay.origin);
				ItemActionDynamic.DebugDisplayHits[ItemActionDynamic.DebugDisplayHits.Count - 1].transform.localScale = Vector3.right * num + Vector3.up * num + Vector3.forward * (num3 - (num2 + 0.2f));
				ItemActionDynamic.DebugDisplayHits[ItemActionDynamic.DebugDisplayHits.Count - 1].layer = 2;
				ItemActionDynamic.DebugDisplayHits[ItemActionDynamic.DebugDisplayHits.Count - 1].transform.position -= Origin.position;
				ItemActionDynamic.DebugDisplayHits[ItemActionDynamic.DebugDisplayHits.Count - 1].GetComponent<MeshRenderer>().material.SetColor("_Color", color);
			}
			if (this.isValidTarget(entityAlive, _actionData.invData.holdingEntity, true) && (entity == null || entityAlive.entityId != entity.entityId) && !itemActionDynamicData.alreadyHitEnts.Contains(entityAlive.entityId))
			{
				list.Add(Voxel.voxelRayHitInfo.Clone());
				itemActionDynamicData.alreadyHitEnts.Add(entityAlive.entityId);
			}
		}
		holdingEntity.SetModelLayer(this.lastModelLayer, false, null);
		itemActionDynamicData.lastWeaponHeadPosition = vector;
		return list.ToArray();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void hitTarget(ItemActionData _actionData, WorldRayHitInfo hitInfo, bool _isGrazingHit = false)
	{
		ItemActionDynamic.ItemActionDynamicData itemActionDynamicData = (ItemActionDynamic.ItemActionDynamicData)_actionData;
		EntityAlive holdingEntity = _actionData.invData.holdingEntity;
		this.tmpTag = _actionData.ActionTags;
		this.tmpTag |= ((holdingEntity.inventory.holdingItem == null) ? ItemActionAttack.MeleeTag : holdingEntity.inventory.holdingItem.ItemTags);
		this.tmpTag = (this.tmpTag | holdingEntity.CurrentStanceTag | holdingEntity.CurrentMovementTag);
		if (!_isGrazingHit && _actionData.invData.itemValue.MaxUseTimes > 0)
		{
			_actionData.invData.itemValue.UseTimes += EffectManager.GetValue(PassiveEffects.DegradationPerUse, _actionData.invData.itemValue, 1f, holdingEntity, null, this.tmpTag, true, true, true, true, true, 1, true, false);
			base.HandleItemBreak(_actionData);
		}
		_actionData.attackDetails.isCriticalHit = false;
		MinEventParams minEventContext = holdingEntity.MinEventContext;
		minEventContext.StartPosition = hitInfo.ray.origin;
		if (hitInfo.tag != null)
		{
			if (GameUtils.IsBlockOrTerrain(hitInfo.tag))
			{
				minEventContext.Other = null;
				minEventContext.BlockValue = hitInfo.fmcHit.blockValue;
				minEventContext.Position = hitInfo.hit.pos;
				itemActionDynamicData.alreadyHitBlocks.Add(hitInfo.fmcHit.blockPos);
				if (_isGrazingHit)
				{
					return;
				}
			}
			else if (hitInfo.tag.StartsWith("E_"))
			{
				minEventContext.Other = hitInfo.hitCollider.transform.GetComponentInParent<EntityAlive>();
				minEventContext.BlockValue = BlockValue.Air;
				minEventContext.Position = hitInfo.hit.pos;
			}
		}
		if (!_isGrazingHit)
		{
			holdingEntity.FireEvent((_actionData.indexInEntityOfAction == 0) ? MinEventTypes.onSelfPrimaryActionRayHit : MinEventTypes.onSelfSecondaryActionRayHit, true);
			_actionData.attackDetails.isCriticalHit = (_actionData.indexInEntityOfAction == 1);
		}
		else
		{
			holdingEntity.FireEvent((_actionData.indexInEntityOfAction == 0) ? MinEventTypes.onSelfPrimaryActionGrazeHit : MinEventTypes.onSelfSecondaryActionGrazeHit, true);
			_actionData.attackDetails.isCriticalHit = false;
		}
		_actionData.attackDetails.WeaponTypeTag = ItemActionAttack.MeleeTag;
		float blockDamage = this.GetDamageBlock(_actionData.invData.itemValue, ItemActionAttack.GetBlockHit(_actionData.invData.world, hitInfo), holdingEntity, _actionData.indexInEntityOfAction);
		float num = this.GetDamageEntity(_actionData.invData.itemValue, holdingEntity, _actionData.indexInEntityOfAction);
		if (_isGrazingHit)
		{
			blockDamage = 0f;
			num *= EffectManager.GetValue(PassiveEffects.GrazeDamageMultiplier, _actionData.invData.itemValue, this.GrazeDamagePercentage, holdingEntity, null, this.tmpTag, true, true, true, true, true, 1, true, false);
		}
		int num2 = 1;
		if (this.bUseParticleHarvesting && (this.particleHarvestingCategory == null || this.particleHarvestingCategory == this.item.MadeOfMaterial.id))
		{
			num2 |= 4;
		}
		if (!this.harvestHitEffectOn && itemActionDynamicData.IsHarvesting)
		{
			num2 |= 8;
		}
		ItemActionAttack.Hit(hitInfo, holdingEntity.entityId, this.DamageType, blockDamage, num, 1f, 1f, this.getCriticalChance(_actionData), ItemAction.GetDismemberChance(_actionData, hitInfo), this.item.MadeOfMaterial.SurfaceCategory, new DamageMultiplier(), new List<string>(), _actionData.attackDetails, num2, this.ActionExp, this.ActionExpBonusMultiplier, null, this.ToolBonuses, itemActionDynamicData.IsHarvesting ? ItemActionAttack.EnumAttackMode.RealAndHarvesting : ItemActionAttack.EnumAttackMode.RealNoHarvesting, _isGrazingHit ? this.GrazeSoundOverrides : this.HitSoundOverrides, -1, null);
		if (!_isGrazingHit)
		{
			GameUtils.HarvestOnAttack(_actionData, this.ToolBonuses);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public float getCriticalChance(ItemActionData _actionData)
	{
		return EffectManager.GetValue(PassiveEffects.CriticalChance, _actionData.invData.itemValue, 0f, _actionData.invData.holdingEntity, null, _actionData.ActionTags, true, true, true, true, true, 1, true, false);
	}

	public float GetDamageEntity(ItemValue _itemValue, EntityAlive _holdingEntity = null, int actionIndex = 0)
	{
		this.tmpTag = ((actionIndex == 0) ? ItemActionAttack.PrimaryTag : ItemActionAttack.SecondaryTag);
		this.tmpTag |= ((_itemValue.ItemClass == null) ? ItemActionAttack.MeleeTag : _itemValue.ItemClass.ItemTags);
		if (_holdingEntity != null)
		{
			this.tmpTag = (this.tmpTag | _holdingEntity.CurrentStanceTag | _holdingEntity.CurrentMovementTag);
		}
		return EffectManager.GetValue(PassiveEffects.EntityDamage, _itemValue, 0f, _holdingEntity, null, this.tmpTag, true, true, true, true, true, 1, true, false);
	}

	public float GetDamageBlock(ItemValue _itemValue, BlockValue _blockValue, EntityAlive _holdingEntity = null, int actionIndex = 0)
	{
		this.tmpTag = ((actionIndex == 0) ? ItemActionAttack.PrimaryTag : ItemActionAttack.SecondaryTag);
		this.tmpTag |= ((_itemValue.ItemClass == null) ? ItemActionAttack.MeleeTag : _itemValue.ItemClass.ItemTags);
		if (_holdingEntity != null)
		{
			this.tmpTag = (this.tmpTag | _holdingEntity.CurrentStanceTag | _holdingEntity.CurrentMovementTag);
		}
		this.tmpTag |= _blockValue.Block.Tags;
		float value = EffectManager.GetValue(PassiveEffects.BlockDamage, _itemValue, 0f, _holdingEntity, null, this.tmpTag, true, true, true, true, true, 1, true, false);
		return Utils.FastMin((float)_blockValue.Block.blockMaterial.MaxIncomingDamage, value);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool isHitValid(WorldRayHitInfo _hitInfo, ItemActionData _actionData, out EntityAlive _hitEntity)
	{
		_hitEntity = null;
		if (_hitInfo == null)
		{
			return false;
		}
		if (!_hitInfo.bHitValid)
		{
			return false;
		}
		ItemActionDynamic.ItemActionDynamicData itemActionDynamicData = (ItemActionDynamic.ItemActionDynamicData)_actionData;
		float sqrMagnitude = (_hitInfo.hit.pos - itemActionDynamicData.rayStartPos).sqrMagnitude;
		if (_hitInfo.tag != null && GameUtils.IsBlockOrTerrain(_hitInfo.tag) && sqrMagnitude > this.BlockRange * this.BlockRange)
		{
			return false;
		}
		if (_hitInfo.tag != null && _hitInfo.tag.StartsWith("E_"))
		{
			EntityAlive entityAlive = GameUtils.GetHitRootEntity(_hitInfo.tag, _hitInfo.transform) as EntityAlive;
			if (entityAlive == null)
			{
				return false;
			}
			if (this.shouldIgnoreTarget(entityAlive, _actionData.invData.holdingEntity, false))
			{
				return false;
			}
			if (sqrMagnitude > this.Range * this.Range)
			{
				return false;
			}
			_hitEntity = entityAlive;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isValidTarget(Entity _target, Entity _self, bool _deadInvalid = true)
	{
		return !this.shouldIgnoreTarget(_target, _self, _deadInvalid);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool shouldIgnoreTarget(Entity _target, Entity _self, bool _ignoreDead = true)
	{
		if (_target == null)
		{
			return true;
		}
		if (_ignoreDead && !_target.IsAlive())
		{
			return true;
		}
		if (_target.entityId == _self.entityId)
		{
			return true;
		}
		if (_target is EntityDrone)
		{
			return (_target as EntityDrone).isAlly(_self as EntityPlayer);
		}
		EntityPlayer entityPlayer = _self as EntityPlayer;
		EntityPlayer entityPlayer2 = _target as EntityPlayer;
		return entityPlayer != null && entityPlayer2 != null && !entityPlayer.FriendlyFireCheck(entityPlayer2);
	}

	public override ItemClass.EnumCrosshairType GetCrosshairType(ItemActionData _actionData)
	{
		if (this.isShowOverlay(_actionData))
		{
			return ItemClass.EnumCrosshairType.Damage;
		}
		return ItemClass.EnumCrosshairType.Plus;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isShowOverlay(ItemActionData _actionData)
	{
		EntityPlayerLocal entityPlayerLocal = _actionData.invData.holdingEntity as EntityPlayerLocal;
		if (entityPlayerLocal == null)
		{
			return false;
		}
		EntityAlive entityAlive = _actionData.attackDetails.entityHit as EntityAlive;
		if (entityAlive is EntityDrone && (entityPlayerLocal.PlayerUI.xui.Dialog.Respondent != null || (float)entityAlive.Health == entityAlive.Stats.Health.Max))
		{
			if (_actionData.uiOpenedByMe && XUiC_FocusedBlockHealth.IsWindowOpen(entityPlayerLocal.PlayerUI))
			{
				XUiC_FocusedBlockHealth.SetData(entityPlayerLocal.PlayerUI, null, 0f);
				_actionData.uiOpenedByMe = false;
			}
			return false;
		}
		if (!this.isShowOverlayInternal(_actionData))
		{
			return false;
		}
		WorldRayHitInfo hitInfo = _actionData.hitInfo;
		if (!hitInfo.bHitValid)
		{
			return false;
		}
		if (hitInfo.tag != null)
		{
			if (GameUtils.IsBlockOrTerrain(hitInfo.tag))
			{
				if (hitInfo.hit.distanceSq > this.BlockRange * this.BlockRange)
				{
					return false;
				}
				if (!hitInfo.hit.blockValue.Block.IsHealthShownInUI(hitInfo.hit.blockValue))
				{
					return false;
				}
			}
			if (hitInfo.tag.StartsWith("E_") && hitInfo.hit.distanceSq > this.Range * this.Range)
			{
				return false;
			}
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isShowOverlayInternal(ItemActionData actionData)
	{
		WorldRayHitInfo executeActionTarget = this.GetExecuteActionTarget(actionData);
		if (executeActionTarget == null)
		{
			return false;
		}
		if (!executeActionTarget.bHitValid)
		{
			return false;
		}
		bool flag = actionData.attackDetails.entityHit is EntityDrone;
		if (actionData.attackDetails.itemsToDrop == null && !flag)
		{
			return false;
		}
		if (actionData.attackDetails.bBlockHit)
		{
			return actionData.attackDetails.raycastHitPosition == executeActionTarget.hit.blockPos;
		}
		Entity hitRootEntity = GameUtils.GetHitRootEntity(executeActionTarget.tag, executeActionTarget.transform);
		return hitRootEntity && hitRootEntity == actionData.attackDetails.entityHit;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void getOverlayData(ItemActionData actionData, out float _perc, out string _text)
	{
		float num = actionData.attackDetails.damageTotalOfTarget;
		float num2 = (float)actionData.attackDetails.damageMax;
		if (actionData.attackDetails.bBlockHit)
		{
			BlockValue block = actionData.invData.world.GetBlock(actionData.attackDetails.hitPosition);
			num = (float)block.damage;
			num2 = (float)block.Block.GetShownMaxDamage();
		}
		else
		{
			EntityAlive entityAlive = actionData.attackDetails.entityHit as EntityAlive;
			if (entityAlive != null)
			{
				if (entityAlive is EntityDrone)
				{
					float num3 = (float)entityAlive.Health;
					float max = entityAlive.Stats.Health.Max;
					_perc = num3 / max;
					_text = string.Format("{0}/{1}", Utils.FastMax(0f, num3).ToCultureInvariantString("0"), max.ToCultureInvariantString());
					return;
				}
				num = (float)(-(float)entityAlive.DeathHealth);
				num2 = (float)EntityClass.list[entityAlive.entityClass].DeadBodyHitPoints;
			}
		}
		_perc = (num2 - num) / num2;
		_text = string.Format("{0}/{1}", Utils.FastMax(0f, num2 - num).ToCultureInvariantString("0"), num2.ToCultureInvariantString());
	}

	public override void OnHUD(ItemActionData actionData, int _x, int _y)
	{
		if (actionData == null)
		{
			return;
		}
		if (!this.canShowOverlay(actionData))
		{
			return;
		}
		EntityPlayerLocal entityPlayerLocal = actionData.invData.holdingEntity as EntityPlayerLocal;
		if (!entityPlayerLocal)
		{
			return;
		}
		LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(entityPlayerLocal);
		if (!this.isShowOverlay(actionData))
		{
			if (actionData.uiOpenedByMe && XUiC_FocusedBlockHealth.IsWindowOpen(uiforPlayer))
			{
				XUiC_FocusedBlockHealth.SetData(uiforPlayer, null, 0f);
				actionData.uiOpenedByMe = false;
				return;
			}
		}
		else
		{
			if (!XUiC_FocusedBlockHealth.IsWindowOpen(uiforPlayer))
			{
				actionData.uiOpenedByMe = true;
			}
			float num;
			string text;
			this.getOverlayData(actionData, out num, out text);
			if (num < 1f)
			{
				XUiC_FocusedBlockHealth.SetData(uiforPlayer, text, num);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool canShowOverlay(ItemActionData actionData)
	{
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public EnumDamageTypes DamageType;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Dictionary<string, ItemActionAttack.Bonuses> ToolBonuses = new Dictionary<string, ItemActionAttack.Bonuses>();

	[PublicizedFrom(EAccessModifier.Protected)]
	public Dictionary<string, string> HitSoundOverrides;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Dictionary<string, string> GrazeSoundOverrides;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int lastModelLayer;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float RangeDefault;

	[PublicizedFrom(EAccessModifier.Protected)]
	public new float Range;

	[PublicizedFrom(EAccessModifier.Protected)]
	public new float BlockRange;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool UsePowerAttackAnimation;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool UsePowerAttackTriggers;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int hitmaskOverride;

	public bool UseGrazingHits;

	public float GrazeStart;

	public float GrazeEnd;

	public float GrazeDamagePercentage;

	public float GrazeStaminaPercentage;

	public bool IsVerticalSwing;

	public bool IsHorizontalSwing;

	public bool InvertSwing;

	public float SwingDegrees;

	public float SwingAngle;

	public int EntityPenetrationCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool harvestHitEffectOn;

	public static bool ShowDebugSwing = false;

	public static List<GameObject> DebugDisplayHits = new List<GameObject>();

	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Global> tmpTag;

	public class ItemActionDynamicData : ItemActionAttackData
	{
		public ItemActionDynamicData(ItemInventoryData _invData, int _indexInEntityOfAction) : base(_invData, _indexInEntityOfAction)
		{
			this.alreadyHitEnts = new List<int>();
			this.alreadyHitBlocks = new List<Vector3i>();
			this.waterCollisionParticles.Init(_invData.holdingEntity.entityId, _invData.item.MadeOfMaterial.SurfaceCategory, "water", 16);
		}

		public Ray ray;

		public Vector3 rayStartPos;

		public bool useExistingRay;

		public bool IsHarvesting;

		public List<int> alreadyHitEnts;

		public List<Vector3i> alreadyHitBlocks;

		public Vector3 lastWeaponHeadPosition = Vector3.zero;

		public Vector3 lastWeaponHeadPositionDebug = Vector3.zero;

		public float lastClipPercentage = -1f;

		public float attackTime;

		public CollisionParticleController waterCollisionParticles = new CollisionParticleController();
	}
}

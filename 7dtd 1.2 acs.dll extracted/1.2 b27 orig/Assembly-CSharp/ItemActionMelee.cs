using System;
using System.Collections.Generic;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionMelee : ItemActionAttack
{
	public override ItemActionData CreateModifierData(ItemInventoryData _invData, int _indexInEntityOfAction)
	{
		return new ItemActionMelee.InventoryDataMelee(_invData, _indexInEntityOfAction);
	}

	public override ItemClass.EnumCrosshairType GetCrosshairType(ItemActionData _actionData)
	{
		if (this.isShowOverlay((ItemActionAttackData)_actionData))
		{
			return ItemClass.EnumCrosshairType.Damage;
		}
		return ItemClass.EnumCrosshairType.Plus;
	}

	public override WorldRayHitInfo GetExecuteActionTarget(ItemActionData _actionData)
	{
		ItemActionMelee.InventoryDataMelee inventoryDataMelee = (ItemActionMelee.InventoryDataMelee)_actionData;
		EntityAlive holdingEntity = inventoryDataMelee.invData.holdingEntity;
		inventoryDataMelee.ray = holdingEntity.GetLookRay();
		if (holdingEntity.IsBreakingBlocks)
		{
			if (inventoryDataMelee.ray.direction.y < 0f)
			{
				inventoryDataMelee.ray.direction = new Vector3(inventoryDataMelee.ray.direction.x, 0f, inventoryDataMelee.ray.direction.z);
				ItemActionMelee.InventoryDataMelee inventoryDataMelee2 = inventoryDataMelee;
				inventoryDataMelee2.ray.origin = inventoryDataMelee2.ray.origin + new Vector3(0f, -0.7f, 0f);
			}
		}
		else if (holdingEntity.GetAttackTarget() != null)
		{
			Vector3 direction = holdingEntity.GetAttackTargetHitPosition() - inventoryDataMelee.ray.origin;
			inventoryDataMelee.ray = new Ray(inventoryDataMelee.ray.origin, direction);
		}
		ItemActionMelee.InventoryDataMelee inventoryDataMelee3 = inventoryDataMelee;
		inventoryDataMelee3.ray.origin = inventoryDataMelee3.ray.origin - 0.15f * inventoryDataMelee.ray.direction;
		int modelLayer = holdingEntity.GetModelLayer();
		holdingEntity.SetModelLayer(2, false, null);
		float distance = Utils.FastMax(this.Range, this.BlockRange) + 0.15f;
		if (holdingEntity is EntityEnemy && holdingEntity.IsBreakingBlocks)
		{
			Voxel.Raycast(inventoryDataMelee.invData.world, inventoryDataMelee.ray, distance, 1073807360, 128, 0.4f);
		}
		else
		{
			EntityAlive x = null;
			int layerMask = -538767381;
			if (Voxel.Raycast(inventoryDataMelee.invData.world, inventoryDataMelee.ray, distance, layerMask, 128, this.SphereRadius))
			{
				x = (ItemActionAttack.GetEntityFromHit(Voxel.voxelRayHitInfo) as EntityAlive);
			}
			if (x == null)
			{
				Voxel.Raycast(inventoryDataMelee.invData.world, inventoryDataMelee.ray, distance, -538488837, 128, this.SphereRadius);
			}
		}
		holdingEntity.SetModelLayer(modelLayer, false, null);
		return _actionData.GetUpdatedHitInfo();
	}

	public override void ExecuteAction(ItemActionData _actionData, bool _bReleased)
	{
		ItemActionMelee.InventoryDataMelee inventoryDataMelee = (ItemActionMelee.InventoryDataMelee)_actionData;
		if (_bReleased)
		{
			inventoryDataMelee.bFirstHitInARow = true;
			return;
		}
		if (Time.time - inventoryDataMelee.lastUseTime < this.Delay)
		{
			return;
		}
		inventoryDataMelee.lastUseTime = Time.time;
		if (inventoryDataMelee.invData.itemValue.MaxUseTimes > 0 && inventoryDataMelee.invData.itemValue.UseTimes >= (float)inventoryDataMelee.invData.itemValue.MaxUseTimes)
		{
			EntityPlayerLocal player = _actionData.invData.holdingEntity as EntityPlayerLocal;
			if (this.item.Properties.Values.ContainsKey(ItemClass.PropSoundJammed))
			{
				Manager.PlayInsidePlayerHead(this.item.Properties.Values[ItemClass.PropSoundJammed], -1, 0f, false, false);
			}
			GameManager.ShowTooltip(player, "ttItemNeedsRepair", false);
			return;
		}
		_actionData.invData.holdingEntity.RightArmAnimationAttack = true;
		ItemActionAttack.AttackHitInfo attackHitInfo;
		inventoryDataMelee.bHarvesting = this.checkHarvesting(_actionData, out attackHitInfo);
		if (inventoryDataMelee.bHarvesting)
		{
			_actionData.invData.holdingEntity.HarvestingAnimation = true;
		}
		string soundStart = this.soundStart;
		if (soundStart != null)
		{
			_actionData.invData.holdingEntity.PlayOneShot(soundStart, false, false, false);
		}
		inventoryDataMelee.bAttackStarted = true;
		if ((double)inventoryDataMelee.invData.holdingEntity.speedForward > 0.009)
		{
			this.rayCastDelay = AnimationDelayData.AnimationDelay[inventoryDataMelee.invData.item.HoldType.Value].RayCastMoving;
			return;
		}
		this.rayCastDelay = AnimationDelayData.AnimationDelay[inventoryDataMelee.invData.item.HoldType.Value].RayCast;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isShowOverlay(ItemActionData actionData)
	{
		if (!base.isShowOverlay(actionData))
		{
			return false;
		}
		if (((ItemActionMelee.InventoryDataMelee)actionData).bFirstHitInARow && Time.time - actionData.lastUseTime <= this.rayCastDelay)
		{
			return false;
		}
		WorldRayHitInfo executeActionTarget = this.GetExecuteActionTarget(actionData);
		return executeActionTarget.bHitValid && (executeActionTarget.tag == null || !GameUtils.IsBlockOrTerrain(executeActionTarget.tag) || executeActionTarget.hit.distanceSq <= this.BlockRange * this.BlockRange) && (executeActionTarget.tag == null || !executeActionTarget.tag.StartsWith("E_") || executeActionTarget.hit.distanceSq <= this.Range * this.Range);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool checkHarvesting(ItemActionData _actionData, out ItemActionAttack.AttackHitInfo myAttackHitInfo)
	{
		WorldRayHitInfo executeActionTarget = this.GetExecuteActionTarget(_actionData);
		ItemValue itemValue = _actionData.invData.itemValue;
		myAttackHitInfo = new ItemActionAttack.AttackHitInfo
		{
			WeaponTypeTag = ItemActionAttack.MeleeTag
		};
		ItemActionAttack.Hit(executeActionTarget, _actionData.invData.holdingEntity.entityId, (this.DamageType == EnumDamageTypes.None) ? EnumDamageTypes.Bashing : this.DamageType, base.GetDamageBlock(itemValue, ItemActionAttack.GetBlockHit(_actionData.invData.world, executeActionTarget), _actionData.invData.holdingEntity, _actionData.indexInEntityOfAction), base.GetDamageEntity(itemValue, _actionData.invData.holdingEntity, _actionData.indexInEntityOfAction), 1f, 1f, 0f, ItemAction.GetDismemberChance(_actionData, executeActionTarget), this.item.MadeOfMaterial.id, this.damageMultiplier, this.getBuffActions(_actionData), myAttackHitInfo, 1, this.ActionExp, this.ActionExpBonusMultiplier, this, this.ToolBonuses, ItemActionAttack.EnumAttackMode.Simulate, null, -1, null);
		if (myAttackHitInfo.bKilled)
		{
			return false;
		}
		if (myAttackHitInfo.itemsToDrop != null && myAttackHitInfo.itemsToDrop.ContainsKey(EnumDropEvent.Harvest))
		{
			List<Block.SItemDropProb> list = myAttackHitInfo.itemsToDrop[EnumDropEvent.Harvest];
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].toolCategory != null && this.ToolBonuses != null && this.ToolBonuses.ContainsKey(list[i].toolCategory))
				{
					return true;
				}
			}
		}
		return false;
	}

	public override bool IsActionRunning(ItemActionData _actionData)
	{
		ItemActionMelee.InventoryDataMelee inventoryDataMelee = (ItemActionMelee.InventoryDataMelee)_actionData;
		return Time.time - inventoryDataMelee.lastUseTime < this.Delay + 0.1f;
	}

	public override void OnHoldingUpdate(ItemActionData _actionData)
	{
		ItemActionMelee.InventoryDataMelee inventoryDataMelee = (ItemActionMelee.InventoryDataMelee)_actionData;
		if (!inventoryDataMelee.bAttackStarted || Time.time - inventoryDataMelee.lastUseTime < this.rayCastDelay)
		{
			return;
		}
		EntityAlive holdingEntity = _actionData.invData.holdingEntity;
		if (this.rayCastDelay <= 0f && !holdingEntity.IsAttackImpact())
		{
			return;
		}
		inventoryDataMelee.bAttackStarted = false;
		ItemActionAttackData.HitDelegate hitDelegate = inventoryDataMelee.hitDelegate;
		inventoryDataMelee.hitDelegate = null;
		if (!holdingEntity.IsAttackValid())
		{
			return;
		}
		float value = EffectManager.GetValue(PassiveEffects.StaminaLoss, inventoryDataMelee.invData.itemValue, 0f, holdingEntity, null, (_actionData.indexInEntityOfAction == 0) ? FastTags<TagGroup.Global>.Parse("primary") : FastTags<TagGroup.Global>.Parse("secondary"), true, true, true, true, true, 1, true, false);
		holdingEntity.AddStamina(-value);
		float damageScale = 1f;
		WorldRayHitInfo worldRayHitInfo;
		if (hitDelegate != null)
		{
			worldRayHitInfo = hitDelegate(out damageScale);
		}
		else
		{
			worldRayHitInfo = this.GetExecuteActionTarget(_actionData);
		}
		if (worldRayHitInfo == null || !worldRayHitInfo.bHitValid)
		{
			return;
		}
		if (worldRayHitInfo.tag != null && GameUtils.IsBlockOrTerrain(worldRayHitInfo.tag) && worldRayHitInfo.hit.distanceSq > this.BlockRange * this.BlockRange)
		{
			return;
		}
		if (worldRayHitInfo.tag != null && worldRayHitInfo.tag.StartsWith("E_") && worldRayHitInfo.hit.distanceSq > this.Range * this.Range)
		{
			return;
		}
		if (inventoryDataMelee.invData.itemValue.MaxUseTimes > 0)
		{
			_actionData.invData.itemValue.UseTimes += EffectManager.GetValue(PassiveEffects.DegradationPerUse, inventoryDataMelee.invData.itemValue, 1f, holdingEntity, null, _actionData.invData.itemValue.ItemClass.ItemTags, true, true, true, true, true, 1, true, false);
			base.HandleItemBreak(_actionData);
		}
		if (ItemAction.ShowDebugDisplayHit)
		{
			DebugLines.Create("MeleeHit", holdingEntity.RootTransform, holdingEntity.position, worldRayHitInfo.hit.pos, new Color(0.7f, 0f, 0f), new Color(1f, 1f, 0f), 0.05f, 0.02f, 1f);
		}
		this.hitTheTarget(inventoryDataMelee, worldRayHitInfo, damageScale);
		if (inventoryDataMelee.bFirstHitInARow)
		{
			inventoryDataMelee.bFirstHitInARow = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void hitTheTarget(ItemActionMelee.InventoryDataMelee _actionData, WorldRayHitInfo hitInfo, float damageScale)
	{
		EntityAlive holdingEntity = _actionData.invData.holdingEntity;
		ItemValue itemValue = _actionData.invData.itemValue;
		float weaponCondition = 1f;
		if (itemValue.MaxUseTimes > 0)
		{
			weaponCondition = ((float)itemValue.MaxUseTimes - itemValue.UseTimes) / (float)itemValue.MaxUseTimes;
		}
		float num = _actionData.invData.item.CritChance.Value;
		num = Mathf.Clamp01(num * (holdingEntity.Stamina / holdingEntity.Stats.Stamina.Max));
		num = EffectManager.GetValue(PassiveEffects.CriticalChance, itemValue, num, holdingEntity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
		_actionData.attackDetails.WeaponTypeTag = ItemActionAttack.MeleeTag;
		int num2 = 1;
		if (this.bUseParticleHarvesting && (this.particleHarvestingCategory == null || this.particleHarvestingCategory == this.item.MadeOfMaterial.id))
		{
			num2 |= 4;
		}
		float blockDamage = base.GetDamageBlock(itemValue, ItemActionAttack.GetBlockHit(_actionData.invData.world, hitInfo), holdingEntity, _actionData.indexInEntityOfAction) * damageScale;
		float damageEntity = base.GetDamageEntity(itemValue, holdingEntity, _actionData.indexInEntityOfAction);
		ItemActionAttack.Hit(hitInfo, holdingEntity.entityId, (this.DamageType == EnumDamageTypes.None) ? EnumDamageTypes.Bashing : this.DamageType, blockDamage, damageEntity, holdingEntity.Stats.Stamina.ValuePercent, weaponCondition, num, ItemAction.GetDismemberChance(_actionData, hitInfo), this.item.MadeOfMaterial.SurfaceCategory, this.damageMultiplier, this.getBuffActions(_actionData), _actionData.attackDetails, num2, this.ActionExp, this.ActionExpBonusMultiplier, this, this.ToolBonuses, _actionData.bHarvesting ? ItemActionAttack.EnumAttackMode.RealAndHarvesting : ItemActionAttack.EnumAttackMode.RealNoHarvesting, null, -1, null);
		GameUtils.HarvestOnAttack(_actionData, this.ToolBonuses);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float rayCastDelay;

	[PublicizedFrom(EAccessModifier.Protected)]
	public class InventoryDataMelee : ItemActionAttackData
	{
		public InventoryDataMelee(ItemInventoryData _invData, int _indexInEntityOfAction) : base(_invData, _indexInEntityOfAction)
		{
		}

		public bool bAttackStarted;

		public Ray ray;

		public bool bHarvesting;

		public bool bFirstHitInARow;
	}
}

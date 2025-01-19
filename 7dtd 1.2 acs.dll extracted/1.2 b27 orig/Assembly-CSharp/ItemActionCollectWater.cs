using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionCollectWater : ItemAction
{
	public override void ReadFrom(DynamicProperties _props)
	{
		base.ReadFrom(_props);
		if (_props.Values.ContainsKey("Change_item_to"))
		{
			this.changeItemToItem = _props.Values["Change_item_to"];
		}
	}

	public override ItemActionData CreateModifierData(ItemInventoryData _invData, int _indexInEntityOfAction)
	{
		return new ItemActionCollectWater.CollectWaterActionData(_invData, _indexInEntityOfAction);
	}

	public override void ExecuteAction(ItemActionData _actionData, bool _bReleased)
	{
		if (!_bReleased)
		{
			return;
		}
		int num = 19500;
		ItemClassWaterContainer itemClassWaterContainer = _actionData.invData.item as ItemClassWaterContainer;
		if (itemClassWaterContainer != null)
		{
			int meta = _actionData.invData.itemValue.Meta;
			num = Mathf.Max(0, itemClassWaterContainer.MaxMass - meta);
		}
		if (num < 195)
		{
			return;
		}
		if (_actionData.lastUseTime > 0f)
		{
			return;
		}
		ItemInventoryData invData = _actionData.invData;
		if (!Voxel.Raycast(invData.world, invData.hitInfo.ray, Constants.cDigAndBuildDistance, 16, 4095, 0f))
		{
			return;
		}
		if (Voxel.voxelRayHitInfo.bHitValid && Voxel.voxelRayHitInfo.hit.voxelData.WaterValue.HasMass())
		{
			_actionData.lastUseTime = Time.time;
			ItemActionCollectWater.CollectWaterActionData collectWaterActionData = (ItemActionCollectWater.CollectWaterActionData)_actionData;
			collectWaterActionData.targetPosition = Voxel.voxelRayHitInfo.hit.blockPos;
			collectWaterActionData.targetMass = num;
			invData.holdingEntity.RightArmAnimationUse = true;
			if (this.soundStart != null)
			{
				invData.holdingEntity.PlayOneShot(this.soundStart, false, false, false);
			}
		}
	}

	public override bool IsActionRunning(ItemActionData _actionData)
	{
		return _actionData.lastUseTime != 0f && Time.time - _actionData.lastUseTime < this.Delay;
	}

	public override void OnHoldingUpdate(ItemActionData _actionData)
	{
		if (_actionData.lastUseTime == 0f || this.IsActionRunning(_actionData))
		{
			return;
		}
		_actionData.lastUseTime = 0f;
		World world = GameManager.Instance.World;
		ChunkCluster chunkCluster = (world != null) ? world.ChunkCache : null;
		if (chunkCluster == null)
		{
			return;
		}
		ItemActionCollectWater.CollectWaterActionData collectWaterActionData = (ItemActionCollectWater.CollectWaterActionData)_actionData;
		int num = CollectWaterUtils.CollectInCube(chunkCluster, collectWaterActionData.targetMass, collectWaterActionData.targetPosition, 1, this.waterPoints);
		if (num > 195)
		{
			NetPackageWaterSet package = NetPackageManager.GetPackage<NetPackageWaterSet>();
			foreach (CollectWaterUtils.WaterPoint waterPoint in this.waterPoints)
			{
				if (waterPoint.massToTake > 0)
				{
					package.AddChange(waterPoint.worldPos, new WaterValue(waterPoint.finalMass));
				}
			}
			GameManager.Instance.SetWaterRPC(package);
			if (!string.IsNullOrEmpty(this.changeItemToItem))
			{
				ItemStack itemStack = new ItemStack(ItemClass.GetItem(this.changeItemToItem, false), _actionData.invData.holdingEntity.inventory.holdingCount);
				itemStack.itemValue.Meta = _actionData.invData.itemValue.Meta + num;
				_actionData.invData.holdingEntity.inventory.SetItem(_actionData.invData.slotIdx, itemStack);
			}
		}
		this.waterPoints.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const string PropChangeItemTo = "Change_item_to";

	[PublicizedFrom(EAccessModifier.Private)]
	public string changeItemToItem;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<CollectWaterUtils.WaterPoint> waterPoints = new List<CollectWaterUtils.WaterPoint>();

	[PublicizedFrom(EAccessModifier.Private)]
	public class CollectWaterActionData : ItemActionAttackData
	{
		public CollectWaterActionData(ItemInventoryData _invData, int _indexInEntityOfAction) : base(_invData, _indexInEntityOfAction)
		{
		}

		public Vector3i targetPosition;

		public int targetMass;
	}
}

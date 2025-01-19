﻿using System;
using System.Collections;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionPlaceAsBlock : ItemAction
{
	public override void ReadFrom(DynamicProperties _props)
	{
		base.ReadFrom(_props);
		if (!_props.Values.ContainsKey("Blockname"))
		{
			throw new Exception("Missing attribute 'blockname' in use_action 'PlaceAsBlock'");
		}
		string text = _props.Values["Blockname"];
		this.blockToPlace = ItemClass.GetItem(text, false).ToBlockValue();
		if (this.blockToPlace.Equals(BlockValue.Air))
		{
			throw new Exception("Unknown block name '" + text + "' in use_action!");
		}
		if (_props.Values.ContainsKey("Change_item_to"))
		{
			this.changeItemTo = _props.Values["Change_item_to"];
			return;
		}
		this.changeItemTo = null;
	}

	public override void ExecuteAction(ItemActionData _actionData, bool _bReleased)
	{
		if (!_bReleased)
		{
			return;
		}
		if (Time.time - _actionData.lastUseTime < this.Delay)
		{
			return;
		}
		if (Time.time - _actionData.lastUseTime < Constants.cBuildIntervall)
		{
			return;
		}
		EntityAlive holdingEntity = _actionData.invData.holdingEntity;
		if (EffectManager.GetValue(PassiveEffects.DisableItem, holdingEntity.inventory.holdingItemItemValue, 0f, holdingEntity, null, _actionData.invData.item.ItemTags, true, true, true, true, true, 1, true, false) > 0f)
		{
			_actionData.lastUseTime = Time.time + 1f;
			Manager.PlayInsidePlayerHead("twitch_no_attack", -1, 0f, false, false);
			return;
		}
		ItemInventoryData invData = _actionData.invData;
		Vector3i lastBlockPos = invData.hitInfo.lastBlockPos;
		if (!invData.hitInfo.bHitValid || lastBlockPos == Vector3i.zero || invData.hitInfo.tag.StartsWith("E_"))
		{
			return;
		}
		BlockValue block = invData.world.GetBlock(lastBlockPos);
		if (!block.isair || (!invData.world.IsEditor() && GameUtils.IsColliderWithinBlock(lastBlockPos, block)))
		{
			return;
		}
		BlockValue blockValue = invData.item.OnConvertToBlockValue(invData.itemValue, this.blockToPlace);
		WorldRayHitInfo worldRayHitInfo = invData.hitInfo.Clone();
		worldRayHitInfo.hit.blockPos = lastBlockPos;
		int placementDistanceSq = blockValue.Block.GetPlacementDistanceSq();
		if (invData.hitInfo.hit.distanceSq > (float)placementDistanceSq)
		{
			return;
		}
		if (!blockValue.Block.CanPlaceBlockAt(invData.world, worldRayHitInfo.hit.clrIdx, lastBlockPos, blockValue, false))
		{
			GameManager.ShowTooltip(invData.holdingEntity as EntityPlayerLocal, "blockCantPlaced", false);
			return;
		}
		BlockPlacement.Result result = blockValue.Block.BlockPlacementHelper.OnPlaceBlock(BlockPlacement.EnumRotationMode.Auto, 0, invData.world, blockValue, worldRayHitInfo.hit, invData.holdingEntity.position);
		blockValue.Block.OnBlockPlaceBefore(invData.world, ref result, invData.holdingEntity, invData.world.GetGameRandom());
		blockValue = result.blockValue;
		if (blockValue.Block.IndexName == "lpblock")
		{
			if (!invData.world.CanPlaceLandProtectionBlockAt(result.blockPos, invData.world.gameManager.GetPersistentLocalPlayer()))
			{
				invData.holdingEntity.PlayOneShot("keystone_build_warning", false, false, false);
				return;
			}
			invData.holdingEntity.PlayOneShot("keystone_placed", false, false, false);
		}
		else if (!invData.world.CanPlaceBlockAt(result.blockPos, invData.world.gameManager.GetPersistentLocalPlayer(), false))
		{
			invData.holdingEntity.PlayOneShot("keystone_build_warning", false, false, false);
			return;
		}
		_actionData.lastUseTime = Time.time;
		blockValue.Block.PlaceBlock(invData.world, result, invData.holdingEntity);
		_actionData.invData.holdingEntity.MinEventContext.ItemActionData = _actionData;
		_actionData.invData.holdingEntity.MinEventContext.BlockValue = blockValue;
		_actionData.invData.holdingEntity.MinEventContext.Position = result.pos;
		_actionData.invData.holdingEntity.FireEvent(MinEventTypes.onSelfPlaceBlock, true);
		QuestEventManager.Current.BlockPlaced(blockValue.Block.GetBlockName(), result.blockPos);
		invData.holdingEntity.RightArmAnimationUse = true;
		if (this.changeItemTo != null)
		{
			ItemValue item = ItemClass.GetItem(this.changeItemTo, false);
			if (!item.IsEmpty())
			{
				invData.holdingEntity.inventory.SetItem(invData.holdingEntity.inventory.holdingItemIdx, new ItemStack(item, 1));
			}
		}
		else
		{
			GameManager.Instance.StartCoroutine(this.decInventoryLater(invData, invData.holdingEntity.inventory.holdingItemIdx));
		}
		invData.holdingEntity.PlayOneShot((this.soundStart != null) ? this.soundStart : "placeblock", false, false, false);
		(invData.holdingEntity as EntityPlayerLocal).DropTimeDelay = 0.5f;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public IEnumerator decInventoryLater(ItemInventoryData data, int index)
	{
		yield return null;
		yield return null;
		yield return new WaitForEndOfFrame();
		ItemStack itemStack = data.holdingEntity.inventory.GetItem(index).Clone();
		if (itemStack.count > 0)
		{
			itemStack.count--;
		}
		data.holdingEntity.inventory.SetItem(index, itemStack);
		yield break;
	}

	public override RenderCubeType GetFocusType(ItemActionData _actionData)
	{
		return RenderCubeType.FullBlockBothSides;
	}

	public override bool IsFocusBlockInside()
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public BlockValue blockToPlace;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string changeItemTo;
}

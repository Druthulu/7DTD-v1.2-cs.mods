﻿using System;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionDisconnectPower : ItemAction
{
	public override ItemActionData CreateModifierData(ItemInventoryData _invData, int _indexInEntityOfAction)
	{
		return new ItemActionDisconnectPower.MyInventoryData(_invData, _indexInEntityOfAction);
	}

	public override void ReadFrom(DynamicProperties _props)
	{
		base.ReadFrom(_props);
	}

	public override void StopHolding(ItemActionData _data)
	{
		((ItemActionDisconnectPower.MyInventoryData)_data).StartDisconnect = false;
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
		_actionData.lastUseTime = Time.time;
		((ItemActionDisconnectPower.MyInventoryData)_actionData).StartDisconnect = true;
	}

	public override bool IsActionRunning(ItemActionData _actionData)
	{
		ItemActionDisconnectPower.MyInventoryData myInventoryData = (ItemActionDisconnectPower.MyInventoryData)_actionData;
		return myInventoryData.StartDisconnect && Time.time - myInventoryData.lastUseTime < 2f * AnimationDelayData.AnimationDelay[myInventoryData.invData.item.HoldType.Value].RayCast;
	}

	public override void OnHoldingUpdate(ItemActionData _actionData)
	{
		ItemActionDisconnectPower.MyInventoryData myInventoryData = (ItemActionDisconnectPower.MyInventoryData)_actionData;
		if (!myInventoryData.StartDisconnect || Time.time - myInventoryData.lastUseTime < AnimationDelayData.AnimationDelay[myInventoryData.invData.item.HoldType.Value].RayCast)
		{
			return;
		}
		myInventoryData.StartDisconnect = false;
		ItemActionDisconnectPower.MyInventoryData myInventoryData2 = (ItemActionDisconnectPower.MyInventoryData)_actionData;
		ItemInventoryData invData = _actionData.invData;
		Vector3i lastBlockPos = invData.hitInfo.lastBlockPos;
		Vector3i blockPos = _actionData.invData.hitInfo.hit.blockPos;
		if (!invData.hitInfo.bHitValid || invData.hitInfo.tag.StartsWith("E_"))
		{
			return;
		}
		if (((ItemActionConnectPower)_actionData.invData.holdingEntity.inventory.holdingItem.Actions[1]).DisconnectWire((ItemActionConnectPower.ConnectPowerData)_actionData.invData.holdingEntity.inventory.holdingItemData.actionData[1]))
		{
			return;
		}
		if (!myInventoryData.invData.world.CanPlaceBlockAt(blockPos, myInventoryData.invData.world.gameManager.GetPersistentLocalPlayer(), false))
		{
			return;
		}
		IPowered poweredBlock = this.GetPoweredBlock(invData);
		if (poweredBlock == null)
		{
			((ItemActionConnectPower)_actionData.invData.holdingEntity.inventory.holdingItem.Actions[1]).DisconnectWire((ItemActionConnectPower.ConnectPowerData)_actionData.invData.holdingEntity.inventory.holdingItemData.actionData[1]);
			return;
		}
		if (myInventoryData.invData.itemValue.MaxUseTimes > 0 && myInventoryData.invData.itemValue.UseTimes >= (float)myInventoryData.invData.itemValue.MaxUseTimes)
		{
			EntityPlayerLocal player = _actionData.invData.holdingEntity as EntityPlayerLocal;
			if (this.item.Properties.Values.ContainsKey(ItemClass.PropSoundJammed))
			{
				Manager.PlayInsidePlayerHead(this.item.Properties.Values[ItemClass.PropSoundJammed], -1, 0f, false, false);
			}
			GameManager.ShowTooltip(player, "ttItemNeedsRepair", false);
			return;
		}
		if (myInventoryData.invData.itemValue.MaxUseTimes > 0)
		{
			_actionData.invData.itemValue.UseTimes += EffectManager.GetValue(PassiveEffects.DegradationPerUse, _actionData.invData.itemValue, 1f, invData.holdingEntity, null, _actionData.invData.itemValue.ItemClass.ItemTags, true, true, true, true, true, 1, true, false);
			base.HandleItemBreak(_actionData);
		}
		_actionData.invData.holdingEntity.RightArmAnimationAttack = true;
		poweredBlock.RemoveParentWithWiringTool(_actionData.invData.holdingEntity.entityId);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IPowered GetPoweredBlock(ItemInventoryData data)
	{
		Block block = data.world.GetBlock(data.hitInfo.hit.blockPos).Block;
		if (!(block is BlockPowered) && !(block is BlockPowerSource))
		{
			return null;
		}
		Vector3i blockPos = data.hitInfo.hit.blockPos;
		ChunkCluster chunkCluster = data.world.ChunkClusters[data.hitInfo.hit.clrIdx];
		if (chunkCluster == null)
		{
			return null;
		}
		Chunk chunk = (Chunk)chunkCluster.GetChunkSync(World.toChunkXZ(blockPos.x), blockPos.y, World.toChunkXZ(blockPos.z));
		if (chunk == null)
		{
			return null;
		}
		TileEntity tileEntity = chunk.GetTileEntity(World.toBlock(blockPos));
		if (tileEntity == null)
		{
			if (block is BlockPowered)
			{
				tileEntity = (block as BlockPowered).CreateTileEntity(chunk);
			}
			else if (block is BlockPowerSource)
			{
				tileEntity = (block as BlockPowerSource).CreateTileEntity(chunk);
			}
			tileEntity.localChunkPos = World.toBlock(blockPos);
			BlockEntityData blockEntity = chunk.GetBlockEntity(blockPos);
			if (blockEntity != null)
			{
				((TileEntityPowered)tileEntity).BlockTransform = blockEntity.transform;
			}
			((TileEntityPowered)tileEntity).InitializePowerData();
			chunk.AddTileEntity(tileEntity);
		}
		return tileEntity as IPowered;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class MyInventoryData : ItemActionAttackData
	{
		public MyInventoryData(ItemInventoryData _invData, int _indexInEntityOfAction) : base(_invData, _indexInEntityOfAction)
		{
		}

		public bool StartDisconnect;
	}
}

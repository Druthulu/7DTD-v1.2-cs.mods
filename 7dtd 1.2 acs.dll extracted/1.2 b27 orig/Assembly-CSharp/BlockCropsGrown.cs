using System;
using System.Collections.Generic;
using System.Globalization;
using Audio;
using UnityEngine.Scripting;

[Preserve]
public class BlockCropsGrown : BlockPlant
{
	public BlockCropsGrown()
	{
		this.CanPickup = true;
		this.IsRandomlyTick = false;
	}

	public override void LateInit()
	{
		base.LateInit();
		if (base.Properties.Values.ContainsKey(BlockCropsGrown.PropBlockBeforeHarvesting))
		{
			this.babyPlant = ItemClass.GetItem(base.Properties.Values[BlockCropsGrown.PropBlockBeforeHarvesting], false).ToBlockValue();
		}
		if (base.Properties.Values.ContainsKey(BlockCropsGrown.PropGrowingBonusHarvestDivisor))
		{
			this.bonusHarvestDivisor = StringParsers.ParseFloat(base.Properties.Values[BlockCropsGrown.PropGrowingBonusHarvestDivisor], 0, -1, NumberStyles.Any);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setPlantBackToBaby(WorldBase _world, int _cIdx, Vector3i _myBlockPos, BlockValue _blockValue)
	{
		this.babyPlant.rotation = _blockValue.rotation;
		_world.SetBlockRPC(_cIdx, _myBlockPos, this.babyPlant);
	}

	public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		List<Block.SItemDropProb> list = null;
		int num;
		if (this.itemsToDrop.TryGetValue(EnumDropEvent.Harvest, out list) && (num = Utils.FastMax(0, list[0].minCount)) > 0)
		{
			if (_blockPos.y > 1)
			{
				int num2 = (int)((float)_world.GetBlock(_blockPos - Vector3i.up).Block.blockMaterial.FertileLevel / this.bonusHarvestDivisor);
				num += num2;
			}
			return string.Format(Localization.Get("pickupCrops", false), num, Localization.Get(list[0].name, false));
		}
		return null;
	}

	public override bool OnBlockActivated(WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		List<Block.SItemDropProb> list = null;
		int num;
		if (this.itemsToDrop.TryGetValue(EnumDropEvent.Harvest, out list) && (num = Utils.FastMax(0, list[0].minCount)) > 0)
		{
			if (_blockPos.y > 1)
			{
				int num2 = (int)((float)_world.GetBlock(_blockPos - Vector3i.up).Block.blockMaterial.FertileLevel / this.bonusHarvestDivisor);
				num += num2;
			}
			ItemStack itemStack = new ItemStack(ItemClass.GetItem(list[0].name, false), num);
			ItemStack @is = itemStack.Clone();
			if ((_player.inventory.CanStackNoEmpty(itemStack) && _player.inventory.AddItem(itemStack)) || _player.bag.AddItem(itemStack) || _player.inventory.AddItem(itemStack))
			{
				_player.PlayOneShot("item_plant_pickup", false, false, false);
				this.setPlantBackToBaby(_world, _cIdx, _blockPos, _blockValue);
				QuestEventManager.Current.BlockPickedUp(_blockValue.Block.GetBlockName(), _blockPos);
				_player.AddUIHarvestingItem(@is, false);
				return true;
			}
			Manager.PlayInsidePlayerHead("ui_denied", -1, 0f, false, false);
		}
		return false;
	}

	public override bool HasBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		return true;
	}

	public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		return this.cmds;
	}

	public override bool OnBlockActivated(string _commandName, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		if (_commandName == "pickup")
		{
			this.OnBlockActivated(_world, _cIdx, _blockPos, _blockValue, _player);
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropBlockBeforeHarvesting = "BlockBeforeHarvesting";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropGrowingBonusHarvestDivisor = "CropsGrown.BonusHarvestDivisor";

	[PublicizedFrom(EAccessModifier.Private)]
	public new BlockActivationCommand[] cmds = new BlockActivationCommand[]
	{
		new BlockActivationCommand("pickup", "hand", true, false)
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockValue babyPlant = BlockValue.Air;

	[PublicizedFrom(EAccessModifier.Private)]
	public float bonusHarvestDivisor = float.MaxValue;
}

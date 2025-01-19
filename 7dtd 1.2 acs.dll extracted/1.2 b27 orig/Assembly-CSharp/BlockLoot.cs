using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class BlockLoot : Block
{
	public BlockLoot()
	{
		this.HasTileEntity = true;
	}

	public override void Init()
	{
		base.Init();
		if (!base.Properties.Values.ContainsKey(BlockLoot.PropLootList))
		{
			throw new Exception("Block with name " + base.GetBlockName() + " doesnt have a loot list");
		}
		this.lootList = base.Properties.Values[BlockLoot.PropLootList];
		base.Properties.ParseFloat(BlockLoot.PropLootStageMod, ref this.LootStageMod);
		base.Properties.ParseFloat(BlockLoot.PropLootStageBonus, ref this.LootStageBonus);
		for (int i = 1; i < 99; i++)
		{
			string text = BlockLoot.PropAlternateLootList + i.ToString();
			if (!base.Properties.Values.ContainsKey(text))
			{
				break;
			}
			string text2 = "";
			if (base.Properties.Params1.ContainsKey(text))
			{
				text2 = base.Properties.Params1[text];
			}
			if (text2 != "")
			{
				FastTags<TagGroup.Global> tag = FastTags<TagGroup.Global>.Parse(text2);
				if (this.AlternateLootList == null)
				{
					this.AlternateLootList = new List<BlockLoot.AlternateLootEntry>();
				}
				this.AlternateLootList.Add(new BlockLoot.AlternateLootEntry
				{
					tag = tag,
					lootEntry = base.Properties.Values[text]
				});
			}
		}
	}

	public override void PlaceBlock(WorldBase _world, BlockPlacement.Result _result, EntityAlive _ea)
	{
		base.PlaceBlock(_world, _result, _ea);
		TileEntityLootContainer tileEntityLootContainer = _world.GetTileEntity(_result.clrIdx, _result.blockPos) as TileEntityLootContainer;
		if (tileEntityLootContainer == null)
		{
			return;
		}
		if (_ea != null && _ea.entityType == EntityType.Player)
		{
			tileEntityLootContainer.bPlayerStorage = true;
			tileEntityLootContainer.worldTimeTouched = _world.GetWorldTime();
			tileEntityLootContainer.SetEmpty();
		}
	}

	public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		TileEntityLootContainer tileEntityLootContainer = _world.GetTileEntity(_clrIdx, _blockPos) as TileEntityLootContainer;
		if (tileEntityLootContainer == null)
		{
			return string.Empty;
		}
		string localizedBlockName = _blockValue.Block.GetLocalizedBlockName();
		PlayerActionsLocal playerInput = ((EntityPlayerLocal)_entityFocusing).playerInput;
		string arg = playerInput.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null) + playerInput.PermanentActions.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null);
		if (!tileEntityLootContainer.bTouched)
		{
			return string.Format(Localization.Get("lootTooltipNew", false), arg, localizedBlockName);
		}
		if (tileEntityLootContainer.IsEmpty())
		{
			return string.Format(Localization.Get("lootTooltipEmpty", false), arg, localizedBlockName);
		}
		return string.Format(Localization.Get("lootTooltipTouched", false), arg, localizedBlockName);
	}

	public override void OnBlockAdded(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockAdded(world, _chunk, _blockPos, _blockValue);
		if (_blockValue.ischild)
		{
			return;
		}
		if (LootContainer.GetLootContainer(this.lootList, true) == null)
		{
			return;
		}
		this.addTileEntity(world, _chunk, _blockPos, _blockValue);
	}

	public override void OnBlockRemoved(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockRemoved(world, _chunk, _blockPos, _blockValue);
		TileEntityLootContainer tileEntityLootContainer = world.GetTileEntity(_chunk.ClrIdx, _blockPos) as TileEntityLootContainer;
		if (tileEntityLootContainer != null)
		{
			tileEntityLootContainer.OnDestroy();
		}
		this.removeTileEntity(world, _chunk, _blockPos, _blockValue);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void addTileEntity(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		TileEntityLootContainer tileEntityLootContainer = new TileEntityLootContainer(_chunk);
		tileEntityLootContainer.localChunkPos = World.toBlock(_blockPos);
		tileEntityLootContainer.lootListName = this.lootList;
		tileEntityLootContainer.SetContainerSize(LootContainer.GetLootContainer(this.lootList, true).size, true);
		_chunk.AddTileEntity(tileEntityLootContainer);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void removeTileEntity(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		_chunk.RemoveTileEntityAt<TileEntityLootContainer>((World)world, World.toBlock(_blockPos));
	}

	public override Block.DestroyedResult OnBlockDestroyedBy(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, int _entityId, bool _bUseHarvestTool)
	{
		TileEntityLootContainer tileEntityLootContainer = _world.GetTileEntity(_clrIdx, _blockPos) as TileEntityLootContainer;
		if (tileEntityLootContainer != null)
		{
			tileEntityLootContainer.OnDestroy();
		}
		if (!GameManager.IsDedicatedServer)
		{
			XUiC_LootWindowGroup.CloseIfOpenAtPos(_blockPos, null);
		}
		return Block.DestroyedResult.Downgrade;
	}

	public override bool OnBlockActivated(WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		if (_player.inventory.IsHoldingItemActionRunning())
		{
			return false;
		}
		TileEntityLootContainer tileEntityLootContainer = _world.GetTileEntity(_cIdx, _blockPos) as TileEntityLootContainer;
		if (tileEntityLootContainer == null)
		{
			return false;
		}
		_player.AimingGun = false;
		Vector3i blockPos = tileEntityLootContainer.ToWorldPos();
		tileEntityLootContainer.bWasTouched = tileEntityLootContainer.bTouched;
		_world.GetGameManager().TELockServer(_cIdx, blockPos, tileEntityLootContainer.entityId, _player.entityId, null);
		return true;
	}

	public override bool OnBlockActivated(string _commandName, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		return _commandName == "Search" && this.OnBlockActivated(_world, _cIdx, _blockPos, _blockValue, _player);
	}

	public override bool HasBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		return true;
	}

	public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		return this.cmds;
	}

	public static string PropLootList = "LootList";

	public static string PropAlternateLootList = "AlternateLootList";

	public static string PropLootStageMod = "LootStageMod";

	public static string PropLootStageBonus = "LootStageBonus";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string lootList;

	public float LootStageMod;

	public float LootStageBonus;

	public List<BlockLoot.AlternateLootEntry> AlternateLootList;

	[PublicizedFrom(EAccessModifier.Private)]
	public new BlockActivationCommand[] cmds = new BlockActivationCommand[]
	{
		new BlockActivationCommand("Search", "search", true, false)
	};

	public struct AlternateLootEntry
	{
		public FastTags<TagGroup.Global> tag;

		public string lootEntry;
	}
}

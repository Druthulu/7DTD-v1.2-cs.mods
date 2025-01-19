using System;
using System.Collections.Generic;
using System.Globalization;
using Audio;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockSecureLoot : Block
{
	public override bool AllowBlockTriggers
	{
		get
		{
			return true;
		}
	}

	public BlockSecureLoot()
	{
		this.HasTileEntity = true;
	}

	public override void Init()
	{
		base.Init();
		if (!base.Properties.Values.ContainsKey(BlockSecureLoot.PropLootList))
		{
			throw new Exception("Block with name " + base.GetBlockName() + " doesnt have a loot list");
		}
		this.lootList = base.Properties.Values[BlockSecureLoot.PropLootList];
		if (base.Properties.Values.ContainsKey(BlockSecureLoot.PropLockPickTime))
		{
			this.lockPickTime = StringParsers.ParseFloat(base.Properties.Values[BlockSecureLoot.PropLockPickTime], 0, -1, NumberStyles.Any);
		}
		else
		{
			this.lockPickTime = 15f;
		}
		if (base.Properties.Values.ContainsKey(BlockSecureLoot.PropLockPickItem))
		{
			this.lockPickItem = base.Properties.Values[BlockSecureLoot.PropLockPickItem];
		}
		if (base.Properties.Values.ContainsKey(BlockSecureLoot.PropLockPickBreakChance))
		{
			this.lockPickBreakChance = StringParsers.ParseFloat(base.Properties.Values[BlockSecureLoot.PropLockPickBreakChance], 0, -1, NumberStyles.Any);
		}
		else
		{
			this.lockPickBreakChance = 0f;
		}
		base.Properties.ParseFloat(BlockSecureLoot.PropLootStageMod, ref this.LootStageMod);
		base.Properties.ParseFloat(BlockSecureLoot.PropLootStageBonus, ref this.LootStageBonus);
		base.Properties.ParseString(BlockSecureLoot.PropOnLockPickSuccessEvent, ref this.lockPickSuccessEvent);
		base.Properties.ParseString(BlockSecureLoot.PropOnLockPickFailedEvent, ref this.lockPickFailedEvent);
	}

	public override void PlaceBlock(WorldBase _world, BlockPlacement.Result _result, EntityAlive _ea)
	{
		base.PlaceBlock(_world, _result, _ea);
		TileEntitySecureLootContainer tileEntitySecureLootContainer = _world.GetTileEntity(_result.clrIdx, _result.blockPos) as TileEntitySecureLootContainer;
		if (tileEntitySecureLootContainer != null)
		{
			tileEntitySecureLootContainer.SetEmpty();
			if (_ea != null && _ea.entityType == EntityType.Player)
			{
				tileEntitySecureLootContainer.bPlayerStorage = true;
				tileEntitySecureLootContainer.SetOwner(PlatformManager.InternalLocalUserIdentifier);
				tileEntitySecureLootContainer.SetLocked(false);
			}
		}
	}

	public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		TileEntitySecureLootContainer tileEntitySecureLootContainer = _world.GetTileEntity(_clrIdx, _blockPos) as TileEntitySecureLootContainer;
		PlayerActionsLocal playerInput = ((EntityPlayerLocal)_entityFocusing).playerInput;
		string arg = playerInput.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null) + playerInput.PermanentActions.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null);
		if (tileEntitySecureLootContainer == null)
		{
			return "";
		}
		string localizedBlockName = _blockValue.Block.GetLocalizedBlockName();
		if (!tileEntitySecureLootContainer.IsLocked())
		{
			return string.Format(Localization.Get("tooltipUnlocked", false), arg, localizedBlockName);
		}
		if (this.lockPickItem == null && !tileEntitySecureLootContainer.LocalPlayerIsOwner())
		{
			return string.Format(Localization.Get("tooltipJammed", false), arg, localizedBlockName);
		}
		return string.Format(Localization.Get("tooltipLocked", false), arg, localizedBlockName);
	}

	public override bool HasBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		return _world.GetTileEntity(_clrIdx, _blockPos) is TileEntitySecureLootContainer;
	}

	public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		TileEntitySecureLootContainer tileEntitySecureLootContainer = _world.GetTileEntity(_clrIdx, _blockPos) as TileEntitySecureLootContainer;
		if (tileEntitySecureLootContainer == null)
		{
			return BlockActivationCommand.Empty;
		}
		PlatformUserIdentifierAbs internalLocalUserIdentifier = PlatformManager.InternalLocalUserIdentifier;
		PersistentPlayerData playerData = _world.GetGameManager().GetPersistentPlayerList().GetPlayerData(tileEntitySecureLootContainer.GetOwner());
		bool flag = tileEntitySecureLootContainer.LocalPlayerIsOwner();
		bool flag2 = !flag && (playerData != null && playerData.ACL != null) && playerData.ACL.Contains(internalLocalUserIdentifier);
		this.cmds[1].enabled = true;
		this.cmds[2].enabled = (!tileEntitySecureLootContainer.IsLocked() && (flag || flag2));
		this.cmds[3].enabled = (tileEntitySecureLootContainer.IsLocked() && flag);
		this.cmds[4].enabled = ((!tileEntitySecureLootContainer.IsUserAllowed(internalLocalUserIdentifier) && tileEntitySecureLootContainer.HasPassword() && tileEntitySecureLootContainer.IsLocked()) || flag);
		this.cmds[0].enabled = (this.lockPickItem != null && tileEntitySecureLootContainer.IsLocked() && !flag);
		this.cmds[5].enabled = (_world.IsEditor() && !GameUtils.IsWorldEditor());
		return this.cmds;
	}

	public override void OnBlockAdded(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockAdded(world, _chunk, _blockPos, _blockValue);
		if (_blockValue.ischild)
		{
			return;
		}
		if (!(world.GetTileEntity(_chunk.ClrIdx, _blockPos) is TileEntitySecureLootContainer))
		{
			TileEntitySecureLootContainer tileEntitySecureLootContainer = new TileEntitySecureLootContainer(_chunk);
			tileEntitySecureLootContainer.localChunkPos = World.toBlock(_blockPos);
			tileEntitySecureLootContainer.lootListName = this.lootList;
			tileEntitySecureLootContainer.SetContainerSize(LootContainer.GetLootContainer(this.lootList, true).size, true);
			_chunk.AddTileEntity(tileEntitySecureLootContainer);
		}
	}

	public override void OnBlockRemoved(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockRemoved(world, _chunk, _blockPos, _blockValue);
		_chunk.RemoveTileEntityAt<TileEntitySecureLootContainer>((World)world, World.toBlock(_blockPos));
	}

	public override Block.DestroyedResult OnBlockDestroyedBy(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, int _entityId, bool _bUseHarvestTool)
	{
		TileEntitySecureLootContainer tileEntitySecureLootContainer = _world.GetTileEntity(_clrIdx, _blockPos) as TileEntitySecureLootContainer;
		if (tileEntitySecureLootContainer != null)
		{
			tileEntitySecureLootContainer.OnDestroy();
		}
		return Block.DestroyedResult.Downgrade;
	}

	public override bool OnBlockActivated(string _commandName, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		if (_blockValue.ischild)
		{
			Vector3i parentPos = _blockValue.Block.multiBlockPos.GetParentPos(_blockPos, _blockValue);
			BlockValue block = _world.GetBlock(parentPos);
			return this.OnBlockActivated(_commandName, _world, _cIdx, parentPos, block, _player);
		}
		TileEntitySecureLootContainer tileEntitySecureLootContainer = _world.GetTileEntity(_cIdx, _blockPos) as TileEntitySecureLootContainer;
		if (tileEntitySecureLootContainer == null)
		{
			return false;
		}
		if (!(_commandName == "Search"))
		{
			if (_commandName == "lock")
			{
				tileEntitySecureLootContainer.SetLocked(true);
				Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, "Misc/locking");
				GameManager.ShowTooltip(_player, "containerLocked", false);
				return true;
			}
			if (_commandName == "unlock")
			{
				tileEntitySecureLootContainer.SetLocked(false);
				Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, "Misc/unlocking");
				GameManager.ShowTooltip(_player, "containerUnlocked", false);
				return true;
			}
			if (_commandName == "keypad")
			{
				LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(_player);
				if (uiforPlayer != null)
				{
					XUiC_KeypadWindow.Open(uiforPlayer, tileEntitySecureLootContainer);
				}
				return true;
			}
			if (!(_commandName == "pick"))
			{
				if (!(_commandName == "trigger"))
				{
					return false;
				}
				XUiC_TriggerProperties.Show(_player.PlayerUI.xui, _cIdx, _blockPos, false, true);
				return true;
			}
			else
			{
				LocalPlayerUI playerUI = _player.PlayerUI;
				ItemValue item = ItemClass.GetItem(this.lockPickItem, false);
				if (playerUI.xui.PlayerInventory.GetItemCount(item) == 0)
				{
					playerUI.xui.CollectedItemList.AddItemStack(new ItemStack(item, 0), true);
					GameManager.ShowTooltip(_player, Localization.Get("ttLockpickMissing", false), false);
					return true;
				}
				_player.AimingGun = false;
				Vector3i blockPos = tileEntitySecureLootContainer.ToWorldPos();
				tileEntitySecureLootContainer.bWasTouched = tileEntitySecureLootContainer.bTouched;
				_world.GetGameManager().TELockServer(_cIdx, blockPos, tileEntitySecureLootContainer.entityId, _player.entityId, "lockpick");
				return true;
			}
		}
		else
		{
			if (!tileEntitySecureLootContainer.IsLocked() || tileEntitySecureLootContainer.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
			{
				return this.OnBlockActivated(_world, _cIdx, _blockPos, _blockValue, _player);
			}
			Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, "Misc/locked");
			return false;
		}
	}

	public void ShowLockpickUi(TileEntitySecureLootContainer _te, EntityPlayerLocal _player)
	{
		Vector3i vector3i = _te.ToWorldPos();
		ItemValue item = ItemClass.GetItem(this.lockPickItem, false);
		LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(_player);
		uiforPlayer.windowManager.Open("timer", true, false, true);
		XUiC_Timer childByType = uiforPlayer.xui.GetChildByType<XUiC_Timer>();
		TimerEventData timerEventData = new TimerEventData();
		timerEventData.CloseEvent += this.EventData_CloseEvent;
		float alternateTime = -1f;
		if (_player.rand.RandomRange(1f) < EffectManager.GetValue(PassiveEffects.LockPickBreakChance, _player.inventory.holdingItemItemValue, this.lockPickBreakChance, _player, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false))
		{
			float value = EffectManager.GetValue(PassiveEffects.LockPickTime, _player.inventory.holdingItemItemValue, this.lockPickTime, _player, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
			float num = value - ((_te.PickTimeLeft == -1f) ? (value - 1f) : (_te.PickTimeLeft + 1f));
			alternateTime = _player.rand.RandomRange(num + 1f, value - 1f);
		}
		timerEventData.Data = new object[]
		{
			_te.GetClrIdx(),
			_te.blockValue,
			vector3i,
			_player,
			item
		};
		timerEventData.Event += this.EventData_Event;
		timerEventData.alternateTime = alternateTime;
		timerEventData.AlternateEvent += this.EventData_CloseEvent;
		childByType.SetTimer(EffectManager.GetValue(PassiveEffects.LockPickTime, _player.inventory.holdingItemItemValue, this.lockPickTime, _player, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false), timerEventData, _te.PickTimeLeft, "");
		Manager.BroadcastPlayByLocalPlayer(vector3i.ToVector3() + Vector3.one * 0.5f, "Misc/unlocking");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EventData_CloseEvent(TimerEventData timerData)
	{
		object[] array = (object[])timerData.Data;
		int clrIdx = (int)array[0];
		Vector3i vector3i = (Vector3i)array[2];
		EntityPlayerLocal entityPlayerLocal = array[3] as EntityPlayerLocal;
		ItemValue itemValue = array[4] as ItemValue;
		LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(entityPlayerLocal);
		Manager.BroadcastPlayByLocalPlayer(vector3i.ToVector3() + Vector3.one * 0.5f, "Misc/locked");
		ItemStack itemStack = new ItemStack(itemValue, 1);
		uiforPlayer.xui.PlayerInventory.RemoveItem(itemStack);
		GameManager.ShowTooltip(entityPlayerLocal, Localization.Get("ttLockpickBroken", false), false);
		uiforPlayer.xui.CollectedItemList.RemoveItemStack(itemStack);
		TileEntitySecureLootContainer tileEntitySecureLootContainer = GameManager.Instance.World.GetTileEntity((int)array[0], vector3i) as TileEntitySecureLootContainer;
		if (tileEntitySecureLootContainer == null)
		{
			return;
		}
		tileEntitySecureLootContainer.PickTimeLeft = Mathf.Max(this.lockPickTime * 0.25f, timerData.timeLeft);
		if (this.lockPickFailedEvent != null)
		{
			GameEventManager.Current.HandleAction(this.lockPickFailedEvent, null, entityPlayerLocal, false, vector3i, "", "", false, true, "", null);
		}
		this.ResetEventData(timerData);
		GameManager.Instance.TEUnlockServer(clrIdx, vector3i, tileEntitySecureLootContainer.EntityId, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EventData_Event(TimerEventData timerData)
	{
		World world = GameManager.Instance.World;
		object[] array = (object[])timerData.Data;
		int clrIdx = (int)array[0];
		BlockValue blockValue = (BlockValue)array[1];
		Vector3i vector3i = (Vector3i)array[2];
		BlockValue block = world.GetBlock(vector3i);
		EntityPlayerLocal entityPlayerLocal = array[3] as EntityPlayerLocal;
		object obj = array[4];
		TileEntitySecureLootContainer tileEntitySecureLootContainer = world.GetTileEntity(clrIdx, vector3i) as TileEntitySecureLootContainer;
		if (tileEntitySecureLootContainer == null)
		{
			return;
		}
		LocalPlayerUI.GetUIForPlayer(entityPlayerLocal);
		if (!this.LockpickDowngradeBlock.isair)
		{
			BlockValue blockValue2 = this.LockpickDowngradeBlock;
			blockValue2 = BlockPlaceholderMap.Instance.Replace(blockValue2, world.GetGameRandom(), vector3i.x, vector3i.z, false);
			blockValue2.rotation = block.rotation;
			blockValue2.meta = block.meta;
			world.SetBlockRPC(clrIdx, vector3i, blockValue2, blockValue2.Block.Density);
		}
		Manager.BroadcastPlayByLocalPlayer(vector3i.ToVector3() + Vector3.one * 0.5f, "Misc/unlocking");
		if (this.lockPickSuccessEvent != null)
		{
			GameEventManager.Current.HandleAction(this.lockPickSuccessEvent, null, entityPlayerLocal, false, vector3i, "", "", false, true, "", null);
		}
		this.ResetEventData(timerData);
		GameManager.Instance.TEUnlockServer(clrIdx, vector3i, tileEntitySecureLootContainer.EntityId, false);
	}

	public override bool OnBlockActivated(WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		if (_blockValue.ischild)
		{
			Vector3i parentPos = _blockValue.Block.multiBlockPos.GetParentPos(_blockPos, _blockValue);
			BlockValue block = _world.GetBlock(parentPos);
			return this.OnBlockActivated(_world, _cIdx, parentPos, block, _player);
		}
		TileEntitySecureLootContainer tileEntitySecureLootContainer = _world.GetTileEntity(_cIdx, _blockPos) as TileEntitySecureLootContainer;
		if (tileEntitySecureLootContainer == null)
		{
			return false;
		}
		_player.AimingGun = false;
		Vector3i blockPos = tileEntitySecureLootContainer.ToWorldPos();
		tileEntitySecureLootContainer.bWasTouched = tileEntitySecureLootContainer.bTouched;
		_world.GetGameManager().TELockServer(_cIdx, blockPos, tileEntitySecureLootContainer.entityId, _player.entityId, null);
		return true;
	}

	public override bool IsWaterBlocked(IBlockAccess _world, Vector3i _blockPos, BlockValue _blockValue, BlockFaceFlag _sides)
	{
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ResetEventData(TimerEventData timerData)
	{
		timerData.AlternateEvent -= this.EventData_CloseEvent;
		timerData.CloseEvent -= this.EventData_CloseEvent;
		timerData.Event -= this.EventData_Event;
	}

	public override void OnTriggered(EntityPlayer _player, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, List<BlockChangeInfo> _blockChanges, BlockTrigger _triggeredBy)
	{
		base.OnTriggered(_player, _world, _cIdx, _blockPos, _blockValue, _blockChanges, _triggeredBy);
		if (!this.LockpickDowngradeBlock.isair)
		{
			BlockValue blockValue = this.LockpickDowngradeBlock;
			blockValue = BlockPlaceholderMap.Instance.Replace(blockValue, _world.GetGameRandom(), _blockPos.x, _blockPos.z, false);
			blockValue.rotation = _blockValue.rotation;
			blockValue.meta = _blockValue.meta;
			_world.SetBlockRPC(_cIdx, _blockPos, blockValue, blockValue.Block.Density);
		}
		Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, "Misc/unlocking");
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropLootList = "LootList";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropLootStageMod = "LootStageMod";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropLootStageBonus = "LootStageBonus";

	public static string PropLockPickTime = "LockPickTime";

	public static string PropLockPickItem = "LockPickItem";

	public static string PropLockPickBreakChance = "LockPickBreakChance";

	public static string PropOnLockPickSuccessEvent = "LockPickSuccessEvent";

	public static string PropOnLockPickFailedEvent = "LockPickFailedEvent";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string lootList;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float lockPickTime;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string lockPickItem;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float lockPickBreakChance;

	public float LootStageMod;

	public float LootStageBonus;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string lockPickSuccessEvent;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string lockPickFailedEvent;

	[PublicizedFrom(EAccessModifier.Private)]
	public new BlockActivationCommand[] cmds = new BlockActivationCommand[]
	{
		new BlockActivationCommand("pick", "unlock", false, false),
		new BlockActivationCommand("Search", "search", false, false),
		new BlockActivationCommand("lock", "lock", false, false),
		new BlockActivationCommand("unlock", "unlock", false, false),
		new BlockActivationCommand("keypad", "keypad", false, false),
		new BlockActivationCommand("trigger", "wrench", true, false)
	};
}

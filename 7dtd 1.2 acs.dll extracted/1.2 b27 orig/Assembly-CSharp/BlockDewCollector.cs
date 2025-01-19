using System;
using UnityEngine.Scripting;

[Preserve]
public class BlockDewCollector : Block
{
	public BlockDewCollector()
	{
		this.HasTileEntity = true;
	}

	public override void Init()
	{
		base.Init();
		base.Properties.ParseString(BlockDewCollector.PropConvertToItem, ref this.ConvertToItem);
		base.Properties.ParseString(BlockDewCollector.PropModdedConvertToItem, ref this.ModdedConvertToItem);
		base.Properties.ParseFloat(BlockDewCollector.PropMinTime, ref this.MinConvertTime);
		base.Properties.ParseFloat(BlockDewCollector.PropMaxTime, ref this.MaxConvertTime);
		base.Properties.ParseFloat(BlockDewCollector.PropModdedSpeed, ref this.ModdedConvertSpeed);
		base.Properties.ParseInt(BlockDewCollector.PropModdedCount, ref this.ModdedConvertCount);
		base.Properties.ParseString(BlockDewCollector.PropOpenSound, ref this.OpenSound);
		base.Properties.ParseString(BlockDewCollector.PropCloseSound, ref this.CloseSound);
		base.Properties.ParseString(BlockDewCollector.PropConvertSound, ref this.ConvertSound);
		base.Properties.ParseFloat(BlockDewCollector.PropTakeDelay, ref this.TakeDelay);
		string text = "1,2,3";
		base.Properties.ParseString("ModTransformNames", ref text);
		this.modTransformNames = text.Split(',', StringSplitOptions.None);
		text = "Count,Speed,Type";
		base.Properties.ParseString("ModTypes", ref text);
		string[] array = text.Split(',', StringSplitOptions.None);
		this.ModTypes = new BlockDewCollector.ModEffectTypes[array.Length];
		int num = 0;
		while (num < array.Length && num < this.ModTypes.Length)
		{
			this.ModTypes[num] = Enum.Parse<BlockDewCollector.ModEffectTypes>(array[num]);
			num++;
		}
	}

	public override void PlaceBlock(WorldBase _world, BlockPlacement.Result _result, EntityAlive _ea)
	{
		base.PlaceBlock(_world, _result, _ea);
		TileEntityDewCollector tileEntityDewCollector = _world.GetTileEntity(_result.clrIdx, _result.blockPos) as TileEntityDewCollector;
		if (tileEntityDewCollector == null)
		{
			return;
		}
		if (_ea != null && _ea.entityType == EntityType.Player)
		{
			tileEntityDewCollector.worldTimeTouched = _world.GetWorldTime();
			tileEntityDewCollector.SetEmpty();
		}
	}

	public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		TileEntityDewCollector tileEntityDewCollector = _world.GetTileEntity(_clrIdx, _blockPos) as TileEntityDewCollector;
		if (tileEntityDewCollector == null)
		{
			return string.Empty;
		}
		string localizedBlockName = _blockValue.Block.GetLocalizedBlockName();
		PlayerActionsLocal playerInput = ((EntityPlayerLocal)_entityFocusing).playerInput;
		string arg = playerInput.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null) + playerInput.PermanentActions.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null);
		if (tileEntityDewCollector.IsWaterEmpty())
		{
			return string.Format(Localization.Get("dewCollectorEmpty", false), arg, localizedBlockName);
		}
		if (tileEntityDewCollector.IsModdedConvertItem)
		{
			return string.Format(Localization.Get("dewCollectorHasWater", false), arg, localizedBlockName);
		}
		return string.Format(Localization.Get("dewCollectorHasDirtyWater", false), arg, localizedBlockName);
	}

	public override void OnBlockAdded(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockAdded(world, _chunk, _blockPos, _blockValue);
		if (_blockValue.ischild)
		{
			return;
		}
		this.addTileEntity(world, _chunk, _blockPos, _blockValue);
	}

	public override void OnBlockRemoved(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockRemoved(world, _chunk, _blockPos, _blockValue);
		TileEntityDewCollector tileEntityDewCollector = world.GetTileEntity(_chunk.ClrIdx, _blockPos) as TileEntityDewCollector;
		if (tileEntityDewCollector != null)
		{
			tileEntityDewCollector.OnDestroy();
		}
		this.removeTileEntity(world, _chunk, _blockPos, _blockValue);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void addTileEntity(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		TileEntityDewCollector tileEntityDewCollector = new TileEntityDewCollector(_chunk);
		tileEntityDewCollector.localChunkPos = World.toBlock(_blockPos);
		tileEntityDewCollector.SetWorldTime();
		_chunk.AddTileEntity(tileEntityDewCollector);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void removeTileEntity(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		_chunk.RemoveTileEntityAt<TileEntityDewCollector>((World)world, World.toBlock(_blockPos));
	}

	public override Block.DestroyedResult OnBlockDestroyedBy(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, int _entityId, bool _bUseHarvestTool)
	{
		TileEntityDewCollector tileEntityDewCollector = _world.GetTileEntity(_clrIdx, _blockPos) as TileEntityDewCollector;
		if (tileEntityDewCollector != null)
		{
			tileEntityDewCollector.OnDestroy();
		}
		if (!GameManager.IsDedicatedServer)
		{
			XUiC_DewCollectorWindowGroup.CloseIfOpenAtPos(_blockPos, null);
		}
		return Block.DestroyedResult.Downgrade;
	}

	public override bool OnBlockActivated(WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		if (_player.inventory.IsHoldingItemActionRunning())
		{
			return false;
		}
		TileEntityDewCollector tileEntityDewCollector = _world.GetTileEntity(_cIdx, _blockPos) as TileEntityDewCollector;
		if (tileEntityDewCollector == null)
		{
			return false;
		}
		_player.AimingGun = false;
		Vector3i blockPos = tileEntityDewCollector.ToWorldPos();
		_world.GetGameManager().TELockServer(_cIdx, blockPos, tileEntityDewCollector.entityId, _player.entityId, null);
		return true;
	}

	public override bool OnBlockActivated(string _commandName, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		if (_commandName == "Search")
		{
			return this.OnBlockActivated(_world, _cIdx, _blockPos, _blockValue, _player);
		}
		if (!(_commandName == "take"))
		{
			return false;
		}
		this.TakeItemWithTimer(_cIdx, _blockPos, _blockValue, _player);
		return true;
	}

	public override bool HasBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		return true;
	}

	public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		bool flag = _world.IsMyLandProtectedBlock(_blockPos, _world.GetGameManager().GetPersistentLocalPlayer(), false);
		this.cmds[1].enabled = (flag && this.TakeDelay > 0f);
		return this.cmds;
	}

	public void TakeItemWithTimer(int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _player)
	{
		if (_blockValue.damage > 0)
		{
			GameManager.ShowTooltip(_player as EntityPlayerLocal, Localization.Get("ttRepairBeforePickup", false), string.Empty, "ui_denied", null, false);
			return;
		}
		if (!(GameManager.Instance.World.GetTileEntity(_blockPos) as TileEntityDewCollector).IsEmpty())
		{
			GameManager.ShowTooltip(_player as EntityPlayerLocal, Localization.Get("ttWorkstationNotEmpty", false), string.Empty, "ui_denied", null, false);
			return;
		}
		LocalPlayerUI playerUI = (_player as EntityPlayerLocal).PlayerUI;
		playerUI.windowManager.Open("timer", true, false, true);
		XUiC_Timer childByType = playerUI.xui.GetChildByType<XUiC_Timer>();
		TimerEventData timerEventData = new TimerEventData();
		timerEventData.Data = new object[]
		{
			_cIdx,
			_blockValue,
			_blockPos,
			_player
		};
		timerEventData.Event += this.EventData_Event;
		childByType.SetTimer(this.TakeDelay, timerEventData, -1f, "");
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
		if (block.damage > 0)
		{
			GameManager.ShowTooltip(entityPlayerLocal, Localization.Get("ttRepairBeforePickup", false), string.Empty, "ui_denied", null, false);
			return;
		}
		if (block.type != blockValue.type)
		{
			GameManager.ShowTooltip(entityPlayerLocal, Localization.Get("ttBlockMissingPickup", false), string.Empty, "ui_denied", null, false);
			return;
		}
		if ((world.GetTileEntity(clrIdx, vector3i) as TileEntityDewCollector).IsUserAccessing())
		{
			GameManager.ShowTooltip(entityPlayerLocal, Localization.Get("ttCantPickupInUse", false), string.Empty, "ui_denied", null, false);
			return;
		}
		LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(entityPlayerLocal);
		ItemStack itemStack = new ItemStack(block.ToItemValue(), 1);
		if (!uiforPlayer.xui.PlayerInventory.AddItem(itemStack))
		{
			uiforPlayer.xui.PlayerInventory.DropItem(itemStack);
		}
		world.SetBlockRPC(clrIdx, vector3i, BlockValue.Air);
	}

	public void UpdateVisible(TileEntityDewCollector _te)
	{
		if (_te.GetChunk().GetBlockEntity(_te.ToWorldPos()).transform)
		{
			ItemStack[] modSlots = _te.ModSlots;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropConvertToItem = "ConvertToItem";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropModdedConvertToItem = "ModdedConvertToItem";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropMinTime = "MinConvertTime";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropMaxTime = "MaxConvertTime";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropModdedSpeed = "ModdedConvertSpeed";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropModdedCount = "ModdedConvertCount";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropOpenSound = "OpenSound";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropCloseSound = "CloseSound";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropConvertSound = "ConvertSound";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropTakeDelay = "TakeDelay";

	public string ConvertToItem;

	public string ModdedConvertToItem;

	public float MinConvertTime = 21600f;

	public float MaxConvertTime = 43200f;

	public float ModdedConvertSpeed = 0.5f;

	public int ModdedConvertCount = 2;

	public string OpenSound;

	public string CloseSound;

	public string ConvertSound;

	[PublicizedFrom(EAccessModifier.Private)]
	public float TakeDelay = 2f;

	[PublicizedFrom(EAccessModifier.Private)]
	public string[] modNames;

	[PublicizedFrom(EAccessModifier.Private)]
	public string[] modTransformNames;

	public BlockDewCollector.ModEffectTypes[] ModTypes;

	[PublicizedFrom(EAccessModifier.Private)]
	public new BlockActivationCommand[] cmds = new BlockActivationCommand[]
	{
		new BlockActivationCommand("Search", "search", true, false),
		new BlockActivationCommand("take", "hand", false, false)
	};

	public enum ModEffectTypes
	{
		Type,
		Speed,
		Count
	}
}

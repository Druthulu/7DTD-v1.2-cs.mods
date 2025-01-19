using System;
using UnityEngine.Scripting;

[Preserve]
public class BlockTimerRelay : BlockPowered
{
	public BlockTimerRelay()
	{
		this.HasTileEntity = true;
	}

	public override void OnBlockAdded(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockAdded(world, _chunk, _blockPos, _blockValue);
		if (_blockValue.ischild)
		{
			return;
		}
		if (!(world.GetTileEntity(_chunk.ClrIdx, _blockPos) is TileEntityPoweredTrigger))
		{
			TileEntityPowered tileEntityPowered = this.CreateTileEntity(_chunk);
			tileEntityPowered.localChunkPos = World.toBlock(_blockPos);
			tileEntityPowered.InitializePowerData();
			_chunk.AddTileEntity(tileEntityPowered);
		}
		_chunk.AddEntityBlockStub(new BlockEntityData(_blockValue, _blockPos)
		{
			bNeedsTemperature = true
		});
	}

	public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		bool flag = _world.GetTileEntity(_clrIdx, _blockPos) is TileEntityPoweredTrigger;
		PlayerActionsLocal playerInput = ((EntityPlayerLocal)_entityFocusing).playerInput;
		if (flag)
		{
			string arg = playerInput.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null) + playerInput.PermanentActions.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null);
			string localizedBlockName = _blockValue.Block.GetLocalizedBlockName();
			return string.Format(Localization.Get("vendingMachineActivate", false), arg, localizedBlockName);
		}
		return "";
	}

	public override bool OnBlockActivated(string _commandName, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		if (_blockValue.ischild)
		{
			Vector3i parentPos = _blockValue.Block.multiBlockPos.GetParentPos(_blockPos, _blockValue);
			BlockValue block = _world.GetBlock(parentPos);
			return this.OnBlockActivated(_commandName, _world, _cIdx, parentPos, block, _player);
		}
		TileEntityPoweredTrigger tileEntityPoweredTrigger = _world.GetTileEntity(_cIdx, _blockPos) as TileEntityPoweredTrigger;
		if (tileEntityPoweredTrigger == null)
		{
			return false;
		}
		if (_commandName == "options")
		{
			_player.AimingGun = false;
			Vector3i blockPos = tileEntityPoweredTrigger.ToWorldPos();
			_world.GetGameManager().TELockServer(_cIdx, blockPos, tileEntityPoweredTrigger.entityId, _player.entityId, null);
			return true;
		}
		if (!(_commandName == "take"))
		{
			return false;
		}
		base.TakeItemWithTimer(_cIdx, _blockPos, _blockValue, _player);
		return true;
	}

	public override bool HasBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		return true;
	}

	public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		bool enabled = _world.CanPlaceBlockAt(_blockPos, _world.GetGameManager().GetPersistentLocalPlayer(), false);
		bool flag = _world.IsMyLandProtectedBlock(_blockPos, _world.GetGameManager().GetPersistentLocalPlayer(), false);
		this.cmds[0].enabled = enabled;
		this.cmds[1].enabled = (flag && this.TakeDelay > 0f);
		return this.cmds;
	}

	public override TileEntityPowered CreateTileEntity(Chunk chunk)
	{
		return new TileEntityPoweredTrigger(chunk)
		{
			TriggerType = PowerTrigger.TriggerTypes.TimerRelay
		};
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public new BlockActivationCommand[] cmds = new BlockActivationCommand[]
	{
		new BlockActivationCommand("options", "tool", true, false),
		new BlockActivationCommand("take", "hand", false, false)
	};
}

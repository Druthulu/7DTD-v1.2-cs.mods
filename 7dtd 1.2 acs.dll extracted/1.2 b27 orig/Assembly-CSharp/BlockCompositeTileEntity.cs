using System;
using System.Collections.Generic;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockCompositeTileEntity : Block
{
	public override bool AllowBlockTriggers
	{
		get
		{
			return true;
		}
	}

	public TileEntityCompositeData CompositeData { get; [PublicizedFrom(EAccessModifier.Private)] set; }

	public BlockCompositeTileEntity()
	{
		this.HasTileEntity = true;
	}

	public override void Init()
	{
		base.Init();
		this.CompositeData = TileEntityCompositeData.ParseBlock(this);
	}

	public override void PlaceBlock(WorldBase _world, BlockPlacement.Result _result, EntityAlive _ea)
	{
		base.PlaceBlock(_world, _result, _ea);
		TileEntityComposite tileEntityComposite = _world.GetTileEntity(_result.clrIdx, _result.blockPos) as TileEntityComposite;
		if (tileEntityComposite != null)
		{
			tileEntityComposite.PlaceBlock(_world, _result, _ea);
		}
	}

	public override void OnBlockAdded(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockAdded(_world, _chunk, _blockPos, _blockValue);
		if (_blockValue.ischild)
		{
			return;
		}
		if (_world.GetTileEntity(_chunk.ClrIdx, _blockPos) is TileEntityComposite)
		{
			return;
		}
		TileEntityComposite te = new TileEntityComposite(_chunk, this)
		{
			localChunkPos = World.toBlock(_blockPos)
		};
		_chunk.AddTileEntity(te);
	}

	public override void OnBlockRemoved(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockRemoved(_world, _chunk, _blockPos, _blockValue);
		if (_blockValue.ischild)
		{
			return;
		}
		_chunk.RemoveTileEntityAt<TileEntityComposite>((World)_world, World.toBlock(_blockPos));
	}

	public override void OnBlockLoaded(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockLoaded(_world, _clrIdx, _blockPos, _blockValue);
		bool ischild = _blockValue.ischild;
	}

	public override void OnBlockUnloaded(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockUnloaded(_world, _clrIdx, _blockPos, _blockValue);
	}

	public override BlockValue OnBlockPlaced(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, GameRandom _rnd)
	{
		return base.OnBlockPlaced(_world, _clrIdx, _blockPos, _blockValue, _rnd);
	}

	public override void OnBlockValueChanged(WorldBase _world, Chunk _chunk, int _clrIdx, Vector3i _blockPos, BlockValue _oldBlockValue, BlockValue _newBlockValue)
	{
		if (this.GetParentPos(_world, _clrIdx, ref _blockPos, ref _newBlockValue))
		{
			this.OnBlockValueChanged(_world, _chunk, _clrIdx, _blockPos, _oldBlockValue, _newBlockValue);
			return;
		}
		base.OnBlockValueChanged(_world, _chunk, _clrIdx, _blockPos, _oldBlockValue, _newBlockValue);
	}

	public override Block.DestroyedResult OnBlockDestroyedBy(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, int _entityId, bool _bUseHarvestTool)
	{
		if (this.GetParentPos(_world, _clrIdx, ref _blockPos, ref _blockValue))
		{
			return this.OnBlockDestroyedBy(_world, _clrIdx, _blockPos, _blockValue, _entityId, _bUseHarvestTool);
		}
		return Block.DestroyedResult.Downgrade;
	}

	public override int OnBlockDamaged(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, int _damagePoints, int _entityIdThatDamaged, ItemActionAttack.AttackHitInfo _attackHitInfo, bool _bUseHarvestTool, bool _bBypassMaxDamage, int _recDepth = 0)
	{
		if (this.GetParentPos(_world, _clrIdx, ref _blockPos, ref _blockValue))
		{
			return _blockValue.Block.OnBlockDamaged(_world, _clrIdx, _blockPos, _blockValue, _damagePoints, _entityIdThatDamaged, _attackHitInfo, _bUseHarvestTool, _bBypassMaxDamage, _recDepth + 1);
		}
		return base.OnBlockDamaged(_world, _clrIdx, _blockPos, _blockValue, _damagePoints, _entityIdThatDamaged, _attackHitInfo, _bUseHarvestTool, _bBypassMaxDamage, _recDepth);
	}

	public override void OnBlockEntityTransformAfterActivated(WorldBase _world, Vector3i _blockPos, int _cIdx, BlockValue _blockValue, BlockEntityData _bed)
	{
		if (_bed == null)
		{
			return;
		}
		Chunk chunk = (Chunk)_world.GetChunkFromWorldPos(_blockPos);
		TileEntityComposite tileEntityComposite = _world.GetTileEntity(_cIdx, _blockPos) as TileEntityComposite;
		if (tileEntityComposite == null)
		{
			tileEntityComposite = new TileEntityComposite(chunk, this)
			{
				localChunkPos = World.toBlock(_blockPos)
			};
			chunk.AddTileEntity(tileEntityComposite);
		}
		tileEntityComposite.SetBlockEntityData(_bed);
		base.OnBlockEntityTransformAfterActivated(_world, _blockPos, _cIdx, _blockValue, _bed);
	}

	public override void OnTriggered(EntityPlayer _player, WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, List<BlockChangeInfo> _blockChanges, BlockTrigger _triggeredBy)
	{
		if (this.GetParentPos(_world, _clrIdx, ref _blockPos, ref _blockValue))
		{
			_blockValue.Block.OnTriggered(_player, _world, _clrIdx, _blockPos, _blockValue, _blockChanges, _triggeredBy);
			return;
		}
		base.OnTriggered(_player, _world, _clrIdx, _blockPos, _blockValue, _blockChanges, _triggeredBy);
		if (!this.DowngradeBlock.isair)
		{
			BlockValue blockValue = this.DowngradeBlock;
			blockValue = BlockPlaceholderMap.Instance.Replace(blockValue, _world.GetGameRandom(), _blockPos.x, _blockPos.z, false);
			blockValue.rotation = _blockValue.rotation;
			blockValue.meta = _blockValue.meta;
			_world.SetBlockRPC(_clrIdx, _blockPos, blockValue, blockValue.Block.Density);
		}
		Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, "Misc/unlocking");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool GetParentPos(WorldBase _world, int _clrIdx, ref Vector3i _blockPos, ref BlockValue _blockValue)
	{
		if (!this.isMultiBlock || !_blockValue.ischild)
		{
			return false;
		}
		ChunkCluster chunkCluster = _world.ChunkClusters[_clrIdx];
		if (chunkCluster == null)
		{
			throw new Exception("ChunkCluster null in " + StackTraceUtility.ExtractStackTrace());
		}
		_blockPos = this.multiBlockPos.GetParentPos(_blockPos, _blockValue);
		_blockValue = chunkCluster.GetBlock(_blockPos);
		if (_blockValue.ischild)
		{
			Log.Error(string.Format("Block on position {0} with name '{1}' should be a parent but is not! (6)", _blockPos, _blockValue.Block.GetBlockName()));
			return false;
		}
		return true;
	}

	public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		if (this.GetParentPos(_world, _clrIdx, ref _blockPos, ref _blockValue))
		{
			return _blockValue.Block.GetActivationText(_world, _blockValue, _clrIdx, _blockPos, _entityFocusing);
		}
		TileEntityComposite tileEntityComposite = _world.GetTileEntity(_clrIdx, _blockPos) as TileEntityComposite;
		if (tileEntityComposite == null)
		{
			return "";
		}
		if (this.commands == null)
		{
			this.commands = tileEntityComposite.InitBlockActivationCommands();
		}
		if (this.commands.Length == 0)
		{
			return null;
		}
		PlayerActionsLocal playerInput = ((EntityPlayerLocal)_entityFocusing).playerInput;
		string activateHotkeyMarkup = playerInput.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null) + playerInput.PermanentActions.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null);
		string localizedBlockName = _blockValue.Block.GetLocalizedBlockName();
		return tileEntityComposite.GetActivationText(_world, _blockPos, _blockValue, _entityFocusing, activateHotkeyMarkup, localizedBlockName);
	}

	public override bool HasBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		if (this.GetParentPos(_world, _clrIdx, ref _blockPos, ref _blockValue))
		{
			return _blockValue.Block.HasBlockActivationCommands(_world, _blockValue, _clrIdx, _blockPos, _entityFocusing);
		}
		TileEntityComposite tileEntityComposite = _world.GetTileEntity(_clrIdx, _blockPos) as TileEntityComposite;
		if (tileEntityComposite == null)
		{
			return false;
		}
		if (this.commands == null)
		{
			this.commands = tileEntityComposite.InitBlockActivationCommands();
		}
		return this.commands.Length != 0 && tileEntityComposite.UpdateBlockActivationCommands(this.commands, _world, _blockPos, _blockValue, _entityFocusing);
	}

	public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		if (this.GetParentPos(_world, _clrIdx, ref _blockPos, ref _blockValue))
		{
			return _blockValue.Block.GetBlockActivationCommands(_world, _blockValue, _clrIdx, _blockPos, _entityFocusing);
		}
		TileEntityComposite tileEntityComposite = _world.GetTileEntity(_clrIdx, _blockPos) as TileEntityComposite;
		if (tileEntityComposite == null)
		{
			return BlockActivationCommand.Empty;
		}
		if (this.commands == null)
		{
			this.commands = tileEntityComposite.InitBlockActivationCommands();
		}
		if (this.commands.Length == 0)
		{
			return this.commands;
		}
		tileEntityComposite.UpdateBlockActivationCommands(this.commands, _world, _blockPos, _blockValue, _entityFocusing);
		return this.commands;
	}

	public override bool OnBlockActivated(string _commandName, WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		if (this.GetParentPos(_world, _clrIdx, ref _blockPos, ref _blockValue))
		{
			return this.OnBlockActivated(_commandName, _world, _clrIdx, _blockPos, _blockValue, _player);
		}
		TileEntityComposite tileEntityComposite = _world.GetTileEntity(_clrIdx, _blockPos) as TileEntityComposite;
		if (tileEntityComposite == null)
		{
			return false;
		}
		if (this.commands == null)
		{
			this.commands = tileEntityComposite.InitBlockActivationCommands();
		}
		return tileEntityComposite.OnBlockActivated(this.commands, _commandName, _world, _blockPos, _blockValue, _player);
	}

	public override bool IsWaterBlocked(IBlockAccess _world, Vector3i _blockPos, BlockValue _blockValue, BlockFaceFlag _sides)
	{
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockActivationCommand[] commands;
}

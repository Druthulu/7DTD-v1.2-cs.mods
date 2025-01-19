using System;
using System.Collections;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class BlockRanged : BlockPowered
{
	public BlockRanged()
	{
		this.HasTileEntity = true;
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

	public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		if (!_world.CanPlaceBlockAt(_blockPos, _world.GetGameManager().GetPersistentLocalPlayer(), false))
		{
			return "";
		}
		bool flag = _world.GetTileEntity(_clrIdx, _blockPos) is TileEntityPoweredRangedTrap;
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
		_player.CancelInventoryActions();
		TileEntityPoweredRangedTrap tileEntityPoweredRangedTrap = _world.GetTileEntity(_cIdx, _blockPos) as TileEntityPoweredRangedTrap;
		if (tileEntityPoweredRangedTrap == null)
		{
			return false;
		}
		if (_commandName == "options")
		{
			_player.AimingGun = false;
			Vector3i worldPos = tileEntityPoweredRangedTrap.ToWorldPos();
			GameManager.Instance.StartCoroutine(this.lockLater(_world, _cIdx, worldPos, tileEntityPoweredRangedTrap.entityId, _player));
			return true;
		}
		if (!(_commandName == "take"))
		{
			return false;
		}
		base.TakeItemWithTimer(_cIdx, _blockPos, _blockValue, _player);
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator lockLater(WorldBase _world, int _cIdx, Vector3i _worldPos, int _tileId, EntityPlayerLocal _player)
	{
		while (_player.IsReloading())
		{
			yield return null;
		}
		_world.GetGameManager().TELockServer(_cIdx, _worldPos, _tileId, _player.entityId, null);
		yield break;
	}

	public override void PlaceBlock(WorldBase _world, BlockPlacement.Result _result, EntityAlive _ea)
	{
		base.PlaceBlock(_world, _result, _ea);
		TileEntityPoweredRangedTrap tileEntityPoweredRangedTrap = _world.GetTileEntity(_result.clrIdx, _result.blockPos) as TileEntityPoweredRangedTrap;
		if (tileEntityPoweredRangedTrap != null && _ea != null && _ea.entityType == EntityType.Player)
		{
			tileEntityPoweredRangedTrap.SetOwner(PlatformManager.InternalLocalUserIdentifier);
		}
	}

	public override void OnBlockEntityTransformAfterActivated(WorldBase _world, Vector3i _blockPos, int _cIdx, BlockValue _blockValue, BlockEntityData _ebcd)
	{
		base.OnBlockEntityTransformAfterActivated(_world, _blockPos, _cIdx, _blockValue, _ebcd);
		AutoTurretController component = _ebcd.transform.gameObject.GetComponent<AutoTurretController>();
		if (component != null)
		{
			component.FireController.BlockPosition = _ebcd.pos.ToVector3();
			component.Init(base.Properties);
		}
		TileEntityPowered tileEntityPowered = (TileEntityPowered)_world.GetTileEntity(_cIdx, _blockPos);
		if (tileEntityPowered == null)
		{
			ChunkCluster chunkCluster = _world.ChunkClusters[_cIdx];
			if (chunkCluster == null)
			{
				return;
			}
			Chunk chunk = (Chunk)chunkCluster.GetChunkFromWorldPos(_blockPos);
			if (chunk == null)
			{
				return;
			}
			tileEntityPowered = this.CreateTileEntity(chunk);
			tileEntityPowered.localChunkPos = World.toBlock(_blockPos);
			tileEntityPowered.InitializePowerData();
			chunk.AddTileEntity(tileEntityPowered);
		}
		tileEntityPowered.BlockTransform = _ebcd.transform;
		tileEntityPowered.MarkWireDirty();
		if (tileEntityPowered.GetParent().y != -9999)
		{
			IPowered powered = _world.GetTileEntity(_cIdx, tileEntityPowered.GetParent()) as IPowered;
			if (powered != null)
			{
				powered.DrawWires();
			}
		}
	}

	public override void Init()
	{
		base.Init();
		if (base.Properties.Values.ContainsKey("AmmoItem"))
		{
			this.AmmoItemName = base.Properties.Values["AmmoItem"];
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool updateState(WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue)
	{
		ChunkCluster chunkCluster = _world.ChunkClusters[_cIdx];
		if (chunkCluster == null)
		{
			return false;
		}
		IChunk chunkSync = chunkCluster.GetChunkSync(World.toChunkXZ(_blockPos.x), World.toChunkY(_blockPos.y), World.toChunkXZ(_blockPos.z));
		if (chunkSync == null)
		{
			return false;
		}
		BlockEntityData blockEntity = chunkSync.GetBlockEntity(_blockPos);
		if (blockEntity == null || !blockEntity.bHasTransform)
		{
			return false;
		}
		bool flag = (_blockValue.meta & 2) > 0;
		TileEntityPoweredRangedTrap tileEntityPoweredRangedTrap = (TileEntityPoweredRangedTrap)_world.GetTileEntity(_cIdx, _blockPos);
		if (flag)
		{
			if (tileEntityPoweredRangedTrap != null)
			{
				PowerManager.Instance.ClientUpdateList.Add(tileEntityPoweredRangedTrap);
			}
		}
		else if (tileEntityPoweredRangedTrap != null)
		{
			PowerManager.Instance.ClientUpdateList.Remove(tileEntityPoweredRangedTrap);
		}
		AutoTurretController component = blockEntity.transform.gameObject.GetComponent<AutoTurretController>();
		if (component == null)
		{
			return false;
		}
		component.TileEntity = tileEntityPoweredRangedTrap;
		component.IsOn = flag;
		return true;
	}

	public override bool ActivateBlock(WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, bool isOn, bool isPowered)
	{
		byte b = (byte)(((int)_blockValue.meta & -3) | (isOn ? 2 : 0));
		if (_blockValue.meta != b)
		{
			_blockValue.meta = b;
			_world.SetBlockRPC(_cIdx, _blockPos, _blockValue);
		}
		ChunkCluster chunkCluster = _world.ChunkClusters[_cIdx];
		if (chunkCluster == null)
		{
			return false;
		}
		IChunk chunkSync = chunkCluster.GetChunkSync(World.toChunkXZ(_blockPos.x), World.toChunkY(_blockPos.y), World.toChunkXZ(_blockPos.z));
		if (chunkSync == null)
		{
			return false;
		}
		BlockEntityData blockEntity = chunkSync.GetBlockEntity(_blockPos);
		if (blockEntity == null || !blockEntity.bHasTransform)
		{
			return false;
		}
		AutoTurretController component = blockEntity.transform.gameObject.GetComponent<AutoTurretController>();
		if (component == null)
		{
			return false;
		}
		TileEntityPoweredRangedTrap tileEntity = (TileEntityPoweredRangedTrap)_world.GetTileEntity(_cIdx, _blockPos);
		component.TileEntity = tileEntity;
		component.IsOn = isOn;
		return isOn;
	}

	public override void OnBlockLoaded(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockLoaded(_world, _clrIdx, _blockPos, _blockValue);
		bool flag = (_blockValue.meta & 2) > 0;
		TileEntityPowered tileEntityPowered = (TileEntityPowered)_world.GetTileEntity(_clrIdx, _blockPos);
		if (flag)
		{
			if (tileEntityPowered != null && tileEntityPowered is TileEntityPoweredBlock)
			{
				PowerManager.Instance.ClientUpdateList.Add(tileEntityPowered as TileEntityPoweredBlock);
				return;
			}
		}
		else if (tileEntityPowered != null && tileEntityPowered is TileEntityPoweredBlock)
		{
			PowerManager.Instance.ClientUpdateList.Remove(tileEntityPowered as TileEntityPoweredBlock);
		}
	}

	public override void OnBlockValueChanged(WorldBase _world, Chunk _chunk, int _clrIdx, Vector3i _blockPos, BlockValue _oldBlockValue, BlockValue _newBlockValue)
	{
		base.OnBlockValueChanged(_world, _chunk, _clrIdx, _blockPos, _oldBlockValue, _newBlockValue);
		this.updateState(_world, _clrIdx, _blockPos, _newBlockValue);
	}

	public override void OnBlockAdded(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockAdded(world, _chunk, _blockPos, _blockValue);
		if (_blockValue.ischild)
		{
			return;
		}
		if (!(world.GetTileEntity(_chunk.ClrIdx, _blockPos) is TileEntityPoweredRangedTrap))
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

	public override TileEntityPowered CreateTileEntity(Chunk chunk)
	{
		return new TileEntityPoweredRangedTrap(chunk)
		{
			PowerItemType = PowerItem.PowerItemTypes.RangedTrap
		};
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string playSound;

	[PublicizedFrom(EAccessModifier.Private)]
	public float soundDelay = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float maxDistance;

	[PublicizedFrom(EAccessModifier.Private)]
	public string[] allowedAmmoTypes;

	[PublicizedFrom(EAccessModifier.Private)]
	public int entityDamage = 20;

	[PublicizedFrom(EAccessModifier.Private)]
	public int blockDamage = 10;

	public string AmmoItemName;

	[PublicizedFrom(EAccessModifier.Private)]
	public new BlockActivationCommand[] cmds = new BlockActivationCommand[]
	{
		new BlockActivationCommand("options", "tool", true, false),
		new BlockActivationCommand("take", "hand", false, false)
	};
}

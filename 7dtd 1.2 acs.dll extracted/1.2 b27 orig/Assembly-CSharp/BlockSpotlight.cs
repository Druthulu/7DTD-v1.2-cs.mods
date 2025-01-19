﻿using System;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockSpotlight : BlockPowered
{
	public BlockSpotlight()
	{
		this.HasTileEntity = true;
	}

	public override void Init()
	{
		base.Init();
	}

	public override void OnBlockEntityTransformAfterActivated(WorldBase _world, Vector3i _blockPos, int _cIdx, BlockValue _blockValue, BlockEntityData _ebcd)
	{
		TileEntityPowered tileEntityPowered = _world.GetTileEntity(_cIdx, _blockPos) as TileEntityPowered;
		if (tileEntityPowered == null)
		{
			ChunkCluster chunkCluster = _world.ChunkClusters[_cIdx];
			if (chunkCluster == null)
			{
				return;
			}
			Chunk chunk = (Chunk)chunkCluster.GetChunkSync(World.toChunkXZ(_blockPos.x), World.toChunkY(_blockPos.y), World.toChunkXZ(_blockPos.z));
			if (chunk == null)
			{
				return;
			}
			tileEntityPowered = this.CreateTileEntity(chunk);
			tileEntityPowered.localChunkPos = World.toBlock(_blockPos);
			tileEntityPowered.InitializePowerData();
			chunk.AddTileEntity(tileEntityPowered);
		}
		if (tileEntityPowered != null)
		{
			tileEntityPowered.WindowGroupToOpen = XUiC_PoweredSpotlightWindowGroup.ID;
		}
		SpotlightController component = _ebcd.transform.gameObject.GetComponent<SpotlightController>();
		if (component != null)
		{
			component.Init(base.Properties);
			component.TileEntity = tileEntityPowered;
		}
		base.OnBlockEntityTransformAfterActivated(_world, _blockPos, _cIdx, _blockValue, _ebcd);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool updateState(WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, bool _bChangeState = false)
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
		TileEntityPoweredBlock tileEntityPoweredBlock = (TileEntityPoweredBlock)_world.GetTileEntity(_cIdx, _blockPos);
		if (tileEntityPoweredBlock != null)
		{
			flag = (flag && tileEntityPoweredBlock.IsToggled);
		}
		if (_bChangeState)
		{
			flag = !flag;
			_blockValue.meta = (byte)(((int)_blockValue.meta & -3) | (flag ? 2 : 0));
			_world.SetBlockRPC(_cIdx, _blockPos, _blockValue);
			if (flag)
			{
				Manager.PlayInsidePlayerHead("switch_up", -1, 0f, false, false);
			}
			else
			{
				Manager.PlayInsidePlayerHead("switch_down", -1, 0f, false, false);
			}
		}
		TileEntityPowered tileEntityPowered = _world.GetTileEntity(_cIdx, _blockPos) as TileEntityPowered;
		if (tileEntityPowered == null)
		{
			tileEntityPowered = this.CreateTileEntity((Chunk)chunkSync);
			tileEntityPowered.localChunkPos = World.toBlock(_blockPos);
			tileEntityPowered.InitializePowerData();
			(chunkSync as Chunk).AddTileEntity(tileEntityPowered);
			tileEntityPowered.WindowGroupToOpen = XUiC_PoweredSpotlightWindowGroup.ID;
		}
		SpotlightController component = blockEntity.transform.gameObject.GetComponent<SpotlightController>();
		if (component != null)
		{
			component.Init(base.Properties);
			component.TileEntity = tileEntityPowered;
			component.IsOn = flag;
		}
		BlockEntityData blockEntity2 = ((World)_world).ChunkClusters[_cIdx].GetBlockEntity(_blockPos);
		if (blockEntity2 != null && blockEntity2.transform != null && blockEntity2.transform.gameObject != null)
		{
			Renderer[] componentsInChildren = blockEntity2.transform.gameObject.GetComponentsInChildren<Renderer>();
			if (componentsInChildren != null)
			{
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					if (componentsInChildren[i].material != componentsInChildren[i].sharedMaterial)
					{
						componentsInChildren[i].material = new Material(componentsInChildren[i].sharedMaterial);
					}
					if (flag)
					{
						componentsInChildren[i].material.SetColor("_EmissionColor", Color.white);
					}
					else
					{
						componentsInChildren[i].material.SetColor("_EmissionColor", Color.black);
					}
					componentsInChildren[i].sharedMaterial = componentsInChildren[i].material;
				}
			}
		}
		Transform transform = blockEntity.transform.Find("MainLight");
		if (transform != null)
		{
			LightLOD component2 = transform.GetComponent<LightLOD>();
			if (component2 != null)
			{
				component2.SwitchOnOff(flag, _blockPos);
				component2.otherLight.enabled = flag;
			}
		}
		return true;
	}

	public override bool IsMovementBlocked(IBlockAccess _world, Vector3i _blockPos, BlockValue blockDef, BlockFace face)
	{
		return false;
	}

	public override void OnBlockLoaded(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockLoaded(_world, _clrIdx, _blockPos, _blockValue);
		this.updateState(_world, _clrIdx, _blockPos, _blockValue, false);
	}

	public override void OnBlockValueChanged(WorldBase _world, Chunk _chunk, int _clrIdx, Vector3i _blockPos, BlockValue _oldBlockValue, BlockValue _newBlockValue)
	{
		base.OnBlockValueChanged(_world, _chunk, _clrIdx, _blockPos, _oldBlockValue, _newBlockValue);
		this.updateState(_world, _clrIdx, _blockPos, _newBlockValue, false);
	}

	public override bool ActivateBlock(WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, bool isOn, bool isPowered)
	{
		_blockValue.meta = (byte)(((int)_blockValue.meta & -3) | (isOn ? 2 : 0));
		_world.SetBlockRPC(_cIdx, _blockPos, _blockValue);
		this.updateState(_world, _cIdx, _blockPos, _blockValue, false);
		return true;
	}

	public static bool IsSwitchOn(byte _metadata)
	{
		return (_metadata & 2) > 0;
	}

	public override TileEntityPowered CreateTileEntity(Chunk chunk)
	{
		return new TileEntityPoweredBlock(chunk)
		{
			PowerItemType = PowerItem.PowerItemTypes.ConsumerToggle
		};
	}

	public override void OnBlockAdded(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockAdded(_world, _chunk, _blockPos, _blockValue);
		if (!(_world.GetTileEntity(_chunk.ClrIdx, _blockPos) is TileEntityPowered))
		{
			TileEntityPowered tileEntityPowered = this.CreateTileEntity(_chunk);
			tileEntityPowered.localChunkPos = World.toBlock(_blockPos);
			tileEntityPowered.InitializePowerData();
			_chunk.AddTileEntity(tileEntityPowered);
			tileEntityPowered.WindowGroupToOpen = XUiC_PoweredSpotlightWindowGroup.ID;
		}
	}

	public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		if (!_world.CanPlaceBlockAt(_blockPos, _world.GetGameManager().GetPersistentLocalPlayer(), false))
		{
			return "";
		}
		bool flag = _world.GetTileEntity(_clrIdx, _blockPos) is TileEntityPowered;
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
		ChunkCluster chunkCluster = _world.ChunkClusters[_cIdx];
		if (chunkCluster == null)
		{
			return false;
		}
		Chunk chunk = (Chunk)chunkCluster.GetChunkSync(World.toChunkXZ(_blockPos.x), World.toChunkY(_blockPos.y), World.toChunkXZ(_blockPos.z));
		if (chunk == null)
		{
			return false;
		}
		TileEntityPowered tileEntityPowered = _world.GetTileEntity(_cIdx, _blockPos) as TileEntityPowered;
		if (tileEntityPowered == null)
		{
			tileEntityPowered = this.CreateTileEntity(chunk);
			tileEntityPowered.localChunkPos = World.toBlock(_blockPos);
			tileEntityPowered.InitializePowerData();
			chunk.AddTileEntity(tileEntityPowered);
			tileEntityPowered.WindowGroupToOpen = XUiC_PoweredSpotlightWindowGroup.ID;
		}
		bool flag = (_blockValue.meta & 2) > 0;
		if (_commandName == "light")
		{
			flag = !flag;
			TileEntityPoweredBlock tileEntityPoweredBlock = tileEntityPowered as TileEntityPoweredBlock;
			if (tileEntityPoweredBlock != null)
			{
				tileEntityPoweredBlock.IsToggled = !tileEntityPoweredBlock.IsToggled;
			}
			return true;
		}
		if (_commandName == "aim")
		{
			_player.AimingGun = false;
			_world.GetGameManager().TELockServer(_cIdx, tileEntityPowered.ToWorldPos(), tileEntityPowered.entityId, _player.entityId, null);
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
		this.cmds[1].enabled = enabled;
		this.cmds[2].enabled = (flag && this.TakeDelay > 0f);
		return this.cmds;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public new BlockActivationCommand[] cmds = new BlockActivationCommand[]
	{
		new BlockActivationCommand("light", "electric_switch", true, false),
		new BlockActivationCommand("aim", "map_cursor", true, false),
		new BlockActivationCommand("take", "hand", false, false)
	};
}

using System;
using System.Collections.Generic;
using Audio;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockDoorSecure : BlockDoor
{
	public override bool AllowBlockTriggers
	{
		get
		{
			return true;
		}
	}

	public BlockDoorSecure()
	{
		this.HasTileEntity = true;
	}

	public override void Init()
	{
		base.Init();
		base.Properties.ParseString(BlockDoorSecure.PropLockedSound, ref this.lockedSound);
		base.Properties.ParseString(BlockDoorSecure.PropLockingSound, ref this.lockingSound);
		base.Properties.ParseString(BlockDoorSecure.PropUnLockingSound, ref this.unlockingSound);
	}

	public override void OnBlockAdded(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockAdded(_world, _chunk, _blockPos, _blockValue);
		if (_world.IsEditor())
		{
			return;
		}
		if (_blockValue.ischild)
		{
			return;
		}
		TileEntitySecureDoor tileEntitySecureDoor = _world.GetTileEntity(_chunk.ClrIdx, _blockPos) as TileEntitySecureDoor;
		if (tileEntitySecureDoor != null)
		{
			return;
		}
		tileEntitySecureDoor = new TileEntitySecureDoor(_chunk);
		tileEntitySecureDoor.SetDisableModifiedCheck(true);
		tileEntitySecureDoor.localChunkPos = World.toBlock(_blockPos);
		tileEntitySecureDoor.SetLocked(BlockDoorSecure.IsDoorLockedMeta(_blockValue.meta));
		tileEntitySecureDoor.SetDisableModifiedCheck(false);
		_chunk.AddTileEntity(tileEntitySecureDoor);
	}

	public override void OnBlockValueChanged(WorldBase _world, Chunk _chunk, int _clrIdx, Vector3i _blockPos, BlockValue _oldBlockValue, BlockValue _newBlockValue)
	{
		base.OnBlockValueChanged(_world, _chunk, _clrIdx, _blockPos, _oldBlockValue, _newBlockValue);
		if (_world.IsEditor())
		{
			this.SetIsLocked(BlockDoorSecure.IsDoorLockedMeta(_newBlockValue.meta), _world, _blockPos);
		}
	}

	public override bool FilterIndexType(BlockValue bv)
	{
		return !(this.IndexName == "TraderOnOff") || !BlockDoorSecure.IsDoorLockedMeta(bv.meta);
	}

	public override void OnBlockRemoved(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockRemoved(world, _chunk, _blockPos, _blockValue);
		_chunk.RemoveTileEntityAt<TileEntitySecureDoor>((World)world, World.toBlock(_blockPos));
	}

	public override bool HasBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		return _world.GetTileEntity(_clrIdx, _blockPos) is TileEntitySecureDoor || _world.IsEditor();
	}

	public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		if (_blockValue.ischild)
		{
			Vector3i parentPos = _blockValue.Block.multiBlockPos.GetParentPos(_blockPos, _blockValue);
			BlockValue block = _world.GetBlock(parentPos);
			return this.GetBlockActivationCommands(_world, block, _clrIdx, parentPos, _entityFocusing);
		}
		TileEntitySecureDoor tileEntitySecureDoor = (TileEntitySecureDoor)_world.GetTileEntity(_clrIdx, _blockPos);
		if (tileEntitySecureDoor == null && !_world.IsEditor())
		{
			return BlockActivationCommand.Empty;
		}
		PlatformUserIdentifierAbs internalLocalUserIdentifier = PlatformManager.InternalLocalUserIdentifier;
		PersistentPlayerData persistentPlayerData = (!_world.IsEditor()) ? _world.GetGameManager().GetPersistentPlayerList().GetPlayerData(tileEntitySecureDoor.GetOwner()) : null;
		bool flag = _world.IsEditor() || (!tileEntitySecureDoor.LocalPlayerIsOwner() && (persistentPlayerData != null && persistentPlayerData.ACL != null) && persistentPlayerData.ACL.Contains(internalLocalUserIdentifier));
		bool flag2 = (!_world.IsEditor()) ? tileEntitySecureDoor.IsLocked() : BlockDoorSecure.IsDoorLockedMeta(_blockValue.meta);
		bool flag3 = _world.IsEditor() || tileEntitySecureDoor.LocalPlayerIsOwner();
		bool flag4 = !_world.IsEditor() && tileEntitySecureDoor.IsUserAllowed(internalLocalUserIdentifier);
		bool flag5 = !_world.IsEditor() && tileEntitySecureDoor.HasPassword();
		((Chunk)_world.ChunkClusters[_clrIdx].GetChunkSync(World.toChunkXZ(_blockPos.x), _blockPos.y, World.toChunkXZ(_blockPos.z))).GetBlockTrigger(World.toBlock(_blockPos));
		this.cmds[0].enabled = BlockDoor.IsDoorOpen(_blockValue.meta);
		this.cmds[1].enabled = !BlockDoor.IsDoorOpen(_blockValue.meta);
		this.cmds[2].enabled = (!flag2 && (flag3 || flag || _world.IsEditor()));
		this.cmds[3].enabled = (flag2 && (flag3 || _world.IsEditor()));
		this.cmds[4].enabled = ((!flag4 && flag5 && flag2) || flag3);
		this.cmds[5].enabled = (_world.IsEditor() && !GameUtils.IsWorldEditor());
		return this.cmds;
	}

	public override bool OnBlockActivated(string _commandName, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		if (_blockValue.ischild)
		{
			Vector3i parentPos = _blockValue.Block.multiBlockPos.GetParentPos(_blockPos, _blockValue);
			BlockValue block = _world.GetBlock(parentPos);
			return this.OnBlockActivated(_commandName, _world, _cIdx, parentPos, block, _player);
		}
		TileEntitySecureDoor tileEntitySecureDoor = (TileEntitySecureDoor)_world.GetTileEntity(_cIdx, _blockPos);
		if (tileEntitySecureDoor == null && !_world.IsEditor())
		{
			return false;
		}
		bool flag = (!_world.IsEditor()) ? tileEntitySecureDoor.IsLocked() : BlockDoorSecure.IsDoorLockedMeta(_blockValue.meta);
		bool flag2 = !_world.IsEditor() && tileEntitySecureDoor.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier);
		if (!(_commandName == "close"))
		{
			if (!(_commandName == "open"))
			{
				if (_commandName == "lock")
				{
					this.SetIsLocked(true, _world, _blockPos);
					return true;
				}
				if (_commandName == "unlock")
				{
					this.SetIsLocked(false, _world, _blockPos);
					return true;
				}
				if (!(_commandName == "keypad"))
				{
					if (_commandName == "trigger")
					{
						XUiC_TriggerProperties.Show(_player.PlayerUI.xui, _cIdx, _blockPos, true, true);
					}
					return false;
				}
				LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(_player);
				if (uiforPlayer != null && tileEntitySecureDoor != null)
				{
					XUiC_KeypadWindow.Open(uiforPlayer, tileEntitySecureDoor);
				}
				return true;
			}
			else
			{
				if (_world.IsEditor() || !flag || flag2)
				{
					base.HandleTrigger(_player, (World)_world, _cIdx, _blockPos, _blockValue);
					return this.OnBlockActivated(_world, _cIdx, _blockPos, _blockValue, _player);
				}
				Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, this.lockedSound);
				return false;
			}
		}
		else
		{
			if (_world.IsEditor() || !flag || flag2)
			{
				base.HandleTrigger(_player, (World)_world, _cIdx, _blockPos, _blockValue);
				return this.OnBlockActivated(_world, _cIdx, _blockPos, _blockValue, _player);
			}
			Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, this.lockedSound);
			return false;
		}
	}

	public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		if (_blockValue.ischild)
		{
			Vector3i parentPos = _blockValue.Block.multiBlockPos.GetParentPos(_blockPos, _blockValue);
			BlockValue block = _world.GetBlock(parentPos);
			return this.GetActivationText(_world, block, _clrIdx, parentPos, _entityFocusing);
		}
		TileEntitySecureDoor tileEntitySecureDoor = (TileEntitySecureDoor)_world.GetTileEntity(_clrIdx, _blockPos);
		if (tileEntitySecureDoor == null && !_world.IsEditor())
		{
			return "";
		}
		bool flag = (!_world.IsEditor()) ? tileEntitySecureDoor.IsLocked() : BlockDoorSecure.IsDoorLockedMeta(_blockValue.meta);
		PlayerActionsLocal playerInput = ((EntityPlayerLocal)_entityFocusing).playerInput;
		string arg = playerInput.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null) + playerInput.PermanentActions.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null);
		string arg2 = Localization.Get("door", false);
		if (!flag)
		{
			return string.Format(Localization.Get("tooltipUnlocked", false), arg, arg2);
		}
		return string.Format(Localization.Get("tooltipLocked", false), arg, arg2);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool IsDoorLockedMeta(byte _metadata)
	{
		return (_metadata & 4) > 0;
	}

	public bool IsDoorLocked(WorldBase _world, Vector3i _blockPos)
	{
		BlockValue block = _world.GetBlock(_blockPos);
		if (block.isair)
		{
			return false;
		}
		if (block.ischild)
		{
			return this.IsDoorLocked(_world, _blockPos + block.parent);
		}
		if (_world.IsEditor())
		{
			return BlockDoorSecure.IsDoorLockedMeta(block.meta);
		}
		TileEntitySecureDoor tileEntitySecureDoor = _world.GetTileEntity(_blockPos) as TileEntitySecureDoor;
		return tileEntitySecureDoor != null && tileEntitySecureDoor.IsLocked();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockValue SetIsLocked(bool _isLocked, WorldBase _world, Vector3i _blockPos)
	{
		BlockValue block = _world.GetBlock(_blockPos);
		if (block.isair)
		{
			return block;
		}
		if (block.ischild)
		{
			return block;
		}
		if (_world.IsEditor())
		{
			return this.SetIsLockedEditor(_isLocked, _world, _blockPos, block);
		}
		this.SetIsLockedNonEditor(_isLocked, _world, _blockPos);
		return block;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetIsLockedNonEditor(bool _isLocked, WorldBase _world, Vector3i _blockPos)
	{
		TileEntitySecureDoor tileEntitySecureDoor = _world.GetTileEntity(_blockPos) as TileEntitySecureDoor;
		if (tileEntitySecureDoor == null || tileEntitySecureDoor.IsLocked() == _isLocked)
		{
			return;
		}
		tileEntitySecureDoor.SetLocked(_isLocked);
		this.PlayLockingSound(_blockPos);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockValue SetIsLockedEditor(bool _isLocked, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue)
	{
		byte meta = _blockValue.meta;
		byte b;
		if (_isLocked)
		{
			b = (meta | 4);
		}
		else
		{
			b = (byte)((int)meta & -5);
		}
		if (meta != b)
		{
			_blockValue.meta = b;
			_world.SetBlockRPC(_blockPos, _blockValue);
			this.PlayLockingSound(_blockPos);
		}
		return _blockValue;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PlayLockingSound(Vector3i _blockPos)
	{
		Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, this.lockingSound);
	}

	public override void OnTriggered(EntityPlayer _player, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, List<BlockChangeInfo> _blockChanges, BlockTrigger _triggeredBy)
	{
		base.OnTriggered(_player, _world, _cIdx, _blockPos, _blockValue, _blockChanges, _triggeredBy);
		if ((_blockValue.meta & 1) != 0)
		{
			Manager.BroadcastPlay(_blockPos.ToVector3() + Vector3.one * 0.5f, this.closeSound, 0f);
		}
		else
		{
			Manager.BroadcastPlay(_blockPos.ToVector3() + Vector3.one * 0.5f, this.openSound, 0f);
		}
		if (_triggeredBy != null && _triggeredBy.Unlock)
		{
			_blockValue = this.SetIsLocked(false, _world, _blockPos);
		}
		_blockValue.meta ^= 1;
		_blockChanges.Add(new BlockChangeInfo(_cIdx, _blockPos, _blockValue));
	}

	public override void OnBlockReset(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		if (_blockValue.ischild)
		{
			return;
		}
		TileEntitySecureDoor tileEntitySecureDoor = _world.GetTileEntity(_chunk.ClrIdx, _blockPos) as TileEntitySecureDoor;
		if (tileEntitySecureDoor != null)
		{
			tileEntitySecureDoor.SetLocked(BlockDoorSecure.IsDoorLockedMeta(_blockValue.meta));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cDoorIsLockedMask = 4;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string lockedSound = "Misc/locked";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string lockingSound = "Misc/locking";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string unlockingSound = "Misc/unlocking";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropLockedSound = "LockedSound";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropLockingSound = "LockingSound";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropUnLockingSound = "UnlockingSound";

	[PublicizedFrom(EAccessModifier.Private)]
	public new BlockActivationCommand[] cmds = new BlockActivationCommand[]
	{
		new BlockActivationCommand("close", "door", false, false),
		new BlockActivationCommand("open", "door", false, false),
		new BlockActivationCommand("lock", "lock", false, false),
		new BlockActivationCommand("unlock", "unlock", false, false),
		new BlockActivationCommand("keypad", "keypad", false, false),
		new BlockActivationCommand("trigger", "wrench", true, false)
	};
}

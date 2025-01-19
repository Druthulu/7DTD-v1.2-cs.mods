using System;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockSwitch : BlockPowered
{
	public BlockSwitch()
	{
		this.HasTileEntity = true;
	}

	public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		PlayerActionsLocal playerInput = ((EntityPlayerLocal)_entityFocusing).playerInput;
		string arg = playerInput.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null) + playerInput.PermanentActions.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null);
		if ((_blockValue.meta & 2) != 0)
		{
			return string.Format(Localization.Get("useSwitchLightOff", false), arg);
		}
		return string.Format(Localization.Get("useSwitchLightOn", false), arg);
	}

	public override bool OnBlockActivated(string _commandName, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		if (!(_commandName == "light"))
		{
			if (!(_commandName == "take"))
			{
				return false;
			}
			base.TakeItemWithTimer(_cIdx, _blockPos, _blockValue, _player);
			return true;
		}
		else
		{
			if (!(_world.GetTileEntity(_cIdx, _blockPos) is TileEntityPoweredTrigger))
			{
				return false;
			}
			this.updateState(_world, _cIdx, _blockPos, _blockValue, true);
			return true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool updateState(WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, bool _bChangeState = false)
	{
		ChunkCluster chunkCluster = _world.ChunkClusters[_cIdx];
		if (chunkCluster == null)
		{
			return false;
		}
		if (chunkCluster.GetChunkSync(World.toChunkXZ(_blockPos.x), World.toChunkY(_blockPos.y), World.toChunkXZ(_blockPos.z)) == null)
		{
			return false;
		}
		bool flag = (_blockValue.meta & 1) > 0;
		bool flag2 = (_blockValue.meta & 2) > 0;
		if (_bChangeState)
		{
			flag2 = !flag2;
			_blockValue.meta = (byte)(((int)_blockValue.meta & -3) | (flag2 ? 2 : 0));
			_blockValue.meta = (byte)(((int)_blockValue.meta & -2) | (flag ? 1 : 0));
			_world.SetBlockRPC(_cIdx, _blockPos, _blockValue);
			if (flag2)
			{
				Manager.BroadcastPlay(_blockPos.ToVector3(), "switch_up", 0f);
			}
			else
			{
				Manager.BroadcastPlay(_blockPos.ToVector3(), "switch_down", 0f);
			}
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			TileEntityPoweredTrigger tileEntityPoweredTrigger = _world.GetTileEntity(_cIdx, _blockPos) as TileEntityPoweredTrigger;
			if (tileEntityPoweredTrigger != null)
			{
				tileEntityPoweredTrigger.IsTriggered = flag2;
			}
		}
		BlockEntityData blockEntity = ((World)_world).ChunkClusters[_cIdx].GetBlockEntity(_blockPos);
		if (blockEntity != null && blockEntity.transform != null && blockEntity.transform.gameObject != null)
		{
			Renderer[] componentsInChildren = blockEntity.transform.gameObject.GetComponentsInChildren<Renderer>();
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
						componentsInChildren[i].material.SetColor("_EmissionColor", flag2 ? Color.green : Color.red);
					}
					else
					{
						componentsInChildren[i].material.SetColor("_EmissionColor", Color.black);
					}
					componentsInChildren[i].sharedMaterial = componentsInChildren[i].material;
					componentsInChildren[i].material.EnableKeyword("_EMISSION");
				}
			}
		}
		return true;
	}

	public override void OnBlockEntityTransformAfterActivated(WorldBase _world, Vector3i _blockPos, int _cIdx, BlockValue _blockValue, BlockEntityData _ebcd)
	{
		base.OnBlockEntityTransformAfterActivated(_world, _blockPos, _cIdx, _blockValue, _ebcd);
		this.updateState(_world, _cIdx, _blockPos, _blockValue, false);
	}

	public override void OnBlockValueChanged(WorldBase _world, Chunk _chunk, int _clrIdx, Vector3i _blockPos, BlockValue _oldBlockValue, BlockValue _newBlockValue)
	{
		base.OnBlockValueChanged(_world, _chunk, _clrIdx, _blockPos, _oldBlockValue, _newBlockValue);
		this.updateState(_world, _clrIdx, _blockPos, _newBlockValue, false);
		BlockEntityData blockEntity = ((World)_world).ChunkClusters[_clrIdx].GetBlockEntity(_blockPos);
		this.updateAnimState(blockEntity, BlockSwitch.IsSwitchOn(_newBlockValue.meta), _newBlockValue);
	}

	public override bool HasBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		return true;
	}

	public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		bool flag = _world.IsMyLandProtectedBlock(_blockPos, _world.GetGameManager().GetPersistentLocalPlayer(), false);
		this.cmds[0].enabled = true;
		this.cmds[1].enabled = (flag && this.TakeDelay > 0f);
		return this.cmds;
	}

	public override bool ActivateBlock(WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, bool isOn, bool isPowered)
	{
		_blockValue.meta = (byte)(((int)_blockValue.meta & -3) | (isOn ? 2 : 0));
		_blockValue.meta = (byte)(((int)_blockValue.meta & -2) | (isPowered ? 1 : 0));
		_world.SetBlockRPC(_cIdx, _blockPos, _blockValue);
		this.updateState(_world, _cIdx, _blockPos, _blockValue, false);
		return true;
	}

	public override TileEntityPowered CreateTileEntity(Chunk chunk)
	{
		return new TileEntityPoweredTrigger(chunk);
	}

	public static bool IsSwitchOn(byte _metadata)
	{
		return (_metadata & 2) > 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateAnimState(BlockEntityData _ebcd, bool _bOpen, BlockValue _blockValue)
	{
		Animator[] componentsInChildren;
		if (_ebcd != null && _ebcd.bHasTransform && (componentsInChildren = _ebcd.transform.GetComponentsInChildren<Animator>()) != null)
		{
			foreach (Animator animator in componentsInChildren)
			{
				animator.SetBool("SwitchActivated", _bOpen);
				animator.SetTrigger("SwitchTrigger");
			}
		}
	}

	public override void ForceAnimationState(BlockValue _blockValue, BlockEntityData _ebcd)
	{
		Animator[] componentsInChildren;
		if (_ebcd != null && _ebcd.bHasTransform && (componentsInChildren = _ebcd.transform.GetComponentsInChildren<Animator>(false)) != null)
		{
			bool flag = BlockSwitch.IsSwitchOn(_blockValue.meta);
			foreach (Animator animator in componentsInChildren)
			{
				animator.SetBool("SwitchActivated", flag);
				if (flag)
				{
					animator.CrossFade("SwitchOnStatic", 0f);
				}
				else
				{
					animator.CrossFade("SwitchOffStatic", 0f);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public new BlockActivationCommand[] cmds = new BlockActivationCommand[]
	{
		new BlockActivationCommand("light", "electric_switch", false, false),
		new BlockActivationCommand("take", "hand", false, false)
	};
}

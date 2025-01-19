﻿using System;
using UnityEngine.Scripting;

[Preserve]
public class BlockRallyMarker : Block
{
	public BlockRallyMarker()
	{
		this.StabilityIgnore = true;
	}

	public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		PlayerActionsLocal playerInput = ((EntityPlayerLocal)_entityFocusing).playerInput;
		Quest quest = ((EntityPlayerLocal)_entityFocusing).QuestJournal.HasQuestAtRallyPosition(_blockPos.ToVector3(), true);
		if (quest != null && !quest.RallyMarkerActivated && ((EntityPlayerLocal)_entityFocusing).QuestJournal.ActiveQuest == null)
		{
			string arg = playerInput.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null) + playerInput.PermanentActions.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null);
			_blockValue.Block.GetLocalizedBlockName();
			return string.Format(Localization.Get("questRallyActivate", false), arg, quest.QuestClass.Name);
		}
		return string.Empty;
	}

	public override bool OnBlockActivated(string _commandName, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		if (_commandName == "activate")
		{
			Quest quest = _player.QuestJournal.HasQuestAtRallyPosition(_blockPos.ToVector3(), true);
			if (quest != null && !quest.RallyMarkerActivated && _player.QuestJournal.ActiveQuest == null)
			{
				QuestEventManager.Current.HandleRallyMarkerActivate(_player, _blockPos, _blockValue);
				return true;
			}
		}
		return false;
	}

	public override bool HasBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		return true;
	}

	public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		this.cmds[0].enabled = true;
		return this.cmds;
	}

	public override void OnBlockLoaded(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockLoaded(_world, _clrIdx, _blockPos, _blockValue);
		if (_blockValue.ischild)
		{
			return;
		}
		ChunkCluster chunkCluster = _world.ChunkClusters[_clrIdx];
		if (chunkCluster == null)
		{
			return;
		}
		Chunk chunk = (Chunk)chunkCluster.GetChunkFromWorldPos(_blockPos);
		if (chunk == null)
		{
			return;
		}
		chunk.AddEntityBlockStub(new BlockEntityData(_blockValue, _blockPos)
		{
			bNeedsTemperature = true
		});
	}

	public override void OnBlockEntityTransformAfterActivated(WorldBase _world, Vector3i _blockPos, int _cIdx, BlockValue _blockValue, BlockEntityData _ebcd)
	{
		base.OnBlockEntityTransformAfterActivated(_world, _blockPos, _cIdx, _blockValue, _ebcd);
		if (_world.IsEditor())
		{
			return;
		}
		_ebcd.transform.GetChild(0).gameObject.SetActive(false);
	}

	public void ShowRallyMarker(bool _show, WorldBase _world, Vector3i _blockPos, int _clrIdx)
	{
		ChunkCluster chunkCluster = _world.ChunkClusters[_clrIdx];
		if (chunkCluster == null)
		{
			return;
		}
		Chunk chunk = (Chunk)chunkCluster.GetChunkFromWorldPos(_blockPos);
		if (chunk == null)
		{
			return;
		}
		chunk.GetBlockEntity(_blockPos).transform.GetChild(0).gameObject.SetActive(_show);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public new BlockActivationCommand[] cmds = new BlockActivationCommand[]
	{
		new BlockActivationCommand("activate", "electric_switch", false, false)
	};
}

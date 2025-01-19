using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockCarExplode : Block
{
	public override bool AllowBlockTriggers
	{
		get
		{
			return true;
		}
	}

	public ExplosionData Explosion
	{
		get
		{
			return this.explosion;
		}
	}

	public override void Init()
	{
		base.Init();
		this.explosion = new ExplosionData(base.Properties, null);
	}

	public override Block.DestroyedResult OnBlockDestroyedBy(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, int _entityId, bool _bUseHarvestTool)
	{
		Block.DestroyedResult result = base.OnBlockDestroyedBy(_world, _clrIdx, _blockPos, _blockValue, _entityId, _bUseHarvestTool);
		if (_blockValue.ischild)
		{
			return Block.DestroyedResult.Keep;
		}
		if (!_bUseHarvestTool)
		{
			this.explode(_blockPos);
			return Block.DestroyedResult.Remove;
		}
		return result;
	}

	public override Block.DestroyedResult OnBlockDestroyedByExplosion(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, int _playerThatStartedExpl)
	{
		base.OnBlockDestroyedByExplosion(_world, _clrIdx, _blockPos, _blockValue, _playerThatStartedExpl);
		if (_blockValue.ischild)
		{
			return Block.DestroyedResult.Keep;
		}
		this.explode(_blockPos);
		return Block.DestroyedResult.Remove;
	}

	public override void OnBlockStartsToFall(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockStartsToFall(_world, _blockPos, _blockValue);
		this.explode(_blockPos);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void explode(Vector3i _blockPos)
	{
		if (this.explosion.ParticleIndex == 0)
		{
			return;
		}
		GameManager instance = GameManager.Instance;
		Quaternion rotation = Quaternion.identity;
		BlockEntityData blockEntity = ((Chunk)instance.World.GetChunkFromWorldPos(_blockPos)).GetBlockEntity(_blockPos);
		if (blockEntity != null && blockEntity.transform)
		{
			rotation = blockEntity.transform.rotation;
		}
		instance.ExplosionServer(0, _blockPos.ToVector3(), _blockPos, rotation, this.explosion, -1, 0.1f, false, null);
	}

	public override void OnBlockEntityTransformAfterActivated(WorldBase _world, Vector3i _blockPos, int _cIdx, BlockValue _blockValue, BlockEntityData _ebcd)
	{
		base.OnBlockEntityTransformAfterActivated(_world, _blockPos, _cIdx, _blockValue, _ebcd);
	}

	public override void OnBlockValueChanged(WorldBase _world, Chunk _chunk, int _clrIdx, Vector3i _blockPos, BlockValue _oldBlockValue, BlockValue _newBlockValue)
	{
		base.OnBlockValueChanged(_world, _chunk, _clrIdx, _blockPos, _oldBlockValue, _newBlockValue);
	}

	public override int OnBlockDamaged(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, int _damagePoints, int _entityIdThatDamaged, ItemActionAttack.AttackHitInfo _attackHitInfo, bool _bUseHarvestTool, bool _bBypassMaxDamage, int _recDepth = 0)
	{
		return base.OnBlockDamaged(_world, _clrIdx, _blockPos, _blockValue, _damagePoints, _entityIdThatDamaged, _attackHitInfo, _bUseHarvestTool, _bBypassMaxDamage, _recDepth);
	}

	public override BlockValue OnBlockPlaced(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, GameRandom _rnd)
	{
		_blockValue.rotation = (byte)(_rnd.RandomFloat * 4f);
		return _blockValue;
	}

	public override bool HasBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		return true;
	}

	public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		this.cmds[0].enabled = (_world.IsEditor() && !GameUtils.IsWorldEditor());
		return this.cmds;
	}

	public override bool OnBlockActivated(string _commandName, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		if (_commandName == "trigger")
		{
			XUiC_TriggerProperties.Show(_player.PlayerUI.xui, _cIdx, _blockPos, false, true);
		}
		return false;
	}

	public override void OnTriggered(EntityPlayer _player, WorldBase _world, int cIdx, Vector3i _blockPos, BlockValue _blockValue, List<BlockChangeInfo> _blockChanges, BlockTrigger _triggeredBy)
	{
		base.OnTriggered(_player, _world, cIdx, _blockPos, _blockValue, _blockChanges, _triggeredBy);
		this.explode(_blockPos);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ExplosionData explosion;

	[PublicizedFrom(EAccessModifier.Private)]
	public new BlockActivationCommand[] cmds = new BlockActivationCommand[]
	{
		new BlockActivationCommand("trigger", "wrench", true, false)
	};
}

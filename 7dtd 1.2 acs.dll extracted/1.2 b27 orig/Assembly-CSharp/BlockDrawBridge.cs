using System;
using UnityEngine.Scripting;

[Preserve]
public class BlockDrawBridge : BlockDoorSecure
{
	public override bool IsMovementBlocked(IBlockAccess _world, Vector3i _blockPos, BlockValue _blockValue, BlockFace _face)
	{
		if (this.isMultiBlock && _blockValue.ischild)
		{
			Vector3i parentPos = this.multiBlockPos.GetParentPos(_blockPos, _blockValue);
			return !BlockDoor.IsDoorOpen(_world.GetBlock(parentPos).meta) || _blockPos.y == parentPos.y;
		}
		return true;
	}

	public override bool IsMovementBlocked(IBlockAccess _world, Vector3i _blockPos, BlockValue _blockValue, BlockFaceFlag _sides)
	{
		if (this.isMultiBlock && _blockValue.ischild)
		{
			Vector3i parentPos = this.multiBlockPos.GetParentPos(_blockPos, _blockValue);
			return !BlockDoor.IsDoorOpen(_world.GetBlock(parentPos).meta) || _blockPos.y == parentPos.y;
		}
		return true;
	}

	public override float GetStepHeight(IBlockAccess world, Vector3i _blockPos, BlockValue _blockValue, BlockFace crossingFace)
	{
		if (world == null)
		{
			return 0f;
		}
		Vector3i vector3i = _blockPos;
		if (this.isMultiBlock && _blockValue.ischild)
		{
			vector3i = this.multiBlockPos.GetParentPos(_blockPos, _blockValue);
			_blockValue = world.GetBlock(vector3i);
		}
		if (!BlockDoor.IsDoorOpen(_blockValue.meta))
		{
			return 1f;
		}
		if (_blockPos.y == vector3i.y)
		{
			return 1f;
		}
		return 0f;
	}

	public override void OnBlockPlaceBefore(WorldBase _world, ref BlockPlacement.Result _bpResult, EntityAlive _ea, GameRandom _rnd)
	{
		_bpResult.blockValue.meta = (_bpResult.blockValue.meta | 1);
	}
}

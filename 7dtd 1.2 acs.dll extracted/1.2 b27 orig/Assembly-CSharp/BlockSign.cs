﻿using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockSign : Block
{
	public override void Init()
	{
		base.Init();
		if (base.Properties.Values.ContainsKey("UpwardsCount"))
		{
			this.upwardsCount = int.Parse(base.Properties.Values["UpwardsCount"]);
		}
		else
		{
			this.upwardsCount = 1;
		}
		this.IsTerrainDecoration = true;
		this.CanDecorateOnSlopes = false;
	}

	public override void OnBlockAdded(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		if ((_blockValue.meta & 1) == 0)
		{
			this.shape.OnBlockAdded(world, _chunk, _blockPos, _blockValue);
			_blockValue.meta |= 1;
			for (int i = _blockPos.y + 1; i < _blockPos.y + this.upwardsCount + 1; i++)
			{
				_chunk.SetBlockRaw(World.toBlockXZ(_blockPos.x), i, World.toBlockXZ(_blockPos.z), _blockValue);
			}
		}
	}

	public override void OnBlockRemoved(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		if (_chunk == null)
		{
			_chunk = (Chunk)_world.GetChunkFromWorldPos(_blockPos);
		}
		if (_chunk == null)
		{
			return;
		}
		if ((_blockValue.meta & 1) == 0)
		{
			this.shape.OnBlockRemoved(_world, _chunk, _blockPos, _blockValue);
			for (int i = _blockPos.y + 1; i < _blockPos.y + this.upwardsCount + 1; i++)
			{
				_chunk.SetBlockRaw(World.toBlockXZ(_blockPos.x), i, World.toBlockXZ(_blockPos.z), BlockValue.Air);
			}
			return;
		}
		if (_world.IsRemote())
		{
			return;
		}
		int num = _blockPos.y - 1;
		while (num >= _blockPos.y - this.upwardsCount && _blockPos.y >= 1)
		{
			BlockValue block = _world.GetBlock(_blockPos.x, num, _blockPos.z);
			if (block.type != _blockValue.type)
			{
				break;
			}
			if ((block.meta & 1) == 0)
			{
				_world.SetBlockRPC(new Vector3i(_blockPos.x, num, _blockPos.z), BlockValue.Air);
				return;
			}
			num--;
		}
	}

	public override void RenderDecorations(Vector3i _worldPos, BlockValue _blockValue, Vector3 _drawPos, Vector3[] _vertices, LightingAround _lightingAround, long _fulltexture, VoxelMesh[] _meshes, INeighborBlockCache _nBlocks)
	{
		if ((_blockValue.meta & 1) == 0)
		{
			this.shape.renderDecorations(_worldPos, _blockValue, _drawPos, _vertices, _lightingAround, _fulltexture, _meshes, _nBlocks);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int upwardsCount;
}

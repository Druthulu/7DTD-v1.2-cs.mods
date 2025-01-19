using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockSpawnEntity : Block
{
	public override void Init()
	{
		base.Init();
		if (!base.Properties.Values.ContainsKey("SpawnClass"))
		{
			throw new Exception(string.Format("Need 'SpawnClass' in block {0}", base.GetBlockName()));
		}
		this.spawnClasses = base.Properties.Values["SpawnClass"].Split(',', StringSplitOptions.None);
	}

	public override void OnBlockAdded(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockAdded(_world, _chunk, _blockPos, _blockValue);
		if (!GameManager.Instance.IsEditMode() && SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && this.spawnClasses.Length != 0)
		{
			EntityCreationData entityCreationData = new EntityCreationData();
			entityCreationData.id = -1;
			string text = this.spawnClasses[(int)_blockValue.meta % this.spawnClasses.Length];
			entityCreationData.entityClass = text.GetHashCode();
			entityCreationData.pos = _blockPos.ToVector3() + new Vector3(0.5f, 0.25f, 0.5f);
			entityCreationData.rot = new Vector3(0f, (float)(90 * (_blockValue.rotation & 3)), 0f);
			_chunk.AddEntityStub(entityCreationData);
			_world.GetWBT().AddScheduledBlockUpdate(0, _blockPos, this.blockID, 80UL);
		}
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
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			_world.GetWBT().AddScheduledBlockUpdate(0, _blockPos, this.blockID, 300UL);
		}
	}

	public override void OnBlockEntityTransformAfterActivated(WorldBase _world, Vector3i _blockPos, int _cIdx, BlockValue _blockValue, BlockEntityData _ebcd)
	{
		base.OnBlockEntityTransformAfterActivated(_world, _blockPos, _cIdx, _blockValue, _ebcd);
		if (_blockValue.ischild)
		{
			return;
		}
		if (!_world.IsEditor())
		{
			_ebcd.transform.gameObject.SetActive(false);
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			_world.GetWBT().AddScheduledBlockUpdate(0, _blockPos, this.blockID, 300UL);
		}
	}

	public override bool UpdateTick(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, bool _bRandomTick, ulong _ticksIfLoaded, GameRandom _rnd)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			if (GameManager.Instance.World.GetEntitiesInBounds(null, new Bounds(_blockPos.ToVector3(), Vector3.one * 2f)).Count == 0 && !GameManager.Instance.IsEditMode())
			{
				ChunkCluster chunkCluster = _world.ChunkClusters[_clrIdx];
				if (chunkCluster == null)
				{
					return false;
				}
				if ((Chunk)chunkCluster.GetChunkFromWorldPos(_blockPos) == null)
				{
					return false;
				}
				if (this.spawnClasses.Length != 0)
				{
					string b = this.spawnClasses[(int)_blockValue.meta % this.spawnClasses.Length];
					int et = -1;
					foreach (KeyValuePair<int, EntityClass> keyValuePair in EntityClass.list.Dict)
					{
						if (keyValuePair.Value.entityClassName == b)
						{
							et = keyValuePair.Key;
						}
					}
					Vector3 transformPos = _blockPos.ToVector3() + new Vector3(0.5f, 0.25f, 0.5f);
					Vector3 rotation = new Vector3(0f, (float)(90 * (_blockValue.rotation & 3)), 0f);
					Entity entity = EntityFactory.CreateEntity(et, transformPos, rotation);
					entity.SetSpawnerSource(EnumSpawnerSource.StaticSpawner);
					GameManager.Instance.World.SpawnEntityInWorld(entity);
					Log.Out("BlockSpawnEntity:: Spawn New Trader.");
				}
			}
			_world.GetWBT().AddScheduledBlockUpdate(0, _blockPos, this.blockID, 320UL);
		}
		return base.UpdateTick(_world, _clrIdx, _blockPos, _blockValue, _bRandomTick, _ticksIfLoaded, _rnd);
	}

	public string[] spawnClasses;
}

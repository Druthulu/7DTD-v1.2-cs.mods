using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockSleeper : Block
{
	public BlockSleeper()
	{
		this.IsSleeperBlock = true;
		this.HasTileEntity = true;
	}

	public override void Init()
	{
		base.Init();
		base.Properties.ParseInt(BlockSleeper.PropPose, ref this.pose);
		this.look = Vector3.forward;
		base.Properties.ParseVec(BlockSleeper.PropLookIdentity, ref this.look);
		string @string = base.Properties.GetString(BlockSleeper.PropExcludeWalkType);
		if (@string.Length > 0)
		{
			string[] array = @string.Split(',', StringSplitOptions.None);
			this.excludedWalkTypes = new List<int>();
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] == "Crawler")
				{
					this.excludedWalkTypes.Add(21);
				}
				else
				{
					Log.Warning("Block {0}, invalid ExcludeWalkType {1}", new object[]
					{
						base.GetBlockName(),
						array[i]
					});
				}
			}
		}
		base.Properties.ParseString(BlockSleeper.PropSpawnGroup, ref this.spawnGroup);
		base.Properties.ParseEnum<BlockSleeper.eMode>(BlockSleeper.PropSpawnMode, ref this.spawnMode);
	}

	public override bool CanPlaceBlockAt(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, bool _bOmitCollideCheck = false)
	{
		return _world.IsEditor() || base.CanPlaceBlockAt(_world, _clrIdx, _blockPos, _blockValue, _bOmitCollideCheck);
	}

	public float GetSleeperRotation(BlockValue _blockValue)
	{
		byte rotation = _blockValue.rotation;
		switch (rotation)
		{
		case 1:
			return 90f;
		case 2:
			return 180f;
		case 3:
			return 270f;
		default:
			switch (rotation)
			{
			case 24:
				return 45f;
			case 25:
				return 135f;
			case 26:
				return 225f;
			case 27:
				return 315f;
			default:
				return 0f;
			}
			break;
		}
	}

	public bool ExcludesWalkType(int _walkType)
	{
		return this.excludedWalkTypes != null && this.excludedWalkTypes.Contains(_walkType);
	}

	public override void OnBlockAdded(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockAdded(_world, _chunk, _blockPos, _blockValue);
		_chunk.AddTileEntity(new TileEntitySleeper(_chunk)
		{
			localChunkPos = World.toBlock(_blockPos)
		});
	}

	public override void OnBlockRemoved(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockRemoved(_world, _chunk, _blockPos, _blockValue);
		_chunk.RemoveTileEntityAt<TileEntitySleeper>((World)_world, World.toBlock(_blockPos));
	}

	public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		if (_world.IsEditor())
		{
			return "Configure Sleeper";
		}
		return null;
	}

	public override bool OnBlockActivated(string _commandName, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		TileEntitySleeper tileEntitySleeper = _world.GetTileEntity(_cIdx, _blockPos) as TileEntitySleeper;
		if (tileEntitySleeper == null)
		{
			return false;
		}
		if (_player != null)
		{
			XUiC_WoPropsSleeperBlock.Open(_player.PlayerUI, tileEntitySleeper);
			return true;
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

	public override bool IsTileEntitySavedInPrefab()
	{
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string PropPose = "Pose";

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string PropLookIdentity = "LookIdentity";

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string PropExcludeWalkType = "ExcludeWalkType";

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string PropSpawnGroup = "SpawnGroup";

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string PropSpawnMode = "SpawnMode";

	public int pose;

	public Vector3 look;

	public string spawnGroup;

	public BlockSleeper.eMode spawnMode;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<int> excludedWalkTypes;

	[PublicizedFrom(EAccessModifier.Private)]
	public new BlockActivationCommand[] cmds = new BlockActivationCommand[]
	{
		new BlockActivationCommand("open", "dummy", true, false)
	};

	public enum eMode
	{
		Normal,
		Bandit,
		Infested
	}
}

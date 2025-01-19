using System;
using System.Collections.Generic;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockLandClaim : Block
{
	public BlockLandClaim()
	{
		base.IsNotifyOnLoadUnload = true;
		this.activePrompt = Localization.Get("activeBlockPrompt", false);
		this.inactivePrompt = Localization.Get("inactiveBlockPrompt", false);
	}

	public bool ServerCheckPrimary(Vector3i _blockPos)
	{
		return GameManager.Instance.persistentPlayers.GetLandProtectionBlockOwner(_blockPos) != null;
	}

	public static bool IsPrimary(BlockValue _blockValue)
	{
		return (_blockValue.meta & 2) == 0;
	}

	public override void OnBlockLoaded(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockLoaded(_world, _clrIdx, _blockPos, _blockValue);
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			bool flag = this.ServerCheckPrimary(_blockPos);
			if (flag != BlockLandClaim.IsPrimary(_blockValue))
			{
				_blockValue.meta = (byte)(((int)_blockValue.meta & -3) | ((!flag) ? 2 : 0));
				if (!BlockLandClaim.IsPrimary(_blockValue))
				{
					_blockValue.damage = this.MaxDamage - 1;
				}
				_world.SetBlockRPC(_clrIdx, _blockPos, _blockValue);
			}
		}
		if (!BlockLandClaim.IsPrimary(_blockValue))
		{
			NavObjectManager.Instance.UnRegisterNavObjectByPosition(_blockPos.ToVector3(), "land_claim");
			if (GameManager.Instance.persistentPlayers.m_lpBlockMap.ContainsKey(_blockPos))
			{
				PersistentPlayerData persistentPlayerData = GameManager.Instance.persistentPlayers.m_lpBlockMap[_blockPos];
				GameManager.Instance.persistentPlayers.m_lpBlockMap.Remove(_blockPos);
				persistentPlayerData.LPBlocks.Remove(_blockPos);
			}
		}
		TileEntityLandClaim tileEntityLandClaim = _world.GetTileEntity(_clrIdx, _blockPos) as TileEntityLandClaim;
		if (tileEntityLandClaim != null)
		{
			if (!BlockLandClaim.IsPrimary(_blockValue))
			{
				tileEntityLandClaim.ShowBounds = false;
			}
			if (tileEntityLandClaim.IsOwner(PlatformManager.InternalLocalUserIdentifier))
			{
				Transform boundsHelper = LandClaimBoundsHelper.GetBoundsHelper(_blockPos.ToVector3());
				if (boundsHelper != null)
				{
					tileEntityLandClaim.BoundsHelper = boundsHelper;
					boundsHelper.gameObject.SetActive(tileEntityLandClaim.ShowBounds);
				}
			}
		}
	}

	public override void OnBlockValueChanged(WorldBase _world, Chunk _chunk, int _clrIdx, Vector3i _blockPos, BlockValue _oldBlockValue, BlockValue _newBlockValue)
	{
		base.OnBlockValueChanged(_world, _chunk, _clrIdx, _blockPos, _oldBlockValue, _newBlockValue);
		if (!BlockLandClaim.IsPrimary(_newBlockValue))
		{
			TileEntityLandClaim tileEntityLandClaim = (TileEntityLandClaim)_world.GetTileEntity(_clrIdx, _blockPos);
			if (tileEntityLandClaim != null)
			{
				tileEntityLandClaim.ShowBounds = false;
				Transform boundsHelper = LandClaimBoundsHelper.GetBoundsHelper(_blockPos.ToVector3());
				if (boundsHelper != null)
				{
					tileEntityLandClaim.BoundsHelper = null;
					boundsHelper.gameObject.SetActive(false);
					LandClaimBoundsHelper.RemoveBoundsHelper(_blockPos.ToVector3());
				}
			}
		}
	}

	public override void OnBlockEntityTransformAfterActivated(WorldBase _world, Vector3i _blockPos, int _cIdx, BlockValue _blockValue, BlockEntityData _ebcd)
	{
		base.OnBlockEntityTransformAfterActivated(_world, _blockPos, _cIdx, _blockValue, _ebcd);
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			bool flag = this.ServerCheckPrimary(_blockPos);
			if (flag != BlockLandClaim.IsPrimary(_blockValue))
			{
				_blockValue.meta = (byte)(((int)_blockValue.meta & -3) | ((!flag) ? 2 : 0));
				if (!BlockLandClaim.IsPrimary(_blockValue))
				{
					_blockValue.damage = this.MaxDamage - 1;
				}
				_world.SetBlockRPC(0, _blockPos, _blockValue);
			}
		}
		if (_ebcd == null)
		{
			return;
		}
		Chunk chunk = (Chunk)((World)_world).GetChunkFromWorldPos(_blockPos);
		TileEntityLandClaim tileEntityLandClaim = (TileEntityLandClaim)_world.GetTileEntity(_cIdx, _blockPos);
		if (tileEntityLandClaim == null)
		{
			tileEntityLandClaim = new TileEntityLandClaim(chunk);
			if (tileEntityLandClaim != null)
			{
				tileEntityLandClaim.localChunkPos = World.toBlock(_blockPos);
				chunk.AddTileEntity(tileEntityLandClaim);
			}
		}
		if (tileEntityLandClaim == null)
		{
			Log.Error("Tile Entity Land Claim was unable to be created!");
			return;
		}
	}

	public override void OnBlockAdded(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockAdded(_world, _chunk, _blockPos, _blockValue);
		TileEntityLandClaim tileEntityLandClaim = _world.GetTileEntity(_chunk.ClrIdx, _blockPos) as TileEntityLandClaim;
		if (tileEntityLandClaim == null)
		{
			TileEntityLandClaim tileEntityLandClaim2 = new TileEntityLandClaim(_chunk);
			tileEntityLandClaim2.localChunkPos = World.toBlock(_blockPos);
			_chunk.AddTileEntity(tileEntityLandClaim2);
			tileEntityLandClaim = tileEntityLandClaim2;
		}
		if (tileEntityLandClaim.IsOwner(PlatformManager.InternalLocalUserIdentifier))
		{
			Transform boundsHelper = LandClaimBoundsHelper.GetBoundsHelper(_blockPos.ToVector3());
			if (boundsHelper != null)
			{
				tileEntityLandClaim.BoundsHelper = boundsHelper;
				boundsHelper.gameObject.SetActive(tileEntityLandClaim.ShowBounds);
			}
		}
	}

	public override void OnBlockRemoved(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockRemoved(_world, _chunk, _blockPos, _blockValue);
		if (_world.GetTileEntity(_chunk.ClrIdx, _blockPos) is TileEntityLandClaim)
		{
			LandClaimBoundsHelper.RemoveBoundsHelper(_blockPos.ToVector3());
		}
	}

	public override void OnBlockUnloaded(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockUnloaded(_world, _clrIdx, _blockPos, _blockValue);
		LandClaimBoundsHelper.RemoveBoundsHelper(_blockPos.ToVector3());
	}

	public override void PlaceBlock(WorldBase _world, BlockPlacement.Result _result, EntityAlive _ea)
	{
		base.PlaceBlock(_world, _result, _ea);
		TileEntityLandClaim tileEntityLandClaim = (TileEntityLandClaim)_world.GetTileEntity(_result.clrIdx, _result.blockPos);
		if (tileEntityLandClaim != null)
		{
			tileEntityLandClaim.SetOwner(PlatformManager.InternalLocalUserIdentifier);
		}
	}

	public override string GetCustomDescription(Vector3i _blockPos, BlockValue _bv)
	{
		if (BlockLandClaim.IsPrimary(_bv))
		{
			return string.Format(this.activePrompt, this.GetLocalizedBlockName());
		}
		return string.Format(this.inactivePrompt, this.GetLocalizedBlockName());
	}

	public void HandleDeactivatingCurrentLandClaims(PersistentPlayerData ppData)
	{
		List<Vector3i> landProtectionBlocks = ppData.GetLandProtectionBlocks();
		World world = GameManager.Instance.World;
		int @int = GameStats.GetInt(EnumGameStats.LandClaimCount);
		if (landProtectionBlocks.Count > @int)
		{
			int num = landProtectionBlocks.Count - @int;
			for (int i = 0; i < num; i++)
			{
				Vector3i vector3i = landProtectionBlocks[0];
				BlockValue block = world.GetBlock(vector3i);
				if (!block.isair)
				{
					if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
					{
						block.meta = 2;
						block.damage = this.MaxDamage - 1;
						world.SetBlockRPC(0, vector3i, block);
						SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEntityMapMarkerRemove>().Setup(EnumMapObjectType.LandClaim, vector3i.ToVector3()), false, -1, -1, -1, null, 192);
					}
					NavObjectManager.Instance.UnRegisterNavObjectByPosition(vector3i.ToVector3(), "land_claim");
					LandClaimBoundsHelper.RemoveBoundsHelper(vector3i.ToVector3());
				}
				GameManager.Instance.persistentPlayers.m_lpBlockMap.Remove(vector3i);
				ppData.LPBlocks.RemoveAt(0);
			}
		}
	}

	public static void HandleDeactivateLandClaim(Vector3i _blockPos)
	{
		World world = GameManager.Instance.World;
		BlockValue block = world.GetBlock(_blockPos);
		if (!block.isair)
		{
			block.meta = 2;
			block.damage = block.Block.MaxDamage - 1;
			world.SetBlockRPC(0, _blockPos, block);
			GameManager.Instance.persistentPlayers.m_lpBlockMap.Remove(_blockPos);
			NavObjectManager.Instance.UnRegisterNavObjectByPosition(_blockPos.ToVector3(), "land_claim");
			LandClaimBoundsHelper.RemoveBoundsHelper(_blockPos.ToVector3());
		}
	}

	public override bool CanRepair(BlockValue _blockValue)
	{
		return base.CanRepair(_blockValue) && BlockLandClaim.IsPrimary(_blockValue);
	}

	public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		if (BlockLandClaim.IsPrimary(_blockValue))
		{
			TileEntityLandClaim tileEntityLandClaim = (TileEntityLandClaim)_world.GetTileEntity(_clrIdx, _blockPos);
			string str = "";
			if (tileEntityLandClaim != null)
			{
				str = (tileEntityLandClaim.IsOwner(PlatformManager.InternalLocalUserIdentifier) ? ("\n" + Localization.Get("useWorkstation", false)) : "");
			}
			return this.GetCustomDescription(_blockPos, _blockValue) + str;
		}
		return this.GetCustomDescription(_blockPos, _blockValue);
	}

	public override bool OnBlockActivated(string _commandName, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		if (_blockValue.ischild)
		{
			Vector3i parentPos = _blockValue.Block.multiBlockPos.GetParentPos(_blockPos, _blockValue);
			BlockValue block = _world.GetBlock(parentPos);
			return this.OnBlockActivated(_commandName, _world, _cIdx, parentPos, block, _player);
		}
		TileEntityLandClaim tileEntityLandClaim = _world.GetTileEntity(_cIdx, _blockPos) as TileEntityLandClaim;
		if (tileEntityLandClaim == null)
		{
			return false;
		}
		if (_commandName == "show_bounds" || _commandName == "hide_bounds")
		{
			tileEntityLandClaim.ShowBounds = !tileEntityLandClaim.ShowBounds;
			this.updateViewBounds(_world, _cIdx, _blockPos, _blockValue, tileEntityLandClaim.ShowBounds);
			return true;
		}
		if (!(_commandName == "remove"))
		{
			return false;
		}
		_world.SetBlockRPC(_blockPos, BlockValue.Air);
		return true;
	}

	public override bool OnBlockActivated(WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		if (_blockValue.ischild)
		{
			Vector3i parentPos = _blockValue.Block.multiBlockPos.GetParentPos(_blockPos, _blockValue);
			BlockValue block = _world.GetBlock(parentPos);
			return this.OnBlockActivated(_world, _cIdx, parentPos, block, _player);
		}
		TileEntityLandClaim tileEntityLandClaim = (TileEntityLandClaim)_world.GetTileEntity(_cIdx, _blockPos);
		if (tileEntityLandClaim == null)
		{
			return false;
		}
		_player.AimingGun = false;
		Vector3i blockPos = tileEntityLandClaim.ToWorldPos();
		_world.GetGameManager().TELockServer(_cIdx, blockPos, tileEntityLandClaim.entityId, _player.entityId, null);
		return true;
	}

	public override bool HasBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		TileEntityLandClaim tileEntityLandClaim = (TileEntityLandClaim)_world.GetTileEntity(_clrIdx, _blockPos);
		return tileEntityLandClaim != null && tileEntityLandClaim.IsOwner(PlatformManager.InternalLocalUserIdentifier) && BlockLandClaim.IsPrimary(_blockValue);
	}

	public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		TileEntityLandClaim tileEntityLandClaim = (TileEntityLandClaim)_world.GetTileEntity(_clrIdx, _blockPos);
		if (tileEntityLandClaim == null)
		{
			return BlockActivationCommand.Empty;
		}
		bool flag = tileEntityLandClaim.IsOwner(PlatformManager.InternalLocalUserIdentifier) && BlockLandClaim.IsPrimary(_blockValue);
		if (!flag)
		{
			return BlockActivationCommand.Empty;
		}
		this.cmds[0].enabled = (flag && !tileEntityLandClaim.ShowBounds);
		this.cmds[1].enabled = (flag && tileEntityLandClaim.ShowBounds);
		this.cmds[2].enabled = flag;
		return this.cmds;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool updateViewBounds(WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, bool _enableState)
	{
		TileEntityLandClaim tileEntityLandClaim = (TileEntityLandClaim)_world.GetTileEntity(_cIdx, _blockPos);
		if (tileEntityLandClaim.IsOwner(PlatformManager.InternalLocalUserIdentifier))
		{
			Transform boundsHelper = LandClaimBoundsHelper.GetBoundsHelper(_blockPos.ToVector3());
			if (boundsHelper != null)
			{
				tileEntityLandClaim.BoundsHelper = boundsHelper;
				boundsHelper.gameObject.SetActive(_enableState);
			}
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string activePrompt;

	[PublicizedFrom(EAccessModifier.Private)]
	public string inactivePrompt;

	[PublicizedFrom(EAccessModifier.Private)]
	public new BlockActivationCommand[] cmds = new BlockActivationCommand[]
	{
		new BlockActivationCommand("show_bounds", "frames", false, false),
		new BlockActivationCommand("hide_bounds", "frames", false, false),
		new BlockActivationCommand("remove", "x", false, false)
	};
}

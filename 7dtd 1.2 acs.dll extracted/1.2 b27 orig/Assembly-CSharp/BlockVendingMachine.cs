using System;
using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class BlockVendingMachine : Block
{
	public BlockVendingMachine()
	{
		this.HasTileEntity = true;
	}

	public override void Init()
	{
		base.Init();
		if (!base.Properties.Values.ContainsKey(BlockVendingMachine.PropTraderID))
		{
			throw new Exception("Block with name " + base.GetBlockName() + " doesnt have a trader ID.");
		}
		int.TryParse(base.Properties.Values[BlockVendingMachine.PropTraderID], out this.traderID);
	}

	public override void PlaceBlock(WorldBase _world, BlockPlacement.Result _result, EntityAlive _ea)
	{
		base.PlaceBlock(_world, _result, _ea);
		TileEntityVendingMachine tileEntityVendingMachine = _world.GetTileEntity(_result.clrIdx, _result.blockPos) as TileEntityVendingMachine;
		if (tileEntityVendingMachine != null && _ea != null && _ea.entityType == EntityType.Player && TraderInfo.traderInfoList[this.traderID].PlayerOwned)
		{
			tileEntityVendingMachine.SetOwner(PlatformManager.InternalLocalUserIdentifier);
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				GameManager.Instance.persistentPlayers.Players[PlatformManager.InternalLocalUserIdentifier].AddVendingMachinePosition(_result.blockPos);
				return;
			}
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackagePlayerVendingMachine>().Setup(PlatformManager.InternalLocalUserIdentifier, _result.blockPos, false), false);
		}
	}

	public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		TileEntityVendingMachine tileEntityVendingMachine = _world.GetTileEntity(_clrIdx, _blockPos) as TileEntityVendingMachine;
		PlayerActionsLocal playerInput = ((EntityPlayerLocal)_entityFocusing).playerInput;
		if (tileEntityVendingMachine != null)
		{
			string arg = playerInput.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null) + playerInput.PermanentActions.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null);
			string arg2 = _blockValue.Block.GetLocalizedBlockName();
			if ((tileEntityVendingMachine.IsRentable || tileEntityVendingMachine.TraderData.TraderInfo.PlayerOwned) && tileEntityVendingMachine.GetOwner() != null)
			{
				PersistentPlayerData playerData = GameManager.Instance.persistentPlayers.GetPlayerData(tileEntityVendingMachine.GetOwner());
				if (playerData != null)
				{
					GameServerInfo gameServerInfo = SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer ? SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo : SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo;
					if ((gameServerInfo != null && gameServerInfo.AllowsCrossplay) || playerData.PlayGroup != DeviceFlags.Current.ToPlayGroup())
					{
						string str = "[sp=" + PlatformManager.NativePlatform.Utils.GetCrossplayPlayerIcon(playerData.PlayGroup, true, playerData.PlatformData.NativeId.PlatformIdentifier) + "]";
						arg2 = string.Format(Localization.Get("xuiVendingWithOwner", false), GameUtils.SafeStringFormat(str + " " + playerData.PlayerName.DisplayName));
					}
					else
					{
						arg2 = string.Format(Localization.Get("xuiVendingWithOwner", false), GameUtils.SafeStringFormat(playerData.PlayerName.DisplayName));
					}
				}
				else
				{
					arg2 = string.Format(Localization.Get("xuiVendingWithOwner", false), Localization.Get("sleepingBagPlayerUnknown", false));
				}
			}
			return string.Format(Localization.Get("vendingMachineActivate", false), arg, arg2);
		}
		return "";
	}

	public override bool HasBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		return _world.GetTileEntity(_clrIdx, _blockPos) is TileEntityVendingMachine;
	}

	public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		TileEntityVendingMachine tileEntityVendingMachine = _world.GetTileEntity(_clrIdx, _blockPos) as TileEntityVendingMachine;
		if (tileEntityVendingMachine == null)
		{
			return BlockActivationCommand.Empty;
		}
		PlatformUserIdentifierAbs internalLocalUserIdentifier = PlatformManager.InternalLocalUserIdentifier;
		PersistentPlayerData playerData = _world.GetGameManager().GetPersistentPlayerList().GetPlayerData(tileEntityVendingMachine.GetOwner());
		bool flag = tileEntityVendingMachine.LocalPlayerIsOwner();
		if (!flag)
		{
			if (playerData != null && playerData.ACL != null)
			{
				playerData.ACL.Contains(internalLocalUserIdentifier);
			}
		}
		bool playerOwned = TraderInfo.traderInfoList[this.traderID].PlayerOwned;
		this.cmds[0].enabled = true;
		this.cmds[1].enabled = (playerOwned && flag && tileEntityVendingMachine.TraderData.PrimaryInventory.Count == 0);
		this.cmds[2].enabled = (playerOwned && ((!tileEntityVendingMachine.IsUserAllowed(internalLocalUserIdentifier) && tileEntityVendingMachine.HasPassword()) || flag));
		this.cmds[3].enabled = (!playerOwned && GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled));
		return this.cmds;
	}

	public override void OnBlockAdded(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockAdded(world, _chunk, _blockPos, _blockValue);
		if (_blockValue.ischild)
		{
			return;
		}
		if (!(world.GetTileEntity(_chunk.ClrIdx, _blockPos) is TileEntityVendingMachine))
		{
			_chunk.AddTileEntity(new TileEntityVendingMachine(_chunk)
			{
				localChunkPos = World.toBlock(_blockPos),
				TraderData = new TraderData(),
				TraderData = 
				{
					TraderID = this.traderID
				}
			});
		}
	}

	public override void OnBlockRemoved(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockRemoved(world, _chunk, _blockPos, _blockValue);
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			TileEntityVendingMachine tileEntityVendingMachine = world.GetTileEntity(_chunk.ClrIdx, _blockPos) as TileEntityVendingMachine;
			if (tileEntityVendingMachine != null)
			{
				PlatformUserIdentifierAbs owner = tileEntityVendingMachine.GetOwner();
				PersistentPlayerData persistentPlayerData;
				if (owner != null && GameManager.Instance.persistentPlayers.Players.TryGetValue(owner, out persistentPlayerData))
				{
					persistentPlayerData.TryRemoveVendingMachinePosition(_blockPos);
				}
			}
		}
		_chunk.RemoveTileEntityAt<TileEntityVendingMachine>((World)world, World.toBlock(_blockPos));
	}

	public override Block.DestroyedResult OnBlockDestroyedBy(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, int _entityId, bool _bUseHarvestTool)
	{
		TileEntityVendingMachine tileEntityVendingMachine = _world.GetTileEntity(_clrIdx, _blockPos) as TileEntityVendingMachine;
		return Block.DestroyedResult.Downgrade;
	}

	public override bool OnBlockActivated(string _commandName, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		if (_blockValue.ischild)
		{
			Vector3i parentPos = _blockValue.Block.multiBlockPos.GetParentPos(_blockPos, _blockValue);
			BlockValue block = _world.GetBlock(parentPos);
			return this.OnBlockActivated(_commandName, _world, _cIdx, parentPos, block, _player);
		}
		TileEntityVendingMachine tileEntityVendingMachine = _world.GetTileEntity(_cIdx, _blockPos) as TileEntityVendingMachine;
		if (tileEntityVendingMachine == null)
		{
			return false;
		}
		LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(_player);
		if (null != uiforPlayer)
		{
			if (_commandName == "trade")
			{
				return this.OnBlockActivated(_world, _cIdx, _blockPos, _blockValue, _player);
			}
			if (_commandName == "take")
			{
				ItemStack itemStack = new ItemStack(_blockValue.ToItemValue(), 1);
				if (uiforPlayer.xui.PlayerInventory.AddItem(itemStack))
				{
					_world.SetBlockRPC(_cIdx, _blockPos, BlockValue.Air);
				}
				return true;
			}
			if (_commandName == "keypad")
			{
				XUiC_KeypadWindow.Open(uiforPlayer, tileEntityVendingMachine);
				return true;
			}
			if (_commandName == "restock")
			{
				_player.PlayOneShot("ui_trader_inv_reset", false, false, false);
				tileEntityVendingMachine.TraderData.lastInventoryUpdate = 0UL;
				return true;
			}
		}
		return false;
	}

	public override bool OnBlockActivated(WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		if (_world.GetBlock(_blockPos.x, _blockPos.y - 1, _blockPos.z).Block.HasTag(BlockTags.Door))
		{
			_blockPos = new Vector3i(_blockPos.x, _blockPos.y - 1, _blockPos.z);
			return this.OnBlockActivated(_world, _cIdx, _blockPos, _blockValue, _player);
		}
		TileEntityVendingMachine tileEntityVendingMachine = _world.GetTileEntity(_cIdx, _blockPos) as TileEntityVendingMachine;
		if (tileEntityVendingMachine == null)
		{
			return false;
		}
		_player.AimingGun = false;
		Vector3i blockPos = tileEntityVendingMachine.ToWorldPos();
		_world.GetGameManager().TELockServer(_cIdx, blockPos, tileEntityVendingMachine.entityId, _player.entityId, null);
		return true;
	}

	public override int OnBlockDamaged(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, int _damagePoints, int _entityIdThatDamaged, ItemActionAttack.AttackHitInfo _attackHitInfo, bool _bUseHarvestTool, bool _bBypassMaxDamage, int _recDepth = 0)
	{
		if (_damagePoints > 0 && base.Properties.Values.ContainsKey("Buff"))
		{
			EntityAlive entityAlive = _world.GetEntity(_entityIdThatDamaged) as EntityAlive;
			if (entityAlive != null && entityAlive as EntityTurret == null)
			{
				bool flag = true;
				if (_attackHitInfo != null && _attackHitInfo.WeaponTypeTag.Equals(ItemActionAttack.ThrownTag))
				{
					flag = true;
				}
				else
				{
					ItemActionRanged itemActionRanged = entityAlive.inventory.holdingItemData.item.Actions[0] as ItemActionRanged;
					if (itemActionRanged == null || (itemActionRanged.Hitmask & 128) != 0)
					{
						flag = false;
					}
				}
				if (!flag)
				{
					string[] array = base.Properties.Values["Buff"].Split(',', StringSplitOptions.None);
					for (int i = 0; i < array.Length; i++)
					{
						entityAlive.Buffs.AddBuff(array[i].Trim(), _blockPos, entityAlive.entityId, true, false, -1f);
					}
				}
			}
		}
		return base.OnBlockDamaged(_world, _clrIdx, _blockPos, _blockValue, _damagePoints, _entityIdThatDamaged, _attackHitInfo, _bUseHarvestTool, _bBypassMaxDamage, _recDepth);
	}

	public override bool IsWaterBlocked(IBlockAccess _world, Vector3i _blockPos, BlockValue _blockValue, BlockFaceFlag _sides)
	{
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropTraderID = "TraderID";

	[PublicizedFrom(EAccessModifier.Protected)]
	public int traderID;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<int> buffActions;

	[PublicizedFrom(EAccessModifier.Private)]
	public new BlockActivationCommand[] cmds = new BlockActivationCommand[]
	{
		new BlockActivationCommand("trade", "vending", false, false),
		new BlockActivationCommand("take", "hand", false, false),
		new BlockActivationCommand("keypad", "keypad", false, false),
		new BlockActivationCommand("restock", "coin", false, false)
	};
}

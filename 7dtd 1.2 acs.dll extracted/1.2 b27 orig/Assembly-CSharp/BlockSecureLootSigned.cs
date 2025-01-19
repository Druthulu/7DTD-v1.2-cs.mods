using System;
using System.Globalization;
using Audio;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockSecureLootSigned : BlockSecureLoot
{
	public override void Init()
	{
		base.Init();
		if (!base.Properties.Values.ContainsKey(BlockSecureLoot.PropLootList))
		{
			throw new Exception("Block with name " + base.GetBlockName() + " doesnt have a loot list");
		}
		this.lootList = base.Properties.Values[BlockSecureLoot.PropLootList];
		if (base.Properties.Values.ContainsKey(BlockSecureLoot.PropLockPickTime))
		{
			this.lockPickTime = StringParsers.ParseFloat(base.Properties.Values[BlockSecureLoot.PropLockPickTime], 0, -1, NumberStyles.Any);
		}
		else
		{
			this.lockPickTime = 15f;
		}
		if (base.Properties.Values.ContainsKey(BlockSecureLoot.PropLockPickItem))
		{
			this.lockPickItem = base.Properties.Values[BlockSecureLoot.PropLockPickItem];
		}
		if (base.Properties.Values.ContainsKey(BlockSecureLoot.PropLockPickBreakChance))
		{
			this.lockPickBreakChance = StringParsers.ParseFloat(base.Properties.Values[BlockSecureLoot.PropLockPickBreakChance], 0, -1, NumberStyles.Any);
		}
		else
		{
			this.lockPickBreakChance = 0f;
		}
		if (base.Properties.Values.ContainsKey("LineWidth"))
		{
			this.characterWidth = int.Parse(base.Properties.Values["LineWidth"]);
		}
		if (base.Properties.Values.ContainsKey("LineCount"))
		{
			this.lineCount = int.Parse(base.Properties.Values["LineCount"]);
		}
	}

	public override void PlaceBlock(WorldBase _world, BlockPlacement.Result _result, EntityAlive _ea)
	{
		base.PlaceBlock(_world, _result, _ea);
		TileEntitySecureLootContainerSigned tileEntitySecureLootContainerSigned = _world.GetTileEntity(_result.clrIdx, _result.blockPos) as TileEntitySecureLootContainerSigned;
		if (tileEntitySecureLootContainerSigned != null)
		{
			tileEntitySecureLootContainerSigned.SetEmpty();
			if (_ea != null && _ea.entityType == EntityType.Player)
			{
				tileEntitySecureLootContainerSigned.bPlayerStorage = true;
				tileEntitySecureLootContainerSigned.SetOwner(PlatformManager.InternalLocalUserIdentifier);
			}
		}
	}

	public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		TileEntitySecureLootContainerSigned tileEntitySecureLootContainerSigned = _world.GetTileEntity(_clrIdx, _blockPos) as TileEntitySecureLootContainerSigned;
		PlayerActionsLocal playerInput = ((EntityPlayerLocal)_entityFocusing).playerInput;
		string arg = playerInput.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null) + playerInput.PermanentActions.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null);
		if (tileEntitySecureLootContainerSigned == null)
		{
			return "";
		}
		string localizedBlockName = _blockValue.Block.GetLocalizedBlockName();
		if (!tileEntitySecureLootContainerSigned.IsLocked())
		{
			return string.Format(Localization.Get("tooltipUnlocked", false), arg, localizedBlockName);
		}
		if (this.lockPickItem == null && !tileEntitySecureLootContainerSigned.LocalPlayerIsOwner())
		{
			return string.Format(Localization.Get("tooltipJammed", false), arg, localizedBlockName);
		}
		return string.Format(Localization.Get("tooltipLocked", false), arg, localizedBlockName);
	}

	public override bool HasBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		return _world.GetTileEntity(_clrIdx, _blockPos) is TileEntitySecureLootContainerSigned;
	}

	public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		TileEntitySecureLootContainerSigned tileEntitySecureLootContainerSigned = _world.GetTileEntity(_clrIdx, _blockPos) as TileEntitySecureLootContainerSigned;
		if (tileEntitySecureLootContainerSigned == null)
		{
			return BlockActivationCommand.Empty;
		}
		PlatformUserIdentifierAbs internalLocalUserIdentifier = PlatformManager.InternalLocalUserIdentifier;
		PersistentPlayerData playerData = _world.GetGameManager().GetPersistentPlayerList().GetPlayerData(tileEntitySecureLootContainerSigned.GetOwner());
		bool flag = tileEntitySecureLootContainerSigned.LocalPlayerIsOwner();
		bool flag2 = !flag && playerData != null && playerData.ACL != null && playerData.ACL.Contains(internalLocalUserIdentifier);
		this.cmds[0].enabled = true;
		this.cmds[1].enabled = (!tileEntitySecureLootContainerSigned.IsLocked() && (flag || flag2));
		this.cmds[2].enabled = (tileEntitySecureLootContainerSigned.IsLocked() && flag);
		this.cmds[3].enabled = ((!tileEntitySecureLootContainerSigned.IsUserAllowed(internalLocalUserIdentifier) && tileEntitySecureLootContainerSigned.HasPassword() && tileEntitySecureLootContainerSigned.IsLocked()) || flag);
		this.cmds[4].enabled = (this.lockPickItem != null && tileEntitySecureLootContainerSigned.IsLocked() && !flag);
		this.cmds[5].enabled = true;
		bool flag3 = PlatformManager.MultiPlatform.PlayerReporting != null && !string.IsNullOrEmpty(tileEntitySecureLootContainerSigned.GetAuthoredText().Text) && !internalLocalUserIdentifier.Equals(tileEntitySecureLootContainerSigned.GetAuthoredText().Author);
		PersistentPlayerData playerData2 = GameManager.Instance.persistentPlayers.GetPlayerData(tileEntitySecureLootContainerSigned.GetAuthoredText().Author);
		bool flag4 = playerData2 != null && playerData2.PlatformData.Blocked[EBlockType.TextChat].IsBlocked();
		this.cmds[6].enabled = (flag3 && !flag4);
		return this.cmds;
	}

	public override void OnBlockAdded(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockAdded(world, _chunk, _blockPos, _blockValue);
		if (_blockValue.ischild)
		{
			return;
		}
		if (!(world.GetTileEntity(_chunk.ClrIdx, _blockPos) is TileEntitySecureLootContainerSigned))
		{
			TileEntitySecureLootContainerSigned tileEntitySecureLootContainerSigned = new TileEntitySecureLootContainerSigned(_chunk);
			tileEntitySecureLootContainerSigned.localChunkPos = World.toBlock(_blockPos);
			tileEntitySecureLootContainerSigned.lootListName = this.lootList;
			tileEntitySecureLootContainerSigned.SetContainerSize(LootContainer.GetLootContainer(this.lootList, true).size, true);
			_chunk.AddTileEntity(tileEntitySecureLootContainerSigned);
		}
	}

	public override void OnBlockEntityTransformAfterActivated(WorldBase _world, Vector3i _blockPos, int _cIdx, BlockValue _blockValue, BlockEntityData _ebcd)
	{
		if (_ebcd == null)
		{
			return;
		}
		Chunk chunk = (Chunk)((World)_world).GetChunkFromWorldPos(_blockPos);
		TileEntitySecureLootContainerSigned tileEntitySecureLootContainerSigned = (TileEntitySecureLootContainerSigned)_world.GetTileEntity(_cIdx, _blockPos);
		if (tileEntitySecureLootContainerSigned == null)
		{
			tileEntitySecureLootContainerSigned = new TileEntitySecureLootContainerSigned(chunk);
			tileEntitySecureLootContainerSigned.localChunkPos = World.toBlock(_blockPos);
			tileEntitySecureLootContainerSigned.lootListName = this.lootList;
			tileEntitySecureLootContainerSigned.SetContainerSize(LootContainer.GetLootContainer(this.lootList, true).size, true);
			chunk.AddTileEntity(tileEntitySecureLootContainerSigned);
		}
		tileEntitySecureLootContainerSigned.SetBlockEntityData(_ebcd);
		base.OnBlockEntityTransformAfterActivated(_world, _blockPos, _cIdx, _blockValue, _ebcd);
	}

	public override void OnBlockRemoved(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockRemoved(world, _chunk, _blockPos, _blockValue);
		_chunk.RemoveTileEntityAt<TileEntitySecureLootContainerSigned>((World)world, World.toBlock(_blockPos));
	}

	public override Block.DestroyedResult OnBlockDestroyedBy(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, int _entityId, bool _bUseHarvestTool)
	{
		TileEntitySecureLootContainerSigned tileEntitySecureLootContainerSigned = _world.GetTileEntity(_clrIdx, _blockPos) as TileEntitySecureLootContainerSigned;
		if (tileEntitySecureLootContainerSigned != null)
		{
			tileEntitySecureLootContainerSigned.OnDestroy();
		}
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
		TileEntitySecureLootContainerSigned te = _world.GetTileEntity(_cIdx, _blockPos) as TileEntitySecureLootContainerSigned;
		if (te == null)
		{
			return false;
		}
		uint num = <PrivateImplementationDetails>.ComputeStringHash(_commandName);
		if (num <= 1541920145U)
		{
			if (num != 431699179U)
			{
				if (num != 1449533269U)
				{
					if (num == 1541920145U)
					{
						if (_commandName == "edit")
						{
							if (GameManager.Instance.IsEditMode() || !te.IsLocked() || te.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
							{
								return this.OnBlockActivated(_world, _cIdx, _blockPos, _blockValue, _player, "sign");
							}
							Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, "Misc/locked");
							return false;
						}
					}
				}
				else if (_commandName == "unlock")
				{
					te.SetLocked(false);
					Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, "Misc/unlocking");
					GameManager.ShowTooltip(_player, "containerUnlocked", false);
					return true;
				}
			}
			else if (_commandName == "report")
			{
				GeneratedTextManager.GetDisplayText(te.GetAuthoredText(), delegate(string _filtered)
				{
					ThreadManager.AddSingleTaskMainThread("OpenReportWindow", delegate(object _)
					{
						PersistentPlayerData playerData = GameManager.Instance.persistentPlayers.GetPlayerData(te.GetAuthoredText().Author);
						XUiC_ReportPlayer.Open((playerData != null) ? playerData.PlayerData : null, EnumReportCategory.VerbalAbuse, string.Format(Localization.Get("xuiReportOffensiveTextMessage", false), _filtered), "");
					}, null);
				}, true, false, GeneratedTextManager.TextFilteringMode.Filter, GeneratedTextManager.BbCodeSupportMode.SupportedAndAddEscapes);
				return true;
			}
		}
		else if (num <= 3445998097U)
		{
			if (num != 3326517961U)
			{
				if (num == 3445998097U)
				{
					if (_commandName == "keypad")
					{
						LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(_player);
						if (uiforPlayer != null)
						{
							XUiC_KeypadWindow.Open(uiforPlayer, te);
						}
						return true;
					}
				}
			}
			else if (_commandName == "Search")
			{
				if (!te.IsLocked() || te.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
				{
					return this.OnBlockActivated(_world, _cIdx, _blockPos, _blockValue, _player, null);
				}
				Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, "Misc/locked");
				return false;
			}
		}
		else if (num != 4010637378U)
		{
			if (num == 4198624760U)
			{
				if (_commandName == "pick")
				{
					LocalPlayerUI playerUI = _player.PlayerUI;
					ItemValue item = ItemClass.GetItem(this.lockPickItem, false);
					if (playerUI.xui.PlayerInventory.GetItemCount(item) == 0)
					{
						playerUI.xui.CollectedItemList.AddItemStack(new ItemStack(item, 0), true);
						GameManager.ShowTooltip(_player, Localization.Get("ttLockpickMissing", false), false);
						return true;
					}
					_player.AimingGun = false;
					Vector3i blockPos = te.ToWorldPos();
					te.bWasTouched = te.bTouched;
					_world.GetGameManager().TELockServer(_cIdx, blockPos, te.entityId, _player.entityId, "lockpick");
					return true;
				}
			}
		}
		else if (_commandName == "lock")
		{
			te.SetLocked(true);
			Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, "Misc/locking");
			GameManager.ShowTooltip(_player, "containerLocked", false);
			return true;
		}
		return false;
	}

	public bool OnBlockActivated(WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _player, string _customUi = null)
	{
		if (_blockValue.ischild)
		{
			Vector3i parentPos = _blockValue.Block.multiBlockPos.GetParentPos(_blockPos, _blockValue);
			BlockValue block = _world.GetBlock(parentPos);
			return this.OnBlockActivated(_world, _cIdx, parentPos, block, _player, _customUi);
		}
		TileEntitySecureLootContainerSigned tileEntitySecureLootContainerSigned = _world.GetTileEntity(_cIdx, _blockPos) as TileEntitySecureLootContainerSigned;
		if (tileEntitySecureLootContainerSigned == null)
		{
			return false;
		}
		_player.AimingGun = false;
		Vector3i blockPos = tileEntitySecureLootContainerSigned.ToWorldPos();
		tileEntitySecureLootContainerSigned.bWasTouched = tileEntitySecureLootContainerSigned.bTouched;
		_world.GetGameManager().TELockServer(_cIdx, blockPos, tileEntitySecureLootContainerSigned.entityId, _player.entityId, _customUi);
		return true;
	}

	public override bool IsWaterBlocked(IBlockAccess _world, Vector3i _blockPos, BlockValue _blockValue, BlockFaceFlag _sides)
	{
		return true;
	}

	public override bool IsTileEntitySavedInPrefab()
	{
		return base.IsTileEntitySavedInPrefab();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int characterWidth;

	[PublicizedFrom(EAccessModifier.Private)]
	public int lineCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public new BlockActivationCommand[] cmds = new BlockActivationCommand[]
	{
		new BlockActivationCommand("Search", "search", false, false),
		new BlockActivationCommand("lock", "lock", false, false),
		new BlockActivationCommand("unlock", "unlock", false, false),
		new BlockActivationCommand("keypad", "keypad", false, false),
		new BlockActivationCommand("pick", "unlock", false, false),
		new BlockActivationCommand("edit", "pen", false, false),
		new BlockActivationCommand("report", "report", false, false)
	};
}

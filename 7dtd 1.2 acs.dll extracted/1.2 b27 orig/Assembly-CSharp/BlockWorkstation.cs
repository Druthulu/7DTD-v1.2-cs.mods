using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockWorkstation : BlockParticle
{
	public BlockWorkstation()
	{
		this.HasTileEntity = true;
	}

	public override void Init()
	{
		base.Init();
		this.TakeDelay = 2f;
		base.Properties.ParseFloat("TakeDelay", ref this.TakeDelay);
		string text = "1,2,3";
		base.Properties.ParseString("Workstation.ToolNames", ref text);
		this.toolTransformNames = text.Split(',', StringSplitOptions.None);
		this.WorkstationData = new WorkstationData(base.GetBlockName(), base.Properties);
		CraftingManager.AddWorkstationData(this.WorkstationData);
	}

	public override void OnBlockAdded(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockAdded(world, _chunk, _blockPos, _blockValue);
		if (_blockValue.ischild)
		{
			return;
		}
		_chunk.AddTileEntity(new TileEntityWorkstation(_chunk)
		{
			localChunkPos = World.toBlock(_blockPos)
		});
	}

	public override void OnBlockRemoved(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockRemoved(world, _chunk, _blockPos, _blockValue);
		_chunk.RemoveTileEntityAt<TileEntityWorkstation>((World)world, World.toBlock(_blockPos));
	}

	public override void PlaceBlock(WorldBase _world, BlockPlacement.Result _result, EntityAlive _ea)
	{
		base.PlaceBlock(_world, _result, _ea);
		TileEntityWorkstation tileEntityWorkstation = (TileEntityWorkstation)_world.GetTileEntity(_result.blockPos);
		if (tileEntityWorkstation != null)
		{
			tileEntityWorkstation.IsPlayerPlaced = true;
		}
	}

	public override bool OnBlockActivated(WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		TileEntityWorkstation tileEntityWorkstation = (TileEntityWorkstation)_world.GetTileEntity(_blockPos);
		if (tileEntityWorkstation == null)
		{
			return false;
		}
		_player.AimingGun = false;
		Vector3i blockPos = tileEntityWorkstation.ToWorldPos();
		_world.GetGameManager().TELockServer(_cIdx, blockPos, tileEntityWorkstation.entityId, _player.entityId, null);
		return true;
	}

	public override void OnBlockValueChanged(WorldBase _world, Chunk _chunk, int _clrIdx, Vector3i _blockPos, BlockValue _oldBlockValue, BlockValue _newBlockValue)
	{
		base.OnBlockValueChanged(_world, _chunk, _clrIdx, _blockPos, _oldBlockValue, _newBlockValue);
		this.checkParticles(_world, _clrIdx, _blockPos, _newBlockValue);
	}

	public override byte GetLightValue(BlockValue _blockValue)
	{
		return 0;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void checkParticles(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
	{
		if (_blockValue.ischild)
		{
			return;
		}
		bool flag = GameManager.Instance.HasBlockParticleEffect(_blockPos);
		if (_blockValue.meta != 0 && !flag)
		{
			this.addParticles(_world, _clrIdx, _blockPos.x, _blockPos.y, _blockPos.z, _blockValue);
			if (this.CraftingParticleLightIntensity > 0f)
			{
				this.UpdateVisible(_world, _blockPos);
				return;
			}
		}
		else if (_blockValue.meta == 0 && flag)
		{
			this.removeParticles(_world, _blockPos.x, _blockPos.y, _blockPos.z, _blockValue);
		}
	}

	public static bool IsLit(BlockValue _blockValue)
	{
		return _blockValue.meta > 0;
	}

	public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		return Localization.Get("useWorkstation", false);
	}

	public override bool OnBlockActivated(string _commandName, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		if (_commandName == "open")
		{
			return this.OnBlockActivated(_world, _cIdx, _blockPos, _blockValue, _player);
		}
		if (!(_commandName == "take"))
		{
			return false;
		}
		this.TakeItemWithTimer(_cIdx, _blockPos, _blockValue, _player);
		return true;
	}

	public override bool HasBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		return true;
	}

	public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		bool flag = _world.IsMyLandProtectedBlock(_blockPos, _world.GetGameManager().GetPersistentLocalPlayer(), false);
		TileEntityWorkstation tileEntityWorkstation = (TileEntityWorkstation)_world.GetTileEntity(_blockPos);
		bool flag2 = false;
		if (tileEntityWorkstation != null)
		{
			flag2 = tileEntityWorkstation.IsPlayerPlaced;
		}
		this.cmds[1].enabled = (flag && flag2 && this.TakeDelay > 0f);
		return this.cmds;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TakeItemWithTimer(int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _player)
	{
		if (_blockValue.damage > 0)
		{
			GameManager.ShowTooltip(_player as EntityPlayerLocal, Localization.Get("ttRepairBeforePickup", false), string.Empty, "ui_denied", null, false);
			return;
		}
		if (!(GameManager.Instance.World.GetTileEntity(_blockPos) as TileEntityWorkstation).IsEmpty)
		{
			GameManager.ShowTooltip(_player as EntityPlayerLocal, Localization.Get("ttWorkstationNotEmpty", false), string.Empty, "ui_denied", null, false);
			return;
		}
		LocalPlayerUI playerUI = (_player as EntityPlayerLocal).PlayerUI;
		playerUI.windowManager.Open("timer", true, false, true);
		XUiC_Timer childByType = playerUI.xui.GetChildByType<XUiC_Timer>();
		TimerEventData timerEventData = new TimerEventData();
		timerEventData.Data = new object[]
		{
			_cIdx,
			_blockValue,
			_blockPos,
			_player
		};
		timerEventData.Event += this.EventData_Event;
		childByType.SetTimer(this.TakeDelay, timerEventData, -1f, "");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EventData_Event(TimerEventData timerData)
	{
		World world = GameManager.Instance.World;
		object[] array = (object[])timerData.Data;
		int clrIdx = (int)array[0];
		BlockValue blockValue = (BlockValue)array[1];
		Vector3i vector3i = (Vector3i)array[2];
		BlockValue block = world.GetBlock(vector3i);
		EntityPlayerLocal entityPlayerLocal = array[3] as EntityPlayerLocal;
		if (block.damage > 0)
		{
			GameManager.ShowTooltip(entityPlayerLocal, Localization.Get("ttRepairBeforePickup", false), string.Empty, "ui_denied", null, false);
			return;
		}
		if (block.type != blockValue.type)
		{
			GameManager.ShowTooltip(entityPlayerLocal, Localization.Get("ttBlockMissingPickup", false), string.Empty, "ui_denied", null, false);
			return;
		}
		if ((world.GetTileEntity(vector3i) as TileEntityWorkstation).IsUserAccessing())
		{
			GameManager.ShowTooltip(entityPlayerLocal, Localization.Get("ttCantPickupInUse", false), string.Empty, "ui_denied", null, false);
			return;
		}
		LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(entityPlayerLocal);
		ItemStack itemStack = new ItemStack(block.ToItemValue(), 1);
		if (!uiforPlayer.xui.PlayerInventory.AddItem(itemStack))
		{
			uiforPlayer.xui.PlayerInventory.DropItem(itemStack);
		}
		world.SetBlockRPC(clrIdx, vector3i, BlockValue.Air);
	}

	public override void OnBlockEntityTransformBeforeActivated(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, BlockEntityData _ebcd)
	{
		base.OnBlockEntityTransformBeforeActivated(_world, _blockPos, _blockValue, _ebcd);
		this.UpdateVisible(_world, _blockPos);
	}

	public void UpdateVisible(WorldBase _world, Vector3i _blockPos)
	{
		TileEntityWorkstation tileEntityWorkstation = _world.GetTileEntity(_blockPos) as TileEntityWorkstation;
		if (tileEntityWorkstation != null)
		{
			this.UpdateVisible(tileEntityWorkstation);
		}
	}

	public void UpdateVisible(TileEntityWorkstation _te)
	{
		BlockEntityData blockEntity = _te.GetChunk().GetBlockEntity(_te.ToWorldPos());
		if (blockEntity == null)
		{
			return;
		}
		Transform transform = blockEntity.transform;
		if (transform)
		{
			ItemStack[] tools = _te.Tools;
			int num = Utils.FastMin(tools.Length, this.toolTransformNames.Length);
			for (int i = 0; i < num; i++)
			{
				Transform transform2 = transform.Find(this.toolTransformNames[i]);
				if (transform2)
				{
					transform2.gameObject.SetActive(!tools[i].IsEmpty());
				}
			}
			Transform transform3 = transform.Find("craft");
			if (transform3)
			{
				bool isCrafting = _te.IsCrafting;
				transform3.gameObject.SetActive(isCrafting);
				if (this.CraftingParticleLightIntensity > 0f)
				{
					Transform blockParticleEffect = GameManager.Instance.GetBlockParticleEffect(_te.ToWorldPos());
					if (blockParticleEffect)
					{
						Light componentInChildren = blockParticleEffect.GetComponentInChildren<Light>();
						if (componentInChildren)
						{
							componentInChildren.intensity = (isCrafting ? this.CraftingParticleLightIntensity : 1f);
							return;
						}
					}
					else if (isCrafting)
					{
						_te.SetVisibleChanged();
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public float TakeDelay;

	public WorkstationData WorkstationData;

	[PublicizedFrom(EAccessModifier.Private)]
	public string[] toolTransformNames;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float CraftingParticleLightIntensity;

	[PublicizedFrom(EAccessModifier.Protected)]
	public new BlockActivationCommand[] cmds = new BlockActivationCommand[]
	{
		new BlockActivationCommand("open", "campfire", true, false),
		new BlockActivationCommand("take", "hand", false, false)
	};
}

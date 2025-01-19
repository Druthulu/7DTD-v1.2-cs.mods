using System;
using System.Collections;
using System.Collections.Generic;
using Audio;
using Platform;
using UnityEngine;

public class BlockToolSelection : ISelectionBoxCallback, IBlockTool
{
	public SelectionBox SelectionBox
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			SelectionBox selectionBox = SelectionBoxManager.Instance.GetCategory("Selection").GetBox("SingleInstance");
			if (selectionBox != null)
			{
				return selectionBox;
			}
			selectionBox = SelectionBoxManager.Instance.GetCategory("Selection").AddBox("SingleInstance", Vector3i.zero, Vector3i.one, false, false);
			selectionBox.SetVisible(false);
			selectionBox.SetSizeVisibility(true);
			return selectionBox;
		}
	}

	public BlockToolSelection()
	{
		BlockToolSelection.Instance = this;
		SelectionBoxManager.Instance.GetCategory("Selection").SetCallback(this);
		PlayerActionsLocal primaryPlayer = PlatformManager.NativePlatform.Input.PrimaryPlayer;
		NGuiAction nguiAction = new NGuiAction(Localization.Get("selectionToolsEditBlocksVolume", false), null, true);
		nguiAction.SetClickActionDelegate(delegate
		{
			GameManager.bVolumeBlocksEditing = !GameManager.bVolumeBlocksEditing;
		});
		nguiAction.SetIsCheckedDelegate(() => GameManager.bVolumeBlocksEditing);
		nguiAction.SetIsVisibleDelegate(() => GameManager.Instance.IsEditMode());
		NGuiAction nguiAction2 = new NGuiAction(Localization.Get("selectionToolsCopySleeperVolume", false), null);
		nguiAction2.SetClickActionDelegate(delegate
		{
			if (XUiC_WoPropsSleeperVolume.selectedPrefabInstance != null && XUiC_WoPropsSleeperVolume.selectedVolumeIndex >= 0)
			{
				int selectedVolumeIndex = XUiC_WoPropsSleeperVolume.selectedVolumeIndex;
				PrefabInstance selectedPrefabInstance = XUiC_WoPropsSleeperVolume.selectedPrefabInstance;
				XUiC_WoPropsSleeperVolume.selectedPrefabInstance.prefab.CloneSleeperVolume(selectedPrefabInstance.name, selectedPrefabInstance.boundingBoxPosition, selectedVolumeIndex);
			}
		});
		nguiAction2.SetIsVisibleDelegate(() => GameManager.Instance.IsEditMode());
		nguiAction2.SetTooltip("selectionToolsCopySleeperVolumeTip");
		NGuiAction nguiAction3 = new NGuiAction(Localization.Get("selectionToolsCopyAirBlocks", false), null, true);
		nguiAction3.SetClickActionDelegate(delegate
		{
			this.copyPasteAirBlocks = !this.copyPasteAirBlocks;
		});
		nguiAction3.SetIsCheckedDelegate(() => this.copyPasteAirBlocks);
		nguiAction3.SetIsVisibleDelegate(new NGuiAction.IsVisibleDelegate(GameManager.Instance.IsEditMode));
		NGuiAction nguiAction4 = new NGuiAction(Localization.Get("selectionToolsClearSelection", false), primaryPlayer.SelectionClear);
		nguiAction4.SetClickActionDelegate(delegate
		{
			if (this.SelectionLockMode == 2)
			{
				this.SelectionLockMode = 0;
				this.SelectionActive = false;
				return;
			}
			this.BeginUndo(0);
			BlockTools.CubeRPC(GameManager.Instance, this.SelectionClrIdx, this.SelectionStart, this.SelectionEnd, BlockValue.Air, MarchingCubes.DensityAir, 0, 0L);
			BlockTools.CubeWaterRPC(GameManager.Instance, this.SelectionStart, this.SelectionEnd, WaterValue.Empty);
			this.EndUndo(0, false);
		});
		nguiAction4.SetIsEnabledDelegate(() => GameManager.Instance.IsEditMode() && this.SelectionActive);
		nguiAction4.SetIsVisibleDelegate(() => GameManager.Instance.IsEditMode());
		nguiAction4.SetTooltip("selectionToolsClearSelectionTip");
		NGuiAction nguiAction5 = new NGuiAction(Localization.Get("selectionToolsFillSelection", false), primaryPlayer.SelectionFill);
		nguiAction5.SetClickActionDelegate(delegate
		{
			this.BeginUndo(0);
			EntityPlayerLocal primaryPlayer2 = GameManager.Instance.World.GetPrimaryPlayer();
			ItemValue holdingItemItemValue = primaryPlayer2.inventory.holdingItemItemValue;
			BlockValue blockValue = holdingItemItemValue.ToBlockValue();
			if (blockValue.isair)
			{
				return;
			}
			Block block = blockValue.Block;
			BlockPlacement.Result result = new BlockPlacement.Result(0, Vector3.zero, Vector3i.zero, blockValue, -1f);
			block.OnBlockPlaceBefore(GameManager.Instance.World, ref result, primaryPlayer2, GameManager.Instance.World.GetGameRandom());
			blockValue = result.blockValue;
			blockValue.rotation = ((primaryPlayer2.inventory.holdingItemData is ItemClassBlock.ItemBlockInventoryData) ? ((ItemClassBlock.ItemBlockInventoryData)primaryPlayer2.inventory.holdingItemData).rotation : blockValue.rotation);
			BlockTools.CubeRPC(GameManager.Instance, this.SelectionClrIdx, this.m_selectionStartPoint, this.m_SelectionEndPoint, blockValue, blockValue.Block.shape.IsTerrain() ? MarchingCubes.DensityTerrain : MarchingCubes.DensityAir, 0, holdingItemItemValue.Texture);
			this.EndUndo(0, false);
		});
		nguiAction5.SetIsEnabledDelegate(() => GameManager.Instance.IsEditMode() && this.SelectionActive);
		nguiAction5.SetIsVisibleDelegate(() => GameManager.Instance.IsEditMode());
		nguiAction5.SetTooltip("selectionToolsFillSelectionTip");
		NGuiAction nguiAction6 = new NGuiAction(Localization.Get("selectionToolsRandomFillSelection", false), null);
		nguiAction6.SetClickActionDelegate(delegate
		{
			this.BeginUndo(0);
			BlockTools.CubeRandomRPC(GameManager.Instance, this.SelectionClrIdx, this.m_selectionStartPoint, this.m_SelectionEndPoint, GameManager.Instance.World.GetPrimaryPlayer().inventory.holdingItemItemValue.ToBlockValue(), 0.1f, new EBlockRotationClasses?(EBlockRotationClasses.Basic90));
			this.EndUndo(0, false);
		});
		nguiAction6.SetIsEnabledDelegate(() => this.SelectionActive);
		nguiAction6.SetIsVisibleDelegate(() => GameManager.Instance.IsEditMode());
		nguiAction6.SetTooltip("selectionToolsRandomFillSelectionTip");
		NGuiAction nguiAction7 = new NGuiAction(Localization.Get("selectionToolsUndo", false), null);
		nguiAction7.SetClickActionDelegate(delegate
		{
			this.blockUndo();
		});
		nguiAction7.SetIsEnabledDelegate(() => this.undoQueue.Count > 0);
		nguiAction7.SetIsVisibleDelegate(() => GameManager.Instance.IsEditMode());
		nguiAction7.SetTooltip("selectionToolsUndoTip");
		NGuiAction nguiAction8 = new NGuiAction(Localization.Get("selectionToolsRedo", false), null);
		nguiAction8.SetClickActionDelegate(delegate
		{
			this.blockRedo();
		});
		nguiAction8.SetIsEnabledDelegate(() => this.redoQueue.Count > 0);
		nguiAction8.SetIsVisibleDelegate(() => GameManager.Instance.IsEditMode());
		nguiAction8.SetTooltip("selectionToolsRedoTip");
		this.actions = new List<NGuiAction>
		{
			nguiAction,
			nguiAction2,
			nguiAction3,
			NGuiAction.Separator,
			nguiAction4,
			nguiAction5,
			nguiAction6,
			NGuiAction.Separator,
			nguiAction7,
			nguiAction8
		};
		foreach (NGuiAction action in this.actions)
		{
			LocalPlayerUI.primaryUI.windowManager.AddGlobalAction(action);
		}
		Origin.OriginChanged = (Action<Vector3>)Delegate.Combine(Origin.OriginChanged, new Action<Vector3>(this.OnOriginChanged));
	}

	public void CheckSpecialKeys(Event ev, PlayerActionsLocal playerActions)
	{
		if (this.hitInfo == null)
		{
			return;
		}
		Vector3i vector3i = (GameManager.Instance.IsEditMode() && playerActions.Run.IsPressed) ? this.hitInfo.hit.blockPos : this.hitInfo.lastBlockPos;
		bool flag = InputUtils.IsMac ? ((ev.modifiers & EventModifiers.Command) > EventModifiers.None) : ((ev.modifiers & EventModifiers.Control) > EventModifiers.None);
		bool flag2 = (ev.modifiers & EventModifiers.Shift) > EventModifiers.None;
		KeyCode keyCode = ev.keyCode;
		if (keyCode != KeyCode.C)
		{
			if (keyCode != KeyCode.V)
			{
				if (keyCode != KeyCode.Z)
				{
					return;
				}
				if (flag)
				{
					this.blockUndo();
				}
			}
			else if (flag)
			{
				if (!flag2 && this.SelectionLockMode != 2)
				{
					if (this.SelectionActive && this.clipboard.size.Equals(Vector3i.one) && !this.SelectionSize.Equals(this.clipboard.size))
					{
						this.BeginUndo(0);
						BlockValue block = this.clipboard.GetBlock(0, 0, 0);
						WaterValue water = this.clipboard.GetWater(0, 0, 0);
						long texture = this.clipboard.GetTexture(0, 0, 0);
						BlockTools.CubeRPC(GameManager.Instance, this.SelectionClrIdx, this.m_selectionStartPoint, this.m_SelectionEndPoint, block, block.Block.shape.IsTerrain() ? MarchingCubes.DensityTerrain : MarchingCubes.DensityAir, 0, texture);
						BlockTools.CubeWaterRPC(GameManager.Instance, this.m_selectionStartPoint, this.m_SelectionEndPoint, water);
						this.EndUndo(0, false);
						return;
					}
					if (this.SelectionActive && !this.SelectionSize.Equals(this.clipboard.size))
					{
						this.SelectionEnd = this.SelectionStart + this.clipboard.size - Vector3i.one;
						return;
					}
					if (!this.SelectionActive)
					{
						this.SelectionStart = vector3i;
						this.SelectionEnd = this.SelectionStart + this.clipboard.size - Vector3i.one;
						this.SelectionActive = true;
						return;
					}
					if (this.SelectionActive && this.SelectionSize.Equals(this.clipboard.size))
					{
						this.blockPaste(0, this.SelectionMin, this.clipboard);
						return;
					}
				}
				else
				{
					if (this.SelectionLockMode != 2)
					{
						if (this.SelectionSize != this.clipboard.size)
						{
							this.SelectionEnd = this.SelectionStart + this.clipboard.size - Vector3i.one;
						}
						this.SelectionActive = true;
						this.SelectionLockMode = 2;
						this.createBlockPreviewFrom(this.clipboard);
						return;
					}
					this.SelectionLockMode = 0;
					this.blockPaste(0, this.SelectionMin, this.clipboard);
					return;
				}
			}
		}
		else if (flag)
		{
			if (!this.SelectionActive)
			{
				this.SelectionStart = vector3i;
				this.SelectionEnd = vector3i;
			}
			this.blockCopy(this.clipboard);
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void rotatePreviewAroundY()
	{
		if (this.previewGORot2 == null)
		{
			return;
		}
		this.previewGORot2.transform.localRotation = Quaternion.AngleAxis(90f, Vector3.up) * this.previewGORot2.transform.localRotation;
		this.clipboard.RotateY(false, 1);
		Vector3 b = this.previewGORot2.transform.localRotation * this.offsetToMin;
		Vector3 vector = this.selectionRotCenter + b;
		Vector3 b2 = this.previewGORot2.transform.localRotation * this.offsetToMax;
		Vector3 vector2 = this.selectionRotCenter + b2;
		Vector3i vector3i = new Vector3i(Utils.Fastfloor(Utils.FastMin(vector.x, vector2.x)), Utils.Fastfloor(Utils.FastMin(vector.y, vector2.y)), Utils.Fastfloor(Utils.FastMin(vector.z, vector2.z)));
		this.SelectionStart = vector3i;
		this.SelectionEnd = vector3i + this.clipboard.size - Vector3i.one;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void removeBlockPreview()
	{
		this.previewGORot3.transform.DestroyChildren();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void createBlockPreviewFrom(Prefab _prefab)
	{
		if (this.previewGOParent == null)
		{
			this.previewGOParent = new GameObject("Preview");
			this.previewGOParent.transform.parent = null;
			this.previewGOParent.transform.localPosition = Vector3.zero;
			this.previewGORot1 = new GameObject("Rot1");
			this.previewGORot1.transform.parent = this.previewGOParent.transform;
			this.previewGORot2 = new GameObject("Rot2");
			this.previewGORot2.transform.parent = this.previewGORot1.transform;
			this.previewGORot3 = new GameObject("Rot3");
			this.previewGORot3.transform.parent = this.previewGORot2.transform;
		}
		else
		{
			this.removeBlockPreview();
		}
		ThreadManager.RunCoroutineSync(_prefab.ToTransform(true, true, true, false, this.previewGORot3.transform, "PrefabImposter", Vector3.zero, DynamicPrefabDecorator.PrefabPreviewLimit));
		Transform transform = this.previewGORot3.transform.Find("PrefabImposter");
		transform.localRotation = Quaternion.identity;
		transform.localPosition = Vector3.zero;
		Vector3 vector = new Vector3((float)(_prefab.size.x / 2), 0f, (float)(_prefab.size.z / 2));
		this.previewGORot1.transform.position = this.SelectionMin.ToVector3() - Origin.position;
		this.previewGORot1.transform.rotation = Quaternion.identity;
		this.previewGORot2.transform.localPosition = vector;
		this.previewGORot2.transform.localRotation = Quaternion.identity;
		this.previewGORot3.transform.localPosition = -vector;
		this.previewGORot3.transform.localRotation = Quaternion.identity;
		vector = -vector;
		vector.y = (float)(-(float)_prefab.size.y / 2);
		this.offsetToMax = vector + (_prefab.size - Vector3i.one).ToVector3() + Vector3.one * 0.5f;
		this.offsetToMin = vector + Vector3.one * 0.5f;
		this.selectionRotCenter = this.SelectionMin.ToVector3() - vector;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnOriginChanged(Vector3 _newOrigin)
	{
		if (this.previewGORot1 == null)
		{
			return;
		}
		this.previewGORot1.transform.position = this.SelectionMin.ToVector3() - Origin.position;
	}

	public void RotateFocusedBlock(WorldRayHitInfo _hitInfo, PlayerActionsLocal _playerActions)
	{
		if (!_hitInfo.bHitValid)
		{
			return;
		}
		Vector3i vector3i = (GameManager.Instance.World.IsEditor() && _playerActions.Run.IsPressed) ? _hitInfo.hit.blockPos : _hitInfo.lastBlockPos;
		BlockValue block = GameManager.Instance.World.ChunkClusters[_hitInfo.hit.clrIdx].GetBlock(vector3i);
		if (block.Block.shape.IsRotatable)
		{
			block.rotation = block.Block.shape.Rotate(false, (int)block.rotation);
			this.setBlock(_hitInfo.hit.clrIdx, vector3i, block);
		}
	}

	public void CheckKeys(ItemInventoryData _data, WorldRayHitInfo _hitInfo, PlayerActionsLocal playerActions)
	{
		if (LocalPlayerUI.primaryUI.windowManager.IsInputActive())
		{
			return;
		}
		this.hitInfo = _hitInfo;
		Vector3i vector3i = (_data.world.IsEditor() && playerActions.Run.IsPressed) ? _hitInfo.hit.blockPos : _hitInfo.lastBlockPos;
		ItemClassBlock.ItemBlockInventoryData itemBlockInventoryData = _data as ItemClassBlock.ItemBlockInventoryData;
		if (itemBlockInventoryData != null)
		{
			BlockValue bv = itemBlockInventoryData.itemValue.ToBlockValue();
			bv.rotation = itemBlockInventoryData.rotation;
			itemBlockInventoryData.rotation = bv.Block.BlockPlacementHelper.OnPlaceBlock(itemBlockInventoryData.mode, itemBlockInventoryData.localRot, GameManager.Instance.World, bv, this.hitInfo.hit, itemBlockInventoryData.holdingEntity.position).blockValue.rotation;
		}
		if (!GameManager.Instance.IsEditMode() && !GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled))
		{
			return;
		}
		if (playerActions.SelectionSet.IsPressed)
		{
			if (GameManager.Instance.World.ChunkClusters[_hitInfo.hit.clrIdx] == null)
			{
				return;
			}
			if (InputUtils.ControlKeyPressed)
			{
				return;
			}
			this.SelectionLockMode = 0;
			this.SelectionClrIdx = _hitInfo.hit.clrIdx;
			Vector3i vector3i2 = vector3i;
			if (!this.SelectionActive)
			{
				Vector3i selectionSize = this.SelectionSize;
				this.SelectionStart = vector3i2;
				if (this.SelectionLockMode == 1)
				{
					this.SelectionEnd = this.SelectionStart + selectionSize - Vector3i.one;
				}
				else
				{
					this.SelectionEnd = this.SelectionStart;
				}
				this.SelectionActive = true;
			}
			else
			{
				this.SelectionEnd = vector3i2;
			}
		}
		if (!GameManager.Instance.IsEditMode())
		{
			return;
		}
		if (playerActions.DensityM1.WasPressed || playerActions.DensityP1.WasPressed || playerActions.DensityM10.WasPressed || playerActions.DensityP10.WasPressed)
		{
			int num = (playerActions.DensityM1.WasPressed || playerActions.DensityP1.WasPressed) ? 1 : 10;
			if (playerActions.DensityM1.WasPressed || playerActions.DensityM10.WasPressed)
			{
				num = -num;
			}
			if (InputUtils.ControlKeyPressed)
			{
				num *= 50;
			}
			BlockValue block = GameManager.Instance.World.GetBlock(_hitInfo.hit.clrIdx, vector3i);
			Block block2 = block.Block;
			if (block2.BlockTag == BlockTags.Door)
			{
				if (num > 0)
				{
					num = ((block.damage + num >= block2.MaxDamagePlusDowngrades) ? (block2.MaxDamagePlusDowngrades - block.damage - 1) : num);
				}
				block2.DamageBlock(GameManager.Instance.World, _hitInfo.hit.clrIdx, vector3i, block, num, -1, null, false, false);
			}
			else
			{
				int num2;
				if (!this.SelectionActive)
				{
					num2 = (int)GameManager.Instance.World.GetDensity(_hitInfo.hit.clrIdx, vector3i);
				}
				else
				{
					num2 = (int)GameManager.Instance.World.GetDensity(0, this.m_selectionStartPoint);
				}
				num2 += num;
				num2 = Utils.FastClamp(num2, (int)MarchingCubes.DensityTerrain, (int)MarchingCubes.DensityAir);
				if (!this.SelectionActive)
				{
					GameManager.Instance.World.SetBlocksRPC(new List<BlockChangeInfo>
					{
						new BlockChangeInfo(_hitInfo.hit.clrIdx, vector3i, (sbyte)num2, false)
					});
				}
				else
				{
					BlockTools.CubeDensityRPC(GameManager.Instance, this.m_selectionStartPoint, this.m_SelectionEndPoint, (sbyte)num2);
				}
			}
		}
		if ((playerActions.FocusCopyBlock.WasPressed || (playerActions.Secondary.WasPressed && InputUtils.ControlKeyPressed)) && GameManager.Instance.IsEditMode() && _hitInfo.bHitValid && !_hitInfo.hit.blockValue.isair)
		{
			EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
			BlockValue blockValue = _hitInfo.hit.blockValue;
			if (blockValue.ischild)
			{
				Vector3i parentPos = blockValue.Block.multiBlockPos.GetParentPos(_hitInfo.hit.blockPos, blockValue);
				blockValue = GameManager.Instance.World.GetBlock(parentPos);
			}
			ItemStack itemStack = new ItemStack(blockValue.ToItemValue(), 99);
			if (blockValue.Block.GetAutoShapeType() != EAutoShapeType.Helper)
			{
				long textureFull = GameManager.Instance.World.ChunkCache.GetTextureFull(_hitInfo.hit.blockPos);
				itemStack.itemValue.Texture = textureFull;
			}
			if (primaryPlayer.inventory.GetItemCount(itemStack.itemValue, true, -1, -1, true) == 0 && primaryPlayer.inventory.CanTakeItem(itemStack))
			{
				int idx;
				if (primaryPlayer.inventory.AddItem(itemStack, out idx))
				{
					ItemClassBlock.ItemBlockInventoryData itemBlockInventoryData2 = primaryPlayer.inventory.GetItemDataInSlot(idx) as ItemClassBlock.ItemBlockInventoryData;
					if (itemBlockInventoryData2 != null)
					{
						itemBlockInventoryData2.damage = blockValue.damage;
						return;
					}
				}
			}
			else
			{
				ItemClassBlock.ItemBlockInventoryData itemBlockInventoryData3 = _data as ItemClassBlock.ItemBlockInventoryData;
				if (itemBlockInventoryData3 != null && this.hasSameShape(blockValue.type, primaryPlayer.inventory.holdingItemItemValue.type))
				{
					itemBlockInventoryData3.rotation = blockValue.rotation;
					itemBlockInventoryData3.damage = blockValue.damage;
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasSameShape(int _blockId1, int _blockId2)
	{
		Block block = Block.list[_blockId1];
		Block block2 = Block.list[_blockId2];
		return !(block.shape.GetType() != block2.shape.GetType()) && (!(block.shape is BlockShapeNew) || block.Properties.Values["Model"] == block2.Properties.Values["Model"]);
	}

	public bool ConsumeScrollWheel(ItemInventoryData _data, float _scrollWheelInput, PlayerActionsLocal _playerInput)
	{
		if ((_playerInput.Reload.IsPressed || _playerInput.PermanentActions.Reload.IsPressed) && _data is ItemClassBlock.ItemBlockInventoryData && Mathf.Abs(_scrollWheelInput) >= 0.001f)
		{
			ItemClassBlock.ItemBlockInventoryData itemBlockInventoryData = (ItemClassBlock.ItemBlockInventoryData)_data;
			itemBlockInventoryData.rotation = itemBlockInventoryData.itemValue.ToBlockValue().Block.BlockPlacementHelper.LimitRotation(itemBlockInventoryData.mode, ref itemBlockInventoryData.localRot, _data.hitInfo.hit, _scrollWheelInput > 0f, itemBlockInventoryData.itemValue.ToBlockValue(), itemBlockInventoryData.rotation);
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i createBlockMoveVector(Vector3 _relPlayerAxis)
	{
		Vector3i zero = Vector3i.zero;
		if (Math.Abs(_relPlayerAxis.x) > Math.Abs(_relPlayerAxis.z))
		{
			zero = new Vector3i(Mathf.Sign(_relPlayerAxis.x), 0f, 0f);
		}
		else
		{
			zero = new Vector3i(0f, 0f, Mathf.Sign(_relPlayerAxis.z));
		}
		return zero;
	}

	public bool ExecuteUseAction(ItemInventoryData _data, bool _bReleased, PlayerActionsLocal playerActions)
	{
		if (!(_data is ItemClassBlock.ItemBlockInventoryData))
		{
			return false;
		}
		bool flag = GameManager.Instance.IsEditMode() || GameStats.GetInt(EnumGameStats.GameModeId) == 2;
		if (flag && playerActions.Drop.IsPressed)
		{
			return false;
		}
		if (_bReleased)
		{
			return false;
		}
		if (Time.time - this.lastBuildTime < Constants.cBuildIntervall)
		{
			return true;
		}
		this.lastBuildTime = Time.time;
		ItemClassBlock.ItemBlockInventoryData itemBlockInventoryData = (ItemClassBlock.ItemBlockInventoryData)_data;
		EntityAlive holdingEntity = itemBlockInventoryData.holdingEntity;
		FastTags<TagGroup.Global> tags = FastTags<TagGroup.Global>.none;
		ItemClassBlock itemClassBlock = itemBlockInventoryData.item as ItemClassBlock;
		if (itemClassBlock != null)
		{
			tags = itemClassBlock.GetBlock().Tags;
		}
		if (EffectManager.GetValue(PassiveEffects.DisableItem, holdingEntity.inventory.holdingItemItemValue, 0f, holdingEntity, null, tags, true, true, true, true, true, 1, true, false) > 0f)
		{
			this.lastBuildTime = Time.time + 1f;
			Manager.PlayInsidePlayerHead("twitch_no_attack", -1, 0f, false, false);
			return false;
		}
		HitInfoDetails hitInfoDetails = itemBlockInventoryData.hitInfo.hit.Clone();
		if (!itemBlockInventoryData.hitInfo.bHitValid)
		{
			return false;
		}
		hitInfoDetails.blockPos = ((flag && playerActions.Run.IsPressed) ? itemBlockInventoryData.hitInfo.hit.blockPos : itemBlockInventoryData.hitInfo.lastBlockPos);
		BlockValue blockValue = itemBlockInventoryData.itemValue.ToBlockValue();
		Block block = blockValue.Block;
		blockValue.damage = itemBlockInventoryData.damage;
		blockValue.rotation = itemBlockInventoryData.rotation;
		World world = GameManager.Instance.World;
		if (!GameManager.Instance.IsEditMode())
		{
			int placementDistanceSq = block.GetPlacementDistanceSq();
			if (hitInfoDetails.distanceSq > (float)placementDistanceSq)
			{
				return true;
			}
			Vector3i freePlacementPosition = block.GetFreePlacementPosition(world, itemBlockInventoryData.hitInfo.hit.clrIdx, hitInfoDetails.blockPos, blockValue, holdingEntity);
			if (!holdingEntity.IsGodMode.Value && GameUtils.IsColliderWithinBlock(freePlacementPosition, blockValue))
			{
				return true;
			}
			if (hitInfoDetails.blockPos == Vector3i.zero)
			{
				return true;
			}
		}
		_data.holdingEntity.RightArmAnimationUse = true;
		BlockPlacement.Result result = block.BlockPlacementHelper.OnPlaceBlock(itemBlockInventoryData.mode, itemBlockInventoryData.localRot, GameManager.Instance.World, blockValue, hitInfoDetails, itemBlockInventoryData.holdingEntity.position);
		block.OnBlockPlaceBefore(itemBlockInventoryData.world, ref result, itemBlockInventoryData.holdingEntity, itemBlockInventoryData.world.GetGameRandom());
		blockValue = result.blockValue;
		block = blockValue.Block;
		if (blockValue.damage == 0)
		{
			blockValue.damage = block.StartDamage;
			result.blockValue.damage = block.StartDamage;
		}
		if (!playerActions.Run.IsPressed)
		{
			result.blockPos = block.GetFreePlacementPosition(itemBlockInventoryData.holdingEntity.world, 0, result.blockPos, blockValue, itemBlockInventoryData.holdingEntity);
		}
		if (!block.CanPlaceBlockAt(itemBlockInventoryData.world, result.clrIdx, result.blockPos, blockValue, false))
		{
			itemBlockInventoryData.holdingEntity.PlayOneShot("keystone_build_warning", false, false, false);
			return true;
		}
		eSetBlockResponse eSetBlockResponse;
		if (!BlockLimitTracker.instance.CanAddBlock(blockValue, result.blockPos, out eSetBlockResponse))
		{
			if (eSetBlockResponse != eSetBlockResponse.PowerBlockLimitExceeded)
			{
				if (eSetBlockResponse == eSetBlockResponse.StorageBlockLimitExceeded)
				{
					GameManager.ShowTooltip(GameManager.Instance.World.GetPrimaryPlayer(), "uicannotaddstorageblock", false);
				}
			}
			else
			{
				GameManager.ShowTooltip(GameManager.Instance.World.GetPrimaryPlayer(), "uicannotaddpowerblock", false);
			}
			return true;
		}
		if (!GameManager.Instance.IsEditMode())
		{
			if (block.IndexName == "lpblock")
			{
				if (!itemBlockInventoryData.world.CanPlaceLandProtectionBlockAt(itemBlockInventoryData.hitInfo.lastBlockPos, itemBlockInventoryData.world.gameManager.GetPersistentLocalPlayer()))
				{
					itemBlockInventoryData.holdingEntity.PlayOneShot("keystone_build_warning", false, false, false);
					return true;
				}
				itemBlockInventoryData.holdingEntity.PlayOneShot("keystone_placed", false, false, false);
			}
			else if (!itemBlockInventoryData.world.CanPlaceBlockAt(itemBlockInventoryData.hitInfo.lastBlockPos, itemBlockInventoryData.world.gameManager.GetPersistentLocalPlayer(), false))
			{
				itemBlockInventoryData.holdingEntity.PlayOneShot("keystone_build_warning", false, false, false);
				return true;
			}
		}
		BiomeDefinition biome = itemBlockInventoryData.world.GetBiome(result.blockPos.x, result.blockPos.z);
		if (biome != null && biome.Replacements.ContainsKey(result.blockValue.type))
		{
			result.blockValue.type = biome.Replacements[result.blockValue.type];
		}
		this.addToUndo(_data.hitInfo.hit.clrIdx, result.blockPos, GameManager.Instance.World.GetBlock(result.blockPos));
		if (Block.list[itemBlockInventoryData.itemValue.type].SelectAlternates)
		{
			if (itemBlockInventoryData.itemValue.TextureAllSides == 0)
			{
				block.PlaceBlock(itemBlockInventoryData.world, result, itemBlockInventoryData.holdingEntity);
			}
			else
			{
				BlockChangeInfo blockChangeInfo = new BlockChangeInfo(0, result.blockPos, blockValue, itemBlockInventoryData.holdingEntity.entityId);
				blockChangeInfo.textureFull = Chunk.TextureIdxToTextureFullValue64(itemBlockInventoryData.itemValue.TextureAllSides);
				blockChangeInfo.bChangeTexture = true;
				GameManager.Instance.World.SetBlocksRPC(new List<BlockChangeInfo>
				{
					blockChangeInfo
				});
			}
		}
		else if (itemBlockInventoryData.itemValue.Texture == 0L)
		{
			block.PlaceBlock(itemBlockInventoryData.world, result, itemBlockInventoryData.holdingEntity);
		}
		else
		{
			BlockChangeInfo blockChangeInfo2 = new BlockChangeInfo(0, result.blockPos, blockValue, itemBlockInventoryData.holdingEntity.entityId);
			blockChangeInfo2.textureFull = itemBlockInventoryData.itemValue.Texture;
			blockChangeInfo2.bChangeTexture = true;
			GameManager.Instance.World.SetBlocksRPC(new List<BlockChangeInfo>
			{
				blockChangeInfo2
			});
		}
		QuestEventManager.Current.BlockPlaced(block.GetBlockName(), result.blockPos);
		itemBlockInventoryData.holdingEntity.RightArmAnimationUse = true;
		itemBlockInventoryData.lastBuildTime = Time.time;
		GameManager.Instance.StartCoroutine(this.decInventoryLater(itemBlockInventoryData, itemBlockInventoryData.holdingEntity.inventory.holdingItemIdx));
		if (!block.shape.IsOmitTerrainSnappingUp && !block.IsTerrainDecoration)
		{
			itemBlockInventoryData.world.ChunkCache.SnapTerrainToPositionAroundRPC(itemBlockInventoryData.world, itemBlockInventoryData.hitInfo.lastBlockPos - Vector3i.up);
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator decInventoryLater(ItemInventoryData data, int index)
	{
		data.holdingEntity.inventory.WaitForSecondaryRelease = (data.holdingEntity.inventory.holdingItemStack.count == 1);
		yield return new WaitForSeconds(0.1f);
		if (!GameManager.Instance.IsEditMode())
		{
			ItemStack itemStack = data.holdingEntity.inventory.GetItem(index).Clone();
			if (itemStack.count > 0)
			{
				itemStack.count--;
			}
			data.holdingEntity.inventory.SetItem(index, itemStack);
		}
		BlockValue blockValue = data.itemValue.ToBlockValue();
		string clipName = "placeblock";
		Block block = blockValue.Block;
		if (block.CustomPlaceSound != null)
		{
			clipName = block.CustomPlaceSound;
		}
		data.holdingEntity.PlayOneShot(clipName, false, false, false);
		yield break;
	}

	public bool ExecuteAttackAction(ItemInventoryData _data, bool _bReleased, PlayerActionsLocal playerActions)
	{
		if (!_bReleased)
		{
			return false;
		}
		bool flag = false;
		if (GameManager.Instance.IsEditMode() && playerActions.Drop.IsPressed)
		{
			return false;
		}
		if (!playerActions.SelectionSet.IsPressed && this.SelectionActive)
		{
			if (!playerActions.Drop.IsPressed)
			{
				flag = (flag || this.SelectionActive);
				if (this.SelectionLockMode == 1)
				{
					Vector3i selectionSize = this.SelectionSize;
					this.SelectionStart = _data.hitInfo.hit.blockPos;
					this.SelectionEnd = this.SelectionStart + selectionSize - Vector3i.one;
				}
				else if (this.SelectionLockMode == 0)
				{
					this.SelectionActive = false;
				}
			}
		}
		else if (GameManager.Instance.IsEditMode() && playerActions.Run.IsPressed && _data.hitInfo.bHitValid)
		{
			Vector3i blockPos = playerActions.Run.IsPressed ? _data.hitInfo.hit.blockPos : _data.hitInfo.lastBlockPos;
			this.setBlock(_data.hitInfo.hit.clrIdx, blockPos, BlockValue.Air);
			flag = true;
		}
		else if (_data is ItemClassBlock.ItemBlockInventoryData)
		{
			ItemClassBlock.ItemBlockInventoryData itemBlockInventoryData = (ItemClassBlock.ItemBlockInventoryData)_data;
			itemBlockInventoryData.itemValue.ToBlockValue().Block.RotateHoldingBlock(itemBlockInventoryData, true, true);
			flag = true;
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void rotateSelectionAroundY()
	{
		Vector3i other = new Vector3i(Mathf.Abs(this.m_selectionStartPoint.x - this.m_SelectionEndPoint.x), Mathf.Abs(this.m_selectionStartPoint.y - this.m_SelectionEndPoint.y), Mathf.Abs(this.m_selectionStartPoint.z - this.m_SelectionEndPoint.z));
		Vector3i vector3i = new Vector3i(Mathf.Min(this.m_selectionStartPoint.x, this.m_SelectionEndPoint.x), Mathf.Min(this.m_selectionStartPoint.y, this.m_SelectionEndPoint.y), Mathf.Min(this.m_selectionStartPoint.z, this.m_SelectionEndPoint.z));
		Prefab prefab = BlockTools.CopyIntoStorage(GameManager.Instance, vector3i, vector3i + other);
		this.BeginUndo(0);
		new Prefab(prefab.size)
		{
			bCopyAirBlocks = true
		}.CopyIntoRPC(GameManager.Instance, vector3i, false);
		prefab.RotateY(false, 1);
		prefab.CopyIntoRPC(GameManager.Instance, vector3i, this.copyPasteAirBlocks);
		this.SelectionStart = vector3i;
		this.SelectionEnd = vector3i + prefab.size - Vector3i.one;
		this.EndUndo(0, false);
	}

	public bool SelectionActive
	{
		get
		{
			return SelectionBoxManager.Instance.IsActive("Selection", "SingleInstance");
		}
		set
		{
			if (this.SelectionActive != value)
			{
				SelectionBox selectionBox = this.SelectionBox;
				SelectionBoxManager.Instance.SetActive("Selection", "SingleInstance", value);
			}
		}
	}

	public int SelectionLockMode
	{
		get
		{
			return this.m_iSelectionLockMode;
		}
		set
		{
			if (this.m_iSelectionLockMode != value)
			{
				this.m_iSelectionLockMode = value;
				this.SelectionBox.SetVisible(this.SelectionActive);
				Color c = BlockToolSelection.colInactive;
				if (this.m_iSelectionLockMode == 1)
				{
					c = new Color(0.5f, 0f, 1f, 0.5f);
					this.removeBlockPreview();
				}
				else if (this.m_iSelectionLockMode == 2)
				{
					c = BlockToolSelection.colActive;
				}
				else
				{
					this.removeBlockPreview();
				}
				this.SelectionBox.SetAllFacesColor(c, true);
			}
		}
	}

	public Vector3i SelectionMin
	{
		get
		{
			return new Vector3i(Utils.FastMin(this.SelectionStart.x, this.SelectionEnd.x), Utils.FastMin(this.SelectionStart.y, this.SelectionEnd.y), Utils.FastMin(this.SelectionStart.z, this.SelectionEnd.z));
		}
	}

	public Vector3i SelectionStart
	{
		get
		{
			return this.m_selectionStartPoint;
		}
		set
		{
			if (!this.m_selectionStartPoint.Equals(value))
			{
				this.m_selectionStartPoint = value;
				this.updateSelection();
			}
		}
	}

	public Vector3i SelectionEnd
	{
		get
		{
			return this.m_SelectionEndPoint;
		}
		set
		{
			if (!this.m_SelectionEndPoint.Equals(value))
			{
				this.m_SelectionEndPoint = value;
				this.updateSelection();
			}
		}
	}

	public Vector3i SelectionSize
	{
		get
		{
			return new Vector3i(Mathf.Abs(this.m_selectionStartPoint.x - this.m_SelectionEndPoint.x) + 1, Mathf.Abs(this.m_selectionStartPoint.y - this.m_SelectionEndPoint.y) + 1, Mathf.Abs(this.m_selectionStartPoint.z - this.m_SelectionEndPoint.z) + 1);
		}
	}

	public void SelectionSizeSet(Vector3i _size)
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateSelection()
	{
		Vector3 v = this.SelectionSize.ToVector3();
		Vector3 v2 = new Vector3((float)Mathf.Min(this.m_selectionStartPoint.x, this.m_SelectionEndPoint.x), (float)Mathf.Min(this.m_selectionStartPoint.y, this.m_SelectionEndPoint.y), (float)Mathf.Min(this.m_selectionStartPoint.z, this.m_SelectionEndPoint.z));
		this.SelectionBox.SetPositionAndSize(new Vector3i(v2), new Vector3i(v));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool setBlock(int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
	{
		BlockValue block = GameManager.Instance.World.GetBlock(_clrIdx, _blockPos);
		if (block.rawData == _blockValue.rawData)
		{
			return false;
		}
		long texture = GameManager.Instance.World.GetTexture(_blockPos.x, _blockPos.y, _blockPos.z);
		this.undoQueue.Add(new List<BlockChangeInfo>
		{
			new BlockChangeInfo(_clrIdx, _blockPos, block, MarchingCubes.DensityAir, texture)
		});
		if (this.undoQueue.Count > 100)
		{
			this.undoQueue.RemoveAt(0);
		}
		if (_blockValue.Block.shape.IsTerrain())
		{
			GameManager.Instance.World.SetBlockRPC(_clrIdx, _blockPos, _blockValue, MarchingCubes.DensityTerrain);
		}
		else
		{
			GameManager.Instance.World.SetBlockRPC(_clrIdx, _blockPos, _blockValue);
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void addToUndo(int _clrIdx, Vector3i _blockPos, BlockValue _oldBlockValue)
	{
		long texture = GameManager.Instance.World.GetTexture(_blockPos.x, _blockPos.y, _blockPos.z);
		this.undoQueue.Add(new List<BlockChangeInfo>
		{
			new BlockChangeInfo(_clrIdx, _blockPos, _oldBlockValue, MarchingCubes.DensityAir, texture)
		});
		if (this.undoQueue.Count > 100)
		{
			this.undoQueue.RemoveAt(0);
		}
	}

	public void BeginUndo(int _clrIdx)
	{
		this.undoChanges = new List<BlockChangeInfo>();
		this.undoClrIdx = _clrIdx;
		ChunkCluster chunkCluster = GameManager.Instance.World.ChunkClusters[_clrIdx];
		if (chunkCluster != null)
		{
			chunkCluster.OnBlockChangedDelegates += this.undoBlockChangeDelegate;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void undoBlockChangeDelegate(Vector3i pos, BlockValue bvOld, sbyte oldDens, long oldTex, BlockValue bvNew)
	{
		if (this.undoChanges != null && !bvOld.ischild)
		{
			this.undoChanges.Add(new BlockChangeInfo(this.undoClrIdx, pos, bvOld, oldDens, oldTex));
		}
	}

	public void EndUndo(int _clrIdx, bool _bRedo = false)
	{
		ChunkCluster chunkCluster = GameManager.Instance.World.ChunkClusters[_clrIdx];
		if (chunkCluster != null)
		{
			chunkCluster.OnBlockChangedDelegates -= this.undoBlockChangeDelegate;
		}
		if (this.undoChanges.Count > 0)
		{
			this.undoChanges.Reverse();
			if (!_bRedo)
			{
				this.undoQueue.Add(this.undoChanges);
			}
			else
			{
				this.redoQueue.Add(this.undoChanges);
			}
		}
		if (this.undoQueue.Count > 100)
		{
			this.undoQueue.RemoveAt(0);
		}
		if (this.redoQueue.Count > 100)
		{
			this.redoQueue.RemoveAt(0);
		}
		this.undoChanges = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i blockCopy(Prefab _storage)
	{
		return _storage.CopyFromWorldWithEntities(GameManager.Instance.World, this.SelectionStart, this.SelectionEnd, null);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void blockPaste(int _clrIdx, Vector3i _destPos, Prefab _storage)
	{
		this.BeginUndo(0);
		_storage.CopyIntoRPC(GameManager.Instance, _destPos, this.copyPasteAirBlocks);
		this.SelectionActive = true;
		this.SelectionStart = _destPos;
		this.SelectionEnd = _destPos + _storage.size - Vector3i.one;
		this.EndUndo(0, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i sizeFromPositions(Vector3i _posStart, Vector3i _posEnd)
	{
		Vector3i vector3i = new Vector3i(Math.Min(_posStart.x, _posEnd.x), Math.Min(_posStart.y, _posEnd.y), Math.Min(_posStart.z, _posEnd.z));
		Vector3i vector3i2 = new Vector3i(Math.Max(_posStart.x, _posEnd.x), Math.Max(_posStart.y, _posEnd.y), Math.Max(_posStart.z, _posEnd.z));
		return new Vector3i(Math.Abs(vector3i2.x - vector3i.x) + 1, Math.Abs(vector3i2.y - vector3i.y) + 1, Math.Abs(vector3i2.z - vector3i.z) + 1);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void union(Vector3i _pos1Start, Vector3i _pos1End, Vector3i _pos2Start, Vector3i _pos2End, out Vector3i _unionStart, out Vector3i _unionEnd)
	{
		_unionStart = new Vector3i(Utils.FastMin(_pos1Start.x, _pos1End.x, _pos2Start.x, _pos2End.x), Utils.FastMin(_pos1Start.y, _pos1End.y, _pos2Start.y, _pos2End.y), Utils.FastMin(_pos1Start.z, _pos1End.z, _pos2Start.z, _pos2End.z));
		_unionEnd = new Vector3i(Utils.FastMax(_pos1Start.x, _pos1End.x, _pos2Start.x, _pos2End.x), Utils.FastMax(_pos1Start.y, _pos1End.y, _pos2Start.y, _pos2End.y), Utils.FastMax(_pos1Start.z, _pos1End.z, _pos2Start.z, _pos2End.z));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void blockUndo()
	{
		if (this.undoQueue.Count == 0)
		{
			return;
		}
		List<BlockChangeInfo> list = this.undoQueue[this.undoQueue.Count - 1];
		if (this.redoQueue.Count > 100)
		{
			this.redoQueue.RemoveAt(0);
		}
		this.BeginUndo(list[0].clrIdx);
		GameManager.Instance.SetBlocksRPC(list, null);
		this.undoQueue.RemoveAt(this.undoQueue.Count - 1);
		this.EndUndo(list[0].clrIdx, true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void blockRedo()
	{
		if (this.redoQueue.Count == 0)
		{
			return;
		}
		List<BlockChangeInfo> list = this.redoQueue[this.redoQueue.Count - 1];
		this.BeginUndo(list[0].clrIdx);
		GameManager.Instance.SetBlocksRPC(list, null);
		this.redoQueue.RemoveAt(this.redoQueue.Count - 1);
		this.EndUndo(list[0].clrIdx, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isPrefabActive()
	{
		return GameManager.Instance.GetDynamicPrefabDecorator() != null && GameManager.Instance.GetDynamicPrefabDecorator().ActivePrefab != null;
	}

	public bool OnSelectionBoxActivated(string _category, string _name, bool _bActivated)
	{
		this.SelectionBox.SetVisible(_bActivated);
		if (!_bActivated)
		{
			this.SelectionLockMode = 0;
		}
		return true;
	}

	public void OnSelectionBoxMoved(string _category, string _name, Vector3 _moveVector)
	{
		Vector3i other = new Vector3i(_moveVector);
		Vector3i zero = Vector3i.zero;
		int selectionLockMode = this.SelectionLockMode;
		this.SelectionStart += other;
		this.SelectionEnd += other;
		this.selectionRotCenter += other.ToVector3();
		if (this.SelectionLockMode == 2)
		{
			this.previewGORot1.transform.position += _moveVector;
		}
	}

	public void OnSelectionBoxSized(string _category, string _name, int _dTop, int _dBottom, int _dNorth, int _dSouth, int _dEast, int _dWest)
	{
		if (this.SelectionLockMode == 2)
		{
			this.SelectionLockMode = 0;
			return;
		}
		if (_dEast != 0 && (_dEast >= 0 || this.SelectionSize.x > 1))
		{
			if (this.SelectionEnd.x > this.SelectionStart.x)
			{
				this.SelectionEnd = new Vector3i(this.SelectionEnd.x + _dEast, this.SelectionEnd.y, this.SelectionEnd.z);
			}
			else
			{
				this.SelectionStart = new Vector3i(this.SelectionStart.x + _dEast, this.SelectionStart.y, this.SelectionStart.z);
			}
		}
		if (_dWest != 0 && (_dWest >= 0 || this.SelectionSize.x > 1))
		{
			if (this.SelectionEnd.x <= this.SelectionStart.x)
			{
				this.SelectionEnd = new Vector3i(this.SelectionEnd.x - _dWest, this.SelectionEnd.y, this.SelectionEnd.z);
			}
			else
			{
				this.SelectionStart = new Vector3i(this.SelectionStart.x - _dWest, this.SelectionStart.y, this.SelectionStart.z);
			}
		}
		if (_dTop != 0 && (_dTop >= 0 || this.SelectionSize.y > 1))
		{
			if (this.SelectionEnd.y > this.SelectionStart.y)
			{
				this.SelectionEnd = new Vector3i(this.SelectionEnd.x, this.SelectionEnd.y + _dTop, this.SelectionEnd.z);
			}
			else
			{
				this.SelectionStart = new Vector3i(this.SelectionStart.x, this.SelectionStart.y + _dTop, this.SelectionStart.z);
			}
		}
		if (_dBottom != 0 && (_dBottom >= 0 || this.SelectionSize.y > 1))
		{
			if (this.SelectionEnd.y <= this.SelectionStart.y)
			{
				this.SelectionEnd = new Vector3i(this.SelectionEnd.x, this.SelectionEnd.y - _dBottom, this.SelectionEnd.z);
			}
			else
			{
				this.SelectionStart = new Vector3i(this.SelectionStart.x, this.SelectionStart.y - _dBottom, this.SelectionStart.z);
			}
		}
		if (_dNorth != 0 && (_dNorth >= 0 || this.SelectionSize.z > 1))
		{
			if (this.SelectionEnd.z > this.SelectionStart.z)
			{
				this.SelectionEnd = new Vector3i(this.SelectionEnd.x, this.SelectionEnd.y, this.SelectionEnd.z + _dNorth);
			}
			else
			{
				this.SelectionStart = new Vector3i(this.SelectionStart.x, this.SelectionStart.y, this.SelectionStart.z + _dNorth);
			}
		}
		if (_dSouth != 0 && (_dSouth >= 0 || this.SelectionSize.z > 1))
		{
			if (this.SelectionEnd.z <= this.SelectionStart.z)
			{
				this.SelectionEnd = new Vector3i(this.SelectionEnd.x, this.SelectionEnd.y, this.SelectionEnd.z - _dSouth);
				return;
			}
			this.SelectionStart = new Vector3i(this.SelectionStart.x, this.SelectionStart.y, this.SelectionStart.z - _dSouth);
		}
	}

	public void OnSelectionBoxMirrored(Vector3i _selAxis)
	{
		EnumMirrorAlong axis = EnumMirrorAlong.XAxis;
		if (_selAxis.y != 0)
		{
			axis = EnumMirrorAlong.YAxis;
		}
		else if (_selAxis.z != 0)
		{
			axis = EnumMirrorAlong.ZAxis;
		}
		if (this.previewGORot3 != null && this.previewGORot3.transform.childCount > 0)
		{
			this.clipboard.Mirror(axis);
			this.removeBlockPreview();
			this.createBlockPreviewFrom(this.clipboard);
			return;
		}
		Prefab prefab = new Prefab();
		prefab.CopyFromWorldWithEntities(GameManager.Instance.World, this.SelectionStart, this.SelectionEnd, null);
		prefab.Mirror(axis);
		prefab.CopyIntoRPC(GameManager.Instance, this.SelectionMin, this.copyPasteAirBlocks);
	}

	public bool OnSelectionBoxDelete(string _category, string _name)
	{
		return false;
	}

	public bool OnSelectionBoxIsAvailable(string _category, EnumSelectionBoxAvailabilities _criteria)
	{
		return _criteria == EnumSelectionBoxAvailabilities.CanResize || _criteria == EnumSelectionBoxAvailabilities.CanMirror;
	}

	public void OnSelectionBoxShowProperties(bool _bVisible, GUIWindowManager _windowManager)
	{
	}

	public void OnSelectionBoxRotated(string _category, string _name)
	{
		if (this.SelectionLockMode == 2)
		{
			this.rotatePreviewAroundY();
			return;
		}
		this.rotateSelectionAroundY();
	}

	public string GetDebugOutput()
	{
		if (this.SelectionActive)
		{
			return string.Format("Selection pos/size: {0}/{1}", this.SelectionStart.ToString(), this.SelectionSize.ToString());
		}
		return "-";
	}

	public List<NGuiAction> GetActions()
	{
		return this.actions;
	}

	public void LoadPrefabIntoClipboard(Prefab _prefab)
	{
		this.clipboard = _prefab;
		this.SelectionLockMode = 2;
		if (this.SelectionSize != this.clipboard.size)
		{
			this.SelectionEnd = this.SelectionStart + this.clipboard.size - Vector3i.one;
		}
		this.SelectionActive = true;
		this.createBlockPreviewFrom(this.clipboard);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cBlockUndoRedoCount = 100;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Color colActive = new Color(1f, 0f, 0f, 0.5f);

	[PublicizedFrom(EAccessModifier.Private)]
	public static Color colInactive = new Color(0f, 0f, 1f, 0.5f);

	public static BlockToolSelection Instance;

	public Prefab clipboard = new Prefab();

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i m_selectionStartPoint;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i m_SelectionEndPoint;

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_iSelectionLockMode;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<List<BlockChangeInfo>> undoQueue = new List<List<BlockChangeInfo>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<List<BlockChangeInfo>> redoQueue = new List<List<BlockChangeInfo>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastBuildTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public const string SelectionBoxName = "SingleInstance";

	[PublicizedFrom(EAccessModifier.Private)]
	public List<NGuiAction> actions;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject previewGOParent;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject previewGORot1;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject previewGORot2;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject previewGORot3;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool copyPasteAirBlocks = true;

	public PlayerActionsLocal playerInput;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 selectionRotCenter;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 offsetToMin;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 offsetToMax;

	[PublicizedFrom(EAccessModifier.Private)]
	public WorldRayHitInfo hitInfo;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bWaitForRelease;

	public int SelectionClrIdx;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<BlockChangeInfo> undoChanges = new List<BlockChangeInfo>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int undoClrIdx;
}

﻿using System;
using System.Collections;
using System.Collections.Generic;
using Audio;
using GUI_2;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionTextureBlock : ItemActionRanged
{
	public override ItemActionData CreateModifierData(ItemInventoryData _invData, int _indexInEntityOfAction)
	{
		return new ItemActionTextureBlock.ItemActionTextureBlockData(_invData, _indexInEntityOfAction, "Muzzle/Particle1");
	}

	public override void ReadFrom(DynamicProperties _props)
	{
		base.ReadFrom(_props);
		if (_props.Values.ContainsKey("RemoveTexture"))
		{
			this.bRemoveTexture = StringParsers.ParseBool(_props.Values["RemoveTexture"], 0, -1, true);
		}
		if (_props.Values.ContainsKey("DefaultTextureID"))
		{
			this.DefaultTextureID = Convert.ToInt32(_props.Values["DefaultTextureID"]);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override int getUserData(ItemActionData _actionData)
	{
		ItemActionTextureBlock.ItemActionTextureBlockData itemActionTextureBlockData = (ItemActionTextureBlock.ItemActionTextureBlockData)_actionData;
		int textureID = (int)BlockTextureData.list[itemActionTextureBlockData.idx].TextureID;
		Color color;
		if (textureID == 0)
		{
			color = Color.gray;
		}
		else
		{
			color = MeshDescription.meshes[0].textureAtlas.uvMapping[textureID].color;
		}
		return ((int)(color.r * 255f) & 255) | ((int)(color.g * 255f) << 8 & 65280) | ((int)(color.b * 255f) << 16 & 16711680);
	}

	public override void ItemActionEffects(GameManager _gameManager, ItemActionData _actionData, int _firingState, Vector3 _startPos, Vector3 _direction, int _userData = 0)
	{
		base.ItemActionEffects(_gameManager, _actionData, _firingState, _startPos, _direction, _userData);
		if (_firingState != 0 && _actionData.invData.model != null)
		{
			ParticleSystem[] componentsInChildren = _actionData.invData.model.GetComponentsInChildren<ParticleSystem>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				Renderer component = componentsInChildren[i].GetComponent<Renderer>();
				if (component != null)
				{
					component.material.SetColor("_Color", new Color32((byte)(_userData & 255), (byte)(_userData >> 8 & 255), (byte)(_userData >> 16 & 255), byte.MaxValue));
				}
			}
		}
	}

	public override void StartHolding(ItemActionData _data)
	{
		base.StartHolding(_data);
		ItemActionTextureBlock.ItemActionTextureBlockData itemActionTextureBlockData = (ItemActionTextureBlock.ItemActionTextureBlockData)_data;
		itemActionTextureBlockData.idx = itemActionTextureBlockData.invData.itemValue.Meta;
	}

	public override bool ConsumeScrollWheel(ItemActionData _actionData, float _scrollWheelInput, PlayerActionsLocal _playerInput)
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool checkAmmo(ItemActionData _actionData)
	{
		if (this.InfiniteAmmo || GameStats.GetInt(EnumGameStats.GameModeId) == 2 || GameStats.GetInt(EnumGameStats.GameModeId) == 8)
		{
			return true;
		}
		EntityAlive holdingEntity = _actionData.invData.holdingEntity;
		return holdingEntity.bag.GetItemCount(this.currentMagazineItem, -1, -1, true) > 0 || holdingEntity.inventory.GetItemCount(this.currentMagazineItem, false, -1, -1, true) > 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool decreaseAmmo(ItemActionData _actionData)
	{
		if (this.InfiniteAmmo)
		{
			return true;
		}
		if (GameStats.GetInt(EnumGameStats.GameModeId) == 2 || GameStats.GetInt(EnumGameStats.GameModeId) == 8)
		{
			return true;
		}
		ItemActionTextureBlock.ItemActionTextureBlockData itemActionTextureBlockData = (ItemActionTextureBlock.ItemActionTextureBlockData)_actionData;
		int num = (int)BlockTextureData.list[itemActionTextureBlockData.idx].PaintCost;
		EntityAlive holdingEntity = _actionData.invData.holdingEntity;
		ItemValue itemValue = this.currentMagazineItem;
		int itemCount = holdingEntity.bag.GetItemCount(itemValue, -1, -1, true);
		int itemCount2 = holdingEntity.inventory.GetItemCount(itemValue, false, -1, -1, true);
		if (itemCount + itemCount2 >= num)
		{
			num -= holdingEntity.bag.DecItem(itemValue, num, false, null);
			if (num > 0)
			{
				holdingEntity.inventory.DecItem(itemValue, num, false, null);
			}
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void ConsumeAmmo(ItemActionData _actionData)
	{
	}

	public override void OnHoldingUpdate(ItemActionData _actionData)
	{
		base.OnHoldingUpdate(_actionData);
		ItemActionTextureBlock.ItemActionTextureBlockData itemActionTextureBlockData = (ItemActionTextureBlock.ItemActionTextureBlockData)_actionData;
		if (itemActionTextureBlockData.bReplacePaintNextTime && Time.time - itemActionTextureBlockData.lastTimeReplacePaintShown > 5f)
		{
			itemActionTextureBlockData.lastTimeReplacePaintShown = Time.time;
			GameManager.ShowTooltip(GameManager.Instance.World.GetLocalPlayers()[0], Localization.Get("ttPaintedTextureReplaced", false), false);
		}
	}

	public override void ExecuteAction(ItemActionData _actionData, bool _bReleased)
	{
		ItemValue holdingItemItemValue = _actionData.invData.holdingEntity.inventory.holdingItemItemValue;
		this.currentMagazineItem = ItemClass.GetItem(this.MagazineItemNames[(int)holdingItemItemValue.SelectedAmmoTypeIndex], false);
		if ((double)_actionData.invData.holdingEntity.speedForward > 0.009)
		{
			this.rayCastDelay = AnimationDelayData.AnimationDelay[_actionData.invData.item.HoldType.Value].RayCastMoving;
		}
		else
		{
			this.rayCastDelay = AnimationDelayData.AnimationDelay[_actionData.invData.item.HoldType.Value].RayCast;
		}
		base.ExecuteAction(_actionData, _bReleased);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override Vector3 fireShot(int _shotIdx, ItemActionRanged.ItemActionDataRanged _actionData, ref bool hitEntity)
	{
		hitEntity = true;
		GameManager.Instance.StartCoroutine(this.fireShotLater(_shotIdx, _actionData));
		return Vector3.zero;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector3 ProjectVectorOnPlane(Vector3 planeNormal, Vector3 vector)
	{
		return vector - Vector3.Dot(vector, planeNormal) * planeNormal;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool checkBlockCanBeChanged(World _world, Vector3i _blockPos, PersistentPlayerData lpRelative)
	{
		return _world.CanPlaceBlockAt(_blockPos, lpRelative, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool checkBlockCanBePainted(World _world, Vector3i _blockPos, BlockValue _blockValue, PersistentPlayerData _lpRelative)
	{
		if (_blockValue.isair)
		{
			return false;
		}
		Block block = _blockValue.Block;
		return block.shape is BlockShapeNew && block.MeshIndex == 0 && this.checkBlockCanBeChanged(_world, _blockPos, _lpRelative);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void getParentBlock(ref BlockValue _blockValue, ref Vector3i _blockPos, ChunkCluster _cc)
	{
		Block block = _blockValue.Block;
		if (_blockValue.ischild)
		{
			Log.Warning("Trying to paint multiblock block: " + _blockValue.Block.GetBlockName());
			_blockPos = block.multiBlockPos.GetParentPos(_blockPos, _blockValue);
			_blockValue = _cc.GetBlock(_blockPos);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int getCurrentPaintIdx(ChunkCluster _cc, Vector3i _blockPos, BlockFace _blockFace, BlockValue _blockValue)
	{
		int blockFaceTexture = _cc.GetBlockFaceTexture(_blockPos, _blockFace);
		if (blockFaceTexture != 0)
		{
			return blockFaceTexture;
		}
		string text;
		return GameUtils.FindPaintIdForBlockFace(_blockValue, _blockFace, out text);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemActionTextureBlock.EPaintResult paintFace(ChunkCluster _cc, int _entityId, ItemActionTextureBlock.ItemActionTextureBlockData _actionData, Vector3i _blockPos, BlockFace _blockFace, BlockValue _blockValue)
	{
		int currentPaintIdx = this.getCurrentPaintIdx(_cc, _blockPos, _blockFace, _blockValue);
		if (_actionData.idx == currentPaintIdx)
		{
			return ItemActionTextureBlock.EPaintResult.SamePaint;
		}
		if (!this.decreaseAmmo(_actionData))
		{
			return ItemActionTextureBlock.EPaintResult.NoPaintAvailable;
		}
		GameManager.Instance.SetBlockTextureServer(_blockPos, _blockFace, _actionData.idx, _entityId);
		return ItemActionTextureBlock.EPaintResult.Painted;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemActionTextureBlock.EPaintResult paintBlock(World _world, ChunkCluster _cc, int _entityId, ItemActionTextureBlock.ItemActionTextureBlockData _actionData, Vector3i _blockPos, BlockFace _blockFace, BlockValue _blockValue, PersistentPlayerData _lpRelative)
	{
		this.getParentBlock(ref _blockValue, ref _blockPos, _cc);
		if (!this.checkBlockCanBePainted(_world, _blockPos, _blockValue, _lpRelative))
		{
			return ItemActionTextureBlock.EPaintResult.CanNotPaint;
		}
		if (BlockToolSelection.Instance.SelectionActive)
		{
			BoundsInt boundsInt = new BoundsInt(BlockToolSelection.Instance.SelectionMin, BlockToolSelection.Instance.SelectionSize);
			if (!boundsInt.Contains(_blockPos))
			{
				return ItemActionTextureBlock.EPaintResult.CanNotPaint;
			}
		}
		if (!_actionData.bPaintAllSides)
		{
			return this.paintFace(_cc, _entityId, _actionData, _blockPos, _blockFace, _blockValue);
		}
		int num = 0;
		for (int i = 0; i <= 5; i++)
		{
			_blockFace = (BlockFace)i;
			ItemActionTextureBlock.EPaintResult epaintResult = this.paintFace(_cc, _entityId, _actionData, _blockPos, _blockFace, _blockValue);
			if (epaintResult == ItemActionTextureBlock.EPaintResult.NoPaintAvailable)
			{
				return epaintResult;
			}
			if (epaintResult == ItemActionTextureBlock.EPaintResult.Painted)
			{
				num++;
			}
		}
		if (num == 0)
		{
			return ItemActionTextureBlock.EPaintResult.SamePaint;
		}
		return ItemActionTextureBlock.EPaintResult.Painted;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void floodFill(World _world, ChunkCluster _cc, int _entityId, ItemActionTextureBlock.ItemActionTextureBlockData _actionData, PersistentPlayerData _lpRelative, int _sourcePaint, Vector3 _hitPosition, Vector3 _hitFaceNormal, Vector3 _dir1, Vector3 _dir2)
	{
		this.visitedPositions.Clear();
		this.visitedRays.Clear();
		this.positionsToCheck.Clear();
		this.positionsToCheck.Push(new Vector2i(0, 0));
		while (this.positionsToCheck.Count > 0)
		{
			Vector2i vector2i = this.positionsToCheck.Pop();
			if (!this.visitedRays.ContainsKey(vector2i))
			{
				this.visitedRays.Add(vector2i, true);
				Vector3 origin = _hitPosition + _hitFaceNormal * 0.2f + (float)vector2i.x * _dir1 + (float)vector2i.y * _dir2;
				Vector3 direction = -_hitFaceNormal * 0.3f;
				float magnitude = direction.magnitude;
				if (Voxel.Raycast(_world, new Ray(origin, direction), magnitude, -555528197, 69, 0f))
				{
					this.worldRayHitInfo.CopyFrom(Voxel.voxelRayHitInfo);
					BlockValue blockValue = this.worldRayHitInfo.hit.blockValue;
					Vector3i blockPos = this.worldRayHitInfo.hit.blockPos;
					bool flag2;
					bool flag;
					if (this.worldRayHitInfo.hitTriangleIdx >= 0 && (!(flag = this.visitedPositions.TryGetValue(blockPos, out flag2)) || flag2))
					{
						if (!flag)
						{
							Vector3 vector;
							Vector3 normalized;
							BlockFace blockFaceFromHitInfo = GameUtils.GetBlockFaceFromHitInfo(blockPos, blockValue, this.worldRayHitInfo.hitCollider, this.worldRayHitInfo.hitTriangleIdx, out vector, out normalized);
							if (blockFaceFromHitInfo == BlockFace.None)
							{
								continue;
							}
							normalized = normalized.normalized;
							if ((double)(normalized - _hitFaceNormal).sqrMagnitude > 0.01)
							{
								continue;
							}
							if (this.getCurrentPaintIdx(_cc, blockPos, blockFaceFromHitInfo, blockValue) != _sourcePaint)
							{
								this.visitedPositions.Add(blockPos, false);
								continue;
							}
							ItemActionTextureBlock.EPaintResult epaintResult = this.paintBlock(_world, _cc, _entityId, _actionData, blockPos, blockFaceFromHitInfo, blockValue, _lpRelative);
							if (epaintResult == ItemActionTextureBlock.EPaintResult.CanNotPaint || epaintResult == ItemActionTextureBlock.EPaintResult.NoPaintAvailable)
							{
								this.visitedPositions.Add(blockPos, false);
								continue;
							}
							this.visitedPositions.Add(blockPos, true);
						}
						this.positionsToCheck.Push(vector2i + Vector2i.down);
						this.positionsToCheck.Push(vector2i + Vector2i.up);
						this.positionsToCheck.Push(vector2i + Vector2i.left);
						this.positionsToCheck.Push(vector2i + Vector2i.right);
					}
				}
			}
		}
		this.visitedPositions.Clear();
		this.visitedRays.Clear();
		this.positionsToCheck.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator fireShotLater(int _shotIdx, ItemActionRanged.ItemActionDataRanged _actionData)
	{
		yield return new WaitForSeconds(this.rayCastDelay);
		EntityAlive holdingEntity = _actionData.invData.holdingEntity;
		PersistentPlayerData playerDataFromEntityID = GameManager.Instance.GetPersistentPlayerList().GetPlayerDataFromEntityID(holdingEntity.entityId);
		Vector3 direction = holdingEntity.GetLookVector((_actionData.muzzle != null) ? _actionData.muzzle.forward : Vector3.zero);
		Vector3i blockPos;
		BlockValue blockValue;
		BlockFace blockFaceFromHitInfo;
		WorldRayHitInfo worldRayHitInfo;
		if (this.getHitBlockFace(_actionData, out blockPos, out blockValue, out blockFaceFromHitInfo, out worldRayHitInfo) == -1 || worldRayHitInfo == null || !worldRayHitInfo.bHitValid)
		{
			yield break;
		}
		ItemActionTextureBlock.ItemActionTextureBlockData itemActionTextureBlockData = (ItemActionTextureBlock.ItemActionTextureBlockData)_actionData;
		ItemInventoryData invData = itemActionTextureBlockData.invData;
		if (this.bRemoveTexture)
		{
			itemActionTextureBlockData.idx = 0;
		}
		World world = GameManager.Instance.World;
		ChunkCluster chunkCluster = world.ChunkClusters[worldRayHitInfo.hit.clrIdx];
		if (chunkCluster == null)
		{
			yield break;
		}
		BlockToolSelection.Instance.BeginUndo(chunkCluster.ClusterIdx);
		if (!itemActionTextureBlockData.bReplacePaintNextTime)
		{
			switch (itemActionTextureBlockData.paintMode)
			{
			case ItemActionTextureBlock.EnumPaintMode.Single:
				this.paintBlock(world, chunkCluster, holdingEntity.entityId, itemActionTextureBlockData, blockPos, blockFaceFromHitInfo, blockValue, playerDataFromEntityID);
				break;
			case ItemActionTextureBlock.EnumPaintMode.Multiple:
			case ItemActionTextureBlock.EnumPaintMode.Spray:
			{
				float num = (itemActionTextureBlockData.paintMode == ItemActionTextureBlock.EnumPaintMode.Spray) ? 7.5f : 1.25f;
				if (worldRayHitInfo.hitTriangleIdx != -1)
				{
					Vector3 vector;
					Vector3 normalFromHitInfo = GameUtils.GetNormalFromHitInfo(blockPos, worldRayHitInfo.hitCollider, worldRayHitInfo.hitTriangleIdx, out vector);
					Vector3 normalized = normalFromHitInfo.normalized;
					Vector3 vector2;
					Vector3 vector3;
					if (Utils.FastAbs(normalized.x) >= Utils.FastAbs(normalized.y) && Utils.FastAbs(normalized.x) >= Utils.FastAbs(normalized.z))
					{
						vector2 = Vector3.up;
						vector3 = Vector3.forward;
					}
					else if (Utils.FastAbs(normalized.y) >= Utils.FastAbs(normalized.x) && Utils.FastAbs(normalized.y) >= Utils.FastAbs(normalized.z))
					{
						vector2 = Vector3.right;
						vector3 = Vector3.forward;
					}
					else
					{
						vector2 = Vector3.right;
						vector3 = Vector3.up;
					}
					vector = ItemActionTextureBlock.ProjectVectorOnPlane(normalized, vector2);
					vector2 = vector.normalized;
					vector = ItemActionTextureBlock.ProjectVectorOnPlane(normalized, vector3);
					vector3 = vector.normalized;
					Vector3 pos = worldRayHitInfo.hit.pos;
					Vector3 origin = worldRayHitInfo.ray.origin;
					for (float num2 = -num; num2 <= num; num2 += 0.5f)
					{
						for (float num3 = -num; num3 <= num; num3 += 0.5f)
						{
							direction = pos + num2 * vector2 + num3 * vector3 - origin;
							int hitMask = 69;
							if (Voxel.Raycast(world, new Ray(origin, direction), this.Range, -555528197, hitMask, 0f))
							{
								WorldRayHitInfo worldRayHitInfo2 = Voxel.voxelRayHitInfo.Clone();
								blockValue = worldRayHitInfo2.hit.blockValue;
								blockPos = worldRayHitInfo2.hit.blockPos;
								blockFaceFromHitInfo = GameUtils.GetBlockFaceFromHitInfo(blockPos, blockValue, worldRayHitInfo2.hitCollider, worldRayHitInfo2.hitTriangleIdx, out vector, out normalFromHitInfo);
								if (blockFaceFromHitInfo != BlockFace.None)
								{
									this.paintBlock(world, chunkCluster, holdingEntity.entityId, itemActionTextureBlockData, blockPos, blockFaceFromHitInfo, blockValue, playerDataFromEntityID);
								}
							}
						}
					}
				}
				break;
			}
			case ItemActionTextureBlock.EnumPaintMode.Fill:
			{
				Vector3 vector4;
				Vector3 vector = GameUtils.GetNormalFromHitInfo(blockPos, worldRayHitInfo.hitCollider, worldRayHitInfo.hitTriangleIdx, out vector4);
				Vector3 normalized2 = vector.normalized;
				Vector3 vector5;
				Vector3 vector6;
				if (Utils.FastAbs(normalized2.x) >= Utils.FastAbs(normalized2.y) && Utils.FastAbs(normalized2.x) >= Utils.FastAbs(normalized2.z))
				{
					vector5 = Vector3.up;
					vector6 = Vector3.forward;
				}
				else if (Utils.FastAbs(normalized2.y) >= Utils.FastAbs(normalized2.x) && Utils.FastAbs(normalized2.y) >= Utils.FastAbs(normalized2.z))
				{
					vector5 = Vector3.right;
					vector6 = Vector3.forward;
				}
				else
				{
					vector5 = Vector3.right;
					vector6 = Vector3.up;
				}
				vector = ItemActionTextureBlock.ProjectVectorOnPlane(normalized2, vector5);
				vector5 = vector.normalized * 0.3f;
				vector = ItemActionTextureBlock.ProjectVectorOnPlane(normalized2, vector6);
				vector6 = vector.normalized * 0.3f;
				int num4 = chunkCluster.GetBlockFaceTexture(blockPos, blockFaceFromHitInfo);
				if (itemActionTextureBlockData.idx == num4)
				{
					yield break;
				}
				if (num4 == 0)
				{
					string text;
					num4 = GameUtils.FindPaintIdForBlockFace(blockValue, blockFaceFromHitInfo, out text);
				}
				if (itemActionTextureBlockData.idx == num4)
				{
					yield break;
				}
				this.floodFill(world, chunkCluster, holdingEntity.entityId, itemActionTextureBlockData, playerDataFromEntityID, num4, worldRayHitInfo.hit.pos, normalized2, vector5, vector6);
				break;
			}
			}
			BlockToolSelection.Instance.EndUndo(chunkCluster.ClusterIdx, false);
			yield break;
		}
		itemActionTextureBlockData.bReplacePaintNextTime = false;
		if (!this.checkBlockCanBeChanged(world, blockPos, playerDataFromEntityID))
		{
			yield break;
		}
		int num5 = chunkCluster.GetBlockFaceTexture(blockPos, blockFaceFromHitInfo);
		if (itemActionTextureBlockData.idx == num5)
		{
			yield break;
		}
		if (num5 == 0)
		{
			string text;
			num5 = GameUtils.FindPaintIdForBlockFace(blockValue, blockFaceFromHitInfo, out text);
		}
		if (num5 != itemActionTextureBlockData.idx)
		{
			BlockToolSelection blockToolSelection = GameManager.Instance.GetActiveBlockTool() as BlockToolSelection;
			if (blockToolSelection == null || !blockToolSelection.SelectionActive)
			{
				this.replacePaintInCurrentPrefab(blockPos, blockFaceFromHitInfo, num5, itemActionTextureBlockData.idx);
			}
			else
			{
				this.replacePaintInCurrentSelection(blockPos, blockFaceFromHitInfo, num5, itemActionTextureBlockData.idx);
			}
		}
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int getHitBlockFace(ItemActionRanged.ItemActionDataRanged _actionData, out Vector3i blockPos, out BlockValue bv, out BlockFace blockFace, out WorldRayHitInfo hitInfo)
	{
		bv = BlockValue.Air;
		blockFace = BlockFace.None;
		hitInfo = null;
		blockPos = Vector3i.zero;
		hitInfo = this.GetExecuteActionTarget(_actionData);
		if (hitInfo == null || !hitInfo.bHitValid || hitInfo.tag == null || !GameUtils.IsBlockOrTerrain(hitInfo.tag))
		{
			return -1;
		}
		ChunkCluster chunkCluster = GameManager.Instance.World.ChunkClusters[hitInfo.hit.clrIdx];
		if (chunkCluster == null)
		{
			return -1;
		}
		bv = hitInfo.hit.blockValue;
		blockPos = hitInfo.hit.blockPos;
		Block block = bv.Block;
		if (bv.ischild)
		{
			blockPos = block.multiBlockPos.GetParentPos(blockPos, bv);
			bv = chunkCluster.GetBlock(blockPos);
		}
		if (bv.Block.MeshIndex != 0)
		{
			return -1;
		}
		blockFace = BlockFace.Top;
		if (bv.Block.shape is BlockShapeNew)
		{
			Vector3 vector;
			Vector3 vector2;
			blockFace = GameUtils.GetBlockFaceFromHitInfo(blockPos, bv, hitInfo.hitCollider, hitInfo.hitTriangleIdx, out vector, out vector2);
		}
		if (blockFace == BlockFace.None)
		{
			return -1;
		}
		return chunkCluster.GetBlockFaceTexture(blockPos, blockFace);
	}

	public void CopyTextureFromWorld(ItemActionRanged.ItemActionDataRanged _actionData)
	{
		if (!(_actionData.invData.holdingEntity is EntityPlayerLocal))
		{
			return;
		}
		Vector3i vector3i;
		BlockValue bv;
		BlockFace blockFace;
		WorldRayHitInfo worldRayHitInfo;
		int num = this.getHitBlockFace(_actionData, out vector3i, out bv, out blockFace, out worldRayHitInfo);
		if (num == -1)
		{
			return;
		}
		if (num == 0)
		{
			string text;
			num = GameUtils.FindPaintIdForBlockFace(bv, blockFace, out text);
		}
		ItemActionTextureBlock.ItemActionTextureBlockData itemActionTextureBlockData = (ItemActionTextureBlock.ItemActionTextureBlockData)_actionData;
		EntityPlayerLocal player = itemActionTextureBlockData.invData.holdingEntity as EntityPlayerLocal;
		BlockTextureData blockTextureData = BlockTextureData.list[num];
		if (blockTextureData != null && !blockTextureData.GetLocked(player))
		{
			itemActionTextureBlockData.idx = num;
			itemActionTextureBlockData.invData.itemValue.Meta = num;
			itemActionTextureBlockData.invData.itemValue = itemActionTextureBlockData.invData.itemValue;
			return;
		}
		Manager.PlayInsidePlayerHead("ui_denied", -1, 0f, false, false);
		GameManager.ShowTooltip(player, Localization.Get("ttPaintTextureIsLocked", false), false);
	}

	public void CopyBlockFromWorld(ItemActionRanged.ItemActionDataRanged _actionData)
	{
		if (!(_actionData.invData.holdingEntity is EntityPlayerLocal))
		{
			return;
		}
		WorldRayHitInfo executeActionTarget = this.GetExecuteActionTarget(_actionData);
		if (executeActionTarget == null || !executeActionTarget.bHitValid || executeActionTarget.tag == null || !GameUtils.IsBlockOrTerrain(executeActionTarget.tag))
		{
			return;
		}
		ChunkCluster chunkCluster = GameManager.Instance.World.ChunkClusters[executeActionTarget.hit.clrIdx];
		if (chunkCluster == null)
		{
			return;
		}
		BlockValue blockValue = executeActionTarget.hit.blockValue;
		Vector3i vector3i = executeActionTarget.hit.blockPos;
		Block block = blockValue.Block;
		if (blockValue.ischild)
		{
			vector3i = block.multiBlockPos.GetParentPos(vector3i, blockValue);
			blockValue = chunkCluster.GetBlock(vector3i);
		}
		if (blockValue.Block.MeshIndex != 0)
		{
			return;
		}
		ItemValue itemValue = executeActionTarget.hit.blockValue.ToItemValue();
		itemValue.Texture = chunkCluster.GetTextureFull(vector3i);
		ItemStack itemStack = new ItemStack(itemValue, 99);
		_actionData.invData.holdingEntity.inventory.AddItem(itemStack);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void onHoldingEntityFired(ItemActionData _actionData)
	{
		if (_actionData.indexInEntityOfAction == 0)
		{
			_actionData.invData.holdingEntity.RightArmAnimationUse = true;
			return;
		}
		_actionData.invData.holdingEntity.RightArmAnimationAttack = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void replacePaintInCurrentPrefab(Vector3i _blockPos, BlockFace _blockFace, int _searchPaintId, int _replacePaintId)
	{
		World world = GameManager.Instance.World;
		DynamicPrefabDecorator dynamicPrefabDecorator = world.ChunkClusters[0].ChunkProvider.GetDynamicPrefabDecorator();
		if (dynamicPrefabDecorator == null)
		{
			return;
		}
		PrefabInstance prefabInstance = GameUtils.FindPrefabForBlockPos(dynamicPrefabDecorator.GetDynamicPrefabs(), _blockPos);
		if (prefabInstance == null)
		{
			return;
		}
		for (int i = prefabInstance.boundingBoxPosition.x; i <= prefabInstance.boundingBoxPosition.x + prefabInstance.boundingBoxSize.x; i++)
		{
			for (int j = prefabInstance.boundingBoxPosition.z; j <= prefabInstance.boundingBoxPosition.z + prefabInstance.boundingBoxSize.z; j++)
			{
				for (int k = 0; k < 256; k++)
				{
					BlockValue block = world.GetBlock(i, k, j);
					if (!block.isair)
					{
						long num = world.GetTexture(i, k, j);
						bool flag = false;
						for (int l = 0; l < 6; l++)
						{
							int num2 = (int)(num >> l * 8 & 255L);
							if (num2 == 0)
							{
								string text;
								num2 = GameUtils.FindPaintIdForBlockFace(block, (BlockFace)l, out text);
							}
							if (num2 == _searchPaintId)
							{
								num &= ~(255L << l * 8);
								num |= (long)_replacePaintId << l * 8;
								flag = true;
							}
						}
						if (flag)
						{
							world.SetTexture(0, i, k, j, num);
						}
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void replacePaintInCurrentSelection(Vector3i _blockPos, BlockFace _blockFace, int _searchPaintId, int _replacePaintId)
	{
		BlockToolSelection blockToolSelection = GameManager.Instance.GetActiveBlockTool() as BlockToolSelection;
		if (blockToolSelection == null)
		{
			return;
		}
		World world = GameManager.Instance.World;
		Vector3i selectionMin = blockToolSelection.SelectionMin;
		for (int i = selectionMin.x; i < selectionMin.x + blockToolSelection.SelectionSize.x; i++)
		{
			for (int j = selectionMin.z; j < selectionMin.z + blockToolSelection.SelectionSize.z; j++)
			{
				for (int k = selectionMin.y; k < selectionMin.y + blockToolSelection.SelectionSize.y; k++)
				{
					BlockValue block = world.GetBlock(i, k, j);
					if (!block.isair)
					{
						long num = world.GetTexture(i, k, j);
						bool flag = false;
						for (int l = 0; l < 6; l++)
						{
							int num2 = (int)(num >> l * 8 & 255L);
							if (num2 == 0)
							{
								string text;
								num2 = GameUtils.FindPaintIdForBlockFace(block, (BlockFace)l, out text);
							}
							if (num2 == _searchPaintId)
							{
								num &= ~(255L << l * 8);
								num |= (long)_replacePaintId << l * 8;
								flag = true;
							}
						}
						if (flag)
						{
							world.SetTexture(0, i, k, j, num);
						}
					}
				}
			}
		}
	}

	public override EnumCameraShake GetCameraShakeType(ItemActionData _actionData)
	{
		return EnumCameraShake.None;
	}

	public override bool ShowAmmoInUI()
	{
		return true;
	}

	public override void SetupRadial(XUiC_Radial _xuiRadialWindow, EntityPlayerLocal _epl)
	{
		ItemActionTextureBlock.ItemActionTextureBlockData itemActionTextureBlockData = (ItemActionTextureBlock.ItemActionTextureBlockData)_epl.inventory.holdingItemData.actionData[1];
		_xuiRadialWindow.ResetRadialEntries();
		object obj = GameStats.GetBool(EnumGameStats.IsCreativeMenuEnabled) || GamePrefs.GetBool(EnumGamePrefs.CreativeMenuEnabled);
		_xuiRadialWindow.CreateRadialEntry(0, "ui_game_symbol_paint_bucket", "UIAtlas", "", Localization.Get("xuiMaterials", false), false);
		_xuiRadialWindow.CreateRadialEntry(1, "ui_game_symbol_paint_brush", "UIAtlas", "", Localization.Get("xuiPaintBrush", false), itemActionTextureBlockData.paintMode == ItemActionTextureBlock.EnumPaintMode.Single);
		_xuiRadialWindow.CreateRadialEntry(2, "ui_game_symbol_paint_roller", "UIAtlas", "", Localization.Get("xuiPaintRoller", false), itemActionTextureBlockData.paintMode == ItemActionTextureBlock.EnumPaintMode.Multiple);
		_xuiRadialWindow.CreateRadialEntry(8, "ui_game_symbol_flood_fill", "UIAtlas", "", Localization.Get("xuiPaintFill", false), itemActionTextureBlockData.paintMode == ItemActionTextureBlock.EnumPaintMode.Fill);
		object obj2 = obj;
		if (obj2 != null)
		{
			_xuiRadialWindow.CreateRadialEntry(3, "ui_game_symbol_paint_spraygun", "UIAtlas", "", Localization.Get("xuiSprayGun", false), itemActionTextureBlockData.paintMode == ItemActionTextureBlock.EnumPaintMode.Spray);
		}
		_xuiRadialWindow.CreateRadialEntry(4, "ui_game_symbol_paint_allsides", "UIAtlas", "", Localization.Get("xuiPaintAllSides", false), itemActionTextureBlockData.bPaintAllSides);
		_xuiRadialWindow.CreateRadialEntry(5, "ui_game_symbol_paint_eyedropper", "UIAtlas", "", Localization.Get("xuiTexturePicker", false), false);
		if (obj2 != null)
		{
			_xuiRadialWindow.CreateRadialEntry(6, "ui_game_symbol_paint_copy_block", "UIAtlas", "", Localization.Get("xuiCopyBlock", false), false);
			_xuiRadialWindow.CreateRadialEntry(7, "ui_game_symbol_book", "UIAtlas", "", Localization.Get("xuiReplacePaint", false), itemActionTextureBlockData.bReplacePaintNextTime);
		}
		_xuiRadialWindow.SetCommonData(UIUtils.ButtonIcon.FaceButtonNorth, new XUiC_Radial.CommandHandlerDelegate(this.handleRadialCommand), new XUiC_Radial.RadialContextHoldingSlotIndex(_epl.inventory.holdingItemIdx), -1, false, new XUiC_Radial.RadialStillValidDelegate(XUiC_Radial.RadialValidSameHoldingSlotIndex));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public new void handleRadialCommand(XUiC_Radial _sender, int _commandIndex, XUiC_Radial.RadialContextAbs _context)
	{
		EntityPlayerLocal entityPlayer = _sender.xui.playerUI.entityPlayer;
		ItemClass holdingItem = entityPlayer.inventory.holdingItem;
		ItemInventoryData holdingItemData = entityPlayer.inventory.holdingItemData;
		if (!(holdingItem.Actions[0] is ItemActionTextureBlock) || !(holdingItem.Actions[1] is ItemActionTextureBlock))
		{
			return;
		}
		ItemActionTextureBlock itemActionTextureBlock = (ItemActionTextureBlock)holdingItem.Actions[0];
		ItemActionTextureBlock itemActionTextureBlock2 = (ItemActionTextureBlock)holdingItem.Actions[1];
		ItemActionTextureBlock.ItemActionTextureBlockData itemActionTextureBlockData = (ItemActionTextureBlock.ItemActionTextureBlockData)holdingItemData.actionData[0];
		ItemActionTextureBlock.ItemActionTextureBlockData itemActionTextureBlockData2 = (ItemActionTextureBlock.ItemActionTextureBlockData)holdingItemData.actionData[1];
		if (_commandIndex != 0 && _commandIndex != 7)
		{
			itemActionTextureBlockData2.bReplacePaintNextTime = false;
		}
		switch (_commandIndex)
		{
		case 0:
			_sender.xui.playerUI.windowManager.Open("materials", true, false, true);
			return;
		case 1:
			itemActionTextureBlockData.paintMode = (itemActionTextureBlockData2.paintMode = ItemActionTextureBlock.EnumPaintMode.Single);
			return;
		case 2:
			itemActionTextureBlockData.paintMode = (itemActionTextureBlockData2.paintMode = ItemActionTextureBlock.EnumPaintMode.Multiple);
			return;
		case 3:
			itemActionTextureBlockData.paintMode = (itemActionTextureBlockData2.paintMode = ItemActionTextureBlock.EnumPaintMode.Spray);
			return;
		case 4:
			itemActionTextureBlockData.bPaintAllSides = (itemActionTextureBlockData2.bPaintAllSides = !itemActionTextureBlockData2.bPaintAllSides);
			return;
		case 5:
			itemActionTextureBlock.CopyTextureFromWorld(itemActionTextureBlockData);
			itemActionTextureBlock2.CopyTextureFromWorld(itemActionTextureBlockData2);
			return;
		case 6:
			itemActionTextureBlock.CopyBlockFromWorld(itemActionTextureBlockData);
			itemActionTextureBlock2.CopyBlockFromWorld(itemActionTextureBlockData2);
			return;
		case 7:
			itemActionTextureBlockData2.bReplacePaintNextTime = !itemActionTextureBlockData2.bReplacePaintNextTime;
			return;
		case 8:
			itemActionTextureBlockData.paintMode = (itemActionTextureBlockData2.paintMode = ItemActionTextureBlock.EnumPaintMode.Fill);
			return;
		default:
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float rayCastDelay;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bRemoveTexture;

	public int DefaultTextureID = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<Vector3i, bool> visitedPositions = new Dictionary<Vector3i, bool>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<Vector2i, bool> visitedRays = new Dictionary<Vector2i, bool>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Stack<Vector2i> positionsToCheck = new Stack<Vector2i>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly WorldRayHitInfo worldRayHitInfo = new WorldRayHitInfo();

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemValue currentMagazineItem;

	public enum EnumPaintMode
	{
		Single,
		Multiple,
		Spray,
		Fill
	}

	public class ItemActionTextureBlockData : ItemActionRanged.ItemActionDataRanged
	{
		public ItemActionTextureBlockData(ItemInventoryData _invData, int _indexInEntityOfAction, string _particleTransform) : base(_invData, _indexInEntityOfAction)
		{
		}

		public int idx = 1;

		public ItemActionTextureBlock.EnumPaintMode paintMode;

		public bool bReplacePaintNextTime;

		public bool bPaintAllSides;

		public float lastTimeReplacePaintShown;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public enum EPaintResult
	{
		CanNotPaint,
		Painted,
		SamePaint,
		NoPaintAvailable
	}
}

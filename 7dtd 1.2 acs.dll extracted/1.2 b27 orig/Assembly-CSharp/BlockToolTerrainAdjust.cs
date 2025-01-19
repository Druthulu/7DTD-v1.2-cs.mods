﻿using System;
using System.Collections.Generic;
using UnityEngine;

public class BlockToolTerrainAdjust : IBlockTool
{
	public BlockToolTerrainAdjust(BlockTools.Brush _brush, NguiWdwTerrainEditor _parentWindow)
	{
		this.brush = _brush;
		this.parentWindow = _parentWindow;
		this.isButtonDown = false;
	}

	public void CheckSpecialKeys(Event ev, PlayerActionsLocal playerActions)
	{
	}

	public void CheckKeys(ItemInventoryData _data, WorldRayHitInfo _hitInfo, PlayerActionsLocal playerActions)
	{
		if (_data == null || _data.actionData == null || _data.actionData[0] == null)
		{
			return;
		}
		this.parentWindow.lastPosition = new Vector3i(_hitInfo.hit.pos);
		this.parentWindow.lastDirection = _hitInfo.ray.direction;
		if (Input.GetMouseButton(0) && this.isButtonDown)
		{
			if (Time.time - _data.actionData[0].lastUseTime < 0.1f)
			{
				return;
			}
			_data.actionData[0].lastUseTime = Time.time;
			if (!_data.hitInfo.bHitValid || !GameUtils.IsBlockOrTerrain(_data.hitInfo.tag))
			{
				return;
			}
			this.blockChanges.Clear();
			BlockTools.PlaceTerrain(_data.world, _data.hitInfo.hit.clrIdx, _data.hitInfo.hit.blockPos + new Vector3i(this.parentWindow.lastDirection.x * 2f, this.parentWindow.lastDirection.y * 2f, this.parentWindow.lastDirection.z * 2f), this.brush, this.blockChanges);
			_data.world.SetBlocksRPC(this.blockChanges);
			return;
		}
		else
		{
			if (!Input.GetMouseButton(1) || !this.isButtonDown)
			{
				if (!Input.GetMouseButton(0) && !Input.GetMouseButton(1) && this.isButtonDown)
				{
					this.isButtonDown = false;
				}
				return;
			}
			if (Time.time - _data.actionData[0].lastUseTime < 0.1f)
			{
				return;
			}
			_data.actionData[0].lastUseTime = Time.time;
			if (!_data.hitInfo.bHitValid || !GameUtils.IsBlockOrTerrain(_data.hitInfo.tag))
			{
				return;
			}
			this.blockChanges.Clear();
			BlockTools.RemoveTerrain(_data.world, _data.hitInfo.hit.clrIdx, _data.hitInfo.hit.blockPos, this.brush, this.blockChanges);
			_data.world.SetBlocksRPC(this.blockChanges);
			return;
		}
	}

	public bool ConsumeScrollWheel(ItemInventoryData _data, float _scrollWheelInput, PlayerActionsLocal _playerInput)
	{
		return false;
	}

	public string GetDebugOutput()
	{
		return "";
	}

	public virtual bool ExecuteAttackAction(ItemInventoryData _data, bool _bReleased, PlayerActionsLocal playerActions)
	{
		this.isButtonDown = true;
		this.lastData = _data;
		return true;
	}

	public bool ExecuteUseAction(ItemInventoryData _data, bool _bReleased, PlayerActionsLocal playerActions)
	{
		this.isButtonDown = true;
		this.lastData = _data;
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public sbyte[] GetLocalDensityMap(WorldBase _world, Vector3i blockTargetPos, Vector3i[] localArea)
	{
		sbyte[] array = new sbyte[localArea.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = _world.GetDensity(0, localArea[i].x, localArea[i].y, localArea[i].z);
		}
		return array;
	}

	public override string ToString()
	{
		return "Dig/Place Terrain";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockTools.Brush brush;

	[PublicizedFrom(EAccessModifier.Private)]
	public NguiWdwTerrainEditor parentWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemInventoryData lastData;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isButtonDown;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<BlockChangeInfo> blockChanges = new List<BlockChangeInfo>();
}

﻿using System;
using System.Collections.Generic;
using System.Globalization;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionOpenBundle : ItemAction
{
	public override ItemActionData CreateModifierData(ItemInventoryData _invData, int _indexInEntityOfAction)
	{
		return new ItemActionOpenBundle.MyInventoryData(_invData, _indexInEntityOfAction);
	}

	public override void ReadFrom(DynamicProperties _props)
	{
		base.ReadFrom(_props);
		if (_props.Values.ContainsKey("Consume"))
		{
			this.Consume = StringParsers.ParseBool(_props.Values["Consume"], 0, -1, true);
		}
		else
		{
			this.Consume = true;
		}
		if (_props.Values.ContainsKey("Create_item"))
		{
			this.CreateItem = _props.Values["Create_item"].Replace(" ", "").Split(',', StringSplitOptions.None);
			if (_props.Values.ContainsKey("Create_item_count"))
			{
				this.CreateItemCount = _props.Values["Create_item_count"].Replace(" ", "").Split(',', StringSplitOptions.None);
			}
			else
			{
				this.CreateItemCount = new string[0];
			}
		}
		else
		{
			this.CreateItem = null;
			this.CreateItemCount = null;
		}
		if (_props.Values.ContainsKey("Random_item"))
		{
			this.RandomItem = _props.Values["Random_item"].Replace(" ", "").Split(',', StringSplitOptions.None);
			if (_props.Values.ContainsKey("Random_item_count"))
			{
				this.RandomItemCount = _props.Values["Random_item_count"].Replace(" ", "").Split(',', StringSplitOptions.None);
			}
			else
			{
				this.RandomItemCount = new string[0];
			}
		}
		else
		{
			this.RandomItem = null;
			this.RandomItemCount = null;
		}
		if (_props.Values.ContainsKey("Random_count"))
		{
			this.RandomCount = StringParsers.ParseSInt32(_props.Values["Random_count"], 0, -1, NumberStyles.Integer);
		}
		_props.ParseBool("Unique_random_only", ref this.UniqueRandomOnly);
		if (_props.Values.ContainsKey("Condition_raycast_block"))
		{
			string[] array = _props.Values["Condition_raycast_block"].Trim().Split(',', StringSplitOptions.None);
			for (int i = 0; i < array.Length; i++)
			{
				int item = int.Parse(array[i].Trim());
				if (this.ConditionBlockTypes == null)
				{
					this.ConditionBlockTypes = new HashSet<int>();
				}
				this.ConditionBlockTypes.Add(item);
			}
			if (this.ConditionBlockTypes != null && this.ConditionBlockTypes.Count == 0)
			{
				this.ConditionBlockTypes = null;
			}
		}
		this.UseAnimation = false;
	}

	public override void StopHolding(ItemActionData _data)
	{
		((ItemActionOpenBundle.MyInventoryData)_data).bEatingStarted = false;
	}

	public override void ExecuteAction(ItemActionData _actionData, bool _bReleased)
	{
		ItemActionOpenBundle.MyInventoryData myInventoryData = (ItemActionOpenBundle.MyInventoryData)_actionData;
		if (!_bReleased)
		{
			return;
		}
		if (Time.time - _actionData.lastUseTime < this.Delay)
		{
			return;
		}
		if (this.IsActionRunning(_actionData))
		{
			return;
		}
		EntityAlive holdingEntity = myInventoryData.invData.holdingEntity;
		if (EffectManager.GetValue(PassiveEffects.DisableItem, holdingEntity.inventory.holdingItemItemValue, 0f, holdingEntity, null, _actionData.invData.item.ItemTags, true, true, true, true, true, 1, true, false) > 0f)
		{
			_actionData.lastUseTime = Time.time + 1f;
			Manager.PlayInsidePlayerHead("twitch_no_attack", -1, 0f, false, false);
			return;
		}
		BlockValue blockValue = BlockValue.Air;
		if (this.ConditionBlockTypes != null)
		{
			Ray lookRay = holdingEntity.GetLookRay();
			int modelLayer = holdingEntity.GetModelLayer();
			holdingEntity.SetModelLayer(2, false, null);
			Voxel.Raycast(myInventoryData.invData.world, lookRay, 2.5f, 131, (holdingEntity is EntityPlayer) ? 0.2f : 0.4f);
			holdingEntity.SetModelLayer(modelLayer, false, null);
			WorldRayHitInfo voxelRayHitInfo = Voxel.voxelRayHitInfo;
			if (!GameUtils.IsBlockOrTerrain(voxelRayHitInfo.tag))
			{
				return;
			}
			HitInfoDetails hit = voxelRayHitInfo.hit;
			blockValue = voxelRayHitInfo.hit.blockValue;
			if (blockValue.isair || !this.ConditionBlockTypes.Contains(blockValue.type))
			{
				lookRay = myInventoryData.invData.holdingEntity.GetLookRay();
				lookRay.origin += lookRay.direction.normalized * 0.5f;
				if (!Voxel.Raycast(myInventoryData.invData.world, lookRay, 2.5f, -538480645, 4095, 0f))
				{
					return;
				}
				HitInfoDetails hit2 = voxelRayHitInfo.hit;
				blockValue = voxelRayHitInfo.hit.blockValue;
				if (blockValue.isair || !this.ConditionBlockTypes.Contains(blockValue.type))
				{
					return;
				}
			}
		}
		_actionData.lastUseTime = Time.time;
		this.ExecuteInstantAction(myInventoryData.invData.holdingEntity, myInventoryData.invData.itemStack, true, null);
	}

	public override bool ExecuteInstantAction(EntityAlive ent, ItemStack stack, bool isHeldItem, XUiC_ItemStack stackController)
	{
		ent.MinEventContext.ItemValue = stack.itemValue;
		ent.MinEventContext.ItemValue.FireEvent(MinEventTypes.onSelfPrimaryActionStart, ent.MinEventContext);
		ent.FireEvent(MinEventTypes.onSelfPrimaryActionStart, false);
		if (this.soundStart != null)
		{
			ent.PlayOneShot(this.soundStart, false, false, false);
		}
		if (this.Consume)
		{
			if (stack.itemValue.MaxUseTimes > 0 && stack.itemValue.UseTimes + 1f < (float)stack.itemValue.MaxUseTimes)
			{
				stack.itemValue.UseTimes += EffectManager.GetValue(PassiveEffects.DegradationPerUse, stack.itemValue, 1f, ent, null, stack.itemValue.ItemClass.ItemTags, true, true, true, true, true, 1, true, false);
				return true;
			}
			if (isHeldItem)
			{
				ent.inventory.DecHoldingItem(1);
			}
			else
			{
				stack.count--;
			}
		}
		ent.MinEventContext.ItemValue = stack.itemValue;
		ent.MinEventContext.ItemValue.FireEvent(MinEventTypes.onSelfPrimaryActionEnd, ent.MinEventContext);
		ent.FireEvent(MinEventTypes.onSelfPrimaryActionEnd, false);
		if (this.CreateItem != null && this.CreateItemCount != null)
		{
			for (int i = 0; i < this.CreateItem.Length; i++)
			{
				string text = (this.CreateItemCount != null && this.CreateItemCount.Length > i) ? this.CreateItemCount[i] : "1";
				ItemClass itemClass = ItemClass.GetItemClass(this.CreateItem[i], false);
				int num;
				if (text.Contains("-"))
				{
					string[] array = text.Split('-', StringSplitOptions.None);
					int min = StringParsers.ParseSInt32(array[0], 0, -1, NumberStyles.Integer);
					int maxExclusive = StringParsers.ParseSInt32(array[1], 0, -1, NumberStyles.Integer) + 1;
					num = ent.rand.RandomRange(min, maxExclusive);
				}
				else
				{
					num = StringParsers.ParseSInt32(text, 0, -1, NumberStyles.Integer);
				}
				ItemValue itemValue;
				if (itemClass.HasQuality)
				{
					itemValue = new ItemValue(itemClass.Id, num, num, false, null, 1f);
					num = 1;
				}
				else
				{
					itemValue = new ItemValue(itemClass.Id, false);
				}
				ItemStack itemStack = new ItemStack(itemValue, num);
				if (!LocalPlayerUI.GetUIForPlayer(ent as EntityPlayerLocal).xui.PlayerInventory.AddItem(itemStack))
				{
					ent.world.gameManager.ItemDropServer(itemStack, ent.GetPosition(), Vector3.zero, -1, 60f, false);
				}
			}
		}
		if (this.RandomItem != null && this.RandomItemCount != null)
		{
			List<int> list = null;
			if (this.UniqueRandomOnly)
			{
				list = new List<int>();
				for (int j = 0; j < this.RandomItem.Length; j++)
				{
					list.Add(j);
				}
				for (int k = 0; k < list.Count * 3; k++)
				{
					int num2 = ent.rand.RandomRange(0, list.Count);
					int num3 = ent.rand.RandomRange(0, list.Count);
					if (num2 != num3)
					{
						int value = list[num2];
						list[num2] = list[num3];
						list[num3] = value;
					}
				}
			}
			int num4 = -1;
			for (int l = 0; l < this.RandomCount; l++)
			{
				int num5;
				if (this.UniqueRandomOnly)
				{
					num4++;
					if (num4 >= this.RandomItem.Length)
					{
						num4 = 0;
					}
					num5 = list[num4];
				}
				else
				{
					num5 = ent.rand.RandomRange(0, this.RandomItem.Length);
				}
				string text2 = (this.RandomItemCount != null && this.RandomItemCount.Length > num5) ? this.RandomItemCount[num5] : "1";
				ItemClass itemClass2;
				for (;;)
				{
					itemClass2 = ItemClass.GetItemClass(this.RandomItem[num5], false);
					if (itemClass2 != null)
					{
						break;
					}
					num5++;
					if (num5 > this.RandomItem.Length)
					{
						num5 = 0;
					}
				}
				int m;
				if (text2.Contains("-"))
				{
					string[] array2 = text2.Split('-', StringSplitOptions.None);
					int min2 = StringParsers.ParseSInt32(array2[0], 0, -1, NumberStyles.Integer);
					int maxExclusive2 = StringParsers.ParseSInt32(array2[1], 0, -1, NumberStyles.Integer) + 1;
					m = ent.rand.RandomRange(min2, maxExclusive2);
				}
				else
				{
					m = StringParsers.ParseSInt32(text2, 0, -1, NumberStyles.Integer);
				}
				ItemValue itemValue2;
				if (itemClass2.HasQuality)
				{
					itemValue2 = new ItemValue(itemClass2.Id, m, m, false, null, 1f);
					m = 1;
				}
				else
				{
					itemValue2 = new ItemValue(itemClass2.Id, false);
				}
				XUiM_PlayerInventory playerInventory = LocalPlayerUI.GetUIForPlayer(ent as EntityPlayerLocal).xui.PlayerInventory;
				while (m > 0)
				{
					ItemStack itemStack2 = new ItemStack(itemValue2, m);
					m -= itemStack2.count;
					if (!playerInventory.AddItem(itemStack2))
					{
						ent.world.gameManager.ItemDropServer(itemStack2, ent.GetPosition(), Vector3.zero, -1, 60f, false);
					}
				}
			}
		}
		return true;
	}

	public new string[] CreateItem;

	public string[] CreateItemCount;

	public new bool Consume;

	public HashSet<int> ConditionBlockTypes;

	public bool UniqueRandomOnly;

	public string[] RandomItem;

	public string[] RandomItemCount;

	public int RandomCount = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public class MyInventoryData : ItemActionAttackData
	{
		public MyInventoryData(ItemInventoryData _invData, int _indexInEntityOfAction) : base(_invData, _indexInEntityOfAction)
		{
		}

		public bool bEatingStarted;
	}
}

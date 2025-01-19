﻿using System;
using System.Collections.Generic;

public class TraderManager
{
	public TraderManager(WorldBase _world)
	{
		this.world = _world;
		this.Random = GameRandomManager.Instance.CreateGameRandom((int)DateTime.Now.Ticks);
	}

	public bool TraderInventoryRequested(TraderData trader, int _entityIdThatOpenedIt)
	{
		TraderInfo traderInfo = TraderInfo.traderInfoList[trader.TraderID];
		if (traderInfo == null)
		{
			return false;
		}
		if (traderInfo.ResetInterval < 1)
		{
			return false;
		}
		ulong worldTime = this.world.GetWorldTime();
		if (worldTime < trader.lastInventoryUpdate)
		{
			trader.lastInventoryUpdate = 1UL;
		}
		if ((int)(worldTime - trader.lastInventoryUpdate) < traderInfo.ResetIntervalInTicks && trader.lastInventoryUpdate > 0UL)
		{
			return false;
		}
		ulong num = (ulong)((long)traderInfo.ResetIntervalInTicks);
		trader.lastInventoryUpdate = worldTime / num * num + 1UL;
		this.HandleFullReset(trader, traderInfo);
		trader.TierItemGroups.Clear();
		for (int i = 0; i < traderInfo.TierItemGroups.Count; i++)
		{
			List<ItemStack> list = traderInfo.SpawnTierGroup(this.Random, i);
			trader.TierItemGroups.Add(list.ToArray());
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleFullReset(TraderData trader, TraderInfo info)
	{
		trader.PrimaryInventory.Clear();
		if (info.ResetInterval != -1)
		{
			List<ItemStack> list = info.Spawn(this.Random);
			for (int i = 0; i < list.Count; i++)
			{
				ItemStack itemStack = list[i];
				ItemClass forId = ItemClass.GetForId(itemStack.itemValue.type);
				if (!forId.HasQuality)
				{
					for (int j = 0; j < trader.PrimaryInventory.Count; j++)
					{
						ItemStack itemStack2 = trader.PrimaryInventory[j];
						if (itemStack2.itemValue.type == itemStack.itemValue.type && itemStack2.count < forId.Stacknumber.Value)
						{
							int num = Math.Min(forId.Stacknumber.Value - itemStack2.count, itemStack.count);
							itemStack2.count += num;
							itemStack.count -= num;
							if (itemStack.count == 0)
							{
								list[i] = ItemStack.Empty.Clone();
							}
						}
					}
				}
				if (!list[i].IsEmpty())
				{
					trader.PrimaryInventory.Add(list[i]);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public WorldBase world;

	public GameRandom Random;

	public static float[] QuestTierMod;

	public static Dictionary<string, TraderStageTemplateGroup> TraderStageTemplates = new Dictionary<string, TraderStageTemplateGroup>();
}

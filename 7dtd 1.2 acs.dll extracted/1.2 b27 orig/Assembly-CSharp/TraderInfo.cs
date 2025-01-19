using System;
using System.Collections.Generic;
using UnityEngine;

public class TraderInfo
{
	public static float BuyMarkup
	{
		get
		{
			return TraderInfo.buyMarkup;
		}
		set
		{
			TraderInfo.buyMarkup = value;
		}
	}

	public static float SellMarkdown
	{
		get
		{
			return TraderInfo.sellMarkdown;
		}
		set
		{
			TraderInfo.sellMarkdown = value;
		}
	}

	public static float QualityMinMod
	{
		get
		{
			return TraderInfo.qualityMinMod;
		}
		set
		{
			TraderInfo.qualityMinMod = value;
		}
	}

	public static float QualityMaxMod
	{
		get
		{
			return TraderInfo.qualityMaxMod;
		}
		set
		{
			TraderInfo.qualityMaxMod = value;
		}
	}

	public static string CurrencyItem { get; set; }

	public int RentTimeInSeconds
	{
		get
		{
			return this.RentTimeInDays * 60 * GamePrefs.GetInt(EnumGamePrefs.DayNightLength);
		}
	}

	public int RentTimeInTicks
	{
		get
		{
			return this.RentTimeInDays * 24000;
		}
	}

	public bool IsOpen
	{
		get
		{
			if (!this.UseOpenHours)
			{
				return true;
			}
			ulong num = GameManager.Instance.World.worldTime % 24000UL;
			if (this.OpenTime < this.CloseTime)
			{
				return this.OpenTime < num && num < this.CloseTime;
			}
			return num > this.OpenTime || num < this.CloseTime;
		}
	}

	public bool ShouldPlayOpenSound
	{
		get
		{
			ulong num = GameManager.Instance.World.worldTime % 24000UL;
			return num > this.OpenTime && num < this.OpenTime + 100UL;
		}
	}

	public bool ShouldPlayCloseSound
	{
		get
		{
			ulong num = GameManager.Instance.World.worldTime % 24000UL;
			return num > this.CloseTime && num < this.CloseTime + 100UL;
		}
	}

	public bool IsWarningTime
	{
		get
		{
			if (!this.UseOpenHours)
			{
				return false;
			}
			ulong num = GameManager.Instance.World.worldTime % 24000UL;
			if (this.OpenTime < this.WarningTime)
			{
				return this.WarningTime < num && num < this.WarningTime + 100UL;
			}
			return this.WarningTime > this.OpenTime || num < this.WarningTime + 100UL;
		}
	}

	public static void InitStatic()
	{
		TraderInfo.traderInfoList = new TraderInfo[256];
		TraderInfo.traderItemGroups = new Dictionary<string, TraderInfo.TraderItemGroup>();
	}

	public void Init()
	{
		TraderInfo.traderInfoList[this.Id] = this;
	}

	public static void Cleanup()
	{
		TraderInfo.traderInfoList = null;
		TraderInfo.traderItemGroups = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void applyRandomDegradation(ref ItemValue _itemValue)
	{
		_itemValue.Meta = ItemClass.GetForId(_itemValue.type).GetInitialMetadata(_itemValue);
		int maxUseTimes = _itemValue.MaxUseTimes;
		if (maxUseTimes == 0)
		{
			return;
		}
		float num = GameManager.Instance.World.GetGameRandom().RandomFloat * 0.6f + 0.2f;
		_itemValue.UseTimes = (float)((int)((float)maxUseTimes * num));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void applyQuality(ref ItemValue _itemValue, int minQuality = 1, int maxQuality = 6)
	{
		if (ItemClass.list[_itemValue.type].HasQuality || ItemClass.list[_itemValue.type].HasSubItems)
		{
			_itemValue = new ItemValue(_itemValue.type, Mathf.Clamp(minQuality, 1, maxQuality), maxQuality, false, null, 1f);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SpawnItem(TraderInfo.TraderItemEntry template, ItemValue item, int countToSpawn, List<ItemStack> spawnedItems)
	{
		if (countToSpawn < 1)
		{
			return;
		}
		if (item.ItemClass == null)
		{
			return;
		}
		ItemClass itemClass = item.ItemClass;
		countToSpawn = Math.Min(countToSpawn, itemClass.Stacknumber.Value);
		int num = itemClass.IsBlock() ? Block.list[item.type].EconomicBundleSize : itemClass.EconomicBundleSize;
		if (itemClass.EconomicValue == -1f)
		{
			return;
		}
		if (num > 1)
		{
			int num2 = countToSpawn % num;
			if (num2 > 0)
			{
				countToSpawn -= num2;
			}
			if (countToSpawn == 0)
			{
				countToSpawn = num;
			}
		}
		if (itemClass.CanStack())
		{
			int value = ItemClass.GetForId(item.type).Stacknumber.Value;
			for (int i = 0; i < spawnedItems.Count; i++)
			{
				ItemStack itemStack = spawnedItems[i];
				if (itemStack.itemValue.type == item.type)
				{
					if (itemStack.CanStack(countToSpawn))
					{
						itemStack.count += countToSpawn;
						spawnedItems[i] = itemStack;
						return;
					}
					int num3 = value - itemStack.count;
					itemStack.count = value;
					spawnedItems[i] = itemStack;
					countToSpawn -= num3;
				}
			}
		}
		int num4 = template.minQuality;
		int maxQuality = template.maxQuality;
		ItemValue itemValue = item.Clone();
		if (item.HasQuality)
		{
			if (num4 <= -1)
			{
				num4 = 1;
				maxQuality = 6;
			}
			if (template != null && template.parentGroup != null && template.parentGroup.modsToInstall.Length != 0)
			{
				itemValue = new ItemValue(item.type, num4, maxQuality, true, template.parentGroup.modsToInstall, template.parentGroup.modChance);
			}
			else
			{
				itemValue = new ItemValue(item.type, num4, maxQuality, true, template.modsToInstall, template.modChance);
			}
		}
		else
		{
			itemValue = new ItemValue(item.type, true);
		}
		if (itemValue.ItemClass != null && itemValue.ItemClass.Actions != null && itemValue.ItemClass.Actions.Length != 0 && itemValue.ItemClass.Actions[0] != null)
		{
			itemValue.Meta = 0;
		}
		ItemStack item2 = new ItemStack(itemValue, countToSpawn);
		spawnedItems.Add(item2);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int RandomSpawnCount(GameRandom random, int min, int max)
	{
		if (min < 0)
		{
			return -1;
		}
		float num = random.RandomRange((float)min - 0.49f, (float)max + 0.49f);
		if (num <= (float)min)
		{
			return min;
		}
		if (num > (float)max)
		{
			num = (float)max;
		}
		int num2 = (int)num;
		float num3 = num - (float)num2;
		if (random.RandomFloat < num3)
		{
			num2++;
		}
		return num2;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SpawnAllItemsFromList(GameRandom random, List<TraderInfo.TraderItemEntry> itemSet, List<ItemStack> spawnedItems)
	{
		for (int i = 0; i < itemSet.Count; i++)
		{
			TraderInfo.TraderItemEntry traderItemEntry = itemSet[i];
			int num = this.RandomSpawnCount(random, traderItemEntry.minCount, traderItemEntry.maxCount);
			if (traderItemEntry.group != null)
			{
				this.SpawnItemsFromGroup(random, traderItemEntry.group, num, spawnedItems, traderItemEntry.uniqueOnly);
			}
			else
			{
				this.SpawnItem(traderItemEntry, traderItemEntry.item.itemValue, num, spawnedItems);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SpawnItemsFromGroup(GameRandom random, TraderInfo.TraderItemGroup group, int numToSpawn, List<ItemStack> spawnedItems, bool uniqueOnly)
	{
		List<int> usedIndices = null;
		if (group.uniqueOnly || uniqueOnly)
		{
			usedIndices = new List<int>();
		}
		for (int i = 0; i < numToSpawn; i++)
		{
			int numToSpawn2 = this.RandomSpawnCount(random, group.minCount, group.maxCount);
			this.SpawnLootItemsFromList(random, group.items, numToSpawn2, spawnedItems, usedIndices);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SpawnLootItemsFromList(GameRandom random, List<TraderInfo.TraderItemEntry> itemSet, int numToSpawn, List<ItemStack> spawnedItems, List<int> usedIndices)
	{
		if (numToSpawn < 1)
		{
			if (numToSpawn == -1)
			{
				this.SpawnAllItemsFromList(random, itemSet, spawnedItems);
			}
			return;
		}
		float num = 0f;
		for (int i = 0; i < itemSet.Count; i++)
		{
			TraderInfo.TraderItemEntry traderItemEntry = itemSet[i];
			if (usedIndices == null || !usedIndices.Contains(i))
			{
				num += traderItemEntry.prob;
			}
		}
		if (num == 0f)
		{
			return;
		}
		for (int j = 0; j < numToSpawn; j++)
		{
			float num2 = 0f;
			float randomFloat = random.RandomFloat;
			for (int k = 0; k < itemSet.Count; k++)
			{
				TraderInfo.TraderItemEntry traderItemEntry2 = itemSet[k];
				if (usedIndices == null || !usedIndices.Contains(k))
				{
					num2 += traderItemEntry2.prob / num;
					if (randomFloat <= num2)
					{
						int num3 = this.RandomSpawnCount(random, traderItemEntry2.minCount, traderItemEntry2.maxCount);
						if (usedIndices != null)
						{
							usedIndices.Add(k);
						}
						if (traderItemEntry2.group != null)
						{
							this.SpawnItemsFromGroup(random, traderItemEntry2.group, num3, spawnedItems, traderItemEntry2.uniqueOnly);
							break;
						}
						this.SpawnItem(traderItemEntry2, traderItemEntry2.item.itemValue, num3, spawnedItems);
						break;
					}
				}
			}
		}
	}

	public List<ItemStack> Spawn(GameRandom random)
	{
		List<ItemStack> list = new List<ItemStack>();
		this.SpawnLootItemsFromList(random, this.traderItems, -1, list, null);
		return list;
	}

	public List<ItemStack> SpawnTierGroup(GameRandom random, int tierGroupIndex)
	{
		List<ItemStack> list = new List<ItemStack>();
		this.Shuffle<TraderInfo.TraderItemEntry>((int)DateTime.Now.Ticks, ref this.TierItemGroups[tierGroupIndex].traderItems);
		int numToSpawn = this.RandomSpawnCount(random, this.minCount, this.maxCount);
		this.SpawnLootItemsFromList(random, this.TierItemGroups[tierGroupIndex].traderItems, numToSpawn, list, null);
		return list;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Shuffle<T>(int seed, ref List<T> list)
	{
		int i = list.Count;
		GameRandom gameRandom = GameRandomManager.Instance.CreateGameRandom(seed);
		while (i > 1)
		{
			i--;
			int index = gameRandom.RandomRange(0, i) % i;
			T value = list[index];
			list[index] = list[i];
			list[i] = value;
		}
		GameRandomManager.Instance.FreeGameRandom(gameRandom);
	}

	public static TraderInfo[] traderInfoList;

	public static Dictionary<string, TraderInfo.TraderItemGroup> traderItemGroups;

	[PublicizedFrom(EAccessModifier.Private)]
	public static float buyMarkup;

	[PublicizedFrom(EAccessModifier.Private)]
	public static float sellMarkdown;

	[PublicizedFrom(EAccessModifier.Private)]
	public static float qualityMinMod;

	[PublicizedFrom(EAccessModifier.Private)]
	public static float qualityMaxMod;

	public int Id;

	public float SalesMarkup;

	public int ResetInterval = 1;

	public int ResetIntervalInTicks = 24000;

	public int MaxItems = 50;

	public int minCount;

	public int maxCount;

	public bool AllowBuy = true;

	public bool AllowSell = true;

	public float OverrideBuyMarkup = -1f;

	public float OverrideSellMarkdown = -1f;

	public bool UseOpenHours;

	public ulong OpenTime;

	public ulong CloseTime;

	public ulong WarningTime;

	public bool PlayerOwned;

	public bool Rentable;

	public int RentCost;

	public int RentTimeInDays;

	public List<TraderInfo.TierItemGroup> TierItemGroups = new List<TraderInfo.TierItemGroup>();

	public List<TraderInfo.TraderItemEntry> traderItems = new List<TraderInfo.TraderItemEntry>();

	public class TraderItem
	{
		public ItemValue itemValue;
	}

	public class TraderItemGroup
	{
		public string name;

		public int minCount;

		public int maxCount;

		public int minQuality = -1;

		public int maxQuality = -1;

		public string[] modsToInstall;

		public float modChance = 1f;

		public bool uniqueOnly;

		public List<TraderInfo.TraderItemEntry> items = new List<TraderInfo.TraderItemEntry>();
	}

	public class TraderItemEntry
	{
		public int minCount;

		public int maxCount;

		public int minQuality;

		public int maxQuality;

		public float prob;

		public string[] modsToInstall;

		public float modChance = 1f;

		public bool uniqueOnly;

		public TraderInfo.TraderItem item;

		public TraderInfo.TraderItemGroup group;

		public TraderInfo.TraderItemGroup parentGroup;
	}

	public class TierItemGroup
	{
		public int minLevel;

		public int maxLevel;

		public int minCount;

		public int maxCount;

		public List<TraderInfo.TraderItemEntry> traderItems = new List<TraderInfo.TraderItemEntry>();
	}
}

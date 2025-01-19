using System;
using System.Collections.Generic;
using UnityEngine;

public class LootContainer
{
	public static void InitStatic()
	{
		LootContainer.Cleanup();
	}

	public void Init()
	{
		LootContainer.lootContainers[this.Name] = this;
	}

	public static void Cleanup()
	{
		LootContainer.lootContainers.Clear();
		LootContainer.lootGroups.Clear();
		LootContainer.lootQualityTemplates.Clear();
		LootContainer.lootProbTemplates.Clear();
	}

	public static bool IsLoaded()
	{
		return LootContainer.lootContainers.Count > 0;
	}

	public static LootContainer GetLootContainer(string _name, bool _errorOnMiss = true)
	{
		if (string.IsNullOrEmpty(_name))
		{
			return null;
		}
		LootContainer result;
		if (LootContainer.lootContainers.TryGetValue(_name, out result))
		{
			return result;
		}
		if (_errorOnMiss)
		{
			Log.Error("LootContainer '" + _name + "' unknown");
		}
		return null;
	}

	public static ItemStack GetRewardItem(string lootGroup, float questDifficulty)
	{
		if (!LootContainer.lootGroups.ContainsKey(lootGroup))
		{
			return ItemStack.Empty.Clone();
		}
		List<ItemStack> list = new List<ItemStack>();
		int num = 1;
		LootContainer.SpawnItemsFromGroup(GameManager.Instance.lootManager.Random, LootContainer.lootGroups[lootGroup], 1, 1f, list, ref num, questDifficulty, 0f, LootContainer.lootGroups[lootGroup].lootQualityTemplate, null, FastTags<TagGroup.Global>.none, true, true, false);
		if (list.Count == 0)
		{
			return ItemStack.Empty.Clone();
		}
		return list[0];
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void SpawnItem(GameRandom random, LootContainer.LootEntry template, ItemValue lootItemValue, int countToSpawn, List<ItemStack> spawnedItems, ref int slotsLeft, float gameStage, string lootQualityTemplate, EntityPlayer player, FastTags<TagGroup.Global> containerTags, bool _forceStacking)
	{
		if (lootItemValue.ItemClass == null)
		{
			return;
		}
		if (player != null)
		{
			countToSpawn = Math.Min((int)EffectManager.GetValue(PassiveEffects.LootQuantity, player.inventory.holdingItemItemValue, (float)countToSpawn, player, null, lootItemValue.ItemClass.ItemTags | containerTags, true, true, true, true, true, 1, true, false), lootItemValue.ItemClass.Stacknumber.Value);
		}
		if (countToSpawn < 1)
		{
			return;
		}
		if (lootItemValue.ItemClass.CanStack())
		{
			int value = lootItemValue.ItemClass.Stacknumber.Value;
			for (int i = 0; i < spawnedItems.Count; i++)
			{
				ItemStack itemStack = spawnedItems[i];
				if (itemStack.itemValue.type == lootItemValue.type)
				{
					if (itemStack.CanStack(countToSpawn) || _forceStacking)
					{
						itemStack.count += countToSpawn;
						return;
					}
					int num = value - itemStack.count;
					itemStack.count = value;
					countToSpawn -= num;
				}
			}
		}
		if (slotsLeft < 1)
		{
			return;
		}
		int num2 = template.minQuality;
		int maxQuality = template.maxQuality;
		string text = lootQualityTemplate;
		if (!string.IsNullOrEmpty(text))
		{
			LootContainer.LootGroup parentGroup = template.parentGroup;
			if (((parentGroup != null) ? parentGroup.lootQualityTemplate : null) == null)
			{
				goto IL_135;
			}
		}
		LootContainer.LootGroup parentGroup2 = template.parentGroup;
		text = ((parentGroup2 != null) ? parentGroup2.lootQualityTemplate : null);
		IL_135:
		if (!string.IsNullOrEmpty(text))
		{
			bool flag = false;
			for (int j = 0; j < LootContainer.lootQualityTemplates[text].templates.Count; j++)
			{
				float randomFloat = random.RandomFloat;
				LootContainer.LootGroup lootGroup = LootContainer.lootQualityTemplates[text].templates[j];
				num2 = lootGroup.minQuality;
				maxQuality = lootGroup.maxQuality;
				if (lootGroup.minLevel <= gameStage && lootGroup.maxLevel >= gameStage)
				{
					for (int k = 0; k < lootGroup.items.Count; k++)
					{
						LootContainer.LootEntry lootEntry = lootGroup.items[k];
						if (random.RandomFloat <= lootEntry.prob)
						{
							num2 = lootEntry.minQuality;
							maxQuality = lootEntry.maxQuality;
							flag = true;
							break;
						}
					}
					if (flag)
					{
						break;
					}
				}
			}
		}
		string[] modsToInstall = template.modsToInstall;
		float modChance = template.modChance;
		if (template.parentGroup != null && template.parentGroup.modsToInstall.Length != 0)
		{
			modsToInstall = template.parentGroup.modsToInstall;
			modChance = template.parentGroup.modChance;
		}
		ItemValue itemValue;
		if (lootItemValue.HasQuality)
		{
			if (num2 <= -1)
			{
				num2 = 1;
				maxQuality = 6;
			}
			itemValue = new ItemValue(lootItemValue.type, num2, maxQuality, true, modsToInstall, modChance);
		}
		else
		{
			itemValue = new ItemValue(lootItemValue.type, 1, 6, true, modsToInstall, modChance);
		}
		ItemClass itemClass = itemValue.ItemClass;
		if (itemClass != null)
		{
			if (itemClass.Actions != null && itemClass.Actions.Length != 0 && itemClass.Actions[0] != null)
			{
				itemValue.Meta = 0;
			}
			if (itemValue.MaxUseTimes > 0)
			{
				itemValue.UseTimes = (float)((int)((float)itemValue.MaxUseTimes * random.RandomRange(0.2f, 0.8f)));
			}
		}
		ItemStack item;
		if (player != null)
		{
			if (!LootContainer.OverrideItems.ContainsKey(player))
			{
				item = new ItemStack(itemValue, countToSpawn);
			}
			else
			{
				string[] array = LootContainer.OverrideItems[player];
				item = new ItemStack(ItemClass.GetItem(array[random.RandomRange(array.Length)], false), 1);
			}
		}
		else
		{
			item = new ItemStack(itemValue, countToSpawn);
		}
		spawnedItems.Add(item);
		slotsLeft--;
	}

	public static int RandomSpawnCount(GameRandom random, int min, int max, float abundance)
	{
		if (min < 0)
		{
			return -1;
		}
		float num = random.RandomRange((float)min - 0.49f, (float)max + 0.49f);
		if (num < (float)min)
		{
			num = (float)min;
		}
		if (num > (float)max)
		{
			num = (float)max;
		}
		num *= abundance;
		int num2 = (int)num;
		float num3 = num - (float)num2;
		if (random.RandomFloat < num3)
		{
			num2++;
		}
		return num2;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void SpawnAllItemsFromList(GameRandom random, List<LootContainer.LootEntry> itemSet, float abundance, List<ItemStack> spawnedItems, ref int slotsLeft, float playerLevelPercentage, float rareLootChance, string lootQualityTemplate, EntityPlayer player, FastTags<TagGroup.Global> containerTags, bool uniqueItems, bool ignoreLootProb, bool _forceStacking)
	{
		for (int i = 0; i < itemSet.Count; i++)
		{
			LootContainer.LootEntry lootEntry = itemSet[i];
			if (!lootEntry.forceProb || random.RandomFloat <= LootContainer.getProbability(player, lootEntry, playerLevelPercentage, ignoreLootProb))
			{
				int num = LootContainer.RandomSpawnCount(random, lootEntry.minCount, lootEntry.maxCount, (lootEntry.group == null) ? abundance : 1f);
				if (lootEntry.group != null)
				{
					if (lootEntry.group.minLevel <= playerLevelPercentage && lootEntry.group.maxLevel >= playerLevelPercentage)
					{
						LootContainer.SpawnItemsFromGroup(random, lootEntry.group, num, abundance, spawnedItems, ref slotsLeft, playerLevelPercentage, rareLootChance, lootQualityTemplate, player, containerTags, uniqueItems, ignoreLootProb, _forceStacking);
					}
				}
				else
				{
					LootContainer.SpawnItem(random, lootEntry, lootEntry.item.itemValue, num, spawnedItems, ref slotsLeft, playerLevelPercentage, lootQualityTemplate, player, containerTags, _forceStacking);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void SpawnItemsFromGroup(GameRandom random, LootContainer.LootGroup group, int numToSpawn, float abundance, List<ItemStack> spawnedItems, ref int slotsLeft, float gameStage, float rareLootChance, string lootQualityTemplate, EntityPlayer player, FastTags<TagGroup.Global> containerTags, bool uniqueItems, bool ignoreLootProb, bool _forceStacking)
	{
		int num = 0;
		while (num < numToSpawn && slotsLeft > 0)
		{
			LootContainer.SpawnLootItemsFromList(random, group.items, LootContainer.RandomSpawnCount(random, group.minCount, group.maxCount, 1f), abundance, spawnedItems, ref slotsLeft, gameStage, rareLootChance, lootQualityTemplate, player, containerTags, uniqueItems, ignoreLootProb, _forceStacking);
			num++;
		}
	}

	public static void SpawnLootItemsFromList(GameRandom random, List<LootContainer.LootEntry> itemSet, int numToSpawn, float abundance, List<ItemStack> spawnedItems, ref int slotsLeft, float lootStage, float rareLootChance, string lootQualityTemplate, EntityPlayer player, FastTags<TagGroup.Global> containerTags, bool uniqueItems, bool ignoreLootProb, bool _forceStacking)
	{
		if (numToSpawn < 1)
		{
			if (numToSpawn == -1)
			{
				LootContainer.SpawnAllItemsFromList(random, itemSet, abundance, spawnedItems, ref slotsLeft, lootStage, rareLootChance, lootQualityTemplate, player, containerTags, uniqueItems, ignoreLootProb, _forceStacking);
			}
			return;
		}
		float num = 0f;
		for (int i = 0; i < itemSet.Count; i++)
		{
			LootContainer.LootEntry lootEntry = itemSet[i];
			if (!lootEntry.forceProb)
			{
				num += LootContainer.getProbability(player, lootEntry, lootStage, ignoreLootProb);
			}
		}
		if (num == 0f)
		{
			return;
		}
		List<int> list = new List<int>();
		for (int j = 0; j < numToSpawn; j++)
		{
			float num2 = 0f;
			float randomFloat = random.RandomFloat;
			for (int k = 0; k < itemSet.Count; k++)
			{
				LootContainer.LootEntry lootEntry2 = itemSet[k];
				if (!list.Contains(k) || (!lootEntry2.forceProb && !uniqueItems))
				{
					float probability = LootContainer.getProbability(player, lootEntry2, lootStage, ignoreLootProb);
					bool flag;
					if (lootEntry2.forceProb)
					{
						flag = (random.RandomFloat <= probability);
					}
					else
					{
						num2 += probability / num;
						flag = (randomFloat <= num2 + rareLootChance);
					}
					if (flag)
					{
						list.Add(k);
						if (uniqueItems)
						{
							num -= LootContainer.getProbability(player, lootEntry2, lootStage, ignoreLootProb);
						}
						int num3 = LootContainer.RandomSpawnCount(random, lootEntry2.minCount, lootEntry2.maxCount, (lootEntry2.group == null) ? abundance : 1f);
						num3 += Mathf.RoundToInt((float)num3 * (lootEntry2.lootstageCountMod * lootStage));
						if (lootEntry2.group == null)
						{
							LootContainer.SpawnItem(random, lootEntry2, lootEntry2.item.itemValue, num3, spawnedItems, ref slotsLeft, lootStage, lootQualityTemplate, player, containerTags, _forceStacking);
							break;
						}
						if (lootEntry2.group.minLevel <= lootStage && lootEntry2.group.maxLevel >= lootStage)
						{
							LootContainer.SpawnItemsFromGroup(random, lootEntry2.group, num3, abundance, spawnedItems, ref slotsLeft, lootStage, rareLootChance, lootQualityTemplate, player, containerTags, uniqueItems, ignoreLootProb, _forceStacking);
							break;
						}
						break;
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static float getProbability(EntityPlayer _player, LootContainer.LootEntry _item, float _lootstage, bool _ignoreLootProb)
	{
		if (_item.lootProbTemplate != string.Empty && LootContainer.lootProbTemplates.ContainsKey(_item.lootProbTemplate))
		{
			LootContainer.LootProbabilityTemplate lootProbabilityTemplate = LootContainer.lootProbTemplates[_item.lootProbTemplate];
			int i = 0;
			while (i < lootProbabilityTemplate.templates.Count)
			{
				LootContainer.LootEntry lootEntry = lootProbabilityTemplate.templates[i];
				if (lootEntry.minLevel <= _lootstage && lootEntry.maxLevel >= _lootstage)
				{
					if (_item.item != null && !_item.item.itemValue.ItemClass.ItemTags.IsEmpty)
					{
						if (_ignoreLootProb)
						{
							return lootEntry.prob;
						}
						return EffectManager.GetValue(PassiveEffects.LootProb, null, lootEntry.prob, _player, null, _item.item.itemValue.ItemClass.ItemTags, true, true, true, true, true, 1, true, false);
					}
					else
					{
						if (_item.tags.IsEmpty)
						{
							return lootEntry.prob;
						}
						return EffectManager.GetValue(PassiveEffects.LootProb, null, lootEntry.prob, _player, null, _item.tags, true, true, true, true, true, 1, true, false);
					}
				}
				else
				{
					i++;
				}
			}
		}
		if (_item.item != null && !_item.item.itemValue.ItemClass.ItemTags.IsEmpty)
		{
			if (_ignoreLootProb)
			{
				return _item.prob;
			}
			return EffectManager.GetValue(PassiveEffects.LootProb, null, _item.prob, _player, null, _item.item.itemValue.ItemClass.ItemTags, true, true, true, true, true, 1, true, false);
		}
		else
		{
			if (_item.tags.IsEmpty)
			{
				return _item.prob;
			}
			return EffectManager.GetValue(PassiveEffects.LootProb, null, _item.prob, _player, null, _item.tags, true, true, true, true, true, 1, true, false);
		}
	}

	public void ExecuteBuffActions(int instigatorId, EntityAlive target)
	{
		if (this.BuffActions != null)
		{
			for (int i = 0; i < this.BuffActions.Count; i++)
			{
				target.Buffs.AddBuff(this.BuffActions[i], -1, true, false, -1f);
			}
		}
	}

	public IList<ItemStack> Spawn(GameRandom random, int _maxItems, float playerLevelPercentage, float rareLootChance, EntityPlayer player, FastTags<TagGroup.Global> containerTags, bool uniqueItems, bool ignoreLootProb)
	{
		List<ItemStack> list = new List<ItemStack>();
		int numToSpawn = Mathf.Min(LootContainer.RandomSpawnCount(random, this.minCount, this.maxCount, 1f), _maxItems);
		float abundance = 1f;
		if (!this.ignoreLootAbundance)
		{
			abundance = (float)GamePrefs.GetInt(EnumGamePrefs.LootAbundance) * 0.01f;
		}
		LootContainer.SpawnLootItemsFromList(random, this.itemsToSpawn, numToSpawn, abundance, list, ref _maxItems, playerLevelPercentage, rareLootChance, this.lootQualityTemplate, player, containerTags, uniqueItems, ignoreLootProb, false);
		return list;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<string, LootContainer> lootContainers = new CaseInsensitiveStringDictionary<LootContainer>();

	public static readonly Dictionary<string, LootContainer.LootGroup> lootGroups = new Dictionary<string, LootContainer.LootGroup>();

	public static readonly Dictionary<string, LootContainer.LootQualityTemplate> lootQualityTemplates = new Dictionary<string, LootContainer.LootQualityTemplate>();

	public static readonly Dictionary<string, LootContainer.LootProbabilityTemplate> lootProbTemplates = new Dictionary<string, LootContainer.LootProbabilityTemplate>();

	public string Name;

	public string soundOpen;

	public string soundClose;

	public Vector2i size;

	public float openTime;

	public int minCount;

	public int maxCount;

	public LootContainer.DestroyOnClose destroyOnClose;

	public string lootQualityTemplate;

	public List<string> BuffActions;

	public bool ignoreLootAbundance;

	public bool useUnmodifiedLootstage;

	public bool UniqueItems;

	public bool IgnoreLootProb;

	public readonly List<LootContainer.LootEntry> itemsToSpawn = new List<LootContainer.LootEntry>();

	public static Dictionary<EntityPlayer, string[]> OverrideItems = new Dictionary<EntityPlayer, string[]>();

	public enum DestroyOnClose
	{
		False,
		True,
		Empty
	}

	public class LootItem
	{
		public ItemValue itemValue;
	}

	public class LootGroup
	{
		public string name;

		public string lootQualityTemplate;

		public int minCount;

		public int maxCount;

		public int minQuality = -1;

		public int maxQuality = -1;

		public float minLevel;

		public float maxLevel;

		public string[] modsToInstall;

		public float modChance = 1f;

		public readonly List<LootContainer.LootEntry> items = new List<LootContainer.LootEntry>();
	}

	public class LootEntry
	{
		public string lootProbTemplate;

		public int minCount;

		public int maxCount;

		public int minQuality;

		public int maxQuality;

		public float minLevel;

		public float maxLevel;

		public float prob;

		public bool forceProb;

		public string[] modsToInstall;

		public float modChance = 1f;

		public float lootstageCountMod;

		public LootContainer.LootItem item;

		public LootContainer.LootGroup group;

		public LootContainer.LootGroup parentGroup;

		public FastTags<TagGroup.Global> tags;
	}

	public class LootProbabilityTemplate
	{
		public string name;

		public readonly List<LootContainer.LootEntry> templates = new List<LootContainer.LootEntry>();
	}

	public class LootQualityTemplate
	{
		public string name;

		public readonly List<LootContainer.LootGroup> templates = new List<LootContainer.LootGroup>();
	}
}

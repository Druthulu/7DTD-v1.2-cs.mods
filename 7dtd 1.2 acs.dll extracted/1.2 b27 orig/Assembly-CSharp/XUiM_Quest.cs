using System;
using Challenges;
using UnityEngine;

public class XUiM_Quest : XUiModel
{
	public event XUiEvent_TrackedQuestChanged OnTrackedQuestChanged;

	public Quest TrackedQuest
	{
		get
		{
			return this.trackedQuest;
		}
		set
		{
			this.trackedQuest = value;
			if (this.OnTrackedQuestChanged != null)
			{
				this.OnTrackedQuestChanged();
			}
		}
	}

	public event XUiEvent_TrackedQuestChanged OnTrackedChallengeChanged;

	public Challenge TrackedChallenge
	{
		get
		{
			return this.trackedChallenge;
		}
		set
		{
			if (this.trackedChallenge != null)
			{
				this.trackedChallenge.IsTracked = false;
				this.trackedChallenge.RemovePrerequisiteHooks();
				this.trackedChallenge.HandleTrackingEnded();
			}
			this.trackedChallenge = value;
			if (this.trackedChallenge != null)
			{
				this.trackedChallenge.IsTracked = true;
				this.trackedChallenge.AddPrerequisiteHooks();
				this.trackedChallenge.HandleTrackingStarted();
				this.trackedChallenge.Owner.Player.PlayerUI.xui.Recipes.TrackedRecipe = null;
				this.trackedChallenge.Owner.Player.QuestJournal.TrackedQuest = null;
			}
			if (this.OnTrackedChallengeChanged != null)
			{
				this.OnTrackedChallengeChanged();
			}
		}
	}

	public void HandleTrackedChallengeChanged()
	{
		if (this.OnTrackedChallengeChanged != null)
		{
			this.OnTrackedChallengeChanged();
		}
	}

	public static string GetQuestItemRewards(Quest quest, EntityPlayer player, string rewardItemFormat, string rewardBonusItemFormat)
	{
		string text = "";
		if (quest != null)
		{
			for (int i = 0; i < quest.Rewards.Count; i++)
			{
				if ((quest.Rewards[i] is RewardItem || quest.Rewards[i] is RewardLootItem) && !quest.Rewards[i].isChosenReward)
				{
					ItemStack itemStack = null;
					if (quest.Rewards[i] is RewardItem)
					{
						itemStack = (quest.Rewards[i] as RewardItem).Item;
					}
					else if (quest.Rewards[i] is RewardLootItem)
					{
						itemStack = (quest.Rewards[i] as RewardLootItem).Item;
					}
					int count = itemStack.count;
					int num = Mathf.FloorToInt(EffectManager.GetValue(PassiveEffects.QuestBonusItemReward, null, (float)itemStack.count, player, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false));
					if (count == num)
					{
						text = text + string.Format(rewardItemFormat, num, itemStack.itemValue.ItemClass.GetLocalizedItemName()) + ", ";
					}
					else
					{
						text = text + string.Format(rewardBonusItemFormat, new object[]
						{
							num,
							itemStack.itemValue.ItemClass.GetLocalizedItemName(),
							num - count,
							Localization.Get("bonus", false)
						}) + ", ";
					}
				}
			}
			if (text != "")
			{
				text = text.Remove(text.Length - 2);
			}
		}
		return text;
	}

	public static bool HasQuestRewards(Quest quest, EntityPlayer player, bool isChosen)
	{
		if (quest != null)
		{
			for (int i = 0; i < quest.Rewards.Count; i++)
			{
				if (quest.Rewards[i].isChosenReward == isChosen && !(quest.Rewards[i] is RewardQuest))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static string GetQuestRewards(Quest quest, EntityPlayer player, bool isChosen, string rewardItemFormat, string rewardItemBonusFormat, string rewardNumberFormat, string rewardNumberBonusFormat)
	{
		string text = "";
		if (quest != null)
		{
			int num = isChosen ? ((int)EffectManager.GetValue(PassiveEffects.QuestRewardOptionCount, null, 1f, player, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false) + 1) : -1;
			for (int i = 0; i < quest.Rewards.Count; i++)
			{
				if (quest.Rewards[i].isChosenReward == isChosen)
				{
					if (isChosen && num-- <= 0)
					{
						break;
					}
					if (quest.Rewards[i] is RewardItem)
					{
						RewardItem rewardItem = quest.Rewards[i] as RewardItem;
						int count = rewardItem.Item.count;
						int num2 = Mathf.FloorToInt(EffectManager.GetValue(PassiveEffects.QuestBonusItemReward, null, (float)rewardItem.Item.count, player, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false));
						if (count == num2)
						{
							text = text + string.Format(rewardItemFormat, num2, rewardItem.Item.itemValue.ItemClass.GetLocalizedItemName()) + ", ";
						}
						else
						{
							text = text + string.Format(rewardItemBonusFormat, new object[]
							{
								num2,
								rewardItem.Item.itemValue.ItemClass.GetLocalizedItemName(),
								num2 - count,
								Localization.Get("bonus", false)
							}) + ", ";
						}
					}
					if (quest.Rewards[i] is RewardLootItem)
					{
						RewardLootItem rewardLootItem = quest.Rewards[i] as RewardLootItem;
						int count2 = rewardLootItem.Item.count;
						text = text + string.Format(rewardItemFormat, rewardLootItem.Item.count, rewardLootItem.Item.itemValue.ItemClass.GetLocalizedItemName()) + ", ";
					}
					else if (quest.Rewards[i] is RewardExp)
					{
						BaseReward baseReward = quest.Rewards[i] as RewardExp;
						int num3 = 0;
						int num4 = Convert.ToInt32(baseReward.Value) * GameStats.GetInt(EnumGameStats.XPMultiplier) / 100;
						num3 += Mathf.FloorToInt(EffectManager.GetValue(PassiveEffects.PlayerExpGain, null, (float)num4, player, null, XUiM_Quest.QuestTag, true, true, true, true, true, 1, true, false));
						text = text + string.Format(rewardNumberFormat, num3, Localization.Get("RewardXP_keyword", false)) + ", ";
					}
					else if (quest.Rewards[i] is RewardSkillPoints)
					{
						int num5 = Convert.ToInt32((quest.Rewards[i] as RewardSkillPoints).Value);
						text = text + string.Format(rewardNumberFormat, num5, Localization.Get("RewardSkillPoints_keyword", false)) + ", ";
					}
				}
			}
			if (text != "")
			{
				text = text.Remove(text.Length - 2);
			}
		}
		return text;
	}

	public static bool HasChainQuestRewards(Quest quest, EntityPlayer player)
	{
		Quest quest2 = quest;
		while (quest != null)
		{
			Quest quest3 = null;
			for (int i = 0; i < quest.Rewards.Count; i++)
			{
				if (!quest.Rewards[i].isChosenReward && quest.Rewards[i].isChainReward && quest != quest2)
				{
					return true;
				}
				if (quest.Rewards[i] is RewardQuest)
				{
					RewardQuest rewardQuest = quest.Rewards[i] as RewardQuest;
					if (rewardQuest.IsChainQuest)
					{
						quest3 = QuestClass.CreateQuest(rewardQuest.ID);
					}
				}
			}
			quest = quest3;
		}
		return false;
	}

	public static string GetChainQuestRewards(Quest quest, EntityPlayer player, string rewardItemFormat, string rewardItemBonusFormat, string rewardNumberFormat, string rewardNumberBonusFormat)
	{
		string text = "";
		Quest quest2 = quest;
		while (quest != null)
		{
			Quest quest3 = null;
			for (int i = 0; i < quest.Rewards.Count; i++)
			{
				if (!quest.Rewards[i].isChosenReward && quest.Rewards[i].isChainReward && quest != quest2)
				{
					if (quest.Rewards[i] is RewardItem)
					{
						RewardItem rewardItem = quest.Rewards[i] as RewardItem;
						int count = rewardItem.Item.count;
						int num = Mathf.FloorToInt(EffectManager.GetValue(PassiveEffects.QuestBonusItemReward, null, (float)rewardItem.Item.count, player, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false));
						if (count == num)
						{
							text = text + string.Format(rewardItemFormat, num, rewardItem.Item.itemValue.ItemClass.GetLocalizedItemName()) + ", ";
						}
						else
						{
							text = text + string.Format(rewardItemBonusFormat, new object[]
							{
								num,
								rewardItem.Item.itemValue.ItemClass.GetLocalizedItemName(),
								num - count,
								Localization.Get("bonus", false)
							}) + ", ";
						}
					}
					else if (quest.Rewards[i] is RewardExp)
					{
						BaseReward baseReward = quest.Rewards[i] as RewardExp;
						int num2 = 0;
						int num3 = Convert.ToInt32(baseReward.Value) * GameStats.GetInt(EnumGameStats.XPMultiplier) / 100;
						num2 += Mathf.FloorToInt(EffectManager.GetValue(PassiveEffects.PlayerExpGain, null, (float)num3, player, null, XUiM_Quest.QuestTag, true, true, true, true, true, 1, true, false));
						text = text + string.Format(rewardNumberFormat, num2, Localization.Get("RewardXP_keyword", false)) + ", ";
					}
					else if (quest.Rewards[i] is RewardSkillPoints)
					{
						int num4 = Convert.ToInt32((quest.Rewards[i] as RewardSkillPoints).Value);
						text = text + string.Format(rewardNumberFormat, num4, Localization.Get("RewardSkillPoints_keyword", false)) + ", ";
					}
				}
				if (quest.Rewards[i] is RewardQuest)
				{
					RewardQuest rewardQuest = quest.Rewards[i] as RewardQuest;
					if (rewardQuest.IsChainQuest)
					{
						quest3 = QuestClass.CreateQuest(rewardQuest.ID);
					}
				}
			}
			quest = quest3;
			if (text != "")
			{
				text = text.Remove(text.Length - 2);
			}
		}
		return text;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Quest trackedQuest;

	[PublicizedFrom(EAccessModifier.Private)]
	public Challenge trackedChallenge;

	public static FastTags<TagGroup.Global> QuestTag = FastTags<TagGroup.Global>.Parse("quest");
}

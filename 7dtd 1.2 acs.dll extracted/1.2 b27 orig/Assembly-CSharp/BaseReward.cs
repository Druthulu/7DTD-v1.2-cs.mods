﻿using System;
using System.IO;

public abstract class BaseReward
{
	public string ID { get; set; }

	public string Value { get; set; }

	public Quest OwnerQuest { get; set; }

	public string Description
	{
		get
		{
			if (!this.displaySetup)
			{
				this.SetupReward();
				this.displaySetup = true;
			}
			return this.description;
		}
		set
		{
			this.description = value;
		}
	}

	public string ValueText
	{
		get
		{
			if (!this.displaySetup)
			{
				this.SetupReward();
				this.displaySetup = true;
			}
			return this.valueText;
		}
		set
		{
			this.valueText = value;
		}
	}

	public string Icon
	{
		get
		{
			if (!this.displaySetup)
			{
				this.SetupReward();
				this.displaySetup = true;
			}
			return this.icon;
		}
		set
		{
			this.icon = value;
		}
	}

	public string IconAtlas { get; set; }

	public bool HiddenReward { get; set; }

	public bool Optional { get; set; }

	public bool isChosenReward { get; set; }

	public bool isChainReward { get; set; }

	public bool isFixedLocation { get; set; }

	public BaseReward.ReceiveStages ReceiveStage { get; set; }

	public byte RewardIndex { get; set; }

	public BaseReward()
	{
		this.IconAtlas = "UIAtlas";
		this.ReceiveStage = BaseReward.ReceiveStages.QuestCompletion;
		this.isFixedLocation = false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void CopyValues(BaseReward reward)
	{
		reward.ID = this.ID;
		reward.Value = this.Value;
		reward.ReceiveStage = this.ReceiveStage;
		reward.HiddenReward = this.HiddenReward;
		reward.Optional = this.Optional;
		reward.isChosenReward = this.isChosenReward;
		reward.isChainReward = this.isChainReward;
		reward.isFixedLocation = this.isFixedLocation;
		reward.RewardIndex = this.RewardIndex;
	}

	public virtual void HandleVariables()
	{
		this.ID = this.OwnerQuest.ParseVariable(this.ID);
		this.Value = this.OwnerQuest.ParseVariable(this.Value);
	}

	public virtual void SetupReward()
	{
	}

	public virtual void GiveReward(EntityPlayer player)
	{
	}

	public void GiveReward()
	{
		this.GiveReward(this.OwnerQuest.OwnerJournal.OwnerPlayer);
	}

	public virtual ItemStack GetRewardItem()
	{
		return ItemStack.Empty;
	}

	public virtual BaseReward Clone()
	{
		return null;
	}

	public virtual void SetupGlobalRewardSettings()
	{
	}

	public virtual void Read(BinaryReader _br)
	{
		this.RewardIndex = _br.ReadByte();
	}

	public virtual void Write(BinaryWriter _bw)
	{
		_bw.Write(this.RewardIndex);
	}

	public virtual void ParseProperties(DynamicProperties properties)
	{
		if (properties.Values.ContainsKey(BaseReward.PropID))
		{
			this.ID = properties.Values[BaseReward.PropID];
		}
		if (properties.Values.ContainsKey(BaseReward.PropValue))
		{
			this.Value = properties.Values[BaseReward.PropValue];
		}
		if (properties.Values.ContainsKey(BaseReward.PropReceiveStage))
		{
			string a = properties.Values[BaseReward.PropReceiveStage];
			if (!(a == "start"))
			{
				if (!(a == "complete"))
				{
					if (a == "aftercomplete")
					{
						this.ReceiveStage = BaseReward.ReceiveStages.AfterCompleteNotification;
					}
				}
				else
				{
					this.ReceiveStage = BaseReward.ReceiveStages.QuestCompletion;
				}
			}
			else
			{
				this.ReceiveStage = BaseReward.ReceiveStages.QuestStart;
			}
		}
		if (properties.Values.ContainsKey(BaseReward.PropOptional))
		{
			bool optional;
			StringParsers.TryParseBool(properties.Values[BaseReward.PropOptional], out optional, 0, -1, true);
			this.Optional = optional;
		}
		if (properties.Values.ContainsKey(BaseReward.PropHidden))
		{
			bool hiddenReward;
			StringParsers.TryParseBool(properties.Values[BaseReward.PropHidden], out hiddenReward, 0, -1, true);
			this.HiddenReward = hiddenReward;
		}
		if (properties.Values.ContainsKey(BaseReward.PropIsChosen))
		{
			bool isChosenReward;
			StringParsers.TryParseBool(properties.Values[BaseReward.PropIsChosen], out isChosenReward, 0, -1, true);
			this.isChosenReward = isChosenReward;
		}
		if (properties.Values.ContainsKey(BaseReward.PropIsFixed))
		{
			bool isFixedLocation;
			StringParsers.TryParseBool(properties.Values[BaseReward.PropIsFixed], out isFixedLocation, 0, -1, true);
			this.isFixedLocation = isFixedLocation;
		}
		if (properties.Values.ContainsKey(BaseReward.PropIsChain))
		{
			bool isChainReward;
			StringParsers.TryParseBool(properties.Values[BaseReward.PropIsChain], out isChainReward, 0, -1, true);
			this.isChainReward = isChainReward;
		}
	}

	public virtual string GetRewardText()
	{
		return "";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool displaySetup;

	[PublicizedFrom(EAccessModifier.Private)]
	public string description = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string valueText = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string icon = "";

	public static string PropID = "id";

	public static string PropValue = "value";

	public static string PropOptional = "optional";

	public static string PropReceiveStage = "stage";

	public static string PropHidden = "hidden";

	public static string PropIsChosen = "ischosen";

	public static string PropIsChain = "chainreward";

	public static string PropIsFixed = "isfixed";

	public enum RewardTypes
	{
		Exp,
		Item,
		Level,
		Quest,
		Recipe,
		ShowTip,
		Skill,
		SkillPoints
	}

	public enum ReceiveStages
	{
		QuestStart,
		QuestCompletion,
		AfterCompleteNotification
	}
}

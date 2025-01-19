using System;
using UnityEngine.Scripting;

[Preserve]
public class RewardExp : BaseReward
{
	public override void SetupReward()
	{
		base.Description = Localization.Get("RewardExp_keyword", false);
		this.SetupValueText();
		base.Icon = "ui_game_symbol_trophy";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupValueText()
	{
		float num = Convert.ToSingle(base.Value) * (float)GameStats.GetInt(EnumGameStats.XPMultiplier) / 100f;
		if (num > 214748368f)
		{
			num = 214748368f;
		}
		base.ValueText = num.ToString("{0}");
	}

	public override void GiveReward(EntityPlayer player)
	{
		int exp = Convert.ToInt32(base.Value);
		player.Progression.AddLevelExp(exp, "_xpFromQuest", Progression.XPTypes.Quest, true, true);
	}

	public override BaseReward Clone()
	{
		RewardExp rewardExp = new RewardExp();
		base.CopyValues(rewardExp);
		return rewardExp;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		if (properties.Values.ContainsKey(RewardExp.PropExp))
		{
			base.Value = properties.Values[RewardExp.PropExp];
		}
	}

	public static string PropExp = "xp";

	[PublicizedFrom(EAccessModifier.Private)]
	public static FastTags<TagGroup.Global> questTag = FastTags<TagGroup.Global>.Parse("quest");
}

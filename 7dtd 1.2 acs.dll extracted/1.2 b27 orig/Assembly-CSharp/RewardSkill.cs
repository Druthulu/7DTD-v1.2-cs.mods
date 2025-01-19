using System;
using UnityEngine.Scripting;

[Preserve]
public class RewardSkill : BaseReward
{
	public override void SetupReward()
	{
		string arg = Localization.Get("RewardSkill_keyword", false);
		base.Description = string.Format("{0} {1}", base.ID, arg);
		base.ValueText = base.Value;
		base.Icon = base.OwnerQuest.OwnerJournal.OwnerPlayer.Progression.GetProgressionValue(base.ID).ProgressionClass.Icon;
	}

	public override void GiveReward(EntityPlayer player)
	{
		ProgressionValue progressionValue = player.Progression.GetProgressionValue(base.ID);
		int num = Convert.ToInt32(base.Value);
		if (progressionValue != null)
		{
			if (progressionValue.Level + num > progressionValue.ProgressionClass.MaxLevel)
			{
				progressionValue.Level = progressionValue.ProgressionClass.MaxLevel;
			}
			else
			{
				progressionValue.Level += num;
			}
			if (progressionValue.ProgressionClass.IsPerk)
			{
				player.MinEventContext.ProgressionValue = progressionValue;
				progressionValue.ProgressionClass.FireEvent(MinEventTypes.onPerkLevelChanged, player.MinEventContext);
			}
			if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEntitySetSkillLevelServer>().Setup(player.entityId, progressionValue.Name, progressionValue.Level), false);
			}
		}
	}

	public override BaseReward Clone()
	{
		RewardSkill rewardSkill = new RewardSkill();
		base.CopyValues(rewardSkill);
		return rewardSkill;
	}
}

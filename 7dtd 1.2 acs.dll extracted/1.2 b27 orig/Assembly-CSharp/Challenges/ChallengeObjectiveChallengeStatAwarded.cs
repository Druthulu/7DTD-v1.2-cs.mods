using System;
using System.Xml.Linq;
using UnityEngine.Scripting;

namespace Challenges
{
	[Preserve]
	public class ChallengeObjectiveChallengeStatAwarded : BaseChallengeObjective
	{
		public override ChallengeObjectiveType ObjectiveType
		{
			get
			{
				return ChallengeObjectiveType.ChallengeStatAwarded;
			}
		}

		public override string DescriptionText
		{
			get
			{
				return Localization.Get(this.statText, false);
			}
		}

		public override void HandleAddHooks()
		{
			QuestEventManager.Current.ChallengeAwardCredit += this.Current_ChallengeAwardCredit;
		}

		public override void HandleRemoveHooks()
		{
			QuestEventManager.Current.ChallengeAwardCredit -= this.Current_ChallengeAwardCredit;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Current_ChallengeAwardCredit(string stat, int awardCount)
		{
			if (this.challengeStat.EqualsCaseInsensitive(stat))
			{
				base.Current += awardCount;
				if (base.Current >= this.MaxCount)
				{
					base.Current = this.MaxCount;
					this.CheckObjectiveComplete(true);
				}
			}
		}

		public override void ParseElement(XElement e)
		{
			base.ParseElement(e);
			if (e.HasAttribute("challenge_stat"))
			{
				this.challengeStat = e.GetAttribute("challenge_stat");
			}
			if (e.HasAttribute("stat_text_key"))
			{
				this.statText = Localization.Get(e.GetAttribute("stat_text_key"), false);
				return;
			}
			if (e.HasAttribute("stat_text"))
			{
				this.statText = e.GetAttribute("stat_text");
			}
		}

		public override BaseChallengeObjective Clone()
		{
			return new ChallengeObjectiveChallengeStatAwarded
			{
				challengeStat = this.challengeStat,
				statText = this.statText
			};
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public string challengeStat = "";

		[PublicizedFrom(EAccessModifier.Private)]
		public string statText = "";
	}
}

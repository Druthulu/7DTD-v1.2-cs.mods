using System;
using UnityEngine.Scripting;

namespace Challenges
{
	[Preserve]
	public class ChallengeObjectiveBloodmoon : BaseChallengeObjective
	{
		public override ChallengeObjectiveType ObjectiveType
		{
			get
			{
				return ChallengeObjectiveType.Bloodmoon;
			}
		}

		public override string DescriptionText
		{
			get
			{
				return Localization.Get("challengeObjectiveBloodMoonCompleted", false) + ":";
			}
		}

		public override void Init()
		{
		}

		public override void HandleAddHooks()
		{
			QuestEventManager.Current.BloodMoonSurvive += this.Current_BloodMoonSurvive;
		}

		public override void HandleRemoveHooks()
		{
			QuestEventManager.Current.BloodMoonSurvive -= this.Current_BloodMoonSurvive;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Current_BloodMoonSurvive()
		{
			int num = base.Current;
			base.Current = num + 1;
			this.CheckObjectiveComplete(true);
		}

		public override BaseChallengeObjective Clone()
		{
			return new ChallengeObjectiveBloodmoon();
		}
	}
}

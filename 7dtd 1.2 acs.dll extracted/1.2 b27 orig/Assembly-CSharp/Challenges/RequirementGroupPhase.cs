using System;
using System.Collections.Generic;

namespace Challenges
{
	public class RequirementGroupPhase
	{
		public void AddChallengeObjective(BaseChallengeObjective obj)
		{
			this.RequirementObjectiveList.Add(obj);
		}

		public void AddHooks()
		{
			for (int i = 0; i < this.RequirementObjectiveList.Count; i++)
			{
				for (int j = 0; j < this.RequirementObjectiveList.Count; j++)
				{
					this.RequirementObjectiveList[j].HandleAddHooks();
				}
			}
		}

		public bool HandleCheckStatus()
		{
			bool result = false;
			for (int i = 0; i < this.RequirementObjectiveList.Count; i++)
			{
				if (!this.RequirementObjectiveList[i].HandleCheckStatus())
				{
					result = true;
				}
				this.RequirementObjectiveList[i].UpdateStatus();
			}
			return result;
		}

		public void HandleRemoveHooks()
		{
			for (int i = 0; i < this.RequirementObjectiveList.Count; i++)
			{
				this.RequirementObjectiveList[i].HandleRemoveHooks();
			}
		}

		public void ResetComplete()
		{
			this.IsComplete = false;
			for (int i = 0; i < this.RequirementObjectiveList.Count; i++)
			{
				this.RequirementObjectiveList[i].ResetComplete();
			}
		}

		public virtual void UpdateStatus()
		{
			for (int i = 0; i < this.RequirementObjectiveList.Count; i++)
			{
				this.RequirementObjectiveList[i].UpdateStatus();
			}
		}

		public void Clone(RequirementGroupPhase phase)
		{
			for (int i = 0; i < phase.RequirementObjectiveList.Count; i++)
			{
				BaseChallengeObjective item = phase.RequirementObjectiveList[i].Clone();
				if (this.RequirementObjectiveList == null)
				{
					this.RequirementObjectiveList = new List<BaseChallengeObjective>();
				}
				this.RequirementObjectiveList.Add(item);
			}
		}

		public RequirementGroupPhase Clone()
		{
			RequirementGroupPhase requirementGroupPhase = new RequirementGroupPhase();
			for (int i = 0; i < this.RequirementObjectiveList.Count; i++)
			{
				BaseChallengeObjective item = this.RequirementObjectiveList[i].Clone();
				if (this.RequirementObjectiveList == null)
				{
					this.RequirementObjectiveList = new List<BaseChallengeObjective>();
				}
				requirementGroupPhase.RequirementObjectiveList.Add(item);
			}
			return requirementGroupPhase;
		}

		public Recipe GetItemRecipe()
		{
			for (int i = 0; i < this.RequirementObjectiveList.Count; i++)
			{
				Recipe recipeItem = this.RequirementObjectiveList[i].GetRecipeItem();
				if (recipeItem != null)
				{
					return recipeItem;
				}
			}
			return null;
		}

		public List<BaseChallengeObjective> RequirementObjectiveList = new List<BaseChallengeObjective>();

		public bool IsComplete;
	}
}

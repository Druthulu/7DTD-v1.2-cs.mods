using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class StatCompareModMax : StatCompareCurrent
{
	public override bool IsValid(MinEventParams _params)
	{
		if (!this.ParamsValid(_params))
		{
			return false;
		}
		switch (this.stat)
		{
		case StatCompareCurrent.StatTypes.Health:
			if (!this.invert)
			{
				return RequirementBase.compareValues(this.target.Stats.Health.ModifiedMax, this.operation, this.value);
			}
			return !RequirementBase.compareValues(this.target.Stats.Health.ModifiedMax, this.operation, this.value);
		case StatCompareCurrent.StatTypes.Stamina:
			if (!this.invert)
			{
				return RequirementBase.compareValues(this.target.Stats.Stamina.ModifiedMax, this.operation, this.value);
			}
			return !RequirementBase.compareValues(this.target.Stats.Stamina.ModifiedMax, this.operation, this.value);
		case StatCompareCurrent.StatTypes.Food:
			if (!this.invert)
			{
				return RequirementBase.compareValues(this.target.Stats.Food.ModifiedMax, this.operation, this.value);
			}
			return !RequirementBase.compareValues(this.target.Stats.Food.ModifiedMax, this.operation, this.value);
		case StatCompareCurrent.StatTypes.Water:
			if (!this.invert)
			{
				return RequirementBase.compareValues(this.target.Stats.Water.ModifiedMax, this.operation, this.value);
			}
			return !RequirementBase.compareValues(this.target.Stats.Water.ModifiedMax, this.operation, this.value);
		default:
			return false;
		}
	}

	public override void GetInfoStrings(ref List<string> list)
	{
		list.Add(string.Format("stat '{1}'% {0}{2} {3}", new object[]
		{
			this.invert ? "NOT " : "",
			this.stat.ToStringCached<StatCompareCurrent.StatTypes>(),
			this.operation.ToStringCached<RequirementBase.OperationTypes>(),
			this.value.ToCultureInvariantString()
		}));
	}
}

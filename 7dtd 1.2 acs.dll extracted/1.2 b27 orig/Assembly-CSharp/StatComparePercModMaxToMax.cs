using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class StatComparePercModMaxToMax : StatCompareCurrent
{
	public override bool IsValid(MinEventParams _params)
	{
		if (!this.ParamsValid(_params))
		{
			return false;
		}
		StatCompareCurrent.StatTypes stat = this.stat;
		if (stat != StatCompareCurrent.StatTypes.Health)
		{
			if (stat != StatCompareCurrent.StatTypes.Stamina)
			{
				return false;
			}
			if (!this.invert)
			{
				return RequirementBase.compareValues(this.target.Stats.Stamina.ModifiedMaxPercent, this.operation, this.value);
			}
			return !RequirementBase.compareValues(this.target.Stats.Stamina.ModifiedMaxPercent, this.operation, this.value);
		}
		else
		{
			if (!this.invert)
			{
				return RequirementBase.compareValues(this.target.Stats.Health.ModifiedMaxPercent, this.operation, this.value);
			}
			return !RequirementBase.compareValues(this.target.Stats.Health.ModifiedMaxPercent, this.operation, this.value);
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

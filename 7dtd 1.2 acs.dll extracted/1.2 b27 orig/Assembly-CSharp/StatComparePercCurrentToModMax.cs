using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class StatComparePercCurrentToModMax : StatCompareCurrent
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
			float modifiedMax = this.target.Stats.Stamina.ModifiedMax;
			if (modifiedMax <= 0f)
			{
				return false;
			}
			if (!this.invert)
			{
				return RequirementBase.compareValues(this.target.Stamina / modifiedMax, this.operation, this.value);
			}
			return !RequirementBase.compareValues(this.target.Stamina / modifiedMax, this.operation, this.value);
		}
		else
		{
			float modifiedMax2 = this.target.Stats.Health.ModifiedMax;
			if (modifiedMax2 <= 0f)
			{
				return false;
			}
			if (!this.invert)
			{
				return RequirementBase.compareValues((float)this.target.Health / modifiedMax2, this.operation, this.value);
			}
			return !RequirementBase.compareValues((float)this.target.Health / modifiedMax2, this.operation, this.value);
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

using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class StatComparePercCurrentToMax : StatCompareCurrent
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
		{
			float max = this.target.Stats.Health.Max;
			if (max <= 0f)
			{
				return false;
			}
			if (!this.invert)
			{
				return RequirementBase.compareValues((float)this.target.Health / max, this.operation, this.value);
			}
			return !RequirementBase.compareValues((float)this.target.Health / max, this.operation, this.value);
		}
		case StatCompareCurrent.StatTypes.Stamina:
		{
			float max2 = this.target.Stats.Stamina.Max;
			if (max2 <= 0f)
			{
				return false;
			}
			if (!this.invert)
			{
				return RequirementBase.compareValues(this.target.Stamina / max2, this.operation, this.value);
			}
			return !RequirementBase.compareValues(this.target.Stamina / max2, this.operation, this.value);
		}
		case StatCompareCurrent.StatTypes.Food:
		{
			float max3 = this.target.Stats.Food.Max;
			if (max3 <= 0f)
			{
				return false;
			}
			if (!this.invert)
			{
				return RequirementBase.compareValues(this.target.Stats.Food.Value / max3, this.operation, this.value);
			}
			return !RequirementBase.compareValues(this.target.Stats.Food.Value / max3, this.operation, this.value);
		}
		case StatCompareCurrent.StatTypes.Water:
		{
			float max4 = this.target.Stats.Water.Max;
			if (max4 <= 0f)
			{
				return false;
			}
			if (!this.invert)
			{
				return RequirementBase.compareValues(this.target.Stats.Water.Value / max4, this.operation, this.value);
			}
			return !RequirementBase.compareValues(this.target.Stats.Water.Value / max4, this.operation, this.value);
		}
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

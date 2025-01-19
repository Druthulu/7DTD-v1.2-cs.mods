using System;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class StatCompareCurrent : TargetedCompareRequirementBase
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
				return RequirementBase.compareValues((float)this.target.Health, this.operation, this.value);
			}
			return !RequirementBase.compareValues((float)this.target.Health, this.operation, this.value);
		case StatCompareCurrent.StatTypes.Stamina:
			if (!this.invert)
			{
				return RequirementBase.compareValues(this.target.Stamina, this.operation, this.value);
			}
			return !RequirementBase.compareValues(this.target.Stamina, this.operation, this.value);
		case StatCompareCurrent.StatTypes.Food:
			if (!this.invert)
			{
				return RequirementBase.compareValues(this.target.Stats.Food.Value, this.operation, this.value);
			}
			return !RequirementBase.compareValues(this.target.Stats.Food.Value, this.operation, this.value);
		case StatCompareCurrent.StatTypes.Water:
			if (!this.invert)
			{
				return RequirementBase.compareValues(this.target.Stats.Water.Value, this.operation, this.value);
			}
			return !RequirementBase.compareValues(this.target.Stats.Water.Value, this.operation, this.value);
		case StatCompareCurrent.StatTypes.Armor:
			if (!this.invert)
			{
				return RequirementBase.compareValues(this.target.equipment.CurrentLowestDurability, this.operation, this.value);
			}
			return !RequirementBase.compareValues(this.target.equipment.CurrentLowestDurability, this.operation, this.value);
		default:
			return false;
		}
	}

	public override void GetInfoStrings(ref List<string> list)
	{
		list.Add(string.Format("stat '{1}' {0} {2} {3}", new object[]
		{
			this.invert ? "NOT" : "",
			this.stat.ToStringCached<StatCompareCurrent.StatTypes>(),
			this.operation.ToStringCached<RequirementBase.OperationTypes>(),
			this.value.ToCultureInvariantString()
		}));
	}

	public override bool ParseXAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXAttribute(_attribute);
		if (!flag && _attribute.Name.LocalName == "stat")
		{
			this.stat = EnumUtils.Parse<StatCompareCurrent.StatTypes>(_attribute.Value, true);
			return true;
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public StatCompareCurrent.StatTypes stat;

	[PublicizedFrom(EAccessModifier.Protected)]
	public enum StatTypes
	{
		None,
		Health,
		Stamina,
		Food,
		Water,
		Armor
	}
}

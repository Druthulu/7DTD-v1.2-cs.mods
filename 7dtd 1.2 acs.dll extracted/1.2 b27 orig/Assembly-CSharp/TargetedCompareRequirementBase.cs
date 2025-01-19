using System;
using System.Xml.Linq;

public class TargetedCompareRequirementBase : RequirementBase
{
	public override bool IsValid(MinEventParams _params)
	{
		return this.ParamsValid(_params);
	}

	public override bool ParamsValid(MinEventParams _params)
	{
		if (!base.ParamsValid(_params))
		{
			return false;
		}
		this.target = null;
		if (this.targetType == TargetedCompareRequirementBase.TargetTypes.other)
		{
			if (_params.Other != null)
			{
				this.target = _params.Other;
			}
		}
		else if (_params.Self != null)
		{
			this.target = _params.Self;
		}
		return this.target != null;
	}

	public override bool ParseXAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXAttribute(_attribute);
		if (!flag && _attribute.Name.LocalName == "target")
		{
			this.targetType = EnumUtils.Parse<TargetedCompareRequirementBase.TargetTypes>(_attribute.Value, true);
			return true;
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public TargetedCompareRequirementBase.TargetTypes targetType;

	[PublicizedFrom(EAccessModifier.Protected)]
	public EntityAlive target;

	[PublicizedFrom(EAccessModifier.Protected)]
	public enum TargetTypes
	{
		self,
		other
	}
}

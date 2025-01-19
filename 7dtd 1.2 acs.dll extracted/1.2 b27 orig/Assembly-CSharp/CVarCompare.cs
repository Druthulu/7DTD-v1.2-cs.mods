using System;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class CVarCompare : TargetedCompareRequirementBase
{
	public override bool IsValid(MinEventParams _params)
	{
		if (!this.ParamsValid(_params))
		{
			return false;
		}
		if (this.invert)
		{
			return !RequirementBase.compareValues(this.target.Buffs.GetCustomVar(this.cvarName, 0f), this.operation, this.value);
		}
		return RequirementBase.compareValues(this.target.Buffs.GetCustomVar(this.cvarName, 0f), this.operation, this.value);
	}

	public override void GetInfoStrings(ref List<string> list)
	{
		list.Add(string.Format("cvar.{0} {1} {2}", this.cvarName, this.operation.ToStringCached<RequirementBase.OperationTypes>(), this.value.ToCultureInvariantString()));
	}

	public override bool ParseXAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXAttribute(_attribute);
		if (!flag && _attribute.Name.LocalName == "cvar")
		{
			this.cvarName = _attribute.Value;
			this.cvarNameHash = this.cvarName.GetHashCode();
			return true;
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public string cvarName;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int cvarNameHash;
}

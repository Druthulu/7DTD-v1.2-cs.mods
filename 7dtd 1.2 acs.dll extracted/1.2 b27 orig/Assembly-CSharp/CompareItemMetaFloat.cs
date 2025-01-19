using System;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class CompareItemMetaFloat : TargetedCompareRequirementBase
{
	public override bool IsValid(MinEventParams _params)
	{
		if (!this.ParamsValid(_params))
		{
			return false;
		}
		ItemValue itemValue = _params.ItemValue;
		if (itemValue == null || string.IsNullOrEmpty(this.metaKey))
		{
			return false;
		}
		object metadata = itemValue.GetMetadata(this.metaKey);
		if (!(metadata is float))
		{
			return false;
		}
		if (this.invert)
		{
			return !RequirementBase.compareValues((float)metadata, this.operation, this.value);
		}
		return RequirementBase.compareValues((float)metadata, this.operation, this.value);
	}

	public override bool ParseXAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXAttribute(_attribute);
		if (!flag && _attribute.Name.LocalName == "key")
		{
			this.metaKey = _attribute.Value;
			return true;
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string metaKey;
}

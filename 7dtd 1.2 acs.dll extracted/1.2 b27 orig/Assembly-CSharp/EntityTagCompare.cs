﻿using System;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class EntityTagCompare : TargetedCompareRequirementBase
{
	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		if (this.hasAllTags)
		{
			if (!this.invert)
			{
				return this.target.HasAllTags(this.tagsToCompare);
			}
			return !this.target.HasAllTags(this.tagsToCompare);
		}
		else
		{
			if (!this.invert)
			{
				return this.target.HasAnyTags(this.tagsToCompare);
			}
			return !this.target.HasAnyTags(this.tagsToCompare);
		}
	}

	public override bool ParseXAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXAttribute(_attribute);
		if (!flag)
		{
			string localName = _attribute.Name.LocalName;
			if (localName == "tags")
			{
				this.tagsToCompare = FastTags<TagGroup.Global>.Parse(_attribute.Value);
				return true;
			}
			if (localName == "has_all_tags")
			{
				this.hasAllTags = StringParsers.ParseBool(_attribute.Value, 0, -1, true);
				return true;
			}
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Global> tagsToCompare;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasAllTags;
}

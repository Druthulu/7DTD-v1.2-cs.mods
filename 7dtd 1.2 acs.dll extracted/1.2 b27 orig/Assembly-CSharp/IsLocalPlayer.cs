﻿using System;
using UnityEngine.Scripting;

[Preserve]
public class IsLocalPlayer : TargetedCompareRequirementBase
{
	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		if (!this.invert)
		{
			return this.target as EntityPlayerLocal != null;
		}
		return !(this.target as EntityPlayerLocal != null);
	}
}

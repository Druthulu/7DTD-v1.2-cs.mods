﻿using System;
using UnityEngine.Scripting;

[Preserve]
public class DialogRequirementBuff : BaseDialogRequirement
{
	public override BaseDialogRequirement.RequirementTypes RequirementType
	{
		get
		{
			return BaseDialogRequirement.RequirementTypes.Buff;
		}
	}

	public override void SetupRequirement()
	{
		string arg = Localization.Get("RequirementBuff_keyword", false);
		base.Description = string.Format("{0} {1}", arg, BuffManager.GetBuff(base.ID).Name);
	}

	public override bool CheckRequirement(EntityPlayer player, EntityNPC talkingTo)
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string name = "";
}

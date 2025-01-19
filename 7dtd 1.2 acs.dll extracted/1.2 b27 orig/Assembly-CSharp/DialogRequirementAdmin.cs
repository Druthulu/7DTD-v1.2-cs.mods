using System;
using UnityEngine.Scripting;

[Preserve]
public class DialogRequirementAdmin : BaseDialogRequirement
{
	public override BaseDialogRequirement.RequirementTypes RequirementType
	{
		get
		{
			return BaseDialogRequirement.RequirementTypes.Admin;
		}
	}

	public override void SetupRequirement()
	{
		string description = Localization.Get("RequirementAdmin_keyword", false);
		base.Description = description;
	}

	public override bool CheckRequirement(EntityPlayer player, EntityNPC talkingTo)
	{
		return GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string name = "";
}

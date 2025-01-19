using System;
using UnityEngine.Scripting;

[Preserve]
public class DialogRequirementDroneState : BaseDialogRequirement
{
	public override BaseDialogRequirement.RequirementTypes RequirementType
	{
		get
		{
			return BaseDialogRequirement.RequirementTypes.DroneState;
		}
	}

	public override bool CheckRequirement(EntityPlayer player, EntityNPC talkingTo)
	{
		EntityDrone entityDrone = talkingTo as EntityDrone;
		if (entityDrone)
		{
			EntityDrone.Orders orders;
			if (Enum.TryParse<EntityDrone.Orders>(base.Value, out orders))
			{
				return entityDrone.OrderState == orders;
			}
			EntityDrone.AllyHealMode allyHealMode;
			if (Enum.TryParse<EntityDrone.AllyHealMode>(base.Value, out allyHealMode) && entityDrone.IsHealModAttached)
			{
				return entityDrone.HealAllyMode == allyHealMode;
			}
			bool flag = entityDrone.TargetCanBeHealed(player);
			if (flag)
			{
				return flag;
			}
		}
		return false;
	}
}

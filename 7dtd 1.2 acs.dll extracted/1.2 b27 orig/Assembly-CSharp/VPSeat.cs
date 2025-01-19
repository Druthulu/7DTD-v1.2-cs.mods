using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class VPSeat : VehiclePart
{
	public override void InitPrefabConnections()
	{
		base.InitIKTarget(AvatarIKGoal.LeftHand, null);
		base.InitIKTarget(AvatarIKGoal.RightHand, null);
		base.InitIKTarget(AvatarIKGoal.LeftFoot, null);
		base.InitIKTarget(AvatarIKGoal.RightFoot, null);
	}
}

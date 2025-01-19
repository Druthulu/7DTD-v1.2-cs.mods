using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class VPSteering : VehiclePart
{
	public override void InitPrefabConnections()
	{
		StringParsers.TryParseFloat(base.GetProperty("steerMaxAngle"), out this.steerMaxAngle, 0, -1, NumberStyles.Any);
		this.properties.ParseVec("steerAngle", ref this.steerAngles);
		this.steeringJoint = base.GetTransform();
		if (this.steeringJoint)
		{
			this.baseRotation = this.steeringJoint.localRotation;
		}
		base.InitIKTarget(AvatarIKGoal.LeftHand, this.steeringJoint);
		base.InitIKTarget(AvatarIKGoal.RightHand, this.steeringJoint);
	}

	public override void Update(float _dt)
	{
		if (this.steerMaxAngle != 0f)
		{
			this.steeringJoint.localRotation = this.baseRotation * Quaternion.AngleAxis(this.vehicle.CurrentSteeringPercent * this.steerMaxAngle, Vector3.up);
		}
		if (this.steerAngles.sqrMagnitude != 0f)
		{
			float currentSteeringPercent = this.vehicle.CurrentSteeringPercent;
			this.steeringJoint.localRotation = this.baseRotation * Quaternion.Euler(currentSteeringPercent * this.steerAngles.x, currentSteeringPercent * this.steerAngles.y, currentSteeringPercent * this.steerAngles.z);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform steeringJoint;

	[PublicizedFrom(EAccessModifier.Private)]
	public Quaternion baseRotation;

	[PublicizedFrom(EAccessModifier.Private)]
	public float steerMaxAngle;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 steerAngles;
}

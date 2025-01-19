using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EntityVBlimp : EntityDriveable
{
	public override void Init(int _entityClass)
	{
		base.Init(_entityClass);
		this.vehicleRB.useGravity = false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void PhysicsInputMove()
	{
		this.vehicleRB.velocity *= 0.996f;
		this.vehicleRB.velocity += new Vector3(0f, -0.001f, 0f);
		this.vehicleRB.angularVelocity *= 0.98f;
		if (this.movementInput != null)
		{
			float num = 2f;
			if (this.movementInput.running)
			{
				num *= 6f;
			}
			this.wheelMotor = this.movementInput.moveForward;
			this.vehicleRB.AddRelativeForce(0f, 0f, this.wheelMotor * num * 0.05f, ForceMode.VelocityChange);
			float num2;
			if (this.movementInput.lastInputController)
			{
				num2 = this.movementInput.moveStrafe * num;
			}
			else
			{
				num2 = this.movementInput.moveStrafe * num;
			}
			this.vehicleRB.AddRelativeTorque(0f, num2 * 0.01f, 0f, ForceMode.VelocityChange);
			if (this.movementInput.jump)
			{
				this.vehicleRB.AddRelativeForce(0f, 0.02f * num, 0f, ForceMode.VelocityChange);
			}
			if (this.movementInput.down)
			{
				this.vehicleRB.AddRelativeForce(0f, -0.02f * num, 0f, ForceMode.VelocityChange);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void SetWheelsForces(float motorTorque, float motorTorqueBase, float brakeTorque)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateWheelsSteering()
	{
	}
}

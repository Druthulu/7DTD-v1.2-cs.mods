using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Audio;
using InControl;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[UnityEngine.Scripting.Preserve]
public class EntityVehicle : EntityAlive, ILockable
{
	public override Entity.EnumPositionUpdateMovementType positionUpdateMovementType
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return Entity.EnumPositionUpdateMovementType.Instant;
		}
	}

	public override bool IsValidAimAssistSnapTarget
	{
		get
		{
			return false;
		}
	}

	public override bool IsValidAimAssistSlowdownTarget
	{
		get
		{
			return false;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		this.bag = new Bag(this);
		base.Awake();
		this.isLocked = true;
	}

	public override void Init(int _entityClass)
	{
		base.Init(_entityClass);
		EntityClass entityClass = EntityClass.list[this.entityClass];
		this.vehicle = new Vehicle(entityClass.entityClassName, this);
		base.transform.tag = "E_Vehicle";
		Vector2i size = LootContainer.GetLootContainer(this.GetLootList(), true).size;
		this.bag.SetupSlots(ItemStack.CreateArray(size.x * size.y));
		Transform physicsTransform = this.PhysicsTransform;
		this.vehicleRB = physicsTransform.GetComponent<Rigidbody>();
		if (this.vehicleRB)
		{
			if (this.vehicleRB.automaticCenterOfMass)
			{
				this.vehicleRB.centerOfMass = new Vector3(0f, 0.1f, 0f);
			}
			this.vehicleRB.sleepThreshold = this.vehicleRB.mass * 0.01f * 0.01f * 0.5f;
			physicsTransform.gameObject.AddComponent<CollisionCallForward>().Entity = this;
			physicsTransform.gameObject.layer = 21;
			Utils.SetTagsIfNoneRecursively(physicsTransform, "E_Vehicle");
			this.SetupDevices();
			this.SetVehicleDriven();
			if (!this.isEntityRemote)
			{
				this.isTryToFall = true;
			}
		}
		this.alertEnabled = false;
		GameManager.Instance.StartCoroutine(this.ApplyCollisionsCoroutine());
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void AddCharacterController()
	{
	}

	public override void PostInit()
	{
		this.LogVehicle("PostInit {0}, {1} (chunk {2}), rbPos {3}", new object[]
		{
			this,
			this.position,
			World.toChunkXZ(this.position),
			this.vehicleRB.position + Origin.position
		});
		base.transform.rotation = this.qrotation;
		if (this.vehicleRB)
		{
			this.PhysicsResetAndSleep();
			this.PhysicsTransform.rotation = this.qrotation;
			this.SetVehicleDriven();
		}
		this.HandleNavObject();
	}

	public override void InitInventory()
	{
		this.inventory = new EntityVehicle.VehicleInventory(GameManager.Instance, this);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupDevices()
	{
		this.SetupMotors();
		this.SetupForces();
		this.SetupWheels();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupForces()
	{
		DynamicProperties properties = this.vehicle.Properties;
		if (properties == null)
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		DynamicProperties dynamicProperties;
		while (num2 < 99 && properties.Classes.TryGetValue("force" + num2.ToString(), out dynamicProperties))
		{
			num++;
			num2++;
		}
		this.forces = new EntityVehicle.Force[num];
		for (int i = 0; i < this.forces.Length; i++)
		{
			EntityVehicle.Force force = new EntityVehicle.Force();
			this.forces[i] = force;
			DynamicProperties dynamicProperties2 = properties.Classes["force" + i.ToString()];
			force.ceiling.x = 9999f;
			force.ceiling.y = 9999f;
			dynamicProperties2.ParseVec("ceiling", ref force.ceiling);
			force.ceiling.y = 1f / Utils.FastMax(0.5f, force.ceiling.y - force.ceiling.x);
			force.force = Vector3.forward;
			dynamicProperties2.ParseVec("force", ref force.force);
			force.trigger = EntityVehicle.Force.Trigger.On;
			dynamicProperties2.ParseEnum<EntityVehicle.Force.Trigger>("trigger", ref force.trigger);
			force.type = EntityVehicle.Force.Type.Relative;
			dynamicProperties2.ParseEnum<EntityVehicle.Force.Type>("type", ref force.type);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupMotors()
	{
		DynamicProperties properties = this.vehicle.Properties;
		if (properties == null)
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		DynamicProperties dynamicProperties;
		while (num2 < 99 && properties.Classes.TryGetValue("motor" + num2.ToString(), out dynamicProperties))
		{
			num++;
			num2++;
		}
		this.motors = new EntityVehicle.Motor[num];
		Transform meshTransform = this.vehicle.GetMeshTransform();
		for (int i = 0; i < this.motors.Length; i++)
		{
			EntityVehicle.Motor motor = new EntityVehicle.Motor();
			this.motors[i] = motor;
			DynamicProperties dynamicProperties2 = properties.Classes["motor" + i.ToString()];
			string @string = dynamicProperties2.GetString("engine");
			if (@string.Length > 0)
			{
				motor.engine = (this.vehicle.FindPart(@string) as VPEngine);
			}
			motor.engineOffPer = 0f;
			dynamicProperties2.ParseFloat("engineOffPer", ref motor.engineOffPer);
			motor.turbo = 1f;
			dynamicProperties2.ParseFloat("turbo", ref motor.turbo);
			motor.rpmAccelMin = 1f;
			motor.rpmAccelMax = 1f;
			dynamicProperties2.ParseVec("rpmAccel_min_max", ref motor.rpmAccelMin, ref motor.rpmAccelMax);
			motor.rpmDrag = 1f;
			dynamicProperties2.ParseFloat("rpmDrag", ref motor.rpmDrag);
			motor.rpmMax = 1f;
			dynamicProperties2.ParseFloat("rpmMax", ref motor.rpmMax);
			if (motor.rpmMax == 0f)
			{
				motor.rpmMax = 0.001f;
			}
			motor.trigger = EntityVehicle.Motor.Trigger.On;
			dynamicProperties2.ParseEnum<EntityVehicle.Motor.Trigger>("trigger", ref motor.trigger);
			string string2 = dynamicProperties2.GetString("transform");
			if (string2.Length > 0)
			{
				motor.transform = meshTransform.Find(string2);
			}
			float num3 = 0f;
			dynamicProperties2.ParseFloat("axis", ref num3);
			motor.axis = (int)num3;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupWheels()
	{
		DynamicProperties properties = this.vehicle.Properties;
		if (properties == null)
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		DynamicProperties dynamicProperties;
		while (num2 < 99 && properties.Classes.TryGetValue("wheel" + num2.ToString(), out dynamicProperties))
		{
			num++;
			num2++;
		}
		this.wheels = new EntityVehicle.Wheel[num];
		Transform physicsTransform = this.PhysicsTransform;
		Transform meshTransform = this.vehicle.GetMeshTransform();
		for (int i = 0; i < this.wheels.Length; i++)
		{
			EntityVehicle.Wheel wheel = new EntityVehicle.Wheel();
			this.wheels[i] = wheel;
			Transform transform = physicsTransform.Find("Wheel" + i.ToString());
			wheel.wheelC = transform.GetComponent<WheelCollider>();
			DynamicProperties dynamicProperties2 = properties.Classes["wheel" + i.ToString()];
			wheel.motorTorqueScale = 1f;
			wheel.brakeTorqueScale = 1f;
			dynamicProperties2.ParseVec("torqueScale_motor_brake", ref wheel.motorTorqueScale, ref wheel.brakeTorqueScale);
			wheel.bounceSound = "vwheel_bounce";
			dynamicProperties2.ParseString("bounceSound", ref wheel.bounceSound);
			wheel.slideSound = "vwheel_slide";
			dynamicProperties2.ParseString("slideSound", ref wheel.slideSound);
			string @string = dynamicProperties2.GetString("steerTransform");
			if (@string.Length > 0)
			{
				wheel.steerT = meshTransform.Find(@string);
				if (wheel.steerT)
				{
					wheel.steerBaseRot = wheel.steerT.localRotation;
				}
			}
			string string2 = dynamicProperties2.GetString("tireTransform");
			if (string2.Length > 0)
			{
				wheel.tireT = meshTransform.Find(string2);
			}
			wheel.isSteerParentOfTire = (wheel.steerT != wheel.tireT);
			if (dynamicProperties2.GetString("tireSuspensionPercent").Length > 0)
			{
				wheel.tireSuspensionPercent = 1f;
			}
		}
	}

	public override void OnXMLChanged()
	{
		this.vehicle.OnXMLChanged();
		this.SetupDevices();
	}

	public new void FixedUpdate()
	{
		this.PhysicsFixedUpdate();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PhysicsResetAndSleep()
	{
		Rigidbody rigidbody = this.vehicleRB;
		Transform physicsTransform = this.PhysicsTransform;
		Vector3 position = this.position - Origin.position;
		physicsTransform.position = position;
		rigidbody.position = position;
		Quaternion rotation = this.ModelTransform.rotation;
		physicsTransform.rotation = rotation;
		rigidbody.rotation = rotation;
		if (!this.vehicleRB.isKinematic)
		{
			rigidbody.velocity = Vector3.zero;
			rigidbody.angularVelocity = Vector3.zero;
			rigidbody.Sleep();
		}
		this.SetWheelsForces(0f, 1f, 0f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PhysicsFixedUpdate()
	{
		float deltaTime = Time.deltaTime;
		Rigidbody rigidbody = this.vehicleRB;
		Transform physicsTransform = this.PhysicsTransform;
		this.wheelMotor = 0f;
		this.wheelBrakes = 0f;
		if (this.isEntityRemote)
		{
			this.vehicleRB.isKinematic = true;
			Vector3 position = Vector3.Lerp(physicsTransform.position, this.position - Origin.position, 0.5f);
			physicsTransform.position = position;
			physicsTransform.rotation = Quaternion.Slerp(physicsTransform.rotation, this.ModelTransform.rotation, 0.3f);
			if (this.incomingRemoteData.Flags > 0)
			{
				this.lastRemoteData = this.currentRemoteData;
				this.currentRemoteData = this.incomingRemoteData;
				this.incomingRemoteData.Flags = 0;
				this.syncPlayTime = 0f;
				this.vehicle.CurrentIsAccel = ((this.currentRemoteData.Flags & 2) > 0);
				this.vehicle.CurrentIsBreak = ((this.currentRemoteData.Flags & 4) > 0);
			}
			if (this.syncPlayTime >= 0f)
			{
				float num = this.syncPlayTime / 0.5f;
				this.syncPlayTime += deltaTime;
				if (num >= 1f)
				{
					num = 1f;
					this.syncPlayTime = -1f;
				}
				float num2 = Mathf.Lerp(this.lastRemoteData.SteeringPercent, this.currentRemoteData.SteeringPercent, num);
				this.vehicle.CurrentSteeringPercent = num2;
				float currentMotorTorquePercent = Mathf.Lerp(this.lastRemoteData.MotorTorquePercent, this.currentRemoteData.MotorTorquePercent, num);
				this.vehicle.CurrentMotorTorquePercent = currentMotorTorquePercent;
				Vector3 vector = Vector3.Lerp(this.lastRemoteData.Velocity, this.currentRemoteData.Velocity, num);
				this.vehicle.CurrentVelocity = vector;
				this.vehicle.CurrentForwardVelocity = Vector3.Dot(vector, physicsTransform.forward);
				this.wheelDir = num2 * this.vehicle.SteerAngleMax;
				this.FixedUpdateMotors();
				this.vehicle.UpdateSimulation();
				int num3 = this.wheels.Length;
				if (num3 > 0 && this.lastRemoteData.parts != null)
				{
					int num4 = 0;
					for (int i = 0; i < num3; i++)
					{
						EntityVehicle.Wheel wheel = this.wheels[i];
						Transform steerT = wheel.steerT;
						if (steerT && wheel.isSteerParentOfTire)
						{
							Quaternion localRotation = Quaternion.Lerp(this.lastRemoteData.parts[num4].rot, this.currentRemoteData.parts[num4].rot, num);
							steerT.localRotation = localRotation;
							num4++;
						}
						Transform tireT = wheel.tireT;
						if (tireT)
						{
							Vector3 localPosition = Vector3.Lerp(this.lastRemoteData.parts[num4].pos, this.currentRemoteData.parts[num4].pos, num);
							tireT.localPosition = localPosition;
							Quaternion localRotation2 = Quaternion.Lerp(this.lastRemoteData.parts[num4].rot, this.currentRemoteData.parts[num4].rot, num);
							tireT.localRotation = localRotation2;
							num4++;
						}
					}
				}
			}
			return;
		}
		this.CheckForOutOfWorld();
		if (!this.RBActive)
		{
			this.PhysicsResetAndSleep();
			this.vehicleRB.isKinematic = true;
			return;
		}
		this.vehicleRB.isKinematic = false;
		if (!this.hasDriver)
		{
			Vector3 vector2 = rigidbody.velocity;
			vector2.x *= 0.98f;
			vector2.z *= 0.98f;
			if (this.GetWheelsOnGround() > 0)
			{
				this.RBNoDriverGndTime += deltaTime;
				float f = this.RBNoDriverGndTime / 8f;
				float num5 = Utils.FastLerp(0.6f, 1f, (0.5f - physicsTransform.up.y) / 0.5f);
				num5 = Utils.FastLerp(1f, num5, Mathf.Pow(f, 3f));
				vector2.x *= num5;
				vector2.z *= num5;
			}
			if (this.collisionGrazeCount >= 2)
			{
				float num6 = vector2.magnitude * 1.4f;
				if (num6 < 1f)
				{
					float num7 = Utils.FastLerpUnclamped((float)Utils.FastMin(3, this.collisionGrazeCount) * 0.29f, 0f, num6);
					vector2 *= 1f - num7;
					rigidbody.angularVelocity *= 1f - num7 * 0.65f;
				}
			}
			vector2.y *= this.vehicle.AirDragVelScale;
			rigidbody.velocity = vector2;
			if (vector2.sqrMagnitude < 0.0100000007f && rigidbody.angularVelocity.sqrMagnitude < 0.0049f)
			{
				this.RBNoDriverSleepTime += deltaTime;
				if (this.RBNoDriverSleepTime >= 3f)
				{
					this.RBActive = false;
					this.RBNoDriverSleepTime = 0f;
				}
			}
			else
			{
				this.RBNoDriverSleepTime = 0f;
			}
			this.collisionGrazeCount = 0;
		}
		Vector3 vector3 = this.vehicleRB.velocity;
		float num8 = this.vehicle.MotorTorqueForward;
		float num9 = this.vehicle.VelocityMaxForward;
		this.vehicle.IsTurbo = false;
		if (this.movementInput != null)
		{
			if (this.movementInput.moveForward < 0f)
			{
				num8 = this.vehicle.MotorTorqueBackward;
				num9 = this.vehicle.VelocityMaxBackward;
			}
			if (this.movementInput.running && this.vehicle.CanTurbo && this.movementInput.moveForward != 0f)
			{
				this.vehicle.IsTurbo = true;
				num8 = this.vehicle.MotorTorqueTurboForward;
				num9 = this.vehicle.VelocityMaxTurboForward;
				if (this.movementInput.moveForward < 0f)
				{
					num8 = this.vehicle.MotorTorqueTurboBackward;
					num9 = this.vehicle.VelocityMaxTurboBackward;
				}
			}
		}
		num8 *= this.vehicle.EffectMotorTorquePer;
		num9 *= this.vehicle.EffectVelocityMaxPer;
		float num10 = (num9 > this.velocityMax) ? 2.5f : 1.5f;
		num9 = Mathf.MoveTowards(this.velocityMax, num9, num10 * deltaTime);
		this.velocityMax = num9;
		if (this.CalcWaterDepth(this.vehicle.WaterDragY) > 0f)
		{
			this.timeInWater += deltaTime;
			if (this.vehicle.WaterDragVelScale != 1f)
			{
				vector3 *= this.vehicle.WaterDragVelScale;
			}
			if (this.vehicle.WaterDragVelMaxScale != 1f)
			{
				num9 = Mathf.Lerp(num9, num9 * this.vehicle.WaterDragVelMaxScale, this.timeInWater * 0.5f);
			}
		}
		else
		{
			this.timeInWater = 0f;
		}
		float num11 = Mathf.Sqrt(vector3.x * vector3.x + vector3.z * vector3.z);
		if (num11 > num9)
		{
			float num12 = num9 / num11;
			vector3.x *= num12;
			vector3.z *= num12;
			this.vehicleRB.velocity = vector3;
		}
		float magnitude = vector3.magnitude;
		if (this.vehicle.WaterLiftForce > 0f)
		{
			float num13 = this.CalcWaterDepth(this.vehicle.WaterLiftY);
			if (num13 > 0f)
			{
				float y = Mathf.Lerp(this.vehicle.WaterLiftForce * 0.05f, this.vehicle.WaterLiftForce, num13 / (this.vehicle.WaterLiftDepth + 0.001f));
				this.vehicleRB.AddForce(new Vector3(0f, y, 0f), ForceMode.VelocityChange);
			}
		}
		float num14 = -this.lastRBVel.y;
		if (num14 > 8f && (magnitude < num14 * 0.45f || Vector3.Dot(this.lastRBVel.normalized, vector3.normalized) < 0.2f))
		{
			int num15 = (int)((num14 - 8f) * 4f + 0.999f);
			this.ApplyDamage(num15 * 10);
			this.ApplyCollisionDamageToAttached(num15);
		}
		this.lastRBPos = this.vehicleRB.position;
		this.lastRBRot = this.vehicleRB.rotation;
		this.lastRBVel = vector3;
		this.lastRBAngVel = this.vehicleRB.angularVelocity;
		float num16 = Vector3.Dot(vector3, physicsTransform.forward);
		this.vehicle.CurrentForwardVelocity = num16;
		this.motorTorque = 0f;
		this.brakeTorque = 0f;
		if (this.wheels.Length != 0)
		{
			if (this.movementInput != null)
			{
				float num17 = Mathf.Pow(magnitude * 0.1f, 2f);
				float num18 = Mathf.Clamp(1f - num17, 0.15f, 1f);
				this.wheelMotor = this.movementInput.moveForward;
				float steerAngleMax = this.vehicle.SteerAngleMax;
				float num19 = this.vehicle.SteerRate * num18 * deltaTime;
				if (EntityVehicle.isTurnTowardsLook)
				{
					float num20 = 0f;
					if (!Input.GetMouseButton(1))
					{
						vp_FPCamera vp_FPCamera = base.GetAttachedPlayerLocal().vp_FPCamera;
						Vector3 forward = base.transform.forward;
						forward.y = 0f;
						Vector3 forward2 = vp_FPCamera.Forward;
						forward2.y = 0f;
						num20 = Vector3.SignedAngle(forward, forward2, Vector3.up);
						if (num16 < -0.02f)
						{
							if (Mathf.Abs(num20) > 90f)
							{
								num20 += 180f;
								if (num20 > 180f)
								{
									num20 -= 360f;
								}
							}
							num20 = -num20;
						}
					}
					float num21 = num19 * 1.2f;
					if ((this.wheelDir < 0f && this.wheelDir < num20) || (this.wheelDir > 0f && this.wheelDir > num20))
					{
						num21 *= 3f;
					}
					this.wheelDir = Mathf.MoveTowards(this.wheelDir, num20, num21);
					this.wheelDir = Mathf.Clamp(this.wheelDir, -steerAngleMax, steerAngleMax);
				}
				else if (this.movementInput.lastInputController)
				{
					this.wheelDir = Mathf.MoveTowards(this.wheelDir, this.movementInput.moveStrafe * steerAngleMax, num19 * 1.5f);
				}
				else
				{
					float moveStrafe = this.movementInput.moveStrafe;
					float num22 = 0f;
					if (moveStrafe < 0f)
					{
						if (this.wheelDir > 0f)
						{
							num22 -= num19 * num17;
						}
						num22 -= num19;
					}
					if (moveStrafe > 0f)
					{
						if (this.wheelDir < 0f)
						{
							num22 += num19 * num17;
						}
						num22 += num19;
					}
					this.wheelDir += num22;
					this.wheelDir = Mathf.Clamp(this.wheelDir, -steerAngleMax, steerAngleMax);
					if (moveStrafe == 0f)
					{
						this.wheelDir = Mathf.MoveTowards(this.wheelDir, 0f, this.vehicle.SteerCenteringRate * deltaTime);
					}
				}
				if (this.wheelMotor != 0f)
				{
					if (this.wheelMotor > 0f)
					{
						if (num16 < -0.5f)
						{
							this.wheelBrakes = 1f;
						}
					}
					else if (num16 > 0.5f)
					{
						this.wheelBrakes = 1f;
					}
					if (!this.movementInput.running)
					{
						this.wheelMotor *= 0.5f;
					}
				}
				if (this.movementInput.jump)
				{
					this.wheelBrakes = 2f;
				}
				if (this.canHop)
				{
					if (this.movementInput.down && this.GetWheelsOnGround() > 0)
					{
						this.canHop = false;
						Vector3 force = Vector3.Slerp(Vector3.up, physicsTransform.up, 0.5f) * this.vehicle.HopForce.x;
						this.vehicleRB.AddForceAtPosition(force, this.vehicleRB.position + physicsTransform.forward * this.vehicle.HopForce.y, ForceMode.VelocityChange);
					}
				}
				else if (!this.movementInput.down)
				{
					this.canHop = true;
				}
			}
			if (this.wheelMotor != 0f)
			{
				if (this.vehicle.HasEnginePart())
				{
					if (this.IsEngineRunning)
					{
						this.motorTorque = this.wheelMotor * num8;
					}
					else
					{
						this.motorTorque = this.wheelMotor * 50f;
					}
				}
				else if (this.vehicle.GetHealth() > 0)
				{
					this.motorTorque = this.wheelMotor * num8;
				}
				else
				{
					this.motorTorque = this.wheelMotor * 10f;
					if (this.rand.RandomFloat < 0.2f)
					{
						this.vehicleRB.AddRelativeForce(0.15f * this.rand.RandomOnUnitSphere, ForceMode.VelocityChange);
					}
					this.wheelDir = Mathf.Clamp(this.wheelDir + (this.rand.RandomFloat * 2f - 1f) * 5f, -this.vehicle.SteerAngleMax, this.vehicle.SteerAngleMax);
				}
				if (magnitude < 0.15f && this.wheelBrakes == 0f && Utils.FastAbs(physicsTransform.up.y) > 0.34f)
				{
					Vector3 force2 = Quaternion.Euler(0f, this.wheelDir, 0f) * (this.vehicle.UnstickForce * Mathf.Sign(this.wheelMotor) * Vector3.forward);
					this.vehicleRB.AddRelativeForce(force2, ForceMode.VelocityChange);
				}
			}
			this.brakeTorque = this.wheelBrakes * this.vehicle.BrakeTorque;
			this.SetWheelsForces(this.motorTorque, num8, this.brakeTorque);
			this.UpdateWheelsCollision();
			this.UpdateWheelsSteering();
		}
		this.vehicleRB.velocity *= this.vehicle.AirDragVelScale;
		this.vehicleRB.angularVelocity *= this.vehicle.AirDragAngVelScale;
		this.PhysicsInputMove();
		this.FixedUpdateMotors();
		this.FixedUpdateForces();
		if (this.hasDriver || base.GetFirstAttached())
		{
			if (this.vehicle.TiltUpForce > 0f)
			{
				Vector3 right = physicsTransform.right;
				Mathf.Abs(right.y);
				float num23 = Mathf.Asin(right.y) * 57.29578f;
				float num24 = this.wheelDir / this.vehicle.SteerAngleMax;
				num24 *= 2f;
				num24 = Mathf.LerpUnclamped(0f, num24, Mathf.Pow(magnitude * 0.1f, 2f));
				float tiltAngleMax = this.vehicle.TiltAngleMax;
				num24 = Mathf.Clamp(num24 * tiltAngleMax, -tiltAngleMax, tiltAngleMax);
				float f2 = num23 + num24;
				float num25 = Mathf.Abs(f2);
				if (num25 > this.vehicle.TiltThreshold)
				{
					float num26 = (num25 - this.vehicle.TiltThreshold) * Mathf.Sign(f2) * 0.01f * -this.vehicle.TiltUpForce;
					num26 = Mathf.Clamp(num26, -4f, 4f);
					this.vehicleRB.AddRelativeTorque(0f, 0f, num26, ForceMode.VelocityChange);
				}
				if (num25 < this.vehicle.TiltDampenThreshold)
				{
					Vector3 angularVelocity = this.vehicleRB.angularVelocity;
					float magnitude2 = angularVelocity.magnitude;
					if (magnitude2 > 0f)
					{
						Vector3 rhs = angularVelocity * (1f / magnitude2);
						float num27 = Mathf.Abs(Vector3.Dot(base.transform.forward, rhs));
						this.vehicleRB.angularVelocity -= angularVelocity * (0.02f + this.vehicle.TiltDampening * num27);
					}
				}
			}
			if (this.vehicle.UpForce > 0f)
			{
				Vector3 up = physicsTransform.up;
				float num28 = Mathf.Abs(Mathf.Acos(up.y) * 57.29578f) - this.vehicle.UpAngleMax;
				if (num28 > 0f)
				{
					float num29 = num28 / 90f;
					Vector3 torque = Vector3.Cross(up, Vector3.up) * (num29 * num29 * this.vehicle.UpForce);
					this.vehicleRB.AddRelativeTorque(torque, ForceMode.VelocityChange);
				}
			}
		}
		Vector3 position2 = physicsTransform.position;
		this.SetPosition(position2 + Origin.position, false);
		this.qrotation = physicsTransform.rotation;
		this.rotation = this.qrotation.eulerAngles;
		this.ModelTransform.rotation = this.qrotation;
		this.vehicle.CurrentIsAccel = (this.motorTorque != 0f && this.brakeTorque == 0f);
		this.vehicle.CurrentIsBreak = (this.brakeTorque != 0f);
		this.vehicle.CurrentSteeringPercent = this.wheelDir / this.vehicle.SteerAngleMax;
		this.vehicle.CurrentVelocity = this.vehicleRB.velocity;
		this.vehicle.UpdateSimulation();
		if (!this.isEntityRemote)
		{
			this.syncHighRateTime += deltaTime;
			if (this.syncHighRateTime >= 0.5f)
			{
				this.SendSyncData(32768);
				this.syncHighRateTime = 0f;
			}
			this.syncLowRateTime += deltaTime;
			if (this.syncLowRateTime >= 2f)
			{
				this.SendSyncData(16384);
				this.syncLowRateTime = 0f;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void PhysicsInputMove()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FixedUpdateForces()
	{
		if (this.movementInput == null)
		{
			return;
		}
		float num = 1f;
		for (int i = 0; i < this.forces.Length; i++)
		{
			EntityVehicle.Force force = this.forces[i];
			float num2 = 1f;
			switch (force.trigger)
			{
			case EntityVehicle.Force.Trigger.Off:
				num2 = 0f;
				break;
			case EntityVehicle.Force.Trigger.InputForward:
				num2 = this.movementInput.moveForward;
				break;
			case EntityVehicle.Force.Trigger.InputStrafe:
				num2 = this.movementInput.moveStrafe;
				break;
			case EntityVehicle.Force.Trigger.InputUp:
				num2 = (float)(this.movementInput.jump ? 1 : 0);
				break;
			case EntityVehicle.Force.Trigger.InputDown:
				num2 = (float)(this.movementInput.down ? 1 : 0);
				break;
			case EntityVehicle.Force.Trigger.Motor0:
			case EntityVehicle.Force.Trigger.Motor1:
			case EntityVehicle.Force.Trigger.Motor2:
			case EntityVehicle.Force.Trigger.Motor3:
			case EntityVehicle.Force.Trigger.Motor4:
			case EntityVehicle.Force.Trigger.Motor5:
			case EntityVehicle.Force.Trigger.Motor6:
			case EntityVehicle.Force.Trigger.Motor7:
			{
				EntityVehicle.Motor motor = this.motors[force.trigger - EntityVehicle.Force.Trigger.Motor0];
				num2 = motor.rpm / motor.rpmMax;
				break;
			}
			}
			if (num2 != 0f)
			{
				num2 *= num;
				float num3 = this.position.y - force.ceiling.x;
				if (num3 > 0f)
				{
					num2 *= Utils.FastMax(0f, 1f - num3 * force.ceiling.y);
				}
				EntityVehicle.Force.Type type = force.type;
				if (type != EntityVehicle.Force.Type.Relative)
				{
					if (type == EntityVehicle.Force.Type.RelativeTorque)
					{
						this.vehicleRB.AddRelativeTorque(force.force * num2, ForceMode.VelocityChange);
					}
				}
				else
				{
					this.vehicleRB.AddRelativeForce(force.force * num2, ForceMode.VelocityChange);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FixedUpdateMotors()
	{
		for (int i = 0; i < this.motors.Length; i++)
		{
			EntityVehicle.Motor motor = this.motors[i];
			motor.rpm *= motor.rpmDrag;
			float num = 0f;
			switch (motor.trigger)
			{
			case EntityVehicle.Motor.Trigger.On:
				num = 1f;
				break;
			case EntityVehicle.Motor.Trigger.InputForward:
				if (this.movementInput != null)
				{
					num = this.movementInput.moveForward;
				}
				break;
			case EntityVehicle.Motor.Trigger.InputStrafe:
				if (this.movementInput != null)
				{
					num = this.movementInput.moveStrafe;
				}
				break;
			case EntityVehicle.Motor.Trigger.InputUp:
				if (this.movementInput != null && this.movementInput.jump)
				{
					num = 1f;
				}
				break;
			case EntityVehicle.Motor.Trigger.InputDown:
				if (this.movementInput != null && this.movementInput.down)
				{
					num = 1f;
				}
				break;
			case EntityVehicle.Motor.Trigger.Vel:
				num = this.vehicle.CurrentForwardVelocity / (this.vehicle.VelocityMaxForward + 0.001f);
				if (num < 0.01f)
				{
					num = 0f;
				}
				break;
			}
			if (num != 0f)
			{
				float num2 = 1f;
				if (this.movementInput != null && this.movementInput.running)
				{
					num2 = motor.turbo;
				}
				if (motor.engine != null && !motor.engine.isRunning)
				{
					num *= motor.engineOffPer;
					num2 = 1f;
				}
				num *= num2;
				switch (motor.type)
				{
				case EntityVehicle.Motor.Type.Spin:
					if (this.hasDriver)
					{
						float num3 = Mathf.Lerp(motor.rpmAccelMin, motor.rpmAccelMax, num);
						motor.rpm += num3;
						motor.rpm = Mathf.Min(motor.rpm, motor.rpmMax * num2);
					}
					break;
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateMotors()
	{
		for (int i = 0; i < this.motors.Length; i++)
		{
			EntityVehicle.Motor motor = this.motors[i];
			Transform transform = motor.transform;
			if (transform)
			{
				Vector3 localEulerAngles = transform.localEulerAngles;
				ref Vector3 ptr = ref localEulerAngles;
				int axis = motor.axis;
				ptr[axis] += motor.rpm * 360f * Time.deltaTime;
				transform.localEulerAngles = localEulerAngles;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int GetWheelsOnGround()
	{
		int num = 0;
		int num2 = this.wheels.Length;
		for (int i = 0; i < num2; i++)
		{
			if (this.wheels[i].isGrounded)
			{
				num++;
			}
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void SetWheelsForces(float motorTorque, float motorTorqueBase, float brakeTorque)
	{
		this.vehicle.CurrentMotorTorquePercent = motorTorque / motorTorqueBase;
		int num = this.wheels.Length;
		for (int i = 0; i < num; i++)
		{
			EntityVehicle.Wheel wheel = this.wheels[i];
			wheel.wheelC.motorTorque = motorTorque * wheel.motorTorqueScale;
			wheel.wheelC.brakeTorque = brakeTorque * wheel.brakeTorqueScale;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateWheelsCollision()
	{
		float wheelPtlScale = this.vehicle.WheelPtlScale;
		for (int i = 0; i < this.wheels.Length; i++)
		{
			EntityVehicle.Wheel wheel = this.wheels[i];
			wheel.isGrounded = false;
			WheelHit wheelHit;
			if (wheel.wheelC.GetGroundHit(out wheelHit))
			{
				float mass = wheel.wheelC.mass;
				if (wheelHit.normal.y >= 0f)
				{
					wheel.isGrounded = true;
				}
				if (wheelHit.force > 260f * mass)
				{
					this.PlayOneShot(wheel.bounceSound, false, false, false);
				}
				float forwardSlip = wheelHit.forwardSlip;
				if (forwardSlip <= -0.9f || forwardSlip >= 0.995f)
				{
					wheel.slideTime += Time.deltaTime;
				}
				else if (Utils.FastAbs(wheelHit.sidewaysSlip) >= 0.19f)
				{
					wheel.slideTime += Time.deltaTime;
				}
				else
				{
					wheel.slideTime = 0f;
				}
				if (wheel.slideTime > 0.2f)
				{
					wheel.slideTime = 0f;
					this.PlayOneShot(wheel.slideSound, false, false, false);
				}
				if (wheelPtlScale > 0f && Utils.FastAbs(forwardSlip) >= 0.5f)
				{
					wheel.ptlTime += Time.deltaTime;
					if (wheel.ptlTime > 0.05f)
					{
						wheel.ptlTime = 0f;
						float lightValue = GameManager.Instance.World.GetLightBrightness(World.worldToBlockPos(wheelHit.point)) * 0.5f;
						ParticleEffect pe = new ParticleEffect("tiresmoke", Vector3.zero, lightValue, new Color(1f, 1f, 1f, 1f), null, wheel.wheelC.transform, false);
						Transform transform = GameManager.Instance.SpawnParticleEffectClientForceCreation(pe, -1, false);
						if (transform)
						{
							transform.position = wheelHit.point;
							transform.localScale = new Vector3(wheelPtlScale, wheelPtlScale, wheelPtlScale);
						}
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateWheelsSteering()
	{
		this.wheels[0].wheelC.steerAngle = this.wheelDir;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Update()
	{
		base.Update();
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && base.GetFirstAttached())
		{
			this.world.entityDistributer.SendFullUpdateNextTick(this);
		}
		if (this.vehicleRB && this.RBActive)
		{
			Quaternion rhs = Quaternion.Euler(0f, this.wheelDir, 0f);
			for (int i = 0; i < this.wheels.Length; i++)
			{
				EntityVehicle.Wheel wheel = this.wheels[i];
				wheel.tireSpinSpeed = Utils.FastLerpUnclamped(wheel.tireSpinSpeed, wheel.wheelC.rotationSpeed, 0.3f);
				wheel.tireSpin += Utils.FastClamp(wheel.tireSpinSpeed * Time.deltaTime, -13f, 13f);
				Vector3 vector;
				Quaternion quaternion;
				wheel.wheelC.GetWorldPose(out vector, out quaternion);
				if (wheel.steerT)
				{
					quaternion = Quaternion.Euler(wheel.tireSpin, 0f, 0f);
					Quaternion quaternion2 = wheel.steerBaseRot * rhs;
					if (!wheel.isSteerParentOfTire)
					{
						quaternion2 *= quaternion;
					}
					wheel.steerT.localRotation = quaternion2;
				}
				if (wheel.tireT)
				{
					if (wheel.tireSuspensionPercent > 0f)
					{
						vector = wheel.tireT.parent.InverseTransformPoint(vector);
						Vector3 localPosition = wheel.tireT.localPosition;
						localPosition.y = vector.y;
						wheel.tireT.localPosition = localPosition;
					}
					if (wheel.steerT)
					{
						if (wheel.isSteerParentOfTire)
						{
							wheel.tireT.localRotation = quaternion;
						}
					}
					else
					{
						wheel.tireT.localRotation = Quaternion.Euler(wheel.tireSpin, 0f, 0f);
					}
				}
			}
		}
		if (this.vehicleRB)
		{
			float deltaTime = Time.deltaTime;
			Vector3 vector2;
			if (!this.isEntityRemote)
			{
				vector2 = this.PhysicsTransform.position + Origin.position;
				this.SetPosition(vector2, false);
				vector2 -= Origin.position;
				this.qrotation = this.PhysicsTransform.rotation;
				this.rotation = this.qrotation.eulerAngles;
				this.ModelTransform.rotation = this.qrotation;
			}
			else
			{
				vector2 = this.ModelTransform.position;
			}
			EntityPlayerLocal attachedPlayerLocal = base.GetAttachedPlayerLocal();
			if (attachedPlayerLocal)
			{
				vp_FPCamera vp_FPCamera = attachedPlayerLocal.vp_FPCamera;
				if (!EntityVehicle.isTurnTowardsLook)
				{
					Vector3 forward = base.transform.forward;
					Vector2 to = new Vector2(forward.x, forward.z);
					this.cameraAngleTarget = Vector2.SignedAngle(this.cameraStartVec, to);
					float num = this.cameraAngle;
					float num2 = Mathf.Abs(Mathf.DeltaAngle(this.cameraAngle, this.cameraAngleTarget));
					this.cameraAngle = Mathf.MoveTowardsAngle(this.cameraAngle, this.cameraAngleTarget, num2 * 0.3f);
					num -= this.cameraAngle;
					vp_FPCamera.Yaw += num;
				}
				float magnitude = this.vehicleRB.velocity.magnitude;
				float num3 = -Mathf.Lerp(this.vehicle.CameraDistance.x, this.vehicle.CameraDistance.y, magnitude / this.vehicle.VelocityMaxForward) * EntityVehicle.cameraDistScale - this.cameraDist;
				if (num3 < 0f)
				{
					this.cameraOutTime += deltaTime;
					if (this.cameraOutTime > 1f)
					{
						num3 *= 0.03f;
						this.cameraDist += num3;
					}
				}
				else if (num3 > 0f)
				{
					this.cameraOutTime = 0f;
					num3 *= 0.22f;
					this.cameraDist += num3;
				}
				vector2.y += 1.8f;
				vector2.y += this.lastRBVel.y * 0.2f;
				this.cameraPos.x = vector2.x;
				this.cameraPos.z = vector2.z;
				float num4 = vector2.y - this.cameraPos.y;
				if ((num4 < 0f && this.cameraVelY > 0f) || (num4 > 0f && this.cameraVelY < 0f))
				{
					this.cameraVelY *= 0.98f;
				}
				this.cameraVelY += num4 * 0.25f * deltaTime;
				this.cameraVelY *= 0.94f;
				this.cameraPos.y = this.cameraPos.y + this.cameraVelY;
				num4 = vector2.y - this.cameraPos.y;
				if (num4 > 2.5f)
				{
					this.cameraPos.y = vector2.y - 2.5f;
					this.cameraVelY = 0f;
				}
				else if (num4 < -2.5f)
				{
					this.cameraPos.y = vector2.y + 2.5f;
					this.cameraVelY = 0f;
				}
				if (this.cameraStartBlend < 1f)
				{
					this.cameraStartBlend += deltaTime * 1.2f;
					vp_FPCamera.DrivingPosition = Vector3.Lerp(this.cameraStartPos, this.cameraPos, this.cameraStartBlend);
					float z = Mathf.Lerp(-0.0001f, this.cameraDist, this.cameraStartBlend);
					vp_FPCamera.Position3rdPersonOffset = new Vector3(0f, 0f, z);
				}
				else
				{
					vp_FPCamera.DrivingPosition = this.cameraPos;
					vp_FPCamera.Position3rdPersonOffset = new Vector3(0f, 0f, this.cameraDist);
				}
			}
		}
		this.UpdateAttachment();
		if (this.RBActive || this.syncPlayTime >= 0f)
		{
			this.UpdateMotors();
		}
		this.vehicle.Update(Time.deltaTime);
		if ((Time.frameCount & 1) == 0)
		{
			this.hitEffectCount = 1;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void updateTransform()
	{
		if (this.isEntityRemote)
		{
			float t = Time.deltaTime * 10f;
			Transform modelTransform = this.ModelTransform;
			Vector3 position = Vector3.Lerp(modelTransform.position, this.position - Origin.position, t);
			Quaternion rotation = Quaternion.Slerp(modelTransform.rotation, this.qrotation, t);
			modelTransform.SetPositionAndRotation(position, rotation);
		}
	}

	public void CameraChangeRotation(float _newRotation)
	{
		if (EntityVehicle.isTurnTowardsLook)
		{
			EntityPlayerLocal attachedPlayerLocal = base.GetAttachedPlayerLocal();
			if (attachedPlayerLocal)
			{
				attachedPlayerLocal.vp_FPCamera.Yaw += _newRotation;
			}
		}
	}

	public override void OriginChanged(Vector3 _deltaPos)
	{
		base.OriginChanged(_deltaPos);
		Vector3 position = this.position - Origin.position;
		this.ModelTransform.position = position;
		this.PhysicsTransform.position = position;
		if (this.vehicleRB)
		{
			this.vehicleRB.position = position;
		}
		this.cameraPos += _deltaPos;
		this.cameraStartPos += _deltaPos;
		EntityPlayerLocal attachedPlayerLocal = base.GetAttachedPlayerLocal();
		if (attachedPlayerLocal)
		{
			attachedPlayerLocal.vp_FPCamera.DrivingPosition += _deltaPos;
		}
	}

	public override void SetPosition(Vector3 _pos, bool _bUpdatePhysics = true)
	{
		base.SetPosition(_pos, _bUpdatePhysics);
		if (!this.isEntityRemote)
		{
			this.ModelTransform.position = _pos - Origin.position;
		}
	}

	public override void SetRotation(Vector3 _rot)
	{
		base.SetRotation(_rot);
		if (!this.isEntityRemote)
		{
			this.ModelTransform.rotation = this.qrotation;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 GetCenterPosition()
	{
		return this.position + this.ModelTransform.up * 0.8f;
	}

	public override bool IsQRotationUsed()
	{
		return true;
	}

	public override float GetHeight()
	{
		return 1f;
	}

	public void AddRelativeForce(Vector3 forceVec, ForceMode mode = ForceMode.VelocityChange)
	{
		if (this.isEntityRemote)
		{
			return;
		}
		if (!this.RBActive)
		{
			this.RBActive = true;
			this.vehicleRB.isKinematic = false;
		}
		this.vehicleRB.AddRelativeForce(forceVec, mode);
	}

	public void AddForce(Vector3 forceVec, ForceMode mode = ForceMode.VelocityChange)
	{
		if (this.isEntityRemote)
		{
			return;
		}
		if (!this.RBActive)
		{
			this.RBActive = true;
			this.vehicleRB.isKinematic = false;
		}
		this.vehicleRB.AddForce(forceVec, mode);
	}

	public override Vector3 GetVelocityPerSecond()
	{
		if (this.isEntityRemote)
		{
			return this.vehicle.CurrentVelocity;
		}
		return this.vehicleRB.velocity;
	}

	public void VelocityFlip()
	{
		if (this.isEntityRemote)
		{
			this.vehicle.CurrentVelocity = new Vector3(this.vehicle.CurrentVelocity.x * -1f, this.vehicle.CurrentVelocity.y, this.vehicle.CurrentVelocity.z * -1f);
			return;
		}
		this.vehicleRB.velocity = new Vector3(this.vehicleRB.velocity.x * -1f, this.vehicleRB.velocity.y, this.vehicleRB.velocity.z * -1f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EnterVehicle(EntityAlive _entity)
	{
		int slot = -1;
		_entity.StartAttachToEntity(this, slot);
		if (this.NavObject != null)
		{
			this.NavObject.IsActive = !(_entity is EntityPlayerLocal);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetVehicleDriven()
	{
		if (base.AttachedMainEntity != null && !base.AttachedMainEntity.isEntityRemote)
		{
			Utils.SetLayerRecursively(this.vehicleRB.gameObject, 21, null);
			this.RBActive = true;
			this.vehicleRB.isKinematic = false;
			this.vehicleRB.WakeUp();
			if (this.world.IsRemote())
			{
				this.vehicleRB.velocity = this.vehicle.CurrentVelocity;
			}
			this.lastRBVel = Vector3.zero;
			return;
		}
		Utils.SetLayerRecursively(this.vehicleRB.gameObject, 21, null);
		if (this.isEntityRemote)
		{
			this.RBActive = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateAttachment()
	{
		Entity attachedMainEntity = base.AttachedMainEntity;
		if (this.hasDriver && attachedMainEntity == null)
		{
			this.DriverRemoved();
		}
		if (attachedMainEntity != null && attachedMainEntity.IsDead())
		{
			((EntityAlive)attachedMainEntity).RemoveIKTargets();
			attachedMainEntity.Detach();
			this.DriverRemoved();
		}
		for (int i = this.delayedAttachments.Count - 1; i >= 0; i--)
		{
			EntityVehicle.DelayedAttach delayedAttach = this.delayedAttachments[i];
			Entity entity = GameManager.Instance.World.GetEntity(delayedAttach.entityId);
			if (entity)
			{
				if (!base.IsAttached(entity))
				{
					entity.AttachToEntity(this, delayedAttach.slot);
				}
				this.delayedAttachments.RemoveAt(i);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DriverRemoved()
	{
		this.hasDriver = false;
		this.vehicle.SetColors();
		this.vehicle.FireEvent(Vehicle.Event.Stop);
		this.isInteractionLocked = false;
		this.RBNoDriverGndTime = 0f;
		this.RBNoDriverSleepTime = 0f;
		this.collisionGrazeCount = 0;
		if (this.GetWheelsOnGround() > 0 && !this.vehicleRB.isKinematic)
		{
			this.vehicleRB.velocity *= 0.5f;
		}
		if (this.NavObject != null)
		{
			this.NavObject.IsActive = true;
		}
	}

	public override int AttachEntityToSelf(Entity _entity, int slot = -1)
	{
		slot = base.AttachEntityToSelf(_entity, slot);
		if (slot >= 0)
		{
			EntityAlive entityAlive = (EntityAlive)_entity;
			int seatPose = this.vehicle.GetSeatPose(slot);
			entityAlive.SetVehiclePoseMode(seatPose);
			entityAlive.transform.gameObject.layer = 24;
			entityAlive.m_characterController.Enable(false);
			entityAlive.SetIKTargets(this.vehicle.GetIKTargets(slot));
			this.isInteractionLocked = (base.GetAttachFreeCount() == 0);
			if (this.nativeCollider)
			{
				this.nativeCollider.enabled = !this.isInteractionLocked;
			}
			if (slot == 0)
			{
				this.hasDriver = true;
				this.vehicle.SetColors();
				this.vehicle.FireEvent(Vehicle.Event.Start);
			}
			this.SetVehicleDriven();
			this.vehicle.TriggerUpdateEffects();
			if (!_entity.isEntityRemote && GameManager.Instance.World != null)
			{
				LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(_entity as EntityPlayerLocal);
				if (uiforPlayer != null && uiforPlayer.playerInput != null)
				{
					PlayerActionsVehicle vehicleActions = uiforPlayer.playerInput.VehicleActions;
					uiforPlayer.ActionSetManager.Insert(vehicleActions, 1, null);
					this.movementInput = new MovementInput();
					this.CameraInit();
				}
			}
		}
		return slot;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void DetachEntity(Entity _entity)
	{
		for (int i = this.delayedAttachments.Count - 1; i >= 0; i--)
		{
			if (this.delayedAttachments[i].entityId == _entity.entityId)
			{
				this.delayedAttachments.RemoveAt(i);
			}
		}
		int num = base.FindAttachSlot(_entity);
		if (num < 0)
		{
			return;
		}
		EntityAlive entityAlive = (EntityAlive)_entity;
		entityAlive.SetVehiclePoseMode(-1);
		entityAlive.RemoveIKTargets();
		int modelLayer = entityAlive.GetModelLayer();
		entityAlive.SetModelLayer(modelLayer, true, null);
		entityAlive.transform.gameObject.layer = 20;
		entityAlive.ModelTransform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
		entityAlive.m_characterController.Enable(true);
		if (!_entity.isEntityRemote && GameManager.Instance.World != null)
		{
			LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(_entity as EntityPlayerLocal);
			if (uiforPlayer != null)
			{
				PlayerActionsVehicle vehicleActions = uiforPlayer.playerInput.VehicleActions;
				uiforPlayer.ActionSetManager.Remove(vehicleActions, 1, null);
			}
			this.movementInput = null;
		}
		if (num == 0)
		{
			this.DriverRemoved();
		}
		bool isEntityRemote = this.isEntityRemote;
		base.DetachEntity(_entity);
		this.isInteractionLocked = (base.GetAttachFreeCount() == 0);
		if (this.nativeCollider)
		{
			this.nativeCollider.enabled = !this.isInteractionLocked;
		}
		this.SetVehicleDriven();
		this.vehicle.TriggerUpdateEffects();
		if (isEntityRemote && !this.isEntityRemote)
		{
			this.RBActive = true;
			this.RBNoDriverSleepTime = 0f;
			this.vehicleRB.isKinematic = false;
			this.vehicleRB.velocity = this.vehicle.CurrentVelocity;
		}
	}

	public override int AttachToEntity(Entity _entity, int slot = -1)
	{
		return -1;
	}

	public override AttachedToEntitySlotInfo GetAttachedToInfo(int _slotIdx)
	{
		AttachedToEntitySlotInfo attachedToEntitySlotInfo = new AttachedToEntitySlotInfo();
		attachedToEntitySlotInfo.bKeep3rdPersonModelVisible = true;
		attachedToEntitySlotInfo.bReplaceLocalInventory = true;
		attachedToEntitySlotInfo.pitchRestriction = new Vector2(-30f, 30f);
		attachedToEntitySlotInfo.yawRestriction = new Vector2(-90f, 90f);
		attachedToEntitySlotInfo.enterParentTransform = base.transform;
		attachedToEntitySlotInfo.enterPosition = new Vector3(0f, 0f, -0.201f);
		attachedToEntitySlotInfo.enterRotation = Vector3.zero;
		DynamicProperties propertiesForClass = this.vehicle.GetPropertiesForClass("seat" + _slotIdx.ToString());
		if (propertiesForClass != null)
		{
			propertiesForClass.ParseVec("position", ref attachedToEntitySlotInfo.enterPosition);
			propertiesForClass.ParseVec("rotation", ref attachedToEntitySlotInfo.enterRotation);
			string @string = propertiesForClass.GetString("exit");
			if (@string.Length > 0)
			{
				char[] separator = new char[]
				{
					'~'
				};
				string[] array = @string.Split(separator);
				for (int i = 0; i < array.Length; i++)
				{
					Vector3 vector = StringParsers.ParseVector3(array[i], 0, -1);
					vector.y += 0.02f;
					AttachedToEntitySlotExit item;
					item.position = base.GetPosition() + base.transform.TransformDirection(vector);
					float num = Mathf.Atan2(vector.x, vector.z) * 57.29578f;
					item.rotation = new Vector3(0f, num + 180f + this.rotation.y, 0f);
					attachedToEntitySlotInfo.exits.Add(item);
				}
			}
		}
		else
		{
			AttachedToEntitySlotExit item2 = default(AttachedToEntitySlotExit);
			item2.position = base.GetPosition() + -2f * base.transform.right;
			item2.rotation = new Vector3(0f, this.rotation.y + 90f, 0f);
			attachedToEntitySlotInfo.exits.Add(item2);
		}
		return attachedToEntitySlotInfo;
	}

	public Vector3 GetExitVelocity()
	{
		Vector3 a = this.GetVelocityPerSecond();
		if (this.GetWheelsOnGround() > 0)
		{
			a *= 0.5f;
		}
		return a * 0.7f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CameraInit()
	{
		Transform transform = base.transform;
		Vector3 forward = transform.forward;
		this.cameraStartVec.x = forward.x;
		this.cameraStartVec.y = forward.z;
		this.cameraPos = transform.position;
		this.cameraPos.y = this.cameraPos.y + 1.8f;
		EntityPlayerLocal attachedPlayerLocal = base.GetAttachedPlayerLocal();
		if (attachedPlayerLocal)
		{
			vp_FPCamera vp_FPCamera = attachedPlayerLocal.vp_FPCamera;
			this.cameraStartPos = vp_FPCamera.transform.position;
			this.cameraStartBlend = 0f;
			vp_FPCamera.m_Current3rdPersonBlend = 1f;
			this.cameraDist = -(this.cameraPos - this.cameraStartPos).magnitude;
		}
	}

	public override void OnCollisionForward(Transform t, Collision collision, bool isStay)
	{
		if (this.isEntityRemote)
		{
			return;
		}
		if (!this.RBActive)
		{
			if (this.vehicleRB.velocity.magnitude > 0.01f && this.vehicleRB.angularVelocity.magnitude > 0.05f)
			{
				this.RBActive = true;
			}
			if (this.vehicleRB.isKinematic && (!collision.rigidbody || collision.rigidbody.velocity.magnitude > 0.05f))
			{
				this.RBActive = true;
			}
		}
		Entity entity = null;
		int layer = collision.gameObject.layer;
		if (layer != 16)
		{
			ColliderHitCallForward component = collision.gameObject.GetComponent<ColliderHitCallForward>();
			if (component)
			{
				entity = component.Entity;
			}
			if (!entity)
			{
				entity = this.FindEntity(collision.transform.parent);
			}
			if (!entity)
			{
				Rigidbody rigidbody = collision.rigidbody;
				if (rigidbody)
				{
					entity = this.FindEntity(rigidbody.transform);
				}
			}
		}
		if (entity && entity.IsSpawned())
		{
			if (collision.impulse.sqrMagnitude > 4f)
			{
				Vector3 vector = -collision.relativeVelocity;
				if (layer != 19)
				{
					vector *= 0.4f;
				}
				float num = vector.magnitude + 0.0001f;
				Vector3 vector2 = vector * (1f / num);
				EnumBodyPartHit enumBodyPartHit = EnumBodyPartHit.Torso;
				bool flag = false;
				Vector3 vector3 = Vector3.zero;
				Vector3 vector4 = Vector3.zero;
				int contactCount = collision.contactCount;
				for (int i = 0; i < contactCount; i++)
				{
					ContactPoint contact = collision.GetContact(i);
					vector3 += contact.point;
					vector4 += contact.normal;
					flag |= contact.thisCollider.CompareTag("E_VehicleStrong");
					string tag = contact.otherCollider.tag;
					enumBodyPartHit |= DamageSource.TagToBodyPart(tag);
				}
				vector3 *= 1f / (float)contactCount;
				vector3 += Origin.position;
				vector4 = Vector3.Normalize(vector4);
				float num2 = -Vector3.Dot(vector2, vector4);
				if (num2 < 0f)
				{
					num2 = 0f;
				}
				if (num > 1f)
				{
					float num3 = Vector3.Dot(entity.motion.normalized, vector2);
					if (num3 > 0.2f)
					{
						float num4 = entity.motion.magnitude * 20f;
						num -= num4 * num3;
					}
				}
				float num5 = num * num2;
				float num6 = this.vehicleRB.mass * 0.2f;
				num6 += 20f;
				float massKg = EntityClass.list[entity.entityClass].MassKg;
				float num7 = num6 / massKg;
				float num8 = Utils.FastClamp(num7, 0.25f, 1.6f);
				float num9 = num5 * num8;
				float num10 = Utils.FastClamp(num7, 1f, 1.5f);
				float num11 = num5 / num10;
				if (massKg < 2f)
				{
					num2 = 0f;
					num9 = 0f;
					num11 = 0f;
				}
				EntityPlayer entityPlayer = entity as EntityPlayer;
				if (entityPlayer && (float)entityPlayer.SpawnedTicks <= 80f)
				{
					num9 = 0f;
					num11 = 0f;
				}
				bool flag2 = this.world.IsWorldEvent(World.WorldEvent.BloodMoon);
				bool flag3 = num7 >= 2f && !flag2 && (this.lastRBVel.sqrMagnitude > 10.2400007f || num9 > 2.1f);
				vector *= num6 * 0.008f;
				vector.y = Utils.FastMin(50f, vector.y + vector.magnitude * 3f);
				if (num9 > 2.1f)
				{
					int entityId = this.entityId;
					Entity firstAttached = base.GetFirstAttached();
					if (firstAttached)
					{
						entityId = firstAttached.entityId;
					}
					DamageSourceEntity damageSourceEntity = new DamageSourceEntity(EnumDamageSource.External, EnumDamageTypes.Crushing, entityId, vector);
					damageSourceEntity.bodyParts = enumBodyPartHit;
					damageSourceEntity.DismemberChance = 1.2f;
					float num12 = 1f + (num9 - 2.1f) * 12f;
					if (entityPlayer)
					{
						num12 = Utils.FastMin(num12, 10f);
					}
					if (flag)
					{
						num12 *= this.vehicle.EffectEntityDamagePer;
					}
					bool flag4 = entity.IsAlive();
					entity.DamageEntity(damageSourceEntity, (int)num12, false, 1f);
					if ((entity.entityFlags & (EntityFlags.Player | EntityFlags.Zombie | EntityFlags.Animal | EntityFlags.Bandit)) > EntityFlags.None && num12 > 70f)
					{
						this.SpawnParticle("blood_vehicle", entity.entityId, 0.22f);
						if (num12 > 200f)
						{
							this.SpawnParticle("blood_vehicle", entity.entityId, 0.35f);
						}
					}
					float num13 = 1f;
					if (flag2)
					{
						this.velocityMax *= 0.7f;
						num13 *= 15f;
					}
					EntityPlayer entityPlayer2 = firstAttached as EntityPlayer;
					if (entityPlayer2)
					{
						entityPlayer2.MinEventContext.Other = (entity as EntityAlive);
						entityPlayer2.FireEvent(MinEventTypes.onSelfVehicleAttackedOther, true);
					}
					if (flag4 && entity.IsDead())
					{
						flag3 = false;
						if (entityPlayer2)
						{
							EntityAlive entityAlive = entity as EntityAlive;
							if (entityAlive)
							{
								entityPlayer2.AddKillXP(entityAlive, 0.5f);
							}
						}
					}
					else if (num9 >= num13)
					{
						float num14 = num9 * 0.09f;
						if (num9 < 8f && num14 > 0.9f)
						{
							num14 = 0.9f;
						}
						if (this.rand.RandomFloat < num14)
						{
							flag3 = true;
						}
					}
				}
				if (entity.emodel.IsRagdollOn)
				{
					num11 *= 0.3f;
				}
				if (flag3)
				{
					entity.emodel.DoRagdoll(2.5f, enumBodyPartHit, vector, vector3, false);
				}
				if (num11 > 2.1f)
				{
					float num15 = 1f + (num11 - 2.1f) * 28f;
					num15 *= this.vehicle.EffectSelfDamagePer;
					if (flag)
					{
						num15 *= this.vehicle.EffectStrongSelfDamagePer;
					}
					float num16 = (this.Health > 1) ? 1f : 0.1f;
					this.damageAccumulator += num15 * num16;
					this.ApplyAccumulatedDamage();
				}
				if (num > 0.1f && num2 > 0.2f)
				{
					this.velocityMax *= Mathf.LerpUnclamped(1f, 0.4f + num10 * 0.396666676f, num2);
					return;
				}
			}
		}
		else
		{
			Vector3 a = this.lastRBVel;
			float magnitude = a.magnitude;
			float num17 = Utils.FastMax(0f, magnitude - 1.5f) * this.vehicleRB.mass * 0.0583333336f;
			if (isStay)
			{
				num17 *= 0.2f;
			}
			if (num17 < 2f)
			{
				this.collisionGrazeCount++;
				return;
			}
			this.collisionBlockDamage = num17;
			this.collisionVelNorm = a * (1f / magnitude);
			this.collisionIgnoreCount = 0;
			int contactCount2 = collision.contactCount;
			for (int j = 0; j < contactCount2; j++)
			{
				ContactPoint contact2 = collision.GetContact(j);
				Ray ray = new Ray(contact2.point + Origin.position + contact2.normal * 0.004f, -contact2.normal);
				bool flag5 = Voxel.Raycast(this.world, ray, 0.03f, -555520021, 69, 0f);
				if (!flag5)
				{
					ray.origin += contact2.normal * -contact2.separation;
					ray.direction = -contact2.normal + this.collisionVelNorm;
					flag5 = Voxel.Raycast(this.world, ray, 0.03f, -555520021, 69, 0f);
				}
				if (flag5 && GameUtils.IsBlockOrTerrain(Voxel.voxelRayHitInfo.tag))
				{
					bool flag6 = false;
					for (int k = 0; k < this.collisionHits.Count; k++)
					{
						if (this.collisionHits[k].hit.blockPos == Voxel.voxelRayHitInfo.hit.blockPos)
						{
							flag6 = true;
							break;
						}
					}
					if (!flag6)
					{
						this.contactPoints.Add(contact2);
						this.collisionHits.Add(Voxel.voxelRayHitInfo.Clone());
					}
				}
				else
				{
					this.collisionIgnoreCount++;
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator ApplyCollisionsCoroutine()
	{
		WaitForFixedUpdate wait = new WaitForFixedUpdate();
		for (;;)
		{
			yield return wait;
			int count = this.contactPoints.Count;
			if (count > 0)
			{
				float num = (this.Health > 1) ? 1f : 0.1f;
				int entityId = this.entityId;
				ItemActionAttack.EnumAttackMode attackMode = ItemActionAttack.EnumAttackMode.RealNoHarvesting;
				if (this.hitEffectCount <= 0)
				{
					attackMode = ItemActionAttack.EnumAttackMode.RealNoHarvestingOrEffects;
				}
				float num2 = 1f / ((float)count + 0.001f);
				float num3 = this.collisionBlockDamage;
				num3 *= num2;
				for (int i = 0; i < count; i++)
				{
					ContactPoint contactPoint = this.contactPoints[i];
					WorldRayHitInfo worldRayHitInfo = this.collisionHits[i];
					float num4 = -Vector3.Dot(contactPoint.normal, this.collisionVelNorm);
					num4 = Mathf.Pow(num4 * 1.01f, 3f);
					num4 = Utils.FastClamp(num4, 0.01f, 1f);
					float num5 = 0f;
					float num6 = 2.5f;
					bool flag = contactPoint.thisCollider.CompareTag("E_VehicleStrong");
					bool flag2 = worldRayHitInfo.tag == "T_Mesh";
					if (flag2)
					{
						if (contactPoint.normal.y < 0.85f)
						{
							num5 = 0.7f + 4f * this.rand.RandomFloat * num4;
							num6 = 0.1f;
						}
					}
					else
					{
						num5 = num3 * num4;
						if (flag)
						{
							num5 *= this.vehicle.EffectBlockDamagePer;
						}
						float vehicleHitScale = worldRayHitInfo.hit.blockValue.Block.VehicleHitScale;
						num5 *= vehicleHitScale;
						num6 /= vehicleHitScale;
						if (num5 < 5f)
						{
							num5 = 0f;
						}
					}
					if (num5 >= 1f)
					{
						List<string> buffActions = null;
						ItemActionAttack.AttackHitInfo attackHitInfo = new ItemActionAttack.AttackHitInfo();
						attackHitInfo.hardnessScale = 1f;
						if (flag2 || !worldRayHitInfo.hit.blockValue.Block.shape.IsTerrain())
						{
							ItemActionAttack.Hit(worldRayHitInfo, entityId, EnumDamageTypes.Bashing, num5, num5, 1f, 1f, 0f, 0.05f, "metal", null, buffActions, attackHitInfo, 1, 0, 0f, null, null, attackMode, null, -1, null);
							int num7 = this.hitEffectCount - 1;
							this.hitEffectCount = num7;
							if (num7 <= 0)
							{
								attackMode = ItemActionAttack.EnumAttackMode.RealNoHarvestingOrEffects;
							}
						}
						if (!attackHitInfo.bBlockHit)
						{
							ChunkCluster chunkCluster = GameManager.Instance.World.ChunkClusters[worldRayHitInfo.hit.clrIdx];
							if (chunkCluster != null)
							{
								Vector3i vector3i = Vector3i.FromVector3Rounded(contactPoint.point + Origin.position);
								for (int j = 0; j >= -1; j--)
								{
									worldRayHitInfo.hit.blockPos.y = vector3i.y + j;
									for (int k = 0; k >= -1; k--)
									{
										worldRayHitInfo.hit.blockPos.z = vector3i.z + k;
										for (int l = 0; l >= -1; l--)
										{
											worldRayHitInfo.hit.blockPos.x = vector3i.x + l;
											if (!chunkCluster.GetBlock(worldRayHitInfo.hit.blockPos).Block.shape.IsTerrain())
											{
												ItemActionAttack.Hit(worldRayHitInfo, entityId, EnumDamageTypes.Bashing, num5, num5, 1f, 1f, 0f, 0.05f, "metal", null, buffActions, attackHitInfo, 1, 0, 0f, null, null, attackMode, null, -1, null);
												int num7 = this.hitEffectCount - 1;
												this.hitEffectCount = num7;
												if (num7 <= 0)
												{
													attackMode = ItemActionAttack.EnumAttackMode.RealNoHarvestingOrEffects;
												}
												if (attackHitInfo.bBlockHit)
												{
													j = -999;
													k = -999;
													break;
												}
											}
										}
									}
								}
							}
						}
						if (attackHitInfo.bKilled && attackHitInfo.bBlockHit)
						{
							BlockModelTree blockModelTree = attackHitInfo.blockBeingDamaged.Block as BlockModelTree;
							if (blockModelTree != null && blockModelTree.isMultiBlock && blockModelTree.multiBlockPos.dim.y >= 12)
							{
								this.velocityMax *= 0.3f;
								this.vehicleRB.AddRelativeForce(Vector3.up * 2.5f, ForceMode.VelocityChange);
								this.vehicleRB.AddRelativeForce(this.collisionVelNorm * 2f, ForceMode.VelocityChange);
							}
						}
						if ((attackHitInfo.bKilled || !attackHitInfo.bBlockHit) && attackHitInfo.hardnessScale > 0f)
						{
							this.collisionIgnoreCount++;
						}
						num5 = Utils.FastMin(num5, (float)attackHitInfo.damageGiven);
					}
					float num8 = num5 * num6;
					num8 *= this.vehicle.EffectSelfDamagePer;
					if (flag)
					{
						num8 *= this.vehicle.EffectStrongSelfDamagePer;
					}
					this.damageAccumulator += num8 * num;
					if (num8 > 50f)
					{
						this.SpawnParticle("blockdestroy_metal", worldRayHitInfo.hit.pos);
					}
				}
				this.ApplyAccumulatedDamage();
				int num9 = this.collisionIgnoreCount - count;
				if (num9 >= 0)
				{
					this.PhysicsRevertCollisionMotion(num9);
				}
				this.contactPoints.Clear();
				this.collisionHits.Clear();
			}
		}
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ApplyAccumulatedDamage()
	{
		if (this.damageAccumulator >= 1f)
		{
			int num = (int)this.damageAccumulator;
			this.damageAccumulator -= (float)num;
			this.ApplyDamage(num);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SpawnParticle(string _particleName, Vector3 _pos)
	{
		Vector3i blockPos = World.worldToBlockPos(_pos);
		float lightBrightness = this.world.GetLightBrightness(blockPos);
		this.world.GetGameManager().SpawnParticleEffectServer(new ParticleEffect(_particleName, _pos, lightBrightness, Color.white, null, null, false), this.entityId, false, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SpawnParticle(string _particleName, int _entityId, float _offsetY)
	{
		Vector3 pos = new Vector3(0f, _offsetY, 0f);
		this.world.GetGameManager().SpawnParticleEffectServer(new ParticleEffect(_particleName, pos, 1f, Color.white, null, _entityId, ParticleEffect.Attachment.Pelvis), this.entityId, false, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PhysicsRevertCollisionMotion(int _ignoreExcess)
	{
		if (_ignoreExcess == 0)
		{
			float num = Time.fixedDeltaTime * 0.5f;
			float num2 = this.lastRBVel.x * num;
			float num3 = this.lastRBVel.z * num;
			if (num2 < -0.0001f || num2 > 0.0001f || num3 < -0.0001f || num3 > 0.0001f)
			{
				this.lastRBPos.x = this.lastRBPos.x + num2;
				this.lastRBPos.z = this.lastRBPos.z + num3;
				this.vehicleRB.position = this.lastRBPos;
			}
		}
		Vector3 velocity = this.vehicleRB.velocity;
		velocity.x = this.lastRBVel.x * 0.9f;
		velocity.z = this.lastRBVel.z * 0.9f;
		velocity.y = this.lastRBVel.y * 0.6f + velocity.y * 0.4f;
		this.vehicleRB.velocity = velocity;
		this.vehicleRB.angularVelocity = this.lastRBAngVel;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DrawRayHandle(Vector3 pos, Vector3 dir, Color color, float duration = 0f)
	{
		Vector3 normalized = Vector3.Cross(Vector3.up, dir).normalized;
		Debug.DrawRay(pos, normalized * 0.005f, Color.blue, duration);
		Debug.DrawRay(pos, dir, color, duration);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DrawBlocks(WorldRayHitInfo hitInfo)
	{
		ChunkCluster chunkCluster = GameManager.Instance.World.ChunkClusters[hitInfo.hit.clrIdx];
		if (chunkCluster == null)
		{
			return;
		}
		for (int i = -1; i <= 1; i++)
		{
			for (int j = -1; j <= 1; j++)
			{
				for (int k = -1; k <= 1; k++)
				{
					Vector3i blockPos = hitInfo.hit.blockPos;
					blockPos.x += j;
					blockPos.y += i;
					blockPos.z += k;
					Vector3 start = blockPos.ToVector3() - Origin.position;
					BlockValue block = chunkCluster.GetBlock(blockPos);
					Color color = Color.black;
					if (!block.isair)
					{
						if (block.Block.shape.IsTerrain())
						{
							color = Color.yellow;
						}
						else
						{
							color = Color.white;
						}
					}
					Debug.DrawRay(start, Vector3.up, color);
					Debug.DrawRay(start, Vector3.right, color);
					Debug.DrawRay(start, Vector3.forward, color);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Entity FindEntity(Transform t)
	{
		Entity componentInChildren = t.GetComponentInChildren<Entity>();
		if (componentInChildren)
		{
			return componentInChildren;
		}
		return t.GetComponentInParent<Entity>();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void entityCollision(Vector3 _motion)
	{
	}

	public static EntityVehicle FindCollisionEntity(Transform t)
	{
		EntityVehicle entityVehicle = t.GetComponent<EntityVehicle>();
		if (!entityVehicle)
		{
			CollisionCallForward componentInParent = t.GetComponentInParent<CollisionCallForward>();
			if (componentInParent)
			{
				entityVehicle = (componentInParent.Entity as EntityVehicle);
			}
		}
		return entityVehicle;
	}

	public override float GetBlockDamageScale()
	{
		EntityAlive entityAlive = base.AttachedMainEntity as EntityAlive;
		if (entityAlive)
		{
			return entityAlive.GetBlockDamageScale();
		}
		return 1f;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void switchModelView(EnumEntityModelView modelView)
	{
	}

	public override void MoveEntityHeaded(Vector3 _direction, bool _isDirAbsolute)
	{
	}

	public override void MoveByAttachedEntity(EntityPlayerLocal _player)
	{
		if (this.movementInput == null)
		{
			return;
		}
		LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(_player);
		if (uiforPlayer == null || uiforPlayer.playerInput == null)
		{
			return;
		}
		PlayerActionsVehicle vehicleActions = uiforPlayer.playerInput.VehicleActions;
		MovementInput movementInput = _player.movementInput;
		if (_player == base.AttachedMainEntity)
		{
			this.movementInput.moveForward = vehicleActions.Move.Y;
			this.movementInput.moveStrafe = vehicleActions.Move.X;
			this.movementInput.down = vehicleActions.Hop.IsPressed;
			this.movementInput.jump = vehicleActions.Brake.IsPressed;
			if (EffectManager.GetValue(PassiveEffects.FlipControls, null, 0f, _player, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false) > 0f)
			{
				this.movementInput.moveForward *= -1f;
				this.movementInput.moveStrafe *= -1f;
			}
			this.movementInput.running = _player.movementInput.running;
			this.movementInput.lastInputController = movementInput.lastInputController;
			if (vehicleActions.ToggleTurnMode.WasPressed && !uiforPlayer.windowManager.IsModalWindowOpen())
			{
				EntityVehicle.isTurnTowardsLook = !EntityVehicle.isTurnTowardsLook;
			}
		}
		MovementInput movementInput2 = movementInput;
		movementInput2.rotation.x = movementInput2.rotation.x * this.vehicle.CameraTurnRate.x;
		MovementInput movementInput3 = movementInput;
		movementInput3.rotation.y = movementInput3.rotation.y * this.vehicle.CameraTurnRate.y;
		float num = vehicleActions.Scroll.Value;
		if (vehicleActions.LastInputType == BindingSourceType.DeviceBindingSource)
		{
			num *= 0.25f;
		}
		if (num != 0f)
		{
			EntityVehicle.cameraDistScale += num * -0.5f;
			EntityVehicle.cameraDistScale = Utils.FastClamp(EntityVehicle.cameraDistScale, 0.3f, 1.2f);
			this.cameraOutTime = 999f;
		}
	}

	public bool HasHeadlight()
	{
		VPHeadlight vpheadlight = this.vehicle.FindPart("headlight") as VPHeadlight;
		return vpheadlight != null && (vpheadlight.GetTransform() || vpheadlight.modInstalled);
	}

	public void ToggleHeadlight()
	{
		this.IsHeadlightOn = !this.IsHeadlightOn;
	}

	public bool IsHeadlightOn
	{
		get
		{
			VPHeadlight vpheadlight = this.vehicle.FindPart("headlight") as VPHeadlight;
			return vpheadlight != null && vpheadlight.IsOn();
		}
		set
		{
			this.vehicle.FireEvent(VehiclePart.Event.LightsOn, null, (float)(value ? 1 : 0));
		}
	}

	public override float GetLightLevel()
	{
		VPHeadlight vpheadlight = this.vehicle.FindPart("headlight") as VPHeadlight;
		if (vpheadlight == null)
		{
			return 0f;
		}
		return vpheadlight.GetLightLevel();
	}

	public void UseHorn()
	{
		string hornSoundName = this.vehicle.GetHornSoundName();
		if (hornSoundName.Length > 0)
		{
			this.PlayOneShot(hornSoundName, false, false, false);
		}
	}

	public bool HasDriver
	{
		get
		{
			return this.hasDriver;
		}
	}

	public override bool linkCapsuleSizeToBoundingBox
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float CalcWaterDepth(float offsetY)
	{
		Vector3 position = this.position;
		position.y += offsetY;
		Vector3i vector3i = World.worldToBlockPos(position);
		if (this.world.IsWater(vector3i))
		{
			for (int i = 0; i < 5; i++)
			{
				vector3i.y++;
				if (!this.world.IsWater(vector3i))
				{
					break;
				}
			}
			return (float)vector3i.y - position.y;
		}
		return 0f;
	}

	public override int Health
	{
		set
		{
			base.Stats.Health.Value = (float)value;
			if (this.vehicle != null)
			{
				this.vehicle.FireEvent(Vehicle.Event.HealthChanged);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override DamageResponse damageEntityLocal(DamageSource _damageSource, int _strength, bool _criticalHit, float impulseScale)
	{
		DamageResponse damageResponse = new DamageResponse
		{
			Source = _damageSource,
			Strength = _strength,
			Critical = _criticalHit,
			HitDirection = Utils.EnumHitDirection.None,
			MovementState = this.MovementState,
			Random = this.rand.RandomFloat,
			ImpulseScale = impulseScale
		};
		this.ProcessDamageResponseLocal(damageResponse);
		return damageResponse;
	}

	public override void ProcessDamageResponseLocal(DamageResponse _dmResponse)
	{
		DamageSource source = _dmResponse.Source;
		if (source.damageType == EnumDamageTypes.Disease || source.damageType == EnumDamageTypes.Suffocation)
		{
			return;
		}
		this.UpdateInteractionUI();
		int strength = _dmResponse.Strength;
		bool critical = _dmResponse.Critical;
		float impulseScale = _dmResponse.ImpulseScale;
		float num = 0f;
		if (base.AttachedMainEntity)
		{
			num = this.vehicle.GetPlayerDamagePercent();
			if (!this.isEntityRemote && this.world.IsWorldEvent(World.WorldEvent.BloodMoon))
			{
				this.velocityMax *= 0.6f;
				this.vehicleRB.AddRelativeForce(_dmResponse.Source.getDirection() * 6f, ForceMode.VelocityChange);
			}
		}
		float num2 = (float)strength * (1f - num);
		this.ApplyDamage((int)num2);
		if (base.AttachedMainEntity && source.bIsDamageTransfer)
		{
			base.AttachedMainEntity.DamageEntity(source, (int)Utils.FastMax((float)strength * num, 2f), critical, impulseScale);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ApplyDamage(int damage)
	{
		int num = this.Health;
		if (num <= 0)
		{
			return;
		}
		bool flag = damage >= 99999;
		if (num == 1 || flag)
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				this.explodeHealth -= (float)damage;
				if (this.explodeHealth <= 0f && (flag || this.rand.RandomFloat < 0.2f))
				{
					this.DropItemsAsBackpack();
					this.Kill();
					GameManager.Instance.ExplosionServer(0, base.GetPosition(), World.worldToBlockPos(base.GetPosition()), base.transform.rotation, EntityClass.list[this.entityClass].explosionData, this.entityId, 0f, false, null);
					return;
				}
			}
		}
		else
		{
			num -= damage;
			if (num <= 1)
			{
				num = 1;
				this.explodeHealth = (float)this.vehicle.GetMaxHealth() * 0.03f;
			}
			this.Health = num;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ApplyCollisionDamageToAttached(int damage)
	{
		DamageSource damageSource = new DamageSource(EnumDamageSource.Internal, EnumDamageTypes.VehicleInside, true);
		int attachMaxCount = base.GetAttachMaxCount();
		for (int i = 0; i < attachMaxCount; i++)
		{
			Entity attached = base.GetAttached(i);
			if (attached)
			{
				attached.DamageEntity(damageSource, damage, false, 1f);
			}
		}
	}

	public override bool HasImmunity(BuffClass _buffClass)
	{
		return _buffClass.DamageType != EnumDamageTypes.Heat;
	}

	public bool IsLockedForLocalPlayer(EntityAlive _entityFocusing)
	{
		bool flag = this.LocalPlayerIsOwner();
		return this.isLocked && !flag && this.hasLock() && !this.isAllowedUser(PlatformManager.InternalLocalUserIdentifier);
	}

	public override EntityActivationCommand[] GetActivationCommands(Vector3i _tePos, EntityAlive _entityFocusing)
	{
		if (this.IsDead())
		{
			return new EntityActivationCommand[0];
		}
		bool flag = this.LocalPlayerIsOwner();
		bool flag2 = !this.isLocked || flag || !this.hasLock() || this.isAllowedUser(PlatformManager.InternalLocalUserIdentifier);
		bool flag3 = base.CanAttach(_entityFocusing) && this.isDriveable();
		bool flag4 = base.IsDriven();
		EntityActivationCommand entityActivationCommand;
		if (!flag4)
		{
			entityActivationCommand = new EntityActivationCommand("drive", "drive", flag3 && flag2);
		}
		else
		{
			entityActivationCommand = new EntityActivationCommand("ride", "drive", flag3 && flag2);
		}
		return new EntityActivationCommand[]
		{
			entityActivationCommand,
			new EntityActivationCommand("service", "service", flag2),
			new EntityActivationCommand("repair", "wrench", this.vehicle.GetRepairAmountNeeded() > 0),
			new EntityActivationCommand("lock", "lock", this.hasLock() && !this.isLocked && !flag4),
			new EntityActivationCommand("unlock", "unlock", this.hasLock() && this.isLocked && flag),
			new EntityActivationCommand("keypad", "keypad", this.hasLock() && this.isLocked && (flag || this.vehicle.PasswordHash != 0)),
			new EntityActivationCommand("refuel", "gas", this.hasGasCan(_entityFocusing) && this.needsFuel()),
			new EntityActivationCommand("take", "hand", !this.hasDriver && flag2),
			new EntityActivationCommand("horn", "horn", this.vehicle.HasHorn()),
			new EntityActivationCommand("storage", "loot_sack", flag2)
		};
	}

	public override bool OnEntityActivated(int _indexInBlockActivationCommands, Vector3i _tePos, EntityAlive _entityFocusing)
	{
		EntityPlayerLocal entityPlayerLocal = _entityFocusing as EntityPlayerLocal;
		LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(entityPlayerLocal);
		if (entityPlayerLocal.inventory.IsHoldingItemActionRunning() || uiforPlayer.xui.isUsingItemActionEntryUse)
		{
			return false;
		}
		int num = -1;
		switch (_indexInBlockActivationCommands)
		{
		case 0:
			if ((!(uiforPlayer != null) || !uiforPlayer.windowManager.IsWindowOpen("windowpaging")) && base.CanAttach(_entityFocusing) && _entityFocusing.AttachedToEntity == null && this.isDriveable() && (!this.isLocked || !this.hasLock() || this.LocalPlayerIsOwner() || this.isAllowedUser(PlatformManager.InternalLocalUserIdentifier)))
			{
				if (EffectManager.GetValue(PassiveEffects.NoVehicle, null, 0f, entityPlayerLocal, null, base.EntityClass.Tags, true, true, true, true, true, 1, true, false) > 0f)
				{
					Manager.PlayInsidePlayerHead("twitch_no_attack", -1, 0f, false, false);
				}
				else
				{
					Vector3 vector = this.position - Origin.position;
					vector.y += 0.5f;
					Vector3 up = Vector3.up;
					bool flag = false;
					for (int i = 0; i < 8; i++)
					{
						Vector3 a = Quaternion.AngleAxis((float)(i * 45), up) * base.transform.forward;
						if (Physics.Raycast(vector + a * 0.25f, up, 1.3f, 65536))
						{
							flag = true;
							Vector3 vector2 = _entityFocusing.position - Origin.position;
							vector2.y += 1.1f;
							vector2 = (vector2 - vector).normalized * this.vehicleRB.mass * 0.005f;
							this.AddForce(vector2, ForceMode.VelocityChange);
							break;
						}
					}
					if (!flag)
					{
						this.EnterVehicle(_entityFocusing);
					}
				}
			}
			break;
		case 1:
			num = _indexInBlockActivationCommands;
			break;
		case 2:
			num = _indexInBlockActivationCommands;
			break;
		case 3:
			this.vehicle.SetLocked(true, entityPlayerLocal);
			this.PlayOneShot("misc/locking", true, false, false);
			this.SendSyncData(2);
			break;
		case 4:
			this.vehicle.SetLocked(false, entityPlayerLocal);
			this.PlayOneShot("misc/unlocking", true, false, false);
			this.SendSyncData(2);
			break;
		case 5:
			this.PlayOneShot("misc/password_type", true, false, false);
			XUiC_KeypadWindow.Open(uiforPlayer, this);
			break;
		case 6:
			num = _indexInBlockActivationCommands;
			break;
		case 7:
			num = _indexInBlockActivationCommands;
			break;
		case 8:
			this.UseHorn();
			break;
		case 9:
			num = _indexInBlockActivationCommands;
			break;
		}
		if (num >= 0)
		{
			this.interactionRequestType = num;
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				this.ValidateInteractingPlayer();
				int entityId = this.interactingPlayerId;
				if (entityId == -1)
				{
					entityId = entityPlayerLocal.entityId;
				}
				this.StartInteraction(entityPlayerLocal.entityId, entityId);
			}
			else
			{
				this.interactingPlayerId = entityPlayerLocal.entityId;
				this.SendSyncData(4096);
				this.interactingPlayerId = -1;
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CheckInteractionRequest(int _playerId, int _requestId)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			if (_requestId != -1)
			{
				this.ValidateInteractingPlayer();
				ushort num = 4096;
				if (this.interactingPlayerId == -1)
				{
					this.interactingPlayerId = _playerId;
					num |= 14;
				}
				NetPackageVehicleDataSync package = NetPackageManager.GetPackage<NetPackageVehicleDataSync>().Setup(this, _playerId, num);
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(package, false, _playerId, -1, -1, null, 192);
				return;
			}
			if (this.interactingPlayerId == _playerId)
			{
				this.interactingPlayerId = -1;
				return;
			}
		}
		else
		{
			this.StartInteraction(_playerId, _requestId);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ValidateInteractingPlayer()
	{
		if (!GameManager.Instance.World.GetEntity(this.interactingPlayerId))
		{
			this.interactingPlayerId = -1;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void StartInteraction(int _playerId, int _requestId)
	{
		EntityPlayerLocal localPlayerFromID = GameManager.Instance.World.GetLocalPlayerFromID(_playerId);
		if (!localPlayerFromID)
		{
			return;
		}
		if (_requestId != _playerId)
		{
			GameManager.ShowTooltip(localPlayerFromID, Localization.Get("ttVehicleInUse", false), string.Empty, "ui_denied", null, false);
			return;
		}
		this.interactingPlayerId = _playerId;
		LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(localPlayerFromID);
		GUIWindowManager windowManager = uiforPlayer.windowManager;
		ushort num = 0;
		switch (this.interactionRequestType)
		{
		case 1:
			((XUiC_VehicleWindowGroup)((XUiWindowGroup)windowManager.GetWindow("vehicle")).Controller).CurrentVehicleEntity = this;
			windowManager.Open("vehicle", true, false, true);
			Manager.BroadcastPlayByLocalPlayer(this.position, "UseActions/service_vehicle");
			return;
		case 2:
			if (XUiM_Vehicle.RepairVehicle(uiforPlayer.xui, this.vehicle))
			{
				num |= 4;
				this.PlayOneShot("crafting/craft_repair_item", true, false, false);
			}
			this.StopInteraction(num);
			return;
		case 3:
		case 4:
		case 5:
		case 8:
			break;
		case 6:
			if (this.AddFuelFromInventory(localPlayerFromID))
			{
				num |= 4;
			}
			this.StopInteraction(num);
			return;
		case 7:
			if (!this.bag.IsEmpty())
			{
				GameManager.ShowTooltip(localPlayerFromID, Localization.Get("ttEmptyVehicleBeforePickup", false), string.Empty, "ui_denied", null, false);
				this.StopInteraction(0);
				return;
			}
			if (!this.hasDriver)
			{
				ItemStack itemStack = new ItemStack(this.vehicle.GetUpdatedItemValue(), 1);
				if (localPlayerFromID.inventory.CanTakeItem(itemStack) || localPlayerFromID.bag.CanTakeItem(itemStack))
				{
					GameManager.Instance.CollectEntityServer(this.entityId, localPlayerFromID.entityId);
				}
				else
				{
					GameManager.ShowTooltip(localPlayerFromID, Localization.Get("xuiInventoryFullForPickup", false), string.Empty, "ui_denied", null, false);
				}
			}
			this.StopInteraction(0);
			return;
		case 9:
			((XUiC_VehicleStorageWindowGroup)((XUiWindowGroup)windowManager.GetWindow("vehicleStorage")).Controller).CurrentVehicleEntity = this;
			windowManager.Open("vehicleStorage", true, false, true);
			break;
		default:
			return;
		}
	}

	public bool CheckUIInteraction()
	{
		EntityPlayerLocal localPlayerFromID = GameManager.Instance.World.GetLocalPlayerFromID(this.interactingPlayerId);
		if (!localPlayerFromID)
		{
			return false;
		}
		float distanceSq = base.GetDistanceSq(localPlayerFromID);
		float num = Constants.cDigAndBuildDistance + 1f;
		return distanceSq <= num * num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateInteractionUI()
	{
		if (GameManager.Instance.World == null)
		{
			return;
		}
		for (int i = 0; i < LocalPlayerUI.PlayerUIs.Count; i++)
		{
			LocalPlayerUI localPlayerUI = LocalPlayerUI.PlayerUIs[i];
			if (localPlayerUI != null && localPlayerUI.xui != null && localPlayerUI.windowManager.IsWindowOpen("vehicle"))
			{
				XUiWindowGroup xuiWindowGroup = (XUiWindowGroup)localPlayerUI.windowManager.GetWindow("vehicle");
				if (xuiWindowGroup != null && xuiWindowGroup.Controller != null)
				{
					xuiWindowGroup.Controller.RefreshBindingsSelfAndChildren();
				}
			}
		}
	}

	public void StopUIInteraction()
	{
		this.StopInteraction(14);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void StopInteraction(ushort syncFlags = 0)
	{
		this.interactingPlayerId = -1;
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			syncFlags |= 4096;
		}
		if (syncFlags != 0)
		{
			this.SendSyncData(syncFlags);
		}
	}

	public void Collect(int _playerId)
	{
		EntityPlayerLocal entityPlayerLocal = this.world.GetEntity(_playerId) as EntityPlayerLocal;
		LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(entityPlayerLocal);
		ItemStack itemStack = new ItemStack(this.vehicle.GetUpdatedItemValue(), 1);
		if (!uiforPlayer.xui.PlayerInventory.AddItem(itemStack))
		{
			GameManager.Instance.ItemDropServer(itemStack, entityPlayerLocal.GetPosition(), Vector3.zero, _playerId, 60f, false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DropItemsAsBackpack()
	{
		List<ItemStack> list = new List<ItemStack>();
		foreach (ItemStack itemStack in this.bag.GetSlots())
		{
			if (!itemStack.IsEmpty())
			{
				list.Add(itemStack);
			}
		}
		ItemValue updatedItemValue = this.vehicle.GetUpdatedItemValue();
		for (int j = 0; j < updatedItemValue.CosmeticMods.Length; j++)
		{
			ItemValue itemValue = updatedItemValue.CosmeticMods[j];
			if (itemValue != null && !itemValue.IsEmpty())
			{
				list.Add(new ItemStack(itemValue, 1));
			}
		}
		for (int k = 0; k < updatedItemValue.Modifications.Length; k++)
		{
			ItemValue itemValue2 = updatedItemValue.Modifications[k];
			if (itemValue2 != null && !itemValue2.IsEmpty())
			{
				list.Add(new ItemStack(itemValue2, 1));
			}
		}
		Vector3 position = this.position;
		position.y += 0.9f;
		EntityLootContainer entityLootContainer = GameManager.Instance.DropContentInLootContainerServer(-1, "DroppedVehicleContainer", position, list.ToArray(), false);
		if (entityLootContainer)
		{
			Vector3 vector = this.rand.RandomOnUnitSphere * 16f;
			vector.y = Utils.FastAbs(vector.y);
			vector.y += 8f;
			entityLootContainer.AddVelocity(vector);
		}
	}

	public void AddMaxFuel()
	{
		this.vehicle.AddFuel(this.vehicle.GetMaxFuelLevel());
	}

	public bool AddFuelFromInventory(EntityAlive entity)
	{
		if (this.vehicle.GetFuelPercent() < 1f)
		{
			float maxFuelLevel = this.vehicle.GetMaxFuelLevel();
			float fuelLevel = this.vehicle.GetFuelLevel();
			float f = Mathf.Min(2500f, (maxFuelLevel - fuelLevel) * 25f);
			float num = this.takeFuel(entity, Mathf.CeilToInt(f));
			this.vehicle.AddFuel(num / 25f);
			this.PlayOneShot("useactions/gas_refill", false, false, false);
			return true;
		}
		return false;
	}

	public int GetFuelCount()
	{
		return Mathf.FloorToInt(this.vehicle.GetFuelLevel() * 25f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float takeFuel(EntityAlive _entityFocusing, int count)
	{
		EntityPlayer entityPlayer = _entityFocusing as EntityPlayer;
		if (!entityPlayer)
		{
			return 0f;
		}
		string fuelItem = this.GetVehicle().GetFuelItem();
		if (fuelItem == "")
		{
			return 0f;
		}
		ItemValue item = ItemClass.GetItem(fuelItem, false);
		int num = entityPlayer.inventory.DecItem(item, count, false, null);
		if (num == 0)
		{
			num = entityPlayer.bag.DecItem(item, count, false, null);
			if (num == 0)
			{
				return 0f;
			}
		}
		float num2 = (float)num;
		LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(_entityFocusing as EntityPlayerLocal);
		if (null != uiforPlayer)
		{
			ItemStack @is = new ItemStack(item, num);
			uiforPlayer.xui.CollectedItemList.RemoveItemStack(@is);
		}
		else
		{
			Log.Warning("EntityVehicle::takeFuel - Failed to remove item stack from player's collected item list.");
		}
		return num2;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isEntityStatic()
	{
		return true;
	}

	public override bool CanBePushed()
	{
		return false;
	}

	public Vehicle GetVehicle()
	{
		return this.vehicle;
	}

	public void SetBagModified()
	{
		this.SendSyncData(8);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SendSyncData(ushort syncFlags)
	{
		NetPackageVehicleDataSync package = NetPackageManager.GetPackage<NetPackageVehicleDataSync>().Setup(this, GameManager.Instance.World.GetPrimaryPlayerId(), syncFlags);
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(package, false);
			return;
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(package, false, -1, -1, -1, null, 192);
	}

	public ushort GetSyncFlagsReplicated(ushort syncFlags)
	{
		return syncFlags & 49159;
	}

	public override void Read(byte _version, BinaryReader _br)
	{
		base.Read(_version, _br);
		if (_version < 26)
		{
			Log.Warning("Vehicle: Ignoring old data v{0}", new object[]
			{
				_version
			});
			return;
		}
		ushort syncFlags = _br.ReadUInt16();
		this.ReadSyncData(_br, syncFlags, 0);
	}

	public void ReadSyncData(BinaryReader _br, ushort syncFlags, int senderId)
	{
		_br.ReadByte();
		if ((syncFlags & 32768) > 0)
		{
			this.incomingRemoteData.Flags = _br.ReadInt32();
			this.incomingRemoteData.Flags = (this.incomingRemoteData.Flags | 1);
			this.incomingRemoteData.MotorTorquePercent = (float)_br.ReadInt16() * 0.0001f;
			this.incomingRemoteData.SteeringPercent = (float)_br.ReadInt16() * 0.0001f;
			this.incomingRemoteData.Velocity = StreamUtils.ReadVector3(_br);
			List<EntityVehicle.RemoteData.Part> list = new List<EntityVehicle.RemoteData.Part>(4);
			this.incomingRemoteData.parts = list;
			for (;;)
			{
				byte b = _br.ReadByte();
				if (b == 0)
				{
					break;
				}
				EntityVehicle.RemoteData.Part item;
				if (b == 2)
				{
					item.pos = StreamUtils.ReadVector3(_br);
				}
				else
				{
					item.pos = Vector3.zero;
				}
				item.rot = StreamUtils.ReadQuaterion(_br);
				list.Add(item);
			}
		}
		if ((syncFlags & 16384) > 0)
		{
			this.IsHeadlightOn = _br.ReadBoolean();
		}
		if ((syncFlags & 1) > 0)
		{
			this.delayedAttachments.Clear();
			int num = (int)_br.ReadByte();
			for (int i = 0; i < num; i++)
			{
				int num2 = _br.ReadInt32();
				if (num2 != -1)
				{
					EntityVehicle.DelayedAttach item2;
					item2.entityId = num2;
					item2.slot = i;
					this.delayedAttachments.Add(item2);
				}
				else
				{
					Entity attached = base.GetAttached(i);
					if (attached)
					{
						attached.Detach();
					}
				}
			}
		}
		if ((syncFlags & 2) > 0)
		{
			byte b2 = _br.ReadByte();
			this.isInteractionLocked = ((b2 & 1) > 0);
			this.isLocked = ((b2 & 2) > 0);
			this.vehicle.OwnerId = PlatformUserIdentifierAbs.FromStream(_br, false, false);
			this.vehicle.PasswordHash = _br.ReadInt32();
			this.vehicle.AllowedUsers.Clear();
			int num3 = (int)_br.ReadByte();
			for (int j = 0; j < num3; j++)
			{
				this.vehicle.AllowedUsers.Add(PlatformUserIdentifierAbs.FromStream(_br, false, false));
			}
		}
		if ((syncFlags & 4) > 0)
		{
			int num4 = (int)_br.ReadByte();
			ItemStack[] array = new ItemStack[num4];
			for (int k = 0; k < num4; k++)
			{
				ItemStack itemStack = new ItemStack();
				itemStack.Read(_br);
				array[k] = itemStack;
			}
			this.vehicle.LoadItems(array);
		}
		if ((syncFlags & 8) > 0)
		{
			int num5 = (int)_br.ReadByte();
			ItemStack[] array2 = new ItemStack[num5];
			for (int l = 0; l < num5; l++)
			{
				ItemStack itemStack2 = new ItemStack();
				array2[l] = itemStack2.Read(_br);
			}
			this.bag.SetSlots(array2);
		}
		if ((syncFlags & 4096) > 0)
		{
			int requestId = _br.ReadInt32();
			this.CheckInteractionRequest(senderId, requestId);
		}
	}

	public override void Write(BinaryWriter _bw, bool _bNetworkWrite)
	{
		base.Write(_bw, _bNetworkWrite);
		ushort num = _bNetworkWrite ? 16399 : 16398;
		_bw.Write(num);
		this.WriteSyncData(_bw, num);
	}

	public void WriteSyncData(BinaryWriter _bw, ushort syncFlags)
	{
		_bw.Write(0);
		if ((syncFlags & 32768) > 0)
		{
			int num = 0;
			if (this.vehicle.CurrentIsAccel)
			{
				num |= 2;
			}
			if (this.vehicle.CurrentIsBreak)
			{
				num |= 4;
			}
			_bw.Write(num);
			_bw.Write((short)(this.vehicle.CurrentMotorTorquePercent * 10000f));
			_bw.Write((short)(this.vehicle.CurrentSteeringPercent * 10000f));
			StreamUtils.Write(_bw, this.vehicle.CurrentVelocity);
			int num2 = this.wheels.Length;
			for (int i = 0; i < num2; i++)
			{
				EntityVehicle.Wheel wheel = this.wheels[i];
				if (wheel.steerT && wheel.isSteerParentOfTire)
				{
					_bw.Write(1);
					StreamUtils.Write(_bw, wheel.steerT.localRotation);
				}
				if (wheel.tireT)
				{
					_bw.Write(2);
					StreamUtils.Write(_bw, wheel.tireT.localPosition);
					StreamUtils.Write(_bw, wheel.tireT.localRotation);
				}
			}
			_bw.Write(0);
		}
		if ((syncFlags & 16384) > 0)
		{
			_bw.Write(this.IsHeadlightOn);
		}
		if ((syncFlags & 1) > 0)
		{
			int attachMaxCount = base.GetAttachMaxCount();
			_bw.Write((byte)attachMaxCount);
			for (int j = 0; j < attachMaxCount; j++)
			{
				Entity attached = base.GetAttached(j);
				_bw.Write(attached ? attached.entityId : -1);
			}
		}
		if ((syncFlags & 2) > 0)
		{
			byte b = 0;
			if (this.isInteractionLocked)
			{
				b |= 1;
			}
			if (this.isLocked)
			{
				b |= 2;
			}
			_bw.Write(b);
			this.vehicle.OwnerId.ToStream(_bw, false);
			_bw.Write(this.vehicle.PasswordHash);
			_bw.Write((byte)this.vehicle.AllowedUsers.Count);
			for (int k = 0; k < this.vehicle.AllowedUsers.Count; k++)
			{
				this.vehicle.AllowedUsers[k].ToStream(_bw, false);
			}
		}
		if ((syncFlags & 4) > 0)
		{
			_bw.Write(1);
			this.vehicle.GetItems()[0].Write(_bw);
		}
		if ((syncFlags & 8) > 0)
		{
			ItemStack[] slots = this.bag.GetSlots();
			_bw.Write((byte)slots.Length);
			for (int l = 0; l < slots.Length; l++)
			{
				slots[l].Write(_bw);
			}
		}
		if ((syncFlags & 4096) > 0)
		{
			_bw.Write(this.interactingPlayerId);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool isDriveable()
	{
		return this.vehicle.IsDriveable();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool hasStorage()
	{
		return this.vehicle.HasStorage();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool hasHandlebars()
	{
		return this.vehicle.HasSteering();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool HasChassis()
	{
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool needsFuel()
	{
		return this.vehicle.HasEnginePart() && this.vehicle.GetFuelPercent() < 1f;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool hasGasCan(EntityAlive _ea)
	{
		string fuelItem = this.GetVehicle().GetFuelItem();
		if (fuelItem == "")
		{
			return false;
		}
		ItemValue item = ItemClass.GetItem(fuelItem, false);
		int num = 0;
		ItemStack[] slots = _ea.bag.GetSlots();
		for (int i = 0; i < slots.Length; i++)
		{
			if (slots[i].itemValue.type == item.type)
			{
				num++;
			}
		}
		for (int j = 0; j < _ea.inventory.PUBLIC_SLOTS; j++)
		{
			if (_ea.inventory.GetItem(j).itemValue.type == item.type)
			{
				num++;
			}
		}
		return num > 0;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool hasLock()
	{
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool isAllowedUser(PlatformUserIdentifierAbs _userIdentifier)
	{
		return this.vehicle.AllowedUsers.Contains(_userIdentifier);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void playStepSound(string stepSound)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void updateTasks()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool canDespawn()
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isRadiationSensitive()
	{
		return false;
	}

	public bool CheckPassword(string _password, PlatformUserIdentifierAbs _steamId, out bool changed)
	{
		changed = false;
		bool flag = Utils.HashString(_password) == this.vehicle.PasswordHash.ToString();
		if (this.LocalPlayerIsOwner())
		{
			if (!flag)
			{
				changed = true;
				this.vehicle.PasswordHash = _password.GetHashCode();
				this.vehicle.AllowedUsers.Clear();
				if (this.vehicle.OwnerId == null)
				{
					this.SetOwner(_steamId);
					this.isLocked = true;
				}
				this.SendSyncData(2);
			}
			return true;
		}
		if (flag)
		{
			this.vehicle.AllowedUsers.Add(_steamId);
			this.SendSyncData(2);
			return true;
		}
		return false;
	}

	public int EntityId
	{
		get
		{
			return this.entityId;
		}
		set
		{
		}
	}

	public bool IsLocked()
	{
		return this.isLocked;
	}

	public void SetLocked(bool _isLocked)
	{
		this.isLocked = _isLocked;
	}

	public PlatformUserIdentifierAbs GetOwner()
	{
		return this.vehicle.OwnerId;
	}

	public override void OnAddedToWorld()
	{
		this.bSpawned = true;
		this.vehicle.OwnerId = this.vehicle.OwnerId;
		this.HandleNavObject();
	}

	public void SetOwner(PlatformUserIdentifierAbs _userIdentifier)
	{
		this.vehicle.OwnerId = _userIdentifier;
	}

	public bool IsUserAllowed(PlatformUserIdentifierAbs _userIdentifier)
	{
		return this.vehicle.AllowedUsers.Contains(_userIdentifier);
	}

	public List<PlatformUserIdentifierAbs> GetUsers()
	{
		return new List<PlatformUserIdentifierAbs>();
	}

	public bool LocalPlayerIsOwner()
	{
		return this.IsOwner(PlatformManager.InternalLocalUserIdentifier);
	}

	public bool IsOwner(PlatformUserIdentifierAbs _userIdentifier)
	{
		return this.vehicle.OwnerId == null || this.vehicle.OwnerId.Equals(_userIdentifier);
	}

	public bool HasPassword()
	{
		return this.vehicle.PasswordHash != 0;
	}

	public string GetPassword()
	{
		return this.vehicle.PasswordHash.ToString();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CheckForOutOfWorld()
	{
		if (this.bDead)
		{
			return;
		}
		Vector3 vector = this.position;
		if (this.world.AdjustBoundsForPlayers(ref vector, 0.2f))
		{
			if (!this.vehicleRB.isKinematic)
			{
				Vector3 velocity = this.vehicleRB.velocity;
				velocity.x *= -0.5f;
				velocity.z *= -0.5f;
				this.vehicleRB.velocity = velocity;
			}
			vector.y = this.vehicleRB.position.y + Origin.position.y;
			this.SetPosition(vector, true);
			EntityPlayerLocal attachedPlayerLocal = base.GetAttachedPlayerLocal();
			if (attachedPlayerLocal)
			{
				GameManager.ShowTooltip(attachedPlayerLocal, Localization.Get("ttWorldEnd", false), false);
			}
			return;
		}
		Vector3 centerPosition = this.GetCenterPosition();
		Chunk chunk = (Chunk)this.world.GetChunkFromWorldPos((int)centerPosition.x, (int)centerPosition.y, (int)centerPosition.z);
		if (chunk == null || !chunk.IsCollisionMeshGenerated || !chunk.IsDisplayed)
		{
			if (!this.vehicleRB.isKinematic)
			{
				this.vehicleRB.velocity = Vector3.zero;
				this.vehicleRB.angularVelocity = Vector3.zero;
			}
			if (!this.hasDriver)
			{
				this.RBActive = false;
				this.isTryToFall = true;
			}
			return;
		}
		Entity firstAttached = base.GetFirstAttached();
		if (firstAttached && !firstAttached.IsSpawned())
		{
			return;
		}
		if (this.RBActive && !this.IsTerrainBelow(centerPosition))
		{
			int num = this.worldTerrainFailCount + 1;
			this.worldTerrainFailCount = num;
			if (num <= 6)
			{
				if (this.worldTerrainFailCount == 2)
				{
					chunk.NeedsRegeneration = true;
					this.LogVehicle("{0}, {1}, center {2}, rbPos {3}, in ground. Chunk regen {4}", new object[]
					{
						base.transform.parent.name,
						vector.ToCultureInvariantString(),
						centerPosition.ToCultureInvariantString(),
						(this.vehicleRB.position + Origin.position).ToCultureInvariantString(),
						chunk
					});
				}
			}
			else if (this.hasWorldValidPos)
			{
				Vector3 vector2 = this.worldValidPos - vector;
				if (vector2.y < 0f)
				{
					vector2.y = 0f;
				}
				float sqrMagnitude = vector2.sqrMagnitude;
				vector2 = vector2.normalized;
				if (sqrMagnitude > 0.122499995f)
				{
					vector = this.worldValidPos + vector2 * 0.1f;
					this.SetPosition(vector, true);
				}
				if (!this.vehicleRB.isKinematic)
				{
					Vector3 vector3 = this.vehicleRB.velocity;
					if (Vector3.Dot(vector3, vector2) < 0f)
					{
						vector3 *= -0.5f;
					}
					vector3.y = 1f + this.rand.RandomFloat * 2f;
					vector3 += vector2 * 3f;
					this.vehicleRB.velocity = vector3;
					this.vehicleRB.angularVelocity = Vector3.zero;
				}
				this.LogVehicle("{0}, {1}, center {2} in ground. back {3}", new object[]
				{
					base.transform.parent.name,
					vector.ToCultureInvariantString(),
					centerPosition.ToCultureInvariantString(),
					this.worldValidPos.ToCultureInvariantString()
				});
				this.worldValidPos.x = this.worldValidPos.x + (this.rand.RandomFloat - 0.5f) * 2f * 0.05f;
				this.worldValidPos.z = this.worldValidPos.z + (this.rand.RandomFloat - 0.5f) * 2f * 0.05f;
				this.worldValidPos.y = this.worldValidPos.y + 0.001f;
				this.worldValidDelay -= Time.deltaTime;
				if (this.worldValidDelay <= 0f)
				{
					this.worldValidDelay = 1f;
					this.worldValidPos.y = this.worldValidPos.y + 1.2f;
				}
			}
			else
			{
				Vector3 pos = centerPosition;
				pos.y = 257f;
				bool flag = this.IsTerrainBelow(pos);
				if (flag)
				{
					vector.y += 3f;
					this.SetPosition(vector, true);
				}
				this.LogVehicle("{0}, {1}, center {2} (vel {3}, {4}) {5}", new object[]
				{
					base.transform.parent.name,
					vector.ToCultureInvariantString(),
					centerPosition.ToCultureInvariantString(),
					this.vehicleRB.velocity,
					this.vehicleRB.angularVelocity,
					flag ? " in ground. up" : " out of world"
				});
				if (!this.vehicleRB.isKinematic)
				{
					this.vehicleRB.velocity *= 0.5f;
					this.vehicleRB.angularVelocity *= 0.5f;
				}
			}
		}
		else
		{
			this.worldTerrainFailCount = 0;
			if (this.hasWorldValidPos)
			{
				if ((this.worldValidPos - vector).sqrMagnitude > 4f)
				{
					this.worldValidPos = vector;
				}
			}
			else
			{
				this.hasWorldValidPos = true;
				this.worldValidPos = vector;
			}
		}
		if (this.isTryToFall)
		{
			this.isTryToFall = false;
			this.RBActive = true;
			this.vehicleRB.WakeUp();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsTerrainBelow(Vector3 pos)
	{
		Ray ray = new Ray(pos - Origin.position, Vector3.down);
		RaycastHit raycastHit;
		if (Physics.Raycast(ray, out raycastHit, 3.40282347E+38f, 1073807360))
		{
			return true;
		}
		Utils.DrawCircleLinesHorzontal(ray.origin, 0.25f, new Color(1f, 1f, 0f), new Color(1f, 0f, 0f), 8, 5f);
		Utils.DrawLine(ray.origin, new Vector3(ray.origin.x, 0f - Origin.position.y, ray.origin.z), new Color(1f, 1f, 0f), new Color(1f, 0f, 0f), 5, 5f);
		ray.origin += new Vector3(0.02f, 0.5f, 0.03f);
		return Physics.SphereCast(ray, 0.1f, out raycastHit, float.MaxValue, 1073807360);
	}

	public override bool IsDeadIfOutOfWorld()
	{
		return false;
	}

	public override void CheckPosition()
	{
		base.CheckPosition();
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && this.Spawned && !this.hasDriver)
		{
			Vector3i vector3i;
			Vector3i vector3i2;
			this.world.GetWorldExtent(out vector3i, out vector3i2);
			if (this.position.y < (float)vector3i.y)
			{
				Chunk chunk = (Chunk)this.world.GetChunkFromWorldPos(new Vector3i((int)this.position.x, (int)this.position.y, (int)this.position.z));
				if (chunk != null && chunk.IsCollisionMeshGenerated && chunk.IsDisplayed)
				{
					this.TeleportToWithinBounds(vector3i.ToVector3(), vector3i2.ToVector3());
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TeleportToWithinBounds(Vector3 _min, Vector3 _max)
	{
		_min.x += 66f;
		_min.z += 66f;
		_max.x -= 66f;
		_max.z -= 66f;
		Vector3 position = this.position;
		if (position.x < _min.x)
		{
			position.x = _min.x;
		}
		else if (position.x > _max.x)
		{
			position.x = _max.x;
		}
		if (position.z < _min.z)
		{
			position.z = _min.z;
		}
		else if (position.z > _max.z)
		{
			position.z = _max.z;
		}
		RaycastHit raycastHit;
		if (Physics.Raycast(new Ray(new Vector3(position.x, 999f, position.z) - Origin.position, Vector3.down), out raycastHit, 3.40282347E+38f, 1076428800))
		{
			position.y = raycastHit.point.y + Origin.position.y + 1f;
			this.SetPosition(position, true);
			Log.Out("Vehicle out of world. Teleporting to " + position.ToCultureInvariantString());
		}
	}

	public void Kill()
	{
		int attachMaxCount = base.GetAttachMaxCount();
		for (int i = 0; i < attachMaxCount; i++)
		{
			Entity attached = base.GetAttached(i);
			if (attached != null)
			{
				attached.Detach();
			}
		}
		this.timeStayAfterDeath = 0;
		this.SetDead();
		this.MarkToUnload();
	}

	public override void OnEntityUnload()
	{
		if (this.vehicleRB)
		{
			this.position = this.vehicleRB.position + Origin.position;
			this.rotation = this.vehicleRB.rotation.eulerAngles;
		}
		base.OnEntityUnload();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void HandleNavObject()
	{
		if (EntityClass.list[this.entityClass].NavObject != "")
		{
			if (this.LocalPlayerIsOwner())
			{
				this.NavObject = NavObjectManager.Instance.RegisterNavObject(EntityClass.list[this.entityClass].NavObject, this.vehicle.GetMeshTransform(), "", false);
				return;
			}
			if (this.NavObject != null)
			{
				NavObjectManager.Instance.UnRegisterNavObject(this.NavObject);
				this.NavObject = null;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LogVehicle(string format, params object[] args)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer || base.GetAttachedPlayerLocal())
		{
			format = string.Format("{0} Vehicle {1}", GameManager.frameCount, format);
			Log.Out(format, args);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cDamageBlockScale = 0.0583333336f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cDamageBlockVelReduction = 1.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cDamageBlockMin = 5f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cDamageBlockSelfPer = 2.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cDamageTerrainSelfPer = 0.1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cDamageEntityScale = 12f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cDamageEntitySelfScale = 28f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cKillEntityXPPer = 0.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cExitVelScale = 0.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cSleepTime = 3f;

	public bool IsEngineRunning;

	public bool isLocked;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isInteractionLocked;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int interactingPlayerId = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int interactionRequestType;

	public Vehicle vehicle;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isTryToFall;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool hasDriver;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float timeInWater;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public MovementInput movementInput;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static bool isTurnTowardsLook = true;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public EntityVehicle.RemoteData incomingRemoteData;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public EntityVehicle.RemoteData currentRemoteData;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public EntityVehicle.RemoteData lastRemoteData;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cSyncHighRateDuration = 0.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float syncHighRateTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float syncPlayTime = -1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float syncLowRateTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cSyncLowRateDuration = 2f;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Rigidbody vehicleRB;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool RBActive;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float RBNoDriverGndTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float RBNoDriverSleepTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 lastRBPos;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Quaternion lastRBRot;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 lastRBVel;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 lastRBAngVel;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float velocityMax;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float damageAccumulator;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int hitEffectCount;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float explodeHealth;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool canHop;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cCameraOffsetY = 1.8f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 cameraStartPos;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float cameraStartBlend;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector2 cameraStartVec;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 cameraPos;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float cameraDist;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static float cameraDistScale = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float cameraAngle;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float cameraAngleTarget;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float cameraOutTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float cameraVelY;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public EntityVehicle.Force[] forces = new EntityVehicle.Force[0];

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public EntityVehicle.Motor[] motors = new EntityVehicle.Motor[0];

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float motorTorque;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float brakeTorque;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float wheelDir;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float wheelMotor;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float wheelBrakes;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public EntityVehicle.Wheel[] wheels = new EntityVehicle.Wheel[0];

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<EntityVehicle.DelayedAttach> delayedAttachments = new List<EntityVehicle.DelayedAttach>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float collisionBlockDamage;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 collisionVelNorm;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int collisionIgnoreCount;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<ContactPoint> contactPoints = new List<ContactPoint>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<WorldRayHitInfo> collisionHits = new List<WorldRayHitInfo>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int collisionGrazeCount;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cFuelItemScale = 25f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const byte cSyncVersion = 0;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const ushort cSyncAttachment = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const ushort cSyncInteractAndSecurity = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const ushort cSyncItem = 4;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const ushort cSyncStorage = 8;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const ushort cSyncInteractRequest = 4096;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const ushort cSyncLowRate = 16384;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const ushort cSyncHighRate = 32768;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const ushort cSyncAllNonRates = 15;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const ushort cSyncLowRateAndNonRates = 16399;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const ushort cSyncReplicate = 49159;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const ushort cSyncSave = 16398;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const byte cSyncInteractAndSecurityFInteracting = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const byte cSyncInteractAndSecurityFLocked = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int cWorldPad = 66;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool hasWorldValidPos;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 worldValidPos;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float worldValidDelay;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int worldTerrainFailCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public struct RemoteData
	{
		public const int cFHasData = 1;

		public const int cFAccel = 2;

		public const int cFBreak = 4;

		public int Flags;

		public float MotorTorquePercent;

		public float SteeringPercent;

		public Vector3 Velocity;

		public List<EntityVehicle.RemoteData.Part> parts;

		public struct Part
		{
			public Vector3 pos;

			public Quaternion rot;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public class Force
	{
		public Vector2 ceiling;

		public Vector3 force;

		public EntityVehicle.Force.Trigger trigger;

		public EntityVehicle.Force.Type type;

		public enum Trigger
		{
			Off,
			On,
			InputForward,
			InputStrafe,
			InputUp,
			InputDown,
			Motor0,
			Motor1,
			Motor2,
			Motor3,
			Motor4,
			Motor5,
			Motor6,
			Motor7
		}

		public enum Type
		{
			Relative,
			RelativeTorque
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public class Motor
	{
		public VPEngine engine;

		public float engineOffPer;

		public float turbo;

		public float rpm;

		public float rpmAccelMin;

		public float rpmAccelMax;

		public float rpmDrag;

		public float rpmMax;

		public EntityVehicle.Motor.Trigger trigger;

		public EntityVehicle.Motor.Type type;

		public Transform transform;

		public int axis;

		public enum Trigger
		{
			Off,
			On,
			InputForward,
			InputStrafe,
			InputUp,
			InputDown,
			Vel
		}

		public enum Type
		{
			Spin,
			Relative,
			RelativeTorque
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public class Wheel
	{
		public float motorTorqueScale;

		public float brakeTorqueScale;

		public string bounceSound;

		public string slideSound;

		public bool isSteerParentOfTire;

		public Transform steerT;

		public Quaternion steerBaseRot;

		public Transform tireT;

		public float tireSpinSpeed;

		public float tireSpin;

		public float tireSuspensionPercent;

		public WheelCollider wheelC;

		public float slideTime;

		public float ptlTime;

		public bool isGrounded;
	}

	public class VehicleInventory : Inventory
	{
		public VehicleInventory(IGameManager _gameManager, EntityAlive _entity) : base(_gameManager, _entity)
		{
			this.cSlotCount = base.PUBLIC_SLOTS + 1;
			this.SetupSlots();
		}

		public override void Execute(int _actionIdx, bool _bReleased, PlayerActionsLocal _playerActions = null)
		{
		}

		public void SetupSlots()
		{
			this.slots = new ItemInventoryData[this.cSlotCount];
			this.models = new Transform[this.cSlotCount];
			this.m_HoldingItemIdx = 0;
			base.Clear();
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void updateHoldingItem()
		{
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly int cSlotCount;
	}

	public struct DelayedAttach
	{
		public int entityId;

		public int slot;
	}
}

using System;
using System.Collections.Generic;
using GamePath;
using UnityEngine;

public class EntityMoveHelper
{
	public EntityMoveHelper(EntityAlive _entity)
	{
		this.entity = _entity;
		this.random = _entity.rand;
		this.moveToPos = _entity.position;
	}

	public void SetMoveTo(Vector3 _pos, bool _canBreakBlocks)
	{
		this.moveToPos = _pos;
		this.moveSpeed = this.entity.GetMoveSpeedAggro();
		this.focusTicks = 0;
		this.isTempMove = false;
		this.CanBreakBlocks = _canBreakBlocks;
		this.isClimb = false;
		this.IsActive = true;
		this.expiryTicks = 10;
		this.ResetStuckCheck();
	}

	public void SetMoveTo(PathEntity path, float _speed, bool _canBreakBlocks)
	{
		PathPoint currentPoint = path.CurrentPoint;
		Vector3 b = this.moveToPos;
		this.moveToPos = currentPoint.AdjustedPositionForEntity(this.entity);
		this.CanBreakBlocks = _canBreakBlocks;
		bool flag = true;
		if (this.IsActive)
		{
			if ((this.moveToPos - b).sqrMagnitude < 0.0100000007f)
			{
				flag = false;
			}
		}
		else
		{
			this.moveToDir = this.entity.rotation.y;
		}
		if (flag)
		{
			this.focusTicks = 0;
			this.isTempMove = false;
			this.ResetStuckCheck();
		}
		this.hasNextPos = false;
		PathPoint nextPoint = path.NextPoint;
		if (nextPoint != null)
		{
			this.hasNextPos = true;
			this.nextMoveToPos = nextPoint.AdjustedPositionForEntity(this.entity);
		}
		this.moveSpeed = _speed;
		this.isClimb = false;
		this.expiryTicks = 40;
		this.IsActive = true;
	}

	public void Stop()
	{
		this.StopMove();
		this.entity.getNavigator().clearPath();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void StopMove()
	{
		this.IsActive = false;
		if (!this.entity.Jumping || this.entity.isSwimming)
		{
			this.entity.SetMoveForward(0f);
			this.entity.SetRotationAndStopTurning(this.entity.rotation);
		}
		this.expiryTicks = 0;
		this.IsBlocked = false;
		this.BlockedTime = 0f;
		this.BlockedEntity = null;
	}

	public void SetFocusPos(Vector3 _pos)
	{
		this.focusPos = _pos;
		this.focusTicks = 5;
	}

	public void UpdateMoveHelper()
	{
		if (!this.IsActive)
		{
			return;
		}
		int num = this.expiryTicks - 1;
		this.expiryTicks = num;
		if (num <= 0)
		{
			this.StopMove();
			return;
		}
		this.ccHeight = this.entity.m_characterController.GetHeight();
		this.ccRadius = this.entity.m_characterController.GetRadius();
		Vector3 position = this.entity.position;
		Vector3 vector = this.moveToPos;
		if (this.isTempMove)
		{
			if (!this.IsBlocked)
			{
				this.isTempMove = false;
				this.ResetStuckCheck();
			}
			else
			{
				vector = this.tempMoveToPos;
			}
		}
		bool jumping = this.entity.Jumping;
		bool flag = jumping || this.entity.isSwimming;
		bool flag2 = jumping && !this.entity.isSwimming;
		float num2 = vector.x - position.x;
		float num3 = vector.z - position.z;
		float num4 = num2 * num2 + num3 * num3;
		float num5 = vector.y - (position.y + 0.05f);
		bool flag3 = this.entity.IsInElevator();
		this.isClimb = false;
		if (flag3 && this.entity.bCanClimbLadders && num4 < 0.108900011f && num5 > 0.1f && !jumping)
		{
			this.isClimb = true;
		}
		else if (num4 <= 0.0004f && Utils.FastAbs(num5) < 0.25f && !this.isTempMove)
		{
			this.StopMove();
			return;
		}
		AvatarController avatarController = this.entity.emodel.avatarController;
		if (avatarController.IsRootMotionForced())
		{
			this.entity.SetMoveForwardWithModifiers(this.moveSpeed, 1f, false);
			this.ResetStuckCheck();
			this.ClearTempMove();
			this.ClearBlocked();
			return;
		}
		if ((!flag && !this.isDigging && !avatarController.IsAnimationWithMotionRunning()) || this.entity.sleepingOrWakingUp || !this.entity.bodyDamage.HasLimbs || !this.entity.bodyDamage.CurrentStun.CanMove() || this.entity.emodel.IsRagdollActive || avatarController.IsLocomotionPreempted())
		{
			this.entity.SetMoveForward(0f);
			this.ResetStuckCheck();
			this.ClearBlocked();
			return;
		}
		float num6 = this.moveToPos.x - position.x;
		float num7 = this.moveToPos.z - position.z;
		float num8 = num6 * num6 + num7 * num7;
		float num9 = this.moveToPos.y - (position.y + 0.05f);
		if (num9 < -1.1f && num8 <= 0.0100000007f && !flag2 && this.entity.onGround)
		{
			this.DigStart(20);
		}
		if (this.isDigging)
		{
			this.DigUpdate();
			return;
		}
		float num10 = Mathf.Atan2(num6, num7) * 57.29578f;
		if (flag2)
		{
			this.moveToDir = num10;
		}
		else
		{
			this.moveToDir = Mathf.MoveTowardsAngle(this.moveToDir, num10, 13f);
		}
		this.entity.emodel.ClearLookAt();
		if (this.hasNextPos || num8 >= 0.0225f)
		{
			float num11 = 9999f;
			if (flag2)
			{
				num11 = this.jumpYaw;
			}
			else
			{
				float num12 = num6;
				float num13 = num7;
				if (this.hasNextPos && num8 <= 2.25f)
				{
					float t = Mathf.Sqrt(num8) / 1.5f;
					num12 = Mathf.Lerp(this.nextMoveToPos.x, this.moveToPos.x, t) - position.x;
					num13 = Mathf.Lerp(this.nextMoveToPos.z, this.moveToPos.z, t) - position.z;
				}
				if (this.focusTicks > 0)
				{
					this.focusTicks--;
					num12 = this.focusPos.x - position.x;
					num13 = this.focusPos.z - position.z;
				}
				if (num12 * num12 + num13 * num13 > 0.0001f)
				{
					num11 = Mathf.Atan2(num12, num13) * 57.29578f;
				}
			}
			if (num11 < 9000f)
			{
				this.entity.SeekYaw(num11, 0f, 30f);
			}
		}
		float num14 = Mathf.Abs(Mathf.DeltaAngle(num10, this.moveToDir));
		float num15 = 1f;
		if (this.IsUnreachableAbove && !this.entity.IsRunning)
		{
			num15 = 1.3f;
		}
		float num16 = num14 - 15f;
		if (num16 > 0f)
		{
			num15 *= 1f - Utils.FastMin(num16 / 30f, 0.8f);
		}
		if (num15 > 0.5f)
		{
			if (this.BlockedTime > 0.1f)
			{
				num15 = 0.5f;
			}
			if (this.focusTicks > 0)
			{
				num15 = 0.45f;
			}
		}
		if (flag3 && !this.entity.onGround)
		{
			num15 = 0.5f;
		}
		if (this.entity.hasBeenAttackedTime > 0 && this.entity.painResistPercent < 1f)
		{
			num15 = 0.1f;
		}
		if (!this.hasNextPos && !this.isTempMove && !jumping && num4 < 0.36f && num15 > 0.1f)
		{
			float num17 = num15 * Mathf.Sqrt(num4) / 0.6f;
			if (num17 < 0.1f)
			{
				num17 = 0.1f;
			}
			num15 = num17;
		}
		bool isBreakingBlocks = this.entity.IsBreakingBlocks;
		if (isBreakingBlocks)
		{
			num15 = 0.03f;
		}
		this.entity.SetMoveForwardWithModifiers(this.moveSpeed, num15, this.isClimb);
		if (num15 > 0f)
		{
			float x = num2;
			float z = num3;
			float minMotion = 0.02f * num15;
			float maxMotion = 1f;
			if (!this.isTempMove)
			{
				if (this.entity.entityType == EntityType.Bandit)
				{
					this.entity.AddMotion(this.moveToDir, this.entity.speedForward * num15 * 40f * 0.02f);
				}
				if (this.SideStepAngle != 0f)
				{
					float f = (this.moveToDir + this.SideStepAngle) * 0.0174532924f;
					x = Mathf.Sin(f);
					z = Mathf.Cos(f);
					minMotion = 0.025f;
					maxMotion = 0.06f;
					this.moveToPos = Vector3.MoveTowards(this.moveToPos, position, 0.0100000007f);
				}
				else if (num4 > 0.422499955f)
				{
					float f2 = this.moveToDir * 0.0174532924f;
					x = Mathf.Sin(f2);
					z = Mathf.Cos(f2);
				}
			}
			this.entity.MakeMotionMoveToward(x, z, minMotion, maxMotion);
			if (flag3)
			{
				Vector3 vector2 = new Vector3(num2, num5, num3).normalized;
				float num18 = Mathf.Pow(this.moveSpeed, 0.4f);
				if (num5 > 0.1f)
				{
					num18 *= 0.7f;
				}
				else if (num5 < -0.1f)
				{
					num18 *= 1.4f;
				}
				vector2 *= num18 * 0.1f;
				this.entity.motion = vector2;
			}
		}
		if (flag2)
		{
			return;
		}
		if (this.entity.isSwimming && this.entity.swimStrokeRate.x > 0f)
		{
			this.swimStrokeDelayTicks--;
			if (this.swimStrokeDelayTicks <= 0)
			{
				this.swimStrokeDelayTicks = (int)(20f / this.random.RandomRange(this.entity.swimStrokeRate.x, this.entity.swimStrokeRate.y));
				this.StartSwimStroke();
				this.swimStrokeDelayTicks += 3;
			}
		}
		if (isBreakingBlocks || num14 > 60f || num15 == 0f)
		{
			this.moveToTicks = 0;
		}
		else
		{
			num = this.moveToTicks + 1;
			this.moveToTicks = num;
			if (num > 6)
			{
				this.moveToTicks = 0;
				float num19 = Mathf.Sqrt(num2 * num2 + num5 * num5 + num3 * num3);
				float num20 = this.moveToDistance - num19;
				if (num20 < 0.021f)
				{
					if (num20 < -0.01f)
					{
						this.moveToDistance = num19;
					}
					num = this.moveToFailCnt + 1;
					this.moveToFailCnt = num;
					if (num >= 3)
					{
						bool flag4 = num9 < -1.1f && num8 <= 0.640000045f;
						if (flag4 && this.entity.onGround && this.random.RandomFloat < 0.6f)
						{
							this.DigStart(80);
							return;
						}
						this.CheckAreaBlocked();
						if (this.IsBlocked)
						{
							if (this.random.RandomFloat < 0.7f)
							{
								this.DamageScale = 6f;
								this.obstacleCheckTickDelay = 40;
								return;
							}
							this.StartJump(false, 0.5f + this.random.RandomFloat * 0.4f, 1.3f);
							return;
						}
						else
						{
							if (flag4)
							{
								return;
							}
							if (this.random.RandomFloat > 0.5f)
							{
								if (this.entity.Attack(false))
								{
									this.entity.Attack(true);
									return;
								}
							}
							else
							{
								this.StartJump(false, 0.7f + this.random.RandomFloat * 0.8f, 1.4f);
							}
							return;
						}
					}
				}
				else
				{
					this.moveToDistance = num19;
					if (num20 >= 0.07f)
					{
						this.moveToFailCnt = 0;
					}
				}
			}
		}
		if (!this.entity.onGround && !this.entity.isSwimming && !flag3 && !this.isClimb && (num9 < -0.5f || num9 > 0.5f))
		{
			this.BlockedTime = 0f;
			this.BlockedEntity = null;
		}
		else
		{
			num = this.obstacleCheckTickDelay - 1;
			this.obstacleCheckTickDelay = num;
			if (num <= 0)
			{
				this.obstacleCheckTickDelay = 4;
				this.IsBlocked = false;
				this.BlockedEntity = null;
				this.blockedDistSq = float.MaxValue;
				if (this.isClimb)
				{
					this.CheckBlockedUp(position);
				}
				else if (num14 < 10f)
				{
					this.CheckEntityBlocked(position, this.moveToPos);
					this.CheckWorldBlocked();
					this.SideStepAngle = 0f;
					if (!this.IsUnreachableAbove && this.hasNextPos && (this.IsBlocked || this.BlockedEntity))
					{
						this.SideStepAngle = this.CalcObstacleSideStep();
						if (this.SideStepAngle != 0f)
						{
							this.isTempMove = false;
							this.BlockedEntity = null;
							this.IsBlocked = false;
						}
					}
					if (this.BlockedEntity)
					{
						if (!this.IsBlocked || this.blockedEntityDistSq < this.blockedDistSq)
						{
							this.moveToTicks = 0;
							if (this.random.RandomFloat < 0.1f)
							{
								if (this.BlockedEntity.moveHelper != null && this.BlockedEntity.moveHelper.IsBlocked)
								{
									this.StartJump(false, 0.7f, this.BlockedEntity.height * 0.8f);
								}
							}
							else
							{
								this.Push(this.BlockedEntity);
							}
						}
					}
					else if ((this.IsBlocked || !this.hasNextPos) && num9 < -1.5f && num8 >= 2.25f && this.entity.onGround)
					{
						float num21 = Mathf.Sqrt(num8 + num9 * num9) + 0.001f;
						if (num9 / num21 < -0.86f)
						{
							this.DigStart(160);
						}
					}
				}
			}
		}
		if (this.IsBlocked)
		{
			this.BlockedTime += 0.05f;
		}
		else
		{
			this.BlockedTime = 0f;
		}
		if (this.entity.CanEntityJump() && !this.isClimb && !flag)
		{
			float num22 = 0f;
			float heightDiff = 0.9f;
			if (this.BlockedTime > 0.15f && this.blockedHeight < 1f)
			{
				num22 = 0.55f + this.random.RandomFloat * 0.3f;
			}
			else if (num9 > 1.5f && num8 <= 0.0400000028f && this.random.RandomFloat < 0.1f)
			{
				num22 = 0.02f;
			}
			if (this.IsUnreachableSideJump && num14 < 25f)
			{
				PathEntity path = this.entity.navigator.getPath();
				if (path == null || path.NodeCountRemaining() <= 1)
				{
					Vector3 a = this.entity.position + this.entity.GetForwardVector() * 0.2f;
					a.y += 0.4f;
					RaycastHit raycastHit;
					if (!Physics.Raycast(a - Origin.position, Vector3.down, out raycastHit, 3.4f, 1082195968) || raycastHit.distance > 2.2f)
					{
						num22 = this.entity.jumpMaxDistance;
						heightDiff = this.UnreachablePos.y - this.entity.position.y;
					}
				}
			}
			if (num22 > 0f)
			{
				Vector3i vector3i = new Vector3i(Utils.Fastfloor(position.x), Utils.Fastfloor(position.y + 2.35f), Utils.Fastfloor(position.z));
				BlockValue block = this.entity.world.GetBlock(vector3i);
				if (!block.Block.IsMovementBlocked(this.entity.world, vector3i, block, BlockFace.None))
				{
					this.StartJump(true, num22, heightDiff);
					if (this.IsUnreachableSideJump)
					{
						this.UnreachablePercent += 0.1f;
						this.IsDestroyAreaTryUnreachable = true;
					}
				}
				this.IsUnreachableSideJump = false;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CheckWorldBlocked()
	{
		this.DamageScale = 1f;
		Vector3 headPosition = this.entity.getHeadPosition();
		headPosition.x = this.entity.position.x * 0.4f + headPosition.x * 0.6f;
		headPosition.z = this.entity.position.z * 0.4f + headPosition.z * 0.6f;
		headPosition.y = this.entity.position.y;
		float num = Utils.FastClamp(this.ccHeight - 0.125f, 0.7f, 1.5f);
		headPosition.y += num;
		Vector3 endPos = this.moveToPos;
		endPos.y = headPosition.y;
		this.CheckBlocked(headPosition, endPos, (num >= 1f) ? 1 : 0);
		if (num >= 1f)
		{
			if (this.IsBlocked)
			{
				return;
			}
			Vector3 vector = headPosition;
			vector.y = this.entity.position.y + this.entity.stepHeight + 0.125f;
			endPos.y = vector.y;
			this.CheckBlocked(vector, endPos, 0);
			if (!this.IsBlocked)
			{
				return;
			}
		}
		if (this.IsBlocked)
		{
			WorldRayHitInfo hitInfo = this.HitInfo;
			endPos.y = headPosition.y + 1f;
			if (num < 1f)
			{
				headPosition.y += 0.3f;
			}
			this.CheckBlocked(headPosition, endPos, 2);
			if (this.IsBlocked)
			{
				BlockValue blockValue = hitInfo.hit.blockValue;
				float num2 = (float)(blockValue.Block.MaxDamage - blockValue.damage);
				if (this.HitInfo.hit.blockPos.x != Utils.Fastfloor(this.moveToPos.x) || this.HitInfo.hit.blockPos.z != Utils.Fastfloor(this.moveToPos.z))
				{
					this.HitInfo = hitInfo;
				}
				else
				{
					BlockValue blockValue2 = this.HitInfo.hit.blockValue;
					float num3 = (float)(blockValue2.Block.MaxDamage - blockValue2.damage);
					if (num2 * 0.7f < num3)
					{
						this.HitInfo = hitInfo;
					}
				}
			}
			this.IsBlocked = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CheckBlocked(Vector3 pos, Vector3 endPos, int baseY)
	{
		this.IsBlocked = false;
		endPos.y -= 0.01f;
		Vector3 vector = endPos - pos;
		float num = vector.magnitude + 0.001f;
		vector *= 1f / num;
		Ray ray = new Ray(pos - vector * 0.375f, vector);
		if (num > this.ccRadius + 0.35f)
		{
			num = this.ccRadius + 0.35f;
			if (this.isTempMove)
			{
				num += 0.4f;
			}
		}
		if (baseY >= 2)
		{
			num += 0.21f;
		}
		if (Voxel.Raycast(this.entity.world, ray, num - 0.125f + 0.375f, 1082195968, 128, 0.125f))
		{
			if (baseY == 0 && Voxel.phyxRaycastHit.normal.y > 0.643f)
			{
				Vector2 vector2;
				vector2.x = Voxel.phyxRaycastHit.normal.x;
				vector2.y = Voxel.phyxRaycastHit.normal.z;
				vector2.Normalize();
				Vector2 vector3;
				vector3.x = vector.x;
				vector3.y = vector.z;
				vector3.Normalize();
				if (vector3.x * vector2.x + vector3.y * vector2.y < -0.7f)
				{
					return;
				}
			}
			if (Voxel.voxelRayHitInfo.hit.blockValue.Block is BlockDamage)
			{
				return;
			}
			this.HitInfo = Voxel.voxelRayHitInfo.Clone();
			this.IsBlocked = true;
			this.blockedHeight = this.HitInfo.hit.pos.y - this.entity.position.y;
			Vector3 a = pos - this.HitInfo.hit.pos;
			float sqrMagnitude = a.sqrMagnitude;
			if (sqrMagnitude < this.blockedDistSq)
			{
				this.blockedDistSq = sqrMagnitude;
				float num2 = 1f / Mathf.Sqrt(sqrMagnitude);
				float num3 = this.ccRadius + 0.4f;
				this.tempMoveToPos = a * (num2 * num3) + this.HitInfo.hit.pos;
				this.tempMoveToPos.y = Mathf.MoveTowards(this.tempMoveToPos.y, this.moveToPos.y, 1f);
				this.isTempMove = true;
				this.obstacleCheckTickDelay = 12;
				this.ResetStuckCheck();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CheckBlockedUp(Vector3 pos)
	{
		this.IsBlocked = false;
		Vector3 headPosition = this.entity.getHeadPosition();
		headPosition.x = pos.x;
		headPosition.z = pos.z;
		headPosition.y -= 0.625f;
		Ray ray = new Ray(headPosition, Vector3.up);
		if (Voxel.Raycast(this.entity.world, ray, 1f, 1082195968, 128, 0.125f))
		{
			if (Voxel.voxelRayHitInfo.hit.blockValue.Block is BlockDamage)
			{
				return;
			}
			this.HitInfo = Voxel.voxelRayHitInfo.Clone();
			this.IsBlocked = true;
			float sqrMagnitude = (pos - this.HitInfo.hit.pos).sqrMagnitude;
			if (sqrMagnitude < this.blockedDistSq)
			{
				this.blockedDistSq = sqrMagnitude;
				this.obstacleCheckTickDelay = 12;
				this.ResetStuckCheck();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CheckAreaBlocked()
	{
		Vector3 headPosition = this.entity.getHeadPosition();
		headPosition.y = this.entity.position.y;
		Vector3 vector = this.moveToPos - headPosition;
		float f = Mathf.Atan2(vector.x, vector.z);
		float num = Mathf.Sin(f);
		float num2 = Mathf.Cos(f);
		vector.Normalize();
		Vector3 vector2 = headPosition + vector * 0.575f;
		for (float num3 = this.ccHeight - 0.125f; num3 > 0.225f; num3 -= 0.25f)
		{
			for (int i = 0; i < 3; i++)
			{
				float num4 = EntityMoveHelper.checkEdgeXs[i];
				float num5 = num4 * num2;
				float num6 = num4 * -num;
				Vector3 pos = headPosition;
				pos.x += num5;
				pos.y += num3;
				pos.z += num6;
				Vector3 endPos = vector2;
				endPos.x += num5;
				endPos.y += num3;
				endPos.z += num6;
				this.CheckBlocked(pos, endPos, 1);
				if (this.IsBlocked)
				{
					return;
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float CalcObstacleSideStep()
	{
		Vector3 headPosition = this.entity.getHeadPosition();
		headPosition.y = this.entity.position.y;
		Vector3 vector = this.moveToPos - headPosition;
		if (vector.y >= 0.6f)
		{
			return 0f;
		}
		float num = Mathf.Sqrt(vector.x * vector.x + vector.z * vector.z);
		if (num <= this.ccRadius + 0.05f)
		{
			return 0f;
		}
		Vector2 vector2 = new Vector2(vector.x / num, vector.z / num);
		headPosition.x -= vector2.x * 0.2f;
		headPosition.z -= vector2.y * 0.2f;
		float angleRad = Mathf.Atan2(vector2.x, vector2.y);
		if (this.CalcObstacleSideStepArc(headPosition, angleRad, 8f, 20f, 10f) == 0f && this.CalcObstacleSideStepArc(headPosition, angleRad, -8f, -20f, -10f) == 0f)
		{
			return 0f;
		}
		float num2 = this.CalcObstacleSideStepArc(headPosition, angleRad, -48f, -20f, 11f);
		float num3 = this.CalcObstacleSideStepArc(headPosition, angleRad, 48f, 20f, -11f);
		if (Utils.FastAbs(num2) < num3)
		{
			if (num2 <= -48f)
			{
				return 0f;
			}
			if (num2 == 0f)
			{
				num2 = -20f;
			}
			return num2 - 50f;
		}
		else
		{
			if (num3 >= 48f)
			{
				return 0f;
			}
			if (num3 == 0f)
			{
				num3 = 20f;
			}
			return num3 + 50f;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float CalcObstacleSideStepArc(Vector3 startPos, float angleRad, float dirMin, float dirMax, float dirStep)
	{
		float num = this.ccRadius + 0.45f;
		Vector3 a = startPos;
		Vector3 direction;
		direction.y = 0f;
		float num2 = dirMin;
		int num3 = (int)Utils.FastAbs((dirMax - dirMin) / dirStep) + 1;
		for (int i = 0; i < num3; i++)
		{
			float num4 = num2 * 0.0174532924f;
			float f = angleRad + num4;
			direction.x = Mathf.Sin(f);
			direction.z = Mathf.Cos(f);
			float maxDistance = num / Mathf.Cos(num4);
			for (float num5 = this.ccHeight - 0.1f; num5 > 0.3f; num5 -= 0.9f)
			{
				a.y = startPos.y + num5;
				RaycastHit raycastHit;
				if (Physics.SphereCast(a - Origin.position, 0.1f, direction, out raycastHit, maxDistance, 1082720256))
				{
					return num2;
				}
			}
			num2 += dirStep;
		}
		return 0f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CheckEntityBlocked(Vector3 pos, Vector3 endPos)
	{
		Vector3 direction = endPos - pos;
		pos.y += 0.7f;
		RaycastHit raycastHit;
		if (Physics.SphereCast(pos - Origin.position, 0.15f, direction, out raycastHit, 0.8f, 524288))
		{
			Transform transform = raycastHit.transform;
			if (transform)
			{
				Transform transform2 = transform.parent.Find("GameObject");
				if (transform2)
				{
					EntityAlive component = transform2.GetComponent<EntityAlive>();
					if (component && component != this.entity)
					{
						float sqrMagnitude = (this.entity.position - component.position).sqrMagnitude;
						float num = this.ccRadius + component.m_characterController.GetRadius() + 0.16f + 0.25f;
						if (sqrMagnitude < num * num)
						{
							this.BlockedEntity = component;
							this.blockedEntityDistSq = sqrMagnitude;
						}
					}
				}
			}
		}
	}

	public void StartJump(bool calcYaw, float distance = 0f, float heightDiff = 0f)
	{
		if (!this.entity.Jumping && (this.entity.onGround || this.entity.IsInElevator()) && !this.entity.Electrocuted)
		{
			this.JumpToPos = this.moveToPos;
			if (!calcYaw)
			{
				this.jumpYaw = this.entity.rotation.y;
			}
			else
			{
				float y = this.moveToPos.x - this.entity.position.x;
				float x = this.moveToPos.z - this.entity.position.z;
				this.jumpYaw = Mathf.Atan2(y, x) * 57.29578f;
			}
			this.entity.Jumping = true;
			this.entity.SetJumpDistance(distance, heightDiff);
			this.IsBlocked = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void StartSwimStroke()
	{
		if (!this.entity.Jumping)
		{
			this.JumpToPos = this.moveToPos;
			float y = this.moveToPos.x - this.entity.position.x;
			float x = this.moveToPos.z - this.entity.position.z;
			this.jumpYaw = Mathf.Atan2(y, x) * 57.29578f;
			this.entity.Jumping = true;
			this.entity.SetSwimValues((float)this.swimStrokeDelayTicks, this.moveToPos - this.entity.position);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Push(EntityAlive blockerEntity)
	{
		Vector3 normalized = (blockerEntity.position - this.entity.position).normalized;
		this.damageResponse.Source = new DamageSource(EnumDamageSource.External, EnumDamageTypes.Bashing, normalized);
		float massKg = EntityClass.list[this.entity.entityClass].MassKg;
		this.damageResponse.StunDuration = 0f;
		this.damageResponse.Strength = (int)(massKg * 0.05f);
		blockerEntity.DoRagdoll(this.damageResponse);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AttackPush(EntityAlive blockerEntity)
	{
		Vector3 normalized = (blockerEntity.position - this.entity.position).normalized;
		this.damageResponse.Source = new DamageSource(EnumDamageSource.External, EnumDamageTypes.Bashing, normalized);
		ItemActionAttackData itemActionAttackData = this.entity.inventory.holdingItemData.actionData[0] as ItemActionAttackData;
		if (itemActionAttackData != null)
		{
			itemActionAttackData.hitDelegate = new ItemActionAttackData.HitDelegate(this.GetAttackHitInfo);
			if (this.entity.Attack(false))
			{
				this.entity.Attack(true);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public WorldRayHitInfo GetAttackHitInfo(out float damageMpy)
	{
		if (this.BlockedEntity)
		{
			float massKg = EntityClass.list[this.entity.entityClass].MassKg;
			if (this.random.RandomFloat < 0.3f)
			{
				this.damageResponse.StunDuration = 0.5f;
				this.damageResponse.Strength = (int)(massKg * 0.4f);
			}
			else
			{
				this.damageResponse.StunDuration = 0f;
				this.damageResponse.Strength = (int)(massKg * 0.2f);
			}
			this.BlockedEntity.DoRagdoll(this.damageResponse);
		}
		damageMpy = 0f;
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DigStart(int forTicks)
	{
		this.digStartPos = this.entity.position;
		if (this.isDigging)
		{
			this.digForTicks = Utils.FastMax(this.digForTicks, (float)forTicks);
			return;
		}
		if (!this.CanBreakBlocks)
		{
			return;
		}
		this.digForTicks = (float)forTicks;
		this.digTicks = 0f;
		this.digActionTicks = 18f;
		this.digAttacked = false;
		this.digForwardCount = 0f;
		AvatarController avatarController = this.entity.emodel.avatarController;
		avatarController.CancelEvent("EndTrigger");
		avatarController.TriggerEvent("DigStartTrigger");
		this.isDigging = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DigUpdate()
	{
		float num = this.digForTicks - 1f;
		this.digForTicks = num;
		if (num <= 0f)
		{
			this.DigStop();
			return;
		}
		this.entity.SetMoveForward(0f);
		if (this.entity.world.IsDark())
		{
			this.expiryTicks = 5;
		}
		this.digTicks += 1f;
		if (this.digTicks < this.digActionTicks)
		{
			return;
		}
		if (!this.entity.emodel.avatarController.IsAnimationDigRunning())
		{
			this.isDigging = false;
			return;
		}
		if ((this.entity.position - this.digStartPos).sqrMagnitude >= 0.25f)
		{
			this.DigStop();
			return;
		}
		if (!this.digAttacked)
		{
			this.entity.emodel.avatarController.TriggerEvent("DigTrigger");
			this.digTicks = 0f;
			this.digActionTicks = 4f;
			this.digAttacked = true;
			return;
		}
		this.digActionTicks = 14f;
		this.digAttacked = false;
		Vector3 position = this.entity.position;
		position.y += 0.6f;
		Vector3 direction;
		float distance;
		if (this.digForwardCount > 0f)
		{
			this.digForwardCount -= 1f;
			direction = this.entity.GetForwardVector();
			distance = 1.1f;
			this.entity.SeekYaw(this.entity.rotation.y + (this.random.RandomFloat * 2f - 1f) * 120f, 0f, 120f);
		}
		else
		{
			position.x += (this.random.RandomFloat - 0.5f) * 0.3f;
			position.z += (this.random.RandomFloat - 0.5f) * 0.3f;
			direction = this.moveToPos - position;
			distance = 1.4000001f;
		}
		Ray ray = new Ray(position, direction);
		if (Voxel.Raycast(this.entity.world, ray, distance, 1082195968, 128, 0.15f))
		{
			WorldRayHitInfo voxelRayHitInfo = Voxel.voxelRayHitInfo;
			DamageMultiplier damageMultiplier = new DamageMultiplier();
			List<string> buffActions = null;
			ItemActionAttack.AttackHitInfo attackHitInfo = new ItemActionAttack.AttackHitInfo();
			attackHitInfo.hardnessScale = 1f;
			float num2 = 1f;
			ItemActionAttack itemActionAttack = this.entity.inventory.holdingItem.Actions[0] as ItemActionAttack;
			if (itemActionAttack != null)
			{
				num2 = itemActionAttack.GetDamageBlock(this.entity.inventory.holdingItemData.actionData[0].invData.itemValue, BlockValue.Air, null, 0);
			}
			ItemActionAttack.Hit(voxelRayHitInfo, this.entity.entityId, EnumDamageTypes.Bashing, num2, num2, 1f, 1f, 0f, 0.05f, "organic", damageMultiplier, buffActions, attackHitInfo, 1, 0, 0f, null, null, ItemActionAttack.EnumAttackMode.RealNoHarvesting, null, -1, null);
			return;
		}
		if (this.digForwardCount == 0f)
		{
			this.digForwardCount = 2f;
			return;
		}
		this.digForwardCount = 0f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DigStop()
	{
		if (this.isDigging)
		{
			this.isDigging = false;
			this.entity.emodel.avatarController.TriggerEvent("EndTrigger");
		}
	}

	public float CalcBlockedDistanceSq()
	{
		Vector3 pos = this.HitInfo.hit.pos;
		Vector3 position = this.entity.position;
		float num = pos.x - position.x;
		float num2 = pos.z - position.z;
		return num * num + num2 * num2;
	}

	public void ClearBlocked()
	{
		this.IsBlocked = false;
		this.BlockedTime = 0f;
	}

	public void ClearTempMove()
	{
		this.isTempMove = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ResetStuckCheck()
	{
		this.SideStepAngle = 0f;
		this.moveToTicks = 0;
		this.moveToFailCnt = 0;
		if (this.isTempMove)
		{
			this.moveToDistance = this.CalcTempMoveDist();
			return;
		}
		this.moveToDistance = this.CalcMoveDist();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float CalcMoveDist()
	{
		Vector3 position = this.entity.position;
		float num = this.moveToPos.x - position.x;
		float num2 = this.moveToPos.z - position.z;
		float num3 = this.moveToPos.y - position.y;
		return Mathf.Sqrt(num * num + num3 * num3 + num2 * num2);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float CalcTempMoveDist()
	{
		Vector3 position = this.entity.position;
		float num = this.tempMoveToPos.x - position.x;
		float num2 = this.tempMoveToPos.z - position.z;
		float num3 = this.tempMoveToPos.y - position.y;
		return Mathf.Sqrt(num * num + num3 * num3 + num2 * num2);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 CalcBlockCenterXZ(Vector3 pos)
	{
		pos.x = (float)Utils.Fastfloor(pos.x) + 0.5f;
		pos.z = (float)Utils.Fastfloor(pos.z) + 0.5f;
		return pos;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 CalcBlockCenter(Vector3 pos)
	{
		pos.x = (float)Utils.Fastfloor(pos.x) + 0.5f;
		pos.y = (float)Utils.Fastfloor(pos.y) + 0.5f;
		pos.z = (float)Utils.Fastfloor(pos.z) + 0.5f;
		return pos;
	}

	public void CalcIfUnreachablePos()
	{
		this.IsUnreachableSideJump = false;
		if (this.entity.Jumping)
		{
			return;
		}
		this.IsUnreachableAbove = false;
		this.IsUnreachableSide = false;
		PathEntity path = this.entity.navigator.getPath();
		if (path != null)
		{
			Vector3 toPos = path.toPos;
			Vector3 rawEndPos = path.rawEndPos;
			float num = rawEndPos.x - toPos.x;
			float num2 = rawEndPos.z - toPos.z;
			float num3 = num * num + num2 * num2;
			float num4 = toPos.y - rawEndPos.y;
			if (num4 > 2.2f && num3 < 25f)
			{
				this.IsUnreachableAbove = true;
				this.UnreachablePos = rawEndPos;
			}
			if (num4 >= -1.5f && num3 >= 1.44f)
			{
				this.IsUnreachableSide = true;
				this.UnreachablePos = rawEndPos;
				float num5 = this.entity.jumpMaxDistance;
				if (num5 > 0f && num4 < 0.5f + num5 * 0.5f)
				{
					num5 += 3.4f;
					if (num3 <= num5 * num5)
					{
						this.IsUnreachableSideJump = true;
					}
				}
			}
		}
	}

	public bool IsMoveToAbove()
	{
		return this.moveToPos.y - this.entity.position.y > 1.9f;
	}

	public bool FindDestroyPos(ref Vector3 destroyPos, bool isLookFar)
	{
		int num = int.MaxValue;
		Vector3i vector3i = Vector3i.zero;
		ChunkCluster chunkCache = this.entity.world.ChunkCache;
		Vector3i vector3i2 = World.worldToBlockPos(destroyPos);
		int num2 = 1;
		int num3 = 1;
		if (isLookFar)
		{
			num2 = this.random.RandomRange(5, 11);
			num3 = -1;
			vector3i2.y -= 2;
		}
		bool flag = false;
		int num4 = this.random.RandomRange(0, 4);
		while (num2 >= 1 && num2 <= 11)
		{
			int num5 = num2 * 2;
			for (int i = -2; i <= 2; i++)
			{
				Vector3i vector3i3;
				vector3i3.y = vector3i2.y + i;
				for (int j = 0; j < 4; j++)
				{
					EntityMoveHelper.DestroyData destroyData = EntityMoveHelper.destroyData[j + num4];
					int num6 = destroyData.offsetX * num2;
					int num7 = destroyData.offsetZ * num2;
					vector3i3.x = vector3i2.x + num6;
					vector3i3.z = vector3i2.z + num7;
					for (int k = 0; k < num5; k++)
					{
						BlockValue block = chunkCache.GetBlock(vector3i3);
						if (!block.isair)
						{
							Block block2 = block.Block;
							if (block2.IsMovementBlocked(this.entity.world, vector3i3, block, BlockFace.None) && block2.StabilitySupport)
							{
								Vector3i vector3i4 = vector3i3;
								vector3i4.y++;
								BlockValue block3 = chunkCache.GetBlock(vector3i4);
								if (!block3.isair)
								{
									Block block4 = block3.Block;
									if (block4.IsMovementBlocked(this.entity.world, vector3i4, block3, BlockFace.None) && block4.StabilitySupport)
									{
										bool flag2 = false;
										int num8 = block2.MaxDamagePlusDowngrades - block.damage;
										if (block2.shape.IsTerrain())
										{
											num8 *= 50;
										}
										else
										{
											flag2 = true;
										}
										int num9 = block4.MaxDamagePlusDowngrades - block3.damage;
										if (block4.shape.IsTerrain())
										{
											num9 *= 50;
										}
										else
										{
											flag2 = true;
										}
										num8 += num9;
										if (num8 < num && (!flag || flag2) && this.IsABlockSideOpen(vector3i3))
										{
											flag = flag2;
											num = num8;
											vector3i = vector3i3;
										}
									}
								}
							}
						}
						vector3i3.x += destroyData.stepX;
						vector3i3.z += destroyData.stepZ;
					}
				}
			}
			if (flag)
			{
				break;
			}
			num2 += num3;
		}
		if (num > 999999)
		{
			return false;
		}
		destroyPos = vector3i.ToVector3CenterXZ();
		destroyPos.y += 1f;
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsABlockSideOpen(Vector3i checkPos)
	{
		ChunkCluster chunkCache = this.entity.world.ChunkCache;
		for (int i = 0; i < EntityMoveHelper.blockOpenOffsets.Length; i += 2)
		{
			Vector3i vector3i = checkPos;
			vector3i.x += EntityMoveHelper.blockOpenOffsets[i];
			vector3i.z += EntityMoveHelper.blockOpenOffsets[i + 1];
			BlockValue block = chunkCache.GetBlock(vector3i);
			if (!block.Block.IsMovementBlocked(this.entity.world, vector3i, block, BlockFace.None))
			{
				return true;
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cDoneXZDistSq = 0.0004f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cCheckBlockedDist = 0.35f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cCheckBlockedRadius = 0.125f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cCheckSidestepDist = 0.35f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cCheckSidestepRadius = 0.1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cTempMoveDist = 0.4f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cYawNextDist = 1.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cMoveDirectDist = 0.65f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cMoveSlowDist = 0.6f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cDigXZDistSq = 0.0100000007f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cDigDiagonalXZDistSq = 2.25f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cDigAngleCos = 0.86f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cJumpUpXZDistSq = 0.0400000028f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cLadderXZDistSq = 0.108900011f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cDestroyRadius = 11;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cUnreachJumpMin = 1.2f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cCollisionMask = 1082195968;

	public Vector3 JumpToPos;

	public bool IsActive;

	public bool CanBreakBlocks;

	public bool IsBlocked;

	public float BlockedTime;

	public EntityAlive BlockedEntity;

	public WorldRayHitInfo HitInfo;

	public float DamageScale;

	public bool IsUnreachableAbove;

	public bool IsUnreachableSide;

	public bool IsUnreachableSideJump;

	public Vector3 UnreachablePos;

	public float SideStepAngle;

	public float UnreachablePercent;

	public bool IsDestroyAreaTryUnreachable;

	public bool IsDestroyArea;

	[PublicizedFrom(EAccessModifier.Private)]
	public DamageResponse damageResponse = DamageResponse.New(false);

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityAlive entity;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameRandom random;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 moveToPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public float moveToDistance;

	[PublicizedFrom(EAccessModifier.Private)]
	public int moveToTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	public int moveToFailCnt;

	[PublicizedFrom(EAccessModifier.Private)]
	public float moveToDir;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 focusPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public int focusTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	public int obstacleCheckTickDelay;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasNextPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 nextMoveToPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 tempMoveToPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isTempMove;

	[PublicizedFrom(EAccessModifier.Private)]
	public float blockedHeight;

	[PublicizedFrom(EAccessModifier.Private)]
	public float blockedDistSq;

	[PublicizedFrom(EAccessModifier.Private)]
	public float blockedEntityDistSq;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDigging;

	[PublicizedFrom(EAccessModifier.Private)]
	public float moveSpeed;

	[PublicizedFrom(EAccessModifier.Private)]
	public int expiryTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isClimb;

	[PublicizedFrom(EAccessModifier.Private)]
	public float jumpYaw;

	[PublicizedFrom(EAccessModifier.Private)]
	public int swimStrokeDelayTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	public float ccRadius;

	[PublicizedFrom(EAccessModifier.Private)]
	public float ccHeight;

	[PublicizedFrom(EAccessModifier.Private)]
	public static float[] checkEdgeXs = new float[]
	{
		0f,
		-0.25f,
		0.25f
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cDigMovedDist = 0.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 digStartPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public float digForTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	public float digTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	public float digActionTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool digAttacked;

	[PublicizedFrom(EAccessModifier.Private)]
	public float digForwardCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public static EntityMoveHelper.DestroyData[] destroyData = new EntityMoveHelper.DestroyData[]
	{
		new EntityMoveHelper.DestroyData(-1, 1, 1, 0),
		new EntityMoveHelper.DestroyData(1, 1, 0, -1),
		new EntityMoveHelper.DestroyData(1, -1, -1, 0),
		new EntityMoveHelper.DestroyData(-1, -1, 0, 1),
		new EntityMoveHelper.DestroyData(-1, 1, 1, 0),
		new EntityMoveHelper.DestroyData(1, 1, 0, -1),
		new EntityMoveHelper.DestroyData(1, -1, -1, 0)
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static int[] blockOpenOffsets = new int[]
	{
		-1,
		0,
		1,
		0,
		0,
		1,
		0,
		-1
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public struct DestroyData
	{
		public DestroyData(int _offsetX, int _offsetZ, int _stepX, int _stepZ)
		{
			this.offsetX = _offsetX;
			this.offsetZ = _offsetZ;
			this.stepX = _stepX;
			this.stepZ = _stepZ;
		}

		public int offsetX;

		public int offsetZ;

		public int stepX;

		public int stepZ;
	}
}

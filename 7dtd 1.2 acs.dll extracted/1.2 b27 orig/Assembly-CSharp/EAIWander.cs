using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EAIWander : EAIBase
{
	public override void Init(EntityAlive _theEntity)
	{
		base.Init(_theEntity);
		this.MutexBits = 1;
	}

	public override bool CanExecute()
	{
		if (this.theEntity.sleepingOrWakingUp)
		{
			return false;
		}
		if (this.theEntity.GetTicksNoPlayerAdjacent() >= 120)
		{
			return false;
		}
		if (this.theEntity.bodyDamage.CurrentStun != EnumEntityStunType.None)
		{
			return false;
		}
		int num = (int)(200f * this.executeWaitTime);
		if (base.GetRandom(1000) >= num)
		{
			return false;
		}
		if (this.manager.lookTime > 0f)
		{
			return false;
		}
		int num2 = (int)this.manager.interestDistance;
		Vector3 vector;
		if (this.theEntity.IsAlert)
		{
			num2 *= 2;
			vector = RandomPositionGenerator.CalcAway(this.theEntity, 0, num2, num2, this.theEntity.LastTargetPos);
		}
		else
		{
			vector = RandomPositionGenerator.Calc(this.theEntity, num2, num2);
		}
		if (vector.Equals(Vector3.zero))
		{
			return false;
		}
		this.position = vector;
		return true;
	}

	public override void Start()
	{
		this.time = 0f;
		this.theEntity.FindPath(this.position, this.theEntity.GetMoveSpeed(), false, this);
	}

	public override bool Continue()
	{
		return this.theEntity.bodyDamage.CurrentStun == EnumEntityStunType.None && this.theEntity.moveHelper.BlockedTime <= 0.3f && this.time <= 30f && !this.theEntity.navigator.noPathAndNotPlanningOne();
	}

	public override void Update()
	{
		this.time += 0.05f;
	}

	public override void Reset()
	{
		this.manager.lookTime = base.RandomFloat * 3f;
		this.theEntity.moveHelper.Stop();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cLookTimeMax = 3f;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 position;

	[PublicizedFrom(EAccessModifier.Private)]
	public float time;
}

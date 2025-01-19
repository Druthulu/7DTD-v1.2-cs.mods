using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EAIRangedAttackTarget : EAIBase
{
	public override void Init(EntityAlive _theEntity)
	{
		base.Init(_theEntity);
		this.MutexBits = 11;
		this.cooldown = 3f;
		this.attackDuration = 20f;
	}

	public override void SetData(DictionarySave<string, string> data)
	{
		base.SetData(data);
		base.GetData(data, "itemType", ref this.itemActionType);
		base.GetData(data, "cooldown", ref this.baseCooldown);
		base.GetData(data, "duration", ref this.attackDuration);
		base.GetData(data, "minRange", ref this.minRange);
		base.GetData(data, "maxRange", ref this.maxRange);
		base.GetData(data, "unreachableRange", ref this.unreachableRange);
	}

	public override bool CanExecute()
	{
		if (this.theEntity.IsDancing)
		{
			return false;
		}
		if (this.cooldown > 0f)
		{
			this.cooldown -= this.executeWaitTime;
			return false;
		}
		if (!this.theEntity.IsAttackValid())
		{
			return false;
		}
		this.entityTarget = this.theEntity.GetAttackTarget();
		return !(this.entityTarget == null) && this.InRange() && this.theEntity.CanSee(this.entityTarget);
	}

	public override void Start()
	{
		this.attackTime = 0f;
	}

	public override bool Continue()
	{
		return this.entityTarget && this.entityTarget.IsAlive() && this.attackTime < this.attackDuration && this.theEntity.hasBeenAttackedTime <= 0;
	}

	public override void Update()
	{
		this.attackTime += 0.05f;
		if (this.attackTime < this.attackDuration * 0.5f)
		{
			Vector3 headPosition = this.entityTarget.getHeadPosition();
			if (this.theEntity.IsInFrontOfMe(headPosition))
			{
				this.theEntity.SetLookPosition(headPosition);
			}
		}
		this.Attack(false);
		ItemActionVomit.ItemActionDataVomit itemActionDataVomit = this.theEntity.inventory.holdingItemData.actionData[this.itemActionType] as ItemActionVomit.ItemActionDataVomit;
		if (itemActionDataVomit != null && itemActionDataVomit.isDone)
		{
			this.attackTime = this.attackDuration;
		}
	}

	public override void Reset()
	{
		this.Attack(true);
		this.theEntity.SetLookPosition(Vector3.zero);
		this.entityTarget = null;
		this.cooldown = this.baseCooldown + this.baseCooldown * 0.5f * base.RandomFloat;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Attack(bool isAttackReleased)
	{
		if (this.itemActionType == 0)
		{
			this.theEntity.Attack(isAttackReleased);
			return;
		}
		this.theEntity.Use(isAttackReleased);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool InRange()
	{
		float distanceSq = this.entityTarget.GetDistanceSq(this.theEntity);
		if (this.unreachableRange > 0f)
		{
			EntityMoveHelper moveHelper = this.theEntity.moveHelper;
			if (moveHelper.IsUnreachableAbove || moveHelper.IsUnreachableSide)
			{
				return distanceSq <= this.unreachableRange * this.unreachableRange;
			}
		}
		return distanceSq >= this.minRange * this.minRange && distanceSq <= this.maxRange * this.maxRange;
	}

	public override string ToString()
	{
		bool flag = this.entityTarget && this.InRange();
		return string.Format("{0} {1}, inRange{2}, Time {3}", new object[]
		{
			base.ToString(),
			this.entityTarget ? this.entityTarget.EntityName : "",
			flag,
			this.attackTime.ToCultureInvariantString("0.00")
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int itemActionType;

	[PublicizedFrom(EAccessModifier.Private)]
	public float baseCooldown;

	[PublicizedFrom(EAccessModifier.Private)]
	public float cooldown;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityAlive entityTarget;

	[PublicizedFrom(EAccessModifier.Private)]
	public float attackTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public float attackDuration;

	[PublicizedFrom(EAccessModifier.Private)]
	public float minRange = 4f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float maxRange = 25f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float unreachableRange;
}

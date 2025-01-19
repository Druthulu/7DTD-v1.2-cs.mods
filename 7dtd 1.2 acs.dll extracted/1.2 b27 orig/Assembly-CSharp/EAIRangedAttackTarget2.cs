using System;
using UnityEngine.Scripting;

[Preserve]
public class EAIRangedAttackTarget2 : EAIBase
{
	public EAIRangedAttackTarget2()
	{
		this.attackTimeout = 10;
		this.MutexBits = 3;
		this.attackPeriodMax = 20;
	}

	public override void SetData(DictionarySave<string, string> data)
	{
		base.SetData(data);
		base.GetData(data, "itemType", ref this.itemActionType);
		base.GetData(data, "attackPeriod", ref this.attackPeriodMax);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool inRange(float _distanceSq)
	{
		float value = EffectManager.GetValue(PassiveEffects.DamageFalloffRange, this.theEntity.inventory.holdingItemItemValue, 0f, this.theEntity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
		return _distanceSq < value * value * 0.25f;
	}

	public override bool CanExecute()
	{
		if (this.theEntity.inventory.holdingItem.Actions == null)
		{
			return false;
		}
		ItemActionRanged itemActionRanged = this.theEntity.inventory.holdingItem.Actions[0] as ItemActionRanged;
		if (itemActionRanged == null)
		{
			return false;
		}
		if (this.theEntity.inventory.holdingItemItemValue.Meta <= 0)
		{
			if (itemActionRanged.CanReload(this.theEntity.inventory.holdingItemData.actionData[0]))
			{
				itemActionRanged.ReloadGun(this.theEntity.inventory.holdingItemData.actionData[0]);
			}
			return false;
		}
		if (this.attackTimeout > 0)
		{
			this.attackTimeout--;
			return false;
		}
		if (!this.theEntity.Spawned || !this.theEntity.IsAttackValid())
		{
			return false;
		}
		this.entityTarget = this.theEntity.GetAttackTarget();
		if (this.entityTarget == null)
		{
			return false;
		}
		float distanceSq = this.entityTarget.GetDistanceSq(this.theEntity);
		if (!this.inRange(distanceSq))
		{
			return false;
		}
		this.bCanSee = this.theEntity.CanSee(this.entityTarget);
		return this.bCanSee;
	}

	public override bool Continue()
	{
		return this.curAttackPeriod > 0 && this.theEntity.hasBeenAttackedTime <= 0;
	}

	public override void Start()
	{
		float delay = this.theEntity.inventory.holdingItem.Actions[0].Delay;
		this.attackTimeout = (int)(delay * 20f);
		this.curAttackPeriod = this.attackPeriodMax;
	}

	public override void Update()
	{
		this.curAttackPeriod--;
		if ((float)this.curAttackPeriod > (float)this.attackPeriodMax * 0.5f && this.theEntity.IsInFrontOfMe(this.entityTarget.getHeadPosition()))
		{
			this.theEntity.SetLookPosition(this.entityTarget.getBellyPosition());
		}
		if (this.inRange(this.entityTarget.GetDistanceSq(this.theEntity)) && this.theEntity.IsInFrontOfMe(this.entityTarget.getHeadPosition()))
		{
			if (this.itemActionType == 0)
			{
				this.theEntity.Attack((float)this.curAttackPeriod < (float)this.attackPeriodMax / 2f);
				return;
			}
			this.theEntity.Use((float)this.curAttackPeriod < (float)this.attackPeriodMax / 2f);
		}
	}

	public override void Reset()
	{
		this.entityTarget = null;
		this.curAttackPeriod = 0;
		float delay = this.theEntity.inventory.holdingItem.Actions[0].Delay;
		this.attackTimeout = (int)(delay * 20f);
		this.attackTimeout = 5 + base.GetRandom(5);
		if (this.itemActionType == 0)
		{
			this.theEntity.Attack(true);
			return;
		}
		this.theEntity.Use(true);
	}

	public override string ToString()
	{
		bool flag = this.entityTarget != null && this.inRange(this.entityTarget.GetDistanceSq(this.theEntity));
		return string.Concat(new string[]
		{
			base.ToString(),
			": ",
			(this.entityTarget != null) ? this.entityTarget.EntityName : "null",
			" see: ",
			this.bCanSee ? "Y" : "N",
			" range=",
			flag ? "Y" : "N"
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityAlive entityTarget;

	[PublicizedFrom(EAccessModifier.Private)]
	public int attackTimeout;

	[PublicizedFrom(EAccessModifier.Private)]
	public int itemActionType;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bCanSee;

	[PublicizedFrom(EAccessModifier.Private)]
	public int curAttackPeriod;

	[PublicizedFrom(EAccessModifier.Private)]
	public int attackPeriodMax;
}

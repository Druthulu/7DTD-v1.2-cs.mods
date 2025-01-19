using System;
using UnityEngine.Scripting;

[Preserve]
public class EAIConsiderCover : EAIBase
{
	public EAIConsiderCover()
	{
		this.MutexBits = 1;
		this.ecm = EntityCoverManager.Instance;
	}

	public override bool CanExecute()
	{
		if (this.theEntity.sleepingOrWakingUp || this.theEntity.bodyDamage.CurrentStun != EnumEntityStunType.None || (this.theEntity.Jumping && !this.theEntity.isSwimming))
		{
			return false;
		}
		this.entityTarget = this.theEntity.GetAttackTarget();
		return !(this.entityTarget == null) && !this.ecm.HasCover(this.theEntity.entityId);
	}

	public override bool Continue()
	{
		return base.Continue();
	}

	public override void Update()
	{
		this.entityTarget == null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityAlive entityTarget;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityCoverManager ecm;
}

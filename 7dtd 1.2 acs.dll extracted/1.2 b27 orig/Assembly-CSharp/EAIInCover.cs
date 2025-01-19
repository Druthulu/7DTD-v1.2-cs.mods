using System;
using GamePath;
using UnityEngine.Scripting;

[Preserve]
public class EAIInCover : EAIBase
{
	public EAIInCover()
	{
		this.MutexBits = 1;
		this.ecm = EntityCoverManager.Instance;
	}

	public override void Start()
	{
		this.coverTicks = 60f;
		PathFinderThread.Instance.RemovePathsFor(this.theEntity.entityId);
	}

	public override bool CanExecute()
	{
		return !this.theEntity.sleepingOrWakingUp && this.theEntity.bodyDamage.CurrentStun == EnumEntityStunType.None && (!this.theEntity.Jumping || this.theEntity.isSwimming) && this.ecm.HasCover(this.theEntity.entityId);
	}

	public override bool Continue()
	{
		return this.ecm.HasCover(this.theEntity.entityId);
	}

	public override void Update()
	{
		if (!this.ecm.HasCover(this.theEntity.entityId))
		{
			return;
		}
		if (this.ecm.GetCoverPos(this.theEntity.entityId) == null)
		{
			return;
		}
		if (this.coverTicks > 0f)
		{
			this.coverTicks -= 1f;
			if (this.coverTicks <= 0f)
			{
				if (base.Random.RandomRange(2) < 1)
				{
					this.freeCover();
					return;
				}
				this.coverTicks = 60f;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void freeCover()
	{
		this.ecm.FreeCover(this.theEntity.entityId);
		this.coverTicks = 60f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityAlive entityTarget;

	[PublicizedFrom(EAccessModifier.Private)]
	public float coverTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityCoverManager ecm;
}

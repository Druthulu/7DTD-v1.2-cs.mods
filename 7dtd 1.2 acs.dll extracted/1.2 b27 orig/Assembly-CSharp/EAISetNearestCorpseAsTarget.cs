using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class EAISetNearestCorpseAsTarget : EAITarget
{
	public override void Init(EntityAlive _theEntity)
	{
		base.Init(_theEntity, 15f, true);
		this.executeDelay = 0.8f;
		this.rndTimeout = 0;
		this.MutexBits = 1;
		this.sorter = new EAISetNearestEntityAsTargetSorter(_theEntity);
	}

	public override void SetData(DictionarySave<string, string> data)
	{
		base.SetData(data);
		string names;
		if (data.TryGetValue("flags", out names))
		{
			EntityClass.ParseEntityFlags(names, ref this.targetFlags);
		}
		base.GetData(data, "maxDistance2d", ref this.maxXZDistance);
	}

	public override bool CanExecute()
	{
		if (this.theEntity.HasInvestigatePosition)
		{
			return false;
		}
		if (this.theEntity.IsSleeping)
		{
			return false;
		}
		if (this.rndTimeout > 0 && base.GetRandom(this.rndTimeout) != 0)
		{
			return false;
		}
		EntityAlive attackTarget = this.theEntity.GetAttackTarget();
		if (attackTarget is EntityPlayer && attackTarget.IsAlive() && base.RandomFloat < 0.95f)
		{
			return false;
		}
		float radius = this.theEntity.IsSleeper ? 7f : this.maxXZDistance;
		this.theEntity.world.GetEntitiesAround(this.targetFlags, this.targetFlags, this.theEntity.position, radius, EAISetNearestCorpseAsTarget.entityList);
		EAISetNearestCorpseAsTarget.entityList.Sort(this.sorter);
		EntityAlive entityAlive = null;
		for (int i = 0; i < EAISetNearestCorpseAsTarget.entityList.Count; i++)
		{
			EntityAlive entityAlive2 = EAISetNearestCorpseAsTarget.entityList[i] as EntityAlive;
			if (entityAlive2 && entityAlive2.IsDead())
			{
				entityAlive = entityAlive2;
				break;
			}
		}
		EAISetNearestCorpseAsTarget.entityList.Clear();
		this.targetEntity = entityAlive;
		return this.targetEntity != null;
	}

	public override void Start()
	{
		base.Start();
		this.theEntity.SetAttackTarget(this.targetEntity, 600);
	}

	public override bool Continue()
	{
		return this.targetEntity && this.targetEntity.IsDead() && !(this.targetEntity != this.theEntity.GetAttackTarget());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityAlive targetEntity;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityFlags targetFlags;

	[PublicizedFrom(EAccessModifier.Private)]
	public int rndTimeout;

	[PublicizedFrom(EAccessModifier.Private)]
	public EAISetNearestEntityAsTargetSorter sorter;

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<Entity> entityList = new List<Entity>();
}

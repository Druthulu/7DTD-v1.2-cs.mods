using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class EAISetNearestEntityAsTargetSorter : IComparer<Entity>
{
	public EAISetNearestEntityAsTargetSorter(Entity _entity)
	{
		this.theEntity = _entity;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int isNearer(Entity _e, Entity _other)
	{
		float distanceSq = this.theEntity.GetDistanceSq(_e);
		float distanceSq2 = this.theEntity.GetDistanceSq(_other);
		if (distanceSq < distanceSq2)
		{
			return -1;
		}
		if (distanceSq > distanceSq2)
		{
			return 1;
		}
		return 0;
	}

	public int Compare(Entity _obj1, Entity _obj2)
	{
		return this.isNearer(_obj1, _obj2);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Entity theEntity;
}

using System;
using UnityEngine.Scripting;

[Preserve]
public class EntityDriveable : EntityVehicle
{
	public override void Init(int _entityClass)
	{
		base.Init(_entityClass);
		if (this.nativeCollider != null)
		{
			this.nativeCollider.enabled = true;
		}
	}
}

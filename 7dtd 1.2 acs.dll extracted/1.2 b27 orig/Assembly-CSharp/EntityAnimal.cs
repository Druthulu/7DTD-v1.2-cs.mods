using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public abstract class EntityAnimal : EntityAlive
{
	public override void OnUpdateLive()
	{
		base.GetEntitySenses().Clear();
		base.OnUpdateLive();
	}

	public override bool IsDrawMapIcon()
	{
		return false;
	}

	public override Color GetMapIconColor()
	{
		return new Color(1f, 0.8235294f, 0.34117648f);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void playStepSound(string stepSound)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isGameMessageOnDeath()
	{
		return false;
	}

	public override int DamageEntity(DamageSource _damageSource, int _strength, bool _criticalHit, float impulseScale)
	{
		return base.DamageEntity(_damageSource, _strength, _criticalHit, impulseScale);
	}

	public override void OnEntityDeath()
	{
		if (this.PhysicsTransform)
		{
			this.PhysicsTransform.gameObject.SetActive(false);
		}
		base.OnEntityDeath();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public EntityAnimal()
	{
	}
}

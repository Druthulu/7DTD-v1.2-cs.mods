using System;
using Audio;
using UnityEngine;

public class AnimationEventBridge : RootTransformRefEntity
{
	public EntityAlive entity
	{
		get
		{
			if (!this._entity && this.RootTransform)
			{
				this._entity = this.RootTransform.GetComponent<EntityAlive>();
			}
			return this._entity;
		}
	}

	public void PlaySound(string name)
	{
		if (this.entity != null)
		{
			EntityPlayer entityPlayer = this.entity as EntityPlayer;
			if (entityPlayer != null && entityPlayer.IsReloadCancelled())
			{
				return;
			}
			this.entity.PlayOneShot(name, false, true, false);
		}
	}

	public void PlayLocalSound(string name)
	{
		this.PlaySound(name);
	}

	public void PlayStepSound()
	{
		if (this.entity != null && (double)(Time.time - this.lastTimeStepPlayed) > 0.1)
		{
			this.entity.PlayStepSound();
			this.lastTimeStepPlayed = Time.time;
		}
	}

	public void DeathImpactLight()
	{
		if (this.entity != null && this.entity.IsDead())
		{
			Manager.Play(this.entity, "impactbodylight", 1f, false);
		}
	}

	public void DeathImpactHeavy()
	{
		if (this.entity != null && this.entity.IsDead())
		{
			Manager.Play(this.entity, "impactbodyheavy", 1f, false);
		}
	}

	public void HideHoldingItem()
	{
		if (this.entity != null && this.entity.emodel != null && this.entity.inventory.models[this.entity.inventory.holdingItemIdx] != null)
		{
			this.entity.inventory.models[this.entity.inventory.holdingItemIdx].gameObject.SetActive(false);
		}
	}

	public void ShowHoldingItem()
	{
		if (this.entity != null && this.entity.emodel != null && this.entity.inventory.models[this.entity.inventory.holdingItemIdx] != null)
		{
			this.entity.inventory.models[this.entity.inventory.holdingItemIdx].gameObject.SetActive(true);
		}
	}

	public void Hit()
	{
		Entity entity = this.entity;
		if (entity && entity.emodel && entity.emodel.avatarController)
		{
			entity.emodel.avatarController.SetAttackImpact();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		EntityAlive entity = this.entity;
		if (entity != null && entity.emodel != null)
		{
			entity.emodel.avatarController.SetCrouching(entity.IsCrouching);
			entity.SetVehiclePoseMode(entity.vehiclePoseMode);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public EntityAlive _entity;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float lastTimeStepPlayed;
}

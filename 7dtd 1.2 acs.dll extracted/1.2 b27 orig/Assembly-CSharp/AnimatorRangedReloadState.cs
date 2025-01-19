using System;
using Audio;
using UnityEngine;

public class AnimatorRangedReloadState : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		EntityAlive componentInParent = animator.GetComponentInParent<EntityAlive>();
		if (componentInParent == null)
		{
			return;
		}
		componentInParent.emodel.avatarController.UpdateInt("CurrentAnim", 3, true);
		this.actionData = (componentInParent.inventory.holdingItemData.actionData[0] as ItemActionRanged.ItemActionDataRanged);
		if (this.actionData == null)
		{
			return;
		}
		this.actionRanged = (ItemActionRanged)componentInParent.inventory.holdingItem.Actions[0];
		if (this.actionData.isReloading)
		{
			return;
		}
		if (this.actionData.invData.item.Properties.Values[ItemClass.PropSoundIdle] != null && this.actionData.invData.holdingEntitySoundID >= 0)
		{
			Manager.Stop(this.actionData.invData.holdingEntity.entityId, this.actionData.invData.item.Properties.Values[ItemClass.PropSoundIdle]);
		}
		this.actionData.wasAiming = this.actionData.invData.holdingEntity.AimingGun;
		if (this.actionData.invData.holdingEntity.AimingGun && this.actionData.invData.item.Actions[1] is ItemActionZoom)
		{
			this.actionData.invData.holdingEntity.inventory.Execute(1, false, null);
			this.actionData.invData.holdingEntity.inventory.Execute(1, true, null);
		}
		if (animator.GetCurrentAnimatorClipInfo(0).Length != 0 && animator.GetCurrentAnimatorClipInfo(0)[0].clip.events.Length == 0)
		{
			if (this.actionRanged.SoundReload != null)
			{
				componentInParent.PlayOneShot(this.actionRanged.SoundReload.Value, false, false, false);
			}
		}
		else if (animator.GetNextAnimatorClipInfo(0).Length != 0 && animator.GetNextAnimatorClipInfo(0)[0].clip.events.Length == 0 && this.actionRanged.SoundReload != null)
		{
			componentInParent.PlayOneShot(this.actionRanged.SoundReload.Value, false, false, false);
		}
		int num = (int)EffectManager.GetValue(PassiveEffects.MagazineSize, this.actionData.invData.itemValue, (float)this.actionRanged.BulletsPerMagazine, this.actionData.invData.holdingEntity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
		ItemActionLauncher itemActionLauncher = this.actionRanged as ItemActionLauncher;
		if (itemActionLauncher != null)
		{
			ItemValue itemValue = this.actionData.invData.itemValue;
			ItemValue item = ItemClass.GetItem(this.actionRanged.MagazineItemNames[(int)itemValue.SelectedAmmoTypeIndex], false);
			ItemActionLauncher.ItemActionDataLauncher itemActionDataLauncher = this.actionData as ItemActionLauncher.ItemActionDataLauncher;
			if (itemActionDataLauncher.isChangingAmmoType)
			{
				itemActionLauncher.DeleteProjectiles(this.actionData);
				itemActionDataLauncher.isChangingAmmoType = false;
			}
			int num2 = 1;
			if (!this.actionData.invData.holdingEntity.isEntityRemote)
			{
				num2 = (itemActionLauncher.HasInfiniteAmmo(this.actionData) ? num : this.GetAmmoCount(this.actionData.invData.holdingEntity, item, num));
			}
			for (int i = itemActionDataLauncher.projectileInstance.Count; i < num2; i++)
			{
				itemActionDataLauncher.projectileInstance.Add(itemActionLauncher.instantiateProjectile(this.actionData, new Vector3(0f, (float)i * 0.005f, 0f)));
			}
		}
		this.actionData.isReloading = true;
		this.actionData.isWeaponReloading = true;
		this.actionData.invData.holdingEntity.MinEventContext.ItemActionData = this.actionData;
		this.actionData.invData.holdingEntity.FireEvent(MinEventTypes.onReloadStart, true);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		EntityAlive componentInParent = animator.GetComponentInParent<EntityAlive>();
		if (componentInParent == null)
		{
			return;
		}
		componentInParent.emodel.avatarController.UpdateBool("Reload", false, true);
		if (this.actionData == null)
		{
			return;
		}
		animator.speed = 1f;
		if (!this.actionData.isReloading)
		{
			return;
		}
		if (!this.actionData.isReloadCancelled)
		{
			EntityAlive holdingEntity = this.actionData.invData.holdingEntity;
			ItemValue item = ItemClass.GetItem(this.actionRanged.MagazineItemNames[(int)this.actionData.invData.itemValue.SelectedAmmoTypeIndex], false);
			int num = (int)EffectManager.GetValue(PassiveEffects.MagazineSize, this.actionData.invData.itemValue, (float)this.actionRanged.BulletsPerMagazine, holdingEntity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
			this.actionData.reloadAmount = this.GetAmmoCountToReload(holdingEntity, item, num);
			if (this.actionData.reloadAmount > 0)
			{
				this.actionData.invData.itemValue.Meta = Utils.FastMin(this.actionData.invData.itemValue.Meta + this.actionData.reloadAmount, num);
				if (this.actionData.invData.item.Properties.Values[ItemClass.PropSoundIdle] != null)
				{
					this.actionData.invData.holdingEntitySoundID = -1;
				}
			}
		}
		this.actionData.isReloading = false;
		this.actionData.isWeaponReloading = false;
		this.actionData.invData.holdingEntity.MinEventContext.ItemActionData = this.actionData;
		this.actionData.invData.holdingEntity.FireEvent(MinEventTypes.onReloadStop, true);
		this.actionData.invData.holdingEntity.OnReloadEnd();
		this.actionData.invData.holdingEntity.inventory.CallOnToolbeltChangedInternal();
		this.actionData.isReloadCancelled = false;
		animator.SetBool("Reload", false);
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (this.actionData == null)
		{
			return;
		}
		if (this.actionData.isReloadCancelled)
		{
			animator.Play(0, -1, 1f);
			animator.Update(1f);
		}
		this.actionData.invData.holdingEntity.MinEventContext.ItemActionData = this.actionData;
		this.actionData.invData.holdingEntity.FireEvent(MinEventTypes.onReloadUpdate, true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int GetAmmoCountToReload(EntityAlive ea, ItemValue ammo, int modifiedMagazineSize)
	{
		if (this.actionRanged.HasInfiniteAmmo(this.actionData))
		{
			if (this.actionRanged.AmmoIsPerMagazine)
			{
				return modifiedMagazineSize;
			}
			return modifiedMagazineSize - this.actionData.invData.itemValue.Meta;
		}
		else if (ea.bag.GetItemCount(ammo, -1, -1, true) > 0)
		{
			if (this.actionRanged.AmmoIsPerMagazine)
			{
				return modifiedMagazineSize * ea.bag.DecItem(ammo, 1, false, null);
			}
			return ea.bag.DecItem(ammo, modifiedMagazineSize - this.actionData.invData.itemValue.Meta, false, null);
		}
		else
		{
			if (ea.inventory.GetItemCount(ammo, false, -1, -1, true) <= 0)
			{
				return 0;
			}
			if (this.actionRanged.AmmoIsPerMagazine)
			{
				return modifiedMagazineSize * ea.inventory.DecItem(ammo, 1, false, null);
			}
			return this.actionData.invData.holdingEntity.inventory.DecItem(ammo, modifiedMagazineSize - this.actionData.invData.itemValue.Meta, false, null);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int GetAmmoCount(EntityAlive ea, ItemValue ammo, int modifiedMagazineSize)
	{
		return Mathf.Min(ea.bag.GetItemCount(ammo, -1, -1, true) + ea.inventory.GetItemCount(ammo, false, -1, -1, true), modifiedMagazineSize);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public ItemActionRanged.ItemActionDataRanged actionData;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public ItemActionRanged actionRanged;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float MultiProjectileOffset = 0.005f;
}

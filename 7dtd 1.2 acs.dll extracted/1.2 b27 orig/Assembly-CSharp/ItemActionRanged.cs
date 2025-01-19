using System;
using System.Collections.Generic;
using System.Globalization;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionRanged : ItemActionAttack
{
	public override ItemActionData CreateModifierData(ItemInventoryData _invData, int _indexInEntityOfAction)
	{
		return new ItemActionRanged.ItemActionDataRanged(_invData, _indexInEntityOfAction);
	}

	public override void ReadFrom(DynamicProperties _props)
	{
		base.ReadFrom(_props);
		if (_props.Values.ContainsKey("bullet_material"))
		{
			this.bulletMaterialName = _props.Values["bullet_material"];
		}
		else
		{
			this.bulletMaterialName = "bullet";
		}
		if (_props.Values.ContainsKey("SupportHarvesting"))
		{
			this.bSupportHarvesting = StringParsers.ParseBool(_props.Values["SupportHarvesting"], 0, -1, true);
		}
		if (_props.Values.ContainsKey("UseMeleeCrosshair"))
		{
			this.bUseMeleeCrosshair = StringParsers.ParseBool(_props.Values["UseMeleeCrosshair"], 0, -1, true);
		}
		if (_props.Values.ContainsKey("EntityPenetrationCount"))
		{
			this.EntityPenetrationCount = StringParsers.ParseSInt32(_props.Values["EntityPenetrationCount"], 0, -1, NumberStyles.Integer);
		}
		else
		{
			this.EntityPenetrationCount = 0;
		}
		if (_props.Values.ContainsKey("BlockPenetrationFactor"))
		{
			this.BlockPenetrationFactor = StringParsers.ParseSInt32(_props.Values["BlockPenetrationFactor"], 0, -1, NumberStyles.Integer);
		}
		if (_props.Values.ContainsKey("AutoReload"))
		{
			this.AutoReload = StringParsers.ParseBool(_props.Values["AutoReload"], 0, -1, true);
		}
		if (_props.Values.ContainsKey("triggerEffectShootDualsense"))
		{
			this.triggerEffectShootDualsense = _props.Values["triggerEffectShootDualsense"];
		}
		else
		{
			this.triggerEffectShootDualsense = string.Empty;
		}
		if (_props.Values.ContainsKey("triggerEffectTriggerPullDualsense"))
		{
			this.triggerEffectTriggerPullDualsense = _props.Values["triggerEffectTriggerPullDualsense"];
		}
		else
		{
			this.triggerEffectTriggerPullDualsense = string.Empty;
		}
		if (_props.Values.ContainsKey("triggerEffectShootXbox"))
		{
			this.triggerEffectShootXbox = _props.Values["triggerEffectShootXbox"];
		}
		else
		{
			this.triggerEffectShootXbox = string.Empty;
		}
		if (_props.Values.ContainsKey("triggerEffectTriggerPullXbox"))
		{
			this.triggerEffectTriggerPullXbox = _props.Values["triggerEffectTriggerPullXbox"];
		}
		else
		{
			this.triggerEffectTriggerPullXbox = string.Empty;
		}
		_props.ParseFloat("SpreadVerticalOffset", ref this.spreadVerticalOffset);
		_props.ParseBool("RapidTrigger", ref this.rapidTrigger);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool canShowOverlay(ItemActionData actionData)
	{
		return this.bSupportHarvesting;
	}

	public bool IsSingleMagazineUsage()
	{
		return this.AmmoIsPerMagazine;
	}

	public override ItemClass.EnumCrosshairType GetCrosshairType(ItemActionData _actionData)
	{
		if (!this.bUseMeleeCrosshair)
		{
			return ItemClass.EnumCrosshairType.Crosshair;
		}
		return ItemClass.EnumCrosshairType.Plus;
	}

	public override RenderCubeType GetFocusType(ItemActionData _actionData)
	{
		return RenderCubeType.None;
	}

	public override void OnModificationsChanged(ItemActionData _data)
	{
		ItemActionRanged.ItemActionDataRanged itemActionDataRanged = _data as ItemActionRanged.ItemActionDataRanged;
		if (itemActionDataRanged.OriginalDelay == -1f && this.Properties.Values.ContainsKey("Delay"))
		{
			itemActionDataRanged.OriginalDelay = StringParsers.ParseFloat(this.Properties.Values["Delay"], 0, -1, NumberStyles.Any);
		}
		if (this.Properties.Values.ContainsKey("ScopeOffset"))
		{
			itemActionDataRanged.ScopeTransformOffset = StringParsers.ParseVector3(itemActionDataRanged.invData.itemValue.GetPropertyOverride("ScopeOffset", this.Properties.Values["ScopeOffset"]), 0, -1);
		}
		else
		{
			itemActionDataRanged.ScopeTransformOffset = StringParsers.ParseVector3(itemActionDataRanged.invData.itemValue.GetPropertyOverride("ScopeOffset", "0,0,0"), 0, -1);
		}
		if (this.Properties.Values.ContainsKey("SideOffset"))
		{
			itemActionDataRanged.SideTransformOffset = StringParsers.ParseVector3(itemActionDataRanged.invData.itemValue.GetPropertyOverride("SideOffset", this.Properties.Values["SideOffset"]), 0, -1);
		}
		else
		{
			itemActionDataRanged.SideTransformOffset = StringParsers.ParseVector3(itemActionDataRanged.invData.itemValue.GetPropertyOverride("SideOffset", "0,0,0"), 0, -1);
		}
		if (this.Properties.Values.ContainsKey("BarrelOffset"))
		{
			itemActionDataRanged.BarrelTransformOffset = StringParsers.ParseVector3(itemActionDataRanged.invData.itemValue.GetPropertyOverride("BarrelOffset", this.Properties.Values["BarrelOffset"]), 0, -1);
		}
		else
		{
			itemActionDataRanged.BarrelTransformOffset = StringParsers.ParseVector3(itemActionDataRanged.invData.itemValue.GetPropertyOverride("BarrelOffset", "0,0,0"), 0, -1);
		}
		if (this.Properties.Values.ContainsKey("ScopeScale"))
		{
			itemActionDataRanged.ScopeTransformScale = StringParsers.ParseVector3(itemActionDataRanged.invData.itemValue.GetPropertyOverride("ScopeScale", this.Properties.Values["ScopeScale"]), 0, -1);
		}
		else
		{
			itemActionDataRanged.ScopeTransformScale = StringParsers.ParseVector3(itemActionDataRanged.invData.itemValue.GetPropertyOverride("ScopeScale", "1,1,1"), 0, -1);
		}
		if (this.Properties.Values.ContainsKey("SideScale"))
		{
			itemActionDataRanged.SideTransformScale = StringParsers.ParseVector3(itemActionDataRanged.invData.itemValue.GetPropertyOverride("SideScale", this.Properties.Values["SideScale"]), 0, -1);
		}
		else
		{
			itemActionDataRanged.SideTransformScale = StringParsers.ParseVector3(itemActionDataRanged.invData.itemValue.GetPropertyOverride("SideScale", "1,1,1"), 0, -1);
		}
		if (this.Properties.Values.ContainsKey("BarrelScale"))
		{
			itemActionDataRanged.BarrelTransformScale = StringParsers.ParseVector3(itemActionDataRanged.invData.itemValue.GetPropertyOverride("BarrelScale", this.Properties.Values["BarrelScale"]), 0, -1);
		}
		else
		{
			itemActionDataRanged.BarrelTransformScale = StringParsers.ParseVector3(itemActionDataRanged.invData.itemValue.GetPropertyOverride("BarrelScale", "1,1,1"), 0, -1);
		}
		if (this.Properties.Values.ContainsKey("ScopeRotation"))
		{
			itemActionDataRanged.ScopeTransformRotation = StringParsers.ParseVector3(itemActionDataRanged.invData.itemValue.GetPropertyOverride("ScopeRotation", this.Properties.Values["ScopeRotation"]), 0, -1);
		}
		else
		{
			itemActionDataRanged.ScopeTransformRotation = StringParsers.ParseVector3(itemActionDataRanged.invData.itemValue.GetPropertyOverride("ScopeRotation", "0,0,0"), 0, -1);
		}
		if (this.Properties.Values.ContainsKey("SideRotation"))
		{
			itemActionDataRanged.SideTransformRotation = StringParsers.ParseVector3(itemActionDataRanged.invData.itemValue.GetPropertyOverride("SideRotation", this.Properties.Values["SideRotation"]), 0, -1);
		}
		else
		{
			itemActionDataRanged.SideTransformRotation = StringParsers.ParseVector3(itemActionDataRanged.invData.itemValue.GetPropertyOverride("SideRotation", "0,0,0"), 0, -1);
		}
		if (this.Properties.Values.ContainsKey("BarrelRotation"))
		{
			itemActionDataRanged.BarrelTransformRotation = StringParsers.ParseVector3(itemActionDataRanged.invData.itemValue.GetPropertyOverride("BarrelRotation", this.Properties.Values["BarrelRotation"]), 0, -1);
		}
		else
		{
			itemActionDataRanged.BarrelTransformRotation = StringParsers.ParseVector3(itemActionDataRanged.invData.itemValue.GetPropertyOverride("BarrelRotation", "0,0,0"), 0, -1);
		}
		string originalValue = "";
		this.Properties.ParseString("Sound_start", ref originalValue);
		itemActionDataRanged.SoundStart = itemActionDataRanged.invData.itemValue.GetPropertyOverride("Sound_start", originalValue);
		originalValue = "";
		this.Properties.ParseString("Sound_loop", ref originalValue);
		itemActionDataRanged.SoundLoop = itemActionDataRanged.invData.itemValue.GetPropertyOverride("Sound_loop", originalValue);
		originalValue = "";
		this.Properties.ParseString("Sound_end", ref originalValue);
		itemActionDataRanged.SoundEnd = itemActionDataRanged.invData.itemValue.GetPropertyOverride("Sound_end", originalValue);
		if (this.soundStart.Contains("silenced"))
		{
			itemActionDataRanged.IsFlashSuppressed = true;
		}
		itemActionDataRanged.Laser = ((itemActionDataRanged.invData.model != null) ? itemActionDataRanged.invData.model.FindInChilds("laser", false) : null);
		itemActionDataRanged.bReleased = true;
		if (itemActionDataRanged.ScopeTransform != null && itemActionDataRanged.ScopeTransform.localPosition != itemActionDataRanged.ScopeTransformOffset)
		{
			itemActionDataRanged.ScopeTransform.localPosition = itemActionDataRanged.ScopeTransformOffset;
		}
		if (itemActionDataRanged.SideTransform != null && itemActionDataRanged.SideTransform.localPosition != itemActionDataRanged.SideTransformOffset)
		{
			itemActionDataRanged.SideTransform.localPosition = itemActionDataRanged.SideTransformOffset;
		}
		if (itemActionDataRanged.BarrelTransform != null && itemActionDataRanged.BarrelTransform.localPosition != itemActionDataRanged.BarrelTransformOffset)
		{
			itemActionDataRanged.BarrelTransform.localPosition = itemActionDataRanged.BarrelTransformOffset;
		}
		if (itemActionDataRanged.ScopeTransform != null && itemActionDataRanged.ScopeTransform.localScale != itemActionDataRanged.ScopeTransformScale)
		{
			itemActionDataRanged.ScopeTransform.localScale = itemActionDataRanged.ScopeTransformScale;
		}
		if (itemActionDataRanged.SideTransform != null && itemActionDataRanged.SideTransform.localScale != itemActionDataRanged.SideTransformScale)
		{
			itemActionDataRanged.SideTransform.localScale = itemActionDataRanged.SideTransformScale;
		}
		if (itemActionDataRanged.BarrelTransform != null && itemActionDataRanged.BarrelTransform.localScale != itemActionDataRanged.BarrelTransformScale)
		{
			itemActionDataRanged.BarrelTransform.localScale = itemActionDataRanged.BarrelTransformScale;
		}
		if (itemActionDataRanged.ScopeTransform != null && itemActionDataRanged.ScopeTransform.localRotation.eulerAngles != itemActionDataRanged.ScopeTransformRotation)
		{
			itemActionDataRanged.ScopeTransform.localRotation = Quaternion.Euler(itemActionDataRanged.ScopeTransformRotation);
		}
		if (itemActionDataRanged.SideTransform != null && itemActionDataRanged.SideTransform.localRotation.eulerAngles != itemActionDataRanged.SideTransformRotation)
		{
			itemActionDataRanged.SideTransform.localRotation = Quaternion.Euler(itemActionDataRanged.SideTransformRotation);
		}
		if (itemActionDataRanged.BarrelTransform != null && itemActionDataRanged.BarrelTransform.localRotation.eulerAngles != itemActionDataRanged.BarrelTransformRotation)
		{
			itemActionDataRanged.BarrelTransform.localRotation = Quaternion.Euler(itemActionDataRanged.BarrelTransformRotation);
		}
	}

	public override void StartHolding(ItemActionData _data)
	{
		base.StartHolding(_data);
		if (_data.invData.holdingEntity as EntityPlayerLocal != null)
		{
			TriggerEffectManager.ControllerTriggerEffect triggerEffect = TriggerEffectManager.GetTriggerEffect(new ValueTuple<string, string>(this.triggerEffectTriggerPullDualsense, this.triggerEffectTriggerPullXbox));
			GameManager.Instance.triggerEffectManager.SetTriggerEffect(TriggerEffectManager.GamepadTrigger.RightTrigger, triggerEffect, false);
		}
	}

	public override void StopHolding(ItemActionData _data)
	{
		base.StopHolding(_data);
		ItemActionRanged.ItemActionDataRanged itemActionDataRanged = (ItemActionRanged.ItemActionDataRanged)_data;
		if (itemActionDataRanged.state != ItemActionFiringState.Off)
		{
			itemActionDataRanged.state = ItemActionFiringState.Off;
			this.ItemActionEffects(GameManager.Instance, itemActionDataRanged, 0, Vector3.zero, Vector3.forward, 0);
		}
		itemActionDataRanged.bReleased = true;
		itemActionDataRanged.lastAccuracy = 1f;
		if (_data.invData.holdingEntity as EntityPlayerLocal != null)
		{
			GameManager.Instance.triggerEffectManager.SetTriggerEffect(TriggerEffectManager.GamepadTrigger.RightTrigger, TriggerEffectManager.NoneEffect, false);
		}
		this.stopParticles(itemActionDataRanged.muzzle);
		this.stopParticles(itemActionDataRanged.muzzle2);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void stopParticles(Transform t)
	{
		if (t != null)
		{
			ParticleSystem[] componentsInChildren = t.GetComponentsInChildren<ParticleSystem>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				UnityEngine.Object.Destroy(componentsInChildren[i]);
			}
		}
	}

	public override bool IsActionRunning(ItemActionData _actionData)
	{
		ItemActionRanged.ItemActionDataRanged itemActionDataRanged = (ItemActionRanged.ItemActionDataRanged)_actionData;
		return this.reloading(itemActionDataRanged) || ((!this.rapidTrigger || !itemActionDataRanged.bReleased || Time.time - itemActionDataRanged.m_LastShotTime <= 0.25f) && Time.time - itemActionDataRanged.m_LastShotTime < itemActionDataRanged.Delay);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void removeCurrentlyLoadedAmmunition(ItemValue _gun, ItemValue _ammo, EntityAlive _entity)
	{
		ItemStack itemStack = new ItemStack(_ammo, _gun.Meta);
		int itemCount = _entity.bag.GetItemCount(_ammo, -1, -1, true);
		int itemCount2 = _entity.inventory.GetItemCount(_ammo, false, -1, -1, true);
		EntityPlayerLocal entityPlayerLocal = _entity as EntityPlayerLocal;
		if (itemStack.count > 0)
		{
			if (itemCount > 0)
			{
				if (!entityPlayerLocal.bag.AddItem(itemStack) && !entityPlayerLocal.inventory.AddItem(itemStack))
				{
					GameManager.Instance.ItemDropServer(itemStack, entityPlayerLocal.GetPosition(), new Vector3(0.5f, 0f, 0.5f), entityPlayerLocal.entityId, 60f, false);
					entityPlayerLocal.PlayOneShot("itemdropped", false, false, false);
				}
			}
			else if (itemCount2 > 0)
			{
				if (!entityPlayerLocal.inventory.AddItem(itemStack) && !entityPlayerLocal.bag.AddItem(itemStack))
				{
					GameManager.Instance.ItemDropServer(itemStack, entityPlayerLocal.GetPosition(), new Vector3(0.5f, 0f, 0.5f), entityPlayerLocal.entityId, 60f, false);
					entityPlayerLocal.PlayOneShot("itemdropped", false, false, false);
				}
			}
			else if (!entityPlayerLocal.bag.AddItem(itemStack) && !entityPlayerLocal.inventory.AddItem(itemStack))
			{
				GameManager.Instance.ItemDropServer(itemStack, entityPlayerLocal.GetPosition(), new Vector3(0.5f, 0f, 0.5f), entityPlayerLocal.entityId, 60f, false);
				entityPlayerLocal.PlayOneShot("itemdropped", false, false, false);
			}
		}
		_gun.Meta = 0;
		_entity.inventory.CallOnToolbeltChangedInternal();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void loadNewAmmunition(ItemValue _gun, ItemValue _ammo, EntityAlive _entity)
	{
		ItemActionRanged.ItemActionDataRanged itemActionDataRanged = (ItemActionRanged.ItemActionDataRanged)_entity.inventory.holdingItemData.actionData[0];
		if ((int)_gun.SelectedAmmoTypeIndex == this.MagazineItemNames.Length)
		{
			_gun.SelectedAmmoTypeIndex = 0;
		}
		itemActionDataRanged.isChangingAmmoType = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setSelectedAmmoById(int _ammoItemId, ItemValue _gun)
	{
		for (int i = 0; i < this.MagazineItemNames.Length; i++)
		{
			if (ItemClass.GetItem(this.MagazineItemNames[i], false).type == _ammoItemId)
			{
				_gun.SelectedAmmoTypeIndex = (byte)i;
				return;
			}
		}
	}

	public virtual void SetAmmoType(EntityAlive _entity, ref ItemValue _gun, int _lastSelectedIndex, int _newSelectedIndex)
	{
		_gun.SelectedAmmoTypeIndex = (byte)_newSelectedIndex;
		if (_gun.Equals(_entity.inventory.holdingItemItemValue))
		{
			this.SwapAmmoType(_entity, ItemClass.GetItem(this.MagazineItemNames[_newSelectedIndex], false).type);
			return;
		}
		ItemValue item = ItemClass.GetItem(this.MagazineItemNames[_lastSelectedIndex], false);
		ItemClass.GetItem(this.MagazineItemNames[_newSelectedIndex], false);
		this.removeCurrentlyLoadedAmmunition(_gun, item, _entity);
		GameManager.Instance.ItemReloadServer(_entity.entityId);
	}

	public void SwapSelectedAmmo(EntityAlive _entity, int _ammoIndex)
	{
		if (_ammoIndex == (int)_entity.inventory.holdingItemItemValue.SelectedAmmoTypeIndex)
		{
			if (_entity.inventory.GetHoldingGun().CanReload(_entity.inventory.holdingItemData.actionData[0]))
			{
				GameManager.Instance.ItemReloadServer(_entity.entityId);
			}
			return;
		}
		ItemClass itemClass = ItemClass.GetItemClass(this.MagazineItemNames[_ammoIndex], false);
		if (itemClass != null)
		{
			this.SwapAmmoType(_entity, itemClass.Id);
		}
	}

	public override void SwapAmmoType(EntityAlive _entity, int _ammoItemId = -1)
	{
		ItemActionRanged.ItemActionDataRanged itemActionDataRanged = (ItemActionRanged.ItemActionDataRanged)_entity.inventory.holdingItemData.actionData[0];
		this.CancelReload(itemActionDataRanged);
		ItemValue itemValue = itemActionDataRanged.invData.itemValue;
		EntityAlive holdingEntity = itemActionDataRanged.invData.holdingEntity;
		ItemValue item = ItemClass.GetItem(this.MagazineItemNames[(int)itemValue.SelectedAmmoTypeIndex], false);
		itemActionDataRanged.reloadAmount = 0;
		this.removeCurrentlyLoadedAmmunition(itemValue, item, holdingEntity);
		if (_ammoItemId == -1)
		{
			for (int i = 0; i < this.MagazineItemNames.Length; i++)
			{
				ItemValue itemValue2 = itemValue;
				itemValue2.SelectedAmmoTypeIndex += 1;
				if ((int)itemValue.SelectedAmmoTypeIndex == this.MagazineItemNames.Length)
				{
					itemValue.SelectedAmmoTypeIndex = 0;
				}
				ItemValue item2 = ItemClass.GetItem(this.MagazineItemNames[(int)itemValue.SelectedAmmoTypeIndex], false);
				if (itemActionDataRanged.invData.holdingEntity.inventory.GetItemCount(item2, false, -1, -1, true) + itemActionDataRanged.invData.holdingEntity.bag.GetItemCount(item2, -1, -1, true) + itemActionDataRanged.invData.itemValue.Meta > 0)
				{
					break;
				}
			}
		}
		else
		{
			this.setSelectedAmmoById(_ammoItemId, itemValue);
		}
		ItemValue item3 = ItemClass.GetItem(this.MagazineItemNames[(int)itemValue.SelectedAmmoTypeIndex], false);
		_entity.inventory.CallOnToolbeltChangedInternal();
		this.loadNewAmmunition(itemValue, item3, holdingEntity);
		if (itemActionDataRanged.invData.holdingEntity as EntityPlayerLocal != null)
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageHoldingItem>().Setup(itemActionDataRanged.invData.holdingEntity), false, -1, -1, -1, null, 192);
			}
			else
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageHoldingItem>().Setup(itemActionDataRanged.invData.holdingEntity), false);
			}
		}
		GameManager.Instance.ItemReloadServer(itemActionDataRanged.invData.holdingEntity.entityId);
		_entity.inventory.ShowHeldItem(true, 0.015f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CycleAmmoType(ItemActionData _actionData, bool excludeNonUnderwaterAmmoTypes)
	{
		if (this.MagazineItemNames.Length <= 1)
		{
			return;
		}
		int num = (int)_actionData.invData.holdingEntity.inventory.holdingItemItemValue.SelectedAmmoTypeIndex;
		int num2 = num;
		num--;
		while (num != num2)
		{
			ItemValue item = ItemClass.GetItem(this.MagazineItemNames[num], false);
			if (excludeNonUnderwaterAmmoTypes && !item.ItemClass.UsableUnderwater)
			{
				num--;
			}
			else
			{
				if (_actionData.invData.holdingEntity.bag.GetItemCount(item, -1, -1, true) > 0)
				{
					break;
				}
				num--;
				if (num < 0)
				{
					num = this.MagazineItemNames.Length - 1;
				}
			}
		}
		this.SwapSelectedAmmo(_actionData.invData.holdingEntity, num);
	}

	public virtual bool IsAmmoUsableUnderwater(EntityAlive holdingEntity)
	{
		int selectedAmmoTypeIndex = (int)holdingEntity.inventory.holdingItemItemValue.SelectedAmmoTypeIndex;
		return ItemClass.GetItem(this.MagazineItemNames[selectedAmmoTypeIndex], false).ItemClass.UsableUnderwater;
	}

	public override void OnHoldingUpdate(ItemActionData _actionData)
	{
		ItemActionRanged.ItemActionDataRanged itemActionDataRanged = (ItemActionRanged.ItemActionDataRanged)_actionData;
		itemActionDataRanged.Delay = 60f / EffectManager.GetValue(PassiveEffects.RoundsPerMinute, itemActionDataRanged.invData.itemValue, 60f / itemActionDataRanged.OriginalDelay, itemActionDataRanged.invData.holdingEntity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
		itemActionDataRanged.lastUseTime = Time.time;
		if (_actionData.invData.holdingEntity.isHeadUnderwater && _actionData.invData.itemValue.ItemClass != null)
		{
			if (!_actionData.invData.itemValue.ItemClass.UsableUnderwater)
			{
				return;
			}
			if (this.MagazineItemNames != null && !ItemClass.GetItemClass(this.MagazineItemNames[(int)_actionData.invData.itemValue.SelectedAmmoTypeIndex], false).UsableUnderwater)
			{
				this.CycleAmmoType(_actionData, true);
				return;
			}
		}
		if (itemActionDataRanged.state != ItemActionFiringState.Off && itemActionDataRanged.m_LastShotTime > 0f && Time.time > itemActionDataRanged.m_LastShotTime + itemActionDataRanged.Delay * 2f)
		{
			this.triggerReleased(itemActionDataRanged, _actionData.indexInEntityOfAction);
		}
		this.updateAccuracy(_actionData, _actionData.invData.holdingEntity.AimingGun);
		if (itemActionDataRanged.SideTransform && itemActionDataRanged.Laser == null && itemActionDataRanged.SideTransform.childCount > 0)
		{
			itemActionDataRanged.Laser = ItemActionRanged.ItemActionDataRanged.getModelChildTransformByName(_actionData.invData, "laser");
		}
		if (ItemAction.ShowDistanceDebugInfo || (_actionData as ItemActionRanged.ItemActionDataRanged).Laser != null)
		{
			this.GetExecuteActionTarget(_actionData);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool reloadCancelled(ItemActionRanged.ItemActionDataRanged actionData)
	{
		return actionData.isReloadCancelled || actionData.isWeaponReloadCancelled;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool notReloadCancelled(ItemActionRanged.ItemActionDataRanged actionData)
	{
		return !actionData.isReloadCancelled || !actionData.isWeaponReloadCancelled;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool reloading(ItemActionRanged.ItemActionDataRanged actionData)
	{
		return actionData.isReloading || actionData.isWeaponReloading;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool notReloading(ItemActionRanged.ItemActionDataRanged actionData)
	{
		return !actionData.isReloading || !actionData.isWeaponReloading;
	}

	public override void CancelReload(ItemActionData _data)
	{
		ItemActionRanged.ItemActionDataRanged itemActionDataRanged = (ItemActionRanged.ItemActionDataRanged)_data;
		if (this.notReloading(itemActionDataRanged) || this.reloadCancelled(itemActionDataRanged))
		{
			return;
		}
		base.CancelReload(_data);
		itemActionDataRanged.isReloadCancelled = true;
		itemActionDataRanged.isWeaponReloadCancelled = true;
		itemActionDataRanged.isChangingAmmoType = false;
		if (itemActionDataRanged.state != ItemActionFiringState.Off)
		{
			itemActionDataRanged.state = ItemActionFiringState.Off;
			this.ItemActionEffects(GameManager.Instance, itemActionDataRanged, 0, Vector3.zero, Vector3.forward, 0);
		}
	}

	public override bool CanReload(ItemActionData _actionData)
	{
		ItemActionRanged.ItemActionDataRanged actionData = (ItemActionRanged.ItemActionDataRanged)_actionData;
		ItemValue holdingItemItemValue = _actionData.invData.holdingEntity.inventory.holdingItemItemValue;
		ItemValue item = ItemClass.GetItem(this.MagazineItemNames[(int)holdingItemItemValue.SelectedAmmoTypeIndex], false);
		int num = (int)EffectManager.GetValue(PassiveEffects.MagazineSize, holdingItemItemValue, (float)this.BulletsPerMagazine, _actionData.invData.holdingEntity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
		return this.notReloading(actionData) && _actionData.invData.itemValue.Meta < num && (_actionData.invData.holdingEntity.inventory.GetItemCount(item, false, -1, -1, true) > 0 || _actionData.invData.holdingEntity.bag.GetItemCount(item, -1, -1, true) > 0 || this.HasInfiniteAmmo(_actionData));
	}

	public override void ReloadGun(ItemActionData _actionData)
	{
		Manager.StopSequence(_actionData.invData.holdingEntity, ((ItemActionRanged.ItemActionDataRanged)_actionData).SoundStart);
		if (!_actionData.invData.holdingEntity.isEntityRemote)
		{
			_actionData.invData.holdingEntity.emodel.avatarController.CancelEvent("WeaponFire");
			_actionData.invData.holdingEntity.OnReloadStart();
		}
	}

	public override bool IsAimingGunPossible(ItemActionData _actionData)
	{
		return this.notReloading((ItemActionRanged.ItemActionDataRanged)_actionData);
	}

	public override EnumCameraShake GetCameraShakeType(ItemActionData _actionData)
	{
		if (!_actionData.invData.holdingEntity.AimingGun)
		{
			return EnumCameraShake.Small;
		}
		return EnumCameraShake.None;
	}

	public override TriggerEffectManager.ControllerTriggerEffect GetControllerTriggerEffectPull()
	{
		return TriggerEffectManager.GetTriggerEffect(new ValueTuple<string, string>(this.triggerEffectTriggerPullDualsense, this.triggerEffectTriggerPullXbox));
	}

	public override TriggerEffectManager.ControllerTriggerEffect GetControllerTriggerEffectShoot()
	{
		return TriggerEffectManager.GetTriggerEffect(new ValueTuple<string, string>(this.triggerEffectShootDualsense, this.triggerEffectShootXbox));
	}

	public override bool AllowItemLoopingSound(ItemActionData _actionData)
	{
		ItemActionRanged.ItemActionDataRanged itemActionDataRanged = (ItemActionRanged.ItemActionDataRanged)_actionData;
		int burstCount = this.GetBurstCount(_actionData);
		return _actionData.invData.itemValue.Meta > 0 && burstCount > 1 && (int)itemActionDataRanged.curBurstCount < burstCount && !string.IsNullOrEmpty(this.soundRepeat) && itemActionDataRanged.state == ItemActionFiringState.Loop;
	}

	public override void ItemActionEffects(GameManager _gameManager, ItemActionData _actionData, int _firingState, Vector3 _startPos, Vector3 _direction, int _userData = 0)
	{
		if (GameManager.Instance.IsPaused())
		{
			return;
		}
		ItemActionRanged.ItemActionDataRanged itemActionDataRanged = _actionData as ItemActionRanged.ItemActionDataRanged;
		EntityAlive holdingEntity = _actionData.invData.holdingEntity;
		EntityPlayerLocal entityPlayerLocal = holdingEntity as EntityPlayerLocal;
		if (entityPlayerLocal != null)
		{
			ItemActionFiringState state = itemActionDataRanged.state;
			if (state != ItemActionFiringState.Off)
			{
				if (state - ItemActionFiringState.Start > 1)
				{
				}
				TriggerEffectManager.ControllerTriggerEffect triggerEffect = TriggerEffectManager.GetTriggerEffect(new ValueTuple<string, string>(this.triggerEffectShootDualsense, this.triggerEffectShootXbox));
				GameManager.Instance.triggerEffectManager.SetTriggerEffect(TriggerEffectManager.GamepadTrigger.RightTrigger, triggerEffect, false);
			}
			else
			{
				TriggerEffectManager.ControllerTriggerEffect triggerEffect = TriggerEffectManager.GetTriggerEffect(new ValueTuple<string, string>(this.triggerEffectTriggerPullDualsense, this.triggerEffectTriggerPullXbox));
				GameManager.Instance.triggerEffectManager.SetTriggerEffect(TriggerEffectManager.GamepadTrigger.RightTrigger, triggerEffect, false);
			}
		}
		bool flag = false;
		if (itemActionDataRanged.state != ItemActionFiringState.Off || holdingEntity.isEntityRemote)
		{
			if (_firingState == 0 && itemActionDataRanged.invData.itemValue.Meta != 0)
			{
				if (!Manager.IsASequence(holdingEntity, itemActionDataRanged.SoundStart))
				{
					if (itemActionDataRanged.state != ItemActionFiringState.Off)
					{
						Manager.Play(holdingEntity, itemActionDataRanged.SoundEnd, 1f, false);
					}
				}
				else if (itemActionDataRanged.state != ItemActionFiringState.Off || holdingEntity.isEntityRemote)
				{
					Manager.StopSequence(holdingEntity, itemActionDataRanged.SoundStart);
				}
			}
			else if (_firingState != 0 && itemActionDataRanged.invData.itemValue.Meta == 0)
			{
				if (!Manager.IsASequence(holdingEntity, itemActionDataRanged.SoundStart))
				{
					if (itemActionDataRanged.state != ItemActionFiringState.Off)
					{
						Manager.Play(holdingEntity, itemActionDataRanged.SoundStart, 1f, false);
						flag = true;
					}
				}
				else if (itemActionDataRanged.state != ItemActionFiringState.Off || holdingEntity.isEntityRemote)
				{
					Manager.StopSequence(holdingEntity, itemActionDataRanged.SoundStart);
				}
			}
			else if (itemActionDataRanged.invData.itemValue.Meta == 0 && Manager.IsASequence(holdingEntity, itemActionDataRanged.SoundStart) && (itemActionDataRanged.state != ItemActionFiringState.Off || holdingEntity.isEntityRemote))
			{
				Manager.StopSequence(holdingEntity, itemActionDataRanged.SoundStart);
			}
		}
		if (_firingState != 0)
		{
			this.onHoldingEntityFired(_actionData);
			string text = (_firingState == 1) ? itemActionDataRanged.SoundStart : itemActionDataRanged.SoundLoop;
			if (!string.IsNullOrEmpty(text))
			{
				if (!Manager.IsASequence(holdingEntity, text))
				{
					if (!flag || _firingState != 1)
					{
						Manager.Play(holdingEntity, text, 1f, false);
					}
				}
				else
				{
					Manager.PlaySequence(holdingEntity, text);
				}
			}
			if (holdingEntity.inventory.IsHUDDisabled())
			{
				return;
			}
			if (!itemActionDataRanged.IsFlashSuppressed && itemActionDataRanged.muzzle)
			{
				bool flag2 = entityPlayerLocal && entityPlayerLocal.bFirstPersonView;
				if (this.particlesMuzzleFire != null)
				{
					ParticleEffect pe = new ParticleEffect((flag2 && this.particlesMuzzleFireFpv != null) ? this.particlesMuzzleFireFpv : this.particlesMuzzleFire, Vector3.zero, 1f, Color.clear, null, itemActionDataRanged.muzzle, false);
					Transform transform = _gameManager.SpawnParticleEffectClientForceCreation(pe, holdingEntity.entityId, false);
					if (transform)
					{
						if (itemActionDataRanged.IsDoubleBarrel && itemActionDataRanged.invData.itemValue.Meta == 0)
						{
							transform.SetParent(itemActionDataRanged.muzzle2, false);
						}
						else
						{
							transform.SetParent(itemActionDataRanged.muzzle, false);
						}
						if (transform.GetComponentsInChildren<ParticleSystem>().Length != 0 && entityPlayerLocal == GameManager.Instance.World.GetPrimaryPlayer() && entityPlayerLocal.vp_FPController.OnValue_IsFirstPerson)
						{
							Utils.SetLayerRecursively(transform.gameObject, 10, null);
						}
					}
				}
				if (this.particlesMuzzleSmoke != null)
				{
					float lightValue = _gameManager.World.GetLightBrightness(World.worldToBlockPos(itemActionDataRanged.muzzle.position)) / 2f;
					ParticleEffect pe2 = new ParticleEffect((flag2 && this.particlesMuzzleSmokeFpv != null) ? this.particlesMuzzleSmokeFpv : this.particlesMuzzleSmoke, Vector3.zero, lightValue, Color.clear, null, null, false);
					Transform transform2 = _gameManager.SpawnParticleEffectClientForceCreation(pe2, holdingEntity.entityId, false);
					if (transform2 && entityPlayerLocal == GameManager.Instance.World.GetPrimaryPlayer() && entityPlayerLocal.vp_FPController.OnValue_IsFirstPerson)
					{
						transform2.gameObject.layer = 10;
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void onHoldingEntityFired(ItemActionData _actionData)
	{
		if (!_actionData.invData.holdingEntity.isEntityRemote)
		{
			_actionData.invData.holdingEntity.emodel.avatarController.UpdateFloat("MeleeAttackSpeed", 1f / ((ItemActionRanged.ItemActionDataRanged)_actionData).Delay, true);
			_actionData.invData.holdingEntity.OnFired();
		}
		(_actionData as ItemActionRanged.ItemActionDataRanged).lastAccuracy *= EffectManager.GetValue(PassiveEffects.IncrementalSpreadMultiplier, _actionData.invData.itemValue, 1f, _actionData.invData.holdingEntity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
		(_actionData as ItemActionRanged.ItemActionDataRanged).lastAccuracy = Mathf.Min((_actionData as ItemActionRanged.ItemActionDataRanged).lastAccuracy, 5f);
		if (_actionData.invData.holdingEntity as EntityPlayerLocal != null)
		{
			GameManager.Instance.triggerEffectManager.SetTriggerEffect(TriggerEffectManager.GamepadTrigger.RightTrigger, TriggerEffectManager.GetTriggerEffect(new ValueTuple<string, string>(this.triggerEffectShootDualsense, this.triggerEffectShootXbox)), false);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void triggerReleased(ItemActionRanged.ItemActionDataRanged myActionData, int _idx)
	{
		myActionData.bPressed = false;
		myActionData.bReleased = true;
		myActionData.invData.gameManager.ItemActionEffectsServer(myActionData.invData.holdingEntity.entityId, myActionData.invData.slotIdx, _idx, 0, Vector3.zero, Vector3.zero, 0);
		myActionData.state = ItemActionFiringState.Off;
		if (myActionData.invData.holdingEntity as EntityPlayerLocal != null)
		{
			GameManager.Instance.triggerEffectManager.SetTriggerEffect(TriggerEffectManager.GamepadTrigger.RightTrigger, TriggerEffectManager.GetTriggerEffect(new ValueTuple<string, string>(this.triggerEffectTriggerPullDualsense, this.triggerEffectTriggerPullXbox)), false);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual int getUserData(ItemActionData _actionData)
	{
		return 0;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void ConsumeAmmo(ItemActionData _actionData)
	{
		_actionData.invData.itemValue.Meta--;
	}

	public override void ExecuteAction(ItemActionData _actionData, bool _bReleased)
	{
		ItemActionRanged.ItemActionDataRanged itemActionDataRanged = (ItemActionRanged.ItemActionDataRanged)_actionData;
		if (_bReleased)
		{
			itemActionDataRanged.bReleased = true;
			itemActionDataRanged.curBurstCount = 0;
			if (Manager.IsASequence(_actionData.invData.holdingEntity, itemActionDataRanged.SoundStart))
			{
				Manager.StopSequence(_actionData.invData.holdingEntity, itemActionDataRanged.SoundStart);
			}
			this.triggerReleased(itemActionDataRanged, _actionData.indexInEntityOfAction);
			return;
		}
		bool flag = !itemActionDataRanged.bPressed;
		bool flag2 = flag && this.rapidTrigger;
		itemActionDataRanged.bPressed = true;
		int burstCount = this.GetBurstCount(_actionData);
		bool flag3 = (int)itemActionDataRanged.curBurstCount < burstCount;
		flag3 |= (burstCount == -1);
		if (!flag2 && !flag3 && !itemActionDataRanged.bReleased)
		{
			return;
		}
		bool bReleased = itemActionDataRanged.bReleased;
		itemActionDataRanged.bReleased = false;
		if (this.reloading(itemActionDataRanged))
		{
			itemActionDataRanged.m_LastShotTime = Time.time;
			return;
		}
		if (!flag2 && Time.time - itemActionDataRanged.m_LastShotTime < itemActionDataRanged.Delay)
		{
			return;
		}
		if (itemActionDataRanged.burstShotStarted)
		{
			itemActionDataRanged.burstShotStarted = false;
		}
		itemActionDataRanged.m_LastShotTime = Time.time;
		EntityAlive holdingEntity = _actionData.invData.holdingEntity;
		holdingEntity.MinEventContext.Other = null;
		if (EffectManager.GetValue(PassiveEffects.DisableItem, holdingEntity.inventory.holdingItemItemValue, 0f, holdingEntity, null, itemActionDataRanged.invData.item.ItemTags, true, true, true, true, true, 1, true, false) > 0f)
		{
			itemActionDataRanged.m_LastShotTime = Time.time + 1f;
			if (flag)
			{
				Manager.PlayInsidePlayerHead("twitch_no_attack", -1, 0f, false, false);
			}
			return;
		}
		if (holdingEntity.isHeadUnderwater && !this.IsAmmoUsableUnderwater(holdingEntity))
		{
			if (flag)
			{
				GameManager.ShowTooltip(holdingEntity as EntityPlayerLocal, "ttCannotUseAtThisTime", false);
			}
			return;
		}
		if (itemActionDataRanged.invData.itemValue.PercentUsesLeft <= 0f)
		{
			if (flag)
			{
				EntityPlayerLocal player = holdingEntity as EntityPlayerLocal;
				if (this.item.Properties.Values.ContainsKey(ItemClass.PropSoundJammed))
				{
					Manager.PlayInsidePlayerHead(this.item.Properties.Values[ItemClass.PropSoundJammed], -1, 0f, false, false);
				}
				GameManager.ShowTooltip(player, "ttItemNeedsRepair", false);
			}
			return;
		}
		itemActionDataRanged.invData.holdingEntity.MinEventContext.ItemValue = itemActionDataRanged.invData.holdingEntity.inventory.holdingItemItemValue;
		itemActionDataRanged.invData.holdingEntity.MinEventContext.ItemActionData = itemActionDataRanged.invData.actionData[0];
		ItemActionRanged.ItemActionDataRanged itemActionDataRanged2 = itemActionDataRanged;
		itemActionDataRanged2.curBurstCount += 1;
		if (!this.checkAmmo(itemActionDataRanged))
		{
			if (bReleased)
			{
				holdingEntity.PlayOneShot(this.soundEmpty, false, false, false);
				if (itemActionDataRanged.state != ItemActionFiringState.Off)
				{
					itemActionDataRanged.invData.gameManager.ItemActionEffectsServer(itemActionDataRanged.invData.holdingEntity.entityId, itemActionDataRanged.invData.slotIdx, itemActionDataRanged.indexInEntityOfAction, 0, Vector3.zero, Vector3.zero, 0);
				}
				itemActionDataRanged.state = ItemActionFiringState.Off;
				if (this.CanReload(itemActionDataRanged))
				{
					itemActionDataRanged.invData.gameManager.ItemReloadServer(itemActionDataRanged.invData.holdingEntity.entityId);
					itemActionDataRanged.invData.holdingEntitySoundID = -2;
				}
			}
			return;
		}
		itemActionDataRanged.burstShotStarted = true;
		itemActionDataRanged.invData.holdingEntity.FireEvent(MinEventTypes.onSelfRangedBurstShotEnd, true);
		itemActionDataRanged.invData.holdingEntity.FireEvent(MinEventTypes.onSelfRangedBurstShotStart, true);
		if (itemActionDataRanged.state == ItemActionFiringState.Off)
		{
			itemActionDataRanged.state = ItemActionFiringState.Start;
		}
		else
		{
			itemActionDataRanged.state = ItemActionFiringState.Loop;
		}
		if (!this.InfiniteAmmo)
		{
			this.ConsumeAmmo(_actionData);
		}
		int modelLayer = holdingEntity.GetModelLayer();
		holdingEntity.SetModelLayer(2, false, null);
		Vector3 shotDirection = Vector3.zero;
		int num = (int)EffectManager.GetValue(PassiveEffects.RoundRayCount, itemActionDataRanged.invData.itemValue, 1f, itemActionDataRanged.invData.holdingEntity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
		bool flag4 = false;
		for (int i = 0; i < num; i++)
		{
			bool flag5 = false;
			shotDirection = this.fireShot(i, itemActionDataRanged, ref flag5);
			if (flag5)
			{
				flag4 = true;
			}
		}
		if (!flag4 && holdingEntity != null)
		{
			holdingEntity.FireEvent((_actionData.indexInEntityOfAction == 0) ? MinEventTypes.onSelfPrimaryActionMissEntity : MinEventTypes.onSelfSecondaryActionMissEntity, true);
		}
		holdingEntity.SetModelLayer(modelLayer, false, null);
		Vector3 startPos;
		Vector3 direction;
		this.getImageActionEffectsStartPosAndDirection(_actionData, out startPos, out direction);
		itemActionDataRanged.invData.gameManager.ItemActionEffectsServer(holdingEntity.entityId, itemActionDataRanged.invData.slotIdx, itemActionDataRanged.indexInEntityOfAction, (int)itemActionDataRanged.state, startPos, direction, this.getUserData(_actionData));
		if (itemActionDataRanged.invData.itemValue.MaxUseTimes > 0)
		{
			_actionData.invData.itemValue.UseTimes += EffectManager.GetValue(PassiveEffects.DegradationPerUse, itemActionDataRanged.invData.itemValue, 1f, itemActionDataRanged.invData.holdingEntity, null, _actionData.invData.itemValue.ItemClass.ItemTags, true, true, true, true, true, 1, true, false);
			if (itemActionDataRanged.invData.itemValue.PercentUsesLeft == 0f)
			{
				itemActionDataRanged.state = ItemActionFiringState.Off;
			}
		}
		if (this.GetMaxAmmoCount(itemActionDataRanged) == 1 && itemActionDataRanged.invData.itemValue.Meta == 0)
		{
			if (itemActionDataRanged.state != ItemActionFiringState.Off)
			{
				itemActionDataRanged.invData.gameManager.ItemActionEffectsServer(holdingEntity.entityId, itemActionDataRanged.invData.slotIdx, itemActionDataRanged.indexInEntityOfAction, 0, Vector3.zero, Vector3.zero, 0);
			}
			itemActionDataRanged.state = ItemActionFiringState.Off;
			this.item.StopHoldingAudio(itemActionDataRanged.invData);
			if (this.AutoReload && this.CanReload(itemActionDataRanged))
			{
				itemActionDataRanged.invData.gameManager.ItemReloadServer(holdingEntity.entityId);
			}
		}
		Vector3 kickbackForce = base.GetKickbackForce(shotDirection);
		holdingEntity.motion += kickbackForce * (holdingEntity.AimingGun ? 0.2f : 0.5f);
		holdingEntity.inventory.CallOnToolbeltChangedInternal();
		base.HandleItemBreak(_actionData);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool checkAmmo(ItemActionData _actionData)
	{
		return this.InfiniteAmmo || _actionData.invData.itemValue.Meta > 0;
	}

	public bool HasInfiniteAmmo(ItemActionData _actionData)
	{
		return EffectManager.GetValue(PassiveEffects.InfiniteAmmo, _actionData.invData.itemValue, 0f, _actionData.invData.holdingEntity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false) > 0f;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual float updateAccuracy(ItemActionData _actionData, bool _isAimingGun)
	{
		EntityAlive holdingEntity = _actionData.invData.holdingEntity;
		ItemValue itemValue = _actionData.invData.itemValue;
		float num;
		if (_isAimingGun)
		{
			num = EffectManager.GetValue(PassiveEffects.SpreadMultiplierAiming, itemValue, 0.1f, holdingEntity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
		}
		else
		{
			num = EffectManager.GetValue(PassiveEffects.SpreadMultiplierHip, itemValue, 1f, holdingEntity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
		}
		if (holdingEntity.moveDirection == Vector3.zero)
		{
			num *= EffectManager.GetValue(PassiveEffects.SpreadMultiplierIdle, itemValue, 0.1f, holdingEntity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
		}
		else if (!holdingEntity.MovementRunning)
		{
			num *= EffectManager.GetValue(PassiveEffects.SpreadMultiplierWalking, itemValue, 1f, holdingEntity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
		}
		else
		{
			num *= EffectManager.GetValue(PassiveEffects.SpreadMultiplierRunning, itemValue, 1f, holdingEntity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
		}
		if (holdingEntity.IsCrouching)
		{
			num *= EffectManager.GetValue(PassiveEffects.SpreadMultiplierCrouching, itemValue, 1f, holdingEntity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
		}
		ItemActionRanged.ItemActionDataRanged itemActionDataRanged = (ItemActionRanged.ItemActionDataRanged)_actionData;
		float num2 = Mathf.Clamp01(EffectManager.GetValue(PassiveEffects.WeaponHandling, itemValue, 0.1f, holdingEntity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false));
		itemActionDataRanged.lastAccuracy = Mathf.Lerp(itemActionDataRanged.lastAccuracy, num, Time.deltaTime * num2 * 15f);
		return itemActionDataRanged.lastAccuracy;
	}

	public virtual float GetRange(ItemActionData _actionData)
	{
		return EffectManager.GetValue(PassiveEffects.MaxRange, _actionData.invData.itemValue, this.Range, _actionData.invData.holdingEntity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
	}

	public virtual int GetMaxAmmoCount(ItemActionData _actionData)
	{
		return (int)EffectManager.GetValue(PassiveEffects.MagazineSize, _actionData.invData.itemValue, (float)this.BulletsPerMagazine, _actionData.invData.holdingEntity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
	}

	public virtual int GetBurstCount(ItemActionData _actionData)
	{
		return (int)EffectManager.GetValue(PassiveEffects.BurstRoundCount, _actionData.invData.itemValue, 1f, _actionData.invData.holdingEntity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual Vector3 getDirectionOffset(ItemActionRanged.ItemActionDataRanged _actionData, Vector3 _forward, int _shotOffset = 0)
	{
		float num = EffectManager.GetValue(PassiveEffects.SpreadDegreesHorizontal, _actionData.invData.itemValue, 45f, _actionData.invData.holdingEntity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
		num *= _actionData.lastAccuracy;
		num *= (float)_actionData.MeanderNoise.Noise((double)Time.time, 0.0, (double)_shotOffset) * 0.66f;
		float x = EffectManager.GetValue(PassiveEffects.SpreadDegreesVertical, _actionData.invData.itemValue, 45f, _actionData.invData.holdingEntity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false) * _actionData.lastAccuracy * ((float)_actionData.MeanderNoise.Noise(0.0, (double)Time.time, (double)_shotOffset) * 0.66f) + this.spreadVerticalOffset;
		Quaternion rotation = Quaternion.LookRotation(_forward, Vector3.up);
		Vector3 point = Quaternion.Euler(x, num, 0f) * Vector3.forward;
		return rotation * point;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual Vector3 getDirectionRandomOffset(ItemActionRanged.ItemActionDataRanged _actionData, Vector3 _forward)
	{
		float num = EffectManager.GetValue(PassiveEffects.SpreadDegreesHorizontal, _actionData.invData.itemValue, 45f, _actionData.invData.holdingEntity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
		num *= _actionData.lastAccuracy;
		num *= _actionData.rand.RandomFloat * 2f - 1f;
		float x = EffectManager.GetValue(PassiveEffects.SpreadDegreesVertical, _actionData.invData.itemValue, 45f, _actionData.invData.holdingEntity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false) * _actionData.lastAccuracy * (_actionData.rand.RandomFloat * 2f - 1f) + this.spreadVerticalOffset;
		Quaternion rotation = Quaternion.LookRotation(_forward, Vector3.up);
		Vector3 point = Quaternion.Euler(x, num, 0f) * Vector3.forward;
		return rotation * point;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual Vector3 fireShot(int _shotIdx, ItemActionRanged.ItemActionDataRanged _actionData, ref bool hitEntityFound)
	{
		EntityAlive holdingEntity = _actionData.invData.holdingEntity;
		ItemValue itemValue = _actionData.invData.itemValue;
		float range = this.GetRange(_actionData);
		Ray lookRay = holdingEntity.GetLookRay();
		lookRay.direction = this.getDirectionOffset(_actionData, lookRay.direction, _shotIdx);
		_actionData.waterCollisionParticles.Reset();
		_actionData.waterCollisionParticles.CheckCollision(lookRay.origin, lookRay.direction, range, holdingEntity.entityId);
		int hitMask = (this.hitmaskOverride == 0) ? 8 : this.hitmaskOverride;
		int num = Mathf.FloorToInt(EffectManager.GetValue(PassiveEffects.EntityPenetrationCount, itemValue, (float)this.EntityPenetrationCount, holdingEntity, null, itemValue.ItemClass.ItemTags, true, true, true, true, true, 1, true, false));
		num++;
		int num2 = Mathf.FloorToInt(EffectManager.GetValue(PassiveEffects.BlockPenetrationFactor, itemValue, (float)this.BlockPenetrationFactor, holdingEntity, null, itemValue.ItemClass.ItemTags, true, true, true, true, true, 1, true, false));
		EntityAlive x = null;
		hitEntityFound = false;
		for (int i = 0; i < num; i++)
		{
			if (Voxel.Raycast(_actionData.invData.world, lookRay, range, -538750997, hitMask, 0f))
			{
				WorldRayHitInfo worldRayHitInfo = Voxel.voxelRayHitInfo.Clone();
				if (worldRayHitInfo.hit.distanceSq > range * range)
				{
					return lookRay.direction;
				}
				lookRay.origin = worldRayHitInfo.hit.pos;
				if (worldRayHitInfo.tag.StartsWith("E_"))
				{
					EntityDrone component = worldRayHitInfo.transform.GetComponent<EntityDrone>();
					if (component && component.isAlly(holdingEntity as EntityPlayer))
					{
						lookRay.origin = worldRayHitInfo.hit.pos + lookRay.direction * 0.1f;
						i--;
						goto IL_3C5;
					}
					string text;
					EntityAlive entityAlive = ItemActionAttack.FindHitEntityNoTagCheck(worldRayHitInfo, out text) as EntityAlive;
					if (x == entityAlive)
					{
						lookRay.origin = worldRayHitInfo.hit.pos + lookRay.direction * 0.1f;
						i--;
						goto IL_3C5;
					}
					holdingEntity.MinEventContext.Other = entityAlive;
					x = entityAlive;
					hitEntityFound = true;
				}
				else
				{
					BlockValue blockHit = ItemActionAttack.GetBlockHit(_actionData.invData.world, worldRayHitInfo);
					i += Mathf.FloorToInt((float)blockHit.Block.MaxDamage / (float)num2);
					holdingEntity.MinEventContext.BlockValue = blockHit;
				}
				float num3 = 1f;
				float value = EffectManager.GetValue(PassiveEffects.DamageFalloffRange, itemValue, range, holdingEntity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
				if (worldRayHitInfo.hit.distanceSq > value * value)
				{
					num3 = 1f - (worldRayHitInfo.hit.distanceSq - value * value) / (range * range - value * value);
				}
				_actionData.attackDetails.isCriticalHit = holdingEntity.AimingGun;
				_actionData.attackDetails.WeaponTypeTag = ItemActionAttack.RangedTag;
				holdingEntity.FireEvent((_actionData.indexInEntityOfAction == 0) ? MinEventTypes.onSelfPrimaryActionRayHit : MinEventTypes.onSelfSecondaryActionRayHit, true);
				ItemActionAttack.Hit(worldRayHitInfo, holdingEntity.entityId, (this.DamageType == EnumDamageTypes.None) ? EnumDamageTypes.Piercing : this.DamageType, base.GetDamageBlock(itemValue, ItemActionAttack.GetBlockHit(_actionData.invData.world, worldRayHitInfo), holdingEntity, 0) * num3, base.GetDamageEntity(itemValue, holdingEntity, 0) * num3, 1f, itemValue.PercentUsesLeft, _actionData.invData.item.CritChance.Value, ItemAction.GetDismemberChance(_actionData, worldRayHitInfo), this.bulletMaterialName, this.damageMultiplier, this.getBuffActions(_actionData), _actionData.attackDetails, 0, this.ActionExp, this.ActionExpBonusMultiplier, null, this.ToolBonuses, this.bSupportHarvesting ? ItemActionAttack.EnumAttackMode.RealAndHarvesting : ItemActionAttack.EnumAttackMode.RealNoHarvesting, null, -1, null);
				if (this.bSupportHarvesting)
				{
					GameUtils.HarvestOnAttack(_actionData, this.ToolBonuses);
				}
			}
			else
			{
				holdingEntity.FireEvent((_actionData.indexInEntityOfAction == 0) ? MinEventTypes.onSelfPrimaryActionRayMiss : MinEventTypes.onSelfSecondaryActionRayMiss, true);
			}
			IL_3C5:;
		}
		return lookRay.direction;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public float getDamageBlock(ItemActionRanged.ItemActionDataRanged _actionData)
	{
		return base.GetDamageBlock(_actionData.invData.itemValue, BlockValue.Air, _actionData.invData.holdingEntity, _actionData.indexInEntityOfAction);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public float getDamageEntity(ItemActionRanged.ItemActionDataRanged _actionData)
	{
		return base.GetDamageEntity(_actionData.invData.itemValue, _actionData.invData.holdingEntity, _actionData.indexInEntityOfAction);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void getImageActionEffectsStartPosAndDirection(ItemActionData _actionData, out Vector3 _startPos, out Vector3 _direction)
	{
		_startPos = Vector3.zero;
		_direction = Vector3.zero;
	}

	public override int GetInitialMeta(ItemValue _itemValue)
	{
		return (int)EffectManager.GetValue(PassiveEffects.MagazineSize, _itemValue, (float)this.BulletsPerMagazine, null, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
	}

	public override WorldRayHitInfo GetExecuteActionTarget(ItemActionData _actionData)
	{
		ItemActionRanged.ItemActionDataRanged itemActionDataRanged = (ItemActionRanged.ItemActionDataRanged)_actionData;
		Ray lookRay = _actionData.invData.holdingEntity.GetLookRay();
		lookRay.direction = this.getDirectionOffset(itemActionDataRanged, lookRay.direction, 0);
		float range = this.GetRange(_actionData);
		itemActionDataRanged.distance = range;
		int modelLayer = _actionData.invData.holdingEntity.GetModelLayer();
		_actionData.invData.holdingEntity.SetModelLayer(2, false, null);
		int hitMask = (this.hitmaskOverride == 0) ? 8 : this.hitmaskOverride;
		bool flag = Voxel.Raycast(_actionData.invData.world, lookRay, range, -538750997, hitMask, 0f);
		_actionData.invData.holdingEntity.SetModelLayer(modelLayer, false, null);
		if (flag)
		{
			WorldRayHitInfo updatedHitInfo = _actionData.GetUpdatedHitInfo();
			itemActionDataRanged.distance = Mathf.Sqrt(updatedHitInfo.hit.distanceSq);
			itemActionDataRanged.damageFalloffPercent = 1f;
			if (itemActionDataRanged.Laser != null)
			{
				itemActionDataRanged.Laser.position = updatedHitInfo.hit.pos - Origin.position;
			}
			float value = EffectManager.GetValue(PassiveEffects.DamageFalloffRange, _actionData.invData.itemValue, range, _actionData.invData.holdingEntity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
			if (updatedHitInfo.hit.distanceSq > value * value)
			{
				itemActionDataRanged.damageFalloffPercent = 1f - (updatedHitInfo.hit.distanceSq - value * value) / (range * range - value * value);
			}
			return updatedHitInfo;
		}
		if (itemActionDataRanged.Laser != null)
		{
			itemActionDataRanged.Laser.position = new Vector3(lookRay.origin.x, lookRay.origin.y, lookRay.origin.z + itemActionDataRanged.distance);
		}
		return null;
	}

	public override void GetItemValueActionInfo(ref List<string> _infoList, ItemValue _itemValue, XUi _xui, int _actionIndex = 0)
	{
		base.GetItemValueActionInfo(ref _infoList, _itemValue, _xui, 0);
		_infoList.Add(ItemAction.StringFormatHandler(Localization.Get("lblHandling", false), EffectManager.GetValue(PassiveEffects.WeaponHandling, _itemValue, 0.1f, _xui.playerUI.entityPlayer, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false)));
		_infoList.Add(ItemAction.StringFormatHandler(Localization.Get("lblRPM", false), EffectManager.GetValue(PassiveEffects.RoundsPerMinute, _itemValue, 60f / this.originalDelay, _xui.playerUI.entityPlayer, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false)));
		_infoList.Add(ItemAction.StringFormatHandler(Localization.Get("lblAttributeFalloffRange", false), string.Format("{0} / {1} {2}", EffectManager.GetValue(PassiveEffects.DamageFalloffRange, _itemValue, 0f, _xui.playerUI.entityPlayer, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false).ToCultureInvariantString(), EffectManager.GetValue(PassiveEffects.MaxRange, _itemValue, this.Range, _xui.playerUI.entityPlayer, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false).ToCultureInvariantString(), Localization.Get("lblAttributeFalloffRangeText", false))));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string bulletMaterialName;

	public bool bSupportHarvesting;

	public bool bUseMeleeCrosshair;

	[PublicizedFrom(EAccessModifier.Private)]
	public float originalDelay = -1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public string triggerEffectShootDualsense;

	[PublicizedFrom(EAccessModifier.Private)]
	public string triggerEffectTriggerPullDualsense;

	[PublicizedFrom(EAccessModifier.Private)]
	public string triggerEffectShootXbox;

	[PublicizedFrom(EAccessModifier.Private)]
	public string triggerEffectTriggerPullXbox;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool rapidTrigger;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool AutoReload = true;

	public int EntityPenetrationCount;

	public int BlockPenetrationFactor = 251;

	[PublicizedFrom(EAccessModifier.Private)]
	public float spreadVerticalOffset;

	public class ItemActionDataRanged : ItemActionAttackData
	{
		public ItemActionDataRanged(ItemInventoryData _invData, int _indexInEntityOfAction) : base(_invData, _indexInEntityOfAction)
		{
			this.IsDoubleBarrel = _invData.item.ItemTags.Test_Bit(FastTags<TagGroup.Global>.GetBit("dBarrel"));
			if (this.IsDoubleBarrel)
			{
				this.muzzle2 = ItemActionRanged.ItemActionDataRanged.getModelChildTransformByName(_invData, "Muzzle_R");
				this.muzzle = ItemActionRanged.ItemActionDataRanged.getModelChildTransformByName(_invData, "Muzzle_L");
			}
			else
			{
				this.muzzle = ItemActionRanged.ItemActionDataRanged.getModelChildTransformByName(_invData, "Muzzle");
			}
			this.Laser = ItemActionRanged.ItemActionDataRanged.getModelChildTransformByName(_invData, "laser");
			this.ScopeTransform = ItemActionRanged.ItemActionDataRanged.getModelChildTransformByName(_invData, "Attachments/Scope");
			this.SideTransform = ItemActionRanged.ItemActionDataRanged.getModelChildTransformByName(_invData, "Attachments/Side");
			this.BarrelTransform = ItemActionRanged.ItemActionDataRanged.getModelChildTransformByName(_invData, "Attachments/Barrel");
			this.hasScopeMod = (this.ScopeTransform != null && this.ScopeTransform.childCount > 0);
			this.hasSideMod = (this.SideTransform != null && this.SideTransform.childCount > 0);
			this.hasBarrelMod = (this.BarrelTransform != null && this.BarrelTransform.childCount > 0);
			Transform modelChildTransformByName = ItemActionRanged.ItemActionDataRanged.getModelChildTransformByName(_invData, "ironsight");
			if (modelChildTransformByName == null)
			{
				modelChildTransformByName = ItemActionRanged.ItemActionDataRanged.getModelChildTransformByName(_invData, "ironsights");
			}
			if (modelChildTransformByName != null)
			{
				modelChildTransformByName.gameObject.SetActive(!this.hasScopeMod);
			}
			Transform modelChildTransformByName2 = ItemActionRanged.ItemActionDataRanged.getModelChildTransformByName(_invData, "scope_rail");
			if (modelChildTransformByName2 != null)
			{
				modelChildTransformByName2.gameObject.SetActive(this.hasScopeMod);
			}
			Transform modelChildTransformByName3 = ItemActionRanged.ItemActionDataRanged.getModelChildTransformByName(_invData, "side_rail");
			if (modelChildTransformByName3 != null)
			{
				modelChildTransformByName3.gameObject.SetActive(this.hasSideMod);
			}
			Transform modelChildTransformByName4 = ItemActionRanged.ItemActionDataRanged.getModelChildTransformByName(_invData, "barrel_rail");
			if (modelChildTransformByName4 != null)
			{
				modelChildTransformByName4.gameObject.SetActive(this.hasBarrelMod);
			}
			this.m_LastShotTime = -1f;
			this.MeanderNoise = new PerlinNoise(_invData.holdingEntity.entityId + _invData.item.Id);
			this.rand = _invData.holdingEntity.rand;
			this.waterCollisionParticles = new CollisionParticleController();
			this.waterCollisionParticles.Init(_invData.holdingEntity.entityId, _invData.item.MadeOfMaterial.SurfaceCategory, "water", 16);
		}

		public static Transform getModelChildTransformByName(ItemInventoryData _invData, string _name)
		{
			if (_invData.model == null)
			{
				return null;
			}
			if (_name.Contains("/"))
			{
				return _invData.model.Find(_name);
			}
			return _invData.model.FindInChilds(_name, false);
		}

		public float m_LastShotTime;

		public int reloadAmount;

		public bool IsDoubleBarrel;

		public Transform muzzle;

		public Transform muzzle2;

		public Transform Laser;

		public ItemActionFiringState state;

		public float lastTimeTriggerPressed;

		public Vector3i currentDiggingLocation;

		public float curBlockDamagePerHit;

		public float curBlockDamage;

		public bool bReleased;

		public bool bPressed;

		public GameRandom rand;

		public PerlinNoise MeanderNoise;

		public byte curBurstCount;

		public float lastAccuracy;

		public float distance;

		public float damageFalloffPercent;

		public bool isReloading;

		public bool isWeaponReloading;

		public bool isReloadCancelled;

		public bool isWeaponReloadCancelled;

		public bool wasAiming;

		public bool isChangingAmmoType;

		public Transform ScopeTransform;

		public Transform SideTransform;

		public Transform BarrelTransform;

		public bool hasScopeMod;

		public bool hasSideMod;

		public bool hasBarrelMod;

		public bool IsFlashSuppressed;

		public Vector3 ScopeTransformOffset;

		public Vector3 SideTransformOffset;

		public Vector3 BarrelTransformOffset;

		public Vector3 ScopeTransformScale = Vector3.one;

		public Vector3 SideTransformScale = Vector3.one;

		public Vector3 BarrelTransformScale = Vector3.one;

		public Vector3 ScopeTransformRotation;

		public Vector3 SideTransformRotation;

		public Vector3 BarrelTransformRotation;

		public string SoundStart;

		public string SoundLoop;

		public string SoundEnd;

		public float Delay;

		public float OriginalDelay = -1f;

		public bool burstShotStarted;

		public CollisionParticleController waterCollisionParticles = new CollisionParticleController();
	}
}

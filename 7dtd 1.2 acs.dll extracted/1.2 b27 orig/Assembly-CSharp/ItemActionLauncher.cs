using System;
using System.Collections.Generic;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionLauncher : ItemActionRanged
{
	public override ItemActionData CreateModifierData(ItemInventoryData _invData, int _indexInEntityOfAction)
	{
		return new ItemActionLauncher.ItemActionDataLauncher(_invData, _indexInEntityOfAction);
	}

	public override void ReadFrom(DynamicProperties _props)
	{
		base.ReadFrom(_props);
	}

	public override void StartHolding(ItemActionData _actionData)
	{
		base.StartHolding(_actionData);
		this.DeleteProjectiles(_actionData);
		ItemActionLauncher.ItemActionDataLauncher itemActionDataLauncher = (ItemActionLauncher.ItemActionDataLauncher)_actionData;
		if (_actionData.invData.itemValue.Meta != 0 && this.GetMaxAmmoCount(itemActionDataLauncher) != 0)
		{
			for (int i = 0; i < _actionData.invData.itemValue.Meta; i++)
			{
				itemActionDataLauncher.projectileInstance.Add(this.instantiateProjectile(_actionData, default(Vector3)));
			}
		}
	}

	public override void StopHolding(ItemActionData _data)
	{
		base.StopHolding(_data);
		this.DeleteProjectiles(_data);
	}

	public void DeleteProjectiles(ItemActionData _actionData)
	{
		ItemActionLauncher.ItemActionDataLauncher itemActionDataLauncher = (ItemActionLauncher.ItemActionDataLauncher)_actionData;
		for (int i = 0; i < itemActionDataLauncher.projectileInstance.Count; i++)
		{
			Transform transform = itemActionDataLauncher.projectileInstance[i];
			if (transform != null)
			{
				UnityEngine.Object.Destroy(transform.gameObject);
			}
		}
		itemActionDataLauncher.projectileInstance.Clear();
	}

	public override void ReloadGun(ItemActionData _actionData)
	{
		Manager.StopSequence(_actionData.invData.holdingEntity, ((ItemActionRanged.ItemActionDataRanged)_actionData).SoundStart);
		if (!_actionData.invData.holdingEntity.isEntityRemote)
		{
			_actionData.invData.holdingEntity.OnReloadStart();
		}
	}

	public override void CancelReload(ItemActionData _actionData)
	{
		base.CancelReload(_actionData);
		ItemActionLauncher.ItemActionDataLauncher actionData = (ItemActionLauncher.ItemActionDataLauncher)_actionData;
		this.ClampAmmoCount(actionData);
	}

	public override void SwapAmmoType(EntityAlive _entity, int _selectedIndex = -1)
	{
		ItemActionLauncher.ItemActionDataLauncher actionData = (ItemActionLauncher.ItemActionDataLauncher)_entity.inventory.holdingItemData.actionData[0];
		this.ClampAmmoCount(actionData);
		base.SwapAmmoType(_entity, _selectedIndex);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ClampAmmoCount(ItemActionLauncher.ItemActionDataLauncher actionData)
	{
		int maxAmmoCount = this.GetMaxAmmoCount(actionData);
		int num = actionData.projectileInstance.Count - maxAmmoCount;
		if (num > 0)
		{
			for (int i = maxAmmoCount; i < actionData.projectileInstance.Count; i++)
			{
				if (actionData.projectileInstance[i] != null)
				{
					UnityEngine.Object.Destroy(actionData.projectileInstance[i].gameObject);
				}
			}
			actionData.projectileInstance.RemoveRange(maxAmmoCount, num);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override Vector3 fireShot(int _shotIdx, ItemActionRanged.ItemActionDataRanged _actionData, ref bool hitEntity)
	{
		hitEntity = true;
		return Vector3.zero;
	}

	public Transform instantiateProjectile(ItemActionData _actionData, Vector3 _positionOffset = default(Vector3))
	{
		ItemValue holdingItemItemValue = _actionData.invData.holdingEntity.inventory.holdingItemItemValue;
		ItemValue item = ItemClass.GetItem(this.MagazineItemNames[(int)holdingItemItemValue.SelectedAmmoTypeIndex], false);
		this.LastProjectileType = item;
		ItemClass forId = ItemClass.GetForId(item.type);
		if (forId == null)
		{
			return null;
		}
		ItemActionLauncher.ItemActionDataLauncher itemActionDataLauncher = (ItemActionLauncher.ItemActionDataLauncher)_actionData;
		int entityId = _actionData.invData.holdingEntity.entityId;
		ItemValue itemValue = new ItemValue(forId.Id, false);
		Transform transform = forId.CloneModel(_actionData.invData.world, itemValue, Vector3.zero, null, BlockShape.MeshPurpose.World, 0L);
		Transform transform2 = itemActionDataLauncher.projectileJoint;
		if (transform2 == null)
		{
			transform2 = ((itemActionDataLauncher.invData.holdingEntity.emodel.avatarController != null) ? itemActionDataLauncher.invData.holdingEntity.emodel.GetRightHandTransform() : null);
		}
		if (transform2 != null)
		{
			transform.parent = transform2;
			transform.localPosition = _positionOffset;
			transform.localRotation = Quaternion.identity;
		}
		else
		{
			transform.parent = null;
		}
		Utils.SetLayerRecursively(transform.gameObject, (transform2 != null) ? transform2.gameObject.layer : 0, null);
		ProjectileMoveScript projectileMoveScript = transform.gameObject.AddComponent<ProjectileMoveScript>();
		projectileMoveScript.itemProjectile = forId;
		projectileMoveScript.itemValueProjectile = itemValue;
		projectileMoveScript.itemValueLauncher = _actionData.invData.holdingEntity.inventory.holdingItemItemValue;
		projectileMoveScript.itemActionProjectile = (ItemActionProjectile)((forId.Actions[0] is ItemActionProjectile) ? forId.Actions[0] : forId.Actions[1]);
		projectileMoveScript.ProjectileOwnerID = entityId;
		projectileMoveScript.actionData = itemActionDataLauncher;
		transform.gameObject.SetActive(true);
		return transform;
	}

	public override void ItemActionEffects(GameManager _gameManager, ItemActionData _actionData, int _firingState, Vector3 _startPos, Vector3 _direction, int _userData = 0)
	{
		base.ItemActionEffects(_gameManager, _actionData, _firingState, _startPos, _direction, _userData);
		if (_firingState == 0)
		{
			return;
		}
		ItemActionLauncher.ItemActionDataLauncher itemActionDataLauncher = (ItemActionLauncher.ItemActionDataLauncher)_actionData;
		int num = this.GetBurstCount(_actionData);
		if (num <= 0)
		{
			return;
		}
		for (int i = itemActionDataLauncher.projectileInstance.Count - 1; i >= 0; i--)
		{
			Transform transform = itemActionDataLauncher.projectileInstance[i];
			if (transform != null)
			{
				transform.GetComponent<ProjectileMoveScript>().Fire(_startPos, this.getDirectionOffset(itemActionDataLauncher, _direction, i), _actionData.invData.holdingEntity, this.hitmaskOverride, 0f);
			}
			itemActionDataLauncher.projectileInstance.RemoveAt(i);
			if (--num <= 0)
			{
				break;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void ConsumeAmmo(ItemActionData _actionData)
	{
		_actionData.invData.itemValue.Meta -= this.GetBurstCount(_actionData);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void getImageActionEffectsStartPosAndDirection(ItemActionData _actionData, out Vector3 _startPos, out Vector3 _direction)
	{
		ItemActionLauncher.ItemActionDataLauncher itemActionDataLauncher = (ItemActionLauncher.ItemActionDataLauncher)_actionData;
		Ray lookRay = itemActionDataLauncher.invData.holdingEntity.GetLookRay();
		_startPos = lookRay.origin;
		_direction = lookRay.direction;
		_direction = this.getDirectionOffset(itemActionDataLauncher, _direction, 0);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public new bool InstantiateOnLoad = true;

	public ItemValue LastProjectileType;

	public class ItemActionDataLauncher : ItemActionRanged.ItemActionDataRanged
	{
		public ItemActionDataLauncher(ItemInventoryData _invData, int _indexInEntityOfAction) : base(_invData, _indexInEntityOfAction)
		{
			this.projectileJoint = ((_invData.model != null) ? _invData.model.FindInChilds("ProjectileJoint", false) : null);
			this.projectileInstance = new List<Transform>();
		}

		public Transform projectileJoint;

		public List<Transform> projectileInstance;

		public float strainPercent = 1f;
	}
}

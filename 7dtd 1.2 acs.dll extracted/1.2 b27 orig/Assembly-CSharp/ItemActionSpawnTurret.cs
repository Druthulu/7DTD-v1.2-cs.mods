using System;
using System.Collections.Generic;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionSpawnTurret : ItemAction
{
	public override ItemActionData CreateModifierData(ItemInventoryData _invData, int _indexInEntityOfAction)
	{
		return new ItemActionSpawnTurret.ItemActionDataSpawnTurret(_invData, _indexInEntityOfAction);
	}

	public override void ReadFrom(DynamicProperties _props)
	{
		base.ReadFrom(_props);
		if (_props.Values.ContainsKey("Turret"))
		{
			this.entityToSpawn = _props.Values["Turret"];
		}
		this.turretSize = new Vector3(0.5f, 0.5f, 0.5f);
		if (_props.Values.ContainsKey("Scale"))
		{
			this.turretSize = StringParsers.ParseVector3(_props.Values["Scale"], 0, -1);
		}
		this.previewSize = new Vector3(1f, 1f, 1f);
		if (_props.Values.ContainsKey("PreviewSize"))
		{
			this.previewSize = StringParsers.ParseVector3(_props.Values["PreviewSize"], 0, -1);
		}
		foreach (KeyValuePair<int, EntityClass> keyValuePair in EntityClass.list.Dict)
		{
			if (keyValuePair.Value.entityClassName == this.entityToSpawn)
			{
				this.entityClassId = keyValuePair.Key;
				break;
			}
		}
	}

	public override void StartHolding(ItemActionData _actionData)
	{
		ItemActionSpawnTurret.ItemActionDataSpawnTurret itemActionDataSpawnTurret = (ItemActionSpawnTurret.ItemActionDataSpawnTurret)_actionData;
		EntityPlayerLocal entityPlayerLocal = itemActionDataSpawnTurret.invData.holdingEntity as EntityPlayerLocal;
		if (!entityPlayerLocal)
		{
			return;
		}
		if (itemActionDataSpawnTurret.TurretPreviewT != null)
		{
			UnityEngine.Object.DestroyImmediate(itemActionDataSpawnTurret.TurretPreviewT.gameObject);
		}
		GameObject original = DataLoader.LoadAsset<GameObject>(entityPlayerLocal.inventory.holdingItem.MeshFile);
		itemActionDataSpawnTurret.TurretPreviewT = UnityEngine.Object.Instantiate<GameObject>(original).transform;
		this.setupPreview(itemActionDataSpawnTurret);
		this.updatePreview(itemActionDataSpawnTurret);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setupPreview(ItemActionSpawnTurret.ItemActionDataSpawnTurret data)
	{
		if (data.PreviewRenderers == null || data.PreviewRenderers.Length == 0 || data.PreviewRenderers[0] == null)
		{
			data.PreviewRenderers = data.TurretPreviewT.GetComponentsInChildren<Renderer>();
		}
		data.TurretPreviewT.localScale = this.previewSize;
		data.TurretPreviewT.GetComponent<SphereCollider>().enabled = false;
		for (int i = 0; i < data.PreviewRenderers.Length; i++)
		{
			data.PreviewRenderers[i].material.color = new Color(2f, 0.25f, 0.25f);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updatePreview(ItemActionSpawnTurret.ItemActionDataSpawnTurret data)
	{
		if (data.PreviewRenderers == null || data.PreviewRenderers.Length == 0 || data.PreviewRenderers[0] == null)
		{
			data.PreviewRenderers = data.TurretPreviewT.GetComponentsInChildren<Renderer>();
		}
		World world = data.invData.world;
		bool flag = this.CalcSpawnPosition(data, ref data.Position) && world.CanPlaceBlockAt(new Vector3i(data.Position), world.GetGameManager().GetPersistentLocalPlayer(), false);
		data.ValidPosition = flag;
		for (int i = 0; i < data.PreviewRenderers.Length; i++)
		{
			data.PreviewRenderers[i].material.color = (flag ? new Color(0.25f, 2f, 0.25f) : new Color(2f, 0.25f, 0.25f));
		}
		Quaternion localRotation = data.TurretPreviewT.localRotation;
		localRotation.eulerAngles = new Vector3(0f, data.invData.holdingEntity.rotation.y, 0f);
		data.TurretPreviewT.localRotation = localRotation;
		data.TurretPreviewT.position = data.Position - Origin.position;
		data.TurretPreviewT.gameObject.SetActive(data.Placing);
	}

	public override void CancelAction(ItemActionData _actionData)
	{
		ItemActionSpawnTurret.ItemActionDataSpawnTurret itemActionDataSpawnTurret = (ItemActionSpawnTurret.ItemActionDataSpawnTurret)_actionData;
		if (itemActionDataSpawnTurret.TurretPreviewT != null && itemActionDataSpawnTurret.invData.holdingEntity is EntityPlayerLocal)
		{
			UnityEngine.Object.Destroy(itemActionDataSpawnTurret.TurretPreviewT.gameObject);
		}
	}

	public override void StopHolding(ItemActionData _actionData)
	{
		ItemActionSpawnTurret.ItemActionDataSpawnTurret itemActionDataSpawnTurret = (ItemActionSpawnTurret.ItemActionDataSpawnTurret)_actionData;
		if (itemActionDataSpawnTurret.TurretPreviewT != null && itemActionDataSpawnTurret.invData.holdingEntity is EntityPlayerLocal)
		{
			UnityEngine.Object.Destroy(itemActionDataSpawnTurret.TurretPreviewT.gameObject);
		}
	}

	public override void OnHoldingUpdate(ItemActionData _actionData)
	{
		ItemActionSpawnTurret.ItemActionDataSpawnTurret itemActionDataSpawnTurret = (ItemActionSpawnTurret.ItemActionDataSpawnTurret)_actionData;
		if (itemActionDataSpawnTurret.invData.item.Actions[0] != null && itemActionDataSpawnTurret.invData.item.Actions[0].IsActionRunning(itemActionDataSpawnTurret.invData.actionData[0]))
		{
			itemActionDataSpawnTurret.Placing = false;
		}
		if (itemActionDataSpawnTurret.TurretPreviewT != null && itemActionDataSpawnTurret.invData.holdingEntity is EntityPlayerLocal)
		{
			this.updatePreview(itemActionDataSpawnTurret);
		}
	}

	public override void ExecuteAction(ItemActionData _actionData, bool _bReleased)
	{
		ItemActionSpawnTurret.ItemActionDataSpawnTurret itemActionDataSpawnTurret = (ItemActionSpawnTurret.ItemActionDataSpawnTurret)_actionData;
		if (!(_actionData.invData.holdingEntity is EntityPlayerLocal))
		{
			return;
		}
		if (!itemActionDataSpawnTurret.Placing)
		{
			if (_bReleased)
			{
				itemActionDataSpawnTurret.Placing = true;
			}
			return;
		}
		if (!itemActionDataSpawnTurret.Placing || !_bReleased)
		{
			return;
		}
		float time = Time.time;
		if (time - _actionData.lastUseTime < this.Delay)
		{
			return;
		}
		if (time - _actionData.lastUseTime < 2f)
		{
			return;
		}
		if (!itemActionDataSpawnTurret.ValidPosition)
		{
			return;
		}
		ItemInventoryData invData = _actionData.invData;
		if (this.entityClassId < 0)
		{
			foreach (KeyValuePair<int, EntityClass> keyValuePair in EntityClass.list.Dict)
			{
				if (keyValuePair.Value.entityClassName == this.entityToSpawn)
				{
					this.entityClassId = keyValuePair.Key;
					break;
				}
			}
			if (this.entityClassId == 0)
			{
				return;
			}
		}
		if (EntityClass.list[this.entityClassId].entityClassName == "entityJunkDrone")
		{
			if (!EntityDrone.IsValidForLocalPlayer())
			{
				return;
			}
			GameManager.Instance.World.EntityLoadedDelegates += EntityDrone.OnClientSpawnRemote;
		}
		_actionData.lastUseTime = time;
		bool flag = false;
		bool flag2 = false;
		if (itemActionDataSpawnTurret.invData.item.HasAnyTags(FastTags<TagGroup.Global>.Parse("drone")))
		{
			flag2 = true;
			if (DroneManager.CanAddMoreDrones())
			{
				flag = true;
			}
		}
		else if ((itemActionDataSpawnTurret.invData.item.HasAnyTags(FastTags<TagGroup.Global>.Parse("turretRanged")) || itemActionDataSpawnTurret.invData.item.HasAnyTags(FastTags<TagGroup.Global>.Parse("turretMelee"))) && TurretTracker.CanAddMoreTurrets())
		{
			flag = true;
		}
		if (flag)
		{
			if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageTurretSpawn>().Setup(this.entityClassId, itemActionDataSpawnTurret.Position, new Vector3(0f, invData.holdingEntity.rotation.y, 0f), invData.holdingEntity.inventory.holdingItemItemValue.Clone(), invData.holdingEntity.entityId), true);
			}
			else
			{
				Entity entity = EntityFactory.CreateEntity(this.entityClassId, itemActionDataSpawnTurret.Position, new Vector3(0f, _actionData.invData.holdingEntity.rotation.y, 0f));
				entity.SetSpawnerSource(EnumSpawnerSource.StaticSpawner);
				EntityTurret entityTurret = entity as EntityTurret;
				if (entityTurret != null)
				{
					entityTurret.factionId = itemActionDataSpawnTurret.invData.holdingEntity.factionId;
					entityTurret.belongsPlayerId = itemActionDataSpawnTurret.invData.holdingEntity.entityId;
					entityTurret.factionRank = itemActionDataSpawnTurret.invData.holdingEntity.factionRank - 1;
					entityTurret.OriginalItemValue = itemActionDataSpawnTurret.invData.itemValue.Clone();
					entityTurret.groundPosition = itemActionDataSpawnTurret.Position;
					entityTurret.OwnerID = PlatformManager.InternalLocalUserIdentifier;
					entityTurret.ForceOn = true;
					entityTurret.rotation = new Vector3(0f, _actionData.invData.holdingEntity.rotation.y, 0f);
					itemActionDataSpawnTurret.invData.holdingEntity.AddOwnedEntity(entityTurret);
				}
				else
				{
					EntityDrone entityDrone = entity as EntityDrone;
					if (entityDrone != null)
					{
						entityDrone.factionId = itemActionDataSpawnTurret.invData.holdingEntity.factionId;
						entityDrone.belongsPlayerId = itemActionDataSpawnTurret.invData.holdingEntity.entityId;
						entityDrone.factionRank = itemActionDataSpawnTurret.invData.holdingEntity.factionRank - 1;
						entityDrone.OriginalItemValue = itemActionDataSpawnTurret.invData.itemValue.Clone();
						entityDrone.SetItemValueToLoad(entityDrone.OriginalItemValue);
						entityDrone.OwnerID = PlatformManager.InternalLocalUserIdentifier;
						entityDrone.PlayWakeupAnim = true;
						itemActionDataSpawnTurret.invData.holdingEntity.AddOwnedEntity(entityDrone);
					}
				}
				GameManager.Instance.World.SpawnEntityInWorld(entity);
			}
			itemActionDataSpawnTurret.Placing = false;
			invData.holdingEntity.RightArmAnimationUse = true;
			(invData.holdingEntity as EntityPlayerLocal).DropTimeDelay = 0.5f;
			if (itemActionDataSpawnTurret.TurretPreviewT != null && itemActionDataSpawnTurret.invData.holdingEntity is EntityPlayerLocal)
			{
				this.ClearPreview(itemActionDataSpawnTurret);
			}
			invData.holdingEntity.inventory.DecHoldingItem(1);
			invData.holdingEntity.PlayOneShot((this.soundStart != null) ? this.soundStart : "placeblock", false, false, false);
			return;
		}
		if (flag2)
		{
			GameManager.ShowTooltip(GameManager.Instance.World.GetPrimaryPlayer(), "uiCannotAddDrone", false);
			return;
		}
		GameManager.ShowTooltip(GameManager.Instance.World.GetPrimaryPlayer(), "uiCannotAddTurret", false);
	}

	public override bool AllowConcurrentActions()
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool CalcSpawnPosition(ItemActionSpawnTurret.ItemActionDataSpawnTurret _actionData, ref Vector3 position)
	{
		World world = _actionData.invData.world;
		Ray lookRay = _actionData.invData.holdingEntity.GetLookRay();
		if (Voxel.Raycast(world, lookRay, 4f + this.turretSize.x, 8454144, 69, 0f))
		{
			position = Voxel.voxelRayHitInfo.hit.pos;
		}
		else
		{
			position = lookRay.origin + lookRay.direction * (4f + this.turretSize.x);
		}
		Collider[] array = Physics.OverlapSphere(position - Origin.position + Vector3.up * 0.525f, 0.5f);
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].gameObject.layer != 18 && !(array[i].gameObject == _actionData.TurretPreviewT.gameObject))
			{
				return false;
			}
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool CheckForSpace(Vector3 localPos, Vector3 dirN, float length, Vector3 axis1N, float axis1Length, Vector3 axis2N, float axis2Length)
	{
		Vector3 b = dirN * length * 0.5f;
		for (float num = -axis1Length * 0.5f; num <= axis1Length * 0.5f; num += 0.2499f)
		{
			Vector3 a = localPos + axis1N * num;
			for (float num2 = -axis2Length * 0.5f; num2 <= axis2Length * 0.5f; num2 += 0.2499f)
			{
				Vector3 a2 = a + axis2N * num2;
				if (Physics.Raycast(a2 - b, dirN, length, 28901376))
				{
					return false;
				}
				if (Physics.Raycast(a2 + b, -dirN, length, 28901376))
				{
					return false;
				}
			}
		}
		return true;
	}

	public void ClearPreview(ItemActionData _data)
	{
		ItemActionSpawnTurret.ItemActionDataSpawnTurret actionData = _data as ItemActionSpawnTurret.ItemActionDataSpawnTurret;
		this.ClearPreview(actionData);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ClearPreview(ItemActionSpawnTurret.ItemActionDataSpawnTurret _actionData)
	{
		if (_actionData.TurretPreviewT)
		{
			UnityEngine.Object.Destroy(_actionData.TurretPreviewT.gameObject);
		}
	}

	public override void Cleanup(ItemActionData _data)
	{
		base.Cleanup(_data);
		ItemActionSpawnTurret.ItemActionDataSpawnTurret itemActionDataSpawnTurret = _data as ItemActionSpawnTurret.ItemActionDataSpawnTurret;
		if (itemActionDataSpawnTurret != null && itemActionDataSpawnTurret.TurretPreviewT != null && itemActionDataSpawnTurret.invData != null && itemActionDataSpawnTurret.invData.holdingEntity is EntityPlayerLocal)
		{
			this.ClearPreview(itemActionDataSpawnTurret);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cColliderMask = 28901376;

	[PublicizedFrom(EAccessModifier.Private)]
	public string entityToSpawn;

	[PublicizedFrom(EAccessModifier.Private)]
	public int entityClassId = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 turretSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 previewSize;

	[PublicizedFrom(EAccessModifier.Protected)]
	public class ItemActionDataSpawnTurret : ItemActionAttackData
	{
		public ItemActionDataSpawnTurret(ItemInventoryData _invData, int _indexInEntityOfAction) : base(_invData, _indexInEntityOfAction)
		{
		}

		public Transform TurretPreviewT;

		public Renderer[] PreviewRenderers;

		public bool ValidPosition;

		public Vector3 Position;

		public bool Placing;
	}
}

using System;
using System.Collections.Generic;
using UnityEngine;

public class ThrownWeaponMoveScript : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public void Awake()
	{
		if (ThrownWeaponMoveScript.gameManager == null)
		{
			ThrownWeaponMoveScript.gameManager = (GameManager)UnityEngine.Object.FindObjectOfType(typeof(GameManager));
		}
	}

	public void Fire(Vector3 _idealStartPosition, Vector3 _flyDirection, Entity _firingEntity, int _hmOverride = 0, float _velocity = -1f)
	{
		this.flyDirection = _flyDirection.normalized;
		this.idealPosition = _idealStartPosition;
		this.firingEntity = _firingEntity;
		this.velocity = this.flyDirection.normalized * EffectManager.GetValue(PassiveEffects.ProjectileVelocity, this.itemValueWeapon, (_velocity == -1f) ? ((float)this.itemActionThrownWeapon.Velocity) : _velocity, _firingEntity as EntityAlive, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
		this.bArmed = true;
		this.hmOverride = _hmOverride;
		this.waterCollisionParticles.Init(this.ProjectileOwnerID, this.itemWeapon.MadeOfMaterial.SurfaceCategory, "water", 16);
		if (_idealStartPosition == Vector3.zero)
		{
			this.previousPosition = base.transform.position + Origin.position;
		}
		else
		{
			this.previousPosition = _idealStartPosition;
		}
		CapsuleCollider component = base.transform.GetComponent<CapsuleCollider>();
		if (component != null)
		{
			component.enabled = false;
		}
		this.gravity = Vector3.up * EffectManager.GetValue(PassiveEffects.ProjectileGravity, this.itemValueWeapon, this.itemActionThrownWeapon.Gravity, _firingEntity as EntityAlive, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
		if (base.transform.GetComponent<OnActivateItemGameObjectReference>() != null)
		{
			base.transform.GetComponent<OnActivateItemGameObjectReference>().ActivateItem(true);
		}
		this.timeShotStarted = Time.time;
		if (base.transform.parent != null)
		{
			base.transform.parent = null;
			Utils.SetLayerRecursively(base.transform.gameObject, 0, null);
		}
		base.transform.position = _idealStartPosition - Origin.position;
		if (!base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(true);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void FixedUpdate()
	{
		if (!this.bArmed)
		{
			if (base.transform.lossyScale.x < 0.01f)
			{
				UnityEngine.Object.Destroy(base.gameObject);
				return;
			}
			if (!base.transform.gameObject.activeInHierarchy)
			{
				UnityEngine.Object.Destroy(base.gameObject);
				return;
			}
			if (!this.stuckInEntity)
			{
				base.transform.position = this.FinalPosition - Origin.position;
				return;
			}
		}
		else
		{
			if (ThrownWeaponMoveScript.gameManager == null || ThrownWeaponMoveScript.gameManager.World == null)
			{
				return;
			}
			if (Time.time - this.timeShotStarted >= this.itemActionThrownWeapon.FlyTime)
			{
				this.velocity += this.gravity * Time.fixedDeltaTime;
			}
			Vector3 vector = this.velocity * Time.fixedDeltaTime;
			if (!ThrownWeaponMoveScript.gameManager.World.IsChunkAreaCollidersLoaded(base.transform.position + vector + Origin.position))
			{
				vector = this.gravity * Time.fixedDeltaTime;
			}
			base.transform.LookAt(base.transform.position + vector);
			base.transform.Rotate(Vector3.right, 90f);
			this.checkCollision(vector);
			if (this.bArmed)
			{
				base.transform.position = base.transform.position + vector;
				if (Time.time - this.timeShotStarted >= this.itemActionThrownWeapon.LifeTime)
				{
					UnityEngine.Object.Destroy(base.gameObject);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void checkCollision(Vector3 _amountToMove)
	{
		if (ThrownWeaponMoveScript.gameManager == null || ThrownWeaponMoveScript.gameManager.World == null)
		{
			return;
		}
		Vector3 a = _amountToMove;
		float num = a.magnitude;
		if (num <= 0f)
		{
			return;
		}
		Vector3 vector = a * (1f / num);
		num += 0.5f;
		EntityAlive entityAlive = (EntityAlive)this.firingEntity;
		Ray ray = new Ray(base.transform.position + Origin.position + vector * -0.2f, vector);
		this.waterCollisionParticles.CheckCollision(ray.origin, ray.direction, num, (entityAlive != null) ? entityAlive.entityId : -1);
		int layerId = 0;
		if (entityAlive != null && entityAlive.emodel != null)
		{
			layerId = entityAlive.GetModelLayer();
			entityAlive.SetModelLayer(2, false, null);
		}
		bool flag = Voxel.Raycast(ThrownWeaponMoveScript.gameManager.World, ray, num, -538750981, (this.hmOverride == 0) ? 8 : this.hmOverride, 0f);
		if (entityAlive != null && entityAlive.emodel != null)
		{
			entityAlive.SetModelLayer(layerId, false, null);
		}
		if (flag && (GameUtils.IsBlockOrTerrain(Voxel.voxelRayHitInfo.tag) || Voxel.voxelRayHitInfo.tag.StartsWith("E_")))
		{
			if (this.firingEntity != null && !this.firingEntity.isEntityRemote)
			{
				this.stuckInEntity = (ItemActionAttack.FindHitEntity(Voxel.voxelRayHitInfo) as EntityAlive);
				entityAlive.MinEventContext.Other = this.stuckInEntity;
				entityAlive.FireEvent(MinEventTypes.onProjectileImpact, false);
				MinEventParams.CachedEventParam.Self = entityAlive;
				MinEventParams.CachedEventParam.Position = Voxel.voxelRayHitInfo.hit.pos;
				MinEventParams.CachedEventParam.ItemValue = this.itemValueWeapon;
				this.itemWeapon.FireEvent(MinEventTypes.onProjectileImpact, MinEventParams.CachedEventParam);
				ItemActionAttack.AttackHitInfo attackDetails = new ItemActionAttack.AttackHitInfo
				{
					WeaponTypeTag = ItemActionAttack.ThrownTag
				};
				if (this.itemValueWeapon.MaxUseTimes > 0)
				{
					this.itemValueWeapon.UseTimes += EffectManager.GetValue(PassiveEffects.DegradationPerUse, this.itemValueWeapon, 1f, this.firingEntity as EntityAlive, null, this.itemValueWeapon.ItemClass.ItemTags | FastTags<TagGroup.Global>.Parse("Secondary"), true, true, true, true, true, 1, true, false);
				}
				ItemActionAttack.Hit(Voxel.voxelRayHitInfo, this.ProjectileOwnerID, EnumDamageTypes.Piercing, this.itemActionThrownWeapon.GetDamageBlock(this.itemValueWeapon, ItemActionAttack.GetBlockHit(ThrownWeaponMoveScript.gameManager.World, Voxel.voxelRayHitInfo), entityAlive, 1), this.itemActionThrownWeapon.GetDamageEntity(this.itemValueWeapon, entityAlive, 1), 1f, 1f, EffectManager.GetValue(PassiveEffects.CriticalChance, this.itemValueWeapon, this.itemWeapon.CritChance.Value, entityAlive, null, this.itemWeapon.ItemTags, true, true, true, true, true, 1, true, false), ItemAction.GetDismemberChance(this.actionData, Voxel.voxelRayHitInfo), this.itemWeapon.MadeOfMaterial.SurfaceCategory, null, this.getBuffActions(), attackDetails, 1, this.itemActionThrownWeapon.ActionExp, this.itemActionThrownWeapon.ActionExpBonusMultiplier, null, null, ItemActionAttack.EnumAttackMode.RealNoHarvesting, null, -1, this.itemValueWeapon);
				this.NavObject = NavObjectManager.Instance.RegisterNavObject(this.itemWeapon.NavObject, base.transform, "", false);
				if (GameUtils.IsBlockOrTerrain(Voxel.voxelRayHitInfo.tag))
				{
					this.ProjectileID = ProjectileManager.AddProjectileItem(base.transform, -1, Voxel.voxelRayHitInfo.hit.pos, a.normalized, this.itemValueWeapon.type);
				}
				else
				{
					this.ProjectileID = ProjectileManager.AddProjectileItem(base.transform, -1, Voxel.voxelRayHitInfo.hit.pos, a.normalized, this.itemValueWeapon.type);
				}
				Utils.SetLayerRecursively(ProjectileManager.GetProjectile(this.ProjectileID).gameObject, 14, null);
			}
			else
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
			this.bArmed = false;
		}
	}

	public void DropStuckProjectile()
	{
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public void OnDestroy()
	{
		if (GameManager.Instance == null || GameManager.Instance.World == null || this.firingEntity == null || base.transform == null || this.itemValueWeapon == null)
		{
			return;
		}
		if (this.stuckInEntity != null)
		{
			NavObjectManager.Instance.UnRegisterNavObject(this.NavObject);
			if (this.ProjectileID != -1 && this.firingEntity != null && !this.firingEntity.isEntityRemote)
			{
				Vector3 bellyPosition = this.stuckInEntity.getBellyPosition();
				if (GameManager.Instance.World.IsChunkAreaLoaded(Mathf.CeilToInt(bellyPosition.x), Mathf.CeilToInt(bellyPosition.y), Mathf.CeilToInt(bellyPosition.z)))
				{
					GameManager.Instance.ItemDropServer(new ItemStack(this.itemValueWeapon, 1), bellyPosition, Vector3.zero, this.ProjectileOwnerID, 1000f, false);
					return;
				}
			}
		}
		else if (this.ProjectileID != -1 && this.firingEntity != null && !this.firingEntity.isEntityRemote && GameManager.Instance.World.IsChunkAreaLoaded(Mathf.CeilToInt(base.transform.position.x + Origin.position.x), Mathf.CeilToInt(base.transform.position.y + Origin.position.y), Mathf.CeilToInt(base.transform.position.z + Origin.position.z)))
		{
			GameManager.Instance.ItemDropServer(new ItemStack(this.itemValueWeapon, 1), base.transform.position + Origin.position + Vector3.up, Vector3.zero, this.ProjectileOwnerID, 1000f, false);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public List<string> getBuffActions()
	{
		return this.itemActionThrownWeapon.BuffActions;
	}

	public const int InvalidID = -1;

	public int ProjectileID = -1;

	public int ProjectileOwnerID;

	public ItemActionThrownWeapon itemActionThrownWeapon;

	public ItemClass itemWeapon;

	public ItemValue itemValueWeapon;

	public ItemActionThrowAway.MyInventoryData actionData;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static GameManager gameManager;

	public Vector3 flyDirection;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector3 idealPosition;

	public Vector3 velocity;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float timeShotStarted;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool bArmed;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Entity firingEntity;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector3 previousPosition;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector3 gravity;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool bOnIdealPos;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public int hmOverride;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public NavObject NavObject;

	public Vector3 FinalPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public CollisionParticleController waterCollisionParticles = new CollisionParticleController();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public EntityAlive stuckInEntity;
}

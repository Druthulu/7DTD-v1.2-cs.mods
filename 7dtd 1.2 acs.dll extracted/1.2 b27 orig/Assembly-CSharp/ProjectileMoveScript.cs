using System;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileMoveScript : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public void Awake()
	{
		if (ProjectileMoveScript.gameManager == null)
		{
			ProjectileMoveScript.gameManager = (GameManager)UnityEngine.Object.FindObjectOfType(typeof(GameManager));
		}
	}

	public void Fire(Vector3 _idealStartPosition, Vector3 _flyDirection, Entity _firingEntity, int _hmOverride = 0, float _radius = 0f)
	{
		this.flyDirection = _flyDirection.normalized;
		this.idealPosition = _idealStartPosition;
		this.firingEntity = _firingEntity;
		this.velocity = this.flyDirection.normalized * EffectManager.GetValue(PassiveEffects.ProjectileVelocity, this.itemValueLauncher, this.itemActionProjectile.Velocity, _firingEntity as EntityAlive, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
		this.hmOverride = _hmOverride;
		this.radius = _radius;
		this.waterCollisionParticles.Init(this.ProjectileOwnerID, this.itemProjectile.MadeOfMaterial.SurfaceCategory, "water", 16);
		Transform transform = base.transform;
		if (_idealStartPosition == Vector3.zero)
		{
			this.previousPosition = transform.position + Origin.position;
		}
		else
		{
			this.previousPosition = _idealStartPosition;
		}
		this.gravity = EffectManager.GetValue(PassiveEffects.ProjectileGravity, this.itemValueLauncher, this.itemActionProjectile.Gravity, _firingEntity as EntityAlive, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
		OnActivateItemGameObjectReference component = transform.GetComponent<OnActivateItemGameObjectReference>();
		if (component)
		{
			component.ActivateItem(true);
		}
		if (transform.parent)
		{
			transform.SetParent(null);
			Utils.SetLayerRecursively(transform.gameObject, 0, null);
		}
		else
		{
			transform.position = _idealStartPosition;
		}
		if (!base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(true);
		}
		this.SetState(ProjectileMoveScript.State.Active);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetState(ProjectileMoveScript.State _state)
	{
		this.state = _state;
		this.stateTime = 0f;
		if (this.state == ProjectileMoveScript.State.Dead)
		{
			Transform transform = base.transform;
			Transform transform2 = transform.Find("MeshExplode");
			if (transform2)
			{
				transform2.gameObject.SetActive(false);
			}
			Light componentInChildren = transform.GetComponentInChildren<Light>();
			if (componentInChildren)
			{
				componentInChildren.gameObject.SetActive(false);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void FixedUpdate()
	{
		float fixedDeltaTime = Time.fixedDeltaTime;
		this.stateTime += fixedDeltaTime;
		if (this.state == ProjectileMoveScript.State.Active)
		{
			if (ProjectileMoveScript.gameManager == null || ProjectileMoveScript.gameManager.World == null)
			{
				return;
			}
			if (this.stateTime > this.itemActionProjectile.FlyTime)
			{
				this.velocity.y = this.velocity.y + this.gravity * fixedDeltaTime;
			}
			Transform transform = base.transform;
			Vector3 vector = transform.position;
			Vector3 b = this.velocity * fixedDeltaTime;
			transform.LookAt(vector + b);
			if (!this.bOnIdealPos)
			{
				this.bOnIdealPos = (this.idealPosition.Equals(Vector3.zero) || this.stateTime > 0.5f);
			}
			if (this.bOnIdealPos)
			{
				vector += b;
			}
			else
			{
				this.idealPosition += b;
				vector += b;
				vector = Vector3.Lerp(vector, this.idealPosition - Origin.position, this.stateTime * 2f);
			}
			transform.position = vector;
			if (this.stateTime >= this.itemActionProjectile.LifeTime)
			{
				this.SetState(ProjectileMoveScript.State.Dead);
			}
		}
		else if (this.state == ProjectileMoveScript.State.Dead && this.stateTime > this.itemActionProjectile.DeadTime)
		{
			this.SetState(ProjectileMoveScript.State.Destroyed);
			UnityEngine.Object.Destroy(base.gameObject);
		}
		this.checkCollision();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void checkCollision()
	{
		if (this.state != ProjectileMoveScript.State.Active)
		{
			return;
		}
		if (ProjectileMoveScript.gameManager == null)
		{
			return;
		}
		World world = ProjectileMoveScript.gameManager.World;
		if (world == null)
		{
			return;
		}
		Vector3 a;
		if (this.bOnIdealPos)
		{
			a = base.transform.position + Origin.position;
		}
		else
		{
			a = this.idealPosition;
		}
		Vector3 vector = a - this.previousPosition;
		float magnitude = vector.magnitude;
		if (magnitude < 0.04f)
		{
			return;
		}
		EntityAlive entityAlive = (EntityAlive)this.firingEntity;
		Ray ray = new Ray(this.previousPosition, vector.normalized);
		this.waterCollisionParticles.CheckCollision(ray.origin, ray.direction, magnitude, (entityAlive != null) ? entityAlive.entityId : -1);
		int num = -1;
		if (entityAlive != null && entityAlive.emodel != null)
		{
			num = entityAlive.GetModelLayer();
			entityAlive.SetModelLayer(2, false, null);
		}
		int hitMask = (this.hmOverride == 0) ? 80 : this.hmOverride;
		bool flag = Voxel.Raycast(world, ray, magnitude, -538750997, hitMask, this.radius);
		if (num >= 0)
		{
			entityAlive.SetModelLayer(num, false, null);
		}
		if (flag && (GameUtils.IsBlockOrTerrain(Voxel.voxelRayHitInfo.tag) || Voxel.voxelRayHitInfo.tag.StartsWith("E_")))
		{
			if (this.firingEntity != null && !this.firingEntity.isEntityRemote)
			{
				entityAlive.MinEventContext.Other = (ItemActionAttack.FindHitEntity(Voxel.voxelRayHitInfo) as EntityAlive);
				ItemActionAttack.AttackHitInfo attackDetails = new ItemActionAttack.AttackHitInfo
				{
					WeaponTypeTag = ItemActionAttack.RangedTag
				};
				ItemActionAttack.Hit(Voxel.voxelRayHitInfo, this.ProjectileOwnerID, EnumDamageTypes.Piercing, Mathf.Lerp(1f, this.itemActionProjectile.GetDamageBlock(this.itemValueLauncher, ItemActionAttack.GetBlockHit(world, Voxel.voxelRayHitInfo), entityAlive, 0), this.actionData.strainPercent), Mathf.Lerp(1f, this.itemActionProjectile.GetDamageEntity(this.itemValueLauncher, entityAlive, 0), this.actionData.strainPercent), 1f, 1f, EffectManager.GetValue(PassiveEffects.CriticalChance, this.itemValueLauncher, this.itemProjectile.CritChance.Value, entityAlive, null, this.itemProjectile.ItemTags, true, true, true, true, true, 1, true, false), ItemAction.GetDismemberChance(this.actionData, Voxel.voxelRayHitInfo), this.itemProjectile.MadeOfMaterial.SurfaceCategory, this.itemActionProjectile.GetDamageMultiplier(), this.getBuffActions(), attackDetails, 1, this.itemActionProjectile.ActionExp, this.itemActionProjectile.ActionExpBonusMultiplier, null, null, ItemActionAttack.EnumAttackMode.RealNoHarvesting, null, -1, this.itemValueLauncher);
				if (entityAlive.MinEventContext.Other == null)
				{
					entityAlive.FireEvent(MinEventTypes.onSelfPrimaryActionMissEntity, true);
				}
				entityAlive.FireEvent(MinEventTypes.onProjectileImpact, false);
				MinEventParams.CachedEventParam.Self = entityAlive;
				MinEventParams.CachedEventParam.Position = Voxel.voxelRayHitInfo.hit.pos;
				MinEventParams.CachedEventParam.ItemValue = this.itemValueProjectile;
				MinEventParams.CachedEventParam.Other = entityAlive.MinEventContext.Other;
				this.itemProjectile.FireEvent(MinEventTypes.onProjectileImpact, MinEventParams.CachedEventParam);
				if (this.itemActionProjectile.Explosion.ParticleIndex > 0)
				{
					Vector3 vector2 = Voxel.voxelRayHitInfo.hit.pos - vector.normalized * 0.1f;
					Vector3i vector3i = World.worldToBlockPos(vector2);
					if (!world.GetBlock(vector3i).isair)
					{
						BlockFace blockFace;
						vector3i = Voxel.OneVoxelStep(vector3i, vector2, -vector.normalized, out vector2, out blockFace);
					}
					ProjectileMoveScript.gameManager.ExplosionServer(Voxel.voxelRayHitInfo.hit.clrIdx, vector2, vector3i, Quaternion.identity, this.itemActionProjectile.Explosion, this.ProjectileOwnerID, 0f, false, this.itemValueLauncher);
					this.SetState(ProjectileMoveScript.State.Dead);
					return;
				}
				if (this.itemProjectile.IsSticky)
				{
					GameRandom gameRandom = world.GetGameRandom();
					if (GameUtils.IsBlockOrTerrain(Voxel.voxelRayHitInfo.tag))
					{
						if (gameRandom.RandomFloat < EffectManager.GetValue(PassiveEffects.ProjectileStickChance, this.itemValueLauncher, 0.5f, entityAlive, null, this.itemProjectile.ItemTags | FastTags<TagGroup.Global>.Parse(Voxel.voxelRayHitInfo.fmcHit.blockValue.Block.blockMaterial.SurfaceCategory), true, true, true, true, true, 1, true, false))
						{
							this.ProjectileID = ProjectileManager.AddProjectileItem(base.transform, -1, Voxel.voxelRayHitInfo.hit.pos, vector.normalized, this.itemValueProjectile.type);
							this.SetState(ProjectileMoveScript.State.Sticky);
						}
						else
						{
							ProjectileMoveScript.gameManager.SpawnParticleEffectServer(new ParticleEffect("impact_metal_on_wood", Voxel.voxelRayHitInfo.hit.pos, Utils.BlockFaceToRotation(Voxel.voxelRayHitInfo.fmcHit.blockFace), 1f, Color.white, string.Format("{0}hit{1}", Voxel.voxelRayHitInfo.fmcHit.blockValue.Block.blockMaterial.SurfaceCategory, this.itemProjectile.MadeOfMaterial.SurfaceCategory), null), this.firingEntity.entityId, false, false);
							this.SetState(ProjectileMoveScript.State.Dead);
						}
					}
					else if (gameRandom.RandomFloat < EffectManager.GetValue(PassiveEffects.ProjectileStickChance, this.itemValueLauncher, 0.5f, entityAlive, null, this.itemProjectile.ItemTags, true, true, true, true, true, 1, true, false))
					{
						this.ProjectileID = ProjectileManager.AddProjectileItem(base.transform, -1, Voxel.voxelRayHitInfo.hit.pos, vector.normalized, this.itemValueProjectile.type);
						Utils.SetLayerRecursively(ProjectileManager.GetProjectile(this.ProjectileID).gameObject, 14, null);
						this.SetState(ProjectileMoveScript.State.Sticky);
					}
					else
					{
						ProjectileMoveScript.gameManager.SpawnParticleEffectServer(new ParticleEffect("impact_metal_on_wood", Voxel.voxelRayHitInfo.hit.pos, Utils.BlockFaceToRotation(Voxel.voxelRayHitInfo.fmcHit.blockFace), 1f, Color.white, "bullethitwood", null), this.firingEntity.entityId, false, false);
						this.SetState(ProjectileMoveScript.State.Dead);
					}
				}
				else
				{
					this.SetState(ProjectileMoveScript.State.Dead);
				}
			}
			else
			{
				this.SetState(ProjectileMoveScript.State.Dead);
			}
		}
		this.previousPosition = a;
	}

	public void OnDestroy()
	{
		if (GameManager.Instance == null || GameManager.Instance.World == null || this.firingEntity == null || this.itemValueProjectile == null)
		{
			return;
		}
		if (this.ProjectileID != -1 && this.firingEntity != null && !this.firingEntity.isEntityRemote)
		{
			Vector3 position = base.transform.position;
			if (GameManager.Instance.World.IsChunkAreaLoaded(Mathf.CeilToInt(position.x + Origin.position.x), Mathf.CeilToInt(position.y + Origin.position.y), Mathf.CeilToInt(position.z + Origin.position.z)))
			{
				GameManager.Instance.ItemDropServer(new ItemStack(this.itemValueProjectile, 1), position + Origin.position, Vector3.zero, this.ProjectileOwnerID, 60f, false);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public List<string> getBuffActions()
	{
		return this.itemActionProjectile.BuffActions;
	}

	public const int InvalidID = -1;

	public int ProjectileID = -1;

	public int ProjectileOwnerID;

	public ItemActionProjectile itemActionProjectile;

	public ItemClass itemProjectile;

	public ItemValue itemValueProjectile;

	public ItemValue itemValueLauncher;

	public ItemActionLauncher.ItemActionDataLauncher actionData;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static GameManager gameManager;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector3 flyDirection;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector3 idealPosition;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector3 velocity;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public ProjectileMoveScript.State state;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float stateTime;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Entity firingEntity;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector3 previousPosition;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float gravity;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool bOnIdealPos;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public int hmOverride;

	public Vector3 FinalPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float radius;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public CollisionParticleController waterCollisionParticles = new CollisionParticleController();

	public enum State
	{
		Idle,
		Active,
		Sticky,
		Dead,
		Destroyed
	}
}

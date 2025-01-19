using System;
using UnityEngine;

public class BlockProjectileMoveScript : ProjectileMoveScript
{
	public BlockProjectileMoveScript()
	{
		this.hmOverride = 32;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void checkCollision()
	{
		this.hmOverride = 32;
		if (this.state != ProjectileMoveScript.State.Active)
		{
			return;
		}
		if (ProjectileMoveScript.gameManager == null || ProjectileMoveScript.gameManager.World == null)
		{
			return;
		}
		Vector3 vector;
		if (this.bOnIdealPos)
		{
			vector = base.transform.position;
		}
		else
		{
			vector = this.idealPosition;
		}
		Vector3 vector2 = vector - this.previousPosition;
		float magnitude = vector2.magnitude;
		if (magnitude < 0.04f)
		{
			return;
		}
		Ray ray = new Ray(this.previousPosition, vector2.normalized);
		int layerId = 0;
		EntityAlive entityAlive = (EntityAlive)this.firingEntity;
		if (entityAlive != null && entityAlive.emodel != null)
		{
			layerId = entityAlive.GetModelLayer();
			entityAlive.SetModelLayer(2, false, null);
		}
		int hitMask = (this.hmOverride == 0) ? 80 : this.hmOverride;
		bool flag = Voxel.Raycast(ProjectileMoveScript.gameManager.World, ray, magnitude, -538750981, hitMask, 0f);
		if (entityAlive != null && entityAlive.emodel != null)
		{
			entityAlive.SetModelLayer(layerId, false, null);
		}
		if (flag && (GameUtils.IsBlockOrTerrain(Voxel.voxelRayHitInfo.tag) || Voxel.voxelRayHitInfo.tag.StartsWith("E_")))
		{
			base.enabled = false;
			UnityEngine.Object.Destroy(base.transform.gameObject);
			Transform transform = Voxel.voxelRayHitInfo.transform;
			string text = null;
			if (Voxel.voxelRayHitInfo.tag.StartsWith("E_BP_"))
			{
				text = Voxel.voxelRayHitInfo.tag.Substring("E_BP_".Length).ToLower();
				transform = GameUtils.GetHitRootTransform(Voxel.voxelRayHitInfo.tag, Voxel.voxelRayHitInfo.transform);
			}
			if (Voxel.voxelRayHitInfo.tag.StartsWith("E_"))
			{
				Entity component = transform.GetComponent<Entity>();
				if (component == null)
				{
					return;
				}
				DamageSourceEntity damageSourceEntity = new DamageSourceEntity(EnumDamageSource.External, EnumDamageTypes.Piercing, -1);
				damageSourceEntity.AttackingItem = this.itemValueProjectile;
				int strength = (int)this.GetProjectileDamageEntity();
				bool flag2 = component.IsDead();
				component.DamageEntity(damageSourceEntity, strength, false, 1f);
				if (this.itemActionProjectile.BuffActions != null && component is EntityAlive)
				{
					string context = (text != null) ? GameUtils.GetChildTransformPath(component.transform, Voxel.voxelRayHitInfo.transform) : null;
					ItemAction.ExecuteBuffActions(this.itemActionProjectile.BuffActions, -1, component as EntityAlive, false, damageSourceEntity.GetEntityDamageBodyPart(component), context);
				}
				if (!flag2 && component.IsDead())
				{
					EntityPlayer entityPlayer = GameManager.Instance.World.GetEntity(this.ProjectileOwnerID) as EntityPlayer;
					if (entityPlayer != null && EntityClass.list.ContainsKey(component.entityClass))
					{
						float value = EffectManager.GetValue(PassiveEffects.ElectricalTrapXP, entityPlayer.inventory.holdingItemItemValue, 0f, entityPlayer, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
						if (value > 0f)
						{
							entityPlayer.AddKillXP(component as EntityAlive, value);
						}
					}
				}
			}
			if (this.itemActionProjectile.Explosion.ParticleIndex > 0)
			{
				Vector3 vector3 = Voxel.voxelRayHitInfo.hit.pos - vector2.normalized * 0.1f;
				Vector3i vector3i = World.worldToBlockPos(vector3);
				if (!ProjectileMoveScript.gameManager.World.GetBlock(vector3i).isair)
				{
					BlockFace blockFace;
					vector3i = Voxel.OneVoxelStep(vector3i, vector3, -vector2.normalized, out vector3, out blockFace);
				}
				ProjectileMoveScript.gameManager.ExplosionServer(Voxel.voxelRayHitInfo.hit.clrIdx, vector3, vector3i, Quaternion.identity, this.itemActionProjectile.Explosion, this.ProjectileOwnerID, 0f, false, this.itemValueProjectile);
			}
		}
		this.previousPosition = vector;
	}

	public float GetProjectileDamageEntity()
	{
		return this.itemActionProjectile.GetDamageEntity(this.itemValueProjectile, null, 0);
	}
}

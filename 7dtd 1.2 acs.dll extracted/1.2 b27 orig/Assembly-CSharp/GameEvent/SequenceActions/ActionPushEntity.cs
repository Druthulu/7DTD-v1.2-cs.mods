using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionPushEntity : ActionBaseTargetAction
	{
		public override BaseAction.ActionCompleteStates PerformTargetAction(Entity target)
		{
			EntityAlive entityAlive = target as EntityAlive;
			if (entityAlive != null)
			{
				DamageResponse dmResponse = DamageResponse.New(false);
				dmResponse.StunDuration = 1f;
				dmResponse.Strength = (int)(this.force * 100f);
				Vector3 vector = Vector3.zero;
				switch (this.direction)
				{
				case ActionPushEntity.Direction.Random:
				{
					GameRandom random = GameEventManager.Current.Random;
					vector = new Vector3(random.RandomFloat, random.RandomFloat, random.RandomFloat);
					vector.Normalize();
					break;
				}
				case ActionPushEntity.Direction.Forward:
					vector = entityAlive.transform.forward;
					break;
				case ActionPushEntity.Direction.Backward:
					vector = entityAlive.transform.forward * -1f;
					break;
				case ActionPushEntity.Direction.Left:
					vector = entityAlive.transform.right * -1f;
					break;
				case ActionPushEntity.Direction.Right:
					vector = entityAlive.transform.right;
					break;
				}
				dmResponse.Source = new DamageSource(EnumDamageSource.External, EnumDamageTypes.Bashing, vector);
				entityAlive.DoRagdoll(dmResponse);
			}
			return BaseAction.ActionCompleteStates.Complete;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool CheckValidPosition(ref Vector3 newPoint, EntityAlive target)
		{
			World world = GameManager.Instance.World;
			Ray ray = new Ray(target.position, (newPoint - target.position).normalized);
			if (Voxel.Raycast(world, ray, this.force, -538750981, 67, 0f))
			{
				newPoint = Voxel.voxelRayHitInfo.hit.pos - ray.direction * 0.1f;
			}
			BlockValue block = world.GetBlock(new Vector3i(newPoint - ray.direction * 0.5f));
			if (block.Block.IsCollideMovement || block.Block.IsCollideArrows)
			{
				newPoint = Voxel.voxelRayHitInfo.hit.pos - ray.direction;
				block = world.GetBlock(new Vector3i(newPoint - ray.direction * 0.5f));
				if (block.Block.IsCollideMovement || block.Block.IsCollideArrows)
				{
					return false;
				}
			}
			return true;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseEnum<ActionPushEntity.Direction>(ActionPushEntity.PropDirection, ref this.direction);
			properties.ParseFloat(ActionPushEntity.PropForce, ref this.force);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionPushEntity
			{
				targetGroup = this.targetGroup,
				force = this.force,
				direction = this.direction
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public ActionPushEntity.Direction direction;

		[PublicizedFrom(EAccessModifier.Protected)]
		public float force = 2f;

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropDirection = "direction";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropForce = "distance";

		public enum Direction
		{
			Random,
			Forward,
			Backward,
			Left,
			Right
		}
	}
}

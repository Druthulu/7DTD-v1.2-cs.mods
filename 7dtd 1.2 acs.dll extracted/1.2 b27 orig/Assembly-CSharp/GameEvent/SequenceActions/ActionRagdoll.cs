using System;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionRagdoll : ActionBaseTargetAction
	{
		public override BaseAction.ActionCompleteStates PerformTargetAction(Entity target)
		{
			EntityAlive entityAlive = target as EntityAlive;
			if (entityAlive != null)
			{
				if (entityAlive.IsInElevator())
				{
					return BaseAction.ActionCompleteStates.InComplete;
				}
				if (entityAlive.AttachedToEntity != null)
				{
					entityAlive.Detach();
				}
				DamageResponse dmResponse = DamageResponse.New(false);
				dmResponse.StunDuration = GameEventManager.GetFloatValue(entityAlive, this.stunDurationText, 1f);
				dmResponse.Source = new DamageSource(EnumDamageSource.External, EnumDamageTypes.Bashing);
				entityAlive.DoRagdoll(dmResponse);
			}
			return BaseAction.ActionCompleteStates.Complete;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseString(ActionRagdoll.PropStunDuration, ref this.stunDurationText);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionRagdoll
			{
				targetGroup = this.targetGroup,
				stunDurationText = this.stunDurationText
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public string stunDurationText;

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropStunDuration = "stun_duration";
	}
}

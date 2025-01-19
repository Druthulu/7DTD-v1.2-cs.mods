using System;
using UnityEngine.Scripting;

namespace GameEvent.SequenceRequirements
{
	[Preserve]
	public class RequirementCVar : BaseOperationRequirement
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public override void OnInit()
		{
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override float LeftSide(Entity target)
		{
			EntityAlive entityAlive = target as EntityAlive;
			if (entityAlive == null)
			{
				return 0f;
			}
			return entityAlive.Buffs.GetCustomVar(this.cvar, 0f);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override float RightSide(Entity target)
		{
			return GameEventManager.GetFloatValue(target as EntityAlive, this.valueText, 0f);
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseString(RequirementCVar.PropCvar, ref this.cvar);
			properties.ParseString(RequirementCVar.PropValue, ref this.valueText);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseRequirement CloneChildSettings()
		{
			return new RequirementCVar
			{
				Invert = this.Invert,
				operation = this.operation,
				cvar = this.cvar,
				valueText = this.valueText
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public string cvar = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public string valueText;

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropCvar = "cvar";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropValue = "value";
	}
}

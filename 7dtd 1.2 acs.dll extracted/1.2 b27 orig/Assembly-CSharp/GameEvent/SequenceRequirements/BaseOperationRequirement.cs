using System;
using UnityEngine.Scripting;

namespace GameEvent.SequenceRequirements
{
	[Preserve]
	public class BaseOperationRequirement : BaseRequirement
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public override void OnInit()
		{
		}

		public override bool CanPerform(Entity target)
		{
			float num = this.LeftSide(target);
			float num2 = this.RightSide(target);
			switch (this.operation)
			{
			case BaseOperationRequirement.OperationTypes.Equals:
			case BaseOperationRequirement.OperationTypes.EQ:
			case BaseOperationRequirement.OperationTypes.E:
				return num == num2;
			case BaseOperationRequirement.OperationTypes.NotEquals:
			case BaseOperationRequirement.OperationTypes.NEQ:
			case BaseOperationRequirement.OperationTypes.NE:
				return num != num2;
			case BaseOperationRequirement.OperationTypes.Less:
			case BaseOperationRequirement.OperationTypes.LessThan:
			case BaseOperationRequirement.OperationTypes.LT:
				return num < num2;
			case BaseOperationRequirement.OperationTypes.Greater:
			case BaseOperationRequirement.OperationTypes.GreaterThan:
			case BaseOperationRequirement.OperationTypes.GT:
				return num > num2;
			case BaseOperationRequirement.OperationTypes.LessOrEqual:
			case BaseOperationRequirement.OperationTypes.LessThanOrEqualTo:
			case BaseOperationRequirement.OperationTypes.LTE:
				return num <= num2;
			case BaseOperationRequirement.OperationTypes.GreaterOrEqual:
			case BaseOperationRequirement.OperationTypes.GreaterThanOrEqualTo:
			case BaseOperationRequirement.OperationTypes.GTE:
				return num >= num2;
			default:
				return true;
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public virtual float LeftSide(Entity target)
		{
			return 0f;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public virtual float RightSide(Entity target)
		{
			return 0f;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseEnum<BaseOperationRequirement.OperationTypes>(BaseOperationRequirement.PropOperation, ref this.operation);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseRequirement CloneChildSettings()
		{
			return new BaseOperationRequirement
			{
				Invert = this.Invert,
				operation = this.operation
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public BaseOperationRequirement.OperationTypes operation = BaseOperationRequirement.OperationTypes.Equals;

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropOperation = "operation";

		[PublicizedFrom(EAccessModifier.Protected)]
		public enum OperationTypes
		{
			None,
			Equals,
			EQ,
			E,
			NotEquals,
			NEQ,
			NE,
			Less,
			LessThan,
			LT,
			Greater,
			GreaterThan,
			GT,
			LessOrEqual,
			LessThanOrEqualTo,
			LTE,
			GreaterOrEqual,
			GreaterThanOrEqualTo,
			GTE
		}
	}
}

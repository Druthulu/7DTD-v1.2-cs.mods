﻿using System;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionSetHordeNight : BaseAction
	{
		public override BaseAction.ActionCompleteStates OnPerformAction()
		{
			if (GameManager.Instance != null && GameManager.Instance.World != null)
			{
				GameManager.Instance.World.aiDirector.BloodMoonComponent.SetForToday(this.keepBMDay);
			}
			return BaseAction.ActionCompleteStates.Complete;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseBool(ActionSetHordeNight.PropKeepBMDay, ref this.keepBMDay);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionSetHordeNight
			{
				keepBMDay = this.keepBMDay
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool keepBMDay = true;

		public static string PropKeepBMDay = "keep_bm_day";
	}
}

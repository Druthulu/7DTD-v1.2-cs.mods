using System;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionResetSleepers : BaseAction
	{
		public override BaseAction.ActionCompleteStates OnPerformAction()
		{
			World world = GameManager.Instance.World;
			int sleeperVolumeCount = world.GetSleeperVolumeCount();
			for (int i = 0; i < sleeperVolumeCount; i++)
			{
				SleeperVolume sleeperVolume = world.GetSleeperVolume(i);
				if (sleeperVolume != null)
				{
					sleeperVolume.DespawnAndReset(world);
				}
			}
			Log.Out("Reset {0} sleeper volumes", new object[]
			{
				sleeperVolumeCount
			});
			return BaseAction.ActionCompleteStates.Complete;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionResetSleepers();
		}
	}
}

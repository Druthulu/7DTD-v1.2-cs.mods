using System;
using System.Collections.Generic;
using UnityEngine;

public static class BlockUtilityNavIcon
{
	public static void UpdateNavIcon(bool _shouldShow, Vector3i _blockPos)
	{
		World world = GameManager.Instance.World;
		BlockValue block = world.GetBlock(_blockPos);
		NavObject navObject = null;
		bool flag = false;
		BlockTrigger blockTrigger = world.GetBlockTrigger(0, _blockPos);
		if (blockTrigger != null)
		{
			flag = blockTrigger.ExcludeIcon;
		}
		EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		if (!flag && _shouldShow && BlockUtilityNavIcon.scBlockTypes.Contains(block.Block.GetBlockName()) && primaryPlayer != null)
		{
			PrefabInstance prefabAtPosition = GameManager.Instance.GetDynamicPrefabDecorator().GetPrefabAtPosition(_blockPos, true);
			GameManager.Instance.GetDynamicPrefabDecorator().GetPrefabAtPosition(primaryPlayer.position, true);
			bool flag2 = false;
			foreach (Quest quest in primaryPlayer.QuestJournal.quests)
			{
				bool flag3 = false;
				foreach (BaseObjective baseObjective in quest.Objectives)
				{
					if (baseObjective.Phase == quest.CurrentPhase && baseObjective is ObjectiveReturnToNPC)
					{
						flag3 = true;
					}
				}
				Vector3 pos;
				if (!flag3 && quest.RallyMarkerActivated && quest.CurrentState == Quest.QuestState.InProgress && quest.GetPositionData(out pos, Quest.PositionDataTypes.POIPosition) && GameManager.Instance.GetDynamicPrefabDecorator().GetPrefabsFromWorldPosInside(pos, quest.QuestTags).Contains(prefabAtPosition))
				{
					flag2 = true;
					break;
				}
			}
			if (flag2)
			{
				NavObject navObject2 = world.GetBlockData(_blockPos) as NavObject;
				navObject = (navObject2 as NavObject);
				if (navObject2 != null && navObject == null)
				{
					Debug.LogError("Incorrect data type in world block data");
					world.ClearBlockData(_blockPos);
				}
				if (navObject == null)
				{
					Vector3 b = Block.list[block.Block.blockID].shape.GetRotation(block) * new Vector3(0f, 0f, -0.5f);
					navObject = NavObjectManager.Instance.RegisterNavObject("quest_switch", _blockPos.ToVector3() + new Vector3(0.5f, 0.5f, 0.5f) + b, "", false, null);
					world.AddBlockData(_blockPos, navObject);
				}
			}
		}
		if (navObject == null)
		{
			BlockUtilityNavIcon.RemoveNavObject(_blockPos);
		}
	}

	public static void RemoveNavObject(Vector3i _blockPos)
	{
		World world = GameManager.Instance.World;
		NavObject navObject = world.GetBlockData(_blockPos) as NavObject;
		if (navObject != null)
		{
			NavObjectManager.Instance.UnRegisterNavObject(navObject);
			world.ClearBlockData(_blockPos);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static HashSet<string> scBlockTypes = new HashSet<string>
	{
		"powerSwitch01",
		"powerSwitch02",
		"pushButtonSwitch01",
		"pushButtonSwitch02",
		"keyRackBoxMetal01",
		"keyRackWood01"
	};
}

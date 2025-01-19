using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class QuestActionTreasureChest : BaseQuestAction
{
	public override void SetupAction()
	{
	}

	public override void PerformAction(Quest ownerQuest)
	{
		World world = GameManager.Instance.World;
		EntityPlayer ownerPlayer = ownerQuest.OwnerJournal.OwnerPlayer;
		float d = (this.Value == "" || this.Value == null) ? 50f : StringParsers.ParseFloat(this.Value, 0, -1, NumberStyles.Any);
		GameRandom gameRandom = world.GetGameRandom();
		Vector3 a = new Vector3(-1f + 2f * gameRandom.RandomFloat, 0f, -1f + 2f * gameRandom.RandomFloat);
		a.Normalize();
		Vector3 vector = ownerPlayer.position + a * d;
		int num = (int)vector.x;
		int num2 = (int)vector.z;
		int num3 = (int)(world.GetHeight(num, num2) - 3);
		BlockValue blockValue = new BlockValue
		{
			type = 372
		};
		Vector3i blockPos = new Vector3i(num, num3, num2);
		world.SetBlockRPC(blockPos, blockValue, sbyte.MaxValue);
		ownerQuest.DataVariables.Add("treasurecontainer", string.Format("{0},{1},{2}", num, num3, num2));
	}

	public override BaseQuestAction Clone()
	{
		QuestActionTreasureChest questActionTreasureChest = new QuestActionTreasureChest();
		base.CopyValues(questActionTreasureChest);
		return questActionTreasureChest;
	}
}

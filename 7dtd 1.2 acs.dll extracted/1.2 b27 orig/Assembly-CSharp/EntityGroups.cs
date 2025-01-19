using System;
using System.Collections.Generic;

public class EntityGroups
{
	public static int GetRandomFromGroup(string _sEntityGroupName, ref int lastClassId, GameRandom random = null)
	{
		List<SEntityClassAndProb> grpList = EntityGroups.list[_sEntityGroupName];
		if (random == null)
		{
			random = GameManager.Instance.World.GetGameRandom();
		}
		int num = 0;
		for (int i = 0; i < 3; i++)
		{
			num = EntityGroups.GetRandomFromGroupList(grpList, random);
			if (num != lastClassId)
			{
				lastClassId = num;
				break;
			}
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int GetRandomFromGroupList(List<SEntityClassAndProb> grpList, GameRandom random)
	{
		float randomFloat = random.RandomFloat;
		float num = 0f;
		for (int i = 0; i < grpList.Count; i++)
		{
			SEntityClassAndProb sentityClassAndProb = grpList[i];
			num += sentityClassAndProb.prob;
			if (randomFloat <= num && sentityClassAndProb.prob > 0f)
			{
				return sentityClassAndProb.entityClassId;
			}
		}
		return -1;
	}

	public static bool IsEnemyGroup(string _sEntityGroupName)
	{
		List<SEntityClassAndProb> list = EntityGroups.list[_sEntityGroupName];
		return list != null && list.Count >= 1 && EntityClass.list[list[0].entityClassId].bIsEnemyEntity;
	}

	public static void Normalize(string _sEntityGroupName, float totalp)
	{
		List<SEntityClassAndProb> list = EntityGroups.list[_sEntityGroupName];
		for (int i = 0; i < list.Count; i++)
		{
			SEntityClassAndProb value = list[i];
			value.prob /= totalp;
			list[i] = value;
		}
	}

	public static void Cleanup()
	{
		if (EntityGroups.list != null)
		{
			EntityGroups.list.Clear();
		}
	}

	public static string DefaultGroupName;

	public static DictionarySave<string, List<SEntityClassAndProb>> list = new DictionarySave<string, List<SEntityClassAndProb>>();
}

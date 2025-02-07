using System;
using System.Collections.Generic;
using System.Globalization;
using HarmonyLib;
using UnityEngine;

[HarmonyPatch(typeof(EntityAlive))]
[HarmonyPatch("CopyPropertiesFromEntityClass")]
public class Patch_RandomSizes
{
	public static void Postfix(ref EntityAlive __instance)
	{
		if (__instance is EntityPlayerLocal)
		{
			return;
		}
		if (RandomSizeHelper.AllowedRandomSize(__instance))
		{
			float min = 1f, max =1f;
            if (__instance is EntityZombie)
            {
				min = RandomSizesZA_Init.zombieMin;
				max = RandomSizesZA_Init.zombieMax;
            }
            if (__instance is EntityAnimal)
            {
				min = RandomSizesZA_Init.animalMin;
				max = RandomSizesZA_Init.animalMax;
            }
            System.Random random = new System.Random();
            float num = (float)(random.NextDouble() * (max - min) + min);
			__instance.Buffs.AddCustomVar("RandomSize", num);
            __instance.gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
            __instance.gameObject.transform.localScale = new Vector3(num, num, num);
		}
	}
}

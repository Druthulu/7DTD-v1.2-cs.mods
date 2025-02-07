using System;
using System.IO;
using HarmonyLib;
using UnityEngine;

[HarmonyPatch(typeof(EntityAlive))]
[HarmonyPatch("Read")]
public class EntityAliveRead
{
	public static void Postfix(EntityAlive __instance, BinaryReader _br)
	{
		try
		{
			if (RandomSizeHelper.AllowedRandomSize(__instance))
			{
				float num = _br.ReadSingle();
                __instance.gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
                __instance.gameObject.transform.localScale = new Vector3(num, num, num);
			}
		}
		catch (Exception)
		{
		}
	}
}

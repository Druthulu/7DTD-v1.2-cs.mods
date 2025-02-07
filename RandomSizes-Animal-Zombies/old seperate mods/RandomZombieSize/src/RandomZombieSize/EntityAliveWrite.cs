using System;
using System.IO;
using HarmonyLib;

[HarmonyPatch(typeof(EntityAlive))]
[HarmonyPatch("Write")]
public class EntityAliveWrite
{
	public static void Postfix(EntityAlive __instance, BinaryWriter _bw)
	{
		try
		{
			if (RandomSizeHelper.AllowedRandomSize(__instance))
			{
				float x = __instance.gameObject.transform.localScale.x;
				_bw.Write(x);
			}
		}
		catch (Exception)
		{
		}
	}
}

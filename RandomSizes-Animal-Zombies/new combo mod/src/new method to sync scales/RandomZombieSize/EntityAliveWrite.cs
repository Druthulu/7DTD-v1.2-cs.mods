using System;
using System.Collections.Generic;
using System.IO;
using HarmonyLib;
/*
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
				Log.Out("[RandomSizesZA Debug] EntityAliveWrite reading scale from entity localScale: {0}", x);
			}
		}
		catch (Exception)
		{
            List<string> entityTagList = __instance.EntityTags.GetTagNames();
            Log.Out("[RandomSizesZA Debug] EntityAliveRead failed, scale read ix {1}. entity is: {0}", string.Join(", ", entityTagList), __instance.gameObject.transform.localScale.x);
        }
	}
}
*/
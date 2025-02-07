using System;
using DynamicMusic;
using HarmonyLib;
using UnityEngine;
using static UIKeyBinding;

[HarmonyPatch(typeof(EntityTrader))]
[HarmonyPatch("PlayAnimReaction")]
[HarmonyPatch(new Type[]
{
    typeof(EntityTrader.AnimReaction)
})]
public class Patch_HappyTraders
{
    public static bool Prefix(EntityTrader __instance)
    {
        AvatarController avatarController = __instance.emodel.avatarController;
        if (avatarController)
        {
            Log.Out("[Happy Traders DEBUG] I think it worked");
            avatarController.TriggerReaction((int)EntityTrader.AnimReaction.Happy);
        }
        else
        {
            Log.Out("[Happy Traders DEBUG] I think it failed");
        }
        return false;
    }
}









/*
[HarmonyPatch(typeof(EntityTrader))]
[HarmonyPatch("TriggerReaction")]
[HarmonyPatch(new Type[]
{
    typeof(int)
})]
public class Patch_HappyTraders
{
	public static bool Prefix(EntityTrader __instance, ref int ___reactionTypeHash)
	{
        if (__instance.anim != null)
        {
            __instance._setInt(AvatarController.reactionTypeHash, (int)"Happy", true);
            __instance._setTrigger(AvatarController.reactionTriggerHash, true);
        }
        return false;
	}
}
*/
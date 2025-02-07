using System;
using System.Collections.Generic;
using System.Globalization;
using HarmonyLib;
using UnityEngine;
//using static EntityVehicle.RemoteData;

[HarmonyPatch(typeof(EntityAlive))]
[HarmonyPatch("CopyPropertiesFromEntityClass")]
public class EnityAliveCopyPropertiesFromEntityClass
{
    public static void Postfix(ref EntityAlive __instance)
    {
        
        if (__instance is EntityPlayerLocal)
        {
            Log.Out("[RandomSizesZA Debug] Entity is Local player");
            LogOut.LO("Entity is Local player");
            return;
        }
        if (RandomSizeHelper.AllowedRandomSize(__instance))
        {
            if (!__instance.Buffs.HasCustomVar("RandomSize"))
            {
                List<string> entityTagList = __instance.EntityTags.GetTagNames();
                //Patch_RandomSizes.RandomSize(__instance);
                float min = 1f, max = 1f;
                if (entityTagList.Contains("zombie"))
                {
                    min = RandomSizesZA_Init.zombieMin;
                    max = RandomSizesZA_Init.zombieMax;
                }
                else if (entityTagList.Contains("animal"))
                {
                    min = RandomSizesZA_Init.animalMin;
                    max = RandomSizesZA_Init.animalMax;
                }
                //var seconds = (int)(GameUtils.WorldTimeToTotalSeconds(GameManager.Instance.World.worldTime));

                /*
                 * Ok, so using the game time was a good idea, and it works like most of the time, however the client's instance of running this method sometimes does not occur on the exact same second as the host
                 * This means there was a decent percentage of de-sync going on.
                 * I believe a custom netpackage is the way to go.
                 * 
                 * I was thinking about doing a seeded random at the start of the mod, but if 3 people play, I believe the host and a client exploring would be in sync, but then the third person would get out of sync.
                 * 
                 * Another reason why netpackage is the way to go.
                 */
                /*
                 * 
                 * The net package seems to work,
                 * Clients initiate this method, the postfix will kick in, and apply scale
                 * Before applying the scale, the client requests size from server sending only the entityId
                 * The server will scan its dictionary
                 * The server fails to find the key int he dict, generates new scale, stores in dict
                 * the server returns the scale
                 * The client applies the scale
                 * hmm, the server also needs to apply the scale.
                 * So if client, send package, if server, just start searching dictionary function.
                 * all apply scale
                 * 
                 * Should we sync the whol dictionary, lookup key in dict, if not found, then server package. um, yeah, but 
                 * its unlikely to ever find a key, oh , then all need to observse the entity killed. server only

               
                //__instance.Buffs.AddCustomVar("RandomSize", num);
                //__instance.Buffs.SetCustomVarNetwork("RandomSzie", num);
                //__instance.Buffs.SetCustomVar("RandomSize", num, true);
                /*if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
                {
                    SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageModifyCVar>().Setup(__instance, "RandomSize", num));
                    Log.Out("[RandomSizesZA Debug] sent package");
                }*/
                //__instance.gameObject.transform.localScale = new Vector3(1f, 1f, 1f);

                // here we send to server to request random
                if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
                {
                    SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(
                        NetPackageManager.GetPackage<NetPkgRandomSizesZA>()
                            .ToServer(__instance.entityId, min, max));
                    Log.Out("[RandomSizesZA Debug] Main thread sent to server {0} {1} {2}", __instance.entityId, min, max);
                }

                if (NetPkgRandomSizesZA.LastEntityId == __instance.entityId)
                {
                    float num = NetPkgRandomSizesZA.LastScale;
                    __instance.gameObject.transform.localScale = new Vector3(num, num, num);
                    Log.Out("[RandomSizesZA Debug] applied scale {0} to entityId: {1}", num, __instance.entityId);
                }
                else
                {
                    Log.Out("[RandomSizesZA Debug] entityId mismatch, expecting {0} but got {1}", __instance.entityId, NetPkgRandomSizesZA.LastEntityId);
                }
            }
        }
    }
}

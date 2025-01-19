using System;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using HarmonyLib;
using LiteNetLib.Layers;
using LiteNetLib;
using UnityEngine;
using static UnityEngine.UI.CanvasScaler;

namespace TacoTown
{
    public class MajorasMaskMoon_Init : IModApi
    {
        public void InitMod(Mod _modInstance)
        {
            Log.Out(" Loading Patch: " + base.GetType().ToString());
            Harmony harmony = new Harmony(base.GetType().ToString());
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
    public static class DynamicPropertyExtensions
    {
        public static void ParseString(this DynamicProperties properties, string propName, out string result, string defaultValue)
        {
            result = null;
            bool flag = !properties.Values.ContainsKey(propName);
            if (flag)
            {
                result = defaultValue;
            }
            else
            {
                properties.ParseString(propName, ref result);
            }
        }
    }

    [HarmonyPatch(typeof(SkyManager), "Init")]
    public class SetMoonTexture_Patch
    {
        public static void Postfix()
        {
            Transform moonTransform = SkyManager.skyManager.transform.FindInChildren(SetMoonTexture_Patch.moonSprite);
            string ambientMoonTexture = "#@modfolder(MajorasMaskMoon):Resources/mm_moon.unity3d?mm_moon_512.png";
            //WorldEnvironment.Properties.ParseString("ambientMoonTexture", out ambientMoonTexture, string.Empty);
            //Log.Out($"{ambientMoonTexture}");
            Texture moonTexture = DataLoader.LoadAsset<Texture>(ambientMoonTexture);
            bool flag = moonTransform == null;
            if (flag)
            {
                string[] array = new string[5];
                array[0] = "Transform '";
                array[1] = SetMoonTexture_Patch.moonSprite;
                array[2] = "' not found on '";
                int num = 3;
                SkyManager skyManager = SkyManager.skyManager;
                array[num] = ((skyManager != null) ? skyManager.gameObject.name : null);
                array[4] = "'.";
                Log.Error(string.Concat(array));
            }
            else
            {
                MeshRenderer renderer;
                bool flag2 = moonTransform.TryGetComponent<MeshRenderer>(out renderer);
                if (flag2)
                {
                    bool flag3 = renderer.material != null && moonTexture != null;
                    if (flag3)
                    {
                        renderer.material.SetTexture(SetMoonTexture_Patch.propColorMap, moonTexture);
                    }
                }
            }
        }

        private static readonly int propColorMap = Shader.PropertyToID("_ColorMap");

        private static readonly string moonSprite = "MoonSprite";
    }
}

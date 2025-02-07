using System;
using System.Reflection;
using HarmonyLib;

public class Taco_Init : IModApi
{
	public void InitMod(Mod _modInstance)
	{
		Log.Out(" Loading Patch: " + base.GetType().ToString());
		Harmony harmony = new Harmony(base.GetType().ToString());
		harmony.PatchAll(Assembly.GetExecutingAssembly());
	}
}

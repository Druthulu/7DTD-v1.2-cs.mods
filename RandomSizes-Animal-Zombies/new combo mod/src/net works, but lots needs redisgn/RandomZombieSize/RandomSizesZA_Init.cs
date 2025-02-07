using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Xml;
using HarmonyLib;


public class RandomSizesZA_Init : IModApi
{
	public void InitMod(Mod _modInstance)
	{
        modsFolderPath = _modInstance.Path;
        ReadXML();
        Log.Out(" Loading Patch: " + base.GetType().ToString());
		Harmony harmony = new Harmony(base.GetType().ToString());
		harmony.PatchAll(Assembly.GetExecutingAssembly());
        // Make sure to reset all data when a new game is started
        ModEvents.GameStartDone.RegisterHandler(NetPkgRandomSizesZA.ResetInfo);

        //ModEvents.EntityKilled.RegisterHandler(NameOfFunc)
        //ModEvents.EntityKilled.RegisterHandler - need to use to remove from dictionary

    }
    //public GameRandom gr = new GameRandom();
    public static string modsFolderPath;
    public static bool randomZombieSizes = true;
    public static bool randomAnimalSizes = true;
    public static float zombieMin = 0.5f;
    public static float zombieMax = 1.5f;
    public static float animalMin = 0.5f;
    public static float animalMax = 1.5f;
    public static bool debug = true;
    public static Dictionary<int, float> entityScaleDict = new Dictionary<int, float>();

    public void ReadXML()
    {
        Log.Out("[RandomSizesZA] Reading prefs in {0}\\settings.xml", RandomSizesZA_Init.modsFolderPath);
        using (XmlReader xmlReader = XmlReader.Create(RandomSizesZA_Init.modsFolderPath + "\\settings.xml"))
        {
            while (xmlReader.Read())
            {
                if (xmlReader.NodeType == XmlNodeType.Element)
                {
                    if (xmlReader.Name.ToString() == "randomZombieSizes")
                    {
                        string temp = xmlReader.ReadElementContentAsString();
                        if (!bool.TryParse(temp, out randomZombieSizes))
                        {
                            Log.Out("[RandomSizesZA] failed to parse randomZombieSizes, using default value of {0}", randomZombieSizes);
                        }
                    }
                    if (xmlReader.Name.ToString() == "randomAnimalSizes")
                    {
                        string temp = xmlReader.ReadElementContentAsString();
                        if (!bool.TryParse(temp, out randomAnimalSizes))
                        {
                            Log.Out("[RandomSizesZA] failed to parse randomAnimalSizes, using default value of {0}", randomAnimalSizes);
                        }
                    }
                    if (xmlReader.Name.ToString() == "zombieMin")
                    {
                        string temp = xmlReader.ReadElementContentAsString();
                        if (!float.TryParse(temp, out zombieMin))
                        {
                            Log.Out("[RandomSizesZA] failed to parse zombieMin, using default value of {0}", zombieMin);
                        }
                        //Debug
                        Log.Out("[RandomSizesZA Debug] read zombieMin of: {0}", zombieMin);
                    }
                    if (xmlReader.Name.ToString() == "zombieMax")
                    {
                        string temp = xmlReader.ReadElementContentAsString();
                        if (!float.TryParse(temp, out zombieMax))
                        {
                            Log.Out("[RandomSizesZA] failed to parse zombieMax, using default value of {0}", zombieMax);
                        }
                    }
                    if (xmlReader.Name.ToString() == "animalMin")
                    {
                        string temp = xmlReader.ReadElementContentAsString();
                        if (!float.TryParse(temp, out animalMin))
                        {
                            Log.Out("[RandomSizesZA] failed to parse animalMin, using default value of {0}", animalMin);
                        }   
                    }
                    if (xmlReader.Name.ToString() == "animalMax")
                    {
                        string temp = xmlReader.ReadElementContentAsString();
                        if (!float.TryParse(temp, out animalMax))
                        {
                            Log.Out("[RandomSizesZA] failed to parse animalMax, using default value of {0}", animalMax);
                        }
                    }
                }
            }
        }
    }
}

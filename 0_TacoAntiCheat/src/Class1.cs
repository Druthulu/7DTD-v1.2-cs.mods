using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Xml;
using HarmonyLib;

namespace TacoTown
{
    public class Init : IModApi
    {
        public static Dictionary<string, bool> features = new Dictionary<string, bool>()
        {
            {"secureAdmin", true},
            {"secureCreativeMenu", true},
            {"secureChunkReset", true},
            {"secureCommandPermission", true},
            {"secureDebugMenu", true},
            {"secureGiveCommands", true},
            {"secureKill", true},
            {"secureListPlayers", true},
            {"securePermissionsAllowed", true},
            {"secureBlockCommands", true},
            {"secureRegionReset", true},
            {"secureGameSettings", true},
            {"secureTwitchCommands", true},
            {"secureForgeInventory", true},
            {"secureTeleport", true},
            {"secureTeleportPlayer", true},
            {"generateRandomCommandNames", true},
            {"secureHelp", true}
        };
        public static Dictionary<string, string> commandSubs = new Dictionary<string, string>()
        {
            {"admin", ""},
            {"creativeMenu", ""},
            {"cm", ""},
            {"chunkReset", ""},
            {"cr", ""},
            {"commandPermission", ""},
            {"cp", ""},
            {"debugMenu", ""},
            {"dm", ""},
            {"giveSelf", ""},
            {"giveSelfXp", ""},
            {"giveXp", ""},
            {"kill", ""},
            {"killAll", ""},
            {"listPlayers", ""},
            {"lp", ""},
            {"listPlayerIds", ""},
            {"lpi", ""},
            {"permissionsAllowed", ""},
            {"pallowed", ""},
            {"pa", ""},
            {"placeBlockRotations", ""},
            {"pbr", ""},
            {"placeBlockShapes", ""},
            {"pbs", ""},
            {"regionReset", ""},
            {"rr", ""},
            {"setGamePref", ""},
            {"sg", ""},
            {"setGameStat", ""},
            {"sgs", ""},
            {"twitch", ""},
            {"wsmats", ""},
            {"teleport", ""},
            {"tp", ""},
            {"teleportPlayer", ""},
            {"tele", ""},
            {"help", ""}
        };
        public void InitMod(Mod _modInstance)
        {
            Init.modsFolderPath = _modInstance.Path;
            ReadXML();
            RandomizeCommandNames();
            string str = " Loading Patch: ";
            Type type = base.GetType();
            Log.Out(str + ((type != null) ? type.ToString() : null));
            Harmony harmony = new Harmony(base.GetType().ToString());
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        public static string modsFolderPath;

        public void ReadXML()
        {
            using (XmlReader xmlReader = XmlReader.Create(Init.modsFolderPath + "\\settings.xml"))
            {
                while (xmlReader.Read())
                {
                    if(xmlReader.NodeType == XmlNodeType.Element){

                        for (int i = 0; i < features.Count; i++)
                        {
                            if (xmlReader.Name.ToString() == features.ElementAt(i).Key)
                            {
                                features[features.ElementAt(i).Key] = xmlReader.ReadElementContentAsBoolean();
                                //Log.Out("Feature " + features.ElementAt(i).Key + " = " + features[features.ElementAt(i).Key]);
                            }
                        }

                        for (int i = 0; i < commandSubs.Count; i++)
                        {
                            if (xmlReader.Name.ToString() == commandSubs.ElementAt(i).Key)
                            {
                                commandSubs[commandSubs.ElementAt(i).Key] = xmlReader.ReadElementContentAsString();
                                //Log.Out("Command name " + commandSubs.ElementAt(i).Key + " = " + commandSubs[commandSubs.ElementAt(i).Key]);
                            }
                        }
                    }
                }
            }
        }

        public void RandomizeCommandNames()
        {
            if (features["secureHelp"])
            {
                commandSubs["help"] = RandomStringGenerator.GenerateRandomString();
            }
            if (features["generateRandomCommandNames"])
            {
                if (features["secureAdmin"])
                {
                    commandSubs["admin"] = RandomStringGenerator.GenerateRandomString();
                }
                if (features["secureCreativeMenu"])
                {
                    commandSubs["creativeMenu"] = RandomStringGenerator.GenerateRandomString();
                    commandSubs["cm"] = RandomStringGenerator.GenerateRandomString();
                }
                if (features["secureChunkReset"])
                {
                    commandSubs["chunkReset"] = RandomStringGenerator.GenerateRandomString();
                    commandSubs["cr"] = RandomStringGenerator.GenerateRandomString();
                }
                if (features["secureCommandPermission"])
                {
                    commandSubs["commandPermission"] = RandomStringGenerator.GenerateRandomString();
                    commandSubs["cp"] = RandomStringGenerator.GenerateRandomString();
                }
                if (features["secureDebugMenu"])
                {
                    commandSubs["debugMenu"] = RandomStringGenerator.GenerateRandomString();
                    commandSubs["dm"] = RandomStringGenerator.GenerateRandomString();
                }
                if (features["secureGiveCommands"])
                {
                    commandSubs["giveSelf"] = RandomStringGenerator.GenerateRandomString();
                    commandSubs["giveSelfXp"] = RandomStringGenerator.GenerateRandomString();
                    commandSubs["giveXp"] = RandomStringGenerator.GenerateRandomString();
                }
                if (features["secureKill"])
                {
                    commandSubs["kill"] = RandomStringGenerator.GenerateRandomString();
                    commandSubs["killAll"] = RandomStringGenerator.GenerateRandomString();
                }
                if (features["secureListPlayers"])
                {
                    commandSubs["listPlayers"] = RandomStringGenerator.GenerateRandomString();
                    commandSubs["lp"] = RandomStringGenerator.GenerateRandomString();
                    commandSubs["listPlayerIds"] = RandomStringGenerator.GenerateRandomString();
                    commandSubs["lpi"] = RandomStringGenerator.GenerateRandomString();
                }
                if (features["securePermissionsAllowed"])
                {
                    commandSubs["permissionsAllowed"] = RandomStringGenerator.GenerateRandomString();
                    commandSubs["pallowed"] = RandomStringGenerator.GenerateRandomString();
                    commandSubs["pa"] = RandomStringGenerator.GenerateRandomString();
                }
                if (features["secureBlockCommands"])
                {
                    commandSubs["placeBlockRotations"] = RandomStringGenerator.GenerateRandomString();
                    commandSubs["pbr"] = RandomStringGenerator.GenerateRandomString();
                    commandSubs["placeBlockShapes"] = RandomStringGenerator.GenerateRandomString();
                    commandSubs["pbs"] = RandomStringGenerator.GenerateRandomString();
                }
                if (features["secureRegionReset"])
                {
                    commandSubs["regionReset"] = RandomStringGenerator.GenerateRandomString();
                    commandSubs["rr"] = RandomStringGenerator.GenerateRandomString();
                }
                if (features["secureGameSettings"])
                {
                    commandSubs["setGamePref"] = RandomStringGenerator.GenerateRandomString();
                    commandSubs["sg"] = RandomStringGenerator.GenerateRandomString();
                    commandSubs["setGameStat"] = RandomStringGenerator.GenerateRandomString();
                    commandSubs["sgs"] = RandomStringGenerator.GenerateRandomString();
                }
                if (features["secureTwitchCommands"])
                {
                    commandSubs["twitch"] = RandomStringGenerator.GenerateRandomString();
                }
                if (features["secureForgeInventory"])
                {
                    commandSubs["wsmats"] = RandomStringGenerator.GenerateRandomString();
                }
                if (features["secureTeleport"])
                {
                    commandSubs["teleport"] = RandomStringGenerator.GenerateRandomString();
                    commandSubs["tp"] = RandomStringGenerator.GenerateRandomString();
                }
                if (features["secureTeleportPlayer"])
                {
                    commandSubs["teleportPlayer"] = RandomStringGenerator.GenerateRandomString();
                    commandSubs["tele"] = RandomStringGenerator.GenerateRandomString();
                }
            }
        }

    }

    public class RandomStringGenerator
    {
        private static readonly string _chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        private static System.Random _random = new System.Random();

        public static string GenerateRandomString(int length = 22)
        {
            char[] buffer = new char[length];
            for (int i = 0; i < length; i++)
            {
                buffer[i] = _chars[_random.Next(_chars.Length)];
            }
            return new string(buffer);
        }
    }

    [HarmonyPatch(typeof(ConsoleCmdHelp), "getCommands")]
    public class Patch_ConsoleCmdHelp
    {
        static string[] Postfix(string[] str)
        {
            if (Init.features["secureHelp"])
            {
                //Log.Out("help is " + Init.commandSubs["help"]);
                return new string[]
                {
                    Init.commandSubs["help"]
                };
            }
            return str;
        }
    }

    [HarmonyPatch(typeof(ConsoleCmdAdmin), "getCommands")]
    public class Patch_ConsoleCmdAdmin
    {
        static string[] Postfix(string[] str)
        {
            if (Init.features["secureAdmin"])
            {
                return new string[]
                {
                    Init.commandSubs["admin"]
                };
            }
            return str;
        }
    }

    [HarmonyPatch(typeof(ConsoleCmdCreativeMenu), "getCommands")]
    public class Patch_ConsoleCmdCreativeMenu
    {
        static string[] Postfix(string[] str)
        {
            if (Init.features["secureCreativeMenu"])
            {
                return new string[]
                {
                    Init.commandSubs["creativeMenu"],
                    Init.commandSubs["cm"]
                };
            }
            return str;
        }
    }

    [HarmonyPatch(typeof(ConsoleCmdChunkReset), "getCommands")]
    public class Patch_ConsoleCmdChunkReset
    {
        static string[] Postfix(string[] str)
        {
            if (Init.features["secureChunkReset"])
            {
                return new string[]
                {
                    Init.commandSubs["chunkReset"],
                    Init.commandSubs["cr"]
                };
            }
            return str;
        }
    }

    [HarmonyPatch(typeof(ConsoleCmdCommandPermissions), "getCommands")]
    public class Patch_ConsoleCmdCommandPermissions
    {
        static string[] Postfix(string[] str)
        {
            if (Init.features["secureCommandPermission"])
            {
                return new string[]
                {
                    Init.commandSubs["commandPermission"],
                    Init.commandSubs["cp"]
                };
            }
            return str;
        }
    }


    [HarmonyPatch(typeof(ConsoleCmdDebugMenu), "getCommands")]
    public class Patch_ConsoleCmdDebugMenu
    {
        static string[] Postfix(string[] str)
        {
            if (Init.features["secureDebugMenu"])
            {
                return new string[]
                {
                    Init.commandSubs["debugMenu"],
                    Init.commandSubs["dm"]
                };
            }
            return str;
        }
    }

    [HarmonyPatch(typeof(ConsoleCmdGiveQualityItem), "getCommands")]
    public class Patch_ConsoleCmdGiveQualityItem
    {
        static string[] Postfix(string[] str)
        {
            if (Init.features["secureGiveCommands"])
            {
                return new string[]
                {
                    Init.commandSubs["giveSelf"]
                };
            }
            return str;
        }
    }

    [HarmonyPatch(typeof(ConsoleCmdSelfExp), "getCommands")]
    public class Patch_ConsoleCmdSelfExp
    {
        static string[] Postfix(string[] str)
        {
            if (Init.features["secureGiveCommands"])
            {
                return new string[]
                {
                    Init.commandSubs["giveSelfXp"]
                };
            }
            return str;
        }
    }

    [HarmonyPatch(typeof(ConsoleCmdGiveXp), "getCommands")]
    public class Patch_ConsoleCmdGiveXp
    {
        static string[] Postfix(string[] str)
        {
            if (Init.features["secureGiveCommands"])
            {
                return new string[]
                {
                    Init.commandSubs["giveXp"]
                };
            }
            return str;
        }
    }

    [HarmonyPatch(typeof(ConsoleCmdKill), "getCommands")]
    public class Patch_ConsoleCmdKill
    {
        static string[] Postfix(string[] str)
        {
            if (Init.features["secureKill"])
            {
                return new string[]
                {
                    Init.commandSubs["kill"]
                };
            }
            return str;
        }
    }

    [HarmonyPatch(typeof(ConsoleCmdKillAll), "getCommands")]
    public class Patch_ConsoleCmdKillAll
    {
        static string[] Postfix(string[] str)
        {
            if (Init.features["secureKill"])
            {
                return new string[]
                {
                    Init.commandSubs["killAll"]
                };
            }
            return str;
        }
    }

    [HarmonyPatch(typeof(ConsoleCmdListPlayers), "getCommands")]
    public class Patch_ConsoleCmdListPlayers
    {
        static string[] Postfix(string[] str)
        {
            if (Init.features["secureListPlayers"])
            {
                return new string[]
                {
                    Init.commandSubs["listPlayers"],
                    Init.commandSubs["lp"]
                };
            }
            return str;
        }
    }

    [HarmonyPatch(typeof(ConsoleCmdListPlayerIds), "getCommands")]
    public class Patch_ConsoleCmdListPlayerIds
    {
        static string[] Postfix(string[] str)
        {
            if (Init.features["secureListPlayers"])
            {
                return new string[]
                {
                    Init.commandSubs["listPlayerIds"],
                    Init.commandSubs["lpi"]
                };
            }
            return str;
        }
    }

    [HarmonyPatch(typeof(ConsoleCmdPermissionsAllowed), "getCommands")]
    public class Patch_ConsoleCmdPermissionsAllowed
    {
        static string[] Postfix(string[] str)
        {
            if (Init.features["securePermissionsAllowed"])
            {
                return new string[]
                {
                    Init.commandSubs["permissionsAllowed"],
                    Init.commandSubs["pallowed"],
                    Init.commandSubs["pa"]
                };
            }
            return str;
        }
    }

    [HarmonyPatch(typeof(ConsoleCmdPlaceBlockRotations), "getCommands")]
    public class Patch_ConsoleCmdPlaceBlockRotations
    {
        static string[] Postfix(string[] str)
        {
            if (Init.features["secureBlockCommands"])
            {
                return new string[]
                {
                    Init.commandSubs["placeBlockRotations"],
                    Init.commandSubs["pbr"]
                };
            }
            return str;
        }
    }

    [HarmonyPatch(typeof(ConsoleCmdPlaceBlockShapes), "getCommands")]
    public class Patch_ConsoleCmdPlaceBlockShapes
    {
        static string[] Postfix(string[] str)
        {
            if (Init.features["secureBlockCommands"])
            {
                return new string[]
                {
                    Init.commandSubs["placeBlockShapes"],
                    Init.commandSubs["pbs"]
                };
            }
            return str;
        }
    }

    [HarmonyPatch(typeof(ConsoleCmdRegionReset), "getCommands")]
    public class Patch_ConsoleCmdRegionReset
    {
        static string[] Postfix(string[] str)
        {
            if (Init.features["secureRegionReset"])
            {
                return new string[]
                {
                    Init.commandSubs["regionReset"],
                    Init.commandSubs["rr"]
                };
            }
            return str;
        }
    }

    [HarmonyPatch(typeof(ConsoleCmdSetGamePref), "getCommands")]
    public class Patch_ConsoleCmdSetGamePref
    {
        static string[] Postfix(string[] str)
        {
            if (Init.features["secureGameSettings"])
            {
                return new string[]
                {
                    Init.commandSubs["setGamePref"],
                    Init.commandSubs["sg"]
                };
            }
            return str;
        }
    }

    [HarmonyPatch(typeof(ConsoleCmdSetGameStat), "getCommands")]
    public class Patch_ConsoleCmdSetGameStat
    {
        static string[] Postfix(string[] str)
        {
            if (Init.features["secureGameSettings"])
            {
                return new string[]
                {
                    Init.commandSubs["setGameStat"],
                    Init.commandSubs["sgs"]
                };
            }
            return str;
        }
    }

    [HarmonyPatch(typeof(ConsoleCmdTwitchCommand), "getCommands")]
    public class Patch_ConsoleCmdTwitchCommand
    {
        static string[] Postfix(string[] str)
        {
            if (Init.features["secureTwitchCommands"])
            {
                return new string[]
                {
                    Init.commandSubs["twitch"]
                };
            }
            return str;
        }
    }

    [HarmonyPatch(typeof(ConsoleCmdWorkstationMaterials), "getCommands")]
    public class Patch_ConsoleCmdWorkstationMaterials
    {
        static string[] Postfix(string[] str)
        {
            if (Init.features["secureForgeInventory"])
            {
                return new string[]
                {
                    Init.commandSubs["wsmats"]
                };
            }
            return str;
        }
    }


    [HarmonyPatch(typeof(ConsoleCmdTeleport), "getCommands")]
    public class Patch_ConsoleCmdTeleport
    {
        static string[] Postfix(string[] str)
        {
            if (Init.features["secureTeleport"])
            {
                return new string[]
                {
                    Init.commandSubs["teleport"],
                    Init.commandSubs["tp"]
                };
            }
            return str;
        }
    }


    [HarmonyPatch(typeof(ConsoleCmdTeleportPlayer), "getCommands")]
    public class Patch_ConsoleCmdTeleportPlayer
    {
        static string[] Postfix(string[] str)
        {
            if (Init.features["secureTeleportPlayer"])
            {
                return new string[]
                {
                    Init.commandSubs["teleportPlayer"],
                    Init.commandSubs["tele"]
                };
            }
            return str;
        }
    }
}

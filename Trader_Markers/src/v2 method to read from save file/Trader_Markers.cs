using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Net;
using System.Linq;
using System.Xml;
using JetBrains.Annotations;
using Utf8Json;
using Webserver;
using Webserver.WebAPI;
using HarmonyLib;
using System.Xml.Linq;
using UniLinq;
using UnityEngine;

namespace TacoTown
{
    [UsedImplicitly]
    public class ModApi : IModApi
    {
        public static bool saveFound = false;
        public static bool worldFound = false;
        public static string gameFolderPath;
        public static string worldPrefabPath;
        public static string saveFolderPath;
        public static string mapFolderPath;
        public static string modsFolderPath;
        public static string saveGameName;
        public static string worldGameName;
        public static bool restrictTradersToMap = true;
        public static bool verboseLogging = false;
        public static string mapInfoPath;
        public static int mapSize = 0;

        public void InitMod(Mod _modInstance)
        {
            modsFolderPath = _modInstance.Path;
            saveFolderPath = GameIO.GetSaveGameDir();
            saveFolderPath = saveFolderPath.Replace('/', Path.DirectorySeparatorChar);
            string[] dirs = saveFolderPath.Split(Path.DirectorySeparatorChar);
            saveGameName = dirs[dirs.Length - 1];
            worldGameName = dirs[dirs.Length - 2];
            if (verboseLogging)
            {
                Log.Out("[ Trader_Markers DEBUG 01] saveGameName: " + saveGameName + " worldGameName: " + worldGameName);
                Log.Out("[ Trader_Markers DEBUG 02] safeFolderPath: " + saveFolderPath);
            }

            // World data
            if (File.Exists(saveFolderPath + "\\..\\..\\..\\Data\\Worlds\\" + worldGameName + "\\prefabs.xml"))
            {
                worldPrefabPath = saveFolderPath + "\\..\\..\\..\\Data\\Worlds\\" + worldGameName + "\\prefabs.xml";
                mapInfoPath = saveFolderPath + "\\..\\..\\..\\Data\\Worlds\\" + worldGameName + "\\map_info.xml";
                worldFound = true;
            }
            else if (File.Exists(saveFolderPath + "\\..\\..\\..\\GeneratedWorlds\\" + worldGameName + "\\prefabs.xml"))
            {
                worldPrefabPath = saveFolderPath + "\\..\\..\\..\\GeneratedWorlds\\" + worldGameName + "\\prefabs.xml";
                mapInfoPath = saveFolderPath + "\\..\\..\\..\\Data\\Worlds\\" + worldGameName + "\\map_info.xml";
                worldFound = true;
            }
            if (verboseLogging && worldFound)
            {
                if (worldFound)
                {
                    Log.Out("[ Trader_Markers DEBUG w1] successfully located prefabs.xml file from: " + worldPrefabPath);
                }
                else
                {
                    Log.Out("[ Trader_Markers DEBUG w2] failed to locate prefabs.xml file from: " + worldPrefabPath);
                }
                
            }

            // Save data
            if (File.Exists(saveFolderPath + "\\map\\mapinfo.json"))
            {
                mapFolderPath = saveFolderPath + "\\map";
                if (verboseLogging)
                {
                    Log.Out("[ Trader_Markers DEBUG s] successfully located map folder file: " + mapFolderPath);
                }
                saveFound = true;
            }

            
        }
    }
    public readonly struct MarkerData
    {
        public MarkerData(string _id, Vector2i _position, string _name, string _icon)
        {
            this.Id = _id;
            this.Position = _position;
            this.Name = _name;
            this.Icon = _icon;
        }

        public readonly string Id;

        public readonly Vector2i Position;

        public readonly string Name;

        public readonly string Icon;
    }
    public class TraderList
    {
        public string TraderName { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public bool restrictTrader {  get; set; }

        public TraderList(string traderName, int x, int y, bool restrictTrader)
        {
            TraderName = traderName;
            X = x;
            Y = y;
            this.restrictTrader = restrictTrader;
        }
    }
    [UsedImplicitly]
    public class Markers : AbsRestApi
    {
        List<TraderList> traderList = new List<TraderList>();
        public void ReadSettingsXML()
        {
            if(File.Exists(ModApi.modsFolderPath + "\\settings.xml"))
            {
                using (XmlReader xmlReader = XmlReader.Create(ModApi.modsFolderPath + "\\settings.xml"))
                {
                    while (xmlReader.Read())
                    {
                        if (xmlReader.NodeType == XmlNodeType.Element)
                        {
                            if (xmlReader.Name.ToString() == "restrictTradersToMap")
                            {
                                ModApi.restrictTradersToMap = xmlReader.ReadElementContentAsBoolean();
                                Log.Out("[ Trader_Markers ] Read settings.xml file and using a (restrict traders) value of: " + ModApi.restrictTradersToMap.ToString());

                            }
                            if (xmlReader.Name.ToString() == "verboseLogging")
                            {
                                ModApi.verboseLogging = xmlReader.ReadElementContentAsBoolean();
                                if (ModApi.verboseLogging)
                                {
                                    Log.Out("[ Trader_Markers ] Verbose logging enabled. Extra Log messages will occur.");
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                Log.Out("[ Trader_Markers ] unable to locate settings.xml at location: " + ModApi.modsFolderPath + "\\settings.xml " + " Using default value of restrictTradersToMap=true, and only showing traders that have been explored previously");
            }
            
        }
        public void ReadMapInfoXML()
        {
            // Scan prefabs.xml and generate list of markers
            if (ModApi.verboseLogging)
            {
                Log.Out("[ Trader_Markers Debug p] parsing map_info.xml from location: " + ModApi.mapInfoPath);
            }
            XElement rootElement = XElement.Load(ModApi.worldPrefabPath);
            foreach (XElement element in rootElement.Elements("property"))
            {
                string name = element.Attribute("name")?.Value;
                if (name != null && name.IndexOf("HeightMapSize", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    string value = element.Attribute("value")?.Value;
                    if (value != null)
                    {
                        string[] positionValues = value.Split(',');
                        if (positionValues.Length >= 1)
                        {
                            if (int.TryParse(positionValues[0], out int mapSize))
                            {
                                ModApi.mapSize = mapSize; // add 30 to have marker appear in the center of the trader prefab
                            }
                            else
                            {
                                Log.Out("[ Trader_Makers FAIL ] failed to scan mapsize from map_info.xml, something is very wrong with this world... seek help");
                            }
                        }
                    }
                }
            }
            Log.Out($"[ Trader_Markers ] Scanned map_info.xml and found world size of {ModApi.mapSize}");
        }
        public void ReadPrefabsXML()
        {
            // Scan prefabs.xml and generate list of markers
            if (ModApi.verboseLogging)
            {
                Log.Out("[ Trader_Markers Debug p] parsing prefabs.xml from location: " + ModApi.worldPrefabPath);
            }
            XElement rootElement = XElement.Load(ModApi.worldPrefabPath);
            foreach (XElement element in rootElement.Elements("decoration"))
            {
                string name = element.Attribute("name")?.Value;
                if (name != null && name.IndexOf("trader", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    string position = element.Attribute("position")?.Value;
                    if (position != null)
                    {
                        string[] positionValues = position.Split(',');
                        if (positionValues.Length >= 3)
                        {
                            if (int.TryParse(positionValues[0], out int x) && int.TryParse(positionValues[2], out int z))
                            {
                                traderList.Add(new TraderList(name, x+30, z+30, ModApi.restrictTradersToMap)); // add 30 to have marker appear in the center of the trader prefab
                            }
                        }
                    }
                }
            }
            Log.Out("[ Trader_Markers ] Scanned prefabs.xml and found " + traderList.Count.ToString() + " traders");
        }
        public void ReadMap()
        {
            string map2zoomPath = ModApi.mapFolderPath + "\\2";
            foreach(var trader in traderList)
            {
                if (trader.restrictTrader)
                {
                    string pngPath = GetPngPath(map2zoomPath, trader.X, trader.Y);
                    if (ModApi.verboseLogging)
                    {
                        Log.Out($"[ Trader_Markers DEBUG m1] Scanning for trader: {trader.TraderName} PNG path {pngPath} Trader location: {trader.X},{trader.Y}");
                    }
                    if (File.Exists(pngPath))
                    {
                        
                        bool hasNonAlphaPixels = CheckForNonAlphaPixels(pngPath, trader.X, trader.Y);
                        if (ModApi.verboseLogging)
                        {
                            Log.Out("[ Trader_Markers DEBUG m2] PNG exists at " + pngPath + " world location:" + trader.X + "," + trader.Y + " chcecked raidus of 5 for any NON alpha pixels, result: " + hasNonAlphaPixels.ToString());
                        }
                        if (hasNonAlphaPixels)
                        {
                            trader.restrictTrader = false;
                            if (ModApi.verboseLogging)
                            {
                                Log.Out("[ Trader_Markers DEBUG m3a] Marking found trader: " + trader.TraderName + " at cooridnates: " + trader.X + "," + trader.Y);
                            }
                        }
                        else
                        {
                            if (ModApi.verboseLogging)
                            {
                                Log.Out("[ Trader_Markers DEBUG m3b] all pixels around this trader are alpha, therefore not explored, and wont appear on the map");
                            }
                        }
                    }
                }
            }
        }
        static string GetPngPath(string basePath, int x, int y)
        {
            // Convert block coordinates to chunk coordinates
            int xChunk = x / 512;
            int yChunk = y / 512;

            // Handle negative coordinates by adjusting the chunk index
            // turns out we dont need this
            // crap was missing tons of traders, testing out this to fix negative workld-> region conversion
            if (x < 0 && x % 512 != 0) xChunk -= 1;
            if (y < 0 && y % 512 != 0) yChunk -= 1;

            // Construct the folder and file name
            string xFolderName = xChunk.ToString();
            string yFileName = yChunk.ToString() + ".png";

            // Combine base path, folder, and file name to form the full path
            return Path.Combine(basePath, xFolderName, yFileName);
        }
        static bool CheckForNonAlphaPixels(string filePath, int x, int y)
        {
            // We need to conver the origin center coordinates to be in the same format as pixel coordinates, so from center 0,0 to top left 0,0
            int half = ModApi.mapSize / 2;
            x = x + half;
            y = ModApi.mapSize - (y + half);
            using (Bitmap bitmap = new Bitmap(filePath))
            {
                // Calculate pixel coordinates
                int pixelX = (x % 512) / 4;
                int pixelY = (y % 512) / 4;

                // to fix bug about negative pixels
                if (pixelX < 0)
                {
                    pixelX = -pixelX;
                }
                if (pixelY < 0)
                {
                    pixelY = -pixelY;
                }

                // Fix more bugs lol
                pixelX = 128 - pixelX; // not sure why I need to do this, but I have one example that shows i need this. testing it out.
                pixelY = 128 - pixelY;

                if (ModApi.verboseLogging)
                {
                    Log.Out($"[ Trader_Markers DEBUG a1] trader Pixel X and Y {pixelX},{pixelY} from path: {filePath} ");
                    //int checkY = y % 512;
                    //Log.Out($"[ Trader_Makers DEBUG a1a] math check on Y {y} % 512 = {checkY} and then integer divide by 4= {checkY/4}");
                }
                int radius = 5;

                for (int dx = -radius; dx <= radius; dx++)
                {
                    for (int dy = -radius; dy <= radius; dy++)
                    {
                        int checkX = pixelX + dx;
                        int checkY = pixelY + dy;

                        if (checkX >= 0 && checkX < bitmap.Width && checkY >= 0 && checkY < bitmap.Height)
                        {
                            System.Drawing.Color pixel = bitmap.GetPixel(checkX, checkY);
                            //if (ModApi.verboseLogging)
                            //{
                                //string[] temp = new string[] { pixel.A.ToString(), pixel.R.ToString(), pixel.G.ToString(), pixel.B.ToString() };
                                /*string temp = string.Concat(new string[]
                                    {
                                        " pixel.A: ",
                                        pixel.A.ToString(),
                                        " pixel.R: ",
                                        pixel.R.ToString(),
                                        " pixel.G: ",
                                        pixel.G.ToString(),
                                        " pixel.B: ",
                                        pixel.B.ToString()
                                    });*/
                                //Log.Out($"[ Trader_Markers DEBUG a2a] checking pixel x,y: {checkX},{checkY}");
                                //Log.Out("[ Trader_Markers DEBUG a2] pixel.A: " + pixel.A.ToString() + " pixel.R: " + pixel.R.ToString() + " pixel.G: " + pixel.G.ToString() + " pixel.B: " + pixel.B.ToString());
                                //Log.Out("[ Trader_Markers DEBUG a2b] checking all pixels: " + temp);

                            //}
                            if (pixel.A != 0) // Check for non-alpha pixel
                            //if (pixel.A != 0 && (pixel.R > 0 || pixel.G > 0 || pixel.B > 0))
                            //if (pixel.R > 0 | pixel.G > 0 | pixel.B > 0) // Check for non-alpha pixel
                            {
                                if (ModApi.verboseLogging)
                                {
                                    Log.Out($"[ Trader_Markers DEBUG a3] pixel x,y: {checkX},{checkY} is not transparent, this pixel has been explored before");
                                }
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
        public Markers() : base(null)
        {
            ReadSettingsXML();
            if (ModApi.worldFound)
            {
                ReadMapInfoXML();
                ReadPrefabsXML();
                if (ModApi.restrictTradersToMap)
                {
                    if (ModApi.saveFound)
                    {
                        ReadMap();
                    }
                }
            }
            else
            {
                Log.Out("[ Trader_Markers ] No trader markers created. World folder not found at path: " + ModApi.worldPrefabPath);
            }

            var count = 0;
            // Iterate through list and create markers
            for (int i = 0; i < traderList.Count; i++)
            {
                if (!traderList[i].restrictTrader)
                {
                    count++;
                    string name = traderList[i].TraderName;
                    int x = traderList[i].X;
                    int y = traderList[i].Y;
                    string text = WebUtils.GenerateGuid();
                    this.markers.Add(text, new MarkerData(text, new Vector2i(x, y), name, null));
                    if (ModApi.verboseLogging)
                    {
                        Log.Out($"[ Trader_Makers DEBUG marker] Marking trader {name} at location {x},{y}");
                    }
                }

            }
            Log.Out("[ Trader_Markers ] marked " + count + " out of " + traderList.Count);
        }

        protected override void HandleRestGet(RequestContext _context)
        {
            string requestPath = _context.RequestPath;
            JsonWriter jsonWriter;
            AbsRestApi.PrepareEnvelopedResult(out jsonWriter);
            if (string.IsNullOrEmpty(requestPath))
            {
                jsonWriter.WriteBeginArray();
                bool flag = true;
                foreach (KeyValuePair<string, MarkerData> keyValuePair in this.markers)
                {
                    string text = keyValuePair.Key;
                    MarkerData markerData = keyValuePair.Value;
                    MarkerData marker = markerData;
                    if (!flag)
                    {
                        jsonWriter.WriteValueSeparator();
                    }
                    flag = false;
                    this.writeMarkerJson(ref jsonWriter, marker);
                }
                jsonWriter.WriteEndArray();
                AbsRestApi.SendEnvelopedResult(_context, ref jsonWriter, HttpStatusCode.OK, null, null, null);
                return;
            }
            MarkerData marker2;
            if (!this.markers.TryGetValue(requestPath, out marker2))
            {
                jsonWriter.WriteRaw(WebUtils.JsonEmptyData);
                AbsRestApi.SendEnvelopedResult(_context, ref jsonWriter, HttpStatusCode.NotFound, null, null, null);
                return;
            }
            jsonWriter.WriteBeginArray();
            this.writeMarkerJson(ref jsonWriter, marker2);
            jsonWriter.WriteEndArray();
            AbsRestApi.SendEnvelopedResult(_context, ref jsonWriter, HttpStatusCode.OK, null, null, null);
        }

        private void writeMarkerJson(ref JsonWriter _writer, MarkerData _marker)
        {
            _writer.WriteRaw(Markers.jsonKeyId);
            _writer.WriteString(_marker.Id);
            _writer.WriteRaw(Markers.jsonKeyX);
            _writer.WriteInt32(_marker.Position.x);
            _writer.WriteRaw(Markers.jsonKeyY);
            _writer.WriteInt32(_marker.Position.y);
            _writer.WriteRaw(Markers.jsonKeyName);
            _writer.WriteString(_marker.Name);
            _writer.WriteRaw(Markers.jsonKeyIcon);
            _writer.WriteString(_marker.Icon ?? "https://cdn-icons-png.flaticon.com/128/324/324763.png");
            _writer.WriteEndObject();
        }

        protected override void HandleRestPost(RequestContext _context, IDictionary<string, object> _jsonInput, byte[] _jsonInputData)
        {
            int x;
            if (!JsonCommons.TryGetJsonField(_jsonInput, "x", out x))
            {
                AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.BadRequest, _jsonInputData, "NO_OR_INVALID_X", null);
                return;
            }
            int y;
            if (!JsonCommons.TryGetJsonField(_jsonInput, "y", out y))
            {
                AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.BadRequest, _jsonInputData, "NO_OR_INVALID_Y", null);
                return;
            }
            string text;
            JsonCommons.TryGetJsonField(_jsonInput, "name", out text);
            if (string.IsNullOrEmpty(text))
            {
                text = null;
            }
            string text2;
            JsonCommons.TryGetJsonField(_jsonInput, "icon", out text2);
            if (string.IsNullOrEmpty(text2))
            {
                text2 = null;
            }
            string text3 = WebUtils.GenerateGuid();
            this.markers.Add(text3, new MarkerData(text3, new Vector2i(x, y), text, text2));
            JsonWriter jsonWriter;
            AbsRestApi.PrepareEnvelopedResult(out jsonWriter);
            jsonWriter.WriteString(text3);
            AbsRestApi.SendEnvelopedResult(_context, ref jsonWriter, HttpStatusCode.Created, null, null, null);
        }

        protected override void HandleRestPut(RequestContext _context, IDictionary<string, object> _jsonInput, byte[] _jsonInputData)
        {
            int x;
            if (!JsonCommons.TryGetJsonField(_jsonInput, "x", out x))
            {
                AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.BadRequest, _jsonInputData, "NO_OR_INVALID_X", null);
                return;
            }
            int y;
            if (!JsonCommons.TryGetJsonField(_jsonInput, "y", out y))
            {
                AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.BadRequest, _jsonInputData, "NO_OR_INVALID_Y", null);
                return;
            }
            string name;
            bool flag = !JsonCommons.TryGetJsonField(_jsonInput, "name", out name);
            string icon;
            bool flag2 = !JsonCommons.TryGetJsonField(_jsonInput, "icon", out icon);
            string requestPath = _context.RequestPath;
            MarkerData markerData;
            if (!this.markers.TryGetValue(requestPath, out markerData))
            {
                AbsRestApi.SendEmptyResponse(_context, HttpStatusCode.NotFound, _jsonInputData, "ID_NOT_FOUND", null);
                return;
            }
            if (flag)
            {
                name = markerData.Name;
            }
            if (flag2)
            {
                icon = markerData.Icon;
            }
            MarkerData markerData2 = new MarkerData(requestPath, new Vector2i(x, y), name, icon);
            this.markers[requestPath] = markerData2;
            JsonWriter jsonWriter;
            AbsRestApi.PrepareEnvelopedResult(out jsonWriter);
            this.writeMarkerJson(ref jsonWriter, markerData2);
            AbsRestApi.SendEnvelopedResult(_context, ref jsonWriter, HttpStatusCode.OK, null, null, null);
        }

        protected override void HandleRestDelete(RequestContext _context)
        {
            string requestPath = _context.RequestPath;
            AbsRestApi.SendEmptyResponse(_context, this.markers.Remove(requestPath) ? HttpStatusCode.NoContent : HttpStatusCode.NotFound, null, null, null);
        }

        public override int[] DefaultMethodPermissionLevels()
        {
            return new int[]
            {
                -2147483647,
                2000,
                1000,
                int.MinValue,
                int.MinValue
            };
        }

        private const int numRandomMarkers = 5;

        private const string defaultIcon = "https://cdn-icons-png.flaticon.com/128/324/324763.png";

        private readonly Dictionary<string, MarkerData> markers = new Dictionary<string, MarkerData>();

        private static readonly byte[] jsonKeyId = JsonWriter.GetEncodedPropertyNameWithBeginObject("id");

        private static readonly byte[] jsonKeyX = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("x");

        private static readonly byte[] jsonKeyY = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("y");

        private static readonly byte[] jsonKeyName = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("name");

        private static readonly byte[] jsonKeyIcon = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("icon");
    }

}

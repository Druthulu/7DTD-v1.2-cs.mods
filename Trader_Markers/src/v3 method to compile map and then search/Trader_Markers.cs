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
        //public static Bitmap finalImage;

        public void InitMod(Mod _modInstance)
        {
            modsFolderPath = _modInstance.Path;
            saveFolderPath = GameIO.GetSaveGameDir();
            saveFolderPath = saveFolderPath.Replace('/', Path.DirectorySeparatorChar);
            string[] dirs = saveFolderPath.Split(Path.DirectorySeparatorChar);
            saveGameName = dirs[dirs.Length - 1];
            worldGameName = dirs[dirs.Length - 2];

            Log.Out("[ Trader_Markers ] saveGameName: " + saveGameName + " worldGameName: " + worldGameName);
            Log.Out("[ Trader_Markers ] safeFolderPath: " + saveFolderPath);

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
                mapInfoPath = saveFolderPath + "\\..\\..\\..\\GeneratedWorlds\\" + worldGameName + "\\map_info.xml";
                worldFound = true;
            }
            if (worldFound)
            {
                Log.Out("[ Trader_Markers ] successfully located prefabs.xml file from: " + worldPrefabPath);
                Log.Out("[ Trader_Markers ] successfully located map_info.xml file from: " + mapInfoPath);
            }
            else
            {
                Log.Out("[ Trader_Markers ] failed to locate prefabs.xml file from: " + worldPrefabPath);
                Log.Out("[ Trader_Markers ] failed to locate map_info.xml file from: " + mapInfoPath);
            }

            // Save data
            if (File.Exists(saveFolderPath + "\\map\\mapinfo.json"))
            {
                mapFolderPath = saveFolderPath + "\\map\\2";
                if (verboseLogging)
                {
                    Log.Out("[ Trader_Markers ] successfully located map folder file: " + mapFolderPath);
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
        public bool restrictTrader { get; set; }

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
        void ReadSettingsXML()
        {
            if (File.Exists(ModApi.modsFolderPath + "\\settings.xml"))
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
        void ReadMapInfoXML()
        {
            // Scan map_info.xml and generate list of markers
            if (ModApi.verboseLogging)
            {
                Log.Out("[ Trader_Markers DEBUG ] parsing map_info.xml from location: " + ModApi.mapInfoPath);
            }
            XElement rootElement = XElement.Load(ModApi.mapInfoPath);
            foreach (XElement element in rootElement.Elements("property"))
            {
                if (element.Attribute("name")?.Value == "HeightMapSize")
                {
                    if (ModApi.verboseLogging)
                    {
                        Log.Out($" [ Trader_Marker DEBUG ] HeightMapSize value: {element.Attribute("value")?.Value}");
                    }
                    string value = element.Attribute("value")?.Value;
                    if (value != null)
                    {
                        string[] positionValues = value.Split(',');
                        if (positionValues.Length >= 1)
                        {
                            if (int.TryParse(positionValues[0], out int mapSize))
                            {
                                ModApi.mapSize = mapSize;
                                Log.Out("[ Trader_Makers ] Scanned mapsize from map_info.xml, map size: " + ModApi.mapSize.ToString());
                            }
                            else
                            {
                                Log.Out("[ Trader_Makers FAIL ] failed to scan mapsize from map_info.xml, something is very wrong with this world... seek help");
                            }
                        }
                    }
                }
            }
            //Log.Out($"[ Trader_Markers ] Scanned map_info.xml and found world size of {ModApi.mapSize}");
        }
        void ReadPrefabsXML()
        {
            // Scan prefabs.xml and generate list of markers
            if (ModApi.verboseLogging)
            {
                Log.Out("[ Trader_Markers Debug] parsing prefabs.xml from location: " + ModApi.worldPrefabPath);
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
                                traderList.Add(new TraderList(name, x + 30, z + 30, ModApi.restrictTradersToMap)); // add 30 to have marker appear in the center of the trader prefab
                            }
                        }
                    }
                }
            }
            Log.Out("[ Trader_Markers ] Scanned prefabs.xml and found " + traderList.Count.ToString() + " traders");
        }
        void ScanTradersAgainstWorldMap(Bitmap worldImage)
        {
            //string map2zoomPath = ModApi.mapFolderPath + "\\2";
            foreach (var trader in traderList)
            {
                if (trader.restrictTrader)
                {
                    if (ModApi.verboseLogging)
                    {
                        Log.Out($"[ Trader_Markers DEBUG m1] Scanning for trader: {trader.TraderName} at location: {trader.X},{trader.Y}");
                    }
                    bool hasNonAlphaPixels = CheckForNonAlphaPixels(worldImage, trader.X, trader.Y);
                    if (ModApi.verboseLogging)
                    {
                        Log.Out("[ Trader_Markers DEBUG m2] Finished scan, result: will list trader:" + hasNonAlphaPixels.ToString());
                    }
                    if (hasNonAlphaPixels)
                    {
                        trader.restrictTrader = false;
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
        bool CheckForNonAlphaPixels(Bitmap worldImage, int x, int y)
        {
            // We need to conver the origin center coordinates to be in the same format as pixel coordinates, so from center 0,0 to top left 0,0
            int half = ModApi.mapSize / 2;
            int pixelX = x + half;

            //int pixelY = ModApi.mapSize - (-y + half);
            int pixelY = half - y;

            // Calculate world coordinates from top left instead of origin
            if (ModApi.verboseLogging)
            {
                Log.Out($"[ Trader_Markers DEBUG m1a] converted world Coordinates are-: {pixelX},{pixelY}");
            }

            // int Divide by 4 to get pixel location
            pixelX = (int)(pixelX / 4.0);
            pixelY = (int)(pixelY / 4.0); // Y seems off by 128 , why would we need to add 128 to the result here?

            // Calculate pixel coordinates
            if (ModApi.verboseLogging)
            {
                Log.Out($"[ Trader_Markers DEBUG m1b] converted pixel Coordinates are-: {pixelX},{pixelY}");
            }
            int radius = 5;

            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    int checkX = pixelX + dx;
                    int checkY = pixelY + dy;

                    if (checkX >= 0 && checkX < worldImage.Width && checkY >= 0 && checkY < worldImage.Height)
                    {
                        System.Drawing.Color pixel = worldImage.GetPixel(checkX, checkY);

                        if (pixel.A != 0) // Check for non-alpha pixel
                        {
                            if (ModApi.verboseLogging)
                            {
                                Log.Out($"[ Trader_Markers DEBUG m1c] pixel x,y: {checkX},{checkY} is not transparent, this pixel has been explored before");
                            }
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        Bitmap BuildWorldMap()
        {
            // Calculate any size world
            int regionSize = 512; // region per zoom 2 png
            int numOfTiles = ModApi.mapSize / regionSize;
            int tileSize = 128; // Each tile is 128x128 pixels
            int worldSize = numOfTiles * tileSize; // Final image size (in pixels) 3584 for 14k map
            int tilesPerAxis = worldSize / tileSize; // Number of tiles in one dimension (28 tiles per axis)

            // Adjust offset to map world coordinates (-14 to 13) into tile grid (0 to 27)
            int offset = numOfTiles / 2; // This converts -14 to 13 into 0 to 27 index range

            if (ModApi.verboseLogging)
            {
                Log.Out($"[Trader_Markers DEBUG BuildWorldMap] worldSize: {worldSize}, numOfTiles: {numOfTiles}, mapSize: {ModApi.mapSize}");
            }

            // Create a blank image that will hold the entire map (3584x3584 pixels)
            var worldImage = new Bitmap(worldSize, worldSize);
           
            var min = -offset; // -14 oh my god, I found the problem
            var max = (offset - 1 ); // 13, the last map chunk is always blank
            // Loop over the world grid, placing each image at the correct location
            for (int y = min; y <= max; y++) // Y goes from -14 to 13
            {
                for (int x = min; x <= max; x++) // X goes from -14 to 13
                {
                    // Calculate the position of the image in the final image (tile position in world)
                    int posX = (x + offset) * tileSize;
                    int posY = (-y + offset) * tileSize -tileSize; // Y is inverted as to the orgin, testing y-offset, need to remove one offset for the first row

                    // Construct the file path for the current tile image
                    string filePath = Path.Combine(ModApi.mapFolderPath, $"{x}\\{y}.png");
                    if (File.Exists(filePath))
                    {
                        // Load the image
                        using (Bitmap tile = new Bitmap(filePath))
                        {
                            // Paste this tile into the final image at the correct position
                            using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(worldImage))
                            {
                                g.DrawImage(tile, posX, posY, tileSize, tileSize);
                            }
                        }
                    }
                }
            }
            if (ModApi.verboseLogging)
            {
                string filePathOutput = Path.Combine(ModApi.saveFolderPath, "output.png");
                worldImage.Save(filePathOutput, System.Drawing.Imaging.ImageFormat.Png);
                Log.Out($"[Trader_Markers DEBUG ] Saved worldImage to {filePathOutput}");
            }
            return worldImage;
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
                        // Build map
                        var worldImage = BuildWorldMap();
                        //Scan traders
                        ScanTradersAgainstWorldMap(worldImage);
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
                        Log.Out($"[ Trader_Makers DEBUG] Marking trader {name} at location {x},{y}");
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

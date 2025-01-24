using System;
using System.Collections.Generic;
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

namespace TacoTown
{
    [UsedImplicitly]
    public class ModApi : IModApi
    {
        public void InitMod(Mod _modInstance)
        {
            ModApi.modsFolderPath = _modInstance.Path;
        }

        public static string modsFolderPath;
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

        public TraderList(string traderName, int x, int y)
        {
            TraderName = traderName;
            X = x;
            Y = y;
        }
    }
    [UsedImplicitly]
    public class Markers : AbsRestApi
    {
        List<TraderList> traderList = new List<TraderList>();
        // Scan prefabs.xml and generate list of markers
        public void ReadXML()
        {
            if (!File.Exists(ModApi.modsFolderPath + "\\prefabs.xml"))
            {
                Log.Out("[Trader_Markers] prefabs.xml file needs to be present in mod folder for scan");
            }
            else
            {
                XElement rootElement = XElement.Load(ModApi.modsFolderPath + "\\prefabs.xml");
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
                                    traderList.Add(new TraderList(name, x+30, z+30)); // add 30 to have marker appear in the center of the trader prefab
                                }
                            }
                        }
                    }
                }
                Log.Out("[Trader_Markers] Scanned and found " + traderList.Count.ToString() + " traders");
            }
            
        }

        public Markers() : base(null)
        {
            ReadXML();
            // Iterate through list and create markers
            for (int i = 0; i < traderList.Count; i++)
            {
                string name = traderList[i].TraderName;
                int x = traderList[i].X;
                int y = traderList[i].Y;
                string text = WebUtils.GenerateGuid();
                this.markers.Add(text, new MarkerData(text, new Vector2i(x, y), name, null));
            }
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

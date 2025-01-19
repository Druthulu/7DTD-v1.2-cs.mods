using System;
using System.Collections.Generic;
using System.Globalization;

public class PrefabData
{
	public PrefabData(PathAbstractions.AbstractedLocation _location, DynamicProperties properties)
	{
		this.location = _location;
		this.Name = _location.Name.ToLower();
		DictionarySave<string, string> values = properties.Values;
		properties.ParseVec("PrefabSize", ref this.size);
		if (values.ContainsKey("POIMarkerSize") && values.ContainsKey("POIMarkerStart"))
		{
			this.POIMarkers.Clear();
			List<Vector3i> list = StringParsers.ParseList<Vector3i>(values["POIMarkerSize"], '#', (string _s, int _start, int _end) => StringParsers.ParseVector3i(_s, _start, _end, false));
			List<Vector3i> list2 = StringParsers.ParseList<Vector3i>(values["POIMarkerStart"], '#', (string _s, int _start, int _end) => StringParsers.ParseVector3i(_s, _start, _end, false));
			List<Prefab.Marker.MarkerTypes> list3 = new List<Prefab.Marker.MarkerTypes>();
			if (values.ContainsKey("POIMarkerType"))
			{
				string[] array = values["POIMarkerType"].Split(',', StringSplitOptions.None);
				for (int i = 0; i < array.Length; i++)
				{
					Prefab.Marker.MarkerTypes item;
					if (Enum.TryParse<Prefab.Marker.MarkerTypes>(array[i], true, out item))
					{
						list3.Add(item);
					}
				}
			}
			List<FastTags<TagGroup.Poi>> list4 = new List<FastTags<TagGroup.Poi>>();
			if (values.ContainsKey("POIMarkerTags"))
			{
				string[] array = values["POIMarkerTags"].Split('#', StringSplitOptions.None);
				for (int j = 0; j < array.Length; j++)
				{
					if (array[j].Length > 0)
					{
						list4.Add(FastTags<TagGroup.Poi>.Parse(array[j]));
					}
					else
					{
						list4.Add(FastTags<TagGroup.Poi>.none);
					}
				}
			}
			List<string> list5 = new List<string>();
			if (values.ContainsKey("POIMarkerGroup"))
			{
				list5.AddRange(values["POIMarkerGroup"].Split(',', StringSplitOptions.None));
			}
			List<string> list6 = new List<string>();
			if (values.ContainsKey("POIMarkerPartToSpawn"))
			{
				list6.AddRange(values["POIMarkerPartToSpawn"].Split(',', StringSplitOptions.None));
			}
			List<int> list7 = new List<int>();
			if (values.ContainsKey("POIMarkerPartRotations"))
			{
				string[] array = values["POIMarkerPartRotations"].Split(',', StringSplitOptions.None);
				string[] array2 = array;
				for (int k = 0; k < array2.Length; k++)
				{
					int item2;
					if (StringParsers.TryParseSInt32(array2[k], out item2, 0, -1, NumberStyles.Integer))
					{
						list7.Add(item2);
					}
					else
					{
						list7.Add(0);
					}
				}
			}
			List<float> list8 = new List<float>();
			if (values.ContainsKey("POIMarkerPartSpawnChance"))
			{
				string[] array = values["POIMarkerPartSpawnChance"].Split(',', StringSplitOptions.None);
				string[] array2 = array;
				for (int k = 0; k < array2.Length; k++)
				{
					float item3;
					if (StringParsers.TryParseFloat(array2[k], out item3, 0, -1, NumberStyles.Any))
					{
						list8.Add(item3);
					}
					else
					{
						list8.Add(0f);
					}
				}
			}
			for (int l = 0; l < list2.Count; l++)
			{
				Prefab.Marker marker = new Prefab.Marker();
				marker.Start = list2[l];
				if (l < list.Count)
				{
					marker.Size = list[l];
				}
				if (l < list3.Count)
				{
					marker.MarkerType = list3[l];
				}
				if (l < list5.Count)
				{
					marker.GroupName = list5[l];
				}
				if (l < list4.Count)
				{
					marker.Tags = list4[l];
				}
				if (l < list6.Count)
				{
					marker.PartToSpawn = list6[l];
				}
				if (l < list7.Count)
				{
					marker.Rotations = (byte)list7[l];
				}
				if (l < list8.Count)
				{
					marker.PartChanceToSpawn = list8[l];
				}
				this.POIMarkers.Add(marker);
			}
		}
		this.RotationsToNorth = (byte)properties.GetInt("RotationToFaceNorth");
		if (properties.Values.ContainsKey("Tags"))
		{
			this.Tags = FastTags<TagGroup.Poi>.Parse(properties.Values["Tags"].Replace(" ", ""));
		}
		if (properties.Values.ContainsKey("ThemeTags"))
		{
			this.ThemeTags = FastTags<TagGroup.Poi>.Parse(properties.Values["ThemeTags"].Replace(" ", ""));
		}
		properties.ParseInt("ThemeRepeatDistance", ref this.ThemeRepeatDistance);
		properties.ParseInt("DuplicateRepeatDistance", ref this.DuplicateRepeatDistance);
		if (properties.Classes.ContainsKey("Stats"))
		{
			WorldStats worldStats = WorldStats.FromProperties(properties.Classes["Stats"]);
			if (worldStats != null)
			{
				this.DensityScore = (float)((worldStats.TotalVertices + 50000) / 100000);
			}
		}
		this.yOffset = properties.GetInt("YOffset");
		properties.ParseInt("DifficultyTier", ref this.DifficultyTier);
	}

	public List<Prefab.Marker> RotatePOIMarkers(bool _bLeft, int _rotCount)
	{
		Vector3i vector3i = this.size;
		List<Prefab.Marker> list = new List<Prefab.Marker>(this.POIMarkers.Count);
		for (int i = 0; i < this.POIMarkers.Count; i++)
		{
			list.Add(new Prefab.Marker(this.POIMarkers[i]));
		}
		for (int j = 0; j < _rotCount; j++)
		{
			for (int k = 0; k < list.Count; k++)
			{
				Prefab.Marker marker = list[k];
				Vector3i other = marker.Size;
				Vector3i start = marker.Start;
				Vector3i vector3i2 = start + other;
				if (_bLeft)
				{
					start = new Vector3i(vector3i.z - start.z, start.y, start.x);
					vector3i2 = new Vector3i(vector3i.z - vector3i2.z, vector3i2.y, vector3i2.x);
				}
				else
				{
					start = new Vector3i(start.z, start.y, vector3i.x - start.x);
					vector3i2 = new Vector3i(vector3i2.z, vector3i2.y, vector3i.x - vector3i2.x);
				}
				if (start.x > vector3i2.x)
				{
					MathUtils.Swap(ref start.x, ref vector3i2.x);
				}
				if (start.z > vector3i2.z)
				{
					MathUtils.Swap(ref start.z, ref vector3i2.z);
				}
				marker.Start = start;
				MathUtils.Swap(ref other.x, ref other.z);
				marker.Size = other;
			}
			MathUtils.Swap(ref vector3i.x, ref vector3i.z);
		}
		return list;
	}

	public static PrefabData LoadPrefabData(PathAbstractions.AbstractedLocation _location)
	{
		if (!SdFile.Exists(_location.FullPathNoExtension + ".xml"))
		{
			return null;
		}
		DynamicProperties dynamicProperties = new DynamicProperties();
		if (!dynamicProperties.Load(_location.Folder, _location.Name, false))
		{
			return null;
		}
		return new PrefabData(_location, dynamicProperties);
	}

	public const int ThemeRepeatDistanceDefault = 300;

	public const int DuplicateRepeatDistanceDefault = 1000;

	public Vector3i size;

	public readonly string Name;

	public FastTags<TagGroup.Poi> Tags;

	public FastTags<TagGroup.Poi> ThemeTags;

	public readonly int ThemeRepeatDistance = 300;

	public readonly int DuplicateRepeatDistance = 1000;

	public byte RotationsToNorth;

	public float DensityScore;

	public int DifficultyTier;

	public int yOffset;

	public PathAbstractions.AbstractedLocation location;

	public List<Prefab.Marker> POIMarkers = new List<Prefab.Marker>();
}

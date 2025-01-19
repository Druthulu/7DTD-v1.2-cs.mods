using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PrefabMarkerList : XUiC_List<XUiC_PrefabMarkerList.PrefabMarkerEntry>
{
	public override void OnOpen()
	{
		base.OnOpen();
		this.RebuildList(false);
	}

	public override void RebuildList(bool _resetFilter = false)
	{
		if (PrefabEditModeManager.Instance.VoxelPrefab == null)
		{
			return;
		}
		this.allEntries.Clear();
		foreach (Prefab.Marker marker in PrefabEditModeManager.Instance.VoxelPrefab.POIMarkers)
		{
			this.allEntries.Add(new XUiC_PrefabMarkerList.PrefabMarkerEntry(this, marker));
		}
		this.allEntries.Sort();
		base.RebuildList(_resetFilter);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<string> groupsResult = new List<string>();

	[Preserve]
	public class PrefabMarkerEntry : XUiListEntry
	{
		public PrefabMarkerEntry(XUiC_PrefabMarkerList _parentList, Prefab.Marker _marker)
		{
			this.parentList = _parentList;
			this.marker = _marker;
		}

		public override int CompareTo(object _otherEntry)
		{
			if (!(_otherEntry is XUiC_PrefabMarkerList.PrefabMarkerEntry))
			{
				return 1;
			}
			return string.Compare(this.marker.GroupName, ((XUiC_PrefabMarkerList.PrefabMarkerEntry)_otherEntry).marker.GroupName, StringComparison.OrdinalIgnoreCase);
		}

		public override bool GetBindingValue(ref string _value, string _bindingName)
		{
			if (_bindingName == "groupname")
			{
				_value = this.marker.GroupName;
				return true;
			}
			if (_bindingName == "groupcolor")
			{
				_value = XUiC_PrefabMarkerList.PrefabMarkerEntry.colorFormatter.Format(new Color(this.marker.GroupColor.r, this.marker.GroupColor.g, this.marker.GroupColor.b, 1f));
				return true;
			}
			if (_bindingName == "markertype")
			{
				_value = this.marker.MarkerType.ToString();
				return true;
			}
			if (!(_bindingName == "markersize"))
			{
				return false;
			}
			if (Prefab.Marker.MarkerSizes.Contains(this.marker.Size))
			{
				_value = ((Prefab.Marker.MarkerSize)Prefab.Marker.MarkerSizes.IndexOf(this.marker.Size)).ToString();
			}
			else
			{
				_value = Prefab.Marker.MarkerSize.Custom.ToString();
			}
			return true;
		}

		public override bool MatchesSearch(string _searchString)
		{
			return this.marker.GroupName.IndexOf(_searchString, StringComparison.OrdinalIgnoreCase) >= 0;
		}

		[Preserve]
		public static bool GetNullBindingValues(ref string _value, string _bindingName)
		{
			if (_bindingName == "groupname")
			{
				_value = string.Empty;
				return true;
			}
			if (_bindingName == "markertype")
			{
				_value = string.Empty;
				return true;
			}
			if (_bindingName == "groupcolor")
			{
				_value = XUiC_PrefabMarkerList.PrefabMarkerEntry.colorFormatter.Format(Color.clear).ToString();
				return true;
			}
			if (!(_bindingName == "markersize"))
			{
				return false;
			}
			_value = string.Empty;
			return true;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly XUiC_PrefabMarkerList parentList;

		public readonly Prefab.Marker marker;

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly CachedStringFormatterXuiRgbaColor colorFormatter = new CachedStringFormatterXuiRgbaColor();
	}
}

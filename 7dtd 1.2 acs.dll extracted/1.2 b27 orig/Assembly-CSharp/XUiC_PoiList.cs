using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PoiList : XUiC_List<XUiC_PoiList.PoiListEntry>
{
	public bool FilterSmallPois
	{
		get
		{
			return this.filterSmallPois;
		}
		set
		{
			if (this.filterSmallPois != value)
			{
				this.filterSmallPois = value;
				this.RebuildList(false);
			}
		}
	}

	public int FilterTier
	{
		get
		{
			return this.filterTier;
		}
		set
		{
			if (this.filterTier != value)
			{
				this.filterTier = value;
				this.RebuildList(false);
			}
		}
	}

	public int MinTier { get; [PublicizedFrom(EAccessModifier.Private)] set; }

	public int MaxTier { get; [PublicizedFrom(EAccessModifier.Private)] set; }

	public override void RebuildList(bool _resetFilter = false)
	{
		this.allEntries.Clear();
		if (GameManager.Instance != null)
		{
			DynamicPrefabDecorator dynamicPrefabDecorator = GameManager.Instance.GetDynamicPrefabDecorator();
			List<PrefabInstance> list = (dynamicPrefabDecorator != null) ? dynamicPrefabDecorator.GetDynamicPrefabs() : null;
			if (list != null)
			{
				foreach (PrefabInstance prefabInstance in list)
				{
					if (!this.openedBefore)
					{
						this.MinTier = Mathf.Min((int)prefabInstance.prefab.DifficultyTier, this.MinTier);
						this.MaxTier = Mathf.Max((int)prefabInstance.prefab.DifficultyTier, this.MaxTier);
					}
					if ((!this.filterSmallPois || prefabInstance.boundingBoxSize.Volume() >= 100) && (this.filterTier < 0 || (int)prefabInstance.prefab.DifficultyTier == this.filterTier))
					{
						this.allEntries.Add(new XUiC_PoiList.PoiListEntry(prefabInstance));
					}
				}
			}
		}
		this.allEntries.Sort();
		base.RebuildList(_resetFilter);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (!this.openedBefore || this.allEntries.Count == 0)
		{
			this.RebuildList(false);
		}
		this.openedBefore = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int SmallPoiVolumeLimit = 100;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool filterSmallPois = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public int filterTier = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool openedBefore;

	[Preserve]
	public class PoiListEntry : XUiListEntry
	{
		public PoiListEntry(PrefabInstance _prefabInstance)
		{
			this.prefabInstance = _prefabInstance;
		}

		public override int CompareTo(object _otherEntry)
		{
			XUiC_PoiList.PoiListEntry poiListEntry = _otherEntry as XUiC_PoiList.PoiListEntry;
			if (poiListEntry == null)
			{
				return 1;
			}
			int num = string.Compare(this.prefabInstance.name, poiListEntry.prefabInstance.name, StringComparison.OrdinalIgnoreCase);
			if (num != 0)
			{
				return num;
			}
			num = this.prefabInstance.boundingBoxPosition.x - poiListEntry.prefabInstance.boundingBoxPosition.x;
			if (num != 0)
			{
				return num;
			}
			return this.prefabInstance.boundingBoxPosition.z - poiListEntry.prefabInstance.boundingBoxPosition.z;
		}

		public override bool GetBindingValue(ref string _value, string _bindingName)
		{
			if (_bindingName == "name")
			{
				_value = this.prefabInstance.prefab.PrefabName;
				return true;
			}
			if (_bindingName == "localizedname")
			{
				_value = this.prefabInstance.prefab.LocalizedName;
				return true;
			}
			if (!(_bindingName == "coords"))
			{
				return false;
			}
			_value = "(" + ValueDisplayFormatters.WorldPos(this.prefabInstance.boundingBoxPosition.ToVector3(), " ", false) + ")";
			return true;
		}

		public override bool MatchesSearch(string _searchString)
		{
			Prefab prefab = this.prefabInstance.prefab;
			return prefab.PrefabName.ContainsCaseInsensitive(_searchString) || prefab.LocalizedName.ContainsCaseInsensitive(_searchString);
		}

		[Preserve]
		public static bool GetNullBindingValues(ref string _value, string _bindingName)
		{
			if (_bindingName == "name")
			{
				_value = string.Empty;
				return true;
			}
			if (_bindingName == "localizedname")
			{
				_value = string.Empty;
				return true;
			}
			if (!(_bindingName == "coords"))
			{
				return false;
			}
			_value = string.Empty;
			return true;
		}

		public readonly PrefabInstance prefabInstance;
	}
}

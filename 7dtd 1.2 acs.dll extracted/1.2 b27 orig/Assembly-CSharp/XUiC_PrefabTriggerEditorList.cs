using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PrefabTriggerEditorList : XUiC_List<XUiC_PrefabTriggerEditorList.PrefabTriggerEntry>
{
	public override void OnOpen()
	{
		base.OnOpen();
		this.RebuildList(false);
	}

	public override void RebuildList(bool _resetFilter = false)
	{
		this.allEntries.Clear();
		this.groupsResult.Clear();
		if (this.EditPrefab != null)
		{
			List<byte> triggerLayers = this.EditPrefab.TriggerLayers;
			for (int i = 0; i < triggerLayers.Count; i++)
			{
				this.allEntries.Add(new XUiC_PrefabTriggerEditorList.PrefabTriggerEntry(this, triggerLayers[i]));
			}
		}
		this.allEntries.Sort();
		base.RebuildList(_resetFilter);
	}

	public Prefab EditPrefab;

	public XUiC_TriggerProperties Owner;

	public XUiC_WoPropsSleeperVolume SleeperOwner;

	public bool IsTriggers;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<string> groupsResult = new List<string>();

	[Preserve]
	public class PrefabTriggerEntry : XUiListEntry
	{
		public PrefabTriggerEntry(XUiC_PrefabTriggerEditorList _parentList, byte _triggerLayer)
		{
			this.parentList = _parentList;
			this.TriggerLayer = _triggerLayer;
			this.name = _triggerLayer.ToString();
		}

		public override int CompareTo(object _otherEntry)
		{
			if (!(_otherEntry is XUiC_PrefabTriggerEditorList.PrefabTriggerEntry))
			{
				return 1;
			}
			return this.TriggerLayer.CompareTo(((XUiC_PrefabTriggerEditorList.PrefabTriggerEntry)_otherEntry).TriggerLayer);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool GetSelected()
		{
			bool result = false;
			if (this.parentList.Owner != null)
			{
				if (this.parentList.Owner.blockTrigger != null || this.parentList.Owner.TriggerVolume != null)
				{
					if (this.parentList.IsTriggers)
					{
						if (this.parentList.Owner.TriggersIndices != null)
						{
							result = this.parentList.Owner.TriggersIndices.Contains(StringParsers.ParseUInt8(this.name, 0, -1, NumberStyles.Integer));
						}
					}
					else if (this.parentList.Owner != null)
					{
						if (this.parentList.Owner.TriggeredByIndices != null)
						{
							result = this.parentList.Owner.TriggeredByIndices.Contains(StringParsers.ParseUInt8(this.name, 0, -1, NumberStyles.Integer));
						}
					}
					else if (this.parentList.SleeperOwner != null && this.parentList.SleeperOwner.TriggeredByIndices != null)
					{
						result = this.parentList.SleeperOwner.TriggeredByIndices.Contains(StringParsers.ParseUInt8(this.name, 0, -1, NumberStyles.Integer));
					}
				}
			}
			else if (this.parentList.SleeperOwner != null && !this.parentList.IsTriggers && this.parentList.SleeperOwner != null && this.parentList.SleeperOwner.TriggeredByIndices != null)
			{
				result = this.parentList.SleeperOwner.TriggeredByIndices.Contains(StringParsers.ParseUInt8(this.name, 0, -1, NumberStyles.Integer));
			}
			return result;
		}

		public override bool GetBindingValue(ref string _value, string _bindingName)
		{
			if (_bindingName == "name")
			{
				_value = this.name;
				return true;
			}
			if (_bindingName == "selected")
			{
				_value = (this.GetSelected() ? "true" : "false");
				return true;
			}
			if (!(_bindingName == "assigned"))
			{
				return false;
			}
			_value = "true";
			return true;
		}

		public override bool MatchesSearch(string _searchString)
		{
			return this.name.IndexOf(_searchString, StringComparison.OrdinalIgnoreCase) >= 0;
		}

		public static bool GetNullBindingValues(ref string _value, string _bindingName)
		{
			if (_bindingName == "name")
			{
				_value = string.Empty;
				return true;
			}
			if (_bindingName == "selected")
			{
				_value = "false";
				return true;
			}
			if (!(_bindingName == "assigned"))
			{
				return false;
			}
			_value = "false";
			return true;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly XUiC_PrefabTriggerEditorList parentList;

		public readonly string name;

		public byte TriggerLayer;
	}
}

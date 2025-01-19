using System;
using UniLinq;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ProfilesList : XUiC_List<XUiC_ProfilesList.ListEntry>
{
	public override void OnOpen()
	{
		base.OnOpen();
		this.RebuildList(false);
	}

	public override void RebuildList(bool _resetFilter = false)
	{
		this.allEntries.Clear();
		foreach (string text in Archetype.s_Archetypes.Keys)
		{
			if (text != "BaseMale" && text != "BaseFemale")
			{
				this.allEntries.Add(new XUiC_ProfilesList.ListEntry(text, true));
			}
		}
		foreach (string name in (from s in ProfileSDF.GetProfiles()
		where (Archetype.GetArchetype(s) == null && ProfileSDF.GetArchetype(s) != null && (ProfileSDF.GetArchetype(s).Equals("BaseMale") || ProfileSDF.GetArchetype(s).Equals("BaseFemale"))) || Archetype.GetArchetype(s) != null
		select s).ToArray<string>())
		{
			bool flag = Archetype.GetArchetype(name) != null;
			if (!flag)
			{
				this.allEntries.Add(new XUiC_ProfilesList.ListEntry(name, flag));
			}
		}
		this.allEntries.Sort();
		base.RebuildList(_resetFilter);
		this.SelectByName(ProfileSDF.CurrentProfileName());
	}

	public bool SelectByName(string _name)
	{
		if (!string.IsNullOrEmpty(_name))
		{
			for (int i = 0; i < this.filteredEntries.Count; i++)
			{
				if (this.filteredEntries[i].name.Equals(_name, StringComparison.OrdinalIgnoreCase))
				{
					base.SelectedEntryIndex = i;
					return true;
				}
			}
		}
		return false;
	}

	[Preserve]
	public class ListEntry : XUiListEntry
	{
		public ListEntry(string _name, bool _isArchetype)
		{
			this.name = _name;
			this.isArchetype = _isArchetype;
		}

		public override int CompareTo(object _otherEntry)
		{
			if (!(_otherEntry is XUiC_ProfilesList.ListEntry))
			{
				return 1;
			}
			int num = -this.isArchetype.CompareTo(((XUiC_ProfilesList.ListEntry)_otherEntry).isArchetype);
			if (num != 0)
			{
				return num;
			}
			return string.Compare(this.name, ((XUiC_ProfilesList.ListEntry)_otherEntry).name, StringComparison.OrdinalIgnoreCase);
		}

		public override bool GetBindingValue(ref string _value, string _bindingName)
		{
			if (_bindingName == "name")
			{
				_value = this.name;
				return true;
			}
			return false;
		}

		public override bool MatchesSearch(string _searchString)
		{
			return this.name.IndexOf(_searchString, StringComparison.OrdinalIgnoreCase) >= 0;
		}

		[Preserve]
		public static bool GetNullBindingValues(ref string _value, string _bindingName)
		{
			if (_bindingName == "name")
			{
				_value = string.Empty;
				return true;
			}
			return false;
		}

		public readonly string name;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly bool isArchetype;
	}
}

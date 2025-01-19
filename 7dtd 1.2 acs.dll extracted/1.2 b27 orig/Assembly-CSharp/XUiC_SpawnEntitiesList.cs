using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SpawnEntitiesList : XUiC_List<XUiC_SpawnEntitiesList.SpawnEntityEntry>
{
	public override void RebuildList(bool _resetFilter = false)
	{
		this.allEntries.Clear();
		foreach (KeyValuePair<int, EntityClass> keyValuePair in EntityClass.list.Dict)
		{
			if (keyValuePair.Value.userSpawnType == EntityClass.UserSpawnType.Menu)
			{
				this.allEntries.Add(new XUiC_SpawnEntitiesList.SpawnEntityEntry(keyValuePair.Value.entityClassName, keyValuePair.Key));
			}
		}
		this.allEntries.Sort();
		base.RebuildList(_resetFilter);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (this.allEntries.Count == 0)
		{
			this.RebuildList(false);
		}
	}

	[Preserve]
	public class SpawnEntityEntry : XUiListEntry
	{
		public SpawnEntityEntry(string _name, int _key)
		{
			this.name = _name;
			this.key = _key;
			this.camelCase = this.name.SeparateCamelCase();
		}

		public override int CompareTo(object _otherEntry)
		{
			if (!(_otherEntry is XUiC_SpawnEntitiesList.SpawnEntityEntry))
			{
				return 1;
			}
			return string.Compare(this.name, ((XUiC_SpawnEntitiesList.SpawnEntityEntry)_otherEntry).name, StringComparison.OrdinalIgnoreCase);
		}

		public override bool GetBindingValue(ref string _value, string _bindingName)
		{
			if (_bindingName == "name")
			{
				_value = this.camelCase;
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

		public readonly int key;

		public readonly string camelCase;
	}
}

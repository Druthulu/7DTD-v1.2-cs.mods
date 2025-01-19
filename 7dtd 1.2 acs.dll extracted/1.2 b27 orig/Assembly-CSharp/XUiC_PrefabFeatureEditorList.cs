﻿using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public abstract class XUiC_PrefabFeatureEditorList : XUiC_List<XUiC_PrefabFeatureEditorList.FeatureEntry>
{
	public event XUiC_PrefabFeatureEditorList.FeatureChangedDelegate FeatureChanged;

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract bool FeatureEnabled(string _featureName);

	public override void Init()
	{
		base.Init();
		base.SelectionChanged += this.FeatureListSelectionChanged;
		this.addInput = (base.GetChildById("addInput") as XUiC_TextInput);
		if (this.addInput != null)
		{
			this.addInput.OnSubmitHandler += this.OnAddInputSubmit;
		}
		XUiController childById = base.GetChildById("addButton");
		if (childById != null)
		{
			childById.OnPress += this.HandleAddEntry;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleAddEntry(XUiController _sender, int _mouseButton)
	{
		this.OnAddFeaturePressed(this.addInput.Text);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnAddInputSubmit(XUiController _sender, string _text)
	{
		this.OnAddFeaturePressed(this.addInput.Text);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract void AddNewFeature(string _featureName);

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnAddFeaturePressed(string _s)
	{
		_s = _s.Trim();
		if (!this.validGroupName(_s))
		{
			return;
		}
		this.AddNewFeature(_s);
		this.RebuildList(false);
		this.addInput.Text = string.Empty;
		XUiC_PrefabFeatureEditorList.FeatureChangedDelegate featureChanged = this.FeatureChanged;
		if (featureChanged == null)
		{
			return;
		}
		featureChanged(this, _s, true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool validGroupName(string _s)
	{
		_s = _s.Trim();
		return _s.Length > 0 && _s.IndexOf(",", StringComparison.OrdinalIgnoreCase) < 0 && !this.groupsResult.ContainsCaseInsensitive(_s);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract void ToggleFeature(string _featureName);

	[PublicizedFrom(EAccessModifier.Private)]
	public void FeatureListSelectionChanged(XUiC_ListEntry<XUiC_PrefabFeatureEditorList.FeatureEntry> _previousEntry, XUiC_ListEntry<XUiC_PrefabFeatureEditorList.FeatureEntry> _newEntry)
	{
		if (_newEntry == null)
		{
			return;
		}
		string name = _newEntry.GetEntry().Name;
		this.ToggleFeature(name);
		_newEntry.IsDirty = true;
		XUiC_PrefabFeatureEditorList.FeatureChangedDelegate featureChanged = this.FeatureChanged;
		if (featureChanged == null)
		{
			return;
		}
		featureChanged(this, name, this.FeatureEnabled(name));
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.RebuildList(false);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract void GetSupportedFeatures();

	public override void RebuildList(bool _resetFilter = false)
	{
		this.allEntries.Clear();
		this.groupsResult.Clear();
		this.GetSupportedFeatures();
		foreach (string name in this.groupsResult)
		{
			this.allEntries.Add(new XUiC_PrefabFeatureEditorList.FeatureEntry(this, name));
		}
		this.allEntries.Sort();
		base.RebuildList(_resetFilter);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_PrefabFeatureEditorList()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput addInput;

	public Prefab EditPrefab;

	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly List<string> groupsResult = new List<string>();

	public delegate void FeatureChangedDelegate(XUiC_PrefabFeatureEditorList _list, string _featureName, bool _selected);

	[Preserve]
	public class FeatureEntry : XUiListEntry
	{
		public FeatureEntry(XUiC_PrefabFeatureEditorList _parentList, string _name)
		{
			this.parentList = _parentList;
			this.Name = _name;
		}

		public override int CompareTo(object _otherEntry)
		{
			XUiC_PrefabFeatureEditorList.FeatureEntry featureEntry = _otherEntry as XUiC_PrefabFeatureEditorList.FeatureEntry;
			if (featureEntry == null)
			{
				return 1;
			}
			return string.Compare(this.Name, featureEntry.Name, StringComparison.OrdinalIgnoreCase);
		}

		public override bool GetBindingValue(ref string _value, string _bindingName)
		{
			if (_bindingName == "name")
			{
				_value = this.Name;
				return true;
			}
			if (_bindingName == "selected")
			{
				bool flag = false;
				if (this.parentList.EditPrefab != null)
				{
					flag = this.parentList.FeatureEnabled(this.Name);
				}
				_value = (flag ? "true" : "false");
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
			return this.Name.IndexOf(_searchString, StringComparison.OrdinalIgnoreCase) >= 0;
		}

		[Preserve]
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
		public readonly XUiC_PrefabFeatureEditorList parentList;

		public readonly string Name;
	}
}

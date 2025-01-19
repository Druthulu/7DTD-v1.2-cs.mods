using System;
using System.Collections.Generic;
using Platform;
using UnityEngine;

public abstract class XUiC_List<T> : XUiController where T : XUiListEntry
{
	public event XUiEvent_ListSelectionChangedEventHandler<T> SelectionChanged;

	public event XUiEvent_ListPageNumberChangedEventHandler PageNumberChanged;

	public event XUiEvent_ListEntryClickedEventHandler<T> ListEntryClicked;

	public event XUiEvent_PageContentsChangedEventHandler PageContentsChanged;

	public int PageLength
	{
		get
		{
			return this.listEntryControllers.Length;
		}
	}

	public int Page
	{
		get
		{
			return this.page;
		}
		set
		{
			int num = Math.Max(0, Math.Min(value, this.LastPage));
			if (num != this.page)
			{
				this.page = num;
				XUiC_Paging xuiC_Paging = this.pager;
				if (xuiC_Paging != null)
				{
					xuiC_Paging.SetPage(this.page);
				}
				this.IsDirty = true;
				this.SelectedEntry = null;
				XUiEvent_ListPageNumberChangedEventHandler pageNumberChanged = this.PageNumberChanged;
				if (pageNumberChanged == null)
				{
					return;
				}
				pageNumberChanged(this.page);
			}
		}
	}

	public int LastPage
	{
		get
		{
			return Math.Max(0, Mathf.CeilToInt((float)this.filteredEntries.Count / (float)this.PageLength) - 1);
		}
	}

	public XUiC_ListEntry<T> SelectedEntry
	{
		get
		{
			return this.selectedEntry;
		}
		set
		{
			if (value != this.selectedEntry)
			{
				T currentSelectedEntry = this.CurrentSelectedEntry;
				XUiC_ListEntry<T> xuiC_ListEntry = this.selectedEntry;
				this.selectedEntry = null;
				if (xuiC_ListEntry != null)
				{
					xuiC_ListEntry.Selected = false;
				}
				this.selectedEntry = value;
				if (this.selectedEntry != null)
				{
					this.selectedEntry.Selected = true;
					this.CurrentSelectedEntry = this.selectedEntry.GetEntry();
					for (int i = 0; i < this.listEntryControllers.Length; i++)
					{
						if (this.selectedEntry == this.listEntryControllers[i])
						{
							this.selectedEntryIndex = this.page * this.PageLength + i;
						}
					}
				}
				else
				{
					this.CurrentSelectedEntry = default(T);
					this.selectedEntryIndex = -1;
				}
				if (currentSelectedEntry != this.CurrentSelectedEntry)
				{
					this.OnSelectionChanged(xuiC_ListEntry, this.selectedEntry);
				}
			}
		}
	}

	public int SelectedEntryIndex
	{
		get
		{
			return this.selectedEntryIndex;
		}
		set
		{
			if (value >= 0 && value < this.EntryCount && this.selectedEntryIndex != value)
			{
				this.Page = value / this.PageLength;
				this.selectedEntryIndex = value;
				this.updateSelectedItemByIndex = true;
				this.updateCurrentPageContents();
				this.updateSelectedItemByIndex = false;
				this.SelectedEntry = this.listEntryControllers[this.selectedEntryIndex % this.PageLength];
				this.IsDirty = true;
			}
		}
	}

	public int EntryCount
	{
		get
		{
			return this.filteredEntries.Count;
		}
	}

	public int UnfilteredEntryCount
	{
		get
		{
			return this.allEntries.Count;
		}
	}

	public bool ClearSelectionOnOpenClose { get; set; } = true;

	public bool ClearSearchTextOnOpenClose { get; set; }

	public bool SelectableEntries { get; set; } = true;

	public bool CursorControllable { get; set; }

	public override void Init()
	{
		base.Init();
		base.OnScroll += this.HandleOnScroll;
		this.pager = base.GetChildByType<XUiC_Paging>();
		if (this.pager != null)
		{
			this.pager.OnPageChanged += delegate()
			{
				this.Page = this.pager.CurrentPageNumber;
			};
		}
		XUiController childById = base.GetChildById("list");
		if (childById != null)
		{
			this.listEntryControllers = new XUiC_ListEntry<T>[childById.Children.Count];
			for (int i = 0; i < childById.Children.Count; i++)
			{
				this.listEntryControllers[i] = (childById.Children[i] as XUiC_ListEntry<T>);
				if (this.listEntryControllers[i] != null)
				{
					this.listEntryControllers[i].OnScroll += this.HandleOnScroll;
					this.listEntryControllers[i].List = this;
				}
				else
				{
					Log.Warning("[XUi] List elements do not have the correct controller set (should be \"XUiC_ListEntry<" + typeof(T).FullName + ">\")");
				}
			}
			if (this.CursorControllable)
			{
				XUiV_Grid xuiV_Grid = childById.ViewComponent as XUiV_Grid;
				if (xuiV_Grid != null)
				{
					if (xuiV_Grid.Arrangement == UIGrid.Arrangement.Horizontal)
					{
						this.columns = xuiV_Grid.Columns;
						this.rows = this.PageLength / this.columns;
					}
					else
					{
						this.rows = xuiV_Grid.Rows;
						this.columns = this.PageLength / this.rows;
					}
				}
				XUiV_Table xuiV_Table = childById.ViewComponent as XUiV_Table;
				if (xuiV_Table != null)
				{
					this.columns = xuiV_Table.Columns;
					this.rows = this.PageLength / this.columns;
				}
			}
		}
		this.searchBox = (base.GetChildById("searchInput") as XUiC_TextInput);
		if (this.searchBox != null)
		{
			this.searchBox.OnChangeHandler += this.OnSearchInputChanged;
			this.searchBox.OnSubmitHandler += this.OnSearchInputSubmit;
		}
		this.RebuildList(true);
	}

	public virtual void RebuildList(bool _resetFilter = false)
	{
		this.SelectedEntry = null;
		if (this.filteredEntries != null)
		{
			this.filteredEntries.Clear();
		}
		this.RefreshView(_resetFilter, true);
	}

	public virtual void RefreshView(bool _resetFilter = false, bool _resetPage = true)
	{
		if (_resetFilter)
		{
			this.searchBox.Text = "";
		}
		XUiC_TextInput xuiC_TextInput = this.searchBox;
		this.OnSearchInputChanged(this, (xuiC_TextInput != null) ? xuiC_TextInput.Text : null, true);
		if (_resetPage)
		{
			this.Page = 0;
		}
	}

	public XUiC_ListEntry<T> IsVisible(T _value)
	{
		foreach (XUiC_ListEntry<T> xuiC_ListEntry in this.listEntryControllers)
		{
			T entry = xuiC_ListEntry.GetEntry();
			if (entry != null && entry == _value)
			{
				return xuiC_ListEntry;
			}
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void HandleOnScroll(XUiController _sender, float _delta)
	{
		if (_delta > 0f)
		{
			XUiC_Paging xuiC_Paging = this.pager;
			if (xuiC_Paging == null)
			{
				return;
			}
			xuiC_Paging.PageDown();
			return;
		}
		else
		{
			XUiC_Paging xuiC_Paging2 = this.pager;
			if (xuiC_Paging2 == null)
			{
				return;
			}
			xuiC_Paging2.PageUp();
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnSearchInputSubmit(XUiController _sender, string _text)
	{
		this.OnSearchInputChanged(_sender, _text, false);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnSearchInputChanged(XUiController _sender, string _text, bool _changeFromCode)
	{
		this.FilterResults(_text);
		this.IsDirty = true;
	}

	public IReadOnlyList<T> AllEntries()
	{
		return this.allEntries;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void FilterResults(string _textMatch)
	{
		if (_textMatch == null)
		{
			this.filteredEntries.Clear();
			this.filteredEntries.AddRange(this.allEntries);
			return;
		}
		if (_textMatch == this.previousMatch && this.filteredEntries.Count == this.allEntries.Count)
		{
			return;
		}
		this.previousMatch = _textMatch;
		this.filteredEntries.Clear();
		if (_textMatch.Length > 0)
		{
			using (List<T>.Enumerator enumerator = this.allEntries.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					T t = enumerator.Current;
					if (t.MatchesSearch(_textMatch))
					{
						this.filteredEntries.Add(t);
					}
				}
				return;
			}
		}
		this.filteredEntries.AddRange(this.allEntries);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnSelectionChanged(XUiC_ListEntry<T> _previousEntry, XUiC_ListEntry<T> _newEntry)
	{
		if (!this.ignore_selection_change && this.SelectionChanged != null)
		{
			this.SelectionChanged(_previousEntry, _newEntry);
			if (!this.SelectableEntries)
			{
				this.ignore_selection_change = true;
				this.SelectedEntry = null;
				this.ignore_selection_change = false;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void goToPageOfCurrentlySelectedEntry()
	{
		if (this.CurrentSelectedEntry != null)
		{
			T currentSelectedEntry = this.CurrentSelectedEntry;
			bool flag = false;
			for (int i = 0; i < this.filteredEntries.Count; i++)
			{
				if (this.filteredEntries[i] == this.CurrentSelectedEntry)
				{
					this.Page = i / this.PageLength;
					flag = true;
					break;
				}
			}
			this.CurrentSelectedEntry = (flag ? currentSelectedEntry : default(T));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateCurrentPageContents()
	{
		if (this.filteredEntries == null)
		{
			Log.Error("filteredEntries is null!");
			return;
		}
		for (int i = 0; i < this.PageLength; i++)
		{
			int num = i + this.PageLength * this.page;
			XUiC_ListEntry<T> xuiC_ListEntry = this.listEntryControllers[i];
			if (xuiC_ListEntry == null)
			{
				Log.Error("listEntry is null! {0} items in listEntryControllers", new object[]
				{
					this.listEntryControllers.Length
				});
				return;
			}
			if (num < this.filteredEntries.Count)
			{
				xuiC_ListEntry.SetEntry(this.filteredEntries[num]);
			}
			else
			{
				xuiC_ListEntry.SetEntry(default(T));
				if (xuiC_ListEntry.Selected)
				{
					xuiC_ListEntry.Selected = false;
				}
			}
			if (!this.updateSelectedItemByIndex && this.CurrentSelectedEntry != null && this.CurrentSelectedEntry == xuiC_ListEntry.GetEntry() && this.SelectedEntry != xuiC_ListEntry)
			{
				this.SelectedEntry = xuiC_ListEntry;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updatePageLabel()
	{
		if (this.pager != null)
		{
			this.pager.CurrentPageNumber = this.page;
			this.pager.LastPageNumber = this.LastPage;
		}
	}

	public override void Update(float _dt)
	{
		if (this.SelectableEntries && this.CursorControllable && this.columns > 0 && this.rows > 0 && PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard)
		{
			PlayerActionsGUI guiactions = this.windowGroup.playerUI.playerInput.GUIActions;
			if (guiactions.Left.WasPressed)
			{
				this.SelectedEntryIndex = Math.Max(0, this.SelectedEntryIndex - this.rows);
			}
			if (guiactions.Right.WasPressed)
			{
				this.SelectedEntryIndex = Math.Min(this.EntryCount - 1, this.SelectedEntryIndex + this.rows);
			}
			if (guiactions.Up.WasPressed)
			{
				this.SelectedEntryIndex = Math.Max(0, this.SelectedEntryIndex - 1);
			}
			if (guiactions.Down.WasPressed)
			{
				this.SelectedEntryIndex = Math.Min(this.EntryCount - 1, this.SelectedEntryIndex + 1);
			}
		}
		if (this.IsDirty)
		{
			this.goToPageOfCurrentlySelectedEntry();
			if (this.page > this.LastPage)
			{
				this.Page = this.LastPage;
			}
			this.updateCurrentPageContents();
			if (this.SelectedEntry != null && this.SelectedEntry.GetEntry() != this.CurrentSelectedEntry)
			{
				this.ClearSelection();
			}
			this.updatePageLabel();
			XUiEvent_PageContentsChangedEventHandler pageContentsChanged = this.PageContentsChanged;
			if (pageContentsChanged != null)
			{
				pageContentsChanged();
			}
			this.IsDirty = false;
		}
		base.Update(_dt);
	}

	public override bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
		if (base.ParseAttribute(_name, _value, _parent))
		{
			return true;
		}
		if (!(_name == "selectable"))
		{
			if (!(_name == "clear_selection_on_open"))
			{
				if (!(_name == "clear_searchtext_on_open"))
				{
					if (!(_name == "cursor_controllable"))
					{
						return false;
					}
					this.CursorControllable = StringParsers.ParseBool(_value, 0, -1, true);
				}
				else
				{
					this.ClearSearchTextOnOpenClose = StringParsers.ParseBool(_value, 0, -1, true);
				}
			}
			else
			{
				this.ClearSelectionOnOpenClose = StringParsers.ParseBool(_value, 0, -1, true);
			}
		}
		else
		{
			this.SelectableEntries = StringParsers.ParseBool(_value, 0, -1, true);
		}
		return true;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (this.ClearSelectionOnOpenClose)
		{
			this.ClearSelection();
		}
		if (this.ClearSearchTextOnOpenClose && this.searchBox != null)
		{
			this.searchBox.Text = string.Empty;
			this.OnSearchInputChanged(this, string.Empty, true);
		}
		this.IsDirty = true;
	}

	public override void OnClose()
	{
		base.OnClose();
		if (this.ClearSelectionOnOpenClose)
		{
			this.ClearSelection();
		}
		if (this.ClearSearchTextOnOpenClose && this.searchBox != null)
		{
			this.searchBox.Text = "";
		}
	}

	public void ClearSelection()
	{
		this.SelectedEntry = null;
	}

	public virtual void OnListEntryClicked(XUiC_ListEntry<T> _entry)
	{
		XUiEvent_ListEntryClickedEventHandler<T> listEntryClicked = this.ListEntryClicked;
		if (listEntryClicked == null)
		{
			return;
		}
		listEntryClicked(_entry);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_List()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public List<T> filteredEntries = new List<T>();

	[PublicizedFrom(EAccessModifier.Protected)]
	public T CurrentSelectedEntry;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_ListEntry<T>[] listEntryControllers;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_ListEntry<T> selectedEntry;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int selectedEntryIndex = -1;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool updateSelectedItemByIndex;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int page;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_TextInput searchBox;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_Paging pager;

	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly List<T> allEntries = new List<T>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int columns;

	[PublicizedFrom(EAccessModifier.Private)]
	public int rows;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string previousMatch = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool ignore_selection_change;
}

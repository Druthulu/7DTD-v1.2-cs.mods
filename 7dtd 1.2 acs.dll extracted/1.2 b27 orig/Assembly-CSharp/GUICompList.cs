using System;
using System.Collections.Generic;
using UnityEngine;

public class GUICompList : GUIComp
{
	public GUICompList(Rect _rect)
	{
		this.rect = _rect;
		this.boxStyle = "box";
		this.listContent = new List<GUIContent>();
	}

	public GUICompList(Rect _rect, string[] _listContent) : this(_rect)
	{
		foreach (string text in _listContent)
		{
			this.listContent.Add(new GUIContent(text));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void initStyle()
	{
		this.listStyle = new GUIStyle("button");
		this.listStyle.fontSize = 12;
		this.listStyle.normal.textColor = Color.white;
		this.listStyle.alignment = TextAnchor.MiddleLeft;
		this.listStyle.fixedHeight = (float)(this.listStyle.fontSize + 9);
		this.listStyle.fontStyle = FontStyle.Normal;
		this.listStyle.normal.background = null;
		this.listStyle.padding.left = 2;
		this.listStyle.padding.right = 2;
		this.listStyle.padding.top = 0;
		this.listStyle.padding.bottom = 0;
		this.listStyle.margin = new RectOffset(0, 0, 0, 0);
		this.listStyle.hover.textColor = Color.yellow;
		this.bInitStyleDone = true;
	}

	public void AddLine(string _line)
	{
		this.listContent.Add(new GUIContent(_line));
	}

	public void RemoveSelectedEntry()
	{
		if (this.listContent.Count > 0 && this.selectedItemIndex != -1)
		{
			this.listContent.RemoveAt(this.selectedItemIndex);
		}
	}

	public void MoveSelectedEntryUp()
	{
		if (this.listContent.Count > 0 && this.selectedItemIndex > 0 && this.selectedItemIndex < this.listContent.Count)
		{
			GUIContent item = this.listContent[this.selectedItemIndex];
			this.RemoveSelectedEntry();
			this.selectedItemIndex--;
			this.listContent.Insert(this.selectedItemIndex, item);
		}
	}

	public void MoveSelectedEntryDown()
	{
		if (this.listContent.Count > 0 && this.selectedItemIndex < this.listContent.Count - 1)
		{
			GUIContent item = this.listContent[this.selectedItemIndex];
			this.RemoveSelectedEntry();
			this.selectedItemIndex++;
			this.listContent.Insert(this.selectedItemIndex, item);
		}
	}

	public void Clear()
	{
		this.listContent.Clear();
	}

	public override void OnGUI()
	{
		if (!this.bInitStyleDone)
		{
			this.initStyle();
		}
		if (this.bScrollToSelection)
		{
			this.scroll = new Vector2(0f, this.listStyle.fixedHeight * (float)this.selectedItemIndex);
			this.bScrollToSelection = false;
		}
		Rect rect = new Rect(this.rect.x, this.rect.y, this.rect.width - 18f, this.listStyle.fixedHeight * (float)this.listContent.Count);
		GUI.Box(this.rect, "", this.boxStyle);
		this.scroll = GUI.BeginScrollView(this.rect, this.scroll, rect);
		this.selectedItemIndex = GUI.SelectionGrid(rect, this.selectedItemIndex, this.listContent.ToArray(), 1, this.listStyle);
		GUI.EndScrollView();
	}

	public override void OnGUILayout()
	{
		if (!this.bInitStyleDone)
		{
			this.initStyle();
		}
		if (this.bScrollToSelection)
		{
			this.scroll = new Vector2(0f, this.listStyle.fixedHeight * (float)this.selectedItemIndex);
			this.bScrollToSelection = false;
		}
		GUILayout.BeginVertical("box", new GUILayoutOption[]
		{
			GUILayout.Width(this.rect.width)
		});
		this.scroll = GUILayout.BeginScrollView(this.scroll, false, true, new GUILayoutOption[]
		{
			GUILayout.Width(this.rect.width),
			GUILayout.Height(this.rect.height)
		});
		this.lastSelectedItemIndex = this.selectedItemIndex;
		this.selectedItemIndex = GUILayout.SelectionGrid(this.lastSelectedItemIndex, this.listContent.ToArray(), 1, this.listStyle, new GUILayoutOption[]
		{
			GUILayout.Width(this.rect.width - 18f)
		});
		GUILayout.EndScrollView();
		GUILayout.EndVertical();
	}

	public bool OnListClicked()
	{
		return this.lastSelectedItemIndex != this.selectedItemIndex;
	}

	public int SelectedItemIndex
	{
		get
		{
			return this.selectedItemIndex;
		}
		set
		{
			this.selectedItemIndex = value;
			this.lastSelectedItemIndex = value;
			this.bScrollToSelection = true;
		}
	}

	public string SelectedEntry
	{
		get
		{
			if (this.selectedItemIndex < 0 || this.selectedItemIndex >= this.listContent.Count)
			{
				return null;
			}
			return this.listContent[this.selectedItemIndex].text;
		}
	}

	public bool SelectEntry(string _entry)
	{
		for (int i = 0; i < this.listContent.Count; i++)
		{
			if (this.listContent[i].text.Equals(_entry))
			{
				this.SelectedItemIndex = i;
				return true;
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int selectedItemIndex = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public int lastSelectedItemIndex = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<GUIContent> listContent;

	[PublicizedFrom(EAccessModifier.Private)]
	public string boxStyle;

	[PublicizedFrom(EAccessModifier.Private)]
	public GUIStyle listStyle;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 scroll;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bScrollToSelection;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bInitStyleDone;
}

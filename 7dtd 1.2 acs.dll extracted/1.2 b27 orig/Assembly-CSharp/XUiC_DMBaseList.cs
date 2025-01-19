using System;

public abstract class XUiC_DMBaseList<T> : XUiC_List<T> where T : XUiListEntry
{
	public event XUiEvent_OnPressEventHandler OnEntryClicked;

	public event XUiEvent_OnPressEventHandler OnEntryDoubleClicked;

	public event XUiEvent_OnHoverEventHandler OnChildElementHovered;

	public override void Init()
	{
		base.Init();
		foreach (XUiC_ListEntry<T> xuiC_ListEntry in this.listEntryControllers)
		{
			xuiC_ListEntry.OnPress += this.EntryClicked;
			xuiC_ListEntry.OnDoubleClick += this.EntryDoubleClicked;
			xuiC_ListEntry.OnHover += this.ChildElementHovered;
		}
		foreach (XUiController xuiController in this.pager.Children)
		{
			xuiController.OnHover += this.ChildElementHovered;
		}
		base.PageContentsChanged += this.PageContentsChangedHandler;
		this.searchBox.OnHover += this.ChildElementHovered;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void PageContentsChangedHandler()
	{
		if (this.hoveredElement != null && this.OnChildElementHovered != null)
		{
			this.OnChildElementHovered(this.hoveredElement, false);
			this.OnChildElementHovered(this.hoveredElement, true);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void EntryClicked(XUiController _sender, int _mouseButton)
	{
		XUiEvent_OnPressEventHandler onEntryClicked = this.OnEntryClicked;
		if (onEntryClicked == null)
		{
			return;
		}
		onEntryClicked(_sender, _mouseButton);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void EntryDoubleClicked(XUiController _sender, int _mouseButton)
	{
		XUiEvent_OnPressEventHandler onEntryDoubleClicked = this.OnEntryDoubleClicked;
		if (onEntryDoubleClicked == null)
		{
			return;
		}
		onEntryDoubleClicked(_sender, _mouseButton);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void ChildElementHovered(XUiController _sender, bool _isOver)
	{
		this.hoveredElement = (_isOver ? _sender : null);
		XUiEvent_OnHoverEventHandler onChildElementHovered = this.OnChildElementHovered;
		if (onChildElementHovered == null)
		{
			return;
		}
		onChildElementHovered(_sender, _isOver);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_DMBaseList()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController hoveredElement;
}

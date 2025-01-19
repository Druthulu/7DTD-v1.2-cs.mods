using System;
using System.Collections.Generic;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_Paging : XUiController
{
	public int CurrentPageNumber
	{
		get
		{
			return this.currentPageNumber;
		}
		set
		{
			value = Mathf.Clamp(value, 0, this.LastPageNumber);
			if (value != this.currentPageNumber)
			{
				this.currentPageNumber = value;
				base.RefreshBindings(false);
			}
		}
	}

	public int LastPageNumber
	{
		get
		{
			return this.lastPageNumber;
		}
		set
		{
			if (value != this.lastPageNumber)
			{
				this.lastPageNumber = value;
				base.RefreshBindings(false);
				if (this.currentPageNumber > this.lastPageNumber)
				{
					this.CurrentPageNumber = this.lastPageNumber;
					XUiEvent_PageChangedEventHandler onPageChanged = this.OnPageChanged;
					if (onPageChanged == null)
					{
						return;
					}
					onPageChanged();
				}
			}
		}
	}

	public event XUiEvent_PageChangedEventHandler OnPageChanged;

	public override void Init()
	{
		base.Init();
		this.btnPageDown = base.GetChildById("pageDown");
		if (this.btnPageDown != null)
		{
			this.btnPageDown.OnPress += delegate(XUiController _sender, int _i)
			{
				this.PageDown();
			};
		}
		this.btnPageUp = base.GetChildById("pageUp");
		if (this.btnPageUp != null)
		{
			this.btnPageUp.OnPress += delegate(XUiController _sender, int _i)
			{
				this.PageUp();
			};
		}
		if (!string.IsNullOrEmpty(this.contentParentName))
		{
			this.contentsParent = base.WindowGroup.Controller.GetChildById(this.contentParentName);
		}
		this.handlePageDownAction = new Action(this.PageDown);
		this.handlePageUpAction = new Action(this.PageUp);
		this.currentPageNumber = 0;
		base.RefreshBindings(false);
	}

	public void PageUp()
	{
		if (this.currentPageNumber < this.LastPageNumber)
		{
			this.currentPageNumber++;
			XUiEvent_PageChangedEventHandler onPageChanged = this.OnPageChanged;
			if (onPageChanged != null)
			{
				onPageChanged();
			}
			base.RefreshBindings(false);
			if (this.currentPageNumber == this.lastPageNumber && base.xui.playerUI.CursorController.navigationTarget == this.btnPageUp.ViewComponent)
			{
				this.btnPageDown.SelectCursorElement(false, false);
			}
		}
	}

	public void PageDown()
	{
		if (this.currentPageNumber > 0)
		{
			this.currentPageNumber--;
			XUiEvent_PageChangedEventHandler onPageChanged = this.OnPageChanged;
			if (onPageChanged != null)
			{
				onPageChanged();
			}
			base.RefreshBindings(false);
			if (this.currentPageNumber == 0 && base.xui.playerUI.CursorController.navigationTarget == this.btnPageDown.ViewComponent)
			{
				this.btnPageUp.SelectCursorElement(false, false);
			}
		}
	}

	public int GetPage()
	{
		return this.CurrentPageNumber;
	}

	public void SetPage(int _page)
	{
		this.CurrentPageNumber = _page;
	}

	public int GetLastPage()
	{
		return this.LastPageNumber;
	}

	public void SetLastPageByElementsAndPageLength(int _elementCount, int _pageLength)
	{
		this.LastPageNumber = Math.Max(0, Mathf.CeilToInt((float)_elementCount / (float)_pageLength) - 1);
	}

	public override bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
		if (_name == "show_max_page")
		{
			this.showMaxPage = StringParsers.ParseBool(_value, 0, -1, true);
			return true;
		}
		if (_name == "separator")
		{
			this.separator = _value;
			return true;
		}
		if (_name == "primary_pager")
		{
			this.primaryPager = StringParsers.ParseBool(_value, 0, -1, true);
			return true;
		}
		if (!(_name == "contents_parent"))
		{
			return base.ParseAttribute(_name, _value, _parent);
		}
		this.contentParentName = _value;
		return true;
	}

	public override bool GetBindingValue(ref string value, string bindingName)
	{
		if (bindingName == "pagenumber")
		{
			value = this.pagenumberFormatter.Format(this.currentPageNumber + 1);
			return true;
		}
		if (bindingName == "maxpagenumber")
		{
			value = this.maxpagenumberFormatter.Format(this.LastPageNumber + 1);
			return true;
		}
		if (bindingName == "showmaxpage")
		{
			value = this.showMaxPage.ToString();
			return true;
		}
		if (!(bindingName == "separator"))
		{
			return base.GetBindingValue(ref value, bindingName);
		}
		value = this.separator;
		return true;
	}

	public void Reset()
	{
		this.currentPageNumber = 0;
		base.RefreshBindings(false);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		XUiC_Paging.activePagers.Add(this);
	}

	public override void OnClose()
	{
		base.OnClose();
		XUiC_Paging.activePagers.Remove(this);
		if (XUiC_Paging.activePagers.Count == 0)
		{
			base.xui.calloutWindow.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuPaging);
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard)
		{
			return;
		}
		if (XUiC_Paging.activePagers.Count == 0)
		{
			return;
		}
		if (XUiC_Paging.activePagers[0] == this && !LocalPlayerUI.IsAnyComboBoxFocused)
		{
			bool flag = false;
			foreach (XUiC_Paging xuiC_Paging in XUiC_Paging.activePagers)
			{
				if (XUiC_Paging.activePagers.Count == 1 || xuiC_Paging.contentsParent == null || (base.xui.playerUI.CursorController.CurrentTarget != null && base.xui.playerUI.CursorController.CurrentTarget.Controller.IsChildOf(xuiC_Paging.contentsParent)))
				{
					XUi.HandlePaging(base.xui, xuiC_Paging.handlePageUpAction, xuiC_Paging.handlePageDownAction, false);
					base.xui.calloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuPaging, 0f);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				base.xui.calloutWindow.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuPaging);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int currentPageNumber;

	public bool showMaxPage;

	public string separator = "/";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool primaryPager = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public int lastPageNumber;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action handlePageDownAction;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action handlePageUpAction;

	[PublicizedFrom(EAccessModifier.Private)]
	public string contentParentName;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController contentsParent;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController btnPageUp;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController btnPageDown;

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<XUiC_Paging> activePagers = new List<XUiC_Paging>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt pagenumberFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt maxpagenumberFormatter = new CachedStringFormatterInt();
}

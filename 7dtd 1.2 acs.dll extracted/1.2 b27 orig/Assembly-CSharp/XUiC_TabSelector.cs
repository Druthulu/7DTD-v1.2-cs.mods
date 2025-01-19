using System;
using System.Collections.Generic;
using GUI_2;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_TabSelector : XUiController
{
	public event Action<int, string> OnTabChanged;

	public bool Enabled
	{
		get
		{
			return this.enabled;
		}
		set
		{
			if (this.enabled != value)
			{
				this.enabled = value;
				this.updateTabVisibility();
				base.RefreshBindings(true);
			}
		}
	}

	public int SelectedTabIndex
	{
		get
		{
			return this.selectedTabIndex;
		}
		set
		{
			if (!this.enabled)
			{
				return;
			}
			if (this.selectedTabIndex != value)
			{
				if (value < 0)
				{
					value = 0;
				}
				else if (value >= this.tabCount)
				{
					value = this.tabCount - 1;
				}
				this.selectedTabIndex = value;
				this.updateTabVisibility();
				if (this.OnTabChanged != null)
				{
					this.OnTabChanged(this.selectedTabIndex, this.tabButtons[this.selectedTabIndex].Text);
				}
				if (base.xui.playerUI.CursorController.navigationTarget != null && base.xui.playerUI.CursorController.navigationTarget.Controller.IsChildOf(this))
				{
					base.xui.playerUI.CursorController.SetNavigationTarget(null);
				}
				if (this.selectTabContentsOnChange)
				{
					this.pendingOnChangeSelection = true;
				}
			}
		}
	}

	public int TabCount
	{
		get
		{
			return this.tabCount;
		}
		set
		{
			if (this.tabCount != value)
			{
				if (value >= this.tabs.Count)
				{
					value = this.tabs.Count;
				}
				if (value >= this.tabButtons.Count)
				{
					value = this.tabButtons.Count;
				}
				this.tabCount = value;
				if (this.tabHeaderBackground != null)
				{
					this.tabHeaderBackground.Sprite.leftAnchor.target = this.tabButtons[value - 1].ViewComponent.UiTransform.Find("border");
					this.tabHeaderBackground.Sprite.leftAnchor.relative = 1f;
				}
			}
		}
	}

	public override void Init()
	{
		base.Init();
		foreach (XUiC_SimpleButton xuiC_SimpleButton in base.GetChildById("tabsHeader").GetChildrenByType<XUiC_SimpleButton>(null))
		{
			this.tabButtons.Add(xuiC_SimpleButton);
			xuiC_SimpleButton.OnPressed += this.HandleOnPress;
			xuiC_SimpleButton.Button.Type = UIBasicSprite.Type.Advanced;
			xuiC_SimpleButton.Button.Sprite.bottomType = UIBasicSprite.AdvancedType.Invisible;
			xuiC_SimpleButton.Button.Sprite.rightType = UIBasicSprite.AdvancedType.Invisible;
			xuiC_SimpleButton.Button.Sprite.leftType = UIBasicSprite.AdvancedType.Invisible;
			xuiC_SimpleButton.Button.IsSnappable = (xuiC_SimpleButton.Button.IsNavigatable = false);
		}
		this.content = base.GetChildById("tabsContents");
		foreach (XUiController xuiController in this.content.Children)
		{
			XUiV_Rect xuiV_Rect = xuiController.ViewComponent as XUiV_Rect;
			if (xuiV_Rect != null)
			{
				this.tabs.Add(xuiV_Rect);
			}
		}
		for (int j = 0; j < this.tabs.Count; j++)
		{
			if (j >= this.tabButtons.Count)
			{
				Log.Warning(string.Concat(new string[]
				{
					"More tabs (",
					this.tabs.Count.ToString(),
					") than tab buttons (",
					this.tabButtons.Count.ToString(),
					") in ",
					base.WindowGroup.ID
				}));
				break;
			}
			if (this.tabs[j].Controller.CustomAttributes.ContainsKey("tab_key"))
			{
				this.tabButtons[j].Text = Localization.Get(this.tabs[j].Controller.CustomAttributes["tab_key"], false);
			}
			this.tabButtons[j].Tooltip = this.tabs[j].ToolTip;
		}
		XUiController childById = base.GetChildById("backgroundMainTabs");
		if (childById != null)
		{
			this.tabHeaderBackground = (childById.ViewComponent as XUiV_Sprite);
			XUiController childById2 = base.GetChildById("border");
			if (childById2 != null)
			{
				Transform uiTransform = childById2.ViewComponent.UiTransform;
				this.tabHeaderBackground.Sprite.rightAnchor.target = uiTransform;
				this.tabHeaderBackground.Sprite.rightAnchor.relative = 1f;
			}
		}
		this.TabCount = this.tabs.Count;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		for (int i = this.tabCount; i < this.tabButtons.Count; i++)
		{
			this.tabButtons[i].Parent.ViewComponent.IsVisible = false;
		}
		this.updateTabVisibility();
		this.pendingOnOpenSelection = !this.selectOnOpen;
		this.pendingOnChangeSelection = false;
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.LeftBumper, "igcoTabLeft", XUiC_GamepadCalloutWindow.CalloutType.Tabs);
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.RightBumper, "igcoTabRight", XUiC_GamepadCalloutWindow.CalloutType.Tabs);
		base.xui.calloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.Tabs, 0f);
	}

	public override void OnClose()
	{
		base.OnClose();
		base.xui.calloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.Tabs);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleOnPress(XUiController _sender, int _mouseButton)
	{
		if (this.enabled)
		{
			this.SelectedTabIndex = this.tabButtons.IndexOf(_sender as XUiC_SimpleButton);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateTabVisibility()
	{
		int num = 0;
		while (num < this.tabs.Count && num < this.tabButtons.Count)
		{
			this.tabButtons[num].Button.Selected = (num == this.selectedTabIndex);
			this.tabButtons[num].Button.IsVisible = this.enabled;
			this.tabButtons[num].Label.IsVisible = this.enabled;
			this.tabs[num].IsVisible = (this.enabled && num == this.selectedTabIndex);
			num++;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void ToggleCategory(int _dir)
	{
		int num = NGUIMath.RepeatIndex(this.selectedTabIndex + _dir, this.tabCount);
		this.SelectedTabIndex = num;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		bool flag = this.tabInputAllowed;
		if (base.xui.playerUI.CursorController.lockNavigationToView != null)
		{
			this.tabInputAllowed = base.IsChildOf(base.xui.playerUI.CursorController.lockNavigationToView.Controller);
		}
		else
		{
			this.tabInputAllowed = true;
		}
		if (this.tabInputAllowed != flag)
		{
			if (this.tabInputAllowed)
			{
				base.xui.calloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.Tabs, 0f);
			}
			else
			{
				base.xui.calloutWindow.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.Tabs);
			}
		}
		LocalPlayerUI playerUI = base.xui.playerUI;
		PlayerActionsGUI guiactions = playerUI.playerInput.GUIActions;
		GUIWindowManager windowManager = playerUI.windowManager;
		if (this.tabInputAllowed && windowManager.IsKeyShortcutsAllowed() && this.isActiveTabSelector)
		{
			if (guiactions.WindowPagingLeft.WasReleased && windowManager.IsWindowOpen(this.windowGroup.ID))
			{
				this.ToggleCategory(-1);
			}
			if (guiactions.WindowPagingRight.WasReleased && windowManager.IsWindowOpen(this.windowGroup.ID))
			{
				this.ToggleCategory(1);
			}
		}
		if (!this.pendingOnOpenSelection)
		{
			this.pendingOnOpenSelection = this.tabs[this.selectedTabIndex].Controller.SelectCursorElement(true, false);
		}
		if (this.pendingOnChangeSelection && this.isActiveTabSelector)
		{
			this.tabs[this.selectedTabIndex].Controller.SelectCursorElement(true, false);
			this.pendingOnChangeSelection = false;
		}
	}

	public string GetTabCaption(int _index)
	{
		if (_index >= this.tabCount)
		{
			throw new ArgumentOutOfRangeException("_index");
		}
		return this.tabButtons[_index].Text;
	}

	public void SetTabCaption(int _index, string _name)
	{
		if (_index >= this.tabCount)
		{
			throw new ArgumentOutOfRangeException("_index");
		}
		this.tabButtons[_index].Text = _name;
	}

	public string GetTabTooltip(int _index)
	{
		if (_index >= this.tabCount)
		{
			throw new ArgumentOutOfRangeException("_index");
		}
		return this.tabButtons[_index].Tooltip;
	}

	public void SetTabTooltip(int _index, string _name)
	{
		if (_index >= this.tabCount)
		{
			throw new ArgumentOutOfRangeException("_index");
		}
		this.tabButtons[_index].Tooltip = _name;
	}

	public XUiV_Rect GetTabRect(int _index)
	{
		if (_index >= this.tabCount)
		{
			throw new ArgumentOutOfRangeException("_index");
		}
		return this.tabs[_index];
	}

	public XUiC_SimpleButton GetTabButton(int _index)
	{
		if (_index >= this.tabCount)
		{
			throw new ArgumentOutOfRangeException("_index");
		}
		return this.tabButtons[_index];
	}

	public bool IsSelected(string _tabKey)
	{
		string text;
		return this.SelectedTabIndex >= 0 && this.tabs.Count > 0 && this.tabs[this.SelectedTabIndex].Controller.CustomAttributes.TryGetValue("tab_key", out text) && text.Equals(_tabKey);
	}

	public override bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
		if (_name == "select_tab_contents_on_open")
		{
			if (!string.IsNullOrEmpty(_value))
			{
				this.selectOnOpen = StringParsers.ParseBool(_value, 0, -1, true);
			}
			return true;
		}
		if (!(_name == "select_tab_contents_on_change"))
		{
			return base.ParseAttribute(_name, _value, _parent);
		}
		if (!string.IsNullOrEmpty(_value))
		{
			this.selectTabContentsOnChange = StringParsers.ParseBool(_value, 0, -1, true);
		}
		return true;
	}

	public override bool GetBindingValue(ref string _value, string _bindingName)
	{
		if (_bindingName == "tabsenabled")
		{
			_value = this.enabled.ToString();
			return true;
		}
		return base.GetBindingValue(ref _value, _bindingName);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<XUiC_SimpleButton> tabButtons = new List<XUiC_SimpleButton>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<XUiV_Rect> tabs = new List<XUiV_Rect>();

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite tabHeaderBackground;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController content;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool enabled = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public int selectedTabIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public int tabCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool selectOnOpen;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool pendingOnOpenSelection;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool tabInputAllowed = true;

	public bool selectTabContentsOnChange = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool pendingOnChangeSelection;

	public bool isActiveTabSelector = true;
}
